using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class RandomPastelColorGenerator
    {
        private readonly System.Random _random;

        public RandomPastelColorGenerator()
        {
            // seed the generator with 2 because
            // this gives a good sequence of colors
            const int RandomSeed = 2;
            _random = new System.Random(RandomSeed);
        }


        /// <summary>
        /// Returns a random pastel color
        /// </summary>
        /// <returns></returns>
        public Color32 GetNext()
        {
            // to create lighter colours:
            // take a random integer between 0 & 128 (rather than between 0 and 255)
            // and then add 64 to make the colour lighter
            byte[] colorBytes = new byte[3];
            colorBytes[0] = (byte)(_random.Next(128) + 64);
            colorBytes[1] = (byte)(_random.Next(128) + 64);
            colorBytes[2] = (byte)(_random.Next(128) + 64);
            Color32 color = new Color32
            {

                // make the color fully opaque
                a = 255,
                r = colorBytes[0],
                g = colorBytes[1],
                b = colorBytes[2]
            };
            LogUtils.DoLog(color.ToString());

            return color;
        }
    }

}

