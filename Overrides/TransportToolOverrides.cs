using ColossalFramework.Threading;
using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
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
            MethodInfo AfterEveryAction = typeof(TransportToolOverrides).GetMethod("AfterEveryAction", allFlags);
            MethodInfo AfterEveryActionZeroable = typeof(TransportToolOverrides).GetMethod("AfterEveryActionZeroable", allFlags);

            LogUtils.DoLog($"Loading TransportToolOverrides Hook");
            try
            {
                var tt = new TransportTool();
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnEnable", allFlags), null, onEnable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnDisable", allFlags), onDisable);
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
            TLMController.Instance.LinearMapCreatingLine?.setVisible(TLMController.LinearMapWhileCreatingLineVisibility);
            TLMController.Instance.LineCreationToolbox?.SetVisible(true);
            TLMController.Instance.SetCurrentSelectedId(0);
        }

        public static void OnDisable()
        {
            LogUtils.DoLog("OnDisableTransportTool");
            TLMController.Instance.SetCurrentSelectedId(0);
            TLMController.Instance.LinearMapCreatingLine?.setVisible(false);
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
            if (ToolManager.instance.m_properties?.CurrentTool is TransportTool)
            {
                ushort lineId = (ushort)m_lineNumField.GetValue(ToolManager.instance.m_properties.CurrentTool as TransportTool);
                if (lineId > 0)
                {
                    new WaitForFixedUpdate();
                    ThreadHelper.dispatcher.Dispatch(() =>
                    {
                        TLMController.RedrawMap(lineId);
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
                    TLMController.RedrawMap(lineId);
                    TLMController.Instance.LineCreationToolbox.SyncForm();
                });

            }
        }

        public Redirector RedirectorInstance { get; } = new Redirector();


    }
}
