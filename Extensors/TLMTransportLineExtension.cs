using ColossalFramework;
using ColossalFramework.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Extensors
{
    class TLMTransportLineExtensionHooks : Redirector
    {
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();

        public static void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading TransportLine Hooks!");
            AddRedirect(typeof(TransportLine), typeof(TLMTransportLine).GetMethod("SimulationStep", allFlags), ref redirects);
        }

        public static void DisableHooks()
        {
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            redirects.Clear();
        }
    }

    class TLMVehiclesLineManager
    {
        private static TLMVehiclesLineManager _instance;
        public static TLMVehiclesLineManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMVehiclesLineManager();
                }
                return _instance;
            }
        }

        private const string SEPARATOR = "∂";
        private const string COMMA = "§";
        private Dictionary<ushort, int> cached_list;

        public int this[ushort i]
        {
            get { return getVehicleCountForLine(i); }
            set { setVehicleCountForLine(i, value); }
        }

        public int getVehicleCountForLine(ushort lineId)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pre loadSubcategoryList");
            loadLinesConfig();
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getAssetListForPrefix: pos loadSubcategoryList");
            if (!cached_list.ContainsKey(lineId))
            {
                return 0;
            }
            else
            {
                return cached_list[lineId];
            }
        }

        public void setVehicleCountForLine(ushort lineId, int vehicles)
        {
            loadLinesConfig();
            cached_list[lineId] = vehicles;
            saveVehicles();
        }

        private Dictionary<ushort, int> getValueFromString(string x)
        {
            string[] array = x.Split(COMMA.ToCharArray());
            var saida = new Dictionary<ushort, int>();
            foreach (string s in array)
            {
                var items = s.Split(SEPARATOR.ToCharArray());
                if (items.Length != 2) continue;
                try
                {
                    ushort lineId = ushort.Parse(items[0]);
                    int vehicles = int.Parse(items[1]);
                    saida[lineId] = vehicles;
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            return saida;
        }

        private void loadLinesConfig()
        {
            if (cached_list == null)
            {
                cached_list = getValueFromString(TLMConfigWarehouse.getCurrentConfigString(TLMConfigWarehouse.ConfigIndex.VEHICLE_LINE));
            }
        }

        private void saveVehicles()
        {
            TLMConfigWarehouse loadedConfig;
            loadedConfig = TransportLinesManagerMod.instance.currentLoadedCityConfig;
            var value = string.Join(COMMA, cached_list.Select(x => x.Key.ToString() + SEPARATOR + x.Value.ToString()).ToArray());
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("saveVehicles NEW VALUE: {0}", value);
            loadedConfig.setString(TLMConfigWarehouse.ConfigIndex.VEHICLE_LINE, value);
        }
    }

    class TLMTransportLine
    {
        private static Array16<int> m_linesCost = new Array16<int>(256);
        private static TransportLine.Flags[] m_flagsLastState = new TransportLine.Flags[256];
        private static bool m_initialized = false;

        ushort m_stops;
        TransportLine.Flags m_flags;
        TransportInfo Info
        {
            get
            {
                return default(TransportInfo);
            }
        }

        public void SimulationStep(ushort lineID)
        {
            if (!m_initialized)
            {
                m_initialized = true;
                for (int i = 0; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++)
                {
                    m_flagsLastState[i] = Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_flags;
                }
            }

            var flagsChanged = (m_flagsLastState[lineID] ^ Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags);
            m_flagsLastState[lineID] = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags;

            if ((flagsChanged & TransportLine.Flags.Complete) != TransportLine.Flags.None)
            {
                if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
                {
                    TLMController.instance.AutoColor(lineID);
                }

                if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED))
                {
                    TLMUtils.setLineName(lineID, TLMUtils.calculateAutoName(lineID));
                }
            }

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("LTLMTransportLine SimulationStep!");
            TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].Info;
            TLMCW.ConfigIndex lineType = TLMCW.getConfigIndexForLine(lineID);

            float defaultCostPerPassengerCapacity = TLMCW.getCostPerPassengerCapacityLine(lineType);

            if (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].Complete)
            {
                int vehicleCount = 0;
                int installedCapacity = 0;
                if (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles != 0)
                {
                    VehicleManager instance = Singleton<VehicleManager>.instance;
                    ushort nextId = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
                    int loopCount = 0;
                    while (nextId != 0)
                    {
                        ushort nextLineVehicle = instance.m_vehicles.m_buffer[(int)nextId].m_nextLineVehicle;
                        vehicleCount++;
                        installedCapacity += TLMLineUtils.getVehicleCapacity(nextId);
                        nextId = nextLineVehicle;
                        if (++loopCount > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
                bool active;
                if (Singleton<SimulationManager>.instance.m_isNightTime)
                {
                    active = ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.DisabledNight) == TransportLine.Flags.None);
                }
                else
                {
                    active = ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.DisabledDay) == TransportLine.Flags.None);
                }
                uint steps = 0u;
                float distance = 0f;
                bool broken = false;
                if (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops != 0)
                {
                    NetManager instance2 = Singleton<NetManager>.instance;
                    ushort stops = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops;
                    ushort nextStop = stops;
                    int count = 0;
                    while (nextStop != 0)
                    {
                        ushort num8 = 0;
                        if (active)
                        {
                            NetNode[] expr_10A_cp_0 = instance2.m_nodes.m_buffer;
                            ushort expr_10A_cp_1 = nextStop;
                            expr_10A_cp_0[(int)expr_10A_cp_1].m_flags = (expr_10A_cp_0[(int)expr_10A_cp_1].m_flags & ~NetNode.Flags.Disabled);
                        }
                        else
                        {
                            NetNode[] expr_130_cp_0 = instance2.m_nodes.m_buffer;
                            ushort expr_130_cp_1 = nextStop;
                            expr_130_cp_0[(int)expr_130_cp_1].m_flags = (expr_130_cp_0[(int)expr_130_cp_1].m_flags | NetNode.Flags.Disabled);
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            ushort segment = instance2.m_nodes.m_buffer[(int)nextStop].GetSegment(i);
                            if (segment != 0 && instance2.m_segments.m_buffer[(int)segment].m_startNode == nextStop)
                            {
                                distance += instance2.m_segments.m_buffer[(int)segment].m_averageLength;
                                num8 = instance2.m_segments.m_buffer[(int)segment].m_endNode;
                                if ((instance2.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.PathLength) == NetSegment.Flags.None)
                                {
                                    broken = true;
                                }
                                break;
                            }
                        }
                        steps += 1u;
                        nextStop = num8;
                        if (nextStop == stops)
                        {
                            break;
                        }
                        if (++count >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
                uint prefix = 0;
                if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportType(info.m_transportType) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
                {
                    prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
                }
                float budgetMultiplierPrefix = TLMUtils.getExtensionFromConfigIndex(TLMCW.getConfigIndexForTransportType(info.m_transportType)).getBudgetMultiplierForHour(prefix, (int) Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 100f;
                float lineCost = vehicleCount * info.m_maintenanceCostPerVehicle / 100;// * defaultCostPerPassengerCapacity;
                if (lineCost != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, m_linesCost.m_buffer[lineID], info.m_class);
                }
                int budget = Singleton<EconomyManager>.instance.GetBudget(info.m_class);
                int necessaryVehicles;

                if (!active)
                {
                    necessaryVehicles = 0;
                }
                else
                {
                    necessaryVehicles = TLMVehiclesLineManager.instance[lineID];
                    if (necessaryVehicles == 0)
                    {
                        if (broken)
                        {
                            necessaryVehicles = vehicleCount;
                        }
                        else
                        {
                            necessaryVehicles = Mathf.CeilToInt(budget * budgetMultiplierPrefix * distance / (info.m_defaultVehicleDistance * 100f));
                        }
                    }
                }
                if (steps != 0u && vehicleCount < necessaryVehicles)
                {
                    TransferManager.TransferReason vehicleReason = info.m_vehicleReason;
                    int index = Singleton<SimulationManager>.instance.m_randomizer.Int32(steps);
                    ushort stop = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].GetStop(index);
                    if (vehicleReason != TransferManager.TransferReason.None && stop != 0)
                    {
                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                        offer.Priority = necessaryVehicles - vehicleCount + 1;
                        offer.TransportLine = lineID;
                        offer.Position = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_position;
                        offer.Amount = 1;
                        offer.Active = false;
                        Singleton<TransferManager>.instance.AddIncomingOffer(vehicleReason, offer);
                    }
                }
                else if (vehicleCount > necessaryVehicles)
                {
                    int index2 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)vehicleCount);
                    ushort vehicle = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].GetVehicle(index2);
                    if (vehicle != 0)
                    {
                        VehicleManager instance3 = Singleton<VehicleManager>.instance;
                        VehicleInfo info2 = instance3.m_vehicles.m_buffer[(int)vehicle].Info;
                        info2.m_vehicleAI.SetTransportLine(vehicle, ref instance3.m_vehicles.m_buffer[(int)vehicle], 0);
                    }
                }
            }
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095u) >= 3840u)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.Update();
                Singleton<TransportManager>.instance.m_passengers[(int)info.m_transportType].Add(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers);
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.Reset();
            }
        }
    }
}
