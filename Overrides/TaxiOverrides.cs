using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{
    public class TaxiOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; } = new Redirector();

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading Taxi Overrides");
            #region Release Line Hooks

            RedirectorInstance.AddRedirect(typeof(TouristAI).GetMethod("GetVehicleInfo", RedirectorUtils.allFlags), null, null, typeof(TaxiOverrides).GetMethod("TranspileGetTaxiProbability_Tourist", RedirectorUtils.allFlags));
            RedirectorInstance.AddRedirect(typeof(ResidentAI).GetMethod("GetTaxiProbability", RedirectorUtils.allFlags), typeof(TaxiOverrides).GetMethod("PreGetTaxiProbability_Resident", RedirectorUtils.allFlags));
            #endregion

        }
        #endregion
        private static IEnumerable<CodeInstruction> TranspileGetTaxiProbability_Tourist(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            MethodInfo GetTaxiProbability_Tourist = typeof(TaxiOverrides).GetMethod("GetTaxiProbability_Tourist", RedirectorUtils.allFlags);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Call
                    && inst[i].operand is MethodInfo mi
                    && mi.Name == "GetTaxiProbability")
                {
                    inst[i].operand = GetTaxiProbability_Tourist;
                    inst.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
                    break;
                }
            }

            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static int GetTaxiProbability_Tourist(TouristAI ai, ref CitizenInstance citizenData) => GameAreaManager.instance.PointOutOfArea(citizenData.GetLastFramePosition()) ? 0 : 20;

        public static bool PreGetTaxiProbability_Resident(ref CitizenInstance citizenData, ref int __result)
        {
            if (GameAreaManager.instance.PointOutOfArea(citizenData.GetLastFramePosition()))
            {
                __result = 0;
                return false;
            }
            return true;
        }

    }
}
