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

namespace Klyte.TransportLinesManager.Extensors
{

    class TLMStopsExtension : ExtensionInterfaceDefaultImpl<TLMStopExtensionProperty, TLMStopsExtension>
    {
        protected override string KvSepLvl1 { get { return "∂"; } }
        protected override string ItSepLvl1 { get { return "§"; } }
        protected override string KvSepLvl2 { get { return "∫"; } }
        protected override string ItSepLvl2 { get { return "≠"; } }
        protected override TLMConfigWarehouse.ConfigIndex ConfigIndexKey { get { return TLMConfigWarehouse.ConfigIndex.STOPS_CONFIG; } }
        
        public string getStopName(uint stopId, ushort lineId)
        {
            if (cachedValues == null) Load();
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getStopName(): Params: {0}, {1}", stopId, lineId);

            if (cachedValues.ContainsKey(stopId) && cachedValues[stopId].ContainsKey(TLMStopExtensionProperty.STOP_LINE_ID) && cachedValues[stopId].ContainsKey(TLMStopExtensionProperty.STOP_NAME) && lineId == ushort.Parse(cachedValues[stopId][TLMStopExtensionProperty.STOP_LINE_ID]))
            {
                return cachedValues[stopId][TLMStopExtensionProperty.STOP_NAME];
            }
            else
            {
                return null;
            }
        }

        public void setStopName(string newName, uint stopId, ushort lineId)
        {
            if (cachedValues == null) Load();
            if (!cachedValues.ContainsKey(stopId))
            {
                cachedValues[stopId] = new Dictionary<TLMStopExtensionProperty, string>();
            }
            if (string.IsNullOrEmpty(newName))
            {
                cachedValues.Remove(stopId);
            }
            else
            {
                cachedValues[stopId][TLMStopExtensionProperty.STOP_NAME] = newName;
                cachedValues[stopId][TLMStopExtensionProperty.STOP_LINE_ID] = lineId.ToString();
            }
            Save();
            Load();
        }

        public void cleanStopInfo(uint stopId, ushort lineId)
        {
            if (cachedValues == null) Load();
            if (cachedValues.ContainsKey(stopId))
            {
                cachedValues.Remove(stopId);
                Save();
                Load();
            }
        }

    }

    internal enum TLMStopExtensionProperty
    {
        STOP_NAME,
        STOP_LINE_ID
    }

}
