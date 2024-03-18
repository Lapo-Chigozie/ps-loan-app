using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.Service;
using LapoLoanWebApi.TwoFactorsHelpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Identity.Client;
using LapoLoanWebApi.EnAndDeHelper;
using System.Globalization;
using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.ModelDto.PagenationFilterModel;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using LapoLoanWebApi.E360Helpers.E360FoundationServices;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.BasedApiController;
using Newtonsoft.Json;
using LapoLoanWebApi.LoanScheduled;

namespace LapoLoanWebApi.Controllers
{
    //[System.Web.Http.RoutePrefix("api/AcctSecut")]
    // [Authorize]
    //[AllowMyRequests]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
    [ApiController]
    // [Route("api/[controller]")]
    //[Route("[controller]")]
    [Route("api/[controller]/[action]")]
    public class AcctSecurityController : ControllerBase
    {
        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private E360AuthHttpValidation e360AuthHttp = null;
        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;

        public AcctSecurityController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Configuration = _configuration;
            this.Environment = _environment;
            this.e360AuthHttp = new E360AuthHttpValidation(this, this.Configuration);
          
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);

            ///  ImageArrangement.GenerateImageData();
        }

        [HttpGet]
        [ActionName("LoadSpinnerRound")]
        public async Task<IActionResult> LoadSpinnerRoundByNo(int Lenght = 0)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetRoundomNo(Lenght));
        }

        //[NonAction]
        //[DisableCors]
        [HttpGet(Name = "AutoRegUser")]
        [ActionName("AutoRegUser")]
        public int AutoRegisterUser()
        {
            return int.Parse(this.User.Claims.First(p => p.Type == ClaimTypes.NameIdentifier).Value);
        }

        [HttpGet(Name = "AutoOverview")]
        //[Route("/[controller]/[action]/{id}")]
        public ActionResult Overview(int? id)
        {
            return Ok(id);
        }

        // [HttpGet]
        // [Route("/[controller]/[action]/{id}")]
        public ActionResult Overviewer(int? id)
        {
            return Ok(id);
        }

        //[HttpPost(Name = "AutoAdminCreateAcctAsync")]
        //  [Authorize()]

        [HttpGet(Name = "AutoAdminCreateAcct")]
        [ActionName("AutoAdminCreateAcct")]
        public async Task<IActionResult> AutoAdminCreateAccounttAsync(string data)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).CreateAdministration());
        }

        [HttpPost(Name = "CreateAcctAsync")]
        public async Task<IActionResult> CreateAccountAsync([FromBody] AcctCreationModel acctCreation)
        {
            return null;
        }

        [HttpPost(Name = "AcctReset")]
        public async Task<IActionResult> AccountReset([FromBody] AcctResetDto acctReset)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).AcctReset(acctReset));
        }

        [HttpPost("NewPwdReset")]
        public async Task<IActionResult> NewPasswordReset([FromBody] NewAcctPasswordResetDto newAcctPasswordReset)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).NewPasswordReset(newAcctPasswordReset));
        }

        //[EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
        [HttpPost(Name = "SignInAuth")]
        [ActionName("SignInAuth")]
        public async Task<IActionResult> SignInAuthentication([FromBody] AcctSignInModel acctSignIn)
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

            return Ok(await new AdminActivitesHelpers(this, this.Configuration).SignInAcctClient(acctSignIn));
        }

        [HttpPost(Name = "TwoFactorAuth")]
        public async Task<System.Web.Http.IHttpActionResult> TwoFactorAuthentication([FromBody] TwoFactorAuthDto twoFactorAuth)
        {
            return (System.Web.Http.IHttpActionResult)Ok(await new AdminActivitesHelpers(this, this.Configuration).TwoFactorAuth(twoFactorAuth));
        }

        [HttpPost]
        [ActionName("ConfirmTwoFactorAuth")]
        public async Task<IActionResult> ConfirmTwoFactorAuthCode([FromBody] TwoFactorAuthÇodeDto twoFactorAuth)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ValidateTwoFactorAuth(twoFactorAuth));
        }

        [HttpPost]
        [ActionName("CheckIfEmailExit")]
        public async Task<IActionResult> CheckIfEmailAddressExit([FromBody] EmailAcctDto twoFactorAuth)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).CheckIfAcctExit(this.Environment, twoFactorAuth.EmailAddress));
        }

        [HttpPost]
        [ActionName("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] PwrdChangeDto pwrdChange)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ChangePassword(pwrdChange));
        }

        [HttpPost]
        [ActionName("InnerChangePassword")]
        public async Task<IActionResult> InneredChangePassword([FromBody] PwrdChangeDto pwrdChange)
        {
            return Ok(await new     AdminActivitesHelpers(this, this.Configuration).InneredChangePassword(pwrdChange));
        }

        [HttpPost]
        [ActionName("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterCustomerDto registerCustomer)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).CreateCustomerAccount(this.Environment, registerCustomer));
        }

        [HttpGet]
        [ActionName("FetchAccountDetails")]
        public async Task<IActionResult> FetchAcctDetails(string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetAcctDetailsById(int.Parse(AcctId)));
        }

        [HttpGet]
        [ActionName("ReSendTwoFactorCode")]
        public async Task<IActionResult> ReSendTwoFactorLoginCode(string AcctId, string message)
        {

            return Ok(await new TwoFactorsHelper(this.Configuration).SendTwoFactorLoginsmsAsync(Environment, message, int.Parse(AcctId)));
        }

        [HttpPost]
        [ActionName("BvnSettledment")]
        public async Task<IActionResult> BvnSettledmentCheckers([FromBody] BvnCheckerDto bvnRequest)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).CheckIfBvnCheckers(bvnRequest));
        }

        [HttpPost]
        [ActionName("RequestBvn")]
        public async Task<IActionResult> RequestBankVerificationNumber([FromBody] BvnRequestDto bvnRequest)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).CheckIfBvnExit(bvnRequest));
        }

        [HttpPost]
        [ActionName("SendBvnAuthentication")]
        public async Task<IActionResult> SendBvnAuth([FromBody] BvnAuthDto bvnAuths)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).TwoFactorBvnAuth(this.Environment, bvnAuths));
        }

        [HttpPost]
        [ActionName("VerifyBvnOtpCode")]
        public async Task<IActionResult> VerifyBvnOtp([FromBody] BvnCodeDto codeDto)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).VerifyBvnAuth(codeDto.code));
        }

        [HttpGet]
        [ActionName("GetAllBanks")]
        public async Task<IActionResult> GetListOfBanks()
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetAllBankList());
        }

        [HttpPost]
        [ActionName("BankAcctDetailsByAccountNo")]
        public async Task<IActionResult> BankAcctDetailsByAcctNo([FromBody] RequestAccountNoDto requestAccount)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetBankAcctDetailsByAcctNo(requestAccount));
        }
     
        //[HttpGet]
        //[ActionName("emailacctverify")]
        //public async Task<IActionResult> emailaccountverify(string AcctId)
        //{
        //    return Ok(await new AdminActivitesHelpers(this, this.Configuration).AccountVerify(this.Environment, AcctId));
        //}

        [HttpGet]
        [ActionName("Updateemailacctverify")]
        public async Task<IActionResult> emailaccountverifys(string data)
        {
            var DataOp = JsonConvert.DeserializeObject<UpdateAccoutModel>(data);

            return Ok(await new AdminActivitesHelpers(this, this.Configuration).emailaccountverifys(this.Environment, DataOp));
        }

        [HttpPost]
        [ActionName("ClientNetPay")]
        public async Task<IActionResult> GetClientNetPay(PagenationFilterDto pagenationFilter)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetClientsNetPayHeader(this.Environment, pagenationFilter));
        }

        [HttpPost]
        [ActionName("ClientNetPay1")]
        public async Task<IActionResult> GetClientNetPay1(PagenationFilterDto pagenationFilter)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetClientsNetPayHeader(this.Environment, pagenationFilter.AcctId, pagenationFilter.AppId, pagenationFilter));
        }

        [HttpGet]
        [ActionName("ClientMonthlyNetPays")]
        public async Task<IActionResult> ClientMonthlyNetPayDetails(string Pfnumber = "")
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetClientsNetPayDetails(this.Environment, Pfnumber));
        }

        [HttpGet]
        [ActionName("DeleteClientNetPay")]
        public async Task<IActionResult> RemoveClientNetPay(string Pfnumber = "", string AccountId = "")
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).DeleteClientsNetPayDetails(this.Environment, Pfnumber));
        }

        [HttpGet]
        [ActionName("GetUserProfileDetails")]
        public async Task<IActionResult> UserProfileDetails(int AccounttId = 0)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetUserProfileDetails(this.Environment, AccounttId));
        }

        [HttpGet]
        [ActionName("GetUserPermissionDetails")]
        public async Task<IActionResult> GetUserPermissionDetails(int AccounttId = 0)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetUserPermissionDetails(this.Environment, AccounttId));
        }

        [HttpPost]
        [ActionName("ChangeProfileDetails")]
        public async Task<IActionResult> UpdateChangeProfileDetails(UpdateProfileModelDto updateProfile)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).UpdateProfile(updateProfile));
        }

        [HttpPost]
        [ActionName("ChangeProfileDetails1")]
        public async Task<IActionResult> UpdateChangeProfileDetails1(UpdateProfileModelDto updateProfile)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).UpdateProfile1(updateProfile));
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("UploadImage")]
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

        [HttpGet]
        [ActionName("UserTwoFactorActivator")]
        public async Task<IActionResult> TwoFactorActivator(int AccounttId = 0)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ActivateTwoFactorAuth(this.Environment, AccounttId));
        }

        [HttpPost]
        [ActionName("RebroundBankList")]
        public async Task<IActionResult> GetAlligbleLoan(GetAlligbleLoanDto getAlligbleLoan)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetAlligbleLoan(this.Environment, getAlligbleLoan));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("newLoanApplication")]
        public async Task<IActionResult> CreateNewLoanApplication([FromBody] NewLoanAppModelDto loanApp)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).SaveNewLoanApp(this.Environment, loanApp));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AllLoanAppList")]
        public async Task<IActionResult> GetAllLoanAppList1(string AccountId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetLoanAppList1(this.Environment, AccountId));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AllDashboardLoanAppList")]
        public async Task<IActionResult> GetAllDashboardLoanAppList(PagenationFilterDto pagenationFilter)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetDashboardLoanAppList(this.Environment, pagenationFilter));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AllDisbursedLoanAppList")]
        public async Task<IActionResult> GetAllDisbursedLoanAppList(PagenationFilterDto pagenationFilter)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetAllDisbursedLoanAppList(this.Environment, pagenationFilter));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AllOngoingAndCompletedLoanAppList")]
        public async Task<IActionResult> GetAllOngoingAndCompletedLoanAppList(PagenationFilterDto pagenationFilter)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetAllOngoingAndCompletedLoanAppList(this.Environment, pagenationFilter));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AllLoanApp")]
        public async Task<IActionResult> GetAllLoanAppList(PagenationFilterDto pagenationFilter)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetLoanAppList(this.Environment, pagenationFilter));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AllMonthlyNetPays")]
        public async Task<IActionResult> MonthsNetPays(PagenationFilterDto pagenationFilter)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetAllMonthlyNetPays(this.Environment, pagenationFilter));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("LoanAppDetails")]
        public async Task<IActionResult> GetLoanAppDetails(string AppHeaderId, string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetLoanAppDetails(this.Environment, AppHeaderId, AcctId));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AdminActivateLoanMethod")]
        public async Task<IActionResult> ActivateLoanMethod([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ActivateLoanMethod(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AdminSaveLoanMethod")]
        public async Task<IActionResult> SaveLoanMethod([FromBody] LoanMethod loanMethod)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).SaveLoanMethod(this.Environment, loanMethod));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("NewNarration")]
        public async Task<IActionResult> AdminSaveNewNarration([FromBody] NewNarrations newNarration)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).AdminSaveNewNarration(this.Environment, newNarration));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AdminLoanMethodList")]
        public async Task<IActionResult> LoanMethodList(string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).LoanMethodList(this.Environment, AcctId));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ListOfNarrationList")]
        public async Task<IActionResult> ListOfNarraList(string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ListOfNarra(this.Environment, AcctId));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AdminLoanMethodListApp")]
        public async Task<IActionResult> LoanMethodListApp(string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).LoanMethodListApp(this.Environment, AcctId));
        }

        //[HttpGet, DisableRequestSizeLimit]
        //[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        //[ActionName("AdminLoanMethodList")]
        //public async Task<IActionResult> LoanMethodList(string AcctId)
        //{
        //    return Ok(await new AdminActivitesHelpers(this).LoanMethodList(this.Environment, AcctId));
        //}

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AdminCancelLoanAppRequest")]
        public async Task<IActionResult> AdminCancelLoanApp([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).AdminCancelLoanApp(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AdminApprovedLoanAppRequest")]
        public async Task<IActionResult> AdminApprovedLoanApp([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).AdminApproveLoanApp(this.Environment, loanAppRequest));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("CancelLoanAppRequest")]
        public async Task<IActionResult> CancelLoanApp([FromBody] CancelLoanAppRequest loanAppRequest)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).CancelLoanApp(this.Environment, loanAppRequest));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ActivateCustomerLoanPermission")]
        public async Task<IActionResult> GetActivateCustomerLoanPermission(string Pfnumber)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ActivateCustomerLoanPermission(this.Environment, Pfnumber));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("ConvertedLoanAmount")]
        public async Task<IActionResult> GetActivateCustomerLoanPermission(double Amount)
        {
            var result = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)Amount)).Replace(".00", "");
            return Ok(new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Loans Retrieved successfully.", true, result, result, Status.Success, StatusMgs.Success));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("EncrptData")]
        public async Task<IActionResult> GetEncrptData(string Retrieved)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).EncriptData(this.Environment, Retrieved));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("DecriptData")]
        public async Task<IActionResult> GetDecriptData(string Retrieved)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).DecriptData(this.Environment, Retrieved));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("newLoanApplication1")]
        public async Task<IActionResult> CreateNewLoanApplication()
        {

            try
            {

                var file = Request.Form.Files[0];

                if (file == null || file.Length <= 0)
                {

                }

                return Ok("");
            }
            catch (Exception ex)
            {
                return Ok("");
            }

        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("CustomerdashboardLoanApp")]
        public async Task<IActionResult> CustomerdashboardLoan([FromBody] CustomerAcctDto customerAcct)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetCustomerDashboardDetails(this.Environment, customerAcct));
        }

        [HttpPost, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("AdmindashboardLoanApp")]
        public async Task<IActionResult> AdmindashboardLoan([FromBody] CustomerAcctDto customerAcct)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetAdminDashboardDetails(this.Environment, customerAcct));
        }

        [HttpGet]
        [ActionName("ValidatePFNumberByProvider")]
        public async Task<IActionResult> ValidatePFNoByProvider(string Number = "PF0015485")
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ValidateClientNumber(this.Environment, Number));
        }

        [HttpPost]
        [ActionName("UpdateLoanSettings")]
        public async Task<IActionResult> AdminUpdateLoanSettings([FromBody] LoanSettingsModel loanSettings)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).AdminUpdateLoanSettings(this.Environment, loanSettings));
        }

        [HttpGet]
        [ActionName("GetLoanSettings")]
        public async Task<IActionResult> AdminGetLoanSettings(string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).AdminGetLoanSettings(this.Environment, AcctId));
        }

        [HttpGet]
        [ActionName("GetE360UsersByTeamMember")]
        public async Task<IActionResult> GetE360UsersByTeamMember(string AcctId, string inputUser, string SystemRef1, string SystemRole1)
        {
            return Ok(await new E360Foundation(this, this.Configuration).GetE360ApiAsync(long.Parse(AcctId), inputUser, SystemRef1, SystemRole1));
        }

        [HttpGet]
        [ActionName("GetE360UsersByCustomer")]
        public async Task<IActionResult> GetE360UsersByCustomer(string AcctId, string inputUser)
        {
            return Ok(await new E360FoundationCustomers(this, this.Configuration).GetE360ApiAsync(long.Parse(AcctId), inputUser));
        }

        [HttpGet]
        [ActionName("GetStates")]
        public async Task<IActionResult> GetStates()
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ListOfStates(this.Environment));
        }

        [HttpGet]
        [ActionName("GetCitiesByStates")]
        public async Task<IActionResult> GetCitiesByStates(int Id)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ListOfCitysByState(this.Environment, Id));
        }

        [HttpGet]
        [ActionName("CheckIfAcctIsActive")]
        public async Task<IActionResult> GetCheckIfAcctIsActive(string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetCheckIfAcctIsActive(AcctId));
        }

        [HttpGet]
        [ActionName("GetBusinessSegments")]
        public async Task<IActionResult> ListOfBusinessSegments()
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ListOfBusinessSegment(this.Environment));
        }

        [HttpGet]
        [ActionName("GetBusinessTypes")]
        public async Task<IActionResult> ListOfGetBusinessTypes()
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).ListOfBusinessTypes(this.Environment, 0));
        }

        [HttpGet, DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [ActionName("GetLoanSetting")]
        public async Task<IActionResult> GetAllLoanSettings(string Id)
        {
            return Ok(await new LoanSchedulerHelpers(this, this.Configuration).GetAllLoanSettings(Environment));
        }

        [HttpGet]
        [ActionName("CheckIfBankAccountIsOk")]
        public async Task<IActionResult> GetCheckIfBankAccountIsOk(string AcctId)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetCheckIfBankAccountIsOk(AcctId));
        }
    }
}
