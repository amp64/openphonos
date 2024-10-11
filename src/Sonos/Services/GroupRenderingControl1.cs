
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class GroupRenderingControl1 : OpenPhonos.UPnP.Service
    {
        public GroupRenderingControl1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetGroupMute_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetGroupMute",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetGroupMute_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool CurrentMute;


            public override void Fill(string[] rawdata)
            {
				CurrentMute = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetGroupMute_Result> GetGroupMute(uint InstanceID)
        {
            return await base.Action_Async(GetGroupMute_Info, new object[] { InstanceID }, new GetGroupMute_Result()) as GetGroupMute_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetGroupMute_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetGroupMute",
            argnames = new string[] { "InstanceID", "DesiredMute" },
            outargs = 0,
        };

        public class SetGroupMute_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetGroupMute_Result> SetGroupMute(uint InstanceID, bool DesiredMute)
        {
            return await base.Action_Async(SetGroupMute_Info, new object[] { InstanceID, DesiredMute }, new SetGroupMute_Result()) as SetGroupMute_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetGroupVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetGroupVolume",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetGroupVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public ushort CurrentVolume;


            public override void Fill(string[] rawdata)
            {
				CurrentVolume = ushort.Parse(rawdata[0]);

            }
        }
        public async Task<GetGroupVolume_Result> GetGroupVolume(uint InstanceID)
        {
            return await base.Action_Async(GetGroupVolume_Info, new object[] { InstanceID }, new GetGroupVolume_Result()) as GetGroupVolume_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetGroupVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetGroupVolume",
            argnames = new string[] { "InstanceID", "DesiredVolume" },
            outargs = 0,
        };

        public class SetGroupVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetGroupVolume_Result> SetGroupVolume(uint InstanceID, ushort DesiredVolume)
        {
            return await base.Action_Async(SetGroupVolume_Info, new object[] { InstanceID, DesiredVolume }, new SetGroupVolume_Result()) as SetGroupVolume_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetRelativeGroupVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetRelativeGroupVolume",
            argnames = new string[] { "InstanceID", "Adjustment" },
            outargs = 1,
        };

        public class SetRelativeGroupVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public ushort NewVolume;


            public override void Fill(string[] rawdata)
            {
				NewVolume = ushort.Parse(rawdata[0]);

            }
        }
        public async Task<SetRelativeGroupVolume_Result> SetRelativeGroupVolume(uint InstanceID, int Adjustment)
        {
            return await base.Action_Async(SetRelativeGroupVolume_Info, new object[] { InstanceID, Adjustment }, new SetRelativeGroupVolume_Result()) as SetRelativeGroupVolume_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SnapshotGroupVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SnapshotGroupVolume",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class SnapshotGroupVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SnapshotGroupVolume_Result> SnapshotGroupVolume(uint InstanceID)
        {
            return await base.Action_Async(SnapshotGroupVolume_Info, new object[] { InstanceID }, new SnapshotGroupVolume_Result()) as SnapshotGroupVolume_Result;
        }
    

    }
}

