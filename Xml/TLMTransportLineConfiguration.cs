using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMTransportLineConfiguration : IBasicExtensionStorage
    {
        [XmlAttribute("isCustom")]
        public bool IsCustom { get; set; } = false;
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
    }

}
