
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class SystemProperties1 : OpenPhonos.UPnP.Service
    {
        public SystemProperties1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetString_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetString",
            argnames = new string[] { "VariableName", "StringValue" },
            outargs = 0,
        };

        public class SetString_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetString_Result> SetString(string VariableName, string StringValue)
        {
            return await base.Action_Async(SetString_Info, new object[] { VariableName, StringValue }, new SetString_Result()) as SetString_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetString_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetString",
            argnames = new string[] { "VariableName" },
            outargs = 1,
        };

        public class GetString_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string StringValue;


            public override void Fill(string[] rawdata)
            {
				StringValue = rawdata[0];

            }
        }
        public async Task<GetString_Result> GetString(string VariableName)
        {
            return await base.Action_Async(GetString_Info, new object[] { VariableName }, new GetString_Result()) as GetString_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Remove_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Remove",
            argnames = new string[] { "VariableName" },
            outargs = 0,
        };

        public class Remove_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Remove_Result> Remove(string VariableName)
        {
            return await base.Action_Async(Remove_Info, new object[] { VariableName }, new Remove_Result()) as Remove_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetWebCode_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetWebCode",
            argnames = new string[] { "AccountType" },
            outargs = 1,
        };

        public class GetWebCode_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string WebCode;


            public override void Fill(string[] rawdata)
            {
				WebCode = rawdata[0];

            }
        }
        public async Task<GetWebCode_Result> GetWebCode(uint AccountType)
        {
            return await base.Action_Async(GetWebCode_Info, new object[] { AccountType }, new GetWebCode_Result()) as GetWebCode_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ProvisionCredentialedTrialAccountX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ProvisionCredentialedTrialAccountX",
            argnames = new string[] { "AccountType", "AccountID", "AccountPassword" },
            outargs = 2,
        };

        public class ProvisionCredentialedTrialAccountX_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool IsExpired;
			public string AccountUDN;


            public override void Fill(string[] rawdata)
            {
				IsExpired = ParseBool(rawdata[0]);
				AccountUDN = rawdata[1];

            }
        }
        public async Task<ProvisionCredentialedTrialAccountX_Result> ProvisionCredentialedTrialAccountX(uint AccountType, string AccountID, string AccountPassword)
        {
            return await base.Action_Async(ProvisionCredentialedTrialAccountX_Info, new object[] { AccountType, AccountID, AccountPassword }, new ProvisionCredentialedTrialAccountX_Result()) as ProvisionCredentialedTrialAccountX_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo AddAccountX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddAccountX",
            argnames = new string[] { "AccountType", "AccountID", "AccountPassword" },
            outargs = 1,
        };

        public class AddAccountX_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string AccountUDN;


            public override void Fill(string[] rawdata)
            {
				AccountUDN = rawdata[0];

            }
        }
        public async Task<AddAccountX_Result> AddAccountX(uint AccountType, string AccountID, string AccountPassword)
        {
            return await base.Action_Async(AddAccountX_Info, new object[] { AccountType, AccountID, AccountPassword }, new AddAccountX_Result()) as AddAccountX_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo AddOAuthAccountX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddOAuthAccountX",
            argnames = new string[] { "AccountType", "AccountToken", "AccountKey", "OAuthDeviceID", "AuthorizationCode", "RedirectURI", "UserIdHashCode", "AccountTier" },
            outargs = 2,
        };

        public class AddOAuthAccountX_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string AccountUDN;
			public string AccountNickname;


            public override void Fill(string[] rawdata)
            {
				AccountUDN = rawdata[0];
				AccountNickname = rawdata[1];

            }
        }
        public async Task<AddOAuthAccountX_Result> AddOAuthAccountX(uint AccountType, string AccountToken, string AccountKey, string OAuthDeviceID, string AuthorizationCode, string RedirectURI, string UserIdHashCode, uint AccountTier)
        {
            return await base.Action_Async(AddOAuthAccountX_Info, new object[] { AccountType, AccountToken, AccountKey, OAuthDeviceID, AuthorizationCode, RedirectURI, UserIdHashCode, AccountTier }, new AddOAuthAccountX_Result()) as AddOAuthAccountX_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RemoveAccount_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveAccount",
            argnames = new string[] { "AccountType", "AccountID" },
            outargs = 0,
        };

        public class RemoveAccount_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RemoveAccount_Result> RemoveAccount(uint AccountType, string AccountID)
        {
            return await base.Action_Async(RemoveAccount_Info, new object[] { AccountType, AccountID }, new RemoveAccount_Result()) as RemoveAccount_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo EditAccountPasswordX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "EditAccountPasswordX",
            argnames = new string[] { "AccountType", "AccountID", "NewAccountPassword" },
            outargs = 0,
        };

        public class EditAccountPasswordX_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<EditAccountPasswordX_Result> EditAccountPasswordX(uint AccountType, string AccountID, string NewAccountPassword)
        {
            return await base.Action_Async(EditAccountPasswordX_Info, new object[] { AccountType, AccountID, NewAccountPassword }, new EditAccountPasswordX_Result()) as EditAccountPasswordX_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetAccountNicknameX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetAccountNicknameX",
            argnames = new string[] { "AccountUDN", "AccountNickname" },
            outargs = 0,
        };

        public class SetAccountNicknameX_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetAccountNicknameX_Result> SetAccountNicknameX(string AccountUDN, string AccountNickname)
        {
            return await base.Action_Async(SetAccountNicknameX_Info, new object[] { AccountUDN, AccountNickname }, new SetAccountNicknameX_Result()) as SetAccountNicknameX_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RefreshAccountCredentialsX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RefreshAccountCredentialsX",
            argnames = new string[] { "AccountType", "AccountUID", "AccountToken", "AccountKey" },
            outargs = 0,
        };

        public class RefreshAccountCredentialsX_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RefreshAccountCredentialsX_Result> RefreshAccountCredentialsX(uint AccountType, uint AccountUID, string AccountToken, string AccountKey)
        {
            return await base.Action_Async(RefreshAccountCredentialsX_Info, new object[] { AccountType, AccountUID, AccountToken, AccountKey }, new RefreshAccountCredentialsX_Result()) as RefreshAccountCredentialsX_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo EditAccountMd_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "EditAccountMd",
            argnames = new string[] { "AccountType", "AccountID", "NewAccountMd" },
            outargs = 0,
        };

        public class EditAccountMd_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<EditAccountMd_Result> EditAccountMd(uint AccountType, string AccountID, string NewAccountMd)
        {
            return await base.Action_Async(EditAccountMd_Info, new object[] { AccountType, AccountID, NewAccountMd }, new EditAccountMd_Result()) as EditAccountMd_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo DoPostUpdateTasks_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "DoPostUpdateTasks",
            argnames = new string[] {  },
            outargs = 0,
        };

        public class DoPostUpdateTasks_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<DoPostUpdateTasks_Result> DoPostUpdateTasks()
        {
            return await base.Action_Async(DoPostUpdateTasks_Info, new object[] {  }, new DoPostUpdateTasks_Result()) as DoPostUpdateTasks_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ResetThirdPartyCredentials_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ResetThirdPartyCredentials",
            argnames = new string[] {  },
            outargs = 0,
        };

        public class ResetThirdPartyCredentials_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ResetThirdPartyCredentials_Result> ResetThirdPartyCredentials()
        {
            return await base.Action_Async(ResetThirdPartyCredentials_Info, new object[] {  }, new ResetThirdPartyCredentials_Result()) as ResetThirdPartyCredentials_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo EnableRDM_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "EnableRDM",
            argnames = new string[] { "RDMValue" },
            outargs = 0,
        };

        public class EnableRDM_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<EnableRDM_Result> EnableRDM(bool RDMValue)
        {
            return await base.Action_Async(EnableRDM_Info, new object[] { RDMValue }, new EnableRDM_Result()) as EnableRDM_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetRDM_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetRDM",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetRDM_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool RDMValue;


            public override void Fill(string[] rawdata)
            {
				RDMValue = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetRDM_Result> GetRDM()
        {
            return await base.Action_Async(GetRDM_Info, new object[] {  }, new GetRDM_Result()) as GetRDM_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReplaceAccountX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReplaceAccountX",
            argnames = new string[] { "AccountUDN", "NewAccountID", "NewAccountPassword", "AccountToken", "AccountKey", "OAuthDeviceID" },
            outargs = 1,
        };

        public class ReplaceAccountX_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string NewAccountUDN;


            public override void Fill(string[] rawdata)
            {
				NewAccountUDN = rawdata[0];

            }
        }
        public async Task<ReplaceAccountX_Result> ReplaceAccountX(string AccountUDN, string NewAccountID, string NewAccountPassword, string AccountToken, string AccountKey, string OAuthDeviceID)
        {
            return await base.Action_Async(ReplaceAccountX_Info, new object[] { AccountUDN, NewAccountID, NewAccountPassword, AccountToken, AccountKey, OAuthDeviceID }, new ReplaceAccountX_Result()) as ReplaceAccountX_Result;
        }
    

    }
}

