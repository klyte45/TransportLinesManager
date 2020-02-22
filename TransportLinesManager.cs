using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.MapDrawer;
using Klyte.TransportLinesManager.OptionsMenu;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;

[assembly: AssemblyVersion("13.2.0.9999")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : BasicIUserMod<TransportLinesManagerMod, TLMController, TLMPanel>
    {

        public TransportLinesManagerMod() => Construct();


        public override string SimpleName => "Transport Lines Manager";
        public override string Description => "Allows to customize and manage your public transport systems.";
        public override bool UseGroup9 => false;

        public override void DoErrorLog(string fmt, params object[] args) => TLMUtils.doErrorLog(fmt, args);

        public override void DoLog(string fmt, params object[] args) => TLMUtils.doLog(fmt, args);

        public override void LoadSettings()
        {

            if (m_priorityRedirector == null)
            {
                m_priorityRedirector = KlyteMonoUtils.CreateElement<Redirector>(null, "UVM_PRIO");
                m_priorityRedirector.AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("Start", RedirectorUtils.allFlags), null, null, typeof(UVMPublicTransportWorldInfoPanel).GetMethod("TranspileStart", RedirectorUtils.allFlags));
            }
        }
        public override void TopSettingsUI(UIHelperExtension helper) => TLMConfigOptions.instance.GenerateOptionsMenu(helper);

        internal void PopulateGroup9(UIHelperExtension helper) => CreateGroup9(helper);

        public override void Group9SettingsUI(UIHelperExtension group9)
        {
            group9.AddButton(Locale.Get("K45_TLM_DRAW_CITY_MAP"), TLMMapDrawer.drawCityMap);
            group9.AddButton("Open generated map folder", () => ColossalFramework.Utils.OpenInFileBrowser(TLMController.exportedMapsFolder));
            group9.AddSpace(2);
            group9.AddButton(Locale.Get("K45_TLM_RELOAD_DEFAULT_CONFIGURATION"), () =>
            {
                TLMConfigWarehouse.GetConfig(null, null).ReloadFromDisk();
                TLMConfigOptions.instance.ReloadData();

            });
            if (IsCityLoaded)
            {
                group9.AddButton(Locale.Get("K45_TLM_EXPORT_CITY_CONFIG"), () =>
                {
                    string path = TLMConfigOptions.instance.currentLoadedCityConfig.Export();
                    ConfirmPanel.ShowModal(Name, string.Format(Locale.Get("K45_TLM_FILE_EXPORTED_TO_TEMPLATE"), path), (x, y) =>
                    {
                        if (y == 1)
                        {
                            ColossalFramework.Utils.OpenInFileBrowser(path);
                        }
                    });
                });
                group9.AddButton(Locale.Get("K45_TLM_IMPORT_CITY_CONFIG"), () =>
                {
                    ConfirmPanel.ShowModal(Name, string.Format(Locale.Get("K45_TLM_FILE_WILL_BE_IMPORTED_TEMPLATE"), TLMConfigOptions.instance.currentLoadedCityConfig.ThisPath), (x, y) =>
                    {
                        if (y == 1)
                        {
                            TLMConfigOptions.instance.currentLoadedCityConfig.ReloadFromDisk();
                            TLMConfigOptions.instance.ReloadData();
                        }
                    });
                });
                group9.AddButton(Locale.Get("K45_TLM_SAVE_CURRENT_CITY_CONFIG_AS_DEFAULT"), () =>
                {
                    TLMConfigOptions.instance.currentLoadedCityConfig.SaveAsDefault();
                    TLMConfigWarehouse.GetConfig(null, null).ReloadFromDisk();
                    TLMConfigOptions.instance.ReloadData();
                });
                group9.AddButton(Locale.Get("K45_TLM_LOAD_DEFAULT_AS_CURRENT_CITY_CONFIG"), () =>
                {
                    TLMConfigOptions.instance.currentLoadedCityConfig.LoadFromDefault();
                    TLMConfigWarehouse.GetConfig(null, null).ReloadFromDisk();
                    TLMConfigOptions.instance.ReloadData();
                });

            }
        }

        protected override void OnLevelLoadingInternal()
        {
            base.OnLevelLoadingInternal();
            TLMController.VerifyIfIsRealTimeEnabled();
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

        public static bool showDistanceLinearMap
        {
            get => Instance.m_showDistanceInLinearMap.value;
            set => Instance.m_showDistanceInLinearMap.value = value;
        }

        public override string IconName => "K45_TLM_Icon";

        private static Redirector m_priorityRedirector;

    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
