using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors.VehicleAIExt
{
    public class BasicTransportExtensionSingleton
    {
        private static Dictionary<TransportSystemDefinition, BasicTransportExtension> _instances = new Dictionary<TransportSystemDefinition, BasicTransportExtension>();

        public static BasicTransportExtension Instance(TransportSystemDefinition T)
        {
            if (T == null) return null;
            if (!_instances.ContainsKey(T))
            {
                _instances[T] = new BasicTransportExtension(T);
            }
            return _instances[T];
        }
    }

    public class BasicTransportExtension
    {
        internal BasicTransportExtension(TransportSystemDefinition t)
        {
            definition = t;
        }

        private TLMConfigWarehouse.ConfigIndex ConfigKeyForAssets
        {
            get {
                return TLMConfigWarehouse.getConfigAssetsForAI(definition);
            }
        }

        public TLMConfigWarehouse.ConfigIndex ConfigKeyForAutoNamingPrefixRule
        {
            get {
                return TLMConfigWarehouse.getConfigPrefixForAI(definition);
            }
        }

        public TLMConfigWarehouse.ConfigIndex ConfigKeyForTransportSystem
        {
            get {
                return TLMConfigWarehouse.getConfigTransportSystemForDefinition(definition);
            }
        }

        private const string PREFIX_SEPARATOR = "∂";
        private const string PREFIX_COMMA = "∞";
        private const string PROPERTY_SEPARATOR = "∫";
        private const string PROPERTY_COMMA = "≠";
        private const string PROPERTY_VALUE_COMMA = "⅞";
        private List<string> basicAssetsList;
        private bool globalLoaded = false;
        private TransportSystemDefinition definition;

        private Dictionary<uint, Dictionary<PrefixConfigIndex, string>> cached_prefixConfigList;
        private Dictionary<uint, Dictionary<PrefixConfigIndex, string>> cached_prefixConfigListGlobal;
        private Dictionary<uint, Dictionary<PrefixConfigIndex, string>> cached_prefixConfigListNonGlobal;


        public List<string> GetAssetListForPrefix(uint prefix, bool global = false)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pre loadSubcategoryList");
            LoadPrefixConfigList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pos loadSubcategoryList");
            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                prefix = 0;
            }

            List<string> assetsList;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pre  if (cached_subcategoryList.ContainsKey(prefix))");
            if (cached_prefixConfigList.ContainsKey(prefix))
            {
                if (!cached_prefixConfigList[prefix].ContainsKey(PrefixConfigIndex.MODELS) || cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS] == string.Empty)
                {
                    assetsList = new List<string>();
                }
                else
                {
                    assetsList = cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS].Split(PROPERTY_VALUE_COMMA.ToCharArray()).ToList();
                }
            }
            else
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: ELSE!");
                assetsList = basicAssetsList;
            }
            return assetsList;
        }

        private uint GetIndexFromStringArray(string x)
        {
            if (uint.TryParse(x.Split(PREFIX_SEPARATOR.ToCharArray())[0], out uint saida))
            {
                return saida;
            }
            return 0xFFFFFFFF;
        }

        private Dictionary<PrefixConfigIndex, string> GetValueFromStringArray(string x)
        {
            string[] array = x.Split(PREFIX_SEPARATOR.ToCharArray());
            var saida = new Dictionary<PrefixConfigIndex, string>();
            if (array.Length != 2)
            {
                return saida;
            }
            foreach (string s in array[1].Split(PROPERTY_COMMA.ToCharArray()))
            {
                var items = s.Split(PROPERTY_SEPARATOR.ToCharArray());
                if (items.Length != 2) continue;
                try
                {
                    PrefixConfigIndex pci = (PrefixConfigIndex)Enum.Parse(typeof(PrefixConfigIndex), items[0]);
                    saida[pci] = items[1];
                }
                catch (Exception e)
                {
                    TLMUtils.doLog("ERRO AO OBTER VALOR: {0}", e);
                    continue;
                }
            }

            return saida;
        }

        private void LoadPrefixConfigList(bool global, bool force = false)
        {
            if (cached_prefixConfigList == null || globalLoaded != global)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadSubcategoryList: pre loadAuxiliarVars");
                LoadAuxiliarVars(global, force);
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadSubcategoryList: pos loadAuxiliarVars");
                if (global)
                {
                    cached_prefixConfigList = cached_prefixConfigListGlobal;
                }
                else
                {
                    cached_prefixConfigList = cached_prefixConfigListNonGlobal;
                }

                globalLoaded = global;
            }
        }

        private void LoadAuxiliarVars(bool global, bool force = false)
        {
            if ((global && cached_prefixConfigListGlobal == null) || (!global && cached_prefixConfigListNonGlobal == null) || force)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: IN!");
                string[] file;
                if (global)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: IF!");
                    file = TLMConfigWarehouse.getConfig(TLMConfigWarehouse.GLOBAL_CONFIG_INDEX, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX).getString(ConfigKeyForAssets).Split(PREFIX_COMMA.ToCharArray());
                }
                else
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: ELSE!");
                    file = TLMConfigWarehouse.getCurrentConfigString(ConfigKeyForAssets).Split(PREFIX_COMMA.ToCharArray());
                }
                cached_prefixConfigList = new Dictionary<uint, Dictionary<PrefixConfigIndex, string>>();
                if (file.Length > 0)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: file.Length > 0");
                    foreach (string s in file)
                    {
                        uint key = GetIndexFromStringArray(s);
                        var value = GetValueFromStringArray(s);
                        cached_prefixConfigList[key] = value;
                    }
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: dic done");
                    cached_prefixConfigList.Remove(0xFFFFFFFF);
                }
                else
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: file.Length == 0");
                    cached_prefixConfigList = new Dictionary<uint, Dictionary<PrefixConfigIndex, string>>();
                }
                basicAssetsList = new List<string>();

                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: pre prefab read");
                for (uint num = 0u; (ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount()); num += 1u)
                {
                    VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                    if (!(prefab == null) && definition.isFromSystem(prefab) && !IsTrailer(prefab))
                    {
                        basicAssetsList.Add(prefab.name);
                    }
                }
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: pre models Check");
                foreach (uint prefix in cached_prefixConfigList.Keys)
                {
                    if (cached_prefixConfigList[prefix].ContainsKey(PrefixConfigIndex.MODELS))
                    {
                        var temp = cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS].Split(PROPERTY_VALUE_COMMA.ToCharArray()).ToList();
                        for (int i = 0; i < temp.Count; i++)
                        {
                            string assetId = temp[i];
                            if (PrefabCollection<VehicleInfo>.FindLoaded(assetId) == null)
                            {
                                temp.RemoveAt(i);
                                i--;
                            }
                        }
                        cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS] = string.Join(PROPERTY_VALUE_COMMA, temp.ToArray());
                    }
                }
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("loadAuxiliarVars: pos models Check");
                SaveSubcategoryList(global);
            }
        }

        private bool IsTrailer(PrefabInfo prefab)
        {
            string @unchecked = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
            return @unchecked.StartsWith("VEHICLE_TITLE") || @unchecked.StartsWith("Trailer");
        }


        private void SetSubcategoryList(Dictionary<uint, Dictionary<PrefixConfigIndex, string>> value, bool global)
        {
            cached_prefixConfigList = value;
            globalLoaded = global;
            SaveSubcategoryList(global);
        }
        private void SaveSubcategoryList(bool global)
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
                var value = string.Join(PREFIX_COMMA, cached_prefixConfigList.Select(x => x.Key.ToString() + PREFIX_SEPARATOR + string.Join(PROPERTY_COMMA, x.Value.Select(y => y.Key.ToString() + PROPERTY_SEPARATOR + y.Value).ToArray())).ToArray());
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("NEW VALUE ({0}): {1}", definition.ToString(), value);
                loadedConfig.setString(ConfigKeyForAssets, value);
                if (global)
                {
                    cached_prefixConfigListGlobal = cached_prefixConfigList;
                }
                else
                {
                    cached_prefixConfigListNonGlobal = cached_prefixConfigList;
                }
            }
            else
            {
                TLMUtils.doErrorLog("Trying to save a different global file subcategory list!!!");
            }

        }


        private bool NeedReload
        {
            get {
                return basicAssetsList == null;
            }
        }


        public string GetPrefixName(uint prefix, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (NeedReload)
            {
                readVehicles(global); if (NeedReload) return "";
            }
            if (cached_prefixConfigList.ContainsKey(prefix) && cached_prefixConfigList[prefix].ContainsKey(PrefixConfigIndex.PREFIX_NAME))
            {
                return cached_prefixConfigList[prefix][PrefixConfigIndex.PREFIX_NAME];
            }
            return "";
        }


        public void SetPrefixName(uint prefix, string name, bool global = false)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("setPrefixName! {0} {1} {2} {3}", definition.ToString(), prefix, name, global);
            LoadPrefixConfigList(global);
            if (NeedReload)
            {
                readVehicles(global);
                if (NeedReload)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("setPrefixName: RELOAD FAILED!");
                    return;
                }
            }
            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                cached_prefixConfigList[prefix] = new Dictionary<PrefixConfigIndex, string>();
            }
            cached_prefixConfigList[prefix][PrefixConfigIndex.PREFIX_NAME] = name;
            SaveSubcategoryList(global);
        }

        public uint[] GetBudgetsMultiplier(uint prefix, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (NeedReload)
            {
                readVehicles(global); if (NeedReload) return new uint[] { 100 };
            }
            if (cached_prefixConfigList.ContainsKey(prefix) && cached_prefixConfigList[prefix].ContainsKey(PrefixConfigIndex.BUDGET_MULTIPLIER))
            {
                string[] savedMultipliers = cached_prefixConfigList[prefix][PrefixConfigIndex.BUDGET_MULTIPLIER].Split(PROPERTY_VALUE_COMMA.ToCharArray());

                uint[] result = new uint[savedMultipliers.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    if (uint.TryParse(savedMultipliers[i], out uint parsed))
                    {
                        result[i] = parsed;
                    }
                    else
                    {
                        return new uint[] { 100 };
                    }
                }
                TLMUtils.doLog("LENGTH SIZE BG PFX= {0}", result.Length);
                return result;
            }
            return new uint[] { 100 };
        }

        public uint GetBudgetMultiplierForHour(uint prefix, int hour)
        {
            LoadPrefixConfigList(false);
            uint result = 100;
            if (cached_prefixConfigList.ContainsKey(prefix) && cached_prefixConfigList[prefix].ContainsKey(PrefixConfigIndex.BUDGET_MULTIPLIER))
            {
                string[] savedMultipliers = cached_prefixConfigList[prefix][PrefixConfigIndex.BUDGET_MULTIPLIER].Split(PROPERTY_VALUE_COMMA.ToCharArray());
                if (savedMultipliers.Length == 1)
                {
                    if (uint.TryParse(savedMultipliers[0], out result))
                    {
                        return result;
                    }
                }
                else if (savedMultipliers.Length == 8)
                {
                    if (uint.TryParse(savedMultipliers[((hour + 23) / 3) % 8], out result))
                    {
                        return result;
                    }
                }
            }
            return 100;
        }


        public void SetBudgetMultiplier(uint prefix, uint[] multipliers, bool global = false)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("setBudgetMultiplier! {0} {1} {2} {3}", definition.ToString(), prefix, multipliers, global);
            LoadPrefixConfigList(global);
            if (NeedReload)
            {
                readVehicles(global);
                if (NeedReload)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getTicketPrice: RELOAD FAILED!");
                    return;
                }
            }
            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                cached_prefixConfigList[prefix] = new Dictionary<PrefixConfigIndex, string>();
            }
            cached_prefixConfigList[prefix][PrefixConfigIndex.BUDGET_MULTIPLIER] = string.Join(PROPERTY_VALUE_COMMA, multipliers.Select(x => x.ToString()).ToArray());
            SaveSubcategoryList(global);
        }

        public uint GetTicketPrice(uint prefix, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (NeedReload)
            {
                readVehicles(global);
                if (NeedReload)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getTicketPrice: RELOAD FAILED!");
                    return 101;
                }
            }
            if (cached_prefixConfigList.ContainsKey(prefix) && cached_prefixConfigList[prefix].ContainsKey(PrefixConfigIndex.TICKET_PRICE))
            {
                if (uint.TryParse(cached_prefixConfigList[prefix][PrefixConfigIndex.TICKET_PRICE], out uint result))
                {
                    return result;
                }
            }
            return GetDefaultTicketPrice();
        }

        public uint GetDefaultTicketPrice()
        {

            switch (definition.subService)
            {
                case ItemClass.SubService.PublicTransportCableCar:
                case ItemClass.SubService.PublicTransportBus:
                case ItemClass.SubService.PublicTransportMonorail:
                    return 100;
                case ItemClass.SubService.PublicTransportMetro:
                case ItemClass.SubService.PublicTransportTaxi:
                case ItemClass.SubService.PublicTransportTrain:
                case ItemClass.SubService.PublicTransportTram:
                    return 200;
                case ItemClass.SubService.PublicTransportPlane:
                    if (definition.vehicleType == VehicleInfo.VehicleType.Blimp)
                    {
                        return 100;
                    }
                    else
                    {
                        return 1000;
                    }
                case ItemClass.SubService.PublicTransportShip:
                    if (definition.vehicleType == VehicleInfo.VehicleType.Ferry)
                    {
                        return 100;
                    }
                    else
                    {
                        return 500;
                    }
                default:
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("subservice not found: {0}", definition.subService);

                    return 103;
            }

        }

        public void SetTicketPrice(uint prefix, uint price, bool global = false)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("setTicketPrice! {0} {1} {2} {3}", definition.ToString(), prefix, price, global);
            LoadPrefixConfigList(global);
            if (NeedReload)
            {
                readVehicles(global);
                if (NeedReload)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("setTicketPrice: RELOAD FAILED!");
                    return;
                }
            }
            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                cached_prefixConfigList[prefix] = new Dictionary<PrefixConfigIndex, string>();
            }
            cached_prefixConfigList[prefix][PrefixConfigIndex.TICKET_PRICE] = price.ToString();
            SaveSubcategoryList(global);
        }

        public Dictionary<string, string> GetBasicAssetsListForPrefix(uint prefix, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (NeedReload)
            {
                readVehicles(global); if (NeedReload) return new Dictionary<string, string>();
            }
            if (cached_prefixConfigList.ContainsKey(prefix) && cached_prefixConfigList[prefix].ContainsKey(PrefixConfigIndex.MODELS))
            {
                if (cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS].Trim() == string.Empty)
                {
                    return new Dictionary<string, string>();
                }
                return cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS].Split(PROPERTY_VALUE_COMMA.ToCharArray()).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
            }
            return basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }


        public Dictionary<string, string> GetBasicAssetsDictionary(bool global = false)
        {
            if (NeedReload)
            {
                readVehicles(global);
                if (NeedReload)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getBasicAssetsDictionary: RELOAD FAILED!");

                    return new Dictionary<string, string>();
                }
            }
            return basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }

        public void AddAssetToPrefixList(uint prefix, string assetId, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("addAssetToPrefixList: {0} => {1}", assetId, prefix);

            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                cached_prefixConfigList[prefix] = new Dictionary<PrefixConfigIndex, string>
                {
                    [PrefixConfigIndex.MODELS] = ""
                };
            }
            var temp = cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS].Split(PROPERTY_VALUE_COMMA.ToCharArray()).ToList();
            temp.Add(assetId);
            cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS] = string.Join(PROPERTY_VALUE_COMMA, temp.ToArray());
            SaveSubcategoryList(global);
            readVehicles(global);
        }

        public void removeAssetFromPrefixList(uint prefix, string assetId, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAssetFromPrefixList: {0} => {1}", assetId, prefix);
            List<string> temp;
            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                cached_prefixConfigList[prefix] = new Dictionary<PrefixConfigIndex, string>
                {
                    [PrefixConfigIndex.MODELS] = ""
                };
                temp = GetAssetListForPrefix(0, global);
            }
            else
            {
                temp = cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS].Split(PROPERTY_VALUE_COMMA.ToCharArray()).ToList();
            }
            if (!temp.Contains(assetId)) return;
            temp.Remove(assetId);
            cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS] = string.Join(PROPERTY_VALUE_COMMA, temp.ToArray());
            SaveSubcategoryList(global);
            readVehicles(global);
        }

        public void removeAllAssetsFromPrefixList(uint prefix, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAllAssetsFromPrefixList: {0}", prefix);
            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                cached_prefixConfigList[prefix] = new Dictionary<PrefixConfigIndex, string>();
            }
            cached_prefixConfigList[prefix][PrefixConfigIndex.MODELS] = "";
            SaveSubcategoryList(global);
            readVehicles(global);
        }

        public void useDefaultAssetsForPrefixList(uint prefix, bool global = false)
        {
            LoadPrefixConfigList(global);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("useDefaultAssetsForPrefixList: {0}", prefix);
            if (!cached_prefixConfigList.ContainsKey(prefix))
            {
                cached_prefixConfigList[prefix] = new Dictionary<PrefixConfigIndex, string>();
                return;
            }
            cached_prefixConfigList[prefix].Remove(PrefixConfigIndex.MODELS);
            SaveSubcategoryList(global);
            readVehicles(global);
        }

        public VehicleInfo getRandomModel(uint prefix)
        {
            var assetList = GetAssetListForPrefix(prefix);
            if (assetList.Count == 0) return null;
            Randomizer r = new Randomizer(new System.Random().Next());
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("POSSIBLE VALUES FOR {2} PREFIX {1}: {0} ", string.Join(",", assetList.ToArray()), prefix, definition.ToString());
            string model = assetList[r.Int32(0, assetList.Count - 1)];
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("MODEL FOR {2} PREFIX {1}: {0} ", model, prefix, definition.ToString());
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
                readVehicles(globalLoaded, true); if (NeedReload) return;
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
            LoadPrefixConfigList(global);
        }

        public int getCapacity(VehicleInfo info, bool noLoop = false)
        {
            if (info == null) return -1;
            int capacity = TLMUtils.GetPrivateField<int>(info.GetAI(), "m_passengerCapacity");
            try
            {
                if (!noLoop)
                {
                    foreach (var trailer in info.m_trailers)
                    {
                        capacity += getCapacity(trailer.m_info, true);
                    }
                }
            }
            catch (Exception e)
            {
                TLMUtils.doLog("ERRO AO OBTER CAPACIDADE: [{0}] {1}", info, e.Message);
            }
            return capacity;
        }

        public static void removeAllUnwantedVehicles()
        {
            for (ushort lineId = 1; lineId < Singleton<TransportManager>.instance.m_lines.m_size; lineId++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAllUnwantedVehicles: line #{0}", lineId);
                    TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
                    uint prefix = 0;
                    if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportInfo(tl.Info) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
                    {
                        prefix = tl.m_lineNumber / 1000u;
                    }
                    VehicleManager instance3 = Singleton<VehicleManager>.instance;
                    VehicleInfo info = instance3.m_vehicles.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].GetVehicle(0)].Info;
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAllUnwantedVehicles: pre model list; type = {0}", info.GetAI());
                    var def = TransportSystemDefinition.from(info);
                    if (def == default(TransportSystemDefinition) || def == null)
                    {
                        if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("NULL TSysDef! {0}+{1}+{2}", info.GetAI().GetType(), info.m_class.m_subService, info.m_vehicleType);
                        continue;
                    }
                    var modelList = BasicTransportExtensionSingleton.Instance(def).GetAssetListForPrefix(prefix);
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("removeAllUnwantedVehicles: models found: {0}", modelList == null ? "?!?" : modelList.Count.ToString());
                    if (modelList.Count > 0)
                    {
                        Dictionary<ushort, VehicleInfo> vehiclesToRemove = new Dictionary<ushort, VehicleInfo>();
                        for (int i = 0; i < tl.CountVehicles(lineId); i++)
                        {
                            var vehicle = tl.GetVehicle(i);
                            if (vehicle != 0)
                            {
                                VehicleInfo info2 = instance3.m_vehicles.m_buffer[(int)vehicle].Info;
                                if (!modelList.Contains(info2.name))
                                {
                                    vehiclesToRemove[vehicle] = info2;
                                }
                            }
                        }

                        foreach (var item in vehiclesToRemove)
                        {
                            item.Value.m_vehicleAI.SetTransportLine(item.Key, ref instance3.m_vehicles.m_buffer[item.Key], 0);
                        }
                    }
                }
            }
        }

        public enum PrefixConfigIndex
        {
            MODELS,
            PREFIX_NAME,
            BUDGET_MULTIPLIER,
            TICKET_PRICE
        }
    }
}
