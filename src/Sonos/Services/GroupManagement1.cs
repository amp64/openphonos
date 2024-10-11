
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class GroupManagement1 : OpenPhonos.UPnP.Service
    {
        public GroupManagement1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo AddMember_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "AddMember",
            argnames = new string[] { "MemberID", "BootSeq" },
            outargs = 5,
        };

        public class AddMember_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string CurrentTransportSettings;
			public string CurrentURI;
			public string GroupUUIDJoined;
			public bool ResetVolumeAfter;
			public string VolumeAVTransportURI;


            public override void Fill(string[] rawdata)
            {
				CurrentTransportSettings = rawdata[0];
				CurrentURI = rawdata[1];
				GroupUUIDJoined = rawdata[2];
				ResetVolumeAfter = ParseBool(rawdata[3]);
				VolumeAVTransportURI = rawdata[4];

            }
        }
        public async Task<AddMember_Result> AddMember(string MemberID, uint BootSeq)
        {
            return await base.Action_Async(AddMember_Info, new object[] { MemberID, BootSeq }, new AddMember_Result()) as AddMember_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo RemoveMember_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "RemoveMember",
            argnames = new string[] { "MemberID" },
            outargs = 0,
        };

        public class RemoveMember_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<RemoveMember_Result> RemoveMember(string MemberID)
        {
            return await base.Action_Async(RemoveMember_Info, new object[] { MemberID }, new RemoveMember_Result()) as RemoveMember_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo ReportTrackBufferingResult_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "ReportTrackBufferingResult",
            argnames = new string[] { "MemberID", "ResultCode" },
            outargs = 0,
        };

        public class ReportTrackBufferingResult_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<ReportTrackBufferingResult_Result> ReportTrackBufferingResult(string MemberID, int ResultCode)
        {
            return await base.Action_Async(ReportTrackBufferingResult_Info, new object[] { MemberID, ResultCode }, new ReportTrackBufferingResult_Result()) as ReportTrackBufferingResult_Result;
        }
    


        private static OpenPhonos.UPnP.Service.ActionInfo SetSourceAreaIds_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "SetSourceAreaIds",
            argnames = new string[] { "DesiredSourceAreaIds" },
            outargs = 0,
        };

        public class SetSourceAreaIds_Result : OpenPhonos.UPnP.Service.ActionResult
        {


            public override void Fill(string[] rawdata)
            {

            }
        }
        public async Task<SetSourceAreaIds_Result> SetSourceAreaIds(string DesiredSourceAreaIds)
        {
            return await base.Action_Async(SetSourceAreaIds_Info, new object[] { DesiredSourceAreaIds }, new SetSourceAreaIds_Result()) as SetSourceAreaIds_Result;
        }
    

    }
}

