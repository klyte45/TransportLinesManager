using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.MapDrawer;
using Klyte.TransportLinesManager.OptionsMenu;
using Klyte.TransportLinesManager.Xml;
using System.Collections.Generic;
using System.Reflection;

[assembly: AssemblyVersion("14.4.0.0")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : BasicIUserMod<TransportLinesManagerMod, TLMController, TLMPanel>
    {
        public override string SimpleName => "Transport Lines Manager";
        public override string Description => "Allows to customize and manage your public transport systems.";
        public override bool UseGroup9 => false;

        protected override Dictionary<ulong, string> IncompatibleModList { get; } = new Dictionary<ulong, string>();

        public TransportLinesManagerMod() : base()
        {
            IncompatibleModList[TLMController.IPT2_MOD_ID] = "IPT2 is incompatible since TLM changes the same code behavior. Isn't recommended to use both together.";
            IncompatibleModList[TLMController.RETURN_VEHICLE_MOD_ID] = "Transport Vehicle Return Patch is not necessary to use along the TLM since version 14." +
            " With the introduction of the Express Buses system, now the vehicles are emptied in the next terminal stop before get to the depot.";
        }

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


        private static readonly SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("K45_TLM_showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
        private static readonly SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("K45_TLM_showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
        private static readonly SavedBool m_savedUseGameClockAsReferenceIfNoDayNightCycle = new SavedBool("K45_TLM_useGameClockAsReferenceIfNoDayNightCycle", Settings.gameSettingsFile, false, true);
        private static readonly SavedBool m_showDistanceInLinearMap = new SavedBool("K45_TLM_showDistanceInLinearMap", Settings.gameSettingsFile, true, true);

        public static bool ShowNearLinesPlop
        {
            get => m_savedShowNearLinesInCityServicesWorldInfoPanel.value;
            set => m_savedShowNearLinesInCityServicesWorldInfoPanel.value = value;
        }
        public static bool ShowNearLinesGrow
        {
            get => m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value;
            set => m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = value;
        }

        public static bool ShowDistanceLinearMap
        {
            get => m_showDistanceInLinearMap.value;
            set => m_showDistanceInLinearMap.value = value;
        }
        public static bool UseGameClockAsReferenceIfNoDayNight
        {
            get => m_savedUseGameClockAsReferenceIfNoDayNightCycle.value;
            set => m_savedUseGameClockAsReferenceIfNoDayNightCycle.value = value;
        }
        public override string IconName => "K45_TLM_Icon";

        protected override Tuple<string, string> GetButtonLink() => Tuple.New("View details of v14.1 at TLM's GitHub", "https://github.com/klyte45/TransportLinesManager/issues?q=milestone%3Av14.1+is%3Aclosed");
    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
