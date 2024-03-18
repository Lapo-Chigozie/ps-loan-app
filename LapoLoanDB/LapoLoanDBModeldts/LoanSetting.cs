using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LoanSetting
{
    [Key]
    public long Id { get; set; }

    [Column(TypeName = "money")]
    public decimal? MinLoanAmount { get; set; }

    [Column(TypeName = "money")]
    public decimal? MaxLoanAmount { get; set; }

    public bool? UseSalaryAsMaxLoanAmount { get; set; }

    [Unicode(false)]
    public string? CreatedByName { get; set; }

    public bool? IsCustomerHasLoanPermission { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdated { get; set; }

    public long? LastUpdatedAccountId { get; set; }

    public bool? UseFlatRateLoanInterestCalculation { get; set; }

    public double? LoanInterest { get; set; }

    [Unicode(false)]
    public string? Message { get; set; }

    public bool? SendEmailToAppliedCustomersWhenApplicationIsSubmitted { get; set; }

    public bool? SendEmailToTeamLeadWhenApplicationIsSubmitted { get; set; }

    public bool? SendSmsToAppliedCustomersWhenApplicationIsSubmitted { get; set; }

    public bool? SendSmsToTeamLeadWhenApplicationIsSubmitted { get; set; }

    [InverseProperty("LoanSetting")]
    public virtual ICollection<LoanApplicationRequestHeader> LoanApplicationRequestHeaders { get; set; } = new List<LoanApplicationRequestHeader>();
}
