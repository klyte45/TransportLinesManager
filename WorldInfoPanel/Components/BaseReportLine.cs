using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal abstract class BaseReportLine<T> : MonoBehaviour where T : TLMTransportLineStatusesManager.BasicReportData
    {
        private static readonly Color32 BackgroundColor = new Color32(49, 52, 58, 255);
        private UIPanel m_container;
        private UILabel m_dateTime;

        private UIPanel m_background;

        private const int TABLE_LINE_SAFE_ZONE_WIDTH = 368;
        private const int DATE_COLUMN_WIDTH = 80;
        protected const int TOTAL_DATA_COLUMNS_SPACE = TABLE_LINE_SAFE_ZONE_WIDTH - DATE_COLUMN_WIDTH;

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
            backgroundColor.a = (byte)((m_container.zOrder % 2 != 0) ? 127 : 255);
            m_background.color = backgroundColor;
            m_background.relativePosition = Vector3.zero;
            m_background.size = m_container.size;

            float xAdvance = 0;

            KlyteMonoUtils.CreateUIElement(out m_dateTime, m_container.transform, "DateTime");
            m_dateTime.autoSize = true;
            m_dateTime.minimumSize = new Vector2(DATE_COLUMN_WIDTH, 30);
            m_dateTime.textScale = 0.7f;
            m_dateTime.textAlignment = UIHorizontalAlignment.Center;
            m_dateTime.relativePosition = new Vector3(0, 0);
            m_dateTime.verticalAlignment = UIVerticalAlignment.Middle;
            m_dateTime.padding = new RectOffset(3, 3, 5, 3);
            m_dateTime.text = "00:00\n25/12/2020";
            KlyteMonoUtils.LimitWidthAndBox(m_dateTime);
            xAdvance += m_dateTime.minimumSize.x;
            AddColumns(ref xAdvance);

        }

        protected abstract void AddColumns(ref float xAdvance);

        protected float InitField(out UILabel label, string name, string tooltipLocale, float xAdvance, float columnWidth)
        {
            KlyteMonoUtils.CreateUIElement(out label, m_container.transform, name);
            label.autoSize = false;
            label.minimumSize = new Vector2(columnWidth, 24);
            label.textScale = 1f;
            label.relativePosition = new Vector3(xAdvance, 3);
            label.textAlignment = UIHorizontalAlignment.Right;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.padding = new RectOffset(3, 3, 5, 3);
            label.isTooltipLocalized = true;
            label.tooltipLocaleID = tooltipLocale;
            KlyteMonoUtils.LimitWidthAndBox(label);
            return label.minimumSize.x;
        }

        public void SetData(T data, bool showDaytime, bool realtimeEnabled)
        {
            if (showDaytime && !realtimeEnabled)
            {
                m_dateTime.text = $"{FloatToHour(data.StartDayTime)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : FloatToHour(data.EndDayTime))}";
            }
            else
            {
                m_dateTime.text = $"{data.StartDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : data.EndDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo))}";
            }
            SetDataInternal(data);
        }

        protected abstract void SetDataInternal(T data);

        public void SetDataTotalizer(T data)
        {
            m_dateTime.text = Locale.Get("K45_TLM_BUDGET_REPORT_TOTALIZER");
            SetDataInternal(data);
            m_background.color = new Color32(15, 20, 30, 255);
        }

        public void AsTitle()
        {
            m_dateTime.localeID = "K45_TLM_FINANCE_REPORT_TITLE_DATE";
            m_dateTime.textAlignment = UIHorizontalAlignment.Center;

            AsTitleInternal();

            Destroy(m_background.gameObject);
        }
        protected abstract void AsTitleInternal();

        public string FloatToHour(float time) => $"{((int)time).ToString("00")}:{((int)(time % 1 * 60)).ToString("00")}";

    }

}

