using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    class TLMHumanAI : Redirector
    {
        protected virtual bool EnterVehicle(ushort instanceID, ref CitizenInstance citizenData)
        {
            citizenData.m_flags &= ~CitizenInstance.Flags.EnteringVehicle;
            citizenData.Unspawn(instanceID);
            uint citizen = citizenData.m_citizen;
            if (citizen != 0u)
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                ushort num = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_vehicle;
                if (num != 0)
                {
                    num = instance.m_vehicles.m_buffer[(int)num].GetFirstVehicle(num);
                }
                if (num != 0)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[(int)num].Info;
                    int ticketPrice = info.m_vehicleAI.GetTicketPrice(num, ref instance.m_vehicles.m_buffer[(int)num]);
                    if (ticketPrice != 0)
                    {
                        Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, ticketPrice, info.m_class);
                    }
                }
            }
            return false;
        }

        #region Hooking
        private static Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();



        public void EnableHooks()
        {
            if (redirects.Count != 0)
            {
                DisableHooks();
            }
            TLMUtils.doLog("Loading HumanAI Hooks!");
            AddRedirect(typeof(HumanAI), typeof(TLMHumanAI).GetMethod("EnterVehicle", allFlags), ref redirects);
          }

        public void DisableHooks()
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
