namespace onboardingKycApi.Models
{
    public class KycRequest
    {
        public string Email { get; set; } = string.Empty;
        public IFormFile ImageFile { get; set; } = null!;
    }

}
