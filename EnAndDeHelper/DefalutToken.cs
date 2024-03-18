using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Newtonsoft.Json;

using System.Net;
using System.Web.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Configuration;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using LapoLoanWebApi.HubTeams;
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.E360Helpers.E360DtoModel;
using LapoLoanWebApi.E360Helpers;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;

namespace LapoLoanWebApi.EnAndDeHelper
{
    public class DefalutToken
    {
        private readonly  IConfiguration Config ;
        public DefalutToken(IConfiguration _config) 
        {
            Config = _config;
        }
       
        public string User_Id()
        {
            return Config.GetValue<string>("User_Id");
        }

        public  string AesKey()
        {
            return Config.GetValue<string>("AesKey");
        }

        public  string AesIv()
        {
            return Config.GetValue<string>("AesIv");
        }

        public  string Access_token()
        {
            return Config.GetValue<string>("Access_token");
        }

        public  string CheckInternetConnection()
        {
            return Config.GetValue<string>("CheckInternetConnection");
        }

        public  string RetriveBankAccount()
        {
          return  Config.GetValue<string>("RetriveBankAccount");
        }

        public  string AccountdetailsNo()
        {
            return Config.GetValue<string>("AccountdetailsNo");
        }

        public  string E360TokenApiUrl()
        {
            return Config.GetValue<string>("E360TokenApiUrl");
        }

        public  string E360UserDivision()
        {
            return Config.GetValue<string>("E360UserDivision");
        }

        public  string E360searchemployees()
        {
            return Config.GetValue<string>("E360searchemployees");
        }


        public string E360ApiUrl() {
            return Config.GetValue<string>("E360ApiUrl");
        }

      
        public  string ApiServer()
        {
            return Config.GetValue<string>("ApiServer");
        }

        // "http://192.168.1.28:8037" http://192.168.1.28:8037 "http://192.168.1.28:8036"

        public  string AngularHost()
        {
            return Config.GetValue<string>("AngularHost");
        }
        
        public  string ClientLogin()
        {
            return AngularHost() + "/signin?IsLoanApp=false";
        }
      
        public static string[] AllowOrignUrl = { "http://localhost:4200"
        , "http://localhost:65379", "https://ps.lapo-nigeria.org" , "https://localhost:7741", "http://localhost:7741" , "http://localhost:55990", "http://10.0.1.4:8080" , "https://10.0.1.4:8080", "https://localhost:4000" , "https://ps-services.lapo-nigeria.org", "http://ps-services.lapo-nigeria.org", "http://10.0.0.131:8015" };

        public static string[] OrignMethods = { /*"OPTIONS",*/ "GET", "POST"  /*, "PUT"*/, /* "PATCH", "DELETE"*/ };


        public  string BankApiToken()
        {
            return Config.GetValue<string>("BankApiToken");
        }

        public  string BasedUrl()
        {
            return Config.GetValue<string>("BasedUrl");
        }

        public  string TokenBasedUrl()
        {
            return Config.GetValue<string>("TokenBasedUrl");
        }

        public  string EmailBasedUrl()
        {
            return Config.GetValue<string>("EmailBasedUrl");
        }



        public  string AccessTokenClient()
        {
            return Config.GetValue<string>("AccessTokenClient");
        }



        public const bool IsLocal = false;   // False is to send email

        // public const int DefaultAcct  = 46;

        public const bool UseDevEmailAndPhoneNumber = false ;  // true  to use dev email

        public static string[] ImageFormats = new string[] { ".png",  ".jpeg", ".jpg" };

        public static string[] SlipImageFormats = new string[] { ".png",   ".gif", ".jpeg", ".jpg" , ".pdf" };
         
        public static string[] ExcelFormats = new string[] { ".xlsx", ".xltx", ".xlsb", ".xlsm" };

        public  string AccountVerifyLink(string identification)
        {
            var urllink = AngularHost() + "/appbvnregister?identification=" + identification.ToString();
            return urllink;
        }

        public  string ResetPasswordUrl(string Username) 
        {
            var urllink = AngularHost() + "/forgetpwrd2?Username=" + Username;
            return urllink;
        }

        public static string MakeImageSrcData(string filename)
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "Resources\\LoanAppImages";

                var fileNameOnly = Path.GetFileName(filename);

                string fullPath = Path.Combine(currentDirectory, path, fileNameOnly);

                byte[] filebytes = System.IO.File.ReadAllBytes(fullPath);

                var urldata = "data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);

                //  return "<%=" + urldata + " %>";

                return urldata;
            }
            catch(Exception xx)
            {
                try
                {

                    byte[] filebytes = System.IO.File.ReadAllBytes("https://www.lapo-nigeria.org/img/logo/LAPO_Logo_2022.png");

                    var urldata = "data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);

                    return urldata;
                }
                catch (Exception exx)
                {
                    return "";
                }
            }
        }


        public const string PublicSectorOfficeEmail = "lapo@gmail.com";
        public const string PublicSectorOfficePhone = "09143704265";

        public const bool IsProduction = true;               //  true IsProduction
        public static ConType ConType = ConType.LiveProd;   //   Test Prod

        public static string GetConnectionString(IConfiguration _configuration, ConType ConType = ConType.None)
        {
            try
            {
                switch (ConType)
                {
                    case ConType.None:
                        return _configuration.GetConnectionString("LocalConnection");
                        break;
                    case ConType.Unknown:
                        return _configuration.GetConnectionString("LocalConnection");
                        break;
                    case ConType.SystemLocal:
                        return _configuration.GetConnectionString("LocalConnection");
                        break;
                    case ConType.LiveProd:
                        return _configuration.GetConnectionString("ProdConnection");
                        break;
                    case ConType.TestProd:
                        return _configuration.GetConnectionString("TestConnection");
                        break;
                    default:
                        return _configuration.GetConnectionString("LocalConnection");
                        break;
                }

                return _configuration.GetConnectionString("LocalConnection");
            }
            catch(Exception xx)
            {
                return _configuration.GetConnectionString("LocalConnection");
            }
        }
    }

    public enum ConType
    {
        Unknown = 0,
        None = 1,
        TestProd = 3,
        SystemLocal = 2,
        LiveProd = 5,
    }
}
