using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.ModShared;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public class PlatformConfig : IIdentifiable
    {
        [XmlAttribute("passengerLaneId")]
        public long? Id { get; set; }

        [XmlIgnore]
        public uint PlatformLaneId => (uint)Id;
        [XmlAttribute("vehicleLaneId")]
        public uint VehicleLaneId { get; set; }

        [XmlElement("targetOutsideConnectionBuildings")]
        public NonSequentialList<OutsideConnectionLineInfo> TargetOutsideConnections { get; set; } = new NonSequentialList<OutsideConnectionLineInfo>();

        public void ReleaseNodes(ushort sourceBuilding)
        {
            if (SimulationManager.exists)
            {
                foreach (var val in TargetOutsideConnections.ToArray())
                {
                    ReleaseNodes(sourceBuilding, val.Value);
                    TargetOutsideConnections.Remove(val.Key);
                }
            }
        }
        public void ReleaseNodes(ushort sourceBuilding, OutsideConnectionLineInfo outsideConnection)
        {
            if (SimulationManager.exists)
            {
                var bm = BuildingManager.instance;
                var instance = NetManager.instance;
                ref Building data = ref bm.m_buildings.m_buffer[sourceBuilding];
                ushort num = 0;
                ushort num2 = data.m_netNode;
                int num3 = 0;
                while (num2 != 0)
                {
                    ushort nextBuildingNode = instance.m_nodes.m_buffer[num2].m_nextBuildingNode;
                    if (num2 == outsideConnection.m_nodeStation || num2 == outsideConnection.m_nodeOutsideConnection)
                    {
                        if (num != 0)
                        {
                            instance.m_nodes.m_buffer[num].m_nextBuildingNode = nextBuildingNode;
                        }
                        else
                        {
                            data.m_netNode = nextBuildingNode;
                        }
                        ReleaseLines(num2);
                        instance.ReleaseNode(num2);
                        num2 = num;
                    }
                    TLMFacade.Instance.OnRegionalLineParameterChanged(num);
                    num = num2;
                    num2 = nextBuildingNode;
                    if (++num3 > 32768)
                    {
                        LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }
        private void ReleaseLines(ushort node)
        {
            NetManager instance = NetManager.instance;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = instance.m_nodes.m_buffer[node].GetSegment(i);
                if (segment != 0)
                {
                    instance.ReleaseSegment(segment, true);
                }
            }
        }
        public void UpdateStationNodes(ushort stationId)
        {
            var keys = TargetOutsideConnections.Keys.ToArray();
            foreach (var key in keys)
            {
                if (TargetOutsideConnections[key] is null && CreateConnectionLines(stationId, (ushort)key) is OutsideConnectionLineInfo conn)
                {
                    TargetOutsideConnections[key] = conn;
                }
                else
                {
                    TargetOutsideConnections.Remove(key);
                }
            }
        }

        public void AddDestination(ushort stationId, ushort outsideConnectionId, string name, Color clr)
        {
            if (!TargetOutsideConnections.ContainsKey(outsideConnectionId))
            {
                if (CreateConnectionLines(stationId, outsideConnectionId) is OutsideConnectionLineInfo conn)
                {
                    conn.Identifier = name;
                    conn.LineColor = clr;
                    TargetOutsideConnections[outsideConnectionId] = conn;
                    TLMFacade.Instance.OnRegionalLineParameterChanged(conn.m_nodeStation);
                }
            }
        }
        public void RemoveDestination(ushort stationId, ushort outsideConnectionId)
        {
            if (TargetOutsideConnections.ContainsKey(outsideConnectionId))
            {
                ReleaseNodes(stationId, TargetOutsideConnections[outsideConnectionId]);
                TLMFacade.Instance.OnRegionalLineParameterChanged(TargetOutsideConnections[outsideConnectionId].m_nodeStation);
                TargetOutsideConnections.Remove(outsideConnectionId);
            }
        }

        private Vector3 StationPlatformPosition => NetManager.instance.m_lanes.m_buffer[VehicleLaneId].m_bezier.Position(.5f);

        private OutsideConnectionLineInfo CreateConnectionLines(ushort stationId, ushort outsideConnectionId)
        {
            ref Building stationBuilding = ref BuildingManager.instance.m_buildings.m_buffer[stationId];
            ref Building outsideConnectionBuilding = ref BuildingManager.instance.m_buildings.m_buffer[outsideConnectionId];
            var outsideConnectionTSD = TransportSystemDefinition.FromOutsideConnection(outsideConnectionBuilding.Info.GetSubService(), outsideConnectionBuilding.Info.GetClassLevel(), VehicleInfo.VehicleType.None);
            if ((stationBuilding.Info.m_buildingAI is TransportStationAI) && (outsideConnectionBuilding.m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None && outsideConnectionTSD != null)
            {
                var stationPlatformPosition = StationPlatformPosition;
                var result = new OutsideConnectionLineInfo();
                NetManager instance = NetManager.instance;
                if (outsideConnectionTSD.CreateConnectionNode(out result.m_nodeStation, stationPlatformPosition))
                {
                    if ((stationBuilding.m_flags & Building.Flags.Active) == Building.Flags.None)
                    {
                        instance.m_nodes.m_buffer[result.m_nodeStation].m_flags |= NetNode.Flags.Disabled;
                    }
                    instance.m_nodes.m_buffer[result.m_nodeStation].m_flags |= NetNode.Flags.Fixed;
                    instance.UpdateNode(result.m_nodeStation);
                    instance.m_nodes.m_buffer[result.m_nodeStation].m_nextBuildingNode = stationBuilding.m_netNode;
                    stationBuilding.m_netNode = result.m_nodeStation;
                }
                Building.Flags incomingOutgoing = ((outsideConnectionBuilding.m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.Incoming) ? Building.Flags.Incoming : Building.Flags.Outgoing;
                Vector3 outsideConnectionPlatformPosition = TransportStationAIExtension.FindStopPosition(outsideConnectionId, ref outsideConnectionBuilding, incomingOutgoing);
                if (outsideConnectionTSD.CreateConnectionNode(out result.m_nodeOutsideConnection, outsideConnectionPlatformPosition))
                {
                    if ((stationBuilding.m_flags & Building.Flags.Active) == Building.Flags.None)
                    {
                        instance.m_nodes.m_buffer[result.m_nodeOutsideConnection].m_flags |= NetNode.Flags.Disabled;
                    }
                    instance.UpdateNode(result.m_nodeOutsideConnection);
                    instance.m_nodes.m_buffer[result.m_nodeOutsideConnection].m_nextBuildingNode = stationBuilding.m_netNode;
                    stationBuilding.m_netNode = result.m_nodeOutsideConnection;
                }
                if (result.m_nodeStation != 0 && result.m_nodeOutsideConnection != 0)
                {
                    if ((outsideConnectionBuilding.m_flags & Building.Flags.Incoming) != Building.Flags.None)
                    {
                        if (outsideConnectionTSD.CreateConnectionSegment(out result.m_segmentToStation, result.m_nodeStation, result.m_nodeOutsideConnection, 0))
                        {
                            instance.m_segments.m_buffer[result.m_segmentToStation].m_flags |= NetSegment.Flags.Untouchable;
                            instance.UpdateSegment(result.m_segmentToStation);
                        }
                    }
                    if ((outsideConnectionBuilding.m_flags & Building.Flags.Outgoing) != Building.Flags.None)
                    {
                        if (outsideConnectionTSD.CreateConnectionSegment(out result.m_segmentToOutsideConnection, result.m_nodeOutsideConnection, result.m_nodeStation, 0))
                        {
                            instance.m_segments.m_buffer[result.m_segmentToOutsideConnection].m_flags |= NetSegment.Flags.Untouchable;
                            instance.UpdateSegment(result.m_segmentToOutsideConnection);
                        }
                    }
                    return result;
                }
                else
                {
                    instance.ReleaseNode(result.m_nodeStation);
                    instance.ReleaseNode(result.m_nodeOutsideConnection);
                }
            }
            return null;
        }

    }
}
