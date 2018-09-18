using ColossalFramework;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Reflection;

namespace Klyte.TransportLinesManager.Overrides
{
    class BusAIOverrides : Redirector<BusAIOverrides>
    {
        #region Hooking

        private static bool preventDefault()
        {
            return false;
        }

        public override void AwakeBody()
        {
            MethodInfo preventDefault = typeof(TransportLineOverrides).GetMethod("preventDefault", allFlags);

            #region Automation Hooks
            MethodInfo preSetTarget = typeof(TransportLineOverrides).GetMethod("preSetTarget", allFlags);

            TLMUtils.doLog("Loading BusAI Logging");
            AddRedirect(typeof(BusAI).GetMethod("SetTarget", allFlags), preSetTarget);
            #endregion


        }
        #endregion

        private static Dictionary<uint, Tuple<ushort, ushort>> m_counterIdx = new Dictionary<uint, Tuple<ushort, ushort>>();


        #region On Line Create

        public static void preSetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding, BusAI __instance)
        {
            var vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleID];
            InstanceID id = default(InstanceID);
            id.TransportLine = vehicle.m_transportLine;
            var name = Singleton<InstanceManager>.instance.GetName(id);
            TLMUtils.doLog($"Estado do bus (${vehicleID}): L = {vehicle.m_transportLine} ({name}), F = {vehicle.m_flags}, T = {targetBuilding} ({TLMLineUtils.getStationName(targetBuilding, vehicle.m_transportLine, ItemClass.SubService.None)})");

        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            TLMUtils.doLog(text, param);
        }

    }
}
