using Klyte.Commons.UI.Sprites;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TROLLEY = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTrolleybus,
                    VehicleInfo.VehicleType.Trolleybus,
                    TransportInfo.TransportType.Trolleybus,
                    ItemClass.Level.Level1,
                    new TransferManager.TransferReason[] { TransferManager.TransferReason.Trolleybus },
                    new Color(1, .517f, 0, 1),
                    30,
                    LineIconSpriteNames.K45_OvalIcon,
                    TransportDefinitionFlags.CanChangeTicketPrice
                );
    }

}
