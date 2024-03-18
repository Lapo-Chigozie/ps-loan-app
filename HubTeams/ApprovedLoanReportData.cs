namespace LapoLoanWebApi.HubTeams
{
    public class ApprovedLoanReportData
    {
        public string Reconciliation_Officer_Hub_Group { get; set; }

        public string Reconciliation_Officer_Hub_Staff_ID { get; set; }
        public string Bank_Code { get; set; }
        public string Bank_Name { get; set; }

        public string Bank_Account_Name { get; set; }

        public string Bank_Account_Number { get; set; }

        public string CustomerName { get; set; }

        public string LoanInterest { get; set; }

        public string LoanTenure { get; set; }

        public string Approved_Date { get; set; }

        public string Request_Date { get; set; }

        public string Request_Code { get; set; }



        public string Loan_Request_Amount { get; set; }

        public string Loan_Repayment_Amount { get; set; }

        public string Total_Repayment_Amount { get; set; }

        public string Exported_By { get; set; }
    }
}
