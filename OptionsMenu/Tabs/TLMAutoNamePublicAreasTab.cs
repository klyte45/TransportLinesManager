using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Linq;
using System.Collections.Generic;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMAutoNamePublicAreasTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;
        private readonly Dictionary<TLMSpecialNamingClass, UITextField> m_textFields = new Dictionary<TLMSpecialNamingClass, UITextField>();
        private readonly Dictionary<TLMSpecialNamingClass, UICheckBox> m_checks = new Dictionary<TLMSpecialNamingClass, UICheckBox>();

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            var group15 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group15.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group15.Self).wrapLayout = true;
            ((UIScrollablePanel)group15.Self).width = parent.width;

            group15.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_AREAS"));
            group15.AddSpace(1);
            group15.AddLabel(Locale.Get("K45_TLM_AUTO_NAME_SETTINGS_PUBLIC_TRANSPORT_DESC"));
            group15.AddSpace(15);

            foreach (var service in Enum.GetValues(typeof(TLMSpecialNamingClass)).OfType<TLMSpecialNamingClass>())
            {
                AddCheckbox(service.GetLocalizedName(), out UICheckBox check, group15, (x) => TLMBaseConfigXML.Instance.GetAutoNameData(service).UseInAutoName = x);
                AddTextField(Locale.Get("K45_TLM_PREFIX_OPTIONAL"), out UITextField textField, group15, (x) => TLMBaseConfigXML.Instance.GetAutoNameData(service).NamingPrefix = x);
                m_checks[service] = check;
                m_textFields[service] = textField;
                group15.AddSpace(5);
            }
        }


        public void ReloadData()
        {
            foreach (var kv in m_textFields)
            {
                kv.Value.text = TLMBaseConfigXML.Instance.GetAutoNameData(kv.Key).NamingPrefix;
            }
            foreach (var kv in m_checks)
            {
                kv.Value.isChecked = TLMBaseConfigXML.Instance.GetAutoNameData(kv.Key).UseInAutoName;
            }
        }


    }
}
