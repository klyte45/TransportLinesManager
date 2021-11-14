using ColossalFramework.UI;
using static Klyte.TransportLinesManager.Extensions.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMStudentTouristsReportLine : BaseReportLine<StudentsTouristsReport>
    {
        private UILabel m_students;
        private UILabel m_tourists;
        private UILabel m_total;

        protected override void SetDataInternal(StudentsTouristsReport data)
        {
            m_students.text = data.Student.ToString();
            m_tourists.text = data.Tourists.ToString();
            m_total.text = data.Total.ToString();
        }

        protected override void AsTitleInternal()
        {
            m_students.localeID = "K45_TLM_PASSENGER_REPORT_TITLE_STUDENT";
            m_tourists.localeID = "K45_TLM_PASSENGER_REPORT_TITLE_TOURIST";
            m_total.localeID = "K45_TLM_PASSENGER_REPORT_TITLE_TOTAL";

            m_students.textAlignment = UIHorizontalAlignment.Center;
            m_tourists.textAlignment = UIHorizontalAlignment.Center;
            m_total.textAlignment = UIHorizontalAlignment.Center;
        }

        protected override void AddColumns(ref float xAdvance)
        {
            xAdvance += InitField(out m_students, "Student", "K45_TLM_PASSENGER_REPORT_TITLE_STUDENT", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
            xAdvance += InitField(out m_tourists, "Tourist", "K45_TLM_PASSENGER_REPORT_TITLE_TOURIST", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
            xAdvance += InitField(out m_total, "Tot", "K45_TLM_PASSENGER_REPORT_TITLE_TOTAL", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
        }

    }

}

