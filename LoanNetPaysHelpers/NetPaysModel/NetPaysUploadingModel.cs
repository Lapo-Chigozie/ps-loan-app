namespace LapoLoanWebApi.LoanNetPaysHelpers.NetPaysModel
{
    public class NetPaysUploadingModel
    {
        public IFormFile File { get; set; }
    }

    public class FileUploadSummary
    {
        public int TotalFilesUploaded { get; set; }
        public string TotalSizeUploaded { get; set; }
        public IList<string> FilePaths { get; set; } = new List<string>();
        public IList<string> NotUploadedFiles { get; set; } = new List<string>();
    }
}
