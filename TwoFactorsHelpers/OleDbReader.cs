using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanScafFoldModel;
using LapoLoanWebApi.LoanScafFoldModel.SqlStoreProcedureCmmdHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Renci.SshNet;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlTypes;
using System.Xml;

namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class OleDbReader
    {
        private  long fileUploadCount = 0;

        private LapoLoanDBContext lapoLoanDB;

        private OleDbConnection excelConnection = null;
        private OleDbDataAdapter adapter = null;
        private System.Data.SqlClient.SqlConnection MsSQlConn = null;
        private ControllerBase ControllerBase { get; set; }
        private IConfiguration _configuration;

        public OleDbReader(ControllerBase controllerBase, IConfiguration _configuration
)
        {
            this._configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.ControllerBase = ControllerBase;
            this.excelConnection = new OleDbConnection();
            this.adapter = new OleDbDataAdapter();

            this.MsSQlConn = new System.Data.SqlClient.SqlConnection(new LapoDBConnectionStrings(_configuration).ConnectionString());
        }

        public event EventHandler<ClientNetPayINFO> onReaderClosed;

        public event EventHandler<ClientNetPayINFO> onValidatingExcelSheetNames;

        public event EventHandler<ClientNetPayINFO> onReadAllExcelSheetNames;

        public event EventHandler<ClientNetPayINFO> onReadErrorExcelSheetName;

        public event EventHandler<ClientNetPayINFO> onReadering;

        public event EventHandler<ClientNetPayINFO> onConnectionOpen;

        public  bool IsExcelConnectionOpened = false;
      
        private async Task<RespondMessageDto> ValidateClientNetPaysByDate(long ClientId , DateTime StartDate, DateTime EndDate)
        {
            System.Data.SqlClient.SqlDataReader SqlReader = null;
            try
            {
                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.ValidateClientNetPaysByDate, this.MsSQlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@STARTNPFDate", StartDate);
                sqlCmd.Parameters.AddWithValue("@ENDNPFDate", EndDate);
                sqlCmd.Parameters.AddWithValue("@ClientId", ClientId);

                try
                {
                    sqlCmd.CommandTimeout = 30000;
                    SqlReader =  sqlCmd.ExecuteReader();
                }
                catch(Exception ex)
                {
                   // await SqlReader.CloseAsync();
                  //  await SqlReader.DisposeAsync();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, null, null, Status.Success, StatusMgs.Success);
                }
                
                //var SqldataAdapter = new System.Data.SqlClient.SqlDataAdapter(sqlCmd);

                //SqldataAdapter.Fill(dataTable);
                //SqldataAdapter.Fill(dataSet);

                //dataTable = new DataTable();
                //dataTable.Load(await sqlCmd.ExecuteReaderAsync());

                //dataSet = new DataSet(); //conn is opened by dataadapter
                //SqldataAdapter.Fill(dataSet);

                if (SqlReader != null && SqlReader.HasRows)
                {
                    while (await SqlReader.ReadAsync())
                    {
                        var Id = Convert.ToUInt32(SqlReader["Id"].ToString());
                        if (Id > 0)
                        {

                            await sqlCmd.DisposeAsync();
                            await SqlReader.CloseAsync();
                            await SqlReader.DisposeAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, Id, Id, Status.Success, StatusMgs.Success);

                        }
                    }
                }

                await sqlCmd.DisposeAsync();
                await SqlReader.CloseAsync();
                await SqlReader.DisposeAsync();
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
               
                await SqlReader.CloseAsync();
                await SqlReader.DisposeAsync();
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
            catch(Exception ex)
            {
                return excelSheets;
            }
        }

        private void ConnectDbUrl(string fullPath)
        {
            string exdbUrl = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 8.0;HDR=YES;'";

            excelConnection.ConnectionString = exdbUrl;
        }

        public async Task<RespondMessageDto> LoadExcelData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string fullPath)
        {

            try
            {
                this.ConnectDbUrl(fullPath);
                this.OpenExcelConnection();

                DataTable dtTables = new DataTable();
               
                //to get the schema of the workbook.
                dtTables = excelConnection.GetSchema();

                //get the tables in the workbook
                
                dtTables = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                 stringsSHEETS = await this.ReadAllExcelSheetNames(dtTables);

                if(stringsSHEETS != null && stringsSHEETS.Count > 0)
                {
                    return await CollectAllDataSet(stringsSHEETS , this.SheetCount);
                }
                else if (stringsSHEETS == null || stringsSHEETS.Count <= 0)
                {
                    this.CloseExcelConnection();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel file must contains a sheets and 10 t0 11 columes in any sheet.", false, " ", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain atleast 10 t0 11 columes.", false, " ", null, Status.Ërror, StatusMgs.Error);
            }
            catch(Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "The excel sheets must contain atleast 10 t0 11 columes.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        private Task<RespondMessageDto> ThrowAnotherSheet()
        {
            SheetCount += 1;

            return this.CollectAllDataSet(stringsSHEETS, SheetCount);
        }

        private long SheetCount = 0;

        private async Task<RespondMessageDto> CollectAllDataSet(List<string> ExcelSheets, long SheetCountIn)
        {
            OleDbCommand cmd = null;
            DbDataReader dataReader = null;

            try
            {

                var sheet = stringsSHEETS[(int)SheetCountIn];
                var RespondMess = await ValidateAllSheetsByHeaders(ExcelSheets);
                if (RespondMess.IsActive == false)
                {
                    return RespondMess;
                }

                //prepare dataset from the tables in the workbook

                cmd = new OleDbCommand();
                cmd.Connection = excelConnection;
                cmd.CommandText = "Select * from [" + sheet + "]";

                try
                {
                    // cmd.ResetCommandTimeout();
                    cmd.CommandTimeout = 30;
                    dataReader = await cmd.ExecuteReaderAsync();
                }
                catch (Exception er)
                {
                    //await dataReader.CloseAsync();
                    //await dataReader.DisposeAsync();
                }

                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
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

                        try
                        {
                            var CurrentDate = Convert.ToDateTime(NetPayDate);
                        }
                        catch (Exception ex)
                        {

                        }

                        var NewExcelSheet = new ClientNetPayINFO()
                        {
                            Account_Number = AccountName,
                            Bank_Name = BankName.ToString(),
                            Command = Command.ToString(),
                            Full_Name = FullName.ToString(),
                            Grade = Grade.ToString(),
                            Grade_Step = Grade_Step.ToString(),
                            Net_Pay = (double)NetPay,
                            Staff_Id = StaffID.ToString(),
                            Pay_Group = Pay_Group.ToString(),
                            NPFDate = Convert.ToDateTime(NetPayDate)
                        };

                        var client = await lapoLoanDB.Clients.Where(x => x.Pfnumber == NewExcelSheet.Staff_Id).FirstOrDefaultAsync();

                        var UploadResult = await UploadExcelDataAsync(null, NewExcelSheet, client);
                        if (UploadResult != null)
                        {

                        }

                        //var countTotoalWorkingSheet1 = stringsSHEETS.Count - (int)((SheetCountIn + 1));
                        //if (countTotoalWorkingSheet1 > 0)
                        //{
                           
                        //}
                        //else
                        //{
                        //    //breakFlag = true;
                        //    //break;
                        //}
                    }
                }

                //DataTable dtItems = new DataTable();
                //dtItems.TableName = sheet;

                //adapter = new OleDbDataAdapter();
                //adapter.SelectCommand = cmd;

                //// adapter.FillSchema(ds
                //adapter.Fill(dtItems);
                //ds.Tables.Add(dtItems);

                var countTotoalWorkingSheet = stringsSHEETS.Count - (int)((SheetCountIn + 1));

                if (countTotoalWorkingSheet > 0)
                {
                    await dataReader.CloseAsync();
                    await dataReader.DisposeAsync();
                    await cmd.DisposeAsync();
                    await dataReader.CloseAsync();
                    await dataReader.DisposeAsync();
                    return await ThrowAnotherSheet();
                }
                else
                {
                    await dataReader.CloseAsync();
                    await dataReader.DisposeAsync();
                    await cmd.DisposeAsync();
                    await dataReader.CloseAsync();
                    await dataReader.DisposeAsync();
                    this.CloseExcelConnection();

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "All data has been uploaded successfully.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Note that not all data where uploaded.", false, "", null, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                await cmd.DisposeAsync();
                await dataReader.CloseAsync();
                await dataReader.DisposeAsync();
                this.CloseExcelConnection();
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Note that not all data where uploaded or " + ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

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
            catch(Exception ex)
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

        private  async Task<RespondMessageDto> CreateClientIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay, int Month, int totalClinetFile)
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
                await  lapoLoanDB.SaveChangesAsync();

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
        private  async Task<RespondMessageDto> CreateIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay, int Month, int totalClinetFile, Client client)
        {
            try
            {
               // var Result768 = await ValidateClientIPFUpload(clientNetPay);

                if (client != null /*&& Result768.IsActive*/)
                {
                    var IsValidDate = await ValidateClientNetPaysByDate(client.Id, clientNetPay.NPFDate , clientNetPay.NPFDate); 

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
                        await  lapoLoanDB.SaveChangesAsync();

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

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success,  "SubbatchDate - Correct excel file and columes", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        protected  RespondMessageDto GetSubbatchDate(ClientNetPayINFO clientNetPays)
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

                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "", null, Status.Ërror, StatusMgs.Error));
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
                var ResultClientIPFUpload =  this.ValidateClientIPFUpload(clientNetPays);
                if (ResultClientIPFUpload.IsActive == false)
                {
                    return ResultClientIPFUpload;
                }

                var MonthlyNamesResult =  this.GetSubbatchDate(clientNetPays);

                if(MonthlyNamesResult.IsActive == false)
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


                if (clientMonthlyNetPay != null)
                {
                    if (client != null)
                    {
                        if(client.Status == StatusMgs.Success || client.Status == StatusMgs.NotActive)
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

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Monthly NetPays not found.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Client Monthly NetPays not found.", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
   
    
    }
}
