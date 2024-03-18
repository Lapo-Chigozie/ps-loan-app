namespace LapoLoanWebApi.ModelDto
{
    public class AcctResetDto
    {
        public string EmailAddress { get; set; }
    }

    public class NewAcctPasswordResetDto
    {
        public string AcctId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
         public string ConfirmPassword { get; set; } = string.Empty;
    }
}
