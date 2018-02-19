using ColossalFramework;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    internal class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition BUS = new TransportSystemDefinition(ItemClass.SubService.PublicTransportBus, VehicleInfo.VehicleType.Car);
        public static readonly TransportSystemDefinition TRAM = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTram, VehicleInfo.VehicleType.Tram);
        public static readonly TransportSystemDefinition METRO = new TransportSystemDefinition(ItemClass.SubService.PublicTransportMetro, VehicleInfo.VehicleType.Metro);
        public static readonly TransportSystemDefinition MONORAIL = new TransportSystemDefinition(ItemClass.SubService.PublicTransportMonorail, VehicleInfo.VehicleType.Monorail);
        public static readonly TransportSystemDefinition TRAIN = new TransportSystemDefinition(ItemClass.SubService.PublicTransportTrain, VehicleInfo.VehicleType.Train);
        public static readonly TransportSystemDefinition FERRY = new TransportSystemDefinition(ItemClass.SubService.PublicTransportShip, VehicleInfo.VehicleType.Ferry);
        public static readonly TransportSystemDefinition BLIMP = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPlane, VehicleInfo.VehicleType.Blimp);
        public static readonly TransportSystemDefinition SHIP = new TransportSystemDefinition(ItemClass.SubService.PublicTransportShip, VehicleInfo.VehicleType.Ship);
        public static readonly TransportSystemDefinition PLANE = new TransportSystemDefinition(ItemClass.SubService.PublicTransportPlane, VehicleInfo.VehicleType.Plane);
        public static readonly TransportSystemDefinition EVAC_BUS = new TransportSystemDefinition(ItemClass.SubService.None, VehicleInfo.VehicleType.Car);
        public static readonly List<TransportSystemDefinition> availableDefinitions = new List<TransportSystemDefinition>(new TransportSystemDefinition[] { EVAC_BUS, BUS, TRAM, METRO, MONORAIL, TRAIN, FERRY, BLIMP,  SHIP, PLANE });

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

        public ITLMTransportTypeExtension GetTransportExtension()
        {
            if (this == BUS) { return TLMTransportTypeExtensionBus.instance; }
            if (this == EVAC_BUS) { return TLMTransportTypeExtensionEvacBus.instance; }
            if (this == TRAM) { return TLMTransportTypeExtensionTram.instance; }
            if (this == TRAIN) { return TLMTransportTypeExtensionTrain.instance; }
            if (this == METRO) { return TLMTransportTypeExtensionMetro.instance; }
            if (this == MONORAIL) { return TLMTransportTypeExtensionMonorail.instance; }
            if (this == FERRY) { return TLMTransportTypeExtensionFerry.instance; }
            if (this == BLIMP) { return TLMTransportTypeExtensionBlimp.instance; }
            if (this == SHIP) { return TLMTransportTypeExtensionShip.instance; }
            if (this == PLANE) { return TLMTransportTypeExtensionPlane.instance; }
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
        public static TransportSystemDefinition from(uint lineId)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            return from(t.Info);
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

    internal abstract class TLMSysDef : Singleton<TLMSysDef> { public abstract TransportSystemDefinition GetTSD(); }
    internal sealed class TLMSysDefBus : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.BUS; } }
    internal sealed class TLMSysDefEvacBus : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.EVAC_BUS; } }
    internal sealed class TLMSysDefTram : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.TRAM; } }
    internal sealed class TLMSysDefMonorail : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.MONORAIL; } }
    internal sealed class TLMSysDefMetro : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.METRO; } }
    internal sealed class TLMSysDefTrain : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.TRAIN; } }
    internal sealed class TLMSysDefFerry : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.FERRY; } }
    internal sealed class TLMSysDefBlimp : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.BLIMP; } }
    internal sealed class TLMSysDefShip : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.SHIP; } }
    internal sealed class TLMSysDefPlane : TLMSysDef { public override TransportSystemDefinition GetTSD() { return TransportSystemDefinition.PLANE; } }
}
