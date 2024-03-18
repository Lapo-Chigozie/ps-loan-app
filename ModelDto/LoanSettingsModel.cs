namespace LapoLoanWebApi.ModelDto
{
    public class LoanSettingsModel
    {
        public string AcctId { get; set; }

        public bool IsBlockLoanPortal { get; set; }

        public string Message { get; set; }

        public bool LoanInterestCalculation { get; set; }

        public double LoanInterest { get; set; }

        public bool LoanScheduled { get; set; }

        public double MaxLoanAmount { get; set; }

        public double MinLoanAmount { get; set; }


        public bool SendSMSCustomerLoanApplicationSubmitted { get; set; }
        public bool SendSMSHubTeamLeadLoanApplicationSubmitted { get; set; }
        public bool SendEmailCustomerLoanApplicationSubmitted { get; set; }
        public bool SendEmailHubTeamLeadLoanApplicationSubmitted { get; set; }





    }


}
