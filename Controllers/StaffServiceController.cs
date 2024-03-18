using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.E360Helpers.E360DtoModel;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.ModelDto.PagenationFilterModel;
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
    public class StaffServiceController : ControllerBase
    {

        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;

        private E360AuthHttpValidation e360AuthHttp;
        public StaffServiceController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Environment = _environment;
            this.Configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            this.e360AuthHttp = new E360AuthHttpValidation(this, this.Configuration);
            ///  ImageArrangement.GenerateImageData();
        }

        [HttpPost]
        [ActionName("AllStaffList")]
        public async Task<IActionResult> GetAllStaffList(/*string AcctId = "PF0015485"*/ PagenationFilterDto pagenationFilter)
        {
            return Ok(await this.e360AuthHttp.GetStaffList(this.Environment, pagenationFilter));
        }

        [HttpPost(Name = "AdminSignInAuth")]
        [ActionName("AdminSignInAuth")]
        public async Task<IActionResult> AdminSignInAuthentication([FromBody] AcctSignInModel acctSignIn)
        {

            // return  Request.CreateResponse(HttpStatusCode.OK, await new AdminActivitesHelpers(this).SignInAcct(acctSignIn));

            // return NoContent();
            // return NotFound();
            // return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployee.EmployeeId }, createdEmployee);
            // return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new employee record");
            // return new HttpResponseMessage(HttpStatusCode.BadRequest);


            //var response = new HttpResponseMessage(HttpStatusCode.OK);
            //response.IsSuccessStatusCode = true;
            //response.EnsureSuccessStatusCode();
            //response.RequestMessage = new HttpRequestMessage() { Version = new Version(1, 0), VersionPolicy = HttpVersionPolicy.RequestVersionExact, Method = System.Net.Http.HttpMethod.Post,  };
            ////Debug.WriteLine("New Product Information");
            ////Debug.WriteLine("Id: " + product.Id);
            ////Debug.WriteLine("Name: " + product.Name);
            ////Debug.WriteLine("Price: " + product.Price);
            //return response;

            //return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK,   = };
            var Data = await this.e360AuthHttp.SignInAcctClient(acctSignIn);

            return Ok(Data);
            //return Ok(await new AdminActivitesHelpers(this).SignInAcct(acctSignIn));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessAction")]
        public async Task<IActionResult> AdminProcessAction([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.ActivateE360Staff(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessAction1")]
        public async Task<IActionResult> AdminProcessAction1([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.ActivateE360Staff1(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessAction2")]
        public async Task<IActionResult> AdminProcessAction2([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.ActivateE360Staff2(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessAction3")]
        public async Task<IActionResult> AdminProcessAction3([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.ActivateE360Staff3(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessAction4")]
        public async Task<IActionResult> AdminProcessAction4([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.ActivateE360Staff4(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessAction5")]
        public async Task<IActionResult> AdminProcessAction5([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.ActivateE360Staff5(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessAction6")]
        public async Task<IActionResult> AdminProcessAction6([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.ActivateE360Staff6(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("RegisterStaffs")]
        public async Task<IActionResult> RegisterStaff([FromBody] E360RegisterModelDto e360Register)
        {
            return Ok(await this.e360AuthHttp.RegisterStaff(e360Register));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessSetPermissionToCreatedStaff")]
        public async Task<IActionResult> AdminProcessSetPermissionToCreatedStaff([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.SetPermissionToCreatedStaff(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessSetPermissionHasPermissionToDisableStaff")]
        public async Task<IActionResult> AdminSetPermissionHasPermissionToDisableStaff([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await this.e360AuthHttp.SetPermissionHasPermissionToDisableStaff(this.Environment, loanAppRequest));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ProcessGetAllStaffAccessRight")]
        public async Task<IActionResult> GetProcessGetStaffAccessRight(string AcctId)
        {
            return Ok(await this.e360AuthHttp.GetProcessGetStaffLoginAccessRight(Environment, int.Parse(AcctId)));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("StaffAccessRight")]
        public async Task<IActionResult> GetStaffAccessRight(string AcctId)
        {
            return Ok(await this.e360AuthHttp.GetStaffAccessRight(Environment, AcctId));
        }
    }
}
