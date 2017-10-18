using ColossalFramework;
using ColossalFramework.Threading;
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

    class TLMVehiclesLineManager
    {
        private static TLMVehiclesLineManager _instance;
        public static TLMVehiclesLineManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMVehiclesLineManager();
                }
                return _instance;
            }
        }

        private const string SEPARATOR = "∂";
        private const string COMMA = "§";
        private Dictionary<ushort, int> cached_list;

        public int this[ushort i]
        {
            get { return getVehicleCountForLine(i); }
            set { setVehicleCountForLine(i, value); }
        }

        public int getVehicleCountForLine(ushort lineId)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pre loadSubcategoryList");
            loadLinesConfig();
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pos loadSubcategoryList");
            if (!cached_list.ContainsKey(lineId))
            {
                return 0;
            }
            else
            {
                return cached_list[lineId];
            }
        }

        public void setVehicleCountForLine(ushort lineId, int vehicles)
        {
            loadLinesConfig();
            cached_list[lineId] = vehicles;
            saveVehicles();
        }

        private Dictionary<ushort, int> getValueFromString(string x)
        {
            string[] array = x.Split(COMMA.ToCharArray());
            var saida = new Dictionary<ushort, int>();
            foreach (string s in array)
            {
                var items = s.Split(SEPARATOR.ToCharArray());
                if (items.Length != 2) continue;
                try
                {
                    ushort lineId = ushort.Parse(items[0]);
                    int vehicles = int.Parse(items[1]);
                    saida[lineId] = vehicles;
                }
                catch (Exception e)
                {
                    TLMUtils.doLog("ERRO AO OBTER VALOR STR: {0}", e);
                    continue;
                }
            }

            return saida;
        }

        private void loadLinesConfig()
        {
            if (cached_list == null)
            {
                cached_list = getValueFromString(TLMConfigWarehouse.getCurrentConfigString(TLMConfigWarehouse.ConfigIndex.VEHICLE_LINE));
            }
        }

        private void saveVehicles()
        {
            TLMConfigWarehouse loadedConfig;
            loadedConfig = TransportLinesManagerMod.instance.currentLoadedCityConfig;
            var value = string.Join(COMMA, cached_list.Select(x => x.Key.ToString() + SEPARATOR + x.Value.ToString()).ToArray());
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("saveVehicles NEW VALUE: {0}", value);
            loadedConfig.setString(TLMConfigWarehouse.ConfigIndex.VEHICLE_LINE, value);
        }
    }
}
