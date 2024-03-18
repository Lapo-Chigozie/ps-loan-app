using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanApplicationRequestRepaymentDetail
{
    [Key]
    public long Id { get; set; }

    public long? LoanRequestHeaderId { get; set; }

    public long? UploadedByMemberId { get; set; }

    [Column("IPFNumber")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Ipfnumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? LoanRequestCode { get; set; }

    [Column(TypeName = "money")]
    public decimal? RepaymentAmount { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? RepaymentStatus { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RepaymentDate { get; set; }

    [ForeignKey("LoanRequestHeaderId")]
    [InverseProperty("LoanApplicationRequestRepaymentDetails")]
    public virtual LoanApplicationRequestHeader? LoanRequestHeader { get; set; }

    [ForeignKey("UploadedByMemberId")]
    [InverseProperty("LoanApplicationRequestRepaymentDetails")]
    public virtual HubTeam? UploadedByMember { get; set; }
}
