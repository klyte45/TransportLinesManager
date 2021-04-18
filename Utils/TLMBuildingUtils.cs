using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;


namespace Klyte.TransportLinesManager.Utils
{
    public class TLMBuildingUtils
    {
        public static string GetBuildingDetails(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, out NamingType namingType, ushort lineId = 0)
        {

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
            TransportSystemDefinition tsd = TransportSystemDefinition.From(b.Info.GetAI());
            prefix = tsd.GetConfig().NamingPrefix?.TrimStart();
            LogUtils.DoLog($"getBuildingName(): serviceFound {serviceFound} - subserviceFound = {subserviceFound} - tsd = {tsd} - prefix = {prefix}");
            namingType = NamingTypeExtensions.From(tsd);

            return bm.GetBuildingName(buildingId, iid);
        }
    }

}

