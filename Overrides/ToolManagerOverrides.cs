using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{

    public class ToolManagerOverrides : Redirector, IRedirectable
    {

        public static SimpleNonSequentialList<TransportLine[]> OutsideConnectionsLinesBuilding = new SimpleNonSequentialList<TransportLine[]>();

        public static SimpleNonSequentialList<Mesh[][]> m_lineMeshes = new SimpleNonSequentialList<Mesh[][]>();
        public static SimpleNonSequentialList<RenderGroup.MeshData[][]> m_lineMeshData = new SimpleNonSequentialList<RenderGroup.MeshData[][]>();

        public void Awake()
        {
            #region Hooks
            System.Reflection.MethodInfo afterEndOverlayImpl = typeof(ToolManagerOverrides).GetMethod("AfterEndOverlayImpl", RedirectorUtils.allFlags);
            AddRedirect(typeof(ToolManager).GetMethod("EndOverlayImpl", RedirectorUtils.allFlags), null, afterEndOverlayImpl);
            #endregion 
        }

        public static void AfterEndOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            if (WorldInfoPanel.AnyWorldInfoPanelOpen() && WorldInfoPanel.GetCurrentInstanceID().Building > 0 || WorldInfoPanel.GetCurrentInstanceID().Type == (InstanceType)TLMInstanceType.BuildingLines)
            {
                var buildingId = WorldInfoPanel.GetCurrentInstanceID().Type == (InstanceType)TLMInstanceType.BuildingLines ? WorldInfoPanel.GetCurrentInstanceID().Index >> 8 : WorldInfoPanel.GetCurrentInstanceID().Building;
                ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
                var info = b.Info;
                if (info.m_buildingAI is TransportStationAI tsai)
                {
                    TransportLinesManagerMod.Controller.BuildingLines.RenderBuildingLines(cameraInfo, (ushort)buildingId);

                    //if (RenderManager.instance.RequireInstance(buildingId, 1u, out uint num))
                    //{
                    //    ref RenderManager.Instance renderInstance = ref RenderManager.instance.m_instances[num];
                    //    var useSecInfo = !(tsai.m_secondaryTransportInfo is null) && tsai.m_secondaryTransportInfo.m_class.m_subService == tsai.m_transportLineInfo.m_class.m_subService && tsai.m_secondaryTransportInfo.m_class.m_level == tsai.m_transportLineInfo.m_class.m_level;
                    //    DepotAI.SpawnPoint[] spawnPoints = useSecInfo ? tsai.m_spawnPoints2 : tsai.m_spawnPoints;
                    //    if (spawnPoints != null && spawnPoints.Length > 0)
                    //    {
                    //        for (int i = 0; i < spawnPoints.Length; i++)
                    //        {
                    //            DepotAI.SpawnPoint point = spawnPoints[i];
                    //            RenderSpawnPoint(cameraInfo, ref renderInstance, i, point.m_target);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        RenderSpawnPoint(cameraInfo, ref renderInstance, 0, useSecInfo ? tsai.m_spawnTarget : tsai.m_spawnTarget);
                    //    }
                    //}
                }

            }
        }

        private static void RenderSpawnPoint(RenderManager.CameraInfo cameraInfo, ref RenderManager.Instance renderInstance, int i, Vector3 point)
            => RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo,
               m_colorOrder[i % m_colorOrder.Length],
               renderInstance.m_dataMatrix1.MultiplyPoint(point),
               3,
               -1, 1280f, false, true);

        internal static readonly Color[] m_colorOrder = new Color[]
             {
                Color.red,
                Color.Lerp(Color.red, Color.yellow,0.5f),
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                Color.Lerp(Color.blue, Color.magenta,0.5f),
                Color.magenta,
                Color.white,
                Color.black,
                Color.Lerp( Color.red,                                    Color.black,0.5f),
                Color.Lerp( Color.Lerp(Color.red, Color.yellow,0.5f),     Color.black,0.5f),
                Color.Lerp( Color.yellow,                                 Color.black,0.5f),
                Color.Lerp( Color.green,                                  Color.black,0.5f),
                Color.Lerp( Color.cyan,                                   Color.black,0.5f),
                Color.Lerp( Color.blue,                                   Color.black,0.5f),
                Color.Lerp( Color.Lerp(Color.blue, Color.magenta,0.5f),   Color.black,0.5f),
                Color.Lerp( Color.magenta,                                Color.black,0.5f),
                Color.Lerp( Color.white,                                  Color.black,0.25f),
                Color.Lerp( Color.white,                                  Color.black,0.75f)
             };
    }
}