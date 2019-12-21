using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.Utils;

namespace Klyte.TransportLinesManager.TextureAtlas
{
    public class CommonTextureAtlas : TextureAtlasDescriptor<CommonTextureAtlas, TLMResourceLoader>
    {
        protected override int Width => 64;
        protected override int Height => 64;
        protected override string ResourceName => "UI.Images.spritesCommon.png";
        protected override string CommonName => "CommonSprites";
        public override string[] SpriteNames => new string[] {
                    "K45Button", "K45ButtonHovered", "K45ButtonFocused", "K45ButtonDisabled"
                };

    }
}
