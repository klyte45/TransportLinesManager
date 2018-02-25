using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.NetNodeExt;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Utils.KlyteUtils;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMLineUtils
    {
        public static Vehicle GetVehicleCapacityAndFill(ushort vehicleID, Vehicle vehicleData, out int fill, out int cap)
        {
            ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)vehicleID].GetFirstVehicle(vehicleID);
            vehicleData.Info.m_vehicleAI.GetBufferStatus(firstVehicle, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)firstVehicle], out string text, out fill, out cap);
            return vehicleData;
        }

        public static void GetQuantityPassengerWaiting(ushort currentStop, out int residents, out int tourists, out int timeTilBored)
        {
            ushort nextStop = TransportLine.GetNextStop(currentStop);
            CitizenManager cm = Singleton<CitizenManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            Vector3 position = nm.m_nodes.m_buffer[(int)currentStop].m_position;
            Vector3 position2 = nm.m_nodes.m_buffer[(int)nextStop].m_position;
            nm.m_nodes.m_buffer[(int)currentStop].m_maxWaitTime = 0;
            int minX = Mathf.Max((int)((position.x - 32f) / 8f + 1080f), 0);
            int minZ = Mathf.Max((int)((position.z - 32f) / 8f + 1080f), 0);
            int maxX = Mathf.Min((int)((position.x + 32f) / 8f + 1080f), 2159);
            int maxZ = Mathf.Min((int)((position.z + 32f) / 8f + 1080f), 2159);
            residents = 0;
            tourists = 0;
            timeTilBored = 255;
            int zIterator = minZ;
            while (zIterator <= maxZ)
            {
                int xIterator = minX;
                while (xIterator <= maxX)
                {
                    ushort citizenIterator = cm.m_citizenGrid[zIterator * 2160 + xIterator];
                    int loopCounter = 0;
                    while (citizenIterator != 0)
                    {
                        ushort nextGridInstance = cm.m_instances.m_buffer[(int)citizenIterator].m_nextGridInstance;
                        if ((cm.m_instances.m_buffer[(int)citizenIterator].m_flags & CitizenInstance.Flags.WaitingTransport) != CitizenInstance.Flags.None)
                        {
                            Vector3 a = cm.m_instances.m_buffer[(int)citizenIterator].m_targetPos;
                            float distance = Vector3.SqrMagnitude(a - position);
                            if (distance < 1024f)
                            {
                                CitizenInfo info = cm.m_instances.m_buffer[(int)citizenIterator].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(citizenIterator, ref cm.m_instances.m_buffer[(int)citizenIterator], position, position2))
                                {
                                    if ((cm.m_citizens.m_buffer[(int)(citizenIterator)].m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None)
                                    {
                                        tourists++;
                                    }
                                    else
                                    {
                                        residents++;
                                    }
                                    timeTilBored = Math.Min(255 - cm.m_instances.m_buffer[citizenIterator].m_waitCounter, timeTilBored);
                                }
                            }
                        }
                        citizenIterator = nextGridInstance;
                        if (++loopCounter > 65536)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                    xIterator++;
                }
                zIterator++;
            }
        }

        public static void RemoveAllFromLine(ushort lineId)
        {
            var line = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineId];
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num2 = line.m_vehicles;
            int num3 = 0;
            while (num2 != 0)
            {
                ushort nextLineVehicle = instance.m_vehicles.m_buffer[(int)num2].m_nextLineVehicle;
                VehicleInfo info2 = instance.m_vehicles.m_buffer[(int)num2].Info;
                info2.m_vehicleAI.SetTransportLine(num2, ref instance.m_vehicles.m_buffer[(int)num2], 0);
                num2 = nextLineVehicle;
                if (++num3 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public static float getEffectiveBugdet(ushort transportLine)
        {
            TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].Info;
            int budgetClass = Singleton<EconomyManager>.instance.GetBudget(info.m_class);
            return budgetClass * getBudgetMultiplierLine(transportLine) / 100f;
        }

        public static float getBudgetMultiplierLine(ushort lineId)
        {
            TransportLine __instance = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            TransportInfo info = __instance.Info;
            int budgetClass = Singleton<EconomyManager>.instance.GetBudget(info.m_class);
            if (TLMTransportLineExtension.instance.GetUseCustomConfig(lineId))
            {
                return TLMTransportLineExtension.instance.GetBudgetMultiplierForHour(lineId, (int)Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 100f;
            }
            else
            {
                var tsd = TLMCW.getDefinitionForLine(ref __instance);
                uint prefix = TLMLineUtils.getPrefix(lineId);
                return TLMLineUtils.getExtensionFromConfigIndex(TLMCW.getConfigIndexForTransportInfo(info)).GetBudgetMultiplierForHour(prefix, (int)Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 100f;
            }
        }
        public static bool isPerHourBudget(ushort lineId)
        {
            TransportLine __instance = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            TransportInfo info = __instance.Info;
            int budgetClass = Singleton<EconomyManager>.instance.GetBudget(info.m_class);
            if (TLMTransportLineExtension.instance.GetUseCustomConfig(lineId))
            {
                return TLMTransportLineExtension.instance.GetBudgetsMultiplier(lineId).Length ==8;
            }
            else
            {
                var tsd = TLMCW.getDefinitionForLine(ref __instance);
                uint prefix = TLMLineUtils.getPrefix(lineId);
                return TLMLineUtils.getExtensionFromConfigIndex(TLMCW.getConfigIndexForTransportInfo(info)).GetBudgetsMultiplier(lineId).Length == 8;
            }
        }

        public static string getLineStringId(ushort lineIdx)
        {
            getLineNamingParameters(lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix);
            return TLMUtils.getString(prefix, s, suffix, nonPrefix, Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_lineNumber, zeros, invertPrefixSuffix);
        }

        public static void getLineNamingParameters(ushort lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix)
        {
            getLineNamingParameters(lineIdx, out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix, out string nil);

        }
        public static int GetStopsCount(ushort lineID)
        {
            return Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].CountStops(lineID);
        }

        public static int GetVehiclesCount(ushort lineID)
        {
            return Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].CountVehicles(lineID);
        }

        public static float GetLineLength(ushort lineID)
        {
            //float totalSize = 0f;
            //for (int i = 0; i < Singleton<TransportManager>.instance.m_lineCurves[(int) lineID].Length; i++) {
            //    Bezier3 bez = Singleton<TransportManager>.instance.m_lineCurves[(int) lineID][i];
            //    totalSize += TLMUtils.calcBezierLenght(bez.a, bez.b, bez.c, bez.d, 0.1f);
            //}
            return Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_totalLength;
        }

        public static TransportSystemDefinition getLineNamingParameters(ushort lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, out string icon)
        {
            var tsd = TLMCW.getDefinitionForLine(lineIdx);
            if (tsd != default(TransportSystemDefinition))
            {
                GetNamingRulesFromTSD(out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix, tsd);
            }
            else
            {
                suffix = default(ModoNomenclatura);
                s = default(Separador);
                prefix = default(ModoNomenclatura);
                nonPrefix = default(ModoNomenclatura);
                zeros = false;
                invertPrefixSuffix = false;
            }
            icon = getIconForLine(lineIdx);
            return tsd;
        }

        public static bool isNumberUsed(int numLinha, TransportSystemDefinition tsdOr, int exclude)
        {
            numLinha = numLinha & 0xFFFF;
            if (numLinha == 0) return true;
            TLMUtils.doLog("tsdOr = " + tsdOr + " | lineNum =" + numLinha + "| cfgIdx = " + tsdOr.toConfigIndex());
            TLMCW.ConfigIndex tipo = tsdOr.toConfigIndex();

            for (ushort i = 1; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
                {
                    continue;
                }
                ushort lnum = Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber;
                var tsd = TLMCW.getDefinitionForLine(i);
                TLMUtils.doLog("tsd = " + tsd + "| lineNum = " + lnum + "| I=" + i + "| cfgIdx = " + tsd.toConfigIndex());
                if (tsd != default(TransportSystemDefinition) && i != exclude && tsd.toConfigIndex() == tipo && lnum == numLinha)
                {
                    return true;
                }
            }
            return false;
        }

        public static void GetNamingRulesFromTSD(out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, TransportSystemDefinition tsd)
        {
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            if (transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG)
            {
                suffix = ModoNomenclatura.Numero;
                s = Separador.Hifen;
                prefix = ModoNomenclatura.Romano;
                nonPrefix = ModoNomenclatura.Numero;
                zeros = false;
                invertPrefixSuffix = false;
            }
            else
            {
                suffix = (ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.SUFFIX);
                s = (Separador)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.SEPARATOR);
                prefix = (ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);
                nonPrefix = (ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.NON_PREFIX);
                zeros = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.LEADING_ZEROS);
                invertPrefixSuffix = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.INVERT_PREFIX_SUFFIX);
            }
        }

        public static bool hasPrefix(ref TransportLine t)
        {
            var tsd = TLMCW.getDefinitionForLine(ref t);
            if (tsd == default(TransportSystemDefinition))
            {
                return false;
            }
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(ushort idx)
        {
            var tsd = TLMCW.getDefinitionForLine(idx);
            if (tsd == default(TransportSystemDefinition))
            {
                return false;
            }
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(TransportInfo t)
        {
            TLMCW.ConfigIndex transportType = TLMCW.getConfigIndexForTransportInfo(t);
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }


        public static uint getPrefix(ushort idx)
        {
            var tsd = TLMCW.getDefinitionForLine(idx);
            if (tsd == default(TransportSystemDefinition))
            {
                return 0;
            }
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            if (transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum)
            {
                return Singleton<TransportManager>.instance.m_lines.m_buffer[idx].m_lineNumber / 1000u;
            }
            else
            {
                return 0;
            }
        }

        public static string getIconForLine(ushort lineIdx)
        {
            TLMCW.ConfigIndex transportType;
            var tsd = TLMCW.getDefinitionForLine(lineIdx);
            if (tsd != default(TransportSystemDefinition))
            {
                transportType = tsd.toConfigIndex();
            }
            else
            {
                transportType = TLMConfigWarehouse.ConfigIndex.BUS_CONFIG;
            }
            return GetIconForIndex(transportType);
        }

        public static string GetIconForIndex(TLMCW.ConfigIndex transportType)
        {
            switch (transportType & TLMCW.ConfigIndex.SYSTEM_PART)
            {
                case TLMCW.ConfigIndex.TRAIN_CONFIG:
                    return "TrainIcon";
                case TLMCW.ConfigIndex.METRO_CONFIG:
                    return "SubwayIcon";
                case TLMCW.ConfigIndex.BUS_CONFIG:
                    return "BusIcon";
                case TLMCW.ConfigIndex.TRAM_CONFIG:
                    return "TramIcon";
                case TLMCW.ConfigIndex.SHIP_CONFIG:
                    return "ShipLineIcon";
                case TLMCW.ConfigIndex.CABLE_CAR_CONFIG:
                    return "CableCarIcon";
                case TLMCW.ConfigIndex.MONORAIL_CONFIG:
                    return "MonorailIcon";
                case TLMCW.ConfigIndex.PLANE_CONFIG:
                    return "PlaneLineIcon";
                case TLMCW.ConfigIndex.FERRY_CONFIG:
                    return "FerryIcon";
                case TLMCW.ConfigIndex.BLIMP_CONFIG:
                    return "BlimpIcon";
                case TLMCW.ConfigIndex.EVAC_BUS_CONFIG:
                    return "EvacBusIcon";
                default:
                    return "BusIcon";
            }
        }

        /// <summary>
        /// </summary>
        /// <returns><c>true</c>, if recusive search for near stops was ended, <c>false</c> otherwise.</returns>
        /// <param name="pos">Position.</param>
        /// <param name="extendedMaxDistance">Max distance.</param>
        /// <param name="linesFound">Lines found.</param>
        public static bool GetNearLines(Vector3 pos, float maxDistance, ref List<ushort> linesFound)
        {
            float extendedMaxDistance = maxDistance * 1.3f;
            int num = Mathf.Max((int)((pos.x - extendedMaxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - extendedMaxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + extendedMaxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + extendedMaxDistance) / 64f + 135f), 269);
            bool noneFound = true;
            NetManager nm = Singleton<NetManager>.instance;
            TransportManager tm = Singleton<TransportManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = nm.m_nodeGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        NetInfo info = nm.m_nodes.m_buffer[(int)num6].Info;
                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport))
                        {
                            ushort transportLine = nm.m_nodes.m_buffer[(int)num6].m_transportLine;
                            TransportSystemDefinition tsd = TLMCW.getDefinitionForLine(transportLine);
                            if (transportLine != 0 && tsd != default(TransportSystemDefinition) && TLMCW.getCurrentConfigBool(tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.SHOW_IN_LINEAR_MAP))
                            {
                                TransportInfo info2 = tm.m_lines.m_buffer[(int)transportLine].Info;
                                if (!linesFound.Contains(transportLine) && (tm.m_lines.m_buffer[(int)transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[(int)num6].m_position);
                                    if (num8 < maxDistance * maxDistance || (info2.m_transportType == TransportInfo.TransportType.Ship && num8 < extendedMaxDistance * extendedMaxDistance))
                                    {
                                        linesFound.Add(transportLine);
                                        GetNearLines(nm.m_nodes.m_buffer[(int)num6].m_position, maxDistance, ref linesFound);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        num6 = nm.m_nodes.m_buffer[(int)num6].m_nextGridNode;
                        if (++num7 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return noneFound;
        }



        //GetNearStopPoints
        public static bool GetNearStopPoints(Vector3 pos, float maxDistance, ref Dictionary<ushort, Vector3> stopsFound, ItemClass.SubService[] subservicesAllowed = null, int maxDepht = 4, int depth = 0)
        {
            if (depth >= maxDepht)
                return false;
            if (subservicesAllowed == null)
            {
                subservicesAllowed = new ItemClass.SubService[] { ItemClass.SubService.PublicTransportTrain, ItemClass.SubService.PublicTransportMetro };
            }
            int num = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);
            bool noneFound = true;
            NetManager nm = Singleton<NetManager>.instance;
            TransportManager tm = Singleton<TransportManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort stopId = nm.m_nodeGrid[i * 270 + j];
                    int num7 = 0;
                    while (stopId != 0)
                    {
                        NetInfo info = nm.m_nodes.m_buffer[(int)stopId].Info;

                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport) && subservicesAllowed.Contains(info.m_class.m_subService))
                        {
                            ushort transportLine = nm.m_nodes.m_buffer[(int)stopId].m_transportLine;
                            if (transportLine != 0)
                            {
                                if (!stopsFound.Keys.Contains(stopId))
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[(int)stopId].m_position);
                                    if (num8 < maxDistance * maxDistance)
                                    {
                                        stopsFound[stopId] = nm.m_nodes.m_buffer[(int)stopId].m_position;
                                        GetNearStopPoints(nm.m_nodes.m_buffer[(int)stopId].m_position, maxDistance, ref stopsFound, subservicesAllowed, maxDepht, depth + 1);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        stopId = nm.m_nodes.m_buffer[(int)stopId].m_nextGridNode;
                        if (++num7 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return noneFound;
        }

        public static Vector2 gridPositionGameDefault(Vector3 pos)
        {
            int x = Mathf.Max((int)((pos.x) / 64f + 135f), 0);
            int z = Mathf.Max((int)((-pos.z) / 64f + 135f), 0);
            return new Vector2(x, z);
        }


        public static Vector2 gridPosition81Tiles(Vector3 pos, float invResolution = 24f)
        {
            int x = Mathf.Max((int)((pos.x) / invResolution + 648), 0);
            int z = Mathf.Max((int)((-pos.z) / invResolution + 648), 0);
            return new Vector2(x, z);
        }

        /// <summary>
        /// Index the lines.
        /// </summary>
        /// <returns>The lines indexed.</returns>
        /// <param name="intersections">Intersections.</param>
        /// <param name="t">Transport line to ignore.</param>
        public static Dictionary<string, ushort> SortLines(List<ushort> intersections, TransportLine t = default(TransportLine))
        {
            TransportManager tm = Singleton<TransportManager>.instance;
            Dictionary<String, ushort> otherLinesIntersections = new Dictionary<String, ushort>();
            foreach (ushort s in intersections)
            {
                TransportLine tl = tm.m_lines.m_buffer[(int)s];
                if (t.Equals(default(TransportLine)) || tl.Info.GetSubService() != t.Info.GetSubService() || tl.m_lineNumber != t.m_lineNumber)
                {
                    string transportTypeLetter = "";
                    var tsd = TLMCW.getDefinitionForLine(s);
                    if (tsd == default(TransportSystemDefinition))
                    {
                        continue;
                    }
                    switch (tsd.toConfigIndex())
                    {
                        case TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG:
                            transportTypeLetter = "A";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG:
                            transportTypeLetter = "B";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG:
                            transportTypeLetter = "C";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG:
                            transportTypeLetter = "D";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG:
                            transportTypeLetter = "E";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG:
                            transportTypeLetter = "F";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG:
                            transportTypeLetter = "G";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG:
                            transportTypeLetter = "H";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG:
                            transportTypeLetter = "I";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                            transportTypeLetter = "J";
                            break;
                    }
                    otherLinesIntersections.Add(transportTypeLetter + tl.m_lineNumber.ToString().PadLeft(5, '0'), s);
                }
            }
            return otherLinesIntersections;
        }



        public static void PrintIntersections(string airport, string harbor, string taxi, string regionalTrainStation, string cableCarStation, UIPanel intersectionsPanel, Dictionary<string, ushort> otherLinesIntersections, float scale = 1.0f, int maxItemsForSizeSwap = 3)
        {
            TransportManager tm = Singleton<TransportManager>.instance;

            int intersectionCount = otherLinesIntersections.Count;
            if (!String.IsNullOrEmpty(airport))
            {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(harbor))
            {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(taxi))
            {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(regionalTrainStation))
            {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(cableCarStation))
            {
                intersectionCount++;
            }
            float size = scale * (intersectionCount > maxItemsForSizeSwap ? 20 : 40);
            float multiplier = scale * (intersectionCount > maxItemsForSizeSwap ? 0.4f : 0.8f);
            foreach (var s in otherLinesIntersections.OrderBy(x => x.Key))
            {
                TransportLine intersectLine = tm.m_lines.m_buffer[(int)s.Value];
                ItemClass.SubService ss = getLineNamingParameters(s.Value, out ModoNomenclatura prefixo, out Separador separador, out ModoNomenclatura sufixo, out ModoNomenclatura naoPrefixado, out bool zeros, out bool invertPrefixSuffix, out string bgSprite).subService;
                createUIElement(out UIButtonLineInfo lineCircleIntersect, intersectionsPanel.transform);
                lineCircleIntersect.autoSize = false;
                lineCircleIntersect.width = size;
                lineCircleIntersect.height = size;
                lineCircleIntersect.color = intersectLine.m_color;
                lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
                lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
                lineCircleIntersect.name = "LineFormat";
                lineCircleIntersect.relativePosition = new Vector3(0f, 0f);
                lineCircleIntersect.atlas = TLMController.taLineNumber;
                lineCircleIntersect.normalBgSprite = bgSprite;
                lineCircleIntersect.hoveredColor = Color.white;
                lineCircleIntersect.hoveredTextColor = Color.red;
                lineCircleIntersect.lineID = s.Value;
                lineCircleIntersect.tooltip = tm.GetLineName(s.Value);
                lineCircleIntersect.eventClick += TLMController.instance.lineInfoPanel.openLineInfo;
                TLMUtils.createUIElement(out UILabel lineNumberIntersect, lineCircleIntersect.transform);
                lineNumberIntersect.autoSize = false;
                lineNumberIntersect.autoHeight = false;
                lineNumberIntersect.width = lineCircleIntersect.width;
                lineNumberIntersect.pivot = UIPivotPoint.MiddleCenter;
                lineNumberIntersect.textAlignment = UIHorizontalAlignment.Center;
                lineNumberIntersect.verticalAlignment = UIVerticalAlignment.Middle;
                lineNumberIntersect.name = "LineNumber";
                lineNumberIntersect.height = size;
                lineNumberIntersect.relativePosition = new Vector3(-0.5f, 0.5f);
                lineNumberIntersect.outlineColor = Color.black;
                lineNumberIntersect.useOutline = true;
                getLineActive(ref intersectLine, out bool day, out bool night);
                bool zeroed;
                unchecked
                {
                    zeroed = (tm.m_lines.m_buffer[s.Value].m_flags & (TransportLine.Flags)TLMTransportLineFlags.ZERO_BUDGET_CURRENT) != 0;
                }
                if (!day || !night || zeroed)
                {
                    TLMUtils.createUIElement(out UILabel daytimeIndicator, lineCircleIntersect.transform);
                    daytimeIndicator.autoSize = false;
                    daytimeIndicator.width = size;
                    daytimeIndicator.height = size;
                    daytimeIndicator.color = Color.white;
                    daytimeIndicator.pivot = UIPivotPoint.MiddleLeft;
                    daytimeIndicator.verticalAlignment = UIVerticalAlignment.Middle;
                    daytimeIndicator.name = "LineTime";
                    daytimeIndicator.relativePosition = new Vector3(0f, 0f);
                    daytimeIndicator.atlas = TLMController.taLineNumber;
                    daytimeIndicator.backgroundSprite = zeroed ? "NoBudgetIcon" : day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
                }
                setLineNumberCircleOnRef(s.Value, lineNumberIntersect);
                lineNumberIntersect.textScale *= multiplier;
                lineNumberIntersect.relativePosition *= multiplier;
            }
            if (airport != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "AirplaneIcon", airport);
            }
            if (harbor != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "ShipIcon", harbor);
            }
            if (taxi != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "TaxiIcon", taxi);
            }
            if (regionalTrainStation != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "RegionalTrainIcon", regionalTrainStation);
            }
            if (cableCarStation != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "CableCarIcon", cableCarStation);
            }
        }


        public static void getLineActive(ref TransportLine t, out bool day, out bool night)
        {
            day = (t.m_flags & TransportLine.Flags.DisabledDay) == TransportLine.Flags.None;
            night = (t.m_flags & TransportLine.Flags.DisabledNight) == TransportLine.Flags.None;
        }

        public static void setLineActive(ref TransportLine t, bool day, bool night)
        {
            t.SetActive(day, night);
        }

        private static void addExtraStationBuildingIntersection(UIComponent parent, float size, string bgSprite, string description)
        {
            TLMUtils.createUIElement(out UILabel lineCircleIntersect, parent.transform);
            lineCircleIntersect.autoSize = false;
            lineCircleIntersect.width = size;
            lineCircleIntersect.height = size;
            lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
            lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
            lineCircleIntersect.name = "LineFormat";
            lineCircleIntersect.relativePosition = new Vector3(0f, 0f);
            lineCircleIntersect.atlas = TLMController.taLineNumber;
            lineCircleIntersect.backgroundSprite = bgSprite;
            lineCircleIntersect.tooltip = description;
        }

        public static void setLineNumberCircleOnRef(ushort lineID, UITextComponent reference, float ratio = 1f)
        {
            getLineNumberCircleOnRefParams(lineID, ratio, out string text, out Color textColor, out float textScale, out Vector3 relativePosition);
            reference.text = text;
            reference.textScale = textScale;
            reference.relativePosition = relativePosition;
            reference.textColor = textColor;
            reference.useOutline = true;
            reference.outlineColor = Color.black;
        }

        private static void getLineNumberCircleOnRefParams(ushort lineID, float ratio, out string text, out Color textColor, out float textScale, out Vector3 relativePosition)
        {
            text = getLineStringId(lineID).Trim();
            string[] textParts = text.Split(new char[] { '\n' });
            int lenght = textParts.Max(x => x.Length);
            if (lenght >= 9 && textParts.Length == 1)
            {
                text = text.Replace("·", "\n").Replace(".", "\n").Replace("-", "\n").Replace("/", "\n").Replace(" ", "\n");
                textParts = text.Split(new char[] { '\n' });
                lenght = textParts.Max(x => x.Length);
            }
            if (lenght >= 8)
            {
                textScale = 0.4f * ratio;
                relativePosition = new Vector3(0f, 0.125f);
            }
            else if (lenght >= 6)
            {
                textScale = 0.666f * ratio;
                relativePosition = new Vector3(0f, 0.5f);
            }
            else if (lenght >= 4)
            {
                textScale = 1f * ratio;
                relativePosition = new Vector3(0f, 1f);
            }
            else if (lenght == 3 || textParts.Length > 1)
            {
                textScale = 1.25f * ratio;
                relativePosition = new Vector3(0f, 1.5f);
            }
            else if (lenght == 2)
            {
                textScale = 1.75f * ratio;
                relativePosition = new Vector3(-0.5f, 0.5f);
            }
            else
            {
                textScale = 2.3f * ratio;
                relativePosition = new Vector3(-0.5f, 0f);
            }
            textColor = TLMTransportLineExtension.instance.GetUseCustomConfig(lineID) ? Color.yellow : Color.white;
        }

        public static int getVehicleCapacity(ushort vehicleId)
        {
            var ai = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].Info.GetAI();
            if (ai as BusAI != null)
            {
                return (ai as BusAI).m_passengerCapacity;
            }
            if (ai as PassengerPlaneAI != null)
            {
                return (ai as PassengerPlaneAI).m_passengerCapacity;
            }
            if (ai as PassengerShipAI != null)
            {
                return (ai as PassengerShipAI).m_passengerCapacity;
            }
            if (ai as PassengerFerryAI != null)
            {
                return (ai as PassengerFerryAI).m_passengerCapacity;
            }
            if (ai as PassengerBlimpAI != null)
            {
                return (ai as PassengerBlimpAI).m_passengerCapacity;
            }
            if (ai as CableCarAI != null)
            {
                return (ai as CableCarAI).m_passengerCapacity;
            }
            //if (ai as MonorailAI != null)
            //{
            //    return (ai as MonorailAI).m_passengerCapacity;
            //}
            if (ai as PassengerTrainAI != null)
            {
                return (ai as PassengerTrainAI).m_passengerCapacity;
            }
            if (ai as TaxiAI != null)
            {
                return (ai as TaxiAI).m_passengerCapacity;
            }
            if (ai as TramAI != null)
            {
                return (ai as TramAI).m_passengerCapacity;
            }
            return 0;
        }

        public static string[] getAllStopsFromLine(ushort lineID)
        {
            TransportLine t = TLMController.instance.tm.m_lines.m_buffer[lineID];
            int stopsCount = t.CountStops(lineID);
            string[] result = new string[stopsCount];
            ItemClass.SubService ss = TLMCW.getDefinitionForLine(lineID).subService;
            for (int i = 0; i < stopsCount; i++)
            {
                ushort stationId = t.GetStop(i);
                result[i] = TLMLineUtils.getFullStationName(stationId, lineID, ss);
            }
            return result;
        }


        #region Line Utils
        public static void setLineColor(ushort lineIdx, Color color)
        {

            Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_color = color;
            Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_flags |= TransportLine.Flags.CustomColor;
        }
        public static void setLineName(ushort lineIdx, string name)
        {
            InstanceID lineIdSelecionado = default(InstanceID);
            lineIdSelecionado.TransportLine = lineIdx;
            if (name.Length > 0)
            {
                Singleton<InstanceManager>.instance.SetName(lineIdSelecionado, name);
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_flags |= TransportLine.Flags.CustomName;
            }
            else
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_flags &= ~TransportLine.Flags.CustomName;
            }
        }
        public static string calculateAutoName(ushort lineIdx, bool complete = false)
        {
            TransportManager tm = Singleton<TransportManager>.instance;
            TransportLine t = tm.m_lines.m_buffer[(int)lineIdx];
            ItemClass.SubService ss = ItemClass.SubService.None;
            string resultName;
            if (t.Info.m_transportType == TransportInfo.TransportType.EvacuationBus)
            {
                return null;
            }
            if (t.Info.m_transportType == TransportInfo.TransportType.Train)
            {
                ss = ItemClass.SubService.PublicTransportTrain;
            }
            else if (t.Info.m_transportType == TransportInfo.TransportType.Metro)
            {
                ss = ItemClass.SubService.PublicTransportMetro;
            }
            int stopsCount = t.CountStops(lineIdx);
            ushort[] stopBuildings = new ushort[stopsCount];
            MultiMap<ushort, Vector3> bufferToDraw = new MultiMap<ushort, Vector3>();
            if (t.Info.m_transportType != TransportInfo.TransportType.Bus && t.Info.m_transportType != TransportInfo.TransportType.Tram && CalculateSimmetry(ss, stopsCount, t, out int middle))
            {
                string station1Name = getStationName(t.GetStop(middle), lineIdx, ss);

                string station2Name = getStationName(t.GetStop(middle + stopsCount / 2), lineIdx, ss);

                resultName = station1Name + " - " + station2Name;
            }
            else
            {
                //float autoNameSimmetryImprecision = 0.075f;
                DistrictManager dm = Singleton<DistrictManager>.instance;
                Dictionary<int, KeyValuePair<TLMCW.ConfigIndex, String>> stationsList = new Dictionary<int, KeyValuePair<TLMCW.ConfigIndex, String>>();
                NetManager nm = Singleton<NetManager>.instance;
                for (int j = 0; j < stopsCount; j++)
                {
                    String value = getStationName(t.GetStop(j), lineIdx, ss, out ItemClass.Service service, out ItemClass.SubService subservice, out string prefix, out ushort buidingId, true);
                    var tsd = TransportSystemDefinition.from(Singleton<BuildingManager>.instance.m_buildings.m_buffer[buidingId].Info.GetAI());
                    stationsList.Add(j, new KeyValuePair<TLMCW.ConfigIndex, string>(tsd != null ? tsd.toConfigIndex() : GameServiceExtensions.toConfigIndex(service, subservice), value));
                }
                uint mostImportantCategoryInLine = stationsList.Select(x => (x.Value.Key).getPriority()).Min();
                if (mostImportantCategoryInLine < int.MaxValue)
                {
                    var mostImportantPlaceIdx = stationsList.Where(x => x.Value.Key.getPriority() == mostImportantCategoryInLine).Min(x => x.Key);
                    var destiny = stationsList[mostImportantPlaceIdx];

                    var inverseIdxCenter = (mostImportantPlaceIdx + stopsCount / 2) % stopsCount;
                    int resultIdx = inverseIdxCenter;
                    //int simmetryMargin = (int)Math.Ceiling(stopsCount * autoNameSimmetryImprecision);
                    //int resultIdx = -1;
                    //var destBuilding = getStationBuilding((uint)mostImportantPlaceIdx, ss);
                    //BuildingManager bm = Singleton<BuildingManager>.instance;
                    //for (int i = 0; i <= simmetryMargin; i++)
                    //{
                    //    int currentI = (inverseIdxCenter + i + stopsCount) % stopsCount;


                    //    var iBuilding = getStationBuilding((uint)currentI, ss);
                    //    if ((resultIdx == -1 || stationsList[currentI].Key.getPriority() < stationsList[resultIdx].Key.getPriority()) && iBuilding != destBuilding)
                    //    {
                    //        resultIdx = currentI;
                    //    }
                    //    if (i == 0) continue;
                    //    currentI = (inverseIdxCenter - i + stopsCount) % stopsCount;
                    //    iBuilding = getStationBuilding((uint)currentI, ss);
                    //    if ((resultIdx == -1 || stationsList[currentI].Key.getPriority() < stationsList[resultIdx].Key.getPriority()) && iBuilding != destBuilding)
                    //    {
                    //        resultIdx = currentI;
                    //    }
                    //}
                    string originName = "";
                    //int districtOriginId = -1;
                    if (resultIdx >= 0 && stationsList[resultIdx].Key.isLineNamingEnabled())
                    {
                        var origin = stationsList[resultIdx];
                        var transportType = origin.Key;
                        var name = origin.Value;
                        originName = GetStationNameWithPrefix(transportType, name);
                        originName += " - ";
                    }
                    //else
                    //{
                    //    NetNode nn = nm.m_nodes.m_buffer[t.GetStop((int)resultIdx)];
                    //    Vector3 location = nn.m_position;
                    //    districtOriginId = dm.GetDistrict(location);
                    //    if (districtOriginId > 0)
                    //    {
                    //        District d = dm.m_districts.m_buffer[districtOriginId];
                    //        originName = dm.GetDistrictName(districtOriginId) + " - ";
                    //    }
                    //    else {
                    //        originName = "";
                    //    }
                    //}
                    if (!destiny.Key.isLineNamingEnabled())
                    {
                        NetNode nn = nm.m_nodes.m_buffer[t.GetStop((int)resultIdx)];
                        Vector3 location = nn.m_position;
                        int districtDestinyId = dm.GetDistrict(location);
                        //if (districtDestinyId == districtOriginId)
                        //{
                        //    District d = dm.m_districts.m_buffer[districtDestinyId];
                        //    return (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + dm.GetDistrictName(districtDestinyId);
                        //}
                        //else
                        if (districtDestinyId > 0)
                        {
                            District d = dm.m_districts.m_buffer[districtDestinyId];
                            return originName + dm.GetDistrictName(districtDestinyId);
                        }
                    }
                    resultName = originName + GetStationNameWithPrefix(destiny.Key, destiny.Value);
                }
                else
                {
                    resultName = autoNameByDistrict(t, stopsCount, out middle);
                }
            }
            if (TLMCW.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME) && complete)
            {
                string format = "[{0}] {1}";
                return string.Format(format, TLMLineUtils.getLineStringId(lineIdx).Replace('\n', ' '), resultName);
            }
            else
            {
                return resultName;
            }
        }
        private static string GetStationNameWithPrefix(TLMCW.ConfigIndex transportType, string name)
        {
            return transportType.getPrefixTextNaming().Trim() + (transportType.getPrefixTextNaming().Trim() != string.Empty ? " " : "") + name;
        }
        private static string autoNameByDistrict(TransportLine t, int stopsCount, out int middle)
        {

            DistrictManager dm = Singleton<DistrictManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            string result = "";
            byte lastDistrict = 0;
            Vector3 local;
            byte district;
            List<int> districtList = new List<int>();
            for (int j = 0; j < stopsCount; j++)
            {
                local = nm.m_nodes.m_buffer[(int)t.GetStop(j)].m_bounds.center;
                district = dm.GetDistrict(local);
                if ((district != lastDistrict) && district != 0)
                {
                    districtList.Add(district);
                }
                if (district != 0)
                {
                    lastDistrict = district;
                }
            }

            local = nm.m_nodes.m_buffer[(int)t.GetStop(0)].m_bounds.center;
            district = dm.GetDistrict(local);
            if ((district != lastDistrict) && district != 0)
            {
                districtList.Add(district);
            }
            middle = -1;
            int[] districtArray = districtList.ToArray();
            if (districtArray.Length == 1)
            {
                return (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + dm.GetDistrictName(districtArray[0]);
            }
            else if (findSimetry(districtArray, out middle))
            {
                int firstIdx = middle;
                int lastIdx = middle + districtArray.Length / 2;

                result = dm.GetDistrictName(districtArray[firstIdx % districtArray.Length]) + " - " + dm.GetDistrictName(districtArray[lastIdx % districtArray.Length]);
                if (lastIdx - firstIdx > 1)
                {
                    result += ", via ";
                    for (int k = firstIdx + 1; k < lastIdx; k++)
                    {
                        result += dm.GetDistrictName(districtArray[k % districtArray.Length]);
                        if (k + 1 != lastIdx)
                        {
                            result += ", ";
                        }
                    }
                }
                return result;
            }
            else
            {
                bool inicio = true;
                foreach (int i in districtArray)
                {
                    result += (inicio ? "" : " - ") + dm.GetDistrictName(i);
                    inicio = false;
                }
                return result;
            }
        }
        public static void setStopName(string newName, uint stopId, ushort lineId, OnEndProcessingBuildingName callback)
        {
            TLMUtils.doLog("setStopName! {0} - {1} - {2}", newName, stopId, lineId);
            ushort buildingId = getStationBuilding(stopId, toSubService(Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_transportType), true, true);
            if (buildingId == 0)
            {
                TLMUtils.doLog("b=0");
                TLMStopsExtension.instance.SetStopName(newName, stopId);
                callback();
            }
            else
            {
                TLMUtils.doLog("b≠0 ({0})", buildingId);
                Singleton<BuildingManager>.instance.StartCoroutine(setBuildingName(buildingId, newName, callback));
            }
        }
        public static bool CalculateSimmetry(ItemClass.SubService ss, int stopsCount, TransportLine t, out int middle)
        {
            int j;
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            middle = -1;
            if ((t.m_flags & (TransportLine.Flags.Invalid | TransportLine.Flags.Temporary)) != TransportLine.Flags.None)
            {
                return false;
            }
            //try to find the loop
            for (j = -1; j < stopsCount / 2; j++)
            {
                int offsetL = (j + stopsCount) % stopsCount;
                int offsetH = (j + 2) % stopsCount;
                NetNode nn1 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetL)];
                NetNode nn2 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetH)];
                ushort buildingId1 = TLMUtils.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                ushort buildingId2 = TLMUtils.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                //					TLMUtils.doLog("buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                //					TLMUtils.doLog("offsetL="+offsetL+"|offsetH="+offsetH);
                if (buildingId1 == buildingId2)
                {
                    middle = j + 1;
                    break;
                }
            }
            //				TLMUtils.doLog("middle="+middle);
            if (middle >= 0)
            {
                for (j = 1; j <= stopsCount / 2; j++)
                {
                    int offsetL = (-j + middle + stopsCount) % stopsCount;
                    int offsetH = (j + middle) % stopsCount;
                    //						TLMUtils.doLog("offsetL="+offsetL+"|offsetH="+offsetH);
                    //						TLMUtils.doLog("t.GetStop (offsetL)="+t.GetStop (offsetH)+"|t.GetStop (offsetH)="+t.GetStop (offsetH));
                    NetNode nn1 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetL)];
                    NetNode nn2 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetH)];
                    ushort buildingId1 = TLMUtils.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                    ushort buildingId2 = TLMUtils.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                    //						TLMUtils.doLog("buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                    //						TLMUtils.doLog("buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                    //						TLMUtils.doLog("offsetL="+offsetL+"|offsetH="+offsetH);
                    if (buildingId1 != buildingId2)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public static string getStationName(uint stopId, ushort lineId, ItemClass.SubService ss, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, out ushort buildingID, bool excludeCargo = false)
        {
            string savedName = TLMStopsExtension.instance.GetStopName(stopId);
            if (savedName != null)
            {
                serviceFound = ItemClass.Service.PublicTransport;
                subserviceFound = toSubService(Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_transportType);
                prefix = "";
                buildingID = 0;
                return savedName;
            }
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            buildingID = getStationBuilding(stopId, ss, excludeCargo);

            Vector3 location = nn.m_position;
            if (buildingID > 0)
            {
                return TLMUtils.getBuildingName(buildingID, out serviceFound, out subserviceFound, out prefix);
            }



            NetNode nextNode = nm.m_nodes.m_buffer[nn.m_nextGridNode];
            //return nm.GetSegmentName(segId);
            ushort segId = FindNearNamedRoad(nn.m_position);
            string segName = nm.GetSegmentName(segId);
            NetSegment seg = nm.m_segments.m_buffer[segId];
            ushort cross1nodeId = seg.m_startNode;
            ushort cross2nodeId = seg.m_endNode;

            string crossSegName = string.Empty;

            NetNode cross1node = nm.m_nodes.m_buffer[cross1nodeId];
            for (int i = 0; i < 8; i++)
            {
                var iSegId = cross1node.GetSegment(i);
                if (iSegId > 0 && iSegId != segId)
                {
                    string iSegName = nm.GetSegmentName(iSegId);
                    if (iSegName != string.Empty && segName != iSegName)
                    {
                        crossSegName = iSegName;
                        break;
                    }
                }
            }
            if (crossSegName == string.Empty)
            {
                NetNode cross2node = nm.m_nodes.m_buffer[cross2nodeId];
                for (int i = 0; i < 8; i++)
                {
                    var iSegId = cross2node.GetSegment(i);
                    if (iSegId > 0 && iSegId != segId)
                    {
                        string iSegName = nm.GetSegmentName(iSegId);
                        if (iSegName != string.Empty && segName != iSegName)
                        {
                            crossSegName = iSegName;
                            break;
                        }
                    }
                }
            }
            prefix = "";
            if (segName != string.Empty)
            {
                serviceFound = ItemClass.Service.Road;
                subserviceFound = ItemClass.SubService.PublicTransportBus;
                if (crossSegName == string.Empty)
                {
                    return segName;
                }
                else
                {
                    prefix = segName + " x ";
                    return crossSegName;
                }

            }
            else
            {
                serviceFound = ItemClass.Service.None;
                subserviceFound = ItemClass.SubService.None;
                return "????????";
            }
            //}

            //}
        }
        public static readonly ItemClass.Service[] seachOrder = new ItemClass.Service[]{
            ItemClass.Service.PublicTransport,
            ItemClass.Service.Monument,
            ItemClass.Service.Beautification,
            ItemClass.Service.Disaster,
            ItemClass.Service.HealthCare,
            ItemClass.Service.FireDepartment,
            ItemClass.Service.PoliceDepartment,
            ItemClass.Service.Tourism,
            ItemClass.Service.Education,
            ItemClass.Service.Garbage,
            ItemClass.Service.Office,
            ItemClass.Service.Commercial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Residential,
            ItemClass.Service.Electricity,
            ItemClass.Service.Water
        };
        private static ItemClass.SubService toSubService(TransportInfo.TransportType t)
        {
            switch (t)
            {
                case TransportInfo.TransportType.Airplane:
                    return ItemClass.SubService.PublicTransportPlane;
                case TransportInfo.TransportType.Bus:
                    return ItemClass.SubService.PublicTransportBus;
                case TransportInfo.TransportType.Metro:
                    return ItemClass.SubService.PublicTransportMetro;
                case TransportInfo.TransportType.Ship:
                    return ItemClass.SubService.PublicTransportShip;
                case TransportInfo.TransportType.Taxi:
                    return ItemClass.SubService.PublicTransportTaxi;
                case TransportInfo.TransportType.Train:
                    return ItemClass.SubService.PublicTransportTrain;
                case TransportInfo.TransportType.Tram:
                    return ItemClass.SubService.PublicTransportTram;
                default:
                    return ItemClass.SubService.None;
            }
        }
        public static string getStationName(ushort stopId, ushort lineId, ItemClass.SubService ss)
        {
            return getStationName(stopId, lineId, ss, out ItemClass.Service serv, out ItemClass.SubService subServ, out string prefix, out ushort buildingId, true);
        }
        public static string getFullStationName(ushort stopId, ushort lineId, ItemClass.SubService ss)
        {
            string result = getStationName(stopId, lineId, ss, out ItemClass.Service serv, out ItemClass.SubService subServ, out string prefix, out ushort buildingId, true);
            return string.IsNullOrEmpty(prefix) ? result : prefix + " " + result;
        }
        public static Vector3 getStationBuildingPosition(uint stopId, ItemClass.SubService ss)
        {
            ushort buildingId = getStationBuilding(stopId, ss);


            if (buildingId > 0)
            {
                BuildingManager bm = Singleton<BuildingManager>.instance;
                Building b = bm.m_buildings.m_buffer[buildingId];
                InstanceID iid = default(InstanceID);
                iid.Building = buildingId;
                return b.m_position;
            }
            else
            {
                NetManager nm = Singleton<NetManager>.instance;
                NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
                return nn.m_position;
            }
        }
        public static ushort getStationBuilding(uint stopId, ItemClass.SubService ss, bool excludeCargo = false, bool restrictToTransportType = false)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            ushort buildingId = 0, tempBuildingId;

            if (ss != ItemClass.SubService.None)
            {
                tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                if (!excludeCargo || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is TransportStationAI)
                {
                    buildingId = tempBuildingId;
                }
            }
            if (buildingId == 0 && !restrictToTransportType)
            {
                tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.Active, Building.Flags.Untouchable);
                if (!excludeCargo || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is TransportStationAI)
                {
                    buildingId = tempBuildingId;
                }
                if (buildingId == 0)
                {
                    tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                    if (!excludeCargo || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is TransportStationAI)
                    {
                        buildingId = tempBuildingId;
                    }
                    if (buildingId == 0)
                    {
                        int iterator = 1;
                        while (buildingId == 0 && iterator < seachOrder.Count())
                        {
                            buildingId = TLMUtils.FindBuilding(nn.m_position, 100f, seachOrder[iterator], ItemClass.SubService.None, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                            iterator++;
                        }
                    }
                }
            }
            return buildingId;

        }
        public static string getPrefixesServedAbstract(ushort m_buildingID, bool secondary)
        {
            Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingID];
            DepotAI ai = b.Info.GetAI() as DepotAI;
            if (ai == null)
                return "";
            List<string> options = TLMUtils.getDepotPrefixesOptions(TLMCW.getConfigIndexForTransportInfo(secondary ? ai.m_secondaryTransportInfo : ai.m_transportInfo));
            var prefixes = TLMDepotAI.getPrefixesServedByDepot(m_buildingID, secondary);
            if (prefixes == null)
            {
                TLMUtils.doErrorLog("DEPOT AI WITH WRONG TYPE!!! id:{0} ({1})", m_buildingID, BuildingManager.instance.GetBuildingName(m_buildingID, default(InstanceID)));
                return null;
            }
            List<string> saida = new List<string>();
            if (prefixes.Contains(0))
                saida.Add(Locale.Get("TLM_UNPREFIXED_SHORT"));
            uint sequenceInit = 0;
            bool isInSequence = false;
            for (uint i = 1; i < options.Count; i++)
            {
                if (prefixes.Contains(i))
                {
                    if (sequenceInit == 0 || !isInSequence)
                    {
                        sequenceInit = i;
                        isInSequence = true;
                    }
                }
                else if (sequenceInit != 0 && isInSequence)
                {
                    if (i - 1 == sequenceInit)
                    {
                        saida.Add(options[(int)sequenceInit]);
                    }
                    else
                    {
                        saida.Add(options[(int)sequenceInit] + "-" + options[(int)(i - 1)]);
                    }
                    isInSequence = false;
                }
            }
            if (sequenceInit != 0 && isInSequence)
            {
                if (sequenceInit == options.Count - 1)
                {
                    saida.Add(options[(int)sequenceInit]);
                }
                else
                {
                    saida.Add(options[(int)sequenceInit] + "-" + options[(int)(options.Count - 1)]);
                }
                isInSequence = false;
            }
            if (prefixes.Contains(65))
                saida.Add(Locale.Get("TLM_REGIONAL_SHORT"));
            return string.Join(" ", saida.ToArray());
        }
        internal static string getTransportSystemPrefixName(TLMConfigWarehouse.ConfigIndex index, uint prefix)
        {
            var extension = getExtensionFromConfigIndex(index);
            if (extension == null)
            {
                return "";
            }
            return extension.GetName(prefix);
        }
        internal static ITLMTransportTypeExtension getExtensionFromConfigIndex(TLMConfigWarehouse.ConfigIndex index)
        {
            var tsd = TLMConfigWarehouse.getTransportSystemDefinitionForConfigTransport(index);
            TLMUtils.doLog("getExtensionFromConfigIndex Target TSD: " + tsd + " from idx: " + index);
            return tsd.GetTransportExtension();
        }
        internal static ITLMTransportTypeExtension getExtensionFromTransportSystemDefinition(TransportSystemDefinition tsd)
        {
            return tsd.GetTransportExtension();
        }
        public static IAssetSelectorExtension getExtensionFromTransportLine(ushort lineID)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];

            if (t.m_lineNumber != 0 && t.m_stops != 0)
            {
                if (TLMTransportLineExtension.instance.GetUseCustomConfig(lineID))
                {
                    return TLMTransportLineExtension.instance;
                }
                else
                {
                    return TransportSystemDefinition.from(lineID).GetTransportExtension();
                }
            }
            return null;
        }
        internal static readonly ModoNomenclatura[] nomenclaturasComNumeros = new ModoNomenclatura[]
        {
        ModoNomenclatura. LatinoMinusculoNumero ,
        ModoNomenclatura. LatinoMaiusculoNumero ,
        ModoNomenclatura. GregoMinusculoNumero,
        ModoNomenclatura. GregoMaiusculoNumero,
        ModoNomenclatura. CirilicoMinusculoNumero,
        ModoNomenclatura. CirilicoMaiusculoNumero
        };
        #endregion
    }

}
