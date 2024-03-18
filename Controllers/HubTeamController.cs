using LapoLoanWebApi.E360Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
using LapoLoanWebApi.HubTeams;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using Newtonsoft.Json;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

namespace LapoLoanWebApi.Controllers
{
    [EnableCors(LapoLoanAllowSpecificOrigins.OriginNamme1)]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public sealed class HubTeamController : ControllerBase
    {
        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        private E360AuthHttpValidation e360AuthHttp;

        private LapoLoanDBContext lapoLoanDB = null;
        private IConfiguration Configuration = null;

        public HubTeamController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Configuration = _configuration;
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
            this.Environment = _environment;
            this.e360AuthHttp = new E360AuthHttpValidation(this, this.Configuration);
            ///  ImageArrangement.GenerateImageData();
        }

        [HttpPost]
        [ActionName("HubTeamList")]
        public async Task<IActionResult> GetHubTeamListAsync([FromBody] PagenationFilterDto pagenationFilter)
        {
             return Ok(await new HubTeamsActivity(this, this.Configuration).GetHubTeamList(Environment, pagenationFilter));
        }

        [HttpPost]
        [ActionName("HubTeamListByNameMember")]
        public async Task<IActionResult> GetHubTeamListByNameMemberListAsync([FromBody] PagenationFilterDto pagenationFilter)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).GetHubTeamByMemberList(Environment, pagenationFilter));
        }

        //[HttpPost]
        //[ActionName("SubHubTeamList")]
        //public async Task<IActionResult> GetSubHubTeamListAsync([FromBody] PagenationFilterDto pagenationFilter)
        //{
        //    return Ok(await new HubTeamsActivity(this).GetSubHubTeamList(Environment, pagenationFilter));
        //}

        [HttpPost]
        [ActionName("CreateHubTeam")]
        public async Task<IActionResult> CreateHubTeamAsync([FromBody] NewHubGroup newHub)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).CreateHubTeam(Environment, newHub));
        }

        [HttpGet]
        [ActionName("ActivateHubGroup")]
        public async Task<IActionResult> ActivateHubGroupAsync(string AppsId)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).ActivateHubGroup(Environment, AppsId));
        }

        [HttpGet]
        [ActionName("ActivateHubTeamMember")]
        public async Task<IActionResult> ActivateHubTeamMemberAsync(string AppsId)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).ActivateHubTeamMember(Environment, AppsId));
        }

        [HttpPost]
        [ActionName("CreateNewHubTeamMember")]
        public async Task<IActionResult> CreateHubTeamMember([FromBody] NewHubTeamMember newHub)
        {

            var json1 = JsonConvert.SerializeObject(newHub);

            return Ok(await new HubTeamsActivity(this, this.Configuration).CreateHubMember(Environment, newHub));
        }

        [HttpPost]
        [ActionName("ExitNewHubTeamMember")]
        public async Task<IActionResult> ExitNewHubTeamMember([FromBody] EditHubTeamMember  editHubTeam)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).ExitNewHubTeamMember(Environment, editHubTeam));
        }

        [HttpPost]
        [ActionName("HubTeamMembers")]
        public async Task<IActionResult> GetHubTeamMembersAsync([FromBody] PagenationFilterDto pagenationFilter)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).GetHubTeamMemberLists(Environment, pagenationFilter));
        }

        [HttpPost]
        [ActionName("HubTeamMembersByGroupId")]
        public async Task<IActionResult> GetHubTeamMembersByGroupAsync([FromBody] PagenationFilterDto pagenationFilter)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).GetHubTeamMemberByGroupLists(Environment, pagenationFilter));
        }

        [HttpPost]
        [ActionName("HubTeamsDisbursmentOfficers")]
        public async Task<IActionResult> GetHubTeamsDisbursmentOfficers([FromBody] PagenationFilterDto pagenationFilter)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).GetHubTeamsDisbursmentOfficers(Environment, pagenationFilter));
        }

        [HttpPost]
        [ActionName("OfficerStandardLoan")]
        public async Task<IActionResult> SetOfficerStandardLoan([FromBody] SetStandardLoanModel pagenationFilter)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).SetOfficerStandardLoan(Environment, pagenationFilter));
        }
        
        [HttpGet]
        [ActionName("ActivateTeamLead")]
        public async Task<IActionResult> ActivateTeamLeadAsync(string AppsId , string AcctId)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).ActivateManagerTeamLead(Environment, AppsId, AcctId));
        }

        [HttpGet]
        [ActionName("ActivateReconciliationOfficerMember")]
        public async Task<IActionResult> ActivateReconciliationOfficers(string AppsId, string AcctId)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).ActivateReconciliationOfficers(Environment, AppsId, AcctId));
        }

        [HttpGet]
        [ActionName("CheckIfOfficerMember")]
        public async Task<IActionResult> checkIfOfficerMember(string OffFirstName, string OffOther)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).CheckIfOfficerMember1(Environment, OffFirstName, OffOther, OffFirstName));
        }

        [HttpGet]
        [ActionName("GetAllReconcilationMembers")]
        public async Task<IActionResult> AllReconcilationMembers(string AccountId, string AppId)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).AllReconcilationMembers(this.Environment,  AccountId,  AppId));
        }

        [HttpGet]
        [ActionName("GetAllBanksNameLists")]
        public async Task<IActionResult> GetAllBanksNameLists(string Number = "PF0015485")
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).GetAllBanksNameLists(this.Environment));
        }

        [HttpGet]
        [ActionName("GetHubTeam")]
        public async Task<IActionResult> GetHubTeam(string Id)
        {
            return Ok(await new HubTeamsActivity(this, this.Configuration).GetAllPermission(this.Environment, Id));
        }
        
        [HttpPost]
        [ActionName("RetrivetedBankAcctName")]
        public async Task<IActionResult> GetBankAcctName([FromBody] BankAcctNameModel bankAcct)
        {
            return Ok(await new AdminActivitesHelpers(this, this.Configuration).GetBankAcctName(this.Environment, bankAcct));
        }
    }
}