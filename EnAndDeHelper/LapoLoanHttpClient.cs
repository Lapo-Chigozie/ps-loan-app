using LapoLoanWebApi.ModelDto;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text;
using Org.BouncyCastle.Utilities.Encoders;

namespace LapoLoanWebApi.EnAndDeHelper
{
    public class LapoLoanHttpClient
    {
        private  static HttpClient _httpClient = null;
        public static async Task<RespondMessageDto> SentAsync(string url , string Token, string data, System.Net.Http.HttpMethod httpMethod)
        {
            try
            {
                 _httpClient = new HttpClient();
                //  _httpClient.BaseAddress = new Uri("http://10.0.0.184:8023");
                // _httpClient.DefaultRequestHeaders.Accept.Clear();
                //_httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");

                //_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("x-lapo-eve-proc", Token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", Token);

                //string errorMessage = "User has no enough permissions to perform requested operation.";

                //var json = "{ \"some\": \"json\"}";

                //if (data != null )
                //{
                //     json = JsonConvert.SerializeObject(data);
                //}

                //var request = new HttpRequestMessage
                //{
                //    Method = httpMethod,
                //    RequestUri = new Uri(url),
                //    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"/*System.Net.Mime.MediaTypeNames.Application.Json*/ )
                //};

                //var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair<string, string>("xPayload", data.ToString())
                //});

                //List<KeyValuePair<string, string>> values = new()
                //{
                //    new KeyValuePair<string, string>("xPayload", data.ToString())
                //};

                //FormUrlEncodedContent requestContent = new(values);

                xPayLd xPay = new xPayLd()
                {
                    xPayload = data
                };

                var xPayx = JsonConvert.SerializeObject(xPay);

                var stringContent = new StringContent(xPayx, UnicodeEncoding.UTF8, "application/json");

                var request = new HttpRequestMessage
                {
                    Method = httpMethod,
                    RequestUri = new Uri(url),
                    Content = stringContent
                };

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    FileLogActivities.CallSevice("BVN CLASS", "BVN Verification Respond Success Message", resultContent);

                    var ResultJson = JsonConvert.DeserializeObject<AccessTokenResponses>(resultContent);

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Token access was successful", true, ResultJson, ResultJson, Status.Success, StatusMgs.Success, true);
                }
                else
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    FileLogActivities.CallSevice("BVN CLASS", "BVN Verification Respond Error Message", resultContent);

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "An error has occur, check internet connection and try again.", false, resultContent, resultContent, Status.Ërror, StatusMgs.Error);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "An error has occur, check internet connection and try again.", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice("BVN CLASS", "BVN Verification Respond Error Message: " + ex.Message ?? ex.InnerException.Message, ex.Message ?? ex.InnerException.Message);

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
    }
}
