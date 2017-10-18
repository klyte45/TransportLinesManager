using ColossalFramework.Globalization;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMAutoColorPalettes
    {
        public const string PALETTE_RANDOM = "<RANDOM>";
        public const char SERIALIZER_ITEM_SEPARATOR = '∞';
        private static RandomPastelColorGenerator gen = new RandomPastelColorGenerator();
        private static Dictionary<string, AutoColorPalette> m_palettes = null;
        public readonly static List<Color32> SaoPaulo2035 = new List<Color32>(new Color32[]{
            new Color32 (117, 0, 0, 255),
            new Color32 (0, 13, 160, 255),
            new Color32 (0, 128, 27, 255),
            new Color32 (250, 0, 0, 255),
            new Color32 (255, 213, 3, 255),
            new Color32 (165, 67, 153, 255),
            new Color32 (244, 115, 33, 255),
            new Color32 (159, 24, 102, 255),
            new Color32 (158, 158, 148, 255),
            new Color32 (0, 168, 142, 255),
            new Color32 (4, 124, 140, 255),
            new Color32 (240, 78, 35, 255),
            new Color32 (4, 43, 106, 255),
            new Color32 (0, 172, 92, 255),
            new Color32 (30, 30, 30, 255),
            new Color32 (180, 178, 177, 255),
            new Color32 (255, 255, 255, 255),
            new Color32 (245, 158, 55, 255),
            new Color32 (167, 139, 107, 255),
            new Color32 (0, 149, 218, 255),
            new Color32 (252, 124, 161, 255),
            new Color32 (95, 44, 143, 255),
            new Color32 (92, 58, 14, 255),
            new Color32 (0, 0, 0, 255),
            new Color32 (100, 100, 100, 255),
            new Color32 (202, 187, 168, 255),
            new Color32 (0, 0, 255, 255),
            new Color32 (208, 45, 255, 255),
            new Color32 (0, 255, 0, 255),
            new Color32 (255, 252, 186, 255)
        });
        public readonly static List<Color32> London2016 = new List<Color32>(new Color32[]{
            new Color32 (137,78,36,255),
            new Color32 (220,36,31,255),
            new Color32 (225,206,0,255),
            new Color32 (0,114,41,255),
            new Color32 (215,153,175,255),
            new Color32 (134,143,152,255),
            new Color32 (117,16,86,255),
            new Color32 (0,0,0,255),
            new Color32 (0,25,168,255),
            new Color32 (0,160,226,255),
            new Color32 (118,208,189,255),
            new Color32 (102,204,0,255),
            new Color32 (232,106,16,255)
        });
        public readonly static List<Color32> Rainbow = new List<Color32>(new Color32[]{
            new Color32 (   25,12 ,243 ,255),
            new Color32 (   36,12 ,243 ,255),
            new Color32 (   56,73 ,245 ,255),
            new Color32 (   85,156,246 ,255),
            new Color32 (  111,233,179 ,255),
            new Color32 (   93,201, 97 ,255),
            new Color32 (   80,170, 40 ,255),
            new Color32 (   81,164, 25 ,255),
            new Color32 (  115,195, 29 ,255),
            new Color32 (  152,220, 31 ,255),
            new Color32 (  249,254, 41 ,255),
            new Color32 (  233,222, 36 ,255),
            new Color32 (  227,194, 33 ,255),
            new Color32 (  219,161, 32 ,255),
            new Color32 (  202, 96, 26 ,255),
            new Color32 (  192, 49, 24 ,255),
            new Color32 (  189,  3, 23 ,255),
            new Color32 (  133,  0, 28 ,255),
            new Color32 (   73,  1, 63 ,255),
            new Color32 (   44,  4, 94 ,255)
        });

        public readonly static List<Color32> RainbowShort = new List<Color32>(new Color32[]{
            new Color32 ( 160,  0,200  ,255),
            new Color32 ( 130,  0,220  ,255),
            new Color32 (  30, 60,255  ,255),
            new Color32 (   0,160,255  ,255),
            new Color32 (   0,200,200  ,255),
            new Color32 (   0,210,140  ,255),
            new Color32 (   0,220,  0  ,255),
            new Color32 ( 160,230, 50  ,255),
            new Color32 ( 230,220, 50  ,255),
            new Color32 ( 230,175, 45  ,255),
            new Color32 ( 240,130, 40  ,255),
            new Color32 ( 250, 60, 60  ,255),
            new Color32 ( 240,  0,130  ,255)
        });


        public readonly static List<Color32> WorldMix = new List<Color32>(new Color32[]{
            new Color32 (0, 0, 0        ,255),
            new Color32 (230, 25, 75     ,255),
            new Color32 (60, 180, 75     ,255),
            new Color32 (255, 225, 25    ,255),
            new Color32 (0, 130, 200     ,255),
            new Color32 (245, 130, 48    ,255),
            new Color32 (145, 30, 180    ,255),
            new Color32 (70, 240, 240    ,255),
            new Color32 (240, 50, 230    ,255),
            new Color32 (210, 245, 60    ,255),
            new Color32 (250, 190, 190 ,255),
            new Color32 (0, 128, 128     ,255),
            new Color32 (230, 190, 255 ,255),
            new Color32 (170, 110, 40    ,255),
            new Color32 (255, 250, 200 ,255),
            new Color32 (128, 0, 0 ,255),
            new Color32 (170, 255, 195 ,255),
            new Color32 (128, 128, 0     ,255),
            new Color32 (255, 215, 180 ,255),
            new Color32 (0, 0, 128      ,255),
            new Color32 (128, 128, 128 ,255),
            new Color32 (255, 255, 255 ,255)
        });

        public readonly static List<Color32> MSMetroUI = new List<Color32>(new Color32[]{
            new Color32   (51,153,51,255),
            new Color32  (162, 0, 255, 255),
            new Color32  (27, 161, 226, 255),
            new Color32  (140, 191, 38, 255),
            new Color32  (229, 20, 0, 255),
            new Color32  (255, 0, 151, 255),
            new Color32  (230, 113, 184, 255),
            new Color32  (160, 80, 0, 255),
            new Color32  (0, 171, 169, 255),
            new Color32  (240, 150, 9, 255),
        });

        public readonly static List<Color32> MatColor100 = new List<Color32>(new Color32[]{
    new Color32(0xcf,0xd8,0xdc,255),        new Color32(0xff,0xcd,0xd2,255),    new Color32(0xf8,0xbb,0xd0,255),    new Color32(0xe1,0xbe,0xe7,255),    new Color32(0xd1,0xc4,0xe9,255),    new Color32(0xc5,0xca,0xe9,255),    new Color32(0xbb,0xde,0xfb,255),    new Color32(0xb3,0xe5,0xfc,255),    new Color32(0xb2,0xeb,0xf2,255),    new Color32(0xb2,0xdf,0xdb,255),    new Color32(0xc8,0xe6,0xc9,255),    new Color32(0xdc,0xed,0xc8,255),    new Color32(0xf0,0xf4,0xc3,255),    new Color32(0xff,0xf9,0xc4,255),    new Color32(0xff,0xec,0xb3,255),    new Color32(0xff,0xe0,0xb2,255),    new Color32(0xff,0xcc,0xbc,255),    new Color32(0xd7,0xcc,0xc8,255),    new Color32(0xf5,0xf5,0xf5,255),    new Color32(0xcf,0xd8,0xdc,255),
      });
        public readonly static List<Color32> MatColor500 = new List<Color32>(new Color32[]{
    new Color32(0x60,0x7d,0x8b,255),         new Color32(0xf4,0x43,0x36,255),   new Color32(0xe9,0x1e,0x63,255),    new Color32(0x9c,0x27,0xb0,255),    new Color32(0x67,0x3a,0xb7,255),    new Color32(0x3f,0x51,0xb5,255),    new Color32(0x21,0x96,0xf3,255),    new Color32(0x03,0xa9,0xf4,255),    new Color32(0x00,0xbc,0xd4,255),    new Color32(0x00,0x96,0x88,255),    new Color32(0x4c,0xaf,0x50,255),    new Color32(0x8b,0xc3,0x4a,255),    new Color32(0xcd,0xdc,0x39,255),    new Color32(0xff,0xeb,0x3b,255),    new Color32(0xff,0xc1,0x07,255),    new Color32(0xff,0x98,0x00,255),    new Color32(0xff,0x57,0x22,255),    new Color32(0x79,0x55,0x48,255),    new Color32(0x9e,0x9e,0x9e,255),    new Color32(0x60,0x7d,0x8b,255),
      });
        public readonly static List<Color32> MatColor900 = new List<Color32>(new Color32[]{
    new Color32(0x26,0x32,0x38,255),         new Color32(0xb7,0x1c,0x1c,255),   new Color32(0x88,0x0e,0x4f,255),    new Color32(0x4a,0x14,0x8c,255),    new Color32(0x31,0x1b,0x92,255),    new Color32(0x1a,0x23,0x7e,255),    new Color32(0x0d,0x47,0xa1,255),    new Color32(0x01,0x57,0x9b,255),    new Color32(0x00,0x60,0x64,255),    new Color32(0x00,0x4d,0x40,255),    new Color32(0x1b,0x5e,0x20,255),    new Color32(0x33,0x69,0x1e,255),    new Color32(0x82,0x77,0x17,255),    new Color32(0xf5,0x7f,0x17,255),    new Color32(0xff,0x6f,0x00,255),    new Color32(0xe6,0x51,0x00,255),    new Color32(0xbf,0x36,0x0c,255),    new Color32(0x3e,0x27,0x23,255),    new Color32(0x21,0x21,0x21,255),    new Color32(0x26,0x32,0x38,255),
        });
        public readonly static List<Color32> MatColorA200 = new List<Color32>(new Color32[]{
   new Color32(0xff,0x6e,0x40,255),            new Color32(0xff,0x52,0x52,255),  new Color32(0xff,0x40,0x81,255),    new Color32(0xe0,0x40,0xfb,255),    new Color32(0x7c,0x4d,0xff,255),    new Color32(0x53,0x6d,0xfe,255),    new Color32(0x44,0x8a,0xff,255),    new Color32(0x40,0xc4,0xff,255),    new Color32(0x18,0xff,0xff,255),    new Color32(0x64,0xff,0xda,255),    new Color32(0x69,0xf0,0xae,255),    new Color32(0xb2,0xff,0x59,255),    new Color32(0xee,0xff,0x41,255),    new Color32(0xff,0xff,0x00,255),    new Color32(0xff,0xd7,0x40,255),    new Color32(0xff,0xab,0x40,255),   
       });
       public readonly static List<Color32> MatColorA400 = new List<Color32>(new Color32[]{
   new Color32(0xff,0x3d,0x00,255),           new Color32(0xff,0x17,0x44,255),   new Color32(0xf5,0x00,0x57,255),    new Color32(0xd5,0x00,0xf9,255),    new Color32(0x65,0x1f,0xff,255),    new Color32(0x3d,0x5a,0xfe,255),    new Color32(0x29,0x79,0xff,255),    new Color32(0x00,0xb0,0xff,255),    new Color32(0x00,0xe5,0xff,255),    new Color32(0x1d,0xe9,0xb6,255),    new Color32(0x00,0xe6,0x76,255),    new Color32(0x76,0xff,0x03,255),    new Color32(0xc6,0xff,0x00,255),    new Color32(0xff,0xea,0x00,255),    new Color32(0xff,0xc4,0x00,255),    new Color32(0xff,0x91,0x00,255),   
       });
       public readonly static List<Color32> MatColorA700 = new List<Color32>(new Color32[]{
   new Color32(0xdd,0x2c,0x00,255),            new Color32(0xd5,0x00,0x00,255),  new Color32(0xc5,0x11,0x62,255),    new Color32(0xaa,0x00,0xff,255),    new Color32(0x62,0x00,0xea,255),    new Color32(0x30,0x4f,0xfe,255),    new Color32(0x29,0x62,0xff,255),    new Color32(0x00,0x91,0xea,255),    new Color32(0x00,0xb8,0xd4,255),    new Color32(0x00,0xbf,0xa5,255),    new Color32(0x00,0xc8,0x53,255),    new Color32(0x64,0xdd,0x17,255),    new Color32(0xae,0xea,0x00,255),    new Color32(0xff,0xd6,0x00,255),    new Color32(0xff,0xab,0x00,255),    new Color32(0xff,0x6d,0x00,255),   
        });

        public readonly static List<Color32> CPTM_SP_2000 = new List<Color32>(new Color32[]{
            new Color32  (93, 47, 145, 255),
            new Color32   (124,97,78,255),
            new Color32  (150, 154, 153, 255),
            new Color32  (77, 140, 211, 255),
            new Color32  (212, 184, 136, 255),
            new Color32  (222, 142, 5, 255),
            new Color32  (67, 39, 123, 255),
            new Color32  (154, 54, 124, 255),
            new Color32  (3, 170, 87, 255),
            new Color32  (14, 14, 14, 255),
        });

        public readonly static List<Color32> SP_BUS_2000 = new List<Color32>(new Color32[]{
            new Color32  (200, 200, 200, 255),
            new Color32   (5,225,31,255),
            new Color32  (0, 77, 133, 255),
            new Color32  (255, 245, 0, 255),
            new Color32  (218, 37, 28, 255),
            new Color32  (0, 115, 100, 255),
            new Color32  (0, 114, 184, 255),
            new Color32  (159, 44, 41, 255),
            new Color32  (229, 119, 24, 255),
        });

        public readonly static AutoColorPalette[] defaultPaletteArray = new AutoColorPalette[] {
                    new AutoColorPalette("São Paulo 2035", SaoPaulo2035),
                    new AutoColorPalette("London 2016", London2016),
                    new AutoColorPalette("Rainbow", Rainbow),
                    new AutoColorPalette("Rainbow Short", RainbowShort),
                    new AutoColorPalette("World Metro Mix", WorldMix),
                    new AutoColorPalette("MS Metro UI", MSMetroUI),
                    new AutoColorPalette("Material Color (100)", MatColor100),
                    new AutoColorPalette("Material Color (500)", MatColor500),
                    new AutoColorPalette("Material Color (900)", MatColor900),
                    new AutoColorPalette("Material Color (A200)", MatColorA200),
                    new AutoColorPalette("Material Color (A400)", MatColorA400),
                    new AutoColorPalette("Material Color (A700)", MatColorA700),
                    new AutoColorPalette("São Paulo CPTM 2000", CPTM_SP_2000),
                    new AutoColorPalette("São Paulo Bus Area 2000", SP_BUS_2000),
                };

        public static string defaultPaletteList
        {
            get
            {
                return ToString(defaultPaletteArray);
            }
        }

        public static string[] paletteList
        {
            get
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("TLMAutoColorPalettes paletteList");
                if (m_palettes == null)
                {
                    init();
                }
                return new string[] { "<" + Locale.Get("TLM_RANDOM") + ">" }.Union(m_palettes.Keys).OrderBy(x => x).ToArray();
            }
        }

        public static string[] paletteListForEditing
        {
            get
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("TLMAutoColorPalettes paletteListForEditing");
                if (m_palettes == null)
                {
                    init();
                }
                return new string[] { "-" + Locale.Get("SELECT") + "-" }.Union(m_palettes.Keys.OrderBy(x => x)).ToArray();
            }
        }

        private static void init()
        {
            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("TLMAutoColorPalettes init()");
            m_palettes = new Dictionary<string, AutoColorPalette>();
            load();
        }

        private static void load()
        {
            string serializedInfo = TransportLinesManagerMod.savedPalettes.value;

            if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading palettes - separator: {1} ; save Value: {0}", serializedInfo, SERIALIZER_ITEM_SEPARATOR);
            string[] items = serializedInfo.Split(SERIALIZER_ITEM_SEPARATOR);
            foreach (string item in items)
            {
                if (TransportLinesManagerMod.instance != null && TransportLinesManagerMod.debugMode) TLMUtils.doLog("Loading palette {0}", items);
                AutoColorPalette acp = AutoColorPalette.parseFromString(item);
                if (acp != null)
                {
                    m_palettes.Add(acp.name, acp);
                }
            }
            foreach (AutoColorPalette p in defaultPaletteArray)
            {
                m_palettes[p.name] = p;
            }

        }

        public static string ToString(IEnumerable<AutoColorPalette> list)
        {
            List<string> vals = new List<string>();
            foreach (AutoColorPalette item in list)
            {
                string val = item.serialize();
                vals.Add(val);
            }
            return string.Join(SERIALIZER_ITEM_SEPARATOR.ToString(), vals.ToArray());
        }

        public static void save()
        {
            TransportLinesManagerMod.savedPalettes.value = ToString(m_palettes.Values);
        }

        public static Color32 getColor(int number, string paletteName, bool randomOnPaletteOverflow)
        {
            if (m_palettes.ContainsKey(paletteName))
            {
                AutoColorPalette palette = m_palettes[paletteName];
                if (!randomOnPaletteOverflow || number <= palette.colors.Count)
                {
                    return palette[number % palette.Count];
                }
            }
            return gen.GetNext();
        }

        public static void setColor(int number, string paletteName, Color newColor)
        {
            if (m_palettes.ContainsKey(paletteName))
            {
                AutoColorPalette palette = m_palettes[paletteName];
                if (number <= palette.Count && number > 0)
                {
                    palette[number % palette.Count] = newColor;
                    save();
                }
            }
        }

        public static List<Color32> getColors(string paletteName)
        {
            if (m_palettes.ContainsKey(paletteName))
            {
                AutoColorPalette palette = m_palettes[paletteName];
                var saida = palette.colors.GetRange(1, palette.colors.Count - 1).ToList();
                saida.Add(palette[0]);
                return saida;
            }
            return null;
        }

        public static string renamePalette(string oldName, string newName)
        {
            if (m_palettes.ContainsKey(oldName) && !m_palettes.ContainsKey(newName))
            {
                m_palettes[newName] = m_palettes[oldName];
                m_palettes.Remove(oldName);
                m_palettes[newName].name = newName;
                save();
                return newName;
            }
            else
                return oldName;
        }

        public static void addColor(string paletteName)
        {
            if (m_palettes.ContainsKey(paletteName))
            {
                m_palettes[paletteName].Add();
                save();
            }
        }

        public static void removeColor(string paletteName, int index)
        {
            if (m_palettes.ContainsKey(paletteName) && m_palettes[paletteName].Count > 1)
            {
                m_palettes[paletteName].RemoveColor(index);
                save();
            }
        }

        public static string addPalette()
        {

            int id = 0;
            string name = "New Palette";
            if (m_palettes.ContainsKey(name))
            {
                while (m_palettes.ContainsKey(name + id))
                {
                    id++;
                }
                name = name + id;
            }

            m_palettes[name] = new AutoColorPalette(name, new Color32[] { Color.white }.ToList());
            save();
            return name;
        }

        public static void removePalette(string paletteName)
        {
            m_palettes.Remove(paletteName);
            save();
        }

    }

    public class AutoColorPalette
    {
        public const char SERIALIZER_SEPARATOR = '∂';
        public const char COLOR_COMP_SEPARATOR = '∫';
        private List<Color32> m_colors;

        public string name
        {
            get;
            set;
        }

        public int Count
        {
            get
            {
                return m_colors.Count;
            }
        }

        public List<Color32> colors
        {
            get
            {
                return m_colors;
            }
        }

        public void Add()
        {
            m_colors.Add(m_colors[0]);
            m_colors[0] = Color.white;
        }

        public void RemoveColor(int index)
        {
            if (Count > 1)
            {
                if (index % m_colors.Count == 0)
                {
                    m_colors[0] = m_colors[m_colors.Count - 1];
                    m_colors.RemoveAt(m_colors.Count - 1);
                }
                else {
                    m_colors.RemoveAt(index % m_colors.Count);
                }
            }
        }

        public AutoColorPalette(string name, IEnumerable<Color32> colors)
        {
            this.name = name;
            this.m_colors = new List<Color32>(colors);
        }

        public Color32 this[int key]
        {
            get
            {
                return m_colors[key];
            }
            set
            {
                m_colors[key] = value;
            }
        }

        public string serialize()
        {
            StringBuilder result = new StringBuilder();
            result.Append(name);
            foreach (Color32 color in m_colors)
            {
                result.Append(SERIALIZER_SEPARATOR);
                result.Append(color.r);
                result.Append(COLOR_COMP_SEPARATOR);
                result.Append(color.g);
                result.Append(COLOR_COMP_SEPARATOR);
                result.Append(color.b);
            }
            return result.ToString();
        }

        public static AutoColorPalette parseFromString(string data)
        {
            string[] parts = data.Split(SERIALIZER_SEPARATOR);
            if (parts.Length <= 1)
            {
                return null;
            }
            string name = parts[0];
            List<Color32> colors = new List<Color32>(parts.Length - 1);
            for (int i = 1; i < parts.Length; i++)
            {
                string thisColor = parts[i];
                string[] thisColorCompounds = thisColor.Split(COLOR_COMP_SEPARATOR);
                if (thisColorCompounds.Length != 3)
                {
                    TLMUtils.doErrorLog("[TLM Palette '" + name + "'] Corrupted serialized color: " + thisColor);
                    return null;
                }

                bool success = byte.TryParse(thisColorCompounds[0], out byte r);
                success &= byte.TryParse(thisColorCompounds[1], out byte g);
                success &= byte.TryParse(thisColorCompounds[2], out byte b);
                if (!success)
                {
                    TLMUtils.doErrorLog("[TLM Palette '" + name + "'] Corrupted serialized color: invalid number in " + thisColor);
                    return null;
                }
                colors.Add(new Color32(r, g, b, 255));
            }
            return new AutoColorPalette(name, colors);
        }

    }

    public class RandomPastelColorGenerator
    {
        private readonly System.Random _random;

        public RandomPastelColorGenerator()
        {
            // seed the generator with 2 because
            // this gives a good sequence of colors
            const int RandomSeed = 2;
            _random = new System.Random(RandomSeed);
        }


        /// <summary>
        /// Returns a random pastel color
        /// </summary>
        /// <returns></returns>
        public Color32 GetNext()
        {
            // to create lighter colours:
            // take a random integer between 0 & 128 (rather than between 0 and 255)
            // and then add 64 to make the colour lighter
            byte[] colorBytes = new byte[3];
            colorBytes[0] = (byte)(_random.Next(128) + 64);
            colorBytes[1] = (byte)(_random.Next(128) + 64);
            colorBytes[2] = (byte)(_random.Next(128) + 64);
            Color32 color = new Color32
            {

                // make the color fully opaque
                a = 255,
                r = colorBytes[0],
                g = colorBytes[1],
                b = colorBytes[2]
            };
            TLMUtils.doLog(color.ToString());

            return color;
        }
    }

}

