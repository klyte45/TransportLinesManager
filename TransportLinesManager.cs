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

[assembly: AssemblyVersion("3.1.*")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : IUserMod, ILoadingExtension
    {

        public static string version
        {
            get
            {
                return typeof(TransportLinesManagerMod).Assembly.GetName().Version.Major + "." + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Minor + " b" + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Build;
            }
        }
        public static string majorVersion
        {
            get
            {
                return typeof(TransportLinesManagerMod).Assembly.GetName().Version.Major + "." + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Minor;
            }
        }
        public static TransportLinesManagerMod instance;

        private SavedBool m_savedOverrideDefaultLineInfoPanel;
        private SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel;
        private SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
        private SavedString m_savedPalettes;

        private UIDropDown editorSelector;
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown> dropDowns = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox> checkBoxes = new Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox>();
        private UIDropDown configSelector;

        private string currentSelectedConfigEditor
        {
            get
            {
                return configSelector.selectedIndex == 0 ? currentCityId : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
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

        public static SavedBool savedOverrideDefaultLineInfoPanel
        {
            get
            {
                return TransportLinesManagerMod.instance.m_savedOverrideDefaultLineInfoPanel;
            }
        }

        public TLMConfigWarehouse currentLoadedCityConfig
        {
            get
            {
                return TLMConfigWarehouse.getConfig(currentCityId, currentCityName);
            }
        }

        private string currentCityId
        {
            get
            {
                if (Singleton<SimulationManager>.instance.m_metaData != null)
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
                if (Singleton<SimulationManager>.instance.m_metaData != null)
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
            SettingsFile tlmSettings = new SettingsFile();
            tlmSettings.fileName = TLMConfigWarehouse.CONFIG_FILENAME;
            GameSettings.AddSettingsFile(tlmSettings);

            m_savedPalettes = new SavedString("savedPalettesTLM", Settings.gameSettingsFile, TLMAutoColorPalettes.defaultPaletteList, true);
            m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
            m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
            //IPT Incompatible
            bool IPTEnabled = Singleton<PluginManager>.instance.GetPluginsInfo().FirstOrDefault(x => x.publishedFileID.AsUInt64 == 424106600L && x.isEnabled) != null;
            m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, !IPTEnabled, true);

            var currentSaveVersion = new SavedString("TLMSaveVersion", Settings.gameSettingsFile, "null", true);
            if (currentSaveVersion.value == "null")
            {
                convertSavegame3_0();
            }
            currentSaveVersion.value = majorVersion;
            toggleOverrideDefaultLineInfoPanel(m_savedOverrideDefaultLineInfoPanel.value);
            instance = this;
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
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAM_PREFIX, m_savedNomenclaturaTremPrefixo.value);

            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.METRO_SEPARATOR, m_savedNomenclaturaMetroSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAIN_SEPARATOR, m_savedNomenclaturaTremSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BUS_SEPARATOR, m_savedNomenclaturaOnibusSeparador.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAM_SEPARATOR, m_savedNomenclaturaTremSeparador.value);

            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.METRO_SUFFIX, m_savedNomenclaturaMetro.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAIN_SUFFIX, m_savedNomenclaturaTrem.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.BUS_SUFFIX, m_savedNomenclaturaOnibus.value);
            globalConfigArray.setInt(TLMConfigWarehouse.ConfigIndex.TRAM_SUFFIX, m_savedNomenclaturaTrem.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_LEADING_ZEROS, m_savedNomenclaturaMetroZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_LEADING_ZEROS, m_savedNomenclaturaTremZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_LEADING_ZEROS, m_savedNomenclaturaOnibusZeros.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAM_LEADING_ZEROS, m_savedNomenclaturaTremZeros.value);

            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_MAIN, m_savedAutoColorPaletteMetro.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_MAIN, m_savedAutoColorPaletteTrem.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_MAIN, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_MAIN, m_savedAutoColorPaletteTrem.value);

            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_SUBLINE, m_savedAutoColorPaletteMetro.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_SUBLINE, m_savedAutoColorPaletteTrem.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_SUBLINE, m_savedAutoColorPaletteOnibus.value);
            globalConfigArray.setString(TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_SUBLINE, m_savedAutoColorPaletteTrem.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_PREFIX_BASED, m_savedAutoColorBasedOnPrefix.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_RANDOM_ON_OVERFLOW, m_savedUseRandomColorOnPaletteOverflow.value);


            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED, m_savedAutoColor.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE, m_savedCircularOnSingleDistrict.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED, m_savedAutoNaming.value);

            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP, m_savedShowMetroLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP, m_savedShowTrainLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP, m_savedShowBusLinesOnLinearMap.value);
            globalConfigArray.setBool(TLMConfigWarehouse.ConfigIndex.TRAM_SHOW_IN_LINEAR_MAP, m_savedShowTrainLinesOnLinearMap.value);
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
                "Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic", "None"
            };
            string[] namingOptionsSeparador = new string[] {
                "<None>","-",".","/", "<Blank Space>","<New Line>"
            };
            UIHelperExtension helper = new UIHelperExtension((UIHelper)helperDefault);
            helper.AddCheckbox("Override default line info panel (Always disabled with IPT!)", m_savedOverrideDefaultLineInfoPanel.value, toggleOverrideDefaultLineInfoPanel);

            helper.AddSpace(10);

            configSelector = (UIDropDown)helper.AddDropdown("Show Configurations For", getOptionsForLoadConfig(), 0, reloadData);
            TLMUtils.doLog("Loading Group 1");
            UIHelperExtension group1 = helper.AddGroupExtended("Buses Config");
            ((UIPanel)group1.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group1.self).backgroundSprite = "EmptySprite";
            ((UIPanel)group1.self).wrapLayout = true;
            ((UIPanel)group1.self).color = new Color32(53, 121, 188, 128);
            group1.AddSpace(30);
            generateDropdownConfig(group1, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.BUS_PREFIX);
            generateDropdownConfig(group1, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.BUS_SEPARATOR);
            generateDropdownConfig(group1, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.BUS_SUFFIX);
            generateDropdownStringValueConfig(group1, "Default Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_MAIN);
            generateDropdownStringValueConfig(group1, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_SUBLINE);
            generateCheckboxConfig(group1, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.BUS_LEADING_ZEROS);
            generateCheckboxConfig(group1, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group1, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_PREFIX_BASED);


            UIHelperExtension group2 = helper.AddGroupExtended("Metro Config");
            ((UIPanel)group2.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group2.self).backgroundSprite = "EmptySprite";
            ((UIPanel)group2.self).wrapLayout = true;
            ((UIPanel)group2.self).color = new Color32(58, 117, 50, 128);
            group2.AddSpace(30);

            generateDropdownConfig(group2, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.METRO_PREFIX);
            generateDropdownConfig(group2, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.METRO_SEPARATOR);
            generateDropdownConfig(group2, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.METRO_SUFFIX);
            generateDropdownStringValueConfig(group2, "Prefix Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_MAIN);
            generateDropdownStringValueConfig(group2, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_SUBLINE);



            generateCheckboxConfig(group2, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.METRO_LEADING_ZEROS);
            generateCheckboxConfig(group2, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group2, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_PREFIX_BASED);

            UIHelperExtension group3 = helper.AddGroupExtended("Train Config");
            ((UIPanel)group3.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group3.self).backgroundSprite = "EmptySprite";
            ((UIPanel)group3.self).wrapLayout = true;
            ((UIPanel)group3.self).color = new Color32(250, 104, 0, 128);
            group3.AddSpace(30);
            generateDropdownConfig(group3, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.TRAIN_PREFIX);
            generateDropdownConfig(group3, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.TRAIN_SEPARATOR);
            generateDropdownConfig(group3, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.TRAIN_SUFFIX);
            generateDropdownStringValueConfig(group3, "Prefix Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_MAIN);
            generateDropdownStringValueConfig(group3, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_SUBLINE);



            generateCheckboxConfig(group3, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.TRAIN_LEADING_ZEROS);
            generateCheckboxConfig(group3, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group3, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_PREFIX_BASED);


            UIHelperExtension group4 = helper.AddGroupExtended("Tram Config");
            ((UIPanel)group4.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group4.self).wrapLayout = true;
            ((UIPanel)group4.self).backgroundSprite = "EmptySprite";
            ((UIPanel)group4.self).color = new Color32(73, 27, 137, 128);
            group4.AddSpace(30);
            generateDropdownConfig(group4, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.TRAM_PREFIX);
            generateDropdownConfig(group4, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.TRAM_SEPARATOR);
            generateDropdownConfig(group4, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.TRAM_SUFFIX);
            generateDropdownStringValueConfig(group4, "Prefix Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_MAIN);
            generateDropdownStringValueConfig(group4, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_SUBLINE);

            generateCheckboxConfig(group4, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.TRAM_LEADING_ZEROS);
            generateCheckboxConfig(group4, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group4, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_PREFIX_BASED);

            UIHelperExtension group7 = helper.AddGroupExtended("Near Lines Config");
            generateCheckboxConfig(group7, "Show bus lines", TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show metro line", TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show train lines", TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show trams lines", TLMConfigWarehouse.ConfigIndex.TRAM_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show seaports", TLMConfigWarehouse.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show airports", TLMConfigWarehouse.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show taxi stops (AD only)", TLMConfigWarehouse.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP);


            UIHelperExtension group8 = helper.AddGroupExtended("Automation");
            generateCheckboxConfig(group8, "Auto coloring enabled", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group8, "Auto naming enabled", TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED);
            generateCheckboxConfig(group8, "Use 'Circular' word on single district lines", TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE);

            TLMUtils.doLog("Loading Group 2");


            UIHelperExtension group5 = helper.AddGroupExtended("Global Options");
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
                catch (Exception e)
                {
                    TLMUtils.doLog("EXCEPTION! {0} | {1} | [{2}]", i.Key, currentConfigWarehouseEditor.getString(i.Key), string.Join(",", i.Value.items));
                }

            }
            foreach (var i in checkBoxes)
            {
                TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                i.Value.isChecked = currentConfigWarehouseEditor.getBool(i.Key);
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
                if (!TLMAutoColorPalettes.paletteList.Contains(idxSel))
                {
                    if (idxSel != oldName || !TLMAutoColorPalettes.paletteList.Contains(newName))
                    {
                        idxSel = TLMAutoColorPalettes.PALETTE_RANDOM;
                    }
                    else {
                        idxSel = newName;
                    }
                }
                paletteDD.selectedIndex = TLMAutoColorPalettes.paletteList.ToList().IndexOf(idxSel);
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
                TLMController.taTLM = CreateTextureAtlas("sprites.png", "TransportLinesManagerSprites", GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("InfoPanel").atlas.material, 32, 32, new string[] {
                    "TransportLinesManagerIcon","TransportLinesManagerIconHovered"
                });
            }
            if (TLMController.taLineNumber == null)
            {
                TLMController.taLineNumber = CreateTextureAtlas("lineFormat.png", "TransportLinesManagerLinearLineSprites", GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("InfoPanel").atlas.material, 64, 64, new string[] {
                    "BusIcon","SubwayIcon","TrainIcon","TramIcon","ShipIcon","AirplaneIcon","TaxiIcon","DayIcon","NightIcon","DisabledIcon"
                });

            }
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

            bool IPTEnabled = Singleton<PluginManager>.instance.GetPluginsInfo().FirstOrDefault(x => x.publishedFileID.AsUInt64 == 424106600L && x.isEnabled) != null;
            m_savedOverrideDefaultLineInfoPanel.value = b && !IPTEnabled;
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
                TLMController.instance.init();
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
