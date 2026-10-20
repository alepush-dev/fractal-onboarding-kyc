using MySqlConnector;
using onboardingKycApi.Models;
using System.Data;

namespace onboardingKycApi.Repository
{
    public class KycRepository : IKycRepository
    {
        private readonly string _connectionString;

        public KycRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<int> InsertarRegistroAsync(string email, KycResponseResult datos, string imageUrl)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand("sp_InsertOnboardingRecord", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            // Parámetros: evitan SQL Injection
            cmd.Parameters.AddWithValue("@p_Email", email);
            cmd.Parameters.AddWithValue("@p_FullName", datos.FullName);
            cmd.Parameters.AddWithValue("@p_DocumentNumber", datos.DocumentNumber);
            cmd.Parameters.AddWithValue("@p_OcrConfidence", datos.Confidence);
            cmd.Parameters.AddWithValue("@p_ImageUrl", imageUrl);

            // El SP devuelve LAST_INSERT_ID() AS NewId
            var nuevoId = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(nuevoId);
        }

        public async Task<IEnumerable<KycResponseModel>> ObtenerRegistrosAsync()
        {
            var registros = new List<KycResponseModel>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = "SELECT Id, Email, FullName, DocumentNumber, OcrConfidence, ImageUrl, CreatedAt FROM clientskyc ORDER BY Id ASC";

            await using var cmd = new MySqlCommand(query, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                registros.Add(new KycResponseModel
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

            return registros;
        }
    }
}
