using ColossalFramework.Math;
using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Klyte.Commons.Extensions.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    public class OutsideConnectionOverrides : Redirector, IRedirectable
    {
        #region Overrides

        private static readonly TransferManager.TransferReason[] m_managedReasons = new TransferManager.TransferReason[]   {
                TransferManager.TransferReason.DummyCar,
                TransferManager.TransferReason.DummyTrain,
                TransferManager.TransferReason.DummyShip,
                TransferManager.TransferReason.DummyPlane
            };

        public static VehicleInfo GetRandomVehicle(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, TransferManager.TransferReason reason)
        {
            if (m_managedReasons.Contains(reason))
            {
                LogUtils.DoLog("START TRANSFER OutsideConnectionAI!!!!!!!!");
                return TryGetRandomVehicle(vm, ref r, service, subService, level);
            }
            return vm.GetRandomVehicleInfo(ref r, service, subService, level);

        }
        private static VehicleInfo TryGetRandomVehicleStation(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {

            LogUtils.DoLog("START TRANSFER StationAI!!!!!!!!");
            return TryGetRandomVehicle(vm, ref r, service, subService, level);
        }

        private static VehicleInfo TryGetRandomVehicle(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {
            var tsd = TransportSystemDefinition.FromOutsideConnection(subService, level);
            if (!(tsd is null))
            {
                VehicleInfo randomVehicleInfo = tsd.GetTransportExtension().GetAModel(0);
                if (randomVehicleInfo != null)
                {
                    return randomVehicleInfo;
                }
            }
            return vm.GetRandomVehicleInfo(ref r, service, subService, level);
        }



        private static IEnumerable<CodeInstruction> TranspileStationAISpawnVehicle(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo TryGetRandomVehicle = typeof(OutsideConnectionOverrides).GetMethod("TryGetRandomVehicleStation", allFlags);

            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt
                    && inst[i].operand is MethodInfo mi
                    && mi.Name == "GetRandomVehicleInfo")
                {
                    inst.RemoveAt(i);
                    inst.InsertRange(i, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Call, TryGetRandomVehicle),

                    });
                }
            }

            LogUtils.PrintMethodIL(inst, true);
            return inst;
        }
        private static IEnumerable<CodeInstruction> TranspileStartConnectionTransferImpl(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo GetRandomVehicle = typeof(OutsideConnectionOverrides).GetMethod("GetRandomVehicle", allFlags);

            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt
                    && inst[i].operand is MethodInfo mi
                    && mi.Name == "GetRandomVehicleInfo")
                {
                    inst.RemoveAt(i);
                    inst.InsertRange(i, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Call, GetRandomVehicle),

                    });
                }
            }

            LogUtils.PrintMethodIL(inst, true);
            return inst;
        }

        public Redirector RedirectorInstance => this;

        #endregion

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading OutsideConnectionAI Hooks!");
            RedirectorInstance.AddRedirect(typeof(OutsideConnectionAI).GetMethod("StartConnectionTransferImpl", allFlags), null, null, typeof(OutsideConnectionOverrides).GetMethod("TranspileStartConnectionTransferImpl", allFlags));
            RedirectorInstance.AddRedirect(typeof(TransportStationAI).GetMethod("CreateOutgoingVehicle", allFlags), null, null, typeof(OutsideConnectionOverrides).GetMethod("TranspileStationAISpawnVehicle", allFlags));
            RedirectorInstance.AddRedirect(typeof(TransportStationAI).GetMethod("CreateIncomingVehicle", allFlags), null, null, typeof(OutsideConnectionOverrides).GetMethod("TranspileStationAISpawnVehicle", allFlags));
        }

        #endregion

    }
}
