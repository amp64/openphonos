
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace SonosServices
{
    public class QPlay1 : OpenPhonos.UPnP.Service
    {
        public QPlay1(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }



        private static OpenPhonos.UPnP.Service.ActionInfo QPlayAuth_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = "QPlayAuth",
            argnames = new string[] { "Seed" },
            outargs = 3,
        };

        public class QPlayAuth_Result : OpenPhonos.UPnP.Service.ActionResult
        {
			public string Code;
			public string MID;
			public string DID;


            public override void Fill(string[] rawdata)
            {
				Code = rawdata[0];
				MID = rawdata[1];
				DID = rawdata[2];

            }
        }
        public async Task<QPlayAuth_Result> QPlayAuth(string Seed)
        {
            return await base.Action_Async(QPlayAuth_Info, new object[] { Seed }, new QPlayAuth_Result()) as QPlayAuth_Result;
        }
    

    }
}

