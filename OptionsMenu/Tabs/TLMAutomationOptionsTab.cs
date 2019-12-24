using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMAutomationOptionsTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group7 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group7.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) group7.Self).wrapLayout = true;
            ((UIScrollablePanel) group7.Self).width = 730;

            group7.AddLabel(Locale.Get("K45_TLM_AUTOMATION_CONFIG"));
            group7.AddSpace(15);

            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("K45_TLM_AUTO_COLOR_ENABLED"), TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("K45_TLM_AUTO_NAME_ENABLED"), TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("K45_TLM_USE_CIRCULAR_AUTO_NAME"), TLMConfigWarehouse.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("K45_TLM_ADD_LINE_NUMBER_AUTO_NAME"), TLMConfigWarehouse.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME);
        }


    }
}
