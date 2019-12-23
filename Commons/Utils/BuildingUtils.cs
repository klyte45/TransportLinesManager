using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemClass;

namespace Klyte.Commons.Utils
{
    public class BuildingUtils
    {
        #region Building Utils
        public static IEnumerator SetBuildingName(ushort buildingID, string name, Action function)
        {
            InstanceID buildingIdSelect = default;
            buildingIdSelect.Building = buildingID;
            yield return Singleton<SimulationManager>.instance.AddAction<bool>(Singleton<BuildingManager>.instance.SetBuildingName(buildingID, name));
            function();
        }

        public static ushort FindBuilding(Vector3 pos, float maxDistance, ItemClass.Service service, ItemClass.SubService subService, TransferManager.TransferReason[] allowedTypes, Building.Flags flagsRequired, Building.Flags flagsForbidden)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            //if (allowedTypes == null || allowedTypes.Length == 0)
            //{
            //    return bm.FindBuilding(pos, maxDistance, service, subService, flagsRequired, flagsForbidden);
            //}


            int num = Mathf.Max((int) (((pos.x - maxDistance) / 64f) + 135f), 0);
            int num2 = Mathf.Max((int) (((pos.z - maxDistance) / 64f) + 135f), 0);
            int num3 = Mathf.Min((int) (((pos.x + maxDistance) / 64f) + 135f), 269);
            int num4 = Mathf.Min((int) (((pos.z + maxDistance) / 64f) + 135f), 269);
            ushort result = 0;
            float currentDistance = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort buildingId = bm.m_buildingGrid[(i * 270) + j];
                    int num7 = 0;
                    while (buildingId != 0)
                    {
                        BuildingInfo info = bm.m_buildings.m_buffer[buildingId].Info;
                        if (!CheckInfoCompatibility(pos, service, subService, allowedTypes, flagsRequired, flagsForbidden, bm, ref result, ref currentDistance, buildingId, info) && info.m_subBuildings?.Length > 0)
                        {
                            foreach (BuildingInfo.SubInfo subBuilding in info.m_subBuildings)
                            {
                                if (subBuilding != null && CheckInfoCompatibility(pos, service, subService, allowedTypes, flagsRequired, flagsForbidden, bm, ref result, ref currentDistance, buildingId, subBuilding.m_buildingInfo))
                                {
                                    break;
                                }
                            }
                        }
                        buildingId = bm.m_buildings.m_buffer[buildingId].m_nextGridBuilding;
                        if (++num7 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private static bool CheckInfoCompatibility(Vector3 pos, ItemClass.Service service, ItemClass.SubService subService, TransferManager.TransferReason[] allowedTypes, Building.Flags flagsRequired, Building.Flags flagsForbidden, BuildingManager bm, ref ushort result, ref float lastNearest, ushort buildingId, BuildingInfo info)
        {
            //doErrorLog($"CheckInfoCompatibility 0  {pos}, {service}, {subService}, {allowedTypes},  {flagsRequired},  {flagsForbidden}, {bm},  {result},  {lastNearest},  {buildingId}, {info}");
            if (info != null && (info.m_class.m_service == service || service == ItemClass.Service.None) && (info.m_class.m_subService == subService || subService == ItemClass.SubService.None))
            {
                //doErrorLog("CheckInfoCompatibility 1");
                Building.Flags flags = bm.m_buildings.m_buffer[buildingId].m_flags;
                //doErrorLog("CheckInfoCompatibility 2");
                if ((flags & (flagsRequired | flagsForbidden)) == flagsRequired)
                {
                    //doErrorLog("CheckInfoCompatibility 3");
                    if (allowedTypes == null
                        || allowedTypes.Length == 0
                        || !(info.GetAI() is DepotAI depotAI)
                        || (depotAI.m_transportInfo != null && allowedTypes.Contains(depotAI.m_transportInfo.m_vehicleReason))
                        || (depotAI.m_secondaryTransportInfo != null && allowedTypes.Contains(depotAI.m_secondaryTransportInfo.m_vehicleReason)))
                    {
                        //doErrorLog("CheckInfoCompatibility 4");
                        float dist = Vector3.SqrMagnitude(pos - bm.m_buildings.m_buffer[buildingId].m_position);
                        //doErrorLog("CheckInfoCompatibility 5");
                        if (dist < lastNearest)
                        {
                            result = buildingId;
                            lastNearest = dist;
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        public static ushort GetBuildingDistrict(uint bId) => GetBuildingDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[bId]);
        public static ushort GetBuildingDistrict(Building b) => DistrictManager.instance.GetDistrict(b.m_position);
        public static ushort GetPark(Vector3 location) => Singleton<DistrictManager>.instance.GetPark(location);



        public static StopPointDescriptor[] GetAllSpawnPoints(BuildingAI buidlingAI)
        {
            if (!(buidlingAI is DepotAI depotAI))
            {
                return null;
            }
            var stops = new List<StopPointDescriptor>();
            if (depotAI.m_spawnPoints != null && depotAI.m_spawnPoints.Length != 0)
            {
                for (int i = 0; i < depotAI.m_spawnPoints.Length; i++)
                {
                    AddSpawnPoint(depotAI.m_transportInfo, stops, depotAI.m_spawnPoints[i].m_position, depotAI.m_spawnPoints[i].m_target, depotAI.m_canInvertTarget);
                }
            }
            else
            {
                AddSpawnPoint(depotAI.m_transportInfo, stops, depotAI.m_spawnPosition, depotAI.m_spawnTarget, depotAI.m_canInvertTarget);
            }
            if (depotAI.m_secondaryTransportInfo != null)
            {
                if (depotAI.m_spawnPoints2 != null && depotAI.m_spawnPoints2.Length != 0)
                {
                    for (int i = 0; i < depotAI.m_spawnPoints2.Length; i++)
                    {
                        AddSpawnPoint(depotAI.m_secondaryTransportInfo, stops, depotAI.m_spawnPoints2[i].m_position, depotAI.m_spawnPoints2[i].m_target, depotAI.m_canInvertTarget);
                    }
                }
                else
                {
                    AddSpawnPoint(depotAI.m_secondaryTransportInfo, stops, depotAI.m_spawnPosition2, depotAI.m_spawnTarget2, depotAI.m_canInvertTarget);
                }
            }
            foreach (BuildingInfo.SubInfo subBuilding in depotAI.m_info.m_subBuildings)
            {
                StopPointDescriptor[] subPlats = GetAllSpawnPoints(subBuilding.m_buildingInfo.m_buildingAI);
                if (subPlats != null)
                {
                    stops.AddRange(subPlats.Select(x =>
                    {
                        x.relativePosition -= subBuilding.m_position;
                        return x;
                    }));
                }
            }
            stops.Sort((x, y) =>
            {
                if (x.relativePosition.x != y.relativePosition.x)
                {
                    return x.relativePosition.x.CompareTo(y.relativePosition.x);
                }

                if (x.relativePosition.z != y.relativePosition.z)
                {
                    return x.relativePosition.z.CompareTo(y.relativePosition.z);
                }

                return x.relativePosition.y.CompareTo(y.relativePosition.y);
            });
            return stops.ToArray();
        }

        private static void AddSpawnPoint(TransportInfo info, List<StopPointDescriptor> stops, Vector3 position, Vector3 target, bool canInvert)
        {
            stops.Add(new StopPointDescriptor
            {
                relativePosition = target,
                vehicleType = info.m_vehicleType
            });
            if (canInvert)
            {
                stops.Add(new StopPointDescriptor
                {
                    relativePosition = (position * 2) - target,
                    vehicleType = info.m_vehicleType
                });
            }
        }

        public class StopPointDescriptor
        {
            public Vector3 relativePosition;
            public VehicleInfo.VehicleType vehicleType;
        }

        public static string GetBuildingName(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound)
        {

            BuildingManager bm = Singleton<BuildingManager>.instance;

            while (bm.m_buildings.m_buffer[buildingId].m_parentBuilding > 0)
            {
                LogUtils.DoLog("getBuildingName(): building id {0} - parent = {1}", buildingId, bm.m_buildings.m_buffer[buildingId].m_parentBuilding);
                buildingId = bm.m_buildings.m_buffer[buildingId].m_parentBuilding;
                bm.m_buildings.m_buffer[buildingId] = bm.m_buildings.m_buffer[buildingId];
            }
            InstanceID iid = default;
            iid.Building = buildingId;
            serviceFound = bm.m_buildings.m_buffer[buildingId].Info?.GetService() ?? default;
            subserviceFound = bm.m_buildings.m_buffer[buildingId].Info?.GetSubService() ?? default;

            return bm.GetBuildingName(buildingId, iid);
        }
        public static bool IsBuildingValidForStation(bool excludeCargo, BuildingManager bm, ushort tempBuildingId) => tempBuildingId > 0 && (
    !excludeCargo
    || !(bm.m_buildings.m_buffer[tempBuildingId].Info.m_buildingAI is DepotAI || bm.m_buildings.m_buffer[tempBuildingId].Info.m_buildingAI is CargoStationAI)
    || bm.m_buildings.m_buffer[tempBuildingId].Info.m_buildingAI is TransportStationAI
    );
      
        #endregion
    }
}
