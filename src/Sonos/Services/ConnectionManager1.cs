
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class ConnectionManager1 : OpenPhonos.UPnP.Service
    {
        public ConnectionManager1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetProtocolInfo_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetProtocolInfo",
            argnames = new string[] {  },
            outargs = 2,
        };

        public class GetProtocolInfo_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string Source;
			public string Sink;


            public override void Fill(string[] rawdata)
            {
				Source = rawdata[0];
				Sink = rawdata[1];

            }
        }
        public async Task<GetProtocolInfo_Result> GetProtocolInfo()
        {
            return await base.Action_Async(GetProtocolInfo_Info, new object[] {  }, new GetProtocolInfo_Result()) as GetProtocolInfo_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetCurrentConnectionIDs_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetCurrentConnectionIDs",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetCurrentConnectionIDs_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string ConnectionIDs;


            public override void Fill(string[] rawdata)
            {
				ConnectionIDs = rawdata[0];

            }
        }
        public async Task<GetCurrentConnectionIDs_Result> GetCurrentConnectionIDs()
        {
            return await base.Action_Async(GetCurrentConnectionIDs_Info, new object[] {  }, new GetCurrentConnectionIDs_Result()) as GetCurrentConnectionIDs_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetCurrentConnectionInfo_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetCurrentConnectionInfo",
            argnames = new string[] { "ConnectionID" },
            outargs = 7,
        };

        public class GetCurrentConnectionInfo_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public int RcsID;
			public int AVTransportID;
			public string ProtocolInfo;
			public string PeerConnectionManager;
			public int PeerConnectionID;
			public string Direction;
			public string Status;


            public override void Fill(string[] rawdata)
            {
				RcsID = int.Parse(rawdata[0]);
				AVTransportID = int.Parse(rawdata[1]);
				ProtocolInfo = rawdata[2];
				PeerConnectionManager = rawdata[3];
				PeerConnectionID = int.Parse(rawdata[4]);
				Direction = rawdata[5];
				Status = rawdata[6];

            }
        }
        public async Task<GetCurrentConnectionInfo_Result> GetCurrentConnectionInfo(int ConnectionID)
        {
            return await base.Action_Async(GetCurrentConnectionInfo_Info, new object[] { ConnectionID }, new GetCurrentConnectionInfo_Result()) as GetCurrentConnectionInfo_Result;
        }
    

    }
}

