using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    class TLMDepotAI : Redirector
    {
        public static TLMDepotAI _instance;
        public static TLMDepotAI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMDepotAI();
                }
                return _instance;
            }
        }
        private const string SEPARATOR = "∂";
        private const string COMMA = "∞";
        private const string SUBCOMMA = "≠";
        private static readonly List<uint> defaultPrefixList = new List<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65 });

        private static Dictionary<TransportInfo.TransportType, Dictionary<ushort, List<uint>>> cached_lists = new Dictionary<TransportInfo.TransportType, Dictionary<ushort, List<uint>>>();

        private static Dictionary<ushort, List<uint>> getDictionaryFromConfigString(string s, TransportInfo.TransportType t)
        {
            Dictionary<ushort, List<uint>> saida = new Dictionary<ushort, List<uint>>();
            var tempArray = s.Split(COMMA.ToCharArray());
            var bm = Singleton<BuildingManager>.instance;
            foreach (string i in tempArray)
            {
                var kv = i.Split(SEPARATOR.ToCharArray());
                if (kv.Length == 2)
                {
                    ushort key;

                    if (ushort.TryParse(kv[0], out key))
                    {
                        DepotAI buildingAI = bm.m_buildings.m_buffer[key].Info.GetAI() as DepotAI;
                        if (buildingAI != null && buildingAI.m_transportInfo.m_transportType == t)
                        {
                            saida[key] = new List<uint>();
                            var subtempArray = kv[1].Split(SUBCOMMA.ToCharArray());
                            foreach (string j in subtempArray)
                            {
                                uint value;
                                if (uint.TryParse(j, out value))
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



        private static void cleanCache()
        {
            cached_lists = new Dictionary<TransportInfo.TransportType, Dictionary<ushort, List<uint>>>();
        }

        private static Dictionary<ushort, List<uint>> getConfigForTransportType(TransportInfo.TransportType t)
        {
            if (!cached_lists.ContainsKey(t))
            {
                string depotList = TLMConfigWarehouse.getCurrentConfigString(TLMConfigWarehouse.getConfigDepotPrefix(t));
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("getConfigForTransportType STRING FOR {0}: {1}", t.ToString(), depotList);
                cached_lists[t] = getDictionaryFromConfigString(depotList, t);
            }
            return cached_lists[t];
        }

        private static void saveConfigForTransportType(TransportInfo.TransportType t, Dictionary<ushort, List<uint>> value)
        {
            string depotList = getConfigStringFromDictionary(value);
            TLMConfigWarehouse.setCurrentConfigString(TLMConfigWarehouse.getConfigDepotPrefix(t), depotList);
            cached_lists[t] = value;
        }

        public static void addPrefixToDepot(ushort buildingID, uint prefix)
        {
            var bm = Singleton<BuildingManager>.instance;
            DepotAI buildingAI = bm.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI;
            if (buildingAI != null)
            {
                var dic = getConfigForTransportType(buildingAI.m_transportInfo.m_transportType);
                if (!dic.ContainsKey(buildingID))
                {
                    dic[buildingID] = new List<uint>(defaultPrefixList);
                }
                dic[buildingID].Add(prefix);

                saveConfigForTransportType(buildingAI.m_transportInfo.m_transportType, dic);
            }
        }

        public static void addAllPrefixesToDepot(ushort buildingID)
        {
            var bm = Singleton<BuildingManager>.instance;
            DepotAI buildingAI = bm.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI;
            if (buildingAI != null)
            {
                var dic = getConfigForTransportType(buildingAI.m_transportInfo.m_transportType);
                if (dic.ContainsKey(buildingID))
                {
                    dic.Remove(buildingID);
                }
                saveConfigForTransportType(buildingAI.m_transportInfo.m_transportType, dic);
            }
        }

        public static void removePrefixFromDepot(ushort buildingID, uint prefix)
        {
            var bm = Singleton<BuildingManager>.instance;
            DepotAI buildingAI = bm.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI;
            if (buildingAI != null)
            {
                var dic = getConfigForTransportType(buildingAI.m_transportInfo.m_transportType);
                if (!dic.ContainsKey(buildingID))
                {
                    dic[buildingID] = new List<uint>(defaultPrefixList);
                }
                dic[buildingID].Remove(prefix);
                saveConfigForTransportType(buildingAI.m_transportInfo.m_transportType, dic);
            }
        }

        public static void removeAllPrefixesFromDepot(ushort buildingID)
        {
            var bm = Singleton<BuildingManager>.instance;
            DepotAI buildingAI = bm.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI;
            if (buildingAI != null)
            {
                var dic = getConfigForTransportType(buildingAI.m_transportInfo.m_transportType);
                dic[buildingID] = new List<uint>();
                saveConfigForTransportType(buildingAI.m_transportInfo.m_transportType, dic);
            }
        }

        public static List<ushort> getAllDepotsFromCity(TransportInfo.TransportType t)
        {
            List<ushort> saida = new List<ushort>();
            var bm = Singleton<BuildingManager>.instance;
            var buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                DepotAI buildingAI = bm.m_buildings.m_buffer[i].Info.GetAI() as DepotAI;
                if (buildingAI != null && buildingAI.m_transportInfo.m_transportType == t)
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
                DepotAI buildingAI = bm.m_buildings.m_buffer[i].Info.GetAI() as DepotAI;
                if (buildingAI != null)
                {
                    saida.Add(i);
                }
            }
            return saida;
        }

        public static List<uint> getPrefixesServedByDepot(ushort buildingID)
        {
            var bm = Singleton<BuildingManager>.instance;
            var buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            DepotAI buildingAI = bm.m_buildings.m_buffer[buildingID].Info.GetAI() as DepotAI;
            if (buildingAI != null)
            {
                var dic = getConfigForTransportType(buildingAI.m_transportInfo.m_transportType);
                if (!dic.ContainsKey(buildingID))
                {
                    return defaultPrefixList;
                }
                return dic[buildingID];
            }
            return null;
        }

        public static List<ushort> getAllowedDepotsForPrefix(TransportInfo.TransportType t, uint prefix)
        {
            var dic = getConfigForTransportType(t);
            List<ushort> saida = getAllDepotsFromCity(t);
            foreach (ushort i in dic.Keys)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("dic[i]: {{{0}}} ||  prefix = {1} || contains = {2}  ",string.Join(",", dic[i].Select(x => x.ToString()).ToArray()),prefix, dic[i].Contains(prefix));
                if (!dic[i].Contains(prefix))
                {
                    saida.Remove(i);
                }
            }
            return saida;
        }



        private static VehicleInfo doModelDraw(TransportLine t)
        {
            if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportType(t.Info.m_transportType) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
            {
                uint prefix = t.m_lineNumber / 1000u;
                BasicTransportExtension extension = TLMUtils.getExtensionFromTransportType(t.Info.m_transportType);
                var randomInfo = extension.getRandomModel(prefix);
                return randomInfo;

            }
            return null;
        }

        public void setRandomBuildingByPrefix(TransportInfo.TransportType tl, uint prefix, ref ushort currentId)
        {
            var allowedDepots = getAllowedDepotsForPrefix(tl, prefix);
            if (allowedDepots.Count == 0 || allowedDepots.Contains(currentId))
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("allowedDepots.Count --{0}-- == 0||  allowedDepots.Contains({1}): --{2}--  ", allowedDepots.Count, currentId, string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()));
                return;
            }
            Randomizer r = new Randomizer(new System.Random().Next());
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("DEPOT POSSIBLE VALUES FOR {2} PREFIX {1}: {0} ", string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()), prefix, tl);
            currentId = allowedDepots[r.Int32(0, allowedDepots.Count - 1)];
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("DEPOT FOR {2} PREFIX {1}: {0} ", currentId, prefix, tl);
        }


        public void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("START TRANSFER!!!!!!!!");
            DepotAI ai = ((DepotAI)data.Info.GetAI());
            TransportInfo m_transportInfo = ((DepotAI)data.Info.GetAI()).m_transportInfo;
            BuildingInfo m_info = ((DepotAI)data.Info.GetAI()).m_info;
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("m_info {0} | m_transportInfo {1} | Line: {2}", m_info.name, m_transportInfo.name, offer.TransportLine);

            if (reason == m_transportInfo.m_vehicleReason)
            {
                VehicleInfo randomVehicleInfo = null;
                if (offer.TransportLine != 0)
                {
                    TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[offer.TransportLine];
                    TransportInfo.TransportType t = tl.Info.m_transportType;
                    randomVehicleInfo = doModelDraw(tl);
                    if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportType(tl.Info.m_transportType) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
                    {
                        setRandomBuildingByPrefix(tl.Info.m_transportType, tl.m_lineNumber / 1000u, ref buildingID);
                    }
                    else
                    {
                        setRandomBuildingByPrefix(tl.Info.m_transportType, 0, ref buildingID);
                    }
                }
                else
                {
                    setRandomBuildingByPrefix(ai.m_transportInfo.m_transportType, 65, ref buildingID);
                }

                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("randomVehicleInfo");
                if (randomVehicleInfo == null)
                {
                    randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
                }
                if (randomVehicleInfo != null)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("randomVehicleInfo != null");
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    Vector3 position;
                    Vector3 vector;
                    this.CalculateSpawnPosition(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, out position, out vector);
                    ushort num;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out num, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, position, reason, false, true))
                    {
                        if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("CreatedVehicle!!!");
                        randomVehicleInfo.m_vehicleAI.SetSource(num, ref vehicles.m_buffer[(int)num], buildingID);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(num, ref vehicles.m_buffer[(int)num], reason, offer);
                    }
                }
            }
            else
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("nor StartTransferCommonBuildingAI");
                StartTransferCommonBuildingAI(buildingID, ref data, reason, offer);
            }
        }
        // CommonBuildingAI
        public void StartTransferCommonBuildingAI(ushort buildingID, ref Building data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer) { }
        public void CalculateSpawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target) { position = Vector3.zero; target = Vector3.zero; }

        #region Hooking
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();



        public void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Depot Hooks!");
            AddRedirect(typeof(DepotAI), typeof(TLMDepotAI).GetMethod("StartTransfer", allFlags), ref redirects);
            AddRedirect(typeof(TLMDepotAI), typeof(CommonBuildingAI).GetMethod("StartTransfer", allFlags), ref redirects, "StartTransferCommonBuildingAI");
            AddRedirect(typeof(TLMDepotAI), typeof(DepotAI).GetMethod("CalculateSpawnPosition", allFlags), ref redirects);
        }

        public void DisableHooks()
        {
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            redirects.Clear();
        }
        #endregion
    }
}
