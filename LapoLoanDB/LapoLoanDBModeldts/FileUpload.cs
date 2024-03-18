using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class FileUpload
{
    [Key]
    public long Id { get; set; }

    [Unicode(false)]
    public string? Src { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UploadDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FinishUploadDate { get; set; }

    public bool? Status { get; set; }
}
