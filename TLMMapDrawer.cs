using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMMapDrawer
    {

        private static Color almostWhite = new Color(0.9f, 0.9f, 0.9f);

        public static void drawCityMap()
        {
            CalculateCoords calc = TLMLineUtils.gridPosition81Tiles;
            NetManager nm = NetManager.instance;
            TLMController controller = TLMController.instance;
            List<Station> stations = new List<Station>();
            Dictionary<Segment2, Color32> svgLines = new Dictionary<Segment2, Color32>();
            Dictionary<ushort, List<Station>> transportLines = new Dictionary<ushort, List<Station>>();
            MultiMap<Vector2, Vector2> intersects = new MultiMap<Vector2, Vector2>();
            //			List<int> usedX = new List<int> ();
            //			List<int> usedY = new List<int> ();
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            for (ushort i = 0; i < controller.tm.m_lines.m_size; i++)
            {
                TransportLine t = controller.tm.m_lines.m_buffer[(int)i];
                if (t.m_lineNumber > 0 && (t.Info.m_transportType == TransportInfo.TransportType.Metro || t.Info.m_transportType == TransportInfo.TransportType.Train))
                {

                    int stopsCount = t.CountStops(i);
                    if (stopsCount == 0)
                    {
                        continue;
                    }
                    Color color = t.m_color;
                    Vector2 ultPos = Vector2.zero;
                    transportLines[i] = new List<Station>();
                    int startStop = 0;
                    int finalStop = stopsCount;
                    int middle = 0;
                    if (TLMUtils.CalculateSimmetry(t.Info.m_stationSubService, stopsCount, t, out middle))
                    {
                        startStop = middle;
                        finalStop = middle + stopsCount / 2 + 1;
                    }
                    for (int j = startStop; j < finalStop; j++)
                    {
                        //						Debug.Log ("ULT POS:" + ultPos);
                        ushort nextStop = t.GetStop(j % stopsCount);
                        string name = TLMUtils.getStationName(nextStop, t.Info.m_stationSubService);

                        Vector3 worldPos = TLMUtils.getStationBuildingPosition(nextStop, t.Info.m_stationSubService);
                        Vector2 pos2D = calc(worldPos);
                        Vector2 gridAdd = Vector2.zero;

                        var idx = stations.FirstOrDefault(x => x.stops.Contains(nextStop));
                        if (idx != null)
                        {
                            transportLines[i].Add(idx);
                        }
                        else
                        {
                            List<ushort> nearStops = new List<ushort>();
                            TLMLineUtils.GetNearStopPoints(worldPos, 100f, ref nearStops);
                            TLMUtils.doLog("Station: ${0}; nearStops: ${1}", name, string.Join(",", nearStops.Select(x => x.ToString()).ToArray()));
                            Station thisStation = new Station(name, pos2D, nearStops);
                            stations.Add(thisStation);
                            transportLines[i].Add(thisStation);
                            if (pos2D.x > maxX)
                            {
                                maxX = pos2D.x;
                            }
                            if (pos2D.y > maxY)
                            {
                                maxY = pos2D.y;
                            }
                            if (pos2D.x < minX)
                            {
                                minX = pos2D.x;
                            }
                            if (pos2D.y < minY)
                            {
                                minY = pos2D.y;
                            }
                        }
                        //						Debug.Log ("POS:" + pos);
                        ultPos = pos2D;
                    }
                }
            }

            SVGTemplate svg = new SVGTemplate((int)((maxY - minY + 16) * SVGTemplate.RADIUS), (int)((maxX - minX + 16) * SVGTemplate.RADIUS), SVGTemplate.RADIUS, minX - 8, minY - 8);

            var linesOrdened = transportLines.OrderBy(x => getLineUID(x.Key)).ToList();
            //calcula as posições de todas as estações no mapa
            foreach (var line in linesOrdened)
            {
                var station0 = line.Value[0];
                var prevPos = station0.getPositionForLine(line.Key, line.Value[1].defaultCentralPos);
                for (int i = 1; i < line.Value.Count; i++)
                {
                    prevPos = line.Value[i].getPositionForLine(line.Key, prevPos);
                }
            }
            //adiciona as exceções
            svg.addStationsToExceptionMap(stations);
            //pinta as linhas
            foreach (var line in linesOrdened)
            {
                svg.addTransportLineOnMap(line.Value, line.Key);
            }
            foreach (var intersectKey in intersects.Keys)
            {
                List<Vector2> intersections;
                intersects.TryGetValue(intersectKey, out intersections);
                foreach (var intersect in intersections)
                {
                    svg.addLineSegment(intersectKey, intersect, Color.gray);
                }
            }
            foreach (var station in stations)
            {
                svg.addStation(station);
            }
            String folder = "Transport Lines Manager";
            if (File.Exists(folder) && (File.GetAttributes(folder) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                File.Delete(folder);
            }
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            String filename = folder + Path.DirectorySeparatorChar + "TLM_MAP_" + Singleton<SimulationManager>.instance.m_metaData.m_CityName + "_" + Singleton<SimulationManager>.instance.m_currentGameTime.ToString("yyyy.MM.dd") + ".html";
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            var sr = File.CreateText(filename);
            sr.WriteLine(svg.getResult());
            sr.Close();
        }

        private static int getLineUID(ushort lineId)
        {
            var t = TLMController.instance.tm.m_lines.m_buffer[lineId];
            int result = ((int)t.Info.m_transportType << 16) | t.m_lineNumber;
            return result;
        }

        public class Station
        {
            public string name
            {
                get; set;
            }
            private Vector2 centralPos
            {
                get; set;
            }
            public Vector2 defaultCentralPos
            {
                get
                {
                    return centralPos;
                }
            }
            public List<ushort> stops
            {
                get; set;
            }
            public Vector2 writePoint
            {
                get
                {
                    return lastPoint;
                }
            }
            public float writeAngle
            {
                get
                {
                    return CardinalPoint.getCardinalPoint(Vector2.Angle(centralPos, lastPoint)).getCardinalAngle();
                }
            }

            private Vector2 lastPoint;

            public Station(string n, Vector2 pos, List<ushort> stops)
            {
                name = n;
                centralPos = pos;
                this.stops = stops;
                lastPoint = pos;
            }

            private Dictionary<Vector2, ushort> stopsPos = new Dictionary<Vector2, ushort>();

            public Vector2 getPositionForLine(ushort lineIndex, Vector2 to)
            {
                Vector2 from = centralPos;

                TLMUtils.doLog("SVGTEMPLATE:getPositionForLine() (STATION: {2}) lineIndex: {0}; from: {1}; stopPos: [{3}]", lineIndex, from, name, string.Join(",", stopsPos.Select(x => x.Key.ToString() + " = " + x.Value).ToArray()));
                if (stopsPos.ContainsValue(lineIndex))
                {
                    return stopsPos.First(x => x.Value == lineIndex).Key;
                }
                if (stopsPos.Count == 0)
                {
                    stopsPos.Add(centralPos, lineIndex);
                    return centralPos;
                }
                else
                {
                    var direction = to - from;
                    direction = new Vector2(Math.Sign(direction.x), Math.Sign(direction.y));
                    var totalOffset = direction;
                    TLMUtils.doLog("SVGTEMPLATE:getPositionForLine() direction: {0}; from: {1}; to: ", direction, from, to);
                    while (stopsPos.ContainsKey(centralPos + totalOffset))
                    {
                        totalOffset += direction;
                    }
                    lastPoint = centralPos + totalOffset;
                    stopsPos.Add(lastPoint, lineIndex);
                    return lastPoint;
                }
            }

            public string getIntegrationLinePath(Vector2 offset, float multiplier)
            {
                if (stopsPos.Count <= 1) return string.Empty;
                StringBuilder result = new StringBuilder();
                Vector2 from = centralPos - offset;
                foreach (Vector2 point in stopsPos.Keys)
                {
                    if (point != centralPos)
                    {
                        Vector2 to = point - offset;
                        result.Append(string.Format(" M {0},{1} L {2},{3} ", from.x * multiplier, from.y * multiplier, to.x * multiplier, to.y * multiplier));
                    }
                }
                return result.ToString();
            }


            public Dictionary<Vector2, ushort> getAllStationPoints()
            {
                return stopsPos;
            }

        }
        private delegate Vector2 CalculateCoords(Vector3 pos);
    }

    public struct CardinalPoint
    {
        public static CardinalPoint getCardinalPoint(float angle)
        {
            angle %= 360;
            angle += 360;
            angle %= 360;

            if (angle < 157.5f && angle >= 112.5f)
            {
                return CardinalPoint.NW;
            }
            else if (angle < 112.5f && angle >= 67.5f)
            {
                return CardinalPoint.N;
            }
            else if (angle < 67.5f && angle >= 22.5f)
            {
                return CardinalPoint.NE;
            }
            else if (angle < 22.5f || angle >= 337.5f)
            {
                return CardinalPoint.E;
            }
            else if (angle < 337.5f && angle >= 292.5f)
            {
                return CardinalPoint.SE;
            }
            else if (angle < 292.5f && angle >= 247.5f)
            {
                return CardinalPoint.S;
            }
            else if (angle < 247.5f && angle >= 202.5f)
            {
                return CardinalPoint.SW;
            }
            else {
                return CardinalPoint.W;
            }

        }

        public static CardinalPoint getCardinalPoint4(float angle)
        {
            angle %= 360;
            angle += 360;
            angle %= 360;

            if (angle < 135f && angle >= 45f)
            {
                return CardinalPoint.N;
            }
            else if (angle < 45f || angle >= 315f)
            {
                return CardinalPoint.E;
            }
            else if (angle < 315f && angle >= 225f)
            {
                return CardinalPoint.S;
            }
            else {
                return CardinalPoint.W;
            }
        }

        private CardinalInternal InternalValue { get; set; }

        public CardinalInternal Value { get { return InternalValue; } }

        public static readonly CardinalPoint N = CardinalInternal.N;
        public static readonly CardinalPoint E = CardinalInternal.E;
        public static readonly CardinalPoint S = CardinalInternal.S;
        public static readonly CardinalPoint W = CardinalInternal.W;
        public static readonly CardinalPoint NE = CardinalInternal.NE;
        public static readonly CardinalPoint SE = CardinalInternal.SE;
        public static readonly CardinalPoint SW = CardinalInternal.SW;
        public static readonly CardinalPoint NW = CardinalInternal.NW;
        public static readonly CardinalPoint ZERO = CardinalInternal.ZERO;

        public static implicit operator CardinalPoint(CardinalInternal otherType)
        {
            return new CardinalPoint
            {
                InternalValue = otherType
            };
        }

        public static implicit operator CardinalInternal(CardinalPoint otherType)
        {
            return otherType.InternalValue;
        }

        public int stepsTo(CardinalPoint other)
        {
            if (other.InternalValue == InternalValue) return 0;
            if ((((int)other.InternalValue) & ((int)other.InternalValue - 1)) != 0 || (((int)InternalValue) & ((int)InternalValue - 1)) != 0) return int.MaxValue;
            CardinalPoint temp = other;
            int count = 0;
            while (temp.InternalValue != this.InternalValue)
            {
                temp++;
                count++;
            }
            if (count > 4) count = count - 8;
            return count;
        }

        public static int operator -(CardinalPoint c, CardinalPoint other)
        {
            return c.stepsTo(other);
        }

        public Vector2 getCardinalOffset()
        {
            switch (InternalValue)
            {
                case CardinalPoint.CardinalInternal.E:
                    return new Vector2(1, 0);
                case CardinalPoint.CardinalInternal.W:
                    return new Vector2(-1, 0);
                case CardinalPoint.CardinalInternal.N:
                    return new Vector2(0, 1);
                case CardinalPoint.CardinalInternal.S:
                    return new Vector2(0, -1);
                case CardinalPoint.CardinalInternal.NE:
                    return new Vector2(1, 1);
                case CardinalPoint.CardinalInternal.NW:
                    return new Vector2(-1, 1);
                case CardinalPoint.CardinalInternal.SE:
                    return new Vector2(1, -1);
                case CardinalPoint.CardinalInternal.SW:
                    return new Vector2(-1, -1);
            }
            return Vector2.zero;
        }


        public Vector2 getCardinalOffset2D()
        {
            switch (InternalValue)
            {
                case CardinalPoint.CardinalInternal.E:
                    return new Vector2(1, 0);
                case CardinalPoint.CardinalInternal.W:
                    return new Vector2(-1, 0);
                case CardinalPoint.CardinalInternal.S:
                    return new Vector2(0, 1);
                case CardinalPoint.CardinalInternal.N:
                    return new Vector2(0, -1);
                case CardinalPoint.CardinalInternal.SE:
                    return new Vector2(1, 1);
                case CardinalPoint.CardinalInternal.SW:
                    return new Vector2(-1, 1);
                case CardinalPoint.CardinalInternal.NE:
                    return new Vector2(1, -1);
                case CardinalPoint.CardinalInternal.NW:
                    return new Vector2(-1, -1);
            }
            return Vector2.zero;
        }

        public int getCardinalAngle()
        {
            switch (InternalValue)
            {
                case CardinalPoint.CardinalInternal.E:
                    return 0;
                case CardinalPoint.CardinalInternal.W:
                    return 180;
                case CardinalPoint.CardinalInternal.N:
                    return 90;
                case CardinalPoint.CardinalInternal.S:
                    return 270;
                case CardinalPoint.CardinalInternal.NE:
                    return 45;
                case CardinalPoint.CardinalInternal.NW:
                    return 135;
                case CardinalPoint.CardinalInternal.SE:
                    return 315;
                case CardinalPoint.CardinalInternal.SW:
                    return 225;
            }
            return 0;
        }

        public static CardinalPoint operator ++(CardinalPoint c)
        {
            switch (c.InternalValue)
            {
                case CardinalInternal.N:
                    return NE;
                case CardinalInternal.NE:
                    return E;
                case CardinalInternal.E:
                    return SE;
                case CardinalInternal.SE:
                    return S;
                case CardinalInternal.S:
                    return SW;
                case CardinalInternal.SW:
                    return W;
                case CardinalInternal.W:
                    return NW;
                case CardinalInternal.NW:
                    return N;
                default:
                    return ZERO;
            }
        }

        public static CardinalPoint operator --(CardinalPoint c)
        {
            switch (c.InternalValue)
            {
                case CardinalInternal.N:
                    return NW;
                case CardinalInternal.NE:
                    return N;
                case CardinalInternal.E:
                    return NE;
                case CardinalInternal.SE:
                    return E;
                case CardinalInternal.S:
                    return SE;
                case CardinalInternal.SW:
                    return S;
                case CardinalInternal.W:
                    return SW;
                case CardinalInternal.NW:
                    return W;
                default:
                    return ZERO;
            }
        }

        public static CardinalPoint operator &(CardinalPoint c1, CardinalPoint c2)
        {
            return new CardinalPoint
            {
                InternalValue = c1.InternalValue & c2.InternalValue
            };
        }

        public static CardinalPoint operator |(CardinalPoint c1, CardinalPoint c2)
        {
            return new CardinalPoint
            {
                InternalValue = c1.InternalValue | c2.InternalValue
            };
        }

        public override bool Equals(object o)
        {

            return o.GetType() == GetType() && this == ((CardinalPoint)o);
        }

        public static bool operator ==(CardinalPoint c1, CardinalPoint c2)
        {
            return c1.InternalValue == c2.InternalValue;
        }
        public static bool operator !=(CardinalPoint c1, CardinalPoint c2)
        {
            return c1.InternalValue != c2.InternalValue;
        }

        public static CardinalPoint operator ~(CardinalPoint c)
        {
            switch (c.InternalValue)
            {
                case CardinalInternal.N:
                    return S;
                case CardinalInternal.NE:
                    return SW;
                case CardinalInternal.E:
                    return W;
                case CardinalInternal.SE:
                    return NW;
                case CardinalInternal.S:
                    return N;
                case CardinalInternal.SW:
                    return NE;
                case CardinalInternal.W:
                    return E;
                case CardinalInternal.NW:
                    return SE;
                default:
                    return ZERO;
            };
        }

        public enum CardinalInternal
        {
            N = 1,
            NE = 2,
            E = 4,
            SE = 8,
            S = 0x10,
            SW = 0x20,
            W = 0x40,
            NW = 0x80,
            ZERO = 0
        }

        public Vector2 getPointForAngle(Vector2 p1, float distance)
        {
            return p1 + this.getCardinalOffset() * distance;
        }


        public override string ToString()
        {
            return InternalValue.ToString();
        }

        public static CardinalPoint getCardinal2D(Vector2 p1, Vector2 p2)
        {
            var lastOffset = p1 - p2;
            if (Math.Abs(lastOffset.x) - Math.Abs(lastOffset.y) == 0)
            {
                if (lastOffset.x > 0)
                {
                    if (lastOffset.y > 0)
                    {
                        return CardinalPoint.SW;
                    }
                    else
                    {
                        return CardinalPoint.NW;
                    }
                }
                else
                {
                    if (lastOffset.y > 0)
                    {
                        return CardinalPoint.SE;
                    }
                    else
                    {
                        return CardinalPoint.NE;
                    }
                }
            }
            else if (Math.Abs(lastOffset.x) - Math.Abs(lastOffset.y) > 0)
            {
                if (lastOffset.x > 0)
                {
                    return CardinalPoint.W;
                }
                else
                {
                    return CardinalPoint.E;
                }
            }
            else
            {
                if (lastOffset.y > 0)
                {
                    return CardinalPoint.S;
                }
                else
                {
                    return CardinalPoint.N;
                }
            }
        }

    }

    public class SVGTemplate
    {
        public const float RADIUS = 20;

        /// <summary>
        /// The header.<>
        /// 0 = Height
        /// 1 = Width
        /// </summary>
        public string getHtmlHeader(int height, int width)
        {
            return "<!DOCTYPE html>" +
             "<html><head> <meta charset=\"UTF-8\"> " +
             "<style>" +
            ResourceLoader.loadResourceString("lineDrawBasicCss.css") +
             "</style>" +
             "</head><body>" +
             string.Format("<svg height='{0}' width='{1}'>", height, width);
        }
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
        private readonly string lineSegment = "<line x1='{0}' y1='{1}' x2='{2}' y2='{3}' style='stroke:rgb({4},{5},{6});stroke-width:" + RADIUS + "' stroke-linecap='round'/>";
        /// <summary>
        /// The line
        /// 0 = path;
        /// 1 = R;
        /// 2 = G;
        /// 3 = B;
        /// 4 = Line ref;
        /// </summary>
        private readonly string pathLine = "<path d='{0}' style='stroke:rgb({1},{2},{3});stroke-width:" + RADIUS + ";fill: none' stroke-linejoin=\"round\" stroke-linecap=\"round\"/>";
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
        private static string getStationTemplate()
        {
            return "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
                    "<circle cx=\"0\" cy=\"0\" r=\"" + RADIUS * 0.4 + "\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
                    "<text x=\"" + RADIUS * 0.8 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
                    "</g>";
        }

        private readonly string stationPointTemplate = "<circle style=\"stroke:rgb(155,155,155); stroke-width:1\" fill=\"rgb({2},{3},{4})\" r=\"" + RADIUS * 0.4 + "\" cy=\"0\" cx=\"0\" transform=\"translate({0},{1})\"/>";
     //   private readonly string stationNameTemplate = "<text transform=\"rotate({2},{0},{1}) translate({0},{1})\" x=\"" + RADIUS * 0.8 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" style=\"text-shadow: 0px 0px 6px white;\">{3}</text>";
        private readonly string stationNameTemplate = "<div class='stationContainer' style='top: {1}px; left: {0}px;' ><p style='transform: rotate({2}deg) translate(12px, 0px) ;'>{3}</p></div>";
        private readonly string stationNameInverseTemplate = "<div class='stationContainer' style='top: {1}px; left: {0}px;' ><p style='transform: rotate({2}deg) translate(12px, 0px) ;'>{3}</p></div>";
        /// <summary>
        /// The station.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Ang (º) 
        /// 3 = Station Name 
        /// </summary>
        private static string getStationReversedTemplate()
        {
            return "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
                    "<circle cx=\"0\" cy=\"0\" r=\"" + RADIUS * 0.4 + "\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
                    "<text x=\"" + RADIUS * 0.8 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" transform=\"rotate(180," + RADIUS / 2 + ",0)\"text-anchor=\"end \" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
                    "</g>";
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
        private readonly string metroLineSymbol = "<g transform=\"translate({0},{1})\">" +
            "  <rect x=\"" + -RADIUS + "\" y=\"" + -RADIUS + "\" width=\"" + RADIUS * 2 + "\" height=\"" + RADIUS * 2 + "\" fill=\"rgb({3},{4},{5})\" stroke=\"black\" stroke-width=\"1\" />" +
            "<text x=\"0\" y=\"" + RADIUS / 3 + "\" fill=\"white\"  stroke=\"black\" stroke-width=\"0.5\" style=\"font-size:" + RADIUS + "px\"   text-anchor=\"middle\">{2}</text>" +
            "</g>";


        /// <summary>
        /// The train line symbol.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Line Name 
        /// 3 = R
        /// 4 = G
        /// 5 = B
        /// </summary>
        private readonly string trainLineSymbol = "<g transform=\"translate({0},{1})\">" +
            "<circle cx=\"0\" cy=\"0\"r=\"" + RADIUS + "\" fill=\"rgb({3},{4},{5})\" stroke=\"black\" stroke-width=\"1\" />" +
            "<text x=\"0\" y=\"" + RADIUS / 3 + "\" fill=\"white\"  stroke=\"black\" stroke-width=\"0.5\" style=\"font-size:" + RADIUS + "px\"   text-anchor=\"middle\">{2}</text>" +
            "</g>";
        /// <summary>
        /// The footer.
        /// </summary>
        public readonly string footer = "</body></html>";




        private StringBuilder svgPart = new StringBuilder();
        private StringBuilder htmlPart = new StringBuilder();
        private float multiplier;
        private int height;
        private int width;


        private Dictionary<int, List<Range<int>>> hRanges = new Dictionary<int, List<Range<int>>>();
        private Dictionary<int, List<Range<int>>> vRanges = new Dictionary<int, List<Range<int>>>();
        /// <summary>
        ///x+y; x range
        /// </summary>
        private Dictionary<int, List<Range<int>>> d1Ranges = new Dictionary<int, List<Range<int>>>();
        /// <summary>
        ///x-y; x range
        /// </summary>
        private Dictionary<int, List<Range<int>>> d2Ranges = new Dictionary<int, List<Range<int>>>();

        private Vector2 offset;

        public SVGTemplate(int width, int height, float multiplier = 1, float offsetX = 0, float offsetY = 0)
        {
            this.multiplier = multiplier;
            this.width = width;
            this.height = height;
            this.offset = new Vector2(offsetX, offsetY);
        }

        public string getResult()
        {
            StringBuilder document = new StringBuilder(getHtmlHeader(width, height));
            document.Append(svgPart);
            document.Append("</svg>");
            document.Append(htmlPart);
            document.Append(footer);
            return document.ToString();
        }

        public void addStationsToExceptionMap(List<TLMMapDrawer.Station> stations)
        {
            foreach (var s in stations)
            {
                foreach (var p in s.getAllStationPoints())
                {
                    addStationToAllRangeMaps(p.Key);
                }
            }
        }

        public void addStation(TLMMapDrawer.Station s)
        {
            bool inverse = false;
            string name = s.name;
            var angle = s.writeAngle;
            switch (CardinalPoint.getCardinalPoint(angle).Value)
            {
                case CardinalPoint.CardinalInternal.NW:
                case CardinalPoint.CardinalInternal.W:
                case CardinalPoint.CardinalInternal.SW:
                    inverse = true;
                    return;
            }
            //     DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "STPUT: " + s.name + " => " + s.position);
            string integrationPath = s.getIntegrationLinePath(offset, multiplier);
            if (integrationPath != string.Empty)
            {
                svgPart.AppendFormat(pathLine, integrationPath, 128, 128, 128);
            }
            foreach (var pos in s.getAllStationPoints())
            {
                var point = pos.Key - offset;
                var t = TLMController.instance.tm.m_lines.m_buffer[(int)pos.Value];
                Color32 stationColor;
                bool day, night;
                t.GetActive(out day, out night);
                if (day && night)
                {
                    stationColor = Color.white;
                }
                else if (day)
                {
                    stationColor = Color.yellow;
                }
                else if (night)
                {
                    stationColor = Color.blue;
                }
                else
                {
                    stationColor = Color.black;
                }
                svgPart.AppendFormat(stationPointTemplate, point.x * multiplier, (point.y * multiplier), stationColor.r, stationColor.g, stationColor.b);
            }
            var namePoint = s.writePoint - offset;
            htmlPart.AppendFormat(inverse ? stationNameInverseTemplate : stationNameTemplate, namePoint.x * multiplier, (namePoint.y * multiplier), angle, s.name);

        }

        public void addLineSegment(Vector2 p1, Vector2 p2, Color32 color)
        {
            p1 -= offset;
            p2 -= offset;
            if (color.r > 240 && color.g > 240 && color.b > 240)
            {
                color = new Color32(240, 240, 240, 255);
            }
            svgPart.AppendFormat(lineSegment, p1.x * multiplier, (p1.y * multiplier), p2.x * multiplier, (p2.y * multiplier), color.r, color.g, color.b);
        }

        public void addTransportLineOnMap(List<TLMMapDrawer.Station> points, ushort transportLineIdx)
        {
            TransportLine t = TLMController.instance.tm.m_lines.m_buffer[(int)transportLineIdx];
            Color32 color = new Color32(Math.Min(t.m_color.r, (byte)240), Math.Min(t.m_color.g, (byte)240), Math.Min(t.m_color.b, (byte)240), 255);
            StringBuilder path = new StringBuilder();
            Vector2 p0 = points[0].getPositionForLine(transportLineIdx, points[1].defaultCentralPos) - offset;
            path.Append("M " + p0.x * multiplier + "," + p0.y * multiplier);
            CardinalPoint lastDirection = CardinalPoint.ZERO;
            TLMUtils.doLog("SVGTEMPLATE:addPath():695 => offset = {0}; multiplier = {1}", offset, multiplier);
            Vector2 sPrevPrev = Vector2.zero;
            Vector2 sPrev = p0;
            for (int i = 1; i < points.Count; i++)
            {
                var s = points[i].getPositionForLine(transportLineIdx, sPrev) - offset;
                CardinalPoint fromDirection = CardinalPoint.ZERO;
                Vector2 basicDirection = new Vector2(0, -1);
                if (i > 1)
                {
                    fromDirection = CardinalPoint.getCardinal2D(sPrevPrev, sPrev);
                    TLMUtils.doLog("SVGTEMPLATE:addPath():752 => sPrevPrev = {0}; sPrev = {1}; s = {2}; direction = {3}; STATION= {4}", sPrevPrev, sPrev, s, fromDirection, points[i].name);
                    var sPrevPrevPrev = Vector2.zero;
                    switch (fromDirection.Value)
                    {
                        case CardinalPoint.CardinalInternal.S:// Λ line
                            basicDirection = new Vector2(0, -1);
                            CASE_S:
                            if (s.y > sPrev.y)
                            {
                                TLMUtils.doLog("SVGTEMPLATE:addPath():709 => CASE Λ");
                                var offsetX = Math.Sign(s.x - sPrevPrev.x);
                                basicDirection = new Vector2(offsetX, 0);
                                sPrevPrevPrev = sPrev;
                                sPrevPrev = sPrev + new Vector2(offsetX, -1);
                                sPrev += new Vector2(offsetX * 2, -1);
                            }
                            else if (Math.Abs((sPrev - s).y) > 1)
                            {
                                sPrevPrev = sPrev;
                                sPrev += basicDirection;
                            }
                            else goto DiagCheck;
                            break;
                        case CardinalPoint.CardinalInternal.N:// V line
                            basicDirection = new Vector2(0, 1);
                            CASE_N:
                            if (s.y < sPrev.y)
                            {
                                TLMUtils.doLog("SVGTEMPLATE:addPath():709 => CASE V");
                                var offsetX = Math.Sign(s.x - sPrevPrev.x);
                                basicDirection = new Vector2(offsetX, 0);
                                sPrevPrevPrev = sPrev;
                                sPrevPrev = sPrev + new Vector2(offsetX, 1);
                                sPrev += new Vector2(offsetX * 2, 1);
                            }
                            else if (Math.Abs((sPrev - s).y) > 1)
                            {
                                sPrevPrev = sPrev;
                                sPrev += basicDirection;
                            }
                            else goto DiagCheck;
                            break;
                        case CardinalPoint.CardinalInternal.E:// < line
                            basicDirection = new Vector2(1, 0);
                            CASE_E:
                            if (s.x < sPrev.x)
                            {
                                TLMUtils.doLog("SVGTEMPLATE:addPath():709 => CASE <");
                                var offsetY = Math.Sign(s.y - sPrevPrev.y);
                                basicDirection = new Vector2(0, offsetY);
                                sPrevPrevPrev = sPrev;
                                sPrevPrev = sPrev + new Vector2(1, offsetY);
                                sPrev += new Vector2(1, offsetY * 2);
                            }
                            else if (Math.Abs((sPrev - s).x) > 1)
                            {
                                sPrevPrev = sPrev;
                                sPrev += basicDirection;
                            }
                            else goto DiagCheck;
                            break;
                        case CardinalPoint.CardinalInternal.W:// > line
                            basicDirection = new Vector2(-1, 0);
                            CASE_W:
                            if (s.x > sPrev.x)
                            {
                                TLMUtils.doLog("SVGTEMPLATE:addPath():709 => CASE >");
                                var offsetY = Math.Sign(s.y - sPrevPrev.y);
                                basicDirection = new Vector2(0, offsetY);
                                sPrevPrevPrev = sPrev;
                                sPrevPrev = sPrev + new Vector2(-1, offsetY);
                                sPrev += new Vector2(-1, offsetY * 2);
                            }
                            else if (Math.Abs((sPrev - s).x) > 1)
                            {
                                sPrevPrev = sPrev;
                                sPrev += basicDirection;
                            }
                            else goto DiagCheck;
                            break;
                        case CardinalPoint.CardinalInternal.SW:
                            basicDirection = new Vector2(-1, -1);
                            if (s.x > sPrev.x)
                            {
                                goto CASE_W;
                            }
                            else if (s.y > sPrev.y)
                            {
                                goto CASE_S;
                            }
                            break;
                        case CardinalPoint.CardinalInternal.SE:
                            basicDirection = new Vector2(1, -1);
                            if (s.x < sPrev.x)
                            {
                                goto CASE_E;
                            }
                            else if (s.y > sPrev.y)
                            {
                                goto CASE_S;
                            }
                            break;
                        case CardinalPoint.CardinalInternal.NW:
                            basicDirection = new Vector2(-1, 1);
                            if (s.x > sPrev.x)
                            {
                                goto CASE_W;
                            }
                            else if (s.y < sPrev.y)
                            {
                                goto CASE_N;
                            }
                            break;
                        case CardinalPoint.CardinalInternal.NE:
                            basicDirection = new Vector2(1, 1);
                            if (s.x < sPrev.x)
                            {
                                goto CASE_E;
                            }
                            else if (s.y < sPrev.y)
                            {
                                goto CASE_N;
                            }
                            break;
                    }

                    if (sPrevPrevPrev != Vector2.zero)
                    {
                        addToPathString(path, sPrevPrevPrev, sPrevPrev);
                    }

                    addToPathString(path, sPrevPrev, sPrev);
                    fromDirection = CardinalPoint.getCardinal2D(sPrevPrev, sPrev);
                }
                else // i==0
                {
                    var diff0 = s - sPrev;
                    if (Math.Abs(Math.Abs(diff0.x) - Math.Abs(diff0.y)) > 1)
                    {
                        var toDirection = CardinalPoint.getCardinal2D(sPrev, s);
                        basicDirection = toDirection.getCardinalOffset();
                        sPrevPrev = sPrev;
                        sPrev += basicDirection;
                        addToPathString(path, sPrevPrev, sPrev);
                        fromDirection = CardinalPoint.getCardinal2D(sPrevPrev, sPrev);
                    }
                }

                DiagCheck:
                var diff = s - sPrev;
                float minChangeCoord = Math.Min(Math.Abs(diff.x), Math.Abs(diff.y));
                var diagPointEndOffset = new Vector2(minChangeCoord * Math.Sign(diff.x), minChangeCoord * Math.Sign(diff.y));
                var diagPointEnd = diagPointEndOffset + sPrev;

                var isHorizontal = Math.Abs(diff.x) > Math.Abs(diff.y);
                var isVertical = Math.Abs(diff.x) < Math.Abs(diff.y);
                var isD1 = (diagPointEnd.y + diagPointEnd.x) == (sPrev.y + sPrev.x) && (diagPointEnd.x - diagPointEnd.y) != (sPrev.x - sPrev.y);
                var isD2 = (diagPointEnd.x - diagPointEnd.y) == (sPrev.x - sPrev.y) && (diagPointEnd.y + diagPointEnd.x) != (sPrev.y + sPrev.x);

                Vector2 offsetRemove = Vector2.zero;
                if (isD1)
                {
                    //diag
                    int index = (int)(diagPointEnd.y + diagPointEnd.x);
                    var targetPointD1 = getFreeD1Point(sPrev, diagPointEnd);
                    if (targetPointD1 == sPrev)
                    {
                        sPrevPrev = sPrev;
                        sPrev += basicDirection;
                        addToPathString(path, sPrevPrev, sPrev);
                        goto DiagCheck;
                    }
                    TLMUtils.doLog("SVGTEMPLATE:addPath() => D1! STATIONS= {0} ({2}) a {1} ({3}); DIAG END: {5}; DIAG IDX: {6}; TARGETPOINT: {4}", points[i - 1].name, points[i].name, sPrev, s, targetPointD1, diagPointEnd, index);
                    offsetRemove = diagPointEnd - targetPointD1;
                    if (isHorizontal)
                    {
                        calcBeginD1X:
                        var targetPointX = getFreeHorizontal(targetPointD1, s - offsetRemove);
                        TLMUtils.doLog("SVGTEMPLATE:addPath() => HORIZONTAL! STATIONS= {0} ({2}) a {1} ({3}); targetPointY: {4}; (s - offsetRemove).x != targetPointY.x && targetPointD1.x != sPrev.x: {5} != {6} && {7} != {8}", points[i - 1].name, points[i].name, sPrev, s, targetPointX, (s - offsetRemove).x, targetPointX.x, targetPointD1.x, sPrev.x);
                        if ((s - offsetRemove).x != (targetPointX).x && targetPointD1.x != sPrev.x)
                        {
                            var direction = (diagPointEnd - sPrev);
                            direction = new Vector2(Math.Sign(direction.x), Math.Sign(direction.y));
                            targetPointD1 -= direction;
                            offsetRemove = diagPointEnd - targetPointD1;
                            goto calcBeginD1X;
                        }
                    }

                    if (isVertical)
                    {
                        calcBeginD1Y:
                        var targetPointY = getFreeVertical(targetPointD1, s - offsetRemove);
                        TLMUtils.doLog("SVGTEMPLATE:addPath() => VERTICAL! STATIONS= {0} ({2}) a {1} ({3}); targetPointX: {4}; (s - offsetRemove).y != targetPointX.y && targetPointD1.y != sPrev.y: {5} != {6} && {7} != {8}", points[i - 1].name, points[i].name, sPrev, s, targetPointY, (s - offsetRemove).y, targetPointY.y, targetPointD1.y, sPrev.y);
                        if ((s - offsetRemove).y != targetPointY.y && targetPointD1.y != sPrev.y)
                        {
                            var direction = (diagPointEnd - sPrev);
                            direction = new Vector2(Math.Sign(direction.x), Math.Sign(direction.y));
                            targetPointD1 -= direction;
                            offsetRemove = diagPointEnd - targetPointD1;
                            goto calcBeginD1Y;
                        }
                    }
                    diagPointEnd -= offsetRemove;
                }
                else if (isD2)
                {
                    //diag
                    int index = (int)(diagPointEnd.x - diagPointEnd.y);
                    var targetPointD2 = getFreeD2Point(sPrev, diagPointEnd);
                    if (targetPointD2 == sPrev)
                    {
                        sPrevPrev = sPrev;
                        sPrev += basicDirection;
                        addToPathString(path, sPrevPrev, sPrev);
                        goto DiagCheck;
                    }
                    TLMUtils.doLog("SVGTEMPLATE:addPath() => D2! STATIONS= {0} ({2}) a {1} ({3}); DIAG END: {5}; DIAG IDX: {6}; TARGETPOINT: {4}", points[i - 1].name, points[i].name, sPrev, s, targetPointD2, diagPointEnd, index);
                    offsetRemove = diagPointEnd - targetPointD2;
                    if (isHorizontal)
                    {
                        calcBeginD2X:
                        var targetPointX = getFreeHorizontal(targetPointD2, s - offsetRemove);
                        TLMUtils.doLog("SVGTEMPLATE:addPath() => HORIZONTAL! STATIONS= {0} ({2}) a {1} ({3}); targetPointY: {4}; (s - offsetRemove).x != targetPointY.x && targetPointD2.x != sPrev.x: {5} != {6} && {7} != {8}", points[i - 1].name, points[i].name, sPrev, s, targetPointX, (s - offsetRemove).x, targetPointX.x, targetPointD2.x, sPrev.x);
                        if (s - offsetRemove != targetPointX && targetPointD2.y != sPrev.y)
                        {
                            var direction = (diagPointEnd - sPrev);
                            direction = new Vector2(Math.Sign(direction.x), Math.Sign(direction.y));
                            targetPointD2 -= direction;
                            offsetRemove = diagPointEnd - targetPointD2;
                            goto calcBeginD2X;
                        }
                    }

                    if (isVertical)
                    {
                        calcBeginD2Y:
                        var targetPointY = getFreeVertical(targetPointD2, s - offsetRemove);
                        TLMUtils.doLog("SVGTEMPLATE:addPath() => VERTICAL! STATIONS= {0} ({2}) a {1} ({3}); targetPointX: {4}; (s - offsetRemove).y != targetPointX.y && targetPointD2.y != sPrev.y: {5} != {6} && {7} != {8}", points[i - 1].name, points[i].name, sPrev, s, targetPointY, (s - offsetRemove).y, targetPointY.y, targetPointD2.y, sPrev.y);
                        if (s - offsetRemove != targetPointY && targetPointD2.x != sPrev.x)
                        {
                            var direction = (diagPointEnd - sPrev);
                            direction = new Vector2(Math.Sign(direction.x), Math.Sign(direction.y));
                            targetPointD2 -= direction;
                            offsetRemove = diagPointEnd - targetPointD2;
                            goto calcBeginD2Y;
                        }
                    }
                    diagPointEnd -= offsetRemove;
                }
                else if (isHorizontal)
                {
                    var targetPointX = getFreeHorizontal(sPrev, s);
                    TLMUtils.doLog("SVGTEMPLATE:addPath() => SÓ HORIZONTAL! STATIONS= {0} ({2}) a {1} ({3}); targetPointY: {4}; (s ).y != targetPointY.x && targetPointD2.x != sPrev.x: {5} != {6}", points[i - 1].name, points[i].name, sPrev, s, targetPointX, (s - offsetRemove).x, targetPointX.x);
                    if (s != targetPointX)
                    {
                        addToPathString(path, sPrev, targetPointX);
                        var offsetX = Math.Sign(s.x - sPrev.x);
                        sPrevPrev = targetPointX;
                        sPrev = targetPointX + new Vector2(offsetX, 1);
                        addToPathString(path, sPrevPrev, sPrev);
                        goto DiagCheck;
                    }
                }
                else if (isVertical)
                {
                    var targetPointY = getFreeVertical(sPrev, s);
                    TLMUtils.doLog("SVGTEMPLATE:addPath() => SÓ VERTICAL! STATIONS= {0} ({2}) a {1} ({3}); targetPointY: {4}; (s).y != targetPointY.y : {5} != {6} ", points[i - 1].name, points[i].name, sPrev, s, targetPointY, (s).y, targetPointY.y);
                    if (s != targetPointY)
                    {
                        addToPathString(path, sPrev, targetPointY);
                        var offsetY = Math.Sign(s.y - sPrev.y);
                        sPrevPrev = targetPointY;
                        sPrev = targetPointY + new Vector2(1, offsetY); ;
                        addToPathString(path, sPrevPrev, sPrev);
                        goto DiagCheck;
                    }
                }

                TLMUtils.doLog("SVGTEMPLATE:addPath() => s - offsetRemove == sPrev : {0} - {1} == {2} = {3}", s, offsetRemove, sPrev, s - offsetRemove == sPrev);

                if (s - offsetRemove == sPrev && offsetRemove != Vector2.zero)
                {
                    sPrevPrev = sPrev;
                    switch (fromDirection.Value)
                    {
                        case CardinalPoint.CardinalInternal.N:
                            sPrev += new Vector2(0, 1);
                            break;
                        case CardinalPoint.CardinalInternal.S:
                            sPrev += new Vector2(0, -1);
                            break;
                        case CardinalPoint.CardinalInternal.W:
                            sPrev += new Vector2(1, 0);
                            break;
                        case CardinalPoint.CardinalInternal.E:
                            sPrev += new Vector2(-1, 0);
                            break;
                        case CardinalPoint.CardinalInternal.ZERO:
                            sPrev += new Vector2(Math.Sign(s.x - sPrev.x), 0);
                            break;
                    }
                    addToPathString(path, sPrevPrev, sPrev);
                    goto DiagCheck;
                }
                TLMUtils.doLog("SVGTEMPLATE:addPath():1095 => sPrevPrev = {0} ; sPrev = {1}; s = {2} ", sPrevPrev, sPrev, s);
                if (sPrev == diagPointEnd && fromDirection.Value != CardinalPoint.CardinalInternal.ZERO && offsetRemove != Vector2.zero)
                {
                    sPrevPrev = sPrev;
                    sPrev = s - offsetRemove;
                    var toDirection = CardinalPoint.getCardinal2D(sPrevPrev, sPrev);
                    if (toDirection.Value != fromDirection.Value)
                    {
                        var direction = (s - sPrevPrev);
                        direction = new Vector2(Math.Sign(direction.x), Math.Sign(direction.y));
                        diagPointEnd += direction;
                        offsetRemove -= direction;
                    }
                }
                TLMUtils.doLog("SVGTEMPLATE:addPath():1108 => sPrevPrev = {0} ; sPrev = {1}; s = {2} ", sPrevPrev, sPrev, s);

                if (isD1 || isD2)
                {
                    if (diagPointEnd != sPrev)
                    {
                        addToPathString(path, sPrev, diagPointEnd);
                        if (diagPointEnd != s)
                        {
                            sPrevPrev = sPrev;
                            sPrev = diagPointEnd;
                        }
                    }
                    TLMUtils.doLog("SVGTEMPLATE:addPath():1116 => sPrevPrev = {0} ; sPrev = {1}; s = {2} ", sPrevPrev, sPrev, s);
                    if (offsetRemove != Vector2.zero)
                    {
                        var diagCompl = s - offsetRemove;
                        addToPathString(path, sPrev, diagCompl);
                        if (diagCompl != s)
                        {
                            sPrevPrev = sPrev;
                            sPrev = diagCompl;
                        }
                        TLMUtils.doLog("SVGTEMPLATE:addPath():1123 => sPrevPrev = {0} ; sPrev = {1}; s = {2} ", sPrevPrev, sPrev, s);
                    }
                }
                if (diagPointEnd + offsetRemove != s)
                {
                    addToPathString(path, sPrev, s);
                }
                sPrevPrev = sPrev;
                sPrev = s;
            }
            svgPart.AppendFormat(pathLine, path.ToString(), color.r, color.g, color.b);

        }

        private void addToPathString(StringBuilder pathString, Vector2 p0, Vector2 p1)
        {
            pathString.Append(" L " + p1.x * multiplier + "," + p1.y * multiplier);
            addSegmentToIndex(p0, p1);
        }

        private Vector2 getFreeHorizontal(Vector2 p1, Vector2 p2)
        {
            if (p1.y != p2.y) return p2;
            int targetX = (int)p2.x;
            TLMUtils.doLog(" getFreeHorizontal idx: {0} hRanges.ContainsKey(index)={1}; p1={2}; p2={3}", (int)p2.y, hRanges.ContainsKey((int)p2.y), p1, p2);
            if (hRanges.ContainsKey((int)p2.y))
            {
                Range<int> lineXs = new Range<int>((int)Math.Min(p1.x, p2.x) + 1, (int)Math.Max(p1.x, p2.x) - 1);
                var searchResult = hRanges[(int)p2.y].FindAll(x => x.IntersectRange(lineXs));
                TLMUtils.doLog(" getFreeHorizontal idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", (int)p2.y, lineXs, searchResult.Count, string.Join(",", hRanges[(int)p2.y].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.x - p1.x)) > 0)
                    {

                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.x);
                    }
                }
            }

            return new Vector2(targetX, p2.y);
        }



        private Vector2 getFreeVertical(Vector2 p1, Vector2 p2)
        {
            if (p1.x != p2.x) return p2;
            int targetY = (int)p2.y;
            TLMUtils.doLog(" getFreeVertical idx: {0} vRanges.ContainsKey(index)={1}; p1={2}; p2={3}", (int)p2.x, vRanges.ContainsKey((int)p2.x), p1, p2);
            if (vRanges.ContainsKey((int)p2.x))
            {
                Range<int> lineYs = new Range<int>((int)Math.Min(p1.y, p2.y) + 1, (int)Math.Max(p1.y, p2.y) - 1);
                var searchResult = vRanges[(int)p2.x].FindAll(x => x.IntersectRange(lineYs));
                TLMUtils.doLog(" getFreeVertical idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", (int)p2.x, lineYs, searchResult.Count, string.Join(",", vRanges[(int)p2.x].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.y - p1.y)) > 0)
                    {
                        targetY = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.y);
                    }
                    else
                    {
                        targetY = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.y);
                    }
                }
            }

            return new Vector2(p2.x, targetY);
        }

        private Vector2 getFreeD1Point(Vector2 p1, Vector2 p2)
        {
            if (p1.x + p1.y != p2.x + p2.y) return p2;
            int targetX = (int)p2.x;
            int index = (int)(p2.x + p2.y);
            TLMUtils.doLog(" getFreeHorizontalD1Point idx: {0} d1Ranges.ContainsKey(index)={1}", index, d1Ranges.ContainsKey(index));
            if (d1Ranges.ContainsKey(index))
            {
                Range<int> lineXs = new Range<int>((int)Math.Min(p1.x, p2.x) + 1, (int)Math.Max(p1.x, p2.x) - 1);
                var searchResult = d1Ranges[index].FindAll(x => x.IntersectRange(lineXs));
                TLMUtils.doLog(" getFreeHorizontalD2Point idx: {0}; X={1};LIST = {3} ; SRC = {2}", index, lineXs, searchResult.Count, string.Join(",", d1Ranges[index].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.x - p1.x)) > 0)
                    {
                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.x);
                    }
                }
            }

            return new Vector2(targetX, index - targetX);
        }



        private Vector2 getFreeD2Point(Vector2 p1, Vector2 p2)
        {
            if (p1.x - p1.y != p2.x - p2.y) return p2;
            int targetX = (int)p2.x;
            int index = (int)(p2.x - p2.y);
            TLMUtils.doLog(" getFreeHorizontalD2Point idx: {0} d2Ranges.ContainsKey(index)={1}", index, d2Ranges.ContainsKey(index));

            if (d2Ranges.ContainsKey(index))
            {
                Range<int> lineXs = new Range<int>((int)Math.Min(p1.x, p2.x) + 1, (int)Math.Max(p1.x, p2.x) - 1);
                var searchResult = d2Ranges[index].FindAll(x => x.IntersectRange(lineXs));
                TLMUtils.doLog(" getFreeHorizontalD2Point idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", index, lineXs, searchResult.Count, string.Join(",", d2Ranges[index].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.x - p1.x)) > 0)
                    {
                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.x);
                    }
                }
            }

            return new Vector2(targetX, targetX - index);
        }

        private void addStationToAllRangeMaps(Vector2 station)
        {
            addVerticalRange((int)station.x, new Range<int>((int)station.y, (int)station.y));
            addHorizontalRange((int)station.y, new Range<int>((int)station.x, (int)station.x));
            addD1Range(station, new Range<int>((int)station.x, (int)station.x));
            addD2Range(station, new Range<int>((int)station.x, (int)station.x));
        }

        private void addVerticalRange(int x, Range<int> values)
        {
            if (!vRanges.ContainsKey(x))
            {
                vRanges[x] = new List<Range<int>>();
            }
            vRanges[x].Add(values);
        }
        private void addHorizontalRange(int y, Range<int> values)
        {
            if (!hRanges.ContainsKey(y))
            {
                hRanges[y] = new List<Range<int>>();
            }
            hRanges[y].Add(values);
        }
        private void addD1Range(Vector2 refPoint, Range<int> values)
        {
            int index = (int)refPoint.x + (int)refPoint.y;
            if (!d1Ranges.ContainsKey(index))
            {
                d1Ranges[index] = new List<Range<int>>();
            }
            d1Ranges[index].Add(values);
        }
        private void addD2Range(Vector2 refPoint, Range<int> values)
        {
            int index = (int)refPoint.x - (int)refPoint.y;
            if (!d2Ranges.ContainsKey(index))
            {
                d2Ranges[index] = new List<Range<int>>();
            }
            d2Ranges[index].Add(values);
        }

        private void addSegmentToIndex(Vector2 p1, Vector2 p2)
        {
            if (p1 == p2) return;
            if (p1.x + p1.y == p2.x + p2.y)
            {
                addD1Range(p1, new Range<int>((int)p2.x, (int)p1.x));
            }

            if (p1.x - p1.y == p2.x - p2.y)
            {
                addD2Range(p1, new Range<int>((int)p2.x, (int)p1.x));
            }

            if (p2.x == p1.x)
            {
                addVerticalRange((int)p2.x, new Range<int>((int)p2.y, (int)p1.y));
            }
            if (p2.y == p1.y)
            {
                addHorizontalRange((int)p2.y, new Range<int>((int)p2.x, (int)p1.x));
            }
        }




        public void addMetroLineIndication(Vector2 point, string name, Color32 color)
        {
            point -= offset;
            svgPart.AppendFormat(metroLineSymbol, point.x * multiplier, (point.y * multiplier), name, color.r, color.g, color.b);
        }

        public void addTrainLineSegment(Vector2 point, string name, Color32 color)
        {
            point -= offset;
            svgPart.AppendFormat(trainLineSymbol, point.x * multiplier, (point.y * multiplier), name, color.r, color.g, color.b);
        }
    }

}

