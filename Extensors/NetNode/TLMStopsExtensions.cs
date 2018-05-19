using ColossalFramework;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Extensors.NetNodeExt
{

    class TLMStopsExtension : ExtensionInterfaceDefaultImpl<TLMStopExtensionProperty, TLMStopsExtension>
    {
        protected override string KvSepLvl1 { get { return "∂"; } }
        protected override string ItSepLvl1 { get { return "§"; } }
        protected override string KvSepLvl2 { get { return "∫"; } }
        protected override string ItSepLvl2 { get { return "≠"; } }
        protected override TLMConfigWarehouse.ConfigIndex ConfigIndexKey { get { return TLMConfigWarehouse.ConfigIndex.STOPS_CONFIG; } }


        public string GetStopName(uint stopId)
        {
            return SafeGet(stopId, TLMStopExtensionProperty.STOP_NAME);
        }

        public void SetStopName(string newName, uint stopId)
        {
            if (string.IsNullOrEmpty(newName?.Trim()))
            {
                SafeCleanEntry(stopId);
            }
            else
            {
                SafeSet(stopId, TLMStopExtensionProperty.STOP_NAME, newName.Trim());
            }
        }
    }

    internal enum TLMStopExtensionProperty
    {
        STOP_NAME
    }

}
