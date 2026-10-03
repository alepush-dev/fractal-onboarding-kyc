using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using onboardingKycApi.Models;
using onboardingKycApi.Service;
using System.Data;

namespace onboardingKycApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KycController : ControllerBase
    {
        private readonly IKycService _kycService;
        private readonly IConfiguration _configuration;

        //Recibimo los servicios necesario para que funcione el controlador
        public KycController(IKycService kycService, IConfiguration configuration)
        {
            _kycService = kycService;
            _configuration = configuration;
        }

        //Endpoint para recibir la foto y el correo, procesar con la IA y guardar en bd
        [HttpPost("process")]
        public async Task<IActionResult> ProcessKyc([FromForm] KycRequest request)
        {

            if (request.ImageFile == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("El correo y la imagen del documento son obligatorios.");
            }

            try
            {
               //Mandamos la imagen a procesar con Gemini y guardamos el archivo localmente
                var kycResult = await _kycService.ProcessKycAsync(request);

                string imageUrl = "/uploads/" + request.ImageFile.FileName;


                //Nos conectamos a MySQL para registrar los datos usando el Stored Procedure
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand("sp_InsertOnboardingRecord", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        //Parametros
                        cmd.Parameters.AddWithValue("p_Email", request.Email);
                        cmd.Parameters.AddWithValue("p_FullName", kycResult.FullName);
                        cmd.Parameters.AddWithValue("p_DocumentNumber", kycResult.DocumentNumber);
                        cmd.Parameters.AddWithValue("p_OcrConfidence", kycResult.Confidence);
                        cmd.Parameters.AddWithValue("p_ImageUrl", imageUrl);

                        object result = await cmd.ExecuteScalarAsync();

                        //Respuestas
                        return Ok(new KycResponseModel
                        {
                            Email = request.Email,
                            FullName = kycResult.FullName,
                            DocumentNumber = kycResult.DocumentNumber,
                            Confidence = kycResult.Confidence,
                            ImageUrl = imageUrl,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno: " + ex.Message });
            }
        }


        // Endpoint para traer la lista de todos los registros históricos para la tabla
        [HttpGet("records")]
        public async Task<IActionResult> GetRecords()
        {
            try
            {
                var records = await _kycService.ObtenerRegistrosAsync();
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los registros: " + ex.Message });
            }
        }
    }
}