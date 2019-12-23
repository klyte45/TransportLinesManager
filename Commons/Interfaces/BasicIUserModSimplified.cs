using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.i18n;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ColossalFramework.UI.UITextureAtlas;

namespace Klyte.Commons.Interfaces
{
    public abstract class BasicIUserModSimplified<U, C> : IUserMod, ILoadingExtension
        where U : BasicIUserModSimplified<U, C>, new()
        where C : MonoBehaviour
    {

        public abstract string SimpleName { get; }
        public abstract string IconName { get; }
        public virtual bool UseGroup9 => true;
        public abstract void LoadSettings();
        public abstract void DoLog(string fmt, params object[] args);
        public abstract void DoErrorLog(string fmt, params object[] args);
        public abstract void TopSettingsUI(UIHelperExtension ext);

        private GameObject m_topObj;
        public Transform RefTransform => m_topObj?.transform;

        public string Name => $"{SimpleName} {Version}";
        public abstract string Description { get; }
        public C Controller { get; private set; }

        public void OnCreated(ILoading loading)
        {

        }
        public void OnLevelLoaded(LoadMode mode)
        {

            OnLevelLoadedInherit(mode);
            OnLevelLoadingInternal();
        }

        protected virtual void OnLevelLoadedInherit(LoadMode mode)
        {
            if (IsValidLoadMode(mode))
            {
                m_topObj = GameObject.Find(typeof(U).Name) ?? new GameObject(typeof(U).Name);
                Type typeTarg = typeof(IRedirectable);
                List<Type> instances = ReflectionUtils.GetInterfaceImplementations(typeTarg, GetType());
                LogUtils.DoLog($"{SimpleName} Redirectors: {instances.Count()}");
                foreach (Type t in instances)
                {
                    m_topObj.AddComponent(t);
                }
                if (typeof(C) != typeof(MonoBehaviour))
                {
                    Controller = m_topObj.AddComponent<C>();
                }

                UIButton toMainMenuButton = GameObject.Find("ToMainMenu")?.GetComponent<UIButton>();
                if (toMainMenuButton != null)
                {
                    toMainMenuButton.isEnabled = false;
                    toMainMenuButton.tooltipLocaleID = "K45_TO_MAIN_MENU_DISABLE_WARNING";
                    toMainMenuButton.isTooltipLocalized = true;
                }
            }
        }

        protected virtual void OnLevelLoadingInternal()
        {

        }

        private static bool IsValidLoadMode(LoadMode mode) => mode == LoadMode.LoadGame || mode == LoadMode.LoadScenario || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario;

        public string GeneralName => $"{SimpleName} (v{Version})";

        public void OnLevelUnloading()
        {
            if (typeof(U).Assembly.GetName().Version.Revision != 9999)
            {
                Application.Quit();
            }
        }
        public virtual void OnReleased() => OnLevelUnloading();

        public static string MinorVersion => MajorVersion + "." + typeof(U).Assembly.GetName().Version.Build;
        public static string MajorVersion => typeof(U).Assembly.GetName().Version.Major + "." + typeof(U).Assembly.GetName().Version.Minor;
        public static string FullVersion => MinorVersion + " r" + typeof(U).Assembly.GetName().Version.Revision;
        public static string Version
        {
            get {
                if (typeof(U).Assembly.GetName().Version.Minor == 0 && typeof(U).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(U).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(U).Assembly.GetName().Version.Build > 0)
                {
                    return MinorVersion;
                }
                else
                {
                    return MajorVersion;
                }
            }
        }

        public bool needShowPopup;

        public static SavedBool DebugMode { get; } = new SavedBool(CommonProperties.Acronym + "_DebugMode", Settings.gameSettingsFile, false, true);
        private SavedString CurrentSaveVersion { get; } = new SavedString(CommonProperties.Acronym + "SaveVersion", Settings.gameSettingsFile, "null", true);
        public static bool IsCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;

        public static U Instance { get; set; }

        protected void Construct()
        {
            Instance = this as U;
            Debug.LogWarningFormat(CommonProperties.Acronym + "v" + MajorVersion + " LOADING ");
            LoadSettings();
            Debug.LogWarningFormat(CommonProperties.Acronym + "v" + MajorVersion + " SETTING FILES");
            if (DebugMode.value)
            {
                Debug.LogWarningFormat("currentSaveVersion.value = {0}, fullVersion = {1}", CurrentSaveVersion.value, FullVersion);
            }

            if (CurrentSaveVersion.value != FullVersion)
            {
                needShowPopup = true;
            }
        }

        private UIComponent m_onSettingsUiComponent;
        private bool m_showLangDropDown = false;

        public void OnSettingsUI(UIHelperBase helperDefault)
        {

            m_onSettingsUiComponent = new UIHelperExtension((UIHelper) helperDefault).Self ?? m_onSettingsUiComponent;

            if (Locale.Get("K45_TEST_UP") != "OK")
            {
                KlyteMonoUtils.CreateElement<KlyteLocaleManager>(new GameObject(typeof(U).Name).transform);
                if (Locale.Get("K45_TEST_UP") != "OK")
                {
                    LogUtils.DoErrorLog("CAN'T LOAD LOCALE!!!!!");
                }
                LocaleManager.eventLocaleChanged += KlyteLocaleManager.ReloadLanguage;
                m_showLangDropDown = true;
            }
            foreach (string lang in KlyteLocaleManager.locales)
            {
                string content = KlyteResourceLoader.LoadResourceString($"UI.i18n.{lang}.properties");
                if (content != null)
                {
                    File.WriteAllText($"{KlyteLocaleManager.m_translateFilesPath}{lang}{Path.DirectorySeparatorChar}1_{Assembly.GetExecutingAssembly().GetName().Name}.txt", content);
                }
                content = KlyteResourceLoader.LoadResourceString($"commons.UI.i18n.{lang}.properties");
                if (content != null)
                {
                    File.WriteAllText($"{KlyteLocaleManager.m_translateFilesPath}{lang}{Path.DirectorySeparatorChar}0_common.txt", content);
                }

            }
            KlyteLocaleManager.ReloadLanguage(true);
            DoWithSettingsUI(new UIHelperExtension(m_onSettingsUiComponent));
        }

        private void DoWithSettingsUI(UIHelperExtension helper)
        {
            foreach (Transform child in helper.Self?.transform)
            {
                GameObject.Destroy(child?.gameObject);
            }

            var newSprites = new List<SpriteInfo>();
            TextureAtlasUtils.LoadIamgesFromResources("commons.UI.Images", ref newSprites);
            TextureAtlasUtils.LoadIamgesFromResources("UI.Images", ref newSprites);
            LogUtils.DoErrorLog($"ADDING {newSprites.Count} sprites!");
            TextureAtlasUtils.RegenerateDefaultTextureAtlas(newSprites);


            helper.Self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b)
                {
                    ShowVersionInfoPopup();
                }
            };

            TopSettingsUI(helper);

            if (UseGroup9)
            {
                CreateGroup9(helper);
            }


            LogUtils.DoLog("End Loading Options");
        }



        protected void CreateGroup9(UIHelperExtension helper)
        {
            UIHelperExtension group9 = helper.AddGroupExtended(Locale.Get("K45_BETAS_EXTRA_INFO"));
            Group9SettingsUI(group9);

            group9.AddCheckbox(Locale.Get("K45_DEBUG_MODE"), DebugMode.value, delegate (bool val)
            { DebugMode.value = val; });
            group9.AddLabel(string.Format(Locale.Get("K45_VERSION_SHOW"), FullVersion));
            group9.AddButton(Locale.Get("K45_RELEASE_NOTES"), delegate ()
            {
                ShowVersionInfoPopup(true);
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

        public bool ShowVersionInfoPopup(bool force = false)
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
                            string title = $"{SimpleName.Replace("&", "and")} v{Version}";
                            string notes = KlyteResourceLoader.LoadResourceString("UI.VersionNotes.txt");
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
                            CurrentSaveVersion.value = FullVersion;
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        LogUtils.DoLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    DoErrorLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }

    }

}
