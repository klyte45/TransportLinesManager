using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMNearLinesController
    {
        private const string NEAR_LINE_TEMPLATE = "K45_TLM_NearLinesItemTemplate";

        private static ushort lastBuildingSelected = 0;

        public static void InitNearLinesOnWorldInfoPanel()
        {
            BuildingWorldInfoPanel[] panelList = UIView.GetAView().GetComponentsInChildren<BuildingWorldInfoPanel>();
            LogUtils.DoLog("WIP LIST: [{0}]", string.Join(", ", panelList.Select(x => x.name).ToArray()));
            CreateNearLineTemplate();
            foreach (BuildingWorldInfoPanel wip in panelList)
            {
                LogUtils.DoLog("LOADING WIP HOOK FOR: {0}", wip.name);
                UIComponent parent2 = wip.GetComponent<UIComponent>();

                if (parent2 == null)
                {
                    continue;
                }
                var isGrow = wip is ZonedBuildingWorldInfoPanel;
                InitPanelNearLinesOnWorldInfoPanel(parent2);
                parent2.eventVisibilityChanged += (x, y) => EventWIPChanged(x, isGrow);
                parent2.eventPositionChanged += (x, y) => EventWIPChanged(x, isGrow);
                parent2.eventSizeChanged += (x, y) => EventWIPChanged(x, isGrow);

            }

        }

        private static void CreateNearLineTemplate()
        {
            var go = new GameObject();
            var size = 40;
            var multiplier = .8f;
            var lineCircleIntersect = go.AddComponent<UIButton>();
            lineCircleIntersect.autoSize = false;
            lineCircleIntersect.width = size;
            lineCircleIntersect.height = size;
            lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
            lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
            lineCircleIntersect.name = "LineFormat";
            lineCircleIntersect.relativePosition = new Vector3(0f, 0f);
            lineCircleIntersect.hoveredColor = Color.white;
            lineCircleIntersect.hoveredTextColor = Color.red;

            KlyteMonoUtils.CreateUIElement(out UILabel lineNumberIntersect, lineCircleIntersect.transform);
            lineNumberIntersect.autoSize = false;
            lineNumberIntersect.autoHeight = false;
            lineNumberIntersect.width = lineCircleIntersect.width;
            lineNumberIntersect.pivot = UIPivotPoint.MiddleCenter;
            lineNumberIntersect.textAlignment = UIHorizontalAlignment.Center;
            lineNumberIntersect.verticalAlignment = UIVerticalAlignment.Middle;
            lineNumberIntersect.name = "LineNumber";
            lineNumberIntersect.height = size;
            lineNumberIntersect.relativePosition = new Vector3(-0.5f, 0.5f);
            lineNumberIntersect.outlineColor = Color.black;
            lineNumberIntersect.useOutline = true;
            KlyteMonoUtils.CreateUIElement(out UILabel daytimeIndicator, lineCircleIntersect.transform);
            daytimeIndicator.autoSize = false;
            daytimeIndicator.width = size;
            daytimeIndicator.height = size;
            daytimeIndicator.color = Color.white;
            daytimeIndicator.pivot = UIPivotPoint.MiddleLeft;
            daytimeIndicator.verticalAlignment = UIVerticalAlignment.Middle;
            daytimeIndicator.name = "LineTime";
            daytimeIndicator.relativePosition = new Vector3(0f, 0f);

            lineNumberIntersect.textScale *= multiplier;
            lineNumberIntersect.relativePosition *= multiplier;

            go.AddComponent<TLMLineItemController>();

            UITemplateUtils.GetTemplateDict()[NEAR_LINE_TEMPLATE] = lineCircleIntersect;
        }

        private static Transform InitPanelNearLinesOnWorldInfoPanel(UIComponent parent)
        {
            var containerParent = parent.GetComponent<UIPanel>();

            UIPanel innerContainer = containerParent.AddUIComponent<UIPanel>();
            innerContainer.relativePosition = new Vector3(0, parent.height);
            innerContainer.width = parent.width;
            innerContainer.autoFitChildrenVertically = true;
            innerContainer.autoLayout = true;
            innerContainer.autoLayoutDirection = LayoutDirection.Vertical;
            innerContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            innerContainer.padding = new RectOffset(2, 2, 2, 2);
            innerContainer.autoLayoutStart = LayoutStart.TopLeft;
            innerContainer.name = "TLMLinesNear";

            innerContainer.width = 300;
            innerContainer.padding.top = 5;
            innerContainer.padding.bottom = 5;
            innerContainer.relativePosition = new Vector3(containerParent.width + 10, 50);
            innerContainer.backgroundSprite = "GenericPanelDark";

            UILabel title = innerContainer.AddUIComponent<UILabel>();
            title.autoSize = false;
            title.width = innerContainer.width;
            title.textAlignment = UIHorizontalAlignment.Left;
            title.localeID = "K45_TLM_NEAR_LINES";
            title.useOutline = true;
            title.height = 18;
            UIPanel listContainer = innerContainer.AddUIComponent<UIPanel>();
            listContainer.width = innerContainer.width;
            listContainer.autoFitChildrenVertically = true;
            listContainer.autoLayout = true;
            listContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            listContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            listContainer.padding = new RectOffset(2, 2, 2, 2);
            listContainer.autoLayoutStart = LayoutStart.TopLeft;
            listContainer.wrapLayout = true;
            listContainer.name = "TLMLinesNearList";

            listContainer.objectUserData = new UITemplateList<UIButton>(listContainer, NEAR_LINE_TEMPLATE);

            return innerContainer.transform;
        }
        private static void EventWIPChanged(UIComponent component, bool isGrow) => UpdateNearLines((isGrow ? TransportLinesManagerMod.ShowNearLinesGrow : TransportLinesManagerMod.ShowNearLinesPlop) ? component : null, true);

        private static void UpdateNearLines(UIComponent parent, bool force = false)
        {
            if (parent != null)
            {
                var linesPanelObj = parent.Find("TLMLinesNear");
                if (!linesPanelObj)
                {
                    LogUtils.DoErrorLog($"TLM BAR NOT FOUND @ {parent.name}!");
                    return;
                }
                System.Reflection.FieldInfo prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                WorldInfoPanel wip = parent.gameObject.GetComponent<WorldInfoPanel>();
                ushort buildingId = ((InstanceID)(prop.GetValue(wip))).Building;
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
                bool showPanel = nearLines.Count > 0;
                if (showPanel)
                {
                    var lines = TLMLineUtils.SortLines(nearLines).Values.ToArray();
                    if (!(linesPanelObj.Find<UIPanel>("TLMLinesNearList")?.objectUserData is UITemplateList<UIButton> templateList))
                    {
                        LogUtils.DoErrorLog($"Error loading templateList @ {parent.name}!");
                        return;
                    }
                    var itemsEntries = templateList.SetItemCount(lines.Length);
                    for (int idx = 0; idx < lines.Length; idx++)
                    {
                        ushort lineId = lines[idx];
                        var itemControl = itemsEntries[idx].GetComponent<TLMLineItemController>();
                        itemControl.ResetData(lineId, sidewalk);
                    }

                }
                linesPanelObj.GetComponent<UIPanel>().isVisible = showPanel;
            }
            else
            {
                var go = GameObject.Find("TLMLinesNear");
                if (!go)
                {
                    return;
                }
                Transform linesPanelObj = go.transform;
                linesPanelObj.GetComponent<UIPanel>().isVisible = false;
            }
        }
    }
}