using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class RepaymentLoan
{
    [Key]
    public long Id { get; set; }

    public long? CreatedAccountById { get; set; }

    public long? LoanHeaderId { get; set; }

    [Column("Loan_Request_Code")]
    [Unicode(false)]
    public string? LoanRequestCode { get; set; }

    [Column("Uploaded_By_Member_Staff_ID")]
    [Unicode(false)]
    public string? UploadedByMemberStaffId { get; set; }

    [Column("Repayment_Status")]
    [Unicode(false)]
    public string? RepaymentStatus { get; set; }

    [Column("Loan_Repayment_For")]
    [Unicode(false)]
    public string? LoanRepaymentFor { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("Repayment_Date", TypeName = "datetime")]
    public DateTime? RepaymentDate { get; set; }

    [Column("Repayment_Amount", TypeName = "money")]
    public decimal? RepaymentAmount { get; set; }

    [Column("Repayment_Balance", TypeName = "money")]
    public decimal? RepaymentBalance { get; set; }

    [Column("Service_Account")]
    [Unicode(false)]
    public string? ServiceAccount { get; set; }

    [Column("Customer_Name")]
    [Unicode(false)]
    public string? CustomerName { get; set; }

    [ForeignKey("CreatedAccountById")]
    [InverseProperty("RepaymentLoans")]
    public virtual SecurityAccount? CreatedAccountBy { get; set; }

    [ForeignKey("LoanHeaderId")]
    [InverseProperty("RepaymentLoans")]
    public virtual LoanApplicationRequestHeader? LoanHeader { get; set; }
}
