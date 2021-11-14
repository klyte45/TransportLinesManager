using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System.Collections.Generic;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMAutoNameBuildingsTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;
        private readonly Dictionary<ItemClass.Service, UITextField> m_textFields = new Dictionary<ItemClass.Service, UITextField>();
        private readonly Dictionary<ItemClass.Service, UICheckBox> m_checks = new Dictionary<ItemClass.Service, UICheckBox>();

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            var group14 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group14.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group14.Self).wrapLayout = true;
            ((UIScrollablePanel)group14.Self).width = 730;

            group14.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_OTHER"));
            group14.AddSpace(1);
            group14.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group14.AddSpace(15);

            foreach (var serviceIt in TLMStationUtils.GetUsableServiceInAutoName())
            {
                var service = serviceIt;
                AddCheckbox(service.ToString(), out UICheckBox check, group14, (x) => TLMBaseConfigXML.CurrentContextConfig.GetAutoNameData(service).UseInAutoName = x);
                AddTextField(Locale.Get("K45_TLM_PREFIX_BUILDING_NAMES"), out UITextField textField, group14, (x) => TLMBaseConfigXML.CurrentContextConfig.GetAutoNameData(service).NamingPrefix = x);
                m_checks[service] = check;
                m_textFields[service] = textField;
                group14.AddSpace(5);
            }
        }
        public void ReloadData()
        {
            foreach (var kv in m_textFields)
            {
                kv.Value.text = TLMBaseConfigXML.CurrentContextConfig.GetAutoNameData(kv.Key).NamingPrefix ?? "";
            }
            foreach (var kv in m_checks)
            {
                kv.Value.isChecked = TLMBaseConfigXML.CurrentContextConfig.GetAutoNameData(kv.Key).UseInAutoName;
            }
        }

    }
}
