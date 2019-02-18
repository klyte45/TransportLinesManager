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

        public void addLine(Station s1, Station s2, MapTransportLine line, Direction d)
        {
            LineSegmentStations lss = getStationsSegment(s1, s2, out bool invert);
            if (invert)
            {
                switch (d)
                {
                    case Direction.S1_TO_S2:
                        d = Direction.S2_TO_S1;
                        break;
                    case Direction.S2_TO_S1:
                        d = Direction.S1_TO_S2;
                        break;
                }
            }
            lss.addLine(line, d);
        }

        public List<LineSegmentStations> getSegments()
        {
            return segments.Values.ToList();
        }


        private LineSegmentStations getStationsSegment(Station s1, Station s2, out bool invert)
        {
            int id = LineSegmentStations.getIdForSegments(s1, s2, out invert);
            if (!segments.ContainsKey(id))
            {
                segments[id] = new LineSegmentStations(s1, s2, this);
            }

            return segments[id];

        }

        //d1 = x+y; d2 = x-y
        private Vector2 getIntersectionPoint(int d1Index, int d2Index)
        {
            return new Vector2((int)((d1Index + d2Index) / 2), (int)((d1Index - d2Index) / 2));
        }

        protected List<Vector2> getPathForStations(Station s1, Station s2)
        {
            TLMUtils.doLog("Calculando caminho entre '{0}' e '{1}' ", s1.name, s2.name);
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
            if (TransportLinesManagerMod.debugMode)
            {
                string points = string.Join(",", saida.Select(x => "(" + x.x + "," + x.y + ")").ToArray());
                TLMUtils.doLog("Points: [{0}]", points);
            }

            return saida;

        }

        private void GetPointsLine(ref Vector2 p1, ref Vector2 p2, ref CardinalPoint c1, ref CardinalPoint c2, ref List<Vector2> saida, int retrying = 0)
        {

            if (retrying > 7)
            {
                TLMUtils.doLog("MAX RETRYING REACHED!");
                return;
            }

            saida.Add(p1);

            CardinalPoint currentDirection = c1;


            //int iterationCount = 0;
            //DiagCheck:
            //iterationCount++;
            var dirS1 = c1.getCardinalOffset2D();
            var dirS2 = c2.getCardinalOffset2D();

            TLMUtils.doLog("c1 = {0};dirS1 = {1}", c1, dirS1);
            TLMUtils.doLog("c2 = {0};dirS2 = {1}", c2, dirS2);


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
                    TLMUtils.doLog("(D1 - D1)");
                    int indexSumS2 = (int)(targetPos.x + targetPos.y);
                    if (indexSumS1 != indexSumS2)
                    {
                        saida.AddRange(pathParallelDiag(dirS1, dirS2, currentPos, targetPos));
                    }
                }
                else if (isD2S2)
                {
                    TLMUtils.doLog("(D1 - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    saida.AddRange(getMidPointsD1D2(currentPos, targetPos, dirS1, dirS2, indexSumS1, indexSubS2));
                }
                else if (isHorizontalS2)
                {
                    TLMUtils.doLog("(D1 - H)");
                    int s2y = (int)targetPos.y;
                    int targetX = indexSumS1 - s2y;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, targetX, s2y, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE! - RETRYING");
                        c1++;
                        GetPointsLine(ref currentPos, ref p2, ref c1, ref c2, ref saida, retrying++);
                    }

                }
                else if (isVerticalS2)
                {
                    TLMUtils.doLog("(D1 - V)");
                    int s2x = (int)targetPos.x;
                    int targetY = indexSumS1 - s2x;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, s2x, targetY, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE - RETRYING");
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
                    TLMUtils.doLog("(D2 - D1)");
                    int indexSumS2 = (int)(currentPos.x + currentPos.y);
                    var points = getMidPointsD1D2(targetPos, currentPos, dirS2, dirS1, indexSumS2, indexSubS1);
                    points.Reverse();
                    saida.AddRange(points);
                }
                else if (isD2S2)
                {
                    TLMUtils.doLog("(D2 - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    if (indexSubS1 != indexSubS2)
                    {
                        saida.AddRange(pathParallelDiag(dirS1, dirS2, currentPos, targetPos));
                    }

                }
                else if (isHorizontalS2)
                {
                    TLMUtils.doLog("(D2 - H)");
                    int s2y = (int)targetPos.y;
                    int targetX = indexSubS1 + s2y;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, targetX, s2y, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE! - RETRYING");
                        c1--;
                        GetPointsLine(ref currentPos, ref p2, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isVerticalS2)
                {
                    TLMUtils.doLog("(D2 - V)");
                    int s2x = (int)targetPos.x;
                    int targetY = indexSubS1 + s2x;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, s2x, targetY, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE! - RETRYING");
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
                    TLMUtils.doLog("(H - D1)");
                    int indexSumS2 = (int)(targetPos.x + targetPos.y);
                    int targetX = indexSumS2 - s1y;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, targetX, s1y, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE! - RETRYING");
                        c2++;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isD2S2)
                {
                    TLMUtils.doLog("(H - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    int targetX = indexSubS2 + s1y;
                    if (!calcIntersecHV(targetPos, currentPos, dirS2, dirS1, targetX, s1y, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE!- RETRYING");
                        c2--;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isHorizontalS2)
                {
                    TLMUtils.doLog("(H - H)");
                    if (currentPos.x != targetPos.x)
                    {
                        saida.AddRange(pathParallel(dirS1, dirS2, currentPos, targetPos));
                    }
                }
                else if (isVerticalS2)
                {
                    TLMUtils.doLog("(H - V)");

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
                            TLMUtils.doLog("WORST CASE! - RETRYING");
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
                    TLMUtils.doLog("(V - D1)");
                    int indexSumS2 = (int)(targetPos.x + targetPos.y);
                    int targetY = indexSumS2 - s1x;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, s1x, targetY, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE! - RETRYING");
                        c2--;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isD2S2)
                {
                    TLMUtils.doLog("(V - D2)");
                    int indexSubS2 = (int)(targetPos.x - targetPos.y);
                    int targetY = s1x - indexSubS2;
                    if (!calcIntersecHV(currentPos, targetPos, dirS1, dirS2, s1x, targetY, ref saida))
                    {
                        TLMUtils.doLog("WORST CASE! - RETRYING");
                        c2++;
                        GetPointsLine(ref p1, ref targetPos, ref c1, ref c2, ref saida, retrying++);
                    }
                }
                else if (isHorizontalS2)
                {
                    TLMUtils.doLog("(V - H)");
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
                            TLMUtils.doLog("WORST CASE - RETRYING!");
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
                    TLMUtils.doLog("(V - V)");
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
                TLMUtils.doErrorLog("WORST CASE (midTargetReachableX = {0}, midTargetReachableY= {1})", midTargetReachableX, midTargetReachableY);
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
                TLMUtils.doErrorLog("WORST CASE (midTargetReachableX = {0}, midTargetReachableY= {1})", midTargetReachableX, midTargetReachableY);
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
                TLMUtils.doErrorLog("WORST CASE (midTargetReachableS1={0};midTargetReachableS2={1})", midTargetReachableS1, midTargetReachableS2);
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
                TLMUtils.doErrorLog("WORST CASE (currentReachble= {0}; targetReachble={1})", currentReachble, targetReachble);
            }
            return saida;
        }

        private Vector2 getFreeHorizontal(Vector2 p1, Vector2 p2)
        {
            TLMUtils.doLog("------------------------------------------------");
            if (p1.y != p2.y) return p2;
            int targetX = (int)p2.x;
            TLMUtils.doLog("getFreeHorizontal idx: {0} hRanges.ContainsKey(index)={1}; p1={2}; p2={3}", (int)p2.y, hRanges.ContainsKey((int)p2.y), p1, p2);
            if (hRanges.ContainsKey((int)p2.y))
            {
                Range<int> lineXs = new Range<int>((int)Math.Min(p1.x, p2.x), (int)Math.Max(p1.x, p2.x));
                var searchResult = hRanges[(int)p2.y].FindAll(x => x.IntersectRange(lineXs));
                TLMUtils.doLog(" getFreeHorizontal idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", (int)p2.y, lineXs, searchResult.Count, string.Join(",", hRanges[(int)p2.y].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.x - p1.x)) > 0)
                    {

                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.x);
                    }
                }
            }
            TLMUtils.doLog(" getFreeHorizontal RESULT=({0}, {1})", targetX, p2.y);
            TLMUtils.doLog("------------------------------------------------");
            return new Vector2(targetX, p2.y);
        }



        private Vector2 getFreeVertical(Vector2 p1, Vector2 p2)
        {
            if (p1.x != p2.x) return p2;
            int targetY = (int)p2.y;
            TLMUtils.doLog("------------------------------------------------");
            TLMUtils.doLog(" getFreeVertical idx: {0} vRanges.ContainsKey(index)={1}; p1={2}; p2={3}", (int)p2.x, vRanges.ContainsKey((int)p2.x), p1, p2);
            if (vRanges.ContainsKey((int)p2.x))
            {
                Range<int> lineYs = new Range<int>((int)Math.Min(p1.y, p2.y), (int)Math.Max(p1.y, p2.y));
                var searchResult = vRanges[(int)p2.x].FindAll(x => x.IntersectRange(lineYs));
                TLMUtils.doLog(" getFreeVertical idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", (int)p2.x, lineYs, searchResult.Count, string.Join(",", vRanges[(int)p2.x].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.y - p1.y)) > 0)
                    {
                        targetY = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.y);
                    }
                    else
                    {
                        targetY = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.y);
                    }
                }
            }

            TLMUtils.doLog(" getFreeVertical RESULT=({1}, {0})", targetY, p2.x);
            TLMUtils.doLog("------------------------------------------------");
            return new Vector2(p2.x, targetY);
        }

        private Vector2 getFreeD1Point(Vector2 p1, Vector2 p2)
        {
            if (p1.x + p1.y != p2.x + p2.y) return p2;
            int targetX = (int)p2.x;
            int index = (int)(p2.x + p2.y);
            TLMUtils.doLog(" getFreeHorizontalD1Point idx: {0} d1Ranges.ContainsKey(index)={1}", index, d1Ranges.ContainsKey(index));
            if (d1Ranges.ContainsKey(index))
            {
                Range<int> lineXs = new Range<int>((int)Math.Min(p1.x, p2.x), (int)Math.Max(p1.x, p2.x));
                var searchResult = d1Ranges[index].FindAll(x => x.IntersectRange(lineXs));
                TLMUtils.doLog(" getFreeHorizontalD2Point idx: {0}; X={1};LIST = {3} ; SRC = {2}", index, lineXs, searchResult.Count, string.Join(",", d1Ranges[index].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.x - p1.x)) > 0)
                    {
                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.x);
                    }
                }
            }

            return new Vector2(targetX, index - targetX);
        }



        private Vector2 getFreeD2Point(Vector2 p1, Vector2 p2)
        {
            if (p1.x - p1.y != p2.x - p2.y) return p2;
            int targetX = (int)p2.x;
            int index = (int)(p2.x - p2.y);
            TLMUtils.doLog(" getFreeHorizontalD2Point idx: {0} d2Ranges.ContainsKey(index)={1}", index, d2Ranges.ContainsKey(index));

            if (d2Ranges.ContainsKey(index))
            {
                Range<int> lineXs = new Range<int>((int)Math.Min(p1.x, p2.x), (int)Math.Max(p1.x, p2.x));
                var searchResult = d2Ranges[index].FindAll(x => x.IntersectRange(lineXs));
                TLMUtils.doLog(" getFreeHorizontalD2Point idx: {0}; X={1};LIST = [{3}] ; SRC = {2}", index, lineXs, searchResult.Count, string.Join(",", d2Ranges[index].Select(x => x.ToString()).ToArray()));
                if (searchResult.Count > 0)
                {
                    if (Math.Sign((p2.x - p1.x)) > 0)
                    {
                        targetX = Math.Max(searchResult.Select(x => x.Minimum - 1).Max(), (int)p1.x);
                    }
                    else
                    {
                        targetX = Math.Min(searchResult.Select(x => x.Maximum + 1).Min(), (int)p1.x);
                    }
                }
            }

            return new Vector2(targetX, targetX - index);
        }

        public void addStationToAllRangeMaps(Vector2 station)
        {
            addVerticalRange((int)station.x, new Range<int>((int)station.y, (int)station.y));
            addHorizontalRange((int)station.y, new Range<int>((int)station.x, (int)station.x));
            addD1Range(station, new Range<int>((int)station.x, (int)station.x));
            addD2Range(station, new Range<int>((int)station.x, (int)station.x));
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


            public Dictionary<MapTransportLine, Direction> lines = new Dictionary<MapTransportLine, Direction>();

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

                return (((int)aId) << 16) | bId;
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
            public void addLine(MapTransportLine m, Direction d)
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
