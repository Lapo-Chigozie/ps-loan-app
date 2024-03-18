using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanReview
{
    [Key]
    public long Id { get; set; }

    public long AccountRequestId { get; set; }

    public long LoanAppRequestHeaderId { get; set; }

    public long? ApprovedByAccountId { get; set; }

    public long? ReviewByAccountId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ReviewDate { get; set; }

    [Unicode(false)]
    public string? Note { get; set; }

    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("AccountRequestId")]
    [InverseProperty("LoanReviewAccountRequests")]
    public virtual SecurityAccount AccountRequest { get; set; } = null!;

    [ForeignKey("ApprovedByAccountId")]
    [InverseProperty("LoanReviewApprovedByAccounts")]
    public virtual SecurityAccount? ApprovedByAccount { get; set; }

    [ForeignKey("LoanAppRequestHeaderId")]
    [InverseProperty("LoanReviews")]
    public virtual LoanApplicationRequestHeader LoanAppRequestHeader { get; set; } = null!;

    [ForeignKey("ReviewByAccountId")]
    [InverseProperty("LoanReviewReviewByAccounts")]
    public virtual SecurityAccount? ReviewByAccount { get; set; }
}
