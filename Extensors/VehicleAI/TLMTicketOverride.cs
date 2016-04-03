using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Klyte.TransportLinesManager.Extensors.VehicleAI
{
    class TLMTicketOverride : Redirector
    {
        public int GetTicketPrice(ushort vehicleID, ref Vehicle vehicleData)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(vehicleData.m_targetPos3);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
            if ((servicePolicies & DistrictPolicies.Services.FreeTransport) == DistrictPolicies.Services.None)
            {
                return (int)ticketPriceForPrefix(vehicleID, ref vehicleData);
            }
            District[] expr_53_cp_0 = instance.m_districts.m_buffer;
            byte expr_53_cp_1 = district;
            expr_53_cp_0[(int)expr_53_cp_1].m_servicePoliciesEffect = (expr_53_cp_0[(int)expr_53_cp_1].m_servicePoliciesEffect | DistrictPolicies.Services.FreeTransport);
            return 0;
        }

        private static int ticketPriceForPrefix(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_transportLine == 0)
            {
                return (int)BasicTransportExtensionSingleton.instance(vehicleData.Info.GetAI().GetType()).getDefaultTicketPrice();
            }
            else {
                return (int)BasicTransportExtensionSingleton.instance(vehicleData.Info.GetAI().GetType()).getTicketPrice((uint)vehicleData.m_transportLine / 1000u);
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
            AddRedirect(typeof(PassengerPlaneAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(PassengerShipAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(PassengerTrainAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
            AddRedirect(typeof(TramAI), typeof(TLMTicketOverride).GetMethod("GetTicketPrice", allFlags), ref redirects);
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
