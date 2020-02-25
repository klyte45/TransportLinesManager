using ColossalFramework.UI;
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
        private Dictionary<string, IUVMPTWIPChild> m_childControls = new Dictionary<string, IUVMPTWIPChild>();


        public void Awake()
        {
            m_bg = component as UIPanel;
            m_bg.autoLayout = true;
            m_bg.autoLayoutDirection = LayoutDirection.Vertical;
            m_bg.clipChildren = true;
            KlyteMonoUtils.CreateTabsComponent(out m_reportTabstrip, out _, m_bg.transform, "LineConfig", new Vector4(0, 0, m_bg.width, 30), new Vector4(0, 30, m_bg.width, m_bg.height - 30));
            m_childControls.Add("FinanceReport", TabCommons.CreateTab<TLMLineFinanceReportTab>(m_reportTabstrip, "InfoPanelIconCurrency", "K45_TLM_WIP_FINANCE_REPORT_TAB", "FinanceReport", false));
        }

        public void UpdateBindings()
        {
            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_childControls)
            {
                if (tab.Value.MayBeVisible())
                {
                    tab.Value.UpdateBindings();
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
    }
}
