using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMPassengerWealthReportLine : MonoBehaviour
    {
        private static readonly Color32 BackgroundColor = new Color32(49, 52, 58, 255);
        private UIPanel m_container;
        private UILabel m_dateTime;
        private UILabel m_low;
        private UILabel m_med;
        private UILabel m_hgh;
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
            m_container.name = "WealthReportLine";
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

            KlyteMonoUtils.CreateUIElement(out m_low, m_container.transform, "Income");
            m_low.autoSize = false;
            m_low.minimumSize = new Vector2(80, 30);
            m_low.textScale = 1f;
            m_low.relativePosition = new Vector3(92, 0);
            m_low.textAlignment = UIHorizontalAlignment.Right;
            m_low.verticalAlignment = UIVerticalAlignment.Middle;
            m_low.padding = new RectOffset(3, 3, 5, 3);
            m_low.text = 100000f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_low);


            KlyteMonoUtils.CreateUIElement(out m_med, m_container.transform, "Expense");
            m_med.autoSize = false;
            m_med.minimumSize = new Vector2(80, 30);
            m_med.textScale = 1f;
            m_med.relativePosition = new Vector3(184, 0);
            m_med.textAlignment = UIHorizontalAlignment.Right;
            m_med.verticalAlignment = UIVerticalAlignment.Middle;
            m_med.padding = new RectOffset(3, 3, 5, 3);
            m_med.text = 100000f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_med);

            KlyteMonoUtils.CreateUIElement(out m_hgh, m_container.transform, "Balance");
            m_hgh.autoSize = false;
            m_hgh.minimumSize = new Vector2(80, 24);
            m_hgh.textScale = 1f;
            m_hgh.relativePosition = new Vector3(270, 3);
            m_hgh.textAlignment = UIHorizontalAlignment.Right;
            m_hgh.verticalAlignment = UIVerticalAlignment.Middle;
            m_hgh.padding = new RectOffset(3, 3, 5, 3);
            m_hgh.text = 10f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_hgh);
        }

        public void SetData(TLMTransportLineStatusesManager.WealthPassengerReport data, bool showDaytime, bool realtimeEnabled)
        {
            if (showDaytime && !realtimeEnabled)
            {
                m_dateTime.text = $"{FloatToHour(data.StartDayTime)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : FloatToHour(data.EndDayTime))}";
            }
            else
            {
                m_dateTime.text = $"{data.StartDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : data.EndDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo))}";
            }
            m_low.text = data.Low.ToString();
            m_med.text = data.Medium.ToString();
            m_hgh.text = data.High.ToString();
        }

        public void SetDataTotalizer(TLMTransportLineStatusesManager.WealthPassengerReport data)
        {
            m_dateTime.text = Locale.Get("K45_TLM_BUDGET_REPORT_TOTALIZER");
            m_low.text = data.Low.ToString();
            m_med.text = data.Medium.ToString();
            m_hgh.text = data.High.ToString();
            m_background.color = new Color32(15, 20, 30, 255);
        }

        public void AsTitle()
        {
            m_dateTime.localeID = "K45_TLM_FINANCE_REPORT_TITLE_DATE";
            m_low.text = "§";
            m_med.text = "§§";
            m_hgh.text = "§§§";

            m_dateTime.textAlignment = UIHorizontalAlignment.Center;
            m_low.textAlignment = UIHorizontalAlignment.Center;
            m_med.textAlignment = UIHorizontalAlignment.Center;
            m_hgh.textAlignment = UIHorizontalAlignment.Center;
            
            Destroy(m_background.gameObject);
        }

        public string FloatToHour(float time) => $"{((int) time).ToString("00")}:{((int) (time % 1 * 60)).ToString("00")}";

    }

}

