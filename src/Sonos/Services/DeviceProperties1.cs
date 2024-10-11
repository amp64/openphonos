
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class DeviceProperties1 : OpenPhonos.UPnP.Service
    {
        public DeviceProperties1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetLEDState_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetLEDState",
            argnames = new string[] { "DesiredLEDState" },
            outargs = 0,
        };

        public class SetLEDState_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetLEDState_Result> SetLEDState(string DesiredLEDState)
        {
            return await base.Action_Async(SetLEDState_Info, new object[] { DesiredLEDState }, new SetLEDState_Result()) as SetLEDState_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetLEDState_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetLEDState",
            argnames = new string[] { },
            outargs = 1,
        };

        public class GetLEDState_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string CurrentLEDState;


            public override void Fill(string[] rawdata)
            {
                CurrentLEDState = rawdata[0];

            }
        }
        public async Task<GetLEDState_Result> GetLEDState()
        {
            return await base.Action_Async(GetLEDState_Info, new object[] { }, new GetLEDState_Result()) as GetLEDState_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo AddBondedZones_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddBondedZones",
            argnames = new string[] { "ChannelMapSet" },
            outargs = 0,
        };

        public class AddBondedZones_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<AddBondedZones_Result> AddBondedZones(string ChannelMapSet)
        {
            return await base.Action_Async(AddBondedZones_Info, new object[] { ChannelMapSet }, new AddBondedZones_Result()) as AddBondedZones_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo RemoveBondedZones_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveBondedZones",
            argnames = new string[] { "ChannelMapSet", "KeepGrouped" },
            outargs = 0,
        };

        public class RemoveBondedZones_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RemoveBondedZones_Result> RemoveBondedZones(string ChannelMapSet, bool KeepGrouped)
        {
            return await base.Action_Async(RemoveBondedZones_Info, new object[] { ChannelMapSet, KeepGrouped }, new RemoveBondedZones_Result()) as RemoveBondedZones_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo CreateStereoPair_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "CreateStereoPair",
            argnames = new string[] { "ChannelMapSet" },
            outargs = 0,
        };

        public class CreateStereoPair_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<CreateStereoPair_Result> CreateStereoPair(string ChannelMapSet)
        {
            return await base.Action_Async(CreateStereoPair_Info, new object[] { ChannelMapSet }, new CreateStereoPair_Result()) as CreateStereoPair_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SeparateStereoPair_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SeparateStereoPair",
            argnames = new string[] { "ChannelMapSet" },
            outargs = 0,
        };

        public class SeparateStereoPair_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SeparateStereoPair_Result> SeparateStereoPair(string ChannelMapSet)
        {
            return await base.Action_Async(SeparateStereoPair_Info, new object[] { ChannelMapSet }, new SeparateStereoPair_Result()) as SeparateStereoPair_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetZoneAttributes_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetZoneAttributes",
            argnames = new string[] { "DesiredZoneName", "DesiredIcon", "DesiredConfiguration" },
            outargs = 0,
        };

        public class SetZoneAttributes_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetZoneAttributes_Result> SetZoneAttributes(string DesiredZoneName, string DesiredIcon, string DesiredConfiguration)
        {
            return await base.Action_Async(SetZoneAttributes_Info, new object[] { DesiredZoneName, DesiredIcon, DesiredConfiguration }, new SetZoneAttributes_Result()) as SetZoneAttributes_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetZoneAttributes_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetZoneAttributes",
            argnames = new string[] { },
            outargs = 2,
        };

        public class GetZoneAttributes_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string CurrentZoneName;
            public string CurrentIcon;
            public string CurrentConfiguration;


            public override void Fill(string[] rawdata)
            {
                CurrentZoneName = rawdata[0];
                CurrentIcon = rawdata[1];
                if (rawdata.Length > 2)
                    CurrentConfiguration = rawdata[2];

            }
        }
        public async Task<GetZoneAttributes_Result> GetZoneAttributes()
        {
            return await base.Action_Async(GetZoneAttributes_Info, new object[] { }, new GetZoneAttributes_Result()) as GetZoneAttributes_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetHouseholdID_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetHouseholdID",
            argnames = new string[] { },
            outargs = 1,
        };

        public class GetHouseholdID_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string CurrentHouseholdID;


            public override void Fill(string[] rawdata)
            {
                CurrentHouseholdID = rawdata[0];

            }
        }
        public async Task<GetHouseholdID_Result> GetHouseholdID()
        {
            return await base.Action_Async(GetHouseholdID_Info, new object[] { }, new GetHouseholdID_Result()) as GetHouseholdID_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetZoneInfo_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetZoneInfo",
            argnames = new string[] { },
            outargs = 8,
        };

        public class GetZoneInfo_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string SerialNumber;
            public string SoftwareVersion;
            public string DisplaySoftwareVersion;
            public string HardwareVersion;
            public string IPAddress;
            public string MACAddress;
            public string CopyrightInfo;
            public string ExtraInfo;
            public uint HTAudioIn;
            public uint Flags;


            public override void Fill(string[] rawdata)
            {
                SerialNumber = rawdata[0];
                SoftwareVersion = rawdata[1];
                DisplaySoftwareVersion = rawdata[2];
                HardwareVersion = rawdata[3];
                IPAddress = rawdata[4];
                MACAddress = rawdata[5];
                CopyrightInfo = rawdata[6];
                ExtraInfo = rawdata[7];
                if (rawdata.Length > 8)
                    HTAudioIn = uint.Parse(rawdata[8]);
                if (rawdata.Length > 9)
                    Flags = uint.Parse(rawdata[9]);

            }
        }
        public async Task<GetZoneInfo_Result> GetZoneInfo()
        {
            return await base.Action_Async(GetZoneInfo_Info, new object[] { }, new GetZoneInfo_Result()) as GetZoneInfo_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetAutoplayLinkedZones_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetAutoplayLinkedZones",
            argnames = new string[] { "IncludeLinkedZones", "Source" },
            outargs = 0,
        };

        public class SetAutoplayLinkedZones_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetAutoplayLinkedZones_Result> SetAutoplayLinkedZones(bool IncludeLinkedZones, string Source)
        {
            return await base.Action_Async(SetAutoplayLinkedZones_Info, new object[] { IncludeLinkedZones, Source }, new SetAutoplayLinkedZones_Result()) as SetAutoplayLinkedZones_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetAutoplayLinkedZones_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetAutoplayLinkedZones",
            argnames = new string[] { "Source" },
            outargs = 1,
        };

        public class GetAutoplayLinkedZones_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public bool IncludeLinkedZones;


            public override void Fill(string[] rawdata)
            {
                IncludeLinkedZones = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetAutoplayLinkedZones_Result> GetAutoplayLinkedZones(string Source)
        {
            return await base.Action_Async(GetAutoplayLinkedZones_Info, new object[] { Source }, new GetAutoplayLinkedZones_Result()) as GetAutoplayLinkedZones_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetAutoplayRoomUUID_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetAutoplayRoomUUID",
            argnames = new string[] { "RoomUUID", "Source" },
            outargs = 0,
        };

        public class SetAutoplayRoomUUID_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetAutoplayRoomUUID_Result> SetAutoplayRoomUUID(string RoomUUID, string Source)
        {
            return await base.Action_Async(SetAutoplayRoomUUID_Info, new object[] { RoomUUID, Source }, new SetAutoplayRoomUUID_Result()) as SetAutoplayRoomUUID_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetAutoplayRoomUUID_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetAutoplayRoomUUID",
            argnames = new string[] { "Source" },
            outargs = 1,
        };

        public class GetAutoplayRoomUUID_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string RoomUUID;


            public override void Fill(string[] rawdata)
            {
                RoomUUID = rawdata[0];

            }
        }
        public async Task<GetAutoplayRoomUUID_Result> GetAutoplayRoomUUID(string Source)
        {
            return await base.Action_Async(GetAutoplayRoomUUID_Info, new object[] { Source }, new GetAutoplayRoomUUID_Result()) as GetAutoplayRoomUUID_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetAutoplayVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetAutoplayVolume",
            argnames = new string[] { "Volume", "Source" },
            outargs = 0,
        };

        public class SetAutoplayVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetAutoplayVolume_Result> SetAutoplayVolume(ushort Volume, string Source)
        {
            return await base.Action_Async(SetAutoplayVolume_Info, new object[] { Volume, Source }, new SetAutoplayVolume_Result()) as SetAutoplayVolume_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetAutoplayVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetAutoplayVolume",
            argnames = new string[] { "Source" },
            outargs = 1,
        };

        public class GetAutoplayVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public ushort CurrentVolume;


            public override void Fill(string[] rawdata)
            {
                CurrentVolume = ushort.Parse(rawdata[0]);

            }
        }
        public async Task<GetAutoplayVolume_Result> GetAutoplayVolume(string Source)
        {
            return await base.Action_Async(GetAutoplayVolume_Info, new object[] { Source }, new GetAutoplayVolume_Result()) as GetAutoplayVolume_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetUseAutoplayVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetUseAutoplayVolume",
            argnames = new string[] { "UseVolume", "Source" },
            outargs = 0,
        };

        public class SetUseAutoplayVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetUseAutoplayVolume_Result> SetUseAutoplayVolume(bool UseVolume, string Source)
        {
            return await base.Action_Async(SetUseAutoplayVolume_Info, new object[] { UseVolume, Source }, new SetUseAutoplayVolume_Result()) as SetUseAutoplayVolume_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetUseAutoplayVolume_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetUseAutoplayVolume",
            argnames = new string[] { "Source" },
            outargs = 1,
        };

        public class GetUseAutoplayVolume_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public bool UseVolume;


            public override void Fill(string[] rawdata)
            {
                UseVolume = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetUseAutoplayVolume_Result> GetUseAutoplayVolume(string Source)
        {
            return await base.Action_Async(GetUseAutoplayVolume_Info, new object[] { Source }, new GetUseAutoplayVolume_Result()) as GetUseAutoplayVolume_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo AddHTSatellite_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddHTSatellite",
            argnames = new string[] { "HTSatChanMapSet" },
            outargs = 0,
        };

        public class AddHTSatellite_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<AddHTSatellite_Result> AddHTSatellite(string HTSatChanMapSet)
        {
            return await base.Action_Async(AddHTSatellite_Info, new object[] { HTSatChanMapSet }, new AddHTSatellite_Result()) as AddHTSatellite_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo RemoveHTSatellite_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveHTSatellite",
            argnames = new string[] { "SatRoomUUID" },
            outargs = 0,
        };

        public class RemoveHTSatellite_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RemoveHTSatellite_Result> RemoveHTSatellite(string SatRoomUUID)
        {
            return await base.Action_Async(RemoveHTSatellite_Info, new object[] { SatRoomUUID }, new RemoveHTSatellite_Result()) as RemoveHTSatellite_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo EnterConfigMode_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "EnterConfigMode",
            argnames = new string[] { "Mode", "Options" },
            outargs = 1,
        };

        public class EnterConfigMode_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string State;


            public override void Fill(string[] rawdata)
            {
                State = rawdata[0];

            }
        }
        public async Task<EnterConfigMode_Result> EnterConfigMode(string Mode, string Options)
        {
            return await base.Action_Async(EnterConfigMode_Info, new object[] { Mode, Options }, new EnterConfigMode_Result()) as EnterConfigMode_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo ExitConfigMode_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ExitConfigMode",
            argnames = new string[] { "Options" },
            outargs = 0,
        };

        public class ExitConfigMode_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ExitConfigMode_Result> ExitConfigMode(string Options)
        {
            return await base.Action_Async(ExitConfigMode_Info, new object[] { Options }, new ExitConfigMode_Result()) as ExitConfigMode_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetButtonState_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetButtonState",
            argnames = new string[] { },
            outargs = 1,
        };

        public class GetButtonState_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string State;


            public override void Fill(string[] rawdata)
            {
                State = rawdata[0];

            }
        }
        public async Task<GetButtonState_Result> GetButtonState()
        {
            return await base.Action_Async(GetButtonState_Info, new object[] { }, new GetButtonState_Result()) as GetButtonState_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetButtonLockState_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetButtonLockState",
            argnames = new string[] { "DesiredButtonLockState" },
            outargs = 0,
        };

        public class SetButtonLockState_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetButtonLockState_Result> SetButtonLockState(string DesiredButtonLockState)
        {
            return await base.Action_Async(SetButtonLockState_Info, new object[] { DesiredButtonLockState }, new SetButtonLockState_Result()) as SetButtonLockState_Result;
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetButtonLockState_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetButtonLockState",
            argnames = new string[] { },
            outargs = 1,
        };

        public class GetButtonLockState_Result : OpenPhonos.UPnP.Service.ActionResult
        {
            public string CurrentButtonLockState;


            public override void Fill(string[] rawdata)
            {
                CurrentButtonLockState = rawdata[0];

            }
        }
        public async Task<GetButtonLockState_Result> GetButtonLockState()
        {
            return await base.Action_Async(GetButtonLockState_Info, new object[] { }, new GetButtonLockState_Result()) as GetButtonLockState_Result;
        }


    }
}

