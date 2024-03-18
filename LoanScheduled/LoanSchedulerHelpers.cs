using Azure;
using LapoLoanWebApi.E360Helpers;
using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LoanScheduled.Model;
using LapoLoanWebApi.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Table.PivotTable;
using Org.BouncyCastle.Crypto.Tls;
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using System.Threading;

namespace LapoLoanWebApi.LoanScheduled
{
    public class LoanSchedulerHelpers
    {
        private ControllerBase controllerBase { get; set; }

        private LapoLoanDBContext lapoLoanDB = null;

        private LapoCipher02 lapoCipher02 { get; set; }
        private LapoCipher02 ReallapoCipher02 { get; set; }
        private LapoCipher00 lapoCipher00 { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }

        private E360AuthHttpClient e360AuthHttpClient { get; set; }
        private IConfiguration _configuration;

        public LoanSchedulerHelpers(ControllerBase controllerBase, IConfiguration configuration)
        {

            this._configuration = configuration;
            this.controllerBase = controllerBase;
            this.lapoLoanDB = new LapoLoanDBContext(_configuration);
            this.e360AuthHttpClient = new E360AuthHttpClient(controllerBase, configuration);

            this.ReallapoCipher02 = new LapoCipher02();
            this.lapoCipher02 = new LapoCipher02();
            this.lapoCipher00 = new LapoCipher00();
            this.lapoCipher01 = new LapoCipher01();
        }

        public async Task<RespondMessageDto> CheckIfCustomerHasRunningLoan(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ScheduledDto scheduled)
        {
            try
            {
                if (scheduled == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Please enter your IPPIS Number.", true, scheduled, scheduled, Status.Success, StatusMgs.Success);
                }

                var AcctId = Convert.ToInt64(scheduled.AccountId);

                var ClientPermission = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.AccountId == AcctId && x.Pfnumber == scheduled.IPPISNumber && (x.Status == StatusMgs.Success || x.Status == StatusMgs.Active || x.Status == StatusMgs.Approved)).FirstOrDefaultAsync();
                if (ClientPermission != null)
                {
                    var ApplyDate = ClientPermission.ApprovedDate.Value - DateTime.Now;

                    var ClientLoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(x => x.LoanAppRequestHeaderId == ClientPermission.Id).FirstOrDefaultAsync();

                    if (ClientLoanDetails != null)
                    {
                        var AmountSchedule = 0.0;
                        var getTenure  = new String(ClientLoanDetails.Tenure.Where(Char.IsDigit).ToArray()); 

                        var getTenureDuration = new String(ClientLoanDetails.Tenure.Where(Char.IsLetter).ToArray());

                        if ((getTenureDuration.ToString().ToLower().Equals("Months".ToLower()) || getTenureDuration.ToString().ToLower().Equals("Month".ToLower())) && Char.IsDigit(Convert.ToChar(getTenure)))
                        {
                            var durationMonth = (double)((Convert.ToInt32(getTenure) * 31) + (31 - ClientPermission.ApprovedDate.Value.Day));

                            AmountSchedule = ((double)ClientLoanDetails.Amount.Value / durationMonth);
                        }

                        if (getTenureDuration.ToString().ToLower().Equals("weeks".ToLower()) || getTenureDuration.ToString().ToLower().Equals("week".ToLower()))
                        {
                            var durationMonth = (double)((Convert.ToInt32(getTenure) * 7) + (31 - ClientPermission.ApprovedDate.Value.Day));

                            AmountSchedule = ((double)ClientLoanDetails.Amount.Value / durationMonth);
                        }

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, AmountSchedule, AmountSchedule, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Unable to fetch loan range.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CheckCustomerHasRunningLoanManual(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ScheduledDto scheduled)
        {
            try
            {
                if (scheduled == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Enter a valid IPPIS Number.", false, scheduled, scheduled, Status.Success, StatusMgs.Success);
                }

                var CheckIfHaveRunningLoan = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.Pfnumber == scheduled.IPPISNumber && ( x.Status == StatusMgs.Pending)).AnyAsync();
                if (CheckIfHaveRunningLoan)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Dear Customer, You have exiting loan that is under review.", false, "", null, Status.Ërror, StatusMgs.Error);
                }
               
                 CheckIfHaveRunningLoan = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.Pfnumber == scheduled.IPPISNumber && (x.Status == StatusMgs.Success || x.Status == StatusMgs.Active || x.Status == StatusMgs.Approved)).AnyAsync();
                if (CheckIfHaveRunningLoan)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Dear customer, You have a repayment loan to complete. Or Contact the admin for support", false, "", null, Status.Ërror, StatusMgs.Error);
                }
              
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Accepted.", true, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CheckCustomerHasRunningLoan(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ScheduledDto scheduled)
        {
            try
            {
                if (scheduled == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Please enter your IPPIS Number.", true, scheduled, scheduled, Status.Success, StatusMgs.Success);
                }

                var AcctId = Convert.ToInt64(scheduled.AccountId);
                var IsOnScheduled = false;
                var ClientPermission = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.AccountId == AcctId && x.Pfnumber == scheduled.IPPISNumber && (x.Status == StatusMgs.Success || x.Status == StatusMgs.Active || x.Status == StatusMgs.Approved)).FirstOrDefaultAsync();
                if (ClientPermission != null)
                {
                    var ApplyDate = ClientPermission.ApprovedDate.Value - DateTime.Now;

                    var ClientLoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(x => x.LoanAppRequestHeaderId == ClientPermission.Id).FirstOrDefaultAsync();

                    if (ClientLoanDetails != null)
                    {
                       
                        var getTenure = new String(ClientLoanDetails.Tenure.Where(Char.IsDigit).ToArray());

                        var getTenureDuration = new String(ClientLoanDetails.Tenure.Where(Char.IsLetter).ToArray());

                        if ((getTenureDuration.ToString().ToLower().Equals("Months".ToLower()) || getTenureDuration.ToString().ToLower().Equals("Month".ToLower())) && Char.IsDigit(Convert.ToChar(getTenure)))
                        {
                            var TenureMonth = Convert.ToInt32(getTenure);

                            var TotalDays = GetDays(ClientPermission.ApprovedDate.Value.Year, ClientPermission.ApprovedDate.Value.Month);

                            var RemainingDays = TotalDays - ClientPermission.ApprovedDate.Value.Day;

                            var TotalTenureDays = GetDaysInMonth(ClientPermission.ApprovedDate.Value.Year, ClientPermission.ApprovedDate.Value.Month, TenureMonth);

                            var TotalDue = TotalTenureDays + RemainingDays;

                            DateTime f = new DateTime(0);
                            var y = f.AddDays(TotalDue);

                            if(DateTime.Now > y)
                            {
                                IsOnScheduled = false;
                            }
                            else
                            {
                                IsOnScheduled = true;
                            }

                            var DataReturn = new { IsOnScheduled = IsOnScheduled, DueDate = y.Date.ToLongDateString() };

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, DataReturn, DataReturn, Status.Success, StatusMgs.Success);
                        }

                        if (getTenureDuration.ToString().ToLower().Equals("weeks".ToLower()) || getTenureDuration.ToString().ToLower().Equals("week".ToLower()))
                        {
                            var TenureDay = Convert.ToInt32(getTenure);

                            var TotalDays = GetDays(ClientPermission.ApprovedDate.Value.Year, ClientPermission.ApprovedDate.Value.Month);

                            var RemainingDays = TotalDays - ClientPermission.ApprovedDate.Value.Day;

                            var TotalTenureDays = GetDaysInMonth(ClientPermission.ApprovedDate.Value.Year, ClientPermission.ApprovedDate.Value.Month, TenureDay);

                            var TotalDue = TotalTenureDays + RemainingDays;

                            DateTime f = new DateTime(0);
                            var y = f.AddDays(TotalDue);

                            if (DateTime.Now > y)
                            {
                                IsOnScheduled = false;
                            }
                            else
                            {
                                IsOnScheduled = true;
                            }

                            var DataReturn = new { IsOnScheduled = IsOnScheduled, DueDate = y.Date.ToLongDateString() };

                            return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, DataReturn, DataReturn, Status.Success, StatusMgs.Success);
                        }

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, null, null, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Unable to fetch loan range.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task BackGroundRunningDueDate(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        {
            try
            {
                var IsOnScheduled = false;
                var ClientLoans = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => (x.Status == StatusMgs.Success || x.Status == StatusMgs.Active || x.Status == StatusMgs.Approved)).ToListAsync();
                if (ClientLoans != null)
                {
                    foreach (var loanHeader in ClientLoans)
                    {
                        var ApplyDate = loanHeader.ApprovedDate.Value - DateTime.Now;

                        var ClientLoanDetails = await lapoLoanDB.LoanApplicationRequestDetails.Where(x => x.LoanAppRequestHeaderId == loanHeader.Id).FirstOrDefaultAsync();

                        if (ClientLoanDetails != null)
                        {
                            var getTenure = new String(ClientLoanDetails.Tenure.Where(Char.IsDigit).ToArray());

                            var getTenureDuration = new String(ClientLoanDetails.Tenure.Where(Char.IsLetter).ToArray());

                            if ((getTenureDuration.ToString().ToLower().Equals("Months".ToLower()) || getTenureDuration.ToString().ToLower().Equals("Month".ToLower())) && Char.IsDigit(Convert.ToChar(getTenure)))
                            {
                                var TenureMonth = Convert.ToInt32(getTenure);

                                var TotalDays = GetDays(loanHeader.ApprovedDate.Value.Year, loanHeader.ApprovedDate.Value.Month);

                                var RemainingDays = TotalDays - loanHeader.ApprovedDate.Value.Day;

                                var TotalTenureDays = GetDaysInMonth(loanHeader.ApprovedDate.Value.Year, loanHeader.ApprovedDate.Value.Month, TenureMonth);

                                var TotalDue = TotalTenureDays + RemainingDays;

                                DateTime f = new DateTime(0);
                                var y = f.AddDays(TotalDue);

                                if (DateTime.Now > y)
                                {
                                    loanHeader.Status = StatusMgs.Completed;
                                    lapoLoanDB.Entry(loanHeader).State = EntityState.Modified;
                                    IsOnScheduled = false;
                                }
                                else
                                {
                                    IsOnScheduled = true;
                                }

                                var DataReturn = new { IsOnScheduled = IsOnScheduled, DueDate = y.Date.ToLongDateString() };

                                //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, DataReturn, DataReturn, Status.Success, StatusMgs.Success);
                            }

                            if (getTenureDuration.ToString().ToLower().Equals("weeks".ToLower()) || getTenureDuration.ToString().ToLower().Equals("week".ToLower()))
                            {
                                var TenureDay = Convert.ToInt32(getTenure);

                                var TotalDays = GetDays(loanHeader.ApprovedDate.Value.Year, loanHeader.ApprovedDate.Value.Month);

                                var RemainingDays = TotalDays - loanHeader.ApprovedDate.Value.Day;

                                var TotalTenureDays = GetDaysInMonth(loanHeader.ApprovedDate.Value.Year, loanHeader.ApprovedDate.Value.Month, TenureDay);

                                var TotalDue = TotalTenureDays + RemainingDays;

                                DateTime f = new DateTime(0);
                                var y = f.AddDays(TotalDue);

                                if (DateTime.Now > y)
                                {
                                    loanHeader.Status = StatusMgs.Completed;
                                    lapoLoanDB.Entry(loanHeader).State = EntityState.Modified;
                                    IsOnScheduled = false;
                                    IsOnScheduled = false;
                                }
                                else
                                {
                                    IsOnScheduled = true;
                                }

                                var DataReturn = new { IsOnScheduled = IsOnScheduled, DueDate = y.Date.ToLongDateString() };

                                //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, DataReturn, DataReturn, Status.Success, StatusMgs.Success);

                            }

                            //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, null, null, Status.Success, StatusMgs.Success);
                        }
                    }
                }

                //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Getting Scheduled Amount was not successful.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                //return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> CalculateScheduledLoanAmount(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ScheduledMethod scheduled)
        {
            try
            {
                if (scheduled == null)
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Please enter your IPPIS Number.", true, scheduled, scheduled, Status.Success, StatusMgs.Success);
                }

                lapoLoanDB = new LapoLoanDBContext(this._configuration);
                var TenureId = lapoCipher01.DecryptString(scheduled.Tenure);
                var Tenure1 = Convert.ToInt64(TenureId);
                var Tenure = lapoLoanDB.LoanTenureSettings.Where(x => x.Id == Tenure1).FirstOrDefault();

                var LoanSettings = lapoLoanDB.LoanSettings.FirstOrDefault();

                if (scheduled != null && Tenure != null && LoanSettings != null)
                {
                    string getTenure = new string(Tenure.TeunerName.Where(Char.IsDigit).ToArray());

                    string TenureDuration = new string(Tenure.TeunerName.Where(Char.IsLetter).ToArray());

                    double loanAmount = Convert.ToDouble(scheduled.Amount);
                    double Interest = LoanSettings.LoanInterest.Value;
                    int numberOfTernure = Convert.ToInt32(getTenure);

                    DateTime LoanExpiredDate = DateTime.Now;

                    TenureDuration = TenureDuration.Replace(" ", "").ToLower();

                    if (TenureDuration.ToLower().Contains("months".ToLower()) || TenureDuration.ToLower().Contains("month".ToLower()))
                    {
                        var InterestPermonth = ((loanAmount * Interest) / 100);

                        var TotalInterest = (InterestPermonth  * numberOfTernure);

                        var TotalAmount = (loanAmount + TotalInterest);

                        var LoanScheduledAmount = (TotalAmount / numberOfTernure);

                        List<LoanRespondsModel> LoanRespondlist = new List<LoanRespondsModel>();

                        for (int i = 1; i <= numberOfTernure; i++)
                        {
                            var newLoanScheduled = new LoanRespondsModel()
                            {
                                no = i,

                                TotalAmount = TotalAmount,

                                TotalAmountWithCurrency = string.Format(new CultureInfo("ig-NG"), "{0:C}", TotalAmount).Replace(".00", ""),

                                InterestAmount = InterestPermonth,

                                InterestAmountWithCurrency = string.Format(new CultureInfo("ig-NG"), "{0:C}", InterestPermonth).Replace(".00", ""),

                                LoanAmount = LoanScheduledAmount,

                                LoanAmountWithCurrency = string.Format(new CultureInfo("ig-NG"), "{0:C}", LoanScheduledAmount).Replace(".00", ""),

                                NextSchedeledDate = LoanExpiredDate.AddMonths(+i).ToLongDateString(),

                                NextSchedeledInDate = LoanExpiredDate.AddMonths(+i),

                                NextSchedeledTime = LoanExpiredDate.AddMonths(+i).ToLongTimeString(),

                                NextSchedeledInTime = LoanExpiredDate.AddMonths(+i).TimeOfDay,
                            };

                            LoanRespondlist.Add(newLoanScheduled);
                        }

                        var newLoanScheled = new { ScheduledList = LoanRespondlist, NumberOfPayments = (int)numberOfTernure > 1 ? numberOfTernure + " Times" : numberOfTernure + " Term", TenureDuration = TenureDuration, DueDate = LoanRespondlist[LoanRespondlist.Count - 1].NextSchedeledDate, DueTime = LoanRespondlist[LoanRespondlist.Count - 1].NextSchedeledTime, InterestPayment = string.Format(new CultureInfo("ig-NG"), "{0:C}", (double)(TotalInterest)).Replace(".00", ""), Interest = LoanSettings.LoanInterest, TotalAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", (double)(TotalAmount)).Replace(".00", ""), TeunerName = Tenure.TeunerName, LoanSettingsId = lapoCipher01.EnryptString(LoanSettings.Id.ToString()), MinAmount = LoanSettings.MinLoanAmount, MaxAmount = LoanSettings.MaxLoanAmount, UsedMaxSalary = LoanSettings.UseSalaryAsMaxLoanAmount.Value, InterestPrincipalPayment = (loanAmount), InterestPayment1 = string.Format(new CultureInfo("ig-NG"), "{0:C}", InterestPermonth).Replace(".00", ""), InterestPerMonth = InterestPermonth, DisbursmentAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", loanAmount).Replace(".00", "") };

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, newLoanScheled, newLoanScheled, Status.Success, StatusMgs.Success);
                    }
                    else
                    {

                        var InterestPermonth = ((loanAmount * Interest) / 100);

                        var TotalInterest = (InterestPermonth * numberOfTernure);

                        var TotalAmount = (loanAmount + TotalInterest);

                        var LoanScheduledAmount = (TotalAmount / numberOfTernure);

                        List<LoanRespondsModel> LoanRespondlist = new List<LoanRespondsModel>();

                        for (int i = 1; i <= numberOfTernure; i++)
                        {
                            var newLoanScheduled = new LoanRespondsModel()
                            {
                                no = i,
                                TotalAmount = TotalAmount,
                                TotalAmountWithCurrency = string.Format(new CultureInfo("ig-NG"), "{0:C}", TotalAmount).Replace(".00", ""),

                                InterestAmount = InterestPermonth,
                                InterestAmountWithCurrency = string.Format(new CultureInfo("ig-NG"), "{0:C}", InterestPermonth).Replace(".00", ""),

                                LoanAmount = LoanScheduledAmount,
                                LoanAmountWithCurrency = string.Format(new CultureInfo("ig-NG"), "{0:C}", LoanScheduledAmount).Replace(".00", ""),

                                NextSchedeledDate = LoanExpiredDate.AddMonths(+i).ToLongDateString(),
                                NextSchedeledInDate = LoanExpiredDate.AddMonths(+i),

                                NextSchedeledTime = LoanExpiredDate.AddMonths(+i).ToLongTimeString(),
                                NextSchedeledInTime = LoanExpiredDate.AddMonths(+i).TimeOfDay,
                            };

                            LoanRespondlist.Add(newLoanScheduled);
                        }

                        var newLoanScheled = new { ScheduledList = LoanRespondlist, NumberOfPayments = (int)numberOfTernure > 1 ? numberOfTernure + " Times" : numberOfTernure + " Term", TenureDuration = TenureDuration, DueDate = LoanRespondlist[LoanRespondlist.Count - 1].NextSchedeledDate, DueTime = LoanRespondlist[LoanRespondlist.Count - 1].NextSchedeledTime, InterestPayment = string.Format(new CultureInfo("ig-NG"), "{0:C}", (double)(TotalInterest)).Replace(".00", ""), Interest = LoanSettings.LoanInterest, TotalAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", (double)(TotalAmount)).Replace(".00", ""), TeunerName = Tenure.TeunerName, LoanSettingsId = lapoCipher01.EnryptString(LoanSettings.Id.ToString()), MinAmount = LoanSettings.MinLoanAmount, MaxAmount = LoanSettings.MaxLoanAmount, UsedMaxSalary = LoanSettings.UseSalaryAsMaxLoanAmount.Value, InterestPrincipalPayment = (loanAmount), InterestPayment1 = string.Format(new CultureInfo("ig-NG"), "{0:C}", InterestPermonth).Replace(".00", ""), InterestPerMonth = InterestPermonth, DisbursmentAmount = string.Format(new CultureInfo("ig-NG"), "{0:C}", loanAmount).Replace(".00", "") };

                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, newLoanScheled, newLoanScheled, Status.Success, StatusMgs.Success);
                    }

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", false, null, null, Status.Success, StatusMgs.Success);

                    //if ((getTenureDuration.ToString().ToLower().Equals("Months".ToLower()) || getTenureDuration.ToString().ToLower().Equals("Month".ToLower())) && Char.IsDigit(Convert.ToChar(getTenure)))
                    //{
                    //    var durationMonth = (double)((Convert.ToInt32(getTenure) * 31) + (31 - DateTime.Now.Day));

                    //    if (Convert.ToInt32(getTenure) <=1)
                    //    {
                    //        AmountSchedule = ((double)scheduled.Amount );
                    //    }

                    //    DateTime dateTime = new DateTime(0);
                    //    var dateTime1 = dateTime.AddDays(durationMonth);

                    //    var newLoanScheled = new { AmountScheduled = AmountSchedule, MethodDays = durationMonth, DurationName = "Month" , DueDate = dateTime1.Date.ToLongDateString(), DueTime = dateTime1.Date.ToLongTimeString() };

                    //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, newLoanScheled, newLoanScheled, Status.Success, StatusMgs.Success);
                    //}

                    //if (getTenureDuration.ToString().ToLower().Equals("weeks".ToLower()) || getTenureDuration.ToString().ToLower().Equals("week".ToLower()))
                    //{
                    //    var durationMonth = (double)((Convert.ToInt32(getTenure) * 7) + (31 - DateTime.Now.Day));

                    //    AmountSchedule = ((double)scheduled.Amount / durationMonth);

                    //    DateTime dateTime = new DateTime(0);
                    //    var dateTime1 = dateTime.AddDays(durationMonth);

                    //    var newLoanScheled = new { AmountScheduled = AmountSchedule, MethodDays = durationMonth, DurationName = "Week", DueDate = dateTime1.Date.ToLongDateString(), DueTime = dateTime1.Date.ToLongTimeString() };

                    //    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, newLoanScheled, newLoanScheled, Status.Success, StatusMgs.Success);
                    //}
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Unable to fetch loan range.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public async Task<RespondMessageDto> GetAllLoanSettings(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        {
            try
            {
                var ClientPermission = await lapoLoanDB.LoanSettings.FirstOrDefaultAsync();
                if (ClientPermission != null)
                {
                    var newLoanSettings = new Model.LoanSettingsModel()
                    {
                        LoanInterest = ClientPermission.LoanInterest.Value,
                        MaxLoanAmount = (double)ClientPermission.MaxLoanAmount.Value,
                        MinLoanAmount = (float)ClientPermission.MinLoanAmount.Value,
                        UseSalaryAsMaxLoanAmount = ClientPermission.UseSalaryAsMaxLoanAmount.Value,
                        Id = this.lapoCipher01.EnryptString(ClientPermission.Id.ToString())
                    };

                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Getting Scheduled Amount was successful.", true, newLoanSettings, newLoanSettings, Status.Success, StatusMgs.Success);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Unable to fetch loan range.", false, "", null, Status.Ërror, StatusMgs.Error);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        private int GetDaysInMonth(int year, int month, int months)
    {
        DateTime dt1 = new DateTime(year, month, 1);
        DateTime dt2 = dt1.AddMonths(months);
        TimeSpan ts = dt2 - dt1;
        return (int)ts.TotalDays;
    }
        private int GetDays(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            return days;
        }
        private int GetDaysCalendar(int year, int month)
        {
            int days = CultureInfo.CurrentCulture.
                Calendar.GetDaysInMonth(year, month);

            return days;
        }
    }
}
