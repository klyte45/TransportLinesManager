using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class MapUtils
    {
        #region Map Position

        public static Vector2 GridPositionGameDefault(Vector3 pos)
        {
            int x = Mathf.Max((int)((pos.x) / 64f + 135f), 0);
            int z = Mathf.Max((int)((-pos.z) / 64f + 135f), 0);
            return new Vector2(x, z);
        }


        public static Vector2 GridPosition81Tiles(Vector3 pos, float invResolution = 24f)
        {
            int x = Mathf.Max((int)((pos.x) / invResolution + 648), 0);
            int z = Mathf.Max((int)((-pos.z) / invResolution + 648), 0);
            return new Vector2(x, z);
        }

        public static Vector2 GetMapTile(Vector3 pos)
        {
            float x = (pos.x + 8640f) / 1920f;
            float z = (pos.z + 8640f) / 1920f;
            return new Vector2(x, z);
        }

        public static Vector3 CalculatePositionRelative(Vector3 absolutePos, float angle, Vector3 reference)
        {
            Vector3 relativePos = new Vector3
            {
                y = absolutePos.y - reference.y
            };

            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            //           position.x = original.x +              cos * offset.x +  sin   * offset.z;
            //position.z            =              original.z + sin * offset.x + (-cos) * offset.z;

            //                   cos * position.x = cos * original.x +                                  cos * cos * offset.x  + sin *   cos  * offset.z;
            //sin * position.z                    =                    sin * original.z +               sin * sin * offset.x  + sin * (-cos) * offset.z;
            //==========================================================================================================================================
            //sin * position.z + cos * position.x = cos * original.x + sin * original.z + (cos * cos + sin * sin) * offset.x;

            relativePos.x = -(cos * reference.x + sin * reference.z - sin * absolutePos.z - cos * absolutePos.x);
            relativePos.z = (-absolutePos.x + reference.x + cos * relativePos.x) / -sin;

            return relativePos;
        }

        public static Vector3 OffsetWithRotation(Vector3 offset, float angle)
        {
            Vector3 position = new Vector3
            {
                y = offset.y
            };

            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            position.x = cos * offset.x + sin * offset.z;
            position.z = sin * offset.x + (-cos) * offset.z;

            return position;
        }

        #endregion
    }
}
