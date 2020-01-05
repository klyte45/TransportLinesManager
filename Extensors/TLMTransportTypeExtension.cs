using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensors
{
    public abstract class TLMTransportTypeExtension<TSD, SG> : DataExtensorBase<SG>, ITLMTransportTypeExtension
        where TSD : TLMSysDef<TSD>, new() where SG : TLMTransportTypeExtension<TSD, SG>, new()
    {

        private List<string> m_basicAssetsList;

        private TransportSystemDefinition Definition => Singleton<TSD>.instance.GetTSD();

        [XmlElement("Configurations")]
        public SimpleNonSequentialList<TLMPrefixConfiguration> Configurations { get; set; } = new SimpleNonSequentialList<TLMPrefixConfiguration>();

        public TLMPrefixConfiguration SafeGet(uint prefix)
        {
            if (!Configurations.ContainsKey(prefix))
            {
                Configurations[prefix] = new TLMPrefixConfiguration();
            }
            return Configurations[prefix];
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
            int savedVal = TLMConfigWarehouse.instance.GetInt(TLMConfigWarehouse.ConfigIndex.DEFAULT_TICKET_PRICE | Singleton<TSD>.instance.GetTSD().ToConfigIndex());
            if (savedVal > 0)
            {
                return (uint) savedVal;
            }
            return (uint) TransportManager.instance.GetTransportInfo(Singleton<TSD>.instance.GetTSD().TransportType).m_ticketPrice;
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
        public uint LineToIndex(ushort lineId) => TLMLineUtils.getPrefix(lineId);

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
}
