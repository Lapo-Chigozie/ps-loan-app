namespace LapoLoanWebApi.E360Helpers.E360DtoModel
{
    public class E360StaffModel
    {
        public string Staff_Name { get; set; }
        public string CreatedDate { get; set; }
        public bool IsBlocked { get; set; }
        public string Staff_Id { get; set; }

        public string Id { get; set; }

        public string Status { get; set; }

        public string Staff_Role { get; set; }

        public bool AccesstoCreatedStaff { get; set; }

        public bool AccesstoActivateStaff { get; set; }

        public string StaffLevel { get; set; }
    }
}
