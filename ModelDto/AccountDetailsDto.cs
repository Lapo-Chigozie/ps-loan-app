namespace LapoLoanWebApi.ModelDto
{
    public class AccountDetailsDto
    {
        public string RoleType { get; set; }
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

        public string AccountType { get; set;}
        public long AccountId { get; set; }
    }
}
