using System.Net.Mail;
using System.Reflection;

namespace LapoLoanWebApi.ModelDto
{
    public class UpdateProfileModelDto
    {
        public string FirstName { get; set; }
        public string Middle { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string AltPhoneNumber { get; set; }
        public string CurrentAddress { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public long AcctId { get; set; }
        public string MarrintalStatus { get; set; }

    }

  
}
