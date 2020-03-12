using ColossalFramework.Globalization;
using ColossalFramework.UI;
using static Klyte.TransportLinesManager.Extensors.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMPassengerWealthReportLine : BaseReportLine<WealthPassengerReport>
    {
        private UILabel m_low;
        private UILabel m_med;
        private UILabel m_hgh;
        private UILabel m_total;


        protected override void SetDataInternal(WealthPassengerReport data)
        {
            m_low.text = data.Low.ToString();
            m_med.text = data.Medium.ToString();
            m_hgh.text = data.High.ToString();
            m_total.text = data.Total.ToString();
        }

        protected override void AsTitleInternal()
        {
            m_low.text = "§";
            m_med.text = "§§";
            m_hgh.text = "§§§";
            m_total.text = Locale.Get("INFO_PUBLICTRANSPORT_TOTAL");

            m_low.textAlignment = UIHorizontalAlignment.Center;
            m_med.textAlignment = UIHorizontalAlignment.Center;
            m_hgh.textAlignment = UIHorizontalAlignment.Center;
            m_total.textAlignment = UIHorizontalAlignment.Center;
        }

        protected override void AddColumns(ref float xAdvance)
        {
            xAdvance += InitField(out m_low, "Low", "INFO_CONNECTIONS_LOWWEALTH", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 4);
            xAdvance += InitField(out m_med, "Med", "INFO_CONNECTIONS_MEDIUMWEALTH", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 4);
            xAdvance += InitField(out m_hgh, "Hgh", "INFO_CONNECTIONS_HIGHWEALTH", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 4);
            xAdvance += InitField(out m_total, "Tot", "INFO_PUBLICTRANSPORT_TOTAL", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 4);
        }
    }
}



