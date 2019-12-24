using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    public interface ITLMTransportTypeExtension :
        IAssetSelectorExtension,
        ITicketPriceExtension,
        INameableExtension,
        IBudgetableExtension,
        IColorSelectableExtension,
        ISafeGettable<PrefixConfiguration>
    {
        #region Use Color For Model
        public bool IsUsingColorForModel(uint prefix);

        public void SetUsingColorForModel(uint prefix, bool value);
        #endregion

        #region Custom Palette
        public string GetCustomPalette(uint prefix);

        public void SetCustomPalette(uint prefix, string paletteName);

        #endregion

        #region Custom Format
        public LineIconSpriteNames GetCustomFormat(uint prefix);

        public void SetCustomFormat(uint prefix, LineIconSpriteNames icon);

        #endregion
    }
    public abstract class TLMTransportTypeExtension<TSD, SG> : DataExtensorBase<SG>, ITLMTransportTypeExtension
        where TSD : TLMSysDef<TSD>, new() where SG : TLMTransportTypeExtension<TSD, SG>, new()
    {

        private List<string> m_basicAssetsList;

        private TransportSystemDefinition Definition => Singleton<TSD>.instance.GetTSD();

        [XmlElement("Configurations")]
        public SimpleNonSequentialList<PrefixConfiguration> Configurations { get; set; } = new SimpleNonSequentialList<PrefixConfiguration>();

        public PrefixConfiguration SafeGet(uint prefix)
        {
            if (!Configurations.ContainsKey(prefix))
            {
                Configurations[prefix] = new PrefixConfiguration();
            }
            return Configurations[prefix];
        }
        IAssetSelectorStorage ISafeGettable<IAssetSelectorStorage>.SafeGet(uint index) => SafeGet(index);
        INameableStorage ISafeGettable<INameableStorage>.SafeGet(uint index) => SafeGet(index);
        ITicketPriceStorage ISafeGettable<ITicketPriceStorage>.SafeGet(uint index) => SafeGet(index);
        IBudgetStorage ISafeGettable<IBudgetStorage>.SafeGet(uint index) => SafeGet(index);
        IColorSelectableStorage ISafeGettable<IColorSelectableStorage>.SafeGet(uint index) => SafeGet(index);

        public uint GetDefaultTicketPrice(uint x = 0)
        {
            int savedVal = TLMConfigWarehouse.instance.GetInt(TLMConfigWarehouse.ConfigIndex.DEFAULT_TICKET_PRICE | Singleton<TSD>.instance.GetTSD().toConfigIndex());
            if (savedVal > 0)
            {
                return (uint) savedVal;
            }
            return (uint) TransportManager.instance.GetTransportInfo(Singleton<TSD>.instance.GetTSD().transportType).m_ticketPrice;
        }

        #region Asset List
        public Dictionary<string, string> GetSelectedBasicAssets(uint prefix)
        {
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            return ExtensionStaticExtensionMethods.GetAssetList(this, prefix).Intersect(m_basicAssetsList).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public Dictionary<string, string> GetAllBasicAssets(uint nil = 0)
        {
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            return m_basicAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public List<string> GetBasicAssetList(uint rel)
        {
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }
            return m_basicAssetsList;
        }
        public VehicleInfo GetAModel(ushort lineID)
        {
            uint prefix = TLMLineUtils.getPrefix(lineID);
            VehicleInfo info = null;
            List<string> assetList = ExtensionStaticExtensionMethods.GetAssetList(this, prefix);
            while (info == null && assetList.Count > 0)
            {
                info = VehicleUtils.GetRandomModel(assetList, out string modelName);
                if (info == null)
                {
                    ExtensionStaticExtensionMethods.RemoveAsset(this, prefix, modelName);
                    assetList = ExtensionStaticExtensionMethods.GetAssetList(this, prefix);
                }
            }
            return info;
        }
        private void LoadBasicAssets()
        {
            TransportSystemDefinition tsd = Definition;
            m_basicAssetsList = TLMUtils.LoadBasicAssets(ref tsd);
        }

        #endregion

        #region Use Color For Model
        public bool IsUsingColorForModel(uint prefix) => SafeGet(prefix).UseColorForModel;

        public void SetUsingColorForModel(uint prefix, bool value) => SafeGet(prefix).UseColorForModel = value;
        #endregion

        #region Custom Palette
        public string GetCustomPalette(uint prefix) => SafeGet(prefix).CustomPalette ?? string.Empty;

        public void SetCustomPalette(uint prefix, string paletteName) => SafeGet(prefix).CustomPalette = paletteName;

        #endregion

        #region Custom Format
        public LineIconSpriteNames GetCustomFormat(uint prefix) => SafeGet(prefix).CustomIcon;

        public void SetCustomFormat(uint prefix, LineIconSpriteNames icon) => SafeGet(prefix).CustomIcon = icon;

        #endregion

        public override string SaveId => $"K45_TLM_{GetType()}";
    }

    public sealed class TLMTransportTypeExtensionNorBus : TLMTransportTypeExtension<TLMSysDefNorBus, TLMTransportTypeExtensionNorBus> { }
    public sealed class TLMTransportTypeExtensionNorBlp : TLMTransportTypeExtension<TLMSysDefNorBlp, TLMTransportTypeExtensionNorBlp> { }
    public sealed class TLMTransportTypeExtensionEvcBus : TLMTransportTypeExtension<TLMSysDefEvcBus, TLMTransportTypeExtensionEvcBus> { }
    public sealed class TLMTransportTypeExtensionNorFer : TLMTransportTypeExtension<TLMSysDefNorFer, TLMTransportTypeExtensionNorFer> { }
    public sealed class TLMTransportTypeExtensionNorMet : TLMTransportTypeExtension<TLMSysDefNorMet, TLMTransportTypeExtensionNorMet> { }
    public sealed class TLMTransportTypeExtensionNorMnr : TLMTransportTypeExtension<TLMSysDefNorMnr, TLMTransportTypeExtensionNorMnr> { }
    public sealed class TLMTransportTypeExtensionNorPln : TLMTransportTypeExtension<TLMSysDefNorPln, TLMTransportTypeExtensionNorPln> { }
    public sealed class TLMTransportTypeExtensionNorShp : TLMTransportTypeExtension<TLMSysDefNorShp, TLMTransportTypeExtensionNorShp> { }
    public sealed class TLMTransportTypeExtensionNorTrn : TLMTransportTypeExtension<TLMSysDefNorTrn, TLMTransportTypeExtensionNorTrn> { }
    public sealed class TLMTransportTypeExtensionNorTrm : TLMTransportTypeExtension<TLMSysDefNorTrm, TLMTransportTypeExtensionNorTrm> { }
    public sealed class TLMTransportTypeExtensionTouBus : TLMTransportTypeExtension<TLMSysDefTouBus, TLMTransportTypeExtensionTouBus> { }
    public sealed class TLMTransportTypeExtensionTouPed : TLMTransportTypeExtension<TLMSysDefTouPed, TLMTransportTypeExtensionTouPed> { }
    public sealed class TLMTransportTypeExtensionTouBal : TLMTransportTypeExtension<TLMSysDefTouBal, TLMTransportTypeExtensionTouBal> { }
    public sealed class TLMTransportTypeExtensionNorCcr : TLMTransportTypeExtension<TLMSysDefNorCcr, TLMTransportTypeExtensionNorCcr> { }
    public sealed class TLMTransportTypeExtensionNorTax : TLMTransportTypeExtension<TLMSysDefNorTax, TLMTransportTypeExtensionNorTax> { }

    public class PrefixConfiguration : IAssetSelectorStorage, INameableStorage, ITicketPriceStorage, IBudgetStorage, IColorSelectableStorage
    {
        [XmlElement("Budget")]
        public TimeableList<BudgetEntryXml> BudgetEntries { get; set; } = new TimeableList<BudgetEntryXml>();
        [XmlElement("AssetsList")]
        public SimpleXmlList<string> AssetList { get; set; } = new SimpleXmlList<string>();
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("ticketPrice")]
        public uint TicketPrice { get; set; } = 0;
        [XmlAttribute("useColorForModel")]
        public bool UseColorForModel { get; set; }

        [XmlIgnore]
        public Color Color { get => m_cachedColor == default ? Color.white : m_cachedColor; set => m_cachedColor = value; }
        [XmlIgnore]
        private Color m_cachedColor;
        [XmlAttribute("color")]
        public string PropColorStr { get => m_cachedColor == default ? null : ColorExtensions.ToRGB(Color); set => m_cachedColor = value.IsNullOrWhiteSpace() ? default : (Color) ColorExtensions.FromRGB(value); }


        [XmlAttribute("customPalette")]
        public string CustomPalette { get; set; }


        [XmlIgnore]
        public LineIconSpriteNames CustomIcon { get; set; } = LineIconSpriteNames.NULL;
        [XmlAttribute("customFormat")]
        public string CustomFormatStr
        {
            get => CustomIcon.ToString();

            set {
                LineIconSpriteNames result;
                try
                {
                    result = (LineIconSpriteNames) Enum.Parse(typeof(LineIconSpriteNames), value);
                }
                catch
                {
                    result = (LineIconSpriteNames) Enum.ToObject(typeof(LineIconSpriteNames), (int.TryParse(value, out int val) ? val : 0));
                }
                CustomIcon = result;
            }
        }

    }

    public class BudgetEntryXml : ITimeable<BudgetEntryXml>
    {
        private int m_hourOfDay;
        private uint m_value;

        [XmlAttribute("startTime")]
        public int? HourOfDay
        {
            get => m_hourOfDay;
            set {
                m_hourOfDay = (value ?? -1) % 24;
                OnEntryChanged?.Invoke(this);
            }
        }

        [XmlAttribute("value")]
        public uint Value
        {
            get => m_value;
            set {
                m_value = value;
                OnEntryChanged?.Invoke(this);
            }
        }

        public event Action<BudgetEntryXml> OnEntryChanged;
    }
}
