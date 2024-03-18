using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("bs_City")]
public partial class BsCity
{
    [Key]
    [Column("City_Id")]
    public int CityId { get; set; }

    [Column("City_Code")]
    [StringLength(10)]
    [Unicode(false)]
    public string CityCode { get; set; } = null!;

    [Column("CIty_Name")]
    [StringLength(100)]
    [Unicode(false)]
    public string CityName { get; set; } = null!;

    [Column("State_Id")]
    public int StateId { get; set; }

    public bool IsVisible { get; set; }

    [Column("City_Status")]
    public short CityStatus { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("BsCities")]
    public virtual BsState State { get; set; } = null!;
}
