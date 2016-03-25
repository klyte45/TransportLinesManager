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
using Klyte.TransportLinesManager.MapDrawer;

[assembly: AssemblyVersion("5.1.1.*")]
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
        private SavedString m_savedPalettes;


        private UIDropDown editorSelector;
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown> dropDowns = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIDropDown>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox> checkBoxes = new Dictionary<TLMConfigWarehouse.ConfigIndex, UICheckBox>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField> textFields = new Dictionary<TLMConfigWarehouse.ConfigIndex, UITextField>();
        private Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel> lineTypesPanels = new Dictionary<TLMConfigWarehouse.ConfigIndex, UIPanel>();
        private UIDropDown configSelector;
        private UIPanel assetSelection;
        private UICheckBox overrideWorldInfoPanelLineOption;

        public bool needShowPopup;

        private string currentSelectedConfigEditor
        {
            get
            {
                return configSelector.selectedIndex == 0 ? currentCityId : TLMConfigWarehouse.GLOBAL_CONFIG_INDEX;
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
            m_IPTCompatibilityMode = new SavedBool("TLM_IPTCompabilityMode", Settings.gameSettingsFile, false, true);
            m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
            m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
            m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
            m_debugMode = new SavedBool("TLMdebugMode", Settings.gameSettingsFile, false, true);
            m_betaMapGen = new SavedBool("TLMbetaMapGen", Settings.gameSettingsFile, false, true);

            if (m_debugMode.value) TLMUtils.doLog("currentSaveVersion.value = {0}, fullVersion = {1}", currentSaveVersion.value, fullVersion);
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
                        if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }

        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Options");
            string[] namingOptionsSufixo = new string[] {
                "Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic"
            };
            string[] namingOptionsPrefixo = new string[] {
                "Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic", "None","Number + Lower Latin","Number + Upper Latin","Number + Lower Greek", "Number + Upper Greek", "Number + Lower Cyrilic", "Number + Upper Cyrilic"
            };
            string[] namingOptionsSeparador = new string[] {
                "<None>","-",".","/", "<Blank Space>","<New Line>"
            };
            UIDropDown systemTypeDropDown = null;
            UIHelperExtension group2sub = null;
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
                assetSelection.isVisible = !value;
                overrideWorldInfoPanelLineOption.isVisible = !value;
                m_IPTCompatibilityMode.value = value;
            };

            helper.AddCheckbox("IPT compatibility mode (Needs restart)", m_IPTCompatibilityMode.value, iptToggle);
            overrideWorldInfoPanelLineOption = (UICheckBox)helper.AddCheckbox("Override default line info panel", m_savedOverrideDefaultLineInfoPanel.value, toggleOverrideDefaultLineInfoPanel);



            helper.AddSpace(10);

            configSelector = (UIDropDown)helper.AddDropdown("Show Configurations For", getOptionsForLoadConfig(), 0, reloadData);
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Group 1");
            foreach (TLMConfigWarehouse.ConfigIndex transportType in new TLMConfigWarehouse.ConfigIndex[] { TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG, TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG, TLMConfigWarehouse.ConfigIndex.BUS_CONFIG, TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG, TLMConfigWarehouse.ConfigIndex.METRO_CONFIG, TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG })
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
                UIDropDown suffixDD = generateDropdownConfig(group1, "Suffix", namingOptionsSufixo, transportType | TLMConfigWarehouse.ConfigIndex.SUFFIX);
                var suffixDDContainer = suffixDD.transform.parent.GetComponent<UIPanel>();
                UIDropDown nonPrefixDD = generateDropdownConfig(group1, "Identifier (for non-prefixed)", namingOptionsSufixo, transportType | TLMConfigWarehouse.ConfigIndex.NON_PREFIX);
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
                    suffixDDContainer.isVisible = isPrefixed;
                    zerosContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero;
                    prefixAsSuffixContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero && (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Numero;
                    autoColorBasedContainer.isVisible = isPrefixed;
                    paletteLabel.text = isPrefixed ? "Palette for Unprefixed" : "Palette";
                    if (systemTypeDropDown != null)
                    {
                        systemTypeDropDown.selectedIndex = 0;
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

            UIHelperExtension group7 = helper.AddGroupExtended("Near Lines Config");
            generateCheckboxConfig(group7, "Show regular bus lines", TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show tram line", TLMConfigWarehouse.ConfigIndex.TRAM_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show metro line", TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show train lines", TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show seaports", TLMConfigWarehouse.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show airports", TLMConfigWarehouse.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP);
            generateCheckboxConfig(group7, "Show taxi stops (AD only)", TLMConfigWarehouse.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP);

            UIHelperExtension group8 = helper.AddGroupExtended("Automation");
            generateCheckboxConfig(group8, "Auto coloring enabled", TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            generateCheckboxConfig(group8, "Auto naming enabled", TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED);
            generateCheckboxConfig(group8, "Use 'Circular' word on single district lines", TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE);
            generateCheckboxConfig(group8, "Add line number inside brackets in generated auto name", TLMConfigWarehouse.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME);

            UIHelperExtension group13 = helper.AddGroupExtended("Auto Naming Settings - Public Transport Buildings");
            ((UIPanel)group13.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group13.self).wrapLayout = true;
            ((UIPanel)group13.self).width = 730;

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


            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Group 2");

            UIHelperExtension group2 = helper.AddGroupExtended("City Assets Selection");
            assetSelection = group2.self.GetComponentInParent<UIPanel>();
            if (isCityLoaded)
            {
                ((UIPanel)group2.self).autoLayoutDirection = LayoutDirection.Horizontal;
                ((UIPanel)group2.self).autoLayoutPadding = new RectOffset(5, 5, 0, 0);
                ((UIPanel)group2.self).wrapLayout = true;
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
                UIDropDown prefixSelection = null;
                TextList<string> defaultAssets = null;
                TextList<string> prefixAssets = null;
                UITextField prefixName = null;
                OnDropdownSelectionChanged loadPrefixAssetList = (int sel) =>
                {
                    if (sel == 0 || systemTypeDropDown.selectedIndex == 0)
                    {
                        ((UIPanel)group2sub.self).enabled = false;
                        return;
                    }
                    ((UIPanel)group2sub.self).enabled = true;
                    prefixName.text = getPrefixNameFromDropDownSelection(systemTypeDropDown.selectedIndex, (uint)(sel - 1), configSelector.selectedIndex == 1);
                    prefixAssets.itemsList = getPrefixAssetListFromDropDownSelection(systemTypeDropDown.selectedIndex, (uint)(sel - 1), configSelector.selectedIndex == 1);
                    var t = getBasicAssetListFromDropDownSelection(systemTypeDropDown.selectedIndex, configSelector.selectedIndex == 1);
                    defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(systemTypeDropDown.selectedIndex, configSelector.selectedIndex == 1).Where(k => !prefixAssets.itemsList.ContainsKey(k.Key)).ToDictionary(k => k.Key, k => k.Value);
                };
                OnDropdownSelectionChanged loadPrefixes = (int sel) =>
                {
                    if (sel == 0)
                    {
                        prefixSelection.isVisible = false;
                        prefixSelection.selectedIndex = 0;
                        return;
                    }
                    prefixSelection.isVisible = true;
                    ((UIPanel)group2sub.self).enabled = false;
                    TLMConfigWarehouse.ConfigIndex transportIndex = getConfigIndexFromDropDownSelection(sel);
                    defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(systemTypeDropDown.selectedIndex, configSelector.selectedIndex == 1);
                    defaultAssets.root.color = TLMConfigWarehouse.getColorForTransportType(transportIndex);
                    var m = (ModoNomenclatura)TLMConfigWarehouse.getCurrentConfigInt(transportIndex | TLMConfigWarehouse.ConfigIndex.PREFIX);
                    prefixSelection.items = TLMUtils.getStringOptionsForPrefix(m, true);
                    prefixSelection.selectedIndex = 0;
                };
                systemTypeDropDown = (UIDropDown)group2.AddDropdown("Transport System", new string[] { "--Select--", "Ship", "Train", "Tram", "Bus", "Plane" }, 0, loadPrefixes);
                prefixSelection = (UIDropDown)group2.AddDropdown("Prefix", new string[] { "" }, 0, loadPrefixAssetList);

                foreach (Transform t in ((UIPanel)group2.self).transform)
                {
                    var panel = t.gameObject.GetComponent<UIPanel>();
                    if (panel)
                    {
                        panel.width = 340;
                    }
                }

                group2sub = group2.AddGroupExtended("Details");

                ((UIPanel)group2sub.self).autoLayoutDirection = LayoutDirection.Horizontal;
                ((UIPanel)group2sub.self).autoLayoutPadding = new RectOffset(2, 2, 0, 0);
                ((UIPanel)group2sub.self).wrapLayout = true;
                ((UIPanel)group2sub.self).width = 720;
                ((UIPanel)group2sub.self).padding = new RectOffset(0, 0, 0, 0);

                prefixName = group2sub.AddTextField("Prefix Name", delegate (string s) { setPrefixNameDropDownSelection(systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), s, configSelector.selectedIndex == 1); });

                defaultAssets = group2sub.AddTextList("Default Assets", new Dictionary<string, string>(), delegate (string idx) { reloadTexture(idx); }, 340, 250);
                prefixAssets = group2sub.AddTextList("Assets for this prefix", new Dictionary<string, string>(), delegate (string idx) { reloadTexture(idx); }, 340, 250);
                foreach (Transform t in ((UIPanel)group2sub.self).transform)
                {
                    var panel = t.gameObject.GetComponent<UIPanel>();
                    if (panel)
                    {
                        panel.width = 340;
                    }
                }

                prefixAssets.root.backgroundSprite = "EmptySprite";
                prefixAssets.root.color = Color.white;
                prefixAssets.root.width = 340;
                defaultAssets.root.backgroundSprite = "EmptySprite";
                defaultAssets.root.width = 340;

                prefixName.GetComponentInParent<UIPanel>().width = 720;
                prefixName.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
                prefixName.GetComponentInParent<UIPanel>().autoLayoutPadding = new RectOffset(5, 5, 3, 3);
                prefixName.GetComponentInParent<UIPanel>().wrapLayout = true;
                group2sub.AddSpace(10);
                OnButtonClicked reload = delegate
                {
                    loadPrefixAssetList(prefixSelection.selectedIndex);
                };
                group2sub.AddButton("Add To Prefix", delegate
                {
                    if (defaultAssets.unselected) return;
                    var selected = defaultAssets.selectedItem;
                    if (selected == null || selected.Equals(default(string))) return;
                    addAssetToPrefixDropDownSelection(systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), selected, configSelector.selectedIndex == 1);
                    reload();
                });
                group2sub.AddButton("Remove From Prefix", delegate
                {
                    if (prefixAssets.unselected) return;
                    var selected = prefixAssets.selectedItem;
                    if (selected == null || selected.Equals(default(string))) return;
                    removeAssetFromPrefixDropDownSelection(systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), selected, configSelector.selectedIndex == 1);
                    reload();
                });

                group2sub.AddButton("Remove All", delegate
                {
                    removeAllAssetsFromPrefixDropDownSelection(systemTypeDropDown.selectedIndex, (uint)(prefixSelection.selectedIndex - 1), configSelector.selectedIndex == 1);
                    reload();
                });
                group2sub.AddButton("Reload", delegate
                {
                    reload();
                });
                ((UIPanel)group2sub.self).enabled = false;
                prefixSelection.isVisible = false;
            }
            else
            {
                group2.AddLabel("Please load a city to get access to active assets!");
            }

            UIHelperExtension group5 = helper.AddGroupExtended("Other Global Options");
            group5.AddSpace(20);
            group5.AddCheckbox("Show near lines in public services buildings' world info panel", m_savedShowNearLinesInCityServicesWorldInfoPanel.value, toggleShowNearLinesInCityServicesWorldInfoPanel);
            group5.AddCheckbox("Show near lines in zoned buildings' world info panel", m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value, toggleShowNearLinesInZonedBuildingWorldInfoPanel);

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Group 3");
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

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Group 3½");
            paletteName.enabled = false;
            colorEditor.Disable();
            colorList.Disable();
            iptToggle.Invoke(isIPTCompatibiltyMode);

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading Group 4");
            UIHelperExtension group9 = helper.AddGroupExtended("Betas & Extra Info");

            group9.AddButton("Draw city map! (alpha)", TLMMapDrawer.drawCityMap);
            group9.AddCheckbox("Debug mode", m_debugMode.value, delegate (bool val) { m_debugMode.value = val; });
            group9.AddLabel("Version: " + version + " rev" + typeof(TransportLinesManagerMod).Assembly.GetName().Version.Revision);
            group9.AddButton("Release notes for this version", delegate () { showVersionInfoPopup(true); });
            UIHelperExtension group10 = helper.AddGroupExtended("Help");
            group10.AddLabel("Coming soon");



        }

        private TLMConfigWarehouse.ConfigIndex getConfigIndexFromDropDownSelection(int index)
        {
            switch (index)
            {
                case 1:
                    return TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG;
                case 2:
                    return TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG;
                case 3:
                    return TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG;
                case 4:
                    return TLMConfigWarehouse.ConfigIndex.BUS_CONFIG;
                case 5:
                    return TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG;
                default:
                    return TLMConfigWarehouse.ConfigIndex.NIL;
            }
        }



        private Dictionary<string, string> getBasicAssetListFromDropDownSelection(int index, bool global)
        {
            return TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).getBasicAssetsDictionary(global);

        }

        private Dictionary<string, string> getPrefixAssetListFromDropDownSelection(int index, uint prefix, bool global)
        {
            return TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).getBasicAssetsListForPrefix(prefix, global);
        }

        private void addAssetToPrefixDropDownSelection(int index, uint prefix, string assetId, bool global)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).addAssetToPrefixList(prefix, assetId, global);
        }

        private void removeAssetFromPrefixDropDownSelection(int index, uint prefix, string assetId, bool global)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).removeAssetFromPrefixList(prefix, assetId, global);
        }

        private void removeAllAssetsFromPrefixDropDownSelection(int index, uint prefix, bool global)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).removeAllAssetsFromPrefixList(prefix, global);
        }

        private void setPrefixNameDropDownSelection(int index, uint prefix, string name, bool global)
        {
            TLMUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).setPrefixName(prefix, name, global);
        }

        private string getPrefixNameFromDropDownSelection(int index, uint prefix, bool global)
        {
            return TLMUtils.getTransportSystemPrefixName(getConfigIndexFromDropDownSelection(index), prefix, global);
        }


        private T getSelectedIndex<T>(params TextList<T>[] boxes)
        {
            foreach (var box in boxes)
            {
                if (!box.unselected)
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("{0} is selected: {1}", box.name, box.selectedItem.ToString());
                    return box.selectedItem;
                }
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("{0} isn't selected", box.name);
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
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("OPÇÔES RECARREGANDO ARQUIVO", currentSelectedConfigEditor);
            foreach (var i in dropDowns)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
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
                            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("TIPO INVÁLIDO!", i);
                            break;
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("EXCEPTION! {0} | {1} | [{2}]", i.Key, currentConfigWarehouseEditor.getString(i.Key), string.Join(",", i.Value.items));
                }

            }
            foreach (var i in checkBoxes)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
                i.Value.isChecked = currentConfigWarehouseEditor.getBool(i.Key);
            }
            foreach (var i in textFields)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("OPÇÔES RECARREGANDO {0}", i);
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
            if (mode != LoadMode.LoadGame)
            {
                return;
            }
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
                  "DepotIcon","PlaneLineIcon","TramIcon","ShipLineIcon","LowBusIcon","HighBusIcon", "BulletTrainIcon","BusIcon","SubwayIcon","TrainIcon","SurfaceMetroIcon","ShipIcon","AirplaneIcon","TaxiIcon","DayIcon","NightIcon","DisabledIcon","SurfaceMetroImage","BulletTrainImage","LowBusImage","HighBusImage","VehicleLinearMap"
                });
            }
            if (!TransportLinesManagerMod.isIPTCompatibiltyMode)
            {
                TLMDepotAI.instance.EnableHooks();
                TLMTransportLineExtensionHooks.EnableHooks();
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
            if (!TransportLinesManagerMod.isIPTCompatibiltyMode)
            {
                //TLMTransportLineExtensionHooks.DisableHooks();
            }
            TLMPublicTransportDetailPanelHooks.instance.DisableHooks();
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
