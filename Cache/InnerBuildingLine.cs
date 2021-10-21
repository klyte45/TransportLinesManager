using Klyte.Commons.Utils;
using System;

namespace Klyte.TransportLinesManager.Cache
{
    public class InnerBuildingLine
    {
        public TransportInfo Info { get; set; }
        public ushort SrcStop { get; set; }
        public ushort DstStop { get; set; }
        public bool BrokenFromSrc { get; set; }
        public bool BrokenFromDst { get; set; }

        public int CountStops()
        {
            int num = 0;
            ushort stops = SrcStop;
            ushort num2 = stops;
            int num3 = 0;
            while (num2 != 0)
            {
                num++;
                num2 = TransportLine.GetNextStop(num2);
                if (num2 == stops)
                {
                    break;
                }
                if (++num3 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num;
        }
        public ushort GetStop(int index)
        {
            if (index == -1)
            {
                return GetLastStop();
            }
            ushort stops = SrcStop;
            ushort num = stops;
            int num2 = 0;
            while (num != 0)
            {
                if (index-- == 0)
                {
                    return num;
                }
                num = TransportLine.GetNextStop(num);
                if (num == stops)
                {
                    break;
                }
                if (++num2 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }
        public ushort GetLastStop()
        {
            NetManager instance = NetManager.instance;
            ushort num = SrcStop;
            int num2 = 0;
            for (; ; )
            {
                bool flag = false;
                int i = 0;
                while (i < 8)
                {
                    ushort segment = instance.m_nodes.m_buffer[num].GetSegment(i);
                    if (segment != 0 && instance.m_segments.m_buffer[segment].m_startNode == num)
                    {
                        num = instance.m_segments.m_buffer[segment].m_endNode;
                        if (num == SrcStop)
                        {
                            return num;
                        }
                        flag = true;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                if (++num2 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    return num;
                }
                if (!flag)
                {
                    return num;
                }
            }
        }

    }
}