using ColossalFramework;
using ColossalFramework.Math;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.Extensors.BuildingAIExt
{
    internal class TLMDepotAI : IRedirectable
    {
        #region Save file handle
        private const string SEPARATOR = "∂";
        private const string COMMA = "∞";
        private const string SUBCOMMA = "≠";
        private static readonly List<uint> defaultPrefixList = new List<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65 });

        private static Dictionary<TransportSystemDefinition, Dictionary<ushort, List<uint>>> cached_lists = new Dictionary<TransportSystemDefinition, Dictionary<ushort, List<uint>>>();

        private static Dictionary<ushort, List<uint>> getDictionaryFromConfigString(string s, ref TransportSystemDefinition tsd)
        {
            TLMUtils.doLog("A");
            var saida = new Dictionary<ushort, List<uint>>();
            ;
            TLMUtils.doLog("b");
            string[] tempArray = (s ?? "").Split(COMMA.ToCharArray());
            TLMUtils.doLog("c");
            BuildingManager bm = Singleton<BuildingManager>.instance;
            TLMUtils.doLog("d");
            foreach (string i in tempArray)
            {
                TLMUtils.doLog("e");
                string[] kv = i.Split(SEPARATOR.ToCharArray());
                TLMUtils.doLog("f");
                if (kv.Length == 2)
                {
                    try
                    {
                        TLMUtils.doLog("g");
                        if (ushort.TryParse(kv[0], out ushort key))
                        {
                            TLMUtils.doLog("h");
                            if (bm?.m_buildings?.m_buffer?[key].Info?.GetAI() is DepotAI buildingAI && tsd.IsFromSystem(buildingAI))
                            {
                                TLMUtils.doLog("i");
                                saida[key] = new List<uint>();
                                TLMUtils.doLog("j");
                                string[] subtempArray = kv[1].Split(SUBCOMMA.ToCharArray());
                                TLMUtils.doLog("k");
                                foreach (string j in subtempArray)
                                {
                                    TLMUtils.doLog("l");
                                    if (uint.TryParse(j, out uint value))
                                    {
                                        TLMUtils.doLog("m");
                                        saida[key].Add(value);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            TLMUtils.doLog("n");
            return saida;
        }



        private static string getConfigStringFromDictionary(Dictionary<ushort, List<uint>> d)
        {
            var temp = new List<string>();
            foreach (ushort i in d.Keys)
            {
                temp.Add(i + SEPARATOR + string.Join(SUBCOMMA, d[i].Select(x => x.ToString()).ToArray()));
            }
            return string.Join(COMMA, temp.ToArray());
        }

        private static void saveConfigForTransportType(ref TransportSystemDefinition tsd, Dictionary<ushort, List<uint>> value)
        {
            string depotList = getConfigStringFromDictionary(value);
            TLMConfigWarehouse.SetCurrentConfigString(TLMConfigWarehouse.getConfigDepotPrefix(ref tsd), depotList);
            cached_lists[tsd] = value;
        }
        #endregion

        #region Cache Manager
        private static void cleanCache() => cached_lists = new Dictionary<TransportSystemDefinition, Dictionary<ushort, List<uint>>>();

        private static Dictionary<ushort, List<uint>> getConfigForTransportType(ref TransportSystemDefinition tsd)
        {
            if (!cached_lists.ContainsKey(tsd))
            {
                string depotList = TLMConfigWarehouse.GetCurrentConfigString(TLMConfigWarehouse.getConfigDepotPrefix(ref tsd));
                if (TransportLinesManagerMod.DebugMode)
                {
                    TLMUtils.doLog("getConfigForTransportType STRING FOR {0}: {1}", tsd.ToString(), depotList);
                }

                cached_lists[tsd] = getDictionaryFromConfigString(depotList, ref tsd);
            }
            return cached_lists[tsd];
        }

        #endregion

        #region CRUD depots
        public static void addPrefixToDepot(ushort buildingID, uint prefix, bool secondary)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.From(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                Dictionary<ushort, List<uint>> dic = getConfigForTransportType(ref tsd);
                if (!dic.ContainsKey(buildingID))
                {
                    dic[buildingID] = new List<uint>(defaultPrefixList);
                }
                dic[buildingID].Add(prefix);

                saveConfigForTransportType(ref tsd, dic);
            }
        }

        public static void addAllPrefixesToDepot(ushort buildingID, bool secondary)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.From(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                Dictionary<ushort, List<uint>> dic = getConfigForTransportType(ref tsd);
                if (dic.ContainsKey(buildingID))
                {
                    dic.Remove(buildingID);
                }
                saveConfigForTransportType(ref tsd, dic);
            }
        }

        public static void removePrefixFromDepot(ushort buildingID, uint prefix, bool secondary)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.From(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                Dictionary<ushort, List<uint>> dic = getConfigForTransportType(ref tsd);
                if (!dic.ContainsKey(buildingID))
                {
                    dic[buildingID] = new List<uint>(defaultPrefixList);
                }
                dic[buildingID].Remove(prefix);
                saveConfigForTransportType(ref tsd, dic);
            }
        }

        public static void removeAllPrefixesFromDepot(ushort buildingID, bool secondary)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.From(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                Dictionary<ushort, List<uint>> dic = getConfigForTransportType(ref tsd);
                dic[buildingID] = new List<uint>();
                saveConfigForTransportType(ref tsd, dic);
            }
        }

        #endregion

        #region Utility methods

        public static List<ushort> getAllDepotsFromCity(ref TransportSystemDefinition tsd)
        {
            var saida = new List<ushort>();
            BuildingManager bm = Singleton<BuildingManager>.instance;
            FastList<ushort> buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                if (bm.m_buildings.m_buffer[i].Info.GetAI() is DepotAI buildingAI && tsd.IsFromSystem(buildingAI))
                {
                    saida.Add(i);
                }
            }
            return saida;
        }
        public static List<ushort> getAllDepotsFromCity()
        {
            var saida = new List<ushort>();
            BuildingManager bm = Singleton<BuildingManager>.instance;
            FastList<ushort> buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                PrefabAI prefAI = bm.m_buildings.m_buffer[i].Info.GetAI();
                if ((prefAI is DepotAI buildingAI && buildingAI.m_maxVehicleCount > 0) || (prefAI is ShelterAI))
                {
                    saida.Add(i);
                }
            }
            return saida;
        }

        public static List<uint> getPrefixesServedByDepot(ushort buildingID, bool secondary)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            FastList<ushort> buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            if (bm.m_buildings.m_buffer[buildingID].Info.GetAI() is DepotAI buildingAI)
            {
                var tsd = TransportSystemDefinition.From(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo);
                if (tsd != default)
                {
                    Dictionary<ushort, List<uint>> dic = getConfigForTransportType(ref tsd);
                    if (!dic.ContainsKey(buildingID))
                    {
                        return defaultPrefixList;
                    }
                    return dic[buildingID];
                }
            }
            return null;
        }

        public static List<ushort> getAllowedDepotsForPrefix(ref TransportSystemDefinition tsd, uint prefix)
        {
            Dictionary<ushort, List<uint>> dic = getConfigForTransportType(ref tsd);
            List<ushort> saida = getAllDepotsFromCity(ref tsd);
            foreach (ushort i in dic.Keys)
            {
                if (TransportLinesManagerMod.DebugMode)
                {
                    TLMUtils.doLog("dic[i]: {{{0}}} ||  prefix = {1} || contains = {2}  ", string.Join(",", dic[i].Select(x => x.ToString()).ToArray()), prefix, dic[i].Contains(prefix));
                }

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
            Interfaces.IAssetSelectorExtension extension = TLMLineUtils.getExtensionFromTransportLine(lineId);
            VehicleInfo randomInfo = extension?.GetAModel(lineId);
            return randomInfo;
        }

        public static void setRandomBuildingByPrefix(ref TransportSystemDefinition tsd, uint prefix, ref ushort currentId)
        {
            List<ushort> allowedDepots = getAllowedDepotsForPrefix(ref tsd, prefix);
            if (allowedDepots.Count == 0 || allowedDepots.Contains(currentId))
            {
                if (TransportLinesManagerMod.DebugMode)
                {
                    TLMUtils.doLog("allowedDepots.Count --{0}-- == 0||  allowedDepots.Contains({1}): --{2}--  ", allowedDepots.Count, currentId, string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()));
                }

                return;
            }
            var r = new Randomizer(new System.Random().Next());
            if (TransportLinesManagerMod.DebugMode)
            {
                TLMUtils.doLog("DEPOT POSSIBLE VALUES FOR {2} PREFIX {1}: {0} ", string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()), prefix, tsd);
            }

            currentId = allowedDepots[r.Int32(0, allowedDepots.Count - 1)];
            if (TransportLinesManagerMod.DebugMode)
            {
                TLMUtils.doLog("DEPOT FOR {2} PREFIX {1}: {0} ", currentId, prefix, tsd);
            }
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

            TLMUtils.doLog("m_info {0} | m_transportInfo {1} | Line: {2}", m_info.name, m_transportInfo.name, offer.TransportLine);


            if (reason == m_transportInfo.m_vehicleReason || (__instance.m_secondaryTransportInfo != null && reason == __instance.m_secondaryTransportInfo.m_vehicleReason))
            {
                VehicleInfo randomVehicleInfo = null;
                var tsd = TransportSystemDefinition.From(__instance.m_transportInfo);

                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[offer.TransportLine];
                TransportInfo.TransportType t = tl.Info.m_transportType;

                if (TLMLineUtils.hasPrefix(ref tl))
                {
                    setRandomBuildingByPrefix(ref tsd, tl.m_lineNumber / 1000u, ref buildingID);
                }
                else
                {
                    setRandomBuildingByPrefix(ref tsd, 0, ref buildingID);
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
                        randomVehicleInfo.m_vehicleAI.SetSource(vehicleID, ref vehicles.m_buffer[vehicleID], buildingID);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(vehicleID, ref vehicles.m_buffer[vehicleID], reason, offer);
                    }
                    return false;
                }
            }
            return true;

        }

        // CommonBuildingAI
        private MethodInfo CalculateSpawnPosition = typeof(DepotAI).GetMethod("CalculateSpawnPosition", allFlags);

        public Redirector RedirectorInstance => new Redirector();
        #endregion

        #region Hooking

        public void Awake()
        {
            TLMUtils.doLog("Loading Depot Hooks!");
            RedirectorInstance.AddRedirect(typeof(DepotAI).GetMethod("StartTransfer", allFlags), typeof(TLMDepotAI).GetMethod("StartTransfer", allFlags));
        }

        #endregion

    }
}
