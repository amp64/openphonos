
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class RenderingControl1 : OpenPhonos.UPnP.Service
    {
        public RenderingControl1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetMute_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetMute",
            argnames = new string[] { "InstanceID", "Channel" },
            outargs = 1,
        };

        public class GetMute_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool CurrentMute;


            public override void Fill(string[] rawdata)
            {
				CurrentMute = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetMute_Result> GetMute(uint InstanceID, string Channel)
        {
            return await base.Action_Async(GetMute_Info, new object[] { InstanceID, Channel }, new GetMute_Result()) as GetMute_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetMute_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetMute",
            argnames = new string[] { "InstanceID", "Channel", "DesiredMute" },
            outargs = 0,
        };

        public class SetMute_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetMute_Result> SetMute(uint InstanceID, string Channel, bool DesiredMute)
        {
            return await base.Action_Async(SetMute_Info, new object[] { InstanceID, Channel, DesiredMute }, new SetMute_Result()) as SetMute_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ResetBasicEQ_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ResetBasicEQ",
            argnames = new string[] { "InstanceID" },
            outargs = 5,
        };

        public class ResetBasicEQ_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public short Bass;
			public short Treble;
			public bool Loudness;
			public ushort LeftVolume;
			public ushort RightVolume;


            public override void Fill(string[] rawdata)
            {
				Bass = short.Parse(rawdata[0]);
				Treble = short.Parse(rawdata[1]);
				Loudness = ParseBool(rawdata[2]);
				LeftVolume = ushort.Parse(rawdata[3]);
				RightVolume = ushort.Parse(rawdata[4]);

            }
        }
        public async Task<ResetBasicEQ_Result> ResetBasicEQ(uint InstanceID)
        {
            return await base.Action_Async(ResetBasicEQ_Info, new object[] { InstanceID }, new ResetBasicEQ_Result()) as ResetBasicEQ_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ResetExtEQ_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ResetExtEQ",
            argnames = new string[] { "InstanceID", "EQType" },
            outargs = 0,
        };

        public class ResetExtEQ_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ResetExtEQ_Result> ResetExtEQ(uint InstanceID, string EQType)
        {
            return await base.Action_Async(ResetExtEQ_Info, new object[] { InstanceID, EQType }, new ResetExtEQ_Result()) as ResetExtEQ_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetVolume",
            argnames = new string[] { "InstanceID", "Channel" },
            outargs = 1,
        };

        public class GetVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public ushort CurrentVolume;


            public override void Fill(string[] rawdata)
            {
				CurrentVolume = ushort.Parse(rawdata[0]);

            }
        }
        public async Task<GetVolume_Result> GetVolume(uint InstanceID, string Channel)
        {
            return await base.Action_Async(GetVolume_Info, new object[] { InstanceID, Channel }, new GetVolume_Result()) as GetVolume_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetVolume",
            argnames = new string[] { "InstanceID", "Channel", "DesiredVolume" },
            outargs = 0,
        };

        public class SetVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetVolume_Result> SetVolume(uint InstanceID, string Channel, ushort DesiredVolume)
        {
            return await base.Action_Async(SetVolume_Info, new object[] { InstanceID, Channel, DesiredVolume }, new SetVolume_Result()) as SetVolume_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetRelativeVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetRelativeVolume",
            argnames = new string[] { "InstanceID", "Channel", "Adjustment" },
            outargs = 1,
        };

        public class SetRelativeVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public ushort NewVolume;


            public override void Fill(string[] rawdata)
            {
				NewVolume = ushort.Parse(rawdata[0]);

            }
        }
        public async Task<SetRelativeVolume_Result> SetRelativeVolume(uint InstanceID, string Channel, int Adjustment)
        {
            return await base.Action_Async(SetRelativeVolume_Info, new object[] { InstanceID, Channel, Adjustment }, new SetRelativeVolume_Result()) as SetRelativeVolume_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetVolumeDB_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetVolumeDB",
            argnames = new string[] { "InstanceID", "Channel" },
            outargs = 1,
        };

        public class GetVolumeDB_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public short CurrentVolume;


            public override void Fill(string[] rawdata)
            {
				CurrentVolume = short.Parse(rawdata[0]);

            }
        }
        public async Task<GetVolumeDB_Result> GetVolumeDB(uint InstanceID, string Channel)
        {
            return await base.Action_Async(GetVolumeDB_Info, new object[] { InstanceID, Channel }, new GetVolumeDB_Result()) as GetVolumeDB_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetVolumeDB_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetVolumeDB",
            argnames = new string[] { "InstanceID", "Channel", "DesiredVolume" },
            outargs = 0,
        };

        public class SetVolumeDB_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetVolumeDB_Result> SetVolumeDB(uint InstanceID, string Channel, short DesiredVolume)
        {
            return await base.Action_Async(SetVolumeDB_Info, new object[] { InstanceID, Channel, DesiredVolume }, new SetVolumeDB_Result()) as SetVolumeDB_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetVolumeDBRange_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetVolumeDBRange",
            argnames = new string[] { "InstanceID", "Channel" },
            outargs = 2,
        };

        public class GetVolumeDBRange_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public short MinValue;
			public short MaxValue;


            public override void Fill(string[] rawdata)
            {
				MinValue = short.Parse(rawdata[0]);
				MaxValue = short.Parse(rawdata[1]);

            }
        }
        public async Task<GetVolumeDBRange_Result> GetVolumeDBRange(uint InstanceID, string Channel)
        {
            return await base.Action_Async(GetVolumeDBRange_Info, new object[] { InstanceID, Channel }, new GetVolumeDBRange_Result()) as GetVolumeDBRange_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetBass_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetBass",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetBass_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public short CurrentBass;


            public override void Fill(string[] rawdata)
            {
				CurrentBass = short.Parse(rawdata[0]);

            }
        }
        public async Task<GetBass_Result> GetBass(uint InstanceID)
        {
            return await base.Action_Async(GetBass_Info, new object[] { InstanceID }, new GetBass_Result()) as GetBass_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetBass_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetBass",
            argnames = new string[] { "InstanceID", "DesiredBass" },
            outargs = 0,
        };

        public class SetBass_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetBass_Result> SetBass(uint InstanceID, short DesiredBass)
        {
            return await base.Action_Async(SetBass_Info, new object[] { InstanceID, DesiredBass }, new SetBass_Result()) as SetBass_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTreble_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTreble",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetTreble_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public short CurrentTreble;


            public override void Fill(string[] rawdata)
            {
				CurrentTreble = short.Parse(rawdata[0]);

            }
        }
        public async Task<GetTreble_Result> GetTreble(uint InstanceID)
        {
            return await base.Action_Async(GetTreble_Info, new object[] { InstanceID }, new GetTreble_Result()) as GetTreble_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetTreble_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetTreble",
            argnames = new string[] { "InstanceID", "DesiredTreble" },
            outargs = 0,
        };

        public class SetTreble_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetTreble_Result> SetTreble(uint InstanceID, short DesiredTreble)
        {
            return await base.Action_Async(SetTreble_Info, new object[] { InstanceID, DesiredTreble }, new SetTreble_Result()) as SetTreble_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetEQ_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetEQ",
            argnames = new string[] { "InstanceID", "EQType" },
            outargs = 1,
        };

        public class GetEQ_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public short CurrentValue;


            public override void Fill(string[] rawdata)
            {
				CurrentValue = short.Parse(rawdata[0]);

            }
        }
        public async Task<GetEQ_Result> GetEQ(uint InstanceID, string EQType)
        {
            return await base.Action_Async(GetEQ_Info, new object[] { InstanceID, EQType }, new GetEQ_Result()) as GetEQ_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetEQ_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetEQ",
            argnames = new string[] { "InstanceID", "EQType", "DesiredValue" },
            outargs = 0,
        };

        public class SetEQ_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetEQ_Result> SetEQ(uint InstanceID, string EQType, short DesiredValue)
        {
            return await base.Action_Async(SetEQ_Info, new object[] { InstanceID, EQType, DesiredValue }, new SetEQ_Result()) as SetEQ_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetLoudness_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetLoudness",
            argnames = new string[] { "InstanceID", "Channel" },
            outargs = 1,
        };

        public class GetLoudness_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool CurrentLoudness;


            public override void Fill(string[] rawdata)
            {
				CurrentLoudness = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetLoudness_Result> GetLoudness(uint InstanceID, string Channel)
        {
            return await base.Action_Async(GetLoudness_Info, new object[] { InstanceID, Channel }, new GetLoudness_Result()) as GetLoudness_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetLoudness_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetLoudness",
            argnames = new string[] { "InstanceID", "Channel", "DesiredLoudness" },
            outargs = 0,
        };

        public class SetLoudness_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetLoudness_Result> SetLoudness(uint InstanceID, string Channel, bool DesiredLoudness)
        {
            return await base.Action_Async(SetLoudness_Info, new object[] { InstanceID, Channel, DesiredLoudness }, new SetLoudness_Result()) as SetLoudness_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetSupportsOutputFixed_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetSupportsOutputFixed",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetSupportsOutputFixed_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool CurrentSupportsFixed;


            public override void Fill(string[] rawdata)
            {
				CurrentSupportsFixed = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetSupportsOutputFixed_Result> GetSupportsOutputFixed(uint InstanceID)
        {
            return await base.Action_Async(GetSupportsOutputFixed_Info, new object[] { InstanceID }, new GetSupportsOutputFixed_Result()) as GetSupportsOutputFixed_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetOutputFixed_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetOutputFixed",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetOutputFixed_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool CurrentFixed;


            public override void Fill(string[] rawdata)
            {
				CurrentFixed = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetOutputFixed_Result> GetOutputFixed(uint InstanceID)
        {
            return await base.Action_Async(GetOutputFixed_Info, new object[] { InstanceID }, new GetOutputFixed_Result()) as GetOutputFixed_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetOutputFixed_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetOutputFixed",
            argnames = new string[] { "InstanceID", "DesiredFixed" },
            outargs = 0,
        };

        public class SetOutputFixed_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetOutputFixed_Result> SetOutputFixed(uint InstanceID, bool DesiredFixed)
        {
            return await base.Action_Async(SetOutputFixed_Info, new object[] { InstanceID, DesiredFixed }, new SetOutputFixed_Result()) as SetOutputFixed_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetHeadphoneConnected_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetHeadphoneConnected",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetHeadphoneConnected_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool CurrentHeadphoneConnected;


            public override void Fill(string[] rawdata)
            {
				CurrentHeadphoneConnected = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetHeadphoneConnected_Result> GetHeadphoneConnected(uint InstanceID)
        {
            return await base.Action_Async(GetHeadphoneConnected_Info, new object[] { InstanceID }, new GetHeadphoneConnected_Result()) as GetHeadphoneConnected_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RampToVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RampToVolume",
            argnames = new string[] { "InstanceID", "Channel", "RampType", "DesiredVolume", "ResetVolumeAfter", "ProgramURI" },
            outargs = 1,
        };

        public class RampToVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint RampTime;


            public override void Fill(string[] rawdata)
            {
				RampTime = uint.Parse(rawdata[0]);

            }
        }
        public async Task<RampToVolume_Result> RampToVolume(uint InstanceID, string Channel, string RampType, ushort DesiredVolume, bool ResetVolumeAfter, string ProgramURI)
        {
            return await base.Action_Async(RampToVolume_Info, new object[] { InstanceID, Channel, RampType, DesiredVolume, ResetVolumeAfter, ProgramURI }, new RampToVolume_Result()) as RampToVolume_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RestoreVolumePriorToRamp_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RestoreVolumePriorToRamp",
            argnames = new string[] { "InstanceID", "Channel" },
            outargs = 0,
        };

        public class RestoreVolumePriorToRamp_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RestoreVolumePriorToRamp_Result> RestoreVolumePriorToRamp(uint InstanceID, string Channel)
        {
            return await base.Action_Async(RestoreVolumePriorToRamp_Info, new object[] { InstanceID, Channel }, new RestoreVolumePriorToRamp_Result()) as RestoreVolumePriorToRamp_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetChannelMap_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetChannelMap",
            argnames = new string[] { "InstanceID", "ChannelMap" },
            outargs = 0,
        };

        public class SetChannelMap_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetChannelMap_Result> SetChannelMap(uint InstanceID, string ChannelMap)
        {
            return await base.Action_Async(SetChannelMap_Info, new object[] { InstanceID, ChannelMap }, new SetChannelMap_Result()) as SetChannelMap_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetRoomCalibrationX_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetRoomCalibrationX",
            argnames = new string[] { "InstanceID", "CalibrationID", "Coefficients", "CalibrationMode" },
            outargs = 0,
        };

        public class SetRoomCalibrationX_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetRoomCalibrationX_Result> SetRoomCalibrationX(uint InstanceID, string CalibrationID, string Coefficients, string CalibrationMode)
        {
            return await base.Action_Async(SetRoomCalibrationX_Info, new object[] { InstanceID, CalibrationID, Coefficients, CalibrationMode }, new SetRoomCalibrationX_Result()) as SetRoomCalibrationX_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetRoomCalibrationStatus_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetRoomCalibrationStatus",
            argnames = new string[] { "InstanceID" },
            outargs = 2,
        };

        public class GetRoomCalibrationStatus_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool RoomCalibrationEnabled;
			public bool RoomCalibrationAvailable;


            public override void Fill(string[] rawdata)
            {
				RoomCalibrationEnabled = ParseBool(rawdata[0]);
				RoomCalibrationAvailable = ParseBool(rawdata[1]);

            }
        }
        public async Task<GetRoomCalibrationStatus_Result> GetRoomCalibrationStatus(uint InstanceID)
        {
            return await base.Action_Async(GetRoomCalibrationStatus_Info, new object[] { InstanceID }, new GetRoomCalibrationStatus_Result()) as GetRoomCalibrationStatus_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetRoomCalibrationStatus_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetRoomCalibrationStatus",
            argnames = new string[] { "InstanceID", "RoomCalibrationEnabled" },
            outargs = 0,
        };

        public class SetRoomCalibrationStatus_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetRoomCalibrationStatus_Result> SetRoomCalibrationStatus(uint InstanceID, bool RoomCalibrationEnabled)
        {
            return await base.Action_Async(SetRoomCalibrationStatus_Info, new object[] { InstanceID, RoomCalibrationEnabled }, new SetRoomCalibrationStatus_Result()) as SetRoomCalibrationStatus_Result;
        }
    

    }
}

