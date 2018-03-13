using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Utils;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.NetNodeExt;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMUtils : KlyteUtils
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
        internal static Color CalculateAutoColor(ushort num, TLMCW.ConfigIndex transportType, bool avoidRandom = false)
        {
            if (transportType == TLMCW.ConfigIndex.EVAC_BUS_CONFIG)
            {
                return TLMCW.getColorForTransportType(transportType);
            }

            bool prefixBased = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_PREFIX_BASED);

            bool randomOnOverflow = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);

            string pal = TLMCW.getCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_SUBLINE);

            if (num >= 1000 && TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
            {
                pal = TLMCW.getCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_MAIN);
                if (prefixBased)
                {
                    num /= 1000;
                }
                else
                {
                    num %= 1000;
                }
            }
            Color c;
            c = TLMAutoColorPalettes.getColor(num, pal, randomOnOverflow, avoidRandom);
            if (c == Color.clear)
            {
                c = TLMCW.getColorForTransportType(transportType);
            }
            return c;
        }
        internal static string[] getStringOptionsForPrefix(ModoNomenclatura m, bool showUnprefixed = false, TLMCW.ConfigIndex nameReferenceSystem = TLMCW.ConfigIndex.NIL)
        {

            List<string> saida = new List<string>(new string[] { "" });
            if (showUnprefixed)
            {
                string unprefixedName = Locale.Get("TLM_UNPREFIXED");
                if (nameReferenceSystem != TLMCW.ConfigIndex.NIL)
                {
                    string prefixName = TLMLineUtils.getTransportSystemPrefixName(nameReferenceSystem, 0);
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
                    addToArrayWithName(gregoMaiusculo, saida, nameReferenceSystem);
                    break;
                case ModoNomenclatura.GregoMinusculo:
                case ModoNomenclatura.GregoMinusculoNumero:
                    addToArrayWithName(gregoMinusculo, saida, nameReferenceSystem);
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                case ModoNomenclatura.CirilicoMaiusculoNumero:
                    addToArrayWithName(cirilicoMaiusculo, saida, nameReferenceSystem);
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                case ModoNomenclatura.CirilicoMinusculoNumero:
                    addToArrayWithName(cirilicoMinusculo, saida, nameReferenceSystem);
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                case ModoNomenclatura.LatinoMaiusculoNumero:
                    addToArrayWithName(latinoMaiusculo, saida, nameReferenceSystem);
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                case ModoNomenclatura.LatinoMinusculoNumero:
                    addToArrayWithName(latinoMinusculo, saida, nameReferenceSystem);
                    break;
                case ModoNomenclatura.Numero:
                    string[] temp = new string[64];
                    for (int i = 1; i <= 64; i++)
                    {
                        temp[i - 1] = i.ToString();
                    }
                    addToArrayWithName(temp, saida, nameReferenceSystem);
                    break;
                case ModoNomenclatura.Romano:
                    string[] tempR = new string[64];
                    for (ushort i = 1; i <= 64; i++)
                    {
                        tempR[i - 1] = ToRomanNumeral(i);
                    }
                    addToArrayWithName(tempR, saida, nameReferenceSystem);
                    break;
            }
            if (TLMLineUtils.nomenclaturasComNumeros.Contains(m))
            {
                addToArrayWithName(numeros, saida, nameReferenceSystem);
            }
            return saida.ToArray();
        }
        private static void addToArrayWithName(string[] input, List<string> saida, TLMCW.ConfigIndex nameReferenceSystem)
        {
            if (nameReferenceSystem == TLMCW.ConfigIndex.NIL)
            {
                saida.AddRange(input);
            }
            else
            {
                for (uint i = 0; i < input.Length; i++)
                {
                    string item = input[i];
                    string prefixName = TLMLineUtils.getTransportSystemPrefixName(nameReferenceSystem, i + 1);
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
        internal static string[] getPrefixesOptions(TLMCW.ConfigIndex transportType, Boolean addDefaults = true)
        {
            transportType &= TLMConfigWarehouse.ConfigIndex.SYSTEM_PART;
            var m = (ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);
            List<string> saida = new List<string>();
            if (addDefaults)
            {
                saida.AddRange(new string[] { Locale.Get("TLM_ALL"), Locale.Get("TLM_UNPREFIXED") });
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
                        saida.Add(ToRomanNumeral(i));
                    }
                    break;
            }
            if (TLMLineUtils.nomenclaturasComNumeros.Contains(m))
            {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida.ToArray();
        }
        internal static List<string> getDepotPrefixesOptions(TLMCW.ConfigIndex transportType)
        {
            transportType &= TLMConfigWarehouse.ConfigIndex.SYSTEM_PART;
            var m = (ModoNomenclatura)TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX);
            List<string> saida = new List<string>(new string[] { Locale.Get("TLM_UNPREFIXED") });
            switch (m)
            {
                case ModoNomenclatura.GregoMaiusculo:
                case ModoNomenclatura.GregoMaiusculoNumero:
                    saida.AddRange(gregoMaiusculo);
                    break;
                case ModoNomenclatura.GregoMinusculo:
                case ModoNomenclatura.GregoMinusculoNumero:
                    saida.AddRange(gregoMinusculo);
                    break;
                case ModoNomenclatura.CirilicoMaiusculo:
                case ModoNomenclatura.CirilicoMaiusculoNumero:
                    saida.AddRange(cirilicoMaiusculo);
                    break;
                case ModoNomenclatura.CirilicoMinusculo:
                case ModoNomenclatura.CirilicoMinusculoNumero:
                    saida.AddRange(cirilicoMinusculo);
                    break;
                case ModoNomenclatura.LatinoMaiusculo:
                case ModoNomenclatura.LatinoMaiusculoNumero:
                    saida.AddRange(latinoMaiusculo);
                    break;
                case ModoNomenclatura.LatinoMinusculo:
                case ModoNomenclatura.LatinoMinusculoNumero:
                    saida.AddRange(latinoMinusculo);
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
                        saida.Add(ToRomanNumeral(i));
                    }
                    break;
            }
            if (TLMLineUtils.nomenclaturasComNumeros.Contains(m))
            {
                saida.AddRange(numeros.Select(x => x.ToString()));
            }
            return saida;
        }
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
                    prefixoSaida = ToRomanNumeral((ushort)prefixNum);
                }
                else
                {
                    prefixoSaida = getStringFromNumber(getStringOptionsForPrefix(prefixo), prefixNum + 1);
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
                        sufixoSaida = getStringFromNumber(gregoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.GregoMinusculo:
                        sufixoSaida = getStringFromNumber(gregoMinusculo, numero);
                        break;
                    case ModoNomenclatura.CirilicoMaiusculo:
                        sufixoSaida = getStringFromNumber(cirilicoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.CirilicoMinusculo:
                        sufixoSaida = getStringFromNumber(cirilicoMinusculo, numero);
                        break;
                    case ModoNomenclatura.LatinoMaiusculo:
                        sufixoSaida = getStringFromNumber(latinoMaiusculo, numero);
                        break;
                    case ModoNomenclatura.LatinoMinusculo:
                        sufixoSaida = getStringFromNumber(latinoMinusculo, numero);
                        break;
                    case ModoNomenclatura.Romano:
                        sufixoSaida = ToRomanNumeral((ushort)numero);
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
                case ModoNomenclatura.Romano:
                    return ToRomanNumeral((ushort)numero);
                default:
                    return "" + numero;
            }
        }
        #endregion

        #region Building Utils
        public static string getBuildingName(ushort buildingId, out ItemClass.Service serviceFound, out ItemClass.SubService subserviceFound, out string prefix)
        {

            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;

            Building b = bm.m_buildings.m_buffer[buildingId];
            while (b.m_parentBuilding > 0)
            {
                doLog("getStationNameWithPrefix(): building id {0} - parent = {1}", buildingId, b.m_parentBuilding);
                buildingId = b.m_parentBuilding;
                b = bm.m_buildings.m_buffer[buildingId];
            }
            InstanceID iid = default(InstanceID);
            iid.Building = buildingId;
            serviceFound = b.Info?.GetService() ?? default(ItemClass.Service);
            subserviceFound = b.Info?.GetSubService() ?? default(ItemClass.SubService);
            TLMCW.ConfigIndex index = GameServiceExtensions.toConfigIndex(serviceFound, subserviceFound);
            if (index == TLMCW.ConfigIndex.PUBLICTRANSPORT_SERVICE_CONFIG)
            {
                var tsd = TransportSystemDefinition.from(b.Info.GetAI());
                index = tsd?.toConfigIndex() ?? index;
            }
            prefix = index.getPrefixTextNaming()?.Trim();

            return bm.GetBuildingName(buildingId, iid);
        }
        #endregion
        #region Logging
        public static void doLog(string format, params object[] args)
        {
            try
            {
                if (TLMSingleton.debugMode)
                {
                    if (TLMSingleton.instance != null)
                    {
                        Debug.LogWarningFormat("TLMRv" + TLMSingleton.version + " " + format, args);

                    }
                    else
                    {
                        Console.WriteLine("TLMRv" + TLMSingleton.version + " " + format, args);
                    }
                }
            }
            catch
            {
                Debug.LogErrorFormat("TLMRv" + TLMSingleton.version + " Erro ao fazer log: {0} (args = {1})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        public static void doErrorLog(string format, params object[] args)
        {
            try
            {
                if (TLMSingleton.instance != null)
                {
                    Debug.LogErrorFormat("TLMRv" + TLMSingleton.version + " " + format, args);
                }
                else
                {
                    Console.WriteLine("TLMRv" + TLMSingleton.version + " " + format, args);
                }
            }
            catch
            {
                Debug.LogErrorFormat("TLMRv" + TLMSingleton.version + " Erro ao logar ERRO!!!: {0} (args = [{1}])", format, args == null ? "" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }

        }
        #endregion

        internal static List<string> LoadBasicAssets(TransportSystemDefinition definition)
        {
            List<string> basicAssetsList = new List<string>();

            TLMUtils.doLog("LoadBasicAssets: pre prefab read");
            for (uint num = 0u; (ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount()); num += 1u)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && definition.isFromSystem(prefab) && !IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
    }


    internal enum ModoNomenclatura
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

    internal enum Separador
    {
        Nenhum = 0,
        Hifen = 1,
        Ponto = 2,
        Barra = 3,
        Espaco = 4,
        QuebraLinha = 5
    }

}

