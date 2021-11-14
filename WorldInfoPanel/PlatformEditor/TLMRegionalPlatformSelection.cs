using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMRegionalPlatformSelection : UICustomControl
    {
        internal static TLMRegionalPlatformSelection Instance { get; private set; }

        private UIPanel m_containerParent;
        private UIPanel m_tableContainer;
        private UICheckBox m_title;
        private UIPanel m_titleRowContainer;
        private UITemplateList<UILabel> m_titleOutsideConnectionsTemplateList;
        private UIPanel m_platformListContainer;
        private UITemplateList<UIPanel> m_platformLinesTemplateList;

        private bool m_dirty = true;

        internal static TLMRegionalPlatformSelection Init(UIComponent parent)
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel panel, parent.transform);
            return panel.gameObject.AddComponent<TLMRegionalPlatformSelection>();
        }

        public void Awake()
        {
            Instance = this;

            m_containerParent = GetComponent<UIPanel>();
            m_containerParent.backgroundSprite = "GenericPanelDark";

            m_containerParent.autoFitChildrenVertically = true;
            m_containerParent.autoLayout = true;
            m_containerParent.autoLayoutDirection = LayoutDirection.Vertical;
            m_containerParent.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_containerParent.padding = new RectOffset(2, 2, 2, 2);
            m_containerParent.autoLayoutStart = LayoutStart.TopLeft;
            m_containerParent.name = "TLMPlatform";
            m_containerParent.autoFitChildrenHorizontally = true;
            m_containerParent.padding.top = 5;
            m_containerParent.padding.bottom = 5;

            m_title = UIHelperExtension.AddCheckboxLocale(m_containerParent, "K45_TLM_REGIONALPLATFORM_USECONFIG", false);
            m_title.label.autoSize = true;
            KlyteMonoUtils.LimitWidthAndBox(m_title.label, 320);
            m_title.height = 24;
            m_title.width = 350;

            KlyteMonoUtils.CreateUIElement(out m_tableContainer, m_containerParent.transform);
            m_tableContainer.width = m_containerParent.width;
            m_tableContainer.autoFitChildrenVertically = true;
            m_tableContainer.autoFitChildrenHorizontally = true;
            m_tableContainer.autoLayout = true;
            m_tableContainer.autoLayoutDirection = LayoutDirection.Vertical;
            m_tableContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_tableContainer.padding = new RectOffset(2, 2, 2, 2);
            m_tableContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_tableContainer.name = "TableContainer";


            KlyteMonoUtils.CreateUIElement(out m_titleRowContainer, m_tableContainer.transform);
            m_titleRowContainer.width = m_tableContainer.width;
            m_titleRowContainer.autoFitChildrenVertically = true;
            m_titleRowContainer.autoFitChildrenHorizontally = true;
            m_titleRowContainer.autoLayout = true;
            m_titleRowContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            m_titleRowContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_titleRowContainer.padding = new RectOffset(2, 2, 2, 2);
            m_titleRowContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_titleRowContainer.wrapLayout = false;
            m_titleRowContainer.name = "TLMOutsideConnections";

            KlyteMonoUtils.CreateUIElement(out UILabel outsideConnectionIcon, m_titleRowContainer.transform);
            outsideConnectionIcon.autoSize = false;
            outsideConnectionIcon.width = 36;
            outsideConnectionIcon.backgroundSprite = "InfoIconOutsideConnections";
            outsideConnectionIcon.height = 36;

            KlyteMonoUtils.CreateUIElement(out UIPanel outsideConnectionColumns, m_titleRowContainer.transform);
            outsideConnectionColumns.autoLayout = true;
            outsideConnectionColumns.autoLayoutDirection = LayoutDirection.Horizontal;
            outsideConnectionColumns.autoFitChildrenHorizontally = true;
            outsideConnectionColumns.height = 36;
            TLMTableTitleOutsideConnection.EnsureTemplate();
            m_titleOutsideConnectionsTemplateList = new UITemplateList<UILabel>(outsideConnectionColumns, TLMTableTitleOutsideConnection.ITEM_TEMPLATE);


            KlyteMonoUtils.CreateUIElement(out m_platformListContainer, m_tableContainer.transform);
            m_platformListContainer.width = m_tableContainer.width;
            m_platformListContainer.autoFitChildrenVertically = true;
            m_platformListContainer.autoFitChildrenHorizontally = true;
            m_platformListContainer.autoLayout = true;
            m_platformListContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            m_platformListContainer.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_platformListContainer.padding = new RectOffset(2, 2, 2, 2);
            m_platformListContainer.autoLayoutStart = LayoutStart.TopLeft;
            m_platformListContainer.wrapLayout = true;
            m_platformListContainer.name = "TLMPlatformRegionalDestinations";
            TLMTableRowOutsideConnection.EnsureTemplate();
            m_platformLinesTemplateList = new UITemplateList<UIPanel>(m_platformListContainer, TLMTableRowOutsideConnection.ITEM_TEMPLATE);
        }

        private void OnToggleUseTlmSettings(UIComponent _, bool value)
        {
            var building = WorldInfoPanel.GetCurrentInstanceID().Building;
            TLMBuildingDataContainer.Instance.SafeGet(building).OnToggleTlmRegionalManagement(value);

            EventWIPChanged();
        }

        internal void EventWIPChanged() => m_dirty = true;
        private void Update()
        {
            if (m_dirty)
            {
                m_title.eventCheckChanged -= OnToggleUseTlmSettings;
                var building = WorldInfoPanel.GetCurrentInstanceID().Building;
                var show = BuildingManager.instance.m_buildings.m_buffer[building].Info.m_buildingAI is TransportStationAI tsai;

                UpdateNearPlatforms(show);
                m_title.eventCheckChanged += OnToggleUseTlmSettings;
                m_dirty = false;
            }
        }

        private void UpdateNearPlatforms(bool show)
        {
            if (!show)
            {
                m_containerParent.isVisible = false;
                return;
            }

            ushort buildingId = WorldInfoPanel.GetCurrentInstanceID().Building;
            var data = TransportLinesManagerMod.Controller.BuildingLines.SafeGet(buildingId);
            if (data.BuildingData.TlmManagedRegionalLines)
            {
                m_title.isChecked = true;
                m_tableContainer.isVisible = true;
            }
            else
            {
                m_title.isChecked = false;
                m_tableContainer.isVisible = false;
            }
            var instance = BuildingManager.instance;
            var nm = NetManager.instance;
            ref Building b = ref instance.m_buildings.m_buffer[buildingId];
            if (!(b.Info.m_buildingAI is TransportStationAI tsai))
            {
                m_containerParent.isVisible = false;
                return;
            }
            var availableOutsideTransportInfo = data.StopPoints.Select(x =>
            {
                ref NetSegment segment = ref nm.m_segments.m_buffer[nm.m_lanes.m_buffer[x.laneId].m_segment];
                var info = segment.Info;
                return TransportSystemDefinition.FromNetInfo(info);
            }).Where(x => x != null && x?.GetTransportInfoIntercity() != null).GroupBy(x => x).Select(x => x.First()).ToList();
            if (availableOutsideTransportInfo.Count == 0)
            {
                m_containerParent.isVisible = false;
                return;
            }
            m_containerParent.isVisible = true;
            var outsideConnections = instance.GetOutsideConnections().ToArray().Where(x => availableOutsideTransportInfo.Any(y => y.IsValidOutsideConnection(x))).ToArray();

            var titleItems = m_titleOutsideConnectionsTemplateList.SetItemCount(outsideConnections.Length);
            for (int i = 0; i < titleItems.Length; i++)
            {
                titleItems[i].GetComponent<TLMTableTitleOutsideConnection>().ResetData(outsideConnections[i]);
            }
            var stops = data.StopPoints;
            var rowItems = m_platformLinesTemplateList.SetItemCount(stops.Length);
            for (ushort i = 0; i < rowItems.Length; i++)
            {
                var row = rowItems[i];
                var trackInfo = nm.m_segments.m_buffer[nm.m_lanes.m_buffer[stops[i].laneId].m_segment].Info;
                if (availableOutsideTransportInfo.Any(x => x.IsValidOutsideConnectionTrack(trackInfo)))
                {
                    row.isVisible = true;
                    var controller = row.GetComponentInChildren<TLMTableRowOutsideConnection>();
                    controller.ResetData(buildingId, TransportSystemDefinition.FromNetInfo(trackInfo), i, outsideConnections);
                }
                else
                {
                    row.isVisible = false;
                }
            }
        }
    }
}