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
using Klyte.Extensions;

namespace Klyte.TransportLinesManager
{
	public class TransportLinesManagerMod :  IUserMod, ILoadingExtension
	{

		public static string version = "3.0";
		public static TransportLinesManagerMod instance;
		private SavedBool m_savedAutoNaming;
		private SavedBool m_savedAutoColor;
		private SavedBool m_savedCircularOnSingleDistrict;
		private SavedBool m_savedUseRandomColorOnPaletteOverflow;
		private SavedBool m_savedAutoColorBasedOnPrefix;
		private SavedBool m_savedOverrideDefaultLineInfoPanel;
		private SavedInt m_savedNomenclaturaMetro;
		private SavedInt m_savedNomenclaturaOnibus;
		private SavedInt m_savedNomenclaturaTrem;
		private SavedInt m_savedNomenclaturaMetroSeparador;
		private SavedInt m_savedNomenclaturaOnibusSeparador;
		private SavedInt m_savedNomenclaturaTremSeparador;
		private SavedInt m_savedNomenclaturaMetroPrefixo;
		private SavedInt m_savedNomenclaturaOnibusPrefixo;
		private SavedInt m_savedNomenclaturaTremPrefixo;
		private SavedBool m_savedNomenclaturaMetroZeros;
		private SavedBool m_savedNomenclaturaOnibusZeros;
		private SavedBool m_savedNomenclaturaTremZeros;
		private SavedString m_savedAutoColorPaletteMetro;
		private SavedString m_savedAutoColorPaletteTrem;
		private SavedString m_savedAutoColorPaletteOnibus;
		private SavedBool m_savedShowMetroLinesOnLinearMap;
		private SavedBool m_savedShowBusLinesOnLinearMap;
		private SavedBool m_savedShowTrainLinesOnLinearMap;
		private SavedBool m_savedShowAirportsOnLinearMap;
		private SavedBool m_savedShowPassengerPortsOnLinearMap;
		private SavedBool m_savedShowTaxiStopsOnLinearMap;
		private SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel;
		private SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
		private SavedString m_savedPalettes;

		public static SavedBool savedAutoNaming {
			get {
				return TransportLinesManagerMod.instance.m_savedAutoNaming;
			}
		}
		
		public static SavedBool savedAutoColor {
			get {
				return TransportLinesManagerMod.instance.m_savedAutoColor;
			}
		}
		
		public static SavedBool savedCircularOnSingleDistrict {
			get {
				return TransportLinesManagerMod.instance.m_savedCircularOnSingleDistrict;
			}
		}
		
		public static SavedBool savedUseRandomColorOnPaletteOverflow {
			get {
				return TransportLinesManagerMod.instance.m_savedUseRandomColorOnPaletteOverflow;
			}
		}
		public static SavedBool savedOverrideDefaultLineInfoPanel {
			get {
				return TransportLinesManagerMod.instance.m_savedOverrideDefaultLineInfoPanel;
			}
		}
		public static SavedBool savedAutoColorBasedOnPrefix {
			get {
				return TransportLinesManagerMod.instance.m_savedAutoColorBasedOnPrefix;
			}
		}
		
		public static SavedBool savedShowMetroLinesOnLinearMap {
			get {
				return TransportLinesManagerMod.instance.m_savedShowMetroLinesOnLinearMap;
			}
		}
		
		public static SavedBool savedShowBusLinesOnLinearMap {
			get {
				return TransportLinesManagerMod.instance.m_savedShowBusLinesOnLinearMap;
			}
		}
		
		public static SavedBool savedShowTrainLinesOnLinearMap {
			get {
				return TransportLinesManagerMod.instance.m_savedShowTrainLinesOnLinearMap;
			}
		}
		
		public static SavedBool savedShowAirportsOnLinearMap {
			get {
				return TransportLinesManagerMod.instance.m_savedShowAirportsOnLinearMap;
			}
		}

		public static SavedBool savedShowPassengerPortsOnLinearMap {
			get {
				return TransportLinesManagerMod.instance.m_savedShowPassengerPortsOnLinearMap;
			}
		}

		public static SavedBool savedShowNearLinesInCityServicesWorldInfoPanel {
			get {
				return TransportLinesManagerMod.instance.m_savedShowNearLinesInCityServicesWorldInfoPanel;
			}
		}
		public static SavedBool savedShowNearLinesInZonedBuildingWorldInfoPanel {
			get {
				return TransportLinesManagerMod.instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel;
			}
		}

		public static SavedBool savedShowTaxiStopsOnLinearMap {
			get {
				return TransportLinesManagerMod.instance.m_savedShowTaxiStopsOnLinearMap;
			}
		}
		
		public static SavedInt savedNomenclaturaMetro {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaMetro;
			}
		}
		
		public static SavedInt savedNomenclaturaOnibus {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaOnibus;
			}
		}
		
		public static SavedInt savedNomenclaturaTrem {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaTrem;
			}
		}
		public static SavedInt savedNomenclaturaMetroSeparador {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaMetroSeparador;
			}
		}
		
		public static SavedInt savedNomenclaturaOnibusSeparador {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaOnibusSeparador;
			}
		}
		
		public static SavedInt savedNomenclaturaTremSeparador {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaTremSeparador;
			}
		}
		
		public static SavedInt savedNomenclaturaMetroPrefixo {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaMetroPrefixo;
			}
		}
		
		public static SavedInt savedNomenclaturaOnibusPrefixo {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaOnibusPrefixo;
			}
		}
		
		public static SavedInt savedNomenclaturaTremPrefixo {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaTremPrefixo;
			}
		}

		
		public static SavedBool savedNomenclaturaMetroZeros {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaMetroZeros;
			}
		}
		
		public static SavedBool savedNomenclaturaOnibusZeros {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaOnibusZeros;
			}
		}
		
		public static SavedBool savedNomenclaturaTremZeros {
			get {
				return TransportLinesManagerMod.instance.m_savedNomenclaturaTremZeros;
			}
		}

		public static SavedString savedAutoColorPaletteMetro {
			get {
				return TransportLinesManagerMod.instance.m_savedAutoColorPaletteMetro;
			}
		}
		
		public static SavedString savedAutoColorPaletteTrem {
			get {
				return TransportLinesManagerMod.instance.m_savedAutoColorPaletteTrem;
			}
		}
		
		public static SavedString savedAutoColorPaletteOnibus {
			get {
				return TransportLinesManagerMod.instance.m_savedAutoColorPaletteOnibus;
			}
		}

		public static SavedString savedPalettes {
			get {
				return TransportLinesManagerMod.instance.m_savedPalettes;
			}
		}

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
			m_savedPalettes = new SavedString ("savedPalettesTLM", Settings.gameSettingsFile, TLMAutoColorPalettes.defaultPaletteList, true);
			m_savedNomenclaturaMetro = new SavedInt ("NomenclaturaMetro", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			m_savedNomenclaturaTrem = new SavedInt ("NomenclaturaTrem", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			m_savedNomenclaturaOnibus = new SavedInt ("NomenclaturaOnibus", Settings.gameSettingsFile, (int)ModoNomenclatura.Numero, true);
			m_savedNomenclaturaMetroSeparador = new SavedInt ("NomenclaturaMetroSeparador", Settings.gameSettingsFile, (int)Separador.Nenhum, true);
			m_savedNomenclaturaTremSeparador = new SavedInt ("NomenclaturaTremSeparador", Settings.gameSettingsFile, (int)Separador.Nenhum, true);
			m_savedNomenclaturaOnibusSeparador = new SavedInt ("NomenclaturaOnibusSeparador", Settings.gameSettingsFile, (int)Separador.Nenhum, true);
			m_savedNomenclaturaMetroPrefixo = new SavedInt ("NomenclaturaMetroPrefixo", Settings.gameSettingsFile, (int)ModoNomenclatura.Nenhum, true);
			m_savedNomenclaturaTremPrefixo = new SavedInt ("NomenclaturaTremPrefixo", Settings.gameSettingsFile, (int)ModoNomenclatura.Nenhum, true);
			m_savedNomenclaturaOnibusPrefixo = new SavedInt ("NomenclaturaOnibusPrefixo", Settings.gameSettingsFile, (int)ModoNomenclatura.Nenhum, true);
			m_savedNomenclaturaOnibusZeros = new SavedBool ("NomenclaturaOnibusZeros", Settings.gameSettingsFile, true, true);
			m_savedNomenclaturaMetroZeros = new SavedBool ("NomenclaturaMetroZeros", Settings.gameSettingsFile, true, true);
			m_savedNomenclaturaTremZeros = new SavedBool ("NomenclaturaTremZeros", Settings.gameSettingsFile, true, true);
			m_savedAutoNaming = new SavedBool ("AutoNameLines", Settings.gameSettingsFile, false, true);
			m_savedAutoColor = new SavedBool ("AutoColorLines", Settings.gameSettingsFile, false, true);
			m_savedOverrideDefaultLineInfoPanel = new SavedBool ("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
			m_savedUseRandomColorOnPaletteOverflow = new SavedBool ("AutoColorUseRandomColorOnPaletteOverflow", Settings.gameSettingsFile, false, true);
			m_savedAutoColorBasedOnPrefix = new SavedBool ("AutoColorBasedOnPrefix", Settings.gameSettingsFile, false, true);
			m_savedCircularOnSingleDistrict = new SavedBool ("AutoNameCircularOnSingleDistrictLineNaming", Settings.gameSettingsFile, true, true);
			m_savedAutoColorPaletteMetro = new SavedString ("AutoColorPaletteMetro", Settings.gameSettingsFile, TLMAutoColorPalettes.PALETTE_RANDOM, true);
			m_savedAutoColorPaletteTrem = new SavedString ("AutoColorPaletteTrem", Settings.gameSettingsFile, TLMAutoColorPalettes.PALETTE_RANDOM, true);
			m_savedAutoColorPaletteOnibus = new SavedString ("AutoColorPaletteOnibus", Settings.gameSettingsFile, TLMAutoColorPalettes.PALETTE_RANDOM, true);
			m_savedShowMetroLinesOnLinearMap = new SavedBool ("showMetroLinesOnLinearMap", Settings.gameSettingsFile, true, true);
			m_savedShowBusLinesOnLinearMap = new SavedBool ("showBusLinesOnLinearMap", Settings.gameSettingsFile, false, true);
			m_savedShowTrainLinesOnLinearMap = new SavedBool ("showTrainLinesOnLinearMap", Settings.gameSettingsFile, true, true);
			m_savedShowTaxiStopsOnLinearMap = new SavedBool ("showTaxiStopsOnLinearMap", Settings.gameSettingsFile, false, true);
			m_savedShowAirportsOnLinearMap = new SavedBool ("showAirportsOnLinearMap", Settings.gameSettingsFile, true, true);
			m_savedShowPassengerPortsOnLinearMap = new SavedBool ("showPassengerPortsOnLinearMap", Settings.gameSettingsFile, true, true);
			m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool ("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
			m_savedShowNearLinesInZonedBuildingWorldInfoPanel=new SavedBool ("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
			instance = this;
		}

		private UIDropDown busPalette ;
		private UIDropDown metroPalette ;
		private UIDropDown trainPalette ;
		private UIDropDown editorSelector ;

		public void OnSettingsUI (UIHelperBase helperDefault)
		{
			string[] namingOptions = new string[] {
				"Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic"
			};
			string[] namingOptionsPrefixo = new string[] {
				"Number","Lower Latin","Upper Latin","Lower Greek", "Upper Greek", "Lower Cyrilic", "Upper Cyrilic", "None"
			};
			string[] namingOptionsSeparador = new string[] {
				"<None>","-",".","/", "<Blank Space>","<New Line>"
			};
			UIHelperExtension helper = new UIHelperExtension ((UIHelper)helperDefault);
			helper.AddCheckbox ("Override default line info panel",m_savedOverrideDefaultLineInfoPanel.value,toggleOverrideDefaultLineInfoPanel);
			UIHelperExtension group1 = helper.AddGroupExtended ("Line Naming Strategy");			
			((UIPanel)group1.self).autoLayoutDirection = LayoutDirection.Horizontal;		
			((UIPanel)group1.self).wrapLayout = true;

			group1.AddCheckbox ("Auto naming enabled", m_savedAutoNaming.value, toggleAutoNaming);
			group1.AddCheckbox ("Use 'Circular' word on single district lines", m_savedCircularOnSingleDistrict.value, toggleCircularAutoName);
			group1.AddDropdown ("Bus Lines Prefix", namingOptionsPrefixo, m_savedNomenclaturaOnibusPrefixo.value, setNamingBusPrefixo);
			group1.AddDropdown ("Bus Lines Separator", namingOptionsSeparador, m_savedNomenclaturaOnibusSeparador.value, setNamingBusSeparador);
			group1.AddDropdown ("Bus Lines Identifier", namingOptions, m_savedNomenclaturaOnibus.value, setNamingBus);
			group1.AddCheckbox ("Leading zeros for bus lines (when prefix is used)",m_savedNomenclaturaOnibusZeros.value,toggleOverrideSavedNomenclaturaOnibusZeros);
			group1.AddSpace (20);
			group1.AddDropdown ("Metro Lines Prefix", namingOptionsPrefixo, m_savedNomenclaturaMetroPrefixo.value, setNamingMetroPrefixo);
			group1.AddDropdown ("Metro Lines Separator", namingOptionsSeparador, m_savedNomenclaturaMetroSeparador.value, setNamingMetroSeparador);
			group1.AddDropdown ("Metro Lines Identifier", namingOptions, m_savedNomenclaturaMetro.value, setNamingMetro);
			group1.AddCheckbox ("Leading zeros for metro lines (when prefix is used)",m_savedNomenclaturaMetroZeros.value,toggleOverrideSavedNomenclaturaMetroZeros);
			group1.AddSpace (20);
			group1.AddDropdown ("Train Lines Prefix", namingOptionsPrefixo, m_savedNomenclaturaTremPrefixo.value, setNamingTrainPrefixo);
			group1.AddDropdown ("Train Lines Separator", namingOptionsSeparador, m_savedNomenclaturaTremSeparador.value, setNamingTrainSeparador);
			group1.AddDropdown ("Train Lines Identifier", namingOptions, m_savedNomenclaturaTrem.value, setNamingTrain);
			group1.AddCheckbox ("Leading zeros for train lines (when prefix is used)",m_savedNomenclaturaTremZeros.value,toggleOverrideSavedNomenclaturaTremZeros);

			UIHelperExtension group2 = helper.AddGroupExtended ("Line Coloring Strategy");
			group2.AddCheckbox ("Auto coloring enabled", m_savedAutoColor.value, toggleAutoColor);
			group2.AddCheckbox ("Random colors on palette overflow", m_savedUseRandomColorOnPaletteOverflow.value, toggleAutoColorRandomOveflow);
			group2.AddCheckbox ("Auto color based on prefix", m_savedAutoColorBasedOnPrefix.value, toggleAutoColorBasedOnPrefix);
			busPalette = group2.AddDropdown ("Bus Lines Palette", TLMAutoColorPalettes.paletteList, m_savedAutoColorPaletteOnibus.value, setAutoColorBus) as UIDropDown;
			metroPalette = group2.AddDropdown ("Metro Lines Palette", TLMAutoColorPalettes.paletteList, m_savedAutoColorPaletteMetro.value, setAutoColorMetro) as UIDropDown;
			trainPalette = group2.AddDropdown ("Train Lines Palette", TLMAutoColorPalettes.paletteList, m_savedAutoColorPaletteTrem.value, setAutoColorTrain) as UIDropDown;
			UIHelperExtension group4 = helper.AddGroupExtended ("Custom palettes config [" + UIHelperExtension.version + "]");
			((group4.self) as UIPanel).autoLayoutDirection = LayoutDirection.Horizontal;
			((group4.self) as UIPanel).wrapLayout = true;

			UITextField paletteName = null;
			DropDownColorSelector colorEditor = null;
			NumberedColorList colorList = null;

			editorSelector = group4.AddDropdown ("Palette Select", TLMAutoColorPalettes.paletteListForEditing, 0, delegate (int sel) {
				if (sel <= 0 || sel >= TLMAutoColorPalettes.paletteListForEditing.Length) {
					paletteName.enabled = false;
					colorEditor.Disable ();
					colorList.Disable ();
				} else {
					paletteName.enabled = true;
					colorEditor.Disable ();
					colorList.colorList = TLMAutoColorPalettes.getColors (TLMAutoColorPalettes.paletteListForEditing [sel]);
					colorList.Enable ();
					paletteName.text = TLMAutoColorPalettes.paletteListForEditing [sel];
				}
			}) as UIDropDown;
			
			group4.AddButton ("Create", delegate() {
				string newName = TLMAutoColorPalettes.addPalette ();
				updateDropDowns ("", "");
				editorSelector.selectedValue = newName;
			});
			group4.AddButton ("Delete", delegate() {
				TLMAutoColorPalettes.removePalette (editorSelector.selectedValue);
				updateDropDowns ("", "");
			});
			paletteName = group4.AddTextField ("Palette Name", delegate(string val) {
				
			}, "", (string value) => {
				string oldName = editorSelector.selectedValue;
				paletteName.text = TLMAutoColorPalettes.renamePalette (oldName, value);
				updateDropDowns (oldName, value);
			});
			paletteName.parent.width = 500;

			colorEditor = group4.AddColorField ("Colors", Color.black, delegate (Color c) {
				TLMAutoColorPalettes.setColor (colorEditor.id, editorSelector.selectedValue, c);				
				colorList.colorList = TLMAutoColorPalettes.getColors (editorSelector.selectedValue);
			}, delegate {
				TLMAutoColorPalettes.removeColor (editorSelector.selectedValue, colorEditor.id);		
				colorList.colorList = TLMAutoColorPalettes.getColors (editorSelector.selectedValue);
			});

			colorList = group4.AddNumberedColorList (null, new List<Color32> (), delegate (int c) {
				colorEditor.id = c;
				colorEditor.selectedColor = TLMAutoColorPalettes.getColor (c, editorSelector.selectedValue);
				colorEditor.title = c.ToString ();
				colorEditor.Enable ();
			}, colorEditor.parent.GetComponentInChildren<UILabel> (), delegate () {
				TLMAutoColorPalettes.addColor (editorSelector.selectedValue);
			});

			paletteName.enabled = false;
			colorEditor.Disable ();
			colorList.Disable ();

			UIHelperExtension group3 = helper.AddGroupExtended ("Linear map line intersections");
			group3.AddCheckbox ("Show metro lines", m_savedShowMetroLinesOnLinearMap.value, toggleShowMetroLinesOnLinearMap);
			group3.AddCheckbox ("Show train lines", m_savedShowTrainLinesOnLinearMap.value, toggleShowTrainLinesOnLinearMap);
			group3.AddCheckbox ("Show bus lines (be careful!)", m_savedShowBusLinesOnLinearMap.value, toggleShowBusLinesOnLinearMap);
			group3.AddCheckbox ("Show airports", m_savedShowAirportsOnLinearMap.value, toggleShowAirportsOnLinearMap);
			group3.AddCheckbox ("Show seaports", m_savedShowPassengerPortsOnLinearMap.value, toggleShowPassengerPortsOnLinearMap);
			group3.AddCheckbox ("Show taxi stops (AD only)", m_savedShowTaxiStopsOnLinearMap.value, toggleShowTaxiStopsOnLinearMap);
			group3.AddSpace (20);
			group3.AddCheckbox ("Show near lines in public services buildings' world info panel", m_savedShowNearLinesInCityServicesWorldInfoPanel.value, toggleShowNearLinesInCityServicesWorldInfoPanel);
			group3.AddCheckbox ("Show near lines in zoned buildings' world info panel", m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value, toggleShowNearLinesInZonedBuildingWorldInfoPanel);
		}

		private void updateDropDowns (string oldName, string newName)
		{

			string idxSel = editorSelector.selectedValue;			
			editorSelector.items = TLMAutoColorPalettes.paletteListForEditing;
			if (!TLMAutoColorPalettes.paletteListForEditing.Contains (idxSel)) {
				if (idxSel != oldName || !TLMAutoColorPalettes.paletteListForEditing.Contains (newName)) {
					editorSelector.selectedIndex = 0;
				} else {
					idxSel = newName;
					editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList ().IndexOf (idxSel);
				}
			} else {
				editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList ().IndexOf (idxSel);
			}

			
			idxSel = (busPalette.selectedValue);
			busPalette.items = TLMAutoColorPalettes.paletteList;
			if (!TLMAutoColorPalettes.paletteList.Contains (idxSel)) {
				if (idxSel != oldName || !TLMAutoColorPalettes.paletteList.Contains (newName)) {
					idxSel = TLMAutoColorPalettes.PALETTE_RANDOM;
				} else {
					idxSel = newName;
				}
			}
			busPalette.selectedIndex = TLMAutoColorPalettes.paletteList.ToList ().IndexOf (idxSel);
			setAutoColorBus (TLMAutoColorPalettes.paletteList.ToList ().IndexOf (idxSel));
			
			idxSel = (metroPalette.selectedValue);
			metroPalette.items = TLMAutoColorPalettes.paletteList;
			if (!TLMAutoColorPalettes.paletteList.Contains (idxSel)) {
				if (idxSel != oldName || !TLMAutoColorPalettes.paletteList.Contains (newName)) {
					idxSel = TLMAutoColorPalettes.PALETTE_RANDOM;
				} else {
					idxSel = newName;
				}
			}
			metroPalette.selectedIndex = TLMAutoColorPalettes.paletteList.ToList ().IndexOf (idxSel);
			setAutoColorMetro (TLMAutoColorPalettes.paletteList.ToList ().IndexOf (idxSel));
			
			idxSel = (trainPalette.selectedValue);
			trainPalette.items = TLMAutoColorPalettes.paletteList;
			if (!TLMAutoColorPalettes.paletteList.Contains (idxSel)) {
				if (idxSel != oldName || !TLMAutoColorPalettes.paletteList.Contains (newName)) {
					idxSel = TLMAutoColorPalettes.PALETTE_RANDOM;
				} else {
					idxSel = newName;
				}
			}
			trainPalette.selectedIndex = TLMAutoColorPalettes.paletteList.ToList ().IndexOf (idxSel);
			setAutoColorTrain (TLMAutoColorPalettes.paletteList.ToList ().IndexOf (idxSel));
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
					"SubwayIcon","TrainIcon","BusIcon","ShipIcon","AirplaneIcon","TaxiIcon","DayIcon","NightIcon","DisabledIcon"
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
			m_savedNomenclaturaOnibus.value = idx;
		}

		private void setNamingMetro (int idx)
		{
			m_savedNomenclaturaMetro.value = idx;
		}

		private void setNamingTrain (int idx)
		{
			m_savedNomenclaturaTrem.value = idx;
		}
		
		private void setNamingBusSeparador (int idx)
		{
			m_savedNomenclaturaOnibusSeparador.value = idx;
		}
		
		private void setNamingMetroSeparador (int idx)
		{
			m_savedNomenclaturaMetroSeparador.value = idx;
		}
		
		private void setNamingTrainSeparador (int idx)
		{
			m_savedNomenclaturaTremSeparador.value = idx;
		}
		
		private void setNamingBusPrefixo (int idx)
		{
			m_savedNomenclaturaOnibusPrefixo.value = idx;
		}
		
		private void setNamingMetroPrefixo (int idx)
		{
			m_savedNomenclaturaMetroPrefixo.value = idx;
		}
		
		private void setNamingTrainPrefixo (int idx)
		{
			m_savedNomenclaturaTremPrefixo.value = idx;
		}
		
		private void setAutoColorBus (int idx)
		{
			m_savedAutoColorPaletteOnibus.value = TLMAutoColorPalettes.paletteList [idx];
		}

		private void setAutoColorMetro (int idx)
		{
			m_savedAutoColorPaletteMetro.value = TLMAutoColorPalettes.paletteList [idx];
		}

		private void setAutoColorTrain (int idx)
		{
			m_savedAutoColorPaletteTrem.value = TLMAutoColorPalettes.paletteList [idx];
		}
		
		private void toggleOverrideSavedNomenclaturaOnibusZeros(bool b){
			m_savedNomenclaturaOnibusZeros.value = b;
		}
		private void toggleOverrideSavedNomenclaturaTremZeros(bool b){
			m_savedNomenclaturaTremZeros.value = b;
		}
		private void toggleOverrideSavedNomenclaturaMetroZeros(bool b){
			m_savedNomenclaturaMetroZeros.value = b;
		}


		private void toggleAutoColor (bool b)
		{
			m_savedAutoColor.value = b;
		}

		private void toggleAutoColorRandomOveflow (bool b)
		{
			m_savedUseRandomColorOnPaletteOverflow.value = b;
		}
		private void toggleOverrideDefaultLineInfoPanel(bool b){
			m_savedOverrideDefaultLineInfoPanel.value = b;
		}

		private void toggleAutoColorBasedOnPrefix (bool b){
			m_savedAutoColorBasedOnPrefix.value = b;
		}

		private void toggleCircularAutoName (bool b)
		{
			m_savedCircularOnSingleDistrict.value = b;
		}

		private void toggleAutoNaming (bool b)
		{
			m_savedAutoNaming.value = b;
		}

		private void toggleShowMetroLinesOnLinearMap (bool b)
		{
			m_savedShowMetroLinesOnLinearMap.value = b;
		}

		private void toggleShowTrainLinesOnLinearMap (bool b)
		{
			m_savedShowTrainLinesOnLinearMap.value = b;
		}

		private void toggleShowBusLinesOnLinearMap (bool b)
		{
			m_savedShowBusLinesOnLinearMap.value = b;
		}
		
		private void toggleShowPassengerPortsOnLinearMap (bool b)
		{
			m_savedShowPassengerPortsOnLinearMap.value = b;
		}

		private void toggleShowAirportsOnLinearMap (bool b)
		{
			m_savedShowAirportsOnLinearMap.value = b;
		}

		private void toggleShowTaxiStopsOnLinearMap (bool b)
		{
			m_savedShowTaxiStopsOnLinearMap.value = b;
		}

		private void toggleShowNearLinesInCityServicesWorldInfoPanel (bool b)
		{
			m_savedShowNearLinesInCityServicesWorldInfoPanel.value = b;
		}
		private void toggleShowNearLinesInZonedBuildingWorldInfoPanel(bool b){
			m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = b;
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
