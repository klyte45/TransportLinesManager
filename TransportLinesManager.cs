using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.i18n;
using Klyte.TransportLinesManager.OptionsMenu;
using Klyte.TransportLinesManager.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("11.1.0.0")]
namespace Klyte.TransportLinesManager
{
    public class TLMMod : IUserMod, ILoadingExtension
    {

        public string Name => "Transport Lines Manager " + TLMSingleton.version;
        public string Description => "Allows to customize and manage your public transport systems. Requires Klyte Commons.";

        private static bool m_isKlyteCommonsLoaded = false;
        public static bool IsKlyteCommonsEnabled()
        {
            if (!m_isKlyteCommonsLoaded)
            {
                try
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
                catch { }
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
        public static readonly string FOLDER_NAME = "TransportLinesManager";
        public static readonly string FOLDER_PATH = TLMUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string PALETTE_SUBFOLDER_NAME = "ColorPalettes";
        public const string EXPORTED_MAPS_SUBFOLDER_NAME = "ExportedMaps";

        public static string palettesFolder => FOLDER_PATH + Path.DirectorySeparatorChar + PALETTE_SUBFOLDER_NAME;
        public static string configsFolder => TLMConfigWarehouse.CONFIG_PATH;
        public static string exportedMapsFolder => FOLDER_PATH + Path.DirectorySeparatorChar + EXPORTED_MAPS_SUBFOLDER_NAME;

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



        private SavedBool m_debugMode;
        public SavedBool m_savedOverrideDefaultLineInfoPanel { get; private set; }
        public SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel { get; private set; }
        public SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel { get; private set; }
        public SavedBool m_showDistanceInLinearMap { get; private set; }


        public bool needShowPopup;
        private bool isLocaleLoaded = false;


        public static bool isIPTLoaded => (bool)(Type.GetType("ImprovedPublicTransport2.ImprovedPublicTransportMod")?.GetField("inGame", Redirector<TLMDepotAI>.allFlags)?.GetValue(null) ?? false);

        public static SavedBool debugMode => TLMSingleton.instance.m_debugMode;


        public static bool showDistanceInLinearMap
        {
            get {
                return instance.m_showDistanceInLinearMap.value;
            }
            set {
                instance.m_showDistanceInLinearMap.value = value;
            }
        }

        public static SavedBool savedShowNearLinesInZonedBuildingWorldInfoPanel => instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel;

        public static SavedBool savedShowNearLinesInCityServicesWorldInfoPanel => instance.m_savedShowNearLinesInCityServicesWorldInfoPanel;

        private SavedString currentSaveVersion = new SavedString("TLMSaveVersion", Settings.gameSettingsFile, "null", true);

        private SavedInt currentLanguageId = new SavedInt("TLMLanguage", Settings.gameSettingsFile, 0, true);

        public int currentLanguageIdx => currentLanguageId.value;

        public static bool overrideWorldInfoPanelLine => instance.m_savedOverrideDefaultLineInfoPanel.value && !isIPTLoaded;



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
            TLMUtils.EnsureFolderCreation(configsFolder);
            string currentConfigPath = PathUtils.AddExtension(TLMConfigWarehouse.CONFIG_PATH + TLMConfigWarehouse.CONFIG_FILENAME + "_" + TLMConfigWarehouse.GLOBAL_CONFIG_INDEX, GameSettings.extension);
            if (!File.Exists(currentConfigPath))
            {
                var legacyFilename = Path.Combine(DataLocation.localApplicationData, PathUtils.AddExtension("TransportsLinesManager5_DEFAULT", GameSettings.extension));
                if (File.Exists(legacyFilename))
                {
                    File.Copy(legacyFilename, currentConfigPath);
                }
            }
            Debug.LogWarningFormat("TLMRv" + TLMSingleton.majorVersion + " LOADING VARS ");


            m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
            m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
            m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
            m_showDistanceInLinearMap = new SavedBool("TLMshowDistanceInLinearMap", Settings.gameSettingsFile, true, true);
            m_debugMode = new SavedBool("TLMdebugMode", Settings.gameSettingsFile, false, true);

            if (m_debugMode.value)
                TLMUtils.doLog("currentSaveVersion.value = {0}, fullVersion = {1}", currentSaveVersion.value, fullVersion);
            if (currentSaveVersion.value != fullVersion)
            {
                needShowPopup = true;
            }
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
            TLMConfigOptions.instance.GenerateOptionsMenu(helper);

        }

        public void autoLoadTLMLocale()
        {
            if (currentLanguageId.value == 0)
            {
                loadTLMLocale(false);
            }
        }
        public void loadTLMLocale(bool force, int? idx = null)
        {
            if (idx != null)
            {
                currentLanguageId.value = (int)idx;
            }
            if (SingletonLite<LocaleManager>.exists)
            {
                TLMLocaleUtils.loadLocale(currentLanguageId.value == 0 ? SingletonLite<LocaleManager>.instance.language : TLMLocaleUtils.getSelectedLocaleByIndex(currentLanguageId.value), force);
                if (!isLocaleLoaded)
                {
                    isLocaleLoaded = true;
                }
            }
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
