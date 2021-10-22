using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager.Cache
{
    public class BuildingTransportLinesCache : MonoBehaviour
    {


        private SimpleNonSequentialList<BuildingTransportData> BuildingTransportData;

        private void Awake() => BuildingTransportData = new SimpleNonSequentialList<BuildingTransportData>();

        private void OnEnable()
        {
            BuildingManager.instance.EventBuildingReleased += ResetBuilding;
            BuildingManager.instance.EventBuildingRelocated += ResetBuilding;
        }
        private void OnDisable()
        {
            BuildingManager.instance.EventBuildingReleased -= ResetBuilding;
            BuildingManager.instance.EventBuildingRelocated -= ResetBuilding;
        }

        private void ResetBuilding(ushort buildingId) => BuildingTransportData.Remove(buildingId);


        public void RenderBuildingLines(RenderManager.CameraInfo cameraInfo, ushort buildingId)
        {
            var targetBuildingId = Building.FindParentBuilding(buildingId);
            if (targetBuildingId == 0)
            {
                targetBuildingId = buildingId;
            }

            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[targetBuildingId];
            var info = b.Info;
            if (info.m_buildingAI is TransportStationAI tsai)
            {
                if (!BuildingTransportData.ContainsKey(targetBuildingId))
                {
                    BuildingTransportData[targetBuildingId] = new BuildingTransportData(targetBuildingId, ref b, tsai);
                }
                BuildingTransportData[targetBuildingId].RenderLines(cameraInfo);
            }
        }

        public void RenderPlatformStops(RenderManager.CameraInfo cameraInfo, ushort buildingId) => SafeGet(buildingId).RenderStopPoints(cameraInfo);

        public BuildingTransportData SafeGet(ushort buildingId)
        {
            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
            var info = b.Info;
            if (b.m_parentBuilding != 0)
            {
                return SafeGet(Building.FindParentBuilding(buildingId));
            }
            if (BuildingTransportData.ContainsKey(buildingId))
            {
                return BuildingTransportData[buildingId];
            }
            if (info.m_buildingAI is TransportStationAI tsai)
            {
                BuildingTransportData[buildingId] = new BuildingTransportData(buildingId, ref b, tsai);
            }
            else
            {
                BuildingTransportData[buildingId] = null;
            }
            return BuildingTransportData[buildingId];
        }






        internal static readonly Color[] COLOR_ORDER = new Color[]
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