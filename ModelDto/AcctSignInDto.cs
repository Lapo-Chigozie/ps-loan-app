
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.ModelDto
{
    public class AcctSignInDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

public class UpdateAcct
{
    public long AccountId { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }
    public DateTime CurrentDate { get; set; }
 
}

public class AccountInfo
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public string Username { get; set; }


    public string Password { get; set; }
    public string Status { get; set; }
    public DateTime LastLoginDate { get; set; }


    public DateTime CreatedDate { get; set; }
    public string AccountType { get; set; }
    public bool AllowLoginTwoFactor { get; set; }
  
}