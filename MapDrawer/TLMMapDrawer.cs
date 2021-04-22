using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
/**
   OBS: Mudar a forma de pintar as linhas, criar caminhos comuns entre estações iguais (ajuda no tram)
   ver como fazer linhas de sentido único no tram
*/
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

            //			List<int> usedX = new List<int> ();
            //			List<int> usedY = new List<int> ();
            int nextStationId = 1;
            for (ushort lineId = 0; lineId < TransportManager.instance.m_lines.m_size; lineId++)
            {
                TransportLine t = TransportManager.instance.m_lines.m_buffer[lineId];

                if (t.m_lineNumber > 0 && allowedTypesToDraw.Contains(t.Info.m_transportType) && (t.m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None)
                {
                    linesByType[t.Info.m_transportType].Add(lineId);
                }
            }
            CalculateCoords calc = MapUtils.GridPosition81Tiles;
            NetManager nm = NetManager.instance;
            float invPrecision = 32;
            //Restart:
            var positions = new Dictionary<int, List<int>>();
            var stations = new List<Station>();
            var transportLines = new Dictionary<ushort, TLMMapTransportLine>();
            foreach (var tt in linesByType)
            {
                foreach (ushort lineId in tt.Value)
                {
                    TransportLine t = TransportManager.instance.m_lines.m_buffer[lineId];
                    float range = 75f;
                    switch (tt.Key)
                    {
                        case TransportInfo.TransportType.Ship:
                            range = 150f;
                            break;
                        case TransportInfo.TransportType.Metro:
                        case TransportInfo.TransportType.Monorail:
                        case TransportInfo.TransportType.Train:
                        case TransportInfo.TransportType.CableCar:
                            range = 100f;
                            break;
                    }


                    int stopsCount = t.CountStops(lineId);
                    if (stopsCount == 0)
                    {
                        continue;
                    }
                    Color color = t.m_color;
                    Vector2 ultPos = Vector2.zero;
                    t.GetActive(out bool day, out bool night);
                    transportLines[lineId] = new TLMMapTransportLine(color, day, night, lineId);
                    int startStop = 0;
                    int finalStop = stopsCount;

                    for (int j = startStop; j < finalStop; j++)
                    {
                        //						Debug.Log ("ULT POS:" + ultPos);
                        ushort nextStop = t.GetStop(j % stopsCount);
                        string name = TLMStationUtils.GetStationName(nextStop, lineId, t.Info.m_stationSubService, out ItemClass.Service service, out ItemClass.SubService nil2, out string prefix, out ushort buildingId, out NamingType namingType);

                        Vector3 worldPos = TLMStationUtils.GetStationBuildingPosition(nextStop, t.Info.m_stationSubService);
                        Vector2 pos2D = calc(worldPos, invPrecision);
                        Vector2 gridAdd = Vector2.zero;


                        Station idx = stations.FirstOrDefault(x => x.stopsWithWorldPos.ContainsKey(nextStop) || x.centralPos == pos2D);
                        if (idx != null)
                        {
                            transportLines[lineId].AddStation(ref idx);
                        }
                        else
                        {
                            //if (positions.containskey((int)pos2d.x) && positions[(int)pos2d.x].contains((int)pos2d.y))
                            //{
                            //    float exp = (float)(math.log(invprecision) / math.log(2)) - 1;
                            //    invprecision = (float)math.pow(2, exp);
                            //    goto restart;
                            //}
                            var nearStops = new Dictionary<ushort, Vector3>();
                            TLMLineUtils.GetNearStopPoints(worldPos, range, ref nearStops, new ItemClass.SubService[] { ItemClass.SubService.PublicTransportShip, ItemClass.SubService.PublicTransportPlane }, 10);
                            TLMLineUtils.GetNearStopPoints(worldPos, range, ref nearStops, new ItemClass.SubService[] { ItemClass.SubService.PublicTransportTrain, ItemClass.SubService.PublicTransportMonorail, ItemClass.SubService.PublicTransportCableCar, ItemClass.SubService.PublicTransportMetro }, 10);
                            TLMLineUtils.GetNearStopPoints(worldPos, range, ref nearStops, new ItemClass.SubService[] { ItemClass.SubService.PublicTransportTram, ItemClass.SubService.PublicTransportBus }, 10);
                            if (!nearStops.ContainsKey(nextStop))
                            {
                                nearStops[nextStop] = NetManager.instance.m_nodes.m_buffer[nextStop].m_position;
                            }
                            LogUtils.DoLog("Station: ${0}; nearStops: ${1}", name, string.Join(",", nearStops.Select(x => x.ToString()).ToArray()));
                            var thisStation = new Station(name, pos2D, worldPos, nearStops, nextStationId++, service, nextStop);
                            stations.Add(thisStation);
                            transportLines[lineId].AddStation(ref thisStation);
                        }
                        if (!positions.ContainsKey((int)pos2D.x))
                        {
                            positions[(int)pos2D.x] = new List<int>();
                        }
                        positions[(int)pos2D.x].Add((int)pos2D.y);
                        //						Debug.Log ("POS:" + pos);
                        ultPos = pos2D;
                    }
                }
            }
            PrintToSVG(stations, transportLines, Singleton<SimulationManager>.instance.m_metaData.m_CityName, Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier, Singleton<SimulationManager>.instance.m_currentGameTime);
            //printToJson(stations, transportLines, Singleton<SimulationManager>.instance.m_metaData.m_CityName + "_" + Singleton<SimulationManager>.instance.m_currentGameTime.ToString("yyyy.MM.dd"));
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

        public static string PrintToSVG(List<Station> stations, Dictionary<ushort, TLMMapTransportLine> transportLines, string cityName, string cityId, DateTime currentTime)
        {
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            foreach (Station s in stations)
            {
                if (s.centralPos.x > maxX)
                {
                    maxX = s.centralPos.x;
                }
                if (s.centralPos.y > maxY)
                {
                    maxY = s.centralPos.y;
                }
                if (s.centralPos.x < minX)
                {
                    minX = s.centralPos.x;
                }
                if (s.centralPos.y < minY)
                {
                    minY = s.centralPos.y;
                }
            }
            return BuildHtml(stations, transportLines, cityName, cityId, currentTime, minX, minY, maxX, maxY);
        }

        private static string BuildHtml(List<Station> stations, Dictionary<ushort, TLMMapTransportLine> transportLines, string cityName, string cityId, DateTime currentTime, float minX, float minY, float maxX, float maxY)
        {
            float maxRadius = Math.Max(stations.Max(x => x.getAllStationOffsetPoints().Count) * 2 + 2, 10);

            var svg = new TLMMapHtmlTemplate();

            var linesOrdened = transportLines.OrderBy(x => GetLineUID(x.Key)).ToList();
            //ordena pela quantidade de linhas passando
            stations = stations.OrderBy(x => x.linesPassingCount).ToList();

            //calcula as posições de todas as estações no mapa
            foreach (KeyValuePair<ushort, TLMMapTransportLine> line in linesOrdened)
            {
                Station station0 = line.Value[0];
                Vector2 prevPos = station0.getPositionForLine(line.Key, line.Value[1].centralPos);
                for (int i = 1; i < line.Value.StationsCount(); i++)
                {
                    prevPos = line.Value[i].getPositionForLine(line.Key, prevPos);
                }
            }
            //adiciona as exceções
            //  svg.addStationsToExceptionMap(stations);
            //pinta as linhas
            //    foreach (KeyValuePair<ushort, MapTransportLine> line in linesOrdened)
            //     {
            //     svg.addTransportLine(line.Value, line.Key);
            //}
            // svg.drawAllLines();
            //   foreach (Station station in stations)
            {
                //      svg.addStation(station, transportLines);
            }
            string cityMapsFolder = TLMController.ExportedMapsFolder + Path.DirectorySeparatorChar + $"{cityName}_{cityId}";
            FileInfo fipalette = FileUtils.EnsureFolderCreation(cityMapsFolder);
            string filename = cityMapsFolder + Path.DirectorySeparatorChar + $"{cityName}_{currentTime.ToString("yyyy-MM-dd-HH-mm-ss")}.html";
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            StreamWriter sr = File.CreateText(filename);
            var cto = new CityTransportObject
            {
                transportLines = transportLines,
                stations = stations
            };
            sr.WriteLine(svg.GetResult(cto, cityName, currentTime));
            sr.Close();
            return filename;
        }

        private static int GetLineUID(ushort lineId) => lineId;


        private delegate Vector2 CalculateCoords(Vector3 pos, float invPrecision);
    }

    public class CityTransportObject
    {
        public Dictionary<ushort, TLMMapTransportLine> transportLines;
        public List<Station> stations;
        public string ToJson() => $@"{{
""transportLines"":{{{string.Join(",\n", transportLines.Select(x => $"\"{x.Key}\":{x.Value.ToJson()}").ToArray())}}},
""stations"":[{string.Join(",\n", stations.Select((x,i) => x.ToJson()).ToArray())}]
}}";
    }

    public class Station
    {

        public string name;
        private Vector2 originalCentralPos;
        public Vector2 centralPos;
        public Vector2 finalPos;
        public int linesPassingCount => linesPassing.Count;
        public Dictionary<ushort, Vector3> stopsWithWorldPos = new Dictionary<ushort, Vector3>();
        public ushort stopId;
        public Vector2 WritePoint => centralPos + lastPoint;
        public float WriteAngle
        {
            get
            {
                CardinalPoint direction = CardinalPoint.E;
                for (int i = 0; i < 8; i++)
                {
                    if (stationConnections.ContainsKey(direction))
                    {
                        direction++;
                    }
                    else
                    {
                        break;
                    }
                }
                return direction.GetCardinalAngle() - 90;
            }
        }

        public List<ushort> linesPassing { get; private set; } = new List<ushort>();
        private Vector2 lastPoint = Vector2.zero;
        public int id { get; private set; }
        private List<int> optimizedWithStationsId = new List<int>();
        private ItemClass.Service service;
        private Dictionary<CardinalPoint, Station> stationConnections = new Dictionary<CardinalPoint, Station>();
        private string districtName;
        private ushort districtId;

        public string ToJson() => $@"{{""name"":""{name}"",""originalCentralPos"":[{originalCentralPos.x},{originalCentralPos.y}],""centralPos"":[{centralPos.x},{centralPos.y}],
                ""finalPos"":[{finalPos.x},{finalPos.y}],""linesPassingCount"":{linesPassingCount},""stops"":{{{string.Join(",", stopsWithWorldPos.Select(x => $@"""{x.Key}"":[{x.Value.x},{x.Value.y},{x.Value.z}]").ToArray())}}},
                ""stopId"":{stopId},""writePoint"":[{WritePoint.x},{WritePoint.y}],""writeAngle"":{WriteAngle},""linesPassing"":[{string.Join(",", linesPassing.Select(x => x.ToString()).ToArray())}],
                ""id"":{id},""service"":""{service}"",""districtId"":{districtId},""districtName"":""{districtName}""}}";

        public Station(string n, Vector2 pos, Vector3 worldPos, Dictionary<ushort, Vector3> stops, int stationId, ItemClass.Service service, ushort stopId, ushort lineId) : this(n, pos, worldPos, stops, stationId, service, stopId) => AddLine(lineId);
        public Station(string n, Vector2 pos, Vector3 worldPos, Dictionary<ushort, Vector3> stops, int stationId, ItemClass.Service service, ushort stopId)
        {
            name = n;
            originalCentralPos = pos;
            centralPos = pos;
            stopsWithWorldPos = stops;
            id = stationId;
            this.stopId = stopId;
            this.service = service;
            DistrictManager dm = Singleton<DistrictManager>.instance;
            districtId = dm.GetDistrict(worldPos);
            districtName = dm.GetDistrictName(districtId);
        }

        public void AddLine(ushort lineId)
        {
            if (!linesPassing.Contains(lineId))
            {
                linesPassing.Add(lineId);
            }
        }

        public int GetLineIdx(ushort lineId) => linesPassing.IndexOf(lineId);

        public CardinalPoint getDirectionForStation(Station s) => stationConnections.FirstOrDefault(x => x.Value.stopId == s.stopId).Key;


        public Vector2 getPositionForLine(ushort lineIndex, Vector2 to) => originalCentralPos;


        public CardinalPoint reserveExit(Station s2)
        {

            if (stopsWithWorldPos.ContainsKey(s2.stopId) || s2.stopsWithWorldPos.ContainsKey(stopId))
            {
                return CardinalPoint.ZERO;
            }
            KeyValuePair<CardinalPoint, Station> s = stationConnections.FirstOrDefault(x => x.Value.stopsWithWorldPos.ContainsKey(s2.stopId));
            if (!s.Equals(default(KeyValuePair<CardinalPoint, Station>)))
            {
                return s.Key;
            }

            var direction = CardinalPoint.GetCardinal2D(centralPos, s2.centralPos);
            return direction;
        }


        public List<ushort> getAllStationOffsetPoints() => linesPassing;

    }

}

