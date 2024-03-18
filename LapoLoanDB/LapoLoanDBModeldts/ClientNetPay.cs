using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class ClientNetPay
{
    [Key]
    public long Id { get; set; }

    public long? ClientId { get; set; }

    public long? CreatedAccountById { get; set; }

    [Column(TypeName = "money")]
    public decimal? NetPay { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Unicode(false)]
    public string? Grade { get; set; }

    [Unicode(false)]
    public string? GradeStep { get; set; }

    [Unicode(false)]
    public string? Command { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? PayGroup { get; set; }

    [Unicode(false)]
    public string? BankAccountNumber { get; set; }

    [Unicode(false)]
    public string? BankAccountName { get; set; }

    [Unicode(false)]
    public string? BankName { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("NPFDate", TypeName = "datetime")]
    public DateTime? Npfdate { get; set; }

    public long? ClientMonthlyNetPayId { get; set; }

    [Column("Pay_Group")]
    [StringLength(50)]
    [Unicode(false)]
    public string? PayGroup1 { get; set; }

    [ForeignKey("ClientId")]
    [InverseProperty("ClientNetPays")]
    public virtual Client? Client { get; set; }

    [ForeignKey("ClientMonthlyNetPayId")]
    [InverseProperty("ClientNetPays")]
    public virtual ClientMonthlyNetPay? ClientMonthlyNetPay { get; set; }

    [ForeignKey("CreatedAccountById")]
    [InverseProperty("ClientNetPays")]
    public virtual SecurityAccount? CreatedAccountBy { get; set; }
}
