namespace LapoLoanWebApi.ModelDto
{
    public class LoanApp
    {
        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public bool IsActive { get; set; }
        public string No { get; set; }
        public string HeaderId { get; set; }
        public string Gender { get; set; }
        public string AccountId { get; set; }
        public string RequestCode { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public string IPPISNumber { get; set; }
        public DateTime CreatedDate1 { get; set; }

        public string createdDate { get; set; }

        public string TypeOfLoan { get; set; }

        public string GroupName { get; set; }

    }

    public class LoanAppReviewStatusModelDto
    {
        public string Status { get; set; }
        public string ApprovedDate  { get; set; }
        public string ApprovedBy  { get; set; }
        public bool IsApproved { get; set; }

        public string Comment { get; set; }
    }
    public class LoanAppListModelDto
    {
        public AllLoanBvnDetails BvnDetail { get; set; }
        public AllLoanAccountDetailsDto AcctDetail { get; set; }
        public AllLoanPersonalDetailsDto ClientDetail { get; set; }
        public AllLoanAppDto LoanDetailsData { get; set; }
        public LoanAppReviewStatusModelDto loanAppReviewStatus { get; set; }
    }

    public class AllLoanAppDto
    {
        public string AppId { get;set; }
        public string AcctNumber { get; set; }
        public string AcctName { get; set; }
        public string BankName { get; set; }
        public double LoanAmount { get; set; }
        public string LoanAmountString { get; set; }
        public int AccountId { get; set; }
        public string PFNumber { get; set; }
        public string Ternor { get; set; }
        public dynamic StaffIdCard { get; set; }
        public dynamic PaySliptfiles { get; set; }
        public dynamic PassportPhotograph { get; set; }

        public bool IsStaffIdCard { get; set; }
        public bool IsPaySliptfiles { get; set; }
        public bool IsPassportPhotograph { get; set; }

        public dynamic StaffIdCardUrl { get; set; }
        public dynamic PaySliptfilesUrl { get; set; }
        public dynamic PassportPhotographUrl { get; set; }

        
        public string ReOfficerFirstName { get; set; }
        public string ReOfficerLastName { get; set; }

        public string ReconciliationOfficerHub { get; set; }

        public bool IsAnonymousOrStandard { get; set; }



        public string EmployeementDate { get; set; }
        public string BusinessSegment{ get; set; }
        public string BusinessType { get; set; }
        public string City { get; set; }
        public string StateOfOrigin { get; set; }





        public string BeneficialaryName { get; set; }
        public string Narration { get; set; }

        public string ExportedDate { get; set; }

        public string ExportedBy { get; set; }
        public string TeamOfficerOthername { get; set; }
        public string TeamGroundName { get; set; }
        public string DisbursementAmount { get; set; }

        public string DisbursementDate { get; set; }
        public string TeamOfficerFirstname { get; set; }


        public string EmploymentDate { get; set; }
        public string RetirementDate { get; set; }
    }

    public class AllLoanPersonalDetailsDto
    {
        public string RelationShip { get; set; }
        public string fullname { get; set; }
        public string PFNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string AltPhoneNumber { get; set; }
        public string ResidentialAddress { get; set; }
        public dynamic retDate { get; set; }
        public string MaritalStatus { get; set; }
        public string nokname { get; set; }
        public string nokphone { get; set; }
        public string nokaddress { get; set; }

        public string ApprovedName { get; set; }
        public string ApprovedDate { get; set; }

    }

    public class AllLoanAccountDetailsDto
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string CurrentAddress { get; set; }

        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AltPhone { get; set; }

        public string AccountType { get; set; }
        public dynamic AccountId { get; set; }
    }

    public class AllLoanBvnDetails
    {
        public string ResponseCode { get; set; }
        public string BVN { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string RegistrationDate { get; set; }
        public string EnrollmentBank { get; set; }
        public string EnrollmentBranch { get; set; }
        public string PhoneNumber1 { get; set; }
        public string WatchListed { get; set; }
    }
}
