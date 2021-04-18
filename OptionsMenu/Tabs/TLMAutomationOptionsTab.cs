using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.TransportLinesManager.Xml;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMAutomationOptionsTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;
        private UICheckBox m_autoColor;
        private UICheckBox m_autoName;
        private UICheckBox m_circular;
        private UICheckBox m_addLineCode;

        public void ReloadData()
        {
            if (TLMBaseConfigXML.Instance is null)
            {
                return;
            }
            m_autoColor.isChecked = TLMBaseConfigXML.CurrentContextConfig.UseAutoColor;
            m_autoName.isChecked = TLMBaseConfigXML.CurrentContextConfig.UseAutoName;
            m_circular.isChecked = TLMBaseConfigXML.CurrentContextConfig.CircularIfSingleDistrictLine;
            m_addLineCode.isChecked = TLMBaseConfigXML.CurrentContextConfig.AddLineCodeInAutoname;
        }

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group7 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group7.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group7.Self).wrapLayout = true;
            ((UIScrollablePanel)group7.Self).width = 730;

            group7.AddLabel(Locale.Get("K45_TLM_AUTOMATION_CONFIG"));
            group7.AddSpace(15);

            AddCheckboxLocale("K45_TLM_AUTO_COLOR_ENABLED", out m_autoColor, group7, (x) => TLMBaseConfigXML.CurrentContextConfig.UseAutoColor = x);
            AddCheckboxLocale("K45_TLM_AUTO_NAME_ENABLED", out m_autoName, group7, (x) => TLMBaseConfigXML.CurrentContextConfig.UseAutoName = x);
            AddCheckboxLocale("K45_TLM_USE_CIRCULAR_AUTO_NAME", out m_circular, group7, (x) => TLMBaseConfigXML.CurrentContextConfig.CircularIfSingleDistrictLine = x);
            AddCheckboxLocale("K45_TLM_ADD_LINE_NUMBER_AUTO_NAME", out m_addLineCode, group7, (x) => TLMBaseConfigXML.CurrentContextConfig.AddLineCodeInAutoname = x);

      
        }


    }
}
