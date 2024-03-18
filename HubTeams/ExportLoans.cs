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
using System.Data.SqlClient;
using OfficeOpenXml;
using LapoLoanWebApi.LoanScheduled;
using LapoLoanWebApi.LoanScheduled.Model;
using System.Data.OleDb;
using System.Data;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Data.Entity.Validation;
using System.Configuration;

namespace LapoLoanWebApi.HubTeams
{
    public sealed class ExportLoansActivity
    {
        private ControllerBase controllerBase { get; set; }

        private LapoLoanDBContext lapoLoanDB = null;
        private LapoCipher02 lapoCipher02 { get; set; }
        private LapoCipher02 ReallapoCipher02 { get; set; }
        private LapoCipher00 lapoCipher00 { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }

        private UploadDisbursmentLoans UploadDisbursmentLoans { get; set; }
        private UploadRepaymentSheduled UploadRepaymentSheduled { get; set; }

        private IConfiguration _configuration;

        public ExportLoansActivity(ControllerBase controllerBase, IConfiguration _configuration)
        {
            this._configuration = _configuration;
            this.lapoCipher01 = new LapoCipher01();
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.controllerBase = controllerBase;
            this.lapoCipher00 = new LapoCipher00();
            this.ReallapoCipher02 = new LapoCipher02();
            this.lapoCipher02 = new LapoCipher02();
            UploadDisbursmentLoans = new UploadDisbursmentLoans(this.controllerBase, this._configuration);
            UploadRepaymentSheduled = new UploadRepaymentSheduled(this.controllerBase, this._configuration);
        }

        public async Task<RespondMessageDto> ExportExcelData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, FormBoard viewModel)
        {
            List<ApprovedLoanReportData> results = new List<ApprovedLoanReportData>();

            try
            {
                if (viewModel.Status == "All" || string.IsNullOrEmpty(viewModel.Status) || string.IsNullOrWhiteSpace(viewModel.Status))
                {
                    viewModel.Status = StatusMgs.Approved;
                }

                var StartDate = Convert.ToDateTime(viewModel.StartDate);
                var EndDate = Convert.ToDateTime(viewModel.EndDate);

                if (StartDate > EndDate)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Date from can not be greater than date to.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Lapo_Approved_Loan = "Lapo_Approved_Loan" + StartDate.ToLongDateString().Replace(" ", "") + EndDate.ToLongDateString().Replace(" ", "") + ".xlsx";

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";

                string HtmlfullPath = Path.Combine(currentDirectory, path, Lapo_Approved_Loan);

                this.lapoLoanDB = new LapoLoanDBContext(this._configuration);

                var LoanApplicals = await lapoLoanDB.LoanApplicationRequestHeaders.Where(a => a.Status == viewModel.Status && (a.RequestDate.Value >= StartDate && a.RequestDate.Value <= EndDate)).ToListAsync();

                if (viewModel.HasMarkedExportLoans)
                {
                    foreach (var kvp in viewModel.MarkedExportLoans)
                    {
                        try
                        {
                            var Id = long.Parse(lapoCipher01.DecryptString(kvp.LoanId));

                            var AcctId = long.Parse(kvp.ExportedBy.ToString());

                            var LoanApplica = await lapoLoanDB.LoanApplicationRequestHeaders.Where(a => a.Id == Id).FirstOrDefaultAsync();

                            var LoanApplicaDetail = await lapoLoanDB.LoanApplicationRequestDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                            var LoanApplicaKycDetail = await lapoLoanDB.KycDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                            var SecurityAcct = await lapoLoanDB.SecurityAccounts.Where(a => a.Id == AcctId).FirstOrDefaultAsync();

                            var Person = await lapoLoanDB.People.Where(a => a.AccountId == SecurityAcct.Id).FirstOrDefaultAsync();

                            var Person1 = await lapoLoanDB.People.Where(a => a.AccountId == LoanApplica.AccountId).FirstOrDefaultAsync();

                            var BankInfo = await lapoLoanDB.BsBankNames.Where(a => a.BankName.ToLower() == LoanApplicaDetail.BankAccount.ToLower()).FirstOrDefaultAsync();

                            var newLoanScheduler = await new LoanSchedulerHelpers(this.controllerBase, this._configuration).CalculateScheduledLoanAmount(_environment, new ScheduledMethod()
                            {
                                IPPISNumber = LoanApplica.Pfnumber,
                                Amount = (double)LoanApplicaDetail.Amount.Value,
                                AccountId = LoanApplica.AccountId.ToString(),
                                Tenure = lapoCipher01.EnryptString(LoanApplica.LoanTenureId.Value.ToString())
                            });

                            if (Person1 != null && newLoanScheduler != null && Person != null && SecurityAcct != null && LoanApplicaKycDetail != null && LoanApplicaDetail != null && kvp != null && BankInfo != null)
                            {
                                var HubTeam = await lapoLoanDB.HubTeams.Where(a => (/*(a.HubMemberFirstName == LoanApplica.TeamOfficerFirstname && a.HubMemberOtherName == LoanApplica.TeamOfficerOthername)  ||*/  a.RefNo == LoanApplica.RelationshipOfficerRef) && lapoLoanDB.HubTeamGroups.Where(h => h.Id == a.GroupId).Any()).FirstOrDefaultAsync();

                                if(HubTeam != null)
                                {
                                    var newApprovedRptData = new ApprovedLoanReportData()
                                    {
                                        Bank_Account_Name = LoanApplicaDetail.BankAccountName,
                                        Bank_Name = LoanApplicaDetail.BankAccount,
                                        Bank_Account_Number = LoanApplicaDetail.BankAccountNumber,
                                        Reconciliation_Officer_Hub_Group = LoanApplica.TeamGroundName,
                                        Exported_By = Person.FirstName + " " + Person.LastName,

                                        LoanTenure = Convert.ToString(newLoanScheduler.DataLoad.TeunerName),

                                        Loan_Request_Amount = Convert.ToString(newLoanScheduler.DataLoad.DisbursmentAmount),

                                        Loan_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.ScheduledList[0].LoanAmountWithCurrency),

                                        LoanInterest = Convert.ToString(newLoanScheduler.DataLoad.Interest),

                                        Total_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.TotalAmount),

                                        Request_Date = Convert.ToString(LoanApplica.CreatedDate.Value.ToLongDateString()),

                                        Request_Code = Convert.ToString(LoanApplica.RequestCode),

                                        Approved_Date = Convert.ToString(LoanApplica.ApprovedDate.Value.ToLongDateString()),

                                        Reconciliation_Officer_Hub_Staff_ID = Convert.ToString(HubTeam.HubMemberId),

                                        CustomerName = Person1.FirstName + "    " + Person1.MiddleName + "   " + Person1.LastName,

                                        Bank_Code = BankInfo.BankShortCode
                                    };

                                    results.Add(newApprovedRptData);
                                }
                                else
                                {
                                    var newApprovedRptData = new ApprovedLoanReportData()
                                    {
                                        Bank_Account_Name = LoanApplicaDetail.BankAccountName,
                                        Bank_Name = LoanApplicaDetail.BankAccount,
                                        Bank_Account_Number = LoanApplicaDetail.BankAccountNumber,
                                        Reconciliation_Officer_Hub_Group = LoanApplica.TeamGroundName,
                                        Exported_By = Person.FirstName + " " + Person.LastName,

                                        LoanTenure = Convert.ToString(newLoanScheduler.DataLoad.TeunerName),

                                        Loan_Request_Amount = Convert.ToString(newLoanScheduler.DataLoad.DisbursmentAmount),

                                        Loan_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.ScheduledList[0].LoanAmountWithCurrency),

                                        LoanInterest = Convert.ToString(newLoanScheduler.DataLoad.Interest),

                                        Total_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.TotalAmount),

                                        Request_Date = Convert.ToString(LoanApplica.CreatedDate.Value.ToLongDateString()),

                                        Request_Code = Convert.ToString(LoanApplica.RequestCode),

                                        Approved_Date = Convert.ToString(LoanApplica.ApprovedDate.Value.ToLongDateString()),

                                        Reconciliation_Officer_Hub_Staff_ID = "Public Sector Team",

                                        CustomerName = Person1.FirstName + "    " + Person1.MiddleName + "   " + Person1.LastName,

                                        Bank_Code = BankInfo.BankShortCode
                                    };

                                    results.Add(newApprovedRptData);
                                }
                            }

                            LoanApplica.ExportedDate = DateTime.Now;
                            LoanApplica.ExportedById = AcctId;
                            lapoLoanDB.Entry(LoanApplica).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                else
                {
                    LoanApplicals.Clear();

                    var AcctId = long.Parse(viewModel.ExportedBy.ToString());

                    LoanApplicals = await lapoLoanDB.LoanApplicationRequestHeaders.Where(a => a.Status == viewModel.Status && (a.RequestDate.Value >= StartDate && a.RequestDate.Value <= EndDate)).ToListAsync();

                    foreach (var kvp in LoanApplicals)
                    {
                        try
                        {
                            var LoanApplicaDetail = await lapoLoanDB.LoanApplicationRequestDetails.Where(a => a.LoanAppRequestHeaderId == kvp.Id).FirstOrDefaultAsync();

                            var LoanApplicaKycDetail = await lapoLoanDB.KycDetails.Where(a => a.LoanAppRequestHeaderId == kvp.Id).FirstOrDefaultAsync();

                            var SecurityAcct = await lapoLoanDB.SecurityAccounts.Where(a => a.Id == AcctId).FirstOrDefaultAsync();

                            var BankInfo = await lapoLoanDB.BsBankNames.Where(a => a.BankName.ToLower() == LoanApplicaDetail.BankAccount.ToLower()).FirstOrDefaultAsync();

                            var Person = await lapoLoanDB.People.Where(a => a.AccountId == SecurityAcct.Id).FirstOrDefaultAsync();

                            var Person1 = await lapoLoanDB.People.Where(a => a.AccountId == kvp.AccountId).FirstOrDefaultAsync();

                          
                            var newLoanScheduler = await new LoanSchedulerHelpers(this.controllerBase, this._configuration).CalculateScheduledLoanAmount(_environment, new ScheduledMethod()
                            {
                                IPPISNumber = kvp.Pfnumber,
                                Amount = (double)LoanApplicaDetail.Amount.Value,
                                AccountId = kvp.AccountId.ToString(),
                                Tenure = lapoCipher01.EnryptString(kvp.LoanTenureId.Value.ToString())
                            });

                            if (newLoanScheduler != null && Person1 != null && Person != null && SecurityAcct != null && LoanApplicaKycDetail != null && LoanApplicaDetail != null && kvp != null && BankInfo != null)
                            {
                                var HubTeam = await lapoLoanDB.HubTeams.Where(a => (/*(a.HubMemberFirstName == kvp.TeamOfficerFirstname && a.HubMemberOtherName == kvp.TeamOfficerOthername)  ||*/ a.RefNo == kvp.RelationshipOfficerRef) && lapoLoanDB.HubTeamGroups.Where(h => h.Id == a.GroupId).Any()).FirstOrDefaultAsync();

                                if(HubTeam != null)
                                {
                                    var newApprovedRptData = new ApprovedLoanReportData()
                                    {
                                        Bank_Account_Name = LoanApplicaDetail.BankAccountName,

                                        Bank_Name = LoanApplicaDetail.BankAccount,

                                        Bank_Account_Number = LoanApplicaDetail.BankAccountNumber,

                                        Reconciliation_Officer_Hub_Group = kvp.TeamGroundName,

                                        Exported_By = Person.FirstName + " " + Person.LastName,

                                        LoanTenure = Convert.ToString(newLoanScheduler.DataLoad.TeunerName),

                                        Loan_Request_Amount = Convert.ToString(newLoanScheduler.DataLoad.DisbursmentAmount),

                                        Loan_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.ScheduledList[0].LoanAmountWithCurrency),

                                        LoanInterest = Convert.ToString(newLoanScheduler.DataLoad.Interest),

                                        Total_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.TotalAmount),

                                        Request_Date = kvp.CreatedDate.Value.ToLongDateString(),

                                        Request_Code = kvp.RequestCode,

                                        Approved_Date = kvp.ApprovedDate.Value.ToLongDateString(),

                                        Reconciliation_Officer_Hub_Staff_ID = HubTeam.HubMemberId,

                                        CustomerName = Person1.FirstName + "    " + Person1.MiddleName + "   " + Person1.LastName,

                                        Bank_Code = BankInfo.BankShortCode
                                    };
                                 
                                    results.Add(newApprovedRptData);
                                }
                                else
                                {
                                    var newApprovedRptData = new ApprovedLoanReportData()
                                    {
                                        Bank_Account_Name = LoanApplicaDetail.BankAccountName,

                                        Bank_Name = LoanApplicaDetail.BankAccount,

                                        Bank_Account_Number = LoanApplicaDetail.BankAccountNumber,

                                        Reconciliation_Officer_Hub_Group = kvp.TeamGroundName,

                                        Exported_By = Person.FirstName + " " + Person.LastName,

                                        LoanTenure = Convert.ToString(newLoanScheduler.DataLoad.TeunerName),

                                        Loan_Request_Amount = Convert.ToString(newLoanScheduler.DataLoad.DisbursmentAmount),

                                        Loan_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.ScheduledList[0].LoanAmountWithCurrency),

                                        LoanInterest = Convert.ToString(newLoanScheduler.DataLoad.Interest),

                                        Total_Repayment_Amount = Convert.ToString(newLoanScheduler.DataLoad.TotalAmount),

                                        Request_Date = kvp.CreatedDate.Value.ToLongDateString(),

                                        Request_Code = kvp.RequestCode,

                                        Approved_Date = kvp.ApprovedDate.Value.ToLongDateString(),

                                        Reconciliation_Officer_Hub_Staff_ID = "Public Sector Team",

                                        CustomerName = Person1.FirstName + "    " + Person1.MiddleName + "   " + Person1.LastName,

                                        Bank_Code = BankInfo.BankShortCode
                                    };

                                    results.Add(newApprovedRptData);
                                }
                              
                            }

                            kvp.ExportedDate = DateTime.Now;
                            kvp.ExportedById = AcctId;
                            lapoLoanDB.Entry(kvp).State = EntityState.Modified;
                            await lapoLoanDB.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                if (results == null || results.Count <= 0)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "No Approved Loan Report found.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                string worksheetsName = "Lapo_Approved_Loan";

                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                DateTime StaticDate = DateTime.Now;
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage pack = new ExcelPackage();
                ExcelWorksheet wrkSht = pack.Workbook.Worksheets.Add(worksheetsName);

                // wrkSht.Cells["A1"].LoadFromDataTable(dt, true);

                wrkSht.Cells["A" + 1].Value = "Request_Code"; //1
                wrkSht.Cells["B" + 1].Value = "Customer Name"; //1
                wrkSht.Cells["C" + 1].Value = "Request_Date"; //2
                wrkSht.Cells["D" + 1].Value = "Approved_Date"; //3
                wrkSht.Cells["E" + 1].Value = "Loan_Tenure"; //4
                wrkSht.Cells["F" + 1].Value = "Reconciliation_Officer_Staff_ID"; //5
                wrkSht.Cells["G" + 1].Value = "Loan_Interest"; //6
                wrkSht.Cells["H" + 1].Value = "Export_Date"; //7
                wrkSht.Cells["I" + 1].Value = "Reconciliation_Officer_Hub_Group"; //8
                wrkSht.Cells["J" + 1].Value = "Bank_Name"; //9

                wrkSht.Cells["K" + 1].Value = "Bank_Account_Name"; //10
                wrkSht.Cells["L" + 1].Value = "Bank_Account_Number"; //11
                wrkSht.Cells["M" + 1].Value = "Loan_Request_Amount"; //12
                wrkSht.Cells["N" + 1].Value = "Loan_Repayment_Amount"; //13

                wrkSht.Cells["O" + 1].Value = "Total_Repayment_Amount"; // 14
                wrkSht.Cells["P" + 1].Value = "Exported_By"; // 15
                wrkSht.Cells["Q" + 1].Value = "BANK CODE"; // 15

                int count = 1;
                foreach (var row in results)
                {
                    count += 1;

                    wrkSht.Cells["A" + count].Value = row.Request_Code; //1
                    wrkSht.Cells["B" + count].Value = row.CustomerName; //1

                    wrkSht.Cells["C" + count].Value = row.Request_Date; //2
                    wrkSht.Cells["D" + count].Value = row.Approved_Date; //3
                    wrkSht.Cells["E" + count].Value = row.LoanTenure; // 4
                    wrkSht.Cells["F" + count].Value = row.Reconciliation_Officer_Hub_Staff_ID; //5

                    wrkSht.Cells["G" + count].Value = row.LoanInterest; //6
                    wrkSht.Cells["H" + count].Value = DateTime.Now.ToLongDateString(); //7
                    wrkSht.Cells["I" + count].Value = row.Reconciliation_Officer_Hub_Group; //8
                    wrkSht.Cells["J" + count].Value = row.Bank_Name; //9

                    wrkSht.Cells["K" + count].Value = row.Bank_Account_Name; // 10
                    wrkSht.Cells["L" + count].Value = row.Bank_Account_Number; // 11
                    wrkSht.Cells["M" + count].Value = row.Loan_Request_Amount; //12
                    wrkSht.Cells["N" + count].Value = row.Loan_Repayment_Amount; //13

                    wrkSht.Cells["O" + count].Value = row.Total_Repayment_Amount; //14
                    wrkSht.Cells["P" + count].Value = row.Exported_By; //15
                    wrkSht.Cells["Q" + count].Value = row.Bank_Code; //15
                }

                pack.SaveAs(HtmlfullPath);

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "No Hub Teams found.", true, pack, pack, Status.Success, StatusMgs.Success);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> UploadApprovedExcelData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string fullPath)
        {
            try
            {
                var ExtractedLoans = await new UploadDisbursmentLoans(this.controllerBase, this._configuration).LoadExcelData(_environment, fullPath);

                FileLogActivities.CallSevice(" ","Start Looping"," ");

                var SucessFull = 0;
                var AlreadyDisbusedFull = 0;

                if (ExtractedLoans.IsActive == true)
                {
                    var listExtractedLoans = ExtractedLoans.DataLoad as List<UploadDisbursmentLoanModel>;

                    foreach (var item in listExtractedLoans)
                    {
                        try
                        {
                            var code = (string)Convert.ToString(item.Request_Code);

                            var LoanApplica = await lapoLoanDB.LoanApplicationRequestHeaders.Where(a => a.RequestCode == code).FirstOrDefaultAsync();

                            if (LoanApplica != null && LoanApplica.Status == StatusMgs.Approved)
                            {
                                FileLogActivities.CallSevice(" ", "List Extracted Approved Loans", " ");

                                    if (item.Disbursement_Date.Add(DateTime.Now.TimeOfDay) < LoanApplica.CreatedDate || item.Disbursement_Date.Add(DateTime.Now.TimeOfDay) < LoanApplica.ApprovedDate)
                                    {
                                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Disbursed Date is invalid.", false, null, null, Status.Success, StatusMgs.Success);
                                    }

                                    var LoanApplicaReqDetail = await lapoLoanDB.LoanApplicationRequestDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                                    if (Convert.ToDouble(LoanApplicaReqDetail.Amount.Value) < item.Disbursed_Amount || item.Disbursed_Amount > Convert.ToDouble(LoanApplicaReqDetail.Amount.Value))
                                    {
                                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Disbursed Amount is invalid.", false, null, null, Status.Success, StatusMgs.Success);
                                    }

                                    FileLogActivities.CallSevice(" ", "Disbursed loans about to add successfully. ", " ");

                                try
                                {
                                    var codeMember = (string)Convert.ToString(item.Disbursement_Officer_Staff_ID);

                                    var HubTeamsLoanApplica = await lapoLoanDB.HubTeams.Where(a => a.HubMemberId == codeMember).FirstOrDefaultAsync();

                                    var SecurityAccount = await lapoLoanDB.SecurityAccounts.Where(a => a.Id == HubTeamsLoanApplica.TeamAccountId.Value).FirstOrDefaultAsync();

                                    if (HubTeamsLoanApplica != null && SecurityAccount != null)
                                    {
                                        LoanApplica.DisbursementById = SecurityAccount.Id;
                                    }
                                }
                                catch (Exception exx)
                                {

                                }

                                    LoanApplica.DisbursementDate = item.Disbursement_Date;
                                    LoanApplica.DisbursementAmount = (decimal)item.Disbursed_Amount;

                                    LoanApplica.Status = item.Status.ToString();

                                    lapoLoanDB.Entry(LoanApplica).State = EntityState.Modified;
                                    await lapoLoanDB.SaveChangesAsync();

                                    FileLogActivities.CallSevice(" ", "Add loans about to add successfully. ", " ");

                                    try
                                    {
                                        var loanSettings1 = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();
                                        var LoanApplicationReq = await lapoLoanDB.KycDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                                        var PeopleReq = await lapoLoanDB.People.Where(a => a.AccountId == LoanApplica.AccountId).FirstOrDefaultAsync();

                                        var SecurityReq = await lapoLoanDB.SecurityAccounts.Where(a => a.Id == LoanApplica.AccountId).FirstOrDefaultAsync();

                                        if (loanSettings1 != null && LoanApplicationReq != null && PeopleReq != null && SecurityReq != null)
                                        {
                                            if (loanSettings1.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings1.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.Value)
                                            {
                                                string messageemail = "PS-Loans: " + "Your loan request has been disbursed into your bank account, Please confirm.";

                                                string subjectemail = "PS-Loans: " + "Your loan has been disbursed successfully.";

                                                await new EmailHelpers(this._configuration).SendDisburstmentLoanAppEmail(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, SecurityReq.Username, new DefalutToken(_configuration).ClientLogin(), messageemail, subjectemail);
                                            }

                                            string message1 = "PS-Loans : Your loan request has been disbursed into your bank account, Please confirm.";

                                            if (LoanApplicationReq != null && !string.IsNullOrEmpty(LoanApplicationReq.PhoneNumber) && !string.IsNullOrWhiteSpace(LoanApplicationReq.PhoneNumber) && loanSettings1.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings1.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.Value)
                                            {
                                                await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, LoanApplicationReq.PhoneNumber, message1, 0);
                                            }
                                        }

                                        var loanSettings = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                                       var  HubTeamsLoanApplica = await lapoLoanDB.HubTeams.Where(a => /*(a.HubMemberFirstName == LoanApplica.TeamOfficerFirstname && a.HubMemberLastName == LoanApplica.TeamOfficerOthername) ||*/ a.RefNo == LoanApplica.RelationshipOfficerRef).FirstOrDefaultAsync();

                                        if (loanSettings != null && HubTeamsLoanApplica != null)
                                        {
                                            var HubGroup = await lapoLoanDB.HubTeamGroups.Where(a => a.IsGroupHead.Value).FirstOrDefaultAsync();

                                            if (HubGroup != null && loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.Value && HubTeamsLoanApplica != null && !string.IsNullOrEmpty(HubGroup.EmailAddress) && !string.IsNullOrWhiteSpace(HubGroup.EmailAddress))
                                            {
                                                string messageemail = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + " loan request has been disbursed into bank account.";

                                                string subjectemail = "PS-Loans: " + "Customer loan has been disbursed successfully.";

                                                await new EmailHelpers(this._configuration).SendDisburstmentLoanAppEmail(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, HubGroup.EmailAddress, new DefalutToken(_configuration).ClientLogin(), messageemail, subjectemail);
                                            }

                                            string message2 = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + " loan request has been disbursed into your bank account.";

                                            if (HubGroup != null && HubTeamsLoanApplica != null && !string.IsNullOrEmpty(HubGroup.PhoneNumber) && !string.IsNullOrWhiteSpace(HubGroup.PhoneNumber) && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.Value)
                                            {
                                                await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, HubGroup.PhoneNumber, message2, 0);
                                            }
                                        }

                                    }
                                    catch (Exception ex) 
                                    {
                                    
                                    
                                    }

                                SucessFull += 1;
                            }
                            else if (LoanApplica != null && LoanApplica.Status == StatusMgs.Disbursed)
                            {
                                AlreadyDisbusedFull += 1;
                            }
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var sb = new StringBuilder();
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    sb.AppendLine(string.Format("Property: {0} Error: {1}",
                                    validationError.PropertyName, validationError.ErrorMessage));
                                }
                            }

                            //  throw new Exception(sb.ToString(), dbEx);

                            var ffaf = "";

                            try
                            {
                                ffaf= dbEx.Message ?? dbEx.InnerException.Message;
                            }
                            catch(Exception ex)
                            {

                            }

                            FileLogActivities.CallSevice(" ", "Start Looping Updating Database " + sb.ToString()  + ffaf, " ");
                        }
                    }

                    FileLogActivities.CallSevice(" ", "Disbursed loans has been uploaded successfully. ", " ");

                    string Messa = "";

                    if(AlreadyDisbusedFull > 0)
                    {
                        Messa = AlreadyDisbusedFull +  "  Disbursed loans has already been uploaded, ";
                    }

                    if(SucessFull > 0)
                    {
                        Messa += SucessFull + "  Disbursed loans has been uploaded successfully.";
                    }
                    else
                    {
                        Messa += "  No Disbursed loans uploaded.";
                    }

                    //if(SucessFull <= 0)
                    //{
                    //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, , true, null, null, Status.Success, StatusMgs.Success);
                    //}

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, Messa, true, null, null, Status.Success, StatusMgs.Success);
                }

                FileLogActivities.CallSevice(" ", "Disbursed loans has not been uploaded. ", " ");

                return ExtractedLoans;
            }
            catch (Exception ex)
            {
                FileLogActivities.CallSevice(" ", "Start Looping Updating Database " + ex.Message ?? ex.InnerException.Message, " ");

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> UploadRepaymentSheduledData(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, string fullPath)
        {
            try
            {
                var ExtractedLoans = await new UploadRepaymentSheduled(this.controllerBase, this._configuration).LoadExcelData(_environment, fullPath);

                var SucessFull = 0;
                var AlreadyDisbusedFull = 0;

                if (ExtractedLoans.IsActive == true)
                {
                    var listExtractedLoans = ExtractedLoans.DataLoad as List<UploadRepaymentLoanModel>;

                    foreach (var item in listExtractedLoans)
                    {
                        try
                        {
                            var code = (string)Convert.ToString(item.Loan_Request_Code);

                            var LoanApplica = await lapoLoanDB.LoanApplicationRequestHeaders.Where(a => a.RequestCode == code).FirstOrDefaultAsync();

                            if (LoanApplica != null && LoanApplica.Status == StatusMgs.Disbursed)
                            {
                                var codeMember = (string)Convert.ToString(item.Uploaded_By_Member_Staff_ID);

                                var HubTeamsLoanApplica = await lapoLoanDB.HubTeams.Where(a => a.HubMemberId == codeMember).FirstOrDefaultAsync();

                                var KycDetail = await lapoLoanDB.KycDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                                var LoanTenureSetting = await lapoLoanDB.LoanTenureSettings.Where(a => a.Id == LoanApplica.LoanTenureId).FirstOrDefaultAsync();

                                var LoanApplicationReq = await lapoLoanDB.LoanApplicationRequestDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                                if (KycDetail != null && LoanApplicationReq != null)
                                {
                                    // LoanApplica.DisbursementById = SecurityAccount.Id;
                                    // LoanApplica.DisbursementDate = item.Repayment_Date;
                                    // LoanApplica.DisbursementAmount = (decimal)item.Repayment_Balance;
                                    // LoanApplica.Status = item.Status.ToString();

                                    var newRepaymentAccount = new RepaymentLoan()
                                    {
                                        CreatedDate = DateTime.Now,
                                        CustomerName = /*KycDetail.FullName */ item.Customer_Name,
                                        LoanHeaderId = LoanApplica.Id,
                                        LoanRepaymentFor = item.Loan_Repayment_For,
                                        LoanRequestCode = code,
                                        RepaymentAmount = (decimal)item.Repayment_Amount,
                                        RepaymentBalance = (decimal)item.Repayment_Balance,
                                        Status = StatusMgs.Ongoing,
                                        RepaymentStatus = item.Repayment_Status,
                                        RepaymentDate = DateTime.Now,
                                        ServiceAccount = item.Service_Account,
                                        UploadedByMemberStaffId = item.Uploaded_By_Member_Staff_ID,
                                    };

                                    try
                                    {
                                      

                                        var SecurityAccount = await lapoLoanDB.SecurityAccounts.Where(a => a.Id == HubTeamsLoanApplica.TeamAccountId.Value).FirstOrDefaultAsync();

                                        if (HubTeamsLoanApplica != null && SecurityAccount != null)
                                        {
                                            newRepaymentAccount.CreatedAccountById = SecurityAccount.Id;
                                        }
                                    }
                                    catch(Exception ex)
                                    {

                                    }

                                    lapoLoanDB.RepaymentLoans.Add(newRepaymentAccount);
                                    await lapoLoanDB.SaveChangesAsync();

                                    var newLoanScheduler = await new LoanSchedulerHelpers(this.controllerBase, this._configuration).CalculateScheduledLoanAmount(_environment, new ScheduledMethod()
                                    {
                                        IPPISNumber = LoanApplica.Pfnumber,
                                        Amount = (double)LoanApplicationReq.Amount.Value,
                                        AccountId = LoanApplica.AccountId.ToString(),
                                        Tenure = lapoCipher01.EnryptString(LoanApplica.LoanTenureId.Value.ToString())
                                    });

                                    if (newLoanScheduler != null)
                                    {
                                        var LoanScheduleTotalAmt = newLoanScheduler.DataLoad.ScheduledList[0].TotalAmount;

                                        var oldrepayment = 0.00;

                                        var ExitingLoanReps = await lapoLoanDB.RepaymentLoans.Where(a => a.LoanHeaderId == LoanApplica.Id && a.LoanRequestCode == code).ToListAsync();

                                        if (ExitingLoanReps != null)
                                        {
                                            foreach (var itm in ExitingLoanReps)
                                            {
                                                oldrepayment += (double)itm.RepaymentAmount.Value;
                                            }
                                        }

                                        if (oldrepayment >= LoanScheduleTotalAmt)
                                        {
                                            var UpdateLoanRep = await lapoLoanDB.RepaymentLoans.Where(a => a.LoanHeaderId == LoanApplica.Id && a.LoanRequestCode == code).ToListAsync();

                                            try
                                            {
                                                var UpdateLoanReps = UpdateLoanRep[UpdateLoanRep.Count - 1];
                                                if (UpdateLoanReps != null && LoanApplica != null)
                                                {
                                                    UpdateLoanReps.Status = StatusMgs.Completed;
                                                    lapoLoanDB.Entry(UpdateLoanReps).State = EntityState.Modified;
                                                    await lapoLoanDB.SaveChangesAsync();

                                                    LoanApplica.Status = StatusMgs.Completed;
                                                    lapoLoanDB.Entry(LoanApplica).State = EntityState.Modified;
                                                    await lapoLoanDB.SaveChangesAsync();

                                                    var loanSettings1 = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                                                    var KycDetails = await lapoLoanDB.KycDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                                                    var PeopleReq = await lapoLoanDB.People.Where(a => a.AccountId == LoanApplica.AccountId).FirstOrDefaultAsync();

                                                    var SecurityReq = await lapoLoanDB.SecurityAccounts.Where(a => a.Id == LoanApplica.AccountId).FirstOrDefaultAsync();

                                                    if (loanSettings1 != null && KycDetails != null && PeopleReq != null && SecurityReq != null)
                                                    {
                                                        var amt = string.Format(new CultureInfo("ig-NG"), "{0:C}", (decimal)LoanScheduleTotalAmt).Replace(".00", "");

                                                        if (loanSettings1.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings1.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.Value)
                                                        {
                                                            string messageemail = "PS-Loans: " + "Your repayment loan  " + amt + "  has been completed.";
                                                            string subjectemail = "PS-Loans: " + "Your repayment loan has been credited.";
                                                            await new EmailHelpers(this._configuration).SendDisburstmentLoanAppEmail(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, SecurityReq.Username, new DefalutToken(_configuration).ClientLogin(), messageemail, subjectemail);
                                                        }

                                                        string message1 = "PS-Loans : Your repayment loan  " + amt + "  has been completed.";

                                                        if (KycDetails != null && !string.IsNullOrEmpty(KycDetails.PhoneNumber) && !string.IsNullOrWhiteSpace(KycDetails.PhoneNumber) && loanSettings1.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings1.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.Value)
                                                        {
                                                            await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, KycDetails.PhoneNumber, message1, 0);
                                                        }
                                                    }

                                                    var loanSettings = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                                                    //HubTeamsLoanApplica = await lapoLoanDB.HubTeams.Where(a => a.HubMemberFirstName == LoanApplica.TeamOfficerFirstname && a.HubMemberLastName == LoanApplica.TeamOfficerOthername).FirstOrDefaultAsync();

                                                    var HubTeamApplica = await lapoLoanDB.HubTeamGroups.Where(a => a.IsGroupHead.HasValue && a.IsGroupHead.Value == true).FirstOrDefaultAsync();

                                                    if (loanSettings != null && HubTeamApplica != null)
                                                    {
                                                        var amt = string.Format(new CultureInfo("ig-NG"), "{0:C}", (decimal)LoanScheduleTotalAmt).Replace(".00", "");

                                                        if (loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.Value && HubTeamApplica != null && !string.IsNullOrEmpty(HubTeamApplica.EmailAddress) && !string.IsNullOrWhiteSpace(HubTeamApplica.EmailAddress))
                                                        {
                                                            string messageemail = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + "  repayment loan  " + amt + "  has been completed.";

                                                            string subjectemail = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + "  repayment loan has been completed.";
                                                            //await new EmailHelpers(this._configuration).SendDisburstmentLoanAppEmail(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, HubTeamApplica.EmailAddress, DefalutToken.ClientLogin, messageemail, subjectemail);

                                                            await new EmailHelpers(this._configuration).SendRepaymentLoanApp(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, HubTeamApplica.EmailAddress, new DefalutToken(_configuration).ClientLogin(), messageemail, subjectemail, "Public Sector Team ", "₦" + (int)item.Repayment_Amount, "₦" + (int)item.Repayment_Balance, item.Loan_Repayment_For, "₦" + (int)LoanApplicationReq.Amount.Value, LoanTenureSetting == null || LoanTenureSetting.TeunerName == null ? item.Loan_Repayment_For : LoanTenureSetting.TeunerName);
                                                        }

                                                        string message2 = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + " repayment loan  " + amt + "  has been completed.";
                                                        if (HubTeamApplica != null && !string.IsNullOrEmpty(HubTeamApplica.PhoneNumber) && !string.IsNullOrWhiteSpace(HubTeamApplica.PhoneNumber) && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.Value)
                                                        {
                                                            await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, HubTeamApplica.PhoneNumber, message2, 0);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    LoanApplica.Status = StatusMgs.Disbursed;
                                                    lapoLoanDB.Entry(LoanApplica).State = EntityState.Modified;
                                                    await lapoLoanDB.SaveChangesAsync();
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        else
                                        {
                                            var loanSettings1 = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();
                                            var KycDetails = await lapoLoanDB.KycDetails.Where(a => a.LoanAppRequestHeaderId == LoanApplica.Id).FirstOrDefaultAsync();

                                            var PeopleReq = await lapoLoanDB.People.Where(a => a.AccountId == LoanApplica.AccountId).FirstOrDefaultAsync();

                                            var SecurityReq = await lapoLoanDB.SecurityAccounts.Where(a => a.Id == LoanApplica.AccountId).FirstOrDefaultAsync();

                                            if (loanSettings1 != null && KycDetails != null && PeopleReq != null && SecurityReq != null)
                                            {
                                                var amt = string.Format(new CultureInfo("ig-NG"), "{0:C}", (decimal)item.Repayment_Amount).Replace(".00", "");

                                                if (loanSettings1.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings1.SendEmailToAppliedCustomersWhenApplicationIsSubmitted.Value)
                                                {
                                                    string messageemail = "PS-Loans: " + "Your loan repayment " + amt + "  has been received.";

                                                    string subjectemail = "PS-Loans: Loan Repayment";

                                                    await new EmailHelpers(this._configuration).SendRepaymentLoanApp(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, SecurityReq.Username, new DefalutToken(_configuration).ClientLogin(), messageemail, subjectemail, item.Customer_Name, "₦" + (int)item.Repayment_Amount, "₦" + (int)item.Repayment_Balance, item.Loan_Repayment_For, "₦" + (int)LoanApplicationReq.Amount.Value, LoanTenureSetting == null || LoanTenureSetting.TeunerName == null ? item.Loan_Repayment_For : LoanTenureSetting.TeunerName);
                                                }

                                                string message1 = "PS-Loans : Your loan repayment " + amt + "  has been received.";

                                                if (KycDetails != null && !string.IsNullOrEmpty(KycDetails.PhoneNumber) && !string.IsNullOrWhiteSpace(KycDetails.PhoneNumber) && loanSettings1.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.HasValue && loanSettings1.SendSmsToAppliedCustomersWhenApplicationIsSubmitted.Value)
                                                {
                                                    await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, KycDetails.PhoneNumber, message1, 0);
                                                }
                                            }

                                            var loanSettings = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();

                                            //HubTeamsLoanApplica = await lapoLoanDB.HubTeamGroups.Where(a => a.HubMemberFirstName == LoanApplica.TeamOfficerFirstname && a.HubMemberLastName == LoanApplica.TeamOfficerOthername).FirstOrDefaultAsync();

                                            var HubTeamApplica = await lapoLoanDB.HubTeamGroups.Where(a => a.IsGroupHead.HasValue && a.IsGroupHead.Value == true).FirstOrDefaultAsync();

                                            if (loanSettings != null && HubTeamsLoanApplica != null)
                                            {
                                                var amt = string.Format(new CultureInfo("ig-NG"), "{0:C}", (decimal)item.Repayment_Amount).Replace(".00", "");

                                                if (loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendEmailToTeamLeadWhenApplicationIsSubmitted.Value && HubTeamApplica != null && !string.IsNullOrEmpty(HubTeamApplica.EmailAddress) && !string.IsNullOrWhiteSpace(HubTeamApplica.EmailAddress))
                                                {
                                                    string messageemail = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + "  repayment loan  " + amt + " has been credited.";
                                                    string subjectemail = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + "  repayment loan has been credited.";
                                                    //await new EmailHelpers(this._configuration).SendRepaymentLoanApp(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, HubTeamApplica.EmailAddress, DefalutToken.ClientLogin, messageemail, subjectemail);

                                                    await new EmailHelpers(this._configuration).SendRepaymentLoanApp(_environment, PeopleReq.FirstName, PeopleReq.MiddleName, HubTeamApplica.EmailAddress, new DefalutToken(_configuration).ClientLogin(), messageemail, subjectemail, "Public Sector Team ", "₦" + (int)item.Repayment_Amount, "₦" + (int)item.Repayment_Balance, item.Loan_Repayment_For, "₦" + (int)LoanApplicationReq.Amount.Value, LoanTenureSetting == null || LoanTenureSetting.TeunerName == null ? item.Loan_Repayment_For : LoanTenureSetting.TeunerName);
                                                }

                                                string message2 = "PS-Loans: " + PeopleReq.FirstName + "   " + PeopleReq.MiddleName + "  repayment loan  " + amt + "  has been credited.";

                                                if (HubTeamApplica != null && !string.IsNullOrEmpty(HubTeamApplica.PhoneNumber) && !string.IsNullOrWhiteSpace(HubTeamApplica.PhoneNumber) && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.HasValue && loanSettings.SendSmsToTeamLeadWhenApplicationIsSubmitted.Value)
                                                {
                                                    await new TwoFactorsHelper(this._configuration).SendSmsAsync(_environment, HubTeamApplica.PhoneNumber, message2, 0);
                                                }
                                            }
                                        }
                                    }
                                }


                                SucessFull += 1;
                            }
                            else if (LoanApplica != null && (LoanApplica.Status == StatusMgs.Ongoing || LoanApplica.Status == StatusMgs.Completed))
                            {
                                AlreadyDisbusedFull += 1;
                            }
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var sb = new StringBuilder();
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    sb.AppendLine(string.Format("Property: {0} Error: {1}",
                                    validationError.PropertyName, validationError.ErrorMessage));
                                }
                            }

                            //  throw new Exception(sb.ToString(), dbEx);

                            var ffaf = "";

                            try
                            {
                                ffaf = dbEx.Message ?? dbEx.InnerException.Message;
                            }
                            catch (Exception ex)
                            {

                            }

                            FileLogActivities.CallSevice(" ", "Start Looping Updating Database " + sb.ToString() + ffaf, " ");
                        }
                    }

                    string Messa = "";

                   

                    if (SucessFull > 0)
                    {
                        Messa += SucessFull + "  Repayment loan has been uploaded successfully..";
                    }
                    else
                    {
                        Messa = " The upload for repayment was unsuccessful. Please verify that the request codes match those in the exported file.";
                    }

                    //if (AlreadyDisbusedFull > 0)
                    //{
                    //    Messa = AlreadyDisbusedFull + "  Repayment loans has already been uploaded.";
                    //}


                    //if(SucessFull <= 0)
                    //{
                    //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, , true, null, null, Status.Success, StatusMgs.Success);
                    //}

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, Messa, true, null, null, Status.Success, StatusMgs.Success);
                }

                return ExtractedLoans;
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, new DefalutToken(_configuration).CheckInternetConnection(), false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
    }
}