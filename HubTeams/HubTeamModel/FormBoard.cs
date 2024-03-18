using LapoLoanWebApi.FileExport.Models;

namespace LapoLoanWebApi.HubTeams.HubTeamModel
{
    public class FormBoard
    {
        public FormBoard() 
        {
            if (this.MarkedExportLoans != null && this.MarkedExportLoans.Count > 0)
            {
                this.HasMarkedExportLoans = true;
            }
        }

        public string ExportedBy { get; set; }
        
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string Status { get; set; }

        public bool HasMarkedExportLoans { get; set; }

        public List<ExportLoanAppModel> MarkedExportLoans { get; set; }
    }
}
