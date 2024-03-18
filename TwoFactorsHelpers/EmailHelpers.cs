using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using LapoLoanWebApi.LoanScheduled.Model;
using LapoLoanWebApi.LoanScheduled;

namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class EmailHelpers
    {
        private IConfiguration _configuration;

        public EmailHelpers(IConfiguration _configuration)
        {
            this._configuration = _configuration;
        }

        private  Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private LapoLoanDBContext lapoLoanDB = null;
      

        public bool Isent = false;

        private  byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public  async Task<RespondMessageDto> SendLoginAuth(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AcctId, string email, string Code)
        {
            try
            {
                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);
                //  string[] filePaths = Directory.GetFiles(Path.Combine(Environment.WebRootPath, "HtmlPackages/"));

                // var FileName = Path.GetFileName(filePath);
                //  var Data = System.IO.File.ReadAllText("MyTextFile.txt");

                // var file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "HtmlPackages", "twofactorAuth.html");

                // string SendData = System.IO.File.ReadAllText(file);

                //  string path = Path.Combine(Environment.WebRootPath, "HtmlPackages/") + "twofactorAuth.html";

                //Read the File data into Byte Array.
                //  byte[] bytes = System.IO.File.ReadAllBytes(path);
                //  string SendData = System.IO.File.ReadAllText(path);

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "twofactorAuth.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData) && this.Isent == false)
                {
                    this.Isent = true;

                    var url = new DefalutToken(_configuration).AngularHost() + "/twofactorauth?AcctId=" + AcctId.ToString() + "&bvn=&code=SDARERTWW&crDate=2023-05-22T11:37:47&expDate=2023-05-22T12:37:47&gedDate=2023-05-22T11:37:47&id=99&expired=true&TwoFactor=true&page=%2Fsignin";

                    var HtmlResult = HtmlData.Replace("codeDisplay", Code).Replace("ActionLink", url).Replace("{codeExpire}", "5");

                    //HtmlResult = HtmlData.Replace("ActionLink", url);
                    //HtmlResult = HtmlData.Replace("{codeExpire}", "140");
                    var HtmlResult1 = HtmlResult;

                    if(DefalutToken.UseDevEmailAndPhoneNumber)
                    {
                        email = "palsofty@gmail.com";
                    }

                    var sendEmail = new SendEmail()
                    {
                        subject = "Your two-factor login otp code is " + Code,
                        emailaddress = email,
                        body = HtmlResult1,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);
                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                { 
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();
                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }
                else
                {
                    this.Isent = false;
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, "", "", ModelDto.Status.Success, StatusMgs.Success);
                }


                this.Isent = false;
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);

            }
            catch (Exception ex)
            {
                this.Isent = false;
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public  async Task<RespondMessageDto> SendAccountCreation(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AcctId, string email, string link)
        {
            try
            {
                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);
                
                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "accountcreationTemplate.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData))
                {
                    var HtmlResult = HtmlData.Replace("ActionLink", link);

                    var HtmlResult1 = HtmlResult;

                    //if (DefalutToken.UseDevEmailAndPhoneNumber)
                    //{
                    //   email = "palsofty@gmail.com";
                    //}

                    SendEmail sendEmail = new SendEmail()
                    {
                        subject = "LAPO Microfinance Bank: Welcome to PS-Loans",
                        emailaddress = email,
                        body = HtmlResult1,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                {
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();
                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    FileLogActivities.CallSevice("Email Sender for customer Reg ", Message + " Code: " + MessageCode, Message);

                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {

                                    FileLogActivities.CallSevice("Email Sender for customer Reg ", Message + " Code: " + MessageCode, Message);
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {

                                try
                                {
                                    string getRes = await response.Content.ReadAsStringAsync();

                                    var geSrchBvnObj = JObject.Parse(getRes);

                                    Boolean Status = (Boolean)geSrchBvnObj["status"];
                                    string Message = (string)geSrchBvnObj["message_description"].ToString();
                                    string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                    FileLogActivities.CallSevice("Email Sender for customer Reg ", Message + " Code: " + MessageCode, Message);

                                }
                                catch(Exception ex)
                                {
                                    FileLogActivities.CallSevice("Email Sender for customer Reg ", ex.Message ?? ex.InnerException.Message, ex.Message?? ex.InnerException.Message);
                                }

                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }
                else
                {

                    FileLogActivities.CallSevice(" ", "Couldn't have access token", " ");
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, "", "", ModelDto.Status.Success, StatusMgs.Success);
                }

                FileLogActivities.CallSevice(" ", "Couldn't have access token", " ");
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice(" ", "Couldn't have access token", " ");
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private  bool IsSend = false;
        private  string Code12333 = "";

        public  async Task<RespondMessageDto> SendBvnAuth(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AcctId, string email, string Code)
        {
            try
            {
                if(Code12333 == Code)
                {
                    IsSend = true;
                }
                else
                {
                    IsSend = false;
                    Code12333 = Code;
                }

                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);
                //  string[] filePaths = Directory.GetFiles(Path.Combine(Environment.WebRootPath, "HtmlPackages/"));

                // var FileName = Path.GetFileName(filePath);
                //  var Data = System.IO.File.ReadAllText("MyTextFile.txt");

                // var file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "HtmlPackages", "twofactorAuth.html");

                // string SendData = System.IO.File.ReadAllText(file);

                //  string path = Path.Combine(Environment.WebRootPath, "HtmlPackages/") + "twofactorAuth.html";

                //Read the File data into Byte Array.
                //  byte[] bytes = System.IO.File.ReadAllBytes(path);
                //  string SendData = System.IO.File.ReadAllText(path);

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "bvnAuthentication.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                FileLogActivities.CallSevice("Email Helper", "Sending Looping", "Starting");

                if (AllAccessTokens != null && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData) && this.IsSend == false)
                {
                    this.IsSend = true;
                      var url = new DefalutToken(_configuration).AngularHost() + "/loanbvnapp?AccountId=" + AcctId;

                    var HtmlResult = HtmlData.Replace("codeDisplay", Code).Replace("ActionLink", url).Replace("codeExpire", "5");

                    //HtmlResult = HtmlData.Replace("ActionLink", url);
                    //HtmlResult = HtmlData.Replace("{codeExpire}", "140");
                    var HtmlResult1 = HtmlResult;

                    //if (DefalutToken.UseDevEmailAndPhoneNumber)
                    //{
                    //    email = "palsofty@gmail.com";
                    //}

                    var sendEmail = new SendEmail()
                    {
                        subject = "PS-Loans: BVN Authentication Code " + Code,
                        emailaddress = email,
                        body = HtmlResult1,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);
                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                               
                                string getRes = await response.Content.ReadAsStringAsync();

                                //FileLogActivities.CallSevice("Email Helper", "Sending Looping" + email, getRes);

                                FileLogActivities.CallSevice("Email Helper", "Sending Success Message:" + new DefalutToken(_configuration).EmailBasedUrl() + email, getRes);

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                {
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();
                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    this.Isent = false;
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {
                                    this.Isent = false;
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                FileLogActivities.CallSevice("Email Helper", "Sending Message:" + new DefalutToken(_configuration).EmailBasedUrl() + email, getRes);

                                this.Isent = false;
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }

                this.Isent = false;
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);

            }
            catch (Exception ex)
            {
                this.Isent = false;

                FileLogActivities.CallSevice("Email Helper", "Sending Looping", ex.Message ?? ex.InnerException.Message);
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
        
        public  async Task<RespondMessageDto> SendNewPasswordAuth(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string email , string link)
        {
            try
            {
                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "newPasswordEmail.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData))
                {
                    var HtmlResult = HtmlData.Replace("ResetYourPasswordLink", link);

                    var HtmlResult1 = HtmlResult;

                    if (DefalutToken.UseDevEmailAndPhoneNumber)
                    {
                        email = "palsofty@gmail.com";
                    }

                    SendEmail sendEmail = new SendEmail()
                    {
                        subject = "LAPO - Password Rest",
                        emailaddress = email,
                        body = HtmlResult1,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                {
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();
                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, "", "", ModelDto.Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);

            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public  async Task<RespondMessageDto> SendNewLoanAppEmail(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string CustomerFistName,  string MiddleName, string email, string link, LoanApplicationRequestDetail loanApplication, string NfNo, string nameifReceiver)
        {
            try
            {
                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "EmailTemplate.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData))
                {
                    var HtmlResult = HtmlData.Replace("ActionLink", link).Replace("CustomerFistName", CustomerFistName).Replace("MiddleName", MiddleName)./*Replace("DisplayIPPISNumber", NfNo).*/Replace("DisplayAmount", string.Format(new CultureInfo("ig-NG"), "{0:C}", NearestRounding.RoundValueToNext100((double)loanApplication.Amount)).Replace(".00", "") ).Replace("DisplayTenure", loanApplication.Tenure).Replace("DisplayBankName", loanApplication.BankAccount).Replace("DisplayAccountName", loanApplication.BankAccountName).Replace("DisplayAccountNo", loanApplication.BankAccountNumber);

                    HtmlResult = "";

                    var NewScheduled = new ScheduledMethod()
                    {
                        AccountId = loanApplication.AccountRequestId.ToString(),
                        Amount = (double)loanApplication.Amount,
                        IPPISNumber = NfNo,
                        Tenure = loanApplication.Tenure,
                    };

                    var Scheduled = await new LoanSchedulerHelpers(null, this._configuration).CalculateScheduledLoanAmount(Environment,  NewScheduled);

                    if (Scheduled.IsActive == false)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                    }

                    if (Scheduled.DataLoad == null)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                    }

                    HtmlResult = HtmlData.Replace("{{customername}}", string.Concat(CustomerFistName, "   " , MiddleName))
                      .Replace("{{RequestAmount}}", Scheduled.DataLoad.DisbursmentAmount)
                      .Replace("{{RequestSource}}", "E- CHANNELS, WEB")
                      .Replace("{{Description}}", "LOAN APPLICATION REQUEST - PUB SECTOR")
                      .Replace("{{ProposedTerm}}", Scheduled.DataLoad.NumberOfPayments)
                      .Replace("{{RequestDate}}", loanApplication.LoanAppRequestHeader.CreatedDate.Value.ToLongDateString())
                      .Replace("{{TimeOfApplication}}", loanApplication.LoanAppRequestHeader.CreatedDate.Value.ToLongTimeString())
                      .Replace("{{DocumentNumber}}", "0")
                      .Replace("{{RqAmount}}", Scheduled.DataLoad.DisbursmentAmount)
                      .Replace("{{MonthlyRepayment}}", Scheduled.DataLoad.ScheduledList[0].LoanAmountWithCurrency)
                      .Replace("{{TotalRepayment}}", Scheduled.DataLoad.TotalAmount)
                      .Replace("{{Interest&Aacute;Rate}}", Scheduled.DataLoad.InterestPayment + "    " + Scheduled.DataLoad.Interest)
                      //.Replace("{{IPPSNumber}}", NewScheduled.IPPISNumber)
                      .Replace("{{RepaymentEndDate}}", Scheduled.DataLoad.DueDate)
                      .Replace("{{nameifReceiver}}", nameifReceiver)
                      .Replace("{{Remark}}", "");
                    
                    var HtmlResult1 = HtmlResult;

                    if (DefalutToken.UseDevEmailAndPhoneNumber)
                    {
                        email = "palsofty@gmail.com";
                    }

                    SendEmail sendEmail = new SendEmail()
                    {
                        subject = "PS-Loans: Your loan application has been submitted successfully.",
                        emailaddress = email,
                        body = HtmlResult1,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                {
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();
                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, "", "", ModelDto.Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);

            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public  async Task<RespondMessageDto> SendProcessLoanAppNyEmail(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string CustomerFistName, string MiddleName, string email, string link, dynamic loanApplication, string NfNo,string date, string time, string status, long AccountId =0)
        {
            try
            {
                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "loanprocessMessage.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);


                // var HtmlResult = HtmlData.Replace("ActionLink", link);
                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData))
                {
                    var scheduled = new ScheduledMethod()
                    {
                        AccountId = AccountId.ToString(),
                        Amount = loanApplication.amount,
                        IPPISNumber = NfNo,
                        Tenure = loanApplication.tenure,
                    };

                    var Scheduled = await new LoanSchedulerHelpers(null, this._configuration).CalculateScheduledLoanAmount(null, scheduled);

                    var HtmlResult = HtmlData.Replace("ActionLink", link).Replace("CustomerFistName", CustomerFistName).Replace("MiddleName", MiddleName)./*Replace("DisplayIPPISNumber", NfNo).*/Replace("DisplayAmount", loanApplication.amount).Replace("DisplayTenure", loanApplication.tenure).Replace("DisplayBankName", loanApplication.bank).Replace("DisplayAccountName", loanApplication.acctname).Replace("DisplayAccountNo", loanApplication.acctNo).Replace("loanmessage1", loanApplication.message).Replace("loanstatus1", status).Replace("loandate1", date).Replace("loantime1", time);

                    if (Scheduled.IsActive)
                    {
                        HtmlResult = HtmlResult.Replace("ScheduledAmount", Convert.ToString(Scheduled.DataLoad.Interest))
                              .Replace("ScheduledIntrest", Convert.ToString(Scheduled.DataLoad.Interest))
                            .Replace("ScheduledNumberofPayment", Convert.ToString(Scheduled.DataLoad.NumberOfPayments))
                            .Replace("DueRepaymentTotalAmount", Convert.ToString(Scheduled.DataLoad.TotalAmount))
                             .Replace("RepaymentDueDate", Convert.ToString(Scheduled.DataLoad.DueDate))
                             .Replace("RepaymentDueTime", Convert.ToString(Scheduled.DataLoad.DueTime));
                    }
                    else
                    {
                        HtmlResult = HtmlResult.Replace("ScheduledAmount", "None")
                            .Replace("ScheduledIntrest", "None")
                          .Replace("ScheduledNumberofPayment", "None")
                          .Replace("DueRepaymentTotalAmount", "None")
                           .Replace("RepaymentDueDate", "None")
                           .Replace("RepaymentDueTime", "None");
                    }
       
                    var HtmlResult1 = HtmlResult;

                    if (DefalutToken.UseDevEmailAndPhoneNumber)
                    {
                        email = "palsofty@gmail.com";
                    }

                    SendEmail sendEmail = new SendEmail()
                    {
                        subject = "PS-Loans: We have received your loan application.", //" + status +"
                        emailaddress = email,
                        body = HtmlResult1,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                {
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();
                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, "", "", ModelDto.Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);

            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public  async Task<RespondMessageDto> SendDisburstmentLoanAppEmail(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string CustomerFistName, string MiddleName, string email, string link, string loanApplicationMessage, string Subject)
        {
            try
            {
                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "DisburstmentLoanAppTemplate.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData))
                {
                    var HtmlResult = HtmlData.Replace("DisplayMessage", loanApplicationMessage);

                    SendEmail sendEmail = new SendEmail()
                    {
                        subject = Subject,
                        emailaddress = email,
                        body = HtmlResult,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                {
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();
                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, "", "", ModelDto.Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SendRepaymentLoanApp(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string CustomerFistName, string MiddleName, string email, string link, string loanApplicationMessage, string Subject, string customer, string RepaymentAmount, string RepaymentAmountbalance, string Paymentfor, string LoanAmount, string LoanTernor)
        {
            try
            {
                Environment = _environment;
                lapoLoanDB = new LapoLoanDBContext(this._configuration);

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "RepaymentEmailTemplate.html");
                string HtmlData = System.IO.File.ReadAllText(fullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false && !string.IsNullOrEmpty(HtmlData) && !string.IsNullOrWhiteSpace(HtmlData))
                {
                    var HtmlResult = HtmlData.Replace("{{NameReceiver}}", customer).Replace("{{MonthReceiver}}", Paymentfor).Replace("{{Re-RepaymentAmount}}", RepaymentAmount).Replace("{{Re-Paymentfor}}", Paymentfor).Replace("{{Re-LoanRepaymentBalance}}", RepaymentAmountbalance).Replace("{{Re-LoanAmount}}", LoanAmount).Replace("{{Re-LoanTenure}}", LoanTernor);

                    var sendEmail = new SendEmail()
                    {
                        subject = Subject,
                        emailaddress = email,
                        body = HtmlResult,
                        hasFile = "No"
                    };

                    var sendemail = JsonConvert.SerializeObject(sendEmail);
                    string LoginEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                    byte[] inbyte = Convert.FromBase64String(LoginEnc);
                    string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                    using MultipartFormDataContent multipartContent = new MultipartFormDataContent()
                    {
                       { new StringContent(hexs, Encoding.UTF8, MediaTypeNames.Text.Plain), "xPayload" }
                    };

                    //var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", AllAccessTokens.RefreshToken);

                        using (var response = await httpClient.PostAsync(new DefalutToken(_configuration).EmailBasedUrl(), multipartContent))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string getRes = await response.Content.ReadAsStringAsync();

                                var geSrchBvnObj = JObject.Parse(getRes);

                                Boolean Status = (Boolean)geSrchBvnObj["status"];
                                string Message = (string)geSrchBvnObj["message_description"].ToString();
                                string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                                if (Status == true)
                                {
                                    string Data = geSrchBvnObj["data"].ToString();
                                    byte[] DataByte = StringToByteArray(Data);
                                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, AllAccessTokens.AesKey, AllAccessTokens.AesIv);

                                    var getUserData = JObject.Parse(DecryptData);

                                    string SenderReference = getUserData["SenderReference"].ToString();

                                    //string aeskey = getUserData["aesKey"].ToString();
                                    //string aesIv = getUserData["aesIv"].ToString();

                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, SenderReference, SenderReference, ModelDto.Status.Success, StatusMgs.Success);

                                }
                                else
                                {
                                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, ModelDto.Status.Success, StatusMgs.Error);
                                }

                            }
                            else
                            {
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                    }
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Succeed", true, "", "", ModelDto.Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Error has occur : This problem is normally caused by incorrect SMTP server settings, or often also by a firewall or antivirus software blocking access. Please try taking the following steps to resolve this issue: Make sure you can successfully send an email from your web mail.", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
    }
}
