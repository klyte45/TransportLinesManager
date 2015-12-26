using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMLineUtils
    {

        public static void GetLineNumberRules(out ModoNomenclatura mn, out ModoNomenclatura mnPrefixo, out Separador sep, out bool zeros, TransportInfo.TransportType tipoLinha)
        {
            switch (tipoLinha)
            {
                case TransportInfo.TransportType.Bus:
                    mn = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaOnibus.value;
                    mnPrefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaOnibusPrefixo.value;
                    sep = (Separador)TransportLinesManagerMod.savedNomenclaturaOnibusSeparador.value;
                    zeros = TransportLinesManagerMod.savedNomenclaturaOnibusZeros.value;
                    break;
                case TransportInfo.TransportType.Metro:
                    mn = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaMetro.value;
                    mnPrefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaMetroPrefixo.value;
                    sep = (Separador)TransportLinesManagerMod.savedNomenclaturaMetroSeparador.value;
                    zeros = TransportLinesManagerMod.savedNomenclaturaMetroZeros.value;
                    break;
                case TransportInfo.TransportType.Train:
                    mn = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaTrem.value;
                    mnPrefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaTremPrefixo.value;
                    sep = (Separador)TransportLinesManagerMod.savedNomenclaturaTremSeparador.value;
                    zeros = TransportLinesManagerMod.savedNomenclaturaTremZeros.value;
                    break;
                default:
                    mn = ModoNomenclatura.Numero;
                    mnPrefixo = ModoNomenclatura.Nenhum;
                    sep = Separador.Nenhum;
                    zeros = false;
                    return;
            }
        }


        /// <summary>
        /// </summary>
        /// <returns><c>true</c>, if recusive search for near stops was ended, <c>false</c> otherwise.</returns>
        /// <param name="pos">Position.</param>
        /// <param name="maxDistance">Max distance.</param>
        /// <param name="linesFound">Lines found.</param>
        public static bool GetNearLines(Vector3 pos, float maxDistance, ref List<ushort> linesFound)
        {
            int num = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);
            bool noneFound = true;
            NetManager nm = Singleton<NetManager>.instance;
            TransportManager tm = Singleton<TransportManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = nm.m_nodeGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        NetInfo info = nm.m_nodes.m_buffer[(int)num6].Info;

                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport) &&
                            ((info.m_class.m_subService == ItemClass.SubService.PublicTransportTrain && TransportLinesManagerMod.savedShowTrainLinesOnLinearMap.value)
                            || (info.m_class.m_subService == ItemClass.SubService.PublicTransportMetro && TransportLinesManagerMod.savedShowMetroLinesOnLinearMap.value)
                            || (info.m_class.m_subService == ItemClass.SubService.PublicTransportBus && TransportLinesManagerMod.savedShowBusLinesOnLinearMap.value)))
                        {
                            ushort transportLine = nm.m_nodes.m_buffer[(int)num6].m_transportLine;
                            if (transportLine != 0)
                            {
                                TransportInfo info2 = tm.m_lines.m_buffer[(int)transportLine].Info;
                                if (!linesFound.Contains(transportLine) && (tm.m_lines.m_buffer[(int)transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[(int)num6].m_position);
                                    if (num8 < maxDistance * maxDistance)
                                    {
                                        linesFound.Add(transportLine);
                                        GetNearLines(nm.m_nodes.m_buffer[(int)num6].m_position, maxDistance, ref linesFound);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        num6 = nm.m_nodes.m_buffer[(int)num6].m_nextGridNode;
                        if (++num7 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return noneFound;
        }
        //GetNearStopPoints
        public static bool GetNearStopPoints(Vector3 pos, float maxDistance, ref List<ushort> stopsFound, int depth = 0)
        {
            if (depth >= 4) return false;
            int num = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);
            bool noneFound = true;
            NetManager nm = Singleton<NetManager>.instance;
            TransportManager tm = Singleton<TransportManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort stopId = nm.m_nodeGrid[i * 270 + j];
                    int num7 = 0;
                    while (stopId != 0)
                    {
                        NetInfo info = nm.m_nodes.m_buffer[(int)stopId].Info;

                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport) &&
                            ((info.m_class.m_subService == ItemClass.SubService.PublicTransportTrain)
                            || (info.m_class.m_subService == ItemClass.SubService.PublicTransportMetro)))
                        {
                            ushort transportLine = nm.m_nodes.m_buffer[(int)stopId].m_transportLine;
                            if (transportLine != 0)
                            {
                                if (!stopsFound.Contains(stopId))
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[(int)stopId].m_position);
                                    if (num8 < maxDistance * maxDistance)
                                    {
                                        stopsFound.Add(stopId);
                                        GetNearStopPoints(nm.m_nodes.m_buffer[(int)stopId].m_position, maxDistance, ref stopsFound, depth + 1);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        stopId = nm.m_nodes.m_buffer[(int)stopId].m_nextGridNode;
                        if (++num7 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return noneFound;
        }

        public static Vector2 gridPositionGameDefault(Vector3 pos)
        {
            int x = Mathf.Max((int)((pos.x) / 64f + 135f), 0);
            int z = Mathf.Max((int)((-pos.z) / 64f + 135f), 0);
            return new Vector2(x, z);
        }


        public static Vector2 gridPosition81Tiles(Vector3 pos)
        {
            int x = Mathf.Max((int)((pos.x) / 64f + 243f), 0);
            int z = Mathf.Max((int)((-pos.z) / 64f + 243f), 0);
            return new Vector2(x, z);
        }

        /// <summary>
        /// Index the lines.
        /// </summary>
        /// <returns>The lines indexed.</returns>
        /// <param name="intersections">Intersections.</param>
        /// <param name="t">Transport line to ignore.</param>
        public static Dictionary<string, ushort> IndexLines(List<ushort> intersections, TransportLine t = default(TransportLine))
        {
            TransportManager tm = Singleton<TransportManager>.instance;
            Dictionary<String, ushort> otherLinesIntersections = new Dictionary<String, ushort>();
            foreach (ushort s in intersections)
            {
                TransportLine tl = tm.m_lines.m_buffer[(int)s];
                if (t.Equals(default(TransportLine)) || tl.Info.GetSubService() != t.Info.GetSubService() || tl.m_lineNumber != t.m_lineNumber)
                {
                    string transportTypeLetter = "";
                    switch (tl.Info.m_transportType)
                    {
                        case TransportInfo.TransportType.Bus:
                            transportTypeLetter = "E";
                            break;
                        case TransportInfo.TransportType.Metro:
                            transportTypeLetter = "B";
                            break;
                        case TransportInfo.TransportType.Train:
                            transportTypeLetter = "C";
                            break;
                    }
                    otherLinesIntersections.Add(transportTypeLetter + tl.m_lineNumber.ToString().PadLeft(5, '0'), s);
                }
            }
            return otherLinesIntersections;
        }

        public static void PrintIntersections(string airport, string port, string taxi, UIPanel intersectionsPanel, Dictionary<string, ushort> otherLinesIntersections, float scale = 1.0f, int maxItemsForSizeSwap = 3)
        {
            TransportManager tm = Singleton<TransportManager>.instance;

            int intersectionCount = otherLinesIntersections.Count;
            if (!String.IsNullOrEmpty(airport))
            {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(port))
            {
                intersectionCount++;
            }
            if (!String.IsNullOrEmpty(taxi))
            {
                intersectionCount++;
            }
            float size = scale * (intersectionCount > maxItemsForSizeSwap ? 20 : 40);
            float multiplier = scale * (intersectionCount > maxItemsForSizeSwap ? 0.4f : 0.8f);
            foreach (var s in otherLinesIntersections.OrderBy(x => x.Key))
            {
                TransportLine intersectLine = tm.m_lines.m_buffer[(int)s.Value];
                String bgSprite;
                ModoNomenclatura nomenclatura, prefixo;
                Separador separador;
                bool zeros;
                ItemClass.SubService ss = setFormatBgByType(intersectLine, out bgSprite, out prefixo, out separador, out nomenclatura, out zeros);
                UIButtonLineInfo lineCircleIntersect = null;
                TLMUtils.createUIElement<UIButtonLineInfo>(ref lineCircleIntersect, intersectionsPanel.transform);
                lineCircleIntersect.autoSize = false;
                lineCircleIntersect.width = size;
                lineCircleIntersect.height = size;
                lineCircleIntersect.color = intersectLine.m_color;
                lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
                lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
                lineCircleIntersect.name = "LineFormat";
                lineCircleIntersect.relativePosition = new Vector3(0f, 0f);
                lineCircleIntersect.atlas = TLMController.taLineNumber;
                lineCircleIntersect.normalBgSprite = bgSprite;
                lineCircleIntersect.hoveredColor = Color.white;
                lineCircleIntersect.hoveredTextColor = Color.red;
                lineCircleIntersect.lineID = s.Value;
                lineCircleIntersect.tooltip = tm.GetLineName(s.Value);
                lineCircleIntersect.eventClick += TLMController.instance.lineInfoPanel.openLineInfo;
                UILabel lineNumberIntersect = null;
                TLMUtils.createUIElement<UILabel>(ref lineNumberIntersect, lineCircleIntersect.transform);
                lineNumberIntersect.autoSize = false;
                lineNumberIntersect.autoHeight = false;
                lineNumberIntersect.width = lineCircleIntersect.width;
                lineNumberIntersect.pivot = UIPivotPoint.MiddleCenter;
                lineNumberIntersect.textAlignment = UIHorizontalAlignment.Center;
                lineNumberIntersect.verticalAlignment = UIVerticalAlignment.Middle;
                lineNumberIntersect.name = "LineNumber";
                lineNumberIntersect.height = size;
                lineNumberIntersect.relativePosition = new Vector3(-0.5f, 0.5f);
                lineNumberIntersect.textColor = Color.white;
                lineNumberIntersect.outlineColor = Color.black;
                lineNumberIntersect.useOutline = true;
                bool day, night;
                intersectLine.GetActive(out day, out night);
                if (!day || !night)
                {
                    UILabel daytimeIndicator = null;
                    TLMUtils.createUIElement<UILabel>(ref daytimeIndicator, lineCircleIntersect.transform);
                    daytimeIndicator.autoSize = false;
                    daytimeIndicator.width = size;
                    daytimeIndicator.height = size;
                    daytimeIndicator.color = Color.white;
                    daytimeIndicator.pivot = UIPivotPoint.MiddleLeft;
                    daytimeIndicator.verticalAlignment = UIVerticalAlignment.Middle;
                    daytimeIndicator.name = "LineTime";
                    daytimeIndicator.relativePosition = new Vector3(0f, 0f);
                    daytimeIndicator.atlas = TLMController.taLineNumber;
                    daytimeIndicator.backgroundSprite = day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
                }
                setLineNumberCircleOnRef(intersectLine.m_lineNumber, prefixo, separador, nomenclatura, zeros, lineNumberIntersect);
                lineNumberIntersect.textScale *= multiplier;
                lineNumberIntersect.relativePosition *= multiplier;
            }
            if (airport != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "AirplaneIcon", airport);
            }
            if (port != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "ShipIcon", port);
            }
            if (taxi != string.Empty)
            {
                addExtraStationBuildingIntersection(intersectionsPanel, size, "TaxiIcon", taxi);
            }
        }

        private static void addExtraStationBuildingIntersection(UIComponent parent, float size, string bgSprite, string description)
        {
            UILabel lineCircleIntersect = null;
            TLMUtils.createUIElement<UILabel>(ref lineCircleIntersect, parent.transform);
            lineCircleIntersect.autoSize = false;
            lineCircleIntersect.width = size;
            lineCircleIntersect.height = size;
            lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
            lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
            lineCircleIntersect.name = "LineFormat";
            lineCircleIntersect.relativePosition = new Vector3(0f, 0f);
            lineCircleIntersect.atlas = TLMController.taLineNumber;
            lineCircleIntersect.backgroundSprite = bgSprite;
            lineCircleIntersect.tooltip = description;
        }

        public static void setLineNumberCircleOnRef(int num, ModoNomenclatura pre, Separador s, ModoNomenclatura mn, bool zeros, UILabel reference, float ratio = 1f)
        {
            reference.text = TLMUtils.getString(pre, s, mn, num, zeros);
            int lenght = reference.text.Length;
            if (lenght >= 4)
            {
                reference.textScale = 1f * ratio;
                reference.relativePosition = new Vector3(0f, 1f);
            }
            else if (lenght == 3)
            {
                reference.textScale = 1.25f * ratio;
                reference.relativePosition = new Vector3(0f, 1.5f);
            }
            else if (lenght == 2)
            {
                reference.textScale = 1.75f * ratio;
                reference.relativePosition = new Vector3(-0.5f, 0.5f);
            }
            else {
                reference.textScale = 2.3f * ratio;
                reference.relativePosition = new Vector3(-0.5f, 0f);
            }
        }

        public static ItemClass.SubService setFormatBgByType(TransportLine line, out String bgSprite, out ModoNomenclatura prefixo, out Separador s, out ModoNomenclatura nomenclatura, out bool zerosEsquerda)
        {
            if (line.Info.m_transportType == TransportInfo.TransportType.Train)
            {
                bgSprite = "TrainIcon";
                nomenclatura = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaTrem.value;
                prefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaTremPrefixo.value;
                s = (Separador)TransportLinesManagerMod.savedNomenclaturaTremSeparador.value;
                zerosEsquerda = TransportLinesManagerMod.savedNomenclaturaTremZeros.value;
                return ItemClass.SubService.PublicTransportTrain;
            }
            else if (line.Info.m_transportType == TransportInfo.TransportType.Metro)
            {
                bgSprite = "SubwayIcon";
                nomenclatura = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaMetro.value;
                prefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaMetroPrefixo.value;
                s = (Separador)TransportLinesManagerMod.savedNomenclaturaMetroSeparador.value;
                zerosEsquerda = TransportLinesManagerMod.savedNomenclaturaMetroZeros.value;
                return ItemClass.SubService.PublicTransportMetro;
            }
            else {
                bgSprite = "BusIcon";
                nomenclatura = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaOnibus.value;
                prefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaOnibusPrefixo.value;
                s = (Separador)TransportLinesManagerMod.savedNomenclaturaOnibusSeparador.value;
                zerosEsquerda = TransportLinesManagerMod.savedNomenclaturaOnibusZeros.value;
                return ItemClass.SubService.None;
            }
        }
    }

    public class TLMUtils
    {
        public static void doLog(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
        }
        public static void createUIElement<T>(ref T uiItem, Transform parent) where T : Component
        {
            GameObject container = new GameObject();
            container.transform.parent = parent;
            uiItem = container.AddComponent<T>();
        }

        public static void uiTextFieldDefaults(UITextField uiItem)
        {
            uiItem.selectionSprite = "EmptySprite";
            uiItem.useOutline = true;
            uiItem.hoveredBgSprite = "TextFieldPanelHovered";
            uiItem.focusedBgSprite = "TextFieldPanel";
            uiItem.builtinKeyNavigation = true;
            uiItem.submitOnFocusLost = true;
        }

        public static Color contrastColor(Color color)
        {
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double a = (0.299 * color.r + 0.587 * color.g + 0.114 * color.b);

            if (a > 0.5)
                d = 0; // bright colors - black font
            else
                d = 1; // dark colors - white font

            return new Color(d, d, d, 1);
        }

        public static float calcBezierLenght(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float precision)
        {

            Vector3 aa = (-a + 3 * (b - c) + d);
            Vector3 bb = 3 * (a + c) - 6 * b;
            Vector3 cc = 3 * (b - a);

            int len = (int)(1.0f / precision);
            float[] arcLengths = new float[len + 1];
            arcLengths[0] = 0;

            Vector3 ov = a;
            Vector3 v;
            float clen = 0.0f;
            for (int i = 1; i <= len; i++)
            {
                float t = (i * precision);
                v = ((aa * t + (bb)) * t + cc) * t + a;
                clen += (ov - v).magnitude;
                arcLengths[i] = clen;
                ov = v;
            }
            return clen;

        }

        public static void createDragHandle(UIComponent parent, UIComponent target)
        {
            createDragHandle(parent, target, -1);
        }

        public static void createDragHandle(UIComponent parent, UIComponent target, float height)
        {
            UIDragHandle dh = null;
            createUIElement<UIDragHandle>(ref dh, parent.transform);
            dh.target = target;
            dh.relativePosition = new Vector3(0, 0);
            dh.width = parent.width;
            dh.height = height < 0 ? parent.height : height;
            dh.name = "DragHandle";
            dh.Start();
        }

        public static void initButton(UIButton button, bool isCheck, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite + "Disabled";
            button.hoveredBgSprite = spriteHov;
            button.focusedBgSprite = spriteHov;
            button.pressedBgSprite = isCheck ? sprite + "Pressed" : spriteHov;
            button.textColor = new Color32(255, 255, 255, 255);
        }

        public static void initButtonSameSprite(UIButton button, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite;
            button.hoveredBgSprite = sprite;
            button.focusedBgSprite = sprite;
            button.pressedBgSprite = sprite;
            button.textColor = new Color32(255, 255, 255, 255);
        }

        public static void initButtonFg(UIButton button, bool isCheck, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalFgSprite = sprite;
            button.disabledFgSprite = sprite;
            button.hoveredFgSprite = spriteHov;
            button.focusedFgSprite = spriteHov;
            button.pressedFgSprite = isCheck ? sprite + "Pressed" : spriteHov;
            button.textColor = new Color32(255, 255, 255, 255);
        }

        public static void copySpritesEvents(UIButton source, UIButton target)
        {
            target.disabledBgSprite = source.disabledBgSprite;
            target.focusedBgSprite = source.focusedBgSprite;
            target.hoveredBgSprite = source.hoveredBgSprite;
            target.normalBgSprite = source.normalBgSprite;
            target.pressedBgSprite = source.pressedBgSprite;

            target.disabledFgSprite = source.disabledFgSprite;
            target.focusedFgSprite = source.focusedFgSprite;
            target.hoveredFgSprite = source.hoveredFgSprite;
            target.normalFgSprite = source.normalFgSprite;
            target.pressedFgSprite = source.pressedFgSprite;

        }

        public static string[] getStringOptionsForPrefix(ModoNomenclatura m)
        {
            List<string> saida = new List<string>(new string[] { "" });
            switch (m)
            {
                case ModoNomenclatura.GregoMaiusculo:
                    saida.AddRange(gregoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.GregoMinusculo:
                    saida.AddRange(gregoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                    saida.AddRange(cirilicoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                    saida.AddRange(cirilicoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                    saida.AddRange(latinoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                    saida.AddRange(latinoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.Numero:
                    for (int i = 1; i <= 64; i++)
                    {
                        saida.Add(i.ToString());
                    }
                    break;
            }
            return saida.ToArray();
        }

        public static string getString(ModoNomenclatura pre, Separador s, ModoNomenclatura m, int numero, bool zerosEsquerda)
        {
            string prefixo = "";
            if (pre != ModoNomenclatura.Nenhum)
            {
                switch (pre)
                {
                    case ModoNomenclatura.GregoMaiusculo:
                        prefixo = getStringFromNumber(gregoMaiusculo, numero / 1000);
                        break;
                    case ModoNomenclatura.GregoMinusculo:
                        prefixo = getStringFromNumber(gregoMinusculo, numero / 1000);
                        break;
                    case ModoNomenclatura.CirilicoMaiusculo:
                        prefixo = getStringFromNumber(cirilicoMaiusculo, numero / 1000);
                        break;
                    case ModoNomenclatura.CirilicoMinusculo:
                        prefixo = getStringFromNumber(cirilicoMinusculo, numero / 1000);
                        break;
                    case ModoNomenclatura.LatinoMaiusculo:
                        prefixo = getStringFromNumber(latinoMaiusculo, numero / 1000);
                        break;
                    case ModoNomenclatura.LatinoMinusculo:
                        prefixo = getStringFromNumber(latinoMinusculo, numero / 1000);
                        break;
                    default:
                        if (numero >= 1000)
                        {
                            prefixo = "" + (numero / 1000);
                        }
                        break;
                }
                numero = numero % 1000;
            }

            if (numero > 0)
            {
                if (prefixo != "" && s != Separador.Nenhum)
                {
                    switch (s)
                    {
                        case Separador.Barra:
                            prefixo += "/";
                            break;
                        case Separador.Espaco:
                            prefixo += " ";
                            break;
                        case Separador.Hifen:
                            prefixo += "-";
                            break;
                        case Separador.Ponto:
                            prefixo += ".";
                            break;
                        case Separador.QuebraLinha:
                            prefixo += "\n";
                            break;
                    }
                }
                switch (m)
                {
                    case ModoNomenclatura.GregoMaiusculo:
                        return prefixo + getStringFromNumber(gregoMaiusculo, numero);
                    case ModoNomenclatura.GregoMinusculo:
                        return prefixo + getStringFromNumber(gregoMinusculo, numero);
                    case ModoNomenclatura.CirilicoMaiusculo:
                        return prefixo + getStringFromNumber(cirilicoMaiusculo, numero);
                    case ModoNomenclatura.CirilicoMinusculo:
                        return prefixo + getStringFromNumber(cirilicoMinusculo, numero);
                    case ModoNomenclatura.LatinoMaiusculo:
                        return prefixo + getStringFromNumber(latinoMaiusculo, numero);
                    case ModoNomenclatura.LatinoMinusculo:
                        return prefixo + getStringFromNumber(latinoMinusculo, numero);
                    default:
                        if (zerosEsquerda && prefixo != "")
                        {
                            return prefixo + numero.ToString("D3");
                        }
                        else {
                            return prefixo + numero;
                        }
                }
            }
            else {
                return prefixo;
            }
        }

        private static string getString(ModoNomenclatura m, int numero)
        {

            switch (m)
            {
                case ModoNomenclatura.GregoMaiusculo:
                    return getStringFromNumber(gregoMaiusculo, numero);
                case ModoNomenclatura.GregoMinusculo:
                    return getStringFromNumber(gregoMinusculo, numero);
                case ModoNomenclatura.CirilicoMaiusculo:
                    return getStringFromNumber(cirilicoMaiusculo, numero);
                case ModoNomenclatura.CirilicoMinusculo:
                    return getStringFromNumber(cirilicoMinusculo, numero);
                case ModoNomenclatura.LatinoMaiusculo:
                    return getStringFromNumber(latinoMaiusculo, numero);
                case ModoNomenclatura.LatinoMinusculo:
                    return getStringFromNumber(latinoMinusculo, numero);
                default:
                    return "" + numero;
            }
        }

        public static string getStringFromNumber(char[] array, int number)
        {
            int arraySize = array.Length;
            string saida = "";
            while (number > 0)
            {
                int idx = (number - 1) % arraySize;
                saida = "" + array[idx] + saida;
                if (number % arraySize == 0)
                {
                    number /= arraySize;
                    number--;
                }
                else {
                    number /= arraySize;
                }

            }
            return saida;
        }

        public static void setLineColor(ushort lineIdx, Color color)
        {

            Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_color = color;
            Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_flags |= TransportLine.Flags.CustomColor;
        }

        public static void setLineName(ushort lineIdx, string name)
        {
            InstanceID lineIdSelecionado = default(InstanceID);
            lineIdSelecionado.TransportLine = lineIdx;
            if (name.Length > 0)
            {
                Singleton<InstanceManager>.instance.SetName(lineIdSelecionado, name);
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_flags |= TransportLine.Flags.CustomName;
            }
            else {
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineIdx].m_flags &= ~TransportLine.Flags.CustomName;
            }
        }

        public static string calculateAutoName(ushort lineIdx)
        {
            TransportManager tm = Singleton<TransportManager>.instance;
            TransportLine t = tm.m_lines.m_buffer[(int)lineIdx];
            ItemClass.SubService ss = ItemClass.SubService.None;
            if (t.Info.m_transportType == TransportInfo.TransportType.Train)
            {
                ss = ItemClass.SubService.PublicTransportTrain;
            }
            else if (t.Info.m_transportType == TransportInfo.TransportType.Metro)
            {
                ss = ItemClass.SubService.PublicTransportMetro;
            }
            int stopsCount = t.CountStops(lineIdx);
            string m_autoName = "";
            ushort[] stopBuildings = new ushort[stopsCount];
            MultiMap<ushort, Vector3> bufferToDraw = new MultiMap<ushort, Vector3>();
            int perfectSimetricLineStationsCount = (stopsCount + 2) / 2;
            bool simetric = t.Info.m_transportType != TransportInfo.TransportType.Bus;
            int middle = -1;
            if (simetric)
            {
                simetric = CalculateSimmetry(ss, stopsCount, t, out middle);
            }
            if (simetric)
            {
                return getStationName(t.GetStop(middle), ss) + " - " + getStationName(t.GetStop(middle + stopsCount / 2), ss);
            }
            else {
                DistrictManager dm = Singleton<DistrictManager>.instance;
                byte lastDistrict = 0;
                Vector3 local;
                byte district;
                List<int> districtList = new List<int>();
                NetManager nm = Singleton<NetManager>.instance;
                for (int j = 0; j < stopsCount; j++)
                {
                    local = nm.m_nodes.m_buffer[(int)t.GetStop(j)].m_bounds.center;
                    district = dm.GetDistrict(local);
                    if ((district != lastDistrict) && district != 0)
                    {
                        districtList.Add(district);
                    }
                    if (district != 0)
                    {
                        lastDistrict = district;
                    }
                }

                local = nm.m_nodes.m_buffer[(int)t.GetStop(0)].m_bounds.center;
                district = dm.GetDistrict(local);
                if ((district != lastDistrict) && district != 0)
                {
                    districtList.Add(district);
                }
                middle = -1;
                int[] districtArray = districtList.ToArray();
                if (districtArray.Length == 1)
                {
                    return (TransportLinesManagerMod.savedCircularOnSingleDistrict.value ? "Circular " : "") + dm.GetDistrictName(districtArray[0]);
                }
                else if (findSimetry(districtArray, out middle))
                {
                    int firstIdx = middle;
                    int lastIdx = middle + districtArray.Length / 2;

                    m_autoName = dm.GetDistrictName(districtArray[firstIdx % districtArray.Length]) + " - " + dm.GetDistrictName(districtArray[lastIdx % districtArray.Length]);
                    if (lastIdx - firstIdx > 1)
                    {
                        m_autoName += ", via ";
                        for (int k = firstIdx + 1; k < lastIdx; k++)
                        {
                            m_autoName += dm.GetDistrictName(districtArray[k % districtArray.Length]);
                            if (k + 1 != lastIdx)
                            {
                                m_autoName += ", ";
                            }
                        }
                    }
                    return m_autoName;
                }
                else {
                    bool inicio = true;
                    foreach (int i in districtArray)
                    {
                        m_autoName += (inicio ? "" : " - ") + dm.GetDistrictName(i);
                        inicio = false;
                    }
                    return m_autoName;
                }
            }
        }

        public static bool CalculateSimmetry(ItemClass.SubService ss, int stopsCount, TransportLine t, out int middle)
        {
            int j;
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            middle = -1;
            //try to find the loop
            for (j = -1; j < stopsCount / 2; j++)
            {
                int offsetL = (j + stopsCount) % stopsCount;
                int offsetH = (j + 2) % stopsCount;
                NetNode nn1 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetL)];
                NetNode nn2 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetH)];
                ushort buildingId1 = bm.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.Untouchable);
                ushort buildingId2 = bm.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.Untouchable);
                //					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                //					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
                if (buildingId1 == buildingId2)
                {
                    middle = j + 1;
                    break;
                }
            }
            //				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"middle="+middle);
            if (middle >= 0)
            {
                for (j = 1; j <= stopsCount / 2; j++)
                {
                    int offsetL = (-j + middle + stopsCount) % stopsCount;
                    int offsetH = (j + middle) % stopsCount;
                    //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
                    //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"t.GetStop (offsetL)="+t.GetStop (offsetH)+"|t.GetStop (offsetH)="+t.GetStop (offsetH));
                    NetNode nn1 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetL)];
                    NetNode nn2 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetH)];
                    ushort buildingId1 = bm.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.Untouchable);
                    ushort buildingId2 = bm.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.Untouchable);
                    //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                    //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                    //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
                    if (buildingId1 != buildingId2)
                    {
                        return false;
                    }
                }
            }
            else {
                return false;
            }
            return true;
        }

        public static string getStationName(uint stopId, ItemClass.SubService ss)
        {
            ushort buildingId = getStationBuilding(stopId, ss);

            if (buildingId > 0)
            {
                BuildingManager bm = Singleton<BuildingManager>.instance;
                Building b = bm.m_buildings.m_buffer[buildingId];
                InstanceID iid = default(InstanceID);
                iid.Building = buildingId;
                return bm.GetBuildingName(buildingId, iid);
            }
            else
            {
                NetManager nm = Singleton<NetManager>.instance;
                NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
                Vector3 location = nn.m_position;
                DistrictManager dm = Singleton<DistrictManager>.instance;
                int dId = dm.GetDistrict(location);
                if (dId > 0)
                {
                    District d = dm.m_districts.m_buffer[dId];
                    return "[D] " + dm.GetDistrictName(dId);
                }
                else {
                    return "[X=" + location.x + "|Y=" + location.y + "|Z=" + location.z + "]";
                }
            }
        }


        public static Vector3 getStationBuildingPosition(uint stopId, ItemClass.SubService ss)
        {
            ushort buildingId = getStationBuilding(stopId, ss);


            if (buildingId > 0)
            {
                BuildingManager bm = Singleton<BuildingManager>.instance;
                Building b = bm.m_buildings.m_buffer[buildingId];
                InstanceID iid = default(InstanceID);
                iid.Building = buildingId;
                return b.m_position;
            }
            else
            {
                NetManager nm = Singleton<NetManager>.instance;
                NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
                return nn.m_position;
            }
        }

        public static ushort getStationBuilding(uint stopId, ItemClass.SubService ss)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            ushort buildingId;
            if (ss != ItemClass.SubService.None)
            {
                buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.Untouchable);
            }
            else {
                buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Monument, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Beautification, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Government, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.HealthCare, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.FireDepartment, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.PoliceDepartment, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Tourism, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Education, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Garbage, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Office, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Commercial, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Industrial, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Water, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Electricity, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
                }
                if (buildingId == 0)
                {
                    buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.Residential, ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                }
            }
            return buildingId;

        }
        /// <summary>
        /// -180° a 180°
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float GetAngleOfLineBetweenTwoPoints(Vector2 p1, Vector2 p2)
        {

            return (float)(Vector2.Angle(p1, p2) * (180f / Math.PI));
        }

        public static bool findSimetry(int[] array, out int middle)
        {
            middle = -1;
            int size = array.Length;
            if (size == 0)
                return false;
            for (int j = -1; j < size / 2; j++)
            {
                int offsetL = (j + size) % size;
                int offsetH = (j + 2) % size;
                if (array[offsetL] == array[offsetH])
                {
                    middle = j + 1;
                    break;
                }
            }
            //			DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"middle="+middle);
            if (middle >= 0)
            {
                for (int k = 1; k <= size / 2; k++)
                {
                    int offsetL = (-k + middle + size) % size;
                    int offsetH = (k + middle) % size;
                    if (array[offsetL] != array[offsetH])
                    {
                        return false;
                    }
                }
            }
            else {
                return false;
            }
            return true;
        }

        public class UIButtonLineInfo : UIButton
        {
            public ushort lineID;
        }

        private static char[] latinoMaiusculo = {
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z'
        };
        private static char[] latinoMinusculo = {
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'g',
            'h',
            'i',
            'j',
            'k',
            'l',
            'm',
            'n',
            'o',
            'p',
            'q',
            'r',
            's',
            't',
            'u',
            'v',
            'w',
            'x',
            'y',
            'z'
        };
        private static char[] gregoMaiusculo = {
            'Α',
            'Β',
            'Γ',
            'Δ',
            'Ε',
            'Ζ',
            'Η',
            'Θ',
            'Ι',
            'Κ',
            'Λ',
            'Μ',
            'Ν',
            'Ξ',
            'Ο',
            'Π',
            'Ρ',
            'Σ',
            'Τ',
            'Υ',
            'Φ',
            'Χ',
            'Ψ',
            'Ω'
        };
        private static char[] gregoMinusculo = {
            'α',
            'β',
            'γ',
            'δ',
            'ε',
            'ζ',
            'η',
            'θ',
            'ι',
            'κ',
            'λ',
            'μ',
            'ν',
            'ξ',
            'ο',
            'π',
            'ρ',
            'σ',
            'τ',
            'υ',
            'φ',
            'χ',
            'ψ',
            'ω'
        };
        private static char[] cirilicoMaiusculo = {
            'А',
            'Б',
            'В',
            'Г',
            'Д',
            'Е',
            'Ё',
            'Ж',
            'З',
            'И',
            'Й',
            'К',
            'Л',
            'М',
            'Н',
            'О',
            'П',
            'Р',
            'С',
            'Т',
            'У',
            'Ф',
            'Х',
            'Ц',
            'Ч',
            'Ш',
            'Щ',
            'Ъ',
            'Ы',
            'Ь',
            'Э',
            'Ю',
            'Я'
        };
        private static char[] cirilicoMinusculo = {
            'а',
            'б',
            'в',
            'г',
            'д',
            'е',
            'ё',
            'ж',
            'з',
            'и',
            'й',
            'к',
            'л',
            'м',
            'н',
            'о',
            'п',
            'р',
            'с',
            'т',
            'у',
            'ф',
            'х',
            'ц',
            'ч',
            'ш',
            'щ',
            'ъ',
            'ы',
            'ь',
            'э',
            'ю',
            'я'
        };

    }

    public class ResourceLoader
    {

        public static Assembly ResourceAssembly
        {
            get
            {
                //return null;
                return Assembly.GetAssembly(typeof(ResourceLoader));
            }
        }

        public static byte[] loadResourceData(string name)
        {
            name = "TransportLinesManager." + name;

            UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "Could not find resource: " + name);
                return null;
            }

            BinaryReader read = new BinaryReader(stream);
            return read.ReadBytes((int)stream.Length);
        }

        public static string loadResourceString(string name)
        {
            name = "TransportLinesManager." + name;

            UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "Could not find resource: " + name);
                return null;
            }

            StreamReader read = new StreamReader(stream);
            return read.ReadToEnd();
        }

        public static Texture2D loadTexture(int x, int y, string filename)
        {
            try
            {
                Texture2D texture = new Texture2D(x, y);
                texture.LoadImage(loadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "The file could not be read:" + e.Message);
            }

            return null;
        }
    }

    public enum ModoNomenclatura
    {
        Numero = 0,
        LatinoMinusculo = 1,
        LatinoMaiusculo = 2,
        GregoMinusculo = 3,
        GregoMaiusculo = 4,
        CirilicoMinusculo = 5,
        CirilicoMaiusculo = 6,
        Nenhum = 7
    }

    public enum Separador
    {
        Nenhum = 0,
        Hifen = 1,
        Ponto = 2,
        Barra = 3,
        Espaco = 4,
        QuebraLinha = 5
    }

    public class Range<T> where T : IComparable<T>
    {
        /// <summary>
        /// Minimum value of the range
        /// </summary>
        public T Minimum { get; set; }

        /// <summary>
        /// Maximum value of the range
        /// </summary>
        public T Maximum { get; set; }

        public Range(T min, T max)
        {
            if (min.CompareTo(max) >= 0)
            {
                var temp = min;
                min = max;
                max = temp;
            }
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Presents the Range in readable format
        /// </summary>
        /// <returns>String representation of the Range</returns>
        public override string ToString() { return String.Format("[{0} - {1}]", Minimum, Maximum); }

        /// <summary>
        /// Determines if the range is valid
        /// </summary>
        /// <returns>True if range is valid, else false</returns>
        public Boolean IsValid() { return Minimum.CompareTo(Maximum) <= 0; }

        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean ContainsValue(T value)
        {
            return (Minimum.CompareTo(value) <= 0) && (value.CompareTo(Maximum) <= 0);
        }


        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean IsBetweenLimits(T value)
        {
            return (Minimum.CompareTo(value) < 0) && (value.CompareTo(Maximum) < 0);
        }

        /// <summary>
        /// Determines if this Range is inside the bounds of another range
        /// </summary>
        /// <param name="Range">The parent range to test on</param>
        /// <returns>True if range is inclusive, else false</returns>
        public Boolean IsInsideRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && Range.ContainsValue(this.Minimum) && Range.ContainsValue(this.Maximum);
        }

      

        /// <summary>
        /// Determines if another range is inside the bounds of this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean ContainsRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && this.ContainsValue(Range.Minimum) && this.ContainsValue(Range.Maximum);
        }

        /// <summary>
        /// Determines if another range intersect this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean IntersectRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && (this.ContainsValue(Range.Minimum) || this.ContainsValue(Range.Maximum) || Range.ContainsValue(this.Maximum) || Range.ContainsValue(this.Maximum));
        }

        public Boolean IsBorderSequence(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && (this.Maximum.Equals(Range.Minimum) || this.Minimum.Equals(Range.Maximum));
        }
    }
}

