using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.Utils.NumberArrays;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMUtils
    {
        public static readonly TransferManager.TransferReason[] defaultAllowedVehicleTypes = {
            TransferManager.TransferReason.Blimp ,
            TransferManager.TransferReason.CableCar ,
            TransferManager.TransferReason.Ferry ,
            TransferManager.TransferReason.MetroTrain ,
            TransferManager.TransferReason.Monorail ,
            TransferManager.TransferReason.PassengerTrain ,
            TransferManager.TransferReason.PassengerPlane ,
            TransferManager.TransferReason.PassengerShip ,
            TransferManager.TransferReason.Tram ,
            TransferManager.TransferReason.Bus
        };

        #region Prefix Operations
        internal static Color CalculateAutoColor(ushort num, TLMCW.ConfigIndex transportType, ref TransportSystemDefinition tsdRef, bool avoidRandom = false, bool allowClear = false)
        {
            if (transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG)
            {
                return TLMCW.getColorForTransportType(transportType);
            }

            bool prefixBased = TLMCW.GetCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_PREFIX_BASED);

            bool randomOnOverflow = TLMCW.GetCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);

            var pal = new List<string>();

            if (num >= 0 && TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX) != (int) ModoNomenclatura.Nenhum)
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

            if (num > 0 && TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX) != (int) ModoNomenclatura.Nenhum)
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

        private static string[] GetStringOptionsForPrefix(ModoNomenclatura m, ref TransportSystemDefinition tsd, bool useNameRefSystem = false, bool showUnprefixed = false, bool noneOption = true)
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
            if (m == ModoNomenclatura.Nenhum)
            {
                return saida.ToArray();
            }
            switch (m)
            {
                case ModoNomenclatura.GregoMaiusculo:
                case ModoNomenclatura.GregoMaiusculoNumero:
                    AddToArrayWithName(gregoMaiusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case ModoNomenclatura.GregoMinusculo:
                case ModoNomenclatura.GregoMinusculoNumero:
                    AddToArrayWithName(gregoMinusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                case ModoNomenclatura.CirilicoMaiusculoNumero:
                    AddToArrayWithName(cirilicoMaiusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                case ModoNomenclatura.CirilicoMinusculoNumero:
                    AddToArrayWithName(cirilicoMinusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                case ModoNomenclatura.LatinoMaiusculoNumero:
                    AddToArrayWithName(latinoMaiusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                case ModoNomenclatura.LatinoMinusculoNumero:
                    AddToArrayWithName(latinoMinusculo, saida, ref tsd, useNameRefSystem);
                    break;
                case ModoNomenclatura.Numero:
                    string[] temp = new string[64];
                    for (int i = 1; i <= 64; i++)
                    {
                        temp[i - 1] = i.ToString();
                    }
                    AddToArrayWithName(temp, saida, ref tsd, useNameRefSystem);
                    break;
                case ModoNomenclatura.Romano:
                    string[] tempR = new string[64];
                    for (ushort i = 1; i <= 64; i++)
                    {
                        tempR[i - 1] = NumberingUtils.ToRomanNumeral(i);
                    }
                    AddToArrayWithName(tempR, saida, ref tsd, useNameRefSystem);
                    break;
            }
            if (TLMLineUtils.nomenclaturasComNumeros.Contains(m))
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
            ushort offset = (ushort) saida.Count;
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
        internal static List<string> getPrefixesOptions(TLMCW.ConfigIndex transportType, bool addDefaults = true)
        {
            transportType &= TLMConfigWarehouse.ConfigIndex.SYSTEM_PART;
            ModoNomenclatura m = GetPrefixModoNomenclatura(transportType);
            doLog("getPrefixesOptions: MODO NOMENCLATURA = " + m);
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
                case ModoNomenclatura.GregoMaiusculo:
                case ModoNomenclatura.GregoMaiusculoNumero:
                    saida.AddRange(gregoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.GregoMinusculo:
                case ModoNomenclatura.GregoMinusculoNumero:
                    saida.AddRange(gregoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                case ModoNomenclatura.CirilicoMaiusculoNumero:
                    saida.AddRange(cirilicoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                case ModoNomenclatura.CirilicoMinusculoNumero:
                    saida.AddRange(cirilicoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                case ModoNomenclatura.LatinoMaiusculoNumero:
                    saida.AddRange(latinoMaiusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                case ModoNomenclatura.LatinoMinusculoNumero:
                    saida.AddRange(latinoMinusculo.Select(x => x.ToString()));
                    break;
                case ModoNomenclatura.Numero:
                    for (int i = 1; i <= 64; i++)
                    {
                        saida.Add(i.ToString());
                    }
                    break;
                case ModoNomenclatura.Romano:
                    for (ushort i = 1; i <= 64; i++)
                    {
                        saida.Add(NumberingUtils.ToRomanNumeral(i));
                    }
                    break;
            }
            if (TLMLineUtils.nomenclaturasComNumeros.Contains(m))
            {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida;
        }

        internal static ModoNomenclatura GetPrefixModoNomenclatura(TLMCW.ConfigIndex transportType) => (ModoNomenclatura) TLMCW.GetCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);

        #endregion

        #region Naming Utils
        internal static string getString(ModoNomenclatura prefixo, Separador s, ModoNomenclatura sufixo, ModoNomenclatura naoPrefixado, int numero, bool leadingZeros, bool invertPrefixSuffix)
        {
            string prefixoSaida = "";
            string separadorSaida = "";
            string sufixoSaida = "";
            int prefixNum = 0;
            if (prefixo != ModoNomenclatura.Nenhum)
            {
                prefixNum = numero / 1000;
                if (prefixo == ModoNomenclatura.Romano)
                {
                    prefixoSaida = NumberingUtils.ToRomanNumeral((ushort) prefixNum);
                }
                else
                {
                    TransportSystemDefinition tsd = default;
                    prefixoSaida = NumberingUtils.GetStringFromNumber(GetStringOptionsForPrefix(prefixo, ref tsd), prefixNum + 1);
                }
                numero = numero % 1000;
            }

            if (numero > 0)
            {
                if (prefixoSaida != "")
                {
                    switch (s)
                    {
                        case Separador.Barra:
                            separadorSaida = "/";
                            break;
                        case Separador.Espaco:
                            separadorSaida = " ";
                            break;
                        case Separador.Hifen:
                            separadorSaida = "-";
                            break;
                        case Separador.Ponto:
                            separadorSaida = ".";
                            break;
                        case Separador.QuebraLinha:
                            separadorSaida = "\n";
                            break;
                        case Separador.Nenhum:
                            if (prefixo == ModoNomenclatura.Romano)
                            {
                                separadorSaida = "·";
                            }
                            break;
                    }
                }
                switch (prefixo != ModoNomenclatura.Nenhum && prefixNum > 0 ? sufixo : naoPrefixado)
                {
                    case ModoNomenclatura.GregoMaiusculo:
                        sufixoSaida = NumberingUtils.GetStringFromNumber(gregoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.GregoMinusculo:
                        sufixoSaida = NumberingUtils.GetStringFromNumber(gregoMinusculo, numero);
                        break;
                    case ModoNomenclatura.CirilicoMaiusculo:
                        sufixoSaida = NumberingUtils.GetStringFromNumber(cirilicoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.CirilicoMinusculo:
                        sufixoSaida = NumberingUtils.GetStringFromNumber(cirilicoMinusculo, numero);
                        break;
                    case ModoNomenclatura.LatinoMaiusculo:
                        sufixoSaida = NumberingUtils.GetStringFromNumber(latinoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.LatinoMinusculo:
                        sufixoSaida = NumberingUtils.GetStringFromNumber(latinoMinusculo, numero);
                        break;
                    case ModoNomenclatura.Romano:
                        sufixoSaida = NumberingUtils.ToRomanNumeral((ushort) numero);
                        break;
                    default:
                        if (leadingZeros && prefixoSaida != "")
                        {
                            sufixoSaida = numero.ToString("D3");
                        }
                        else
                        {
                            sufixoSaida = numero.ToString();
                        }
                        break;
                }

                if (invertPrefixSuffix && sufixo == ModoNomenclatura.Numero && prefixo != ModoNomenclatura.Numero && prefixo != ModoNomenclatura.Romano)
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


        private static string getString(ModoNomenclatura m, int numero)
        {

            switch (m)
            {
                case ModoNomenclatura.GregoMaiusculo:
                    return NumberingUtils.GetStringFromNumber(gregoMaiusculo, numero);
                case ModoNomenclatura.GregoMinusculo:
                    return NumberingUtils.GetStringFromNumber(gregoMinusculo, numero);
                case ModoNomenclatura.CirilicoMaiusculo:
                    return NumberingUtils.GetStringFromNumber(cirilicoMaiusculo, numero);
                case ModoNomenclatura.CirilicoMinusculo:
                    return NumberingUtils.GetStringFromNumber(cirilicoMinusculo, numero);
                case ModoNomenclatura.LatinoMaiusculo:
                    return NumberingUtils.GetStringFromNumber(latinoMaiusculo, numero);
                case ModoNomenclatura.LatinoMinusculo:
                    return NumberingUtils.GetStringFromNumber(latinoMinusculo, numero);
                case ModoNomenclatura.Romano:
                    return NumberingUtils.ToRomanNumeral((ushort) numero);
                default:
                    return "" + numero;
            }
        }
        #endregion

        #region Building Utils
        public static string getBuildingName(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix, ushort lineId = 0)
        {

            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;

            Building b = bm.m_buildings.m_buffer[buildingId];
            while (b.m_parentBuilding > 0)
            {
                doLog("getBuildingName(): building id {0} - parent = {1}", buildingId, b.m_parentBuilding);
                buildingId = b.m_parentBuilding;
                b = bm.m_buildings.m_buffer[buildingId];
            }
            InstanceID iid = default;
            iid.Building = buildingId;
            serviceFound = b.Info?.GetService() ?? default;
            subserviceFound = b.Info?.GetSubService() ?? default;
            var index = GameServiceExtensions.ToConfigIndex(serviceFound, subserviceFound);
            TransportSystemDefinition tsd = default;
            if ((index & TLMCW.ConfigIndex.DESC_DATA) == TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG)
            {
                tsd = TransportSystemDefinition.From(b.Info.GetAI());
                index = tsd.ToConfigIndex();
            }
            prefix = index.GetSystemStationNamePrefix(lineId)?.TrimStart();
            doLog($"getBuildingName(): serviceFound {serviceFound} - subserviceFound = {subserviceFound} - tsd = {tsd} - index = {index} - prefix = {prefix}");

            return bm.GetBuildingName(buildingId, iid);
        }
        #endregion
        #region Logging
        public static void doLog(string format, params object[] args) => LogUtils.DoLog(format, args);
        public static void doErrorLog(string format, params object[] args) => LogUtils.DoErrorLog(format, args);
        #endregion

        internal static List<string> LoadBasicAssets(ref TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();

            TLMUtils.doLog("LoadBasicAssets: pre prefab read");
            for (uint num = 0u; num < (ulong) PrefabCollection<VehicleInfo>.PrefabCount(); num += 1u)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && definition.IsFromSystem(prefab) && !VehicleUtils.IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
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
        Nenhum = 7,
        LatinoMinusculoNumero = 8,
        LatinoMaiusculoNumero = 9,
        GregoMinusculoNumero = 10,
        GregoMaiusculoNumero = 11,
        CirilicoMinusculoNumero = 12,
        CirilicoMaiusculoNumero = 13,
        Romano = 14
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

}

