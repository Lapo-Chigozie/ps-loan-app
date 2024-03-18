namespace LapoLoanWebApi.HubTeams.HubTeamModel
{
    public class HubTeamGroupModel
    {
        public string HubTeamGroupName { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public string Id { get; set; }

        public string No { get; set;  }

        public string TeamLead
        {
            get; set; }

        
    }

    public class HubTeams
    {
        public string HubTeamGroupName { get; set; }
        public string HubTeamGroupId { get; set; }

        public string TeamId { get; set; }
        public string HubTeamName { get; set; }
    }

    public class SubHubTeamGroupModel
    {
        public string SubHubTeamGroupName { get; set; }
        public string HubTeamGroupName { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public string GroupId { get; set; }

        public string Id { get; set; }

        public string No { get; set; }
    }

    public class HubTeamMemberModel
    {
        public string Name { get; set; }

        public string GroupName { get; set; }

        public string Role { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedByName { get; set; }


        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }

        public bool HideEditButton { get; set; }
        public string Id { get; set; }
        public string No { get; set; }
    }

    public class HubTeamMemberRolesModel
    {
        public string Name { get; set; }

        public string CreatedByName { get; set; }
        public string Role { get; set; }

        public string IsTeamLead { get; set; }

        public string Hub { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }

        public string CreatedBy { get; set; }

        public string IsReconciliationOfficers { get; set; }

        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public  string Id { get; set; }
        public string No { get; set; }
    }
}
