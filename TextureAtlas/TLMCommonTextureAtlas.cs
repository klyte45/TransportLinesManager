using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

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
