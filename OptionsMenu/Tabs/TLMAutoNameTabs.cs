using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMAutoNamePublicTransportTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group13 = new UIHelperExtension(parent);
            ((UIPanel)group13.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group13.self).wrapLayout = true;
            ((UIPanel)group13.self).width = 730;

            group13.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT"));
            group13.AddSpace(1);
            group13.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group13.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameTransportCategories)
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group13, TLMConfigWarehouse.getNameForTransportType(ci), TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | ci).width = 300;
                var textFieldPanel = TLMConfigOptions.instance.generateTextFieldConfig(group13, Locale.Get("K45_TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                group13.AddSpace(1);
            }
        }


    }
    internal class TLMAutoNameBuildingsTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group14 = new UIHelperExtension(parent);
            ((UIPanel)group14.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group14.self).wrapLayout = true;
            ((UIPanel)group14.self).width = 730;

            group14.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_OTHER"));
            group14.AddSpace(1);
            group14.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group14.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameCategories)
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group14, TLMConfigWarehouse.getNameForServiceType(ci), TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | ci).width = 300;
                var textFieldPanel = TLMConfigOptions.instance.generateTextFieldConfig(group14, Locale.Get("K45_TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                group14.AddSpace(2);
            }
        }


    }
    internal class TLMAutoNamePublicAreasTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group15 = new UIHelperExtension(parent);
            ((UIPanel)group15.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group15.self).wrapLayout = true;
            ((UIPanel)group15.self).width = 730;

            group15.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_AREAS"));
            group15.AddSpace(1);
            group15.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group15.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.extraAutoNameCategories)
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group15, TLMConfigWarehouse.getNameForServiceType(ci), TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | ci).width = 300;
                var textFieldPanel = TLMConfigOptions.instance.generateTextFieldConfig(group15, Locale.Get("K45_TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                group15.AddSpace(2);
            }
        }


    }
}
