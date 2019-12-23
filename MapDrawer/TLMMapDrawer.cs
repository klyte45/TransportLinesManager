using ColossalFramework;
using Klyte.Commons.i18n;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
/**
   OBS: Mudar a forma de pintar as linhas, criar caminhos comuns entre estações iguais (ajuda no tram)
   ver como fazer linhas de sentido único no tram
*/
namespace Klyte.TransportLinesManager.MapDrawer
{
    public class TLMMapDrawer
    {

        private static Color almostWhite = new Color(0.9f, 0.9f, 0.9f);

        public static void drawCityMap()
        {

            TLMController controller = TLMController.instance;
            var linesByType = new Dictionary<TransportInfo.TransportType, List<ushort>>();

            foreach (object type in Enum.GetValues(typeof(TransportInfo.TransportType)))
            {
                linesByType[(TransportInfo.TransportType) type] = new List<ushort>();
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
            var transportLines = new Dictionary<ushort, MapTransportLine>();
            foreach (TransportInfo.TransportType tt in linesByType.Keys)
            {
                if (!linesByType.ContainsKey(tt))
                {
                    continue;
                }

                foreach (ushort lineId in linesByType[tt])
                {
                    TransportLine t = TransportManager.instance.m_lines.m_buffer[lineId];
                    float range = 75f;
                    switch (tt)
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
                    transportLines[lineId] = new MapTransportLine(color, day, night, lineId);
                    int startStop = 0;
                    int finalStop = stopsCount;

                    for (int j = startStop; j < finalStop; j++)
                    {
                        //						Debug.Log ("ULT POS:" + ultPos);
                        ushort nextStop = t.GetStop(j % stopsCount);
                        string name = TLMLineUtils.getStationName(nextStop, lineId, t.Info.m_stationSubService, out ItemClass.Service service, out ItemClass.SubService nil2, out string prefix, out ushort buildingId, out NamingType namingType);

                        Vector3 worldPos = TLMLineUtils.getStationBuildingPosition(nextStop, t.Info.m_stationSubService);
                        Vector2 pos2D = calc(worldPos, invPrecision);
                        Vector2 gridAdd = Vector2.zero;


                        Station idx = stations.FirstOrDefault(x => x.stopsWithWorldPos.ContainsKey(nextStop) || x.centralPos == pos2D);
                        if (idx != null)
                        {
                            transportLines[lineId].addStation(ref idx);
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
                            TLMUtils.doLog("Station: ${0}; nearStops: ${1}", name, string.Join(",", nearStops.Select(x => x.ToString()).ToArray()));
                            var thisStation = new Station(name, pos2D, worldPos, nearStops, nextStationId++, service, nextStop);
                            stations.Add(thisStation);
                            transportLines[lineId].addStation(ref thisStation);
                        }
                        if (!positions.ContainsKey((int) pos2D.x))
                        {
                            positions[(int) pos2D.x] = new List<int>();
                        }
                        positions[(int) pos2D.x].Add((int) pos2D.y);
                        //						Debug.Log ("POS:" + pos);
                        ultPos = pos2D;
                    }
                }
            }
            printToSVG(stations, transportLines, Singleton<SimulationManager>.instance.m_metaData.m_CityName, Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier, Singleton<SimulationManager>.instance.m_currentGameTime);
            //printToJson(stations, transportLines, Singleton<SimulationManager>.instance.m_metaData.m_CityName + "_" + Singleton<SimulationManager>.instance.m_currentGameTime.ToString("yyyy.MM.dd"));
        }


        public static string getJson(List<Station> stations, Dictionary<ushort, MapTransportLine> transportLines)
        {
            var cto = new CityTransportObject
            {
                transportLines = transportLines
            };

            return cto.toJson();
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
        };

        public static string printToSVG(List<Station> stations, Dictionary<ushort, MapTransportLine> transportLines, string cityName, string cityId, DateTime currentTime)
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
            return drawSVG(stations, transportLines, cityName, cityId, currentTime, minX, minY, maxX, maxY);
        }

        private static string drawSVG(List<Station> stations, Dictionary<ushort, MapTransportLine> transportLines, string cityName, string cityId, DateTime currentTime, float minX, float minY, float maxX, float maxY)
        {
            float maxRadius = Math.Max(stations.Max(x => x.getAllStationOffsetPoints().Count) * 2 + 2, 10);

            var svg = new SVGTemplate((int) ((maxY - minY + 16)), (int) ((maxX - minX + 16)), maxRadius, minX - maxRadius, minY - maxRadius);

            var linesOrdened = transportLines.OrderBy(x => getLineUID(x.Key)).ToList();
            //ordena pela quantidade de linhas passando
            stations = stations.OrderBy(x => x.linesPassingCount).ToList();

            //calcula as posições de todas as estações no mapa
            foreach (KeyValuePair<ushort, MapTransportLine> line in linesOrdened)
            {
                Station station0 = line.Value[0];
                Vector2 prevPos = station0.getPositionForLine(line.Key, line.Value[1].centralPos);
                for (int i = 1; i < line.Value.stationsCount(); i++)
                {
                    prevPos = line.Value[i].getPositionForLine(line.Key, prevPos);
                }
            }
            //adiciona as exceções
            svg.addStationsToExceptionMap(stations);
            //pinta as linhas
            foreach (KeyValuePair<ushort, MapTransportLine> line in linesOrdened)
            {
                svg.addTransportLine(line.Value, line.Key);
            }
            svg.drawAllLines();
            foreach (Station station in stations)
            {
                svg.addStation(station, transportLines);
            }
            string cityMapsFolder = TLMController.exportedMapsFolder + Path.DirectorySeparatorChar + $"{cityName} ({cityId})";
            FileInfo fipalette = FileUtils.EnsureFolderCreation(cityMapsFolder);
            string filename = cityMapsFolder + Path.DirectorySeparatorChar + currentTime.ToString("yyyy-MM-dd-HH-mm-ss") + ".html";
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            StreamWriter sr = File.CreateText(filename);
            var cto = new CityTransportObject
            {
                transportLines = transportLines
            };
            sr.WriteLine(svg.getResult(cto, cityName, currentTime));
            sr.Close();
            return filename;
        }

        private static int getLineUID(ushort lineId) => lineId;


        private delegate Vector2 CalculateCoords(Vector3 pos, float invPrecision);
    }

    public class CityTransportObject
    {
        public Dictionary<ushort, MapTransportLine> transportLines;
        public string toJson() => $"{{\"transportLines\":{{{string.Join(",", transportLines.Select(x => $"\"{x.Key}\":{x.Value.toJson()}").ToArray())}}}}}";
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
        public Vector2 writePoint => centralPos + lastPoint;
        public float writeAngle
        {
            get {
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

        public string toJson()
        {
            return $@"{{""name"":""{name}"",""originalCentralPos"":[{originalCentralPos.x},{originalCentralPos.y}],""centralPos"":[{centralPos.x},{centralPos.y}],
                ""finalPos"":[{finalPos.x},{finalPos.y}],""linesPassingCount"":{linesPassingCount},""stops"":{{{string.Join(",", stopsWithWorldPos.Select(x => $@"""{x.Key}"":[{x.Value.x},{x.Value.y},{x.Value.z}]").ToArray())}}},
                ""stopId"":{stopId},""writePoint"":[{writePoint.x},{writePoint.y}],""writeAngle"":{writeAngle},""linesPassing"":[{string.Join(",", linesPassing.Select(x => x.ToString()).ToArray())}],
                ""id"":{id},""service"":""{service}"",""districtId"":{districtId},""districtName"":""{districtName}""}}";
        }

        public Station(string n, Vector2 pos, Vector3 worldPos, Dictionary<ushort, Vector3> stops, int stationId, ItemClass.Service service, ushort stopId, ushort lineId) : this(n, pos, worldPos, stops, stationId, service, stopId) => addLine(lineId);
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

        public void addLine(ushort lineId)
        {
            if (!linesPassing.Contains(lineId))
            {
                linesPassing.Add(lineId);
            }
        }

        public int getLineIdx(ushort lineId) => linesPassing.IndexOf(lineId);

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
            CardinalPoint directionOr = direction;
            if (stationConnections.Count >= 8)
            {
                return direction;
            }
            var directionAlt = CardinalPoint.GetCardinal2D4(centralPos, s2.centralPos);

            bool isForward = direction > directionAlt;

            if (stationConnections.ContainsKey(direction))
            {
                if (isForward)
                {
                    direction++;
                }
                else
                {
                    direction--;
                }
            }

            if (stationConnections.ContainsKey(direction))
            {
                direction = directionOr;
                if (isForward)
                {
                    direction--;
                }
                else
                {
                    direction++;
                }
            }

            stationConnections[direction] = s2;
            return direction;
        }


        public List<ushort> getAllStationOffsetPoints() => linesPassing;

    }

    public class SVGTemplate
    {

        /// <summary>
        /// The header.<>
        /// 0 = Height
        /// 1 = Width
        /// </summary>
        public string getHtmlHeader(int height, int width, CityTransportObject cto)
        {
            return $@"
             <!DOCTYPE html><html><head> <meta charset='UTF-8'> 
             <style>{KlyteResourceLoader.LoadResourceString("MapDrawer.lineDrawBasicCss.css") }</style>
             <script src=""https://code.jquery.com/jquery-3.3.1.min.js"" integrity=""sha256-FgpCb/KJQlLNfOu91ta32o/NMZxltwRo8QtmkMRdAu8="" crossorigin=""anonymous""></script>
             <script>var _infoLines = {cto.toJson()};</script>
             <script>{KlyteResourceLoader.LoadResourceString("MapDrawer.app.js") }</script>
             </head><body>
             <style id=""styleSelectionLineMap""></style>
             <svg id=""map"" height='{height}' width='{width}'>
             <defs>
             <marker orient=""auto"" markerHeight=""6"" markerWidth=""6"" refY=""2.5"" refX=""1"" viewBox=""0 0 10 5"" id=""Triangle1""><path d=""M 0 0 L 10 2.5 L 0 5 z""/></marker>
             <marker orient=""auto"" markerHeight=""6"" markerWidth=""6"" refY=""2.5"" refX=""1"" viewBox=""0 0 10 5"" id=""Triangle2""><path d=""M 10 0 L 0 2.5 L 10 5 z""/></marker>
             </defs>";

        }
        public string getHtmlFooter(string cityName, DateTime date) => $@"<div id=""linesPanel""><div id=""title"">{cityName}</div><div id=""date"">{date.ToString(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => c.TwoLetterISOLanguageName == KlyteLocaleManager.CurrentLanguageId).FirstOrDefault())}</div><div id=""content""></div></body></html>";
        /// <summary>
        /// The line segment. <>
        /// 0 = X1 
        /// 1 = Y1
        /// 2 = X2 
        /// 3 = Y2
        /// 4 = R
        /// 5 = G 
        /// 6 = B 
        /// </summary>
        //private string getLineSegmentTemplate(float x1, float x2, float y1, float y2, Color32 lineColor)
        //{
        //    return $@"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' style='stroke:rgb({lineColor.r},{lineColor.g},{lineColor.b});stroke-width:{multiplier}' stroke-linecap='round'/>";
        //}
        /// <summary>
        /// The line
        /// 0 = path;
        /// 1 = R;
        /// 2 = G;
        /// 3 = B;
        /// 4 = Line type;
        /// </summary>
        // private readonly string pathLine = "<path d='{0}' class=\"path{4}\" style='stroke:rgb({1},{2},{3});' stroke-linejoin=\"round\" stroke-linecap=\"round\"/>";
        ///// <summary>
        ///// The integration.<>
        ///// 0 = X 
        ///// 1 = Y 
        ///// 2 = Ang (º) 
        ///// 3 = Offset 
        ///// </summary>
        //		private readonly string integration = "<g transform=\" translate({0},{1}) rotate({2}, {0},{1})\">" +
        //			"<line x1=\"0\" y1=\"0\" x2=\"{3}\" y2=\"0\" style=\"stroke:rgb(155,155,155);stroke-width:30\" stroke-linecap=\"round\"/>" +
        //			"<circle cx=\"0\" cy=\"0\" r=\"12\" fill=\"white\" />" +
        //			"<circle cx=\"{3}\" cy=\"0\" r=\"12\" fill=\"white\" />" +
        //			"</g>";
        /// <summary>
        /// The station.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Ang (º) 
        /// 3 = Station Name 
        /// </summary>
        //private static string getStationTemplate()
        //{
        //    return "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
        //            "<circle cx=\"0\" cy=\"0\" r=\"" + maxRadius * 0.4 + "\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
        //            "<text x=\"" + maxRadius * 0.8 + "\" y=\"" + maxRadius / 6 + "\" fill=\"black\" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
        //            "</g>";
        //}

        private static string getStationPointTemplate(int idx, Color32 lineColor, float tx, float ty, int lineId, int stationId)
        {
            int baseRadius = 3;
            int additionalRadius = 2;
            return $@"<circle style=""stroke:rgb({ lineColor.r },{lineColor.g },{ lineColor.b }); stroke-width:2"" fill=""{ (idx == 0 ? "white" : "transparent") }"" r=""{ (baseRadius + additionalRadius * idx) }"" cy=""0"" cx=""0"" transform=""translate({tx},{ty})"" class=""_lid_{lineId} _sid_{stationId}"" />";
        }
        //   private readonly string stationNameTemplate = "<text transform=\"rotate({2},{0},{1}) translate({0},{1})\" x=\"" + RADIUS * 0.8 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" style=\"text-shadow: 0px 0px 6px white;\">{3}</text>";
        private string getStationNameTemplate(string name, int stops, int stationId, float x, float y, float rotationDeg, List<ushort> lines)
        {
            float translate = stops + 2;
            return $"<div class='stationContainer {string.Join(" ", lines.Select(z => "_lid_" + z).ToArray())}' style='top: {y}px; left: {x}px;' ><p style='transform: rotate({rotationDeg}deg) translate({ translate }px, { translate }px) ;'>{name}</p></div>";
        }
        private string getStationNameInverseTemplate(string name, int stops, int stationId, float x, float y, float rotationDeg, List<ushort> lines)
        {
            float translate = stops + 2;
            return $"<div class='stationContainer {string.Join(" ", lines.Select(z => "_lid_" + z).ToArray())}' style='top: {y}px; left: {x}px;' ><p style='transform: rotate({rotationDeg}deg) translate({ translate }px, { translate }px) ;'><y>{name}</y></p></div>";
        }
        private string getStationNameVerticalTemplate(string name, int stops, int stationId, float x, float y, float rotationDeg, List<ushort> lines)
        {
            float translate = stops + 2;
            return $"<div class='stationContainer {string.Join(" ", lines.Select(z => "_lid_" + z).ToArray())}' style='top: {y}px; left: {x}px;' ><p style='transform: rotate({rotationDeg}deg) translate({ translate }px, { translate }px) ;'><x  style='transform: rotate({-rotationDeg}deg)'>{name}</x></p></div>";
        }


        /// <summary>
        /// The metro line symbol.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Line Name 
        /// 3 = R
        /// 4 = G
        /// 5 = B
        /// </summary>
        //private readonly string metroLineSymbol = "<g transform=\"translate({0},{1})\">" +
        //    "  <rect x=\"" + -maxRadius + "\" y=\"" + -maxRadius + "\" width=\"" + maxRadius * 2 + "\" height=\"" + maxRadius * 2 + "\" fill=\"rgb({3},{4},{5})\" stroke=\"black\" stroke-width=\"1\" />" +
        //    "<text x=\"0\" y=\"" + maxRadius / 3 + "\" fill=\"white\"  stroke=\"black\" stroke-width=\"0.5\" style=\"font-size:" + maxRadius + "px\"   text-anchor=\"middle\">{2}</text>" +
        //    "</g>";


        /// <summary>
        /// The train line symbol.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Line Name 
        /// 3 = R
        /// 4 = G
        /// 5 = B
        /// </summary>
        //private readonly string trainLineSymbol = "<g transform=\"translate({0},{1})\">" +
        //    "<circle cx=\"0\" cy=\"0\"r=\"" + maxRadius + "\" fill=\"rgb({3},{4},{5})\" stroke=\"black\" stroke-width=\"1\" />" +
        //    "<text x=\"0\" y=\"" + maxRadius / 3 + "\" fill=\"white\"  stroke=\"black\" stroke-width=\"0.5\" style=\"font-size:" + maxRadius + "px\"   text-anchor=\"middle\">{2}</text>" +
        //    "</g>";
        /// <summary>
        /// The footer.
        /// </summary>



        private LineSegmentStationsManager segmentManager = new LineSegmentStationsManager();
        private StringBuilder svgPart = new StringBuilder();
        private StringBuilder htmlStationsPart = new StringBuilder();
        private float multiplier;
        private int height;
        private int width;




        private Vector2 offset;

        public SVGTemplate(int width, int height, float multiplier = 1, float offsetX = 0, float offsetY = 0)
        {
            this.multiplier = multiplier;
            this.width = (int) (width * multiplier);
            this.height = (int) (height * multiplier);
            offset = new Vector2(offsetX, offsetY);
        }

        public string getResult(CityTransportObject cto, string cityName, DateTime currentTime)
        {
            var document = new StringBuilder(getHtmlHeader(width, height, cto));
            document.Append(svgPart);
            document.Append("</svg><div id=\"stationsContainer\">");
            document.Append(htmlStationsPart);
            document.Append("</div>");
            document.Append(getHtmlFooter(cityName, currentTime));
            return document.ToString();
        }

        public void addStationsToExceptionMap(List<Station> stations)
        {
            foreach (Station s in stations)
            {
                segmentManager.addStationToAllRangeMaps(s.centralPos);
            }
        }

        public void addStation(Station s, Dictionary<ushort, MapTransportLine> lines)
        {
            bool inverse = false;
            bool vertical = false;
            string name = s.name;
            float angle = s.writeAngle;
            switch (CardinalPoint.GetCardinalPoint(angle).Value)
            {
                case CardinalPoint.CardinalInternal.SW:
                case CardinalPoint.CardinalInternal.S:
                case CardinalPoint.CardinalInternal.SE:
                    inverse = true;
                    break;
                case CardinalPoint.CardinalInternal.E:
                case CardinalPoint.CardinalInternal.W:
                    vertical = true;
                    break;
            }

            foreach (ushort pos in s.getAllStationOffsetPoints())
            {
                Vector2 point = s.centralPos - offset;// + pos.Key;
                MapTransportLine line = lines[pos];
                svgPart.Append(getStationPointTemplate(s.getLineIdx(pos), line.lineColor, point.x * multiplier, point.y * multiplier, line.lineId, s.id));
            }
            Vector2 namePoint = s.writePoint - offset;
            htmlStationsPart.Append(vertical ?
                getStationNameVerticalTemplate(s.name, s.getAllStationOffsetPoints().Count, s.id, (namePoint.x + 0.5f) * multiplier, ((namePoint.y + 0.5f) * multiplier), angle, s.linesPassing) : inverse ?
                getStationNameInverseTemplate(s.name, s.getAllStationOffsetPoints().Count, s.id, (namePoint.x + 0.5f) * multiplier, ((namePoint.y + 0.5f) * multiplier), angle, s.linesPassing) :
                getStationNameTemplate(s.name, s.getAllStationOffsetPoints().Count, s.id, (namePoint.x + 0.5f) * multiplier, ((namePoint.y + 0.5f) * multiplier), angle, s.linesPassing));

        }

        public void addTransportLine(MapTransportLine points, ushort transportLineIdx)
        {
            int count = points.stationsCount();
            for (int i = 1; i <= count; i++)
            {
                Station s1 = points[i - 1];
                Station s2 = points[i % count];
                segmentManager.addLine(s1, s2, points, LineSegmentStationsManager.Direction.S1_TO_S2);
            }
        }

        internal void drawAllLines()
        {
            TransportLine[] tls = Singleton<TransportManager>.instance.m_lines.m_buffer;
            List<LineSegmentStationsManager.LineSegmentStations> segments = segmentManager.getSegments();
            var segmentDict = new List<Tuple<float, string>>();
            foreach (LineSegmentStationsManager.LineSegmentStations segment in segments)
            {
                List<Vector2> basePoints = segment.path;
                for (int i = 0; i < basePoints.Count; i++)
                {
                    basePoints[i] = (basePoints[i] - offset) * multiplier;
                }
                float offsetNeg = 0;
                float offsetPos = 0;
                var dir = CardinalPoint.GetCardinal2D(segment.s1.centralPos, segment.s2.centralPos);
                dir++;
                dir++;
                Vector2 offsetDir = dir.GetCardinalOffset2D();
                foreach (KeyValuePair<MapTransportLine, LineSegmentStationsManager.Direction> line in segment.lines)
                {
                    float width = 0;
                    TransportInfo.TransportType tt = tls[line.Key.lineId].Info.m_transportType;
                    switch (tt)
                    {
                        case TransportInfo.TransportType.Tram:
                        case TransportInfo.TransportType.Bus:
                            width = 1;
                            break;
                        case TransportInfo.TransportType.Train:
                            width = 5;
                            break;
                        case TransportInfo.TransportType.Metro:
                            width = 2.5f;
                            break;
                        case TransportInfo.TransportType.Monorail:
                            width = 4;
                            break;
                        case TransportInfo.TransportType.Ship:
                            width = 8;
                            break;
                    }
                    float coordMultiplier = 0;
                    if (offsetNeg > offsetPos)
                    {
                        coordMultiplier = (offsetPos + width / 2);
                        offsetPos += width;
                    }
                    else if (offsetNeg < offsetPos)
                    {
                        coordMultiplier = -(offsetNeg + width / 2);
                        offsetNeg += width;
                    }
                    else
                    {
                        offsetPos = width / 2;
                        offsetNeg = width / 2;
                    }

                    Vector2 lineTotalOffset = offsetDir * coordMultiplier * 2;
                    var points = new Vector2[basePoints.Count];
                    for (int i = 0; i < basePoints.Count; i++)
                    {
                        if (i == 0)
                        {
                            CardinalPoint cp = segment.s1.getDirectionForStation(segment.s2);
                            cp++;
                            cp++;
                            points[i] = basePoints[i] + cp.GetCardinalOffset2D() * coordMultiplier * 2;
                        }
                        else if (i == basePoints.Count - 1)
                        {
                            CardinalPoint cp = segment.s2.getDirectionForStation(segment.s1);
                            cp++;
                            cp++;
                            points[i] = basePoints[i] + cp.GetCardinalOffset2D() * coordMultiplier * 2;
                        }
                        else
                        {
                            points[i] = basePoints[i] + lineTotalOffset;
                        }
                    }
                    segmentDict.Add(Tuple.New(width, getLineElement(points, line.Key, tt)));
                }

            }
            segmentDict.Sort((x, y) => (int) (y.First - x.First));
            svgPart.Append(string.Join("\n", segmentDict.Select(x => x.Second).ToArray()));

        }

        private string getLineElement(Vector2[] points, MapTransportLine line, TransportInfo.TransportType tt) => $@"<polyline points=""{string.Join(",", points.Select(x => "" + x.x + "," + x.y).ToArray())}"" class=""path{tt.ToString()} _lid_{line.lineId}"" style='stroke:rgb({ line.lineColor.r},{ line.lineColor.g},{ line.lineColor.b});' stroke-linejoin=""round"" stroke-linecap=""round""/>";





        //public void addMetroLineIndication(Vector2 point, string name, Color32 color)
        //{
        //    point -= offset;
        //    svgPart.AppendFormat(metroLineSymbol, point.x * multiplier, (point.y * multiplier), name, color.r, color.g, color.b);
        //}

        //public void addTrainLineSegment(Vector2 point, string name, Color32 color)
        //{
        //    point -= offset;
        //    svgPart.AppendFormat(trainLineSymbol, point.x * multiplier, (point.y * multiplier), name, color.r, color.g, color.b);
        //}
    }

}

