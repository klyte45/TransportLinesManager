using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    public class TLMLineUtils
    {
        public static Vehicle GetVehicleCapacityAndFill(ushort vehicleID, Vehicle vehicleData, out int fill, out int cap)
        {
            ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int) vehicleID].GetFirstVehicle(vehicleID);
            string text;
            vehicleData.Info.m_vehicleAI.GetBufferStatus(firstVehicle, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int) firstVehicle], out text, out fill, out cap);
            return vehicleData;
        }

        public static void GetQuantityPassengerWaiting(ushort currentStop, out int residents, out int tourists)
        {
            ushort nextStop = TransportLine.GetNextStop(currentStop);
            CitizenManager cm = Singleton<CitizenManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            Vector3 position = nm.m_nodes.m_buffer[(int) currentStop].m_position;
            Vector3 position2 = nm.m_nodes.m_buffer[(int) nextStop].m_position;
            nm.m_nodes.m_buffer[(int) currentStop].m_maxWaitTime = 0;
            int minX = Mathf.Max((int) ((position.x - 32f) / 8f + 1080f), 0);
            int minZ = Mathf.Max((int) ((position.z - 32f) / 8f + 1080f), 0);
            int maxX = Mathf.Min((int) ((position.x + 32f) / 8f + 1080f), 2159);
            int maxZ = Mathf.Min((int) ((position.z + 32f) / 8f + 1080f), 2159);
            residents = 0;
            tourists = 0;
            int zIterator = minZ;
            while (zIterator <= maxZ) {
                int xIterator = minX;
                while (xIterator <= maxX) {
                    ushort citizenIterator = cm.m_citizenGrid[zIterator * 2160 + xIterator];
                    int loopCounter = 0;
                    while (citizenIterator != 0) {
                        ushort nextGridInstance = cm.m_instances.m_buffer[(int) citizenIterator].m_nextGridInstance;
                        if ((cm.m_instances.m_buffer[(int) citizenIterator].m_flags & CitizenInstance.Flags.WaitingTransport) != CitizenInstance.Flags.None) {
                            Vector3 a = cm.m_instances.m_buffer[(int) citizenIterator].m_targetPos;
                            float distance = Vector3.SqrMagnitude(a - position);
                            if (distance < 1024f) {
                                CitizenInfo info = cm.m_instances.m_buffer[(int) citizenIterator].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(citizenIterator, ref cm.m_instances.m_buffer[(int) citizenIterator], position, position2)) {
                                    if ((cm.m_citizens.m_buffer[(int) (citizenIterator)].m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None) {
                                        tourists++;
                                    } else {
                                        residents++;
                                    }
                                }
                            }
                        }
                        citizenIterator = nextGridInstance;
                        if (++loopCounter > 65536) {
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
            var line = Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineId];
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num2 = line.m_vehicles;
            int num3 = 0;
            while (num2 != 0) {
                ushort nextLineVehicle = instance.m_vehicles.m_buffer[(int) num2].m_nextLineVehicle;
                VehicleInfo info2 = instance.m_vehicles.m_buffer[(int) num2].Info;
                info2.m_vehicleAI.SetTransportLine(num2, ref instance.m_vehicles.m_buffer[(int) num2], 0);
                num2 = nextLineVehicle;
                if (++num3 > 16384) {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public static float getEffectiveBugdet(ushort transportLine)
        {
            int num2 = Singleton<EconomyManager>.instance.GetBudget(Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].Info.m_class);
            int budget = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].m_budget;
            return (num2 * budget) / 10000f;
        }

        public static string getLineStringId(ushort lineIdx)
        {
            ModoNomenclatura prefix;
            Separador s;
            ModoNomenclatura suffix;
            ModoNomenclatura nonPrefix;
            bool zeros;
            bool invertPrefixSuffix;
            getLineNamingParameters(lineIdx, out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix);
            return TLMUtils.getString(prefix, s, suffix, nonPrefix, Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineIdx].m_lineNumber, zeros, invertPrefixSuffix);
        }

        public static void getLineNamingParameters(ushort lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix)
        {
            string nil;
            getLineNamingParameters(lineIdx, out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix, out nil);

        }
        public static int GetStopsCount(ushort lineID)
        {
            return Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].CountStops(lineID);
        }

        public static int GetVehiclesCount(ushort lineID)
        {
            return Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].CountVehicles(lineID);
        }

        public static float GetLineLength(ushort lineID)
        {
            float totalSize = 0f;
            for (int i = 0; i < Singleton<TransportManager>.instance.m_lineCurves[(int) lineID].Length; i++) {
                Bezier3 bez = Singleton<TransportManager>.instance.m_lineCurves[(int) lineID][i];
                totalSize += TLMUtils.calcBezierLenght(bez.a, bez.b, bez.c, bez.d, 0.1f);
            }
            return totalSize;
        }

        public static TransportSystemDefinition getLineNamingParameters(ushort lineIdx, out ModoNomenclatura prefix, out Separador s, out ModoNomenclatura suffix, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, out string icon)
        {
            var tsd = TLMCW.getDefinitionForLine(lineIdx);
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();

            suffix = (ModoNomenclatura) TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.SUFFIX);
            s = (Separador) TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.SEPARATOR);
            prefix = (ModoNomenclatura) TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);
            nonPrefix = (ModoNomenclatura) TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.NON_PREFIX);
            zeros = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.LEADING_ZEROS);
            invertPrefixSuffix = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.INVERT_PREFIX_SUFFIX);
            icon = getIconForLine(lineIdx);
            return tsd;
        }

        public static bool hasPrefix(TransportLine t)
        {
            var tsd = TLMCW.getDefinitionForLine(t);
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            return ((ModoNomenclatura) TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != ModoNomenclatura.Nenhum;
        }

        public static string getIconForLine(ushort lineIdx)
        {
            var tsd = TLMCW.getDefinitionForLine(lineIdx);
            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            switch (transportType) {
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
            int num = Mathf.Max((int) ((pos.x - extendedMaxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int) ((pos.z - extendedMaxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int) ((pos.x + extendedMaxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int) ((pos.z + extendedMaxDistance) / 64f + 135f), 269);
            bool noneFound = true;
            NetManager nm = Singleton<NetManager>.instance;
            TransportManager tm = Singleton<TransportManager>.instance;
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort num6 = nm.m_nodeGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0) {
                        NetInfo info = nm.m_nodes.m_buffer[(int) num6].Info;
                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport)) {
                            ushort transportLine = nm.m_nodes.m_buffer[(int) num6].m_transportLine;
                            TransportSystemDefinition tsd = TLMCW.getDefinitionForLine(transportLine);
                            if (transportLine != 0 && tsd != default(TransportSystemDefinition) && TLMCW.getCurrentConfigBool(tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.SHOW_IN_LINEAR_MAP)) {
                                TransportInfo info2 = tm.m_lines.m_buffer[(int) transportLine].Info;
                                if (!linesFound.Contains(transportLine) && (tm.m_lines.m_buffer[(int) transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None) {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[(int) num6].m_position);
                                    if (num8 < maxDistance * maxDistance || (info2.m_transportType == TransportInfo.TransportType.Ship && num8 < extendedMaxDistance * extendedMaxDistance)) {
                                        linesFound.Add(transportLine);
                                        GetNearLines(nm.m_nodes.m_buffer[(int) num6].m_position, maxDistance, ref linesFound);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        num6 = nm.m_nodes.m_buffer[(int) num6].m_nextGridNode;
                        if (++num7 >= 32768) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return noneFound;
        }



        //GetNearStopPoints
        public static bool GetNearStopPoints(Vector3 pos, float maxDistance, ref List<ushort> stopsFound, ItemClass.SubService[] subservicesAllowed = null, int maxDepht = 4, int depth = 0)
        {
            if (depth >= maxDepht)
                return false;
            if (subservicesAllowed == null) {
                subservicesAllowed = new ItemClass.SubService[] { ItemClass.SubService.PublicTransportTrain, ItemClass.SubService.PublicTransportMetro };
            }
            int num = Mathf.Max((int) ((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int) ((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int) ((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int) ((pos.z + maxDistance) / 64f + 135f), 269);
            bool noneFound = true;
            NetManager nm = Singleton<NetManager>.instance;
            TransportManager tm = Singleton<TransportManager>.instance;
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort stopId = nm.m_nodeGrid[i * 270 + j];
                    int num7 = 0;
                    while (stopId != 0) {
                        NetInfo info = nm.m_nodes.m_buffer[(int) stopId].Info;

                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport) && subservicesAllowed.Contains(info.m_class.m_subService)) {
                            ushort transportLine = nm.m_nodes.m_buffer[(int) stopId].m_transportLine;
                            if (transportLine != 0) {
                                if (!stopsFound.Contains(stopId)) {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[(int) stopId].m_position);
                                    if (num8 < maxDistance * maxDistance) {
                                        stopsFound.Add(stopId);
                                        GetNearStopPoints(nm.m_nodes.m_buffer[(int) stopId].m_position, maxDistance, ref stopsFound, subservicesAllowed, maxDepht, depth + 1);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        stopId = nm.m_nodes.m_buffer[(int) stopId].m_nextGridNode;
                        if (++num7 >= 32768) {
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
            int x = Mathf.Max((int) ((pos.x) / 64f + 135f), 0);
            int z = Mathf.Max((int) ((-pos.z) / 64f + 135f), 0);
            return new Vector2(x, z);
        }


        public static Vector2 gridPosition81Tiles(Vector3 pos, float invResolution = 24f)
        {
            int x = Mathf.Max((int) ((pos.x) / invResolution + 648), 0);
            int z = Mathf.Max((int) ((-pos.z) / invResolution + 648), 0);
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
            foreach (ushort s in intersections) {
                TransportLine tl = tm.m_lines.m_buffer[(int) s];
                if (t.Equals(default(TransportLine)) || tl.Info.GetSubService() != t.Info.GetSubService() || tl.m_lineNumber != t.m_lineNumber) {
                    string transportTypeLetter = "";
                    switch (TLMCW.getDefinitionForLine(s).toConfigIndex()) {
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



        public static void PrintIntersections(string airport, string harbor, string taxi, string regionalTrainStation, UIPanel intersectionsPanel, Dictionary<string, ushort> otherLinesIntersections, float scale = 1.0f, int maxItemsForSizeSwap = 3)
        {
            TransportManager tm = Singleton<TransportManager>.instance;

            int intersectionCount = otherLinesIntersections.Count;
            if (!String.IsNullOrEmpty(airport)) {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(harbor)) {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(taxi)) {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(regionalTrainStation)) {
                intersectionCount++;
            }
            float size = scale * (intersectionCount > maxItemsForSizeSwap ? 20 : 40);
            float multiplier = scale * (intersectionCount > maxItemsForSizeSwap ? 0.4f : 0.8f);
            foreach (var s in otherLinesIntersections.OrderBy(x => x.Key)) {
                TransportLine intersectLine = tm.m_lines.m_buffer[(int) s.Value];
                String bgSprite;
                ModoNomenclatura sufixo, prefixo, naoPrefixado;
                Separador separador;
                bool zeros;
                bool invertPrefixSuffix;
                ItemClass.SubService ss = getLineNamingParameters(s.Value, out prefixo, out separador, out sufixo, out naoPrefixado, out zeros, out invertPrefixSuffix, out bgSprite).subService;
                UIButtonLineInfo lineCircleIntersect = null;
                TLMUtils.createUIElement<UIButtonLineInfo>(ref lineCircleIntersect, intersectionsPanel.transform);
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
                UILabel lineNumberIntersect = null;
                TLMUtils.createUIElement<UILabel>(ref lineNumberIntersect, lineCircleIntersect.transform);
                lineNumberIntersect.autoSize = false;
                lineNumberIntersect.autoHeight = false;
                lineNumberIntersect.width = lineCircleIntersect.width;
                lineNumberIntersect.pivot = UIPivotPoint.MiddleCenter;
                lineNumberIntersect.textAlignment = UIHorizontalAlignment.Center;
                lineNumberIntersect.verticalAlignment = UIVerticalAlignment.Middle;
                lineNumberIntersect.name = "LineNumber";
                lineNumberIntersect.height = size;
                lineNumberIntersect.relativePosition = new Vector3(-0.5f, 0.5f);
                lineNumberIntersect.textColor = Color.white;
                lineNumberIntersect.outlineColor = Color.black;
                lineNumberIntersect.useOutline = true;
                bool day, night;
                getLineActive(ref intersectLine, out day, out night);
                if (!day || !night) {
                    UILabel daytimeIndicator = null;
                    TLMUtils.createUIElement<UILabel>(ref daytimeIndicator, lineCircleIntersect.transform);
                    daytimeIndicator.autoSize = false;
                    daytimeIndicator.width = size;
                    daytimeIndicator.height = size;
                    daytimeIndicator.color = Color.white;
                    daytimeIndicator.pivot = UIPivotPoint.MiddleLeft;
                    daytimeIndicator.verticalAlignment = UIVerticalAlignment.Middle;
                    daytimeIndicator.name = "LineTime";
                    daytimeIndicator.relativePosition = new Vector3(0f, 0f);
                    daytimeIndicator.atlas = TLMController.taLineNumber;
                    daytimeIndicator.backgroundSprite = day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
                }
                setLineNumberCircleOnRef(s.Value, lineNumberIntersect);
                lineNumberIntersect.textScale *= multiplier;
                lineNumberIntersect.relativePosition *= multiplier;
            }
            if (airport != string.Empty) {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "AirplaneIcon", airport);
            }
            if (harbor != string.Empty) {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "ShipIcon", harbor);
            }
            if (taxi != string.Empty) {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "TaxiIcon", taxi);
            }
            if (regionalTrainStation != string.Empty) {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "RegionalTrainIcon", regionalTrainStation);
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
            UILabel lineCircleIntersect = null;
            TLMUtils.createUIElement<UILabel>(ref lineCircleIntersect, parent.transform);
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
            string text;
            float textScale;
            Vector3 relativePosition;
            getLineNumberCircleOnRefParams(lineID, ratio, out text, out textScale, out relativePosition);
            reference.text = text;
            reference.textScale = textScale;
            reference.relativePosition = relativePosition;
        }

        private static void getLineNumberCircleOnRefParams(ushort lineID, float ratio, out string text, out float textScale, out Vector3 relativePosition)
        {
            text = TLMLineUtils.getLineStringId(lineID);
            int lenght = text.Length;
            if (lenght >= 4) {
                textScale = 1f * ratio;
                relativePosition = new Vector3(0f, 1f);
            } else if (lenght == 3) {
                textScale = 1.25f * ratio;
                relativePosition = new Vector3(0f, 1.5f);
            } else if (lenght == 2) {
                textScale = 1.75f * ratio;
                relativePosition = new Vector3(-0.5f, 0.5f);
            } else {
                textScale = 2.3f * ratio;
                relativePosition = new Vector3(-0.5f, 0f);
            }
        }

        public static int getVehicleCapacity(ushort vehicleId)
        {
            var ai = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].Info.GetAI();
            if (ai as BusAI != null) {
                return (ai as BusAI).m_passengerCapacity;
            }
            if (ai as PassengerPlaneAI != null) {
                return (ai as PassengerPlaneAI).m_passengerCapacity;
            }
            if (ai as PassengerShipAI != null) {
                return (ai as PassengerShipAI).m_passengerCapacity;
            }
            if (ai as PassengerFerryAI != null) {
                return (ai as PassengerFerryAI).m_passengerCapacity;
            }
            if (ai as PassengerBlimpAI != null) {
                return (ai as PassengerBlimpAI).m_passengerCapacity;
            }
            if (ai as CableCarAI != null) {
                return (ai as CableCarAI).m_passengerCapacity;
            }
            //if (ai as MonorailAI != null)
            //{
            //    return (ai as MonorailAI).m_passengerCapacity;
            //}
            if (ai as PassengerTrainAI != null) {
                return (ai as PassengerTrainAI).m_passengerCapacity;
            }
            if (ai as TaxiAI != null) {
                return (ai as TaxiAI).m_passengerCapacity;
            }
            if (ai as TramAI != null) {
                return (ai as TramAI).m_passengerCapacity;
            }
            return 0;
        }

    }

}
