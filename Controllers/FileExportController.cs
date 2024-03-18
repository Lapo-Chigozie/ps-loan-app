using LapoLoanWebApi.E360Helpers;
using Microsoft.AspNetCore.Mvc;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.Service;
using LapoLoanWebApi.TwoFactorsHelpers;
using Microsoft.AspNetCore.Cors;
using LapoLoanWebApi.HubTeams;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using Newtonsoft.Json;
using ExcelDataReader;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanNetPaysHelpers;
using Microsoft.EntityFrameworkCore;


namespace LapoLoanWebApi.Controllers
{

    [EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public sealed class FileExportController : ControllerBase
    {
        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;
        private OleDbReader OleDbReader;
        private NetPaysServices NetPaysServices;
        private UploadDisbursmentLoans UploadDisbursmentLoans;

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        private E360AuthHttpValidation e360AuthHttp;

        public FileExportController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Configuration = _configuration;
            this.e360AuthHttp = new E360AuthHttpValidation(this, this.Configuration);
            ///  ImageArrangement.GenerateImageData();
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            Environment = _environment;
            OleDbReader = new OleDbReader(this, this.Configuration);
            this.NetPaysServices = new NetPaysServices(this, this.Configuration);
            this.UploadDisbursmentLoans = new UploadDisbursmentLoans(this, this.Configuration);
        }

        [HttpPost]
        [ActionName("ExportApprovedLoanApps")]
        public async Task<IActionResult> GetExportApprovedLoanApp([FromBody] FormBoard viewModel)
        {
            var ActY = await new ExportLoansActivity(this, this.Configuration).ExportExcelData(this.Environment, viewModel);

            if (ActY != null && ActY.IsActive)
            {

                var StartDate = Convert.ToDateTime(viewModel.StartDate);
                var EndDate = Convert.ToDateTime(viewModel.EndDate);

                if (StartDate > EndDate)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var Lapo_Approved_Loan = "Lapo_Approved_Loan" + StartDate.ToLongDateString().Replace(" ", "") + EndDate.ToLongDateString().Replace(" ", "") + ".xlsx";

                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var memoryStream = new MemoryStream();

                ActY.DataLoad.SaveAs((Stream)memoryStream);

                memoryStream.Seek(0L, SeekOrigin.Begin);

                var content = memoryStream.ToArray();
                Console.Write(content);

                return File(content, contentType, Lapo_Approved_Loan);
            }

            return Ok(ActY);
        }

        [HttpGet]
        [ActionName("GetExportApprovedLoanApps")]
        public async Task<IActionResult> GetExportApprovedLoanApps(string FileUrl)
        {
            var viewModel = JsonConvert.DeserializeObject<FormBoard>(FileUrl);

            var ActY = await new ExportLoansActivity(this, this.Configuration).ExportExcelData(this.Environment, viewModel);

            if (ActY != null && ActY.IsActive)
            {
                var StartDate = Convert.ToDateTime(viewModel.StartDate);
                var EndDate = Convert.ToDateTime(viewModel.EndDate);

                if (StartDate > EndDate)
                {
                    return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error));
                }

                var Lapo_Approved_Loan = "Lapo_Approved_Loan" + StartDate.ToLongDateString().Replace(" ", "") + EndDate.ToLongDateString().Replace(" ", "") + ".xlsx";

                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var memoryStream = new MemoryStream();

                ActY.DataLoad.SaveAs((Stream)memoryStream);

                memoryStream.Seek(0L, SeekOrigin.Begin);

                var content = memoryStream.ToArray();
                Console.Write(content);

                return File(content, contentType, Lapo_Approved_Loan);
            }

            return Ok(ActY);
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        [ActionName("UploadDisbursLoans")]
        public async Task<IActionResult> UploadDisbursLoansAsync()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files.First();

                if (formCollection.Files.ToList().Count <= 0)
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

                    var Result12 = await new ExportLoansActivity(this, this.Configuration).UploadApprovedExcelData(Environment, fullPath);

                    if (Result12 != null && Result12.IsActive)
                    {
                        return Ok(Result12);
                    }
                    else
                    {
                        return Ok(Result12);
                    }

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

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        [ActionName("UploadRepaymentLoans")]
        public async Task<IActionResult> UploadRepaymentLoansAsync()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var files = formCollection.Files.First();

                if (formCollection.Files.ToList().Count <= 0)
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

                    var Result12 = await new ExportLoansActivity(this, this.Configuration).UploadRepaymentSheduledData(Environment, fullPath);

                    if (Result12 != null && Result12.IsActive)
                    {
                        return Ok(Result12);
                    }
                    else
                    {
                        return Ok(Result12);
                    }

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
    }
}
