using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("bs_Ministries")]
public partial class BsMinistry
{
    [Key]
    [Column("Mins_Id")]
    public short MinsId { get; set; }

    [Column("Mins_Name")]
    [StringLength(100)]
    [Unicode(false)]
    public string? MinsName { get; set; }

    [Column("Mins_Scope_Id")]
    public short? MinsScopeId { get; set; }

    [Column("isVisible")]
    public bool? IsVisible { get; set; }
}
