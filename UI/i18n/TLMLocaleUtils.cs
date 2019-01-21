using ColossalFramework.Globalization;
using Klyte.Commons.i18n;
using Klyte.TransportLinesManager.Utils;
using System;

namespace Klyte.TransportLinesManager.i18n
{
    internal class TLMLocaleUtils : KlyteLocaleUtils<TLMLocaleUtils, TLMResourceLoader>
    {
        protected override string[] locales => new string[] { "en", "pt", "ko", "de", "cn", "pl", "nl", "fr", "es", "ru" };

        protected override string prefix => "TLM_";

        protected override string packagePrefix => "Klyte.TransportLinesManager";
    }
}
