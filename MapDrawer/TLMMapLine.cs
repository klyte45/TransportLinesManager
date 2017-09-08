using ColossalFramework;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Utils;
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
        private TransportInfo.TransportType transportType
        {
            get; set;
        }
        public string lineName
        {
            get; set;
        }
        public string lineStringIdentifier
        {
            get; set;
        }
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
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            this.lineName = Singleton<TransportManager>.instance.GetLineName(lineId);
            this.lineStringIdentifier = TLMLineUtils.getLineStringId(lineId);
            transportType = t.Info.m_transportType;
            stations = new List<Station>();
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


    class MapTests
    {
        public static void Main(string[] args)
        {
            new LineSegmentStationsManager().getPath(new Vector2(678, 545), new Vector2(685, 556), CardinalPoint.SE, CardinalPoint.NW);
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
