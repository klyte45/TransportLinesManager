extern alias ADR;
using UnityEngine;

namespace Klyte.TransportLinesManager.ModShared
{
    internal abstract class IBridgeWTS : MonoBehaviour
    {
        public abstract bool WtsAvailable { get; }
    }
}