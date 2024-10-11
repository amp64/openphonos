
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class Queue1 : OpenPhonos.UPnP.Service
    {
        public Queue1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo AddURI_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddURI",
            argnames = new string[] { "QueueID", "UpdateID", "EnqueuedURI", "EnqueuedURIMetaData", "DesiredFirstTrackNumberEnqueued", "EnqueueAsNext" },
            outargs = 4,
        };

        public class AddURI_Result : OpenPhonos.UPnP.Service.ActionResult
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
        public async Task<AddURI_Result> AddURI(uint QueueID, uint UpdateID, string EnqueuedURI, string EnqueuedURIMetaData, uint DesiredFirstTrackNumberEnqueued, bool EnqueueAsNext)
        {
            return await base.Action_Async(AddURI_Info, new object[] { QueueID, UpdateID, EnqueuedURI, EnqueuedURIMetaData, DesiredFirstTrackNumberEnqueued, EnqueueAsNext }, new AddURI_Result()) as AddURI_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo AddMultipleURIs_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddMultipleURIs",
            argnames = new string[] { "QueueID", "UpdateID", "ContainerURI", "ContainerMetaData", "DesiredFirstTrackNumberEnqueued", "EnqueueAsNext", "NumberOfURIs", "EnqueuedURIsAndMetaData" },
            outargs = 4,
        };

        public class AddMultipleURIs_Result : OpenPhonos.UPnP.Service.ActionResult
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
        public async Task<AddMultipleURIs_Result> AddMultipleURIs(uint QueueID, uint UpdateID, string ContainerURI, string ContainerMetaData, uint DesiredFirstTrackNumberEnqueued, bool EnqueueAsNext, uint NumberOfURIs, string EnqueuedURIsAndMetaData)
        {
            return await base.Action_Async(AddMultipleURIs_Info, new object[] { QueueID, UpdateID, ContainerURI, ContainerMetaData, DesiredFirstTrackNumberEnqueued, EnqueueAsNext, NumberOfURIs, EnqueuedURIsAndMetaData }, new AddMultipleURIs_Result()) as AddMultipleURIs_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo AttachQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AttachQueue",
            argnames = new string[] { "QueueOwnerID" },
            outargs = 2,
        };

        public class AttachQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint QueueID;
			public string QueueOwnerContext;


            public override void Fill(string[] rawdata)
            {
				QueueID = uint.Parse(rawdata[0]);
				QueueOwnerContext = rawdata[1];

            }
        }
        public async Task<AttachQueue_Result> AttachQueue(string QueueOwnerID)
        {
            return await base.Action_Async(AttachQueue_Info, new object[] { QueueOwnerID }, new AttachQueue_Result()) as AttachQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Backup_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Backup",
            argnames = new string[] {  },
            outargs = 0,
        };

        public class Backup_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<Backup_Result> Backup()
        {
            return await base.Action_Async(Backup_Info, new object[] {  }, new Backup_Result()) as Backup_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Browse_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Browse",
            argnames = new string[] { "QueueID", "StartingIndex", "RequestedCount" },
            outargs = 4,
        };

        public class Browse_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string Result;
			public uint NumberReturned;
			public uint TotalMatches;
			public uint UpdateID;


            public override void Fill(string[] rawdata)
            {
				Result = rawdata[0];
				NumberReturned = uint.Parse(rawdata[1]);
				TotalMatches = uint.Parse(rawdata[2]);
				UpdateID = uint.Parse(rawdata[3]);

            }
        }
        public async Task<Browse_Result> Browse(uint QueueID, uint StartingIndex, uint RequestedCount)
        {
            return await base.Action_Async(Browse_Info, new object[] { QueueID, StartingIndex, RequestedCount }, new Browse_Result()) as Browse_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo CreateQueue_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "CreateQueue",
            argnames = new string[] { "QueueOwnerID", "QueueOwnerContext", "QueuePolicy" },
            outargs = 1,
        };

        public class CreateQueue_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint QueueID;


            public override void Fill(string[] rawdata)
            {
				QueueID = uint.Parse(rawdata[0]);

            }
        }
        public async Task<CreateQueue_Result> CreateQueue(string QueueOwnerID, string QueueOwnerContext, string QueuePolicy)
        {
            return await base.Action_Async(CreateQueue_Info, new object[] { QueueOwnerID, QueueOwnerContext, QueuePolicy }, new CreateQueue_Result()) as CreateQueue_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RemoveAllTracks_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveAllTracks",
            argnames = new string[] { "QueueID", "UpdateID" },
            outargs = 1,
        };

        public class RemoveAllTracks_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				NewUpdateID = uint.Parse(rawdata[0]);

            }
        }
        public async Task<RemoveAllTracks_Result> RemoveAllTracks(uint QueueID, uint UpdateID)
        {
            return await base.Action_Async(RemoveAllTracks_Info, new object[] { QueueID, UpdateID }, new RemoveAllTracks_Result()) as RemoveAllTracks_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RemoveTrackRange_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveTrackRange",
            argnames = new string[] { "QueueID", "UpdateID", "StartingIndex", "NumberOfTracks" },
            outargs = 1,
        };

        public class RemoveTrackRange_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				NewUpdateID = uint.Parse(rawdata[0]);

            }
        }
        public async Task<RemoveTrackRange_Result> RemoveTrackRange(uint QueueID, uint UpdateID, uint StartingIndex, uint NumberOfTracks)
        {
            return await base.Action_Async(RemoveTrackRange_Info, new object[] { QueueID, UpdateID, StartingIndex, NumberOfTracks }, new RemoveTrackRange_Result()) as RemoveTrackRange_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReorderTracks_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReorderTracks",
            argnames = new string[] { "QueueID", "StartingIndex", "NumberOfTracks", "InsertBefore", "UpdateID" },
            outargs = 1,
        };

        public class ReorderTracks_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				NewUpdateID = uint.Parse(rawdata[0]);

            }
        }
        public async Task<ReorderTracks_Result> ReorderTracks(uint QueueID, uint StartingIndex, uint NumberOfTracks, uint InsertBefore, uint UpdateID)
        {
            return await base.Action_Async(ReorderTracks_Info, new object[] { QueueID, StartingIndex, NumberOfTracks, InsertBefore, UpdateID }, new ReorderTracks_Result()) as ReorderTracks_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReplaceAllTracks_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReplaceAllTracks",
            argnames = new string[] { "QueueID", "UpdateID", "ContainerURI", "ContainerMetaData", "CurrentTrackIndex", "NewCurrentTrackIndices", "NumberOfURIs", "EnqueuedURIsAndMetaData" },
            outargs = 2,
        };

        public class ReplaceAllTracks_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint NewQueueLength;
			public uint NewUpdateID;


            public override void Fill(string[] rawdata)
            {
				NewQueueLength = uint.Parse(rawdata[0]);
				NewUpdateID = uint.Parse(rawdata[1]);

            }
        }
        public async Task<ReplaceAllTracks_Result> ReplaceAllTracks(uint QueueID, uint UpdateID, string ContainerURI, string ContainerMetaData, uint CurrentTrackIndex, string NewCurrentTrackIndices, uint NumberOfURIs, string EnqueuedURIsAndMetaData)
        {
            return await base.Action_Async(ReplaceAllTracks_Info, new object[] { QueueID, UpdateID, ContainerURI, ContainerMetaData, CurrentTrackIndex, NewCurrentTrackIndices, NumberOfURIs, EnqueuedURIsAndMetaData }, new ReplaceAllTracks_Result()) as ReplaceAllTracks_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SaveAsSonosPlaylist_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SaveAsSonosPlaylist",
            argnames = new string[] { "QueueID", "Title", "ObjectID" },
            outargs = 1,
        };

        public class SaveAsSonosPlaylist_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string AssignedObjectID;


            public override void Fill(string[] rawdata)
            {
				AssignedObjectID = rawdata[0];

            }
        }
        public async Task<SaveAsSonosPlaylist_Result> SaveAsSonosPlaylist(uint QueueID, string Title, string ObjectID)
        {
            return await base.Action_Async(SaveAsSonosPlaylist_Info, new object[] { QueueID, Title, ObjectID }, new SaveAsSonosPlaylist_Result()) as SaveAsSonosPlaylist_Result;
        }
    

    }
}

