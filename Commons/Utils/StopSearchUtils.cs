using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class StopSearchUtils
    {
        #region Stop Search Utils
        public static List<ushort> FindNearStops(Vector3 position) => FindNearStops(position, ItemClass.Service.PublicTransport, true, 24f, out _, out _);
        public static List<ushort> FindNearStops(Vector3 position, ItemClass.Service service, bool allowUnderground, float maxDistance, out List<float> distanceSqrA, out List<Vector3> stopPositions, List<Quad2> boundaries = null) => FindNearStops(position, service, service, VehicleInfo.VehicleType.None, allowUnderground, maxDistance, out distanceSqrA, out stopPositions, boundaries);
        public static List<ushort> FindNearStops(Vector3 position, ItemClass.Service service, ItemClass.Service service2, VehicleInfo.VehicleType stopType, bool allowUnderground, float maxDistance,
             out List<float> distanceSqrA, out List<Vector3> stopPositions, List<Quad2> boundaries = null)
        {


            var bounds = new Bounds(position, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int num = Mathf.Max((int) (((bounds.min.x - 64f) / 64f) + 135f), 0);
            int num2 = Mathf.Max((int) (((bounds.min.z - 64f) / 64f) + 135f), 0);
            int num3 = Mathf.Min((int) (((bounds.max.x + 64f) / 64f) + 135f), 269);
            int num4 = Mathf.Min((int) (((bounds.max.z + 64f) / 64f) + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            var result = new List<Tuple<ushort, float, Vector3>>();

            float maxDistSqr = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    int idx = (i * 270) + j;
                    ushort nodeId = 0;
                    int num7 = 0;
                    try
                    {
                        nodeId = instance.m_nodeGrid[idx];
                        num7 = 0;
                        while (nodeId != 0)
                        {
                            NetInfo info = instance.m_nodes.m_buffer[nodeId].Info;
                            if (info != null
                                && (info.m_class.m_service == service || info.m_class.m_service == service2)
                                && (instance.m_nodes.m_buffer[nodeId].m_flags & (NetNode.Flags.Collapsed)) == NetNode.Flags.None
                                && (instance.m_nodes.m_buffer[nodeId].m_flags & (NetNode.Flags.Created)) != NetNode.Flags.None
                                && instance.m_nodes.m_buffer[nodeId].m_transportLine > 0
                                && (allowUnderground || !info.m_netAI.IsUnderground())
                                && (stopType == VehicleInfo.VehicleType.None || stopType == TransportManager.instance.m_lines.m_buffer[instance.m_nodes.m_buffer[nodeId].m_transportLine].Info.m_vehicleType))
                            {
                                NetNode node = instance.m_nodes.m_buffer[nodeId];
                                Vector3 nodePos = node.m_position;
                                if (boundaries != null && boundaries.Count != 0 && !boundaries.Any(x => x.Intersect(VectorUtils.XZ(nodePos))))
                                {
                                    goto GOTO_NEXT;
                                }
                                float delta = Mathf.Max(Mathf.Max(bounds.min.x - 64f - nodePos.x, bounds.min.z - 64f - nodePos.z), Mathf.Max(nodePos.x - bounds.max.x - 64f, nodePos.z - bounds.max.z - 64f));
                                if (delta < 0f && instance.m_nodes.m_buffer[nodeId].m_bounds.Intersects(bounds))
                                {
                                    float num14 = Vector3.SqrMagnitude(position - nodePos);
                                    if (num14 < maxDistSqr)
                                    {
                                        result.Add(Tuple.New(nodeId, num14, nodePos));
                                    }
                                }
                            }
                            GOTO_NEXT:
                            nodeId = instance.m_nodes.m_buffer[nodeId].m_nextGridNode;
                            if (++num7 >= 36864)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogUtils.DoErrorLog($"ERROR ON TRYING FindNearStops: (It = {num7}; Init = {idx}; Curr = {nodeId})==>  {e.Message}\n{e.StackTrace}");
                    }
                }
            }
            result = result.OrderBy(x => x.First).ToList();
            distanceSqrA = result.Select(x => x.Second).ToList();
            stopPositions = result.Select(x => x.Third).ToList();
            return result.Select(x => x.First).ToList();
        }


        private const float m_defaultStopOffset = 0.5019608f;

        public struct StopPointDescriptorLanes
        {
            public Bezier3 platformLine;
            public float width;
            public VehicleInfo.VehicleType vehicleType;
            public ushort laneId;
            public sbyte subbuildingId;
            public Vector3 directionPath;

            public override string ToString() => $"{platformLine.Position(0.5f)} (w={width} | {vehicleType} | {subbuildingId} | {laneId} | DIR = {directionPath} ({directionPath.GetAngleXZ()}°))";
        }

        public static StopPointDescriptorLanes[] MapStopPoints(BuildingInfo buildingInfo, float thresold)
        {
            var result = new List<StopPointDescriptorLanes>();
            if (buildingInfo?.m_paths == null)
            {
                return result.ToArray();
            }

            foreach (BuildingInfo.PathInfo path in buildingInfo.m_paths)
            {
                Vector3 position = path.m_nodes[0];
                Vector3 position2 = path.m_nodes[1];


                position.z *= -1;
                position2.z *= -1;
                Vector3 directionPath = Quaternion.AngleAxis(90, Vector3.up) * (position2 - position).normalized;

                foreach (NetInfo.Lane refLane in path.m_netInfo.m_lanes)
                {
                    if (refLane.m_stopType == VehicleInfo.VehicleType.None)
                    {
                        continue;
                    }
                    NetInfo.Lane lane = FindNearestVehicleStopLane(path.m_netInfo.m_lanes, refLane, out ushort laneId);
                    if (lane == null)
                    {
                        continue;
                    }


                    LogUtils.DoLog($"[{buildingInfo}] pos + dir = ({position} {position2} + {directionPath})");
                    Vector3 lanePos = position + (lane.m_position / 2 * directionPath) + new Vector3(0, lane.m_verticalOffset);
                    Vector3 lanePos2 = position2 + (lane.m_position / 2 * directionPath) + new Vector3(0, lane.m_verticalOffset);
                    Vector3 b3, c;
                    if (path.m_curveTargets == null || path.m_curveTargets.Length == 0)
                    {
                        NetSegment.CalculateMiddlePoints(lanePos, Vector3.zero, lanePos2, Vector3.zero, true, true, out b3, out c);
                    }
                    else
                    {
                        GetMiddlePointsFor(path, out b3, out c);
                        LogUtils.DoLog($"[{buildingInfo}] GetMiddlePointsFor path =  ({b3} {c})");
                        b3 += (lane.m_position * directionPath) + new Vector3(0, lane.m_verticalOffset);
                        c += (lane.m_position * directionPath) + new Vector3(0, lane.m_verticalOffset);
                        b3.y = c.y = (lanePos.y + lanePos2.y) / 2;
                    }
                    var refBezier = new Bezier3(lanePos, b3, c, lanePos2);
                    LogUtils.DoLog($"[{buildingInfo}]refBezier = {refBezier} ({lanePos} {b3} {c} {lanePos2})");


                    Vector3 positionR = refBezier.Position(m_defaultStopOffset);
                    Vector3 direction = refBezier.Tangent(m_defaultStopOffset);
                    LogUtils.DoLog($"[{buildingInfo}]1positionR = {positionR}; direction = {direction}");

                    Vector3 normalized = Vector3.Cross(Vector3.up, direction).normalized;
                    positionR += normalized * (MathUtils.SmootherStep(0.5f, 0f, Mathf.Abs(m_defaultStopOffset - 0.5f)) * lane.m_stopOffset);
                    LogUtils.DoLog($"[{buildingInfo}]2positionR = {positionR}; direction = {direction}; {normalized}");
                    result.Add(new StopPointDescriptorLanes
                    {
                        platformLine = refBezier,
                        width = lane.m_width,
                        vehicleType = refLane.m_stopType,
                        laneId = laneId,
                        subbuildingId = -1,
                        directionPath = directionPath * (path.m_invertSegments == (refLane.m_finalDirection == NetInfo.Direction.AvoidForward || refLane.m_finalDirection == NetInfo.Direction.Backward) ? 1 : -1)

                    });

                }
            }
            for (int i = 0; i < buildingInfo.m_subBuildings.Length; i++)
            {
                StopPointDescriptorLanes[] subPlats = MapStopPoints(buildingInfo.m_subBuildings[i].m_buildingInfo, thresold);
                if (subPlats != null)
                {
                    var rotationToApply = Quaternion.AngleAxis(buildingInfo.m_subBuildings[i].m_angle, Vector3.up);
                    result.AddRange(subPlats.Select(x =>
                    {
                        x.platformLine.a = (rotationToApply * x.platformLine.a) + buildingInfo.m_subBuildings[i].m_position;
                        x.platformLine.b = (rotationToApply * x.platformLine.b) + buildingInfo.m_subBuildings[i].m_position;
                        x.platformLine.c = (rotationToApply * x.platformLine.c) + buildingInfo.m_subBuildings[i].m_position;
                        x.platformLine.d = (rotationToApply * x.platformLine.d) + buildingInfo.m_subBuildings[i].m_position;
                        x.subbuildingId = (sbyte) i;
                        return x;
                    }));
                }
            }
            result.Sort((x, y) =>
            {
                int priorityX = VehicleToPriority(x.vehicleType);
                int priorityY = VehicleToPriority(y.vehicleType);
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
            if (CommonProperties.DebugMode)
            {
                LogUtils.DoLog($"{buildingInfo.name} PLAT ORDER:\n{string.Join("\n", result.Select((x, y) => $"{y}=> {x.ToString()}").ToArray())}");
            }
            return result.ToArray();
        }

        private static NetInfo.Lane FindNearestVehicleStopLane(NetInfo.Lane[] laneGroup, NetInfo.Lane refLane, out ushort laneId)
        {
            NetInfo.Lane nearestLane = null;
            float nearestDist = float.MaxValue;
            laneId = 0xffff;
            for (ushort i = 0; i < laneGroup.Length; i++)
            {
                NetInfo.Lane lane = laneGroup[i];
                if ((lane.m_vehicleType & refLane.m_stopType) != VehicleInfo.VehicleType.None)
                {
                    float dist = Mathf.Abs(lane.m_position - refLane.m_position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestLane = lane;
                        laneId = i;
                    }
                }
            }

            return nearestLane;
        }

        private static void GetMiddlePointsFor(BuildingInfo.PathInfo pathInfo, out Vector3 b, out Vector3 c)
        {
            b = Vector3.zero;
            c = Vector3.zero;
            if (pathInfo.m_finalNetInfo != null && pathInfo.m_nodes != null && pathInfo.m_nodes.Length != 0)
            {
                Vector3 vector = Building.CalculatePosition(new Vector3(), 0, pathInfo.m_nodes[0]);
                if (!pathInfo.m_finalNetInfo.m_useFixedHeight)
                {
                    vector.y = NetSegment.SampleTerrainHeight(pathInfo.m_finalNetInfo, vector, false, pathInfo.m_nodes[0].y);
                }
                var ray = new Ray(vector + new Vector3(0f, 8f, 0f), Vector3.down);
                if (NetTool.MakeControlPoint(ray, 16f, pathInfo.m_finalNetInfo, true, NetNode.Flags.Untouchable, NetSegment.Flags.Untouchable, Building.Flags.All, pathInfo.m_nodes[0].y - pathInfo.m_finalNetInfo.m_buildHeight, true, out NetTool.ControlPoint controlPoint))
                {
                    Vector3 vector2 = controlPoint.m_position - vector;
                    if (!pathInfo.m_finalNetInfo.m_useFixedHeight)
                    {
                        vector2.y = 0f;
                    }
                    float sqrMagnitude = vector2.sqrMagnitude;
                    if (sqrMagnitude > pathInfo.m_maxSnapDistance * pathInfo.m_maxSnapDistance)
                    {
                        controlPoint.m_position = vector;
                        controlPoint.m_elevation = 0f;
                        controlPoint.m_node = 0;
                        controlPoint.m_segment = 0;
                    }
                    else
                    {
                        controlPoint.m_position.y = vector.y;
                    }
                }
                else
                {
                    controlPoint.m_position = vector;
                }
                int j = 1;

                vector = Building.CalculatePosition(Vector3.zero, 0, pathInfo.m_nodes[j]);
                if (!pathInfo.m_finalNetInfo.m_useFixedHeight)
                {
                    vector.y = NetSegment.SampleTerrainHeight(pathInfo.m_finalNetInfo, vector, false, pathInfo.m_nodes[j].y);
                }
                ray = new Ray(vector + new Vector3(0f, 8f, 0f), Vector3.down);
                if (NetTool.MakeControlPoint(ray, 16f, pathInfo.m_finalNetInfo, true, NetNode.Flags.Untouchable, NetSegment.Flags.Untouchable, Building.Flags.All, pathInfo.m_nodes[j].y - pathInfo.m_finalNetInfo.m_buildHeight, true, out NetTool.ControlPoint controlPoint2))
                {
                    Vector3 vector3 = controlPoint2.m_position - vector;
                    if (!pathInfo.m_finalNetInfo.m_useFixedHeight)
                    {
                        vector3.y = 0f;
                    }
                    float sqrMagnitude2 = vector3.sqrMagnitude;
                    if (sqrMagnitude2 > pathInfo.m_maxSnapDistance * pathInfo.m_maxSnapDistance)
                    {
                        controlPoint2.m_position = vector;
                        controlPoint2.m_elevation = 0f;
                        controlPoint2.m_node = 0;
                        controlPoint2.m_segment = 0;
                    }
                    else
                    {
                        controlPoint2.m_position.y = vector.y;
                    }
                }
                else
                {
                    controlPoint2.m_position = vector;
                }
                NetTool.ControlPoint middlePoint = controlPoint2;
                if (pathInfo.m_curveTargets != null && pathInfo.m_curveTargets.Length >= j)
                {
                    middlePoint.m_position = Building.CalculatePosition(Vector3.zero, 0, pathInfo.m_curveTargets[j - 1]);
                    if (!pathInfo.m_finalNetInfo.m_useFixedHeight)
                    {
                        middlePoint.m_position.y = NetSegment.SampleTerrainHeight(pathInfo.m_finalNetInfo, middlePoint.m_position, false, pathInfo.m_curveTargets[j - 1].y);
                    }
                }
                else
                {
                    middlePoint.m_position = (controlPoint.m_position + controlPoint2.m_position) * 0.5f;
                }
                middlePoint.m_direction = VectorUtils.NormalizeXZ(middlePoint.m_position - controlPoint.m_position);
                controlPoint2.m_direction = VectorUtils.NormalizeXZ(controlPoint2.m_position - middlePoint.m_position);
                NetSegment.CalculateMiddlePoints(controlPoint.m_position, middlePoint.m_direction, controlPoint2.m_position, -controlPoint2.m_direction, true, true, out b, out c);
                //controlPoint = controlPoint2;

            }
        }

        public static int VehicleToPriority(VehicleInfo.VehicleType tt)
        {
            switch (tt)
            {
                case VehicleInfo.VehicleType.Car:
                    return 99;
                case VehicleInfo.VehicleType.Metro:
                case VehicleInfo.VehicleType.Train:
                case VehicleInfo.VehicleType.Monorail:
                    return 20;
                case VehicleInfo.VehicleType.Ship:
                    return 10;
                case VehicleInfo.VehicleType.Plane:
                    return 5;
                case VehicleInfo.VehicleType.Tram:
                    return 88;
                case VehicleInfo.VehicleType.Helicopter:
                    return 7;
                case VehicleInfo.VehicleType.Ferry:
                    return 15;

                case VehicleInfo.VehicleType.CableCar:
                    return 30;
                case VehicleInfo.VehicleType.Blimp:
                    return 12;
                case VehicleInfo.VehicleType.Balloon:
                    return 11;
                default:
                    return 9999;
            }
        }
        #endregion
    }
}
