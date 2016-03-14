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
    class TLMAirplaneModifyRedirects : Redirector
    {
        private static TLMAirplaneModifyRedirects _instance;
        public static TLMAirplaneModifyRedirects instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TLMAirplaneModifyRedirects();
                }
                return _instance;
            }
        }

        public TLMAirplaneModifyRedirects()
        {
        }

        #region Hooks for PassengerShipAI

        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 v4, Vector3 v3) { TLMUtils.doLog("StartPathFind??? WHYYYYYYY!?"); return false; }
        public void OnCreated(ILoading loading)
        {
            TLMUtils.doLog("TLMShipRedirects Criado!");
        }

        public Color GetColorBase(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode)
        {
            return Color.black;
        }

        

        // PassengerShipAI
        public Color GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode)
        {
            if (infoMode == InfoManager.InfoMode.Transport)
            {
                ushort transportLine = data.m_transportLine;
                if (transportLine != 0)
                {
                    return Singleton<TransportManager>.instance.m_lines.m_buffer[(int)transportLine].GetColor();
                }
                return Singleton<TransportManager>.instance.m_properties.m_transportColors[(int)TransportInfo.TransportType.Ship];
            }
            else
            {
                if (infoMode != InfoManager.InfoMode.Connections)
                {
                    ushort transportLine = data.m_transportLine;
                    if (transportLine != 0)
                    {
                        return Singleton<TransportManager>.instance.m_lines.m_buffer[(int)transportLine].GetColor();
                    }
                    return GetColorBase(vehicleID, ref data, infoMode);
                }
                InfoManager.SubInfoMode currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
                if (currentSubMode == InfoManager.SubInfoMode.WindPower && (data.m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != Vehicle.Flags.None)
                {
                    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor;
                }
                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
            }
        }




        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            ExtraVehiclesStats.OnVehicleStop(vehicleID, vehicleData);
            //ORIGINAL
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding].m_position;
                    return this.StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position);
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
                return this.StartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position3);
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
            TLMUtils.doLog("Loading Airplane Hooks!");
            AddRedirect(typeof(PassengerPlaneAI), typeof(TLMAirplaneModifyRedirects).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null), ref redirects);
            AddRedirect(typeof(TLMAirplaneModifyRedirects), typeof(AircraftAI).GetMethod("StartPathFind", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(Vector3) }, null), ref redirects);


            AddRedirect(typeof(PassengerPlaneAI), typeof(TLMAirplaneModifyRedirects).GetMethod("GetColor", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), ref redirects);
            AddRedirect(typeof(TLMAirplaneModifyRedirects), typeof(VehicleAI).GetMethod("GetColor", allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), ref redirects, "GetColorBase");


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
