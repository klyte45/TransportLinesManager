using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    internal class PublicTransportWorldInfoPanelOverrides : MonoBehaviour,IRedirectable
    {
        public Redirector RedirectorInstance => new Redirector();

        private static bool OpenDetailPanel(int idx)
        {
            TransportSystemDefinition def;
            UiCategoryTab cat = UiCategoryTab.LineListing;
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

            TLMPanel.Instance?.OpenAt(cat, def);
            return false;
        }

        public static bool OpenDetailPanelDefaultTab()
        {
            OpenDetailPanel(0);
            return false;
        }

        #region Hooking

        public static bool preventDefault() => false;

        public void Awake()
        {

            MethodInfo OpenDetailPanel = typeof(PublicTransportWorldInfoPanelOverrides).GetMethod("OpenDetailPanel", allFlags);
            MethodInfo OpenDetailPanelDefaultTab = typeof(PublicTransportWorldInfoPanelOverrides).GetMethod("OpenDetailPanelDefaultTab", allFlags);

            TLMUtils.doLog("Loading PublicTransportInfoViewPanel Hooks!");
            RedirectorInstance.AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), OpenDetailPanel);
            RedirectorInstance.AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanelDefaultTab", allFlags), OpenDetailPanelDefaultTab);
            RedirectorInstance.AddRedirect(typeof(ToursInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), OpenDetailPanel);

            MethodInfo preventDefault = typeof(PublicTransportWorldInfoPanelOverrides).GetMethod("preventDefault", allFlags);
            MethodInfo from3 = typeof(PublicTransportLineInfo).GetMethod("RefreshData", allFlags);
            TLMUtils.doErrorLog("Muting PublicTransportLineInfo: {0} ({1}=>{2}})", typeof(PublicTransportLineInfo), from3, preventDefault);
            RedirectorInstance.AddRedirect(from3, preventDefault);
        }

        private string getOrdinal(int nth)
        {
            if (nth % 10 == 1 && nth % 100 != 11)
            {
                return "st";
            }
            else if (nth % 10 == 2 && nth % 100 != 12)
            {
                return "nd";
            }
            else if (nth % 10 == 3 && nth % 100 != 13)
            {
                return "rd";
            }
            else
            {
                return "th";
            }
        }


        #endregion
    }

}
