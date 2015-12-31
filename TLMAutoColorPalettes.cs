using ColossalFramework.Plugins;
using ColossalFramework.UI;
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
        private static Dictionary<string, AutoColorPalette> m_palettes;
        private static List<Color32> SaoPaulo2035 = new List<Color32>(new Color32[]{
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

        public static string defaultPaletteList
        {
            get
            {
                return new AutoColorPalette("São Paulo 2035", SaoPaulo2035).serialize();
            }
        }

        public static string[] paletteList
        {
            get
            {
                if (m_palettes == null)
                {
                    init();
                }
                return new string[] { PALETTE_RANDOM }.Union(m_palettes.Keys).OrderBy(x => x).ToArray();
            }
        }

        public static string[] paletteListForEditing
        {
            get
            {
                if (m_palettes == null)
                {
                    init();
                }
                return new string[] { "-Choose...-" }.Union(m_palettes.Keys).OrderBy(x => x).ToArray();
            }
        }

        private static void init()
        {

            m_palettes = new Dictionary<string, AutoColorPalette>();
            load();
        }

        private static void load()
        {
            string serializedInfo = TransportLinesManagerMod.savedPalettes.value;
            string[] items = serializedInfo.Split(SERIALIZER_ITEM_SEPARATOR);
            foreach (string item in items)
            {
                AutoColorPalette acp = AutoColorPalette.parseFromString(item);
                if (acp != null)
                {
                    m_palettes.Add(acp.name, acp);
                }
            }
            m_palettes["São Paulo 2035"] = new AutoColorPalette("São Paulo 2035", SaoPaulo2035);
        }

        public static void save()
        {
            List<string> vals = new List<string>();
            foreach (AutoColorPalette item in m_palettes.Values)
            {
                string val = item.serialize();
                vals.Add(val);
            }
            TransportLinesManagerMod.savedPalettes.value = string.Join(SERIALIZER_ITEM_SEPARATOR.ToString(), vals.ToArray());
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
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "[TLM Palette '" + name + "'] Corrupted serialized color: " + thisColor);
                    return null;
                }
                byte r, g, b;
                bool success = byte.TryParse(thisColorCompounds[0], out r);
                success &= byte.TryParse(thisColorCompounds[1], out g);
                success &= byte.TryParse(thisColorCompounds[2], out b);
                if (!success)
                {
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "[TLM Palette '" + name + "'] Corrupted serialized color: invalid number in " + thisColor);
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
            Color32 color = new Color32();

            // make the color fully opaque
            color.a = 255;
            color.r = colorBytes[0];
            color.g = colorBytes[1];
            color.b = colorBytes[2];
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, color.ToString());

            return color;
        }
    }

}

