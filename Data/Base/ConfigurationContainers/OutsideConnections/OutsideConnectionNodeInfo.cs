using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensions
{
    public class OutsideConnectionLineInfo : IIdentifiable
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

        [XmlAttribute("stringIdentifier")]
        public string Identifier { get; set; }

        [XmlAttribute("color")]
        public string LineColorStr { get => LineColor.ToRGB(); set => LineColor = ColorExtensions.FromRGB(value); }

        [XmlIgnore]
        public Color LineColor;
    }
}
