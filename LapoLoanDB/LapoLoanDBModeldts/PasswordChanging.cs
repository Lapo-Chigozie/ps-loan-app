using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class PasswordChanging
{
    [Key]
    public long Id { get; set; }

    public long AccountId { get; set; }

    public long ChangedByAccountId { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("PasswordChangingAccounts")]
    public virtual SecurityAccount Account { get; set; } = null!;

    [ForeignKey("ChangedByAccountId")]
    [InverseProperty("PasswordChangingChangedByAccounts")]
    public virtual SecurityAccount ChangedByAccount { get; set; } = null!;
}
