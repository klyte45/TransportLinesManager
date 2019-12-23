using ColossalFramework.Math;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class KlyteMathUtils
    {
        #region Math Utils

        public static float CalcBezierLenght(Bezier3 bz, float precision = 0.5f)
        {
            return CalcBezierLenght(bz.a, bz.b, bz.c, bz.d, precision);
        }
        public static float CalcBezierLenght(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float precision)
        {

            Vector3 aa = (-a + 3 * (b - c) + d);
            Vector3 bb = 3 * (a + c) - 6 * b;
            Vector3 cc = 3 * (b - a);

            int len = (int)(1.0f / precision);
            float[] arcLengths = new float[len + 1];
            arcLengths[0] = 0;

            Vector3 ov = a;
            Vector3 v;
            float clen = 0.0f;
            for (int i = 1; i <= len; i++)
            {
                float t = (i * precision);
                v = ((aa * t + (bb)) * t + cc) * t + a;
                clen += (ov - v).magnitude;
                arcLengths[i] = clen;
                ov = v;
            }
            return clen;

        }

        public static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public static Vector2 DegreeToVector2(float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }
        #endregion
    }
}
