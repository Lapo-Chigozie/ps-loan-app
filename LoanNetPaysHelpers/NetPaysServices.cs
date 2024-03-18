using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanNetPaysHelpers.NetPaysUploadSimulationManagerHelper;
using LapoLoanWebApi.LoanScafFoldModel;
using LapoLoanWebApi.LoanScafFoldModel.SqlStoreProcedureCmmdHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace LapoLoanWebApi.LoanNetPaysHelpers
{
    public class NetPaysServices
    {
        private long fileUploadCount = 0;
        private NetPaysUploadSimulationManager uploadSimulationManager;
        private LapoLoanDBContext lapoLoanDB ;
        private OleDbConnection excelConnection = null;
        private OleDbDataAdapter adapter = null;
        private System.Data.SqlClient.SqlConnection MsSQlConn = null;
        private ControllerBase ControllerBase { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }
        private E360AuthHttpClient e360AuthHttpClient { get; set; }


        public event EventHandler<ClientNetPayINFO> onReaderClosed;

        public event EventHandler<ClientNetPayINFO> onValidatingExcelSheetNames;

        public event EventHandler<ClientNetPayINFO> onReadAllExcelSheetNames;

        public event EventHandler<ClientNetPayINFO> onReadErrorExcelSheetName;

        public event EventHandler<ClientNetPayINFO> onReadering;

        public event EventHandler<ClientNetPayINFO> onConnectionOpen;

        public bool IsExcelConnectionOpened = false;

        private IConfiguration _configuration;

        public NetPaysServices(ControllerBase controllerBase, IConfiguration configuration)
        {
            this._configuration = configuration;
            this._configuration = configuration;
            this.ControllerBase = controllerBase;
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.e360AuthHttpClient = new E360AuthHttpClient(controllerBase, configuration);

            this.lapoCipher01 = new LapoCipher01();

            this.excelConnection = new OleDbConnection();
            this.adapter = new OleDbDataAdapter();

            this.uploadSimulationManager = new NetPaysUploadSimulationManager();
            this.MsSQlConn = new System.Data.SqlClient.SqlConnection(new LapoDBConnectionStrings(this._configuration).ConnectionString());
        }

        private async Task<RespondMessageDto> ValidateClientNetPaysByDate(long ClientId, DateTime StartDate, DateTime EndDate)
        {
            // System.Data.SqlClient.SqlDataReader SqlReader = null;
            try
            {
                //this.OpenExcelConnection();
                //var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.ValidateClientNetPaysByDate, this.MsSQlConn);
                //sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                //sqlCmd.Parameters.AddWithValue("@STARTNPFDate", StartDate);
                //sqlCmd.Parameters.AddWithValue("@ENDNPFDate", EndDate);
                //sqlCmd.Parameters.AddWithValue("@ClientId", ClientId);

                //try
                //{
                //    sqlCmd.CommandTimeout = 30000;
                //    SqlReader = sqlCmd.ExecuteReader();
                //}
                //catch (Exception ex)
                //{
                //    this.CloseExcelConnection();
                //    await SqlReader.CloseAsync();
                //     await SqlReader.DisposeAsync();
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, null, null, Status.Success, StatusMgs.Success);
                //}

                //var SqldataAdapter = new System.Data.SqlClient.SqlDataAdapter(sqlCmd);

                //SqldataAdapter.Fill(dataTable);
                //SqldataAdapter.Fill(dataSet);

                //dataTable = new DataTable();
                //dataTable.Load(await sqlCmd.ExecuteReaderAsync());

                //dataSet = new DataSet(); //conn is opened by dataadapter
                //SqldataAdapter.Fill(dataSet);

                //if (SqlReader != null && SqlReader.HasRows)
                //{
                //    while (await SqlReader.ReadAsync())
                //    {
                //        var Id = Convert.ToUInt32(SqlReader["Id"].ToString());
                //        if (Id > 0)
                //        {
                //            this.CloseExcelConnection();
                //            await sqlCmd.DisposeAsync();
                //            await SqlReader.CloseAsync();
                //            await SqlReader.DisposeAsync();
                //            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, Id, Id, Status.Success, StatusMgs.Success);

                //        }
                //    }
                //}

                var NetPayExit = await lapoLoanDB.ClientNetPays.Where(x => x.ClientId == ClientId && x.Npfdate >= StartDate && x.Npfdate <= EndDate).ToListAsync();

                if(NetPayExit.Count > 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, "", "", Status.Success, StatusMgs.Success);
                }

                this.CloseExcelConnection();
                // await sqlCmd.DisposeAsync();
               // await SqlReader.CloseAsync();
               //  await SqlReader.DisposeAsync();
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                this.CloseExcelConnection();
                //await SqlReader.CloseAsync();
                //await SqlReader.DisposeAsync();
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, true, null, null, Status.Failed, StatusMgs.Error);
            }
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

                if (stringsSHEETS != null && stringsSHEETS.Count  <= 0)
                {
                    this.CloseExcelConnection();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "We couldn’t find any Excel sheets in the file your uploading.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

                if (stringsSHEETS != null && stringsSHEETS.Count > 1)
                {
                    this.CloseExcelConnection();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Excel file must contain Maximum of 3 sheets before uploading.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

                if (stringsSHEETS == null || stringsSHEETS.Count <= 0)
                {
                    this.CloseExcelConnection();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel file must contains a sheets and 10 t0 11 columns in any sheet.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

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
                DataTable dtExcel2 = new DataTable();
                DataTable dtExcel3 = new DataTable();

                dtExcel1.Columns.AddRange(new DataColumn[10]
                {
                        new DataColumn ("Staff_Id", typeof(string)),
                        new DataColumn ("Full_Name", typeof(string)),
                        new DataColumn ("Bank_Name", typeof(string)),
                        new DataColumn ("Account_Number", typeof(string)),
                        new DataColumn ("Net_Pay", typeof(decimal)),
                        new DataColumn ("Grade", typeof(string)),
                        new DataColumn ("Grade_Step", typeof(long)),
                        new DataColumn ("Pay_Group", typeof(string)),
                         new DataColumn ("Command", typeof(string)),
                        new DataColumn ("NPF_Date", typeof(string))
                });

                dtExcel2.Columns.AddRange(new DataColumn[10]
                {
                         new DataColumn ("Staff_Id", typeof(string)),
                        new DataColumn ("Full_Name", typeof(string)),
                        new DataColumn ("Bank_Name", typeof(string)),
                        new DataColumn ("Account_Number", typeof(string)),
                        new DataColumn ("Net_Pay", typeof(decimal)),
                        new DataColumn ("Grade", typeof(string)),
                        new DataColumn ("Grade_Step", typeof(long)),
                        new DataColumn ("Pay_Group", typeof(string)),
                         new DataColumn ("Command", typeof(string)),
                        new DataColumn ("NPF_Date", typeof(string))
                });

                dtExcel3.Columns.AddRange(new DataColumn[10]
                {
                        new DataColumn ("Staff_Id", typeof(string)),
                        new DataColumn ("Full_Name", typeof(string)),
                        new DataColumn ("Bank_Name", typeof(string)),
                        new DataColumn ("Account_Number", typeof(string)),
                        new DataColumn ("Net_Pay", typeof(decimal)),
                        new DataColumn ("Grade", typeof(string)),
                        new DataColumn ("Grade_Step", typeof(long)),
                        new DataColumn ("Pay_Group", typeof(string)),
                         new DataColumn ("Command", typeof(string)),
                        new DataColumn ("NPF_Date", typeof(string))
                });

                //DataColumn Ref_No = new DataColumn("Trans_Ref_Number", typeof(String));
                //DataColumn DtCreated = new DataColumn("DateCreated", typeof(DateTime));
                //Ref_No.DefaultValue = TransNumb;
                //DtCreated.DefaultValue = DateTime.Now;
                //dtExcel1.Columns.Add(Ref_No);
                //dtExcel1.Columns.Add(DtCreated);

                // string formated_ConString = string.Format(_ConnectionString_Excel7, excelPath);

                this.OpenExcelConnection();
                var DataSheetRows = this.excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows;

                if(DataSheetRows.Count > 0)
                {
                    string Sheet1 = DataSheetRows[0]["TABLE_NAME"].ToString();
                    string sqlText = string.Concat("Select * FROM [", Sheet1, "] where Staff_Id is not null");
                    var oda1 = new OleDbDataAdapter(sqlText, this.excelConnection);
                    oda1.Fill(dtExcel1);
                    this.Loop1Stage = NetPaysEnum.NetPaysEnum.HasSheet1;
                    this.CloseExcelConnection();

                    if (DataSheetRows.Count > 1 && DataSheetRows[1] != null)
                    {
                        this.OpenExcelConnection();
                        string Sheet2 = DataSheetRows[1]["TABLE_NAME"].ToString();
                        string sqlText2 = string.Concat("Select * FROM [", Sheet2, "] where Staff_Id is not null");
                        var oda2 = new OleDbDataAdapter(sqlText2, this.excelConnection);
                        oda2.Fill(dtExcel2);
                        this.Loop2Stage = NetPaysEnum.NetPaysEnum.HasSheet2;
                        this.CloseExcelConnection();
                    }

                    if (DataSheetRows.Count > 2 && DataSheetRows[2]!=null)
                    {
                        this.OpenExcelConnection();
                        string Sheet3 = DataSheetRows[2]["TABLE_NAME"].ToString();
                        string sqlText3 = string.Concat("Select * FROM [", Sheet3, "] where Staff_Id is not null");
                        var oda3 = new OleDbDataAdapter(sqlText3, this.excelConnection);
                        oda3.Fill(dtExcel3);
                        this.Loop3Stage = NetPaysEnum.NetPaysEnum.HasSheet3;
                        this.CloseExcelConnection();
                    }
                      
                    var UploadResult = await SaveAllNetPaysAsync(dtExcel1, dtExcel2, dtExcel3, DateTime.Now);

                    return UploadResult;
                }

                this.CloseExcelConnection();

                var message = "Customers Monthly NetPays was not uploaded. ";

                if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                }

                if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                }

                if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 3. check and correct where you have invalid date in NPF_Date -column in sheet 3.";
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, message, false, message, message, Status.Ërror, StatusMgs.Error);

            }
            catch (Exception ex)
            {
                this.CloseExcelConnection();

                var message = "Note that not all data where uploaded. Issue from: " + ex.Message ?? ex.InnerException.Message;

                if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                }

                if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                }

                if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 3. check and correct where you have invalid date in NPF_Date -column in sheet 3.";
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, message, false, message, message, Status.Ërror, StatusMgs.Error);
            }

            //return (Rslt) ? await rRes.ResponderDefault("DF1001SUC", httpContextAccessor.HttpContext) : await rRes.ResponderDefault("DF1001NOR", httpContextAccessor.HttpContext);
        }

        public async Task<RespondMessageDto> SaveAllNetPaysAsync(DataTable dataTable1, DataTable dataTable2, DataTable dataTable3, DateTime StartUploadTime)
        {
            try
            {
                if(dataTable1 != null && dataTable1.Rows.Count > 50002)
                {
                    string message = dataTable1.Rows.Count + " rows is much on Sheet 3, Maximum data must be below or equal to 5000.";
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, "", null, Status.Success, StatusMgs.Success);
                }

                if (dataTable2 != null && dataTable2.Rows.Count > 50002)
                {
                    string message = dataTable2.Rows.Count + " rows is much on Sheet 2, Maximum data must be below or equal to 5000.";
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, "", null, Status.Success, StatusMgs.Success);
                }

                if (dataTable3 != null && dataTable3.Rows.Count > 50002)
                {
                    string message = dataTable3.Rows.Count + " rows is much on Sheet 3, Maximum data must be below or equal to 5000.";
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, "", null, Status.Success, StatusMgs.Success);
                }

                if (this.Loop1Stage == NetPaysEnum.NetPaysEnum.HasSheet1)  // Takes 4 mins to bind Data Table,  4 * 3 = 16mins for 3 Data Tables
                {
                    StartUploadTime = DateTime.Now;
                    this.Loop1Stage = NetPaysEnum.NetPaysEnum.StartSheet;
                    Task.Run(() => this.SaveSheet1NetPaysAsync(dataTable1).Wait());
                   
                }

                if (this.Loop2Stage == NetPaysEnum.NetPaysEnum.HasSheet2)
                {
                    this.Loop2Stage = NetPaysEnum.NetPaysEnum.StartSheet;
                    Task.Run(() => this.SaveSheet2NetPaysAsync(dataTable2).Wait());
                }

                if (this.Loop3Stage == NetPaysEnum.NetPaysEnum.HasSheet3)
                {
                    this.Loop3Stage = NetPaysEnum.NetPaysEnum.StartSheet;
                    Task.Run(() => this.SaveSheet3NetPaysAsync(dataTable3).Wait());
                }

                // this.uploadSimulationManager.StartSimulations(dataTable1, dataTable2, dataTable3).Wait();
                Thread.Sleep(dataTable1.Rows.Count);

                if (this.Loop1Stage ==  NetPaysEnum.NetPaysEnum.HasError || this.Loop2Stage == NetPaysEnum.NetPaysEnum.HasError || this.Loop3Stage == NetPaysEnum.NetPaysEnum.HasError)
                {
                    dataTable1 = null;
                    dataTable2 = null;
                    dataTable3 = null;
                    this.CloseExcelConnection();

                    var message = "No excel sheets was uploaded, Before choosing excel file, Make sure all this column (Staff_Id, Full_Name, Bank_Name, Account_Number, Net_Pay, Grade, Grade_Step, Pay_Group, Command, NPF_Date) are exiting and well renamed as it is here in each excel file sheets.";

                    if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                    }

                    if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                    }

                    if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 3. check and correct where you have invalid date in NPF_Date column in sheet 3.";
                    }

                    var UploadedFinishTime = DateTime.Now - StartUploadTime;

                    message.Replace("\n", Environment.NewLine);
                    message += " " + UploadedFinishTime.TotalHours + "Hrs, " + UploadedFinishTime.TotalMinutes + "Mins";

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, message, message, Status.Success, StatusMgs.Success);
                }

                else if (this.Loop1Stage == NetPaysEnum.NetPaysEnum.Finished && this.Loop2Stage == NetPaysEnum.NetPaysEnum.None && this.Loop3Stage == NetPaysEnum.NetPaysEnum.None)
                {
                    dataTable1 = null;
                    dataTable2 = null;
                    dataTable3 = null;
                    this.CloseExcelConnection();

                    var message = "Excel sheets (1) has been uploaded successfully, Before choosing excel file, Make sure all this column (Staff_Id, Full_Name, Bank_Name, Account_Number, Net_Pay, Grade, Grade_Step, Pay_Group, Command, NPF_Date) are exiting and well renamed as it is here in each excel file sheets.";

                    if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                    }

                    if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                    }

                    if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 3. check and correct where you have invalid date in NPF_Date column in sheet 3.";
                    }

                    var UploadedFinishTime = DateTime.Now - StartUploadTime;

                    message.Replace("\n", Environment.NewLine);
                    message += " " + UploadedFinishTime.TotalHours + "Hrs, " + UploadedFinishTime.TotalMinutes + "Mins";

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, message, message, Status.Success, StatusMgs.Success);
                }

                else if (this.Loop2Stage == NetPaysEnum.NetPaysEnum.Finished || this.Loop1Stage == NetPaysEnum.NetPaysEnum.HasError || this.Loop3Stage == NetPaysEnum.NetPaysEnum.HasError)
                {
                    dataTable1 = null;
                    dataTable2 = null;
                    dataTable3 = null;
                    this.CloseExcelConnection();

                    var message = "Excel sheets (2) has been uploaded successfully, Before choosing excel file, Make sure all this columes (Staff_Id, Full_Name, Bank_Name, Account_Number, Net_Pay, Grade, Grade_Step, Pay_Group, Command, NPF_Date) are exiting and well renamed as it is here in each excel file sheets.";

                    if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                    }

                    if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                    }

                    if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 3. check and correct where you have invalid date in NPF_Date -column in sheet 3.";
                    }

                    var UploadedFinishTime = DateTime.Now - StartUploadTime;

                    message.Replace("\n", Environment.NewLine);
                    message += " " + UploadedFinishTime.TotalHours + "Hrs, " + UploadedFinishTime.TotalMinutes + "Mins";

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, message, message, Status.Success, StatusMgs.Success);
                }

                else if (this.Loop3Stage == NetPaysEnum.NetPaysEnum.Finished || this.Loop1Stage == NetPaysEnum.NetPaysEnum.HasError || this.Loop2Stage == NetPaysEnum.NetPaysEnum.HasError)
                {
                    dataTable1 = null;
                    dataTable2 = null;
                    dataTable3 = null;
                    this.CloseExcelConnection();

                    var message = "Excel sheets (3) has been uploaded successfully, Before choosing excel file, Make sure all this column (Staff_Id, Full_Name, Bank_Name, Account_Number, Net_Pay, Grade, Grade_Step, Pay_Group, Command, NPF_Date) are exiting and well renamed as it is here in each excel file sheets.";

                    if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                    }

                    if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                    }

                    if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                    {
                        message.Replace("\n", Environment.NewLine);
                        message += " 3. check and correct where you have invalid date in NPF_Date -column in sheet 3.";
                    }

                    var UploadedFinishTime = DateTime.Now - StartUploadTime;

                    message.Replace("\n", Environment.NewLine);
                    message += " " + UploadedFinishTime.TotalHours + "Hrs, " + UploadedFinishTime.TotalMinutes + "Mins";

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, false, message, message, Status.Success, StatusMgs.Success);
                }

                else if (this.Loop3Stage == NetPaysEnum.NetPaysEnum.Finished && this.Loop2Stage == NetPaysEnum.NetPaysEnum.Finished && this.Loop1Stage == NetPaysEnum.NetPaysEnum.Finished)
                {
                    if (this.TotalSheet1 == this.TotalSheet1Counting && this.TotalSheet2 == this.TotalSheet2Counting && this.TotalSheet3 == this.TotalSheet3Counting)
                    {
                        dataTable1 = null;
                        dataTable2 = null;
                        dataTable3 = null;

                        this.CloseExcelConnection();

                        var message = "All excel sheets has been uploaded successfully, Before choosing excel file, Make sure all this column (Staff_Id, Full_Name, Bank_Name, Account_Number, Net_Pay, Grade, Grade_Step, Pay_Group, Command, NPF_Date) are exiting and well renamed as it is here in each excel file sheets.";

                        if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                        {
                            message.Replace("\n", Environment.NewLine);
                            message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                        }

                        if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                        {
                            message.Replace("\n", Environment.NewLine);
                            message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                        }

                        if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                        {
                            message.Replace("\n", Environment.NewLine);
                            message += " 3. check and correct where you have invalid date in NPF_Date -column in sheet 3.";
                        }

                        var UploadedFinishTime = DateTime.Now - StartUploadTime;

                        message.Replace("\n", Environment.NewLine);
                        message += " " + UploadedFinishTime.TotalHours + "Hrs, " + UploadedFinishTime.TotalMinutes + "Mins";

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, message, true, message, message, Status.Success, StatusMgs.Success);
                    }
                }

                return await SaveAllNetPaysAsync(dataTable1,  dataTable2, dataTable3, StartUploadTime);

            }
            catch (Exception ex)
            {
                dataTable1 = null;
                dataTable2 = null;
                dataTable3 = null;

                this.CloseExcelConnection();

                var message = "Note that not all data where uploaded. Issue from: " + ex.Message ?? ex.InnerException.Message;

                if (this.Sheet1HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 1. check and correct where you have invalid date in NPF_Date column in sheet 1.";
                }

                if (this.Sheet2HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 2. check and correct where you have invalid date in NPF_Date column in sheet 2.";
                }

                if (this.Sheet3HasDateError == NetPaysEnum.NetPaysEnum.HasDateError)
                {
                    message.Replace("\n", Environment.NewLine);
                    message += " 3. check and correct where you have invalid date in NPF_Date -column in sheet 3.";
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, message, false, message, message, Status.Ërror, StatusMgs.Error);
            }
        }

        private NetPaysEnum.NetPaysEnum Sheet1HasDateError = NetPaysEnum.NetPaysEnum.None;
        private NetPaysEnum.NetPaysEnum Sheet2HasDateError = NetPaysEnum.NetPaysEnum.None;
        private NetPaysEnum.NetPaysEnum Sheet3HasDateError = NetPaysEnum.NetPaysEnum.None;

        private NetPaysEnum.NetPaysEnum Loop1Stage = NetPaysEnum.NetPaysEnum.None;
        private NetPaysEnum.NetPaysEnum Loop2Stage = NetPaysEnum.NetPaysEnum.None;
        private NetPaysEnum.NetPaysEnum Loop3Stage = NetPaysEnum.NetPaysEnum.None;

        private int TotalSheet1 = 0;
        private int TotalSheet1Counting = 0;

        private int TotalSheet2 = 0;
        private int TotalSheet2Counting = 0;

        private int TotalSheet3 = 0;
        private int TotalSheet3Counting = 0;

        public async Task SaveSheet3NetPaysAsync(DataTable dataTable3)
        {
            try
            {
                this.TotalSheet3 = dataTable3.Rows.Count;

                try
                {
                    foreach (DataRow row in dataTable3.Rows)
                    {

                        var StaffID = row["Staff_Id"].ToString();
                        var FullName = row["Full_Name"].ToString();
                        var BankName = row["Bank_Name"].ToString();
                        var AccountName = row["Account_Number"].ToString();
                        var NetPay = row["Net_Pay"].ToString();
                        var Grade = row["Grade"].ToString();
                        var Grade_Step = row["Grade_Step"].ToString();
                        var Pay_Group = row["Pay_Group"].ToString();
                        var Command = row["Command"].ToString();
                        var NetPayDate = row["NPF_Date"].ToString();

                        try
                        {
                            var CurrentDate = Convert.ToDateTime(NetPayDate);
                        }
                        catch (Exception ex)
                        {
                            this.Sheet3HasDateError = NetPaysEnum.NetPaysEnum.HasDateError;
                        }

                        var NewExcelSheet = new ClientNetPayINFO()
                        {
                            Account_Number = AccountName,
                            Bank_Name = BankName.ToString(),
                            Command = Command.ToString(),
                            Full_Name = FullName.ToString(),
                            Grade = Grade.ToString(),
                            Grade_Step = Grade_Step.ToString(),
                            Net_Pay = (double)Convert.ToDouble(NetPay),
                            Staff_Id = StaffID.ToString(),
                            Pay_Group = Pay_Group.ToString(),
                            NPFDate = Convert.ToDateTime(NetPayDate)
                        };

                        var client = await lapoLoanDB.Clients.Where(x => x.Pfnumber == NewExcelSheet.Staff_Id).FirstOrDefaultAsync();

                        this.TotalSheet3Counting += 1;
                        var UploadResult = await this.UploadExcelDataAsync(null, NewExcelSheet, client);
                      
                    }

                    dataTable3 = null;
                    this.CloseExcelConnection();
                    this.Loop3Stage = NetPaysEnum.NetPaysEnum.Finished;
                }
                catch (Exception exp1)
                {
                    dataTable3 = null;
                    this.CloseExcelConnection();
                    this.Loop3Stage = NetPaysEnum.NetPaysEnum.HasError;
                }
            }
            catch (Exception ex)
            {
                dataTable3 = null;
                this.Loop3Stage = NetPaysEnum.NetPaysEnum.HasError;
                this.CloseExcelConnection();
            }
        }

        public async Task SaveSheet2NetPaysAsync(DataTable dataTable2)
        {
            try
            {
                this.TotalSheet2 = dataTable2.Rows.Count;

                try
                {
                    foreach (DataRow row in dataTable2.Rows)
                    {
                        var StaffID = row["Staff_Id"].ToString();
                        var FullName = row["Full_Name"].ToString();
                        var BankName = row["Bank_Name"].ToString();
                        var AccountName = row["Account_Number"].ToString();
                        var NetPay = row["Net_Pay"].ToString();
                        var Grade = row["Grade"].ToString();
                        var Grade_Step = row["Grade_Step"].ToString();
                        var Pay_Group = row["Pay_Group"].ToString();
                        var Command = row["Command"].ToString();
                        var NetPayDate = row["NPF_Date"].ToString();
                        try
                        {
                            var CurrentDate = Convert.ToDateTime(NetPayDate);
                        }
                        catch (Exception ex)
                        {
                            this.Sheet2HasDateError = NetPaysEnum.NetPaysEnum.HasDateError;
                        }

                        var NewExcelSheet = new ClientNetPayINFO()
                        {
                            Account_Number = AccountName,
                            Bank_Name = BankName.ToString(),
                            Command = Command.ToString(),
                            Full_Name = FullName.ToString(),
                            Grade = Grade.ToString(),
                            Grade_Step = Grade_Step.ToString(),
                            Net_Pay = (double)Convert.ToDouble(NetPay),
                            Staff_Id = StaffID.ToString(),
                            Pay_Group = Pay_Group.ToString(),
                            NPFDate = Convert.ToDateTime(NetPayDate)
                        };

                        var client = await lapoLoanDB.Clients.Where(x => x.Pfnumber == NewExcelSheet.Staff_Id).FirstOrDefaultAsync();

                        this.TotalSheet2Counting += 1;
                        var UploadResult = await this.UploadExcelDataAsync(null, NewExcelSheet, client);

                    }

                    dataTable2 = null;
                    this.CloseExcelConnection();
                    this.Loop2Stage = NetPaysEnum.NetPaysEnum.Finished;
                }
                catch (Exception exp1)
                {
                    dataTable2 = null;
                    this.CloseExcelConnection();
                    this.Loop2Stage = NetPaysEnum.NetPaysEnum.HasError;
                }
            }
            catch (Exception ex)
            {
                dataTable2 = null;
                this.Loop2Stage = NetPaysEnum.NetPaysEnum.HasError;
                this.CloseExcelConnection();
            }
        }
        public async Task SaveSheet1NetPaysAsync(DataTable dataTable1)
        {
            try
            {
                this.TotalSheet1 = dataTable1.Rows.Count;

                try
                {
                    foreach (DataRow row in dataTable1.Rows)
                    {
                        var StaffID = row["Staff_Id"].ToString();
                        var FullName = row["Full_Name"].ToString();
                        var BankName = row["Bank_Name"].ToString();
                        var AccountName = row["Account_Number"].ToString();
                        var NetPay = row["Net_Pay"].ToString();
                        var Grade = row["Grade"].ToString();
                        var Grade_Step = row["Grade_Step"].ToString();
                        var Pay_Group = row["Pay_Group"].ToString();
                        var Command = row["Command"].ToString();
                        var NetPayDate = row["NPF_Date"].ToString();

                        try
                        {
                            var CurrentDate = DateTime.Parse(NetPayDate);
                        }
                        catch (Exception ex)
                        {
                            this.Sheet1HasDateError = NetPaysEnum.NetPaysEnum.HasDateError;
                        }

                        var NewExcelSheet = new ClientNetPayINFO()
                        {
                            Account_Number = AccountName,
                            Bank_Name = BankName.ToString(),
                            Command = Command.ToString(),
                            Full_Name = FullName.ToString(),
                            Grade = Grade.ToString(),
                            Grade_Step = Grade_Step.ToString(),
                            Net_Pay = (double)Convert.ToDouble(NetPay),
                            Staff_Id = StaffID.ToString(),
                            Pay_Group = Pay_Group.ToString(),
                            NPFDate = Convert.ToDateTime(NetPayDate)
                        };

                        var client = await lapoLoanDB.Clients.Where(x => x.Pfnumber == NewExcelSheet.Staff_Id).FirstOrDefaultAsync();

                        TotalSheet1Counting += 1;
                        var UploadResult = await this.UploadExcelDataAsync(null, NewExcelSheet, client);                   
                    }

                    dataTable1 = null;
                    this.CloseExcelConnection();
                    this.Loop1Stage = NetPaysEnum.NetPaysEnum.Finished;
                }
                catch (Exception exp1)
                {
                    dataTable1 = null;
                    this.CloseExcelConnection();
                    this.Loop1Stage = NetPaysEnum.NetPaysEnum.HasError;
                }
            }
            catch (Exception ex)
            {
                dataTable1 = null;
                this.Loop1Stage = NetPaysEnum.NetPaysEnum.HasError;
                this.CloseExcelConnection();
            }
        }


        // Next Line
        private async Task<RespondMessageDto> ValidateAllSheetsByHeaders(List<string> ExcelSheets)
        {

            try
            {
                var Result = new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, null, true, "", null, Status.Success, StatusMgs.Success);

                foreach (string sheet in ExcelSheets)
                {
                    Result = await ValidateAllSheetsByDetails(sheet);
                    if (Result.IsActive == false)
                    {
                        return Result;
                    }
                }

                return Result;
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> ValidateAllSheetsByDetails(string SheetName)
        {
            try
            {
                //prepare dataset from the tables in the workbook

                var cmd = new OleDbCommand();
                cmd.Connection = excelConnection;
                cmd.CommandText = "Select * from [" + SheetName + "]";

                var dataReader = await cmd.ExecuteReaderAsync();

                if (dataReader.HasRows)
                {

                    while (dataReader.Read())
                    {
                        try
                        {
                            var StaffID = dataReader.GetValue(0);
                            var FullName = dataReader.GetValue(1);
                            var BankName = dataReader.GetValue(2);
                            var AccountName = dataReader.GetValue(3).ToString();
                            var NetPay = dataReader.GetValue(4);
                            var Grade = dataReader.GetValue(5);
                            var Grade_Step = dataReader.GetValue(6);
                            var Pay_Group = dataReader.GetValue(7);
                            var Command = dataReader.GetValue(8);
                            var NetPayDate = dataReader.GetValue(9);
                            var CurrentDate = Convert.ToDateTime(NetPayDate);

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "The excel sheets contain atleast 10 t0 11 columes. Ok", true, "", null, Status.Success, StatusMgs.Success);
                        }
                        catch (Exception ex)
                        {
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain atleast 10 t0 11 columes.", false, "", null, Status.Failed, StatusMgs.Error);
                        }
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain atleast 10 t0 11 columes.", false, "", null, Status.Failed, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> CreateClientIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay, int Month, int totalClinetFile)
        {
            try
            {
                //  var Result768 = await ValidateClientIPFUpload(clientNetPay);

                var NewClient = new Client()
                {
                    FullName = clientNetPay.Full_Name,
                    Pfnumber = clientNetPay.Staff_Id,
                    Status = StatusMgs.Success,
                    CreatedAccountById = 34,
                    CreatedDate = DateTime.Now,
                    Npfdate = clientNetPay.NPFDate,
                };
                lapoLoanDB.Clients.Add(NewClient);
                await lapoLoanDB.SaveChangesAsync();

                var NewClient1 = new ClientNetPay()
                {
                    ClientId = NewClient.Id,
                    Command = clientNetPay.Command,
                    NetPay = (decimal)clientNetPay.Net_Pay,
                    GradeStep = clientNetPay.Grade_Step,
                    Grade = clientNetPay.Grade,
                    BankName = clientNetPay.Bank_Name,
                    BankAccountName = NewClient.FullName,
                    BankAccountNumber = clientNetPay.Account_Number.ToString(),
                    CreatedAccountById = 34,
                    Status = StatusMgs.Success,
                    CreatedDate = DateTime.Now,
                    Npfdate = clientNetPay.NPFDate,
                    ClientMonthlyNetPayId = Month
                };
                lapoLoanDB.ClientNetPays.Add(NewClient1);
                await lapoLoanDB.SaveChangesAsync();

                fileUploadCount += 1;

                //if (Result768.IsActive)
                //{
                // return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS account was created successful.", true, "", null, Status.Success, StatusMgs.Success);
                //}

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS NetPay created successful.", false, "", null, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
        private async Task<RespondMessageDto> CreateIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay, int Month, int totalClinetFile, Client client)
        {
            try
            {
                // var Result768 = await ValidateClientIPFUpload(clientNetPay);

                if (client != null /*&& Result768.IsActive*/)
                {
                    var IsValidDate = await ValidateClientNetPaysByDate(client.Id, clientNetPay.NPFDate, clientNetPay.NPFDate);

                    if (IsValidDate.IsActive == false && !string.IsNullOrWhiteSpace(clientNetPay.Staff_Id) && !string.IsNullOrEmpty(clientNetPay.Staff_Id))
                    {
                        var NewClientNetPay = new ClientNetPay()
                        {
                            ClientId = client.Id,
                            Command = clientNetPay.Command,
                            NetPay = (decimal)clientNetPay.Net_Pay,
                            GradeStep = clientNetPay.Grade_Step,
                            Grade = clientNetPay.Grade,
                            BankName = clientNetPay.Bank_Name,
                            BankAccountName = clientNetPay.Full_Name,
                            BankAccountNumber = clientNetPay.Account_Number.ToString(),
                            CreatedAccountById = 34,
                            Status = StatusMgs.Success,
                            CreatedDate = DateTime.Now,
                            Npfdate = clientNetPay.NPFDate,
                            ClientMonthlyNetPayId = Month,
                        };
                        lapoLoanDB.ClientNetPays.Add(NewClientNetPay);
                        await lapoLoanDB.SaveChangesAsync();

                        fileUploadCount += 1;

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS NetPay data saved successful.", true, "", null, Status.Success, StatusMgs.Success);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Validate ClientNet Pays By Date not successful.", false, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Client not found.", false, "", null, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private RespondMessageDto ValidateClientIPFUpload(ClientNetPayINFO clientNetPay)
        {
            try
            {
                if (clientNetPay == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid data along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrWhiteSpace(clientNetPay.Staff_Id) || string.IsNullOrEmpty(clientNetPay.Staff_Id) || string.IsNullOrWhiteSpace(clientNetPay.Net_Pay.ToString()) || string.IsNullOrEmpty(clientNetPay.Net_Pay.ToString()) || string.IsNullOrWhiteSpace(clientNetPay.Full_Name) || string.IsNullOrEmpty(clientNetPay.Full_Name) || string.IsNullOrWhiteSpace(clientNetPay.Bank_Name) || string.IsNullOrEmpty(clientNetPay.Bank_Name) || string.IsNullOrWhiteSpace(clientNetPay.Command) || string.IsNullOrEmpty(clientNetPay.Command) || string.IsNullOrWhiteSpace(clientNetPay.Full_Name) || string.IsNullOrEmpty(clientNetPay.Full_Name) || string.IsNullOrWhiteSpace(clientNetPay.Grade) || string.IsNullOrEmpty(clientNetPay.Grade) || string.IsNullOrWhiteSpace(clientNetPay.Grade_Step) || string.IsNullOrEmpty(clientNetPay.Grade_Step) || string.IsNullOrWhiteSpace(clientNetPay.NPFDate.ToString()) || string.IsNullOrEmpty(clientNetPay.NPFDate.ToString()) || string.IsNullOrWhiteSpace(clientNetPay.Staff_Id) || string.IsNullOrEmpty(clientNetPay.Staff_Id))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid data along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (clientNetPay.Net_Pay <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid net-pay along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (clientNetPay.NPFDate.Month > 28 && clientNetPay.NPFDate.Month < 31)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid date along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //DateTime Na; 
                //if (DateTime.TryParse(clientNetPay.NPFDate.ToString(),  Na))
                //{
                //    return false;
                //}

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "SubbatchDate - Correct excel file and columes", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        protected RespondMessageDto GetSubbatchDate(ClientNetPayINFO clientNetPays)
        {
            try
            {
                var DistintResult = new FileName();

                if (clientNetPays != null)
                {

                    var NetPayfDate = clientNetPays.NPFDate.ToString().Replace("12:00:00AM", "");

                    var NetPayfDateResult = Convert.ToDateTime((NetPayfDate.ToString()).ToString().Trim());

                    DistintResult = new FileName() { Name = NetPayfDateResult.ToString("MMMM"), Month = NetPayfDateResult.Month, Year = NetPayfDateResult.Year, FileDate = NetPayfDateResult };

                    return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "SubbatchDate - Correct excel file and columes", true, DistintResult, DistintResult, Status.Success, StatusMgs.Success));

                }

                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columns name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "", null, Status.Ërror, StatusMgs.Error));
            }
            catch (Exception ex)
            {
                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error));
            }
        }

        private async Task<RespondMessageDto> UploadExcelDataAsync(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPays, Client client)
        {
            try
            {

                ClientMonthlyNetPay clientMonthlyNetPay = null;

                var ResultClientIPFUpload = this.ValidateClientIPFUpload(clientNetPays);
                if (ResultClientIPFUpload.IsActive == false)
                {
                    return ResultClientIPFUpload;
                }

                var MonthlyNamesResult = this.GetSubbatchDate(clientNetPays);

                if (MonthlyNamesResult.IsActive == false)
                {
                    return MonthlyNamesResult;
                }

                var Monthly = (long)Convert.ToInt64(MonthlyNamesResult.DataLoad.Month);
                var Yearly = (long)Convert.ToInt64(MonthlyNamesResult.DataLoad.Year);

                try
                {
                    clientMonthlyNetPay = await lapoLoanDB.ClientMonthlyNetPays.Where(x => x.Year.Value == Yearly && x.Month.Value == Monthly).FirstOrDefaultAsync();
                }
                catch (Exception exx)
                {

                }

                if(clientMonthlyNetPay == null)
                {
                    var NetPayfDateResult = Convert.ToDateTime((clientNetPays.NPFDate.ToString()).ToString().Trim());

                    var newClientMonthlyNetPay = new ClientMonthlyNetPay()
                    {
                        CreatedDate = 1,
                        Year = Yearly,
                        Month = Monthly,
                        MonthName = NetPayfDateResult.ToString("MMMM"), 
                    };
                    lapoLoanDB.ClientMonthlyNetPays.Add(newClientMonthlyNetPay);
                    await lapoLoanDB.SaveChangesAsync();

                    try
                    {
                        clientMonthlyNetPay = await lapoLoanDB.ClientMonthlyNetPays.Where(x => x.Year.Value == Yearly && x.Month.Value == Monthly).FirstOrDefaultAsync();
                    }
                    catch (Exception exx)
                    {

                    }
                }

             

                if (clientMonthlyNetPay != null)
                {
                    if (client != null)
                    {
                        if (client.Status == StatusMgs.Success || client.Status == StatusMgs.NotActive)
                        {
                            var Result900 = await this.CreateIPFNumberUpload(_environment, clientNetPays, (int)clientMonthlyNetPay.Id, 0, client);

                            return Result900;
                        }

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Client not found.", true, "", null, Status.Success, StatusMgs.Success);
                    }
                    else
                    {
                        var Result900 = await this.CreateClientIPFNumberUpload(_environment, clientNetPays, (int)clientMonthlyNetPay.Id, 0);

                        return Result900;
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customers Monthly NetPays not found.", true, "", null, Status.Success, StatusMgs.Success);
                }
              

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customers Monthly NetPays not found.", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        //Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Command, NPFDate
    }
}
