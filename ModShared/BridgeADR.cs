extern alias ADR;
using ADR::Klyte.Addresses.ModShared;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager.ModShared
{
    internal class BridgeADR : IBridgeADR
    {
        public override bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName) => AdrFacade.GetStreetAndNumber(sidewalk, midPosBuilding, out streetName, out number);
        public override bool GetStreetSuffix(Vector3 sidewalk, Vector3 midPosBuilding, out string streetName)
        {
            SegmentUtils.GetNearestSegment(sidewalk, out _, out _, out ushort targetSegmentId);
            streetName = AdrFacade.GetStreetSuffix(targetSegmentId);
            return !(streetName is null);
        }
    }
}