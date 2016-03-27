using ColossalFramework.Globalization;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    public class BasicTransportExtensionSingleton
    {
        private static Dictionary<Type, BasicTransportExtension> _instances = new Dictionary<Type, BasicTransportExtension>();

        public static BasicTransportExtension instance(Type T)
        {
            if (!_instances.ContainsKey(T))
            {
                _instances[T] = new BasicTransportExtension(T);
            }
            return _instances[T];
        }
    }

    public class BasicTransportExtension
    {
        internal BasicTransportExtension(Type t)
        {
            type = t;
        }

        private TLMConfigWarehouse.ConfigIndex configKeyForAssets
        {
            get
            {
                return TLMConfigWarehouse.getConfigAssetsForAI(type);
            }
        }

        public TLMConfigWarehouse.ConfigIndex configKeyForAutoNamingPrefixRule
        {
            get
            {
                return TLMConfigWarehouse.getConfigPrefixForAI(type);
            }
        }

        public TLMConfigWarehouse.ConfigIndex configKeyForTransportSystem
        {
            get
            {
                return TLMConfigWarehouse.getConfigTransportSystemForAI(type);
            }
        }

        private const string SEPARATOR = "∂";
        private const string COMMA = "∞";
        private const string SUBSEPARATOR = "∫";
        private const string SUBCOMMA = "≠";
        private const string SUBSUBCOMMA = "⅞";
        private List<string> basicAssetsList;
        private bool globalLoaded = false;
        private Type type;

        private Dictionary<uint, Dictionary<PrefixConfigIndex, string>> cached_subcategoryList;
        private Dictionary<uint, Dictionary<PrefixConfigIndex, string>> cached_subcategoryListGlobal;
        private Dictionary<uint, Dictionary<PrefixConfigIndex, string>> cached_subcategoryListNonGlobal;


        public List<string> getAssetListForPrefix(uint prefix, bool global = false)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pre loadSubcategoryList");
            loadSubcategoryList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pos loadSubcategoryList");
            if (!cached_subcategoryList.ContainsKey(prefix))
            {
                prefix = 0;
            }

            List<string> assetsList;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pre  if (cached_subcategoryList.ContainsKey(prefix))");
            if (cached_subcategoryList.ContainsKey(prefix))
            {
                if (!cached_subcategoryList[prefix].ContainsKey(PrefixConfigIndex.MODELS) || cached_subcategoryList[prefix][PrefixConfigIndex.MODELS] == string.Empty)
                {
                    assetsList = new List<string>();
                }
                else
                {
                    assetsList = cached_subcategoryList[prefix][PrefixConfigIndex.MODELS].Split(SUBSUBCOMMA.ToCharArray()).ToList();
                }
            }
            else
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: ELSE!");
                assetsList = basicAssetsList;
            }
            return assetsList;
        }

        private uint getIndexFromStringArray(string x)
        {
            uint saida;
            if (uint.TryParse(x.Split(SEPARATOR.ToCharArray())[0], out saida))
            {
                return saida;
            }
            return 0xFFFFFFFF;
        }

        private Dictionary<PrefixConfigIndex, string> getValueFromStringArray(string x)
        {
            string[] array = x.Split(SEPARATOR.ToCharArray());
            var saida = new Dictionary<PrefixConfigIndex, string>();
            if (array.Length != 2)
            {
                return saida;
            }
            foreach (string s in array[1].Split(SUBCOMMA.ToCharArray()))
            {
                var items = s.Split(SUBSEPARATOR.ToCharArray());
                if (items.Length != 2) continue;
                try
                {
                    PrefixConfigIndex pci = (PrefixConfigIndex)Enum.Parse(typeof(PrefixConfigIndex), items[0]);
                    saida[pci] = items[1];
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            return saida;
        }

        private void loadSubcategoryList(bool global, bool force = false)
        {
            if (cached_subcategoryList == null || globalLoaded != global)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadSubcategoryList: pre loadAuxiliarVars");
                loadAuxiliarVars(global, force);
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadSubcategoryList: pos loadAuxiliarVars");
                if (global)
                {
                    cached_subcategoryList = cached_subcategoryListGlobal;
                }
                else
                {
                    cached_subcategoryList = cached_subcategoryListNonGlobal;
                }

                globalLoaded = global;
            }
        }

        private void loadAuxiliarVars(bool global, bool force = false)
        {
            if ((global && cached_subcategoryListGlobal == null) || (!global && cached_subcategoryListNonGlobal == null) || force)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: IN!");
                string[] file;
                if (global)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: IF!");
                    file = TLMConfigWarehouse.getConfig(TLMConfigWarehouse.GLOBAL_CONFIG_INDEX, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX).getString(configKeyForAssets).Split(COMMA.ToCharArray());
                }
                else
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: ELSE!");
                    file = TLMConfigWarehouse.getCurrentConfigString(configKeyForAssets).Split(COMMA.ToCharArray());
                }
                cached_subcategoryList = new Dictionary<uint, Dictionary<PrefixConfigIndex, string>>();
                if (file.Length > 0)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: file.Length > 0");
                    foreach (string s in file)
                    {
                        uint key = getIndexFromStringArray(s);
                        var value = getValueFromStringArray(s);
                        cached_subcategoryList[key] = value;
                    }
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: dic done");
                    cached_subcategoryList.Remove(0xFFFFFFFF);
                }
                else
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: file.Length == 0");
                    cached_subcategoryList = new Dictionary<uint, Dictionary<PrefixConfigIndex, string>>();
                }
                basicAssetsList = new List<string>();
                var trailerList = new List<string>();

                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: pre prefab read");
                for (uint num = 0u; (ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount()); num += 1u)
                {
                    VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                    if (!(prefab == null) && prefab.GetAI().GetType() == type)
                    {
                        basicAssetsList.Add(prefab.name);
                        if (prefab.m_trailers != null && prefab.m_trailers.Length > 0)
                        {
                            foreach (var trailer in prefab.m_trailers)
                            {
                                if (trailer.m_info.name != prefab.name)
                                {
                                    trailerList.Add(trailer.m_info.name);
                                }
                            }
                        }
                    }
                }
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: pos prefab read");
                basicAssetsList.RemoveAll(x => trailerList.Contains(x));
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: pre models Check");
                foreach (uint prefix in cached_subcategoryList.Keys)
                {
                    if (cached_subcategoryList[prefix].ContainsKey(PrefixConfigIndex.MODELS))
                    {
                        var temp = cached_subcategoryList[prefix][PrefixConfigIndex.MODELS].Split(SUBSUBCOMMA.ToCharArray()).ToList();
                        for (int i = 0; i < temp.Count; i++)
                        {
                            string assetId = temp[i];
                            if (PrefabCollection<VehicleInfo>.FindLoaded(assetId) == null)
                            {
                                temp.RemoveAt(i);
                                i--;
                            }
                        }
                        cached_subcategoryList[prefix][PrefixConfigIndex.MODELS] = string.Join(SUBSUBCOMMA, temp.ToArray());
                    }
                }
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: pos models Check");
                saveSubcategoryList(global);
            }
        }


        private void setSubcategoryList(Dictionary<uint, Dictionary<PrefixConfigIndex, string>> value, bool global)
        {
            cached_subcategoryList = value;
            globalLoaded = global;
            saveSubcategoryList(global);
        }
        private void saveSubcategoryList(bool global)
        {
            if (global == globalLoaded)
            {
                TLMConfigWarehouse loadedConfig;
                if (global)
                {
                    loadedConfig = TLMConfigWarehouse.getConfig(TLMConfigWarehouse.GLOBAL_CONFIG_INDEX, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX);
                }
                else
                {
                    loadedConfig = TransportLinesManagerMod.instance.currentLoadedCityConfig;
                }
                var value = string.Join(COMMA, cached_subcategoryList.Select(x => x.Key.ToString() + SEPARATOR + string.Join(SUBCOMMA, x.Value.Select(y => y.Key.ToString() + SUBSEPARATOR + y.Value).ToArray())).ToArray());
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("NEW VALUE ({0}): {1}", type.ToString(), value);
                loadedConfig.setString(configKeyForAssets, value);
                if (global)
                {
                    cached_subcategoryListGlobal = cached_subcategoryList;
                }
                else
                {
                    cached_subcategoryListNonGlobal = cached_subcategoryList;
                }
            }
            else
            {
                TLMUtils.doErrorLog("Trying to save a different global file subcategory list!!!");
            }

        }


        private bool needReload
        {
            get
            {
                return basicAssetsList == null;
            }
        }


        public string getPrefixName(uint prefix, bool global = false)
        {
            loadSubcategoryList(global);
            if (needReload)
            {
                readVehicles(global); if (needReload) return "";
            }
            if (cached_subcategoryList.ContainsKey(prefix) && cached_subcategoryList[prefix].ContainsKey(PrefixConfigIndex.PREFIX_NAME))
            {
                return cached_subcategoryList[prefix][PrefixConfigIndex.PREFIX_NAME];
            }
            return "";
        }


        public void setPrefixName(uint prefix, string name, bool global = false)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("setPrefixName! {0} {1} {2} {3}", type.ToString(), prefix, name, global);
            loadSubcategoryList(global);
            if (needReload)
            {
                readVehicles(global); if (needReload) return;
            }
            if (!cached_subcategoryList.ContainsKey(prefix))
            {
                cached_subcategoryList[prefix] = new Dictionary<PrefixConfigIndex, string>();
            }
            cached_subcategoryList[prefix][PrefixConfigIndex.PREFIX_NAME] = name;
            saveSubcategoryList(global);
        }


        public Dictionary<string, string> getBasicAssetsListForPrefix(uint prefix, bool global = false)
        {
            loadSubcategoryList(global);
            if (needReload)
            {
                readVehicles(global); if (needReload) return new Dictionary<string, string>();
            }
            if (cached_subcategoryList.ContainsKey(prefix) && cached_subcategoryList[prefix].ContainsKey(PrefixConfigIndex.MODELS))
            {
                if (cached_subcategoryList[prefix][PrefixConfigIndex.MODELS].Trim() == string.Empty)
                {
                    return new Dictionary<string, string>();
                }
                return cached_subcategoryList[prefix][PrefixConfigIndex.MODELS].Split(SUBSUBCOMMA.ToCharArray()).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
            }
            return basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }


        public Dictionary<string, string> getBasicAssetsDictionary(bool global = false)
        {
            if (needReload)
            {
                readVehicles(global); if (needReload) return new Dictionary<string, string>();
            }
            return basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }

        public void addAssetToPrefixList(uint prefix, string assetId, bool global = false)
        {
            loadSubcategoryList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("addAssetToPrefixList: {0} => {1}", assetId, prefix);

            if (!cached_subcategoryList.ContainsKey(prefix))
            {
                cached_subcategoryList[prefix] = new Dictionary<PrefixConfigIndex, string>();
                cached_subcategoryList[prefix][PrefixConfigIndex.MODELS] = "";
            }
            var temp = cached_subcategoryList[prefix][PrefixConfigIndex.MODELS].Split(SUBSUBCOMMA.ToCharArray()).ToList();
            temp.Add(assetId);
            cached_subcategoryList[prefix][PrefixConfigIndex.MODELS] = string.Join(SUBSUBCOMMA, temp.ToArray());
            saveSubcategoryList(global);
            readVehicles(global);
        }

        public void removeAssetFromPrefixList(uint prefix, string assetId, bool global = false)
        {
            loadSubcategoryList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAssetFromPrefixList: {0} => {1}", assetId, prefix);
            List<string> temp;
            if (!cached_subcategoryList.ContainsKey(prefix))
            {
                cached_subcategoryList[prefix] = new Dictionary<PrefixConfigIndex, string>();
                cached_subcategoryList[prefix][PrefixConfigIndex.MODELS] = "";
                temp = getAssetListForPrefix(0, global);
            }
            else {
                temp = cached_subcategoryList[prefix][PrefixConfigIndex.MODELS].Split(SUBSUBCOMMA.ToCharArray()).ToList();
            }
            if (!temp.Contains(assetId)) return;
            temp.Remove(assetId);
            cached_subcategoryList[prefix][PrefixConfigIndex.MODELS] = string.Join(SUBSUBCOMMA, temp.ToArray());
            saveSubcategoryList(global);
            readVehicles(global);
        }

        public void removeAllAssetsFromPrefixList(uint prefix, bool global = false)
        {
            loadSubcategoryList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAssetFromPrefixList: {0}", prefix);
            if (!cached_subcategoryList.ContainsKey(prefix))
            {
                cached_subcategoryList[prefix] = new Dictionary<PrefixConfigIndex, string>();
            }
            cached_subcategoryList[prefix][PrefixConfigIndex.MODELS] = "";
            saveSubcategoryList(global);
            readVehicles(global);
        }

        public void useDefaultAssetsForPrefixList(uint prefix, bool global = false)
        {
            loadSubcategoryList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAssetFromPrefixList: {0}", prefix);
            if (!cached_subcategoryList.ContainsKey(prefix))
            {
                cached_subcategoryList[prefix] = new Dictionary<PrefixConfigIndex, string>();
                return;
            }
            cached_subcategoryList[prefix].Remove(PrefixConfigIndex.MODELS);
            saveSubcategoryList(global);
            readVehicles(global);
        }

        public VehicleInfo getRandomModel(uint prefix)
        {
            var assetList = getAssetListForPrefix(prefix);
            if (assetList.Count == 0) return null;
            Randomizer r = new Randomizer(new System.Random().Next());
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("POSSIBLE VALUES FOR {2} PREFIX {1}: {0} ", string.Join(",", assetList.ToArray()), prefix, type.ToString());
            string model = assetList[r.Int32(0, assetList.Count - 1)];
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("MODEL FOR {2} PREFIX {1}: {0} ", model, prefix, type.ToString());
            var saida = PrefabCollection<VehicleInfo>.FindLoaded(model);
            if (saida == null)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("MODEL DOESN'T EXIST!");
                removeAssetFromPrefixList(prefix, model);
                return getRandomModel(prefix);
            }
            return saida;
        }

        public void forceReload()
        {
            basicAssetsList = null;
            try
            {
                readVehicles(globalLoaded, true); if (needReload) return;
            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog(e.Message);
                basicAssetsList = new List<string>();
            }
        }

        private void readVehicles(bool global, bool force = false)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("PrefabCount: {0} ({1})", PrefabCollection<VehicleInfo>.PrefabCount(), PrefabCollection<VehicleInfo>.LoadedCount());
            if (PrefabCollection<VehicleInfo>.LoadedCount() == 0)
            {
                TLMUtils.doErrorLog("Prefabs not loaded!");
                return;
            }
            loadSubcategoryList(global);
        }

        public int getCapacity(VehicleInfo info)
        {
            if (info == null) return -1;
            return TLMUtils.GetPrivateField<int>(info.GetAI(), "m_passengerCapacity");
        }

        public enum PrefixConfigIndex
        {
            MODELS,
            PREFIX_NAME
        }
    }
}
