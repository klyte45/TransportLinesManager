using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.Commons;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TransferManager;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition : IIdentifiable
    {
        private static readonly NonSequentialList<TransportSystemDefinition> registeredTsd;

        static TransportSystemDefinition() => registeredTsd = new NonSequentialList<TransportSystemDefinition>()
        {
            [0] = BUS,
            [0] = BLIMP,
            [0] = BALLOON,
            [0] = CABLE_CAR,
            [0] = EVAC_BUS,
            [0] = FERRY,
            [0] = HELICOPTER,
            [0] = METRO,
            [0] = MONORAIL,
            [0] = PLANE,
            [0] = POST,
            [0] = SHIP,
            [0] = TAXI,
            [0] = TOUR_BUS,
            [0] = TOUR_PED,
            [0] = TRAIN,
            [0] = TRAM,
            [0] = TROLLEY,
            [0] = FISHING,
        };

        private static readonly Dictionary<TransportSystemDefinition, TransportInfoContainer> m_infoList = new Dictionary<TransportSystemDefinition, TransportInfoContainer>();

        public class TransportInfoContainer
        {
            public TransportInfo Local { get; internal set; }
            public TransportInfo Intercity { get; internal set; }
        }

        public static Dictionary<TransportSystemDefinition, TransportInfoContainer> TransportInfoDict
        {
            get
            {
                if (m_infoList.Count == 0)
                {
                    LogUtils.DoLog("TSD loading infos");
                    for (uint i = 0; i < PrefabCollection<TransportInfo>.LoadedCount(); i++)
                    {
                        TransportInfo info = PrefabCollection<TransportInfo>.GetLoaded(i);
                        var tsd = FromLocal(info);
                        if (tsd == default)
                        {
                            tsd = FromIntercity(info);

                            if (tsd == default)
                            {
                                LogUtils.DoErrorLog($"TSD not found for info: {info}");
                                continue;
                            }
                            else if (m_infoList.ContainsKey(tsd))
                            {
                                if (m_infoList[tsd].Intercity != null)
                                {
                                    LogUtils.DoErrorLog($"More than one info for same TSD Intercity \"{tsd}\": {m_infoList[tsd]},{info}");
                                    continue;
                                }
                                m_infoList[tsd].Intercity = info;
                            }
                            else
                            {
                                m_infoList[tsd] = new TransportInfoContainer
                                {
                                    Intercity = info
                                };
                            }
                        }
                        else if (m_infoList.ContainsKey(tsd))
                        {
                            if (m_infoList[tsd].Local != null)
                            {
                                LogUtils.DoErrorLog($"More than one info for same TSD Local \"{tsd}\": {m_infoList[tsd]},{info}");
                                continue;
                            }
                            m_infoList[tsd].Local = info;
                        }
                        else
                        {
                            m_infoList[tsd] = new TransportInfoContainer
                            {
                                Local = info
                            };
                        }
                    }
                    IEnumerable<TransportSystemDefinition> missing = registeredTsd.Values.Where(x => !m_infoList.ContainsKey(x));
                    if (missing.Count() > 0 && CommonProperties.DebugMode)
                    {
                        LogUtils.DoLog($"Some TSDs can't find their infos: [{string.Join(", ", missing.Select(x => x.ToString()).ToArray())}]\nIgnore if you don't have all DLCs installed");
                    }
                    LogUtils.DoLog("TSD end loading infos");
                }
                return m_infoList;
            }
        }

        public ItemClass.SubService SubService { get; }
        public VehicleInfo.VehicleType VehicleType { get; }
        public TransportInfo.TransportType TransportType { get; }
        public ItemClass.Level Level { get; }
        public ItemClass.Level? LevelAdditional { get; }
        public ItemClass.Level? LevelIntercity { get; }
        private uint Index_Internal { get; }
        public TransferReason[] Reasons { get; }
        public Color Color { get; }
        public int DefaultCapacity { get; }
        public LineIconSpriteNames DefaultIcon { get; }
        public bool HasLines { get; }
        public uint Id { get => Index_Internal; set { } }
        long? IIdentifiable.Id { get => Id; set { } }

        private TransportSystemDefinition(
        ItemClass.SubService subService,
            VehicleInfo.VehicleType vehicleType,
            TransportInfo.TransportType transportType,
            ItemClass.Level level,
            TransferReason[] reasons,
            Color color,
            int defaultCapacity,
            LineIconSpriteNames defaultIcon,
            bool hasLines,
            ItemClass.Level? levelIntercity = null,
            ItemClass.Level? levelAdditional = null)
        {
            VehicleType = vehicleType;
            SubService = subService;
            TransportType = transportType;
            Level = level;
            LevelAdditional = levelAdditional;
            Reasons = reasons;
            Color = color;
            DefaultCapacity = defaultCapacity;
            DefaultIcon = defaultIcon;
            HasLines = hasLines;
            LevelIntercity = levelIntercity;
            Index_Internal = GetTsdIndex(TransportType, SubService, VehicleType, Level, LevelAdditional, LevelIntercity);
        }


        internal static uint GetTsdIndex(TransportInfo.TransportType TransportType, ItemClass.SubService SubService, VehicleInfo.VehicleType VehicleType, ItemClass.Level Level, ItemClass.Level? LevelAdditional, ItemClass.Level? LevelIntercity)
        {
            uint levelBitmask = (1u << ((byte)Level)) | (LevelAdditional is null ? 0u : (1u << ((byte)LevelAdditional)));

            return (((uint)TransportType & 0x1fu) << 19)
                       | ((((uint)KlyteMathUtils.BitScanForward((uint)VehicleType) + 1u) & 0x1Fu) << 14)
                       | (((uint)SubService & 0x3fu) << 8)
                       | ((levelBitmask & 0x1fu) << 3)
                       | ((LevelIntercity is null ? 7u : (uint)LevelIntercity) & 0x7u);
        }

        internal static void GetParametersFromTsdIndex(uint num, out TransportInfo.TransportType TransportType, out ItemClass.SubService SubService, out VehicleInfo.VehicleType VehicleType, out ItemClass.Level Level, out ItemClass.Level? LevelAdditional, out ItemClass.Level? LevelIntercity)
        {
            TransportType = (TransportInfo.TransportType)((num >> 19) & 0x1f);
            SubService = (ItemClass.SubService)((num >> 8) & 0x3f);
            VehicleType = (VehicleInfo.VehicleType)(1 << (int)(((num >> 14) & 0x1f) - 1));
            Level = (ItemClass.Level)(KlyteMathUtils.BitScanForward((num >> 3) & 0x1f));

            var restLevel = (ulong)(((num >> 3) & 0x1f) - (1 << (int)Level));

            LevelAdditional = restLevel == 0 ? null : (ItemClass.Level?)KlyteMathUtils.BitScanForward(restLevel);
            LevelIntercity = (num & 0x7) == 7 ? null : (ItemClass.Level?)(num & 0x7);
        }

        public ITLMTransportTypeExtension GetTransportExtension() => TLMTransportTypeDataContainer.Instance?.SafeGet(Index_Internal);
        public bool IsTour() => SubService == ItemClass.SubService.PublicTransportTours;
        public bool IsShelterAiDepot() => this == EVAC_BUS;
        public bool HasVehicles() => VehicleType != VehicleInfo.VehicleType.None;
        public bool IsPrefixable()
        {
            switch (TransportType)
            {
                case TransportInfo.TransportType.HotAirBalloon:
                case TransportInfo.TransportType.Taxi:
                case TransportInfo.TransportType.CableCar:
                case TransportInfo.TransportType.Pedestrian:
                case TransportInfo.TransportType.EvacuationBus:
                case TransportInfo.TransportType.Fishing:
                    return false;
                default:
                    return true;
            }
        }

        public string GetTransportTypeIcon()
        {
            switch (TransportType)
            {
                case TransportInfo.TransportType.EvacuationBus: return "SubBarFireDepartmentDisaster";
                case TransportInfo.TransportType.Pedestrian: return "SubBarPublicTransportWalkingTours";
                case TransportInfo.TransportType.TouristBus: return "SubBarPublicTransportTours";
                case TransportInfo.TransportType.HotAirBalloon: return "IconBalloonTours";
                case TransportInfo.TransportType.Post: return "SubBarPublicTransportPost";
                case TransportInfo.TransportType.CableCar: return PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.EvacuationBus);
                case TransportInfo.TransportType.Airplane:
                    return VehicleType == VehicleInfo.VehicleType.Helicopter
                        ? "IconPolicyHelicopterPriority"
                        : PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportType);
                //case TransportInfo.TransportType.Ship:
                //case TransportInfo.TransportType.Bus:
                //case TransportInfo.TransportType.Metro:
                //case TransportInfo.TransportType.Train:
                //case TransportInfo.TransportType.Taxi:
                //case TransportInfo.TransportType.Tram:
                //case TransportInfo.TransportType.Monorail:
                default: return PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportType);
            };
        }

        public bool IsFromSystem(VehicleInfo info) => info.m_class.m_subService == SubService
            && info.m_vehicleType == VehicleType
            && (info.m_class.m_level == Level || info.m_class.m_level == LevelAdditional)
            && (VehicleUtils.GetTransportInfoField(info.m_vehicleAI)?.GetValue(info.m_vehicleAI) as TransportInfo)?.m_transportType == TransportType
            && VehicleUtils.GetVehicleCapacityField(info.m_vehicleAI) != null;
        public bool IsFromSystemIntercity(VehicleInfo info) => !(LevelIntercity is null)
            && info.m_class.m_subService == SubService
            && info.m_vehicleType == VehicleType
            && (info.m_class.m_level == LevelIntercity)
            && (VehicleUtils.GetTransportInfoField(info.m_vehicleAI)?.GetValue(info.m_vehicleAI) as TransportInfo)?.m_transportType == TransportType
            && VehicleUtils.GetVehicleCapacityField(info.m_vehicleAI) != null;

        public bool IsFromSystem(TransportInfo info) => info != null && info.m_class.m_subService == SubService && info.m_vehicleType == VehicleType && info.m_transportType == TransportType;

        public bool IsFromSystem(DepotAI p) => p != null && ((p.m_info.m_class.m_subService == SubService && p.m_transportInfo.m_vehicleType == VehicleType && p.m_maxVehicleCount > 0 && p.m_transportInfo.m_transportType == TransportType)
                || (p.m_secondaryTransportInfo != null && p.m_secondaryTransportInfo.m_vehicleType == VehicleType && p.m_maxVehicleCount2 > 0 && p.m_secondaryTransportInfo.m_transportType == TransportType));
        public bool IsFromSystem(ref TransportLine tl) => (tl.Info.m_class.m_subService == SubService && tl.Info.m_vehicleType == VehicleType && tl.Info.m_transportType == TransportType);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != typeof(TransportSystemDefinition))
            {
                return false;
            }
            var other = (TransportSystemDefinition)obj;

            return Index_Internal == other.Index_Internal;
        }

        public static bool operator ==(TransportSystemDefinition a, TransportSystemDefinition b) => Equals(a, b);
        public static bool operator !=(TransportSystemDefinition a, TransportSystemDefinition b) => !(a == b);

        public static TransportSystemDefinition From(PrefabAI buildingAI) => buildingAI is DepotAI depotAI ? FromLocal(depotAI.m_transportInfo) : buildingAI is OutsideConnectionAI ocAI ? FromIntercity(ocAI.m_transportInfo) : null;
        public static TransportSystemDefinition FromLocal(TransportInfo info)
        {
            if (info is null)
            {
                return default;
            }
            TransportSystemDefinition result = registeredTsd.Values.FirstOrDefault(x =>
            x.SubService == info.m_class.m_subService
            && x.VehicleType == info.m_vehicleType
            && x.TransportType == info.m_transportType
            && (x.Level == info.GetClassLevel() || x.LevelAdditional == info.GetClassLevel()));
            if (result == default)
            {
                LogUtils.DoErrorLog($"Local TSD NOT FOUND FOR TRANSPORT INFO: info.m_class.m_subService={info.m_class.m_subService}, info.m_vehicleType={info.m_vehicleType}, info.m_transportType={info.m_transportType}, info.classLevel = {info.GetClassLevel()}");
            }
            return result;
        }
        public static TransportSystemDefinition FromIntercity(TransportInfo info)
        {
            if (info is null)
            {
                return default;
            }
            TransportSystemDefinition result = registeredTsd.Values.FirstOrDefault(x =>
            x.SubService == info.m_class.m_subService
            && x.VehicleType == info.m_vehicleType
            && x.TransportType == info.m_transportType
            && x.LevelIntercity == info.GetClassLevel());
            if (result == default)
            {
                LogUtils.DoLog($"Intercity TSD NOT FOUND FOR TRANSPORT INFO: info.m_class.m_subService={info.m_class.m_subService}, info.m_vehicleType={info.m_vehicleType}, info.m_transportType={info.m_transportType}, info.classLevel = {info.GetClassLevel()}");
            }
            return result;
        }
        public static TransportSystemDefinition From(VehicleInfo info) =>
            info is null
                ? (default)
                : registeredTsd.Values.FirstOrDefault(x =>
                    x.SubService == info.m_class.m_subService
                    && x.VehicleType == info.m_vehicleType
                    && ReflectionUtils.HasField(info.GetAI(), "m_transportInfo")
                    && ReflectionUtils.GetPrivateField<TransportInfo>(info.GetAI(), "m_transportInfo") is TransportInfo ti
                    && ti.m_transportType == x.TransportType
                    && (x.Level == ti.GetClassLevel() || x.LevelAdditional == ti.GetClassLevel() || x.LevelIntercity == ti.GetClassLevel())
                );
        public static TransportSystemDefinition FromLineId(ushort lineId, bool fromBuilding)
        {
            if (!fromBuilding)
            {
                return GetDefinitionForLine(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineId]);
            }
            else
            {
                var buildingId = TLMStationUtils.GetStationBuilding(lineId, lineId, fromBuilding);
                ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
                var systemAI = b.Info.GetAI();
                return systemAI is TransportStationAI tsai
                    ? FromIntercity(tsai.UseSecondaryTransportInfoForConnection() ? tsai.m_secondaryTransportInfo : tsai.m_transportInfo)
                    : null;
            }
        }

        public static TransportSystemDefinition FromOutsideConnection(ItemClass.SubService SubService, ItemClass.Level Level) => registeredTsd.Where(x => x.Value.LevelIntercity == Level && x.Value.SubService == SubService).FirstOrDefault().Value;
        public static TransportSystemDefinition From(TransportInfo.TransportType TransportType, ItemClass.SubService SubService, VehicleInfo.VehicleType VehicleType, ItemClass.Level Level)
        {
            var targetMask = GetTsdIndex(TransportType, SubService, VehicleType, Level, null, null);
            LogUtils.DoLog($"Index ({TransportType},{SubService},{VehicleType},{Level}) == {targetMask.ToString("X8")}\n{Environment.StackTrace}");
            return FromIndex(targetMask);
        }

        public static TransportSystemDefinition FromIndex(uint idx)
        {
            var result = registeredTsd.Where(x => (x.Key & 0xFFFF00) == (idx & 0xFFFF00) && (x.Key & 0xf8 & idx) > 0).FirstOrDefault().Value;
            if (result is null)
            {
                LogUtils.DoErrorLog($"Invalid Index! Searching for: {idx.ToString("X8")}");
            }

            return result;
        }

        public static TransportSystemDefinition GetDefinitionForLine(ushort i)
        {
            if (i == 0)
            {
                throw new Exception("INVALID LINE TO GET DEFINITION: Line 0");
            }
            return GetDefinitionForLine(ref Singleton<TransportManager>.instance.m_lines.m_buffer[i]);
        }

        public static TransportSystemDefinition GetDefinitionForLine(ref TransportLine t) => FromLocal(t.Info);

        public override string ToString() => SubService.ToString() + "|" + VehicleType.ToString();

        public override int GetHashCode()
        {
            int hashCode = 286451371;
            hashCode = (hashCode * -1521134295) + SubService.GetHashCode();
            hashCode = (hashCode * -1521134295) + VehicleType.GetHashCode();
            return hashCode;
        }

        public float GetEffectivePassengerCapacityCost()
        {
            int settedCost = GetConfig()?.DefaultCostPerPassenger ?? 0;
            return settedCost == 0 ? GetDefaultPassengerCapacityCostLocal() : settedCost / 100f;
        }
        public float GetDefaultPassengerCapacityCostLocal() => TransportInfoDict.TryGetValue(this, out TransportInfoContainer info) && !(info.Local is null) ? info.Local.m_maintenanceCostPerVehicle / (float)DefaultCapacity : -1;

        public LineIconSpriteNames GetBgIcon()
        {
            var conf = GetConfig();
            var iconName = conf.DefaultLineIcon;
            return iconName == LineIconSpriteNames.NULL ? DefaultIcon : iconName;
        }


        public TLMTransportTypeConfigurationsXML GetConfig() => TLMBaseConfigXML.CurrentContextConfig.GetTransportData(this);

        public string GetTransportName() =>
              this == TRAIN ? Locale.Get("VEHICLE_TITLE", "Train Engine")
            : this == TRAM ? Locale.Get("VEHICLE_TITLE", "Tram")
            : this == METRO ? Locale.Get("VEHICLE_TITLE", "Metro")
            : this == BUS ? Locale.Get("VEHICLE_TITLE", "Bus")
            : this == PLANE ? Locale.Get("VEHICLE_TITLE", "Aircraft Passenger")
            : this == SHIP ? Locale.Get("VEHICLE_TITLE", "Ship Passenger")
            : this == BLIMP ? Locale.Get("VEHICLE_TITLE", "Blimp")
            : this == FERRY ? Locale.Get("VEHICLE_TITLE", "Ferry")
            : this == MONORAIL ? Locale.Get("VEHICLE_TITLE", "Monorail Front")
            : this == EVAC_BUS ? Locale.Get("VEHICLE_TITLE", "Evacuation Bus")
            : this == TOUR_BUS ? Locale.Get("TOOLTIP_TOURISTBUSLINES")
            : this == TOUR_PED ? Locale.Get("TOOLTIP_WALKINGTOURS")
            : this == CABLE_CAR ? Locale.Get("VEHICLE_TITLE", "Cable Car")
            : this == TAXI ? Locale.Get("VEHICLE_TITLE", "Taxi")
            : this == HELICOPTER ? Locale.Get("VEHICLE_TITLE", "Passenger Helicopter")
            : this == TROLLEY ? Locale.Get("VEHICLE_TITLE", "Trolleybus 01")
            : "???";
        public bool CanHaveTerminals() => TLMController.Instance.ConnectorWTS.WtsAvailable ||
            (TransportType == TransportInfo.TransportType.Bus && TLMBaseConfigXML.CurrentContextConfig.ExpressBusesEnabled) ||
            (TransportType == TransportInfo.TransportType.Tram && TLMBaseConfigXML.CurrentContextConfig.ExpressTramsEnabled) ||
            (TransportType == TransportInfo.TransportType.Trolleybus && TLMBaseConfigXML.CurrentContextConfig.ExpressTrolleybusesEnabled);



    }
}
