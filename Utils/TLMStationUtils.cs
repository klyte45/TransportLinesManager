using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemClass;


namespace Klyte.TransportLinesManager.Utils
{
    internal static class TLMStationUtils
    {

        #region Station
        public static void SetStopName(string newName, ushort stopId, ushort lineId, Action callback)
        {
            if (lineId == 0)
            {
                return;
            }

            LogUtils.DoLog("setStopName! {0} - {1} - {2}", newName, stopId, lineId);
            ushort buildingId = GetStationBuilding(stopId, Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService, true, true);
            if (buildingId == 0)
            {
                LogUtils.DoLog("b=0");
                Singleton<BuildingManager>.instance.StartCoroutine(SetNodeName(stopId, newName, callback));
            }
            else
            {
                LogUtils.DoLog("b≠0 ({0})", buildingId);
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
            InstanceID id = new InstanceID
            {
                NetNode = stopId
            };
            return InstanceManager.instance.GetName(id);
        }

        private static IEnumerator<bool> SetNodeName(ushort nodeId, string name)
        {
            bool result = false;
            NetNode.Flags flags = NetManager.instance.m_nodes.m_buffer[nodeId].m_flags;
            LogUtils.DoLog($"SetNodeName({nodeId},{name}) {flags}");
            if (nodeId != 0 && flags != NetNode.Flags.None)
            {
                var id = default(InstanceID);
                id.NetNode = nodeId;
                Singleton<InstanceManager>.instance.SetName(id, name.IsNullOrWhiteSpace() ? null : name);
                result = true;
            }
            yield return result;
            TransportLinesManagerMod.Controller.SharedInstance.OnAutoNameParameterChanged();
        }

        public static string GetStationName(
            ushort stopId,
            ushort lineId,
            ItemClass.SubService ss,
            out ItemClass.Service serviceFound,
            out ItemClass.SubService subserviceFound,
            out string prefix,
            out ushort buildingID,
            out NamingType resultNamingType,
            ushort srcBuildingId,
            bool excludeCargo = false,
            bool useRestrictionForAreas = false,
            bool useRoadMainNameOnAddress = false)
        {
            if (lineId == 0 && srcBuildingId == 0)
            {
                buildingID = stopId;
                return GetBuildingNameForStation(lineId, out serviceFound, out subserviceFound, out prefix, stopId, out resultNamingType);
            }
            TransportSystemDefinition tsd = TransportSystemDefinition.FromLineId(lineId, srcBuildingId);
            string savedName = GetStopName(stopId);
            if (savedName != null)
            {
                serviceFound = ItemClass.Service.PublicTransport;
                subserviceFound = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService;
                prefix = tsd.GetConfig().NamingPrefix?.TrimStart();
                buildingID = 0;
                resultNamingType = NamingTypeExtensions.From(tsd);
                return savedName;
            }

            NetManager nm = Singleton<NetManager>.instance;
            ref NetNode nn = ref nm.m_nodes.m_buffer[stopId];
            Vector3 location = nn.m_position;
            if (tsd.VehicleType == VehicleInfo.VehicleType.Car || tsd.VehicleType == VehicleInfo.VehicleType.Tram || tsd.VehicleType == VehicleInfo.VehicleType.Trolleybus)
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
                            var tsd2 = TransportSystemDefinition.FromLineId(targetLineId, 0);

                            serviceFound = ItemClass.Service.PublicTransport;
                            subserviceFound = Singleton<TransportManager>.instance.m_lines.m_buffer[targetLineId].Info.m_class.m_subService;
                            prefix = tsd2.GetConfig().NamingPrefix?.TrimStart();
                            buildingID = 0;
                            resultNamingType = NamingTypeExtensions.From(tsd2);
                            return savedName;
                        }
                    }
                }
            }


            buildingID = GetStationBuilding(stopId, ss, excludeCargo);

            if (buildingID > 0)
            {
                return GetBuildingNameForStation(lineId, out serviceFound, out subserviceFound, out prefix, buildingID, out resultNamingType);
            }


            prefix = "";
            byte parkId = DistrictManager.instance.GetPark(location);
            if (parkId > 0)
            {
                var idx = DistrictManager.instance.m_parks.m_buffer[parkId].GetNamingClass();
                var idxConfig = idx.GetConfig();
                if (!useRestrictionForAreas || idxConfig.UseInAutoName)
                {
                    prefix = idxConfig.NamingPrefix?.TrimStart();
                    serviceFound = 0;
                    subserviceFound = 0;
                    resultNamingType = idx.ToNamingType();
                    return DistrictManager.instance.GetParkName(parkId);
                }
            }
            int number = 0;
            if ((useRoadMainNameOnAddress ? TransportLinesManagerMod.Controller.ConnectorADR.GetStreetSuffix(location, location, out string streetName) : TransportLinesManagerMod.Controller.ConnectorADR.GetAddressStreetAndNumber(location, location, out number, out streetName))
                && !streetName.IsNullOrWhiteSpace()
                && (!useRestrictionForAreas || TLMSpecialNamingClass.Address.GetConfig().UseInAutoName))
            {
                prefix = TLMSpecialNamingClass.Address.GetConfig().NamingPrefix?.TrimStart();
                serviceFound = ItemClass.Service.Road;
                subserviceFound = ItemClass.SubService.PublicTransportBus;
                resultNamingType = NamingType.ADDRESS;
                return useRoadMainNameOnAddress ? streetName : $"{number} {streetName}";

            }
            else if (DistrictManager.instance.GetDistrict(location) > 0 && (!useRestrictionForAreas || TLMSpecialNamingClass.District.GetConfig().UseInAutoName))
            {
                prefix = TLMSpecialNamingClass.District.GetConfig().NamingPrefix?.TrimStart();
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
                return "<Somewhere>";
            }
        }

        private static string GetBuildingNameForStation(ushort lineId, out Service serviceFound, out SubService subserviceFound, out string prefix, ushort buildingID, out NamingType resultNamingType)
        {
            string name = TLMBuildingUtils.GetBuildingDetails(buildingID, out serviceFound, out subserviceFound, out prefix, out resultNamingType, lineId);
            if (resultNamingType == NamingType.NONE)
            {
                resultNamingType = NamingTypeExtensions.From(serviceFound, subserviceFound);
            }
            return name;
        }

        public static Service[] GetUsableServiceInAutoName() => m_searchOrderStationNamingRule.OfType<Service>().ToArray();

        //ORDEM DE BUSCA DE CONFIG
        private static object[] m_searchOrderStationNamingRule = new object[] {
            TransportSystemDefinition.PLANE,
            TransportSystemDefinition.SHIP,
            TransportSystemDefinition.BLIMP,
            TransportSystemDefinition.FERRY,
            TransportSystemDefinition.CABLE_CAR,
            TransportSystemDefinition.TRAIN,
            TransportSystemDefinition.METRO,
            TransportSystemDefinition.MONORAIL,
            TransportSystemDefinition.TRAM,
            TransportSystemDefinition.BUS,
            TransportSystemDefinition.TOUR_PED,
            TransportSystemDefinition.TOUR_BUS,
            TransportSystemDefinition.BALLOON,
            TransportSystemDefinition.TAXI,
            ItemClass.Service.PublicTransport,
            ItemClass.Service.Monument,
            ItemClass.Service.Beautification,
            ItemClass.Service.Tourism,
            ItemClass.Service.Natural,
            ItemClass.Service.Disaster,
            ItemClass.Service.HealthCare,
            ItemClass.Service.FireDepartment,
            ItemClass.Service.PoliceDepartment,
            ItemClass.Service.Education,
            ItemClass.Service.Garbage,
            ItemClass.Service.Road,
            ItemClass.Service.Citizen,
            ItemClass.Service.Electricity,
            ItemClass.Service.Water,
            ItemClass.Service.Office,
            ItemClass.Service.Commercial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Residential,
            TLMSpecialNamingClass.Campus,
            TLMSpecialNamingClass.ParkArea,
            TLMSpecialNamingClass.Industrial,
            TLMSpecialNamingClass.District,
            TLMSpecialNamingClass.Address,
        };


        public static string GetStationName(ushort stopId, ushort lineId, ItemClass.SubService ss, ushort buildingId) => GetStationName(stopId, lineId, ss, out _, out _, out _, out _, out _, buildingId, excludeCargo: true);
        public static string GetFullStationName(ushort stopId, ushort lineId, ItemClass.SubService ss, ushort buildingId)
        {
            string result = GetStationName(stopId, lineId, ss, out _, out _, out string prefix, out _, out _, buildingId, true, false, false);
            return string.IsNullOrEmpty(prefix) ? result : prefix + " " + result;
        }
        public static Vector3 GetStationBuildingPosition(uint stopId, ItemClass.SubService ss)
        {
            ushort buildingId = GetStationBuilding(stopId, ss);
            if (buildingId > 0)
            {
                BuildingManager bm = Singleton<BuildingManager>.instance;
                ref Building b = ref bm.m_buildings.m_buffer[buildingId];
                InstanceID iid = default;
                iid.Building = buildingId;
                return b.m_position;
            }
            else
            {
                NetManager nm = Singleton<NetManager>.instance;
                ref NetNode nn = ref nm.m_nodes.m_buffer[(int)stopId];
                return nn.m_position;
            }
        }

        public static ushort GetStationBuilding(ushort stopId, ushort lineId, ushort srcBuildingId)
        {
            NetManager nm = Singleton<NetManager>.instance;
            if (stopId > nm.m_nodes.m_buffer.Length)
            {
                return stopId;
            }

            BuildingManager bm = Singleton<BuildingManager>.instance;
            ushort tempBuildingId;
            Vector3 position = nm.m_nodes.m_buffer[stopId].m_position;

            SubService ss = srcBuildingId == 0 ? TransportManager.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService : BuildingManager.instance.m_buildings.m_buffer[srcBuildingId].Info.m_class.m_subService;

            if (ss != ItemClass.SubService.None)
            {
                tempBuildingId = BuildingUtils.FindBuilding(position, 100f, ItemClass.Service.PublicTransport, ss, m_defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.None);

                if (BuildingManager.instance.m_buildings.m_buffer[tempBuildingId].m_parentBuilding != 0)
                {
                    tempBuildingId = Building.FindParentBuilding(tempBuildingId);
                }
                if (BuildingUtils.IsBuildingValidForStation(true, bm, tempBuildingId))
                {
                    return tempBuildingId;
                }
            }

            tempBuildingId = BuildingUtils.FindBuilding(position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, m_defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.None);
            if (BuildingManager.instance.m_buildings.m_buffer[tempBuildingId].m_parentBuilding != 0)
            {
                tempBuildingId = Building.FindParentBuilding(tempBuildingId);
            }
            if (BuildingUtils.IsBuildingValidForStation(true, bm, tempBuildingId))
            {
                return tempBuildingId;
            }


            tempBuildingId = BuildingUtils.FindBuilding(position, 100f, ItemClass.Service.Road, ItemClass.SubService.None, null, Building.Flags.None, Building.Flags.None);
            if (BuildingManager.instance.m_buildings.m_buffer[tempBuildingId].m_parentBuilding != 0)
            {
                tempBuildingId = Building.FindParentBuilding(tempBuildingId);
            }
            if (BuildingUtils.IsBuildingValidForStation(true, bm, tempBuildingId))
            {
                return tempBuildingId;
            }

            return 0;

        }

        private static readonly TransferManager.TransferReason[] m_defaultAllowedVehicleTypes = {
            TransferManager.TransferReason.Blimp ,
            TransferManager.TransferReason.CableCar ,
            TransferManager.TransferReason.Ferry ,
            TransferManager.TransferReason.MetroTrain ,
            TransferManager.TransferReason.Monorail ,
            TransferManager.TransferReason.PassengerTrain ,
            TransferManager.TransferReason.PassengerPlane ,
            TransferManager.TransferReason.PassengerShip ,
            TransferManager.TransferReason.Tram ,
            TransferManager.TransferReason.TouristBus ,
            TransferManager.TransferReason.IntercityBus ,
            TransferManager.TransferReason.Bus
        };

        public static ushort GetStationBuilding(uint stopId, ItemClass.SubService ss, bool excludeCargo = false, bool restrictToTransportType = false)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            ref NetNode nn = ref nm.m_nodes.m_buffer[(int)stopId];
            ushort tempBuildingId;

            if (ss != ItemClass.SubService.None)
            {
                tempBuildingId = BuildingUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMLineUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.None);
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
                    tempBuildingId = BuildingUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, TransportSystemDefinition.FromLocal(TransportManager.instance.m_lines.m_buffer[nn.m_transportLine].Info).Reasons, Building.Flags.None, Building.Flags.None);
                    if (IsBuildingValidForStation(excludeCargo, bm, tempBuildingId))
                    {
                        var parent = Building.FindParentBuilding(tempBuildingId);
                        return parent == 0 ? tempBuildingId : parent;
                    }
                }


                foreach (object idx in m_searchOrderStationNamingRule)
                {
                    ITLMAutoNameConfigurable conf;
                    Service serv;
                    SubService subserv;
                    if (idx is TransportSystemDefinition def)
                    {
                        conf = def.GetConfig();
                        serv = Service.PublicTransport;
                        subserv = def.SubService;
                    }
                    else if (idx is ItemClass.Service service)
                    {
                        conf = service.GetConfig();
                        serv = service;
                        subserv = 0;
                    }
                    else
                    {
                        continue;
                    }
                    if (conf.UseInAutoName)
                    {
                        tempBuildingId = BuildingUtils.FindBuilding(nn.m_position, 100f, serv, subserv, null, Building.Flags.None, Building.Flags.None);
                        if (tempBuildingId > 0 && (!(idx is TransportSystemDefinition) || IsBuildingValidForStation(excludeCargo, bm, tempBuildingId)))
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
