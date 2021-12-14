using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMTableRowOutsideConnection : UICustomControl
    {

        public const string ITEM_TEMPLATE = "K45_TLM_TLMTableRowOutsideConnection";

        public static void EnsureTemplate()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(ITEM_TEMPLATE))
            {
                return;
            }
            var go = new GameObject();
            var rowContainer = go.AddComponent<UIPanel>();
            rowContainer.height = 18;
            rowContainer.autoFitChildrenHorizontally = true;
            rowContainer.autoLayout = true;
            rowContainer.wrapLayout = false;
            rowContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            KlyteMonoUtils.CreateUIElement(out UISprite platformClr, rowContainer.transform);
            platformClr.autoSize = false;
            platformClr.width = 36;
            platformClr.spriteName = "EmptySprite";
            platformClr.height = 18;
            platformClr.name = "PlatformClr";

            KlyteMonoUtils.CreateUIElement(out UILabel platformNum, platformClr.transform);
            platformNum.autoSize = false;
            platformNum.width = 36;
            platformNum.height = 18;
            platformNum.name = "PlatformNum";
            platformNum.useOutline = true;
            platformNum.textAlignment = UIHorizontalAlignment.Center;
            platformNum.padding.top = 3;

            KlyteMonoUtils.CreateUIElement(out UIPanel outsideConnectionColumns, rowContainer.transform);
            outsideConnectionColumns.autoLayout = true;
            outsideConnectionColumns.autoLayoutDirection = LayoutDirection.Horizontal;
            outsideConnectionColumns.autoFitChildrenHorizontally = true;
            outsideConnectionColumns.height = 18;
            outsideConnectionColumns.name = "SelectionColumns";

            go.AddComponent<TLMTableRowOutsideConnection>();

            UITemplateUtils.GetTemplateDict()[ITEM_TEMPLATE] = rowContainer;
        }

        private UISprite m_platClr;
        private UILabel m_platNum;
        private ushort m_platformIdx;
        private ushort m_buildingId;
        private TransportSystemDefinition m_tsd;
        private UITemplateList<UIPanel> m_columnDataSelectionList;

        public void Awake()
        {
            m_platClr = Find<UISprite>("PlatformClr");
            m_platNum = m_platClr.Find<UILabel>("PlatformNum");
            KlyteMonoUtils.LimitWidthAndBox(m_platNum, 36, true);
            m_platNum.Disable();

            TLMTableRowDataOutsideConnection.EnsureTemplate();
            m_columnDataSelectionList = new UITemplateList<UIPanel>(Find<UIPanel>("SelectionColumns"), TLMTableRowDataOutsideConnection.ITEM_TEMPLATE);
        }

        public void ResetData(ushort buildingId, TransportSystemDefinition sysDef, ushort platformIdx, ushort[] targetOutsideConnections)
        {
            m_buildingId = buildingId;
            m_platformIdx = platformIdx;
            m_tsd = sysDef;
            ReloadData(targetOutsideConnections);
        }

        private void ReloadData(ushort[] targetOutsideConnections)
        {
            m_platClr.color = TLMController.COLOR_ORDER[m_platformIdx % TLMController.COLOR_ORDER.Length];
            m_platNum.text = $"{m_platformIdx + 1}";

            var columns = m_columnDataSelectionList.SetItemCount(targetOutsideConnections.Length);
            TransportLinesManagerMod.Controller.BuildingLines.SafeGet(m_buildingId).GetPlatformData(m_platformIdx, out PlatformConfig dataObj);
            for (int i = 0; i < columns.Length; i++)
            {
                UIPanel column = columns[i];
                var outsideConnId = targetOutsideConnections[i];
                if (m_tsd.IsValidOutsideConnection(outsideConnId))
                {
                    column.Enable();
                    column.GetComponent<TLMTableRowDataOutsideConnection>().ResetData(dataObj?.TargetOutsideConnections.ContainsKey(outsideConnId) ?? false, (x) => ToggleOutsideConnection(x, outsideConnId));
                }
                else
                {
                    column.Disable();
                }
            }
        }

        private void ToggleOutsideConnection(bool allow, ushort outsideConnId)
        {
            if (allow)
            {
                TransportLinesManagerMod.Controller.BuildingLines.SafeGet(m_buildingId).AddRegionalLine(m_platformIdx, outsideConnId, $"REG\n{m_buildingId.ToString("X4")}\n{outsideConnId.ToString("X4")}", TLMController.COLOR_ORDER[SimulationManager.instance.m_randomizer.Int32((uint)TLMController.COLOR_ORDER.Length)]);
            }
            else
            {
                TransportLinesManagerMod.Controller.BuildingLines.SafeGet(m_buildingId).RemoveRegionalLine(m_platformIdx, outsideConnId);
            }
        }

    }
}