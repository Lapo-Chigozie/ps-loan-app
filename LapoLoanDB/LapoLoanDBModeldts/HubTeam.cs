using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("Hub_Teams")]
public partial class HubTeam
{
    [Key]
    public long Id { get; set; }

    public long? TeamAccountId { get; set; }

    public long? CreatedByAccountId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedDate { get; set; }

    public bool? IsBlocked { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? HubMemberLastNames { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? HubMemberFirstNames { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? HubMemberOtherNames { get; set; }

    [Column("HubMemberID")]
    [StringLength(50)]
    [Unicode(false)]
    public string? HubMemberId { get; set; }

    public long? GroupId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BackEndUsers { get; set; }

    public bool? AccessRightToViewDisbursementLoan { get; set; }

    public bool? AccessRightToViewUploadBackRepaymentLoan { get; set; }

    [Column("AccessRightToExportDISBURSEMENTLoan")]
    public bool? AccessRightToExportDisbursementloan { get; set; }

    public bool? AccessRightToAnonymousLoanApplication { get; set; }

    [Column("AccessRightToUploadBackDISBURSEMENTLoan")]
    public bool? AccessRightToUploadBackDisbursementloan { get; set; }

    public bool? AccessRightToUploadBackRepaymentLoan { get; set; }

    public bool? AccessRightToPrintLoan { get; set; }

    public bool? AccessRightToProceedLoan { get; set; }

    public bool? ViewLoanNarration { get; set; }

    public bool? CreateLoanNarration { get; set; }

    public bool? AccessRighttodisablecustomerstoapplyforaloan { get; set; }

    public bool? AccessRighttoviewcustomers { get; set; }

    public bool? AccessRighttodisablehubs { get; set; }

    public bool? AccessRighttoviewtenure { get; set; }

    public bool? AccessRighttocreatetenure { get; set; }

    public bool? AccessRighttoloansettings { get; set; }

    public bool? AccessRighttoteamsAndpermissions { get; set; }

    public bool? AccessRighttorejectaloan { get; set; }

    public bool? AccessRighttoviewcustomersloans { get; set; }

    public bool? AccessRighttoapprovecustomerloan { get; set; }

    public bool? AccessRighttoviewveammembers { get; set; }

    public bool? AccessRighttocreateateammember { get; set; }

    public bool? AccessRighttoviewhubs { get; set; }

    public bool? AccessRighttocreateahub { get; set; }

    public bool? AccessRighttoviewloandetails { get; set; }

    public bool? AccessRightToEditTeamMemberPermissions { get; set; }

    [Unicode(false)]
    public string? RefNo { get; set; }

    [ForeignKey("CreatedByAccountId")]
    [InverseProperty("HubTeamCreatedByAccounts")]
    public virtual SecurityAccount? CreatedByAccount { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("HubTeams")]
    public virtual HubTeamGroup? Group { get; set; }

    [InverseProperty("HubTeams")]
    public virtual ICollection<HubTeamManager> HubTeamManagers { get; set; } = new List<HubTeamManager>();

    [InverseProperty("HubTeams")]
    public virtual ICollection<HubTeamReconciliationOfficer> HubTeamReconciliationOfficers { get; set; } = new List<HubTeamReconciliationOfficer>();

    [InverseProperty("HubTeam")]
    public virtual ICollection<HubTeamsDisbursmentOfficer> HubTeamsDisbursmentOfficers { get; set; } = new List<HubTeamsDisbursmentOfficer>();

    [InverseProperty("UploadedByMember")]
    public virtual ICollection<LoanApplicationRequestRepaymentDetail> LoanApplicationRequestRepaymentDetails { get; set; } = new List<LoanApplicationRequestRepaymentDetail>();

    [ForeignKey("TeamAccountId")]
    [InverseProperty("HubTeamTeamAccounts")]
    public virtual SecurityAccount? TeamAccount { get; set; }
}
