using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMAutoColorPaletteContainer
    {
        public const string PALETTE_RANDOM = "<RANDOM>";
        public const char SERIALIZER_ITEM_SEPARATOR = '∞';
        private static RandomPastelColorGenerator gen = new RandomPastelColorGenerator();
        private static Dictionary<string, TLMAutoColorPalette> m_palettes = null;
        public static readonly List<Color32> SaoPaulo2035 = new List<Color32>(new Color32[]{
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
        public static readonly List<Color32> London2016 = new List<Color32>(new Color32[]{
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
        public static readonly List<Color32> Rainbow = new List<Color32>(new Color32[]{
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

        public static readonly List<Color32> RainbowShort = new List<Color32>(new Color32[]{
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


        public static readonly List<Color32> WorldMix = new List<Color32>(new Color32[]{
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

        public static readonly List<Color32> MSMetroUI = new List<Color32>(new Color32[]{
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

        public static readonly List<Color32> MatColor100 = new List<Color32>(new Color32[]{
    new Color32(0xcf,0xd8,0xdc,255),        new Color32(0xff,0xcd,0xd2,255),    new Color32(0xf8,0xbb,0xd0,255),    new Color32(0xe1,0xbe,0xe7,255),    new Color32(0xd1,0xc4,0xe9,255),    new Color32(0xc5,0xca,0xe9,255),    new Color32(0xbb,0xde,0xfb,255),    new Color32(0xb3,0xe5,0xfc,255),    new Color32(0xb2,0xeb,0xf2,255),    new Color32(0xb2,0xdf,0xdb,255),    new Color32(0xc8,0xe6,0xc9,255),    new Color32(0xdc,0xed,0xc8,255),    new Color32(0xf0,0xf4,0xc3,255),    new Color32(0xff,0xf9,0xc4,255),    new Color32(0xff,0xec,0xb3,255),    new Color32(0xff,0xe0,0xb2,255),    new Color32(0xff,0xcc,0xbc,255),    new Color32(0xd7,0xcc,0xc8,255),    new Color32(0xf5,0xf5,0xf5,255),    new Color32(0xcf,0xd8,0xdc,255),
      });
        public static readonly List<Color32> MatColor500 = new List<Color32>(new Color32[]{
    new Color32(0x60,0x7d,0x8b,255),         new Color32(0xf4,0x43,0x36,255),   new Color32(0xe9,0x1e,0x63,255),    new Color32(0x9c,0x27,0xb0,255),    new Color32(0x67,0x3a,0xb7,255),    new Color32(0x3f,0x51,0xb5,255),    new Color32(0x21,0x96,0xf3,255),    new Color32(0x03,0xa9,0xf4,255),    new Color32(0x00,0xbc,0xd4,255),    new Color32(0x00,0x96,0x88,255),    new Color32(0x4c,0xaf,0x50,255),    new Color32(0x8b,0xc3,0x4a,255),    new Color32(0xcd,0xdc,0x39,255),    new Color32(0xff,0xeb,0x3b,255),    new Color32(0xff,0xc1,0x07,255),    new Color32(0xff,0x98,0x00,255),    new Color32(0xff,0x57,0x22,255),    new Color32(0x79,0x55,0x48,255),    new Color32(0x9e,0x9e,0x9e,255),    new Color32(0x60,0x7d,0x8b,255),
      });
        public static readonly List<Color32> MatColor900 = new List<Color32>(new Color32[]{
    new Color32(0x26,0x32,0x38,255),         new Color32(0xb7,0x1c,0x1c,255),   new Color32(0x88,0x0e,0x4f,255),    new Color32(0x4a,0x14,0x8c,255),    new Color32(0x31,0x1b,0x92,255),    new Color32(0x1a,0x23,0x7e,255),    new Color32(0x0d,0x47,0xa1,255),    new Color32(0x01,0x57,0x9b,255),    new Color32(0x00,0x60,0x64,255),    new Color32(0x00,0x4d,0x40,255),    new Color32(0x1b,0x5e,0x20,255),    new Color32(0x33,0x69,0x1e,255),    new Color32(0x82,0x77,0x17,255),    new Color32(0xf5,0x7f,0x17,255),    new Color32(0xff,0x6f,0x00,255),    new Color32(0xe6,0x51,0x00,255),    new Color32(0xbf,0x36,0x0c,255),    new Color32(0x3e,0x27,0x23,255),    new Color32(0x21,0x21,0x21,255),    new Color32(0x26,0x32,0x38,255),
        });
        public static readonly List<Color32> MatColorA200 = new List<Color32>(new Color32[]{
   new Color32(0xff,0x6e,0x40,255),            new Color32(0xff,0x52,0x52,255),  new Color32(0xff,0x40,0x81,255),    new Color32(0xe0,0x40,0xfb,255),    new Color32(0x7c,0x4d,0xff,255),    new Color32(0x53,0x6d,0xfe,255),    new Color32(0x44,0x8a,0xff,255),    new Color32(0x40,0xc4,0xff,255),    new Color32(0x18,0xff,0xff,255),    new Color32(0x64,0xff,0xda,255),    new Color32(0x69,0xf0,0xae,255),    new Color32(0xb2,0xff,0x59,255),    new Color32(0xee,0xff,0x41,255),    new Color32(0xff,0xff,0x00,255),    new Color32(0xff,0xd7,0x40,255),    new Color32(0xff,0xab,0x40,255),
       });
        public static readonly List<Color32> MatColorA400 = new List<Color32>(new Color32[]{
   new Color32(0xff,0x3d,0x00,255),           new Color32(0xff,0x17,0x44,255),   new Color32(0xf5,0x00,0x57,255),    new Color32(0xd5,0x00,0xf9,255),    new Color32(0x65,0x1f,0xff,255),    new Color32(0x3d,0x5a,0xfe,255),    new Color32(0x29,0x79,0xff,255),    new Color32(0x00,0xb0,0xff,255),    new Color32(0x00,0xe5,0xff,255),    new Color32(0x1d,0xe9,0xb6,255),    new Color32(0x00,0xe6,0x76,255),    new Color32(0x76,0xff,0x03,255),    new Color32(0xc6,0xff,0x00,255),    new Color32(0xff,0xea,0x00,255),    new Color32(0xff,0xc4,0x00,255),    new Color32(0xff,0x91,0x00,255),
       });
        public static readonly List<Color32> MatColorA700 = new List<Color32>(new Color32[]{
   new Color32(0xdd,0x2c,0x00,255),            new Color32(0xd5,0x00,0x00,255),  new Color32(0xc5,0x11,0x62,255),    new Color32(0xaa,0x00,0xff,255),    new Color32(0x62,0x00,0xea,255),    new Color32(0x30,0x4f,0xfe,255),    new Color32(0x29,0x62,0xff,255),    new Color32(0x00,0x91,0xea,255),    new Color32(0x00,0xb8,0xd4,255),    new Color32(0x00,0xbf,0xa5,255),    new Color32(0x00,0xc8,0x53,255),    new Color32(0x64,0xdd,0x17,255),    new Color32(0xae,0xea,0x00,255),    new Color32(0xff,0xd6,0x00,255),    new Color32(0xff,0xab,0x00,255),    new Color32(0xff,0x6d,0x00,255),
        });

        public static readonly List<Color32> CPTM_SP_2000 = new List<Color32>(new Color32[]{
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

        public static readonly List<Color32> SP_BUS_2000 = new List<Color32>(new Color32[]{
            new Color32  (200, 200, 200, 255),
            new Color32  (5,225,31,255),
            new Color32  (0, 77, 133, 255),
            new Color32  (255, 245, 0, 255),
            new Color32  (218, 37, 28, 255),
            new Color32  (0, 115, 100, 255),
            new Color32  (0, 114, 184, 255),
            new Color32  (159, 44, 41, 255),
            new Color32  (229, 119, 24, 255),
        });

        public static readonly TLMAutoColorPalette[] defaultPaletteArray = new TLMAutoColorPalette[] {
                    new TLMAutoColorPalette("São Paulo 2035", SaoPaulo2035),
                    new TLMAutoColorPalette("London 2016", London2016),
                    new TLMAutoColorPalette("Rainbow", Rainbow),
                    new TLMAutoColorPalette("Rainbow Short", RainbowShort),
                    new TLMAutoColorPalette("World Metro Mix", WorldMix),
                    new TLMAutoColorPalette("MS Metro UI", MSMetroUI),
                    new TLMAutoColorPalette("Material Color (100)", MatColor100),
                    new TLMAutoColorPalette("Material Color (500)", MatColor500),
                    new TLMAutoColorPalette("Material Color (900)", MatColor900),
                    new TLMAutoColorPalette("Material Color (A200)", MatColorA200),
                    new TLMAutoColorPalette("Material Color (A400)", MatColorA400),
                    new TLMAutoColorPalette("Material Color (A700)", MatColorA700),
                    new TLMAutoColorPalette("São Paulo CPTM 2000", CPTM_SP_2000),
                    new TLMAutoColorPalette("São Paulo Bus Area 2000", SP_BUS_2000),
                };

        public static string[] PaletteList
        {
            get
            {
                LogUtils.DoLog("TLMAutoColorPalettes paletteList");
                if (m_palettes == null)
                {
                    Init();
                }
                return new string[] { "<" + Locale.Get("K45_TLM_RANDOM") + ">" }.Union(m_palettes.Keys).OrderBy(x => x).ToArray();
            }
        }

        public static string[] PaletteListForEditing
        {
            get
            {
                LogUtils.DoLog("TLMAutoColorPalettes paletteListForEditing");
                if (m_palettes == null)
                {
                    Init();
                }
                return new string[] { "-" + Locale.Get("SELECT") + "-" }.Union(m_palettes.Keys.OrderBy(x => x)).ToArray();
            }
        }

        private static void Init()
        {
            LogUtils.DoLog("TLMAutoColorPalettes init()");
            Reload();
        }

        public static void Reload()
        {
            m_palettes = new Dictionary<string, TLMAutoColorPalette>();
            Load();
        }

        private static Dictionary<string, string> GetPalettesAsDictionary()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var pal in m_palettes)
            {
                if (!result.ContainsKey(pal.Key))
                {
                    result[pal.Key] = pal.Value.ToFileContent();
                }
            }
            return result;
        }

        public static void SaveAll()
        {
            FileUtils.EnsureFolderCreation(TLMController.PalettesFolder);
            var filesToSave = GetPalettesAsDictionary();
            foreach (var file in filesToSave)
            {
                File.WriteAllText(TLMController.PalettesFolder + Path.DirectorySeparatorChar + file.Key + TLMAutoColorPalette.EXT_PALETTE, file.Value);
            }
        }

        public static void Save(string palette)
        {
            if (!palette.IsNullOrWhiteSpace() && m_palettes.ContainsKey(palette))
            {
                m_palettes[palette].Save();
            }
        }

        private static void Load()
        {
            m_palettes = new Dictionary<string, TLMAutoColorPalette>();
            FileUtils.EnsureFolderCreation(TLMController.PalettesFolder);
            foreach (var filename in Directory.GetFiles(TLMController.PalettesFolder, "*" + TLMAutoColorPalette.EXT_PALETTE).Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string fileContents = File.ReadAllText(TLMController.PalettesFolder + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                var name = filename.Substring(0, filename.Length - 4);
                m_palettes[name] = TLMAutoColorPalette.FromFileContent(name, fileContents.Split(TLMAutoColorPalette.ENTRY_SEPARATOR).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray());
                LogUtils.DoLog("LOADED PALETTE ({0}) QTT: {1}", filename, m_palettes[name].Count);
            }
        }


        public static Color32 GetColor(int number, string[] paletteOrderSearch, bool randomOnPaletteOverflow, bool avoidRandom = false)
        {
            foreach (var paletteName in paletteOrderSearch)
            {
                if (!paletteName.IsNullOrWhiteSpace() && m_palettes.ContainsKey(paletteName))
                {
                    TLMAutoColorPalette palette = m_palettes[paletteName];
                    if (!randomOnPaletteOverflow || number <= palette.Colors.Count)
                    {
                        return palette[number % palette.Count];
                    }
                }
            }
            return avoidRandom ? (Color32)Color.clear : gen.GetNext();
        }

        public static List<Color32> GetColors(string paletteName)
        {
            if (!paletteName.IsNullOrWhiteSpace() && m_palettes.ContainsKey(paletteName))
            {
                TLMAutoColorPalette palette = m_palettes[paletteName];
                return palette.Colors;
            }
            return null;
        }

        public static TLMAutoColorPalette GetPalette(string paletteName)
        {
            if (!paletteName.IsNullOrWhiteSpace() && m_palettes.ContainsKey(paletteName))
            {
                TLMAutoColorPalette palette = m_palettes[paletteName];
                return palette;
            }
            return null;
        }

        public static void AddPalette(string paletteName)
        {
            if (!paletteName.IsNullOrWhiteSpace() && !m_palettes.ContainsKey(paletteName))
            {
                m_palettes[paletteName] = new TLMAutoColorPalette(paletteName, new List<Color32> { Color.white });
            }
        }

    }

}

