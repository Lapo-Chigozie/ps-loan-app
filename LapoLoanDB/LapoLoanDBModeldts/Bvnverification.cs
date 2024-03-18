using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("BVNVerifications")]
public partial class Bvnverification
{
    [Key]
    public long Id { get; set; }

    public long AccountRequestId { get; set; }

    public long? LoadAppRequestHeaderId { get; set; }

    [Column("BVNVerification")]
    [StringLength(15)]
    [Unicode(false)]
    public string? Bvnverification1 { get; set; }

    [Unicode(false)]
    public string? Code { get; set; }

    public DateTime? GenaratedDateTime { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ExpiredDateTime { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("AccountRequestId")]
    [InverseProperty("Bvnverifications")]
    public virtual SecurityAccount AccountRequest { get; set; } = null!;

    [ForeignKey("LoadAppRequestHeaderId")]
    [InverseProperty("Bvnverifications")]
    public virtual LoanApplicationRequestHeader? LoadAppRequestHeader { get; set; }
}
