using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class HubTeamReconciliationOfficer
{
    [Key]
    public long Id { get; set; }

    [Column("Hub_TeamsId")]
    public long? HubTeamsId { get; set; }

    [Column("HUB_TEAM_SubGroupId")]
    public long? HubTeamSubGroupId { get; set; }

    public long? CreatedByAccountId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RemovedDate { get; set; }

    public long? RemovedByAccountId { get; set; }

    [ForeignKey("CreatedByAccountId")]
    [InverseProperty("HubTeamReconciliationOfficerCreatedByAccounts")]
    public virtual SecurityAccount? CreatedByAccount { get; set; }

    [ForeignKey("HubTeamSubGroupId")]
    [InverseProperty("HubTeamReconciliationOfficers")]
    public virtual HubTeamGroup? HubTeamSubGroup { get; set; }

    [ForeignKey("HubTeamsId")]
    [InverseProperty("HubTeamReconciliationOfficers")]
    public virtual HubTeam? HubTeams { get; set; }

    [ForeignKey("RemovedByAccountId")]
    [InverseProperty("HubTeamReconciliationOfficerRemovedByAccounts")]
    public virtual SecurityAccount? RemovedByAccount { get; set; }
}
