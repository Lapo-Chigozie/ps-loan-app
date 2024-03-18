using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

[Table("bs_Bank_Names")]
public partial class BsBankName
{
    [Column("Bank_Id")]
    public short BankId { get; set; }

    [Key]
    [Column("Bank_ShortCode")]
    [StringLength(5)]
    [Unicode(false)]
    public string BankShortCode { get; set; } = null!;

    [Column("Bank_LongCode")]
    [StringLength(15)]
    [Unicode(false)]
    public string? BankLongCode { get; set; }

    [Column("Bank_Name")]
    [StringLength(70)]
    [Unicode(false)]
    public string? BankName { get; set; }

    [Column("Bank_Slug")]
    [StringLength(80)]
    [Unicode(false)]
    public string? BankSlug { get; set; }

    [Column("Bank_Gateway")]
    [StringLength(20)]
    [Unicode(false)]
    public string? BankGateway { get; set; }

    [Column("Bank_Status")]
    public bool? BankStatus { get; set; }

    [Column("Bank_Country")]
    public short? BankCountry { get; set; }

    [Column("Bank_Currency")]
    [StringLength(10)]
    [Unicode(false)]
    public string? BankCurrency { get; set; }

    [Column("Bank_Acc_Type")]
    [StringLength(20)]
    [Unicode(false)]
    public string? BankAccType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateCreated { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateUpdated { get; set; }

    [Column("isVisible")]
    public bool? IsVisible { get; set; }
}
