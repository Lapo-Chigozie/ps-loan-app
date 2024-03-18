using NETCore.Encrypt.Shared;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace LapoLoanWebApi.ModelDto
{
    public class ClientNetPayDto
    {
        public bool IsActive { get; set; }
        public int No { get; set; }
        public string AccountId { get; set; }
        public string FullName { get; set; }
        public string PFNumber { get; set; }
        public string  Status { get; set; }
        public string CreatedDate { get; set; }

    }

    public class ClientNetPayDetailsDto
    {
        public string PFNumber { get; set; }
        public string No { get; set; }
        public string Id { get; set; }
      
        public string ClientId { get; set; }
        public string  CreatedAccountById { get; set; }
        public string  NetPay { get; set; }
        public double NetPayMoney { get; set; }
        public string  Status { get; set; }
        public string Grade { get; set; }
        public string GradeStep { get; set; }
        public string Command { get; set; }
        public string PayGroup { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }
        public string CreatedDate { get; set; }
        public string NPFDate { get; set; }


    }

    public class ClientCustomers
    {
        public string No { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string AltNativeNumber { get; set; }

        public string id { get; set; }
        

        public string EmailAddress { get; set; }

        public string Gender { get; set; }
        public string Status { get; set; }

        public string  CreatedDate { get; set; }
    }
}
