using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using UnityEngine;
using static Klyte.TransportLinesManager.UI.TLMReportsTab;

namespace Klyte.TransportLinesManager.UI
{

    public class TLMLineFinanceReportTab : UICustomControl, ITLMReportChild
    {

        private UIPanel m_bg;
        private TLMFinanceReportLine[] m_reportLines = new TLMFinanceReportLine[17];
        private TLMFinanceReportLine m_aggregateLine;


        #region Overridable

        public void Awake()
        {
            m_bg = component as UIPanel;
            m_bg.autoLayout = true;
            m_bg.autoLayoutDirection = LayoutDirection.Vertical;
            m_bg.clipChildren = true;

            var uiHelper = new UIHelperExtension(m_bg);

            UILabel titleLabel = uiHelper.AddLabel("");
            titleLabel.autoSize = true;
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.minimumSize = new Vector2(m_bg.width, 0);
            KlyteMonoUtils.LimitWidth(titleLabel, m_bg.width);
            titleLabel.localeID = "K45_TLM_FINANCIAL_REPORT";

            KlyteMonoUtils.CreateUIElement(out UIPanel listTitle, m_bg.transform, "LT");
            TLMFinanceReportLine titleList = listTitle.gameObject.AddComponent<TLMFinanceReportLine>();
            titleList.AsTitle();

            KlyteMonoUtils.CreateUIElement(out UIPanel reportLinesContainer, m_bg.transform, "listContainer", new Vector4(0, 0, m_bg.width, m_bg.height - titleLabel.height -  listTitle.height - 35));
            reportLinesContainer.autoLayout = true;
            reportLinesContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            KlyteMonoUtils.CreateScrollPanel(reportLinesContainer, out UIScrollablePanel reportLines, out _, reportLinesContainer.width - 10, reportLinesContainer.height, Vector3.zero);

            for (int i = 0; i < m_reportLines.Length; i++)
            {
                KlyteMonoUtils.CreateUIElement(out UIPanel line, reportLines.transform, $"L{i}");
                m_reportLines[i] = line.gameObject.AddComponent<TLMFinanceReportLine>();
            }
            KlyteMonoUtils.CreateUIElement(out UIPanel aggregateLine, m_bg.transform, $"L_AGG");
            m_aggregateLine = aggregateLine.gameObject.AddComponent<TLMFinanceReportLine>();
        }


        public void UpdateBindings(bool showDayTime)
        {
            if (m_bg.isVisible)
            {
                System.Collections.Generic.List<TLMTransportLineStatusesManager.IncomeExpenseReport> report = TLMTransportLineStatusesManager.instance.GetLineFinanceReport(GetLineID());
                var totalizer = new TLMTransportLineStatusesManager.IncomeExpenseReport();
                for (int i = 0; i < m_reportLines.Length; i++)
                {
                    m_reportLines[i].SetData(report[16 - i], showDayTime, TLMController.IsRealTimeEnabled);
                    if (i > 0)
                    {
                        totalizer.Income += report[16 - i].Income;
                        totalizer.Expense += report[16 - i].Expense;
                    }
                }
                m_aggregateLine.SetDataTotalizer(totalizer);
            }
        }

        #endregion



        internal static ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        public bool MayBeVisible() => TransportSystemDefinition.From(UVMPublicTransportWorldInfoPanel.GetLineID()).HasVehicles();

    }
}