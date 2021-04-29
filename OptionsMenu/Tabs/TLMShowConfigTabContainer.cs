using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMShowConfigTabContainer : UICustomControl, ITLMConfigOptionsTab
    {
        private UIScrollablePanel parent;
        private UITabContainer tabContainer;
        private void Awake()
        {
            parent = GetComponentInChildren<UIScrollablePanel>();

            int parentWidth = 730;

            KlyteMonoUtils.CreateUIElement(out UITabstrip strip, parent.transform, "TLMTabstrip", new Vector4(5, 0, parentWidth - 10, 40));
            KlyteMonoUtils.CreateUIElement(out tabContainer, parent.transform, "TLMTabContainer", new Vector4(0, 40, parentWidth - 10, 660));

            strip.tabPages = tabContainer;

            UIButton tabTemplate = CreateTabSubStripTemplate();

            UIComponent bodyContent = CreateContentTemplate(parentWidth, 660, false);

            foreach (var kv in ReflectionUtils.GetSubtypesRecursive(typeof(TLMShowConfigTab), GetType()))
            {
                var tsd = (kv.GetConstructor(new Type[0]).Invoke(null) as TLMShowConfigTab).TSD;


                GameObject tab = Instantiate(tabTemplate.gameObject);
                GameObject body = Instantiate(bodyContent.gameObject);
                string name = tsd.GetTransportName();
                string bgIcon = KlyteResourceLoader.GetDefaultSpriteNameFor(tsd.DefaultIcon, true);
                string fgIcon = tsd.GetTransportTypeIcon();
                UIButton tabButton = tab.GetComponent<UIButton>();
                tabButton.tooltip = name;
                tabButton.hoveredBgSprite = bgIcon;
                tabButton.focusedBgSprite = bgIcon;
                tabButton.normalBgSprite = bgIcon;
                tabButton.disabledBgSprite = bgIcon;
                tabButton.focusedColor = Color.green;
                tabButton.hoveredColor = new Color(0, 0.5f, 0f);
                tabButton.color = Color.white;
                tabButton.disabledColor = Color.gray;
                if (!string.IsNullOrEmpty(fgIcon))
                {
                    KlyteMonoUtils.CreateUIElement(out UIButton secSprite, tabButton.transform, "OverSprite", new Vector4(5, 5, 30, 30));
                    secSprite.normalFgSprite = fgIcon;
                    secSprite.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                    secSprite.isInteractive = false;
                    secSprite.disabledColor = Color.black;
                }
                strip.AddTab(name, tab, body, new Type[] { kv });
            }
            strip.selectedIndex = -1;
            strip.selectedIndex = 0;
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


        private static UIComponent CreateContentTemplate(float width, float height, bool scrollable)
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel contentContainer, null);
            contentContainer.name = "Container";
            contentContainer.size = new Vector3(width, height);
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

        public void ReloadData()
        {
            foreach (var tab in tabContainer.GetComponentsInChildren<ITLMConfigOptionsTab>())
            {
                try
                {
                    tab.ReloadData();
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"Error reloading data for {tab}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}
