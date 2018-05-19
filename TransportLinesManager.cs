using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ColossalFramework.DataBinding;
using Klyte.TransportLinesManager.MapDrawer;
using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.i18n;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using System.IO;

[assembly: AssemblyVersion("9.0.0.*")]
namespace Klyte.TransportLinesManager
{
    public class TLMMod : IUserMod, ILoadingExtension
    {

        public string Name => "TLM Reborn " + TLMSingleton.version;
        public string Description => "Reviewed version of TLM. Requires Klyte Commons.";

        private static bool m_isKlyteCommonsLoaded = false;
        public static bool IsKlyteCommonsEnabled()
        {
            if (!m_isKlyteCommonsLoaded)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var assembly = (from a in assemblies
                                where a.GetType("Klyte.Commons.KlyteCommonsMod") != null
                                select a).SingleOrDefault();
                if (assembly != null)
                {
                    m_isKlyteCommonsLoaded = true;
                }
            }
            return m_isKlyteCommonsLoaded;
        }


        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            if (!IsKlyteCommonsEnabled())
            {
                return;
            }
            UIHelperExtension lastUIHelper = new UIHelperExtension((UIHelper)helperDefault);
            TLMSingleton.instance.LoadSettingsUI(lastUIHelper);
        }

        public void OnCreated(ILoading loading) { }

        public void OnLevelLoaded(LoadMode mode)
        {
            if (!IsKlyteCommonsEnabled())
            {
                throw new Exception("Transport Lines Manager Reborn now requires Klyte Commons active!");
            }
            TLMSingleton.instance.doOnLevelLoad(mode);
        }

        public void OnLevelUnloading()
        {
            TLMSingleton.instance.doOnLevelUnload();
            try
            {
                GameObject.Destroy(TLMSingleton.instance?.gameObject);
            }
            catch { }
        }

        public void OnReleased()
        {
            try
            {
                GameObject.Destroy(TLMSingleton.instance?.gameObject);
            }
            catch { }
        }
    }

    internal class TLMSingleton : Singleton<TLMSingleton>
    {
        public static readonly string FOLDER_NAME = TLMUtils.BASE_FOLDER_PATH + "TransportLinesManager";
        public const string PALETTE_SUBFOLDER_NAME = "ColorPalettes";

        public static string palettesFolder => FOLDER_NAME + Path.DirectorySeparatorChar + PALETTE_SUBFOLDER_NAME;

        public static string minorVersion
        {
            get {
                return majorVersion + "." + typeof(TLMSingleton).Assembly.GetName().Version.Build;
            }
        }
        public static string majorVersion
        {
            get {
                return typeof(TLMSingleton).Assembly.GetName().Version.Major + "." + typeof(TLMSingleton).Assembly.GetName().Version.Minor;
            }
        }
        public static string fullVersion
        {
            get {
                return minorVersion + " r" + typeof(TLMSingleton).Assembly.GetName().Version.Revision;
            }
        }
        public static string version
        {
            get {
                if (typeof(TLMSingleton).Assembly.GetName().Version.Minor == 0 && typeof(TLMSingleton).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(TLMSingleton).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(TLMSingleton).Assembly.GetName().Version.Build > 0)
                {
                    return minorVersion;
                }
                else
                {
                    return majorVersion;
                }
            }
        }


        private SavedBool m_savedOverrideDefaultLineInfoPanel;
        private SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel;
        private SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
        private SavedBool m_debugMode;
        private SavedBool m_betaMapGen;
        private SavedBool m_showDistanceInLinearMap;


        private UIDropDown editorSelector;
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown> dropDowns = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox> checkBoxes = new Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField> textFields = new Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel> lineTypesPanels = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel>();
        private UIDropDown configSelector;
        private UICheckBox overrideWorldInfoPanelLineOption;

        public bool needShowPopup;
        private bool isLocaleLoaded = false;

        private string currentSelectedConfigEditor => configSelector.selectedIndex == 0 ? currentCityId : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;

        public static bool isIPTLoaded => (bool)(Type.GetType("ImprovedPublicTransport2.ImprovedPublicTransportMod")?.GetField("inGame", Redirector<TLMDepotAI>.allFlags)?.GetValue(null) ?? false);

        public static SavedBool debugMode => TLMSingleton.instance.m_debugMode;

        public static SavedBool betaMapGen => TLMSingleton.instance.m_betaMapGen;

        public static bool showDistanceInLinearMap
        {
            get {
                return TLMSingleton.instance.m_showDistanceInLinearMap.value;
            }
            set {
                TLMSingleton.instance.m_showDistanceInLinearMap.value = value;
            }
        }

        public static SavedBool savedShowNearLinesInZonedBuildingWorldInfoPanel => TLMSingleton.instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel;

        public static SavedBool savedShowNearLinesInCityServicesWorldInfoPanel => TLMSingleton.instance.m_savedShowNearLinesInCityServicesWorldInfoPanel;

        private SavedString currentSaveVersion => new SavedString("TLMSaveVersion", Settings.gameSettingsFile, "null", true);

        private SavedInt currentLanguageId => new SavedInt("TLMLanguage", Settings.gameSettingsFile, 0, true);

        public static bool overrideWorldInfoPanelLine => TLMSingleton.instance.m_savedOverrideDefaultLineInfoPanel.value && !isIPTLoaded;

        internal TLMConfigWarehouse currentLoadedCityConfig => TLMConfigWarehouse.getConfig(currentCityId, currentCityName);

        public static bool isCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;

        private string currentCityId => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
        private string currentCityName => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_CityName : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;


        private TLMConfigWarehouse currentConfigWarehouseEditor => TLMConfigWarehouse.getConfig(currentSelectedConfigEditor, currentCityName);

        private string[] optionsForLoadConfig => currentCityId == TLMConfigWarehouse.GLOBAL_CONFIG_INDEX ? new string[] { TLMConfigWarehouse.GLOBAL_CONFIG_INDEX } : new string[] { currentCityName, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };

        internal void doOnLevelLoad(LoadMode mode)
        {

            TLMUtils.doLog("LEVEL LOAD");
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario)
            {
                TLMUtils.doLog("NOT GAME ({0})", mode);
                return;
            }

            Assembly asm = Assembly.GetAssembly(typeof(TLMSingleton));
            Type[] types = asm.GetTypes();

            TLMController.instance.Awake();
        }

        public void Awake()
        {
            Debug.LogWarningFormat("TLMRv" + TLMSingleton.majorVersion + " LOADING TLM ");
            SettingsFile tlmSettings = new SettingsFile
            {
                fileName = TLMConfigWarehouse.CONFIG_FILENAME
            };
            Debug.LogWarningFormat("TLMRv" + TLMSingleton.majorVersion + " SETTING FILES");
            try
            {
                GameSettings.AddSettingsFile(tlmSettings);
            }
            catch (Exception e)
            {
                SettingsFile tryLoad = GameSettings.FindSettingsFileByName(TLMConfigWarehouse.CONFIG_FILENAME);
                if (tryLoad == null)
                {
                    Debug.LogErrorFormat("TLMRv" + majorVersion + " SETTING FILES FAIL!!! ");
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }
                else
                {
                    tlmSettings = tryLoad;
                }
            }
            Debug.LogWarningFormat("TLMRv" + TLMSingleton.majorVersion + " LOADING VARS ");


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
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(this.autoLoadTLMLocale);
            if (instance != null) GameObject.Destroy(instance);
            loadTLMLocale(false);

            var fipalette = TLMUtils.EnsureFolderCreation(palettesFolder);
            if (Directory.GetFiles(TLMSingleton.palettesFolder, "*" + TLMAutoColorPalettes.EXT_PALETTE).Length == 0)
            {
                SavedString savedPalettes = new SavedString("savedPalettesTLM", Settings.gameSettingsFile, "", false);
                TLMAutoColorPalettes.ConvertLegacyPalettes(savedPalettes);
                //savedPalettes.Delete();
            }
            onAwake?.Invoke();
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
                            string notes = TLMResourceLoader.instance.loadResourceString("UI.VersionNotes.txt");
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
                        if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                            TLMUtils.doLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                        TLMUtils.doLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }

        internal delegate void OnLocaleLoaded();
        internal static event OnLocaleLoaded onAwake;

        internal void LoadSettingsUI(UIHelperExtension helper)
        {
            try
            {
                foreach (Transform child in helper.self.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            catch
            {

            }

            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
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

            helper.self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b)
                {
                    showVersionInfoPopup();
                }
            };

            overrideWorldInfoPanelLineOption = (UICheckBox)helper.AddCheckboxLocale("TLM_OVERRIDE_DEFAULT_LINE_INFO", m_savedOverrideDefaultLineInfoPanel.value, toggleOverrideDefaultLineInfoPanel);

            helper.AddSpace(10);

            configSelector = (UIDropDown)helper.AddDropdownLocalized("TLM_SHOW_CONFIG_FOR", optionsForLoadConfig, 0, reloadData);
            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("Loading Group 1");
            foreach (TLMConfigWarehouse.ConfigIndex transportType in new TLMConfigWarehouse.ConfigIndex[] {
                TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG,
                TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG,
                TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG,
                TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG,
                TLMConfigWarehouse.ConfigIndex.BUS_CONFIG,
                TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG,
                TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG ,
                TLMConfigWarehouse.ConfigIndex.METRO_CONFIG,
                TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG,
                TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG,
                TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG
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
            UIHelperExtension group72 = helper.AddGroupExtended(Locale.Get("TLM_DEFAULT_PRICE"));
            ((UIPanel)group72.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group72.self).wrapLayout = true;
            ((UIPanel)group72.self).width = 730;
            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableTicketTransportCategories)
            {
                var textField = generateNumberFieldConfig(group72, TLMConfigWarehouse.getNameForTransportType(ci), TLMConfigWarehouse.ConfigIndex.DEFAULT_TICKET_PRICE | ci);
                var textFieldPanel = textField.GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                textFieldPanel.GetComponentInChildren<UILabel>().minimumSize = new Vector2(420, 0);
                group72.AddSpace(2);
            }

            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("Loading Group 2");
            UIHelperExtension group7 = helper.AddGroupExtended(Locale.Get("TLM_NEAR_LINES_CONFIG"));
            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_SERVICES_BUILDINGS"), m_savedShowNearLinesInCityServicesWorldInfoPanel.value, toggleShowNearLinesInCityServicesWorldInfoPanel);
            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_ZONED_BUILDINGS"), m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value, toggleShowNearLinesInZonedBuildingWorldInfoPanel);
            group7.AddSpace(20);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_BUS"), TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_METRO"), TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TRAIN"), TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_SHIP"), TLMConfigWarehouse.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_PLANE"), TLMConfigWarehouse.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP);
            if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.AfterDark))
            {
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TAXI"), TLMConfigWarehouse.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP);
            }
            if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Snowfall))
            {
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TRAM"), TLMConfigWarehouse.ConfigIndex.TRAM_SHOW_IN_LINEAR_MAP);
            }
            if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.NaturalDisasters))
            {
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_EVAC_BUS"), TLMConfigWarehouse.ConfigIndex.EVAC_BUS_SHOW_IN_LINEAR_MAP);
            }
            if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.InMotion))
            {
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_FERRY"), TLMConfigWarehouse.ConfigIndex.FERRY_SHOW_IN_LINEAR_MAP);
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_BLIMP"), TLMConfigWarehouse.ConfigIndex.BLIMP_SHOW_IN_LINEAR_MAP);
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_MONORAIL"), TLMConfigWarehouse.ConfigIndex.MONORAIL_SHOW_IN_LINEAR_MAP);
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_CABLE_CAR"), TLMConfigWarehouse.ConfigIndex.CABLE_CAR_SHOW_IN_LINEAR_MAP);
            }
            if (Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Parks))
            {
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TOUR_BUS"), TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG_SHOW_IN_LINEAR_MAP);
                generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TOUR_PED"), TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG_SHOW_IN_LINEAR_MAP);
            }

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

            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("Loading Group 3");

            var fiPalette = TLMUtils.EnsureFolderCreation(TLMSingleton.palettesFolder);

            UIHelperExtension group6 = helper.AddGroupExtended(Locale.Get("TLM_CUSTOM_PALETTE_CONFIG"));
            ((group6.self) as UIPanel).autoLayoutDirection = LayoutDirection.Horizontal;
            ((group6.self) as UIPanel).wrapLayout = true;
            group6.AddLabel(Locale.Get("TLM_PALETTE_FOLDER_LABEL") + ":");
            var namesFilesButton = ((UIButton)group6.AddButton("/", () => { ColossalFramework.Utils.OpenInFileBrowser(fiPalette.FullName); }));
            namesFilesButton.textColor = Color.yellow;
            TLMUtils.LimitWidth(namesFilesButton, 710);
            namesFilesButton.text = fiPalette.FullName + Path.DirectorySeparatorChar;
            ((UIButton)group6.AddButton(Locale.Get("TLM_RELOAD_PALETTES"), delegate ()
            {
                TLMAutoColorPalettes.Reload();
                updateDropDowns();
            })).width = 710;

            NumberedColorList colorList = null;
            editorSelector = group6.AddDropdown(Locale.Get("TLM_PALETTE_VIEW"), TLMAutoColorPalettes.paletteListForEditing, 0, delegate (int sel)
            {
                if (sel <= 0 || sel >= TLMAutoColorPalettes.paletteListForEditing.Length)
                {
                    colorList.Disable();
                }
                else
                {
                    colorList.colorList = TLMAutoColorPalettes.getColors(TLMAutoColorPalettes.paletteListForEditing[sel]);
                    colorList.Enable();
                }
            }) as UIDropDown;
            editorSelector.GetComponentInParent<UIPanel>().width = 710;
            editorSelector.width = 710;

            colorList = group6.AddNumberedColorList(null, new List<Color32>(), (c) => { }, null, null);
            colorList.m_atlasToUse = TLMController.taLineNumber;
            colorList.m_spriteName = "SubwayIcon";
            
            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("Loading Group 4");
            UIHelperExtension group9 = helper.AddGroupExtended(Locale.Get("TLM_BETAS_EXTRA_INFO"));
            group9.AddDropdownLocalized("TLM_MOD_LANG", TLMLocaleUtils.getLanguageIndex(), currentLanguageId.value, delegate (int idx)
            {
                currentLanguageId.value = idx;
                loadTLMLocale(true);
            });
            group9.AddButton(Locale.Get("TLM_DRAW_CITY_MAP"), TLMMapDrawer.drawCityMap);
            group9.AddCheckbox(Locale.Get("TLM_DEBUG_MODE"), m_debugMode.value, delegate (bool val) { m_debugMode.value = val; });
            group9.AddLabel("Version: " + version + " rev" + typeof(TLMSingleton).Assembly.GetName().Version.Revision);
            group9.AddLabel(Locale.Get("TLM_ORIGINAL_KC_VERSION") + " " + string.Join(".", TLMResourceLoader.instance.loadResourceString("TLMVersion.txt").Split(".".ToCharArray()).Take(3).ToArray()));
            group9.AddButton(Locale.Get("TLM_RELEASE_NOTES"), delegate ()
            {
                showVersionInfoPopup(true);
            });

            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
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
                if (!isLocaleLoaded)
                {
                    isLocaleLoaded = true;
                }
            }
        }

        private T getSelectedIndex<T>(params TextList<T>[] boxes)
        {
            foreach (var box in boxes)
            {
                if (!box.unselected)
                {
                    if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                        TLMUtils.doLog("{0} is selected: {1}", box.name, box.selectedItem.ToString());
                    return box.selectedItem;
                }
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
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

        private UITextField generateNumberFieldConfig(UIHelperExtension group, string title, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            textFields[configIndex] = group.AddTextField(title, delegate (string s) { if (int.TryParse(s, out int val)) currentConfigWarehouseEditor.setInt(configIndex, val); }, currentConfigWarehouseEditor.getInt(configIndex).ToString());
            textFields[configIndex].numericalOnly = true;
            return textFields[configIndex];
        }

        private void reloadData(int selection)
        {
            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                TLMUtils.doLog("OPÇÔES RECARREGANDO ARQUIVO", currentSelectedConfigEditor);
            foreach (var i in dropDowns)
            {
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
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
                            if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                                TLMUtils.doLog("TIPO INVÁLIDO!", i);
                            break;
                    }
                }
                catch
                {
                    if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                        TLMUtils.doLog("EXCEPTION! {0} | {1} | [{2}]", i.Key, currentConfigWarehouseEditor.getString(i.Key), string.Join(",", i.Value.items));
                }

            }
            foreach (var i in checkBoxes)
            {
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                    TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                i.Value.isChecked = currentConfigWarehouseEditor.getBool(i.Key);
            }
            foreach (var i in textFields)
            {
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                    TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                i.Value.text = currentConfigWarehouseEditor.getString(i.Key);
            }
        }


        private void updateDropDowns()
        {
            string idxSel = editorSelector.selectedValue;
            editorSelector.items = TLMAutoColorPalettes.paletteListForEditing;
            editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList().IndexOf(idxSel);

            foreach (var ci in TLMConfigWarehouse.PALETTES_INDEXES)
            {
                UIDropDown paletteDD = dropDowns[ci];
                if (!paletteDD)
                    continue;
                idxSel = (paletteDD.selectedValue);
                paletteDD.items = TLMAutoColorPalettes.paletteList;
                if (!paletteDD.items.Contains(idxSel))
                {
                    idxSel = TLMAutoColorPalettes.PALETTE_RANDOM;
                }
                paletteDD.selectedIndex = paletteDD.items.ToList().IndexOf(idxSel);
            }
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

        internal void doOnLevelUnload()
        {
            if (TLMController.instance != null)
            {
                GameObject.Destroy(TLMController.instance);
            }
        }
    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
