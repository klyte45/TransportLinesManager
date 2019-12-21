using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.Commons.Interfaces
{
    public abstract class TextureAtlasDescriptor<A, R> : Singleton<A> where A : TextureAtlasDescriptor<A, R> where R : KlyteResourceLoader<R>
    {
        protected virtual int Width => 64;
        protected virtual int Height => 64;
        protected abstract string ResourceName { get; }
        protected abstract string CommonName { get; }
        public abstract string[] SpriteNames { get; }
        protected UITextureAtlas m_atlas;

        public void Awake()
        {
            m_atlas = Singleton<R>.instance.CreateTextureAtlas(ResourceName, CommonName, (UIView.GetAView() ?? FindObjectOfType<UIView>()).defaultAtlas.material, Width, Height, SpriteNames);
        }

        public UITextureAtlas atlas => m_atlas;
    }
}
