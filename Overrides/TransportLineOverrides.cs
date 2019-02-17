using ColossalFramework;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{
    class TransportLineOverrides : Redirector<TransportLineOverrides>
    {
        #region Hooking

        private static bool preventDefault()
        {
            return false;
        }

        public override void AwakeBody()
        {
            MethodInfo preventDefault = typeof(TransportLineOverrides).GetMethod("preventDefault", allFlags);

            #region Automation Hooks
            MethodInfo doAutomation = typeof(TransportLineOverrides).GetMethod("doAutomation", allFlags);
            MethodInfo preDoAutomation = typeof(TransportLineOverrides).GetMethod("preDoAutomation", allFlags);

            TLMUtils.doLog("Loading AutoColor & AutoName Hook");
            AddRedirect(typeof(TransportLine).GetMethod("AddStop", allFlags), preDoAutomation, doAutomation);
            #endregion


            #region Ticket Override Hooks
            MethodInfo GetTicketPricePre = typeof(TransportLineOverrides).GetMethod("GetTicketPricePre", allFlags);

            TLMUtils.doLog("Loading Ticket Override Hooks");
            AddRedirect(typeof(PassengerPlaneAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            AddRedirect(typeof(PassengerShipAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            AddRedirect(typeof(TramAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            AddRedirect(typeof(PassengerTrainAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            AddRedirect(typeof(PassengerBlimpAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            AddRedirect(typeof(PassengerFerryAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            AddRedirect(typeof(BusAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            AddRedirect(typeof(CableCarAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre);
            //AddRedirect(typeof(TaxiAI).GetMethod("GetTicketPrice", allFlags), GetTicketPricePre); // Waiting fix
            #endregion
            #region Bus Spawn Unbunching
            MethodInfo BusUnbuncher = typeof(TransportLineOverrides).GetMethod("BusUnbuncher", allFlags);
            AddRedirect(typeof(TransportLine).GetMethod("AddVehicle", allFlags), null, BusUnbuncher);
            #endregion



            #region Color Override Hooks
            MethodInfo GetColorFor = typeof(TransportLineOverrides).GetMethod("GetColorFor", allFlags);

            TLMUtils.doLog("Loading Color Override Hooks");
            AddRedirect(typeof(PassengerPlaneAI).GetMethod("GetColor", allFlags), null, GetColorFor);
            AddRedirect(typeof(PassengerShipAI).GetMethod("GetColor", allFlags), null, GetColorFor);
            AddRedirect(typeof(TramAI).GetMethod("GetColor", allFlags), null, GetColorFor);
            AddRedirect(typeof(PassengerTrainAI).GetMethod("GetColor", allFlags), null, GetColorFor);
            AddRedirect(typeof(PassengerBlimpAI).GetMethod("GetColor", allFlags), null, GetColorFor);
            AddRedirect(typeof(PassengerFerryAI).GetMethod("GetColor", allFlags), null, GetColorFor);
            AddRedirect(typeof(BusAI).GetMethod("GetColor", allFlags), null, GetColorFor);
            #endregion

            #region Budget Override Hooks

            MethodInfo SimulationStepPre = typeof(TransportLineOverrides).GetMethod("SimulationStepPre", allFlags);
            TLMUtils.doLog("Loading SimulationStepPre Hook");
            AddRedirect(typeof(TransportLine).GetMethod("SimulationStep", allFlags), SimulationStepPre);
            #endregion

        }
        #endregion

        private static Dictionary<uint, Tuple<ushort, ushort>> m_counterIdx = new Dictionary<uint, Tuple<ushort, ushort>>();


        #region On Line Create

        public static void preDoAutomation(ushort lineID, ref TransportLine.Flags __state)
        {
            __state = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags;
        }

        public static void doAutomation(ushort lineID, TransportLine.Flags __state)
        {
            TLMUtils.doLog("OLD: " + __state + " ||| NEW: " + Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags);
            if (lineID > 0 && (__state & TransportLine.Flags.Complete) == TransportLine.Flags.None && (__state & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None
                    && (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & (TransportLine.Flags.Temporary)) == TransportLine.Flags.None)
                {
                    if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
                    {
                        TLMController.instance.AutoColor(lineID);
                    }
                    if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED))
                    {
                        TLMController.instance.AutoName(lineID);
                    }
                    TLMController.instance.LineCreationToolbox.incrementNumber();
                    TLMTransportLineExtension.instance.SafeCleanEntry(lineID);
                }
            }
            if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None &&
                (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.CustomColor) != TransportLine.Flags.None
                )
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~TransportLine.Flags.CustomColor;
            }

        }
        #endregion

        #region Budget Override
        public static bool SimulationStepPre(ushort lineID)
        {
            try
            {
                TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
                bool activeDayNightManagedByTLM = TLMLineUtils.isPerHourBudget(lineID);
                if (t.m_lineNumber != 0 && t.m_stops != 0)
                {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget = (ushort)(TLMLineUtils.getBudgetMultiplierLine(lineID) * 100);
                }

                unchecked
                {
                    TLMLineUtils.getLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID], out bool day, out bool night);
                    bool isNight = Singleton<SimulationManager>.instance.m_isNightTime;
                    bool zeroed = false;
                    if ((activeDayNightManagedByTLM || (isNight && night) || (!isNight && day)) && Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget == 0)
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags |= (TransportLine.Flags)TLMTransportLineFlags.ZERO_BUDGET_CURRENT;
                        zeroed = true;
                    }
                    else
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~(TransportLine.Flags)TLMTransportLineFlags.ZERO_BUDGET_CURRENT;
                    }
                    TLMUtils.doLog("activeDayNightManagedByTLM = {0}; zeroed = {1}", activeDayNightManagedByTLM, zeroed);
                    if (activeDayNightManagedByTLM)
                    {
                        if (zeroed)
                        {
                            TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID], false, false);
                        }
                        else
                        {
                            TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID], true, true);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog("Error processing budget for line: {0}\n{1}", lineID, e);
            }
            return true;
        }


        #endregion

        #region Ticket Override
        public static bool GetTicketPricePre(ushort vehicleID, ref Vehicle vehicleData, ref int __result)
        {
            return ticketPriceForPrefix(vehicleID, ref vehicleData, ref __result);
        }

        private static bool ticketPriceForPrefix(ushort vehicleID, ref Vehicle vehicleData, ref int __result)
        {
            var def = TransportSystemDefinition.from(vehicleData.Info);

            if (def == default(TransportSystemDefinition))
            {
                return true;
            }

            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(vehicleData.m_targetPos3);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
            DistrictPolicies.Event @event = instance.m_districts.m_buffer[(int)district].m_eventPolicies & Singleton<EventManager>.instance.GetEventPolicyMask();
            float multiplier;
            if (vehicleData.Info.m_class.m_subService == ItemClass.SubService.PublicTransportTours)
            {
                multiplier = 1;
            }
            else
            {
                if ((servicePolicies & DistrictPolicies.Services.FreeTransport) != DistrictPolicies.Services.None)
                {
                    __result = 0;
                    return false;
                }
                if ((@event & DistrictPolicies.Event.ComeOneComeAll) != DistrictPolicies.Event.None)
                {
                    __result = 0;
                    return false;
                }
                if ((servicePolicies & DistrictPolicies.Services.HighTicketPrices) != DistrictPolicies.Services.None)
                {
                    District[] expr_114_cp_0 = instance.m_districts.m_buffer;
                    byte expr_114_cp_1 = district;
                    expr_114_cp_0[(int)expr_114_cp_1].m_servicePoliciesEffect = (expr_114_cp_0[(int)expr_114_cp_1].m_servicePoliciesEffect | DistrictPolicies.Services.HighTicketPrices);
                    multiplier = 5f / 4f;
                }
                else
                {
                    multiplier = 1;
                }
            }
            if (vehicleData.m_transportLine == 0)
            {
                __result = (int)(def.GetTransportExtension().GetDefaultTicketPrice(0) * multiplier);
                return false;
            }
            else
            {
                uint prefixValue = 0;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(vehicleData.m_transportLine))
                {
                    prefixValue = TLMTransportLineExtension.instance.GetTicketPrice(vehicleData.m_transportLine);
                }
                if (prefixValue == 0)
                {
                    prefixValue = def.GetTransportExtension().GetTicketPrice(TLMLineUtils.getPrefix(vehicleData.m_transportLine));
                }

                __result = (int)(multiplier * prefixValue);
                return false;
            }
        }
        #endregion

        #region Color Override
        private static void GetColorFor(ushort vehicleID, ref Vehicle data, ref Color __result, InfoManager.InfoMode infoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.TrafficRoutes:
                    return;
                case InfoManager.InfoMode.Underground:
                case InfoManager.InfoMode.ParkMaintenance:
                    IL_1D:
                    if (infoMode != InfoManager.InfoMode.None)
                    {
                        if (infoMode != InfoManager.InfoMode.Transport)
                        {
                            if (infoMode != InfoManager.InfoMode.EscapeRoutes)
                            {
                                return;
                            }
                            goto IL_1G;
                        }
                        else
                        {
                            return;
                            //goto IL_1G;
                        }
                    }
                    IL_1G:
                    ushort transportLine = data.m_transportLine;
                    if (transportLine != 0)
                    {
                        var tsd = TransportSystemDefinition.getDefinitionForLine(transportLine);
                        if (tsd.transportType == TransportInfo.TransportType.EvacuationBus)
                        {
                            return;
                        }

                        var ext = tsd.GetTransportExtension();
                        var prefix = TLMLineUtils.getPrefix(transportLine);

                        if (ext.IsUsingColorForModel(prefix))
                        {
                            __result = ext.GetColor(prefix);
                        }
                        else
                        {
                            __result = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)transportLine].GetColor();
                        }
                    }
                    return;
                case InfoManager.InfoMode.Tours:
                    ushort transportLine2 = data.m_transportLine;
                    var tsd2 = TransportSystemDefinition.getDefinitionForLine(transportLine2);
                    if (tsd2.transportType != TransportInfo.TransportType.TouristBus)
                    {
                        return;
                    }
                    goto IL_1G;
                case InfoManager.InfoMode.Tourism:
                    return;
                default:
                    goto IL_1D;
            }
        }
        #endregion

        #region Bus Spawn Unbunching
        private static void BusUnbuncher(ushort vehicleID, ref Vehicle data, bool findTargetStop)
        {
            if (findTargetStop && (data.Info.GetAI() is BusAI || data.Info.GetAI() is TramAI) && data.m_transportLine > 0)
            {
                TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[data.m_transportLine];
                data.m_targetBuilding = t.GetStop(SimulationManager.instance.m_randomizer.Int32((uint)t.CountStops(data.m_transportLine)));
            }
        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            TLMUtils.doLog(text, param);
        }

    }
}
