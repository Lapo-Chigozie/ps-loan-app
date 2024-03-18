using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class Client
{
    [Key]
    public long Id { get; set; }

    public long? CreatedAccountById { get; set; }

    public long? AccountId { get; set; }

    [Unicode(false)]
    public string? FullName { get; set; }

    [Column("PFNumber")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Pfnumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("NPFDate", TypeName = "datetime")]
    public DateTime? Npfdate { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("ClientAccounts")]
    public virtual SecurityAccount? Account { get; set; }

    [InverseProperty("Client")]
    public virtual ICollection<ClientNetPay> ClientNetPays { get; set; } = new List<ClientNetPay>();

    [ForeignKey("CreatedAccountById")]
    [InverseProperty("ClientCreatedAccountBies")]
    public virtual SecurityAccount? CreatedAccountBy { get; set; }
}
