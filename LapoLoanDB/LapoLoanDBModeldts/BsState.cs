using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("bs_State")]
public partial class BsState
{
    [Key]
    [Column("State_Id")]
    public int StateId { get; set; }

    [Column("State_Code")]
    [StringLength(10)]
    [Unicode(false)]
    public string StateCode { get; set; } = null!;

    [Column("Sate_Name")]
    [StringLength(20)]
    [Unicode(false)]
    public string SateName { get; set; } = null!;

    [Column("Cnt_Id")]
    public short CntId { get; set; }

    public bool IsVisible { get; set; }

    [Column("State_Status")]
    public short StateStatus { get; set; }

    [InverseProperty("State")]
    public virtual ICollection<BsCity> BsCities { get; set; } = new List<BsCity>();
}
