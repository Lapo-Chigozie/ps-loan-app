using Newtonsoft.Json;

namespace LapoLoanWebApi.ModelDto
{
    public class BvnRespondsDto
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


    public class BvnCheckerRespondsDto
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

        public string EmailAddress { get; set; }
        public string AccountId { get; set; }
    }
}
