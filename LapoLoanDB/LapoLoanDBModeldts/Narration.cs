using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class Narration
{
    [Key]
    public long Id { get; set; }

    [Unicode(false)]
    public string? Name { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public long? CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    [InverseProperty("Narrations")]
    public virtual SecurityAccount? CreatedBy { get; set; }
}
