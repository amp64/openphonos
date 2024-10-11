
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class AudioIn1 : OpenPhonos.UPnP.Service
    {
        public AudioIn1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo StartTransmissionToGroup_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "StartTransmissionToGroup",
            argnames = new string[] { "CoordinatorID" },
            outargs = 1,
        };

        public class StartTransmissionToGroup_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentTransportSettings;


            public override void Fill(string[] rawdata)
            {
				CurrentTransportSettings = rawdata[0];

            }
        }
        public async Task<StartTransmissionToGroup_Result> StartTransmissionToGroup(string CoordinatorID)
        {
            return await base.Action_Async(StartTransmissionToGroup_Info, new object[] { CoordinatorID }, new StartTransmissionToGroup_Result()) as StartTransmissionToGroup_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo StopTransmissionToGroup_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "StopTransmissionToGroup",
            argnames = new string[] { "CoordinatorID" },
            outargs = 0,
        };

        public class StopTransmissionToGroup_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<StopTransmissionToGroup_Result> StopTransmissionToGroup(string CoordinatorID)
        {
            return await base.Action_Async(StopTransmissionToGroup_Info, new object[] { CoordinatorID }, new StopTransmissionToGroup_Result()) as StopTransmissionToGroup_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetAudioInputAttributes_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetAudioInputAttributes",
            argnames = new string[] { "DesiredName", "DesiredIcon" },
            outargs = 0,
        };

        public class SetAudioInputAttributes_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetAudioInputAttributes_Result> SetAudioInputAttributes(string DesiredName, string DesiredIcon)
        {
            return await base.Action_Async(SetAudioInputAttributes_Info, new object[] { DesiredName, DesiredIcon }, new SetAudioInputAttributes_Result()) as SetAudioInputAttributes_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetAudioInputAttributes_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetAudioInputAttributes",
            argnames = new string[] {  },
            outargs = 2,
        };

        public class GetAudioInputAttributes_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentName;
			public string CurrentIcon;


            public override void Fill(string[] rawdata)
            {
				CurrentName = rawdata[0];
				CurrentIcon = rawdata[1];

            }
        }
        public async Task<GetAudioInputAttributes_Result> GetAudioInputAttributes()
        {
            return await base.Action_Async(GetAudioInputAttributes_Info, new object[] {  }, new GetAudioInputAttributes_Result()) as GetAudioInputAttributes_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetLineInLevel_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetLineInLevel",
            argnames = new string[] { "DesiredLeftLineInLevel", "DesiredRightLineInLevel" },
            outargs = 0,
        };

        public class SetLineInLevel_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetLineInLevel_Result> SetLineInLevel(int DesiredLeftLineInLevel, int DesiredRightLineInLevel)
        {
            return await base.Action_Async(SetLineInLevel_Info, new object[] { DesiredLeftLineInLevel, DesiredRightLineInLevel }, new SetLineInLevel_Result()) as SetLineInLevel_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetLineInLevel_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetLineInLevel",
            argnames = new string[] {  },
            outargs = 2,
        };

        public class GetLineInLevel_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public int CurrentLeftLineInLevel;
			public int CurrentRightLineInLevel;


            public override void Fill(string[] rawdata)
            {
				CurrentLeftLineInLevel = int.Parse(rawdata[0]);
				CurrentRightLineInLevel = int.Parse(rawdata[1]);

            }
        }
        public async Task<GetLineInLevel_Result> GetLineInLevel()
        {
            return await base.Action_Async(GetLineInLevel_Info, new object[] {  }, new GetLineInLevel_Result()) as GetLineInLevel_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SelectAudio_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SelectAudio",
            argnames = new string[] { "ObjectID" },
            outargs = 0,
        };

        public class SelectAudio_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SelectAudio_Result> SelectAudio(string ObjectID)
        {
            return await base.Action_Async(SelectAudio_Info, new object[] { ObjectID }, new SelectAudio_Result()) as SelectAudio_Result;
        }
    

    }
}

