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

        private string currentSelectedConfigEditor;

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
                return TLMConfigWarehouse.getConfig(currentCityId);
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
            else
            {
                loadConfigArray();
            }
            currentSaveVersion.value = majorVersion;
            toggleOverrideDefaultLineInfoPanel(m_savedOverrideDefaultLineInfoPanel.value);
            currentSelectedConfigEditor = currentCityId;
            instance = this;
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

        private TLMConfigWarehouse currentConfigWarehouseEditor
        {
            get
            {
                return TLMConfigWarehouse.getConfig(currentSelectedConfigEditor);
            }
        }

        private void loadConfigArray()
        {

        }
        private void convertSavegame3_0()
        {
            TLMUtils.doLog("Converting old save from 3.0");
            var globalConfigArray = TLMConfigWarehouse.getConfig(TLMConfigWarehouse.GLOBAL_CONFIG_INDEX);

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

        private UIDropDown editorSelector;
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown> dropDowns = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox> checkBoxes = new Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox>();

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
            helper.AddDropdown("Show Configurations For", getOptionsForLoadConfig(), 0, reloadData);
            TLMUtils.doLog("Loading Group 1");
            UIHelperExtension group1 = helper.AddGroupExtended("Line Naming Strategy");
            ((UIPanel)group1.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group1.self).wrapLayout = true;


            generateCheckboxConfig(group1, "Auto coloring enabled", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group1, "Auto naming enabled", TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED);
            generateCheckboxConfig(group1, "Use 'Circular' word on single district lines", TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE);

            group1.AddSpace(20);
            group1.AddLabel("Buses Config");

            generateDropdownStringValueConfig(group1, "Default Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_MAIN);
            generateDropdownStringValueConfig(group1, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_SUBLINE);

            generateDropdownConfig(group1, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.BUS_PREFIX);
            generateDropdownConfig(group1, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.BUS_SEPARATOR);
            generateDropdownConfig(group1, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.BUS_SUFFIX);


            generateCheckboxConfig(group1, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.BUS_LEADING_ZEROS);
            generateCheckboxConfig(group1, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group1, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.BUS_PALETTE_PREFIX_BASED);

            group1.AddSpace(30);
            group1.AddLabel("Metro Config");
            generateDropdownStringValueConfig(group1, "Prefix Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_MAIN);
            generateDropdownStringValueConfig(group1, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_SUBLINE);

            generateDropdownConfig(group1, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.METRO_PREFIX);
            generateDropdownConfig(group1, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.METRO_SEPARATOR);
            generateDropdownConfig(group1, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.METRO_SUFFIX);


            generateCheckboxConfig(group1, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.METRO_LEADING_ZEROS);
            generateCheckboxConfig(group1, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group1, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.METRO_PALETTE_PREFIX_BASED);

            group1.AddSpace(30);
            group1.AddLabel("Train Config");

            generateDropdownStringValueConfig(group1, "Prefix Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_MAIN);
            generateDropdownStringValueConfig(group1, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_SUBLINE);

            generateDropdownConfig(group1, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.TRAIN_PREFIX);
            generateDropdownConfig(group1, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.TRAIN_SEPARATOR);
            generateDropdownConfig(group1, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.TRAIN_SUFFIX);


            generateCheckboxConfig(group1, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.TRAIN_LEADING_ZEROS);
            generateCheckboxConfig(group1, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group1, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.TRAIN_PALETTE_PREFIX_BASED);

            group1.AddSpace(30);
            group1.AddLabel("Tram Config");

            generateDropdownStringValueConfig(group1, "Prefix Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_MAIN);
            generateDropdownStringValueConfig(group1, "Secondary Palette", TLMAutoColorPalettes.paletteList, TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_SUBLINE);

            generateDropdownConfig(group1, "Prefix", namingOptionsPrefixo, TLMConfigWarehouse.ConfigIndex.TRAM_PREFIX);
            generateDropdownConfig(group1, "Separator", namingOptionsSeparador, TLMConfigWarehouse.ConfigIndex.TRAM_SEPARATOR);
            generateDropdownConfig(group1, "Identifier", namingOptionsSufixo, TLMConfigWarehouse.ConfigIndex.TRAM_SUFFIX);


            generateCheckboxConfig(group1, "Leading zeros (when prefix is used)", TLMConfigWarehouse.ConfigIndex.TRAM_LEADING_ZEROS);
            generateCheckboxConfig(group1, "Random colors on palette overflow", TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_RANDOM_ON_OVERFLOW);
            generateCheckboxConfig(group1, "Auto color based on prefix", TLMConfigWarehouse.ConfigIndex.TRAM_PALETTE_PREFIX_BASED);

            TLMUtils.doLog("Loading Group 2");

            UIHelperExtension group2 = helper.AddGroupExtended("Linear map line intersections");
            generateCheckboxConfig(group2, "Show metro line", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group2, "Show train/tram lines", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group2, "Show bus lines", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group2, "Show seaports", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group2, "Show airports", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group2, "Show taxi stops (AD only)", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);

            group2.AddSpace(20);
            group2.AddCheckbox("Show near lines in public services buildings' world info panel", m_savedShowNearLinesInCityServicesWorldInfoPanel.value, toggleShowNearLinesInCityServicesWorldInfoPanel);
            group2.AddCheckbox("Show near lines in zoned buildings' world info panel", m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value, toggleShowNearLinesInZonedBuildingWorldInfoPanel);

            TLMUtils.doLog("Loading Group 3");
            UIHelperExtension group3 = helper.AddGroupExtended("Custom palettes config [" + UIHelperExtension.version + "]");
            ((group3.self) as UIPanel).autoLayoutDirection = LayoutDirection.Horizontal;
            ((group3.self) as UIPanel).wrapLayout = true;

            UITextField paletteName = null;
            DropDownColorSelector colorEditor = null;
            NumberedColorList colorList = null;

            editorSelector = group3.AddDropdown("Palette Select", TLMAutoColorPalettes.paletteListForEditing, 0, delegate (int sel)
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

            group3.AddButton("Create", delegate ()
            {
                string newName = TLMAutoColorPalettes.addPalette();
                updateDropDowns("", "");
                editorSelector.selectedValue = newName;
            });
            group3.AddButton("Delete", delegate ()
            {
                TLMAutoColorPalettes.removePalette(editorSelector.selectedValue);
                updateDropDowns("", "");
            });
            paletteName = group3.AddTextField("Palette Name", delegate (string val)
            {

            }, "", (string value) =>
            {
                string oldName = editorSelector.selectedValue;
                paletteName.text = TLMAutoColorPalettes.renamePalette(oldName, value);
                updateDropDowns(oldName, value);
            });
            paletteName.parent.width = 500;

            colorEditor = group3.AddColorField("Colors", Color.black, delegate (Color c)
            {
                TLMAutoColorPalettes.setColor(colorEditor.id, editorSelector.selectedValue, c);
                colorList.colorList = TLMAutoColorPalettes.getColors(editorSelector.selectedValue);
            }, delegate
            {
                TLMAutoColorPalettes.removeColor(editorSelector.selectedValue, colorEditor.id);
                colorList.colorList = TLMAutoColorPalettes.getColors(editorSelector.selectedValue);
            });

            colorList = group3.AddNumberedColorList(null, new List<Color32>(), delegate (int c)
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
            dropDowns[configIndex] = group.AddDropdown(title, options, currentConfigWarehouseEditor.getString(configIndex), delegate (int i) { currentConfigWarehouseEditor.setString(configIndex, dropDowns[configIndex].items[i]); });
            return dropDowns[configIndex];
        }

        private void reloadData(int selection)
        {

        }

        private string[] getOptionsForLoadConfig()
        {
            if (currentCityId == TLMConfigWarehouse.GLOBAL_CONFIG_INDEX)
            {
                return new string[] { TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };
            }
            else return new string[] { currentCityId, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX };
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
