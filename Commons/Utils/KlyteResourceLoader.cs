using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public static class KlyteResourceLoader
    {
        public static string Prefix { get; } = "Klyte";

        public static string GetDefaultSpriteNameFor<E>(E value) where E : Enum => GetDefaultSpriteNameFor(value.ToString());
        public static string GetDefaultSpriteNameFor(string value)
        {
            if (value.StartsWith("__"))
            {
                return value.ToString().Substring(2);
            }
            if (value.StartsWith("K45_"))
            {
                return value;
            }
            return $"K45_{Prefix}_{value}";
        }

        public static byte[] LoadResourceData(string name)
        {
            name = $"{Prefix}.{name}"; 

            var stream = (UnmanagedMemoryStream) Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream == null)
            {
                LogUtils.DoLog("Could not find resource: " + name);
                return null;
            }

            var read = new BinaryReader(stream);
            return read.ReadBytes((int) stream.Length);
        }

        public static string LoadResourceString(string name)
        {
            name = $"{Prefix}.{name}";

            var stream = (UnmanagedMemoryStream) Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream == null)
            {
                LogUtils.DoLog("Could not find resource: " + name);
                return null;
            }

            var read = new StreamReader(stream);
            return read.ReadToEnd();
        }
        public static IEnumerable<string> LoadResourceStringLines(string name)
        {
            name = $"{Prefix}.{name}";

            using var stream = (UnmanagedMemoryStream) Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream == null)
            {
                LogUtils.DoLog("Could not find resource: " + name);
                yield break;
            }

            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static Texture2D LoadTexture(string filename)
        {
            try
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.LoadImage(LoadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

        public static AssetBundle LoadBundle(string filename)
        {
            try
            {
                return AssetBundle.LoadFromMemory(LoadResourceData(filename));
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

    }
}
