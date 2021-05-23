extern alias ADR;

namespace Klyte.TransportLinesManager.ModShared
{
    internal class BridgeWTSFallback : IBridgeWTS
    {
        public override bool WtsAvailable { get; } = false;
    }
}