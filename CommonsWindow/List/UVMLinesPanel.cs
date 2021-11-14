using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow
{

    internal class UVMLinesPanel : UICustomControl
    {
        public UIPanel MainContainer { get; private set; }

        internal TransportSystemDefinition TSD
        {
            get => m_tsd; set
            {
                m_tsd = value;
                m_isUpdated = false;
            }
        }
        private UICheckBox m_visibilityToggle;
        private LineSortCriterion m_lastSortCriterionLines;
        private bool m_reverseOrder = false;
        private UIButton m_autoNameAll;
        private UIButton m_autoColorAll;
        private int m_lastLineCount;
        private UIPanel m_createdTitleLine;
        protected UIPanel titleLine;
        protected UITemplateList<UIPanel> lineItems;
        private UIScrollablePanel listPanel;
        protected UILabel m_countLines;
        private TransportSystemDefinition m_tsd = TransportSystemDefinition.BUS;


        private bool m_isUpdated;

        #region Awake
        protected void Awake()
        {

            MainContainer = GetComponent<UIPanel>();

            CreateTitleRow(out titleLine, MainContainer);


            KlyteMonoUtils.CreateScrollPanel(MainContainer, out listPanel, out UIScrollbar scrollbar, MainContainer.width - 30, MainContainer.height - 70, new Vector3(5, 40));
            listPanel.autoLayout = true;
            listPanel.autoLayoutDirection = LayoutDirection.Vertical;
            listPanel.eventVisibilityChanged += OnToggleVisible;
            UVMLineListItem.EnsureTemplate();
            lineItems = new UITemplateList<UIPanel>(listPanel, UVMLineListItem.LINE_LIST_ITEM_TEMPLATE);

            m_countLines = UIHelperExtension.AddLabel(MainContainer, "LineCounter", MainContainer.width, out UIPanel counterContainer);
            m_countLines.padding.left = 5;
            counterContainer.relativePosition = new Vector3(0, MainContainer.height - 20);
        }

        private void OnToggleVisible(UIComponent component, bool value)
        {
            if (value)
            {
                RefreshLines();
            }
        }

        #endregion

        protected void Update()
        {
            if (!component.isVisible)
            {
                return;
            }
            if (!m_isUpdated || m_lastLineCount != TransportManager.instance.m_lineCount)
            {
                m_lastLineCount = Singleton<TransportManager>.instance.m_lineCount;
                RefreshLines();
            }
        }


        #region Awake
        protected void Start()
        {
            m_lastSortCriterionLines = LineSortCriterion.LINE_NUMBER;

            UIComponent parent = GetComponent<UIComponent>();
            KlyteMonoUtils.CreateUIElement(out m_autoNameAll, parent.transform);
            m_autoNameAll.relativePosition = new Vector3(parent.width - 50f, -5);
            m_autoNameAll.textScale = 0.6f;
            m_autoNameAll.width = 40;
            m_autoNameAll.height = 40;
            m_autoNameAll.tooltip = Locale.Get("K45_TLM_AUTO_NAME_ALL_TOOLTIP");
            KlyteMonoUtils.InitButton(m_autoNameAll, true, "ButtonMenu");
            m_autoNameAll.name = "AutoNameAll";
            m_autoNameAll.isVisible = true;
            m_autoNameAll.normalFgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoNameIcon);
            m_autoNameAll.eventClick += (component, eventParam) =>
            {
                foreach (UVMLineListItem item in lineItems.items.Select(x => x.GetComponentInChildren<UVMLineListItem>()))
                {
                    item.DoAutoName();
                }
            };

            KlyteMonoUtils.CreateUIElement(out m_autoColorAll, parent.transform);
            m_autoColorAll.relativePosition = new Vector3(parent.width - 90f, -5);
            m_autoColorAll.textScale = 0.6f;
            m_autoColorAll.width = 40;
            m_autoColorAll.height = 40;
            m_autoColorAll.tooltip = Locale.Get("K45_TLM_AUTO_COLOR_ALL_TOOLTIP");
            KlyteMonoUtils.InitButton(m_autoColorAll, true, "ButtonMenu");
            m_autoColorAll.name = "AutoColorAll";
            m_autoColorAll.isVisible = true;
            m_autoColorAll.normalFgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoColorIcon);
            m_autoColorAll.eventClick += (component, eventParam) =>
            {
                foreach (UVMLineListItem item in lineItems.items.Select(x => x.GetComponentInChildren<UVMLineListItem>()))
                {
                    item.DoAutoColor();
                }
            };

        }
        #endregion

        #region title row
        protected void CreateTitleRow(out UIPanel titleLine, UIComponent parent)
        {
            LogUtils.DoLog("Creating Title Row ");
            KlyteMonoUtils.CreateUIElement(out titleLine, parent.transform, "TLMtitleline", new Vector4(5, 0, parent.width - 10, 40));
            titleLine.padding = new RectOffset(0, 0, 50, 0);
            m_createdTitleLine = titleLine;

            KlyteMonoUtils.CreateUIElement(out UILabel codColor, titleLine.transform, "codColor");
            codColor.minimumSize = new Vector2(60, 0);
            codColor.area = new Vector4(80, 10, codColor.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(codColor, (uint)codColor.width);
            codColor.textAlignment = UIHorizontalAlignment.Center;
            codColor.prefix = Locale.Get("PUBLICTRANSPORT_LINECOLOR");
            codColor.text = "/";
            codColor.suffix = Locale.Get("K45_TLM_CODE_SHORT");
            codColor.eventClicked += CodColor_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel lineName, titleLine.transform, "lineName");
            lineName.minimumSize = new Vector2(200, 0);
            lineName.area = new Vector4(140, 10, lineName.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(lineName, (uint)lineName.width);
            lineName.textAlignment = UIHorizontalAlignment.Center;
            lineName.text = Locale.Get("PUBLICTRANSPORT_LINENAME");
            lineName.eventClicked += LineName_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel stops, titleLine.transform, "stops");
            stops.minimumSize = new Vector2(80, 0);
            stops.area = new Vector4(340, 10, stops.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(stops, (uint)stops.width);
            stops.textAlignment = UIHorizontalAlignment.Center;
            stops.text = Locale.Get("PUBLICTRANSPORT_LINESTOPS");
            stops.eventClicked += Stops_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel vehicles, titleLine.transform, "vehicles");
            vehicles.minimumSize = new Vector2(110, 0);
            vehicles.area = new Vector4(430, 10, vehicles.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(vehicles, (uint)vehicles.width);
            vehicles.textAlignment = UIHorizontalAlignment.Center;
            vehicles.text = Locale.Get("PUBLICTRANSPORT_VEHICLES");
            vehicles.eventClicked += Vehicles_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel passengers, titleLine.transform, "passengers");
            passengers.minimumSize = new Vector2(80, 0);
            passengers.area = new Vector4(540, 10, passengers.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(passengers, (uint)passengers.width);
            passengers.textAlignment = UIHorizontalAlignment.Center;
            passengers.text = Locale.Get("PUBLICTRANSPORT_PASSENGERS");
            passengers.eventClicked += Passengers_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel profitLW, titleLine.transform, "profit");
            profitLW.minimumSize = new Vector2(150, 0);
            profitLW.area = new Vector4(625, 10, profitLW.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(profitLW, (uint)profitLW.width);
            profitLW.textAlignment = UIHorizontalAlignment.Center;
            profitLW.text = Locale.Get(TLMController.IsRealTimeEnabled ? "K45_TLM_BALANCE_LAST_HOUR_HALF" : "K45_TLM_BALANCE_LAST_WEEK");
            profitLW.eventClicked += Profit_eventClicked;

            AwakeShowLineButton();

            LogUtils.DoLog("End creating Title Row ");

        }

        private void AwakeShowLineButton()
        {
            m_visibilityToggle = new UIHelperExtension(m_createdTitleLine).AddCheckboxNoLabel("LineVisibility");
            ((UISprite)m_visibilityToggle.checkedBoxObject).spriteName = "LineVisibilityToggleOn";
            ((UISprite)m_visibilityToggle.checkedBoxObject).tooltipLocaleID = "PUBLICTRANSPORT_HIDELINE";
            ((UISprite)m_visibilityToggle.checkedBoxObject).isTooltipLocalized = true;

            ((UISprite)m_visibilityToggle.checkedBoxObject).size = new Vector2(24, 24);
            ((UISprite)m_visibilityToggle.components[0]).spriteName = "LineVisibilityToggleOff";
            ((UISprite)m_visibilityToggle.components[0]).tooltipLocaleID = "PUBLICTRANSPORT_SHOWLINE";
            ((UISprite)m_visibilityToggle.components[0]).isTooltipLocalized = true;
            ((UISprite)m_visibilityToggle.components[0]).size = new Vector2(24, 24);
            m_visibilityToggle.relativePosition = new Vector3(20, 10);
            m_visibilityToggle.isChecked = true;

            m_visibilityToggle.eventCheckChanged += ToggleAllLinesVisibility;
        }


        private void LineName_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.NAME ? !m_reverseOrder : false;
            m_lastSortCriterionLines = LineSortCriterion.NAME;
            RefreshLines();
        }

        private void Passengers_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.PASSENGER ? !m_reverseOrder : true;
            m_lastSortCriterionLines = LineSortCriterion.PASSENGER;
            RefreshLines();
        }
        private void Profit_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.PROFIT ? !m_reverseOrder : true;
            m_lastSortCriterionLines = LineSortCriterion.PROFIT;
            RefreshLines();
        }

        private void Vehicles_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.VEHICLE ? !m_reverseOrder : true;
            m_lastSortCriterionLines = LineSortCriterion.VEHICLE;
            RefreshLines();
        }

        private void Stops_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.STOP ? !m_reverseOrder : true;
            m_lastSortCriterionLines = LineSortCriterion.STOP;
            RefreshLines();
        }

        private void CodColor_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.LINE_NUMBER ? !m_reverseOrder : false;
            m_lastSortCriterionLines = LineSortCriterion.LINE_NUMBER;
            RefreshLines();
        }

        private void ToggleAllLinesVisibility(UIComponent component, bool value) =>
            Singleton<SimulationManager>.instance.AddAction(() =>
                                                                {
                                                                    foreach (UIComponent item in lineItems.items)
                                                                    {
                                                                        var comp = item.GetComponent<UVMLineListItem>();
                                                                        comp.ChangeLineVisibility(value);
                                                                    }
                                                                    m_isUpdated = false;
                                                                });
        #endregion



        public void RefreshLines()
        {
            m_visibilityToggle.area = new Vector4(8, 5, 28, 28);
            List<ushort> lines = TargetTsdLines();

            m_countLines.text = string.Format(Locale.Get("K45_TLM_TOTALIZERTEXT"), lines.Count - lines.Where(x => x == 0).Count(), TransportManager.instance.m_lineCount - 1);
            var newItems = lineItems.SetItemCount(lines.Count);
            if (lines.Count == 0)
            {
                return;
            }
            List<ushort> result;
            switch (m_lastSortCriterionLines)
            {
                case LineSortCriterion.NAME:
                    result = OnNameSort(lines);
                    break;
                case LineSortCriterion.PASSENGER:
                    result = OnPassengerSort(lines);
                    break;
                case LineSortCriterion.STOP:
                    result = OnStopSort(lines);
                    break;
                case LineSortCriterion.VEHICLE:
                    result = OnVehicleSort(lines);
                    break;
                case LineSortCriterion.PROFIT:
                    result = OnProfitSort(lines);
                    break;
                case LineSortCriterion.LINE_NUMBER:
                default:
                    result = OnLineNumberSort(lines);
                    break;
            }
            for (int i = 0; i < lineItems.items.Count; i++)
            {
                newItems[i].GetComponent<UVMLineListItem>().LineID = result[i];
            }

            m_isUpdated = true;
        }

        private List<ushort> TargetTsdLines()
        {
            List<ushort> lines = new List<ushort>();

            if (!(TSD.LevelIntercity is null))
            {
                lines.Add(0);
            }
            for (ushort lineID = 1; lineID < TransportManager.instance.m_lines.m_buffer.Length; lineID++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created && TSD.IsFromSystem(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID]))
                {
                    lines.Add(lineID);
                }
            }
            return lines;
        }

        #region Sorting

        private enum LineSortCriterion
        {
            NAME,
            STOP,
            VEHICLE,
            PASSENGER,
            PROFIT,
            LINE_NUMBER
        }
        private static int Compare(Tuple<ushort, string> left, Tuple<ushort, string> right) => string.Compare(left.Second, right.Second, StringComparison.InvariantCulture);
        private static int Compare(Tuple<ushort, long> left, Tuple<ushort, long> right) => left.Second.CompareTo(right.Second);
        private static int Compare(Tuple<ushort, int> left, Tuple<ushort, int> right) => left.Second.CompareTo(right.Second);


        private List<ushort> OnNameSort(List<ushort> lines) => SortingUtils.QuicksortList(lines
                .Select(x => Tuple.New(x, Singleton<TransportManager>.instance.GetLineName(x)))
                .ToList(),
                new Comparison<Tuple<ushort, string>>(Compare),
                m_reverseOrder).Select(x => x.First).ToList();


        private List<ushort> OnStopSort(List<ushort> lines) => SortingUtils.QuicksortList(lines
                .Select(x => Tuple.New(x, Singleton<TransportManager>.instance.m_lines.m_buffer[x].CountStops(x))).ToList(),
                new Comparison<Tuple<ushort, int>>(Compare),
                m_reverseOrder).Select(x => x.First).ToList();


        private List<ushort> OnVehicleSort(List<ushort> lines) => SortingUtils.QuicksortList(lines
                .Select(x => Tuple.New(x, Singleton<TransportManager>.instance.m_lines.m_buffer[x].CountVehicles(x))).ToList(),
                new Comparison<Tuple<ushort, int>>(Compare),
                m_reverseOrder).Select(x => x.First).ToList();

        private List<ushort> OnPassengerSort(List<ushort> lines) => SortingUtils.QuicksortList(lines
                .Select(x =>
                {
                    int averageCount = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[x].m_passengers.m_residentPassengers.m_averageCount;
                    int averageCount2 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[x].m_passengers.m_touristPassengers.m_averageCount;
                    return Tuple.New(x, averageCount + averageCount2);
                }).ToList(),
                new Comparison<Tuple<ushort, int>>(Compare),
                m_reverseOrder).Select(x => x.First).ToList();

        private List<ushort> OnLineNumberSort(List<ushort> lines) => SortingUtils.QuicksortList(lines
                .Select(x => Tuple.New(x, TLMLineUtils.GetLineStringId(x, false)))
                .ToList(),
                new Comparison<Tuple<ushort, string>>(Compare),
                m_reverseOrder).Select(x => x.First).ToList();

        private List<ushort> OnProfitSort(List<ushort> lines) => SortingUtils.QuicksortList(lines
                .Select(x =>
                {
                    TLMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForLine(x, out long income, out long expense);
                    return Tuple.New(x, income - expense);
                }).ToList(),
                new Comparison<Tuple<ushort, long>>(Compare),
                m_reverseOrder).Select(x => x.First).ToList();
        #endregion

    }

}
