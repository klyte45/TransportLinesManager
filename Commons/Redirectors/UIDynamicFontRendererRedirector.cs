using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static ColossalFramework.UI.UIDynamicFont;

namespace Klyte.Commons.Redirectors
{

    public class UIDynamicFontRendererRedirector : Redirector, IRedirectable
    {
        public const string TAG_LINE = "k45Symbol";
        public readonly string[] LEGACY_TAG_LINE = new string[] {"k45LineSymbol"};

        private static UIDynamicFontRendererRedirector Instance { get; set; }

        public Redirector RedirectorInstance => Instance;
        #region Awake 
        public void Awake()
        {
            if (GetList().Contains(TAG_LINE))
            {
                Destroy(this);
                return;
            }
            Instance = this;

            GetList().Add(TAG_LINE);
            GetList().AddRange(LEGACY_TAG_LINE);


            AddRedirect(typeof(DynamicFontRenderer).GetMethod("CalculateTokenRenderSize", RedirectorUtils.allFlags), null, null, GetType().GetMethod("CalculateTokenRenderSizeTranspile", RedirectorUtils.allFlags));
            AddRedirect(typeof(DynamicFontRenderer).GetMethod("RenderLine", RedirectorUtils.allFlags), null, null, GetType().GetMethod("RenderLineTranspile", RedirectorUtils.allFlags));

        }

        public static IEnumerable<CodeInstruction> CalculateTokenRenderSizeTranspile(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            CodeInstruction returnInst = inst[inst.Count - 1];
            for (int i = 0; i < inst.Count - 1; i++)
            {
                if (inst[i].operand is Label lbl && (inst[i].opcode == OpCodes.Br || inst[i].opcode == OpCodes.Br_S) && returnInst.labels.Contains(lbl))
                {
                    inst[i].opcode = OpCodes.Ret;
                    inst[i].operand = null;
                }
            }

            var firstCode = new CodeInstruction(OpCodes.Ldarg_0)
            {
                labels = inst[inst.Count - 1].labels
            };
            inst[inst.Count - 1].labels = new List<Label>();
            inst.InsertRange(inst.Count - 1, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ret),
                firstCode,
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call,typeof(UIDynamicFontRendererRedirector).GetMethod("CalcTokenRenderSizeNewTags")),
            });
            int j = 0;
            LogUtils.DoLog($"TRANSPILLED:\n\t{string.Join("\n\t", inst.Select(x => $"{(j++).ToString("D8")} {x.opcode.ToString().PadRight(10)} {ParseOperand(inst, x.operand)}").ToArray())}");
            return inst;
        }

        private static string ParseOperand(List<CodeInstruction> instr, object operand)
        {
            if (operand is null)
            {
                return null;
            }

            if (operand is Label lbl)
            {
                return "LBL: " + instr.Select((x, y) => Tuple.New(x, y)).Where(x => x.First.labels.Contains(lbl)).Select(x => $"{x.Second.ToString("D8")} {x.First.opcode.ToString().PadRight(10)} {ParseOperand(instr, x.First.operand)}").FirstOrDefault();
            }
            else
            {
                return operand.ToString();
            }
        }

        public static IEnumerable<CodeInstruction> RenderLineTranspile(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            CodeInstruction returnInst = inst[inst.Count - 1];
            for (int i = 0; i < inst.Count - 3; i++)
            {
                if (inst[i].opcode == OpCodes.Ldstr && inst[i].operand is string str && str == "color")
                {
                    var newRef = new Label();
                    object oldRef = inst[i + 2].operand;
                    inst[i + 2].opcode = OpCodes.Brtrue;
                    inst[i + 2].operand = newRef;
                    inst[i + 3].labels.Add(newRef);

                    inst.InsertRange(i + 3, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldstr, TAG_LINE),
                        new CodeInstruction(OpCodes.Call,typeof(UIDynamicFontRendererRedirector).GetMethod("Matches")),
                        new CodeInstruction(OpCodes.Brfalse, oldRef),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Ldarg_S, 4),
                        new CodeInstruction(OpCodes.Call,typeof(UIDynamicFontRendererRedirector).GetMethod("RenderSpriteLine")),
                        new CodeInstruction(OpCodes.Br, oldRef),
                    });

                    break;
                }
            }

            int j = 0;
            LogUtils.DoLog($"TRANSPILLED:\n\t{string.Join("\n\t", inst.Select(x => $"{(j++).ToString("D8")} {x.opcode.ToString().PadRight(10)} {ParseOperand(inst, x.operand)}").ToArray())}");
            return inst;
        }

        public static bool Matches(UIMarkupToken token, string text)
        {
            int length = token.length;
            if (length != text.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (char.ToLower(token.source[token.startOffset + i]) != char.ToLower(text[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static void CalcTokenRenderSizeNewTags(DynamicFontRenderer renderer, UIMarkupToken token)
        {
            if (token.tokenType == UIMarkupTokenType.StartTag && Matches(token, TAG_LINE) && renderer.spriteAtlas != null && token.GetAttribute(0).m_Value.value.Split(',').Length == 3)
            {
                float num = 0f;
                Texture2D texture = renderer.spriteAtlas.texture;
                float num3 = ((UIDynamicFont) renderer.font).baseline * renderer.textScale * 2.5f;
                string value = token.GetAttribute(0).m_Value.value.Split(',')[0];
                UITextureAtlas.SpriteInfo spriteInfo = renderer.spriteAtlas[value];
                if (spriteInfo != null)
                {
                    float num4 = spriteInfo.region.width * texture.width / (spriteInfo.region.height * texture.height);
                    num = Mathf.CeilToInt((num3 * num4));
                }
                token.height = Mathf.CeilToInt(num3);
                typeof(UIMarkupToken).GetProperty("width").GetSetMethod(true).Invoke(token, new object[] { Mathf.CeilToInt(num) });
            }
        }

        private static List<string> GetList() => (List<string>) typeof(UIMarkupTokenizer).GetField("kValidTags", RedirectorUtils.allFlags).GetValue(null);

        public static void RenderSpriteLine(DynamicFontRenderer renderer, UIMarkupToken token, Vector3 position, UIRenderData destination)
        {
            string[] args = token.GetAttribute(0)?.m_Value?.value?.Split(new char[] { ',' }, 3);
            if (args == null || args.Length != 3)
            {
                LogUtils.DoErrorLog($"INVALID ARGUMENT: {token.GetAttribute(0)?.m_Value?.value ?? "<NULL>"}");
                return;
            }

            UITextureAtlas.SpriteInfo spriteInfo = renderer.spriteAtlas[args[0]];
            if (spriteInfo == null)
            {
                CODebugBase<InternalLogChannel>.Warn(InternalLogChannel.UI, "Missing sprite " + args[0] + " in " + renderer.spriteAtlas.name);
            }
            else
            {
                var midLine = new Vector3(position.x, position.y - (((UIDynamicFont) renderer.font).baseline * renderer.textScale / 2));

                Texture2D texture = renderer.spriteAtlas.texture;
                float calcHeight = ((UIDynamicFont) renderer.font).baseline * renderer.textScale * 2f;
                float calcProportion = spriteInfo.region.width * texture.width / (spriteInfo.region.height * texture.height);
                float calcWidth = Mathf.CeilToInt(calcHeight * calcProportion);

                int height = Mathf.CeilToInt(calcHeight);
                int width = Mathf.CeilToInt(calcWidth);

                Color32 bgColor = ColorExtensions.FromRGB(args[1]);
                Color32 color2 = ApplyOpacity(renderer, bgColor);
                var size = new Vector3(width, height);
                Vector2 textDimensions = MeasureTextWidth(renderer, args[2], renderer.textScale, out Vector2 yBounds);

                float imageProportions = width / spriteInfo.width;

                float borderWidth = renderer.textScale * 2;

                float textBoundHeight = height - (spriteInfo.border.vertical * imageProportions) - borderWidth;
                float textBoundWidth = width - (spriteInfo.border.horizontal * imageProportions) - borderWidth;

                var textAreaSize = new Vector4((spriteInfo.border.left * imageProportions) + (borderWidth / 2), (-spriteInfo.border.top + spriteInfo.border.bottom) * imageProportions / 2, textBoundWidth, textBoundHeight);

                float textScale = renderer.textScale;
                float multipler = Mathf.Min(Mathf.Min(3.5f, textAreaSize.z / textDimensions.x), Mathf.Min(3.5f, textAreaSize.w / textDimensions.y));
                if (multipler > 1)
                {
                    textScale *= 1 + ((multipler - 1) / 2.1f);
                    multipler = 1;
                    textDimensions = MeasureTextWidth(renderer, args[2], textScale, out yBounds);
                }
                float midLineOffset = (((UIDynamicFont) renderer.font).baseline / 2 * renderer.textScale);

                Color contrastColor = KlyteMonoUtils.ContrastColor(bgColor);

                RenderSprite(renderer.spriteBuffer, new RenderOptions
                {
                    atlas = renderer.spriteAtlas,
                    color = contrastColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = position - new Vector3(0, -(height / 2) + midLineOffset),
                    pixelsToUnits = renderer.pixelRatio,
                    size = size,
                    spriteInfo = spriteInfo
                });

                RenderSprite(renderer.spriteBuffer, new RenderOptions
                {
                    atlas = renderer.spriteAtlas,
                    color = color2,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = position - new Vector3(0, -(height / 2) + midLineOffset) + (new Vector3(borderWidth, -borderWidth) / 2),
                    pixelsToUnits = renderer.pixelRatio,
                    size = size - new Vector3(borderWidth, borderWidth),
                    spriteInfo = spriteInfo
                });
                midLineOffset = ((UIDynamicFont) renderer.font).baseline * textScale;
                Vector3 targetTextPos = midLine + VectorUtils.XY_(textAreaSize) + (new Vector3(textAreaSize.z - (textDimensions.x * multipler), -textDimensions.y) / 2);
                RenderText(renderer, args[2], targetTextPos, multipler, destination, textScale, contrastColor, bgColor);

            }
        }

        private static Vector2 MeasureTextWidth(DynamicFontRenderer renderer, string text, float textScale, out Vector2 yBounds)
        {
            float width = 1f;
            float height = 0;
            int size = Mathf.CeilToInt(((UIDynamicFont) renderer.font).baseline * textScale) * 2;
            ((UIDynamicFont) renderer.font).RequestCharacters(text, size, FontStyle.Normal);
            yBounds = new Vector2(float.MaxValue, float.MinValue);
            CharacterInfo characterInfo = default;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                ((UIDynamicFont) renderer.font).baseFont.GetCharacterInfo(c, out characterInfo, size, FontStyle.Normal);
                if (c == '\t')
                {
                    width += renderer.tabSize;
                }
                else
                {
                    width += ((c != ' ') ? characterInfo.maxX : (characterInfo.advance + (renderer.characterSpacing * textScale)));
                    height = Mathf.Max(characterInfo.glyphHeight, height);
                    yBounds.x = Mathf.Min(yBounds.x, characterInfo.minY);
                    yBounds.y = Mathf.Max(yBounds.y, characterInfo.maxY);
                }
            }
            if (text.Length > 2)
            {
                width += (text.Length - 2) * renderer.characterSpacing * textScale;
            }
            return new Vector2(width, height) / 2f;
        }

        private static void RenderText(DynamicFontRenderer renderer, string text, Vector3 position, float rescale, UIRenderData renderData, float textScale, Color textColor, Color outlineColor)
        {
            var uidynamicFont = (UIDynamicFont) renderer.font;
            float size = ((UIDynamicFont) renderer.font).baseline * textScale;
            FontStyle style = FontStyle.Normal;
            int descent = uidynamicFont.Descent * 2;
            int ascent = renderer.font.baseFont.ascent * 2;
            PoolList<Vector3> vertices = renderData.vertices;
            PoolList<int> triangles = renderData.triangles;
            PoolList<Vector2> uvs = renderData.uvs;
            PoolList<Color32> colors = renderData.colors;
            float x = position.x;
            float y = position.y;
            renderData.material = uidynamicFont.material;
            Color color2 = ApplyOpacity(renderer, textColor);
            Color c = color2;
            if (renderer.bottomColor != null)
            {
                c = ApplyOpacity(renderer, textColor);
            }
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0)
                {
                    x += renderer.characterSpacing * textScale;
                }
                if (uidynamicFont.baseFont.GetCharacterInfo(text[i], out CharacterInfo glyph, Mathf.CeilToInt(size) * 2, style))
                {
                    float num3 = (glyph.maxY / 2f);
                    float minX = x + (glyph.minX / 2f);
                    float maxY = y + num3;
                    float maxX = minX + (glyph.glyphWidth * rescale / 2f);
                    float minY = maxY - (glyph.glyphHeight / 2f);
                    Vector3 vector = new Vector3(minX, maxY) * renderer.pixelRatio;
                    Vector3 vector2 = new Vector3(maxX, maxY) * renderer.pixelRatio;
                    Vector3 vector3 = new Vector3(maxX, minY) * renderer.pixelRatio;
                    Vector3 vector4 = new Vector3(minX, minY) * renderer.pixelRatio;
                    if (renderer.shadow)
                    {
                        AddTriangleIndices(vertices, triangles);
                        Vector3 b = renderer.shadowOffset * renderer.pixelRatio;
                        vertices.Add(vector + b);
                        vertices.Add(vector2 + b);
                        vertices.Add(vector3 + b);
                        vertices.Add(vector4 + b);
                        Color c2 = ApplyOpacity(renderer, renderer.shadowColor);
                        Color32 item = c2.linear;
                        colors.Add(item);
                        colors.Add(item);
                        colors.Add(item);
                        colors.Add(item);
                        AddUVCoords(uvs, glyph);
                    }
                    if (outlineColor != default)
                    {
                        for (int j = 0; j < kOutlineOffsets.Length; j++)
                        {
                            AddTriangleIndices(vertices, triangles);
                            Vector3 b2 = kOutlineOffsets[j] * renderer.outlineSize * renderer.pixelRatio;
                            vertices.Add(vector + b2);
                            vertices.Add(vector2 + b2);
                            vertices.Add(vector3 + b2);
                            vertices.Add(vector4 + b2);
                            Color c3 = ApplyOpacity(renderer, outlineColor);
                            Color32 item2 = c3.linear;
                            colors.Add(item2);
                            colors.Add(item2);
                            colors.Add(item2);
                            colors.Add(item2);
                            AddUVCoords(uvs, glyph);
                        }
                    }
                    AddTriangleIndices(vertices, triangles);
                    vertices.Add(vector);
                    vertices.Add(vector2);
                    vertices.Add(vector3);
                    vertices.Add(vector4);
                    Color32 item3 = color2.linear;
                    Color32 item4 = c.linear;
                    colors.Add(item3);
                    colors.Add(item3);
                    colors.Add(item4);
                    colors.Add(item4);
                    AddUVCoords(uvs, glyph);
                    x += glyph.maxX / 2f * rescale;
                }
            }
        }
        private static void AddUVCoords(PoolList<Vector2> uvs, CharacterInfo glyph)
        {
            uvs.Add(glyph.uvTopLeft);
            uvs.Add(glyph.uvTopRight);
            uvs.Add(glyph.uvBottomRight);
            uvs.Add(glyph.uvBottomLeft);
        }

        private static void AddTriangleIndices(PoolList<Vector3> verts, PoolList<int> triangles)
        {
            int count = verts.Count;
            int[] array = kTriangleIndices;
            for (int i = 0; i < array.Length; i++)
            {
                triangles.Add(count + array[i]);
            }
        }
        private static Color32 ApplyOpacity(UIFontRenderer renderer, Color32 color)
        {
            color.a = (byte) (renderer.opacity * 255f);
            return color;
        }

        internal static Rect RenderSprite(UIRenderData data, RenderOptions options)
        {
            options.baseIndex = data.vertices.Count;
            RebuildTriangles(data, options);
            Rect bounds = RebuildVertices(data, options);
            RebuildUV(data, options);
            RebuildColors(data, options);
            if (options.fillAmount < 1f)
            {
                DoFill(data, options);
            }
            return bounds;
        }
        internal static readonly int[] kTriangleIndices = new int[]{
            0,
            1,
            3,
            3,
            1,
            2
        };

        private static Vector2[] kOutlineOffsets = new Vector2[]
            {
                new Vector2(-1f, -1f),
                new Vector2(-1f, 1f),
                new Vector2(1f, -1f),
                new Vector2(1f, 1f)
            };
        private static void RebuildTriangles(UIRenderData renderData, RenderOptions options)
        {
            int baseIndex = options.baseIndex;
            PoolList<int> triangles = renderData.triangles;
            triangles.EnsureCapacity(triangles.Count + kTriangleIndices.Length);
            for (int i = 0; i < kTriangleIndices.Length; i++)
            {
                triangles.Add(baseIndex + kTriangleIndices[i]);
            }
        }

        private static Rect RebuildVertices(UIRenderData renderData, RenderOptions options)
        {
            PoolList<Vector3> vertices = renderData.vertices;
            int baseIndex = options.baseIndex;
            float x = 0f;
            float y = 0f;
            float x2 = Mathf.Ceil(options.size.x);
            float y2 = Mathf.Ceil(-options.size.y);
            vertices.Add(new Vector3(x, y, 0f) * options.pixelsToUnits);
            vertices.Add(new Vector3(x2, y, 0f) * options.pixelsToUnits);
            vertices.Add(new Vector3(x2, y2, 0f) * options.pixelsToUnits);
            vertices.Add(new Vector3(x, y2, 0f) * options.pixelsToUnits);
            Vector3 b = (options.offset * 10).RoundToInt() * options.pixelsToUnits / 10;
            for (int i = 0; i < 4; i++)
            {
                vertices[baseIndex + i] = (vertices[baseIndex + i] + b).Quantize(options.pixelsToUnits);
            }
            return new Rect(vertices[0], vertices[2]);
        }
        private static void RebuildUV(UIRenderData renderData, RenderOptions options)
        {
            Rect region = options.spriteInfo.region;
            PoolList<Vector2> uvs = renderData.uvs;
            uvs.Add(new Vector2(region.x, region.yMax));
            uvs.Add(new Vector2(region.xMax, region.yMax));
            uvs.Add(new Vector2(region.xMax, region.y));
            uvs.Add(new Vector2(region.x, region.y));
            Vector2 value = Vector2.zero;
            if (options.flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            {
                value = uvs[1];
                uvs[1] = uvs[0];
                uvs[0] = value;
                value = uvs[3];
                uvs[3] = uvs[2];
                uvs[2] = value;
            }
            if (options.flip.IsFlagSet(UISpriteFlip.FlipVertical))
            {
                value = uvs[0];
                uvs[0] = uvs[3];
                uvs[3] = value;
                value = uvs[1];
                uvs[1] = uvs[2];
                uvs[2] = value;
            }
        }
        private static void DoFill(UIRenderData renderData, RenderOptions options)
        {
            int baseIndex = options.baseIndex;
            PoolList<Vector3> vertices = renderData.vertices;
            PoolList<Vector2> uvs = renderData.uvs;
            int index = baseIndex + 3;
            int index2 = baseIndex + 2;
            int index3 = baseIndex;
            int index4 = baseIndex + 1;
            if (options.invertFill)
            {
                if (options.fillDirection == UIFillDirection.Horizontal)
                {
                    index = baseIndex + 1;
                    index2 = baseIndex;
                    index3 = baseIndex + 2;
                    index4 = baseIndex + 3;
                }
                else
                {
                    index = baseIndex;
                    index2 = baseIndex + 1;
                    index3 = baseIndex + 3;
                    index4 = baseIndex + 2;
                }
            }
            if (options.fillDirection == UIFillDirection.Horizontal)
            {
                vertices[index2] = Vector3.Lerp(vertices[index2], vertices[index], 1f - options.fillAmount);
                vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index3], 1f - options.fillAmount);
                uvs[index2] = Vector2.Lerp(uvs[index2], uvs[index], 1f - options.fillAmount);
                uvs[index4] = Vector2.Lerp(uvs[index4], uvs[index3], 1f - options.fillAmount);
                return;
            }
            vertices[index3] = Vector3.Lerp(vertices[index3], vertices[index], 1f - options.fillAmount);
            vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index2], 1f - options.fillAmount);
            uvs[index3] = Vector2.Lerp(uvs[index3], uvs[index], 1f - options.fillAmount);
            uvs[index4] = Vector2.Lerp(uvs[index4], uvs[index2], 1f - options.fillAmount);
        }
        private static void RebuildColors(UIRenderData renderData, RenderOptions options)
        {
            Color32 item = options.color.linear;
            PoolList<Color32> colors = renderData.colors;
            for (int i = 0; i < 4; i++)
            {
                colors.Add(item);
            }
        }
        internal struct RenderOptions
        {
            // Token: 0x17000040 RID: 64
            // (get) Token: 0x06000164 RID: 356 RVA: 0x0000867A File Offset: 0x0000687A
            // (set) Token: 0x06000165 RID: 357 RVA: 0x00008682 File Offset: 0x00006882
            public UITextureAtlas atlas
            {
                get => m_Atlas;
                set => m_Atlas = value;
            }

            // Token: 0x17000041 RID: 65
            // (get) Token: 0x06000166 RID: 358 RVA: 0x0000868B File Offset: 0x0000688B
            // (set) Token: 0x06000167 RID: 359 RVA: 0x00008693 File Offset: 0x00006893
            public UITextureAtlas.SpriteInfo spriteInfo
            {
                get => m_SpriteInfo;
                set => m_SpriteInfo = value;
            }

            // Token: 0x17000042 RID: 66
            // (get) Token: 0x06000168 RID: 360 RVA: 0x0000869C File Offset: 0x0000689C
            // (set) Token: 0x06000169 RID: 361 RVA: 0x000086A4 File Offset: 0x000068A4
            public Color color
            {
                get => m_Color;
                set => m_Color = value;
            }

            // Token: 0x17000043 RID: 67
            // (get) Token: 0x0600016A RID: 362 RVA: 0x000086AD File Offset: 0x000068AD
            // (set) Token: 0x0600016B RID: 363 RVA: 0x000086B5 File Offset: 0x000068B5
            public float pixelsToUnits
            {
                get => m_PixelsToUnits;
                set => m_PixelsToUnits = value;
            }

            // Token: 0x17000044 RID: 68
            // (get) Token: 0x0600016C RID: 364 RVA: 0x000086BE File Offset: 0x000068BE
            // (set) Token: 0x0600016D RID: 365 RVA: 0x000086C6 File Offset: 0x000068C6
            public Vector2 size
            {
                get => m_Size;
                set => m_Size = value;
            }

            // Token: 0x17000045 RID: 69
            // (get) Token: 0x0600016E RID: 366 RVA: 0x000086CF File Offset: 0x000068CF
            // (set) Token: 0x0600016F RID: 367 RVA: 0x000086D7 File Offset: 0x000068D7
            public UISpriteFlip flip
            {
                get => m_Flip;
                set => m_Flip = value;
            }

            // Token: 0x17000046 RID: 70
            // (get) Token: 0x06000170 RID: 368 RVA: 0x000086E0 File Offset: 0x000068E0
            // (set) Token: 0x06000171 RID: 369 RVA: 0x000086E8 File Offset: 0x000068E8
            public bool invertFill
            {
                get => m_InvertFill;
                set => m_InvertFill = value;
            }

            // Token: 0x17000047 RID: 71
            // (get) Token: 0x06000172 RID: 370 RVA: 0x000086F1 File Offset: 0x000068F1
            // (set) Token: 0x06000173 RID: 371 RVA: 0x000086F9 File Offset: 0x000068F9
            public UIFillDirection fillDirection
            {
                get => m_FillDirection;
                set => m_FillDirection = value;
            }

            // Token: 0x17000048 RID: 72
            // (get) Token: 0x06000174 RID: 372 RVA: 0x00008702 File Offset: 0x00006902
            // (set) Token: 0x06000175 RID: 373 RVA: 0x0000870A File Offset: 0x0000690A
            public float fillAmount
            {
                get => m_FillAmount;
                set => m_FillAmount = value;
            }

            // Token: 0x17000049 RID: 73
            // (get) Token: 0x06000176 RID: 374 RVA: 0x00008713 File Offset: 0x00006913
            // (set) Token: 0x06000177 RID: 375 RVA: 0x0000871B File Offset: 0x0000691B
            public Vector3 offset
            {
                get => m_Offset;
                set => m_Offset = value;
            }

            // Token: 0x1700004A RID: 74
            // (get) Token: 0x06000178 RID: 376 RVA: 0x00008724 File Offset: 0x00006924
            // (set) Token: 0x06000179 RID: 377 RVA: 0x0000872C File Offset: 0x0000692C
            public int baseIndex
            {
                get => m_BaseIndex;
                set => m_BaseIndex = value;
            }

            // Token: 0x04000075 RID: 117
            private UITextureAtlas m_Atlas;

            // Token: 0x04000076 RID: 118
            private UITextureAtlas.SpriteInfo m_SpriteInfo;

            // Token: 0x04000077 RID: 119
            private Color32 m_Color;

            // Token: 0x04000078 RID: 120
            private float m_PixelsToUnits;

            // Token: 0x04000079 RID: 121
            private Vector2 m_Size;

            // Token: 0x0400007A RID: 122
            private UISpriteFlip m_Flip;

            // Token: 0x0400007B RID: 123
            private bool m_InvertFill;

            // Token: 0x0400007C RID: 124
            private UIFillDirection m_FillDirection;

            // Token: 0x0400007D RID: 125
            private float m_FillAmount;

            // Token: 0x0400007E RID: 126
            private Vector3 m_Offset;

            // Token: 0x0400007F RID: 127
            private int m_BaseIndex;
        }

        #endregion
    }
}