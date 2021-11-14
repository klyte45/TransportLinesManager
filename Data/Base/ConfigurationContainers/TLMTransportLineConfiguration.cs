using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMTransportLineConfiguration : IBasicExtensionStorage
    {
        private string customIdentifier;
        private bool isCustom = false;

        [XmlAttribute("isCustom")]
        public bool IsCustom
        {
            get => isCustom; set
            {
                if (!value && isCustom)
                {
                    DisplayAbsoluteValues = false;
                    BudgetEntries = new TimeableList<BudgetEntryXml>();
                    AssetList = new SimpleXmlList<string>();
                    TicketPriceEntries = new TimeableList<TicketPriceEntryXml>();
                    DepotsAllowed = new SimpleXmlHashSet<ushort>();
                }
                isCustom = value;
            }
        }
        [XmlAttribute("displayAbsoluteValues")]
        public bool DisplayAbsoluteValues { get; set; } = false;
        [XmlElement("Budget")]
        public TimeableList<BudgetEntryXml> BudgetEntries { get; set; } = new TimeableList<BudgetEntryXml>();
        [XmlElement("AssetsList")]
        public SimpleXmlList<string> AssetList { get; set; } = new SimpleXmlList<string>();
        [XmlElement("TicketPrices")]
        public TimeableList<TicketPriceEntryXml> TicketPriceEntries { get; set; } = new TimeableList<TicketPriceEntryXml>();
        [XmlElement("DepotsAllowed")]
        public SimpleXmlHashSet<ushort> DepotsAllowed { get; set; } = new SimpleXmlHashSet<ushort>();

        [XmlAttribute("isZeroed")]
        public bool IsZeroed { get; set; }

        [XmlAttribute("customIdentifier")]
        public string CustomCode
        {
            get => customIdentifier; set
            {
                customIdentifier = value.TrimToNull();
                if (!LoadingManager.instance.m_currentlyLoading)
                {
                    TLMController.Instance.SharedInstance.OnLineSymbolParameterChanged();
                }
            }
        }
    }

}
