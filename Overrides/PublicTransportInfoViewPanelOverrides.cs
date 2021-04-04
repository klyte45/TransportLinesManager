using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Extensions.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    internal class PublicTransportInfoViewPanelOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance => new Redirector();

        public static TLMLineCreationToolbox Toolbox { get; private set; }

        private static bool OpenDetailPanel(int idx)
        {
            TransportSystemDefinition def;
            switch (idx)
            {
                case 0:
                    def = TransportSystemDefinition.BUS;
                    break;
                case 1:
                    def = TransportSystemDefinition.TRAM;
                    break;
                case 2:
                    def = TransportSystemDefinition.METRO;
                    break;
                case 3:
                    def = TransportSystemDefinition.TRAIN;
                    break;
                case 4:
                    def = TransportSystemDefinition.FERRY;
                    break;
                case 5:
                    def = TransportSystemDefinition.BLIMP;
                    break;
                case 6:
                    def = TransportSystemDefinition.MONORAIL;
                    break;
                case 8:
                    def = TransportSystemDefinition.TOUR_PED;
                    break;
                case 9:
                    def = TransportSystemDefinition.TOUR_BUS;
                    break;
                default:
                    def = TransportSystemDefinition.BUS;
                    break;
            }

            TLMPanel.Instance?.OpenAt(def);
            return false;
        }

        public static bool OpenDetailPanelDefaultTab()
        {
            OpenDetailPanel(0);
            return false;
        }

        public static void AfterAwake(PublicTransportInfoViewPanel __instance) => Toolbox = __instance.gameObject.AddComponent<TLMLineCreationToolbox>();

        #region Hooking


        public void Awake()
        {

            MethodInfo OpenDetailPanel = typeof(PublicTransportInfoViewPanelOverrides).GetMethod("OpenDetailPanel", allFlags);
            MethodInfo OpenDetailPanelDefaultTab = typeof(PublicTransportInfoViewPanelOverrides).GetMethod("OpenDetailPanelDefaultTab", allFlags);

            LogUtils.DoLog($"Loading PublicTransportInfoViewPanel Hooks!");
            RedirectorInstance.AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), OpenDetailPanel);
            RedirectorInstance.AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanelDefaultTab", allFlags), OpenDetailPanelDefaultTab);
            RedirectorInstance.AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("Start", RedirectorUtils.allFlags), typeof(PublicTransportInfoViewPanelOverrides).GetMethod("AfterAwake", RedirectorUtils.allFlags));
            RedirectorInstance.AddRedirect(typeof(ToursInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), OpenDetailPanel);

            MethodInfo preventDefault = typeof(Redirector).GetMethod("PreventDefault", allFlags);
            MethodInfo from3 = typeof(PublicTransportLineInfo).GetMethod("RefreshData", allFlags);
            LogUtils.DoLog("Muting PublicTransportLineInfo: {0} ({1}=>{2})", typeof(PublicTransportLineInfo), from3, preventDefault);
            RedirectorInstance.AddRedirect(from3, preventDefault);
        }



        #endregion
    }

}
