namespace LapoLoanWebApi.ModelDto
{
    public class AccessTokenResponses
    {
        public int status { get; set; }
        public string message_code { get; set; }
        public string message_description { get; set; }
        public string data { get; set; }
        public string timestamp { get; set; }
    }
}
