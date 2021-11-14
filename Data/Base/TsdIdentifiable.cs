using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.Extensions;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{
    public abstract class TsdIdentifiable : IIdentifiable
    {
        [XmlAttribute("tsdSubService")]
        public ItemClass.SubService SubService { get; set; }
        [XmlAttribute("tsdVehicleType")]
        public VehicleInfo.VehicleType VehicleType { get; set; }
        [XmlAttribute("tsdTransportType")]
        public TransportInfo.TransportType TransportType { get; set; }
        [XmlAttribute("tsdLevel")]
        public ItemClass.Level Level { get; set; }

        [XmlIgnore]
        public long? Id
        {
            get => TSD?.Id;
            set
            {
                if (value is null)
                {
                    return;
                }
                TSD = TransportSystemDefinition.FromIndex((uint)value);
            }
        }

        protected TransportSystemDefinition TSD
        {
            get => TransportSystemDefinition.From(TransportType, SubService, VehicleType, Level);
            set
            {
                SubService = value.SubService;
                VehicleType = value.VehicleType;
                TransportType = value.TransportType;
                Level = value.Level;
            }
        }
    }

}
