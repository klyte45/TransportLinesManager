using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMDefaultCostPerPassengerConfigTab : UICustomControl
    {
        private UIComponent m_parent;

        public void Awake()
        {
            m_parent = GetComponentInParent<UIComponent>();
            var group72 = new UIHelperExtension(m_parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel) group72.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel) group72.Self).wrapLayout = true;
            ((UIScrollablePanel) group72.Self).width = 730;

            group72.AddLabel(Locale.Get("K45_TLM_DEFAULT_COST_PER_PASSENGER"));
            group72.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableTicketTransportCategories)
            {
                UITextField textField = TLMConfigOptions.instance.generateNumberFieldConfig(group72, TLMConfigWarehouse.getNameForTransportType(ci), TLMConfigWarehouse.ConfigIndex.DEFAULT_COST_PER_PASSENGER_CAPACITY | ci);
                UIPanel textFieldPanel = textField.GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                UILabel label = textFieldPanel.GetComponentInChildren<UILabel>();
                label.minimumSize = new Vector2(420, 0);
                KlyteMonoUtils.LimitWidth(label);
                if (TLMConfigWarehouse.IsCityLoaded)
                {
                    label.eventVisibilityChanged += (x, y) =>
                    {
                        if (y)
                        {
                            float defaultCost = TLMConfigWarehouse.GetTransportSystemDefinitionForConfigTransport(ci).GetDefaultPassengerCapacityCost();
                            if (defaultCost >= 0)
                            {
                                label.suffix = $" ({(defaultCost).ToString("C3", LocaleManager.cultureInfo)})";
                            }
                            else
                            {
                                label.suffix = $" (N/A)";
                                textField.isVisible = false;
                            }
                        }
                    };
                }
                group72.AddSpace(2);
            }

        }


    }
}
