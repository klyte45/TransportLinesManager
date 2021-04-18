using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.Utils;

namespace Klyte.TransportLinesManager.Xml
{
    public enum TLMSpecialNamingClass
    {
        None,
        Campus,
        Industrial,
        ParkArea,
        District,
        Address
    }

    public static class TLMSpecialNamingClassExtensions
    {
        public static TLMAutoNameConfigurationData<TLMSpecialNamingClass> GetConfig(this TLMSpecialNamingClass clazz) => TLMBaseConfigXML.Instance.GetAutoNameData(clazz);

        public static NamingType ToNamingType(this TLMSpecialNamingClass clazz)
        {
            switch (clazz)
            {
                case TLMSpecialNamingClass.Campus:
                    return NamingType.CAMPUS;
                case TLMSpecialNamingClass.Industrial:
                    return NamingType.INDUSTRY_AREA;
                case TLMSpecialNamingClass.ParkArea:
                    return NamingType.PARKAREA;
                case TLMSpecialNamingClass.District:
                    return NamingType.DISTRICT;
                case TLMSpecialNamingClass.Address:
                    return NamingType.ADDRESS;
                default:
                    return 0;
            }
        }

        public static string GetLocalizedName(this TLMSpecialNamingClass clazz) => Locale.Get("K45_TLM_SPECIALNAMINGCLASS", clazz.ToString());
    }
}
