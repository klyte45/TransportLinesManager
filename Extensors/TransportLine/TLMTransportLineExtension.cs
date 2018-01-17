using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Threading;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Extensors.TransportLineExt
{

    enum TLMTransportLineFlags
    {
        ZERO_BUDGET_DAY = 0x40000000,
        ZERO_BUDGET_NIGHT = 0x20000000,
        ZERO_BUDGET_SETTED = 0x10000000
    }

    enum TLMTransportLineExtensionsKey
    {
        USE_CUSTOM_CONFIG,
        LOCAL_MODEL_LIST,
        LOCAL_TICKET_PRICE,
        LOCAL_BUDGET
    }

    class TLMTransportLineExtensions : ExtensionInterfaceDefaultImpl<TLMTransportLineExtensionsKey, TLMTransportLineExtensions>, ITLMAssetSelectorExtension
    {
        protected override TLMCW.ConfigIndex ConfigIndexKey => TLMCW.ConfigIndex.LINES_CONFIG;
        private Dictionary<TransportSystemDefinition, List<string>> basicAssetsList = new Dictionary<TransportSystemDefinition, List<string>>();

        public void SetUseCustomConfig(ushort lineId, bool value)
        {
            SafeSet(lineId, TLMTransportLineExtensionsKey.USE_CUSTOM_CONFIG, value.ToString());
        }

        public bool GetUseCustomConfig(ushort lineId)
        {
            return Boolean.TryParse(SafeGet(lineId, TLMTransportLineExtensionsKey.USE_CUSTOM_CONFIG), out bool result) && result;
        }

        #region Asset List
        public List<string> GetAssetList(uint lineId)
        {
            string value = SafeGet(lineId, TLMTransportLineExtensionsKey.LOCAL_MODEL_LIST);
            if (string.IsNullOrEmpty(value))
            {
                return new List<string>();
            }
            else
            {
                return value.Split(ItSepLvl3.ToCharArray()).ToList();
            }
        }
        public Dictionary<string, string> GetSelectedBasicAssets(uint lineId)
        {
            TransportSystemDefinition tsd = TransportSystemDefinition.from(lineId);
            if (!basicAssetsList.ContainsKey(tsd)) basicAssetsList[tsd] = TLMUtils.LoadBasicAssets(tsd);
            return GetAssetList(lineId).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", TLMUtils.getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public Dictionary<string, string> GetAllBasicAssets(uint lineId)
        {
            TransportSystemDefinition tsd = TransportSystemDefinition.from(lineId);
            if (!basicAssetsList.ContainsKey(tsd)) basicAssetsList[tsd] = TLMUtils.LoadBasicAssets(tsd);
            return basicAssetsList[tsd].ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", TLMUtils.getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public void AddAsset(uint lineId, string assetId)
        {
            var temp = GetAssetList(lineId);
            if (temp.Contains(assetId)) return;
            temp.Add(assetId);
            SafeSet(lineId, TLMTransportLineExtensionsKey.LOCAL_MODEL_LIST, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void RemoveAsset(uint lineId, string assetId)
        {
            var temp = GetAssetList(lineId);
            if (!temp.Contains(assetId)) return;
            temp.RemoveAll(x => x == assetId);
            SafeSet(lineId, TLMTransportLineExtensionsKey.LOCAL_MODEL_LIST, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void UseDefaultAssets(uint lineId)
        {
            SafeCleanProperty(lineId, TLMTransportLineExtensionsKey.LOCAL_MODEL_LIST);
        }
        public VehicleInfo GetAModel(ushort lineId)
        {
            return TLMUtils.GetRandomModel(GetAssetList(lineId));
        }
        #endregion

    }
}
