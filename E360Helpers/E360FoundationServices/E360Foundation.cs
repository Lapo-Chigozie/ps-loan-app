using LapoLoanWebApi.E360Helpers.E360DtoModel;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using NETCore.Encrypt.Internal;
using System.Net.Http;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Drawing;
using System.Collections.Specialized;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.HubTeams;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using Microsoft.EntityFrameworkCore;
using LapoLoanWebApi.E360Helpers.E360FoundationServices.Model;
using System.Net.Mime;
using ExcelDataReader.Log;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Org.BouncyCastle.Utilities.Encoders;
using ServiceStack;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.IdentityModel.Tokens;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace LapoLoanWebApi.E360Helpers.E360FoundationServices
{
    public class E360Foundation
    {
        private LapoLoanDBContext lapoLoanDB = null;
        private LapoCipher02 lapoCipher02 { get; set; }
        private LapoCipher02 ReallapoCipher02 { get; set; }
        private LapoCipher00 lapoCipher00 { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }

        private UploadDisbursmentLoans UploadDisbursmentLoans { get; set; }
        private UploadRepaymentSheduled UploadRepaymentSheduled { get; set; }

        private E360HttpAccessTokenClient AccessTokenClient { get; set; }

        private IConfiguration _configuration;

        private ControllerBase controllerBase { get; set; }
        public E360Foundation(ControllerBase controllerBase, IConfiguration configuration
)
        {
            this._configuration = configuration;
            this.controllerBase = controllerBase;
            this.AccessTokenClient = new E360HttpAccessTokenClient(controllerBase, _configuration);
            this.lapoLoanDB = new LapoLoanDBContext(_configuration);
        }

        private string GetE360Rendering(HttpResponseMessage resp)
        {
            try
            {
                       string allHeaders = Enumerable
                       .Empty<(String name, String value)>()
                       // Add the main Response headers as a flat list of value-tuples with potentially duplicate `name` values:
                       .Concat( resp.Headers
                               .SelectMany(kvp => kvp.Value
                                   .Select(v => (name: kvp.Key, value: v))
                               )
                       )
                       // Concat with the content-specific headers as a flat list of value-tuples with potentially duplicate `name` values:
                       .Concat(resp.Content.Headers
                               .SelectMany(kvp => kvp.Value
                                   .Select(v => (name: kvp.Key, value: v))
                               )
                       )
                       // Render to a string:
                       .Aggregate(
                           seed: new StringBuilder(),
                           func: (sb, pair) => sb.Append(pair.name).Append(": ").Append(pair.value).AppendLine(),
                           resultSelector: sb => sb.ToString()
                       );

                return allHeaders;
            }
            catch (Exception ex)
            {
                return ex.Message ?? ex.InnerException.Message;
            }
        }

        private NameValueCollection GetE360Rendering1(HttpResponseMessage resp)
        {
            try
            {
                 NameValueCollection allHeaders = Enumerable
                 .Empty<(String name, String value)>()
                 // Add the main Response headers as a flat list of value-tuples with potentially duplicate `name` values:
                 .Concat(
                     resp.Headers
                         .SelectMany(kvp => kvp.Value
                             .Select(v => (name: kvp.Key, value: v))
                         )
                 )
                 // Concat with the content-specific headers as a flat list of value-tuples with potentially duplicate `name` values:
                 .Concat(
                     resp.Content.Headers
                         .SelectMany(kvp => kvp.Value
                             .Select(v => (name: kvp.Key, value: v))
                         )
                 )
                 .Aggregate(
                     seed: new NameValueCollection(),
                     func: (nvc, pair) => { nvc.Add(pair.name, pair.value); return nvc; },
                     resultSelector: nvc => nvc
                 );

                return allHeaders;
            }
            catch (Exception ex)
            {
                return new NameValueCollection();
            }
        }

        private async Task<RespondMessageDto> GetE360ApiTokenAsync()
        {
            try
            {
                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                // httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                //httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", string.Concat(hexsto, token));

                var response = await httpClient.GetAsync(new DefalutToken(_configuration).E360TokenApiUrl());

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string getRes = await response.Content.ReadAsStringAsync();

                    if (response.Headers.Contains("x-lapo-eve-proc"))
                    {
                        var cdnCacheStatus = response.Headers.GetValues("x-lapo-eve-proc").FirstOrDefault();
                        var maxAge = response.Headers.CacheControl?.MaxAge;

                        var lapoEve_Proc = cdnCacheStatus.Split('~');

                        return new RespondMessageDto(null, null, null, null, null, null, null, " ", true, lapoEve_Proc, lapoEve_Proc, Status.Success, StatusMgs.Success, false);
                    }

                    string error = response.Headers.GetValues("X-Error").FirstOrDefault();
                    string errorCode = response.Headers.GetValues("X-Error-Code").FirstOrDefault();

                    if (response.Content.Headers.ContentType?.MediaType == "application/json")
                    {
                        
                    }

                    FileLogActivities.CallSevice("E360 ", getRes, "No user");
                }
                else
                {
                    string getRes = await response.Content.ReadAsStringAsync();
                    FileLogActivities.CallSevice("E360 ", getRes, "No user");
                }

                return new RespondMessageDto(null, null, null, null, null, null, null, "", false, null, null, Status.Failed, StatusMgs.Failed, false);
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice("E360 ", ex.Message?? ex.InnerException.Message, "No user");
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> MakeContract()
        {
            HttpClient client = new();

            using HttpResponseMessage response = await client.GetAsync(new DefalutToken(_configuration).E360TokenApiUrl());

            FileLogActivities.CallSevice("E360 ", "Start Token", "No user");
            var credentials = "";

            if (response.Headers.TryGetValues("x-lapo-eve-proc", out IEnumerable<string> resHeaders))
            {
                credentials = resHeaders.FirstOrDefault();
            }

            FileLogActivities.CallSevice("E360 ", "Success Token", "No user");
            return new RespondMessageDto(null, null, null, null, null, null, null, " ", true, credentials.Split("~"), credentials.Split("~"), Status.Success, StatusMgs.Success, false);
        }

        public async Task<RespondMessageDto> GetE360ApiAsync(long StaffId, string inputUser, string StaffRefs, string StaffUserRoles)
        {
            try
            {
                string UserRole ="", StaffRef ="";
                var httpClient = new HttpClient();

                // var E360Status = await this.GetE360ApiTokenAsync();

                var E360Status = await MakeContract();

                if (E360Status == null || E360Status.IsActive == false)
                {
                    return E360Status;
                }

                try
                {
                    // System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                    // var folderName = Path.Combine("AccessTokenLogs", "Documents");

                    string currentDirectory = Directory.GetCurrentDirectory();
                    string path = "HtmlPackages";

                    string fullDir = Path.Combine(currentDirectory, path, "E360ApiTokenAccess.txt");

                    if (!string.IsNullOrEmpty(fullDir) && !string.IsNullOrWhiteSpace(fullDir) && System.IO.File.Exists(fullDir))
                    {
                        var Alltext = System.IO.File.ReadAllText(fullDir);

                        string[] strings = { Alltext, "_ _ __ __ __ __ __ _ _ __ ___", "_ ___ ___ __ __ __ __ __ __", "__ __ ____ ____ ___ __ __", "___ ____ ___ ____ __ ___", "__ __ ____ ___ __ ___ ___", "New Access Token ", "Access Token: " + E360Status.DataLoad[0], "Aes Key: " + E360Status.DataLoad[1], "AesIv: " + E360Status.DataLoad[2], "3: " + E360Status.DataLoad[3], "4: " + E360Status.DataLoad[4], "Created Date: " + DateTime.Now.ToString(), "Time: " + DateTime.Now.ToLongTimeString() };

                        System.IO.File.WriteAllLines(fullDir, strings);

                        string HtmData = System.IO.File.ReadAllText(fullDir);
                    }
                }
                catch (Exception ex)
                {
                    FileLogActivities.CallSevice("E360 ", ex.Message?? ex.InnerException.Message, "No user");
                }

                var lapoPeople1 = await lapoLoanDB.People.Where(s => s.AccountId == StaffId).FirstOrDefaultAsync();

                if (lapoPeople1 == null)
                {
                    FileLogActivities.CallSevice("E360 ", "People ID", "No user");
                    return new RespondMessageDto(null, null, null, null, null, null, null, "", false, null, null, Status.Failed, StatusMgs.Failed, false);
                }
                else
                {
                    var lapoPeople2 = await lapoLoanDB.SecurityAccounts.Where(s => s.PersonId == lapoPeople1.Id && s.Id == StaffId).FirstOrDefaultAsync();

                    if (lapoPeople1 != null  && lapoPeople2 != null  && (lapoPeople2.Username.ToLower().Equals("SN0001".ToLower()) || lapoPeople2.Username.ToLower().Equals("0001".ToLower())))
                    {
                        StaffRef = StaffRefs;
                        UserRole = StaffUserRoles;
                    }
                    else if (lapoPeople1 != null)
                    {
                        StaffRef = lapoPeople1.Staff;
                        UserRole = lapoPeople1.UserRole;
                    }
                    else
                    {

                    }
                }

                // lapoPeople.Staff = "afa74845-8c63-44cd-a775-b729b2bd759c";

                var WidgetDiv = new WidgetDivisionModel()
                {
                    tk = (string)E360Status.DataLoad[0],
                    src = "AS-IN-D659B-e3M",
                    rl = UserRole,
                    us = StaffRef,
                };

                // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                // httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

                var PostHeaderData = JsonConvert.SerializeObject(WidgetDiv);

                //new { tk = WidgetDiv.tk,  src = "AS-IN-D659B-e3M", }

                var KeyApi = (string)E360Status.DataLoad[1];
                var AIKey = (string)E360Status.DataLoad[2];

                string encrypted = EncryptProvider.AESEncrypt(PostHeaderData, KeyApi, AIKey);
                byte[] inbyteto = Convert.FromBase64String(encrypted);
                string hexsto = BitConverter.ToString(inbyteto).Replace("-", "").ToLower();

                httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", string.Concat(hexsto, WidgetDiv.tk));

                // Body -Post
                var WidgetStaff = new WidgetStaffModel()
                {
                    //xFromDate = "",
                    //xToDate = "",
                    //xPageIndex = 0,
                    //xPageSize = 0,
                    //xRowCount = 0,
                    //xApp = "AS-IN-D659B-e3M",
                    //xBuCode = null,
                    //xScopeRef = null, //"DV003",
                    //xScope = null, //"Divison",
                    xParam = StaffRef
                };

                var postData = JsonConvert.SerializeObject(WidgetStaff);

                string PostEnc = EncryptProvider.AESEncrypt(postData, KeyApi, AIKey);
                byte[] inbyte = Convert.FromBase64String(PostEnc);
                string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                var stringContent = new StringContent(hexs,  Encoding.UTF8, /* MediaTypeNames.Application.Json*/   "application/json" /*in older versions*/);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(new DefalutToken(_configuration).E360UserDivision()),
                    Content = stringContent
                };

                var response = await httpClient.PostAsync(new DefalutToken(_configuration).E360UserDivision(), stringContent);

                string getRes1 = await response.Content.ReadAsStringAsync();

                response = await httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.IsSuccessStatusCode)
                {
                    getRes1 = await response.Content.ReadAsStringAsync();

                    var geSrchBvnObj = JObject.Parse(getRes1);
                    string Data1 = geSrchBvnObj["data"].ToString();

                    // var Result1 =  JsonConvert.DeserializeObject<WidgetStaffRespondsModel>(getRes1);
                    FileLogActivities.CallSevice("E360 ", getRes1, "No user");

                    string Data = Data1.ToString();
                    byte[] DataByte = StringToByteArray(Data);
                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, KeyApi, AIKey);

                    // get the relationship
                    var getUserData = JObject.Parse(DecryptData);
                    string DivisionCode = getUserData["DivisionCode"].ToString();
                    string DivisionName = getUserData["DivisionName"].ToString();
                    string ZoneName = getUserData["ZoneName"].ToString();

                    var newUserData = new { DivisionCode = DivisionCode, DivisionName = DivisionName, ZoneName = ZoneName , Token = WidgetDiv.tk, KeyApi = KeyApi, AIKey = AIKey,  InputUser = inputUser };

                    return await GetE360ApiAsync(newUserData, StaffId, StaffRef,  UserRole);

                    /// return new RespondMessageDto(null, null, null, null, null, null, null, " ", true, newUserData, newUserData, Status.Success, StatusMgs.Success, true);
                }
                else
                {
                    string getRes = await response.Content.ReadAsStringAsync();
                    FileLogActivities.CallSevice("E360 ", getRes, "No user");
                }

                return new RespondMessageDto(null, null, null, null, null, null, null, "", false, null, null, Status.Failed, StatusMgs.Failed, false);
            }
            catch (Exception ex)
            {

                FileLogActivities.CallSevice("E360 ", ex.Message ?? ex.InnerException.Message, "No user");
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetE360ApiAsync(dynamic Division, long StaffId, string Staff1, string UserRole1)
        {
            try
            {
                string UserRole = "", Staff = "";

                var httpClient = new HttpClient();

                // var E360Status = await this.GetE360ApiTokenAsync();

                var lapoPeople1 = await lapoLoanDB.People.Where(s => s.AccountId == StaffId).FirstOrDefaultAsync();

                if (lapoPeople1 == null)
                {
                    FileLogActivities.CallSevice("E360 ", "People ID", "No user");
                    return new RespondMessageDto(null, null, null, null, null, null, null, "", false, null, null, Status.Failed, StatusMgs.Failed, false);
                }
                else
                {
                    var lapoPeople2 = await lapoLoanDB.SecurityAccounts.Where(s => s.PersonId == lapoPeople1.Id && s.Id == StaffId).FirstOrDefaultAsync();

                    if (lapoPeople1 != null && lapoPeople2 != null && (lapoPeople2.Username.ToLower().Equals("SN0001".ToLower()) || lapoPeople2.Username.ToLower().Equals("0001".ToLower())))
                    {
                        Staff = Staff1;
                        UserRole = UserRole1;
                    }
                    else if(lapoPeople1 != null)
                    {
                        Staff = lapoPeople1.Staff;
                        UserRole = lapoPeople1.UserRole;
                    }
                    else
                    {

                    }
                }

                // lapoPeople.Staff = "afa74845-8c63-44cd-a775-b729b2bd759c";

                var WidgetDiv = new WidgetDivisionModel()
                {
                    tk = (string)Division.Token,
                    src = "AS-IN-D659B-e3M",
                    rl = UserRole,
                    us = Staff,
                };

                // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                // httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

                var PostHeaderData = JsonConvert.SerializeObject(WidgetDiv);

                //new { tk = WidgetDiv.tk,  src = "AS-IN-D659B-e3M", }

                var KeyApi = (string)Division.KeyApi;
                var AIKey = (string)Division.AIKey;

                string encrypted = EncryptProvider.AESEncrypt(PostHeaderData, KeyApi, AIKey);
                byte[] inbyteto = Convert.FromBase64String(encrypted);
                string hexsto = BitConverter.ToString(inbyteto).Replace("-", "").ToLower();

                httpClient.DefaultRequestHeaders.Add("x-lapo-eve-proc", string.Concat(hexsto, Division.Token));

                // Body -Post
                var WidgetStaff = new WidgetDivisionSearchModel()
                {
                    xFromDate = "",
                    xToDate = "",
                    xPageIndex = 1,
                    xPageSize = 1,
                    xRowCount = 1,
                    xApp = "AS-IN-D659B-e3M",
                    xBuCode = "",
                    xScopeRef = "DV003", // Division.DivisionCode, //"DV003",
                    xScope =  "Divison", 
                    xParam = Division.InputUser
                };

                //Division.DivisionName,
                // var newUserData = new { DivisionCode = DivisionCode, DivisionName = DivisionName, ZoneName = ZoneName };

                var postData = JsonConvert.SerializeObject(WidgetStaff);

                string PostEnc = EncryptProvider.AESEncrypt(postData, KeyApi, AIKey);
                byte[] inbyte = Convert.FromBase64String(PostEnc);
                string hexs = BitConverter.ToString(inbyte).Replace("-", "").ToLower();

                var stringContent = new StringContent(hexs, Encoding.UTF8, /* MediaTypeNames.Application.Json*/   "application/json" /*in older versions*/);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(new DefalutToken(_configuration).E360searchemployees()),
                    Content = stringContent
                };

                var response = await httpClient.PostAsync(new DefalutToken(_configuration).E360searchemployees(), stringContent);

                string getRes1 = await response.Content.ReadAsStringAsync();

                response = await httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.IsSuccessStatusCode)
                {
                    getRes1 = await response.Content.ReadAsStringAsync();

                    var geSrchBvnObj = JObject.Parse(getRes1);
                    string Data1 = geSrchBvnObj["data"].ToString();

                    string Data = Data1.ToString();
                    byte[] DataByte = StringToByteArray(Data);
                    string Database64String = Convert.ToBase64String(DataByte, 0, DataByte.Length);
                    string DecryptData = EncryptProvider.AESDecrypt(Database64String, KeyApi, AIKey);

                    FileLogActivities.CallSevice("E360 ", getRes1, getRes1);

                    //  var getUserData = JObject.Parse(DecryptData);

                    var Result1 = JsonConvert.DeserializeObject<List<WidgetDivisionSearchRespondModel>>(DecryptData);

                   var ListData = new   List<WidgetDivisionRespondModel>();

                    foreach(var itm in Result1)
                    {
                        var splitNames = itm.ItemName.Split(' ');

                        var newList1 = new WidgetDivisionRespondModel()
                        {
                            FirstName = splitNames[0] == null || splitNames[0] == "" ? "" : splitNames[0],
                            LastName = splitNames[1] == null || splitNames[1] == "" ? "" : splitNames[1],
                            OtherName = splitNames[2] == null || splitNames[2] == "" ? "" : splitNames[2],

                            Gender = itm.Gender,
                            Bu = itm.Bu,
                            ItemCode = itm.ItemCode,
                            ItemName = itm.ItemName,
                            Item_Title_Desc = itm.Item_Title_Desc,
                            xStatus = itm.xStatus,
                            xStatusId = itm.xStatusId,
                        };

                        if(string.IsNullOrEmpty(newList1.LastName) || string.IsNullOrWhiteSpace(newList1.LastName))
                        {
                            newList1.LastName = splitNames[3] == null || splitNames[3] == "" ? "" : splitNames[3];
                        }

                        ListData.Add(newList1);
                    }
                  
                    //string DivisionCode = getUserData["DivisionCode"].ToString();
                    //string DivisionName = getUserData["DivisionName"].ToString();
                    //string ZoneName = getUserData["ZoneName"].ToString();

                    //var newUserData = new { DivisionCode = DivisionCode, DivisionName = DivisionName, ZoneName = ZoneName,  };

                    if(ListData.Count > 0)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, null, " ", true, ListData, ListData, Status.Success, StatusMgs.Success, true);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, null, "", false, null, null, Status.Failed, StatusMgs.Failed, false);
                }
                else
                {
                    string getRes = await response.Content.ReadAsStringAsync();

                    FileLogActivities.CallSevice("E360 ", getRes, getRes);
                }

                return new RespondMessageDto(null, null, null, null, null, null, null, "", false, null, null, Status.Failed, StatusMgs.Failed, false);
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice("E360 ", ex.Message ?? ex.InnerException.Message, ex.Message);
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
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
