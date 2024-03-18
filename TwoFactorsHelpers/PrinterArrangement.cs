using DocumentFormat.OpenXml.Drawing.Charts;
using Google.Protobuf.WellKnownTypes;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanScheduled;
using LapoLoanWebApi.LoanScheduled.Model;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data;
using ServiceStack;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class PrinterArrangement
    {
        private ControllerBase controllerBase { get; set; }

        private LapoLoanDBContext lapoLoanDB = null;

        private LapoCipher02 lapoCipher02 { get; set; }
        private LapoCipher02 ReallapoCipher02 { get; set; }
        private LapoCipher00 lapoCipher00 { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }

        private IConfiguration _configuration;
        public PrinterArrangement(ControllerBase controllerBase, IConfiguration configuration
)
        {
            this._configuration = configuration;
             this.lapoCipher01 = new LapoCipher01();
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.controllerBase = controllerBase;
            this.lapoCipher00 = new LapoCipher00();
            this.ReallapoCipher02 = new LapoCipher02();
            this.lapoCipher02 = new LapoCipher02();
        }

        public async Task<RespondMessageDto> SetrinterLoanApp(PrinterLoanAppModel printerLoan)
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";

                string HtmlfullPath = Path.Combine(currentDirectory, path, "PrintPreView.html");
                string HtmData = System.IO.File.ReadAllText(HtmlfullPath);

                string User_Id = new DefalutToken(_configuration).User_Id();

                //var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();
                var appId = lapoCipher01.DecryptString(printerLoan.loanDetailsData.appId);
                var PreviewData = await this.SetPrinterLoanApp(appId);

                long AppId = Convert.ToInt64(appId);

                var LoanHApp1 = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.Id == AppId).FirstOrDefaultAsync();

                if(LoanHApp1== null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "An error has occur,Loan application id is not found or Failed to arrange loan print out.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                var Peopl = await lapoLoanDB.People.Where(x => x.AccountId == LoanHApp1.AccountId).FirstOrDefaultAsync();

                if (Peopl == null || PreviewData == null || PreviewData.IsActive == false || PreviewData.DataLoad == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "An error has occur,Loan application id is not found or Failed to arrange loan print out.", false, "", null, Status.Ërror, StatusMgs.Error);
                }

                if (!string.IsNullOrEmpty(HtmData) && !string.IsNullOrWhiteSpace(HtmData))
                {
                    var scheduled = new ScheduledMethod()
                    {
                        AccountId = printerLoan.loanDetailsData.accountId.ToString(),
                        Amount = printerLoan.loanDetailsData.loanAmount,
                        IPPISNumber = printerLoan.loanDetailsData.pFNumber,
                        Tenure = printerLoan.loanDetailsData.ternor,
                    };

                    var Scheled = await new LoanSchedulerHelpers(null, this._configuration).CalculateScheduledLoanAmount(null, scheduled);

                    var DataLoad = PreviewData.DataLoad as PrintPreviewDto;

                    var bvn = "";

                    for(var i = 0; i <= DataLoad.BVN.Length -1; i++)
                    {
                        bvn += "<li>" + DataLoad.BVN[i] + "</li>";
                    }

                    var customerName = "";

                    for (var i = 0; i <= DataLoad.ClientInfo.First_Name.Length - 1; i++)
                    {
                        customerName += "<li>" + DataLoad.ClientInfo.First_Name[i] + "</li>";
                    }

                    customerName += "<li style=\"color:#fff\">" + "0" + "</li>";

                    for (var i = 0; i <= DataLoad.ClientInfo.Middle_Name.Length - 1; i++)
                    {
                        customerName += "<li>" + DataLoad.ClientInfo.Middle_Name[i] + "</li>";
                    }

                    customerName += "<li style=\"color:#fff\">" + "0" + "</li>";

                    for (var i = 0; i <= DataLoad.ClientInfo.Last_Name.Length - 1; i++)
                    {
                        customerName += "<li>" + DataLoad.ClientInfo.Last_Name[i] + "</li>";
                    }

                    var dateOfBirth = "";

                    for (var i = 0; i <= DataLoad.ClientInfo.DateOfBirth.Length - 1; i++)
                    {
                        dateOfBirth += "<li>" + DataLoad.ClientInfo.DateOfBirth[i] + "</li>";
                    }

                    var phoneNo = "";

                    for (var i = 0; i <= DataLoad.ClientInfo.Phone_Number.Length - 1; i++)
                    {
                        phoneNo += "<li>" + DataLoad.ClientInfo.Phone_Number[i] + "</li>";
                    }

                   
                    var marr = "";
                    var single = "";
                    var Divorced = "";

                    if (DataLoad.ClientInfo.Marital_Status == "Married")
                    {
                        Divorced = "<input type=\"checkbox\"  value=\"false\" />";
                        single = "<input type=\"checkbox\"  value=\"false\" />";
                        marr = "<input type=\"checkbox\" checked=\"checked\" value=\"true\" />";
                    }

                    if (DataLoad.ClientInfo.Marital_Status == "Single")
                    {
                        Divorced = "<input type=\"checkbox\"  value=\"false\" />";
                        single = "<input type=\"checkbox\" checked=\"checked\" value=\"true\" />";
                        marr = "<input type=\"checkbox\"  value=\"false\" />";
                    }

                    if (DataLoad.ClientInfo.Marital_Status == "Divorced")
                    {
                        Divorced = "<input type=\"checkbox\" checked=\"checked\" value=\"true\" />";
                        single = "<input type=\"checkbox\" value=\"false\" />";
                        marr = "<input type=\"checkbox\" value=\"false\" />";
                    }

                    var RequestAmt = "";

                    for (var i = 0; i <= DataLoad.ClientLoanDetail.LoanAmountInCurrency.Length - 1; i++)
                    {
                        RequestAmt += "<li>" + DataLoad.ClientLoanDetail.LoanAmountInCurrency[i] + "</li>";
                    }

                    var ApprovedAmt = "";

                    for (var i = 0; i <= DataLoad.ClientLoanDetail.ApprovedLoanAmountInCurrency.Length - 1; i++)
                    {
                        ApprovedAmt += "<li>" + DataLoad.ClientLoanDetail.ApprovedLoanAmountInCurrency[i] + "</li>";
                    }

                    var ApprovedDate = "";

                    for (var i = 0; i <= DataLoad.ClientLoan.DisbursmentDate.Length - 1; i++)
                    {
                        ApprovedDate += "<li>" + DataLoad.ClientLoan.DisbursmentDate[i] + "</li>";
                    }

                    //if(string.IsNullOrEmpty(ApprovedDate) || string.IsNullOrEmpty(ApprovedDate) || ApprovedDate == "")
                    //{

                    //}

                    var bankNo = "";

                    for (var i = 0; i <= DataLoad.ClientBank.BankAccountNumber.Length - 1; i++)
                    {
                        bankNo += "<li>" + DataLoad.ClientBank.BankAccountNumber[i] + "</li>";
                    }

                    //var bankNo = "";

                    //for (var i = 0; i <= DataLoad.ClientBank.BankAccountNumber.Length; i++)
                    //{
                    //    bankNo += "<li>" + DataLoad.ClientBank.BankAccountNumber[i] + "</li>";
                    //}



                    var HtmlResult = HtmData.Replace("{{profileimage}}", DataLoad.PassPortImage).Replace("{{BVN}}", bvn)
                        .Replace("{{Organization/OfficeAddress}}", DataLoad.OrganizationAndOffice).Replace("{{customerName}}", customerName).Replace("{{customerName}}", customerName).Replace("{{OracleID}}", "")
                        .Replace("{{HomeAddress}}", DataLoad.ClientInfo.Home_Address)
                        .Replace("{{Sex}}", DataLoad.ClientInfo.Gender)
                    .Replace("{{{{DateOfBirth}}}}", dateOfBirth)
                    .Replace("{{PHONENO}}", phoneNo)
                    .Replace("{{{{AltphoneNo}}}}", DataLoad.ClientInfo.Alt_Phone_Number).Replace("{{{{AltphoneNo}}}}", DataLoad.ClientInfo.Alt_Phone_Number)
                    .Replace("{{{{AltphoneNo}}}}", DataLoad.ClientInfo.Alt_Phone_Number)
                     .Replace("{{marr}}", marr)
                      .Replace("{{single}}", single)
                       .Replace("{{Divorced}}", Divorced)
                       .Replace("{{NEXTOFKININFORMATION}}", DataLoad.ClientNextOfKinInfo.Name)
                      .Replace("{{KinNAME}}", DataLoad.ClientNextOfKinInfo.Name)
                      .Replace("{{KinPHONENO}}", DataLoad.ClientNextOfKinInfo.Phone_Number)
                       .Replace("{{NextKinADDRESS}}", DataLoad.ClientNextOfKinInfo.Address)
                         .Replace("{{KinPHONENO}}", DataLoad.ClientNextOfKinInfo.Phone_Number)
                           .Replace("{{LOANDETAILS}}", "None")
                      .Replace("{{RequestAmt}}", RequestAmt)
                    .Replace("{{AMOUNTREQUESTEDINWORD}}", DataLoad.ClientLoanDetail.LoanAmountInWords)
                     .Replace("{{INTERESTRATE}}", DataLoad.ClientLoan.InterestRate + " %")
                     .Replace("{{{{ApprovedAmt}}}}", ApprovedAmt)
                    .Replace("{{AMOUNTREQUESTEDINWORD}}", DataLoad.ClientLoanDetail.ApprovedLoanAmountInWords)
                     .Replace("{{MONTHLYINSTALLMENTS}}", DataLoad.ClientLoan.MonthlyInstallmentAmount_Currency)
                    .Replace("{{LOANTENURE}}", DataLoad.ClientLoan.LoanTernure)
                    .Replace("{{MonthlyNet_Pay_Currency}}", DataLoad.ClientLoan.MonthlyNet_Pay_Currency)
                       .Replace("{{ACCOUNTNUMBER}}", bankNo)
                    .Replace("{{BANKNAME}}", DataLoad.ClientBank.BankAccount)
                     .Replace("{{ACCOUNTNAME}}", DataLoad.ClientBank.BankAccountName)
                     .Replace("{{PURPOSEOFLOAN}}", DataLoad.ClientLoan.Narration)
                      .Replace("{{ApprovedLoanAmountInWords}}", DataLoad.ClientLoanDetail.ApprovedLoanAmountInWords)
                         .Replace("{{RELATION}}", DataLoad.ClientNextOfKinInfo.NokRelationShip)
                     .Replace("{{LoanAmountInWords}}", DataLoad.ClientLoanDetail.LoanAmountInWords);


                    if (LoanHApp1.Status == StatusMgs.Disbursed)
                    {
                        HtmlResult = HtmlResult.Replace("{{DISBURSMENTDATE}}", ApprovedDate);
                    }
                    else
                    {
                        ApprovedDate = "";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";
                        ApprovedDate += "<li></li>";

                        HtmlResult = HtmlResult.Replace("{{DISBURSMENTDATE}}", ApprovedDate);
                    }


                    var topHtmlDisburse = "<p class='image-p'><span class='empty-mark span-loan-type-image-active'><img src= '../../assets/images/icons8-check-160.png' width='25' /></span><span class='empty-mark-text'>TOP UP LOAN</span></p>";

                    var topHtml = "<p class='image-p span-loan-type-image'><span class='empty-mark'></span><span class='empty-mark-text'>TOP UP LOAN</span></p>";

                    if(LoanHApp1.Status == StatusMgs.Disbursed || LoanHApp1.Status == StatusMgs.Completed)
                    {
                        HtmlResult = HtmlResult.Replace("{{topHtmlDisburse}}", topHtmlDisburse);
                    }
                    else
                    {
                        HtmlResult = HtmlResult.Replace("{{topHtmlDisburse}}", topHtml);
                    }


                    var EmpDate = "";

                    var EmpDateR = Peopl.EmploymentDate.Value.ToShortDateString();

                    for (var i = 0; i <= EmpDateR.Length - 1; i++)
                    {
                        EmpDate += "<li>" + EmpDateR[i] + "</li>";
                    }

                    var RetirDate = "";

                    var RetirDateR = Peopl.RetirementDate.Value.ToShortDateString();

                    for (var i = 0; i <= RetirDateR.Length - 1; i++)
                    {
                        RetirDate += "<li>" + RetirDateR[i] + "</li>";
                    }

                    HtmlResult = HtmlResult.Replace("{{EmpDateR}}", EmpDate).Replace("{{RetirDateR}}", RetirDate);

                    //if (Scheled.IsActive)
                    //{
                    //    HtmlResult = HtmlResult.Replace("{{this.LoanScheduleData.teunerName}}", Convert.ToString(Scheled.DataLoad.TeunerName))
                    //       .Replace("{{this.LoanScheduleData.interest}}", Convert.ToString(Scheled.DataLoad.Interest))
                    //       .Replace("{{this.LoanScheduleData.amountScheduled}}", Convert.ToString(Scheled.DataLoad.AmountScheduled))
                    //        .Replace("{{this.LoanScheduleData.numberOfPayments}}", Convert.ToString(Scheled.DataLoad.NumberOfPayments))
                    //         .Replace("{{this.LoanScheduleData.totalAmount}}", Convert.ToString(Scheled.DataLoad.TotalAmount))
                    //           .Replace("{{this.LoanScheduleData.dueDate}}", Convert.ToString(Scheled.DataLoad.DueDate))
                    //            .Replace("{{this.LoanScheduleData.dueTime}}", Convert.ToString(Scheled.DataLoad.DueTime));
                    //}
                    //else
                    //{
                    //    HtmlResult = HtmlResult.Replace("{{this.LoanScheduleData.teunerName}}", "None")
                    //       .Replace("{{this.LoanScheduleDatainterest}}", "None")
                    //       .Replace("{{this.LoanScheduleData.amountScheduled}}", "None")
                    //        .Replace("{{this.LoanScheduleData.numberOfPayments}}", "None")
                    //         .Replace("{{this.LoanScheduleData.totalAmount}}", "None")
                    //           .Replace("{{this.LoanScheduleData.dueDate}}", "None")
                    //            .Replace("{{this.LoanScheduleData.dueTime}}", "None");
                    //}


                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "loan print out arrangment successful.", true, HtmlResult, HtmlResult, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Failed to arrange loan print out.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SetrinterLoanApp1(PrinterLoanAppModel printerLoan)
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
               
                string HtmlfullPath = Path.Combine(currentDirectory, path, "app-loan-app-review.component.html");
                string HtmData = System.IO.File.ReadAllText(HtmlfullPath);

                string CSSfullPath = Path.Combine(currentDirectory, path, "app-loan-app-review.component.css");
                string CSSData = System.IO.File.ReadAllText(CSSfullPath);

                 string User_Id = new DefalutToken(_configuration).User_Id();

                //var AllAccessTokens = await lapoLoanDB.AccessTokens.Where(s => s.UserId == User_Id && s.IsActive.Value).FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(HtmData) && !string.IsNullOrWhiteSpace(HtmData) && !string.IsNullOrEmpty(CSSData) && !string.IsNullOrWhiteSpace(CSSData))
                {
                    var scheduled = new ScheduledMethod()
                    {
                        AccountId = printerLoan.loanDetailsData.accountId.ToString(),
                        Amount = printerLoan.loanDetailsData.loanAmount,
                        IPPISNumber = printerLoan.loanDetailsData.pFNumber,
                        Tenure = printerLoan.loanDetailsData.ternor,
                    };

                    var Scheled = await new LoanSchedulerHelpers(null, this._configuration).CalculateScheduledLoanAmount(null, scheduled);

                    var HtmlResult = HtmData.Replace("{{THIS.CSSPRINTSTATUS.LOAN.STATUS}}", CSSData).Replace("{{this.LoanDetails.loanAppReviewStatus.status}}", printerLoan.loanAppReviewStatus.status)
                        .Replace("{{this.LoanDetails.loanAppReviewStatus.approvedBy}}", printerLoan.loanAppReviewStatus.approvedBy)
                      .Replace("{{this.LoanDetails.loanAppReviewStatus.approvedDate}}", printerLoan.loanAppReviewStatus.approvedDate)
                     .Replace("{{this.LoanDetails.loanAppReviewStatus.comment}}", printerLoan.loanAppReviewStatus.comment)
                      .Replace("{{this.LoanDetails.bvnDetail.bvn}}", printerLoan.bvnDetail.bVN)
                       .Replace("{{this.LoanDetails.bvnDetail.firstName}}", printerLoan.bvnDetail.firstName)
                        .Replace("{{this.LoanDetails.bvnDetail.middleName}}", printerLoan.bvnDetail.middleName)
                        .Replace("{{this.LoanDetails.bvnDetail.lastName}}", printerLoan.bvnDetail.lastName)
                        .Replace("{{this.LoanDetails.bvnDetail.phoneNumber1}}", printerLoan.bvnDetail.phoneNumber1)
                        .Replace("{{this.LoanDetails.loanDetailsData.loanAmountString}}", printerLoan.loanDetailsData.loanAmountString)
                         .Replace("{{this.LoanDetails.loanDetailsData.ternor}}", printerLoan.loanDetailsData.ternor)
                          .Replace("{{this.LoanDetails.loanDetailsData.bankName}}", printerLoan.loanDetailsData.bankName)
                          .Replace("{{this.LoanDetails.loanDetailsData.acctName}}", printerLoan.loanDetailsData.acctName)
                             .Replace("{{this.LoanDetails.loanDetailsData.acctNumber}}", printerLoan.loanDetailsData.acctNumber)
                             .Replace("{{this.LoanDetails.clientDetail.pfNumber}}", printerLoan.clientDetail.pFNumber)
                               .Replace("{{this.LoanDetails.clientDetail.fullname}}", printerLoan.clientDetail.fullname)
                                .Replace("{{this.LoanDetails.clientDetail.dateOfBirth}}", printerLoan.clientDetail.dateOfBirth)
                                  .Replace("{{this.LoanDetails.clientDetail.age}}", "122")
                                  .Replace("{{this.LoanDetails.clientDetail.maritalStatus}}", printerLoan.clientDetail.maritalStatus)
                                  .Replace("{{this.LoanDetails.acctDetail.email}}", printerLoan.acctDetail.email)
                                   .Replace("{{this.LoanDetails.clientDetail.phoneNumber}}", printerLoan.clientDetail.phoneNumber)
                                     .Replace("{{this.LoanDetails.clientDetail.altPhoneNumber}}", printerLoan.clientDetail.altPhoneNumber)
                                     .Replace("{{this.LoanDetails.clientDetail.residentialAddress}}", printerLoan.clientDetail.residentialAddress)
                                      .Replace("{{this.LoanDetails.clientDetail.nokname}}", printerLoan.clientDetail.nokname)
                                        .Replace("{{this.LoanDetails.clientDetail.nokphone}}", printerLoan.clientDetail.nokphone)
                                        .Replace("{{this.LoanDetails.clientDetail.nokaddress}}", printerLoan.clientDetail.nokaddress)
                      .Replace("{{this.LoanDetails.loanDetailsData.staffIdCard}}", this.MakeImageSrcData(printerLoan.loanDetailsData.staffIdCardUrl))

                       .Replace("{{this.LoanDetails.loanDetailsData.passportPhotograph}}", this.MakeImageSrcData(printerLoan.loanDetailsData.passportPhotographUrl))

                     .Replace("{{this.LoanDetails.loanDetailsData.paySliptfiles}}", this.MakeImageSrcData(printerLoan.loanDetailsData.paySliptfilesUrl));
                    
                    if (Scheled.IsActive)
                    {
                         HtmlResult = HtmlResult.Replace("{{this.LoanScheduleData.teunerName}}", Convert.ToString(Scheled.DataLoad.TeunerName))
                            .Replace("{{this.LoanScheduleData.interest}}", Convert.ToString(Scheled.DataLoad.Interest))
                            .Replace("{{this.LoanScheduleData.amountScheduled}}", Convert.ToString(Scheled.DataLoad.AmountScheduled))
                             .Replace("{{this.LoanScheduleData.numberOfPayments}}", Convert.ToString(Scheled.DataLoad.NumberOfPayments))
                              .Replace("{{this.LoanScheduleData.totalAmount}}", Convert.ToString(Scheled.DataLoad.TotalAmount))
                                .Replace("{{this.LoanScheduleData.dueDate}}", Convert.ToString(Scheled.DataLoad.DueDate))
                                 .Replace("{{this.LoanScheduleData.dueTime}}", Convert.ToString(Scheled.DataLoad.DueTime));
                    }
                    else
                    {
                        HtmlResult = HtmlResult.Replace("{{this.LoanScheduleData.teunerName}}", "None")
                           .Replace("{{this.LoanScheduleData.interest}}", "None")
                           .Replace("{{this.LoanScheduleData.amountScheduled}}", "None")
                            .Replace("{{this.LoanScheduleData.numberOfPayments}}", "None")
                             .Replace("{{this.LoanScheduleData.totalAmount}}", "None")
                               .Replace("{{this.LoanScheduleData.dueDate}}", "None")
                                .Replace("{{this.LoanScheduleData.dueTime}}", "None");
                    }


                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "loan print out arrangment successful.", true, HtmlResult, HtmlResult, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Failed to arrange loan print out.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch(Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> SetPrinterLoanApp(string HashAppId)
        {
            try
            {
                LoanApplicationRequestHeader LoanHApp = null;
                LoanApplicationRequestDetail LoanAppDetails = null;
                KycDetail KycDetail= null;
                SecurityAccount securityAccount = null;
                Person People = null;
                //Client client = null;
                //ClientNetPay clientNetPay = null;
                LoanTenureSetting loanTenureSetting = null;

                try
                {
                    long AppId = Convert.ToInt64(HashAppId);

                    LoanHApp = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.Id == AppId).FirstOrDefaultAsync();

                    LoanAppDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(x => x.LoanAppRequestHeaderId == LoanHApp.Id).FirstOrDefaultAsync();

                    KycDetail = await lapoLoanDB.KycDetails.Where(x => x.LoanAppRequestHeaderId == LoanHApp.Id).FirstOrDefaultAsync();

                    securityAccount = await lapoLoanDB.SecurityAccounts.Where(x => x.Id == LoanHApp.AccountId).FirstOrDefaultAsync();

                    People = await lapoLoanDB.People.Where(x => x.AccountId == LoanHApp.AccountId).FirstOrDefaultAsync();

                    //client = await lapoLoanDB.Clients.Where(x => x.Pfnumber == LoanHApp.Pfnumber).FirstOrDefaultAsync();

                    //clientNetPay = await lapoLoanDB.ClientNetPays.Where(x => x.ClientId == client.Id).FirstOrDefaultAsync();

                    string Hub_Name = "";

                    var HubTeam = await lapoLoanDB.HubTeams.Where(x => /*(x.HubMemberFirstName == LoanHApp.TeamOfficerFirstname && x.HubMemberLastName == LoanHApp.TeamOfficerOthername)  ||*/ x.RefNo == LoanHApp.RelationshipOfficerRef).FirstOrDefaultAsync();

                    if (HubTeam != null)
                    {
                        var HubTeamGroup = await lapoLoanDB.HubTeamGroups.Where(x => x.Id == HubTeam.GroupId).FirstOrDefaultAsync();

                        if(HubTeamGroup!=null)
                        {
                            Hub_Name = HubTeamGroup.HubTeamGroupName;
                        }
                    }

                    long tenureId = Convert.ToInt64(lapoCipher01.DecryptString(LoanAppDetails.Tenure));

                    loanTenureSetting = await lapoLoanDB.LoanTenureSettings.Where(x => x.Id == tenureId).FirstOrDefaultAsync();

                    var NewPrintPre = new PrintPreviewDto()
                    {
                        BVN = LoanHApp.Bvn,
                        PassPortImage = this.MakeImageSrcData(LoanAppDetails.PassportUrl),
                        OrganizationAndOffice = "N P F " + Hub_Name,       
                    };

                    NewPrintPre.ClientOracle = new ClientOracles()
                    {
                        IPPS_Number = LoanHApp.Pfnumber==null ?"": LoanHApp.Pfnumber,
                        IDIssueDate = LoanHApp.CreatedDate.Value.ToShortDateString(),
                        IDExpirementDate = LoanHApp.ApprovedDate.Value.AddMonths(+ GetNumber(loanTenureSetting.TeunerName)).ToShortDateString(),
                        IDNumber = LoanHApp.Pfnumber == null ? "" : LoanHApp.Pfnumber,
                    };

                    NewPrintPre.ClientBank = new ClientBank()
                    {
                        BankAccount = LoanAppDetails.BankAccount,
                        BankAccountNumber = LoanAppDetails.BankAccountNumber,
                        BankAccountName = LoanAppDetails.BankAccountName
                    };

                    NewPrintPre.ClientNextOfKinInfo = new ClientNextOfKinInfos()
                    {
                        Address = KycDetail.NokAddress,
                        Phone_Number = KycDetail.NokPhoneNumber,
                        Name = KycDetail.NokName,
                         NokRelationShip = KycDetail.NokRelationShip
                    };

                    NewPrintPre.ClientInfo = new ClientInfos()
                    {
                        Alt_Phone_Number = KycDetail.AltPhoneNumber,
                        Home_Address = KycDetail.CurrentAddress,
                        DateOfBirth = KycDetail.DateOfBirth.Value.ToShortDateString(),
                        Phone_Number = KycDetail.PhoneNumber,
                        NoOfChildren = "0",
                        Marital_Status = KycDetail.MaritalStatus,
                        First_Name = People.FirstName,
                        Middle_Name = People.MiddleName,
                        Last_Name = People.LastName,
                        Gender = People.Gender,
                        Has_IDCard = true,
                        HasPass_Port = true,
                        HasBVN = true, 
                    };

                    var loanSchedule = await new LoanSchedulerHelpers(this.controllerBase, this._configuration).CalculateScheduledLoanAmount(null, new ScheduledMethod() { IPPISNumber = LoanHApp.Pfnumber, Tenure = lapoCipher01.EnryptString(loanTenureSetting.Id.ToString()), Amount = (double)LoanAppDetails.Amount.Value , AccountId  = LoanHApp.AccountId.ToString() });

                    NewPrintPre.ClientLoan = new ClientLoan()
                    {
                        InterestRate = Convert.ToString(LoanHApp.LoanInterest.Value.ToString()),
                        LoanTernure = loanTenureSetting.TeunerName,

                        DisbursmentAmount = string.Format(Convert.ToString(loanSchedule.DataLoad.DisbursmentAmount).Replace(".000", "").Replace(".000M", "").Replace(".0000", "").Replace(".0000M", "").Replace(".00", "")),
                        DisbursmentDate = LoanHApp.ApprovedDate.Value.ToShortDateString(),

                        MonthlyInstallmentAmount = string.Format(Convert.ToString(loanSchedule.DataLoad.InterestPerMonth).Replace(".000", "").Replace(".000M", "").Replace(".0000", "").Replace(".0000M", "").Replace(".00", "")),
                        MonthlyInstallmentAmount_Currency = loanSchedule.DataLoad.ScheduledList[0]. LoanAmountWithCurrency,

                        MonthlyNet_Pay = string.Format( "{0:C}", 0).Replace(".00", ""),

                        MonthlyNet_Pay_Currency = string.Format(new CultureInfo("ig-NG"), "{0:C}", 0).Replace(".00", ""), 
                        Narration = LoanHApp.Narration == null ? "": LoanHApp.Narration
                    };

                    //string.Format(new CultureInfo("ig-NG"), "{0:C}", Convert.ToString(loanSchedule.DataLoad.InterestPerMonth).Replace(".000", "").Replace(".000M", "").Replace(".0000", "").Replace(".0000M", "").Replace(".00", ""))

                    var newAmtNumberToWord =  new NumberToWords(this.controllerBase, this._configuration);

                    var LoanAmountNumberToWords = await newAmtNumberToWord.NumberToWorders((int)LoanAppDetails.Amount.Value);

                    var ApprovedLoanAmountInWords = await newAmtNumberToWord.NumberToWorders((int)LoanAppDetails.Amount.Value);

                     var Amt1 = string.Format(new CultureInfo("ig-NG"), "{0:C}", Convert.ToString(LoanAppDetails.Amount.Value.ToString().Replace(".000", "").Replace(".000M", "").Replace(".0000", "").Replace(".0000M", "").Replace(".00", "")));

                    var formatMoney = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
                     Amt1 =  string.Format(formatMoney, "{0:c}", LoanAppDetails.Amount.Value).Replace(" ₫", "").Replace(".", ",");

                    var Amt5 = string.Format(new CultureInfo("ig-NG"), "{0:C}", LoanAppDetails.Amount.Value); 
                    
                    NewPrintPre.ClientLoanDetail = new ClientLoanDetails()
                    {
                        LoanAmount = Amt5,

                        ApprovedLoanAmount = Amt5,
                        
                        ApprovedLoanAmountInCurrency = Amt1,
                      
                        LoanAmountInCurrency = Amt1,

                        ApprovedLoanAmountInWords = ApprovedLoanAmountInWords +  "  Naira",
                        LoanAmountInWords = LoanAmountNumberToWords +  "  Naira",
                    };

                    NewPrintPre.ClientEmployment = new ClientEmployments()
                    {
                        Retirement_Date = "None",
                        Expire_Date = "None",
                    };

                    // var newLoanScheled = new { ScheduledList = LoanRespondlist, NumberOfPayments = (int)numberOfTernure > 1 ? numberOfTernure + " Times" : numberOfTernure + " Term", TenureDuration = TenureDuration, DueDate = LoanRespondlist[LoanRespondlist.Count - 1].NextSchedeledDate, DueTime = LoanRespondlist[LoanRespondlist.Count - 1].NextSchedeledTime, InterestPayment = string.Format(new CultureInfo("ig-NG"), "{0:C}", (double)(TotalInterest)).Replace(".00", ""), Interest = LoanSettings.LoanInterest, TotalAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", (double)(TotalAmount)).Replace(".00", ""), TeunerName = Tenure.TeunerName, LoanSettingsId = lapoCipher01.EnryptString(LoanSettings.Id.ToString()), MinAmount = LoanSettings.MinLoanAmount, MaxAmount = LoanSettings.MaxLoanAmount, UsedMaxSalary = LoanSettings.UseSalaryAsMaxLoanAmount.Value, InterestPrincipalPayment = (loanAmount), InterestPayment1 = string.Format(new CultureInfo("ig-NG"), "{0:C}", InterestPermonth).Replace(".00", "") };

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success,"Loan Details Print Preview", true, NewPrintPre, NewPrintPre, Status.Success, StatusMgs.Success);
                }
                catch (Exception exx)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, exx.Message ?? exx.InnerException.Message, false, null, null, Status.Ërror, StatusMgs.Error);
                }
            }
            catch(Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private int GetNumber(string Value)
        {
            try
            {
                string getTenure = new string(Value.Where(Char.IsDigit).ToArray());

                return Convert.ToInt32(getTenure);
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        private int GetLastInt(string line)
        {
            int offset = line.Length;
            for (int i = line.Length - 1; i >= 0; i--)
            {
                char c = line[i];
                if (char.IsDigit(c))
                {
                    offset--;
                }
                else
                {
                    if (offset == line.Length)
                    {
                        // No int at the end
                        return -1;
                    }
                    return int.Parse(line.Substring(offset));
                }
            }
            return int.Parse(line.Substring(offset));
        }
        private string MakeImageSrcData(string filename)
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "Resources\\LoanAppImages";

                var fileNameOnly = Path.GetFileName(filename);

                string fullPath = Path.Combine(currentDirectory, path, fileNameOnly);

                byte[] filebytes = System.IO.File.ReadAllBytes(fullPath);

                var urldata = "data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);

                return urldata; // "<%=" +   + " %>";
            }
            catch (Exception ex)
            {
                try
                {

                    byte[] filebytes = System.IO.File.ReadAllBytes("https://www.lapo-nigeria.org/img/logo/LAPO_Logo_2022.png");

                    var urldata = "data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);

                    return urldata;
                }
                catch(Exception exx)
                {
                    return  "";
                }
            }
        }
    }
}
