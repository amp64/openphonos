
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class AlarmClock1 : OpenPhonos.UPnP.Service
    {
        public AlarmClock1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetFormat_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetFormat",
            argnames = new string[] { "DesiredTimeFormat", "DesiredDateFormat" },
            outargs = 0,
        };

        public class SetFormat_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetFormat_Result> SetFormat(string DesiredTimeFormat, string DesiredDateFormat)
        {
            return await base.Action_Async(SetFormat_Info, new object[] { DesiredTimeFormat, DesiredDateFormat }, new SetFormat_Result()) as SetFormat_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetFormat_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetFormat",
            argnames = new string[] {  },
            outargs = 2,
        };

        public class GetFormat_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentTimeFormat;
			public string CurrentDateFormat;


            public override void Fill(string[] rawdata)
            {
				CurrentTimeFormat = rawdata[0];
				CurrentDateFormat = rawdata[1];

            }
        }
        public async Task<GetFormat_Result> GetFormat()
        {
            return await base.Action_Async(GetFormat_Info, new object[] {  }, new GetFormat_Result()) as GetFormat_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetTimeZone_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetTimeZone",
            argnames = new string[] { "Index", "AutoAdjustDst" },
            outargs = 0,
        };

        public class SetTimeZone_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetTimeZone_Result> SetTimeZone(int Index, bool AutoAdjustDst)
        {
            return await base.Action_Async(SetTimeZone_Info, new object[] { Index, AutoAdjustDst }, new SetTimeZone_Result()) as SetTimeZone_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTimeZone_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTimeZone",
            argnames = new string[] {  },
            outargs = 2,
        };

        public class GetTimeZone_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public int Index;
			public bool AutoAdjustDst;


            public override void Fill(string[] rawdata)
            {
				Index = int.Parse(rawdata[0]);
				AutoAdjustDst = ParseBool(rawdata[1]);

            }
        }
        public async Task<GetTimeZone_Result> GetTimeZone()
        {
            return await base.Action_Async(GetTimeZone_Info, new object[] {  }, new GetTimeZone_Result()) as GetTimeZone_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTimeZoneAndRule_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTimeZoneAndRule",
            argnames = new string[] {  },
            outargs = 3,
        };

        public class GetTimeZoneAndRule_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public int Index;
			public bool AutoAdjustDst;
			public string CurrentTimeZone;


            public override void Fill(string[] rawdata)
            {
				Index = int.Parse(rawdata[0]);
				AutoAdjustDst = ParseBool(rawdata[1]);
				CurrentTimeZone = rawdata[2];

            }
        }
        public async Task<GetTimeZoneAndRule_Result> GetTimeZoneAndRule()
        {
            return await base.Action_Async(GetTimeZoneAndRule_Info, new object[] {  }, new GetTimeZoneAndRule_Result()) as GetTimeZoneAndRule_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTimeZoneRule_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTimeZoneRule",
            argnames = new string[] { "Index" },
            outargs = 1,
        };

        public class GetTimeZoneRule_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string TimeZone;


            public override void Fill(string[] rawdata)
            {
				TimeZone = rawdata[0];

            }
        }
        public async Task<GetTimeZoneRule_Result> GetTimeZoneRule(int Index)
        {
            return await base.Action_Async(GetTimeZoneRule_Info, new object[] { Index }, new GetTimeZoneRule_Result()) as GetTimeZoneRule_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetTimeServer_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetTimeServer",
            argnames = new string[] { "DesiredTimeServer" },
            outargs = 0,
        };

        public class SetTimeServer_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetTimeServer_Result> SetTimeServer(string DesiredTimeServer)
        {
            return await base.Action_Async(SetTimeServer_Info, new object[] { DesiredTimeServer }, new SetTimeServer_Result()) as SetTimeServer_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTimeServer_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTimeServer",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetTimeServer_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentTimeServer;


            public override void Fill(string[] rawdata)
            {
				CurrentTimeServer = rawdata[0];

            }
        }
        public async Task<GetTimeServer_Result> GetTimeServer()
        {
            return await base.Action_Async(GetTimeServer_Info, new object[] {  }, new GetTimeServer_Result()) as GetTimeServer_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetTimeNow_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetTimeNow",
            argnames = new string[] { "DesiredTime", "TimeZoneForDesiredTime" },
            outargs = 0,
        };

        public class SetTimeNow_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetTimeNow_Result> SetTimeNow(string DesiredTime, string TimeZoneForDesiredTime)
        {
            return await base.Action_Async(SetTimeNow_Info, new object[] { DesiredTime, TimeZoneForDesiredTime }, new SetTimeNow_Result()) as SetTimeNow_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetHouseholdTimeAtStamp_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetHouseholdTimeAtStamp",
            argnames = new string[] { "TimeStamp" },
            outargs = 1,
        };

        public class GetHouseholdTimeAtStamp_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string HouseholdUTCTime;


            public override void Fill(string[] rawdata)
            {
				HouseholdUTCTime = rawdata[0];

            }
        }
        public async Task<GetHouseholdTimeAtStamp_Result> GetHouseholdTimeAtStamp(string TimeStamp)
        {
            return await base.Action_Async(GetHouseholdTimeAtStamp_Info, new object[] { TimeStamp }, new GetHouseholdTimeAtStamp_Result()) as GetHouseholdTimeAtStamp_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTimeNow_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTimeNow",
            argnames = new string[] {  },
            outargs = 4,
        };

        public class GetTimeNow_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentUTCTime;
			public string CurrentLocalTime;
			public string CurrentTimeZone;
			public uint CurrentTimeGeneration;


            public override void Fill(string[] rawdata)
            {
				CurrentUTCTime = rawdata[0];
				CurrentLocalTime = rawdata[1];
				CurrentTimeZone = rawdata[2];
				CurrentTimeGeneration = uint.Parse(rawdata[3]);

            }
        }
        public async Task<GetTimeNow_Result> GetTimeNow()
        {
            return await base.Action_Async(GetTimeNow_Info, new object[] {  }, new GetTimeNow_Result()) as GetTimeNow_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo CreateAlarm_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "CreateAlarm",
            argnames = new string[] { "StartLocalTime", "Duration", "Recurrence", "Enabled", "RoomUUID", "ProgramURI", "ProgramMetaData", "PlayMode", "Volume", "IncludeLinkedZones" },
            outargs = 1,
        };

        public class CreateAlarm_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint AssignedID;


            public override void Fill(string[] rawdata)
            {
				AssignedID = uint.Parse(rawdata[0]);

            }
        }
        public async Task<CreateAlarm_Result> CreateAlarm(string StartLocalTime, string Duration, string Recurrence, bool Enabled, string RoomUUID, string ProgramURI, string ProgramMetaData, string PlayMode, ushort Volume, bool IncludeLinkedZones)
        {
            return await base.Action_Async(CreateAlarm_Info, new object[] { StartLocalTime, Duration, Recurrence, Enabled, RoomUUID, ProgramURI, ProgramMetaData, PlayMode, Volume, IncludeLinkedZones }, new CreateAlarm_Result()) as CreateAlarm_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo UpdateAlarm_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "UpdateAlarm",
            argnames = new string[] { "ID", "StartLocalTime", "Duration", "Recurrence", "Enabled", "RoomUUID", "ProgramURI", "ProgramMetaData", "PlayMode", "Volume", "IncludeLinkedZones" },
            outargs = 0,
        };

        public class UpdateAlarm_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<UpdateAlarm_Result> UpdateAlarm(uint ID, string StartLocalTime, string Duration, string Recurrence, bool Enabled, string RoomUUID, string ProgramURI, string ProgramMetaData, string PlayMode, ushort Volume, bool IncludeLinkedZones)
        {
            return await base.Action_Async(UpdateAlarm_Info, new object[] { ID, StartLocalTime, Duration, Recurrence, Enabled, RoomUUID, ProgramURI, ProgramMetaData, PlayMode, Volume, IncludeLinkedZones }, new UpdateAlarm_Result()) as UpdateAlarm_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo DestroyAlarm_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "DestroyAlarm",
            argnames = new string[] { "ID" },
            outargs = 0,
        };

        public class DestroyAlarm_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<DestroyAlarm_Result> DestroyAlarm(uint ID)
        {
            return await base.Action_Async(DestroyAlarm_Info, new object[] { ID }, new DestroyAlarm_Result()) as DestroyAlarm_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ListAlarms_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ListAlarms",
            argnames = new string[] {  },
            outargs = 2,
        };

        public class ListAlarms_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentAlarmList;
			public string CurrentAlarmListVersion;


            public override void Fill(string[] rawdata)
            {
				CurrentAlarmList = rawdata[0];
				CurrentAlarmListVersion = rawdata[1];

            }
        }
        public async Task<ListAlarms_Result> ListAlarms()
        {
            return await base.Action_Async(ListAlarms_Info, new object[] {  }, new ListAlarms_Result()) as ListAlarms_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetDailyIndexRefreshTime_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetDailyIndexRefreshTime",
            argnames = new string[] { "DesiredDailyIndexRefreshTime" },
            outargs = 0,
        };

        public class SetDailyIndexRefreshTime_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetDailyIndexRefreshTime_Result> SetDailyIndexRefreshTime(string DesiredDailyIndexRefreshTime)
        {
            return await base.Action_Async(SetDailyIndexRefreshTime_Info, new object[] { DesiredDailyIndexRefreshTime }, new SetDailyIndexRefreshTime_Result()) as SetDailyIndexRefreshTime_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetDailyIndexRefreshTime_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetDailyIndexRefreshTime",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetDailyIndexRefreshTime_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentDailyIndexRefreshTime;


            public override void Fill(string[] rawdata)
            {
				CurrentDailyIndexRefreshTime = rawdata[0];

            }
        }
        public async Task<GetDailyIndexRefreshTime_Result> GetDailyIndexRefreshTime()
        {
            return await base.Action_Async(GetDailyIndexRefreshTime_Info, new object[] {  }, new GetDailyIndexRefreshTime_Result()) as GetDailyIndexRefreshTime_Result;
        }
    

    }
}

