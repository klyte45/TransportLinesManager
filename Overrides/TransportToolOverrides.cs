using ColossalFramework.Threading;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    internal class TransportToolOverrides : MonoBehaviour, IRedirectable
    {
        public void Awake()
        {

            #region Automation Hooks
            MethodInfo onEnable = typeof(TransportToolOverrides).GetMethod("OnEnable", allFlags);
            MethodInfo onDisable = typeof(TransportToolOverrides).GetMethod("OnDisable", allFlags);
            MethodInfo AfterEveryAction = typeof(TransportToolOverrides).GetMethod("AfterEveryAction", allFlags);
            MethodInfo AfterEveryActionZeroable = typeof(TransportToolOverrides).GetMethod("AfterEveryActionZeroable", allFlags);

            TLMUtils.doLog($"Loading TransportToolOverrides Hook");
            try
            {
                var tt = new TransportTool();
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnEnable", allFlags), onEnable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnDisable", allFlags), onDisable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("NewLine", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags),  AfterEveryAction);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("AddStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags),  AfterEveryAction);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("RemoveStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags),  AfterEveryActionZeroable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("CancelPrevStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags),  AfterEveryActionZeroable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("CancelMoveStop", allFlags).Invoke(tt, new object[0]).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags),  AfterEveryActionZeroable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("MoveStop", allFlags).Invoke(tt, new object[] { false }).GetType().GetMethod("MoveNext", RedirectorUtils.allFlags),  AfterEveryAction);
                Destroy(tt);
            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog("ERRO AO CARREGAR HOOKS: {0}\n{1}", e.Message, e.StackTrace);
            }

            #endregion

        }

        public static void OnEnable()
        {
            TLMUtils.doLog("OnEnableTransportTool");
            TransportLinesManagerMod.Instance.ShowVersionInfoPopup();
            TLMController.instance.LinearMapCreatingLine?.setVisible(TLMController.LinearMapWhileCreatingLineVisibility);
            TLMController.instance.LineCreationToolbox?.setVisible(true);
            TLMController.instance.SetCurrentSelectedId(0);
        }

        public static void OnDisable()
        {
            TLMUtils.doLog("OnDisableTransportTool");
            TLMController.instance.SetCurrentSelectedId(0);
            TLMController.instance.LinearMapCreatingLine?.setVisible(false);
            TLMController.instance.LineCreationToolbox?.setVisible(false);
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
            ushort lineId = (ushort)m_lineNumField.GetValue(ToolManager.instance.m_properties?.CurrentTool as TransportTool ?? new TransportTool());
            if (lineId > 0)
            {
                new WaitForFixedUpdate();
                ThreadHelper.dispatcher.Dispatch(() =>
                {
                    TLMController.RedrawMap(lineId);
                    TLMController.instance.LineCreationToolbox.syncForm();
                });
            }
        }
        public static void AfterEveryActionZeroable()
        {
            ushort lineId = (ushort)m_lineNumField.GetValue(ToolManager.instance.m_properties?.CurrentTool as TransportTool ?? new TransportTool());
            new WaitForFixedUpdate();
            ThreadHelper.dispatcher.Dispatch(() =>
            {
                TLMController.RedrawMap(lineId);
                TLMController.instance.LineCreationToolbox.syncForm();
            });

        }

        public Redirector RedirectorInstance { get; } = new Redirector();


    }
}
