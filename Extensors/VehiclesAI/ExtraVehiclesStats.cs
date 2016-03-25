using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Klyte.TransportLinesManager.Extensors
{
    class ExtraVehiclesStats
    {
        private static ExtraVehiclesStats _instance;
        public static ExtraVehiclesStats instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExtraVehiclesStats();
                }
                return _instance;
            }
        }

        private ExtraVehiclesStats() { }

        private Dictionary<ushort, float> vehicleLastTravelAvgFill = new Dictionary<ushort, float>();
        private Dictionary<ushort, float> vehicleLastTravelStdDevFill = new Dictionary<ushort, float>();
        private Dictionary<ushort, List<float>> vehicleLastTravelFillPerStopFillTemp = new Dictionary<ushort, List<float>>();
        private Dictionary<ushort, long> vehicleLastTravelFramesLineLapTake = new Dictionary<ushort, long>();
        private Dictionary<ushort, long> vehicleLastTravelFrameNumberLineLapStarted = new Dictionary<ushort, long>();
        private Dictionary<ushort, ushort> vehicleLastTravelLine = new Dictionary<ushort, ushort>();

        public static void OnVehicleStop(ushort vehicleID, Vehicle vehicleData)
        {
            if (vehicleData.m_transportLine != 0)
            {
                 if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)  TLMUtils.doLog("StartPathFindFake vId={0}; stopTarget = {1}; 1st stop = {2}", vehicleID, vehicleData.m_targetBuilding, Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].m_stops);
                if (vehicleData.m_targetBuilding == Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].m_stops)
                {
                    ExtraVehiclesStats.instance.endLap(vehicleID, vehicleData.m_transportLine);
                }
                int fill, cap;
                vehicleData = TLMLineUtils.GetVehicleCapacityAndFill(vehicleID, vehicleData, out fill, out cap);
                float fillRate = (float)fill / cap;
                ExtraVehiclesStats.instance.addExtraStatsData(vehicleID, fillRate, vehicleData.m_transportLine);

            }
            else
            {
                ExtraVehiclesStats.instance.removeExtraStatsData(vehicleID);
            }

        }



        public void addExtraStatsData(ushort vehicleId, float fillRate, ushort line)
        {
            //  if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)  TLMUtils.doLog("ExtraVehiclesStats.addExtraStatsData: vid={0}, lid={2}, fill={1}", vehicleId, fillRate, line);
            if (!vehicleLastTravelLine.ContainsKey(vehicleId))
            {
                return;
            }
            else if (vehicleLastTravelLine[vehicleId] != line)
            {
                removeExtraStatsData(vehicleId);
            }
            else if (vehicleLastTravelFillPerStopFillTemp.ContainsKey(vehicleId))
            {
                vehicleLastTravelFillPerStopFillTemp[vehicleId].Add(fillRate);
            }
        }

        public void endLap(ushort vehicleId, ushort line)
        {
             if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)  TLMUtils.doLog("ExtraVehiclesStats.endLap: vid={0}, lid={1}", vehicleId, line);
            if (!vehicleLastTravelLine.ContainsKey(vehicleId) || vehicleLastTravelLine[vehicleId] != line)
            {
                removeExtraStatsData(vehicleId);
            }

            vehicleLastTravelLine[vehicleId] = line;
            if (vehicleLastTravelFillPerStopFillTemp.ContainsKey(vehicleId))
            {
                vehicleLastTravelAvgFill[vehicleId] = vehicleLastTravelFillPerStopFillTemp[vehicleId].Average();
                vehicleLastTravelStdDevFill[vehicleId] = (float)CalculateStdDev(vehicleLastTravelFillPerStopFillTemp[vehicleId]);
                vehicleLastTravelFillPerStopFillTemp[vehicleId].Clear();
                 if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)  TLMUtils.doLog("ExtraVehiclesStats.endLap: vid={0}, fill= {1} ± {2}", vehicleId, vehicleLastTravelAvgFill[vehicleId].ToString("0.00%"), vehicleLastTravelStdDevFill[vehicleId].ToString("0.00%"));
            }
            else {
                vehicleLastTravelFillPerStopFillTemp[vehicleId] = new List<float>();
            }
            if (vehicleLastTravelFrameNumberLineLapStarted.ContainsKey(vehicleId))
            {
                vehicleLastTravelFramesLineLapTake[vehicleId] = Singleton<SimulationManager>.instance.m_currentFrameIndex - vehicleLastTravelFrameNumberLineLapStarted[vehicleId];
                 if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)  TLMUtils.doLog("ExtraVehiclesStats.endLap: vid={0}, time={1}", vehicleId, string.Format("{0} frames", vehicleLastTravelFramesLineLapTake[vehicleId]));
            }
            vehicleLastTravelFrameNumberLineLapStarted[vehicleId] = Singleton<SimulationManager>.instance.m_currentFrameIndex;
        }

        public void removeExtraStatsData(ushort vehicleId)
        {
             if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)  TLMUtils.doLog("ExtraVehiclesStats.removeExtraStatsData: vid={0}", vehicleId);
            vehicleLastTravelAvgFill.Remove(vehicleId);
            vehicleLastTravelStdDevFill.Remove(vehicleId);
            vehicleLastTravelFillPerStopFillTemp.Remove(vehicleId);
            vehicleLastTravelFramesLineLapTake.Remove(vehicleId);
            vehicleLastTravelFrameNumberLineLapStarted.Remove(vehicleId);
        }

        public Dictionary<ushort, ExtraData> getLineVehiclesData(ushort lineId)
        {
            Dictionary<ushort, ExtraData> result = new Dictionary<ushort, ExtraData>();
            ushort[] vehiclesWithData = vehicleLastTravelLine.Where(x => x.Value == lineId).Select(x => x.Key).ToArray();
            foreach (ushort vehicleId in vehiclesWithData)
            {
                if (vehicleLastTravelAvgFill.ContainsKey(vehicleId) && vehicleLastTravelFramesLineLapTake.ContainsKey(vehicleId))
                {
                    if (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_transportLine == lineId)
                    {
                        result.Add(vehicleId, new ExtraData(vehicleLastTravelAvgFill[vehicleId], vehicleLastTravelStdDevFill[vehicleId], vehicleLastTravelFramesLineLapTake[vehicleId]));
                    }
                    else
                    {
                        removeExtraStatsData(vehicleId);
                    }
                }
            }
            return result;
        }

        private double CalculateStdDev(IEnumerable<float> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        public struct ExtraData
        {
            public float avgFill;
            public float stdDevFill;
            public long framesTakenLap;

            public ExtraData(float v1, float v3, long v2)
            {
                avgFill = v1;
                framesTakenLap = v2;
                stdDevFill = v3;
            }

            public TimeSpan timeTakenLap
            {
                get
                {
                    return new TimeSpan(Singleton<SimulationManager>.instance.m_timePerFrame.Ticks * framesTakenLap);
                }
            }

            public static string framesToTimeTakenLapFormated(long frames)
            {
                var time = new TimeSpan(Singleton<SimulationManager>.instance.m_timePerFrame.Ticks * frames);
                return string.Format("{0}d {1}h{2}m", time.TotalDays.ToString("0"), time.Hours, time.Minutes.ToString("00"));
            }

            public static string framesToDaysTakenLapFormated(long frames)
            {
                var time = new TimeSpan(Singleton<SimulationManager>.instance.m_timePerFrame.Ticks * frames);
                return string.Format("{0}d", time.TotalDays.ToString("0.00"));
            }
        }
    }
}
