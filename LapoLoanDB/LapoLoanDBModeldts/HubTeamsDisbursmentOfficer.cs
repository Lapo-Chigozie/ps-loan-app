using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class HubTeamsDisbursmentOfficer
{
    [Key]
    public long Id { get; set; }

    public long? HubTeamId { get; set; }

    public long? CreatedByAccountId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastDisbursmentDate { get; set; }

    public long? HubGroupId { get; set; }

    [ForeignKey("CreatedByAccountId")]
    [InverseProperty("HubTeamsDisbursmentOfficers")]
    public virtual SecurityAccount? CreatedByAccount { get; set; }

    [ForeignKey("HubGroupId")]
    [InverseProperty("HubTeamsDisbursmentOfficers")]
    public virtual HubTeamGroup? HubGroup { get; set; }

    [ForeignKey("HubTeamId")]
    [InverseProperty("HubTeamsDisbursmentOfficers")]
    public virtual HubTeam? HubTeam { get; set; }
}
