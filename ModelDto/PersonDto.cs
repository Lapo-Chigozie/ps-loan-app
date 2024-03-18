using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.ModelDto
{
    public class PersonDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName
        { get; set; }
        public string Gender
        { get; set; }
        public DateTime CreatedDate
        { get; set; }
        public string Staff
        { get; set; }
        public string PhoneNumber
        { get; set; }
        public string EmailAddress
        { get; set; }
        public string CurrentAddress
        { get; set; }
        public string AltPhoneNumber
        { get; set; }
        public int Age
        { get; set; }
        public string PositionOrRole
        { get; set; }

    }
}
