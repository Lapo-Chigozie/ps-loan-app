using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanApplicationRequestDetail
{
    [Key]
    public long Id { get; set; }

    public long AccountRequestId { get; set; }

    public long LoanAppRequestHeaderId { get; set; }

    public long? UpdatedByAccountId { get; set; }

    [Column(TypeName = "money")]
    public decimal? Amount { get; set; }

    [Unicode(false)]
    public string? Tenure { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BankAccount { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BankAccountName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BankAccountNumber { get; set; }

    public DateTime CreatedDate { get; set; }

    [Unicode(false)]
    public string? PassportUrl { get; set; }

    [Column("IDCardUrl")]
    [Unicode(false)]
    public string? IdcardUrl { get; set; }

    [Unicode(false)]
    public string? PaySlipUrl { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("AccountRequestId")]
    [InverseProperty("LoanApplicationRequestDetailAccountRequests")]
    public virtual SecurityAccount AccountRequest { get; set; } = null!;

    [ForeignKey("LoanAppRequestHeaderId")]
    [InverseProperty("LoanApplicationRequestDetails")]
    public virtual LoanApplicationRequestHeader LoanAppRequestHeader { get; set; } = null!;

    [ForeignKey("UpdatedByAccountId")]
    [InverseProperty("LoanApplicationRequestDetailUpdatedByAccounts")]
    public virtual SecurityAccount? UpdatedByAccount { get; set; }
}
