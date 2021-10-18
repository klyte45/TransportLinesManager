using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition PLANE = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportPlane,
            VehicleInfo.VehicleType.Plane,
            TransportInfo.TransportType.Airplane,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerPlane },
            new Color32(0xa8, 0x01, 0x7a, 255),
            200,
            LineIconSpriteNames.K45_PentagonIcon,
            true);
    }

}
