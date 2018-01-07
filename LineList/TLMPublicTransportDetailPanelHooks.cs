using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.VehicleAIExt;
using Klyte.TransportLinesManager.LineList.ExtraUI;
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
    class TLMPublicTransportDetailPanelHooks : Redirector<TLMPublicTransportDetailPanelHooks>
    {
        private bool panelOverriden = false;
        private int tryCount = 0;

        private static void OpenDetailPanel(int idx)
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



        public override void EnableHooks()
        {
            MethodInfo preventDefault = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("preventDefault", allFlags);
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
            AddRedirect(typeof(PublicTransportDetailPanel).GetMethod("SetActiveTab", allFlags), preventDefault);

            TLMUtils.doLog("Loading PublicTransportLineInfo Hooks!");
            AddRedirect(typeof(PublicTransportLineInfo).GetMethod("RefreshData", allFlags), preventDefault);

            TLMUtils.doLog("Loading PublicTransportInfoViewPanel Hooks!");
            AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), preventDefault, OpenDetailPanel);

            TLMUtils.doLog("Remove PublicTransportDetailPanel Hooks!");
            update();
        }

        public void update()
        {
            if (tryCount < 100 && !panelOverriden)
            {
                try
                {
                    var go = GameObject.Find("(Library) PublicTransportDetailPanel");
                    GameObject.Destroy(go.GetComponentInChildren<PublicTransportDetailPanel>());
                    TLMPublicTransportDetailPanel.instance = go.AddComponent<TLMPublicTransportDetailPanel>();
                    panelOverriden = true;
                }
                catch (Exception e)
                {
                    tryCount++;
                    TLMUtils.doLog("Failed to load panel. Trying again a " + tryCount + getOrdinal(tryCount) + " time next frame:\n{0}", e);
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

        private const int NUM_TRANSPORT_SYSTEMS = 10;

        public static TLMPublicTransportDetailPanel instance;

        private static readonly string kLineTemplate = "LineTemplate";

        private int m_LastLineCount;

        private bool m_Ready;


        private bool m_LinesUpdated;

        private bool[] m_ToggleAllState;

        private LineSortCriterion m_LastSortCriterionLines;

        private UITabstrip m_Strip;

        private bool m_isDepotView = true;

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
            TLMCW.ConfigIndex.EVAC_BUS_CONFIG
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
        private UIComponent m_EvacLinesContainer;


        private UIComponent m_PlaneDepotsContainer;
        private UIComponent m_TramDepotsContainer;
        private UIComponent m_MetroDepotsContainer;
        private UIComponent m_TrainDepotsContainer;
        private UIComponent m_ShipDepotsContainer;
        private UIComponent m_MonorailDepotsContainer;
        private UIComponent m_BusDepotsContainer;
        private UIComponent m_BlimpDepotsContainer;
        private UIComponent m_FerryDepotsContainer;
        private UIComponent m_EvacDepotsContainer;

        private UICheckBox m_ToggleAll;
        private UIButton m_DayIcon;
        private UIButton m_NightIcon;
        private UIButton m_DayNightIcon;
        private UIButton m_DisabledIcon;
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
        private int m_evacCount = 0;

        //TLM
        private int m_shipCount = 0;
        private int m_planeCount = 0;


        private bool m_isChangingTab;

        private UILabel m_LineCount;


        //stríp buttons
        private UIButton bus_strip;
        private UIButton tram_strip;
        private UIButton metro_strip;
        private UIButton train_strip;
        private UIButton ferry_strip;
        private UIButton blimp_strip;
        private UIButton monorail_strip;
        private UIButton ship_strip;
        private UIButton plane_strip;
        private UIButton evac_strip;

        private UIButton planeDepot_strip;
        private UIButton blimpDepot_strip;
        private UIButton shipDepot_strip;
        private UIButton ferryDepot_strip;
        private UIButton trainDepot_strip;
        private UIButton monorailDepot_strip;
        private UIButton metroDepot_strip;
        private UIButton tramDepot_strip;
        private UIButton busDepot_strip;
        private UIButton evacDepot_strip;

        //Extra UI
        public TLMPrefixEditorUI prefixEditor { get; internal set; }


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
            get {
                return tabSystemOrder[m_Strip.selectedIndex % tabSystemOrder.Length];
            }
        }

        public bool isOnCurrentPrefixFilter(List<uint> prefixes)
        {
            return !m_prefixFilter.isVisible || m_prefixFilter.selectedIndex == 0 || prefixes.Contains((uint)(m_prefixFilter.selectedIndex - 1));
        }


        public bool m_isLineView
        {
            get {
                return !m_isDepotView && !m_isPrefixEditor;
            }
        }
        public bool m_isPrefixEditor
        {
            get {
                return m_Strip.selectedIndex == NUM_TRANSPORT_SYSTEMS * 2;
            }
        }

        #region Awake
        private void Awake()
        {
            //this.m_Strip.tab
            enabled = true;
            TLMUtils.clearAllVisibilityEvents(this.GetComponent<UIPanel>());

            this.m_LineCount = base.Find<UILabel>("LabelLineCount");
            prefixEditor = new TLMPrefixEditorUI();

            AwakeRearrangeTabs();
            AwakeDepotTitleComponents();
            AwakeLinesTitleComponents();
            AwakeTopButtons();
            AwakeDayNightOptions();
            AwakePrefixFilter();
            prefixEditor.Init();

            toggleDepotView();
            SetActiveTab(0);
            m_Ready = true;
        }

        private void AwakeDepotTitleComponents()
        {
            //depot title
            var tempTitle = Find<UIPanel>("LineTitle");
            m_depotsTitle = GameObject.Instantiate<UIPanel>(tempTitle);
            m_depotsTitle.transform.SetParent(tempTitle.transform.parent);
            m_depotsTitle.relativePosition = tempTitle.relativePosition;
            m_depotsTitle.isVisible = false;
            GameObject.Destroy(m_depotsTitle.Find<UIButton>("DayButton").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UIButton>("NightButton").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UIButton>("DayNightButton").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UICheckBox>("ToggleAll").gameObject);
            GameObject.Destroy(m_depotsTitle.Find<UIButton>("StopsTitle").gameObject);
            m_depotsTitle.Find<UIButton>("ColorTitle").text = Locale.Get("TUTORIAL_ADVISER_TITLE", "District");
            m_depotsTitle.Find<UIButton>("ColorTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
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

            initContainers();
            fillContainers();
            fixAvailability();
            assignContainers();
            cleanDefaultEvents();
            createAddedContainers();
            cleanContainers();
            reorderStrips();
            reorderContainers();
        }

        #region AwakeRearrangeTabs

        private void initContainers()
        {
            TLMUtils.doLog("Strips Lines");
            bus_strip = m_Strip.tabs[0].GetComponent<UIButton>();
            tram_strip = m_Strip.tabs[1].GetComponent<UIButton>();
            metro_strip = m_Strip.tabs[2].GetComponent<UIButton>();
            train_strip = m_Strip.tabs[3].GetComponent<UIButton>();
            ferry_strip = m_Strip.tabs[4].GetComponent<UIButton>();
            blimp_strip = m_Strip.tabs[5].GetComponent<UIButton>();
            monorail_strip = m_Strip.tabs[6].GetComponent<UIButton>();
            ship_strip = m_Strip.AddTab("");
            plane_strip = m_Strip.AddTab("");
            evac_strip = m_Strip.AddTab("");

            planeDepot_strip = m_Strip.AddTab("");
            blimpDepot_strip = m_Strip.AddTab("");
            shipDepot_strip = m_Strip.AddTab("");
            ferryDepot_strip = m_Strip.AddTab("");
            trainDepot_strip = m_Strip.AddTab("");
            monorailDepot_strip = m_Strip.AddTab("");
            metroDepot_strip = m_Strip.AddTab("");
            tramDepot_strip = m_Strip.AddTab("");
            busDepot_strip = m_Strip.AddTab("");
            evacDepot_strip = m_Strip.AddTab("");

        }
        private void fillContainers()
        {
            int tabIt = 0;

            addIcon("PlaneLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref plane_strip, false, tabIt++, "TLM_PUBLICTRANSPORT_AIRPLANELINES");
            addIcon("Blimp", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref blimp_strip, false, tabIt++, "TLM_PUBLICTRANSPORT_BLIMPLINES");
            addIcon("ShipLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ship_strip, false, tabIt++, "TLM_PUBLICTRANSPORT_SHIPLINES");
            addIcon("Ferry", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ferry_strip, false, tabIt++, "TLM_PUBLICTRANSPORT_FERRYLINES");
            addIcon("Train", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref train_strip, false, tabIt++, "PUBLICTRANSPORT_TRAINLINES");
            addIcon("Monorail", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Monorail), ref monorail_strip, false, tabIt++, "PUBLICTRANSPORT_MONORAILLINES");
            addIcon("Subway", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metro_strip, false, tabIt++, "PUBLICTRANSPORT_METROLINES");
            addIcon("Tram", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tram_strip, false, tabIt++, "PUBLICTRANSPORT_TRAMLINES");
            addIcon("Bus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref bus_strip, false, tabIt++, "PUBLICTRANSPORT_BUSLINES");
            addIcon("EvacBus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref evac_strip, false, tabIt++, "TLM_PUBLICTRANSPORT_EVACBUSLINES");

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Tab init - depots");
            tabIt = 0;
            addIcon("PlaneLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref planeDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_AIRPLANE_DEPOTS");
            addIcon("Blimp", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Airplane), ref blimpDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_BLIMP_DEPOTS");
            addIcon("ShipLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref shipDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_SHIP_DEPOTS");
            addIcon("Ferry", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ferryDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_FERRY_DEPOTS");
            addIcon("Train", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref trainDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_TRAIN_DEPOTS");
            addIcon("Monorail", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Monorail), ref monorailDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_MONORAIL_DEPOTS");
            addIcon("Subway", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metroDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_METRO_DEPOTS");
            addIcon("Tram", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tramDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_TRAM_DEPOTS");
            addIcon("Bus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref busDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_BUS_DEPOTS");
            addIcon("EvacBus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref evacDepot_strip, false, NUM_TRANSPORT_SYSTEMS + tabIt++, "TLM_PUBLICTRANSPORT_EVACBUS_DEPOTS");

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Tab init - star");
            var prefixEditor = m_Strip.AddTab("");
            prefixEditor.textScale = 2.25f;
            prefixEditor.useOutline = true;
            addIcon("Star", "", ref prefixEditor, false, NUM_TRANSPORT_SYSTEMS * 2);
        }
        private void fixAvailability()
        {
            tram_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Tram);
            ferry_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            monorail_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            blimp_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            evac_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.EvacuationBus);

            tramDepot_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Tram);
            ferryDepot_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            monorailDepot_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            blimpDepot_strip.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            evacDepot_strip.isVisible = false;
        }
        private void assignContainers()
        {
            this.m_BusLinesContainer = Find<UIComponent>("BusDetail").Find("Container");
            this.m_TramLinesContainer = Find<UIComponent>("TramDetail").Find("Container");
            this.m_MetroLinesContainer = Find<UIComponent>("MetroDetail").Find("Container");
            this.m_TrainLinesContainer = Find<UIComponent>("TrainDetail").Find("Container");
            this.m_BlimpLinesContainer = Find<UIComponent>("BlimpDetail").Find("Container");
            this.m_MonorailLinesContainer = Find<UIComponent>("MonorailDetail").Find("Container");
            this.m_FerryLinesContainer = Find<UIComponent>("FerryDetail").Find("Container");
        }
        private void cleanDefaultEvents()
        {
            m_BusLinesContainer.eventVisibilityChanged += null;
            m_TramLinesContainer.eventVisibilityChanged += null;
            m_MetroLinesContainer.eventVisibilityChanged += null;
            m_TrainLinesContainer.eventVisibilityChanged += null;
            m_BlimpLinesContainer.eventVisibilityChanged += null;
            m_MonorailLinesContainer.eventVisibilityChanged += null;
            m_FerryLinesContainer.eventVisibilityChanged += null;
        }
        private void createAddedContainers()
        {
            int tabIt;
            CopyContainerFromBus(NUM_TRANSPORT_SYSTEMS - 3, ref m_ShipLinesContainer);
            CopyContainerFromBus(NUM_TRANSPORT_SYSTEMS - 2, ref m_PlaneLinesContainer);
            CopyContainerFromBus(NUM_TRANSPORT_SYSTEMS - 1, ref m_EvacLinesContainer);

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
            CopyContainerFromBus(tabIt++, ref m_EvacDepotsContainer);

            CopyContainerFromBus(tabIt, ref prefixEditor.m_PrefixEditor);
        }
        private void cleanContainers()
        {
            RemoveExtraLines(0, ref m_BusLinesContainer);
            RemoveExtraLines(0, ref m_TramLinesContainer);
            RemoveExtraLines(0, ref m_MetroLinesContainer);
            RemoveExtraLines(0, ref m_TrainLinesContainer);
            RemoveExtraLines(0, ref m_MonorailLinesContainer);
            RemoveExtraLines(0, ref m_ShipLinesContainer);
            RemoveExtraLines(0, ref m_PlaneLinesContainer);
            RemoveExtraLines(0, ref m_EvacLinesContainer);
            RemoveExtraLines(0, ref prefixEditor.m_PrefixEditor);
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
            RemoveExtraLines(0, ref m_EvacDepotsContainer);
        }
        private void reorderStrips()
        {
            int tabIt = 0;
            plane_strip.zOrder = tabIt++;
            blimp_strip.zOrder = (tabIt++);
            ship_strip.zOrder = (tabIt++);
            ferry_strip.zOrder = (tabIt++);
            train_strip.zOrder = (tabIt++);
            monorail_strip.zOrder = (tabIt++);
            metro_strip.zOrder = (tabIt++);
            tram_strip.zOrder = (tabIt++);
            bus_strip.zOrder = (tabIt++);
            evac_strip.zOrder = (tabIt++);
        }
        private void reorderContainers()
        {
            int tabIt = 0;
            m_PlaneLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_BlimpLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_ShipLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_FerryLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_TrainLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_MonorailLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_MetroLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_TramLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_BusLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
            m_EvacLinesContainer.GetComponentInParent<UIPanel>().zOrder = (tabIt++);
        }

        #endregion

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
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Find Color title");
            var colorTitle = m_linesTitle.Find<UIButton>("ColorTitle");
            colorTitle.text += "/" + Locale.Get("TLM_CODE_SHORT");
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
            m_buttonRemoveUnwanted.isVisible = !TransportLinesManagerMod.isIPTLoaded;
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
            m_buttonPrefixConfig.isVisible = !TransportLinesManagerMod.isIPTLoaded;
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
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Find Original buttons");
            m_DayIcon = m_linesTitle.Find<UIButton>("DayButton");
            m_NightIcon = m_linesTitle.Find<UIButton>("NightButton");
            m_DayNightIcon = m_linesTitle.Find<UIButton>("DayNightButton");

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Set Tooltips");
            m_DayIcon.tooltip = Locale.Get("TLM_DAY_FILTER_TOOLTIP");
            m_NightIcon.tooltip = Locale.Get("TLM_NIGHT_FILTER_TOOLTIP");
            m_DayNightIcon.tooltip = Locale.Get("TLM_DAY_NIGHT_FILTER_TOOLTIP");

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Set events");
            m_DayIcon.eventClick += (x, y) =>
            {
                m_showDayLines = !m_showDayLines;
                m_DayIcon.color = m_showDayLines ? Color.white : Color.black;
                m_DayIcon.focusedColor = !m_showDayLines ? Color.white : Color.black;
            };
            m_NightIcon.eventClick += (x, y) =>
            {
                m_showNightLines = !m_showNightLines;
                m_NightIcon.color = m_showNightLines ? Color.white : Color.black;
                m_NightIcon.focusedColor = !m_showDayLines ? Color.white : Color.black;
            };
            m_DayNightIcon.eventClick += (x, y) =>
            {
                m_showDayNightLines = !m_showDayNightLines;
                m_DayNightIcon.color = m_showDayNightLines ? Color.white : Color.black;
                m_DayNightIcon.focusedColor = !m_showDayLines ? Color.white : Color.black;
            };
            if (!TransportLinesManagerMod.isIPTLoaded)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                    TLMUtils.doLog("Create disabled button");
                m_DisabledIcon = GameObject.Instantiate(m_DayIcon.gameObject).GetComponent<UIButton>();
                m_DisabledIcon.transform.SetParent(m_DayIcon.transform.parent);
                m_NightIcon.relativePosition = new Vector3(678, 14);
                m_DayNightIcon.relativePosition = new Vector3(704, 14);
                m_DisabledIcon.normalBgSprite = "Niet";
                m_DisabledIcon.hoveredBgSprite = "Niet";
                m_DisabledIcon.pressedBgSprite = "Niet";
                m_DisabledIcon.disabledBgSprite = "Niet";
                m_DisabledIcon.tooltip = Locale.Get("TLM_DISABLED_LINES_FILTER_TOOLTIP");
                m_DisabledIcon.eventClick += (x, y) =>
                {
                    m_showDisabledLines = !m_showDisabledLines;
                    m_DisabledIcon.color = m_showDisabledLines ? Color.white : Color.black;
                    m_DisabledIcon.focusedColor = !m_showDayLines ? Color.white : Color.black;
                };
                m_DisabledIcon.relativePosition = new Vector3(736, 14);
            }
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
            if (tooltipText == "")
            {
                targetButton.width = 01;
                targetButton.height = 01;
                TLMUtils.initButtonSameSprite(targetButton, "");
                targetButton.isVisible = false;
            }
            else
            {
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
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("addIcon: pre eventClick");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("addIcon: init label icon");
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
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("addIcon: end");
        }



        #endregion

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
            return (int)typeof(PublicTransportDetailPanel).GetMethod("NaturalCompare", Redirector<TLMDepotAI>.allFlags).Invoke(null, new object[] { left, right });
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

        private void OnDepotNameSort()
        {
            if (!m_isDepotView || m_isPrefixEditor)
                return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareDepotNames));
            // m_LastSortCriterionDepot = DepotSortCriterion.NAME;
            uIComponent.Invalidate();
        }

        private void OnDepotDistrictSort()
        {
            if (!m_isDepotView || m_isPrefixEditor)
                return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareDepotDistricts));
            //m_LastSortCriterionDepot = DepotSortCriterion.DISTRICT;
            uIComponent.Invalidate();
        }

        private void OnStopSort()
        {
            if (!m_isLineView)
                return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareStops));
            this.m_LastSortCriterionLines = LineSortCriterion.STOP;
            uIComponent.Invalidate();
        }

        private void OnVehicleSort()
        {
            if (!m_isLineView)
                return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareVehicles));
            this.m_LastSortCriterionLines = LineSortCriterion.VEHICLE;
            uIComponent.Invalidate();
        }

        private void OnPassengerSort()
        {
            if (!m_isLineView)
                return;
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            if (uIComponent.components.Count == 0)
                return;
            Quicksort(uIComponent.components, new Comparison<UIComponent>(ComparePassengers));
            this.m_LastSortCriterionLines = LineSortCriterion.PASSENGER;
            uIComponent.Invalidate();
        }

        private void OnLineNumberSort()
        {
            if (!m_isLineView)
                return;
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
            this.m_Strip.selectedIndex = idx;
            if (m_isDepotView != (idx > NUM_TRANSPORT_SYSTEMS && idx != NUM_TRANSPORT_SYSTEMS * 2))
            {
                toggleDepotView();
            }
        }


        public void toggleDepotView()
        {
            m_isDepotView = !m_isDepotView;

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

            tram_strip.isVisible = !m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Tram);
            ferry_strip.isVisible = !m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            monorail_strip.isVisible = !m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            blimp_strip.isVisible = !m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            evac_strip.isVisible = !m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.EvacuationBus);

            tramDepot_strip.isVisible = m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Tram);
            ferryDepot_strip.isVisible = m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            monorailDepot_strip.isVisible = m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            blimpDepot_strip.isVisible = m_isDepotView && Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Monorail);
            evacDepot_strip.isVisible = false;


            bus_strip.isVisible = !m_isDepotView;
            metro_strip.isVisible = !m_isDepotView;
            train_strip.isVisible = !m_isDepotView;
            ship_strip.isVisible = !m_isDepotView;
            plane_strip.isVisible = !m_isDepotView;

            planeDepot_strip.isVisible = m_isDepotView;
            shipDepot_strip.isVisible = m_isDepotView;
            trainDepot_strip.isVisible = m_isDepotView;
            metroDepot_strip.isVisible = m_isDepotView;
            busDepot_strip.isVisible = m_isDepotView;

            if (m_Strip.selectedIndex != NUM_TRANSPORT_SYSTEMS * 2 && m_isDepotView != m_Strip.selectedIndex > NUM_TRANSPORT_SYSTEMS)
            {
                var tabVal = (m_isDepotView ? NUM_TRANSPORT_SYSTEMS : 0) + (m_Strip.selectedIndex % NUM_TRANSPORT_SYSTEMS);
                if (tabVal == NUM_TRANSPORT_SYSTEMS + 9)//Shelter
                {
                    tabVal--;
                }
                SetActiveTab(tabVal);
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
                    m_evacCount = 0;

                    for (ushort lineIdIterator = 1; lineIdIterator < 256; lineIdIterator += 1)
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdIterator].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created)
                        {
                            var tsd = TLMCW.getDefinitionForLine(lineIdIterator);
                            if (tsd != default(TransportSystemDefinition))
                            {
                                switch (tsd.toConfigIndex())
                                {
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
                                    case TLMCW.ConfigIndex.EVAC_BUS_CONFIG:
                                        comp = m_EvacLinesContainer;
                                        m_evacCount = AddToList(m_evacCount, lineIdIterator, ref comp);
                                        break;

                                }
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
                    comp = m_EvacLinesContainer;
                    RemoveExtraLines(m_evacCount, ref comp);

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
                int evacCount = 0;

                UIComponent comp;
                foreach (ushort buildingID in TLMDepotAI.getAllDepotsFromCity())
                {
                    PrefabAI prefabAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.GetAI();
                    if (prefabAI is ShelterAI)
                    {
                        comp = m_EvacDepotsContainer;
                        evacCount = AddDepotToList(evacCount, buildingID, false, ref comp);
                        continue;
                    }
                    DepotAI ai = prefabAI as DepotAI;
                    var tiArray = new TransportInfo[] {
                       ai.m_transportInfo,
                        ai.m_secondaryTransportInfo
                    };
                    foreach (TransportInfo info in tiArray)
                    {
                        var tsd = TransportSystemDefinition.from(info);
                        if (tsd == default(TransportSystemDefinition)
                            || (ai.m_transportInfo == info && ai.m_maxVehicleCount == 0)
                            || (ai.m_secondaryTransportInfo == info && ai.m_maxVehicleCount2 == 0)
                            )
                        {
                            continue;
                        }
                        switch (tsd.toConfigIndex())
                        {
                            case TLMCW.ConfigIndex.BUS_CONFIG:
                                comp = m_BusDepotsContainer;
                                busCount = AddDepotToList(busCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.TRAM_CONFIG:
                                comp = m_TramDepotsContainer;
                                tramCount = AddDepotToList(tramCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.METRO_CONFIG:
                                comp = m_MetroDepotsContainer;
                                metroCount = AddDepotToList(metroCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.TRAIN_CONFIG:
                                comp = m_TrainDepotsContainer;
                                trainCount = AddDepotToList(trainCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.SHIP_CONFIG:
                                comp = m_ShipDepotsContainer;
                                shipCount = AddDepotToList(shipCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.PLANE_CONFIG:
                                comp = m_PlaneDepotsContainer;
                                planeCount = AddDepotToList(planeCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.MONORAIL_CONFIG:
                                comp = m_MonorailDepotsContainer;
                                monorailCount = AddDepotToList(monorailCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.BLIMP_CONFIG:
                                comp = m_BlimpDepotsContainer;
                                blimpCount = AddDepotToList(blimpCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.FERRY_CONFIG:
                                comp = m_FerryDepotsContainer;
                                ferryCount = AddDepotToList(ferryCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                            case TLMCW.ConfigIndex.EVAC_BUS_CONFIG:
                                comp = m_EvacDepotsContainer;
                                evacCount = AddDepotToList(evacCount, buildingID, info == ai.m_secondaryTransportInfo, ref comp);
                                break;
                        }
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
                comp = m_EvacDepotsContainer;
                RemoveExtraLines(evacCount, ref comp);

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
            TLMPublicTransportLineInfoItem publicTransportLineInfo;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("PreIF");
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Count = {0}; Component = {1}; components count = {2}", count, component.ToString(), component.components.Count);
            if (count >= component.components.Count)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                    TLMUtils.doLog("IF TRUE");
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                publicTransportLineInfo = temp.AddComponent<TLMPublicTransportLineInfoItem>();
                component.AttachUIComponent(publicTransportLineInfo.gameObject);
            }
            else
            {
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

        private int AddDepotToList(int count, ushort buildingID, bool secondary, ref UIComponent component)
        {
            TLMPublicTransportDepotInfo publicTransportDepotInfo;
            if (count >= component.components.Count)
            {
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                publicTransportDepotInfo = temp.AddComponent<TLMPublicTransportDepotInfo>();
                component.AttachUIComponent(publicTransportDepotInfo.gameObject);
            }
            else
            {
                publicTransportDepotInfo = component.components[count].GetComponent<TLMPublicTransportDepotInfo>();
            }
            publicTransportDepotInfo.buildingId = buildingID;
            publicTransportDepotInfo.secondary = secondary;
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
                string[] filterOptions = TLMUtils.getPrefixesOptions(tabSystemOrder[idx % NUM_TRANSPORT_SYSTEMS]);
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
            m_buttonRemoveUnwanted.isVisible = !m_isDepotView && !TransportLinesManagerMod.isIPTLoaded;

            if (m_isDepotView && !m_isPrefixEditor)
            {
                m_depotsTitle.Find<UIButton>("NameTitle").text = string.Format(Locale.Get("TLM_DEPOT_NAME_PATTERN"), Locale.Get("TLM_PUBLICTRANSPORT_OF_DEPOT", currentSelectedSystem.ToString()));
            }

            if (m_isPrefixEditor)
            {
                GetComponent<UIPanel>().height = 910;
            }

            m_depotsTitle.relativePosition = m_linesTitle.relativePosition;
            if (!TransportLinesManagerMod.isIPTLoaded)
            {
                m_DisabledIcon.relativePosition = new Vector3(736, 14);
            }
            RefreshLines();
            OnLineNumberSort();
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
            if (this.m_Strip.selectedIndex < 0 && visible)
            {
                this.m_Strip.selectedIndex = 0;
            }
            else if (this.m_Strip.selectedIndex > -1 && this.m_Strip.selectedIndex < this.m_Strip.tabContainer.components.Count)
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
            else if (visible)
            {
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
                        TLMPublicTransportLineInfoItem uIComponent2 = uIComponent.components[i].GetComponent<TLMPublicTransportLineInfoItem>();
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
                        TLMPublicTransportLineInfoItem uIComponent2 = uIComponent.components[i].GetComponent<TLMPublicTransportLineInfoItem>();
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
                    m_busCount,
                    m_evacCount
                }[transportTabIndex % NUM_TRANSPORT_SYSTEMS];
            }
            else
            {
                this.m_LineCount.text = "";
            }
        }

    }


}
