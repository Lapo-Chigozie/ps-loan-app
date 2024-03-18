namespace LapoLoanWebApi.HubTeams.HubTeamModel
{
    public class NewHubTeamMember
    {
        public string EnterTeamMemberID { get; set; }
        public string EnterLastName { get; set; }
        public string EnterMiddleName { get; set; }
        public string EnterFirstName { get; set; }
        public string SelectHubGroupId { get; set; }

        public string EnterPhoneNumber { get; set; }
        public string EnterEmailAddress { get; set; }

        public long CreatedByAccountId { get; set; }

        public string TeamMemberOfficeAddress { get; set; }

        public string UserType { get; set; }

        public string SelectHubGroupIdd { get; set; }
        public bool AccessRightToEditTeamMemberPermissions { get; set; }

        public bool AccessRightToViewDisbursementLoan { get; set; }
        public bool AccessRightToViewUploadBackRepaymentLoan
        { get; set; }
        public bool AccessRightToExportDISBURSEMENTLoan
        { get; set; }
        public bool AccessRightToAnonymousLoanApplication
        { get; set; }
        public bool AccessRightToUploadBackDISBURSEMENTLoan
        { get; set; }
        public bool AccessRightToUploadBackRepaymentLoan
        { get; set; }
        public bool AccessRightToPrintLoan
        { get; set; }
        public bool AccessRightToProceedLoan
        { get; set; }
        public bool ViewLoanNarration
        { get; set; }
        public bool CreateLoanNarration
        { get; set; }
        public bool AccessRighttodisablecustomerstoapplyforaloan
        { get; set; }
        public bool AccessRighttoviewcustomers
        { get; set; }
        public bool AccessRighttodisablehubs
        { get; set; }
        public bool AccessRighttoviewtenure
        { get; set; }
        public bool AccessRighttocreatetenure
        { get; set; }
        public bool AccessRighttoloansettings
        { get; set; }
        public bool AccessRighttoteamsAndpermissions
        { get; set; }
        public bool AccessRighttorejectaloan
        { get; set; }
        public bool AccessRighttoviewcustomersloans
        { get; set; }
        public bool AccessRighttoapprovecustomerloan
        { get; set; }
        public bool AccessRighttoviewveammembers
        { get; set; }
        public bool AccessRighttocreateateammember
        { get; set; }
        public bool AccessRighttoviewhubs
        { get; set; }
        public bool AccessRighttocreateahub
        { get; set; }
        public bool AccessRighttoviewloandetails
        { get; set; }
    }


    public class EditHubTeamMember
    {
        public string SelectHubGroupId { get; set; }

        public string SelectHubGroupIdd { get; set; }
        public string UserType { get; set; }

     
        public bool AccessRightToEditTeamMemberPermissions { get; set; }

        public bool AccessRightToViewDisbursementLoan { get; set; }
        public bool AccessRightToViewUploadBackRepaymentLoan
        { get; set; } =false;
        public bool AccessRightToExportDISBURSEMENTLoan
        { get; set; } = false;
        public bool AccessRightToAnonymousLoanApplication
        { get; set; } = false;
        public bool AccessRightToUploadBackDISBURSEMENTLoan
        { get; set; } = false;
        public bool AccessRightToUploadBackRepaymentLoan
        { get; set; } = false;
        public bool AccessRightToPrintLoan
        { get; set; } = false;
        public bool AccessRightToProceedLoan
        { get; set; } = false;
        public bool ViewLoanNarration
        { get; set; } = false;
        public bool CreateLoanNarration
        { get; set; } = false;
        public bool AccessRighttodisablecustomerstoapplyforaloan
        { get; set; } = false;
        public bool AccessRighttoviewcustomers
        { get; set; } = false;
        public bool AccessRighttodisablehubs
        { get; set; } = false;
        public bool AccessRighttoviewtenure
        { get; set; } = false;
        public bool AccessRighttocreatetenure
        { get; set; } = false;
        public bool AccessRighttoloansettings
        { get; set; } = false;
        public bool AccessRighttoteamsAndpermissions
        { get; set; } = false;
        public bool AccessRighttorejectaloan
        { get; set; } = false;
        public bool AccessRighttoviewcustomersloans
        { get; set; } = false;
        public bool AccessRighttoapprovecustomerloan
        { get; set; } = false;
        public bool AccessRighttoviewveammembers
        { get; set; } = false;
        public bool AccessRighttocreateateammember
        { get; set; } = false;
        public bool AccessRighttoviewhubs
        { get; set; } = false;
        public bool AccessRighttocreateahub
        { get; set; } = false;
        public bool AccessRighttoviewloandetails
        { get; set; } = false;
    }
    
}
