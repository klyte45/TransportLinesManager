using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.TransportLinesManager.Xml;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMModInfoTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;
        private UITextField maxVehicle;

        public void ReloadData() => maxVehicle.text = TLMBaseConfigXML.CurrentContextConfig.MaxVehiclesOnAbsoluteMode.ToString("0");

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group9 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group9.Self).wrapLayout = true;
            ((UIScrollablePanel)group9.Self).width = 700;

            AddIntField(Locale.Get("K45_TLM_MAXIMUM_VEHICLE_COUNT_FOR_SPECIFIC_LINE_CONFIG"), out maxVehicle, group9, (x) => TLMBaseConfigXML.Instance.MaxVehiclesOnAbsoluteMode = x, false);
            maxVehicle.maxLength = 3;

            TransportLinesManagerMod.Instance.PopulateGroup9(group9);
        }

    }
}
