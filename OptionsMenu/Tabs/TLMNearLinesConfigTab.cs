using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMNearLinesConfigTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group7 = new UIHelperExtension(parent);
            ((UIPanel)group7.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group7.self).wrapLayout = true;
            ((UIPanel)group7.self).width = 730;

            group7.AddLabel(Locale.Get("TLM_NEAR_LINES_CONFIG"));
            group7.AddSpace(15);

            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_SERVICES_BUILDINGS"), TransportLinesManagerMod.showNearLinesPlop, toggleShowNearLinesInCityServicesWorldInfoPanel);
            group7.AddCheckbox(Locale.Get("TLM_NEAR_LINES_SHOW_IN_ZONED_BUILDINGS"), TransportLinesManagerMod.showNearLinesGrow, toggleShowNearLinesInZonedBuildingWorldInfoPanel);
            group7.AddSpace(20);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_BUS"), TLMConfigWarehouse.ConfigIndex.BUS_SHOW_IN_LINEAR_MAP);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_METRO"), TLMConfigWarehouse.ConfigIndex.METRO_SHOW_IN_LINEAR_MAP);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TRAIN"), TLMConfigWarehouse.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_SHIP"), TLMConfigWarehouse.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP);
            TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_PLANE"), TLMConfigWarehouse.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP);
            if (LoadingManager.instance.m_currentlyLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.AfterDark))
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TAXI"), TLMConfigWarehouse.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP);
            }
            if (LoadingManager.instance.m_currentlyLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Snowfall))
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TRAM"), TLMConfigWarehouse.ConfigIndex.TRAM_SHOW_IN_LINEAR_MAP);
            }
            if (LoadingManager.instance.m_currentlyLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.NaturalDisasters))
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_EVAC_BUS"), TLMConfigWarehouse.ConfigIndex.EVAC_BUS_SHOW_IN_LINEAR_MAP);
            }
            if (LoadingManager.instance.m_currentlyLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.InMotion))
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_FERRY"), TLMConfigWarehouse.ConfigIndex.FERRY_SHOW_IN_LINEAR_MAP);
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_BLIMP"), TLMConfigWarehouse.ConfigIndex.BLIMP_SHOW_IN_LINEAR_MAP);
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_MONORAIL"), TLMConfigWarehouse.ConfigIndex.MONORAIL_SHOW_IN_LINEAR_MAP);
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_CABLE_CAR"), TLMConfigWarehouse.ConfigIndex.CABLE_CAR_SHOW_IN_LINEAR_MAP);
            }
            if (LoadingManager.instance.m_currentlyLoading || Singleton<LoadingManager>.instance.SupportsExpansion(ICities.Expansion.Parks))
            {
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TOUR_BUS"), TLMConfigWarehouse.ConfigIndex.TOUR_BUS_CONFIG_SHOW_IN_LINEAR_MAP);
                TLMConfigOptions.instance.generateCheckboxConfig(group7, Locale.Get("TLM_NEAR_LINES_SHOW_TOUR_PED"), TLMConfigWarehouse.ConfigIndex.TOUR_PED_CONFIG_SHOW_IN_LINEAR_MAP);
            }

        }
        private void toggleShowNearLinesInCityServicesWorldInfoPanel(bool b)
        {
            TransportLinesManagerMod.showNearLinesPlop = b;
        }

        private void toggleShowNearLinesInZonedBuildingWorldInfoPanel(bool b)
        {
            TransportLinesManagerMod.showNearLinesGrow = b;
        }


    }
}
