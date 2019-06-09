using Klyte.Commons.i18n;
using Klyte.TransportLinesManager.Utils;

namespace Klyte.TransportLinesManager.i18n
{
    public class TLMLocaleUtils : KlyteLocaleUtils<TLMLocaleUtils, TLMResourceLoader>
    {

        public override string prefix => "TLM_";

        protected override string packagePrefix => "Klyte.TransportLinesManager";
    }
}
