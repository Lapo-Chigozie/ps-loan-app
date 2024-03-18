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
    public class RefinerysController : ControllerBase
    {
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;
        public RefinerysController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            this.Environment = _environment;
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("EncriptRefinery")]
        public async Task<IActionResult> GetEncriptRefinery([FromBody] AppRefineryHash  appRefinery)
        {
            return Ok(await new RefineryHashings(Environment, this).GetEncriptRefinery( appRefinery));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("DecriptRefinery")]
        public async Task<IActionResult> GetDecriptRefinery([FromBody] AppRefineryHash appRefinery)
        {
            return Ok(await new RefineryHashings(Environment, this).GetDecriptRefinery(appRefinery));
        }
    }
}
