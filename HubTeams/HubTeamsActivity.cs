using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.ModelDto.PagenationFilterModel;
using LapoLoanWebApi.ModelDto.PagenationHelper;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using LapoLoanWebApi.HubTeams.HubTeamModel;
using System.Text.RegularExpressions;
using LapoLoanWebApi.TwoFactorsHelpers;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Components.Web;
using System.Timers;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Google.Protobuf.WellKnownTypes;
using System.Net.NetworkInformation;
using LapoLoanWebApi.LoanScafFoldModel.SqlStoreProcedureCmmdHelper;
using LapoLoanWebApi.LoanScafFoldModel;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace LapoLoanWebApi.HubTeams
{
    public class HubTeamsActivity
    {
        private ControllerBase controllerBase { get; set; }

        private LapoLoanDBContext lapoLoanDB = null;
        private LapoCipher02 lapoCipher02 { get; set; }
        private LapoCipher02 ReallapoCipher02 { get; set; }
        private LapoCipher00 lapoCipher00 { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }
        private IConfiguration _configuration;

        private LapoDBConnectionStrings LapoDBConnectionStrings { get; set; } = null;
        private System.Data.SqlClient.SqlConnection SqlConn = null;
        private Microsoft.Data.SqlClient.SqlConnection MsSQlConn = null;
        private System.Data.SQLite.Linq.SQLiteProviderFactory SqlLite = null;

        public HubTeamsActivity(ControllerBase controllerBase, IConfiguration configuration)
        {
            this._configuration = configuration;
            this.lapoCipher01 = new LapoCipher01();
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.controllerBase = controllerBase;
            this.lapoCipher00 = new LapoCipher00();
            this.ReallapoCipher02 = new LapoCipher02();
            this.lapoCipher02 = new LapoCipher02();


            this.lapoLoanDB = new LapoLoanDBContext(_configuration);
            this.LapoDBConnectionStrings = new LapoDBConnectionStrings(_configuration);

            this.SqlConn = new System.Data.SqlClient.SqlConnection(LapoDBConnectionStrings.ConnectionString());
            this.MsSQlConn = new Microsoft.Data.SqlClient.SqlConnection(LapoDBConnectionStrings.ConnectionString());
            this.SqlLite = new System.Data.SQLite.Linq.SQLiteProviderFactory();
        }

        private async Task<RespondMessageDto> GetHubGruop()
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.View_Procedure_Hub_Team_Groups, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@CodeGenerated", "CodeGenerated");
                sqlCmd.Parameters.AddWithValue("@AccountId", 1);
                var SqlReader = await sqlCmd.ExecuteReaderAsync();

                var HubTeamGroups = new List<HubTeamGroup>();

                //var SqldataAdapter = new System.Data.SqlClient.SqlDataAdapter(sqlCmd);

                //SqldataAdapter.Fill(dataTable);
                //SqldataAdapter.Fill(dataSet);

                //dataTable = new DataTable();
                //dataTable.Load(await sqlCmd.ExecuteReaderAsync());

                //dataSet = new DataSet(); //conn is opened by dataadapter
                //SqldataAdapter.Fill(dataSet);

                if (SqlReader.HasRows)
                {
                    while (await SqlReader.ReadAsync())
                    {
                        if (await SqlReader.ReadAsync())
                        {
                            // var Id = Convert.ToUInt32(SqlReader["Id"].ToString());

                            var newHubTeamGroup = new HubTeamGroup()
                            {
                                Status = Convert.ToString(SqlReader["Status"].ToString()),
                                CreatedDate = Convert.ToDateTime(SqlReader["CreatedDate"].ToString()),
                                CreatedByAccountId = Convert.ToInt64(SqlReader["CreatedByAccountId"].ToString()),
                                EmailAddress = Convert.ToString(SqlReader["EmailAddress"].ToString()),
                                IsGroupHead = Convert.ToBoolean(SqlReader["IsGroupHead"].ToString()),
                                HubTeamGroupName = Convert.ToString(SqlReader["HUB_TEAM_GroupName"].ToString()),
                                PhoneNumber = Convert.ToString(SqlReader["PhoneNumber"].ToString()),
                                UpdatedDate = Convert.ToDateTime(SqlReader["UpdatedDate"].ToString()),
                                Id = Convert.ToInt64(SqlReader["Id"].ToString()),
                            };

                            HubTeamGroups.Add(newHubTeamGroup);
                        }
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Success", true, HubTeamGroups, HubTeamGroups, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, HubTeamGroups, HubTeamGroups, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        private async Task<RespondMessageDto> GetHubTeams()
        {
            try
            {
                this.SqlConn.Open();

                var sqlCmd = new System.Data.SqlClient.SqlCommand(SqlStoreProcedureCmdHelpers.View_Procedure_HubTeams, SqlConn);
                sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@CodeGenerated", "CodeGenerated");
                sqlCmd.Parameters.AddWithValue("@AccountId", 1);
                var SqlReader = await sqlCmd.ExecuteReaderAsync();

                var HubTeams = new List<HubTeam>();

                //var SqldataAdapter = new System.Data.SqlClient.SqlDataAdapter(sqlCmd);

                //SqldataAdapter.Fill(dataTable);
                //SqldataAdapter.Fill(dataSet);

                //dataTable = new DataTable();
                //dataTable.Load(await sqlCmd.ExecuteReaderAsync());

                //dataSet = new DataSet(); //conn is opened by dataadapter
                //SqldataAdapter.Fill(dataSet);

                if (SqlReader.HasRows)
                {
                    while (await SqlReader.ReadAsync())
                    {
                        if (await SqlReader.ReadAsync())
                        {
                            // var Id = Convert.ToUInt32(SqlReader["Id"].ToString());

                            var newHubTeams = new HubTeam()
                            {
                                Status = Convert.ToString(SqlReader["Status"].ToString()),
                                CreatedDate = Convert.ToDateTime(SqlReader["CreatedDate"].ToString()),
                                CreatedByAccountId = Convert.ToInt64(SqlReader["CreatedByAccountId"].ToString()),
                                UpdatedDate = Convert.ToDateTime(SqlReader["UpdatedDate"].ToString()),
                                Id = Convert.ToInt64(SqlReader["Id"].ToString()),
                                 AccessRightToAnonymousLoanApplication = Convert.ToBoolean(SqlReader["AccessRightToAnonymousLoanApplication"].ToString()),
                                  AccessRighttoapprovecustomerloan = Convert.ToBoolean(SqlReader["AccessRighttoapprovecustomerloan"].ToString()),
                                   AccessRighttocreateahub = Convert.ToBoolean(SqlReader["AccessRighttocreateahub"].ToString()),
                                    AccessRighttocreateateammember = Convert.ToBoolean(SqlReader["AccessRighttocreateateammember"].ToString()), 
                                     AccessRighttocreatetenure = Convert.ToBoolean(SqlReader["AccessRighttocreatetenure"].ToString()),
                                      AccessRightToExportDisbursementloan = Convert.ToBoolean(SqlReader["AccessRightToExportDisbursementloan"].ToString()),
                                       AccessRighttodisablehubs = Convert.ToBoolean(SqlReader["AccessRighttodisablehubs"].ToString()),
                                        AccessRighttoloansettings = Convert.ToBoolean(SqlReader["AccessRighttoloansettings"].ToString()),
                                         AccessRighttodisablecustomerstoapplyforaloan = Convert.ToBoolean(SqlReader["AccessRighttodisablecustomerstoapplyforaloan"].ToString()),
                                          AccessRightToEditTeamMemberPermissions = Convert.ToBoolean(SqlReader["AccessRightToEditTeamMemberPermissions"].ToString()),
                                           AccessRightToPrintLoan = Convert.ToBoolean(SqlReader["AccessRightToPrintLoan"].ToString()),
                                            AccessRightToProceedLoan = Convert.ToBoolean(SqlReader["AccessRightToProceedLoan"].ToString()),
                                             AccessRightToViewDisbursementLoan = Convert.ToBoolean(SqlReader["AccessRightToViewDisbursementLoan"].ToString()),
                                              AccessRighttoviewcustomers = Convert.ToBoolean(SqlReader["AccessRighttoviewcustomers"].ToString()),
                                               AccessRighttoteamsAndpermissions = Convert.ToBoolean(SqlReader["AccessRighttoteamsAndpermissions"].ToString()),
                                                AccessRighttorejectaloan = Convert.ToBoolean(SqlReader["AccessRighttorejectaloan"].ToString()),
                                                 AccessRightToUploadBackDisbursementloan = Convert.ToBoolean(SqlReader["AccessRightToUploadBackDisbursementloan"].ToString()),
                                                  AccessRightToUploadBackRepaymentLoan = Convert.ToBoolean(SqlReader["AccessRightToUploadBackRepaymentLoan"].ToString()),
                                                   AccessRighttoviewcustomersloans = Convert.ToBoolean(SqlReader["AccessRighttoviewcustomersloans"].ToString()),
                                                    AccessRighttoviewhubs = Convert.ToBoolean(SqlReader["AccessRighttoviewhubs"].ToString()),
                                                     AccessRighttoviewloandetails = Convert.ToBoolean(SqlReader["AccessRighttoviewloandetails"].ToString()),
                                                      AccessRighttoviewtenure = Convert.ToBoolean(SqlReader["AccessRighttoviewtenure"].ToString()),
                                                       AccessRightToViewUploadBackRepaymentLoan = Convert.ToBoolean(SqlReader["AccessRightToViewUploadBackRepaymentLoan"].ToString()),
                                                        AccessRighttoviewveammembers = Convert.ToBoolean(SqlReader["AccessRighttoviewveammembers"].ToString()),
                                                         CreateLoanNarration = Convert.ToBoolean(SqlReader["CreateLoanNarration"].ToString()),
                                                          ViewLoanNarration = Convert.ToBoolean(SqlReader["ViewLoanNarration"].ToString()),
                                                           TeamAccountId = Convert.ToInt64(SqlReader["TeamAccountId"].ToString()),
                                                            HubMemberId = Convert.ToString(SqlReader["HubMemberId"].ToString()),
                                                             GroupId = Convert.ToInt64(SqlReader["GroupId"].ToString()),
                                                              BackEndUsers = Convert.ToString(SqlReader["BackEndUsers"].ToString()),
                                                               HubMemberFirstNames = Convert.ToString(SqlReader["HubMemberFirstNames"].ToString()),
                                                                HubMemberLastNames = Convert.ToString(SqlReader["HubMemberLastNames"].ToString()),
                                                                 HubMemberOtherNames = Convert.ToString(SqlReader["HubMemberOtherNames"].ToString()),
                                                                  RefNo = Convert.ToString(SqlReader["RefNo"].ToString()),
                                                                   IsBlocked = Convert.ToBoolean(SqlReader["IsBlocked"].ToString()),
                            };

                            HubTeams.Add(newHubTeams);
                        }
                    }

                    return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Success, "Success", true, HubTeams, HubTeams, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, SqlReader, null, sqlCmd, StatusMgs.Failed, "Failed", false, HubTeams, HubTeams, Status.Failed, StatusMgs.Failed);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(this.SqlConn, this.MsSQlConn, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, null, null, Status.Failed, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetHubTeamList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<HubTeamGroup> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                Loanheaders = await lapoLoanDB.HubTeamGroups.Where(s => s.Status == "Active").ToListAsync();

                //var dataResult = await GetHubGruop();

                //try
                //{
                //    if (dataResult.IsActive && dataResult != null)
                //    {
                //        Loanheaders = dataResult.Data as List<HubTeamGroup>;
                //    }
                //    else
                //    {
                //        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(s => s.Status == "Active").ToListAsync();
                //    }
                //}
                //catch(Exception ex)
                //{
                //    Loanheaders = await lapoLoanDB.HubTeamGroups.Where(s => s.Status == "Active").ToListAsync();
                //}

                //Loanheaders = await lapoLoanDB.HubTeamGroups.Where(s => s.Status == "Active" && !s.HubTeamGroupName.StartsWith("Public Sector Office - from domi account") && !s.HubTeamGroupName.StartsWith("Public Sector Office")).ToListAsync();

               

                if (pagenationFilter.IsSearchBar)
                {
                    double Amount = 0;

                    try
                    {
                        Amount = Convert.ToDouble(pagenationFilter.searchText);
                    }
                    catch (Exception exs)
                    {

                    }

                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubTeamGroupName.StartsWith(pagenationFilter.searchText))).ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubTeamGroupName.StartsWith(pagenationFilter.searchText) || n.Status.StartsWith(pagenationFilter.searchText)) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)).ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                    }
                }

                var HubTeamsAppList = new List<LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamGroupModel>();
               
                string Names = "";

                string TeamLead = "";

                Loanheaders = Loanheaders.Where(s => s.Status == StatusMgs.Active).ToList();

                if (Loanheaders == null || Loanheaders.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub Teams found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                foreach (var itm in Loanheaders)
                {
                    try
                    {
                        
                        //if (!itm.HubTeamGroupName.ToLower().StartsWith("Head Office".ToLower()) && !itm.HubTeamGroupName.ToLower().Contains("Head Office".ToLower()) && !itm.HubTeamGroupName.ToLower().StartsWith("Public Sector".ToLower()) && !itm.HubTeamGroupName.ToLower().Contains("Public Sector".ToLower()) && !itm.HubTeamGroupName.ToLower().Contains("Public Sector Office - from domi account".ToLower()))
                        //{
                            no += 1;

                            var name = await lapoLoanDB.People.Where(a => a.AccountId == itm.CreatedByAccountId).FirstOrDefaultAsync();
                            if (name != null)
                            {
                                Names = name.FirstName + "  " + name.LastName;
                            }
                            else
                            {
                                Names = "";
                            }

                            var HubTeam = await lapoLoanDB.HubTeams.Where(a => a.GroupId == itm.Id && lapoLoanDB.HubTeamManagers.Where(u => u.HubTeamsId == a.Id && u.HubTeamSubGroupId == a.GroupId && u.Status == StatusMgs.Active).Any()).FirstOrDefaultAsync();

                            if (HubTeam != null)
                            {
                                TeamLead = HubTeam.HubMemberFirstNames + "  " + HubTeam.HubMemberLastNames;
                            }
                            else
                            {
                                TeamLead = "";
                            }

                            var newHubTeamApp1 = new LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamGroupModel()
                            {
                                CreatedByName = Names.ToLower().Contains("Olorunmo Ojo".ToLower()) || Names.ToLower().Contains("Olorunmo".ToLower()) || Names.ToLower().StartsWith("Olorunmo".ToLower()) ? "System Admin" : Names,
                                CreatedDate = itm.CreatedDate.Value.ToLongDateString(),
                                Status = itm.Status,
                                HubTeamGroupName = itm.HubTeamGroupName,
                                Id = lapoCipher01.EnryptString(itm.Id),
                                No = no.ToString(),
                                TeamLead = TeamLead
                            };

                            HubTeamsAppList.Add(newHubTeamApp1);
                        //}
                    }
                    catch (Exception er)
                    {

                    }
                }

                if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
                {
                    var Pagenation = await new PagenationsHelper().GetPagenation<HubTeamGroupModel>(HubTeamsAppList, pagenationFilter);

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hubs retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetHubTeamByMemberList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<HubTeamGroup> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

              //  Loanheaders = await lapoLoanDB.HubTeamGroups.Where(a => a.Status == "Active").ToListAsync();

                Loanheaders = await lapoLoanDB.HubTeamGroups.Where(s => s.Status == "Active" && !s.HubTeamGroupName.StartsWith("Public Sector Office - from domi account") && !s.HubTeamGroupName.StartsWith("Public Sector Office")).ToListAsync();

                if (Loanheaders == null || Loanheaders.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }


                //if (pagenationFilter.IsSearchBar)
                //{
                //    double Amount = 0;

                //    try
                //    {
                //        Amount = Convert.ToDouble(pagenationFilter.searchText);
                //    }
                //    catch (Exception exs)
                //    {

                //    }

                //    if (pagenationFilter.status.Equals("All"))
                //    {
                //        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubTeamGroupName.StartsWith(pagenationFilter.searchText))).ToListAsync();
                //    }
                //    else
                //    {
                //        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubTeamGroupName.StartsWith(pagenationFilter.searchText) || n.Status.StartsWith(pagenationFilter.searchText)) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                //    }
                //}
                //else
                //{
                //    if (pagenationFilter.status.Equals("All"))
                //    {
                //        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)).ToListAsync();
                //    }
                //    else
                //    {
                //        Loanheaders = await lapoLoanDB.HubTeamGroups.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                //    }
                //}


                var HubTeamsAppList = new List<LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamGroupModel>();
                string Names = "";

                foreach (var itm in Loanheaders)
                {
                    try
                    {
                        no += 1;

                        var name = await lapoLoanDB.People.Where(a => a.AccountId == itm.CreatedByAccountId).FirstOrDefaultAsync();
                        if (name != null)
                        {
                            Names = name.FirstName + "  " + name.LastName;
                        }
                        else
                        {
                            Names = "";
                        }

                        var newHubTeamApp1 = new LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamGroupModel()
                        {
                            CreatedByName = Names,
                            CreatedDate = itm.CreatedDate.Value.ToLongDateString(),
                            Status = itm.Status,
                            HubTeamGroupName = itm.HubTeamGroupName,
                            Id = lapoCipher01.EnryptString(itm.Id),
                            No = no.ToString(),
                        };

                        HubTeamsAppList.Add(newHubTeamApp1);
                    }
                    catch (Exception er)
                    {

                    }
                }

                if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
                {
                    var Pagenation = await new PagenationsHelper().GetPagenation<HubTeamGroupModel>(HubTeamsAppList, pagenationFilter);

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hubs retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        #region Old GetSubHubTeamList
        //public async Task<RespondMessageDto> GetSubHubTeamList(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        //{
        //    try
        //    {
        //        List<HubTeamSubGroup> Loanheaders = null;
        //        int no = 0;
        //        var LoanAppList = new List<LoanAppListModelDto>();

        //        var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
        //        var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

        //        if (fromDate > toDate)
        //        {
        //            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
        //        }

        //        var Idd = Convert.ToInt64(lapoCipher01.DecryptString(pagenationFilter.AppId));

        //        var HubTeamGroups = await lapoLoanDB.HubTeamSubGroups.Where(c => c.HubTeamGroupId == Idd).ToListAsync();

        //        if (HubTeamGroups == null || HubTeamGroups.Count <= 0)
        //        {
        //            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub Teams found.", false, "", null, Status.Ërror, StatusMgs.Error);
        //        }

        //        if (pagenationFilter.IsSearchBar)
        //        {
        //            double Amount = 0;

        //            try
        //            {
        //                Amount = Convert.ToDouble(pagenationFilter.searchText);
        //            }
        //            catch (Exception exs)
        //            {

        //            }

        //            if (pagenationFilter.status.Equals("All"))
        //            {
        //                Loanheaders = await lapoLoanDB.HubTeamSubGroups.Where(n => n.HubTeamGroupId == Idd && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubTeamSubGroupName.Equals(pagenationFilter.searchText))).ToListAsync();
        //            }
        //            else
        //            {
        //                Loanheaders = await lapoLoanDB.HubTeamSubGroups.Where(n => n.HubTeamGroupId == Idd && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubTeamSubGroupName.Equals(pagenationFilter.searchText) || n.Status.Equals(pagenationFilter.searchText)) || n.Status.Equals(pagenationFilter.status)).ToListAsync();
        //            }
        //        }
        //        else
        //        {
        //            if (pagenationFilter.status.Equals("All"))
        //            {
        //                Loanheaders = await lapoLoanDB.HubTeamSubGroups.Where(n => n.HubTeamGroupId == Idd && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)).ToListAsync();
        //            }
        //            else
        //            {
        //                Loanheaders = await lapoLoanDB.HubTeamSubGroups.Where(n => n.HubTeamGroupId == Idd && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
        //            }
        //        }

        //        var HubTeamsAppList = new List<LapoLoanWebApi.HubTeams.HubTeamModel.SubHubTeamGroupModel>();

        //        foreach (var itm in HubTeamGroups)
        //        {
        //            try
        //            {
        //                no += 1;

        //                var newHubTeamApp1 = new LapoLoanWebApi.HubTeams.HubTeamModel.SubHubTeamGroupModel()
        //                {
        //                    CreatedByName = itm.CreatedByAccount.Person.FirstName + " " + itm.CreatedByAccount.Person.FirstName,
        //                    CreatedDate = itm.CreatedDate.Value.ToLongDateString(),
        //                    Status = itm.Status,
        //                    SubHubTeamGroupName = itm.HubTeamSubGroupName,
        //                    HubTeamGroupName= itm.HubTeamGroup.HubTeamGroupName,
        //                    Id = lapoCipher01.EnryptString(itm.Id), 
        //                    GroupId = pagenationFilter.AppId,
        //                    No = no.ToString(),
        //                };

        //                HubTeamsAppList.Add(newHubTeamApp1);
        //            }
        //            catch (Exception er)
        //            {

        //            }
        //        }

        //        if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
        //        {
        //            var Pagenation = await new PagenationsHelper().GetPagenation<SubHubTeamGroupModel>(HubTeamsAppList, pagenationFilter);

        //            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Teams App Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
        //        }

        //        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
        //    }
        //}

        #endregion
        public async Task<RespondMessageDto> CreateHubTeam(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, NewHubGroup newHub)
        {
            try
            {
                //  List<HubTeamSubGroup> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();


                if(newHub.IsGroupHeadOffice)
                {
                    if(string.IsNullOrEmpty(newHub.EnterPhoneNumber) || string.IsNullOrWhiteSpace(newHub.EnterPhoneNumber))
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error,  " ", false, "Phone Number is required", null, Status.Ërror, StatusMgs.Error);
                    }

                    if (string.IsNullOrEmpty(newHub.EnterEmailAddress) || string.IsNullOrWhiteSpace(newHub.EnterEmailAddress))
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, " .", false, "Email Address is required", null, Status.Ërror, StatusMgs.Error);
                    }
                }


                var HubTeamGroups = await lapoLoanDB.HubTeamGroups.Where(c => c.HubTeamGroupName == newHub.GroupName).FirstOrDefaultAsync();

                if (HubTeamGroups != null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, newHub.GroupName +  " is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (newHub.IsGroupHeadOffice)
                {
                     HubTeamGroups = await lapoLoanDB.HubTeamGroups.Where(c => c.IsGroupHead == newHub.IsGroupHeadOffice  || (c.IsGroupHead == newHub.IsGroupHeadOffice  && c.HubTeamGroupName == newHub.GroupName)).FirstOrDefaultAsync();

                    if (HubTeamGroups != null)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, newHub.GroupName + " is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }
                }

                var newHubTeamGroup = new HubTeamGroup()
                {
                    CreatedDate = DateTime.Now,
                    Status = StatusMgs.Active,
                    UpdatedDate = DateTime.Now,
                    HubTeamGroupName = newHub.GroupName,
                    IsGroupHead = newHub.IsGroupHeadOffice,
                    CreatedByAccountId = long.Parse(newHub.CreateAccountId),
                    EmailAddress = newHub.EnterEmailAddress,
                    PhoneNumber = newHub.EnterPhoneNumber,
                };
                lapoLoanDB.HubTeamGroups.Add(newHubTeamGroup);
                await lapoLoanDB.SaveChangesAsync();

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, newHub.GroupName + " successfully added.", true, "", "", Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SetOfficerStandardLoan(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, SetStandardLoanModel setStandardLoan)
        {
            try
            {
            
                int no = 0;

                var Id = long.Parse(lapoCipher01.DecryptString(setStandardLoan.ItemId));
                var OffId = long.Parse(setStandardLoan.ReconciliationOfficer);

                var LoanApplicationRequest = await lapoLoanDB.LoanApplicationRequestHeaders.Where(c => c.Id == Id  && c.IsGroundStandardOrAnonymous == false && c.Status ==  StatusMgs.Pending).FirstOrDefaultAsync();

                if (LoanApplicationRequest == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This loan application is no longer exiting as Anonymous loan application.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeam = await lapoLoanDB.HubTeams.Where(c => c.Id == OffId && c.Status == StatusMgs.Active).FirstOrDefaultAsync();

                if (HubTeam == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Reconciliation Officer is no longer exiting or activated.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(c => c.Id == HubTeam.GroupId && c.Status == StatusMgs.Active).FirstOrDefaultAsync();

                if (HubTeamGroup == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Reconciliation Officer Hub is no longer exiting or activated.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (LoanApplicationRequest != null)
                {
                    LoanApplicationRequest.RelationshipOfficerRef = HubTeam.RefNo;
                    LoanApplicationRequest.TeamOfficerFirstname = HubTeam.HubMemberFirstNames;
                    LoanApplicationRequest.TeamOfficerOthername = HubTeam.HubMemberLastNames;
                    LoanApplicationRequest.TeamGroundName = HubTeamGroup.HubTeamGroupName;
                    LoanApplicationRequest.IsGroundStandardOrAnonymous = true;

                    lapoLoanDB.Entry(LoanApplicationRequest).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Reconciliation Officer saved successfully.", true, "", "", Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "This loan application is no longer exiting as Anonymous loan application.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        #region Old CreateSubHubTeam
        //public async Task<RespondMessageDto> CreateSubHubTeam(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, SubHubTeamModel newHub)
        //{
        //    try
        //    {
        //        List<HubTeamSubGroup> Loanheaders = null;
        //        int no = 0;
        //        var LoanAppList = new List<LoanAppListModelDto>();

        //        var GroupId = lapoCipher01.DecryptString(newHub.GroupId);

        //        var HubTeamGroups = await lapoLoanDB.HubTeamSubGroups.Where(c => c.HubTeamSubGroupName == newHub.SubGroupName).FirstOrDefaultAsync();

        //        if (HubTeamGroups != null)
        //        {
        //            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Hub Group is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
        //        }

        //        var newHubTeamGroup = new HubTeamSubGroup()
        //        {
        //            CreatedDate = DateTime.Now,
        //            Status = StatusMgs.Active,
        //            UpdatedDate = DateTime.Now,
        //            HubTeamSubGroupName = newHub.SubGroupName,
        //            HubTeamGroupId = long.Parse(GroupId),
        //            CreatedByAccountId = long.Parse(newHub.CreateAccountId),
        //        };
        //        lapoLoanDB.HubTeamSubGroups.Add(newHubTeamGroup);
        //        lapoLoanDB.SaveChangesAsync();

        //        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Group was created successfully.", true, "", "", Status.Success, StatusMgs.Success);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
        //    }
        //}
        #endregion Old Region
        public async Task<RespondMessageDto> ActivateHubGroup(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AppsId)
        {
            try
            {
                var AppId = long.Parse(lapoCipher01.DecryptString(AppsId));

                var HubTeamGroups = await lapoLoanDB.HubTeamGroups.Where(c => c.Id == AppId).FirstOrDefaultAsync();

                if (HubTeamGroups != null)
                {
                    if(HubTeamGroups.Status == StatusMgs.Active)
                    {

                        HubTeamGroups.Status = StatusMgs.NotActive;
                        HubTeamGroups.UpdatedDate = DateTime.Now;

                        lapoLoanDB.Entry(HubTeamGroups).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub de-activated successfully.", true, "", "", Status.Success, StatusMgs.Success);
                    }
                    else
                    {
                        HubTeamGroups.Status = StatusMgs.Active;
                        HubTeamGroups.UpdatedDate = DateTime.Now;

                        lapoLoanDB.Entry(HubTeamGroups).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub activated successfully.", true, "", "", Status.Success, StatusMgs.Success);
                    }
                }

              
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Hub does not exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateHubTeamMember(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AppsId)
        {
            try
            {
                var AppId = long.Parse(lapoCipher01.DecryptString(AppsId));

                var HubTeamGroups = await lapoLoanDB.HubTeams.Where(c => c.Id == AppId).FirstOrDefaultAsync();

                if (HubTeamGroups != null)
                {
                    if (HubTeamGroups.Status == StatusMgs.Active)
                    {

                        HubTeamGroups.Status = StatusMgs.NotActive;
                        HubTeamGroups.UpdatedDate = DateTime.Now;

                        lapoLoanDB.Entry(HubTeamGroups).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Team Member de-activated successfully.", true, "", "", Status.Success, StatusMgs.Success);
                    }
                    else
                    {
                        HubTeamGroups.Status = StatusMgs.Active;
                        HubTeamGroups.UpdatedDate = DateTime.Now;

                        lapoLoanDB.Entry(HubTeamGroups).State = EntityState.Modified;
                        await lapoLoanDB.SaveChangesAsync();

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Team Member activated successfully.", true, "", "", Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Member does not exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        #region Old ActivateSubHubGroup
        //public async Task<RespondMessageDto> ActivateSubHubGroup(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AppsId)
        //{
        //    try
        //    {
        //        var AppId = long.Parse(lapoCipher01.DecryptString(AppsId));

        //        var HubTeamGroups = await lapoLoanDB.HubTeamSubGroups.Where(c => c.Id == AppId).FirstOrDefaultAsync();

        //        if (HubTeamGroups != null)
        //        {
        //            if (HubTeamGroups.Status == StatusMgs.Active)
        //            {
        //                HubTeamGroups.Status = StatusMgs.NotActive;
        //                HubTeamGroups.UpdatedDate = DateTime.Now;

        //                lapoLoanDB.Entry(HubTeamGroups).State = EntityState.Modified;
        //                await lapoLoanDB.SaveChangesAsync();

        //                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Sub Hub Group activated successfully.", true, "", "", Status.Success, StatusMgs.Success);
        //            }
        //            else
        //            {
        //                HubTeamGroups.Status = StatusMgs.Active;
        //                HubTeamGroups.UpdatedDate = DateTime.Now;

        //                lapoLoanDB.Entry(HubTeamGroups).State = EntityState.Modified;
        //                await lapoLoanDB.SaveChangesAsync();

        //                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Sub Hub Group de-activated successfully.", true, "", "", Status.Success, StatusMgs.Success);
        //            }
        //        }

        //        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Sub Hub Group is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);

        //    }
        //    catch (System.Exception ex)
        //    {
        //        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
        //    }
        //}
        #endregion  

        public async Task<RespondMessageDto> CreateHubMember1(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, NewHubTeamMember newHub)
        {
            try
            {
                if (string.IsNullOrEmpty(newHub.EnterTeamMemberID) || string.IsNullOrWhiteSpace(newHub.EnterTeamMemberID))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Member is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //if (string.IsNullOrEmpty(newHub.EnterPhoneNumber) || string.IsNullOrWhiteSpace(newHub.EnterPhoneNumber))
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff Phone number is required", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (newHub.EnterPhoneNumber.Length < 11 || newHub.EnterPhoneNumber.Length > 11)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff Phone number must be 11 digit number", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (newHub.EnterEmailAddress.Length > 0 && !newHub.EnterEmailAddress.Contains("@"))
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff Email address is invalid", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (!newHub.EnterTeamMemberID.StartsWith("SN"))
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID must start with SN", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == newHub.CreatedByAccountId).FirstOrDefaultAsync();

                //if (HasPermission == null /*&& newHub.CreatedByAccountId != 34*/)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (HasPermission != null && HasPermission.HasPermissionToCreatedStaff.HasValue && HasPermission.HasPermissionToCreatedStaff.Value == false /*&& newHub.CreatedByAccountId != 34*/)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == newHub.EnterTeamMemberID).AnyAsync();

                var SecurityPerm = await lapoLoanDB.SecurityPermissions.Where(x => x.UserId == newHub.EnterTeamMemberID).AnyAsync();

                var HubTeamG = await lapoLoanDB.HubTeamGroups.ToListAsync();

                if (HubTeamG.Count > 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID is already exiting", false, "", null, Status.Ërror, StatusMgs.Error);
                }

              //  List<HubTeamSubGroup> Loanheaders = null;

                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();


                var HubTeamGroups = await lapoLoanDB.HubTeams.Where(c => c.HubMemberId == newHub.EnterTeamMemberID).FirstOrDefaultAsync();

                var Peopl = await lapoLoanDB.People.Where(c => c.Staff == newHub.EnterTeamMemberID).FirstOrDefaultAsync();

                if (Peopl != null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Member is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //var personDt = await lapoLoanDB.People.Where(c => c.EmailAddress == newHub.EnterEmailAddress  || c.PhoneNumber == newHub.EnterPhoneNumber || c.AltPhoneNumber == newHub.EnterPhoneNumber).FirstOrDefaultAsync();

                //if (personDt != null)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Email, Phone No, Alt Phone No is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //var SecurityDt = await lapoLoanDB.SecurityAccounts.Where(c => c.Username == newHub.EnterEmailAddress).FirstOrDefaultAsync();

                //if (SecurityDt != null)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Email Address is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //var HubTeamsDt = await lapoLoanDB.HubTeams.Where(c => c.EmailAddress == newHub.EnterEmailAddress || c.PhoneNumber == newHub.EnterPhoneNumber ).FirstOrDefaultAsync();

                //if (HubTeamsDt != null)
                //{
                //     return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Email,       Phone No, Alt Phone No is already exiting.", false, "", null, Status.Ërror,    StatusMgs.Error);
                //}

                var Pwd = HashSalt.HashPassword("Admin123!!");

                var newAcct = new SecurityAccount()
                {
                    AccountType = AccountType.Adiministration,
                    AccountVerify = true,
                    AllowLoginTwoFactor = false,
                    CreatedDate = DateTime.Now,
                    Status = StatusMgs.Active,
                    Username = newHub.EnterTeamMemberID,
                    Password = Pwd,
                    LastLoginDate = DateTime.Now, 
                };
                lapoLoanDB.SecurityAccounts.Add(newAcct);
                await lapoLoanDB.SaveChangesAsync();

                var newPeople = new Person()
                {
                    CreatedDate = DateTime.Now,
                    AccountId = newAcct.Id,
                    LastName = newHub.EnterLastName,
                    EmailAddress = newHub.EnterEmailAddress,
                    AltPhoneNumber = newHub.EnterPhoneNumber,
                    CurrentAddress = newHub.TeamMemberOfficeAddress,
                    Age = "25",
                    PhoneNumber = newHub.EnterPhoneNumber,
                    MiddleName = newHub.EnterMiddleName,
                    Staff = newHub.EnterTeamMemberID,
                    PositionOrRole = AccountType.Adiministration,
                    Gender = "Male",
                    FirstName = newHub.EnterFirstName,  
                };
                lapoLoanDB.People.Add(newPeople);
                await lapoLoanDB.SaveChangesAsync();

                var updateS = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == newAcct.Id).FirstOrDefaultAsync();
                if (updateS != null)
                {
                    updateS.PersonId = newPeople.Id;
                    lapoLoanDB.Entry(updateS).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                var newSecurityPer = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.SecurityPermission()
                {
                    UserId = newHub.EnterTeamMemberID,
                    StaffName = newPeople.FirstName + ",    " + newPeople.LastName,
                    AccountId = newAcct.Id,
                    Status = StatusMgs.Active,
                    BlockedDate = DateTime.Now,
                    ActivatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsBlocked = true,
                    HasPermissionToCreatedStaff = false,
                    HasPermissionToDisableStaff = false,
                    IsSupperAdmin = false,
                    AccessRightApprovedLoan = false,
                    TenureAccessRight = false,
                    LoanSettingAccessRight = false,
                    NetPaysAccessRight = false,
                    LoanCompletedAccessRight = false,
                    GeneralPermissionsAccessRight = false,
                    CustomerLoanPermission = false,

                      
                };

                newSecurityPer.Status = StatusMgs.Active;
                lapoLoanDB.SecurityPermissions.Add(newSecurityPer);
                await lapoLoanDB.SaveChangesAsync();

                var HubTeamGroup = new HubTeamGroup()
                {
                    CreatedByAccountId = newAcct.Id,
                    IsGroupHead = true,
                    PhoneNumber = DefalutToken.PublicSectorOfficePhone,
                    UpdatedDate = DateTime.Now,
                    Status = "Active",
                    EmailAddress = DefalutToken.PublicSectorOfficeEmail,
                    CreatedDate = DateTime.Now,
                    HubTeamGroupName = "Public Sector Office - from domi account", 
                };
                lapoLoanDB.HubTeamGroups.Add(HubTeamGroup);
                await lapoLoanDB.SaveChangesAsync();

               // var GroupId = lapoCipher01.DecryptString(HubTeamGroup.Id.ToString());

                var newHubTeamMember = new HubTeam()
                {
                    CreatedDate = DateTime.Now,
                    HubMemberOtherNames = newHub.EnterMiddleName,
                    HubMemberLastNames = newHub.EnterLastName,
                    HubMemberFirstNames = newHub.EnterFirstName,
                    UpdatedDate = DateTime.Now,
                    CreatedByAccountId = updateS.Id,
                    IsBlocked = false,
                    Status = StatusMgs.Active,
                    HubMemberId = newHub.EnterTeamMemberID,
                    TeamAccountId = updateS.Id,
                    GroupId = HubTeamGroup.Id,
                    BackEndUsers = newHub.UserType,
                    RefNo = newHub.EnterTeamMemberID,
                    AccessRightToAnonymousLoanApplication = newHub.AccessRightToAnonymousLoanApplication,
                    AccessRighttoapprovecustomerloan = newHub.AccessRighttoapprovecustomerloan,
                    AccessRighttocreateahub = newHub.AccessRighttocreateahub,
                    AccessRighttocreateateammember = newHub.AccessRighttocreateateammember,
                    AccessRighttocreatetenure = newHub.AccessRighttocreatetenure,
                    AccessRighttodisablecustomerstoapplyforaloan = newHub.AccessRighttodisablecustomerstoapplyforaloan,
                    AccessRighttodisablehubs = newHub.AccessRighttodisablehubs,
                    AccessRightToEditTeamMemberPermissions = newHub.AccessRightToEditTeamMemberPermissions,
                    AccessRightToExportDisbursementloan = newHub.AccessRightToEditTeamMemberPermissions,
                    AccessRighttoloansettings = newHub.AccessRighttoloansettings,
                    AccessRightToPrintLoan = newHub.AccessRightToPrintLoan,
                    AccessRightToProceedLoan = newHub.AccessRightToProceedLoan,
                    AccessRighttorejectaloan = newHub.AccessRighttorejectaloan,
                    AccessRighttoteamsAndpermissions = newHub.AccessRighttoteamsAndpermissions,
                    AccessRightToUploadBackDisbursementloan = newHub.AccessRightToUploadBackDISBURSEMENTLoan,
                    AccessRighttoviewveammembers = newHub.AccessRighttoviewveammembers,
                    AccessRightToViewDisbursementLoan = newHub.AccessRightToViewDisbursementLoan,
                    AccessRighttoviewcustomersloans = newHub.AccessRighttoviewcustomersloans,
                    AccessRightToUploadBackRepaymentLoan = newHub.AccessRightToUploadBackRepaymentLoan,
                    AccessRighttoviewcustomers = newHub.AccessRighttoviewcustomers,
                    AccessRighttoviewhubs = newHub.AccessRighttoviewcustomers,
                    AccessRighttoviewloandetails = newHub.AccessRighttoviewcustomers,
                    AccessRighttoviewtenure = newHub.AccessRighttoviewcustomers,
                    AccessRightToViewUploadBackRepaymentLoan = newHub.AccessRightToViewUploadBackRepaymentLoan,
                    CreateLoanNarration = newHub.CreateLoanNarration,
                    ViewLoanNarration = newHub.ViewLoanNarration
                };
                lapoLoanDB.HubTeams.Add(newHubTeamMember);
                await lapoLoanDB.SaveChangesAsync();

                switch (newHub.UserType)
                {
                    case "TEAM LEAD":
                        await this.ActivateManagerTeamLead(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;
                    case "RECONCILIATION AND ACCOUNT OFFICER":

                        await this.ActivateReconciliationOfficers(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;

                    case "DISBURSEMENT OFFICER":

                        await this.ActivateDisbusmentOfficers(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;

                    default:

                        break;
                }

                if (newHub.UserType == "TEAM LEADS")
                {

                }

                if (newHub.UserType == "RECONCILIATION AND ACCOUNT OFFICERS")
                {

                }

                string message2 = "Lapo : Your Staff ID has been added to Lapo loan application portal.";

                if (!string.IsNullOrEmpty(newHub.EnterPhoneNumber) && !string.IsNullOrWhiteSpace(newHub.EnterPhoneNumber))
                {
                    await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, newHub.EnterPhoneNumber, message2, 0);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "New Team Memeber has been added successfully.", true, "", "", Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> RespondHubMember(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, NewHubTeamMember newHub)
        {
            try
            {
                if (string.IsNullOrEmpty(newHub.EnterTeamMemberID) || string.IsNullOrWhiteSpace(newHub.EnterTeamMemberID))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Member is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

               // var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == newHub.EnterTeamMemberID).AnyAsync();

                var Pwd = HashSalt.HashPassword(newHub.SelectHubGroupIdd);

                var newAcct = new SecurityAccount()
                {
                    AccountType = AccountType.Adiministration,
                    AccountVerify = true,
                    AllowLoginTwoFactor = false,
                    CreatedDate = DateTime.Now,
                    Status = StatusMgs.Active,
                    Username = newHub.EnterTeamMemberID,
                    Password = Pwd,
                    LastLoginDate = DateTime.Now,
                };
                lapoLoanDB.SecurityAccounts.Add(newAcct);
                await lapoLoanDB.SaveChangesAsync();

                var newPeople = new Person()
                {
                    CreatedDate = DateTime.Now,
                    AccountId = newAcct.Id,
                    LastName = newHub.EnterLastName,
                    EmailAddress = newHub.EnterEmailAddress,
                    AltPhoneNumber = newHub.EnterPhoneNumber,
                    CurrentAddress = newHub.TeamMemberOfficeAddress,
                    Age = "25",
                    PhoneNumber = newHub.EnterPhoneNumber,
                    MiddleName = newHub.EnterMiddleName,
                    Staff = newHub.EnterTeamMemberID,
                    PositionOrRole = AccountType.Adiministration,
                    Gender = "Male",
                    FirstName = newHub.EnterFirstName,
                };
                lapoLoanDB.People.Add(newPeople);
                await lapoLoanDB.SaveChangesAsync();

                var updateS = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == newAcct.Id).FirstOrDefaultAsync();
                if (updateS != null)
                {
                    updateS.PersonId = newPeople.Id;
                    lapoLoanDB.Entry(updateS).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                var newSecurityPer = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.SecurityPermission()
                {
                    UserId = newHub.EnterTeamMemberID,
                    StaffName = newPeople.FirstName + ",  " + newPeople.LastName,
                    AccountId = newAcct.Id,
                    Status = StatusMgs.Active,
                    BlockedDate = DateTime.Now,
                    ActivatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsBlocked = true,
                    HasPermissionToCreatedStaff = false,
                    HasPermissionToDisableStaff = false,
                    IsSupperAdmin = false,
                    AccessRightApprovedLoan = false,
                    TenureAccessRight = false,
                    LoanSettingAccessRight = false,
                    NetPaysAccessRight = false,
                    LoanCompletedAccessRight = false,
                    GeneralPermissionsAccessRight = false,
                    CustomerLoanPermission = false,
                };

                newSecurityPer.Status = StatusMgs.Active;
                lapoLoanDB.SecurityPermissions.Add(newSecurityPer);
                await lapoLoanDB.SaveChangesAsync();

                var HubTeamGroup = new HubTeamGroup()
                {
                    CreatedByAccountId = newAcct.Id,
                    IsGroupHead = true,
                    PhoneNumber = DefalutToken.PublicSectorOfficePhone,
                    UpdatedDate = DateTime.Now,
                    Status = "Active",
                    EmailAddress = DefalutToken.PublicSectorOfficeEmail,
                    CreatedDate = DateTime.Now,
                    HubTeamGroupName = "Public Sector Office",
                };
                lapoLoanDB.HubTeamGroups.Add(HubTeamGroup);
                await lapoLoanDB.SaveChangesAsync();

                var newHubTeamMember = new HubTeam()
                {
                    CreatedDate = DateTime.Now,
                    HubMemberOtherNames = newHub.EnterMiddleName,
                    HubMemberLastNames = newHub.EnterLastName,
                    HubMemberFirstNames = newHub.EnterFirstName,
                    UpdatedDate = DateTime.Now,
                    CreatedByAccountId = updateS.Id,
                    IsBlocked = false,
                    Status = StatusMgs.Active,
                    HubMemberId = newHub.EnterTeamMemberID,
                    TeamAccountId = updateS.Id,
                    GroupId = HubTeamGroup.Id,
                    BackEndUsers = newHub.UserType,
                    RefNo = newHub.EnterTeamMemberID,
                    AccessRightToAnonymousLoanApplication = newHub.AccessRightToAnonymousLoanApplication,
                    AccessRighttoapprovecustomerloan = newHub.AccessRighttoapprovecustomerloan,
                    AccessRighttocreateahub = newHub.AccessRighttocreateahub,
                    AccessRighttocreateateammember = newHub.AccessRighttocreateateammember,
                    AccessRighttocreatetenure = newHub.AccessRighttocreatetenure,
                    AccessRighttodisablecustomerstoapplyforaloan = newHub.AccessRighttodisablecustomerstoapplyforaloan,
                    AccessRighttodisablehubs = newHub.AccessRighttodisablehubs,
                    AccessRightToEditTeamMemberPermissions = newHub.AccessRightToEditTeamMemberPermissions,
                    AccessRightToExportDisbursementloan = newHub.AccessRightToEditTeamMemberPermissions,
                    AccessRighttoloansettings = newHub.AccessRighttoloansettings,
                    AccessRightToPrintLoan = newHub.AccessRightToPrintLoan,
                    AccessRightToProceedLoan = newHub.AccessRightToProceedLoan,
                    AccessRighttorejectaloan = newHub.AccessRighttorejectaloan,
                    AccessRighttoteamsAndpermissions = newHub.AccessRighttoteamsAndpermissions,
                    AccessRightToUploadBackDisbursementloan = newHub.AccessRightToUploadBackDISBURSEMENTLoan,
                    AccessRighttoviewveammembers = newHub.AccessRighttoviewveammembers,
                    AccessRightToViewDisbursementLoan = newHub.AccessRightToViewDisbursementLoan,
                    AccessRighttoviewcustomersloans = newHub.AccessRighttoviewcustomersloans,
                    AccessRightToUploadBackRepaymentLoan = newHub.AccessRightToUploadBackRepaymentLoan,
                    AccessRighttoviewcustomers = newHub.AccessRighttoviewcustomers,
                    AccessRighttoviewhubs = newHub.AccessRighttoviewcustomers,
                    AccessRighttoviewloandetails = newHub.AccessRighttoviewcustomers,
                    AccessRighttoviewtenure = newHub.AccessRighttoviewcustomers,
                    AccessRightToViewUploadBackRepaymentLoan = newHub.AccessRightToViewUploadBackRepaymentLoan,
                    CreateLoanNarration = newHub.CreateLoanNarration,
                    ViewLoanNarration = newHub.ViewLoanNarration
                };
                lapoLoanDB.HubTeams.Add(newHubTeamMember);
                await lapoLoanDB.SaveChangesAsync();

                switch (newHub.UserType)
                {
                    case "TEAM LEAD":
                        await this.ActivateManagerTeamLead(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;
                    case "RECONCILIATION AND ACCOUNT OFFICER":
                        await this.ActivateReconciliationOfficers(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;
                    case "DISBURSEMENT OFFICER":
                        await this.ActivateDisbusmentOfficers(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;

                    default:
                        break;
                }

                if (newHub.UserType == "TEAM LEADS")
                {

                }

                if (newHub.UserType == "RECONCILIATION AND ACCOUNT OFFICERS")
                {

                }

                string message2 = "Lapo : Your Staff ID has been added to Lapo loan application portal.";

                if (!string.IsNullOrEmpty(newHub.EnterPhoneNumber) && !string.IsNullOrWhiteSpace(newHub.EnterPhoneNumber))
                {
                    await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, newHub.EnterPhoneNumber, message2, 0);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "New team memeber has been added successfully.", true, "", "", Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }


        public async Task<RespondMessageDto> CreateHubMember(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, NewHubTeamMember newHub)
        {
            try
            {
                if (string.IsNullOrEmpty(newHub.EnterTeamMemberID) || string.IsNullOrWhiteSpace(newHub.EnterTeamMemberID))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Member is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //if (string.IsNullOrEmpty(newHub.EnterPhoneNumber) || string.IsNullOrWhiteSpace(newHub.EnterPhoneNumber))
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff Phone number is required", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (newHub.EnterPhoneNumber.Length < 11 || newHub.EnterPhoneNumber.Length > 11)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff Phone number must be 11 digit number", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (newHub.EnterEmailAddress.Length > 0 && !newHub.EnterEmailAddress.Contains("@"))
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff Email address is invalid", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (!newHub.EnterTeamMemberID.StartsWith("SN"))
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID must start with SN", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //var HasPermission = await lapoLoanDB.SecurityPermissions.Where(x => x.AccountId == newHub.CreatedByAccountId).FirstOrDefaultAsync();

                //if (HasPermission == null /*&& newHub.CreatedByAccountId != 34*/)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //if (HasPermission != null && HasPermission.HasPermissionToCreatedStaff.HasValue && HasPermission.HasPermissionToCreatedStaff.Value == false /*&& newHub.CreatedByAccountId != 34*/)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "You don't has permission to create new Staff ID", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                var UserExit = await lapoLoanDB.SecurityAccounts.Where(x => x.Username == newHub.EnterTeamMemberID).AnyAsync();

                var SecurityPerm = await lapoLoanDB.SecurityPermissions.Where(x => x.UserId == newHub.EnterTeamMemberID).AnyAsync();

                //var HubTeamG = await lapoLoanDB.HubTeamGroups.ToListAsync();

                //if (HubTeamG.Count > 0)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID is already exiting", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                // List<HubTeamSubGroup> Loanheaders = null;

                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                 var GroupId = lapoCipher01.DecryptString(newHub.SelectHubGroupId.ToString());

                var HubTeamGroups = await lapoLoanDB.HubTeams.Where(c => c.HubMemberId == newHub.EnterTeamMemberID || c.RefNo == newHub.EnterTeamMemberID).FirstOrDefaultAsync();

                if (HubTeamGroups != null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Member already exit.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                //var Peopl = await lapoLoanDB.People.Where(c => c.Staff == newHub.EnterTeamMemberID).FirstOrDefaultAsync();

                //if (Peopl != null)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Member is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //var personDt = await lapoLoanDB.People.Where(c => c.EmailAddress == newHub.EnterEmailAddress  || c.PhoneNumber == newHub.EnterPhoneNumber || c.AltPhoneNumber == newHub.EnterPhoneNumber).FirstOrDefaultAsync();

                //if (personDt != null)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Email, Phone No, Alt Phone No is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //var SecurityDt = await lapoLoanDB.SecurityAccounts.Where(c => c.Username == newHub.EnterEmailAddress).FirstOrDefaultAsync();

                //if (SecurityDt != null)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Email Address is already exiting.", false, "", null, Status.Ërror, StatusMgs.Error);
                //}

                //var HubTeamsDt = await lapoLoanDB.HubTeams.Where(c => c.EmailAddress == newHub.EnterEmailAddress || c.PhoneNumber == newHub.EnterPhoneNumber ).FirstOrDefaultAsync();

                //if (HubTeamsDt != null)
                //{
                //     return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Email,       Phone No, Alt Phone No is already exiting.", false, "", null, Status.Ërror,    StatusMgs.Error);
                //}

                var Pwd = HashSalt.HashPassword("Admin123!!");

                var newAcct = new SecurityAccount()
                {
                    AccountType = AccountType.Adiministration,
                    AccountVerify = true,
                    AllowLoginTwoFactor = false,
                    CreatedDate = DateTime.Now,
                    Status = StatusMgs.Active,
                    Username = newHub.EnterTeamMemberID,
                    Password = Pwd,
                    LastLoginDate = DateTime.Now,
                };
                lapoLoanDB.SecurityAccounts.Add(newAcct);
                await lapoLoanDB.SaveChangesAsync();

                var newPeople = new Person()
                {
                    CreatedDate = DateTime.Now,
                    AccountId = newAcct.Id,
                    LastName = newHub.EnterLastName,
                    EmailAddress = newHub.EnterEmailAddress,
                    AltPhoneNumber = newHub.EnterPhoneNumber,
                    CurrentAddress = newHub.TeamMemberOfficeAddress,
                    Age = "25",
                    PhoneNumber = newHub.EnterPhoneNumber,
                    MiddleName = newHub.EnterMiddleName,
                    Staff = newHub.EnterTeamMemberID,
                    PositionOrRole = AccountType.Adiministration,
                    Gender = "Male",
                    FirstName = newHub.EnterFirstName,
                };
                lapoLoanDB.People.Add(newPeople);
                await lapoLoanDB.SaveChangesAsync();

                var updateS = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == newAcct.Id).FirstOrDefaultAsync();
                if (updateS != null)
                {
                    updateS.PersonId = newPeople.Id;
                    lapoLoanDB.Entry(updateS).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                var newSecurityPer = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.SecurityPermission()
                {
                    UserId = newHub.EnterTeamMemberID,
                    StaffName = newPeople.FirstName + ",    " + newPeople.LastName,
                    AccountId = newAcct.Id,
                    Status = StatusMgs.Active,
                    BlockedDate = DateTime.Now,
                    ActivatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsBlocked = true,
                    HasPermissionToCreatedStaff = true,
                    HasPermissionToDisableStaff = true,
                    IsSupperAdmin = true,
                    AccessRightApprovedLoan = true,
                    TenureAccessRight = true,
                    LoanSettingAccessRight = true,
                    NetPaysAccessRight = true,
                    LoanCompletedAccessRight = true,
                    GeneralPermissionsAccessRight = true,
                    CustomerLoanPermission = true,
                };

                newSecurityPer.Status = StatusMgs.Active;
                lapoLoanDB.SecurityPermissions.Add(newSecurityPer);
                await lapoLoanDB.SaveChangesAsync();

                var newHubTeamMember = new HubTeam()
                {
                    CreatedDate = DateTime.Now,
                    HubMemberOtherNames = newHub.EnterMiddleName,
                    HubMemberLastNames = newHub.EnterLastName,
                    HubMemberFirstNames = newHub.EnterFirstName,
                    UpdatedDate = DateTime.Now,
                    CreatedByAccountId = newHub.CreatedByAccountId,
                    IsBlocked = false,
                    Status = StatusMgs.Active,
                    HubMemberId = newHub.EnterTeamMemberID,
                    TeamAccountId = updateS.Id,
                    GroupId = long.Parse(GroupId),
                    BackEndUsers = newHub.UserType,
                    RefNo = newHub.EnterTeamMemberID,
                    AccessRightToAnonymousLoanApplication = newHub.AccessRightToAnonymousLoanApplication,
                    AccessRighttoapprovecustomerloan = newHub.AccessRighttoapprovecustomerloan,
                    AccessRighttocreateahub = newHub.AccessRighttocreateahub,
                    AccessRighttocreateateammember = newHub.AccessRighttocreateateammember,
                    AccessRighttocreatetenure = newHub.AccessRighttocreatetenure,
                    AccessRighttodisablecustomerstoapplyforaloan = newHub.AccessRighttodisablecustomerstoapplyforaloan,
                    AccessRighttodisablehubs = newHub.AccessRighttodisablehubs,
                    AccessRightToEditTeamMemberPermissions = newHub.AccessRightToEditTeamMemberPermissions,
                    AccessRightToExportDisbursementloan = newHub.AccessRightToExportDISBURSEMENTLoan,
                    AccessRighttoloansettings = newHub.AccessRighttoloansettings,
                    AccessRightToPrintLoan = newHub.AccessRightToPrintLoan,
                    AccessRightToProceedLoan = newHub.AccessRightToProceedLoan,
                    AccessRighttorejectaloan = newHub.AccessRighttorejectaloan,
                    AccessRighttoteamsAndpermissions = newHub.AccessRighttoteamsAndpermissions,
                    AccessRightToUploadBackDisbursementloan = newHub.AccessRightToUploadBackDISBURSEMENTLoan,
                    AccessRighttoviewveammembers = newHub.AccessRighttoviewveammembers,
                    AccessRightToViewDisbursementLoan = newHub.AccessRightToViewDisbursementLoan,
                    AccessRighttoviewcustomersloans = newHub.AccessRighttoviewcustomersloans,
                    AccessRightToUploadBackRepaymentLoan = newHub.AccessRightToUploadBackRepaymentLoan,
                    AccessRighttoviewcustomers = newHub.AccessRighttoviewcustomers,
                    AccessRighttoviewhubs = newHub.AccessRighttoviewhubs,
                    AccessRighttoviewloandetails = newHub.AccessRighttoviewloandetails,
                    AccessRighttoviewtenure = newHub.AccessRighttoviewtenure,
                    AccessRightToViewUploadBackRepaymentLoan = newHub.AccessRightToViewUploadBackRepaymentLoan,
                    CreateLoanNarration = newHub.CreateLoanNarration,
                    ViewLoanNarration = newHub.ViewLoanNarration
                };
                lapoLoanDB.HubTeams.Add(newHubTeamMember);
                await lapoLoanDB.SaveChangesAsync();

                switch (newHub.UserType)
                {
                    case "TEAM LEAD":
                        await this.ActivateManagerTeamLead(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;
                    case "RECONCILIATION AND ACCOUNT OFFICER":

                        await this.ActivateReconciliationOfficers(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;

                    case "DISBURSEMENT OFFICER":

                        await this.ActivateDisbusmentOfficers(_environment, lapoCipher01.EnryptString(newHubTeamMember.Id), newHub.CreatedByAccountId.ToString());
                        break;

                    default:

                        break;
                }

                if (newHub.UserType == "TEAM LEADS")
                {

                }

                if (newHub.UserType == "RECONCILIATION AND ACCOUNT OFFICERS")
                {

                }

                string message2 = "Lapo : Your Staff ID has been added to Lapo loan application portal.";

                if (!string.IsNullOrEmpty(newHub.EnterPhoneNumber) && !string.IsNullOrWhiteSpace(newHub.EnterPhoneNumber))
                {
                    await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, newHub.EnterPhoneNumber, message2, 0);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Team member has been added successfully.", true, "", "", Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
        public async Task<RespondMessageDto> ExitNewHubTeamMember(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, EditHubTeamMember newHub)
        {
            try
            {
                if (string.IsNullOrEmpty(newHub.SelectHubGroupId) || string.IsNullOrWhiteSpace(newHub.SelectHubGroupId))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Staff ID is required", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var GroupId = long.Parse(newHub.SelectHubGroupId);

                var HasPermission = await lapoLoanDB.HubTeams.Where(x => x.Id == GroupId).FirstOrDefaultAsync();

                if (HasPermission == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Member not found", false, "Member not found", null, Status.Ërror, StatusMgs.Error);
                }

                if (HasPermission != null)
                {
                    HasPermission.BackEndUsers = newHub.UserType;
                    HasPermission.GroupId = long.Parse(lapoCipher01.DecryptString(newHub.SelectHubGroupIdd));
                    HasPermission.AccessRightToAnonymousLoanApplication = newHub.AccessRightToAnonymousLoanApplication;
                    HasPermission.AccessRighttoapprovecustomerloan = newHub.AccessRighttoapprovecustomerloan;
                    HasPermission.AccessRighttocreateahub = newHub.AccessRighttocreateahub;
                    HasPermission.AccessRighttocreateateammember = newHub.AccessRighttocreateateammember;
                    HasPermission.AccessRighttocreatetenure = newHub.AccessRighttocreatetenure;
                    HasPermission.AccessRighttodisablecustomerstoapplyforaloan = newHub.AccessRighttodisablecustomerstoapplyforaloan;
                    HasPermission.AccessRighttodisablehubs = newHub.AccessRighttodisablehubs;
                    HasPermission.AccessRightToEditTeamMemberPermissions = newHub.AccessRightToEditTeamMemberPermissions;
                    HasPermission.AccessRightToExportDisbursementloan = newHub.AccessRightToExportDISBURSEMENTLoan;
                    HasPermission.AccessRighttoloansettings = newHub.AccessRighttoloansettings;
                    HasPermission.AccessRightToPrintLoan = newHub.AccessRightToPrintLoan;
                    HasPermission.AccessRightToProceedLoan = newHub.AccessRightToProceedLoan;
                    HasPermission.AccessRighttorejectaloan = newHub.AccessRighttorejectaloan;
                    HasPermission.AccessRighttoteamsAndpermissions = newHub.AccessRighttoteamsAndpermissions;
                    HasPermission.AccessRightToUploadBackDisbursementloan = newHub.AccessRightToUploadBackDISBURSEMENTLoan;
                    HasPermission.AccessRighttoviewveammembers = newHub.AccessRighttoviewveammembers;
                    HasPermission.AccessRightToViewDisbursementLoan = newHub.AccessRightToViewDisbursementLoan;
                    HasPermission.AccessRighttoviewcustomersloans = newHub.AccessRighttoviewcustomersloans;
                    HasPermission.AccessRightToUploadBackRepaymentLoan = newHub.AccessRightToUploadBackRepaymentLoan;
                    HasPermission.AccessRighttoviewcustomers = newHub.AccessRighttoviewcustomers;
                    HasPermission.AccessRighttoviewhubs = newHub.AccessRighttoviewhubs;
                    HasPermission.AccessRighttoviewloandetails = newHub.AccessRighttoviewloandetails;
                    HasPermission.AccessRighttoviewtenure = newHub.AccessRighttoviewtenure;
                    HasPermission.AccessRightToViewUploadBackRepaymentLoan = newHub.AccessRightToViewUploadBackRepaymentLoan;
                    HasPermission.CreateLoanNarration = newHub.CreateLoanNarration;
                    HasPermission.ViewLoanNarration = newHub.ViewLoanNarration;

                    lapoLoanDB.Entry(HasPermission).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Permission updated successfully.", true, "", "", Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Please check internet connection and try again.", false, "Please check internet connection and try again.", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetHubTeamMemberLists(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<HubTeam> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                Loanheaders = await lapoLoanDB.HubTeams.Where(s => s.Status == "Active").ToListAsync();

                //var dataResult = await GetHubTeams();

                //try
                //{
                //    if (dataResult.IsActive && dataResult != null)
                //    {
                //        Loanheaders = dataResult.Data as List<HubTeam>;
                //    }
                //    else
                //    {
                //        Loanheaders = await lapoLoanDB.HubTeams.Where(s => s.Status == "Active").ToListAsync();
                //    }
                //}
                //catch(Exception ex)
                //{
                //    Loanheaders = await lapoLoanDB.HubTeams.Where(s => s.Status == "Active").ToListAsync();
                //}

                //Loanheaders = await lapoLoanDB.HubTeams.Where(s => s.Status == "Active" && !s.HubMemberFirstNames.StartsWith("Olorunmo")).ToListAsync();


                if (pagenationFilter.IsSearchBar)
                {
                    double Amount = 0;

                    try
                    {
                        Amount = Convert.ToDouble(pagenationFilter.searchText);
                    }
                    catch (Exception exs)
                    {

                    }

                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubMemberFirstNames.StartsWith(pagenationFilter.searchText) || n.HubMemberOtherNames.StartsWith(pagenationFilter.searchText) || n.HubMemberLastNames.StartsWith(pagenationFilter.searchText) || n.HubMemberId.StartsWith(pagenationFilter.searchText) /*|| n.EmailAddress.StartsWith(pagenationFilter.searchText) || n.PhoneNumber.StartsWith(pagenationFilter.searchText)*/)).ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubMemberFirstNames.StartsWith(pagenationFilter.searchText) || n.HubMemberOtherNames.StartsWith(pagenationFilter.searchText) || n.HubMemberLastNames.StartsWith(pagenationFilter.searchText) || n.HubMemberId.StartsWith(pagenationFilter.searchText) /*|| n.EmailAddress.StartsWith(pagenationFilter.searchText) || n.PhoneNumber.StartsWith(pagenationFilter.searchText)*/) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)).ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                    }
                }

                var HubTeamsAppList = new List<LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamMemberModel>();
                string Names ="" , Names1 ="", GName ="";
                bool IsStatus = false;

                Loanheaders = Loanheaders.Where(s => s.HubMemberId != "SN0001"  && s.Status == StatusMgs.Active).ToList();

                if (Loanheaders == null || Loanheaders.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub Teams found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                foreach (var itm in Loanheaders)
                {
                    try
                    {
                        var name = await lapoLoanDB.People.Where(a => a.AccountId == itm.CreatedByAccountId).FirstOrDefaultAsync();
                        if (name != null)
                        {
                            Names = name.FirstName + "  " + name.LastName;
                        }
                        else
                        {
                            Names = "";
                        }

                         name = await lapoLoanDB.People.Where(a => a.AccountId == itm.TeamAccountId).FirstOrDefaultAsync();

                        if (name != null)
                        {
                            Names1 = name.FirstName + "  " + name.LastName;
                        }
                        else
                        {
                            Names1 = "";
                        }

                        var gName = await lapoLoanDB.HubTeamGroups.Where(a => a.Id == itm.GroupId).FirstOrDefaultAsync();

                        if (gName != null)
                        {
                            GName = gName.HubTeamGroupName;
                        }
                        else
                        {
                            GName = "";
                        }

                        var gName1 = await lapoLoanDB.HubTeamManagers.Where(a => a.HubTeamsId == itm.Id && a.Status == StatusMgs.Active && a.HubTeamSubGroupId == itm.GroupId).FirstOrDefaultAsync();

                        if (gName1 != null)
                        {
                            IsStatus = true;
                        }
                        else
                        {
                            IsStatus = false;
                        }

                        var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(a => a.IsGroupHead.HasValue && a.IsGroupHead.Value).FirstOrDefaultAsync();

                        //if (!GName.ToLower().Contains("Public Sector Office - from domi account".ToLower()) && !Names1.ToLower().Contains("Olorunmo Ojo".ToLower()) && !Names1.ToLower().StartsWith("Olorunmo Ojo".ToLower()) && !Names1.ToLower().StartsWith("Olorunmo".ToLower())  && !GName.ToLower().Contains("Public Sector Office".ToLower()) && !GName.ToLower().StartsWith("Public Sector Office".ToLower()))
                        //{
                            no += 1;

                            var newHubTeamApp1 = new LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamMemberModel()
                            {
                                CreatedByName = Names,
                                CreatedDate = itm.CreatedDate.Value.ToLongDateString(),
                                Status = itm.Status,
                                Name = Names1,
                                PhoneNumber = HubTeamGroup == null ? "09088776655" : HubTeamGroup.PhoneNumber,
                                EmailAddress = HubTeamGroup == null ? "lap@lap.ng.org" : HubTeamGroup.EmailAddress,
                                Id = lapoCipher01.EnryptString(itm.Id),
                                No = no.ToString(),
                                GroupName = GName,
                                Role = itm.BackEndUsers,
                                HideEditButton = false,
                            };

                            if (pagenationFilter.AccountId == itm.TeamAccountId)
                            {
                                newHubTeamApp1.HideEditButton = true;
                            }

                            if (IsStatus == true)
                            {
                                newHubTeamApp1.Role = "Team Lead";
                            }

                            HubTeamsAppList.Add(newHubTeamApp1);
                        //}
                    }
                    catch (Exception er)
                    {

                    }
                }

                if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
                {
                    var Pagenation = await new PagenationsHelper().GetPagenation<HubTeamMemberModel>(HubTeamsAppList, pagenationFilter);

                    var Data = new { Pagenation = Pagenation, Hub_Name = "" };

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Teams App Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAllBanksNameLists(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        {
            try
            {
                List<BsBankName> Loanheaders = null;
                int no = 0;

                Loanheaders = await lapoLoanDB.BsBankNames.ToListAsync();

                if (Loanheaders == null || Loanheaders.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub Teams found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeamsAppList = new List<BankAndCodeModel>();
                string Names = "", Names1 = "", GName = "";

                foreach (var itm in Loanheaders)
                {
                    try
                    {
                        no += 1;

                        var newHubTeamApp1 = new BankAndCodeModel()
                        {
                             Name = itm.BankName,
                              Id = itm.BankShortCode
                        };

                        HubTeamsAppList.Add(newHubTeamApp1);
                    }
                    catch (Exception er)
                    {

                    }
                }

                if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Teams App Applications retrieved successfully.", true, HubTeamsAppList, HubTeamsAppList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAllPermission(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string Id)
        {
            try
            {
                var no = Convert.ToInt64(lapoCipher01.DecryptString(Id));

                if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrEmpty(Id))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Team Memeber Id is required.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeam = await lapoLoanDB.HubTeams.Where(u => u.Id == no).FirstOrDefaultAsync();

                var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(a => a.IsGroupHead.HasValue && a.IsGroupHead.Value).FirstOrDefaultAsync();

                if (HubTeam != null)
                {
                    var NewHubTeamMembe = new NewHubTeamMember()
                    {
                        EnterPhoneNumber = HubTeamGroup == null ? "09088776655" : HubTeamGroup.PhoneNumber,
                        EnterEmailAddress = HubTeamGroup == null ? "lap@lap.ng.org" : HubTeamGroup.EmailAddress,
                        EnterMiddleName = HubTeam.HubMemberOtherNames,
                        EnterFirstName = HubTeam.HubMemberFirstNames,
                        EnterLastName = HubTeam.HubMemberLastNames,
                        TeamMemberOfficeAddress = HubTeam.Id.ToString(),
                        UserType = HubTeam.BackEndUsers,
                        SelectHubGroupIdd = lapoCipher01.EnryptString(HubTeam.GroupId.Value.ToString()),
                    };

                    try
                    {
                        NewHubTeamMembe. AccessRightToAnonymousLoanApplication = HubTeam.AccessRightToAnonymousLoanApplication.Value;
                        NewHubTeamMembe.AccessRighttoapprovecustomerloan = HubTeam.AccessRighttoapprovecustomerloan.Value;
                       NewHubTeamMembe.AccessRighttocreateahub = HubTeam.AccessRighttocreateahub.Value;
                       NewHubTeamMembe.AccessRighttocreateateammember = HubTeam.AccessRighttocreateateammember.Value;
                        NewHubTeamMembe.AccessRighttocreatetenure = HubTeam.AccessRighttocreatetenure.Value;
                        NewHubTeamMembe.AccessRighttodisablecustomerstoapplyforaloan = HubTeam.AccessRighttodisablecustomerstoapplyforaloan.Value;
                       NewHubTeamMembe.AccessRighttodisablehubs = HubTeam.AccessRighttodisablehubs.Value;
                       NewHubTeamMembe.AccessRightToEditTeamMemberPermissions = HubTeam.AccessRightToEditTeamMemberPermissions.Value;
                       NewHubTeamMembe.AccessRightToExportDISBURSEMENTLoan = HubTeam.AccessRightToExportDisbursementloan.Value;
                       NewHubTeamMembe.AccessRighttoloansettings = HubTeam.AccessRighttoloansettings.Value;
                        NewHubTeamMembe.AccessRightToPrintLoan = HubTeam.AccessRightToPrintLoan.Value;
                        NewHubTeamMembe.AccessRightToProceedLoan = HubTeam.AccessRightToProceedLoan.Value;
                        NewHubTeamMembe.AccessRighttorejectaloan = HubTeam.AccessRighttorejectaloan.Value;
                       NewHubTeamMembe.AccessRighttoteamsAndpermissions = HubTeam.AccessRighttoteamsAndpermissions.Value;
                       NewHubTeamMembe.AccessRighttoviewtenure = HubTeam.AccessRighttoviewtenure.Value;
                        NewHubTeamMembe.AccessRighttoviewloandetails = HubTeam.AccessRighttoviewloandetails.Value;
                       NewHubTeamMembe.AccessRightToUploadBackDISBURSEMENTLoan = HubTeam.AccessRightToUploadBackDisbursementloan.Value;
                       NewHubTeamMembe.AccessRightToUploadBackRepaymentLoan = HubTeam.AccessRightToUploadBackRepaymentLoan.Value;
                        NewHubTeamMembe.AccessRighttoviewcustomers = HubTeam.AccessRighttoviewcustomers.Value;
                        NewHubTeamMembe.AccessRighttoviewcustomersloans = HubTeam.AccessRighttoviewcustomersloans.Value;
                       NewHubTeamMembe.AccessRightToViewDisbursementLoan = HubTeam.AccessRightToViewDisbursementLoan.Value;
                        NewHubTeamMembe.AccessRighttoviewhubs = HubTeam.AccessRighttoviewhubs.Value;
                       NewHubTeamMembe.AccessRightToViewUploadBackRepaymentLoan = HubTeam.AccessRightToViewUploadBackRepaymentLoan.Value;
                        NewHubTeamMembe.AccessRighttoviewveammembers = HubTeam.AccessRighttoviewveammembers.Value;
                       NewHubTeamMembe.ViewLoanNarration = HubTeam.ViewLoanNarration.Value;
                        NewHubTeamMembe.CreateLoanNarration = HubTeam.CreateLoanNarration.Value;
                    }
                    catch(Exception ex)
                    {

                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Teams App Applications retrieved successfully.", true, NewHubTeamMembe, NewHubTeamMembe, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetHubTeamMemberByGroupLists(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<HubTeam> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

                var AppId = long.Parse(lapoCipher01.DecryptString(pagenationFilter.AppId));

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                Loanheaders = await lapoLoanDB.HubTeams.Where(g=>g.GroupId == AppId).ToListAsync();

                if (Loanheaders == null || Loanheaders.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub Teams found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (pagenationFilter.IsSearchBar)
                {
                    double Amount = 0;

                    try
                    {
                        Amount = Convert.ToDouble(pagenationFilter.searchText);
                    }
                    catch (Exception exs)
                    {

                    }

                  

                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => n.GroupId == AppId  && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubMemberFirstNames.StartsWith(pagenationFilter.searchText) || n.HubMemberOtherNames.StartsWith(pagenationFilter.searchText) || n.HubMemberLastNames.StartsWith(pagenationFilter.searchText) || n.HubMemberId.StartsWith(pagenationFilter.searchText) /*|| n.EmailAddress.StartsWith(pagenationFilter.searchText) || n.PhoneNumber.StartsWith(pagenationFilter.searchText)*/)).ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => n.GroupId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubMemberFirstNames.StartsWith(pagenationFilter.searchText) || n.HubMemberOtherNames.StartsWith(pagenationFilter.searchText) || n.HubMemberLastNames.StartsWith(pagenationFilter.searchText) || n.HubMemberId.StartsWith(pagenationFilter.searchText) /*|| n.EmailAddress.StartsWith(pagenationFilter.searchText) || n.PhoneNumber.StartsWith(pagenationFilter.searchText)*/) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => n.GroupId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate)).ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => n.GroupId == AppId && (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status.Equals(pagenationFilter.status)).ToListAsync();
                    }
                }

                var HubTeamsAppList = new List<LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamMemberRolesModel>();

                string Names = "", Names1 = "", GName = "";
                bool IsStatus = false, IsReconStatus = false, IsDisburs = false;

                foreach (var itm in Loanheaders)
                {
                    try
                    {
                        no += 1;

                        var name = await lapoLoanDB.People.Where(a => a.AccountId == itm.TeamAccountId).FirstOrDefaultAsync();
                        if (name != null)
                        {
                            Names = name.FirstName + "  " + name.LastName;
                        }
                        else
                        {
                            Names = "";
                        }

                        var gName = await lapoLoanDB.HubTeamManagers.Where(a => a.HubTeamsId == itm.Id    && a.Status ==  StatusMgs.Active  && a.HubTeamSubGroupId == itm.GroupId).FirstOrDefaultAsync();

                        if (gName != null)
                        {
                            IsStatus = true;
                        }
                        else
                        {
                            IsStatus = false;
                        }

                        var HubTeamRecon = await lapoLoanDB.HubTeamReconciliationOfficers.Where(a => a.HubTeamsId == itm.Id && a.Status == StatusMgs.Active && a.HubTeamSubGroupId == itm.GroupId).FirstOrDefaultAsync();

                        if (HubTeamRecon != null)
                        {
                            IsReconStatus = true;
                        }
                        else
                        {
                            IsReconStatus = false;
                        }

                        var HubTeamsDisburs = await lapoLoanDB.HubTeamsDisbursmentOfficers.Where(a => a.HubTeamId == itm.Id && a.Status == StatusMgs.Active && a.HubGroupId == itm.GroupId).FirstOrDefaultAsync();

                        if (HubTeamsDisburs != null)
                        {
                            IsDisburs = true;
                        }
                        else
                        {
                            IsDisburs = false;
                        }

                        var newHubTeamApp1 = new LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamMemberRolesModel()
                        {
                            CreatedDate = itm.CreatedDate.Value.ToLongDateString(),
                            Status = itm.Status,
                            No = no.ToString(),
                            IsTeamLead = IsStatus == true ? "Team Lead" : "",
                            IsReconciliationOfficers = IsReconStatus ? "Reconciliation Officer" : "",
                            Name = Names,
                            Id = lapoCipher01.EnryptString(itm.Id),    
                            Role = itm.BackEndUsers
                        };

                        if (IsStatus == true)
                        {
                            newHubTeamApp1.Role = "Team Lead";
                        }

                        HubTeamsAppList.Add(newHubTeamApp1);
                    }
                    catch (Exception er)
                    {

                    }
                }

                if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
                {
                    var Pagenation = await new PagenationsHelper().GetPagenation<HubTeamMemberRolesModel>(HubTeamsAppList, pagenationFilter);

                    var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(g => g.Id == AppId).FirstOrDefaultAsync();

                    if (HubTeamGroup == null)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data found.", false, "", null, Status.Ërror, StatusMgs.Error);
                    }

                    var Data = new { Pagenation  = Pagenation , Hub_Name = HubTeamGroup.HubTeamGroupName };

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Teams App Applications retrieved successfully.", true, Data, Data, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetHubTeamsDisbursmentOfficers(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, PagenationFilterDto pagenationFilter)
        {
            try
            {
                List<HubTeam> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                var fromDate = Convert.ToDateTime(pagenationFilter.dateFrom);
                var toDate = Convert.ToDateTime(pagenationFilter.dateTo);

               // var AppId = long.Parse(lapoCipher01.DecryptString(pagenationFilter.AppId));

                if (fromDate > toDate)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                Loanheaders = await lapoLoanDB.HubTeams.ToListAsync();

                if (Loanheaders == null || Loanheaders.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub Teams found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }


                if (pagenationFilter.IsSearchBar)
                {
                    double Amount = 0;

                    try
                    {
                        Amount = Convert.ToDouble(pagenationFilter.searchText);
                    }
                    catch (Exception exs)
                    {

                    }

                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubMemberFirstNames.StartsWith(pagenationFilter.searchText) || n.HubMemberOtherNames.StartsWith(pagenationFilter.searchText) || n.HubMemberLastNames.StartsWith(pagenationFilter.searchText) || n.HubMemberId.StartsWith(pagenationFilter.searchText) /*|| n.EmailAddress.StartsWith(pagenationFilter.searchText) || n.PhoneNumber.StartsWith(pagenationFilter.searchText)*/) && n.BackEndUsers == "DISBURSEMENT OFFICER").ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && (n.HubMemberFirstNames.StartsWith(pagenationFilter.searchText) || n.HubMemberOtherNames.StartsWith(pagenationFilter.searchText) || n.HubMemberLastNames.StartsWith(pagenationFilter.searchText) || n.HubMemberId.StartsWith(pagenationFilter.searchText) /*|| n.EmailAddress.StartsWith(pagenationFilter.searchText) || n.PhoneNumber.StartsWith(pagenationFilter.searchText)*/) && n.Status.Equals(pagenationFilter.status)  && n.BackEndUsers == "DISBURSEMENT OFFICER").ToListAsync();
                    }
                }
                else
                {
                    if (pagenationFilter.status.Equals("All"))
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.BackEndUsers == "DISBURSEMENT OFFICER").ToListAsync();
                    }
                    else
                    {
                        Loanheaders = await lapoLoanDB.HubTeams.Where(n => (n.CreatedDate >= fromDate && n.CreatedDate <= toDate) && n.Status.Equals(pagenationFilter.status) && n.BackEndUsers == "DISBURSEMENT OFFICER").ToListAsync();
                    }
                }

                var HubTeamsAppList = new List<LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamMemberRolesModel>();

                string Names = "", Names1 = "", GName = "" , HubTeamGroupName="", NamesC ="";
                bool IsStatus = false, IsReconStatus = false, IsDisburs = false;

                foreach (var itm in Loanheaders)
                {
                    try
                    {
                        no += 1;

                        var name = await lapoLoanDB.People.Where(a => a.AccountId == itm.TeamAccountId).FirstOrDefaultAsync();
                        if (name != null)
                        {
                            Names = name.FirstName + "  " + name.LastName;
                        }
                        else
                        {
                            Names = "";
                        }

                         name = await lapoLoanDB.People.Where(a => a.AccountId == itm.CreatedByAccountId).FirstOrDefaultAsync();
                        if (name != null)
                        {
                            NamesC = name.FirstName + "  " + name.LastName;
                        }
                        else
                        {
                            NamesC = "";
                        }

                        //var gName = await lapoLoanDB.HubTeamManagers.Where(a => a.HubTeamsId == itm.Id && a.Status == StatusMgs.Active && a.HubTeamSubGroupId == itm.GroupId).FirstOrDefaultAsync();

                        //if (gName != null)
                        //{
                        //    IsStatus = true;
                        //}
                        //else
                        //{
                        //    IsStatus = false;
                        //}

                        var HubTeamsDisburs = await lapoLoanDB.HubTeamsDisbursmentOfficers.Where(a => a.HubTeamId == itm.Id && a.Status == StatusMgs.Active && a.HubGroupId == itm.GroupId).FirstOrDefaultAsync();

                        if (HubTeamsDisburs != null)
                        {
                            IsDisburs = true;
                        }
                        else
                        {
                            IsDisburs = false;
                        }

                        var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(a => a.Id == itm.GroupId).FirstOrDefaultAsync();

                        if (HubTeamsDisburs != null)
                        {
                            HubTeamGroupName = HubTeamGroup.HubTeamGroupName;
                        }
                        else
                        {
                            HubTeamGroupName = "";
                        }

                       var HubTeamGroup1 = await lapoLoanDB.HubTeamGroups.Where(a => a.IsGroupHead.HasValue && a.IsGroupHead.Value).FirstOrDefaultAsync();

                        var newHubTeamApp1 = new LapoLoanWebApi.HubTeams.HubTeamModel.HubTeamMemberRolesModel()
                        {
                            CreatedDate = itm.CreatedDate.Value.ToLongDateString(),
                            Status = itm.Status,
                            No = no.ToString(),
                            IsTeamLead = IsDisburs == true ? "Disbursment Officers" : "",
                            IsReconciliationOfficers = "",
                            Name = Names,
                            Id = lapoCipher01.EnryptString(itm.Id),
                            Email = HubTeamGroup1 == null ? "" : HubTeamGroup1.EmailAddress,
                            Tel = HubTeamGroup1 == null ? "" : HubTeamGroup1.PhoneNumber,
                            Role = itm.BackEndUsers,
                            Hub = HubTeamGroupName,
                            CreatedByName = NamesC,
                            CreatedBy = NamesC
                        };

                        HubTeamsAppList.Add(newHubTeamApp1);
                    }
                    catch (Exception er)
                    {

                    }
                }

                if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
                {
                    var Pagenation = await new PagenationsHelper().GetPagenation<HubTeamMemberRolesModel>(HubTeamsAppList, pagenationFilter);

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Teams App Applications retrieved successfully.", true, Pagenation, Pagenation, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data found.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateManagerTeamLead(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AppsId, string AcctId)
        {
            try
            {
                var AppId = long.Parse(lapoCipher01.DecryptString(AppsId));

                var HubTeam = await lapoLoanDB.HubTeams.Where(c => c.Id == AppId).FirstOrDefaultAsync();

                if(HubTeam == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Hub Teams Member not found", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeamGroups = await lapoLoanDB.HubTeamManagers.Where(c => c.HubTeamSubGroupId == HubTeam.GroupId).ToListAsync();

                foreach (var group in HubTeamGroups)
                {
                    group.Status = StatusMgs.NotActive;
                    group.RemovedDate = DateTime.Now;
                    group.RemovedByAccountId = long.Parse(AcctId);
                    lapoLoanDB.Entry(group).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                var HubTeamManager = await lapoLoanDB.HubTeamManagers.Where(c => c.HubTeamsId == HubTeam.Id).FirstOrDefaultAsync();

                if(HubTeamManager == null)
                {
                    var hubManager = new HubTeamManager()
                    {
                        RemovedDate = DateTime.Now,
                        CreatedDate = DateTime.Now,
                        CreatedByAccountId = long.Parse(AcctId),
                        IsReconciliationOfficers = false,
                        RemovedByAccountId = long.Parse(AcctId),
                        HubTeamSubGroupId = HubTeam.GroupId,
                        HubTeamsId = HubTeam.Id,
                        Status = StatusMgs.Active,
                    };
                    lapoLoanDB.HubTeamManagers.Add(hubManager);
                    await lapoLoanDB.SaveChangesAsync();
                }
                else
                {
                    HubTeamManager.CreatedDate = DateTime.Now;
                    HubTeamManager.HubTeamSubGroupId = HubTeam.GroupId;
                    HubTeamManager.HubTeamsId = HubTeam.Id;
                    HubTeamManager.Status = StatusMgs.Active;
                    lapoLoanDB.Entry(HubTeamManager).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Teams Member has been activated as Team Lead to this Hub", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateReconciliationOfficers(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AppsId, string AcctId)
        {
            try
            {
                var AppId = long.Parse(lapoCipher01.DecryptString(AppsId));

                var HubTeam = await lapoLoanDB.HubTeams.Where(c => c.Id == AppId).FirstOrDefaultAsync();

                if (HubTeam == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Hub Teams Member not found", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeamGroups = await lapoLoanDB.HubTeamReconciliationOfficers.Where(c => c.HubTeamSubGroupId == HubTeam.GroupId).ToListAsync();

                foreach (var group in HubTeamGroups)
                {
                    group.Status = StatusMgs.NotActive;
                    group.RemovedDate = DateTime.Now;
                    group.RemovedByAccountId = long.Parse(AcctId);
                    lapoLoanDB.Entry(group).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                var HubTeamManager = await lapoLoanDB.HubTeamReconciliationOfficers.Where(c => c.HubTeamsId == HubTeam.Id).FirstOrDefaultAsync();

                if (HubTeamManager == null)
                {
                    var hubManager = new HubTeamReconciliationOfficer()
                    {
                        RemovedDate = DateTime.Now,
                        CreatedDate = DateTime.Now,
                        CreatedByAccountId = long.Parse(AcctId),
                        RemovedByAccountId = long.Parse(AcctId),
                        HubTeamSubGroupId = HubTeam.GroupId,
                        HubTeamsId = HubTeam.Id,
                        Status = StatusMgs.Active,
                    };
                    lapoLoanDB.HubTeamReconciliationOfficers.Add(hubManager);
                    await lapoLoanDB.SaveChangesAsync();
                }
                else
                {
                    HubTeamManager.CreatedDate = DateTime.Now;
                    HubTeamManager.HubTeamSubGroupId = HubTeam.GroupId;
                    HubTeamManager.HubTeamsId = HubTeam.Id;
                    HubTeamManager.Status = StatusMgs.Active;
                    lapoLoanDB.Entry(HubTeamManager).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Teams Member has been activated as Reconciliation Officers to this Hub", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> ActivateDisbusmentOfficers(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AppsId, string AcctId)
        {
            try
            {
                var AppId = long.Parse(lapoCipher01.DecryptString(AppsId));

                var HubTeam = await lapoLoanDB.HubTeams.Where(c => c.Id == AppId).FirstOrDefaultAsync();

                if (HubTeam == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Hub Teams Member not found", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeamGroups = await lapoLoanDB.HubTeamsDisbursmentOfficers.Where(c => c.HubGroupId == HubTeam.GroupId).ToListAsync();

                foreach (var group in HubTeamGroups)
                {
                    group.Status = StatusMgs.NotActive;
                    group.LastDisbursmentDate = DateTime.Now;
                    lapoLoanDB.Entry(group).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                var HubTeamManager = await lapoLoanDB.HubTeamsDisbursmentOfficers.Where(c => c.HubTeamId == HubTeam.Id && c.HubGroupId == HubTeam.GroupId).FirstOrDefaultAsync();

                if (HubTeamManager == null)
                {
                    var hubManager = new HubTeamsDisbursmentOfficer()
                    {
                        
                        CreatedDate = DateTime.Now,
                        CreatedByAccountId = long.Parse(AcctId),
                        HubGroupId = HubTeam.GroupId,
                         HubTeamId = HubTeam.Id,
                          LastDisbursmentDate = DateTime.Now,
                        Status = StatusMgs.Active,
                    };
                    lapoLoanDB.HubTeamsDisbursmentOfficers.Add(hubManager);
                    await lapoLoanDB.SaveChangesAsync();
                }
                else
                {
                    HubTeamManager.CreatedDate = DateTime.Now;
                    HubTeamManager.Status = StatusMgs.Active;
                    lapoLoanDB.Entry(HubTeamManager).State = EntityState.Modified;
                    await lapoLoanDB.SaveChangesAsync();
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Teams Member has been activated as Reconciliation Officers to this Hub", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CheckIfOfficerMember1(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string FirstName, string OtherName, string Ref)
        {
            try
            {
                var HubTeam = await lapoLoanDB.HubTeams.Where(c => /*(c.HubMemberFirstNames == FirstName && c.HubMemberLastNames == OtherName) ||*/ (c.RefNo == Ref || c.RefNo == FirstName || c.RefNo == OtherName || c.HubMemberId == OtherName || c.HubMemberId == Ref) && c.BackEndUsers == "RELATIONSHIP OFFICER").FirstOrDefaultAsync();

                if (HubTeam != null)
                {
                    if(HubTeam.Status != StatusMgs.Active)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Relationship Officer is not active", false, "", null, Status.Ërror, StatusMgs.Error);
                    }
                   
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Relationship Officer was found", true, "", null, Status.Ërror, StatusMgs.Error);
                }

                //var HubTeam12 = await lapoLoanDB.People.Where(c =>  (c.Staff == Ref || c.Staff == FirstName || c.Staff == OtherName || c.Staff == OtherName || c.Staff == Ref)).FirstOrDefaultAsync();

                //if (HubTeam12 != null)
                //{
                //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Relationship Officer was found", true, "", null, Status.Ërror, StatusMgs.Error);
                //}

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Kindly enter a Relationship Officer", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> AllReconcilationMembers(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string AccountId, string AppId)
        {
            try
            {
                List<HubTeam> Loanheaders = null;
                int no = 0;
                var LoanAppList = new List<LoanAppListModelDto>();

                Loanheaders = await lapoLoanDB.HubTeams.Where(a => a.Status == StatusMgs.Active  && a.BackEndUsers == "RECONCILIATION AND ACCOUNT OFFICER").ToListAsync();

                if (Loanheaders == null || Loanheaders.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Hub Teams found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var HubTeamsAppList = new List<LapoLoanWebApi.HubTeams.HubTeamModel.HubTeams>();
                string Names = "";

                foreach (var itm in Loanheaders)
                {
                    try
                    {
                        no += 1;

                        var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(a => a.Id == itm.GroupId).FirstOrDefaultAsync();
                        if (HubTeamGroup != null)
                        {
                            var newHubTeamApp1 = new LapoLoanWebApi.HubTeams.HubTeamModel.HubTeams()
                            {
                                HubTeamGroupId = itm.GroupId.Value.ToString(),
                                TeamId = itm.Id.ToString(),
                                HubTeamName = itm.HubMemberFirstNames + "  " + itm.HubMemberLastNames,
                                HubTeamGroupName = HubTeamGroup.HubTeamGroupName
                            };

                            HubTeamsAppList.Add(newHubTeamApp1);
                        }
                    }
                    catch (Exception er)
                    {

                    }
                }

                if (HubTeamsAppList != null && (HubTeamsAppList.Count >= 0 || HubTeamsAppList.Count <= 0))
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Hub Teams App Applications retrieved successfully.", true, HubTeamsAppList, HubTeamsAppList, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No data retrieved.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
    }
}