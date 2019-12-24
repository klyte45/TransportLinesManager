using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static Klyte.TransportLinesManager.TextureAtlas.UVMTextureAtlas;

namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    public struct TransportSystemDefinition
    {
        private static Dictionary<TransportSystemDefinition, Type> m_sysDefinitions = new Dictionary<TransportSystemDefinition, Type>();


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
        public static readonly TransportSystemDefinition BALLOON = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTours, VehicleInfo.VehicleType.Balloon, TransportInfo.TransportType.HotAirBalloon);


        public static Dictionary<TransportSystemDefinition, ITLMTransportTypeExtension> AvailableDefinitions
        {
            get {
                if (m_availableDefinitions.Count == 0)
                {
                    m_availableDefinitions[BUS] = TLMTransportTypeExtensionNorBus.Instance;
                    m_availableDefinitions[METRO] = TLMTransportTypeExtensionNorMet.Instance;
                    m_availableDefinitions[TRAIN] = TLMTransportTypeExtensionNorTrn.Instance;
                    m_availableDefinitions[SHIP] = TLMTransportTypeExtensionNorShp.Instance;
                    m_availableDefinitions[PLANE] = TLMTransportTypeExtensionNorPln.Instance;

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.AfterDark))
                    //{
                    m_availableDefinitions[TAXI] = TLMTransportTypeExtensionNorTax.Instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Snowfall))
                    //{
                    m_availableDefinitions[TRAM] = TLMTransportTypeExtensionNorTrm.Instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.NaturalDisasters))
                    //{
                    m_availableDefinitions[EVAC_BUS] = TLMTransportTypeExtensionEvcBus.Instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.InMotion))
                    //{
                    m_availableDefinitions[MONORAIL] = TLMTransportTypeExtensionNorMnr.Instance;
                    m_availableDefinitions[FERRY] = TLMTransportTypeExtensionNorFer.Instance;
                    m_availableDefinitions[BLIMP] = TLMTransportTypeExtensionNorBlp.Instance;
                    m_availableDefinitions[CABLE_CAR] = TLMTransportTypeExtensionNorCcr.Instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.GreenCities))
                    //{
                    //NONE
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Parks))
                    //{
                    m_availableDefinitions[TOUR_BUS] = TLMTransportTypeExtensionTouBus.Instance;
                    m_availableDefinitions[TOUR_PED] = TLMTransportTypeExtensionTouPed.Instance;
                    m_availableDefinitions[BALLOON] = TLMTransportTypeExtensionTouBal.Instance;
                    //}
                }
                return m_availableDefinitions;
            }
        }
        public static readonly Dictionary<TransportSystemDefinition, ITLMTransportTypeExtension> m_availableDefinitions = new Dictionary<TransportSystemDefinition, ITLMTransportTypeExtension>();
        public static Dictionary<TransportSystemDefinition, Type> SysDefinitions
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

                    InitTypes(true, ref m_sysDefinitions);
                }
                return m_sysDefinitions;
            }
        }

        private static void InitTypes(bool isLoading, ref Dictionary<TransportSystemDefinition, Type> tempDef)
        {
            tempDef[BUS] = typeof(TLMSysDefNorBus);
            tempDef[METRO] = typeof(TLMSysDefNorMet);
            tempDef[TRAIN] = typeof(TLMSysDefNorTrn);
            tempDef[SHIP] = typeof(TLMSysDefNorShp);
            tempDef[PLANE] = typeof(TLMSysDefNorPln);
            //if (isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.AfterDark))
            //{
            tempDef[TAXI] = typeof(TLMSysDefNorTax);
            //}

            //if (isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Snowfall))
            //{
            tempDef[TRAM] = typeof(TLMSysDefNorTrm);
            //}

            //if (isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.NaturalDisasters))
            //{
            tempDef[EVAC_BUS] = typeof(TLMSysDefEvcBus);
            //}

            //if (isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.InMotion))
            //{
            tempDef[MONORAIL] = typeof(TLMSysDefNorMnr);
            tempDef[FERRY] = typeof(TLMSysDefNorFer);
            tempDef[BLIMP] = typeof(TLMSysDefNorBlp);
            tempDef[CABLE_CAR] = typeof(TLMSysDefNorCcr);
            //}

            //if (isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.GreenCities))
            //{
            //NONE
            //}

            //if (isLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Parks))
            //{
            tempDef[TOUR_BUS] = typeof(TLMSysDefTouBus);
            tempDef[TOUR_PED] = typeof(TLMSysDefTouPed);
            tempDef[BALLOON] = typeof(TLMSysDefTouBal);
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

        public ITLMTransportTypeExtension GetTransportExtension()
        {
            AvailableDefinitions.TryGetValue(this, out ITLMTransportTypeExtension result);
            return result;
        }
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

        public Type GetDefType() => SysDefinitions[this];

        public string GetTransportTypeIcon() => PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportType);

        public bool IsFromSystem(VehicleInfo info) => info.m_class.m_subService == SubService && info.m_vehicleType == VehicleType && ReflectionUtils.HasField(info.GetAI(), "m_transportInfo") && (info.GetAI().GetType().GetField("m_transportInfo").GetValue(info.GetAI()) as TransportInfo).m_transportType == TransportType && ReflectionUtils.HasField(info.GetAI(), "m_passengerCapacity");

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
            TransportSystemDefinition result = AvailableDefinitions.Keys.FirstOrDefault(x => x.SubService == info.m_class.m_subService && x.VehicleType == info.m_vehicleType && x.TransportType == info.m_transportType);
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
            TransportSystemDefinition result = AvailableDefinitions.Keys.FirstOrDefault(x => x.SubService == info.m_class.m_subService && x.VehicleType == info.m_vehicleType && ReflectionUtils.HasField(info.GetAI(), "m_transportInfo") && ReflectionUtils.GetPrivateField<TransportInfo>(info.GetAI(), "m_transportInfo").m_transportType == x.TransportType);
            return result;
        }
        public static TransportSystemDefinition From(uint lineId)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            return From(t.Info);
        }

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
        public IconName GetCircleSpriteName()
        {
            IconName result = IconName.IconU;
            switch (TransportType)
            {
                case TransportInfo.TransportType.Bus:
                    result = IconName.IconBus;
                    break;
                case TransportInfo.TransportType.Metro:
                    result = IconName.IconMetro;
                    break;
                case TransportInfo.TransportType.Train:
                    result = IconName.IconPassengerTrain;
                    break;
                case TransportInfo.TransportType.Ship:
                    if (VehicleType == VehicleInfo.VehicleType.Ferry)
                    {
                        result = IconName.IconFerry;
                    }
                    else
                    {
                        result = IconName.IconShip;
                    }

                    break;
                case TransportInfo.TransportType.Airplane:
                    if (VehicleType == VehicleInfo.VehicleType.Blimp)
                    {
                        result = IconName.IconBlimp;
                    }
                    else
                    {
                        result = IconName.IconAirplane;
                    }
                    break;
                case TransportInfo.TransportType.Tram:
                    result = IconName.IconTram;
                    break;
                case TransportInfo.TransportType.EvacuationBus:
                    result = IconName.IconEvacBus;
                    break;
                case TransportInfo.TransportType.Monorail:
                    result = IconName.IconMonorail;
                    break;
                case TransportInfo.TransportType.Pedestrian:
                    result = IconName.IconPath;
                    break;
                case TransportInfo.TransportType.TouristBus:
                    result = IconName.IconSightseenBus;
                    break;
            }
            return result;
        }
    }
    public abstract class TLMSysDef<T> : Singleton<T> where T : TLMSysDef<T> { public abstract TransportSystemDefinition GetTSD(); }
    public sealed class TLMSysDefNorBus : TLMSysDef<TLMSysDefNorBus> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BUS; }
    public sealed class TLMSysDefEvcBus : TLMSysDef<TLMSysDefEvcBus> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.EVAC_BUS; }
    public sealed class TLMSysDefNorTrm : TLMSysDef<TLMSysDefNorTrm> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TRAM; }
    public sealed class TLMSysDefNorMnr : TLMSysDef<TLMSysDefNorMnr> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.MONORAIL; }
    public sealed class TLMSysDefNorMet : TLMSysDef<TLMSysDefNorMet> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.METRO; }
    public sealed class TLMSysDefNorTrn : TLMSysDef<TLMSysDefNorTrn> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TRAIN; }
    public sealed class TLMSysDefNorFer : TLMSysDef<TLMSysDefNorFer> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.FERRY; }
    public sealed class TLMSysDefNorBlp : TLMSysDef<TLMSysDefNorBlp> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BLIMP; }
    public sealed class TLMSysDefNorShp : TLMSysDef<TLMSysDefNorShp> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.SHIP; }
    public sealed class TLMSysDefNorPln : TLMSysDef<TLMSysDefNorPln> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.PLANE; }
    public sealed class TLMSysDefTouBus : TLMSysDef<TLMSysDefTouBus> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TOUR_BUS; }
    public sealed class TLMSysDefTouPed : TLMSysDef<TLMSysDefTouPed> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TOUR_PED; }
    public sealed class TLMSysDefTouBal : TLMSysDef<TLMSysDefTouBal> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BALLOON; }
    public sealed class TLMSysDefNorCcr : TLMSysDef<TLMSysDefNorCcr> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.CABLE_CAR; }
    public sealed class TLMSysDefNorTax : TLMSysDef<TLMSysDefNorTax> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TAXI; }

}
