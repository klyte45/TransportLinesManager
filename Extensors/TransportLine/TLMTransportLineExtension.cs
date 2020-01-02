using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensors.TransportLineExt
{
    internal enum TLMTransportLineFlags : uint
    {
        ZERO_BUDGET_CURRENT = 0x80000000
    }

    public class TLMTransportLineExtension : DataExtensorBase<TLMTransportLineExtension>, ISafeGettable<TransportLineConfiguration>, ISafeGettable<IAssetSelectorStorage>, IAssetSelectorExtension, IBudgetableExtension, ITicketPriceExtension
    {
        [XmlElement("Configurations")]
        public SimpleNonSequentialList<TransportLineConfiguration> Configurations { get; set; } = new SimpleNonSequentialList<TransportLineConfiguration>();
        internal void SafeCleanEntry(ushort lineID) => Configurations[lineID] = new TransportLineConfiguration();
        public TransportLineConfiguration SafeGet(uint lineId)
        {
            if (!Configurations.ContainsKey(lineId))
            {
                Configurations[lineId] = new TransportLineConfiguration();
            }
            return Configurations[lineId];
        }
        IAssetSelectorStorage ISafeGettable<IAssetSelectorStorage>.SafeGet(uint index) => SafeGet(index);
        IBudgetStorage ISafeGettable<IBudgetStorage>.SafeGet(uint index) => SafeGet(index);
        ITicketPriceStorage ISafeGettable<ITicketPriceStorage>.SafeGet(uint index) => SafeGet(index);

        public override string SaveId => $"K45_TLM_TLMTransportLineExtension";

        private Dictionary<TransportSystemDefinition, List<string>> m_basicAssetsList = new Dictionary<TransportSystemDefinition, List<string>>();

        public void SetUseCustomConfig(ushort lineId, bool value) => SafeGet(lineId).IsCustom = value;

        public bool IsUsingCustomConfig(ushort lineId) => SafeGet(lineId).IsCustom;

        #region Asset List
        public List<string> GetBasicAssetList(uint rel)
        {

            var tsd = TransportSystemDefinition.From(rel);
            if (!m_basicAssetsList.ContainsKey(tsd))
            {
                m_basicAssetsList[tsd] = TLMUtils.LoadBasicAssets(ref tsd);
            }
            return m_basicAssetsList[tsd];
        }
        public Dictionary<string, string> GetSelectedBasicAssets(uint lineId) => ExtensionStaticExtensionMethods.GetAssetList(this, lineId).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x) != null).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        public Dictionary<string, string> GetAllBasicAssets(uint lineId)
        {
            var tsd = TransportSystemDefinition.From(lineId);
            if (!m_basicAssetsList.ContainsKey(tsd))
            {
                m_basicAssetsList[tsd] = TLMUtils.LoadBasicAssets(ref tsd);
            }

            return m_basicAssetsList[tsd].ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        }
        public VehicleInfo GetAModel(ushort lineId)
        {
            VehicleInfo info = null;
            List<string> assetList = ExtensionStaticExtensionMethods.GetAssetList(this, lineId);
            while (info == null && assetList.Count > 0)
            {
                info = VehicleUtils.GetRandomModel(assetList, out string modelName);
                if (info == null)
                {
                    ExtensionStaticExtensionMethods.RemoveAsset(this, lineId, modelName);
                    assetList = ExtensionStaticExtensionMethods.GetAssetList(this, lineId);
                }
            }
            return info;
        }

        #endregion

        #region Ticket Price

        public uint GetDefaultTicketPrice(uint lineId = 0)
        {
            var tsd = TransportSystemDefinition.From(lineId);
            switch (tsd.SubService)
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
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Blimp)
                    {
                        return 100;
                    }
                    else
                    {
                        return 1000;
                    }
                case ItemClass.SubService.PublicTransportShip:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Ferry)
                    {
                        return 100;
                    }
                    else
                    {
                        return 500;
                    }
                case ItemClass.SubService.PublicTransportTours:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Car)
                    {
                        return 100;
                    }
                    else if (tsd.VehicleType == VehicleInfo.VehicleType.None)
                    {
                        return 0;
                    }
                    return 102;
                default:
                    TLMUtils.doLog("subservice not found: {0}", tsd.SubService);
                    return 103;
            }

        }

        #endregion

    }


    public class TransportLineConfiguration : IAssetSelectorStorage, IBudgetStorage, ITicketPriceStorage
    {
        [XmlAttribute("isCustom")]
        public bool IsCustom { get; set; } = false;
        [XmlElement("Budget")]
        public TimeableList<BudgetEntryXml> BudgetEntries { get; set; } = new TimeableList<BudgetEntryXml>();
        [XmlElement("AssetsList")]
        public SimpleXmlList<string> AssetList { get; set; } = new SimpleXmlList<string>();
        [XmlElement("TicketPrices")]
        public TimeableList<TicketPriceEntryXml> TicketPriceEntries { get; set; }
    }
}
