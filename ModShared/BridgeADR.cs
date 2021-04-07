extern alias ADR;
using ADR::Klyte.Addresses.ModShared;
using UnityEngine;

namespace Klyte.TransportLinesManager.ModShared
{
    internal class BridgeADR : IBridgeADR
    {
        public override bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName) => AdrFacade.GetStreetAndNumber(sidewalk, midPosBuilding, out streetName, out number);
    }
}