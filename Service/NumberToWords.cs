using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;
using Microsoft.AspNetCore.Mvc;

namespace LapoLoanWebApi.Service
{
    public class NumberToWords
    {
        private ControllerBase controllerBase { get; set; }

        private LapoLoanDBContext lapoLoanDB = null;

        private LapoCipher02 lapoCipher02 { get; set; }
        private LapoCipher02 ReallapoCipher02 { get; set; }
        private LapoCipher00 lapoCipher00 { get; set; }
        private LapoCipher01 lapoCipher01 { get; set; }

        private IConfiguration _configuration;


        public NumberToWords(ControllerBase controllerBase, IConfiguration _configuration
)
        {
            this._configuration = _configuration;
            this.lapoCipher01 = new LapoCipher01();
            this.lapoLoanDB = new LapoLoanDBContext(this._configuration);
            this.controllerBase = controllerBase;
            this.lapoCipher00 = new LapoCipher00();
            this.ReallapoCipher02 = new LapoCipher02();
            this.lapoCipher02 = new LapoCipher02();
        }

        public async Task<string> GetNumberToWord1(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + await GetNumberToWord1(Math.Abs(number));

            var words = "";

            if (number / 1000000000 > 0)
            {
                words += await GetNumberToWord1(number / 1000000000) + " billion ";
                number %= 1000000000;
            }

            if (number / 1000000 > 0)
            {
                words += await GetNumberToWord1(number / 1000000) + " million ";
                number %= 1000000;
            }

            if (number / 1000 > 0)
            {
                words += await GetNumberToWord1(number / 1000) + " thousand ";
                number %= 1000;
            }

            if (number / 100 > 0)
            {
                words += await GetNumberToWord1(number / 100) + " hundred ";
                number %= 100;
            }

            words = await SmallNumberToWord(number, words);

            return words;
        }
        private async Task<string> SmallNumberToWord(int number, string words)
        {
            if (number <= 0) return words;
            if (words != "")
                words += " ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }

            return words;
        }
        public async Task<string> GetNumberToWord(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + await GetNumberToWord(Math.Abs(number));

            string words = "";

            if ((number / 1000000000) > 0)
            {
                words += await GetNumberToWord(number / 1000000000) + " billion ";
                number %= 1000000000;
            }

            if ((number / 1000000) > 0)
            {
                words += await GetNumberToWord(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += await GetNumberToWord(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += await GetNumberToWord(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += " ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        private string[] units = { "Zero", "One", "Two", "Three",
    "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
    "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
    "Seventeen", "Eighteen", "Nineteen" };
       
        private string[] tens = { "", "", "Twenty", "Thirty", "Forty",
    "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        public async Task<string> ConvertAmount(double amount)
        {
            try
            {
                Console.WriteLine("Enter a Number to convert to words");

                string number = Console.ReadLine();

                number = await ConvertAmount(double.Parse(number));

                Console.WriteLine("Number in words is \n{0}", number);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                Int64 amount_int = (Int64)amount;
                Int64 amount_dec = (Int64)Math.Round((amount - (double)(amount_int)) * 100);
                if (amount_dec == 0)
                {
                    return Convert1(amount_int) + " Only.";
                }
                else
                {
                    return Convert1(amount_int) + " Point " + Convert1(amount_dec) + " Only.";
                }
            }
            catch (Exception e)
            {
                // TODO: handle exception  
            }
            return "";
        }

        private async Task<string> Convert1(Int64 i)
        {
            try
            {
                if (i < 20)
                {
                    return units[i];
                }
                if (i < 100)
                {
                    return tens[i / 10] + ((i % 10 > 0) ? " " + Convert1(i % 10) : "");
                }
                if (i < 1000)
                {
                    return units[i / 100] + " Hundred"
                            + ((i % 100 > 0) ? " And " + Convert1(i % 100) : "");
                }
                if (i < 100000)
                {
                    return Convert1(i / 1000) + " Thousand "
                            + ((i % 1000 > 0) ? " " + Convert1(i % 1000) : "");
                }
                if (i < 10000000)
                {
                    return Convert1(i / 100000) + " Lakh "
                            + ((i % 100000 > 0) ? " " + Convert1(i % 100000) : "");
                }
                if (i < 1000000000)
                {
                    return Convert1(i / 10000000) + " Crore "
                            + ((i % 10000000 > 0) ? " " + Convert1(i % 10000000) : "");
                }

                return Convert1(i / 1000000000) + " Arab "
                        + ((i % 1000000000 > 0) ? " " + Convert1(i % 1000000000) : "");
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task<string> Get1NumberToWords(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + Get1NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += Get1NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += Get1NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += Get1NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
        private async Task<string> ConvertToWords(String numb)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = "Only";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = "and";// just to separate whole numbers from points/cents    
                        endStr = "Paisa " + endStr;//Cents    
                        pointStr = await ConvertDecimals(points);
                    }
                }

                var NotPay = await ConvertWholeNumber(wholeNo);

                val = String.Format("{0} {1}{2} {3}", NotPay.Trim(), andStr, pointStr, endStr);

                return val;
            }
            catch (Exception ex)
            {
                return val;
            }
        }

        private async Task<string> ConvertDecimals(string number)
        {
            string cd = "", digit = "", engOne = "";

            try
            {
                for (int i = 0; i < number.Length; i++)
                {
                    digit = number[i].ToString();
                    if (digit.Equals("0"))
                    {
                        engOne = "Zero";
                    }
                    else
                    {
                        engOne = await Ones(digit);
                    }
                    cd += " " + engOne;
                }

                return cd;
            }
            catch (Exception ex)
            {
                return cd;
            }
        }

        private async Task<string> Tens(string Number)
        {
            int _Number = Convert.ToInt32(Number);
            string name = null;

            try
            {
                switch (_Number)
                {
                    case 10:
                        name = "Ten";
                        break;
                    case 11:
                        name = "Eleven";
                        break;
                    case 12:
                        name = "Twelve";
                        break;
                    case 13:
                        name = "Thirteen";
                        break;
                    case 14:
                        name = "Fourteen";
                        break;
                    case 15:
                        name = "Fifteen";
                        break;
                    case 16:
                        name = "Sixteen";
                        break;
                    case 17:
                        name = "Seventeen";
                        break;
                    case 18:
                        name = "Eighteen";
                        break;
                    case 19:
                        name = "Nineteen";
                        break;
                    case 20:
                        name = "Twenty";
                        break;
                    case 30:
                        name = "Thirty";
                        break;
                    case 40:
                        name = "Fourty";
                        break;
                    case 50:
                        name = "Fifty";
                        break;
                    case 60:
                        name = "Sixty";
                        break;
                    case 70:
                        name = "Seventy";
                        break;
                    case 80:
                        name = "Eighty";
                        break;
                    case 90:
                        name = "Ninety";
                        break;
                    default:
                        if (_Number > 0)
                        {
                            name = await Tens(Number.Substring(0, 1) + "0") + " " + await Ones(Number.Substring(1));
                        }
                        break;
                }

                return name;
            }
            catch (Exception ex)
            {
                return name;
            }
        }

        private async Task<string> ConvertWholeNumber(string Number)
        {
            string word = "";

            try
            {
                bool beginsZero = false;//tests for 0XX    
                bool isDone = false;//test if already translated    
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))    
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric    
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping    
                    string place = "";//digit grouping name:hundres,thousand,etc...    
                    switch (numDigits)
                    {
                        case 1://ones' range    

                            word = await Ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range    
                            word = await Tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range    
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range    
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range    
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range    
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...    
                        default:
                            isDone = true;
                            break;
                    }

                    if (!isDone)
                    {
                        //if transalation is not done, continue...(Recursion comes in now!!)    

                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = await ConvertWholeNumber(Number.Substring(0, pos)) + place + await ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = await ConvertWholeNumber(Number.Substring(0, pos)) + await ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros    
                        //if (beginsZero) word = " and " + word.Trim();    
                    }

                    //ignore digit grouping names    
                    if (word.Trim().Equals(place.Trim())) word = "";
                }

                return word.Trim();
            }
            catch (Exception e)
            {
                return word.Trim();
            }

        }

        private async Task<string> Ones(string Number)
        {
            string name = "";
            try
            {
                int _Number = Convert.ToInt32(Number);

                switch (_Number)
                {

                    case 1:
                        name = "One";
                        break;
                    case 2:
                        name = "Two";
                        break;
                    case 3:
                        name = "Three";
                        break;
                    case 4:
                        name = "Four";
                        break;
                    case 5:
                        name = "Five";
                        break;
                    case 6:
                        name = "Six";
                        break;
                    case 7:
                        name = "Seven";
                        break;
                    case 8:
                        name = "Eight";
                        break;
                    case 9:
                        name = "Nine";
                        break;
                }

                return name;
            }
            catch (Exception ex)
            {
                return name;
            }
        }

        private async Task<string> Convert_Click(string Number)
        {
            string ConvertedNumber = " ";
            int number = Convert.ToInt32(Number);
            int Count = 0;

            string[] ones = { " One ", " Two ", " Three ", " Four ", " Five ", " Six ", " Seven ", " Eight ", " Nine " };
            string[] teens = { " Eleven ", " Twelve ", " Thirteen ", " Fourtte n ", " Fifteen ", " Sixteen ", " Seventeen ", " Eighteen ", " Nineteen " };
            string[] tens = { " Ten ", " Twenty ", " Thirty ", " Fourty ", " Fifty ", " Sixty ", " Seventy ", " Eighty ", " Ninenty " };
            string[] moreThenTens = { " hundred ", " thousand ", " million " };

            try
            {
                while (number >= 0)
                {
                    if (number == 0)
                    {
                        ConvertedNumber = "Zero";
                    }

                    else if (number < 10)
                    {
                        for (int Counter = 1; Counter < 10; Counter++)
                        {
                            ConvertedNumber += ones[Counter - 1];
                        }
                    }

                    else if (number >= 10 && number < 100)
                    {
                        while (number > 10)
                        {
                            if (number > 10 && number < 20)
                            {
                                for (int x = 11; x < 20; x++)
                                {
                                    if (number == x)
                                    {
                                        ConvertedNumber = teens[x - 11];
                                    }
                                }
                            }

                            else
                            {
                                number -= 10;
                                Count++;
                            }

                            ConvertedNumber += tens[Count - 1];
                        }
                    }

                    else if (number >= 100 && number < 1000)
                    {
                        while (number > 100)
                        {
                            number -= 100;
                            Count++;
                        }
                        ConvertedNumber += ones[Count - 1] + moreThenTens[0];
                    }

                    else if (number >= 1000 && number < 10000)
                    {
                        while (number > 1000)
                        {
                            number -= 1000;
                            Count++;
                        }

                        ConvertedNumber += ones[Count - 1] + moreThenTens[1];
                    }

                    else if (number >= 10000 && number < 100000)
                    {
                        while (number > 10000)
                        {
                            if (number < 20000)
                            {
                                number -= 10000;
                                Count++;
                            }

                            else if (number >= 20000)
                            {
                                number -= 10000;
                                Count++;
                            }

                        }

                        if (Count >= 1)
                        {
                            if (number < 1000)
                            {
                                ConvertedNumber += tens[0] + moreThenTens[1];
                            }

                            else if (number >= 1000)
                            {
                                ConvertedNumber += teens[Count - 1] + moreThenTens[1];
                            }
                        }

                        else if (Count > 1)
                        {
                            ConvertedNumber += tens[Count - 1] + moreThenTens[1];
                        }
                    }

                    else if (number >= 100000 && number < 1000000)
                    {
                        while (number > 100000)
                        {
                            number -= 100000;
                            Count++;
                        }

                        ConvertedNumber += ones[Count - 1] + moreThenTens[0] + moreThenTens[1];
                    }

                    else if (number >= 1000000 && number < 10000000)
                    {
                        while (number > 1000000)
                        {
                            number -= 1000000;
                            Count++;
                        }

                        ConvertedNumber += ones[Count - 1] + moreThenTens[2];
                    }

                    else if (number == 10000000)
                    {
                        while (number > 10000000)
                        {
                            number -= 10000000;
                            Count++;
                        }

                        ConvertedNumber += tens[0] + moreThenTens[2];
                    }
                }

                return ConvertedNumber;
            }
            catch (Exception ex)
            {

            }

            return ConvertedNumber;
        }

        public async Task<string> DecimalToText(string decimalPart)
        {
            string[] digits = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            string result = "";
            foreach (char c in decimalPart)
            {
                int i = (int)c - 48;
                if (i < 0 || i > 9) return ""; // invalid number, don't return anything
                result += " " + digits[i];
            }
            return result;
        }

        public async Task<string> NumberToText(int number, bool useAnd)
        {
            if (number == 0) return "Zero";



            if (number == -2147483648) return "Minus Two Hundred " + "and " + "Fourteen Crore Seventy Four Lakh Eighty Three Thousand Six Hundred " + "and " + "Forty Eight";

            int[] num = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (number < 0)
            {
                sb.Append("Minus ");
                number = -number;
            }
            string[] words0 = { "", "One ", "Two ", "Three ", "Four ", "Five ", "Six ", "Seven ", "Eight ", "Nine " };
            string[] words1 = { "Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ", "Fifteen ", "Sixteen ", "Seventeen ", "Eighteen ", "Nineteen " };
            string[] words2 = { "Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ", "Seventy ", "Eighty ", "Ninety " };
            string[] words3 = { "Thousand ", "Lakh ", "Crore " };
            num[0] = number % 1000; // units
            num[1] = number / 1000;
            num[2] = number / 100000;
            num[1] = num[1] - 100 * num[2]; // thousands
            num[3] = number / 10000000; // crores
            num[2] = num[2] - 100 * num[3]; // lakhs
            for (int i = 3; i > 0; i--)
            {
                if (num[i] != 0)
                {
                    first = i;
                    break;
                }
            }

            for (int i = first; i >= 0; i--)
            {
                if (num[i] == 0) continue;

                u = num[i] % 10; // ones 
                t = num[i] / 10;
                h = num[i] / 100; // hundreds
                t = t - 10 * h; // tens

                if (h > 0) sb.Append(words0[h] + "Hundred ");
                if (u > 0 || t > 0)
                {
                    if (h > 0 || i < first) sb.Append("and ");

                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }
                if (i != 0) sb.Append(words3[i - 1]);
            }

            string temp = sb.ToString().TrimEnd();


            return temp;
        }

        public async Task<string> OriginalWord(int number)
        {
            string[] ones = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            string[] tens = { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            string word = "";
            if (number == 0)
            {
                Console.WriteLine(ones[number]);
                word += ones[number];
            }
            if (number > 0 && number <= 10)
            {
                Console.WriteLine(ones[number]);
                word += ones[number];
            }
            if (number > 10 && number < 20)
            {
                Console.WriteLine(ones[number]);
                word += ones[number];
            }
            if (number >= 20 && number < 100)
            {
                int last = Math.Abs(number) % 10;
                if (last > 0)
                {
                    Console.WriteLine(tens[number / 10] + " " + ones[last]);
                    word += tens[number / 10] + " " + ones[last];
                }
                else
                {
                    Console.WriteLine(tens[number / 10]);
                    word += tens[number / 10];
                }

            }
            if (number >= 100 && number <= 9999)
            {
                int spliter = Math.Abs(number / 10) % 10;
                if (number.ToString().Length == 2)
                {
                    if ((spliter / 100) % 10 > 10)
                    {
                        int first = number / 100;
                        int second = Math.Abs(number / 10) % 10;
                        word += ones[first] + " hundred " + tens[second];
                        Console.WriteLine(word);
                    }
                    else
                    {
                        int first = number / 100;
                        int second = Math.Abs(number / 10) % 10;
                        word += ones[first] + " hundred " + tens[second] + " " + ones[Math.Abs(number) % 10];
                        Console.WriteLine(word);
                    }
                }
                else if (number.ToString().Length >= 3)
                {
                    if ((number / 1000) % 10 > 0)
                    {
                        int first = number / 1000;
                        int secondDigit = Math.Abs(number / 100) % 10;
                        if (secondDigit > 0)
                        {
                            int thirdDigit = Math.Abs(number / 100) % 10;
                            int lastDigit = Math.Abs(number / 1000) / 10;
                            if (thirdDigit > 0)
                            {
                                if (lastDigit == 0)
                                {
                                    // if number contains 0 at second to the last index
                                    int last2Digit = Math.Abs(number) % 10;
                                    int secondToTheLast = Math.Abs(number) / 10 % 10;
                                    int thirdToTheLast = Math.Abs(number) % 100;
                                    int lastTwoDigits = Math.Abs(number) % 100;
                                    if (last2Digit > 0 && secondToTheLast > 0)
                                    {
                                        int lastTwoDigit = Math.Abs(number) % 100 / 10;
                                        int last = Math.Abs(thirdToTheLast) % 10;

                                        if (lastTwoDigits > 0 && lastTwoDigits < 20)
                                        {
                                            word += ones[first] + " thousand " + ones[number / 100 % 10] + " hundred " + ones[lastTwoDigits];
                                            Console.WriteLine(word);
                                        }
                                        else if (lastTwoDigits > 19)
                                        {
                                            word += ones[first] + " thousand " + ones[number / 100 % 10] + " hundred " + tens[lastTwoDigit] + " " + ones[last];
                                            Console.WriteLine(word);
                                        }

                                    }
                                    else
                                    {
                                        if (lastTwoDigits == 00)
                                        {
                                            word += ones[first] + " thousand " + ones[number / 100 % 10] + " hundred ";
                                            Console.WriteLine(word);
                                        }
                                        else
                                        {
                                            if (lastTwoDigits < 10)
                                            {
                                                word += ones[first] + " thousand " + ones[number / 100 % 10] + " hundred " + tens[secondToTheLast] + " " + ones[last2Digit];
                                                Console.WriteLine(word);
                                            }
                                            else
                                            {
                                                word += ones[first] + " thousand " + ones[number / 100 % 10] + " hundred " + tens[secondToTheLast];
                                                Console.WriteLine(word);
                                            }

                                        }

                                    }

                                }
                                else
                                {
                                    word += ones[first] + " thousand " + ones[number / 100 % 10] + " hundred " + tens[thirdDigit] + " " + ones[Math.Abs(number / 100) % 10];
                                    Console.WriteLine(word);
                                }

                            }
                            else
                            {

                                word += ones[first] + " thousand " + ones[number / 100 % 10] + " hundred " + tens[thirdDigit] + ones[Math.Abs(number) % 100];
                                Console.WriteLine(word);
                            }

                        }
                        else
                        {
                            if (
                                number == 1000
                                || number == 2000
                                || number == 3000
                                || number == 4000
                                || number == 5000
                                || number == 6000
                                || number == 7000
                                || number == 8000
                                || number == 9000
                             )
                            {
                                word += ones[number / 1000] + " thousand ";
                                Console.WriteLine(word);
                            }
                            else
                            {
                                int firstDigit = Math.Abs(Math.Abs(number) % 100) / 10;
                                int lastTwoDigit = Math.Abs(number) % 100;
                                int firstDig = Math.Abs(lastTwoDigit / 10) % 100;
                                int last = Math.Abs(lastTwoDigit) % 10;
                                if (Math.Abs(number) % 10 == 0)
                                {
                                    word += ones[first] + " thousand " + ones[number / 100 % 10] + " " + tens[firstDigit];
                                    Console.WriteLine(word);
                                }
                                else
                                {
                                    if (lastTwoDigit > 0 && lastTwoDigit < 20)
                                    {
                                        if (lastTwoDigit < 10)
                                        {
                                            word += ones[first] + " thousand " + ones[number / 100 % 10] + " " + ones[number / 100 % 10] + " " + ones[lastTwoDigit];
                                            Console.WriteLine(word);
                                        }
                                        else
                                        {
                                            word += ones[first] + " thousand " + ones[number / 100 % 10] + " " + ones[lastTwoDigit];
                                            Console.WriteLine(word);
                                        }

                                    }
                                    else if (lastTwoDigit > 19)
                                    {
                                        word += ones[first] + " thousand " + ones[number / 100 % 10] + " " + tens[firstDig] + " " + ones[last];
                                        Console.WriteLine(word);
                                    }

                                }

                            }


                        }

                    }
                    else
                    {
                        int first = Math.Abs(number / 100) % 10;
                        int middle = Math.Abs(number / 10) % 10;
                        int last = Math.Abs(number) % 10;
                        int last2Digit = Math.Abs(number) % 100;
                        Console.WriteLine(middle);
                        if (last2Digit > 0 && last2Digit < 20)
                        {
                            if (last2Digit < 10)
                            {
                                word += ones[first] + " hundred " + tens[middle] + " " + ones[last2Digit];
                                Console.WriteLine(word);
                            }
                            else
                            {
                                word += ones[first] + " hundred " + ones[last2Digit];
                                Console.WriteLine(word);
                            }

                        }
                        else if (last2Digit > 19)
                        {
                            word += ones[first] + " hundred " + tens[middle] + " " + ones[last];
                            Console.WriteLine(word);
                        }
                        else
                        {
                            word += ones[first] + " hundred " + tens[middle] + ones[last];
                            Console.WriteLine(word);
                        }
                    }
                }
            }
            else if (number >= 10000)
            {
                Console.WriteLine("Max number is 9999");
            }

            return word;
        }

        public async Task<string> NumberToWorders(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + await NumberToWorders(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += await NumberToWorders(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += await NumberToWorders(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += await NumberToWorders(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
    }
}