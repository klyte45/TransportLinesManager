using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.i18n;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Klyte.Commons.Interfaces
{
    public abstract class BasicIUserMod<U, R, C, A, T> : IUserMod, ILoadingExtension
        where U : BasicIUserMod<U, R, C, A, T>, new()
        where R : KlyteResourceLoader<R>
        where C : MonoBehaviour
        where A : TextureAtlasDescriptor<A, R>
        where T : UICustomControl
    {

        public abstract string SimpleName { get; }
        public virtual bool UseGroup9 => true;
        public abstract void LoadSettings();
        public abstract void doLog(string fmt, params object[] args);
        public abstract void doErrorLog(string fmt, params object[] args);
        public abstract void TopSettingsUI(UIHelperExtension ext);

        private GameObject topObj;
        public Transform refTransform => topObj?.transform;

        protected virtual float? TabWidth => null;


        public string Name => $"{SimpleName} {version}";
        public abstract string Description { get; }

        private C m_controller;
        public C controller => m_controller;

        public void OnCreated(ILoading loading)
        {

        }
        public virtual void OnLevelLoaded(LoadMode mode)
        {
            topObj = new GameObject(typeof(U).Name);
            Type typeTarg = typeof(Redirector<>);
            System.Collections.Generic.List<Type> instances = KlyteUtils.GetSubtypesRecursive(typeTarg, typeof(U));
            doLog($"{SimpleName} Redirectors: {instances.Count()}");
            foreach (Type t in instances)
            {
                topObj.AddComponent(t);
            }
            if (typeof(C) != typeof(MonoBehaviour))
            {
                m_controller = topObj.AddComponent<C>();
            }

        }

        public string GeneralName => $"{SimpleName} (v{version})";

        public void OnLevelUnloading()
        {
            if (typeof(U).Assembly.GetName().Version.Revision != 9999)
            {
                Application.Quit();
            }
            Type typeTarg = typeof(Redirector<>);
            System.Collections.Generic.List<Type> instances = KlyteUtils.GetSubtypesRecursive(typeTarg, typeof(U));
            doLog($"{SimpleName} Redirectors: {instances.Count()}");
            foreach (Type t in instances)
            {
                GameObject.Destroy((Redirector) KlyteUtils.GetPrivateStaticField("instance", t));
            }
            GameObject.Destroy(topObj);
            typeTarg = typeof(Singleton<>);
            instances = KlyteUtils.GetSubtypesRecursive(typeTarg, typeof(U));

            foreach (Type t in instances)
            {
                GameObject.Destroy(((MonoBehaviour) KlyteUtils.GetPrivateStaticProperty("instance", t)));
            }
        }
        public virtual void OnReleased() => OnLevelUnloading();

        public static string minorVersion => majorVersion + "." + typeof(U).Assembly.GetName().Version.Build;
        public static string majorVersion => typeof(U).Assembly.GetName().Version.Major + "." + typeof(U).Assembly.GetName().Version.Minor;
        public static string fullVersion => minorVersion + " r" + typeof(U).Assembly.GetName().Version.Revision;
        public static string version
        {
            get {
                if (typeof(U).Assembly.GetName().Version.Minor == 0 && typeof(U).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(U).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(U).Assembly.GetName().Version.Build > 0)
                {
                    return minorVersion;
                }
                else
                {
                    return majorVersion;
                }
            }
        }

        public static U instance { get; private set; }

        public bool needShowPopup;

        public static bool LocaleLoaded { get; private set; } = false;


        public static SavedBool debugMode { get; } = new SavedBool("TLMDebugMode", Settings.gameSettingsFile, false, true);
        private SavedString currentSaveVersion => new SavedString("TLMSaveVersion", Settings.gameSettingsFile, "null", true);
        public static bool isCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;




        protected void Construct()
        {
            instance = this as U;
            Debug.LogWarningFormat("TLMv" + majorVersion + " LOADING ");
            LoadSettings();
            Debug.LogWarningFormat("TLMv" + majorVersion + " SETTING FILES");
            if (debugMode.value)
            {
                Debug.LogWarningFormat("currentSaveVersion.value = {0}, fullVersion = {1}", currentSaveVersion.value, fullVersion);
            }

            if (currentSaveVersion.value != fullVersion)
            {
                needShowPopup = true;
            }
        }

        private UIComponent m_onSettingsUiComponent;
        internal static readonly string m_translateFilesPath = $"{TLMUtils.BASE_FOLDER_PATH}__translations{Path.DirectorySeparatorChar}";
        private bool m_showLangDropDown = false;
        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            m_onSettingsUiComponent = new UIHelperExtension((UIHelper) helperDefault).self ?? m_onSettingsUiComponent;

            if (Locale.Get("K45_TEST_UP") != "OK")
            {
                TLMUtils.createElement<KlyteLocaleManager>(new GameObject(typeof(U).Name).transform);
                if (Locale.Get("K45_TEST_UP") != "OK")
                {
                    TLMUtils.doErrorLog("CAN'T LOAD LOCALE!!!!!");
                }
                LocaleManager.eventLocaleChanged += KlyteLocaleManager.ReloadLanguage;
                m_showLangDropDown = true;
            }
            foreach (string lang in KlyteLocaleManager.locales)
            {
                string content = Singleton<R>.instance.loadResourceString($"UI.i18n.{lang}.properties");
                if (content != null)
                {
                    File.WriteAllText($"{m_translateFilesPath}{lang}{Path.DirectorySeparatorChar}1_{Assembly.GetExecutingAssembly().GetName().Name}.txt", content);
                }
                content = Singleton<R>.instance.loadResourceString($"commons.UI.i18n.{lang}.properties");
                if (content != null)
                {
                    File.WriteAllText($"{m_translateFilesPath}{lang}{Path.DirectorySeparatorChar}0_common.txt", content);
                }

            }
            DoWithSettingsUI(new UIHelperExtension(m_onSettingsUiComponent));

        }

        private void DoWithSettingsUI(UIHelperExtension helper)
        {
            foreach (Transform child in helper.self?.transform)
            {
                GameObject.Destroy(child?.gameObject);
            }

            helper.self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b)
                {
                    showVersionInfoPopup();
                }
            };

            TopSettingsUI(helper);

            if (UseGroup9)
            {
                CreateGroup9(helper);
            }

            doLog("End Loading Options");
        }

        protected void CreateGroup9(UIHelperExtension helper)
        {
            UIHelperExtension group9 = helper.AddGroupExtended(Locale.Get("K45_BETAS_EXTRA_INFO"));
            Group9SettingsUI(group9);

            group9.AddCheckbox(Locale.Get("K45_DEBUG_MODE"), debugMode.value, delegate (bool val)
            { debugMode.value = val; });
            group9.AddLabel(string.Format(Locale.Get("K45_VERSION_SHOW"), fullVersion));

            group9.AddButton(Locale.Get("K45_RELEASE_NOTES"), delegate ()
            {
                showVersionInfoPopup(true);
            });

            if (m_showLangDropDown)
            {
                UIDropDown dd = null;
                dd = group9.AddDropdownLocalized("K45_MOD_LANG", (new string[] { "K45_GAME_DEFAULT_LANGUAGE" }.Concat(KlyteLocaleManager.locales.Select(x => $"K45_LANG_{x}")).Select(x => Locale.Get(x))).ToArray(), KlyteLocaleManager.GetLoadedLanguage(), delegate (int idx)
                {
                    KlyteLocaleManager.SaveLoadedLanguage(idx);
                    KlyteLocaleManager.ReloadLanguage();
                    KlyteLocaleManager.RedrawUIComponents();
                });
            }
            else
            {
                group9.AddLabel(string.Format(Locale.Get("K45_LANG_CTRL_MOD_INFO"), Locale.Get("K45_MOD_CONTROLLING_LOCALE")));
            }
        }

        public virtual void Group9SettingsUI(UIHelperExtension group9) { }

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
                            string title = $"{SimpleName.Replace("&", "and")} v{version}";
                            string notes = Singleton<R>.instance.loadResourceString("UI.VersionNotes.txt");
                            string text = $"{SimpleName.Replace("&", "and")} was updated! Release notes:\r\n\r\n" + notes;
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
                        doLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    doErrorLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }

        private delegate void OnLocaleLoadedFirstTime();
        private event OnLocaleLoadedFirstTime eventOnLoadLocaleEnd;



    }

}
