
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class AVTransport1 : OpenPhonos.UPnP.Service
    {
        public AVTransport1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo SetAVTransportURI_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetAVTransportURI",
            argnames = new string[] { "InstanceID", "CurrentURI", "CurrentURIMetaData" },
            outargs = 0,
        };

        public class SetAVTransportURI_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetAVTransportURI_Result> SetAVTransportURI(uint InstanceID, string CurrentURI, string CurrentURIMetaData)
        {
            return await base.Action_Async(SetAVTransportURI_Info, new object[] { InstanceID, CurrentURI, CurrentURIMetaData }, new SetAVTransportURI_Result()) as SetAVTransportURI_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetNextAVTransportURI_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetNextAVTransportURI",
            argnames = new string[] { "InstanceID", "NextURI", "NextURIMetaData" },
            outargs = 0,
        };

        public class SetNextAVTransportURI_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetNextAVTransportURI_Result> SetNextAVTransportURI(uint InstanceID, string NextURI, string NextURIMetaData)
        {
            return await base.Action_Async(SetNextAVTransportURI_Info, new object[] { InstanceID, NextURI, NextURIMetaData }, new SetNextAVTransportURI_Result()) as SetNextAVTransportURI_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo AddURIToQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddURIToQueue",
            argnames = new string[] { "InstanceID", "EnqueuedURI", "EnqueuedURIMetaData", "DesiredFirstTrackNumberEnqueued", "EnqueueAsNext" },
            outargs = 3,
        };

        public class AddURIToQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint FirstTrackNumberEnqueued;
			public uint NumTracksAdded;
			public uint NewQueueLength;


            public override void Fill(string[] rawdata)
            {
				FirstTrackNumberEnqueued = uint.Parse(rawdata[0]);
				NumTracksAdded = uint.Parse(rawdata[1]);
				NewQueueLength = uint.Parse(rawdata[2]);

            }
        }
        public async Task<AddURIToQueue_Result> AddURIToQueue(uint InstanceID, string EnqueuedURI, string EnqueuedURIMetaData, uint DesiredFirstTrackNumberEnqueued, bool EnqueueAsNext)
        {
            return await base.Action_Async(AddURIToQueue_Info, new object[] { InstanceID, EnqueuedURI, EnqueuedURIMetaData, DesiredFirstTrackNumberEnqueued, EnqueueAsNext }, new AddURIToQueue_Result()) as AddURIToQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo AddMultipleURIsToQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddMultipleURIsToQueue",
            argnames = new string[] { "InstanceID", "UpdateID", "NumberOfURIs", "EnqueuedURIs", "EnqueuedURIsMetaData", "ContainerURI", "ContainerMetaData", "DesiredFirstTrackNumberEnqueued", "EnqueueAsNext" },
            outargs = 4,
        };

        public class AddMultipleURIsToQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint FirstTrackNumberEnqueued;
			public uint NumTracksAdded;
			public uint NewQueueLength;
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				FirstTrackNumberEnqueued = uint.Parse(rawdata[0]);
				NumTracksAdded = uint.Parse(rawdata[1]);
				NewQueueLength = uint.Parse(rawdata[2]);
				NewUpdateID = uint.Parse(rawdata[3]);

            }
        }
        public async Task<AddMultipleURIsToQueue_Result> AddMultipleURIsToQueue(uint InstanceID, uint UpdateID, uint NumberOfURIs, string EnqueuedURIs, string EnqueuedURIsMetaData, string ContainerURI, string ContainerMetaData, uint DesiredFirstTrackNumberEnqueued, bool EnqueueAsNext)
        {
            return await base.Action_Async(AddMultipleURIsToQueue_Info, new object[] { InstanceID, UpdateID, NumberOfURIs, EnqueuedURIs, EnqueuedURIsMetaData, ContainerURI, ContainerMetaData, DesiredFirstTrackNumberEnqueued, EnqueueAsNext }, new AddMultipleURIsToQueue_Result()) as AddMultipleURIsToQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReorderTracksInQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReorderTracksInQueue",
            argnames = new string[] { "InstanceID", "StartingIndex", "NumberOfTracks", "InsertBefore", "UpdateID" },
            outargs = 0,
        };

        public class ReorderTracksInQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ReorderTracksInQueue_Result> ReorderTracksInQueue(uint InstanceID, uint StartingIndex, uint NumberOfTracks, uint InsertBefore, uint UpdateID)
        {
            return await base.Action_Async(ReorderTracksInQueue_Info, new object[] { InstanceID, StartingIndex, NumberOfTracks, InsertBefore, UpdateID }, new ReorderTracksInQueue_Result()) as ReorderTracksInQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RemoveTrackFromQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveTrackFromQueue",
            argnames = new string[] { "InstanceID", "ObjectID", "UpdateID" },
            outargs = 0,
        };

        public class RemoveTrackFromQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RemoveTrackFromQueue_Result> RemoveTrackFromQueue(uint InstanceID, string ObjectID, uint UpdateID)
        {
            return await base.Action_Async(RemoveTrackFromQueue_Info, new object[] { InstanceID, ObjectID, UpdateID }, new RemoveTrackFromQueue_Result()) as RemoveTrackFromQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RemoveTrackRangeFromQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveTrackRangeFromQueue",
            argnames = new string[] { "InstanceID", "UpdateID", "StartingIndex", "NumberOfTracks" },
            outargs = 1,
        };

        public class RemoveTrackRangeFromQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				NewUpdateID = uint.Parse(rawdata[0]);

            }
        }
        public async Task<RemoveTrackRangeFromQueue_Result> RemoveTrackRangeFromQueue(uint InstanceID, uint UpdateID, uint StartingIndex, uint NumberOfTracks)
        {
            return await base.Action_Async(RemoveTrackRangeFromQueue_Info, new object[] { InstanceID, UpdateID, StartingIndex, NumberOfTracks }, new RemoveTrackRangeFromQueue_Result()) as RemoveTrackRangeFromQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RemoveAllTracksFromQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveAllTracksFromQueue",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class RemoveAllTracksFromQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RemoveAllTracksFromQueue_Result> RemoveAllTracksFromQueue(uint InstanceID)
        {
            return await base.Action_Async(RemoveAllTracksFromQueue_Info, new object[] { InstanceID }, new RemoveAllTracksFromQueue_Result()) as RemoveAllTracksFromQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SaveQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SaveQueue",
            argnames = new string[] { "InstanceID", "Title", "ObjectID" },
            outargs = 1,
        };

        public class SaveQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string AssignedObjectID;


            public override void Fill(string[] rawdata)
            {
				AssignedObjectID = rawdata[0];

            }
        }
        public async Task<SaveQueue_Result> SaveQueue(uint InstanceID, string Title, string ObjectID)
        {
            return await base.Action_Async(SaveQueue_Info, new object[] { InstanceID, Title, ObjectID }, new SaveQueue_Result()) as SaveQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo BackupQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "BackupQueue",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class BackupQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<BackupQueue_Result> BackupQueue(uint InstanceID)
        {
            return await base.Action_Async(BackupQueue_Info, new object[] { InstanceID }, new BackupQueue_Result()) as BackupQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo CreateSavedQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "CreateSavedQueue",
            argnames = new string[] { "InstanceID", "Title", "EnqueuedURI", "EnqueuedURIMetaData" },
            outargs = 4,
        };

        public class CreateSavedQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NumTracksAdded;
			public uint NewQueueLength;
			public string AssignedObjectID;
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				NumTracksAdded = uint.Parse(rawdata[0]);
				NewQueueLength = uint.Parse(rawdata[1]);
				AssignedObjectID = rawdata[2];
				NewUpdateID = uint.Parse(rawdata[3]);

            }
        }
        public async Task<CreateSavedQueue_Result> CreateSavedQueue(uint InstanceID, string Title, string EnqueuedURI, string EnqueuedURIMetaData)
        {
            return await base.Action_Async(CreateSavedQueue_Info, new object[] { InstanceID, Title, EnqueuedURI, EnqueuedURIMetaData }, new CreateSavedQueue_Result()) as CreateSavedQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo AddURIToSavedQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddURIToSavedQueue",
            argnames = new string[] { "InstanceID", "ObjectID", "UpdateID", "EnqueuedURI", "EnqueuedURIMetaData", "AddAtIndex" },
            outargs = 3,
        };

        public class AddURIToSavedQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NumTracksAdded;
			public uint NewQueueLength;
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				NumTracksAdded = uint.Parse(rawdata[0]);
				NewQueueLength = uint.Parse(rawdata[1]);
				NewUpdateID = uint.Parse(rawdata[2]);

            }
        }
        public async Task<AddURIToSavedQueue_Result> AddURIToSavedQueue(uint InstanceID, string ObjectID, uint UpdateID, string EnqueuedURI, string EnqueuedURIMetaData, uint AddAtIndex)
        {
            return await base.Action_Async(AddURIToSavedQueue_Info, new object[] { InstanceID, ObjectID, UpdateID, EnqueuedURI, EnqueuedURIMetaData, AddAtIndex }, new AddURIToSavedQueue_Result()) as AddURIToSavedQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReorderTracksInSavedQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReorderTracksInSavedQueue",
            argnames = new string[] { "InstanceID", "ObjectID", "UpdateID", "TrackList", "NewPositionList" },
            outargs = 3,
        };

        public class ReorderTracksInSavedQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public int QueueLengthChange;
			public uint NewQueueLength;
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				QueueLengthChange = int.Parse(rawdata[0]);
				NewQueueLength = uint.Parse(rawdata[1]);
				NewUpdateID = uint.Parse(rawdata[2]);

            }
        }
        public async Task<ReorderTracksInSavedQueue_Result> ReorderTracksInSavedQueue(uint InstanceID, string ObjectID, uint UpdateID, string TrackList, string NewPositionList)
        {
            return await base.Action_Async(ReorderTracksInSavedQueue_Info, new object[] { InstanceID, ObjectID, UpdateID, TrackList, NewPositionList }, new ReorderTracksInSavedQueue_Result()) as ReorderTracksInSavedQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetMediaInfo_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetMediaInfo",
            argnames = new string[] { "InstanceID" },
            outargs = 9,
        };

        public class GetMediaInfo_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NrTracks;
			public string MediaDuration;
			public string CurrentURI;
			public string CurrentURIMetaData;
			public string NextURI;
			public string NextURIMetaData;
			public string PlayMedium;
			public string RecordMedium;
			public string WriteStatus;


            public override void Fill(string[] rawdata)
            {
				NrTracks = uint.Parse(rawdata[0]);
				MediaDuration = rawdata[1];
				CurrentURI = rawdata[2];
				CurrentURIMetaData = rawdata[3];
				NextURI = rawdata[4];
				NextURIMetaData = rawdata[5];
				PlayMedium = rawdata[6];
				RecordMedium = rawdata[7];
				WriteStatus = rawdata[8];

            }
        }
        public async Task<GetMediaInfo_Result> GetMediaInfo(uint InstanceID)
        {
            return await base.Action_Async(GetMediaInfo_Info, new object[] { InstanceID }, new GetMediaInfo_Result()) as GetMediaInfo_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTransportInfo_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTransportInfo",
            argnames = new string[] { "InstanceID" },
            outargs = 3,
        };

        public class GetTransportInfo_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentTransportState;
			public string CurrentTransportStatus;
			public string CurrentSpeed;


            public override void Fill(string[] rawdata)
            {
				CurrentTransportState = rawdata[0];
				CurrentTransportStatus = rawdata[1];
				CurrentSpeed = rawdata[2];

            }
        }
        public async Task<GetTransportInfo_Result> GetTransportInfo(uint InstanceID)
        {
            return await base.Action_Async(GetTransportInfo_Info, new object[] { InstanceID }, new GetTransportInfo_Result()) as GetTransportInfo_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetPositionInfo_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetPositionInfo",
            argnames = new string[] { "InstanceID" },
            outargs = 8,
        };

        public class GetPositionInfo_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint Track;
			public string TrackDuration;
			public string TrackMetaData;
			public string TrackURI;
			public string RelTime;
			public string AbsTime;
			public int RelCount;
			public int AbsCount;


            public override void Fill(string[] rawdata)
            {
				Track = uint.Parse(rawdata[0]);
				TrackDuration = rawdata[1];
				TrackMetaData = rawdata[2];
				TrackURI = rawdata[3];
				RelTime = rawdata[4];
				AbsTime = rawdata[5];
				RelCount = int.Parse(rawdata[6]);
				AbsCount = int.Parse(rawdata[7]);

            }
        }
        public async Task<GetPositionInfo_Result> GetPositionInfo(uint InstanceID)
        {
            return await base.Action_Async(GetPositionInfo_Info, new object[] { InstanceID }, new GetPositionInfo_Result()) as GetPositionInfo_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetDeviceCapabilities_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetDeviceCapabilities",
            argnames = new string[] { "InstanceID" },
            outargs = 3,
        };

        public class GetDeviceCapabilities_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string PlayMedia;
			public string RecMedia;
			public string RecQualityModes;


            public override void Fill(string[] rawdata)
            {
				PlayMedia = rawdata[0];
				RecMedia = rawdata[1];
				RecQualityModes = rawdata[2];

            }
        }
        public async Task<GetDeviceCapabilities_Result> GetDeviceCapabilities(uint InstanceID)
        {
            return await base.Action_Async(GetDeviceCapabilities_Info, new object[] { InstanceID }, new GetDeviceCapabilities_Result()) as GetDeviceCapabilities_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetTransportSettings_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetTransportSettings",
            argnames = new string[] { "InstanceID" },
            outargs = 2,
        };

        public class GetTransportSettings_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string PlayMode;
			public string RecQualityMode;


            public override void Fill(string[] rawdata)
            {
				PlayMode = rawdata[0];
				RecQualityMode = rawdata[1];

            }
        }
        public async Task<GetTransportSettings_Result> GetTransportSettings(uint InstanceID)
        {
            return await base.Action_Async(GetTransportSettings_Info, new object[] { InstanceID }, new GetTransportSettings_Result()) as GetTransportSettings_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetCrossfadeMode_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetCrossfadeMode",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetCrossfadeMode_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool CrossfadeMode;


            public override void Fill(string[] rawdata)
            {
				CrossfadeMode = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetCrossfadeMode_Result> GetCrossfadeMode(uint InstanceID)
        {
            return await base.Action_Async(GetCrossfadeMode_Info, new object[] { InstanceID }, new GetCrossfadeMode_Result()) as GetCrossfadeMode_Result;
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
    


        private static OpenPhonos.UPnP.Service.ActionInfo Seek_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Seek",
            argnames = new string[] { "InstanceID", "Unit", "Target" },
            outargs = 0,
        };

        public class Seek_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Seek_Result> Seek(uint InstanceID, string Unit, string Target)
        {
            return await base.Action_Async(Seek_Info, new object[] { InstanceID, Unit, Target }, new Seek_Result()) as Seek_Result;
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
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetPlayMode_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetPlayMode",
            argnames = new string[] { "InstanceID", "NewPlayMode" },
            outargs = 0,
        };

        public class SetPlayMode_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetPlayMode_Result> SetPlayMode(uint InstanceID, string NewPlayMode)
        {
            return await base.Action_Async(SetPlayMode_Info, new object[] { InstanceID, NewPlayMode }, new SetPlayMode_Result()) as SetPlayMode_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetCrossfadeMode_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetCrossfadeMode",
            argnames = new string[] { "InstanceID", "CrossfadeMode" },
            outargs = 0,
        };

        public class SetCrossfadeMode_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetCrossfadeMode_Result> SetCrossfadeMode(uint InstanceID, bool CrossfadeMode)
        {
            return await base.Action_Async(SetCrossfadeMode_Info, new object[] { InstanceID, CrossfadeMode }, new SetCrossfadeMode_Result()) as SetCrossfadeMode_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo NotifyDeletedURI_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "NotifyDeletedURI",
            argnames = new string[] { "InstanceID", "DeletedURI" },
            outargs = 0,
        };

        public class NotifyDeletedURI_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<NotifyDeletedURI_Result> NotifyDeletedURI(uint InstanceID, string DeletedURI)
        {
            return await base.Action_Async(NotifyDeletedURI_Info, new object[] { InstanceID, DeletedURI }, new NotifyDeletedURI_Result()) as NotifyDeletedURI_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetCurrentTransportActions_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetCurrentTransportActions",
            argnames = new string[] { "InstanceID" },
            outargs = 1,
        };

        public class GetCurrentTransportActions_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string Actions;


            public override void Fill(string[] rawdata)
            {
				Actions = rawdata[0];

            }
        }
        public async Task<GetCurrentTransportActions_Result> GetCurrentTransportActions(uint InstanceID)
        {
            return await base.Action_Async(GetCurrentTransportActions_Info, new object[] { InstanceID }, new GetCurrentTransportActions_Result()) as GetCurrentTransportActions_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo BecomeCoordinatorOfStandaloneGroup_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "BecomeCoordinatorOfStandaloneGroup",
            argnames = new string[] { "InstanceID" },
            outargs = 2,
        };

        public class BecomeCoordinatorOfStandaloneGroup_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string DelegatedGroupCoordinatorID;
			public string NewGroupID;


            public override void Fill(string[] rawdata)
            {
				DelegatedGroupCoordinatorID = rawdata[0];
				NewGroupID = rawdata[1];

            }
        }
        public async Task<BecomeCoordinatorOfStandaloneGroup_Result> BecomeCoordinatorOfStandaloneGroup(uint InstanceID)
        {
            return await base.Action_Async(BecomeCoordinatorOfStandaloneGroup_Info, new object[] { InstanceID }, new BecomeCoordinatorOfStandaloneGroup_Result()) as BecomeCoordinatorOfStandaloneGroup_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo DelegateGroupCoordinationTo_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "DelegateGroupCoordinationTo",
            argnames = new string[] { "InstanceID", "NewCoordinator", "RejoinGroup" },
            outargs = 0,
        };

        public class DelegateGroupCoordinationTo_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<DelegateGroupCoordinationTo_Result> DelegateGroupCoordinationTo(uint InstanceID, string NewCoordinator, bool RejoinGroup)
        {
            return await base.Action_Async(DelegateGroupCoordinationTo_Info, new object[] { InstanceID, NewCoordinator, RejoinGroup }, new DelegateGroupCoordinationTo_Result()) as DelegateGroupCoordinationTo_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo BecomeGroupCoordinator_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "BecomeGroupCoordinator",
            argnames = new string[] { "InstanceID", "CurrentCoordinator", "CurrentGroupID", "OtherMembers", "TransportSettings", "CurrentURI", "CurrentURIMetaData", "SleepTimerState", "AlarmState", "StreamRestartState", "CurrentQueueTrackList", "CurrentVLIState" },
            outargs = 0,
        };

        public class BecomeGroupCoordinator_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<BecomeGroupCoordinator_Result> BecomeGroupCoordinator(uint InstanceID, string CurrentCoordinator, string CurrentGroupID, string OtherMembers, string TransportSettings, string CurrentURI, string CurrentURIMetaData, string SleepTimerState, string AlarmState, string StreamRestartState, string CurrentQueueTrackList, string CurrentVLIState)
        {
            return await base.Action_Async(BecomeGroupCoordinator_Info, new object[] { InstanceID, CurrentCoordinator, CurrentGroupID, OtherMembers, TransportSettings, CurrentURI, CurrentURIMetaData, SleepTimerState, AlarmState, StreamRestartState, CurrentQueueTrackList, CurrentVLIState }, new BecomeGroupCoordinator_Result()) as BecomeGroupCoordinator_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo BecomeGroupCoordinatorAndSource_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "BecomeGroupCoordinatorAndSource",
            argnames = new string[] { "InstanceID", "CurrentCoordinator", "CurrentGroupID", "OtherMembers", "CurrentURI", "CurrentURIMetaData", "SleepTimerState", "AlarmState", "StreamRestartState", "CurrentAVTTrackList", "CurrentQueueTrackList", "CurrentSourceState", "ResumePlayback" },
            outargs = 0,
        };

        public class BecomeGroupCoordinatorAndSource_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<BecomeGroupCoordinatorAndSource_Result> BecomeGroupCoordinatorAndSource(uint InstanceID, string CurrentCoordinator, string CurrentGroupID, string OtherMembers, string CurrentURI, string CurrentURIMetaData, string SleepTimerState, string AlarmState, string StreamRestartState, string CurrentAVTTrackList, string CurrentQueueTrackList, string CurrentSourceState, bool ResumePlayback)
        {
            return await base.Action_Async(BecomeGroupCoordinatorAndSource_Info, new object[] { InstanceID, CurrentCoordinator, CurrentGroupID, OtherMembers, CurrentURI, CurrentURIMetaData, SleepTimerState, AlarmState, StreamRestartState, CurrentAVTTrackList, CurrentQueueTrackList, CurrentSourceState, ResumePlayback }, new BecomeGroupCoordinatorAndSource_Result()) as BecomeGroupCoordinatorAndSource_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ChangeCoordinator_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ChangeCoordinator",
            argnames = new string[] { "InstanceID", "CurrentCoordinator", "NewCoordinator", "NewTransportSettings", "CurrentAVTransportURI" },
            outargs = 0,
        };

        public class ChangeCoordinator_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ChangeCoordinator_Result> ChangeCoordinator(uint InstanceID, string CurrentCoordinator, string NewCoordinator, string NewTransportSettings, string CurrentAVTransportURI)
        {
            return await base.Action_Async(ChangeCoordinator_Info, new object[] { InstanceID, CurrentCoordinator, NewCoordinator, NewTransportSettings, CurrentAVTransportURI }, new ChangeCoordinator_Result()) as ChangeCoordinator_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ChangeTransportSettings_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ChangeTransportSettings",
            argnames = new string[] { "InstanceID", "NewTransportSettings", "CurrentAVTransportURI" },
            outargs = 0,
        };

        public class ChangeTransportSettings_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ChangeTransportSettings_Result> ChangeTransportSettings(uint InstanceID, string NewTransportSettings, string CurrentAVTransportURI)
        {
            return await base.Action_Async(ChangeTransportSettings_Info, new object[] { InstanceID, NewTransportSettings, CurrentAVTransportURI }, new ChangeTransportSettings_Result()) as ChangeTransportSettings_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ConfigureSleepTimer_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ConfigureSleepTimer",
            argnames = new string[] { "InstanceID", "NewSleepTimerDuration" },
            outargs = 0,
        };

        public class ConfigureSleepTimer_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ConfigureSleepTimer_Result> ConfigureSleepTimer(uint InstanceID, string NewSleepTimerDuration)
        {
            return await base.Action_Async(ConfigureSleepTimer_Info, new object[] { InstanceID, NewSleepTimerDuration }, new ConfigureSleepTimer_Result()) as ConfigureSleepTimer_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetRemainingSleepTimerDuration_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetRemainingSleepTimerDuration",
            argnames = new string[] { "InstanceID" },
            outargs = 2,
        };

        public class GetRemainingSleepTimerDuration_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string RemainingSleepTimerDuration;
			public uint CurrentSleepTimerGeneration;


            public override void Fill(string[] rawdata)
            {
				RemainingSleepTimerDuration = rawdata[0];
				CurrentSleepTimerGeneration = uint.Parse(rawdata[1]);

            }
        }
        public async Task<GetRemainingSleepTimerDuration_Result> GetRemainingSleepTimerDuration(uint InstanceID)
        {
            return await base.Action_Async(GetRemainingSleepTimerDuration_Info, new object[] { InstanceID }, new GetRemainingSleepTimerDuration_Result()) as GetRemainingSleepTimerDuration_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RunAlarm_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RunAlarm",
            argnames = new string[] { "InstanceID", "AlarmID", "LoggedStartTime", "Duration", "ProgramURI", "ProgramMetaData", "PlayMode", "Volume", "IncludeLinkedZones" },
            outargs = 0,
        };

        public class RunAlarm_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RunAlarm_Result> RunAlarm(uint InstanceID, uint AlarmID, string LoggedStartTime, string Duration, string ProgramURI, string ProgramMetaData, string PlayMode, ushort Volume, bool IncludeLinkedZones)
        {
            return await base.Action_Async(RunAlarm_Info, new object[] { InstanceID, AlarmID, LoggedStartTime, Duration, ProgramURI, ProgramMetaData, PlayMode, Volume, IncludeLinkedZones }, new RunAlarm_Result()) as RunAlarm_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo StartAutoplay_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "StartAutoplay",
            argnames = new string[] { "InstanceID", "ProgramURI", "ProgramMetaData", "Volume", "IncludeLinkedZones", "ResetVolumeAfter" },
            outargs = 0,
        };

        public class StartAutoplay_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<StartAutoplay_Result> StartAutoplay(uint InstanceID, string ProgramURI, string ProgramMetaData, ushort Volume, bool IncludeLinkedZones, bool ResetVolumeAfter)
        {
            return await base.Action_Async(StartAutoplay_Info, new object[] { InstanceID, ProgramURI, ProgramMetaData, Volume, IncludeLinkedZones, ResetVolumeAfter }, new StartAutoplay_Result()) as StartAutoplay_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetRunningAlarmProperties_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetRunningAlarmProperties",
            argnames = new string[] { "InstanceID" },
            outargs = 3,
        };

        public class GetRunningAlarmProperties_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint AlarmID;
			public string GroupID;
			public string LoggedStartTime;


            public override void Fill(string[] rawdata)
            {
				AlarmID = uint.Parse(rawdata[0]);
				GroupID = rawdata[1];
				LoggedStartTime = rawdata[2];

            }
        }
        public async Task<GetRunningAlarmProperties_Result> GetRunningAlarmProperties(uint InstanceID)
        {
            return await base.Action_Async(GetRunningAlarmProperties_Info, new object[] { InstanceID }, new GetRunningAlarmProperties_Result()) as GetRunningAlarmProperties_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SnoozeAlarm_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SnoozeAlarm",
            argnames = new string[] { "InstanceID", "Duration" },
            outargs = 0,
        };

        public class SnoozeAlarm_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SnoozeAlarm_Result> SnoozeAlarm(uint InstanceID, string Duration)
        {
            return await base.Action_Async(SnoozeAlarm_Info, new object[] { InstanceID, Duration }, new SnoozeAlarm_Result()) as SnoozeAlarm_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo EndDirectControlSession_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "EndDirectControlSession",
            argnames = new string[] { "InstanceID" },
            outargs = 0,
        };

        public class EndDirectControlSession_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<EndDirectControlSession_Result> EndDirectControlSession(uint InstanceID)
        {
            return await base.Action_Async(EndDirectControlSession_Info, new object[] { InstanceID }, new EndDirectControlSession_Result()) as EndDirectControlSession_Result;
        }
    

    }
}

