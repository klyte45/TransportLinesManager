using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMFinanceReportLine : MonoBehaviour
    {
        private static readonly Color32 BackgroundColor = new Color32(49, 52, 58, 255);
        private UIPanel m_container;
        private UILabel m_dateTime;
        private UILabel m_income;
        private UILabel m_expense;
        private UILabel m_balance;
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

            KlyteMonoUtils.CreateUIElement(out m_income, m_container.transform, "Income");
            m_income.autoSize = false;
            m_income.minimumSize = new Vector2(80, 30);
            m_income.textScale = 1f;
            m_income.relativePosition = new Vector3(92, 0);
            m_income.textAlignment = UIHorizontalAlignment.Right;
            m_income.verticalAlignment = UIVerticalAlignment.Middle;
            m_income.padding = new RectOffset(3, 3, 5, 3);
            m_income.text = 100000f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_income);


            KlyteMonoUtils.CreateUIElement(out m_expense, m_container.transform, "Expense");
            m_expense.autoSize = false;
            m_expense.minimumSize = new Vector2(80, 30);
            m_expense.textScale = 1f;
            m_expense.relativePosition = new Vector3(184, 0);
            m_expense.textAlignment = UIHorizontalAlignment.Right;
            m_expense.verticalAlignment = UIVerticalAlignment.Middle;
            m_expense.padding = new RectOffset(3, 3, 5, 3);
            m_expense.text = 100000f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_expense);

            KlyteMonoUtils.CreateUIElement(out m_balance, m_container.transform, "Balance");
            m_balance.autoSize = false;
            m_balance.minimumSize = new Vector2(80, 24);
            m_balance.textScale = 1f;
            m_balance.relativePosition = new Vector3(270, 3);
            m_balance.textAlignment = UIHorizontalAlignment.Right;
            m_balance.verticalAlignment = UIVerticalAlignment.Middle;
            m_balance.backgroundSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(LineIconSpriteNames.K45_SquareIcon);
            m_balance.padding = new RectOffset(3, 3, 5, 3);
            m_balance.color = new Color32(0xff, 0, 0, 0xff);
            m_balance.text = 10f.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            KlyteMonoUtils.LimitWidthAndBox(m_balance);
        }

        private Color m_profitColor = new Color32(0, 0x88, 0, 0xff);
        private Color m_lossColor = new Color32(0xaa, 0, 0, 0xff);

        public void SetData(TLMTransportLineStatusesManager.IncomeExpense data, bool showDaytime, bool realtimeEnabled)
        {
            if (showDaytime && !realtimeEnabled)
            {
                m_dateTime.text = $"{FloatToHour(data.StartDayTime)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : FloatToHour(data.EndDayTime))}";
            }
            else
            {
                m_dateTime.text = $"{data.StartDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo)}\n{(m_container.zOrder == 0 ? Locale.Get("K45_TLM_BUDGET_REPORT_LIST_CURRENT_TIME") : data.EndDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo))}";
            }
            m_income.text = (data.Income / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_expense.text = (data.Expense / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_balance.text = ((data.Income - data.Expense) / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_balance.color = data.Income < data.Expense ? m_lossColor : m_profitColor;
        }

        public void SetDataTotalizer(TLMTransportLineStatusesManager.IncomeExpense data)
        {
            m_dateTime.text = Locale.Get("K45_TLM_BUDGET_REPORT_TOTALIZER");
            m_income.text = (data.Income / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_expense.text = (data.Expense / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_balance.text = ((data.Income - data.Expense) / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_balance.color = data.Income < data.Expense ? m_lossColor : m_profitColor;
            m_background.color = new Color32(15, 20, 30, 255);
        }

        public void AsTitle()
        {
            m_dateTime.localeID = "K45_TLM_FINANCE_REPORT_TITLE_DATE";
            m_income.localeID = "K45_TLM_FINANCE_REPORT_TITLE_INCOME";
            m_expense.localeID = "K45_TLM_FINANCE_REPORT_TITLE_EXPENSE";
            m_balance.localeID = "K45_TLM_FINANCE_REPORT_TITLE_BALANCE";

            m_dateTime.textAlignment = UIHorizontalAlignment.Center;
            m_income.textAlignment = UIHorizontalAlignment.Center;
            m_expense.textAlignment = UIHorizontalAlignment.Center;
            m_balance.textAlignment = UIHorizontalAlignment.Center;

            m_balance.backgroundSprite = null;

            Destroy(m_background.gameObject);
        }

        public string FloatToHour(float time) => $"{((int) time).ToString("00")}:{((int) (time % 1 * 60)).ToString("00")}";

    }

}

