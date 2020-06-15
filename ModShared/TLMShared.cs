using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.ModShared
{
    public class TLMShared : MonoBehaviour
    {
        public static TLMShared Instance => TransportLinesManagerMod.Controller?.SharedInstance;

        internal void OnLineSymbolParameterChanged() => EventLineSymbolParameterChanged?.Invoke();

        public event Action EventLineSymbolParameterChanged;
    }
}