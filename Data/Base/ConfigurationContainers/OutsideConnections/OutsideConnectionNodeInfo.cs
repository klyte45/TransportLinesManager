using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    public class OutsideConnectionNodeInfo : IIdentifiable
    {
        [XmlAttribute("outsideConnectionId")]
        public long? Id { get; set; }

        [XmlAttribute("nodeOC")]
        public ushort m_nodeOutsideConnection;

        [XmlAttribute("nodeStation")]
        public ushort m_nodeStation;

        [XmlAttribute("segmentToOC")]
        public ushort m_segmentToOutsideConnection;

        [XmlAttribute("segmentToStation")]
        public ushort m_segmentToStation;

    }
}
