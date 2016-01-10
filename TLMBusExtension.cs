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

namespace Klyte.TransportLinesManager
{
    class TLMBusModifyRedirects : Redirector
    {
        private static TLMBusModifyRedirects _instance;
        public static TLMBusModifyRedirects instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMBusModifyRedirects();
                }
                return _instance;
            }
        }

        private const string SEPARATOR = "∂";
        private static List<string> busAssetsList;

        private static List<string> cached_lowBusAssetsList;
        private static List<string> lowBusAssetsList
        {
            get
            {
                if (cached_lowBusAssetsList == null)
                {
                    cached_lowBusAssetsList = TransportLinesManagerMod.lowBusAssets.value.Split(SEPARATOR.ToCharArray()).ToList();
                }
                return cached_lowBusAssetsList;
            }
            set
            {
                TransportLinesManagerMod.lowBusAssets.value = string.Join(SEPARATOR, value.Select(x => x.ToString()).ToArray());
                cached_lowBusAssetsList = value;
            }
        }


        private static List<string> cached_highBusAssetsList;
        private static List<string> highBusAssetsList
        {
            get
            {
                if (cached_highBusAssetsList == null)
                {
                    cached_highBusAssetsList = TransportLinesManagerMod.highBusAssets.value.Split(SEPARATOR.ToCharArray()).ToList();
                }
                return cached_highBusAssetsList;
            }
            set
            {
                TransportLinesManagerMod.highBusAssets.value = string.Join(SEPARATOR, value.Select(x => x.ToString()).ToArray());
                cached_highBusAssetsList = value;
            }
        }


        private static List<string> cached_inactiveBusAssetsList;
        private static List<string> inactiveBusAssetsList
        {
            get
            {
                if (cached_inactiveBusAssetsList == null)
                {
                    cached_inactiveBusAssetsList = TransportLinesManagerMod.inactiveBuses.value.Split(SEPARATOR.ToCharArray()).ToList();
                }
                return cached_inactiveBusAssetsList;
            }
            set
            {
                TransportLinesManagerMod.inactiveBuses.value = string.Join(SEPARATOR, value.Select(x => x.ToString()).ToArray());
                cached_inactiveBusAssetsList = value;
            }
        }

        public static Dictionary<string, string> getLowBusAssetDictionary()
        {
            if (busAssetsList == null)
            {
                readVehicles();
            }
            return lowBusAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", (PrefabCollection<VehicleInfo>.FindLoaded(x).GetAI() as BusAI).m_passengerCapacity, x));
        }

        public static Dictionary<string, string> getHighBusAssetDictionary()
        {
            if (busAssetsList == null)
            {
                readVehicles();
            }
            return highBusAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", (PrefabCollection<VehicleInfo>.FindLoaded(x).GetAI() as BusAI).m_passengerCapacity, x));
        }

        public static Dictionary<string, string> getInactiveBusAssetDictionary()
        {
            if (busAssetsList == null)
            {
                readVehicles();
            }
            return inactiveBusAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", (PrefabCollection<VehicleInfo>.FindLoaded(x).GetAI() as BusAI).m_passengerCapacity, x));

        }

        public static Dictionary<string, string> getBusAssetDictionary()
        {
            if (busAssetsList == null)
            {
                readVehicles();
            }
            return busAssetsList.ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", (PrefabCollection<VehicleInfo>.FindLoaded(x).GetAI() as BusAI).m_passengerCapacity, x));
        }

        private static void removeFromAllLists(string assetId)
        {
            TLMUtils.doLog("removingFromAllLists: {0}", assetId);
            if (busAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = lowBusAssetsList;
            if (lowBusAssetsList.Contains(assetId))
            {
                items.Remove(assetId);
                lowBusAssetsList = items;
            }

            items = highBusAssetsList;
            if (items.Contains(assetId))
            {
                items.Remove(assetId);
                highBusAssetsList = items;
            }

            items = inactiveBusAssetsList;
            if (items.Contains(assetId))
            {
                items.Remove(assetId);
                inactiveBusAssetsList = items;
            }
        }

        public static List<string> addAssetToBusList(string assetId)
        {
            TLMUtils.doLog("addAssetToBusList: {0}", assetId);
            removeFromAllLists(assetId);
            readVehicles();
            return busAssetsList;
        }

        public static List<string> addAssetToLowBusList(string assetId)
        {
            TLMUtils.doLog("addAssetToLowBusList: {0}", assetId);
            if (busAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = lowBusAssetsList;
            if (!items.Contains(assetId))
            {
                removeFromAllLists(assetId);
                items.Add(assetId);
                lowBusAssetsList = items;
                readVehicles();
            }
            return items;
        }

        public static List<string> addAssetToHighBusList(string assetId)
        {
            TLMUtils.doLog("addAssetToHighBusList: {0}", assetId);
            if (busAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = highBusAssetsList;
            if (!items.Contains(assetId))
            {
                removeFromAllLists(assetId);
                items.Add(assetId);
                highBusAssetsList = items;
                readVehicles();
            }
            return items;
        }

        public static List<string> addAssetToInactiveBusList(string assetId)
        {
            TLMUtils.doLog("addAssetToInactiveBusList: {0}", assetId);
            if (busAssetsList == null)
            {
                readVehicles();
            }
            List<string> items = inactiveBusAssetsList;
            if (!items.Contains(assetId))
            {
                removeFromAllLists(assetId);
                items.Add(assetId);
                inactiveBusAssetsList = items;
                readVehicles();
            }
            return items;
        }

        public static bool isLowBusLine(ushort line)
        {
            return TLMConfigWarehouse.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.LOW_BUS_LINES_IDS).Contains((int)line);
        }
        public static bool isHighBusLine(ushort line)
        {
            return TLMConfigWarehouse.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_LINES_IDS).Contains((int)line);
        }
        public static bool isRegularBusLine(ushort line)
        {
            return !(isHighBusLine(line) ^ isLowBusLine(line));
        }


        public static bool isLowBusAvaliable()
        {
            if (busAssetsList == null)
            {
                readVehicles();
            }
            return lowBusAssetsList.Count != 0 && busAssetsList.Count != 0;
        }

        public static bool isHighBusAvaliable()
        {
            if (busAssetsList == null)
            {
                readVehicles();
            }
            return highBusAssetsList.Count != 0 && busAssetsList.Count != 0;
        }

        public static VehicleInfo getRandomLowBus()
        {
            if (lowBusAssetsList.Count == 0) return null;
            Randomizer r = new Randomizer(new Random().Next());
            TLMUtils.doLog("POSSIBLE VALUES FOR LOW BUS: {0}", string.Join(",", lowBusAssetsList.ToArray()));
            return PrefabCollection<VehicleInfo>.FindLoaded(lowBusAssetsList[r.Int32(0, lowBusAssetsList.Count - 1)]);
        }

        public static VehicleInfo getRandomBus()
        {
            if (busAssetsList.Count == 0) return null;
            Randomizer r = new Randomizer(new Random().Next());
            TLMUtils.doLog("POSSIBLE VALUES FOR REG BUS: {0}", string.Join(",", busAssetsList.ToArray()));
            return PrefabCollection<VehicleInfo>.FindLoaded(busAssetsList[r.Int32(0, busAssetsList.Count - 1)]);
        }

        public static VehicleInfo getRandomHighBus()
        {
            if (highBusAssetsList.Count == 0) return null;
            Randomizer r = new Randomizer(new Random().Next());
            TLMUtils.doLog("POSSIBLE VALUES FOR HIGH BUS: {0}", string.Join(",", highBusAssetsList.ToArray()));
            return PrefabCollection<VehicleInfo>.FindLoaded(highBusAssetsList[r.Int32(0, highBusAssetsList.Count - 1)]);
        }

        public static void forceReload()
        {
            busAssetsList = null;
        }

        private static void readVehicles()
        {
            cached_lowBusAssetsList = null;
            cached_highBusAssetsList = null;
            cached_inactiveBusAssetsList = null;

            busAssetsList = new List<string>();
            var trailerBusList = new List<string>();
            uint num = 0u;

            TLMUtils.doLog("PrefabCount: {0} ({1})", PrefabCollection<VehicleInfo>.PrefabCount(), PrefabCollection<VehicleInfo>.LoadedCount());
            while ((ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount()))
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && prefab.GetAI().GetType() == typeof(BusAI))
                {
                    busAssetsList.Add(prefab.name);
                    if (prefab.m_trailers != null && prefab.m_trailers.Length > 0)
                    {
                        foreach (var trailer in prefab.m_trailers)
                        {
                            if (trailer.m_info.name != prefab.name)
                            {
                                trailerBusList.Add(trailer.m_info.name);
                            }
                        }
                    }
                }
                num += 1u;
            }
            busAssetsList.RemoveAll(x => trailerBusList.Contains(x));
            var lowBusListTemp = new List<string>();
            var bulletListTemp = new List<string>();
            var inactiveListTemp = new List<string>();
            foreach (string id in lowBusAssetsList)
            {
                if (busAssetsList.Contains(id))
                {
                    lowBusListTemp.Add(id);
                    busAssetsList.Remove(id);
                }
            }
            lowBusAssetsList = lowBusListTemp;
            foreach (string id in highBusAssetsList)
            {
                if (busAssetsList.Contains(id))
                {
                    bulletListTemp.Add(id);
                    busAssetsList.Remove(id);
                }
            }
            highBusAssetsList = bulletListTemp;
            foreach (string id in inactiveBusAssetsList)
            {
                if (busAssetsList.Contains(id))
                {
                    busAssetsList.Remove(id);
                    inactiveListTemp.Add(id);
                }
            }
            inactiveBusAssetsList = inactiveListTemp;
        }

        public TLMBusModifyRedirects()
        {
        }

        #region Hooks for BusAI

        public void SetTransportLine(ushort vehicleID, ref Vehicle data, ushort transportLine)
        {
            var t = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine];
            TLMUtils.doLog("SetTransportLine! Prefab id: {0} ({4}), For line: {1} {2} ({3})", data.Info.m_prefabDataIndex, t.Info.m_transportType, t.m_lineNumber, transportLine, data.Info.name);
            TLMUtils.doLog("SetTransportLine! isLowBusAvaliable? {0}", isLowBusAvaliable());
            TLMUtils.doLog("SetTransportLine! isHighBusAvaliable? {0}", isHighBusAvaliable());
            this.RemoveLine(vehicleID, ref data);

            data.m_transportLine = transportLine;
            if (transportLine != 0)
            {
                if (t.Info.m_transportType == TransportInfo.TransportType.Bus)
                {
                    var transportType = TLMConfigWarehouse.getConfigIndexForLine(transportLine);

                    if (transportType == TLMConfigWarehouse.ConfigIndex.LOW_BUS_CONFIG && isLowBusAvaliable())
                    {
                        var randomInfo = getRandomLowBus();

                        if (randomInfo != null)
                        {
                            data.Info = randomInfo;
                        }
                    }
                    else if (transportType == TLMConfigWarehouse.ConfigIndex.HIGH_BUS_CONFIG && isHighBusAvaliable())
                    {
                        var randomInfo = getRandomHighBus();
                        if (randomInfo != null)
                        {
                            data.Info = randomInfo;
                        }
                    }
                    else
                    {
                        var randomInfo = getRandomBus();
                        if (randomInfo != null)
                        {
                            data.Info = randomInfo;
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
            TLMUtils.doLog("TLMLowBusRedirects Criado!");
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
            TLMUtils.doLog("Loading LowBus Hooks!");
            AddRedirect(typeof(BusAI), typeof(TLMBusModifyRedirects).GetMethod("SetTransportLine", allFlags), ref redirects);
            AddRedirect(typeof(TLMBusModifyRedirects), typeof(BusAI).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null), ref redirects); ;
            AddRedirect(typeof(TLMBusModifyRedirects), typeof(BusAI).GetMethod("RemoveLine", allFlags), ref redirects);
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
