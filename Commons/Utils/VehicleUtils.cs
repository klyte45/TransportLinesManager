using ColossalFramework.Globalization;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;

namespace Klyte.Commons.Utils
{
    public class VehicleUtils
    {
        #region Vehicle Utils
        public static VehicleInfo GetRandomModel(List<string> assetList, out string selectedModel)
        {
            selectedModel = null;
            if (assetList.Count == 0)
            {
                return null;
            }

            var r = new Randomizer(new System.Random().Next());

            selectedModel = assetList[r.Int32(0, assetList.Count - 1)];

            VehicleInfo saida = PrefabCollection<VehicleInfo>.FindLoaded(selectedModel);
            if (saida == null)
            {
                LogUtils.DoLog("MODEL DOESN'T EXIST!");
                return null;
            }
            return saida;
        }
        public static int GetCapacity(VehicleInfo info) => GetCapacity(info, info.m_vehicleAI);
        public static int GetCapacity<AI>(VehicleInfo info, AI ai, bool noLoop = false) where AI : VehicleAI
        {
            if (info == null)
            {
                return -1;
            }

            int capacity = ReflectionUtils.GetGetFieldDelegate<AI, int>("m_passengerCapacity", ai.GetType())(ai);
            try
            {
                if (!noLoop)
                {
                    foreach (VehicleInfo.VehicleTrailer trailer in info.m_trailers)
                    {
                        capacity += trailer.m_info == null ? 0 : GetCapacity(trailer.m_info, trailer.m_info.m_vehicleAI, true);
                    }
                }
            }
            catch (Exception e)
            {
                LogUtils.DoLog("ERRO AO OBTER CAPACIDADE: [{0}] {1}", info, e.Message);
            }
            return capacity;
        }

        public static bool IsTrailer(PrefabInfo prefab)
        {
            string @unchecked = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
            return @unchecked.StartsWith("VEHICLE_TITLE") || @unchecked.StartsWith("Trailer");
        }


        #endregion
    }

}