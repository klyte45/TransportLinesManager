using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Klyte.TransportLinesManager.Extensors
{
    class TLMTrainModifyRedirects : Redirector
    {
        private static TLMTrainModifyRedirects _instance;
        public static TLMTrainModifyRedirects instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMTrainModifyRedirects();
                }
                return _instance;
            }
        }

        private const string SEPARATOR = "∂";
        private static List<string> trainsAssetsList;

        private static Dictionary<string, int> capacities = new Dictionary<string, int>();

        private static List<string> cached_tramAssetsList;
        private static List<string> tramAssetsList
        {
            get
            {
                if (cached_tramAssetsList == null)
                {
                    cached_tramAssetsList = TransportLinesManagerMod.tramAssets.value.Split(SEPARATOR.ToCharArray()).ToList();
                }
                return cached_tramAssetsList;
            }
            set
            {
                TransportLinesManagerMod.tramAssets.value = string.Join(SEPARATOR, value.Select(x => x.ToString()).ToArray());
                cached_tramAssetsList = value;
            }
        }


        private static List<string> cached_bulletTrainAssetsList;
        private static List<string> bulletTrainAssetsList
        {
            get
            {
                if (cached_bulletTrainAssetsList == null)
                {
                    cached_bulletTrainAssetsList = TransportLinesManagerMod.bulletTrainAssets.value.Split(SEPARATOR.ToCharArray()).ToList();
                }
                return cached_bulletTrainAssetsList;
            }
            set
            {
                TransportLinesManagerMod.bulletTrainAssets.value = string.Join(SEPARATOR, value.Select(x => x.ToString()).ToArray());
                cached_bulletTrainAssetsList = value;
            }
        }


        private static List<string> cached_inactiveTrainAssetsList;
        private static List<string> inactiveTrainAssetsList
        {
            get
            {
                if (cached_inactiveTrainAssetsList == null)
                {
                    cached_inactiveTrainAssetsList = TransportLinesManagerMod.inactiveTrains.value.Split(SEPARATOR.ToCharArray()).ToList();
                }
                return cached_inactiveTrainAssetsList;
            }
            set
            {
                TransportLinesManagerMod.inactiveTrains.value = string.Join(SEPARATOR, value.Select(x => x.ToString()).ToArray());
                cached_inactiveTrainAssetsList = value;
            }
        }

        public static Dictionary<string, string> getTramAssetDictionary()
        {
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            return tramAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", capacities[x], x));
        }

        public static Dictionary<string, string> getBulletTrainAssetDictionary()
        {
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            return bulletTrainAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", capacities[x], x));
        }

        public static Dictionary<string, string> getInactiveTrainAssetDictionary()
        {
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            return inactiveTrainAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", capacities[x], x));

        }

        public static Dictionary<string, string> getTrainAssetDictionary()
        {
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            return trainsAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", capacities[x], x));
        }

        private static void removeFromAllLists(string assetId)
        {
            TLMUtils.doLog("removingFromAllLists: {0}", assetId);
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = tramAssetsList;
            if (tramAssetsList.Contains(assetId))
            {
                items.Remove(assetId);
                tramAssetsList = items;
            }

            items = bulletTrainAssetsList;
            if (items.Contains(assetId))
            {
                items.Remove(assetId);
                bulletTrainAssetsList = items;
            }

            items = inactiveTrainAssetsList;
            if (items.Contains(assetId))
            {
                items.Remove(assetId);
                inactiveTrainAssetsList = items;
            }
        }

        public static List<string> addAssetToTrainList(string assetId)
        {
            removeFromAllLists(assetId);
            readVehicles();
            return trainsAssetsList;
        }

        public static List<string> addAssetToTramList(string assetId)
        {
            TLMUtils.doLog("addAssetToTramList: {0}", assetId);
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = tramAssetsList;
            if (!items.Contains(assetId))
            {
                removeFromAllLists(assetId);
                items.Add(assetId);
                tramAssetsList = items;
                readVehicles();
            }
            return items;
        }

        public static List<string> addAssetToBulletTrainList(string assetId)
        {
            TLMUtils.doLog("addAssetToBulletTrainList: {0}", assetId);
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = bulletTrainAssetsList;
            if (!items.Contains(assetId))
            {
                removeFromAllLists(assetId);
                items.Add(assetId);
                bulletTrainAssetsList = items;
                readVehicles();
            }
            return items;
        }

        public static List<string> addAssetToInactiveTrainList(string assetId)
        {
            TLMUtils.doLog("addAssetToInactiveTrainList: {0}", assetId);
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = inactiveTrainAssetsList;
            if (!items.Contains(assetId))
            {
                removeFromAllLists(assetId);
                items.Add(assetId);
                inactiveTrainAssetsList = items;
                readVehicles();
            }
            return items;
        }

        public static bool isTramLine(ushort line)
        {
            return TLMConfigWarehouse.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.TRAM_LINES_IDS).Contains((int)line);
        }
        public static bool isBulletTrainLine(ushort line)
        {
            return TLMConfigWarehouse.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_LINES_IDS).Contains((int)line);
        }
        public static bool isTrainLine(ushort line)
        {
            return !(isTramLine(line) ^ isBulletTrainLine(line));
        }

        public static bool isTramAvaliable()
        {
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            return tramAssetsList.Count != 0 && trainsAssetsList.Count != 0;
        }

        public static bool isBulletTrainAvaliable()
        {
            if (trainsAssetsList == null)
            {
                readVehicles();
            }
            return bulletTrainAssetsList.Count != 0 && trainsAssetsList.Count != 0;
        }

        public static VehicleInfo getRandomTram()
        {
            if (tramAssetsList.Count == 0) return null;
            Randomizer r = new Randomizer(new Random().Next());
            return PrefabCollection<VehicleInfo>.FindLoaded(tramAssetsList[r.Int32(0, tramAssetsList.Count - 1)]);
        }

        public static VehicleInfo getRandomTrain()
        {
            if (trainsAssetsList.Count == 0) return null;
            var avaliableTrains = trainsAssetsList;
            Randomizer r = new Randomizer(new Random().Next());
            return PrefabCollection<VehicleInfo>.FindLoaded(avaliableTrains[r.Int32(0, avaliableTrains.Count - 1)]);
        }

        public static VehicleInfo getRandomBulletTrain()
        {
            if (bulletTrainAssetsList.Count == 0) return null;
            var avaliableTrains = bulletTrainAssetsList;
            Randomizer r = new Randomizer(new Random().Next());
            return PrefabCollection<VehicleInfo>.FindLoaded(avaliableTrains[r.Int32(0, bulletTrainAssetsList.Count - 1)]);
        }

        public static void forceReload()
        {
            trainsAssetsList = null;
        }

        private static void readVehicles()
        {
            cached_tramAssetsList = null;
            cached_bulletTrainAssetsList = null;
            cached_inactiveTrainAssetsList = null;

            capacities = new Dictionary<string, int>();

            trainsAssetsList = new List<string>();
            var trailerTrainsList = new List<string>();
            uint num = 0u;

            TLMUtils.doLog("PrefabCount: {0} ({1})", PrefabCollection<VehicleInfo>.PrefabCount(), PrefabCollection<VehicleInfo>.LoadedCount());
            while ((ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount()))
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && prefab.GetAI().GetType() == typeof(PassengerTrainAI))
                {
                    trainsAssetsList.Add(prefab.name);
                    capacities[prefab.name] = (prefab.GetAI() as PassengerTrainAI).m_passengerCapacity;
                    if (prefab.m_trailers != null && prefab.m_trailers.Length > 0)
                    {
                        foreach (var trailer in prefab.m_trailers)
                        {
                            if (trailer.m_info.name != prefab.name)
                            {
                                trailerTrainsList.Add(trailer.m_info.name);
                                capacities[prefab.name] += (trailer.m_info.GetAI() as PassengerTrainAI).m_passengerCapacity;
                            }
                        }
                    }
                }
                num += 1u;
            }
            TLMUtils.doLog("Train List: {0}", string.Join(",", trainsAssetsList.ToArray()));
            TLMUtils.doLog("trailerTrainsList: {0}", string.Join(",", trailerTrainsList.ToArray()));
            TLMUtils.doLog("PrefabCount: {0} ({1})", PrefabCollection<VehicleInfo>.PrefabCount(), PrefabCollection<VehicleInfo>.LoadedCount());
            trainsAssetsList.RemoveAll(x => trailerTrainsList.Contains(x) || x.Contains(".Trailer"));
            var tramListTemp = new List<string>();
            var bulletListTemp = new List<string>();
            var inactiveListTemp = new List<string>();
            foreach (string id in tramAssetsList)
            {
                if (trainsAssetsList.Contains(id))
                {
                    tramListTemp.Add(id);
                    trainsAssetsList.Remove(id);
                }
            }
            tramAssetsList = tramListTemp;
            foreach (string id in bulletTrainAssetsList)
            {
                if (trainsAssetsList.Contains(id))
                {
                    bulletListTemp.Add(id);
                    trainsAssetsList.Remove(id);
                }
            }
            bulletTrainAssetsList = bulletListTemp;
            foreach (string id in inactiveTrainAssetsList)
            {
                if (trainsAssetsList.Contains(id))
                {
                    trainsAssetsList.Remove(id);
                    inactiveListTemp.Add(id);
                }
            }
            inactiveTrainAssetsList = inactiveListTemp;
        }

        public TLMTrainModifyRedirects()
        {
        }

        #region Hooks for PassengerTrainAI

        public void SetTransportLine(ushort vehicleID, ref Vehicle data, ushort transportLine)
        {
            var t = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine];
            TLMUtils.doLog("SetTransportLine! Prefab id: {0} ({4}), For line: {1} {2} ({3})", data.Info.m_prefabDataIndex, t.Info.m_transportType, t.m_lineNumber, transportLine, data.Info.name);
            TLMUtils.doLog("SetTransportLine! isTramAvaliable? {0}", isTramAvaliable());
            this.RemoveLine(vehicleID, ref data);

            data.m_transportLine = transportLine;
            if (transportLine != 0)
            {
                if (t.Info.m_transportType == TransportInfo.TransportType.Train)
                {
                    var transportType = TLMConfigWarehouse.getConfigIndexForLine(transportLine);

                    if (transportType == TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG && isTramAvaliable())
                    {
                        if (!tramAssetsList.Contains(data.Info.name))
                        {
                            var randomInfo = getRandomTram();
                            if (randomInfo != null)
                            {
                                data.Info = randomInfo;
                            }
                        }
                    }
                    else if (transportType == TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_CONFIG && isBulletTrainAvaliable())
                    {
                        if (!bulletTrainAssetsList.Contains(data.Info.name))
                        {
                            var randomInfo = getRandomBulletTrain();
                            if (randomInfo != null)
                            {
                                data.Info = randomInfo;
                            }
                        }
                    }
                    else
                    {
                        if (!trainsAssetsList.Contains(data.Info.name))
                        {
                            var randomInfo = getRandomTrain();
                            if (randomInfo != null)
                            {
                                data.Info = randomInfo;
                            }
                        }
                    }
                }
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)transportLine].AddVehicle(vehicleID, ref data, true);
            }
            else
            {
                data.m_flags |= Vehicle.Flags.GoingBack;
            }
            if (!this.StartPathFind(vehicleID, ref data))
            {
                data.Unspawn(vehicleID);
            }
        }
        private void RemoveLine(ushort vehicleID, ref Vehicle data)
        {

            TLMUtils.doLog("RemoveLine??? WHYYYYYYY!?");
            if (data.m_transportLine != 0)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[(int)data.m_transportLine].RemoveVehicle(vehicleID, ref data);
                data.m_transportLine = 0;
            }
        }
        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData) { TLMUtils.doLog("StartPathFind??? WHYYYYYYY!?"); return false; }

        public void OnCreated(ILoading loading)
        {
            TLMUtils.doLog("TLMTramRedirects Criado!");
        }
        #endregion

        //#region Hooks for PublicTransportVehicleWorldInfoPanel
        //private void IconChanged(UIComponent comp, string text)
        //{

        //    PublicTransportVehicleWorldInfoPanel ptvwip = Singleton<PublicTransportVehicleWorldInfoPanel>.instance;
        //    ushort lineId = m_instance.TransportLine;
        //    UISprite iconSprite = ptvwip.gameObject.transform.Find("VehicleType").GetComponent<UISprite>();
        //    TLMUtils.doLog("lineId == {0}", lineId);
        //}
        //InstanceID m_instance;
        //#endregion

        public void OnReleased()
        {
        }

        #region Hooking
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();



        public void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            TLMUtils.doLog("Loading Tram Hooks!");
            AddRedirect(typeof(PassengerTrainAI), typeof(TLMTrainModifyRedirects).GetMethod("SetTransportLine", allFlags), ref redirects);
            AddRedirect(typeof(TLMTrainModifyRedirects), typeof(PassengerTrainAI).GetMethod("StartPathFind", allFlags), ref redirects); ;
            AddRedirect(typeof(TLMTrainModifyRedirects), typeof(PassengerTrainAI).GetMethod("RemoveLine", allFlags), ref redirects);
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
