using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class KycDetail
{
    [Key]
    public long Id { get; set; }

    public long AccountRequestId { get; set; }

    public long LoanAppRequestHeaderId { get; set; }

    [StringLength(18)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [Unicode(false)]
    public string? EmailAddress { get; set; }

    [StringLength(18)]
    [Unicode(false)]
    public string? AltPhoneNumber { get; set; }

    [Unicode(false)]
    public string? CurrentAddress { get; set; }

    public DateTime? RetirementDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? MaritalStatus { get; set; }

    [Unicode(false)]
    public string? NokName { get; set; }

    [StringLength(18)]
    [Unicode(false)]
    public string? NokPhoneNumber { get; set; }

    [Unicode(false)]
    public string? NokAddress { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateOfBirth { get; set; }

    public long? Age { get; set; }

    [Unicode(false)]
    public string? FullName { get; set; }

    [Column("PFNumber")]
    [Unicode(false)]
    public string? Pfnumber { get; set; }

    [Unicode(false)]
    public string? NokRelationShip { get; set; }

    [ForeignKey("AccountRequestId")]
    [InverseProperty("KycDetails")]
    public virtual SecurityAccount AccountRequest { get; set; } = null!;

    [ForeignKey("LoanAppRequestHeaderId")]
    [InverseProperty("KycDetails")]
    public virtual LoanApplicationRequestHeader LoanAppRequestHeader { get; set; } = null!;
}
