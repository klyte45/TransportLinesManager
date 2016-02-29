using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Extensors
{
    //class TLMTransportLineExtensionHooks : Redirector
    //{
    //    private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();

    //    public static void EnableHooks()
    //    {
    //        if (redirects.Count != 0)
    //        {
    //            DisableHooks();
    //        }
    //        TLMUtils.doLog("Loading TransportLine Hooks!");
    //        AddRedirect(typeof(TransportLine), typeof(TLMTransportLine).GetMethod("SimulationStep", allFlags), ref redirects);
    //        AddRedirect(typeof(TransportLine), typeof(TLMTransportLine).GetMethod("CheckPrevPath", allFlags), ref redirects);

    //        AddRedirect(typeof(TLMTransportLine), typeof(TransportLine).GetMethod("GetLastStop", allFlags), ref redirects);


    //    }

    //    public static void DisableHooks()
    //    {
    //        foreach (var kvp in redirects)
    //        {
    //            RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
    //        }
    //        redirects.Clear();
    //    }
    //}

    //class TLMTransportLine
    //{
    //    private static Array16<int> m_linesCost = new Array16<int>(256);
    //    private static void SetLineCost(ushort lineId, int val)
    //    {
    //        m_linesCost.m_buffer[lineId] = val;
    //    }

    //    public static int GetLineCost(ushort lineId)
    //    {
    //        if (lineId > 0 && lineId < m_linesCost.m_size)
    //        {
    //            return m_linesCost.m_buffer[lineId];
    //        }
    //        return -1;
    //    }
    //    ushort m_stops;
    //    TransportLine.Flags m_flags;
    //    TransportInfo Info
    //    {
    //        get
    //        {
    //            return default(TransportInfo);
    //        }
    //    }

    //    public void SimulationStep(ushort lineID)
    //    {
    //        TLMUtils.doLog("LTLMTransportLine SimulationStep!");
    //        TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
    //        TransportInfo info = tl.Info;
    //        TLMCW.ConfigIndex lineType = TLMCW.getConfigIndexForLine(lineID);

    //        float defaultCostPerPassengerCapacity = TLMCW.getCostPerPassengerCapacityLine(lineType);

    //        if (tl.Complete)
    //        {
    //            int vehicleCount = 0;
    //            int installedCapacity = 0;
    //            if (tl.m_vehicles != 0)
    //            {
    //                VehicleManager instance = Singleton<VehicleManager>.instance;
    //                ushort nextId = tl.m_vehicles;
    //                int loopCount = 0;
    //                while (nextId != 0)
    //                {
    //                    ushort nextLineVehicle = instance.m_vehicles.m_buffer[(int)nextId].m_nextLineVehicle;
    //                    vehicleCount++;
    //                    installedCapacity += TLMLineUtils.getVehicleCapacity(nextId);
    //                    nextId = nextLineVehicle;
    //                    if (++loopCount > 16384)
    //                    {
    //                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
    //                        break;
    //                    }
    //                }
    //            }
    //            bool active;
    //            if (Singleton<SimulationManager>.instance.m_isNightTime)
    //            {
    //                active = ((tl.m_flags & TransportLine.Flags.DisabledNight) == TransportLine.Flags.None);
    //            }
    //            else
    //            {
    //                active = ((tl.m_flags & TransportLine.Flags.DisabledDay) == TransportLine.Flags.None);
    //            }
    //            uint steps = 0u;
    //            float distance = 0f;
    //            bool broken = false;
    //            if (tl.m_stops != 0)
    //            {
    //                NetManager instance2 = Singleton<NetManager>.instance;
    //                ushort stops = tl.m_stops;
    //                ushort nextStop = stops;
    //                int count = 0;
    //                while (nextStop != 0)
    //                {
    //                    ushort num8 = 0;
    //                    if (active)
    //                    {
    //                        NetNode[] expr_10A_cp_0 = instance2.m_nodes.m_buffer;
    //                        ushort expr_10A_cp_1 = nextStop;
    //                        expr_10A_cp_0[(int)expr_10A_cp_1].m_flags = (expr_10A_cp_0[(int)expr_10A_cp_1].m_flags & ~NetNode.Flags.Disabled);
    //                    }
    //                    else
    //                    {
    //                        NetNode[] expr_130_cp_0 = instance2.m_nodes.m_buffer;
    //                        ushort expr_130_cp_1 = nextStop;
    //                        expr_130_cp_0[(int)expr_130_cp_1].m_flags = (expr_130_cp_0[(int)expr_130_cp_1].m_flags | NetNode.Flags.Disabled);
    //                    }
    //                    for (int i = 0; i < 8; i++)
    //                    {
    //                        ushort segment = instance2.m_nodes.m_buffer[(int)nextStop].GetSegment(i);
    //                        if (segment != 0 && instance2.m_segments.m_buffer[(int)segment].m_startNode == nextStop)
    //                        {
    //                            distance += instance2.m_segments.m_buffer[(int)segment].m_averageLength;
    //                            num8 = instance2.m_segments.m_buffer[(int)segment].m_endNode;
    //                            if ((instance2.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.PathLength) == NetSegment.Flags.None)
    //                            {
    //                                broken = true;
    //                            }
    //                            break;
    //                        }
    //                    }
    //                    steps += 1u;
    //                    nextStop = num8;
    //                    if (nextStop == stops)
    //                    {
    //                        break;
    //                    }
    //                    if (++count >= 32768)
    //                    {
    //                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
    //                        break;
    //                    }
    //                }
    //            }
    //            float lineCost = installedCapacity * defaultCostPerPassengerCapacity;
    //            SetLineCost(lineID, (int)lineCost);
    //            if (lineCost != 0)
    //            {
    //                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, m_linesCost.m_buffer[lineID], info.m_class);
    //            }
    //            int budget = Singleton<EconomyManager>.instance.GetBudget(info.m_class);
    //            int necessaryVehicles;
    //            if (active)
    //            {
    //                if (broken)
    //                {
    //                    necessaryVehicles = vehicleCount;
    //                }
    //                else
    //                {
    //                    necessaryVehicles = Mathf.CeilToInt((float)budget * distance / (info.m_defaultVehicleDistance * 100f));
    //                }
    //            }
    //            else
    //            {
    //                necessaryVehicles = 0;
    //            }
    //            if (steps != 0u && vehicleCount < necessaryVehicles)
    //            {
    //                TransferManager.TransferReason vehicleReason = info.m_vehicleReason;
    //                int index = Singleton<SimulationManager>.instance.m_randomizer.Int32(steps);
    //                ushort stop = tl.GetStop(index);
    //                if (vehicleReason != TransferManager.TransferReason.None && stop != 0)
    //                {
    //                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
    //                    offer.Priority = necessaryVehicles - vehicleCount + 1;
    //                    offer.TransportLine = lineID;
    //                    offer.Position = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_position;
    //                    offer.Amount = 1;
    //                    offer.Active = false;
    //                    Singleton<TransferManager>.instance.AddIncomingOffer(vehicleReason, offer);
    //                }
    //            }
    //            else if (vehicleCount > necessaryVehicles)
    //            {
    //                int index2 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)vehicleCount);
    //                ushort vehicle = tl.GetVehicle(index2);
    //                if (vehicle != 0)
    //                {
    //                    VehicleManager instance3 = Singleton<VehicleManager>.instance;
    //                    VehicleInfo info2 = instance3.m_vehicles.m_buffer[(int)vehicle].Info;
    //                    info2.m_vehicleAI.SetTransportLine(vehicle, ref instance3.m_vehicles.m_buffer[(int)vehicle], 0);
    //                }
    //            }
    //        }
    //        if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095u) >= 3840u)
    //        {
    //            tl.m_passengers.Update();
    //            Singleton<TransportManager>.instance.m_passengers[(int)info.m_transportType].Add(ref tl.m_passengers);
    //            tl.m_passengers.Reset();
    //        }
    //    }

    //    public ushort GetLastStop() { return 0; }

    //    public bool CheckPrevPath(int stopIndex, out bool failed)
    //    {
    //        failed = false;
    //        if (this.m_stops != 0)
    //        {
    //            ushort num;
    //            if (stopIndex == -1)
    //            {
    //                if ((this.m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
    //                {
    //                    num = this.GetLastStop();
    //                }
    //                else
    //                {
    //                    num = this.m_stops;
    //                }
    //            }
    //            else
    //            {
    //                num = this.m_stops;
    //                for (int i = 0; i < stopIndex; i++)
    //                {
    //                    num = TransportLine.GetNextStop(num);
    //                    if (num == this.m_stops)
    //                    {
    //                        break;
    //                    }
    //                }
    //            }
    //            if (num == 0)
    //            {
    //                return false;
    //            }
    //            ushort prevSegment = TransportLine.GetPrevSegment(num);
    //            if (prevSegment == 0)
    //            {
    //                return true;
    //            }
    //            if (this.Info.m_transportType != TransportInfo.TransportType.Ship)
    //            {
    //                NetManager instance = Singleton<NetManager>.instance;
    //                if ((this.m_flags & TransportLine.Flags.Temporary) != TransportLine.Flags.None && (instance.m_segments.m_buffer[(int)prevSegment].m_flags & NetSegment.Flags.WaitingPath) != NetSegment.Flags.None)
    //                {
    //                    return false;
    //                }
    //                if ((instance.m_segments.m_buffer[(int)prevSegment].m_flags & NetSegment.Flags.PathFailed) != NetSegment.Flags.None)
    //                {
    //                    failed = true;
    //                    return false;
    //                }
    //                if (instance.m_segments.m_buffer[(int)prevSegment].m_path == 0u)
    //                {
    //                    failed = true;
    //                    return false;
    //                }
    //            }
    //        }
    //        return true;
    //    }
    //}
}
