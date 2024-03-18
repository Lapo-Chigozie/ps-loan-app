using ExcelDataReader;
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.Service;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using Microsoft.EntityFrameworkCore;
using LapoLoanWebApi.EnAndDeHelper;
using MySql.Data;
using MySqlX.XDevAPI.Common;
using Microsoft.Extensions.Hosting;
using LapoLoanWebApi.TwoFactorsHelpers;
using LapoLoanWebApi.LoanNetPaysHelpers;
using Microsoft.AspNetCore.WebUtilities;
using LapoLoanWebApi.LoanNetPaysHelpers.NetPaysModel;
using Microsoft.Net.Http.Headers;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
//using System.Web.Http.Cors;

namespace LapoLoanWebApi.Controllers
{

    [EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
    [ApiController]
    // [Route("api/[controller]")]
    //[Route("[controller]")]
    [Route("api/[controller]/[action]")]
    public class FileUploaderController : ControllerBase
    {
        private LapoLoanDBContext lapoLoanDB = null;
        private readonly IConfiguration Configuration = null;

        private OleDbReader OleDbReader;
        private NetPaysServices NetPaysServices;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public FileUploaderController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            Environment = _environment;
            this.Configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            OleDbReader = new OleDbReader(this, this.Configuration);
            this.NetPaysServices = new NetPaysServices(this, this.Configuration);
        }

        [HttpPost]
        //[Authorize]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("UploadImageOld")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null)
                return Ok(new { success = false, message = "You have to attach a file" });

            var fileName = file.FileName;
            // var extension = Path.GetExtension(fileName);

            // Add validations here...

            var localPath = $"{Path.Combine(System.AppContext.BaseDirectory, "upload")}\\{fileName}";

            // Create dir if not exists
            Directory.CreateDirectory(Path.Combine(System.AppContext.BaseDirectory, "upload"));

            using (var stream = new FileStream(localPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // db.SomeContext.Add(someData);
            // await db.SaveChangesAsync();

            return Ok(new { success = true, message = "All set", fileName });
        }

        // [HttpPost, DisableRequestSizeLimit]
        //[RequestSizeLimit(500 * 1024 * 1024)]       //unit is bytes => 500Mb
        //[RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
        //[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue /*int.MaxValue*/)]
        // [RequestSizeLimit(40000000)]
        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        [ActionName("UploadImage")]
        public async Task<IActionResult> UploadAsync()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files.First();

                if(formCollection.Files.ToList().Count <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose a file and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var file = Request.Form.Files[0];

                if (file == null || file.Length <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose a file and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var folderName = Path.Combine("Resources", "Documents");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var files1 = Request.Form.Files;
                if (files1 != null)
                {

                    var stream = files1[0].OpenReadStream();
                    StreamReader reader = new StreamReader(stream);
                    string result = reader.ReadToEnd();
                  
                }

                if (file.Length > 0)
                {
                    var fileName = Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);

                    var excelValidation = await FileValidationHelper.ValidateFileExcel(Environment, new FileValidationModel() { FileName = fullPath, AcctLogin = "", size = "" });
                    if (excelValidation.IsActive == false)
                    {
                        return Ok(excelValidation);
                    }

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    //Bank_Name
                    //Account_Number
                    //NPFDate 
                    //Command
                    //Grade_Step
                    //Grade
                    //Net_Pay
                    //Full_Name
                    //Staff_Id

                    bool isallow = false;

                    List<ClientNetPayINFO> clientpfInfos = new List<ClientNetPayINFO>();
                    var fileName1 = "/" + Path.GetFileName(fullPath);
                    // For .net core, the next line requires the NuGet package, 
                    // System.Text.Encoding.CodePages
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = System.IO.File.Open(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        if (isallow)
                        {
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
                            {
                                while (reader.Read()) //Each row of the file
                                {
                                    var Result = new ClientNetPayINFO();

                                    try
                                    {

                                        // var resultEx = reader[0];
                                        // var workSheet = reader.MergeCells[0];
                                        Result.Staff_Id = reader.GetValue(0).ToString();
                                        Result.Staff_Id = reader.GetString(0).ToString();
                                      
                                        //(ExcelDataReader.Core.OpenXmlFormat.XlsxWorkbook)
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Staff ID is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.Full_Name = reader.GetValue(1).ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Full Name is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.Bank_Name = reader.GetValue(2).ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Bank Name is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.Account_Number = Convert.ToString(reader.GetValue(3).ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Account Number is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.Net_Pay = Convert.ToDouble(reader.GetValue(4).ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Net Pay is not digit character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.Grade = reader.GetValue(5).ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Grade is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.Grade_Step = reader.GetValue(6).ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Grade step is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.Command = reader.GetValue(7).ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where Command step is not letter character or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }
                                    try
                                    {
                                        Result.NPFDate = Convert.ToDateTime(reader.GetValue(8).ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        // return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Check the excel file where NPF Date is not Date format or is not exit", false, "", null, Status.Ërror, StatusMgs.Error));
                                    }

                                    clientpfInfos.Add(Result);
                                }
                            }
                        }
                    }

                    var ExitFile = await lapoLoanDB.FileUploads.Where(x => x.Src == fullPath).FirstOrDefaultAsync();
                    if (ExitFile != null)
                    {
                        lapoLoanDB.FileUploads.Remove(ExitFile);
                        await lapoLoanDB.SaveChangesAsync();
                    }

                    var newFileUpload = new FileUpload()
                    {
                        FinishUploadDate = DateTime.Now,
                        Status = false,
                        UploadDate = DateTime.Now,
                        Src = fullPath,
                    };
                    lapoLoanDB.FileUploads.Add(newFileUpload);
                    await lapoLoanDB.SaveChangesAsync();

                    var Result12 = await NetPaysServices.LoadExcelData(Environment, fullPath);
                    if (Result12 != null && Result12.IsActive)
                    {
                        return Ok(Result12);
                    }
                    else
                    {
                        return Ok(Result12);
                    }

                    //FileInfo file1 = new FileInfo(Path.Combine(fullPath));
                    //using (var stream = new MemoryStream())
                    //{
                    //    //using (var package = new ExcelPackage(stream))
                    //    //{
                    //    //    package.SaveAs(file1);
                    //    //}
                    //}

                    //await ExcelFileReader.ReadFileAgain(Environment, fullPath);

                    /// Code

                    //if (clientpfInfos.Count > 0) 
                    //{
                    //    if (System.IO.File.Exists(fullPath))
                    //    {
                    //        System.IO.File.Delete(fullPath);
                    //    }

                    //    var Result = await new AdminActivitesHelpers(this).ValidateClientIPFNumberUpload(Environment, clientpfInfos);

                    //    if (Result.IsActive)
                    //    {
                    //         return Ok(Result);
                    //    }
                    //    else
                    //    {
                    //        return Ok(Result);
                    //    }

                    //    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The file you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
                    //}
                    //else
                    //{
                    //    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The file you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
                    //}


                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The file you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
                }
                else
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The file you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The file you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
            }
            catch (Exception ex)
            {
                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Internal server error: " + ex.Message, false, "", null, Status.Ërror, StatusMgs.Error));
                // return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("UploadIDCard")]
        public async Task<IActionResult> UploadIDCardAsync()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files.First();

                if (formCollection.Files.ToList().Count <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose another ID Card and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }


                var file = Request.Form.Files[0];

                if (file == null || file.Length <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose another ID Card and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var folderName = Path.Combine("Resources", "LoanAppImages");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var files1 = Request.Form.Files;
                if (files1 != null)
                {

                    var stream = files1[0].OpenReadStream();
                    StreamReader reader = new StreamReader(stream);

                    string result = reader.ReadToEnd();

                }

                if (file.Length > 0)
                {
                    var fileName = Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

              
                    string PFNumber = "", AccountId = "";
                    long Acct = 0;

                    var Request1 = Request.Form.Keys.ToList();
                    if (Request1 != null && Request1.Count > 1)
                    {
                        AccountId = Request.Form["PFNumber"].ToString();
                        PFNumber = Request.Form["AcctId"].ToString();

                        Acct = Convert.ToInt64(AccountId);
                        PFNumber = PFNumber.Replace("\"", string.Empty).Replace("\\", string.Empty).Trim('"').Replace(@"""", "");
                    }

                    var ClientKyc = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Pfnumber == PFNumber && n.AccountId == Acct && n.Status == StatusMgs.Pending).FirstOrDefaultAsync();
                    if (ClientKyc != null)
                    {
                        var ClientDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == ClientKyc.Id && n.AccountRequestId == Acct && n.Status == StatusMgs.Pending).FirstOrDefaultAsync();

                        if (ClientDetails != null)
                        {
                            var foldername = Path.Combine("Resources", "LoanAppImages");
                            pathToSave = Path.Combine(new DefalutToken(Configuration).ApiServer(), folderName);

                            var fullPath1 = Path.Combine(pathToSave, fileName);
                            ClientDetails.IdcardUrl = fullPath1;

                            lapoLoanDB.Entry(ClientDetails).State = EntityState.Modified;
                            lapoLoanDB.SaveChangesAsync();

                            return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "ID Card was succcessfully uploaded", true, "", null, Status.Ërror, StatusMgs.Error));
                        }
                    }

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                else
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The ID Card you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The ID Card is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
            }
            catch (Exception ex)
            {
                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Internal server error: " + ex.Message, false, "", null, Status.Ërror, StatusMgs.Error));
                // return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("UploadPayslip")]
        public async Task<IActionResult> UploadPayslipAsync()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files.First();

                if (formCollection.Files.ToList().Count <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose another Payslip and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var file = Request.Form.Files[0];

                if (file == null || file.Length <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose another Payslip and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var folderName = Path.Combine("Resources", "LoanAppImages");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var files1 = Request.Form.Files;
                if (files1 != null)
                {

                    var stream = files1[0].OpenReadStream();
                    StreamReader reader = new StreamReader(stream);

                    string result = reader.ReadToEnd();

                }

                if (file.Length > 0)
                {
                    var fileName = Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    string PFNumber = "", AccountId = "";
                    long Acct = 0;

                    var Request1 = Request.Form.Keys.ToList();
                    if (Request1 != null && Request1.Count > 1)
                    {
                        AccountId = Request.Form["PFNumber"].ToString();
                        PFNumber = Request.Form["AcctId"].ToString();

                        Acct = Convert.ToInt64(AccountId);
                        PFNumber = PFNumber.Replace("\"", string.Empty).Replace("\\", string.Empty).Trim('"').Replace(@"""", "");
                    }

                    var ClientKyc = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Pfnumber == PFNumber && n.AccountId == Acct && n.Status == StatusMgs.Pending).FirstOrDefaultAsync();
                    if (ClientKyc != null)
                    {
                        var ClientDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == ClientKyc.Id && n.AccountRequestId == Acct && n.Status == StatusMgs.Pending).FirstOrDefaultAsync();

                        if (ClientDetails != null)
                        {
                            var foldername = Path.Combine("Resources", "LoanAppImages");
                            pathToSave = Path.Combine(new DefalutToken(Configuration).ApiServer(), folderName);
                            var  fullPath1 = Path.Combine(pathToSave, fileName);

                            ClientDetails.PaySlipUrl =  fullPath1;

                            lapoLoanDB.Entry(ClientDetails).State = EntityState.Modified;
                            lapoLoanDB.SaveChangesAsync();

                            return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Payslip was succcessfully uploaded", true, "", null, Status.Ërror, StatusMgs.Error));
                        }
                    }

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                else
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The Payslip you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The Payslip is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
            }
            catch (Exception ex)
            {
                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Internal server error: " + ex.Message, false, "", null, Status.Ërror, StatusMgs.Error));
                // return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("UploadPassport")]
        public async Task<IActionResult> UploadPassportAsync()
        {
            try
            {

                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files.First();

                if (formCollection.Files.ToList().Count <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose another Passport and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var file = Request.Form.Files[0];

                if (file == null || file.Length <= 0)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: Choose another Passport and try again", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var folderName = Path.Combine("Resources", "LoanAppImages");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var files1 = Request.Form.Files;
                if (files1 != null)
                {

                    var stream = files1[0].OpenReadStream();
                    StreamReader reader = new StreamReader(stream);

                    string result = reader.ReadToEnd();

                }

                if (file.Length > 0)
                {
                    var fileName = Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    string PFNumber ="", AccountId ="";
                    long Acct =0;
                    //"\"
                    //    \""
                    var Request1 = Request.Form.Keys.ToList();
                    if(Request1!=null && Request1.Count > 1)
                    {
                         AccountId = Request.Form["PFNumber"].ToString();
                         PFNumber = Request.Form["AcctId"].ToString();

                         Acct = Convert.ToInt64(AccountId);
                         PFNumber = PFNumber.Replace("\"", string.Empty).Replace("\\", string.Empty).Trim('"').Replace(@"""", "");

                    }

                    var ClientKyc = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Pfnumber == PFNumber && n.AccountId == Acct && n.Status == StatusMgs.Pending).FirstOrDefaultAsync();
                    if (ClientKyc != null)
                    {
                        var ClientDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == ClientKyc.Id && n.AccountRequestId == Acct && n.Status == StatusMgs.Pending).FirstOrDefaultAsync();

                        if (ClientDetails != null)
                        {
                            //dbPath
                         
                            var foldername = Path.Combine("Resources", "LoanAppImages");
                             pathToSave = Path.Combine(new DefalutToken(Configuration).ApiServer(), folderName);
                            var fullPath1 = Path.Combine(pathToSave, fileName);

                            ClientDetails.PassportUrl = fullPath1;

                            lapoLoanDB.Entry(ClientDetails).State = EntityState.Modified;
                            lapoLoanDB.SaveChangesAsync();

                            return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Passport was succcessfully uploaded", true, "", null, Status.Ërror, StatusMgs.Error));
                        }
                    }

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                else
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The Passport you upload is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "File error: The Passport is a corrupt file, Choose another file", false, "", null, Status.Ërror, StatusMgs.Error));
            }
            catch (Exception ex)
            {
                return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Internal server error: " + ex.Message, false, "", null, Status.Ërror, StatusMgs.Error));
                // return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        //[HttpPost("upload-stream-multipartreader")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        //public async Task<IActionResult> Upload()
        //{
        //    var fileUploadSummary = await _fileService.UploadFileAsync(HttpContext.Request.Body, Request.ContentType);
        //    return CreatedAtAction(nameof(Upload), fileUploadSummary);
        //}
        //public async Task<FileUploadSummary> UploadFileAsync(Stream fileStream, string contentType)
        //{
        //    var fileCount = 0;
        //    long totalSizeInBytes = 0;

        //    var boundary = GetBoundary(MediaTypeHeaderValue.Parse(contentType));
        //    var multipartReader = new MultipartReader(boundary, fileStream);
        //    var section = await multipartReader.ReadNextSectionAsync();

        //    var filePaths = new List<string>();
        //    var notUploadedFiles = new List<string>();

        //    while (section != null)
        //    {
        //        var fileSection = section.AsFileSection();
        //        if (fileSection != null)
        //        {
        //            totalSizeInBytes += await SaveFileAsync(fileSection, filePaths, notUploadedFiles);
        //            fileCount++;
        //        }

        //        section = await multipartReader.ReadNextSectionAsync();
        //    }

        //    return new FileUploadSummary
        //    {
        //        TotalFilesUploaded = fileCount,
        //        TotalSizeUploaded = ConvertSizeToString(totalSizeInBytes),
        //        FilePaths = filePaths,
        //        NotUploadedFiles = notUploadedFiles
        //    };
        //}
    }
}
