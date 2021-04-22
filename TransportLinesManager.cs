using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.MapDrawer;
using Klyte.TransportLinesManager.OptionsMenu;
using Klyte.TransportLinesManager.Xml;
using System.Collections.Generic;
using System.Reflection;
using static Klyte.Commons.UI.DefaultEditorUILib;

[assembly: AssemblyVersion("14.0.0.*")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : BasicIUserMod<TransportLinesManagerMod, TLMController, TLMPanel>
    {
        public override string SimpleName => "Transport Lines Manager";
        public override string Description => "Allows to customize and manage your public transport systems.";
        public override bool UseGroup9 => false;


        protected override List<ulong> IncompatibleModList { get; } = new List<ulong>()
        {
            TLMController.IPT2_MOD_ID
        };
        protected override List<string> IncompatibleDllModList { get; } = new List<string>()
        {
            "ImprovedPublicTransport2"
        };


        public override void TopSettingsUI(UIHelperExtension helper) => TLMConfigOptions.instance.GenerateOptionsMenu(helper);

        internal void PopulateGroup9(UIHelperExtension helper) => CreateGroup9(helper);

        public override void Group9SettingsUI(UIHelperExtension group9)
        {
            group9.AddButton(Locale.Get("K45_TLM_DRAW_CITY_MAP"), TLMMapDrawer.DrawCityMap);
            group9.AddButton("Open generated map folder", () => ColossalFramework.Utils.OpenInFileBrowser(TLMController.ExportedMapsFolder));
            group9.AddSpace(2);
            group9.AddButton(Locale.Get("K45_TLM_RELOAD_DEFAULT_CONFIGURATION"), () =>
            {
                TLMBaseConfigXML.ReloadGlobalFile();
                TLMConfigOptions.instance.ReloadData();
            });
            if (IsCityLoaded)
            {
                //group9.AddButton(Locale.Get("K45_TLM_EXPORT_CITY_CONFIG"), () =>
                //{
                //    string path = TLMConfigOptions.instance.currentLoadedCityConfig.Export();
                //    ConfirmPanel.ShowModal(Name, string.Format(Locale.Get("K45_TLM_FILE_EXPORTED_TO_TEMPLATE"), path), (x, y) =>
                //    {
                //        if (y == 1)
                //        {
                //            ColossalFramework.Utils.OpenInFileBrowser(path);
                //        }
                //    });
                //});
                //group9.AddButton(Locale.Get("K45_TLM_IMPORT_CITY_CONFIG"), () =>
                //{
                //    ConfirmPanel.ShowModal(Name, string.Format(Locale.Get("K45_TLM_FILE_WILL_BE_IMPORTED_TEMPLATE"), TLMConfigOptions.instance.currentLoadedCityConfig.ThisPath), (x, y) =>
                //    {
                //        if (y == 1)
                //        {
                //            TLMConfigOptions.instance.currentLoadedCityConfig.ReloadFromDisk();
                //            TLMConfigOptions.instance.ReloadData();
                //        }
                //    });
                //});
                group9.AddButton(Locale.Get("K45_TLM_SAVE_CURRENT_CITY_CONFIG_AS_DEFAULT"), () =>
                {
                    TLMBaseConfigXML.Instance.ExportAsGlobalConfig();
                    TLMConfigWarehouse.GetConfig(null, null).ReloadFromDisk();
                    TLMConfigOptions.instance.ReloadData();
                });
                group9.AddButton(Locale.Get("K45_TLM_LOAD_DEFAULT_AS_CURRENT_CITY_CONFIG"), () =>
                {
                    TLMBaseConfigXML.Instance.LoadFromGlobal();
                    TLMConfigOptions.instance.ReloadData();
                });

            }
            else
            {
                group9.AddButton(Locale.Get("K45_TLM_SAVE_CURRENT_CITY_CONFIG_AS_DEFAULT"), TLMBaseConfigXML.GlobalFile.ExportAsGlobalConfig);
            }
            TLMConfigOptions.instance.ReloadData();
            base.Group9SettingsUI(group9);
        }

        protected override void OnLevelLoadingInternal()
        {
            base.OnLevelLoadingInternal();
            TLMController.VerifyIfIsRealTimeEnabled();
            TransportSystemDefinition.TransportInfoDict.ToString();
        }


        private static readonly SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
        private static readonly SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
        private static readonly SavedBool m_showDistanceInLinearMap = new SavedBool("TLMshowDistanceInLinearMap", Settings.gameSettingsFile, true, true);

        public static bool showNearLinesPlop
        {
            get => m_savedShowNearLinesInCityServicesWorldInfoPanel.value;
            set => m_savedShowNearLinesInCityServicesWorldInfoPanel.value = value;
        }
        public static bool showNearLinesGrow
        {
            get => m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value;
            set => m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = value;
        }

        public static bool showDistanceLinearMap
        {
            get => m_showDistanceInLinearMap.value;
            set => m_showDistanceInLinearMap.value = value;
        }

        public override string IconName => "K45_TLM_Icon";


    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
