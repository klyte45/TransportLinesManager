using ColossalFramework.UI;
using Klyte.Commons.Extensions;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMModInfoTab : UICustomControl, ITLMConfigOptionsTab
    {

        UIComponent parent;

        public void ReloadData() { }

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
