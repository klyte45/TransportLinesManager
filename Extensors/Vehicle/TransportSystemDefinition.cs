using ColossalFramework;
using Klyte.TransportLinesManager.Extensors.VehicleAIExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Klyte.TransportLinesManager.Extensors
{
    public class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition BUS = new TransportSystemDefinition(ItemClass.SubService.PublicTransportBus, VehicleInfo.VehicleType.Car);
        public static readonly TransportSystemDefinition TRAM = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTram, VehicleInfo.VehicleType.Tram);
        public static readonly TransportSystemDefinition METRO = new TransportSystemDefinition(ItemClass.SubService.PublicTransportMetro, VehicleInfo.VehicleType.Metro);
        public static readonly TransportSystemDefinition MONORAIL = new TransportSystemDefinition(ItemClass.SubService.PublicTransportMonorail, VehicleInfo.VehicleType.Monorail);
        public static readonly TransportSystemDefinition TRAIN = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTrain, VehicleInfo.VehicleType.Train);
        public static readonly TransportSystemDefinition FERRY = new TransportSystemDefinition(ItemClass.SubService.PublicTransportShip, VehicleInfo.VehicleType.Ferry);
        public static readonly TransportSystemDefinition BLIMP = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPlane, VehicleInfo.VehicleType.Blimp);
        public static readonly TransportSystemDefinition CABLE_CAR = new TransportSystemDefinition(ItemClass.SubService.PublicTransportCableCar, VehicleInfo.VehicleType.CableCar);
        public static readonly TransportSystemDefinition SHIP = new TransportSystemDefinition(ItemClass.SubService.PublicTransportShip, VehicleInfo.VehicleType.Ship);
        public static readonly TransportSystemDefinition PLANE = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPlane, VehicleInfo.VehicleType.Plane);
        public static readonly TransportSystemDefinition TAXI = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTaxi, VehicleInfo.VehicleType.Car);
        public static readonly TransportSystemDefinition EVAC_BUS = new TransportSystemDefinition(ItemClass.SubService.None, VehicleInfo.VehicleType.Car);
        public static readonly List<TransportSystemDefinition> availableDefinitions = new List<TransportSystemDefinition>(new TransportSystemDefinition[] { EVAC_BUS, BUS, TRAM, METRO, MONORAIL, TRAIN, FERRY, BLIMP, CABLE_CAR, SHIP, PLANE, TAXI });

        public ItemClass.SubService subService
        {
            get;
        }
        public VehicleInfo.VehicleType vehicleType
        {
            get;
        }

        private TransportSystemDefinition(
        ItemClass.SubService subservice,
        VehicleInfo.VehicleType vehicleType)
        {
            this.vehicleType = vehicleType;
            this.subService = subservice;
        }

        public ITLMTransportExtension GetTransportExtension()
        {
            if (this == BUS) { return TLMTransportExtensionBus.instance; }
            if (this == EVAC_BUS) { return TLMTransportExtensionEvacBus.instance; }
            if (this == TRAM) { return TLMTransportExtensionTram.instance; }
            if (this == TRAIN) { return TLMTransportExtensionTrain.instance; }
            if (this == METRO) { return TLMTransportExtensionMetro.instance; }
            if (this == MONORAIL) { return TLMTransportExtensionMonorail.instance; }
            if (this == FERRY) { return TLMTransportExtensionFerry.instance; }
            if (this == BLIMP) { return TLMTransportExtensionBlimp.instance; }
            if (this == SHIP) { return TLMTransportExtensionShip.instance; }
            if (this == PLANE) { return TLMTransportExtensionPlane.instance; }
            return null;

        }

        public bool isFromSystem(VehicleInfo info)
        {
            return info.m_class.m_subService == subService && info.m_vehicleType == vehicleType && TLMUtils.HasField(info.GetAI(), "m_passengerCapacity");
        }

        public bool isFromSystem(DepotAI p)
        {
            return (p.m_info.m_class.m_subService == subService && p.m_transportInfo.m_vehicleType == vehicleType && p.m_maxVehicleCount > 0)
                || (p.m_secondaryTransportInfo != null && p.m_secondaryTransportInfo.m_vehicleType == vehicleType && p.m_maxVehicleCount2 > 0);
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

            return this.subService == other.subService && this.vehicleType == other.vehicleType;
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
                return null;
            }
            return from(depotAI.m_transportInfo);
        }

        public static TransportSystemDefinition from(TransportInfo info)
        {
            if (info == null)
            {
                return default(TransportSystemDefinition);
            }
            return availableDefinitions.FirstOrDefault(x => x.subService == info.m_class.m_subService && x.vehicleType == info.m_vehicleType);
        }
        public static TransportSystemDefinition from(VehicleInfo info)
        {
            if (info == null)
            {
                return default(TransportSystemDefinition);
            }
            return availableDefinitions.FirstOrDefault(x => x.subService == info.m_class.m_subService && x.vehicleType == info.m_vehicleType);
        }

        public TLMConfigWarehouse.ConfigIndex toConfigIndex()
        {
            return TLMConfigWarehouse.getConfigTransportSystemForDefinition(this);
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

    public abstract class TLMSysDef : Singleton<TLMSysDef> { public abstract TransportSystemDefinition GetTSD(); }
    public sealed class TLMSysDefBus : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.BUS; } }
    public sealed class TLMSysDefEvacBus : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.EVAC_BUS; } }
    public sealed class TLMSysDefTram : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.TRAM; } }
    public sealed class TLMSysDefMonorail : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.MONORAIL; } }
    public sealed class TLMSysDefMetro : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.METRO; } }
    public sealed class TLMSysDefTrain : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.TRAIN; } }
    public sealed class TLMSysDefFerry : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.FERRY; } }
    public sealed class TLMSysDefBlimp : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.BLIMP; } }
    public sealed class TLMSysDefShip : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.SHIP; } }
    public sealed class TLMSysDefPlane : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.PLANE; } }
}
