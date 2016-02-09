using ColossalFramework;
using Klyte.TransportLinesManager.Extensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.MapDrawer
{
    public struct MapTransportLine
    {
        public ushort lineId
        {
            get; set;
        }
        private List<Station> stations;
        private List<EquationDegree1Segment> segments;
        public Color32 lineColor
        {
            get; set;
        }

        public bool activeDay
        {
            get; set;
        }

        public bool activeNight
        {
            get; set;
        }

        public MapTransportLine(Color32 color, bool day, bool night, ushort lineId)
        {
            stations = new List<Station>();
            segments = new List<EquationDegree1Segment>();
            lineColor = color;
            activeDay = day;
            activeNight = night;
            this.lineId = lineId;
        }

        public void addStation(ref Station s)
        {
            stations.Add(s);
            s.addLine(lineId);
        }

        public Station this[int i]
        {
            get { return stations[i]; }
        }

        public int stationsCount()
        {
            return stations.Count;
        }
    }

    struct EquationDegree1Segment
    {
        public EquationDegree1 equation
        {
            get; set;
        }
        public float minSum
        {
            get; set;
        }
        public float maxSum
        {
            get; set;
        }

    }

    class EquationDegree1
    {
        public enum Direction
        {
            N_S = 90,
            W_E = 0,
            NW_SE = 135,
            SW_NE = 45
        }

        private Direction α;
        public float b
        {
            get; set;
        }
        private float m
        {
            get
            {
                switch (α)
                {
                    case Direction.NW_SE:
                        return -1;
                    case Direction.N_S:
                        return float.PositiveInfinity;
                    case Direction.SW_NE:
                        return 1;
                    case Direction.W_E:
                    default:
                        return 0;
                }
            }
        }

        public EquationDegree1(Direction angle, float linearCoef)
        {
            α = angle;
            b = linearCoef;
        }

        public void getCoordsForSumXY(float sum, out float x, out float y)
        {
            switch (α)
            {
                case Direction.NW_SE:
                case Direction.SW_NE:
                    x = (sum - b) / 2;
                    y = m * x + b;
                    return;
                case Direction.N_S:
                    x = b;
                    y = sum - x;
                    return;
                case Direction.W_E:
                default:
                    y = b;
                    x = sum - y;
                    return;
            }
        }

        public float getYForX(float x)
        {
            switch (α)
            {

                case Direction.N_S:
                    return float.NaN;
                case Direction.NW_SE:
                case Direction.SW_NE:
                case Direction.W_E:
                default:
                    return m * x + b;
            }
        }

        public bool isInLine(float sum, float x)
        {
            switch (α)
            {

                case Direction.N_S:
                    return x == b;
                case Direction.NW_SE:
                case Direction.SW_NE:
                case Direction.W_E:
                default:
                    return m * x + b + x == sum;
            }
        }

        public static Direction getDirection(Vector2 p1, Vector2 p2)
        {
            switch ((CardinalPoint.CardinalInternal)CardinalPoint.getCardinal2D(p1, p2))
            {
                case CardinalPoint.CardinalInternal.E:
                case CardinalPoint.CardinalInternal.W:
                    return Direction.W_E;
                case CardinalPoint.CardinalInternal.S:
                case CardinalPoint.CardinalInternal.N:
                    return Direction.N_S;
                case CardinalPoint.CardinalInternal.NE:
                case CardinalPoint.CardinalInternal.SW:
                    return Direction.SW_NE;
                case CardinalPoint.CardinalInternal.SE:
                case CardinalPoint.CardinalInternal.NW:
                default:
                    return Direction.NW_SE;
            }
        }


    }

    class MapTests : Redirector
    {
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();
        public static void Main(string[] args)
        {
            AddRedirect(typeof(MapTests), typeof(TLMMapDrawer).GetMethod("getLineUID", allFlags), ref redirects);
            AddRedirect(typeof(MapTests), typeof(TLMUtils).GetMethod("doLog", allFlags), ref redirects);
            List<Station> stations = new List<Station>();
            Dictionary<ushort, MapTransportLine> linhas = new Dictionary<ushort, MapTransportLine>();

            var stationsList = File.ReadAllLines(@"Transport Lines Manager\stationList.txt");
            var linesList = File.ReadAllLines(@"Transport Lines Manager\lineList.txt");

            for (int i = 0; i < stationsList.Length; i++)
            {
                var parsedData = stationsList[i].Split(',');
                stations.Add(new Station(parsedData[0], new Vector2(float.Parse(parsedData[1]) * 10, float.Parse(parsedData[2]) * 10f), new ushort[] { parsedData[0][0] }.ToList(), parsedData[0][0]));
            }

            for (ushort i = 0; i < linesList.Length; i++)
            {
                var parsedData = linesList[i].Split(',');
                linhas[i] = new MapTransportLine(TLMAutoColorPalettes.SaoPaulo2035[i + 1], true, true, i);
                foreach (string s in parsedData)
                {
                    ushort stop = s[0];
                    var station = stations.FirstOrDefault(x => x.stops.Contains(stop));
                    linhas[i].addStation(ref station);
                }
            }

            Console.WriteLine("Salvo em: {0}", Path.GetFullPath(TLMMapDrawer.printToSVG(stations, linhas, "TESTE")));
            Console.Read();
        }

        private static int getLineUID(ushort lineId)
        {
            return lineId;
        }
        private static void doLog(string s, params object[] param)
        {
            Console.WriteLine(s, param);
        }


    }
}
