namespace LapoLoanWebApi.E360Helpers.E360DtoModel
{
    public class E360RegisterModelDto
    {
        public string Staff_ID { get; set; }
        public bool IsActive { get; set; }
        public int CreatingStaff_ID { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsAccessRightCreatePermission { get; set; }
        public bool IsAccessRightActivatorPermission { get; set; }

        public bool IsStaffsLoanPermissionAccessRight { get; set; }

        public bool IsStaffsLoanTenurePermissionAccessRight { get; set; }
        public bool IsStaffsLoanSettingsPermissionAccessRight { get; set; }
        public bool IsStaffsNetPaysPermissionAccessRight
        { get; set; }
        public bool IsStaffsCompleteLoanRepaymentPermissionAccessRight
        { get; set; }
        public bool IsStaffsBlockCustomerApplyLoanPermissionAccessRight
        { get; set; }

    }
}
