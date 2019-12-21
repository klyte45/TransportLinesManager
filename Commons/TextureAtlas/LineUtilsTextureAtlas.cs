using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.Commons.TextureAtlas
{
    public class LineUtilsTextureAtlas : TextureAtlasDescriptor<LineUtilsTextureAtlas, KCResourceLoader>
    {
        protected override string ResourceName => "UI.Images.lineFormat.png";
        protected override string CommonName => "KCLinearLineSprites";
        public override string[] SpriteNames => new string[] {
                        "MapIcon",
                        "OvalIcon",
                        "RoundedHexagonIcon",
                        "RoundedPentagonIcon",
                        "RoundedTriangleIcon",
                        "OctagonIcon",
                        "HeptagonIcon",
                        "10StarIcon",
                        "9StarIcon",
                        "7StarIcon",
                        "6StarIcon",
                        "5StarIcon",
                        "4StarIcon",
                        "3StarIcon",
                        "CameraIcon",
                        "MountainIcon",
                        "ConeIcon",
                        "TriangleIcon",
                        "CrossIcon",
                        "DepotIcon",
                        "LinearHalfStation",
                        "LinearStation",
                        "LinearBg",
                        "PentagonIcon",
                        "TrapezeIcon",
                        "DiamondIcon",
                        "8StarIcon",
                        "CableCarIcon",
                        "ParachuteIcon",
                        "HexagonIcon",
                        "SquareIcon",
                        "CircleIcon",
                        "RoundedSquareIcon",
                        "ShipIcon",
                        "AirplaneIcon",
                        "TaxiIcon",
                        "DayIcon",
                        "NightIcon",
                        "DisabledIcon",
                        "NoBudgetIcon",
                        "BulletTrainImage",
                        "LowBusImage",
                        "HighBusImage",
                        "VehicleLinearMap",
                        "RegionalTrainIcon"
                };
    }
}
