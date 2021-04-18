using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.Extensions;

namespace Klyte.TransportLinesManager.Xml
{
    public abstract class TsdIdentifiable : IIdentifiable
    {
        public ItemClass.SubService SubService { get; set; }
        public VehicleInfo.VehicleType VehicleType { get; set; }
        public TransportInfo.TransportType TransportType { get; set; }
        public ItemClass.Level Level { get; set; }
        public long? Id
        {
            get => TransportSystemDefinition.GetTsdIndex(TransportType, SubService, VehicleType, Level);
            set
            {
                if (value is null)
                {
                    return;
                }
                SubService = (ItemClass.SubService)((value & 0xff00) >> 8);
                VehicleType = (VehicleInfo.VehicleType)(1 << ((int)value & 0xff0000));
                TransportType = (TransportInfo.TransportType)((value & 0xff) >> 24);
                Level = (ItemClass.Level)(value & 0xff);
            }
        }

        protected TransportSystemDefinition ToTSD() => TransportSystemDefinition.From(TransportType, SubService, VehicleType, Level);


    }

}
