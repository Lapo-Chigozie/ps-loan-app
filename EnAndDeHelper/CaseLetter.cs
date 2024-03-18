using System.Globalization;

namespace LapoLoanWebApi.EnAndDeHelper
{
    public class CaseLetter
    {
        public CaseLetter() { }

        public static string ChangeAllCaseLetterToUpper(string name) {
           string fullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
        return fullName;
    }
    }
}
