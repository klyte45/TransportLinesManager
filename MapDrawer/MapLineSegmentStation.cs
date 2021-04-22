using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.MapDrawer
{
    public class LineSegmentStationsManager
    {
        private Dictionary<int, List<Range<int>>> hRanges = new Dictionary<int, List<Range<int>>>();
        private Dictionary<int, List<Range<int>>> vRanges = new Dictionary<int, List<Range<int>>>();
        /// <summary>
        ///x+y; x range
        /// </summary>
        private Dictionary<int, List<Range<int>>> d1Ranges = new Dictionary<int, List<Range<int>>>();
        /// <summary>
        ///x-y; x range
        /// </summary>
        private Dictionary<int, List<Range<int>>> d2Ranges = new Dictionary<int, List<Range<int>>>();
        private Dictionary<int, LineSegmentStations> segments = new Dictionary<int, LineSegmentStations>();
       

     

        //d1 = x+y; d2 = x-y
        private Vector2 getIntersectionPoint(int d1Index, int d2Index)
        {
            return new Vector2((d1Index + d2Index) / 2, (d1Index - d2Index) / 2);
        }

        protected List<Vector2> getPathForStations(Station s1, Station s2)
        {
            LogUtils.DoLog("Calculando caminho entre '{0}' e '{1}' ", s1.name, s2.name);
            Vector2 originalPos = s1.centralPos;
            Vector2 toPos = s2.centralPos;

            CardinalPoint s1Exit = s1.reserveExit(s2);
            CardinalPoint s2Exit = s2.reserveExit(s1);
            return getPath(originalPos, toPos, s1Exit, s2Exit);
        }
        public List<Vector2> getPath(Vector2 originalPos, Vector2 toPos, CardinalPoint s1Exit, CardinalPoint s2Exit)
        {
            //var angle = originalPos.GetAngleToPoint(toPos);
            //var tan = Math.Tan(angle / Mathf.Rad2Deg - 0.000001);
            Vector2 p1, p2;
            CardinalPoint c1, c2;
            //if (angle == 90 || angle == 270 || tan >= 0)
            //{
            p1 = originalPos;
            p2 = toPos;
            c1 = s1Exit;
            c2 = s2Exit;
            //}
            //else
            //{
            //    p1 = toPos;
            //    p2 = originalPos;
            //    c1 = s2Exit;
            //    c2 = s1Exit;
            //}



            List<Vector2> saida = new List<Vector2>();

            GetPointsLine(ref p1, ref p2, ref c1, ref c2, ref saida);

            for (int i = 1; i < saida.Count; i++)
            {
                addSegmentToIndex(saida[i - 1], saida[i]);
            }
            if (TransportLinesManagerMod.DebugMode)
            {
                string points = string.Join(",", saida.Select(x => "(" + x.x + "," + x.y + ")").ToArray());
                LogUtils.DoLog("Points: [{0}]", points);
            }

            return saida;

        }

        private void GetPointsLine(ref Vector2 p1, ref Vector2 p2, ref CardinalPoint c1, ref CardinalPoint c2, ref List<Vector2> saida, int retrying = 0)
        {

            if (retrying > 7)
            {
                LogUtils.DoLog("MAX RETRYING REACHED!");
                return;
            }

            saida.Add(p1);

            CardinalPoint currentDirection = c1;


            //int iterationCount = 0;
            //DiagCheck:
            //iterationCount++;
            var dirS1 = c1.GetCardinalOffset2D();
            var dirS2 = c2.GetCardinalOffset2D();

            LogUtils.DoLog("c1 = {0};dirS1 = {1}", c1, dirS1);
            LogUtils.DoLog("c2 = {0};dirS2 = {1}", c2, dirS2);


            Vector2 currentPos = p1 + dirS1;
            Vector2 targetPos = p2 + dirS2;

            saida.Add(currentPos);

            var isHorizontalS1 = Math.Abs(dirS1.x) > Math.Abs(dirS1.y);
            var isVerticalS1 = Math.Abs(dirS1.x) < Math.Abs(dirS1.y);
            var isD1S1 = (dirS1.y + dirS1.x) == 0 && (dirS1.x - dirS1.y) != 0;
            var isD2S1 = (dirS1.x - dirS1.y) == 0 && (dirS1.y + dirS1.x) != 0;

            var isHorizontalS2 = Math.Abs(dirS2.x) > Math.Abs(dirS2.y);
            var isVerticalS2 = Math.Abs(dirS2.x) < Math.Abs(dirS2.y);
            var isD1S2 = (dirS2.y + dirS2.x) == 0 && (dirS2.x - dirS2.y) != 0;
            var isD2S2 = (dirS2.x - dirS2.y) == 0 && (dirS2.y + dirS2.x) != 0;

            var Δx = targetPos.x - currentPos.x;
            var Δy = targetPos.y - currentPos.y;

            var iΔx = Math.Abs(Δx);
            var iΔy = Math.Abs(Δy);

            #region D1S1
            if (isD1S1)
            {
                int indexSumS1 = (int)(currentPos.x + currentPos.y);
                if (isD1S2)
                {
                    LogUtils.DoLog("(D1 - D1)");
                    int indexSumS2 = (int)(targetPos.x + targetPos.y);
                    if (indexSumS1 != indexSumS2)
                    {
                        saida.AddRange(pathParallelDiag(dirS1, dirS2, currentPos, targetPos));
                    }
                }
                else if (isD2S2)
                {
                    LogUtils.DoLog("(D1 - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    saida.AddRange(getMidPointsD1D2(currentPos, targetPos, dirS1, dirS2, indexSumS1, indexSubS2));
                }
                else if (isHorizontalS2)
                {
                    LogUtils.DoLog("(D1 - H)");
                    int s2y = (int)targetPos.y;
                    int targetX = indexSumS1 - s2y;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, targetX, s2y, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE! - RETRYING");
                        c1++;
                        GetPointsLine(ref currentPos, ref p2, ref c1, ref c2, ref saida, retrying++);
                    }

                }
                else if (isVerticalS2)
                {
                    LogUtils.DoLog("(D1 - V)");
                    int s2x = (int)targetPos.x;
                    int targetY = indexSumS1 - s2x;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, s2x, targetY, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE - RETRYING");
                        c1--;
                        GetPointsLine(ref currentPos, ref p2, ref c1, ref c2, ref saida, retrying++);
                    }
                }
            }
            #endregion
            #region D2S1
            else if (isD2S1)
            {
                int indexSubS1 = (int)(currentPos.x - currentPos.y);
                if (isD1S2)
                {
                    LogUtils.DoLog("(D2 - D1)");
                    int indexSumS2 = (int)(currentPos.x + currentPos.y);
                    var points = getMidPointsD1D2(targetPos, currentPos, dirS2, dirS1, indexSumS2, indexSubS1);
                    points.Reverse();
                    saida.AddRange(points);
                }
                else if (isD2S2)
                {
                    LogUtils.DoLog("(D2 - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    if (indexSubS1 != indexSubS2)
                    {
                        saida.AddRange(pathParallelDiag(dirS1, dirS2, currentPos, targetPos));
                    }

                }
                else if (isHorizontalS2)
                {
                    LogUtils.DoLog("(D2 - H)");
                    int s2y = (int)targetPos.y;
                    int targetX = indexSubS1 + s2y;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, targetX, s2y, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE! - RETRYING");
                        c1--;
                        GetPointsLine(ref currentPos, ref p2, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isVerticalS2)
                {
                    LogUtils.DoLog("(D2 - V)");
                    int s2x = (int)targetPos.x;
                    int targetY = indexSubS1 + s2x;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, s2x, targetY, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE! - RETRYING");
                        c1++;
                        GetPointsLine(ref currentPos, ref p2, ref c1, ref c2, ref saida, retrying++);
                    }
                }
            }
            #endregion
            #region HS1
            else if (isHorizontalS1)
            {
                int s1y = (int)(currentPos.y);
                if (isD1S2)
                {
                    LogUtils.DoLog("(H - D1)");
                    int indexSumS2 = (int)(targetPos.x + targetPos.y);
                    int targetX = indexSumS2 - s1y;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, targetX, s1y, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE! - RETRYING");
                        c2++;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isD2S2)
                {
                    LogUtils.DoLog("(H - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    int targetX = indexSubS2 + s1y;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, targetX, s1y, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE!- RETRYING");
                        c2--;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isHorizontalS2)
                {
                    LogUtils.DoLog("(H - H)");
                    if (currentPos.x != targetPos.x)
                    {
                        saida.AddRange(pathParallel(dirS1, dirS2, currentPos, targetPos));
                    }
                }
                else if (isVerticalS2)
                {
                    LogUtils.DoLog("(H - V)");

                    if (iΔx != iΔy)
                    {
                        int s2x = (int)currentPos.x;
                        var ΔiΔ = Math.Abs(iΔx - iΔy);
                        if (iΔx > iΔy)
                        {
                            s2x += (int)(Math.Sign(Δx) * (iΔx - ΔiΔ));
                        }
                        else
                        {
                            s1y += (int)(Math.Sign(Δy) * (iΔy - ΔiΔ));
                        }
                        if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, s2x, s1y, ref saida))
                        {
                            LogUtils.DoLog("WORST CASE! - RETRYING");
                            var nextP1 = new Vector2(p1.x + dirS1.x, p1.y + Math.Sign(Δy));
                            if (Math.Sign(Δy) == Math.Sign(dirS1.x))
                            {
                                c1++;
                            }
                            else
                            {
                                c1--;
                            }
                            GetPointsLine(ref nextP1, ref p2, ref c1, ref c2, ref saida, retrying++);
                        }
                    }
                }
            }
            #endregion 
            #region VS1
            else if (isVerticalS1)
            {
                int s1x = (int)(currentPos.x);
                if (isD1S2)
                {
                    LogUtils.DoLog("(V - D1)");
                    int indexSumS2 = (int)(targetPos.x + targetPos.y);
                    int targetY = indexSumS2 - s1x;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, s1x, targetY, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE! - RETRYING");
                        c2--;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isD2S2)
                {
                    LogUtils.DoLog("(V - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    int targetY = s1x - indexSubS2;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, s1x, targetY, ref saida))
                    {
                        LogUtils.DoLog("WORST CASE! - RETRYING");
                        c2++;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isHorizontalS2)
                {
                    LogUtils.DoLog("(V - H)");
                    if (iΔx != iΔy)
                    {
                        int s2y = (int)targetPos.y;
                        var ΔiΔ = Math.Abs(iΔx - iΔy);
                        if (iΔx > iΔy)
                        {
                            s1x += (int)(Math.Sign(Δx) * (iΔx - ΔiΔ));
                        }
                        else
                        {
                            s2y += (int)(Math.Sign(Δy) * (iΔy - ΔiΔ));
                        }
                        if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, s1x, s2y, ref saida))
                        {
                            LogUtils.DoLog("WORST CASE - RETRYING!");
                            var nextP1 = new Vector2(p1.x + Math.Sign(Δx), p1.y + dirS1.y);
                            if (Math.Sign(Δx) == Math.Sign(dirS1.y))
                            {
                                c1--;
                            }
                            else
                            {
                                c1++;
                            }
                            GetPointsLine(ref nextP1, ref p2, ref c1, ref c2, ref saida, retrying++);
                        }
                    }
                }
                else if (isVerticalS2)
                {
                    LogUtils.DoLog("(V - V)");
                    if (currentPos.y != targetPos.y)
                    {
                        saida.AddRange(pathParallel(dirS1, dirS2, currentPos, targetPos));
                    }
                }
            }
            #endregion 

            saida.Add(targetPos);
            saida.Add(p2);
        }

        private static List<Vector2> pathParallelDiag(Vector2 dirS1, Vector2 dirS2, Vector2 currentPos, Vector2 targetPos)
        {
            List<Vector2> saida = new List<Vector2>();
            float Δx = (int)Math.Abs(currentPos.x - targetPos.x);
            float Δy = (int)Math.Abs(currentPos.y - targetPos.y);
            var sigx = Math.Sign(targetPos.x - currentPos.x);
            var sigy = Math.Sign(targetPos.y - currentPos.y);

            bool midTargetReachableX = dirS2.x / dirS1.x < 0 && sigx / dirS2.x < 0;
            bool midTargetReachableY = dirS2.y / dirS1.y < 0 && sigy / dirS2.y < 0;

            if (midTargetReachableX && midTargetReachableY)
            {
                if (Δx < Δy)
                {
                    saida.Add(currentPos + new Vector2(Δx / 2 * sigx, Δx / 2 * sigy));
                    saida.Add(targetPos - new Vector2(Δx / 2 * sigx, Δx / 2 * sigy));
                }
                else
                {
                    saida.Add(currentPos + new Vector2(Δy / 2 * sigx, Δy / 2 * sigy));
                    saida.Add(targetPos - new Vector2(Δy / 2 * sigx, Δy / 2 * sigy));
                }
            }
            else
            {
                LogUtils.DoErrorLog("WORST CASE (midTargetReachableX = {0}, midTargetReachableY= {1})", midTargetReachableX, midTargetReachableY);
            }
            return saida;
        }


        private static List<Vector2> pathParallel(Vector2 dirS1, Vector2 dirS2, Vector2 currentPos, Vector2 targetPos)
        {
            List<Vector2> saida = new List<Vector2>();
            float Δx = (int)Math.Abs(currentPos.x - targetPos.x);
            float Δy = (int)Math.Abs(currentPos.y - targetPos.y);
            var sigx = Math.Sign(targetPos.x - currentPos.x);
            var sigy = Math.Sign(targetPos.y - currentPos.y);

            bool midTargetReachableX = dirS2.x / dirS1.x < 0 || dirS1.x == 0;
            bool midTargetReachableY = dirS2.y / dirS1.y < 0 || dirS1.y == 0;

            if (midTargetReachableX && midTargetReachableY)
            {
                if (Δx < Δy)
                {
                    saida.Add(currentPos + new Vector2(Δx / 2 * sigx, Δx / 2 * sigy));
                    saida.Add(targetPos - new Vector2(Δx / 2 * sigx, Δx / 2 * sigy));
                }
                else
                {
                    saida.Add(currentPos + new Vector2(Δy / 2 * sigx, Δy / 2 * sigy));
                    saida.Add(targetPos - new Vector2(Δy / 2 * sigx, Δy / 2 * sigy));
                }
            }
            else
            {
                LogUtils.DoErrorLog("WORST CASE (midTargetReachableX = {0}, midTargetReachableY= {1})", midTargetReachableX, midTargetReachableY);
            }
            return saida;
        }

        private static bool calcIntersecHV(Vector2 pV, Vector2 pH, Vector2 Δy, Vector2 Δx, int targetX, int targetY, ref List<Vector2> saida)
        {
            Vector2 midTarget = new Vector2(targetX, targetY);
            bool midTargetReachableS1 = (midTarget.y - pV.y) / Δy.y > 0;
            bool midTargetReachableS2 = (midTarget.x - pH.x) / Δx.x > 0;
            if (midTargetReachableS1 && midTargetReachableS2)
            {
                saida.Add(midTarget);
                return true;
            }
            else
            {
                LogUtils.DoErrorLog("WORST CASE (midTargetReachableS1={0};midTargetReachableS2={1})", midTargetReachableS1, midTargetReachableS2);
                return false;
            }
        }

        private List<Vector2> getMidPointsD1D2(Vector2 p1, Vector2 p2, Vector2 dirP1, Vector2 dirP2, int indexSumP1, int indexSubP2)
        {
            List<Vector2> saida = new List<Vector2>();
            Vector2 intersection = getIntersectionPoint(indexSumP1, indexSubP2);
            int currentMidx = (int)(indexSumP1 - intersection.y);
            int targetMidx = (int)(indexSubP2 + intersection.y);

            bool currentReachble = currentMidx / dirP1.x > 0;
            bool targetReachble = targetMidx / dirP2.x > 0;
            if (currentReachble && targetReachble)
            {
                if (p2.x != p1.x && p2.y != p1.y)
                {
                    var Δx = (int)Math.Abs(p1.x - p2.x);
                    var Δy = (int)Math.Abs(p1.y - p2.y);
                    var sigx = Math.Sign(p2.x - p1.x);
                    var sigy = Math.Sign(p2.y - p1.y);
                    if (Δx < Δy)
                    {
                        saida.Add(p1 + new Vector2(Δx * sigx, Δx * sigy));
                    }
                    else
                    {
                        saida.Add(p1 + new Vector2(Δy * sigx, Δy * sigy));
                    }
                }
            }
            else
            {
                LogUtils.DoErrorLog("WORST CASE (currentReachble= {0}; targetReachble={1})", currentReachble, targetReachble);
            }
            return saida;
        }

        private void addVerticalRange(int x, Range<int> values)
        {
            if (!vRanges.ContainsKey(x))
            {
                vRanges[x] = new List<Range<int>>();
            }
            vRanges[x].Add(values);
        }
        private void addHorizontalRange(int y, Range<int> values)
        {
            if (!hRanges.ContainsKey(y))
            {
                hRanges[y] = new List<Range<int>>();
            }
            hRanges[y].Add(values);
        }
        private void addD1Range(Vector2 refPoint, Range<int> values)
        {
            int index = (int)refPoint.x + (int)refPoint.y;
            if (!d1Ranges.ContainsKey(index))
            {
                d1Ranges[index] = new List<Range<int>>();
            }
            d1Ranges[index].Add(values);
        }
        private void addD2Range(Vector2 refPoint, Range<int> values)
        {
            int index = (int)refPoint.x - (int)refPoint.y;
            if (!d2Ranges.ContainsKey(index))
            {
                d2Ranges[index] = new List<Range<int>>();
            }
            d2Ranges[index].Add(values);
        }

        private void addSegmentToIndex(Vector2 p1, Vector2 p2)
        {
            if (p1 == p2) return;
            if (p1.x + p1.y == p2.x + p2.y)
            {
                addD1Range(p1, new Range<int>((int)p2.x, (int)p1.x));
            }

            if (p1.x - p1.y == p2.x - p2.y)
            {
                addD2Range(p1, new Range<int>((int)p2.x, (int)p1.x));
            }

            if (p2.x == p1.x)
            {
                addVerticalRange((int)p2.x, new Range<int>((int)p2.y, (int)p1.y));
            }
            if (p2.y == p1.y)
            {
                addHorizontalRange((int)p2.y, new Range<int>((int)p2.x, (int)p1.x));
            }
        }

        public class LineSegmentStations
        {
            public Station s1
            {
                get; internal set;
            }
            public Station s2
            {
                get; internal set;
            }
            public List<Vector2> path
            {
                get; internal set;
            }
            public int id
            {
                get {
                    return getIdForSegments(s1, s2, out bool x);
                }
            }


            public Dictionary<TLMMapTransportLine, Direction> lines = new Dictionary<TLMMapTransportLine, Direction>();

            public static int getIdForSegments(Station a, Station b, out bool invert)
            {
                ushort aId = a.stopId;
                ushort bId = b.stopId;

                if (bId > aId)
                {
                    var temp = aId;
                    aId = bId;
                    bId = temp;
                    invert = true;
                }
                else
                {
                    invert = false;
                }

                return (aId << 16) | bId;
            }



            public LineSegmentStations(Station a, Station b, LineSegmentStationsManager manager)
            {
                getIdForSegments(a, b, out bool invert);
                if (invert)
                {
                    s1 = b;
                    s2 = a;
                }
                else
                {
                    s1 = a;
                    s2 = b;
                }
                path = manager.getPathForStations(s1, s2);
            }
            public void addLine(TLMMapTransportLine m, Direction d)
            {
                if (lines.ContainsKey(m))
                {
                    lines[m] |= d;
                }
                else
                {
                    lines[m] = d;
                }
            }
        }

        public enum Direction
        {
            NONE = 0,
            S1_TO_S2 = 1,
            S2_TO_S1 = 2,
            BOTH = 3
        }
    }


}
