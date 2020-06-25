using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using System.Collections.Generic;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMUtils
    {
        public static readonly TransferManager.TransferReason[] defaultAllowedVehicleTypes = {
            TransferManager.TransferReason.Blimp ,
            TransferManager.TransferReason.CableCar ,
            TransferManager.TransferReason.Ferry ,
            TransferManager.TransferReason.MetroTrain ,
            TransferManager.TransferReason.Monorail ,
            TransferManager.TransferReason.PassengerTrain ,
            TransferManager.TransferReason.PassengerPlane ,
            TransferManager.TransferReason.PassengerShip ,
            TransferManager.TransferReason.Tram ,
            TransferManager.TransferReason.Bus
        };



        #region Naming Utils
      
        #endregion

        #region Building Utils
        public static string GetBuildingDetails(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, ushort lineId = 0)
        {

            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;

            Building b = bm.m_buildings.m_buffer[buildingId];
            while (b.m_parentBuilding > 0)
            {
                DoLog("getBuildingName(): building id {0} - parent = {1}", buildingId, b.m_parentBuilding);
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
            DoLog($"getBuildingName(): serviceFound {serviceFound} - subserviceFound = {subserviceFound} - tsd = {tsd} - index = {index} - prefix = {prefix}");

            return bm.GetBuildingName(buildingId, iid);
        }
        #endregion
        #region Logging
        public static void DoLog(string format, params object[] args) => LogUtils.DoLog(format, args);
        public static void DoErrorLog(string format, params object[] args) => LogUtils.DoErrorLog(format, args);
        #endregion

        internal static List<string> LoadBasicAssets(ref TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();

            TLMUtils.DoLog("LoadBasicAssets: pre prefab read");
            for (uint num = 0u; num < (ulong)PrefabCollection<VehicleInfo>.PrefabCount(); num += 1u)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && definition.IsFromSystem(prefab) && !VehicleUtils.IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
    }

}

