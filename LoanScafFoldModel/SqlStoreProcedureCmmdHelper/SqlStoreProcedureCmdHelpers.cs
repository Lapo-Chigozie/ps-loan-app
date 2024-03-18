namespace LapoLoanWebApi.LoanScafFoldModel.SqlStoreProcedureCmmdHelper
{
    public class SqlStoreProcedureCmdHelpers
    {
        public const string ValidateIfHasCreatedAcct = "[dbo].[ValidateIfHasRegisterAcct]";
        public const string CreateAdminAccount = "[dbo].[CreateAdmin_Bound]";
        public const string GetAccount = "[dbo].[AccountSignInAuth]";

        public const string RegisterTwoFactorAuthRegister = "[dbo].[TwoFactorAuthRegister]";

        public const string VerifyTwoFactorAuthExit = "[dbo].[VerifyTwoFactorAuthExit]";

        public const string ValidateOTPUser    = "[dbo].[ValidateOTPUser]";

        public const string ValidateIdentityExit = "[dbo].[ValidateIdentityExit]";

        public const string ChangePassword = "[dbo].[ChangePassword]";

        public const string ValidateClientNetPaysByDate = "[dbo].[ValidateClientNetPaysByDate]";

        public const string View_Procedure_HubTeams = "[dbo].[View_Procedure_HubTeams]";

        public const string View_Procedure_Hub_Team_Groups = "[dbo].[View_Procedure_Hub_Team_Groups]";
    }
}
