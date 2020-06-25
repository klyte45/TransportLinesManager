using ColossalFramework;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Linq;
using UnityEngine;
using static TransportInfo;

namespace Klyte.TransportLinesManager.ModShared
{
    public class TLMShared : MonoBehaviour
    {
        public static TLMShared Instance => TransportLinesManagerMod.Controller?.SharedInstance;

        internal void OnLineSymbolParameterChanged() => EventLineSymbolParameterChanged?.Invoke();
        internal void OnAutoNameParameterChanged() => EventAutoNameParameterChanged?.Invoke();

        public event Action EventLineSymbolParameterChanged;
        public event Action EventAutoNameParameterChanged;

        private string m_identifierFormatBusTrolley = "E FSTU";
        private string m_identifierFormatMetroTrainTram = "OTUL";
        private TransportType[] m_railTypes = new TransportType[]
        {
            TransportInfo.TransportType.Metro,
            TransportInfo.TransportType.Train,
            TransportInfo.TransportType.Tram,
            TransportInfo.TransportType.Monorail,
        };
        public string GetVehicleIdentifier(ushort vehicleId)
        {
            var firstVehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetFirstVehicle(vehicleId);
            var tlId = VehicleManager.instance.m_vehicles.m_buffer[firstVehicle].m_transportLine;
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[tlId];
            string identifierFormat = m_railTypes.Contains(tl.Info.m_transportType) ? m_identifierFormatMetroTrainTram : m_identifierFormatBusTrolley;

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
                    ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[tlId];
                    if (TLMPrefixesUtils.HasPrefix(ref tl))
                    {
                        var tsd = TransportSystemDefinition.From(tl.Info);
                        var prefix = (int)TLMPrefixesUtils.GetPrefix(tlId);
                        var nameMode = TLMPrefixesUtils.GetPrefixModoNomenclatura(tsd.ToConfigIndex());
                        linePrefix = TLMPrefixesUtils.GetStringFromNameMode(nameMode, prefix).Trim().PadLeft(3, '\0');
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
                    depotPrefix = VehicleManager.instance.m_vehicles.m_buffer[firstVehicle].m_sourceBuilding.ToString("X3").ToUpper();
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
                    result += item switch
                    {
                        'D' => GetDepotPrefix()[0],
                        'E' => GetDepotPrefix()[1],
                        'F' => GetDepotPrefix()[2],

                        'M' => GetModelPrefix()[0],
                        'N' => GetModelPrefix()[1],
                        'O' => GetModelPrefix()[2],

                        'P' => GetLinePrefix()[0],
                        'Q' => GetLinePrefix()[1],
                        'R' => GetLinePrefix()[2],

                        'J' => GetVehicleNthTrailer().Replace('\0', '0')[0],
                        'K' => GetVehicleNthTrailer().Replace('\0', '0')[1],
                        'L' => GetVehicleNthTrailer().Replace('\0', '0')[2],

                        'S' => GetVehicleNthDepot().Replace('\0', '0')[0],
                        'T' => GetVehicleNthDepot().Replace('\0', '0')[1],
                        'U' => GetVehicleNthDepot().Replace('\0', '0')[2],

                        'V' => GetVehicleInstanceString().Replace('\0', '0')[0],
                        'W' => GetVehicleInstanceString().Replace('\0', '0')[1],
                        'X' => GetVehicleInstanceString().Replace('\0', '0')[2],
                        'Y' => GetVehicleInstanceString().Replace('\0', '0')[3],
                        'Z' => GetVehicleInstanceString().Replace('\0', '0')[4],

                        'j' => GetVehicleNthTrailer()[0],
                        'k' => GetVehicleNthTrailer()[1],
                        'l' => GetVehicleNthTrailer()[2],

                        's' => GetVehicleNthDepot()[0],
                        't' => GetVehicleNthDepot()[1],
                        'u' => GetVehicleNthDepot()[2],

                        'v' => GetVehicleInstanceString()[0],
                        'w' => GetVehicleInstanceString()[1],
                        'x' => GetVehicleInstanceString()[2],
                        'y' => GetVehicleInstanceString()[3],
                        'z' => GetVehicleInstanceString()[4],

                        _ => item
                    };
                }
            }
            return result.Replace("\0", "");
        }
    }
}