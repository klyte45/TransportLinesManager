using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Klyte.TransportLinesManager.Extensors
{
    public struct TransportSystemDefinition
    {
        private static Dictionary<TransportSystemDefinition, Func<ITLMSysDef>> m_sysDefinitions = new Dictionary<TransportSystemDefinition, Func<ITLMSysDef>>();


        public static readonly TransportSystemDefinition BUS = new TransportSystemDefinition(ItemClass.SubService.PublicTransportBus, VehicleInfo.VehicleType.Car, TransportInfo.TransportType.Bus);
        public static readonly TransportSystemDefinition TRAM = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTram, VehicleInfo.VehicleType.Tram, TransportInfo.TransportType.Tram);
        public static readonly TransportSystemDefinition METRO = new TransportSystemDefinition(ItemClass.SubService.PublicTransportMetro, VehicleInfo.VehicleType.Metro, TransportInfo.TransportType.Metro);
        public static readonly TransportSystemDefinition MONORAIL = new TransportSystemDefinition(ItemClass.SubService.PublicTransportMonorail, VehicleInfo.VehicleType.Monorail, TransportInfo.TransportType.Monorail);
        public static readonly TransportSystemDefinition TRAIN = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTrain, VehicleInfo.VehicleType.Train, TransportInfo.TransportType.Train);
        public static readonly TransportSystemDefinition FERRY = new TransportSystemDefinition(ItemClass.SubService.PublicTransportShip, VehicleInfo.VehicleType.Ferry, TransportInfo.TransportType.Ship);
        public static readonly TransportSystemDefinition BLIMP = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPlane, VehicleInfo.VehicleType.Blimp, TransportInfo.TransportType.Airplane);
        public static readonly TransportSystemDefinition SHIP = new TransportSystemDefinition(ItemClass.SubService.PublicTransportShip, VehicleInfo.VehicleType.Ship, TransportInfo.TransportType.Ship);
        public static readonly TransportSystemDefinition PLANE = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPlane, VehicleInfo.VehicleType.Plane, TransportInfo.TransportType.Airplane);
        public static readonly TransportSystemDefinition EVAC_BUS = new TransportSystemDefinition(ItemClass.SubService.None, VehicleInfo.VehicleType.Car, TransportInfo.TransportType.EvacuationBus);
        public static readonly TransportSystemDefinition TOUR_PED = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTours, VehicleInfo.VehicleType.None, TransportInfo.TransportType.Pedestrian);
        public static readonly TransportSystemDefinition TOUR_BUS = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTours, VehicleInfo.VehicleType.Car, TransportInfo.TransportType.TouristBus);
        public static readonly TransportSystemDefinition CABLE_CAR = new TransportSystemDefinition(ItemClass.SubService.PublicTransportCableCar, VehicleInfo.VehicleType.CableCar, TransportInfo.TransportType.CableCar);
        public static readonly TransportSystemDefinition TAXI = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTaxi, VehicleInfo.VehicleType.Car, TransportInfo.TransportType.Taxi);
        public static readonly TransportSystemDefinition BALLOON = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTours, VehicleInfo.VehicleType.None, TransportInfo.TransportType.HotAirBalloon);
        public static readonly TransportSystemDefinition POST = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPost, VehicleInfo.VehicleType.None, TransportInfo.TransportType.Post);
        public static readonly TransportSystemDefinition TROLLEY = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTrolleybus, VehicleInfo.VehicleType.Trolleybus, TransportInfo.TransportType.Trolleybus);
        public static readonly TransportSystemDefinition HELICOPTER = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPlane, VehicleInfo.VehicleType.Helicopter, TransportInfo.TransportType.Helicopter);


        private static readonly Dictionary<TransportSystemDefinition, TransportInfo> m_infoList = new Dictionary<TransportSystemDefinition, TransportInfo>();
        public static Dictionary<TransportSystemDefinition, TransportInfo> TransportInfoDict
        {
            get {
                if (m_infoList.Count == 0)
                {
                    TLMUtils.doLog("TSD loading infos");
                    for (uint i = 0; i < PrefabCollection<TransportInfo>.LoadedCount(); i++)
                    {
                        TransportInfo info = PrefabCollection<TransportInfo>.GetLoaded(i);
                        var tsd = TransportSystemDefinition.From(info);
                        if (tsd == default)
                        {
                            TLMUtils.doErrorLog($"TSD not found for info: {info}");
                            continue;
                        }
                        if (m_infoList.ContainsKey(tsd))
                        {
                            TLMUtils.doErrorLog($"More than one info for same TSD \"{tsd}\": {m_infoList[tsd]},{info}");
                            continue;
                        }
                        m_infoList[tsd] = info;
                    }
                    IEnumerable<TransportSystemDefinition> missing = m_sysDefinitions.Keys.Where(x => !m_infoList.ContainsKey(x));
                    if (missing.Count() > 0)
                    {
                        TLMUtils.doLog($"Some TSDs can't find their infos: [{string.Join(", ", missing.Select(x => x.ToString()).ToArray())}]\nIgnore if you don't have all DLCs installed");
                    }
                    TLMUtils.doLog("TSD end loading infos");
                }
                return m_infoList;
            }
        }
        public static Dictionary<TransportSystemDefinition, Func<ITLMSysDef>> SysDefinitions
        {
            get {
                if (m_sysDefinitions.Count == 0)
                {
                    //bool isLoading = Singleton<LoadingManager>.instance.m_currentlyLoading;
                    //if (isLoading)
                    //{
                    //    TLMUtils.doErrorLog("STILL LOADING!");
                    //    var tempDef = new Dictionary<TransportSystemDefinition, Type>();
                    //    InitTypes(isLoading, ref tempDef);
                    //    return tempDef;
                    //}
                    //else
                    //{
                    //InitTypes(isLoading, ref m_sysDefinitions);
                    //}

                    InitTypes(ref m_sysDefinitions);
                }
                return m_sysDefinitions;
            }
        }

        private static void InitTypes(ref Dictionary<TransportSystemDefinition, Func<ITLMSysDef>> tempDef)
        {
            tempDef[BUS] = () => TLMSysDefNorBus.instance;
            tempDef[METRO] = () => TLMSysDefNorMet.instance;
            tempDef[TRAIN] = () => TLMSysDefNorTrn.instance;
            tempDef[SHIP] = () => TLMSysDefNorShp.instance;
            tempDef[PLANE] = () => TLMSysDefNorPln.instance;
            //if()=>isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.AfterDark))
            //{
            tempDef[TAXI] = () => TLMSysDefNorTax.instance;
            //}

            //if()=>isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Snowfall))
            //{
            tempDef[TRAM] = () => TLMSysDefNorTrm.instance;
            //}

            //if()=>isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.NaturalDisasters))
            //{
            tempDef[EVAC_BUS] = () => TLMSysDefEvcBus.instance;
            //}

            //if()=>isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.InMotion))
            //{
            tempDef[MONORAIL] = () => TLMSysDefNorMnr.instance;
            tempDef[FERRY] = () => TLMSysDefNorFer.instance;
            tempDef[BLIMP] = () => TLMSysDefNorBlp.instance;
            tempDef[CABLE_CAR] = () => TLMSysDefNorCcr.instance;
            //}

            //if()=>isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.GreenCities))
            //{
            //NONE
            //}

            //if()=>isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Parks))
            //{
            tempDef[TOUR_BUS] = () => TLMSysDefTouBus.instance;
            tempDef[TOUR_PED] = () => TLMSysDefTouPed.instance;
            tempDef[BALLOON] = () => TLMSysDefTouBal.instance;
            //}
            //if()=>isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Industry))
            //{
            tempDef[POST] = () => TLMSysDefPstPst.instance;
            //}
            // if (isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Urban))
            //{
            tempDef[HELICOPTER] = () => TLMSysDefNorHel.instance;
            tempDef[TROLLEY] = () => TLMSysDefNorTrl.instance;
            //}
        }

        public ItemClass.SubService SubService { get; }
        public VehicleInfo.VehicleType VehicleType { get; }
        public TransportInfo.TransportType TransportType { get; }

        private TransportSystemDefinition(
        ItemClass.SubService subService,
        VehicleInfo.VehicleType vehicleType,
        TransportInfo.TransportType transportType)
        {
            VehicleType = vehicleType;
            SubService = subService;
            TransportType = transportType;
        }

        public ITLMTransportTypeExtension GetTransportExtension() => SysDefinitions[this]().GetExtension();
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
                    return false;
                default:
                    return true;
            }
        }

        public Type GetDefType() => SysDefinitions[this]().GetType();

        public string GetTransportTypeIcon()
        {
            return TransportType switch
            {
                TransportInfo.TransportType.EvacuationBus => "SubBarFireDepartmentDisaster",
                TransportInfo.TransportType.Pedestrian => "SubBarPublicTransportWalkingTours",
                TransportInfo.TransportType.TouristBus => "SubBarPublicTransportTours",
                TransportInfo.TransportType.HotAirBalloon => "IconBalloonTours",
                TransportInfo.TransportType.Post => "SubBarPublicTransportPost",
                TransportInfo.TransportType.CableCar => PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.EvacuationBus),
                //case TransportInfo.TransportType.Ship:
                //case TransportInfo.TransportType.Airplane:
                //case TransportInfo.TransportType.Bus:
                //case TransportInfo.TransportType.Metro:
                //case TransportInfo.TransportType.Train:
                //case TransportInfo.TransportType.Taxi:
                //case TransportInfo.TransportType.Tram:
                //case TransportInfo.TransportType.Monorail:
                _ => PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportType),
            };
        }

        public bool IsFromSystem(VehicleInfo info) => info.m_class.m_subService == SubService && info.m_vehicleType == VehicleType && (VehicleUtils.GetTransportInfoField(info.m_vehicleAI)?.GetValue(info.m_vehicleAI) as TransportInfo)?.m_transportType == TransportType && VehicleUtils.GetVehicleCapacityField(info.m_vehicleAI) != null;

        public bool IsFromSystem(TransportInfo info) => info != null && info.m_class.m_subService == SubService && info.m_vehicleType == VehicleType && info.m_transportType == TransportType;

        public bool IsFromSystem(DepotAI p)
        {
            return p != null && ((p.m_info.m_class.m_subService == SubService && p.m_transportInfo.m_vehicleType == VehicleType && p.m_maxVehicleCount > 0 && p.m_transportInfo.m_transportType == TransportType)
                || (p.m_secondaryTransportInfo != null && p.m_secondaryTransportInfo.m_vehicleType == VehicleType && p.m_maxVehicleCount2 > 0 && p.m_secondaryTransportInfo.m_transportType == TransportType));
        }
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
            var other = (TransportSystemDefinition) obj;

            return SubService == other.SubService && VehicleType == other.VehicleType && TransportType == other.TransportType;
        }

        public static bool operator ==(TransportSystemDefinition a, TransportSystemDefinition b)
        {
            if (Object.Equals(a, null) || Object.Equals(b, null))
            {
                return Object.Equals(a, null) == Object.Equals(b, null);
            }
            return a.Equals(b);
        }
        public static bool operator !=(TransportSystemDefinition a, TransportSystemDefinition b) => !(a == b);

        public static TransportSystemDefinition From(PrefabAI buildingAI)
        {
            var depotAI = buildingAI as DepotAI;
            if (depotAI == null)
            {
                return default;
            }
            return From(depotAI.m_transportInfo);
        }

        public static TransportSystemDefinition From(TransportInfo info)
        {
            if (info == null)
            {
                return default;
            }
            TransportSystemDefinition result = SysDefinitions.Keys.FirstOrDefault(x => x.SubService == info.m_class.m_subService && x.VehicleType == info.m_vehicleType && x.TransportType == info.m_transportType);
            if (result == default)
            {
                TLMUtils.doErrorLog($"TSD NOT FOUND FOR TRANSPORT INFO: info.m_class.m_subService={info.m_class.m_subService}, info.m_vehicleType={info.m_vehicleType}, info.m_transportType={info.m_transportType}");
            }
            return result;
        }
        public static TransportSystemDefinition From(VehicleInfo info)
        {
            if (info == null)
            {
                return default;
            }
            TransportSystemDefinition result = SysDefinitions.Keys.FirstOrDefault(x => x.SubService == info.m_class.m_subService && x.VehicleType == info.m_vehicleType && ReflectionUtils.HasField(info.GetAI(), "m_transportInfo") && ReflectionUtils.GetPrivateField<TransportInfo>(info.GetAI(), "m_transportInfo").m_transportType == x.TransportType);
            return result;
        }
        public static TransportSystemDefinition From(uint lineId) => GetDefinitionForLine(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineId]);
        public static TransportSystemDefinition GetDefinitionForLine(ushort i) => GetDefinitionForLine(ref Singleton<TransportManager>.instance.m_lines.m_buffer[i]);
        public static TransportSystemDefinition GetDefinitionForLine(ref TransportLine t) => From(t.Info);

        public TLMConfigWarehouse.ConfigIndex ToConfigIndex() => TLMConfigWarehouse.getConfigTransportSystemForDefinition(ref this);

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
            int settedCost = TLMConfigWarehouse.GetCurrentConfigInt(ToConfigIndex() | TLMConfigWarehouse.ConfigIndex.DEFAULT_COST_PER_PASSENGER_CAPACITY);
            if (settedCost == 0)
            {
                return GetDefaultPassengerCapacityCost();
            }

            return settedCost / 100f;
        }
        public float GetDefaultPassengerCapacityCost() => TransportInfoDict.TryGetValue(this, out TransportInfo info) ? info.m_maintenanceCostPerVehicle / (float) GetDefaultPassengerCapacity() : -1;
        public int GetDefaultPassengerCapacity()
        {
            int result = 1;
            switch (TransportType)
            {
                case TransportInfo.TransportType.Bus:
                    result = 30;
                    break;
                case TransportInfo.TransportType.Metro:
                    result = 180;
                    break;
                case TransportInfo.TransportType.Train:
                    result = 240;
                    break;
                case TransportInfo.TransportType.Ship:
                    if (VehicleType == VehicleInfo.VehicleType.Ferry)
                    {
                        result = 50;
                    }
                    else
                    {
                        result = 100;
                    }

                    break;
                case TransportInfo.TransportType.Airplane:
                    if (VehicleType == VehicleInfo.VehicleType.Blimp)
                    {
                        result = 35;
                    }
                    else
                    {
                        result = 200;
                    }
                    break;
                case TransportInfo.TransportType.Tram:
                    result = 90;
                    break;
                case TransportInfo.TransportType.EvacuationBus:
                    result = 50;
                    break;
                case TransportInfo.TransportType.Monorail:
                    result = 180;
                    break;
                case TransportInfo.TransportType.Pedestrian:
                    result = 1;
                    break;
                case TransportInfo.TransportType.TouristBus:
                    result = 30;
                    break;
            }
            return result;
        }
    }
    public interface ITLMSysDef { TransportSystemDefinition GetTSD(); ITLMTransportTypeExtension GetExtension(); }
    public abstract class TLMSysDef<T> : Singleton<T>, ITLMSysDef where T : TLMSysDef<T> { public abstract ITLMTransportTypeExtension GetExtension(); public abstract TransportSystemDefinition GetTSD(); }
    public sealed class TLMSysDefNorBus : TLMSysDef<TLMSysDefNorBus> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorBus.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BUS; }
    public sealed class TLMSysDefEvcBus : TLMSysDef<TLMSysDefEvcBus> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionEvcBus.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.EVAC_BUS; }
    public sealed class TLMSysDefNorTrm : TLMSysDef<TLMSysDefNorTrm> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorTrm.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TRAM; }
    public sealed class TLMSysDefNorMnr : TLMSysDef<TLMSysDefNorMnr> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorMnr.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.MONORAIL; }
    public sealed class TLMSysDefNorMet : TLMSysDef<TLMSysDefNorMet> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorMet.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.METRO; }
    public sealed class TLMSysDefNorTrn : TLMSysDef<TLMSysDefNorTrn> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorTrn.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TRAIN; }
    public sealed class TLMSysDefNorFer : TLMSysDef<TLMSysDefNorFer> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorFer.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.FERRY; }
    public sealed class TLMSysDefNorBlp : TLMSysDef<TLMSysDefNorBlp> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorBlp.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BLIMP; }
    public sealed class TLMSysDefNorShp : TLMSysDef<TLMSysDefNorShp> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorShp.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.SHIP; }
    public sealed class TLMSysDefNorPln : TLMSysDef<TLMSysDefNorPln> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorPln.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.PLANE; }
    public sealed class TLMSysDefTouBus : TLMSysDef<TLMSysDefTouBus> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionTouBus.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TOUR_BUS; }
    public sealed class TLMSysDefPstPst : TLMSysDef<TLMSysDefPstPst> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionPstPst.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.POST; }
    public sealed class TLMSysDefTouPed : TLMSysDef<TLMSysDefTouPed> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionTouPed.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TOUR_PED; }
    public sealed class TLMSysDefTouBal : TLMSysDef<TLMSysDefTouBal> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionTouBal.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BALLOON; }
    public sealed class TLMSysDefNorCcr : TLMSysDef<TLMSysDefNorCcr> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorCcr.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.CABLE_CAR; }
    public sealed class TLMSysDefNorTax : TLMSysDef<TLMSysDefNorTax> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorTax.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TAXI; }
    public sealed class TLMSysDefNorTrl : TLMSysDef<TLMSysDefNorTrl> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorTrl.Instance;public override TransportSystemDefinition  GetTSD() => TransportSystemDefinition.TROLLEY; }
    public sealed class TLMSysDefNorHel : TLMSysDef<TLMSysDefNorHel> { public override ITLMTransportTypeExtension GetExtension() => TLMTransportTypeExtensionNorHel.Instance; public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.HELICOPTER; }

}
