
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.ModelDto
{
    public class RespondMessageDto
    {
        public string Message { get; set; }
        public string TryCatchMessage { get; set; }
        public bool IsActive { get; set; }
        public object Data { get; set; }
        public dynamic DataLoad { get; set; }
        public Status status { get; set; }
        public string StatusMgs { get; set; }

        public bool IsTwoFactorAuth { get; set; }

        private System.Data.SqlClient.SqlConnection sqlCnn { get; set; }
        private Microsoft.Data.SqlClient.SqlConnection MsSQlCnn { get; set; }
        private SqlDataReader MsReader { get; set; }

        private SqlDataReader Reader { get; set; }
        private  SqlCommand sqlCommand { get; set; }
        private SqlCommand MsSqlCommand { get; set; }

        public RespondMessageDto(System.Data.SqlClient.SqlConnection sqlCnn, Microsoft.Data.SqlClient.SqlConnection MsSQlCnn, SqlDataReader MsReader, SqlDataReader Reader, SqlCommand MsSqlCommand, SqlCommand sqlCommand, string Message, string TryCatchMessage, bool IsActive, object Data, dynamic DataLoad, Status Status, string StatusMgs, bool IsTwoFactorAuth = false)
        {
            this.Message = Message;
            this.TryCatchMessage = TryCatchMessage;
            this.IsActive = IsActive;
            this.Data = Data;
            this.DataLoad = DataLoad;
            this.status = Status;
            this.StatusMgs = StatusMgs;

            this.IsTwoFactorAuth = IsTwoFactorAuth;
            this.sqlCnn = sqlCnn;
            this.MsSQlCnn = MsSQlCnn;

            this.MsReader = MsReader;
            this.Reader = Reader;

            this.sqlCommand = sqlCommand;
            this.MsSqlCommand = MsSqlCommand;

            Closed();
        }

        public void Closed()
        {
            try
            {
                if (this.sqlCommand != null)
                {
                    this.sqlCommand.Clone();
                   
                }

                if (this.MsSqlCommand != null)
                {
                    this.MsSqlCommand.Clone();
                }

                if (this.MsReader != null)
                {
                    this.MsReader.CloseAsync();
                }

                if (this.Reader != null)
                {
                    this.Reader.CloseAsync();
                }

                if (this.sqlCnn != null)
                {
                    this.sqlCnn.CloseAsync();
                }

                if (this.MsSQlCnn != null)
                {
                    this.MsSQlCnn.CloseAsync();
                }
            }
            catch (Exception ex) 
            { 
            
            }
        }
    }

    public class AccountType
    {
        public const string Adiministration = "Admin";
        public const string Employee = "Employee";
        public const string Customer = "Customer";
        
    }

    public enum Status {
        None = 0,
        Completed=1,
        NotCompleted = 9,
        Success =2,
        Failed=3,
        Ongoing=4,
        Joined=5,
        Active=6,
        NotActive=7,
        Ërror=8
    }

    public class StatusMgs
    {
        public const string Approved = "Approved";
        public const string Completed = "Completed";
        public const string NotCompleted = "Not Completed";

        public const string Disbursed = "Disbursed";

        public const string None = "None";
        public const string Success = "Success";
        public const string Error = "Error";
        public const string Failed = "Failed";
        public const string Ongoing = "Ongoing";
        public const string Joined = "Joined";
        public const string Active = "Active";
        public const string NotActive = "Not Active";
        public const string Pending = "Pending";
        public const string Cancel = "Cancelled";

        public const string Declined = "Declined";
    }
}
