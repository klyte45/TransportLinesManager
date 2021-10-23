using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace Klyte.TransportLinesManager.Utils
{
    public static class TransportStationAIExtension
    {
        public static bool UseSecondaryTransportInfoForConnection(this TransportStationAI tsai) => !(tsai.m_secondaryTransportInfo is null) && tsai.m_secondaryTransportInfo.m_class.m_subService == tsai.m_transportLineInfo.m_class.m_subService && tsai.m_secondaryTransportInfo.m_class.m_level == tsai.m_transportLineInfo.m_class.m_level;
        public static bool IsIntercityBusConnection(this TransportStationAI tsai, BuildingInfo connectionInfo) => connectionInfo.m_class.m_service == ItemClass.Service.Road && tsai.m_transportLineInfo.m_class.m_service == ItemClass.Service.PublicTransport && connectionInfo.m_class.m_subService == ItemClass.SubService.None && tsai.m_transportLineInfo.m_class.m_subService == ItemClass.SubService.PublicTransportBus;
        public static bool IsIntercityBusConnectionTrack(this TransportStationAI tsai, NetInfo trackInfo) => trackInfo.m_class.m_service == ItemClass.Service.Road && tsai.m_transportLineInfo.m_class.m_service == ItemClass.Service.PublicTransport && trackInfo.m_class.m_subService == ItemClass.SubService.None && tsai.m_transportLineInfo.m_class.m_subService == ItemClass.SubService.PublicTransportBus;
        public static bool IsValidOutsideConnection(this TransportStationAI tsai, ushort outsideConnectionBuildingId) => BuildingManager.instance.m_buildings.m_buffer[outsideConnectionBuildingId].Info is BuildingInfo outsideConn
         && (
             (outsideConn.m_class.m_service == tsai.m_transportLineInfo.m_class.m_service && outsideConn.m_class.m_subService == tsai.m_transportLineInfo.m_class.m_subService)
             || tsai.IsIntercityBusConnection(outsideConn));
        public static bool IsValidOutsideConnectionTrack(this TransportStationAI tsai, NetInfo netInfo) =>
            (netInfo.m_class.m_service == tsai.m_transportLineInfo.m_class.m_service && netInfo.m_class.m_subService == tsai.m_transportLineInfo.m_class.m_subService)
              || tsai.IsIntercityBusConnectionTrack(netInfo);

        public static bool CreateConnectionNode(this TransportStationAI tsai, out ushort node, Vector3 position)
        {
            NetManager instance = Singleton<NetManager>.instance;
            if (instance.CreateNode(out node, ref Singleton<SimulationManager>.instance.m_randomizer, tsai.m_transportLineInfo, position, Singleton<SimulationManager>.instance.m_currentBuildIndex))
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
        public static ushort FindNearestConnection(this TransportStationAI tsai, ushort outsideConnection, Building.Flags incomingOutgoing)
        {
            ushort result = 0;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            float num = float.PositiveInfinity;
            Vector3 position = instance.m_buildings.m_buffer[(int)outsideConnection].m_position;
            FastList<ushort> outsideConnections = instance.GetOutsideConnections();
            BuildingInfo info = instance.m_buildings.m_buffer[(int)outsideConnection].Info;
            foreach (ushort num2 in outsideConnections)
            {
                if ((instance.m_buildings.m_buffer[(int)num2].m_flags & incomingOutgoing) == incomingOutgoing)
                {
                    BuildingInfo info2 = instance.m_buildings.m_buffer[(int)num2].Info;
                    if (info != null && info2 != null && info.m_class.m_service == info2.m_class.m_service && num2 != outsideConnection)
                    {
                        float sqrMagnitude = (instance.m_buildings.m_buffer[(int)num2].m_position - position).sqrMagnitude;
                        if (sqrMagnitude < num)
                        {
                            num = sqrMagnitude;
                            result = num2;
                        }
                    }
                }
            }
            return result;
        }
        public static bool CreateConnectionSegment(this TransportStationAI tsai, out ushort segment, ushort startNode, ushort endNode, int gateIndex)
        {
            NetManager instance = Singleton<NetManager>.instance;
            Vector3 position = instance.m_nodes.m_buffer[(int)startNode].m_position;
            Vector3 position2 = instance.m_nodes.m_buffer[(int)endNode].m_position;
            Vector3 vector = position2 - position;
            vector = VectorUtils.NormalizeXZ(vector);
            if (instance.CreateSegment(out segment, ref Singleton<SimulationManager>.instance.m_randomizer, tsai.m_transportLineInfo, startNode, endNode, vector, -vector, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
            {
                instance.m_segments.m_buffer[(int)segment].m_trafficLightState0 = (byte)gateIndex;
                Singleton<SimulationManager>.instance.m_currentBuildIndex += 2U;
                return true;
            }
            segment = 0;
            return false;
        }
    }
}

