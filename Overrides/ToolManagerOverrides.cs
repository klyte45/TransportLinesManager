using Klyte.Commons.Extensions;
using Klyte.TransportLinesManager.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{

    public class ToolManagerOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            #region Hooks
            System.Reflection.MethodInfo afterEndOverlayImpl = typeof(ToolManagerOverrides).GetMethod("AfterEndOverlayImpl", RedirectorUtils.allFlags);
            AddRedirect(typeof(ToolManager).GetMethod("EndOverlayImpl", RedirectorUtils.allFlags), null, afterEndOverlayImpl);
            #endregion 
        }

        public static void AfterEndOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            if (WorldInfoPanel.AnyWorldInfoPanelOpen() && (WorldInfoPanel.GetCurrentInstanceID().Building > 0 || WorldInfoPanel.GetCurrentInstanceID().Type == (InstanceType)TLMInstanceType.BuildingLines))
            {
                var buildingId = WorldInfoPanel.GetCurrentInstanceID().Type == (InstanceType)TLMInstanceType.BuildingLines ? WorldInfoPanel.GetCurrentInstanceID().Index >> 8 : WorldInfoPanel.GetCurrentInstanceID().Building;
                ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
                var info = b.Info;
                if (info.m_buildingAI is TransportStationAI tsai && tsai.m_transportLineInfo?.m_class.m_subService == ItemClass.SubService.PublicTransportTrain)
                {
                    TransportLinesManagerMod.Controller.BuildingLines.RenderBuildingLines(cameraInfo, (ushort)buildingId);
                    TransportLinesManagerMod.Controller.BuildingLines.RenderPlatformStops(cameraInfo, (ushort)buildingId);
                }

            }
        }
    }
}