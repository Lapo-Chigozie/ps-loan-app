using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.E360Helpers.E360DtoModel;
using LapoLoanWebApi.EnAndDeHelper;

namespace LapoLoanWebApi.E360Helpers
{
    public class E360AuthHttpClient
    {
        private E360HttpAccessTokenClient AccessTokenClient { get; set; }
        private IConfiguration configuration;

        //private string E360ApiUrl = DefalutToken.E360ApiUrl;

        
        private ControllerBase controllerBase { get; set; }
        public E360AuthHttpClient(ControllerBase controllerBase, IConfiguration _configuration) 
        {
             configuration = _configuration;
            this.controllerBase = controllerBase;
            this.AccessTokenClient = new E360HttpAccessTokenClient(controllerBase, configuration);
        }

        public async Task<RespondMessageDto> SignInToE360([FromBody] AcctSignInModel acctSignIn)
        {
            try
            {
                string Ttoken = await this.AccessTokenClient.TokenAsync();

                if(string.IsNullOrEmpty(Ttoken) || string.IsNullOrEmpty(Ttoken))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid access token", false, null, null, Status.Ërror, StatusMgs.Error);
                }

                string[] HeaderResponse = Ttoken.Split(new Char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

                string token = HeaderResponse[0];
                string aesKey = HeaderResponse[1];
                string IV = HeaderResponse[2];

                //byte[] byteaes = Encoding.ASCII.GetBytes(aesKey);
                //byte[] ByteIV = Encoding.ASCII.GetBytes(IV);

                var middlekey = JsonConvert.SerializeObject(new
                {
                    tk = token,
                    src = "AS-IN-D659B-e3M"
                });

                string encrypted = EncryptProvider.AESEncrypt(middlekey, aesKey, IV);
                byte[] inbyteto = Convert.FromBase64String(encrypted);
                string hexsto = BitConverter.ToString(inbyteto).Replace("-", "").ToLower();

                string retrnString = string.Empty;
                var logn = new E360AuthLoginDto()
                {
                    UsN = acctSignIn.EmailAddress,
                    Pwd = acctSignIn.Password,
                    xAppSource = "AS-IN-D659B-e3M"
                };

                //string encrytionToken = Encrypt(token, byteaes, ByteIV);
                //string encrypted = EncryptProvider.AESEncrypt(token, aesKey, IV);

                var lognx = JsonConvert.SerializeObject(logn);

                string LoginEnc = EncryptProvider.AESEncrypt(lognx, aesKey, IV);
                byte[] inbyte = Convert.FromBase64String(LoginEnc);
                string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();
                var stringContent = new StringContent(hexs, UnicodeEncoding.UTF8, "application/json");

                //var stringContent = new StringContent(lognx, UnicodeEncoding.UTF8, "application/json");

                //string LoginEnc = EncryptProvider.AESEncrypt(lognx, aesKey, IV);
                //string encrytSerial = Encrypt(lognx, byteaes, ByteIV);
                //var stringContent = new StringContent(encrytSerial, UnicodeEncoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", string.Concat(hexsto, token));

                    using (var response = await httpClient.PostAsync(new DefalutToken(configuration). E360ApiUrl(), stringContent))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string getRes = await response.Content.ReadAsStringAsync();

                            /***In order to read the returned custome status value, first convert the string to JObject***/
                            var geSrchBvnObj = JObject.Parse(getRes);

                            /***then create a bool variable to access the value of the 'status' item/property or JToken as a bool***/
                            bool Status1 = (Boolean)geSrchBvnObj["status"];
                            string Message = (string)geSrchBvnObj["message_description"].ToString();
                            string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                            // if the status code is true redirect to either change passwor or dashbord
                            if (Status1 == true)
                            {
                                //decrypt the Data Object 
                                //  var geSrchBvnObj = JObject.Parse(getRes);
                                string Data = geSrchBvnObj["data"].ToString();
                                byte[] DataByte = StringToByteArray(Data);
                                string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                                string DecryptData = EncryptProvider.AESDecrypt(Database64String, aesKey, IV);

                                //get the relationship
                                var getUserData = JObject.Parse(DecryptData);
                                string UserID = getUserData["Employee_No"].ToString();
                                string UserFirstName = getUserData["FirstName"].ToString();
                                string UserLastName = getUserData["LastName"].ToString();
                                string UserRef = getUserData["UserRef"].ToString();
                                string UserRole = getUserData["uRole"].ToString();

                                string Pri_Email_Address = getUserData["Pri_Email_Address"].ToString();
                                string JF_Name = getUserData["JF_Name"].ToString();

                                var ResponedUserId = new E360AuthLoginRespondsDto()
                                {
                                    UserID = UserID,
                                    UserFirstName = UserFirstName,
                                    UserLastName = UserLastName,
                                    UserRef = UserRef,
                                    UserRole = UserRole,
                                    StaffID = acctSignIn.EmailAddress,
                                    Role = JF_Name,
                                    Email = Pri_Email_Address
                                };

                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Sign-In was successful", true, ResponedUserId, ResponedUserId, Status.Success, StatusMgs.Success, true);
                            }
                            else
                            {
                                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, Message, false, null, null, Status.Ërror, StatusMgs.Error);
                            }
                        }
                        else
                        {
                            string getRes = await response.Content.ReadAsStringAsync();

                            /***In order to read the returned custome status value, first convert the string to JObject***/
                            var geSrchBvnObj = JObject.Parse(getRes);

                            /***then create a bool variable to access the value of the 'status' item/property or JToken as a bool***/
                            bool Status1 = (Boolean)geSrchBvnObj["status"];
                            string Message = (string)geSrchBvnObj["message_description"].ToString();
                            string MessageCode = (string)geSrchBvnObj["message_code"].ToString();

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, Message /*String.Format("Hello{0}.\\ncurrent Date and time:{1}{2}", "Invalid UserID or Password", DateTime.Now.ToString(), getRes)*/, false, null, null, Status.Ërror, StatusMgs.Error);
                        }
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid UserID or Password", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
