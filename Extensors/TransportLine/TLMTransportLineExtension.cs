using ColossalFramework;
using ColossalFramework.Threading;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Extensors
{

    enum TLMTransportLineFlags
    {
        ZERO_BUDGET_DAY = 0x40000000,
        ZERO_BUDGET_NIGHT = 0x20000000,
        ZERO_BUDGET_SETTED = 0x10000000
    }

    enum TLMTransportLineExtensionsKey
    {
        IGNORE_BUDGET_PREFIX
    }

    class TLMTransportLineExtensions : ExtensionInterfaceDefaultImpl<TLMTransportLineExtensionsKey, TLMTransportLineExtensions>
    {
        protected override TLMCW.ConfigIndex ConfigIndexKey => TLMCW.ConfigIndex.LINES_CONFIG;

        public void SetIgnorePrefixBudget(ushort lineId, bool value)
        {
            SafeSet(lineId, TLMTransportLineExtensionsKey.IGNORE_BUDGET_PREFIX, value.ToString());
        }

        public bool GetIgnorePrefixBudget(ushort lineId)
        {
            return Boolean.TryParse(SafeGet(lineId, TLMTransportLineExtensionsKey.IGNORE_BUDGET_PREFIX), out bool result) && result;
        }
    }
}
