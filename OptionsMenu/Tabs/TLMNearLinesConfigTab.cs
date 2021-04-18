using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMNearLinesConfigTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;

        public void ReloadData() { }

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group7 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group7.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group7.Self).wrapLayout = true;
            ((UIScrollablePanel)group7.Self).width = 730;

            group7.AddLabel(Locale.Get("K45_TLM_NEAR_LINES_CONFIG"));
            group7.AddSpace(15);

            group7.AddCheckbox(Locale.Get("K45_TLM_NEAR_LINES_SHOW_IN_SERVICES_BUILDINGS"), TransportLinesManagerMod.showNearLinesPlop, toggleShowNearLinesInCityServicesWorldInfoPanel);
            group7.AddCheckbox(Locale.Get("K45_TLM_NEAR_LINES_SHOW_IN_ZONED_BUILDINGS"), TransportLinesManagerMod.showNearLinesGrow, toggleShowNearLinesInZonedBuildingWorldInfoPanel);

        }
        private void toggleShowNearLinesInCityServicesWorldInfoPanel(bool b) => TransportLinesManagerMod.showNearLinesPlop = b;

        private void toggleShowNearLinesInZonedBuildingWorldInfoPanel(bool b) => TransportLinesManagerMod.showNearLinesGrow = b;


    }
}
