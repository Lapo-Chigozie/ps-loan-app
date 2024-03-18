using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LoanNetPaysHelpers;
using LapoLoanWebApi.ModelDto;
using LapoLoanWebApi.TwoFactorsHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace LapoLoanWebApi.LoanScheduled.UploadHostedServices
{
    public sealed class FileUploadHostedServices : BackgroundService
    {
        private readonly ILogger<FileUploadHostedServices> _logger;
        //private int _executionCount;
        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.LapoLoanDBContext lapoLoanDB = null;
        private List<NetPaysBackgroundServices> CallSeviceLists = new List<NetPaysBackgroundServices>();
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public FileUploadHostedServices(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, ILogger<FileUploadHostedServices> logger, IConfiguration configuration)
        {
            this._configuration = configuration;
            this._environment = _environment;
            this.lapoLoanDB = new LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts.LapoLoanDBContext(this._configuration);
            this._logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Timed Hosted Service running.");

                // When the timer should have no due-time, then do the work once now.
                CallSevice();

                using PeriodicTimer timer = new(TimeSpan.FromMinutes(15));

                try
                {
                    while (await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        this.CallSevice();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Timed Hosted Service is stopping.");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void CallSevice()
        {
            try
            {
                var Files = await lapoLoanDB.FileUploads.Where(x => x.Status == false).ToListAsync();

                if (Files != null && Files.Count > 0)
                {
                    Files.ForEach(async f =>
                    {
                        var NetPaysServices = new NetPaysBackgroundServices(null, this._configuration);
                        await NetPaysServices.LoadExcelData(this._environment, f.Src);

                        CallSeviceLists.Add(NetPaysServices);
                    });
                }
            }
            catch (Exception epx)
            {

            }
        }
    }
}
