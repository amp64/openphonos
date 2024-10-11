using System.Resources;

namespace OpenPhonos.UPnP
{
    public class StringResource
    {
        public static string Get(string id)
        {
            return UPnP.ResourceManager.GetString(id);
        }
    }
}
