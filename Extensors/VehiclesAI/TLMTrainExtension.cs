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


        #region Hooks for PassengerTrainAI


        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            TLMUtils.doLog("StartPathFind!!!!!??? AEHOOO!");
            ExtraVehiclesStats.OnVehicleStop(vehicleID, vehicleData);
            //ORIGINAL
            if (vehicleData.m_leadingVehicle == 0)
            {
                Vector3 startPos;
                if ((vehicleData.m_flags & Vehicle.Flags.Reversed) != Vehicle.Flags.None)
                {
                    ushort lastVehicle = vehicleData.GetLastVehicle(vehicleID);
                    startPos = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)lastVehicle].m_targetPos0;
                }
                else
                {
                    startPos = vehicleData.m_targetPos0;
                }
                if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    if (vehicleData.m_sourceBuilding != 0)
                    {
                        Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding].m_position;
                        return this.StartPathFind(vehicleID, ref vehicleData, startPos, position);
                    }
                }
                else if ((vehicleData.m_flags & Vehicle.Flags.DummyTraffic) != Vehicle.Flags.None)
                {
                    if (vehicleData.m_targetBuilding != 0)
                    {
                        Vector3 position2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding].m_position;
                        return this.StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position2);
                    }
                }
                else if (vehicleData.m_targetBuilding != 0)
                {
                    Vector3 position3 = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)vehicleData.m_targetBuilding].m_position;
                    return this.StartPathFind(vehicleID, ref vehicleData, startPos, position3);
                }
            }
            return false;
        }

        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 v4, Vector3 v3) { TLMUtils.doLog("StartPathFind??? WHYYYYYYY!?"); return false; }


        public void OnCreated(ILoading loading)
        {
            TLMUtils.doLog("TLMSurfaceMetroRedirects Criado!");
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
            TLMUtils.doLog("Loading Train Hooks!");
            AddRedirect(typeof(PassengerTrainAI), typeof(TLMTrainModifyRedirects).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null), ref redirects);
            AddRedirect(typeof(TLMTrainModifyRedirects), typeof(TrainAI).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(Vector3) }, null), ref redirects);
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
