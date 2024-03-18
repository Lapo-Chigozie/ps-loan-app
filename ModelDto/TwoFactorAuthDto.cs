using Microsoft.Identity.Client;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.ModelDto
{
    public class TwoFactorAuthDto
    {
        public string Code { get; set; }
        public string KeyOfTwoFactor { get; set; }
        public DateTime ExpireDateTime { get; set; }

        public bool Éxpired { get; set; }
    }

    public class PwrdChangeDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

       
    }

    public class RegisterCustomerDto
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public bool PrivacyPolicy { get; set; }
        public bool TermAndConditions { get; set; }
    }

    public class EmailAcctDto
    {
        public string EmailAddress { get; set; }
    }

    public class TwoFactorAuthÇodeDto
    {
        public string Code { get; set; }
        public string AccountId  { get; set; }
    }

    public class ReturnTwoFactorAuthDto
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string Code { get; set; }
        public string BVNVerification { get; set; }
        public DateTime ExpiredDateTime { get; set; }
        public DateTime GenaratedDateTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActivexpired { get; set; }

        public bool IsTwoFactorAuth { get; set; }
       
    }

    public class SaveTwoFactorAuthDto
    {
      
        public long AccountId { get; set; }
        public string CodeGenerated { get; set; }
        public string BVNVerification { get; set; }
        public DateTime ExpiredDateTime { get; set; }
        public DateTime GenaratedDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool Active { get; set; }

        public bool IsTwoFactorAuth { get; set; }
    }

}
