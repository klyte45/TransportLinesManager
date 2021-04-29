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


namespace Klyte.TransportLinesManager.Utils
{
    public static class TLMPrefixesUtils
    {
        #region Prefix Operations

        public static bool HasPrefix(TransportSystemDefinition tsd) => tsd != default && tsd != TransportSystemDefinition.EVAC_BUS && tsd.GetConfig().Prefix != NamingMode.None;

        public static bool HasPrefix(ref TransportLine t) => HasPrefix(TransportSystemDefinition.GetDefinitionForLine(ref t));

        public static bool HasPrefix(ushort idx) => HasPrefix(TransportSystemDefinition.GetDefinitionForLine(idx));

        public static bool HasPrefix(TransportInfo t) => HasPrefix(TransportSystemDefinition.From(t));

        public static uint GetPrefix(ushort idx) =>
            HasPrefix(TransportSystemDefinition.GetDefinitionForLine(idx))
                ? Singleton<TransportManager>.instance.m_lines.m_buffer[idx].m_lineNumber / 1000u
                : 0;
        internal static Color CalculateAutoColor(ushort num, TransportSystemDefinition tsdRef, bool avoidRandom = false, bool allowClear = false)
        {
            var config = tsdRef.GetConfig();

            if (tsdRef.TransportType == TransportInfo.TransportType.EvacuationBus)
            {
                return tsdRef.Color;
            }

            bool prefixBased = config.PalettePrefixBased;

            bool randomOnOverflow = config.PaletteRandomOnOverflow;

            var pal = new List<string>();

            if (num >= 0 && config.Prefix != NamingMode.None)
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
                    if (prefix > 0 && prefixBased)
                    {
                        num /= 1000;
                    }
                    else
                    {
                        num %= 1000;
                    }
                }
                pal.Add(config.Palette);
            }
            else
            {
                pal.Add(config.Palette);
            }
            Color c;
            c = TLMAutoColorPaletteContainer.GetColor(num, pal.ToArray(), randomOnOverflow, avoidRandom);
            if (c == Color.clear && !allowClear)
            {
                c = tsdRef.Color;
            }
            return c;
        }
        internal static LineIconSpriteNames GetLineIcon(ushort num, TransportSystemDefinition tsdRef)
        {
            var config = tsdRef.GetConfig();

            if (num > 0 && config.Prefix != NamingMode.None)
            {
                uint prefix = num / 1000u;
                ITLMTransportTypeExtension ext = tsdRef.GetTransportExtension();
                LineIconSpriteNames format = ext.GetCustomFormat(prefix);
                if (format != default)
                {
                    return format;
                }

            }
            return tsdRef.GetBgIcon();
        }
        internal static string[] GetStringOptionsForPrefix(TransportSystemDefinition tsd, bool showUnprefixed = false, bool useNameRefSystem = false, bool noneOption = true)
        {
            var config = tsd.GetConfig();
            var prefixNamingMode = config.Prefix;
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
            if (prefixNamingMode == NamingMode.None)
            {
                return saida.ToArray();
            }
            switch (prefixNamingMode)
            {
                case NamingMode.GreekUpper:
                case NamingMode.GreekUpperNumber:
                    AddToArrayWithName(gregoMaiusculo, saida, tsd, useNameRefSystem);
                    break;
                case NamingMode.GreekLower:
                case NamingMode.GreekLowerNumber:
                    AddToArrayWithName(gregoMinusculo, saida, tsd, useNameRefSystem);
                    break;
                case NamingMode.CyrillicUpper:
                case NamingMode.CyrillicUpperUpper:
                    AddToArrayWithName(cirilicoMaiusculo, saida, tsd, useNameRefSystem);
                    break;
                case NamingMode.CyrillicLower:
                case NamingMode.CyrillicLowerNumber:
                    AddToArrayWithName(cirilicoMinusculo, saida, tsd, useNameRefSystem);
                    break;
                case NamingMode.LatinUpper:
                case NamingMode.LatinUpperNumber:
                    AddToArrayWithName(latinoMaiusculo, saida, tsd, useNameRefSystem);
                    break;
                case NamingMode.LatinLower:
                case NamingMode.LatinLowerNumber:
                    AddToArrayWithName(latinoMinusculo, saida, tsd, useNameRefSystem);
                    break;
                case NamingMode.Number:
                    string[] temp = new string[64];
                    for (int i = 1; i <= 64; i++)
                    {
                        temp[i - 1] = i.ToString();
                    }
                    AddToArrayWithName(temp, saida, tsd, useNameRefSystem);
                    break;
                case NamingMode.Roman:
                    string[] tempR = new string[64];
                    for (ushort i = 1; i <= 64; i++)
                    {
                        tempR[i - 1] = NumberingUtils.ToRomanNumeral(i);
                    }
                    AddToArrayWithName(tempR, saida, tsd, useNameRefSystem);
                    break;
            }
            if (TLMLineUtils.m_numberedNamingTypes.Contains(prefixNamingMode))
            {
                AddToArrayWithName(numeros, saida, tsd, useNameRefSystem);
            }
            if (!noneOption && !showUnprefixed)
            {
                saida.RemoveAt(0);
            }
            return saida.ToArray();
        }

        internal static string[] SimpleOptionsArrayForNamingType(NamingMode prefixNamingMode)
        {
            var saida = new List<string>(new string[1]);
            if (prefixNamingMode == NamingMode.None)
            {
                return saida.ToArray();
            }
            switch (prefixNamingMode)
            {
                case NamingMode.GreekUpper:
                case NamingMode.GreekUpperNumber:
                    saida.AddRange(gregoMaiusculo);
                    break;
                case NamingMode.GreekLower:
                case NamingMode.GreekLowerNumber:
                    saida.AddRange(gregoMinusculo);
                    break;
                case NamingMode.CyrillicUpper:
                case NamingMode.CyrillicUpperUpper:
                    saida.AddRange(cirilicoMaiusculo);
                    break;
                case NamingMode.CyrillicLower:
                case NamingMode.CyrillicLowerNumber:
                    saida.AddRange(cirilicoMinusculo);
                    break;
                case NamingMode.LatinUpper:
                case NamingMode.LatinUpperNumber:
                    saida.AddRange(latinoMaiusculo);
                    break;
                case NamingMode.LatinLower:
                case NamingMode.LatinLowerNumber:
                    saida.AddRange(latinoMinusculo);
                    break;
                case NamingMode.Number:
                    string[] temp = new string[64];
                    for (int i = 1; i <= 64; i++)
                    {
                        temp[i - 1] = i.ToString();
                    }
                    saida.AddRange(temp);
                    break;
                case NamingMode.Roman:
                    string[] tempR = new string[64];
                    for (ushort i = 1; i <= 64; i++)
                    {
                        tempR[i - 1] = NumberingUtils.ToRomanNumeral(i);
                    }
                    saida.AddRange(tempR);
                    break;
            }
            if (TLMLineUtils.m_numberedNamingTypes.Contains(prefixNamingMode))
            {
                saida.AddRange(numeros);
            }
            return saida.ToArray();
        }

        private static void AddToArrayWithName(string[] input, List<string> saida, TransportSystemDefinition tsd, bool addPrefixName)
        {
            if (addPrefixName)
            {
                ushort offset = (ushort)saida.Count;
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
            else
            {
                saida.AddRange(input);
            }

        }

        internal static List<string> GetPrefixesOptions(TransportSystemDefinition tsd, bool addDefaults = true)
        {
            var config = tsd.GetConfig();
            NamingMode m = config.Prefix;
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
            if (TLMLineUtils.m_numberedNamingTypes.Contains(m))
            {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida;
        }




        internal static string GetStringFromNameMode(NamingMode mode, int num) =>
                  mode == NamingMode.Roman ? NumberingUtils.ToRomanNumeral((ushort)num)
                : mode == NamingMode.Number ? num.ToString("D")
                : NumberingUtils.GetStringFromNumber(SimpleOptionsArrayForNamingType(mode), num + 1);

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

                return invertPrefixSuffix && sufixo == NamingMode.Number && prefixo != NamingMode.Number && prefixo != NamingMode.Roman
                    ? sufixoSaida + separadorSaida + prefixoSaida
                    : prefixoSaida + separadorSaida + sufixoSaida;
            }
            else
            {
                return prefixoSaida;
            }
        }
        #endregion
    }

}

