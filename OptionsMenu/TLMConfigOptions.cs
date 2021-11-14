using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.OptionsMenu.Tabs;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;
using static Klyte.TransportLinesManager.OptionsMenu.TLMConfigOptions;

namespace Klyte.TransportLinesManager.OptionsMenu
{
    internal class TLMConfigOptions : Singleton<TLMConfigOptions>
    {
        public static bool IsCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;

        private UITabContainer tabContainer;

        internal static readonly NamingMode[] namingOptionsSuffix = new NamingMode[] {
             NamingMode.Number                  ,
             NamingMode.Roman                   ,
             NamingMode.LatinLower              ,
             NamingMode.LatinUpper              ,
             NamingMode.GreekLower              ,
             NamingMode.GreekUpper              ,
             NamingMode.CyrillicLower           ,
             NamingMode.CyrillicUpper           ,
            };
        internal static readonly NamingMode[] namingOptionsPrefix = new NamingMode[] {
                NamingMode.None                    ,
                NamingMode.Number                  ,
                NamingMode.Roman                   ,
                NamingMode.LatinLower              ,
                NamingMode.LatinLowerNumber        ,
                NamingMode.LatinUpper              ,
                NamingMode.LatinUpperNumber        ,
                NamingMode.GreekLower              ,
                NamingMode.GreekLowerNumber        ,
                NamingMode.GreekUpper              ,
                NamingMode.GreekUpperNumber        ,
                NamingMode.CyrillicLower           ,
                NamingMode.CyrillicLowerNumber     ,
                NamingMode.CyrillicUpper           ,
                NamingMode.CyrillicUpperUpper      ,
            };
        internal static readonly Separator[] namingOptionsSeparator = new Separator[] {
             Separator.None,
             Separator.Hyphen,
             Separator.Dot,
             Separator.Slash,
             Separator.Space
            };


        public void GenerateOptionsMenu(UIHelperExtension helper)
        {
            LogUtils.DoLog("Loading Options");

            KlyteMonoUtils.CreateUIElement(out UITabstrip strip, helper.Self.transform, "TabListTLMopt", new Vector4(5, 0, 730, 40));
            float effectiveOffsetY = strip.height;

            KlyteMonoUtils.CreateUIElement(out tabContainer, helper.Self.transform, "TabContainerTLMopt", new Vector4(0, 40, 725, 710));
            tabContainer.autoSize = true;
            strip.tabPages = tabContainer;

            helper.Self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b)
                {
                    TransportLinesManagerMod.Instance.ShowVersionInfoPopup();
                    ReloadData();
                }
                try
                {
                    strip.selectedIndex = strip.tabCount - 1;
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog(e.Message + "\n" + e.StackTrace);
                }
            };

            foreach (ConfigTabs tab in Enum.GetValues(typeof(ConfigTabs)))
            {
                UIButton superTab = CreateTabTemplate();
                superTab.normalFgSprite = tab.GetTabFgSprite();
                superTab.tooltip = tab.GetTabName();

                KlyteMonoUtils.CreateUIElement(out UIPanel contentParent, null);
                KlyteMonoUtils.CreateScrollPanel(contentParent, out UIScrollablePanel content, out _, tabContainer.width, tabContainer.height, Vector3.zero);
                content.name = "Container";
                content.area = new Vector4(0, 0, tabContainer.width, tabContainer.height);
                content.autoLayout = true;
                content.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
                content.autoLayoutDirection = LayoutDirection.Vertical;

                strip.AddTab(tab.ToString(), superTab.gameObject, contentParent.gameObject, tab.GetTabGenericContentImpl());
            }

            LogUtils.DoLog("End Loading Options");
        }

        #region UI generation utils

        private static UIButton CreateTabTemplate()
        {
            KlyteMonoUtils.CreateUIElement(out UIButton tabTemplate, null, "TLMTabTemplate");
            KlyteMonoUtils.InitButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }



        #endregion

        public void ReloadData()
        {
            foreach (var tab in tabContainer.GetComponentsInChildren<ITLMConfigOptionsTab>())
            {
                try
                {
                    tab.ReloadData();
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"Error reloading data for {tab}: {e.Message}\n{e.StackTrace}");
                }
            }
        }


        internal enum ConfigTabs
        {
            TransportSystem = 0,
            NearLines = 1,
            Automation = 2,
            AutoName_BD = 3,
            AutoName_PA = 4,
            Palettes = 5,
            About = 6
        }
    }

    internal static class TabsExtension
    {
        public static string GetTabName(this ConfigTabs tab)
        {
            switch (tab)
            {
                case ConfigTabs.TransportSystem: return Locale.Get("K45_TLM_TRANSPORT_SYSTEM");
                case ConfigTabs.NearLines: return Locale.Get("K45_TLM_NEAR_LINES_CONFIG");
                case ConfigTabs.Automation: return Locale.Get("K45_TLM_AUTOMATION_CONFIG");
                case ConfigTabs.AutoName_BD: return Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_OTHER");
                case ConfigTabs.AutoName_PA: return Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_AREAS");
                case ConfigTabs.Palettes: return Locale.Get("K45_TLM_CUSTOM_PALETTE_CONFIG");
                case ConfigTabs.About: return Locale.Get("K45_TLM_BETAS_EXTRA_INFO");
                default: throw new Exception($"Not supported: {tab}");
            };
        }
        public static string GetTabFgSprite(this ConfigTabs tab)
        {
            switch (tab)
            {
                case ConfigTabs.TransportSystem: return "ParkLevelStar";
                case ConfigTabs.NearLines: return "RelocateIcon";
                case ConfigTabs.Automation: return "Options";
                case ConfigTabs.AutoName_BD: return "ToolbarIconMonuments";
                case ConfigTabs.AutoName_PA: return "ToolbarIconDistrict";
                case ConfigTabs.Palettes: return "ZoningOptionFill";
                case ConfigTabs.About: return "CityInfo";
                default: throw new Exception($"Not supported: {tab}");
            };
        }

        public static Type GetTabGenericContentImpl(this ConfigTabs tab)
        {
            switch (tab)
            {
                case ConfigTabs.TransportSystem: return typeof(TLMShowConfigTabContainer);
                case ConfigTabs.NearLines: return typeof(TLMNearLinesConfigTab);
                case ConfigTabs.Automation: return typeof(TLMAutomationOptionsTab);
                case ConfigTabs.AutoName_BD: return typeof(TLMAutoNameBuildingsTab);
                case ConfigTabs.AutoName_PA: return typeof(TLMAutoNamePublicAreasTab);
                case ConfigTabs.Palettes: return typeof(TLMPaletteOptionsTab);
                case ConfigTabs.About: return typeof(TLMModInfoTab);
                default: return null;
            };
        }
    }


}
