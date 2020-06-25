using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    internal static class NamingTypeExtensions
    {
        public static int GetNamePrecedenceRate(this NamingType namingType)
        {
            switch (namingType)
            {
                case NamingType.NONE:
                    return 0x7FFFFFFF;
                case NamingType.PLANE:
                    return -0x00000005;
                case NamingType.BLIMP:
                    return 0x00000001;
                case NamingType.SHIP:
                    return -0x00000002;
                case NamingType.FERRY:
                    return 0x00000001;
                case NamingType.TRAIN:
                    return 0x00000003;
                case NamingType.MONORAIL:
                    return 0x00000004;
                case NamingType.TRAM:
                    return 0x00000006;
                case NamingType.METRO:
                    return 0x00000005;
                case NamingType.BUS:
                    return 0x00000007;
                case NamingType.TOUR_BUS:
                    return 0x00000009;
                case NamingType.MONUMENT:
                    return 0x00000005;
                case NamingType.CAMPUS:
                    return 0x00000005;
                case NamingType.BEAUTIFICATION:
                    return 0x0000000a;
                case NamingType.HEALTHCARE:
                    return 0x0000000b;
                case NamingType.POLICEDEPARTMENT:
                    return 0x0000000b;
                case NamingType.FIREDEPARTMENT:
                    return 0x0000000b;
                case NamingType.EDUCATION:
                    return 0x0000000c;
                case NamingType.DISASTER:
                    return 0x0000000d;
                case NamingType.GARBAGE:
                    return 0x0000000f;
                case NamingType.PARKAREA:
                    return 0x00000005;
                case NamingType.DISTRICT:
                    return 0x00000010;
                case NamingType.INDUSTRY_AREA:
                    return 0x00000010;
                case NamingType.ADDRESS:
                    return 0x00000011;
                case NamingType.RICO:
                    return 0x000000e;
                case NamingType.CABLE_CAR:
                    return 0x00000004;
                case NamingType.TROLLEY:
                    return 0x00000006;
                case NamingType.HELICOPTER:
                    return 0x00000001;
                default:
                    return 0x7FFFFFFF;
            }
        }

        public static NamingType From(ItemClass.Service service, ItemClass.SubService subService) => From(GameServiceExtensions.ToConfigIndex(service, subService));
        public static NamingType From(TLMCW.ConfigIndex ci)
        {
            switch (ci & (TLMCW.ConfigIndex.SYSTEM_PART | TLMCW.ConfigIndex.DESC_DATA))
            {
                case TLMCW.ConfigIndex.PLANE_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.PLANE;
                case TLMCW.ConfigIndex.BLIMP_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.BLIMP;
                case TLMCW.ConfigIndex.SHIP_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.SHIP;
                case TLMCW.ConfigIndex.FERRY_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.FERRY;
                case TLMCW.ConfigIndex.TRAIN_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.TRAIN;
                case TLMCW.ConfigIndex.MONORAIL_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.MONORAIL;
                case TLMCW.ConfigIndex.TRAM_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.TRAM;
                case TLMCW.ConfigIndex.METRO_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.METRO;
                case TLMCW.ConfigIndex.BUS_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.BUS;
                case TLMCW.ConfigIndex.TOUR_BUS_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.TOUR_BUS;
                case TLMCW.ConfigIndex.CABLE_CAR_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.CABLE_CAR;
                case TLMCW.ConfigIndex.MONUMENT_SERVICE_CONFIG:
                    return NamingType.MONUMENT;
                case TLMCW.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                    return NamingType.BEAUTIFICATION;
                case TLMCW.ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                    return NamingType.HEALTHCARE;
                case TLMCW.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                    return NamingType.POLICEDEPARTMENT;
                case TLMCW.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                    return NamingType.FIREDEPARTMENT;
                case TLMCW.ConfigIndex.EDUCATION_SERVICE_CONFIG:
                    return NamingType.EDUCATION;
                case TLMCW.ConfigIndex.DISASTER_SERVICE_CONFIG:
                    return NamingType.DISASTER;
                case TLMCW.ConfigIndex.GARBAGE_SERVICE_CONFIG:
                    return NamingType.GARBAGE;
                case TLMCW.ConfigIndex.PARKAREA_NAME_CONFIG:
                    return NamingType.PARKAREA;
                case TLMCW.ConfigIndex.DISTRICT_NAME_CONFIG:
                    return NamingType.DISTRICT;
                case TLMCW.ConfigIndex.ADDRESS_NAME_CONFIG:
                    return NamingType.ADDRESS;
                case TLMCW.ConfigIndex.VARSITY_SPORTS_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.MUSEUMS_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.PLAYER_EDUCATION_SERVICE_CONFIG:
                    return NamingType.CAMPUS;
                case TLMCW.ConfigIndex.PLAYER_INDUSTRY_SERVICE_CONFIG:
                    return NamingType.INDUSTRY_AREA;
                case TLMCW.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.OFFICE_SERVICE_CONFIG:
                    return NamingType.RICO;
                case TLMCW.ConfigIndex.TROLLEY_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.TROLLEY;
                case TLMCW.ConfigIndex.HELICOPTER_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return NamingType.HELICOPTER;
                default:
                    LogUtils.DoErrorLog($"UNKNOWN NAME TYPE:{ci} ({((int)ci).ToString("X8")})");
                    return NamingType.NONE;

            }
        }
    }

}
