using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    public class TLMLineFinanceReportTab : UICustomControl, IUVMPTWIPChild
    {

        private UIPanel m_bg;
        private TLMFinanceReportLine[] m_reportLines = new TLMFinanceReportLine[17];
        private bool showDayTime = false;


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

            UICheckBox checkChangeDateLabel = uiHelper.AddCheckboxLocale("K45_TLM_SHOW_DAYTIME_INSTEAD_DATE", false, (x) => showDayTime = x);
            KlyteMonoUtils.LimitWidth(checkChangeDateLabel.label, m_bg.width - 50);

            KlyteMonoUtils.CreateUIElement(out UIPanel listTitle, m_bg.transform, "LT");
            TLMFinanceReportLine titleList = listTitle.gameObject.AddComponent<TLMFinanceReportLine>();
            titleList.AsTitle();

            KlyteMonoUtils.CreateUIElement(out UIPanel reportLinesContainer, m_bg.transform, "listContainer", new Vector4(0, 0, m_bg.width, m_bg.height - titleLabel.height - checkChangeDateLabel.height - listTitle.height - 10));
            reportLinesContainer.autoLayout = true;
            reportLinesContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            KlyteMonoUtils.CreateScrollPanel(reportLinesContainer, out UIScrollablePanel reportLines, out _, reportLinesContainer.width - 10, reportLinesContainer.height, Vector3.zero);

            for (int i = 0; i < m_reportLines.Length; i++)
            {
                KlyteMonoUtils.CreateUIElement(out UIPanel line, reportLines.transform, $"L{i}");
                m_reportLines[i] = line.gameObject.AddComponent<TLMFinanceReportLine>();
            }

        }


        public void OnEnable()
        {
        }

        public void OnDisable()
        { }

        public void UpdateBindings()
        {
            if (m_bg.isVisible)
            {
                System.Collections.Generic.List<TLMTransportLineStatusesManager.IncomeExpense> report = TLMTransportLineStatusesManager.instance.GetLineReport(GetLineID());
                for (int i = 0; i < m_reportLines.Length; i++)
                {
                    m_reportLines[i].SetData(report[16 - i], showDayTime);
                }
            }
        }
        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }

            ushort lineID = GetLineID();
            if (lineID != 0)
            {
            }

        }

        #endregion


        public void OnGotFocus()
        {
        }

        internal static ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        public static string GetVehicleTypeIcon(ushort lineId) => TransportSystemDefinition.From(lineId).GetCircleSpriteName().ToString();



    }
}