using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Klyte.Commons.Overrides
{
    public class NetManagerOverrides : Redirector<NetManagerOverrides>
    {


        #region Events
        public static event Action<ushort> eventNodeChanged;
        public static event Action<ushort> eventSegmentChanged;
        public static event Action<ushort> eventSegmentReleased;
        public static event Action<ushort> eventSegmentNameChanged;

        private static void OnNodeChanged(ref ushort node)
        {
            var node_ = node;
            new AsyncAction(() =>
            {
                eventNodeChanged?.Invoke(node_);
            }).Execute();
        }
        private static void OnSegmentCreated(ref ushort segment, ref ushort startNode, ref ushort endNode)
        {
            var startNode_ = startNode;
            var segment_ = segment;
            var endNode_ = endNode;

            new AsyncAction(() =>
            {
                eventNodeChanged?.Invoke(startNode_);
                eventNodeChanged?.Invoke(endNode_);
                eventSegmentChanged?.Invoke(segment_);
            }).Execute();
        }
        private static void OnSegmentReleased(ref ushort segment)
        {
            var segment_ = segment;
            new AsyncAction(() =>
            {
                eventNodeChanged?.Invoke(NetManager.instance.m_segments.m_buffer[segment_].m_startNode);
                eventNodeChanged?.Invoke(NetManager.instance.m_segments.m_buffer[segment_].m_endNode);
                eventSegmentChanged?.Invoke(segment_);
                eventSegmentReleased?.Invoke(segment_);
            }).Execute();
        }
        private static void OnSegmentNameChanged(ref ushort segmentID)
        {
            var segment_ = segmentID;
            new AsyncAction(() =>
            {
                eventSegmentNameChanged?.Invoke(segment_);
            }).Execute();
        }
        #endregion

        #region Hooking

        public override void AwakeBody()
        {
            KlyteUtils.doLog("Loading Net Manager Overrides");
            #region Net Manager Hooks
            MethodInfo OnNodeChanged = GetType().GetMethod("OnNodeChanged", allFlags);
            MethodInfo OnSegmentCreated = GetType().GetMethod("OnSegmentCreated", allFlags);
            MethodInfo OnSegmentReleased = GetType().GetMethod("OnSegmentReleased", allFlags);
            MethodInfo OnSegmentNameChanged = GetType().GetMethod("OnSegmentNameChanged", allFlags);

            AddRedirect(typeof(NetManager).GetMethod("CreateNode", allFlags), null, OnNodeChanged);
            AddRedirect(typeof(NetManager).GetMethod("ReleaseNode", allFlags), null, OnNodeChanged);
            AddRedirect(typeof(NetManager).GetMethod("CreateSegment", allFlags), null, OnSegmentCreated);
            AddRedirect(typeof(NetManager).GetMethod("ReleaseSegment", allFlags), OnSegmentReleased);
            AddRedirect(typeof(NetManager).GetMethod("SetSegmentNameImpl", allFlags), null, OnSegmentNameChanged);
            #endregion

        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            KlyteUtils.doLog(text, param);
        }

    }
}
