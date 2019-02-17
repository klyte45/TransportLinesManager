using ColossalFramework;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    internal struct TransportSystemDefinition
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


        public static Dictionary<TransportSystemDefinition, ITLMTransportTypeExtension> availableDefinitions
        {
            get {
                if (m_availableDefinitions.Count == 0)
                {
                    m_availableDefinitions[BUS] = TLMTransportTypeExtensionNorBus.instance;
                    m_availableDefinitions[METRO] = TLMTransportTypeExtensionNorMet.instance;
                    m_availableDefinitions[TRAIN] = TLMTransportTypeExtensionNorTrn.instance;
                    m_availableDefinitions[SHIP] = TLMTransportTypeExtensionNorShp.instance;
                    m_availableDefinitions[PLANE] = TLMTransportTypeExtensionNorPln.instance;

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.AfterDark))
                    //{
                    m_availableDefinitions[TAXI] = TLMTransportTypeExtensionNorTax.instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Snowfall))
                    //{
                    m_availableDefinitions[TRAM] = TLMTransportTypeExtensionNorTrm.instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.NaturalDisasters))
                    //{
                    m_availableDefinitions[EVAC_BUS] = TLMTransportTypeExtensionEvcBus.instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.InMotion))
                    //{
                    m_availableDefinitions[MONORAIL] = TLMTransportTypeExtensionNorMnr.instance;
                    m_availableDefinitions[FERRY] = TLMTransportTypeExtensionNorFer.instance;
                    m_availableDefinitions[BLIMP] = TLMTransportTypeExtensionNorBlp.instance;
                    m_availableDefinitions[CABLE_CAR] = TLMTransportTypeExtensionNorCcr.instance;
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.GreenCities))
                    //{
                    //NONE
                    //}

                    //if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Parks))
                    //{
                    m_availableDefinitions[TOUR_BUS] = TLMTransportTypeExtensionTouBus.instance;
                    m_availableDefinitions[TOUR_PED] = TLMTransportTypeExtensionTouPed.instance;
                    m_availableDefinitions[BALLOON] = TLMTransportTypeExtensionTouBal.instance;
                    //}
                }
                return m_availableDefinitions;
            }
        }
        public static readonly Dictionary<TransportSystemDefinition, ITLMTransportTypeExtension> m_availableDefinitions = new Dictionary<TransportSystemDefinition, ITLMTransportTypeExtension>();
        public static Dictionary<TransportSystemDefinition, Type> sysDefinitions
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

        public ItemClass.SubService subService
        {
            get;
        }
        public VehicleInfo.VehicleType vehicleType
        {
            get;
        }
        public TransportInfo.TransportType transportType
        {
            get;
        }

        private TransportSystemDefinition(
        ItemClass.SubService subService,
        VehicleInfo.VehicleType vehicleType,
        TransportInfo.TransportType transportType)
        {
            this.vehicleType = vehicleType;
            this.subService = subService;
            this.transportType = transportType;
        }

        public ITLMTransportTypeExtension GetTransportExtension()
        {
            availableDefinitions.TryGetValue(this, out ITLMTransportTypeExtension result);
            return result;
        }
        public bool isTour()
        {
            return subService == ItemClass.SubService.PublicTransportTours;
        }
        public bool isShelterAiDepot()
        {
            return this == EVAC_BUS;
        }
        public bool hasVehicles()
        {
            return vehicleType != VehicleInfo.VehicleType.None;
        }
        public bool isPrefixable()
        {
            switch (transportType)
            {
                case TransportInfo.TransportType.HotAirBalloon:
                case TransportInfo.TransportType.Taxi:
                case TransportInfo.TransportType.CableCar:
                case TransportInfo.TransportType.Pedestrian:
                case TransportInfo.TransportType.EvacuationBus:
                    return false;
                default: return true;
            }
        }

        internal Type GetDefType()
        {
            return sysDefinitions[this];
        }

        public String getTransportTypeIcon()
        {
            return PublicTransportWorldInfoPanel.GetVehicleTypeIcon(transportType);
        }

        public bool isFromSystem(VehicleInfo info)
        {
            return info.m_class.m_subService == subService && info.m_vehicleType == vehicleType && TLMUtils.HasField(info.GetAI(), "m_transportInfo") && TLMUtils.GetPrivateField<TransportInfo>(info.GetAI(), "m_transportInfo").m_transportType == transportType && TLMUtils.HasField(info.GetAI(), "m_passengerCapacity");
        }

        public bool isFromSystem(TransportInfo info)
        {
            return info != null && info.m_class.m_subService == subService && info.m_vehicleType == vehicleType && info.m_transportType == transportType;
        }

        public bool isFromSystem(DepotAI p)
        {
            return p != null && ((p.m_info.m_class.m_subService == subService && p.m_transportInfo.m_vehicleType == vehicleType && p.m_maxVehicleCount > 0 && p.m_transportInfo.m_transportType == transportType)
                || (p.m_secondaryTransportInfo != null && p.m_secondaryTransportInfo.m_vehicleType == vehicleType && p.m_maxVehicleCount2 > 0 && p.m_secondaryTransportInfo.m_transportType == transportType));
        }
        public bool isFromSystem(TransportLine tl)
        {
            return (tl.Info.m_class.m_subService == subService && tl.Info.m_vehicleType == vehicleType && tl.Info.m_transportType == transportType);
        }

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
            TransportSystemDefinition other = (TransportSystemDefinition)obj;

            return this.subService == other.subService && this.vehicleType == other.vehicleType && this.transportType == other.transportType;
        }

        public static bool operator ==(TransportSystemDefinition a, TransportSystemDefinition b)
        {
            if (Object.Equals(a, null) || Object.Equals(b, null))
            {
                return Object.Equals(a, null) == Object.Equals(b, null);
            }
            return a.Equals(b);
        }
        public static bool operator !=(TransportSystemDefinition a, TransportSystemDefinition b)
        {
            return !(a == b);
        }

        public static TransportSystemDefinition from(PrefabAI buildingAI)
        {
            DepotAI depotAI = buildingAI as DepotAI;
            if (depotAI == null)
            {
                return default(TransportSystemDefinition);
            }
            return from(depotAI.m_transportInfo);
        }

        public static TransportSystemDefinition from(TransportInfo info)
        {
            if (info == null)
            {
                return default(TransportSystemDefinition);
            }
            var result = availableDefinitions.Keys.FirstOrDefault(x => x.subService == info.m_class.m_subService && x.vehicleType == info.m_vehicleType && x.transportType == info.m_transportType);
            if (result == default(TransportSystemDefinition))
            {
                TLMUtils.doErrorLog($"TSD NOT FOUND FOR TRANSPORT INFO: info.m_class.m_subService={info.m_class.m_subService}, info.m_vehicleType={info.m_vehicleType}, info.m_transportType={info.m_transportType}");
            }
            return result;
        }
        public static TransportSystemDefinition from(VehicleInfo info)
        {
            if (info == null)
            {
                return default(TransportSystemDefinition);
            }
            var result = availableDefinitions.Keys.FirstOrDefault(x => x.subService == info.m_class.m_subService && x.vehicleType == info.m_vehicleType && TLMUtils.HasField(info.GetAI(), "m_transportInfo") && TLMUtils.GetPrivateField<TransportInfo>(info.GetAI(), "m_transportInfo").m_transportType == x.transportType);
            return result;
        }
        public static TransportSystemDefinition from(uint lineId)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            return from(t.Info);
        }

        public static TransportSystemDefinition getDefinitionForLine(ushort i) => getDefinitionForLine(ref Singleton<TransportManager>.instance.m_lines.m_buffer[i]);
        public static TransportSystemDefinition getDefinitionForLine(ref TransportLine t) => from(t.Info);

        public TLMConfigWarehouse.ConfigIndex toConfigIndex()
        {
            return TLMConfigWarehouse.getConfigTransportSystemForDefinition(ref this);
        }

        public override string ToString()
        {
            return subService.ToString() + "|" + vehicleType.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = 286451371;
            hashCode = hashCode * -1521134295 + subService.GetHashCode();
            hashCode = hashCode * -1521134295 + vehicleType.GetHashCode();
            return hashCode;
        }
    }

    internal abstract class TLMSysDef<T> : Singleton<T> where T : TLMSysDef<T> { public abstract TransportSystemDefinition GetTSD(); }
    internal sealed class TLMSysDefNorBus : TLMSysDef<TLMSysDefNorBus> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BUS; }
    internal sealed class TLMSysDefEvcBus : TLMSysDef<TLMSysDefEvcBus> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.EVAC_BUS; }
    internal sealed class TLMSysDefNorTrm : TLMSysDef<TLMSysDefNorTrm> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TRAM; }
    internal sealed class TLMSysDefNorMnr : TLMSysDef<TLMSysDefNorMnr> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.MONORAIL; }
    internal sealed class TLMSysDefNorMet : TLMSysDef<TLMSysDefNorMet> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.METRO; }
    internal sealed class TLMSysDefNorTrn : TLMSysDef<TLMSysDefNorTrn> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TRAIN; }
    internal sealed class TLMSysDefNorFer : TLMSysDef<TLMSysDefNorFer> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.FERRY; }
    internal sealed class TLMSysDefNorBlp : TLMSysDef<TLMSysDefNorBlp> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BLIMP; }
    internal sealed class TLMSysDefNorShp : TLMSysDef<TLMSysDefNorShp> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.SHIP; }
    internal sealed class TLMSysDefNorPln : TLMSysDef<TLMSysDefNorPln> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.PLANE; }
    internal sealed class TLMSysDefTouBus : TLMSysDef<TLMSysDefTouBus> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TOUR_BUS; }
    internal sealed class TLMSysDefTouPed : TLMSysDef<TLMSysDefTouPed> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TOUR_PED; }
    internal sealed class TLMSysDefTouBal : TLMSysDef<TLMSysDefTouBal> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.BALLOON; }
    internal sealed class TLMSysDefNorCcr : TLMSysDef<TLMSysDefNorCcr> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.CABLE_CAR; }
    internal sealed class TLMSysDefNorTax : TLMSysDef<TLMSysDefNorTax> { public override TransportSystemDefinition GetTSD() => TransportSystemDefinition.TAXI; }

}
