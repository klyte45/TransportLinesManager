using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using System.Collections.Generic;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMUtils
    {
        public static readonly TransferManager.TransferReason[] defaultAllowedVehicleTypes = {
            TransferManager.TransferReason.Blimp ,
            TransferManager.TransferReason.CableCar ,
            TransferManager.TransferReason.Ferry ,
            TransferManager.TransferReason.MetroTrain ,
            TransferManager.TransferReason.Monorail ,
            TransferManager.TransferReason.PassengerTrain ,
            TransferManager.TransferReason.PassengerPlane ,
            TransferManager.TransferReason.PassengerShip ,
            TransferManager.TransferReason.Tram ,
            TransferManager.TransferReason.Bus
        };



        #region Naming Utils

        #endregion

        #region Building Utils

        #endregion
        #region Logging
        public static void DoLog(string format, params object[] args) => LogUtils.DoLog(format, args);
        public static void DoErrorLog(string format, params object[] args) => LogUtils.DoErrorLog(format, args);
        #endregion

        internal static List<string> LoadBasicAssets(ref TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();

            TLMUtils.DoLog("LoadBasicAssets: pre prefab read");
            for (uint num = 0u; num < (ulong)PrefabCollection<VehicleInfo>.PrefabCount(); num += 1u)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && definition.IsFromSystem(prefab) && !VehicleUtils.IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
    }


}

