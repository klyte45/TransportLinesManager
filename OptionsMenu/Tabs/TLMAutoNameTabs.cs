using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMAutoNamePublicTransportTab : UICustomControl
    {
        private UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            var group13 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel) group13.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) group13.Self).wrapLayout = true;
            ((UIScrollablePanel) group13.Self).width = 730;

            group13.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT"));
            group13.AddSpace(1);
            group13.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group13.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameTransportCategories)
            {
                UICheckBox checkbox = TLMConfigOptions.instance.generateCheckboxConfig(group13, TLMConfigWarehouse.getNameForTransportType(ci), TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_USE_FOR_AUTO_NAMING_REF | ci, 200);
                UITextField textField = TLMConfigOptions.instance.generateTextFieldConfig(group13, Locale.Get("K45_TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.PUBLICTRANSPORT_AUTO_NAMING_REF_TEXT | ci);
                UIPanel textFieldPanel = textField.GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                UILabel title = textFieldPanel.GetComponentInChildren<UILabel>();
                title.textAlignment = UIHorizontalAlignment.Center;
                KlyteMonoUtils.LimitWidthAndBox(title, 220, true);
                textFieldPanel.AttachUIComponent(checkbox.gameObject);
                checkbox.eventVisibilityChanged += (x, y) =>
                {
                    if (x)
                    {
                        checkbox.zOrder = 0;
                    }
                };
                group13.AddSpace(1);
            }
        }


    }
    internal class TLMAutoNameBuildingsTab : UICustomControl
    {
        private UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            var group14 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel) group14.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) group14.Self).wrapLayout = true;
            ((UIScrollablePanel) group14.Self).width = 730;

            group14.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_OTHER"));
            group14.AddSpace(1);
            group14.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group14.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableAutoNameCategories)
            {
                UICheckBox checkbox = TLMConfigOptions.instance.generateCheckboxConfig(group14, TLMConfigWarehouse.GetNameForServiceType(ci), TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | ci, 200);
                UIPanel textFieldPanel = TLMConfigOptions.instance.generateTextFieldConfig(group14, Locale.Get("K45_TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                UILabel title = textFieldPanel.GetComponentInChildren<UILabel>();
                title.textAlignment = UIHorizontalAlignment.Center;
                KlyteMonoUtils.LimitWidthAndBox(title, 220, true);
                textFieldPanel.AttachUIComponent(checkbox.gameObject);
                checkbox.eventVisibilityChanged += (x, y) =>
                {
                    if (x)
                    {
                        checkbox.zOrder = 0;
                    }
                };
                group14.AddSpace(2);
            }
        }


    }
    internal class TLMAutoNamePublicAreasTab : UICustomControl
    {
        private UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            var group15 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel) group15.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) group15.Self).wrapLayout = true;
            ((UIScrollablePanel) group15.Self).width = 730;

            group15.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_AREAS"));
            group15.AddSpace(1);
            group15.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group15.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.extraAutoNameCategories)
            {
                UICheckBox checkbox = TLMConfigOptions.instance.generateCheckboxConfig(group15, TLMConfigWarehouse.GetNameForServiceType(ci), TLMConfigWarehouse.ConfigIndex.USE_FOR_AUTO_NAMING_REF | ci, 200);
                UIPanel textFieldPanel = TLMConfigOptions.instance.generateTextFieldConfig(group15, Locale.Get("K45_TLM_PREFIX_OPTIONAL"), TLMConfigWarehouse.ConfigIndex.AUTO_NAMING_REF_TEXT | ci).GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                UILabel title = textFieldPanel.GetComponentInChildren<UILabel>();
                title.textAlignment = UIHorizontalAlignment.Center;
                KlyteMonoUtils.LimitWidthAndBox(title, 220, true);
                textFieldPanel.AttachUIComponent(checkbox.gameObject);
                checkbox.eventVisibilityChanged += (x, y) =>
                {
                    if (x)
                    {
                        checkbox.zOrder = 0;
                    }
                };
                group15.AddSpace(2);
            }
        }


    }
}
