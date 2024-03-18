using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanNetPaysHelpers.NetPaysEnum;
using LapoLoanWebApi.LoanScafFoldModel;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using System.Data;
using System.Configuration;

namespace LapoLoanWebApi.HubTeams
{
    public class UploadRepaymentSheduled
    {
        private long fileUploadCount = 0;
        private OleDbConnection excelConnection = null;
        private OleDbDataAdapter adapter = null;
        private System.Data.SqlClient.SqlConnection MsSQlConn = null;
        private ControllerBase ControllerBase { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }
        private E360AuthHttpClient e360AuthHttpClient { get; set; }

        private LapoLoanDBContext lapoLoanDB;

        public event EventHandler<ClientNetPayINFO> onReaderClosed;

        public event EventHandler<ClientNetPayINFO> onValidatingExcelSheetNames;

        public event EventHandler<ClientNetPayINFO> onReadAllExcelSheetNames;

        public event EventHandler<ClientNetPayINFO> onReadErrorExcelSheetName;

        public event EventHandler<ClientNetPayINFO> onReadering;

        public event EventHandler<ClientNetPayINFO> onConnectionOpen;

        public bool IsExcelConnectionOpened = false;

        private IConfiguration _configuration;


        public UploadRepaymentSheduled(ControllerBase controllerBase, IConfiguration configuration
)
        {

            this._configuration =  configuration;
            this.ControllerBase = controllerBase;
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.e360AuthHttpClient = new E360AuthHttpClient(controllerBase, configuration);

            this.lapoCipher01 = new LapoCipher01();

            this.excelConnection = new OleDbConnection();
            this.adapter = new OleDbDataAdapter();

            this.MsSQlConn = new System.Data.SqlClient.SqlConnection(new LapoDBConnectionStrings(this._configuration).ConnectionString());
        }

        private List<string> stringsSHEETS = new List<string>();

        private async void OpenExcelConnection()
        {
            this.IsExcelConnectionOpened = true;

            if (this.excelConnection.State == System.Data.ConnectionState.Closed)
            {
                await this.excelConnection.OpenAsync();
            }

            if (this.MsSQlConn.State == System.Data.ConnectionState.Closed)
            {
                await this.MsSQlConn.OpenAsync();
            }

            return;
        }

        private async void CloseExcelConnection()
        {
            this.IsExcelConnectionOpened = false;

            if (this.excelConnection.State == System.Data.ConnectionState.Open)
            {
                await this.excelConnection.CloseAsync();
            }

            if (this.MsSQlConn.State == System.Data.ConnectionState.Open)
            {
                await this.MsSQlConn.CloseAsync();
            }

            return;
        }

        private void ConnectDbUrl(string fullPath)
        {
            string exdbUrl = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 8.0;HDR=YES;'";

            excelConnection.ConnectionString = exdbUrl;
        }

        private async Task<List<string>> ReadAllExcelSheetNames(DataTable dataTables)
        {
            List<string> excelSheets = new List<string>();

            try
            {
                if ((dataTables != null))
                {
                    // Add the sheet name to the string array.
                    foreach (DataRow row in dataTables.Rows)
                    {
                        var ResultData = row["TABLE_NAME"].ToString();
                        excelSheets.Add(ResultData);
                    }

                    return excelSheets;
                }

                return excelSheets;
            }
            catch (Exception ex)
            {
                return excelSheets;
            }
        }

        public async Task<RespondMessageDto> LoadExcelData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string fullPath)
        {
            try
            {
                this.ConnectDbUrl(fullPath);
                DataTable dtTables = new DataTable();

                //to get the schema of the workbook.
                // dtTables = excelConnection.GetSchema();

                //get the tables in the workbook
                this.OpenExcelConnection();
                dtTables = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                this.CloseExcelConnection();
                stringsSHEETS = await this.ReadAllExcelSheetNames(dtTables);

                if (stringsSHEETS == null || stringsSHEETS.Count <= 0)
                {
                    this.CloseExcelConnection();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "We couldn’t find any Excel sheets in the file your uploading.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

                if (stringsSHEETS != null && stringsSHEETS.Count > 1)
                {
                    this.CloseExcelConnection();
                   // return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Excel file must contain Maximum of 3 sheets before uploading.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

                //if (stringsSHEETS == null || stringsSHEETS.Count <= 0)
                //{
                //    this.CloseExcelConnection();
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel file must contains a sheets and 10 t0 11 columns in any sheet.", false, " ", null, Status.Ërror, StatusMgs.Error);
                //}

                if (stringsSHEETS != null && stringsSHEETS.Count > 0)
                {
                    this.CloseExcelConnection();
                    return await this.ImportFromExcelBulked(stringsSHEETS, stringsSHEETS.Count);
                }

                this.CloseExcelConnection();
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain at least 10 t0 11 columns.", false, " ", null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                this.CloseExcelConnection();
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain at least 10 t0 11 columns.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ImportFromExcelBulked(List<string> ExcelSheets, int totalSheet)
        {
            try
            {
                DataTable dtExcel1 = new DataTable();

                dtExcel1.Columns.AddRange(new DataColumn[10]
                {
                       new DataColumn ("Loan_Request_Code", typeof(string)),
                       new DataColumn ("Uploaded_By_Member_Staff_ID", typeof(string)),
                       new DataColumn ("Repayment_Amount", typeof(decimal)),
                       new DataColumn ("Status", typeof(string)),
                       new DataColumn ("Repayment_Status", typeof(string)),
                       new DataColumn ("Repayment_Date", typeof(DateTime)),

                       new DataColumn ("Loan_Repayment_For", typeof(string)),
                       new DataColumn ("Repayment_Balance", typeof(decimal)),

                       new DataColumn ("Customer_Name", typeof(string)),
                       new DataColumn ("Service_Account", typeof(string))
                });

                this.OpenExcelConnection();

                var DataSheetRows = this.excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows;

                if (DataSheetRows.Count > 0)
                {
                    string Sheet1 = DataSheetRows[0]["TABLE_NAME"].ToString();
                    string sqlText = string.Concat("select * from [", Sheet1, "] where Loan_Request_Code is not  null");
                    var oda1 = new OleDbDataAdapter(sqlText, this.excelConnection);
                    oda1.Fill(dtExcel1);

                    this.CloseExcelConnection();
                    this.Loop1Stage = NetPaysEnum.HasSheet1;

                    return await SaveAllNetPaysAsync(dtExcel1);
                }

                this.CloseExcelConnection();

                var message = "Loan_Request_Code, Uploaded_By_Member_Staff_ID, Repayment_Amount is decimal, Status,Repayment_Status, Repayment_Date, Loan_Repayment_For, Repayment_Balance, Customer_Name, Service_Account headers are required before uploading";

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, message, false, message, message, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                this.CloseExcelConnection();

                var message = "Loan_Request_Code, Uploaded_By_Member_Staff_ID, Repayment_Amount is decimal, Status,Repayment_Status, Repayment_Date, Loan_Repayment_For, Repayment_Balance, Customer_Name, Service_Account headers are required before uploading" + ex.Message ?? ex.InnerException.Message;

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, message, false, message, message, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SaveAllNetPaysAsync(DataTable dataTable1)
        {
            try
            {
                if (dataTable1 != null && dataTable1.Rows.Count > 2000)
                {
                    string message = dataTable1.Rows.Count + "Data rows is much on Sheet 1, Maximum data must be below or equal to 2000.";

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, "", null, Status.Success, StatusMgs.Success);
                }

                if (this.Loop1Stage == NetPaysEnum.HasSheet1)  // Takes 4 mins to bind Data Table,  4 * 3 = 16mins for 3 Data Tables
                {
                    this.Loop1Stage = NetPaysEnum.StartSheet;
                    // Task.Run(() =>.Wait());

                    return await this.SaveSheet1NetPaysAsync(dataTable1);
                }

                // Thread.Sleep(dataTable1.Rows.Count);

                return await this.SaveSheet1NetPaysAsync(dataTable1);

            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex.Message ?? ex.InnerException.Message, ex.Message ?? ex.InnerException.Message, Status.Ërror, StatusMgs.Error);
            }
        }

        private NetPaysEnum Sheet1HasDateError = NetPaysEnum.None;
        private NetPaysEnum Sheet2HasDateError = NetPaysEnum.None;
        private NetPaysEnum Sheet3HasDateError = NetPaysEnum.None;

        private NetPaysEnum Loop1Stage = NetPaysEnum.None;
        private NetPaysEnum Loop2Stage = NetPaysEnum.None;
        private NetPaysEnum Loop3Stage = NetPaysEnum.None;

        private int TotalSheet1 = 0;
        private int TotalSheet1Counting = 0;

        private int TotalSheet2 = 0;
        private int TotalSheet2Counting = 0;

        private int TotalSheet3 = 0;
        private int TotalSheet3Counting = 0;

        public async Task<RespondMessageDto> SaveSheet1NetPaysAsync(DataTable dataTable1)
        {
            try
            {
                var ListRepaymentLoans = new List<UploadRepaymentLoanModel>();
                this.TotalSheet1 = dataTable1.Rows.Count;

                foreach (DataRow row in dataTable1.Rows)
                {
                    var Customer_Name = row["Customer_Name"].ToString();
                    var Service_Account = row["Service_Account"].ToString();
                    var Loan_Request_Code = row["Loan_Request_Code"].ToString();
                    var Uploaded_By_Member_Staff_ID = row["Uploaded_By_Member_Staff_ID"].ToString();
                    var Repayment_Amount = row["Repayment_Amount"].ToString();
                    var Status = row["Status"].ToString();
                    var Repayment_Status = row["Repayment_Status"].ToString();
                    var Repayment_Date = row["Repayment_Date"].ToString();
                    var Loan_Repayment_For = row["Loan_Repayment_For"].ToString();
                    var Repayment_Balance = row["Repayment_Balance"].ToString();

                    var NewExcelSheet = new UploadRepaymentLoanModel()
                    {
                        Customer_Name = Customer_Name,
                        Loan_Repayment_For = Loan_Repayment_For,
                        Loan_Request_Code = Loan_Request_Code,
                        Repayment_Status = Repayment_Status,
                        Service_Account = Service_Account,
                        Uploaded_By_Member_Staff_ID = Uploaded_By_Member_Staff_ID,
                        Status = Status
                    };

                    try
                    {
                        NewExcelSheet.Repayment_Date = Convert.ToDateTime(Repayment_Date);
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        NewExcelSheet.Repayment_Amount = Convert.ToDouble(Repayment_Amount);
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        NewExcelSheet.Repayment_Balance = Convert.ToDouble(Repayment_Balance);
                    }
                    catch (Exception ex)
                    {

                    }

                    ListRepaymentLoans.Add(NewExcelSheet);

                    TotalSheet1Counting += 1;
                }

                dataTable1 = null;
              
                this.Loop1Stage = NetPaysEnum.Finished;

                this.CloseExcelConnection();

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, ListRepaymentLoans, ListRepaymentLoans, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                dataTable1 = null;

                this.Loop1Stage = NetPaysEnum.HasError;

                this.CloseExcelConnection();

                var message = "Loan_Request_Code, Uploaded_By_Member_Staff_ID, Repayment_Amount is decimal, Status,Repayment_Status, Repayment_Date, Loan_Repayment_For, Repayment_Balance, Customer_Name, Service_Account headers are required before uploading" + ex.Message ?? ex.InnerException.Message;

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, message, false, message, message, Status.Ërror, StatusMgs.Error);
            }
        }

       //  Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Command, NPFDate
    }
}
