using System.Reflection.Metadata;

namespace LapoLoanWebApi.ModelDto.PagenationFilterModel
{
    public class PagenationFilterDto
    {
        public bool MarkAllData { get; set; } = false;

        public string status { get; set; }
        public string dateTo { get; set; }
        public string dateFrom { get; set; }
        public int pageDataSize { get; set; }
        public string searchText { get; set; }
        public int AccountId { get; set; }
        public PageNextSelection PageNext { get; set; }

        public AccountPermissionDetails PermissionPage { get; set; }

        public bool IsSearchBar { get; set; }

        public string AcctId { get; set; }
        public string AppId { get; set; }

        public bool IsSelected { get; set; }
        public int SelectedNumber { get; set; }
    }
    public class PageNextSelection
    {
        public int StopTakeCount { get; set; }

        public int SkipCount { get; set; }
        public int TakeCount { get; set; }

        public int LastTakeCount
        { get; set; }
        public int LastSkipCount
        { get; set; }

        public bool HasBar { get; set; }
    }
}
