using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanApplicationRequestHeader
{
    [Key]
    public long Id { get; set; }

    public long AccountId { get; set; }

    public long? ApprovedByAccountId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? RequestCode { get; set; }

    public DateTime? RequestDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    [Column("BVN")]
    [StringLength(15)]
    [Unicode(false)]
    public string? Bvn { get; set; }

    [Column("PFNumber")]
    [StringLength(15)]
    [Unicode(false)]
    public string? Pfnumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Unicode(false)]
    public string? ApprovedComment { get; set; }

    public long? LoanSettingId { get; set; }

    public long? LoanTenureId { get; set; }

    public double? LoanInterest { get; set; }

    [Column(TypeName = "money")]
    public decimal? MinAmount { get; set; }

    [Column(TypeName = "money")]
    public decimal? MaxAmount { get; set; }

    [Unicode(false)]
    public string? TeamOfficerFirstname { get; set; }

    [Unicode(false)]
    public string? TeamOfficerOthername { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DisbursementDate { get; set; }

    public long? DisbursementById { get; set; }

    [Column(TypeName = "money")]
    public decimal? DisbursementAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExportedDate { get; set; }

    public long? ExportedById { get; set; }

    [Unicode(false)]
    public string? TeamGroundName { get; set; }

    public bool? IsGroundStandardOrAnonymous { get; set; }

    [Unicode(false)]
    public string? Narration { get; set; }

    [Unicode(false)]
    public string? BeneficialaryName { get; set; }

    [Unicode(false)]
    public string? RelationshipOfficerRef { get; set; }

    [Unicode(false)]
    public string? City { get; set; }

    [Unicode(false)]
    public string? State { get; set; }

    [Unicode(false)]
    public string? IndustrySegment { get; set; }

    [Unicode(false)]
    public string? BusinessType { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("LoanApplicationRequestHeaderAccounts")]
    public virtual SecurityAccount Account { get; set; } = null!;

    [ForeignKey("ApprovedByAccountId")]
    [InverseProperty("LoanApplicationRequestHeaderApprovedByAccounts")]
    public virtual SecurityAccount? ApprovedByAccount { get; set; }

    [InverseProperty("LoadAppRequestHeader")]
    public virtual ICollection<Bvnverification> Bvnverifications { get; set; } = new List<Bvnverification>();

    [ForeignKey("DisbursementById")]
    [InverseProperty("LoanApplicationRequestHeaderDisbursementBies")]
    public virtual SecurityAccount? DisbursementBy { get; set; }

    [ForeignKey("ExportedById")]
    [InverseProperty("LoanApplicationRequestHeaderExportedBies")]
    public virtual SecurityAccount? ExportedBy { get; set; }

    [InverseProperty("LoanAppRequestHeader")]
    public virtual ICollection<KycDetail> KycDetails { get; set; } = new List<KycDetail>();

    [InverseProperty("AccountAppRequestHeader")]
    public virtual ICollection<LoanAppRequestTransation> LoanAppRequestTransations { get; set; } = new List<LoanAppRequestTransation>();

    [InverseProperty("LoanAppRequestHeader")]
    public virtual ICollection<LoanApplicationRequestDetail> LoanApplicationRequestDetails { get; set; } = new List<LoanApplicationRequestDetail>();

    [InverseProperty("LoanRequestHeader")]
    public virtual ICollection<LoanApplicationRequestRepaymentDetail> LoanApplicationRequestRepaymentDetails { get; set; } = new List<LoanApplicationRequestRepaymentDetail>();

    [InverseProperty("LoanAppRequestHeader")]
    public virtual ICollection<LoanReview> LoanReviews { get; set; } = new List<LoanReview>();

    [ForeignKey("LoanSettingId")]
    [InverseProperty("LoanApplicationRequestHeaders")]
    public virtual LoanSetting? LoanSetting { get; set; }

    [ForeignKey("LoanTenureId")]
    [InverseProperty("LoanApplicationRequestHeaders")]
    public virtual LoanTenureSetting? LoanTenure { get; set; }

    [InverseProperty("LoanHeader")]
    public virtual ICollection<RepaymentLoan> RepaymentLoans { get; set; } = new List<RepaymentLoan>();
}
