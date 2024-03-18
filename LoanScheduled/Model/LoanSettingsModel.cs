namespace LapoLoanWebApi.LoanScheduled.Model
{
    public class LoanSettingsModel
    {
        public double LoanInterest { get; set; }

        public double MinLoanAmount { get; set; }

        public double MaxLoanAmount { get; set; }

        public bool UseSalaryAsMaxLoanAmount { get; set; }

        public string Id { get; set; }
    }
}
