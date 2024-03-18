using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.HubTeams;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.ModelDto;
using NETCore.Encrypt.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LapoLoanWebApi.LoanScheduled.Model
{
    public class TimedHostedService : BackgroundService
    {
        private readonly ILogger<TimedHostedService> _logger;
        //private int _executionCount;
        private readonly IConfiguration _configuration;

        private LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.LapoLoanDBContext lapoLoanDB = null;

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public TimedHostedService(ILogger<TimedHostedService> logger, IConfiguration configuration)
        {
            this._configuration = configuration;
            this._logger = logger;
            this._configuration = configuration;
            lapoLoanDB = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.LapoLoanDBContext(this._configuration);
          

            //var conn = _configuration.GetConnectionString("ProdConnection");
            //conn = _configuration.GetConnectionString("LocalConnection");
            //conn = _configuration.GetConnectionString("TestConnection");

            //string apiKey = new DefalutToken(_configuration).BankApiToken();

            //apiKey = apiKey.Replace("", "");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            // When the timer should have no due-time, then do the work once now.
            this.CallSevice();

            using PeriodicTimer timer = new(TimeSpan.FromMinutes(25));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    this.CallSevice();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        private async void CallSevice()
        {
            try
            {
                if (DefalutToken.IsProduction == false)
                {
                     this.CallRegister();
                }
                 
                string User_Id = new DefalutToken(_configuration).User_Id(); // "565957433e29160f07b53232087d";

                var AllAccessTokens = lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefault();

                if (DefalutToken.IsProduction)
                {
                    if (AllAccessTokens == null)
                    {
                        var TokenAccessResult = await new AccessTokenHttpClient(_configuration).GetBvnTokenClientAsync();

                        if (TokenAccessResult.IsActive && TokenAccessResult.status == Status.Success)
                        {
                            var DataToken = TokenAccessResult.DataLoad as AccessTokenResponses;
                            var DataToken1 = TokenAccessResult.Data as AccessTokenResponses;

                            string Data = DataToken1.data.ToString();
                            byte[] DataByte = StringToByteArray(Data);
                            string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);

                            var aesKey = AllAccessTokens.AesKey;
                            var aesIv = AllAccessTokens.AesIv;

                            string DecryptData = NETCore.Encrypt.EncryptProvider.AESDecrypt(Database64String, aesKey, aesIv);

                            var getUserData = JObject.Parse(DecryptData);

                            string acc_Token = getUserData["access_token"].ToString();
                            aesKey = getUserData["aesKey"].ToString();
                            aesIv = getUserData["aesIv"].ToString();

                            var NormalKey = new AESRoot() { access_token = acc_Token, aesIv = aesIv, aesKey = aesKey };


                            if (DataToken != null && DataToken1 != null && NormalKey != null)
                            {

                                try
                                {
                                    // System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                                    // var folderName = Path.Combine("AccessTokenLogs", "Documents");

                                    string currentDirectory = Directory.GetCurrentDirectory();
                                    string path = "HtmlPackages";

                                    string fullDir = Path.Combine(currentDirectory, path, "AccessTokenLogs.txt");

                                    if (!string.IsNullOrEmpty(fullDir) && !string.IsNullOrWhiteSpace(fullDir) && System.IO.File.Exists(fullDir))
                                    {
                                        var alltext = System.IO.File.ReadAllText(fullDir);

                                        string[] strings = { alltext, "_ _ __ __ __ __ __ _ _ __ ___", "_ ___ ___ __ __ __ __ __ __", "__ __ ____ ____ ___ __ __", "___ ____ ___ ____ __ ___", "__ __ ____ ___ __ ___ ___", "New Access Token ", "Access Token: " + acc_Token, "Aes Key: " + aesKey, "AesIv: " + aesIv, "Created Date: " + DateTime.Now.ToString(), "Time: " + DateTime.Now.ToLongTimeString() };

                                        System.IO.File.WriteAllLines(fullDir, strings);

                                        string HtmData = System.IO.File.ReadAllText(fullDir);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }

                                var newAccessToken = new AccessToken()
                                {
                                    CreatedDate = DateTime.Now,
                                    Data = DataToken1.data,
                                    IsActive = true,
                                    MessageDescription = DataToken1.message_description,
                                    Status = DataToken1.status,
                                    MessageCode = DataToken1.message_code,
                                    Timestamp = NormalKey.token_exp.ToString(),
                                    RefreshToken = NormalKey.access_token,
                                    UserId = User_Id,
                                    AesIv = NormalKey.aesIv,
                                    AesKey = NormalKey.aesKey,
                                };
                                lapoLoanDB.AccessTokens.Add(newAccessToken);
                                lapoLoanDB.SaveChanges();
                                return;
                            }

                            return;
                        }
                        else
                        {
                            try
                            {
                                // System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                                // var folderName = Path.Combine("AccessTokenLogs", "Documents");

                                string currentDirectory = Directory.GetCurrentDirectory();
                                string path = "HtmlPackages";

                                string fullDir = Path.Combine(currentDirectory, path, "AccessTokenLogs.txt");

                                if (TokenAccessResult != null && !string.IsNullOrEmpty(fullDir) && !string.IsNullOrWhiteSpace(fullDir) && System.IO.File.Exists(fullDir))
                                {
                                    var alltext = System.IO.File.ReadAllText(fullDir);

                                    string[] strings = { alltext, "_ _ __ __ __ __ __ _ _ __ ___", "_ ___ ___ __ __ __ __ __ __", "__ __ ____ ____ ___ __ __", "___ ____ ___ ____ __ ___", "__ __ ____ ___ __ ___ ___", "Error Access Token ","Error Message: " + TokenAccessResult.Message, "Error Message: " + TokenAccessResult.TryCatchMessage, "Created Date: " + DateTime.Now.ToString(), "Time: " + DateTime.Now.ToLongTimeString() };

                                    System.IO.File.WriteAllLines(fullDir, strings);

                                    string HtmData = System.IO.File.ReadAllText(fullDir);
                                    return;
                                }
                                return;
                            }
                            catch (Exception ex)
                            {
                                return;
                            }
                        }                 
                    }
                    else
                    {
                        if (AllAccessTokens != null)
                        {
                            //var CurrentDateTime = DateTime.Now;
                            //var TimeExpire = AllAccessTokens.CreatedDate.Value.AddMinutes(+10);

                            //if (CurrentDateTime > TimeExpire)
                            //{

                            RespondMessageDto TokenAccessResult = await new AccessTokenHttpClient(_configuration).GetBvnTokenClientAsync(AllAccessTokens.RefreshToken);

                            if (TokenAccessResult != null && TokenAccessResult.IsActive && TokenAccessResult.status == Status.Success)
                            {
                                var DataToken = TokenAccessResult.DataLoad as AccessTokenResponses;
                                var DataToken1 = TokenAccessResult.Data as AccessTokenResponses;

                                string Data = DataToken1.data.ToString();
                                byte[] DataByte = StringToByteArray(Data);
                                string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);

                                var aesKey = AllAccessTokens.AesKey;
                                var aesIv = AllAccessTokens.AesIv;

                                string DecryptData = NETCore.Encrypt.EncryptProvider.AESDecrypt(Database64String, aesKey, aesIv);

                                var getUserData = JObject.Parse(DecryptData);

                                string acc_Token = getUserData["access_token"].ToString();
                                aesKey = getUserData["aesKey"].ToString();
                                aesIv = getUserData["aesIv"].ToString();

                                var NormalKey = new AESRoot() { access_token = acc_Token, aesIv = aesIv, aesKey = aesKey };

                                if (DataToken != null && DataToken1 != null && NormalKey != null)
                                {
                                    try
                                    {
                                        // System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                                        // var folderName = Path.Combine("AccessTokenLogs", "Documents");

                                        string currentDirectory = Directory.GetCurrentDirectory();
                                        string path = "HtmlPackages";

                                        string fullDir = Path.Combine(currentDirectory, path, "AccessTokenLogs.txt");

                                        if (!string.IsNullOrEmpty(fullDir) && !string.IsNullOrWhiteSpace(fullDir) && System.IO.File.Exists(fullDir))
                                        {
                                            var alltext = System.IO.File.ReadAllText(fullDir);

                                            string[] strings = { alltext,    "___ ____ ___ ____ __ ___", "__ __ ____ ___ __ ___ ___", "New Access Token ", "Access Token: " + acc_Token, "Aes Key: " + aesKey, "AesIv: " + aesIv, "Created Date: " + DateTime.Now.ToString(), "Time: " + DateTime.Now.ToLongTimeString() };

                                            System.IO.File.WriteAllLines(fullDir, strings);

                                            string HtmData = System.IO.File.ReadAllText(fullDir);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    try
                                    {
                                        AllAccessTokens.CreatedDate = DateTime.Now;
                                        AllAccessTokens.IsActive = true;
                                        AllAccessTokens.MessageDescription = DataToken1.message_description;
                                        AllAccessTokens.Status = DataToken1.status;
                                        AllAccessTokens.MessageCode = DataToken1.message_code;
                                        AllAccessTokens.Timestamp = NormalKey.token_exp.ToString();
                                        AllAccessTokens.RefreshToken = NormalKey.access_token;
                                        AllAccessTokens.UserId = User_Id;
                                        AllAccessTokens.AesIv = NormalKey.aesIv;
                                        AllAccessTokens.AesKey = NormalKey.aesKey;
                                        lapoLoanDB.Entry(AllAccessTokens).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                        await lapoLoanDB.SaveChangesAsync();
                                        return;
                                    }
                                    catch (Exception ex)
                                    {
                                        return;
                                    }

                                    return;
                                }

                                return;
                            }
                            else
                            {
                                try
                                {
                                    // System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                                    // var folderName = Path.Combine("AccessTokenLogs", "Documents");

                                    string currentDirectory = Directory.GetCurrentDirectory();
                                    string path = "HtmlPackages";

                                    string fullDir = Path.Combine(currentDirectory, path, "AccessTokenLogs.txt");

                                    if (TokenAccessResult!=null && !string.IsNullOrEmpty(fullDir) && !string.IsNullOrWhiteSpace(fullDir) && System.IO.File.Exists(fullDir))
                                    {
                                        var alltext = System.IO.File.ReadAllText(fullDir);

                                        string[] strings = { alltext, "_ _ __ __ __ __ __ _ _ __ ___", "_ ___ ___ __ __ __ __ __ __", "__ __ ____ ____ ___ __ __", "___ ____ ___ ____ __ ___", "__ __ ____ ___ __ ___ ___", "Error Access Token ", "Error Message: " + TokenAccessResult.Message, "Error Message: " + TokenAccessResult.TryCatchMessage, "Created Date: " + DateTime.Now.ToString(), "Time: " + DateTime.Now.ToLongTimeString() };

                                        System.IO.File.WriteAllLines(fullDir, strings);

                                        string HtmData = System.IO.File.ReadAllText(fullDir);
                                        return;
                                    }
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    return;
                                }
                                return;
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (AllAccessTokens == null)
                        {
                            return;
                        }

                        AllAccessTokens.CreatedDate = DateTime.Now;
                        AllAccessTokens.IsActive = true;
                        AllAccessTokens.MessageDescription ="Success";
                        AllAccessTokens.Status = 1;
                        AllAccessTokens.MessageCode = "";
                        AllAccessTokens.Timestamp = "1800";
                        AllAccessTokens.RefreshToken = "3b562f064a4bb416e599fc67d2eb159ee9f8c173a631c1ad8dc8ece08a3af3c504db6991a15bdd9a934b7e4b072335532ab532687b0cd2e54dfbfc11dc41f014";
                        AllAccessTokens.UserId = User_Id;
                        AllAccessTokens.AesIv = ".3uya2^m?__j).yc";
                        AllAccessTokens.AesKey = "^!x5j.p@z5erwnk?c_o_(62oi!^6j@n1";
                        lapoLoanDB.Entry(AllAccessTokens).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();
                        return;
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }

                return;
            }
            catch (Exception epx)
            {
                return;
            }
        }

        private async void CallRegister()
        {
            try
            {
                var StaffData = "{\"EnterTeamMemberID\":\"SN0001\",\"EnterLastName\":\"Ojo\",\"EnterMiddleName\":\"Sunday\",\"EnterFirstName\":\"Olorunmo\",\"SelectHubGroupId\":\"MTQ=\",\"EnterPhoneNumber\":\"\",\"EnterEmailAddress\":\"\",\"CreatedByAccountId\":107,\"TeamMemberOfficeAddress\":\"40, Okporo Road, Opposite Ecobank, Rumuodara, Port-Harcourt, Rivers State\",\"UserType\":\"GROUP HEAD\",\"SelectHubGroupIdd\":\"\",\"AccessRightToEditTeamMemberPermissions\":true,\"AccessRightToViewDisbursementLoan\":true,\"AccessRightToViewUploadBackRepaymentLoan\":true,\"AccessRightToExportDISBURSEMENTLoan\":true,\"AccessRightToAnonymousLoanApplication\":true,\"AccessRightToUploadBackDISBURSEMENTLoan\":true,\"AccessRightToUploadBackRepaymentLoan\":true,\"AccessRightToPrintLoan\":true,\"AccessRightToProceedLoan\":false,\"ViewLoanNarration\":true,\"CreateLoanNarration\":true,\"AccessRighttodisablecustomerstoapplyforaloan\":true,\"AccessRighttoviewcustomers\":true,\"AccessRighttodisablehubs\":true,\"AccessRighttoviewtenure\":true,\"AccessRighttocreatetenure\":true,\"AccessRighttoloansettings\":true,\"AccessRighttoteamsAndpermissions\":true,\"AccessRighttorejectaloan\":true,\"AccessRighttoviewcustomersloans\":true,\"AccessRighttoapprovecustomerloan\":true,\"AccessRighttoviewveammembers\":true,\"AccessRighttocreateateammember\":true,\"AccessRighttoviewhubs\":true,\"AccessRighttocreateahub\":true,\"AccessRighttoviewloandetails\":true}";

                var json1 = JsonConvert.DeserializeObject<NewHubTeamMember>(StaffData);

                await new HubTeamsActivity(null, this._configuration).CreateHubMember1(null, json1);
            }
            catch (Exception epx)
            {

            }
        }
    }
}
