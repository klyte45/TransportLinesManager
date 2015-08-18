using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Reflection;
using ColossalFramework.Plugins;
using System.IO;
using ColossalFramework.Threading;
using System.Runtime.CompilerServices;
using ColossalFramework.Math;
using ColossalFramework.Globalization;

namespace TransportLinesManager
{
	public class TransportLinesManagerMod :  IUserMod, ILoadingExtension
	{

		public static string version = "2.3";
		public static TransportLinesManagerMod instance;
		public SavedBool savedAutoNaming;
		public SavedBool savedAutoColor;
		public SavedBool savedCircularOnSingleDistrict;
		public SavedBool savedUseRandomColorOnPaletteOverflow;
		public SavedInt savedNomenclaturaMetro;
		public SavedInt savedNomenclaturaOnibus;
		public SavedInt savedNomenclaturaTrem;
		public SavedInt savedAutoColorPaletteMetro;
		public SavedInt savedAutoColorPaletteTrem;
		public SavedInt savedAutoColorPaletteOnibus;

		public string Name { 
			get { 
				
				return "Transport Lines Manager " + version; 
			} 
		}
		
		public string Description { 
			get { return "A shortcut to manage all city's public transports lines."; } 
		} 

		public void OnCreated (ILoading loading)
		{
		}

		public TransportLinesManagerMod ()
		{			
			savedNomenclaturaMetro = new SavedInt ("NomenclaturaMetro", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			savedNomenclaturaTrem = new SavedInt ("NomenclaturaTrem", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			savedNomenclaturaOnibus = new SavedInt ("NomenclaturaOnibus", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			savedAutoNaming = new SavedBool ("AutoNameLines", Settings.gameSettingsFile, false, true);
			savedAutoColor = new SavedBool ("AutoColorLines", Settings.gameSettingsFile, false, true);
			savedUseRandomColorOnPaletteOverflow = new SavedBool ("AutoColorUseRandomColorOnPaletteOverflow", Settings.gameSettingsFile, false, true);
			savedCircularOnSingleDistrict = new SavedBool ("AutoNameCircularOnSingleDistrictLineNaming", Settings.gameSettingsFile, true, true);
			savedAutoColorPaletteMetro = new SavedInt ("AutoColorPaletteMetro", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			savedAutoColorPaletteTrem = new SavedInt ("AutoColorPaletteTrem", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			savedAutoColorPaletteOnibus = new SavedInt ("AutoColorPaletteOnibus", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			instance = this;
		}

		public void OnSettingsUI (UIHelperBase helper)
		{
			string[] namingOptions = new string[] {
				"Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic"
			};
			string[] palettes = new string[] {
				"Random","São Paulo 2035 (30)"
			};
			UIHelperBase group1 = helper.AddGroup ("Line Naming Strategy");			
			group1.AddCheckbox ("Auto naming enabled", savedAutoNaming.value, toggleAutoNaming);
			group1.AddCheckbox ("Use 'Circular' word on single district lines", savedCircularOnSingleDistrict.value, toggleCircularAutoName);
			group1.AddDropdown ("Bus Lines Identifier", namingOptions, savedNomenclaturaOnibus.value, setNamingBus);
			group1.AddDropdown ("Metro Lines Identifier", namingOptions, savedNomenclaturaMetro.value, setNamingMetro);
			group1.AddDropdown ("Train Lines Identifier", namingOptions, savedNomenclaturaTrem.value, setNamingTrain);
			UIHelperBase group2 = helper.AddGroup ("Line Coloring Strategy");
			group2.AddCheckbox ("Auto coloring enabled", savedAutoColor.value, toggleAutoColor);
			group2.AddCheckbox ("Random colors on palette overflow", savedUseRandomColorOnPaletteOverflow.value, toggleAutoColorRandomOveflow);
			group2.AddDropdown ("Bus Lines Palette", palettes, savedAutoColorPaletteOnibus.value, setAutoColorBus);
			group2.AddDropdown ("Metro Lines Palette", palettes, savedAutoColorPaletteMetro.value, setAutoColorMetro);
			group2.AddDropdown ("Train Lines Palette", palettes, savedAutoColorPaletteTrem.value, setAutoColorTrain);
		}
		
		public void OnLevelLoaded (LoadMode mode)
		{

			if (TLMController.instance == null) {
				TLMController.instance = new TLMController ();
			}
			
			if (TLMController.taTLM == null) {
				TLMController.taTLM = CreateTextureAtlas ("sprites.png", "TransportLinesManagerSprites", GameObject.FindObjectOfType<UIView> ().FindUIComponent<UIPanel> ("InfoPanel").atlas.material, 32, 32, new string[] {
					"TransportLinesManagerIcon","TransportLinesManagerIconHovered"
				});
			}
			if (TLMController.taLineNumber == null) {
				TLMController.taLineNumber = CreateTextureAtlas ("lineFormat.png", "TransportLinesManagerLinearLineSprites", GameObject.FindObjectOfType<UIView> ().FindUIComponent<UIPanel> ("InfoPanel").atlas.material, 64, 64, new string[] {
					"SubwayIcon","TrainIcon","BusIcon"
				});

			}
//			Log.debug ("LEVELLOAD");
		}
		
		public void OnLevelUnloading ()
		{
			if (TLMController.instance != null) {
				TLMController.instance.destroy ();
			}
//			Log.debug ("LEVELUNLOAD");
		}

		public void OnReleased ()
		{

		}

		UITextureAtlas CreateTextureAtlas (string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
		{
			Texture2D tex = new Texture2D (spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false);
			tex.filterMode = FilterMode.Bilinear;
			{ // LoadTexture
				tex.LoadImage (ResourceLoader.loadResourceData (textureFile));
				tex.Apply (true, true);
			}
			UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas> ();
			{ // Setup atlas
				Material material = (Material)Material.Instantiate (baseMaterial);
				material.mainTexture = tex;
				atlas.material = material;
				atlas.name = atlasName;
			}
			// Add sprites
			for (int i = 0; i < spriteNames.Length; ++i) {
				float uw = 1.0f / spriteNames.Length;
				var spriteInfo = new UITextureAtlas.SpriteInfo () {
					name = spriteNames[i],
					texture = tex,
					region = new Rect(i * uw, 0, uw, 1),
				};
				atlas.AddSprite (spriteInfo);
			}
			return atlas;
		}

		///Metodos de seleçao
		private void setNamingBus (int idx)
		{
			savedNomenclaturaOnibus.value = idx;
		}

		private void setNamingMetro (int idx)
		{
			savedNomenclaturaMetro.value = idx;
		}

		private void setNamingTrain (int idx)
		{
			savedNomenclaturaTrem.value = idx;
		}
		
		private void setAutoColorBus (int idx)
		{
			savedAutoColorPaletteOnibus.value = idx;
		}

		private void setAutoColorMetro (int idx)
		{
			savedAutoColorPaletteMetro.value = idx;
		}

		private void setAutoColorTrain (int idx)
		{
			savedAutoColorPaletteTrem.value = idx;
		}

		private void toggleAutoColor (bool b)
		{
			savedAutoColor.value = b;
		}
		private void toggleAutoColorRandomOveflow (bool b)
		{
			savedUseRandomColorOnPaletteOverflow.value = b;
		}

		private void toggleCircularAutoName (bool b)
		{
			savedCircularOnSingleDistrict.value = b;
		}
		private void toggleAutoNaming (bool b)
		{
			savedAutoNaming.value = b;
		}

		
	}
	
	public class TransportLinesManagerThreadMod : ThreadingExtensionBase
	{					
		public override void OnCreated (IThreading threading)
		{			
			if (TLMController.instance != null) {
				TLMController.instance.destroy ();
			}
		}
		
		public override void OnUpdate (float realTimeDelta, float simulationTimeDelta)
		{
			if (TLMController.instance != null) {
				TLMController.instance.init ();
			} 
		}	 
	}

	public class UIButtonLineInfo : UIButton
	{
		public ushort lineID;
	}

	public class UIRadialChartAge: UIRadialChart
	{
		public void AddSlice (Color32 innerColor, Color32 outterColor)
		{
			SliceSettings slice = new UIRadialChart.SliceSettings ();
			slice.outterColor = outterColor;
			slice.innerColor = innerColor;
			this.m_Slices.Add (slice);
			this.Invalidate ();
		}
	}

}
