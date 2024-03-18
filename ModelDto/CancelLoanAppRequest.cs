namespace LapoLoanWebApi.ModelDto
{
    public class CancelLoanAppRequest
    {
        public string AccountId { get; set; }

        public string Comment { get; set; }

        public string LoadHeaderId { get; set; }

        public int CreatingStaff_ID { get; set; }

    }
}
