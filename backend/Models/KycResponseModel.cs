namespace onboardingKycApi.Models
{
    public class KycResponseModel : KycResponseResult
    {
        public int Id {  get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
