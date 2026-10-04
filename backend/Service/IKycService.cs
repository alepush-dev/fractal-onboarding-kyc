using onboardingKycApi.Models;

namespace onboardingKycApi.Service
{
    public interface IKycService
    {
        Task<KycResponseModel> ProcessKycAsync(KycRequest request);

        Task<IEnumerable<KycResponseModel>> ObtenerRegistrosAsync();
    }
}
