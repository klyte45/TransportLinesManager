using ColossalFramework.UI;
using Klyte.Commons.Extensors;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMModInfoTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group9 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel) group9.Self).wrapLayout = true;
            ((UIScrollablePanel) group9.Self).width = 730;
            TransportLinesManagerMod.Instance.PopulateGroup9(group9);
        }
        
    }
}
