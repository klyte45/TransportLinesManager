using ColossalFramework;
using ColossalFramework.Math;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.Extensions.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    public class TLMDepotAIOverrides : MonoBehaviour, IRedirectable
    {
        private static TLMDepotAIOverrides Instance { get; set; }


        #region Generation methods

        private static VehicleInfo DoModelDraw(ushort lineId)
        {
            Interfaces.IBasicExtension extension = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
            VehicleInfo randomInfo = extension.GetAModel(lineId);
            return randomInfo;
        }

        public void SetRandomBuilding(ref TransportSystemDefinition tsd, ushort lineId, ref ushort currentId)
        {
            Interfaces.IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
            List<ushort> allowedDepots = config.GetAllowedDepots(ref tsd, lineId);
            if (allowedDepots.Count == 0 || allowedDepots.Contains(currentId))
            {
                if (TransportLinesManagerMod.DebugMode)
                {
                    LogUtils.DoLog("allowedDepots.Count --{0}-- == 0||  allowedDepots.Contains({1}): --{2}--  ", allowedDepots.Count, currentId, string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()));
                }

                return;
            }
            var r = new Randomizer(new System.Random().Next());
            if (TransportLinesManagerMod.DebugMode)
            {
                LogUtils.DoLog("DEPOT POSSIBLE VALUES FOR {2} LINE {1}: {0} ", string.Join(",", allowedDepots.Select(x => x.ToString()).ToArray()), lineId, tsd);
            }

            currentId = allowedDepots[r.Int32(0, allowedDepots.Count - 1)];
            if (TransportLinesManagerMod.DebugMode)
            {
                LogUtils.DoLog("DEPOT FOR {2} LINE {1}: {0} ", currentId, lineId, tsd);
            }
        }
        #endregion

        #region Overrides

        private static readonly TransferManager.TransferReason[] m_managedReasons = new TransferManager.TransferReason[]   {
                TransferManager.TransferReason.Tram,
                TransferManager.TransferReason.PassengerTrain,
                TransferManager.TransferReason.PassengerShip,
                TransferManager.TransferReason.PassengerPlane,
                TransferManager.TransferReason.MetroTrain,
                TransferManager.TransferReason.Monorail,
                TransferManager.TransferReason.CableCar,
                TransferManager.TransferReason.Blimp,
                TransferManager.TransferReason.Bus,
                TransferManager.TransferReason.Ferry,
                TransferManager.TransferReason.Trolleybus,
                TransferManager.TransferReason.PassengerHelicopter
            };

        public static bool StartTransfer(DepotAI __instance, ushort buildingID, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (!m_managedReasons.Contains(reason) || offer.TransportLine == 0)
            {
                return true;
            }

            LogUtils.DoLog("START TRANSFER!!!!!!!!");
            TransportInfo m_transportInfo = __instance.m_transportInfo;
            BuildingInfo m_info = __instance.m_info;

            LogUtils.DoLog("m_info {0} | m_transportInfo {1} | Line: {2}", m_info.name, m_transportInfo.name, offer.TransportLine);


            if (reason == m_transportInfo.m_vehicleReason || (__instance.m_secondaryTransportInfo != null && reason == __instance.m_secondaryTransportInfo.m_vehicleReason))
            {
                var tsd = TransportSystemDefinition.From(__instance.m_transportInfo);

                Instance.SetRandomBuilding(ref tsd, offer.TransportLine, ref buildingID);

                LogUtils.DoLog("randomVehicleInfo");
                VehicleInfo randomVehicleInfo = DoModelDraw(offer.TransportLine);
                if (randomVehicleInfo == null)
                {
                    randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
                }
                if (randomVehicleInfo != null)
                {
                    LogUtils.DoLog("randomVehicleInfo != null");
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    __instance.CalculateSpawnPosition(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, out Vector3 position, out Vector3 vector);
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicleID, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, position, reason, false, true))
                    {
                        LogUtils.DoLog("CreatedVehicle!!!");
                        randomVehicleInfo.m_vehicleAI.SetSource(vehicleID, ref vehicles.m_buffer[vehicleID], buildingID);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(vehicleID, ref vehicles.m_buffer[vehicleID], reason, offer);
                    }
                    return false;
                }
            }
            return true;

        }

        // CommonBuildingAI

        public Redirector RedirectorInstance => new Redirector();

        #endregion

        #region Hooking

        public void Awake()
        {
            Instance = this;
            LogUtils.DoLog("Loading Depot Hooks!");
            RedirectorInstance.AddRedirect(typeof(DepotAI).GetMethod("StartTransfer", allFlags), typeof(TLMDepotAIOverrides).GetMethod("StartTransfer", allFlags));
        }

        #endregion

    }
}
