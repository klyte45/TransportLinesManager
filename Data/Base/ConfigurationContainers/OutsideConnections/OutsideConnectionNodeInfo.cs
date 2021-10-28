using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.ModShared;
using System;
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
        public string Identifier
        {
            get => identifier; set
            {
                identifier = value;
                TLMFacade.Instance?.OnRegionalLineParameterChanged(m_nodeStation);
            }
        }

        [XmlAttribute("color")]
        public string LineColorStr { get => LineColor.ToRGB(); set => LineColor = ColorExtensions.FromRGB(value); }

        [XmlAttribute("bgForm")]
        [Obsolete("Serialization Only!", true)]
        public string LineBgForm
        {
            get => LineBgSprite.ToString();
            set
            {
                try
                {
                    LineBgSprite = (LineIconSpriteNames)Enum.Parse(typeof(LineIconSpriteNames), value);
                }
                catch
                {
                    LineBgSprite = LineIconSpriteNames.K45_TriangleIcon;
                }
            }
        }

        [XmlIgnore]
        public LineIconSpriteNames LineBgSprite
        {
            get => lineBgSprite; set
            {
                lineBgSprite = value;
                TLMFacade.Instance?.OnRegionalLineParameterChanged(m_nodeStation);
            }
        }
        [XmlIgnore]
        public Color LineColor
        {
            get => lineColor; set
            {
                lineColor = value;
                TLMFacade.Instance?.OnRegionalLineParameterChanged(m_nodeStation);
            }
        }

        private Color lineColor;
        private string identifier;
        private LineIconSpriteNames lineBgSprite = LineIconSpriteNames.K45_TriangleIcon;
    }
}
