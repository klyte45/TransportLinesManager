using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{
    public class NetManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; set; }


        #region Events
        public static event Action<ushort> EventNodeChanged;
        public static event Action<ushort> EventSegmentChanged;
        public static event Action<ushort> EventSegmentReleased;
        public static event Action<ushort> EventSegmentNameChanged;

#pragma warning disable IDE0051 // Remover membros privados não utilizados
        private static void OnNodeChanged(ref ushort node)
        {
            ushort node_ = node;
            SimulationManager.instance.AddAction(() => EventNodeChanged?.Invoke(node_)).Execute();
        }
        private static void OnSegmentCreated(ref ushort segment, ref ushort startNode, ref ushort endNode)
        {
            ushort startNode_ = startNode;
            ushort segment_ = segment;
            ushort endNode_ = endNode;

            SimulationManager.instance.AddAction(() =>
            {
                EventNodeChanged?.Invoke(startNode_);
                EventNodeChanged?.Invoke(endNode_);
                EventSegmentChanged?.Invoke(segment_);
            }).Execute();
        }
        private static void OnSegmentReleased(ref ushort segment)
        {
            ushort segment_ = segment;
            SimulationManager.instance.AddAction(() =>
            {
                EventNodeChanged?.Invoke(NetManager.instance.m_segments.m_buffer[segment_].m_startNode);
                EventNodeChanged?.Invoke(NetManager.instance.m_segments.m_buffer[segment_].m_endNode);
                EventSegmentChanged?.Invoke(segment_);
                EventSegmentReleased?.Invoke(segment_);
            }).Execute();
        }
        private static void OnSegmentNameChanged(ref ushort segmentID)
        {
            ushort segment_ = segmentID;
            SimulationManager.instance.AddAction(() => EventSegmentNameChanged?.Invoke(segment_)).Execute();
        }
        #endregion
#pragma warning restore IDE0051 // Remover membros privados não utilizados

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading Net Manager Overrides");
            RedirectorInstance = KlyteMonoUtils.CreateElement<Redirector>(transform);
            #region Net Manager Hooks
            MethodInfo OnNodeChanged = GetType().GetMethod("OnNodeChanged", RedirectorUtils.allFlags);
            MethodInfo OnSegmentCreated = GetType().GetMethod("OnSegmentCreated", RedirectorUtils.allFlags);
            MethodInfo OnSegmentReleased = GetType().GetMethod("OnSegmentReleased", RedirectorUtils.allFlags);
            MethodInfo OnSegmentNameChanged = GetType().GetMethod("OnSegmentNameChanged", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("CreateNode", RedirectorUtils.allFlags), null, OnNodeChanged);
            RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("ReleaseNode", RedirectorUtils.allFlags), null, OnNodeChanged);
            RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("CreateSegment", RedirectorUtils.allFlags), null, OnSegmentCreated);
            RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("ReleaseSegment", RedirectorUtils.allFlags), OnSegmentReleased);
            RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("SetSegmentNameImpl", RedirectorUtils.allFlags), null, OnSegmentNameChanged);
            #endregion

        }
        #endregion


    }
}
