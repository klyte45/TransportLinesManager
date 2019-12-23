using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.MapDrawer;
using Klyte.TransportLinesManager.OptionsMenu;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;

[assembly: AssemblyVersion("12.99.99.99")]
namespace Klyte.TransportLinesManager 
{
    public class TransportLinesManagerMod : BasicIUserMod<TransportLinesManagerMod, TLMController, TLMPublicTransportManagementPanel>
    {

        public TransportLinesManagerMod() => Construct();


        public override string SimpleName => "Transport Lines Manager";
        public override string Description => "Allows to customize and manage your public transport systems.";
        public override bool UseGroup9 => false;

        public override void DoErrorLog(string fmt, params object[] args) => TLMUtils.doErrorLog(fmt, args);

        public override void DoLog(string fmt, params object[] args) => TLMUtils.doLog(fmt, args);

        public override void LoadSettings()
        {
        }

        public override void TopSettingsUI(UIHelperExtension helper) => TLMConfigOptions.instance.GenerateOptionsMenu(helper);

        internal void PopulateGroup9(UIHelperExtension helper) => CreateGroup9(helper);

        public override void Group9SettingsUI(UIHelperExtension group9)
        {
            group9.AddButton(Locale.Get("K45_TLM_DRAW_CITY_MAP"), TLMMapDrawer.drawCityMap);
            group9.AddButton("Open generated map folder", () => ColossalFramework.Utils.OpenInFileBrowser(TLMController.exportedMapsFolder));
        }

        private readonly SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
        private readonly SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
        private readonly SavedBool m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
        private readonly SavedBool m_showDistanceInLinearMap = new SavedBool("TLMshowDistanceInLinearMap", Settings.gameSettingsFile, true, true);

        public static bool showNearLinesPlop
        {
            get => Instance.m_savedShowNearLinesInCityServicesWorldInfoPanel.value;
            set => Instance.m_savedShowNearLinesInCityServicesWorldInfoPanel.value = value;
        }
        public static bool showNearLinesGrow
        {
            get => Instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value;
            set => Instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = value;
        }
        public static bool overrideWorldInfoPanelLine
        {
            get => Instance.m_savedOverrideDefaultLineInfoPanel.value;
            set => Instance.m_savedOverrideDefaultLineInfoPanel.value = value;
        }
        public static bool showDistanceLinearMap
        {
            get => Instance.m_showDistanceInLinearMap.value;
            set => Instance.m_showDistanceInLinearMap.value = value;
        }

        public override string IconName => "K45_TLM_Icon";
    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
