using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.Cache
{
    public class BuildingTransportData
    {
        private List<InnerBuildingLine> RegionalLines { get; } = new List<InnerBuildingLine>();
        private ushort BuildingId { get; }
        public int RegionalLinesCount => RegionalLines.Count;
        private StopPointDescriptorLanes[] StopPoints { get; }

        public BuildingTransportData(ushort buildingId, ref Building b, TransportStationAI tsai)
        {
            BuildingId = buildingId;
            var useSecInfo = tsai.UseSecondaryTransportInfoForConnection();
            var targetInfo = useSecInfo ? tsai.m_secondaryTransportInfo : tsai.m_transportInfo;
            MapBuildingLines(buildingId, buildingId, targetInfo);
            var nextSubBuildingId = b.m_subBuilding;
            do
            {
                MapBuildingLines(buildingId, nextSubBuildingId, targetInfo);
                nextSubBuildingId = BuildingManager.instance.m_buildings.m_buffer[nextSubBuildingId].m_subBuilding;
            } while (nextSubBuildingId != 0);

            for (ushort i = 0; i < RegionalLines.Count; i++)
            {
                RegionalLines[i].UpdateMeshData();
            }

            StopPoints = MapStopPoints();
        }

        public InnerBuildingLine SafeGetRegionalLine(ushort lineId) => lineId < RegionalLines.Count ? RegionalLines[lineId] : null;

        public void RenderStopPoints(RenderManager.CameraInfo cameraInfo)
        {
            NetManager instance = NetManager.instance;
            for (int i = 0; i < StopPoints.Length; i++)
            {

                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo,
                   BuildingTransportLinesCache.COLOR_ORDER[i % BuildingTransportLinesCache.COLOR_ORDER.Length],
                     instance.m_lanes.m_buffer[StopPoints[i].laneId].m_bezier.Position(0.5f),
                     StopPoints[i].width * 2,
                   -1, 1280f, false, true);
            }
        }
        public void RenderLines(RenderManager.CameraInfo cameraInfo)
        {
            for (ushort i = 0; i < RegionalLines.Count; i++)
            {
                RegionalLines[i].RenderLine(cameraInfo, i);
            }
        }


        private void MapBuildingLines(ushort buildingIdKey, ushort buildingId, TransportInfo targetInfo)
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
                InnerBuildingLine transportLine =
                    TLMStationUtils.GetStationBuilding(nextNodeId, (ushort)RegionalLines.Count, buildingId) != buildingIdKey
                        ? new InnerBuildingLine
                        {
                            Info = targetInfo,
                            DstStop = nextNodeId,
                            SrcStop = NetManager.instance.m_segments.m_buffer[node.m_segment0].GetOtherNode(nextNodeId)
                        }
                        : new InnerBuildingLine
                        {
                            Info = targetInfo,
                            SrcStop = nextNodeId,
                            DstStop = NetManager.instance.m_segments.m_buffer[node.m_segment0].GetOtherNode(nextNodeId)
                        };
                RegionalLines.Add(transportLine);
                nextNodeId = node.m_nextBuildingNode;
            } while (nextNodeId != 0);
        }

        private StopPointDescriptorLanes[] MapStopPoints() => MapStopPoints(BuildingId, 1f);
        private StopPointDescriptorLanes[] MapStopPoints(ushort buildingId, float thresold)
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
                            if (MapLane(nextLaneId, ref segment, ref srcNode, ref dstNode, directionPath, segment.Info.m_lanes[k], out StopPointDescriptorLanes mappingResult))
                            {
                                result.Add(mappingResult);
                            }
                            nextLaneId = nm.m_lanes.m_buffer[nextLaneId].m_nextLane;
                            k++;
                        }
                    }
                }
            }
            var subBuildingId = b.m_subBuilding;
            var subbuildingIndex = 0;
            while (subBuildingId > 0)
            {
                StopPointDescriptorLanes[] subPlats = MapStopPoints(subBuildingId, thresold);
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
        private static bool MapLane(uint laneId, ref NetSegment segment, ref NetNode src, ref NetNode dst, Vector3 directionPath, NetInfo.Lane refLane, out StopPointDescriptorLanes result)
        {
            result = default;
            if (refLane.m_stopType == VehicleInfo.VehicleType.None)
            {
                return false;
            }
            ref NetLane nl = ref NetManager.instance.m_lanes.m_buffer[laneId];
            result = new StopPointDescriptorLanes
            {
                platformLine = nl.m_bezier,
                width = refLane.m_width,
                vehicleType = refLane.m_stopType,
                laneId = laneId,
                subbuildingId = -1,
                directionPath = directionPath * ((segment.m_flags & NetSegment.Flags.Invert) != 0 == (refLane.m_finalDirection == NetInfo.Direction.AvoidForward || refLane.m_finalDirection == NetInfo.Direction.Backward) ? 1 : -1)
            };
            return true;
        }

    }
}