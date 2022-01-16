using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.Cache
{
    public class BuildingTransportDataCache
    {
        internal NonSequentialList<InnerBuildingLine> RegionalLines { get; } = new NonSequentialList<InnerBuildingLine>();
        private ushort BuildingId { get; }
        public int RegionalLinesCount => RegionalLines.Count;
        public StopPointDescriptorLanes[] StopPoints { get; }

        public TLMBuildingsConfiguration BuildingData => TLMBuildingDataContainer.Instance.SafeGet(BuildingId);

        public BuildingTransportDataCache(ushort buildingId, ref Building b, TransportStationAI tsai)
        {
            BuildingId = buildingId;
            RemapLines(buildingId, ref b, tsai);
            StopPoints = MapStopPoints();
        }



        public void RemapLines()
        {
            RegionalLines.Clear();
            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[BuildingId];
            RemapLines(BuildingId, ref b, b.Info.m_buildingAI as TransportStationAI);
        }

        private void RemapLines(ushort buildingId, ref Building b, TransportStationAI tsai)
        {
            MapBuildingLines(buildingId, buildingId);
            var nextSubBuildingId = b.m_subBuilding;
            do
            {
                MapBuildingLines(buildingId, nextSubBuildingId);
                nextSubBuildingId = BuildingManager.instance.m_buildings.m_buffer[nextSubBuildingId].m_subBuilding;
            } while (nextSubBuildingId != 0);

            foreach (var item in RegionalLines.Values)
            {
                item.UpdateMeshData();
            }
            TransportLinesManagerMod.Controller.BuildingLines.InvalidateLinesCache();
        }

        public InnerBuildingLine SafeGetRegionalLine(ushort lineId) => RegionalLines.TryGetValue(lineId, out InnerBuildingLine result) ? result : null;

        public void RenderStopPoints(RenderManager.CameraInfo cameraInfo)
        {
            NetManager instance = NetManager.instance;
            for (int i = 0; i < StopPoints.Length; i++)
            {

                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo,
                   TLMController.COLOR_ORDER[i % TLMController.COLOR_ORDER.Length],
                     instance.m_lanes.m_buffer[StopPoints[i].laneId].m_bezier.Position(0.5f),
                     StopPoints[i].width * 2,
                   -1, 1280f, false, true);
            }
        }
        public void RenderLines(RenderManager.CameraInfo cameraInfo)
        {
            foreach (var line in RegionalLines.Values)
            {
                line.RenderLine(cameraInfo);
            }
        }


        private void MapBuildingLines(ushort buildingIdKey, ushort buildingId)
        {
            var nextNodeId = BuildingManager.instance.m_buildings.m_buffer[buildingId].m_netNode;
            do
            {
                var currentNode = nextNodeId;
                ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[currentNode];

                if (!(node.Info.m_netAI is TransportLineAI))
                {
                    break;
                }
                if (RegionalLines.Any(x => x.Value.SrcStop == nextNodeId || x.Value.DstStop == nextNodeId))
                {
                    nextNodeId = node.m_nextBuildingNode;
                    continue;
                }
                var otherNode = NetManager.instance.m_segments.m_buffer[node.m_segment0].GetOtherNode(nextNodeId);
                var thisBuilding = TLMStationUtils.GetStationBuilding(nextNodeId, buildingIdKey, true);
                var otherNodeBuilding = TLMStationUtils.GetStationBuilding(otherNode, buildingIdKey, true);

                if (thisBuilding != buildingIdKey)
                {
                    var x = thisBuilding;
                    thisBuilding = otherNodeBuilding;
                    otherNodeBuilding = x;

                    x = nextNodeId;
                    nextNodeId = otherNode;
                    otherNode = x;
                }

                ref Building outsideConnectionBuilding = ref BuildingManager.instance.m_buildings.m_buffer[otherNodeBuilding];
                var tsd = TransportSystemDefinition.FromOutsideConnection(outsideConnectionBuilding.Info.GetSubService(), outsideConnectionBuilding.Info.GetClassLevel(), VehicleInfo.VehicleType.None);
                if(tsd is null) // Unsupported regional line type
                {
                    nextNodeId = node.m_nextBuildingNode;
                    continue;
                }
                InnerBuildingLine transportLine =
                    new InnerBuildingLine
                    {
                        Info = tsd.GetTransportInfoIntercity(),
                        SrcStop = nextNodeId,
                        DstStop = otherNode,
                        SrcBuildingId = thisBuilding,
                        DstBuildingId = otherNodeBuilding
                    };
                RegionalLines[transportLine.SrcStop] = transportLine;
                nextNodeId = node.m_nextBuildingNode;
            } while (nextNodeId != 0);
        }

        private StopPointDescriptorLanes[] MapStopPoints() => MapStopPoints(BuildingId, 1f);
        private StopPointDescriptorLanes[] MapStopPoints(ushort buildingId, float thresold, bool isSubBuilding = false)
        {
            var result = new List<StopPointDescriptorLanes>();

            var buildingNoodeIds = new List<ushort>();

            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];

            var nextNodeId = b.m_netNode;
            var nm = NetManager.instance;
            while (nextNodeId > 0)
            {
                buildingNoodeIds.Add(nextNodeId);
                nextNodeId = nm.m_nodes.m_buffer[nextNodeId].m_nextBuildingNode;
            }

            var mappedSegments = new List<ushort>();

            for (int i = 0; i < buildingNoodeIds.Count; i++)
            {
                var nodeId = buildingNoodeIds[i];
                ref NetNode node = ref nm.m_nodes.m_buffer[nodeId];

                for (int j = 0; j < 8; j++)
                {
                    var segmentId = node.GetSegment(j);

                    if (segmentId == 0)
                    {
                        break;
                    }
                    ref NetSegment segment = ref nm.m_segments.m_buffer[segmentId];
                    var otherNodeId = segment.GetOtherNode(nodeId);
                    if (buildingNoodeIds.Contains(otherNodeId) && !mappedSegments.Contains(segmentId) && (segment.m_flags & NetSegment.Flags.Untouchable) != 0)
                    {
                        mappedSegments.Add(segmentId);
                        var segmentInfo = segment.Info;
                        var srcNodeId = segment.m_startNode;
                        var dstNodeId = segment.m_endNode;

                        ref NetNode srcNode = ref nm.m_nodes.m_buffer[srcNodeId];
                        ref NetNode dstNode = ref nm.m_nodes.m_buffer[dstNodeId];

                        Vector3 srcNodePos = srcNode.m_position;
                        Vector3 dstNodePos = dstNode.m_position;


                        srcNodePos.z *= -1;
                        dstNodePos.z *= -1;
                        Vector3 directionPath = Quaternion.AngleAxis(90, Vector3.up) * (dstNodePos - srcNodePos).normalized;
                        var nextLaneId = segment.m_lanes;
                        int k = 0;
                        while (nextLaneId != 0)
                        {
                            if (MapLane(nextLaneId, k, ref segment, directionPath, segment.Info.m_lanes[k], out StopPointDescriptorLanes mappingResult))
                            {
                                result.Add(mappingResult);
                            }
                            nextLaneId = nm.m_lanes.m_buffer[nextLaneId].m_nextLane;
                            k++;
                        }
                    }
                }
            }
            if (!isSubBuilding)
            {
            var subBuildingId = b.m_subBuilding;
            var subbuildingIndex = 0;
            while (subBuildingId > 0)
            {
                    StopPointDescriptorLanes[] subPlats = MapStopPoints(subBuildingId, thresold, true);
                if (subPlats != null)
                {
                    result.AddRange(subPlats.Select(x =>
                    {
                        x.subbuildingId = (sbyte)subbuildingIndex;
                        return x;
                    }));
                }
                subBuildingId = BuildingManager.instance.m_buildings.m_buffer[subBuildingId].m_subBuilding;
                subbuildingIndex++;
            }
            }
            result = result.OrderByDescending(x => x.subbuildingId).GroupBy(x => x.platformLaneId).Select(x => x.First()).ToList();
            result.Sort((x, y) =>
            {
                int priorityX = StopSearchUtils.VehicleToPriority(x.vehicleType);
                int priorityY = StopSearchUtils.VehicleToPriority(y.vehicleType);
                if (priorityX != priorityY)
                {
                    return priorityX.CompareTo(priorityY);
                }

                Vector3 centerX = (x.platformLine.Position(0.5f));
                Vector3 centerY = (y.platformLine.Position(0.5f));
                if (Mathf.Abs(centerX.y - centerY.y) >= thresold)
                {
                    return -centerX.y.CompareTo(centerY.y);
                }

                if (Mathf.Abs(centerX.z - centerY.z) >= thresold)
                {
                    return -centerX.z.CompareTo(centerY.z);
                }

                return -centerX.x.CompareTo(centerY.x);
            });

            return result.ToArray();
        }
        private static bool MapLane(uint laneId, int laneIdx, ref NetSegment segment, Vector3 directionPath, NetInfo.Lane refLane, out StopPointDescriptorLanes result)
        {
            result = default;
            if (refLane.m_stopType == VehicleInfo.VehicleType.None)
            {
                return false;
            }

            segment.GetLeftAndRightLanes(segment.m_startNode, NetInfo.LaneType.Vehicle, refLane.m_stopType, laneIdx, false, out int leftIdx, out int rightIdx, out uint leftLane, out uint rightLane);
            ref NetLane nl = ref NetManager.instance.m_lanes.m_buffer[laneId];
            if (leftIdx >= 0 && rightIdx >= 0)
            {
                var lanes = segment.Info.m_lanes;
                if (Mathf.Abs(lanes[laneIdx].m_position - lanes[leftIdx].m_position) < Mathf.Abs(lanes[laneIdx].m_position - lanes[rightIdx].m_position))
                {
                    rightIdx = -1;
                }
                else
                {
                    leftIdx = -1;
                }
            }
            if (leftLane > 0 && leftIdx >= 0)
            {
                result = new StopPointDescriptorLanes
                {
                    platformLine = nl.m_bezier,
                    width = refLane.m_width,
                    vehicleType = refLane.m_stopType,
                    laneId = leftLane,
                    platformLaneId = laneId,
                    subbuildingId = -1,
                    directionPath = directionPath * ((segment.m_flags & NetSegment.Flags.Invert) != 0 == (refLane.m_finalDirection == NetInfo.Direction.AvoidForward || refLane.m_finalDirection == NetInfo.Direction.Backward) ? 1 : -1)
                };
                return true;

            }
            if (rightLane > 0 && rightIdx >= 0)
            {
                result = new StopPointDescriptorLanes
                {
                    platformLine = nl.m_bezier,
                    width = refLane.m_width,
                    vehicleType = refLane.m_stopType,
                    laneId = rightLane,
                    platformLaneId = laneId,
                    subbuildingId = -1,
                    directionPath = directionPath * ((segment.m_flags & NetSegment.Flags.Invert) != 0 == (refLane.m_finalDirection == NetInfo.Direction.AvoidForward || refLane.m_finalDirection == NetInfo.Direction.Backward) ? 1 : -1)
                };
                return true;

            }
            return false;
        }
        internal void GetPlatformData(ushort platformIdx, out PlatformConfig dataObj)
        {
            var targetLaneIdx = StopPoints[platformIdx];
            if (!BuildingData.PlatformMappings.TryGetValue(targetLaneIdx.platformLaneId, out dataObj))
            {
                dataObj = BuildingData.PlatformMappings[targetLaneIdx.platformLaneId] = new PlatformConfig();
                dataObj.VehicleLaneId = targetLaneIdx.laneId;
            }
        }
        public void AddRegionalLine(ushort platformId, ushort outsideConnectionId, string name, Color clr)
        {
            GetPlatformData(platformId, out PlatformConfig dataObj);
            dataObj.AddDestination(BuildingId, outsideConnectionId, name, clr);
            RemapLines();
        }
        public void RemoveRegionalLine(ushort platformId, ushort outsideConnectionId)
        {
            GetPlatformData(platformId, out PlatformConfig dataObj);
            dataObj.RemoveDestination(BuildingId, outsideConnectionId);
            RemapLines();
        }

    }
}