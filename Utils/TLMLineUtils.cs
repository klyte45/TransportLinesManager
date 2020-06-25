using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Redirectors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    public class TLMLineUtils
    {
        public static Vehicle GetVehicleCapacityAndFill(ushort vehicleID, Vehicle vehicleData, out int fill, out int cap)
        {
            ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].GetFirstVehicle(vehicleID);
            vehicleData.Info.m_vehicleAI.GetBufferStatus(firstVehicle, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle], out string text, out fill, out cap);
            return vehicleData;
        }

        public static void GetQuantityPassengerWaiting(ushort currentStop, out int residents, out int tourists, out int timeTilBored)
        {
            ushort nextStop = TransportLine.GetNextStop(currentStop);
            CitizenManager cm = Singleton<CitizenManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            Vector3 position = nm.m_nodes.m_buffer[currentStop].m_position;
            Vector3 position2 = nm.m_nodes.m_buffer[nextStop].m_position;
            nm.m_nodes.m_buffer[currentStop].m_maxWaitTime = 0;
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
                        ushort nextGridInstance = cm.m_instances.m_buffer[citizenIterator].m_nextGridInstance;
                        if ((cm.m_instances.m_buffer[citizenIterator].m_flags & CitizenInstance.Flags.WaitingTransport) != CitizenInstance.Flags.None)
                        {
                            Vector3 a = cm.m_instances.m_buffer[citizenIterator].m_targetPos;
                            float distance = Vector3.SqrMagnitude(a - position);
                            if (distance < 1024f)
                            {
                                CitizenInfo info = cm.m_instances.m_buffer[citizenIterator].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(citizenIterator, ref cm.m_instances.m_buffer[citizenIterator], position, position2))
                                {
                                    if ((cm.m_citizens.m_buffer[citizenIterator].m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None)
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
            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num2 = line.m_vehicles;
            int num3 = 0;
            while (num2 != 0)
            {
                ushort nextLineVehicle = instance.m_vehicles.m_buffer[num2].m_nextLineVehicle;
                VehicleInfo info2 = instance.m_vehicles.m_buffer[num2].Info;
                info2.m_vehicleAI.SetTransportLine(num2, ref instance.m_vehicles.m_buffer[num2], 0);
                num2 = nextLineVehicle;
                if (++num3 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }



        public static float GetEffectiveBudget(ushort transportLine) => GetEffectiveBudgetInt(transportLine) / 100f;
        public static int GetEffectiveBudgetInt(ushort transportLine)
        {
            TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].Info;
            Tuple<float, int, int, float, bool> lineBudget = GetBudgetMultiplierLineWithIndexes(transportLine);
            int budgetClass = lineBudget.Fifth ? 100 : Singleton<EconomyManager>.instance.GetBudget(info.m_class);
            return (int)(budgetClass * lineBudget.First);
        }

        public static float GetBudgetMultiplierLine(ushort lineId) => GetBudgetMultiplierLineWithIndexes(lineId).First;

        //public static bool GetConfigForLine(ushort lineId, out TransportLineConfiguration lineConfig, out PrefixConfiguration prefixConfig)
        //{
        //    lineConfig = TLMTransportLineExtension.Instance.SafeGet(lineId);
        //    var tsd = TransportSystemDefinition.From(lineId);
        //    prefixConfig = (tsd.GetTransportExtension() as ISafeGettable<PrefixConfiguration>).SafeGet(hasPrefix(ref tsd) ? getPrefix(lineId) : 0);
        //    return lineConfig != null || prefixConfig != null;
        //}

        public static IBasicExtensionStorage GetEffectiveConfigForLine(ushort lineId)
        {
            if (TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineId))
            {
                return TLMTransportLineExtension.Instance.SafeGet(lineId);
            }
            else
            {
                var tsd = TransportSystemDefinition.From(lineId);
                return (tsd.GetTransportExtension() as ISafeGettable<TLMPrefixConfiguration>).SafeGet(hasPrefix(ref tsd) ? getPrefix(lineId) : 0);
            }
        }
        public static IBasicExtension GetEffectiveExtensionForLine(ushort lineId)
        {
            if (TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineId))
            {
                return TLMTransportLineExtension.Instance;
            }
            else
            {
                var tsd = TransportSystemDefinition.From(lineId);
                return tsd.GetTransportExtension();
            }
        }

        public static Tuple<float, int, int, float, bool> GetBudgetMultiplierLineWithIndexes(ushort lineId)
        {
            IBasicExtensionStorage currentConfig = GetEffectiveConfigForLine(lineId);
            TimeableList<BudgetEntryXml> budgetConfig = currentConfig.BudgetEntries;
            if (budgetConfig.Count == 0)
            {
                GetEffectiveExtensionForLine(lineId).SetBudgetMultiplierForLine(lineId, Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_budget, 0);
                currentConfig = GetEffectiveConfigForLine(lineId);
                budgetConfig = currentConfig.BudgetEntries;
            }
            Tuple<Tuple<BudgetEntryXml, int>, Tuple<BudgetEntryXml, int>, float> currentBudget = budgetConfig.GetAtHour(Singleton<SimulationManager>.instance.m_currentDayTimeHour);
            return Tuple.New(Mathf.Lerp(currentBudget.First.First.Value, currentBudget.Second.First.Value, currentBudget.Third) / 100f, currentBudget.First.Second, currentBudget.Second.Second, currentBudget.Third, currentConfig is TLMTransportLineConfiguration);

        }
        public static string getLineStringId(ushort lineIdx)
        {
            getLineNamingParameters(lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix);
            return TLMUtils.GetString(prefix, s, suffix, nonPrefix, Singleton<TransportManager>.instance.m_lines.m_buffer[lineIdx].m_lineNumber, zeros, invertPrefixSuffix);
        }

        public static void getLineNamingParameters(ushort lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix) => getLineNamingParameters(lineIdx, out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix, out string nil);
        public static int GetStopsCount(ushort lineID) => Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountStops(lineID);

        public static int GetVehiclesCount(ushort lineID) => Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountVehicles(lineID);

        public static float GetLineLength(ushort lineID)
        {
            //float totalSize = 0f;
            //for (int i = 0; i < Singleton<TransportManager>.instance.m_lineCurves[(int) lineID].Length; i++) {
            //    Bezier3 bez = Singleton<TransportManager>.instance.m_lineCurves[(int) lineID][i];
            //    totalSize += TLMUtils.calcBezierLenght(bez.a, bez.b, bez.c, bez.d, 0.1f);
            //}
            return Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_totalLength;
        }

        public static TransportSystemDefinition getLineNamingParameters(ushort lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, out string icon)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(lineIdx);
            if (tsd != default)
            {
                GetNamingRulesFromTSD(out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix, ref tsd);
            }
            else
            {
                suffix = default;
                s = default;
                prefix = default;
                nonPrefix = default;
                zeros = false;
                invertPrefixSuffix = false;
            }
            icon = getIconForLine(lineIdx);
            return tsd;
        }

        public static bool IsLineNumberAlredyInUse(int numLinha, ref TransportSystemDefinition tsdOr, int exclude)
        {
            numLinha = numLinha & 0xFFFF;
            if (numLinha == 0)
            {
                return true;
            }

            TLMUtils.doLog("tsdOr = " + tsdOr + " | lineNum =" + numLinha + "| cfgIdx = " + tsdOr.ToConfigIndex());
            var tipo = tsdOr.ToConfigIndex();

            for (ushort i = 1; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
                {
                    continue;
                }
                ushort lnum = Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber;
                var tsd = TransportSystemDefinition.GetDefinitionForLine(i);
                TLMUtils.doLog("tsd = " + tsd + "| lineNum = " + lnum + "| I=" + i + "| cfgIdx = " + tsd.ToConfigIndex());
                if (tsd != default && i != exclude && tsd.ToConfigIndex() == tipo && lnum == numLinha)
                {
                    return true;
                }
            }
            return false;
        }
        public static void GetNamingRulesFromTSD(out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, ref TransportSystemDefinition tsd)

        {
            var transportType = tsd.ToConfigIndex();
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
                suffix = (ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.SUFFIX);
                s = (Separador)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.SEPARATOR);
                prefix = (ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);
                nonPrefix = (ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.NON_PREFIX);
                zeros = TLMCW.GetCurrentConfigBool(transportType | TLMCW.ConfigIndex.LEADING_ZEROS);
                invertPrefixSuffix = TLMCW.GetCurrentConfigBool(transportType | TLMCW.ConfigIndex.INVERT_PREFIX_SUFFIX);
            }
        }
        public static bool hasPrefix(ref TransportSystemDefinition tsd)
        {
            if (tsd == default)
            {
                return false;
            }
            var transportType = tsd.ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(ref TransportLine t)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(ref t);
            if (tsd == default)
            {
                return false;
            }
            var transportType = tsd.ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(ushort idx)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(idx);
            if (tsd == default)
            {
                return false;
            }
            var transportType = tsd.ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static bool hasPrefix(TransportInfo t)
        {
            var transportType = TransportSystemDefinition.From(t).ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }


        public static uint getPrefix(ushort idx)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(idx);
            if (tsd == default)
            {
                return 0;
            }
            var transportType = tsd.ToConfigIndex();
            if (transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((ModoNomenclatura)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum)
            {
                uint prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[idx].m_lineNumber / 1000u;
                //LogUtils.DoLog($"Prefix {prefix} for lineId {idx}");
                return prefix;
            }
            else
            {
                //LogUtils.DoLog($"Prefix 0 (def) for lineId {idx}");
                return 0;
            }
        }

        public static string getIconForLine(ushort lineIdx, bool noBorder = true)
        {
            TLMCW.ConfigIndex transportType;
            var tsd = TransportSystemDefinition.GetDefinitionForLine(lineIdx);
            transportType = tsd.ToConfigIndex();
            //if (tsd != default)
            //{
            //    transportType = tsd.toConfigIndex();
            //}
            //else
            //{
            //    transportType = TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG;
            //}
            return KlyteResourceLoader.GetDefaultSpriteNameFor(TLMUtils.GetLineIcon(TransportManager.instance.m_lines.m_buffer[lineIdx].m_lineNumber, transportType, ref tsd), noBorder);
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
                        NetInfo info = nm.m_nodes.m_buffer[num6].Info;
                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport))
                        {
                            ushort transportLine = nm.m_nodes.m_buffer[num6].m_transportLine;
                            var tsd = TransportSystemDefinition.GetDefinitionForLine(transportLine);
                            if (transportLine != 0 && tsd != default && TLMCW.GetCurrentConfigBool(tsd.ToConfigIndex() | TLMConfigWarehouse.ConfigIndex.SHOW_IN_LINEAR_MAP))
                            {
                                TransportInfo info2 = tm.m_lines.m_buffer[transportLine].Info;
                                if (!linesFound.Contains(transportLine) && (tm.m_lines.m_buffer[transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[num6].m_position);
                                    if (num8 < maxDistance * maxDistance || (info2.m_transportType == TransportInfo.TransportType.Ship && num8 < extendedMaxDistance * extendedMaxDistance))
                                    {
                                        linesFound.Add(transportLine);
                                        GetNearLines(nm.m_nodes.m_buffer[num6].m_position, maxDistance, ref linesFound);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        num6 = nm.m_nodes.m_buffer[num6].m_nextGridNode;
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
            {
                return false;
            }

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
                        NetInfo info = nm.m_nodes.m_buffer[stopId].Info;

                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport) && subservicesAllowed.Contains(info.m_class.m_subService))
                        {
                            ushort transportLine = nm.m_nodes.m_buffer[stopId].m_transportLine;
                            if (transportLine != 0)
                            {
                                if (!stopsFound.Keys.Contains(stopId))
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[stopId].m_position);
                                    if (num8 < maxDistance * maxDistance)
                                    {
                                        stopsFound[stopId] = nm.m_nodes.m_buffer[stopId].m_position;
                                        GetNearStopPoints(nm.m_nodes.m_buffer[stopId].m_position, maxDistance, ref stopsFound, subservicesAllowed, maxDepht, depth + 1);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        stopId = nm.m_nodes.m_buffer[stopId].m_nextGridNode;
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
        public static Dictionary<string, ushort> SortLines(List<ushort> intersections, TransportLine t = default)
        {
            TransportManager tm = Singleton<TransportManager>.instance;
            var otherLinesIntersections = new Dictionary<string, ushort>();
            foreach (ushort s in intersections)
            {
                TransportLine tl = tm.m_lines.m_buffer[s];
                if (t.Equals(default(TransportLine)) || tl.Info.GetSubService() != t.Info.GetSubService() || tl.m_lineNumber != t.m_lineNumber)
                {
                    var sortString = GetLineSortString(s, ref tl);
                    if (sortString != null)
                    {
                        otherLinesIntersections.Add(sortString, s);
                    }
                }
            }
            return otherLinesIntersections;
        }

        public static string GetLineSortString(ushort s, ref TransportLine tl)
        {
            string transportTypeLetter = "";
            var tsd = TransportSystemDefinition.GetDefinitionForLine(s);
            if (tsd == default)
            {
                return null;
            }
            switch (tsd.ToConfigIndex())
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
                case TLMConfigWarehouse.ConfigIndex.HELICOPTER_CONFIG:
                    transportTypeLetter = "D";
                    break;
                case TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG:
                    transportTypeLetter = "E";
                    break;
                case TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG:
                    transportTypeLetter = "F";
                    break;
                case TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG:
                    transportTypeLetter = "G";
                    break;
                case TLMConfigWarehouse.ConfigIndex.METRO_CONFIG:
                    transportTypeLetter = "H";
                    break;
                case TLMConfigWarehouse.ConfigIndex.CABLE_CAR_CONFIG:
                    transportTypeLetter = "I";
                    break;
                case TLMConfigWarehouse.ConfigIndex.TROLLEY_CONFIG:
                    transportTypeLetter = "J";
                    break;
                case TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG:
                    transportTypeLetter = "K";
                    break;
                case TLMConfigWarehouse.ConfigIndex.BUS_CONFIG:
                    transportTypeLetter = "L";
                    break;
                case TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG:
                    transportTypeLetter = "M";
                    break;
                case TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG:
                    transportTypeLetter = "N";
                    break;
            }
            return transportTypeLetter + tl.m_lineNumber.ToString().PadLeft(5, '0');
        }


        public static void PrintIntersections(string airport, string harbor, string taxi, string regionalTrainStation, string cableCarStation, UIPanel intersectionsPanel, Dictionary<string, ushort> otherLinesIntersections, Vector3 position, float scale = 1.0f, int maxItemsForSizeSwap = 3)
        {
            TransportManager tm = Singleton<TransportManager>.instance;

            int intersectionCount = otherLinesIntersections.Count;
            if (!string.IsNullOrEmpty(airport))
            {
                intersectionCount++;
            }
            if (!string.IsNullOrEmpty(harbor))
            {
                intersectionCount++;
            }
            if (!string.IsNullOrEmpty(taxi))
            {
                intersectionCount++;
            }
            if (!string.IsNullOrEmpty(regionalTrainStation))
            {
                intersectionCount++;
            }
            if (!string.IsNullOrEmpty(cableCarStation))
            {
                intersectionCount++;
            }
            float size = scale * (intersectionCount > maxItemsForSizeSwap ? 20 : 40);
            float multiplier = scale * (intersectionCount > maxItemsForSizeSwap ? 0.4f : 0.8f);
            foreach (KeyValuePair<string, ushort> s in otherLinesIntersections.OrderBy(x => x.Key))
            {
                TransportLine intersectLine = tm.m_lines.m_buffer[s.Value];
                ItemClass.SubService ss = getLineNamingParameters(s.Value, out ModoNomenclatura prefixo, out Separador separador, out ModoNomenclatura sufixo, out ModoNomenclatura naoPrefixado, out bool zeros, out bool invertPrefixSuffix, out string bgSprite).SubService;
                KlyteMonoUtils.CreateUIElement(out UIButtonLineInfo lineCircleIntersect, intersectionsPanel.transform);
                lineCircleIntersect.autoSize = false;
                lineCircleIntersect.width = size;
                lineCircleIntersect.height = size;
                lineCircleIntersect.color = intersectLine.m_color;
                lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
                lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
                lineCircleIntersect.name = "LineFormat";
                lineCircleIntersect.relativePosition = new Vector3(0f, 0f);
                lineCircleIntersect.normalBgSprite = bgSprite;
                lineCircleIntersect.hoveredColor = Color.white;
                lineCircleIntersect.hoveredTextColor = Color.red;
                lineCircleIntersect.lineID = s.Value;
                lineCircleIntersect.tooltip = tm.GetLineName(s.Value);
                lineCircleIntersect.eventClick += (x, y) =>
                {
                    InstanceID iid = InstanceID.Empty;
                    iid.TransportLine = s.Value;
                    WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);

                };
                KlyteMonoUtils.CreateUIElement(out UILabel lineNumberIntersect, lineCircleIntersect.transform);
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
                    KlyteMonoUtils.CreateUIElement(out UILabel daytimeIndicator, lineCircleIntersect.transform);
                    daytimeIndicator.autoSize = false;
                    daytimeIndicator.width = size;
                    daytimeIndicator.height = size;
                    daytimeIndicator.color = Color.white;
                    daytimeIndicator.pivot = UIPivotPoint.MiddleLeft;
                    daytimeIndicator.verticalAlignment = UIVerticalAlignment.Middle;
                    daytimeIndicator.name = "LineTime";
                    daytimeIndicator.relativePosition = new Vector3(0f, 0f);
                    /*TODO: !!!!!! */
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

        public static void setLineActive(ref TransportLine t, bool day, bool night) => t.SetActive(day, night);

        private static void addExtraStationBuildingIntersection(UIComponent parent, float size, string bgSprite, string description)
        {
            KlyteMonoUtils.CreateUIElement(out UILabel lineCircleIntersect, parent.transform);
            lineCircleIntersect.autoSize = false;
            lineCircleIntersect.width = size;
            lineCircleIntersect.height = size;
            lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
            lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
            lineCircleIntersect.name = "LineFormat";
            lineCircleIntersect.relativePosition = new Vector3(0f, 0f);
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
            textColor = TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineID) ? Color.yellow : Color.white;
        }

        public static int getVehicleCapacity(ushort vehicleId)
        {
            PrefabAI ai = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].Info.GetAI();
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
            ItemClass.SubService ss = TransportSystemDefinition.GetDefinitionForLine(lineID).SubService;
            for (int i = 0; i < stopsCount; i++)
            {
                ushort stationId = t.GetStop(i);
                result[i] = TLMStationUtils.GetFullStationName(stationId, lineID, ss);
            }
            return result;
        }


        #region Line Utils
        public static AsyncTask<bool> SetLineColor(ushort lineIdx, Color color) => Singleton<SimulationManager>.instance.AddAction<bool>(TransportManager.instance.SetLineColor(lineIdx, color));
        public static AsyncTask<bool> SetLineName(ushort lineIdx, string name) => Singleton<SimulationManager>.instance.AddAction<bool>(TransportManager.instance.SetLineName(lineIdx, name));

        private static TransportInfo.TransportType[] m_roadTransportTypes = new TransportInfo.TransportType[] { TransportInfo.TransportType.Bus, TransportInfo.TransportType.Tram };

        public static string CalculateAutoName(ushort lineIdx, out ushort startStation, out ushort endStation, out string startStationStr, out string endStationStr)
        {
            ref TransportLine t = ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineIdx];
            if ((t.m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
            {
                startStation = 0;
                endStation = 0;
                startStationStr = null;
                endStationStr = null;
                return null;
            }
            ushort nextStop = t.m_stops;
            bool allowPrefixInStations = m_roadTransportTypes.Contains(t.Info.m_transportType);
            var stations = new List<Tuple<NamingType, string, ushort>>();
            do
            {
                NetNode stopNode = NetManager.instance.m_nodes.m_buffer[nextStop];
                string stationName = TLMStationUtils.GetStationName(nextStop, lineIdx, t.Info.m_class.m_subService, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefixFound, out ushort buildingId, out NamingType namingType, true, true);
                var tuple = Tuple.New(namingType, allowPrefixInStations ? $"{prefixFound?.Trim()} {stationName?.Trim()}".Trim() : stationName, nextStop);
                stations.Add(tuple);
                nextStop = TransportLine.GetNextStop(nextStop);
            } while (nextStop != t.m_stops && nextStop != 0);
            string prefix = "";
            if (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME))
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
                        Tuple<NamingType, string> A = stations[(middle + i) % stations.Count];
                        Tuple<NamingType, string> B = stations[(middle - i) % stations.Count];
                        if (A.First != B.First || A.Second != B.Second)
                        {
                            simmetric = false;
                            break;
                        }
                    }
                    if (simmetric)
                    {
                        startStation = stations[middle % stations.Count].Third;
                        endStation = stations[(middle + (stations.Count / 2)) % stations.Count].Third;

                        startStationStr = stations[middle % stations.Count].Second;
                        endStationStr = stations[(middle + stations.Count / 2) % stations.Count].Second;

                        if (startStationStr == endStationStr)
                        {
                            startStationStr = (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + startStationStr;
                            endStationStr = startStationStr;
                            return $"{prefix}{startStationStr}";
                        }
                        else
                        {
                            return $"{prefix}{startStationStr} - {endStationStr}";
                        }
                    }
                }
            }
            var idxStations = stations.Select((x, y) => Tuple.New(y, x.First, x.Second, x.Third)).OrderBy(x => x.Second.GetNamePrecedenceRate()).ToList();

            int targetStart = 0;
            int mostRelevantEndIdx = -1;
            int j = 0;
            int maxDistanceEnd = (int)(idxStations.Count / 8f + 0.5f);
            TLMUtils.doLog("idxStations");
            do
            {
                Tuple<int, NamingType, string> peerCandidate = idxStations.Where(x => x.Third != idxStations[j].Third && Math.Abs((x.First < idxStations[j].First ? x.First + idxStations.Count : x.First) - idxStations.Count / 2 - idxStations[j].First) <= maxDistanceEnd).OrderBy(x => x.Second.GetNamePrecedenceRate()).FirstOrDefault();
                if (peerCandidate != null && (mostRelevantEndIdx == -1 || stations[mostRelevantEndIdx].First.GetNamePrecedenceRate() > peerCandidate.Second.GetNamePrecedenceRate()))
                {
                    targetStart = j;
                    mostRelevantEndIdx = peerCandidate.First;
                }
                j++;
            } while (j < idxStations.Count && idxStations[j].Second.GetNamePrecedenceRate() == idxStations[0].Second.GetNamePrecedenceRate());

            if (mostRelevantEndIdx >= 0)
            {
                startStation = idxStations[targetStart].Fourth;
                endStation = stations[mostRelevantEndIdx].Third;
                startStationStr = idxStations[targetStart].Third;
                endStationStr = stations[mostRelevantEndIdx].Second;
                if (startStationStr == endStationStr)
                {
                    startStationStr = (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + startStationStr;
                    endStationStr = startStationStr;
                    return $"{prefix}{startStationStr}";
                }
                else
                {
                    return $"{prefix}{startStationStr} - {endStationStr}";
                }
            }
            else
            {
                startStation = idxStations[0].Fourth;
                endStation = 0;
                startStationStr = (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + idxStations[0].Third;
                endStationStr = null;
                return prefix + startStationStr;
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
                NetNode nn1 = nm.m_nodes.m_buffer[t.GetStop(offsetL)];
                NetNode nn2 = nm.m_nodes.m_buffer[t.GetStop(offsetH)];
                ushort buildingId1 = BuildingUtils.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                ushort buildingId2 = BuildingUtils.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
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
                    NetNode nn1 = nm.m_nodes.m_buffer[t.GetStop(offsetL)];
                    NetNode nn2 = nm.m_nodes.m_buffer[t.GetStop(offsetH)];
                    ushort buildingId1 = BuildingUtils.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
                    ushort buildingId2 = BuildingUtils.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, TLMUtils.defaultAllowedVehicleTypes, Building.Flags.None, Building.Flags.Untouchable);
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


        public static Tuple<string, Color, string> GetIconStringParameters(ushort lineId) => Tuple.New(getIconForLine(lineId, false), Singleton<TransportManager>.instance.GetLineColor(lineId), getLineStringId(lineId));
        internal static string GetIconString(ushort lineId) => GetIconString(getIconForLine(lineId, false), Singleton<TransportManager>.instance.GetLineColor(lineId), getLineStringId(lineId));
        internal static string GetIconString(string iconName, Color color, string lineString) => $"<{UIDynamicFontRendererRedirector.TAG_LINE} {iconName},{color.ToRGB()},{lineString}>";



        public static int CalculateTargetVehicleCount(ref TransportLine t, ushort lineId, float lineLength) => CalculateTargetVehicleCount(t.Info, lineLength, GetEffectiveBudget(lineId));
        public static int CalculateTargetVehicleCount(TransportInfo info, float lineLength, float budget) => Mathf.CeilToInt(budget * lineLength / info.m_defaultVehicleDistance);
        public static float CalculateBudgetForEachVehicle(TransportInfo info, float lineLength) => info.m_defaultVehicleDistance / lineLength;

        public static int GetTicketPriceForVehicle(VehicleAI ai, ushort vehicleID, ref Vehicle vehicleData)
        {
            var def = TransportSystemDefinition.From(vehicleData.Info);

            if (def == default)
            {
                LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}):DEFAULT TSD FOR {ai}");
                return ai.GetTicketPrice(vehicleID, ref vehicleData);
            }

            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(vehicleData.m_targetPos3);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Event @event = instance.m_districts.m_buffer[district].m_eventPolicies & Singleton<EventManager>.instance.GetEventPolicyMask();
            float multiplier;
            if (vehicleData.Info.m_class.m_subService == ItemClass.SubService.PublicTransportTours)
            {
                multiplier = 1;
            }
            else
            {
                if ((servicePolicies & DistrictPolicies.Services.FreeTransport) != DistrictPolicies.Services.None)
                {
                    LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): FreeTransport at district!");
                    return 0;
                }
                if ((@event & DistrictPolicies.Event.ComeOneComeAll) != DistrictPolicies.Event.None)
                {
                    LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): ComeOneComeAll at district!");
                    return 0;
                }
                if ((servicePolicies & DistrictPolicies.Services.HighTicketPrices) != DistrictPolicies.Services.None)
                {
                    LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): HighTicketPrices at district!");
                    instance.m_districts.m_buffer[district].m_servicePoliciesEffect = (instance.m_districts.m_buffer[district].m_servicePoliciesEffect | DistrictPolicies.Services.HighTicketPrices);
                    multiplier = 5f / 4f;
                }
                else
                {
                    multiplier = 1;
                }
            }
            uint ticketPriceDefault = GetTicketPriceForLine(ref def, vehicleData.m_transportLine).First.Value;
            LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): multiplier = {multiplier}, ticketPriceDefault = {ticketPriceDefault}");

            return (int)(multiplier * ticketPriceDefault);

        }

        public static Tuple<TicketPriceEntryXml, int> GetTicketPriceForLine(ref TransportSystemDefinition tsd, ushort lineId) => GetTicketPriceForLine(ref tsd, lineId, SimulationManager.instance.m_currentDayTimeHour);
        public static Tuple<TicketPriceEntryXml, int> GetTicketPriceForLine(ref TransportSystemDefinition tsd, ushort lineId, float hour)
        {
            Tuple<TicketPriceEntryXml, int> ticketPriceDefault = null;
            if (lineId > 0)
            {
                if (TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineId))
                {
                    ticketPriceDefault = TLMTransportLineExtension.Instance.GetTicketPriceForHourForLine(lineId, hour);
                }
                if ((ticketPriceDefault?.First?.Value ?? 0) == 0)
                {
                    ticketPriceDefault = tsd.GetTransportExtension().GetTicketPriceForHourForLine(lineId, hour);
                }

            }
            if ((ticketPriceDefault?.First?.Value ?? 0) == 0)
            {
                ticketPriceDefault = Tuple.New(new TicketPriceEntryXml() { Value = (uint)TLMCW.GetSettedTicketPrice(tsd.ToConfigIndex()) }, -1);
            }
            if ((ticketPriceDefault?.First?.Value ?? 0) == 0)
            {
                ticketPriceDefault = Tuple.New(new TicketPriceEntryXml() { Value = (uint)TransportManager.instance.m_lines.m_buffer[lineId].Info.m_ticketPrice }, -1);
            }

            return ticketPriceDefault;
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
