using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TAXI = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTaxi,
            VehicleInfo.VehicleType.Car,
            TransportInfo.TransportType.Taxi,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Taxi },
            new Color32(60, 184, 120, 255),
            1,
            LineIconSpriteNames.K45_TriangleIcon,
            false);
    }

}
