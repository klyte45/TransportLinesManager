using ColossalFramework;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensions;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{
    public class CityServiceWorldInfoPanelOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; set; }


        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading CityServiceWorldInfoPanel Overrides");
            RedirectorInstance = KlyteMonoUtils.CreateElement<Redirector>(transform);
            #region Net Manager Hooks
            MethodInfo OnNodeChanged = GetType().GetMethod("OnGoToLines", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(CityServiceWorldInfoPanel).GetMethod("OnLinesOverviewClicked", RedirectorUtils.allFlags), OnNodeChanged);
            #endregion

        }
        #endregion


        private static bool OnGoToLines(CityServiceWorldInfoPanel __instance)
        {
            InstanceID m_InstanceID = (InstanceID)typeof(CityServiceWorldInfoPanel).GetField("m_InstanceID", RedirectorUtils.allFlags).GetValue(__instance);
            if (m_InstanceID.Type != InstanceType.Building || m_InstanceID.Building == 0)
            {
                return false;
            }
            ushort building = m_InstanceID.Building;
            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info;
            if (info != null && info.m_buildingAI is TransportStationAI stationAI)
            {
                TLMPanel.Instance.OpenAt(TransportSystemDefinition.From(stationAI));
            }
            return false;
        }

    }
}
