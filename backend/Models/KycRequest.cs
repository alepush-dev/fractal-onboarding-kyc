namespace onboardingKycApi.Models
{
    public class KycRequest
    {
        public string Email { get; set; }
        public IFormFile ImageFile { get; set; }
    }

}
