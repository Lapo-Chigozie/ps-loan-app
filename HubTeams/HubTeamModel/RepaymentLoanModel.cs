using DocumentFormat.OpenXml.Office2010.Excel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.HubTeams.HubTeamModel
{
    public class RepaymentLoanModel
    {
        public string   CreatedAccount { get;set; }
        public string Loan_Request_Code  { get;set; }
        public string Uploaded_By_Member_Staff_ID { get;set; }
        public string Repayment_Status   { get; set; }
        public string Loan_Repayment_For { get; set; }
        public string Status  { get; set; }
        public string CreatedDate { get; set; }
        public string Repayment_Date { get; set; }
        public string Repayment_Amount   { get; set; }
        public string Repayment_Balance { get; set; }
        public string Service_Account { get; set; }
        public string Customer_Name  { get; set; }

        public string No { get; set; }
        
    }
}
