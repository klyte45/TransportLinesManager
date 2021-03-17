using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;

namespace Klyte.TransportLinesManager.Extensors
{
    internal static class GameServiceExtensions
    {
        public static TLMConfigWarehouse.ConfigIndex ToConfigIndex(this DistrictPark park)
        {
            if (park.IsCampus)
            {
                return TLMConfigWarehouse.ConfigIndex.CAMPUS_AREA_NAME_CONFIG;
            }
            if (park.IsIndustry)
            {
                return TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_AREA_NAME_CONFIG;
            }
            return TLMConfigWarehouse.ConfigIndex.PARKAREA_NAME_CONFIG;
        }

        public static TLMConfigWarehouse.ConfigIndex ToConfigIndex(ItemClass.Service s, ItemClass.SubService ss)
        {
            switch (s)
            {
                case ItemClass.Service.Residential:
                    return TLMConfigWarehouse.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG;
                case ItemClass.Service.Commercial:
                    return TLMConfigWarehouse.ConfigIndex.COMMERCIAL_SERVICE_CONFIG;
                case ItemClass.Service.Industrial:
                    return TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG;
                case ItemClass.Service.Natural:
                    return TLMConfigWarehouse.ConfigIndex.NATURAL_SERVICE_CONFIG;
                //case ItemClass.Service.Unused2:
                //    return TLMConfigWarehouse.ConfigIndex.UNUSED2_SERVICE_CONFIG;
                case ItemClass.Service.Citizen:
                    return TLMConfigWarehouse.ConfigIndex.CITIZEN_SERVICE_CONFIG;
                case ItemClass.Service.Tourism:
                    return TLMConfigWarehouse.ConfigIndex.TOURISM_SERVICE_CONFIG;
                case ItemClass.Service.Office:
                    return TLMConfigWarehouse.ConfigIndex.OFFICE_SERVICE_CONFIG;
                case ItemClass.Service.Road:
                    if (ss == ItemClass.SubService.PublicTransportBus)
                    {
                        return TLMConfigWarehouse.ConfigIndex.ADDRESS_NAME_CONFIG;
                    }
                    else
                    {
                        return TLMConfigWarehouse.ConfigIndex.ROAD_SERVICE_CONFIG;
                    }

                case ItemClass.Service.Electricity:
                    return TLMConfigWarehouse.ConfigIndex.ELECTRICITY_SERVICE_CONFIG;
                case ItemClass.Service.Water:
                    return TLMConfigWarehouse.ConfigIndex.WATER_SERVICE_CONFIG;
                case ItemClass.Service.Beautification:
                    return TLMConfigWarehouse.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG;
                case ItemClass.Service.Garbage:
                    return TLMConfigWarehouse.ConfigIndex.GARBAGE_SERVICE_CONFIG;
                case ItemClass.Service.HealthCare:
                    return TLMConfigWarehouse.ConfigIndex.HEALTHCARE_SERVICE_CONFIG;
                case ItemClass.Service.PoliceDepartment:
                    return TLMConfigWarehouse.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG;
                case ItemClass.Service.Education:
                    return TLMConfigWarehouse.ConfigIndex.EDUCATION_SERVICE_CONFIG;
                case ItemClass.Service.Monument:
                    return TLMConfigWarehouse.ConfigIndex.MONUMENT_SERVICE_CONFIG;
                case ItemClass.Service.FireDepartment:
                    return TLMConfigWarehouse.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG;
                case ItemClass.Service.PublicTransport:
                    switch (ss)
                    {
                        case ItemClass.SubService.PublicTransportBus: return TLMConfigWarehouse.ConfigIndex.BUS_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportCableCar: return TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportMetro: return TLMConfigWarehouse.ConfigIndex.METRO_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportMonorail: return TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportPlane: return TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportShip: return TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportTaxi: return TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportTours: return TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportTrain: return TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportTram: return TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        case ItemClass.SubService.PublicTransportTrolleybus: return TLMConfigWarehouse.ConfigIndex.TROLLEY_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                        default: return TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG;
                    }
                case ItemClass.Service.Disaster:
                    return TLMConfigWarehouse.ConfigIndex.DISASTER_SERVICE_CONFIG;
                case ItemClass.Service.PlayerIndustry:
                    return TLMConfigWarehouse.ConfigIndex.PLAYER_INDUSTRY_SERVICE_CONFIG;
                case ItemClass.Service.PlayerEducation:
                    return TLMConfigWarehouse.ConfigIndex.PLAYER_EDUCATION_SERVICE_CONFIG;
                case ItemClass.Service.Museums:
                    return TLMConfigWarehouse.ConfigIndex.MUSEUMS_SERVICE_CONFIG;
                case ItemClass.Service.VarsitySports:
                    return TLMConfigWarehouse.ConfigIndex.VARSITY_SPORTS_SERVICE_CONFIG;
                default:
                    return 0;
            }
        }

        public static ItemClass.Service ToServiceSubservice(this TLMConfigWarehouse.ConfigIndex cfg, out ItemClass.SubService ss)
        {
            ss = ItemClass.SubService.None;
            switch (cfg)
            {
                case TLMConfigWarehouse.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                    return ItemClass.Service.Residential;

                case TLMConfigWarehouse.ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                    return ItemClass.Service.Commercial;

                case TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                    return ItemClass.Service.Industrial;
                case TLMConfigWarehouse.ConfigIndex.NATURAL_SERVICE_CONFIG:
                    return ItemClass.Service.Natural;
                case TLMConfigWarehouse.ConfigIndex.CITIZEN_SERVICE_CONFIG:
                    return ItemClass.Service.Citizen;
                case TLMConfigWarehouse.ConfigIndex.TOURISM_SERVICE_CONFIG:
                    return ItemClass.Service.Tourism;
                case TLMConfigWarehouse.ConfigIndex.OFFICE_SERVICE_CONFIG:
                    return ItemClass.Service.Office;
                case TLMConfigWarehouse.ConfigIndex.ADDRESS_NAME_CONFIG:
                    ss = ItemClass.SubService.PublicTransportBus;
                    return ItemClass.Service.Road;
                case TLMConfigWarehouse.ConfigIndex.ROAD_SERVICE_CONFIG:
                    return ItemClass.Service.Road;

                case TLMConfigWarehouse.ConfigIndex.ELECTRICITY_SERVICE_CONFIG:
                    return ItemClass.Service.Electricity;
                case TLMConfigWarehouse.ConfigIndex.WATER_SERVICE_CONFIG:
                    return ItemClass.Service.Water;
                case TLMConfigWarehouse.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                    return ItemClass.Service.Beautification;
                case TLMConfigWarehouse.ConfigIndex.GARBAGE_SERVICE_CONFIG:
                    return ItemClass.Service.Garbage;
                case TLMConfigWarehouse.ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                    return ItemClass.Service.HealthCare;
                case TLMConfigWarehouse.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                    return ItemClass.Service.PoliceDepartment;
                case TLMConfigWarehouse.ConfigIndex.EDUCATION_SERVICE_CONFIG:
                    return ItemClass.Service.Education;
                case TLMConfigWarehouse.ConfigIndex.MONUMENT_SERVICE_CONFIG:
                    return ItemClass.Service.Monument;
                case TLMConfigWarehouse.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                    return ItemClass.Service.FireDepartment;
                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportBus;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportCableCar;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportMetro;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportMonorail;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HELICOPTER_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportPlane;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportShip;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportTaxi;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportTours;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportTrain;
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG | TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    ss = ItemClass.SubService.PublicTransportTram;
                    return ItemClass.Service.PublicTransport;

                case TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    return ItemClass.Service.PublicTransport;
                case TLMConfigWarehouse.ConfigIndex.DISASTER_SERVICE_CONFIG:
                    return ItemClass.Service.Disaster;

                case TLMConfigWarehouse.ConfigIndex.CAMPUS_AREA_NAME_CONFIG:
                    return ItemClass.Service.PlayerEducation;
                case TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_AREA_NAME_CONFIG:
                    return ItemClass.Service.PlayerIndustry;
                case TLMConfigWarehouse.ConfigIndex.PARKAREA_NAME_CONFIG:
                    ss = ItemClass.SubService.BeautificationParks;
                    return ItemClass.Service.Beautification;

                case TLMConfigWarehouse.ConfigIndex.PLAYER_INDUSTRY_SERVICE_CONFIG:
                    return ItemClass.Service.PlayerIndustry;
                case TLMConfigWarehouse.ConfigIndex.PLAYER_EDUCATION_SERVICE_CONFIG:
                    return ItemClass.Service.PlayerEducation;
                case TLMConfigWarehouse.ConfigIndex.MUSEUMS_SERVICE_CONFIG:
                    return ItemClass.Service.Museums;
                case TLMConfigWarehouse.ConfigIndex.VARSITY_SPORTS_SERVICE_CONFIG:
                    return ItemClass.Service.VarsitySports;
                default:
                    return 0;
            }
        }
        public static uint getPriority(this TLMConfigWarehouse.ConfigIndex idx)
        {
            uint saida;
            switch (idx)
            {
                case TLMConfigWarehouse.ConfigIndex.ADDRESS_NAME_CONFIG:
                    return (uint) TLMConfigWarehouse.namingOrder.Length;
                case TLMConfigWarehouse.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.NATURAL_SERVICE_CONFIG:
                //case TLMConfigWarehouse.ConfigIndex.UNUSED2_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CITIZEN_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOURISM_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.OFFICE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ROAD_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ELECTRICITY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.WATER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.GARBAGE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONUMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.DISASTER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_INDUSTRY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MUSEUMS_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.VARSITY_SPORTS_SERVICE_CONFIG:
                    saida = TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | idx) ? (uint) Array.IndexOf(TLMConfigWarehouse.namingOrder, idx) : uint.MaxValue;
                    break;
                case TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG:
                    saida = TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | idx) ? (uint) Array.IndexOf(TLMConfigWarehouse.namingOrder, idx) : uint.MaxValue;
                    break;
                case TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TROLLEY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HELICOPTER_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BALLOON_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG:
                    saida = TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | idx) ? 3 : uint.MaxValue;
                    break;
                default:
                    saida = uint.MaxValue;
                    break;
            }
            LogUtils.DoLog("ConfigIndex.getPriority(): {0} ==> {1}", idx.ToString(), saida);
            return saida;
        }
        public static string GetSystemStationNamePrefix(this TLMConfigWarehouse.ConfigIndex idx, ushort lineId = 0)
        {
            switch (idx)
            {
                case TLMConfigWarehouse.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.NATURAL_SERVICE_CONFIG:
                //case TLMConfigWarehouse.ConfigIndex.UNUSED2_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CITIZEN_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOURISM_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.OFFICE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ROAD_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ELECTRICITY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.WATER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.GARBAGE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONUMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.DISASTER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.DISTRICT_NAME_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ADDRESS_NAME_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PARKAREA_NAME_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_INDUSTRY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MUSEUMS_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.VARSITY_SPORTS_SERVICE_CONFIG:
                    return TLMConfigWarehouse.GetCurrentConfigString(TLMConfigWarehouse.ConfigIndex.AUTO_NAMING_REF_TEXT | idx);
                case TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TROLLEY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HELICOPTER_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BALLOON_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG:
                    return TLMConfigWarehouse.GetCurrentConfigString(TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT | idx);
                default:
                    return "";
            }

        }
        public static bool isLineNamingEnabled(this TLMConfigWarehouse.ConfigIndex idx)
        {
            switch (idx)
            {
                case TLMConfigWarehouse.ConfigIndex.ADDRESS_NAME_CONFIG:
                    return true;
                case TLMConfigWarehouse.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.NATURAL_SERVICE_CONFIG:
                //case TLMConfigWarehouse.ConfigIndex.UNUSED2_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CITIZEN_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOURISM_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.OFFICE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ROAD_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ELECTRICITY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.WATER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.GARBAGE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONUMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.DISASTER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_INDUSTRY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MUSEUMS_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.VARSITY_SPORTS_SERVICE_CONFIG:
                    return TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | idx);
                case TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TROLLEY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HELICOPTER_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BALLOON_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG:
                    return TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | idx);
                default:
                    return false;
            }
        }
        public static bool isPublicTransport(this TLMConfigWarehouse.ConfigIndex idx)
        {
            switch (idx)
            {
                case TLMConfigWarehouse.ConfigIndex.ADDRESS_NAME_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.NATURAL_SERVICE_CONFIG:
                //case TLMConfigWarehouse.ConfigIndex.UNUSED2_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CITIZEN_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOURISM_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.OFFICE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ROAD_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.ELECTRICITY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.WATER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.GARBAGE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONUMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.DISASTER_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_INDUSTRY_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.PLAYER_EDUCATION_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MUSEUMS_SERVICE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.VARSITY_SPORTS_SERVICE_CONFIG:
                    return false;
                case TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TROLLEY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.HELICOPTER_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.BALLOON_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG:
                case TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG:
                    return true;
                default:
                    return false;
            }
        }
    }
}
