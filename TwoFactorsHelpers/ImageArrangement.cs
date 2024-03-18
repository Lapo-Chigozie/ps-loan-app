namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class ImageArrangement
    {
        public ImageArrangement() { }
        public static string GenerateImageData()
        {
            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string path = "HtmlPackages";
                string fullPath = Path.Combine(currentDirectory, path, "headerlogo.png");
               

                byte[] filebytes = System.IO.File.ReadAllBytes(fullPath);

                var urldata = "data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);

                return urldata; // "<%=" +   + " %>";
            }
            catch (Exception ex)
            {
                try
                {

                    byte[] filebytes = System.IO.File.ReadAllBytes("https://www.lapo-nigeria.org/img/logo/LAPO_Logo_2022.png");

                    var urldata = "data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);

                    return urldata;
                }
                catch (Exception exx)
                {
                    return "";
                }
            }
        }
    }
}
