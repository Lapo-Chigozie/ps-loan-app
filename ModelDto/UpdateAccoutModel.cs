namespace LapoLoanWebApi.ModelDto
{
    public class UpdateAccoutModel
    {
        public string EmploymentGender { get; set; }
        public string EmploymentFirstName { get; set; }

        public string EmploymentMiddleName { get; set; }
        public string EmploymentLastName { get; set; }
        public DateTime EmploymentDateOfBirth { get; set; }
        public string EmploymentPhoneNumber { get; set; }
        public DateTime EmploymentDate { get; set; }
        public string identification   { get; set; }

        public string Bvn { get; set; }

        public DateTime RetirementDate  { get; set; }

        public string EmploymentMaritalStatus { get; set; }
    }


}
