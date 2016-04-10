using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Extensors
{

    class TLMStopsExtension
    {
        private static TLMStopsExtension _instance;
        public static TLMStopsExtension instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMStopsExtension();
                }
                return _instance;
            }
        }

        private const string SEPARATOR = "∂";
        private const string COMMA = "§";
        private const string SUBSEPARATOR = "∫";
        private const string SUBCOMMA = "≠";
        private const TLMConfigWarehouse.ConfigIndex CONFIG = TLMConfigWarehouse.ConfigIndex.STOPS_CONFIG;
        private Dictionary<uint, Dictionary<Property, string>> cached_list = null;

        public void load()
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("TLMStopsExtension.load()");
            var file = TLMConfigWarehouse.getCurrentConfigString(CONFIG).Split(COMMA.ToCharArray());
            cached_list = new Dictionary<uint, Dictionary<Property, string>>();
            if (file.Length > 0)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("TLMStopsExtension.load(): file.Length > 0");
                foreach (string s in file)
                {
                    uint key = getIndexFromStringArray(s);
                    var value = TLMUtils.getValueFromStringArray<Property>(s, SEPARATOR, SUBCOMMA, SUBSEPARATOR);
                    cached_list[key] = value;
                }
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("TLMStopsExtension.load(): dic done");
                cached_list.Remove(0);
            }
        }

        private uint getIndexFromStringArray(string x)
        {
            uint saida;
            if (uint.TryParse(x.Split(SEPARATOR.ToCharArray())[0], out saida))
            {
                return saida;
            }
            return 0;
        }

        public string getStopName(uint stopId, ushort lineId)
        {
            if (cached_list == null) load();
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getStopName(): Params: {0}, {1}", stopId, lineId);

            if (cached_list.ContainsKey(stopId) && cached_list[stopId].ContainsKey(Property.STOP_LINE_ID) && cached_list[stopId].ContainsKey(Property.STOP_NAME) && lineId == ushort.Parse(cached_list[stopId][Property.STOP_LINE_ID]))
            {
                return cached_list[stopId][Property.STOP_NAME];
            }
            else
            {
                return null;
            }
        }

        public void setStopName(string newName, uint stopId, ushort lineId)
        {
            if (cached_list == null) load();
            if (!cached_list.ContainsKey(stopId))
            {
                cached_list[stopId] = new Dictionary<Property, string>();
            }
            if (string.IsNullOrEmpty(newName))
            {
                cached_list.Remove(stopId);
            }
            else
            {
                cached_list[stopId][Property.STOP_NAME] = newName;
                cached_list[stopId][Property.STOP_LINE_ID] = lineId.ToString();
            }
            saveStops();
            load();
        }

        public void cleanStopInfo(uint stopId, ushort lineId)
        {
            if (cached_list == null) load();
            if (cached_list.ContainsKey(stopId))
            {
                cached_list.Remove(stopId);
                saveStops();
                load();
            }
        }

        private void saveStops()
        {
            TLMConfigWarehouse loadedConfig = TransportLinesManagerMod.instance.currentLoadedCityConfig;
            var value = string.Join(COMMA, cached_list.Select(x => x.Key.ToString() + SEPARATOR + string.Join(SUBCOMMA, x.Value.Select(y => string.Format("{0}{1}{2}", y.Key.ToString(), SUBSEPARATOR, y.Value)).ToArray())).ToArray());
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("saveStops NEW VALUE: {0}", value);
            loadedConfig.setString(TLMConfigWarehouse.ConfigIndex.STOPS_CONFIG, value);
        }

        private enum Property
        {
            STOP_NAME,
            STOP_LINE_ID
        }
    }

}
