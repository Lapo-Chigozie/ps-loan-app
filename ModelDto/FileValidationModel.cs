using System.ComponentModel.DataAnnotations;

namespace LapoLoanWebApi.ModelDto
{
    public class FileValidationModel
    {
        [Required]
        public string FileName { get; set; }
        [Required]
        public string AcctLogin { get; set; }

        [Required]
        public string size { get; set; }

    }
}
