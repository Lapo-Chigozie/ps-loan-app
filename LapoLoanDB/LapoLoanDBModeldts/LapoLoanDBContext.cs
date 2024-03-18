using System;
using System.Collections.Generic;
using LapoLoanWebApi.EnAndDeHelper;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class LapoLoanDBContext : DbContext
{
    private IConfiguration _configuration = null;
    public LapoLoanDBContext(IConfiguration _configuration)
    {
        this._configuration = _configuration;
    }

    public LapoLoanDBContext(DbContextOptions<LapoLoanDBContext> options)
        : base(options)
    {

    }

    public virtual DbSet<AccessToken> AccessTokens { get; set; }

    public virtual DbSet<AcctLoginVerification> AcctLoginVerifications { get; set; }

    public virtual DbSet<BsBankName> BsBankNames { get; set; }

    public virtual DbSet<BsBusinessSegment> BsBusinessSegments { get; set; }

    public virtual DbSet<BsCity> BsCities { get; set; }

    public virtual DbSet<BsMinistry> BsMinistries { get; set; }

    public virtual DbSet<BsState> BsStates { get; set; }

    public virtual DbSet<Bvnverification> Bvnverifications { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientMonthlyNetPay> ClientMonthlyNetPays { get; set; }

    public virtual DbSet<ClientNetPay> ClientNetPays { get; set; }

    public virtual DbSet<FileUpload> FileUploads { get; set; }

    public virtual DbSet<HubTeam> HubTeams { get; set; }

    public virtual DbSet<HubTeamGroup> HubTeamGroups { get; set; }

    public virtual DbSet<HubTeamManager> HubTeamManagers { get; set; }

    public virtual DbSet<HubTeamReconciliationOfficer> HubTeamReconciliationOfficers { get; set; }

    public virtual DbSet<HubTeamsDisbursmentOfficer> HubTeamsDisbursmentOfficers { get; set; }

    public virtual DbSet<KycDetail> KycDetails { get; set; }

    public virtual DbSet<LoanAppRequestTransation> LoanAppRequestTransations { get; set; }

    public virtual DbSet<LoanApplicationRequestDetail> LoanApplicationRequestDetails { get; set; }

    public virtual DbSet<LoanApplicationRequestHeader> LoanApplicationRequestHeaders { get; set; }

    public virtual DbSet<LoanApplicationRequestRepaymentDetail> LoanApplicationRequestRepaymentDetails { get; set; }

    public virtual DbSet<LoanReview> LoanReviews { get; set; }

    public virtual DbSet<LoanSetting> LoanSettings { get; set; }

    public virtual DbSet<LoanSheduled> LoanSheduleds { get; set; }

    public virtual DbSet<LoanTenureSetting> LoanTenureSettings { get; set; }

    public virtual DbSet<Narration> Narrations { get; set; }

    public virtual DbSet<PasswordChanging> PasswordChangings { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<RepaymentLoan> RepaymentLoans { get; set; }

    public virtual DbSet<SecurityAccount> SecurityAccounts { get; set; }

    public virtual DbSet<SecurityPermission> SecurityPermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(DefalutToken.GetConnectionString(this._configuration, DefalutToken.ConType));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcctLoginVerification>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.AcctLoginVerifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcctLoginVerifications_SecurityAccounts");
        });

        modelBuilder.Entity<BsBusinessSegment>(entity =>
        {
            entity.HasKey(e => e.BizSegId).HasName("PK_bs_Customer_Segment");

            entity.Property(e => e.BizSegId).ValueGeneratedNever();
        });

        modelBuilder.Entity<BsCity>(entity =>
        {
            entity.Property(e => e.CityId).ValueGeneratedNever();

            entity.HasOne(d => d.State).WithMany(p => p.BsCities)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bs_City_bs_State");
        });

        modelBuilder.Entity<BsMinistry>(entity =>
        {
            entity.Property(e => e.MinsId).ValueGeneratedNever();
        });

        modelBuilder.Entity<BsState>(entity =>
        {
            entity.HasKey(e => e.StateId).HasName("PK_State_t");

            entity.Property(e => e.StateId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Bvnverification>(entity =>
        {
            entity.HasOne(d => d.AccountRequest).WithMany(p => p.Bvnverifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BVNVerifications_SecurityAccounts");

            entity.HasOne(d => d.LoadAppRequestHeader).WithMany(p => p.Bvnverifications).HasConstraintName("FK_BVNVerifications_LoanApplicationRequestHeaders");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.ClientAccounts).HasConstraintName("FK_Clients_SecurityAccounts1");

            entity.HasOne(d => d.CreatedAccountBy).WithMany(p => p.ClientCreatedAccountBies).HasConstraintName("FK_Clients_SecurityAccounts");
        });

        modelBuilder.Entity<ClientNetPay>(entity =>
        {
            entity.HasOne(d => d.Client).WithMany(p => p.ClientNetPays).HasConstraintName("FK_ClientNetPays_Clients");

            entity.HasOne(d => d.ClientMonthlyNetPay).WithMany(p => p.ClientNetPays).HasConstraintName("FK_ClientNetPays_ClientMonthlyNetPays");

            entity.HasOne(d => d.CreatedAccountBy).WithMany(p => p.ClientNetPays).HasConstraintName("FK_ClientNetPays_SecurityAccounts");
        });

        modelBuilder.Entity<HubTeam>(entity =>
        {
            entity.HasOne(d => d.CreatedByAccount).WithMany(p => p.HubTeamCreatedByAccounts).HasConstraintName("FK_Hub_Teams_SecurityAccounts1");

            entity.HasOne(d => d.Group).WithMany(p => p.HubTeams).HasConstraintName("FK_Hub_Teams_HUB_TEAM_Groups");

            entity.HasOne(d => d.TeamAccount).WithMany(p => p.HubTeamTeamAccounts).HasConstraintName("FK_Hub_Teams_SecurityAccounts");
        });

        modelBuilder.Entity<HubTeamGroup>(entity =>
        {
            entity.HasOne(d => d.CreatedByAccount).WithMany(p => p.HubTeamGroups).HasConstraintName("FK_HUB_TEAM_Groups_SecurityAccounts");
        });

        modelBuilder.Entity<HubTeamManager>(entity =>
        {
            entity.HasOne(d => d.CreatedByAccount).WithMany(p => p.HubTeamManagerCreatedByAccounts).HasConstraintName("FK_HubTeamManagers_SecurityAccounts1");

            entity.HasOne(d => d.HubTeamSubGroup).WithMany(p => p.HubTeamManagers).HasConstraintName("FK_HubTeamManagers_HUB_TEAM_SubGroups");

            entity.HasOne(d => d.HubTeams).WithMany(p => p.HubTeamManagers).HasConstraintName("FK_HubTeamManagers_Hub_Teams");

            entity.HasOne(d => d.RemovedByAccount).WithMany(p => p.HubTeamManagerRemovedByAccounts).HasConstraintName("FK_HubTeamManagers_SecurityAccounts");
        });

        modelBuilder.Entity<HubTeamReconciliationOfficer>(entity =>
        {
            entity.HasOne(d => d.CreatedByAccount).WithMany(p => p.HubTeamReconciliationOfficerCreatedByAccounts).HasConstraintName("FK_HubTeamReconciliationOfficers_SecurityAccounts1");

            entity.HasOne(d => d.HubTeamSubGroup).WithMany(p => p.HubTeamReconciliationOfficers).HasConstraintName("FK_HubTeamReconciliationOfficers_HUB_TEAM_Groups");

            entity.HasOne(d => d.HubTeams).WithMany(p => p.HubTeamReconciliationOfficers).HasConstraintName("FK_HubTeamReconciliationOfficers_Hub_Teams");

            entity.HasOne(d => d.RemovedByAccount).WithMany(p => p.HubTeamReconciliationOfficerRemovedByAccounts).HasConstraintName("FK_HubTeamReconciliationOfficers_SecurityAccounts");
        });

        modelBuilder.Entity<HubTeamsDisbursmentOfficer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DisbursmentOfficers");

            entity.HasOne(d => d.CreatedByAccount).WithMany(p => p.HubTeamsDisbursmentOfficers).HasConstraintName("FK_HubTeamsDisbursmentOfficers_SecurityAccounts");

            entity.HasOne(d => d.HubGroup).WithMany(p => p.HubTeamsDisbursmentOfficers).HasConstraintName("FK_HubTeamsDisbursmentOfficers_HUB_TEAM_Groups");

            entity.HasOne(d => d.HubTeam).WithMany(p => p.HubTeamsDisbursmentOfficers).HasConstraintName("FK_HubTeamsDisbursmentOfficers_Hub_Teams");
        });

        modelBuilder.Entity<KycDetail>(entity =>
        {
            entity.HasOne(d => d.AccountRequest).WithMany(p => p.KycDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KycDetails_SecurityAccounts");

            entity.HasOne(d => d.LoanAppRequestHeader).WithMany(p => p.KycDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KycDetails_LoanApplicationRequestHeaders");
        });

        modelBuilder.Entity<LoanAppRequestTransation>(entity =>
        {
            entity.HasOne(d => d.AccountAppRequestHeader).WithMany(p => p.LoanAppRequestTransations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanAppRequestTransations_LoanApplicationRequestHeaders");

            entity.HasOne(d => d.AccountRequest).WithMany(p => p.LoanAppRequestTransationAccountRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanAppRequestTransations_SecurityAccounts");

            entity.HasOne(d => d.CreditedByAccount).WithMany(p => p.LoanAppRequestTransationCreditedByAccounts).HasConstraintName("FK_LoanAppRequestTransations_SecurityAccounts1");

            entity.HasOne(d => d.DebitedByAccount).WithMany(p => p.LoanAppRequestTransationDebitedByAccounts).HasConstraintName("FK_LoanAppRequestTransations_SecurityAccounts2");
        });

        modelBuilder.Entity<LoanApplicationRequestDetail>(entity =>
        {
            entity.HasOne(d => d.AccountRequest).WithMany(p => p.LoanApplicationRequestDetailAccountRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanApplicationRequestDetails_SecurityAccounts1");

            entity.HasOne(d => d.LoanAppRequestHeader).WithMany(p => p.LoanApplicationRequestDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanApplicationRequestDetails_LoanApplicationRequestHeaders");

            entity.HasOne(d => d.UpdatedByAccount).WithMany(p => p.LoanApplicationRequestDetailUpdatedByAccounts).HasConstraintName("FK_LoanApplicationRequestDetails_SecurityAccounts");
        });

        modelBuilder.Entity<LoanApplicationRequestHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_LoanApplicationRequests");

            entity.HasOne(d => d.Account).WithMany(p => p.LoanApplicationRequestHeaderAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanApplicationRequestHeaders_SecurityAccounts");

            entity.HasOne(d => d.ApprovedByAccount).WithMany(p => p.LoanApplicationRequestHeaderApprovedByAccounts).HasConstraintName("FK_LoanApplicationRequestHeaders_SecurityAccounts1");

            entity.HasOne(d => d.DisbursementBy).WithMany(p => p.LoanApplicationRequestHeaderDisbursementBies).HasConstraintName("FK_LoanApplicationRequestHeaders_SecurityAccounts2");

            entity.HasOne(d => d.ExportedBy).WithMany(p => p.LoanApplicationRequestHeaderExportedBies).HasConstraintName("FK_LoanApplicationRequestHeaders_SecurityAccounts3");

            entity.HasOne(d => d.LoanSetting).WithMany(p => p.LoanApplicationRequestHeaders).HasConstraintName("FK_LoanApplicationRequestHeaders_LoanSettings");

            entity.HasOne(d => d.LoanTenure).WithMany(p => p.LoanApplicationRequestHeaders).HasConstraintName("FK_LoanApplicationRequestHeaders_LoanTenureSettings");
        });

        modelBuilder.Entity<LoanApplicationRequestRepaymentDetail>(entity =>
        {
            entity.HasOne(d => d.LoanRequestHeader).WithMany(p => p.LoanApplicationRequestRepaymentDetails).HasConstraintName("FK_LoanApplicationRequestRepaymentDetails_LoanApplicationRequestHeaders");

            entity.HasOne(d => d.UploadedByMember).WithMany(p => p.LoanApplicationRequestRepaymentDetails).HasConstraintName("FK_LoanApplicationRequestRepaymentDetails_Hub_Teams");
        });

        modelBuilder.Entity<LoanReview>(entity =>
        {
            entity.HasOne(d => d.AccountRequest).WithMany(p => p.LoanReviewAccountRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanReviews_SecurityAccounts");

            entity.HasOne(d => d.ApprovedByAccount).WithMany(p => p.LoanReviewApprovedByAccounts).HasConstraintName("FK_LoanReviews_SecurityAccounts1");

            entity.HasOne(d => d.LoanAppRequestHeader).WithMany(p => p.LoanReviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanReviews_LoanApplicationRequestHeaders");

            entity.HasOne(d => d.ReviewByAccount).WithMany(p => p.LoanReviewReviewByAccounts).HasConstraintName("FK_LoanReviews_SecurityAccounts2");
        });

        modelBuilder.Entity<LoanTenureSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Tenures");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.LoanTenureSettings).HasConstraintName("FK_Tenures_SecurityAccounts");
        });

        modelBuilder.Entity<Narration>(entity =>
        {
            entity.HasOne(d => d.CreatedBy).WithMany(p => p.Narrations).HasConstraintName("FK_Narrations_SecurityAccounts");
        });

        modelBuilder.Entity<PasswordChanging>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.PasswordChangingAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordChangings_SecurityAccounts");

            entity.HasOne(d => d.ChangedByAccount).WithMany(p => p.PasswordChangingChangedByAccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordChangings_SecurityAccounts1");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.People)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_People_SecurityAccounts");
        });

        modelBuilder.Entity<RepaymentLoan>(entity =>
        {
            entity.HasOne(d => d.CreatedAccountBy).WithMany(p => p.RepaymentLoans).HasConstraintName("FK_RepaymentLoans_SecurityAccounts");

            entity.HasOne(d => d.LoanHeader).WithMany(p => p.RepaymentLoans).HasConstraintName("FK_RepaymentLoans_LoanApplicationRequestHeaders");
        });

        modelBuilder.Entity<SecurityAccount>(entity =>
        {
            entity.HasOne(d => d.Person).WithMany(p => p.SecurityAccounts).HasConstraintName("FK_SecurityAccounts_People");
        });

        modelBuilder.Entity<SecurityPermission>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.SecurityPermissions).HasConstraintName("FK_SecurityPermissions_SecurityAccounts");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
