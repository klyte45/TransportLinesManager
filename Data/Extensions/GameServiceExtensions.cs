using Klyte.TransportLinesManager.Xml;
using static ItemClass;

namespace Klyte.TransportLinesManager.Extensions
{
    internal static class GameServiceExtensions
    {
        public static TLMSpecialNamingClass GetNamingClass(this DistrictPark park) =>
              park.IsCampus ? TLMSpecialNamingClass.Campus
            : park.IsIndustry ? TLMSpecialNamingClass.Industrial
            : TLMSpecialNamingClass.ParkArea;
        public static TLMAutoNameConfigurationData<Service> GetConfig(this Service service) => TLMBaseConfigXML.Instance.GetAutoNameData(service);

    }
}
