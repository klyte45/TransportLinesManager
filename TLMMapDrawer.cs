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
            Dictionary<Vector2, Station> stations = new Dictionary<Vector2, Station>();
            Dictionary<Segment2, Color32> svgLines = new Dictionary<Segment2, Color32>();
            Dictionary<TransportLine, List<Station>> transportLines = new Dictionary<TransportLine, List<Station>>();
            MultiMap<Vector2, Vector2> intersects = new MultiMap<Vector2, Vector2>();
            float nil = 0;
            //			List<int> usedX = new List<int> ();
            //			List<int> usedY = new List<int> ();
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
                    Segment2 lastSeg = default(Segment2);


                    transportLines[t] = new List<Station>();

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

                        Vector2 pos = calc(TLMUtils.getStationBuildingPosition(nextStop, t.Info.m_stationSubService));
                        Vector2 gridAdd = Vector2.zero;

                        ushort nextNextStop = t.GetStop(j % stopsCount);
                        Vector2 gridNextPos = calc(TLMUtils.getStationBuildingPosition(nextNextStop, t.Info.m_stationSubService));
                        float angle = -TLMUtils.GetAngleOfLineBetweenTwoPoints(pos, gridNextPos);
                        CardinalPoint cardinal = CardinalPoint.getCardinalPoint(angle);

                        bool intersectStation = false;
                        int countIterations = 0;
                        while (stations.Keys.Contains(pos) || svgLines.Keys.Where(x => x.DistanceSqr(pos, out nil) == 0).Count() > 0)
                        {
                            //							Debug.Log ("COUNT:" + lines.Keys.Where(x=>x.DistanceSqr(pos,out nil)==0).Count());
                            if (!intersectStation && stations.Keys.Contains(pos))
                            {
                                intersectStation = true;
                            }
                            if (gridAdd == Vector2.zero)
                            {
                                gridAdd = cardinal.getCardinalOffset();
                            }
                            else {
                                pos -= gridAdd;
                                gridAdd = (cardinal++).getCardinalOffset() * (1 + (countIterations / 8));
                            }
                            pos += gridAdd;
                            countIterations++;
                        }



                        if (ultPos != Vector2.zero)
                        {
                            Segment2 s = new Segment2(ultPos, pos);
                            float sAngle = Vector2.Angle(s.a, s.b);
                            svgLines.Add(s, color);
                            lastSeg = s;
                        }



                        CardinalPoint cp = CardinalPoint.E;
                        int vizinhosVazios = 0;
                        int maxVizinhosVazios = 0;
                        CardinalPoint melhorCp = cp;
                        for (int v = 0; v < 8; v++, cp++)
                        {
                            Vector2 testPos = pos + cp.getCardinalOffset();
                            if (!stations.Keys.Contains(testPos) && svgLines.Keys.Where(x => x.DistanceSqr(testPos, out nil) == 0).Count() == 0)
                            {
                                vizinhosVazios++;
                                if (vizinhosVazios > maxVizinhosVazios)
                                {
                                    maxVizinhosVazios = vizinhosVazios;
                                    melhorCp = cp;
                                }
                            }
                            else if (stations.Keys.Contains(testPos))
                            {
                                intersects.Add(pos, testPos);
                                name = "";
                            }
                        }
                        Station thisStation = new Station(name, melhorCp, pos);
                        stations.Add(pos, thisStation);

                        transportLines[t].Add(thisStation);

                        //						Debug.Log ("POS:" + pos);
                        ultPos = pos;
                    }
                }
            }
            float minX = Math.Min(svgLines.Min(x => Math.Min(x.Key.a.x, x.Key.b.x)), stations.Min(x => x.Key.x));
            float minY = Math.Min(svgLines.Min(x => Math.Min(x.Key.a.y, x.Key.b.y)), stations.Min(x => x.Key.y));
            float maxX = Math.Max(svgLines.Max(x => Math.Max(x.Key.a.x, x.Key.b.x)), stations.Max(x => x.Key.x));
            float maxY = Math.Max(svgLines.Max(x => Math.Max(x.Key.a.y, x.Key.b.y)), stations.Max(x => x.Key.y));

            SVGTemplate svg = new SVGTemplate((int)((maxY - minY + 16) * SVGTemplate.RADIUS), (int)((maxX - minX + 16) * SVGTemplate.RADIUS), SVGTemplate.RADIUS, minX - 8, minY - 8);
            //foreach (var line in svgLines)
            //{
            //    svg.addLineSegment(line.Key.a, line.Key.b, line.Value);
            //}
            foreach (var line in transportLines)
            {
                svg.addPath(line.Value, line.Key.m_color);
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
                CardinalPoint angle = station.Value.preferredAngle;
                if (!string.IsNullOrEmpty(station.Value.name))
                {
                    Vector2 testingPoint = station.Key + angle.getCardinalOffset();
                    int countIterations = 0;
                    while (stations.Keys.Contains(testingPoint) || svgLines.Keys.Where(x => x.DistanceSqr(testingPoint, out nil) == 0).Count() > 0)
                    {
                        //							Debug.Log ("COUNT:" + lines.Keys.Where(x=>x.DistanceSqr(pos,out nil)==0).Count());
                        angle++;
                        testingPoint = station.Key + angle.getCardinalOffset();
                        countIterations++;
                        if (countIterations >= 8)
                        {
                            break;
                        }
                    }
                }
                svg.addStation(station.Value, angle.getCardinalAngle(), station.Value.name);
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

        public struct Station
        {
            public string name
            {
                get; set;
            }
            public CardinalPoint preferredAngle
            {
                get; set;
            }
            public Vector2 position
            {
                get; set;
            }

            public Station(string n, CardinalPoint pref, Vector2 pos)
            {
                name = n;
                preferredAngle = pref;
                position = pos;
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


    }

    public class SVGTemplate
    {
        public const float RADIUS = 20;

        /// <summary>
        /// The header.<>
        /// 0 = Height
        /// 1 = Width
        /// </summary>
        public readonly string header = "<!DOCTYPE html>" +
            "<html><head> <meta charset=\"UTF-8\"> " +
            "<style>" +
            "* {{" +
            "font-family: 'AvantGarde Md BT', Arial;" +
            "font-weight: bold;" +
            "}}" +
            "</style>" +
            "</head><body>" +
            "<svg height='{0}' width='{1}'>";
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
        /// The line segment. <>
        /// 0 = X1 
        /// 1 = Y1
        /// 2 = X2 
        /// 3 = Y2
        /// 4 = R
        /// 5 = G 
        /// 6 = B 
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
        private readonly string station = "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
            "<circle cx=\"0\" cy=\"0\" r=\"" + RADIUS * 0.4 + "\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
                "<text x=\"" + RADIUS * 0.8 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
            "</g>";
        /// <summary>
        /// The station for 4.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Ang (º) 
        /// 3 = Station Name 
        /// </summary>
        private readonly string station4 = "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
            "<circle cx=\"0\" cy=\"0\" r=\"" + RADIUS * 0.85 + "\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
                "<text x=\"" + RADIUS * 1.8 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
            "</g>";

        /// <summary>
        /// The station for 4.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Ang (º) 
        /// 3 = Station Name 
        /// </summary>
        private readonly string station9 = "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
            "<circle cx=\"0\" cy=\"0\" r=\"" + RADIUS * 1.3 + "\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
                "<text x=\"" + RADIUS * 2.8 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
            "</g>";

        /// <summary>
        /// The station.<>
        /// 0 = X 
        /// 1 = Y 
        /// 2 = Ang (º) 
        /// 3 = Station Name 
        /// </summary>
        private readonly string stationReversed = "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
            "<circle cx=\"0\" cy=\"0\" r=\"" + RADIUS * 0.4 + "\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
                "<text x=\"" + RADIUS / 2 + "\" y=\"" + RADIUS / 6 + "\" fill=\"black\" transform=\"rotate(180," + RADIUS / 2 + ",0)\"text-anchor=\"end \" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
            "</g>";

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
        public readonly string footer = "</svg></body></html>";
        private StringBuilder document;
        private float multiplier;
        private int height;


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
            document = new StringBuilder(String.Format(header, width, height));
            this.multiplier = multiplier;
            this.height = height;
            this.offset = new Vector2(offsetX, offsetY);
        }

        public string getResult()
        {
            document.Append(footer);
            return document.ToString();
        }

        public void addStation(TLMMapDrawer.Station s, float angle, string name)
        {
            Vector2 point = s.position;

            switch (CardinalPoint.getCardinalPoint(angle).Value)
            {
                case CardinalPoint.CardinalInternal.NW:
                case CardinalPoint.CardinalInternal.W:
                case CardinalPoint.CardinalInternal.SW:
                    addStationReversed(s, angle, name);
                    return;
            }
            //     DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "STPUT: " + s.name + " => " + s.position);
            point -= offset;
            document.AppendFormat(station, point.x * multiplier, (point.y * multiplier), angle, (name));
        }

        public void addStationReversed(TLMMapDrawer.Station s, float angle, string name)
        {
            Vector2 point = s.position;
            switch (CardinalPoint.getCardinalPoint(angle).Value)
            {
                case CardinalPoint.CardinalInternal.NE:
                case CardinalPoint.CardinalInternal.E:
                case CardinalPoint.CardinalInternal.SE:
                    addStation(s, angle, name);
                    return;
            }
            //   DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "STPUT: " + s.name + " => " + s.position);
            point -= offset;
            document.AppendFormat(stationReversed, point.x * multiplier, (point.y * multiplier), angle, (name));
        }

        public void addLineSegment(Vector2 p1, Vector2 p2, Color32 color)
        {
            p1 -= offset;
            p2 -= offset;
            if (color.r > 240 && color.g > 240 && color.b > 240)
            {
                color = new Color32(240, 240, 240, 255);
            }
            document.AppendFormat(lineSegment, p1.x * multiplier, (p1.y * multiplier), p2.x * multiplier, (p2.y * multiplier), color.r, color.g, color.b);
        }

        public void addPath(List<TLMMapDrawer.Station> points, Color32 color)
        {
            StringBuilder path = new StringBuilder();
            Vector2 p0 = points[0].position - offset;
            path.Append("M " + p0.x * multiplier + "," + p0.y * multiplier);
            CardinalPoint lastDirection = CardinalPoint.ZERO;
            doLog("SVGTEMPLATE:addPath():695 => offset = {0}; multiplier = {1}", offset, multiplier);
            for (int i = 1; i < points.Count; i++)
            {
                var sPrev = points[i - 1].position - offset;
                var s = points[i].position - offset;

                if (i > 1)
                {
                    var sPrevPrev = points[i - 2].position - offset;
                    var lastOffset = sPrevPrev - sPrev;
                    CardinalPoint fromDirection = CardinalPoint.ZERO;
                    if (Math.Abs(lastOffset.x) - Math.Abs(lastOffset.y) > 0)
                    {
                        if (lastOffset.x > 0)
                        {
                            fromDirection = CardinalPoint.W;
                        }
                        else
                        {
                            fromDirection = CardinalPoint.E;
                        }
                    }
                    else
                    {
                        if (lastOffset.y > 0)
                        {
                            fromDirection = CardinalPoint.S;
                        }
                        else
                        {
                            fromDirection = CardinalPoint.N;
                        }
                    }
                    doLog("SVGTEMPLATE:addPath():752 => sPrevPrev = {0}; sPrev = {1}; s = {2}; direction = {3}; STATION= {4}", sPrevPrev, sPrev, s, fromDirection, points[i].name);
                    if (s.y >= sPrev.y && fromDirection == CardinalPoint.S)// Λ line
                    {
                        Debug.LogWarning("SVGTEMPLATE:addPath():709 => CASE Λ");
                        var offsetX = Math.Sign(s.x - sPrevPrev.x);
                        path.Append(" L " + (offsetX + sPrev.x) * multiplier + "," + (sPrev.y - 1) * multiplier);
                        path.Append(" L " + (offsetX * 2 + sPrev.x) * multiplier + "," + (sPrev.y - 1) * multiplier);
                        sPrev += new Vector2(offsetX * 2, -1);
                    }
                    else if (s.y <= sPrev.y && fromDirection == CardinalPoint.N)// V line
                    {
                        Debug.LogWarning("SVGTEMPLATE:addPath():709 => CASE V");
                        var offsetX = Math.Sign(s.x - sPrevPrev.x);
                        path.Append(" L " + (offsetX + sPrev.x) * multiplier + "," + (sPrev.y + 1) * multiplier);
                        path.Append(" L " + (offsetX * 2 + sPrev.x) * multiplier + "," + (sPrev.y + 1) * multiplier);
                        sPrev += new Vector2(offsetX, 1);
                    }
                    else if (s.x <= sPrev.x && fromDirection == CardinalPoint.E)// < line
                    {
                        Debug.LogWarning("SVGTEMPLATE:addPath():709 => CASE <");
                        var offsetY = Math.Sign(-s.y - sPrevPrev.y);
                        path.Append(" L " + (1 + sPrev.x) * multiplier + "," + (sPrev.y + offsetY) * multiplier);
                        path.Append(" L " + (1 + sPrev.x) * multiplier + "," + (sPrev.y + offsetY * 2) * multiplier);
                        sPrev += new Vector2(1, offsetY);
                    }
                    else if (s.x >= sPrev.x && fromDirection == CardinalPoint.W)// > line
                    {
                        Debug.LogWarning("SVGTEMPLATE:addPath():709 => CASE >");
                        var offsetY = Math.Sign(s.y - sPrevPrev.y);
                        path.Append(" L " + (-1 + sPrev.x) * multiplier + "," + (sPrev.y + offsetY) * multiplier);
                        path.Append(" L " + (-1 + sPrev.x) * multiplier + "," + (sPrev.y + offsetY * 2) * multiplier);
                        sPrev += new Vector2(-1, offsetY * 2);
                    }

                    //doLog("SVGTEMPLATE:addPath():709 => sPrevPrev = {0}; sPrev = {1}; s = {2}; angle = {3}; anglePrev = {4}", sPrevPrev, sPrev, s, angle, anglePrev);
                    //if (anglePrev.Value == (~angle).Value)
                    //{
                    //    var distance = anglePrev - angle;
                    //    while (distance > 2)
                    //    {
                    //        sPrev = ((distance < 1 ? ++anglePrev : --anglePrev)).getPointForAngle2D(sPrev, 1);
                    //        distance = anglePrev - angle;
                    //        doLog("SVGTEMPLATE:addPath():717 => angle = {0}; anglePrev = {1}; distance = {2}", angle, anglePrev, distance);
                    //        path.Append(" L " + sPrev.x * multiplier + "," + sPrev.y * multiplier);
                    //    }
                    //}
                }

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
                    doLog("SVGTEMPLATE:addPath() => D1! STATIONS= {0} ({2}) a {1} ({3}); DIAG END: {5}; DIAG IDX: {6}; TARGETPOINT: {4}", points[i - 1].name, points[i].name, sPrev, s, targetPointD1, diagPointEnd, index);
                    offsetRemove = diagPointEnd - targetPointD1;
                    //if (isHorizontal)
                    //{
                    //    doLog("SVGTEMPLATE:addPath() => HORIZONTAL! ");
                    //    calcBeginD1X:
                    //    var targetPointX = getFreeHorizontal(targetPointD1, s - offsetRemove);
                    //    if (s - offsetRemove != targetPointX && targetPointD1.x != sPrev.x)
                    //    {
                    //        var direction = (diagPointEnd - sPrev);
                    //        direction.Normalize();
                    //        targetPointD1 -= direction;
                    //        offsetRemove = diagPointEnd - targetPointD1;
                    //        goto calcBeginD1X;
                    //    }
                    //    if (!hRanges.ContainsKey((int)targetPointX.y))
                    //    {
                    //        hRanges[(int)targetPointX.y] = new List<Range<int>>();
                    //    }
                    //    var targetXlineXs = new Range<int>((int)targetPointD1.x, (int)(s - offsetRemove).x);
                    //    hRanges[(int)targetPointX.y].Add(targetXlineXs);
                    //}

                    //if (isVertical)
                    //{
                    //    doLog("SVGTEMPLATE:addPath() => VERTICAL! ");
                    //    calcBeginD1Y:
                    //    var targetPointY = getFreeVertical(targetPointD1, s - offsetRemove);
                    //    if (s - offsetRemove != targetPointY && targetPointD1.y != sPrev.y)
                    //    {
                    //        var direction = (diagPointEnd - sPrev);
                    //        direction.Normalize();
                    //        targetPointD1 -= direction;
                    //        offsetRemove = diagPointEnd - targetPointD1;
                    //        goto calcBeginD1Y;
                    //    }
                    //    if (!vRanges.ContainsKey((int)targetPointY.x))
                    //    {
                    //        vRanges[(int)targetPointY.x] = new List<Range<int>>();
                    //    }
                    //    var targetYlineYs = new Range<int>((int)targetPointD1.y, (int)(s - offsetRemove).y);
                    //    vRanges[(int)targetPointY.x].Add(targetYlineYs);
                    //}
                    diagPointEnd -= offsetRemove;

                    if (!d1Ranges.ContainsKey(index))
                    {
                        d1Ranges[index] = new List<Range<int>>();
                    }
                    var lineXs = new Range<int>((int)sPrev.x, (int)diagPointEnd.x);
                    d1Ranges[index].Add(lineXs);
                }

                if (isD2)
                {
                    //diag
                    int index = (int)(diagPointEnd.x - diagPointEnd.y);
                    var targetPointD2 = getFreeD2Point(sPrev, diagPointEnd);
                    doLog("SVGTEMPLATE:addPath() => D2! STATIONS= {0} ({2}) a {1} ({3}); DIAG END: {5}; DIAG IDX: {6}; TARGETPOINT: {4}", points[i - 1].name, points[i].name, sPrev, s, targetPointD2, diagPointEnd, index);
                    offsetRemove = diagPointEnd - targetPointD2;
                    //if (isHorizontal)
                    //{
                    //    doLog("SVGTEMPLATE:addPath() => HORIZONTAL! ");
                    //    calcBeginD2X:
                    //    var targetPointX = getFreeHorizontal(targetPointD2, s - offsetRemove);
                    //    if (s - offsetRemove != targetPointX && targetPointD2.x != sPrev.x)
                    //    {
                    //        var direction = (diagPointEnd - sPrev);
                    //        direction.Normalize();
                    //        targetPointD2 -= direction;
                    //        offsetRemove = diagPointEnd - targetPointD2;
                    //        goto calcBeginD2X;
                    //    }
                    //    if (!hRanges.ContainsKey((int)targetPointX.y))
                    //    {
                    //        hRanges[(int)targetPointX.y] = new List<Range<int>>();
                    //    }
                    //    var targetXlineXs = new Range<int>((int)targetPointD2.x, (int)(s - offsetRemove).x);
                    //    hRanges[(int)targetPointX.y].Add(targetXlineXs);
                    //}

                    //if (isVertical)
                    //{
                    //    doLog("SVGTEMPLATE:addPath() => VERTICAL! ");
                    //    calcBeginD2Y:
                    //    var targetPointY = getFreeVertical(targetPointD2, s - offsetRemove);
                    //    if (s - offsetRemove != targetPointY && targetPointD2.y != sPrev.y)
                    //    {
                    //        var direction = (diagPointEnd - sPrev);
                    //        direction.Normalize();
                    //        targetPointD2 -= direction;
                    //        offsetRemove = diagPointEnd - targetPointD2;
                    //        goto calcBeginD2Y;
                    //    }
                    //    if (!vRanges.ContainsKey((int)targetPointY.x))
                    //    {
                    //        vRanges[(int)targetPointY.x] = new List<Range<int>>();
                    //    }
                    //    var targetYlineYs = new Range<int>((int)targetPointD2.y, (int)(s - offsetRemove).y);
                    //    vRanges[(int)targetPointY.x].Add(targetYlineYs);
                    //}
                    diagPointEnd -= offsetRemove;

                    if (!d2Ranges.ContainsKey(index))
                    {
                        d2Ranges[index] = new List<Range<int>>();
                    }
                    var lineXs = new Range<int>((int)sPrev.x, (int)diagPointEnd.x);
                    d2Ranges[index].Add(lineXs);
                }

                if (isD1 || isD2)
                {
                    path.Append(" L " + diagPointEnd.x * multiplier + "," + diagPointEnd.y * multiplier);
                    if (offsetRemove != Vector2.zero)
                    {
                        var diagCompl = s - offsetRemove;
                        path.Append(" L " + diagCompl.x * multiplier + "," + diagCompl.y * multiplier);
                    }
                }
                if (diagPointEnd + offsetRemove != s)
                {
                    path.Append(" L " + s.x * multiplier + "," + s.y * multiplier);
                }

            }
            document.AppendFormat(pathLine, path.ToString(), color.r, color.g, color.b);

        }

        private Vector2 getFreeHorizontal(Vector2 p1, Vector2 p2)
        {
            if (p1.y != p2.y) return p2;
            int targetX = (int)p2.x;
            doLog(" getFreeHorizontal idx: {0} hRanges.ContainsKey(index)={1}", (int)p2.y, hRanges.ContainsKey((int)p2.y));
            if (hRanges.ContainsKey((int)p2.y))
            {
                Range<int> lineXs = new Range<int>((int)p1.x, (int)p2.x);
                var searchResult = hRanges[(int)p2.y].FindAll(x => x.IntersectRange(lineXs));
                doLog(" getFreeHorizontal idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", (int)p2.y, lineXs, searchResult.Count, string.Join(",", hRanges[(int)p2.y].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2 - p1).x) > 0)
                    {

                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Min(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Max(), (int)p1.x);
                    }
                }
            }

            return new Vector2(targetX, p2.y);
        }



        private Vector2 getFreeVertical(Vector2 p1, Vector2 p2)
        {
            if (p1.x != p2.x) return p2;
            int targetY = (int)p2.y;
            doLog(" getFreeVertical idx: {0} vRanges.ContainsKey(index)={1}", (int)p2.x, vRanges.ContainsKey((int)p2.x));
            if (vRanges.ContainsKey((int)p2.x))
            {
                Range<int> lineYs = new Range<int>((int)p1.y, (int)p2.y);
                var searchResult = vRanges[(int)p2.x].FindAll(x => x.IntersectRange(lineYs));
                doLog(" getFreeVertical idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", (int)p2.x, lineYs, searchResult.Count, string.Join(",", vRanges[(int)p2.x].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2 - p1).y) > 0)
                    {
                        targetY = Math.Max(searchResult.Select(x => x.Minimum - 1).Min(), (int)p1.y);
                    }
                    else
                    {
                        targetY = Math.Min(searchResult.Select(x => x.Maximum + 1).Max(), (int)p1.y);
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
            doLog(" getFreeHorizontalD1Point idx: {0} d2Ranges.ContainsKey(index)={1}", index, d1Ranges.ContainsKey(index));
            if (d1Ranges.ContainsKey(index))
            {
                Range<int> lineXs = new Range<int>((int)p1.x, (int)p2.x);
                var searchResult = d1Ranges[index].FindAll(x => x.IntersectRange(lineXs));
                doLog(" getFreeHorizontalD2Point idx: {0}; X={1};LIST = {3} ; SRC = {2}", index, lineXs, searchResult.Count, string.Join(",", d1Ranges[index].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2 - p1).x) > 0)
                    {
                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Min(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Max(), (int)p1.x);
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
            doLog(" getFreeHorizontalD1Point idx: {0} d2Ranges.ContainsKey(index)={1}", index, d2Ranges.ContainsKey(index));

            if (d2Ranges.ContainsKey(index))
            {
                Range<int> lineXs = new Range<int>((int)p1.x, (int)p2.x);
                var searchResult = d2Ranges[index].FindAll(x => x.IntersectRange(lineXs));
                doLog(" getFreeHorizontalD2Point idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", index, lineXs, searchResult.Count, string.Join(",", d2Ranges[index].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2 - p1).x) > 0)
                    {
                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Min(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Max(), (int)p1.x);
                    }
                }
            }

            return new Vector2(targetX, targetX - index);
        }

        private void doLog(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
        }

        public void addMetroLineIndication(Vector2 point, string name, Color32 color)
        {
            point -= offset;
            document.AppendFormat(metroLineSymbol, point.x * multiplier, (point.y * multiplier), name, color.r, color.g, color.b);
        }

        public void addTrainLineSegment(Vector2 point, string name, Color32 color)
        {
            point -= offset;
            document.AppendFormat(trainLineSymbol, point.x * multiplier, (point.y * multiplier), name, color.r, color.g, color.b);
        }
    }

}

