using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.OptionsMenu.Tabs;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMPrefixOptionsTab : UICustomControl, IUVMPTWIPChild
    {
        private UILabel m_title;

        public UIPanel MainPanel { get; private set; }
        private UIHelperExtension m_helper;

        private UITextField m_prefixName;
        private UICheckBox m_useColorForModel;
        private UIDropDown m_paletteDD;
        private UIDropDown m_formatDD;
        private UIColorField m_prefixColor;

        private bool m_isLoading;
        private TransportSystemDefinition TransportSystem => TransportSystemDefinition.From(GetLineID());
        private ITLMTransportTypeExtension Extension => TransportSystem.GetTransportExtension();
        public uint SelectedPrefix => TLMLineUtils.GetPrefix(GetLineID());
        internal static ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        public void Awake()
        {
            MainPanel = GetComponent<UIPanel>();
            MainPanel.relativePosition = new Vector3(510f, 0.0f);
            MainPanel.width = 350;
            MainPanel.height = GetComponentInParent<UIComponent>().height;
            MainPanel.zOrder = 50;
            MainPanel.color = new Color32(255, 255, 255, 255);
            MainPanel.name = "AssetSelectorWindow";
            MainPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            MainPanel.autoLayout = true;
            MainPanel.autoLayoutDirection = LayoutDirection.Vertical;

            KlyteMonoUtils.CreateUIElement(out m_title, MainPanel.transform);
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.autoSize = false;
            m_title.autoHeight = true;
            m_title.width = MainPanel.width - 30f;
            m_title.relativePosition = new Vector3(5, 5);
            m_title.textScale = 0.9f;
            m_title.localeID = "K45_TLM_ASSETS_FOR_PREFIX";

            m_helper = new UIHelperExtension(MainPanel);

            TLMUtils.doLog("Name");
            m_prefixName = CreateMiniTextField("K45_TLM_PREFIX_NAME", OnPrefixNameChange);


            TLMUtils.doLog("ColorForModel");
            m_useColorForModel = m_helper.AddCheckboxLocale("K45_TLM_USE_PREFIX_COLOR_FOR_VEHICLE", false, OnUseColorVehicleChange);
            KlyteMonoUtils.LimitWidth(m_useColorForModel.label, 340, true);
            m_useColorForModel.label.textScale = 1;

            TLMUtils.doLog("ColorSel");
            CreateColorSelector();

            TLMUtils.doLog("Palette");
            m_paletteDD = CreateMiniDropdown("K45_TLM_PALETTE", SetPalettePrefix, new string[1]);
            ReloadPalettes();
            TLMPaletteOptionsTab.onPaletteReloaded += ReloadPalettes;

            TLMUtils.doLog("Format");
            m_formatDD = CreateMiniDropdown("K45_TLM_ICON", SetFormatPrefix, TLMLineIconExtension.getDropDownOptions(Locale.Get("K45_TLM_LINE_ICON_ENUM_TT_DEFAULT")));

        }

        private UIDropDown CreateMiniDropdown(string localeId, OnDropdownSelectionChanged onValueChanged, string[] values)
        {
            UIDropDown ddObj = UIHelperExtension.CloneBasicDropDownLocalized(localeId, values, onValueChanged, 0, MainPanel, out UILabel label, out UIPanel container);
            container.autoFitChildrenHorizontally = false;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayout = true;
            container.autoFitChildrenHorizontally = true;
            container.autoFitChildrenVertically = true;
            ReflectionUtils.GetEventField(typeof(UIDropDown), "eventMouseWheel")?.SetValue(ddObj, null);
            ddObj.isLocalized = false;
            ddObj.autoSize = false;
            ddObj.horizontalAlignment = UIHorizontalAlignment.Center;
            ddObj.itemPadding = new RectOffset(2, 2, 6, 6);
            ddObj.textFieldPadding = new RectOffset(4, 40, 4, 4);
            ddObj.name = localeId;
            ddObj.size = new Vector3(240, 22);
            ddObj.textScale = 0.8f;
            ddObj.listPosition = UIDropDown.PopupListPosition.Automatic;
            //KlyteMonoUtils.InitButtonFull(ddObj, false, "OptionsDropboxListbox");
            ddObj.horizontalAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.LimitWidthAndBox(label, 130);
            label.textScale = 1;
            label.padding.top = 4;
            label.position = Vector3.zero;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Left;

            return ddObj;
        }

        private UITextField CreateMiniTextField(string localeId, OnTextSubmitted onValueChanged)
        {
            UITextField ddObj = UIHelperExtension.AddTextfield(MainPanel, localeId, "", out UILabel label, out UIPanel container);
            container.autoFitChildrenHorizontally = false;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayout = true;
            container.autoFitChildrenHorizontally = true;
            container.autoFitChildrenVertically = true;

            ddObj.isLocalized = false;
            ddObj.autoSize = false;
            ddObj.eventTextSubmitted += (x, y) => onValueChanged(y);
            ddObj.name = localeId;
            ddObj.size = new Vector3(240, 22);
            ddObj.textScale = 1;


            KlyteMonoUtils.LimitWidthAndBox(label, 130);
            label.textScale = 1;
            label.padding.top = 4;
            label.position = Vector3.zero;
            label.isLocalized = true;
            label.localeID = localeId;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Left;

            return ddObj;
        }

        private void OnPrefixNameChange(string val)
        {
            if (!m_isLoading)
            {
                Extension.SetName(SelectedPrefix, val);
                UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
            }
        }
        private void OnUseColorVehicleChange(bool val)
        {
            if (!m_isLoading)
            {
                Extension.SetUsingColorForModel(SelectedPrefix, val);
            }
        }

        private void SetPalettePrefix(int value)
        {
            if (!m_isLoading)
            {
                Extension.SetCustomPalette(SelectedPrefix, value == 0 ? null : m_paletteDD.selectedValue);
            }
        }

        private void SetFormatPrefix(int value)
        {
            if (!m_isLoading)
            {
                Extension.SetCustomFormat(SelectedPrefix, (LineIconSpriteNames) Enum.Parse(typeof(LineIconSpriteNames), value.ToString()));
                UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
            }
        }

        private void ReloadPalettes()
        {
            m_isLoading = true;
            string valSel = (m_paletteDD.selectedValue);
            m_paletteDD.items = TLMAutoColorPalettes.paletteList;
            if (!m_paletteDD.items.Contains(valSel))
            {
                valSel = TLMAutoColorPalettes.PALETTE_RANDOM;
            }
            m_paletteDD.selectedIndex = m_paletteDD.items.ToList().IndexOf(valSel);
            m_paletteDD.items[0] = Locale.Get("K45_TLM_USE_DEFAULT_PALETTE");
            m_isLoading = false;
        }
        private void CreateColorSelector()
        {
            m_prefixColor = m_helper.AddColorPicker("A", Color.clear, OnChangePrefixColor, out UILabel lbl, out UIPanel container);

            KlyteMonoUtils.LimitWidthAndBox(lbl, 260, true);
            lbl.isLocalized = true;
            lbl.localeID = "K45_TLM_PREFIX_COLOR_LABEL";
            lbl.verticalAlignment = UIVerticalAlignment.Middle;
            lbl.font = UIHelperExtension.defaultFontCheckbox;
            lbl.textScale = 1;

            KlyteMonoUtils.CreateUIElement(out UIButton resetColor, container.transform, "PrefixColorReset", new Vector4(290, 0, 0, 0));
            KlyteMonoUtils.InitButton(resetColor, false, "ButtonMenu");
            KlyteMonoUtils.LimitWidth(resetColor, 80, true);
            resetColor.textPadding = new RectOffset(5, 5, 5, 2);
            resetColor.autoSize = true;
            resetColor.localeID = "K45_TLM_RESET_COLOR";
            resetColor.eventClick += OnResetColor;
        }
        private void OnResetColor(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_prefixColor.selectedColor = Color.clear;
            Extension.SetColor(SelectedPrefix, Color.clear);
        }

        private void OnChangePrefixColor(Color selectedColor) => Extension.SetColor(SelectedPrefix, selectedColor);
        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }
            m_isLoading = true;
            uint sel = SelectedPrefix;
            m_prefixColor.selectedColor = Extension.GetColor(sel);
            m_useColorForModel.isChecked = Extension.IsUsingColorForModel(sel);
            m_prefixName.text = Extension.GetName(sel) ?? "";
            m_paletteDD.selectedIndex = Math.Max(0, m_paletteDD.items.ToList().IndexOf(Extension.GetCustomPalette(sel)));
            m_formatDD.selectedIndex = Math.Max(0, (int) Extension.GetCustomFormat(sel));

            m_title.text = string.Format(Locale.Get("K45_TLM_PREFIX_TAB_WIP_TITLE"), sel > 0 ? NumberingUtils.GetStringFromNumber(TLMUtils.GetStringOptionsForPrefix(TransportSystem), (int) sel + 1) : Locale.Get("K45_TLM_UNPREFIXED"), TLMConfigWarehouse.getNameForTransportType(TransportSystem.ToConfigIndex()));

            m_isLoading = false;

        }
        public void UpdateBindings() { }
        public void OnEnable() { }
        public void OnDisable() { }
        public void OnGotFocus() { }
        public bool MayBeVisible() => TLMLineUtils.HasPrefix(GetLineID()) && !TLMTransportLineExtension.Instance.IsUsingCustomConfig(GetLineID());
        public void Hide() =>MainPanel.isVisible=false;
    }
}
