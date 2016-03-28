using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Klyte.TransportLinesManager.i18n
{
    class TLMLocaleUtils
    {
        private const string lineSeparator = "\r\n";
        private const string kvSeparator = "=";
        private const string idxSeparator = ":";
        private const string localeKeySeparator = "|";
        private const string commentChar = "#";

        public static void loadLocale(string localeId)
        {
            string load = ResourceLoader.loadResourceString("UI.i18n." + localeId + ".properties");
            if (load == null)
            {
                load = ResourceLoader.loadResourceString("UI.i18n.en.properties");
                if (load == null)
                {
                    TLMUtils.doLog("LOCALE NOT LOADED!!!!");
                    return;
                }
                localeId = "en";
            }

            var locale = TLMUtils.GetPrivateField<Locale>(LocaleManager.instance, "m_Locale");
            Locale.Key k;


            foreach (var myString in load.Split(new string[] { lineSeparator },  StringSplitOptions.RemoveEmptyEntries))
            {
                if (myString.StartsWith(commentChar)) continue;
                if (!myString.Contains(kvSeparator)) continue;
                var array = myString.Split(kvSeparator.ToCharArray(), 2);
                string value = array[1];
                int idx = 0;
                string localeKey = null;
                if (array[0].Contains(idxSeparator))
                {
                    array = array[0].Split(idxSeparator.ToCharArray());
                    if (!int.TryParse(array[1], out idx))
                    {
                        continue;
                    }
                }
                if (array[0].Contains(localeKeySeparator))
                {
                    array = array[0].Split(localeKeySeparator.ToCharArray());
                    localeKey = array[1];
                }

                k = new Locale.Key()
                {
                    m_Identifier = "TLM_" + array[0],
                    m_Key = localeKey,
                    m_Index = idx
                };
                if (!locale.Exists(k))
                {
                    locale.AddLocalizedString(k, value.Replace("\\n", "\n"));
                }
            }

            if (localeId != "en")
            {
                loadLocale("en");
            }
        }
    }
}
