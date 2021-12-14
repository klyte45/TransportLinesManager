using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TOUR_PED = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTours,
            VehicleInfo.VehicleType.None,
            TransportInfo.TransportType.Pedestrian,
            ItemClass.Level.Level5,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.TouristA, TransferManager.TransferReason.TouristB, TransferManager.TransferReason.TouristC, TransferManager.TransferReason.TouristD },
            new Color32(83, 157, 48, 255),
            1,
            LineIconSpriteNames.K45_MountainIcon,
            true);
    }

}
