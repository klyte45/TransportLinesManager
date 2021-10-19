using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow
{

    public class TLMPanel : BasicKPanel<TransportLinesManagerMod, TLMController, TLMPanel>
    {
        private UITabstrip m_stripMain;

        public override float PanelWidth => 875;
        public override float PanelHeight => component.parent.height;

        internal UVMLinesPanel m_linesPanel;

        #region Awake
        protected override void AwakeActions()
        {

            KlyteMonoUtils.CreateUIElement(out m_stripMain, MainPanel.transform, "TLMTabstrip", new Vector4(5, 45, MainPanel.width - 10, 40));
            CreateTsdTabstrip();
            m_stripMain.eventVisibilityChanged += OnOpenClosePanel;
            m_stripMain.selectedIndex = 0;

            KlyteMonoUtils.CreateUIElement(out UIPanel bodyContent, MainPanel.transform, "Content", new Vector4(0, 85, MainPanel.width, MainPanel.height - 85));
            m_linesPanel = bodyContent.gameObject.AddComponent<UVMLinesPanel>();
            m_stripMain.eventSelectedIndexChanged += (x, y) =>
            {
                if (y >= 0)
                {
                    m_linesPanel.TSD = m_stripMain.tabs[y]?.objectUserData as TransportSystemDefinition;
                }
            };
        }

        private void OnOpenClosePanel(UIComponent component, bool value)
        {
            if (value)
            {
                TransportLinesManagerMod.Instance.ShowVersionInfoPopup();
            }
        }

        internal void OpenAt(TransportSystemDefinition tsd)
        {
            TLMController.Instance.OpenTLMPanel();
            m_stripMain.selectedIndex = m_stripMain.Find<UIComponent>(tsd.GetTransportName())?.zOrder ?? -1;
        }

        private void CreateTsdTabstrip()
        {
            foreach (var tsd in TransportSystemDefinition.TransportInfoDict.Keys.Where(x => x.HasLines))
            {
                string name = tsd.GetTransportName();
                UIButton tabButton = m_stripMain.AddTab();
                UpdateTabSubStripTemplate(tabButton);
                string bgIcon = KlyteResourceLoader.GetDefaultSpriteNameFor(TLMPrefixesUtils.GetLineIcon(0, tsd), true);
                string fgIcon = tsd.GetTransportTypeIcon();
                tabButton.tooltip = name;
                tabButton.name = tsd.ToString();
                tabButton.hoveredBgSprite = bgIcon;
                tabButton.focusedBgSprite = bgIcon;
                tabButton.normalBgSprite = bgIcon;
                tabButton.disabledBgSprite = bgIcon;
                tabButton.focusedColor = Color.green;
                tabButton.hoveredColor = new Color(0, 0.5f, 0f);
                tabButton.color = Color.black;
                tabButton.disabledColor = Color.gray;
                if (!string.IsNullOrEmpty(fgIcon))
                {
                    KlyteMonoUtils.CreateUIElement(out UIButton secSprite, tabButton.transform, "OverSprite", new Vector4(5, 5, 30, 30));
                    secSprite.normalFgSprite = fgIcon;
                    secSprite.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                    secSprite.isInteractive = false;
                    secSprite.disabledColor = Color.black;
                }
                tabButton.objectUserData = tsd;
            }
        }

        private static UIButton UpdateTabSubStripTemplate(UIButton tabTemplate)
        {
            KlyteMonoUtils.InitButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }

        #endregion
    }
}
