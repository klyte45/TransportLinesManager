using Klyte.Commons.Extensions;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Reflection;
namespace Klyte.TransportLinesManager.Overrides
{
    internal class PublicTransportAtRoadAIOverrides : Redirector, IRedirectable
    {
        public void Awake() => AddRedirect(typeof(VehicleAI).GetMethod("ArrivingToDestination", RedirectorUtils.allFlags), GetType().GetMethod("PreArrivingToDestination"));

        private static MethodInfo BusAI_StartPathFind = typeof(BusAI).GetMethod("StartPathFind", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        private static MethodInfo TramAI_StartPathFind = typeof(TramAI).GetMethod("StartPathFind", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        private static MethodInfo TrolleyAI_StartPathFind = typeof(TrolleybusAI).GetMethod("StartPathFind", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);


        public static bool PreArrivingToDestination(ushort vehicleID, ref Vehicle vehicleData, VehicleAI __instance)
        {
            if (!(
                (__instance is BusAI && TLMBaseConfigXML.CurrentContextConfig.ExpressBusesEnabled)
                || (__instance is TramAI && TLMBaseConfigXML.CurrentContextConfig.ExpressTramsEnabled)
                || (__instance is TrolleybusAI && TLMBaseConfigXML.CurrentContextConfig.ExpressTrolleybusesEnabled)
                ) || vehicleData.m_transportLine == 0 || (vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0 || vehicleData.GetFirstVehicle(vehicleID) != vehicleID)
            {
                return true;
            }
            var currentStop = vehicleData.m_targetBuilding;
            ref TransportLine line = ref TransportManager.instance.m_lines.m_buffer[vehicleData.m_transportLine];
            if (currentStop == 0 || currentStop == line.m_stops)
            {
                return true;
            }
            TLMLineUtils.GetQuantityPassengerWaiting(currentStop, out int residents, out int tourists, out _);
            var unloadPredict = TLMLineUtils.GetQuantityPassengerUnloadOnNextStop(vehicleID, ref vehicleData, out bool full);
            if (unloadPredict > 0 || (!full && residents + tourists > 0))
            {
                return true;
            }
            var nextStop = TransportLine.GetNextStop(currentStop);
            vehicleData.m_targetBuilding = nextStop;
            var obj = new object[] { vehicleID, vehicleData };
            if (__instance is BusAI busAi)
            {
                if (!(bool)BusAI_StartPathFind.Invoke(busAi, obj))
                {
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }
                vehicleData = (Vehicle)obj[1];
            }
            else if (__instance is TrolleybusAI trolleyAI)
            {
                if (!(bool)TrolleyAI_StartPathFind.Invoke(trolleyAI, obj))
                {
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }
                vehicleData = (Vehicle)obj[1];
            }
            else if (__instance is TramAI tramAI)
            {
                if (!(bool)TramAI_StartPathFind.Invoke(tramAI, obj))
                {
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }
                vehicleData = (Vehicle)obj[1];
            }
            else
            {
                return true;
            }
            return false;
        }
    }

}
