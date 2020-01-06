using ColossalFramework;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    internal class TransportToolOverrides : MonoBehaviour, IRedirectable
    {

        #region Hooking
        public void Awake()
        {

            #region Automation Hooks
            MethodInfo onEnable = typeof(TransportToolOverrides).GetMethod("OnEnable", allFlags);
            MethodInfo onDisable = typeof(TransportToolOverrides).GetMethod("OnDisable", allFlags);
            MethodInfo OnToolGUIPos = typeof(TransportToolOverrides).GetMethod("OnToolGUIPos", allFlags);
            MethodInfo SimulationStepPos = typeof(TransportToolOverrides).GetMethod("SimulationStepPos", allFlags);

            TLMUtils.doLog("Loading TransportToolOverrides Hook");
            try
            {
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnEnable", allFlags), onEnable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnDisable", allFlags), onDisable);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("OnToolGUI", allFlags), null, OnToolGUIPos);
                RedirectorInstance.AddRedirect(typeof(TransportTool).GetMethod("SimulationStep", allFlags), null, SimulationStepPos);
            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog("ERRO AO CARREGAR HOOKS: {0}", e.StackTrace);
            }
            #endregion


        }
        #endregion
        private static readonly FieldInfo m_tt_lineCurrent = typeof(TransportTool).GetField("m_line", allFlags);
        private static readonly FieldInfo m_tt_lineTemp = typeof(TransportTool).GetField("m_tempLine", allFlags);
        private static readonly FieldInfo m_tt_mode = typeof(TransportTool).GetField("m_mode", allFlags);


        public static void OnEnable()
        {
            TLMUtils.doLog("OnEnableTransportTool");
            TransportLinesManagerMod.Instance.ShowVersionInfoPopup();
            TLMController.instance.LinearMapCreatingLine?.setVisible(true);
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

        private static ToolStatus m_lastState = new ToolStatus();
        private static float m_lastLength = 0;
        private static bool m_needsUpdate = false;
        private static TransportTool m_ttInstance;

        public TransportToolOverrides()
        {
        }

        private static bool IsInsideUI => Singleton<ToolController>.instance.IsInsideUI;

        private static bool HasInputFocus => Singleton<ToolController>.instance.HasInputFocus;

        public Redirector RedirectorInstance => new Redirector();

        public static void OnToolGUIPos(ref TransportTool __instance, ref Event e)
        {
            lock (__instance)
            {
                if (e.type == EventType.MouseUp && !IsInsideUI)
                {
                    m_ttInstance = __instance;
                    m_needsUpdate = true;
                }
            }
        }

        public static void SimulationStepPos(ref TransportTool __instance)
        {
            if (m_lastState.m_lineCurrent > 0 && Math.Abs(m_lastLength - Singleton<TransportManager>.instance.m_lines.m_buffer[m_lastState.m_lineCurrent].m_totalLength) > 0.001f)
            {
                m_ttInstance = __instance;
                m_needsUpdate = true;
            }
        }

        private static void RedrawMap(ToolStatus __state)
        {
            if (__state.m_lineCurrent > 0 || (Singleton<TransportManager>.instance.m_lines.m_buffer[TLMController.instance.CurrentSelectedId].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
            {
                TLMController.instance.SetCurrentSelectedId(__state.m_lineCurrent);
                if (__state.m_lineCurrent > 0 && TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
                {
                    TLMController.AutoColor(__state.m_lineCurrent, true, true);
                }
                TLMController.instance.LinearMapCreatingLine.redrawLine();
                m_lastLength = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lastState.m_lineCurrent].m_totalLength;
            }
        }

        private struct ToolStatus
        {
            public Mode m_mode;
            public ushort m_lineTemp;
            public ushort m_lineCurrent;

            public override string ToString() => $"mode={m_mode};lineTemp={m_lineTemp};lineCurrent={m_lineCurrent}";

        }
        private enum Mode
        {
            NewLine,
            AddStops,
            MoveStops
        }

        public void Update()
        {
            if (m_needsUpdate && m_ttInstance != null)
            {
                TLMUtils.doLog("OnToolGUIPostTransportTool");
                var currentState = new ToolStatus();
                TLMUtils.doLog("__state => {0} | tt_mode=> {1} | tt_lineCurrent => {2}", currentState, m_tt_mode, m_tt_lineCurrent);
                currentState.m_mode = (Mode) m_tt_mode.GetValue(m_ttInstance);
                currentState.m_lineCurrent = (ushort) m_tt_lineCurrent.GetValue(m_ttInstance);
                currentState.m_lineTemp = (ushort) m_tt_lineTemp.GetValue(m_ttInstance);
                TLMUtils.doLog("__state = {0} => {1}, newMode = {2}", m_lastState, currentState, currentState.m_mode);
                m_lastState = currentState;
                RedrawMap(currentState);
                m_needsUpdate = false;
            }
        }
    }
}
