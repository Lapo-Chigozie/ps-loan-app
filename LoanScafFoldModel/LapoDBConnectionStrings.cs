using LapoLoanWebApi.EnAndDeHelper;
using Microsoft.Extensions.Configuration;

namespace LapoLoanWebApi.LoanScafFoldModel
{
    public class LapoDBConnectionStrings
    {
        private IConfiguration Configuration = null;

        public LapoDBConnectionStrings(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // public const string ConnectionString = "Server=.;Database=LapoLoan_AppDB;Trusted_Connection=True;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=true;Connection Timeout=30";

        public string ConnectionString()
        {
            return DefalutToken.GetConnectionString(this.Configuration, DefalutToken.ConType);
        }

        // Configuration.GetConnectionString("DefaultConnection");

        // "Server=LAPO\\MSSQLSERVER14;Database=LapoLoanDbTest;Integrated Security = False; User=LapoDev; password=Lapo12345;MultipleActiveResultSets=true;Encrypt=False";
    }
}
