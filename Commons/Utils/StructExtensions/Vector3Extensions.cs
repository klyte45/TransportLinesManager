using ColossalFramework.Math;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public static class Vector3Extensions
    {
        public static float GetAngleXZ(this Vector3 dir) => Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        public static float SqrDistance(this Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return (vector.x * vector.x) + (vector.y * vector.y) + (vector.z * vector.z);
        }

        public static Segment3 ToRayY(this Vector3 vector) => new Segment3(new Vector3(vector.x, -999999f, vector.z), new Vector3(vector.x, 999999f, vector.z));
    }
}
