using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.OptionsMenu.Tabs;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.TransportLinesManager.OptionsMenu
{
    internal abstract class TLMShowConfigTab : UICustomControl, ITLMConfigOptionsTab
    {
        public UIPanel mainPanel { get; private set; }
        public UIHelperExtension m_uiHelper;

        public static TLMShowConfigTab instance { get; private set; }

        internal abstract TransportSystemDefinition TSD { get; }
                
        private UIDropDown m_paletteDD;
        private UICheckBox m_prefixIncrement;
        private UICheckBox m_zerosContainer;
        private UICheckBox m_prefixAsSuffixContainer;
        private UICheckBox m_autoColorBasedContainer;
        private UICheckBox m_randomPaletteOnOverflow;
        private UIDropDown m_suffixDD;
        private UIDropDown m_nonPrefixDD;
        private UIDropDown m_separatorDD;
        private UIDropDown m_prefixDD;
        private UITextField m_vehicleNumberFormatLocal;
        private UITextField m_vehicleNumberFormatForeign;
        private UICheckBox m_useInAutoName;
        private UITextField m_namingPrefix;
        private UILabel m_defaultCostLbl;
        private UITextField m_defaultCost;
        private UITextField m_defaultTicketPrice;

        private void Awake()
        {
            instance = this;
            mainPanel = GetComponent<UIPanel>();
            mainPanel.autoFitChildrenVertically = true;
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_uiHelper = new UIHelperExtension(mainPanel);

            m_uiHelper.AddLabel(string.Format(Locale.Get("K45_TLM_CONFIGS_FOR"), TSD.GetTransportName()));
            UIPanel panel = m_uiHelper.Self.GetComponentInParent<UIPanel>();
            ((UIPanel)m_uiHelper.Self).autoLayoutDirection = LayoutDirection.Vertical;
            ((UIPanel)m_uiHelper.Self).autoFitChildrenVertically = true;
            ((UIPanel)m_uiHelper.Self).backgroundSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_MenuPanel_color);
            ((UIPanel)m_uiHelper.Self).wrapLayout = true;
            ((UIPanel)m_uiHelper.Self).padding = new RectOffset(10, 10, 10, 15);
            ((UIPanel)m_uiHelper.Self).color = TSD.Color;
            ((UIPanel)m_uiHelper.Self).width = 730;
            m_uiHelper.AddSpace(30);


            AddDropdown(Locale.Get("K45_TLM_PREFIX"), out m_prefixDD, m_uiHelper, TLMConfigOptions.namingOptionsPrefix.Select(x => x.GetName()).ToArray(), (x) => TSD.GetConfig().Prefix = TLMConfigOptions.namingOptionsPrefix[x]);
            AddDropdown(Locale.Get("K45_TLM_SEPARATOR"), out m_separatorDD, m_uiHelper, TLMConfigOptions.namingOptionsSeparator.Select(x => x.GetName()).ToArray(), (x) => TSD.GetConfig().Separator = TLMConfigOptions.namingOptionsSeparator[x]);
            AddDropdown(Locale.Get("K45_TLM_SUFFIX"), out m_suffixDD, m_uiHelper, TLMConfigOptions.namingOptionsSuffix.Select(x => x.GetName()).ToArray(), (x) => TSD.GetConfig().Suffix = TLMConfigOptions.namingOptionsSuffix[x]);
            AddDropdown(Locale.Get("K45_TLM_IDENTIFIER_NON_PREFIXED"), out m_nonPrefixDD, m_uiHelper, TLMConfigOptions.namingOptionsSuffix.Select(x => x.GetName()).ToArray(), (x) => TSD.GetConfig().NonPrefixedNaming = TLMConfigOptions.namingOptionsSuffix[x]);
            AddDropdown(Locale.Get("K45_TLM_PALETTE"), out m_paletteDD, m_uiHelper, TLMAutoColorPalettes.paletteList, (x) => TSD.GetConfig().Palette = TLMAutoColorPalettes.paletteList[x]);
            AddDropdown(Locale.Get("K45_TLM_PALETTE"), out m_paletteDD, m_uiHelper, Enum.GetValues(typeof(LineIconSpriteNames)).OfType<LineIconSpriteNames>().Select(x => x.GetNameForTLM()).ToArray(), (x) => TSD.GetConfig().DefaultLineIcon = (LineIconSpriteNames)x);
            m_uiHelper.AddSpace(5);

            AddTextField(Locale.Get("K45_TLM_NUMBERVEHICLEPATTERN_LOCAL"), out m_vehicleNumberFormatLocal, m_uiHelper, (x) => TSD.GetConfig().VehicleIdentifierFormatLocal = x ?? "");
            AddTextField(Locale.Get("K45_TLM_NUMBERVEHICLEPATTERN_FOREIGN"), out m_vehicleNumberFormatForeign, m_uiHelper, (x) => TSD.GetConfig().VehicleIdentifierFormatForeign = x ?? "");
            m_uiHelper.AddSpace(5);

            AddCheckbox(Locale.Get("K45_TLM_USE_AUTO_NAME"), out m_useInAutoName, m_uiHelper, (x) => TSD.GetConfig().UseInAutoName = x);
            AddTextField(Locale.Get("K45_TLM_PREFIX_OPTIONAL"), out m_namingPrefix, m_uiHelper, (x) => TSD.GetConfig().DefaultTicketPrice = int.TryParse(x, out int val) ? val : 0, TSD.GetConfig().DefaultTicketPrice.ToString());
            m_uiHelper.AddSpace(5);

            AddTextField(Locale.Get("K45_TLM_COST_PER_PASSENGER_CONFIG"), out m_defaultCost, out m_defaultCostLbl, m_uiHelper, (x) => TSD.GetConfig().DefaultCostPerPassenger = int.TryParse(x, out int val) ? val : 0, TSD.GetConfig().DefaultCostPerPassenger.ToString());
            AddTextField(Locale.Get("K45_TLM_DEFAULT_PRICE"), out m_defaultTicketPrice, m_uiHelper, (x) => TSD.GetConfig().DefaultTicketPrice = int.TryParse(x, out int val) ? val : 0, TSD.GetConfig().DefaultTicketPrice.ToString());
            m_uiHelper.AddSpace(5);

            AddCheckboxLocale("K45_TLM_NEAR_LINES_SHOW", out m_zerosContainer, m_uiHelper, (x) => TSD.GetConfig().ShowInLinearMap = x);
            AddCheckboxLocale("K45_TLM_LEADING_ZEROS_SUFFIX", out m_zerosContainer, m_uiHelper, (x) => TSD.GetConfig().UseLeadingZeros = x);
            AddCheckboxLocale("K45_TLM_INVERT_PREFIX_SUFFIX_ORDER", out m_prefixAsSuffixContainer, m_uiHelper, (x) => TSD.GetConfig().InvertPrefixSuffix = x);
            AddCheckboxLocale("K45_TLM_RANDOM_ON_PALETTE_OVERFLOW", out m_randomPaletteOnOverflow, m_uiHelper, (x) => TSD.GetConfig().PaletteRandomOnOverflow = x);
            AddCheckboxLocale("K45_TLM_AUTO_COLOR_BASED_ON_PREFIX", out m_autoColorBasedContainer, m_uiHelper, (x) => TSD.GetConfig().PalettePrefixBased = x);
            AddCheckboxLocale("K45_TLM_LINENUMBERING_BASED_IN_PREFIX", out m_prefixIncrement, m_uiHelper, (x) => TSD.GetConfig().IncrementPrefixOnNewLine = x);

            AddButtonInEditorRow(m_vehicleNumberFormatLocal, CommonsSpriteNames.K45_QuestionMark, Help_VehicleNumberFormat, null, true, 30);
            AddButtonInEditorRow(m_vehicleNumberFormatForeign, CommonsSpriteNames.K45_QuestionMark, Help_VehicleNumberFormat, null, true, 30);

            m_prefixDD.eventSelectedIndexChanged += OnPrefixOptionChange;
            m_suffixDD.eventSelectedIndexChanged += OnSuffixOptionChange;
            OnPrefixOptionChange(m_prefixDD, m_prefixDD.selectedIndex);
        }

        private void Help_VehicleNumberFormat() => K45DialogControl.ShowModalHelp("Vehicles.NumberPattern", Locale.Get("K45_TLM_NUMBERINGVEHICLESHELP"), 0);

        private void OnPrefixOptionChange(UIComponent c, int sel)
        {
            bool isPrefixed = (NamingMode)sel != NamingMode.None;
            m_separatorDD.parent.isVisible = isPrefixed;
            m_prefixIncrement.isVisible = isPrefixed;
            m_suffixDD.parent.isVisible = isPrefixed;
            m_zerosContainer.isVisible = isPrefixed && (NamingMode)m_suffixDD.selectedIndex == NamingMode.Number;
            m_prefixAsSuffixContainer.isVisible = isPrefixed && (NamingMode)m_suffixDD.selectedIndex == NamingMode.Number && (NamingMode)m_prefixDD.selectedIndex != NamingMode.Number;
            m_autoColorBasedContainer.isVisible = isPrefixed;
        }

        private void OnSuffixOptionChange(UIComponent c, int sel)
        {
            bool isPrefixed = (NamingMode)m_prefixDD.selectedIndex != NamingMode.None;
            m_zerosContainer.isVisible = isPrefixed && (NamingMode)sel == NamingMode.Number;
            m_prefixAsSuffixContainer.isVisible = isPrefixed && (NamingMode)sel == NamingMode.Number && (NamingMode)m_prefixDD.selectedIndex != NamingMode.Number;
        }

        public void ReloadData()
        {
            var config = TSD.GetConfig();
            m_paletteDD.selectedValue = config.Palette;
            m_suffixDD.selectedIndex = Array.IndexOf(TLMConfigOptions.namingOptionsSuffix, config.Suffix);
            m_nonPrefixDD.selectedIndex = Array.IndexOf(TLMConfigOptions.namingOptionsSuffix, config.NonPrefixedNaming);
            m_separatorDD.selectedIndex = Array.IndexOf(TLMConfigOptions.namingOptionsSeparator, config.Separator);
            m_prefixDD.selectedIndex = Array.IndexOf(TLMConfigOptions.namingOptionsPrefix, config.Prefix);
            m_prefixIncrement.isChecked = config.IncrementPrefixOnNewLine;
            m_zerosContainer.isChecked = config.UseLeadingZeros;
            m_prefixAsSuffixContainer.isChecked = config.InvertPrefixSuffix;
            m_autoColorBasedContainer.isChecked = config.PalettePrefixBased;
            m_randomPaletteOnOverflow.isChecked = config.PaletteRandomOnOverflow;
            m_vehicleNumberFormatLocal.text = config.VehicleIdentifierFormatLocal ?? "";
            m_vehicleNumberFormatForeign.text = config.VehicleIdentifierFormatForeign ?? "";
            m_useInAutoName.isChecked = config.UseInAutoName;
            m_namingPrefix.text = config.NamingPrefix ?? "";
            m_defaultCost.text = config.DefaultCostPerPassenger.ToString();
            m_defaultCostLbl.suffix = $" (Def: {(TSD.GetDefaultPassengerCapacityCost() is float cost && cost >= 0 ? cost.ToString("C3") : "N/A")})";
        }
    }

    internal sealed class TLMShowConfigTabNorBus : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.BUS; }
    internal sealed class TLMShowConfigTabNorTrm : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.TRAM; }
    internal sealed class TLMShowConfigTabNorMnr : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.MONORAIL; }
    internal sealed class TLMShowConfigTabNorMet : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.METRO; }
    internal sealed class TLMShowConfigTabNorTrn : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.TRAIN; }
    internal sealed class TLMShowConfigTabNorFer : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.FERRY; }
    internal sealed class TLMShowConfigTabNorBlp : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.BLIMP; }
    internal sealed class TLMShowConfigTabNorShp : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.SHIP; }
    internal sealed class TLMShowConfigTabNorPln : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.PLANE; }
    internal sealed class TLMShowConfigTabTouBus : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.TOUR_BUS; }
    internal sealed class TLMShowConfigTabTouPed : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.TOUR_PED; }
    internal sealed class TLMShowConfigTabNorTrl : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.TROLLEY; }
    internal sealed class TLMShowConfigTabNorHel : TLMShowConfigTab { internal override TransportSystemDefinition TSD { get; } = TransportSystemDefinition.HELICOPTER; }
}
