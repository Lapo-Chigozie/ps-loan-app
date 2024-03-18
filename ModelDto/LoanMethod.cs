namespace LapoLoanWebApi.ModelDto
{
    public class LoanMethod
    {
        public string Id { get; set; }
        public int Name { get; set; }
        public string Date { get; set; }
        public int Description { get; set; }
        public string Status { get; set; }
        public string NewNameLoanTenure { get; set; }
        public string NewLoanInterest { get; set; }
    }

    public class NameLoanMethod
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public string LoanInterest { get; set; }
        
    }

    public class NarrationM
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
    }
}
