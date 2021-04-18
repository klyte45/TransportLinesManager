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
            get
            {
                var val = TransportSystemDefinition.GetTsdIndex(TransportType, SubService, VehicleType, Level);
                return val == 0 ? null : (long?)val;
            }

            set
            {
                if (value is null)
                {
                    return;
                }
                var longVal = value ?? 0L;
                SubService = (ItemClass.SubService)((longVal & 0xff00) >> 8);
                VehicleType = (VehicleInfo.VehicleType)(((int)longVal & 0xff0000) == 0 ? 0 : (1 << ((((int)longVal & 0xff0000) >> 16) - 1)));
                TransportType = (TransportInfo.TransportType)((longVal & 0xff000000) >> 24);
                Level = (ItemClass.Level)(longVal & 0xff);
            }
        }

        protected TransportSystemDefinition ToTSD() => TransportSystemDefinition.From(TransportType, SubService, VehicleType, Level);


    }

}
