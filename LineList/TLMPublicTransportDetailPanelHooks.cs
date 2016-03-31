using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
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

        private static TLMPublicTransportDetailPanelHooks _instance;
        public static TLMPublicTransportDetailPanelHooks instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMPublicTransportDetailPanelHooks();
                }
                return _instance;
            }
        }

        private void RefreshData(bool colors, bool visible) { }
        private void OnTabChanged(UIComponent c, int idx) { }
        private void Awake() { }
        private void RefreshLines() { }
        private void OnChangeVisibleAll(bool vb) { }
        private void OnNameSort() { }
        private void OnStopSort() { }
        private void OnVehicleSort() { }
        private void OnPassengerSort() { }
        private void OpenDetailPanel(int idx)
        {
            TLMPublicTransportDetailPanel publicTransportDetailPanel = UIView.library.Show<TLMPublicTransportDetailPanel>("PublicTransportDetailPanel", true, false);
            publicTransportDetailPanel.SetActiveTab(5 - idx);
        }

        #region Hooking
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();
        public void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading TLMPublicTransportDetailPanelHooks Hooks!");
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("RefreshLines", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("Awake", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnTabChanged", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnChangeVisibleAll", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnNameSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnStopSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnVehicleSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnPassengerSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportInfoViewPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OpenDetailPanel", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportLineInfo), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("RefreshData", allFlags), ref redirects);

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Inverse TLMPublicTransportDetailPanelHooks Hooks!");
            AddRedirect(typeof(TLMPublicTransportDetailPanel), typeof(PublicTransportDetailPanel).GetMethod("NaturalCompare", allFlags), ref redirects);

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Swap TLMPublicTransportDetailPanelHooks Hooks!");
            update();
        }

        public void update()
        {
            if (tryCount < 100 && !panelOverriden)
            {
                try
                {
                    var go = GameObject.Find("UIView").GetComponentInChildren<PublicTransportDetailPanel>().gameObject;
                    GameObject.Destroy(go.GetComponent<PublicTransportDetailPanel>());
                    TLMPublicTransportDetailPanel.instance = go.AddComponent<TLMPublicTransportDetailPanel>();
                    panelOverriden = true;
                }
                catch (Exception e)
                {
                    tryCount++;
                    TLMUtils.doLog("Failed to load panel. Trying again a " + tryCount + getOrdinal(tryCount) + " time next frame");
                }
            }
        }

        private string getOrdinal(int nth)
        {
            if (nth % 10 == 1 && nth % 100 != 11)
            {
                return "st";
            }
            else if (nth % 10 == 2 && nth % 100 != 12)
            {
                return "nd";
            }
            else if (nth % 10 == 3 && nth % 100 != 13)
            {
                return "rd";
            }
            else
            {
                return "th";
            }
        }

        public void DisableHooks()
        {
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            redirects.Clear();
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
            TLMCW.ConfigIndex.SHIP_CONFIG,
            TLMCW.ConfigIndex.TRAIN_CONFIG,
            TLMCW.ConfigIndex.METRO_CONFIG,
            TLMCW.ConfigIndex.TRAM_CONFIG,
            TLMCW.ConfigIndex.BUS_CONFIG,
            TLMCW.ConfigIndex.NIL,
        };

        private UIComponent m_BusLinesContainer;
        private UIComponent m_PlaneLinesContainer;
        private UIComponent m_TramLinesContainer;
        private UIComponent m_MetroLinesContainer;
        private UIComponent m_TrainLinesContainer;
        private UIComponent m_ShipLinesContainer;

        private UIComponent m_PrefixEditor;

        private UIComponent m_BusDepotsContainer;
        private UIComponent m_PlaneDepotsContainer;
        private UIComponent m_TramDepotsContainer;
        private UIComponent m_MetroDepotsContainer;
        private UIComponent m_TrainDepotsContainer;
        private UIComponent m_ShipDepotsContainer;

        private UICheckBox m_ToggleAll;
        private UISprite m_DayIcon;
        private UISprite m_NightIcon;
        private UISprite m_DayNightIcon;
        private UISprite m_DisabledIcon;
        private UIDropDown m_prefixFilter;

        private UIPanel m_linesTitle;
        private UIPanel m_depotsTitle;

        private UIButton m_buttonAutoName;
        private UIButton m_buttonAutoColor;

        private bool m_showDayNightLines = true;
        private bool m_showDayLines = true;
        private bool m_showNightLines = true;
        private bool m_showDisabledLines = true;


        public UIDropDown m_systemTypeDropDown = null;

        public bool isActivityVisible(bool day, bool night)
        {
            if (day && night)
            {
                return m_showDayNightLines;
            }
            else if (day)
            {
                return m_showDayLines;
            }
            else if (night)
            {
                return m_showNightLines;
            }
            else
            {
                return m_showDisabledLines;
            }
        }

        public bool isOnCurrentPrefixFilter(int lineNumber)
        {
            return !m_prefixFilter.isVisible || m_prefixFilter.selectedIndex == 0 || m_prefixFilter.selectedIndex - 1 == (int)(lineNumber / 1000);
        }

        public TLMCW.ConfigIndex currentSelectedSystem
        {
            get
            {
                return tabSystemOrder[m_Strip.selectedIndex % tabSystemOrder.Length];
            }
        }

        public bool isOnCurrentPrefixFilter(List<uint> prefixes)
        {
            return !m_prefixFilter.isVisible || m_prefixFilter.selectedIndex == 0 || prefixes.Contains((uint)(m_prefixFilter.selectedIndex - 1));
        }

        public bool isDepotView
        {
            get
            {
                return m_Strip.selectedIndex >= 7;
            }
        }
        public bool isLineView
        {
            get
            {
                return m_Strip.selectedIndex <= 5;
            }
        }
        public bool isPrefixEditor
        {
            get
            {
                return m_Strip.selectedIndex == 6;
            }
        }

        private static int CompareDepotNames(UIComponent left, UIComponent right)
        {
            TLMPublicTransportDepotInfo component = left.GetComponent<TLMPublicTransportDepotInfo>();
            TLMPublicTransportDepotInfo component2 = right.GetComponent<TLMPublicTransportDepotInfo>();
            return string.Compare(component.buidingName, component2.buidingName, StringComparison.InvariantCulture); //NaturalCompare(component.lineName, component2.lineName);
        }

        private static int CompareDepotDistricts(UIComponent left, UIComponent right)
        {
            TLMPublicTransportDepotInfo component = left.GetComponent<TLMPublicTransportDepotInfo>();
            TLMPublicTransportDepotInfo component2 = right.GetComponent<TLMPublicTransportDepotInfo>();
            return string.Compare(component.districtName, component2.districtName, StringComparison.InvariantCulture); //NaturalCompare(component.lineName, component2.lineName);
        }

        private static int CompareNames(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfo component = left.GetComponent<TLMPublicTransportLineInfo>();
            TLMPublicTransportLineInfo component2 = right.GetComponent<TLMPublicTransportLineInfo>();
            return string.Compare(component.lineName, component2.lineName, StringComparison.InvariantCulture); //NaturalCompare(component.lineName, component2.lineName);
        }

        private static int CompareLineNumbers(UIComponent left, UIComponent right)
        {
            if (left == null || right == null) return 0;
            TLMPublicTransportLineInfo component = left.GetComponent<TLMPublicTransportLineInfo>();
            TLMPublicTransportLineInfo component2 = right.GetComponent<TLMPublicTransportLineInfo>();
            if (component == null || component2 == null) return 0;
            return component.lineNumber.CompareTo(component2.lineNumber);
        }

        private static int CompareStops(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfo component = left.GetComponent<TLMPublicTransportLineInfo>();
            TLMPublicTransportLineInfo component2 = right.GetComponent<TLMPublicTransportLineInfo>();
            return NaturalCompare(component2.stopCounts, component.stopCounts);
        }

        private static int CompareVehicles(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfo component = left.GetComponent<TLMPublicTransportLineInfo>();
            TLMPublicTransportLineInfo component2 = right.GetComponent<TLMPublicTransportLineInfo>();
            return NaturalCompare(component2.vehicleCounts, component.vehicleCounts);
        }

        private static int ComparePassengers(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfo component = left.GetComponent<TLMPublicTransportLineInfo>();
            TLMPublicTransportLineInfo component2 = right.GetComponent<TLMPublicTransportLineInfo>();
            return component2.passengerCountsInt.CompareTo(component.passengerCountsInt);
        }
        private static int NaturalCompare(string left, string right)
        {
            return 0;
        }

        private void Awake()
        {
            //this.m_Strip.tab
            enabled = true;
            TLMUtils.clearAllVisibilityEvents(this.GetComponent<UIPanel>());

            m_linesTitle = Find<UIPanel>("LineTitle");
            m_depotsTitle = GameObject.Instantiate<UIPanel>(m_linesTitle);
            m_depotsTitle.transform.SetParent(m_linesTitle.transform.parent);
            m_depotsTitle.relativePosition = m_linesTitle.relativePosition;
            m_depotsTitle.isVisible = false;

            this.m_Strip = Find<UITabstrip>("Tabstrip");

            this.m_Strip.relativePosition = new Vector3(13, 45);

            var ship = m_Strip.AddTab("");
            var plane = m_Strip.AddTab("");
            var bus = m_Strip.tabs[0].GetComponent<UIButton>();
            var tram = m_Strip.tabs[1].GetComponent<UIButton>();
            var metro = m_Strip.tabs[2].GetComponent<UIButton>();
            var train = m_Strip.tabs[3].GetComponent<UIButton>();

            var prefixEditor = m_Strip.AddTab("*");
            prefixEditor.textScale = 2.25f;
            prefixEditor.useOutline = true;

            var planeDepot = m_Strip.AddTab("");
            var shipDepot = m_Strip.AddTab("");
            var trainDepot = m_Strip.AddTab("");
            var metroDepot = m_Strip.AddTab("");
            var tramDepot = m_Strip.AddTab("");
            var busDepot = m_Strip.AddTab("");


            addIcon("PlaneLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref plane, false, 0, "TLM_PUBLICTRANSPORT_AIRPLANELINES");
            addIcon("ShipLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ship, false, 1, "TLM_PUBLICTRANSPORT_WATERLINES");
            addIcon("Train", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref train, false, 2, "PUBLICTRANSPORT_TRAINLINES");
            addIcon("Subway", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metro, false, 3, "PUBLICTRANSPORT_METROLINES");
            addIcon("Tram", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tram, false, 4, "PUBLICTRANSPORT_TRAMLINES");
            addIcon("Bus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref bus, false, 5, "PUBLICTRANSPORT_BUSLINES");

            addIcon("RoundSquare", "", ref prefixEditor, false, 6, "TLM_CITY_ASSETS_SELECTION");

            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref planeDepot, false, 7, "TLM_PUBLICTRANSPORT_HANGARS");
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref shipDepot, false, 8, "TLM_PUBLICTRANSPORT_HARBORS");
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref trainDepot, false, 9, "TLM_PUBLICTRANSPORT_TRAINDEPOTS");
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metroDepot, false, 10, "TLM_PUBLICTRANSPORT_METRODEPOTS");
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tramDepot, false, 11, "TLM_PUBLICTRANSPORT_TRAMDEPOTS");
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref busDepot, false, 12, "TLM_PUBLICTRANSPORT_BUSDEPOTS");


            tram.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Tram);

            this.m_BusLinesContainer = Find<UIComponent>("BusDetail").Find("Container");
            this.m_TramLinesContainer = Find<UIComponent>("TramDetail").Find("Container");
            this.m_MetroLinesContainer = Find<UIComponent>("MetroDetail").Find("Container");
            this.m_TrainLinesContainer = Find<UIComponent>("TrainDetail").Find("Container");

            m_BusLinesContainer.eventVisibilityChanged += null;
            m_TramLinesContainer.eventVisibilityChanged += null;
            m_MetroLinesContainer.eventVisibilityChanged += null;
            m_TrainLinesContainer.eventVisibilityChanged += null;

            CopyContainerFromBus(4, ref m_ShipLinesContainer);
            CopyContainerFromBus(5, ref m_PlaneLinesContainer);

            CopyContainerFromBus(6, ref m_PrefixEditor);

            CopyContainerFromBus(7, ref m_PlaneDepotsContainer);
            CopyContainerFromBus(8, ref m_ShipDepotsContainer);
            CopyContainerFromBus(9, ref m_TrainDepotsContainer);
            CopyContainerFromBus(10, ref m_MetroDepotsContainer);
            CopyContainerFromBus(11, ref m_TramDepotsContainer);
            CopyContainerFromBus(12, ref m_BusDepotsContainer);


            RemoveExtraLines(0, ref m_BusLinesContainer);
            RemoveExtraLines(0, ref m_TramLinesContainer);
            RemoveExtraLines(0, ref m_MetroLinesContainer);
            RemoveExtraLines(0, ref m_TrainLinesContainer);
            RemoveExtraLines(0, ref m_ShipLinesContainer);
            RemoveExtraLines(0, ref m_PlaneLinesContainer);
            RemoveExtraLines(0, ref m_PrefixEditor);
            RemoveExtraLines(0, ref m_BusDepotsContainer);
            RemoveExtraLines(0, ref m_TramDepotsContainer);
            RemoveExtraLines(0, ref m_MetroDepotsContainer);
            RemoveExtraLines(0, ref m_TrainDepotsContainer);
            RemoveExtraLines(0, ref m_ShipDepotsContainer);
            RemoveExtraLines(0, ref m_PlaneDepotsContainer);


            plane.zOrder = 0;
            ship.zOrder = (1);
            train.zOrder = (2);
            metro.zOrder = (3);
            tram.zOrder = (4);
            bus.zOrder = (5);

            m_PlaneLinesContainer.GetComponentInParent<UIPanel>().zOrder = (0);
            m_ShipLinesContainer.GetComponentInParent<UIPanel>().zOrder = (1);
            m_TrainLinesContainer.GetComponentInParent<UIPanel>().zOrder = (2);
            m_MetroLinesContainer.GetComponentInParent<UIPanel>().zOrder = (3);
            m_TramLinesContainer.GetComponentInParent<UIPanel>().zOrder = (4);
            m_BusLinesContainer.GetComponentInParent<UIPanel>().zOrder = (5);

            this.m_ToggleAllState = new bool[this.m_Strip.tabCount / 2];
            this.m_Strip.eventSelectedIndexChanged += null;
            this.m_Strip.eventSelectedIndexChanged += new PropertyChangedEventHandler<int>(this.OnTabChanged);
            this.m_ToggleAll = m_linesTitle.Find<UICheckBox>("ToggleAll");
            this.m_ToggleAll.eventCheckChanged += new PropertyChangedEventHandler<bool>(this.CheckChangedFunction);
            for (int i = 0; i < this.m_ToggleAllState.Length; i++)
            {
                this.m_ToggleAllState[i] = true;
            }
            m_linesTitle.Find<UIButton>("NameTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnNameSort();
            };
            m_linesTitle.Find<UIButton>("StopsTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnStopSort();
            };
            m_linesTitle.Find<UIButton>("VehiclesTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnVehicleSort();
            };
            m_linesTitle.Find<UIButton>("PassengersTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnPassengerSort();
            };
            var colorTitle = m_linesTitle.Find<UILabel>("ColorTitle");
            colorTitle.suffix = "/" + Locale.Get("TLM_CODE_SHORT");
            colorTitle.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnLineNumberSort();
            };

            this.m_LastSortCriterionLines = LineSortCriterion.DEFAULT;

            //Auto color & Auto Name
            TLMUtils.createUIElement<UIButton>(ref m_buttonAutoName, transform);
            m_buttonAutoName.pivot = UIPivotPoint.TopRight;
            m_buttonAutoName.text = Locale.Get("TLM_AUTO_NAME_ALL");
            m_buttonAutoName.textScale = 0.6f;
            m_buttonAutoName.width = 105;
            m_buttonAutoName.height = 15;
            m_buttonAutoName.tooltip = Locale.Get("TLM_AUTO_NAME_ALL_TOOLTIP");
            TLMUtils.initButton(m_buttonAutoName, true, "ButtonMenu");
            m_buttonAutoName.name = "AutoName";
            m_buttonAutoName.isVisible = true;
            m_buttonAutoName.eventClick += (component, eventParam) =>
            {
                OnAutoNameAll();
            };

            TLMUtils.createUIElement<UIButton>(ref m_buttonAutoColor, transform);
            m_buttonAutoColor.pivot = UIPivotPoint.TopRight;
            m_buttonAutoColor.text = Locale.Get("TLM_AUTO_COLOR_ALL");
            m_buttonAutoColor.textScale = 0.6f;
            m_buttonAutoColor.width = 105;
            m_buttonAutoColor.height = 15;
            m_buttonAutoColor.tooltip = Locale.Get("TLM_AUTO_COLOR_ALL_TOOLTIP");
            TLMUtils.initButton(m_buttonAutoColor, true, "ButtonMenu");
            m_buttonAutoColor.name = "AutoColor";
            m_buttonAutoColor.isVisible = true;
            m_buttonAutoColor.eventClick += (component, eventParam) =>
            {
                OnAutoColorAll();
            };

            //filters
            m_DayIcon = m_linesTitle.Find<UISprite>("DaySprite");
            m_NightIcon = m_linesTitle.Find<UISprite>("NightSprite");
            m_DayNightIcon = m_linesTitle.Find<UISprite>("DayNightSprite");
            m_DisabledIcon = GameObject.Instantiate(m_DayIcon.gameObject).GetComponent<UISprite>();
            m_DisabledIcon.transform.SetParent(m_DayIcon.transform.parent);
            m_NightIcon.relativePosition = new Vector3(670, 14);
            m_DayNightIcon.relativePosition = new Vector3(695, 14);
            m_DisabledIcon.spriteName = "Niet";

            m_DayIcon.tooltip = Locale.Get("TLM_DAY_FILTER_TOOLTIP");
            m_NightIcon.tooltip = Locale.Get("TLM_NIGHT_FILTER_TOOLTIP");
            m_DayNightIcon.tooltip = Locale.Get("TLM_DAY_NIGHT_FILTER_TOOLTIP");
            m_DisabledIcon.tooltip = Locale.Get("TLM_DISABLED_LINES_FILTER_TOOLTIP");

            m_DayIcon.eventClick += (x, y) =>
            {
                m_showDayLines = !m_showDayLines;
                m_DayIcon.color = m_showDayLines ? Color.white : Color.black;
            };
            m_NightIcon.eventClick += (x, y) =>
            {
                m_showNightLines = !m_showNightLines;
                m_NightIcon.color = m_showNightLines ? Color.white : Color.black;
            };
            m_DayNightIcon.eventClick += (x, y) =>
            {
                m_showDayNightLines = !m_showDayNightLines;
                m_DayNightIcon.color = m_showDayNightLines ? Color.white : Color.black;
            };
            m_DisabledIcon.eventClick += (x, y) =>
            {
                m_showDisabledLines = !m_showDisabledLines;
                m_DisabledIcon.color = m_showDisabledLines ? Color.white : Color.black;
            };

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

            m_DisabledIcon.relativePosition = new Vector3(736, 14);
            m_buttonAutoColor.relativePosition = new Vector3(655, 61);
            m_buttonAutoName.relativePosition = new Vector3(655, 43);

            var icon = Find<UISprite>("Icon");
            icon.spriteName = "TransportLinesManagerIconHovered";
            icon.atlas = TLMController.taTLM;

            var title = Find<UILabel>("Label");
            title.suffix = " - TLM v" + TransportLinesManagerMod.version;

            component.relativePosition = new Vector3(395, 58);

            //depot title

            GameObject.Destroy(m_depotsTitle.Find<UISprite>("DaySprite").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UISprite>("NightSprite").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UISprite>("DayNightSprite").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UICheckBox>("ToggleAll").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UIButton>("StopsTitle").gameObject);
            m_depotsTitle.Find<UILabel>("ColorTitle").text = Locale.Get("TUTORIAL_ADVISER_TITLE", "District");
            m_depotsTitle.Find<UILabel>("ColorTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnDepotDistrictSort();
            };
            m_depotsTitle.Find<UIButton>("NameTitle").text = "Station/Depot Name";
            m_depotsTitle.Find<UIButton>("NameTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnDepotNameSort();
            };
            m_depotsTitle.Find<UIButton>("VehiclesTitle").text = Locale.Get("TLM_PREFIXES_SERVED");
            m_depotsTitle.Find<UIButton>("VehiclesTitle").size += new Vector2(100, 0);
            m_depotsTitle.Find<UIButton>("PassengersTitle").text = Locale.Get("TLM_ADD_REMOVE");
            m_depotsTitle.Find<UIButton>("PassengersTitle").absolutePosition += new Vector3(100, 0);
            m_depotsTitle.Find<UIButton>("PassengersTitle").size += new Vector2(100, 0);

            //Prefix editor
            initPrefixEditor();

            m_Ready = true;
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



        private void addIcon(string namePrefix, string iconName, ref UIButton targetButton, bool alternativeIconAtlas, int tabIdx, string tooltipText, bool isTooltipLocale = true)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("addIcon: init " + namePrefix);

            TLMUtils.initButtonFg(targetButton, false, "");

            targetButton.atlas = TLMController.taLineNumber;
            targetButton.width = 40;
            targetButton.height = 40;
            targetButton.name = namePrefix + "Legend";
            TLMUtils.initButtonSameSprite(targetButton, namePrefix + "Icon");
            targetButton.color = new Color32(20, 20, 20, 255);
            targetButton.hoveredColor = Color.gray;
            targetButton.focusedColor = Color.green / 2;
            targetButton.eventClick += null;
            targetButton.eventClick += (x, y) =>
           {
               SetActiveTab(tabIdx);
           };
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("addIcon: pre eventClick");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("addIcon: init label icon");
            UILabel icon = targetButton.AddUIComponent<UILabel>();
            if (alternativeIconAtlas)
            {
                icon.atlas = TLMController.taLineNumber;
                icon.width = 27;
                icon.height = 27;
                icon.relativePosition = new Vector3(6f, 6);
            }
            else
            {
                icon.width = 30;
                icon.height = 20;
                icon.relativePosition = new Vector3(5f, 10f);
            }

            if (isTooltipLocale)
            {
                icon.tooltipLocaleID = tooltipText;
            }
            else
            {
                icon.tooltip = tooltipText;
            }

            icon.backgroundSprite = iconName;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("addIcon: end");
        }


        private void OnNameSort()
        {
            if (!isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareNames));
            this.m_LastSortCriterionLines = LineSortCriterion.NAME;
            uIComponent.Invalidate();
        }

        private void OnDepotNameSort()
        {
            if (!isDepotView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareDepotNames));
            // m_LastSortCriterionDepot = DepotSortCriterion.NAME;
            uIComponent.Invalidate();
        }

        private void OnDepotDistrictSort()
        {
            if (!isDepotView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareDepotDistricts));
            //m_LastSortCriterionDepot = DepotSortCriterion.DISTRICT;
            uIComponent.Invalidate();
        }

        private void OnStopSort()
        {
            if (!isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareStops));
            this.m_LastSortCriterionLines = LineSortCriterion.STOP;
            uIComponent.Invalidate();
        }

        private void OnVehicleSort()
        {
            if (!isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareVehicles));
            this.m_LastSortCriterionLines = LineSortCriterion.VEHICLE;
            uIComponent.Invalidate();
        }

        private void OnPassengerSort()
        {
            if (!isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(ComparePassengers));
            this.m_LastSortCriterionLines = LineSortCriterion.PASSENGER;
            uIComponent.Invalidate();
        }

        private void OnLineNumberSort()
        {
            if (!isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
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
            while (i <= num)
            {
                while (comp(elements[i], y) < 0)
                {
                    i++;
                }
                while (comp(elements[num], y) > 0)
                {
                    num--;
                }
                if (i <= num)
                {
                    UIComponent value = elements[i];
                    elements[i] = elements[num];
                    elements[i].forceZOrder = i;
                    elements[num] = value;
                    elements[num].forceZOrder = num;
                    i++;
                    num--;
                }
            }
            if (left < num)
            {
                Quicksort(elements, left, num, comp);
            }
            if (i < right)
            {
                Quicksort(elements, i, right, comp);
            }
        }

        public void SetActiveTab(int idx)
        {
            if (this.m_Strip.selectedIndex != idx)
            {
                this.m_Strip.selectedIndex = idx;
            }
        }

        public void RefreshLines()
        {
            if (Singleton<TransportManager>.exists)
            {
                int busCount = 0;
                int tramCount = 0;
                int metroCount = 0;
                int trainCount = 0;

                //TLM
                int shipCount = 0;
                int planeCount = 0;

                UIComponent comp;
                if (isLineView)
                {
                    for (ushort lineIdIterator = 1; lineIdIterator < 256; lineIdIterator += 1)
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdIterator].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created)
                        {
                            switch (TLMCW.getConfigIndexForLine(lineIdIterator))
                            {
                                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                                    comp = m_BusLinesContainer;
                                    busCount = AddToList(busCount, lineIdIterator, ref comp);
                                    break;
                                case TLMCW.ConfigIndex.TRAM_CONFIG:
                                    comp = m_TramLinesContainer;
                                    tramCount = AddToList(tramCount, lineIdIterator, ref comp);
                                    break;
                                case TLMCW.ConfigIndex.METRO_CONFIG:
                                    comp = m_MetroLinesContainer;
                                    metroCount = AddToList(metroCount, lineIdIterator, ref comp);
                                    break;
                                case TLMCW.ConfigIndex.TRAIN_CONFIG:
                                    comp = m_TrainLinesContainer;
                                    trainCount = AddToList(trainCount, lineIdIterator, ref comp);
                                    break;
                                case TLMCW.ConfigIndex.SHIP_CONFIG:
                                    comp = m_ShipLinesContainer;
                                    shipCount = AddToList(shipCount, lineIdIterator, ref comp);
                                    break;
                                case TLMCW.ConfigIndex.PLANE_CONFIG:
                                    comp = m_PlaneLinesContainer;
                                    planeCount = AddToList(planeCount, lineIdIterator, ref comp);
                                    break;
                            }
                        }
                    }
                    comp = m_BusLinesContainer;
                    RemoveExtraLines(busCount, ref comp);
                    comp = m_TramLinesContainer;
                    RemoveExtraLines(tramCount, ref comp);
                    comp = m_MetroLinesContainer;
                    RemoveExtraLines(metroCount, ref comp);
                    comp = m_TrainLinesContainer;
                    RemoveExtraLines(trainCount, ref comp);
                    comp = m_ShipLinesContainer;
                    RemoveExtraLines(shipCount, ref comp);
                    comp = m_PlaneLinesContainer;
                    RemoveExtraLines(planeCount, ref comp);

                    this.m_LinesUpdated = true;
                }
            }

            if (isDepotView && Singleton<BuildingManager>.exists)
            {
                int busCount = 0;
                int tramCount = 0;
                int metroCount = 0;
                int trainCount = 0;

                //TLM
                int shipCount = 0;
                int planeCount = 0;

                UIComponent comp;
                foreach (ushort buildingID in TLMDepotAI.getAllDepotsFromCity())
                {
                    switch ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI).m_transportInfo.m_transportType)
                    {
                        case TransportInfo.TransportType.Bus:
                            comp = m_BusDepotsContainer;
                            busCount = AddDepotToList(busCount, buildingID, ref comp);
                            break;
                        case TransportInfo.TransportType.Tram:
                            comp = m_TramDepotsContainer;
                            tramCount = AddDepotToList(tramCount, buildingID, ref comp);
                            break;
                        case TransportInfo.TransportType.Metro:
                            comp = m_MetroDepotsContainer;
                            metroCount = AddDepotToList(metroCount, buildingID, ref comp);
                            break;
                        case TransportInfo.TransportType.Train:
                            comp = m_TrainDepotsContainer;
                            trainCount = AddDepotToList(trainCount, buildingID, ref comp);
                            break;
                        case TransportInfo.TransportType.Ship:
                            comp = m_ShipDepotsContainer;
                            shipCount = AddDepotToList(shipCount, buildingID, ref comp);
                            break;
                        case TransportInfo.TransportType.Airplane:
                            comp = m_PlaneDepotsContainer;
                            planeCount = AddDepotToList(planeCount, buildingID, ref comp);
                            break;
                    }

                }
                comp = m_BusDepotsContainer;
                RemoveExtraLines(busCount, ref comp);
                comp = m_TramDepotsContainer;
                RemoveExtraLines(tramCount, ref comp);
                comp = m_MetroDepotsContainer;
                RemoveExtraLines(metroCount, ref comp);
                comp = m_TrainDepotsContainer;
                RemoveExtraLines(trainCount, ref comp);
                comp = m_ShipDepotsContainer;
                RemoveExtraLines(shipCount, ref comp);
                comp = m_PlaneDepotsContainer;
                RemoveExtraLines(planeCount, ref comp);

                this.m_LinesUpdated = true;
            }
        }

        private static void RemoveExtraLines(int linesCount, ref UIComponent component)
        {
            while (component.components.Count > linesCount)
            {
                UIComponent uIComponent = component.components[linesCount];
                component.RemoveUIComponent(uIComponent);
                UnityEngine.Object.Destroy(uIComponent.gameObject);
            }
        }

        private int AddToList(int count, ushort lineIdIterator, ref UIComponent component)
        {
            TLMPublicTransportLineInfo publicTransportLineInfo;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("PreIF");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Count = {0}; Component = {1}; components count = {2}", count, component.ToString(), component.components.Count);
            if (count >= component.components.Count)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("IF TRUE");
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                publicTransportLineInfo = temp.AddComponent<TLMPublicTransportLineInfo>();
                component.AttachUIComponent(publicTransportLineInfo.gameObject);
            }
            else
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("IF FALSE");
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("component.components[count] = {0};", component.components[count]);
                publicTransportLineInfo = component.components[count].GetComponent<TLMPublicTransportLineInfo>();
            }
            publicTransportLineInfo.lineID = lineIdIterator;
            publicTransportLineInfo.RefreshData(true, false);
            count++;
            return count;
        }

        private int AddDepotToList(int count, ushort buildingID, ref UIComponent component)
        {
            TLMPublicTransportDepotInfo publicTransportDepotInfo;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("PreIF");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Count = {0}; Component = {1}; components count = {2}", count, component.ToString(), component.components.Count);
            if (count >= component.components.Count)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("IF TRUE");
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                publicTransportDepotInfo = temp.AddComponent<TLMPublicTransportDepotInfo>();
                component.AttachUIComponent(publicTransportDepotInfo.gameObject);
            }
            else
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("IF FALSE");
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("component.components[count] = {0};", component.components[count]);
                publicTransportDepotInfo = component.components[count].GetComponent<TLMPublicTransportDepotInfo>();
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("publicTransportDepotInfo = {0};", publicTransportDepotInfo);
            }
            publicTransportDepotInfo.buildingId = buildingID;
            publicTransportDepotInfo.RefreshData();
            count++;
            return count;
        }

        bool isChangingTab;
        private void OnTabChanged(UIComponent c, int idx)
        {
            if (this.m_ToggleAll != null)
            {
                isChangingTab = true;
                this.m_ToggleAll.isChecked = idx < m_Strip.tabCount / 2 ? this.m_ToggleAllState[idx] : false;
                isChangingTab = false;
            }
            if (isDepotView || isLineView)
            {
                string[] filterOptions = TLMUtils.getFilterPrefixesOptions(tabSystemOrder[idx % (m_Strip.tabCount / 2)]);
                if (filterOptions.Length < 3)
                {
                    m_prefixFilter.isVisible = false;
                }
                else
                {
                    m_prefixFilter.isVisible = true;
                    m_prefixFilter.items = filterOptions;
                    m_prefixFilter.selectedIndex = 0;
                }
            }
            else
            {
                m_prefixFilter.isVisible = false;
            }

            m_depotsTitle.isVisible = isDepotView;
            m_linesTitle.isVisible = isLineView;
            m_buttonAutoName.isVisible = isLineView;
            m_buttonAutoColor.isVisible = isLineView;

            if (isDepotView)
            {
                m_depotsTitle.Find<UIButton>("NameTitle").text = string.Format(Locale.Get("TLM_DEPOT_NAME_PATTERN"), Locale.Get("TLM_PUBLICTRANSPORT_OF_DEPOT", currentSelectedSystem.ToString()));
                RefreshLines();
            }

            if (isPrefixEditor)
            {
                GetComponent<UIPanel>().height = 731;
            }

            m_depotsTitle.relativePosition = m_linesTitle.relativePosition;
            m_DisabledIcon.relativePosition = new Vector3(736, 14);
            this.OnLineNumberSort();
        }

        private void CheckChangedFunction(UIComponent c, bool r)
        {
            if (!isChangingTab)
            {
                this.OnChangeVisibleAll(r);
            }
        }

        private void OnChangeVisibleAll(bool visible)
        {
            if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count)
            {
                this.m_ToggleAllState[this.m_Strip.selectedIndex] = visible;
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container").Find("LinesContainer");
                if (uIComponent != null)
                {
                    for (int i = 0; i < uIComponent.components.Count; i++)
                    {
                        UIComponent uIComponent2 = uIComponent.components[i];
                        if (uIComponent2 != null)
                        {
                            UICheckBox uICheckBox = uIComponent2.Find<UICheckBox>("LineVisible");
                            if (uICheckBox)
                            {
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
            if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count)
            {
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
                if (uIComponent != null)
                {
                    for (int i = 0; i < uIComponent.components.Count; i++)
                    {
                        TLMPublicTransportLineInfo uIComponent2 = uIComponent.components[i].GetComponent<TLMPublicTransportLineInfo>();
                        if (uIComponent2 != null)
                        {
                            uIComponent2.DoAutoName();
                        }
                    }
                }
                this.RefreshLines();
            }
        }



        private void OnAutoColorAll()
        {
            if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count)
            {
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
                if (uIComponent != null)
                {
                    for (int i = 0; i < uIComponent.components.Count; i++)
                    {
                        TLMPublicTransportLineInfo uIComponent2 = uIComponent.components[i].GetComponent<TLMPublicTransportLineInfo>();
                        if (uIComponent2 != null)
                        {
                            uIComponent2.DoAutoColor();
                        }
                    }
                }
                this.RefreshLines();
            }
        }

        private void Update()
        {
            if (Singleton<TransportManager>.exists && this.m_Ready && this.m_LastLineCount != Singleton<TransportManager>.instance.m_lineCount)
            {
                this.RefreshLines();
                this.m_LastLineCount = Singleton<TransportManager>.instance.m_lineCount;
            }
            if (this.m_LinesUpdated)
            {
                this.m_LinesUpdated = false;
                if (this.m_LastSortCriterionLines != LineSortCriterion.DEFAULT)
                {
                    if (this.m_LastSortCriterionLines == LineSortCriterion.NAME)
                    {
                        this.OnNameSort();
                    }
                    else if (this.m_LastSortCriterionLines == LineSortCriterion.PASSENGER)
                    {
                        this.OnPassengerSort();
                    }
                    else if (this.m_LastSortCriterionLines == LineSortCriterion.STOP)
                    {
                        this.OnStopSort();
                    }
                    else if (this.m_LastSortCriterionLines == LineSortCriterion.VEHICLE)
                    {
                        this.OnVehicleSort();
                    }
                    else if (this.m_LastSortCriterionLines == LineSortCriterion.LINE_NUMBER)
                    {
                        this.OnLineNumberSort();
                    }
                }
                else
                {
                    this.OnLineNumberSort();
                }
            }
        }
        #region Asset Selection
        private void initPrefixEditor()
        {
            UIHelperExtension group2 = new UIHelperExtension(m_PrefixEditor);
            TLMUtils.doLog("INIT G2");
            ((UIScrollablePanel)group2.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group2.self).autoLayoutPadding = new RectOffset(5, 5, 0, 0);
            ((UIScrollablePanel)group2.self).wrapLayout = true;
            TLMUtils.doLog("INIT reloadTexture");
            OnTextChanged reloadTexture = delegate (string s)
            {
                //if (!string.IsNullOrEmpty(s))
                //{
                //    var prefab = PrefabCollection<VehicleInfo>.FindLoaded(s);
                //    if (prefab != null)
                //    {
                //        m_previewRenderer.mesh = prefab.m_mesh;
                //        m_previewRenderer.material = prefab.m_material;
                //        m_previewRenderer.Render();
                //        m_currentSelectionBus.texture = m_previewRenderer.texture;
                //    }
                //}
            };
            UIDropDown prefixSelection = null;
            TextList<string> defaultAssets = null;
            TextList<string> prefixAssets = null;
            UITextField prefixName = null;
            UISlider budgetMultiplier = null;
            UIHelperExtension group2sub = null;
            TLMUtils.doLog("INIT loadPrefixAssetList");
            OnDropdownSelectionChanged loadPrefixAssetList = (int sel) =>
            {
                if (sel == 0 || m_systemTypeDropDown.selectedIndex == 0)
                {
                    ((UIPanel)group2sub.self).enabled = false;
                    return;
                }
                ((UIPanel)group2sub.self).enabled = true;
                prefixName.text = getPrefixNameFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1));
                budgetMultiplier.value = getPrefixBudgetMultiplierFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1));
                budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL") + ": ";
                budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().autoSize = true;
                budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().wordWrap = false;
                budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().text = string.Format("x{0:0.00}", budgetMultiplier.value);
                prefixAssets.itemsList = getPrefixAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1));
                var t = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex);
                defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex).Where(k => !prefixAssets.itemsList.ContainsKey(k.Key)).ToDictionary(k => k.Key, k => k.Value);
            };
            TLMUtils.doLog("INIT loadPrefixes");
            OnDropdownSelectionChanged loadPrefixes = (int sel) =>
            {
                if (sel == 0)
                {
                    prefixSelection.isVisible = false;
                    prefixSelection.selectedIndex = 0;
                    return;
                }
                prefixSelection.isVisible = true;
                ((UIPanel)group2sub.self).enabled = false;
                TLMConfigWarehouse.ConfigIndex transportIndex = getConfigIndexFromDropDownSelection(sel);
                defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex);
                defaultAssets.root.color = TLMConfigWarehouse.getColorForTransportType(transportIndex);
                var m = (ModoNomenclatura)TLMConfigWarehouse.getCurrentConfigInt(transportIndex | TLMConfigWarehouse.ConfigIndex.PREFIX);
                prefixSelection.items = TLMUtils.getStringOptionsForPrefix(m, true);
                prefixSelection.selectedIndex = 0;
            };
            TLMUtils.doLog("INIT m_systemTypeDropDown");
            m_systemTypeDropDown = (UIDropDown)group2.AddDropdown(Locale.Get("TLM_TRANSPORT_SYSTEM"), new string[] { "--"+Locale.Get("SELECT")+"--",
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.BUS_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG) }, 0, loadPrefixes);
            prefixSelection = (UIDropDown)group2.AddDropdown(Locale.Get("TLM_PREFIX"), new string[] { "" }, 0, loadPrefixAssetList);

            foreach (Transform t in group2.self.transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }
            TLMUtils.doLog("INIT TLM_DETAILS");

            group2sub = group2.AddGroupExtended(Locale.Get("TLM_DETAILS"));

            ((UIPanel)group2sub.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group2sub.self).autoLayoutPadding = new RectOffset(2, 2, 0, 0);
            ((UIPanel)group2sub.self).wrapLayout = true;
            ((UIPanel)group2sub.self).width = 720;
            ((UIPanel)group2sub.self).padding = new RectOffset(0, 0, 0, 0);

            prefixName = group2sub.AddTextField(Locale.Get("TLM_PREFIX_NAME"), delegate (string s) { setPrefixNameDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), s); });
            budgetMultiplier = (UISlider)group2sub.AddSlider(Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL"), 0.25f, 5, 0.05f, 1, delegate (float f)
            {
                budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().text = string.Format("x{0:0.00}", f);
                setBudgetMultiplierDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), f);
            });

            defaultAssets = group2sub.AddTextList(Locale.Get("TLM_DEFAULT_ASSETS"), new Dictionary<string, string>(), delegate (string idx) { reloadTexture(idx); }, 340, 250);
            prefixAssets = group2sub.AddTextList(Locale.Get("TLM_ASSETS_FOR_PREFIX"), new Dictionary<string, string>(), delegate (string idx) { reloadTexture(idx); }, 340, 250);
            foreach (Transform t in ((UIPanel)group2sub.self).transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }

            prefixAssets.root.backgroundSprite = "EmptySprite";
            prefixAssets.root.color = Color.white;
            prefixAssets.root.width = 340;
            defaultAssets.root.backgroundSprite = "EmptySprite";
            defaultAssets.root.width = 340;

            prefixName.GetComponentInParent<UIPanel>().width = 300;
            prefixName.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
            prefixName.GetComponentInParent<UIPanel>().autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            prefixName.GetComponentInParent<UIPanel>().wrapLayout = true;

            budgetMultiplier.GetComponentInParent<UIPanel>().width = 300;
            budgetMultiplier.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
            budgetMultiplier.GetComponentInParent<UIPanel>().autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            budgetMultiplier.GetComponentInParent<UIPanel>().wrapLayout = true;
            group2sub.AddSpace(10);
            OnButtonClicked reload = delegate
            {
                loadPrefixAssetList(prefixSelection.selectedIndex);
            };
            group2sub.AddButton(Locale.Get("TLM_ADD"), delegate
            {
                if (defaultAssets.unselected) return;
                var selected = defaultAssets.selectedItem;
                if (selected == null || selected.Equals(default(string))) return;
                addAssetToPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), selected);
                reload();
            });
            group2sub.AddButton(Locale.Get("TLM_REMOVE"), delegate
            {
                if (prefixAssets.unselected) return;
                var selected = prefixAssets.selectedItem;
                if (selected == null || selected.Equals(default(string))) return;
                removeAssetFromPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), selected);
                reload();
            });

            group2sub.AddButton(Locale.Get("TLM_REMOVE_ALL"), delegate
            {
                removeAllAssetsFromPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1));
                reload();
            });
            group2sub.AddButton(Locale.Get("TLM_RELOAD"), delegate
            {
                reload();
            });
            ((UIPanel)group2sub.self).enabled = false;
            prefixSelection.isVisible = false;

        }



        private Dictionary<string, string> getBasicAssetListFromDropDownSelection(int index, bool global = false)
        {
            return TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).getBasicAssetsDictionary(global);

        }

        private Dictionary<string, string> getPrefixAssetListFromDropDownSelection(int index, uint prefix, bool global = false)
        {
            return TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).getBasicAssetsListForPrefix(prefix, global);
        }

        private void addAssetToPrefixDropDownSelection(int index, uint prefix, string assetId, bool global = false)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).addAssetToPrefixList(prefix, assetId, global);
        }

        private void removeAssetFromPrefixDropDownSelection(int index, uint prefix, string assetId, bool global = false)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).removeAssetFromPrefixList(prefix, assetId, global);
        }

        private void removeAllAssetsFromPrefixDropDownSelection(int index, uint prefix, bool global = false)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).removeAllAssetsFromPrefixList(prefix, global);
        }

        private void setPrefixNameDropDownSelection(int index, uint prefix, string name, bool global = false)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).setPrefixName(prefix, name, global);
        }

        private void setBudgetMultiplierDropDownSelection(int index, uint prefix, float value, bool global = false)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).setBudgetMultiplier(prefix, (uint)(value * 100), global);
        }
        private string getPrefixNameFromDropDownSelection(int index, uint prefix, bool global = false)
        {
            return TLMUtils.getTransportSystemPrefixName(getConfigIndexFromDropDownSelection(index), prefix, global);
        }
        private float getPrefixBudgetMultiplierFromDropDownSelection(int index, uint prefix, bool global = false)
        {
            return TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).getBudgetMultiplier(prefix, global) / 100f;
        }
        private TLMConfigWarehouse.ConfigIndex getConfigIndexFromDropDownSelection(int index)
        {
            switch (index)
            {
                case 1:
                    return TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG;
                case 2:
                    return TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG;
                case 3:
                    return TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG;
                case 4:
                    return TLMConfigWarehouse.ConfigIndex.BUS_CONFIG;
                case 5:
                    return TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG;
                default:
                    return TLMConfigWarehouse.ConfigIndex.NIL;
            }
        }
        #endregion


    }


}
