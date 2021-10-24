using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.ModShared
{
    public class TLMFacade : MonoBehaviour
    {
        public static TLMFacade Instance => TransportLinesManagerMod.Controller?.SharedInstance;

        internal void OnLineSymbolParameterChanged()
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventLineSymbolParameterChanged?.Invoke();
            }
        }

        internal void OnAutoNameParameterChanged()
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventAutoNameParameterChanged?.Invoke();
            }
        }

        internal void OnVehicleIdentifierParameterChanged()
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventVehicleIdentifierParameterChanged?.Invoke();
            }
        }

        internal void OnLineDestinationsChanged(ushort lineId)
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                EventLineDestinationsChanged?.Invoke(lineId);
                if (TLMBaseConfigXML.Instance.UseAutoName)
                {
                    TLMController.AutoName(lineId);
                }
            }
        }

        public event Action EventLineSymbolParameterChanged;
        public event Action EventAutoNameParameterChanged;
        public event Action EventVehicleIdentifierParameterChanged;
        public event Action<ushort> EventLineDestinationsChanged;

        public static string GetFullStationName(ushort stopId, ushort lineId, ItemClass.SubService subService) =>
            stopId == 0 ? ""
                : TLMLineUtils.IsRoadLine(lineId) ? TLMStationUtils.GetFullStationName(stopId, lineId, subService, false)
                : TLMStationUtils.GetStationName(stopId, lineId, subService, false);

        public static Tuple<string, Color, string> GetIconStringParameters(ushort lineID) => TLMLineUtils.GetIconStringParameters(lineID);
        public static ushort GetStationBuilding(ushort stopId, ushort lineId) => TLMStationUtils.GetStationBuilding(stopId, lineId, false);
        public static string GetLineSortString(ushort lineId, ref TransportLine transportLine) => TLMLineUtils.GetLineSortString(lineId, ref transportLine);

        public string GetVehicleIdentifier(ushort vehicleId)
        {
            var firstVehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetFirstVehicle(vehicleId);
            ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicle];
            var tlId = vehicle.m_transportLine;
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[tlId];

            var tsd = TransportSystemDefinition.From(vehicle.Info);
            var config = tsd.GetConfig();
            string identifierFormat = (tlId == 0 && vehicle.m_targetBuilding != 0 ? config.VehicleIdentifierFormatForeign : config.VehicleIdentifierFormatLocal);

            string result = "";

            string linePrefix = null;
            string depotPrefix = null;
            string modelPrefix = null;
            string vehicleString = null;
            string vehicleNthDepot = null;
            string vehicleNthTrailer = null;

            string GetLinePrefix()
            {
                if (linePrefix == null)
                {
                    ref TransportLine tl2 = ref TransportManager.instance.m_lines.m_buffer[tlId];
                    if (TLMPrefixesUtils.HasPrefix(ref tl2))
                    {
                        var tsd2 = TransportSystemDefinition.FromLocal(tl2.Info);
                        var prefix = (int)TLMPrefixesUtils.GetPrefix(tlId);
                        linePrefix = TLMPrefixesUtils.GetStringFromNameMode(tsd2.GetConfig().Prefix, prefix).Trim().PadLeft(3, '\0');
                    }
                    else
                    {
                        linePrefix = "\0\0\0";
                    }
                }
                return linePrefix;
            }
            string GetDepotPrefix()
            {
                if (depotPrefix == null)
                {
                    depotPrefix = VehicleManager.instance.m_vehicles.m_buffer[firstVehicle].m_sourceBuilding.ToString("D3");
                    depotPrefix = depotPrefix.Substring(depotPrefix.Length - 3, 3);
                }
                return depotPrefix;
            }
            string GetModelPrefix()
            {
                if (modelPrefix == null)
                {
                    var info = VehicleManager.instance.m_vehicles.m_buffer[firstVehicle].Info;

                    modelPrefix = (info.name.Contains(".") ? info.name.Split(new char[] { '.' }, 2)[1] : info.name).ToUpper().Substring(0, 3);
                }
                return modelPrefix;
            }

            string GetVehicleInstanceString()
            {
                if (vehicleString == null)
                {
                    vehicleString = vehicleId.ToString().PadLeft(5, '\0');
                }
                return vehicleString;
            }

            string GetVehicleNthDepot()
            {
                if (vehicleNthDepot == null)
                {
                    int counter = 0;
                    ref Vehicle[] vBuffer = ref VehicleManager.instance.m_vehicles.m_buffer;
                    var targetVehicle = firstVehicle;
                    var depotId = vBuffer[targetVehicle].m_sourceBuilding;
                    ref Building[] buffer = ref BuildingManager.instance.m_buildings.m_buffer;
                    var nextVehicle = buffer[depotId].m_ownVehicles;
                    while (nextVehicle != targetVehicle)
                    {
                        counter++;
                        nextVehicle = vBuffer[nextVehicle].m_nextOwnVehicle;
                        if (nextVehicle == 0)
                        {
                            counter = -1;
                            break;
                        }
                        if (counter > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!A\n" + Environment.StackTrace);
                            break;
                        }
                    }

                    vehicleNthDepot = counter.ToString().PadLeft(3, '\0'); ;
                }
                return vehicleNthDepot;
            }

            string GetVehicleNthTrailer()
            {
                if (vehicleNthTrailer == null)
                {
                    int counter = 0;
                    ref Vehicle[] vBuffer = ref VehicleManager.instance.m_vehicles.m_buffer;
                    var nextVehicle = firstVehicle;
                    while (nextVehicle != vehicleId)
                    {
                        nextVehicle = vBuffer[nextVehicle].m_trailingVehicle;
                        counter++;
                        if (nextVehicle == 0)
                        {
                            counter = -1;
                            break;
                        }
                        if (counter > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!B\n" + Environment.StackTrace);
                            break;
                        }
                    }

                    vehicleNthTrailer = counter.ToString().PadLeft(3, '\0'); ;
                }
                return vehicleNthTrailer;
            }


            char GetLetter(char item)
            {
                switch (item)
                {
                    case 'D': return GetDepotPrefix()[0];
                    case 'E': return GetDepotPrefix()[1];
                    case 'F': return GetDepotPrefix()[2];

                    case 'M': return GetModelPrefix()[0];
                    case 'N': return GetModelPrefix()[1];
                    case 'O': return GetModelPrefix()[2];

                    case 'P': return GetLinePrefix()[0];
                    case 'Q': return GetLinePrefix()[1];
                    case 'R': return GetLinePrefix()[2];

                    case 'J': return GetVehicleNthTrailer().Replace('\0', '0')[0];
                    case 'K': return GetVehicleNthTrailer().Replace('\0', '0')[1];
                    case 'L': return GetVehicleNthTrailer().Replace('\0', '0')[2];

                    case 'S': return GetVehicleNthDepot().Replace('\0', '0')[0];
                    case 'T': return GetVehicleNthDepot().Replace('\0', '0')[1];
                    case 'U': return GetVehicleNthDepot().Replace('\0', '0')[2];

                    case 'V': return GetVehicleInstanceString().Replace('\0', '0')[0];
                    case 'W': return GetVehicleInstanceString().Replace('\0', '0')[1];
                    case 'X': return GetVehicleInstanceString().Replace('\0', '0')[2];
                    case 'Y': return GetVehicleInstanceString().Replace('\0', '0')[3];
                    case 'Z': return GetVehicleInstanceString().Replace('\0', '0')[4];

                    case 'j': return GetVehicleNthTrailer()[0];
                    case 'k': return GetVehicleNthTrailer()[1];
                    case 'l': return GetVehicleNthTrailer()[2];

                    case 's': return GetVehicleNthDepot()[0];
                    case 't': return GetVehicleNthDepot()[1];
                    case 'u': return GetVehicleNthDepot()[2];

                    case 'v': return GetVehicleInstanceString()[0];
                    case 'w': return GetVehicleInstanceString()[1];
                    case 'x': return GetVehicleInstanceString()[2];
                    case 'y': return GetVehicleInstanceString()[3];
                    case 'z':
                        return GetVehicleInstanceString()[4];

                    default: return item;
                };
            }

            bool escapeNext = false;
            foreach (char item in identifierFormat)
            {
                if (escapeNext)
                {
                    result += item;
                    escapeNext = false;
                }
                else if (item == '\\')
                {
                    escapeNext = true;
                }
                else
                {
                    result += GetLetter(item);
                }
            }
            return result.Replace("\0", "").Trim();
        }
        [Obsolete("Deprecated in TLM14, use the alternative signature with destination list.", true)]
        public static void CalculateAutoName(ushort lineId, out ushort startStation, out ushort endStation, out string startStationStr, out string endStationStr)
        {
            TLMLineUtils.CalculateAutoName(lineId, 0, out List<DestinationPoco> destinations);
            if (destinations.Count > 0)
            {
                startStation = destinations.First().stopId;
                endStation = destinations.Last().stopId;
                startStationStr = destinations.First().stopName;
                endStationStr = destinations.Last().stopName;
            }
            else
            {
                startStation = endStation = 0;
                startStationStr = endStationStr = null;
            }

        }
        public static void CalculateAutoName(ushort lineId, out List<DestinationPoco> destinations)
            => TLMLineUtils.CalculateAutoName(lineId, 0, out destinations);

        public static string GetLineStringId(ushort lineId) => TLMLineUtils.GetLineStringId(lineId);

        public class DestinationPoco
        {
            public ushort stopId;
            public string stopName;
        }
    }
}