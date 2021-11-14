using Klyte.TransportLinesManager.Extensions;
using System.Collections.Generic;
using static Klyte.TransportLinesManager.Extensions.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.UI
{

    internal class TLMLineFinanceReportTab : BasicReportTab<TLMFinanceReportLine, IncomeExpenseReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_FINANCIAL_REPORT";
        public override bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetCurrentTSD().HasVehicles();
        protected override List<IncomeExpenseReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineFinanceReport(lineId);
        protected override void AddToTotalizer(ref IncomeExpenseReport totalizer, IncomeExpenseReport data)
        {
            totalizer.Income += data.Income;
            totalizer.Expense += data.Expense;
        }
    }
}