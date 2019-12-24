using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow.Components;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.LineDetailWindow.Components;
using Klyte.TransportLinesManager.OptionsMenu.Tabs;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow
{

    internal abstract class TLMTabControllerPrefixList<T> : UICustomControl, IBudgetControlParentInterface where T : TLMSysDef<T>
    {
        public UIScrollablePanel mainPanel { get; private set; }
        public UIHelperExtension m_uiHelper;
        private static TLMTabControllerPrefixList<T> m_instance;
        public static TLMTabControllerPrefixList<T> instance => m_instance;

        private UIDropDown m_prefixSelector;
        private UIHelperExtension m_subpanel;
        private UIPanel m_panelAssetSelector;

        private UIColorField m_prefixColor;
        private UITextField m_prefixName;
        private UICheckBox m_useColorForModel;
        private UITextField m_prefixTicketPrice;

        private UIDropDown m_paletteDD;
        private UIDropDown m_formatDD;
        private TLMBudgetControlSliders m_budgetSliders;

        private TLMAssetSelectorWindowPrefixTab<T> m_assetSelectorWindow;

        public static OnPrefixLoad eventOnPrefixChange;
        public static OnColorChanged eventOnColorChange;
        public int SelectedPrefix => m_prefixSelector?.selectedIndex ?? -1;


        private static ITLMTransportTypeExtension extension => Singleton<T>.instance.GetTSD().GetTransportExtension();

        private static Type ImplClassChildren => ReflectionUtils.GetImplementationForGenericType(typeof(TLMAssetSelectorWindowPrefixTab<>), typeof(T));

        public ushort CurrentSelectedId => (ushort) SelectedPrefix;

        public bool PrefixSelectionMode => true;


        public TransportSystemDefinition TransportSystem => Singleton<T>.instance.GetTSD();


        #region Awake
        private void Awake()
        {
            if (m_instance != null)
            {
                throw new Exception("MULTIPLE INSTANTIATION!!!!!!!!");
            }

            m_instance = this;
            mainPanel = GetComponentInChildren<UIScrollablePanel>();
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_uiHelper = new UIHelperExtension(mainPanel);

            TLMUtils.doLog("PrefixDD");

            m_prefixSelector = m_uiHelper.AddDropdownLocalized("K45_TLM_PREFIX", new string[0], -1, onChangePrefix);

            ReloadPrefixOptions();
            TLMUtils.doLog("PrefixDD Panel");
            UIPanel ddPanel = m_prefixSelector.GetComponentInParent<UIPanel>();
            ConfigComponentPanel(m_prefixSelector);


            TLMUtils.doLog("SubPanel");
            KlyteMonoUtils.CreateUIElement(out UIPanel subpanel, mainPanel.transform, "Subpanel", new Vector4(0, 0, 500, 180));
            subpanel.autoLayout = true;
            subpanel.autoLayoutDirection = LayoutDirection.Vertical;
            subpanel.autoSize = true;
            subpanel.autoFitChildrenVertically = true;
            m_subpanel = new UIHelperExtension(subpanel);

            TLMUtils.doLog("AssetSelector");
            KlyteMonoUtils.CreateUIElement(out m_panelAssetSelector, mainPanel.transform, "AssetSelector", new Vector4(0, 0, 0, 0.0001f));
            m_assetSelectorWindow = KlyteMonoUtils.CreateElement(ImplClassChildren, m_panelAssetSelector.transform).GetComponent<TLMAssetSelectorWindowPrefixTab<T>>();

            m_subpanel.Self.isVisible = false;
            m_assetSelectorWindow.mainPanel.isVisible = false;

            TLMUtils.doLog("Name");
            m_prefixName = m_subpanel.AddTextField(Locale.Get("K45_TLM_PREFIX_NAME"), null, "", onPrefixNameChange);
            ConfigComponentPanel(m_prefixName);

            TLMUtils.doLog("Price");
            m_prefixTicketPrice = m_subpanel.AddTextField(Locale.Get("K45_TLM_TICKET_PRICE_LABEL"), null, "", onTicketChange);
            ConfigComponentPanel(m_prefixTicketPrice);

            TLMUtils.doLog("ColorForModel");
            m_useColorForModel = m_subpanel.AddCheckboxLocale("K45_TLM_USE_PREFIX_COLOR_FOR_VEHICLE", false, onUseColorVehicleChange);
            KlyteMonoUtils.LimitWidth(m_useColorForModel.label, 420, true);

            TLMUtils.doLog("ColorSel");
            CreateColorSelector();

            TLMUtils.doLog("Budget");
            KlyteMonoUtils.CreateUIElement(out UIPanel m_budgetPanel, subpanel.transform, "BudgetPanel", new Vector4(0, 0, 460, 180));
            CreateBudgetSliders(m_budgetPanel);
            m_budgetPanel.relativePosition = new Vector3(550, 0);

            TLMUtils.doLog("Palette");
            m_paletteDD = m_subpanel.AddDropdownLocalized("K45_TLM_PALETTE", new string[0], -1, SetPalettePrefix);

            TLMUtils.doLog("PaletteDD Panel");
            UIPanel palettePanel = m_paletteDD.GetComponentInParent<UIPanel>();
            ConfigComponentPanel(m_paletteDD);
            reloadPalettes();
            TLMPaletteOptionsTab.onPaletteReloaded += reloadPalettes;

            TLMUtils.doLog("Format");
            m_formatDD = m_subpanel.AddDropdownLocalized("K45_TLM_ICON", TLMLineIconExtension.getDropDownOptions(Locale.Get("K45_TLM_LINE_ICON_ENUM_TT_DEFAULT")), -1, SetFormatPrefix);
            ConfigComponentPanel(m_formatDD);


            GetComponent<UIComponent>().eventVisibilityChanged += (x, y) => forceRefresh();
            TLMConfigWarehouse.EventOnPropertyChanged += OnWarehouseChange;
        }
        private void SetPalettePrefix(int value) => extension.SetCustomPalette((uint) SelectedPrefix, value == 0 ? null : m_paletteDD.selectedValue);
        private void SetFormatPrefix(int value) => extension.SetCustomFormat((uint) SelectedPrefix, (LineIconSpriteNames) Enum.Parse(typeof(LineIconSpriteNames), value.ToString()));

        private void OnWarehouseChange(TLMConfigWarehouse.ConfigIndex idx, bool? newValueBool, int? newValueInt, string newValueString)
        {
            if (idx == (TransportSystem.ToConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX))
            {
                ReloadPrefixOptions();
            }
        }

        private void reloadPalettes()
        {
            string valSel = (m_paletteDD.selectedValue);
            m_paletteDD.items = TLMAutoColorPalettes.paletteList;
            if (!m_paletteDD.items.Contains(valSel))
            {
                valSel = TLMAutoColorPalettes.PALETTE_RANDOM;
            }
            m_paletteDD.selectedIndex = m_paletteDD.items.ToList().IndexOf(valSel);
            m_paletteDD.items[0] = "-------------------";
        }

        private void CreateBudgetSliders(UIPanel reference)
        {
            TLMUtils.doLog("SLIDERS");
            KlyteMonoUtils.CreateElement(out m_budgetSliders, reference.transform, "Budget Sliders");
        }

        private void ConfigComponentPanel(UIComponent reference)
        {
            reference.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
            reference.GetComponentInParent<UIPanel>().wrapLayout = false;
            reference.GetComponentInParent<UIPanel>().autoLayout = false;
            reference.GetComponentInParent<UIPanel>().height = 40;
            KlyteMonoUtils.CreateUIElement(out UIPanel labelContainer, reference.parent.transform, "lblContainer", new Vector4(0, 0, 240, reference.height));
            labelContainer.zOrder = 0;
            UILabel lbl = reference.parent.GetComponentInChildren<UILabel>();
            lbl.transform.SetParent(labelContainer.transform);
            lbl.textAlignment = UIHorizontalAlignment.Center;
            lbl.minimumSize = new Vector2(240, reference.height);
            KlyteMonoUtils.LimitWidth(lbl, 240);
            lbl.verticalAlignment = UIVerticalAlignment.Middle;
            lbl.pivot = UIPivotPoint.TopCenter;
            lbl.relativePosition = new Vector3(0, lbl.relativePosition.y);
            reference.relativePosition = new Vector3(240, 0);
        }

        private void ReloadPrefixOptions()
        {
            int selIdx = m_prefixSelector.selectedIndex;
            m_prefixSelector.items = TLMUtils.getStringOptionsForPrefix(Singleton<T>.instance.GetTSD().ToConfigIndex(), true, true, false);
            m_prefixSelector.selectedIndex = selIdx;
        }

        private void CreateColorSelector()
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel panelColorSelector, m_subpanel.Self.transform, "ColorSelector", new Vector4(500, 60, 0, 0));
            panelColorSelector.autoLayout = true;
            panelColorSelector.autoLayoutDirection = LayoutDirection.Horizontal;
            panelColorSelector.autoLayoutPadding = new RectOffset(3, 3, 0, 0);
            panelColorSelector.autoFitChildrenHorizontally = true;
            panelColorSelector.autoFitChildrenVertically = true;

            KlyteMonoUtils.CreateUIElement(out UILabel lbl, panelColorSelector.transform, "PrefixColorLabel", new Vector4(5, 12, 250, 40));
            KlyteMonoUtils.LimitWidth(lbl, 250, true);
            lbl.localeID = "K45_TLM_PREFIX_COLOR_LABEL";
            lbl.verticalAlignment = UIVerticalAlignment.Middle;
            lbl.font = UIHelperExtension.defaultFontCheckbox;

            m_prefixColor = KlyteMonoUtils.CreateColorField(panelColorSelector);
            m_prefixColor.eventSelectedColorChanged += onChangePrefixColor;

            KlyteMonoUtils.CreateUIElement(out UIButton resetColor, panelColorSelector.transform, "PrefixColorReset", new Vector4(290, 0, 0, 0));
            KlyteMonoUtils.InitButton(resetColor, false, "ButtonMenu");
            KlyteMonoUtils.LimitWidth(resetColor, 200);
            resetColor.textPadding = new RectOffset(5, 5, 5, 2);
            resetColor.autoSize = true;
            resetColor.localeID = "K45_TLM_RESET_COLOR";
            resetColor.eventClick += onResetColor;
        }
        #endregion
        #region Actions
        private void onResetColor(UIComponent component, UIMouseEventParameter eventParam) => m_prefixColor.selectedColor = Color.clear;
        private void onChangePrefixColor(UIComponent component, Color selectedColor)
        {
            if (!isChanging && m_prefixSelector.selectedIndex >= 0)
            {
                extension.SetColor((uint) m_prefixSelector.selectedIndex, selectedColor);
            }
            eventOnColorChange?.Invoke(selectedColor);
        }
        private void onUseColorVehicleChange(bool val)
        {
            if (!isChanging && m_prefixSelector.selectedIndex >= 0)
            {
                extension.SetUsingColorForModel((uint) m_prefixSelector.selectedIndex, val);
            }
        }
        private void onPrefixNameChange(string val)
        {
            if (!isChanging && m_prefixSelector.selectedIndex >= 0)
            {
                extension.SetName((uint) m_prefixSelector.selectedIndex, val);
                ReloadPrefixOptions();
            }
        }
        private void onTicketChange(string val)
        {
            if (!isChanging && m_prefixSelector.selectedIndex >= 0 && uint.TryParse(val, out uint intVal))
            {
                extension.SetTicketPrice((uint) m_prefixSelector.selectedIndex, intVal);
            }
        }

        #endregion

        #region On Selection Changed
        private bool isChanging = false;

        public event OnItemSelectedChanged onSelectionChanged;

        private void onChangePrefix(int sel)
        {
            isChanging = true;
            m_subpanel.Self.isVisible = sel >= 0;
            m_assetSelectorWindow.mainPanel.isVisible = sel >= 0;
            if (sel >= 0)
            {
                m_prefixColor.selectedColor = extension.GetColor((uint) sel);
                m_useColorForModel.isChecked = extension.IsUsingColorForModel((uint) sel);
                m_prefixTicketPrice.text = extension.GetTicketPrice((uint) sel).ToString();
                m_prefixName.text = extension.GetName((uint) sel) ?? "";
                m_paletteDD.selectedIndex = Math.Max(0, m_paletteDD.items.ToList().IndexOf(extension.GetCustomPalette((uint) sel)));
                m_formatDD.selectedIndex = Math.Max(0, (int) extension.GetCustomFormat((uint) sel));

                eventOnPrefixChange?.Invoke(sel);
                onSelectionChanged?.Invoke();
            }
            isChanging = false;
        }

        public void forceRefresh() => onChangePrefix(SelectedPrefix);
        #endregion

        private void Start()
        {
        }

        private void Update()
        {

        }

    }

    internal sealed class TLMTabControllerPrefixListNorBus : TLMTabControllerPrefixList<TLMSysDefNorBus> { }
    internal sealed class TLMTabControllerPrefixListNorTrm : TLMTabControllerPrefixList<TLMSysDefNorTrm> { }
    internal sealed class TLMTabControllerPrefixListNorMnr : TLMTabControllerPrefixList<TLMSysDefNorMnr> { }
    internal sealed class TLMTabControllerPrefixListNorMet : TLMTabControllerPrefixList<TLMSysDefNorMet> { }
    internal sealed class TLMTabControllerPrefixListNorTrn : TLMTabControllerPrefixList<TLMSysDefNorTrn> { }
    internal sealed class TLMTabControllerPrefixListNorFer : TLMTabControllerPrefixList<TLMSysDefNorFer> { }
    internal sealed class TLMTabControllerPrefixListNorBlp : TLMTabControllerPrefixList<TLMSysDefNorBlp> { }
    internal sealed class TLMTabControllerPrefixListNorShp : TLMTabControllerPrefixList<TLMSysDefNorShp> { }
    internal sealed class TLMTabControllerPrefixListNorPln : TLMTabControllerPrefixList<TLMSysDefNorPln> { }
    internal sealed class TLMTabControllerPrefixListTouBus : TLMTabControllerPrefixList<TLMSysDefTouBus> { }

    public delegate void OnPrefixLoad(int prefix);
    public delegate void OnColorChanged(Color newColor);
}
