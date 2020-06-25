using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using System;
using System.Collections.Generic;
using UnityEngine;
using CIdx = Klyte.TransportLinesManager.TLMConfigWarehouse.ConfigIndex;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    public static class TLMStationUtils
    {

        #region Station


        private static string GetStationNameWithPrefix(TLMCW.ConfigIndex transportType, string name) => transportType.GetSystemStationNamePrefix().Trim() + (transportType.GetSystemStationNamePrefix().Trim() != string.Empty ? " " : "") + name;
        public static void SetStopName(string newName, ushort stopId, ushort lineId, Action callback)
        {
            TLMUtils.DoLog("setStopName! {0} - {1} - {2}", newName, stopId, lineId);
            ushort buildingId = GetStationBuilding(stopId, Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService, true, true);
            if (buildingId == 0)
            {
                TLMUtils.DoLog("b=0");
                Singleton<BuildingManager>.instance.StartCoroutine(SetNodeName(stopId, newName, callback));
            }
            else
            {
                TLMUtils.DoLog("b≠0 ({0})", buildingId);
                Singleton<BuildingManager>.instance.StartCoroutine(BuildingUtils.SetBuildingName(buildingId, newName, callback));
            }
        }

        public static IEnumerator<object> SetNodeName(ushort nodeId, string name, Action function)
        {
            yield return Singleton<SimulationManager>.instance.AddAction<bool>(SetNodeName(nodeId, name));
            function();
        }
        public static string GetStopName(ushort stopId)
        {
            InstanceID id = default;
            id.NetNode = stopId;
            return InstanceManager.instance.GetName(id);
        }

        private static IEnumerator<bool> SetNodeName(ushort nodeId, string name)
        {
            bool result = false;
            NetNode.Flags flags = NetManager.instance.m_nodes.m_buffer[nodeId].m_flags;
            TLMUtils.DoLog($"SetNodeName({nodeId},{name}) {flags}");
            if (nodeId != 0 && flags != NetNode.Flags.None)
            {
                var id = default(InstanceID);
                id.NetNode = nodeId;
                Singleton<InstanceManager>.instance.SetName(id, name.IsNullOrWhiteSpace() ? null : name);
                result = true;
            }
            yield return result;
            yield break;

        }

        public static string GetStationName(ushort stopId, ushort lineId, ItemClass.SubService ss, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, out ushort buildingID, out NamingType resultNamingType, bool excludeCargo = false, bool useRestrictionForAreas = false)
        {
            string savedName = GetStopName(stopId);
            var tsd = TransportSystemDefinition.From(lineId);
            if (savedName != null)
            {
                serviceFound = ItemClass.Service.PublicTransport;
                subserviceFound = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService;
                prefix = tsd.ToConfigIndex().GetSystemStationNamePrefix(lineId)?.TrimStart();
                buildingID = 0;
                resultNamingType = NamingTypeExtensions.From(serviceFound, subserviceFound);
                return savedName;
            }

            NetManager nm = Singleton<NetManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[stopId];
            Vector3 location = nn.m_position;
            if (tsd.VehicleType == VehicleInfo.VehicleType.Car || tsd.VehicleType == VehicleInfo.VehicleType.Tram)
            {
                List<ushort> nearStops = StopSearchUtils.FindNearStops(location, nn.Info.GetService(), true, 50f, out _, out _);

                foreach (ushort otherStop in nearStops)
                {
                    if (otherStop != stopId)
                    {
                        savedName = GetStopName(otherStop);
                        ;
                        if (savedName != null)
                        {
                            ushort targetLineId = NetManager.instance.m_nodes.m_buffer[otherStop].m_transportLine;
                            var tsd2 = TransportSystemDefinition.From(targetLineId);

                            serviceFound = ItemClass.Service.PublicTransport;
                            subserviceFound = Singleton<TransportManager>.instance.m_lines.m_buffer[targetLineId].Info.m_class.m_subService;
                            prefix = tsd2.ToConfigIndex().GetSystemStationNamePrefix(targetLineId)?.TrimStart();
                            buildingID = 0;
                            resultNamingType = NamingTypeExtensions.From(serviceFound, subserviceFound);
                            return savedName;
                        }
                    }
                }
            }

            buildingID = GetStationBuilding(stopId, ss, excludeCargo);

            if (buildingID > 0)
            {
                string name = TLMUtils.GetBuildingDetails(buildingID, out serviceFound, out subserviceFound, out prefix, lineId);
                resultNamingType = NamingTypeExtensions.From(serviceFound, subserviceFound);
                return name;
            }


            prefix = "";
            byte parkId = DistrictManager.instance.GetPark(location);
            if (parkId > 0)
            {
                var idx = DistrictManager.instance.m_parks.m_buffer[parkId].ToConfigIndex();
                if (!useRestrictionForAreas || TLMCW.GetCurrentConfigBool(idx | TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF))
                {
                    prefix = idx.GetSystemStationNamePrefix(lineId)?.TrimStart();
                    serviceFound = idx.ToServiceSubservice(out subserviceFound);
                    resultNamingType = idx switch
                    {
                        CIdx.CAMPUS_AREA_NAME_CONFIG => NamingType.CAMPUS,
                        CIdx.INDUSTRIAL_AREA_NAME_CONFIG => NamingType.INDUSTRY_AREA,
                        _ => NamingType.PARKAREA,
                    };
                    return DistrictManager.instance.GetParkName(parkId);
                }
            }
            if (SegmentUtils.GetAddressStreetAndNumber(location, location, out int number, out string streetName) && (!useRestrictionForAreas || TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.ADDRESS_NAME_CONFIG | TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF)) && !string.IsNullOrEmpty(streetName))
            {
                prefix = TLMCW.ConfigIndex.ADDRESS_NAME_CONFIG.GetSystemStationNamePrefix(lineId)?.TrimStart();
                serviceFound = ItemClass.Service.Road;
                subserviceFound = ItemClass.SubService.PublicTransportBus;
                resultNamingType = NamingType.ADDRESS;
                return streetName + ", " + number;

            }
            else if (DistrictManager.instance.GetDistrict(location) > 0 && (!useRestrictionForAreas || TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.DISTRICT_NAME_CONFIG | TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF)))
            {
                prefix = TLMCW.ConfigIndex.DISTRICT_NAME_CONFIG.GetSystemStationNamePrefix(lineId)?.TrimStart();
                serviceFound = ItemClass.Service.Natural;
                subserviceFound = ItemClass.SubService.None;
                resultNamingType = NamingType.DISTRICT;
                return DistrictManager.instance.GetDistrictName(DistrictManager.instance.GetDistrict(location));
            }
            else
            {
                serviceFound = ItemClass.Service.None;
                subserviceFound = ItemClass.SubService.None;
                resultNamingType = NamingType.NONE;
                return "????????";
            }
        }


        //ORDEM DE BUSCA DE CONFIG
        private static CIdx[] m_searchOrderStationNamingRule = new CIdx[] {
        CIdx.PLANE_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.SHIP_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.BLIMP_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.FERRY_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.CABLE_CAR_USE_FOR_AUTO_NAMING_REF              ,
        CIdx.TRAIN_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.METRO_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.MONORAIL_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.TRAM_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.BUS_USE_FOR_AUTO_NAMING_REF                    ,
        CIdx.TOUR_PED_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.TOUR_BUS_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.BALOON_USE_FOR_AUTO_NAMING_REF                 ,
        CIdx.TAXI_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF        ,
        CIdx.MONUMENT_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.BEAUTIFICATION_USE_FOR_AUTO_NAMING_REF         ,
        CIdx.TOURISM_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.NATURAL_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.DISASTER_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.HEALTHCARE_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.FIREDEPARTMENT_USE_FOR_AUTO_NAMING_REF         ,
        CIdx.POLICEDEPARTMENT_USE_FOR_AUTO_NAMING_REF       ,
        CIdx.EDUCATION_USE_FOR_AUTO_NAMING_REF              ,
        CIdx.GARBAGE_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.ROAD_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.CITIZEN_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.ELECTRICITY_USE_FOR_AUTO_NAMING_REF            ,
        CIdx.WATER_USE_FOR_AUTO_NAMING_REF                  ,

        CIdx.OFFICE_USE_FOR_AUTO_NAMING_REF                 ,
        CIdx.COMMERCIAL_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.INDUSTRIAL_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.RESIDENTIAL_USE_FOR_AUTO_NAMING_REF            ,

        //CIdx.UNUSED2_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.CAMPUS_AREA_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.PARKAREA_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.INDUSTRIAL_AREA_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.DISTRICT_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.ADDRESS_USE_FOR_AUTO_NAMING_REF                ,
        };


        public static string GetStationName(ushort stopId, ushort lineId, ItemClass.SubService ss) => GetStationName(stopId, lineId, ss, out ItemClass.Service serv, out ItemClass.SubService subServ, out string prefix, out ushort buildingId, out NamingType namingType, true);
        public static string GetFullStationName(ushort stopId, ushort lineId, ItemClass.SubService ss)
        {
            string result = GetStationName(stopId, lineId, ss, out ItemClass.Service serv, out ItemClass.SubService subServ, out string prefix, out ushort buildingId, out NamingType namingType, true);
            return string.IsNullOrEmpty(prefix) ? result : prefix + " " + result;
        }
        public static Vector3 GetStationBuildingPosition(uint stopId, ItemClass.SubService ss)
        {
            ushort buildingId = GetStationBuilding(stopId, ss);


            if (buildingId > 0)
            {
                BuildingManager bm = Singleton<BuildingManager>.instance;
                Building b = bm.m_buildings.m_buffer[buildingId];
                InstanceID iid = default;
                iid.Building = buildingId;
                return b.m_position;
            }
            else
            {
                NetManager nm = Singleton<NetManager>.instance;
                NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
                return nn.m_position;
            }
        }

        public static ushort GetStationBuilding(uint stopId, ItemClass.SubService ss, bool excludeCargo = false, bool restrictToTransportType = false)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            ushort tempBuildingId;


            if (ss != ItemClass.SubService.None)
            {
                tempBuildingId = BuildingUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.None);
                if (IsBuildingValidForStation(excludeCargo, bm, tempBuildingId))
                {
                    var parent = Building.FindParentBuilding(tempBuildingId);
                    return parent == 0 ? tempBuildingId : parent;
                }
            }
            if (!restrictToTransportType)
            {
                if (nn.m_transportLine > 0)
                {
                    tempBuildingId = BuildingUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, TLMCW.getTransferReasonFromSystemId(TransportSystemDefinition.From(TransportManager.instance.m_lines.m_buffer[nn.m_transportLine].Info).ToConfigIndex()), Building.Flags.None, Building.Flags.None);
                    if (IsBuildingValidForStation(excludeCargo, bm, tempBuildingId))
                    {
                        var parent = Building.FindParentBuilding(tempBuildingId);
                        return parent == 0 ? tempBuildingId : parent;
                    }
                }


                foreach (CIdx idx in m_searchOrderStationNamingRule)
                {
                    if (TLMCW.GetCurrentConfigBool(idx))
                    {
                        tempBuildingId = BuildingUtils.FindBuilding(nn.m_position, 100f, (ItemClass.Service)((int)idx & (int)CIdx.DESC_DATA), TLMCW.getSubserviceFromSystemId(idx), null, Building.Flags.None, Building.Flags.None);
                        if (IsBuildingValidForStation(excludeCargo, bm, tempBuildingId))
                        {
                            var parent = Building.FindParentBuilding(tempBuildingId);
                            return parent == 0 ? tempBuildingId : parent;
                        }
                    }

                }
            }
            return 0;

        }

        private static bool IsBuildingValidForStation(bool excludeCargo, BuildingManager bm, ushort tempBuildingId)
        {
            var ai = bm.m_buildings.m_buffer[tempBuildingId].Info.m_buildingAI;
            return tempBuildingId > 0 && ((!excludeCargo && (ai is DepotAI || ai is CargoStationAI)) || ai is TransportStationAI || ai is OutsideConnectionAI);
        }
        #endregion
    }

}
