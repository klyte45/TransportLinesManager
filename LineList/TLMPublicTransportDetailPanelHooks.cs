using ColossalFramework;
using ColossalFramework.UI;
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

        #region Hooking
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();
        public void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            TLMUtils.doLog("Loading TLMPublicTransportDetailPanelHooks Hooks!");
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("RefreshLines", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("Awake", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnTabChanged", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnChangeVisibleAll", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnNameSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnStopSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnVehicleSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportDetailPanel), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OnPassengerSort", allFlags), ref redirects);
            AddRedirect(typeof(PublicTransportLineInfo), typeof(TLMPublicTransportDetailPanelHooks).GetMethod("RefreshData", allFlags), ref redirects);

            TLMUtils.doLog("Inverse TLMPublicTransportDetailPanelHooks Hooks!");
            AddRedirect(typeof(TLMPublicTransportDetailPanel), typeof(PublicTransportDetailPanel).GetMethod("NaturalCompare", allFlags), ref redirects);

            TLMUtils.doLog("Swap TLMPublicTransportDetailPanelHooks Hooks!");
            var go = GameObject.Find("UIView").GetComponentInChildren<PublicTransportDetailPanel>().gameObject;
            GameObject.Destroy(go.GetComponent<PublicTransportDetailPanel>());
            TLMPublicTransportDetailPanel.instance = go.AddComponent<TLMPublicTransportDetailPanel>();


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

        public static TLMPublicTransportDetailPanel instance;

        private static readonly string kLineTemplate = "LineTemplate";

        private int m_LastLineCount;

        private bool m_Ready;

        private bool m_LinesUpdated;

        private bool[] m_ToggleAllState;

        private LineSortCriterion m_LastSortCriterion;

        private UITabstrip m_Strip;

        private readonly TLMCW.ConfigIndex[] tabSystemOrder =
        {
            TLMCW.ConfigIndex.SHIP_CONFIG,
            TLMCW.ConfigIndex.BULLET_TRAIN_CONFIG,
            TLMCW.ConfigIndex.TRAIN_CONFIG,
            TLMCW.ConfigIndex.SURFACE_METRO_CONFIG,
            TLMCW.ConfigIndex.METRO_CONFIG,
            TLMCW.ConfigIndex.TRAM_CONFIG,
            TLMCW.ConfigIndex.HIGH_BUS_CONFIG,
            TLMCW.ConfigIndex.BUS_CONFIG,
            TLMCW.ConfigIndex.LOW_BUS_CONFIG
        };

        private UIComponent m_BusLinesContainer;
        private UIComponent m_TramLinesContainer;
        private UIComponent m_MetroLinesContainer;
        private UIComponent m_TrainLinesContainer;
        private UIComponent m_LowBusLinesContainer;
        private UIComponent m_HighBusLinesContainer;
        private UIComponent m_SurfaceMetroLinesContainer;
        private UIComponent m_BulletTrainLinesContainer;
        private UIComponent m_ShipLinesContainer;

        private UICheckBox m_ToggleAll;
        private UISprite m_DayIcon;
        private UISprite m_NightIcon;
        private UISprite m_DayNightIcon;
        private UISprite m_DisabledIcon;
        private UIDropDown m_prefixFilter;

        private bool m_showDayNightLines = true;
        private bool m_showDayLines = true;
        private bool m_showNightLines = true;
        private bool m_showDisabledLines = true;

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



        private static int CompareNames(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfo component = left.GetComponent<TLMPublicTransportLineInfo>();
            TLMPublicTransportLineInfo component2 = right.GetComponent<TLMPublicTransportLineInfo>();
            return string.Compare(component.lineName, component2.lineName, false); //NaturalCompare(component.lineName, component2.lineName);
        }

        private static int CompareLineNumbers(UIComponent left, UIComponent right)
        {
            TLMPublicTransportLineInfo component = left.GetComponent<TLMPublicTransportLineInfo>();
            TLMPublicTransportLineInfo component2 = right.GetComponent<TLMPublicTransportLineInfo>();
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

            this.m_Strip = Find<UITabstrip>("Tabstrip");

            this.m_Strip.relativePosition = new Vector3(13, 45);

            var lowBus = m_Strip.AddTab("");
            var highBus = m_Strip.AddTab("");
            var surfMetro = m_Strip.AddTab("");
            var bulletTrain = m_Strip.AddTab("");
            var ship = m_Strip.AddTab("");
            var bus = m_Strip.tabs[0].GetComponent<UIButton>();
            var tram = m_Strip.tabs[1].GetComponent<UIButton>();
            var metro = m_Strip.tabs[2].GetComponent<UIButton>();
            var train = m_Strip.tabs[3].GetComponent<UIButton>();


            addIcon("ShipLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref ship, false, 0, "Ship Lines");
            addIcon("BulletTrain", "BulletTrainImage", ref bulletTrain, true, 1, "Bullet Train Lines");
            addIcon("Train", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref train, false, 2, "PUBLICTRANSPORT_TRAINLINES", true);
            addIcon("SurfaceMetro", "SurfaceMetroImage", ref surfMetro, true, 3, "Surface Metro Lines");
            addIcon("Subway", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metro, false, 4, "PUBLICTRANSPORT_METROLINES", true);
            addIcon("Tram", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tram, false, 5, "PUBLICTRANSPORT_TRAMLINES", true);
            addIcon("HighBus", "HighBusImage", ref highBus, true, 6, "High Capacity Bus Lines");
            addIcon("Bus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref bus, false, 7, "PUBLICTRANSPORT_BUSLINES", true);
            addIcon("LowBus", "LowBusImage", ref lowBus, true, 8, "Low Capacity Bus Lines");


            tram.isVisible = Singleton<TransportManager>.instance.TransportTypeLoaded(TransportInfo.TransportType.Tram);
            lowBus.isVisible = !TransportLinesManagerMod.isIPTCompatibiltyMode;
            highBus.isVisible = !TransportLinesManagerMod.isIPTCompatibiltyMode;
            surfMetro.isVisible = !TransportLinesManagerMod.isIPTCompatibiltyMode;
            bulletTrain.isVisible = !TransportLinesManagerMod.isIPTCompatibiltyMode;

            this.m_BusLinesContainer = Find<UIComponent>("BusDetail").Find("Container");
            this.m_TramLinesContainer = Find<UIComponent>("TramDetail").Find("Container");
            this.m_MetroLinesContainer = Find<UIComponent>("MetroDetail").Find("Container");
            this.m_TrainLinesContainer = Find<UIComponent>("TrainDetail").Find("Container");

            m_BusLinesContainer.eventVisibilityChanged += null;
            m_TramLinesContainer.eventVisibilityChanged += null;
            m_MetroLinesContainer.eventVisibilityChanged += null;
            m_TrainLinesContainer.eventVisibilityChanged += null;

            CopyContainerFromBus(4, ref m_LowBusLinesContainer);
            CopyContainerFromBus(5, ref m_HighBusLinesContainer);
            CopyContainerFromBus(6, ref m_SurfaceMetroLinesContainer);
            CopyContainerFromBus(7, ref m_BulletTrainLinesContainer);
            CopyContainerFromBus(8, ref m_ShipLinesContainer);

            RemoveExtraLines(0, ref m_BusLinesContainer);
            RemoveExtraLines(0, ref m_TramLinesContainer);
            RemoveExtraLines(0, ref m_MetroLinesContainer);
            RemoveExtraLines(0, ref m_TrainLinesContainer);
            RemoveExtraLines(0, ref m_LowBusLinesContainer);
            RemoveExtraLines(0, ref m_HighBusLinesContainer);
            RemoveExtraLines(0, ref m_SurfaceMetroLinesContainer);
            RemoveExtraLines(0, ref m_BulletTrainLinesContainer);
            RemoveExtraLines(0, ref m_ShipLinesContainer);


            ship.zOrder = (0);
            bulletTrain.zOrder = (1);
            train.zOrder = (2);
            surfMetro.zOrder = (3);
            metro.zOrder = (4);
            tram.zOrder = (5);
            highBus.zOrder = (6);
            bus.zOrder = (7);
            lowBus.zOrder = (8);

            m_ShipLinesContainer.GetComponentInParent<UIPanel>().zOrder = (0);
            m_BulletTrainLinesContainer.GetComponentInParent<UIPanel>().zOrder = (1);
            m_TrainLinesContainer.GetComponentInParent<UIPanel>().zOrder = (2);
            m_SurfaceMetroLinesContainer.GetComponentInParent<UIPanel>().zOrder = (3);
            m_MetroLinesContainer.GetComponentInParent<UIPanel>().zOrder = (4);
            m_TramLinesContainer.GetComponentInParent<UIPanel>().zOrder = (5);
            m_HighBusLinesContainer.GetComponentInParent<UIPanel>().zOrder = (6);
            m_BusLinesContainer.GetComponentInParent<UIPanel>().zOrder = (7);
            m_LowBusLinesContainer.GetComponentInParent<UIPanel>().zOrder = (8);




            this.m_ToggleAllState = new bool[this.m_Strip.tabCount];
            this.m_Strip.eventSelectedIndexChanged += null;
            this.m_Strip.eventSelectedIndexChanged += new PropertyChangedEventHandler<int>(this.OnTabChanged);
            this.m_ToggleAll = Find<UICheckBox>("ToggleAll");
            this.m_ToggleAll.eventCheckChanged += new PropertyChangedEventHandler<bool>(this.CheckChangedFunction);
            for (int i = 0; i < this.m_ToggleAllState.Length; i++)
            {
                this.m_ToggleAllState[i] = true;
            }
            Find<UIButton>("NameTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnNameSort();
            };
            Find<UIButton>("StopsTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnStopSort();
            };
            Find<UIButton>("VehiclesTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnVehicleSort();
            };
            Find<UIButton>("PassengersTitle").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnPassengerSort();
            };
            var colorTitle = Find<UILabel>("ColorTitle");
            colorTitle.suffix = "/Code";
            colorTitle.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.OnLineNumberSort();
            };

            this.m_LastSortCriterion = LineSortCriterion.DEFAULT;

            //Auto color & Auto Name
            UIButton buttonAutoName = null;
            TLMUtils.createUIElement<UIButton>(ref buttonAutoName, transform);
            buttonAutoName.pivot = UIPivotPoint.TopRight;
            buttonAutoName.text = "Auto Name All";
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.width = 105;
            buttonAutoName.height = 15;
            buttonAutoName.tooltip = "Use auto name in all lines";
            TLMUtils.initButton(buttonAutoName, true, "ButtonMenu");
            buttonAutoName.name = "AutoName";
            buttonAutoName.isVisible = true;
            buttonAutoName.eventClick += (component, eventParam) =>
            {
                OnAutoNameAll();
            };

            UIButton buttonAutoColor = null;
            TLMUtils.createUIElement<UIButton>(ref buttonAutoColor, transform);
            buttonAutoColor.pivot = UIPivotPoint.TopRight;
            buttonAutoColor.text = "Auto Color All";
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 105;
            buttonAutoColor.height = 15;
            buttonAutoColor.tooltip = "Pick a color from the palette for each line";
            TLMUtils.initButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.name = "AutoColor";
            buttonAutoColor.isVisible = true;
            buttonAutoColor.eventClick += (component, eventParam) =>
            {
                OnAutoColorAll();
            };

            //filters
            m_DayIcon = Find<UISprite>("DaySprite");
            m_NightIcon = Find<UISprite>("NightSprite");
            m_DayNightIcon = Find<UISprite>("DayNightSprite");
            m_DisabledIcon = GameObject.Instantiate(m_DayIcon.gameObject).GetComponent<UISprite>();
            m_DisabledIcon.transform.SetParent(m_DayIcon.transform.parent);
            m_NightIcon.relativePosition = new Vector3(670, 14);
            m_DayNightIcon.relativePosition = new Vector3(695, 14);
            m_DisabledIcon.spriteName = "Niet";

            m_DayIcon.tooltip = "Click to show/hide day only lines";
            m_NightIcon.tooltip = "Click to show/hide night only lines";
            m_DayNightIcon.tooltip = "Click to show/hide 24h lines";
            m_DisabledIcon.tooltip = "Click to show/hide disabled and broken lines";

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
            prefixFilterLabel.text = "Prefix\nFilter";
            prefixFilterLabel.relativePosition = new Vector3(27, -35);
            prefixFilterLabel.textAlignment = UIHorizontalAlignment.Center;

            this.SetActiveTab(1);
            this.SetActiveTab(0);
            m_DisabledIcon.relativePosition = new Vector3(736, 14);
            buttonAutoColor.relativePosition = new Vector3(655, 61);
            buttonAutoName.relativePosition = new Vector3(655, 43);

            var icon = Find<UISprite>("Icon");
            icon.spriteName = "TransportLinesManagerIconHovered";
            icon.atlas = TLMController.taTLM;

            var title = Find<UILabel>("Label");
            title.suffix = " - TLM v" + TransportLinesManagerMod.version;

            component.relativePosition = new Vector3(395, 58);

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
        }



        private void addIcon(string namePrefix, string iconName, ref UIButton targetButton, bool alternativeIconAtlas, int tabIdx, string tooltipText, bool isTooltipLocale = false)
        {
            TLMUtils.doLog("addIcon: init " + namePrefix);

            TLMUtils.initButtonFg(targetButton, false, "");

            targetButton.atlas = TLMController.taLineNumber;
            targetButton.width = 40;
            targetButton.height = 40;
            targetButton.name = namePrefix + "Legend";
            TLMUtils.initButtonSameSprite(targetButton, namePrefix + "Icon");
            targetButton.hoveredColor = Color.gray;
            targetButton.focusedColor = Color.green;
            targetButton.eventClick += null;
            targetButton.eventClick += (x, y) =>
           {
               SetActiveTab(tabIdx);
           };
            TLMUtils.doLog("addIcon: pre eventClick");
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
            TLMUtils.doLog("addIcon: end");
        }


        private void OnNameSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareNames));
            this.m_LastSortCriterion = LineSortCriterion.NAME;
            uIComponent.Invalidate();
        }

        private void OnStopSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareStops));
            this.m_LastSortCriterion = LineSortCriterion.STOP;
            uIComponent.Invalidate();
        }

        private void OnVehicleSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareVehicles));
            this.m_LastSortCriterion = LineSortCriterion.VEHICLE;
            uIComponent.Invalidate();
        }

        private void OnPassengerSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            Quicksort(uIComponent.components, new Comparison<UIComponent>(ComparePassengers));
            this.m_LastSortCriterion = LineSortCriterion.PASSENGER;
            uIComponent.Invalidate();
        }

        private void OnLineNumberSort()
        {
            UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
            Quicksort(uIComponent.components, new Comparison<UIComponent>(CompareLineNumbers));
            this.m_LastSortCriterion = LineSortCriterion.LINE_NUMBER;
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
            this.m_Strip.selectedIndex = idx;
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
                int lowBusCount = 0;
                int highBusCount = 0;
                int surfaceMetroCount = 0;
                int bulletTrainCount = 0;

                for (ushort lineIdIterator = 1; lineIdIterator < 256; lineIdIterator += 1)
                {
                    if ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdIterator].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created)
                    {
                        switch (TLMCW.getConfigIndexForLine(lineIdIterator))
                        {
                            case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                                busCount = AddToList(busCount, lineIdIterator, ref m_BusLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.TRAM_CONFIG:
                                tramCount = AddToList(tramCount, lineIdIterator, ref m_TramLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.METRO_CONFIG:
                                metroCount = AddToList(metroCount, lineIdIterator, ref m_MetroLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.TRAIN_CONFIG:
                                trainCount = AddToList(trainCount, lineIdIterator, ref m_TrainLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.BULLET_TRAIN_CONFIG:
                                bulletTrainCount = AddToList(bulletTrainCount, lineIdIterator, ref m_BulletTrainLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.SURFACE_METRO_CONFIG:
                                surfaceMetroCount = AddToList(surfaceMetroCount, lineIdIterator, ref m_SurfaceMetroLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.LOW_BUS_CONFIG:
                                lowBusCount = AddToList(lowBusCount, lineIdIterator, ref m_LowBusLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.HIGH_BUS_CONFIG:
                                highBusCount = AddToList(highBusCount, lineIdIterator, ref m_HighBusLinesContainer);
                                break;
                            case TLMCW.ConfigIndex.SHIP_CONFIG:
                                shipCount = AddToList(shipCount, lineIdIterator, ref m_ShipLinesContainer);
                                break;
                        }
                    }
                }
                RemoveExtraLines(busCount, ref this.m_BusLinesContainer);
                RemoveExtraLines(tramCount, ref this.m_TramLinesContainer);
                RemoveExtraLines(metroCount, ref this.m_MetroLinesContainer);
                RemoveExtraLines(trainCount, ref this.m_TrainLinesContainer);
                RemoveExtraLines(lowBusCount, ref this.m_LowBusLinesContainer);
                RemoveExtraLines(highBusCount, ref this.m_HighBusLinesContainer);
                RemoveExtraLines(bulletTrainCount, ref this.m_BulletTrainLinesContainer);
                RemoveExtraLines(surfaceMetroCount, ref this.m_SurfaceMetroLinesContainer);
                RemoveExtraLines(shipCount, ref this.m_ShipLinesContainer);

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
            TLMUtils.doLog("PreIF");
            TLMUtils.doLog("Count = {0}; Component = {1}; components count = {2}", count, component.ToString(), component.components.Count);
            if (count >= component.components.Count)
            {
                TLMUtils.doLog("IF TRUE");
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                publicTransportLineInfo = temp.AddComponent<TLMPublicTransportLineInfo>();
                component.AttachUIComponent(publicTransportLineInfo.gameObject);
            }
            else
            {
                TLMUtils.doLog("IF FALSE");
                TLMUtils.doLog("component.components[count] = {0};", component.components[count]);
                publicTransportLineInfo = component.components[count].GetComponent<TLMPublicTransportLineInfo>();
            }
            publicTransportLineInfo.lineID = lineIdIterator;
            publicTransportLineInfo.RefreshData(true, false);
            count++;
            return count;
        }
        bool isChangingTab;
        private void OnTabChanged(UIComponent c, int idx)
        {
            if (this.m_ToggleAll != null)
            {
                isChangingTab = true;
                this.m_ToggleAll.isChecked = this.m_ToggleAllState[idx];
                isChangingTab = false;
            }
            string[] filterOptions = TLMUtils.getFilterPrefixesOptions(tabSystemOrder[idx]);
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

            m_DisabledIcon.relativePosition = new Vector3(736, 14);

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
                UIComponent uIComponent = this.m_Strip.tabContainer.components[this.m_Strip.selectedIndex].Find("Container");
                if (uIComponent != null)
                {
                    for (int i = 0; i < uIComponent.components.Count; i++)
                    {
                        UIComponent uIComponent2 = uIComponent.components[i];
                        if (uIComponent2 != null)
                        {
                            UICheckBox uICheckBox = uIComponent2.Find<UICheckBox>("LineVisible");
                            uICheckBox.isChecked = visible;
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
                if (this.m_LastSortCriterion != LineSortCriterion.DEFAULT)
                {
                    if (this.m_LastSortCriterion == LineSortCriterion.NAME)
                    {
                        this.OnNameSort();
                    }
                    else if (this.m_LastSortCriterion == LineSortCriterion.PASSENGER)
                    {
                        this.OnPassengerSort();
                    }
                    else if (this.m_LastSortCriterion == LineSortCriterion.STOP)
                    {
                        this.OnStopSort();
                    }
                    else if (this.m_LastSortCriterion == LineSortCriterion.VEHICLE)
                    {
                        this.OnVehicleSort();
                    }
                    else if (this.m_LastSortCriterion == LineSortCriterion.LINE_NUMBER)
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

    }


}
