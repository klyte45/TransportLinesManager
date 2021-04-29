using Klyte.TransportLinesManager.Interfaces;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition BALLOON = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportTours,
            VehicleInfo.VehicleType.None,
            TransportInfo.TransportType.HotAirBalloon,
            ItemClass.Level.Level4,
            new TransferManager.TransferReason[]
                {
                    TransferManager.TransferReason.TouristA,
                    TransferManager.TransferReason.TouristB,
                    TransferManager.TransferReason.TouristC,
                    TransferManager.TransferReason.TouristD
                },
            default,
            1,
            default);
     
    }

}
