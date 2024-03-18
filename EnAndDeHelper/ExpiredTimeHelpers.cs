namespace LapoLoanWebApi.EnAndDeHelper
{
    public class ExpiredTimeHelpers
    {
        public static bool HasPassed2hoursFrom(DateTime fromDate, DateTime expireDate, int PassedHour)
        {
            return expireDate - fromDate > TimeSpan.FromHours(PassedHour);
        }

        public static bool HasPassed2hoursFrom1(DateTime now, DateTime expires, int PassedHour)
        {
            return (now - expires).TotalHours >= PassedHour;
        }

        public static bool IsExpired(DateTime specificDate)
        {
            bool flag = false;
            DateTime currentDate = DateTime.Now;

            DateTime target;

            if (DateTime.TryParse(specificDate.ToString(), out target))
            {
                flag = target < currentDate;
            }

            return flag;
        }

        public static bool IsExpired10(DateTime specificDate)
        {
            return specificDate < DateTime.Now;
        }
    }
}
