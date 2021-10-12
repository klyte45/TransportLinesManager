using ColossalFramework;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMPrefixConfiguration : IAssetSelectorStorage, INameableStorage, IColorSelectableStorage, IBasicExtensionStorage
    {
        [XmlElement("Budget")]        
        public TimeableList<BudgetEntryXml> BudgetEntries { get; set; } = new TimeableList<BudgetEntryXml>();
        [XmlElement("TicketPrices")]
        public TimeableList<TicketPriceEntryXml> TicketPriceEntries { get; set; } = new TimeableList<TicketPriceEntryXml>();
        [XmlElement("AssetsList")]
        public SimpleXmlList<string> AssetList { get; set; } = new SimpleXmlList<string>();
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("useColorForModel")]
        public bool UseColorForModel { get; set; }

        [XmlIgnore]
        public Color Color { get => m_cachedColor; set => m_cachedColor = value; }
        [XmlIgnore]
        private Color m_cachedColor;
        [XmlAttribute("color")]
        public string PropColorStr { get => m_cachedColor == default ? null : ColorExtensions.ToRGB(Color); set => m_cachedColor = value.IsNullOrWhiteSpace() ? default : (Color) ColorExtensions.FromRGB(value); }


        [XmlAttribute("customPalette")]
        public string CustomPalette { get; set; }


        [XmlIgnore]
        public LineIconSpriteNames CustomIcon { get; set; } = LineIconSpriteNames.NULL;
        [XmlAttribute("customFormat")]
        public string CustomFormatStr
        {
            get => CustomIcon.ToString();

            set {
                LineIconSpriteNames result;
                try
                {
                    result = (LineIconSpriteNames) Enum.Parse(typeof(LineIconSpriteNames), value);
                }
                catch
                {
                    result = (LineIconSpriteNames) Enum.ToObject(typeof(LineIconSpriteNames), (int.TryParse(value, out int val) ? val : 0));
                }
                CustomIcon = result;
            }
        }

        [XmlElement("DepotsAllowed")]
        public SimpleXmlHashSet<ushort> DepotsAllowed { get; set; } = new SimpleXmlHashSet<ushort>();

    }
}
