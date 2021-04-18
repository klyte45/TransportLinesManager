using Klyte.Commons.UI.Sprites;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition INTERCITY_BUS = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportBus,
                    VehicleInfo.VehicleType.Car,
                    TransportInfo.TransportType.Bus,
                    ItemClass.Level.Level3,
                    new TransferManager.TransferReason[] { TransferManager.TransferReason.IntercityBus },
                    new Color32(23, 91, 128, 255),
                    50,
                    LineIconSpriteNames.K45_OctagonIcon,
                    0
                );
    }

}
