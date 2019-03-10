using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.NetNodeExt;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.Commons.TextureAtlas;
using Klyte.TransportLinesManager.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.Utils.KlyteUtils;
using CIdx = Klyte.TransportLinesManager.TLMConfigWarehouse.ConfigIndex;
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
            TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            TransportInfo info = tl.Info;
            int budgetClass = Singleton<EconomyManager>.instance.GetBudget(info.m_class);
            if (TLMTransportLineExtension.instance.IsUsingCustomConfig(lineId))
            {
                if (TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(lineId))
                {
                    int targetCount = (int)TLMTransportLineExtension.instance.GetBudgetMultiplierForHour(lineId, Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 5;
                    tl.m_budget = 100;
                    float unitCount = tl.CalculateTargetVehicleCount();
                    return targetCount / unitCount + 0.005f;
                }
                else
                {
                    return TLMTransportLineExtension.instance.GetBudgetMultiplierForHour(lineId, Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 100f;
                }
            }
            else
            {
                var tsd = TransportSystemDefinition.getDefinitionForLine(ref tl);
                uint prefix = TLMLineUtils.getPrefix(lineId);
                return TLMLineUtils.getExtensionFromConfigIndex(tsd.toConfigIndex()).GetBudgetMultiplierForHour(prefix, Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 100f;
            }
        }
        public static bool isPerHourBudget(ushort lineId)
        {
            TransportLine __instance = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            TransportInfo info = __instance.Info;
            int budgetClass = Singleton<EconomyManager>.instance.GetBudget(info.m_class);
            if (TLMTransportLineExtension.instance.IsUsingCustomConfig(lineId))
            {
                return TLMTransportLineExtension.instance.GetBudgetsMultiplier(lineId).Length == 8;
            }
            else
            {
                var tsd = TransportSystemDefinition.getDefinitionForLine(ref __instance);
                uint prefix = TLMLineUtils.getPrefix(lineId);
                return TLMLineUtils.getExtensionFromConfigIndex(tsd.toConfigIndex()).GetBudgetsMultiplier(prefix).Length == 8;
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
            var tsd = TransportSystemDefinition.getDefinitionForLine(lineIdx);
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

        public static bool isNumberUsed(int numLinha, ref TransportSystemDefinition tsdOr, int exclude)
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
                var tsd = TransportSystemDefinition.getDefinitionForLine(i);
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
        public static bool hasPrefix(ref TransportSystemDefinition tsd)
        {
            if (tsd == default(TransportSystemDefinition))
            {
                return false;
            }
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(ref TransportLine t)
        {
            var tsd = TransportSystemDefinition.getDefinitionForLine(ref t);
            if (tsd == default(TransportSystemDefinition))
            {
                return false;
            }
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(ushort idx)
        {
            var tsd = TransportSystemDefinition.getDefinitionForLine(idx);
            if (tsd == default(TransportSystemDefinition))
            {
                return false;
            }
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(TransportInfo t)
        {
            TLMCW.ConfigIndex transportType = TransportSystemDefinition.from(t).toConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }


        public static uint getPrefix(ushort idx)
        {
            var tsd = TransportSystemDefinition.getDefinitionForLine(idx);
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
            var tsd = TransportSystemDefinition.getDefinitionForLine(lineIdx);
            if (tsd != default(TransportSystemDefinition))
            {
                transportType = tsd.toConfigIndex();
            }
            else
            {
                transportType = TLMConfigWarehouse.ConfigIndex.BUS_CONFIG;
            }
            return TLMUtils.GetLineIcon(TransportManager.instance.m_lines.m_buffer[lineIdx].m_lineNumber, transportType, ref tsd).getImageName();
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
                            TransportSystemDefinition tsd = TransportSystemDefinition.getDefinitionForLine(transportLine);
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
                    var tsd = TransportSystemDefinition.getDefinitionForLine(s);
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
                        case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG:
                            transportTypeLetter = "K";
                            break;
                        case TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG:
                            transportTypeLetter = "L";
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
                lineCircleIntersect.atlas = LineUtilsTextureAtlas.instance.atlas;
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
                    daytimeIndicator.atlas = LineUtilsTextureAtlas.instance.atlas;
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
            lineCircleIntersect.atlas = LineUtilsTextureAtlas.instance.atlas;
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
            textColor = TLMTransportLineExtension.instance.IsUsingCustomConfig(lineID) ? Color.yellow : Color.white;
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
            TransportLine t = TransportManager.instance.m_lines.m_buffer[lineID];
            int stopsCount = t.CountStops(lineID);
            string[] result = new string[stopsCount];
            ItemClass.SubService ss = TransportSystemDefinition.getDefinitionForLine(lineID).subService;
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

        private static TransportInfo.TransportType[] roadTransportTypes = new TransportInfo.TransportType[] { TransportInfo.TransportType.Bus, TransportInfo.TransportType.Tram };

        public static string calculateAutoName(ushort lineIdx)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx];
            if ((t.m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
            {
                return null;
            }
            ushort nextStop = t.m_stops;
            bool allowPrefixInStations = roadTransportTypes.Contains(t.Info.m_transportType);
            List<Tuple<NamingType, string>> stations = new List<Tuple<NamingType, string>>();
            do
            {
                var stopNode = NetManager.instance.m_nodes.m_buffer[nextStop];
                var stationName = getStationName(nextStop, lineIdx, t.Info.m_class.m_subService, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefixFound, out ushort buildingId, out NamingType namingType, true, true);
                var tuple = Tuple.New(namingType, allowPrefixInStations ? $"{prefixFound?.Trim()} {stationName?.Trim()}".Trim() : stationName);
                stations.Add(tuple);
                nextStop = TransportLine.GetNextStop(nextStop);
            } while (nextStop != t.m_stops && nextStop != 0);
            string prefix = "";
            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME))
            {
                prefix = $"[{getLineStringId(lineIdx)}] ";
            }
            TLMUtils.doLog($"stations => [{string.Join(" ; ", stations.Select(x => $"{x.First}|{x.Second}").ToArray())}]");
            if (stations.Count % 2 == 0 && stations.Count > 2)
            {
                TLMUtils.doLog($"Try Simmetric");
                int middle = -1;
                for (int i = 1; i <= stations.Count / 2; i++)
                {
                    if (stations[i - 1].First == stations[i + 1].First && stations[i - 1].Second == stations[i + 1].Second)
                    {
                        middle = i;
                        break;
                    }
                }
                TLMUtils.doLog($"middle => {middle}");
                if (middle != -1)
                {
                    bool simmetric = true;
                    middle += stations.Count;
                    for (int i = 1; i < stations.Count / 2; i++)
                    {
                        var A = stations[(middle + i) % stations.Count];
                        var B = stations[(middle - i) % stations.Count];
                        if (A.First != B.First || A.Second != B.Second)
                        {
                            simmetric = false;
                            break;
                        }
                    }
                    if (simmetric)
                    {
                        return $"{prefix}{stations[(middle) % stations.Count].Second } - { stations[(middle + stations.Count / 2) % stations.Count].Second}";
                    }
                }
            }
            List<Tuple<int, NamingType, string>> idxStations = stations.Select((x, y) => Tuple.New(y, x.First, x.Second)).OrderBy(x => x.Second.GetNamePrecedenceRate()).ToList();

            int targetStart = 0;
            int mostRelevantEndIdx = -1;
            int j = 0;
            int maxDistanceEnd = (int)(idxStations.Count / 8f + 0.5f);
            TLMUtils.doLog("idxStations");
            do
            {
                var peerCandidate = idxStations.Where(x => x.Third != idxStations[j].Third && Math.Abs((x.First < idxStations[j].First ? x.First + idxStations.Count : x.First) - idxStations.Count / 2 - idxStations[j].First) <= maxDistanceEnd).OrderBy(x => x.Second.GetNamePrecedenceRate()).FirstOrDefault();
                if (peerCandidate != null && (mostRelevantEndIdx == -1 || stations[mostRelevantEndIdx].First.GetNamePrecedenceRate() > peerCandidate.Second.GetNamePrecedenceRate()))
                {
                    targetStart = j;
                    mostRelevantEndIdx = peerCandidate.First;
                }
                j++;
            } while (j < idxStations.Count && idxStations[j].Second.GetNamePrecedenceRate() == idxStations[0].Second.GetNamePrecedenceRate());

            if (mostRelevantEndIdx >= 0)
            {
                return $"{prefix}{idxStations[targetStart].Third} - {stations[mostRelevantEndIdx].Second}";
            }
            else
            {
                return prefix + (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + idxStations[0].Third;
            }

        }

        private static string GetStationNameWithPrefix(TLMCW.ConfigIndex transportType, string name)
        {
            return transportType.getPrefixTextNaming().Trim() + (transportType.getPrefixTextNaming().Trim() != string.Empty ? " " : "") + name;
        }

        public static void setStopName(string newName, uint stopId, ushort lineId, OnEndProcessingBuildingName callback)
        {
            TLMUtils.doLog("setStopName! {0} - {1} - {2}", newName, stopId, lineId);
            ushort buildingId = getStationBuilding(stopId, Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService, true, true);
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
        public static string getStationName(uint stopId, ushort lineId, ItemClass.SubService ss, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, out ushort buildingID, out NamingType resultNamingType, bool excludeCargo = false, bool useRestrictionForAreas = false)
        {
            string savedName = TLMStopsExtension.instance.GetStopName(stopId);
            if (savedName != null)
            {
                serviceFound = ItemClass.Service.PublicTransport;
                subserviceFound = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService;
                prefix = "";
                buildingID = 0;
                resultNamingType = NamingTypeExtensions.from(serviceFound, subserviceFound);
                return savedName;
            }


            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];

            var nearStops = FindNearStops(nn.m_position);
            foreach (var stop in nearStops)
            {
                if (stop != stopId)
                {
                    savedName = TLMStopsExtension.instance.GetStopName(stopId);
                    if (savedName != null)
                    {
                        serviceFound = ItemClass.Service.PublicTransport;
                        subserviceFound = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info.m_class.m_subService;
                        prefix = "";
                        buildingID = 0;
                        resultNamingType = NamingTypeExtensions.from(serviceFound, subserviceFound);
                        return savedName;
                    }
                }
            }

            buildingID = getStationBuilding(stopId, ss, excludeCargo);

            if (buildingID > 0)
            {
                var name = TLMUtils.getBuildingName(buildingID, out serviceFound, out subserviceFound, out prefix);
                resultNamingType = NamingTypeExtensions.from(serviceFound, subserviceFound);
                return name;
            }
            Vector3 location = nn.m_position;
            prefix = "";
            if (GetPark(location) > 0 && (!useRestrictionForAreas || TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.PARKAREA_NAME_CONFIG | TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF)))
            {
                prefix = TLMCW.ConfigIndex.PARKAREA_NAME_CONFIG.getPrefixTextNaming();
                serviceFound = ItemClass.Service.Natural;
                subserviceFound = ItemClass.SubService.BeautificationParks;
                resultNamingType = NamingType.PARKAREA;
                return DistrictManager.instance.GetParkName(GetPark(location));
            }
            else if (GetAddressStreetAndNumber(location, location, out int number, out string streetName) && (!useRestrictionForAreas || TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.ADDRESS_NAME_CONFIG | TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF)) && !string.IsNullOrEmpty(streetName))
            {
                prefix = TLMCW.ConfigIndex.ADDRESS_NAME_CONFIG.getPrefixTextNaming();
                serviceFound = ItemClass.Service.Road;
                subserviceFound = ItemClass.SubService.PublicTransportBus;
                resultNamingType = NamingType.ADDRESS;
                return streetName + ", " + number;

            }
            else if (DistrictManager.instance.GetDistrict(location) > 0 && (!useRestrictionForAreas || TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.DISTRICT_NAME_CONFIG | TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF)))
            {
                prefix = TLMCW.ConfigIndex.DISTRICT_NAME_CONFIG.getPrefixTextNaming();
                serviceFound = ItemClass.Service.Natural;
                subserviceFound = ItemClass.SubService.None;
                resultNamingType = NamingType.DISTRICT;
                return DistrictManager.instance.GetDistrictName(DistrictManager.instance.GetDistrict(location));
            }
            else
            {
                serviceFound = ItemClass.Service.None;
                subserviceFound = ItemClass.SubService.None;
                resultNamingType = NamingType.NONE;
                return "????????";
            }
        }
        public static readonly ItemClass.Service[] searchOrder = new ItemClass.Service[]{
            ItemClass.Service.PublicTransport,
            ItemClass.Service.Monument,
            ItemClass.Service.Beautification,
            ItemClass.Service.Natural,
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
        public static string getStationName(ushort stopId, ushort lineId, ItemClass.SubService ss)
        {
            return getStationName(stopId, lineId, ss, out ItemClass.Service serv, out ItemClass.SubService subServ, out string prefix, out ushort buildingId, out NamingType namingType, true);
        }
        public static string getFullStationName(ushort stopId, ushort lineId, ItemClass.SubService ss)
        {
            string result = getStationName(stopId, lineId, ss, out ItemClass.Service serv, out ItemClass.SubService subServ, out string prefix, out ushort buildingId, out NamingType namingType, true);
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

        //ORDEM DE BUSCA DE CONFIG
        private static CIdx[] searchOrderStationNamingRule = new CIdx[] {
        CIdx.PLANE_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.SHIP_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.BLIMP_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.FERRY_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.CABLE_CAR_USE_FOR_AUTO_NAMING_REF              ,
        CIdx.TRAIN_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.METRO_USE_FOR_AUTO_NAMING_REF                  ,
        CIdx.MONORAIL_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.TRAM_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.BUS_USE_FOR_AUTO_NAMING_REF                    ,
        CIdx.TOUR_PED_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.TOUR_BUS_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.BALOON_USE_FOR_AUTO_NAMING_REF                 ,
        CIdx.TAXI_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF        ,
        CIdx.MONUMENT_USE_FOR_AUTO_NAMING_REF               ,
        CIdx.BEAUTIFICATION_USE_FOR_AUTO_NAMING_REF         ,
        CIdx.TOURISM_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.NATURAL_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.DISASTER_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.HEALTHCARE_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.FIREDEPARTMENT_USE_FOR_AUTO_NAMING_REF         ,
        CIdx.POLICEDEPARTMENT_USE_FOR_AUTO_NAMING_REF       ,
        CIdx.EDUCATION_USE_FOR_AUTO_NAMING_REF              ,
        CIdx.GARBAGE_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.ROAD_USE_FOR_AUTO_NAMING_REF                   ,
        CIdx.CITIZEN_USE_FOR_AUTO_NAMING_REF                ,
        CIdx.ELECTRICITY_USE_FOR_AUTO_NAMING_REF            ,
        CIdx.WATER_USE_FOR_AUTO_NAMING_REF                  ,

        CIdx.OFFICE_USE_FOR_AUTO_NAMING_REF                 ,
        CIdx.COMMERCIAL_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.INDUSTRIAL_USE_FOR_AUTO_NAMING_REF             ,
        CIdx.RESIDENTIAL_USE_FOR_AUTO_NAMING_REF            ,

        //CIdx.UNUSED2_USE_FOR_AUTO_NAMING_REF                ,
        //CIdx.PARKAREA_USE_FOR_AUTO_NAMING_REF               ,
        //CIdx.DISTRICT_USE_FOR_AUTO_NAMING_REF               ,
        //CIdx.ADDRESS_USE_FOR_AUTO_NAMING_REF                ,
        };

        public static ushort getStationBuilding(uint stopId, ItemClass.SubService ss, bool excludeCargo = false, bool restrictToTransportType = false)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            ushort tempBuildingId;


            if (ss != ItemClass.SubService.None)
            {
                tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                if (IsBuildingValidForStation(excludeCargo, bm, tempBuildingId))
                {
                    return tempBuildingId;
                }
            }
            if (!restrictToTransportType)
            {
                if (nn.m_transportLine > 0)
                {
                    tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, TLMCW.getTransferReasonFromSystemId(TransportSystemDefinition.from(TransportManager.instance.m_lines.m_buffer[nn.m_transportLine].Info).toConfigIndex()), Building.Flags.None, Building.Flags.Untouchable);
                    if (IsBuildingValidForStation(excludeCargo, bm, tempBuildingId))
                    {
                        return tempBuildingId;
                    }
                }


                foreach (var idx in searchOrderStationNamingRule)
                {
                    if (TLMCW.getCurrentConfigBool(idx))
                    {
                        tempBuildingId = TLMUtils.FindBuilding(nn.m_position, 100f, (ItemClass.Service)(idx & CIdx.DESC_DATA), TLMCW.getSubserviceFromSystemId(idx), null, Building.Flags.None, Building.Flags.Untouchable);
                        if (IsBuildingValidForStation(excludeCargo, bm, tempBuildingId))
                        {
                            return tempBuildingId;
                        }
                    }

                }
            }
            return 0;

        }

        private static bool IsBuildingValidForStation(bool excludeCargo, BuildingManager bm, ushort tempBuildingId)
        {
            return tempBuildingId > 0 && (!excludeCargo || !(bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is DepotAI || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is CargoStationAI) || bm.m_buildings.m_buffer[tempBuildingId].Info.GetAI() is TransportStationAI);
        }

        public static string getPrefixesServedString(ushort m_buildingID, bool secondary)
        {
            Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingID];
            DepotAI ai = b.Info.GetAI() as DepotAI;
            if (ai == null)
                return "";
            string[] options = TLMUtils.getStringOptionsForPrefix(TransportSystemDefinition.from(secondary ? ai.m_secondaryTransportInfo : ai.m_transportInfo).toConfigIndex(), true);
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
            for (uint i = 1; i < options.Length; i++)
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
                if (sequenceInit == options.Length - 1)
                {
                    saida.Add(options[(int)sequenceInit]);
                }
                else
                {
                    saida.Add(options[(int)sequenceInit] + "-" + options[(int)(options.Length - 1)]);
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
            //TLMUtils.doLog("getExtensionFromConfigIndex Target TSD: " + tsd + " from idx: " + index);
            return tsd.GetTransportExtension();
        }
        internal static ITLMTransportTypeExtension getExtensionFromTransportSystemDefinition(ref TransportSystemDefinition tsd)
        {
            return tsd.GetTransportExtension();
        }
        public static IAssetSelectorExtension getExtensionFromTransportLine(ushort lineID)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];

            if (t.m_lineNumber != 0 && t.m_stops != 0)
            {
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(lineID))
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

    internal enum NamingType
    {
        NONE,
        PLANE,
        BLIMP,
        SHIP,
        FERRY,
        TRAIN,
        MONORAIL,
        TRAM,
        METRO,
        BUS,
        TOUR_BUS,
        CABLE_CAR,
        MONUMENT,
        BEAUTIFICATION,
        HEALTHCARE,
        POLICEDEPARTMENT,
        FIREDEPARTMENT,
        EDUCATION,
        DISASTER,
        GARBAGE,
        PARKAREA,
        DISTRICT,
        ADDRESS,
        RICO
    }

    internal static class NamingTypeExtensions
    {
        public static int GetNamePrecedenceRate(this NamingType namingType)
        {
            switch (namingType)
            {
                case NamingType.NONE: return 0x7FFFFFFF;
                case NamingType.PLANE: return -0x00000005;
                case NamingType.BLIMP: return 0x00000001;
                case NamingType.SHIP: return -0x00000002;
                case NamingType.FERRY: return 0x00000001;
                case NamingType.TRAIN: return 0x00000003;
                case NamingType.MONORAIL: return 0x00000004;
                case NamingType.TRAM: return 0x00000006;
                case NamingType.METRO: return 0x00000005;
                case NamingType.BUS: return 0x00000007;
                case NamingType.TOUR_BUS: return 0x00000009;
                case NamingType.MONUMENT: return 0x00000005;
                case NamingType.BEAUTIFICATION: return 0x0000000a;
                case NamingType.HEALTHCARE: return 0x0000000b;
                case NamingType.POLICEDEPARTMENT: return 0x0000000b;
                case NamingType.FIREDEPARTMENT: return 0x0000000b;
                case NamingType.EDUCATION: return 0x0000000c;
                case NamingType.DISASTER: return 0x0000000d;
                case NamingType.GARBAGE: return 0x0000000f;
                case NamingType.PARKAREA: return 0x00000005;
                case NamingType.DISTRICT: return 0x00000010;
                case NamingType.ADDRESS: return 0x00000011;
                case NamingType.RICO: return 0x000000e;
                case NamingType.CABLE_CAR: return 0x00000004;
                default: return 0x7FFFFFFF;
            }
        }

        public static NamingType from(ItemClass.Service service, ItemClass.SubService subService)
        {
            return from(GameServiceExtensions.toConfigIndex(service, subService));
        }
        public static NamingType from(TLMCW.ConfigIndex ci)
        {
            switch (ci & (TLMCW.ConfigIndex.SYSTEM_PART | TLMCW.ConfigIndex.DESC_DATA))
            {
                case TLMCW.ConfigIndex.PLANE_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.PLANE;
                case TLMCW.ConfigIndex.BLIMP_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.BLIMP;
                case TLMCW.ConfigIndex.SHIP_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.SHIP;
                case TLMCW.ConfigIndex.FERRY_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.FERRY;
                case TLMCW.ConfigIndex.TRAIN_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.TRAIN;
                case TLMCW.ConfigIndex.MONORAIL_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.MONORAIL;
                case TLMCW.ConfigIndex.TRAM_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.TRAM;
                case TLMCW.ConfigIndex.METRO_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.METRO;
                case TLMCW.ConfigIndex.BUS_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.BUS;
                case TLMCW.ConfigIndex.TOUR_BUS_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.TOUR_BUS;
                case TLMCW.ConfigIndex.CABLE_CAR_CONFIG | TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG: return NamingType.CABLE_CAR;
                case TLMCW.ConfigIndex.MONUMENT_SERVICE_CONFIG: return NamingType.MONUMENT;
                case TLMCW.ConfigIndex.BEAUTIFICATION_SERVICE_CONFIG: return NamingType.BEAUTIFICATION;
                case TLMCW.ConfigIndex.HEALTHCARE_SERVICE_CONFIG: return NamingType.HEALTHCARE;
                case TLMCW.ConfigIndex.POLICEDEPARTMENT_SERVICE_CONFIG: return NamingType.POLICEDEPARTMENT;
                case TLMCW.ConfigIndex.FIREDEPARTMENT_SERVICE_CONFIG: return NamingType.FIREDEPARTMENT;
                case TLMCW.ConfigIndex.EDUCATION_SERVICE_CONFIG: return NamingType.EDUCATION;
                case TLMCW.ConfigIndex.DISASTER_SERVICE_CONFIG: return NamingType.DISASTER;
                case TLMCW.ConfigIndex.GARBAGE_SERVICE_CONFIG: return NamingType.GARBAGE;
                case TLMCW.ConfigIndex.PARKAREA_NAME_CONFIG: return NamingType.PARKAREA;
                case TLMCW.ConfigIndex.DISTRICT_NAME_CONFIG: return NamingType.DISTRICT;
                case TLMCW.ConfigIndex.ADDRESS_NAME_CONFIG: return NamingType.ADDRESS;
                case TLMCW.ConfigIndex.RESIDENTIAL_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.INDUSTRIAL_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.COMMERCIAL_SERVICE_CONFIG:
                case TLMCW.ConfigIndex.OFFICE_SERVICE_CONFIG: return NamingType.RICO;
                default: TLMUtils.doErrorLog($"UNKNOWN NAME TYPE:{ci} ({((int)ci).ToString("X8")})"); return NamingType.NONE;

            }
        }
    }

}
