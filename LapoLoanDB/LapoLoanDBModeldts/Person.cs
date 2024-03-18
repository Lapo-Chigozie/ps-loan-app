using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class Person
{
    [Key]
    public long Id { get; set; }

    public long AccountId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? LastName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? MiddleName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Age { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Staff { get; set; }

    [StringLength(18)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string? EmailAddress { get; set; }

    [StringLength(450)]
    [Unicode(false)]
    public string? CurrentAddress { get; set; }

    [StringLength(18)]
    [Unicode(false)]
    public string? AltPhoneNumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? PositionOrRole { get; set; }

    public DateTime CreatedDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UserRole { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EmploymentDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RetirementDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? MarrintalStatus { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("People")]
    public virtual SecurityAccount Account { get; set; } = null!;

    [InverseProperty("Person")]
    public virtual ICollection<SecurityAccount> SecurityAccounts { get; set; } = new List<SecurityAccount>();
}
