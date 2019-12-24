using ColossalFramework;
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
                        var def = TransportSystemDefinition.from(lineId);
                        extension = def.GetTransportExtension();
                    }

                    TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
                    var modelList = extension.GetAssetList(idx);
                    VehicleManager vm = Singleton<VehicleManager>.instance;
                    VehicleInfo info = vm.m_vehicles.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].GetVehicle(0)].Info;

                    if (TransportLinesManagerMod.DebugMode) TLMUtils.doLog("removeAllUnwantedVehicles: models found: {0}", modelList == null ? "?!?" : modelList.Count.ToString());

                    if (modelList.Count > 0)
                    {
                        Dictionary<ushort, VehicleInfo> vehiclesToRemove = new Dictionary<ushort, VehicleInfo>();
                        for (int i = 0; i < tl.CountVehicles(lineId); i++)
                        {
                            var vehicle = tl.GetVehicle(i);
                            if (vehicle != 0)
                            {
                                VehicleInfo info2 = vm.m_vehicles.m_buffer[vehicle].Info;
                                if (!modelList.Contains(info2.name))
                                {
                                    vehiclesToRemove[vehicle] = info2;
                                }
                            }
                        }
                        foreach (var item in vehiclesToRemove)
                        {
                            item.Value.m_vehicleAI.SetTransportLine(item.Key, ref vm.m_vehicles.m_buffer[item.Key], 0);
                        }
                    }
                }
            }
        }
    }
}
