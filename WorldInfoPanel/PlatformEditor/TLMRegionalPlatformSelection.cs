using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMRegionalPlatformSelection : UICustomControl
    {
        internal static TLMRegionalPlatformSelection Instance { get; private set; }

        private UIPanel m_containerParent;
        private UIPanel m_tableContainer;
        private UILabel m_title;
        private UIPanel m_titleRowContainer;
        private UITemplateList<UILabel> m_titleOutsideConnectionsTemplateList;
        private UIPanel m_platformListContainer;
        private UITemplateList<UIButton> m_platformLinesTemplateList;
        private ushort lastBuildingSelected = 0;


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


            KlyteMonoUtils.CreateUIElement(out m_title, m_containerParent.transform);
            m_title.autoSize = true;
            m_title.width = m_containerParent.width;
            m_title.textAlignment = UIHorizontalAlignment.Left;
            m_title.localeID = "K45_TLM_REGIONALPLATFORM_CONFIG";
            m_title.useOutline = true;
            m_title.height = 18;

            KlyteMonoUtils.CreateUIElement(out m_tableContainer, m_containerParent.transform);
            m_tableContainer.width = m_containerParent.width;
            m_tableContainer.autoFitChildrenVertically = true;
            m_tableContainer.autoFitChildrenHorizontally= true;
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
            m_platformLinesTemplateList = new UITemplateList<UIButton>(m_platformListContainer, TLMLineItemButtonControl.LINE_ITEM_TEMPLATE);
        }
        internal void EventWIPChanged(UIComponent component)
        {
            var building = WorldInfoPanel.GetCurrentInstanceID().Building;
            var show = BuildingManager.instance.m_buildings.m_buffer[building].Info.m_buildingAI is TransportStationAI;

            UpdateNearPlatforms(show);
        }

        private void UpdateNearPlatforms(bool show, bool force = false)
        {
            if (!show)
            {
                m_containerParent.isVisible = false;
                return;
            }
            ushort buildingId = WorldInfoPanel.GetCurrentInstanceID().Building;
            var instance = BuildingManager.instance;
            ref Building b = ref instance.m_buildings.m_buffer[buildingId];
            if (!(b.Info.m_buildingAI is TransportStationAI tsai) || tsai.m_transportLineInfo is null)
            {
                m_containerParent.isVisible = false;
                return;
            }
            m_containerParent.isVisible = true;
            var outsideConnections = instance.GetOutsideConnections().ToArray().Where(x =>
            instance.m_buildings.m_buffer[x].Info is BuildingInfo outsideConn
            && (
                (outsideConn.m_class.m_service == tsai.m_transportLineInfo.m_class.m_service && outsideConn.m_class.m_subService == tsai.m_transportLineInfo.m_class.m_subService)
                || tsai.IsIntercityBusConnection(outsideConn))
            ).ToArray();

            var titleItems = m_titleOutsideConnectionsTemplateList.SetItemCount(outsideConnections.Length);
            for (int i = 0; i < titleItems.Length; i++)
            {
                titleItems[i].GetComponent<TLMTableTitleOutsideConnection>().ResetData(outsideConnections[i]);
            }
        }
    }
}