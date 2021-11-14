using ColossalFramework.Globalization;
using ICities;
using Klyte.Commons;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.ModShared;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    [XmlRoot("TransportTypeExtension")]
    public class TLMTransportTypeConfigurations : TsdIdentifiable, ITLMTransportTypeExtension
    {

        private List<string> m_basicAssetsList;
        private List<string> m_basicAssetsListIntercity;

        private TransportSystemDefinition Definition => TSD;

        [XmlElement("Configurations")]
        public SimpleNonSequentialList<TLMPrefixConfiguration> Configurations { get; set; } = new SimpleNonSequentialList<TLMPrefixConfiguration>();

        private SimpleXmlDictionary<string, TLMAssetConfiguration> AssetConfigurations { get; set; } = new SimpleXmlDictionary<string, TLMAssetConfiguration>();

        [XmlElement("AssetConfigurations")]
        public SimpleXmlDictionary<string, TLMAssetConfiguration> AssetConfigurationsStore
        {
            get
            {
                var result = new SimpleXmlDictionary<string, TLMAssetConfiguration>();
                foreach (KeyValuePair<string, TLMAssetConfiguration> entry in AssetConfigurations)
                {
                    if (entry.Value.Capacity >= 0)
                    {
                        result[entry.Key] = entry.Value;
                    }
                }
                return result;
            }
            set
            {
                try
                {
                    AssetConfigurations = value;
                    if (!LoadingManager.instance.m_currentlyLoading)
                    {
                        InitCapacitiesInAssets();
                    }
                }
                catch (Exception e)
                {

                    LogUtils.DoErrorLog($"ERROR SETTING ASSET CONFIG: {e}=> {e.Message}\n{e.StackTrace}");
                }
            }
        }

        public TLMPrefixConfiguration SafeGet(uint prefix)
        {
            if (!Configurations.ContainsKey(prefix))
            {
                Configurations[prefix] = new TLMPrefixConfiguration();
            }
            return Configurations[prefix];
        }
        private TLMAssetConfiguration SafeGetAsset(string assetName)
        {
            if (!AssetConfigurations.ContainsKey(assetName))
            {
                AssetConfigurations[assetName] = new TLMAssetConfiguration();
            }
            return AssetConfigurations[assetName];
        }
        IAssetSelectorStorage ISafeGettable<IAssetSelectorStorage>.SafeGet(uint index) => SafeGet(index);
        INameableStorage ISafeGettable<INameableStorage>.SafeGet(uint index) => SafeGet(index);
        ITicketPriceStorage ISafeGettable<ITicketPriceStorage>.SafeGet(uint index) => SafeGet(index);
        IBudgetStorage ISafeGettable<IBudgetStorage>.SafeGet(uint index) => SafeGet(index);
        IColorSelectableStorage ISafeGettable<IColorSelectableStorage>.SafeGet(uint index) => SafeGet(index);
        IDepotSelectionStorage ISafeGettable<IDepotSelectionStorage>.SafeGet(uint index) => SafeGet(index);
        IBasicExtensionStorage ISafeGettable<IBasicExtensionStorage>.SafeGet(uint index) => SafeGet(index);

        public uint GetDefaultTicketPrice(uint x = 0)
        {
            int savedVal = Definition.GetConfig().DefaultTicketPrice;
            return savedVal > 0 ? (uint)savedVal : (uint)TransportManager.instance.GetTransportInfo(TransportType).m_ticketPrice;
        }
        #region Asset properties

        public bool IsCustomCapacity(string name) => AssetConfigurations.ContainsKey(name);
        public int GetCustomCapacity(string name)
        {
            int capacity = SafeGetAsset(name).Capacity;
            return capacity < 0 ? m_defaultCapacities[name] : capacity;
        }

        public void SetVehicleCapacity(string assetName, int newCapacity)
        {
            VehicleInfo vehicle = PrefabCollection<VehicleInfo>.FindLoaded(assetName);
            if (vehicle != null && !VehicleUtils.IsTrailer(vehicle))
            {
                Dictionary<string, MutableTuple<float, int>> assetsCapacitiesPercentagePerTrailer = GetCapacityRelative(vehicle);
                int capacityUsed = 0;
                foreach (KeyValuePair<string, MutableTuple<float, int>> entry in assetsCapacitiesPercentagePerTrailer)
                {
                    SafeGetAsset(entry.Key).Capacity = Mathf.RoundToInt(newCapacity <= 0 ? -1f : entry.Value.First * newCapacity);
                    capacityUsed += SafeGetAsset(entry.Key).Capacity * entry.Value.Second;
                }
                if (newCapacity > 0 && capacityUsed != newCapacity)
                {
                    SafeGetAsset(assetsCapacitiesPercentagePerTrailer.Keys.ElementAt(0)).Capacity += (newCapacity - capacityUsed) / assetsCapacitiesPercentagePerTrailer[assetsCapacitiesPercentagePerTrailer.Keys.ElementAt(0)].Second;
                }
                foreach (string entry in assetsCapacitiesPercentagePerTrailer.Keys)
                {
                    VehicleAI vai = PrefabCollection<VehicleInfo>.FindLoaded(entry).m_vehicleAI;
                    SetVehicleCapacity(vai, SafeGetAsset(entry).Capacity);
                }
                SimulationManager.instance.StartCoroutine(TLMVehicleUtils.UpdateCapacityUnitsFromTSD());
            }
        }

        public void InitCapacitiesInAssets()
        {
            var keys = AssetConfigurations.Keys.ToList();
            foreach (string entry in keys)
            {
                try
                {
                    VehicleInfo info = PrefabCollection<VehicleInfo>.FindLoaded(entry);
                    if (info != null)
                    {
                        VehicleAI ai = info.m_vehicleAI;
                        UpdateDefaultCapacity(ai);
                        SetVehicleCapacity(ai, SafeGetAsset(entry).Capacity);
                    }
                    else
                    {
                        AssetConfigurations.Remove(entry);
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"ERROR LOADING ASSET CONFIG: {e}=> {e.Message}\n{e.StackTrace}");
                }
            }
            SimulationManager.instance.StartCoroutine(TLMVehicleUtils.UpdateCapacityUnitsFromTSD());
        }

        private static readonly Dictionary<string, int> m_defaultCapacities = new Dictionary<string, int>();

        public Dictionary<string, MutableTuple<float, int>> GetCapacityRelative(VehicleInfo info)
        {
            var relativeParts = new Dictionary<string, MutableTuple<float, int>>();
            GetCapacityRelative(info, info.m_vehicleAI, ref relativeParts, out _);
            return relativeParts;
        }

        private void GetCapacityRelative<AI>(VehicleInfo info, AI ai, ref Dictionary<string, MutableTuple<float, int>> relativeParts, out int totalCapacity, bool noLoop = false) where AI : VehicleAI
        {
            if (info == null)
            {
                totalCapacity = 0;
                return;
            }

            totalCapacity = UpdateDefaultCapacity(ai);
            if (relativeParts.ContainsKey(info.name))
            {
                relativeParts[info.name].Second++;
            }
            else
            {
                relativeParts[info.name] = MutableTuple.New((float)totalCapacity, 1);
            }
            if (!noLoop)
            {
                try
                {
                    foreach (VehicleInfo.VehicleTrailer trailer in info.m_trailers)
                    {
                        if (trailer.m_info != null)
                        {
                            GetCapacityRelative(trailer.m_info, trailer.m_info.m_vehicleAI, ref relativeParts, out int capacity, true);
                            totalCapacity += capacity;
                        }
                    }

                    for (int i = 0; i < relativeParts.Keys.Count; i++)
                    {
                        relativeParts[relativeParts.Keys.ElementAt(i)].First /= totalCapacity;
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoLog($"ERRO AO OBTER CAPACIDADE REL: [{info}] {e} {e.Message}\n{e.StackTrace}");
                }
            }
        }

        private int UpdateDefaultCapacity<AI>(AI ai) where AI : VehicleAI
        {
            if (!m_defaultCapacities.ContainsKey(ai.m_info.name))
            {
                m_defaultCapacities[ai.m_info.name] = (int)VehicleUtils.GetVehicleCapacityField(ai).GetValue(ai);
                if (CommonProperties.DebugMode)
                {
                    LogUtils.DoLog($"STORED DEFAULT VEHICLE CAPACITY {m_defaultCapacities[ai.m_info.name] } for {ai.m_info.name}");
                }
            }
            return m_defaultCapacities[ai.m_info.name];
        }

        private void SetVehicleCapacity<AI>(AI ai, int newCapacity) where AI : VehicleAI
        {
            int defaultCapacity = UpdateDefaultCapacity(ai);
            if (newCapacity <= 0)
            {
                newCapacity = defaultCapacity;
            }
            VehicleUtils.GetVehicleCapacityField(ai).SetValue(ai, newCapacity);
            if (CommonProperties.DebugMode)
            {
                LogUtils.DoLog($"SET VEHICLE CAPACITY {newCapacity} at {ai.m_info.name}");
            }
        }

        #endregion

        #region Asset List
        public Dictionary<string, string> GetSelectedBasicAssetsForLine(ushort lineId)
        {
            if (lineId > 0)
            {
                if (m_basicAssetsList == null)
                {
                    LoadBasicAssets();
                }

                return ExtensionStaticExtensionMethods.GetAssetListForLine(this, lineId).Intersect(m_basicAssetsList).ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x));
            }
            else
            {

                if (m_basicAssetsListIntercity == null)
                {
                    LoadBasicAssetsInterCity();
                }
                return ExtensionStaticExtensionMethods.GetAssetListForLine(this, lineId).Intersect(m_basicAssetsListIntercity).ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x));
            }
        }
        public Dictionary<string, string> GetAllBasicAssetsForLine(ushort lineId)
        {
            if (lineId > 0)
            {
                if (m_basicAssetsList == null)
                {
                    LoadBasicAssets();
                }

                return m_basicAssetsList.ToDictionary(x => x, x => Locale.GetUnchecked("VEHICLE_TITLE", x));
            }
            else
            {

                if (m_basicAssetsListIntercity == null)
                {
                    LoadBasicAssetsInterCity();
                }
                return m_basicAssetsListIntercity.ToDictionary(x => x, x => Locale.GetUnchecked("VEHICLE_TITLE", x));
            }
        }
        public List<string> GetBasicAssetListForLine(ushort lineId)
        {
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }
            return m_basicAssetsList;
        }
        public VehicleInfo GetAModel(ushort lineID)
        {
            VehicleInfo info = null;
            List<string> assetList = ExtensionStaticExtensionMethods.GetAssetListForLine(this, lineID);
            while (info == null && assetList.Count > 0)
            {
                info = VehicleUtils.GetRandomModel(assetList, out string modelName);
                if (info == null)
                {
                    ExtensionStaticExtensionMethods.RemoveAssetFromLine(this, lineID, modelName);
                    assetList = ExtensionStaticExtensionMethods.GetAssetListForLine(this, lineID);
                }
            }
            return info;
        }
        private void LoadBasicAssets()
        {
            TransportSystemDefinition tsd = Definition;
            m_basicAssetsList = TLMPrefabUtils.LoadBasicAssets(tsd);
        }
        private void LoadBasicAssetsInterCity()
        {
            TransportSystemDefinition tsd = Definition;
            m_basicAssetsListIntercity = TLMPrefabUtils.LoadBasicAssetsIntercity(tsd);
        }

        #endregion

        #region Use Color For Model
        public bool IsUsingColorForModel(uint prefix) => SafeGet(prefix).UseColorForModel;

        public void SetUsingColorForModel(uint prefix, bool value) => SafeGet(prefix).UseColorForModel = value;
        #endregion

        #region Custom Palette
        public string GetCustomPalette(uint prefix) => SafeGet(prefix).CustomPalette ?? string.Empty;

        public void SetCustomPalette(uint prefix, string paletteName)
        {
            SafeGet(prefix).CustomPalette = paletteName;
            TLMFacade.Instance?.OnLineSymbolParameterChanged();
        }

        #endregion

        #region Custom Format
        public LineIconSpriteNames GetCustomFormat(uint prefix) => SafeGet(prefix).CustomIcon;

        public void SetCustomFormat(uint prefix, LineIconSpriteNames icon)
        {
            SafeGet(prefix).CustomIcon = icon;

            TLMFacade.Instance?.OnLineSymbolParameterChanged();
        }

        #endregion
        public uint LineToIndex(ushort lineId) => TLMPrefixesUtils.GetPrefix(lineId);

        public void OnReleased()
        {
            var keys = AssetConfigurations.Keys.ToList();
            foreach (string entry in keys)
            {
                try
                {
                    VehicleInfo info = PrefabCollection<VehicleInfo>.FindLoaded(entry);
                    if (info != null)
                    {
                        VehicleAI ai = info.m_vehicleAI;
                        SetVehicleCapacity(ai, 0);
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"ERROR ROLLING BACK ASSET CONFIG: {e}=> {e.Message}\n{e.StackTrace}");
                }
            }
        }
        public static TLMTransportTypeConfigurations GetLoadData(ISerializableData serializableData, string ID)
        {
            if (ID == null || ToolManager.instance.m_properties.m_mode != ItemClass.Availability.Game)
            {
                LogUtils.DoWarnLog($"Trying to load out of game!");
                return null;
            }
            if (!serializableData.EnumerateData().Contains(ID))
            {
                LogUtils.DoLog($"Savegame has no legacy {ID} (default)");
                return null;
            }
            using (var memoryStream = new MemoryStream(serializableData.LoadData(ID)))
            {
                try
                {
                    byte[] storage = memoryStream.ToArray();
                    var file = System.Text.Encoding.UTF8.GetString(storage);
                    if (!file.StartsWith("<"))
                    {
                        file = ZipUtils.Unzip(storage);
                    }
                    file = file.Replace(ID.Split('.').Last(), "TransportTypeExtension");
                    return XmlUtils.DefaultXmlDeserialize<TLMTransportTypeConfigurations>(file);
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"Error trying to load legacy TLMTransportTypeExtension (ID = {ID}): {e.Message}\n{e.StackTrace}");
                    return null;
                }
            }
        }
    }
}
