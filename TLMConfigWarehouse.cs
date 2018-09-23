using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    internal class TLMConfigWarehouse : ConfigWarehouseBase<TLMConfigWarehouse.ConfigIndex, TLMConfigWarehouse>
    {
        public const string CONFIG_FILENAME = "CityConfigV10";
        public static readonly string CONFIG_PATH = TLMUtils.BASE_FOLDER_PATH + TLMSingleton.FOLDER_NAME + Path.DirectorySeparatorChar + "CityConfigs" + Path.DirectorySeparatorChar;
        public override string ConfigFilename => CONFIG_FILENAME;
        public override string ConfigPath => CONFIG_PATH;
        public const string TRUE_VALUE = "1";
        public const string FALSE_VALUE = "0";
        public static readonly ConfigIndex[] PALETTES_INDEXES = new ConfigIndex[] {
           ConfigIndex.SHIP_PALETTE_MAIN,
           ConfigIndex.TRAIN_PALETTE_MAIN,
           ConfigIndex.TRAM_PALETTE_MAIN,
           ConfigIndex.METRO_PALETTE_MAIN ,
           ConfigIndex.BUS_PALETTE_MAIN ,
           ConfigIndex.PLANE_PALETTE_MAIN ,
           ConfigIndex.MONORAIL_PALETTE_MAIN ,
           ConfigIndex.TOUR_BUS_CONFIG_PALETTE_MAIN ,
           ConfigIndex.TOUR_PED_CONFIG_PALETTE_MAIN ,
           ConfigIndex.PLANE_PALETTE_SUBLINE,
           ConfigIndex.SHIP_PALETTE_SUBLINE,
           ConfigIndex.TRAIN_PALETTE_SUBLINE,
           ConfigIndex.TRAM_PALETTE_SUBLINE,
           ConfigIndex.METRO_PALETTE_SUBLINE,
           ConfigIndex.BUS_PALETTE_SUBLINE,
           ConfigIndex.MONORAIL_PALETTE_SUBLINE,
           ConfigIndex.TOUR_BUS_CONFIG_PALETTE_SUBLINE,
           ConfigIndex.TOUR_PED_CONFIG_PALETTE_SUBLINE,
        };
        public bool unsafeMode = false;
        public TLMConfigWarehouse() { }



        public static Color32 getColorForTransportType(ConfigIndex i)
        {
            switch (i & ConfigIndex.SYSTEM_PART)
            {
                case ConfigIndex.TRAIN_CONFIG:
                    return new Color32(250, 104, 0, 255);
                case ConfigIndex.TRAM_CONFIG:
                    return new Color32(73, 27, 137, 255);
                case ConfigIndex.METRO_CONFIG:
                    return new Color32(58, 224, 50, 255);
                case ConfigIndex.BUS_CONFIG:
                    return new Color32(53, 121, 188, 255);
                case ConfigIndex.PLANE_CONFIG:
                    return new Color32(0xa8, 0x01, 0x7a, 255);
                case ConfigIndex.SHIP_CONFIG:
                    return new Color32(0xa3, 0xb0, 0, 255);
                case ConfigIndex.BLIMP_CONFIG:
                    return new Color32(0xd8, 0x01, 0xaa, 255);
                case ConfigIndex.FERRY_CONFIG:
                    return new Color32(0xe3, 0xf0, 0, 255);
                case ConfigIndex.MONORAIL_CONFIG:
                    return new Color32(217, 51, 89, 255);
                case ConfigIndex.CABLE_CAR_CONFIG:
                    return new Color32(31, 96, 225, 255);
                case ConfigIndex.TAXI_CONFIG:
                    return new Color32(60, 184, 120, 255);
                case ConfigIndex.EVAC_BUS_CONFIG:
                    return new Color32(202, 162, 31, 255);
                case ConfigIndex.TOUR_BUS_CONFIG:
                    return new Color32(110, 152, 251, 255);
                case ConfigIndex.TOUR_PED_CONFIG:
                    return new Color32(83, 157, 48, 255);
                default:
                    return new Color();

            }
        }

        public static float getCostPerPassengerCapacityLine(ConfigIndex i)
        {
            switch (i & ConfigIndex.SYSTEM_PART)
            {
                case ConfigIndex.TRAIN_CONFIG:
                    return 50f / 400;
                case ConfigIndex.SHIP_CONFIG:
                    return 50f / 800;
                case ConfigIndex.PLANE_CONFIG:
                    return 50f / 1000;
                case ConfigIndex.TRAM_CONFIG:
                    return 50f / 90;
                case ConfigIndex.METRO_CONFIG:
                    return 50f / 180;
                case ConfigIndex.MONORAIL_CONFIG:
                    return 50f / 180;
                case ConfigIndex.FERRY_CONFIG:
                    return 50f / 50;
                case ConfigIndex.BLIMP_CONFIG:
                    return 50f / 70;
                case ConfigIndex.BUS_CONFIG:
                    return 50f / 60;
                case ConfigIndex.TOUR_BUS_CONFIG:
                    return 50f / 60;
                case ConfigIndex.TOUR_PED_CONFIG:
                    return 0;
                default:
                    return 50f / 30;

            }
        }

        public static string getNameForServiceType(ConfigIndex i) => Locale.Get(getLocaleIdForIndex(i, out string key), key);

        private static string getLocaleIdForIndex(ConfigIndex i, out string key)
        {
            switch (i & ConfigIndex.DESC_DATA)
            {
                case ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                    key = "Beautification";
                    break;
                case ConfigIndex.ELECTRICITY_SERVICE_CONFIG:
                    key = "Electricity";
                    break;
                case ConfigIndex.WATER_SERVICE_CONFIG:
                    key = "WaterAndSewage";
                    break;
                case ConfigIndex.GARBAGE_SERVICE_CONFIG:
                    key = "Garbage";
                    break;
                case ConfigIndex.ROAD_SERVICE_CONFIG:
                    key = "Roads";
                    break;
                case ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                    key = "Healthcare";
                    break;
                case ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                    key = "Police";
                    break;
                case ConfigIndex.EDUCATION_SERVICE_CONFIG:
                    key = "Education";
                    break;
                case ConfigIndex.MONUMENT_SERVICE_CONFIG:
                    key = "Monuments";
                    break;
                case ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                    key = "FireDepartment";
                    break;
                case ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                    key = "PublicTransport";
                    break;
                case ConfigIndex.DISASTER_SERVICE_CONFIG:
                    key = "Government";
                    break;
                case ConfigIndex.DISTRICT_NAME_CONFIG:
                    key = "District";
                    break;
                default:
                    key = null;
                    break;
            }
            switch (i & ConfigIndex.DESC_DATA)
            {
                case ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                    return "DISTRICT_RESIDENTIAL";
                case ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                    return "DISTRICT_COMMERCIAL";
                case ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                    return "DISTRICT_INDUSTRIAL";
                case ConfigIndex.NATURAL_SERVICE_CONFIG:
                    return "NATURAL_SERVICE";
                //case ConfigIndex.UNUSED2_SERVICE_CONFIG:
                //return "Unused2";
                case ConfigIndex.CITIZEN_SERVICE_CONFIG:
                    return "INCOME_CITIZEN";
                case ConfigIndex.TOURISM_SERVICE_CONFIG:
                    return "INCOME_TOURIST";
                case ConfigIndex.OFFICE_SERVICE_CONFIG:
                    return "DISTRICT_OFFICE";
                case ConfigIndex.ADDRESS_NAME_CONFIG:
                    return "TLM_ROAD_NAMING_STOP";
                case ConfigIndex.PARKAREA_NAME_CONFIG:
                    return "TLM_PARKAREA_NAMING_STOP";
                case ConfigIndex.ROAD_SERVICE_CONFIG:
                case ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG:
                case ConfigIndex.GARBAGE_SERVICE_CONFIG:
                case ConfigIndex.ELECTRICITY_SERVICE_CONFIG:
                case ConfigIndex.WATER_SERVICE_CONFIG:
                case ConfigIndex.HEALTHCARE_SERVICE_CONFIG:
                case ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG:
                case ConfigIndex.EDUCATION_SERVICE_CONFIG:
                case ConfigIndex.MONUMENT_SERVICE_CONFIG:
                case ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG:
                case ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG:
                case ConfigIndex.DISASTER_SERVICE_CONFIG:
                case ConfigIndex.DISTRICT_NAME_CONFIG:
                    return "MAIN_TOOL";
                default:
                    return "???";

            }
        }

        public static string getBgIconForIndex(TLMConfigWarehouse.ConfigIndex transportType)
        {
            switch (transportType & TLMConfigWarehouse.ConfigIndex.SYSTEM_PART)
            {
                case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG:
                    return "TrainIcon";
                case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG:
                    return "SubwayIcon";
                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                    return "BusIcon";
                case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG:
                    return "TramIcon";
                case TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG:
                    return "ShipLineIcon";
                case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG:
                    return "CableCarTabIcon";
                case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG:
                    return "MonorailIcon";
                case TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG:
                    return "PlaneLineIcon";
                case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG:
                    return "FerryIcon";
                case TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG:
                    return "BlimpIcon";
                case TLMConfigWarehouse.ConfigIndex.EVAC_BUS_CONFIG:
                    return "EvacBusIcon";
                case TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG:
                    return "TourPedIcon";
                case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG:
                    return "TourBusIcon";
                case TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG:
                    return "TaxiTabIcon";
                default:
                    return "BusIcon";
            }
        }
        public static string getNameForTransportType(ConfigIndex i)
        {
            switch (i & ConfigIndex.SYSTEM_PART)
            {
                case ConfigIndex.TRAIN_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Train Engine");
                case ConfigIndex.TRAM_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Tram");
                case ConfigIndex.METRO_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Metro");
                case ConfigIndex.BUS_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Bus");
                case ConfigIndex.PLANE_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Aircraft Passenger");
                case ConfigIndex.SHIP_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Ship Passenger");
                case ConfigIndex.BLIMP_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Blimp");
                case ConfigIndex.FERRY_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Ferry");
                case ConfigIndex.MONORAIL_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Monorail Front");
                case ConfigIndex.EVAC_BUS_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Evacuation Bus");
                case ConfigIndex.TOUR_BUS_CONFIG:
                    return Locale.Get("TOOLTIP_TOURISTBUSLINES");
                case ConfigIndex.TOUR_PED_CONFIG:
                    return Locale.Get("TOOLTIP_WALKINGTOURS");
                case ConfigIndex.CABLE_CAR_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Cable Car");
                case ConfigIndex.TAXI_CONFIG:
                    return Locale.Get("VEHICLE_TITLE", "Taxi");
                default:
                    return "???";

            }
        }

        public override bool getDefaultBoolValueForProperty(ConfigIndex i)
        {
            return defaultTrueBoolProperties.Contains(i);
        }

        public override int getDefaultIntValueForProperty(ConfigIndex i)
        {
            if ((i & ConfigIndex.DEFAULT_TICKET_PRICE) == ConfigIndex.DEFAULT_TICKET_PRICE && (i & ConfigIndex.ADC_DESC_PART) == 0 && TransportManager.exists)
            {
                var tsd = getTransportSystemDefinitionForConfigTransport(i & ConfigIndex.SYSTEM_PART);
                if (tsd != default(TransportSystemDefinition))
                {
                    return TransportManager.instance.GetTransportInfo(tsd.transportType)?.m_ticketPrice ?? 0;
                }
            }
            switch (i)
            {
                default:
                    return 0;
            }
        }

        public static ItemClass.SubService getSubserviceFromSystemId(ConfigIndex idx)
        {
            var systemIdx = idx & ConfigIndex.SYSTEM_PART;
            switch (systemIdx)
            {
                case ConfigIndex.TRAIN_CONFIG: return ItemClass.SubService.PublicTransportTrain;
                case ConfigIndex.TRAM_CONFIG: return ItemClass.SubService.PublicTransportTram;
                case ConfigIndex.METRO_CONFIG: return ItemClass.SubService.PublicTransportMetro;
                case ConfigIndex.BUS_CONFIG: return ItemClass.SubService.PublicTransportBus;
                case ConfigIndex.EVAC_BUS_CONFIG: return ItemClass.SubService.PublicTransportBus;
                case ConfigIndex.PLANE_CONFIG: return ItemClass.SubService.PublicTransportPlane;
                case ConfigIndex.SHIP_CONFIG: return ItemClass.SubService.PublicTransportShip;
                case ConfigIndex.MONORAIL_CONFIG: return ItemClass.SubService.PublicTransportMonorail;
                case ConfigIndex.TAXI_CONFIG: return ItemClass.SubService.PublicTransportTaxi;
                case ConfigIndex.CABLE_CAR_CONFIG: return ItemClass.SubService.PublicTransportCableCar;
                case ConfigIndex.TOUR_PED_CONFIG: return ItemClass.SubService.PublicTransportTours;
                case ConfigIndex.TOUR_BUS_CONFIG: return ItemClass.SubService.PublicTransportTours;
                case ConfigIndex.BALLOON_CONFIG: return ItemClass.SubService.PublicTransportTours;
                case ConfigIndex.BLIMP_CONFIG: return ItemClass.SubService.PublicTransportPlane;
                case ConfigIndex.FERRY_CONFIG: return ItemClass.SubService.PublicTransportShip;

                default:
                    return ItemClass.SubService.None;
            }
        }
        public static TransferManager.TransferReason[] getTransferReasonFromSystemId(ConfigIndex idx)
        {



            var systemIdx = idx & ConfigIndex.SYSTEM_PART;
            switch (systemIdx)
            {
                case ConfigIndex.TRAIN_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerTrain };
                case ConfigIndex.TRAM_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Tram };
                case ConfigIndex.METRO_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.MetroTrain, TransferManager.TransferReason.PassengerTrain };
                case ConfigIndex.BUS_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Bus };
                case ConfigIndex.EVAC_BUS_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.EvacuateA, TransferManager.TransferReason.EvacuateB, TransferManager.TransferReason.EvacuateC, TransferManager.TransferReason.EvacuateD, TransferManager.TransferReason.EvacuateVipA, TransferManager.TransferReason.EvacuateVipB, TransferManager.TransferReason.EvacuateVipC, TransferManager.TransferReason.EvacuateVipD };
                case ConfigIndex.PLANE_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerPlane };
                case ConfigIndex.SHIP_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerShip };
                case ConfigIndex.MONORAIL_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Monorail };
                case ConfigIndex.TAXI_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Taxi };
                case ConfigIndex.CABLE_CAR_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.CableCar };
                case ConfigIndex.TOUR_PED_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristA, TransferManager.TransferReason.TouristB, TransferManager.TransferReason.TouristC, TransferManager.TransferReason.TouristD };
                case ConfigIndex.TOUR_BUS_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristBus };
                case ConfigIndex.BALLOON_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristA, TransferManager.TransferReason.TouristB, TransferManager.TransferReason.TouristC, TransferManager.TransferReason.TouristD };
                case ConfigIndex.BLIMP_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Blimp };
                case ConfigIndex.FERRY_CONFIG: return new TransferManager.TransferReason[] { TransferManager.TransferReason.Ferry };

                default:
                    return null;
            }
        }

        public static bool isServiceLineNameable(ItemClass.Service s) => getCurrentConfigBool(ConfigIndex.USE_FOR_AUTO_NAMING_REF | GameServiceExtensions.toConfigIndex(s, ItemClass.SubService.None));
        public static string getPrefixForServiceLineNameable(ItemClass.Service s) => getCurrentConfigString(ConfigIndex.AUTO_NAMING_REF_TEXT | GameServiceExtensions.toConfigIndex(s, ItemClass.SubService.None));
        public static bool isPublicTransportLineNameable(ref TransportSystemDefinition tsd) => getCurrentConfigBool(ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | getConfigTransportSystemForDefinition(ref tsd));
        public static string getPrefixForPublicTransportLineNameable(ref TransportSystemDefinition tsd) => getCurrentConfigString(ConfigIndex.PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT | getConfigTransportSystemForDefinition(ref tsd));

        public static ConfigIndex getConfigAssetsForAI(ref TransportSystemDefinition tsd)
        {
            if (tsd == TransportSystemDefinition.BUS)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_BUS;
            }
            else if (tsd == TransportSystemDefinition.TRAIN)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_TRAIN;
            }
            else if (tsd == TransportSystemDefinition.TRAM)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_TRAM;
            }
            else if (tsd == TransportSystemDefinition.SHIP)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_SHIP;
            }
            else if (tsd == TransportSystemDefinition.PLANE)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_PLANE;
            }
            else if (tsd == TransportSystemDefinition.FERRY)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_SHIP;
            }
            else if (tsd == TransportSystemDefinition.BLIMP)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_PLANE;
            }
            else if (tsd == TransportSystemDefinition.MONORAIL)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_MONORAIL;
            }
            else if (tsd == TransportSystemDefinition.METRO)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_METRO;
            }
            else if (tsd == TransportSystemDefinition.EVAC_BUS)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_EVAC_BUS;
            }
            else if (tsd == TransportSystemDefinition.TOUR_BUS)
            {
                return ConfigIndex.PREFIX_BASED_ASSETS_TOUR_BUS;
            }
            else
            {
                return ConfigIndex.NIL;
            }
        }
        public static ConfigIndex getConfigDepotPrefix(ref TransportSystemDefinition tsd)
        {

            if (tsd == TransportSystemDefinition.BUS)
            {
                return ConfigIndex.DEPOT_PREFIXES_BUS;
            }
            else if (tsd == TransportSystemDefinition.TRAIN)
            {
                return ConfigIndex.DEPOT_PREFIXES_TRAIN;
            }
            else if (tsd == TransportSystemDefinition.TRAM)
            {
                return ConfigIndex.DEPOT_PREFIXES_TRAM;
            }
            else if (tsd == TransportSystemDefinition.SHIP)
            {
                return ConfigIndex.DEPOT_PREFIXES_SHIP;
            }
            else if (tsd == TransportSystemDefinition.PLANE)
            {
                return ConfigIndex.DEPOT_PREFIXES_PLANE;
            }
            else if (tsd == TransportSystemDefinition.FERRY)
            {
                return ConfigIndex.DEPOT_PREFIXES_SHIP;
            }
            else if (tsd == TransportSystemDefinition.BLIMP)
            {
                return ConfigIndex.DEPOT_PREFIXES_PLANE;
            }
            else if (tsd == TransportSystemDefinition.MONORAIL)
            {
                return ConfigIndex.DEPOT_PREFIXES_MONORAIL;
            }
            else if (tsd == TransportSystemDefinition.METRO)
            {
                return ConfigIndex.DEPOT_PREFIXES_METRO;
            }
            else
            {
                return ConfigIndex.NIL;
            }
        }
        public static ConfigIndex getConfigPrefixForAI(ref TransportSystemDefinition tsd) => getConfigTransportSystemForDefinition(ref tsd) | ConfigIndex.PREFIX;

        public static ConfigIndex getConfigTransportSystemForDefinition(ref TransportSystemDefinition tsd)
        {
            if (tsd == TransportSystemDefinition.BUS)
            {
                return ConfigIndex.BUS_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TRAIN)
            {
                return ConfigIndex.TRAIN_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TRAM)
            {
                return ConfigIndex.TRAM_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.SHIP)
            {
                return ConfigIndex.SHIP_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.PLANE)
            {
                return ConfigIndex.PLANE_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.FERRY)
            {
                return ConfigIndex.FERRY_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.BLIMP)
            {
                return ConfigIndex.BLIMP_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.MONORAIL)
            {
                return ConfigIndex.MONORAIL_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.METRO)
            {
                return ConfigIndex.METRO_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.EVAC_BUS)
            {
                return ConfigIndex.EVAC_BUS_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TOUR_BUS)
            {
                return ConfigIndex.TOUR_BUS_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TOUR_PED)
            {
                return ConfigIndex.TOUR_PED_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.CABLE_CAR)
            {
                return ConfigIndex.CABLE_CAR_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.TAXI)
            {
                return ConfigIndex.TAXI_CONFIG;
            }
            else if (tsd == TransportSystemDefinition.BALLOON)
            {
                return ConfigIndex.BALLOON_CONFIG;
            }
            else
            {
                return ConfigIndex.NIL;
            }
        }
        public static TransportSystemDefinition getTransportSystemDefinitionForConfigTransport(ConfigIndex T)
        {

            switch (T)
            {
                case ConfigIndex.BUS_CONFIG:
                    return TransportSystemDefinition.BUS;
                case ConfigIndex.TRAIN_CONFIG:
                    return TransportSystemDefinition.TRAIN;
                case ConfigIndex.TRAM_CONFIG:
                    return TransportSystemDefinition.TRAM;
                case ConfigIndex.SHIP_CONFIG:
                    return TransportSystemDefinition.SHIP;
                case ConfigIndex.PLANE_CONFIG:
                    return TransportSystemDefinition.PLANE;
                case ConfigIndex.METRO_CONFIG:
                    return TransportSystemDefinition.METRO;
                case ConfigIndex.MONORAIL_CONFIG:
                    return TransportSystemDefinition.MONORAIL;
                case ConfigIndex.BLIMP_CONFIG:
                    return TransportSystemDefinition.BLIMP;
                case ConfigIndex.FERRY_CONFIG:
                    return TransportSystemDefinition.FERRY;
                case ConfigIndex.EVAC_BUS_CONFIG:
                    return TransportSystemDefinition.EVAC_BUS;
                case ConfigIndex.TOUR_BUS_CONFIG:
                    return TransportSystemDefinition.TOUR_BUS;
                case ConfigIndex.TOUR_PED_CONFIG:
                    return TransportSystemDefinition.TOUR_PED;
                default:
                    return default(TransportSystemDefinition);
            }
        }


        public static readonly ConfigIndex[] configurableTicketTransportCategories = {
            ConfigIndex. TRAIN_CONFIG      ,
            ConfigIndex. TRAM_CONFIG       ,
            ConfigIndex. METRO_CONFIG      ,
            ConfigIndex. BUS_CONFIG        ,
            ConfigIndex. PLANE_CONFIG      ,
            ConfigIndex. SHIP_CONFIG       ,
            ConfigIndex. MONORAIL_CONFIG   ,
            ConfigIndex. FERRY_CONFIG      ,
            ConfigIndex. BLIMP_CONFIG      ,
            ConfigIndex. CABLE_CAR_CONFIG  ,
            ConfigIndex. TAXI_CONFIG       ,
            ConfigIndex. TOUR_BUS_CONFIG   ,
        };

        public static readonly ConfigIndex[] configurableAutoNameTransportCategories = {
            ConfigIndex.PLANE_CONFIG,
            ConfigIndex.BLIMP_CONFIG,
            ConfigIndex.SHIP_CONFIG,
            ConfigIndex.FERRY_CONFIG,
            ConfigIndex.TRAIN_CONFIG,
            ConfigIndex.MONORAIL_CONFIG,
            ConfigIndex.TRAM_CONFIG,
            ConfigIndex.METRO_CONFIG,
            ConfigIndex.BUS_CONFIG,
            ConfigIndex.TOUR_BUS_CONFIG,
            ConfigIndex.TOUR_PED_CONFIG,
        };
        public static readonly ConfigIndex[] configurableAutoNameCategories = {
            ConfigIndex.MONUMENT_SERVICE_CONFIG,
            ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG,
            ConfigIndex.HEALTHCARE_SERVICE_CONFIG,
            ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG,
            ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG,
            ConfigIndex.EDUCATION_SERVICE_CONFIG,
            ConfigIndex.DISASTER_SERVICE_CONFIG,
            ConfigIndex.GARBAGE_SERVICE_CONFIG,
        };
        public static readonly ConfigIndex[] extraAutoNameCategories = {
            ConfigIndex. PARKAREA_NAME_CONFIG       ,
            ConfigIndex. DISTRICT_NAME_CONFIG       ,
            ConfigIndex. ADDRESS_NAME_CONFIG
        };
        public static readonly ConfigIndex[] defaultTrueBoolProperties = {
             ConfigIndex.MONUMENT_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.BEAUTIFICATION_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.TRAIN_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.METRO_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.BUS_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.PLANE_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.SHIP_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.PARKAREA_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.DISTRICT_USE_FOR_AUTO_NAMING_REF,
             ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME,
             ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.METRO_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.BUS_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.MONORAIL_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.CABLE_CAR_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP ,
             ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP ,
        };
        public static readonly ConfigIndex[] namingOrder =
        {
            TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG  ,
            TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG  ,
            TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG  ,
            TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.METRO_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.BUS_CONFIG   ,
            TLMConfigWarehouse.ConfigIndex.TAXI_CONFIG  ,
            TLMConfigWarehouse.ConfigIndex.NATURAL_SERVICE_CONFIG   ,
            TLMConfigWarehouse.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG    ,
            TLMConfigWarehouse.ConfigIndex.MONUMENT_SERVICE_CONFIG  ,
            TLMConfigWarehouse.ConfigIndex.HEALTHCARE_SERVICE_CONFIG    ,
            TLMConfigWarehouse.ConfigIndex.EDUCATION_SERVICE_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.DISASTER_SERVICE_CONFIG    ,
            TLMConfigWarehouse.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG  ,
            TLMConfigWarehouse.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG    ,
            TLMConfigWarehouse.ConfigIndex.GARBAGE_SERVICE_CONFIG   ,
            TLMConfigWarehouse.ConfigIndex.WATER_SERVICE_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.ELECTRICITY_SERVICE_CONFIG   ,
            TLMConfigWarehouse.ConfigIndex.ROAD_SERVICE_CONFIG  ,
            TLMConfigWarehouse.ConfigIndex.OFFICE_SERVICE_CONFIG    ,
            TLMConfigWarehouse.ConfigIndex.TOURISM_SERVICE_CONFIG   ,
            TLMConfigWarehouse.ConfigIndex.CITIZEN_SERVICE_CONFIG   ,
            TLMConfigWarehouse.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG    ,
            TLMConfigWarehouse.ConfigIndex.COMMERCIAL_SERVICE_CONFIG    ,
            TLMConfigWarehouse.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG   ,
            //TLMConfigWarehouse.ConfigIndex.UNUSED2_SERVICE_CONFIG
        };

        public enum ConfigIndex
        {
            NIL = -1,
            ADC_DESC_PART = 0x7F000000,
            SYSTEM_PART = 0xFF0000,
            TYPE_PART = 0x00FF00,
            DESC_DATA = 0xFF,

            GLOBAL_CONFIG = 0x1000000,
            USE_FOR_AUTO_NAMING_REF = 0x2000000 | TYPE_BOOL,
            AUTO_NAMING_REF_TEXT = 0x3000000 | TYPE_STRING,

            TYPE_STRING = 0x0100,
            TYPE_INT = 0x0200,
            TYPE_BOOL = 0x0300,
            TYPE_LIST = 0x0400,
            TYPE_DICTIONARY = 0x0500,

            AUTO_COLOR_ENABLED = GLOBAL_CONFIG | 0x2 | TYPE_BOOL,
            CIRCULAR_IN_SINGLE_DISTRICT_LINE = GLOBAL_CONFIG | 0x3 | TYPE_BOOL,
            AUTO_NAME_ENABLED = GLOBAL_CONFIG | 0x4 | TYPE_BOOL,
            ADD_LINE_NUMBER_IN_AUTONAME = GLOBAL_CONFIG | 0x9 | TYPE_BOOL,
            PREFIX_BASED_ASSETS_BUS = GLOBAL_CONFIG | 0xA | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_TRAM = GLOBAL_CONFIG | 0xB | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_TRAIN = GLOBAL_CONFIG | 0xC | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_SHIP = GLOBAL_CONFIG | 0xD | TYPE_DICTIONARY,
            DEPOT_PREFIXES_BUS = GLOBAL_CONFIG | 0xE | TYPE_DICTIONARY,
            DEPOT_PREFIXES_TRAM = GLOBAL_CONFIG | 0xF | TYPE_DICTIONARY,
            DEPOT_PREFIXES_TRAIN = GLOBAL_CONFIG | 0x10 | TYPE_DICTIONARY,
            DEPOT_PREFIXES_METRO = GLOBAL_CONFIG | 0x11 | TYPE_DICTIONARY,
            DEPOT_PREFIXES_SHIP = GLOBAL_CONFIG | 0x12 | TYPE_DICTIONARY,
            DEPOT_PREFIXES_PLANE = GLOBAL_CONFIG | 0x18 | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_PLANE = GLOBAL_CONFIG | 0x19 | TYPE_DICTIONARY,
            LINES_CONFIG = GLOBAL_CONFIG | 0x1A | TYPE_DICTIONARY,
            STOPS_CONFIG = GLOBAL_CONFIG | 0x1B | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_MONORAIL = GLOBAL_CONFIG | 0x1D | TYPE_DICTIONARY,
            DEPOT_PREFIXES_MONORAIL = GLOBAL_CONFIG | 0x1F | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_METRO = GLOBAL_CONFIG | 0x20 | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_EVAC_BUS = GLOBAL_CONFIG | 0x21 | TYPE_DICTIONARY,
            PREFIX_BASED_ASSETS_TOUR_BUS = GLOBAL_CONFIG | 0x22 | TYPE_DICTIONARY,

            TRAIN_CONFIG = TransportInfo.TransportType.Train << 16,
            TRAM_CONFIG = TransportInfo.TransportType.Tram << 16,
            METRO_CONFIG = TransportInfo.TransportType.Metro << 16,
            BUS_CONFIG = TransportInfo.TransportType.Bus << 16 | 0x800000,
            EVAC_BUS_CONFIG = TransportInfo.TransportType.EvacuationBus << 16,
            PLANE_CONFIG = TransportInfo.TransportType.Airplane << 16,
            SHIP_CONFIG = TransportInfo.TransportType.Ship << 16,
            MONORAIL_CONFIG = TransportInfo.TransportType.Monorail << 16,
            TAXI_CONFIG = TransportInfo.TransportType.Taxi << 16,
            CABLE_CAR_CONFIG = TransportInfo.TransportType.CableCar << 16,
            TOUR_PED_CONFIG = TransportInfo.TransportType.Pedestrian << 16,
            TOUR_BUS_CONFIG = TransportInfo.TransportType.TouristBus << 16,
            BALLOON_CONFIG = TransportInfo.TransportType.HotAirBalloon << 16,
            BLIMP_CONFIG = TransportInfo.TransportType.Airplane << 16 | 0x800000,
            FERRY_CONFIG = TransportInfo.TransportType.Ship << 16 | 0x800000,



            RESIDENTIAL_SERVICE_CONFIG = ItemClass.Service.Residential,
            COMMERCIAL_SERVICE_CONFIG = ItemClass.Service.Commercial,
            INDUSTRIAL_SERVICE_CONFIG = ItemClass.Service.Industrial,
            NATURAL_SERVICE_CONFIG = ItemClass.Service.Natural,
            //UNUSED2_SERVICE_CONFIG = ItemClass.Service.Unused2,
            CITIZEN_SERVICE_CONFIG = ItemClass.Service.Citizen,
            TOURISM_SERVICE_CONFIG = ItemClass.Service.Tourism,
            OFFICE_SERVICE_CONFIG = ItemClass.Service.Office,
            ROAD_SERVICE_CONFIG = ItemClass.Service.Road,
            ELECTRICITY_SERVICE_CONFIG = ItemClass.Service.Electricity,
            WATER_SERVICE_CONFIG = ItemClass.Service.Water,
            BEAUTIFICATION_SERVICE_CONFIG = ItemClass.Service.Beautification,
            GARBAGE_SERVICE_CONFIG = ItemClass.Service.Garbage,
            HEALTHCARE_SERVICE_CONFIG = ItemClass.Service.HealthCare,
            POLICEDEPARTMENT_SERVICE_CONFIG = ItemClass.Service.PoliceDepartment,
            EDUCATION_SERVICE_CONFIG = ItemClass.Service.Education,
            MONUMENT_SERVICE_CONFIG = ItemClass.Service.Monument,
            FIREDEPARTMENT_SERVICE_CONFIG = ItemClass.Service.FireDepartment,
            PUBLICTRANSPORT_SERVICE_CONFIG = ItemClass.Service.PublicTransport,
            DISASTER_SERVICE_CONFIG = ItemClass.Service.Disaster,
            PARKAREA_NAME_CONFIG = 0xfd,
            DISTRICT_NAME_CONFIG = 0xfe,
            ADDRESS_NAME_CONFIG = 0xff,



            PREFIX = 0x1 | TYPE_INT,
            SEPARATOR = 0x2 | TYPE_INT,
            SUFFIX = 0x3 | TYPE_INT,
            LEADING_ZEROS = 0x4 | TYPE_BOOL,
            PALETTE_MAIN = 0x5 | TYPE_STRING,
            PALETTE_SUBLINE = 0x6 | TYPE_STRING,
            PALETTE_RANDOM_ON_OVERFLOW = 0x7 | TYPE_BOOL,
            PALETTE_PREFIX_BASED = 0x8 | TYPE_BOOL,
            SHOW_IN_LINEAR_MAP = 0x9 | TYPE_BOOL,
            INVERT_PREFIX_SUFFIX = 0xA | TYPE_BOOL,
            //DEFAULT_COST_PER_PASSENGER_CAPACITY = 0xB | TYPE_INT,
            NON_PREFIX = 0xC | TYPE_INT,
            PREFIX_INCREMENT = 0xD | TYPE_BOOL,
            DEFAULT_TICKET_PRICE = 0xE | TYPE_INT,

            TRAIN_PREFIX = TRAIN_CONFIG | PREFIX,
            TRAM_PREFIX = TRAM_CONFIG | PREFIX,
            METRO_PREFIX = METRO_CONFIG | PREFIX,
            BUS_PREFIX = BUS_CONFIG | PREFIX,
            SHIP_PREFIX = SHIP_CONFIG | PREFIX,
            PLANE_PREFIX = PLANE_CONFIG | PREFIX,
            MONORAIL_PREFIX = MONORAIL_CONFIG | PREFIX,
            FERRY_PREFIX = FERRY_CONFIG | PREFIX,
            BLIMP_PREFIX = BLIMP_CONFIG | PREFIX,
            TOUR_PED_CONFIG_PREFIX = TOUR_PED_CONFIG | PREFIX,
            TOUR_BUS_CONFIG_PREFIX = TOUR_BUS_CONFIG | PREFIX,

            TRAIN_SEPARATOR = TRAIN_CONFIG | SEPARATOR,
            TRAM_SEPARATOR = TRAM_CONFIG | SEPARATOR,
            METRO_SEPARATOR = METRO_CONFIG | SEPARATOR,
            BUS_SEPARATOR = BUS_CONFIG | SEPARATOR,
            SHIP_SEPARATOR = SHIP_CONFIG | SEPARATOR,
            PLANE_SEPARATOR = PLANE_CONFIG | SEPARATOR,
            MONORAIL_SEPARATOR = MONORAIL_CONFIG | SEPARATOR,
            FERRY_SEPARATOR = FERRY_CONFIG | SEPARATOR,
            BLIMP_SEPARATOR = BLIMP_CONFIG | SEPARATOR,
            TOUR_PED_CONFIG_SEPARATOR = TOUR_PED_CONFIG | SEPARATOR,
            TOUR_BUS_CONFIG_SEPARATOR = TOUR_BUS_CONFIG | SEPARATOR,

            TRAIN_SUFFIX = TRAIN_CONFIG | SUFFIX,
            TRAM_SUFFIX = TRAM_CONFIG | SUFFIX,
            METRO_SUFFIX = METRO_CONFIG | SUFFIX,
            BUS_SUFFIX = BUS_CONFIG | SUFFIX,
            SHIP_SUFFIX = SHIP_CONFIG | SUFFIX,
            PLANE_SUFFIX = PLANE_CONFIG | SUFFIX,
            MONORAIL_SUFFIX = MONORAIL_CONFIG | SUFFIX,
            FERRY_SUFFIX = FERRY_CONFIG | SUFFIX,
            BLIMP_SUFFIX = BLIMP_CONFIG | SUFFIX,
            TOUR_PED_CONFIG_SUFFIX = TOUR_PED_CONFIG | SUFFIX,
            TOUR_BUS_CONFIG_SUFFIX = TOUR_BUS_CONFIG | SUFFIX,

            TRAIN_NON_PREFIX = TRAIN_CONFIG | NON_PREFIX,
            TRAM_NON_PREFIX = TRAM_CONFIG | NON_PREFIX,
            METRO_NON_PREFIX = METRO_CONFIG | NON_PREFIX,
            BUS_NON_PREFIX = BUS_CONFIG | NON_PREFIX,
            SHIP_NON_PREFIX = SHIP_CONFIG | NON_PREFIX,
            PLANE_NON_PREFIX = PLANE_CONFIG | NON_PREFIX,
            MONORAIL_NON_PREFIX = MONORAIL_CONFIG | NON_PREFIX,
            FERRY_NON_PREFIX = FERRY_CONFIG | NON_PREFIX,
            BLIMP_NON_PREFIX = BLIMP_CONFIG | NON_PREFIX,
            TOUR_PED_CONFIG_NON_PREFIX = TOUR_PED_CONFIG | NON_PREFIX,
            TOUR_BUS_CONFIG_NON_PREFIX = TOUR_BUS_CONFIG | NON_PREFIX,

            TRAIN_LEADING_ZEROS = TRAIN_CONFIG | LEADING_ZEROS,
            TRAM_LEADING_ZEROS = TRAM_CONFIG | LEADING_ZEROS,
            METRO_LEADING_ZEROS = METRO_CONFIG | LEADING_ZEROS,
            BUS_LEADING_ZEROS = BUS_CONFIG | LEADING_ZEROS,
            SHIP_LEADING_ZEROS = SHIP_CONFIG | LEADING_ZEROS,
            PLANE_LEADING_ZEROS = PLANE_CONFIG | LEADING_ZEROS,
            MONORAIL_LEADING_ZEROS = MONORAIL_CONFIG | LEADING_ZEROS,
            FERRY_LEADING_ZEROS = FERRY_CONFIG | LEADING_ZEROS,
            BLIMP_LEADING_ZEROS = BLIMP_CONFIG | LEADING_ZEROS,
            TOUR_PED_CONFIG_LEADING_ZEROS = TOUR_PED_CONFIG | LEADING_ZEROS,
            TOUR_BUS_CONFIG_LEADING_ZEROS = TOUR_BUS_CONFIG | LEADING_ZEROS,

            TRAIN_INVERT_PREFIX_SUFFIX = TRAIN_CONFIG | INVERT_PREFIX_SUFFIX,
            TRAM_INVERT_PREFIX_SUFFIX = TRAM_CONFIG | INVERT_PREFIX_SUFFIX,
            METRO_INVERT_PREFIX_SUFFIX = METRO_CONFIG | INVERT_PREFIX_SUFFIX,
            BUS_INVERT_PREFIX_SUFFIX = BUS_CONFIG | INVERT_PREFIX_SUFFIX,
            SHIP_INVERT_PREFIX_SUFFIX = SHIP_CONFIG | INVERT_PREFIX_SUFFIX,
            PLANE_INVERT_PREFIX_SUFFIX = PLANE_CONFIG | INVERT_PREFIX_SUFFIX,
            MONORAIL_INVERT_PREFIX_SUFFIX = MONORAIL_CONFIG | INVERT_PREFIX_SUFFIX,
            FERRY_INVERT_PREFIX_SUFFIX = FERRY_CONFIG | INVERT_PREFIX_SUFFIX,
            BLIMP_INVERT_PREFIX_SUFFIX = BLIMP_CONFIG | INVERT_PREFIX_SUFFIX,
            TOUR_PED_CONFIG_INVERT_PREFIX_SUFFIX = TOUR_PED_CONFIG | INVERT_PREFIX_SUFFIX,
            TOUR_BUS_CONFIG_INVERT_PREFIX_SUFFIX = TOUR_BUS_CONFIG | INVERT_PREFIX_SUFFIX,

            TRAIN_PALETTE_MAIN = TRAIN_CONFIG | PALETTE_MAIN,
            TRAM_PALETTE_MAIN = TRAM_CONFIG | PALETTE_MAIN,
            METRO_PALETTE_MAIN = METRO_CONFIG | PALETTE_MAIN,
            BUS_PALETTE_MAIN = BUS_CONFIG | PALETTE_MAIN,
            SHIP_PALETTE_MAIN = SHIP_CONFIG | PALETTE_MAIN,
            PLANE_PALETTE_MAIN = PLANE_CONFIG | PALETTE_MAIN,
            MONORAIL_PALETTE_MAIN = MONORAIL_CONFIG | PALETTE_MAIN,
            FERRY_PALETTE_MAIN = FERRY_CONFIG | PALETTE_MAIN,
            BLIMP_PALETTE_MAIN = BLIMP_CONFIG | PALETTE_MAIN,
            TOUR_PED_CONFIG_PALETTE_MAIN = TOUR_PED_CONFIG | PALETTE_MAIN,
            TOUR_BUS_CONFIG_PALETTE_MAIN = TOUR_BUS_CONFIG | PALETTE_MAIN,

            TRAIN_PALETTE_SUBLINE = TRAIN_CONFIG | PALETTE_SUBLINE,
            TRAM_PALETTE_SUBLINE = TRAM_CONFIG | PALETTE_SUBLINE,
            METRO_PALETTE_SUBLINE = METRO_CONFIG | PALETTE_SUBLINE,
            BUS_PALETTE_SUBLINE = BUS_CONFIG | PALETTE_SUBLINE,
            SHIP_PALETTE_SUBLINE = SHIP_CONFIG | PALETTE_SUBLINE,
            PLANE_PALETTE_SUBLINE = PLANE_CONFIG | PALETTE_SUBLINE,
            MONORAIL_PALETTE_SUBLINE = MONORAIL_CONFIG | PALETTE_SUBLINE,
            FERRY_PALETTE_SUBLINE = FERRY_CONFIG | PALETTE_SUBLINE,
            BLIMP_PALETTE_SUBLINE = BLIMP_CONFIG | PALETTE_SUBLINE,
            TOUR_PED_CONFIG_PALETTE_SUBLINE = TOUR_PED_CONFIG | PALETTE_SUBLINE,
            TOUR_BUS_CONFIG_PALETTE_SUBLINE = TOUR_BUS_CONFIG | PALETTE_SUBLINE,

            TRAIN_PALETTE_RANDOM_ON_OVERFLOW = TRAIN_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TRAM_PALETTE_RANDOM_ON_OVERFLOW = TRAM_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            METRO_PALETTE_RANDOM_ON_OVERFLOW = METRO_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            BUS_PALETTE_RANDOM_ON_OVERFLOW = BUS_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            SHIP_PALETTE_RANDOM_ON_OVERFLOW = SHIP_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            PLANE_PALETTE_RANDOM_ON_OVERFLOW = PLANE_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            MONORAIL_PALETTE_RANDOM_ON_OVERFLOW = MONORAIL_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            FERRY_PALETTE_RANDOM_ON_OVERFLOW = FERRY_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            BLIMP_PALETTE_RANDOM_ON_OVERFLOW = BLIMP_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TOUR_PED_CONFIG_PALETTE_RANDOM_ON_OVERFLOW = TOUR_PED_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TOUR_BUS_CONFIG_PALETTE_RANDOM_ON_OVERFLOW = TOUR_BUS_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,

            TRAIN_PALETTE_PREFIX_BASED = TRAIN_CONFIG | PALETTE_PREFIX_BASED,
            TRAM_PALETTE_PREFIX_BASED = TRAM_CONFIG | PALETTE_PREFIX_BASED,
            METRO_PALETTE_PREFIX_BASED = METRO_CONFIG | PALETTE_PREFIX_BASED,
            BUS_PALETTE_PREFIX_BASED = BUS_CONFIG | PALETTE_PREFIX_BASED,
            SHIP_PALETTE_PREFIX_BASED = SHIP_CONFIG | PALETTE_PREFIX_BASED,
            PLANE_PALETTE_PREFIX_BASED = PLANE_CONFIG | PALETTE_PREFIX_BASED,
            MONORAIL_PALETTE_PREFIX_BASED = MONORAIL_CONFIG | PALETTE_PREFIX_BASED,
            FERRY_PALETTE_PREFIX_BASED = FERRY_CONFIG | PALETTE_PREFIX_BASED,
            BLIMP_PALETTE_PREFIX_BASED = BLIMP_CONFIG | PALETTE_PREFIX_BASED,
            TOUR_PED_CONFIG_PALETTE_PREFIX_BASED = TOUR_PED_CONFIG | PALETTE_PREFIX_BASED,
            TOUR_BUS_CONFIG_PALETTE_PREFIX_BASED = TOUR_BUS_CONFIG | PALETTE_PREFIX_BASED,

            TRAIN_SHOW_IN_LINEAR_MAP = TRAIN_CONFIG | SHOW_IN_LINEAR_MAP,
            TRAM_SHOW_IN_LINEAR_MAP = TRAM_CONFIG | SHOW_IN_LINEAR_MAP,
            METRO_SHOW_IN_LINEAR_MAP = METRO_CONFIG | SHOW_IN_LINEAR_MAP,
            BUS_SHOW_IN_LINEAR_MAP = BUS_CONFIG | SHOW_IN_LINEAR_MAP,
            PLANE_SHOW_IN_LINEAR_MAP = PLANE_CONFIG | SHOW_IN_LINEAR_MAP,
            SHIP_SHOW_IN_LINEAR_MAP = SHIP_CONFIG | SHOW_IN_LINEAR_MAP,
            MONORAIL_SHOW_IN_LINEAR_MAP = MONORAIL_CONFIG | SHOW_IN_LINEAR_MAP,
            FERRY_SHOW_IN_LINEAR_MAP = FERRY_CONFIG | SHOW_IN_LINEAR_MAP,
            BLIMP_SHOW_IN_LINEAR_MAP = BLIMP_CONFIG | SHOW_IN_LINEAR_MAP,
            CABLE_CAR_SHOW_IN_LINEAR_MAP = CABLE_CAR_CONFIG | SHOW_IN_LINEAR_MAP,
            TAXI_SHOW_IN_LINEAR_MAP = TAXI_CONFIG | SHOW_IN_LINEAR_MAP,
            EVAC_BUS_SHOW_IN_LINEAR_MAP = EVAC_BUS_CONFIG | SHOW_IN_LINEAR_MAP,
            TOUR_PED_CONFIG_SHOW_IN_LINEAR_MAP = TOUR_PED_CONFIG | SHOW_IN_LINEAR_MAP,
            TOUR_BUS_CONFIG_SHOW_IN_LINEAR_MAP = TOUR_BUS_CONFIG | SHOW_IN_LINEAR_MAP,


            TRAIN_DEFAULT_TICKET_PRICE = TRAIN_CONFIG | DEFAULT_TICKET_PRICE,
            TRAM_DEFAULT_TICKET_PRICE = TRAM_CONFIG | DEFAULT_TICKET_PRICE,
            METRO_DEFAULT_TICKET_PRICE = METRO_CONFIG | DEFAULT_TICKET_PRICE,
            BUS_DEFAULT_TICKET_PRICE = BUS_CONFIG | DEFAULT_TICKET_PRICE,
            PLANE_DEFAULT_TICKET_PRICE = PLANE_CONFIG | DEFAULT_TICKET_PRICE,
            SHIP_DEFAULT_TICKET_PRICE = SHIP_CONFIG | DEFAULT_TICKET_PRICE,
            MONORAIL_DEFAULT_TICKET_PRICE = MONORAIL_CONFIG | DEFAULT_TICKET_PRICE,
            FERRY_DEFAULT_TICKET_PRICE = FERRY_CONFIG | DEFAULT_TICKET_PRICE,
            BLIMP_DEFAULT_TICKET_PRICE = BLIMP_CONFIG | DEFAULT_TICKET_PRICE,
            CABLE_CAR_DEFAULT_TICKET_PRICE = CABLE_CAR_CONFIG | DEFAULT_TICKET_PRICE,
            TAXI_DEFAULT_TICKET_PRICE = TAXI_CONFIG | DEFAULT_TICKET_PRICE,
            TOUR_BUS_CONFIG_DEFAULT_TICKET_PRICE = TOUR_BUS_CONFIG | DEFAULT_TICKET_PRICE,

            //TRAIN_DEFAULT_COST_PER_PASSENGER_CAPACITY = TRAIN_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //TRAM_DEFAULT_COST_PER_PASSENGER_CAPACITY = TRAM_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //METRO_DEFAULT_COST_PER_PASSENGER_CAPACITY = METRO_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //BUS_DEFAULT_COST_PER_PASSENGER_CAPACITY = BUS_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //SHIP_DEFAULT_COST_PER_PASSENGER_CAPACITY = SHIP_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //PLANE_DEFAULT_COST_PER_PASSENGER_CAPACITY = PLANE_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //MONORAIL_DEFAULT_COST_PER_PASSENGER_CAPACITY = MONORAIL_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //FERRY_DEFAULT_COST_PER_PASSENGER_CAPACITY = FERRY_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //BLIMP_DEFAULT_COST_PER_PASSENGER_CAPACITY = BLIMP_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //TOUR_PED_CONFIG_DEFAULT_COST_PER_PASSENGER_CAPACITY = TOUR_PED_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,
            //TOUR_BUS_CONFIG_DEFAULT_COST_PER_PASSENGER_CAPACITY = TOUR_BUS_CONFIG | DEFAULT_COST_PER_PASSENGER_CAPACITY,

            TRAIN_PREFIX_INCREMENT = TRAIN_CONFIG | PREFIX_INCREMENT,
            METRO_PREFIX_INCREMENT = METRO_CONFIG | PREFIX_INCREMENT,
            BUS_PREFIX_INCREMENT = BUS_CONFIG | PREFIX_INCREMENT,
            TRAM_PREFIX_INCREMENT = TRAM_CONFIG | PREFIX_INCREMENT,
            PLANE_PREFIX_INCREMENT = PLANE_CONFIG | PREFIX_INCREMENT,
            SHIP_PREFIX_INCREMENT = SHIP_CONFIG | PREFIX_INCREMENT,
            MONORAIL_PREFIX_INCREMENT = MONORAIL_CONFIG | PREFIX_INCREMENT,
            FERRY_PREFIX_INCREMENT = FERRY_CONFIG | PREFIX_INCREMENT,
            BLIMP_PREFIX_INCREMENT = BLIMP_CONFIG | PREFIX_INCREMENT,
            TOUR_PED_CONFIG_PREFIX_INCREMENT = TOUR_PED_CONFIG | PREFIX_INCREMENT,
            TOUR_BUS_CONFIG_PREFIX_INCREMENT = TOUR_BUS_CONFIG | PREFIX_INCREMENT,

            RESIDENTIAL_USE_FOR_AUTO_NAMING_REF = RESIDENTIAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            COMMERCIAL_USE_FOR_AUTO_NAMING_REF = COMMERCIAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            INDUSTRIAL_USE_FOR_AUTO_NAMING_REF = INDUSTRIAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            NATURAL_USE_FOR_AUTO_NAMING_REF = NATURAL_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            //UNUSED2_USE_FOR_AUTO_NAMING_REF = UNUSED2_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            CITIZEN_USE_FOR_AUTO_NAMING_REF = CITIZEN_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            TOURISM_USE_FOR_AUTO_NAMING_REF = TOURISM_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            OFFICE_USE_FOR_AUTO_NAMING_REF = OFFICE_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            ROAD_USE_FOR_AUTO_NAMING_REF = ROAD_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            ELECTRICITY_USE_FOR_AUTO_NAMING_REF = ELECTRICITY_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            WATER_USE_FOR_AUTO_NAMING_REF = WATER_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            BEAUTIFICATION_USE_FOR_AUTO_NAMING_REF = BEAUTIFICATION_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            GARBAGE_USE_FOR_AUTO_NAMING_REF = GARBAGE_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            HEALTHCARE_USE_FOR_AUTO_NAMING_REF = HEALTHCARE_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            POLICEDEPARTMENT_USE_FOR_AUTO_NAMING_REF = POLICEDEPARTMENT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            EDUCATION_USE_FOR_AUTO_NAMING_REF = EDUCATION_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            MONUMENT_USE_FOR_AUTO_NAMING_REF = MONUMENT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            FIREDEPARTMENT_USE_FOR_AUTO_NAMING_REF = FIREDEPARTMENT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF = PUBLICTRANSPORT_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,
            DISASTER_USE_FOR_AUTO_NAMING_REF = DISASTER_SERVICE_CONFIG | USE_FOR_AUTO_NAMING_REF,


            PARKAREA_USE_FOR_AUTO_NAMING_REF = PARKAREA_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,
            DISTRICT_USE_FOR_AUTO_NAMING_REF = DISTRICT_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,
            ADDRESS_USE_FOR_AUTO_NAMING_REF = ADDRESS_NAME_CONFIG | USE_FOR_AUTO_NAMING_REF,


            TRAIN_USE_FOR_AUTO_NAMING_REF = TRAIN_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TRAM_USE_FOR_AUTO_NAMING_REF = TRAM_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            METRO_USE_FOR_AUTO_NAMING_REF = METRO_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            BUS_USE_FOR_AUTO_NAMING_REF = BUS_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            PLANE_USE_FOR_AUTO_NAMING_REF = PLANE_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TAXI_USE_FOR_AUTO_NAMING_REF = TAXI_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            SHIP_USE_FOR_AUTO_NAMING_REF = SHIP_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            CABLE_CAR_USE_FOR_AUTO_NAMING_REF = CABLE_CAR_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            MONORAIL_USE_FOR_AUTO_NAMING_REF = MONORAIL_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            FERRY_USE_FOR_AUTO_NAMING_REF = FERRY_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            BLIMP_USE_FOR_AUTO_NAMING_REF = BLIMP_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TOUR_PED_USE_FOR_AUTO_NAMING_REF = TOUR_PED_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            TOUR_BUS_USE_FOR_AUTO_NAMING_REF = TOUR_BUS_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,
            BALOON_USE_FOR_AUTO_NAMING_REF = BALLOON_CONFIG | PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF,

            RESIDENTIAL_AUTO_NAMING_REF_TEXT = RESIDENTIAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            COMMERCIAL_AUTO_NAMING_REF_TEXT = COMMERCIAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            INDUSTRIAL_AUTO_NAMING_REF_TEXT = INDUSTRIAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            UNUSED1_AUTO_NAMING_REF_TEXT = NATURAL_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            //UNUSED2_AUTO_NAMING_REF_TEXT = UNUSED2_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            CITIZEN_AUTO_NAMING_REF_TEXT = CITIZEN_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            TOURISM_AUTO_NAMING_REF_TEXT = TOURISM_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            OFFICE_AUTO_NAMING_REF_TEXT = OFFICE_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            ROAD_AUTO_NAMING_REF_TEXT = ROAD_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            ELECTRICITY_AUTO_NAMING_REF_TEXT = ELECTRICITY_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            WATER_AUTO_NAMING_REF_TEXT = WATER_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            BEAUTIFICATION_AUTO_NAMING_REF_TEXT = BEAUTIFICATION_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            GARBAGE_AUTO_NAMING_REF_TEXT = GARBAGE_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            HEALTHCARE_AUTO_NAMING_REF_TEXT = HEALTHCARE_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            POLICEDEPARTMENT_AUTO_NAMING_REF_TEXT = POLICEDEPARTMENT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            EDUCATION_AUTO_NAMING_REF_TEXT = EDUCATION_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            MONUMENT_AUTO_NAMING_REF_TEXT = MONUMENT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            FIREDEPARTMENT_AUTO_NAMING_REF_TEXT = FIREDEPARTMENT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT = PUBLICTRANSPORT_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,
            GOVERNMENT_AUTO_NAMING_REF_TEXT = DISASTER_SERVICE_CONFIG | AUTO_NAMING_REF_TEXT,

            PARKAREA_NAMING_REF_TEXT = PARKAREA_NAME_CONFIG | AUTO_NAMING_REF_TEXT,
            DISTRICT_NAMING_REF_TEXT = DISTRICT_NAME_CONFIG | AUTO_NAMING_REF_TEXT,
            ADDRESS_NAMING_REF_TEXT = ADDRESS_NAME_CONFIG | AUTO_NAMING_REF_TEXT,

            TRAIN_AUTO_NAMING_REF_TEXT = TRAIN_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            METRO_AUTO_NAMING_REF_TEXT = METRO_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            BUS_AUTO_NAMING_REF_TEXT = BUS_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TRAM_AUTO_NAMING_REF_TEXT = TRAM_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            PLANE_AUTO_NAMING_REF_TEXT = PLANE_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TAXI_AUTO_NAMING_REF_TEXT = TAXI_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            SHIP_AUTO_NAMING_REF_TEXT = SHIP_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            CABLE_CAR_AUTO_NAMING_REF_TEXT = CABLE_CAR_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            MONORAIL_AUTO_NAMING_REF_TEXT = MONORAIL_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            FERRY_AUTO_NAMING_REF_TEXT = FERRY_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            BLIMP_AUTO_NAMING_REF_TEXT = BLIMP_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TOUR_PED_AUTO_NAMING_REF_TEXT = TOUR_PED_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            TOUR_BUS_AUTO_NAMING_REF_TEXT = TOUR_BUS_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,
            BALOON_AUTO_NAMING_REF_TEXT = BALLOON_CONFIG | PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT,


        }
    }
}
