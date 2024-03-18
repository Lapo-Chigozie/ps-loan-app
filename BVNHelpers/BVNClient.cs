using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Http.HttpResults;
using NETCore.Encrypt;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace LapoLoanWebApi.BVNHelpers
{
    public class BVNClientHelper
    {
        private LapoLoanDBContext lapoLoanDB;
        private IConfiguration configuration;
        public BVNClientHelper(IConfiguration _configuration
)
        { 
           this.lapoLoanDB = new LapoLoanDBContext(_configuration);
            _configuration = _configuration;
        }

        private  byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public  async Task<RespondMessageDto> SendBvnClientAsync(string BvnRequest)
        {
            try
            {
                string User_Id = new  DefalutToken(configuration).User_Id();

                var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (AllAccessTokens != null && DefalutToken.IsLocal == false)
                {
                    try
                    {
                        BvnClas verBvn = new BvnClas()
                        {
                            BVN = BvnRequest
                        };

                        var sendemail = JsonConvert.SerializeObject(verBvn);
                        string verBvnEnc = EncryptProvider.AESEncrypt(sendemail, AllAccessTokens.AesKey, AllAccessTokens.AesIv);
                        byte[] inbyte = Convert.FromBase64String(verBvnEnc);
                        string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                        string Url = new DefalutToken(configuration).BasedUrl();

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

                          FileLogActivities.CallSevice("New PS-Loan ", "Saving new loan application ", DataResult.Data.ToString());

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, bvnDetails, bvnDetails, Status.Success, StatusMgs.Success, true);
                        }
                        else
                        {
                            FileLogActivities.CallSevice("New PS-Loan ", "Saving new loan application ", DataResult.Data.ToString());

                        }
                    }
                    catch (Exception ex)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Failed, StatusMgs.Failed);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Check network connection and try again", false, null, null, Status.Failed, StatusMgs.Failed);
                }

                //else
                //{
                //    string defaultData = "{\r\n\"ResponseCode\": \"00\",\r\n\"BVN\": \"22222222226\",\r\n\"FirstName\": \"MOHAMMAD\",\r\n\"MiddleName\": \"A\",\r\n\"LastName\": \"Chidebelu Eze\",\r\n\"DateOfBirth\": \"14-May-1908\",\r\n\"RegistrationDate\": \"11-JAN-15\",\r\n\"EnrollmentBank\": \"035\",\r\n\"EnrollmentBranch\": \"Lekki\",\r\n\"PhoneNumber1\": \"07069302232\",\r\n\"WatchListed\": \"NO\"\r\n} ";

                //    var bvnDetails = JsonConvert.DeserializeObject<BvnRespondsDto>(defaultData);

                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, bvnDetails, bvnDetails, Status.Success, StatusMgs.Success, true);
                //}

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, "Check network connection and try again", false, null, null, Status.Failed, StatusMgs.Failed);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Failed, StatusMgs.Failed);
            }
        }
    }
}
