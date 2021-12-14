
using Klyte.Commons.Extensions;
using Klyte.TransportLinesManager;
using Klyte.TransportLinesManager.Cache;
using Klyte.TransportLinesManager.Extensions;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMVehicleOverrides : Redirector, IRedirectable
    {
        public void Awake() => AddRedirect(typeof(PassengerTrainAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
        public static bool PreGetColorPassengerLineVehicle(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (data.m_leadingVehicle == 0 
                && infoMode == InfoManager.InfoMode.None 
                && data.m_transportLine == 0
                && data.m_custom != 0
                && TransportLinesManagerMod.Controller.BuildingLines[data.m_custom] is InnerBuildingLine cacheItem
                && cacheItem.LineDataObject is OutsideConnectionLineInfo ocli)
            {
                __result = ocli.LineColor;
                return false;
            }
            return true;
        }

    }
}