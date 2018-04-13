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
using Klyte.TransportLinesManager.Extensors.TransportLineExt;

namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    internal interface ITLMTransportTypeExtension : IAssetSelectorExtension, ITicketPriceExtension, INameableExtension, IBudgetableExtension { }

    internal abstract class TLMTransportTypeExtension<TSD, SG> : ExtensionInterfaceDefaultImpl<PrefixConfigIndex, SG>, ITLMTransportTypeExtension where TSD : TLMSysDef<TSD>, new() where SG : TLMTransportTypeExtension<TSD, SG>
    {

        protected override TLMConfigWarehouse.ConfigIndex ConfigIndexKey
        {
            get {
                var tsd = definition;
                return TLMConfigWarehouse.getConfigAssetsForAI(ref tsd);
            }
        }
        protected override bool AllowGlobal { get { return false; } }

        private List<string> basicAssetsList;

        private TransportSystemDefinition definition => Singleton<TSD>.instance.GetTSD();

        #region Prefix Name
        public string GetName(uint prefix)
        {
            return SafeGet(prefix, PrefixConfigIndex.PREFIX_NAME);
        }
        public void SetName(uint prefix, string name)
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
        public uint GetDefaultTicketPrice(uint x = 0)
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
                    if (TLMSingleton.instance != null && TLMSingleton.debugMode) TLMUtils.doLog("subservice not found: {0}", definition.subService);
                    return 103;
            }

        }
        public void SetTicketPrice(uint prefix, uint price)
        {
            SafeSet(prefix, PrefixConfigIndex.TICKET_PRICE, price.ToString());
        }
        #endregion

        #region Asset List
        public List<string> GetAssetList(uint prefix)
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
        public Dictionary<string, string> GetSelectedBasicAssets(uint prefix)
        {
            if (basicAssetsList == null) LoadBasicAssets();
            return GetAssetList(prefix).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", TLMUtils.getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public Dictionary<string, string> GetAllBasicAssets(uint nil = 0)
        {
            if (basicAssetsList == null) LoadBasicAssets();
            return basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", TLMUtils.getCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public void AddAsset(uint prefix, string assetId)
        {
            var temp = GetAssetList(prefix);
            if (temp.Contains(assetId)) return;
            temp.Add(assetId);
            SafeSet(prefix, PrefixConfigIndex.MODELS, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void RemoveAsset(uint prefix, string assetId)
        {
            var temp = GetAssetList(prefix);
            if (!temp.Contains(assetId)) return;
            temp.RemoveAll(x => x == assetId);
            SafeSet(prefix, PrefixConfigIndex.MODELS, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void UseDefaultAssets(uint prefix)
        {
            SafeCleanProperty(prefix, PrefixConfigIndex.MODELS);
        }
        public VehicleInfo GetAModel(ushort lineID)
        {
            var prefix = TLMLineUtils.getPrefix(lineID);
            VehicleInfo info = null;
            List<string> assetList = GetAssetList(prefix);
            while (info == null && assetList.Count > 0)
            {
                info = TLMUtils.GetRandomModel(assetList, out string modelName);
                if (info == null)
                {
                    RemoveAsset(prefix, modelName);
                    assetList = GetAssetList(prefix);
                }
            }
            return info;
        }
        public void LoadBasicAssets()
        {
            var tsd = definition;
            basicAssetsList = TLMUtils.LoadBasicAssets(ref tsd);
        }
        #endregion

    }

    internal sealed class TLMTransportTypeExtensionNorBus : TLMTransportTypeExtension<TLMSysDefNorBus, TLMTransportTypeExtensionNorBus> { }
    internal sealed class TLMTransportTypeExtensionNorBlp : TLMTransportTypeExtension<TLMSysDefNorBlp, TLMTransportTypeExtensionNorBlp> { }
    internal sealed class TLMTransportTypeExtensionEvcBus : TLMTransportTypeExtension<TLMSysDefEvcBus, TLMTransportTypeExtensionEvcBus> { }
    internal sealed class TLMTransportTypeExtensionNorFer : TLMTransportTypeExtension<TLMSysDefNorFer, TLMTransportTypeExtensionNorFer> { }
    internal sealed class TLMTransportTypeExtensionNorMet : TLMTransportTypeExtension<TLMSysDefNorMet, TLMTransportTypeExtensionNorMet> { }
    internal sealed class TLMTransportTypeExtensionNorMnr : TLMTransportTypeExtension<TLMSysDefNorMnr, TLMTransportTypeExtensionNorMnr> { }
    internal sealed class TLMTransportTypeExtensionNorPln : TLMTransportTypeExtension<TLMSysDefNorPln, TLMTransportTypeExtensionNorPln> { }
    internal sealed class TLMTransportTypeExtensionNorShp : TLMTransportTypeExtension<TLMSysDefNorShp, TLMTransportTypeExtensionNorShp> { }
    internal sealed class TLMTransportTypeExtensionNorTrn : TLMTransportTypeExtension<TLMSysDefNorTrn, TLMTransportTypeExtensionNorTrn> { }
    internal sealed class TLMTransportTypeExtensionNorTrm : TLMTransportTypeExtension<TLMSysDefNorTrm, TLMTransportTypeExtensionNorTrm> { }
    internal sealed class TLMTransportTypeExtensionTouBus : TLMTransportTypeExtension<TLMSysDefTouBus, TLMTransportTypeExtensionTouBus> { }
    internal sealed class TLMTransportTypeExtensionTouPed : TLMTransportTypeExtension<TLMSysDefTouPed, TLMTransportTypeExtensionTouPed> { }
    internal sealed class TLMTransportTypeExtensionTouBal : TLMTransportTypeExtension<TLMSysDefTouBal, TLMTransportTypeExtensionTouBal> { }
    internal sealed class TLMTransportTypeExtensionNorCcr : TLMTransportTypeExtension<TLMSysDefNorCcr, TLMTransportTypeExtensionNorCcr> { }
    internal sealed class TLMTransportTypeExtensionNorTax : TLMTransportTypeExtension<TLMSysDefNorTax, TLMTransportTypeExtensionNorTax> { }

    internal sealed class TLMTransportExtensionUtils
    {

        public static void RemoveAllUnwantedVehicles()
        {
            for (ushort lineId = 1; lineId < Singleton<TransportManager>.instance.m_lines.m_size; lineId++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                {
                    uint idx;
                    IAssetSelectorExtension extension;
                    if (TLMTransportLineExtension.instance.GetUseCustomConfig(lineId))
                    {
                        idx = lineId;
                        extension = TLMTransportLineExtension.instance;
                    }
                    else
                    {
                        idx = TLMLineUtils.getPrefix(lineId);
                        var def = TransportSystemDefinition.from(lineId);
                        extension = def.GetTransportExtension();
                    }

                    TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
                    var modelList = extension.GetAssetList(idx);
                    VehicleManager vm = Singleton<VehicleManager>.instance;
                    VehicleInfo info = vm.m_vehicles.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].GetVehicle(0)].Info;

                    if (TLMSingleton.instance != null && TLMSingleton.debugMode) TLMUtils.doLog("removeAllUnwantedVehicles: models found: {0}", modelList == null ? "?!?" : modelList.Count.ToString());

                    if (modelList.Count > 0)
                    {
                        Dictionary<ushort, VehicleInfo> vehiclesToRemove = new Dictionary<ushort, VehicleInfo>();
                        for (int i = 0; i < tl.CountVehicles(lineId); i++)
                        {
                            var vehicle = tl.GetVehicle(i);
                            if (vehicle != 0)
                            {
                                VehicleInfo info2 = vm.m_vehicles.m_buffer[(int)vehicle].Info;
                                if (!modelList.Contains(info2.name))
                                {
                                    vehiclesToRemove[vehicle] = info2;
                                }
                            }
                        }
                        foreach (var item in vehiclesToRemove)
                        {
                            item.Value.m_vehicleAI.SetTransportLine(item.Key, ref vm.m_vehicles.m_buffer[item.Key], 0);
                        }
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
