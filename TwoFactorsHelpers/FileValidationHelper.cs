using LapoLoanWebApi.EnAndDeHelper;
using LapoLoanWebApi.ModelDto;

namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class FileValidationHelper
    {
        FileValidationHelper()
        {

        }

        public static async Task<RespondMessageDto> ValidateSlipFileImage(FileValidationModel fileValidation)
        {
            try
            {
                var arryImgFormart = DefalutToken.SlipImageFormats;

                var extention = System.IO.Path.GetExtension(fileValidation.FileName);

                if (!string.IsNullOrEmpty(extention) && !string.IsNullOrWhiteSpace(extention))
                {
                    var exteionExit = arryImgFormart.Where(e => e.ToLower().Contains(extention.ToLower()) && e.ToLower().Equals(extention.ToLower())).Any();
                    if (exteionExit)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "File is correct and accept.", true, fileValidation.FileName, fileValidation.FileName, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "You must upload image format of .png .jpeg .jpg, .pdf", false, fileValidation.FileName, fileValidation.FileName, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public static async Task<RespondMessageDto> ValidateFileImage(FileValidationModel fileValidation)
        {
            try
            {
                var arryImgFormart = DefalutToken.ImageFormats;

                var extention = System.IO.Path.GetExtension(fileValidation.FileName);

                if (!string.IsNullOrEmpty(extention) && !string.IsNullOrWhiteSpace(extention))
                {
                    var exteionExit = arryImgFormart.Where(e => e.ToLower().Contains(extention.ToLower()) && e.ToLower().Equals(extention.ToLower())).Any();
                    if (exteionExit)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "File is correct and accept.", true, fileValidation.FileName, fileValidation.FileName, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "You must upload image format of .png .jpeg .jpg", false, fileValidation.FileName, fileValidation.FileName, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }

        public static async Task<RespondMessageDto> ValidateFileExcel(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, FileValidationModel fileValidation)
        {
            try
            {
                var arryImgFormart = DefalutToken.ExcelFormats;

                var extention = System.IO.Path.GetExtension(fileValidation.FileName);

                if (!string.IsNullOrEmpty(extention) && !string.IsNullOrWhiteSpace(extention))
                {
                    var exteionExit = arryImgFormart.Where(e => e.ToLower().Contains(extention.ToLower()) && e.ToLower().Equals(extention.ToLower())).Any();
                    if (exteionExit)
                    {
                        return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "File is correct and accept.", true, fileValidation.FileName, fileValidation.FileName, Status.Success, StatusMgs.Success);
                    }
                }

                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Success, "Upload excel file and try again", false, fileValidation.FileName, fileValidation.FileName, Status.Success, StatusMgs.Success);
            }
            catch (System.Exception ex)
            {
                return new RespondMessageDto(null, null, null, null, null, null, StatusMgs.Error, ex.Message ?? ex.InnerException.Message, false, ex, null, Status.Ërror, StatusMgs.Error);
            }
        }
    }
}
