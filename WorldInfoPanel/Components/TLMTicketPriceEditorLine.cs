using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.Xml;

namespace Klyte.TransportLinesManager.UI
{
    public class TLMTicketPriceEditorLine : TLMBaseSliderEditorLine<TLMTicketPriceEditorLine, TicketPriceEntryXml>
    {
        public const string TICKET_PRICE_LINE_TEMPLATE = "TLM_TicketPriceLineTemplate";


        public static void EnsureTemplate() => EnsureTemplate(TICKET_PRICE_LINE_TEMPLATE);
        public override string GetValueFormat(ref TransportLine t) => Entry.Value == 0 ? Locale.Get("K45_TLM_DEFAULT_TICKET_VALUE") : (Entry.Value / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
    }

}

