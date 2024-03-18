namespace LapoLoanWebApi.ModelDto
{
    public class SignInModel
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
