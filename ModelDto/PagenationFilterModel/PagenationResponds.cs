using System.Drawing.Printing;

namespace LapoLoanWebApi.ModelDto.PagenationFilterModel
{
    public class PagenationResponds
    {
        public long PageSize { get; set; }
        public long UnderPageCount { get; set; }

        public object JsonData { get; set; }

        public long TotalData { get; set; }

        public long ActivePagenation { get; set; }

        public long MinSelectedNumber { get; set; }

        public long MaxSelectedNumber { get; set; }

        public long SkipLastData { get; set; }
        public long TakeData { get; set; }

        public string Search { get; set; }
        public string StatusSearch { get; set; }

        public string LastSelectedNumber { get; set; }

        public string NextSelectedNumber { get; set; }

        public string TotalSelectedNumber { get; set; }

        public object Data { get; set; }

        public List<PageMode> LoopPageModes { get; set; }


        public List<PageMode> PageModes { get; set; }

        public bool HasPreviousPagenation { get; set; }

        public bool HasNextPagenation { get; set; }
    }
}
