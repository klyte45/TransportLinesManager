using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.Extensors;
using static Klyte.TransportLinesManager.Extensors.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMPassengerAgeReportLine : BaseReportLine<AgePassengerReport>
    {
        private UILabel m_child;
        private UILabel m_teens;
        private UILabel m_young;
        private UILabel m_adult;
        private UILabel m_elder;
        private UILabel m_total;

        protected override void SetDataInternal(TLMTransportLineStatusesManager.AgePassengerReport data)
        {
            m_child.text = data.Child.ToString();
            m_teens.text = data.Teen.ToString();
            m_young.text = data.Young.ToString();
            m_adult.text = data.Adult.ToString();
            m_elder.text = data.Elder.ToString();
            m_total.text = data.Total.ToString();
        }

        protected override void AsTitleInternal()
        {
            m_child.text = Locale.Get("K45_TLM_AGE_REPORT_COLUMN_CHILD_SHORT");
            m_teens.text = Locale.Get("K45_TLM_AGE_REPORT_COLUMN_TEENS_SHORT");
            m_young.text = Locale.Get("K45_TLM_AGE_REPORT_COLUMN_YOUNG_SHORT");
            m_adult.text = Locale.Get("K45_TLM_AGE_REPORT_COLUMN_ADULT_SHORT");
            m_elder.text = Locale.Get("K45_TLM_AGE_REPORT_COLUMN_ELDER_SHORT");
            m_total.text = Locale.Get("K45_TLM_AGE_REPORT_COLUMN_TOTAL_SHORT");

            m_child.textAlignment = UIHorizontalAlignment.Center;
            m_teens.textAlignment = UIHorizontalAlignment.Center;
            m_young.textAlignment = UIHorizontalAlignment.Center;
            m_adult.textAlignment = UIHorizontalAlignment.Center;
            m_elder.textAlignment = UIHorizontalAlignment.Center;
            m_total.textAlignment = UIHorizontalAlignment.Center;
        }

        protected override void AddColumns(ref float xAdvance)
        {
            xAdvance += InitField(out m_child, "Ch", "ZONEDBUILDING_CHILDREN", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 6);
            xAdvance += InitField(out m_teens, "Tn", "ZONEDBUILDING_TEENS", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 6);
            xAdvance += InitField(out m_young, "YA", "ZONEDBUILDING_YOUNGS", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 6);
            xAdvance += InitField(out m_adult, "Ad", "ZONEDBUILDING_ADULTS", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 6);
            xAdvance += InitField(out m_elder, "Ed", "ZONEDBUILDING_SENIORS", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 6);
            xAdvance += InitField(out m_total, "Tt", "INFO_PUBLICTRANSPORT_TOTAL", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 6);
        }
    }

}

