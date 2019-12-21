using ColossalFramework.Globalization;
using System;
using System.Linq;

namespace Klyte.TransportLinesManager.UI
{
    enum TLMLineIcon
    {
        NULL,
        Circle,
        Oval,
        Triangle,
        Square,
        Pentagon,
        Hexagon,
        Heptagon,
        Octagon,
        Cone,
        Parachute,
        RoundedTriangle,
        RoundedSquare,
        RoundedPentagon,
        RoundedHexagon,
        Diamond,
        Trapeze,
        Star3,
        Star4,
        Star5,
        Star6,
        Star7,
        Star8,
        Star9,
        Star10,
        Cross,
        Map,
        Camera,
        Mountain,
        Depot,

    }

    static class TLMLineIconExtension
    {
        public static string getImageName(this TLMLineIcon icon)
        {
            switch (icon)
            {
                case TLMLineIcon.Camera: return "CameraIcon";
                case TLMLineIcon.Mountain: return "MountainIcon";
                case TLMLineIcon.Cone: return "ConeIcon";
                case TLMLineIcon.Triangle: return "TriangleIcon";
                case TLMLineIcon.Cross: return "CrossIcon";
                case TLMLineIcon.Depot: return "DepotIcon";
                case TLMLineIcon.Pentagon: return "PentagonIcon";
                case TLMLineIcon.Trapeze: return "TrapezeIcon";
                case TLMLineIcon.Diamond: return "DiamondIcon";
                case TLMLineIcon.Star8: return "8StarIcon";
                case TLMLineIcon.Parachute: return "ParachuteIcon";
                default:
                case TLMLineIcon.Hexagon: return "HexagonIcon";
                case TLMLineIcon.Square: return "SquareIcon";
                case TLMLineIcon.Circle: return "CircleIcon";
                case TLMLineIcon.RoundedSquare: return "RoundedSquareIcon";
                case TLMLineIcon.Map: return "MapIcon";
                case TLMLineIcon.Oval: return "OvalIcon";
                case TLMLineIcon.RoundedHexagon: return "RoundedHexagonIcon";
                case TLMLineIcon.RoundedPentagon: return "RoundedPentagonIcon";
                case TLMLineIcon.RoundedTriangle: return "RoundedTriangleIcon";
                case TLMLineIcon.Octagon: return "OctagonIcon";
                case TLMLineIcon.Heptagon: return "HeptagonIcon";
                case TLMLineIcon.Star10: return "10StarIcon";
                case TLMLineIcon.Star9: return "9StarIcon";
                case TLMLineIcon.Star7: return "7StarIcon";
                case TLMLineIcon.Star6: return "6StarIcon";
                case TLMLineIcon.Star5: return "5StarIcon";
                case TLMLineIcon.Star4: return "4StarIcon";
                case TLMLineIcon.Star3: return "3StarIcon";

            }


        }


        public static string[] getDropDownOptions(string option0 = null)
        {
            var result = Enum.GetValues(typeof(TLMLineIcon)).OfType<TLMLineIcon>().Select(x =>
            {
                return Locale.Get("K45_TLM_LINE_ICON_ENUM", x.ToString());
            }).ToArray();

            if (option0 != null)
            {
                result[0] = option0;
            }
            return result;

        }
    }
}
