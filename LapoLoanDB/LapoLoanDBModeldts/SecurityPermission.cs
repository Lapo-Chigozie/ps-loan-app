using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class SecurityPermission
{
    [Key]
    public long Id { get; set; }

    [Unicode(false)]
    public string? UserId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActivatedDate { get; set; }

    [Column("Staff_Name")]
    [Unicode(false)]
    public string? StaffName { get; set; }

    public bool? IsBlocked { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BlockedDate { get; set; }

    public long? AccountId { get; set; }

    public bool? HasPermissionToCreatedStaff { get; set; }

    public bool? HasPermissionToDisableStaff { get; set; }

    public bool? IsSupperAdmin { get; set; }

    [Column("Statt_ID")]
    [Unicode(false)]
    public string? StattId { get; set; }

    public bool? AccessRightApprovedLoan { get; set; }

    public bool? TenureAccessRight { get; set; }

    public bool? LoanSettingAccessRight { get; set; }

    public bool? NetPaysAccessRight { get; set; }

    public bool? GeneralPermissionsAccessRight { get; set; }

    public bool? CustomerLoanPermission { get; set; }

    public bool? LoanCompletedAccessRight { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("SecurityPermissions")]
    public virtual SecurityAccount? Account { get; set; }
}
