using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Xml;

namespace Klyte.TransportLinesManager.Utils
{
    public class TLMBuildingUtils
    {
        public static string GetBuildingDetails(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, out NamingType namingType, ushort lineId = 0)
        {

            BuildingManager bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingId].m_parentBuilding > 0)
            {
                buildingId = Building.FindParentBuilding(buildingId);
            }
            ref Building b = ref bm.m_buildings.m_buffer[buildingId];
            InstanceID iid = default;
            iid.Building = buildingId;
            serviceFound = b.Info?.GetService() ?? default;
            subserviceFound = b.Info?.GetSubService() ?? default;
            TransportSystemDefinition tsd = TransportSystemDefinition.From(b.Info.GetAI());
            if (tsd is null)
            {
                var data = TLMBaseConfigXML.CurrentContextConfig.GetAutoNameData(serviceFound);
                prefix = data?.NamingPrefix?.Trim();
                namingType = NamingTypeExtensions.From(serviceFound, subserviceFound);

            }
            else
            {
                prefix = tsd.GetConfig().NamingPrefix?.TrimStart();
                LogUtils.DoLog($"getBuildingName(): serviceFound {serviceFound} - subserviceFound = {subserviceFound} - tsd = {tsd} - prefix = {prefix}");
                namingType = NamingTypeExtensions.From(tsd);
            }
            var targetName = bm.GetBuildingName(buildingId, iid);
            if (targetName?.StartsWith("BUILDING_TITLE") ?? true)
            {
                targetName = b.Info.GetUncheckedLocalizedTitle();
            }
            return targetName;

        }
    }

}

