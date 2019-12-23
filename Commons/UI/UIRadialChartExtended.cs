using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class UIRadialChartExtended : UISprite
    {
        public int sliceCount => m_Slices.Count;

        public SliceSettingsExtended GetSlice(int idx)
        {
            if (idx >= 0 && idx < sliceCount)
            {
                m_Slices[idx].Setter(this);
                return m_Slices[idx];
            }
            return null;
        }

        public void AddSlice()
        {
            m_Slices.Add(new SliceSettingsExtended());
            Invalidate();
        }

        public void RemoveSlice(int i)
        {
            m_Slices.RemoveAt(i);
            Invalidate();
        }

        public UIPivotPoint fillOrigin
        {
            get => m_FillOrigin;
            set {
                if (value != m_FillOrigin)
                {
                    m_FillOrigin = value;
                    Invalidate();
                }
            }
        }

        public void SetValues(params int[] percentages)
        {
            if (percentages.Length != sliceCount)
            {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.UI, string.Concat(new object[]
                {
                    "Percentage count should be ",
                    sliceCount,
                    " but is ",
                    percentages.Length
                }), base.gameObject);
                return;
            }
            float num = 0f;
            for (int i = 0; i < sliceCount; i++)
            {
                SliceSettingsExtended sliceSettings = m_Slices[i];
                sliceSettings.Setter(null);
                sliceSettings.startValue = Mathf.Max(num, 0f);
                num += percentages[i] * 0.01f;
                sliceSettings.endValue = Mathf.Min(num, 1f);
            }
            Invalidate();
        }

        public void SetValues(params float[] percentages)
        {
            if (percentages.Length != sliceCount)
            {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.UI, string.Concat(new object[]
                {
                    "Percentage count should be ",
                    sliceCount,
                    " but is ",
                    percentages.Length
                }), base.gameObject);
                return;
            }
            float num = 0f;
            for (int i = 0; i < sliceCount; i++)
            {
                SliceSettingsExtended sliceSettings = m_Slices[i];
                sliceSettings.Setter(null);
                sliceSettings.startValue = Mathf.Max(num, 0f);
                num += percentages[i];
                sliceSettings.endValue = Mathf.Min(num, 1f);
            }
            Invalidate();
        }

        protected override void OnRebuildRenderData()
        {
            if (!(base.atlas != null) || !(base.atlas.material != null) || !base.isVisible)
            {
                return;
            }
            if (base.spriteInfo == null)
            {
                return;
            }
            base.renderData.material = base.atlas.material;
            PoolList<Vector3> vertices = base.renderData.vertices;
            PoolList<int> triangles = base.renderData.triangles;
            PoolList<Vector2> uvs = base.renderData.uvs;
            PoolList<Color32> colors = base.renderData.colors;
            for (int i = 0; i < m_Slices.Count; i++)
            {
                BuildMeshData(vertices, triangles, uvs, colors, m_Slices[i]);
            }
        }

        private void BuildMeshData(PoolList<Vector3> vertices, PoolList<int> indices, PoolList<Vector2> uvs, PoolList<Color32> colors, SliceSettingsExtended slice)
        {
            using var poolList = PoolList<Vector3>.Obtain();
            poolList.AddRange(kBaseVerts.Select(x => x * slice.sizeMultiplier).ToArray());
            int num;
            int index;
            switch (fillOrigin)
            {
                case UIPivotPoint.TopLeft:
                    num = 4;
                    index = 5;
                    poolList.RemoveAt(6);
                    poolList.RemoveAt(0);
                    break;
                case UIPivotPoint.TopCenter:
                    num = 6;
                    index = 0;
                    break;
                case UIPivotPoint.TopRight:
                    num = 4;
                    index = 0;
                    poolList.RemoveAt(2);
                    poolList.RemoveAt(0);
                    break;
                case UIPivotPoint.MiddleLeft:
                    num = 6;
                    index = 6;
                    break;
                case UIPivotPoint.MiddleCenter:
                    num = 8;
                    poolList.Add(poolList[0]);
                    poolList.Insert(0, Vector3.zero);
                    index = 0;
                    break;
                case UIPivotPoint.MiddleRight:
                    num = 6;
                    index = 2;
                    break;
                case UIPivotPoint.BottomLeft:
                    num = 4;
                    index = 4;
                    poolList.RemoveAt(6);
                    poolList.RemoveAt(4);
                    break;
                case UIPivotPoint.BottomCenter:
                    num = 6;
                    index = 4;
                    break;
                case UIPivotPoint.BottomRight:
                    num = 4;
                    index = 2;
                    poolList.RemoveAt(4);
                    poolList.RemoveAt(2);
                    break;
                default:
                    throw new NotImplementedException();
            }
            StartClosestToPivot(poolList, index);
            using var poolList2 = PoolList<int>.Obtain();
            for (int i = 1; i < poolList.Count - 1; i++)
            {
                poolList2.Add(0);
                poolList2.Add(i);
                poolList2.Add(i + 1);
            }
            float num2 = 1f / num;
            float num3 = (1f - slice.startValue).Quantize(num2);
            float num4 = slice.endValue.Quantize(num2);
            int num5 = Mathf.CeilToInt(num4 / num2) + 1;
            int num6 = Mathf.CeilToInt(num3 / num2) + 1;
            for (int j = num5; j < num; j++)
            {
                if (base.invertFill)
                {
                    poolList2.RemoveRange(0, 3);
                }
                else
                {
                    poolList.RemoveAt(poolList.Count - 1);
                    poolList2.RemoveRange(poolList2.Count - 3, 3);
                }
            }
            for (int k = num6; k < num; k++)
            {
                if (base.invertFill)
                {
                    poolList.RemoveAt(poolList.Count - 1);
                    poolList2.RemoveRange(poolList2.Count - 3, 3);
                }
                else
                {
                    poolList2.RemoveRange(0, 3);
                }
            }
            var array = new Vector3[poolList.Count];
            poolList.CopyTo(array);
            if (slice.startValue > 0f)
            {
                int num7 = poolList2[base.invertFill ? (poolList2.Count - 2) : 2];
                int num8 = poolList2[base.invertFill ? (poolList2.Count - 1) : 1];
                float t = (1f - slice.startValue - num3) / num2;
                poolList[num8] = Vector3.Lerp(array[num7], array[num8], t);
            }
            if (slice.endValue < 1f)
            {
                int num9 = poolList2[base.invertFill ? 2 : (poolList2.Count - 2)];
                int num10 = poolList2[base.invertFill ? 1 : (poolList2.Count - 1)];
                float t2 = (slice.endValue - num4) / num2;
                poolList[num10] = Vector3.Lerp(array[num9], array[num10], t2);
            }
            BuildUV(uvs, poolList, slice.sizeMultiplier);
            float d = base.PixelsToUnits();
            Vector3 b = d * base.size;
            Vector3 b2 = base.pivot.TransformToCenter(base.size, base.arbitraryPivotOffset) * d;
            for (int l = 0; l < poolList.Count; l++)
            {
                poolList[l] = Vector3.Scale(poolList[l], b) + b2;
            }
            for (int m = 0; m < poolList2.Count; m++)
            {
                indices.Add(vertices.Count + poolList2[m]);
            }
            vertices.AddRange(poolList);
            BuildColors(colors, poolList.Count, slice.innerColor, slice.outterColor);
        }

        private void StartClosestToPivot(PoolList<Vector3> list, int index)
        {
            if (index == 0)
            {
                return;
            }
            PoolList<Vector3> range = list.GetRange(index, list.Count - index);
            list.RemoveRange(index, list.Count - index);
            list.InsertRange(0, range);
        }

        private void BuildUV(PoolList<Vector2> uvs, PoolList<Vector3> vertices, float multiplier)
        {
            UITextureAtlas.SpriteInfo spriteInfo = base.spriteInfo;
            if (spriteInfo == null)
            {
                return;
            }
            Rect region = spriteInfo.region;
            if (base.flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            {
                region = new Rect(region.xMax, region.y, -region.width, region.height);
            }
            if (base.flip.IsFlagSet(UISpriteFlip.FlipVertical))
            {
                region = new Rect(region.x, region.yMax, region.width, -region.height);
            }
            var b = new Vector2(region.x, region.y);
            var b2 = new Vector3(0.5f, 0.5f);
            var b3 = new Vector2(region.width, region.height);
            uvs.EnsureCapacity(vertices.Count * sliceCount);
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 a = (vertices[i] / multiplier) + b2;
                uvs.Add(Vector2.Scale(a, b3) + b);
            }
        }

        private Color MultiplyColors(Color lhs, Color rhs) => new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);

        private void BuildColors(PoolList<Color32> colors, int vertCount, Color32 inner, Color32 outter)
        {
            Color c = base.ApplyOpacity(base.isEnabled ? base.color : base.disabledColor);
            colors.EnsureCapacity(vertCount * sliceCount);
            for (int i = 0; i < vertCount; i++)
            {
                Color c2 = MultiplyColors((i == 0) ? inner : outter, c);
                Color item = c2.linear;
                colors.Add(item);
            }
        }

        private static readonly Vector3[] kBaseVerts = new Vector3[]
{
            new Vector3(0f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0f, -0.5f, 0f),
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(-0.5f, 0.5f, 0f)
};

        [SerializeField]
        protected List<SliceSettingsExtended> m_Slices = new List<SliceSettingsExtended>();

        [SerializeField]
        protected UIPivotPoint m_FillOrigin = UIPivotPoint.MiddleCenter;

        [Serializable]
        public class SliceSettingsExtended
        {
            public void Setter(UIRadialChartExtended chart) => m_Chart = chart;

            public float startValue
            {
                get => m_StartValue;
                set {
                    if (!Mathf.Approximately(value, m_StartValue))
                    {
                        m_StartValue = Mathf.Max(0f, Mathf.Min(m_EndValue, value));
                        if (m_Chart != null)
                        {
                            m_Chart.Invalidate();
                        }
                    }
                }
            }

            public float endValue
            {
                get => m_EndValue;
                set {
                    if (!Mathf.Approximately(value, m_EndValue))
                    {
                        m_EndValue = Mathf.Max(m_StartValue, Mathf.Min(1f, value));
                        if (m_Chart != null)
                        {
                            m_Chart.Invalidate();
                        }
                    }
                }
            }

            public Color32 innerColor
            {
                get => m_InnerColor;
                set {
                    if (!m_InnerColor.Equals(value))
                    {
                        m_InnerColor = value;
                        if (m_Chart != null)
                        {
                            m_Chart.Invalidate();
                        }
                    }
                }
            }

            public Color32 outterColor
            {
                get => m_OutterColor;
                set {
                    if (!m_OutterColor.Equals(value))
                    {
                        m_OutterColor = value;
                        if (m_Chart != null)
                        {
                            m_Chart.Invalidate();
                        }
                    }
                }
            }
            public float sizeMultiplier
            {
                get => m_SizeMultiplier;
                set {
                    if (!m_SizeMultiplier.Equals(value))
                    {
                        m_SizeMultiplier = Mathf.Max(0, Mathf.Min(1, value));
                        if (m_Chart != null)
                        {
                            m_Chart.Invalidate();
                        }
                    }
                }
            }

            private UIRadialChartExtended m_Chart;

            [SerializeField]
            protected float m_StartValue;

            [SerializeField]
            protected float m_EndValue = 1f;

            [SerializeField]
            protected float m_SizeMultiplier = 1f;

            [SerializeField]
            protected Color32 m_InnerColor;

            [SerializeField]
            protected Color32 m_OutterColor;
        }

        public void AddSlice(Color32 innerColor, Color32 outterColor, float multiplier = 1)
        {
            var slice = new SliceSettingsExtended
            {
                outterColor = outterColor,
                innerColor = innerColor,
                sizeMultiplier = multiplier
            };
            m_Slices.Add(slice);
            Invalidate();
        }
        public void SetValues(float offset, int[] percentages, int[] multipliers = null)
        {
            if (percentages.Length != sliceCount)
            {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.UI, string.Concat(new object[]
                {
            "Percentage count should be ",
            sliceCount,
            " but is ",
            percentages.Length
                }), base.gameObject);
                return;
            }
            if (multipliers != null && multipliers.Length != sliceCount)
            {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.UI, string.Concat(new object[]
                {
            "Multipliers count should be ",
            sliceCount,
            " but is ",
            multipliers.Length
                }), base.gameObject);
                return;
            }
            float multiplierDivider = multipliers.Max();
            float num = offset;
            for (int i = 0; i < sliceCount; i++)
            {
                SliceSettingsExtended sliceSettings = m_Slices[i];
                sliceSettings.Setter(null);
                sliceSettings.startValue = Mathf.Max(num % 1, 0f);
                num += percentages[i] * 0.01f;
                sliceSettings.endValue = Mathf.Min(num % 1, 1f);
                if (multipliers != null)
                {
                    sliceSettings.sizeMultiplier = multipliers[i] / multiplierDivider;
                }
            }
            Invalidate();
        }
        public void SetValuesStarts(int[] starts)
        {
            if (starts.Length != sliceCount)
            {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.UI, string.Concat(new object[]
                {
            "Starts count should be ",
            sliceCount,
            " but is ",
            starts.Length
                }), base.gameObject);
                return;
            }
            float num = 0;
            for (int i = 0; i < sliceCount; i++)
            {
                SliceSettingsExtended sliceSettings = m_Slices[i];
                sliceSettings.Setter(null);
                sliceSettings.startValue = num;
                if (i == sliceCount - 1)
                {
                    num = 1f;
                }
                else
                {
                    num = starts[i + 1] * 0.01f;
                }
                sliceSettings.endValue = num;
            }
            Invalidate();
        }
    }
}
