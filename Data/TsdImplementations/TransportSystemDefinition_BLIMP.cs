using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition BLIMP = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportPlane,
            VehicleInfo.VehicleType.Blimp,
            TransportInfo.TransportType.Airplane,
            ItemClass.Level.Level2,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Blimp },
            new Color32(0xd8, 0x01, 0xaa, 255),
            35,
            LineIconSpriteNames.K45_ParachuteIcon,
            true);
    }

}
