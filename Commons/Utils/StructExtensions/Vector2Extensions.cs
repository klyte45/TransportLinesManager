using UnityEngine;

namespace Klyte.Commons.Utils
{
    public static class Vector2Extensions
    {
        public static float GetAngleToPoint(this Vector2 from, Vector2 to)
        {
            float ca = to.x - from.x;
            float co = -to.y + from.y;
            //LogUtils.DoLog($"ca = {ca},co = {co};");
            if (co == 0)
            {
                if (ca < 0)
                {
                    return 270;
                }
                else
                {
                    return 90;
                }
            }
            if (co < 0)
            {
                return (360 - ((Mathf.Atan(ca / co) * Mathf.Rad2Deg + 360) % 360) % 360);
            }
            else
            {
                return 360 - ((Mathf.Atan(ca / co) * Mathf.Rad2Deg + 180 + 360) % 360);
            }
        }
    }
}
