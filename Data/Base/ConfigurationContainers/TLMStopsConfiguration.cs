using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    public class TLMStopsConfiguration : IIdentifiable
    {
        private bool isTerminal = false;

        [XmlAttribute("stopId")]
        public long? Id { get; set; }

        [XmlAttribute("isTerminal")]
        public bool IsTerminal
        {
            get => isTerminal; set
            {
                isTerminal = value;
                if (!LoadingManager.instance.m_currentlyLoading)
                {
                    TLMController.Instance?.SharedInstance.OnLineDestinationsChanged(NetManager.instance.m_nodes.m_buffer[Id ?? 0].m_transportLine);
                }
            }
        }
    }
}
