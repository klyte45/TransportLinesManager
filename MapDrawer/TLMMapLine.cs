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
        public ushort lineId;
        private List<Station> stations;
        private TransportInfo.TransportType transportType;
        private ItemClass.SubService subservice;
        private VehicleInfo.VehicleType vehicleType;
        public string lineName;
        public string lineStringIdentifier;
        public Color32 lineColor;
        public bool activeDay;
        public bool activeNight;
        public ushort lineNumber;

        public MapTransportLine(Color32 color, bool day, bool night, ushort lineId)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
            this.lineName = Singleton<TransportManager>.instance.GetLineName(lineId);
            this.lineStringIdentifier = TLMLineUtils.getLineStringId(lineId);
            transportType = t.Info.m_transportType;
            subservice = t.Info.GetSubService();
            vehicleType = t.Info.m_vehicleType;
            stations = new List<Station>();
            lineColor = color;
            activeDay = day;
            activeNight = night;
            this.lineId = lineId;
            this.lineNumber = t.m_lineNumber;
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

        public String toJson()
        {
            var simmetry = TLMUtils.findSimetry(stations.Select(x => (int)x.stopId).ToArray(), out int middle);
            return $"{{\"lineId\": {lineId},\"stations\": [{string.Join(",", stations.Select(x => x.toJson()).ToArray())}],\"transportType\": \"{transportType}\"," +
                $"\"subservice\": \"{subservice}\",\"vehicleType\": \"{vehicleType}\",\"lineName\": \"{lineName}\",\"lineStringIdentifier\": \"{lineStringIdentifier}\"," +
                $"\"lineColor\": \"#{(lineColor.r.ToString("X2") + lineColor.g.ToString("X2") + lineColor.b.ToString("X2"))}\",\"activeDay\": {activeDay.ToString().ToLower()}," +
                $" \"activeNight\": {activeNight.ToString().ToLower()}, \"lineNumber\": {lineNumber}, \"simmetryRange\": " + (simmetry ? ("[" + middle + "," + (middle + stations.Count / 2 + 2) + "]") : "null") + "}";
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
