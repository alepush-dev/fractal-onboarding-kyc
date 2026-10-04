namespace onboardingKycApi.Models
{
    public class KycResponseResult
    {
        public string FullName { get; set; }
        public string DocumentNumber { get; set;  }
        public decimal Confidence { get; set; }
    }
}
