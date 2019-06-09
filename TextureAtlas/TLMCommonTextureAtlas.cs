using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.Utils;

namespace Klyte.TransportLinesManager.TextureAtlas
{
    public class TLMCommonTextureAtlas : TextureAtlasDescriptor<TLMCommonTextureAtlas, TLMResourceLoader>
    {
        protected override string ResourceName => "UI.Images.sprites.png";
        protected override string CommonName => "TransportLinesManagerSprites";
        public override string[] SpriteNames => new string[] {
                    "TransportLinesManagerIcon","TransportLinesManagerIconHovered","AutoNameIcon","AutoColorIcon","RemoveUnwantedIcon","ConfigIcon","24hLineIcon", "PerHourIcon","AbsoluteMode","RelativeMode","Copy","Paste"
                };
    }
}
