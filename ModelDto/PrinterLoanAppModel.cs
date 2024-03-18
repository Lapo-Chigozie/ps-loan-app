namespace LapoLoanWebApi.ModelDto
{
    public class PrinterLoanAppModel
    {
        public PrinterLoanBvnDetails bvnDetail { get; set; }
        public PrinterLoanAccountDetailsDto acctDetail { get; set; }
        public PrinterLoanDetailsDto clientDetail { get; set; }
        public PrinterLoanAppDto loanDetailsData { get; set; }
        public PrinterLoanReviewStatusModelDto loanAppReviewStatus { get; set; }
    }


    //public class LoanApp
    //{
    //    public string Name { get; set; }
    //    public bool IsActive { get; set; }
    //    public string No { get; set; }
    //    public string HeaderId { get; set; }
    //    public string Gender { get; set; }
    //    public string AccountId { get; set; }
    //    public string RequestCode { get; set; }
    //    public string Amount { get; set; }
    //    public string Status { get; set; }
    //    public string IPPISNumber { get; set; }
    //    public DateTime CreatedDate1 { get; set; }

    //    public string createdDate { get; set; }

    //}

    public class PrinterLoanReviewStatusModelDto
    {
        public string status { get; set; }
        public string approvedDate { get; set; }
        public string approvedBy { get; set; }
        public bool isApproved { get; set; }

        public string comment { get; set; }
    }

    public class PrinterLoanAppDto
    {
        public string appId { get; set; }
        public string acctNumber { get; set; }
        public string acctName { get; set; }
        public string bankName { get; set; }
        public double loanAmount { get; set; }
        public string loanAmountString { get; set; }
        public int accountId { get; set; }
        public string pFNumber { get; set; }
        public string ternor { get; set; }
        public string staffIdCard { get; set; }
        public string paySliptfiles { get; set; }
        public string passportPhotograph { get; set; }


        public string staffIdCardUrl { get; set; }
        public string paySliptfilesUrl { get; set; }
        public string passportPhotographUrl { get; set; }
    }

    public class PrinterLoanDetailsDto
    {
        public string fullname { get; set; }
        public string pFNumber { get; set; }
        public string dateOfBirth { get; set; }
        public string phoneNumber { get; set; }
        public string altPhoneNumber { get; set; }
        public string residentialAddress { get; set; }
        public string retDate { get; set; }
        public string maritalStatus { get; set; }
        public string nokname { get; set; }
        public string nokphone { get; set; }
        public string nokaddress { get; set; }

        public string approvedName { get; set; }
        public string approvedDate { get; set; }

    }

    public class PrinterLoanAccountDetailsDto
    {
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string age { get; set; }
        public string currentAddress { get; set; }

        public string address { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string altPhone { get; set; }

        public string accountType { get; set; }
        public int accountId { get; set; }

    }

    public class PrinterLoanBvnDetails
    {
        public string responseCode { get; set; }
        public string bVN { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string dateOfBirth { get; set; }
        public string registrationDate { get; set; }
        public string enrollmentBank { get; set; }
        public string enrollmentBranch { get; set; }
        public string phoneNumber1 { get; set; }
        public string watchListed { get; set; }
    }
}
