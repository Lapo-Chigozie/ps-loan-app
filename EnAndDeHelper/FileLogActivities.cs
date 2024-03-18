using LapoLoanWebApi.ModelDto;
using NETCore.Encrypt.Internal;
using Newtonsoft.Json.Linq;

namespace LapoLoanWebApi.EnAndDeHelper
{
    public class FileLogActivities
    {
        public static async void CallSevice(string ClassName, string comment, string Error)
        {
            try
            {
                //  System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                //  var folderName = Path.Combine("AccessTokenLogs", "Documents");

                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";

                string fullDir = Path.Combine(currentDirectory, path, "AccessErrorLog.txt");

                if (!string.IsNullOrEmpty(fullDir) && !string.IsNullOrWhiteSpace(fullDir) && System.IO.File.Exists(fullDir))
                {
                    var alltext = System.IO.File.ReadAllText(fullDir);

                    string[] strings = { alltext,   "__ __ ____ ___ __ ___ ___", "New Log", "Comment: " + comment, "Class Name: " + ClassName, "Created Date: " + DateTime.Now.ToString(), "Time: "  + DateTime.Now.ToLongTimeString(), "HttpRespond: " + Error };

                    System.IO.File.WriteAllLines(fullDir, strings);

                    string HtmData = System.IO.File.ReadAllText(fullDir);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
