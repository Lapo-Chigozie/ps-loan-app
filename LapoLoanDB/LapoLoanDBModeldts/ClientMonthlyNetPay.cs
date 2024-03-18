using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LapoLoanWebApi.LapoLoanDB.LapoLoanDBModeldts;

public partial class ClientMonthlyNetPay
{
    [Key]
    public long Id { get; set; }

    public long? Month { get; set; }

    [Unicode(false)]
    public string? MonthName { get; set; }

    public long? Year { get; set; }

    public long? CreatedDate { get; set; }

    [InverseProperty("ClientMonthlyNetPay")]
    public virtual ICollection<ClientNetPay> ClientNetPays { get; set; } = new List<ClientNetPay>();
}
