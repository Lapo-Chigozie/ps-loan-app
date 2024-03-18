using LapoLoanWebApi.Service;
using Microsoft.AspNetCore.Mvc;
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
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.TwoFactorsHelpers;
using LapoLoanWebApi.LoanNetPaysHelpers;

namespace LapoLoanWebApi.Controllers
{
    [EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FileValidationController : ControllerBase
    {
        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;
        private OleDbReader OleDbReader;
        private NetPaysServices NetPaysServices;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public FileValidationController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {

            this.Configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            Environment = _environment;
            OleDbReader = new OleDbReader(this, this.Configuration);
            this.NetPaysServices = new NetPaysServices(this, this.Configuration);
        
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)] //xlsx
        [ActionName("ValidateFileUploadImage")]
        public async Task<IActionResult> ValidateFileImage([FromBody]FileValidationModel fileValidation)
        {
            return Ok(await FileValidationHelper.ValidateFileImage(fileValidation));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)] //xlsx
        [ActionName("ValidateSlipFileImage1")]
        public async Task<IActionResult> ValidateSlipFileImage([FromBody] FileValidationModel fileValidation)
        {
            return Ok(await FileValidationHelper.ValidateSlipFileImage(fileValidation));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ValidateFileUploadExcel")]
        public async Task<IActionResult> ValidateFileExcel(FileValidationModel fileValidation)
        {
            return Ok(await FileValidationHelper.ValidateFileImage(fileValidation));
        }
    }
}


