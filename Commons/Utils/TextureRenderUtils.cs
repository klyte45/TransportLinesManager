using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Redirectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class TextureRenderUtils
    {
        public static Texture2D RenderSpriteLineToTexture(UIDynamicFont font, UITextureAtlas atlas, string spriteName, Color bgColor, string text) => RenderSpriteLine(font, atlas, spriteName, bgColor, text);
        public static Texture2D RenderTextToTexture(UIDynamicFont font, string text, Color textColor, out Vector2 textDimensions, Color outlineColor = default)
        {
            float textScale = 2f;

            textDimensions = MeasureTextWidth(font, text, textScale, out Vector2 yBounds);


            var tex = new Texture2D((int) textDimensions.x, (int) textDimensions.y, TextureFormat.ARGB32, false);
            tex.SetPixels(new Color[(int) (textDimensions.x * textDimensions.y)]);

            RenderText(font, text, new Vector3(0, -yBounds.x), textScale, textColor, outlineColor, tex);

            return RescalePowerOf2(tex);

        }

        private static Texture2D RescalePowerOf2(Texture2D tex)
        {
            var imageSize = new Vector2(Mathf.Max(Mathf.NextPowerOfTwo(tex.width), 1), Mathf.Max(Mathf.NextPowerOfTwo(tex.height), 1));

            TextureScaler.scale(tex, (int) imageSize.x, (int) imageSize.y);
            return tex;
        }

        public static Texture2D RenderSpriteLine(UIDynamicFont font, UITextureAtlas atlas, string spriteName, Color bgColor, string text, float textScale = 1)
        {

            UITextureAtlas.SpriteInfo spriteInfo = atlas[spriteName];
            if (spriteInfo == null)
            {
                CODebugBase<InternalLogChannel>.Warn(InternalLogChannel.UI, "Missing sprite " + spriteName + " in " + atlas.name);
                return null;
            }
            else
            {
                textScale *= 2;

                Texture2D texture = atlas.texture;
                float calcHeight = font.size * textScale * 2;
                float calcProportion = spriteInfo.region.width * texture.width / (spriteInfo.region.height * texture.height);
                float calcWidth = Mathf.CeilToInt(calcHeight * calcProportion);

                int height = Mathf.CeilToInt(calcHeight);
                int width = Mathf.CeilToInt(calcWidth);

                float textureScale = height / (spriteInfo.region.height * texture.height);

                LogUtils.DoLog($"height = {height} - width = {width} -  renderer.pixelRatio = 1 - textureScale = {height} / {(spriteInfo.region.height * texture.height)}");

                var size = new Vector3(width, height);
                float borderWidth = textScale * 3;

                Vector2 textDimensions = MeasureTextWidth(font, text, textScale, out Vector2 yBounds);
                float borderBottom = Mathf.Max(0, (spriteInfo.border.bottom * textScale * 2) + Mathf.Min(0, yBounds.x));
                var textAreaSize = new Vector4((spriteInfo.border.left * textScale * 2) + borderWidth, borderBottom + borderWidth, width - (spriteInfo.border.horizontal * textScale * 2) - borderWidth, height - (spriteInfo.border.top * textScale * 2) - borderBottom - borderWidth);

                float multipler = Mathf.Min(Mathf.Min(3.5f, textAreaSize.z / textDimensions.x), Mathf.Min(3.5f, textAreaSize.w / textDimensions.y));
                if (multipler > 1)
                {
                    textScale *= 1 + ((multipler - 1) / 2.1f);
                    multipler = 1;
                    textDimensions = MeasureTextWidth(font, text, textScale, out yBounds);
                }

                var imageSize = new Vector2(Mathf.NextPowerOfTwo((int) Mathf.Max(textDimensions.x * multipler, width)), Mathf.NextPowerOfTwo((int) Mathf.Max(textDimensions.y, height)));


                var tex = new Texture2D((int) imageSize.x, (int) imageSize.y, TextureFormat.ARGB32, false);
                tex.SetPixels(new Color[(int) (imageSize.x * imageSize.y)]);


                var texText = new Texture2D((int) textDimensions.x, (int) textDimensions.y, TextureFormat.ARGB32, false);
                texText.SetPixels(new Color[(int) (textDimensions.x * textDimensions.y)]);

                Color contrastColor = KlyteMonoUtils.ContrastColor(bgColor);

                Vector2 position = RenderSprite(atlas, spriteName, contrastColor, tex, textureScale);
                RenderSprite(atlas, spriteName, bgColor, tex, null, tex.height - (int) (borderWidth * 2), null, new Vector2((textScale / 2) - 0.5f, (textScale / 2) - 0.5f), (a, b) =>
                          {
                              if (b.a == 1)
                              {
                                  return b;
                              }

                              if (b.a == 0)
                              {
                                  return a;
                              }

                              float totalAlpha = a.a + b.a;
                              return (a * (1 - b.a)) + (b * b.a);

                          });
                Vector2 posText = position + VectorUtils.XY(textAreaSize) + new Vector2((textAreaSize.z / 2) - (textDimensions.x * multipler / 2) + 1, (textAreaSize.w / 2) - (textDimensions.y / 2) - (yBounds.x / 2));

                RenderText(font, text, new Vector3(0, -yBounds.x), textScale, contrastColor, bgColor, texText);

                if (multipler < 1)
                {
                    TextureScaler.scale(texText, (int) (texText.width * multipler), texText.height);
                }
                MergeTextures(tex, texText.GetPixels(), (int) posText.x, (int) posText.y, texText.width, texText.height, false);
                UnityEngine.Object.Destroy(texText);
                tex.Apply();

                return tex;
            }
        }
        private static Vector2 MeasureTextWidth(UIDynamicFont font, string text, float textScale, out Vector2 yBounds)
        {
            float width = 1f;
            int size = Mathf.CeilToInt(font.size * textScale);
            font.RequestCharacters(text, size, FontStyle.Normal);
            yBounds = new Vector2(9999999f, -999999999f);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                font.baseFont.GetCharacterInfo(c, out CharacterInfo characterInfo, size, FontStyle.Normal);
                if (c == '\t')
                {
                    width += 3f * characterSpacing;
                }
                else
                {
                    width += ((c != ' ') ? characterInfo.maxX : (characterInfo.advance + (characterSpacing * textScale)));
                    yBounds.x = Mathf.Min(yBounds.x, characterInfo.minY);
                    yBounds.y = Mathf.Max(yBounds.y, characterInfo.maxY);
                }
            }
            if (text.Length > 2)
            {
                width += (text.Length - 2) * characterSpacing * textScale;
            }
            return new Vector2(width + 6, yBounds.y - yBounds.x + 6);
        }

        private static ColorInfo ParseColor(UIMarkupToken token, Color defaultColor)
        {
            var result = new ColorInfo(Color.white, true);
            if (token.attributeCount == 1)
            {
                string value = token.GetAttribute(0).m_Value.value;
                result.color = UIMarkupStyle.ParseColor(value, defaultColor);
                result.overrideColor = true;
            }
            return result;
        }


        public static Texture2D RenderTokenizedText(UIDynamicFont uidynamicFont, float textScale, string text, Color baseColor, out Vector2 textRealSize)
        {
            if (text.IsNullOrWhiteSpace())
            {
                textRealSize = Vector2.zero;
                return new Texture2D(1, 1);
            }
            var textColors = new Stack<ColorInfo>();
            textColors.Clear();
            textColors.Push(new ColorInfo(baseColor));
            var tokens = (PoolList<UIMarkupToken>) typeof(UIMarkupTokenizer).GetMethod("Tokenize", RedirectorUtils.allFlags).Invoke(null, new object[] { text });
            Vector2 texSize = CalculateTextureSize(uidynamicFont, textScale, ref tokens, out int startYPos);
            if (texSize.x <= 0 || texSize.y <= 0)
            {
                textRealSize = Vector2.zero;
                return new Texture2D(1, 1);
            }
            var position = new Vector3(0, startYPos);
            var result = new Texture2D((int) texSize.x, (int) texSize.y, TextureFormat.RGBA32, false);
            result.SetPixels(new Color[result.width * result.height]);
            RenderLine(tokens, uidynamicFont, textScale, textColors, position, result);
            tokens.Release();
            textRealSize = new Vector2(result.width, result.height);
            return RescalePowerOf2(result);
        }

        private static Vector2 CalculateTextureSize(UIDynamicFont font, float textScale, ref PoolList<UIMarkupToken> tokens, out int startYPos)
        {
            float xAdvance = 0;
            var yBounds = new Vector2(999999999999999999999999f, -99999999999999999999999999f);
            for (int i = 0; i < tokens.Count; i++)
            {
                UIMarkupToken token = tokens[i];
                if (token.tokenType == UIMarkupTokenType.Text)
                {
                    Vector2 textDimensions = MeasureTextWidth(font, token.value, textScale, out Vector2 yBoundsCalc);
                    xAdvance += textDimensions.x;
                    yBounds.x = Mathf.Min(yBounds.x, yBoundsCalc.x);
                    yBounds.y = Mathf.Max(yBounds.y, yBoundsCalc.y);
                }
                else if (token.tokenType == UIMarkupTokenType.Whitespace)
                {
                    int size2 = Mathf.CeilToInt(font.size * textScale);
                    float num2 = characterSpacing * textScale;
                    float num = 0;
                    font.RequestCharacters(" ", size2, FontStyle.Normal);
                    for (int j = 0; j < token.length; j++)
                    {
                        char c = token[j];
                        int multiplier = 1;
                        if (c == '\t')
                        {
                            multiplier = 4;
                        }
                        font.baseFont.GetCharacterInfo(' ', out CharacterInfo characterInfo, size2, FontStyle.Normal);
                        num += (characterInfo.advance + num2) * multiplier;
                    }
                    token.height = Mathf.CeilToInt(num);
                    xAdvance += token.height;
                    yBounds.x = Mathf.Min(yBounds.x, 0);
                    yBounds.y = Mathf.Max(yBounds.y, token.height);
                }
                else if (token.tokenType == UIMarkupTokenType.StartTag)
                {
                    if (UIDynamicFontRendererRedirector.Matches(token, "sprite"))
                    {
                        if (token.attributeCount != 1)
                        {
                            tokens.RemoveAt(i);
                            i--;
                            continue;
                        }
                        UITextureAtlas.SpriteInfo spriteInfo = UIView.GetAView().defaultAtlas[token.GetAttribute(0).m_Value.value];
                        if (spriteInfo == null)
                        {
                            tokens.RemoveAt(i);
                            i--;
                            continue;
                        }
                        float targetScale = font.baseline * textScale / spriteInfo.texture.height;

                        token.height = (int) (targetScale * spriteInfo.texture.height);
                        xAdvance += (int) (targetScale * spriteInfo.texture.width);
                        yBounds.x = Mathf.Min(yBounds.x, 0);
                        yBounds.y = Mathf.Max(yBounds.y, token.height);
                    }
                    else if (UIDynamicFontRendererRedirector.Matches(token, UIDynamicFontRendererRedirector.TAG_LINE))
                    {
                        if (token.attributeCount != 1)
                        {
                            tokens.RemoveAt(i);
                            i--;
                            continue;
                        }
                        string[] attrs = token.GetAttribute(0).m_Value.value.Split(',');
                        if (attrs.Length != 3 || !Regex.IsMatch(attrs[1], "^[0-9a-fA-F]{6}$"))
                        {
                            tokens.RemoveAt(i);
                            i--;
                            continue;
                        }
                        UITextureAtlas.SpriteInfo spriteInfo = UIView.GetAView().defaultAtlas[attrs[0]];
                        if (spriteInfo == null)
                        {
                            tokens.RemoveAt(i);
                            i--;
                            continue;
                        }
                        float baseScale = font.baseline * textScale / spriteInfo.texture.height;
                        float targetScale = baseScale * 2;

                        token.height = (int) (targetScale * spriteInfo.texture.height);
                        xAdvance += (int) (targetScale * spriteInfo.texture.width);
                        yBounds.x = Mathf.Min(yBounds.x, -token.height / 3);
                        yBounds.y = Mathf.Max(yBounds.y, token.height / 3 * 2);
                    }
                }
            }
            if (tokens.Count == 0)
            {
                startYPos = 0;
                return new Vector2(1, 1);
            }
            float ySize = yBounds.y - yBounds.x;
            startYPos = (int) (-yBounds.x);
            return new Vector2(xAdvance, ySize);
        }

        // Token: 0x06001451 RID: 5201 RVA: 0x00058EF8 File Offset: 0x000570F8
        private static void RenderLine(PoolList<UIMarkupToken> m_Tokens, UIDynamicFont uidynamicFont, float textScale, Stack<ColorInfo> colors, Vector3 position, Texture2D outputTexture)
        {
            for (int i = 0; i < m_Tokens.Count; i++)
            {
                UIMarkupToken uimarkupToken = m_Tokens[i];
                UIMarkupTokenType tokenType = uimarkupToken.tokenType;
                if (tokenType == UIMarkupTokenType.Text)
                {
                    ColorInfo colorInfo = colors.Peek();
                    position.x = RenderText(uidynamicFont, uimarkupToken.value, position, textScale, colorInfo.color, default, outputTexture);
                }
                else if (tokenType == UIMarkupTokenType.Whitespace)
                {
                    position.x += uimarkupToken.height;
                }
                else if (tokenType == UIMarkupTokenType.StartTag)
                {
                    if (UIDynamicFontRendererRedirector.Matches(uimarkupToken, "sprite"))
                    {
                        ColorInfo colorInfo2 = colors.Peek();
                        position.x += RenderSprite(UIView.GetAView().defaultAtlas, uimarkupToken.GetAttribute(0).m_Value.value, colorInfo2.color, outputTexture, null, uimarkupToken.height, position).z;
                    }
                    else if (UIDynamicFontRendererRedirector.Matches(uimarkupToken, "color"))
                    {
                        colors.Push(ParseColor(uimarkupToken, colors.First().color));
                    }
                    else if (UIDynamicFontRendererRedirector.Matches(uimarkupToken, UIDynamicFontRendererRedirector.TAG_LINE))
                    {
                        string[] args = uimarkupToken.GetAttribute(0)?.m_Value?.value?.Split(new char[] { ',' }, 3);
                        if (args == null || args.Length != 3)
                        {
                            LogUtils.DoErrorLog($"INVALID ARGUMENT: {uimarkupToken.GetAttribute(0)?.m_Value?.value ?? "<NULL>"}");
                            continue;
                        }
                        Texture2D spriteLineTex = RenderSpriteLine(uidynamicFont, UIView.GetAView().defaultAtlas, args[0], ColorExtensions.FromRGB(args[1]), args[2], textScale);
                        if (spriteLineTex.height > outputTexture.height)
                        {
                            float scale = ((float) outputTexture.height) / spriteLineTex.height;
                            TextureScaler.scale(spriteLineTex, Mathf.RoundToInt(spriteLineTex.width * scale), Mathf.RoundToInt(spriteLineTex.height * scale));
                        }
                        int targetY = (outputTexture.height - spriteLineTex.height) / 2;
                        outputTexture.SetPixels((int) position.x, targetY, spriteLineTex.width, spriteLineTex.height, spriteLineTex.GetPixels());
                        position.x += spriteLineTex.width;
                    }
                }
                else if (tokenType == UIMarkupTokenType.EndTag && UIDynamicFontRendererRedirector.Matches(uimarkupToken, "color") && colors.Count > 1)
                {
                    colors.Pop();
                }
            }
        }

        private static float RenderText(UIDynamicFont uidynamicFont, string text, Vector3 position, float textScale, Color textColor, Color outlineColor, Texture2D tex)
        {
            float size = (uidynamicFont.size * textScale);
            FontStyle style = FontStyle.Normal;
            float x = position.x;
            float y = position.y;
            Color color2 = textColor;
            Color c = color2;
            Texture2D readableTex = ((Texture2D) uidynamicFont.baseFont.material.mainTexture).MakeReadable();
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0)
                {
                    x += characterSpacing * textScale;
                }
                if (uidynamicFont.baseFont.GetCharacterInfo(text[i], out CharacterInfo glyph, Mathf.CeilToInt(size), style))
                {
                    if (text[i] == ' ')
                    {
                        x += (glyph.advance + (characterSpacing * textScale));
                        continue;
                    }
                    float num3 = (glyph.maxY);
                    float minX = x + glyph.minX;
                    float maxY = y + num3;
                    float minY = maxY - glyph.glyphHeight;
                    var vector4 = new Vector3(minX, minY);

                    float minU = Mathf.Min(glyph.uvTopLeft.x, glyph.uvTopRight.x, glyph.uvBottomRight.x, glyph.uvBottomLeft.x);
                    float maxU = Mathf.Max(glyph.uvTopLeft.x, glyph.uvTopRight.x, glyph.uvBottomRight.x, glyph.uvBottomLeft.x);
                    float minV = Mathf.Min(glyph.uvTopLeft.y, glyph.uvTopRight.y, glyph.uvBottomRight.y, glyph.uvBottomLeft.y);
                    float maxV = Mathf.Max(glyph.uvTopLeft.y, glyph.uvTopRight.y, glyph.uvBottomRight.y, glyph.uvBottomLeft.y);
                    int sizeU = (int) ((maxU - minU) * readableTex.width);
                    int sizeV = (int) ((maxV - minV) * readableTex.height);
                    Color[] colors = readableTex.GetPixels(Mathf.RoundToInt(minU * readableTex.width), Mathf.RoundToInt(minV * readableTex.height), sizeU, sizeV);
                    if (outlineColor != default)
                    {
                        for (int j = 0; j < kOutlineOffsets.Length; j++)
                        {
                            Vector3 b2 = kOutlineOffsets[j] * 3;
                            Vector3 targetOffset = vector4 + b2;
                            MergeTextures(tex, colors.Select(x => new Color(outlineColor.r, outlineColor.g, outlineColor.b, x.a)).ToArray(), Mathf.RoundToInt(targetOffset.x), Mathf.RoundToInt(targetOffset.y), glyph.glyphWidth, glyph.glyphHeight, glyph.flipped, !glyph.flipped, glyph.flipped, true);
                        }
                    }


                    MergeTextures(tex, colors.Select(x => new Color(textColor.r, textColor.g, textColor.b, x.a)).ToArray(), Mathf.RoundToInt(vector4.x), Mathf.RoundToInt(vector4.y), glyph.glyphWidth, glyph.glyphHeight, glyph.flipped, !glyph.flipped, glyph.flipped);
                    x += glyph.maxX;
                }
            }
            return x;
        }

        private static void MergeTextures(Texture2D tex, Color[] colors, int startX, int startY, int sizeX, int sizeY, bool swapXY = false, bool flipVertical = false, bool flipHorizontal = false, bool plain = false)
        {
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Color orPixel = tex.GetPixel(startX + i, startY + j);
                    Color newPixel = colors[((flipVertical ? sizeY - j - 1 : j) * (swapXY ? 1 : sizeX)) + ((flipHorizontal ? sizeX - i - 1 : i) * (swapXY ? sizeY : 1))];

                    if (plain && newPixel.a != 1)
                    {
                        continue;
                    }

                    tex.SetPixel(startX + i, startY + j, Color.Lerp(orPixel, newPixel, newPixel.a));
                }
            }
        }

        internal static Vector4 RenderSprite(UITextureAtlas atlas, string spriteName, Color color, Texture2D tex, float? targetScale, int? targetHeight = null, Vector2? position = null, Vector2 positionOffset = default, Func<Color, Color, Color> blendFunction = null)
        {
            UITextureAtlas.SpriteInfo spriteInfo = atlas[spriteName];
            if (targetScale == null)
            {
                if (targetHeight == null)
                {
                    LogUtils.DoErrorLog("Target scale or target height must be set to render sprite!");
                    return new Vector2();
                }
                targetScale = (float) targetHeight / spriteInfo.height;
            }
            Texture2D readableTexture = spriteInfo.texture.MakeReadable();
            TextureScaler.scale(readableTexture, (int) (readableTexture.width * targetScale), (int) (readableTexture.height * targetScale));
            int width = readableTexture.width;
            int height = readableTexture.height;
            Vector2 targetPosition = (position ?? new Vector2((tex.width - width) / 2, (tex.height - height) / 2)) + positionOffset;
            Color[] colors = readableTexture.GetPixels();
            if (blendFunction == null)
            {
                blendFunction = (x, y) => y;
            }

            tex.SetPixels((int) targetPosition.x, (int) targetPosition.y, width, height, colors.Select((x, y) => blendFunction(tex.GetPixel((int) targetPosition.x + (y % width), (int) targetPosition.y + (y / width)), x * color)).ToArray());
            return new Vector4(targetPosition.x, targetPosition.y, width, height);
        }

        private static Vector2[] kOutlineOffsets = new Vector2[]
            {
                new Vector2(-1f, -1f),
                new Vector2(-1f, 0),
                new Vector2(-1f, 1f),
                new Vector2(0, 1f),
                new Vector2(1f, -1f),
                new Vector2(1f, 0),
                new Vector2(1f, 1f),
                new Vector2(0, -1f)
            };
        private static float characterSpacing = 0;


        private struct ColorInfo
        {
            // Token: 0x06001442 RID: 5186 RVA: 0x000589EE File Offset: 0x00056BEE
            public ColorInfo(Color32 c)
            {
                color = c;
                overrideColor = false;
            }

            // Token: 0x06001443 RID: 5187 RVA: 0x000589FE File Offset: 0x00056BFE
            public ColorInfo(Color32 c, bool o)
            {
                color = c;
                overrideColor = o;
            }

            // Token: 0x04000881 RID: 2177
            public Color32 color;

            // Token: 0x04000882 RID: 2178
            public bool overrideColor;
        }
    }

}
