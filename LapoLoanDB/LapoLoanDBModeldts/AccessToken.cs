using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class AccessToken
{
    [Key]
    public int Id { get; set; }

    public int? Status { get; set; }

    [Column("Message_code")]
    [Unicode(false)]
    public string? MessageCode { get; set; }

    [Column("Message_description")]
    [Unicode(false)]
    public string? MessageDescription { get; set; }

    [Unicode(false)]
    public string? Data { get; set; }

    [Unicode(false)]
    public string? Timestamp { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    [Unicode(false)]
    public string? UserId { get; set; }

    [Unicode(false)]
    public string? RefreshToken { get; set; }

    [Unicode(false)]
    public string? AesKey { get; set; }

    [Unicode(false)]
    public string? AesIv { get; set; }
}
