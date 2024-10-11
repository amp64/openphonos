
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class VirtualLineIn1 : OpenPhonos.UPnP.Service
    {
        public VirtualLineIn1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo StartTransmission_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "StartTransmission",
            argnames = new string[] { "InstanceID", "CoordinatorID" },
            outargs = 1,
        };

        public class StartTransmission_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentTransportSettings;


            public override void Fill(string[] rawdata)
            {
				CurrentTransportSettings = rawdata[0];

            }
        }
        public async Task<StartTransmission_Result> StartTransmission(uint InstanceID, string CoordinatorID)
        {
            return await base.Action_Async(StartTransmission_Info, new object[] { InstanceID, CoordinatorID }, new StartTransmission_Result()) as StartTransmission_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo StopTransmission_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "StopTransmission",
            argnames = new string[] { "InstanceID", "CoordinatorID" },
            outargs = 0,
        };

        public class StopTransmission_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<StopTransmission_Result> StopTransmission(uint InstanceID, string CoordinatorID)
        {
            return await base.Action_Async(StopTransmission_Info, new object[] { InstanceID, CoordinatorID }, new StopTransmission_Result()) as StopTransmission_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Play_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Play",
            argnames = new string[] { "InstanceID", "Speed" },
            outargs = 0,
        };

        public class Play_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Play_Result> Play(uint InstanceID, string Speed)
        {
            return await base.Action_Async(Play_Info, new object[] { InstanceID, Speed }, new Play_Result()) as Play_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Pause_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Pause",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class Pause_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Pause_Result> Pause(uint InstanceID)
        {
            return await base.Action_Async(Pause_Info, new object[] { InstanceID }, new Pause_Result()) as Pause_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Next_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Next",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class Next_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Next_Result> Next(uint InstanceID)
        {
            return await base.Action_Async(Next_Info, new object[] { InstanceID }, new Next_Result()) as Next_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Previous_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Previous",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class Previous_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Previous_Result> Previous(uint InstanceID)
        {
            return await base.Action_Async(Previous_Info, new object[] { InstanceID }, new Previous_Result()) as Previous_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Stop_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Stop",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class Stop_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Stop_Result> Stop(uint InstanceID)
        {
            return await base.Action_Async(Stop_Info, new object[] { InstanceID }, new Stop_Result()) as Stop_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetVolume",
            argnames = new string[] { "InstanceID", "DesiredVolume" },
            outargs = 0,
        };

        public class SetVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetVolume_Result> SetVolume(uint InstanceID, ushort DesiredVolume)
        {
            return await base.Action_Async(SetVolume_Info, new object[] { InstanceID, DesiredVolume }, new SetVolume_Result()) as SetVolume_Result;
        }
    

    }
}

