using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.TextureAtlas
{
    public abstract class TextureAtlasDescriptor<A, E> : Singleton<A> where A : TextureAtlasDescriptor<A, E> where E : Enum
    {
        protected virtual int Width => 64;
        protected virtual int Height => 64;
        protected abstract string ResourceName { get; }
        protected abstract string CommonName { get; }

        protected UITextureAtlas m_atlas;

        public void Awake() => m_atlas = CreateTextureAtlas(ResourceName, CommonName, (UIView.GetAView() ?? FindObjectOfType<UIView>()).defaultAtlas.material, Width, Height, Enum.GetNames(typeof(E)));

        public UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            var tex = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            { // LoadTexture
                tex.LoadImage(KlyteResourceLoader.LoadResourceData(textureFile));
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


        public UITextureAtlas Atlas => m_atlas;
    }
}
