using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Klyte.TransportLinesManager.Extensors;
using ColossalFramework.DataBinding;
using Klyte.TransportLinesManager.LineList;

[assembly: AssemblyVersion("5.0.0.*")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : IUserMod, ILoadingExtension
    {

        public static string version
        {
            get
            {
                return majorVersion + "." + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Build;
            }
        }
        public static string majorVersion
        {
            get
            {
                return typeof(TransportLinesManagerMod).Assembly.GetName().Version.Major + "." + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Minor;
            }
        }
        public static string fullVersion
        {
            get
            {
                return version + " r" + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Revision;
            }
        }
        public static TransportLinesManagerMod instance;

        //private PreviewRenderer m_previewRenderer;
        //private UITextureSprite m_currentSelectionBus;
        //private UITextureSprite m_currentSelectionTrain;



        private SavedBool m_savedOverrideDefaultLineInfoPanel;
        private SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel;
        private SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
        private SavedBool m_IPTCompatibilityMode;
        private SavedBool m_debugMode;
        private SavedBool m_betaMapGen;
        private SavedString m_surfaceMetroAssets;
        private SavedString m_bulletTrainAssets;
        private SavedString m_inactiveTrains;
        private SavedString m_lowBusAssets;
        private SavedString m_highBusAssets;
        private SavedString m_inactiveBuses;
        private SavedString m_savedPalettes;

        private TextList<string> listTrains = null;
        private TextList<string> listSurfaceMetros = null;
        private TextList<string> listBulletTrains = null;
        private TextList<string> listInactivesTrains = null;


        private TextList<string> listBus = null;
        private TextList<string> listLowBus = null;
        private TextList<string> listHighBus = null;
        private TextList<string> listInactiveBuses = null;

        private UIDropDown editorSelector;
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown> dropDowns = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox> checkBoxes = new Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField> textFields = new Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel> lineTypesPanels = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel>();
        private UIDropDown configSelector;
        private UIPanel busAssetSelections;
        private UIPanel trainAssetSelections;
        private UICheckBox overrideWorldInfoPanelLineOption;

        private bool needShowPopup;

        private string currentSelectedConfigEditor
        {
            get
            {
                return configSelector.selectedIndex == 0 ? currentCityId : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
            }
        }

        public static SavedString surfaceMetroAssets
        {
            get
            {
                return TransportLinesManagerMod.instance.m_surfaceMetroAssets;
            }
        }
        public static SavedString bulletTrainAssets
        {
            get
            {
                return TransportLinesManagerMod.instance.m_bulletTrainAssets;
            }
        }
        public static SavedString inactiveTrains
        {
            get
            {
                return TransportLinesManagerMod.instance.m_inactiveTrains;
            }
        }

        public static SavedBool IPTCompatibilityMode
        {
            get
            {
                return TransportLinesManagerMod.instance.m_IPTCompatibilityMode;
            }
        }
        public static SavedBool debugMode
        {
            get
            {
                return TransportLinesManagerMod.instance.m_debugMode;
            }
        }

        public static SavedBool betaMapGen
        {
            get
            {
                return TransportLinesManagerMod.instance.m_betaMapGen;
            }
        }

        public static SavedString lowBusAssets
        {
            get
            {
                return TransportLinesManagerMod.instance.m_lowBusAssets;
            }
        }
        public static SavedString highBusAssets
        {
            get
            {
                return TransportLinesManagerMod.instance.m_highBusAssets;
            }
        }
        public static SavedString inactiveBuses
        {
            get
            {
                return TransportLinesManagerMod.instance.m_inactiveBuses;
            }
        }

        public static SavedString savedPalettes
        {
            get
            {
                return TransportLinesManagerMod.instance.m_savedPalettes;
            }
        }

        public static SavedBool savedShowNearLinesInZonedBuildingWorldInfoPanel
        {
            get
            {
                return TransportLinesManagerMod.instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
            }
        }

        public static SavedBool savedShowNearLinesInCityServicesWorldInfoPanel
        {
            get
            {
                return TransportLinesManagerMod.instance.m_savedShowNearLinesInCityServicesWorldInfoPanel;
            }
        }

        private SavedString currentSaveVersion
        {
            get
            {
                return new SavedString("TLMSaveVersion", Settings.gameSettingsFile, "null", true);
            }
        }

        public static bool overrideWorldInfoPanelLine
        {
            get
            {
                return TransportLinesManagerMod.instance.m_savedOverrideDefaultLineInfoPanel.value && !isIPTCompatibiltyMode;
            }
        }

        public TLMConfigWarehouse currentLoadedCityConfig
        {
            get
            {
                return TLMConfigWarehouse.getConfig(currentCityId, currentCityName);
            }
        }

        public static bool isCityLoaded
        {
            get
            {
                return Singleton<SimulationManager>.instance.m_metaData != null;
            }
        }

        private string currentCityId
        {
            get
            {
                if (isCityLoaded)
                {
                    return Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier;
                }
                else return TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
            }
        }
        private string currentCityName
        {
            get
            {
                if (isCityLoaded)
                {
                    return Singleton<SimulationManager>.instance.m_metaData.m_CityName;
                }
                else return TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
            }
        }

        private TLMConfigWarehouse currentConfigWarehouseEditor
        {
            get
            {
                return TLMConfigWarehouse.getConfig(currentSelectedConfigEditor, currentCityName);
            }
        }

        private string[] getOptionsForLoadConfig()
        {
            if (currentCityId == TLMConfigWarehouse.GLOBAL_CONFIG_INDEX)
            {
                return new string[] { TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };
            }
            else return new string[] { currentCityName, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };
        }

        public static bool isIPTCompatibiltyMode
        {
            get
            {
                return TransportLinesManagerMod.instance.m_IPTCompatibilityMode.value;
            }
        }

        public string Name
        {
            get
            {

                return "Transport Lines Manager " + version;
            }
        }

        public string Description
        {
            get { return "A shortcut to manage all city's public transports lines."; }
        }



        public void OnCreated(ILoading loading)
        {
        }

        public TransportLinesManagerMod()
        {
            Debug.LogWarningFormat("TLMv" + TransportLinesManagerMod.majorVersion + " LOADING TLM ");
            SettingsFile tlmSettings = new SettingsFile();
            tlmSettings.fileName = TLMConfigWarehouse.CONFIG_FILENAME;
            Debug.LogWarningFormat("TLMv" + TransportLinesManagerMod.majorVersion + " SETTING FILES");
            try
            {
                GameSettings.AddSettingsFile(tlmSettings);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("TLMv" + TransportLinesManagerMod.majorVersion + " SETTING FILES FAIL!!! ");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
            Debug.LogWarningFormat("TLMv" + TransportLinesManagerMod.majorVersion + " LOADING VARS ");

            m_savedPalettes = new SavedString("savedPalettesTLM", Settings.gameSettingsFile, "", true);
            m_surfaceMetroAssets = new SavedString("TLMSurfaceMetroAssets", Settings.gameSettingsFile, "", true);
            m_bulletTrainAssets = new SavedString("TLMBulletTrainAssets", Settings.gameSettingsFile, "", true);
            m_inactiveTrains = new SavedString("TLMInactiveTrains", Settings.gameSettingsFile, "", true);
            m_lowBusAssets = new SavedString("TLMLowBusAssets", Settings.gameSettingsFile, "", true);
            m_highBusAssets = new SavedString("TLMHighBusAssets", Settings.gameSettingsFile, "", true);
            m_inactiveBuses = new SavedString("TLMInactiveBus", Settings.gameSettingsFile, "", true);
            m_IPTCompatibilityMode = new SavedBool("TLM_IPTCompabilityMode", Settings.gameSettingsFile, false, true);
            m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
            m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
            m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
            m_debugMode = new SavedBool("TLMdebugMode", Settings.gameSettingsFile, false, true);
            m_betaMapGen = new SavedBool("TLMbetaMapGen", Settings.gameSettingsFile, false, true);

            try
            {
                if (currentSaveVersion.value == "null")
                {
                    convertSavegame3_0();
                    currentSaveVersion.value = "!";
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("TLMv" + TransportLinesManagerMod.majorVersion + " CONVERT FILES FAIL!!! ");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
            TLMUtils.doLog("currentSaveVersion.value = {0}, fullVersion = {1}", currentSaveVersion.value, fullVersion);
            if (currentSaveVersion.value != fullVersion)
            {
                needShowPopup = true;
            }
            toggleOverrideDefaultLineInfoPanel(m_savedOverrideDefaultLineInfoPanel.value);
            instance = this;
        }

        public bool showVersionInfoPopup(bool force = false)
        {
            if (needShowPopup || force)
            {
                try
                {
                    UIComponent uIComponent = UIView.library.ShowModal("ExceptionPanel");
                    if (uIComponent != null)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        BindPropertyByKey component = uIComponent.GetComponent<BindPropertyByKey>();
                        if (component != null)
                        {
                            string title = "Transport Lines Manager v" + version;
                            string notes = ResourceLoader.loadResourceString("UI.VersionNotes.txt");
                            string text = "Transport Lines Manager was updated! Release notes:\r\n\r\n" + notes;
                            string img = "IconMessage";
                            component.SetProperties(TooltipHelper.Format(new string[]
                            {
                            "title",
                            title,
                            "message",
                            text,
                            "img",
                            img
                            }));
                            needShowPopup = false;
                            currentSaveVersion.value = fullVersion;
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        TLMUtils.doLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    TLMUtils.doLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }



        private void convertSavegame3_0()
        {
            TLMUtils.doLog("Converting old save from 3.0");
            var globalConfigArray = TLMConfigWarehouse.getConfig(TLMConfigWarehouse.GLOBAL_CONFIG_INDEX, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX);

            var m_savedNomenclaturaMetro = new SavedInt("NomenclaturaMetro", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, false);
            var m_savedNomenclaturaTrem = new SavedInt("NomenclaturaTrem", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, false);
            var m_savedNomenclaturaOnibus = new SavedInt("NomenclaturaOnibus", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, false);

            var m_savedNomenclaturaMetroSeparador = new SavedInt("NomenclaturaMetroSeparador", Settings.gameSettingsFile, (int)Separador.Nenhum, false);
            var m_savedNomenclaturaTremSeparador = new SavedInt("NomenclaturaTremSeparador", Settings.gameSettingsFile, (int)Separador.Nenhum, false);
            var m_savedNomenclaturaOnibusSeparador = new SavedInt("NomenclaturaOnibusSeparador", Settings.gameSettingsFile, (int)Separador.Nenhum, false);

            var m_savedNomenclaturaMetroPrefixo = new SavedInt("NomenclaturaMetroPrefixo", Settings.gameSettingsFile, (int)ModoNomenclatura.Nenhum, false);
            var m_savedNomenclaturaTremPrefixo = new SavedInt("NomenclaturaTremPrefixo", Settings.gameSettingsFile, (int)ModoNomenclatura.Nenhum, false);
            var m_savedNomenclaturaOnibusPrefixo = new SavedInt("NomenclaturaOnibusPrefixo", Settings.gameSettingsFile, (int)ModoNomenclatura.Nenhum, false);

            var m_savedNomenclaturaOnibusZeros = new SavedBool("NomenclaturaOnibusZeros", Settings.gameSettingsFile, true, false);
            var m_savedNomenclaturaMetroZeros = new SavedBool("NomenclaturaMetroZeros", Settings.gameSettingsFile, true, false);
            var m_savedNomenclaturaTremZeros = new SavedBool("NomenclaturaTremZeros", Settings.gameSettingsFile, true, false);

            var m_savedAutoColorPaletteMetro = new SavedString("AutoColorPaletteMetro", Settings.gameSettingsFile, TLMAutoColorPalettes.PALETTE_RANDOM, false);
            var m_savedAutoColorPaletteTrem = new SavedString("AutoColorPaletteTrem", Settings.gameSettingsFile, TLMAutoColorPalettes.PALETTE_RANDOM, false);
            var m_savedAutoColorPaletteOnibus = new SavedString("AutoColorPaletteOnibus", Settings.gameSettingsFile, TLMAutoColorPalettes.PALETTE_RANDOM, false);

            var m_savedAutoColorBasedOnPrefix = new SavedBool("AutoColorBasedOnPrefix", Settings.gameSettingsFile, false, false);
            var m_savedUseRandomColorOnPaletteOverflow = new SavedBool("AutoColorUseRandomColorOnPaletteOverflow", Settings.gameSettingsFile, false, false);

            var m_savedAutoColor = new SavedBool("AutoColorLines", Settings.gameSettingsFile, false, false);
            var m_savedCircularOnSingleDistrict = new SavedBool("AutoNameCircularOnSingleDistrictLineNaming", Settings.gameSettingsFile, true, false);
            var m_savedAutoNaming = new SavedBool("AutoNameLines", Settings.gameSettingsFile, false, false);

            var m_savedShowMetroLinesOnLinearMap = new SavedBool("showMetroLinesOnLinearMap", Settings.gameSettingsFile, true, false);
            var m_savedShowBusLinesOnLinearMap = new SavedBool("showBusLinesOnLinearMap", Settings.gameSettingsFile, false, false);
            var m_savedShowTrainLinesOnLinearMap = new SavedBool("showTrainLinesOnLinearMap", Settings.gameSettingsFile, true, false);
            var m_savedShowTaxiStopsOnLinearMap = new SavedBool("showTaxiStopsOnLinearMap", Settings.gameSettingsFile, false, false);
            var m_savedShowAirportsOnLinearMap = new SavedBool("showAirportsOnLinearMap", Settings.gameSettingsFile, true, false);
            var m_savedShowPassengerPortsOnLinearMap = new SavedBool("showPassengerPortsOnLinearMap", Settings.gameSettingsFile, true, false);

            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.METRO_PREFIX, m_savedNomenclaturaMetroPrefixo.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAIN_PREFIX, m_savedNomenclaturaTremPrefixo.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BUS_PREFIX, m_savedNomenclaturaOnibusPrefixo.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.LOW_BUS_PREFIX, m_savedNomenclaturaOnibusPrefixo.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_PREFIX, m_savedNomenclaturaOnibusPrefixo.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_PREFIX, m_savedNomenclaturaTremPrefixo.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_PREFIX, m_savedNomenclaturaTremPrefixo.value);

            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.METRO_SEPARATOR, m_savedNomenclaturaMetroSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAIN_SEPARATOR, m_savedNomenclaturaTremSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BUS_SEPARATOR, m_savedNomenclaturaOnibusSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.LOW_BUS_SEPARATOR, m_savedNomenclaturaOnibusSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_SEPARATOR, m_savedNomenclaturaOnibusSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_SEPARATOR, m_savedNomenclaturaTremSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_SEPARATOR, m_savedNomenclaturaTremSeparador.value);

            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.METRO_SUFFIX, m_savedNomenclaturaMetro.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAIN_SUFFIX, m_savedNomenclaturaTrem.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BUS_SUFFIX, m_savedNomenclaturaOnibus.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.LOW_BUS_SUFFIX, m_savedNomenclaturaOnibus.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_SUFFIX, m_savedNomenclaturaOnibus.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_SUFFIX, m_savedNomenclaturaTrem.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_SUFFIX, m_savedNomenclaturaTrem.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_LEADING_ZEROS, m_savedNomenclaturaMetroZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_LEADING_ZEROS, m_savedNomenclaturaTremZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_LEADING_ZEROS, m_savedNomenclaturaOnibusZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.LOW_BUS_LEADING_ZEROS, m_savedNomenclaturaOnibusZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_LEADING_ZEROS, m_savedNomenclaturaOnibusZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_LEADING_ZEROS, m_savedNomenclaturaTremZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_LEADING_ZEROS, m_savedNomenclaturaTremZeros.value);

            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_MAIN, m_savedAutoColorPaletteMetro.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_MAIN, m_savedAutoColorPaletteTrem.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_MAIN, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.LOW_BUS_PALETTE_MAIN, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_PALETTE_MAIN, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_PALETTE_MAIN, m_savedAutoColorPaletteTrem.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_PALETTE_MAIN, m_savedAutoColorPaletteTrem.value);

            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_SUBLINE, m_savedAutoColorPaletteMetro.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_SUBLINE, m_savedAutoColorPaletteTrem.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_SUBLINE, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.LOW_BUS_PALETTE_SUBLINE, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_PALETTE_SUBLINE, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_PALETTE_SUBLINE, m_savedAutoColorPaletteTrem.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_PALETTE_SUBLINE, m_savedAutoColorPaletteTrem.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.LOW_BUS_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.LOW_BUS_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);


            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED, m_savedAutoColor.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE, m_savedCircularOnSingleDistrict.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED, m_savedAutoNaming.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP, m_savedShowMetroLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP, m_savedShowTrainLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP, m_savedShowBusLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.LOW_BUS_SHOW_IN_LINEAR_MAP, m_savedShowBusLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_SHOW_IN_LINEAR_MAP, m_savedShowBusLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_SHOW_IN_LINEAR_MAP, m_savedShowTrainLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_SHOW_IN_LINEAR_MAP, m_savedShowTrainLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP, m_savedShowAirportsOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP, m_savedShowTaxiStopsOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP, m_savedShowPassengerPortsOnLinearMap.value);

            TLMUtils.doLog("Success Converting default data! Saving commons");
        }


        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            TLMUtils.doLog("Loading Options");
            string[] namingOptionsSufixo = new string[] {
                "Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic"
            };
            string[] namingOptionsPrefixo = new string[] {
                "Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic", "None","Number + Lower Latin","Number + Upper Latin","Number + Lower Greek", "Number + Upper Greek", "Number + Lower Cyrilic", "Number + Upper Cyrilic"
            };
            string[] namingOptionsSeparador = new string[] {
                "<None>","-",".","/", "<Blank Space>","<New Line>"
            };
            UIHelperExtension helper = new UIHelperExtension((UIHelper)helperDefault);
            //m_previewRenderer = helper.self.gameObject.AddComponent<PreviewRenderer>();
            //m_previewRenderer.cameraRotation = 120f;
            //m_previewRenderer.zoom = 3f;
            //m_previewRenderer.size = new Vector2(200, 200);

            helper.self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b && needShowPopup)
                {
                    showVersionInfoPopup();
                }
            };

            OnCheckChanged iptToggle = delegate (bool value)
            {
                lineTypesPanels[TLMConfigWarehouse.ConfigIndex.LOW_BUS_CONFIG].isVisible = !value;
                lineTypesPanels[TLMConfigWarehouse.ConfigIndex.HIGH_BUS_CONFIG].isVisible = !value;
                lineTypesPanels[TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_CONFIG].isVisible = !value;
                lineTypesPanels[TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_CONFIG].isVisible = !value;
                busAssetSelections.isVisible = !value;
                trainAssetSelections.isVisible = !value;
                overrideWorldInfoPanelLineOption.isVisible = !value;
                m_IPTCompatibilityMode.value = value;
            };

            helper.AddCheckbox("IPT compatibility mode (Needs restart)", m_IPTCompatibilityMode.value, iptToggle);
            overrideWorldInfoPanelLineOption = (UICheckBox)helper.AddCheckbox("Override default line info panel", m_savedOverrideDefaultLineInfoPanel.value, toggleOverrideDefaultLineInfoPanel);



            helper.AddSpace(10);

            configSelector = (UIDropDown)helper.AddDropdown("Show Configurations For", getOptionsForLoadConfig(), 0, reloadData);
            TLMUtils.doLog("Loading Group 1");
            foreach (TLMConfigWarehouse.ConfigIndex transportType in new TLMConfigWarehouse.ConfigIndex[] { TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG, TLMConfigWarehouse.ConfigIndex.BUS_CONFIG, TLMConfigWarehouse.ConfigIndex.LOW_BUS_CONFIG, TLMConfigWarehouse.ConfigIndex.HIGH_BUS_CONFIG, TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG, TLMConfigWarehouse.ConfigIndex.METRO_CONFIG, TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_CONFIG, TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG, TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_CONFIG })
            {
                UIHelperExtension group1 = helper.AddGroupExtended(TLMConfigWarehouse.getNameForTransportType(transportType) + " Config");
                lineTypesPanels[transportType] = group1.self.GetComponentInParent<UIPanel>();
                ((UIPanel)group1.self).autoLayoutDirection = LayoutDirection.Horizontal;
                ((UIPanel)group1.self).backgroundSprite = "EmptySprite";
                ((UIPanel)group1.self).wrapLayout = true;
                var systemColor = TLMConfigWarehouse.getColorForTransportType(transportType);
                ((UIPanel)group1.self).color = new Color32((byte)(systemColor.r * 0.7f), (byte)(systemColor.g * 0.7f), (byte)(systemColor.b * 0.7f), 0xff);
                ((UIPanel)group1.self).width = 730;
                group1.AddSpace(30);
                UIDropDown prefixDD = generateDropdownConfig(group1, "Prefix", namingOptionsPrefixo, transportType | TLMConfigWarehouse.ConfigIndex.PREFIX);
                var separatorContainer = generateDropdownConfig(group1, "Separator", namingOptionsSeparador, transportType | TLMConfigWarehouse.ConfigIndex.SEPARATOR).transform.parent.GetComponent<UIPanel>();
                UIDropDown suffixDD = generateDropdownConfig(group1, "Identifier", namingOptionsSufixo, transportType | TLMConfigWarehouse.ConfigIndex.SUFFIX);
                var prefixedPaletteContainer = generateDropdownStringValueConfig(group1, "Palette for Prefixed", TLMAutoColorPalettes.paletteList, transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_MAIN).transform.parent.GetComponent<UIPanel>();
                var paletteLabel = generateDropdownStringValueConfig(group1, "Palette for Unprefixed", TLMAutoColorPalettes.paletteList, transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_SUBLINE).transform.parent.GetComponentInChildren<UILabel>();
                var zerosContainer = generateCheckboxConfig(group1, "Leading zeros on suffix", transportType | TLMConfigWarehouse.ConfigIndex.LEADING_ZEROS);
                var prefixAsSuffixContainer = generateCheckboxConfig(group1, "Invert prefix/suffix order (allow to put letters after a large number. Ex: 637A)", transportType | TLMConfigWarehouse.ConfigIndex.INVERT_PREFIX_SUFFIX);
                generateCheckboxConfig(group1, "Random colors on palette overflow", transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);
                var autoColorBasedContainer = generateCheckboxConfig(group1, "Auto color based on prefix for prefixed lines", transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_PREFIX_BASED);
                PropertyChangedEventHandler<int> updateFunction = delegate (UIComponent c, int sel)
                {
                    bool isPrefixed = (ModoNomenclatura)sel != ModoNomenclatura.Nenhum;
                    separatorContainer.isVisible = isPrefixed;
                    prefixedPaletteContainer.isVisible = isPrefixed;
                    zerosContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero;
                    prefixAsSuffixContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero && (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Numero;
                    autoColorBasedContainer.isVisible = isPrefixed;
                    paletteLabel.text = isPrefixed ? "Palette for Unprefixed" : "Palette";
                };
                prefixDD.eventSelectedIndexChanged += updateFunction;
                suffixDD.eventSelectedIndexChanged += delegate (UIComponent c, int sel)
                {
                    bool isPrefixed = (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Nenhum;
                    zerosContainer.isVisible = isPrefixed && (ModoNomenclatura)sel == ModoNomenclatura.Numero;
                    prefixAsSuffixContainer.isVisible = isPrefixed && (ModoNomenclatura)sel == ModoNomenclatura.Numero && (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Numero;
                };
                updateFunction.Invoke(null, prefixDD.selectedIndex);
            }

            UIHelperExtension group7 = helper.AddGroupExtended("Near Lines Config");
            generateCheckboxConfig(group7, "Show high capacity bus lines", TLMConfigWarehouse.ConfigIndex.HIGH_BUS_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show regular bus lines", TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show low capacity bus lines", TLMConfigWarehouse.ConfigIndex.LOW_BUS_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show tram line", TLMConfigWarehouse.ConfigIndex.TRAM_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show metro line", TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show surface metros lines", TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show train lines", TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show bullet train lines", TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show seaports", TLMConfigWarehouse.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show airports", TLMConfigWarehouse.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show taxi stops (AD only)", TLMConfigWarehouse.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP);

            UIHelperExtension group8 = helper.AddGroupExtended("Automation");
            generateCheckboxConfig(group8, "Auto coloring enabled", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group8, "Auto naming enabled", TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED);

            UIHelperExtension group13 = helper.AddGroupExtended("Auto Naming Settings - Public Transport Buildings");
            ((UIPanel)group13.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group13.self).wrapLayout = true;
            ((UIPanel)group13.self).width = 730;

            generateCheckboxConfig(group13, "Use 'Circular' word on single district lines", TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE);
            group13.AddSpace(1);
            group13.AddLabel("Allow naming lines using buildings with below functions:\n(District names are always the last choice)");
            group13.AddSpace(1);
            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameTransportCategories)
            {
                generateCheckboxConfig(group13, TLMConfigWarehouse.getNameForTransportType(ci) + " Buildings", TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | ci).width = 300;
                var textFieldPanel = generateTextFieldConfig(group13, "Prefix (optional):", TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                group13.AddSpace(1);
            }
            UIHelperExtension group14 = helper.AddGroupExtended("Auto Naming Settings - Other Buildings");
            ((UIPanel)group14.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group14.self).wrapLayout = true;
            ((UIPanel)group14.self).width = 730;
            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameCategories)
            {
                generateCheckboxConfig(group14, TLMConfigWarehouse.getNameForServiceType(ci), TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | ci).width = 300;
                var textFieldPanel = generateTextFieldConfig(group14, "Prefix (optional):", TLMConfigWarehouse.ConfigIndex.AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                group14.AddSpace(2);
            }


            TLMUtils.doLog("Loading Group 2");

            UIHelperExtension group2 = helper.AddGroupExtended("Bus Assets Selection (Global)");
            busAssetSelections = group2.self.GetComponentInParent<UIPanel>();
            if (isCityLoaded)
            {
                ((UIPanel)group2.self).autoLayoutDirection = LayoutDirection.Horizontal;
                ((UIPanel)group2.self).autoLayoutPadding = new RectOffset(5, 5, 0, 0);
                ((UIPanel)group2.self).wrapLayout = true;
                TLMBusModifyRedirects.forceReload();

                OnTextChanged reloadTexture = delegate (string s)
                {
                    //if (!string.IsNullOrEmpty(s))
                    //{
                    //    var prefab = PrefabCollection<VehicleInfo>.FindLoaded(s);
                    //    if (prefab != null)
                    //    {
                    //        m_previewRenderer.mesh = prefab.m_mesh;
                    //        m_previewRenderer.material = prefab.m_material;
                    //        m_previewRenderer.Render();
                    //        m_currentSelectionBus.texture = m_previewRenderer.texture;
                    //    }
                    //}
                };

                listBus = group2.AddTextList("Bus as Regular Buses", TLMBusModifyRedirects.getBusAssetDictionary(), delegate (string idx) { listLowBus.unselect(); listHighBus.unselect(); listInactiveBuses.unselect(); reloadTexture(idx); }, 340, 250);
                listLowBus = group2.AddTextList("Bus as Low Bus", TLMBusModifyRedirects.getLowBusAssetDictionary(), delegate (string idx) { listBus.unselect(); listHighBus.unselect(); listInactiveBuses.unselect(); reloadTexture(idx); }, 340, 250);
                listHighBus = group2.AddTextList("Bus as High Bus", TLMBusModifyRedirects.getHighBusAssetDictionary(), delegate (string idx) { listLowBus.unselect(); listBus.unselect(); listInactiveBuses.unselect(); reloadTexture(idx); }, 340, 250);
                listInactiveBuses = group2.AddTextList("Buses Inactives", TLMBusModifyRedirects.getInactiveBusAssetDictionary(), delegate (string idx) { listLowBus.unselect(); listHighBus.unselect(); listBus.unselect(); reloadTexture(idx); }, 340, 250);
                listBus.root.backgroundSprite = "EmptySprite";
                listBus.root.color = TLMConfigWarehouse.getColorForTransportType(TLMConfigWarehouse.ConfigIndex.BUS_CONFIG);
                listBus.root.width = 340;
                listLowBus.root.backgroundSprite = "EmptySprite";
                listLowBus.root.color = TLMConfigWarehouse.getColorForTransportType(TLMConfigWarehouse.ConfigIndex.LOW_BUS_CONFIG);
                listLowBus.root.width = 340;
                listHighBus.root.backgroundSprite = "EmptySprite";
                listHighBus.root.color = TLMConfigWarehouse.getColorForTransportType(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_CONFIG);
                listHighBus.root.width = 340;
                listInactiveBuses.root.backgroundSprite = "EmptySprite";
                listInactiveBuses.root.color = Color.gray;
                listInactiveBuses.root.width = 340;
                foreach (Transform t in ((UIPanel)group2.self).transform)
                {
                    var panel = t.gameObject.GetComponent<UIPanel>();
                    if (panel)
                    {
                        panel.width = 340;
                    }
                }
                group2.AddSpace(10);
                OnButtonClicked reload = delegate
                {
                    listBus.itemsList = TLMBusModifyRedirects.getBusAssetDictionary();
                    listLowBus.itemsList = TLMBusModifyRedirects.getLowBusAssetDictionary();
                    listHighBus.itemsList = TLMBusModifyRedirects.getHighBusAssetDictionary();
                    listInactiveBuses.itemsList = TLMBusModifyRedirects.getInactiveBusAssetDictionary();
                };
                group2.AddButton("Move to Regular", delegate
                {
                    if (!listBus.unselected) return;
                    var selected = getSelectedIndex(listLowBus, listHighBus, listInactiveBuses);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMBusModifyRedirects.addAssetToBusList(selected);
                    reload();
                });
                group2.AddButton("Move to Low", delegate
                {
                    if (!listLowBus.unselected) return;
                    var selected = getSelectedIndex(listHighBus, listBus, listInactiveBuses);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMBusModifyRedirects.addAssetToLowBusList(selected);
                    reload();
                });
                group2.AddButton("Move to High", delegate
                {
                    if (!listHighBus.unselected) return;
                    var selected = getSelectedIndex(listLowBus, listBus, listInactiveBuses);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMBusModifyRedirects.addAssetToHighBusList(selected);
                    reload();
                });
                group2.AddButton("Move to Inactive", delegate
                {
                    if (!listInactiveBuses.unselected) return;
                    var selected = getSelectedIndex(listLowBus, listBus, listHighBus);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMBusModifyRedirects.addAssetToInactiveBusList(selected);
                    reload();
                });
                group2.AddButton("Reload", delegate
                {
                    reload();
                });
            }
            else
            {
                group2.AddLabel("Please load a city to get access to active buses!");
            }

            UIHelperExtension group3 = helper.AddGroupExtended("Trains Assets Selection (Global)");
            trainAssetSelections = group3.self.GetComponentInParent<UIPanel>();
            if (isCityLoaded)
            {
                ((UIPanel)group3.self).autoLayoutDirection = LayoutDirection.Horizontal;
                ((UIPanel)group3.self).autoLayoutPadding = new RectOffset(5, 5, 0, 0);
                ((UIPanel)group3.self).wrapLayout = true;
                TLMTrainModifyRedirects.forceReload();

                OnTextChanged reloadTexture = delegate (string s)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        var prefab = PrefabCollection<VehicleInfo>.FindLoaded(s);
                        if (prefab != null)
                        {
                            //m_previewRenderer.mesh = prefab.m_mesh;
                            //m_previewRenderer.material = prefab.m_material;
                            //m_previewRenderer.Render();
                            //m_currentSelectionTrain.texture = m_previewRenderer.texture;
                        }
                    }
                };

                listTrains = group3.AddTextList("Trains as Trains", TLMTrainModifyRedirects.getTrainAssetDictionary(), delegate (string idx) { listSurfaceMetros.unselect(); listBulletTrains.unselect(); listInactivesTrains.unselect(); reloadTexture(idx); }, 340, 250);
                listSurfaceMetros = group3.AddTextList("Trains as Surface Metros", TLMTrainModifyRedirects.getSurfaceMetroAssetDictionary(), delegate (string idx) { listTrains.unselect(); listBulletTrains.unselect(); listInactivesTrains.unselect(); reloadTexture(idx); }, 340, 250);
                listBulletTrains = group3.AddTextList("Trains as Bullet Trains", TLMTrainModifyRedirects.getBulletTrainAssetDictionary(), delegate (string idx) { listSurfaceMetros.unselect(); listTrains.unselect(); listInactivesTrains.unselect(); reloadTexture(idx); }, 340, 250);
                listInactivesTrains = group3.AddTextList("Trains Inactives", TLMTrainModifyRedirects.getInactiveTrainAssetDictionary(), delegate (string idx) { listSurfaceMetros.unselect(); listBulletTrains.unselect(); listTrains.unselect(); reloadTexture(idx); }, 340, 250);
                listTrains.root.backgroundSprite = "EmptySprite";
                listTrains.root.color = TLMConfigWarehouse.getColorForTransportType(TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG);
                listTrains.root.width = 340;
                listSurfaceMetros.root.backgroundSprite = "EmptySprite";
                listSurfaceMetros.root.color = TLMConfigWarehouse.getColorForTransportType(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_CONFIG);
                listSurfaceMetros.root.width = 340;
                listBulletTrains.root.backgroundSprite = "EmptySprite";
                listBulletTrains.root.color = TLMConfigWarehouse.getColorForTransportType(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_CONFIG);
                listBulletTrains.root.width = 340;
                listInactivesTrains.root.backgroundSprite = "EmptySprite";
                listInactivesTrains.root.color = Color.gray;
                listInactivesTrains.root.width = 340;
                foreach (Transform t in ((UIPanel)group3.self).transform)
                {
                    var panel = t.gameObject.GetComponent<UIPanel>();
                    if (panel)
                    {
                        panel.width = 340;
                    }
                }
                group3.AddSpace(10);
                OnButtonClicked reload = delegate
                {
                    listTrains.itemsList = TLMTrainModifyRedirects.getTrainAssetDictionary();
                    listSurfaceMetros.itemsList = TLMTrainModifyRedirects.getSurfaceMetroAssetDictionary();
                    listBulletTrains.itemsList = TLMTrainModifyRedirects.getBulletTrainAssetDictionary();
                    listInactivesTrains.itemsList = TLMTrainModifyRedirects.getInactiveTrainAssetDictionary();
                };
                group3.AddButton("Move to Train", delegate
                {
                    if (!listTrains.unselected) return;
                    var selected = getSelectedIndex(listSurfaceMetros, listBulletTrains, listInactivesTrains);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMTrainModifyRedirects.addAssetToTrainList(selected);
                    reload();
                });
                group3.AddButton("Move to S. Metro", delegate
                {
                    if (!listSurfaceMetros.unselected) return;
                    var selected = getSelectedIndex(listBulletTrains, listTrains, listInactivesTrains);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMTrainModifyRedirects.addAssetToSurfaceMetroList(selected);
                    reload();
                });
                group3.AddButton("Move to Bullet", delegate
                {
                    if (!listBulletTrains.unselected) return;
                    var selected = getSelectedIndex(listSurfaceMetros, listTrains, listInactivesTrains);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMTrainModifyRedirects.addAssetToBulletTrainList(selected);
                    reload();
                });
                group3.AddButton("Move to Inactive", delegate
                {
                    if (!listInactivesTrains.unselected) return;
                    var selected = getSelectedIndex(listSurfaceMetros, listTrains, listBulletTrains);
                    if (selected == null || selected.Equals(default(string))) return;
                    TLMTrainModifyRedirects.addAssetToInactiveTrainList(selected);
                    reload();
                });
                group3.AddButton("Reload", delegate
                {
                    reload();
                });
            }
            else
            {
                group3.AddLabel("Please load a city to get access to active trains!");
            }



            UIHelperExtension group5 = helper.AddGroupExtended("Other Global Options");
            group5.AddSpace(20);
            group5.AddCheckbox("Show near lines in public services buildings' world info panel", m_savedShowNearLinesInCityServicesWorldInfoPanel.value, toggleShowNearLinesInCityServicesWorldInfoPanel);
            group5.AddCheckbox("Show near lines in zoned buildings' world info panel", m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value, toggleShowNearLinesInZonedBuildingWorldInfoPanel);

            TLMUtils.doLog("Loading Group 3");
            UIHelperExtension group6 = helper.AddGroupExtended("Custom palettes config [" + UIHelperExtension.version + "]");
            ((group6.self) as UIPanel).autoLayoutDirection = LayoutDirection.Horizontal;
            ((group6.self) as UIPanel).wrapLayout = true;

            UITextField paletteName = null;
            DropDownColorSelector colorEditor = null;
            NumberedColorList colorList = null;

            editorSelector = group6.AddDropdown("Palette Select", TLMAutoColorPalettes.paletteListForEditing, 0, delegate (int sel)
            {
                if (sel <= 0 || sel >= TLMAutoColorPalettes.paletteListForEditing.Length)
                {
                    paletteName.enabled = false;
                    colorEditor.Disable();
                    colorList.Disable();
                }
                else {
                    paletteName.enabled = true;
                    colorEditor.Disable();
                    colorList.colorList = TLMAutoColorPalettes.getColors(TLMAutoColorPalettes.paletteListForEditing[sel]);
                    colorList.Enable();
                    paletteName.text = TLMAutoColorPalettes.paletteListForEditing[sel];
                }
            }) as UIDropDown;

            group6.AddButton("Create", delegate ()
            {
                string newName = TLMAutoColorPalettes.addPalette();
                updateDropDowns("", "");
                editorSelector.selectedValue = newName;
            });
            group6.AddButton("Delete", delegate ()
            {
                TLMAutoColorPalettes.removePalette(editorSelector.selectedValue);
                updateDropDowns("", "");
            });
            paletteName = group6.AddTextField("Palette Name", delegate (string val)
            {

            }, "", (string value) =>
            {
                string oldName = editorSelector.selectedValue;
                paletteName.text = TLMAutoColorPalettes.renamePalette(oldName, value);
                updateDropDowns(oldName, value);
            });
            paletteName.parent.width = 500;

            colorEditor = group6.AddColorField("Colors", Color.black, delegate (Color c)
            {
                TLMAutoColorPalettes.setColor(colorEditor.id, editorSelector.selectedValue, c);
                colorList.colorList = TLMAutoColorPalettes.getColors(editorSelector.selectedValue);
            }, delegate
            {
                TLMAutoColorPalettes.removeColor(editorSelector.selectedValue, colorEditor.id);
                colorList.colorList = TLMAutoColorPalettes.getColors(editorSelector.selectedValue);
            });

            colorList = group6.AddNumberedColorList(null, new List<Color32>(), delegate (int c)
            {
                colorEditor.id = c;
                colorEditor.selectedColor = TLMAutoColorPalettes.getColor(c, editorSelector.selectedValue, false);
                colorEditor.title = c.ToString();
                colorEditor.Enable();
            }, colorEditor.parent.GetComponentInChildren<UILabel>(), delegate ()
            {
                TLMAutoColorPalettes.addColor(editorSelector.selectedValue);
            });

            paletteName.enabled = false;
            colorEditor.Disable();
            colorList.Disable();
            iptToggle.Invoke(isIPTCompatibiltyMode);
            UIHelperExtension group9 = helper.AddGroupExtended("Betas & Extra Info");
            group9.AddCheckbox("[Alpha] Linear Map Exporter (Needs city reload)", m_betaMapGen.value, delegate (bool val) { m_betaMapGen.value = val; });
            group9.AddCheckbox("Debug mode", m_debugMode.value, delegate (bool val) { m_debugMode.value = val; });
            group9.AddLabel("Version: " + version + " rev" + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Revision);
            group9.AddButton("Release notes for this version", delegate () { showVersionInfoPopup(true); });
            UIHelperExtension group10 = helper.AddGroupExtended("Help");
            group10.AddLabel("Coming soon");



        }

        private T getSelectedIndex<T>(params TextList<T>[] boxes)
        {
            foreach (var box in boxes)
            {
                if (!box.unselected)
                {
                    TLMUtils.doLog("{0} is selected: {1}", box.name, box.selectedItem.ToString());
                    return box.selectedItem;
                }
                TLMUtils.doLog("{0} isn't selected", box.name);
            }
            return default(T);
        }

        private UICheckBox generateCheckboxConfig(UIHelperExtension group, string title, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            checkBoxes[configIndex] = (UICheckBox)group.AddCheckbox(title, currentConfigWarehouseEditor.getBool(configIndex), delegate (bool b) { currentConfigWarehouseEditor.setBool(configIndex, b); });

            return checkBoxes[configIndex];
        }

        private UIDropDown generateDropdownConfig(UIHelperExtension group, string title, string[] options, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            dropDowns[configIndex] = (UIDropDown)group.AddDropdown(title, options, currentConfigWarehouseEditor.getInt(configIndex), delegate (int i) { currentConfigWarehouseEditor.setInt(configIndex, i); });
            return dropDowns[configIndex];
        }

        private UIDropDown generateDropdownStringValueConfig(UIHelperExtension group, string title, string[] options, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            dropDowns[configIndex] = group.AddDropdown(title, options, currentConfigWarehouseEditor.getString(configIndex), delegate (int i) { currentConfigWarehouseEditor.setString(configIndex, options[i]); });
            return dropDowns[configIndex];
        }


        private UITextField generateTextFieldConfig(UIHelperExtension group, string title, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            textFields[configIndex] = group.AddTextField(title, delegate (string s) { currentConfigWarehouseEditor.setString(configIndex, s); }, currentConfigWarehouseEditor.getString(configIndex));
            return textFields[configIndex];
        }

        private void reloadData(int selection)
        {
            TLMUtils.doLog("OPÇÔES RECARREGANDO ARQUIVO", currentSelectedConfigEditor);
            foreach (var i in dropDowns)
            {
                TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                try
                {
                    switch (i.Key & TLMConfigWarehouse.ConfigIndex.TYPE_PART)
                    {
                        case TLMConfigWarehouse.ConfigIndex.TYPE_INT:
                            i.Value.selectedIndex = currentConfigWarehouseEditor.getInt(i.Key);
                            break;
                        case TLMConfigWarehouse.ConfigIndex.TYPE_STRING:
                            int selectedIndex = i.Value.items.ToList().IndexOf(currentConfigWarehouseEditor.getString(i.Key));
                            i.Value.selectedIndex = Math.Max(selectedIndex, 0);
                            break;
                        default:
                            TLMUtils.doLog("TIPO INVÁLIDO!", i);
                            break;
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    TLMUtils.doLog("EXCEPTION! {0} | {1} | [{2}]", i.Key, currentConfigWarehouseEditor.getString(i.Key), string.Join(",", i.Value.items));
                }

            }
            foreach (var i in checkBoxes)
            {
                TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                i.Value.isChecked = currentConfigWarehouseEditor.getBool(i.Key);
            }
            foreach (var i in textFields)
            {
                TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                i.Value.text = currentConfigWarehouseEditor.getString(i.Key);
            }
        }


        private void updateDropDowns(string oldName, string newName)
        {

            string idxSel = editorSelector.selectedValue;
            editorSelector.items = TLMAutoColorPalettes.paletteListForEditing;
            if (!TLMAutoColorPalettes.paletteListForEditing.Contains(idxSel))
            {
                if (idxSel != oldName || !TLMAutoColorPalettes.paletteListForEditing.Contains(newName))
                {
                    editorSelector.selectedIndex = 0;
                }
                else {
                    idxSel = newName;
                    editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList().IndexOf(idxSel);
                }
            }
            else {
                editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList().IndexOf(idxSel);
            }

            foreach (var ci in TLMConfigWarehouse.PALETTES_INDEXES)
            {
                UIDropDown paletteDD = dropDowns[ci];
                if (!paletteDD) continue;
                idxSel = (paletteDD.selectedValue);
                paletteDD.items = TLMAutoColorPalettes.paletteList;
                if (!paletteDD.items.Contains(idxSel))
                {
                    if (idxSel != oldName || !paletteDD.items.Contains(newName))
                    {
                        idxSel = TLMAutoColorPalettes.PALETTE_RANDOM;
                    }
                    else {
                        idxSel = newName;
                    }
                }
                paletteDD.selectedIndex = paletteDD.items.ToList().IndexOf(idxSel);
            }
        }



        public void OnLevelLoaded(LoadMode mode)
        {

            if (TLMController.instance == null)
            {
                TLMController.instance = new TLMController();
            }

            if (TLMController.taTLM == null)
            {
                TLMController.taTLM = CreateTextureAtlas("UI.Images.sprites.png", "TransportLinesManagerSprites", GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("InfoPanel").atlas.material, 32, 32, new string[] {
                    "TransportLinesManagerIcon","TransportLinesManagerIconHovered"
                });
            }
            if (TLMController.taLineNumber == null)
            {
                TLMController.taLineNumber = CreateTextureAtlas("UI.Images.lineFormat.png", "TransportLinesManagerLinearLineSprites", GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("InfoPanel").atlas.material, 64, 64, new string[] {
                  "TramIcon","ShipLineIcon","LowBusIcon","HighBusIcon", "BulletTrainIcon","BusIcon","SubwayIcon","TrainIcon","SurfaceMetroIcon","ShipIcon","AirplaneIcon","TaxiIcon","DayIcon","NightIcon","DisabledIcon","SurfaceMetroImage","BulletTrainImage","LowBusImage","HighBusImage","VehicleLinearMap"
                });
            }
            if (!TransportLinesManagerMod.isIPTCompatibiltyMode)
            {
                TLMTrainModifyRedirects.instance.EnableHooks();
                TLMBusModifyRedirects.instance.EnableHooks();
                TLMShipModifyRedirects.instance.EnableHooks();
            }
            TLMPublicTransportDetailPanelHooks.instance.EnableHooks();

            //			Log.debug ("LEVELLOAD");
        }

        public void OnLevelUnloading()
        {
            if (TLMController.instance != null)
            {
                TLMController.instance.destroy();
            }
            //			Log.debug ("LEVELUNLOAD");
        }

        public void OnReleased()
        {

        }

        UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            Texture2D tex = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Bilinear;
            { // LoadTexture
                tex.LoadImage(ResourceLoader.loadResourceData(textureFile));
                tex.Apply(true, true);
            }
            UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            { // Setup atlas
                Material material = (Material)Material.Instantiate(baseMaterial);
                material.mainTexture = tex;
                atlas.material = material;
                atlas.name = atlasName;
            }
            // Add sprites
            for (int i = 0; i < spriteNames.Length; ++i)
            {
                float uw = 1.0f / spriteNames.Length;
                var spriteInfo = new UITextureAtlas.SpriteInfo()
                {
                    name = spriteNames[i],
                    texture = tex,
                    region = new Rect(i * uw, 0, uw, 1),
                };
                atlas.AddSprite(spriteInfo);
            }
            return atlas;
        }



        private void toggleOverrideDefaultLineInfoPanel(bool b)
        {
            m_savedOverrideDefaultLineInfoPanel.value = b;
        }

        private void toggleShowNearLinesInCityServicesWorldInfoPanel(bool b)
        {
            m_savedShowNearLinesInCityServicesWorldInfoPanel.value = b;
        }

        private void toggleShowNearLinesInZonedBuildingWorldInfoPanel(bool b)
        {
            m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = b;
        }

    }

    public class TransportLinesManagerThreadMod : ThreadingExtensionBase
    {
        public override void OnCreated(IThreading threading)
        {
            if (TLMController.instance != null)
            {
                TLMController.instance.destroy();
            }
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (TLMController.instance != null)
            {
                TLMController.instance.update();
            }
        }
    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }

    public class UIRadialChartAge : UIRadialChart
    {
        public void AddSlice(Color32 innerColor, Color32 outterColor)
        {
            SliceSettings slice = new UIRadialChart.SliceSettings();
            slice.outterColor = outterColor;
            slice.innerColor = innerColor;
            this.m_Slices.Add(slice);
            this.Invalidate();
        }
    }


}
