using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.Extensors;
using static Klyte.TransportLinesManager.Extensors.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMPassengerGenderReportLine : BaseReportLine<GenderPassengerReport>
    {
        private UILabel m_male;
        private UILabel m_female;
        private UILabel m_total;

        protected override void SetDataInternal(GenderPassengerReport data)
        {
            m_male.text = data.Male.ToString();
            m_female.text = data.Female.ToString();
            m_total.text = data.Total.ToString();
        }

        protected override void AsTitleInternal()
        {
            m_male.text = Locale.Get("K45_TLM_GENDER_REPORT_COLUMN_MALE");
            m_female.text = Locale.Get("K45_TLM_GENDER_REPORT_COLUMN_FEMALE");
            m_total.text = Locale.Get("INFO_PUBLICTRANSPORT_TOTAL");

            m_male.textAlignment = UIHorizontalAlignment.Center;
            m_female.textAlignment = UIHorizontalAlignment.Center;
            m_total.textAlignment = UIHorizontalAlignment.Center;
        }

        protected override void AddColumns(ref float xAdvance)
        {
            xAdvance += InitField(out m_male, "M", "K45_TLM_GENDER_REPORT_COLUMN_MALE", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
            xAdvance += InitField(out m_female, "F", "K45_TLM_GENDER_REPORT_COLUMN_FEMALE", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
            xAdvance += InitField(out m_total, "Tt", "INFO_PUBLICTRANSPORT_TOTAL", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
        }
    }

}

