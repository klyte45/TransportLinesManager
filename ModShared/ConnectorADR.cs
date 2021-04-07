extern alias ADR;
using ADR::Klyte.Addresses.ModShared;
using UnityEngine;

namespace Klyte.TransportLinesManager.ModShared
{
    internal class ConnectorADR : IConnectorADR
    {
        public override bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName) => AdrShared.GetStreetAndNumber(sidewalk, midPosBuilding, out streetName, out number);
    }
}