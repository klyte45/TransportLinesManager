using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ColossalFramework.DataBinding;
using Klyte.TransportLinesManager.LineList;
using Klyte.TransportLinesManager.MapDrawer;
using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.i18n;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Overrides;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using ColossalFramework.PlatformServices;

[assembly: AssemblyVersion("8.0.0.*")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : IUserMod, ILoadingExtension
    {

        public static string minorVersion
        {
            get {
                return majorVersion + "." + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Build;
            }
        }
        public static string majorVersion
        {
            get {
                return typeof(TransportLinesManagerMod).Assembly.GetName().Version.Major + "." + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Minor;
            }
        }
        public static string fullVersion
        {
            get {
                return minorVersion + " r" + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Revision;
            }
        }
        public static string version
        {
            get {
                if (typeof(TransportLinesManagerMod).Assembly.GetName().Version.Minor == 0 && typeof(TransportLinesManagerMod).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(TransportLinesManagerMod).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(TransportLinesManagerMod).Assembly.GetName().Version.Build > 0)
                {
                    return minorVersion;
                }
                else
                {
                    return majorVersion;
                }
            }
        }


        public static TransportLinesManagerMod instance;


        private SavedBool m_savedOverrideDefaultLineInfoPanel;
        private SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel;
        private SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
        private SavedBool m_debugMode;
        private SavedBool m_betaMapGen;
        private SavedBool m_showDistanceInLinearMap;
        private SavedString m_savedPalettes;


        private UIDropDown editorSelector;
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown> dropDowns = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox> checkBoxes = new Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField> textFields = new Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel> lineTypesPanels = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel>();
        private UIDropDown configSelector;
        private UICheckBox overrideWorldInfoPanelLineOption;

        public bool needShowPopup;

        private string currentSelectedConfigEditor
        {
            get {
                return configSelector.selectedIndex == 0 ? currentCityId : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
            }
        }

        public static bool isIPTLoaded
        {
            get {
                Type IPT2 = Type.GetType("ImprovedPublicTransport2.ImprovedPublicTransportMod");
                return IPT2 != null && (bool)IPT2.GetField("inGame", Redirector<TLMDepotAI>.allFlags).GetValue(null);
            }
        }

        public static SavedBool debugMode
        {
            get {
                return TransportLinesManagerMod.instance.m_debugMode;
            }
        }

        public static SavedBool betaMapGen
        {
            get {
                return TransportLinesManagerMod.instance.m_betaMapGen;
            }
        }

        public static bool showDistanceInLinearMap
        {
            get {
                return TransportLinesManagerMod.instance.m_showDistanceInLinearMap.value;
            }
            set {
                TransportLinesManagerMod.instance.m_showDistanceInLinearMap.value = value;
            }
        }

        public static SavedString savedPalettes
        {
            get {
                return TransportLinesManagerMod.instance.m_savedPalettes;
            }
        }

        public static SavedBool savedShowNearLinesInZonedBuildingWorldInfoPanel
        {
            get {
                return TransportLinesManagerMod.instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
            }
        }

        public static SavedBool savedShowNearLinesInCityServicesWorldInfoPanel
        {
            get {
                return TransportLinesManagerMod.instance.m_savedShowNearLinesInCityServicesWorldInfoPanel;
            }
        }

        private SavedString currentSaveVersion
        {
            get {
                return new SavedString("TLMSaveVersion", Settings.gameSettingsFile, "null", true);
            }
        }

        private SavedInt currentLanguageId
        {
            get {
                return new SavedInt("TLMLanguage", Settings.gameSettingsFile, 0, true);
            }
        }

        public static bool overrideWorldInfoPanelLine
        {
            get {
                return TransportLinesManagerMod.instance.m_savedOverrideDefaultLineInfoPanel.value && !isIPTLoaded;
            }
        }

        public TLMConfigWarehouse currentLoadedCityConfig
        {
            get {
                return TLMConfigWarehouse.getConfig(currentCityId, currentCityName);
            }
        }

        public static bool isCityLoaded
        {
            get {
                return Singleton<SimulationManager>.instance.m_metaData != null;
            }
        }

        private string currentCityId
        {
            get {
                if (isCityLoaded)
                {
                    return Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier;
                }
                else
                    return TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
            }
        }
        private string currentCityName
        {
            get {
                if (isCityLoaded)
                {
                    return Singleton<SimulationManager>.instance.m_metaData.m_CityName;
                }
                else
                    return TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
            }
        }

        private TLMConfigWarehouse currentConfigWarehouseEditor
        {
            get {
                return TLMConfigWarehouse.getConfig(currentSelectedConfigEditor, currentCityName);
            }
        }

        private string[] getOptionsForLoadConfig()
        {
            if (currentCityId == TLMConfigWarehouse.GLOBAL_CONFIG_INDEX)
            {
                return new string[] { TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };
            }
            else
                return new string[] { currentCityName, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };
        }

        public string Name
        {
            get {

                return "TLM Reborn " + version;
            }
        }

        public string Description
        {
            get {
                return "Reviewed version of TLM.";
            }
        }



        public void OnCreated(ILoading loading)
        {
        }

        public TransportLinesManagerMod()
        {

            Debug.LogWarningFormat("TLMRv" + TransportLinesManagerMod.majorVersion + " LOADING TLM ");
            SettingsFile tlmSettings = new SettingsFile
            {
                fileName = TLMConfigWarehouse.CONFIG_FILENAME
            };
            Debug.LogWarningFormat("TLMRv" + TransportLinesManagerMod.majorVersion + " SETTING FILES");
            try
            {
                GameSettings.AddSettingsFile(tlmSettings);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("TLMRv" + TransportLinesManagerMod.majorVersion + " SETTING FILES FAIL!!! ");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
            Debug.LogWarningFormat("TLMRv" + TransportLinesManagerMod.majorVersion + " LOADING VARS ");

            m_savedPalettes = new SavedString("savedPalettesTLM", Settings.gameSettingsFile, "", true);
            m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
            m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
            m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
            m_debugMode = new SavedBool("TLMdebugMode", Settings.gameSettingsFile, false, true);
            m_betaMapGen = new SavedBool("TLMbetaMapGen", Settings.gameSettingsFile, false, true);
            m_showDistanceInLinearMap = new SavedBool("TLMshowDistanceInLinearMap", Settings.gameSettingsFile, true, true);

            if (m_debugMode.value)
                TLMUtils.doLog("currentSaveVersion.value = {0}, fullVersion = {1}", currentSaveVersion.value, fullVersion);
            if (currentSaveVersion.value != fullVersion)
            {
                needShowPopup = true;
            }
            toggleOverrideDefaultLineInfoPanel(m_savedOverrideDefaultLineInfoPanel.value);
            loadTLMLocale(false);
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(this.autoLoadTLMLocale);
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
                        if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                            TLMUtils.doLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                        TLMUtils.doLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }

        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            var oldTLMid = new PublishedFileId(408875519);
            if (!string.IsNullOrEmpty(PlatformService.workshop.GetSubscribedItemPath(oldTLMid)))
            {
                TLMUtils.doLog("Removing old TLM");
                PlatformService.workshop.Unsubscribe(oldTLMid);
            }

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Loading Options");
            loadTLMLocale(false);
            string[] namingOptionsSufixo = new string[] {
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 0)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 1)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 2)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 3)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 4)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 5)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 6)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 14))
            };
            string[] namingOptionsPrefixo = new string[] {
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 0)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 1)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 2)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 3)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 4)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 5)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 6)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 7)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 8)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 9)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 10)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 11)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 12)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 13)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 14))
            };
            string[] namingOptionsSeparador = new string[] {
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 0)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 1)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 2)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 3)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 4)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 5)),
            };
            UIHelperExtension helper = new UIHelperExtension((UIHelper)helperDefault);

            helper.self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b)
                {
                    showVersionInfoPopup();
                }
            };

            overrideWorldInfoPanelLineOption = (UICheckBox)helper.AddCheckboxLocale("TLM_OVERRIDE_DEFAULT_LINE_INFO", m_savedOverrideDefaultLineInfoPanel.value, toggleOverrideDefaultLineInfoPanel);

            helper.AddSpace(10);

            configSelector = (UIDropDown)helper.AddDropdownLocalized("TLM_SHOW_CONFIG_FOR", getOptionsForLoadConfig(), 0, reloadData);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Loading Group 1");
            foreach (TLMConfigWarehouse.ConfigIndex transportType in new TLMConfigWarehouse.ConfigIndex[] {
                TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG,
                TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG,
                TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG,
                TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG,
                TLMConfigWarehouse.ConfigIndex.BUS_CONFIG,
                TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG,
                TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG,
                TLMConfigWarehouse.ConfigIndex.METRO_CONFIG,
                TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG
            })
            {
                UIHelperExtension group1 = helper.AddGroupExtended(string.Format(Locale.Get("TLM_CONFIGS_FOR"), TLMConfigWarehouse.getNameForTransportType(transportType)));
                lineTypesPanels[transportType] = group1.self.GetComponentInParent<UIPanel>();
                ((UIPanel)group1.self).autoLayoutDirection = LayoutDirection.Horizontal;
                ((UIPanel)group1.self).backgroundSprite = "EmptySprite";
                ((UIPanel)group1.self).wrapLayout = true;
                var systemColor = TLMConfigWarehouse.getColorForTransportType(transportType);
                ((UIPanel)group1.self).color = new Color32((byte)(systemColor.r * 0.7f), (byte)(systemColor.g * 0.7f), (byte)(systemColor.b * 0.7f), 0xff);
                ((UIPanel)group1.self).width = 730;
                group1.AddSpace(30);
                UIDropDown prefixDD = generateDropdownConfig(group1, Locale.Get("TLM_PREFIX"), namingOptionsPrefixo, transportType | TLMConfigWarehouse.ConfigIndex.PREFIX);
                var separatorContainer = generateDropdownConfig(group1, Locale.Get("TLM_SEPARATOR"), namingOptionsSeparador, transportType | TLMConfigWarehouse.ConfigIndex.SEPARATOR).transform.parent.GetComponent<UIPanel>();
                UIDropDown suffixDD = generateDropdownConfig(group1, Locale.Get("TLM_SUFFIX"), namingOptionsSufixo, transportType | TLMConfigWarehouse.ConfigIndex.SUFFIX);
                var suffixDDContainer = suffixDD.transform.parent.GetComponent<UIPanel>();
                UIDropDown nonPrefixDD = generateDropdownConfig(group1, Locale.Get("TLM_IDENTIFIER_NON_PREFIXED"), namingOptionsSufixo, transportType | TLMConfigWarehouse.ConfigIndex.NON_PREFIX);
                var prefixedPaletteContainer = generateDropdownStringValueConfig(group1, Locale.Get("TLM_PALETTE_PREFIXED"), TLMAutoColorPalettes.paletteList, transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_MAIN).transform.parent.GetComponent<UIPanel>();
                var paletteLabel = generateDropdownStringValueConfig(group1, Locale.Get("TLM_PALETTE_UNPREFIXED"), TLMAutoColorPalettes.paletteList, transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_SUBLINE).transform.parent.GetComponentInChildren<UILabel>();
                var zerosContainer = generateCheckboxConfig(group1, Locale.Get("TLM_LEADING_ZEROS_SUFFIX"), transportType | TLMConfigWarehouse.ConfigIndex.LEADING_ZEROS);
                var prefixAsSuffixContainer = generateCheckboxConfig(group1, Locale.Get("TLM_INVERT_PREFIX_SUFFIX_ORDER"), transportType | TLMConfigWarehouse.ConfigIndex.INVERT_PREFIX_SUFFIX);
                generateCheckboxConfig(group1, Locale.Get("TLM_RANDOM_ON_PALETTE_OVERFLOW"), transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);
                var autoColorBasedContainer = generateCheckboxConfig(group1, Locale.Get("TLM_AUTO_COLOR_BASED_ON_PREFIX"), transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_PREFIX_BASED);
                var prefixIncrement = generateCheckboxConfig(group1, Locale.Get("TLM_LINENUMBERING_BASED_IN_PREFIX"), transportType | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT);
                PropertyChangedEventHandler<int> updateFunction = delegate (UIComponent c, int sel)
                {
                    bool isPrefixed = (ModoNomenclatura)sel != ModoNomenclatura.Nenhum;
                    separatorContainer.isVisible = isPrefixed;
                    prefixedPaletteContainer.isVisible = isPrefixed;
                    prefixIncrement.isVisible = isPrefixed;
                    suffixDDContainer.isVisible = isPrefixed;
                    zerosContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero;
                    prefixAsSuffixContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero && (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Numero;
                    autoColorBasedContainer.isVisible = isPrefixed;
                    paletteLabel.text = isPrefixed ? Locale.Get("TLM_PALETTE_UNPREFIXED") : Locale.Get("TLM_PALETTE");
                    if (TLMPublicTransportDetailPanel.instance != null && TLMPublicTransportDetailPanel.instance.prefixEditor.m_systemTypeDropDown != null)
                    {
                        TLMPublicTransportDetailPanel.instance.prefixEditor.m_systemTypeDropDown.selectedIndex = 0;
                    }
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

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Loading Group 2");
            UIHelperExtension group7 = helper.AddGroupExtended(Locale.Get("TLM_NEAR_LINES_CONFIG"));
            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_SERVICES_BUILDINGS"), m_savedShowNearLinesInCityServicesWorldInfoPanel.value, toggleShowNearLinesInCityServicesWorldInfoPanel);
            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_ZONED_BUILDINGS"), m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value, toggleShowNearLinesInZonedBuildingWorldInfoPanel);
            group7.AddSpace(20);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_BUS"), TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TRAM"), TLMConfigWarehouse.ConfigIndex.TRAM_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_METRO"), TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TRAIN"), TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_SHIP"), TLMConfigWarehouse.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_PLANE"), TLMConfigWarehouse.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_FERRY"), TLMConfigWarehouse.ConfigIndex.FERRY_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_BLIMP"), TLMConfigWarehouse.ConfigIndex.BLIMP_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TAXI"), TLMConfigWarehouse.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_MONORAIL"), TLMConfigWarehouse.ConfigIndex.MONORAIL_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_CABLE_CAR"), TLMConfigWarehouse.ConfigIndex.CABLE_CAR_SHOW_IN_LINEAR_MAP);

            UIHelperExtension group8 = helper.AddGroupExtended(Locale.Get("TLM_AUTOMATION_CONFIG"));
            generateCheckboxConfig(group8, Locale.Get("TLM_AUTO_COLOR_ENABLED"), TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group8, Locale.Get("TLM_AUTO_NAME_ENABLED"), TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED);
            generateCheckboxConfig(group8, Locale.Get("TLM_USE_CIRCULAR_AUTO_NAME"), TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE);
            generateCheckboxConfig(group8, Locale.Get("TLM_ADD_LINE_NUMBER_AUTO_NAME"), TLMConfigWarehouse.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME);

            UIHelperExtension group13 = helper.AddGroupExtended(Locale.Get("TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT"));
            ((UIPanel)group13.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group13.self).wrapLayout = true;
            ((UIPanel)group13.self).width = 730;

            group13.AddSpace(1);
            group13.AddLabel(Locale.Get("TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group13.AddSpace(1);
            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameTransportCategories)
            {
                generateCheckboxConfig(group13, TLMConfigWarehouse.getNameForTransportType(ci), TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | ci).width = 300;
                var textFieldPanel = generateTextFieldConfig(group13, Locale.Get("TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                group13.AddSpace(1);
            }
            UIHelperExtension group14 = helper.AddGroupExtended(Locale.Get("TLM_AUTO_NAME_SETTINGS_OTHER"));
            ((UIPanel)group14.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group14.self).wrapLayout = true;
            ((UIPanel)group14.self).width = 730;
            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameCategories)
            {
                generateCheckboxConfig(group14, TLMConfigWarehouse.getNameForServiceType(ci), TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | ci).width = 300;
                var textFieldPanel = generateTextFieldConfig(group14, Locale.Get("TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                group14.AddSpace(2);
            }

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Loading Group 3");
            UIHelperExtension group6 = helper.AddGroupExtended(Locale.Get("TLM_CUSTOM_PALETTE_CONFIG") + " [" + UIHelperExtension.version + "]");
            ((group6.self) as UIPanel).autoLayoutDirection = LayoutDirection.Horizontal;
            ((group6.self) as UIPanel).wrapLayout = true;

            UITextField paletteName = null;
            DropDownColorSelector colorEditor = null;
            NumberedColorList colorList = null;

            editorSelector = group6.AddDropdown(Locale.Get("TLM_PALETTE_SELECT"), TLMAutoColorPalettes.paletteListForEditing, 0, delegate (int sel)
            {
                if (sel <= 0 || sel >= TLMAutoColorPalettes.paletteListForEditing.Length)
                {
                    paletteName.enabled = false;
                    colorEditor.Disable();
                    colorList.Disable();
                }
                else
                {
                    paletteName.enabled = true;
                    colorEditor.Disable();
                    colorList.colorList = TLMAutoColorPalettes.getColors(TLMAutoColorPalettes.paletteListForEditing[sel]);
                    colorList.Enable();
                    paletteName.text = TLMAutoColorPalettes.paletteListForEditing[sel];
                }
            }) as UIDropDown;

            group6.AddButton(Locale.Get("CREATE"), delegate ()
            {
                string newName = TLMAutoColorPalettes.addPalette();
                updateDropDowns("", "");
                editorSelector.selectedValue = newName;
            });
            group6.AddButton(Locale.Get("TLM_DELETE"), delegate ()
            {
                TLMAutoColorPalettes.removePalette(editorSelector.selectedValue);
                updateDropDowns("", "");
            });
            paletteName = group6.AddTextField(Locale.Get("TLM_PALETTE_NAME"), delegate (string val)
            {

            }, "", (string value) =>
            {
                string oldName = editorSelector.selectedValue;
                paletteName.text = TLMAutoColorPalettes.renamePalette(oldName, value);
                updateDropDowns(oldName, value);
            });
            paletteName.parent.width = 500;

            colorEditor = group6.AddColorField(Locale.Get("TLM_COLORS"), Color.black, delegate (Color c)
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

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Loading Group 3½");
            paletteName.enabled = false;
            colorEditor.Disable();
            colorList.Disable();

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("Loading Group 4");
            UIHelperExtension group9 = helper.AddGroupExtended(Locale.Get("TLM_BETAS_EXTRA_INFO"));
            group9.AddDropdownLocalized("TLM_MOD_LANG", TLMLocaleUtils.getLanguageIndex(), currentLanguageId.value, delegate (int idx)
            {
                currentLanguageId.value = idx;
                loadTLMLocale(true);
            });
            group9.AddButton(Locale.Get("TLM_DRAW_CITY_MAP"), TLMMapDrawer.drawCityMap);
            group9.AddCheckbox(Locale.Get("TLM_DEBUG_MODE"), m_debugMode.value, delegate (bool val) { m_debugMode.value = val; });
            group9.AddLabel("Version: " + version + " rev" + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Revision);
            group9.AddButton(Locale.Get("TLM_RELEASE_NOTES"), delegate ()
            {
                showVersionInfoPopup(true);
            });

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("End Loading Options");
        }

        public void autoLoadTLMLocale()
        {
            if (currentLanguageId.value == 0)
            {
                loadTLMLocale(false);
            }
        }
        public void loadTLMLocale(bool force)
        {
            if (SingletonLite<LocaleManager>.exists)
            {
                TLMLocaleUtils.loadLocale(currentLanguageId.value == 0 ? SingletonLite<LocaleManager>.instance.language : TLMLocaleUtils.getSelectedLocaleByIndex(currentLanguageId.value), force);
            }
        }

        private T getSelectedIndex<T>(params TextList<T>[] boxes)
        {
            foreach (var box in boxes)
            {
                if (!box.unselected)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                        TLMUtils.doLog("{0} is selected: {1}", box.name, box.selectedItem.ToString());
                    return box.selectedItem;
                }
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
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
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                TLMUtils.doLog("OPÇÔES RECARREGANDO ARQUIVO", currentSelectedConfigEditor);
            foreach (var i in dropDowns)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
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
                            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                                TLMUtils.doLog("TIPO INVÁLIDO!", i);
                            break;
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                        TLMUtils.doLog("EXCEPTION! {0} | {1} | [{2}]", i.Key, currentConfigWarehouseEditor.getString(i.Key), string.Join(",", i.Value.items));
                }

            }
            foreach (var i in checkBoxes)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
                    TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                i.Value.isChecked = currentConfigWarehouseEditor.getBool(i.Key);
            }
            foreach (var i in textFields)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode)
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
                else
                {
                    idxSel = newName;
                    editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList().IndexOf(idxSel);
                }
            }
            else
            {
                editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList().IndexOf(idxSel);
            }

            foreach (var ci in TLMConfigWarehouse.PALETTES_INDEXES)
            {
                UIDropDown paletteDD = dropDowns[ci];
                if (!paletteDD)
                    continue;
                idxSel = (paletteDD.selectedValue);
                paletteDD.items = TLMAutoColorPalettes.paletteList;
                if (!paletteDD.items.Contains(idxSel))
                {
                    if (idxSel != oldName || !paletteDD.items.Contains(newName))
                    {
                        idxSel = TLMAutoColorPalettes.PALETTE_RANDOM;
                    }
                    else
                    {
                        idxSel = newName;
                    }
                }
                paletteDD.selectedIndex = paletteDD.items.ToList().IndexOf(idxSel);
            }
        }

        private bool m_loaded = false;

        public void OnLevelLoaded(LoadMode mode)
        {
            TLMUtils.doLog("LEVEL LOAD");
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                m_loaded = false;
                TLMUtils.doLog("NOT GAME ({0})", mode);
                return;
            }

            if (TLMController.taTLM == null)
            {
                TLMController.taTLM = CreateTextureAtlas("UI.Images.sprites.png", "TransportLinesManagerSprites", GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("InfoPanel").atlas.material, 64, 64, new string[] {
                    "TransportLinesManagerIcon","TransportLinesManagerIconHovered","AutoNameIcon","AutoColorIcon","RemoveUnwantedIcon","ConfigIcon","24hLineIcon", "PerHourIcon"
                });
            }
            if (TLMController.taLineNumber == null)
            {
                TLMController.taLineNumber = CreateTextureAtlas("UI.Images.lineFormat.png", "TransportLinesManagerLinearLineSprites", GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("InfoPanel").atlas.material, 64, 64, new string[] {
                "EvacBusIcon","DepotIcon", "LinearHalfStation","LinearStation","LinearBg","PlaneLineIcon","TramIcon","ShipLineIcon","FerryIcon","CableCarIcon", "BlimpIcon","BusIcon","SubwayIcon","TrainIcon","MonorailIcon","ShipIcon","AirplaneIcon","TaxiIcon","DayIcon",
                    "NightIcon","DisabledIcon","NoBudgetIcon","BulletTrainImage","LowBusImage","HighBusImage","VehicleLinearMap","RegionalTrainIcon"
                });
            }

            Assembly asm = Assembly.GetAssembly(typeof(TransportLinesManagerMod));
            Type[] types = asm.GetTypes();

            TLMPublicTransportDetailPanelHooks.instance.EnableHooks();
            TransportLineOverrides.instance.EnableHooks();
            TLMDepotAI.instance.EnableHooks();
            TransportToolOverrides.instance.EnableHooks();
            TransportManagerOverrides.instance.EnableHooks();
            loadTLMLocale(false);
            m_loaded = true;
        }

        public void OnLevelUnloading()
        {
            if (!m_loaded) { return; }
            if (TLMController.instance != null)
            {
                TLMController.instance.destroy();
            }
            //			Log.debug ("LEVELUNLOAD");
            m_loaded = false;
        }

        public void OnReleased()
        {

        }

        UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            Texture2D tex = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };
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
        public override void OnReleased()
        {
            if (TLMController.instance != null)
            {
                TLMController.instance.destroy();
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
            SliceSettings slice = new UIRadialChart.SliceSettings
            {
                outterColor = outterColor,
                innerColor = innerColor
            };
            this.m_Slices.Add(slice);
            this.Invalidate();
        }
    }


}
