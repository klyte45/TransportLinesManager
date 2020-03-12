using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using UnityEngine;
using static Klyte.TransportLinesManager.Extensors.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMFinanceReportLine : BaseReportLine<IncomeExpenseReport>
    {
        private UILabel m_income;
        private UILabel m_expense;
        private UILabel m_balance;

        private Color m_profitColor = new Color32(0, 0x88, 0, 0xff);
        private Color m_lossColor = new Color32(0xaa, 0, 0, 0xff);

        protected override void AsTitleInternal()
        {
            m_income.localeID = "K45_TLM_FINANCE_REPORT_TITLE_INCOME";
            m_expense.localeID = "K45_TLM_FINANCE_REPORT_TITLE_EXPENSE";
            m_balance.localeID = "K45_TLM_FINANCE_REPORT_TITLE_BALANCE";

            m_income.textAlignment = UIHorizontalAlignment.Center;
            m_expense.textAlignment = UIHorizontalAlignment.Center;
            m_balance.textAlignment = UIHorizontalAlignment.Center;
            m_balance.backgroundSprite = null;
        }
        protected override void SetDataInternal(IncomeExpenseReport data)
        {
            m_income.text = (data.Income / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_expense.text = (data.Expense / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_balance.text = ((data.Income - data.Expense) / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            m_balance.color = data.Income < data.Expense ? m_lossColor : m_profitColor;
        }
        protected override void AddColumns(ref float xAdvance)
        {
            xAdvance += InitField(out m_income, "Income", "K45_TLM_FINANCE_REPORT_TITLE_INCOME", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
            xAdvance += InitField(out m_expense, "Expense", "K45_TLM_FINANCE_REPORT_TITLE_EXPENSE", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
            xAdvance += InitField(out m_balance, "Balance", "K45_TLM_FINANCE_REPORT_TITLE_BALANCE", xAdvance, TOTAL_DATA_COLUMNS_SPACE / 3);
            m_balance.backgroundSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(LineIconSpriteNames.K45_SquareIcon);
        }
    }

}

