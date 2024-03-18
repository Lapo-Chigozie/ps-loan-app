using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanSheduled
{
    [Key]
    public long Id { get; set; }

    public double? Interest { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public long? CreatedById { get; set; }

    [Column(TypeName = "money")]
    public decimal? TotalAmount { get; set; }

    [Column(TypeName = "money")]
    public decimal? Balance { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NextSchduled { get; set; }

    [Column(TypeName = "money")]
    public decimal? NextSchduledAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApprovedDate { get; set; }

    [Column(TypeName = "money")]
    public decimal? AmountPaid { get; set; }
}
