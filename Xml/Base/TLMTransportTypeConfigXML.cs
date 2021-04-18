using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.Extensions;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMTransportTypeConfigXML : ExtensionInterfaceIndexableImpl<TLMTransportTypeExtension, TLMTransportTypeConfigXML>
    {
        public override string SaveId => "K45_TLM_TransportTypeConfig";
    }
}
