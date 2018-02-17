using ColossalFramework;
using ColossalFramework.Math;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors.BuildingAIExt
{
    class TLMDepotAI : Redirector<TLMDepotAI>
    {
        #region Save file handle
        private const string SEPARATOR = "∂";
        private const string COMMA = "∞";
        private const string SUBCOMMA = "≠";
        private static readonly List<uint> defaultPrefixList = new List<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65 });

        private static Dictionary<TransportSystemDefinition, Dictionary<ushort, List<uint>>> cached_lists = new Dictionary<TransportSystemDefinition, Dictionary<ushort, List<uint>>>();

        private static Dictionary<ushort, List<uint>> getDictionaryFromConfigString(string s, TransportSystemDefinition tsd)
        {
            Dictionary<ushort, List<uint>> saida = new Dictionary<ushort, List<uint>>();
            var tempArray = s.Split(COMMA.ToCharArray());
            var bm = Singleton<BuildingManager>.instance;
            foreach (string i in tempArray)
            {
                var kv = i.Split(SEPARATOR.ToCharArray());
                if (kv.Length == 2)
                {

                    if (ushort.TryParse(kv[0], out ushort key))
                    {
                        if (bm.m_buildings.m_buffer[key].Info.GetAI() is DepotAI buildingAI && tsd.isFromSystem(buildingAI))
                        {
                            saida[key] = new List<uint>();
                            var subtempArray = kv[1].Split(SUBCOMMA.ToCharArray());
                            foreach (string j in subtempArray)
                            {
                                if (uint.TryParse(j, out uint value))
                                {
                                    saida[key].Add(value);
                                }
                            }
                        }
                    }
                }
            }
            return saida;
        }

        private static string getConfigStringFromDictionary(Dictionary<ushort, List<uint>> d)
        {
            List<string> temp = new List<string>();
            foreach (ushort i in d.Keys)
            {
                temp.Add(i + SEPARATOR + string.Join(SUBCOMMA, d[i].Select(x => x.ToString()).ToArray()));
            }
            return string.Join(COMMA, temp.ToArray());
        }

        private static void saveConfigForTransportType(TransportSystemDefinition tsd, Dictionary<ushort, List<uint>> value)
        {
            string depotList = getConfigStringFromDictionary(value);
            TLMConfigWarehouse.setCurrentConfigString(TLMConfigWarehouse.getConfigDepotPrefix(tsd), depotList);
            cached_lists[tsd] = value;
        }
        #endregion

        #region Cache Manager
        private static void cleanCache()
        {
            cached_lists = new Dictionary<TransportSystemDefinition, Dictionary<ushort, List<uint>>>();
        }

        private static Dictionary<ushort, List<uint>> getConfigForTransportType(TransportSystemDefinition tsd)
        {
            if (!cached_lists.ContainsKey(tsd))
            {
                string depotList = TLMConfigWarehouse.getCurrentConfigString(TLMConfigWarehouse.getConfigDepotPrefix(tsd));
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                    TLMUtils.doLog("getConfigForTransportType STRING FOR {0}: {1}", tsd.ToString(), depotList);
                cached_lists[tsd] = getDictionaryFromConfigString(depotList, tsd);
            }
            return cached_lists[tsd];
        }

        #endregion

        #region CRUD depots
        public static void addPrefixToDepot(ushort buildingID, uint prefix, bool secondary)
        {
            var bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.from(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                var dic = getConfigForTransportType(tsd);
                if (!dic.ContainsKey(buildingID))
                {
                    dic[buildingID] = new List<uint>(defaultPrefixList);
                }
                dic[buildingID].Add(prefix);

                saveConfigForTransportType(tsd, dic);
            }
        }

        public static void addAllPrefixesToDepot(ushort buildingID, bool secondary)
        {
            var bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.from(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                var dic = getConfigForTransportType(tsd);
                if (dic.ContainsKey(buildingID))
                {
                    dic.Remove(buildingID);
                }
                saveConfigForTransportType(tsd, dic);
            }
        }

        public static void removePrefixFromDepot(ushort buildingID, uint prefix, bool secondary)
        {
            var bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.from(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                var dic = getConfigForTransportType(tsd);
                if (!dic.ContainsKey(buildingID))
                {
                    dic[buildingID] = new List<uint>(defaultPrefixList);
                }
                dic[buildingID].Remove(prefix);
                saveConfigForTransportType(tsd, dic);
            }
        }

        public static void removeAllPrefixesFromDepot(ushort buildingID, bool secondary)
        {
            var bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.from(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                var dic = getConfigForTransportType(tsd);
                dic[buildingID] = new List<uint>();
                saveConfigForTransportType(tsd, dic);
            }
        }

        #endregion

        #region Utility methods

        public static List<ushort> getAllDepotsFromCity(TransportSystemDefinition tsd)
        {
            List<ushort> saida = new List<ushort>();
            var bm = Singleton<BuildingManager>.instance;
            var buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                if (bm.m_buildings.m_buffer[i].Info.GetAI() is DepotAI buildingAI && tsd.isFromSystem(buildingAI))
                {
                    saida.Add(i);
                }
            }
            return saida;
        }
        public static List<ushort> getAllDepotsFromCity()
        {
            List<ushort> saida = new List<ushort>();
            var bm = Singleton<BuildingManager>.instance;
            var buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                PrefabAI prefAI = bm.m_buildings.m_buffer[i].Info.GetAI();
                if (prefAI is DepotAI buildingAI && buildingAI.m_maxVehicleCount > 0)
                {
                    saida.Add(i);
                }
            }
            return saida;
        }

        public static List<uint> getPrefixesServedByDepot(ushort buildingID, bool secondary)
        {
            var bm = Singleton<BuildingManager>.instance;
            var buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.from(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                if (tsd != default(TransportSystemDefinition))
                {
                    var dic = getConfigForTransportType(tsd);
                    if (!dic.ContainsKey(buildingID))
                    {
                        return defaultPrefixList;
                    }
                    return dic[buildingID];
                }
            }
            return null;
        }

        public static List<ushort> getAllowedDepotsForPrefix(TransportSystemDefinition tsd, uint prefix)
        {
            var dic = getConfigForTransportType(tsd);
            List<ushort> saida = getAllDepotsFromCity(tsd);
            foreach (ushort i in dic.Keys)
            {
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                    TLMUtils.doLog("dic[i]: {{{0}}} ||  prefix = {1} || contains = {2}  ", string.Join(",", dic[i].Select(x => x.ToString()).ToArray()), prefix, dic[i].Contains(prefix));
                if (!dic[i].Contains(prefix))
                {
                    saida.Remove(i);
                }
            }
            return saida;
        }

        #endregion

        #region Generation methods

        private static VehicleInfo doModelDraw(ushort lineId)
        {
            var extension = TLMLineUtils.getExtensionFromTransportLine(lineId);
            var randomInfo = extension?.GetAModel(lineId);
            return randomInfo;
        }

        public static void setRandomBuildingByPrefix(TransportSystemDefinition tsd, uint prefix, ref ushort currentId)
        {
            var allowedDepots = getAllowedDepotsForPrefix(tsd, prefix);
            if (allowedDepots.Count == 0 || allowedDepots.Contains(currentId))
            {
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                    TLMUtils.doLog("allowedDepots.Count --{0}-- == 0||  allowedDepots.Contains({1}): --{2}--  ", allowedDepots.Count, currentId, string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()));
                return;
            }
            Randomizer r = new Randomizer(new System.Random().Next());
            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("DEPOT POSSIBLE VALUES FOR {2} PREFIX {1}: {0} ", string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()), prefix, tsd);
            currentId = allowedDepots[r.Int32(0, allowedDepots.Count - 1)];
            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("DEPOT FOR {2} PREFIX {1}: {0} ", currentId, prefix, tsd);
        }
        #endregion

        #region Overrides

        private static readonly TransferManager.TransferReason[] managedReasons = new TransferManager.TransferReason[]   {
                TransferManager.TransferReason.Tram,
                TransferManager.TransferReason.PassengerTrain,
                TransferManager.TransferReason.PassengerShip,
                TransferManager.TransferReason.PassengerPlane,
                TransferManager.TransferReason.MetroTrain,
                TransferManager.TransferReason.Monorail,
                TransferManager.TransferReason.CableCar,
                TransferManager.TransferReason.Blimp,
                TransferManager.TransferReason.Bus,
                TransferManager.TransferReason.Ferry
            };

        public static bool StartTransfer(DepotAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (!managedReasons.Contains(reason) || offer.TransportLine == 0)
            {
                return true;
            }

            TLMUtils.doLog("START TRANSFER!!!!!!!!");
            TransportInfo m_transportInfo = __instance.m_transportInfo;
            BuildingInfo m_info = __instance.m_info;

            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("m_info {0} | m_transportInfo {1} | Line: {2}", m_info.name, m_transportInfo.name, offer.TransportLine);


            if (reason == m_transportInfo.m_vehicleReason || (__instance.m_secondaryTransportInfo != null && reason == __instance.m_secondaryTransportInfo.m_vehicleReason))
            {
                VehicleInfo randomVehicleInfo = null;
                var tsd = TransportSystemDefinition.from(__instance.m_transportInfo);

                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[offer.TransportLine];
                TransportInfo.TransportType t = tl.Info.m_transportType;

                if (TLMLineUtils.hasPrefix(ref tl))
                {
                    setRandomBuildingByPrefix(tsd, tl.m_lineNumber / 1000u, ref buildingID);
                }
                else
                {
                    setRandomBuildingByPrefix(tsd, 0, ref buildingID);
                }

                TLMUtils.doLog("randomVehicleInfo");
                randomVehicleInfo = doModelDraw(offer.TransportLine);
                if (randomVehicleInfo == null)
                {
                    randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
                }
                if (randomVehicleInfo != null)
                {
                    TLMUtils.doLog("randomVehicleInfo != null");
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    __instance.CalculateSpawnPosition(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, out Vector3 position, out Vector3 vector);
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicleID, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, position, reason, false, true))
                    {
                        TLMUtils.doLog("CreatedVehicle!!!");
                        randomVehicleInfo.m_vehicleAI.SetSource(vehicleID, ref vehicles.m_buffer[(int)vehicleID], buildingID);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(vehicleID, ref vehicles.m_buffer[(int)vehicleID], reason, offer);
                    }
                    return false;
                }
            }
            return true;

        }
        // CommonBuildingAI
        MethodInfo CalculateSpawnPosition = typeof(DepotAI).GetMethod("CalculateSpawnPosition", allFlags);
        #endregion

        #region Hooking

        public override void Awake()
        {
            TLMUtils.doLog("Loading Depot Hooks!");
            AddRedirect(typeof(DepotAI).GetMethod("StartTransfer", allFlags), typeof(TLMDepotAI).GetMethod("StartTransfer", allFlags));
        }

        #endregion

    }
}
