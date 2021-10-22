using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMNearLinesController : UICustomControl
    {
        private UIPanel m_containerParent;
        private UIPanel m_localContainer;
        private UILabel m_title;
        private UIPanel m_listContainer;
        private UITemplateList<UIButton> m_localLinesTemplateList;
        private UIPanel m_regionalContainer;
        private UILabel m_regTitle;
        private UIPanel m_regListContainer;
        private UITemplateList<UIButton> m_regionalLinesTemplateList;
        private ushort lastBuildingSelected = 0;

        public static void InitNearLinesOnWorldInfoPanel()
        {
            BuildingWorldInfoPanel[] panelList = UIView.GetAView().GetComponentsInChildren<BuildingWorldInfoPanel>();
            LogUtils.DoLog("WIP LIST: [{0}]", string.Join(", ", panelList.Select(x => x.name).ToArray()));
            TLMLineItemButtonControl.EnsureTemplate();
            foreach (BuildingWorldInfoPanel wip in panelList)
            {
                LogUtils.DoLog("LOADING WIP HOOK FOR: {0}", wip.name);
                UIComponent parent2 = wip.GetComponent<UIComponent>();

                if (parent2 is null)
                {
                    continue;
                }
                var isGrow = wip is ZonedBuildingWorldInfoPanel;
                var controller = InitPanelNearLinesOnWorldInfoPanel(parent2);
                parent2.eventVisibilityChanged += (x, y) => controller.EventWIPChanged(x, isGrow);
                parent2.eventPositionChanged += (x, y) => controller.EventWIPChanged(x, isGrow);
                parent2.eventSizeChanged += (x, y) => controller.EventWIPChanged(x, isGrow);

            }

        }

        private static TLMNearLinesController InitPanelNearLinesOnWorldInfoPanel(UIComponent parent)
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel panel, parent.transform);
            return panel.gameObject.AddComponent<TLMNearLinesController>();
        }

        public void Awake()
        {
            m_containerParent = GetComponent<UIPanel>();
            m_containerParent.relativePosition = new Vector3(m_containerParent.parent.width + 15, 50);
            m_containerParent.backgroundSprite = "GenericPanelDark";


            m_containerParent.autoFitChildrenVertically = true;
            m_containerParent.autoLayout = true;
            m_containerParent.autoLayoutDirection = LayoutDirection.Vertical;
            m_containerParent.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_containerParent.padding = new RectOffset(2, 2, 2, 2);
            m_containerParent.autoLayoutStart = LayoutStart.TopLeft;
            m_containerParent.name = "TLMLinesNear";
            m_containerParent.width = 300;
            m_containerParent.padding.top = 5;
            m_containerParent.padding.bottom = 5;


            KlyteMonoUtils.CreateUIElement(out m_localContainer, m_containerParent.transform);
            m_localContainer.width = m_containerParent.width;
            m_localContainer.autoFitChildrenVertically = true;
            m_localContainer.autoLayout = true;
            m_localContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_localContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_localContainer.padding = new RectOffset(2, 2, 2, 2);
            m_localContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_localContainer.name = "TLMLinesNearRegional";

            KlyteMonoUtils.CreateUIElement(out m_title, m_localContainer.transform);
            m_title.autoSize = false;
            m_title.width = m_localContainer.width;
            m_title.textAlignment = UIHorizontalAlignment.Left;
            m_title.localeID = "K45_TLM_NEAR_LINES";
            m_title.useOutline = true;
            m_title.height = 18;

            KlyteMonoUtils.CreateUIElement(out m_listContainer, m_localContainer.transform);
            m_listContainer.width = m_localContainer.width;
            m_listContainer.autoFitChildrenVertically = true;
            m_listContainer.autoLayout = true;
            m_listContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            m_listContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_listContainer.padding = new RectOffset(2, 2, 2, 2);
            m_listContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_listContainer.wrapLayout = true;
            m_listContainer.name = "TLMLinesNearList";

            m_localLinesTemplateList = new UITemplateList<UIButton>(m_listContainer, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);


            KlyteMonoUtils.CreateUIElement(out m_regionalContainer, m_containerParent.transform);
            m_regionalContainer.width = m_containerParent.width;
            m_regionalContainer.autoFitChildrenVertically = true;
            m_regionalContainer.autoLayout = true;
            m_regionalContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_regionalContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_regionalContainer.padding = new RectOffset(2, 2, 2, 2);
            m_regionalContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_regionalContainer.name = "TLMLinesNearRegional";

            KlyteMonoUtils.CreateUIElement(out m_regTitle, m_regionalContainer.transform);
            m_regTitle.autoSize = false;
            m_regTitle.width = m_regionalContainer.width;
            m_regTitle.textAlignment = UIHorizontalAlignment.Left;
            m_regTitle.localeID = "K45_TLM_NEAR_LINES_REGIONAL";
            m_regTitle.useOutline = true;
            m_regTitle.height = 18;


            KlyteMonoUtils.CreateUIElement(out m_regListContainer, m_regionalContainer.transform);
            m_regListContainer.width = m_regionalContainer.width;
            m_regListContainer.autoFitChildrenVertically = true;
            m_regListContainer.autoLayout = true;
            m_regListContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            m_regListContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_regListContainer.padding = new RectOffset(2, 2, 2, 2);
            m_regListContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_regListContainer.wrapLayout = true;
            m_regListContainer.name = "TLMLinesNearListRegional";
            m_regionalLinesTemplateList = new UITemplateList<UIButton>(m_regListContainer, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);
        }
        private void EventWIPChanged(UIComponent component, bool isGrow) => UpdateNearLines((isGrow ? TransportLinesManagerMod.ShowNearLinesGrow : TransportLinesManagerMod.ShowNearLinesPlop), true);

        private void UpdateNearLines(bool show, bool force = false)
        {
            if (!show)
            {
                m_containerParent.isVisible = false;
                return;
            }
            ushort buildingId = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (lastBuildingSelected == buildingId && !force)
            {
                return;
            }
            else
            {
                lastBuildingSelected = buildingId;
            }
            ref Building b = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];

            var nearLines = new List<ushort>();
            Vector3 sidewalk = b.CalculateSidewalkPosition();
            TLMLineUtils.GetNearLines(sidewalk, 120f, ref nearLines);
            bool showLocal = nearLines.Count > 0;
            if (showLocal)
            {
                var localLines = TLMLineUtils.SortLines(nearLines).Values.ToArray();
                var itemsEntries = m_localLinesTemplateList.SetItemCount(localLines.Length);
                for (int idx = 0; idx < localLines.Length; idx++)
                {
                    ushort lineId = localLines[idx];
                    var itemControl = itemsEntries[idx].GetComponent<TLMLineItemButtonControl>();
                    itemControl.ResetData(0, lineId, sidewalk);
                }
            }

            var regionalLines = TransportLinesManagerMod.Controller.BuildingLines.SafeGet(buildingId);
            var showRegional = regionalLines != null && regionalLines.RegionalLinesCount > 0;
            if (showRegional)
            {
                var itemsEntries = m_regionalLinesTemplateList.SetItemCount(regionalLines.RegionalLinesCount);
                for (ushort idx = 0; idx < regionalLines.RegionalLinesCount; idx++)
                {
                    var itemControl = itemsEntries[idx].GetComponent<TLMLineItemButtonControl>();
                    itemControl.ResetData(buildingId, idx, sidewalk);
                }
            }

            m_localContainer.isVisible = showLocal;
            m_regionalContainer.isVisible = showRegional;
            m_containerParent.isVisible = showLocal || showRegional;
        }
    }
}