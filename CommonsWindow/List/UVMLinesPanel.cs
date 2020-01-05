using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow
{

    internal class UVMLinesPanel<T> : UICustomControl where T : TLMSysDef<T>
    {
        public UIPanel MainContainer { get; private set; }

        private UICheckBox m_visibilityToggle;
        private LineSortCriterion m_lastSortCriterionLines;
        private bool m_reverseOrder = false;
        private UIButton m_autoNameAll;
        private UIButton m_autoColorAll;
        private int m_lastLineCount;

        private bool m_pendentCreateViewToggleButton = true;
        private UIPanel m_createdTitleLine;


        protected UIScrollablePanel mainPanel;
        protected UIPanel titleLine;
        public bool IsUpdated { get; set; }

        #region Awake
        protected void Awake()
        {

            MainContainer = GetComponent<UIPanel>();

            CreateTitleRow(out titleLine, MainContainer);


            KlyteMonoUtils.CreateScrollPanel(MainContainer, out mainPanel, out UIScrollbar scrollbar, MainContainer.width - 30, MainContainer.height - 50, new Vector3(5, 40));
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            mainPanel.eventVisibilityChanged += OnToggleVisible;
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
            if (!mainPanel.isVisible)
            {
                return;
            }
            if (Singleton<TransportManager>.exists && m_lastLineCount != Singleton<TransportManager>.instance.m_lineCount)
            {
                RefreshLines();
                m_lastLineCount = Singleton<TransportManager>.instance.m_lineCount;
            }
            if (!IsUpdated)
            {
                RefreshLines();
            }
            DoOnUpdate();

        }

        protected void RemoveExtraLines(int linesCount)
        {
            while (mainPanel.components.Count > linesCount)
            {
                UIComponent uIComponent = mainPanel.components[linesCount];
                mainPanel.RemoveUIComponent(uIComponent);
                Destroy(uIComponent.gameObject);
            }
        }

        #region Sorting

        protected static int NaturalCompare(string left, string right) => SortingUtils.NaturalCompare(left, right);



        protected static void Quicksort(IList<UIComponent> elements, Comparison<UIComponent> comp, bool invert) => SortingUtils.Quicksort(elements, comp, invert);

        #endregion

        #region Awake
        protected void Start()
        {
            m_lastSortCriterionLines = LineSortCriterion.DEFAULT;

            UIComponent parent = GetComponent<UIComponent>();
            KlyteMonoUtils.CreateUIElement(out m_autoNameAll, parent.transform);
            m_autoNameAll.relativePosition = new Vector3(parent.width - 40f, 0);
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
                foreach (UVMLineListItem item in mainPanel.GetComponentsInChildren<UVMLineListItem>())
                {
                    item.DoAutoName();
                }
            };

            KlyteMonoUtils.CreateUIElement(out m_autoColorAll, parent.transform);
            m_autoColorAll.relativePosition = new Vector3(parent.width - 80f, 0);
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
                foreach (UVMLineListItem item in mainPanel.GetComponentsInChildren<UVMLineListItem>())
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
            TryCreateVisibilityToggleButton();

            KlyteMonoUtils.CreateUIElement(out UILabel codColor, titleLine.transform, "codColor");
            codColor.minimumSize = new Vector2(60, 0);
            codColor.area = new Vector4(80, 10, codColor.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(codColor, (uint) codColor.width);
            codColor.textAlignment = UIHorizontalAlignment.Center;
            codColor.prefix = Locale.Get("PUBLICTRANSPORT_LINECOLOR");
            codColor.text = "/";
            codColor.suffix = Locale.Get("K45_TLM_CODE_SHORT");
            codColor.eventClicked += CodColor_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel lineName, titleLine.transform, "lineName");
            lineName.minimumSize = new Vector2(200, 0);
            lineName.area = new Vector4(140, 10, lineName.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(lineName, (uint) lineName.width);
            lineName.textAlignment = UIHorizontalAlignment.Center;
            lineName.text = Locale.Get("PUBLICTRANSPORT_LINENAME");
            lineName.eventClicked += LineName_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel stops, titleLine.transform, "stops");
            stops.minimumSize = new Vector2(80, 0);
            stops.area = new Vector4(340, 10, stops.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(stops, (uint) stops.width);
            stops.textAlignment = UIHorizontalAlignment.Center;
            stops.text = Locale.Get("PUBLICTRANSPORT_LINESTOPS");
            stops.eventClicked += Stops_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel vehicles, titleLine.transform, "vehicles");
            vehicles.minimumSize = new Vector2(110, 0);
            vehicles.area = new Vector4(430, 10, vehicles.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(vehicles, (uint) vehicles.width);
            vehicles.textAlignment = UIHorizontalAlignment.Center;
            vehicles.text = Locale.Get("PUBLICTRANSPORT_VEHICLES");
            vehicles.eventClicked += Vehicles_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel passengers, titleLine.transform, "passengers");
            passengers.minimumSize = new Vector2(80, 0);
            passengers.area = new Vector4(540, 10, passengers.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(passengers, (uint) passengers.width);
            passengers.textAlignment = UIHorizontalAlignment.Center;
            passengers.text = Locale.Get("PUBLICTRANSPORT_PASSENGERS");
            passengers.eventClicked += Passengers_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel profitLW, titleLine.transform, "profit");
            profitLW.minimumSize = new Vector2(80, 0);
            profitLW.area = new Vector4(625, 10, profitLW.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidthAndBox(profitLW, (uint) profitLW.width);
            profitLW.textAlignment = UIHorizontalAlignment.Center;
            profitLW.text = Locale.Get("K45_TLM_BALANCE_LAST_WEEK");
            profitLW.eventClicked += Profit_eventClicked;

            LogUtils.DoLog("End creating Title Row ");

        }

        private void TryCreateVisibilityToggleButton()
        {
            if (m_pendentCreateViewToggleButton)
            {
                try
                {
                    m_visibilityToggle = Instantiate(FindObjectOfType<UIView>().FindUIComponent<UICheckBox>("LineVisible"));
                    m_visibilityToggle.transform.SetParent(m_createdTitleLine.transform);
                    m_visibilityToggle.eventCheckChanged += ToggleAllLinesVisibility;
                    m_pendentCreateViewToggleButton = false;
                }
                catch
                {
                }
            }
        }

        protected void DoOnUpdate() => TryCreateVisibilityToggleButton();

        private void LineName_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.NAME ? !m_reverseOrder : false;
            m_lastSortCriterionLines = LineSortCriterion.NAME;
            RefreshLines();
        }

        private void Passengers_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.PASSENGER ? !m_reverseOrder : false;
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
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.VEHICLE ? !m_reverseOrder : false;
            m_lastSortCriterionLines = LineSortCriterion.VEHICLE;
            RefreshLines();
        }

        private void Stops_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.STOP ? !m_reverseOrder : false;
            m_lastSortCriterionLines = LineSortCriterion.STOP;
            RefreshLines();
        }

        private void CodColor_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_reverseOrder = m_lastSortCriterionLines == LineSortCriterion.LINE_NUMBER ? !m_reverseOrder : false;
            m_lastSortCriterionLines = LineSortCriterion.LINE_NUMBER;
            RefreshLines();
        }

        private void ToggleAllLinesVisibility(UIComponent component, bool value)
        {
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                foreach (UIComponent item in mainPanel.components)
                {
                    var comp = (UVMLineListItem) item.GetComponent(ImplClassChildren);
                    comp.ChangeLineVisibility(value);
                }
                IsUpdated = false;
            });
        }
        #endregion

        private void AddToList(ushort lineID, ref int count)
        {
            UVMLineListItem lineInfoItem;
            Type implClassBuildingLine = ImplClassChildren;
            if (count >= mainPanel.components.Count)
            {
                var temp = new GameObject();
                temp.AddComponent<UIPanel>();
                lineInfoItem = (UVMLineListItem) temp.AddComponent(implClassBuildingLine);
                mainPanel.AttachUIComponent(lineInfoItem.gameObject);
            }
            else
            {
                lineInfoItem = (UVMLineListItem) mainPanel.components[count].GetComponent(implClassBuildingLine);
            }
            lineInfoItem.LineID = lineID;
            lineInfoItem.RefreshData(true, true);
            count++;
        }

        private static Type ImplClassChildren => typeof(UVMLineListItem);
        private readonly TransportSystemDefinition m_sysDef = TLMSysDef<T>.instance.GetTSD();
        public void RefreshLines()
        {
            try
            {
                TryCreateVisibilityToggleButton();
                m_visibilityToggle.area = new Vector4(8, 5, 28, 28);

                int count = 0;
                for (ushort lineID = 1; lineID < TransportManager.instance.m_lines.m_buffer.Length; lineID++)
                {
                    if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created && m_sysDef.IsFromSystem(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID]))
                    {
                        AddToList(lineID, ref count);
                    }

                }
                RemoveExtraLines(count);

                switch (m_lastSortCriterionLines)
                {
                    case LineSortCriterion.NAME:
                        OnNameSort();
                        break;
                    case LineSortCriterion.PASSENGER:
                        OnPassengerSort();
                        break;
                    case LineSortCriterion.STOP:
                        OnStopSort();
                        break;
                    case LineSortCriterion.VEHICLE:
                        OnVehicleSort();
                        break;
                    case LineSortCriterion.PROFIT:
                        OnProfitSort();
                        break;
                    case LineSortCriterion.LINE_NUMBER:
                    default:
                        OnLineNumberSort();
                        break;
                }
            }
            catch { }
            IsUpdated = true;
        }

        #region Sorting

        private enum LineSortCriterion
        {
            DEFAULT,
            NAME,
            STOP,
            VEHICLE,
            PASSENGER,
            PROFIT,
            LINE_NUMBER
        }

        private static int CompareNames(UIComponent left, UIComponent right)
        {
            UVMLineListItem component = left.GetComponent<UVMLineListItem>();
            UVMLineListItem component2 = right.GetComponent<UVMLineListItem>();
            return string.Compare(component.LineName, component2.LineName, StringComparison.InvariantCulture);
        }

        private static int CompareProfit(UIComponent left, UIComponent right)
        {
            if (left == null || right == null)
            {
                return 0;
            }

            UVMLineListItem component = left.GetComponent<UVMLineListItem>();
            UVMLineListItem component2 = right.GetComponent<UVMLineListItem>();
            if (component == null || component2 == null)
            {
                return 0;
            }
            TLMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForLine(component.LineID, out long income, out long expense);
            long profit1 = income - expense;
            TLMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForLine(component2.LineID, out income, out expense);
            long profit2 = income - expense;
            return profit1.CompareTo(profit2);

        }

        private static int CompareLineNumbers(UIComponent left, UIComponent right)
        {
            if (left == null || right == null)
            {
                return 0;
            }

            UVMLineListItem component = left.GetComponent<UVMLineListItem>();
            UVMLineListItem component2 = right.GetComponent<UVMLineListItem>();
            if (component == null || component2 == null)
            {
                return 0;
            }
            var tsd = TransportSystemDefinition.From(component.LineID);
            var tsd2 = TransportSystemDefinition.From(component2.LineID);
            if (tsd == tsd2)
            {
                return component.LineNumber.CompareTo(component2.LineNumber);
            }
            else
            {
                return tsd.GetHashCode().CompareTo(tsd2.GetHashCode());
            }
        }

        private static int CompareStops(UIComponent left, UIComponent right)
        {
            UVMLineListItem component = left.GetComponent<UVMLineListItem>();
            UVMLineListItem component2 = right.GetComponent<UVMLineListItem>();
            return component2.StopCounts.CompareTo(component.StopCounts);
        }

        private static int CompareVehicles(UIComponent left, UIComponent right)
        {
            UVMLineListItem component = left.GetComponent<UVMLineListItem>();
            UVMLineListItem component2 = right.GetComponent<UVMLineListItem>();
            return component2.VehicleCounts.CompareTo(component.VehicleCounts);
        }

        private static int ComparePassengers(UIComponent left, UIComponent right)
        {
            UVMLineListItem component = left.GetComponent<UVMLineListItem>();
            UVMLineListItem component2 = right.GetComponent<UVMLineListItem>();
            return component2.PassengerCountsInt.CompareTo(component.PassengerCountsInt);
        }

        private void OnNameSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareNames), m_reverseOrder);
            m_lastSortCriterionLines = LineSortCriterion.NAME;
            mainPanel.Invalidate();
        }

        private void OnStopSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareStops), m_reverseOrder);
            m_lastSortCriterionLines = LineSortCriterion.STOP;
            mainPanel.Invalidate();
        }

        private void OnVehicleSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareVehicles), m_reverseOrder);
            m_lastSortCriterionLines = LineSortCriterion.VEHICLE;
            mainPanel.Invalidate();
        }

        private void OnPassengerSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(ComparePassengers), m_reverseOrder);
            m_lastSortCriterionLines = LineSortCriterion.PASSENGER;
            mainPanel.Invalidate();
        }

        private void OnLineNumberSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareLineNumbers), m_reverseOrder);
            m_lastSortCriterionLines = LineSortCriterion.LINE_NUMBER;
            mainPanel.Invalidate();
        }
        private void OnProfitSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareProfit), m_reverseOrder);
            m_lastSortCriterionLines = LineSortCriterion.PROFIT;
            mainPanel.Invalidate();
        }
        #endregion

    }

    internal sealed class UVMLinesPanelNorBus : UVMLinesPanel<TLMSysDefNorBus> { }
    internal sealed class UVMLinesPanelNorTrm : UVMLinesPanel<TLMSysDefNorTrm> { }
    internal sealed class UVMLinesPanelNorMnr : UVMLinesPanel<TLMSysDefNorMnr> { }
    internal sealed class UVMLinesPanelNorMet : UVMLinesPanel<TLMSysDefNorMet> { }
    internal sealed class UVMLinesPanelNorTrn : UVMLinesPanel<TLMSysDefNorTrn> { }
    internal sealed class UVMLinesPanelNorFer : UVMLinesPanel<TLMSysDefNorFer> { }
    internal sealed class UVMLinesPanelNorBlp : UVMLinesPanel<TLMSysDefNorBlp> { }
    internal sealed class UVMLinesPanelNorShp : UVMLinesPanel<TLMSysDefNorShp> { }
    internal sealed class UVMLinesPanelNorPln : UVMLinesPanel<TLMSysDefNorPln> { }
    internal sealed class UVMLinesPanelEvcBus : UVMLinesPanel<TLMSysDefEvcBus> { }
    internal sealed class UVMLinesPanelTouBus : UVMLinesPanel<TLMSysDefTouBus> { }
    internal sealed class UVMLinesPanelTouPed : UVMLinesPanel<TLMSysDefTouPed> { }

}
