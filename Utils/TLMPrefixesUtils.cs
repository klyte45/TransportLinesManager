using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.Utils.NumberArrays;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    public static class TLMPrefixesUtils
    {
        #region Prefix Operations

        public static bool HasPrefix(ref TransportSystemDefinition tsd)
        {
            if (tsd == default)
            {
                return false;
            }
            var transportType = tsd.ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((NamingMode)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != NamingMode.None;
        }

        public static bool HasPrefix(ref TransportLine t)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(ref t);
            if (tsd == default)
            {
                return false;
            }
            var transportType = tsd.ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((NamingMode)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != NamingMode.None;
        }

        public static bool HasPrefix(ushort idx)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(idx);
            if (tsd == default)
            {
                return false;
            }
            var transportType = tsd.ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((NamingMode)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != NamingMode.None;
        }

        public static bool HasPrefix(TransportInfo t)
        {
            var transportType = TransportSystemDefinition.From(t).ToConfigIndex();
            return transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((NamingMode)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != NamingMode.None;
        }


        public static uint GetPrefix(ushort idx)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(idx);
            if (tsd == default)
            {
                return 0;
            }
            var transportType = tsd.ToConfigIndex();
            if (transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG || ((NamingMode)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX)) != NamingMode.None)
            {
                uint prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[idx].m_lineNumber / 1000u;
                //LogUtils.DoLog($"Prefix {prefix} for lineId {idx}");
                return prefix;
            }
            else
            {
                //LogUtils.DoLog($"Prefix 0 (def) for lineId {idx}");
                return 0;
            }
        }
        internal static Color CalculateAutoColor(ushort num, TLMCW.ConfigIndex transportType, ref TransportSystemDefinition tsdRef, bool avoidRandom = false, bool allowClear = false)
        {
            if (transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG)
            {
                return TLMCW.getColorForTransportType(transportType);
            }

            bool prefixBased = TLMCW.GetCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_PREFIX_BASED);

            bool randomOnOverflow = TLMCW.GetCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);

            var pal = new List<string>();

            if (num >= 0 && TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX) != (int)NamingMode.None)
            {
                uint prefix = num / 1000u;
                ITLMTransportTypeExtension ext = tsdRef.GetTransportExtension();
                string tempPal = ext.GetCustomPalette(prefix) ?? string.Empty;
                if (tempPal != string.Empty)
                {
                    pal.Add(tempPal);
                    num %= 1000;
                }
                else
                {
                    if (prefixBased)
                    {
                        num /= 1000;
                    }
                    else
                    {
                        num %= 1000;
                    }
                }
                pal.Add(TLMCW.GetCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_MAIN));
            }
            else
            {
                pal.Add(TLMCW.GetCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_MAIN));
            }
            Color c;
            c = TLMAutoColorPalettes.getColor(num, pal.ToArray(), randomOnOverflow, avoidRandom);
            if (c == Color.clear && !allowClear)
            {
                c = TLMCW.getColorForTransportType(transportType);
            }
            return c;
        }
        internal static LineIconSpriteNames GetLineIcon(ushort num, TLMCW.ConfigIndex transportType, ref TransportSystemDefinition tsdRef)
        {

            if (num > 0 && TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX) != (int)NamingMode.None)
            {
                uint prefix = num / 1000u;
                ITLMTransportTypeExtension ext = tsdRef.GetTransportExtension();
                LineIconSpriteNames format = ext.GetCustomFormat(prefix);
                if (format != default)
                {
                    return format;
                }

            }
            return TLMCW.getBgIconForIndex(transportType);
        }
        internal static string[] GetStringOptionsForPrefix(TransportSystemDefinition tsd, bool showUnprefixed = false, bool useNameRefSystem = false, bool noneOption = true) => GetStringOptionsForPrefix(GetPrefixModoNomenclatura(tsd.ToConfigIndex()), ref tsd, showUnprefixed, useNameRefSystem, noneOption);

        private static string[] GetStringOptionsForPrefix(NamingMode m, ref TransportSystemDefinition tsd, bool useNameRefSystem = false, bool showUnprefixed = false, bool noneOption = true)
        {
            var saida = new List<string>(new string[noneOption ? 1 : 0]);
            if (!noneOption)
            {
                string unprefixedName = Locale.Get("K45_TLM_UNPREFIXED");
                if (useNameRefSystem)
                {
                    string prefixName = tsd.GetTransportExtension().GetName(0);
                    if (!string.IsNullOrEmpty(prefixName))
                    {
                        unprefixedName += " - " + prefixName;
                    }
                }
                saida.Add(unprefixedName);
            }
            if (m == NamingMode.None)
            {
                return saida.ToArray();
            }
            switch (m)
            {
                case NamingMode.GreekUpper:
                case NamingMode.GreekUpperNumber:
                    AddToArrayWithName(gregoMaiusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case NamingMode.GreekLower:
                case NamingMode.GreekLowerNumber:
                    AddToArrayWithName(gregoMinusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case NamingMode.CyrillicUpper:
                case NamingMode.CyrillicUpperUpper:
                    AddToArrayWithName(cirilicoMaiusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case NamingMode.CyrillicLower:
                case NamingMode.CyrillicLowerNumber:
                    AddToArrayWithName(cirilicoMinusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case NamingMode.LatinUpper:
                case NamingMode.LatinUpperNumber:
                    AddToArrayWithName(latinoMaiusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case NamingMode.LatinLower:
                case NamingMode.LatinLowerNumber:
                    AddToArrayWithName(latinoMinusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case NamingMode.Number:
                    string[] temp = new string[64];
                    for (int i = 1; i <= 64; i++)
                    {
                        temp[i - 1] = i.ToString();
                    }
                    AddToArrayWithName(temp, saida, ref tsd, useNameRefSystem);
                    break;
                case NamingMode.Roman:
                    string[] tempR = new string[64];
                    for (ushort i = 1; i <= 64; i++)
                    {
                        tempR[i - 1] = NumberingUtils.ToRomanNumeral(i);
                    }
                    AddToArrayWithName(tempR, saida, ref tsd, useNameRefSystem);
                    break;
            }
            if (TLMLineUtils.m_nomenclaturasComNumeros.Contains(m))
            {
                AddToArrayWithName(numeros, saida, ref tsd, useNameRefSystem);
            }
            if (!noneOption && !showUnprefixed)
            {
                saida.RemoveAt(0);
            }
            return saida.ToArray();
        }
        private static void AddToArrayWithName(string[] input, List<string> saida, ref TransportSystemDefinition tsd, bool usePrefixName = false)
        {
            ushort offset = (ushort)saida.Count;
            if (!usePrefixName)
            {
                saida.AddRange(input);
            }
            else
            {
                for (uint i = 0; i < input.Length; i++)
                {
                    string item = input[i];
                    string prefixName = tsd == default ? null : tsd.GetTransportExtension().GetName(offset + i);
                    if (string.IsNullOrEmpty(prefixName))
                    {
                        saida.Add(item);
                    }
                    else
                    {
                        saida.Add(item + " - " + prefixName);
                    }

                }
            }
        }
        internal static List<string> GetPrefixesOptions(TLMCW.ConfigIndex transportType, bool addDefaults = true)
        {
            transportType &= TLMConfigWarehouse.ConfigIndex.SYSTEM_PART;
            NamingMode m = GetPrefixModoNomenclatura(transportType);
            LogUtils.DoLog("getPrefixesOptions: MODO NOMENCLATURA = " + m);
            var saida = new List<string>();
            if (addDefaults)
            {
                saida.AddRange(new string[] { Locale.Get("K45_TLM_ALL"), Locale.Get("K45_TLM_UNPREFIXED") });
            }
            else
            {
                saida.Add("/");
            }
            switch (m)
            {
                case NamingMode.GreekUpper:
                case NamingMode.GreekUpperNumber:
                    saida.AddRange(gregoMaiusculo.Select(x => x.ToString()));
                    break;
                case NamingMode.GreekLower:
                case NamingMode.GreekLowerNumber:
                    saida.AddRange(gregoMinusculo.Select(x => x.ToString()));
                    break;
                case NamingMode.CyrillicUpper:
                case NamingMode.CyrillicUpperUpper:
                    saida.AddRange(cirilicoMaiusculo.Select(x => x.ToString()));
                    break;
                case NamingMode.CyrillicLower:
                case NamingMode.CyrillicLowerNumber:
                    saida.AddRange(cirilicoMinusculo.Select(x => x.ToString()));
                    break;
                case NamingMode.LatinUpper:
                case NamingMode.LatinUpperNumber:
                    saida.AddRange(latinoMaiusculo.Select(x => x.ToString()));
                    break;
                case NamingMode.LatinLower:
                case NamingMode.LatinLowerNumber:
                    saida.AddRange(latinoMinusculo.Select(x => x.ToString()));
                    break;
                case NamingMode.Number:
                    for (int i = 1; i <= 64; i++)
                    {
                        saida.Add(i.ToString());
                    }
                    break;
                case NamingMode.Roman:
                    for (ushort i = 1; i <= 64; i++)
                    {
                        saida.Add(NumberingUtils.ToRomanNumeral(i));
                    }
                    break;
            }
            if (TLMLineUtils.m_nomenclaturasComNumeros.Contains(m))
            {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida;
        }

        internal static NamingMode GetPrefixModoNomenclatura(TLMCW.ConfigIndex transportType) => (NamingMode)TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);



        internal static string GetStringFromNameMode(NamingMode mode, int num)
        {
            string result;
            if (mode == NamingMode.Roman)
            {
                result = NumberingUtils.ToRomanNumeral((ushort)num);
            }
            else if (mode == NamingMode.Number)
            {
                result = num.ToString("D");
            }
            else
            {
                TransportSystemDefinition tsd = default;
                result = NumberingUtils.GetStringFromNumber(GetStringOptionsForPrefix(mode, ref tsd), num + 1);
            }

            return result;
        }

        internal static string GetString(NamingMode prefixo, Separator s, NamingMode sufixo, NamingMode naoPrefixado, int numero, bool leadingZeros, bool invertPrefixSuffix)
        {
            string prefixoSaida = "";
            string separadorSaida = "";
            string sufixoSaida;
            int prefixNum = 0;
            if (prefixo != NamingMode.None)
            {
                prefixNum = numero / 1000;
                if (prefixNum > 0)
                {
                    prefixoSaida = GetStringFromNameMode(prefixo, prefixNum);
                }

                numero %= 1000;
            }

            if (numero > 0)
            {
                if (prefixoSaida != "")
                {
                    switch (s)
                    {
                        case Separator.Slash:
                            separadorSaida = "/";
                            break;
                        case Separator.Space:
                            separadorSaida = " ";
                            break;
                        case Separator.Hyphen:
                            separadorSaida = "-";
                            break;
                        case Separator.Dot:
                            separadorSaida = ".";
                            break;
                        case Separator.None:
                            if (prefixo == NamingMode.Roman)
                            {
                                separadorSaida = "·";
                            }
                            break;
                    }
                }

                var targetNameModeSuffix = prefixo != NamingMode.None && prefixNum > 0 ? sufixo : naoPrefixado;
                leadingZeros &= targetNameModeSuffix == NamingMode.Number;
                sufixoSaida = GetStringFromNameMode(targetNameModeSuffix, numero).PadLeft(leadingZeros ? 3 : 0, '0');

                if (invertPrefixSuffix && sufixo == NamingMode.Number && prefixo != NamingMode.Number && prefixo != NamingMode.Roman)
                {
                    return sufixoSaida + separadorSaida + prefixoSaida;
                }
                else
                {
                    return prefixoSaida + separadorSaida + sufixoSaida;
                }
            }
            else
            {
                return prefixoSaida;
            }
        }
        #endregion
    }

}

