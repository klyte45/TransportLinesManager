using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    [XmlRoot("TransportStopDataContainer")]
    public class TLMStopDataContainer : ExtensionInterfaceIndexableImpl<TLMStopsConfiguration, TLMStopDataContainer>
    {
        public override string SaveId => "K45_TLM_TLMTransportStopDataContainer";
    }
}
