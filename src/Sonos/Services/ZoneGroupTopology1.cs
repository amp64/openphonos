
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class ZoneGroupTopology1 : OpenPhonos.UPnP.Service
    {
        public ZoneGroupTopology1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo CheckForUpdate_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "CheckForUpdate",
            argnames = new string[] { "UpdateType", "CachedOnly", "Version" },
            outargs = 1,
        };

        public class CheckForUpdate_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string UpdateItem;


            public override void Fill(string[] rawdata)
            {
				UpdateItem = rawdata[0];

            }
        }
        public async Task<CheckForUpdate_Result> CheckForUpdate(string UpdateType, bool CachedOnly, string Version)
        {
            return await base.Action_Async(CheckForUpdate_Info, new object[] { UpdateType, CachedOnly, Version }, new CheckForUpdate_Result()) as CheckForUpdate_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo BeginSoftwareUpdate_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "BeginSoftwareUpdate",
            argnames = new string[] { "UpdateURL", "Flags", "ExtraOptions" },
            outargs = 0,
        };

        public class BeginSoftwareUpdate_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<BeginSoftwareUpdate_Result> BeginSoftwareUpdate(string UpdateURL, uint Flags, string ExtraOptions)
        {
            return await base.Action_Async(BeginSoftwareUpdate_Info, new object[] { UpdateURL, Flags, ExtraOptions }, new BeginSoftwareUpdate_Result()) as BeginSoftwareUpdate_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReportUnresponsiveDevice_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReportUnresponsiveDevice",
            argnames = new string[] { "DeviceUUID", "DesiredAction" },
            outargs = 0,
        };

        public class ReportUnresponsiveDevice_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ReportUnresponsiveDevice_Result> ReportUnresponsiveDevice(string DeviceUUID, string DesiredAction)
        {
            return await base.Action_Async(ReportUnresponsiveDevice_Info, new object[] { DeviceUUID, DesiredAction }, new ReportUnresponsiveDevice_Result()) as ReportUnresponsiveDevice_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReportAlarmStartedRunning_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReportAlarmStartedRunning",
            argnames = new string[] {  },
            outargs = 0,
        };

        public class ReportAlarmStartedRunning_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ReportAlarmStartedRunning_Result> ReportAlarmStartedRunning()
        {
            return await base.Action_Async(ReportAlarmStartedRunning_Info, new object[] {  }, new ReportAlarmStartedRunning_Result()) as ReportAlarmStartedRunning_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SubmitDiagnostics_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SubmitDiagnostics",
            argnames = new string[] { "IncludeControllers", "Type" },
            outargs = 1,
        };

        public class SubmitDiagnostics_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint DiagnosticID;


            public override void Fill(string[] rawdata)
            {
				DiagnosticID = uint.Parse(rawdata[0]);

            }
        }
        public async Task<SubmitDiagnostics_Result> SubmitDiagnostics(bool IncludeControllers, string Type)
        {
            return await base.Action_Async(SubmitDiagnostics_Info, new object[] { IncludeControllers, Type }, new SubmitDiagnostics_Result()) as SubmitDiagnostics_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RegisterMobileDevice_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RegisterMobileDevice",
            argnames = new string[] { "MobileDeviceName", "MobileDeviceUDN", "MobileIPAndPort" },
            outargs = 0,
        };

        public class RegisterMobileDevice_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RegisterMobileDevice_Result> RegisterMobileDevice(string MobileDeviceName, string MobileDeviceUDN, string MobileIPAndPort)
        {
            return await base.Action_Async(RegisterMobileDevice_Info, new object[] { MobileDeviceName, MobileDeviceUDN, MobileIPAndPort }, new RegisterMobileDevice_Result()) as RegisterMobileDevice_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetZoneGroupAttributes_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetZoneGroupAttributes",
            argnames = new string[] {  },
            outargs = 3,
        };

        public class GetZoneGroupAttributes_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentZoneGroupName;
			public string CurrentZoneGroupID;
			public string CurrentZonePlayerUUIDsInGroup;
			public string CurrentMuseHouseholdId;


            public override void Fill(string[] rawdata)
            {
				CurrentZoneGroupName = rawdata[0];
				CurrentZoneGroupID = rawdata[1];
				CurrentZonePlayerUUIDsInGroup = rawdata[2];
                if (rawdata.Length > 3)
				    CurrentMuseHouseholdId = rawdata[3];

            }
        }
        public async Task<GetZoneGroupAttributes_Result> GetZoneGroupAttributes()
        {
            return await base.Action_Async(GetZoneGroupAttributes_Info, new object[] {  }, new GetZoneGroupAttributes_Result()) as GetZoneGroupAttributes_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetZoneGroupState_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetZoneGroupState",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetZoneGroupState_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string ZoneGroupState;


            public override void Fill(string[] rawdata)
            {
				ZoneGroupState = rawdata[0];

            }
        }
        public async Task<GetZoneGroupState_Result> GetZoneGroupState()
        {
            return await base.Action_Async(GetZoneGroupState_Info, new object[] {  }, new GetZoneGroupState_Result()) as GetZoneGroupState_Result;
        }
    

    }
}

