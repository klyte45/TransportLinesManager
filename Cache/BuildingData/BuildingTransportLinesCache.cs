using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Overrides;
using UnityEngine;

namespace Klyte.TransportLinesManager.Cache
{
    public class BuildingTransportLinesCache : MonoBehaviour
    {


        private SimpleNonSequentialList<BuildingTransportDataCache> BuildingTransportDataCache;

        private void Awake() => BuildingTransportDataCache = new SimpleNonSequentialList<BuildingTransportDataCache>();

        private void OnEnable()
        {
            BuildingManager.instance.EventBuildingReleased += ResetBuilding;
            BuildingManager.instance.EventBuildingRelocated += ResetBuilding;
            NetManagerOverrides.EventNodeChanged += ResetAllBuilding;
        }
        private void OnDisable()
        {
            BuildingManager.instance.EventBuildingReleased -= ResetBuilding;
            BuildingManager.instance.EventBuildingRelocated -= ResetBuilding;
            NetManagerOverrides.EventNodeChanged -= ResetAllBuilding;
        }

        private void ResetBuilding(ushort buildingId) => BuildingTransportDataCache.Remove(buildingId);
        private void ResetAllBuilding(ushort _) => BuildingTransportDataCache.Clear();


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
                if (!BuildingTransportDataCache.ContainsKey(targetBuildingId))
                {
                    BuildingTransportDataCache[targetBuildingId] = new BuildingTransportDataCache(targetBuildingId, ref b, tsai);
                }
                BuildingTransportDataCache[targetBuildingId].RenderLines(cameraInfo);
            }
        }

        public void RenderPlatformStops(RenderManager.CameraInfo cameraInfo, ushort buildingId) => SafeGet(buildingId).RenderStopPoints(cameraInfo);

        public BuildingTransportDataCache SafeGet(ushort buildingId)
        {
            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
            var info = b.Info;
            if (b.m_parentBuilding != 0)
            {
                return SafeGet(Building.FindParentBuilding(buildingId));
            }
            if (BuildingTransportDataCache.ContainsKey(buildingId))
            {
                return BuildingTransportDataCache[buildingId];
            }
            BuildingTransportDataCache[buildingId] = info.m_buildingAI is TransportStationAI tsai ? new BuildingTransportDataCache(buildingId, ref b, tsai) : null;
            return BuildingTransportDataCache[buildingId];
        }
    }
}