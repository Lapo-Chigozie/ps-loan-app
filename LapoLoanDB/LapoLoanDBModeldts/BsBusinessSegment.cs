using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("bs_Business_Segment")]
public partial class BsBusinessSegment
{
    [Key]
    [Column("BizSeg_Id")]
    public short BizSegId { get; set; }

    [Column("BizSeg_Type")]
    [StringLength(10)]
    [Unicode(false)]
    public string BizSegType { get; set; } = null!;

    [Column("BizSeg_Desc")]
    [StringLength(50)]
    [Unicode(false)]
    public string BizSegDesc { get; set; } = null!;

    [Column("BizSeg_Status")]
    public short BizSegStatus { get; set; }

    public bool IsVisible { get; set; }
}
