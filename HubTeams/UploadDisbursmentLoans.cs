using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanNetPaysHelpers.NetPaysEnum;
using LapoLoanWebApi.LoanScafFoldModel;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Data.OleDb;

namespace LapoLoanWebApi.HubTeams
{
    public sealed class UploadDisbursmentLoans
    {
        private long fileUploadCount = 0;
        private OleDbConnection excelConnection =  new OleDbConnection();
        private OleDbDataAdapter adapter =  new OleDbDataAdapter();
        //private System.Data.SqlClient.SqlConnection MsSQlConn = null;
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

        private IConfiguration _configuration = null;

        public UploadDisbursmentLoans(ControllerBase controllerBase, IConfiguration _configuration)
        {
            this._configuration = _configuration;
            this.ControllerBase = controllerBase;
            this.lapoLoanDB = new LapoLoanDBContext(_configuration);
            this.e360AuthHttpClient = new E360AuthHttpClient(controllerBase, _configuration);

            this.lapoCipher01 = new LapoCipher01();

            this.excelConnection = new OleDbConnection();
            this.adapter = new OleDbDataAdapter();

            //this.MsSQlConn = new System.Data.SqlClient.SqlConnection(new LapoDBConnectionStrings(_configuration).ConnectionString());
        }

        private List<string> stringsSHEETS = new List<string>();

        private async void OpenExcelConnection()
        {
            try
            {
                this.IsExcelConnectionOpened = true;
                 this.excelConnection.Open();

                FileLogActivities.CallSevice("Disburstment Controller", "excel Connection Open", "None");

                if (this.excelConnection.State == System.Data.ConnectionState.Closed)
                {
                     this.excelConnection.Open();

                    FileLogActivities.CallSevice("Disburstment Controller", "excel Connection Open", "None");
                }

                if (this.excelConnection.State == System.Data.ConnectionState.Open)
                {
                    FileLogActivities.CallSevice("Disburstment Controller", "Checked Connection Open 1", "None");

                    // await this.MsSQlConn.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice("Disburstment Controller", "Open Excel Connection Error" + ex.Message ?? ex.InnerException.Message, "None");
            }

            return;
        }

        private async void CloseExcelConnection()
        {
            try
            {
                this.IsExcelConnectionOpened = false;

                this.excelConnection.Close();

                if (this.excelConnection.State == System.Data.ConnectionState.Open)
                {
                    await this.excelConnection.CloseAsync();
                }

                if (this.excelConnection.State == System.Data.ConnectionState.Closed)
                {
                    FileLogActivities.CallSevice("Disburstment Controller", "Checked Connection Closed 1", "None");
                }
            }
            catch(Exception ex)
            {
                FileLogActivities.CallSevice("Disburstment Controller ", "Open Excel Connection Error: " + ex.Message ?? ex.InnerException.Message, "None");
            }

            return;
        }

        private void ConnectDbUrl(string fullPath)
        {
            try
            {
                if (this.excelConnection == null)
                {
                    this.excelConnection = new OleDbConnection();
                }
                   

                string exdbUrl = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 8.0;HDR=YES;'";

                // exdbUrl = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source="
                //  + fullPath + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";

                // exdbUrl = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'";

                // exdbUrl  = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + "; Extended Properties='Excel 12.0 Xml;HDR=Yes;IMEX=2'";

               // exdbUrl = "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + fullPath + ";Extended Properties='Excel 12.0 XML;HDR=YES'";

                exdbUrl = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 8.0;HDR=YES;'";

                excelConnection.ConnectionString = exdbUrl;

                FileLogActivities.CallSevice("Disburstment Controller ", "Connection Db Url 1", "None");

                FileLogActivities.CallSevice("Disburstment Controller ", "Connection Db Url: (" + fullPath + ")", "None");
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice("Disburstment Controller ", "Bind Url Connection Error: " + ex.Message ?? ex.InnerException.Message, "None");
            }
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

                    FileLogActivities.CallSevice("Disburstment Controller", "Checked Connection Open 1", "None");

                    return excelSheets;
                }

                return excelSheets;
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice("Disburstment Controller", "Error: " + ex.Message ?? ex.InnerException.Message, "Error");

                return excelSheets;
            }
        }

        public async Task<RespondMessageDto> LoadExcelData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string fullPath)
        {
            try
            {
                FileLogActivities.CallSevice("Disburstment Controller", "Enter Excel Data Function", "None");

                this.ConnectDbUrl(fullPath);

                FileLogActivities.CallSevice("Disburstment Controller", "Enter Excel Data Function", "None");

                DataTable dtTables = new DataTable();

                //to get the schema of the workbook. // dtTables = excelConnection.GetSchema();  //get the tables in the workbook

                this.OpenExcelConnection();

                dtTables = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                this.CloseExcelConnection();
                stringsSHEETS = await this.ReadAllExcelSheetNames(dtTables);

                if (stringsSHEETS == null || stringsSHEETS.Count <= 0)
                {
                    this.CloseExcelConnection();

                    FileLogActivities.CallSevice("Disburstment Controller", "Sheet is less than 0", "None");

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "We couldn’t find any Excel sheets in the file your uploading.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

                if (stringsSHEETS != null && stringsSHEETS.Count > 0)
                {
                    FileLogActivities.CallSevice("Disburstment Controller", "Sheet is greater than 0", "None");

                    this.CloseExcelConnection();
                    //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Excel file must contain Maximum of 3 sheets before uploading.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

                //if (stringsSHEETS == null || stringsSHEETS.Count <= 0)
                //{
                //    this.CloseExcelConnection();
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel file must contains a sheets and 10 to 11 columns in any sheet.", false, " ", null, Status.Ërror, StatusMgs.Error);
                //}

                if (stringsSHEETS != null && stringsSHEETS.Count > 0)
                {
                    this.CloseExcelConnection();

                    FileLogActivities.CallSevice("Disburstment Controller", "Sheet is greater than 0", "None");

                    return await this.ImportFromExcelBulked(stringsSHEETS, stringsSHEETS.Count, fullPath);
                }

                this.CloseExcelConnection();

                FileLogActivities.CallSevice("Disburstment Controller", "Error: The excel sheets must contain at least 10 to 11 columns.", "None");

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain at least 10 to 11 columns.", false, " ", null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                this.CloseExcelConnection();

                FileLogActivities.CallSevice("Disburstment Controller", "Error: " + ex.Message ?? ex.InnerException.Message, "None");

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain at least 10 to 11 columns.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> ImportFromExcelBulked(List<string> ExcelSheets, int totalSheet, string Path)
        {
            try
            {
                FileLogActivities.CallSevice("Disburstment Controller", "Import From Excel Bulked", "None");

                DataTable dtExcel1 = new DataTable();
               
                dtExcel1.Columns.AddRange(new DataColumn[5]
                {
                        new DataColumn ("Request_Code", typeof(string)),
                        new DataColumn ("Status", typeof(string)),
                        new DataColumn ("Disbursement_Date", typeof(DateTime)),
                        new DataColumn ("Disbursement_Officer_Staff_ID", typeof(string)),
                        new DataColumn ("Disbursed_Amount", typeof(decimal))
                });

                this.ConnectDbUrl(Path);
                this.OpenExcelConnection();

                var DataSheetRows = this.excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows;

                if (DataSheetRows.Count > 0)
                {
                    string Sheet1 = DataSheetRows[0]["TABLE_NAME"].ToString();
                    string sqlText = string.Concat("Select * FROM [", Sheet1, "] where Request_Code is not null");

                    var oda1 = new OleDbDataAdapter(sqlText, this.excelConnection);
                    oda1.Fill(dtExcel1);

                    this.CloseExcelConnection();

                    FileLogActivities.CallSevice("Disburstment Controller", "Close Excel Connection Has Sheet 1", "None");

                    this.Loop1Stage = NetPaysEnum.HasSheet1;

                    return await SaveAllNetPaysAsync(dtExcel1);
                }

                this.CloseExcelConnection();

                FileLogActivities.CallSevice("Disburstment Controller", "Close Excel Connection", "None");

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "", false, "", "", Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                this.CloseExcelConnection();

                var message = "Note that not all data where uploaded. Issue from: " + ex.Message ?? ex.InnerException.Message;

                FileLogActivities.CallSevice("Disburstment Controller", message, "None");

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, message, false, message, message, Status.Ërror, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> SaveAllNetPaysAsync(DataTable dataTable1)
        {
            try
            {
                FileLogActivities.CallSevice("Disburstment Controller", "SaveAllNetPaysAsync", "None");

                if (dataTable1 != null && dataTable1.Rows.Count > 2000)
                {
                    string message = dataTable1.Rows.Count + " Data rows is much on Sheet 1, Maximum data must be below or equal to 2000.";

                    FileLogActivities.CallSevice("Disburstment Controller", message, "None");
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, "", null, Status.Success, StatusMgs.Success);
                }

                if (this.Loop1Stage == NetPaysEnum.HasSheet1)  // Takes 4 mins to bind Data Table,  4 * 3 = 16mins for 3 Data Tables
                {
                    this.Loop1Stage = NetPaysEnum.StartSheet;
                    
                    // Task.Run(() =>.Wait());

                    FileLogActivities.CallSevice("Disburstment Controller", "Takes 4 mins to bind Data Table,  4 * 3 = 16mins for 3 Data Tables", "None");
                    return await this.SaveSheet1NetPaysAsync(dataTable1);
                }

                // Thread.Sleep(dataTable1.Rows.Count);

                FileLogActivities.CallSevice("Disburstment Controller", "Takes 4 mins to bind Data Table,  4 * 3 = 16mins for 3 Data Tables", "None");

                return await this.SaveSheet1NetPaysAsync(dataTable1);
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice("Disburstment Controller", ex.Message ?? ex.InnerException.Message, "None");

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

        private async Task<RespondMessageDto> SaveSheet1NetPaysAsync(DataTable dataTable1)
        {
            try
            {
                FileLogActivities.CallSevice("Disburstment Controller", "SaveSheet1NetPaysAsync", "None");

                var ListApprovedLoans =  new List<UploadDisbursmentLoanModel>();
                this.TotalSheet1 = dataTable1.Rows.Count;

                foreach (DataRow row in dataTable1.Rows)
                {
                    try
                    {
                        var Request_Code = row["Request_Code"].ToString();
                        var Status = row["Status"].ToString();
                        var Disbursement_Date = row["Disbursement_Date"].ToString();
                        var Disbursement_Officer_Staff_ID = row["Disbursement_Officer_Staff_ID"].ToString();
                        var Disbursed_Amount = row["Disbursed_Amount"].ToString();

                        var NewExcelSheet = new UploadDisbursmentLoanModel()
                        {
                            Request_Code = Request_Code,
                            Disbursement_Officer_Staff_ID = Disbursement_Officer_Staff_ID,
                            Status = Status
                        };

                        try
                        {
                            NewExcelSheet.Disbursed_Amount = Convert.ToDouble(Disbursed_Amount);
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            NewExcelSheet.Disbursement_Date = Convert.ToDateTime(Disbursement_Date);
                        }
                        catch (Exception ex)
                        {

                        }

                        ListApprovedLoans.Add(NewExcelSheet);

                        TotalSheet1Counting += 1;

                        FileLogActivities.CallSevice("Disburstment Controller", "Loop Data Table: Added Successfully", "None");
                    }
                    catch(Exception ex)
                    {
                        FileLogActivities.CallSevice("Disburstment Controller", "Loop Data Table: " + ex.Message ?? ex.InnerException.Message, "None");
                    }
                }

                dataTable1 = null;

                FileLogActivities.CallSevice("Disburstment Controller", "Close Excel Connection List Approved Loans", "None");

                FileLogActivities.CallSevice("Disburstment Controller", "Close Excel Connection List Approved Loans: " + JsonConvert.SerializeObject(ListApprovedLoans), "None");

                this.CloseExcelConnection();

                this.Loop1Stage = NetPaysEnum.Finished;

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, ListApprovedLoans, ListApprovedLoans, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                dataTable1 = null;

                this.Loop1Stage = NetPaysEnum.HasError;
                this.CloseExcelConnection();

                FileLogActivities.CallSevice("Disburstment Controller", "Exception Disburstment: " + ex.Message ?? ex.InnerException.Message, "None");

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex.Message ?? ex.InnerException.Message, ex.Message ?? ex.InnerException.Message, Status.Ërror, StatusMgs.Error);
            }
        }

        //Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Command, NPFDate
    }
}
