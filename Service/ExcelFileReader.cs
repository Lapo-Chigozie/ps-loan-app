using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.ModelDto;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Relational;
using OfficeOpenXml;
using System.Runtime.CompilerServices;

namespace LapoLoanWebApi.Service
{
    public class ExcelFileReader
    {
        private  DateTime StaticDate;
        private  int fileUploadCount = 0;

        private LapoLoanDBContext lapoLoanDB;

        private IConfiguration _configuration;

        public ExcelFileReader(IConfiguration _configuration
)
        {
            this._configuration = _configuration;
            this.lapoLoanDB =  new LapoLoanDBContext(this._configuration);
        }

        public  async void SaveMonthlyNames(List<FileName> files)
        {
            try
            {
              
                if (files.Count > 0)
                {
                    foreach (var worksheet in files)
                    {
                        var newFile = lapoLoanDB.ClientMonthlyNetPays.Where(s => s.Year == worksheet.Year && worksheet.Month == s.Month).FirstOrDefault();
                        if(newFile == null)
                        {
                            var NewNetPayMonth = new ClientMonthlyNetPay();
                            NewNetPayMonth.Year = worksheet.Year;
                            NewNetPayMonth.Month = worksheet.Month;
                            NewNetPayMonth.MonthName = worksheet.Name;
                            NewNetPayMonth.CreatedDate = 1;
                            lapoLoanDB.ClientMonthlyNetPays.Add(NewNetPayMonth);
                            await lapoLoanDB.SaveChangesAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        public  async Task<RespondMessageDto> ValidateExcelFile(List<ExcelWorksheet> worksheets, bool SaveDatabase = true)
        {
            try
            {
              
                if (worksheets.Count > 0)
                {
                    foreach (var worksheet in worksheets)
                    {
                        if (worksheet != null)
                        {
                            var rowCount = worksheet.Dimension.Rows;
                            for (int row = 3; row <= rowCount; row++)
                            {
                                try
                                {

                                    if(worksheet.Cells[row, 10].Value == null)
                                    {
                                        return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", null, Status.Ërror, StatusMgs.Error));
                                    }

                                    var NetPayfDate = worksheet.Cells[row, 10].Value;

                                    //   var  NetPayfDate  = NetPayfDate1[9].ToString().Replace(" ", "").Replace("12:00:00AM", "");

                                    if (NetPayfDate == null)
                                    {
                                        return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", null, Status.Ërror, StatusMgs.Error));
                                    }

                                    var NetPayfDateResult = Convert.ToDateTime((NetPayfDate.ToString()).ToString().Trim());

                                    if (NetPayfDateResult == null)
                                    {
                                        return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", null, Status.Ërror, StatusMgs.Error));
                                    }

                                    return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, null, null, Status.Success, StatusMgs.Success));
                                }
                                catch (Exception ex)
                                {
                                    return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", null, Status.Ërror, StatusMgs.Error));
                                }
                            }

                        }
                    }
                }

                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", null, Status.Ërror, StatusMgs.Error));
            }
            catch (Exception ex)
            {
                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error));
            }
        }

        public  async Task<RespondMessageDto> GetMonthlyNames(List<ExcelWorksheet> worksheets, bool SaveDatabase = true)
        {
            try
            {
                var FileNamelists = new List<FileName>();
                if (worksheets.Count > 0)
                {
                    foreach (var worksheet in worksheets)
                    {
                        if (worksheet != null)
                        {
                            var rowCount = worksheet.Dimension.Rows;
                            for (int row = 3; row <= rowCount; row ++)
                            {
                                try
                                {
                                    var NetPayfDate = worksheet.Cells[row, 10].Value;

                                //   var  NetPayfDate  = NetPayfDate1[9].ToString().Replace(" ", "").Replace("12:00:00AM", "");

                                    var NetPayfDateResult = Convert.ToDateTime((NetPayfDate.ToString()).ToString().Trim());

                                    FileNamelists.Add(new FileName() { Name = NetPayfDateResult.ToString("MMMM"), Month = NetPayfDateResult.Month, Year = NetPayfDateResult.Year, FileDate = NetPayfDateResult });
                                }
                                catch(Exception ex)
                                {

                                }
                            }

                        }
                    }

                    FileNamelists = FileNamelists.DistinctBy(x=>x.Name).Distinct().ToList();

                    if (SaveDatabase)
                    {
                       SaveMonthlyNames(FileNamelists);
                    }
                    
                    return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, FileNamelists, FileNamelists, Status.Success, StatusMgs.Success));
                }

                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "", null, Status.Ërror, StatusMgs.Error));
            }
            catch(Exception ex)
            {
                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error));
            }
        }

        public  async Task<RespondMessageDto> GetMonthlyName(ExcelWorksheet worksheet)
        {
            try
            {
                var DistintResult = new FileName();

                if (worksheet != null)
                {
                    if (worksheet != null)
                    {
                        var rowCount = worksheet.Dimension.Rows;
                        for (int row = 3; row <= rowCount; row ++)
                        {
                            var NetPayfDate = worksheet.Cells[row, 10].Value.ToString().Replace(" ", "").Replace("12:00:00AM", "");

                            var NetPayfDateResult = Convert.ToDateTime((NetPayfDate.ToString()).ToString().Trim());

                            DistintResult =  new FileName() { Name = NetPayfDateResult.ToString("MMMM"), Month = NetPayfDateResult.Month, Year = NetPayfDateResult.Year, FileDate = NetPayfDateResult };

                        }

                        return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, StatusMgs.Success, true, DistintResult, DistintResult, Status.Success, StatusMgs.Success));
                    }

                }

                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Some columes name in the excel file are missing, Make sure all those cell are available before uploading (Staff Id, Full Name, Bank Name, Account Number, Net Pay, Grade, Grade Step, Pay Group, Command, NetPay Date)", false, "", null, Status.Ërror, StatusMgs.Error));
            }
            catch (Exception ex)
            {
                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error));
            }
        }

        public  async Task<RespondMessageDto> ReadFileAgain(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment,  string fullPath)
        {

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            DateTime StaticDate = DateTime.Now;
            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullPath)))
            {
                // var firstSheet = package.Workbook.Worksheets.["First Sheet"];
                List<ExcelWorksheet> worksheets = package.Workbook.Worksheets.ToList();

                var ResultValidateExcelFile = await new ExcelFileReader(this._configuration).ValidateExcelFile(worksheets);
                if (ResultValidateExcelFile.IsActive == false)
                {
                    return ResultValidateExcelFile;
                }

                var ResultMonthlys = await new ExcelFileReader(this._configuration).GetMonthlyNames(worksheets);

                var ResultSaveMonthly = await new ExcelFileReader(this._configuration).SaveMonthly00000(_environment, worksheets, fullPath);

                return (ResultSaveMonthly);
            }
        }

        private static int SheetCount = 0;
        private static int row = 0; 
        private static bool IsRowHasError = false;
        public async  Task<RespondMessageDto> SaveMonthly00000(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, List<ExcelWorksheet> worksheets, string FileUrl)
        {
            try
            {
                if (worksheets.Count > 0)
                {
                    string Staff_Id = "";

                    var SheetsCount = worksheets.Count;

                    for (int row1 = 0; row1 <= SheetsCount; row1 ++)
                    {
                        var Result = new ClientNetPayINFO();

                        SheetCount = row1;

                        if (worksheets[SheetCount] == null || worksheets[SheetCount].Dimension == null)
                        {
                            SheetCount = SheetCount - 1;

                            row1 = SheetCount;
                            IsRowHasError = true;
                            await ReadFileAgain(_environment, FileUrl);
                        }
                        else
                        {
                            IsRowHasError = false;
                            SheetCount = row1;
                        }

                        var rowCount = worksheets[SheetCount].Dimension.Rows;

                        for (int SheetRowCount = 3; SheetRowCount <= rowCount; SheetRowCount ++)
                        {
                            row = SheetRowCount;

                            Result = new ClientNetPayINFO();

                            if (IsRowHasError)
                            {
                                IsRowHasError = true;

                                row = row - 1;

                                SheetRowCount = row;
                                await ReadFileAgain(_environment, FileUrl);
                            }
                            else
                            {
                                IsRowHasError = false;
                                row = SheetRowCount;
                            }

                            //  var nfDate1 = worksheet.Cells[row, 9].Value.ToString().Replace(" ", "").Replace("12:00:00AM", "");
                            //  var NPFDate = Convert.ToDateTime((nfDate1.ToString()).ToString().Trim());

                            try 
                            {
                                Staff_Id = (worksheets[SheetCount].Cells[row, 1].Value).ToString().Replace(" ", "");
                            }
                            catch(Exception xx)
                            {
                                Staff_Id = "";
                            }

                            if (Staff_Id != null && !string.IsNullOrWhiteSpace(Staff_Id) && !string.IsNullOrEmpty(Staff_Id))
                            {    
                                try
                                {
                                    if (!string.IsNullOrWhiteSpace(Staff_Id) || !string.IsNullOrWhiteSpace(Staff_Id))
                                    {
                                        Result.Staff_Id = (worksheets[SheetCount].Cells[row, 1].Value).ToString().Replace(" ", "");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Staff ID is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Full_Name = (worksheets[SheetCount].Cells[row, 2].Value ?? string.Empty).ToString();
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Full Name is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Bank_Name = (worksheets[SheetCount].Cells[row, 3].Value ?? string.Empty).ToString();
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Bank Name is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Account_Number = Convert.ToString((worksheets[SheetCount].Cells[row, 4].Value).ToString()).Replace(" ", "");
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Account Number is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Net_Pay = Convert.ToDouble((worksheets[SheetCount].Cells[row, 5].Value ?? string.Empty).ToString());
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Net Pay is not digit character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Grade = (worksheets[SheetCount].Cells[row, 6].Value ?? string.Empty).ToString();
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Grade is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Grade_Step = (worksheets[SheetCount].Cells[row, 7].Value ?? string.Empty).ToString();
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Grade step is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Pay_Group = (worksheets[SheetCount].Cells[row, 8].Value ?? string.Empty).ToString();
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Grade step is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    Result.Command = (worksheets[SheetCount].Cells[row, 9].Value ?? string.Empty).ToString().Trim();
                                }
                                catch (Exception ex)
                                {
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Command step is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                try
                                {
                                    var nfDate = worksheets[SheetCount].Cells[row, 10].Value.ToString().Replace(" ", "").Replace("12:00:00AM", "");
                                    Result.NPFDate = Convert.ToDateTime((nfDate.ToString()).ToString().Trim());
                                    StaticDate = Result.NPFDate;
                                }
                                catch (Exception ex)
                                {
                                    Result.NPFDate = StaticDate;
                                    //return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "ome Client Net-Pay Date is Invalid in the excel file", false, "", null, Status.Ërror, StatusMgs.Error));
                                }

                                var uploadResult = await ValidateClientIPFNumberUpload(_environment, worksheets[SheetCount], Result);
                                if (!uploadResult.IsActive && !string.IsNullOrWhiteSpace(Staff_Id) && !string.IsNullOrEmpty(Staff_Id))
                                {
                                    return uploadResult;
                                }
                            }

                            if (row >= rowCount)
                            {
                                row = 0;
                            }
                        }
                    }

                    var subCount = fileUploadCount;
                    if (fileUploadCount > 0)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, subCount + "Customer Net-Pay was uploaded successful.", true, "", null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Customer Net-Pay was not uploaded successful.", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return (new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error));
            }
        }

        public   async Task<RespondMessageDto> ValidateClientIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ExcelWorksheet worksheet, ClientNetPayINFO clientNetPays)
        {
            try
            {
                var ResultClientIPFUpload = await ValidateClientIPFUpload(clientNetPays);
                if (ResultClientIPFUpload.IsActive == false)
                {
                    return ResultClientIPFUpload;
                }

                var MonthlyNamesResult = await GetMonthlyName(worksheet);

                var Monthly = (long)Convert.ToInt64(MonthlyNamesResult.DataLoad.Month);
                var Yearly = (long)Convert.ToInt64(MonthlyNamesResult.DataLoad.Year);

                var ClientMonthlyNetPay = await lapoLoanDB.ClientMonthlyNetPays.Where(x => x.Year == Yearly && x.Month.Value == Monthly).FirstOrDefaultAsync();

                if (ClientMonthlyNetPay != null)
                {
                    var AccountAva = await lapoLoanDB.Clients.Where(x => x.Pfnumber == clientNetPays.Staff_Id).AnyAsync();

                    if (AccountAva)
                    {
                        var Result900 = await CreateIPFNumberUpload(_environment, clientNetPays, (int)ClientMonthlyNetPay.Id, 0);
                        if (!Result900.IsActive)
                        {
                            return Result900;
                        }
                    }
                    else
                    {
                        var Result900 = await CreateClientIPFNumberUpload(_environment, clientNetPays, (int)ClientMonthlyNetPay.Id, 0);

                        if (!Result900.IsActive)
                        {
                            return Result900;
                        }
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Successful.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS NetPay created successful.", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public  async Task<RespondMessageDto> ValidateClientIPFUpload(ClientNetPayINFO clientNetPay)
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

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Successful.", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch(Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, "", null, Status.Ërror, StatusMgs.Error);
            }
        }

        public  async Task<RespondMessageDto> CreateClientIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay, int Month, int totalClinetFile)
        {
            try
            {
                var AccountAva = await lapoLoanDB.Clients.Where(x => x.Pfnumber == clientNetPay.Staff_Id).AnyAsync();

                var Result768 = await ValidateClientIPFUpload(clientNetPay);

                if (AccountAva == false && Result768.IsActive)
                {
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

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS account was created successful.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS NetPay created successful.", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public  async Task<RespondMessageDto> CreateIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay,int Month, int totalClinetFile)
        {
            try
            {
                var AccountAva = await lapoLoanDB.Clients.Where(x => x.Pfnumber == clientNetPay.Staff_Id && x.Status == StatusMgs.Success).FirstOrDefaultAsync();

                var Result768 = await ValidateClientIPFUpload(clientNetPay);

                if (AccountAva != null && Result768.IsActive)
                {
                    var AccountAvDate = await lapoLoanDB.ClientNetPays.Where(x => x.ClientId == AccountAva.Id && (clientNetPay.NPFDate >= x.Npfdate && clientNetPay.NPFDate <= x.Npfdate)).AnyAsync();

                    if (AccountAvDate == false && !string.IsNullOrWhiteSpace(clientNetPay.Staff_Id) && !string.IsNullOrEmpty(clientNetPay.Staff_Id))
                    {
                        var NewClientNetPay = new ClientNetPay()
                        {
                            ClientId = AccountAva.Id,
                            Command = clientNetPay.Command,
                            NetPay = (decimal)clientNetPay.Net_Pay,
                            GradeStep = clientNetPay.Grade_Step,
                            Grade = clientNetPay.Grade,
                            BankName = clientNetPay.Bank_Name,
                            BankAccountName = AccountAva.FullName,
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

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS NetPay created successful.", true, "", null, Status.Success, StatusMgs.Success);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS NetPay created successful.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Customer IPPIS NetPay created successful.", true, "", null, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private string LoadData(string myString)
        {
          return  myString
   .Replace("\\", String.Empty)
   .Replace("\"", String.Empty)
   .Replace("[", String.Empty)
   .Replace("]", String.Empty);
        }

    }

    public class FileName
    {
        public DateTime FileDate { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Name { get; set; }
    }
}
