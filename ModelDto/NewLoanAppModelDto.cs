namespace LapoLoanWebApi.ModelDto
{
    public class NewLoanAppModelDto
    {
        public LoanBvnDetails BvnDetail { get; set; }
        public LoanAccountDetailsDto AcctDetail { get; set; }
        public LoanPersonalDetailsDto ClientDetail { get; set; }
        public LoanAppDto LoanDetailsData { get; set; }
    }

    public class LoanAppDto
    {
        public string AcctNumber { get; set; }
        public string AcctName { get; set; }
        public string BankName { get; set; }
        public double LoanAmount { get; set; }

        public int AccountId { get; set; }
        public string PFNumber { get; set; }

        public string Ternor { get; set; }

        public string OfficerOtherName { get; set; }
        public string OfficerFirstName { get; set; }

        public string RelationshipOfficerRef { get; set; }

        public dynamic StaffIdCard { get; set; }
        public dynamic PaySliptfiles { get; set; }
        public dynamic PassportPhotograph { get; set; }

    }

    public class LoanPersonalDetailsDto
    {
        public string Reasonforthisloan { get; set; }
        public string fullname { get; set; }
        public string RelationShip { get; set; }
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
        public dynamic acctDetails { get; set; }
        public dynamic BvnResponds { get; set; }



        public string CusState { get; set; }
        public string CusCity { get; set; }
        public string CusStateName { get; set; }
        public string CusCityName { get; set; }
        public string CusBusinessTypeText { get; set; }
        public string CusBusinessTypeValue { get; set; }
        public string CusBusinessSegmentText { get; set; }
        public string CusBusinessSegmentValue { get; set; }
    }

    public class LoanAccountDetailsDto
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

    public class LoanBvnDetails
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
