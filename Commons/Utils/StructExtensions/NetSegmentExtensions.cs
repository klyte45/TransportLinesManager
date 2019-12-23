using ColossalFramework;
using ColossalFramework.Math;

namespace Klyte.Commons.Utils
{
    public static class NetSegmentExtensions
    {
        public static Bezier3 GetBezier(this NetSegment segment)
        {
            Bezier3 bezier = default;
            bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_startNode].m_position;
            bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_endNode].m_position;
            bool smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            NetSegment.CalculateMiddlePoints(bezier.a, segment.m_startDirection, bezier.d, segment.m_endDirection, smoothStart, smoothEnd, out bezier.b, out bezier.c);
            return bezier;
        }
    }
}
