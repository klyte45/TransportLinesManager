using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class SegmentUtils
    {
        #region Segment Utils
        public static void UpdateSegmentNamesView()
        {
            if (Singleton<NetManager>.exists)
            {
                NetManager nm = Singleton<NetManager>.instance;

                var segments = new HashSet<ushort>();

                for (ushort i = 1; i < nm.m_segments.m_size; i++)
                {
                    if (nm.m_segments.m_buffer[i].m_flags != 0)
                    {
                        nm.UpdateSegmentRenderer(i, false);
                        segments.Add(i);
                    }
                }
                nm.m_updateNameVisibility = segments;
            }
        }

        private static List<ushort> CalculatePathNet(ushort segmentID, bool startSegment, bool requireSameDirection, bool requireSameSizeAndType)
        {
            var result = new List<ushort>();
            ushort edgeNode;
            if (startSegment)
            {
                edgeNode = NetManager.instance.m_segments.m_buffer[segmentID].m_startNode;
            }
            else
            {
                edgeNode = NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;
            }
            CalculatePathNet(segmentID, segmentID, edgeNode, startSegment, ref result, requireSameDirection, requireSameSizeAndType);
            return result;
        }


        private static void CalculatePathNet(ushort firstSegmentID, ushort segmentID, ushort nodeCurr, bool insertOnEnd, ref List<ushort> segmentOrder, bool requireSameDirection, bool requireSameSizeAndType)
        {
            var strict = requireSameDirection || requireSameSizeAndType;
            var possibilities = new List<ushort>();
            var crossingDirectionsCounter = 0;
            for (var i = 0; i < 8; i++)
            {
                var segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && firstSegmentID != segment && segment != segmentID && !segmentOrder.Contains(segment) && IsSameName(segmentID, segment, false))
                {
                    crossingDirectionsCounter++;
                    if (strict && !IsSameName(segmentID, segment, requireSameDirection, requireSameSizeAndType))
                    {
                        continue;
                    }
                    possibilities.Add(segment);
                }
            }
            if (strict && crossingDirectionsCounter > 1)
            {
                return;
            }
            if (possibilities.Count > 0)
            {
                var segment = possibilities.Min();
                var otherEdgeNode = NetManager.instance.m_segments.m_buffer[segment].GetOtherNode(nodeCurr);
                if (insertOnEnd || segmentOrder.Count == 0)
                {
                    segmentOrder.Add(segment);
                }
                else
                {
                    segmentOrder.Insert(0, segment);
                }
                CalculatePathNet(firstSegmentID, segment, otherEdgeNode, insertOnEnd, ref segmentOrder, requireSameDirection, requireSameSizeAndType);
            }
        }

        public static List<ushort> GetCrossingPath(ushort relSegId)
        {
            var nodeCurr = NetManager.instance.m_segments.m_buffer[relSegId].m_startNode;
            var possibilities = new List<ushort>();
            for (var i = 0; i < 8; i++)
            {
                var segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && relSegId != segment && !IsSameName(relSegId, segment, false))
                {
                    foreach (var prevSeg in possibilities)
                    {
                        if (IsSameName(prevSeg, segment, false))
                        {
                            goto LoopEnd;
                        }
                    }
                    possibilities.Add(segment);
                }
                LoopEnd:
                continue;
            }
            nodeCurr = NetManager.instance.m_segments.m_buffer[relSegId].GetOtherNode(nodeCurr);
            for (var i = 0; i < 8; i++)
            {
                var segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && relSegId != segment && !IsSameName(relSegId, segment, false))
                {
                    foreach (var prevSeg in possibilities)
                    {
                        if (IsSameName(prevSeg, segment, false))
                        {
                            goto LoopEnd2;
                        }
                    }
                    possibilities.Add(segment);
                }
                LoopEnd2:
                continue;
            }
            return possibilities;
        }

        public static List<ushort> GetSegmentOrderRoad(ushort segmentID, bool requireSameDirection, bool requireSameSizeAndType, bool localAdjust, out bool startRef, out bool endRef)
        {
            NetSegment.Flags flags = NetManager.instance.m_segments.m_buffer[segmentID].m_flags;
            if (segmentID != 0 && flags != NetSegment.Flags.None)
            {
                List<ushort> path = CalculatePathNet(segmentID, false, requireSameDirection, requireSameSizeAndType);
                path.Add(segmentID);

                var startNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_startNode;
                var endNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_endNode;
                var startNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_startNode;
                var endNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;

                var circular = (path.Count > 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef || endNode0 == endNodeRef || endNode0 == startNodeRef)) ||
                    (path.Count == 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef) && (endNode0 == endNodeRef || endNode0 == startNodeRef));

                if (circular)
                {
                    LogUtils.DoLog("Circular!");
                    var refer = path.Min();
                    var referIdx = path.IndexOf(refer);
                    if (referIdx != 0)
                    {
                        path = path.GetRange(referIdx, path.Count - referIdx).Union(path.Take(referIdx)).ToList();
                    }
                }
                else
                {
                    path.AddRange(CalculatePathNet(segmentID, true, requireSameDirection, requireSameSizeAndType));
                }
                //doLog($"[s={strict}]path = [{string.Join(",", path.Select(x => x.ToString()).ToArray())}]");
                GetEdgeNodes(ref path, out startRef, out endRef, localAdjust);
                return path;
            }
            startRef = false;
            endRef = false;
            return null;
        }

        public static void GetClosestPositionAndDirectionAndPoint(NetSegment s, Vector3 point, out Vector3 pos, out Vector3 dir, out float length)
        {
            Bezier3 curve = default;
            curve.a = Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_startNode].m_position;
            curve.d = Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_endNode].m_position;
            var smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            var smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            NetSegment.CalculateMiddlePoints(curve.a, s.m_startDirection, curve.d, s.m_endDirection, smoothStart, smoothEnd, out curve.b, out curve.c);
            var closestDistance = 1E+11f;
            var pointPerc = 0f;
            Vector3 targetA = curve.a;
            for (var i = 1; i <= 16; i++)
            {
                Vector3 vector = curve.Position(i / 16f);
                var dist = Segment3.DistanceSqr(targetA, vector, point, out var distSign);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    pointPerc = (i - 1f + distSign) / 16f;
                }
                targetA = vector;
            }
            var precision = 0.03125f;
            for (var j = 0; j < 4; j++)
            {
                Vector3 a2 = curve.Position(Mathf.Max(0f, pointPerc - precision));
                Vector3 vector2 = curve.Position(pointPerc);
                Vector3 b = curve.Position(Mathf.Min(1f, pointPerc + precision));
                var num6 = Segment3.DistanceSqr(a2, vector2, point, out var num7);
                var num8 = Segment3.DistanceSqr(vector2, b, point, out var num9);
                if (num6 < num8)
                {
                    pointPerc = Mathf.Max(0f, pointPerc - (precision * (1f - num7)));
                }
                else
                {
                    pointPerc = Mathf.Min(1f, pointPerc + (precision * num9));
                }
                precision *= 0.5f;
            }
            pos = curve.Position(pointPerc);
            dir = VectorUtils.NormalizeXZ(curve.Tangent(pointPerc));
            length = pointPerc;
        }
        public static bool IsSameName(ushort segment1, ushort segment2, bool strict = false) => IsSameName(segment1, segment2, strict, strict);
        public static bool IsSameName(ushort segment1, ushort segment2, bool requireSameDirection, bool requireSameSizeAndHighway) => IsSameName(segment1, segment2, requireSameDirection, requireSameSizeAndHighway, requireSameSizeAndHighway);
        public static bool IsSameName(ushort segment1, ushort segment2, bool requireSameDirection, bool requireSameSize, bool requireSameHigwayFlag, bool requireSameDistrict = false, bool requireSameAI = false)
        {
            NetManager nm = NetManager.instance;
            NetInfo info = nm.m_segments.m_buffer[segment1].Info;
            NetInfo info2 = nm.m_segments.m_buffer[segment2].Info;
            if (info.m_class.m_service != info2.m_class.m_service)
            {
                return false;
            }
            if (info.m_class.m_subService != info2.m_class.m_subService)
            {
                return false;
            }
            if (requireSameDirection)
            {
                NetSegment seg1 = nm.m_segments.m_buffer[segment1];
                NetSegment seg2 = nm.m_segments.m_buffer[segment2];
                if ((seg1.m_endNode == seg2.m_endNode || seg1.m_startNode == seg2.m_startNode) && (nm.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) == (nm.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.Invert))
                {
                    return false;
                }
                if ((seg1.m_endNode == seg2.m_startNode || seg1.m_startNode == seg2.m_endNode) && (nm.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) != (nm.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.Invert))
                {
                    return false;
                }
            }

            if (requireSameSize)
            {
                if (!((info.m_forwardVehicleLaneCount == info2.m_backwardVehicleLaneCount && info2.m_forwardVehicleLaneCount == info.m_backwardVehicleLaneCount)
                    || (info2.m_forwardVehicleLaneCount == info.m_forwardVehicleLaneCount && info2.m_backwardVehicleLaneCount == info.m_backwardVehicleLaneCount)))
                {
                    return false;
                }
            }
            if (requireSameAI)
            {
                if (info.GetAI().GetType() != info2.GetAI().GetType())
                {
                    return false;
                }
            }
            else if (requireSameHigwayFlag)
            {
                if ((info.GetAI() as RoadBaseAI)?.m_highwayRules != (info2.GetAI() as RoadBaseAI)?.m_highwayRules)
                {
                    return false;
                }
            }
            if (requireSameDistrict)
            {
                Vector3 pos1 = nm.m_segments.m_buffer[segment1].m_middlePosition;
                Vector3 pos2 = nm.m_segments.m_buffer[segment2].m_middlePosition;
                if (DistrictManager.instance.GetDistrict(pos1) != DistrictManager.instance.GetDistrict(pos2))
                {
                    return false;
                }
            }
            var customName1 = (nm.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            var customName2 = (nm.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            if (customName1 != customName2)
            {
                return false;
            }
            if (customName1)
            {
                InstanceID id = default;
                id.NetSegment = segment1;
                var name = Singleton<InstanceManager>.instance.GetName(id);
                id.NetSegment = segment2;
                var name2 = Singleton<InstanceManager>.instance.GetName(id);
                return name == name2;
            }
            var nameSeed = nm.m_segments.m_buffer[segment1].m_nameSeed;
            var nameSeed2 = nm.m_segments.m_buffer[segment2].m_nameSeed;
            return nameSeed == nameSeed2;
        }



        private static List<ushort> GetEdgeNodes(ref List<ushort> accessSegments, out bool nodeStartS, out bool nodeStartE, bool localAdjust)
        {
            if (accessSegments.Count > 1)
            {
                NetSegment seg0 = NetManager.instance.m_segments.m_buffer[accessSegments[0]];
                NetSegment seg1 = NetManager.instance.m_segments.m_buffer[accessSegments[1]];
                NetSegment segN2 = NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 2]];
                NetSegment segN1 = NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 1]];
                nodeStartS = seg1.m_startNode == seg0.m_endNode || seg1.m_endNode == seg0.m_endNode;
                nodeStartE = segN2.m_startNode == segN1.m_endNode || segN2.m_endNode == segN1.m_endNode;
                if (localAdjust)
                {
                    if (nodeStartS == ((NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0))
                    {
                        accessSegments.Reverse();
                        var temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
                else
                {
                    var nodeF = nodeStartS ? seg0.m_startNode : seg0.m_endNode;
                    var nodeL = nodeStartE ? segN1.m_startNode : segN1.m_endNode;
                    if (VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeF].m_position).GetAngleToPoint(VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeL].m_position)) > 180)
                    {
                        accessSegments.Reverse();
                        var temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
            }
            else
            {
                nodeStartS = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) == 0;
                nodeStartE = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0;
            }

            return accessSegments;
        }

        public static IEnumerable<Tuple<ushort, float>> GetSegmentRoadEdges(ushort segmentId, bool requireSameDirection, bool requireSameSizeAndType, bool localAdjust, out ComparableRoad startRef, out ComparableRoad endRef)
        {
            List<ushort> accessSegments = GetSegmentOrderRoad(segmentId, requireSameDirection, requireSameSizeAndType, localAdjust, out var nodeStartS, out var nodeStartE);
            if (accessSegments == null)
            {
                startRef = default;
                endRef = default;
                return null;
            }
            //doLog($"[{segmentId}-> sd {requireSameDirection} rsst {requireSameSizeAndType} la {localAdjust}] segs = [{string.Join(",", accessSegments.Select(x => x.ToString()).ToArray())}]; start={nodeStartS}; end={nodeStartE}");
            startRef = new ComparableRoad(accessSegments[0], nodeStartS);
            endRef = new ComparableRoad(accessSegments[accessSegments.Count - 1], nodeStartE);
            return accessSegments.Select(x => Tuple.New(x, NetManager.instance.m_segments.m_buffer[x].m_averageLength));
        }

        public struct ComparableRoad
        {
            public ComparableRoad(ushort segmentId, bool startNode)
            {
                NetSegment segment = NetManager.instance.m_segments.m_buffer[segmentId];
                if (startNode)
                {
                    nodeReference = segment.m_startNode;
                }
                else
                {
                    nodeReference = segment.m_endNode;
                }

                var entering = startNode != ((NetManager.instance.m_segments.m_buffer[segmentId].m_flags & NetSegment.Flags.Invert) != 0);

                NetNode node = NetManager.instance.m_nodes.m_buffer[nodeReference];
                isPassing = false;
                segmentReference = 0;
                for (var i = 0; i < 7; i++)
                {
                    var segment1 = node.GetSegment(i);
                    if (segment1 > 0 && segment1 != segmentId)
                    {
                        for (var j = i + 1; j < 8; j++)
                        {
                            var segment2 = node.GetSegment(j);
                            if (segment2 > 0 && segment2 != segmentId)
                            {
                                isPassing = IsSameName(segment1, segment2, true);
                                if (isPassing)
                                {
                                    segmentReference = segment1;
                                    break;
                                }
                            }
                        }
                        if (isPassing)
                        {
                            break;
                        }
                    }
                }
                if (!isPassing)
                {

                    for (var i = 0; i < 7; i++)
                    {
                        var segment1 = node.GetSegment(i);
                        if (segment1 > 0 && segment1 != segmentId)
                        {
                            NetSegment segment1Obj = NetManager.instance.m_segments.m_buffer[segment1];
                            var isSegment1StartNode = segment1Obj.m_startNode == nodeReference;
                            var isSegment1Entering = isSegment1StartNode != ((NetManager.instance.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) != 0);

                            if (!(segment1Obj.Info.GetAI() is RoadBaseAI seg1Ai))
                            {
                                continue;
                            }

                            var isSegment1TwoWay = segment1Obj.Info.m_hasBackwardVehicleLanes && segment1Obj.Info.m_hasForwardVehicleLanes;

                            if (!isSegment1TwoWay && isSegment1Entering == entering)
                            {
                                LogUtils.DoLog($"IGNORED: {segment1} (Tw=>{isSegment1TwoWay},entering=>{entering},s1entering=>{isSegment1Entering})");
                                continue;
                            }

                            if (segmentReference == 0)
                            {
                                segmentReference = segment1;
                            }
                            else
                            {
                                NetSegment segmentRefObj = NetManager.instance.m_segments.m_buffer[segmentReference];
                                if (!(segmentRefObj.Info.GetAI() is RoadBaseAI roadAi))
                                {
                                    continue;
                                }
                                if (!roadAi.m_highwayRules && seg1Ai.m_highwayRules)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                                if (roadAi.m_highwayRules && !seg1Ai.m_highwayRules)
                                {
                                    continue;
                                }
                                var laneCount1 = (segment1Obj.Info.m_forwardVehicleLaneCount + segment1Obj.Info.m_backwardVehicleLaneCount);
                                var laneCountRef = (segmentRefObj.Info.m_forwardVehicleLaneCount + segmentRefObj.Info.m_backwardVehicleLaneCount);
                                if (laneCount1 > laneCountRef)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                                if (laneCount1 < laneCountRef)
                                {
                                    continue;
                                }
                                if (segment1Obj.Info.m_halfWidth > segmentRefObj.Info.m_halfWidth)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (segmentReference > 0)
                {
                    NetSegment segmentRefObj = NetManager.instance.m_segments.m_buffer[segmentReference];
                    NetInfo infoRef = segmentRefObj.Info;
                    isHighway = infoRef.GetAI() is RoadBaseAI aiRef && aiRef.m_highwayRules;
                    width = infoRef.m_halfWidth * 2;
                    lanes = infoRef.m_backwardVehicleLaneCount + infoRef.m_forwardVehicleLaneCount;
                }
                else
                {
                    isHighway = false;
                    width = 0;
                    lanes = 0;
                }
            }

            public readonly ushort nodeReference;
            public readonly ushort segmentReference;
            public readonly bool isHighway;
            public readonly bool isPassing;
            public readonly float width;
            public readonly int lanes;

            public int CompareTo(ComparableRoad otherRoad)
            {
                var result = 0;
                if (otherRoad.isHighway != isHighway)
                {
                    result = isHighway ? 1 : -1;
                }
                if (otherRoad.isPassing != isPassing)
                {
                    result = isPassing ? 1 : -1;
                }
                if (otherRoad.width != width)
                {
                    result = otherRoad.width < width ? 1 : -1;
                }
                if (otherRoad.lanes != lanes)
                {
                    result = otherRoad.lanes < lanes ? 1 : -1;
                }
                LogUtils.DoLog($"cmp: {this} & {otherRoad} => {result}");
                return result;
            }

            public override string ToString() => $"[n{nodeReference} s{segmentReference} h:{isHighway} p:{isPassing} w{width} l{lanes}]";
        }

        public static byte GetCardinalDirection(ComparableRoad startRef, ComparableRoad endRef)
        {
            NetNode nodeS = NetManager.instance.m_nodes.m_buffer[startRef.nodeReference];
            NetNode nodeE = NetManager.instance.m_nodes.m_buffer[endRef.nodeReference];

            var cardinalDirection = CardinalPoint.GetCardinalPoint(VectorUtils.XZ(nodeS.m_position).GetAngleToPoint(VectorUtils.XZ(nodeE.m_position))).GetCardinalIndex8();
            return cardinalDirection;
        }


        #endregion
        #region Streets Addressing
        private static readonly ushort[] m_closestSegsFind = new ushort[16];
        public static bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName)
        {
            GetNearestSegment(sidewalk, out Vector3 targetPosition, out var targetLength, out var targetSegmentId);
            if (targetSegmentId == 0)
            {
                number = 0;
                streetName = string.Empty;
                return false;
            }

            number = GetNumberAt(targetLength, targetSegmentId, out var startAsEnd);
            streetName = NetManager.instance.GetSegmentName(targetSegmentId)?.ToString();
            var angleTg = VectorUtils.XZ(targetPosition).GetAngleToPoint(VectorUtils.XZ(midPosBuilding));
            if (angleTg == 90 || angleTg == 270)
            {
                angleTg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }

            NetSegment targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            Vector3 startSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_startNode].m_position;
            Vector3 endSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_endNode].m_position;

            var angleSeg = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(endSeg - startSeg));
            if (angleSeg == 180)
            {
                angleSeg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }


            //doLog($"angleTg = {angleTg};angleSeg = {angleSeg}");
            if ((angleTg + 90) % 360 < 180 ^ angleSeg < 180 ^ startAsEnd)
            {
                number &= ~1;
            }
            else
            {
                number |= 1;
            }
            return true;
        }
        public static void GetNearestSegment(Vector3 sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId)
        {
            NetManager.instance.GetClosestSegments(sidewalk, m_closestSegsFind, out var found);
            targetPosition = default;
            targetLength = 0;
            targetSegmentId = 0;
            if (found == 0)
            {
                return;
            }
            else if (found > 1)
            {
                var minSqrDist = float.MaxValue;
                for (var i = 0; i < found; i++)
                {
                    var segId = m_closestSegsFind[i];
                    NetSegment seg = NetManager.instance.m_segments.m_buffer[segId];
                    if (!(seg.Info.GetAI() is RoadBaseAI))
                    {
                        continue;
                    }

                    GetClosestPositionAndDirectionAndPoint(seg, sidewalk, out Vector3 position, out _, out var length);
                    var sqrDist = Vector3.SqrMagnitude(sidewalk - position);
                    if (i == 0 || sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        targetPosition = position;
                        targetLength = length;
                        targetSegmentId = segId;
                    }
                }
            }
            else
            {
                targetSegmentId = m_closestSegsFind[0];
                GetClosestPositionAndDirectionAndPoint(NetManager.instance.m_segments.m_buffer[targetSegmentId], sidewalk, out targetPosition, out _, out targetLength);
            }
        }


        public static int GetNumberAt(ushort targetSegmentId, bool startNode)
        {
            List<ushort> roadSegments = GetSegmentOrderRoad(targetSegmentId, false, false, false, out _, out _);
            var targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
            NetSegment targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            var targetNode = startNode ? targSeg.m_startNode : targSeg.m_endNode;
            NetSegment preSeg = default;
            if (targetSegmentIdIdx > 0)
            {
                preSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
            }
            if (preSeg.m_endNode != targetNode && preSeg.m_startNode != targetNode)
            {
                targetSegmentIdIdx++;
            }
            float distanceFromStart = 0;
            for (var i = 0; i < targetSegmentIdIdx; i++)
            {
                distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
            }
            return (int) Math.Round(distanceFromStart);
        }

        private static int GetNumberAt(float targetLength, ushort targetSegmentId, out bool startAsEnd)
        {
            //doLog($"targets = S:{targetSegmentId} P:{targetPosition} D:{targetDirection.magnitude} L:{targetLength}");

            List<ushort> roadSegments = GetSegmentOrderRoad(targetSegmentId, false, false, false, out _, out _);
            //doLog("roadSegments = [{0}] ", string.Join(",", roadSegments.Select(x => x.ToString()).ToArray()));
            var targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
            //doLog($"targSeg = {targetSegmentIdIdx} ({targetSegmentId})");
            NetSegment targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            NetSegment preSeg = default;
            NetSegment posSeg = default;
            //doLog("PreSeg");
            if (targetSegmentIdIdx > 0)
            {
                preSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
            }
            //doLog("PosSeg");
            if (targetSegmentIdIdx < roadSegments.Count - 1)
            {
                posSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx + 1]];
            }
            //doLog("startAsEnd");
            startAsEnd = (targetSegmentIdIdx > 0 && (preSeg.m_endNode == targSeg.m_endNode || preSeg.m_startNode == targSeg.m_endNode)) || (targetSegmentIdIdx < roadSegments.Count && (posSeg.m_endNode == targSeg.m_startNode || posSeg.m_startNode == targSeg.m_startNode));

            //doLog($"startAsEnd = {startAsEnd}");

            if (startAsEnd)
            {
                targetLength = 1 - targetLength;
            }
            //doLog("streetName"); 

            //doLog("distanceFromStart");
            float distanceFromStart = 0;
            for (var i = 0; i < targetSegmentIdIdx; i++)
            {
                distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
            }
            distanceFromStart += targetLength * targSeg.m_averageLength;
            return (int) Math.Round(distanceFromStart);

            //doLog($"number = {number} B");
        }
        #endregion
    }
}
