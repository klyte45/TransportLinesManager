using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMAutoColorPalette
    {
        public const char ENTRY_SEPARATOR = '\n';
        public const string EXT_PALETTE = ".txt";

        public string Name { get; set; }

        public int Count => Colors.Count;

        public List<Color32> Colors { get; }

        public void Add() => Colors.Add(Color.white);

        public void RemoveColor(int index)
        {
            if (Count > 1)
            {
                if (index % Colors.Count == 0)
                {
                    Colors[0] = Colors[Colors.Count - 1];
                    Colors.RemoveAt(Colors.Count - 1);
                }
                else
                {
                    Colors.RemoveAt(index % Colors.Count);
                }
            }
        }

        public TLMAutoColorPalette(string name, IEnumerable<Color32> colors)
        {
            Name = name;
            Colors = new List<Color32>(colors);
        }

        public Color32 this[int key]
        {
            get => Colors[key];
            set
            {
                if ((Color)value == default)
                {
                    Colors.RemoveAt(key);
                }
                else
                {
                    Colors[key] = value;
                }
            }
        }

        public string ToFileContent() => string.Join(ENTRY_SEPARATOR.ToString(), Colors.Select(x => x.ToRGB()).ToArray());

        public static TLMAutoColorPalette FromFileContent(string name, string[] fileContentLines)
        {
            var colors = fileContentLines.Select(x => ColorExtensions.FromRGB(x));
            return new TLMAutoColorPalette(name, colors);
        }

        public void Save() => File.WriteAllText(Path.Combine(TLMController.PalettesFolder, $"{Name}{EXT_PALETTE}"), ToFileContent());

    }

}

