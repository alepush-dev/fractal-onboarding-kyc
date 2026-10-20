namespace onboardingKycApi.Models
{
    public class KycResponseResult
    {
        public string FullName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
    }
}
