namespace LapoLoanWebApi.ModelDto
{
    public class AcctDto
    {
        public string AccountType { get; set; }
        public bool AllowLoginTwoFactor { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
    }


    public class BvnAuthDto
    {
        public string Phone { get; set; }
        public string BVN { get; set; }
        public string EmailAddress { get; set; }
        public object AccountId { get; set; }
    }
}
