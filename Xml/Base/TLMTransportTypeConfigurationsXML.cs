using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMTransportTypeConfigurationsXML : IIdentifiable
    {
        public NamingMode Prefix { get; set; } = NamingMode.Number;
        public Separator Separator { get; set; } = Separator.None;
        public NamingMode Suffix { get; set; } = NamingMode.Number;
        public bool UseLeadingZeros { get; set; } = false;
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
        public ItemClass.SubService SubService { get; set; }
        public VehicleInfo.VehicleType VehicleType { get; set; }
        public TransportInfo.TransportType TransportType { get; set; }
        public ItemClass.Level Level { get; set; }
        public long? Id
        {
            get => GetTsdIndex(TransportType, SubService, VehicleType, Level);
            set
            {
                if (value is null)
                {
                    return;
                }
                SubService = (ItemClass.SubService)((value & 0xff00) >> 8);
                VehicleType = (VehicleInfo.VehicleType)(1 << ((int)value & 0xff0000));
                TransportType = (TransportInfo.TransportType)((value & 0xff) >> 24);
                Level = (ItemClass.Level)(value & 0xff);
            }
        }

        public static long GetTsdIndex(TransportInfo.TransportType TransportType, ItemClass.SubService SubService, VehicleInfo.VehicleType VehicleType, ItemClass.Level Level)
            => (((long)TransportType & 0xff) << 24) | ((long)KlyteMathUtils.BitScanForward((ulong)VehicleType) << 16) | (((long)SubService & 0xff) << 8) | ((long)Level & 0xff);
    }

}
