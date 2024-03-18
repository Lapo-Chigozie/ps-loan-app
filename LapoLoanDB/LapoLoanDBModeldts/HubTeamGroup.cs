using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("HUB_TEAM_Groups")]
public partial class HubTeamGroup
{
    [Key]
    public long Id { get; set; }

    public long? CreatedByAccountId { get; set; }

    [Column("HUB_TEAM_GroupName")]
    [Unicode(false)]
    public string? HubTeamGroupName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedDate { get; set; }

    public bool? IsGroupHead { get; set; }

    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [Unicode(false)]
    public string? EmailAddress { get; set; }

    [ForeignKey("CreatedByAccountId")]
    [InverseProperty("HubTeamGroups")]
    public virtual SecurityAccount? CreatedByAccount { get; set; }

    [InverseProperty("HubTeamSubGroup")]
    public virtual ICollection<HubTeamManager> HubTeamManagers { get; set; } = new List<HubTeamManager>();

    [InverseProperty("HubTeamSubGroup")]
    public virtual ICollection<HubTeamReconciliationOfficer> HubTeamReconciliationOfficers { get; set; } = new List<HubTeamReconciliationOfficer>();

    [InverseProperty("Group")]
    public virtual ICollection<HubTeam> HubTeams { get; set; } = new List<HubTeam>();

    [InverseProperty("HubGroup")]
    public virtual ICollection<HubTeamsDisbursmentOfficer> HubTeamsDisbursmentOfficers { get; set; } = new List<HubTeamsDisbursmentOfficer>();
}
