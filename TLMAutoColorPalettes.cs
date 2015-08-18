using UnityEngine;
using System.Collections;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Reflection;
using ColossalFramework.Plugins;
using System.IO;
using ColossalFramework.Threading;
using System.Runtime.CompilerServices;
using ColossalFramework.Math;
using ColossalFramework.Globalization;

namespace TransportLinesManager 
{
	public class TLMAutoColorPalettes	{ 
		private static RandomPastelColorGenerator gen = new RandomPastelColorGenerator ();

		private static Color32[] SaoPaulo2035 = new Color32[]{
			new Color32(117,0,0,255),
			new Color32(0,13,160,255),
			new Color32(0,128,27,255),
			new Color32(250,0,0,255),
			new Color32(255,213,3,255),
			new Color32(165,67,153,255),
			new Color32(244,115,33,255),
			new Color32(159,24,102,255),
			new Color32(158,158,148,255),
			new Color32(0,168,142,255),
			new Color32(4,124,140,255),
			new Color32(240,78,35,255),
			new Color32(4,43,106,255),
			new Color32(0,172,92,255),
			new Color32(30,30,30,255),
			new Color32(180,178,177,255),
			new Color32(255,255,255,255),
			new Color32(245,158,55,255),
			new Color32(167,139,107,255),
			new Color32(0,149,218,255),
			new Color32(252,124,161,255),
			new Color32(95,44,143,255),
			new Color32(92,58,14,255),
			new Color32(0,0,0,255),
			new Color32(100,100,100,255),
			new Color32(202,187,168,255),
			new Color32(0,0,255,255),
			new Color32(208,45,255,255),
			new Color32(0,255,0,255),
			new Color32(255,252,186,255)
		};

		public enum Pallete {
			Random = 0,
			SaoPaulo2035 = 1
		}

		public static Color32 getColor(int number, TLMAutoColorPalettes.Pallete palette){
			bool randomOnPaletteOverflow = TLMController.instance.savedUseRandomColorOnPaletteOverflow.value;
			switch (palette) {
			case Pallete.SaoPaulo2035:
				if(!randomOnPaletteOverflow || number <= SaoPaulo2035.Length){
					return SaoPaulo2035[number % SaoPaulo2035.Length];
				}
				break;
			}
			return gen.GetNext();
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
			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, color.ToString());
			
			return color;
		}
	}

}

