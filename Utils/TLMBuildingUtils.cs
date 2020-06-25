using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    public class TLMBuildingUtils
    {
        public static string GetBuildingDetails(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, ushort lineId = 0)
        {

            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;

            Building b = bm.m_buildings.m_buffer[buildingId];
            while (b.m_parentBuilding > 0)
            {
                LogUtils.DoLog("getBuildingName(): building id {0} - parent = {1}", buildingId, b.m_parentBuilding);
                buildingId = b.m_parentBuilding;
                b = bm.m_buildings.m_buffer[buildingId];
            }
            InstanceID iid = default;
            iid.Building = buildingId;
            serviceFound = b.Info?.GetService() ?? default;
            subserviceFound = b.Info?.GetSubService() ?? default;
            var index = GameServiceExtensions.ToConfigIndex(serviceFound, subserviceFound);
            TransportSystemDefinition tsd = default;
            if ((index & TLMCW.ConfigIndex.DESC_DATA) == TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG)
            {
                tsd = TransportSystemDefinition.From(b.Info.GetAI());
                index = tsd.ToConfigIndex();
            }
            prefix = index.GetSystemStationNamePrefix(lineId)?.TrimStart();
            LogUtils.DoLog($"getBuildingName(): serviceFound {serviceFound} - subserviceFound = {subserviceFound} - tsd = {tsd} - index = {index} - prefix = {prefix}");

            return bm.GetBuildingName(buildingId, iid);
        }
    }

}

