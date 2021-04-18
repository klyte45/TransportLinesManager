using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMTransportTypeConfigurationsXML : TsdIdentifiable, ITLMAutoNameConfigurable
    {
        public NamingMode Prefix { get; set; } = NamingMode.Number;
        public Separator Separator { get; set; } = Separator.None;
        public NamingMode Suffix { get; set; } = NamingMode.Number;
        public NamingMode NonPrefixedNaming { get; set; } = NamingMode.Number;
        public bool UseLeadingZeros { get; set; } = false;
        public bool IncrementPrefixOnNewLine { get; set; } = false;
        public string Palette { get; set; } = null;
        public bool PaletteRandomOnOverflow { get; set; } = false;
        public bool PalettePrefixBased { get; set; } = true;
        public bool ShowInLinearMap { get; set; } = true;
        public bool InvertPrefixSuffix { get; set; } = false;
        public int DefaultCostPerPassenger { get; set; } = 0;
        public int DefaultTicketPrice { get; set; } = 0;
        public LineIconSpriteNames DefaultLineIcon { get; set; } = LineIconSpriteNames.K45_CircleIcon;
        public string VehicleIdentifierFormatLocal { get; set; } = "";
        public string VehicleIdentifierFormatForeign { get; set; } = "";
        public bool UseInAutoName { get; set; }
        public string NamingPrefix { get; set; }


    }

}
