using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;


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
                    return 0x00000010;
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
                    return 0x00000010;
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
                case NamingType.TERMINAL:
                    return -0x00000020;
                default:
                    return 0x7FFFFFFF;
            }
        }

        public static NamingType From(TransportSystemDefinition tsd) => tsd == TransportSystemDefinition.PLANE ? NamingType.PLANE
                    : tsd == TransportSystemDefinition.SHIP ? NamingType.SHIP
                    : tsd == TransportSystemDefinition.BLIMP ? NamingType.BLIMP
                    : tsd == TransportSystemDefinition.HELICOPTER ? NamingType.HELICOPTER
                    : tsd == TransportSystemDefinition.TRAIN ? NamingType.TRAIN
                    : tsd == TransportSystemDefinition.FERRY ? NamingType.FERRY
                    : tsd == TransportSystemDefinition.MONORAIL ? NamingType.MONORAIL
                    : tsd == TransportSystemDefinition.METRO ? NamingType.METRO
                    : tsd == TransportSystemDefinition.CABLE_CAR ? NamingType.CABLE_CAR
                    : tsd == TransportSystemDefinition.TROLLEY ? NamingType.TROLLEY
                    : tsd == TransportSystemDefinition.TRAM ? NamingType.TRAM
                    : tsd == TransportSystemDefinition.BUS ? NamingType.BUS
                    : tsd == TransportSystemDefinition.TOUR_BUS ? NamingType.TOUR_BUS
                    : NamingType.NONE;

        public static NamingType From(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.Monument: return NamingType.MONUMENT;
                case ItemClass.Service.Natural:
                case ItemClass.Service.Fishing:
                case ItemClass.Service.Beautification: return NamingType.BEAUTIFICATION;
                case ItemClass.Service.HealthCare: return NamingType.HEALTHCARE;
                case ItemClass.Service.PoliceDepartment: return NamingType.POLICEDEPARTMENT;
                case ItemClass.Service.FireDepartment: return NamingType.FIREDEPARTMENT;
                case ItemClass.Service.Education: return NamingType.EDUCATION;
                case ItemClass.Service.Disaster: return NamingType.DISASTER;
                case ItemClass.Service.Water:
                case ItemClass.Service.Electricity:
                case ItemClass.Service.Garbage: return NamingType.GARBAGE;
                case ItemClass.Service.VarsitySports:
                case ItemClass.Service.PlayerEducation:
                case ItemClass.Service.Museums: return NamingType.CAMPUS;
                case ItemClass.Service.PlayerIndustry: return NamingType.INDUSTRY_AREA;
                case ItemClass.Service.Office:
                case ItemClass.Service.Residential:
                case ItemClass.Service.Industrial:
                case ItemClass.Service.Commercial: return NamingType.RICO;
                default:
                    LogUtils.DoErrorLog($"UNREGISTRED NAMING TYPE:{service}");
                    return NamingType.NONE;

            }
        }
    }

}
