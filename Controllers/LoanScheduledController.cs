using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanScheduled;
using LapoLoanWebApi.LoanScheduled.Model;
using LapoLoanWebApi.Service;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LapoLoanWebApi.Controllers
{
    [EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LoanScheduledController : ControllerBase
    {
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        private LoanSchedulerHelpers e360AuthHttp;

        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;
        public LoanScheduledController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            this.Environment = _environment;
            this.e360AuthHttp = new LoanSchedulerHelpers(this, this.Configuration);
            ///  ImageArrangement.GenerateImageData();
            ///  
            this.e360AuthHttp.BackGroundRunningDueDate(_environment);
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("CheckCustomerHasRunningLoan")]
        public async Task<IActionResult> GetCheckCustomerHasRunningLoan(ScheduledDto scheduled)
        {
            return Ok(await this.e360AuthHttp.CheckCustomerHasRunningLoan(Environment, scheduled));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("CalculateScheduledLoanAmount")]
        public async Task<IActionResult> GetCalculateScheduledLoanAmount(ScheduledMethod scheduled)
        {
            return Ok(await this.e360AuthHttp.CalculateScheduledLoanAmount(Environment, scheduled));
        }

       
    }
}
