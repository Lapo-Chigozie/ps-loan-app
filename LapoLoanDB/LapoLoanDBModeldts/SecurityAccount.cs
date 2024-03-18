using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class SecurityAccount
{
    [Key]
    public long Id { get; set; }

    public long? PersonId { get; set; }

    [Unicode(false)]
    public string? Password { get; set; }

    [StringLength(300)]
    [Unicode(false)]
    public string? Username { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; }

    [Unicode(false)]
    public string? AccountType { get; set; }

    public bool? AllowLoginTwoFactor { get; set; }

    public bool? AccountVerify { get; set; }

    [Unicode(false)]
    public string? BvnAccount { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<AcctLoginVerification> AcctLoginVerifications { get; set; } = new List<AcctLoginVerification>();

    [InverseProperty("AccountRequest")]
    public virtual ICollection<Bvnverification> Bvnverifications { get; set; } = new List<Bvnverification>();

    [InverseProperty("Account")]
    public virtual ICollection<Client> ClientAccounts { get; set; } = new List<Client>();

    [InverseProperty("CreatedAccountBy")]
    public virtual ICollection<Client> ClientCreatedAccountBies { get; set; } = new List<Client>();

    [InverseProperty("CreatedAccountBy")]
    public virtual ICollection<ClientNetPay> ClientNetPays { get; set; } = new List<ClientNetPay>();

    [InverseProperty("CreatedByAccount")]
    public virtual ICollection<HubTeam> HubTeamCreatedByAccounts { get; set; } = new List<HubTeam>();

    [InverseProperty("CreatedByAccount")]
    public virtual ICollection<HubTeamGroup> HubTeamGroups { get; set; } = new List<HubTeamGroup>();

    [InverseProperty("CreatedByAccount")]
    public virtual ICollection<HubTeamManager> HubTeamManagerCreatedByAccounts { get; set; } = new List<HubTeamManager>();

    [InverseProperty("RemovedByAccount")]
    public virtual ICollection<HubTeamManager> HubTeamManagerRemovedByAccounts { get; set; } = new List<HubTeamManager>();

    [InverseProperty("CreatedByAccount")]
    public virtual ICollection<HubTeamReconciliationOfficer> HubTeamReconciliationOfficerCreatedByAccounts { get; set; } = new List<HubTeamReconciliationOfficer>();

    [InverseProperty("RemovedByAccount")]
    public virtual ICollection<HubTeamReconciliationOfficer> HubTeamReconciliationOfficerRemovedByAccounts { get; set; } = new List<HubTeamReconciliationOfficer>();

    [InverseProperty("TeamAccount")]
    public virtual ICollection<HubTeam> HubTeamTeamAccounts { get; set; } = new List<HubTeam>();

    [InverseProperty("CreatedByAccount")]
    public virtual ICollection<HubTeamsDisbursmentOfficer> HubTeamsDisbursmentOfficers { get; set; } = new List<HubTeamsDisbursmentOfficer>();

    [InverseProperty("AccountRequest")]
    public virtual ICollection<KycDetail> KycDetails { get; set; } = new List<KycDetail>();

    [InverseProperty("AccountRequest")]
    public virtual ICollection<LoanAppRequestTransation> LoanAppRequestTransationAccountRequests { get; set; } = new List<LoanAppRequestTransation>();

    [InverseProperty("CreditedByAccount")]
    public virtual ICollection<LoanAppRequestTransation> LoanAppRequestTransationCreditedByAccounts { get; set; } = new List<LoanAppRequestTransation>();

    [InverseProperty("DebitedByAccount")]
    public virtual ICollection<LoanAppRequestTransation> LoanAppRequestTransationDebitedByAccounts { get; set; } = new List<LoanAppRequestTransation>();

    [InverseProperty("AccountRequest")]
    public virtual ICollection<LoanApplicationRequestDetail> LoanApplicationRequestDetailAccountRequests { get; set; } = new List<LoanApplicationRequestDetail>();

    [InverseProperty("UpdatedByAccount")]
    public virtual ICollection<LoanApplicationRequestDetail> LoanApplicationRequestDetailUpdatedByAccounts { get; set; } = new List<LoanApplicationRequestDetail>();

    [InverseProperty("Account")]
    public virtual ICollection<LoanApplicationRequestHeader> LoanApplicationRequestHeaderAccounts { get; set; } = new List<LoanApplicationRequestHeader>();

    [InverseProperty("ApprovedByAccount")]
    public virtual ICollection<LoanApplicationRequestHeader> LoanApplicationRequestHeaderApprovedByAccounts { get; set; } = new List<LoanApplicationRequestHeader>();

    [InverseProperty("DisbursementBy")]
    public virtual ICollection<LoanApplicationRequestHeader> LoanApplicationRequestHeaderDisbursementBies { get; set; } = new List<LoanApplicationRequestHeader>();

    [InverseProperty("ExportedBy")]
    public virtual ICollection<LoanApplicationRequestHeader> LoanApplicationRequestHeaderExportedBies { get; set; } = new List<LoanApplicationRequestHeader>();

    [InverseProperty("AccountRequest")]
    public virtual ICollection<LoanReview> LoanReviewAccountRequests { get; set; } = new List<LoanReview>();

    [InverseProperty("ApprovedByAccount")]
    public virtual ICollection<LoanReview> LoanReviewApprovedByAccounts { get; set; } = new List<LoanReview>();

    [InverseProperty("ReviewByAccount")]
    public virtual ICollection<LoanReview> LoanReviewReviewByAccounts { get; set; } = new List<LoanReview>();

    [InverseProperty("CreatedBy")]
    public virtual ICollection<LoanTenureSetting> LoanTenureSettings { get; set; } = new List<LoanTenureSetting>();

    [InverseProperty("CreatedBy")]
    public virtual ICollection<Narration> Narrations { get; set; } = new List<Narration>();

    [InverseProperty("Account")]
    public virtual ICollection<PasswordChanging> PasswordChangingAccounts { get; set; } = new List<PasswordChanging>();

    [InverseProperty("ChangedByAccount")]
    public virtual ICollection<PasswordChanging> PasswordChangingChangedByAccounts { get; set; } = new List<PasswordChanging>();

    [InverseProperty("Account")]
    public virtual ICollection<Person> People { get; set; } = new List<Person>();

    [ForeignKey("PersonId")]
    [InverseProperty("SecurityAccounts")]
    public virtual Person? Person { get; set; }

    [InverseProperty("CreatedAccountBy")]
    public virtual ICollection<RepaymentLoan> RepaymentLoans { get; set; } = new List<RepaymentLoan>();

    [InverseProperty("Account")]
    public virtual ICollection<SecurityPermission> SecurityPermissions { get; set; } = new List<SecurityPermission>();
}
