using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition FERRY = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportShip,
            VehicleInfo.VehicleType.Ferry,
            TransportInfo.TransportType.Ship,
            ItemClass.Level.Level2,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Ferry },
            new Color32(0xe3, 0xf0, 0, 255),
            50,
            LineIconSpriteNames.K45_S08StarIcon);
    }

}
