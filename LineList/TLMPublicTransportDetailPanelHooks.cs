using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.LineList
{
    class TLMPublicTransportDetailPanelHooks : Redirector
    {
        private bool panelOverriden = false;
        private int tryCount = 0;

        private readonly HarmonyInstance harmony = HarmonyInstance.Create("com.klyte.transportlinemanager.ptdphooks");

        public override HarmonyInstance GetHarmonyInstance()
        {
            return harmony;
        }

        private static TLMPublicTransportDetailPanelHooks _instance;
        public static TLMPublicTransportDetailPanelHooks instance
        {
            get {
                if (_instance == null) {
                    _instance = new TLMPublicTransportDetailPanelHooks();
                }
                return _instance;
            }
        }

        private static void OpenDetailPanel(int idx)
        {
            TLMPublicTransportDetailPanel publicTransportDetailPanel = UIView.library.Show<TLMPublicTransportDetailPanel>("PublicTransportDetailPanel", true, false);
            int idxRes = idx;
            switch (idx) {
                case 0:
                    idxRes = 8;
                    break;
                case 1:
                    idxRes = 7;
                    break;
                case 2:
                    idxRes = 6;
                    break;
                case 3:
                    idxRes = 4;
                    break;
                case 4:
                    idxRes = 3;
                    break;
                case 5:
                    idxRes = 1;
                    break;
                default:
                    idxRes = 5;
                    break;
            }

            publicTransportDetailPanel.SetActiveTab(idxRes);
        }
  
        #region Hooking

        public static bool preventDefault()
        {
            return false;
        }

        public static void preDoAutomation(ushort lineID, ref Boolean __state)
        {
            __state = (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None;
            if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None &&
                (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.CustomColor) != TransportLine.Flags.None
                ) {
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~TransportLine.Flags.CustomColor;
            }

        }

        public static void doAutomation(ushort lineID, Boolean __state)
        {
            if (lineID > 0 && !__state && (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None) {
                if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED)) {
                    TLMController.instance.AutoColor(lineID);
                }
                if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED)) {
                    TLMUtils.setLineName(lineID, TLMUtils.calculateAutoName(lineID));
                }
            }
        }

        public void EnableHooks()
        {
            MethodInfo preventDefault = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("preventDefault", allFlags);
            MethodInfo doAutomation = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("doAutomation", allFlags);
            MethodInfo preDoAutomation = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("preDoAutomation", allFlags);
            MethodInfo OpenDetailPanel = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OpenDetailPanel", allFlags);




            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("RefreshLines", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("Awake", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnTabChanged", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnChangeVisibleAll", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnNameSort", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnStopSort", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnVehicleSort", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnPassengerSort", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnDayAndNightSort", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnDaySort", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnColorSort", allFlags), preventDefault);
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("OnNightSort", allFlags), preventDefault);
            TLMUtils.doLog("Loading PublicTransportLineInfo Hooks!");
            AddRedirect(typeof(PublicTransportLineInfo).GetMethod("RefreshData", allFlags), preventDefault);

            TLMUtils.doLog("Loading PublicTransportInfoViewPanel Hooks!");
            AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), preventDefault, OpenDetailPanel);

            TLMUtils.doLog("Loading AutoColor & AutoName Hooks!");
            AddRedirect(typeof(TransportLine).GetMethod("AddStop", allFlags), preDoAutomation, doAutomation);

            TLMUtils.doLog("Remove PublicTransportDetailPanel Hooks!");
            update();
        }

        public void update()
        {
            if (tryCount < 100 && !panelOverriden) {
                try {
                    var go = GameObject.Find("(Library) PublicTransportDetailPanel");
                    GameObject.Destroy(go.GetComponentInChildren<PublicTransportDetailPanel>());
                    TLMPublicTransportDetailPanel.instance = go.AddComponent<TLMPublicTransportDetailPanel>();
                    panelOverriden = true;
                } catch (Exception e) {
                    tryCount++;
                    TLMUtils.doLog("Failed to load panel. Trying again a " + tryCount + getOrdinal(tryCount) + " time next frame");
                }
            }
        }

        private string getOrdinal(int nth)
        {
            if (nth % 10 == 1 && nth % 100 != 11) {
                return "st";
            } else if (nth % 10 == 2 && nth % 100 != 12) {
                return "nd";
            } else if (nth % 10 == 3 && nth % 100 != 13) {
                return "rd";
            } else {
                return "th";
            }
        }

        #endregion
    }

    class TLMPublicTransportDetailPanel : UICustomControl
    {
        private enum LineSortCriterion
        {
            DEFAULT,
            NAME,
            STOP,
            VEHICLE,
            PASSENGER,
            LINE_NUMBER
        }

        private enum DepotSortCriterion
        {
            DEFAULT,
            NAME,
            DISTRICT
        }

        private const int NUM_TRANSPORT_SYSTEMS = 9;

        public static TLMPublicTransportDetailPanel instance;

        private static readonly string kLineTemplate = "LineTemplate";

        private int m_LastLineCount;

        private bool m_Ready;


        private bool m_LinesUpdated;

        private bool[] m_ToggleAllState;

        private LineSortCriterion m_LastSortCriterionLines;

        private UITabstrip m_Strip;

        public static readonly TLMCW.ConfigIndex[] tabSystemOrder =
        {
            TLMCW.ConfigIndex.PLANE_CONFIG,
            TLMCW.ConfigIndex.BLIMP_CONFIG,
            TLMCW.ConfigIndex.SHIP_CONFIG,
            TLMCW.ConfigIndex.FERRY_CONFIG,
            TLMCW.ConfigIndex.TRAIN_CONFIG,
            TLMCW.ConfigIndex.MONORAIL_CONFIG,
            TLMCW.ConfigIndex.METRO_CONFIG,
            TLMCW.ConfigIndex.TRAM_CONFIG,
            TLMCW.ConfigIndex.BUS_CONFIG
        };

        private UIComponent m_BusLinesContainer;
        private UIComponent m_PlaneLinesContainer;
        private UIComponent m_TramLinesContainer;
        private UIComponent m_MetroLinesContainer;
        private UIComponent m_TrainLinesContainer;
        private UIComponent m_ShipLinesContainer;
        private UIComponent m_MonorailLinesContainer;
        private UIComponent m_BlimpLinesContainer;
        private UIComponent m_FerryLinesContainer;

        private UICheckBox m_ToggleAll;
        private UIButton m_DayIcon;
        private UIButton m_NightIcon;
        private UIButton m_DayNightIcon;
        private UIButton m_DisabledIcon;
        private UIDropDown m_prefixFilter;

        private UIPanel m_linesTitle;

        private UIButton m_buttonAutoName;
        private UIButton m_buttonAutoColor;

        private bool m_showDayNightLines = true;
        private bool m_showDayLines = true;
        private bool m_showNightLines = true;
        private bool m_showDisabledLines = true;

        private int m_busCount = 0;
        private int m_tramCount = 0;
        private int m_metroCount = 0;
        private int m_trainCount = 0;
        private int m_blimpCount = 0;
        private int m_ferryCount = 0;
        private int m_monorailCount = 0;

        //TLM
        private int m_shipCount = 0;
        private int m_planeCount = 0;

        private bool m_isChangingTab;

        private UILabel m_LineCount;

        public UIDropDown m_systemTypeDropDown = null;

        public bool isActivityVisible(bool day, bool night)
        {
            if (day && night) {
                return m_showDayNightLines;
            } else if (day) {
                return m_showDayLines;
            } else if (night) {
                return m_showNightLines;
            } else {
                return m_showDisabledLines;
            }
        }

        public bool isOnCurrentPrefixFilter(int lineNumber)
        {
            return !m_prefixFilter.isVisible || m_prefixFilter.selectedIndex == 0 || m_prefixFilter.selectedIndex - 1 == (int) (lineNumber / 1000);
        }

        public TLMCW.ConfigIndex currentSelectedSystem
        {
            get {
                return tabSystemOrder[m_Strip.selectedIndex % tabSystemOrder.Length];
            }
        }

        public bool isOnCurrentPrefixFilter(List<uint> prefixes)
        {
            return !m_prefixFilter.isVisible || m_prefixFilter.selectedIndex == 0 || prefixes.Contains((uint) (m_prefixFilter.selectedIndex - 1));
        }

        private void Awake()
        {
            //this.m_Strip.tab
            enabled = true;
            TLMUtils.clearAllVisibilityEvents(this.GetComponent<UIPanel>());

            this.m_LineCount = base.Find<UILabel>("LabelLineCount");

            AwakeRearrangeTabs();
            AwakeLinesTitleComponents();
            AwakeTopButtons();
            AwakeDayNightOptions();
            AwakePrefixFilter();
            fixShortcutButtons();

            m_Ready = true;
        }

        private void fixShortcutButtons()
        {

        }

        private void AwakePrefixFilter()
        {
            m_prefixFilter = UIHelperExtension.CloneBasicDropDownNoLabel(new string[] {
                "All"
            }, (x) => { }, component);

            m_prefixFilter.area = new Vector4(765, 80, 100, 35);

            var prefixFilterLabel = m_prefixFilter.AddUIComponent<UILabel>();
            prefixFilterLabel.text = Locale.Get("TLM_PREFIX_FILTER");
            prefixFilterLabel.relativePosition = new Vector3(0, -35);
            prefixFilterLabel.textAlignment = UIHorizontalAlignment.Center;
            prefixFilterLabel.wordWrap = true;
            prefixFilterLabel.autoSize = false;
            prefixFilterLabel.width = 100;
            prefixFilterLabel.height = 36;


            UISprite icon = Find<UISprite>("Icon");
            icon.spriteName = "TransportLinesManagerIconHovered";
            icon.atlas = TLMController.taTLM;

            var title = Find<UILabel>("Label");
            title.suffix = " - TLM Lite v" + TransportLinesManagerMod.version;

            component.relativePosition = new Vector3(395, 58);
        }

        private void AwakeRearrangeTabs()
        {
            this.m_Strip = Find<UITabstrip>("Tabstrip");
            this.m_Strip.relativePosition = new Vector3(13, 45);

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Strips Lines");
            var bus = m_Strip.tabs[0].GetComponent<UIButton>();
            var tram = m_Strip.tabs[1].GetComponent<UIButton>();
            var metro = m_Strip.tabs[2].GetComponent<UIButton>();
            var train = m_Strip.tabs[3].GetComponent<UIButton>();
            var ferry = m_Strip.tabs[4].GetComponent<UIButton>();
            var blimp = m_Strip.tabs[5].GetComponent<UIButton>();
            var monorail = m_Strip.tabs[6].GetComponent<UIButton>();
            var ship = m_Strip.AddTab("");
            var plane = m_Strip.AddTab("");

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Tab init - lines");
            int tabIt = 0;

            addIcon("PlaneLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref plane, false, tabIt++, "TLM_PUBLICTRANSPORT_AIRPLANELINES");
            addIcon("Blimp", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref blimp, false, tabIt++, "TLM_PUBLICTRANSPORT_BLIMPLINES");
            addIcon("ShipLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ship, false, tabIt++, "TLM_PUBLICTRANSPORT_SHIPLINES");
            addIcon("Ferry", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ferry, false, tabIt++, "TLM_PUBLICTRANSPORT_FERRYLINES");
            addIcon("Train", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref train, false, tabIt++, "PUBLICTRANSPORT_TRAINLINES");
            addIcon("Monorail", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Monorail), ref monorail, false, tabIt++, "PUBLICTRANSPORT_MONORAILLINES");
            addIcon("Subway", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metro, false, tabIt++, "PUBLICTRANSPORT_METROLINES");
            addIcon("Tram", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tram, false, tabIt++, "PUBLICTRANSPORT_TRAMLINES");
            addIcon("Bus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref bus, false, tabIt++, "PUBLICTRANSPORT_BUSLINES");

            tram.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Tram);
            ferry.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            monorail.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            blimp.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);

            this.m_BusLinesContainer = Find<UIComponent>("BusDetail").Find("Container");
            this.m_TramLinesContainer = Find<UIComponent>("TramDetail").Find("Container");
            this.m_MetroLinesContainer = Find<UIComponent>("MetroDetail").Find("Container");
            this.m_TrainLinesContainer = Find<UIComponent>("TrainDetail").Find("Container");
            this.m_BlimpLinesContainer = Find<UIComponent>("BlimpDetail").Find("Container");
            this.m_MonorailLinesContainer = Find<UIComponent>("MonorailDetail").Find("Container");
            this.m_FerryLinesContainer = Find<UIComponent>("FerryDetail").Find("Container");

            m_BusLinesContainer.eventVisibilityChanged += null;
            m_TramLinesContainer.eventVisibilityChanged += null;
            m_MetroLinesContainer.eventVisibilityChanged += null;
            m_TrainLinesContainer.eventVisibilityChanged += null;
            m_BlimpLinesContainer.eventVisibilityChanged += null;
            m_MonorailLinesContainer.eventVisibilityChanged += null;
            m_FerryLinesContainer.eventVisibilityChanged += null;

            CopyContainerFromBus(NUM_TRANSPORT_SYSTEMS - 2, ref m_ShipLinesContainer);
            CopyContainerFromBus(NUM_TRANSPORT_SYSTEMS - 1, ref m_PlaneLinesContainer);

            RemoveExtraLines(0, ref m_BusLinesContainer);
            RemoveExtraLines(0, ref m_TramLinesContainer);
            RemoveExtraLines(0, ref m_MetroLinesContainer);
            RemoveExtraLines(0, ref m_TrainLinesContainer);
            RemoveExtraLines(0, ref m_MonorailLinesContainer);
            RemoveExtraLines(0, ref m_ShipLinesContainer);
            RemoveExtraLines(0, ref m_PlaneLinesContainer);
            RemoveExtraLines(0, ref m_FerryLinesContainer);
            RemoveExtraLines(0, ref m_BlimpLinesContainer);

            tabIt = 0;
            plane.zOrder = tabIt++;
            blimp.zOrder = (tabIt++);
            ship.zOrder = (tabIt++);
            ferry.zOrder = (tabIt++);
            train.zOrder = (tabIt++);
            monorail.zOrder = (tabIt++);
            metro.zOrder = (tabIt++);
            tram.zOrder = (tabIt++);
            bus.zOrder = (tabIt++);

            tabIt = 0;
            m_PlaneLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_BlimpLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_ShipLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_FerryLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_TrainLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_MonorailLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_MetroLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_TramLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_BusLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
        }

        private void AwakeLinesTitleComponents()
        {

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Find Panel title");
            m_linesTitle = Find<UIPanel>("LineTitle");
            this.m_ToggleAllState = new bool[this.m_Strip.tabCount / 2];
            this.m_Strip.eventSelectedIndexChanged += null;
            this.m_Strip.eventSelectedIndexChanged += new PropertyChangedEventHandler<int>(this.OnTabChanged);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Find Toggle button");
            this.m_ToggleAll = m_linesTitle.Find<UICheckBox>("ToggleAll");
            this.m_ToggleAll.eventCheckChanged += new PropertyChangedEventHandler<bool>(this.CheckChangedFunction);
            for (int i = 0; i < this.m_ToggleAllState.Length; i++) {
                this.m_ToggleAllState[i] = true;
            }
            m_linesTitle.Find<UIButton>("NameTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r) {
                this.OnNameSort();
            };
            m_linesTitle.Find<UIButton>("StopsTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r) {
                this.OnStopSort();
            };
            m_linesTitle.Find<UIButton>("VehiclesTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r) {
                this.OnVehicleSort();
            };
            m_linesTitle.Find<UIButton>("PassengersTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r) {
                this.OnPassengerSort();
            };
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Find Color title");
            var colorTitle = m_linesTitle.Find<UIButton>("ColorTitle");
            colorTitle.text += "/" + Locale.Get("TLM_CODE_SHORT");
            colorTitle.eventClick += delegate (UIComponent c, UIMouseEventParameter r) {
                this.OnLineNumberSort();
            };

            this.m_LastSortCriterionLines = LineSortCriterion.DEFAULT;
        }

        private void AwakeTopButtons()
        {
            //Auto color & Auto Name
            TLMUtils.createUIElement<UIButton>(ref m_buttonAutoName, transform);
            m_buttonAutoName.pivot = UIPivotPoint.TopRight;
            m_buttonAutoName.textScale = 0.6f;
            m_buttonAutoName.width = 40;
            m_buttonAutoName.height = 40;
            m_buttonAutoName.tooltip = Locale.Get("TLM_AUTO_NAME_ALL_TOOLTIP");
            TLMUtils.initButton(m_buttonAutoName, true, "ButtonMenu");
            m_buttonAutoName.name = "AutoName";
            m_buttonAutoName.isVisible = true;
            m_buttonAutoName.eventClick += (component, eventParam) => {
                OnAutoNameAll();
            };

            var icon = m_buttonAutoName.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.spriteName = "AutoNameIcon";
            icon.width = 36;
            icon.height = 36;

            TLMUtils.createUIElement<UIButton>(ref m_buttonAutoColor, transform);
            m_buttonAutoColor.pivot = UIPivotPoint.TopRight;
            m_buttonAutoColor.textScale = 0.6f;
            m_buttonAutoColor.width = 40;
            m_buttonAutoColor.height = 40;
            m_buttonAutoColor.tooltip = Locale.Get("TLM_AUTO_COLOR_ALL_TOOLTIP");
            TLMUtils.initButton(m_buttonAutoColor, true, "ButtonMenu");
            m_buttonAutoColor.name = "AutoColor";
            m_buttonAutoColor.isVisible = true;
            m_buttonAutoColor.eventClick += (component, eventParam) => {
                OnAutoColorAll();
            };

            icon = m_buttonAutoColor.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "AutoColorIcon";

            m_buttonAutoColor.relativePosition = new Vector3(675, 43);
            m_buttonAutoName.relativePosition = new Vector3(720, 43);
        }

        private void AwakeDayNightOptions()
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Find Original buttons");
            m_DayIcon = m_linesTitle.Find<UIButton>("DayButton");
            m_NightIcon = m_linesTitle.Find<UIButton>("NightButton");
            m_DayNightIcon = m_linesTitle.Find<UIButton>("DayNightButton");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Create disabled button");
            m_DisabledIcon = GameObject.Instantiate(m_DayIcon.gameObject).GetComponent<UIButton>();
            m_DisabledIcon.transform.SetParent(m_DayIcon.transform.parent);
            m_NightIcon.relativePosition = new Vector3(670, 14);
            m_DayNightIcon.relativePosition = new Vector3(695, 14);
            m_DisabledIcon.normalBgSprite = "Niet";
            m_DisabledIcon.hoveredBgSprite = "Niet";
            m_DisabledIcon.pressedBgSprite = "Niet";
            m_DisabledIcon.disabledBgSprite = "Niet";


            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Set Tooltips");
            m_DayIcon.tooltip = Locale.Get("TLM_DAY_FILTER_TOOLTIP");
            m_NightIcon.tooltip = Locale.Get("TLM_NIGHT_FILTER_TOOLTIP");
            m_DayNightIcon.tooltip = Locale.Get("TLM_DAY_NIGHT_FILTER_TOOLTIP");
            m_DisabledIcon.tooltip = Locale.Get("TLM_DISABLED_LINES_FILTER_TOOLTIP");

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Set events");
            m_DayIcon.eventClick += (x, y) => {
                m_showDayLines = !m_showDayLines;
                m_DayIcon.color = m_showDayLines ? Color.white : Color.black;
                m_DayIcon.focusedColor = m_showDayLines ? Color.white : Color.black;
            };
            m_NightIcon.eventClick += (x, y) => {
                m_showNightLines = !m_showNightLines;
                m_NightIcon.color = m_showNightLines ? Color.white : Color.black;
                m_NightIcon.focusedColor = m_showDayLines ? Color.white : Color.black;
            };
            m_DayNightIcon.eventClick += (x, y) => {
                m_showDayNightLines = !m_showDayNightLines;
                m_DayNightIcon.color = m_showDayNightLines ? Color.white : Color.black;
                m_DayNightIcon.focusedColor = m_showDayLines ? Color.white : Color.black;
            };
            m_DisabledIcon.eventClick += (x, y) => {
                m_showDisabledLines = !m_showDisabledLines;
                m_DisabledIcon.color = m_showDisabledLines ? Color.white : Color.black;
                m_DisabledIcon.focusedColor = m_showDayLines ? Color.white : Color.black;
            };

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Set position");
            m_DisabledIcon.relativePosition = new Vector3(736, 14);
        }

        private void CopyContainerFromBus(int idx, ref UIComponent item)
        {
            item = GameObject.Instantiate(m_BusLinesContainer.gameObject).GetComponent<UIComponent>();
            item.transform.SetParent(m_Strip.tabContainer.gameObject.transform.GetChild(idx));
            item.name = "Container";
            var scroll = GameObject.Instantiate(Find<UIComponent>("BusDetail").Find("Scrollbar"));
            scroll.transform.SetParent(m_Strip.tabContainer.gameObject.transform.GetChild(idx));
            scroll.name = "Scrollbar";
            item.transform.localPosition = m_BusLinesContainer.transform.localPosition;
            scroll.transform.localPosition = Find<UIComponent>("BusDetail").Find("Scrollbar").transform.localPosition;
            item.GetComponent<UIScrollablePanel>().verticalScrollbar = scroll.GetComponent<UIScrollbar>();
            item.eventVisibilityChanged += null;
            scroll.GetComponent<UIScrollbar>().zOrder = 1;
        }



        private void addIcon(string namePrefix, string iconName, ref UIButton targetButton, bool alternativeIconAtlas, int tabIdx, string tooltipText = "", bool isTooltipLocale = true)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("addIcon: init " + namePrefix);

            TLMUtils.initButtonFg(targetButton, false, "");

            targetButton.atlas = TLMController.taLineNumber;
            if (tooltipText == "") {
                targetButton.width = 01;
                targetButton.height = 01;
                TLMUtils.initButtonSameSprite(targetButton, "");
                targetButton.isVisible = false;
            } else {
                targetButton.width = 40;
                targetButton.height = 40;
                targetButton.name = namePrefix + "Legend";
                TLMUtils.initButtonSameSprite(targetButton, namePrefix + "Icon");
                targetButton.color = new Color32(20, 20, 20, 255);
                targetButton.hoveredColor = Color.gray;
                targetButton.focusedColor = Color.green / 2;
                targetButton.eventClick += null;
                targetButton.eventClick += (x, y) => {
                    SetActiveTab(tabIdx);
                };
            }
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("addIcon: pre eventClick");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("addIcon: init label icon");
            UILabel icon = targetButton.AddUIComponent<UILabel>();
            if (alternativeIconAtlas) {
                icon.atlas = TLMController.taLineNumber;
                icon.width = 27;
                icon.height = 27;
                icon.relativePosition = new Vector3(6f, 6);
            } else {
                icon.width = 30;
                icon.height = 20;
                icon.relativePosition = new Vector3(5f, 10f);
            }

            if (isTooltipLocale) {
                icon.tooltipLocaleID = tooltipText;
            } else {
                icon.tooltip = tooltipText;
            }

            icon.backgroundSprite = iconName;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("addIcon: end");
        }

        #region Sorting

        private static int CompareNames(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfoItem component = left.GetComponent<TLMPublicTransportLineInfoItem>();
            TLMPublicTransportLineInfoItem component2 = right.GetComponent<TLMPublicTransportLineInfoItem>();
            return string.Compare(component.lineName, component2.lineName, StringComparison.InvariantCulture); //NaturalCompare(component.lineName, component2.lineName);
        }

        private static int CompareLineNumbers(UIComponent left, UIComponent right)
        {
            if (left == null || right == null)
                return 0;
            TLMPublicTransportLineInfoItem component = left.GetComponent<TLMPublicTransportLineInfoItem>();
            TLMPublicTransportLineInfoItem component2 = right.GetComponent<TLMPublicTransportLineInfoItem>();
            if (component == null || component2 == null)
                return 0;
            return component.lineNumber.CompareTo(component2.lineNumber);
        }

        private static int CompareStops(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfoItem component = left.GetComponent<TLMPublicTransportLineInfoItem>();
            TLMPublicTransportLineInfoItem component2 = right.GetComponent<TLMPublicTransportLineInfoItem>();
            return NaturalCompare(component2.stopCounts, component.stopCounts);
        }

        private static int CompareVehicles(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfoItem component = left.GetComponent<TLMPublicTransportLineInfoItem>();
            TLMPublicTransportLineInfoItem component2 = right.GetComponent<TLMPublicTransportLineInfoItem>();
            return NaturalCompare(component2.vehicleCounts, component.vehicleCounts);
        }

        private static int ComparePassengers(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfoItem component = left.GetComponent<TLMPublicTransportLineInfoItem>();
            TLMPublicTransportLineInfoItem component2 = right.GetComponent<TLMPublicTransportLineInfoItem>();
            return component2.passengerCountsInt.CompareTo(component.passengerCountsInt);
        }
        private static int NaturalCompare(string left, string right)
        {
            return (int) typeof(PublicTransportDetailPanel).GetMethod("NaturalCompare", Redirector.allFlags).Invoke(null, new object[] { left, right });
        }
        private void OnNameSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareNames));
            this.m_LastSortCriterionLines = LineSortCriterion.NAME;
            uIComponent.Invalidate();
        }


        private void OnStopSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareStops));
            this.m_LastSortCriterionLines = LineSortCriterion.STOP;
            uIComponent.Invalidate();
        }

        private void OnVehicleSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareVehicles));
            this.m_LastSortCriterionLines = LineSortCriterion.VEHICLE;
            uIComponent.Invalidate();
        }

        private void OnPassengerSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(ComparePassengers));
            this.m_LastSortCriterionLines = LineSortCriterion.PASSENGER;
            uIComponent.Invalidate();
        }

        private void OnLineNumberSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareLineNumbers));
            this.m_LastSortCriterionLines = LineSortCriterion.LINE_NUMBER;
            uIComponent.Invalidate();
        }

        public static void Quicksort(IList<UIComponent> elements, Comparison<UIComponent> comp)
        {
            Quicksort(elements, 0, elements.Count - 1, comp);
        }

        public static void Quicksort(IList<UIComponent> elements, int left, int right, Comparison<UIComponent> comp)
        {
            int i = left;
            int num = right;
            UIComponent y = elements[(left + right) / 2];
            while (i <= num) {
                while (comp(elements[i], y) < 0) {
                    i++;
                }
                while (comp(elements[num], y) > 0) {
                    num--;
                }
                if (i <= num) {
                    UIComponent value = elements[i];
                    elements[i] = elements[num];
                    elements[i].forceZOrder = i;
                    elements[num] = value;
                    elements[num].forceZOrder = num;
                    i++;
                    num--;
                }
            }
            if (left < num) {
                Quicksort(elements, left, num, comp);
            }
            if (i < right) {
                Quicksort(elements, i, right, comp);
            }
        }
        #endregion

        public void SetActiveTab(int idx)
        {
            var selIdx = idx;
            if (idx >= NUM_TRANSPORT_SYSTEMS) {
                selIdx = NUM_TRANSPORT_SYSTEMS - 1;
            }

            if (this.m_Strip.selectedIndex != selIdx) {
                this.m_Strip.selectedIndex = selIdx;
                m_Strip.tabs[idx].GetComponentInChildren<UIButton>().state = UIButton.ButtonState.Focused;
                RefreshLines();
            }
        }


        public void RefreshLines()
        {
            if (Singleton<TransportManager>.exists) {
                UIComponent comp;
                m_busCount = 0;
                m_tramCount = 0;
                m_metroCount = 0;
                m_trainCount = 0;

                //TLM
                m_shipCount = 0;
                m_planeCount = 0;
                m_ferryCount = 0;
                m_blimpCount = 0;
                m_monorailCount = 0;

                for (ushort lineIdIterator = 1; lineIdIterator < 256; lineIdIterator += 1) {
                    if ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineIdIterator].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created) {
                        switch (TLMCW.getDefinitionForLine(lineIdIterator).toConfigIndex()) {
                            case TLMCW.ConfigIndex.BUS_CONFIG:
                                comp = m_BusLinesContainer;
                                m_busCount = AddToList(m_busCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.TRAM_CONFIG:
                                comp = m_TramLinesContainer;
                                m_tramCount = AddToList(m_tramCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.METRO_CONFIG:
                                comp = m_MetroLinesContainer;
                                m_metroCount = AddToList(m_metroCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.TRAIN_CONFIG:
                                comp = m_TrainLinesContainer;
                                m_trainCount = AddToList(m_trainCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.SHIP_CONFIG:
                                comp = m_ShipLinesContainer;
                                m_shipCount = AddToList(m_shipCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.PLANE_CONFIG:
                                comp = m_PlaneLinesContainer;
                                m_planeCount = AddToList(m_planeCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.FERRY_CONFIG:
                                comp = m_FerryLinesContainer;
                                m_ferryCount = AddToList(m_ferryCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.BLIMP_CONFIG:
                                comp = m_BlimpLinesContainer;
                                m_blimpCount = AddToList(m_blimpCount, lineIdIterator, ref comp);
                                break;
                            case TLMCW.ConfigIndex.MONORAIL_CONFIG:
                                comp = m_MonorailLinesContainer;
                                m_monorailCount = AddToList(m_monorailCount, lineIdIterator, ref comp);
                                break;
                        }
                    }
                }
                comp = m_BusLinesContainer;
                RemoveExtraLines(m_busCount, ref comp);
                comp = m_TramLinesContainer;
                RemoveExtraLines(m_tramCount, ref comp);
                comp = m_MetroLinesContainer;
                RemoveExtraLines(m_metroCount, ref comp);
                comp = m_TrainLinesContainer;
                RemoveExtraLines(m_trainCount, ref comp);
                comp = m_ShipLinesContainer;
                RemoveExtraLines(m_shipCount, ref comp);
                comp = m_PlaneLinesContainer;
                RemoveExtraLines(m_planeCount, ref comp);
                comp = m_MonorailLinesContainer;
                RemoveExtraLines(m_monorailCount, ref comp);
                comp = m_BlimpLinesContainer;
                RemoveExtraLines(m_blimpCount, ref comp);
                comp = m_FerryLinesContainer;
                RemoveExtraLines(m_ferryCount, ref comp);

                this.m_LinesUpdated = true;

            }

        }

        private static void RemoveExtraLines(int linesCount, ref UIComponent component)
        {
            while (component.components.Count > linesCount) {
                UIComponent uIComponent = component.components[linesCount];
                component.RemoveUIComponent(uIComponent);
                UnityEngine.Object.Destroy(uIComponent.gameObject);
            }
        }

        private int AddToList(int count, ushort lineIdIterator, ref UIComponent component)
        {
            TLMPublicTransportLineInfoItem publicTransportLineInfo;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("PreIF");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Count = {0}; Component = {1}; components count = {2}", count, component.ToString(), component.components.Count);
            if (count >= component.components.Count) {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                    TLMUtils.doLog("IF TRUE");
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                publicTransportLineInfo = temp.AddComponent<TLMPublicTransportLineInfoItem>();
                component.AttachUIComponent(publicTransportLineInfo.gameObject);
            } else {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                    TLMUtils.doLog("IF FALSE");
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                    TLMUtils.doLog("component.components[count] = {0};", component.components[count]);
                publicTransportLineInfo = component.components[count].GetComponent<TLMPublicTransportLineInfoItem>();
            }
            publicTransportLineInfo.lineID = lineIdIterator;
            publicTransportLineInfo.RefreshData(true, false);
            count++;
            return count;
        }

        private void OnTabChanged(UIComponent c, int idx)
        {
            if (this.m_ToggleAll != null) {
                m_isChangingTab = true;
                this.m_ToggleAll.isChecked = idx < this.m_ToggleAllState.Length ? this.m_ToggleAllState[idx] : false;
                m_isChangingTab = false;
            }
            string[] filterOptions = TLMUtils.getFilterPrefixesOptions(tabSystemOrder[idx % (m_Strip.tabCount / 2)]);
            if (filterOptions.Length < 3) {
                m_prefixFilter.isVisible = false;
            } else {
                m_prefixFilter.isVisible = true;
                m_prefixFilter.items = filterOptions;
                m_prefixFilter.selectedIndex = 0;
            }

            m_DisabledIcon.relativePosition = new Vector3(736, 14);
            this.OnLineNumberSort();
            RefreshLineCount(m_Strip.selectedIndex);
        }

        private void CheckChangedFunction(UIComponent c, bool r)
        {
            if (!m_isChangingTab) {
                this.OnChangeVisibleAll(r);
            }
        }

        private void OnChangeVisibleAll(bool visible)
        {
            if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count) {
                this.m_ToggleAllState[this.m_Strip.selectedIndex] = visible;
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
                if (uIComponent != null) {
                    for (int i = 0; i < uIComponent.components.Count; i++) {
                        UIComponent uIComponent2 = uIComponent.components[i];
                        if (uIComponent2 != null) {
                            UICheckBox uICheckBox = uIComponent2.Find<UICheckBox>("LineVisible");
                            if (uICheckBox) {
                                uICheckBox.isChecked = visible;
                            }
                        }
                    }
                }
                this.RefreshLines();
            }
        }

        private void OnAutoNameAll()
        {
            if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count) {
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
                if (uIComponent != null) {
                    for (int i = 0; i < uIComponent.components.Count; i++) {
                        TLMPublicTransportLineInfoItem uIComponent2 = uIComponent.components[i].GetComponent<TLMPublicTransportLineInfoItem>();
                        if (uIComponent2 != null) {
                            uIComponent2.DoAutoName();
                        }
                    }
                }
                this.RefreshLines();
            }
        }


        private void OnAutoColorAll()
        {
            if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count) {
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
                if (uIComponent != null) {
                    for (int i = 0; i < uIComponent.components.Count; i++) {
                        TLMPublicTransportLineInfoItem uIComponent2 = uIComponent.components[i].GetComponent<TLMPublicTransportLineInfoItem>();
                        if (uIComponent2 != null) {
                            uIComponent2.DoAutoColor();
                        }
                    }
                }
                this.RefreshLines();
            }
        }

        private void Update()
        {
            if (Singleton<TransportManager>.exists && this.m_Ready && this.m_LastLineCount != Singleton<TransportManager>.instance.m_lineCount) {
                this.RefreshLines();
                this.m_LastLineCount = Singleton<TransportManager>.instance.m_lineCount;
                RefreshLineCount(m_Strip.selectedIndex);
            }
            if (this.m_LinesUpdated) {
                this.m_LinesUpdated = false;
                if (this.m_LastSortCriterionLines != LineSortCriterion.DEFAULT) {
                    if (this.m_LastSortCriterionLines == LineSortCriterion.NAME) {
                        this.OnNameSort();
                    } else if (this.m_LastSortCriterionLines == LineSortCriterion.PASSENGER) {
                        this.OnPassengerSort();
                    } else if (this.m_LastSortCriterionLines == LineSortCriterion.STOP) {
                        this.OnStopSort();
                    } else if (this.m_LastSortCriterionLines == LineSortCriterion.VEHICLE) {
                        this.OnVehicleSort();
                    } else if (this.m_LastSortCriterionLines == LineSortCriterion.LINE_NUMBER) {
                        this.OnLineNumberSort();
                    }
                } else {
                    this.OnLineNumberSort();
                }
            }
        }



        private void formatTabButton(UIButton tabButton)
        {
            tabButton.textPadding = new RectOffset(10, 10, 5, 0);
            tabButton.autoSize = true;
            tabButton.normalBgSprite = "GenericTab";
            tabButton.focusedBgSprite = "GenericTabFocused";
            tabButton.hoveredBgSprite = "GenericTabHovered";
            tabButton.pressedBgSprite = "GenericTabPressed";
            tabButton.disabledBgSprite = "GenericTabDisabled";
        }


        private void RefreshLineCount(int transportTabIndex)
        {
            string arg = Locale.Get("TLM_PUBLICTRANSPORT_LINECOUNT", transportTabIndex);
            this.m_LineCount.text = arg + ": " + new int[] {
                    m_planeCount,
                    m_blimpCount,
                    m_shipCount,
                    m_ferryCount,
                    m_trainCount,
                    m_monorailCount,
                    m_metroCount,
                    m_tramCount,
                    m_busCount
                }[transportTabIndex];
        }

    }


}
