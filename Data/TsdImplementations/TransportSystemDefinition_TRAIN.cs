using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition TRAIN = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTrain,
            VehicleInfo.VehicleType.Train,
            TransportInfo.TransportType.Train,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerTrain },
            new Color32(250, 104, 0, 255),
            240,
            LineIconSpriteNames.K45_CircleIcon,
            true);
    }

}
