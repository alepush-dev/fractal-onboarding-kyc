using onboardingKycApi.Models;
using onboardingKycApi.Repository;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace onboardingKycApi.Service
{
    // Lógica de negocio: valida, llama a la IA, guarda la imagen y registra en la BD
    public class KycService : IKycService
    {
        // Reglas de validación del archivo
        private static readonly string[] TiposPermitidos = { "image/jpeg", "image/png", "image/webp" };
        private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        // Reintentos ante Gemini saturado (503) o límite de peticiones (429)
        private const int MaxIntentos = 5;

        // Por debajo de esta confianza consideramos que la imagen no es legible (borrosa, oscura, cortada)
        private const decimal ConfianzaMinima = 0.6m;
        private const string NoEncontrado = "No encontrado";

        private readonly IKycRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KycService> _logger;

        public KycService(
            IKycRepository repository,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<KycService> logger)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<KycResponseModel> ProcessKycAsync(KycRequest request)
        {
            // 1. Validar los datos de entrada
            ValidarSolicitud(request);

            // 2. Leer la imagen una sola vez
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await request.ImageFile.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // 3. Extraer los datos con la IA
            KycResponseResult datos = await ExtraerDatosConGeminiAsync(fileBytes, request.ImageFile.ContentType);

            // 4. Verificar que la imagen se haya podido leer ( imagen borrosa)
            ValidarLegilibilidad(datos);

            // 4. Guardar la imagen solo si la IA respondió bien (así no quedan archivos huérfanos)
            string imageUrl = await GuardarImagenAsync(fileBytes, request.ImageFile.FileName);

            // 5. Registrar en la base de datos y obtener el Id real
            int nuevoId = await _repository.InsertarRegistroAsync(request.Email, datos, imageUrl);

            return new KycResponseModel
            {
                Id = nuevoId,
                Email = request.Email,
                FullName = datos.FullName,
                DocumentNumber = datos.DocumentNumber,
                Confidence = datos.Confidence,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now
            };
        }

        public Task<IEnumerable<KycResponseModel>> ObtenerRegistrosAsync()
        {
            return _repository.ObtenerRegistrosAsync();
        }

        // ---------- Métodos privados ----------

        private static void ValidarSolicitud(KycRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !MailAddress.TryCreate(request.Email, out _))
                throw new ArgumentException("El correo electrónico no es válido.");

            if (request.ImageFile == null || request.ImageFile.Length == 0)
                throw new ArgumentException("No se ha adjuntado ninguna imagen.");

            if (!TiposPermitidos.Contains(request.ImageFile.ContentType?.ToLowerInvariant()))
                throw new ArgumentException("Formato no soportado. Solo se aceptan imágenes JPG, PNG o WEBP.");

            if (request.ImageFile.Length > TamanoMaximoBytes)
                throw new ArgumentException("La imagen supera el tamaño máximo de 5 MB.");
        }

        private static void ValidarLegilibilidad(KycResponseResult datos)
        {
            bool faltanDatos = datos.FullName == NoEncontrado || datos.DocumentNumber == NoEncontrado;
            if (faltanDatos || datos.Confidence < ConfianzaMinima)
                throw new ImagenIlegibleException(
                    "No pudimos leer el documento. La imagen puede estar borrosa, oscura o cortada. Intenta con una foto más nítida.");
        }

        private async Task<KycResponseResult> ExtraerDatosConGeminiAsync(byte[] fileBytes, string mimeType)
        {
            var apiKey = _configuration["GeminiSettings:ApiKey"];
            const string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                text = "Analiza este documento de identidad. Devuelve solo un JSON con estas claves: " +
                                       "FullName (texto), DocumentNumber (texto) y Confidence (número entre 0 y 1). " +
                                       "Si no encuentras un dato, usa null."
                            },
                            new { inline_data = new { mime_type = mimeType, data = Convert.ToBase64String(fileBytes) } }
                        }
                    }
                },
                // Le pedimos a Gemini que responda directamente en JSON
                generationConfig = new { responseMimeType = "application/json" }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);

            // HttpClient creado por la fábrica: reutiliza conexiones
            var httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage? response = null;

            for (int intento = 1; intento <= MaxIntentos; intento++)
            {
                using var mensaje = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };
                // La API key va en un header, no en la URL, para que no quede en logs
                mensaje.Headers.Add("x-goog-api-key", apiKey);

                response = await httpClient.SendAsync(mensaje);

                if (response.IsSuccessStatusCode)
                    break;

                int statusCode = (int)response.StatusCode;
                bool esReintentable = statusCode == 429 || statusCode == 503;
                if (!esReintentable || intento == MaxIntentos)
                    break;

                // Backoff: esperamos más en cada intento
                int espera = statusCode == 429 ? 12000 : 2000 * intento;
                _logger.LogWarning("Gemini respondió {Status}. Reintento {Intento} en {Espera} ms.", statusCode, intento, espera);
                await Task.Delay(espera);
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                var detalle = response != null ? await response.Content.ReadAsStringAsync() : "Sin respuesta";
                _logger.LogError("Error de Gemini: {Status}. {Detalle}", response?.StatusCode, detalle);
                throw new HttpRequestException("No se pudo procesar el documento con el servicio de IA.");
            }

            // Respuesta de Gemini: candidates[0].content.parts[0].text contiene nuestro JSON
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            string texto = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            // Por seguridad limpiamos los ```json que a veces agrega el modelo
            string cleanJson = texto.Replace("```json", "").Replace("```", "").Trim();

            using var innerDoc = JsonDocument.Parse(cleanJson);
            var datos = innerDoc.RootElement;

            return new KycResponseResult
            {
                FullName = LeerTexto(datos, "FullName"),
                DocumentNumber = LeerTexto(datos, "DocumentNumber"),
                Confidence = LeerConfianza(datos)
            };
        }

        private async Task<string> GuardarImagenAsync(byte[] fileBytes, string nombreOriginal)
        {
            string webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadsFolder = Path.Combine(webRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder); // no hace nada si ya existe

            // Nombre único con Guid: evita que un archivo sobrescriba a otro
            string extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
            string nombreArchivo = $"{Guid.NewGuid()}{extension}";

            await File.WriteAllBytesAsync(Path.Combine(uploadsFolder, nombreArchivo), fileBytes);

            var host = _httpContextAccessor.HttpContext?.Request.Host;
            return $"https://{host}/uploads/{nombreArchivo}";
        }

        // Lee un campo de texto aunque venga como "FullName" o "fullName", o como número (ej. el DNI)
        private static string LeerTexto(JsonElement datos, string campo)
        {
            if (!BuscarPropiedad(datos, campo, out var valor))
                return "No encontrado";

            return valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString() ?? "No encontrado",
                JsonValueKind.Number => valor.GetRawText(),
                _ => "No encontrado"
            };
        }

        // Lee la confianza y la deja siempre entre 0 y 1
        private static decimal LeerConfianza(JsonElement datos)
        {
            if (!BuscarPropiedad(datos, "Confidence", out var valor))
                return 0m;

            decimal confianza = 0m;
            if (valor.ValueKind == JsonValueKind.Number)
                confianza = valor.GetDecimal();
            else if (valor.ValueKind == JsonValueKind.String)
                decimal.TryParse(valor.GetString()?.Replace("%", ""), System.Globalization.NumberStyles.Number,
                                 System.Globalization.CultureInfo.InvariantCulture, out confianza);

            if (confianza > 1) confianza /= 100; // si la IA respondió 95 en vez de 0.95
            return Math.Clamp(confianza, 0m, 1m);
        }

        // Busca la propiedad sin importar mayúsculas/minúsculas
        private static bool BuscarPropiedad(JsonElement datos, string campo, out JsonElement valor)
        {
            foreach (var propiedad in datos.EnumerateObject())
            {
                if (string.Equals(propiedad.Name, campo, StringComparison.OrdinalIgnoreCase))
                {
                    valor = propiedad.Value;
                    return true;
                }
            }
            valor = default;
            return false;
        }
    }
}
