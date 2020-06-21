using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow
{

    public class TLMPanel : BasicKPanel<TransportLinesManagerMod, TLMController, TLMPanel>
    {
        private UILabel m_directionLabel;

        private UITabstrip m_stripMain;

        public override float PanelWidth => 875;
        public override float PanelHeight => component.parent.height;

        #region Awake
        protected override void AwakeActions()
        {

            KlyteMonoUtils.CreateUIElement(out m_stripMain, MainPanel.transform, "TLMTabstrip", new Vector4(5, 45, MainPanel.width - 10, 40));

            KlyteMonoUtils.CreateUIElement(out UITabContainer tabContainer, MainPanel.transform, "TLMTabContainer", new Vector4(0, 85, MainPanel.width, MainPanel.height - 85));
            m_stripMain.tabPages = tabContainer;

            CreateTsdTabstrip();

            m_stripMain.selectedIndex = 0;
            m_stripMain.selectedIndex = -1;
            m_stripMain.eventSelectedIndexChanged += SetViewMode;
            m_stripMain.eventVisibilityChanged += OnOpenClosePanel;
        }

        private void OnOpenClosePanel(UIComponent component, bool value)
        {
            if (value)
            {
                TransportLinesManagerMod.Instance.ShowVersionInfoPopup();
                SetViewMode(null, m_stripMain.selectedIndex);
            }
        }

        private void SetViewMode(UIComponent component, int value)
        {
            //if (!GetComponent<UIComponent>().isVisible)
            //{
            return;
            //}
            //switch ((UiCategoryTab)value)
            //{
            //    case UiCategoryTab.LineListing:
            //    case UiCategoryTab.DepotListing:
            //        InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Transport, InfoManager.SubInfoMode.Default);
            //        return;
            //    case UiCategoryTab.TourListing:
            //        InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Tours, InfoManager.SubInfoMode.Default);
            //        return;
            //    default:
            //        InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            //        return;
            //}
        }

        internal void OpenAt(TransportSystemDefinition tsd)
        {
            m_stripMain.selectedIndex = m_stripMain.Find<UIComponent>(tsd.GetDefType().Name)?.zOrder ?? -1;
            TLMController.instance.OpenTLMPanel();
        }

        private void CreateTsdTabstrip()
        {


            UIButton tabTemplate = CreateTabSubStripTemplate();

            UIComponent bodyContent = CreateContentTemplate(m_stripMain.tabContainer.width, m_stripMain.tabContainer.height, false);

            foreach (KeyValuePair<TransportSystemDefinition, Func<ITLMSysDef>> kv in TransportSystemDefinition.SysDefinitions)
            {
                Type[] components;
                Type targetType;
                try
                {
                    targetType = ReflectionUtils.GetImplementationForGenericType(typeof(UVMLinesPanel<>), kv.Value().GetType());
                    components = new Type[] { targetType };
                }
                catch
                {
                    continue;
                }
                TransportSystemDefinition tsd = kv.Key;
                GameObject tab = Instantiate(tabTemplate.gameObject);
                GameObject body = Instantiate(bodyContent.gameObject);
                var configIdx = kv.Key.ToConfigIndex();
                string name = kv.Value().GetType().Name;
                TLMUtils.doLog($"configIdx = {configIdx};kv.Key = {kv.Key}; kv.Value= {kv.Value} ");
                string bgIcon = KlyteResourceLoader.GetDefaultSpriteNameFor(TLMUtils.GetLineIcon(0, configIdx, ref tsd), true);
                string fgIcon = kv.Key.GetTransportTypeIcon();
                UIButton tabButton = tab.GetComponent<UIButton>();
                tabButton.tooltip = TLMConfigWarehouse.getNameForTransportType(configIdx);
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
                m_stripMain.AddTab(name, tab, body, components);
            }
        }

        private static UIButton CreateTabSubStripTemplate()
        {
            KlyteMonoUtils.CreateUIElement(out UIButton tabTemplate, null, "TLMTabTemplate");
            KlyteMonoUtils.InitButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }

        private void CreateTitleBar()
        {
            KlyteMonoUtils.CreateUIElement(out UILabel titlebar, MainPanel.transform, "TLMListPanel", new Vector4(75, 10, MainPanel.width - 150, 20));
            titlebar.autoSize = false;
            titlebar.text = "Transport Lines Manager v" + TransportLinesManagerMod.Version;
            titlebar.textAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out UIButton closeButton, MainPanel.transform, "CloseButton", new Vector4(MainPanel.width - 37, 5, 32, 32));
            KlyteMonoUtils.InitButton(closeButton, false, "buttonclose", true);
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.eventClick += (x, y) =>
            {
                TLMController.instance.CloseTLMPanel();
            };

            KlyteMonoUtils.CreateUIElement(out UISprite logo, MainPanel.transform, "TLMLogo", new Vector4(22, 5f, 32, 32));
            logo.spriteName = TransportLinesManagerMod.Instance.IconName;
        }

        private static UIComponent CreateContentTemplate(float width, float height, bool scrollable)
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel contentContainer, null);
            contentContainer.name = "Container";
            contentContainer.size = new Vector4(width, height);
            if (scrollable)
            {
                KlyteMonoUtils.CreateUIElement(out UIScrollablePanel scrollPanel, contentContainer.transform, "ScrollPanel");
                scrollPanel.width = contentContainer.width - 20f;
                scrollPanel.height = contentContainer.height;
                scrollPanel.autoLayoutDirection = LayoutDirection.Vertical;
                scrollPanel.autoLayoutStart = LayoutStart.TopLeft;
                scrollPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
                scrollPanel.autoLayout = true;
                scrollPanel.clipChildren = true;
                scrollPanel.relativePosition = new Vector3(5, 0);

                KlyteMonoUtils.CreateUIElement(out UIPanel trackballPanel, contentContainer.transform, "Trackball");
                trackballPanel.width = 10f;
                trackballPanel.height = scrollPanel.height;
                trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
                trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
                trackballPanel.autoLayout = true;
                trackballPanel.relativePosition = new Vector3(contentContainer.width - 15, 0);

                KlyteMonoUtils.CreateUIElement(out UIScrollbar scrollBar, trackballPanel.transform, "Scrollbar");
                scrollBar.width = 10f;
                scrollBar.height = scrollBar.parent.height;
                scrollBar.orientation = UIOrientation.Vertical;
                scrollBar.pivot = UIPivotPoint.BottomLeft;
                scrollBar.AlignTo(trackballPanel, UIAlignAnchor.TopRight);
                scrollBar.minValue = 0f;
                scrollBar.value = 0f;
                scrollBar.incrementAmount = 25f;

                KlyteMonoUtils.CreateUIElement(out UISlicedSprite scrollBg, scrollBar.transform, "ScrollbarBg");
                scrollBg.relativePosition = Vector2.zero;
                scrollBg.autoSize = true;
                scrollBg.size = scrollBg.parent.size;
                scrollBg.fillDirection = UIFillDirection.Vertical;
                scrollBg.spriteName = "ScrollbarTrack";
                scrollBar.trackObject = scrollBg;

                KlyteMonoUtils.CreateUIElement(out UISlicedSprite scrollFg, scrollBg.transform, "ScrollbarFg");
                scrollFg.relativePosition = Vector2.zero;
                scrollFg.fillDirection = UIFillDirection.Vertical;
                scrollFg.autoSize = true;
                scrollFg.width = scrollFg.parent.width - 4f;
                scrollFg.spriteName = "ScrollbarThumb";
                scrollBar.thumbObject = scrollFg;
                scrollPanel.verticalScrollbar = scrollBar;
                scrollPanel.eventMouseWheel += delegate (UIComponent component, UIMouseEventParameter param)
                {
                    scrollPanel.scrollPosition += new Vector2(0f, Mathf.Sign(param.wheelDelta) * -1f * scrollBar.incrementAmount);
                    param.Use();
                };
            }
            return contentContainer;
        }
        #endregion
    }
}
