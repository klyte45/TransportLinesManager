using ColossalFramework;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;
namespace Klyte.TransportLinesManager.Overrides
{
    public class TransportLineOverrides : MonoBehaviour, IRedirectable
    {
        #region Hooking

        public void Awake()
        {

            #region Automation Hooks
            MethodInfo doAutomation = typeof(TransportLineOverrides).GetMethod("DoAutomation", allFlags);
            MethodInfo preDoAutomation = typeof(TransportLineOverrides).GetMethod("PreDoAutomation", allFlags);

            LogUtils.DoLog("Loading AutoColor & AutoName Hook");
            RedirectorInstance.AddRedirect(typeof(TransportLine).GetMethod("AddStop", allFlags), preDoAutomation, doAutomation);
            #endregion


            #region Ticket Override Hooks
            MethodInfo GetTicketPriceTranspile = typeof(TransportLineOverrides).GetMethod("TicketPriceTranspilerEnterVehicle", allFlags);

            LogUtils.DoLog("Loading Ticket Override Hooks");
            RedirectorInstance.AddRedirect(typeof(HumanAI).GetMethod("EnterVehicle", allFlags), null, null, GetTicketPriceTranspile);
            #endregion
            #region Bus Spawn Unbunching
            MethodInfo BusUnbuncher = typeof(TransportLineOverrides).GetMethod("BusUnbuncher", allFlags);
            RedirectorInstance.AddRedirect(typeof(TransportLine).GetMethod("AddVehicle", allFlags), null, BusUnbuncher);
            #endregion



            #region Color Override Hooks
            MethodInfo TranspileGetColor = typeof(TransportLineOverrides).GetMethod("TranspileGetColor", allFlags);

            List<Type> allVehicleAI = ReflectionUtils.GetSubtypesRecursive(typeof(VehicleAI), typeof(VehicleAI));
            LogUtils.DoLog($"allVehicleAI size = {allVehicleAI.Count}");
            foreach (Type ai in allVehicleAI)
            {
                MethodInfo colorMethod = ai.GetMethod("GetColor", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null);
                if (colorMethod == null)
                {
                    continue;
                }

                LogUtils.DoLog($"Loading Color Override Hooks for {ai}");
                RedirectorInstance.AddRedirect(colorMethod, null, null, TranspileGetColor);
            }
            #endregion

            #region Budget Override Hooks

            MethodInfo TranspileSimulationStepLine = typeof(TransportLineOverrides).GetMethod("TranspileSimulationStepLine", allFlags);
            MethodInfo TranspileSimulationStepAI = typeof(TransportLineOverrides).GetMethod("TranspileSimulationStepAI", allFlags);
            LogUtils.DoLog("Loading SimulationStepPre Hook");
            RedirectorInstance.AddRedirect(typeof(TransportLine).GetMethod("SimulationStep", allFlags), null, null, TranspileSimulationStepLine);
            RedirectorInstance.AddRedirect(typeof(TransportLineAI).GetMethod("SimulationStep", allFlags, null, new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType() }, null), null, null, TranspileSimulationStepAI);
            #endregion

        }
        #endregion

        private static Dictionary<uint, Tuple<ushort, ushort>> m_counterIdx = new Dictionary<uint, Tuple<ushort, ushort>>();

        public Redirector RedirectorInstance => new Redirector();


        #region On Line Create

        public static void PreDoAutomation(ushort lineID, ref TransportLine.Flags __state) => __state = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags;

        public static void DoAutomation(ushort lineID, TransportLine.Flags __state)
        {
            LogUtils.DoLog("OLD: " + __state + " ||| NEW: " + Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags);
            if (lineID > 0 && (__state & TransportLine.Flags.Complete) == TransportLine.Flags.None && (__state & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None
                    && (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & (TransportLine.Flags.Temporary)) == TransportLine.Flags.None)
                {
                    if (TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
                    {
                        TLMController.AutoColor(lineID);
                    }
                    if (TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED))
                    {
                        TLMController.AutoName(lineID);
                    }
                    TLMController.instance.LineCreationToolbox.incrementNumber();
                    TLMTransportLineExtension.Instance.SafeCleanEntry(lineID);
                }
            }
            if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None &&
                (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.CustomColor) != TransportLine.Flags.None
                )
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~TransportLine.Flags.CustomColor;
            }

        }
        #endregion

        #region Budget Override
        private static readonly MethodInfo m_targetVehicles = typeof(TransportLine).GetMethod("CalculateTargetVehicleCount", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_setActive = typeof(TransportLine).GetMethod("SetActive", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_newTargetVehicles = typeof(TransportLineOverrides).GetMethod("NewCalculateTargetVehicleCount", RedirectorUtils.allFlags);
        private static readonly FieldInfo m_budgetField = typeof(TransportLine).GetField("m_budget", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getBudgetInt = typeof(TLMLineUtils).GetMethod("GetEffectiveBudgetInt", RedirectorUtils.allFlags);
        public static IEnumerable<CodeInstruction> TranspileSimulationStepLine(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            inst.InsertRange(0, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_getBudgetInt),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Cgt),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_getBudgetInt),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Cgt),
                        new CodeInstruction(OpCodes.Call,m_setActive ),
                    });
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Call && inst[i].operand == m_targetVehicles)
                {
                    inst[i - 1].opcode = OpCodes.Ldarg_1;
                    inst[i] = new CodeInstruction(OpCodes.Call, m_newTargetVehicles);
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static IEnumerable<CodeInstruction> TranspileSimulationStepAI(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand == m_budgetField)
                {
                    inst[i] = new CodeInstruction(OpCodes.Call, m_getBudgetInt);
                    inst[i + 1] = new CodeInstruction(OpCodes.Stloc_S, 4);
                    inst.RemoveAt(i + 9);
                    inst.RemoveAt(i + 8);
                    inst.RemoveAt(i + 7);
                    inst.RemoveAt(i + 6);
                    inst.RemoveAt(i + 5);
                    inst.RemoveAt(i + 4);
                    inst.RemoveAt(i + 3);
                    inst.RemoveAt(i + 2);
                    inst.RemoveAt(i - 1);
                    inst.RemoveAt(i - 4);
                    inst.RemoveAt(i - 5);
                    inst.RemoveAt(i - 6);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        public static int NewCalculateTargetVehicleCount(ushort lineId)
        {
            ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[lineId];
            float lineLength = t.m_totalLength;
            if (lineLength == 0f && t.m_stops != 0)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort stops = t.m_stops;
                ushort num2 = stops;
                int num3 = 0;
                while (num2 != 0)
                {
                    ushort num4 = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        ushort segment = instance.m_nodes.m_buffer[num2].GetSegment(i);
                        if (segment != 0 && instance.m_segments.m_buffer[segment].m_startNode == num2)
                        {
                            lineLength += instance.m_segments.m_buffer[segment].m_averageLength;
                            num4 = instance.m_segments.m_buffer[segment].m_endNode;
                            break;
                        }
                    }
                    num2 = num4;
                    if (num2 == stops)
                    {
                        break;
                    }
                    if (++num3 >= 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
                t.m_totalLength = lineLength;
            }
            return TLMLineUtils.CalculateTargetVehicleCount(ref t, lineId, lineLength);
        }

        #endregion

        #region Ticket Override
        private static readonly MethodInfo m_getTicketPriceForPrefix = typeof(TLMLineUtils).GetMethod("GetTicketPriceForVehicle", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getTicketPriceDefault = typeof(VehicleAI).GetMethod("GetTicketPrice", RedirectorUtils.allFlags);

        public static IEnumerable<CodeInstruction> TicketPriceTranspilerEnterVehicle(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand == m_getTicketPriceDefault)
                {
                    inst[i] = new CodeInstruction(OpCodes.Call, m_getTicketPriceForPrefix);
                    inst.RemoveAt(i + 3);
                    inst.RemoveAt(i + 2);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }


        #endregion

        #region Color Override
        private static readonly MethodInfo m_getColorTL = typeof(TransportLine).GetMethod("GetColor", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getColorFor = typeof(TransportLineOverrides).GetMethod("GetColorFor", RedirectorUtils.allFlags);

        public static IEnumerable<CodeInstruction> TranspileGetColor(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Call && inst[i].operand == m_getColorTL)
                {
                    inst[i] = inst[i - 2];
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Call, m_getColorFor));
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        private static Color GetColorFor(ref TransportLine line, ushort transportLine)
        {
            if (transportLine != 0)
            {
                var tsd = TransportSystemDefinition.GetDefinitionForLine(transportLine);
                if (tsd.TransportType == TransportInfo.TransportType.EvacuationBus)
                {
                    return Singleton<TransportManager>.instance.m_properties.m_transportColors[(int)line.Info.m_transportType];
                }

                ITLMTransportTypeExtension ext = tsd.GetTransportExtension();
                uint prefix = TLMPrefixesUtils.GetPrefix(transportLine);

                if (ext.IsUsingColorForModel(prefix) && ext.GetColor(prefix) != default)
                {
                    return ext.GetColor(prefix);
                }
                else
                {
                    return Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].GetColor();
                }
            }
            return Singleton<TransportManager>.instance.m_properties.m_transportColors[(int)line.Info.m_transportType];

        }
        #endregion

        #region Bus Spawn Unbunching
        private static void BusUnbuncher(ushort vehicleID, ref Vehicle data, bool findTargetStop)
        {
            if (findTargetStop && (data.Info.GetAI() is BusAI || data.Info.GetAI() is TramAI) && data.m_transportLine > 0)
            {
                TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[data.m_transportLine];
                data.m_targetBuilding = t.GetStop(SimulationManager.instance.m_randomizer.Int32((uint)t.CountStops(data.m_transportLine)));
            }
        }
        #endregion

    }
}
