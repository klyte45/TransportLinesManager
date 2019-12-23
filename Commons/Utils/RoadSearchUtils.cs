using ColossalFramework;
using System;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class RoadSearchUtils
    {
        #region Road Search Utils
        public static ushort FindNearNamedRoad(Vector3 position)
        {
            return FindNearNamedRoad(position, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                    VehicleInfo.VehicleType.Car, false, false, 256f,
                    out _, out _, out _, out _);
        }
        public static ushort FindNearNamedRoad(Vector3 position, ItemClass.Service service, NetInfo.LaneType laneType, VehicleInfo.VehicleType vehicleType,
            bool allowUnderground, bool requireConnect, float maxDistance,
            out PathUnit.Position pathPosA, out PathUnit.Position pathPosB, out float distanceSqrA, out float distanceSqrB)
        {
            return FindNearNamedRoad(position, service, service, laneType,
                vehicleType, VehicleInfo.VehicleType.None, allowUnderground,
                requireConnect, maxDistance,
                out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB);
        }
        public static ushort FindNearNamedRoad(Vector3 position, ItemClass.Service service, ItemClass.Service service2, NetInfo.LaneType laneType,
            VehicleInfo.VehicleType vehicleType, VehicleInfo.VehicleType stopType, bool allowUnderground,
            bool requireConnect, float maxDistance,
            out PathUnit.Position pathPosA, out PathUnit.Position pathPosB, out float distanceSqrA, out float distanceSqrB)
        {


            Bounds bounds = new Bounds(position, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int num = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            pathPosA.m_segment = 0;
            pathPosA.m_lane = 0;
            pathPosA.m_offset = 0;
            distanceSqrA = 1E+10f;
            pathPosB.m_segment = 0;
            pathPosB.m_lane = 0;
            pathPosB.m_offset = 0;
            distanceSqrB = 1E+10f;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = instance.m_segmentGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        NetInfo info = instance.m_segments.m_buffer[(int)num6].Info;
                        if (info != null && (info.m_class.m_service == service || info.m_class.m_service == service2) && (instance.m_segments.m_buffer[(int)num6].m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) == NetSegment.Flags.None && (allowUnderground || !info.m_netAI.IsUnderground()))
                        {
                            ushort startNode = instance.m_segments.m_buffer[(int)num6].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[(int)num6].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[(int)startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[(int)endNode].m_position;
                            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num9 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            if ((num8 < 0f || num9 < 0f) && instance.m_segments.m_buffer[(int)num6].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[(int)num6].GetClosestLanePosition(position, laneType, vehicleType, stopType, requireConnect, out Vector3 b, out int num10, out float num11, out Vector3 b2, out int num12, out float num13))
                            {
                                float num14 = Vector3.SqrMagnitude(position - b);
                                if (num14 < num5)
                                {
                                    num5 = num14;
                                    pathPosA.m_segment = num6;
                                    pathPosA.m_lane = (byte)num10;
                                    pathPosA.m_offset = (byte)Mathf.Clamp(Mathf.RoundToInt(num11 * 255f), 0, 255);
                                    distanceSqrA = num14;
                                    num14 = Vector3.SqrMagnitude(position - b2);
                                    if (num12 == -1 || num14 >= maxDistance * maxDistance)
                                    {
                                        pathPosB.m_segment = 0;
                                        pathPosB.m_lane = 0;
                                        pathPosB.m_offset = 0;
                                        distanceSqrB = 1E+10f;
                                    }
                                    else
                                    {
                                        pathPosB.m_segment = num6;
                                        pathPosB.m_lane = (byte)num12;
                                        pathPosB.m_offset = (byte)Mathf.Clamp(Mathf.RoundToInt(num13 * 255f), 0, 255);
                                        distanceSqrB = num14;
                                    }
                                }
                            }
                        }
                        num6 = instance.m_segments.m_buffer[(int)num6].m_nextGridSegment;
                        if (++num7 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return pathPosA.m_segment;
        }
        #endregion
    }
}
