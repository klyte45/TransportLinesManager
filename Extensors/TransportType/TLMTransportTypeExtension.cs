using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    public interface ITLMTransportTypeExtension : IAssetSelectorExtension, ITicketPriceExtension, INameableExtension, IBudgetableExtension, IUseColorForModelExtension, IColorSelectableExtension, ICustomPaletteExtension, ICustomGeometricFormatExtension { }

    public abstract class TLMTransportTypeExtension<TSD, SG> : ExtensionInterfaceDefaultImpl<PrefixConfigIndex, SG>, ITLMTransportTypeExtension where TSD : TLMSysDef<TSD>, new() where SG : TLMTransportTypeExtension<TSD, SG>
    {

        protected override TLMConfigWarehouse.ConfigIndex ConfigIndexKey
        {
            get {
                TransportSystemDefinition tsd = definition;
                return TLMConfigWarehouse.getConfigAssetsForAI(ref tsd);
            }
        }
        protected override bool AllowGlobal => false;

        private List<string> basicAssetsList;

        private TransportSystemDefinition definition => Singleton<TSD>.instance.GetTSD();

        #region Prefix Name
        public string GetName(uint prefix) => SafeGet(prefix, PrefixConfigIndex.PREFIX_NAME);
        public void SetName(uint prefix, string name) => SafeSet(prefix, PrefixConfigIndex.PREFIX_NAME, name);
        #endregion

        #region Budget Multiplier
        public uint[] GetBudgetsMultiplier(uint prefix)
        {
            string value = SafeGet(prefix, PrefixConfigIndex.BUDGET_MULTIPLIER);
            if (value == null)
            {
                return new uint[] { 100 };
            }

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
        public uint GetBudgetMultiplierForHour(uint prefix, float hour)
        {
            uint[] savedMultipliers = GetBudgetsMultiplier(prefix);
            if (savedMultipliers.Length == 1)
            {
                return savedMultipliers[0];
            }
            else if (savedMultipliers.Length == 8)
            {
                int refMultiplierIdx = (((int) hour + 23) / 3) % 8;
                float phasePercentage = (hour + 23) % 3;
                if (phasePercentage < .5f)
                {
                    return (uint) Mathf.Lerp(savedMultipliers[(refMultiplierIdx + 7) % 8], savedMultipliers[refMultiplierIdx], 0.5f + phasePercentage);
                }
                else if (phasePercentage > 2.5f)
                {
                    return (uint) Mathf.Lerp(savedMultipliers[refMultiplierIdx], savedMultipliers[(refMultiplierIdx + 1) % 8], (phasePercentage) - 2.5f);
                }
                else
                {
                    return savedMultipliers[refMultiplierIdx];
                }
            }
            return 100;
        }
        public void SetBudgetMultiplier(uint prefix, uint[] multipliers) => SafeSet(prefix, PrefixConfigIndex.BUDGET_MULTIPLIER, string.Join(ItSepLvl3, multipliers.Select(x => x.ToString()).ToArray()));
        #endregion

        #region Ticket Price
        public uint GetTicketPrice(uint prefix)
        {
            if (uint.TryParse(SafeGet(prefix, PrefixConfigIndex.TICKET_PRICE), out uint result) && result > 0)
            {
                return result;
            }
            return GetDefaultTicketPrice();
        }
        public uint GetDefaultTicketPrice(uint x = 0)
        {
            int savedVal = TLMConfigWarehouse.instance.GetInt(TLMConfigWarehouse.ConfigIndex.DEFAULT_TICKET_PRICE | Singleton<TSD>.instance.GetTSD().toConfigIndex());
            if (savedVal > 0)
            {
                return (uint) savedVal;
            }
            return (uint) TransportManager.instance.GetTransportInfo(Singleton<TSD>.instance.GetTSD().transportType).m_ticketPrice;
        }
        public void SetTicketPrice(uint prefix, uint price) => SafeSet(prefix, PrefixConfigIndex.TICKET_PRICE, price.ToString());
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
            if (basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            return GetAssetList(prefix).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public Dictionary<string, string> GetAllBasicAssets(uint nil = 0)
        {
            if (basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            return basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public void AddAsset(uint prefix, string assetId)
        {
            List<string> temp = GetAssetList(prefix);
            if (temp.Contains(assetId))
            {
                return;
            }

            temp.Add(assetId);
            SafeSet(prefix, PrefixConfigIndex.MODELS, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void RemoveAsset(uint prefix, string assetId)
        {
            List<string> temp = GetAssetList(prefix);
            if (!temp.Contains(assetId))
            {
                return;
            }

            temp.RemoveAll(x => x == assetId);
            SafeSet(prefix, PrefixConfigIndex.MODELS, string.Join(ItSepLvl3, temp.ToArray()));
        }
        public void UseDefaultAssets(uint prefix) => SafeCleanProperty(prefix, PrefixConfigIndex.MODELS);
        public VehicleInfo GetAModel(ushort lineID)
        {
            uint prefix = TLMLineUtils.getPrefix(lineID);
            VehicleInfo info = null;
            List<string> assetList = GetAssetList(prefix);
            while (info == null && assetList.Count > 0)
            {
                info = VehicleUtils.GetRandomModel(assetList, out string modelName);
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
            TransportSystemDefinition tsd = definition;
            basicAssetsList = TLMUtils.LoadBasicAssets(ref tsd);
        }

        #endregion

        #region Color
        public Color GetColor(uint prefix) => SerializationUtils.DeserializeColor(SafeGet(prefix, PrefixConfigIndex.COLOR), ItSepLvl3);

        public void SetColor(uint prefix, Color value)
        {
            if (value.a < 1)
            {
                CleanColor(prefix);
            }
            else
            {
                SafeSet(prefix, PrefixConfigIndex.COLOR, SerializationUtils.SerializeColor(value, ItSepLvl3));
            }
        }

        public void CleanColor(uint prefix) => SafeCleanProperty(prefix, PrefixConfigIndex.COLOR);
        #endregion

        #region Use Color For Model
        public bool IsUsingColorForModel(uint prefix) => bool.TryParse(SafeGet(prefix, PrefixConfigIndex.USE_COLOR_FOR_MODEL), out bool result) && result;

        public void SetUsingColorForModel(uint prefix, bool value) => SafeSet(prefix, PrefixConfigIndex.USE_COLOR_FOR_MODEL, value.ToString());
        #endregion

        #region Custom Palette
        public string GetCustomPalette(uint prefix) => SafeGet(prefix, PrefixConfigIndex.CUSTOM_PALETTE) ?? string.Empty;

        public void SetCustomPalette(uint prefix, string paletteName) => SafeSet(prefix, PrefixConfigIndex.CUSTOM_PALETTE, paletteName);

        #endregion

        #region Custom Format
        public LineIconSpriteNames GetCustomFormat(uint prefix)
        {
            string valueSet = SafeGet(prefix, PrefixConfigIndex.CUSTOM_FORMAT);
            if (valueSet == null || !Enum.IsDefined(typeof(LineIconSpriteNames), valueSet))
            {
                return LineIconSpriteNames.NULL;
            }
            else
            {
                return (LineIconSpriteNames) Enum.Parse(typeof(LineIconSpriteNames), valueSet);
            }
        }

        public void SetCustomFormat(uint prefix, LineIconSpriteNames icon) => SafeSet(prefix, PrefixConfigIndex.CUSTOM_FORMAT, icon.ToString());

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




    public enum PrefixConfigIndex
    {
        MODELS,
        PREFIX_NAME,
        BUDGET_MULTIPLIER,
        TICKET_PRICE,
        COLOR,
        USE_COLOR_FOR_MODEL,
        CUSTOM_PALETTE,
        CUSTOM_FORMAT
    }
}
