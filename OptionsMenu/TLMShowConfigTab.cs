using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.OptionsMenu
{
    internal class TLMShowConfigTab<T> : UICustomControl where T : TLMSysDef<T>
    {
        public UIPanel mainPanel { get; private set; }
        public UIHelperExtension m_uiHelper;
        private static TLMShowConfigTab<T> m_instance;
        public static TLMShowConfigTab<T> instance => m_instance;

        private TransportSystemDefinition m_tsd = TLMSysDef<T>.instance.GetTSD();
        private TLMConfigOptions m_tlmCo = TLMConfigOptions.instance;

        UIPanel separatorContainer;
        UIPanel paletteContainer;
        UICheckBox prefixIncrement;
        UIPanel suffixDDContainer;
        UICheckBox zerosContainer;
        UICheckBox prefixAsSuffixContainer;
        UICheckBox autoColorBasedContainer;
        UIDropDown suffixDD;
        UIDropDown nonPrefixDD;
        UIDropDown prefixDD;

        private void Awake()
        {
            if (m_instance != null) throw new Exception("MULTIPLE INSTANTIATION!!!!!!!!");
            m_instance = this;
            mainPanel = GetComponent<UIPanel>();
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_uiHelper = new UIHelperExtension(mainPanel);

            var transportType = m_tsd.toConfigIndex();


            m_uiHelper.AddLabel(string.Format(Locale.Get("TLM_CONFIGS_FOR"), TLMConfigWarehouse.getNameForTransportType(transportType)));
            var panel = m_uiHelper.self.GetComponentInParent<UIPanel>();
            ((UIPanel)m_uiHelper.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)m_uiHelper.self).backgroundSprite = "EmptySprite";
            ((UIPanel)m_uiHelper.self).wrapLayout = true;
            var systemColor = TLMConfigWarehouse.getColorForTransportType(transportType);
            ((UIPanel)m_uiHelper.self).color = new Color32((byte)(systemColor.r * 0.7f), (byte)(systemColor.g * 0.7f), (byte)(systemColor.b * 0.7f), 0xff);
            ((UIPanel)m_uiHelper.self).width = 730;
            m_uiHelper.AddSpace(30);
            prefixDD = m_tlmCo.generateDropdownConfig(m_uiHelper, Locale.Get("TLM_PREFIX"), m_tlmCo.namingOptionsPrefixo, transportType | TLMConfigWarehouse.ConfigIndex.PREFIX);
            separatorContainer = m_tlmCo.generateDropdownConfig(m_uiHelper, Locale.Get("TLM_SEPARATOR"), m_tlmCo.namingOptionsSeparador, transportType | TLMConfigWarehouse.ConfigIndex.SEPARATOR).transform.parent.GetComponent<UIPanel>();
            suffixDD = m_tlmCo.generateDropdownConfig(m_uiHelper, Locale.Get("TLM_SUFFIX"), m_tlmCo.namingOptionsSufixo, transportType | TLMConfigWarehouse.ConfigIndex.SUFFIX);
            suffixDDContainer = suffixDD.transform.parent.GetComponent<UIPanel>();
            nonPrefixDD = m_tlmCo.generateDropdownConfig(m_uiHelper, Locale.Get("TLM_IDENTIFIER_NON_PREFIXED"), m_tlmCo.namingOptionsSufixo, transportType | TLMConfigWarehouse.ConfigIndex.NON_PREFIX);
            paletteContainer = m_tlmCo.generateDropdownStringValueConfig(m_uiHelper, Locale.Get("TLM_PALETTE_PREFIXED"), TLMAutoColorPalettes.paletteList, transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_MAIN).transform.parent.GetComponent<UIPanel>();
            zerosContainer = m_tlmCo.generateCheckboxConfig(m_uiHelper, Locale.Get("TLM_LEADING_ZEROS_SUFFIX"), transportType | TLMConfigWarehouse.ConfigIndex.LEADING_ZEROS);
            prefixAsSuffixContainer = m_tlmCo.generateCheckboxConfig(m_uiHelper, Locale.Get("TLM_INVERT_PREFIX_SUFFIX_ORDER"), transportType | TLMConfigWarehouse.ConfigIndex.INVERT_PREFIX_SUFFIX);
            m_tlmCo.generateCheckboxConfig(m_uiHelper, Locale.Get("TLM_RANDOM_ON_PALETTE_OVERFLOW"), transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);
            autoColorBasedContainer = m_tlmCo.generateCheckboxConfig(m_uiHelper, Locale.Get("TLM_AUTO_COLOR_BASED_ON_PREFIX"), transportType | TLMConfigWarehouse.ConfigIndex.PALETTE_PREFIX_BASED);
            prefixIncrement = m_tlmCo.generateCheckboxConfig(m_uiHelper, Locale.Get("TLM_LINENUMBERING_BASED_IN_PREFIX"), transportType | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT);

            prefixDD.eventSelectedIndexChanged += OnPrefixOptionChange;
            suffixDD.eventSelectedIndexChanged += OnSuffixOptionChange;
            OnPrefixOptionChange(prefixDD, prefixDD.selectedIndex);
        }

        private void OnPrefixOptionChange(UIComponent c, int sel)
        {
            bool isPrefixed = (ModoNomenclatura)sel != ModoNomenclatura.Nenhum;
            separatorContainer.isVisible = isPrefixed;            
            prefixIncrement.isVisible = isPrefixed;
            suffixDDContainer.isVisible = isPrefixed;
            zerosContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero;
            prefixAsSuffixContainer.isVisible = isPrefixed && (ModoNomenclatura)suffixDD.selectedIndex == ModoNomenclatura.Numero && (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Numero;
            autoColorBasedContainer.isVisible = isPrefixed;
        }

        private void OnSuffixOptionChange(UIComponent c, int sel)
        {
            bool isPrefixed = (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Nenhum;
            zerosContainer.isVisible = isPrefixed && (ModoNomenclatura)sel == ModoNomenclatura.Numero;
            prefixAsSuffixContainer.isVisible = isPrefixed && (ModoNomenclatura)sel == ModoNomenclatura.Numero && (ModoNomenclatura)prefixDD.selectedIndex != ModoNomenclatura.Numero;
        }
    }

    internal sealed class TLMShowConfigTabNorBus : TLMShowConfigTab<TLMSysDefNorBus> { }
    internal sealed class TLMShowConfigTabNorTrm : TLMShowConfigTab<TLMSysDefNorTrm> { }
    internal sealed class TLMShowConfigTabNorMnr : TLMShowConfigTab<TLMSysDefNorMnr> { }
    internal sealed class TLMShowConfigTabNorMet : TLMShowConfigTab<TLMSysDefNorMet> { }
    internal sealed class TLMShowConfigTabNorTrn : TLMShowConfigTab<TLMSysDefNorTrn> { }
    internal sealed class TLMShowConfigTabNorFer : TLMShowConfigTab<TLMSysDefNorFer> { }
    internal sealed class TLMShowConfigTabNorBlp : TLMShowConfigTab<TLMSysDefNorBlp> { }
    internal sealed class TLMShowConfigTabNorShp : TLMShowConfigTab<TLMSysDefNorShp> { }
    internal sealed class TLMShowConfigTabNorPln : TLMShowConfigTab<TLMSysDefNorPln> { }
    internal sealed class TLMShowConfigTabTouBus : TLMShowConfigTab<TLMSysDefTouBus> { }
}
