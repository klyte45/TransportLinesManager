using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.OptionsMenu.Tabs;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.TransportLinesManager.OptionsMenu.TLMConfigOptions;

namespace Klyte.TransportLinesManager.OptionsMenu
{
    class TLMConfigOptions : Singleton<TLMConfigOptions>
    {

        private UIDropDown configSelector;

        private string currentSelectedConfigEditor => TLMConfigOptions.instance.configSelector.selectedIndex == 0 ? currentCityId : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;

        private TLMConfigWarehouse currentConfigWarehouseEditor => TLMConfigWarehouse.getConfig(currentSelectedConfigEditor, currentCityName);

        private string[] optionsForLoadConfig => currentCityId == TLMConfigWarehouse.GLOBAL_CONFIG_INDEX ? new string[] { TLMConfigWarehouse.GLOBAL_CONFIG_INDEX } : new string[] { currentCityName, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };

        public static bool isCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;

        private string currentCityId => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
        private string currentCityName => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_CityName : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
        internal TLMConfigWarehouse currentLoadedCityConfig => TLMConfigWarehouse.getConfig(currentCityId, currentCityName);


        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown> dropDowns = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox> checkBoxes = new Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField> textFields = new Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField>();


        internal readonly string[] namingOptionsSufixo = new string[] {
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 0)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 1)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 2)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 3)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 4)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 5)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 6)),
                Locale.Get("TLM_MODO_NOMENCLATURA",Enum.GetName(typeof(ModoNomenclatura), 14))
            };
        internal readonly string[] namingOptionsPrefixo = new string[] {
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
        internal readonly string[] namingOptionsSeparador = new string[] {
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 0)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 1)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 2)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 3)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 4)),
                Locale.Get("TLM_SEPARATOR",Enum.GetName(typeof(Separador), 5)),
            };


        public void GenerateOptionsMenu(UIHelperExtension helper)
        {

            TLMUtils.doLog("Loading Options");
            TLMSingleton.instance.loadTLMLocale(false);

            var overrideWorldInfoPanelLineOption = (UICheckBox)helper.AddCheckboxLocale("TLM_OVERRIDE_DEFAULT_LINE_INFO", TLMSingleton.instance.m_savedOverrideDefaultLineInfoPanel.value, toggleOverrideDefaultLineInfoPanel);

            helper.AddSpace(10);

            configSelector = (UIDropDown)helper.AddDropdownLocalized("TLM_SHOW_CONFIG_FOR", optionsForLoadConfig, 0, reloadData);

            TLMUtils.createUIElement(out UITabstrip strip, helper.self.transform, "TabListTLMopt", new Vector4(5, 0, 730, 40));
            var effectiveOffsetY = strip.height;

            TLMUtils.createUIElement(out UITabContainer tabContainer, helper.self.transform, "TabContainerTLMopt", new Vector4(0, 40, 730, 500));
            tabContainer.autoSize = true;
            strip.tabPages = tabContainer;

            helper.self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b)
                {
                    TLMSingleton.instance.showVersionInfoPopup();
                }
                strip.selectedIndex = strip.tabCount - 1;
            };

            foreach (ConfigTabs tab in Enum.GetValues(typeof(ConfigTabs)))
            {
                UIButton superTab = CreateTabTemplate();
                superTab.normalFgSprite = tab.getTabFgSprite();
                superTab.tooltip = tab.getTabName();

                TLMUtils.createUIElement(out UIPanel content, null);
                content.name = "Container";
                content.area = new Vector4(0, 0, tabContainer.width, tabContainer.height);
                content.autoLayout = true;
                content.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
                content.padding = new RectOffset(5, 5, 5, 5);
                content.autoLayoutDirection = LayoutDirection.Vertical;

                strip.AddTab(tab.ToString(), superTab.gameObject, content.gameObject, tab.getTabGenericContentImpl());
            }

            TLMUtils.doLog("End Loading Options");
        }

        #region UI generation utils

        private static UIButton CreateTabTemplate()
        {
            TLMUtils.createUIElement(out UIButton tabTemplate, null, "TLMTabTemplate");
            TLMUtils.initButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }

        internal UICheckBox generateCheckboxConfig(UIHelperExtension group, string title, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            checkBoxes[configIndex] = (UICheckBox)group.AddCheckbox(title, currentConfigWarehouseEditor.getBool(configIndex), delegate (bool b) { currentConfigWarehouseEditor.setBool(configIndex, b); });

            return checkBoxes[configIndex];
        }

        internal UIDropDown generateDropdownConfig(UIHelperExtension group, string title, string[] options, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            dropDowns[configIndex] = (UIDropDown)group.AddDropdown(title, options, currentConfigWarehouseEditor.getInt(configIndex), delegate (int i) { currentConfigWarehouseEditor.setInt(configIndex, i); }, true);
            return dropDowns[configIndex];
        }

        internal UIDropDown generateDropdownStringValueConfig(UIHelperExtension group, string title, string[] options, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            dropDowns[configIndex] = group.AddDropdown(title, options, currentConfigWarehouseEditor.getString(configIndex), delegate (int i) { currentConfigWarehouseEditor.setString(configIndex, options[i]); }, true);
            return dropDowns[configIndex];
        }


        internal UIDropDown generateDropdownEnumStringValueConfig<T>(UIHelperExtension group, string title, string[] options, TLMConfigWarehouse.ConfigIndex configIndex) where T : struct, IConvertible
        {
            int currentValue;
            try
            {
                currentValue = (int)Enum.Parse(typeof(T), currentConfigWarehouseEditor.getString(configIndex));
            }
            catch
            {
                currentValue = 0;
            }
            dropDowns[configIndex] = (UIDropDown)group.AddDropdown(title, options, currentValue, delegate (int i) { currentConfigWarehouseEditor.setString(configIndex, Enum.GetNames(typeof(T))[i]); }, true);
            return dropDowns[configIndex];
        }


        internal UITextField generateTextFieldConfig(UIHelperExtension group, string title, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            textFields[configIndex] = group.AddTextField(title, delegate (string s) { currentConfigWarehouseEditor.setString(configIndex, s); }, currentConfigWarehouseEditor.getString(configIndex));
            return textFields[configIndex];
        }

        internal UITextField generateNumberFieldConfig(UIHelperExtension group, string title, TLMConfigWarehouse.ConfigIndex configIndex)
        {
            textFields[configIndex] = group.AddTextField(title, delegate (string s) { if (int.TryParse(s, out int val)) currentConfigWarehouseEditor.setInt(configIndex, val); }, currentConfigWarehouseEditor.getInt(configIndex).ToString());
            textFields[configIndex].numericalOnly = true;
            return textFields[configIndex];
        }


        #endregion


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
                catch
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


        internal void updateDropDowns()
        {
            foreach (var ci in TLMConfigWarehouse.PALETTES_INDEXES)
            {
                UIDropDown paletteDD = dropDowns[ci];
                if (!paletteDD)
                    continue;
                string idxSel = (paletteDD.selectedValue);
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
            TLMSingleton.instance.m_savedOverrideDefaultLineInfoPanel.value = b;
        }


        internal enum ConfigTabs
        {
            TransportSystem = 0,
            TicketPrices = 1,
            NearLines = 2,
            Automation = 3,
            AutoName_PT = 4,
            AutoName_BD = 5,
            AutoName_PA = 6,
            Palettes = 7,
            About = 8
        }
    }
    static class TabsExtension
    {
        public static string getTabName(this ConfigTabs tab)
        {
            switch (tab)
            {
                case ConfigTabs.TransportSystem: return Locale.Get("TLM_TRANSPORT_SYSTEM");
                case ConfigTabs.TicketPrices: return Locale.Get("TLM_DEFAULT_PRICE");
                case ConfigTabs.NearLines: return Locale.Get("TLM_NEAR_LINES_CONFIG");
                case ConfigTabs.Automation: return Locale.Get("TLM_AUTOMATION_CONFIG");
                case ConfigTabs.AutoName_PT: return Locale.Get("TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT");
                case ConfigTabs.AutoName_BD: return Locale.Get("TLM_AUTO_NAME_SETTINGS_OTHER");
                case ConfigTabs.AutoName_PA: return Locale.Get("TLM_AUTO_NAME_SETTINGS_PUBLIC_AREAS");
                case ConfigTabs.Palettes: return Locale.Get("TLM_CUSTOM_PALETTE_CONFIG");
                case ConfigTabs.About: return Locale.Get("TLM_BETAS_EXTRA_INFO");
                default:
                    throw new Exception($"Not supported: {tab}");
            }

        }
        public static string getTabFgSprite(this ConfigTabs tab)
        {
            switch (tab)
            {
                case ConfigTabs.TransportSystem: return "ParkLevelStar";
                case ConfigTabs.TicketPrices: return "FootballTicketIcon";
                case ConfigTabs.NearLines: return "RelocateIcon";
                case ConfigTabs.Automation: return "Options";
                case ConfigTabs.AutoName_PT: return "ToolbarIconPublicTransport";
                case ConfigTabs.AutoName_BD: return "ToolbarIconMonuments";
                case ConfigTabs.AutoName_PA: return "ToolbarIconDistrict";
                case ConfigTabs.Palettes: return "ZoningOptionFill";
                case ConfigTabs.About: return "CityInfo";
                default:
                    throw new Exception($"Not supported: {tab}");
            }
        }

        public static Type getTabGenericContentImpl(this ConfigTabs tab)
        {
            switch (tab)
            {
                case ConfigTabs.TransportSystem: return typeof(TLMShowConfigTabContainer);
                case ConfigTabs.TicketPrices: return typeof(TLMDefaultTicketPriceConfigTab);
                case ConfigTabs.NearLines: return typeof(TLMNearLinesConfigTab);
                case ConfigTabs.Automation: return typeof(TLMAutomationOptionsTab);
                case ConfigTabs.AutoName_PT: return typeof(TLMAutoNamePublicTransportTab);
                case ConfigTabs.AutoName_BD: return typeof(TLMAutoNameBuildingsTab);
                case ConfigTabs.AutoName_PA: return typeof(TLMAutoNamePublicAreasTab);
                case ConfigTabs.Palettes: return typeof(TLMPaletteOptionsTab);
                case ConfigTabs.About: return typeof(TLMModInfoTab);
                default:
                    return null;
            }
        }
    }


}
