using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.CommonsWindow.Components;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow
{
    internal abstract class TLMTabControllerDepotHooks<T, V> : Redirector<T> where T : TLMTabControllerDepotHooks<T, V> where V : TLMSysDef<V>
    {

        public static void AfterCreateBuilding(bool __result,ref ushort building, BuildingInfo info)
        {
            if (building == 0) return;
            Building bd = BuildingManager.instance.m_buildings.m_buffer[building];
            if (__result && TLMTabControllerDepotList<V>.exists && Singleton<V>.instance.GetTSD().isFromSystem(bd.Info.GetAI() as DepotAI))
            {
                TLMTabControllerDepotList<V>.instance.isUpdated = false;
            }
        }
        public static void BeforeReleaseBuilding(ushort building, ref bool __state)
        {
            Building bd = BuildingManager.instance.m_buildings.m_buffer[building];
            __state = building != 0 && Singleton<V>.instance.GetTSD().isFromSystem(bd.Info.GetAI() as DepotAI);
        }
        public static void AfterReleaseBuilding(ref bool __state)
        {
            if (__state && TLMTabControllerDepotList<V>.exists)
            {
                TLMTabControllerDepotList<V>.instance.isUpdated = false;
            }
        }

        public override void AwakeBody()
        {
            var from = typeof(BuildingManager).GetMethod("CreateBuilding", allFlags);
            var to = typeof(TLMTabControllerDepotHooks<T, V>).GetMethod("AfterCreateBuilding", allFlags);
            var from2 = typeof(BuildingManager).GetMethod("ReleaseBuilding", allFlags);
            var to2b = typeof(TLMTabControllerDepotHooks<T, V>).GetMethod("BeforeReleaseBuilding", allFlags);
            var to2a = typeof(TLMTabControllerDepotHooks<T, V>).GetMethod("AfterReleaseBuilding", allFlags);
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

    internal sealed class TLMTabControllerDepotHooksNorBus : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorBus, TLMSysDefNorBus> { }
    internal sealed class TLMTabControllerDepotHooksNorTrm : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorTrm, TLMSysDefNorTrm> { }
    internal sealed class TLMTabControllerDepotHooksNorMnr : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorMnr, TLMSysDefNorMnr> { }
    internal sealed class TLMTabControllerDepotHooksNorMet : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorMet, TLMSysDefNorMet> { }
    internal sealed class TLMTabControllerDepotHooksNorTrn : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorTrn, TLMSysDefNorTrn> { }
    internal sealed class TLMTabControllerDepotHooksNorFer : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorFer, TLMSysDefNorFer> { }
    internal sealed class TLMTabControllerDepotHooksNorBlp : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorBlp, TLMSysDefNorBlp> { }
    internal sealed class TLMTabControllerDepotHooksNorShp : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorShp, TLMSysDefNorShp> { }
    internal sealed class TLMTabControllerDepotHooksNorPln : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksNorPln, TLMSysDefNorPln> { }
    internal sealed class TLMTabControllerDepotHooksTouBus : TLMTabControllerDepotHooks<TLMTabControllerDepotHooksTouBus, TLMSysDefTouBus> { }




    internal abstract class TLMTabControllerDepotList<T> : TLMTabControllerListBase<T> where T : TLMSysDef<T>
    {


        private DepotSortCriterion m_LastSortCriterion;
        private bool reverseOrder = false;

        protected override void OnUpdateStateChange(bool state)
        {
            if (!state)
            {
                for (int i = 0; i < mainPanel.components.Count; i++)
                {
                    mainPanel.components[i].GetComponent<TLMDepotListItem<T>>()?.Invalidate();
                }
            }
        }
        protected override bool HasRegionalPrefixFilter => true;

        #region Awake
        protected override void Awake()
        {
            base.Awake();
            m_LastSortCriterion = DepotSortCriterion.DEFAULT;
        }
        #endregion

        #region title row
        protected override void CreateTitleRow(out UIPanel titleLine, UIComponent parent)
        {
            TLMUtils.createUIElement(out titleLine, parent.transform, "TLMtitleline", new Vector4(5, 0, parent.width - 10, 40));

            TLMUtils.createUIElement(out UILabel districtTitle, titleLine.transform, "districtTitle");
            districtTitle.minimumSize = new Vector2(140, 0);
            districtTitle.area = new Vector4(00, 10, districtTitle.minimumSize.x, 18);
            TLMUtils.LimitWidth(districtTitle, (uint)districtTitle.width);
            districtTitle.textAlignment = UIHorizontalAlignment.Center;
            districtTitle.text = Locale.Get("TUTORIAL_ADVISER_TITLE", "District");
            districtTitle.eventClicked += District_eventClicked;

            TLMUtils.createUIElement(out UILabel lineName, titleLine.transform, "lineName");
            lineName.minimumSize = new Vector2(200, 0);
            lineName.area = new Vector4(140, 10, lineName.minimumSize.x, 18);
            TLMUtils.LimitWidth(lineName, (uint)lineName.width);
            lineName.textAlignment = UIHorizontalAlignment.Center;
            lineName.text = string.Format(Locale.Get("K45_TLM_DEPOT_NAME_PATTERN"), Locale.Get("K45_TLM_PUBLICTRANSPORT_OF_DEPOT", Singleton<T>.instance.GetTSD().toConfigIndex().ToString()));
            lineName.eventClicked += Name_eventClicked;

            TLMUtils.createUIElement(out UILabel prefixesServed, titleLine.transform, "prefixesServed");
            prefixesServed.minimumSize = new Vector2(210, 0);
            prefixesServed.area = new Vector4(340, 10, prefixesServed.minimumSize.x, 18);
            TLMUtils.LimitWidth(prefixesServed, (uint)prefixesServed.width);
            prefixesServed.textAlignment = UIHorizontalAlignment.Center;
            prefixesServed.text = Locale.Get("K45_TLM_PREFIXES_SERVED");

            AwakePrefixFilter();
        }

        private void Name_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterion == DepotSortCriterion.NAME ? !reverseOrder : false;
            RefreshLines();
        }

        private void District_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            reverseOrder = m_LastSortCriterion == DepotSortCriterion.DISTRICT ? !reverseOrder : false;
            RefreshLines();
        }
        #endregion

        private static Type ImplClassChildren => TLMUtils.GetImplementationForGenericType(typeof(TLMDepotListItem<>), typeof(T));

        private void AddToList(ushort buildingId, bool secondary, ref int count)
        {
            TLMDepotListItem<T> lineInfoItem;
            Type implClass = ImplClassChildren;
            if (count >= mainPanel.components.Count)
            {
                UIPanel bg = mainPanel.AddUIComponent<UIPanel>();
                bg.gameObject.AddComponent(implClass);
                lineInfoItem = bg.GetComponent<TLMDepotListItem<T>>();
            }
            else
            {
                lineInfoItem = mainPanel.components[count].GetComponent<TLMDepotListItem<T>>();
            }
            lineInfoItem.buildingId = buildingId;
            lineInfoItem.secondary = secondary;
            lineInfoItem.Invalidate();
            lineInfoItem.RefreshData();
            count++;
        }


        public override void RefreshLines()
        {
            var tsd = Singleton<T>.instance.GetTSD();
            bool hasPrefix = TLMLineUtils.hasPrefix(ref tsd);
            int count = 0;

            foreach (ushort buildingID in TLMDepotAI.getAllDepotsFromCity())
            {
                PrefabAI prefabAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.GetAI();
                if (prefabAI is ShelterAI && tsd.isShelterAiDepot())
                {
                    AddToList(buildingID, false, ref count);

                }
                else if (!tsd.isShelterAiDepot() && prefabAI is DepotAI ai)
                {
                    var tiArray = new TransportInfo[] { ai.m_transportInfo, ai.m_secondaryTransportInfo };
                    foreach (TransportInfo info in tiArray)
                    {
                        if (tsd.isFromSystem(info) && (!hasPrefix || m_prefixFilter.selectedIndex == 0 || TLMDepotAI.getPrefixesServedByDepot(buildingID, info == ai.m_secondaryTransportInfo).Contains((uint)(m_prefixFilter.selectedIndex - 1))))
                        {
                            AddToList(buildingID, info == ai.m_secondaryTransportInfo, ref count);
                        }
                    }
                }
            }

            RemoveExtraLines(count);

            switch (m_LastSortCriterion)
            {
                case DepotSortCriterion.NAME: OnNameSort(); break;
                case DepotSortCriterion.DISTRICT: OnDistrictSort(); break;
            }
            isUpdated = true;
        }

        #region Sorting

        private enum DepotSortCriterion
        {
            DEFAULT,
            NAME,
            DISTRICT
        }

        private static int CompareNames(UIComponent left, UIComponent right)
        {
            TLMDepotListItem<T> component = left.GetComponent<TLMDepotListItem<T>>();
            TLMDepotListItem<T> component2 = right.GetComponent<TLMDepotListItem<T>>();
            return string.Compare(component.buidingName, component2.buidingName, StringComparison.InvariantCulture);
        }

        private static int CompareDistricts(UIComponent left, UIComponent right)
        {
            TLMDepotListItem<T> component = left.GetComponent<TLMDepotListItem<T>>();
            TLMDepotListItem<T> component2 = right.GetComponent<TLMDepotListItem<T>>();
            return string.Compare(component.districtName, component2.districtName, StringComparison.InvariantCulture);
        }

        private void OnNameSort()
        {
            if (mainPanel.components.Count == 0)
                return;
            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareNames), reverseOrder);
            m_LastSortCriterion = DepotSortCriterion.NAME;
            mainPanel.Invalidate();
        }

        private void OnDistrictSort()
        {
            if (mainPanel.components.Count == 0)
                return;
            Quicksort(mainPanel.components, new Comparison<UIComponent>(CompareDistricts), reverseOrder);
            m_LastSortCriterion = DepotSortCriterion.DISTRICT;
            mainPanel.Invalidate();
        }

        #endregion

    }
    internal sealed class TLMTabControllerDepotListNorBus : TLMTabControllerDepotList<TLMSysDefNorBus> { }
    internal sealed class TLMTabControllerDepotListNorTrm : TLMTabControllerDepotList<TLMSysDefNorTrm> { }
    internal sealed class TLMTabControllerDepotListNorMnr : TLMTabControllerDepotList<TLMSysDefNorMnr> { }
    internal sealed class TLMTabControllerDepotListNorMet : TLMTabControllerDepotList<TLMSysDefNorMet> { }
    internal sealed class TLMTabControllerDepotListNorTrn : TLMTabControllerDepotList<TLMSysDefNorTrn> { }
    internal sealed class TLMTabControllerDepotListNorFer : TLMTabControllerDepotList<TLMSysDefNorFer> { }
    internal sealed class TLMTabControllerDepotListNorBlp : TLMTabControllerDepotList<TLMSysDefNorBlp> { }
    internal sealed class TLMTabControllerDepotListNorShp : TLMTabControllerDepotList<TLMSysDefNorShp> { }
    internal sealed class TLMTabControllerDepotListNorPln : TLMTabControllerDepotList<TLMSysDefNorPln> { }
    internal sealed class TLMTabControllerDepotListTouBus : TLMTabControllerDepotList<TLMSysDefTouBus> { }

}
