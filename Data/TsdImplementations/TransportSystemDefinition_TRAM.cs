using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TRAM = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTram,
            VehicleInfo.VehicleType.Tram,
            TransportInfo.TransportType.Tram,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Tram },
            new Color32(73, 27, 137, 255),
            90,
            LineIconSpriteNames.K45_TrapezeIcon);
    }

}
