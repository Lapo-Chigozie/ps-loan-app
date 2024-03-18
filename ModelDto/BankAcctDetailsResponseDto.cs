namespace LapoLoanWebApi.ModelDto
{
    public class BankAcctDetailsResponseDto
    {
        public string  account_number { get; set; }
        public string account_name { get; set; }
        public string branch_code { get; set; }
        public string branch_name { get; set; }
        public string customer_number { get; set; }
        public string bank_verification_number { get; set; }
        public string account_type { get; set; }
        public string account_class { get; set; }
        public string account_status { get; set; }
        public string gender { get; set; }
        public string primary_phone_number { get; set; }

        public string primary_email_address { get; set; }
        public string primary_physical_address { get; set; }

             
    }
}
