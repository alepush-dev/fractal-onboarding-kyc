namespace onboardingKycApi.Models
{
    public class KycResponseModel : KycResponseResult
    {
        public int Id {  get; set; }
        public string Email { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
