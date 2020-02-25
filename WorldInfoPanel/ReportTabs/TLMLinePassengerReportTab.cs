using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using UnityEngine;
using static Klyte.TransportLinesManager.UI.TLMReportsTab;

namespace Klyte.TransportLinesManager.UI
{

    public class TLMLinePassengerReportTab : UICustomControl, ITLMReportChild
    {

        private UIPanel m_bg;
        private TLMPassengerReportLine[] m_reportLines = new TLMPassengerReportLine[17];
        private TLMPassengerReportLine m_aggregateLine;


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
            titleLabel.localeID = "K45_TLM_PASSENGERS_LINE_REPORT";

            KlyteMonoUtils.CreateUIElement(out UIPanel listTitle, m_bg.transform, "LT");
            TLMPassengerReportLine titleList = listTitle.gameObject.AddComponent<TLMPassengerReportLine>();
            titleList.AsTitle();

            KlyteMonoUtils.CreateUIElement(out UIPanel reportLinesContainer, m_bg.transform, "listContainer", new Vector4(0, 0, m_bg.width, m_bg.height - titleLabel.height - listTitle.height - 35));
            reportLinesContainer.autoLayout = true;
            reportLinesContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            KlyteMonoUtils.CreateScrollPanel(reportLinesContainer, out UIScrollablePanel reportLines, out _, reportLinesContainer.width - 10, reportLinesContainer.height, Vector3.zero);

            for (int i = 0; i < m_reportLines.Length; i++)
            {
                KlyteMonoUtils.CreateUIElement(out UIPanel line, reportLines.transform, $"L{i}");
                m_reportLines[i] = line.gameObject.AddComponent<TLMPassengerReportLine>();
            }
            KlyteMonoUtils.CreateUIElement(out UIPanel aggregateLine, m_bg.transform, $"L_AGG");
            m_aggregateLine = aggregateLine.gameObject.AddComponent<TLMPassengerReportLine>();
        }


        public void OnEnable()
        {
        }

        public void OnDisable()
        { }

        public void UpdateBindings(bool showDayTime)
        {
            if (m_bg.isVisible)
            {
                System.Collections.Generic.List<TLMTransportLineStatusesManager.PassengerReport> report = TLMTransportLineStatusesManager.instance.GetLineStudentTouristsTotalReport(UVMPublicTransportWorldInfoPanel.GetLineID());
                var totalizer = new TLMTransportLineStatusesManager.PassengerReport();
                for (int i = 0; i < m_reportLines.Length; i++)
                {
                    m_reportLines[i].SetData(report[16 - i], showDayTime, TLMController.IsRealTimeEnabled);
                    if (i > 0)
                    {
                        totalizer.Student += report[16 - i].Student;
                        totalizer.Tourists += report[16 - i].Tourists;
                        totalizer.Total += report[16 - i].Total;
                    }
                }
                m_aggregateLine.SetDataTotalizer(totalizer);
            }
        }


        #endregion

        public bool MayBeVisible() => true;

    }
}