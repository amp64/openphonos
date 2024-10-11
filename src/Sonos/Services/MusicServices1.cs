
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class MusicServices1 : OpenPhonos.UPnP.Service
    {
        public MusicServices1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetSessionId_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetSessionId",
            argnames = new string[] { "ServiceId", "Username" },
            outargs = 1,
        };

        public class GetSessionId_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string SessionId;


            public override void Fill(string[] rawdata)
            {
				SessionId = rawdata[0];

            }
        }
        public async Task<GetSessionId_Result> GetSessionId(uint ServiceId, string Username)
        {
            return await base.Action_Async(GetSessionId_Info, new object[] { ServiceId, Username }, new GetSessionId_Result()) as GetSessionId_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ListAvailableServices_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ListAvailableServices",
            argnames = new string[] {  },
            outargs = 3,
        };

        public class ListAvailableServices_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string AvailableServiceDescriptorList;
			public string AvailableServiceTypeList;
			public string AvailableServiceListVersion;


            public override void Fill(string[] rawdata)
            {
				AvailableServiceDescriptorList = rawdata[0];
				AvailableServiceTypeList = rawdata[1];
				AvailableServiceListVersion = rawdata[2];

            }
        }
        public async Task<ListAvailableServices_Result> ListAvailableServices()
        {
            return await base.Action_Async(ListAvailableServices_Info, new object[] {  }, new ListAvailableServices_Result()) as ListAvailableServices_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo UpdateAvailableServices_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "UpdateAvailableServices",
            argnames = new string[] {  },
            outargs = 0,
        };

        public class UpdateAvailableServices_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<UpdateAvailableServices_Result> UpdateAvailableServices()
        {
            return await base.Action_Async(UpdateAvailableServices_Info, new object[] {  }, new UpdateAvailableServices_Result()) as UpdateAvailableServices_Result;
        }
    

    }
}

