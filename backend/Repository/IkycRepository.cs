using onboardingKycApi.Models;

namespace onboardingKycApi.Repository
{
    // Contrato de acceso a datos: el resto de la aplicación no sabe que detrás hay MySQL
    public interface IKycRepository
    {
        // Registra un cliente usando el Stored Procedure y devuelve el Id generado
        Task<int> InsertarRegistroAsync(string email, KycResponseResult datos, string imageUrl);

        // Devuelve el historial completo de registros
        Task<IEnumerable<KycResponseModel>> ObtenerRegistrosAsync();
    }
}
