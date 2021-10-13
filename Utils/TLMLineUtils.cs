using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.TransportLinesManager.ModShared.TLMFacade;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMLineUtils
    {
        public static void GetQuantityPassengerWaiting(ushort currentStop, out int residents, out int tourists, out int timeTilBored)
        {
            var residentsIn = 0;
            var touristsIn = 0;
            var timeTilBoredIn = 255;
            var cm = CitizenManager.instance;
            DoWithEachPassengerWaiting(currentStop, (citizen) =>
            {
                if ((cm.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None)
                {
                    touristsIn++;
                }
                else
                {
                    residentsIn++;
                }
                timeTilBoredIn = Math.Min(255 - cm.m_instances.m_buffer[citizen].m_waitCounter, timeTilBoredIn);
            });

            residents = residentsIn;
            tourists = touristsIn;
            timeTilBored = timeTilBoredIn;
        }


        public static void DoWithEachPassengerWaiting(ushort currentStop, Action<ushort> actionToDo)
        {
            ushort nextStop = TransportLine.GetNextStop(currentStop);
            CitizenManager cm = Singleton<CitizenManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            Vector3 position = nm.m_nodes.m_buffer[currentStop].m_position;
            Vector3 position2 = nm.m_nodes.m_buffer[nextStop].m_position;
            nm.m_nodes.m_buffer[currentStop].m_maxWaitTime = 0;
            int minX = Mathf.Max((int)((position.x - 64) / 8f + 1080f), 0);
            int minZ = Mathf.Max((int)((position.z - 64) / 8f + 1080f), 0);
            int maxX = Mathf.Min((int)((position.x + 64) / 8f + 1080f), 2159);
            int maxZ = Mathf.Min((int)((position.z + 64) / 8f + 1080f), 2159);
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
                            if (distance < 8196f)
                            {
                                CitizenInfo info = cm.m_instances.m_buffer[citizenIterator].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(citizenIterator, ref cm.m_instances.m_buffer[citizenIterator], position, position2))
                                {
                                    actionToDo(citizenIterator);
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

        public static float GetEffectiveBudget(ushort transportLine) => GetEffectiveBudgetInt(transportLine) / 100f;

        public static int GetEffectiveBudgetInt(ushort transportLine)
        {
            ref TransportLine tl = ref Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine];
            TransportInfo info = tl.Info;
            Tuple<float, int, int, float, bool> lineBudget = GetBudgetMultiplierLineWithIndexes(transportLine);
            int budgetClass = lineBudget.Fifth ? 100 : Singleton<EconomyManager>.instance.GetBudget(info.m_class);

            var result = (int)(budgetClass * lineBudget.First);
            var lineCfg = TLMTransportLineExtension.Instance.SafeGet(transportLine);
            if (result == 0 != lineCfg.IsZeroed)
            {
                lineCfg.IsZeroed = result == 0;
                if (lineCfg.IsZeroed)
                {
                    SimulationManager.instance.StartCoroutine(MakePassengersBored(transportLine, SimulationManager.instance.m_referenceFrameIndex));
                }
            }
            return result;
        }

        private static IEnumerator MakePassengersBored(ushort transportLine, uint simulationFrameStart)
        {
            int citizensCount = 0;
            do
            {
                do
                {
                    yield return 0;
                } while (SimulationManager.instance.m_referenceFrameIndex - simulationFrameStart < 5);
                if (!TLMTransportLineExtension.Instance.SafeGet(transportLine).IsZeroed)
                {
                    yield break;
                }
                ushort stop = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].m_stops;
                citizensCount = 0;
                do
                {
                    var citizensToBored = new List<ushort>();
                    DoWithEachPassengerWaiting(stop, (citizenId) => citizensToBored.Add(citizenId));
                    Randomizer r = new Randomizer();
                    foreach (var citizenId in citizensToBored)
                    {
                        CitizenManager.instance.m_instances.m_buffer[citizenId].m_waitCounter = byte.MaxValue;
                    }
                    citizensCount += citizensToBored.Count;
                    stop = TransportLine.GetNextStop(stop);
                } while (stop != Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].m_stops);
                simulationFrameStart = SimulationManager.instance.m_referenceFrameIndex;
            } while (citizensCount > 0 || !TLMTransportLineExtension.Instance.SafeGet(transportLine).IsZeroed);
        }

        public static IBasicExtensionStorage GetEffectiveConfigForLine(ushort lineId)
        {
            if (TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineId))
            {
                return TLMTransportLineExtension.Instance.SafeGet(lineId);
            }
            else
            {
                var tsd = TransportSystemDefinition.From(lineId);
                return (tsd.GetTransportExtension() as ISafeGettable<TLMPrefixConfiguration>).SafeGet(TLMPrefixesUtils.GetPrefix(lineId));
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

        public static float ReferenceTimer => (TransportLinesManagerMod.UseGameClockAsReferenceIfNoDayNight && !Singleton<SimulationManager>.instance.m_enableDayNight) ? (float)Singleton<SimulationManager>.instance.m_currentGameTime.TimeOfDay.TotalHours % 24 : Singleton<SimulationManager>.instance.m_currentDayTimeHour;

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
            Tuple<Tuple<BudgetEntryXml, int>, Tuple<BudgetEntryXml, int>, float> currentBudget = budgetConfig.GetAtHour(ReferenceTimer);
            return Tuple.New(Mathf.Lerp(currentBudget.First.First.Value, currentBudget.Second.First.Value, currentBudget.Third) / 100f, currentBudget.First.Second, currentBudget.Second.Second, currentBudget.Third, currentConfig is TLMTransportLineConfiguration);

        }
        public static string GetLineStringId(ushort lineIdx)
        {
            if (TLMTransportLineExtension.Instance.SafeGet(lineIdx).CustomCode is string customId && !customId.IsNullOrWhiteSpace())
            {
                return customId;
            }
            GetLineNamingParameters(lineIdx, out NamingMode prefix, out Separator s, out NamingMode suffix, out NamingMode nonPrefix, out bool zeros, out bool invertPrefixSuffix);
            return TLMPrefixesUtils.GetString(prefix, s, suffix, nonPrefix, Singleton<TransportManager>.instance.m_lines.m_buffer[lineIdx].m_lineNumber, zeros, invertPrefixSuffix);
        }

        public static void GetLineNamingParameters(ushort lineIdx, out NamingMode prefix, out Separator s, out NamingMode suffix, out NamingMode nonPrefix, out bool zeros, out bool invertPrefixSuffix) => GetLineNamingParameters(lineIdx, out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix, out string nil);


        public static TransportSystemDefinition GetLineNamingParameters(ushort lineIdx, out NamingMode prefix, out Separator s, out NamingMode suffix, out NamingMode nonPrefix, out bool zeros, out bool invertPrefixSuffix, out string icon)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(lineIdx);
            if (tsd != default)
            {
                GetNamingRulesFromTSD(out prefix, out s, out suffix, out nonPrefix, out zeros, out invertPrefixSuffix, tsd);
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
            icon = GetIconForLine(lineIdx);
            return tsd;
        }

        public static bool IsLineNumberAlredyInUse(int numLinha, TransportSystemDefinition tsdOr, int exclude)
        {
            numLinha &= 0xFFFF;
            if (numLinha == 0)
            {
                return true;
            }

            for (ushort i = 1; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
                {
                    continue;
                }
                ushort lnum = Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber;
                var tsd = TransportSystemDefinition.GetDefinitionForLine(i);
                if (tsd != default && i != exclude && tsd == tsdOr && lnum == numLinha)
                {
                    return true;
                }
            }
            return false;
        }
        public static void GetNamingRulesFromTSD(out NamingMode prefix, out Separator s, out NamingMode suffix, out NamingMode nonPrefix, out bool zeros, out bool invertPrefixSuffix, TransportSystemDefinition tsd)

        {
            if (tsd == TransportSystemDefinition.EVAC_BUS)
            {
                suffix = NamingMode.Number;
                s = Separator.Hyphen;
                prefix = NamingMode.Roman;
                nonPrefix = NamingMode.Number;
                zeros = false;
                invertPrefixSuffix = false;
            }
            else
            {
                var config = tsd.GetConfig();
                suffix = config.Suffix;
                s = config.Separator;
                prefix = config.Prefix;
                nonPrefix = config.NonPrefixedNaming;
                zeros = config.UseLeadingZeros;
                invertPrefixSuffix = config.InvertPrefixSuffix;
            }
        }

        public static string GetIconForLine(ushort lineIdx, bool noBorder = true) =>
            KlyteResourceLoader.GetDefaultSpriteNameFor(TLMPrefixesUtils.GetLineIcon(TransportManager.instance.m_lines.m_buffer[lineIdx].m_lineNumber, TransportSystemDefinition.GetDefinitionForLine(lineIdx)), noBorder);


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
                            if (transportLine != 0 && tsd != default && tsd.GetConfig().ShowInLinearMap)
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
            var tsd = TransportSystemDefinition.GetDefinitionForLine(s);
            if (tsd == default)
            {
                return null;
            }
            string transportTypeLetter =
              tsd == TransportSystemDefinition.PLANE ? "A"
            : tsd == TransportSystemDefinition.SHIP ? "B"
            : tsd == TransportSystemDefinition.BLIMP ? "C"
            : tsd == TransportSystemDefinition.HELICOPTER ? "D"
            : tsd == TransportSystemDefinition.TRAIN ? "E"
            : tsd == TransportSystemDefinition.FERRY ? "F"
            : tsd == TransportSystemDefinition.MONORAIL ? "G"
            : tsd == TransportSystemDefinition.METRO ? "H"
            : tsd == TransportSystemDefinition.CABLE_CAR ? "I"
            : tsd == TransportSystemDefinition.TROLLEY ? "J"
            : tsd == TransportSystemDefinition.TRAM ? "K"
            : tsd == TransportSystemDefinition.BUS ? "L"
            : tsd == TransportSystemDefinition.TOUR_BUS ? "M"
            : tsd == TransportSystemDefinition.TOUR_PED ? "N"
            : "";


            return transportTypeLetter + tl.m_lineNumber.ToString().PadLeft(5, '0');
        }



        public static void SetLineNumberCircleOnRef(ushort lineID, UITextComponent reference, float ratio = 1f)
        {
            GetLineNumberCircleOnRefParams(lineID, ratio, out string text, out Color textColor, out float textScale, out Vector3 relativePosition);
            reference.text = text;
            reference.textScale = textScale;
            reference.relativePosition = relativePosition;
            reference.textColor = textColor;
            reference.useOutline = true;
            reference.outlineColor = Color.black;
        }

        private static void GetLineNumberCircleOnRefParams(ushort lineID, float ratio, out string text, out Color textColor, out float textScale, out Vector3 relativePosition)
        {
            text = GetLineStringId(lineID).Trim();
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


        public static string[] GetAllStopsFromLine(ushort lineID)
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


        private static int colorChangeCooldown = 0;
        private static readonly Dictionary<ushort, Color> colorChangeTarget = new Dictionary<ushort, Color>();
        internal static void SetLineColor(MonoBehaviour parent, ushort lineId, Color color) => parent.StartCoroutine(ChangeColorCoroutine(parent, lineId, color));

        private static IEnumerator ChangeColorCoroutine(MonoBehaviour comp, ushort id, Color newColor)
        {
            colorChangeTarget[id] = newColor;
            if (colorChangeCooldown > 0)
            {
                yield break;
            }
            colorChangeCooldown = 3;
            var targetColor = colorChangeTarget[id];
            do
            {
                colorChangeCooldown--;
                yield return 0;
                if (targetColor != colorChangeTarget[id])
                {
                    colorChangeCooldown = 3;
                    targetColor = colorChangeTarget[id];
                }
            } while (colorChangeCooldown > 0);

            yield return RunColorChange(comp, id, targetColor);
            yield break;
        }

        public static IEnumerator RunColorChange(MonoBehaviour comp, ushort id, Color targetColor)
        {
            if (Singleton<SimulationManager>.exists)
            {
                AsyncTask<bool> task = Singleton<SimulationManager>.instance.AddAction(Singleton<TransportManager>.instance.SetLineColor(id, targetColor));
                yield return task.WaitTaskCompleted(comp);
                if (UVMPublicTransportWorldInfoPanel.GetLineID() == id)
                {
                    UVMPublicTransportWorldInfoPanel.ForceReload();
                }
            }
        }

        public static AsyncTask<bool> SetLineName(ushort lineIdx, string name) => Singleton<SimulationManager>.instance.AddAction(TransportManager.instance.SetLineName(lineIdx, name));

        private static TransportInfo.TransportType[] m_roadTransportTypes = new TransportInfo.TransportType[] { TransportInfo.TransportType.Bus, TransportInfo.TransportType.Tram, TransportInfo.TransportType.Trolleybus };
        internal static bool IsRoadLine(ushort lineId) => m_roadTransportTypes.Contains(TransportManager.instance.m_lines.m_buffer[lineId].Info.m_transportType);
        public static string CalculateAutoName(ushort lineIdx, out List<DestinationPoco> stationDestinations)
        {
            ref TransportLine t = ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineIdx];
            stationDestinations = new List<DestinationPoco>();
            if ((t.m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
            {
                return null;
            }
            ushort nextStop = t.m_stops;
            bool allowPrefixInStations = m_roadTransportTypes.Contains(t.Info.m_transportType);
            var stations = new List<Tuple<NamingType, string, ushort, bool>>();
            var allowTerminals = TransportSystemDefinition.From(lineIdx).CanHaveTerminals();
            do
            {
                NetNode stopNode = NetManager.instance.m_nodes.m_buffer[nextStop];
                string stationName = TLMStationUtils.GetStationName(nextStop, lineIdx, t.Info.m_class.m_subService, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefixFound, out ushort buildingId, out NamingType namingType, true, true, true);
                var tuple = Tuple.New(namingType, allowPrefixInStations ? $"{prefixFound?.Trim()} {stationName?.Trim()}".Trim() : stationName.Trim(), nextStop, allowTerminals && (TLMStopDataContainer.Instance.SafeGet(nextStop).IsTerminal || nextStop == t.m_stops));
                stations.Add(tuple);
                nextStop = TransportLine.GetNextStop(nextStop);
            } while (nextStop != t.m_stops && nextStop != 0);
            string prefix = "";
            if (TLMBaseConfigXML.Instance.AddLineCodeInAutoname)
            {
                prefix = $"[{GetLineStringId(lineIdx)}] ";
            }
            var hasAnyTerminals = allowTerminals && stations.Where(x => x.Fourth).Count() > 1;
            if (hasAnyTerminals)
            {
                stations = stations.Select((x) => x.Fourth ? Tuple.New(NamingType.TERMINAL, x.Second, x.Third, x.Fourth) : x).ToList();
                stationDestinations = stations.Where(x => x.Fourth).Select(x => new DestinationPoco { stopId = x.Third, stopName = x.Second }).ToList();
            }
            LogUtils.DoLog($"stations => [{string.Join(" ; ", stations.Select(x => $"{x.First}|{x.Second}").ToArray())}]");
            string startStationStr, endStationStr;
            if (stations.Count % 2 == 0 && stations.Count > 2)
            {
                LogUtils.DoLog($"Try Simmetric");
                int middle = -1;
                for (int i = 1; i <= stations.Count / 2; i++)
                {
                    if (stations[i - 1].First == stations[i + 1].First && stations[i - 1].Second == stations[i + 1].Second)
                    {
                        middle = i;
                        break;
                    }
                }
                LogUtils.DoLog($"middle => {middle}");
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
                        startStationStr = stations[middle % stations.Count].Second;
                        endStationStr = stations[(middle + (stations.Count / 2)) % stations.Count].Second;
                        if (!hasAnyTerminals)
                        {
                            stationDestinations.Add(new DestinationPoco { stopId = stations[middle % stations.Count].Third, stopName = startStationStr });
                            stationDestinations.Add(new DestinationPoco { stopId = stations[(middle + (stations.Count / 2)) % stations.Count].Third, stopName = endStationStr });
                        }


                        if (startStationStr == endStationStr)
                        {
                            startStationStr = (TLMBaseConfigXML.Instance.CircularIfSingleDistrictLine ? "Circular " : "") + startStationStr;
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
            int maxDistanceEnd = (int)((idxStations.Count / 8f) + 0.5f);
            LogUtils.DoLog("idxStations");
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
                startStationStr = idxStations[targetStart].Third;
                endStationStr = stations[mostRelevantEndIdx].Second;
                if (!hasAnyTerminals)
                {
                    stationDestinations.Add(new DestinationPoco { stopId = idxStations[targetStart].Fourth, stopName = startStationStr });
                    stationDestinations.Add(new DestinationPoco { stopId = stations[mostRelevantEndIdx].Third, stopName = endStationStr });
                }
                if (startStationStr == endStationStr)
                {
                    startStationStr = (TLMBaseConfigXML.Instance.CircularIfSingleDistrictLine ? "Circular " : "") + startStationStr;
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
                startStationStr = (TLMBaseConfigXML.Instance.CircularIfSingleDistrictLine ? "Circular " : "") + idxStations[0].Third;
                if (!hasAnyTerminals)
                {
                    stationDestinations.Add(new DestinationPoco { stopId = idxStations[0].Fourth, stopName = startStationStr });
                }
                return prefix + startStationStr;
            }

        }

        public static Tuple<string, Color, string> GetIconStringParameters(ushort lineId) => Tuple.New(GetIconForLine(lineId, false), Singleton<TransportManager>.instance.GetLineColor(lineId), GetLineStringId(lineId));



        public static int ProjectTargetVehicleCount(TransportInfo info, float lineLength, float budget) => Mathf.CeilToInt(budget * lineLength / info.m_defaultVehicleDistance);
        public static float CalculateBudgetForEachVehicle(TransportInfo info, float lineLength) => info.m_defaultVehicleDistance / lineLength;


        public static Tuple<TicketPriceEntryXml, int> GetTicketPriceForLine(TransportSystemDefinition tsd, ushort lineId) => GetTicketPriceForLine(tsd, lineId, ReferenceTimer);
        public static Tuple<TicketPriceEntryXml, int> GetTicketPriceForLine(TransportSystemDefinition tsd, ushort lineId, float hour)
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
                ticketPriceDefault = Tuple.New(new TicketPriceEntryXml() { Value = (uint)tsd.GetConfig().DefaultTicketPrice }, -1);
            }
            if ((ticketPriceDefault?.First?.Value ?? 0) == 0)
            {
                ticketPriceDefault = Tuple.New(new TicketPriceEntryXml() { Value = (uint)TransportManager.instance.m_lines.m_buffer[lineId].Info.m_ticketPrice }, -1);
            }

            return ticketPriceDefault;
        }




        internal static readonly NamingMode[] m_numberedNamingTypes = new NamingMode[]
        {
        NamingMode. LatinLowerNumber ,
        NamingMode. LatinUpperNumber ,
        NamingMode. GreekLowerNumber,
        NamingMode. GreekUpperNumber,
        NamingMode. CyrillicLowerNumber,
        NamingMode. CyrillicUpperUpper
        };

        public static readonly TransferManager.TransferReason[] defaultAllowedVehicleTypes = {
            TransferManager.TransferReason.Blimp ,
            TransferManager.TransferReason.CableCar ,
            TransferManager.TransferReason.Ferry ,
            TransferManager.TransferReason.MetroTrain ,
            TransferManager.TransferReason.Monorail ,
            TransferManager.TransferReason.PassengerTrain ,
            TransferManager.TransferReason.PassengerPlane ,
            TransferManager.TransferReason.PassengerShip ,
            TransferManager.TransferReason.Tram ,
            TransferManager.TransferReason.Bus
        };
    }

}
