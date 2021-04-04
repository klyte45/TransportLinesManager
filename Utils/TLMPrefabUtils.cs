using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System.Collections.Generic;

namespace Klyte.TransportLinesManager.Utils
{
    public static class TLMPrefabUtils
    {

        internal static List<string> LoadBasicAssets(ref TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();

            LogUtils.DoLog("LoadBasicAssets: pre prefab read");
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

