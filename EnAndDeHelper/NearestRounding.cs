namespace LapoLoanWebApi.EnAndDeHelper
{
    public class NearestRounding
    {
        public static int RoundValueToNext100(double value)
        {
            int result = (int)Math.Round(value / 100);
            if (value > 0 && result == 0)
            {
                result = 1;
            }
            return (int)result * 100;
        }

        private int RoundValueToNext1001(double value)
        {
            return (int)(Math.Ceiling(value / 100) * 100);
        }

        private static int RoundValueToNext1002(double value)
        {
            int result = (int)Math.Round(value / 100, 0, MidpointRounding.AwayFromZero);

            if (value > 0 && result == 0)
            {
                result = 1;
            }
            return (int)result * 100;
        }
    }
}
