using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition BUS = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportBus,
            VehicleInfo.VehicleType.Car,
            TransportInfo.TransportType.Bus,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Bus },
            new Color32(53, 121, 188, 255),
            30,
            LineIconSpriteNames.K45_HexagonIcon,
            true,
            ItemClass.Level.Level3,
            ItemClass.Level.Level2
        );
    }

}
