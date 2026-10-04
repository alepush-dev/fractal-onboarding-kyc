using Microsoft.AspNetCore.Hosting;
using MySqlConnector;
using onboardingKycApi.Models;
using System.Buffers.Text;
using System.Text.Json;


namespace onboardingKycApi.Service
{
    public class KycService : IKycService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        //Inyección nueva para obtener la ruta física de la aplicación y guardar la imagen
        public KycService(IWebHostEnvironment environment, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        //Método de procesamiento adaptado para retornar KycResponseModel
        public async Task<KycResponseModel> ProcessKycAsync(KycRequest request)
        {
            if (request.ImageFile == null || request.ImageFile.Length == 0)
            {
                throw new ArgumentException("No se ha adjuntado ninguna imagen.");
            }
     

            //  Guardar la imagen físicamente en el servidor para obtener la ImageUrl      
            string webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            string uploadsFolder = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string safeFileName = request.ImageFile.FileName.Replace(" ", "_");
            string filePath = Path.Combine(uploadsFolder, safeFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.ImageFile.CopyToAsync(fileStream);
            }

            svar httpRequest = _httpContextAccessor.HttpContext?.Request;
            string baseUrl = $"{httpRequest?.Scheme}://{httpRequest?.Host}";

            string dbImageUrl = $"{baseUrl}/uploads/{safeFileName}";

            string imageText64;
            using (var memoryStream = new MemoryStream())
            {
                await request.ImageFile.CopyToAsync(memoryStream);
                byte[] arregloDeBytes = memoryStream.ToArray();
                imageText64 = Convert.ToBase64String(arregloDeBytes);
            }

            // Llamar a la API de Gemini Flash
            var apiKey = _configuration["GeminiSettings:ApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={apiKey}";

            using var httpClient = new HttpClient();

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = "Analiza este documento de identidad y devuélveme un JSON puro con: FullName, DocumentNumber y Confidence." },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = request.ImageFile.ContentType,
                                    data = imageText64
                                }
                            }
                        }
                    }
                }
            };

            var jsonPaload = System.Text.Json.JsonSerializer.Serialize(requestBody);
            int maxIntentos = 5;
            int intentoActual = 0;
            HttpResponseMessage response = null;

            while (intentoActual < maxIntentos)
            {
                var content = new StringContent(jsonPaload, System.Text.Encoding.UTF8, "application/json");
                response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    break;
                }

                int statusCode = (int)response.StatusCode;
                if (statusCode != 503 && statusCode != 429)
                {
                    break;
                }

                intentoActual++;
                if (intentoActual < maxIntentos)
                {
                    int tiempoEspera = (statusCode == 429) ? 12000 : (2000 * intentoActual);
                    await Task.Delay(tiempoEspera);
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al comunicarse con la API de Gemini tras varios reintentos: {response.ReasonPhrase}. Detalles: {errorDetails}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Interpretar la respuesta de Gemini
            using var doc = System.Text.Json.JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;
            var textResponse = root.GetProperty("candidates")[0]
                                   .GetProperty("content")
                                   .GetProperty("parts")[0]
                                   .GetProperty("text");
            string data_str = textResponse.GetString();

            string cleanJson = data_str.Replace("```json", "").Replace("```", "").Trim();

            using JsonDocument innerDoc = System.Text.Json.JsonDocument.Parse(cleanJson);
            var innerRoot = innerDoc.RootElement;


            //Extraer el nombre completo del documento detectado
            string extractedFullName = "No encontrado";
            if (innerRoot.TryGetProperty("fullName", out var fnElem) && fnElem.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                extractedFullName = fnElem.GetString() ?? "No encontrado";
            }
            else if (innerRoot.TryGetProperty("FullName", out fnElem) && fnElem.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                extractedFullName = fnElem.GetString() ?? "No encontrado";
            }

            //Extraer el número del DNI
            string extractedDocNumber = "No encontrado";
            if (innerRoot.TryGetProperty("documentNumber", out var dnElem) && dnElem.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                extractedDocNumber = dnElem.GetString() ?? "No encontrado";
            }
            else if (innerRoot.TryGetProperty("DocumentNumber", out dnElem) && dnElem.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                extractedDocNumber = dnElem.GetString() ?? "No encontrado";
            }

            //Extraer el porcentaje de confianza del reconocimiento OCR
            decimal ocrConfidence = 0.0m;
            if (innerRoot.TryGetProperty("confidence", out var confElem) && confElem.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                ocrConfidence = confElem.GetDecimal();
            }
            else if (innerRoot.TryGetProperty("Confidence", out confElem) && confElem.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                ocrConfidence = confElem.GetDecimal();
            }


            //Retornar el ResponseModel completo para que pinte la tarjeta de la derecha
            return new KycResponseModel
            {
                Id = 1,
                Email = request.Email,
                FullName = extractedFullName,
                DocumentNumber = extractedDocNumber,
                Confidence = ocrConfidence,
                ImageUrl = dbImageUrl,
                CreatedAt = DateTime.Now
            };
        }


        //Método para listar los registros históricos en la tabla de React
        public async Task<IEnumerable<KycResponseModel>> ObtenerRegistrosAsync()
        {
            var listaRegistros = new List<KycResponseModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, Email, FullName, DocumentNumber, OcrConfidence, ImageUrl, CreatedAt FROM clientskyc ORDER BY Id ASC";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            listaRegistros.Add(new KycResponseModel
                            {
                                Id = reader.GetInt32("Id"),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString("Email"),
                                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? string.Empty : reader.GetString("FullName"),
                                DocumentNumber = reader.IsDBNull(reader.GetOrdinal("DocumentNumber")) ? string.Empty : reader.GetString("DocumentNumber"),
                                Confidence = reader.IsDBNull(reader.GetOrdinal("OcrConfidence")) ? 0 : reader.GetDecimal("OcrConfidence"),
                                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? string.Empty : reader.GetString("ImageUrl"),
                                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.Now : reader.GetDateTime("CreatedAt")
                            });
                        }
                    }
                }
            }

            return listaRegistros;
        }
    }
}