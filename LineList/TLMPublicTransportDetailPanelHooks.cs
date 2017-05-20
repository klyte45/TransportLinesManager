﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.VehicleAIExt;
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
            int idxRes = idx;
            switch (idx)
            {
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
                case 6:
                    idxRes = 5;
                    break;
            }

            publicTransportDetailPanel.SetActiveTab(idxRes);
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

        private const int NUM_TRANSPORT_SYSTEMS = 9;

        public static TLMPublicTransportDetailPanel instance;

        private static readonly string kLineTemplate = "LineTemplate";

        private int m_LastLineCount;

        private bool m_Ready;


        private bool m_LinesUpdated;

        private bool[] m_ToggleAllState;

        private LineSortCriterion m_LastSortCriterionLines;

        private UITabstrip m_Strip;

        private bool m_isDepotView = false;

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
            TLMCW.ConfigIndex.BUS_CONFIG,
            TLMCW.ConfigIndex.NIL,
        };
        
        private UIComponent m_PlaneLinesContainer;
        private UIComponent m_BlimpLinesContainer;
        private UIComponent m_ShipLinesContainer;
        private UIComponent m_FerryLinesContainer;
        private UIComponent m_TrainLinesContainer;
        private UIComponent m_MonorailLinesContainer;
        private UIComponent m_MetroLinesContainer;
        private UIComponent m_TramLinesContainer;
        private UIComponent m_BusLinesContainer;

        private UIComponent m_PrefixEditor;

        private UIComponent m_PlaneDepotsContainer;
        private UIComponent m_BlimpDepotsContainer;
        private UIComponent m_ShipDepotsContainer;
        private UIComponent m_FerryDepotsContainer;
        private UIComponent m_TrainDepotsContainer;
        private UIComponent m_MonorailDepotsContainer;
        private UIComponent m_MetroDepotsContainer;
        private UIComponent m_TramDepotsContainer;
        private UIComponent m_BusDepotsContainer;

        private UICheckBox m_ToggleAll;
        private UISprite m_DayIcon;
        private UISprite m_NightIcon;
        private UISprite m_DayNightIcon;
        private UISprite m_DisabledIcon;
        private UIDropDown m_prefixFilter;

        private UISprite m_depotIcon;

        private UIPanel m_linesTitle;
        private UIPanel m_depotsTitle;

        private UIButton m_buttonAutoName;
        private UIButton m_buttonAutoColor;
        private UIButton m_buttonRemoveUnwanted;
        private UIButton m_buttonDepotToggle;
        private UIButton m_buttonPrefixConfig;

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

        //asset editor
        TextList<string> m_defaultAssets;
        TextList<string> m_prefixAssets;
        UIDropDown m_prefixSelection;

        //per hour budget
        uint[] m_hourBudgets = new uint[8];
        UICheckBox m_chkSingleBudget = null;
        UICheckBox m_chkPerHourBudget = null;
        UISlider[] m_budgetSliders;
        bool m_isLoadingPrefixData;

        private bool m_isChangingTab;

        private UILabel m_LineCount;

        public UIDropDown m_systemTypeDropDown = null;


        private UITabstrip m_StripAsteriskTab;

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


        public bool m_isLineView
        {
            get
            {
                return !m_isDepotView && !m_isPrefixEditor;
            }
        }
        public bool m_isPrefixEditor
        {
            get
            {
                return m_Strip.selectedIndex == NUM_TRANSPORT_SYSTEMS * 2;
            }
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
            AwakeDepotTitleComponents();
            AwakePrefixEditor();

            m_Ready = true;
        }

        private void AwakeDepotTitleComponents()
        {
            //depot title
            m_depotsTitle = GameObject.Instantiate<UIPanel>(m_linesTitle);
            m_depotsTitle.transform.SetParent(m_linesTitle.transform.parent);
            m_depotsTitle.relativePosition = m_linesTitle.relativePosition;
            m_depotsTitle.isVisible = false;
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
            title.suffix = " - TLM v" + TransportLinesManagerMod.version;

            component.relativePosition = new Vector3(395, 58);
        }

        private void AwakeRearrangeTabs()
        {
            this.m_Strip = Find<UITabstrip>("Tabstrip");
            this.m_Strip.relativePosition = new Vector3(13, 45);

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Strips Lines");
            var bus = m_Strip.tabs[0].GetComponent<UIButton>();
            var tram = m_Strip.tabs[1].GetComponent<UIButton>();
            var metro = m_Strip.tabs[2].GetComponent<UIButton>();
            var train = m_Strip.tabs[3].GetComponent<UIButton>();
            var ferry = m_Strip.tabs[4].GetComponent<UIButton>();
            var blimp = m_Strip.tabs[5].GetComponent<UIButton>();
            var monorail = m_Strip.tabs[6].GetComponent<UIButton>();
            var ship = m_Strip.AddTab("");
            var plane = m_Strip.AddTab(""); // cable-car? -alborzka

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Mid Tab");
            var prefixEditor = m_Strip.AddTab("");
            prefixEditor.textScale = 2.25f;
            prefixEditor.useOutline = true;

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Strips Depots");

            var planeDepot = m_Strip.AddTab("");
            var blimpDepot = m_Strip.AddTab("");
            var shipDepot = m_Strip.AddTab("");
            var ferryDepot = m_Strip.AddTab("");
            var trainDepot = m_Strip.AddTab("");
            var monorailDepot = m_Strip.AddTab("");
            var metroDepot = m_Strip.AddTab("");
            var tramDepot = m_Strip.AddTab("");
            var busDepot = m_Strip.AddTab("");

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Tab init - lines");
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

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Tab init - depots");
            tabIt = 0;
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref planeDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref blimpDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref shipDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ferryDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref trainDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Monorail), ref monorailDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metroDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tramDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);
            addIcon("Depot", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref busDepot, false, NUM_TRANSPORT_SYSTEMS + tabIt++);

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Tab init - star");
            addIcon("Star", "", ref prefixEditor, false, NUM_TRANSPORT_SYSTEMS * 2);

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
            // planes and ships? -alborzka

            m_BusLinesContainer.eventVisibilityChanged += null;
            m_TramLinesContainer.eventVisibilityChanged += null;
            m_MetroLinesContainer.eventVisibilityChanged += null;
            m_TrainLinesContainer.eventVisibilityChanged += null;
            m_BlimpLinesContainer.eventVisibilityChanged += null;
            m_MonorailLinesContainer.eventVisibilityChanged += null;
            m_FerryLinesContainer.eventVisibilityChanged += null;
            // planes and ships? -alborzka
            
            CopyContainerFromBus(NUM_TRANSPORT_SYSTEMS - 2, ref m_ShipLinesContainer);
            CopyContainerFromBus(NUM_TRANSPORT_SYSTEMS - 1, ref m_PlaneLinesContainer);

            tabIt = NUM_TRANSPORT_SYSTEMS;
            CopyContainerFromBus(tabIt++, ref m_PlaneDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_BlimpDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_ShipDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_FerryDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_TrainDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_MonorailDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_MetroDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_TramDepotsContainer);
            CopyContainerFromBus(tabIt++, ref m_BusDepotsContainer);

            CopyContainerFromBus(tabIt, ref m_PrefixEditor);

            RemoveExtraLines(0, ref m_BusLinesContainer);
            RemoveExtraLines(0, ref m_TramLinesContainer);
            RemoveExtraLines(0, ref m_MetroLinesContainer);
            RemoveExtraLines(0, ref m_TrainLinesContainer);
            RemoveExtraLines(0, ref m_MonorailLinesContainer);
            RemoveExtraLines(0, ref m_ShipLinesContainer);
            RemoveExtraLines(0, ref m_PlaneLinesContainer);
            RemoveExtraLines(0, ref m_PrefixEditor);
            RemoveExtraLines(0, ref m_BusDepotsContainer);
            RemoveExtraLines(0, ref m_TramDepotsContainer);
            RemoveExtraLines(0, ref m_MetroDepotsContainer);
            RemoveExtraLines(0, ref m_TrainDepotsContainer);
            RemoveExtraLines(0, ref m_ShipDepotsContainer);
            RemoveExtraLines(0, ref m_PlaneDepotsContainer);
            RemoveExtraLines(0, ref m_MonorailDepotsContainer);
            RemoveExtraLines(0, ref m_FerryLinesContainer);
            RemoveExtraLines(0, ref m_BlimpLinesContainer);
            RemoveExtraLines(0, ref m_BlimpDepotsContainer);
            RemoveExtraLines(0, ref m_FerryDepotsContainer);

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
            m_linesTitle = Find<UIPanel>("LineTitle");
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
            m_buttonAutoName.eventClick += (component, eventParam) =>
            {
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
            m_buttonAutoColor.eventClick += (component, eventParam) =>
            {
                OnAutoColorAll();
            };

            icon = m_buttonAutoColor.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "AutoColorIcon";

            TLMUtils.createUIElement<UIButton>(ref m_buttonRemoveUnwanted, transform);
            m_buttonRemoveUnwanted.pivot = UIPivotPoint.TopRight;
            m_buttonRemoveUnwanted.textScale = 0.6f;
            m_buttonRemoveUnwanted.width = 40;
            m_buttonRemoveUnwanted.height = 40;
            m_buttonRemoveUnwanted.tooltip = Locale.Get("TLM_REMOVE_UNWANTED_TOOLTIP");
            TLMUtils.initButton(m_buttonRemoveUnwanted, true, "ButtonMenu");
            m_buttonRemoveUnwanted.name = "RemoveUnwanted";
            m_buttonRemoveUnwanted.isVisible = true;
            m_buttonRemoveUnwanted.eventClick += (component, eventParam) =>
            {
                OnRemoveUnwanted();
            };

            icon = m_buttonRemoveUnwanted.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "RemoveUnwantedIcon";

            TLMUtils.createUIElement<UIButton>(ref m_buttonDepotToggle, transform);
            m_buttonDepotToggle.pivot = UIPivotPoint.TopRight;
            m_buttonDepotToggle.textScale = 0.6f;
            m_buttonDepotToggle.width = 40;
            m_buttonDepotToggle.height = 40;
            m_buttonDepotToggle.tooltip = Locale.Get("TLM_TOGGLE_LINES_DEPOT_TOOLTIP");
            TLMUtils.initButton(m_buttonDepotToggle, true, "ButtonMenu");
            m_buttonDepotToggle.name = "DepotToggleButton";
            m_buttonDepotToggle.isVisible = true;
            m_buttonDepotToggle.eventClick += (component, eventParam) =>
            {
                toggleDepotView();
            };

            m_depotIcon = m_buttonDepotToggle.AddUIComponent<UISprite>();
            m_depotIcon.relativePosition = new Vector3(2, 2);
            m_depotIcon.atlas = TLMController.taLineNumber;
            m_depotIcon.width = 36;
            m_depotIcon.height = 36;
            m_depotIcon.spriteName = "DepotIcon";

            TLMUtils.createUIElement<UIButton>(ref m_buttonPrefixConfig, transform);
            m_buttonPrefixConfig.pivot = UIPivotPoint.TopRight;
            m_buttonPrefixConfig.textScale = 0.6f;
            m_buttonPrefixConfig.width = 40;
            m_buttonPrefixConfig.height = 40;
            m_buttonPrefixConfig.tooltip = Locale.Get("TLM_CITY_ASSETS_SELECTION");
            TLMUtils.initButton(m_buttonPrefixConfig, true, "ButtonMenu");
            m_buttonPrefixConfig.name = "DepotToggleButton";
            m_buttonPrefixConfig.isVisible = true;
            m_buttonPrefixConfig.eventClick += (component, eventParam) =>
            {
                SetActiveTab(NUM_TRANSPORT_SYSTEMS * 2);
            };

            icon = m_buttonPrefixConfig.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "ConfigIcon";

            m_buttonDepotToggle.relativePosition = new Vector3(540, 43);
            m_buttonPrefixConfig.relativePosition = new Vector3(585, 43);
            m_buttonRemoveUnwanted.relativePosition = new Vector3(630, 43);
            m_buttonAutoColor.relativePosition = new Vector3(675, 43);
            m_buttonAutoName.relativePosition = new Vector3(720, 43);
        }

        private void AwakeDayNightOptions()
        {
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
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("addIcon: init " + namePrefix);

            TLMUtils.initButtonFg(targetButton, false, "");

            targetButton.atlas = TLMController.taLineNumber;
            if (tooltipText == "")
            {
                targetButton.width = 01;
                targetButton.height = 01;
                TLMUtils.initButtonSameSprite(targetButton, "");
                targetButton.isVisible = false;
            }
            else {
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
            }
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

        #region Sorting

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
        private void OnNameSort()
        {
            if (!m_isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareNames));
            this.m_LastSortCriterionLines = LineSortCriterion.NAME;
            uIComponent.Invalidate();
        }

        private void OnDepotNameSort()
        {
            if (!m_isDepotView || m_isPrefixEditor) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareDepotNames));
            // m_LastSortCriterionDepot = DepotSortCriterion.NAME;
            uIComponent.Invalidate();
        }

        private void OnDepotDistrictSort()
        {
            if (!m_isDepotView || m_isPrefixEditor) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareDepotDistricts));
            //m_LastSortCriterionDepot = DepotSortCriterion.DISTRICT;
            uIComponent.Invalidate();
        }

        private void OnStopSort()
        {
            if (!m_isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareStops));
            this.m_LastSortCriterionLines = LineSortCriterion.STOP;
            uIComponent.Invalidate();
        }

        private void OnVehicleSort()
        {
            if (!m_isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareVehicles));
            this.m_LastSortCriterionLines = LineSortCriterion.VEHICLE;
            uIComponent.Invalidate();
        }

        private void OnPassengerSort()
        {
            if (!m_isLineView) return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0) return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(ComparePassengers));
            this.m_LastSortCriterionLines = LineSortCriterion.PASSENGER;
            uIComponent.Invalidate();
        }

        private void OnLineNumberSort()
        {
            if (!m_isLineView) return;
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
        #endregion

        public void SetActiveTab(int idx)
        {
            var selIdx = idx;
            if (this.m_isDepotView || idx == NUM_TRANSPORT_SYSTEMS * 2)
            {
                selIdx += NUM_TRANSPORT_SYSTEMS;
            }

            if (this.m_Strip.selectedIndex != selIdx)
            {
                this.m_Strip.selectedIndex = selIdx;
                m_Strip.tabs[idx].GetComponentInChildren<UIButton>().state = UIButton.ButtonState.Focused;
                RefreshLines();
            }
        }


        public void toggleDepotView()
        {
            this.m_isDepotView = !this.m_isDepotView;

            if (m_isDepotView)
            {
                m_buttonDepotToggle.tooltip = Locale.Get("TLM_LIST_LINES_TOOLTIP");
                m_depotIcon.spriteName = "BusIcon";
            }
            else
            {
                m_buttonDepotToggle.tooltip = Locale.Get("TLM_LIST_DEPOT_TOOLTIP");
                m_depotIcon.spriteName = "DepotIcon";
            }

            if (this.m_Strip.selectedIndex != NUM_TRANSPORT_SYSTEMS * 2)
            {
                SetActiveTab(this.m_Strip.selectedIndex % NUM_TRANSPORT_SYSTEMS);
            }
        }

        public void RefreshLines()
        {
            if (Singleton<TransportManager>.exists)
            {
                UIComponent comp;
                if (m_isLineView)
                {
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

                    for (ushort lineIdIterator = 1; lineIdIterator < 256; lineIdIterator += 1)
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdIterator].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created)
                        {
                            switch (TLMCW.getDefinitionForLine(lineIdIterator).toConfigIndex())
                            {
                                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
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

            if (m_isDepotView && Singleton<BuildingManager>.exists)
            {
                int busCount = 0;
                int tramCount = 0;
                int metroCount = 0;
                int trainCount = 0;
                int shipCount = 0;
                int planeCount = 0;
                int monorailCount = 0;
                int blimpCount = 0;
                int ferryCount = 0;

                UIComponent comp;
                foreach (ushort buildingID in TLMDepotAI.getAllDepotsFromCity())
                {
                    switch (TransportSystemDefinition.from((Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI).m_transportInfo).toConfigIndex())
                    {
                        case TLMCW.ConfigIndex.BUS_CONFIG:
                            comp = m_BusDepotsContainer;
                            busCount = AddDepotToList(busCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.TRAM_CONFIG:
                            comp = m_TramDepotsContainer;
                            tramCount = AddDepotToList(tramCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.METRO_CONFIG:
                            comp = m_MetroDepotsContainer;
                            metroCount = AddDepotToList(metroCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.TRAIN_CONFIG:
                            comp = m_TrainDepotsContainer;
                            trainCount = AddDepotToList(trainCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.SHIP_CONFIG:
                            comp = m_ShipDepotsContainer;
                            shipCount = AddDepotToList(shipCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.PLANE_CONFIG:
                            comp = m_PlaneDepotsContainer;
                            planeCount = AddDepotToList(planeCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.MONORAIL_CONFIG:
                            comp = m_MonorailDepotsContainer;
                            monorailCount = AddDepotToList(monorailCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.BLIMP_CONFIG:
                            comp = m_BlimpDepotsContainer;
                            blimpCount = AddDepotToList(blimpCount, buildingID, ref comp);
                            break;
                        case TLMCW.ConfigIndex.FERRY_CONFIG:
                            comp = m_FerryDepotsContainer;
                            ferryCount = AddDepotToList(ferryCount, buildingID, ref comp);
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
                comp = m_MonorailDepotsContainer;
                RemoveExtraLines(monorailCount, ref comp);
                comp = m_FerryDepotsContainer;
                RemoveExtraLines(ferryCount, ref comp);
                comp = m_BlimpDepotsContainer;
                RemoveExtraLines(blimpCount, ref comp);

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

        private void OnTabChanged(UIComponent c, int idx)
        {
            if (this.m_ToggleAll != null)
            {
                m_isChangingTab = true;
                this.m_ToggleAll.isChecked = idx < this.m_ToggleAllState.Length ? this.m_ToggleAllState[idx] : false;
                m_isChangingTab = false;
            }
            if (!m_isPrefixEditor)
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

            m_depotsTitle.isVisible = m_isDepotView && !m_isPrefixEditor;
            m_linesTitle.isVisible = m_isLineView;
            m_buttonAutoName.isVisible = m_isLineView;
            m_buttonAutoColor.isVisible = m_isLineView;
            m_buttonRemoveUnwanted.isVisible = !m_isDepotView;

            if (m_isDepotView && !m_isPrefixEditor)
            {
                m_depotsTitle.Find<UIButton>("NameTitle").text = string.Format(Locale.Get("TLM_DEPOT_NAME_PATTERN"), Locale.Get("TLM_PUBLICTRANSPORT_OF_DEPOT", currentSelectedSystem.ToString()));
                RefreshLines();
            }

            if (m_isPrefixEditor)
            {
                GetComponent<UIPanel>().height = 910;
            }

            m_depotsTitle.relativePosition = m_linesTitle.relativePosition;
            m_DisabledIcon.relativePosition = new Vector3(736, 14);
            this.OnLineNumberSort();
            RefreshLineCount(m_Strip.selectedIndex);
        }

        private void CheckChangedFunction(UIComponent c, bool r)
        {
            if (!m_isChangingTab)
            {
                this.OnChangeVisibleAll(r);
            }
        }

        private void OnChangeVisibleAll(bool visible)
        {
            if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count)
            {
                this.m_ToggleAllState[this.m_Strip.selectedIndex] = visible;
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
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

        private void OnRemoveUnwanted()
        {
            BasicTransportExtension.removeAllUnwantedVehicles();
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
                RefreshLineCount(m_Strip.selectedIndex);
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

        private void reloadAssetsList(int idx)
        {
            //if (true)
            //{
            m_prefixAssets.itemsList = getPrefixAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(idx - 1));
            var t = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex);
            m_defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex).Where(k => !m_prefixAssets.itemsList.ContainsKey(k.Key)).ToDictionary(k => k.Key, k => k.Value);
            m_StripAsteriskTab.EnableTab(1);
            //}
            //else
            //{
            //    m_StripAsteriskTab.DisableTab(1);
            //}
        }

        private void AwakePrefixEditor()
        {
            UIHelperExtension group2 = new UIHelperExtension(m_PrefixEditor);


            TLMUtils.doLog("INIT G2");
            ((UIScrollablePanel)group2.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group2.self).autoLayoutPadding = new RectOffset(5, 5, 0, 0);
            ((UIScrollablePanel)group2.self).wrapLayout = true;
            ((UIScrollablePanel)group2.self).autoLayout = true;
            TLMUtils.doLog("INIT reloadTexture");
            UITextField prefixName = null;
            UITextField ticketPrice = null;
            TLMUtils.doLog("INIT loadPrefixAssetList");
            OnDropdownSelectionChanged selectPrefixAction = (int sel) =>
            {
                m_isLoadingPrefixData = true;
                if (sel == 0 || m_systemTypeDropDown.selectedIndex == 0)
                {
                    ((UIScrollablePanel)group2.self).autoLayout = false;
                    m_StripAsteriskTab.tabPages.enabled = false;

                    m_StripAsteriskTab.enabled = false;
                    return;
                }
                prefixName.text = getPrefixNameFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1));
                var hourBudgetsSaved = getPrefixBudgetMultiplierFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1));
                m_chkPerHourBudget.isChecked = hourBudgetsSaved.Length == 8;
                m_chkSingleBudget.isChecked = hourBudgetsSaved.Length == 1;
                for (int i = 0; i < 8; i++)
                {
                    m_hourBudgets[i] = hourBudgetsSaved[i % hourBudgetsSaved.Length];
                }
                updateBudgetSliders();
                ticketPrice.text = (getTicketPriceFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1))).ToString();
                reloadAssetsList(sel);
                m_StripAsteriskTab.tabPages.enabled = true;
                m_StripAsteriskTab.enabled = true;
                m_StripAsteriskTab.selectedIndex = 0;
                m_isLoadingPrefixData = false;
            };
            TLMUtils.doLog("INIT loadPrefixes");
            OnDropdownSelectionChanged loadPrefixes = (int sel) =>
            {
                if (sel == 0)
                {
                    m_prefixSelection.isVisible = false;
                    m_prefixSelection.selectedIndex = 0;
                    m_StripAsteriskTab.tabPages.enabled = false;
                    return;
                }
                m_prefixSelection.isVisible = true;
                m_StripAsteriskTab.tabPages.enabled = false;
                TLMConfigWarehouse.ConfigIndex transportIndex = getConfigIndexFromDropDownSelection(sel);
                m_defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex);
                m_defaultAssets.root.color = TLMConfigWarehouse.getColorForTransportType(transportIndex);
                var m = (ModoNomenclatura)TLMConfigWarehouse.getCurrentConfigInt(transportIndex | TLMConfigWarehouse.ConfigIndex.PREFIX);
                m_prefixSelection.items = TLMUtils.getStringOptionsForPrefix(m, true);
                m_prefixSelection.selectedIndex = 0;
            };
            TLMUtils.doLog("INIT m_systemTypeDropDown");
            m_systemTypeDropDown = (UIDropDown)group2.AddDropdown(Locale.Get("TLM_TRANSPORT_SYSTEM"), new string[] { "--"+Locale.Get("SELECT")+"--",
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.BUS_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.METRO_CONFIG),
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG) ,
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG) ,
                    TLMConfigWarehouse.getNameForTransportType(TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG) }, 0, loadPrefixes);
            m_prefixSelection = (UIDropDown)group2.AddDropdown(Locale.Get("TLM_PREFIX"), new string[] { "" }, 0, selectPrefixAction);


            foreach (Transform t in group2.self.transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }


            TLMUtils.doLog("INIT TLM_TABS");
            m_StripAsteriskTab = group2.self.AddUIComponent<UITabstrip>();
            m_StripAsteriskTab.width = 840;
            m_StripAsteriskTab.height = 50;

            m_StripAsteriskTab.tabPages = group2.self.AddUIComponent<UITabContainer>(); ;
            m_StripAsteriskTab.tabPages.width = 840;
            m_StripAsteriskTab.tabPages.height = 630;

            UIHelperExtension detailsTabContainer = createNewAsteriskTab(Locale.Get("TLM_DETAILS"));
            prefixName = detailsTabContainer.AddTextField(Locale.Get("TLM_PREFIX_NAME"), delegate (string s) { setPrefixNameDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), s); });
            ticketPrice = detailsTabContainer.AddTextField(Locale.Get("TLM_TICKET_PRICE_LABEL"), delegate (string s)
            {
                uint f = uint.Parse("0" + s);
                setTicketPriceDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), f);
            });
            prefixName.GetComponentInParent<UIPanel>().width = 300;
            prefixName.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
            prefixName.GetComponentInParent<UIPanel>().autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            prefixName.GetComponentInParent<UIPanel>().wrapLayout = true;

            ticketPrice.numericalOnly = true;
            ticketPrice.maxLength = 7;

            foreach (Transform t in ((UIPanel)detailsTabContainer.self).transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }

            UIHelperExtension assetSelectionTabContainer = createNewAsteriskTab(Locale.Get("TLM_CITY_ASSETS_SELECTION"));
            m_defaultAssets = assetSelectionTabContainer.AddTextList(Locale.Get("TLM_DEFAULT_ASSETS"), new Dictionary<string, string>(), delegate (string idx) { }, 340, 250);
            m_prefixAssets = assetSelectionTabContainer.AddTextList(Locale.Get("TLM_ASSETS_FOR_PREFIX"), new Dictionary<string, string>(), delegate (string idx) { }, 340, 250);
            foreach (Transform t in ((UIPanel)assetSelectionTabContainer.self).transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }

            m_prefixAssets.root.backgroundSprite = "EmptySprite";
            m_prefixAssets.root.color = Color.white;
            m_prefixAssets.root.width = 340;
            m_defaultAssets.root.backgroundSprite = "EmptySprite";
            m_defaultAssets.root.width = 340;
            assetSelectionTabContainer.AddSpace(10);
            OnButtonClicked reload = delegate
            {
                reloadAssetsList(m_prefixSelection.selectedIndex);
            };
            assetSelectionTabContainer.AddButton(Locale.Get("TLM_ADD"), delegate
            {
                if (m_defaultAssets.unselected) return;
                var selected = m_defaultAssets.selectedItem;
                if (selected == null || selected.Equals(default(string))) return;
                addAssetToPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), selected);
                reload();
            });
            assetSelectionTabContainer.AddButton(Locale.Get("TLM_REMOVE"), delegate
            {
                if (m_prefixAssets.unselected) return;
                var selected = m_prefixAssets.selectedItem;
                if (selected == null || selected.Equals(default(string))) return;
                removeAssetFromPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), selected);
                reload();
            });

            assetSelectionTabContainer.AddButton(Locale.Get("TLM_REMOVE_ALL"), delegate
            {
                removeAllAssetsFromPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1));
                reload();
            });
            assetSelectionTabContainer.AddButton(Locale.Get("TLM_RELOAD"), delegate
            {
                reload();
            });

            UIHelperExtension perPeriodBudgetContainer = createNewAsteriskTab(Locale.Get("TLM_PREFIX_BUDGET"));
            m_budgetSliders = new UISlider[8];
            m_chkPerHourBudget = (UICheckBox)perPeriodBudgetContainer.AddCheckbox(Locale.Get("TLM_USE_PER_PERIOD_BUDGET"), false, delegate (bool val)
            {
                for (int i = 0; i < 8; i++)
                {
                    m_hourBudgets[i] = m_hourBudgets[0];
                }
                updateBudgetSliders();
            });
            m_chkSingleBudget = (UICheckBox)perPeriodBudgetContainer.AddCheckbox(Locale.Get("TLM_USE_SINGLE_BUDGET"), true, delegate (bool val) { updateBudgetSliders(); });
            m_chkPerHourBudget.group = m_chkPerHourBudget.parent;
            m_chkSingleBudget.group = m_chkPerHourBudget.parent;
            for (int i = 0; i < 8; i++)
            {
                var j = i;
                m_budgetSliders[i] = GenerateBudgetMultiplierField(perPeriodBudgetContainer, Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL", i) + ":", delegate (float f)
                {
                    m_budgetSliders[j].transform.parent.GetComponentInChildren<UILabel>().text = string.Format(" x{0:0.00}", f);
                    if (!m_isLoadingPrefixData)
                    {
                        m_hourBudgets[j] = (uint)(f * 100);
                        setBudgetMultiplierDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1));
                    }
                });
            }

            //------
            m_prefixSelection.isVisible = false;
            m_StripAsteriskTab.tabPages.enabled = false;
            m_StripAsteriskTab.enabled = false;
        }

        private void updateBudgetSliders()
        {
            if (m_chkSingleBudget.isChecked)
            {
                m_budgetSliders[0].parent.GetComponentInChildren<UILabel>().prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL") + ":";
            }
            else
            {
                m_budgetSliders[0].parent.GetComponentInChildren<UILabel>().prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL", 0) + ":";
            }
            for (int i = 0; i < 8; i++)
            {
                m_budgetSliders[i].parent.isVisible = i == 0 || !m_chkSingleBudget.isChecked;
                m_budgetSliders[i].value = m_hourBudgets[i] / 100f;
                m_budgetSliders[i].transform.parent.GetComponentInChildren<UILabel>().text = string.Format(" x{0:0.00}", m_budgetSliders[i].value);
            }
        }

        private UIHelperExtension createNewAsteriskTab(string title)
        {
            formatTabButton(m_StripAsteriskTab.AddTab(title));
            UIHelperExtension newTab = new UIHelperExtension(m_StripAsteriskTab.tabContainer.components[m_StripAsteriskTab.tabContainer.components.Count - 1]);
            ((UIPanel)newTab.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)newTab.self).autoLayoutPadding = new RectOffset(2, 2, 0, 0);
            ((UIPanel)newTab.self).wrapLayout = true;
            ((UIPanel)newTab.self).autoSize = true;
            ((UIPanel)newTab.self).autoLayout = true;
            ((UIPanel)newTab.self).width = 680;
            ((UIPanel)newTab.self).isVisible = false;
            ((UIPanel)newTab.self).padding = new RectOffset(0, 0, 0, 0);
            return newTab;
        }

        private UISlider GenerateBudgetMultiplierField(UIHelperExtension uiHelper, string title, OnValueChanged action)
        {
            UISlider budgetMultiplier = (UISlider)uiHelper.AddSlider(Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL"), 0f, 5, 0.05f, 1, action);
            budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().prefix = title;
            budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().autoSize = true;
            budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().wordWrap = false;
            budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>().text = string.Format(" x{0:0.00}", 0);
            budgetMultiplier.GetComponentInParent<UIPanel>().width = 300;
            budgetMultiplier.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
            budgetMultiplier.GetComponentInParent<UIPanel>().autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            budgetMultiplier.GetComponentInParent<UIPanel>().wrapLayout = true;
            return budgetMultiplier;
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

        #region Asset Selection & details functions

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

        private void setBudgetMultiplierDropDownSelection(int index, uint prefix, bool global = false)
        {
            uint[] saveData;
            if (m_chkSingleBudget.isChecked)
            {
                saveData = new uint[] { m_hourBudgets[0] };
            }
            else
            {
                saveData = m_hourBudgets;
            }
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).setBudgetMultiplier(prefix, saveData, global);
        }
        private void setTicketPriceDropDownSelection(int index, uint prefix, uint value, bool global = false)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).setTicketPrice(prefix, value, global);
        }
        private string getPrefixNameFromDropDownSelection(int index, uint prefix, bool global = false)
        {
            return TLMUtils.getTransportSystemPrefixName(getConfigIndexFromDropDownSelection(index), prefix, global);
        }
        private uint[] getPrefixBudgetMultiplierFromDropDownSelection(int index, uint prefix, bool global = false)
        {
            return TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).getBudgetsMultiplier(prefix, global);
        }
        private uint getTicketPriceFromDropDownSelection(int index, uint prefix, bool global = false)
        {
            return TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).getTicketPrice(prefix, global);
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
                case 6:
                    return TLMConfigWarehouse.ConfigIndex.METRO_CONFIG;
                case 7:
                    return TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG;
                case 8:
                    return TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG;
                case 9:
                    return TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG;
                default:
                    return TLMConfigWarehouse.ConfigIndex.NIL;
            }
        }
        #endregion


        private void RefreshLineCount(int transportTabIndex)
        {
            if (m_isLineView)
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
            else
            {
                this.m_LineCount.text = "";
            }
        }
    }
}
