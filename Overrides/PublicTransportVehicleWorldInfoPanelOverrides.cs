using ColossalFramework;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensions;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{
    public class PublicTransportVehicleWorldInfoPanelOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; set; }



        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading PublicTransportVehicleWorldInfoPanel Overrides");
            RedirectorInstance = KlyteMonoUtils.CreateElement<Redirector>(transform);
            MethodInfo OnNodeChanged = GetType().GetMethod("OnGoToLines", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(PublicTransportVehicleWorldInfoPanel).GetMethod("OnLinesOverviewClicked", RedirectorUtils.allFlags), OnNodeChanged);

        }
        #endregion


        private static bool OnGoToLines(PublicTransportVehicleWorldInfoPanel __instance)
        {
            InstanceID m_InstanceID = (InstanceID)typeof(PublicTransportVehicleWorldInfoPanel).GetField("m_InstanceID", RedirectorUtils.allFlags).GetValue(__instance);
            if (m_InstanceID.Type != InstanceType.Vehicle || m_InstanceID.Vehicle == 0)
            {
                return false;
            }
            ushort vehicle = m_InstanceID.Vehicle;
            ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle].GetFirstVehicle(vehicle);
            if (firstVehicle != 0)
            {
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle].Info;
                if (info != null)
                {
                    TLMPanel.Instance.OpenAt(TransportSystemDefinition.From(info));
                }
            }
            return false;
        }

    }
}
