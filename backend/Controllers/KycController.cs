using Microsoft.AspNetCore.Mvc;
using onboardingKycApi.Models;
using onboardingKycApi.Service;
using System.Text.Json;

namespace onboardingKycApi.Controllers
{
    // El Controller solo recibe la petición HTTP, delega al Service y traduce el resultado a un código HTTP
    [Route("api/[controller]")]
    [ApiController]
    public class KycController : ControllerBase
    {
        private readonly IKycService _kycService;
        private readonly ILogger<KycController> _logger;

        public KycController(IKycService kycService, ILogger<KycController> logger)
        {
            _kycService = kycService;
            _logger = logger;
        }

        // POST api/Kyc/process: recibe correo + foto, la procesa con IA y la registra
        [HttpPost("process")]
        public async Task<IActionResult> ProcessKyc([FromForm] KycRequest request)
        {
            try
            {
                var resultado = await _kycService.ProcessKycAsync(request);
                return Ok(resultado); // 200
            }
            catch (ArgumentException ex)
            {
                // Error del usuario (datos inválidos)
                return BadRequest(new { message = ex.Message }); // 400
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                // Falló el servicio externo de IA o devolvió algo que no se pudo leer
                _logger.LogError(ex, "Error al procesar el documento con la IA.");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "El servicio de IA no pudo procesar el documento. Intenta nuevamente." });
            }
            catch (Exception ex)
            {
                // Error inesperado: se registra el detalle, pero al usuario no se le muestra
                _logger.LogError(ex, "Error inesperado en ProcessKyc.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ocurrió un error interno. Intenta nuevamente." });
            }
        }

        // GET api/Kyc/records: historial de registros para la tabla de clientes
        [HttpGet("records")]
        public async Task<IActionResult> GetRecords()
        {
            try
            {
                var registros = await _kycService.ObtenerRegistrosAsync();
                return Ok(registros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los registros.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error al obtener los registros." });
            }
        }
    }
}
