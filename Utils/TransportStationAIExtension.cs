using ColossalFramework;
using ColossalFramework.Math;
using Klyte.TransportLinesManager.Extensions;
using UnityEngine;

namespace Klyte.TransportLinesManager.Utils
{
    public static class TransportStationAIExtension
    {
        public static bool UseSecondaryTransportInfoForConnection(this TransportStationAI tsai) => !(tsai.m_secondaryTransportInfo is null) && tsai.m_secondaryTransportInfo.m_class.m_subService == tsai.m_transportLineInfo?.m_class.m_subService && tsai.m_secondaryTransportInfo.m_class.m_level == tsai.m_transportLineInfo.m_class.m_level;

        public static bool CreateConnectionNode(this TransportSystemDefinition tsd, out ushort node, Vector3 position)
        {
            NetManager instance = Singleton<NetManager>.instance;
            if (instance.CreateNode(out node, ref Singleton<SimulationManager>.instance.m_randomizer, tsd.GetLineInfoIntercity(), position, Singleton<SimulationManager>.instance.m_currentBuildIndex))
            {
                NetNode[] buffer = instance.m_nodes.m_buffer;
                ushort num = node;
                buffer[num].m_flags = (buffer[num].m_flags | NetNode.Flags.Untouchable);
                Singleton<SimulationManager>.instance.m_currentBuildIndex += 1U;
                return true;
            }
            node = 0;
            return false;
        }
        public static Vector3 FindStopPosition(ushort targetID, ref Building target, Building.Flags incomingOutgoing)
        {
            NetManager instance = Singleton<NetManager>.instance;
            ushort num = target.FindParentNode(targetID);
            Vector3 result = FindStopPosition(targetID, ref target);
            for (int i = 0; i < 8; i++)
            {
                ushort segment = instance.m_nodes.m_buffer[num].GetSegment(i);
                if (segment != 0)
                {
                    instance.m_segments.m_buffer[segment].GetLeftAndRightLanes(num, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, -1, false, out _, out _, out uint num4, out uint num5);
                    if (incomingOutgoing == Building.Flags.Outgoing || num5 == 0U)
                    {
                        if (num4 != 0U)
                        {
                            result = instance.m_lanes.m_buffer[num4].CalculatePosition(0.5f);
                        }
                    }
                    else if ((incomingOutgoing == Building.Flags.Incoming || num4 == 0U) && num5 != 0U)
                    {
                        result = instance.m_lanes.m_buffer[num5].CalculatePosition(0.5f);
                    }
                    break;
                }
            }
            return result;
        }
        private static Vector3 FindStopPosition(ushort targetID, ref Building target)
        {
            NetManager instance = Singleton<NetManager>.instance;
            ushort num = target.FindParentNode(targetID);
            Vector3 result = target.m_position;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = instance.m_nodes.m_buffer[num].GetSegment(i);
                if (segment != 0)
                {
                    ushort startNode = instance.m_segments.m_buffer[segment].m_startNode;
                    ushort endNode = instance.m_segments.m_buffer[segment].m_endNode;
                    Vector3 position = instance.m_nodes.m_buffer[startNode].m_position;
                    Vector3 position2 = instance.m_nodes.m_buffer[endNode].m_position;
                    result = Vector3.Lerp(position, position2, 0.5f);
                    break;
                }
            }
            return result;
        }
        public static bool CreateConnectionSegment(this TransportSystemDefinition tsd, out ushort segment, ushort startNode, ushort endNode, int gateIndex)
        {
            NetManager instance = Singleton<NetManager>.instance;
            Vector3 position = instance.m_nodes.m_buffer[startNode].m_position;
            Vector3 position2 = instance.m_nodes.m_buffer[endNode].m_position;
            Vector3 vector = position2 - position;
            vector = VectorUtils.NormalizeXZ(vector);
            if (instance.CreateSegment(out segment, ref Singleton<SimulationManager>.instance.m_randomizer, tsd.GetLineInfoIntercity(), startNode, endNode, vector, -vector, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
            {
                instance.m_segments.m_buffer[segment].m_trafficLightState0 = (byte)gateIndex;
                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2U;
                return true;
            }
            segment = 0;
            return false;
        }
    }
}

