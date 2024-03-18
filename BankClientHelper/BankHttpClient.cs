using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.ModelDto;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Configuration;

namespace LapoLoanWebApi.BankClientHelper
{
    public class BankHttpClient
    {
        private string fmtString {get;set;}
        private  Uri BaseUrlUri;
        private HttpClient client;
        private LapoLoanWebApi.BankClientHelper.ResponseMsg.ResponseMsg msg;
        private string baseUrl = "https://api.paystack.co/bank/";
        private IConfiguration _configuration;
        public BankHttpClient(IConfiguration configuration)
        {
            this._configuration = configuration;
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

       
        private HttpClient GetClient()
        {
            return client;
        }

        private HttpClient GetClientWithBearer(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<RespondMessageDto> GetBankNameAsync(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, BankAcctNameModel bankAcct)
        {
            try
            {

                string apiKey = new DefalutToken(_configuration).BankApiToken();

                var client = GetClient();
                string token = apiKey;
                string MethdNm = "resolve";

                var accname = bankAcct.AcctNo;
                /*** reconstruct the uri by passing the Account_Search vallue to the end of the url***/
                 baseUrl = string.Concat(baseUrl, MethdNm);

                var builder = new UriBuilder(baseUrl);
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["account_number"] = bankAcct.AcctNo.ToString();
                query["bank_code"] = bankAcct.Selected;
                builder.Query = query.ToString();
                baseUrl = builder.ToString();

                /***create a new construct and reinitialize the CallApi() class***/
                this.BaseUrlUri = new Uri(baseUrl);
                this.client.BaseAddress = this.BaseUrlUri;

                /***Pass the api key to the predefined method within the CallApi() class***/
                client = GetClientWithBearer(token);

                /***Make the actual api call and create an HttpResponseMessage variable to recieve the response***/
                HttpResponseMessage response = await client.GetAsync(baseUrl);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = this.BaseUrlUri,
                    Content = new StringContent("", Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
                };

                response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    /***Read the response as a string and pass to a local variable***/
                    string getRes = await response.Content.ReadAsStringAsync();

                    /***In order to read the returned custome status value, first convert the string to JObject***/
                    var geSrchBvnObj = JObject.Parse(getRes);

                    /***then create a bool variable to access the value of the 'status' item/property or JToken as a bool***/
                    var LbSrchStat = (bool)geSrchBvnObj["status"];
                    string LbSrch_Ac_Nm = geSrchBvnObj["data"]["account_name"].ToString();

                    /***check if the status boolean value is true***/
                    if (LbSrchStat == true)
                    {

                        if (string.IsNullOrWhiteSpace(LbSrch_Ac_Nm) ||  string.IsNullOrEmpty(LbSrch_Ac_Nm) || LbSrch_Ac_Nm.Equals("Not found"))
                        {
                            msg = new LapoLoanWebApi.BankClientHelper.ResponseMsg.ResponseMsg();
                            msg.Status = false;
                            msg.Message = "Account number resolved but locally reviewed";
                            msg.Data = null;

                            fmtString = JsonConvert.SerializeObject(msg);
                        }
                        else
                        {
                            /***If the 'status' value is true, it means the Account_Search search with paystack api successfully returned the Account_Search details,
                             hence pass the entire string result to be returned to the frontend form.***/
                            fmtString = LbSrch_Ac_Nm;

                            /***To extract and pass the Account_Search details of the api response to our local db, 
                             we had to directly access the 'data' item/property or JToken as string***/
                            string LbSrchData = geSrchBvnObj["data"].ToString();

                            /***then pass the data model variable to the method that saves the Account_Search details to our local db***/
                            // Pst.PostCustAccnt(AcMdl);

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Account number resolved but locally reviewed", true, fmtString, fmtString, Status.Success, StatusMgs.Success, true);
                        }
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Account number resolved but locally reviewed", false, getRes, getRes, Status.Success, StatusMgs.Success, false);
                }
                else
                {
                    /***if the httpclient response status code is however not successful,
                     read the error custome response from paystack as string***/
                    var getRes = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(getRes.ToString()) || string.IsNullOrWhiteSpace(getRes.ToString()))
                    {
                        /***Since the response is empty, throw below exception so it will be handled by the global filter***/
                        // throw new HttpException();
                    }

                    else
                    {
                        /***Pass the string result to be returned to the frontend form.***/
                        fmtString = "error";
                    }


                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Account number resolved but locally reviewed", false, getRes, getRes, Status.Success, StatusMgs.Success, false);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Account number resolved but locally reviewed", false, "Account number resolved but locally reviewed", "Account number resolved but locally reviewed", Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message  + ",  Account number resolved but locally reviewed", false, ex,  null,  Status.Ërror,  StatusMgs.Error);
            }
        }
    }
}
