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
        public static readonly TransportSystemDefinition[] availableDefinitions = { BUS, TRAM, METRO, MONORAIL, TRAIN, FERRY, BLIMP, CABLE_CAR, SHIP, PLANE, TAXI };

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

        public bool isFromSystem(VehicleInfo info)
        {
            return info.m_class.m_subService == subService && info.m_vehicleType == vehicleType && TLMUtils.HasField(info.GetAI(), "m_passengerCapacity");
        }

        public bool isFromSystem(DepotAI p)
        {
            return p.m_info.m_class.m_subService == subService && p.m_transportInfo.m_vehicleType == vehicleType;
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
        public static TransportSystemDefinition from(ItemClass.SubService subService, VehicleInfo.VehicleType vehicleType)
        {
            var item = availableDefinitions.FirstOrDefault(x => x.subService == subService && x.vehicleType == vehicleType);
            if (item == default(TransportSystemDefinition))
            {
                TLMUtils.doErrorLog("TSD NOT FOUND!!! {0}-{1}", subService, vehicleType);
            }
            return item;
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
            return availableDefinitions.FirstOrDefault(x => x.subService == info.m_class.m_subService && x.vehicleType == info.m_vehicleType);
        }
        public static TransportSystemDefinition from(VehicleInfo info)
        {
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
    }
}
