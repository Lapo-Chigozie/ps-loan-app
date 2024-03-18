using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.Service;
using LapoLoanWebApi.TwoFactorsHelpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LapoLoanWebApi.Controllers
{
    [EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PrintServiceController : ControllerBase
    {
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;

        public PrintServiceController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            this. Environment = _environment;
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("SetLoanPrinterArrangement")]
        public async Task<IActionResult> GetSetLoanPrinterArrangement([FromBody] PrinterLoanAppModel printerLoanApp)
        {
            return Ok( await new PrinterArrangement(this, this.Configuration).SetrinterLoanApp(printerLoanApp));
        }
    }
}
