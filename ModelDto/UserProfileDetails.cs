using NETCore.Encrypt.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.ModelDto
{
    public class UserProfileDetails
    {
        public string Id { get;set;}
        public string AccountId    { get;set;}
        public string FirstName  { get;set;}
        public string  LastName { get; set;}
        public string  Gender { get; set;}
        public string Age  { get; set;}
        public string Staff   { get; set;}
        public string  PhoneNumber { get; set;}
        public string EmailAddress { get; set;}
        public string CurrentAddress { get; set;}
        public string  AltPhoneNumber { get; set;}
        public string PositionOrRole { get; set;}
        public string CreatedDate { get; set;}
        public string MiddleName { get; set; }

        public string RoleType { get; set; }
        public string HubGroup { get; set; }


        public string BankVerificationNumber { get; set; }

        public string BVN { get; set; }

        public string MarrintalStatus { get; set; }
        public string Occuptaion { get; set; }

        public string EmploymentDate { get; set; }
        public string RetirementDate { get; set; }
    }

    public class AccountDetails
    {
            public UserProfileDetails userProfileDetails { get; set; }
            public string Id  { get; set;}
            public string PersonId { get; set; }
            public string Password { get; set; }
            public string Username { get; set; }
            public string Status { get; set; }
            public string LastLoginDate   { get; set;}
            public string CreatedDate { get; set; }
            public string AccountType { get; set; }
            public bool AllowLoginTwoFactor { get; set; }
            public bool AccountVerify { get; set; }
    }

    public class AccountPermissionDetails
    {
        public bool IsRELATIONSHIPOFFICER { get; set; }
        public bool IsTEAMLEADS { get; set; }
        public bool IsRECONCILIATIONANDACCOUNTOFFICER { get; set; }
        public bool IsASSISTANTHEADOFOPERATION { get; set; }
        public bool IsHEADOFOPERATIONS { get; set; }
        public bool IsGROUPHEAD { get; set; }
        public bool IsDISBURSEMENTOFFICER { get; set; }

        public bool IsDeveloperTeam { get; set; }
        public bool AccessRightToAnonymousLoanApplication { get; set; }

        public bool AccessRightToApprovedLoan { get; set; }
        public bool AccessRightToCancelLoan { get; set; }


       


        public bool AccessRightToUploadBackDISBURSEMENTLoan { get; set; }
        public bool AccessRightToViewLoan { get; set; }

        public bool AccessRightToPrintLoan { get; set; }
        public bool AccessRightToProceedLoan { get; set; }
        public bool AccessRightToUploadBackRepaymentLoan { get; set; }
        public bool AccessRightToViewUploadBackRepaymentLoan { get; set; }

        public bool AccessRightToViewDisbursementLoan { get; set; }

        public bool IsGeneralPermissionsAccessRight { get; set; }
        public bool IsTenureAccessRight { get; set; }
        public bool IsLoanSettingAccessRight { get; set; }
        public bool IsNetPaysAccessRight { get; set; }

        public bool IsCustomerLoanPermission { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public long TeamId { get; set; }




        


        public bool AccessRighttoapprovecustomerloan { get; set; }
        public bool AccessRighttocreateahub { get; set; }
        public bool AccessRighttocreateateammember { get; set; }
        public bool AccessRighttocreatetenure { get; set; }
        public bool AccessRighttodisablecustomerstoapplyforaloan { get; set; }
        public bool AccessRighttodisablehubs { get; set; }
        public bool AccessRightToEditTeamMemberPermissions { get; set; }


        public bool AccessRightToExportDisbursementloan1 { get; set; }
        public bool AccessRightToExportDISBURSEMENTLoan { get; set; }



        public bool AccessRighttoloansettings { get; set; }
        public bool AccessRighttorejectaloan { get; set; }
        public bool AccessRighttoteamsAndpermissions { get; set; }

        //public bool AccessRightToUploadBackDisbursementloan { get; set; }

        public bool AccessRighttoviewveammembers { get; set; }
        public bool AccessRighttoviewcustomersloans { get; set; }
        public bool AccessRighttoviewcustomers { get; set; }
        public bool AccessRighttoviewhubs { get; set; }
        public bool AccessRighttoviewloandetails { get; set; }
        public bool AccessRighttoviewtenure { get; set; }
        public bool CreateLoanNarration { get; set; }
        public bool ViewLoanNarration { get; set; }
    }
}
