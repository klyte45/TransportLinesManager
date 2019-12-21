using ColossalFramework;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.Utils;
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
                TLMUtils.doLog("Could not find resource: " + name);
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
                TLMUtils.doLog("Could not find resource: " + name);
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
                TLMUtils.doLog("Could not find resource: " + name);
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
                TLMUtils.doErrorLog("The file could not be read:" + e.Message);
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
                TLMUtils.doErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

    }
    public abstract class KlyteResourceLoader<T> : Singleton<T> where T : KlyteResourceLoader<T>
    {
        protected abstract string prefix { get; }
        private Type resourceReference => typeof(T);
        public virtual Shader GetLoadedShader(string shaderName) => null;

        public byte[] loadResourceData(string name)
        {
            name = prefix + name;

            var stream = (UnmanagedMemoryStream) Assembly.GetAssembly(resourceReference).GetManifestResourceStream(name);
            if (stream == null)
            {
                KlyteUtils.doErrorLog("Could not find resource: " + name);
                return null;
            }

            var read = new BinaryReader(stream);
            return read.ReadBytes((int) stream.Length);
        }

        public string loadResourceString(string name)
        {
            name = prefix + name;

            var stream = (UnmanagedMemoryStream) Assembly.GetAssembly(resourceReference).GetManifestResourceStream(name);
            if (stream == null)
            {
                KlyteUtils.doErrorLog("Could not find resource: " + name);
                return null;
            }

            var read = new StreamReader(stream);
            return read.ReadToEnd();
        }

        public Texture2D loadTexture(int x, int y, string filename)
        {
            try
            {
                var texture = new Texture2D(x, y);
                texture.LoadImage(loadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                KlyteUtils.doErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

        public AssetBundle loadBundle(string filename)
        {
            try
            {
                return AssetBundle.LoadFromMemory(loadResourceData(filename));
            }
            catch (Exception e)
            {
                KlyteUtils.doErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

        public UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            var tex = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            { // LoadTexture
                tex.LoadImage(loadResourceData(textureFile));
                tex.Apply(true, true);
            }
            UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            { // Setup atlas
                var material = Material.Instantiate(baseMaterial);
                material.mainTexture = tex;
                atlas.material = material;
                atlas.name = atlasName;
            }
            // Add sprites
            for (int i = 0; i < spriteNames.Length; ++i)
            {
                float uw = 1.0f / spriteNames.Length;
                var spriteInfo = new UITextureAtlas.SpriteInfo()
                {
                    name = spriteNames[i],
                    texture = tex,
                    region = new Rect(i * uw, 0, uw, 1),
                };
                atlas.AddSprite(spriteInfo);
            }
            return atlas;
        }
    }

    public sealed class KCResourceLoader : KlyteResourceLoader<KCResourceLoader>
    {
        protected override string prefix => "Klyte.Commons.";
    }
}
