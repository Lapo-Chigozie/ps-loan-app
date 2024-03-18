namespace LapoLoanWebApi.LoanScheduled.Model
{
    public class LoanRespondsModel
    {
        public double no { get; set; }
        public double TotalAmount { get; set; }
        public string TotalAmountWithCurrency { get; set; }

        public string NextSchedeledDate { get; set; }

        public DateTime NextSchedeledInDate { get; set; }

        public string NextSchedeledTime { get; set; }

        public TimeSpan NextSchedeledInTime { get; set; }


        public double LoanAmount { get; set; }
        public string LoanAmountWithCurrency { get; set; }

        public double InterestAmount { get; set; }
        public string InterestAmountWithCurrency { get; set; }
    }
}
