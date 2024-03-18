using LapoLoanWebApi.E360Helpers.E360DtoModel;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.ModelDto.PagenationFilterModel;
using LapoLoanWebApi.ModelDto.PagenationHelper;
using LapoLoanWebApi.TwoFactorsHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Renci.SshNet;
using System.Globalization;
using System.Security.Permissions;

namespace LapoLoanWebApi.E360Helpers
{
    public class E360AuthHttpValidation
    {
        private ControllerBase controllerBase { get; set; }

        private LapoLoanDBContext lapoLoanDB = null;

        private LapoCipher02 lapoCipher02 { get; set; }
        private LapoCipher02 ReallapoCipher02 { get; set; }
        private LapoCipher00 lapoCipher00 { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }

        private E360AuthHttpClient e360AuthHttpClient { get; set; }
        private IConfiguration _configuration;

        public E360AuthHttpValidation(ControllerBase controllerBase, IConfiguration configuration) 
        {
            this._configuration = configuration;
            this.controllerBase = controllerBase;
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.e360AuthHttpClient = new E360AuthHttpClient(controllerBase, _configuration);

            this.ReallapoCipher02 = new LapoCipher02();
            this.lapoCipher02 = new LapoCipher02();
            this.lapoCipher00 = new LapoCipher00();
            this.lapoCipher01 = new LapoCipher01();
        }

        public async Task<RespondMessageDto> RegisterStaff(E360RegisterModelDto  e360Register)
        {
            try
            {
                if (e360Register == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(e360Register.Staff_ID) || string.IsNullOrWhiteSpace(e360Register.Staff_ID))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(e360Register.PhoneNumber) || string.IsNullOrWhiteSpace(e360Register.PhoneNumber))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff phone number is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (e360Register.PhoneNumber.Length < 11 || e360Register.PhoneNumber.Length > 11)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff phone number must be 11 digit number", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (e360Register.EmailAddress.Length > 0 && !e360Register.EmailAddress.Contains("@"))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff email address is invalid", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == e360Register.CreatingStaff_ID).FirstOrDefaultAsync();

                if(HasPermission == null && e360Register.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToCreatedStaff.Value == false && e360Register.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == e360Register.Staff_ID).AnyAsync();

                var SecurityPerm = await lapoLoanDB.SecurityPermissions.Where(x => x.UserId == e360Register.Staff_ID).AnyAsync();

                if(UserExit || SecurityPerm)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID is already exiting", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var newAdmin = new AcctDto()
                {
                    AccountType = AccountType.Adiministration,
                    AllowLoginTwoFactor = true,
                    CreatedDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Username = e360Register.Staff_ID,
                    Status = StatusMgs.Active,
                };

                var Pwd = HashSalt.HashPassword("Admin123!!");
                newAdmin.Password = Pwd;

                var newPerson = new PersonDto()
                {
                    Age = 25,
                    AltPhoneNumber = e360Register.PhoneNumber,
                    CreatedDate = DateTime.Now,
                    CurrentAddress = "12 Aguda Ogba Rd, Lagos Nigeria",
                    EmailAddress = e360Register.EmailAddress.Replace(" ", "") ?? newAdmin.Username.Replace(" ", ""),
                    FirstName = "Lapo Admin",
                    LastName = "Lapo Admin",
                    MiddleName = "Lapo Admin",
                    Gender = "Male",
                    PhoneNumber = e360Register.PhoneNumber.Replace(" ", ""),
                    PositionOrRole = AccountType.Adiministration,
                    Staff = AccountType.Adiministration,
                };

                var newSecurityPer1 = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.SecurityAccount()
                {
                    Status = StatusMgs.NotActive,
                    CreatedDate = DateTime.Now,
                    AccountType = newAdmin.AccountType,
                    AllowLoginTwoFactor = true,
                    AccountVerify = false,
                    LastLoginDate = DateTime.Now,
                    Username = e360Register.Staff_ID,
                    Password = Pwd,
                };

                if (e360Register.IsActive)
                {
                    newSecurityPer1.Status = StatusMgs.Active;
                }

                lapoLoanDB.SecurityAccounts.Add(newSecurityPer1);
                await lapoLoanDB.SaveChangesAsync();

                var new1Person = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.Person()
                {
                    Gender = newPerson.Gender,
                    LastName = newPerson.LastName,
                    MiddleName = newPerson.MiddleName,
                    FirstName = newPerson.FirstName,
                    AltPhoneNumber = newPerson.AltPhoneNumber,
                    Age = newPerson.Age.ToString(),
                    PhoneNumber = newPerson.PhoneNumber,
                    CreatedDate = DateTime.Now,
                    AccountId = newSecurityPer1.Id,
                    EmailAddress = newPerson.EmailAddress,
                    Staff = newPerson.Staff,
                    PositionOrRole = newPerson.PositionOrRole,
                    CurrentAddress = newPerson.CurrentAddress, 

                };
                lapoLoanDB.People.Add(new1Person);
                await lapoLoanDB.SaveChangesAsync();

                var newSecurityPer = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.SecurityPermission()
                {
                    ActivatedDate = DateTime.Now,
                    UserId = e360Register.Staff_ID,
                    StaffName = newPerson.FirstName + ", " + newPerson.LastName,
                    AccountId = newSecurityPer1.Id,
                    BlockedDate = DateTime.Now,
                    IsBlocked = false,
                    Status = StatusMgs.Error,
                    CreatedDate = DateTime.Now,
                    HasPermissionToCreatedStaff = e360Register.IsAccessRightCreatePermission,
                    HasPermissionToDisableStaff = e360Register.IsAccessRightActivatorPermission,
                    IsSupperAdmin = false,
                    AccessRightApprovedLoan = e360Register.IsStaffsLoanPermissionAccessRight,
                    TenureAccessRight = e360Register.IsStaffsLoanTenurePermissionAccessRight,
                    LoanSettingAccessRight = e360Register.IsStaffsLoanSettingsPermissionAccessRight,
                    NetPaysAccessRight = e360Register.IsStaffsNetPaysPermissionAccessRight,
                    LoanCompletedAccessRight = e360Register.IsStaffsCompleteLoanRepaymentPermissionAccessRight,
                    GeneralPermissionsAccessRight = true,
                    CustomerLoanPermission = e360Register.IsStaffsBlockCustomerApplyLoanPermissionAccessRight
                };

                if (e360Register.IsActive)
                {
                    newSecurityPer.Status = StatusMgs.Active;
                }

                lapoLoanDB.SecurityPermissions.Add(newSecurityPer);
                await lapoLoanDB.SaveChangesAsync();

                var data12 = await lapoLoanDB.SecurityAccounts.Where(s => s.Id == newSecurityPer1.Id).FirstOrDefaultAsync();
                if (data12 != null && new1Person!=null)
                {
                    data12.PersonId = new1Person.Id;
                    lapoLoanDB.Entry(data12).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Staff ID has been created and activated", true, null, null, Status.Success, StatusMgs.Success, true);

            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> SmartSignInAcctClient(AcctSignInModel acctSignIn)
        {
            try
            {
                if (acctSignIn == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Enter your login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.Password) || string.IsNullOrWhiteSpace(acctSignIn.Password))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.EmailAddress) || string.IsNullOrWhiteSpace(acctSignIn.EmailAddress))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Email Address or Username is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await new AdminActivitesHelpers(this.controllerBase, _configuration).GetAccount(new AcctSignInDto() { Email = acctSignIn.EmailAddress, Password = acctSignIn.Password });

                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == acctSignIn.EmailAddress && x.Status == StatusMgs.Active).FirstOrDefaultAsync();
                if (UserExit.AccountVerify == null || UserExit.AccountVerify.HasValue == false || UserExit.AccountVerify == false)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Your login account has not been verify, login into your email address " + UserExit.Username + " and clink a verify link to activate your account.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (Result.DataLoad.AccountType == AccountType.Customer)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You account login don't have permission to access the application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    var HashSalt1 = HashSalt.HashPassword(acctSignIn.Password);

                    var ResultLoginHash = HashSalt.VerifyHashedPassword(Result.DataLoad.Password, acctSignIn.Password);

                    if (acctSignIn.Password == lapoCipher01.DecryptString(Result.DataLoad.Password) || ResultLoginHash)
                    {
                        try
                        {
                            // var Id = (long)Convert.ToInt64(Result.DataLoad.Id);
                            var lapoLoan = await lapoLoanDB.SecurityAccounts.Where(s => s.Id == UserExit.Id).FirstOrDefaultAsync();
                            if (lapoLoan != null)
                            {
                                lapoLoan.LastLoginDate = DateTime.Now;
                                lapoLoanDB.Entry(lapoLoan).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }
                        }
                        catch (Exception eu)
                        {

                        }

                        if (Result.DataLoad.AllowLoginTwoFactor)
                        {
                            var TwoFactor = await await new AdminActivitesHelpers(this.controllerBase, _configuration).GenarateTwoFactorAuth(Result.DataLoad.Id);

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, TwoFactor.Data, TwoFactor.DataLoad, Status.Success, StatusMgs.Success, true);
                        }

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success);
                    }
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid username or password", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Failed to sign into your account, Enter valid login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SignInAcctClient([FromBody] AcctSignInModel acctSignIn)
        {
            try
            {
                // var Result1 = await new E360GenAdminAuthHttpClient(this.controllerBase, this._configuration).CallRegisterAsync(acctSignIn.EmailAddress, acctSignIn.Password);
                //if(Result1.Status == 0)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, Result1.Message, false, Result1.Message, Result1.Message, Status.Ërror, StatusMgs.Error);
                //}

                if((acctSignIn.EmailAddress == "LoanAppAdmin@Lapo.com.ng" || acctSignIn.EmailAddress.ToLower() == "SN0001".ToLower()) && DefalutToken.IsProduction == true)
                {
                    return await SmartSignInAcctClient(acctSignIn);
                }
              
                if (acctSignIn == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Enter your login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.Password) || string.IsNullOrWhiteSpace(acctSignIn.Password))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.EmailAddress) || string.IsNullOrWhiteSpace(acctSignIn.EmailAddress))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var newE360Login = new AcctSignInModel()
                {
                    EmailAddress = acctSignIn.EmailAddress,
                    Password = acctSignIn.Password,
                    RememberMe = true
                };

                var IsE360LoginSuccess = await this.e360AuthHttpClient.SignInToE360(newE360Login);

                if (IsE360LoginSuccess == null || IsE360LoginSuccess.IsActive == false)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, IsE360LoginSuccess.TryCatchMessage, false, null, null, Status.Ërror, StatusMgs.Error);
                }

                var E360Details = IsE360LoginSuccess.DataLoad as E360AuthLoginRespondsDto;

                if (E360Details == null || IsE360LoginSuccess.DataLoad == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, IsE360LoginSuccess.TryCatchMessage, false, null, null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await new AdminActivitesHelpers(this.controllerBase, _configuration).GetAccount(new AcctSignInDto() { Email = acctSignIn.EmailAddress, Password = acctSignIn.Password });

                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => (x.Username == acctSignIn.EmailAddress || x.Username == E360Details.UserRef) && (x.Status == StatusMgs.Active || x.Status == StatusMgs.Approved || x.Status == StatusMgs.Success)).FirstOrDefaultAsync();

                var UserExitPermission = await lapoLoanDB.SecurityPermissions.Where(x => (x.UserId == acctSignIn.EmailAddress || x.UserId == E360Details.UserRef) && (x.Status == StatusMgs.Active || x.Status == StatusMgs.Approved || x.Status == StatusMgs.Success) && x.IsBlocked == true).FirstOrDefaultAsync();

                var HubTeams = await lapoLoanDB.HubTeams.Where(x => (x.HubMemberId == acctSignIn.EmailAddress || x.HubMemberId == E360Details.UserRef) && (x.Status == StatusMgs.Active || x.Status == StatusMgs.Approved || x.Status == StatusMgs.Success) && x.IsBlocked == false).FirstOrDefaultAsync();

                if (UserExit == null || UserExitPermission == null || HubTeams == null)
                {
                    if (E360Details != null)
                    {
                        var Peop = await lapoLoanDB.People.Where(x => x.Staff == E360Details.UserRef).FirstOrDefaultAsync();

                        if (Peop != null)
                        {
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Staff ID or Password is invalid.", false, null, null, Status.Failed, StatusMgs.Failed);
                        }
                    }
                }

                if (UserExitPermission == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Your account login don't have permission to access the application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (UserExitPermission==null || UserExitPermission.Status == StatusMgs.NotActive)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Your account login don't have permission to access the application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (UserExitPermission.IsBlocked.Value == false)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Your account login has been blocked to access this application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (Result.DataLoad != null && Result.DataLoad.AccountType == AccountType.Customer)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Your account login don't have permission to access the application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    if (UserExit != null && UserExit.AccountVerify.HasValue &&  (UserExit.AccountVerify.Value == false || UserExit.AccountVerify.Value == true))
                    {
                        var HashSalt1 = HashSalt.HashPassword(acctSignIn.Password);

                        UserExit.AccountVerify = true;
                        UserExit.Password = HashSalt1;
                        lapoLoanDB.Entry(UserExit).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        Result.DataLoad.Password = HashSalt1;
                    }
                    else
                    {
                        if (UserExit.AccountVerify == null || UserExit.AccountVerify.HasValue == false || UserExit.AccountVerify == false)
                        {
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Your login account has not been verify, login into your email address " + UserExit.Username + " and clink a verify link to activate your account.", false, null, null, Status.Failed, StatusMgs.Failed);
                        }
                    }

                    var ResultLoginHash = HashSalt.VerifyHashedPassword(Result.DataLoad.Password, acctSignIn.Password);

                    if (/*acctSignIn.Password == lapoCipher01.DecryptString(Result.DataLoad.Password) ||*/ ResultLoginHash!=null && ResultLoginHash)
                    {
                        try
                        {
                            var lapoPeople = await lapoLoanDB.People.Where(s => s.AccountId == UserExit.Id).FirstOrDefaultAsync();
                            if (lapoPeople != null && IsE360LoginSuccess.DataLoad!=null)
                            {
                                if (E360Details != null)
                                {
                                    UserExitPermission.StaffName = E360Details.UserFirstName + ", " + E360Details.UserLastName;
                                    UserExitPermission.StattId = E360Details.UserID;
                                    UserExitPermission.AccountId = UserExit.Id;
                                    lapoLoanDB.Entry(UserExitPermission).State = EntityState.Modified;
                                    await lapoLoanDB.SaveChangesAsync();

                                    lapoPeople.PositionOrRole = E360Details.Role;
                                    lapoPeople.Staff = E360Details.UserRef;
                                    lapoPeople.FirstName = E360Details.UserFirstName;
                                    lapoPeople.LastName = E360Details.UserLastName;
                                    lapoPeople.MiddleName = E360Details.UserLastName;
                                    lapoPeople.EmailAddress = E360Details.Email;
                                    lapoPeople.UserRole = E360Details.UserRole;
                                    lapoLoanDB.Entry(lapoPeople).State = EntityState.Modified;
                                    await lapoLoanDB.SaveChangesAsync();
                                }
                            }

                            var Id = (long)Convert.ToInt64(Result.DataLoad.Id);
                            var lapoLoan = await lapoLoanDB.SecurityAccounts.Where(s => s.Id == Id).FirstOrDefaultAsync();
                            if (lapoLoan != null)
                            {
                                lapoLoan.Username = acctSignIn.EmailAddress;
                                lapoLoan.LastLoginDate = DateTime.Now;
                                lapoLoanDB.Entry(lapoLoan).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }

                            if (UserExitPermission != null)
                            {
                                UserExitPermission.UserId = acctSignIn.EmailAddress;
                                lapoLoanDB.Entry(UserExitPermission).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }

                            if (HubTeams != null)
                            {
                                HubTeams.HubMemberId = acctSignIn.EmailAddress;
                                lapoLoanDB.Entry(HubTeams).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }
                        }
                        catch (Exception eu)
                        {

                        }

                        if (Result.DataLoad.AllowLoginTwoFactor)
                        {
                            var TwoFactor = await new AdminActivitesHelpers(this.controllerBase, _configuration).GenarateTwoFactorAuth(Result.DataLoad.Id);

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, TwoFactor.Data, TwoFactor.DataLoad, Status.Success, StatusMgs.Success, true);
                        }

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success, false);
                    }
                }
                else
                {
                    var Password = "";

                    if (UserExit != null && UserExit.AccountVerify.HasValue && (UserExit.AccountVerify.Value == false || UserExit.AccountVerify.Value == true))
                    {
                        var HashSalt1 = HashSalt.HashPassword(acctSignIn.Password);

                        UserExit.AccountVerify = true;
                        UserExit.Password = HashSalt1;
                        lapoLoanDB.Entry(UserExit).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                       Password = HashSalt1;
                    }
                    else
                    {
                        if (UserExit.AccountVerify == null || UserExit.AccountVerify.HasValue == false || UserExit.AccountVerify == false)
                        {
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Your login account has not been verify, login into your email address " + UserExit.Username + " and clink a verify link to activate your account.", false, null, null, Status.Failed, StatusMgs.Failed);
                        }
                    }

                    var ResultLoginHash = HashSalt.VerifyHashedPassword(Password, acctSignIn.Password);

                    if (/*acctSignIn.Password == lapoCipher01.DecryptString(Result.DataLoad.Password) ||*/ ResultLoginHash != null && ResultLoginHash)
                    {
                        try
                        {
                            var lapoPeople = await lapoLoanDB.People.Where(s => s.AccountId == UserExit.Id).FirstOrDefaultAsync();
                            if (lapoPeople != null && IsE360LoginSuccess.DataLoad != null)
                            {
                                if (E360Details != null)
                                {
                                    UserExitPermission.StaffName = E360Details.UserFirstName + ", " + E360Details.UserLastName;
                                    UserExitPermission.StattId = E360Details.UserID;
                                    UserExitPermission.AccountId = UserExit.Id;
                                    lapoLoanDB.Entry(UserExitPermission).State = EntityState.Modified;
                                    await lapoLoanDB.SaveChangesAsync();

                                    lapoPeople.PositionOrRole = E360Details.Role;
                                    lapoPeople.Staff = E360Details.UserRef;
                                    lapoPeople.FirstName = E360Details.UserFirstName;
                                    lapoPeople.LastName = E360Details.UserLastName;
                                    lapoPeople.MiddleName = E360Details.UserLastName;
                                    lapoPeople.EmailAddress = E360Details.Email;
                                    lapoPeople.UserRole = E360Details.UserRole;
                                    lapoLoanDB.Entry(lapoPeople).State = EntityState.Modified;
                                    await lapoLoanDB.SaveChangesAsync();

                                }
                            }

                            var Id = UserExit.Id;
                            var lapoLoan = await lapoLoanDB.SecurityAccounts.Where(s => s.Id == Id).FirstOrDefaultAsync();
                            if (lapoLoan != null)
                            {
                                lapoLoan.Username = acctSignIn.EmailAddress;
                                lapoLoan.LastLoginDate = DateTime.Now;
                                lapoLoanDB.Entry(lapoLoan).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }

                            if (UserExitPermission != null)
                            {
                                UserExitPermission.UserId = acctSignIn.EmailAddress;
                                lapoLoanDB.Entry(UserExitPermission).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }

                            if (HubTeams != null)
                            {
                                HubTeams.HubMemberId = acctSignIn.EmailAddress;
                                lapoLoanDB.Entry(HubTeams).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }
                        }
                        catch (Exception eu)
                        {

                        }

                        if (UserExit.AllowLoginTwoFactor.HasValue && UserExit.AllowLoginTwoFactor.Value)
                        {
                            var TwoFactor = await new AdminActivitesHelpers(this.controllerBase, _configuration).GenarateTwoFactorAuth(UserExit.Id);

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, TwoFactor.Data, TwoFactor.DataLoad, Status.Success, StatusMgs.Success, true);
                        }

                      return await this.SignInAcctClient(acctSignIn);
                      //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success, false);
                    }
                    else
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid Staff ID or password", false, "", null, Status.Ërror, StatusMgs.Error);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Failed to sign into your account, Enter valid login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex.Message ?? ex.InnerException.Message, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetStaffList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                var AcctIdd = Convert.ToInt32(pagenationFilter.AccountId.ToString());
                //var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var LoanMethodList = new List<E360StaffModel>();

                var Clients = await lapoLoanDB.SecurityPermissions.ToListAsync();

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (pagenationFilter.IsSearchBar)
                {
                    double Amount = 0;

                    try
                    {
                        Amount = Convert.ToDouble(pagenationFilter.searchText);
                    }
                    catch (Exception exs)
                    {

                    }

                    if (pagenationFilter.status.Equals("All"))
                    {
                        Clients = await lapoLoanDB.SecurityPermissions.Where(n => /*n.AccountId == acc &&*/ ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.ActivatedDate >= fromDate && n.ActivatedDate <= toDate)) && (n.UserId.Equals(pagenationFilter.searchText) || n.StaffName.Equals(pagenationFilter.searchText))).ToListAsync();
                    }
                    else
                    {
                        Clients = await lapoLoanDB.SecurityPermissions.Where(n => /*n.AccountId == acc &&*/ ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.ActivatedDate >= fromDate && n.ActivatedDate <= toDate)) && n.Status == pagenationFilter.status && (n.UserId.Equals(pagenationFilter.searchText) || n.StaffName.Equals(pagenationFilter.searchText))).ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        Clients = await lapoLoanDB.SecurityPermissions.Where(n => /*n.AccountId == acc &&*/ ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.ActivatedDate >= fromDate && n.ActivatedDate <= toDate))).ToListAsync();
                    }
                    else
                    {
                        Clients = await lapoLoanDB.SecurityPermissions.Where(n => /*n.AccountId == acc &&*/ ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.ActivatedDate >= fromDate && n.ActivatedDate <= toDate)) && n.Status == pagenationFilter.status).ToListAsync();
                    }
                }

                if (Clients == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No permission was found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                Clients = Clients.OrderByDescending(s => s.CreatedDate).OrderByDescending(s => s.Status == "Active").ToList();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenures = new E360StaffModel()
                        {
                            CreatedDate = itm.CreatedDate.Value.ToShortDateString(),
                            Id = lapoCipher01.EnryptString(itm.Id.ToString()),
                            IsBlocked = itm.IsBlocked.Value,
                            Staff_Id = itm.UserId,
                            Staff_Name = itm.StaffName,
                            Status = itm.Status, 
                            AccesstoActivateStaff = itm.HasPermissionToCreatedStaff.Value,
                            AccesstoCreatedStaff = itm.HasPermissionToDisableStaff.Value,
                        };

                        var ClientPeople = await lapoLoanDB.People.Where(x=>x.AccountId == itm.AccountId).FirstOrDefaultAsync();

                        if (ClientPeople != null)
                        {
                            newTenures.Staff_Name = ClientPeople.FirstName;
                        }

                        if (itm.IsSupperAdmin.Value)
                        {
                            newTenures.StaffLevel = "Supper Admin";
                        }
                        else
                        {
                            newTenures.StaffLevel = "Admin";
                        }

                        var ClientPerson = await lapoLoanDB.People.Where(x=>x.AccountId == itm.AccountId).FirstOrDefaultAsync();
                        if (ClientPerson != null)
                        {
                            newTenures.Staff_Role = ClientPerson.PositionOrRole;
                        }
                        else
                        {
                            newTenures.Staff_Role = "Admin";
                        }

                        LoanMethodList.Add(newTenures);
                    }

                    if (LoanMethodList != null && (LoanMethodList.Count >= 0 || LoanMethodList.Count <= 0))
                    {
                        //var data = new List<LoanApp>();

                        //    foreach (var Iky in newLoanAppList)
                        //    {
                        //        no += 1;
                        //        var newLoanApp1 = new LoanApp()
                        //        {
                        //            AccountId = Iky.AccountId,
                        //            Amount = Iky.Amount,
                        //            CreatedDate1 = Iky.CreatedDate1,
                        //            Gender = Iky.Gender,
                        //            Name = Iky.Name,
                        //            IPPISNumber = Iky.IPPISNumber,
                        //            RequestCode = Iky.RequestCode,
                        //            Status = Iky.Status,
                        //            HeaderId = Iky.HeaderId,
                        //            createdDate = Iky.createdDate,
                        //            No = no.ToString()
                        //        };
                        //        data.Add(newLoanApp1);
                        //    }


                        var Pagenation = await new PagenationsHelper().GetPagenation<E360StaffModel>(LoanMethodList, pagenationFilter);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Staff ID was found in the Staffs records.", false, "", null, Status.Ërror, StatusMgs.Error);

                  //  return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Staff ID is invalid.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Staff ID was found in the Staffs records.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateE360Staff(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.IsBlocked.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to stop this staff not to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (Person != null && Client.Status == StatusMgs.Active)
                    {

                        if (Client.IsSupperAdmin.HasValue && Client.IsSupperAdmin == true)
                        {
                            Client.HasPermissionToDisableStaff = true;
                            Client.GeneralPermissionsAccessRight = true;
                            Client.Status = StatusMgs.Active;
                            Client.ActivatedDate = DateTime.Now;
                            Client.IsSupperAdmin = true;
                            Client.HasPermissionToDisableStaff = true;
                            Client.HasPermissionToCreatedStaff = true;
                            Client.IsBlocked = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }


                        Client.Status = StatusMgs.NotActive;
                        Client.ActivatedDate = DateTime.Now;
                        Client.IsSupperAdmin = false;
                        Client.HasPermissionToDisableStaff = false;
                        Client.HasPermissionToCreatedStaff = false;
                        Client.IsBlocked = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your login account on lapo loan has been de-activated.";
                        await new TwoFactorsHelper(this._configuration).AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This staff login has been de-activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Person != null && Client.Status == StatusMgs.NotActive)
                    {
                        Client.Status = StatusMgs.Active;
                        Client.ActivatedDate = DateTime.Now;
                        Client.IsSupperAdmin = true;
                        Client.HasPermissionToDisableStaff = true;
                        Client.HasPermissionToCreatedStaff = true;
                        Client.IsBlocked = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your login account on lapo loan has been activated.";
                        await new TwoFactorsHelper(this._configuration).AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This staff login has been activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This staff login has been de-activated already.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateE360Staff1(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.AccessRightApprovedLoan.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (Person != null && (Client.AccessRightApprovedLoan ==null || Client.AccessRightApprovedLoan.Value))
                    {
                        if (Client.IsSupperAdmin.HasValue && Client.IsSupperAdmin == true)
                        {
                            Client.AccessRightApprovedLoan = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }

                        Client.AccessRightApprovedLoan = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "You has been de-activated from (Approving, Cancelling and Completing) customers loans.";
                        await new TwoFactorsHelper(this._configuration).AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This Staff has been de-activated from (Approving, Cancelling and Completing) customers loans successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Person != null && Client.AccessRightApprovedLoan.Value == false)
                    {
                        Client.AccessRightApprovedLoan = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your has been activated from (Approving, Cancelling and Completing) customers loans.";
                        await new TwoFactorsHelper(this._configuration).AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This Staff has been activated from (Approving, Cancelling and Completing) customers loans successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This staff access right has been de-activated already.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SetPermissionHasPermissionToDisableStaff(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToDisableStaff.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    if (Client.HasPermissionToDisableStaff ==false)
                    {
                        Client.HasPermissionToDisableStaff = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Permission to activate is successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Client.HasPermissionToDisableStaff == true)
                    {

                        if (Client.IsSupperAdmin.HasValue && Client.IsSupperAdmin == true)
                        {
                            Client.HasPermissionToDisableStaff = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }


                        Client.HasPermissionToDisableStaff = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Permission to de-activate is successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Permission not found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SetPermissionToCreatedStaff(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToCreatedStaff.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    if (Client.HasPermissionToCreatedStaff == false)
                    {

                       

                        Client.HasPermissionToCreatedStaff = true;
                        Client.GeneralPermissionsAccessRight = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Permission activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Client.HasPermissionToCreatedStaff == true)
                    {
                        if (Client.IsSupperAdmin.HasValue && Client.IsSupperAdmin == true)
                        {
                            Client.HasPermissionToCreatedStaff = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }

                        Client.HasPermissionToCreatedStaff = false;
                        Client.GeneralPermissionsAccessRight = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Permission de-activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Permission not found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetProcessGetStaffLoginAccessRight(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, long Id)
        {
            try
            {
               if(Id == 34)
                {
                    var permissinResult = new { HasPermissionToCreatedStaff = true, HasPermissionToDisableStaff = true, SupperAdmin = true, HasPermissionToLoanApprovalStaff = true };

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Permissions was successfully.", true, permissinResult, permissinResult, Status.Success, StatusMgs.Success);
                }

                var ClientPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == Id).FirstOrDefaultAsync();

                if (ClientPermission != null)
                {
                    var permissinResult = new { HasPermissionToCreatedStaff = ClientPermission.HasPermissionToCreatedStaff, HasPermissionToDisableStaff = ClientPermission .HasPermissionToDisableStaff , SupperAdmin = ClientPermission.IsSupperAdmin, HasPermissionToLoanApprovalStaff = ClientPermission.AccessRightApprovedLoan = ClientPermission.AccessRightApprovedLoan == null ? false : ClientPermission.AccessRightApprovedLoan.Value, IsStaffsLoanTenurePermissionAccessRight = ClientPermission.TenureAccessRight.Value, IsStaffsLoanSettingsPermissionAccessRight = ClientPermission.LoanSettingAccessRight.Value, IsStaffsNetPaysPermissionAccessRight = ClientPermission.NetPaysAccessRight.Value, IsStaffsCompleteLoanRepaymentPermissionAccessRight = ClientPermission.LoanCompletedAccessRight.Value, IsStaffsBlockCustomerApplyLoanPermissionAccessRight = ClientPermission.CustomerLoanPermission.Value }; 

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Permissions was successfully.", true, permissinResult, permissinResult, Status.Success, StatusMgs.Success);
                }

                var permissinResult1 = new { HasPermissionToCreatedStaff = false, HasPermissionToDisableStaff = false, SupperAdmin = false, HasPermissionToLoanApprovalStaff = false, IsStaffsLoanTenurePermissionAccessRight = false, IsStaffsLoanSettingsPermissionAccessRight = false, IsStaffsNetPaysPermissionAccessRight = false, IsStaffsCompleteLoanRepaymentPermissionAccessRight = false, IsStaffsBlockCustomerApplyLoanPermissionAccessRight = false };
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Permission not found.", false, permissinResult1, permissinResult1, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                var permissinResult = new { HasPermissionToCreatedStaff = false, HasPermissionToDisableStaff = false, SupperAdmin = false, HasPermissionToLoanApprovalStaff = false, IsStaffsLoanTenurePermissionAccessRight = false, IsStaffsLoanSettingsPermissionAccessRight = false, IsStaffsNetPaysPermissionAccessRight = false, IsStaffsCompleteLoanRepaymentPermissionAccessRight = false, IsStaffsBlockCustomerApplyLoanPermissionAccessRight = false };

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, permissinResult, permissinResult, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetStaffAccessRight(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string Id)
        {
            try
            {
                if (Id == "34")
                {
                    var permissinResult = new { HasPermissionToCreatedStaff = true, HasPermissionToDisableStaff = true, SupperAdmin = true, HasPermissionToLoanApprovalStaff = true, };

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Permissions was successfully.", true, permissinResult, permissinResult, Status.Success, StatusMgs.Success);
                }

                var AccountId = (long)Convert.ToInt64(lapoCipher01.DecryptString(Id));
                var ClientPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.Id == AccountId).FirstOrDefaultAsync();

                if (ClientPermission != null)
                {
                    var permissinResult = new { HasPermissionToCreatedStaff = ClientPermission.HasPermissionToCreatedStaff, HasPermissionToDisableStaff = ClientPermission.HasPermissionToDisableStaff, SupperAdmin = ClientPermission.IsSupperAdmin, HasPermissionToLoanApprovalStaff = ClientPermission.AccessRightApprovedLoan == null ? false : ClientPermission.AccessRightApprovedLoan .Value  , IsStaffsLoanTenurePermissionAccessRight  = ClientPermission.TenureAccessRight.Value, IsStaffsLoanSettingsPermissionAccessRight  = ClientPermission.LoanSettingAccessRight.Value, IsStaffsNetPaysPermissionAccessRight = ClientPermission.NetPaysAccessRight.Value, IsStaffsCompleteLoanRepaymentPermissionAccessRight = ClientPermission.LoanCompletedAccessRight.Value, IsStaffsBlockCustomerApplyLoanPermissionAccessRight = ClientPermission .CustomerLoanPermission.Value };


                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Permissions was successfully.", true, permissinResult, permissinResult, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Permission not found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }


        public async Task<RespondMessageDto> ActivateE360Staff2(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToDisableStaff.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (Person != null && (Client.LoanCompletedAccessRight == null || Client.LoanCompletedAccessRight.Value) && (Client.IsSupperAdmin == null || Client.IsSupperAdmin == false || Client.IsSupperAdmin == true))
                    {

                        if (Client.IsSupperAdmin.HasValue && Client.IsSupperAdmin == true)
                        {
                            Client.NetPaysAccessRight = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }

                        Client.LoanCompletedAccessRight = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "You has been de-activated from (Approving, Cancelling and Completing) customers loans.";
                     //   await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right de-activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Person != null && (Client.LoanCompletedAccessRight == null || Client.LoanCompletedAccessRight.Value == false))
                    {
                        Client.LoanCompletedAccessRight = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your has been activated from (Approving, Cancelling and Completing) customers loans.";
                        // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This staff access right has been de-activated already.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateE360Staff3(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToDisableStaff.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (Person != null && (Client.CustomerLoanPermission == null || Client.CustomerLoanPermission.Value) && (Client.IsSupperAdmin == null || Client.IsSupperAdmin == false || Client.IsSupperAdmin == true))
                    {

                        if (Client.IsSupperAdmin == true)
                        {
                            Client.NetPaysAccessRight = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }


                        Client.CustomerLoanPermission = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "You has been de-activated from (Approving, Cancelling and Completing) customers loans.";
                        // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right de-activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Person != null && (Client.CustomerLoanPermission ==null ||Client.CustomerLoanPermission.Value == false))
                    {
                        Client.CustomerLoanPermission = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your has been activated from (Approving, Cancelling and Completing) customers loans.";
                        // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This staff access right has been de-activated already.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateE360Staff4(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToDisableStaff.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (Person != null && (Client.NetPaysAccessRight == null || Client.NetPaysAccessRight.Value) && (Client.IsSupperAdmin == null || Client.IsSupperAdmin == false || Client.IsSupperAdmin == true))
                    {
                        if (Client.IsSupperAdmin == true)
                        {
                            Client.NetPaysAccessRight = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }




                        Client.NetPaysAccessRight = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "You has been de-activated from (Approving, Cancelling and Completing) customers loans.";
                       // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right de-activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Person != null && (Client.NetPaysAccessRight == null ||Client.NetPaysAccessRight.Value == false))
                    {
                        Client.NetPaysAccessRight = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your has been activated from (Approving, Cancelling and Completing) customers loans.";
                        // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This staff access right has been de-activated already.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateE360Staff5(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToDisableStaff.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (Person != null && (Client.LoanSettingAccessRight == null || Client.LoanSettingAccessRight.Value) && (Client.IsSupperAdmin == null || Client.IsSupperAdmin == false || Client.IsSupperAdmin == true))
                    {

                        if (Client.IsSupperAdmin == true)
                        {
                            Client.LoanSettingAccessRight = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }




                        Client.LoanSettingAccessRight = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "You has been de-activated from (Approving, Cancelling and Completing) customers loans.";
                       // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right de-activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Person != null && (Client.LoanSettingAccessRight==null || Client.LoanSettingAccessRight.Value == false))
                    {

                       


                        Client.LoanSettingAccessRight = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your has been activated from (Approving, Cancelling and Completing) customers loans.";
                        // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This staff access right has been de-activated already.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateE360Staff6(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == loanAppRequest.CreatingStaff_ID).FirstOrDefaultAsync();

                if (HasPermission == null && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null && HasPermission.HasPermissionToDisableStaff.Value == false && loanAppRequest.CreatingStaff_ID != 34)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't have permission in this Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.SecurityPermissions.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (Person != null && (Client.TenureAccessRight == null || Client.TenureAccessRight.Value) && (Client.IsSupperAdmin == null || Client.IsSupperAdmin == false || Client.IsSupperAdmin == true))
                    {

                        Client.TenureAccessRight = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "You has been de-activated from (Approving, Cancelling and Completing) customers loans.";
                        // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right de-activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else if (Person != null && (Client.TenureAccessRight == null || Client.TenureAccessRight.Value == false))
                    {


                        if (Client.IsSupperAdmin == true)
                        {
                            Client.TenureAccessRight = true;
                            Client.GeneralPermissionsAccessRight = true;
                            lapoLoanDB.Entry(Client).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "This account is a Supper Admin Team Member and can not be de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                        }


                        Client.TenureAccessRight = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        //var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "Your has been activated from (Approving, Cancelling and Completing) customers loans.";
                        // await new TwoFactorsHelper().AdminSendLoanSmsAsync(_environment, message, Client.AccountId ?? Person.AccountId);

                        //await EmailHelpers.SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, DefalutToken.ClientLogin, loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Access Right activated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This staff access right has been de-activated already.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
    }
}
