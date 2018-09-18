using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.Utils;
using System;

namespace Klyte.TransportLinesManager.i18n
{
    internal class TLMLocaleUtils
    {
        private const string lineSeparator = "\r\n";
        private const string kvSeparator = "=";
        private const string idxSeparator = ">";
        private const string localeKeySeparator = "|";
        private const string commentChar = "#";
        private const string ignorePrefixChar = "%";
        private static string language = "";
        private static string[] locales = new string[] { "en", "pt", "ko", "de", "cn", "pl", "nl", "fr", "es", "ru" };

        public static string loadedLanguage
        {
            get {
                return language;
            }
        }

        public static string loadedLanguageEffective
        {
            get {
                return language.Length == 0 ? "en" : language.Substring(0, 2);
            }
        }

        public static string[] getLanguageIndex()
        {
            Array8<string> saida = new Array8<string>((uint)locales.Length + 1);
            saida.m_buffer[0] = Locale.Get("TLM_GAME_DEFAULT_LANGUAGE");
            for (int i = 0; i < locales.Length; i++)
            {
                saida.m_buffer[i + 1] = Locale.Get("TLM_LANG", locales[i]);
            }
            return saida.m_buffer;
        }

        public static string getSelectedLocaleByIndex(int idx)
        {
            if (idx <= 0 || idx > locales.Length)
            {
                return "en";
            }
            return locales[idx - 1];
        }

        public static void loadLocale(string localeId, bool force, string prefix = "TLM_", string packagePrefix = "Klyte.TransportLinesManager")
        {
            if (force)
            {
                LocaleManager.ForceReload();
            }
            else
            {
                loadLocaleIntern(localeId, true, prefix, packagePrefix);
            }
        }
        private static void loadLocaleIntern(string localeId, bool setLocale, string prefix, string packagePrefix)
        {
            string load = TLMResourceLoader.instance.loadResourceString("UI.i18n." + localeId + ".properties");
            if (load == null)
            {
                TLMUtils.doErrorLog("FILE " + "UI.i18n." + localeId + ".properties" + " NOT LOADED!!!!");
                load = TLMResourceLoader.instance.loadResourceString("UI.i18n.en.properties");
                if (load == null)
                {
                    TLMUtils.doErrorLog("LOCALE NOT LOADED!!!!");
                    return;
                }
                localeId = "en";
            }
            var locale = TLMUtils.GetPrivateField<Locale>(LocaleManager.instance, "m_Locale");
            Locale.Key k;


            foreach (var myString in load.Split(new string[] { lineSeparator }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (myString.StartsWith(commentChar)) continue;
                if (!myString.Contains(kvSeparator)) continue;
                bool noPrefix = myString.StartsWith(ignorePrefixChar);
                var array = myString.Split(kvSeparator.ToCharArray(), 2);
                string value = array[1];
                int idx = 0;
                string localeKey = null;
                if (array[0].Contains(idxSeparator))
                {
                    var arrayIdx = array[0].Split(idxSeparator.ToCharArray());
                    if (!int.TryParse(arrayIdx[1], out idx))
                    {
                        continue;
                    }
                    array[0] = arrayIdx[0];

                }
                if (array[0].Contains(localeKeySeparator))
                {
                    array = array[0].Split(localeKeySeparator.ToCharArray());
                    localeKey = array[1];
                }

                k = new Locale.Key()
                {
                    m_Identifier = noPrefix ? array[0].Substring(1) : prefix + array[0],
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
                loadLocaleIntern("en", false, prefix, packagePrefix);
            }
            if (setLocale)
            {
                language = localeId;
            }

        }
    }
}
