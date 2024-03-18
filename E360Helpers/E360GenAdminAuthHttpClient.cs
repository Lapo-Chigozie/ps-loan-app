using LapoLoanWebApi.E360Helpers.E360DtoModel;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using LapoLoanWebApi.HubTeams;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using Microsoft.EntityFrameworkCore;
using LapoLoanWebApi.EnAndDeHelper;
using System.Configuration;

namespace LapoLoanWebApi.E360Helpers
{
    public class E360GenAdminAuthHttpClient
    {
        private LapoLoanDBContext lapoLoanDB = null;
        private ControllerBase controllerBase { get; set; }
        private E360AuthHttpClient e360AuthHttpClient { get; set; }
        private E360HttpAccessTokenClient AccessTokenClient { get; set; }
        private IConfiguration Configuration;
        public E360GenAdminAuthHttpClient(ControllerBase controllerBase, IConfiguration _configuration)
        {
            this.controllerBase = controllerBase;
            this.Configuration = _configuration;
            this.e360AuthHttpClient = new E360AuthHttpClient(controllerBase, Configuration);
            this.AccessTokenClient = new E360HttpAccessTokenClient(controllerBase, Configuration);

            this.lapoLoanDB = new LapoLoanDBContext(_configuration);

            

           
        }

        public async Task<dynamic> CallRegisterAsync(string Username, string Password)
        {
            try
            {
                if (Username.ToLower().Equals("12213".ToLower()) && Username.ToLower().Equals("SN12213".ToLower()))
                {
                    var newE360Login = new AcctSignInModel()
                    {
                        EmailAddress = Username,
                        Password = Password,
                        RememberMe = true
                    };

                    var IsE360LoginSuccess = await this.e360AuthHttpClient.SignInToE360(newE360Login);

                    if (IsE360LoginSuccess == null || IsE360LoginSuccess.IsActive == false)
                    {
                        return new { Status = 0, Message = IsE360LoginSuccess.TryCatchMessage };
                    }

                    var E360Details = IsE360LoginSuccess.DataLoad as E360AuthLoginRespondsDto;

                    if (E360Details == null || IsE360LoginSuccess.DataLoad == null)
                    {
                        return new { Status = 0, Message = IsE360LoginSuccess.TryCatchMessage };
                    }
                   
                    var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == E360Details.UserRef).FirstOrDefaultAsync();

                    if (UserExit != null)
                    {
                        return new { Status = 1, Message = IsE360LoginSuccess.TryCatchMessage };
                    }

                    var newNewHubTeam = new NewHubTeamMember()
                    {
                        EnterEmailAddress = E360Details.Email,
                        EnterFirstName = E360Details.UserFirstName,
                        EnterLastName = E360Details.UserLastName,
                        EnterMiddleName = E360Details.UserLastName,
                        EnterPhoneNumber = "", // newHubTeam.StaffID,
                        TeamMemberOfficeAddress = "40, Okporo Road, Opposite Ecobank, Rumuodara, Port-Harcourt, Rivers State",
                        UserType = AccountType.Adiministration,
                        EnterTeamMemberID = E360Details.UserRef,
                        CreatedByAccountId = 1,
                        SelectHubGroupId = "GROUP HEAD",
                        SelectHubGroupIdd = Password, /// 40, Okporo Road, Opposite Ecobank, Rumuodara, Port-Harcourt, Rivers State

                        AccessRightToAnonymousLoanApplication = true,
                        AccessRighttoapprovecustomerloan = true,
                        AccessRighttocreateahub = true,
                        AccessRighttocreateateammember = true,
                        AccessRighttoviewtenure = true,
                        AccessRightToViewUploadBackRepaymentLoan = true,
                        AccessRighttoviewveammembers = true,
                        CreateLoanNarration = true,
                        ViewLoanNarration = true,
                        AccessRighttocreatetenure = true,
                        AccessRighttodisablecustomerstoapplyforaloan = true,
                        AccessRighttodisablehubs = true,
                        AccessRightToEditTeamMemberPermissions = true,
                        AccessRightToExportDISBURSEMENTLoan = true,
                        AccessRighttoloansettings = true,
                        AccessRightToPrintLoan = true,
                        AccessRightToProceedLoan = true,
                        AccessRighttorejectaloan = true,
                        AccessRighttoteamsAndpermissions = true,
                        AccessRightToUploadBackDISBURSEMENTLoan = true,
                        AccessRightToUploadBackRepaymentLoan = true,
                        AccessRighttoviewcustomers = true,
                        AccessRighttoviewcustomersloans = true,
                        AccessRightToViewDisbursementLoan = true,
                        AccessRighttoviewhubs = true,
                        AccessRighttoviewloandetails = true
                    };

                    var StaffData = "{\"EnterTeamMemberID\":\"SN0001\",\"EnterLastName\":\"Ojo\",\"EnterMiddleName\":\"Sunday\",\"EnterFirstName\":\"Olorunmo\",\"SelectHubGroupId\":\"MTQ=\",\"EnterPhoneNumber\":\"\",\"EnterEmailAddress\":\"\",\"CreatedByAccountId\":107,\"TeamMemberOfficeAddress\":\"40, Okporo Road, Opposite Ecobank, Rumuodara, Port-Harcourt, Rivers State\",\"UserType\":\"GROUP HEAD\",\"SelectHubGroupIdd\":\"\",\"AccessRightToEditTeamMemberPermissions\":true,\"AccessRightToViewDisbursementLoan\":true,\"AccessRightToViewUploadBackRepaymentLoan\":true,\"AccessRightToExportDISBURSEMENTLoan\":true,\"AccessRightToAnonymousLoanApplication\":true,\"AccessRightToUploadBackDISBURSEMENTLoan\":true,\"AccessRightToUploadBackRepaymentLoan\":true,\"AccessRightToPrintLoan\":true,\"AccessRightToProceedLoan\":false,\"ViewLoanNarration\":true,\"CreateLoanNarration\":true,\"AccessRighttodisablecustomerstoapplyforaloan\":true,\"AccessRighttoviewcustomers\":true,\"AccessRighttodisablehubs\":true,\"AccessRighttoviewtenure\":true,\"AccessRighttocreatetenure\":true,\"AccessRighttoloansettings\":true,\"AccessRighttoteamsAndpermissions\":true,\"AccessRighttorejectaloan\":true,\"AccessRighttoviewcustomersloans\":true,\"AccessRighttoapprovecustomerloan\":true,\"AccessRighttoviewveammembers\":true,\"AccessRighttocreateateammember\":true,\"AccessRighttoviewhubs\":true,\"AccessRighttocreateahub\":true,\"AccessRighttoviewloandetails\":true}";

                    var json1 = JsonConvert.DeserializeObject<NewHubTeamMember>(JsonConvert.SerializeObject(newNewHubTeam));

                    var DataResult = await new HubTeamsActivity(this.controllerBase,  this.Configuration).RespondHubMember(null, json1);

                    if (DataResult != null && DataResult.status == Status.Success && DataResult.IsActive)
                    {
                        return new { Status = 2, Message = DataResult.Message };
                    }

                    return new { Status = 0, Message = new DefalutToken(Configuration).CheckInternetConnection() };
                }

                return new { Status = 1, Message = "This Staff ID is not a valid staff for this process" };
            }
            catch (Exception ex)
            {
                return new { Status = 0,  Message = ex.Message ?? ex.InnerException.Message };
            }
        }
    }
}
