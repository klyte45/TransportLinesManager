using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.TextureAtlas;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.TextureAtlas;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow
{

    public class TLMPublicTransportManagementPanel : UICustomControl
    {
        private const int NUM_SERVICES = 0;
        private static TLMPublicTransportManagementPanel m_instance;
        private UIPanel controlContainer;

        public static TLMPublicTransportManagementPanel instance => m_instance;
        public UIPanel mainPanel { get; private set; }

        private UILabel m_directionLabel;

        private UITabstrip m_StripMain;
        private Dictionary<UiCategoryTab, UITabstrip> m_StripsSubcategories = new Dictionary<UiCategoryTab, UITabstrip>();

        #region Awake
        private void Awake()
        {
            m_instance = this;
            controlContainer = GetComponent<UIPanel>();
            controlContainer.area = new Vector4(0, 0, 0, 0);
            controlContainer.isVisible = false;
            controlContainer.name = "TLMPanel";

            TLMUtils.createUIElement(out UIPanel _mainPanel, GetComponent<UIPanel>().transform, "TLMListPanel", new Vector4(0, 0, 885, controlContainer.parent.height));
            mainPanel = _mainPanel;
            mainPanel.backgroundSprite = "MenuPanel2";

            CreateTitleBar();

            TLMUtils.createUIElement(out m_StripMain, mainPanel.transform, "TLMTabstrip", new Vector4(5, 40, mainPanel.width - 10, 40));

            TLMUtils.createUIElement(out UITabContainer tabContainer, mainPanel.transform, "TLMTabContainer", new Vector4(0, 80, mainPanel.width, mainPanel.height - 80));
            m_StripMain.tabPages = tabContainer;

            foreach (UiCategoryTab tab in Enum.GetValues(typeof(UiCategoryTab)))
            {
                UIButton superTab = CreateTabTemplate();
                superTab.normalFgSprite = tab.getTabFgSprite();
                superTab.tooltip = tab.getTabName();

                TLMUtils.createUIElement(out UIPanel content, null);
                content.name = "Container";
                content.area = new Vector4(0, 40, mainPanel.width, mainPanel.height - 80);

                m_StripMain.AddTab("TLMTab" + tab, superTab.gameObject, content.gameObject);
                CreateTsdTabstrip(out UITabstrip subStrip, content, tab);
                m_StripsSubcategories[tab] = subStrip;
                if (m_StripsSubcategories[tab].tabCount > 0)
                {
                    m_StripsSubcategories[tab].selectedIndex = 0;
                    m_StripsSubcategories[tab].selectedIndex = -1;
                }
                else
                {
                    m_StripsSubcategories[tab].enabled = false;
                }
            }
            m_StripMain.selectedIndex = 0;
            m_StripMain.selectedIndex = -1;
            m_StripMain.eventSelectedIndexChanged += SetViewMode;
            m_StripMain.eventVisibilityChanged += OnOpenClosePanel;
        }

        private void OnOpenClosePanel(UIComponent component, bool value)
        {
            if (value)
            {
                TransportLinesManagerMod.instance.showVersionInfoPopup();
                SetViewMode(null, m_StripMain.selectedIndex);
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

        internal void OpenAt(UiCategoryTab cat, TransportSystemDefinition tsd)
        {
            if (cat == UiCategoryTab.LineListing && tsd.isTour())
            {
                cat = UiCategoryTab.TourListing;
            }
            if (tsd != null)
            {
                m_StripsSubcategories[cat].selectedIndex = m_StripsSubcategories[cat].Find<UIComponent>(tsd.GetDefType().Name)?.zOrder ?? -1;
            }
            TLMUtils.doLog($"OpenAt: {cat} ({(int)cat})=>{tsd.GetDefType()}");
            m_StripMain.selectedIndex = (int)cat;
            TLMController.instance.OpenTLMPanel();
        }

        private void CreateTsdTabstrip(out UITabstrip strip, UIComponent parent, UiCategoryTab category)
        {
            TLMUtils.createUIElement(out strip, parent.transform, "TLMTabstrip", new Vector4(5, 0, parent.width - 10, 40));
            var effectiveOffsetY = strip.height;

            TLMUtils.createUIElement(out UITabContainer tabContainer, parent.transform, "TLMTabContainer", new Vector4(0, 40, parent.width, parent.height - 40));
            strip.tabPages = tabContainer;

            UIButton tabTemplate = CreateTabSubStripTemplate();

            UIComponent bodyContent = CreateContentTemplate(parent.width, parent.height - effectiveOffsetY - 10, category.isScrollable());

            foreach (var kv in TransportSystemDefinition.sysDefinitions)
            {
                Type[] components;
                Type targetType;
                try
                {
                    targetType = KlyteUtils.GetImplementationForGenericType(category.getTabGenericContentImpl(), kv.Value);
                    components = new Type[] { targetType };
                }
                catch
                {
                    continue;
                }
                var tsd = kv.Key;
                GameObject tab = Instantiate(tabTemplate.gameObject);
                GameObject body = Instantiate(bodyContent.gameObject);
                var configIdx = kv.Key.toConfigIndex();
                String name = kv.Value.Name;
                TLMUtils.doLog($"configIdx = {configIdx};kv.Key = {kv.Key}; kv.Value= {kv.Value} ");
                String bgIcon = TLMUtils.GetLineIcon(0, configIdx, ref tsd).getImageName();
                String fgIcon = kv.Key.getTransportTypeIcon();
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
                    TLMUtils.createUIElement(out UIButton secSprite, tabButton.transform, "OverSprite", new Vector4(5, 5, 30, 30));
                    secSprite.normalFgSprite = fgIcon;
                    secSprite.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                    secSprite.isInteractive = false;
                    secSprite.disabledColor = Color.black;
                }
                strip.AddTab(name, tab, body, components);
            }
        }



        private static UIButton CreateTabTemplate()
        {
            TLMUtils.createUIElement(out UIButton tabTemplate, null, "TLMTabTemplate");
            TLMUtils.initButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }

        private static UIButton CreateTabSubStripTemplate()
        {
            TLMUtils.createUIElement(out UIButton tabTemplate, null, "TLMTabTemplate");
            TLMUtils.initButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            tabTemplate.atlas = LineUtilsTextureAtlas.instance.atlas;
            return tabTemplate;
        }

        private void CreateTitleRowBuilding(ref UIPanel titleLine, UIComponent parent)
        {
            TLMUtils.createUIElement(out titleLine, parent.transform, "TLMtitleline", new Vector4(5, 80, parent.width - 10, 40));

            TLMUtils.createUIElement(out UILabel districtNameLabel, titleLine.transform, "districtNameLabel");
            districtNameLabel.autoSize = false;
            districtNameLabel.area = new Vector4(0, 10, 175, 18);
            districtNameLabel.textAlignment = UIHorizontalAlignment.Center;
            districtNameLabel.text = Locale.Get("TUTORIAL_ADVISER_TITLE", "District");

            TLMUtils.createUIElement(out UILabel buildingNameLabel, titleLine.transform, "buildingNameLabel");
            buildingNameLabel.autoSize = false;
            buildingNameLabel.area = new Vector4(200, 10, 198, 18);
            buildingNameLabel.textAlignment = UIHorizontalAlignment.Center;
            buildingNameLabel.text = Locale.Get("K45_TLM_BUILDING_NAME_LABEL");

            TLMUtils.createUIElement(out UILabel vehicleCapacityLabel, titleLine.transform, "vehicleCapacityLabel");
            vehicleCapacityLabel.autoSize = false;
            vehicleCapacityLabel.area = new Vector4(400, 10, 200, 18);
            vehicleCapacityLabel.textAlignment = UIHorizontalAlignment.Center;
            vehicleCapacityLabel.text = Locale.Get("K45_TLM_VEHICLE_CAPACITY_LABEL");

            TLMUtils.createUIElement(out m_directionLabel, titleLine.transform, "directionLabel");
            m_directionLabel.autoSize = false;
            m_directionLabel.area = new Vector4(600, 10, 200, 18);
            m_directionLabel.textAlignment = UIHorizontalAlignment.Center;
            m_directionLabel.text = Locale.Get("K45_TLM_DIRECTION_LABEL");

        }

        private void CreateTitleBar()
        {
            TLMUtils.createUIElement(out UILabel titlebar, mainPanel.transform, "TLMListPanel", new Vector4(75, 10, mainPanel.width - 150, 20));
            titlebar.autoSize = false;
            titlebar.text = "Transport Lines Manager v" + TransportLinesManagerMod.version;
            titlebar.textAlignment = UIHorizontalAlignment.Center;

            TLMUtils.createUIElement(out UIButton closeButton, mainPanel.transform, "CloseButton", new Vector4(mainPanel.width - 37, 5, 32, 32));
            TLMUtils.initButton(closeButton, false, "buttonclose", true);
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.eventClick += (x, y) =>
            {
                TLMController.instance.CloseTLMPanel();
            };

            TLMUtils.createUIElement(out UISprite logo, mainPanel.transform, "TLMLogo", new Vector4(22, 5f, 32, 32));
            logo.atlas = TLMCommonTextureAtlas.instance.atlas;
            logo.spriteName = "TransportLinesManagerIconHovered";
        }

        private static UIComponent CreateContentTemplate(float width, float height, bool scrollable)
        {
            TLMUtils.createUIElement(out UIPanel contentContainer, null);
            contentContainer.name = "Container";
            contentContainer.area = new Vector4(0, 0, width, height);
            if (scrollable)
            {
                TLMUtils.createUIElement(out UIScrollablePanel scrollPanel, contentContainer.transform, "ScrollPanel");
                scrollPanel.width = contentContainer.width - 20f;
                scrollPanel.height = contentContainer.height;
                scrollPanel.autoLayoutDirection = LayoutDirection.Vertical;
                scrollPanel.autoLayoutStart = LayoutStart.TopLeft;
                scrollPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
                scrollPanel.autoLayout = true;
                scrollPanel.clipChildren = true;
                scrollPanel.relativePosition = new Vector3(5, 0);

                TLMUtils.createUIElement(out UIPanel trackballPanel, contentContainer.transform, "Trackball");
                trackballPanel.width = 10f;
                trackballPanel.height = scrollPanel.height;
                trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
                trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
                trackballPanel.autoLayout = true;
                trackballPanel.relativePosition = new Vector3(contentContainer.width - 15, 0);

                TLMUtils.createUIElement(out UIScrollbar scrollBar, trackballPanel.transform, "Scrollbar");
                scrollBar.width = 10f;
                scrollBar.height = scrollBar.parent.height;
                scrollBar.orientation = UIOrientation.Vertical;
                scrollBar.pivot = UIPivotPoint.BottomLeft;
                scrollBar.AlignTo(trackballPanel, UIAlignAnchor.TopRight);
                scrollBar.minValue = 0f;
                scrollBar.value = 0f;
                scrollBar.incrementAmount = 25f;

                TLMUtils.createUIElement(out UISlicedSprite scrollBg, scrollBar.transform, "ScrollbarBg");
                scrollBg.relativePosition = Vector2.zero;
                scrollBg.autoSize = true;
                scrollBg.size = scrollBg.parent.size;
                scrollBg.fillDirection = UIFillDirection.Vertical;
                scrollBg.spriteName = "ScrollbarTrack";
                scrollBar.trackObject = scrollBg;

                TLMUtils.createUIElement(out UISlicedSprite scrollFg, scrollBg.transform, "ScrollbarFg");
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


        private void SetActiveTab(int idx)
        {
            m_StripMain.selectedIndex = idx;
        }

        private void Update()
        {
        }
    }
    internal enum UiCategoryTab
    {
        LineListing = 0,
        TourListing = 1,
        DepotListing = 2,
        PrefixEditor = 3
    }
    internal static class TabsExtension
    {
        public static string getTabName(this UiCategoryTab tab)
        {
            switch (tab)
            {
                case UiCategoryTab.LineListing: return Locale.Get("K45_TLM_LIST_LINES_TOOLTIP");
                case UiCategoryTab.DepotListing: return Locale.Get("K45_TLM_LIST_DEPOT_TOOLTIP");
                case UiCategoryTab.TourListing: return Locale.Get("K45_TLM_LIST_TOURS_TOOLTIP");
                case UiCategoryTab.PrefixEditor: return Locale.Get("K45_TLM_CITY_ASSETS_SELECTION");
                default:
                    throw new Exception($"Not supported: {tab}");
            }

        }
        public static string getTabFgSprite(this UiCategoryTab tab)
        {
            switch (tab)
            {
                case UiCategoryTab.LineListing: return "ToolbarIconPublicTransport";
                case UiCategoryTab.DepotListing: return "UIFilterBigBuildings";
                case UiCategoryTab.TourListing: return "InfoIconTours";
                case UiCategoryTab.PrefixEditor: return "InfoIconLevel";
                default:
                    throw new Exception($"Not supported: {tab}");
            }
        }
        public static bool isScrollable(this UiCategoryTab tab)
        {
            switch (tab)
            {
                case UiCategoryTab.LineListing: return false;
                case UiCategoryTab.DepotListing: return false;
                case UiCategoryTab.TourListing: return false;
                case UiCategoryTab.PrefixEditor:
                default:
                    return true;
            }
        }
        public static Type getTabGenericContentImpl(this UiCategoryTab tab)
        {
            switch (tab)
            {
                case UiCategoryTab.LineListing: return typeof(TLMTabControllerLineListTransport<>);
                case UiCategoryTab.TourListing: return typeof(TLMTabControllerLineListTourism<>);
                case UiCategoryTab.DepotListing: return typeof(TLMTabControllerDepotList<>);
                case UiCategoryTab.PrefixEditor: return typeof(TLMTabControllerPrefixList<>);
                default:
                    throw new Exception($"Not supported: {tab}");
            }
        }
    }
}
