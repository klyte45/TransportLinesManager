using System.Linq;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class SerializationUtils
    {
        #region Default (de)serialization
        public static string SerializeColor(Color32 value, string separator) => string.Join(separator, new string[] { value.r.ToString(), value.g.ToString(), value.b.ToString() });

        public static Color DeserializeColor(string value, string separator)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var list = value.Split(separator.ToCharArray()).ToList();
                if (list.Count == 3 && byte.TryParse(list[0], out var r) && byte.TryParse(list[1], out var g) && byte.TryParse(list[2], out var b))
                {
                    return new Color32(r, g, b, 255);
                }
                else
                {
                    LogUtils.DoLog($"val = {value}; list = {string.Join(",", list.ToArray())} (Size {list.Count})");
                }
            }
            return Color.clear;
        }
        #endregion
    }
}
