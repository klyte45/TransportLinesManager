using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    public class TLMStopsConfiguration : IIdentifiable
    {
        private bool isTerminus = false;

        [XmlIgnore]
        public long? Id { get; set; }

        [XmlAttribute("isTerminus")]
        public bool IsTerminus
        {
            get => isTerminus; set
            {
                isTerminus = value;
                TLMController.Instance.SharedInstance.OnLineDestinationsChanged(NetManager.instance.m_nodes.m_buffer[Id ?? 0].m_transportLine);
            }
        }
    }
}
