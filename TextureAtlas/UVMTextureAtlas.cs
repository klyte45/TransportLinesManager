using ColossalFramework.Globalization;
using static Klyte.TransportLinesManager.TextureAtlas.UVMTextureAtlas;

namespace Klyte.TransportLinesManager.TextureAtlas
{
    public class UVMTextureAtlas : TextureAtlasDescriptor<UVMTextureAtlas, IconName>
    {


        protected override int Width => 64;
        protected override int Height => 64;
        protected override string ResourceName
        {
            get {
                if (LocaleManager.instance.language.StartsWith("pt"))
                {
                    return "UI.TextureAtlas.icon_pt.png";
                }
                return "UI.TextureAtlas.icon.png";
            }
        }

        protected override string CommonName => "UVMTextureAtlas";

        public enum IconName
        {
            IconU, IconL, IconS, IconW, IconG, IconD, IconI,
            IconBus, Icon___, IconTram, IconMetro, IconMonorail, IconPassengerTrain, IconFerry, Icon____, IconBlimp, IconShip, IconAirplane, IconPath, IconSightseenBus, IconEvacBus
        }
    }
}
