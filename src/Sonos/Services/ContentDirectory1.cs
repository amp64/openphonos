
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class ContentDirectory1 : OpenPhonos.UPnP.Service
    {
        public ContentDirectory1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo GetSearchCapabilities_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetSearchCapabilities",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetSearchCapabilities_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string SearchCaps;


            public override void Fill(string[] rawdata)
            {
				SearchCaps = rawdata[0];

            }
        }
        public async Task<GetSearchCapabilities_Result> GetSearchCapabilities()
        {
            return await base.Action_Async(GetSearchCapabilities_Info, new object[] {  }, new GetSearchCapabilities_Result()) as GetSearchCapabilities_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetSortCapabilities_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetSortCapabilities",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetSortCapabilities_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string SortCaps;


            public override void Fill(string[] rawdata)
            {
				SortCaps = rawdata[0];

            }
        }
        public async Task<GetSortCapabilities_Result> GetSortCapabilities()
        {
            return await base.Action_Async(GetSortCapabilities_Info, new object[] {  }, new GetSortCapabilities_Result()) as GetSortCapabilities_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetSystemUpdateID_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetSystemUpdateID",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetSystemUpdateID_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint Id;


            public override void Fill(string[] rawdata)
            {
				Id = uint.Parse(rawdata[0]);

            }
        }
        public async Task<GetSystemUpdateID_Result> GetSystemUpdateID()
        {
            return await base.Action_Async(GetSystemUpdateID_Info, new object[] {  }, new GetSystemUpdateID_Result()) as GetSystemUpdateID_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetAlbumArtistDisplayOption_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetAlbumArtistDisplayOption",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetAlbumArtistDisplayOption_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string AlbumArtistDisplayOption;


            public override void Fill(string[] rawdata)
            {
				AlbumArtistDisplayOption = rawdata[0];

            }
        }
        public async Task<GetAlbumArtistDisplayOption_Result> GetAlbumArtistDisplayOption()
        {
            return await base.Action_Async(GetAlbumArtistDisplayOption_Info, new object[] {  }, new GetAlbumArtistDisplayOption_Result()) as GetAlbumArtistDisplayOption_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetLastIndexChange_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetLastIndexChange",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetLastIndexChange_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string LastIndexChange;


            public override void Fill(string[] rawdata)
            {
				LastIndexChange = rawdata[0];

            }
        }
        public async Task<GetLastIndexChange_Result> GetLastIndexChange()
        {
            return await base.Action_Async(GetLastIndexChange_Info, new object[] {  }, new GetLastIndexChange_Result()) as GetLastIndexChange_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo Browse_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "Browse",
            argnames = new string[] { "ObjectID", "BrowseFlag", "Filter", "StartingIndex", "RequestedCount", "SortCriteria" },
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
        public async Task<Browse_Result> Browse(string ObjectID, string BrowseFlag, string Filter, uint StartingIndex, uint RequestedCount, string SortCriteria)
        {
            return await base.Action_Async(Browse_Info, new object[] { ObjectID, BrowseFlag, Filter, StartingIndex, RequestedCount, SortCriteria }, new Browse_Result()) as Browse_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo FindPrefix_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "FindPrefix",
            argnames = new string[] { "ObjectID", "Prefix" },
            outargs = 2,
        };

        public class FindPrefix_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint StartingIndex;
			public uint UpdateID;


            public override void Fill(string[] rawdata)
            {
				StartingIndex = uint.Parse(rawdata[0]);
				UpdateID = uint.Parse(rawdata[1]);

            }
        }
        public async Task<FindPrefix_Result> FindPrefix(string ObjectID, string Prefix)
        {
            return await base.Action_Async(FindPrefix_Info, new object[] { ObjectID, Prefix }, new FindPrefix_Result()) as FindPrefix_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetAllPrefixLocations_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetAllPrefixLocations",
            argnames = new string[] { "ObjectID" },
            outargs = 3,
        };

        public class GetAllPrefixLocations_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public uint TotalPrefixes;
			public string PrefixAndIndexCSV;
			public uint UpdateID;


            public override void Fill(string[] rawdata)
            {
				TotalPrefixes = uint.Parse(rawdata[0]);
				PrefixAndIndexCSV = rawdata[1];
				UpdateID = uint.Parse(rawdata[2]);

            }
        }
        public async Task<GetAllPrefixLocations_Result> GetAllPrefixLocations(string ObjectID)
        {
            return await base.Action_Async(GetAllPrefixLocations_Info, new object[] { ObjectID }, new GetAllPrefixLocations_Result()) as GetAllPrefixLocations_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo CreateObject_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "CreateObject",
            argnames = new string[] { "ContainerID", "Elements" },
            outargs = 2,
        };

        public class CreateObject_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string ObjectID;
			public string Result;


            public override void Fill(string[] rawdata)
            {
				ObjectID = rawdata[0];
				Result = rawdata[1];

            }
        }
        public async Task<CreateObject_Result> CreateObject(string ContainerID, string Elements)
        {
            return await base.Action_Async(CreateObject_Info, new object[] { ContainerID, Elements }, new CreateObject_Result()) as CreateObject_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo UpdateObject_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "UpdateObject",
            argnames = new string[] { "ObjectID", "CurrentTagValue", "NewTagValue" },
            outargs = 0,
        };

        public class UpdateObject_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<UpdateObject_Result> UpdateObject(string ObjectID, string CurrentTagValue, string NewTagValue)
        {
            return await base.Action_Async(UpdateObject_Info, new object[] { ObjectID, CurrentTagValue, NewTagValue }, new UpdateObject_Result()) as UpdateObject_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo DestroyObject_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "DestroyObject",
            argnames = new string[] { "ObjectID" },
            outargs = 0,
        };

        public class DestroyObject_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<DestroyObject_Result> DestroyObject(string ObjectID)
        {
            return await base.Action_Async(DestroyObject_Info, new object[] { ObjectID }, new DestroyObject_Result()) as DestroyObject_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RefreshShareIndex_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RefreshShareIndex",
            argnames = new string[] { "AlbumArtistDisplayOption" },
            outargs = 0,
        };

        public class RefreshShareIndex_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RefreshShareIndex_Result> RefreshShareIndex(string AlbumArtistDisplayOption)
        {
            return await base.Action_Async(RefreshShareIndex_Info, new object[] { AlbumArtistDisplayOption }, new RefreshShareIndex_Result()) as RefreshShareIndex_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RequestResort_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RequestResort",
            argnames = new string[] { "SortOrder" },
            outargs = 0,
        };

        public class RequestResort_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RequestResort_Result> RequestResort(string SortOrder)
        {
            return await base.Action_Async(RequestResort_Info, new object[] { SortOrder }, new RequestResort_Result()) as RequestResort_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetShareIndexInProgress_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetShareIndexInProgress",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetShareIndexInProgress_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool IsIndexing;


            public override void Fill(string[] rawdata)
            {
				IsIndexing = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetShareIndexInProgress_Result> GetShareIndexInProgress()
        {
            return await base.Action_Async(GetShareIndexInProgress_Info, new object[] {  }, new GetShareIndexInProgress_Result()) as GetShareIndexInProgress_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo GetBrowseable_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "GetBrowseable",
            argnames = new string[] {  },
            outargs = 1,
        };

        public class GetBrowseable_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public bool IsBrowseable;


            public override void Fill(string[] rawdata)
            {
				IsBrowseable = ParseBool(rawdata[0]);

            }
        }
        public async Task<GetBrowseable_Result> GetBrowseable()
        {
            return await base.Action_Async(GetBrowseable_Info, new object[] {  }, new GetBrowseable_Result()) as GetBrowseable_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetBrowseable_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetBrowseable",
            argnames = new string[] { "Browseable" },
            outargs = 0,
        };

        public class SetBrowseable_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetBrowseable_Result> SetBrowseable(bool Browseable)
        {
            return await base.Action_Async(SetBrowseable_Info, new object[] { Browseable }, new SetBrowseable_Result()) as SetBrowseable_Result;
        }
    

    }
}

