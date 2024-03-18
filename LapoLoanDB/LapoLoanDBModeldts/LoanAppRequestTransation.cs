using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanAppRequestTransation
{
    [Key]
    public long Id { get; set; }

    public long AccountRequestId { get; set; }

    public long AccountAppRequestHeaderId { get; set; }

    public long? CreditedByAccountId { get; set; }

    public long? DebitedByAccountId { get; set; }

    [Column(TypeName = "money")]
    public decimal Credit { get; set; }

    [Column(TypeName = "money")]
    public decimal Debit { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string VoucherNo { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string TransactionStatus { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? LoanTransactionStatus { get; set; }

    [ForeignKey("AccountAppRequestHeaderId")]
    [InverseProperty("LoanAppRequestTransations")]
    public virtual LoanApplicationRequestHeader AccountAppRequestHeader { get; set; } = null!;

    [ForeignKey("AccountRequestId")]
    [InverseProperty("LoanAppRequestTransationAccountRequests")]
    public virtual SecurityAccount AccountRequest { get; set; } = null!;

    [ForeignKey("CreditedByAccountId")]
    [InverseProperty("LoanAppRequestTransationCreditedByAccounts")]
    public virtual SecurityAccount? CreditedByAccount { get; set; }

    [ForeignKey("DebitedByAccountId")]
    [InverseProperty("LoanAppRequestTransationDebitedByAccounts")]
    public virtual SecurityAccount? DebitedByAccount { get; set; }
}
