using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMReportsTab : UICustomControl, IUVMPTWIPChild
    {
        private UIPanel m_bg;
        private UITabstrip m_reportTabstrip;
        private Dictionary<string, ITLMReportChild> m_childControls = new Dictionary<string, ITLMReportChild>();
        private bool m_showDayTime = false;

        public void Awake()
        {
            m_bg = component as UIPanel;
            m_bg.autoLayout = true;
            m_bg.autoLayoutDirection = LayoutDirection.Vertical;
            m_bg.clipChildren = true;

            var uiHelper = new UIHelperExtension(m_bg);

            float heightCheck = 0f;
            if (!TLMController.IsRealTimeEnabled)
            {
                UICheckBox m_checkChangeDateLabel = uiHelper.AddCheckboxLocale("K45_TLM_SHOW_DAYTIME_INSTEAD_DATE", false, (x) => m_showDayTime = x);
                KlyteMonoUtils.LimitWidth(m_checkChangeDateLabel.label, m_bg.width - 50);
                heightCheck = m_checkChangeDateLabel.height;
            }
            KlyteMonoUtils.CreateTabsComponent(out m_reportTabstrip, out _, m_bg.transform, "LineConfig", new Vector4(0, 0, m_bg.width, 30), new Vector4(0, 30, m_bg.width, m_bg.height - heightCheck - 30));
            m_childControls.Add("FinanceReport", TabCommons.CreateTab<TLMLineFinanceReportTab>(m_reportTabstrip, "InfoPanelIconCurrency", "K45_TLM_WIP_FINANCE_REPORT_TAB", "FinanceReport", false));
            m_childControls.Add("PassengerReport", TabCommons.CreateTab<TLMLinePassengerReportTab>(m_reportTabstrip, "InfoIconPopulation", "K45_TLM_WIP_PASSENGER_REPORT_TAB", "PassengerReport", false));
            m_childControls.Add("PassengerWealthReport", TabCommons.CreateTab<TLMLinePassengerWealthReportTab>(m_reportTabstrip, "InfoIconLandValue", "K45_TLM_WIP_PASSENGER_WEALTH_REPORT_TAB", "PassengerWealthReport", false));
        }

        public void UpdateBindings()
        {
            foreach (KeyValuePair<string, ITLMReportChild> tab in m_childControls)
            {
                if (tab.Value.MayBeVisible())
                {
                    tab.Value.UpdateBindings(m_showDayTime);
                }
            }
        }
        public void OnEnable() { }
        public void OnDisable() { }
        public void OnGotFocus() { }
        public bool MayBeVisible() => true;
        public void Hide() => m_bg.isVisible = false;
        public void OnSetTarget(Type source)
        {

        }


        public interface ITLMReportChild
        {
            void UpdateBindings(bool showDayTime);
            bool MayBeVisible();
        }
    }
}
