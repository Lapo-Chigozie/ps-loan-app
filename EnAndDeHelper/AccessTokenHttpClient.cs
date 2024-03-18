using LapoLoanWebApi.ModelDto;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace LapoLoanWebApi.EnAndDeHelper
{
    public class AccessTokenHttpClient
    {
        private IConfiguration configuration;
        public AccessTokenHttpClient(IConfiguration _configuration) {
            configuration = _configuration;
        }

        public  async Task<RespondMessageDto> GetBvnTokenClientAsync(string RefreshToken = "")
        {
            try
            {
                string User_Id = new  DefalutToken(configuration).User_Id();
                string AesKey = new DefalutToken(configuration).AesKey();
                string AesIv = new DefalutToken(configuration).AesIv();
                string Access_token = new DefalutToken(configuration).Access_token();

                if (RefreshToken != null && !string.IsNullOrEmpty(RefreshToken) && !string.IsNullOrWhiteSpace(RefreshToken))
                {
                    Access_token = RefreshToken;
                }

                string Token_exp = "259200";

                string CombinationTokenAndUserId = string.Concat( Access_token , User_Id);

                string UserKeyUrl = new DefalutToken(configuration).TokenBasedUrl() + "/generateuserkey";

                var _httpClient = new HttpClient();

                _httpClient.BaseAddress = new Uri(new DefalutToken(configuration).TokenBasedUrl());
                _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                _httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", CombinationTokenAndUserId);

                var json = JsonConvert.SerializeObject(null);

                var request = new HttpRequestMessage
                {
                    Method = System.Net.Http.HttpMethod.Get,
                    RequestUri = new Uri(UserKeyUrl),
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"/*System.Net.Mime.MediaTypeNames.Application.Json*/ /* or "application/json" in older versions */),
                };

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.IsSuccessStatusCode)
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    var ResultJson = JsonConvert.DeserializeObject<AccessTokenResponses>(resultContent);

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, ResultJson.message_description + ",  code: " + ResultJson.message_code, true, ResultJson, ResultJson, Status.Success, StatusMgs.Success, true);
                }
                else
                {
                    string resultContent = await response.Content.ReadAsStringAsync();

                    var ResultJson = JsonConvert.DeserializeObject<AccessTokenResponses>(resultContent);

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Failed, ResultJson.message_description + ",  code: " + ResultJson.message_code, false, resultContent, resultContent, Status.Failed, StatusMgs.Failed, false);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "An error has occur, check internet connection and try again.", false, null, null, Status.Ërror, StatusMgs.Error);
            }
            catch(Exception ex)
            {

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);

            }
        }
    }
}
