using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.i18n;
using Klyte.TransportLinesManager.MapDrawer;
using Klyte.TransportLinesManager.OptionsMenu;
using Klyte.TransportLinesManager.TextureAtlas;
using Klyte.TransportLinesManager.Utils;
using System.IO;
using System.Reflection;

[assembly: AssemblyVersion("11.2.0.0")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : BasicIUserMod<TransportLinesManagerMod, TLMLocaleUtils, TLMResourceLoader, TLMController, TLMCommonTextureAtlas, TLMPublicTransportManagementPanel>
    {

        public TransportLinesManagerMod()
        {
            Construct();
        }

        protected override ModTab? Tab => ModTab.TransportLinesManager;

        public override string SimpleName => "Transport Lines Manager";
        public override string Description => "Allows to customize and manage your public transport systems. Requires Klyte Commons.";
        public override bool UseGroup9 => false;

        public override void doErrorLog(string fmt, params object[] args)
        {
            TLMUtils.doErrorLog(fmt, args);
        }

        public override void doLog(string fmt, params object[] args)
        {
            TLMUtils.doLog(fmt, args);
        }

        public override void LoadSettings()
        {
        }

        public override void TopSettingsUI(UIHelperExtension helper)
        {
            TLMConfigOptions.instance.GenerateOptionsMenu(helper);
        }

        internal void PopulateGroup9(UIHelperExtension helper)
        {
            CreateGroup9(helper);
        }

        public override void Group9SettingsUI(UIHelperExtension group9)
        {
            group9.AddButton(Locale.Get("TLM_DRAW_CITY_MAP"), TLMMapDrawer.drawCityMap);
            group9.AddButton("Open generated map folder", () => ColossalFramework.Utils.OpenInFileBrowser(exportedMapsFolder));
        }

        private SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
        private SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
        private SavedBool m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
        private SavedBool m_showDistanceInLinearMap = new SavedBool("TLMshowDistanceInLinearMap", Settings.gameSettingsFile, true, true);

        public static bool showNearLinesPlop
        {
            get { return instance.m_savedShowNearLinesInCityServicesWorldInfoPanel.value; }
            set { instance.m_savedShowNearLinesInCityServicesWorldInfoPanel.value = value; }
        }
        public static bool showNearLinesGrow
        {
            get { return instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value; }
            set { instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = value; }
        }
        public static bool overrideWorldInfoPanelLine
        {
            get { return instance.m_savedOverrideDefaultLineInfoPanel.value; }
            set { instance.m_savedOverrideDefaultLineInfoPanel.value = value; }
        }
        public static bool showDistanceLinearMap
        {
            get {
                return instance.m_showDistanceInLinearMap.value;
            }
            set {
                instance.m_showDistanceInLinearMap.value = value;
            }
        }

        public static readonly string FOLDER_NAME = "TransportLinesManager";
        public static readonly string FOLDER_PATH = TLMUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string PALETTE_SUBFOLDER_NAME = "ColorPalettes";
        public const string EXPORTED_MAPS_SUBFOLDER_NAME = "ExportedMaps";

        public static string palettesFolder => FOLDER_PATH + Path.DirectorySeparatorChar + PALETTE_SUBFOLDER_NAME;
        public static string configsFolder => TLMConfigWarehouse.CONFIG_PATH;
        public static string exportedMapsFolder => FOLDER_PATH + Path.DirectorySeparatorChar + EXPORTED_MAPS_SUBFOLDER_NAME;
    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
