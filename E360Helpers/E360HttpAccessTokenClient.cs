using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;

namespace LapoLoanWebApi.E360Helpers
{
    public class E360HttpAccessTokenClient
    {
        // private string Url = "http://10.0.0.184:8015/03a3b2c6f7d8e1c4_0a";

        //private string Url = DefalutToken.AccessTokenClient ? "http://10.0.0.143:8015/03a3b2c6f7d8e1c4_0a" : "http://10.0.0.184:8015/03a3b2c6f7d8e1c4_0a";

      

        private string token = string.Empty;
        private ControllerBase controllerBase { get; set; }
        private IConfiguration configuration { get; set; }
        public E360HttpAccessTokenClient(ControllerBase controllerBase, IConfiguration _configuration) 
        {
            this.controllerBase = controllerBase;
            this.configuration = _configuration;
        }

        public async Task<string> TokenAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(new DefalutToken(configuration).AccessTokenClient()))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            token = response.Headers.GetValues("x-lapo-eve-proc").FirstOrDefault();
                            return token;
                        }
                        else
                        {
                            return token;
                        }
                    }
                }

                return token;
            }
            catch(Exception ex)
            {
                return token;
            }
        }
    }
}
