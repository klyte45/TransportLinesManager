using ColossalFramework.Globalization;
using ColossalFramework.Threading;
using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static Klyte.Commons.Extensions.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    internal class TransportToolOverrides : MonoBehaviour, IRedirectable
    {
        public void Awake()
        {

            #region Automation Hooks
            MethodInfo onEnable = typeof(TransportToolOverrides).GetMethod("OnEnable", allFlags);
            MethodInfo onDisable = typeof(TransportToolOverrides).GetMethod("OnDisable", allFlags);
            MethodInfo transpileOnToolUpdate = typeof(TransportToolOverrides).GetMethod("TranspileOnToolUpdate", allFlags);
            MethodInfo AfterEveryAction = typeof(TransportToolOverrides).GetMethod("AfterEveryAction", allFlags);
            MethodInfo AfterEveryActionZeroable = typeof(TransportToolOverrides).GetMethod("AfterEveryActionZeroable", allFlags);

            LogUtils.DoLog($"Loading TransportToolOverrides Hook");
            try
            {
                var tt = new TransportTool();
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnEnable", allFlags), null, onEnable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnDisable", allFlags), onDisable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnToolUpdate", allFlags), null, null, transpileOnToolUpdate);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("NewLine", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags), AfterEveryAction);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("AddStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags), AfterEveryAction);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("RemoveStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags), AfterEveryActionZeroable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("CancelPrevStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags), AfterEveryActionZeroable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("CancelMoveStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags), AfterEveryActionZeroable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("MoveStop", allFlags).Invoke(tt, new object[] { false }).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags), AfterEveryAction);
                Destroy(tt);
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("ERRO AO CARREGAR HOOKS: {0}\n{1}", e.Message, e.StackTrace);
            }

            #endregion

        }

        public static void OnEnable()
        {
            LogUtils.DoLog("OnEnableTransportTool");
            TLMController.Instance.LineCreationToolbox?.SetVisible(true);
            TLMController.Instance.SetCurrentSelectedId(0);
        }

        public static void OnDisable()
        {
            LogUtils.DoLog("OnDisableTransportTool");
            TLMController.Instance.SetCurrentSelectedId(0);
            TLMController.Instance.LineCreationToolbox?.SetVisible(false);
        }

        private static IEnumerable<CodeInstruction> TranspileAfterEveryAction(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            MethodInfo AfterEveryAction = typeof(TransportToolOverrides).GetMethod("AfterEveryAction", allFlags);
            inst.RemoveAt(inst.Count - 1);
            inst.AddRange(new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, AfterEveryAction),
                        new CodeInstruction(OpCodes.Ret),
                    });

            LogUtils.PrintMethodIL(inst, true);
            return inst;
        }

        private static readonly FieldInfo m_lineNumField = typeof(TransportTool).GetField("m_line", RedirectorUtils.allFlags);
        public static void AfterEveryAction()
        {
            if (ToolManager.instance.m_properties?.CurrentTool is TransportTool tt)
            {
                ushort lineId = (ushort)m_lineNumField.GetValue(tt);
                if (lineId > 0)
                {
                    new WaitForFixedUpdate();
                    ThreadHelper.dispatcher.Dispatch(() =>
                    {
                        TLMController.Instance.LineCreationToolbox.SyncForm();
                    });
                }
            }

        }
        public static void AfterEveryActionZeroable()
        {
            if (ToolManager.instance.m_properties?.CurrentTool is TransportTool)
            {
                ushort lineId = (ushort)m_lineNumField.GetValue(ToolManager.instance.m_properties.CurrentTool as TransportTool);
                new WaitForFixedUpdate();
                ThreadHelper.dispatcher.Dispatch(() =>
                {
                    TLMController.Instance.LineCreationToolbox.SyncForm();
                });

            }
        }

        public Redirector RedirectorInstance { get; } = new Redirector();

        private static IEnumerable<CodeInstruction> TranspileOnToolUpdate(IEnumerable<CodeInstruction> instr)
        {
            bool transpiled = false;
            var instrList = instr.ToList();
            for (int i = 2; i < instrList.Count - 4; i++)
            {

                if (instrList[i - 1].opcode == OpCodes.Ldc_I4_1
                    && instrList[i].opcode == OpCodes.Ldloc_S
                    && instrList[i].operand is LocalBuilder lb
                    && lb.LocalIndex == 6
                    && instrList[i + 3].opcode == OpCodes.Call
                    && instrList[i + 3].operand is MethodInfo mi
                    && mi.Name == "ShowToolInfo")
                {
                    instrList.InsertRange(i + 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldloc_S, 0),
                        new CodeInstruction(OpCodes.Ldloc_S, 1),
                        new CodeInstruction(OpCodes.Ldloc_S, 2),
                        new CodeInstruction(OpCodes.Ldloc_S, 3),
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Ldloc_S, 5),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,typeof(TransportTool).GetField("m_line", allFlags)),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,typeof(TransportTool).GetField("m_errors", allFlags)),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, typeof(TransportToolOverrides).GetMethod("ProcessTextTool"))
                    });
                    transpiled = true;
                    break;
                }
            }
            LogUtils.PrintMethodIL(instrList);
            if (!transpiled)
            {
                LogUtils.DoErrorLog("Transpilation failed!");
                throw new Exception("Transpilation failed!");
            }
            return instrList;
        }

        private static readonly ItemClass.SubService[] checkableTransportTypes = new ItemClass.SubService[]
        {
            ItemClass.SubService.PublicTransportBus ,
            ItemClass.SubService.PublicTransportTrolleybus,
            ItemClass.SubService.PublicTransportTram
        };

        public static string ProcessTextTool(string text, int mode, ushort lastEditLine, ushort tempLine, int hoverStopIndex, int hoverSegmentIndex, Vector3 hitPosition, ushort line, ToolBase.ToolErrors errors, TransportTool tt)
        {
            if (errors != ToolBase.ToolErrors.Pending && errors != ToolBase.ToolErrors.RaycastFailed)
            {
                if (mode == 1)//AddStops
                {
                    if (line > 0)
                    {
                        var stopId = TransportManager.instance.m_lines.m_buffer[tempLine].GetLastStop();
                        text += $"\n<color white>{TLMStationUtils.GetFullStationName(stopId, line, tt.m_prefab.m_class.m_subService)}</color> @ <color #{TransportManager.instance.GetLineColor(line).SetBrightness(1).ClampSaturation(.5f).ToRGB()}>{TransportManager.instance.GetLineName(line)}</color>";
                        text += ProcessNeighborStops(line, tt, stopId);
                    }
                }
                else if (mode == 2) //Move Stops
                {
                    if ((hoverStopIndex != -1 || hoverSegmentIndex != -1) && line > 0)
                    {
                        var stopId = TransportManager.instance.m_lines.m_buffer[tempLine].GetStop(Math.Max(hoverStopIndex, hoverSegmentIndex));
                        text += $"\n<color white>{TLMStationUtils.GetFullStationName(stopId, line, tt.m_prefab.m_class.m_subService)}</color> @ <color #{TransportManager.instance.GetLineColor(line).SetBrightness(1).ClampSaturation(.5f).ToRGB()}>{TransportManager.instance.GetLineName(line)}</color>";
                        text += ProcessNeighborStops(line, tt, stopId);
                    }
                }
                else if (mode == 0)// New Line
                {
                    if (hoverStopIndex != -1 && lastEditLine > 0)
                    {
                        var stopId = TransportManager.instance.m_lines.m_buffer[lastEditLine].GetStop(hoverStopIndex);
                        text += $"\n<color white>{TLMStationUtils.GetFullStationName(stopId, lastEditLine, tt.m_prefab.m_class.m_subService)}</color> @ <color #{TransportManager.instance.GetLineColor(lastEditLine).SetBrightness(1).ClampSaturation(.5f).ToRGB()}>{TransportManager.instance.GetLineName(lastEditLine)}</color>";
                        text += ProcessNeighborStops(lastEditLine, tt, stopId);
                    }
                    else if (hoverSegmentIndex != -1 && lastEditLine > 0)
                    {
                        var prevStopId = TransportManager.instance.m_lines.m_buffer[lastEditLine].GetStop(hoverSegmentIndex);
                        var nextStopId = TransportLine.GetNextStop(prevStopId);

                        ref NetSegment segm = ref NetManager.instance.m_segments.m_buffer[TransportLine.GetNextSegment(prevStopId)];


                        string c0 = checkableTransportTypes.Contains(tt.m_prefab.m_class.m_subService) ? GetDistanceColor(segm.m_averageLength) : "white";
                        var d0 = $"<color {c0}>{segm.m_averageLength.ToString("N0")}m</color>";

                        text +=
                            $"\n<color #{TransportManager.instance.GetLineColor(lastEditLine).SetBrightness(1).ClampSaturation(.5f).ToRGB()}>{TransportManager.instance.GetLineName(lastEditLine)}</color>" +
                            $"\n{Locale.Get("K45_TLM_TOOLINFO_PREVSTOP")}: {TLMStationUtils.GetFullStationName(prevStopId, lastEditLine, tt.m_prefab.m_class.m_subService)}" +
                            $"\n{Locale.Get("K45_TLM_TOOLINFO_NEXTSTOP")}: {TLMStationUtils.GetFullStationName(nextStopId, lastEditLine, tt.m_prefab.m_class.m_subService)}" +
                            $"\n{Locale.Get("K45_TLM_TOOLINFO_SEGMENTLENGHT")}: {d0}";
                    }
                }
            }
            return text;
        }

        private static string ProcessNeighborStops(ushort line, TransportTool tt, ushort stopId)
        {
            var nm = NetManager.instance;
            var seg0Id = nm.m_nodes.m_buffer[stopId].m_segment0;
            var seg1Id = nm.m_nodes.m_buffer[stopId].m_segment1;

            ref NetSegment seg0 =ref nm.m_segments.m_buffer[seg0Id];
            ref NetSegment seg1 =ref nm.m_segments.m_buffer[seg1Id];
            bool invert = seg0.m_endNode == stopId;

            return invert
                ? PrintDistances(line, tt, stopId, seg0Id, seg1Id, ref seg0, ref seg1)
                : PrintDistances(line, tt, stopId, seg1Id, seg0Id, ref seg1, ref seg0);
        }

        private static string PrintDistances(ushort lastEditLine, TransportTool tt, ushort stopId, ushort seg0Id, ushort seg1Id, ref NetSegment seg0, ref NetSegment seg1)
        {
            string c0 = checkableTransportTypes.Contains(tt.m_prefab.m_class.m_subService) ? GetDistanceColor(seg0.m_averageLength) : "white";
            var c1 = checkableTransportTypes.Contains(tt.m_prefab.m_class.m_subService) ? GetDistanceColor(seg1.m_averageLength) : "white";
            var d0 = $"<color {c0}>{seg0.m_averageLength.ToString("N0")}m</color>";
            var d1 = $"<color {c1}>{seg1.m_averageLength.ToString("N0")}m</color>";

            return (seg0Id > 0 ? $"\n{Locale.Get("K45_TLM_TOOLINFO_PREVSTOP")}: {d0} @ {TLMStationUtils.GetFullStationName(seg0.GetOtherNode(stopId), lastEditLine, tt.m_prefab.m_class.m_subService)}" : "") +
                (seg1Id > 0 ? $"\n{Locale.Get("K45_TLM_TOOLINFO_NEXTSTOP")}: {d1} @ {TLMStationUtils.GetFullStationName(seg1.GetOtherNode(stopId), lastEditLine, tt.m_prefab.m_class.m_subService)}" : "");
        }

        private static string GetDistanceColor(float averageLength) => averageLength < 100 ? "red" : averageLength < 250 ? "yellow" : averageLength > 2500 ? "yellow" : "green";
    }
}
