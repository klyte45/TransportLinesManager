using ColossalFramework;
using ColossalFramework.Math;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;

namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    internal sealed class TLMTransportExtensionUtils
    {

        public static void RemoveAllUnwantedVehicles()
        {
            var randomizer = new Randomizer(SimulationManager.instance.m_timeOffsetTicks);
            for (ushort lineId = 1; lineId < Singleton<TransportManager>.instance.m_lines.m_size; lineId++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                {
                    uint idx;
                    IAssetSelectorExtension extension;
                    if (TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineId))
                    {
                        idx = lineId;
                        extension = TLMTransportLineExtension.Instance;
                    }
                    else
                    {
                        idx = TLMLineUtils.getPrefix(lineId);
                        var def = TransportSystemDefinition.From(lineId);
                        extension = def.GetTransportExtension();
                    }

                    TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
                    List<string> modelList = extension.GetAssetList(idx);
                    VehicleManager vm = Singleton<VehicleManager>.instance;

                    if (TransportLinesManagerMod.DebugMode)
                    {
                        TLMUtils.doLog("removeAllUnwantedVehicles: models found: {0}", modelList == null ? "?!?" : modelList.Count.ToString());
                    }

                    if (modelList.Count > 0)
                    {
                        var vehiclesToRemove = new Dictionary<ushort, VehicleInfo>();
                        for (int i = 0; i < tl.CountVehicles(lineId); i++)
                        {
                            ushort vehicle = tl.GetVehicle(i);
                            if (vehicle != 0)
                            {
                                VehicleInfo info2 = vm.m_vehicles.m_buffer[vehicle].Info;
                                if (!modelList.Contains(info2.name))
                                {
                                    vehiclesToRemove[vehicle] = info2;
                                }
                            }
                        }
                        foreach (KeyValuePair<ushort, VehicleInfo> item in vehiclesToRemove)
                        {
                            ReplaceVehicleModel(item.Key, extension.GetAModel(lineId));
                            // item.Value.m_vehicleAI.SetTransportLine(item.Key, ref vm.m_vehicles.m_buffer[item.Key], 0);
                        }
                    }
                }
            }
        }

        private static void ReplaceVehicleModel(ushort idx, VehicleInfo newInfo)
        {
            VehicleManager instance = VehicleManager.instance;
            Singleton<CitizenManager>.instance.ReleaseUnits(instance.m_vehicles.m_buffer[idx].m_citizenUnits);
            instance.m_vehicles.m_buffer[idx].Unspawn(idx);
            instance.m_vehicles.m_buffer[idx].Info = newInfo;
            instance.m_vehicles.m_buffer[idx].Spawn(idx);
            newInfo.m_vehicleAI.CreateVehicle(idx, ref instance.m_vehicles.m_buffer[idx]);
        }
    }
}
