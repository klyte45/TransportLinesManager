using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    [XmlRoot("AssetConfiguration")]
    public class TLMAssetConfiguration
    {
        [XmlAttribute("capacity")]
        public int Capacity { get; set; }

    }
}
