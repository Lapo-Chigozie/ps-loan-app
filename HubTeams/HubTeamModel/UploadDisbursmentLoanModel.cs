namespace LapoLoanWebApi.HubTeams.HubTeamModel
{
    public class UploadDisbursmentLoanModel
    {
        public string Request_Code { get; set; }
        public string Status { get; set; }
        public DateTime Disbursement_Date { get; set; }
        public string Disbursement_Officer_Staff_ID { get; set; }
        public double Disbursed_Amount { get; set; }
    }

    public class UploadRepaymentLoanModel
    {
        public string Customer_Name { get; set; }
        public string Service_Account { get; set; }
        public string Loan_Request_Code { get; set; }
        public string Uploaded_By_Member_Staff_ID { get; set; }
        public double Repayment_Amount { get; set; }


        public string Status { get; set; }
        public string Repayment_Status { get; set; }
        public DateTime Repayment_Date { get; set; }
        public string Loan_Repayment_For { get; set; }
        public double Repayment_Balance { get; set; }
    }
}
