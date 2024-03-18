//using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml;
using Google.Protobuf.WellKnownTypes;
using LapoLoanWebApi.BankClientHelper;
using LapoLoanWebApi.BVNHelpers;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanScafFoldModel;
using LapoLoanWebApi.LoanScafFoldModel.SqlStoreProcedureCmmdHelper;
using LapoLoanWebApi.LoanScheduled;
using LapoLoanWebApi.LoanScheduled.Model;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.ModelDto.PagenationFilterModel;
using LapoLoanWebApi.ModelDto.PagenationHelper;
using LapoLoanWebApi.TwoFactorsHelpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore.Encrypt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
//using static Google.Protobuf.Collections.MapField<TKey, TValue>;
//using Microsoft.Data.SqlClient;
//using System.Data.SqlClient;
//using System.Data.SQLite;

namespace LapoLoanWebApi.LapoRepositoryHelper
{
    public class AdminActivitesHelpers
    {
        private LapoDBConnectionStrings LapoDBConnectionStrings { get; set; } = null;
        private System.Data.SqlClient.SqlConnection SqlConn = null;
        private Microsoft.Data.SqlClient.SqlConnection MsSQlConn = null;
        private System.Data.SQLite.Linq.SQLiteProviderFactory SqlLite = null;

        private LapoLoanDBContext lapoLoanDB = null;
        private LapoCipher02 lapoCipher02 { get; set; } = null;
        private LapoCipher02 ReallapoCipher02 { get; set; } = null;
        private LapoCipher00 lapoCipher00 { get; set; } = null;
        private LapoCipher01 lapoCipher01 { get; set; } = null;
        private ControllerBase ControllerBase { get; set; } = null;

        private IConfiguration _configuration { get; set; } = null;

      
        public AdminActivitesHelpers(ControllerBase controllerBase, IConfiguration configuration)
        {
            this.ControllerBase = controllerBase;
            this._configuration = configuration;

            this.ReallapoCipher02 = new LapoCipher02();
            this.lapoCipher02 = new LapoCipher02();
            this.lapoCipher00 = new LapoCipher00();
            this.lapoCipher01 = new LapoCipher01();

            this.lapoLoanDB = new LapoLoanDBContext(_configuration);
            this.LapoDBConnectionStrings = new LapoDBConnectionStrings(_configuration);

            this.SqlConn = new System.Data.SqlClient.SqlConnection(LapoDBConnectionStrings.ConnectionString());
            this.MsSQlConn = new Microsoft.Data.SqlClient.SqlConnection(LapoDBConnectionStrings.ConnectionString());
            this.SqlLite = new System.Data.SQLite.Linq.SQLiteProviderFactory();

        }

        private DataTable dataTable = new DataTable();
        private DataSet dataSet = new DataSet();

        private async Task<RespondMessageDto> HasExitingAccount()
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.ValidateIfHasCreatedAcct, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@CurrentDateTime", DateTime.Now);
                var SqlReader = await sqlCmd.ExecuteReaderAsync();

                //var SqldataAdapter = new System.Data.SqlClient.SqlDataAdapter(sqlCmd);

                //SqldataAdapter.Fill(dataTable);
                //SqldataAdapter.Fill(dataSet);

                //dataTable = new DataTable();
                //dataTable.Load(await sqlCmd.ExecuteReaderAsync());

                //dataSet = new DataSet(); //conn is opened by dataadapter
                //SqldataAdapter.Fill(dataSet);

                if (SqlReader.HasRows)
                {
                    while (await SqlReader.ReadAsync())
                    {
                        if (await SqlReader.ReadAsync())
                        {
                            var Id = Convert.ToUInt32(SqlReader["Id"].ToString());
                            if (Id > 0)
                            {

                                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Success", true, Id, Id, Status.Success, StatusMgs.Success);

                            }
                        }
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> SaveTwoFactorAuth(SaveTwoFactorAuthDto saveTwoFactorAuth)
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.RegisterTwoFactorAuthRegister, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@AccountId", saveTwoFactorAuth.AccountId);
                sqlCmd.Parameters.AddWithValue("@Active", saveTwoFactorAuth.Active);
                sqlCmd.Parameters.AddWithValue("@ExpiredDateTime", saveTwoFactorAuth.ExpiredDateTime);
                sqlCmd.Parameters.AddWithValue("@BVNVerification", saveTwoFactorAuth.BVNVerification);
                sqlCmd.Parameters.AddWithValue("@GenaratedDateTime", saveTwoFactorAuth.GenaratedDateTime);
                sqlCmd.Parameters.AddWithValue("@CreatedDateTime", saveTwoFactorAuth.CreatedDateTime);
                sqlCmd.Parameters.AddWithValue("@CodeGenerated", saveTwoFactorAuth.CodeGenerated);

                var SqlReader = await sqlCmd.ExecuteReaderAsync();
                ReturnTwoFactorAuthDto newAcct = null;

                if (SqlReader.HasRows)
                {
                    newAcct = new ReturnTwoFactorAuthDto();

                    while (SqlReader.Read())
                    {
                        newAcct.Code = Convert.ToString(SqlReader["Code"].ToString());
                        newAcct.BVNVerification = Convert.ToString(SqlReader["BVNVerification"].ToString());
                        newAcct.IsActivexpired = Convert.ToBoolean(SqlReader["IsActive"].ToString());
                        newAcct.CreatedDate = Convert.ToDateTime(SqlReader["CreatedDate"].ToString());
                        newAcct.ExpiredDateTime = Convert.ToDateTime(SqlReader["ExpiredDateTime"].ToString());
                        newAcct.GenaratedDateTime = Convert.ToDateTime(SqlReader["GenaratedDateTime"].ToString());
                        newAcct.Id = Convert.ToInt64(SqlReader["Id"].ToString());
                        newAcct.AccountId = Convert.ToInt64(SqlReader["AccountId"].ToString());
                        newAcct.IsTwoFactorAuth = true;

                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Success", true, newAcct, newAcct, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, newAcct, newAcct, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ValidateOTPUser(string AcctId, string Code)
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.ValidateOTPUser, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@AccountId", AcctId);
                sqlCmd.Parameters.AddWithValue("@Code", Code);

                var SqlReader = await sqlCmd.ExecuteReaderAsync();
                ReturnTwoFactorAuthDto newAcct = null;

                if (SqlReader.HasRows)
                {
                    while (SqlReader.Read())
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Success", true, new { Success = true }, new { Success = true }, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, newAcct, newAcct, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetTwoFactorAuth(long AcctId, string Code)
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.VerifyTwoFactorAuthExit, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@AccountId", AcctId);
                sqlCmd.Parameters.AddWithValue("@CodeGenerated", Code);

                var SqlReader = await sqlCmd.ExecuteReaderAsync();
                ReturnTwoFactorAuthDto newAcct = null;

                if (SqlReader.HasRows)
                {
                    newAcct = new ReturnTwoFactorAuthDto();

                    while (SqlReader.Read())
                    {
                        newAcct.Code = Convert.ToString(SqlReader["Code"].ToString());
                        newAcct.BVNVerification = Convert.ToString(SqlReader["BVNVerification"].ToString());
                        newAcct.IsActivexpired = Convert.ToBoolean(SqlReader["IsActive"].ToString());
                        newAcct.CreatedDate = Convert.ToDateTime(SqlReader["CreatedDate"].ToString());
                        newAcct.ExpiredDateTime = Convert.ToDateTime(SqlReader["ExpiredDateTime"].ToString());
                        newAcct.GenaratedDateTime = Convert.ToDateTime(SqlReader["GenaratedDateTime"].ToString());
                        newAcct.Id = Convert.ToInt64(SqlReader["Id"].ToString());
                        newAcct.AccountId = Convert.ToInt64(SqlReader["AccountId"].ToString());
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Success", true, newAcct, newAcct, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, newAcct, newAcct, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> CreateAccount(AcctDto acctDto, PersonDto personDto)
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.CreateAdminAccount, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                sqlCmd.Parameters.AddWithValue("@Password", acctDto.Password);
                sqlCmd.Parameters.AddWithValue("@AllowLoginTwoFactor", acctDto.AllowLoginTwoFactor);
                sqlCmd.Parameters.AddWithValue("@LastLoginDate", acctDto.LastLoginDate);
                sqlCmd.Parameters.AddWithValue("@Username", acctDto.Username);
                sqlCmd.Parameters.AddWithValue("@Status", acctDto.Status);
                sqlCmd.Parameters.AddWithValue("@FirstName", personDto.FirstName);
                sqlCmd.Parameters.AddWithValue("@LastName", personDto.LastName);
                sqlCmd.Parameters.AddWithValue("@MiddleName", personDto.MiddleName);
                sqlCmd.Parameters.AddWithValue("@Gender", personDto.Gender);
                sqlCmd.Parameters.AddWithValue("@Staff", personDto.Staff);
                sqlCmd.Parameters.AddWithValue("@PhoneNumber", personDto.PhoneNumber);
                sqlCmd.Parameters.AddWithValue("@EmailAddress", personDto.EmailAddress);
                sqlCmd.Parameters.AddWithValue("@CurrentAddress", personDto.CurrentAddress);
                sqlCmd.Parameters.AddWithValue("@AltPhoneNumber", personDto.AltPhoneNumber);
                sqlCmd.Parameters.AddWithValue("@Age", personDto.Age);
                sqlCmd.Parameters.AddWithValue("@PositionOrRole", personDto.PositionOrRole);
                sqlCmd.Parameters.AddWithValue("@AccountType", acctDto.AccountType);


                var SqlReader = await sqlCmd.ExecuteReaderAsync();

                if (SqlReader.HasRows)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Áccount Created success", true, null, null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAccount(AcctSignInDto acctSignIn)
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.GetAccount, SqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@UserName", acctSignIn.Email);
                sqlCmd.Parameters.AddWithValue("@Password", acctSignIn.Password);

                var SqlReader = await sqlCmd.ExecuteReaderAsync();

                AccountInfo newAcct = null;

                if (SqlReader.HasRows)
                {
                    newAcct = new AccountInfo();

                    while (SqlReader.Read())
                    {
                        newAcct.PersonId = Convert.ToInt64(SqlReader["PersonId"].ToString());
                        newAcct.Status = Convert.ToString(SqlReader["Status"].ToString());
                        newAcct.AllowLoginTwoFactor = Convert.ToBoolean(SqlReader["AllowLoginTwoFactor"].ToString());
                        newAcct.CreatedDate = Convert.ToDateTime(SqlReader["CreatedDate"].ToString());
                        newAcct.AccountType = Convert.ToString(SqlReader["AccountType"].ToString());
                        newAcct.Username = Convert.ToString(SqlReader["Username"].ToString());
                        newAcct.Password = Convert.ToString(SqlReader["Password"].ToString());
                        newAcct.LastLoginDate = Convert.ToDateTime(SqlReader["LastLoginDate"].ToString());
                        newAcct.Id = Convert.ToInt64(SqlReader["Id"].ToString());
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Succeed", true, newAcct, newAcct, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, newAcct, newAcct, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> UpdateAccount(UpdateAcct updateAcct)
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.GetAccount, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@UserName", updateAcct.UserName);
                sqlCmd.Parameters.AddWithValue("@Password", updateAcct.Password);
                sqlCmd.Parameters.AddWithValue("@AccountId", updateAcct.AccountId);
                sqlCmd.Parameters.AddWithValue("@CurrentDate", updateAcct.CurrentDate);

                var SqlReader = await sqlCmd.ExecuteReaderAsync();

                AccountInfo newAcct = null;

                if (SqlReader.HasRows)
                {
                    newAcct = new AccountInfo();

                    while (SqlReader.Read())
                    {
                        newAcct.PersonId = Convert.ToInt64(SqlReader["PersonId"].ToString());
                        newAcct.Status = Convert.ToString(SqlReader["Status"].ToString());
                        newAcct.AllowLoginTwoFactor = Convert.ToBoolean(SqlReader["AllowLoginTwoFactor"].ToString());
                        newAcct.CreatedDate = Convert.ToDateTime(SqlReader["CreatedDate"].ToString());
                        newAcct.AccountType = Convert.ToString(SqlReader["AccountType"].ToString());
                        newAcct.Username = Convert.ToString(SqlReader["Username"].ToString());
                        newAcct.Password = Convert.ToString(SqlReader["Password"].ToString());
                        newAcct.LastLoginDate = Convert.ToDateTime(SqlReader["LastLoginDate"].ToString());
                        newAcct.Id = Convert.ToInt64(SqlReader["Id"].ToString());
                    }


                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Success", true, newAcct, newAcct, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, newAcct, newAcct, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> UpdateAccount(AcctDto acctDto, PersonDto personDto)
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.CreateAdminAccount, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                sqlCmd.Parameters.AddWithValue("@Password", acctDto.Password);
                sqlCmd.Parameters.AddWithValue("@AllowLoginTwoFactor", acctDto.AllowLoginTwoFactor);
                sqlCmd.Parameters.AddWithValue("@LastLoginDate", acctDto.LastLoginDate);

                var SqlReader = await sqlCmd.ExecuteReaderAsync();

                if (SqlReader.HasRows)
                {

                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SignInAcct(AcctSignInModel acctSignIn)
        {
            try
            {
                if (acctSignIn == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter your login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.Password) || string.IsNullOrWhiteSpace(acctSignIn.Password))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.EmailAddress) || string.IsNullOrWhiteSpace(acctSignIn.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Email Address or Username is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await GetAccount(new AcctSignInDto() { Email = acctSignIn.EmailAddress, Password = acctSignIn.Password });

                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == acctSignIn.EmailAddress && x.Status == StatusMgs.Active).FirstOrDefaultAsync();
                if (UserExit.AccountVerify == null || UserExit.AccountVerify.HasValue == false || UserExit.AccountVerify == false)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your login account has not been verify, login into your email address " + UserExit.Username + " and clink a verify link to activate your account.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (Result.DataLoad.AccountType == AccountType.Customer)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You account login don't have permission to access the application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    var HashSalt1 = HashSalt.HashPassword(acctSignIn.Password);

                    var ResultLoginHash = HashSalt.VerifyHashedPassword(Result.DataLoad.Password, acctSignIn.Password);

                    if (acctSignIn.Password == lapoCipher01.DecryptString(Result.DataLoad.Password) || ResultLoginHash)
                    {
                        try
                        {
                            var Id = (long)Convert.ToInt64(Result.DataLoad.Id);
                            var lapoLoan = await lapoLoanDB.SecurityAccounts.Where(s => s.Username == acctSignIn.EmailAddress).FirstOrDefaultAsync();
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

                            var TwoFactor = await GenarateTwoFactorAuth(Result.DataLoad.Id);

                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, TwoFactor.Data, TwoFactor.DataLoad, Status.Success, StatusMgs.Success, true);
                        }

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success);
                    }
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid username or password", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Failed to sign into your account, Enter valid login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SignInAcctClient(AcctSignInModel acctSignIn)
        {
            try
            {
                if (acctSignIn == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter your login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.Password) || string.IsNullOrWhiteSpace(acctSignIn.Password))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.EmailAddress) || string.IsNullOrWhiteSpace(acctSignIn.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Email Address or Username is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await GetAccount(new AcctSignInDto() { Email = acctSignIn.EmailAddress, Password = acctSignIn.Password });

                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == acctSignIn.EmailAddress && x.Status == StatusMgs.Active).FirstOrDefaultAsync();

                if (UserExit == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, String.Concat("Invaild email address"), false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (UserExit != null && UserExit.AccountVerify == null || UserExit.AccountVerify.HasValue == false || UserExit.AccountVerify == false)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, $"Your login account has not been verified, login into your email address {UserExit.Username} and click a verify link to activate your account.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (Result.DataLoad.AccountType == AccountType.Adiministration)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You account login don't have permission to access the application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    var HashSalt1 = HashSalt.HashPassword(acctSignIn.Password);

                    var ResultLoginHash = HashSalt.VerifyHashedPassword(Result.DataLoad.Password, acctSignIn.Password);

                    if (acctSignIn.Password == lapoCipher01.DecryptString(Result.DataLoad.Password) || ResultLoginHash ||  acctSignIn.Password == Result.DataLoad.Password)
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
                            var TwoFactor = await GenarateTwoFactorAuth(Result.DataLoad.Id);

                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, TwoFactor.Data, TwoFactor.DataLoad, Status.Success, StatusMgs.Success, true);
                        }

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success);
                    }
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid username or password", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Failed to sign into your account. Enter valid login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CreateAdministration()
        {
            try
            {
                if (DefalutToken.IsProduction == false)
                {
                    var Result = await HasExitingAccount();
                    if (Result.IsActive)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.NotCompleted, "Has Created Account Already", false, null, Result, Status.NotCompleted, StatusMgs.NotCompleted);
                    }

                    var newAdmin = new AcctDto()
                    {
                        AccountType = AccountType.Adiministration,
                        AllowLoginTwoFactor = true,
                        CreatedDate = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        Username = "LoanAppAdmin@Lapo.com.ng",
                        Status = StatusMgs.Active,
                    };

                    var Pwd = HashSalt.HashPassword("Admin123!!");
                    //lapoCipher01.EnryptString("Admin123!!");
                    newAdmin.Password = Pwd;

                    var newPerson = new PersonDto()
                    {
                        Age = 25,
                        AltPhoneNumber = "09144332211",
                        CreatedDate = DateTime.Now,
                        CurrentAddress = "12 Aguda Ogba Rd, Lagos Nigeria",
                        EmailAddress = newAdmin.Username,
                        FirstName = "Lapo Admin",
                        LastName = "Lapo Admin",
                        MiddleName = "Lapo Admin",
                        Gender = "Male",
                        PhoneNumber = "45",
                        PositionOrRole = AccountType.Adiministration,
                        Staff = AccountType.Adiministration,
                    };

                    var NewAcctResult = await CreateAccount(newAdmin, newPerson);
                    if (NewAcctResult.IsActive == true && NewAcctResult.status == Status.Success)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Saved", true, null, newAdmin, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Failed", false, null, "", Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> UpdateAcct(AcctSignInModel acctSignIn)
        {
            try
            {
                if (acctSignIn == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter your login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.Password) || string.IsNullOrWhiteSpace(acctSignIn.Password))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctSignIn.EmailAddress) || string.IsNullOrWhiteSpace(acctSignIn.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Email Address or Username is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await GetAccount(new AcctSignInDto() { Email = acctSignIn.EmailAddress, Password = acctSignIn.Password });

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    if (acctSignIn.Password == lapoCipher02.DecryptString(Result.DataLoad.Password))
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Failed to sign  into your account, Enter your login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AcctReset(AcctResetDto acctReset)
        {
            try
            {
                if (acctReset == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter your email address or username and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(acctReset.EmailAddress) || string.IsNullOrWhiteSpace(acctReset.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Email Address or Username is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await GetAccount(new AcctSignInDto() { Email = acctReset.EmailAddress, Password = "" });

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid username or email address", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Failed to sign  into your account, Enter Email Address or Username and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> NewPasswordReset(NewAcctPasswordResetDto newAcctPasswordReset)
        {
            try
            {
                if (newAcctPasswordReset == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter your new password and confirm password and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(newAcctPasswordReset.Password) || string.IsNullOrWhiteSpace(newAcctPasswordReset.Password))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(newAcctPasswordReset.ConfirmPassword) || string.IsNullOrWhiteSpace(newAcctPasswordReset.ConfirmPassword))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Confirm Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (newAcctPasswordReset.ConfirmPassword != newAcctPasswordReset.Password)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Confirm Password does not match Password is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result0 = await GetAccount(new AcctSignInDto() { Email = newAcctPasswordReset.Email });
                if (Result0 != null)
                {
                    var Result = await UpdateAccount(new UpdateAcct() { AccountId = Convert.ToInt64(lapoCipher01.DecryptString(newAcctPasswordReset.AcctId)), CurrentDate = DateTime.Now, UserName = Result0.DataLoad.Username, Password = lapoCipher01.EnryptString(newAcctPasswordReset.ConfirmPassword) });

                    if (Result != null && Result.IsActive && Result.status == Status.Success)
                    {
                        if (newAcctPasswordReset.Password == lapoCipher02.DecryptString(Result.DataLoad.Password))
                        {
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success);
                        }
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Failed to sign  into your account, Enter your login details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GenarateTwoFactorAuth(long? AcctId)
        {
            try
            {

                if (!AcctId.HasValue)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Login details is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (AcctId.Value == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Login details is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (AcctId.Value == 0)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Login details is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var CodeGenerated = await new RandomGeneratorHelpers(this._configuration).GenarateRandomCode(ControllerBase, AcctId.Value, "", 1, 6);

                var newAuth = new SaveTwoFactorAuthDto() { AccountId = AcctId.Value, Active = true, BVNVerification = "", GenaratedDateTime = DateTime.Now, CreatedDateTime = DateTime.Now, ExpiredDateTime = DateTime.Now.AddHours(+1), CodeGenerated = CaseLetter.ChangeAllCaseLetterToUpper(CodeGenerated.DataLoad) };

                newAuth.CodeGenerated = newAuth.CodeGenerated.ToUpper();

                var ResultVeriation = await SaveTwoFactorAuth(newAuth);

                if (ResultVeriation != null && ResultVeriation.IsActive && ResultVeriation.status == Status.Success)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Succeed", true, ResultVeriation.Data, ResultVeriation.DataLoad, Status.Success, StatusMgs.Success);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid username or password address", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Key-Of-Two Factor is required", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> TwoFactorAuth(TwoFactorAuthDto twoFactorAuth)
        {
            try
            {
                if (twoFactorAuth == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter the 6 digit code send to your phone number or email address and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(twoFactorAuth.Code) || string.IsNullOrWhiteSpace(twoFactorAuth.Code))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter the 6 digit code send to your phone number or email address and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(twoFactorAuth.KeyOfTwoFactor) || string.IsNullOrWhiteSpace(twoFactorAuth.KeyOfTwoFactor))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Key-Of-Two Factor is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await GetTwoFactorAuth(Convert.ToInt64(ReallapoCipher02.DecryptString(twoFactorAuth.KeyOfTwoFactor)), twoFactorAuth.Code);

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    var ExpireDate = Convert.ToDateTime(Result.DataLoad.ExpiredDateTime);
                    if (ExpiredTimeHelpers.IsExpired10(ExpireDate))
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "6 digit code has expired", false, "", "", Status.Ërror, StatusMgs.Error);
                    }

                    // Code, BVNVerification, IsActivexpired,  CreatedDate, GenaratedDateTime,  Id, AccountId

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Two-factor Auth code Succeeded", true, Result.Data, Result.DataLoad, Status.Success, StatusMgs.Success);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid username or password address", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Key-Of-Two Factor is required", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ValidateTwoFactorAuth(TwoFactorAuthÇodeDto twoFactorAuth)
        {
            try
            {
                if (twoFactorAuth == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter the 6 digit code send to your phone number or email address and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(twoFactorAuth.Code) || string.IsNullOrWhiteSpace(twoFactorAuth.Code))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter the 6 digit code send to your phone number or email address and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(twoFactorAuth.AccountId) || string.IsNullOrWhiteSpace(twoFactorAuth.AccountId))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter the 6 digit code send to your phone number or email address and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Result = await ValidateOTPUser(twoFactorAuth.AccountId, twoFactorAuth.Code);

                if (Result != null && Result.IsActive && Result.status == Status.Success)
                {
                    return Result;
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "The 6 digit code you entered is invalid", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Key-Of-Two Factor is required", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CheckIfAcctExit(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string EmailAddress)
        {
            try
            {
                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == EmailAddress).FirstOrDefaultAsync();
                if (UserExit != null)
                {
                    await new EmailHelpers(this._configuration).SendNewPasswordAuth(_environment, EmailAddress, new DefalutToken(_configuration).ResetPasswordUrl(lapoCipher01.EnryptString(EmailAddress)));

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Before you change your password, login to your email account (" + EmailAddress + ") to reset your password.", true, UserExit.Username, UserExit.Username, Status.Success, StatusMgs.Success);
                }

                //this.SqlConn.Open();
                //var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.ValidateIdentityExit, SqlConn);
                //sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                //sqlCmd.Parameters.AddWithValue("@EmailAddress", EmailAddress);
                //var SqlReader = await sqlCmd.ExecuteReaderAsync();
                //ReturnTwoFactorAuthDto newAcct = null;
                //if (SqlReader.HasRows)
                //{
                //    newAcct = new ReturnTwoFactorAuthDto();
                //    while (SqlReader.Read())
                //    {
                //    }
                //}

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "This Email address does'nt exit, try again.", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ChangePassword(PwrdChangeDto pwrdChange)
        {
            try
            {

                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.ChangePassword, SqlConn);

                if (string.IsNullOrWhiteSpace(pwrdChange.Password) || string.IsNullOrEmpty(pwrdChange.Password))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Password is required.", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (string.IsNullOrWhiteSpace(pwrdChange.ConfirmPassword) || string.IsNullOrEmpty(pwrdChange.ConfirmPassword))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Confirm Password is required.", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (string.IsNullOrWhiteSpace(pwrdChange.Username) || string.IsNullOrEmpty(pwrdChange.Username))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Your email address is required.", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (pwrdChange.Password != pwrdChange.ConfirmPassword)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Password must match Confirm Password.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (pwrdChange.ConfirmPassword.Length < 6)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "You must type atleast 6 or more characters words.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (pwrdChange.Password.Length < 6)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "You must type atleast 6 or more characters words.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                string emailDct = lapoCipher01.DecryptString(pwrdChange.Username);
                if (string.IsNullOrWhiteSpace(emailDct) || string.IsNullOrEmpty(emailDct))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Your email address is not correct.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var acct = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == emailDct).FirstOrDefaultAsync();
                if (acct != null)
                {
                    acct.Password = HashSalt.HashPassword(pwrdChange.ConfirmPassword); /*lapoCipher01.EnryptString(pwrdChange.ConfirmPassword);*/
                    acct.LastLoginDate = DateTime.Now;
                    lapoLoanDB.Entry(acct).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Success, "Your password has been changed successfully", true, null, null, Status.Success, StatusMgs.Success);
                }

                //sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                //sqlCmd.Parameters.AddWithValue("@Username", pwrdChange.Username);
                //sqlCmd.Parameters.AddWithValue("@ConfirmPassword", lapoCipher01.EnryptString(pwrdChange.ConfirmPassword));
                //sqlCmd.Parameters.AddWithValue("@Password",   lapoCipher01.EnryptString(pwrdChange.Password));

                //var SqlReader = await sqlCmd.ExecuteReaderAsync();
                //ReturnTwoFactorAuthDto newAcct = null;

                //if (SqlReader.HasRows)
                //{
                //    newAcct = new ReturnTwoFactorAuthDto();

                //    while (SqlReader.Read())
                //    {

                //    }
                //}

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Your password has not changed. type atleast 6 or more characters words and try again.", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> InneredChangePassword(PwrdChangeDto pwrdChange)
        {
            try
            {

                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.ChangePassword, SqlConn);

                if (string.IsNullOrWhiteSpace(pwrdChange.Password) || string.IsNullOrEmpty(pwrdChange.Password))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Password is required.", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (string.IsNullOrWhiteSpace(pwrdChange.ConfirmPassword) || string.IsNullOrEmpty(pwrdChange.ConfirmPassword))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Confirm Password is required.", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (string.IsNullOrWhiteSpace(pwrdChange.Username) || string.IsNullOrEmpty(pwrdChange.Username))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Your email address is required.", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (pwrdChange.Password != pwrdChange.ConfirmPassword)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Password must match Confirm Password.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (pwrdChange.ConfirmPassword.Length < 6)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "You must type atleast 6 or more characters words.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (pwrdChange.Password.Length < 6)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "You must type atleast 6 or more characters words.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var emailDct = (long)Convert.ToInt32(pwrdChange.Username);

                var acct = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == emailDct).FirstOrDefaultAsync();

                if (acct != null)
                {
                    var ResultLoginHash = HashSalt.VerifyHashedPassword(acct.Password, pwrdChange.Password);

                    if (ResultLoginHash)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "You can't use your previous password again.", false, null, null, Status.Failed, StatusMgs.Failed);
                    }
                }

                if (acct != null)
                {
                    acct.Password = HashSalt.HashPassword(pwrdChange.ConfirmPassword); /*lapoCipher01.EnryptString(pwrdChange.ConfirmPassword);*/
                    acct.LastLoginDate = DateTime.Now;
                    lapoLoanDB.Entry(acct).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Success, "Your password has been changed successfully", true, null, null, Status.Success, StatusMgs.Success);
                }

                //sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                //sqlCmd.Parameters.AddWithValue("@Username", pwrdChange.Username);
                //sqlCmd.Parameters.AddWithValue("@ConfirmPassword", lapoCipher01.EnryptString(pwrdChange.ConfirmPassword));
                //sqlCmd.Parameters.AddWithValue("@Password",   lapoCipher01.EnryptString(pwrdChange.Password));

                //var SqlReader = await sqlCmd.ExecuteReaderAsync();
                //ReturnTwoFactorAuthDto newAcct = null;

                //if (SqlReader.HasRows)
                //{
                //    newAcct = new ReturnTwoFactorAuthDto();

                //    while (SqlReader.Read())
                //    {

                //    }
                //}

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, sqlCmd, StatusMgs.Failed, "Your password has not changed. type atleast 6 or more characters words and try again.", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CreateCustomerAccount(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, RegisterCustomerDto registerCustomer)
        {
            try
            {
                if (string.IsNullOrEmpty(registerCustomer.ConfirmPassword) || string.IsNullOrWhiteSpace(registerCustomer.ConfirmPassword))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Confirm Password is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (string.IsNullOrEmpty(registerCustomer.Password) || string.IsNullOrWhiteSpace(registerCustomer.Password))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Password is Required", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (string.IsNullOrEmpty(registerCustomer.EmailAddress) || string.IsNullOrWhiteSpace(registerCustomer.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your email address is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (!registerCustomer.TermAndConditions)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your must read and accept the Term And Conditions", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else if (!registerCustomer.PrivacyPolicy)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your must read and accept the Privacy Policy", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (registerCustomer.ConfirmPassword.Length < 6 || registerCustomer.Password.Length < 6)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Passwords can not be less than 6 characters", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var HasExitingAcct = await lapoLoanDB.SecurityAccounts.Where(x => x.Username.ToLower() == registerCustomer.EmailAddress.ToLower()).AnyAsync();
                if (HasExitingAcct)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "This email already exists.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var newSecurity = new SecurityAccount()
                {
                    CreatedDate = DateTime.Now,
                    AccountType = AccountType.Customer,
                    LastLoginDate = DateTime.Now,
                    AllowLoginTwoFactor = false,
                    Status = StatusMgs.Active,
                    Username = registerCustomer.EmailAddress,
                    Password = HashSalt.HashPassword(registerCustomer.ConfirmPassword),
                    AccountVerify = false,
                };

                lapoLoanDB.SecurityAccounts.Add(newSecurity);
                await lapoLoanDB.SaveChangesAsync();

                var newPerson = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.Person()
                {
                    EmailAddress = registerCustomer.EmailAddress,
                    AccountId = newSecurity.Id,
                    CurrentAddress = "",
                    Age = "",
                    LastName = "",
                    MiddleName = "",
                    PhoneNumber = "",
                    PositionOrRole = "",
                    Staff = "",
                    Gender = "",
                    FirstName = "",
                    CreatedDate = DateTime.Now,
                    AltPhoneNumber = "",
                };
                lapoLoanDB.People.Add(newPerson);
                await lapoLoanDB.SaveChangesAsync();

                var HasExitingAcct1 = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == registerCustomer.EmailAddress).FirstOrDefaultAsync();

                if (HasExitingAcct1 != null)
                {
                    FileLogActivities.CallSevice("Email Sender for customer Reg: ", "About to send a e-mail", "Sending E-mail");

                    await new EmailHelpers(this._configuration).SendAccountCreation(_environment, HasExitingAcct1.Id.ToString(), HasExitingAcct1.Username, new DefalutToken(_configuration).AccountVerifyLink(new LapoCipher00().encrypt(HasExitingAcct1.Id.ToString())));

                    HasExitingAcct1.PersonId = newPerson.Id;

                    lapoLoanDB.Entry(HasExitingAcct1).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your account has been successfully created", true, new { AcctId = HasExitingAcct1.Id, AccId = new LapoCipher00().encrypt(HasExitingAcct1.Id.ToString()) }, new { AcctId = HasExitingAcct1.Id, AccId = new LapoCipher00().encrypt(HasExitingAcct1.Id.ToString()) }, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "This email already exists", false, null, null, Status.Failed, StatusMgs.Failed, true);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Check your network connection and try again.", false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAcctDetailsById(int Id = 0)
        {
            try
            {
                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (UserExit == null || UserExit.AccountVerify == false)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your login account has not been verify, login into your email address " + UserExit.Username + " and clink a verify link to activate your account try again.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (UserExit != null)
                {
                    var PersonExit = await lapoLoanDB.People.Where(x => x.AccountId == UserExit.Id).FirstOrDefaultAsync();

                    if (PersonExit != null)
                    {
                        var acctDetails = new AccountDetailsDto();
                        acctDetails.Phone = PersonExit.PhoneNumber;
                        acctDetails.AltPhone = PersonExit.AltPhoneNumber;
                        acctDetails.CurrentAddress = PersonExit.CurrentAddress;
                        acctDetails.Age = PersonExit.Age;
                        acctDetails.Gender = PersonExit.Gender;
                        acctDetails.AccountType = UserExit.AccountType;
                        acctDetails.Address = PersonExit.CurrentAddress;
                        acctDetails.FirstName = PersonExit.FirstName;
                        acctDetails.MiddleName = PersonExit.MiddleName;
                        acctDetails.LastName = PersonExit.LastName;
                        acctDetails.AccountId = Id;
                        acctDetails.Email = UserExit.Username;
                        acctDetails.AccountType = UserExit.AccountType;

                        var HubTeamsE = await lapoLoanDB.HubTeams.Where(x => x.TeamAccountId == UserExit.Id).FirstOrDefaultAsync();

                        if (HubTeamsE != null)
                        {
                            acctDetails.RoleType = HubTeamsE.BackEndUsers;

                            acctDetails.RoleType = acctDetails.RoleType.Replace("AND", "").Replace("ACCOUNT", "");
                            acctDetails.RoleType = acctDetails.RoleType.Replace("ACCOUNT", "");
                        }

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Get Account Details successfully", true, acctDetails, acctDetails, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "This Email address does'nt exit, try again.", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public async Task<RespondMessageDto> CheckIfBvnExit(BvnRequestDto bvnRequest)
        {
            try
            {

                if (string.IsNullOrEmpty(bvnRequest.BvnRequest) || string.IsNullOrWhiteSpace(bvnRequest.BvnRequest))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Bvn is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var BvnAcct = await lapoLoanDB.Bvnverifications.Where(x => x.Bvnverification1.ToLower() == bvnRequest.BvnRequest.ToLower() && x.IsActive.Value).ToListAsync();
                if (BvnAcct.Count > 0)
                {
                    foreach (var item in BvnAcct)
                    {
                        item.IsActive = false;
                        lapoLoanDB.Entry(item).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();
                    }
                }

                var People1 = await lapoLoanDB.People.Where(x => x.AccountId == bvnRequest.AcctId).FirstOrDefaultAsync();
                if (People1 == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Invalid account login", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var SecurityAccts = await lapoLoanDB.SecurityAccounts.Where(x => x.BvnAccount == bvnRequest.BvnRequest && x.Id == bvnRequest.AcctId).FirstOrDefaultAsync();
                if (SecurityAccts == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Enter BVN associated with your profile.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                string User_Id =   new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false)
                {
                    try
                    {
                        BvnClas verBvn = new BvnClas()
                        {
                            BVN = bvnRequest.BvnRequest
                        };

                        var sendemail = JsonConvert.SerializeObject(verBvn);
                        string verBvnEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);
                        byte[] inbyte = Convert.FromBase64String(verBvnEnc);
                        string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                        string Url =   new DefalutToken(_configuration).BasedUrl();

                        RespondMessageDto DataResult = await LapoLoanHttpClient.SentAsync(Url, AllAccessTokens.RefreshToken, hexs, httpMethod: System.Net.Http.HttpMethod.Post);

                        if (DataResult != null && DataResult.IsActive == true && DataResult.status == Status.Success)
                        {
                            var DataToken = DataResult.Data as AccessTokenResponses;
                            var DataToken1 = DataResult.DataLoad as AccessTokenResponses;

                            string Data = DataToken1.data.ToString();
                            byte[] DataByte = StringToByteArray(Data);
                            string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);

                            var aesKey = AllAccessTokens.AesKey;
                            var aesIv = AllAccessTokens.AesIv;

                            string DecryptData = NETCore.Encrypt.EncryptProvider.AESDecrypt(Database64String, aesKey, aesIv);

                            var getUserData = JObject.Parse(DecryptData);

                            var bvnDetails = JsonConvert.DeserializeObject<BvnRespondsDto>(DecryptData);

                            //string acc_Token = getUserData["access_token"].ToString();
                            //aesKey = getUserData["aesKey"].ToString();
                            //aesIv = getUserData["aesIv"].ToString();

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "OTP will be sent to your email @ " + People1.EmailAddress + " and the phone number associated with your BVN. (" + bvnDetails.PhoneNumber1 + ")", true, bvnDetails,  bvnDetails, Status.Success, StatusMgs.Success, true);
                        }

                        try
                        {
                            //var DataToken = DataResult.Data as AccessTokenResponses;
                           
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "BVN Error: "+ DataResult.Data.ToString(), false, null, null, Status.Failed, StatusMgs.Failed);
                        }
                        catch(Exception ex2)
                        {
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your BVN is Incorrect Or Network Connection and try again: " + DataResult.Data.ToString(), false, null, null, Status.Failed, StatusMgs.Failed);
                        }
                    }
                    catch (Exception ex)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your BVN is Incorrect Or Network Connection and try again", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //else
                //{
                //    string defaultData = "{\r\n\"ResponseCode\": \"00\",\r\n\"BVN\": \"22222222226\",\r\n\"FirstName\": \"MOHAMMAD\",\r\n\"MiddleName\": \"A\",\r\n\"LastName\": \"Chidebelu Eze\",\r\n\"DateOfBirth\": \"14-May-1990\",\r\n\"RegistrationDate\": \"11-JAN-15\",\r\n\"EnrollmentBank\": \"035\",\r\n\"EnrollmentBranch\": \"Lekki\",\r\n\"PhoneNumber1\": \"07069302232\",\r\n\"WatchListed\": \"NO\"\r\n} ";

                //    var bvnDetails1 = JsonConvert.DeserializeObject<BvnRespondsDto>(defaultData);

                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Message was successful", true, bvnDetails1, bvnDetails1, Status.Success, StatusMgs.Success, true);
                //}

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Check network connection and try again", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CheckIfBvnCheckers(BvnCheckerDto bvnRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(bvnRequest.BvnRequest) || string.IsNullOrWhiteSpace(bvnRequest.BvnRequest))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Bvn is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var BvnAcct = await lapoLoanDB.Bvnverifications.Where(x => x.Bvnverification1.ToLower() == bvnRequest.BvnRequest.ToLower() && x.IsActive.Value).ToListAsync();
                if (BvnAcct.Count > 0)
                {
                    foreach (var item in BvnAcct)
                    {
                        item.IsActive = false;
                        lapoLoanDB.Entry(item).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();
                    }
                }

                var SecurityAccount = await lapoLoanDB.SecurityAccounts.Where(x => x.BvnAccount.ToLower() == bvnRequest.BvnRequest.ToLower()).AnyAsync();

                if (SecurityAccount)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "BVN already exit by another user.", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var dadad = new LapoCipher00().Decrypt(bvnRequest.AcctId);

                var Id = long.Parse(dadad);
                var People1 = await lapoLoanDB.People.Where(x => x.AccountId == Id).FirstOrDefaultAsync();

                if (People1 == null)
                {

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Invalid account login", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                string User_Id =   new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null &&    DefalutToken.IsLocal == false)
                {
                    try
                    {
                        BvnClas verBvn = new BvnClas()
                        {
                            BVN = bvnRequest.BvnRequest
                        };

                        var sendemail = JsonConvert.SerializeObject(verBvn);
                        string verBvnEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);
                        byte[] inbyte = Convert.FromBase64String(verBvnEnc);
                        string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                        string Url =   new DefalutToken(_configuration).BasedUrl();

                        RespondMessageDto DataResult = await LapoLoanHttpClient.SentAsync(Url, AllAccessTokens.RefreshToken, hexs, httpMethod: System.Net.Http.HttpMethod.Post);

                        if (DataResult != null && DataResult.IsActive == true && DataResult.status == Status.Success)
                        {
                            var DataToken = DataResult.Data as AccessTokenResponses;
                            var DataToken1 = DataResult.DataLoad as AccessTokenResponses;

                            string Data = DataToken1.data.ToString();
                            byte[] DataByte = StringToByteArray(Data);
                            string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);

                            var aesKey = AllAccessTokens.AesKey;
                            var aesIv = AllAccessTokens.AesIv;

                            string DecryptData = NETCore.Encrypt.EncryptProvider.AESDecrypt(Database64String, aesKey, aesIv);

                            var getUserData = JObject.Parse(DecryptData);

                            var bvnDetails = JsonConvert.DeserializeObject<BvnCheckerRespondsDto>(DecryptData);

                            bvnDetails.EmailAddress = People1.EmailAddress;
                            bvnDetails.AccountId = People1.AccountId.ToString();

                            //string acc_Token = getUserData["access_token"].ToString();
                            //aesKey = getUserData["aesKey"].ToString();
                            //aesIv = getUserData["aesIv"].ToString();

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "OTP will be sent to your email @ " + People1.EmailAddress + " and the phone number associated with your BVN. (" + bvnDetails.PhoneNumber1 + ")", true, bvnDetails, bvnDetails, Status.Success, StatusMgs.Success, true);

                        }
                    }
                    catch (Exception ex)
                    {

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Check network connection and try again", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //else
                //{
                //    string defaultData = "{\r\n\"ResponseCode\": \"00\",\r\n\"BVN\": \"22222222226\",\r\n\"FirstName\": \"MOHAMMAD\",\r\n\"MiddleName\": \"A\",\r\n\"LastName\": \"Chidebelu Eze\",\r\n\"DateOfBirth\": \"14-May-1990\",\r\n\"RegistrationDate\": \"11-JAN-15\",\r\n\"EnrollmentBank\": \"035\",\r\n\"EnrollmentBranch\": \"Lekki\",\r\n\"PhoneNumber1\": \"07069302232\",\r\n\"WatchListed\": \"NO\"\r\n} ";

                //    var bvnDetails1 = JsonConvert.DeserializeObject<BvnRespondsDto>(defaultData);

                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Message was successful", true, bvnDetails1, bvnDetails1, Status.Success, StatusMgs.Success, true);
                //}

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Check network connection and try again", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetCheckIfAcctIsActive(string bvnRequest)
        {
            try
            {
                if(string.IsNullOrEmpty(bvnRequest) || string.IsNullOrWhiteSpace(bvnRequest))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Bvn is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                var dadad = new LapoCipher00().Decrypt(bvnRequest);

                var Id = long.Parse(dadad);
                var People1 = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (People1 != null && People1.AccountVerify == true)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Invalid account login", true, null, null, Status.Success, StatusMgs.Success, true);
                }

                return new RespondMessageDto( this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Check network connection and try again", false, null, null, Status.Failed, StatusMgs.Failed );
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetCheckIfBankAccountIsOk(string bvnRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(bvnRequest) || string.IsNullOrWhiteSpace(bvnRequest))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Bvn is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(bvnRequest.ToString(), "^[0-9]*$"))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //if (System.Text.RegularExpressions.Regex.IsMatch("^[0-9]", bvnRequest.ToString()))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Good Bank Account Number", true, null, null, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> TwoFactorBvnAuth(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, BvnAuthDto bvnAuth)
        {
            try
            {
                if (bvnAuth == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Provide your Bvn details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(bvnAuth.BVN) || string.IsNullOrWhiteSpace(bvnAuth.BVN))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Provide your Bvn details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(bvnAuth.EmailAddress) || string.IsNullOrWhiteSpace(bvnAuth.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Provide your Email details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(bvnAuth.Phone) || string.IsNullOrWhiteSpace(bvnAuth.Phone))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Provide your Email details and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(bvnAuth.AccountId.ToString()) || string.IsNullOrWhiteSpace(bvnAuth.AccountId.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Login into your account and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var AcctId = (long)int.Parse(bvnAuth.AccountId.ToString());
                var BvnAcct = await lapoLoanDB.Bvnverifications.Where(x => x.Bvnverification1.ToLower() == bvnAuth.BVN.ToLower() && x.IsActive.Value && x.AccountRequestId == AcctId).ToListAsync();
                if (BvnAcct.Count > 0)
                {
                    foreach (var item in BvnAcct)
                    {
                        item.IsActive = false;
                        lapoLoanDB.Entry(item).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();
                    }
                }

                var CodeGenerated = await new RandomGeneratorHelpers(this._configuration).BvnGenarateRandomCode(ControllerBase, AcctId, "", 1, 6);

                //var newAuth = new SaveTwoFactorAuthDto() { AccountId = AcctId.Value, Active = true, BVNVerification = "", GenaratedDateTime = DateTime.Now, CreatedDateTime = DateTime.Now, ExpiredDateTime = DateTime.Now.AddHours(+1), };

                //var LoanApplicationRequest = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.AccountId == AcctId && x.Bvn == bvnAuth.BVN).FirstOrDefaultAsync();
                //if (LoanApplicationRequest == null)
                //{

                //}

                var Code = CaseLetter.ChangeAllCaseLetterToUpper(CodeGenerated.DataLoad);
                Code = Code.ToString().ToUpper();

                var User = await lapoLoanDB.People.Where(x => x.AccountId == AcctId).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(Code) || string.IsNullOrWhiteSpace(Code))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "The bvn you enter is invalid", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (User != null)
                {
                    var SecurityAcct = await lapoLoanDB.SecurityAccounts.Where(x => x.PersonId == User.Id).FirstOrDefaultAsync();

                    if (SecurityAcct != null)
                    {
                        RespondMessageDto EmailSenderResult = await new EmailHelpers(this._configuration).SendBvnAuth(_environment, AcctId.ToString(), SecurityAcct.Username, Code);

                        if (EmailSenderResult != null && (EmailSenderResult.IsActive || !EmailSenderResult.IsActive))
                        {
                            var newBvnverify = new Bvnverification()
                            {
                                Code = Code,
                                CreatedDate = DateTime.Now,
                                ExpiredDateTime = DateTime.Now,
                                GenaratedDateTime = DateTime.Now,
                                IsActive = true,
                                Bvnverification1 = bvnAuth.BVN,
                                AccountRequestId = AcctId,
                                // LoadAppRequestHeaderId = AcctId, 
                            };
                            lapoLoanDB.Bvnverifications.Add(newBvnverify);
                            await lapoLoanDB.SaveChangesAsync();

                            string message = "PS-loans - Your otp code " + Code;
                            await new TwoFactorsHelper(this._configuration).SendBVNsmsAsync(_environment, bvnAuth.Phone, message, SecurityAcct.Id);

                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Two-factor Auth code Succeeded", true, Code, Code, Status.Success, StatusMgs.Success);
                        }
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "The bvn you enter is invalid", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> VerifyBvnAuth(string Code)
        {
            try
            {
                if (Code == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter the Bvn otp Code and try again", false, "", "", Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(Code) || string.IsNullOrWhiteSpace(Code))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter the Bvn otp Code and try again", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //var AcctId = (long)int.Parse(bvnAuth.AccountId.ToString());
                var BvnAcct = await lapoLoanDB.Bvnverifications.Where(x => x.Code == Code && x.IsActive.Value).ToListAsync();
                if (BvnAcct.Count > 0)
                {
                    foreach (var item in BvnAcct)
                    {
                        item.IsActive = false;
                        lapoLoanDB.Entry(item).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "otp Auth code Succeeded", true, Code, Code, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter a valid bvn otp Code and try again", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetRoundomNo(int Code)
        {
            try
            {
                if (Code <= 0)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Spinner length must be grater than 0", false, Code, Code, Status.Ërror, StatusMgs.Error);
                }

                if (Code > 0)
                {
                    var Result = new RandomGeneratorHelpers(this._configuration).RandomNumber(1, Code);

                    var SpriningResult = giid.SpriningListStype[Result];

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Spinner length is grater than 0", true, SpriningResult, SpriningResult, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Spinner length must be grater than 0", false, Code, Code, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAllBankList()
        {
            try
            {
                List<BankListResponseDto> BankList = new List<BankListResponseDto>();

                string User_Id =   new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false)
                {
                    try
                    {
                        string Url =   new DefalutToken(_configuration).RetriveBankAccount();

                        RespondMessageDto DataResult = await LapoLoanHttpClient.SentAsync(Url, AllAccessTokens.RefreshToken, "", httpMethod: System.Net.Http.HttpMethod.Get);

                        if (DataResult != null && DataResult.IsActive == true && DataResult.status == Status.Success)
                        {
                            var DataToken = DataResult.Data as AccessTokenResponses;
                            var DataToken1 = DataResult.DataLoad as AccessTokenResponses;

                            string Data = DataToken1.data.ToString();
                            byte[] DataByte = StringToByteArray(Data);
                            string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);

                            var aesKey = AllAccessTokens.AesKey;
                            var aesIv = AllAccessTokens.AesIv;

                            string DecryptData = NETCore.Encrypt.EncryptProvider.AESDecrypt(Database64String, aesKey, aesIv);


                            var getUserData = JObject.Parse(DecryptData);

                            BankList = JsonConvert.DeserializeObject<List<BankListResponseDto>>(DecryptData);

                            //string acc_Token = getUserData["access_token"].ToString();
                            //aesKey = getUserData["aesKey"].ToString();
                            //aesIv = getUserData["aesIv"].ToString();

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, BankList, BankList, Status.Success, StatusMgs.Success, true);

                        }
                    }
                    catch (Exception ex)
                    {

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
                    }
                }
                else
                {
                    BankList.Add(new BankListResponseDto() { bank = "Guarantee Trust Bank PLC", bank_code = "1" });

                    BankList.Add(new BankListResponseDto() { bank = "Opay", bank_code = "2" });

                    BankList.Add(new BankListResponseDto() { bank = "First Bank PLC", bank_code = "3" });

                    BankList.Add(new BankListResponseDto() { bank = "Zeninth Bank PLC", bank_code = "4" });

                    BankList.Add(new BankListResponseDto() { bank = "Access Bank PLC", bank_code = "5" });

                    BankList.Add(new BankListResponseDto() { bank = "Union Bank PLC", bank_code = "6" });

                    BankList.Add(new BankListResponseDto() { bank = "Access Diamond Bank PLC", bank_code = "7" });

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Message was successful", true, BankList, BankList, Status.Success, StatusMgs.Success, true);

                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetBankAcctDetailsByAcctNo(RequestAccountNoDto requestAccount)
        {
            try
            {
                if (string.IsNullOrEmpty(requestAccount.AccountNo) || string.IsNullOrWhiteSpace(requestAccount.AccountNo))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your bank account number is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                string User_Id =   new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false)
                {
                    try
                    {
                        var AccountNo = new RequestAccountNoDto()
                        {
                            AccountNo = requestAccount.AccountNo
                        };

                        var sendemail = JsonConvert.SerializeObject(AccountNo);
                        string verBvnEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);
                        byte[] inbyte = Convert.FromBase64String(verBvnEnc);
                        string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                        string Url =   new DefalutToken(_configuration).AccountdetailsNo();

                        RespondMessageDto DataResult = await LapoLoanHttpClient.SentAsync(Url, AllAccessTokens.RefreshToken, hexs, httpMethod: System.Net.Http.HttpMethod.Post);

                        if (DataResult != null && DataResult.IsActive == true && DataResult.status == Status.Success)
                        {
                            var DataToken = DataResult.Data as AccessTokenResponses;
                            var DataToken1 = DataResult.DataLoad as AccessTokenResponses;

                            string Data = DataToken1.data.ToString();
                            byte[] DataByte = StringToByteArray(Data);
                            string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);

                            var aesKey = AllAccessTokens.AesKey;
                            var aesIv = AllAccessTokens.AesIv;

                            string DecryptData = NETCore.Encrypt.EncryptProvider.AESDecrypt(Database64String, aesKey, aesIv);

                            var getUserData = JObject.Parse(DecryptData);

                            var BankAcctDetails = JsonConvert.DeserializeObject<BankAcctDetailsResponseDto>(DecryptData);

                            //string acc_Token = getUserData["access_token"].ToString();
                            //aesKey = getUserData["aesKey"].ToString();
                            //aesIv = getUserData["aesIv"].ToString();

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, BankAcctDetails, BankAcctDetails, Status.Success, StatusMgs.Success, true);

                        }
                    }
                    catch (Exception ex)
                    {

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
                }
                else
                {
                    var newAcctDetails = new BankAcctDetailsResponseDto()
                    {
                        account_class = "",
                        account_name = "Aliu Jonathan",
                        account_number = "22222222222",
                        account_status = "",
                        account_type = "",
                        bank_verification_number = "0",
                        branch_code = "SASF1",
                        branch_name = "MILE 12 BRANCH",
                        customer_number = "004",
                        gender = "M",
                        primary_email_address = "aliu.jonathan@gmail.com",
                        primary_phone_number = "08080000000",
                        primary_physical_address = ""
                    };

                    //"": "",
                    //"": "004",
                    //"": "",
                    //"": "000000000",
                    //"": "",
                    //"": "S",
                    //"": "",
                    //"": "N",
                    //"": "M",
                    //"": "",
                    //"": "",
                    //"": "10 Gbogan st Yaba Lagos"

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Bank Account Details was successful", true, newAcctDetails, newAcctDetails, Status.Success, StatusMgs.Success, true);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Failed", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AccountVerify(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AccountId)
        {
            try
            {
                if (AccountId == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not available.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(AccountId) || string.IsNullOrWhiteSpace(AccountId))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "An error has occur, Your account ID is missing.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var AcctId = (long)int.Parse(new LapoCipher00().Decrypt(AccountId));
                var AccountAva = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == AcctId).FirstOrDefaultAsync();

                if (AccountAva.AccountVerify == true)
                {

                    AccountAva.AccountVerify = true;
                    lapoLoanDB.Entry(AccountAva).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your account has been activated successfully. you can now login to your account.", true, "", null, Status.Success, StatusMgs.Success);
                }
                else
                {

                    AccountAva.AccountVerify = true;
                    lapoLoanDB.Entry(AccountAva).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your account has been activated successfully. you can now login to your account.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your account is not a valid account. contact the admin to resolve your account", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> emailaccountverifys(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, UpdateAccoutModel accoutModel)
        {
            try
            {
                if (accoutModel == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not available.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(accoutModel.identification) || string.IsNullOrWhiteSpace(accoutModel.identification))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Account ID is missing.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(accoutModel.EmploymentDate.ToString()) || string.IsNullOrWhiteSpace(accoutModel.EmploymentDate.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Employment Date is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(accoutModel.RetirementDate.ToString()) || string.IsNullOrWhiteSpace(accoutModel.RetirementDate.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Retirement Date is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(accoutModel.Bvn.ToString()) || string.IsNullOrWhiteSpace(accoutModel.Bvn.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Bvn is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (Convert.ToDateTime(accoutModel.RetirementDate).Date  == Convert.ToDateTime(accoutModel.EmploymentDate).Date)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Employment and Retirement Date can not be the same date", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (Convert.ToDateTime(accoutModel.RetirementDate).Date <= Convert.ToDateTime(accoutModel.EmploymentDate).Date)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "RetirementDate must be greater than Employment Date.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var IsbvnExit = await lapoLoanDB.SecurityAccounts.Where(x => x.BvnAccount == accoutModel.Bvn).AnyAsync();
                if (IsbvnExit)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "BVN already exits by another user.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var DateNow = DateTime.Now;

                if (Convert.ToDateTime(accoutModel.EmploymentDateOfBirth).Date >= DateNow.Date)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date-of-birth must be less than current date.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //if (accoutModel.EmploymentPhoneNumber.Length > 11)
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter phone number in the formart 080********", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                var phn = (string)Convert.ToString(accoutModel.EmploymentPhoneNumber);

                var Peopl11 = await lapoLoanDB.People.Where(x => x.PhoneNumber == phn).AnyAsync();
                if (Peopl11)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This Phone Number is already associated with another customer.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var AcctId = (long)int.Parse(new LapoCipher00().Decrypt(accoutModel.identification));
                var AccountAva = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == AcctId).FirstOrDefaultAsync();
                if (AccountAva == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account does not exist with us. Please verify your account with the activation link in your email.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (AccountAva != null && AccountAva.AccountVerify == true)
                {
                    var Peopl = await lapoLoanDB.People.Where(x => x.AccountId == AccountAva.Id).FirstOrDefaultAsync();
                    if (Peopl != null)
                    {
                        Peopl.RetirementDate = Convert.ToDateTime(accoutModel.RetirementDate);
                        Peopl.EmploymentDate = Convert.ToDateTime(accoutModel.EmploymentDate.ToString());
                        Peopl.FirstName = accoutModel.EmploymentFirstName;
                        Peopl.MiddleName = accoutModel.EmploymentMiddleName;
                        Peopl.PhoneNumber = accoutModel.EmploymentPhoneNumber;
                        Peopl.LastName = accoutModel.EmploymentLastName;
                        Peopl.Age = Convert.ToDateTime(accoutModel.EmploymentDateOfBirth).ToLongDateString();
                        Peopl.Gender = accoutModel.EmploymentGender;
                        Peopl.MarrintalStatus = accoutModel.EmploymentMaritalStatus;

                        lapoLoanDB.Entry(Peopl).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();
                    }

                    AccountAva.AccountVerify = true;

                    AccountAva.BvnAccount = accoutModel.Bvn;
                    lapoLoanDB.Entry(AccountAva).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    var newAcct = new { Username = AccountAva.Username, Password = AccountAva.Password };

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Account successfully activated.", true, newAcct, newAcct, Status.Success, StatusMgs.Success);
                }
                else if (AccountAva != null && AccountAva.AccountVerify == false)
                {
                    var Peopl = await lapoLoanDB.People.Where(x => x.AccountId == AccountAva.Id).FirstOrDefaultAsync();
                    if (Peopl != null)
                    {
                        Peopl.RetirementDate = Convert.ToDateTime(accoutModel.RetirementDate);
                        Peopl.EmploymentDate = Convert.ToDateTime(accoutModel.EmploymentDate);
                        Peopl.FirstName = accoutModel.EmploymentFirstName;
                        Peopl.MiddleName = accoutModel.EmploymentMiddleName;
                        Peopl.PhoneNumber = accoutModel.EmploymentPhoneNumber;
                        Peopl.LastName = accoutModel.EmploymentLastName;
                        Peopl.Age = Convert.ToDateTime(accoutModel.EmploymentDateOfBirth).ToLongDateString();
                        Peopl.Gender = accoutModel.EmploymentGender;
                        Peopl.MarrintalStatus = accoutModel.EmploymentMaritalStatus;
                        lapoLoanDB.Entry(Peopl).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();
                    }

                    AccountAva.BvnAccount = accoutModel.Bvn;
                    AccountAva.AccountVerify = true;
                    lapoLoanDB.Entry(AccountAva).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    var newAcct = new { Username = AccountAva.Username, Password = AccountAva.Password };

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Account successfully activated.", true, newAcct, newAcct, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account does not exist with us. Please verify your account with the activation link in your email.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetClientsNetPayHeader(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                var Clients = await lapoLoanDB.SecurityAccounts.Where(n => n.AccountType == AccountType.Customer).ToListAsync();

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
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
                        Clients = await lapoLoanDB.SecurityAccounts.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.AccountType == AccountType.Customer && (n.AccountType.Equals(pagenationFilter.searchText) || n.Username.Equals(pagenationFilter.searchText) || lapoLoanDB.People.Where(s => s.AccountId == n.Id && (s.MiddleName.Equals(pagenationFilter.searchText) || s.FirstName.Equals(pagenationFilter.searchText) || s.EmailAddress.Equals(pagenationFilter.searchText) || s.LastName.Equals(pagenationFilter.searchText) || s.PhoneNumber.Equals(pagenationFilter.searchText) || s.AltPhoneNumber.Equals(pagenationFilter.searchText))).Any())).ToListAsync();

                    }
                    else
                    {
                        Clients = await lapoLoanDB.SecurityAccounts.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.AccountType == AccountType.Customer && n.Status == pagenationFilter.status && (n.AccountType.Equals(pagenationFilter.searchText) || n.Username.Equals(pagenationFilter.searchText) || lapoLoanDB.People.Where(s => s.AccountId == n.Id && (s.MiddleName.Equals(pagenationFilter.searchText) || s.FirstName.Equals(pagenationFilter.searchText) || s.EmailAddress.Equals(pagenationFilter.searchText) || s.LastName.Equals(pagenationFilter.searchText) || s.PhoneNumber.Equals(pagenationFilter.searchText) || s.AltPhoneNumber.Equals(pagenationFilter.searchText))).Any())).ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        Clients = await lapoLoanDB.SecurityAccounts.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.AccountType == AccountType.Customer).ToListAsync();
                    }
                    else
                    {
                        Clients = await lapoLoanDB.SecurityAccounts.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)  && n.AccountType == AccountType.Customer && n.Status == pagenationFilter.status).ToListAsync();
                    }
                }

                if (Clients == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No customer not was found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                Clients = Clients.OrderByDescending(s => s.CreatedDate).OrderByDescending(s => s.Status == "Active").ToList();

                if (Clients != null && Clients.Count > 1)
                {
                    List<ClientCustomers> clientNetPays = new List<ClientCustomers>();
                    int number = 0;

                    foreach (var clientNetPay in Clients)
                    {
                        number += 1;
                        var newClientNetPay = new ClientCustomers()
                        {
                            Status = clientNetPay.Status,
                            CreatedDate = clientNetPay.CreatedDate.ToLongDateString(),
                            No = number.ToString(),
                            id = clientNetPay.Id.ToString()
                        };

                        var clit = await lapoLoanDB.People.Where(n => n.AccountId == clientNetPay.Id).FirstOrDefaultAsync();

                        if (clit != null)
                        {
                            newClientNetPay.AltNativeNumber = clit.AltPhoneNumber;
                            newClientNetPay.EmailAddress = clit.EmailAddress;
                            newClientNetPay.FirstName = clit.FirstName;
                            newClientNetPay.Gender = clit.Gender;
                            newClientNetPay.LastName = clit.LastName;
                            newClientNetPay.MiddleName = clit.MiddleName;
                            newClientNetPay.PhoneNumber = clit.PhoneNumber;
                        }

                        clientNetPays.Add(newClientNetPay);
                    }

                    if (clientNetPays != null && (clientNetPays.Count >= 0 || clientNetPays.Count <= 0))
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


                        var Pagenations = await new PagenationsHelper().GetPagenation<ClientCustomers>(clientNetPays, pagenationFilter);

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Customer retrieved successfully.", true, Pagenations, Pagenations, Status.Success, StatusMgs.Success);
                    }

                    // return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Clients Retrieved  Successfully.", true, clientNetPays, clientNetPays, Status.Success, StatusMgs.Success);

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No customer was found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No customer was found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetClientsNetPayHeader(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AccountId, string AppId, PagenationFilterDto pagenationFilter)
        {
            try
            {

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }


                var appId = long.Parse(AppId);
                var ClientNetPays = await lapoLoanDB.ClientNetPays.Where(s => s.ClientMonthlyNetPayId == appId).ToListAsync();

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
                        ClientNetPays = await lapoLoanDB.ClientNetPays.Where(n => n.ClientMonthlyNetPayId == appId && ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.Npfdate >= fromDate && n.Npfdate <= toDate)) && (n.NetPay.Equals(pagenationFilter.searchText) || n.PayGroup1.Equals(pagenationFilter.searchText) || n.PayGroup.Equals(pagenationFilter.searchText) || n.GradeStep.Equals(pagenationFilter.searchText) || n.Command.Equals(pagenationFilter.searchText) || n.Grade.Equals(pagenationFilter.searchText) || n.BankAccountName.Equals(pagenationFilter.searchText) || n.BankAccountNumber.Equals(pagenationFilter.searchText)) || lapoLoanDB.Clients.Where(s => s.Id == n.ClientId && (s.FullName.Equals(pagenationFilter.searchText) /*|| s.Pfnumber.Equals(pagenationFilter.searchText)*/)).Any()).ToListAsync();
                    }
                    else
                    {
                        ClientNetPays = await lapoLoanDB.ClientNetPays.Where( n =>  n.ClientMonthlyNetPayId == appId && ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.Npfdate >= fromDate && n.Npfdate <= toDate)) && n.Status == pagenationFilter.status && (n.NetPay.Equals(pagenationFilter.searchText) || n.PayGroup1.Equals(pagenationFilter.searchText)  || n.PayGroup.Equals(pagenationFilter.searchText) || n.GradeStep.Equals(pagenationFilter.searchText) || n.Command.Equals(pagenationFilter.searchText) || n.Grade.Equals(pagenationFilter.searchText)   || n.BankAccountName.Equals(pagenationFilter.searchText) || n.BankAccountNumber.Equals(pagenationFilter.searchText) || lapoLoanDB.Clients.Where(s => s.Id == n.ClientId && (s.FullName.Equals(pagenationFilter.searchText) || s.Pfnumber.Equals(pagenationFilter.searchText))).Any())).ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        ClientNetPays = await lapoLoanDB.ClientNetPays.Where(n => /*n.AccountId == acc &&*/ ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.Npfdate >= fromDate && n.Npfdate <= toDate)) && n.ClientMonthlyNetPayId == appId).ToListAsync();
                    }
                    else
                    {
                        ClientNetPays = await lapoLoanDB.ClientNetPays.Where(n => /*n.AccountId == acc &&*/ ((n.CreatedDate >= fromDate && n.CreatedDate <= toDate) || (n.Npfdate >= fromDate && n.Npfdate <= toDate)) && n.Status == pagenationFilter.status && n.ClientMonthlyNetPayId == appId).ToListAsync();
                    }
                }

                if (ClientNetPays == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You don't have any applied loan for now.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                ClientNetPays = ClientNetPays.OrderByDescending(s => s.CreatedDate).OrderByDescending(s => s.Status == "Success").ToList();

                if (ClientNetPays != null && ClientNetPays.Count > 1)
                {
                    List<ClientNetPayDto> clientNetPays = new List<ClientNetPayDto>();
                    int number = 0;

                    foreach (var itm in ClientNetPays)
                    {
                        var Clients = await lapoLoanDB.Clients.Where(s => s.Id == itm.ClientId).ToListAsync();

                        //if (pagenationFilter.IsSearchBar)
                        //{
                        //     Clients = await lapoLoanDB.Clients.Where(s => s.Id == itm.ClientId && (s.FullName.Equals(pagenationFilter.searchText) || s.Pfnumber.Equals(pagenationFilter.searchText))).ToListAsync();
                        //}

                        foreach (var clientNetPay in Clients)
                        {
                            number += 1;
                            var newClientNetPay = new ClientNetPayDto();
                            newClientNetPay.No = number;
                            newClientNetPay.FullName = clientNetPay.FullName; 
                            newClientNetPay.PFNumber =/* clientNetPay.Pfnumber*/ "";
                            newClientNetPay.AccountId = clientNetPay.AccountId.ToString();
                            newClientNetPay.CreatedDate = clientNetPay.CreatedDate.Value.ToLongDateString();
                            newClientNetPay.Status = clientNetPay.Status;
                            clientNetPays.Add(newClientNetPay);
                        }
                    }

                    clientNetPays = clientNetPays.DistinctBy(i => i.PFNumber).DistinctBy(i => i.PFNumber).ToList();

                    // clientNetPays = clientNetPays.Distinct().ToList();
                    // clientNetPays = clientNetPays.Select(c => c.PFNumber).ToList().ToList();

                    if (clientNetPays != null && (clientNetPays.Count >= 0 || clientNetPays.Count <= 0))
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


                        var Pagenation = await new PagenationsHelper().GetPagenation<ClientNetPayDto>(clientNetPays, pagenationFilter);

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                    }

                    //  return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Clients Retrieved  Successfully.", true, clientNetPays, clientNetPays, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Customer Net-Pays was not found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetClientsNetPayDetails(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AccountId)
        {
            try
            {
                var Client = await lapoLoanDB.Clients.Where(n => n.Pfnumber == AccountId).FirstOrDefaultAsync();
                if (Client != null)
                {
                    var ClientNetPays = await lapoLoanDB.ClientNetPays.Where(n => n.ClientId == Client.Id).ToListAsync();

                    List<ClientNetPayDetailsDto> clientNetPays = new List<ClientNetPayDetailsDto>();
                    int number = 0;

                    foreach (var clientNetPay in ClientNetPays)
                    {
                        number += 1;
                        var newClientNetPay = new ClientNetPayDetailsDto()
                        {
                            PFNumber = Client.Pfnumber,
                            No = number.ToString(),
                            BankAccountName = clientNetPay.BankAccountName,
                            BankAccountNumber = clientNetPay.BankAccountNumber,
                            BankName = clientNetPay.BankName,
                            ClientId = clientNetPay.ClientId.Value.ToString(),
                            CreatedDate = clientNetPay.CreatedDate.Value.ToLongDateString(),
                            CreatedAccountById = clientNetPay.CreatedAccountById.Value.ToString(),
                            Command = clientNetPay.Command,
                            Grade = clientNetPay.Grade,
                            GradeStep = clientNetPay.GradeStep,
                            Id = clientNetPay.Id.ToString(),
                            NetPay = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)clientNetPay.NetPay.Value)).Replace(".00", ""),
                            NPFDate = clientNetPay.Npfdate.Value.ToString(),
                            PayGroup = clientNetPay.PayGroup,
                            Status = clientNetPay.Status,
                            NetPayMoney = (double)clientNetPay.NetPay.Value
                        };
                        clientNetPays.Add(newClientNetPay);
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Clients Retrieved  Successfully.", true, clientNetPays, clientNetPays, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client Net-Pays not Retrieved Successfully.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> DeleteClientsNetPayDetails(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AccountId)
        {
            try
            {
                var AcctId = Convert.ToInt32(AccountId);
                var Client = await lapoLoanDB.ClientNetPays.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    lapoLoanDB.ClientNetPays.Remove(Client);
                    await lapoLoanDB.SaveChangesAsync();
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Clients Retrieved  Successfully.", true, null, null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client Net-Pays not Retrieved Successfully.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ValidateClientNumber(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string IPNumber)
        {
            try
            {
                if (IPNumber == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "IPPIS Number is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(IPNumber) || string.IsNullOrWhiteSpace(IPNumber))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "IPPIS Number is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var AccountAva = await lapoLoanDB.Clients.Where(x => x.Pfnumber == IPNumber).FirstOrDefaultAsync();

                var Data = new LoanScheduled.Model.ScheduledDto() { IPPISNumber = IPNumber, AccountId = "1" };

                var Result = await new LoanSchedulerHelpers(this.ControllerBase, this._configuration).CheckCustomerHasRunningLoanManual(_environment, Data);
                if (Result.IsActive == false)
                {
                    return Result;
                }

                if (AccountAva != null)
                {
                    if (lapoLoanDB.ClientNetPays.Where(x => x.ClientId == AccountAva.Id).ToList().Count >= 3)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "IPPIS Number is successful.", true, "", null, Status.Success, StatusMgs.Success);
                    }
                    else
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your IPPIS Number is not eligible to take loan, you must has atleast 3 months pay slip.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid IPPIS Number, You don't have permission to apply for this loan. Contact the admin to upload your monthly pay slip.", false, "", null, Status.Ërror, StatusMgs.Error);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid IPPIS Number, You don't have permission to apply for this loan. Contact the admin to upload your monthly pay slip.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid IPPIS Number, You don't have permission to apply for this loan. Contact the admin to upload your monthly pay slip.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        private int fileUploadCount = 0;
        public async Task<RespondMessageDto> ValidateClientIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, List<ClientNetPayINFO> clientNetPays)
        {
            try
            {
                foreach (var clientNetPay in clientNetPays)
                {
                    var ResultClientIPFUpload = this.ValidateClientIPFUpload(clientNetPay);
                    if (ResultClientIPFUpload.IsActive == false)
                    {
                        return ResultClientIPFUpload;
                    }
                }

                foreach (var clientNetPay in clientNetPays)
                {
                    var AccountAva = await lapoLoanDB.Clients.Where(x => x.Pfnumber == clientNetPay.Staff_Id).AnyAsync();

                    if (AccountAva)
                    {
                        await this.CreateIPFNumberUpload(_environment, clientNetPay, clientNetPays.Count);
                    }
                    else
                    {
                        await this.CreateClientIPFNumberUpload(_environment, clientNetPay, clientNetPays.Count);
                    }
                }

                var subCount = fileUploadCount;
                if (fileUploadCount > 0)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, subCount + " Client Net-Pay was uploaded successful.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client Net-Pay was not uploaded successful.", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public RespondMessageDto ValidateClientIPFUpload(ClientNetPayINFO clientNetPay)
        {
            if (clientNetPay == null)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid data along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
            }

            if (string.IsNullOrWhiteSpace(clientNetPay.Staff_Id) || string.IsNullOrEmpty(clientNetPay.Staff_Id) || string.IsNullOrWhiteSpace(clientNetPay.Net_Pay.ToString()) || string.IsNullOrEmpty(clientNetPay.Net_Pay.ToString()) || string.IsNullOrWhiteSpace(clientNetPay.Full_Name) || string.IsNullOrEmpty(clientNetPay.Full_Name) || string.IsNullOrWhiteSpace(clientNetPay.Bank_Name) || string.IsNullOrEmpty(clientNetPay.Bank_Name) || string.IsNullOrWhiteSpace(clientNetPay.Command) || string.IsNullOrEmpty(clientNetPay.Command) || string.IsNullOrWhiteSpace(clientNetPay.Full_Name) || string.IsNullOrEmpty(clientNetPay.Full_Name) || string.IsNullOrWhiteSpace(clientNetPay.Grade) || string.IsNullOrEmpty(clientNetPay.Grade) || string.IsNullOrWhiteSpace(clientNetPay.Grade_Step) || string.IsNullOrEmpty(clientNetPay.Grade_Step) || string.IsNullOrWhiteSpace(clientNetPay.NPFDate.ToString()) || string.IsNullOrEmpty(clientNetPay.NPFDate.ToString()) || string.IsNullOrWhiteSpace(clientNetPay.Staff_Id) || string.IsNullOrEmpty(clientNetPay.Staff_Id))
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid data along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
            }

            if (clientNetPay.Net_Pay <= 0)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid net-pay along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
            }

            if (clientNetPay.NPFDate.Month > 28 && clientNetPay.NPFDate.Month < 31)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid date along the line.", false, "", null, Status.Ërror, StatusMgs.Error);
            }

            //DateTime Na; 
            //if (DateTime.TryParse(clientNetPay.NPFDate.ToString(),  Na))
            //{
            //    return false;
            //}

            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Successful.", true, "", null, Status.Success, StatusMgs.Success);
        }

        public async Task<RespondMessageDto> CreateClientIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay, int totalClinetFile)
        {
            try
            {
                var AccountAva = await lapoLoanDB.Clients.Where(x => x.Pfnumber == clientNetPay.Staff_Id).AnyAsync();

                if (AccountAva == false && this.ValidateClientIPFUpload(clientNetPay).IsActive)
                {

                    var NewClient = new Client()
                    {
                        FullName = clientNetPay.Full_Name,
                        Pfnumber = clientNetPay.Staff_Id,
                        Status = StatusMgs.Success,
                        CreatedAccountById = 34,
                        CreatedDate = DateTime.Now,
                        Npfdate = clientNetPay.NPFDate,
                    };
                    lapoLoanDB.Clients.Add(NewClient);
                    await lapoLoanDB.SaveChangesAsync();

                    var NewClient1 = new ClientNetPay()
                    {
                        ClientId = NewClient.Id,
                        Command = clientNetPay.Command,
                        NetPay = (decimal)clientNetPay.Net_Pay,
                        GradeStep = clientNetPay.Grade_Step,
                        Grade = clientNetPay.Grade,
                        BankName = clientNetPay.Bank_Name,
                        BankAccountName = NewClient.FullName,
                        BankAccountNumber = clientNetPay.Account_Number.ToString(),
                        CreatedAccountById = 34,
                        Status = StatusMgs.Success,
                        CreatedDate = DateTime.Now,
                        Npfdate = clientNetPay.NPFDate,
                    };
                    lapoLoanDB.ClientNetPays.Add(NewClient1);
                    await lapoLoanDB.SaveChangesAsync();

                    fileUploadCount += 1;

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Client IPPIS account created successful.", true, "", null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client IPPIS account was not create.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CreateIPFNumberUpload(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ClientNetPayINFO clientNetPay, int totalClinetFile)
        {
            try
            {
                var AccountAva = await lapoLoanDB.Clients.Where(x => x.Pfnumber == clientNetPay.Staff_Id && x.Status == StatusMgs.Success).FirstOrDefaultAsync();

                if (AccountAva != null && this.ValidateClientIPFUpload(clientNetPay).IsActive)
                {
                    //var AccountAv = await lapoLoanDB.ClientNetPays.Where(x => x.ClientId == AccountAva.Id && x.Status == StatusMgs.Success ).FirstOrDefaultAsync();

                    //if (AccountAv == null)
                    //{
                    var AccountAvDate = await lapoLoanDB.ClientNetPays.Where(x => x.ClientId == AccountAva.Id && (clientNetPay.NPFDate >= x.Npfdate && clientNetPay.NPFDate <= x.Npfdate)).AnyAsync();

                    if (AccountAvDate == false && !string.IsNullOrWhiteSpace(clientNetPay.Staff_Id) && !string.IsNullOrEmpty(clientNetPay.Staff_Id))
                    {
                        var NewClientNetPay = new ClientNetPay()
                        {
                            ClientId = AccountAva.Id,
                            Command = clientNetPay.Command,
                            NetPay = (decimal)clientNetPay.Net_Pay,
                            GradeStep = clientNetPay.Grade_Step,
                            Grade = clientNetPay.Grade,
                            BankName = clientNetPay.Bank_Name,
                            BankAccountName = AccountAva.FullName,
                            BankAccountNumber = clientNetPay.Account_Number.ToString(),
                            CreatedAccountById = 34,
                            Status = StatusMgs.Success,
                            CreatedDate = DateTime.Now,
                            Npfdate = clientNetPay.NPFDate,
                        };
                        lapoLoanDB.ClientNetPays.Add(NewClientNetPay);
                        await lapoLoanDB.SaveChangesAsync();

                        fileUploadCount += 1;

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Client IPPIS NetPay created successful.", true, "", null, Status.Success, StatusMgs.Success);
                    }
                    //}
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client IPPIS NetPay was not create.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetUserProfileDetails(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, int AccountId)
        {
            try
            {
                var AcctId = Convert.ToInt64(AccountId);
                var Client = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var newAcctDetail = new AccountDetails()
                    {
                        AccountType = Client.AccountType,
                        AccountVerify = Client.AccountVerify.Value,
                        AllowLoginTwoFactor = Client.AllowLoginTwoFactor.Value,
                        CreatedDate = Client.CreatedDate.ToLongDateString() + ", Time " + Client.CreatedDate.ToLongTimeString(),
                        LastLoginDate = Client.LastLoginDate.Value.ToLongDateString() + ", Time " + Client.LastLoginDate.Value.ToLongTimeString(),
                        Username = Client.Username,
                        Id = lapoCipher01.EnryptString(Client.Id.ToString()),
                    };

                    var PersonDetails = await lapoLoanDB.People.Where(n => n.AccountId == Client.Id).FirstOrDefaultAsync();

                    if (PersonDetails != null)
                    {
                        var newDetails = new UserProfileDetails()
                        {
                            Age = PersonDetails.Age,
                            EmailAddress = PersonDetails.EmailAddress,
                            FirstName = PersonDetails.FirstName,
                            CreatedDate = PersonDetails.CreatedDate.ToLongDateString() + ",  Time " +  PersonDetails.CreatedDate.ToLongTimeString(),
                            AltPhoneNumber = PersonDetails.AltPhoneNumber,
                            CurrentAddress = PersonDetails.CurrentAddress,
                            Gender = PersonDetails.Gender,
                            LastName = PersonDetails.LastName,
                            PhoneNumber = PersonDetails.PhoneNumber,
                            Staff = PersonDetails.Staff,
                            PositionOrRole = PersonDetails.PositionOrRole,
                            AccountId = lapoCipher01.EnryptString(PersonDetails.AccountId.ToString()),
                            Id = lapoCipher01.EnryptString(PersonDetails.Id.ToString()),
                            MiddleName = PersonDetails.MiddleName,
                            HubGroup = "",
                            RoleType = "",
                        };

                        newDetails.MarrintalStatus = PersonDetails.MarrintalStatus == null || PersonDetails.MarrintalStatus == "" ? "" : PersonDetails.MarrintalStatus;

                        try
                        {
                            newDetails.RetirementDate = PersonDetails.RetirementDate != null && PersonDetails.RetirementDate.HasValue ? PersonDetails.RetirementDate.Value.ToLongDateString() : "";

                            newDetails.EmploymentDate = PersonDetails.EmploymentDate != null && PersonDetails.EmploymentDate.HasValue ? PersonDetails.EmploymentDate.Value.ToLongDateString() : "";

                            newDetails.BankVerificationNumber = Client.BvnAccount == null || Client.BvnAccount == "" ? "No BVN yet" : Client.BvnAccount;

                            var Hubass = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == PersonDetails.AccountId).FirstOrDefaultAsync();

                            if (Hubass != null)
                            {
                                newDetails.BVN = Hubass.Bvn;
                                newDetails.Occuptaion = Hubass.IndustrySegment + " - " + Hubass.BusinessType;
                            }
                            else
                            {
                                newDetails.BVN = "No BVN yet";
                                newDetails.Occuptaion = "No Occuptaion yet";
                            }
                        }
                        catch(Exception exx)
                        {

                        }

                        var HubTeam = await lapoLoanDB.HubTeams.Where(n => n.TeamAccountId == PersonDetails.AccountId).FirstOrDefaultAsync();

                        if (HubTeam != null)
                        {
                            if(HubTeam.BackEndUsers.ToLower().Contains("AND ACCOUNT".ToLower()))
                            {
                                newDetails.RoleType = HubTeam.BackEndUsers.Replace("AND ACCOUNT" , "");
                            }
                            else
                            {
                                newDetails.RoleType = HubTeam.BackEndUsers;
                            }

                            var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(n => n.Id == HubTeam.GroupId).FirstOrDefaultAsync();

                            if (HubTeamGroup != null)
                            {
                                newDetails.HubGroup = HubTeamGroup.HubTeamGroupName;
                            }
                        }

                        newAcctDetail.userProfileDetails = newDetails;
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Client Personal Details Retrieved  Successfully.", true, newAcctDetail, newAcctDetail, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client Personal Details was not Retrieved", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetUserPermissionDetails(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, int AccountId)
        {
            try
            {
                var AcctId = Convert.ToInt32(AccountId);
                var Client = await lapoLoanDB.HubTeams.Where(n => n.TeamAccountId == AcctId /*&& n.Status == StatusMgs.Active*/).FirstOrDefaultAsync();

                if (Client != null)
                {
                    var newAcctDetail = new AccountPermissionDetails()
                    {
                        IsASSISTANTHEADOFOPERATION = false,
                        IsDISBURSEMENTOFFICER = false,
                        IsGROUPHEAD = false,
                        IsHEADOFOPERATIONS = false,
                        IsRELATIONSHIPOFFICER = false,
                        IsTEAMLEADS = false,
                        IsRECONCILIATIONANDACCOUNTOFFICER = false,

                        IsCustomerLoanPermission = false,
                        IsGeneralPermissionsAccessRight = false,
                        IsLoanSettingAccessRight = false,
                        IsNetPaysAccessRight = false,
                        IsTenureAccessRight = false,
                        GroupName = "Default",
                        GroupId = 0,
                        TeamId = 0,


                        AccessRightToViewUploadBackRepaymentLoan = Client.AccessRightToViewUploadBackRepaymentLoan.Value,
                        AccessRightToApprovedLoan = Client.AccessRighttoapprovecustomerloan.Value,
                        AccessRightToCancelLoan = Client.AccessRighttorejectaloan.Value,
                        AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value,
                        AccessRightToAnonymousLoanApplication = Client.AccessRightToAnonymousLoanApplication.Value,
                        AccessRightToUploadBackDISBURSEMENTLoan = Client.AccessRightToUploadBackDisbursementloan.Value,

                        AccessRightToUploadBackRepaymentLoan = Client.AccessRightToUploadBackRepaymentLoan.Value,
                        AccessRightToPrintLoan = Client.AccessRightToPrintLoan.Value,
                        AccessRightToProceedLoan = Client.AccessRightToProceedLoan.Value,
                        AccessRightToViewDisbursementLoan = Client.AccessRightToViewDisbursementLoan.Value,
                        //AccessRightToViewDisbursementLoan
                        AccessRighttoapprovecustomerloan = Client.AccessRighttoapprovecustomerloan.Value,
                        AccessRighttocreateahub = Client.AccessRighttocreateahub.Value,
                        AccessRighttocreateateammember = Client.AccessRighttocreateateammember.Value,
                        AccessRighttocreatetenure = Client.AccessRighttocreatetenure.Value,
                        AccessRighttodisablecustomerstoapplyforaloan = Client.AccessRighttodisablecustomerstoapplyforaloan.Value,
                        AccessRighttodisablehubs = Client.AccessRighttodisablehubs.Value,

                        AccessRightToEditTeamMemberPermissions = Client.AccessRightToEditTeamMemberPermissions.Value,
                        AccessRightToExportDisbursementloan1 = Client.AccessRightToExportDisbursementloan.Value,

                        AccessRightToExportDISBURSEMENTLoan = Client.AccessRightToExportDisbursementloan.Value,
                     
                        AccessRighttoloansettings = Client.AccessRighttoloansettings.Value,
                        AccessRighttorejectaloan = Client.AccessRighttorejectaloan.Value,
                        AccessRighttoteamsAndpermissions = Client.AccessRighttoteamsAndpermissions.Value,

                        //AccessRightToUploadBackDisbursementloan = Client.AccessRightToUploadBackDisbursementloan.Value,

                        AccessRighttoviewveammembers = Client.AccessRighttoviewveammembers.Value,
                        AccessRighttoviewcustomersloans = Client.AccessRighttoviewcustomersloans.Value,
                        AccessRighttoviewcustomers = Client.AccessRighttoviewcustomers.Value,

                        AccessRighttoviewhubs = Client.AccessRighttoviewhubs.Value,
                        AccessRighttoviewloandetails = Client.AccessRighttoviewloandetails.Value,
                        AccessRighttoviewtenure = Client.AccessRighttoviewtenure.Value,

                        CreateLoanNarration = Client.CreateLoanNarration.Value,
                        ViewLoanNarration = Client.ViewLoanNarration.Value, 
                        IsDeveloperTeam =false,
                    };

                    var SecurityPer = await lapoLoanDB.SecurityPermissions.Where(n => n.AccountId == AcctId && (n.Status == StatusMgs.Active || n.Status == StatusMgs.Success)).FirstOrDefaultAsync();

                    if (SecurityPer != null)
                    {
                        newAcctDetail.IsCustomerLoanPermission = SecurityPer.CustomerLoanPermission.Value;

                        newAcctDetail.IsGeneralPermissionsAccessRight = SecurityPer.GeneralPermissionsAccessRight.Value;

                        newAcctDetail.IsLoanSettingAccessRight = SecurityPer.LoanSettingAccessRight.Value;

                        newAcctDetail.IsNetPaysAccessRight = SecurityPer.NetPaysAccessRight.Value;

                        newAcctDetail.IsTenureAccessRight = SecurityPer.TenureAccessRight.Value;
                    }

                    if (Client != null)
                    {
                        newAcctDetail.GroupId = Client.GroupId.Value;
                        newAcctDetail.TeamId = Client.Id;

                       var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(n => n.Id == Client.GroupId /*&& n.Status == StatusMgs.Active*/).FirstOrDefaultAsync();

                        if (HubTeamGroup != null)
                        {
                            newAcctDetail.GroupName = HubTeamGroup.HubTeamGroupName.ToString();
                        }
                    }

                    switch (Client.BackEndUsers)
                    {
                        case "TEAM LEADS":
                            newAcctDetail.AccessRightToViewDisbursementLoan = Client.AccessRightToViewDisbursementLoan.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.IsTEAMLEADS = true;
                            break;
                        case "TEAM LEAD":
                            newAcctDetail.AccessRightToViewDisbursementLoan = Client.AccessRightToViewDisbursementLoan.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.IsTEAMLEADS = true;
                            break;
                        case "RECONCILIATION AND ACCOUNT OFFICER":
                            newAcctDetail.AccessRightToCancelLoan = Client.AccessRighttorejectaloan.Value;
                            newAcctDetail.AccessRightToApprovedLoan = Client.AccessRighttoapprovecustomerloan.Value;
                            newAcctDetail.AccessRightToPrintLoan = Client.AccessRightToPrintLoan.Value;
                            newAcctDetail.AccessRightToProceedLoan = Client.AccessRightToProceedLoan.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.IsRECONCILIATIONANDACCOUNTOFFICER = true;
                            break;
                        case "DISBURSEMENT OFFICER":
                            newAcctDetail.AccessRightToPrintLoan = Client.AccessRightToPrintLoan.Value;
                            newAcctDetail.AccessRightToViewDisbursementLoan = Client.AccessRightToViewDisbursementLoan.Value;
                            newAcctDetail.AccessRightToUploadBackDISBURSEMENTLoan = Client.AccessRightToUploadBackDisbursementloan.Value;
                            newAcctDetail.AccessRightToExportDISBURSEMENTLoan = Client.AccessRightToExportDisbursementloan.Value;
                            newAcctDetail.IsDISBURSEMENTOFFICER = true;
                            break;
                        //case "GROUP HEAD":
                        //    newAcctDetail.AccessRightToViewDisbursementLoan = true;
                        //    newAcctDetail.AccessRightToViewUploadBackRepaymentLoan = true;
                        //    newAcctDetail.AccessRightToViewLoan = true;
                        //    newAcctDetail.AccessRightToAnonymousLoanApplication = true;
                        //    newAcctDetail.IsGROUPHEAD = true;
                        //    break;
                        case "HEAD OF OPERATIONS":
                            newAcctDetail.AccessRightToViewDisbursementLoan = Client.AccessRightToViewDisbursementLoan.Value;
                            newAcctDetail.AccessRightToViewUploadBackRepaymentLoan = Client.AccessRightToViewUploadBackRepaymentLoan.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.AccessRightToAnonymousLoanApplication = Client.AccessRightToAnonymousLoanApplication.Value;
                            newAcctDetail.IsHEADOFOPERATIONS = true;
                            break;
                        case "ASSISTANT HEAD OF OPERATION":
                            newAcctDetail.AccessRightToViewUploadBackRepaymentLoan = Client.AccessRightToViewUploadBackRepaymentLoan.Value;
                            newAcctDetail.AccessRightToUploadBackRepaymentLoan = Client.AccessRightToUploadBackRepaymentLoan.Value;
                            newAcctDetail.IsASSISTANTHEADOFOPERATION = true;
                            break;
                        case "RELATIONSHIP OFFICER":
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.IsRELATIONSHIPOFFICER = true;
                            break;
                        case "DEVELOPER TEAM" :
                            newAcctDetail.AccessRightToViewUploadBackRepaymentLoan = Client.AccessRightToViewUploadBackRepaymentLoan.Value;
                            newAcctDetail.AccessRightToApprovedLoan = Client.AccessRighttoapprovecustomerloan.Value;
                            newAcctDetail.AccessRightToCancelLoan = Client.AccessRighttorejectaloan.Value;
                            newAcctDetail.AccessRightToExportDISBURSEMENTLoan = Client.AccessRightToExportDisbursementloan.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.AccessRightToAnonymousLoanApplication = Client.AccessRightToAnonymousLoanApplication.Value;
                            newAcctDetail.AccessRightToUploadBackDISBURSEMENTLoan = Client.AccessRightToUploadBackDisbursementloan.Value; 
                            newAcctDetail.AccessRightToUploadBackRepaymentLoan = Client.AccessRightToUploadBackRepaymentLoan.Value;

                            newAcctDetail.IsGROUPHEAD = false;
                            newAcctDetail.IsDISBURSEMENTOFFICER = false;
                            newAcctDetail.IsRECONCILIATIONANDACCOUNTOFFICER = false;

                            newAcctDetail.IsDeveloperTeam = true;
                            newAcctDetail.AccessRightToPrintLoan = Client.AccessRightToPrintLoan.Value;
                            newAcctDetail.AccessRightToProceedLoan = Client.AccessRightToProceedLoan.Value;
                            break;
                        case  "GROUP HEAD" :
                            newAcctDetail.AccessRightToViewUploadBackRepaymentLoan = Client.AccessRightToViewUploadBackRepaymentLoan.Value;
                            newAcctDetail.AccessRightToApprovedLoan = Client.AccessRighttoapprovecustomerloan.Value;
                            newAcctDetail.AccessRightToCancelLoan = Client.AccessRighttorejectaloan.Value;
                            newAcctDetail.AccessRightToExportDISBURSEMENTLoan = Client.AccessRightToExportDisbursementloan.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.AccessRightToAnonymousLoanApplication = Client.AccessRightToAnonymousLoanApplication.Value;
                            newAcctDetail.AccessRightToUploadBackDISBURSEMENTLoan = Client.AccessRightToUploadBackDisbursementloan.Value;
                            newAcctDetail.AccessRightToUploadBackRepaymentLoan = Client.AccessRightToUploadBackRepaymentLoan.Value;

                            newAcctDetail.IsGROUPHEAD = true;
                            newAcctDetail.IsDISBURSEMENTOFFICER = false;
                            newAcctDetail.IsRECONCILIATIONANDACCOUNTOFFICER = false;
                            newAcctDetail.IsDeveloperTeam = false;

                            newAcctDetail.AccessRightToPrintLoan = Client.AccessRightToPrintLoan.Value;
                            newAcctDetail.AccessRightToProceedLoan = Client.AccessRightToProceedLoan.Value;
                            break;
                        default:
                            newAcctDetail.AccessRightToViewUploadBackRepaymentLoan = Client.AccessRightToViewUploadBackRepaymentLoan.Value;
                            newAcctDetail.AccessRightToApprovedLoan = Client.AccessRighttoapprovecustomerloan.Value;
                            newAcctDetail.AccessRightToCancelLoan = Client.AccessRighttorejectaloan.Value;
                            newAcctDetail.AccessRightToExportDISBURSEMENTLoan = Client.AccessRightToExportDisbursementloan.Value;
                            newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                            newAcctDetail.AccessRightToAnonymousLoanApplication = Client.AccessRightToAnonymousLoanApplication.Value;
                            newAcctDetail.AccessRightToUploadBackDISBURSEMENTLoan = Client.AccessRightToUploadBackDisbursementloan.Value;   newAcctDetail.AccessRightToUploadBackRepaymentLoan = Client.AccessRightToUploadBackRepaymentLoan.Value;
                            newAcctDetail.IsDeveloperTeam = true;
                            newAcctDetail.AccessRightToPrintLoan = Client.AccessRightToPrintLoan.Value;
                            newAcctDetail.AccessRightToProceedLoan = Client.AccessRightToProceedLoan.Value;
                            break;
                    }

                    try
                    {
                        newAcctDetail.AccessRightToViewUploadBackRepaymentLoan = Client.AccessRightToViewUploadBackRepaymentLoan.Value;
                        newAcctDetail.AccessRightToApprovedLoan = Client.AccessRighttoapprovecustomerloan.Value;
                        newAcctDetail.AccessRightToCancelLoan = Client.AccessRighttorejectaloan.Value;

                        newAcctDetail.AccessRightToViewLoan = Client.AccessRighttoviewcustomersloans.Value;
                        newAcctDetail.AccessRightToAnonymousLoanApplication = Client.AccessRightToAnonymousLoanApplication.Value;

                        newAcctDetail.AccessRightToUploadBackDISBURSEMENTLoan = Client.AccessRightToUploadBackDisbursementloan.Value;

                        newAcctDetail.AccessRightToUploadBackRepaymentLoan = Client.AccessRightToUploadBackRepaymentLoan.Value;
                        newAcctDetail.AccessRightToPrintLoan = Client.AccessRightToPrintLoan.Value;
                        newAcctDetail.AccessRightToProceedLoan = Client.AccessRightToProceedLoan.Value;
                        newAcctDetail.AccessRightToViewDisbursementLoan = Client.AccessRightToViewDisbursementLoan.Value;

                        newAcctDetail.AccessRighttoapprovecustomerloan = Client.AccessRighttoapprovecustomerloan.Value;
                        newAcctDetail.AccessRighttocreateahub = Client.AccessRighttocreateahub.Value;
                        newAcctDetail.AccessRighttocreateateammember = Client.AccessRighttocreateateammember.Value;
                        newAcctDetail.AccessRighttocreatetenure = Client.AccessRighttocreatetenure.Value;
                        newAcctDetail.AccessRighttodisablecustomerstoapplyforaloan = Client.AccessRighttodisablecustomerstoapplyforaloan.Value;
                        newAcctDetail.AccessRighttodisablehubs = Client.AccessRighttodisablehubs.Value;

                        newAcctDetail.AccessRightToEditTeamMemberPermissions = Client.AccessRightToEditTeamMemberPermissions.Value;

                        newAcctDetail.AccessRightToExportDisbursementloan1 = Client.AccessRightToExportDisbursementloan.Value;

                        newAcctDetail.AccessRightToExportDISBURSEMENTLoan = Client.AccessRightToExportDisbursementloan.Value;

                        newAcctDetail.AccessRighttoloansettings = Client.AccessRighttoloansettings.Value;
                        newAcctDetail.AccessRighttorejectaloan = Client.AccessRighttorejectaloan.Value;
                        newAcctDetail.AccessRighttoteamsAndpermissions = Client.AccessRighttoteamsAndpermissions.Value;

                        //AccessRightToUploadBackDisbursementloan = Client.AccessRightToUploadBackDisbursementloan.Value,

                        newAcctDetail.AccessRighttoviewveammembers = Client.AccessRighttoviewveammembers.Value;
                        newAcctDetail.AccessRighttoviewcustomersloans = Client.AccessRighttoviewcustomersloans.Value;
                        newAcctDetail.AccessRighttoviewcustomers = Client.AccessRighttoviewcustomers.Value;

                        newAcctDetail.AccessRighttoviewhubs = Client.AccessRighttoviewhubs.Value;
                        newAcctDetail.AccessRighttoviewloandetails = Client.AccessRighttoviewloandetails.Value;
                        newAcctDetail.AccessRighttoviewtenure = Client.AccessRighttoviewtenure.Value;

                        newAcctDetail.CreateLoanNarration = Client.CreateLoanNarration.Value;
                        newAcctDetail.ViewLoanNarration = Client.ViewLoanNarration.Value;
                    }
                    catch(Exception exxx)
                    {

                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Client Personal Details Retrieved  Successfully.", true, newAcctDetail, newAcctDetail, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client Personal Details was not Retrieved", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> UpdateProfile(UpdateProfileModelDto updateProfile)
        {
            try
            {
                if (string.IsNullOrEmpty(updateProfile.FirstName) || string.IsNullOrWhiteSpace(updateProfile.FirstName))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "First Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.Middle) || string.IsNullOrWhiteSpace(updateProfile.Middle))
                //{

                //    updateProfile.Middle = "";
                //    //return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Middle Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (string.IsNullOrEmpty(updateProfile.LastName) || string.IsNullOrWhiteSpace(updateProfile.LastName))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Last Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (string.IsNullOrEmpty(updateProfile.PhoneNumber) || string.IsNullOrWhiteSpace(updateProfile.PhoneNumber))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Phone Number is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.AltPhoneNumber) || string.IsNullOrWhiteSpace(updateProfile.AltPhoneNumber))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Alternative Phone Number is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                //if (string.IsNullOrEmpty(updateProfile.Age) || string.IsNullOrWhiteSpace(updateProfile.Age))
                //{
                //    //return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your age is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (string.IsNullOrWhiteSpace(updateProfile.MarrintalStatus) || string.IsNullOrEmpty(updateProfile.MarrintalStatus))
                {
                    updateProfile.MarrintalStatus = "";
                }

                if (string.IsNullOrEmpty(updateProfile.EmailAddress) || string.IsNullOrWhiteSpace(updateProfile.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Email Address is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (string.IsNullOrEmpty(updateProfile.CurrentAddress) || string.IsNullOrWhiteSpace(updateProfile.CurrentAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Current Address is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (string.IsNullOrEmpty(updateProfile.Gender) || string.IsNullOrWhiteSpace(updateProfile.Gender))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Gender is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.Middle) || string.IsNullOrWhiteSpace(updateProfile.Middle))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your Middle Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (updateProfile.PhoneNumber.Length < 11 || updateProfile.PhoneNumber.Length > 11)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Phone number must be 11 digits number", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.Age) || string.IsNullOrWhiteSpace(updateProfile.Age))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Date-Of-Birth is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                var DatetOdat = DateTime.Now;

                //if (Convert.ToDateTime(updateProfile.Age) >= DatetOdat)
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Date-Of-Birth is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                //var Dat = (DatetOdat.Year - Convert.ToDateTime(updateProfile.Age).Year);

                //if (Dat < 18)
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Date-Of-Birth is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (updateProfile.AltPhoneNumber.Length > 0)
                {
                    //if (updateProfile.AltPhoneNumber.Length > 11 || updateProfile.AltPhoneNumber.Length < 11)
                    //{
                    //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Alternative phone number must be 11 digit number.", false, null, null, Status.Failed, StatusMgs.Failed);
                    //}
                }
                else
                {
                    updateProfile.AltPhoneNumber = updateProfile.PhoneNumber;
                }

                var AcctId = Convert.ToInt32(updateProfile.AcctId);

                var AcctSyc = await lapoLoanDB.SecurityAccounts.Where(s => s.Id == AcctId).FirstOrDefaultAsync();

                if (AcctSyc != null)
                {
                    //var personPhoneExit = await lapoLoanDB.People.Where(s => s.AccountId != AcctSyc.Id && s.PhoneNumber == updateProfile.PhoneNumber).FirstOrDefaultAsync();

                    //if (personPhoneExit != null)
                    //{
                    //    if (string.IsNullOrEmpty(personPhoneExit.PhoneNumber) || string.IsNullOrWhiteSpace(personPhoneExit.PhoneNumber))
                    //    {
                    //        if (personPhoneExit != null)
                    //        {
                    //            /// return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "This Phone Number is already been used by another user", false, null, null, Status.Failed, StatusMgs.Failed);
                    //        }
                    //    }
                    //    else
                    //    {

                    //    }
                    //}

                    var PersonSyc = await lapoLoanDB.People.Where(s => s.AccountId == AcctSyc.Id).FirstOrDefaultAsync();

                    if (PersonSyc != null)
                    {
                        PersonSyc.AltPhoneNumber = updateProfile.AltPhoneNumber;
                        PersonSyc.PhoneNumber = updateProfile.PhoneNumber;
                        PersonSyc.LastName = updateProfile.LastName;
                        PersonSyc.Age = updateProfile.Age;
                        // Convert.ToDateTime(updateProfile.Age).ToLongDateString();
                        PersonSyc.EmailAddress = updateProfile.EmailAddress;
                        PersonSyc.CurrentAddress = updateProfile.CurrentAddress;
                        PersonSyc.Gender = updateProfile.Gender;
                        PersonSyc.MiddleName = updateProfile.Middle;
                        PersonSyc.FirstName = updateProfile.FirstName;
                        PersonSyc.MarrintalStatus = updateProfile.MarrintalStatus;

                        lapoLoanDB.Entry(PersonSyc).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your profile details has been saved successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your profile details has not save, Fill the form properly and try again.", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> UpdateProfile1(UpdateProfileModelDto updateProfile)
        {
            try
            {
                if (string.IsNullOrEmpty(updateProfile.FirstName) || string.IsNullOrWhiteSpace(updateProfile.FirstName))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "First Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.Middle) || string.IsNullOrWhiteSpace(updateProfile.Middle))
                //{

                //    updateProfile.Middle = "";
                //    //return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Middle Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (string.IsNullOrEmpty(updateProfile.LastName) || string.IsNullOrWhiteSpace(updateProfile.LastName))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Last Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (string.IsNullOrEmpty(updateProfile.PhoneNumber) || string.IsNullOrWhiteSpace(updateProfile.PhoneNumber))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Phone Number is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.AltPhoneNumber) || string.IsNullOrWhiteSpace(updateProfile.AltPhoneNumber))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Alternative Phone Number is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                //if (string.IsNullOrEmpty(updateProfile.Age) || string.IsNullOrWhiteSpace(updateProfile.Age))
                //{
                //    //return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your age is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (string.IsNullOrWhiteSpace(updateProfile.MarrintalStatus) || string.IsNullOrEmpty(updateProfile.MarrintalStatus))
                {
                    updateProfile.MarrintalStatus = "";
                }

                if (string.IsNullOrEmpty(updateProfile.EmailAddress) || string.IsNullOrWhiteSpace(updateProfile.EmailAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Email Address is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (string.IsNullOrEmpty(updateProfile.CurrentAddress) || string.IsNullOrWhiteSpace(updateProfile.CurrentAddress))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Current Address is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                if (string.IsNullOrEmpty(updateProfile.Gender) || string.IsNullOrWhiteSpace(updateProfile.Gender))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Gender is required", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.Middle) || string.IsNullOrWhiteSpace(updateProfile.Middle))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your Middle Name is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (updateProfile.PhoneNumber.Length < 11 || updateProfile.PhoneNumber.Length > 11)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Phone number must be 11 digits number", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //if (string.IsNullOrEmpty(updateProfile.Age) || string.IsNullOrWhiteSpace(updateProfile.Age))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Date-Of-Birth is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                var DatetOdat = DateTime.Now;

                //if (Convert.ToDateTime(updateProfile.Age) >= DatetOdat)
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Date-Of-Birth is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                //var Dat = (DatetOdat.Year - Convert.ToDateTime(updateProfile.Age).Year);

                //if (Dat < 18)
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Date-Of-Birth is required", false, null, null, Status.Failed, StatusMgs.Failed);
                //}

                if (updateProfile.AltPhoneNumber.Length > 0)
                {
                    //if (updateProfile.AltPhoneNumber.Length > 11 || updateProfile.AltPhoneNumber.Length < 11)
                    //{
                    //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Alternative phone number must be 11 digit number.", false, null, null, Status.Failed, StatusMgs.Failed);
                    //}
                }
                else
                {
                    updateProfile.AltPhoneNumber = updateProfile.PhoneNumber;
                }

                var AcctId = Convert.ToInt32(updateProfile.AcctId);

                var AcctSyc = await lapoLoanDB.SecurityAccounts.Where(s => s.Id == AcctId).FirstOrDefaultAsync();

                if (AcctSyc != null)
                {
                    var PersonSyc = await lapoLoanDB.People.Where(s => s.AccountId == AcctSyc.Id).FirstOrDefaultAsync();

                    if (PersonSyc != null)
                    {
                        PersonSyc.AltPhoneNumber = updateProfile.AltPhoneNumber;
                        //PersonSyc.PhoneNumber = updateProfile.PhoneNumber;
                        //PersonSyc.LastName = updateProfile.LastName;

                   // PersonSyc.Age = updateProfile.Age.Length <= 4 ? updateProfile.Age :  Convert.ToDateTime(updateProfile.Age).ToLongDateString();

                        PersonSyc.EmailAddress = updateProfile.EmailAddress;
                        PersonSyc.CurrentAddress = updateProfile.CurrentAddress;
                        PersonSyc.Gender = updateProfile.Gender;
                        PersonSyc.MiddleName = updateProfile.Middle;
                        //PersonSyc.FirstName = updateProfile.FirstName;
                        PersonSyc.MarrintalStatus = updateProfile.MarrintalStatus;

                        lapoLoanDB.Entry(PersonSyc).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your profile details has been saved successfully.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Failed, "Your profile details has not save, Fill the form properly and try again.", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateTwoFactorAuth(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, int AccountId)
        {
            try
            {
                var AcctId = Convert.ToInt32(AccountId);
                var Client = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                if (Client != null)
                {

                    if (Client.AllowLoginTwoFactor.Value)
                    {
                        Client.AllowLoginTwoFactor = false;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Login two-factor has been de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                    else
                    {
                        Client.AllowLoginTwoFactor = true;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Login two-factor has been activated.", true, null, null, Status.Success, StatusMgs.Success);
                    }

                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Error, Login two-factor has not been activate.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAlligbleLoan(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, GetAlligbleLoanDto getAlligbleLoan)
        {
            try
            {
                double StartAmount6 = 0, EndAmount6 = 0; 
                var AcctId = Convert.ToInt32(getAlligbleLoan.AccountId.ToString());
                var Client = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                if (Client != null && Client.AccountType == AccountType.Customer)
                {
                    var ClientAcct90 = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                    if (ClientAcct90 != null)
                    {
                        StartAmount6 = (double)ClientAcct90.MinLoanAmount.Value;
                        EndAmount6 = (double)ClientAcct90.MaxLoanAmount.Value;
                    }

                    var DataReturn1 = new { BankName = "BankName", BankAccountName = "BankAccountName", BankAccountNo = "BankAccountNumber", EndAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100(EndAmount6)).Replace(".00", ""), StartAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100(StartAmount6)).Replace(".00", ""), PFNumber = getAlligbleLoan.PFNumber, AccountId = getAlligbleLoan.AccountId, StartAmounty = NearestRounding.RoundValueToNext100(StartAmount6), EndAmounty = NearestRounding.RoundValueToNext100(EndAmount6) };

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "successfully.", true, DataReturn1, DataReturn1, Status.Success, StatusMgs.Success);

                    var ClientAcct = await lapoLoanDB.Clients.Where(n => n.Pfnumber == getAlligbleLoan.PFNumber).FirstOrDefaultAsync();

                    if (ClientAcct != null)
                    {
                        var ClientAcct1 = await lapoLoanDB.People.Where(n => n.AccountId == Client.Id).FirstOrDefaultAsync();

                        if (ClientAcct1 != null)
                        {
                            if (string.IsNullOrEmpty(ClientAcct1.FirstName) || string.IsNullOrEmpty(ClientAcct1.FirstName))
                            {
                                ClientAcct1.FirstName = ClientAcct.FullName;
                                lapoLoanDB.Entry(ClientAcct1).State = EntityState.Modified;
                                await lapoLoanDB.SaveChangesAsync();
                            }
                        }

                        var ClientAcct2 = await lapoLoanDB.ClientNetPays.Where(n => n.ClientId == ClientAcct.Id).ToListAsync();

                        if (ClientAcct2 == null || ClientAcct2.Count < 3)
                        {
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "IPPIS Number is not approved to take loan for now, your must received 3 month salary before your IPPIS will be active to take any loan.", false, "", null, Status.Ërror, StatusMgs.Error);
                        }
                        else
                        {
                            var StartAmount = 200000.00;
                            var EndAmount = 500000.00;

                            if (ClientAcct2 != null)
                            {
                                var LastThreePayment1 = ClientAcct2.Max(x => x.Npfdate).Value;

                                var LastThreePayment2 = ClientAcct2.Where(x => x.Npfdate != LastThreePayment1).Max(x => x.Npfdate).Value;

                                var LastThreePayment3 = ClientAcct2.Where(x => x.Npfdate != LastThreePayment2 && x.Npfdate != LastThreePayment1).Max(x => x.Npfdate).Value;


                                var EndAmount1 = ClientAcct2.ToList().Where(x => x.Npfdate == LastThreePayment1).FirstOrDefault();
                                if (EndAmount1 != null)
                                {
                                    EndAmount = ((double)EndAmount1.NetPay.Value * 5);
                                }

                                var StartAmount1 = ClientAcct2.ToList().Where(x => x.Npfdate == LastThreePayment3).FirstOrDefault();
                                if (StartAmount1 != null)
                                {
                                    StartAmount = (double)StartAmount1.NetPay.Value;
                                }

                                var ClientAcct8 = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                                if (ClientAcct8 != null && ClientAcct8.UseSalaryAsMaxLoanAmount == false)
                                {
                                    StartAmount = (double)ClientAcct8.MinLoanAmount.Value;
                                }
                            }

                            var DataReturn = new { BankName = ClientAcct2[0].BankName, BankAccountName = ClientAcct2[0].BankAccountName, BankAccountNo = ClientAcct2[0].BankAccountNumber, EndAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100(EndAmount)).Replace(".00", ""), StartAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100(StartAmount)).Replace(".00", ""), PFNumber = getAlligbleLoan.PFNumber, AccountId = getAlligbleLoan.AccountId, StartAmounty = NearestRounding.RoundValueToNext100(StartAmount), EndAmounty = NearestRounding.RoundValueToNext100(EndAmount) };

                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "successfully.", true, DataReturn, DataReturn, Status.Success, StatusMgs.Success);

                        }
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "IPPIS Number you provided is invalid, log out and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Client IPPIS Number is invalid for this login user, log out and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid login account.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SaveNewLoanApp(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, NewLoanAppModelDto loanApp)
        {
            try
            {
                var acc = (long)loanApp.LoanDetailsData.AccountId;
                var AcctId = acc;
                var ClientAcct = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                if (ClientAcct == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo accounts.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (loanApp == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (loanApp.LoanDetailsData == null || string.IsNullOrEmpty(loanApp.LoanDetailsData.AcctNumber) || string.IsNullOrWhiteSpace(loanApp.LoanDetailsData.AcctNumber))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(loanApp.LoanDetailsData.AcctNumber.ToString(), "^[0-9]*$"))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(loanApp.LoanDetailsData.AcctNumber.ToString(), "^[0-9]*$"))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //if (System.Text.RegularExpressions.Regex.IsMatch("^[0-9]", loanApp.LoanDetailsData.AcctNumber.ToString()))
                //{
                //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                if (ClientAcct != null && (ClientAcct.Status == StatusMgs.Active || ClientAcct.Status == StatusMgs.Success))
                {
                    // var Client = await lapoLoanDB.Clients.Where(n => n.Pfnumber == loanApp.LoanDetailsData.PFNumber).FirstOrDefaultAsync();

                    var HubTeam = await lapoLoanDB.HubTeams.Where(n => /*(n.HubMemberLastName == loanApp.LoanDetailsData.OfficerOtherName && n.HubMemberFirstName == loanApp.LoanDetailsData.OfficerFirstName) ||*/ n.RefNo == loanApp.LoanDetailsData.RelationshipOfficerRef).FirstOrDefaultAsync();

                    if (loanApp.LoanDetailsData.OfficerFirstName != "Default" && loanApp.LoanDetailsData.OfficerOtherName != "Default"  && !string.IsNullOrEmpty(loanApp.LoanDetailsData.OfficerFirstName) && !string.IsNullOrWhiteSpace(loanApp.LoanDetailsData.OfficerFirstName) && !string.IsNullOrEmpty(loanApp.LoanDetailsData.OfficerOtherName) && !string.IsNullOrWhiteSpace(loanApp.LoanDetailsData.OfficerOtherName) && !string.IsNullOrEmpty(loanApp.LoanDetailsData.RelationshipOfficerRef) && !string.IsNullOrWhiteSpace(loanApp.LoanDetailsData.RelationshipOfficerRef))
                    {
                        if (HubTeam == null)
                        {
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Hub or Member does not exit", false, "", null, Status.Ërror, StatusMgs.Error);
                        }
                    }

                    //if (Client == null)
                    //{
                    //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "IPPIS NO does not supply us your monthly. Contact the administrator and try again later", false, "", null, Status.Ërror, StatusMgs.Error);
                    //}

                    var LoansClients = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => /*n.Pfnumber == loanApp.LoanDetailsData.PFNumber &&*/ n.AccountId == AcctId && (n.Status == StatusMgs.Pending || n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Disbursed || n.Status == StatusMgs.Approved)).ToListAsync();

                    if (LoansClients != null && LoansClients.Count() > 0)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Oops! You have a running loan. Please complete payment and come back to us.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    if (loanApp.BvnDetail == null || string.IsNullOrEmpty(loanApp.BvnDetail.BVN) || string.IsNullOrWhiteSpace(loanApp.BvnDetail.BVN))
                    {
                        //return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Oops! Invalid BVN.", false, "", null, Status.Ërror, StatusMgs.Error);

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Unable to get your bvn details check the internet network connection and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    //  var RequestBvn = await new BVNClientHelper(this._configuration).SendBvnClientAsync(loanApp.BvnDetail.BVN);

                    //if (Client == null || (Client.Status == StatusMgs.NotActive || Client.Status == StatusMgs.Error))
                    //{
                    //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You don't have privilege or permission to apply for a loan on this platform. Contact the customer support", false, "You don't have privilege or permission to apply for a loan on this platform.", null, Status.Ërror, StatusMgs.Error);
                    //}
      
                        var newScheduled = new ScheduledMethod()
                        {
                            AccountId = AcctId.ToString(),
                            IPPISNumber = loanApp.LoanDetailsData.PFNumber,
                            Amount = loanApp.LoanDetailsData.LoanAmount,
                            Tenure = loanApp.LoanDetailsData.Ternor,

                        };

                        var loanDetails = await new LoanSchedulerHelpers(this.ControllerBase, this._configuration).CalculateScheduledLoanAmount(_environment, newScheduled);

                        if (loanDetails == null || !loanDetails.IsActive)
                        {
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan interest rate has problem try again later", false, "", null, Status.Ërror, StatusMgs.Error);
                        }

                        var GCodeResult = await new RandomGeneratorHelpers(this._configuration).GenarateLoanRequestCodeRandomCode(ControllerBase, AcctId, "", 1, 15);
                        if (!GCodeResult.IsActive)
                        {
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "We are unable to gerenerate requestcode now, try again later.", false, "", null, Status.Ërror, StatusMgs.Error);
                        }

                        var year = (Convert.ToDateTime(loanApp.ClientDetail.DateOfBirth)).Year;

                        var transformDate = DateTime.Now.ToLongDateString().ToString();

                        var CurrentYear = Convert.ToDateTime(transformDate).Year;

                        var AvgAcceptYear = CurrentYear - year;

                        if (AvgAcceptYear < 25 || AvgAcceptYear > 65)
                        {
                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your your age is not accepted to apply for this loan.", false, "", null, Status.Ërror, StatusMgs.Error);
                        }

                        var newLoanApp = new LoanApplicationRequestHeader()
                        {
                            ApprovedDate = DateTime.Now,
                            CreatedDate = DateTime.Now,
                            AccountId = AcctId,
                            RequestDate = DateTime.Now,
                            RequestCode = GCodeResult.DataLoad,
                            Bvn = loanApp.BvnDetail.BVN,
                            Pfnumber = loanApp.ClientDetail.PFNumber,
                            Status = StatusMgs.Pending,
                            LoanInterest = loanDetails.DataLoad.Interest,
                            MinAmount = loanDetails.DataLoad.MinAmount,
                            MaxAmount = loanDetails.DataLoad.MaxAmount,
                            LoanSettingId = Convert.ToInt64(lapoCipher01.DecryptString(loanDetails.DataLoad.LoanSettingsId)),
                            LoanTenureId = Convert.ToInt64(lapoCipher01.DecryptString(loanApp.LoanDetailsData.Ternor)),
                            ApprovedComment = "",
                            IsGroundStandardOrAnonymous = false,
                            Narration = "",
                            BeneficialaryName = loanApp.ClientDetail.fullname,
                            ExportedDate = DateTime.Now,
                            DisbursementAmount = 0,
                        };

                        if (loanApp.ClientDetail != null)
                        {
                            newLoanApp.IndustrySegment = loanApp.ClientDetail.CusBusinessSegmentText;
                            newLoanApp.City = loanApp.ClientDetail.CusCityName;
                            newLoanApp.BusinessType = loanApp.ClientDetail.CusBusinessTypeText;
                            newLoanApp.State = loanApp.ClientDetail.CusStateName;
                        }

                        if (loanApp != null && loanApp.LoanDetailsData != null && !string.IsNullOrEmpty(loanApp.LoanDetailsData.OfficerFirstName) && !string.IsNullOrWhiteSpace(loanApp.LoanDetailsData.OfficerFirstName) && !string.IsNullOrEmpty(loanApp.LoanDetailsData.OfficerOtherName) && !string.IsNullOrWhiteSpace(loanApp.LoanDetailsData.OfficerOtherName) && loanApp.LoanDetailsData.OfficerFirstName.ToString() != "Default".ToString() && loanApp.LoanDetailsData.OfficerOtherName.ToString() != "Default".ToString() && loanApp.LoanDetailsData.RelationshipOfficerRef.ToString() != "Default".ToString())
                        {
                            newLoanApp.IsGroundStandardOrAnonymous = true;

                            var info = loanApp.LoanDetailsData.OfficerOtherName.Split(",");

                            newLoanApp.TeamOfficerOthername = info[0];
                            newLoanApp.TeamOfficerFirstname = info[1];
                            newLoanApp.RelationshipOfficerRef = loanApp.LoanDetailsData.RelationshipOfficerRef; 

                            var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(n => n.Id == HubTeam.GroupId).FirstOrDefaultAsync();

                            if (HubTeamGroup != null)
                            {
                                newLoanApp.TeamGroundName = HubTeamGroup.HubTeamGroupName;
                            }
                            else
                            {
                                newLoanApp.TeamGroundName = "Default";
                            }
                        }
                        else
                        {
                            newLoanApp.IsGroundStandardOrAnonymous = false;
                            var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(n =>  n.IsGroupHead.HasValue &&  n.IsGroupHead.Value == true).FirstOrDefaultAsync();

                            if (HubTeamGroup != null)
                            {
                                newLoanApp.TeamGroundName = HubTeamGroup.HubTeamGroupName;
                                newLoanApp.TeamOfficerOthername = "Default";
                                newLoanApp.TeamOfficerFirstname = "Default";
                            }
                        }

                        newLoanApp.Narration  = loanApp.ClientDetail.Reasonforthisloan;

                        lapoLoanDB.LoanApplicationRequestHeaders.Add(newLoanApp);
                        await lapoLoanDB.SaveChangesAsync();

                        var newKycDetail = new KycDetail()
                        {
                            EmailAddress = loanApp.AcctDetail.Email,
                            CurrentAddress = loanApp.ClientDetail.ResidentialAddress,
                            AltPhoneNumber = loanApp.ClientDetail.AltPhoneNumber,
                            MaritalStatus = loanApp.ClientDetail.MaritalStatus,
                            NokAddress = loanApp.ClientDetail.nokaddress,
                            NokName = loanApp.ClientDetail.nokname,
                            AccountRequestId = AcctId,
                            PhoneNumber = loanApp.ClientDetail.PhoneNumber,
                            NokPhoneNumber = loanApp.ClientDetail.nokphone,
                            RetirementDate = Convert.ToDateTime(loanApp.ClientDetail.DateOfBirth),
                            LoanAppRequestHeaderId = newLoanApp.Id,
                            DateOfBirth = Convert.ToDateTime(loanApp.ClientDetail.DateOfBirth),
                            Age = (long)AvgAcceptYear,
                            FullName = loanApp.ClientDetail.fullname,
                            Pfnumber = loanApp.ClientDetail.PFNumber,
                            NokRelationShip = loanApp.ClientDetail.RelationShip
                        };
                        lapoLoanDB.KycDetails.Add(newKycDetail);
                        await lapoLoanDB.SaveChangesAsync();

                        var loanRequestDetails = new LoanApplicationRequestDetail()
                        {
                            CreatedDate = DateTime.Now,
                            AccountRequestId = AcctId,
                            BankAccountNumber = loanApp.LoanDetailsData.AcctNumber.ToString(),
                            Amount = Convert.ToDecimal(loanApp.LoanDetailsData.LoanAmount),
                            IsActive = true,
                            Status = StatusMgs.Pending,
                            Tenure = loanApp.LoanDetailsData.Ternor,
                            BankAccountName = loanApp.LoanDetailsData.AcctName,
                            LoanAppRequestHeaderId = newLoanApp.Id,
                            BankAccount = loanApp.LoanDetailsData.BankName
                        };
                        lapoLoanDB.LoanApplicationRequestDetails.Add(loanRequestDetails);
                        await lapoLoanDB.SaveChangesAsync();

                        //var HubTeams = await lapoLoanDB.HubTeams.Where(u => u.HubMemberFirstName == loanApp.LoanDetailsData.OfficerFirstName && u.HubMemberLastName == loanApp.LoanDetailsData.OfficerOtherName).FirstOrDefaultAsync();

                        var HubTeams = await lapoLoanDB.HubTeamGroups.Where(u => u.IsGroupHead.Value == true).FirstOrDefaultAsync();

                        var loanSettings = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                        if (loanSettings != null)
                        {
                            if (loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.Value && HubTeams != null && !string.IsNullOrEmpty(HubTeams.EmailAddress) && !string.IsNullOrWhiteSpace(HubTeams.EmailAddress))
                            {
                                await new EmailHelpers(this._configuration).SendNewLoanAppEmail(_environment, loanApp.AcctDetail.FirstName, loanApp.AcctDetail.MiddleName, HubTeams.EmailAddress, new DefalutToken(_configuration).ClientLogin(), loanRequestDetails, loanApp.ClientDetail.PFNumber, "PUBLIC SECTOR TEAM");
                            }

                            string message2 = "PS-Loan : A new loan application has been submitted and waiting for review";

                            if (HubTeams != null && !string.IsNullOrEmpty(HubTeams.PhoneNumber) && !string.IsNullOrWhiteSpace(HubTeams.PhoneNumber) && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.Value)
                            {
                                await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, HubTeams.PhoneNumber, message2, 0);
                            }

                            if (loanSettings.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.Value)
                            {
                                await new EmailHelpers(this._configuration).SendNewLoanAppEmail(_environment, loanApp.AcctDetail.FirstName, loanApp.AcctDetail.MiddleName, ClientAcct.Username, new DefalutToken(_configuration).ClientLogin(), loanRequestDetails, loanApp.ClientDetail.PFNumber, loanApp.AcctDetail.FirstName + "     " + loanApp.AcctDetail.MiddleName);
                            }

                            string message1 = "PS-Loan : Your loan application has been submitted and it under review";

                            if (HubTeams != null && !string.IsNullOrEmpty(loanApp.ClientDetail.PhoneNumber) && !string.IsNullOrWhiteSpace(loanApp.ClientDetail.PhoneNumber) && loanSettings.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.Value)
                            {
                                await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, loanApp.ClientDetail.PhoneNumber, message1, 0);
                            }
                        }
                     
                        var PersonDetail = await lapoLoanDB.People.Where(n => n.AccountId == AcctId).FirstOrDefaultAsync();

                        var CusAcct = await lapoLoanDB.SecurityAccounts.Where(n => n.BvnAccount == loanApp.BvnDetail.BVN).FirstOrDefaultAsync();

                        // var bvnDetails = RequestBvn.DataLoad as BvnRespondsDto;

                        if (PersonDetail != null && CusAcct != null)
                        {
                            #region PersonDetail
                            //var bvnDetails = await lapoLoanDB.People.Where(n => n.AccountId == AcctId).FirstOrDefaultAsync();

                            //    if (bvnDetails != null)
                            //    {
                            //        PersonDetail.FirstName = bvnDetails.FirstName;
                            //        PersonDetail.MiddleName = bvnDetails.MiddleName;
                            //        PersonDetail.LastName = bvnDetails.LastName;
                            //        PersonDetail.PhoneNumber = bvnDetails.PhoneNumber;
                            //        PersonDetail.AltPhoneNumber = loanApp.ClientDetail.AltPhoneNumber;
                            //        PersonDetail.CurrentAddress = loanApp.ClientDetail.ResidentialAddress;
                            //        PersonDetail.Gender = loanApp.ClientDetail.MaritalStatus;
                            //        PersonDetail.Age = newKycDetail.Age.Value.ToString();
                            //        lapoLoanDB.Entry(PersonDetail).State = EntityState.Modified;
                            //        await lapoLoanDB.SaveChangesAsync();
                            //    }
                            //    else
                            //    {
                               
                            //            PersonDetail.FirstName = "";
                            //            PersonDetail.MiddleName = "";
                            //            PersonDetail.LastName = "";
                            //            PersonDetail.PhoneNumber = "";
                            //            PersonDetail.AltPhoneNumber = loanApp.ClientDetail.AltPhoneNumber;
                            //            PersonDetail.CurrentAddress = loanApp.ClientDetail.ResidentialAddress;
                            //            PersonDetail.Gender = loanApp.ClientDetail.MaritalStatus;
                            //            PersonDetail.Age = newKycDetail.Age.Value.ToString();
                            //            lapoLoanDB.Entry(PersonDetail).State = EntityState.Modified;
                            //            await lapoLoanDB.SaveChangesAsync();
                                
                            //    }
                            #endregion
                        }

                        if (loanApp != null && loanApp.ClientDetail != null && newKycDetail != null)
                        {
                            #region OldPerson
                        //var ClientPerson = await lapoLoanDB.People.Where(n => n.AccountId == AcctId).FirstOrDefaultAsync();

                        //var bvnDetails = await lapoLoanDB.People.Where(n => n.AccountId == AcctId).FirstOrDefaultAsync();

                        //if (ClientPerson != null && bvnDetails!=null)
                        //{
                        //    ClientPerson.FirstName = bvnDetails.FirstName;
                        //    ClientPerson.MiddleName = bvnDetails.MiddleName;
                        //    ClientPerson.LastName = bvnDetails.LastName;
                        //    ClientPerson.PhoneNumber = bvnDetails.PhoneNumber;
                        //    // ClientPerson.AltPhoneNumber = loanApp.ClientDetail.AltPhoneNumber;
                        //    // ClientPerson.CurrentAddress = loanApp.ClientDetail.ResidentialAddress;
                        //    //  ClientPerson.Gender = loanApp.ClientDetail.MaritalStatus;
                        //    ClientPerson.Age = newKycDetail.Age.ToString();
                        //    lapoLoanDB.Entry(ClientPerson).State = EntityState.Modified;
                        //    await lapoLoanDB.SaveChangesAsync();
                        //}
                        #endregion
                        }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your loan application has been submitted for review. We will notify you once it has been authorized.", true, new { Id = loanRequestDetails.Id, AcctId = AcctId }, new { Id = loanRequestDetails.Id, AcctId = AcctId }, Status.Success, StatusMgs.Success);
                    
                    //return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your client IPPIS Number is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your account is no longer activate to apply for a loan on Lapo Portal", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your account login is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "An error has occur, Check internet connection and try again.", false, ex.Message ?? ex.InnerException.Message, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetLoanAppList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<LoanApplicationRequestHeader> Loanheaders = null;
                // var LoanAppList = new List<LoanAppListModelDto>();

                var acc = (long)Convert.ToInt64(pagenationFilter.AccountId);

                //if (DefalutToken.IsLocal)
                //{
                //    acc = DefalutToken.DefaultAcct;
                //}

                var ClientAcct = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == acc).FirstOrDefaultAsync();

                if (ClientAcct == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo accounts.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //var sdsdsd = Convert.ToDateTime("7-15-2023 7:45:59 PM");
                //var datab = DateTime.Now.ToString();
                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (ClientAcct != null && (ClientAcct.Status == StatusMgs.Active || ClientAcct.Status == StatusMgs.Success))
                {
                    var Client = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == acc).FirstOrDefaultAsync();

                    if (Client == null)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No loan application was found", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    int no = 0;

                    var newLoanAppList = new List<LoanApp>();

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
                            Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == acc && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any()  || n.Bvn.Equals(pagenationFilter.searchText))).ToListAsync();
                        }
                        else
                        {
                            Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == acc && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any()  || n.Bvn.Equals(pagenationFilter.searchText))).ToListAsync();
                        }
                    }
                    else
                    {
                        if (pagenationFilter.status.Equals("All"))
                        {
                            Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == acc && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)).ToListAsync();
                        }
                        else
                        {
                            Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == acc && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                        }
                    }

                    if (Loanheaders == null)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You don't have any applied loan for now.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    Loanheaders = Loanheaders.OrderByDescending(s => s.CreatedDate).OrderByDescending(s => s.Status == "Pending").ToList();

                    var LoanPerson = await lapoLoanDB.People.Where(n => n.AccountId == acc).FirstOrDefaultAsync();

                    if (Loanheaders == null || Loanheaders.Count < 0)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid login account.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    if (LoanPerson == null)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid login account.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    foreach (var lh in Loanheaders)
                    {
                        var LoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                        LoanDetails = LoanDetails.OrderByDescending(s => s.CreatedDate).ToList();

                        foreach (var ld in LoanDetails)
                        {
                            var lkys = await lapoLoanDB.KycDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                            foreach (var Iky in lkys)
                            {
                                no += 1;

                                var newLoanApp1 = new LoanApp()
                                {
                                    AccountId = lh.AccountId.ToString(),
                                    Amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)ld.Amount)).Replace(".00", ""),
                                    CreatedDate1 = lh.CreatedDate.Value,
                                    Gender = LoanPerson.Gender,
                                    Name = LoanPerson.FirstName + ", " + LoanPerson.MiddleName + ", " + LoanPerson.LastName,
                                    IPPISNumber = /*lh.Pfnumber*/ "",
                                    RequestCode = lh.RequestCode,
                                    Status = lh.Status,
                                    HeaderId = lapoCipher01.EnryptString(lh.Id.ToString()),
                                    createdDate = lh.CreatedDate.Value.ToLongDateString(),
                                    No = no.ToString()
                                };
                                newLoanAppList.Add(newLoanApp1);
                            }
                        }
                    }

                    // newLoanAppList = newLoanAppList.OrderBy(e => e.CreatedDate1).ThenBy(e => e.CreatedDate1).ToList();

                    //  newLoanAppList = newLoanAppList.OrderByDescending(s => s.CreatedDate1).ToList();

                    //no = 0;

                    if (newLoanAppList != null && (newLoanAppList.Count >= 0 || newLoanAppList.Count <= 0))
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

                      
                        var Pagenation = await new PagenationsHelper().GetPagenation<LoanApp>(newLoanAppList, pagenationFilter);

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                    }

                    // await EmailHelpers.SendNewLoanAppEmail(_environment, loanApp.AcctDetail.FirstName, loanApp.AcctDetail.MiddleName, ClientAcct.Username, DefalutToken.ClientLogin);

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Not Loans Applications retrieved", false, "", null, Status.Ërror, StatusMgs.Error);


                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your IPPIS Number is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);

                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your login account is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your account login is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetLoanAppList1(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AcctId)
        {
            try
            {
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                //var acc = (long)Convert.ToInt32(AcctId);

                //if (DefalutToken.IsLocal)
                //{
                //    acc = DefalutToken.DefaultAcct;
                //}

                var ClientAccts = await lapoLoanDB.SecurityAccounts.Where(n => n.AccountType == AccountType.Customer).ToListAsync();

                if (ClientAccts == null || ClientAccts.Count <= 0)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo accounts.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var newLoanAppList = new List<LoanApp>();

                foreach (var itm in ClientAccts)
                {
                    var Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id).ToListAsync();

                    var LoanPerson = await lapoLoanDB.People.Where(n => n.AccountId == itm.Id).FirstOrDefaultAsync();

                    Loanheaders = Loanheaders.OrderByDescending(s => s.CreatedDate).ToList();

                    foreach (var lh in Loanheaders)
                    {
                        var LoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                        LoanDetails = LoanDetails.OrderByDescending(s => s.CreatedDate).ToList();

                        foreach (var ld in LoanDetails)
                        {
                            var lkys = await lapoLoanDB.KycDetails.Where(n => n.LoanAppRequestHeaderId == ld.Id).ToListAsync();

                            foreach (var Iky in lkys)
                            {
                                try
                                {
                                    no += 1;
                                    var newLoanApp1 = new LoanApp()
                                    {
                                        AccountId = lh.AccountId.ToString(),
                                        Amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)ld.Amount)).Replace(".00", ""),
                                        CreatedDate1 = lh.CreatedDate.Value,
                                        Gender = LoanPerson.Gender,
                                        Name = LoanPerson.FirstName + ", " + LoanPerson.MiddleName + ", " + LoanPerson.LastName,
                                        IPPISNumber = /*lh.Pfnumber*/ "",
                                        RequestCode = lh.RequestCode,
                                        Status = lh.Status,
                                        HeaderId = lapoCipher01.EnryptString(lh.Id.ToString()),
                                        createdDate = lh.CreatedDate.Value.ToLongDateString(),
                                        No = no.ToString(),

                                    };

                                    if (lh.Status == StatusMgs.Success || lh.Status == StatusMgs.Active)
                                    {
                                        newLoanApp1.IsActive = true;
                                    }
                                    else
                                    {
                                        newLoanApp1.IsActive = false;
                                    }

                                    newLoanAppList.Add(newLoanApp1);
                                }
                                catch (Exception er)
                                {

                                }
                            }
                        }
                    }
                }

                //newLoanAppList = newLoanAppList.OrderByDescending(s => s.CreatedDate1).ToList();
                //no = 0;
                // newLoanAppList = newLoanAppList.OrderBy(e => e.CreatedDate1).ThenBy(e => e.CreatedDate1).ToList();

                if (newLoanAppList != null && newLoanAppList.Count > 0)
                {
                    //var newLoanAppList1 = new List<LoanApp>();

                    //foreach (var Iky in newLoanAppList)
                    //{
                    //    try
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
                    //            No = no.ToString(),
                    //             IsActive = Iky.IsActive

                    //        };

                    //        newLoanAppList1.Add(newLoanApp1);
                    //    }
                    //    catch (Exception er)
                    //    {

                    //    }
                    //}


                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, newLoanAppList, newLoanAppList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetDashboardLoanAppList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<LoanApplicationRequestHeader> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                //var acc = (long)Convert.ToInt32(AcctId);

                //if (  new DefalutToken(_configuration).IsLocal)
                //{
                //    acc =   new DefalutToken(_configuration).DefaultAcct;
                //}

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var ClientAccts = await lapoLoanDB.SecurityAccounts.Where(n => n.AccountType == AccountType.Customer).ToListAsync();

                if (ClientAccts == null || ClientAccts.Count <= 0)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var newLoanAppList = new List<LoanApp>();

                var CtAccts = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == pagenationFilter.AccountId && n.AccountVerify == true).FirstOrDefaultAsync();

                if (CtAccts == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var PAccts = await lapoLoanDB.People.Where(n => n.AccountId == CtAccts.Id).FirstOrDefaultAsync();

                if (PAccts == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HAccts = await lapoLoanDB.HubTeams.Where(n => n.TeamAccountId == CtAccts.Id).FirstOrDefaultAsync();

                if (HAccts == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var hubTeam = await lapoLoanDB.HubTeams.Where(n => n.TeamAccountId == pagenationFilter.AccountId).FirstOrDefaultAsync();

                if (hubTeam == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var group = await lapoLoanDB.HubTeamGroups.Where(n => n.Id == hubTeam.GroupId).FirstOrDefaultAsync();

                if (group == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                foreach (var itm in ClientAccts)
                {
                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.Status == pagenationFilter.status)).ToListAsync();

                    if (Loanheaders == null && Loanheaders.Count <= 0)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No loan application was found", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    if (pagenationFilter.PermissionPage.IsHEADOFOPERATIONS || pagenationFilter.PermissionPage.IsGROUPHEAD)
                    {
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
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText))  && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                        }

                        if(Loanheaders.Count <= 0)
                        {
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
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                                else
                                {
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                            }
                            else
                            {
                                if (pagenationFilter.status.Equals("All"))
                                {
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                                else
                                {
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                            }
                        }
                    }
                    else if (pagenationFilter.PermissionPage.IsDISBURSEMENTOFFICER || pagenationFilter.PermissionPage.IsDISBURSEMENTOFFICER)
                    {
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
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)  && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                        }
                    }
                    else
                    {
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
                                   Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                        }
                    }

                    if (Loanheaders == null)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You don't have any applied loan for now.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    Loanheaders = Loanheaders.OrderByDescending(s => s.Status == StatusMgs.Pending).ToList();

                    Loanheaders = Loanheaders.OrderByDescending(s => s.CreatedDate).ToList();

                    var LoanPerson = await lapoLoanDB.People.Where(n => n.AccountId == itm.Id).FirstOrDefaultAsync();

                    foreach (var lh in Loanheaders)
                    {
                        if(pagenationFilter.PermissionPage.IsDeveloperTeam == true  && (lh.IsGroundStandardOrAnonymous == true || lh.IsGroundStandardOrAnonymous == false) || (pagenationFilter.PermissionPage.IsGROUPHEAD == true && (lh.IsGroundStandardOrAnonymous == true || lh.IsGroundStandardOrAnonymous == false)))
                        {
                            //Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id /*&& (n.Status == pagenationFilter.status)*/ /*&& PAccts.Staff == n.RelationshipOfficerRef*/).ToListAsync();

                            var LoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                            LoanDetails = LoanDetails.OrderByDescending(s => s.CreatedDate).ToList();

                            foreach (var ld in LoanDetails)
                            {
                                var lkys = await lapoLoanDB.KycDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                                foreach (var Iky in lkys)
                                {
                                    try
                                    {
                                        no += 1;
                                        var newLoanApp1 = new LoanApp()
                                        {
                                            AccountId = lh.AccountId.ToString(),
                                            Amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)ld.Amount)).Replace(".00", ""),
                                            CreatedDate1 = lh.CreatedDate.Value,
                                            Gender = LoanPerson.Gender,
                                            Name = LoanPerson.FirstName + ", " + LoanPerson.MiddleName + ", " + LoanPerson.LastName,
                                            IPPISNumber = lh.Pfnumber,
                                            RequestCode = lh.RequestCode,
                                            Status = lh.Status,
                                            HeaderId = lapoCipher01.EnryptString(lh.Id.ToString()),
                                            createdDate = lh.CreatedDate.Value.ToLongDateString(),
                                            No = no.ToString(),
                                            GroupName = "",
                                            TypeOfLoan = ""

                                        };

                                        if (lh.IsGroundStandardOrAnonymous.HasValue && lh.IsGroundStandardOrAnonymous.Value)
                                        {
                                            newLoanApp1.TypeOfLoan = "Standard Loan";
                                            newLoanApp1.GroupName = lh.TeamGroundName;
                                        }
                                        else
                                        {
                                            newLoanApp1.GroupName = lh.TeamGroundName;
                                            newLoanApp1.TypeOfLoan = "Anonymous Loan";
                                        }


                                        if (lh.Status == StatusMgs.Success || lh.Status == StatusMgs.Active)
                                        {
                                            newLoanApp1.IsActive = true;
                                        }
                                        else
                                        {
                                            newLoanApp1.IsActive = false;
                                        }

                                        if (pagenationFilter.MarkAllData)
                                        {
                                            newLoanApp1.IsSelected = true;
                                        }

                                        newLoanAppList.Add(newLoanApp1);
                                    }
                                    catch (Exception er)
                                    {

                                    }
                                }
                            }
                        }
                        else
                        {
                            var hubTeamRef = await lapoLoanDB.HubTeams.Where(n => n.RefNo == lh.RelationshipOfficerRef).FirstOrDefaultAsync();

                            if (hubTeamRef != null && group.Id == hubTeamRef.GroupId.Value)
                            {
                                //Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id /*&& (n.Status == pagenationFilter.status)*/ /*&& PAccts.Staff == n.RelationshipOfficerRef*/).ToListAsync();

                                var LoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                                LoanDetails = LoanDetails.OrderByDescending(s => s.CreatedDate).ToList();

                                foreach (var ld in LoanDetails)
                                {
                                    var lkys = await lapoLoanDB.KycDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                                    foreach (var Iky in lkys)
                                    {
                                        try
                                        {
                                            no += 1;
                                            var newLoanApp1 = new LoanApp()
                                            {
                                                AccountId = lh.AccountId.ToString(),
                                                Amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)ld.Amount)).Replace(".00", ""),
                                                CreatedDate1 = lh.CreatedDate.Value,
                                                Gender = LoanPerson.Gender,
                                                Name = LoanPerson.FirstName + ", " + LoanPerson.MiddleName + ", " + LoanPerson.LastName,
                                                IPPISNumber = lh.Pfnumber,
                                                RequestCode = lh.RequestCode,
                                                Status = lh.Status,
                                                HeaderId = lapoCipher01.EnryptString(lh.Id.ToString()),
                                                createdDate = lh.CreatedDate.Value.ToLongDateString(),
                                                No = no.ToString(),
                                                GroupName = "",
                                                TypeOfLoan = ""

                                            };

                                            if (lh.IsGroundStandardOrAnonymous.HasValue && lh.IsGroundStandardOrAnonymous.Value)
                                            {
                                                newLoanApp1.TypeOfLoan = "Standard Loan";
                                                newLoanApp1.GroupName = lh.TeamGroundName;
                                            }
                                            else
                                            {
                                                newLoanApp1.GroupName = lh.TeamGroundName;
                                                newLoanApp1.TypeOfLoan = "Anonymous Loan";
                                            }

                                            if (lh.Status == StatusMgs.Success || lh.Status == StatusMgs.Active)
                                            {
                                                newLoanApp1.IsActive = true;
                                            }
                                            else
                                            {
                                                newLoanApp1.IsActive = false;
                                            }

                                            if (pagenationFilter.MarkAllData)
                                            {
                                                newLoanApp1.IsSelected = true;
                                            }

                                            newLoanAppList.Add(newLoanApp1);
                                        }
                                        catch (Exception er)
                                        {

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // newLoanAppList = newLoanAppList.OrderBy(e => e.CreatedDate1).ThenBy(e =>   e.CreatedDate1).ToList();

                // no = 0;

                // var ascendingOrder = newLoanAppList.OrderBy(i => i).ToList();
                // var descendingOrder = newLoanAppList.OrderByDescending(i => i).ToList();

                // var bellow = newLoanAppList.Where(i => i <= 20).OrderBy(i);
                // var above = newLoanAppList.Where(i => i > 20).OrderByDescending(i);

                // newLoanAppList = newLoanAppList.OrderByDescending(s => s.Status == StatusMgs.Pending).ToList();

                newLoanAppList = newLoanAppList.OrderByDescending(s => Convert.ToDateTime(s.createdDate)).ToList();

                newLoanAppList = newLoanAppList.OrderByDescending(s => s.CreatedDate1).ToList();

                if (newLoanAppList != null && (newLoanAppList.Count >= 0 || newLoanAppList.Count <= 0))
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


                    var Pagenation = await new PagenationsHelper().GetPagenation<LoanApp>(newLoanAppList, pagenationFilter);

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAllDisbursedLoanAppList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<LoanApplicationRequestHeader> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                //var acc = (long)Convert.ToInt32(AcctId);

                //if (  new DefalutToken(_configuration).IsLocal)
                //{
                //    acc =   new DefalutToken(_configuration).DefaultAcct;
                //}

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var ClientAccts = await lapoLoanDB.SecurityAccounts.Where(n => n.AccountType == AccountType.Customer).ToListAsync();

                if (ClientAccts == null || ClientAccts.Count <= 0)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo accounts.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var newLoanAppList = new List<LoanApp>();

                foreach (var itm in ClientAccts)
                {
                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.Status == pagenationFilter.status)).ToListAsync();

                    if (Loanheaders == null && Loanheaders.Count <= 0)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No loan application was found", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    if (pagenationFilter.PermissionPage.IsHEADOFOPERATIONS || pagenationFilter.PermissionPage.IsGROUPHEAD)
                    {
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
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && n.IsGroundStandardOrAnonymous == false).ToListAsync();
                            }
                        }

                        if (Loanheaders.Count <= 0)
                        {
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
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                                else
                                {
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                            }
                            else
                            {
                                if (pagenationFilter.status.Equals("All"))
                                {
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                                else
                                {
                                    Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                                }
                            }
                        }
                    }
                    else if (pagenationFilter.PermissionPage.IsDISBURSEMENTOFFICER || pagenationFilter.PermissionPage.IsDISBURSEMENTOFFICER)
                    {
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
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Approved || n.Status == StatusMgs.Active)).ToListAsync();
                            }
                        }
                    }
                    else
                    {
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
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && (lapoLoanDB.LoanApplicationRequestDetails.Where(d => d.LoanAppRequestHeaderId == n.Id && (d.Amount == (decimal)(Amount))).Any() /*|| n.Pfnumber.Equals(pagenationFilter.searchText)*/ || n.Bvn.Equals(pagenationFilter.searchText)) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == itm.Id && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status && n.IsGroundStandardOrAnonymous == true).ToListAsync();
                            }
                        }
                    }

                    if (Loanheaders == null)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You don't have any applied loan for now.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    Loanheaders = Loanheaders.OrderByDescending(s => s.CreatedDate).OrderByDescending(s => s.Status == "Pending").ToList();

                    var LoanPerson = await lapoLoanDB.People.Where(n => n.AccountId == itm.Id).FirstOrDefaultAsync();

                    Loanheaders = Loanheaders.OrderByDescending(s => s.CreatedDate).ToList();

                    foreach (var lh in Loanheaders)
                    {
                        var LoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                        LoanDetails = LoanDetails.OrderByDescending(s => s.CreatedDate).ToList();

                        foreach (var ld in LoanDetails)
                        {
                            var lkys = await lapoLoanDB.KycDetails.Where(n => n.LoanAppRequestHeaderId == lh.Id).ToListAsync();

                            foreach (var Iky in lkys)
                            {
                                try
                                {
                                    no += 1;
                                    var newLoanApp1 = new LoanApp()
                                    {
                                        AccountId = lh.AccountId.ToString(),
                                        Amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)ld.Amount)).Replace(".00", ""),
                                        CreatedDate1 = lh.CreatedDate.Value,
                                        Gender = LoanPerson.Gender,
                                        Name = LoanPerson.FirstName + ", " + LoanPerson.MiddleName + ", " + LoanPerson.LastName,
                                        IPPISNumber = /*lh.Pfnumber*/ "",
                                        RequestCode = lh.RequestCode,
                                        Status = lh.Status,
                                        HeaderId = lapoCipher01.EnryptString(lh.Id.ToString()),
                                        createdDate = lh.CreatedDate.Value.ToLongDateString(),
                                        No = no.ToString(),
                                        GroupName = "",
                                        TypeOfLoan = ""
                                    };

                                    if (lh.IsGroundStandardOrAnonymous.HasValue && lh.IsGroundStandardOrAnonymous.Value)
                                    {
                                        newLoanApp1.TypeOfLoan = "Standard Loan";
                                        newLoanApp1.GroupName = lh.TeamGroundName;
                                    }
                                    else
                                    {
                                        newLoanApp1.GroupName = lh.TeamGroundName;
                                        newLoanApp1.TypeOfLoan = "Anonymous Loan";
                                    }


                                    if (lh.Status == StatusMgs.Success || lh.Status == StatusMgs.Active)
                                    {
                                        newLoanApp1.IsActive = true;
                                    }
                                    else
                                    {
                                        newLoanApp1.IsActive = false;
                                    }

                                    newLoanAppList.Add(newLoanApp1);
                                }
                                catch (Exception er)
                                {

                                }
                            }
                        }
                    }
                }

                //newLoanAppList = newLoanAppList.OrderBy(e => e.CreatedDate1).ThenBy(e => e.CreatedDate1).ToList();

                //no = 0;
                //newLoanAppList = newLoanAppList.OrderByDescending(s => s.CreatedDate1).ToList();
                //var ascendingOrder = newLoanAppList.OrderBy(i => i).ToList();
                //var descendingOrder = newLoanAppList.OrderByDescending(i => i).ToList();

                //var bellow = newLoanAppList.Where(i => i <= 20).OrderBy(i);
                //var above = newLoanAppList.Where(i => i > 20).OrderByDescending(i);

                newLoanAppList = newLoanAppList.OrderByDescending(s => Convert.ToDateTime(s.createdDate)).ToList();

                newLoanAppList = newLoanAppList.OrderByDescending(s => Convert.ToDateTime(s.createdDate)).ToList();

                newLoanAppList = newLoanAppList.OrderByDescending(s => s.CreatedDate1).ToList();

                if (newLoanAppList != null && (newLoanAppList.Count >= 0 || newLoanAppList.Count <= 0))
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


                    var Pagenation = await new PagenationsHelper().GetPagenation<LoanApp>(newLoanAppList, pagenationFilter);

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAllOngoingAndCompletedLoanAppList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<RepaymentLoan> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                //var acc = (long)Convert.ToInt32(AcctId);

                //if (  new DefalutToken(_configuration).IsLocal)
                //{
                //    acc =   new DefalutToken(_configuration).DefaultAcct;
                //}

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                var AppId = Convert.ToInt64(lapoCipher01.DecryptString(pagenationFilter.AppId));

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var newLoanAppList = new List<RepaymentLoanModel>();

                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.Status == pagenationFilter.status)).ToListAsync();

                    if (pagenationFilter.PermissionPage.IsHEADOFOPERATIONS || pagenationFilter.PermissionPage.IsGROUPHEAD)
                    {
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
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                            }
                        }

                        if (Loanheaders.Count <= 0)
                        {
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
                                    Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                                }
                                else
                                {
                                    Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                                }
                            }
                            else
                            {
                                if (pagenationFilter.status.Equals("All"))
                                {
                                    Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                                }
                                else
                                {
                                    Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                                }
                            }
                        }
                    }
                    else if (pagenationFilter.PermissionPage.IsDISBURSEMENTOFFICER || pagenationFilter.PermissionPage.IsDISBURSEMENTOFFICER)
                    {
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
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                            }
                        }
                        else
                        {
                            // && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                            }
                        }
                    }
                    else
                    {
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
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                            }
                        }
                        else
                        {
                            if (pagenationFilter.status.Equals("All"))
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.Status == StatusMgs.Ongoing || n.Status == StatusMgs.Completed)).ToListAsync();
                            }
                            else
                            {
                                Loanheaders = await lapoLoanDB.RepaymentLoans.Where(n => n.LoanHeaderId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status == pagenationFilter.status).ToListAsync();
                            }
                        }
                    }

                    if (Loanheaders == null)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No repayment loan was found.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    Loanheaders = Loanheaders.OrderByDescending(s => s.CreatedDate).OrderByDescending(s => s.Status == StatusMgs.Ongoing).ToList();

                    // var LoanPerson = await lapoLoanDB.People.Where(n => n.AccountId == itm.Id).FirstOrDefaultAsync();

                    Loanheaders = Loanheaders.OrderByDescending(s => s.CreatedDate).ToList();

                    foreach (var lh in Loanheaders)
                    {
                        var LoanApplicationReq = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Id == lh.LoanHeaderId).ToListAsync();

                        try
                        {
                            no += 1;

                            var newLoanApp1 = new RepaymentLoanModel()
                            {
                                Repayment_Amount = "₦" + Convert.ToString((int)Decimal.Truncate (lh.RepaymentAmount.Value)), //string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)lh.RepaymentAmount.Value)).Replace(".00", ""),
                                Repayment_Date = lh.RepaymentDate.Value.ToLongDateString(),
                                Status = lh.Status,
                                CreatedDate = lh.CreatedDate.Value.ToLongDateString(),
                                Customer_Name = lh.CustomerName,
                                Loan_Repayment_For = lh.LoanRepaymentFor,
                                Loan_Request_Code = lh.LoanRequestCode,
                                Uploaded_By_Member_Staff_ID = lh.UploadedByMemberStaffId,
                                Service_Account = lh.ServiceAccount,
                                Repayment_Status = lh.RepaymentStatus,
                                Repayment_Balance = "₦" + Convert.ToString((int)Decimal.Truncate(lh.RepaymentBalance.Value)),
                                // string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)lh.RepaymentBalance.Value)).Replace(".00", ""),
                                No = no.ToString(),
                                CreatedAccount = "",
                            };
                       
                            newLoanAppList.Add(newLoanApp1);
                        }
                        catch (Exception er)
                        {

                        }
                    }

                //newLoanAppList = newLoanAppList.OrderBy(e => e.CreatedDate1).ThenBy(e => e.CreatedDate1).ToList();

                //no = 0;

                //  newLoanAppList = newLoanAppList.OrderByDescending(s => s.CreatedDate).OrderByDescending(s => s.Status == StatusMgs.Ongoing).ToList();

                // newLoanAppList = newLoanAppList.OrderByDescending(s => s.CreatedDate).ToList();

                //var ascendingOrder = newLoanAppList.OrderBy(i => i).ToList();
                //var descendingOrder = newLoanAppList.OrderByDescending(i => i).ToList();

                //var bellow = newLoanAppList.Where(i => i <= 20).OrderBy(i);
                //var above = newLoanAppList.Where(i => i > 20).OrderByDescending(i);

                newLoanAppList = newLoanAppList.OrderByDescending(s => Convert.ToDateTime(s.CreatedDate)).ToList();

                newLoanAppList = newLoanAppList.OrderByDescending(s => Convert.ToDateTime(s.CreatedDate)).ToList();

                if (newLoanAppList != null && (newLoanAppList.Count >= 0 || newLoanAppList.Count <= 0))
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

                    var Pagenation = await new PagenationsHelper().GetPagenation<RepaymentLoanModel>(newLoanAppList, pagenationFilter);

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetLoanAppDetails(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AppHeaderId, string AccounttId)
        {
            try
            {
                var LoanAppList = new List<LoanAppListModelDto>();

                var AcctId = (long)Convert.ToInt32(lapoCipher01.DecryptString(AppHeaderId));

                var AccountId = (long)Convert.ToInt32(AccounttId);

                if (DefalutToken.IsLocal)
                {
                    //  AccountId = DefalutToken.DefaultAcct;
                }

                var ClientAcct = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == AccountId).FirstOrDefaultAsync();

                if (ClientAcct == null || ClientAcct.Status == StatusMgs.NotActive)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This account is not active on lapo accounts or have not been activated from your email.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (ClientAcct != null && ClientAcct.Status == StatusMgs.Active)
                {
                    var Client = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                    if (Client == null)
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "No loan application was found", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    //var RequestBvn = await new BVNClientHelper(_configuration).SendBvnClientAsync(Client.Bvn);

                    #region

                    //if (RequestBvn == null || RequestBvn.IsActive == false)
                    //{
                    //    var personacct = await lapoLoanDB.People.Where(n => n.AccountId == ClientAcct.Id).FirstOrDefaultAsync();

                    //    if (personacct != null)
                    //    {
                    //        var bvnDetails = new BvnRespondsDto()
                    //        {
                    //            FirstName = personacct.FirstName,
                    //            DateOfBirth = DateTime.Now.ToString(),
                    //            MiddleName = personacct.MiddleName,
                    //            LastName = personacct.LastName,
                    //            PhoneNumber1 = personacct.PhoneNumber,
                    //            BVN = Client.Bvn,
                    //            RegistrationDate = DateTime.Now.ToString(),
                    //            EnrollmentBank = "",
                    //            EnrollmentBranch = "",
                    //            ResponseCode = "",
                    //            WatchListed = ""
                    //        };

                    //        RequestBvn.DataLoad = bvnDetails;
                    //        RequestBvn.Data = bvnDetails;
                    //    }

                    //    // return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Bvn you used to apply for loan is invalid. or Bvn server has error occur", false, "", null, Status.Ërror, StatusMgs.Error);
                    //}

                    #endregion

                    if (Client != null)
                    {
                        LoanAppListModelDto LoanApp = null;

                        var Loanheader = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                        if (Loanheader != null)
                        {
                            var LoanPerson = await lapoLoanDB.People.Where(n => n.AccountId == Loanheader.AccountId).FirstOrDefaultAsync();

                            if (LoanPerson != null)
                            {
                                var LoanDetail = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == Loanheader.Id).FirstOrDefaultAsync();

                                if (LoanDetail != null)
                                {
                                    var SecurityAcct = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == Loanheader.AccountId).FirstOrDefaultAsync();

                                    if (SecurityAcct != null)
                                    {
                                        var KycDetail = await lapoLoanDB.KycDetails.Where(n => n.LoanAppRequestHeaderId == Loanheader.Id).FirstOrDefaultAsync();

                                        //var bvnDetails = RequestBvn.DataLoad as BvnRespondsDto;

                                        if (KycDetail != null/* && bvnDetails != null*/)
                                        {
                                            LoanApp = new LoanAppListModelDto();

                                            if (Loanheader.Status == StatusMgs.Approved)
                                            {
                                                LoanApp.loanAppReviewStatus = new LoanAppReviewStatusModelDto()
                                                {
                                                    ApprovedDate = Loanheader.ApprovedDate.Value.ToLongDateString(),
                                                    Comment = Loanheader.ApprovedComment,
                                                    Status = Loanheader.Status,
                                                    IsApproved = true
                                                };

                                                if (Loanheader.ApprovedByAccount != null)
                                                {
                                                    LoanApp.loanAppReviewStatus.ApprovedBy = Loanheader.ApprovedByAccount.Username;

                                                }
                                                else
                                                {
                                                    LoanApp.loanAppReviewStatus.ApprovedBy = "";
                                                }
                                            }
                                            else
                                            {
                                                LoanApp.loanAppReviewStatus = new LoanAppReviewStatusModelDto()
                                                {
                                                    ApprovedBy = "",
                                                    ApprovedDate = Loanheader.ApprovedDate.Value.ToLongDateString(),
                                                    Comment = Loanheader.ApprovedComment,
                                                    Status = Loanheader.Status,
                                                    IsApproved = false
                                                };
                                            }

                                            LoanApp.LoanDetailsData = new AllLoanAppDto()
                                            {
                                                AccountId = (int)Loanheader.AccountId,
                                                AcctName = LoanDetail.BankAccountName,
                                                BankName = LoanDetail.BankAccount,
                                                AcctNumber = LoanDetail.BankAccountNumber,
                                                LoanAmount = (double)LoanDetail.Amount,
                                                LoanAmountString = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)LoanDetail.Amount)).Replace(".00", ""),
                                                PFNumber = Loanheader.Pfnumber,
                                                Ternor = LoanDetail.Tenure,

                                                PassportPhotograph = DefalutToken.MakeImageSrcData(LoanDetail.PassportUrl),
                                                PaySliptfiles = DefalutToken.MakeImageSrcData(LoanDetail.PaySlipUrl),
                                                StaffIdCard = DefalutToken.MakeImageSrcData(LoanDetail.IdcardUrl),

                                                AppId = lapoCipher01.EnryptString(Loanheader.Id.ToString()),

                                                PassportPhotographUrl = LoanDetail.PassportUrl,
                                                PaySliptfilesUrl = LoanDetail.PaySlipUrl,
                                                StaffIdCardUrl = LoanDetail.IdcardUrl,

                                                ReOfficerFirstName = Loanheader.TeamOfficerFirstname,
                                                ReOfficerLastName = Loanheader.TeamOfficerOthername,
                                            };


                                            try
                                            {
                                                if ((LoanDetail.IdcardUrl.ToLower().EndsWith(".pdf".ToLower()) || LoanDetail.IdcardUrl.ToLower().Contains(".pdf".ToLower())) || (LoanDetail.IdcardUrl.ToLower().EndsWith(".docx".ToLower()) || LoanDetail.IdcardUrl.ToLower().Contains(".docx".ToLower())))
                                                {
                                                    LoanApp.LoanDetailsData.IsStaffIdCard = true;
                                                }
                                                else
                                                {
                                                    LoanApp.LoanDetailsData.IsStaffIdCard = false;
                                                }

                                                if ((LoanDetail.PaySlipUrl.ToLower().EndsWith(".pdf".ToLower()) || LoanDetail.PaySlipUrl.ToLower().Contains(".pdf".ToLower())) || (LoanDetail.PaySlipUrl.ToLower().EndsWith(".docx".ToLower()) || LoanDetail.PaySlipUrl.ToLower().Contains(".docx".ToLower())))
                                                {
                                                    LoanApp.LoanDetailsData.IsPaySliptfiles = true;
                                                }
                                                else
                                                {
                                                    LoanApp.LoanDetailsData.IsPaySliptfiles = false;
                                                }

                                                if ((LoanDetail.PassportUrl.ToLower().EndsWith(".pdf".ToLower()) ||LoanDetail.PassportUrl.ToLower().Contains(".pdf".ToLower())) || (LoanDetail.PassportUrl.ToLower().EndsWith(".docx".ToLower()) || LoanDetail.PassportUrl.ToLower().Contains(".docx".ToLower())))
                                                {
                                                    LoanApp.LoanDetailsData.IsPassportPhotograph = true;
                                                }
                                                else
                                                {
                                                    LoanApp.LoanDetailsData.IsPassportPhotograph = false;
                                                }
                                            }
                                            catch(Exception exp1)
                                            {

                                            }
                                         
                                            try
                                            {
                                                var SecurityAcc = await lapoLoanDB.People.Where(n => n.AccountId == Loanheader.ExportedById.Value).FirstOrDefaultAsync();
                                                if (LoanApp.LoanDetailsData != null && SecurityAcc != null)
                                                {
                                                    LoanApp.LoanDetailsData.EmploymentDate = SecurityAcc.EmploymentDate != null && SecurityAcc.EmploymentDate.HasValue && SecurityAcc.EmploymentDate.Value != null ? SecurityAcc.EmploymentDate.Value.ToLongDateString() : "";
                                                    LoanApp.LoanDetailsData.RetirementDate = SecurityAcc.RetirementDate != null && SecurityAcc.RetirementDate.HasValue && SecurityAcc.RetirementDate.Value != null ? SecurityAcc.RetirementDate.Value.ToLongDateString() : "";

                                                    if (SecurityAcc != null)
                                                    {
                                                        LoanApp.LoanDetailsData.ExportedBy = SecurityAcc.FirstName + "  " + SecurityAcc.LastName;
                                                    }
                                                    else
                                                    {
                                                        LoanApp.LoanDetailsData.ExportedBy = "";
                                                    }
                                                }
                                            }
                                            catch(Exception exxx)
                                            {
                                                LoanApp.LoanDetailsData.ExportedBy = "";
                                                LoanApp.LoanDetailsData.EmploymentDate ="";
                                                LoanApp.LoanDetailsData.RetirementDate = "";
                                            }

                                            LoanApp.LoanDetailsData.BeneficialaryName = Loanheader.BeneficialaryName==null ? "" : Loanheader.BeneficialaryName;
                                            LoanApp.LoanDetailsData.Narration = Loanheader.Narration;

                                            LoanApp.LoanDetailsData.ExportedDate = Loanheader.ExportedDate == null ? "" : Loanheader.ExportedDate.Value.ToLongDateString();

                                            LoanApp.LoanDetailsData.TeamOfficerOthername = Loanheader.TeamOfficerOthername==null ? "" : Loanheader.TeamOfficerOthername;

                                            LoanApp.LoanDetailsData.TeamGroundName = Loanheader.TeamGroundName==null? "" : Loanheader.TeamGroundName;

                                            LoanApp.LoanDetailsData.DisbursementAmount = Loanheader.DisbursementAmount == null ? "" : string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)Loanheader.DisbursementAmount.Value)).Replace(".00", "");

                                            LoanApp.LoanDetailsData.DisbursementDate = Loanheader.DisbursementDate == null ? "" : Loanheader.DisbursementDate.Value.ToLongDateString();

                                            LoanApp.LoanDetailsData.TeamOfficerFirstname = Loanheader.TeamOfficerFirstname;

                                            LoanApp.LoanDetailsData.IsAnonymousOrStandard = Loanheader.IsGroundStandardOrAnonymous.Value;

                                            var HubTeam = await lapoLoanDB.HubTeams.Where(n => /*(n.HubMemberFirstName == Loanheader.TeamOfficerFirstname && n.HubMemberLastName == Loanheader.TeamOfficerOthername)  ||*/ n.RefNo == Loanheader.RelationshipOfficerRef).FirstOrDefaultAsync();

                                            if (HubTeam != null)
                                            {
                                               var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(n => n.Id == HubTeam.GroupId).FirstOrDefaultAsync();
                                                if (HubTeamGroup != null)
                                                {
                                                    LoanApp.LoanDetailsData.ReconciliationOfficerHub = HubTeamGroup.HubTeamGroupName;
                                                }
                                                else
                                                {
                                                    LoanApp.LoanDetailsData.ReconciliationOfficerHub = Loanheader.TeamGroundName;
                                                }
                                            }
                                            else
                                            {
                                                LoanApp.LoanDetailsData.ReconciliationOfficerHub = Loanheader.TeamGroundName;
                                            }

                                            try
                                            {
                                                var SecurityAcc1 = await lapoLoanDB.People.Where(n => n.AccountId == Loanheader.AccountId).FirstOrDefaultAsync();

                                                if (SecurityAcc1 != null)
                                                {
                                                    LoanApp.LoanDetailsData.EmployeementDate = SecurityAcc1.EmploymentDate.Value.ToLongDateString();
                                                    LoanApp.LoanDetailsData.RetirementDate = SecurityAcc1.RetirementDate.Value.ToLongDateString();
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                            if (Loanheader != null)
                                            {
                                                LoanApp.LoanDetailsData.City = Loanheader.City;
                                                LoanApp.LoanDetailsData.StateOfOrigin = Loanheader.State;
                                                LoanApp.LoanDetailsData.BusinessSegment = Loanheader.IndustrySegment;
                                                LoanApp.LoanDetailsData.BusinessType = Loanheader.BusinessType;
                                            }

                                            var People1 = await lapoLoanDB.People.Where(n => n.AccountId == Loanheader.AccountId).FirstOrDefaultAsync();

                                            LoanApp.AcctDetail = new AllLoanAccountDetailsDto()
                                            {
                                                AccountId = (int)Loanheader.AccountId,
                                                AccountType = SecurityAcct.AccountType,
                                                Address = KycDetail.CurrentAddress,
                                                Age = KycDetail.Age.ToString(),
                                                CurrentAddress = KycDetail.CurrentAddress,
                                                AltPhone = KycDetail.AltPhoneNumber,
                                                Email = KycDetail.EmailAddress,
                                                FirstName = People1.FirstName,
                                                LastName = People1.LastName,
                                                MiddleName = People1.MiddleName,
                                                Gender = LoanPerson.Gender,
                                                Phone = People1.PhoneNumber,
                                            };

                                            LoanApp.ClientDetail = new AllLoanPersonalDetailsDto()
                                            {
                                                MaritalStatus = KycDetail.MaritalStatus,
                                                fullname = KycDetail.FullName,
                                                DateOfBirth = KycDetail.DateOfBirth.Value.ToLongDateString(),
                                                nokname = KycDetail.NokName,
                                                AltPhoneNumber = KycDetail.AltPhoneNumber,
                                                ApprovedDate = Loanheader.ApprovedDate.Value.ToLongDateString(),
                                                ApprovedName = "", 
                                                RelationShip = KycDetail.NokRelationShip == null ? "" : KycDetail.NokRelationShip,
                                                nokphone = KycDetail.NokPhoneNumber,
                                                nokaddress = KycDetail.NokAddress,
                                                PhoneNumber = KycDetail.PhoneNumber,
                                                PFNumber = KycDetail.Pfnumber ,
                                                ResidentialAddress = KycDetail.CurrentAddress,
                                                retDate = KycDetail.RetirementDate.Value.ToLongDateString(),
                                            };

                                            LoanApp.BvnDetail = new AllLoanBvnDetails()
                                            {
                                                BVN = Loanheader.Bvn,
                                                FirstName = People1.FirstName,
                                                LastName = People1.LastName,
                                                DateOfBirth = People1.Age,
                                                MiddleName = People1.MiddleName,
                                                PhoneNumber1 = People1.PhoneNumber,
                                                EnrollmentBank = "" /*bvnDetails.EnrollmentBank*/,
                                                EnrollmentBranch = "" /*bvnDetails.EnrollmentBranch*/,
                                                RegistrationDate = "" /*bvnDetails.RegistrationDate*/,
                                                ResponseCode = "" /*bvnDetails.ResponseCode*/,
                                                WatchListed = "" /*bvnDetails.WatchListed*/
                                            };

                                            if (LoanApp != null)
                                            {
                                                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Retrieved successfully.", true, LoanApp, LoanApp, Status.Success, StatusMgs.Success);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // await EmailHelpers.SendNewLoanAppEmail(_environment, loanApp.AcctDetail.FirstName, loanApp.AcctDetail.MiddleName, ClientAcct.Username, DefalutToken.ClientLogin);

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your IPPIS Number is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }
                    else
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your IPPIS Number is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your IPPIS Number is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your login account is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Your account login is no longer activate to apply for this loan or has been deactivated.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CancelLoanApp(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                // AcctId = 46;

                var Client = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => (n.AccountId == AcctId && n.Id == AppId) || n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null && Client.Status != StatusMgs.Cancel)
                {
                    Client.Status = StatusMgs.Cancel;
                    Client.ApprovedComment = loanAppRequest.Comment;
                    Client.ApprovedDate = DateTime.Now;
                    lapoLoanDB.Entry(Client).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your loan application has been cancelled.", true, null, null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "This loan has already been cancelled.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateCustomerLoanPermission(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string PfNumber)
        {
            try
            {
                //var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                //var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                long AcctId = long.Parse(PfNumber);

                var Client = await lapoLoanDB.SecurityAccounts.Where(n => n.Id == AcctId).FirstOrDefaultAsync();

                if (Client != null && (Client.Status == StatusMgs.Active || Client.Status == StatusMgs.Success))
                {
                    Client.Status = StatusMgs.NotActive;
                    lapoLoanDB.Entry(Client).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Customer has been disabled.", true, null, null, Status.Success, StatusMgs.Success);
                }
                else if (Client != null && (Client.Status == StatusMgs.NotActive || Client.Status == StatusMgs.NotActive))
                {
                    Client.Status = StatusMgs.Active;
                    lapoLoanDB.Entry(Client).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Customer has been enabled.", true, null, null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Customer cannot be de-activate", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AdminCancelLoanApp(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null && Client.Status == StatusMgs.Pending)
                {
                    var loandts = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == Client.Id).FirstOrDefaultAsync();

                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();
                   
                    var lSettings = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                    if (loandts != null && Person != null && lSettings!=null)
                    {
                        Client.Status = StatusMgs.Cancel;
                        Client.ApprovedComment = loanAppRequest.Comment;
                        Client.ApprovedDate = DateTime.Now;
                        Client.ApprovedByAccountId = AcctId;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "PS-Loans " + Person.FirstName + " " + Person.LastName + " Your loan application was rejected. Reasons: " + Client.ApprovedComment;

                        if (lSettings.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.HasValue && lSettings.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.Value)
                        {
                            await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, Person.PhoneNumber, message, Client.AccountId);
                        }

                        if (lSettings.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.HasValue && lSettings.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.Value)
                        {
                            await new EmailHelpers(this._configuration).SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, new DefalutToken(_configuration).ClientLogin(), loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);
                        }

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan application has been cancelled.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan has already been cancelled.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AdminApproveLoanApp(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null && Client.Status == StatusMgs.Pending)
                {
                    var loandts = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == Client.Id).FirstOrDefaultAsync();
                   
                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    var lSettings =  await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                    if (loandts != null && Person != null && lSettings!=null)
                    {
                        Client.Status = StatusMgs.Approved;
                        Client.ApprovedComment = "" + " Approved Note: " + "  " + loanAppRequest.Comment;
                        Client.ApprovedDate = DateTime.Now;
                        Client.ApprovedByAccountId = AcctId;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "From Lapo - " + Person.FirstName + " " + Person.LastName + " Your loan has been approved. Please wait while we transfer loan amount into your bank account.";

                        if (lSettings.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.HasValue && lSettings.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.Value)
                        {
                            await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, Person.PhoneNumber, message, Client.AccountId);
                        }

                        if (lSettings.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.HasValue && lSettings.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.Value)
                        {
                            await new EmailHelpers(this._configuration).SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, new DefalutToken(_configuration).ClientLogin(), loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status);
                        }

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan application has been approved.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }
                else if (Client != null && (Client.Status == StatusMgs.Approved || Client.Status == StatusMgs.Success))
                {
                    var loandts = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == Client.Id).FirstOrDefaultAsync();

                    var Person = await lapoLoanDB.People.Where(n => n.AccountId == Client.AccountId).FirstOrDefaultAsync();

                    if (loandts != null && Person != null)
                    {
                        Client.Status = StatusMgs.Completed;
                        Client.ApprovedComment = " Approved Note: " + Client.ApprovedComment + " Due Note: " + "  " + loanAppRequest.Comment;
                        Client.ApprovedDate = DateTime.Now;
                        Client.ApprovedByAccountId = AcctId;
                        lapoLoanDB.Entry(Client).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        var loaddetails = new { amount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loandts.Amount)).Replace(".00", ""), tenure = loandts.Tenure, bank = loandts.BankAccount, acctname = loandts.BankAccountName, acctNo = loandts.BankAccountNumber, message = Client.ApprovedComment };

                        var message = "From Lapo - " + Person.FirstName + " " + Person.LastName + " Your loan repayment has been completed.";
                        await new TwoFactorsHelper(this._configuration).AdminSendLoanSmsAsync(_environment, message, Client.AccountId);

                        await new EmailHelpers(this._configuration).SendProcessLoanAppNyEmail(_environment, Person.FirstName, Person.MiddleName, Person.EmailAddress, new DefalutToken(_configuration).ClientLogin(), loaddetails, Client.Pfnumber, Client.ApprovedDate.Value.ToLongDateString(), Client.ApprovedDate.Value.ToLongTimeString(), Client.Status, Client.AccountId);

                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan repayment is completed", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan application has already been cancelled or Approved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAllMonthlyNetPays(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var ClientNetPays = await lapoLoanDB.ClientMonthlyNetPays.ToListAsync();

                if (pagenationFilter.IsSearchBar)
                {
                    long Amount = 0;
                    try
                    {
                        Amount = Convert.ToInt64(pagenationFilter.searchText);
                    }
                    catch (Exception exs)
                    {

                    }

                    if (pagenationFilter.status.Equals("All"))
                    {
                        ClientNetPays = await lapoLoanDB.ClientMonthlyNetPays.Where(n => /*((n.Year >= fromDate.Year && n.Year <= toDate.Year)) &&*/  (n.MonthName.Contains(pagenationFilter.searchText) || Amount == n.Year)).ToListAsync();
                    }
                    else
                    {
                        ClientNetPays = await lapoLoanDB.ClientMonthlyNetPays.Where(n =>/* ((n.Year >= fromDate.Year && n.Year <= toDate.Year)) &&*/ (n.MonthName.Contains(pagenationFilter.searchText) || Amount == n.Year)).ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        ClientNetPays = await lapoLoanDB.ClientMonthlyNetPays.Where(n => ((n.Year >= fromDate.Year && n.Year <= toDate.Year))).ToListAsync();
                    }
                    else
                    {
                        ClientNetPays = await lapoLoanDB.ClientMonthlyNetPays.Where(n => ((n.Year >= fromDate.Year && n.Year <= toDate.Year))).ToListAsync();
                    }
                }

                if (ClientNetPays == null)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "You don't have any applied loan for now.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                ClientNetPays = ClientNetPays.OrderByDescending(s => s.Year).OrderByDescending(s => s.Month).ToList();

                var clientNetPays = new List<MonthsNetPay>();
                int number = 0;

                foreach (var clientNetPay in ClientNetPays)
                {
                    number += 1;
                    var newClientNetPay = new MonthsNetPay()
                    {
                        MonthlYear = clientNetPay.Year.ToString(),
                        MonthlyName = clientNetPay.MonthName,
                        Id = clientNetPay.Id.ToString(),
                        No = number.ToString()
                    };

                    clientNetPays.Add(newClientNetPay);
                }

                if (clientNetPays != null && (clientNetPays.Count >= 0 || clientNetPays.Count <= 0))
                {
                    var Pagenation = await new PagenationsHelper().GetPagenation<MonthsNetPay>(clientNetPays, pagenationFilter);

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loans Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Monthly net pay Retrieved  Successfully.", true, clientNetPays, clientNetPays, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> DecriptData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string loanAppRequest)
        {
            try
            {
                var dataHash = lapoCipher02.DecryptString(loanAppRequest);

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Your loan application has been approved.", true, dataHash, dataHash, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> EncriptData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string dataEncriptData)
        {
            try
            {
                dataEncriptData = lapoCipher02.DecryptString(dataEncriptData);

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Monthly net pay Retrieved  Successfully.", true, dataEncriptData, dataEncriptData, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> LoanMethodList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AcctId)
        {
            try
            {
                var AcctIdd = Convert.ToInt32(AcctId);
                //var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var LoanMethodList = new List<NameLoanMethod>();

                var Clients = await lapoLoanDB.LoanTenureSettings.ToListAsync();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenure = new NameLoanMethod()
                        {
                            Status = itm.Status,
                            Name = itm.TeunerName,
                            Description = itm.TeunerName,
                            Date = itm.CreatedDate.Value.ToShortDateString(),
                            Id = lapoCipher01.EnryptString(itm.Id.ToString()),
                            LoanInterest = itm.LoanInterest.Value.ToString()
                        };
                        LoanMethodList.Add(newTenure);
                    }

                    LoanMethodList = LoanMethodList.OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Unable to fetch loan tenure.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Unable to fetch loan tenure.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SaveLoanMethod(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, LoanMethod loanMethod)
        {
            try
            {
                if (string.IsNullOrEmpty(loanMethod.NewLoanInterest.ToString()) || string.IsNullOrWhiteSpace(loanMethod.NewLoanInterest.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan Interest is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(loanMethod.NewNameLoanTenure.ToString()) || string.IsNullOrWhiteSpace(loanMethod.NewNameLoanTenure.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan Tenure Method is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(loanMethod.Name.ToString()) || string.IsNullOrWhiteSpace(loanMethod.Name.ToString()) || loanMethod.Name < 0)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan tenure Field must contain number.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (loanMethod.Name > 1 && !loanMethod.NewNameLoanTenure.ToLower().EndsWith("s".ToLower()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Select a valid Loan Tenure, Input Tenure is greater than 1.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (loanMethod.Name <= 1 && loanMethod.NewNameLoanTenure.ToLower().EndsWith("s".ToLower()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Select a valid Loan Tenure, Input Tenure is less than 2.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (!loanMethod.NewLoanInterest.Contains("."))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan Interest must be decimal.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                int valueT;
                if (int.TryParse(loanMethod.Name.ToString(), out valueT))
                {

                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan tenure must be number.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                decimal value;
                if (Decimal.TryParse(loanMethod.NewLoanInterest.ToString(), out value))
                {

                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan Interest must be decimal.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var loanMethodName = loanMethod.Name.ToString() + "  " + loanMethod.NewNameLoanTenure.ToString();

                var AcctId = Convert.ToInt32(loanMethod.Id);

                //var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var LoanIntre = Convert.ToDouble(loanMethod.NewLoanInterest.ToString());

                var Client = await lapoLoanDB.LoanTenureSettings.Where(n => n.TeunerName == loanMethodName).FirstOrDefaultAsync();

                if (Client == null)
                {
                    var newTenures = new LoanTenureSetting()
                    {
                        CreatedDate = DateTime.Now,
                        Status = StatusMgs.Active,
                        TeunerName = loanMethodName,
                        CreatedById = AcctId,
                        LoanInterest = LoanIntre
                    };

                    newTenures.Status = StatusMgs.Active;
                    newTenures.CreatedDate = DateTime.Now;
                    lapoLoanDB.LoanTenureSettings.Add(newTenures);
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan Tenure has was created successful.", true, null, null, Status.Success, StatusMgs.Success);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan Tenure is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan Tenure has been saved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ListOfNarra(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AcctId)
        {
            try
            {
                var AcctIdd = Convert.ToInt32(AcctId);
                //var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var LoanMethodList = new List<NarrationM>();

                var Clients = await lapoLoanDB.Narrations.ToListAsync();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenure = new NarrationM()
                        {
                            Name = itm.Name,
                            Date = itm.CreatedDate.Value.ToShortDateString(),
                        };
                        LoanMethodList.Add(newTenure);
                    }

                    //LoanMethodList = LoanMethodList.OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Narration has been retrieved.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Unable to fetch loan tenure.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AdminSaveNewNarration(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, NewNarrations newNarration)
        {
            try
            {
                if (string.IsNullOrEmpty(newNarration.NewNarration.ToString()) || string.IsNullOrWhiteSpace(newNarration.NewNarration.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan Narration is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (string.IsNullOrEmpty(newNarration.Id.ToString()) || string.IsNullOrWhiteSpace(newNarration.Id.ToString()))
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "User not found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Client = await lapoLoanDB.Narrations.Where(n => n.Name == newNarration.NewNarration).FirstOrDefaultAsync();

                if (Client == null)
                {
                    var newTenures = new Narration()
                    {
                        CreatedDate = DateTime.Now,
                        Name = newNarration.NewNarration,
                        CreatedById = Convert.ToInt64(newNarration.Id)
                    };
                    lapoLoanDB.Narrations.Add(newTenures);
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Narration has been created successfully.", true, null, null, Status.Success, StatusMgs.Success);
                }
                else
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Narration is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                }
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateLoanMethod(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CancelLoanAppRequest loanAppRequest)
        {
            try
            {
                // var AcctId = Convert.ToInt32(loanAppRequest.AccountId);
                var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var Client = await lapoLoanDB.LoanTenureSettings.Where(n => n.Id == AppId).FirstOrDefaultAsync();

                if (Client != null && Client.Status == StatusMgs.NotActive)
                {
                    Client.Status = StatusMgs.Active;
                    lapoLoanDB.Entry(Client).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan status has been activate.", true, null, null, Status.Success, StatusMgs.Success);
                }
                else if (Client != null && Client.Status == StatusMgs.Active)
                {
                    Client.Status = StatusMgs.NotActive;
                    lapoLoanDB.Entry(Client).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan status has been de-activated.", true, null, null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan application has already been cancelled.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> LoanMethodListApp(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AcctId)
        {
            try
            {
                var AcctIdd = Convert.ToInt32(AcctId);
                //var AppId = (long)Convert.ToInt32(lapoCipher01.DecryptString(loanAppRequest.LoadHeaderId));

                var LoanMethodList = new List<NameLoanMethod>();
                var Clients = await lapoLoanDB.LoanTenureSettings.Where(s => s.Status == StatusMgs.Active).ToListAsync();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenure = new NameLoanMethod()
                        {

                            Status = itm.Status,
                            Name = itm.TeunerName,
                            Description = itm.TeunerName,
                            Date = itm.CreatedDate.Value.ToShortDateString(),
                            Id = lapoCipher01.EnryptString(itm.Id.ToString()),
                            LoanInterest = itm.LoanInterest.Value.ToString()
                        };
                        LoanMethodList.Add(newTenure);
                    }

                    LoanMethodList = LoanMethodList.OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                    //LoanMethodList = LoanMethodList.OrderByDescending(o => o.Name).ThenBy(i => i.Name).ToList();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Unable to fetch loan tenure.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Unable to fetch loan tenure.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetCustomerDashboardDetails(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CustomerAcctDto customerAcct)
        {
            try
            {

                double TotalamountNetPay, TotalLoanAmount, TotalCompletedLoan;

                TotalamountNetPay = 0;
                TotalLoanAmount = 0;

                TotalCompletedLoan = 0;

                var AppId = (long)Convert.ToInt32(customerAcct.CustomerId);

                var newCustomerDashboard = new DashboardModelDto();

                //var CurrentYear = DateTime.Now.Year - 5;
                //var CurrentMonth = DateTime.Now.Month;

                var PendingLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == AppId && n.Status == StatusMgs.Pending /*&& n.CreatedDate.Value.Year == CurrentYear*/).ToListAsync();

                var CancelLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == AppId && n.Status == StatusMgs.Cancel /*&& n.CreatedDate.Value.Year == CurrentYear*/).ToListAsync();

                var TotalApprovedLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == AppId && n.Status == StatusMgs.Approved /*&& n.ApprovedDate.Value.Year == CurrentYear*/).ToListAsync();

                var TotalCompletedLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.AccountId == AppId && n.Status == StatusMgs.Completed /*&& n.ApprovedDate.Value.Year == CurrentYear*/).ToListAsync();

                var Netpays = await lapoLoanDB.Clients.Where(n => n.AccountId == AppId).FirstOrDefaultAsync();

                if(TotalCompletedLoanList != null)
                {
                    foreach (var itm in TotalCompletedLoanList)
                    {
                        var ApprovedDetailsList = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == itm.Id /*&& n.CreatedDate.Year == CurrentYear && n.CreatedDate.Month == CurrentMonth*/).ToListAsync();

                        foreach (var dt in ApprovedDetailsList)
                        {
                            TotalCompletedLoan += (double)dt.Amount.Value;
                        }
                    }
                }

                if (TotalApprovedLoanList != null)
                {
                    foreach (var itm in TotalApprovedLoanList)
                    {
                        var ApprovedDetailsList = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == itm.Id /*&& n.CreatedDate.Year == CurrentYear && n.CreatedDate.Month == CurrentMonth*/).ToListAsync();

                        foreach (var dt in ApprovedDetailsList)
                        {
                            TotalLoanAmount += (double)dt.Amount.Value;
                        }
                    }
                }

                //if (Netpays != null)
                //{
                //    var Netpayss = await lapoLoanDB.ClientNetPays.Where(n => n.ClientId == Netpays.Id && n.Npfdate.Value.Year == CurrentYear && n.Npfdate.Value.Month == CurrentMonth).ToListAsync();

                //    foreach (var itm in Netpayss)
                //    {

                //    }
                //}

                TotalamountNetPay = 0 ;

                //if (Netpays != null)
                //{
                //    var Netpayss = await lapoLoanDB.ClientNetPays.Where(n => n.ClientId == Netpays.Id).ToListAsync();

                //    foreach (var itm in Netpayss)
                //    {
                //        TotalamountNetPay += (double)itm.NetPay.Value;
                //    }
                //}

                if (TotalCompletedLoan > 0)
                {
                    newCustomerDashboard.TotalCompletedLoan = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)TotalCompletedLoan)).Replace(".00", "").ToString();
                    newCustomerDashboard.TotalCompletedLoan1 = TotalCompletedLoanList.Count.ToString();
                }
                else
                {
                    newCustomerDashboard.TotalCompletedLoan = "0";
                    newCustomerDashboard.TotalCompletedLoan1 = TotalCompletedLoanList.Count.ToString();
                }

                if (PendingLoanList != null && PendingLoanList.Count > 0)
                {
                    newCustomerDashboard.TotalPendingLoan = PendingLoanList.Count.ToString();
                }
                else
                {
                    newCustomerDashboard.TotalPendingLoan = 0.ToString();
                }

                if (CancelLoanList != null && CancelLoanList.Count > 0)
                {
                    newCustomerDashboard.TotalCancelledLoan = CancelLoanList.Count.ToString();
                }
                else
                {
                    newCustomerDashboard.TotalCancelledLoan = 0.ToString();
                }

                if (TotalamountNetPay > 0)
                {
                    newCustomerDashboard.TotalNetPay = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)TotalamountNetPay)).Replace(".00", "").ToString();
                }
                else
                {
                    newCustomerDashboard.TotalNetPay = "0";
                }

                if (TotalLoanAmount > 0)
                {
                    newCustomerDashboard.TotalLoanAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)TotalLoanAmount)).Replace(".00", "").ToString();
                }
                else
                {
                    newCustomerDashboard.TotalLoanAmount = "0";
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan application has been approved.", true, newCustomerDashboard, newCustomerDashboard, Status.Success, StatusMgs.Success);

            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAdminDashboardDetails(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, CustomerAcctDto customerAcct1)
        {
            try
            {
                double TotalamountNetPay, TotalLoanAmount, TotalCompletedLoan1;

                TotalamountNetPay = 0;
                TotalLoanAmount = 0;
                TotalCompletedLoan1 = 0;

                // var AppId = (long)Convert.ToInt32(customerAcct.CustomerId);

                //var CurrentYear = DateTime.Now.Year - 5;
                //var CurrentMonth = DateTime.Now.Month;

                var newCustomerDashboard = new DashboardModelDto();

                var PendingLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Status == StatusMgs.Pending /*&& n.CreatedDate.Value.Year == CurrentYear*/).ToListAsync();

                var CancelLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => (n.Status == StatusMgs.Cancel || n.Status == "Cancel")/* && n.CreatedDate.Value.Year == CurrentYear*/).ToListAsync();

                var TotalApprovedLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Status == StatusMgs.Ongoing/* && n.CreatedDate.Value.Year == CurrentYear*/).ToListAsync();

                var TotalCompletedLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Status == StatusMgs.Completed /*&& n.CreatedDate.Value.Year == CurrentYear*/).ToListAsync();

                var TotalOngoingLoanList = await lapoLoanDB.LoanApplicationRequestHeaders.Where(n => n.Status == StatusMgs.Approved /*&& n.CreatedDate.Value.Year == CurrentYear*/).ToListAsync();


               // var Netpays = await lapoLoanDB.Clients.ToListAsync();

                if (TotalApprovedLoanList != null)
                {
                    foreach (var itm in TotalApprovedLoanList)
                    {
                        var ApprovedDetailsList = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == itm.Id).ToListAsync();

                        foreach (var dt in ApprovedDetailsList)
                        {
                            TotalLoanAmount += (double)dt.Amount.Value;
                        }
                    }
                }


                if (TotalCompletedLoanList != null)
                {
                    foreach (var itm in TotalCompletedLoanList)
                    {
                        var CompletedDetailsList = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == itm.Id).ToListAsync();

                        foreach (var dt in CompletedDetailsList)
                        {
                            TotalCompletedLoan1 += (double)dt.Amount.Value;
                        }
                    }
                }


                //if (Netpays != null)
                //{
                //    foreach (var dt in Netpays)
                //    {
                //        var Netpayss = await lapoLoanDB.ClientNetPays.Where(n => n.ClientId == dt.Id && n.Npfdate.Value.Month == CurrentMonth && n.Npfdate.Value.Year == CurrentYear).ToListAsync();

                //        foreach (var itm in Netpayss)
                //        {

                //        }
                //    }
                //}


                if (TotalOngoingLoanList != null)
                {
                    foreach (var itm in TotalOngoingLoanList)
                    {
                        var CompletedDetailsList = await lapoLoanDB.LoanApplicationRequestDetails.Where(n => n.LoanAppRequestHeaderId == itm.Id).ToListAsync();

                        foreach (var dt in CompletedDetailsList)
                        {
                            TotalamountNetPay += (double)dt.Amount.Value;
                        }
                    }
                }

                //if (Netpays != null)
                //{
                //    var Netpayss = await lapoLoanDB.ClientNetPays.Where(n => n.ClientId == Netpays.Id).ToListAsync();

                //    foreach (var itm in Netpayss)
                //    {
                //        TotalamountNetPay += (double)itm.NetPay.Value;
                //    }
                //}

                if (PendingLoanList != null && PendingLoanList.Count > 0)
                {
                    newCustomerDashboard.TotalPendingLoan = PendingLoanList.Count.ToString();
                }
                else
                {
                    newCustomerDashboard.TotalPendingLoan = 0.ToString();
                }

                if (CancelLoanList != null && CancelLoanList.Count > 0)
                {
                    newCustomerDashboard.TotalCancelledLoan = CancelLoanList.Count.ToString();
                }
                else
                {
                    newCustomerDashboard.TotalCancelledLoan = 0.ToString();
                }

                if (TotalamountNetPay > 0)
                {
                    newCustomerDashboard.TotalNetPay = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)TotalamountNetPay)).Replace(".00", "").ToString();
                }
                else
                {
                    newCustomerDashboard.TotalNetPay = "0";
                }

                if (TotalLoanAmount > 0)
                {
                    newCustomerDashboard.TotalLoanAmount1 = TotalApprovedLoanList.Count.ToString(); 
                    newCustomerDashboard.TotalLoanAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)TotalLoanAmount)).Replace(".00", "").ToString();
                }
                else
                {
                    newCustomerDashboard.TotalLoanAmount1 = TotalApprovedLoanList.Count.ToString();
                    newCustomerDashboard.TotalLoanAmount = "0";
                }


                newCustomerDashboard.TotalCompletedLoan = TotalCompletedLoanList.Count.ToString();
                newCustomerDashboard.TotalCompletedLoan1 = string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)TotalCompletedLoan1)).Replace(".00", "").ToString();
                
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan application has been approved.", true, newCustomerDashboard, newCustomerDashboard, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AdminUpdateLoanSettings(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ModelDto.LoanSettingsModel loanSettings)
        {
            try
            {
                try
                {
                    decimal.Parse(loanSettings.LoanInterest.ToString());
                }
                catch(Exception exxx)
                {
                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Enter a valid Interest Rate and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var AcctId = Convert.ToInt32(loanSettings.AcctId);

                var LoanIntre = Convert.ToDouble(loanSettings.LoanInterest.ToString());

                var LoanSet = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                if (LoanSet == null)
                {
                    var newTenures = new LoanSetting()
                    {
                        LastUpdated = DateTime.Now,
                        IsCustomerHasLoanPermission = loanSettings.IsBlockLoanPortal,
                        CreatedByName = "",
                        LastUpdatedAccountId = (long)AcctId,
                        LoanInterest = loanSettings.LoanInterest,
                        MaxLoanAmount = (decimal)loanSettings.MaxLoanAmount,
                        MinLoanAmount = (decimal)loanSettings.MinLoanAmount,
                        UseFlatRateLoanInterestCalculation = loanSettings.LoanInterestCalculation,
                        UseSalaryAsMaxLoanAmount = loanSettings.LoanScheduled,
                        Message = loanSettings.Message,
                         SendSmsToTeamLeadWhenApplicationIsSubmitted = loanSettings.SendSMSHubTeamLeadLoanApplicationSubmitted,
                          SendEmailToTeamLeadWhenApplicationIsSubmitted = loanSettings.SendEmailHubTeamLeadLoanApplicationSubmitted,
                           SendEmailToAppliedCustomersWhenApplicationIsSubmitted= loanSettings.SendEmailCustomerLoanApplicationSubmitted,
                            SendSmsToAppliedCustomersWhenApplicationIsSubmitted = loanSettings.SendSMSCustomerLoanApplicationSubmitted
                    };
                    lapoLoanDB.LoanSettings.Add(newTenures);
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan settings was created successfully.", true, null, null, Status.Success, StatusMgs.Success);
                }
                else
                {

                    LoanSet.LastUpdated = DateTime.Now;
                    LoanSet.IsCustomerHasLoanPermission = loanSettings.IsBlockLoanPortal;
                    LoanSet.CreatedByName = "";
                    LoanSet.LastUpdatedAccountId = (long)AcctId;
                    LoanSet.LoanInterest = loanSettings.LoanInterest;
                    LoanSet.MaxLoanAmount = (decimal)loanSettings.MaxLoanAmount;
                    LoanSet.MinLoanAmount = (decimal)loanSettings.MinLoanAmount;
                    LoanSet.UseFlatRateLoanInterestCalculation = loanSettings.LoanInterestCalculation;
                    LoanSet.UseSalaryAsMaxLoanAmount = loanSettings.LoanScheduled;
                    LoanSet.Message = loanSettings.Message;

                    LoanSet.SendSmsToTeamLeadWhenApplicationIsSubmitted = loanSettings.SendSMSHubTeamLeadLoanApplicationSubmitted;
                    LoanSet.SendEmailToTeamLeadWhenApplicationIsSubmitted = loanSettings.SendEmailHubTeamLeadLoanApplicationSubmitted;
                    LoanSet.SendEmailToAppliedCustomersWhenApplicationIsSubmitted = loanSettings.SendEmailCustomerLoanApplicationSubmitted;
                    LoanSet.SendSmsToAppliedCustomersWhenApplicationIsSubmitted = loanSettings.SendSMSCustomerLoanApplicationSubmitted;

                    lapoLoanDB.Entry(LoanSet).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan settings was updated successfully.", true, null, null, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Fill in the requred space properly and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetBankAcctName(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, BankAcctNameModel bankAcct)
        {
            try
            {
                if (bankAcct != null)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(bankAcct.AcctNo.ToString(), "^[0-9]*$"))
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(bankAcct.AcctNo.ToString(), "^[0-9]*$"))
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(bankAcct.AcctNo.ToString(), "^[0-9]*$"))
                    {
                        return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    //if (System.Text.RegularExpressions.Regex.IsMatch("^[0-9]", bankAcct.AcctNo.ToString()))
                    //{
                    //    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
                    //}

                    var IsResult = await new BankHttpClient(_configuration).GetBankNameAsync(_environment, bankAcct);
                    if (IsResult != null && IsResult.IsActive)
                    {
                        var BsBankNames = await lapoLoanDB.BsBankNames.Where(u => u.BankShortCode == bankAcct.Selected).FirstOrDefaultAsync();

                        if (BsBankNames != null)
                        {
                            var data = new { BankName = BsBankNames.BankName, AcctName = IsResult.DataLoad };

                            return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, IsResult.Message, true, data, data, Status.Success, StatusMgs.Success);
                        }
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, IsResult.DataLoad, IsResult.DataLoad, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Invalid bank account details. Please check and try again" , false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AdminGetLoanSettings(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AccountId)
        {
            try
            {
                var LoanSet = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                if (LoanSet != null)
                {
                    var newSettings = new ModelDto.LoanSettingsModel()
                    {
                        AcctId = AccountId,
                        IsBlockLoanPortal = LoanSet.IsCustomerHasLoanPermission.Value,

                        LoanInterest = LoanSet.LoanInterest.Value,

                        MinLoanAmount = (double)LoanSet.MinLoanAmount.Value,

                        MaxLoanAmount = (double)LoanSet.MaxLoanAmount.Value,

                        Message = LoanSet.Message,

                        LoanScheduled = LoanSet.UseSalaryAsMaxLoanAmount.Value,

                        LoanInterestCalculation = LoanSet.UseFlatRateLoanInterestCalculation.Value,

                        SendEmailCustomerLoanApplicationSubmitted = LoanSet.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.Value,

                          SendSMSHubTeamLeadLoanApplicationSubmitted = LoanSet.SendSmsToTeamLeadWhenApplicationIsSubmitted.Value,

                           SendSMSCustomerLoanApplicationSubmitted = LoanSet.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.Value,

                            SendEmailHubTeamLeadLoanApplicationSubmitted = LoanSet.SendEmailToTeamLeadWhenApplicationIsSubmitted.Value,
                    };

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan settings was retrieved successfully.", true, newSettings, newSettings, Status.Success, StatusMgs.Success);
                }
                else
                {
                    var newSettings = new ModelDto.LoanSettingsModel()
                    {
                        AcctId = AccountId,
                        IsBlockLoanPortal = false,
                        LoanInterest = 0.0,
                        MinLoanAmount = 0.0,
                        MaxLoanAmount = 0.0,
                        Message = "",
                        LoanScheduled = true,
                        LoanInterestCalculation = false,

                        SendEmailCustomerLoanApplicationSubmitted = false,

                        SendSMSHubTeamLeadLoanApplicationSubmitted = false,

                        SendSMSCustomerLoanApplicationSubmitted = false,

                        SendEmailHubTeamLeadLoanApplicationSubmitted = false,
                    };

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "Loan settings was retrieved successfully.", true, newSettings, newSettings, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "Loan settings not found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ListOfStates(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        {
            try
            {                
                var LoanMethodList = new List<StateModel>();

                var Clients = await lapoLoanDB.BsStates.ToListAsync();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenure = new StateModel()
                        {
                            Id = itm.StateId.ToString(),
                            Name = itm.SateName
                        };

                        LoanMethodList.Add(newTenure);
                    }

                    //LoanMethodList = LoanMethodList.OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "State has been retrieved.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "State has been retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ListOfCitysByState(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, int Id)
        {
            try
            {
                var LoanMethodList = new List<StateModel>();

                var Clients = await lapoLoanDB.BsCities.Where(s=>s.StateId == Id).ToListAsync();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenure = new StateModel()
                        {
                            Id = itm.CityId.ToString(),
                            Name = itm.CityName
                        };

                        LoanMethodList.Add(newTenure);
                    }

                    //LoanMethodList = LoanMethodList.OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "State has been retrieved.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "State has been retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ListOfBusinessSegment(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        {
            try
            {
                var LoanMethodList = new List<StateModel>();

                var Clients = await lapoLoanDB.BsBusinessSegments.Where(n => n.BizSegType.ToLower() == "024".ToLower() || n.BizSegDesc.ToLower() == "Public Sector".ToLower()).ToListAsync();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenure = new StateModel()
                        {
                            Id = itm.BizSegId.ToString(),
                            Name = itm.BizSegDesc
                        };

                        LoanMethodList.Add(newTenure);
                    }

                    LoanMethodList = LoanMethodList.Where(n => n.Name.ToLower() == "Public Sector".ToLower()).ToList();

                    //LoanMethodList = LoanMethodList.OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "State has been retrieved.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "State has been retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ListOfBusinessTypes(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, int Id)
        {
            try
            {
                var LoanMethodList = new List<StateModel>();

                var Clients = await lapoLoanDB.BsMinistries.ToListAsync();

                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var itm in Clients)
                    {
                        var newTenure = new StateModel()
                        {
                            Id = itm.MinsId.ToString(),
                            Name = itm.MinsName
                        };

                        LoanMethodList.Add(newTenure);
                    }

                    //LoanMethodList = LoanMethodList.OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Success, "State has been retrieved.", true, LoanMethodList, LoanMethodList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, "State has been retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error,   new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }
    }
}
