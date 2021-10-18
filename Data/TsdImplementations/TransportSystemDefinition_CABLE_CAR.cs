using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition CABLE_CAR = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportCableCar,
            VehicleInfo.VehicleType.CableCar,
            TransportInfo.TransportType.CableCar,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.CableCar },
            new Color32(31, 96, 225, 255),
            1,
            LineIconSpriteNames.K45_ConeIcon,
            false);
    }

}
