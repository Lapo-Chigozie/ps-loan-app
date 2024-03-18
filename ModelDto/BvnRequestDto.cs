namespace LapoLoanWebApi.ModelDto
{
    public class BvnRequestDto
    {
        public string BvnRequest { get; set; }

        public long AcctId { get; set; }
        
    }

    public class BvnCheckerDto
    {
        public string BvnRequest { get; set; }

        public string AcctId { get; set; }

    }
    public class BvnRequestModelDto
    {
        public string BVN { get; set; }
    }

    public class Payload
    {
        public string xPayload { get; set; }
    }
    
    public class AESRoot
    {
        public string aesKey { get; set; }
        public string aesIv { get; set; }
        public string access_token { get; set; }
        public int token_exp { get; set; }
    }
}
