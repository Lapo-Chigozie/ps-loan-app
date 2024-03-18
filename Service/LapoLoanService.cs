using Microsoft.AspNetCore.DataProtection;
using ServiceStack;

namespace LapoLoanWebApi.Service
{
    public class LapoLoanService : IService
    {
        private const string Purpose = "my protection purpose";
        private readonly IDataProtectionProvider _provider;

        public LapoLoanService(IDataProtectionProvider provider)
        {
            _provider = provider;
        }

        public string Encrypt(string plainText)
        {
            var protector = _provider.CreateProtector(Purpose);
            return protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            var protector = _provider.CreateProtector(Purpose);
            return protector.Unprotect(cipherText);
        }
    }
}
