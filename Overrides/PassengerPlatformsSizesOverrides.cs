using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{
    public class PassengerPlatformsSizesOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; } = new Redirector();

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading PassengerPlatformsSizesOverrides Overrides");
            foreach (var type in new Type[]
            {
                typeof(PassengerTrainAI),
                typeof(PassengerBlimpAI),
                typeof(PassengerFerryAI),
                typeof(PassengerHelicopterAI),
                typeof(PassengerPlaneAI),
                typeof(PassengerShipAI),
                typeof(TramAI),
                typeof(TrolleybusAI),
                typeof(BusAI),
                typeof(CableCarAI),
            })
            {
                LogUtils.DoLog($"Overriding {type}");
                RedirectorInstance.AddRedirect(type.GetMethod("LoadPassengers", RedirectorUtils.allFlags), null, null, GetType().GetMethod("TransplieRaiseHalfGrid", RedirectorUtils.allFlags));
                var chkPas = type.GetMethod("CheckPassengers", RedirectorUtils.allFlags);
                if (chkPas != null)
                {
                    RedirectorInstance.AddRedirect(chkPas, null, null, GetType().GetMethod("TransplieRaiseHalfGrid", RedirectorUtils.allFlags));
                }
            }

        }
        #endregion
        private static IEnumerable<CodeInstruction> TransplieRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }

            LogUtils.PrintMethodIL(inst);
            return inst;
        }
    }
}