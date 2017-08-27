using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Klyte.TransportLinesManager.Extensors.VehicleAIExt
{
    class TLMTicketOverride : Redirector
    {
        public int GetTicketPrice(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_transportLine == 0)
            {
                return ticketPriceForPrefix(vehicleID, ref vehicleData);
            }
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(vehicleData.m_targetPos3);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
            DistrictPolicies.Event @event = instance.m_districts.m_buffer[(int)district].m_eventPolicies & Singleton<EventManager>.instance.GetEventPolicyMask();

            if ((servicePolicies & DistrictPolicies.Services.FreeTransport) != DistrictPolicies.Services.None)
            {
                District[] expr_80_cp_0 = instance.m_districts.m_buffer;
                byte expr_80_cp_1 = district;
                expr_80_cp_0[(int)expr_80_cp_1].m_servicePoliciesEffect = (expr_80_cp_0[(int)expr_80_cp_1].m_servicePoliciesEffect | DistrictPolicies.Services.FreeTransport);
                return 0;
            }
            if ((@event & DistrictPolicies.Event.ComeOneComeAll) != DistrictPolicies.Event.None)
            {
                District[] expr_AC_cp_0 = instance.m_districts.m_buffer;
                byte expr_AC_cp_1 = district;
                expr_AC_cp_0[(int)expr_AC_cp_1].m_eventPoliciesEffect = (expr_AC_cp_0[(int)expr_AC_cp_1].m_eventPoliciesEffect | DistrictPolicies.Event.ComeOneComeAll);
                return 0;
            }
            if ((servicePolicies & DistrictPolicies.Services.HighTicketPrices) != DistrictPolicies.Services.None)
            {
                District[] expr_D8_cp_0 = instance.m_districts.m_buffer;
                byte expr_D8_cp_1 = district;
                expr_D8_cp_0[(int)expr_D8_cp_1].m_servicePoliciesEffect = (expr_D8_cp_0[(int)expr_D8_cp_1].m_servicePoliciesEffect | DistrictPolicies.Services.HighTicketPrices);
                return (int)ticketPriceForPrefix(vehicleID, ref vehicleData) * 5 / 4;
            }

            return ticketPriceForPrefix(vehicleID, ref vehicleData);
        }

        private static int ticketPriceForPrefix(ushort vehicleID, ref Vehicle vehicleData)
        {
            var def = TransportSystemDefinition.from(vehicleData.Info.m_class.m_subService, vehicleData.Info.m_vehicleType);

            if (def == default(TransportSystemDefinition))
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("NULL TSysDef! {0}+{1}+{2}", vehicleData.Info.GetAI().GetType(), vehicleData.Info.m_class.m_subService, vehicleData.Info.m_vehicleType);
                return 109;
            }
            if (def.vehicleType == VehicleInfo.VehicleType.Ferry)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(vehicleData.m_targetPos3);
                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
                if ((servicePolicies & DistrictPolicies.Services.PreferFerries) != DistrictPolicies.Services.None)
                {
                    District[] expr_6E_cp_0 = instance.m_districts.m_buffer;
                    byte expr_6E_cp_1 = district;
                    expr_6E_cp_0[(int)expr_6E_cp_1].m_servicePoliciesEffect = (expr_6E_cp_0[(int)expr_6E_cp_1].m_servicePoliciesEffect | DistrictPolicies.Services.PreferFerries);
                    return 0;
                }
            }
            if (vehicleData.m_transportLine == 0)
            {
                var value = (int)BasicTransportExtensionSingleton.instance(def).getDefaultTicketPrice();
                return value;
            }
            else {
                var value = (int)(BasicTransportExtensionSingleton.instance(def).getTicketPrice((uint)vehicleData.m_transportLine));
            
                return value;
            }
        }

        #region Hooking
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();

        public static void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Ticket Price Hooks!");
            AddRedirect(typeof(BusAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(PassengerBlimpAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(PassengerFerryAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(PassengerPlaneAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(PassengerShipAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(PassengerTrainAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(TramAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            //AddRedirect(typeof(CableCarAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            //AddRedirect(typeof(MonorailAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
        }

        public static void DisableHooks()
        {
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            redirects.Clear();
        }
        #endregion
    }
}
