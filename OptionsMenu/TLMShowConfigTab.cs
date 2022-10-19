﻿using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.ModShared;
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
        private UIDropDown m_iconDD;
        private UICheckBox m_prefixIncrement;
        private UICheckBox m_nearLines;
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
        private UICheckBox m_requireStartAtTerminal;
        private UITextField m_namingPrefix;
        private UILabel m_defaultCostLbl;
        private UITextField m_defaultCost;
        private UITextField m_defaultTicketPrice;

        private void Awake()
        {
            instance = this;
            mainPanel = GetComponent<UIPanel>();
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_uiHelper = new UIHelperExtension(mainPanel);

            m_uiHelper.AddLabel(string.Format(Locale.Get("K45_TLM_CONFIGS_FOR"), TSD.GetTransportName()));
            UIPanel panel = m_uiHelper.Self.GetComponentInParent<UIPanel>();
            ((UIPanel)m_uiHelper.Self).autoLayoutDirection = LayoutDirection.Vertical;
            ((UIPanel)m_uiHelper.Self).backgroundSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_MenuPanel_color);
            ((UIPanel)m_uiHelper.Self).wrapLayout = true;
            ((UIPanel)m_uiHelper.Self).padding = new RectOffset(10, 10, 10, 15);
            ((UIPanel)m_uiHelper.Self).color = TSD.Color;
            ((UIPanel)m_uiHelper.Self).width = 730;
            m_uiHelper.AddSpace(30);


            AddDropdown(Locale.Get("K45_TLM_PREFIX"), out m_prefixDD, m_uiHelper, TLMConfigOptions.namingOptionsPrefix.Select(x => Tuple.New(x.GetName(), x)).OrderBy(x => x.First).ToArray(), (x) =>
             {
                 TSD.GetConfig().Prefix = x;
                 OnPrefixOptionChange();
                 TLMFacade.Instance?.OnLineSymbolParameterChanged();
             });
            AddDropdown(Locale.Get("K45_TLM_SEPARATOR"), out m_separatorDD, m_uiHelper, TLMConfigOptions.namingOptionsSeparator.Select(x => Tuple.New(x.GetName(), x)).OrderBy(x => x.First).ToArray(), (x) =>
            {
                TSD.GetConfig().Separator = x;
                TLMFacade.Instance?.OnLineSymbolParameterChanged();
            });
            AddDropdown(Locale.Get("K45_TLM_SUFFIX"), out m_suffixDD, m_uiHelper, TLMConfigOptions.namingOptionsSuffix.Select(x => Tuple.New(x.GetName(), x)).OrderBy(x => x.First).ToArray(), (x) =>
            {
                TSD.GetConfig().Suffix = x;
                OnSuffixOptionChange();
                TLMFacade.Instance?.OnLineSymbolParameterChanged();
            });
            AddDropdown(Locale.Get("K45_TLM_IDENTIFIER_NON_PREFIXED"), out m_nonPrefixDD, m_uiHelper, TLMConfigOptions.namingOptionsSuffix.Select(x => Tuple.New(x.GetName(), x)).OrderBy(x => x.First).ToArray(), (x) =>
            {
                TSD.GetConfig().NonPrefixedNaming = x;
                TLMFacade.Instance?.OnLineSymbolParameterChanged();
            });
            AddDropdown(Locale.Get("K45_TLM_PALETTE"), out m_paletteDD, m_uiHelper, TLMAutoColorPaletteContainer.PaletteList.Select(x => Tuple.New(x, x)).OrderBy(x => x.First).ToArray(), (x) =>
               {
                   TSD.GetConfig().Palette = x;
                   TLMFacade.Instance?.OnLineSymbolParameterChanged();

               });
            AddDropdown(Locale.Get("K45_TLM_ICON"), out m_iconDD, m_uiHelper, Enum.GetValues(typeof(LineIconSpriteNames)).OfType<LineIconSpriteNames>().Select(x => Tuple.New(x.GetNameForTLM(), x)).OrderBy(x => x.First).ToArray(), (x) =>
               {
                   TSD.GetConfig().DefaultLineIcon = x;
                   TLMFacade.Instance?.OnLineSymbolParameterChanged();
               });
            m_uiHelper.AddSpace(5);

            AddCheckbox(Locale.Get("K45_TLM_USE_AUTO_NAME"), out m_useInAutoName, m_uiHelper, (x) =>
            {
                TSD.GetConfig().UseInAutoName = x;
                TLMFacade.Instance?.OnLineSymbolParameterChanged();
            });
            AddCheckbox(Locale.Get("K45_TLM_REQUIRESTARTTERMINAL"), out m_requireStartAtTerminal, m_uiHelper, (x) => TSD.GetConfig().RequireLineStartTerminal = x, TSD.GetConfig().RequireLineStartTerminal);
            AddTextField(Locale.Get("K45_TLM_PREFIX_BUILDING_NAMES"), out m_namingPrefix, m_uiHelper, (x) =>
            {
                TSD.GetConfig().NamingPrefix = x;
                TLMFacade.Instance?.OnAutoNameParameterChanged();
            }, TSD.GetConfig().NamingPrefix);
            m_uiHelper.AddSpace(5);

            if (TSD.VehicleType != VehicleInfo.VehicleType.None)
            {
                AddTextField(Locale.Get("K45_TLM_NUMBERVEHICLEPATTERN_LOCAL"), out m_vehicleNumberFormatLocal, m_uiHelper, (x) =>
                {
                    TSD.GetConfig().VehicleIdentifierFormatLocal = x ?? "";
                    TLMFacade.Instance?.OnVehicleIdentifierParameterChanged();
                });
                AddTextField(Locale.Get("K45_TLM_NUMBERVEHICLEPATTERN_FOREIGN"), out m_vehicleNumberFormatForeign, m_uiHelper, (x) =>
                {
                    TSD.GetConfig().VehicleIdentifierFormatForeign = x ?? "";
                    TLMFacade.Instance?.OnVehicleIdentifierParameterChanged();
                });
                m_uiHelper.AddSpace(5);

                AddIntField(Locale.Get("K45_TLM_COST_PER_PASSENGER_CONFIG"), out m_defaultCost, out m_defaultCostLbl, m_uiHelper, (x) => TSD.GetConfig().DefaultCostPerPassenger = x, false);
                AddIntField(Locale.Get("K45_TLM_DEFAULT_PRICE"), out m_defaultTicketPrice, m_uiHelper, (x) => TSD.GetConfig().DefaultTicketPrice = x, false);
                m_uiHelper.AddSpace(5);

                AddButtonInEditorRow(m_vehicleNumberFormatLocal, CommonsSpriteNames.K45_QuestionMark, Help_VehicleNumberFormat, null, true, 30);
                AddButtonInEditorRow(m_vehicleNumberFormatForeign, CommonsSpriteNames.K45_QuestionMark, Help_VehicleNumberFormat, null, true, 30);
            }

            AddCheckboxLocale("K45_TLM_NEAR_LINES_SHOW", out m_nearLines, m_uiHelper, (x) => TSD.GetConfig().ShowInLinearMap = x);
            AddCheckboxLocale("K45_TLM_LEADING_ZEROS_SUFFIX", out m_zerosContainer, m_uiHelper, (x) =>
            {
                TSD.GetConfig().UseLeadingZeros = x;
                TLMFacade.Instance?.OnLineSymbolParameterChanged();
            });
            AddCheckboxLocale("K45_TLM_INVERT_PREFIX_SUFFIX_ORDER", out m_prefixAsSuffixContainer, m_uiHelper, (x) =>
            {
                TSD.GetConfig().InvertPrefixSuffix = x;
                TLMFacade.Instance?.OnLineSymbolParameterChanged();
            });
            AddCheckboxLocale("K45_TLM_RANDOM_ON_PALETTE_OVERFLOW", out m_randomPaletteOnOverflow, m_uiHelper, (x) => TSD.GetConfig().PaletteRandomOnOverflow = x);
            AddCheckboxLocale("K45_TLM_AUTO_COLOR_BASED_ON_PREFIX", out m_autoColorBasedContainer, m_uiHelper, (x) => TSD.GetConfig().PalettePrefixBased = x);
            AddCheckboxLocale("K45_TLM_LINENUMBERING_BASED_IN_PREFIX", out m_prefixIncrement, m_uiHelper, (x) => TSD.GetConfig().IncrementPrefixOnNewLine = x);
            ReloadData();
        }

        private void Help_VehicleNumberFormat() => K45DialogControl.ShowModalHelp("Vehicles.NumberPattern", Locale.Get("K45_TLM_NUMBERINGVEHICLESHELP"), 0);

        private void OnPrefixOptionChange()
        {
            bool isPrefixed = TSD.GetConfig().Prefix != NamingMode.None;
            m_separatorDD.parent.isVisible = isPrefixed;
            m_prefixIncrement.isVisible = isPrefixed;
            m_suffixDD.parent.isVisible = isPrefixed;
            m_autoColorBasedContainer.isVisible = isPrefixed;
            OnSuffixOptionChange();
        }

        private void OnSuffixOptionChange()
        {
            var config = TSD.GetConfig();
            bool isPrefixed = config.Prefix != NamingMode.None;
            m_zerosContainer.isVisible = isPrefixed && config.Suffix == NamingMode.Number;
            m_prefixAsSuffixContainer.isVisible = isPrefixed && config.Suffix == NamingMode.Number && m_prefixDD.GetSelection<NamingMode>() != NamingMode.Number;
        }

        public void ReloadData()
        {
            var config = TSD.GetConfig();
            m_paletteDD.SetSelection(config.Palette ?? "<" + Locale.Get("K45_TLM_RANDOM") + ">");
            m_iconDD.SetSelection(config.DefaultLineIcon);
            m_suffixDD.SetSelection(config.Suffix);
            m_nonPrefixDD.SetSelection(config.NonPrefixedNaming);
            m_separatorDD.SetSelection(config.Separator);
            m_prefixDD.SetSelection(config.Prefix);
            m_prefixIncrement.isChecked = config.IncrementPrefixOnNewLine;
            m_zerosContainer.isChecked = config.UseLeadingZeros;
            m_prefixAsSuffixContainer.isChecked = config.InvertPrefixSuffix;
            m_autoColorBasedContainer.isChecked = config.PalettePrefixBased;
            m_randomPaletteOnOverflow.isChecked = config.PaletteRandomOnOverflow;
            m_useInAutoName.isChecked = config.UseInAutoName;
            m_requireStartAtTerminal.isChecked = config.RequireLineStartTerminal;
            m_namingPrefix.text = config.NamingPrefix ?? "";
            m_nearLines.isChecked = config.ShowInLinearMap;
            if (TSD.VehicleType != VehicleInfo.VehicleType.None)
            {
                m_vehicleNumberFormatLocal.text = config.VehicleIdentifierFormatLocal ?? "";
                m_vehicleNumberFormatForeign.text = config.VehicleIdentifierFormatForeign ?? "";
                m_defaultCost.text = config.DefaultCostPerPassenger.ToString();
                m_defaultCostLbl.suffix = $" (Def: {(TSD.GetDefaultPassengerCapacityCostLocal() is float cost && cost >= 0 ? (cost * 100).ToString("0.00") + "¢" : "N /A")})";
                m_defaultTicketPrice.text = config.DefaultTicketPrice.ToString();
            }
            OnPrefixOptionChange();
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
