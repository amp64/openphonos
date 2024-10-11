using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPhonos.UPnP
{
    public class UPnPException : Exception
    {
        public const int UnknownError = 999;         // Outside of values in the spec

        public int ErrorCode
        {
            get;
            private set;
        }

        public string Function
        {
            get;
            private set;
        }

        public string Address
        {
            get;
            private set;
        }

        public override string Message
        {
            get
            {
                switch (ErrorCode)
                {
                    case 501:
                        if (Function == "GetStringX")
                            return "Your account needs to be reauthorized using an official Sonos app";
                        break;
                    case 803:
                        if (Function == "CreateObject")
                            return "Name already exists";
                        break;
                    case 805:
                        return StringResource.Get("FavoritesFull");
                    case 1010:
                        return string.Format(StringResource.Get("Reauth_Official"), string.Empty);
                }
                return string.Format(StringResource.Get("UPNP_Error"), ErrorCode);
            }
        }

        internal UPnPException(string errorcode, string function, string address)
        {
            int code;
            if (errorcode == null || !int.TryParse(errorcode, out code))
            {
                ErrorCode = UnknownError;
            }
            else
            {
                ErrorCode = code;
            }
            Function = function;
            Address = address;
        }
    }
}
