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
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    class TLMBusModifyRedirects : BasicTransportExtension<BusAI>
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

        #region Hooks for BusAI

        public void SetTransportLine(ushort vehicleID, ref Vehicle data, ushort transportLine)
        {
            var t = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine];
            TLMUtils.doLog("SetTransportLine! Prefab id: {0} ({4}), For line: {1} {2} ({3})", data.Info.m_prefabDataIndex, t.Info.m_transportType, t.m_lineNumber, transportLine, data.Info.name);
            this.RemoveLine(vehicleID, ref data);

            data.m_transportLine = transportLine;
            if (transportLine != 0)
            {
                if (t.Info.m_transportType == TransportInfo.TransportType.Bus && TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.BUS_PREFIX) != (int)ModoNomenclatura.Nenhum)
                {
                    uint prefix = t.m_lineNumber / 1000u;

                    List<string> assetsList = TLMBusModifyRedirects.instance.getAssetListForPrefix(prefix);


                    if (!assetsList.Contains(data.Info.name))
                    {
                        var randomInfo = instance.getRandomModel(prefix);
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
        private void RemoveLine(ushort vehicleID, ref Vehicle data)        {            TLMUtils.doLog("RemoveLine??? WHYYYYYYY!?");        }
        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 v4, Vector3 v3) { TLMUtils.doLog("StartPathFind??? WHYYYYYYY!?"); return false; }
        public void OnCreated(ILoading loading)
        {
            TLMUtils.doLog("TLMBusRedirects Criado!");
        }



        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            ExtraVehiclesStats.OnVehicleStop(vehicleID, vehicleData);
            //ORIGINAL
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    Vector3 endPos = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding].CalculateSidewalkPosition();
                    return this.StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, endPos);
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)vehicleData.m_targetBuilding].m_position;
                return this.StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, position);
            }
            return false;

        }

        //info.m_vehicleAI.GetBufferStatus(firstVehicle, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)firstVehicle], out text, out fill, out cap);

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
            TLMUtils.doLog("Loading Low/High Bus Hooks!");
            AddRedirect(typeof(BusAI), typeof(TLMBusModifyRedirects).GetMethod("SetTransportLine", allFlags), ref redirects);
            AddRedirect(typeof(BusAI), typeof(TLMBusModifyRedirects).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null), ref redirects);
            AddRedirect(typeof(TLMBusModifyRedirects), typeof(CarAI).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(Vector3) }, null), ref redirects);
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
