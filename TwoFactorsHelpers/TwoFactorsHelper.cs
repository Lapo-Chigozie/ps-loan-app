using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.ModelDto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;

namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class TwoFactorsHelper
    {
        private LapoLoanDBContext lapoLoanDB = null;
        private string Url = "http://10.0.0.163:5112";

        private IConfiguration _configuration;

        public TwoFactorsHelper(IConfiguration _configuration
)
        {
            this._configuration = _configuration;
            Url = "http://10.0.0.94:3000/send_sms";
            lapoLoanDB= new LapoLoanDBContext( _configuration
);
        }

        public async Task<RespondMessageDto> SendTwoFactorLoginsmsAsync(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string message, long AcctId)
        {
            //"phoneNumber: "07052177326",
            //"accountNumber": "0000000000",
            //"content": "testing"
            string AccountNumber = "0000000000";
            var newsms = new SmsDto();

            var User  = await lapoLoanDB.People.Where(x=> x.AccountId == AcctId).FirstOrDefaultAsync();
            if (User != null && !string.IsNullOrEmpty(User.PhoneNumber) && !string.IsNullOrWhiteSpace(User.PhoneNumber))
            {
                newsms.phoneNumber = User.PhoneNumber;

                if (newsms.phoneNumber.StartsWith("+") || newsms.phoneNumber.StartsWith("+234"))
                {
                    newsms.phoneNumber = newsms.phoneNumber.Replace("+", "");
                }

                if (newsms.phoneNumber.StartsWith("0"))
                {
                    newsms.phoneNumber = "234" + newsms.phoneNumber.Substring(1);
                }
            }
            else
            {
                ////newsms.phoneNumber = "2349134704265";
            }

            //if (DefalutToken.UseDevEmailAndPhoneNumber)
            //{
            //    newsms.phoneNumber = "+2349134704265";
            //}

            var code = await lapoLoanDB.AcctLoginVerifications.Where(x => x.AccountId == AcctId && x.IsActive).FirstOrDefaultAsync();
            if (code != null)
            {
                message += " " + code.Code;
                
            }

            var SecurityAcct = await lapoLoanDB.SecurityAccounts.Where(x => x.PersonId == User.Id).FirstOrDefaultAsync();
            if (SecurityAcct != null && code != null)
            {
                var Result1 =  await new EmailHelpers(this._configuration).SendLoginAuth(_environment, AcctId.ToString(), User.EmailAddress ?? SecurityAcct.Username , code.Code);

                if (DefalutToken.IsLocal)
                {
                    return Result1;
                }
            }

            // newsms.phoneNumber = "2349134704265";
            newsms.content = message;
            newsms.account = AccountNumber;

            try
            {
                var client = new HttpClient();
                Url = "http://10.0.0.163:5112";
                client.BaseAddress = new Uri(Url);

                var content = new FormUrlEncodedContent(new[]
                {
                             new KeyValuePair<string, string>("account",  newsms.account),
                             new KeyValuePair<string, string>("phoneNumber", newsms.phoneNumber),
                              new KeyValuePair<string, string>("content",   newsms.content),
                });

                var response = await client.PostAsync("/send_sms", content);

              
                var json = JsonConvert.SerializeObject(newsms);

              
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Url + "/send_sms"),
                    Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
                };

                 response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                {
                    string resultContent = await response.Content.ReadAsStringAsync();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, resultContent, resultContent, Status.Success, StatusMgs.Success, true);
                }
                else
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    if (DefalutToken.IsLocal)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, resultContent, resultContent, Status.Success, StatusMgs.Success, true);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Message was not successful", false, resultContent, resultContent, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Message was not successful", false, "Message was not successful", "Message was not successful", Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SendBVNsmsAsync(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string PhoneNumber, string message, long AcctId)
        {
            //"phoneNumber: "07052177326",
            //"accountNumber": "0000000000",
            //"content": "testing"
            string AccountNumber = "0000000000";
            var newsms = new SmsDto();

           // newsms.phoneNumber = PhoneNumber;

            if (PhoneNumber != null && !string.IsNullOrEmpty(PhoneNumber) && !string.IsNullOrWhiteSpace(PhoneNumber))
            {
                //  newsms.phoneNumber = PhoneNumber;

                if (PhoneNumber.StartsWith("+") || PhoneNumber.StartsWith("+234"))
                {
                    PhoneNumber = PhoneNumber.Replace("+","");
                }

                if (PhoneNumber.StartsWith("0"))
                {
                    PhoneNumber = "234" + PhoneNumber.Substring(1);
                }
            }
            else
            {
                // newsms.phoneNumber = "2 349 134 704 265" "2 348 143 810 341";
            }

            newsms.phoneNumber = PhoneNumber;

          //if (DefalutToken.UseDevEmailAndPhoneNumber)
          //{
          //    newsms.phoneNumber = "+2349134704265";
          //}

          var code = await lapoLoanDB.Bvnverifications.Where(x => x.AccountRequestId == AcctId && x.IsActive.Value).FirstOrDefaultAsync();
            if (code != null)
            {
               // message += code.Code;
            }

            var SecurityAcct = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == AcctId).FirstOrDefaultAsync();
            if (SecurityAcct != null && code != null)
            {
                //SendLoginAuth
                var Result1 =  await new EmailHelpers(this._configuration).SendBvnAuth(_environment, AcctId.ToString(), SecurityAcct.Username, code.Code);

                if (DefalutToken.IsLocal)
                {
                    return Result1;
                }
            }

            //  newsms.phoneNumber = "2349134704265";

            newsms.content = message;
            newsms.account = AccountNumber;

            string smsurl = "http://10.0.0.163:5112";

            try
            {
                var client = new HttpClient();

                client.BaseAddress = new Uri(smsurl);
               
                var content = new FormUrlEncodedContent(new[]
                {
                             new KeyValuePair<string, string>("account",  newsms.account),
                             new KeyValuePair<string, string>("phoneNumber", newsms.phoneNumber),
                              new KeyValuePair<string, string>("content",   newsms.content),

                });

                var response = await client.PostAsync("/send_sms", content);


                var json = JsonConvert.SerializeObject(newsms);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(smsurl + "/send_sms"),
                    Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
                };

                 var response1 = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                {
                    string resultContent = await response.Content.ReadAsStringAsync();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, newsms, newsms, Status.Success, StatusMgs.Success, true);
                }
                else
                {
                    string resultContent = await response.Content.ReadAsStringAsync();
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, newsms, newsms, Status.Success, StatusMgs.Success, true);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Message was not successful", false, "Message was not successful", "Message was not successful", Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AdminSendLoanSmsAsync(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string message, long AcctId)
        {
            //"phoneNumber: "07052177326",
            //"accountNumber": "0000000000",
            //"content": "testing"
            string AccountNumber = "0000000000";
            var newsms = new SmsDto();

            var User = await lapoLoanDB.People.Where(x => x.AccountId == AcctId).FirstOrDefaultAsync();
            if (User != null && !string.IsNullOrEmpty(User.PhoneNumber) && !string.IsNullOrWhiteSpace(User.PhoneNumber))
            {
                newsms.phoneNumber = User.PhoneNumber;

                if (newsms.phoneNumber.StartsWith("+") || newsms.phoneNumber.StartsWith("+234"))
                {
                    newsms.phoneNumber = newsms.phoneNumber.Replace("+", "");
                }

                if (newsms.phoneNumber.StartsWith("0"))
                {
                    newsms.phoneNumber = "234" + newsms.phoneNumber.Substring(1);
                }
            }
            else
            {
           ///     newsms.phoneNumber = "2349134704265";
            }

            //if (DefalutToken.UseDevEmailAndPhoneNumber)
            //{
            //    newsms.phoneNumber = "+2349134704265";
            //}

            newsms.content = message;
            newsms.account = AccountNumber;

            try
            {
                var client = new HttpClient();
                Url = "http://10.0.0.163:5112";
                client.BaseAddress = new Uri(Url);

                var content = new FormUrlEncodedContent(new[]
                {
                             new KeyValuePair<string, string>("account",  newsms.account),
                             new KeyValuePair<string, string>("phoneNumber", newsms.phoneNumber),
                              new KeyValuePair<string, string>("content",   newsms.content),
                });

                var response = await client.PostAsync("/send_sms", content);

                var json = JsonConvert.SerializeObject(newsms);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Url + "/send_sms"),
                    Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
                };

                response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, resultContent, resultContent, Status.Success, StatusMgs.Success, true);
                }
                else
                {

                    string resultContent = await response.Content.ReadAsStringAsync();

                    if (DefalutToken.IsLocal)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, resultContent, resultContent, Status.Success, StatusMgs.Success, true);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Message was not successful", false, resultContent, resultContent, Status.Ërror, StatusMgs.Error);

                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Message was not successful", false, "Message was not successful", "Message was not successful", Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SendSmsAsync(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment,string Phone, string Message, long AcctId)
        {
            //"phoneNumber: "07052177326",
            //"accountNumber": "0000000000",
            //"content": "testing"
            string AccountNumber = "0000000000";
            var newsms = new SmsDto();

          
            if (Phone != null && !string.IsNullOrEmpty(Phone) && !string.IsNullOrWhiteSpace(Phone))
            {
                newsms.phoneNumber = Phone;

                if (newsms.phoneNumber.StartsWith("+") || newsms.phoneNumber.StartsWith("+234"))
                {
                    newsms.phoneNumber = newsms.phoneNumber.Replace("+", "");
                }

                if (newsms.phoneNumber.StartsWith("0"))
                {
                    newsms.phoneNumber = "234" + newsms.phoneNumber.Substring(1);
                }
            }
            else
            {
                ///     newsms.phoneNumber = "2349134704265";
            }

            //if (DefalutToken.UseDevEmailAndPhoneNumber)
            //{
            //    newsms.phoneNumber = "+2349134704265";
            //}

            newsms.phoneNumber = newsms.phoneNumber;
            newsms.content = Message;
            newsms.account = AccountNumber;

            try
            {
                var client = new HttpClient();
                Url = "http://10.0.0.163:5112";
                client.BaseAddress = new Uri(Url);

                var content = new FormUrlEncodedContent(new[]
                {
                             new KeyValuePair<string, string>("account",  newsms.account),
                             new KeyValuePair<string, string>("phoneNumber", newsms.phoneNumber),
                              new KeyValuePair<string, string>("content",   newsms.content),
                });

                var response = await client.PostAsync("/send_sms", content);


                var json = JsonConvert.SerializeObject(newsms);


                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Url + "/send_sms"),
                    Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
                };

                 response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, resultContent, resultContent, Status.Success, StatusMgs.Success, true);
                }
                else
                {

                    string resultContent = await response.Content.ReadAsStringAsync();

                    if (DefalutToken.IsLocal)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Message was successful", true, resultContent, resultContent, Status.Success, StatusMgs.Success, true);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Message was not successful", false, resultContent, resultContent, Status.Ërror, StatusMgs.Error);

                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Message was not successful", false, "Message was not successful", "Message was not successful", Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }
    }

    public class SmsDto
    {
        public string account { get; set; }
        public string phoneNumber { get; set; }
      
        public string content { get; set; }
    }
}
