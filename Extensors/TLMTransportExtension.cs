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
using Klyte.TransportLinesManager.Interfaces;

namespace Klyte.TransportLinesManager.Extensors.VehicleAIExt
{
    public abstract class TLMTransportExtension<TSD, SG> : ExtensionInterfaceDefaultImpl<PrefixConfigIndex, SG>, ITLMTransportExtension where TSD : TLMSysDef, new() where SG : TLMTransportExtension<TSD, SG>
    {
        private string ItSepLvl3 { get { return "⅞"; } }

        protected override TLMConfigWarehouse.ConfigIndex ConfigIndexKey
        {
            get {
                return TLMConfigWarehouse.getConfigAssetsForAI(definition);
            }
        }
        protected override bool AllowGlobal { get { return false; } }

        private List<string> basicAssetsList;

        private TransportSystemDefinition definition => Singleton<TSD>.instance.GetTSD();

        #region Utils
        private bool IsTrailer(PrefabInfo prefab)
        {
            string @unchecked = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
            return @unchecked.StartsWith("VEHICLE_TITLE") || @unchecked.StartsWith("Trailer");
        }
        #endregion

        #region Subcategory List
        #endregion

        #region Prefix Name
        public string GetPrefixName(uint prefix)
        {
            return SafeGet(prefix, PrefixConfigIndex.PREFIX_NAME);
        }
        public void SetPrefixName(uint prefix, string name)
        {
            SafeSet(prefix, PrefixConfigIndex.PREFIX_NAME, name);
        }
        #endregion

        #region Budget Multiplier
        public uint[] GetBudgetsMultiplier(uint prefix)
        {
            string value = SafeGet(prefix, PrefixConfigIndex.BUDGET_MULTIPLIER);
            if (value == null) return new uint[] { 100 };
            string[] savedMultipliers = value.Split(ItSepLvl3.ToCharArray());

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
        public uint GetBudgetMultiplierForHour(uint prefix, int hour)
        {
            uint[] savedMultipliers = GetBudgetsMultiplier(prefix);
            if (savedMultipliers.Length == 1)
            {
                return savedMultipliers[0];
            }
            else if (savedMultipliers.Length == 8)
            {
                return savedMultipliers[((hour + 23) / 3) % 8];
            }
            return 100;
        }
        public void SetBudgetMultiplier(uint prefix, uint[] multipliers)
        {
            SafeSet(prefix, PrefixConfigIndex.BUDGET_MULTIPLIER, string.Join(ItSepLvl3, multipliers.Select(x => x.ToString()).ToArray()));
        }
        #endregion

        #region Ticket Price
        public uint GetTicketPrice(uint prefix)
        {

            if (uint.TryParse(SafeGet(prefix, PrefixConfigIndex.TICKET_PRICE), out uint result))
            {
                return result;
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
        public void SetTicketPrice(uint prefix, uint price)
        {
            SafeSet(prefix, PrefixConfigIndex.TICKET_PRICE, price.ToString());
        }
        #endregion

        #region Asset List
        public List<string> GetAssetListForPrefix(uint prefix)
        {
            string value = SafeGet(prefix, PrefixConfigIndex.MODELS);
            if (string.IsNullOrEmpty(value))
            {
                return new List<string>();
            }
            else
            {
                return value.Split(ItSepLvl3.ToCharArray()).ToList();
            }
        }
        public Dictionary<string, string> GetBasicAssetsListForPrefix(uint prefix)
        {
            if (basicAssetsList == null) LoadBasicAssets();
            return GetAssetListForPrefix(prefix).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public Dictionary<string, string> GetBasicAssetsDictionary()
        {
            if (basicAssetsList == null) LoadBasicAssets();
            return basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public void AddAssetToPrefixList(uint prefix, string assetId)
        {
            var temp = GetAssetListForPrefix(prefix);
            temp.Add(assetId);
            SafeSet(prefix, PrefixConfigIndex.MODELS, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void RemoveAssetFromPrefixList(uint prefix, string assetId)
        {
            var temp = GetAssetListForPrefix(prefix);
            if (!temp.Contains(assetId)) return;
            temp.Remove(assetId);
            SafeSet(prefix, PrefixConfigIndex.MODELS, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void RemoveAllAssetsFromPrefixList(uint prefix)
        {
            SafeSet(prefix, PrefixConfigIndex.MODELS, "");
        }
        public void UseDefaultAssetsForPrefixList(uint prefix)
        {
            SafeCleanProperty(prefix, PrefixConfigIndex.MODELS);
        }
        #endregion

        #region Vehicle Utils
        public VehicleInfo GetRandomModel(uint prefix)
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
                RemoveAssetFromPrefixList(prefix, model);
                return GetRandomModel(prefix);
            }
            return saida;
        }
        private int getCapacity(VehicleInfo info, bool noLoop = false)
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
        private void LoadBasicAssets()
        {
            basicAssetsList = new List<string>();

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("LoadBasicAssets: pre prefab read");
            for (uint num = 0u; (ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount()); num += 1u)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && definition.isFromSystem(prefab) && !IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
        }
        #endregion

    }

    public sealed class TLMTransportExtensionBus : TLMTransportExtension<TLMSysDefBus, TLMTransportExtensionBus> { }
    public sealed class TLMTransportExtensionBlimp : TLMTransportExtension<TLMSysDefBlimp, TLMTransportExtensionBlimp> { }
    public sealed class TLMTransportExtensionEvacBus : TLMTransportExtension<TLMSysDefBlimp, TLMTransportExtensionEvacBus> { }
    public sealed class TLMTransportExtensionFerry : TLMTransportExtension<TLMSysDefFerry, TLMTransportExtensionFerry> { }
    public sealed class TLMTransportExtensionMetro : TLMTransportExtension<TLMSysDefMetro, TLMTransportExtensionMetro> { }
    public sealed class TLMTransportExtensionMonorail : TLMTransportExtension<TLMSysDefMonorail, TLMTransportExtensionMonorail> { }
    public sealed class TLMTransportExtensionPlane : TLMTransportExtension<TLMSysDefPlane, TLMTransportExtensionPlane> { }
    public sealed class TLMTransportExtensionShip : TLMTransportExtension<TLMSysDefShip, TLMTransportExtensionShip> { }
    public sealed class TLMTransportExtensionTrain : TLMTransportExtension<TLMSysDefTrain, TLMTransportExtensionTrain> { }
    public sealed class TLMTransportExtensionTram : TLMTransportExtension<TLMSysDefTram, TLMTransportExtensionTram> { }

    public sealed class TLMTransportExtensionUtils
    {

        public static void RemoveAllUnwantedVehicles()
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
                    var modelList = def.GetTransportExtension().GetAssetListForPrefix(prefix);
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
    }

    public interface ITLMTransportExtension
    {
        string GetPrefixName(uint prefix);
        void SetPrefixName(uint prefix, string name);

        uint[] GetBudgetsMultiplier(uint prefix);
        uint GetBudgetMultiplierForHour(uint prefix, int hour);
        void SetBudgetMultiplier(uint prefix, uint[] multipliers);

        uint GetTicketPrice(uint prefix);
        uint GetDefaultTicketPrice();
        void SetTicketPrice(uint prefix, uint price);

        List<string> GetAssetListForPrefix(uint prefix);
        Dictionary<string, string> GetBasicAssetsListForPrefix(uint prefix);
        Dictionary<string, string> GetBasicAssetsDictionary();
        void AddAssetToPrefixList(uint prefix, string assetId);
        void RemoveAssetFromPrefixList(uint prefix, string assetId);
        void RemoveAllAssetsFromPrefixList(uint prefix);
        void UseDefaultAssetsForPrefixList(uint prefix);

        VehicleInfo GetRandomModel(uint prefix);
    }

    public enum PrefixConfigIndex
    {
        MODELS,
        PREFIX_NAME,
        BUDGET_MULTIPLIER,
        TICKET_PRICE
    }
}
