using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMPassengerReportLine : MonoBehaviour
    {
        private static readonly Color32 BackgroundColor = new Color32(49, 52, 58, 255);
        private UIPanel m_container;
        private UILabel m_dateTime;
        private UILabel m_students;
        private UILabel m_tourists;
        private UILabel m_total;
        private UIPanel m_background;

        public void Awake()
        {
            m_container = GetComponent<UIPanel>();
            m_container.width = transform.GetComponentInParent<UIComponent>().width;
            m_container.height = 30;
            m_container.autoLayout = false;
            m_container.autoLayoutDirection = LayoutDirection.Horizontal;
            m_container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_container.wrapLayout = false;
            m_container.name = "FinanceReportLine";
            m_container.eventSizeChanged += (x, y) => m_background.size = y;

            KlyteMonoUtils.CreateUIElement(out m_background, transform, "BG");
            m_background.backgroundSprite = "InfoviewPanel";

            Color32 backgroundColor = BackgroundColor;
            backgroundColor.a = (byte) ((m_container.zOrder % 2 != 0) ? 127 : 255);
            m_background.color = backgroundColor;
            m_background.relativePosition = Vector3.zero;
            m_background.size = m_container.size;

            KlyteMonoUtils.CreateUIElement(out m_dateTime, m_container.transform, "DateTime");
            m_dateTime.autoSize = true;
            m_dateTime.minimumSize = new Vector2(80, 30);
            m_dateTime.textScale = 0.7f;
            m_dateTime.textAlignment = UIHorizontalAlignment.Center;
            m_dateTime.relativePosition = new Vector3(0, 0);
            m_dateTime.verticalAlignment = UIVerticalAlignment.Middle;
            m_dateTime.padding = new RectOffset(3, 3, 5, 3);
            m_dateTime.text = "00:00\n25/12/2020";
            KlyteMonoUtils.LimitWidthAndBox(m_dateTime);

            KlyteMonoUtils.CreateUIElement(out m_students, m_container.transform, "Income");
            m_students.autoSize = false;
            m_students.minimumSize = new Vector2(80, 30);
            m_students.textScale = 1f;
            m_students.relativePosition = new Vector3(92, 0);
            m_students.textAlignment = UIHorizontalAlignment.Right;
            m_students.verticalAlignment = UIVerticalAlignment.Middle;
            m_students.padding = new RectOffset(3, 3, 5, 3);
            m_students.text = 100000f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_students);


            KlyteMonoUtils.CreateUIElement(out m_tourists, m_container.transform, "Expense");
            m_tourists.autoSize = false;
            m_tourists.minimumSize = new Vector2(80, 30);
            m_tourists.textScale = 1f;
            m_tourists.relativePosition = new Vector3(184, 0);
            m_tourists.textAlignment = UIHorizontalAlignment.Right;
            m_tourists.verticalAlignment = UIVerticalAlignment.Middle;
            m_tourists.padding = new RectOffset(3, 3, 5, 3);
            m_tourists.text = 100000f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_tourists);

            KlyteMonoUtils.CreateUIElement(out m_total, m_container.transform, "Balance");
            m_total.autoSize = false;
            m_total.minimumSize = new Vector2(80, 24);
            m_total.textScale = 1f;
            m_total.relativePosition = new Vector3(270, 3);
            m_total.textAlignment = UIHorizontalAlignment.Right;
            m_total.verticalAlignment = UIVerticalAlignment.Middle;
            m_total.padding = new RectOffset(3, 3, 5, 3);
            m_total.text = 10f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_total);
        }

        public void SetData(TLMTransportLineStatusesManager.PassengerReport data, bool showDaytime, bool realtimeEnabled)
        {
            if (showDaytime && !realtimeEnabled)
            {
                m_dateTime.text = $"{FloatToHour(data.StartDayTime)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : FloatToHour(data.EndDayTime))}";
            }
            else
            {
                m_dateTime.text = $"{data.StartDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : data.EndDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo))}";
            }
            m_students.text = data.Student.ToString();
            m_tourists.text = data.Tourists.ToString();
            m_total.text = data.Total.ToString();
        }

        public void SetDataTotalizer(TLMTransportLineStatusesManager.PassengerReport data)
        {
            m_dateTime.text = Locale.Get("K45_TLM_BUDGET_REPORT_TOTALIZER");
            m_students.text = data.Student.ToString();
            m_tourists.text = data.Tourists.ToString();
            m_total.text = data.Total.ToString();
            m_background.color = new Color32(15, 20, 30, 255);
        }

        public void AsTitle()
        {
            m_dateTime.localeID = "K45_TLM_FINANCE_REPORT_TITLE_DATE";
            m_students.localeID = "K45_TLM_PASSENGER_REPORT_TITLE_STUDENT";
            m_tourists.localeID = "K45_TLM_PASSENGER_REPORT_TITLE_TOURIST";
            m_total.localeID = "K45_TLM_PASSENGER_REPORT_TITLE_TOTAL";

            m_dateTime.textAlignment = UIHorizontalAlignment.Center;
            m_students.textAlignment = UIHorizontalAlignment.Center;
            m_tourists.textAlignment = UIHorizontalAlignment.Center;
            m_total.textAlignment = UIHorizontalAlignment.Center;
            
            Destroy(m_background.gameObject);
        }

        public string FloatToHour(float time) => $"{((int) time).ToString("00")}:{((int) (time % 1 * 60)).ToString("00")}";

    }

}

