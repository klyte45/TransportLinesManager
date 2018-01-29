using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Overrides
{
    class TransportToolOverrides : Redirector<TransportToolOverrides>
    {

        #region Hooking

        private static bool preventDefault()
        {
            return false;
        }

        public override void Awake()
        {
            MethodInfo preventDefault = typeof(TransportToolOverrides).GetMethod("preventDefault", allFlags);

            #region Automation Hooks
            MethodInfo onEnable = typeof(TransportToolOverrides).GetMethod("OnEnable", allFlags);
            MethodInfo onDisable = typeof(TransportToolOverrides).GetMethod("OnDisable", allFlags);
            MethodInfo OnToolGUIPos = typeof(TransportToolOverrides).GetMethod("OnToolGUIPos", allFlags);
            MethodInfo SimulationStepPos = typeof(TransportToolOverrides).GetMethod("SimulationStepPos", allFlags);

            TLMUtils.doLog("Loading TransportToolOverrides Hook");
            try
            {
                AddRedirect(typeof(TransportTool).GetMethod("OnEnable", allFlags), onEnable);
                AddRedirect(typeof(TransportTool).GetMethod("OnDisable", allFlags), onDisable);
                AddRedirect(typeof(TransportTool).GetMethod("OnToolGUI", allFlags), null, OnToolGUIPos);
                AddRedirect(typeof(TransportTool).GetMethod("SimulationStep", allFlags), null, SimulationStepPos);
            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog("ERRO AO CARREGAR HOOKS: {0}", e.StackTrace);
            }
            #endregion


        }
        #endregion
        static FieldInfo tt_lineCurrent = typeof(TransportTool).GetField("m_line", allFlags);
        static FieldInfo tt_lineTemp = typeof(TransportTool).GetField("m_tempLine", allFlags);
        static FieldInfo tt_mode = typeof(TransportTool).GetField("m_mode", allFlags);


        private static void OnEnable()
        {
            TLMUtils.doLog("OnEnableTransportTool");
            TLMSingleton.instance.showVersionInfoPopup();
            TLMController.instance.LinearMapCreatingLine?.setVisible(true);
            TLMController.instance.LineCreationToolbox?.setVisible(true);
            TLMController.instance.setCurrentSelectedId(0);
            TLMController.instance.LinearMapCreatingLine?.redrawLine();
            TLMController.instance.lineInfoPanel?.Hide();
        }

        private static void OnDisable()
        {
            TLMUtils.doLog("OnDisableTransportTool");
            TLMController.instance.setCurrentSelectedId(0);
            TLMController.instance.LinearMapCreatingLine?.setVisible(false);
            TLMController.instance.LineCreationToolbox?.setVisible(false);
        }

        private static ToolStatus lastState = new ToolStatus();
        private static float lastLength = 0;
        private static int frameCountRedraw = 0;

        public static void resetLength()
        {
            lastLength = 0;
        }
        private static bool isInsideUI
        {
            get {
                return Singleton<ToolController>.instance.IsInsideUI;
            }
        }

        private static bool HasInputFocus
        {
            get {
                return Singleton<ToolController>.instance.HasInputFocus;
            }
        }

        private static void OnToolGUIPos(ref TransportTool __instance, ref Event e)
        {
            if (e.type == EventType.MouseUp && !isInsideUI)
            {
                TLMUtils.doLog("OnToolGUIPostTransportTool");
                ToolStatus currentState = new ToolStatus();
                TLMUtils.doLog("__state => {0} | tt_mode=> {1} | tt_lineCurrent => {2}", currentState, tt_mode, tt_lineCurrent);
                currentState.m_mode = (Mode)tt_mode.GetValue(__instance);
                currentState.m_lineCurrent = (ushort)tt_lineCurrent.GetValue(__instance);
                currentState.m_lineTemp = (ushort)tt_lineTemp.GetValue(__instance);
                TLMUtils.doLog("__state = {0} | {1}, newMode = {2}", lastState.m_mode, lastState.m_lineCurrent, currentState.m_mode);
                redrawMap(currentState);
                lastState = currentState;
                resetLength();
            }
            if (TLMController.instance.LineCreationToolbox.isVisible())
            {
                TLMController.instance.LineCreationToolbox.eachFrame();
            }
        }

        private static void SimulationStepPos(ref TransportTool __instance)
        {
            if (lastState.m_lineCurrent > 0 && lastLength != Singleton<TransportManager>.instance.m_lines.m_buffer[lastState.m_lineCurrent].m_totalLength)
            {
                if (frameCountRedraw < 5 || HasInputFocus)
                {
                    frameCountRedraw++;
                }
                else
                {
                    frameCountRedraw = 0;
                    lastLength = Singleton<TransportManager>.instance.m_lines.m_buffer[lastState.m_lineCurrent].m_totalLength;
                    TLMController.instance.LinearMapCreatingLine.redrawLine();
                }
            }
        }

        private static void redrawMap(ToolStatus __state)
        {
            if (__state.m_lineCurrent > 0 || (Singleton<TransportManager>.instance.m_lines.m_buffer[TLMController.instance.CurrentSelectedId].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
            {
                TLMController.instance.setCurrentSelectedId(__state.m_lineCurrent);
                if (__state.m_lineCurrent > 0 && TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
                {
                    TLMController.instance.AutoColor(__state.m_lineCurrent);
                }
            }
            TLMController.instance.LinearMapCreatingLine.redrawLine();
        }

        private struct ToolStatus
        {
            public Mode m_mode;
            public ushort m_lineTemp;
            public ushort m_lineCurrent;

        }
        private enum Mode
        {
            NewLine,
            AddStops,
            MoveStops
        }

    }
}
