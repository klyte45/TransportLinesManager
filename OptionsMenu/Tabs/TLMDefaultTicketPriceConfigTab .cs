using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using UnityEngine;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMDefaultTicketPriceConfigTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group72 = new UIHelperExtension(parent);
            ((UIPanel)group72.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group72.Self).wrapLayout = true;
            ((UIPanel)group72.Self).width = 730;

            group72.AddLabel(Locale.Get("K45_TLM_DEFAULT_PRICE"));
            group72.AddSpace(15);

            foreach (TLMConfigWarehouse.ConfigIndex ci in TLMConfigWarehouse.configurableTicketTransportCategories)
            {
                var textField = TLMConfigOptions.instance.generateNumberFieldConfig(group72, TLMConfigWarehouse.getNameForTransportType(ci), TLMConfigWarehouse.ConfigIndex.DEFAULT_TICKET_PRICE | ci);
                var textFieldPanel = textField.GetComponentInParent<UIPanel>();
                textFieldPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                textFieldPanel.autoFitChildrenVertically = true;
                textFieldPanel.GetComponentInChildren<UILabel>().minimumSize = new Vector2(420, 0);
                group72.AddSpace(2);
            }

        }


    }
}
