using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Klyte.TransportLinesManager.MapDrawer
{
    public class TLMMapDrawer
    {
        public static void DrawCityMap()
        {

            TLMController controller = TLMController.Instance;
            var linesByType = new Dictionary<TransportInfo.TransportType, List<ushort>>();

            foreach (object type in Enum.GetValues(typeof(TransportInfo.TransportType)))
            {
                linesByType[(TransportInfo.TransportType)type] = new List<ushort>();
            }

            int nextStationId = 1;
            for (ushort lineId = 0; lineId < TransportManager.instance.m_lines.m_size; lineId++)
            {
                ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[lineId];

                if (t.m_lineNumber > 0 && allowedTypesToDraw.Contains(t.Info.m_transportType) && (t.m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None)
                {
                    linesByType[t.Info.m_transportType].Add(lineId);
                }
            }
            Func<Vector3, float, Vector2> calc = MapUtils.GridPosition81Tiles;
            NetManager nm = NetManager.instance;
            float invPrecision = 35;

            var stations = new List<TLMMapStation>();
            var transportLines = new Dictionary<ushort, TLMMapTransportLine>();
            foreach (var tt in linesByType.OrderByDescending(x => GetRangeFromTransportType(x.Key)))
            {
                foreach (ushort lineId in tt.Value)
                {
                    ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[lineId];
                    float range = GetRangeFromTransportType(t.Info.m_transportType);

                    int stopsCount = t.CountStops(lineId);
                    if (stopsCount == 0)
                    {
                        continue;
                    }
                    Color color = t.m_color;
                    t.GetActive(out bool day, out bool night);
                    transportLines[lineId] = new TLMMapTransportLine(color, day, night, lineId);
                    ushort startStop = t.m_stops;
                    ushort nextStop = startStop;
                    do
                    {
                        string name = TLMStationUtils.GetStationName(nextStop, lineId, t.Info.m_stationSubService, out ItemClass.Service service, out ItemClass.SubService nil2, out string prefix, out ushort buildingId, out NamingType namingType, 0);

                        Vector3 worldPos = TLMStationUtils.GetStationBuildingPosition(nextStop, t.Info.m_stationSubService);
                        Vector2 pos2D = calc(worldPos, invPrecision);
                        Vector2 gridAdd = Vector2.zero;

                        TLMMapStation idx = stations.FirstOrDefault(x => x.stopsWithWorldPos.ContainsKey(nextStop));
                        if (idx != null)
                        {
                            transportLines[lineId].AddStation(ref idx);
                        }
                        else
                        {
                            var nearStops = new Dictionary<ushort, Vector3>();
                            TLMLineUtils.GetNearStopPoints(worldPos, range, ref nearStops, new ItemClass.SubService[] {
                                ItemClass.SubService.PublicTransportShip,
                                ItemClass.SubService.PublicTransportPlane,
                                ItemClass.SubService.PublicTransportTrain,
                                ItemClass.SubService.PublicTransportMonorail,
                                ItemClass.SubService.PublicTransportCableCar,
                                ItemClass.SubService.PublicTransportMetro ,
                                ItemClass.SubService.PublicTransportTram,
                                ItemClass.SubService.PublicTransportBus,
                                ItemClass.SubService.PublicTransportTrolleybus
                            }, 15);
                            if (!nearStops.ContainsKey(nextStop))
                            {
                                nearStops[nextStop] = NetManager.instance.m_nodes.m_buffer[nextStop].m_position;
                            }
                            var thisStation = new TLMMapStation(name, pos2D, worldPos, nearStops, nextStationId++, service, nextStop);
                            stations.Add(thisStation);
                            transportLines[lineId].AddStation(ref thisStation);
                        }
                        nextStop = TransportLine.GetNextStop(nextStop);
                    } while (nextStop != startStop && nextStop != 0);
                }
            }
            BuildHtml(stations, transportLines, Singleton<SimulationManager>.instance.m_metaData.m_CityName, Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier, Singleton<SimulationManager>.instance.m_currentGameTime);
        }

        private static float GetRangeFromTransportType(TransportInfo.TransportType t)
        {
            float range = 35;
            switch (t)
            {
                case TransportInfo.TransportType.Ship:
                case TransportInfo.TransportType.Airplane:
                    range = 120;
                    break;
                case TransportInfo.TransportType.Metro:
                case TransportInfo.TransportType.Monorail:
                case TransportInfo.TransportType.Train:
                case TransportInfo.TransportType.CableCar:
                    range = 90;
                    break;
            }

            return range;
        }

        private static TransportInfo.TransportType[] allowedTypesToDraw =
        {
            TransportInfo.TransportType.Monorail,
            TransportInfo.TransportType.Airplane,
            TransportInfo.TransportType.Metro,
            TransportInfo.TransportType.Ship,
            TransportInfo.TransportType.TouristBus,
            TransportInfo.TransportType.Pedestrian,
            TransportInfo.TransportType.Train,
            TransportInfo.TransportType.Tram,
            TransportInfo.TransportType.Bus,
            TransportInfo.TransportType.Trolleybus,
            TransportInfo.TransportType.Helicopter,
        };

        private static string BuildHtml(List<TLMMapStation> stations, Dictionary<ushort, TLMMapTransportLine> transportLines, string cityName, string cityId, DateTime currentTime)
        {
            var svg = new TLMMapHtmlTemplate();
            string cityMapsFolder = TLMController.ExportedMapsFolder + Path.DirectorySeparatorChar + $"{cityName}_{cityId}";
            FileUtils.EnsureFolderCreation(cityMapsFolder);
            string filename = cityMapsFolder + Path.DirectorySeparatorChar + $"{cityName}_{currentTime.ToString("yyyy-MM-dd-HH-mm-ss")}.html";
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            StreamWriter sr = File.CreateText(filename);
            var cto = new TLMMapCityTransportObject
            {
                transportLines = transportLines,
                stations = stations.OrderBy(x => x.Id).ToList()
            };
            sr.WriteLine(svg.GetResult(cto, cityName, currentTime));
            sr.Close();
            return filename;
        }
    }

}

