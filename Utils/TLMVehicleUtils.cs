using ColossalFramework;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.Utils
{
    public class TLMVehicleUtils
    {

        private static int GetTotalUnitGroups(uint unitID)
        {
            int num = 0;
            while (unitID != 0u)
            {
                CitizenUnit citizenUnit = Singleton<CitizenManager>.instance.m_units.m_buffer[(int)((UIntPtr)unitID)];
                unitID = citizenUnit.m_nextUnit;
                num++;
            }
            return num;
        }
        public static IEnumerator UpdateCapacityUnitsFromTSD()
        {
            int count = 0;
            Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
            int i = 0;
            TransportSystemDefinition tsd;
            ITLMTransportTypeExtension ext;
            while (i < (long)((ulong)vehicles.m_size))
            {
                if ((vehicles.m_buffer[i].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned && (tsd = TransportSystemDefinition.From(vehicles.m_buffer[i].Info)) != default && (ext = tsd.GetTransportExtension()).IsCustomCapacity(vehicles.m_buffer[i].Info.name))
                {
                    int capacity = ext.GetCustomCapacity(vehicles.m_buffer[i].Info.name);
                    if (capacity != -1)
                    {
                        CitizenUnit[] units = Singleton<CitizenManager>.instance.m_units.m_buffer;
                        uint unit = vehicles.m_buffer[i].m_citizenUnits;
                        int currentUnitCount = GetTotalUnitGroups(unit);
                        int newUnitCount = Mathf.CeilToInt(capacity / 5f);
                        if (newUnitCount < currentUnitCount)
                        {
                            uint j = unit;
                            for (int k = 1; k < newUnitCount; k++)
                            {
                                j = units[(int)((UIntPtr)j)].m_nextUnit;
                            }
                            Singleton<CitizenManager>.instance.ReleaseUnits(units[(int)((UIntPtr)j)].m_nextUnit);
                            units[(int)((UIntPtr)j)].m_nextUnit = 0u;
                            count++;
                        }
                        else if (newUnitCount > currentUnitCount)
                        {
                            uint l = unit;
                            while (units[(int)((UIntPtr)l)].m_nextUnit != 0u)
                            {
                                l = units[(int)((UIntPtr)l)].m_nextUnit;
                            }
                            int newCapacity = capacity - currentUnitCount * 5;
                            if (!Singleton<CitizenManager>.instance.CreateUnits(out units[l].m_nextUnit, ref Singleton<SimulationManager>.instance.m_randomizer, 0, (ushort)i, 0, 0, 0, newCapacity, 0))
                            {
                                LogUtils.DoErrorLog("FAILED CREATING UNITS!!!!");
                            }
                            count++;
                        }
                    }
                }
                if (i % 256 == 255)
                {
                    yield return i % 256;
                }
                i++;
            }
            yield break;
        }

        public static void RemoveAllUnwantedVehicles()
        {
            VehicleManager vm = Singleton<VehicleManager>.instance;
            for (ushort lineId = 1; lineId < Singleton<TransportManager>.instance.m_lines.m_size; lineId++)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                {
                    IBasicExtension extension = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
                    ref TransportLine tl = ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
                    List<string> modelList = extension.GetAssetListForLine(lineId);

                    if (TransportLinesManagerMod.DebugMode)
                    {
                        TLMUtils.DoLog("removeAllUnwantedVehicles: models found: {0}", modelList == null ? "?!?" : modelList.Count.ToString());
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
                            if (item.Value.m_vehicleAI is BusAI)
                            {
                                VehicleUtils.ReplaceVehicleModel(item.Key, extension.GetAModel(lineId));
                            }
                            else
                            {
                                item.Value.m_vehicleAI.SetTransportLine(item.Key, ref vm.m_vehicles.m_buffer[item.Key], 0);
                            }
                        }
                    }
                }
            }
        }
    }

}
