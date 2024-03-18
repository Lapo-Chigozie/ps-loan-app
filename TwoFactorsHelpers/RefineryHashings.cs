using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class RefineryHashings
    {
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private ControllerBase ControllerBase { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }
        public RefineryHashings(Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment, ControllerBase controllerBase)
        {
            this.ControllerBase = controllerBase;
            this.Environment = Environment;
            this.lapoCipher01 = new LapoCipher01();
        }

        public async Task<RespondMessageDto> GetDecriptRefinery(AppRefineryHash appRefinery)
        {
            try
            {
                var HashData = this.lapoCipher01.DecryptString(appRefinery.Hash);

                var Data = JsonConvert.DeserializeObject<object>(HashData);

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, ".", true, Data, Data, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetEncriptRefinery(AppRefineryHash appRefinery)
        {
            try
            {

                // var data2 = "ValueKind = Object : \"";

                var data3 = Convert.ToString(appRefinery.Data);

                var Data = JsonConvert.SerializeObject(data3);

                var HashData = this.lapoCipher01.EnryptString(Data);

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, ".", true, HashData, HashData, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
    }
}
