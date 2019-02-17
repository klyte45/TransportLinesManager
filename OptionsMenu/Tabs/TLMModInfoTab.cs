using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.i18n;
using Klyte.TransportLinesManager.MapDrawer;
using Klyte.TransportLinesManager.Utils;
using System.Linq;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMModInfoTab : UICustomControl
    {

        UIComponent parent;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group9 = new UIHelperExtension(parent);
            ((UIPanel)group9.self).wrapLayout = true;
            ((UIPanel)group9.self).width = 730;

            group9.AddLabel(Locale.Get("TLM_BETAS_EXTRA_INFO"));
            group9.AddSpace(15);

            group9.AddDropdownLocalized("TLM_MOD_LANG", TLMLocaleUtils.instance.getLanguageIndex(), TLMSingleton.instance.currentLanguageIdx, delegate (int idx)
            {
                TLMSingleton.instance.loadTLMLocale(true, idx);
            });
            group9.AddButton(Locale.Get("TLM_DRAW_CITY_MAP"), TLMMapDrawer.drawCityMap);
            group9.AddCheckbox(Locale.Get("TLM_DEBUG_MODE"), TLMSingleton.debugMode.value, delegate (bool val) { TLMSingleton.debugMode.value = val; });
            group9.AddLabel("Version: " + TLMSingleton.version + " rev" + typeof(TLMSingleton).Assembly.GetName().Version.Revision);
            group9.AddLabel(Locale.Get("TLM_ORIGINAL_KC_VERSION") + " " + string.Join(".", TLMResourceLoader.instance.loadResourceString("TLMVersion.txt").Split(".".ToCharArray()).Take(3).ToArray()));
            group9.AddButton(Locale.Get("TLM_RELEASE_NOTES"), delegate ()
            {
                TLMSingleton.instance.showVersionInfoPopup(true);
            });
        }


    }
}
