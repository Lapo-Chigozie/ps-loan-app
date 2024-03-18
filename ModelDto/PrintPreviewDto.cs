namespace LapoLoanWebApi.ModelDto
{
    public class PrintPreviewDto
    {
        public string PassPortImage { get; set; }
        public string BVN { get; set; }
        public string OrganizationAndOffice  { get; set;}

        public ClientInfos ClientInfo { get; set; }

        public ClientNextOfKinInfos ClientNextOfKinInfo { get; set; }

        public ClientOracles ClientOracle { get; set; }

        public ClientLoanDetails ClientLoanDetail { get; set;  }

        public ClientLoan ClientLoan { get; set; }
        public ClientBank ClientBank { get; set; }

        public ClientEmployments ClientEmployment { get; set; }
    }

    public class ClientInfos
    {
        public string First_Name { get; set; }
        public string Middle_Name { get; set; }
        public string Last_Name { get; set; }
        public string Home_Address { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }

        public string Phone_Number { get; set; }
        public string Alt_Phone_Number { get; set; }

        public string Marital_Status { get; set; }

        public string NoOfChildren { get; set; }

        public bool Has_IDCard { get; set; }
        public bool HasPass_Port { get; set; }
        public bool HasBVN { get; set; }
    }

    public class ClientNextOfKinInfos
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone_Number { get; set; }

        public string NokRelationShip { get; set; }
        
    }

    public class ClientLoanDetails
    {
        public string LoanAmount { get; set; }
        public string LoanAmountInCurrency { get; set; }
        public string LoanAmountInWords { get; set; }

        public string ApprovedLoanAmount { get; set; }
        public string ApprovedLoanAmountInCurrency { get; set; }
        public string ApprovedLoanAmountInWords { get; set; }
    }

    public class ClientLoan
    {
        public string MonthlyInstallmentAmount { get; set; }
        public string MonthlyInstallmentAmount_Currency { get; set; }

        public string LoanTernure { get; set; }
        public string InterestRate { get; set; }

        public string DisbursmentAmount { get; set; }
        public string DisbursmentDate { get; set; }

        public string MonthlyNet_Pay { get; set; }
        public string MonthlyNet_Pay_Currency { get; set; }


        public string Narration { get; set; }
        
    }

    public class ClientBank
    {
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
    }

    public class ClientOracles
    {
        public string IPPS_Number { get; set; }

        public string IDNumber { get; set; }

        public string IDIssueDate { get; set; }
        public string IDExpirementDate { get; set; }
    }

    public class ClientEmployments
    {
        public string Expire_Date { get; set; }
        public string Retirement_Date { get; set; }
    }
}
