namespace LapoLoanWebApi.ModelDto.PagenationFilterModel
{
    public class PageMode
    {
        public long StartPage { get; set; }
        public long EndPage { get; set; }
        public long TotalData { get; set; }
        public bool IsSelected { get; set; }
        public long SelectedNumber { get; set; }

        public long SkipLastData{ get; set; }
        public long TakeData { get; set; }
    }
}
