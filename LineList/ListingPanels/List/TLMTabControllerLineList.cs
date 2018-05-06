using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using static Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.UI
{
    internal abstract class TLMTabControllerLineHooks<T, V> : Redirector<T> where T : TLMTabControllerLineHooks<T, V> where V : TLMSysDef<V>
    {
        private static TLMTabControllerLineHooks<T, V> instance;

        public static void AfterCreateLine(bool __result, ushort lineID, BuildingInfo info)
        {
            if (lineID == 0) return;
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
        public static void AfterReleaseLine(bool __result, ref bool __state)
        {
            if (__result && __state && TLMBasicTabControllerLineList<V>.exists)
            {
                TLMBasicTabControllerLineList<V>.instance.isUpdated = false;
            }
        }

        public override void AwakeBody()
        {
            instance = this;
            TransportSystemDefinition def = Singleton<V>.instance.GetTSD();

            var from = typeof(TransportManager).GetMethod("CreateLine", allFlags);
            var to = typeof(TLMTabControllerLineHooks<T, V>).GetMethod("AfterCreateLine", allFlags);
            var from2 = typeof(TransportManager).GetMethod("ReleaseLine", allFlags);
            var to2b = typeof(TLMTabControllerLineHooks<T, V>).GetMethod("BeforeReleaseLine", allFlags);
            var to2a = typeof(TLMTabControllerLineHooks<T, V>).GetMethod("AfterReleaseLine", allFlags);
            TLMUtils.doLog("Loading After Hooks: {0} ({1}=>{2})", typeof(BuildingManager), from, to);
            TLMUtils.doLog("Loading After & Before Hooks: {0} ({1}=>{2}+{3})", typeof(BuildingManager), from2, to2a, to2b);
            AddRedirect(from, null, to);
            AddRedirect(from2, to2a, to2b);
        }

        public override void doLog(string text, params object[] param)
        {
            TLMUtils.doLog(text, param);
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

        protected override void OnUpdateStateChange(bool state) { }
        protected override bool HasRegionalPrefixFilter => false;

        #region Awake
        protected override void Awake()
        {
            base.Awake();
            m_LastSortCriterionLines = LineSortCriterion.DEFAULT;
        }
        #endregion

        #region title row
        protected override void CreateTitleRow(out UIPanel titleLine, UIComponent parent)
        {
            TLMUtils.createUIElement(out titleLine, parent.transform, "TLMtitleline", new Vector4(5, 0, parent.width - 10, 40));

            m_visibilityToggle = Instantiate(FindObjectOfType<UIView>().FindUIComponent<UICheckBox>("LineVisible"));
            m_visibilityToggle.transform.SetParent(titleLine.transform);
            m_visibilityToggle.eventCheckChanged += toggleAllLinesVisibility;

            TLMUtils.createUIElement(out UILabel codColor, titleLine.transform, "codColor");
            codColor.minimumSize = new Vector2(60, 0);
            codColor.area = new Vector4(80, 10, codColor.minimumSize.x, 18);
            TLMUtils.LimitWidth(codColor, (uint)codColor.width);
            codColor.textAlignment = UIHorizontalAlignment.Center;
            codColor.prefix = Locale.Get("PUBLICTRANSPORT_LINECOLOR");
            codColor.text = "/";
            codColor.suffix = Locale.Get("TLM_CODE_SHORT");
            codColor.eventClicked += CodColor_eventClicked;

            TLMUtils.createUIElement(out UILabel lineName, titleLine.transform, "lineName");
            lineName.minimumSize = new Vector2(200, 0);
            lineName.area = new Vector4(140, 10, lineName.minimumSize.x, 18);
            TLMUtils.LimitWidth(lineName, (uint)lineName.width);
            lineName.textAlignment = UIHorizontalAlignment.Center;
            lineName.text = Locale.Get("PUBLICTRANSPORT_LINENAME");
            lineName.eventClicked += LineName_eventClicked; ;

            TLMUtils.createUIElement(out UILabel stops, titleLine.transform, "stops");
            stops.minimumSize = new Vector2(80, 0);
            stops.area = new Vector4(340, 10, stops.minimumSize.x, 18);
            TLMUtils.LimitWidth(stops, (uint)stops.width);
            stops.textAlignment = UIHorizontalAlignment.Center;
            stops.text = Locale.Get("PUBLICTRANSPORT_LINESTOPS");
            stops.eventClicked += Stops_eventClicked; ;

            if (Singleton<T>.instance.GetTSD().hasVehicles())
            {
                TLMUtils.createUIElement(out UILabel vehicles, titleLine.transform, "vehicles");
                vehicles.minimumSize = new Vector2(110, 0);
                vehicles.area = new Vector4(430, 10, vehicles.minimumSize.x, 18);
                TLMUtils.LimitWidth(vehicles, (uint)vehicles.width);
                vehicles.textAlignment = UIHorizontalAlignment.Center;
                vehicles.text = Locale.Get("PUBLICTRANSPORT_VEHICLES");
                vehicles.eventClicked += Vehicles_eventClicked; ;
            }

            TLMUtils.createUIElement(out UILabel passengers, titleLine.transform, "passengers");
            passengers.minimumSize = new Vector2(80, 0);
            passengers.area = new Vector4(540, 10, passengers.minimumSize.x, 18);
            TLMUtils.LimitWidth(passengers, (uint)passengers.width);
            passengers.textAlignment = UIHorizontalAlignment.Center;
            passengers.text = Locale.Get("PUBLICTRANSPORT_PASSENGERS");
            passengers.eventClicked += Passengers_eventClicked;

            AwakeDayNightOptions();
            AwakePrefixFilter();
        }

        private void LineName_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.NAME ? !reverseOrder : false;
            RefreshLines();
        }

        private void Passengers_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.PASSENGER ? !reverseOrder : false;
            RefreshLines();
        }

        private void Vehicles_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.VEHICLE ? !reverseOrder : false;
            RefreshLines();
        }

        private void Stops_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.STOP ? !reverseOrder : false;
            RefreshLines();
        }

        private void CodColor_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterionLines == LineSortCriterion.LINE_NUMBER ? !reverseOrder : false;
            RefreshLines();
        }

        private void AwakeDayNightOptions()
        {
            var temp = GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("LineTitle");
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
            foreach (var item in mainPanel.components)
            {
                TLMLineListItem<T> comp = (TLMLineListItem<T>)item.GetComponent(ImplClassChildren);
                comp.ChangeLineVisibility(value);
            }
            isUpdated = false;
        }
        #endregion

        private void AddToList(ushort lineID, ref int count)
        {
            TLMLineListItem<T> lineInfoItem;
            Type implClassBuildingLine = ImplClassChildren;
            if (count >= mainPanel.components.Count)
            {
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                lineInfoItem = (TLMLineListItem<T>)temp.AddComponent(implClassBuildingLine);
                mainPanel.AttachUIComponent(lineInfoItem.gameObject);
            }
            else
            {
                lineInfoItem = (TLMLineListItem<T>)mainPanel.components[count].GetComponent(implClassBuildingLine);
            }
            lineInfoItem.lineID = lineID;
            lineInfoItem.RefreshData(true, true);
            count++;
        }

        private static Type ImplClassChildren => TLMUtils.GetImplementationForGenericType(typeof(TLMLineListItem<>), typeof(T));

        protected override void RefreshLines()
        {

            m_DayIcon.relativePosition = new Vector3(655, 14);
            m_NightIcon.relativePosition = new Vector3(682, 14);
            m_DayNightIcon.relativePosition = new Vector3(701, 14);
            m_visibilityToggle.area = new Vector4(8, 5, 28, 28);

            var tsd = Singleton<T>.instance.GetTSD();
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
                case LineSortCriterion.NAME: OnNameSort(); break;
                case LineSortCriterion.PASSENGER: OnPassengerSort(); break;
                case LineSortCriterion.STOP: OnStopSort(); break;
                case LineSortCriterion.VEHICLE: OnVehicleSort(); break;
                case LineSortCriterion.LINE_NUMBER: default: OnLineNumberSort(); break;
            }
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
                return 0;
            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            if (component == null || component2 == null)
                return 0;
            return component.lineNumber.CompareTo(component2.lineNumber);
        }

        private static int CompareStops(UIComponent left, UIComponent right)
        {
            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            return NaturalCompare(component2.stopCounts, component.stopCounts);
        }

        private static int CompareVehicles(UIComponent left, UIComponent right)
        {
            TLMLineListItem<T> component = left.GetComponent<TLMLineListItem<T>>();
            TLMLineListItem<T> component2 = right.GetComponent<TLMLineListItem<T>>();
            return NaturalCompare(component2.vehicleCounts, component.vehicleCounts);
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
                return;
            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareNames), reverseOrder);
            this.m_LastSortCriterionLines = LineSortCriterion.NAME;
            mainPanel.Invalidate();
        }

        private void OnStopSort()
        {
            if (mainPanel.components.Count == 0)
                return;
            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareStops), reverseOrder);
            this.m_LastSortCriterionLines = LineSortCriterion.STOP;
            mainPanel.Invalidate();
        }

        private void OnVehicleSort()
        {
            if (mainPanel.components.Count == 0)
                return;
            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareVehicles), reverseOrder);
            this.m_LastSortCriterionLines = LineSortCriterion.VEHICLE;
            mainPanel.Invalidate();
        }

        private void OnPassengerSort()
        {
            if (mainPanel.components.Count == 0)
                return;
            Quicksort(mainPanel.components, new Comparison<UIComponent>(ComparePassengers), reverseOrder);
            this.m_LastSortCriterionLines = LineSortCriterion.PASSENGER;
            mainPanel.Invalidate();
        }

        private void OnLineNumberSort()
        {
            if (mainPanel.components.Count == 0)
                return;
            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareLineNumbers), reverseOrder);
            this.m_LastSortCriterionLines = LineSortCriterion.LINE_NUMBER;
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
