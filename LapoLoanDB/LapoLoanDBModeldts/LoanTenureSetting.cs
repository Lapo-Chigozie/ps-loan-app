using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanTenureSetting
{
    [Key]
    public long Id { get; set; }

    [Unicode(false)]
    public string? TeunerName { get; set; }

    public long? CreatedById { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    public double? LoanInterest { get; set; }

    [ForeignKey("CreatedById")]
    [InverseProperty("LoanTenureSettings")]
    public virtual SecurityAccount? CreatedBy { get; set; }

    [InverseProperty("LoanTenure")]
    public virtual ICollection<LoanApplicationRequestHeader> LoanApplicationRequestHeaders { get; set; } = new List<LoanApplicationRequestHeader>();
}
