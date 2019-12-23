using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow.Components;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.CommonsWindow
{
    internal abstract class TLMTabControllerLineHooks<T, V> : IRedirectable where T : TLMTabControllerLineHooks<T, V> where V : TLMSysDef<V>
    {
        public Redirector RedirectorInstance => new Redirector();

        public static void AfterCreateLine(bool __result, ref ushort lineID, BuildingInfo info)
        {
            if (lineID == 0)
            {
                return;
            }

            TransportLine tl = TransportManager.instance.m_lines.m_buffer[lineID];
            if (__result && TLMBasicTabControllerLineList<V>.exists && (Singleton<V>.instance?.GetTSD().isFromSystem(tl) ?? false))
            {
                TLMBasicTabControllerLineList<V>.instance.isUpdated = false;
            }
        }
        public static void BeforeReleaseLine(ushort lineID, ref bool __state)
        {
            TransportLine tl = TransportManager.instance.m_lines.m_buffer[lineID];
            __state = lineID != 0 && Singleton<V>.instance.GetTSD().isFromSystem(tl);
        }
        public static void AfterReleaseLine(ref bool __state)
        {
            if (__state && TLMBasicTabControllerLineList<V>.exists)
            {
                TLMBasicTabControllerLineList<V>.instance.isUpdated = false;
            }
        }

        public void Awake()
        {

            System.Reflection.MethodInfo from = typeof(TransportManager).GetMethod("CreateLine", allFlags);
            System.Reflection.MethodInfo to = typeof(TLMTabControllerLineHooks<T, V>).GetMethod("AfterCreateLine", allFlags);
            System.Reflection.MethodInfo from2 = typeof(TransportManager).GetMethod("ReleaseLine", allFlags);
            System.Reflection.MethodInfo to2b = typeof(TLMTabControllerLineHooks<T, V>).GetMethod("BeforeReleaseLine", allFlags);
            System.Reflection.MethodInfo to2a = typeof(TLMTabControllerLineHooks<T, V>).GetMethod("AfterReleaseLine", allFlags);
            TLMUtils.doLog("Loading After Hooks: {0} ({1}=>{2})", typeof(TransportManager), from, to);
            RedirectorInstance.AddRedirect(from, null, to);
            TLMUtils.doLog("Loading After & Before Hooks: {0} ({1}=>{2}+{3})", typeof(TransportManager), from2, to2a, to2b);
            RedirectorInstance.AddRedirect(from2, to2a, to2b);
        }

    }

    internal sealed class TLMTabControllerLineHooksNorBus : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorBus, TLMSysDefNorBus> { }
    internal sealed class TLMTabControllerLineHooksEvcBus : TLMTabControllerLineHooks<TLMTabControllerLineHooksEvcBus, TLMSysDefEvcBus> { }
    internal sealed class TLMTabControllerLineHooksNorTrm : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorTrm, TLMSysDefNorTrm> { }
    internal sealed class TLMTabControllerLineHooksNorMnr : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorMnr, TLMSysDefNorMnr> { }
    internal sealed class TLMTabControllerLineHooksNorMet : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorMet, TLMSysDefNorMet> { }
    internal sealed class TLMTabControllerLineHooksNorTrn : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorTrn, TLMSysDefNorTrn> { }
    internal sealed class TLMTabControllerLineHooksNorFer : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorFer, TLMSysDefNorFer> { }
    internal sealed class TLMTabControllerLineHooksNorBlp : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorBlp, TLMSysDefNorBlp> { }
    internal sealed class TLMTabControllerLineHooksNorShp : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorShp, TLMSysDefNorShp> { }
    internal sealed class TLMTabControllerLineHooksNorPln : TLMTabControllerLineHooks<TLMTabControllerLineHooksNorPln, TLMSysDefNorPln> { }
    internal sealed class TLMTabControllerLineHooksTouBus : TLMTabControllerLineHooks<TLMTabControllerLineHooksTouBus, TLMSysDefTouBus> { }
    internal sealed class TLMTabControllerLineHooksTouPed : TLMTabControllerLineHooks<TLMTabControllerLineHooksTouPed, TLMSysDefTouPed> { }




    internal abstract class TLMBasicTabControllerLineList<T> : TLMTabControllerListBase<T> where T : TLMSysDef<T>
    {


        private UICheckBox m_visibilityToggle;
        private LineSortCriterion m_LastSortCriterionLines;
        private bool reverseOrder = false;
        private UIButton m_DayIcon;
        private UIButton m_NightIcon;
        private UIButton m_DayNightIcon;
        private UIButton m_DisabledIcon;
        private UIButton m_autoNameAll;
        private UIButton m_autoColorAll;

        private bool pendentCreateViewToggleButton = true;
        private UIPanel createdTitleLine;

        protected override void OnUpdateStateChange(bool state) { }
        protected override bool HasRegionalPrefixFilter => false;

        #region Awake
        protected void Start()
        {
            m_LastSortCriterionLines = LineSortCriterion.DEFAULT;

            UIComponent parent = GetComponent<UIComponent>();
            KlyteMonoUtils.CreateUIElement(out m_autoNameAll, parent.transform);
            m_autoNameAll.relativePosition = new Vector3(parent.width - 60f, -80);
            m_autoNameAll.textScale = 0.6f;
            m_autoNameAll.width = 40;
            m_autoNameAll.height = 40;
            m_autoNameAll.tooltip = Locale.Get("K45_TLM_AUTO_NAME_ALL_TOOLTIP");
            KlyteMonoUtils.InitButton(m_autoNameAll, true, "ButtonMenu");
            m_autoNameAll.name = "AutoNameAll";
            m_autoNameAll.isVisible = true;
            m_autoNameAll.eventClick += (component, eventParam) =>
            {
                foreach (TLMLineListItem<T> item in mainPanel.GetComponentsInChildren<TLMLineListItem<T>>())
                {
                    item.DoAutoName();
                }
            };

            UISprite icon = m_autoNameAll.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoNameIcon);


            KlyteMonoUtils.CreateUIElement(out m_autoColorAll, parent.transform);
            m_autoColorAll.relativePosition = new Vector3(parent.width - 120f, -80);
            m_autoColorAll.textScale = 0.6f;
            m_autoColorAll.width = 40;
            m_autoColorAll.height = 40;
            m_autoColorAll.tooltip = Locale.Get("K45_TLM_AUTO_COLOR_ALL_TOOLTIP");
            KlyteMonoUtils.InitButton(m_autoColorAll, true, "ButtonMenu");
            m_autoColorAll.name = "AutoColorAll";
            m_autoColorAll.isVisible = true;
            m_autoColorAll.eventClick += (component, eventParam) =>
            {
                foreach (TLMLineListItem<T> item in mainPanel.GetComponentsInChildren<TLMLineListItem<T>>())
                {
                    item.DoAutoColor();
                }
            };

            icon = m_autoColorAll.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoColorIcon);
        }
        #endregion

        #region title row
        protected override void CreateTitleRow(out UIPanel titleLine, UIComponent parent)
        {
            TLMUtils.doLog("Creating Title Row " + typeof(T));
            KlyteMonoUtils.CreateUIElement(out titleLine, parent.transform, "TLMtitleline", new Vector4(5, 0, parent.width - 10, 40));
            createdTitleLine = titleLine;
            TryCreateVisibilityToggleButton();

            KlyteMonoUtils.CreateUIElement(out UILabel codColor, titleLine.transform, "codColor");
            codColor.minimumSize = new Vector2(60, 0);
            codColor.area = new Vector4(80, 10, codColor.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidth(codColor, (uint) codColor.width);
            codColor.textAlignment = UIHorizontalAlignment.Center;
            codColor.prefix = Locale.Get("PUBLICTRANSPORT_LINECOLOR");
            codColor.text = "/";
            codColor.suffix = Locale.Get("K45_TLM_CODE_SHORT");
            codColor.eventClicked += CodColor_eventClicked;

            KlyteMonoUtils.CreateUIElement(out UILabel lineName, titleLine.transform, "lineName");
            lineName.minimumSize = new Vector2(200, 0);
            lineName.area = new Vector4(140, 10, lineName.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidth(lineName, (uint) lineName.width);
            lineName.textAlignment = UIHorizontalAlignment.Center;
            lineName.text = Locale.Get("PUBLICTRANSPORT_LINENAME");
            lineName.eventClicked += LineName_eventClicked;
            ;

            KlyteMonoUtils.CreateUIElement(out UILabel stops, titleLine.transform, "stops");
            stops.minimumSize = new Vector2(80, 0);
            stops.area = new Vector4(340, 10, stops.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidth(stops, (uint) stops.width);
            stops.textAlignment = UIHorizontalAlignment.Center;
            stops.text = Locale.Get("PUBLICTRANSPORT_LINESTOPS");
            stops.eventClicked += Stops_eventClicked;
            ;

            if (Singleton<T>.instance.GetTSD().hasVehicles())
            {
                KlyteMonoUtils.CreateUIElement(out UILabel vehicles, titleLine.transform, "vehicles");
                vehicles.minimumSize = new Vector2(110, 0);
                vehicles.area = new Vector4(430, 10, vehicles.minimumSize.x, 18);
                KlyteMonoUtils.LimitWidth(vehicles, (uint) vehicles.width);
                vehicles.textAlignment = UIHorizontalAlignment.Center;
                vehicles.text = Locale.Get("PUBLICTRANSPORT_VEHICLES");
                vehicles.eventClicked += Vehicles_eventClicked;
                ;
            }

            KlyteMonoUtils.CreateUIElement(out UILabel passengers, titleLine.transform, "passengers");
            passengers.minimumSize = new Vector2(80, 0);
            passengers.area = new Vector4(540, 10, passengers.minimumSize.x, 18);
            KlyteMonoUtils.LimitWidth(passengers, (uint) passengers.width);
            passengers.textAlignment = UIHorizontalAlignment.Center;
            passengers.text = Locale.Get("PUBLICTRANSPORT_PASSENGERS");
            passengers.eventClicked += Passengers_eventClicked;

            AwakeDayNightOptions();
            AwakePrefixFilter();
            TLMUtils.doLog("End creating Title Row " + typeof(T));

        }

        private void TryCreateVisibilityToggleButton()
        {
            if (pendentCreateViewToggleButton)
            {
                try
                {
                    m_visibilityToggle = Instantiate(FindObjectOfType<UIView>().FindUIComponent<UICheckBox>("LineVisible"));
                    m_visibilityToggle.transform.SetParent(createdTitleLine.transform);
                    m_visibilityToggle.eventCheckChanged += toggleAllLinesVisibility;
                    pendentCreateViewToggleButton = false;
                }
                catch
                {
                }
            }
        }

        protected override void DoOnUpdate() => TryCreateVisibilityToggleButton();

        private void LineName_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.NAME ? !reverseOrder : false;
            m_LastSortCriterionLines = LineSortCriterion.NAME;
            RefreshLines();
        }

        private void Passengers_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.PASSENGER ? !reverseOrder : false;
            m_LastSortCriterionLines = LineSortCriterion.PASSENGER;
            RefreshLines();
        }

        private void Vehicles_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.VEHICLE ? !reverseOrder : false;
            m_LastSortCriterionLines = LineSortCriterion.VEHICLE;
            RefreshLines();
        }

        private void Stops_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.STOP ? !reverseOrder : false;
            m_LastSortCriterionLines = LineSortCriterion.STOP;
            RefreshLines();
        }

        private void CodColor_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.LINE_NUMBER ? !reverseOrder : false;
            m_LastSortCriterionLines = LineSortCriterion.LINE_NUMBER;
            RefreshLines();
        }

        private void AwakeDayNightOptions()
        {
            UIPanel temp = GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("LineTitle");
            TLMUtils.doLog("Find Original buttons");
            m_DayIcon = GameObject.Instantiate(temp.Find<UIButton>("DayButton"));
            m_NightIcon = GameObject.Instantiate(temp.Find<UIButton>("NightButton"));
            m_DayNightIcon = GameObject.Instantiate(temp.Find<UIButton>("DayNightButton"));

            m_DayIcon.transform.SetParent(titleLine.transform);
            m_NightIcon.transform.SetParent(titleLine.transform);
            m_DayNightIcon.transform.SetParent(titleLine.transform);

            TLMUtils.doLog("Create disabled button");
            m_DisabledIcon = GameObject.Instantiate(m_DayIcon.gameObject, m_DayIcon.transform.parent).GetComponent<UIButton>();
            m_DisabledIcon.transform.SetParent(titleLine.transform);
            m_DisabledIcon.normalBgSprite = "Niet";
            m_DisabledIcon.hoveredBgSprite = "Niet";
            m_DisabledIcon.pressedBgSprite = "Niet";
            m_DisabledIcon.disabledBgSprite = "Niet";
            m_DisabledIcon.relativePosition = new Vector3(733, 14);
        }

        private void toggleAllLinesVisibility(UIComponent component, bool value)
        {
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                foreach (UIComponent item in mainPanel.components)
                {
                    var comp = (TLMLineListItem<T>) item.GetComponent(ImplClassChildren);
                    comp.ChangeLineVisibility(value);
                }
                isUpdated = false;
            });
        }
        #endregion

        private void AddToList(ushort lineID, ref int count)
        {
            TLMLineListItem<T> lineInfoItem;
            Type implClassBuildingLine = ImplClassChildren;
            if (count >= mainPanel.components.Count)
            {
                var temp = new GameObject();
                temp.AddComponent<UIPanel>();
                lineInfoItem = (TLMLineListItem<T>) temp.AddComponent(implClassBuildingLine);
                mainPanel.AttachUIComponent(lineInfoItem.gameObject);
            }
            else
            {
                lineInfoItem = (TLMLineListItem<T>) mainPanel.components[count].GetComponent(implClassBuildingLine);
            }
            lineInfoItem.lineID = lineID;
            lineInfoItem.RefreshData(true, true);
            count++;
        }

        private static Type ImplClassChildren => ReflectionUtils.GetImplementationForGenericType(typeof(TLMLineListItem<>), typeof(T));

        public override void RefreshLines()
        {
            try
            {
                TryCreateVisibilityToggleButton();
                m_DayIcon.relativePosition = new Vector3(655, 14);
                m_NightIcon.relativePosition = new Vector3(682, 14);
                m_DayNightIcon.relativePosition = new Vector3(701, 14);
                m_visibilityToggle.area = new Vector4(8, 5, 28, 28);

                TransportSystemDefinition tsd = Singleton<T>.instance.GetTSD();
                bool hasPrefix = TLMLineUtils.hasPrefix(ref tsd);
                int count = 0;
                for (ushort lineID = 1; lineID < TransportManager.instance.m_lines.m_buffer.Length; lineID++)
                {
                    TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
                    if (tl.Complete && Singleton<T>.instance.GetTSD().isFromSystem(tl) && (!hasPrefix || m_prefixFilter.selectedIndex == 0 || m_prefixFilter.selectedIndex - 1 == TLMLineUtils.getPrefix(lineID)))
                    {
                        AddToList(lineID, ref count);
                    }

                }
                RemoveExtraLines(count);

                switch (m_LastSortCriterionLines)
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
                    case LineSortCriterion.LINE_NUMBER:
                    default:
                        OnLineNumberSort();
                        break;
                }
            }
            catch { }
            isUpdated = true;
        }

        #region Sorting

        private enum LineSortCriterion
        {
            DEFAULT,
            NAME,
            STOP,
            VEHICLE,
            PASSENGER,
            LINE_NUMBER
        }

        private static int CompareNames(UIComponent left, UIComponent right)
        {
            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            return string.Compare(component.lineName, component2.lineName, StringComparison.InvariantCulture);
        }

        private static int CompareLineNumbers(UIComponent left, UIComponent right)
        {
            if (left == null || right == null)
            {
                return 0;
            }

            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            if (component == null || component2 == null)
            {
                return 0;
            }

            return component.lineNumber.CompareTo(component2.lineNumber);
        }

        private static int CompareStops(UIComponent left, UIComponent right)
        {
            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            return component2.stopCounts.CompareTo(component.stopCounts);
        }

        private static int CompareVehicles(UIComponent left, UIComponent right)
        {
            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            return component2.vehicleCounts.CompareTo(component.vehicleCounts);
        }

        private static int ComparePassengers(UIComponent left, UIComponent right)
        {
            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            return component2.passengerCountsInt.CompareTo(component.passengerCountsInt);
        }

        private void OnNameSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareNames), reverseOrder);
            m_LastSortCriterionLines = LineSortCriterion.NAME;
            mainPanel.Invalidate();
        }

        private void OnStopSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareStops), reverseOrder);
            m_LastSortCriterionLines = LineSortCriterion.STOP;
            mainPanel.Invalidate();
        }

        private void OnVehicleSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareVehicles), reverseOrder);
            m_LastSortCriterionLines = LineSortCriterion.VEHICLE;
            mainPanel.Invalidate();
        }

        private void OnPassengerSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(ComparePassengers), reverseOrder);
            m_LastSortCriterionLines = LineSortCriterion.PASSENGER;
            mainPanel.Invalidate();
        }

        private void OnLineNumberSort()
        {
            if (mainPanel.components.Count == 0)
            {
                return;
            }

            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareLineNumbers), reverseOrder);
            m_LastSortCriterionLines = LineSortCriterion.LINE_NUMBER;
            mainPanel.Invalidate();
        }
        #endregion

    }
    internal abstract class TLMTabControllerLineListTransport<T> : TLMBasicTabControllerLineList<T> where T : TLMSysDef<T> { };
    internal sealed class TLMTabControllerLineListNorBus : TLMTabControllerLineListTransport<TLMSysDefNorBus> { }
    internal sealed class TLMTabControllerLineListEvcBus : TLMTabControllerLineListTransport<TLMSysDefEvcBus> { }
    internal sealed class TLMTabControllerLineListNorTrm : TLMTabControllerLineListTransport<TLMSysDefNorTrm> { }
    internal sealed class TLMTabControllerLineListNorMnr : TLMTabControllerLineListTransport<TLMSysDefNorMnr> { }
    internal sealed class TLMTabControllerLineListNorMet : TLMTabControllerLineListTransport<TLMSysDefNorMet> { }
    internal sealed class TLMTabControllerLineListNorTrn : TLMTabControllerLineListTransport<TLMSysDefNorTrn> { }
    internal sealed class TLMTabControllerLineListNorFer : TLMTabControllerLineListTransport<TLMSysDefNorFer> { }
    internal sealed class TLMTabControllerLineListNorBlp : TLMTabControllerLineListTransport<TLMSysDefNorBlp> { }
    internal sealed class TLMTabControllerLineListNorShp : TLMTabControllerLineListTransport<TLMSysDefNorShp> { }
    internal sealed class TLMTabControllerLineListNorPln : TLMTabControllerLineListTransport<TLMSysDefNorPln> { }

    internal abstract class TLMTabControllerLineListTourism<T> : TLMBasicTabControllerLineList<T> where T : TLMSysDef<T> { };
    internal sealed class TLMTabControllerLineListTouBus : TLMTabControllerLineListTourism<TLMSysDefTouBus> { }
    internal sealed class TLMTabControllerLineListTouPed : TLMTabControllerLineListTourism<TLMSysDefTouPed> { }

}
