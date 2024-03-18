namespace LapoLoanWebApi.LoanScheduled.Model
{
    public class ScheduledDto
    {
        public string IPPISNumber { get; set; }
        public string  AccountId { get; set; }
    }

    public class ScheduledMethod
    {
        public string IPPISNumber { get; set; }
        public string AccountId { get; set; }

        public double Amount { get; set; }

        public string Tenure { get; set; }

    }
}
