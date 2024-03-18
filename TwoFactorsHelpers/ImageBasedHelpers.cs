using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class ImageBasedHelpers
    {
      public  string GetBase64Uri(string imgFile)
        {
            var ext = Path.GetExtension(imgFile);
            if (dicSupportedFormats.TryGetValue(ext, out var typeStr))
                return $"<img src=\"data:image/{typeStr};base64,{Convert.ToBase64String(File.ReadAllBytes(imgFile))}\" />";
            else
                return null;
        }

        public Dictionary<string, string> dicSupportedFormats = new Dictionary<string, string>
        {
                {".jpg", "jpeg"},
                {".jpeg", "jpeg"},
                {".jfif", "jpeg"},
                {".pjp", "jpeg"},
                {".pjpeg", "jpeg"},
                {".png", "png"},
                {".svg", "svg+xml"},
                {".webp", "webp"},
                {".gif", "gif"},
                {".avif", "avif"},
                {".apng", "apng"},
                {".ico", "x-icon"},
                {".cur", "x-icon"},
                {".tiff", "tiff"},
                {".tif", "tiff"},
        };

        public static string GetDataURL(string imgFile)
        {
            return "<img src=\"data:image/"
                        + Path.GetExtension(imgFile).Replace(".", "")
                        + ";base64,"
                        + Convert.ToBase64String(File.ReadAllBytes(imgFile)) + "\" />";
        }

        //public File  GetImage(byte[] data)
        //{
        //    MemoryStream ms = new MemoryStream(data);
        //    return FileInfo(ms.ToArray(), "image/png");
        //}

        //public static string DataUriContent(this UrlHelper url, string path)
        //{
        //    var filePath = Server.MapPath(path);
        //    var sb = new StringBuilder();
        //    sb.Append("data:image/")
        //        .Append((Path.GetExtension(filePath) ?? "png").Replace(".", ""))
        //        .Append(";base64,")
        //        .Append(Convert.ToBase64String(File.ReadAllBytes(filePath)));
        //    return sb.ToString();
        //}

        //<img src="data:image;base64,@System.Convert.ToBase64String(Model.Image)" />

        //<img src = "@Url.Action("GetImage", "MyController", new { data = Model.Image })" />

        //<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==' class='form-avatar'></img>
    }
}
