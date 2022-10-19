using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{
    [XmlRoot("TransportType")]
    public class TLMTransportTypeConfigurationsXML : TsdIdentifiable, ITLMAutoNameConfigurable
    {
        private string vehicleIdentifierFormatLocal = "VWXYZ";
        private string vehicleIdentifierFormatForeign = "VWXYZ";

        [XmlAttribute("prefix")]
        public NamingMode Prefix { get; set; } = NamingMode.Number;
        [XmlAttribute("separator")]
        public Separator Separator { get; set; } = Separator.None;
        [XmlAttribute("suffix")]
        public NamingMode Suffix { get; set; } = NamingMode.Number;
        [XmlAttribute("nonPrefixed")]
        public NamingMode NonPrefixedNaming { get; set; } = NamingMode.Number;
        [XmlAttribute("leadingZeros")]
        public bool UseLeadingZeros { get; set; } = false;
        [XmlAttribute("incrementByPrefix")]
        public bool IncrementPrefixOnNewLine { get; set; } = false;
        [XmlAttribute("palette")]
        public string Palette { get; set; } = "";
        [XmlAttribute("randomPaletteOnOverflow")]
        public bool PaletteRandomOnOverflow { get; set; } = false;
        [XmlAttribute("paletteBasedOnPrefix")]
        public bool PalettePrefixBased { get; set; } = true;
        [XmlAttribute("showInLinearMap")]
        public bool ShowInLinearMap { get; set; } = true;
        [XmlAttribute("invertSuffixPrefix")]
        public bool InvertPrefixSuffix { get; set; } = false;
        [XmlAttribute("defaultCostPerPassenger")]
        public int DefaultCostPerPassenger { get; set; } = 0;
        [XmlAttribute("defaultTicketPrice")]
        public int DefaultTicketPrice { get; set; } = 0;
        [XmlAttribute("lineIcon")]
        public LineIconSpriteNames DefaultLineIcon { get; set; } = LineIconSpriteNames.NULL;
        [XmlAttribute("vehicleIdentifierLocal")]
        public string VehicleIdentifierFormatLocal { get => vehicleIdentifierFormatLocal; set => vehicleIdentifierFormatLocal = value.TrimToNull() ?? "VWXYZ"; }
        [XmlAttribute("VehicleIdentifierForeign")]
        public string VehicleIdentifierFormatForeign { get => vehicleIdentifierFormatForeign; set => vehicleIdentifierFormatForeign = value.TrimToNull() ?? "VWXYZ"; }
        [XmlAttribute("useInAutoname")]
        public bool UseInAutoName { get; set; }
        [XmlAttribute("buildingNamePrefix")]
        public string NamingPrefix { get; set; }
        [XmlAttribute("requireLineStartTerminal")]
        public bool RequireLineStartTerminal { get; set; } = true;

    }

}
