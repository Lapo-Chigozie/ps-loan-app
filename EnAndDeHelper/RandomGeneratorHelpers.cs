using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using LapoLoanWebApi.LapoRepositoryHelper;
using LapoLoanWebApi.ModelDto;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace LapoLoanWebApi.EnAndDeHelper
{
    public class RandomGeneratorHelpers
    {
        // Instantiate random number generator.
        // It is better to keep a single Random instance
        // and keep using Next on the same instance.
        private IConfiguration _configuration;

        public RandomGeneratorHelpers(IConfiguration _configuration
)
        {
            this._configuration = _configuration;
        }

        private readonly  Random _random = new Random();

        // Generates a random number within a range.
        public  int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        // Generates a random string with a given size.
        private  string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.

            // char is a single Unicode character
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length = 26

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        // Generates a random password.
        // 4-LowerCase + 4-Digits + 2-UpperCase
        private  string RandomPassword(int Start, int End)
        {
            var passwordBuilder = new StringBuilder();

            // 4-Letters lower case
            passwordBuilder.Append(RandomString(4, true));

            // 4-Digits between 1000 and 9999
            passwordBuilder.Append(RandomNumber(Start, End));

            // 2-Letters upper case
            passwordBuilder.Append(RandomString(2));
            return passwordBuilder.ToString();
        }

        public  async Task<RespondMessageDto> GenarateRandomCode(Microsoft.AspNetCore.Mvc.ControllerBase controllerBase, long AcctId, string Code, int Start, int End)
        {
            try
            {
                var CodeResult = RandomPassword(Start, End);

                Code = CodeResult;

                var Result = await new AdminActivitesHelpers(controllerBase, this._configuration).GetTwoFactorAuth(AcctId, CodeResult);

                if (Result == null && Result.status == Status.Success && Result.IsActive && Result.status == Status.Success)
                {
                    return await GenarateRandomCode(controllerBase, AcctId, CodeResult, Start, End);
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, CodeResult.Remove(0, 1), CodeResult.Remove(0,1), Status.Success, StatusMgs.Success, true);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid code Genarated", false, null, null, Status.Failed, StatusMgs.Error, true);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Failed, StatusMgs.Error, true);
            }
        }

        private LapoLoanDBContext lapoLoanDB = null;
        public  async Task<RespondMessageDto> BvnGenarateRandomCode(Microsoft.AspNetCore.Mvc.ControllerBase controllerBase, long AcctId, string Code, int Start, int End)
        {
            try
            {
                lapoLoanDB = new LapoLoanDBContext(this._configuration);

                var CodeResult = RandomPassword(Start, End);

                Code = CodeResult;

                var Result = await lapoLoanDB.Bvnverifications.Where(x => x.Code == Code).AnyAsync();
                if (Result)
                {
                    return await BvnGenarateRandomCode(controllerBase, AcctId, CodeResult, Start, End);
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, CodeResult.Remove(0, 1), CodeResult.Remove(0, 1), Status.Success, StatusMgs.Success, true);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid code Genarated", false, null, null, Status.Failed, StatusMgs.Error, true);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Failed, StatusMgs.Error, true);
            }
        }

        private void Main()
        {
            System.Random random = new System.Random();
            System.Console.WriteLine(random.Next());
            System.Console.WriteLine(random.Next(50));
            System.Console.WriteLine(random.Next(10, 50));
            System.Console.WriteLine(random.Next());
         }

        public  async Task<RespondMessageDto> GenarateLoanRequestCodeRandomCode(Microsoft.AspNetCore.Mvc.ControllerBase controllerBase, long AcctId, string Code, int Start, int End)
        {
            try
            {
                lapoLoanDB = new LapoLoanDBContext(this._configuration);

                var CodeResult = RandomPassword(Start, End);

                Code = CodeResult;

                Code = Code.Remove(0, 1);

                var Result = await lapoLoanDB.LoanApplicationRequestHeaders.Where(x => x.RequestCode == Code).AnyAsync();

                if (Result)
                {
                    return await GenarateLoanRequestCodeRandomCode(controllerBase, AcctId, CodeResult, Start, End);
                }
                else
                {
                    return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Success", true, CodeResult.Remove(0, 1), CodeResult.Remove(0, 1), Status.Success, StatusMgs.Success, true);
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, "Invalid code Genarated", false, null, null, Status.Failed, StatusMgs.Error, true);
            }
            catch (Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, null, null, Status.Failed, StatusMgs.Error, true);
            }
        }
    }
}
