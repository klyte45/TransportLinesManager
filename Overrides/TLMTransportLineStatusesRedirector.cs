using ColossalFramework;
using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static EconomyManager;

namespace Klyte.TransportLinesManager.Overrides
{

    public class TLMTransportLineStatusesRedirector : Redirector, IRedirectable
    {
        public Redirector RedirectorInstance => this;

        public void Awake()
        {
            AddRedirect(typeof(TransportLine).GetMethod("SimulationStep", RedirectorUtils.allFlags), null, null, typeof(TLMTransportLineStatusesRedirector).GetMethod("TranspileSimulationStepLine", RedirectorUtils.allFlags));
            AddRedirect(typeof(HumanAI).GetMethod("EnterVehicle", RedirectorUtils.allFlags), null, null, typeof(TLMTransportLineStatusesRedirector).GetMethod("TranspileHumanEnterVehicle", RedirectorUtils.allFlags));

            AddRedirect(typeof(StatisticsManager).GetMethod("SimulationStepImpl", RedirectorUtils.allFlags), null, typeof(TLMTransportLineStatusesManager).GetMethod("SimulationStepImpl", RedirectorUtils.allFlags));
            AddRedirect(typeof(StatisticsManager).GetMethod("UpdateData", RedirectorUtils.allFlags), null, typeof(TLMTransportLineStatusesManager).GetMethod("UpdateData", RedirectorUtils.allFlags));

        }


        private static readonly MethodInfo m_economyManagerCallFetch = typeof(EconomyManager).GetMethod("FetchResource", RedirectorUtils.allFlags, null, new Type[] { typeof(Resource), typeof(int), typeof(ItemClass) }, null);
        private static readonly MethodInfo m_economyManagerCallAdd = typeof(EconomyManager).GetMethod("AddResource", RedirectorUtils.allFlags, null, new Type[] { typeof(Resource), typeof(int), typeof(ItemClass) }, null);
        private static readonly MethodInfo m_doTransportLineEconomyManagement = typeof(TLMTransportLineStatusesRedirector).GetMethod("DoTransportLineEconomyManagement", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_doHumanAiEconomyManagement = typeof(TLMTransportLineStatusesRedirector).GetMethod("DoHumanAiEconomyManagement", RedirectorUtils.allFlags);
        public static IEnumerable<CodeInstruction> TranspileSimulationStepLine(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand == m_economyManagerCallFetch)
                {
                    inst[i - 1] = new CodeInstruction(OpCodes.Ldarg_1);
                    inst[i] = new CodeInstruction(OpCodes.Ldarg_0);
                    inst[i + 1] = new CodeInstruction(OpCodes.Call, m_doTransportLineEconomyManagement);
                    //inst.RemoveAt(i + 1);
                    inst.RemoveAt(i);
                    //inst.RemoveAt(i - 1);
                    inst.RemoveAt(i - 2);
                    inst.RemoveAt(i - 3);
                    inst.RemoveAt(i - 4);
                    inst.RemoveAt(i - 5);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static IEnumerable<CodeInstruction> TranspileHumanEnterVehicle(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand == m_economyManagerCallAdd)
                {
                    inst[i] = new CodeInstruction(OpCodes.Ldloc_2);
                    inst.InsertRange(i + 1, new List<CodeInstruction>(){
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_doHumanAiEconomyManagement)
                    });
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        public static void DoTransportLineEconomyManagement(ushort lineId)
        {
            LogUtils.DoLog($"DoTransportLineEconomyManagement : line {lineId}");
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[lineId];
            VehicleManager instance3 = VehicleManager.instance;
            ushort currVehicle = tl.m_vehicles;
            int loopCounter = 0;
            var capacities = new Dictionary<ushort, int>();
            while (currVehicle != 0)
            {
                ushort nextLineVehicle = instance3.m_vehicles.m_buffer[currVehicle].m_nextLineVehicle;
                capacities[currVehicle] = VehicleUtils.GetCapacity(instance3.m_vehicles.m_buffer[currVehicle].Info);
                currVehicle = nextLineVehicle;
                if (++loopCounter > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            int amount = 0;
            var tsd = TransportSystemDefinition.GetDefinitionForLine(ref tl);
            Citizen refNull = default;
            foreach (KeyValuePair<ushort, int> entry in capacities)
            {
                int cost = (int) (entry.Value * tsd.GetEffectivePassengerCapacityCost());
                TLMTransportLineStatusesManager.instance.AddToVehicle(entry.Key, 0, cost, ref refNull);
                amount += cost;
            }


            LogUtils.DoLog($"DoTransportLineEconomyManagement : line {lineId} ({tsd} {tl.m_lineNumber}) ;amount = {amount}");
            TLMTransportLineStatusesManager.instance.AddToLine(lineId, 0, amount, ref refNull, 0);
            EconomyManager.instance.FetchResource(Resource.Maintenance, amount, tl.Info.m_class);
        }

        public static int DoHumanAiEconomyManagement(EconomyManager instance, Resource resource, int amount, ItemClass itemClass, ushort vehicleId, ushort citizenId)
        {
            LogUtils.DoLog($"DoHumanAiEconomyManagement : vehicleId {vehicleId}");
            ushort lineId = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_transportLine;
            ref Citizen citizen = ref CitizenManager.instance.m_citizens.m_buffer[citizenId];
            instance.AddResource(resource, amount, itemClass);
            if (lineId != 0)
            {
                ushort stopId = TransportLine.GetPrevStop(VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding);
                TLMTransportLineStatusesManager.instance.AddToLine(lineId, amount, 0, ref citizen, citizenId);
                TLMTransportLineStatusesManager.instance.AddToVehicle(vehicleId, amount, 0, ref citizen);
                TLMTransportLineStatusesManager.instance.AddToStop(stopId, amount, ref citizen);
                LogUtils.DoLog($"DoHumanAiEconomyManagement : line {lineId};amount = {amount}; citizen = {citizenId}");
            }

            return 0;
        }

    }
}