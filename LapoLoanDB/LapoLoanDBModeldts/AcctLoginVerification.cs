using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class AcctLoginVerification
{
    [Key]
    public long Id { get; set; }

    public long AccountId { get; set; }

    [Column("BVNVerification")]
    [StringLength(15)]
    [Unicode(false)]
    public string? Bvnverification { get; set; }

    [StringLength(6)]
    [Unicode(false)]
    public string Code { get; set; } = null!;

    public DateTime GenaratedDateTime { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ExpiredDateTime { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("AcctLoginVerifications")]
    public virtual SecurityAccount Account { get; set; } = null!;
}
