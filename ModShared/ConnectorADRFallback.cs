extern alias ADR;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager.ModShared
{
    internal class ConnectorADRFallback : IConnectorADR
    {
        public override bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName) => SegmentUtils.GetBasicAddressStreetAndNumber(sidewalk, midPosBuilding, out number, out streetName);
    }
}