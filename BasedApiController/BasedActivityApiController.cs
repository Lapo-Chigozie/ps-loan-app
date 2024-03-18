using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using Microsoft.AspNetCore.Mvc;

namespace LapoLoanWebApi.BasedApiController
{
    public class BasedActivityApiController : ControllerBase
    {
        public Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment = null;
        public E360AuthHttpValidation e360AuthHttp = null;
        public LapoLoanDBContext lapoLoanDB = null;
        public IConfiguration Configuration = null;

        public BasedActivityApiController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IConfiguration _configuration)
        {
            this.Configuration = _configuration;
            this.Environment = _environment;
            this.e360AuthHttp = new E360AuthHttpValidation(this, this.Configuration);
        
            this.lapoLoanDB = new LapoLoanDBContext(this.Configuration);
        }
    }
}
