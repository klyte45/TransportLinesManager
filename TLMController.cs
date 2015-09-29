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

namespace Klyte.TransportLinesManager
{
	public class TLMController
	{
		//		private Material lineMaterial;
		public static TLMController instance;
		public static UITextureAtlas taTLM = null;
		public static UITextureAtlas taLineNumber = null;


				
		public UIView uiView;
		public UIComponent mainRef;
		public  TransportManager tm;
		public  InfoManager im;
		public   UIButton abrePainelButton;
		public  bool initialized = false;
		private TLMMainPanel m_mainPanel;
		private TLMLineInfoPanel m_lineInfoPanel;
		private int lastLineCount = 0;

		public TLMMainPanel mainPanel {
			get {
				return m_mainPanel;
			}
		}

		public TLMLineInfoPanel lineInfoPanel {
			get {
				return m_lineInfoPanel;
			}
		}

		public Transform transform {
			get {
				return mainRef.transform;
			}
		}

		public TLMController ()
		{
		}

		public    void destroy ()
		{

			
			
			if (m_mainPanel != null && m_mainPanel.gameObject != null) {
				UnityEngine.Object.Destroy (m_mainPanel.gameObject);
			}

			if (abrePainelButton != null && abrePainelButton.gameObject != null) {
				UnityEngine.Object.Destroy (abrePainelButton.gameObject);
			}
			if (m_lineInfoPanel != null && m_lineInfoPanel.linearMap != null && m_lineInfoPanel.linearMap.gameObject != null) {
				UnityEngine.Object.Destroy (m_lineInfoPanel.linearMap.gameObject);
			}
			
			if (m_lineInfoPanel != null && m_lineInfoPanel.gameObject != null) {
				UnityEngine.Object.Destroy (m_lineInfoPanel.gameObject);
			}

			initialized = false;
		}
		
		public    void init ()
		{
			if (((GameObject.FindGameObjectWithTag ("GameController").GetComponent<ToolController> ()).m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
				return;
			}
			if (!initialized) {	


				uiView = GameObject.FindObjectOfType<UIView> ();			
				if (uiView == null)
					return;
				mainRef = uiView.FindUIComponent<UIPanel> ("InfoPanel").Find<UITabContainer> ("InfoViewsContainer").Find<UIPanel> ("InfoViewsPanel");


				
				tm = Singleton<TransportManager>.instance;				
				im = Singleton<InfoManager>.instance;
				createViews ();				
				mainRef.clipChildren = false;
				UIPanel container = mainRef.Find<UIPanel> ("Container");
				abrePainelButton = container.Find<UIButton> ("PublicTransport");
//				container.AttachUIComponent (abrePainelButton.gameObject);
				
				
				abrePainelButton.atlas = taTLM;
				abrePainelButton.tooltip = "Transport Lines Manager (v" + TransportLinesManagerMod.version + ")";
				abrePainelButton.name = "TransportLinesManagerButton";
				TLMUtils.initButtonFg (abrePainelButton, false, "TransportLinesManagerIcon");	
				abrePainelButton.eventClick += swapWindow;	
				abrePainelButton.eventVisibilityChanged += (UIComponent component, bool value) => {
					if (!value) {
						fecharTelaTransportes (component, (UIMouseEventParameter)null);
					}
				};
				
				
				container.height = 37 * ((int)((container.childCount + 1) / 2)) + 6;

//				Array16<Vehicle> vehiclesOriginal =  VehicleManager.instance.m_vehicles;
//				VehicleManager.instance.m_vehicles =  new Array16<Vehicle> (ushort.MaxValue);
//				ushort j;
//				for(int i =0; i< vehiclesOriginal.m_buffer.Length;i++){
//					VehicleManager.instance.m_vehicles.CreateItem(out j);
//					VehicleManager.instance.m_vehicles.m_buffer[j] = vehiclesOriginal.m_buffer[i];
//				}
//				var prop = VehicleManager.instance.GetType().GetField("m_renderBuffer", System.Reflection.BindingFlags.NonPublic
//				                                | System.Reflection.BindingFlags.Instance);
//				prop.SetValue(VehicleManager.instance, new ulong[1024]);


				initialized = true;
			}
			if (m_mainPanel.isVisible || m_lineInfoPanel.isVisible) {
				if (!tm.LinesVisible) {
					tm.LinesVisible = true;
				}
				if (im.CurrentMode != InfoManager.InfoMode.Transport) {			
					im.SetCurrentMode (InfoManager.InfoMode.Transport, InfoManager.SubInfoMode.NormalTransport);
				}
			}
			if (m_lineInfoPanel.isVisible) {
				m_lineInfoPanel.updateBidings ();
			}

			if (lastLineCount != tm.m_lineCount && (TransportLinesManagerMod.savedAutoColor.value || TransportLinesManagerMod.savedAutoNaming.value)) {
				CheckForAutoChanges ();
				if (mainPanel.isVisible) {
					mainPanel.Show ();
				}
			}
			lastLineCount = tm.m_lineCount;
		}

		void CheckForAutoChanges ()
		{
			for (ushort i = 0; i < tm.m_lines.m_size; i++) {
				TransportLine t = tm.m_lines.m_buffer [(int)i];
				if ((t.m_flags & (TransportLine.Flags.Created)) != TransportLine.Flags.None) {
					if (TransportLinesManagerMod.savedAutoNaming.value && ((t.m_flags & (TransportLine.Flags.CustomName)) == TransportLine.Flags.None)) {
						AutoName (i);
					}
					if (TransportLinesManagerMod.savedAutoColor.value && ((t.m_flags & (TransportLine.Flags.CustomColor)) == TransportLine.Flags.None)) {
						AutoColor (i);
					}
				}
			}
		}

		public Color AutoColor (ushort i)
		{
			TransportLine t = tm.m_lines.m_buffer [(int)i];
			try {
				string pal = TLMAutoColorPalettes.PALETTE_RANDOM;
				if (t.Info.m_transportType == TransportInfo.TransportType.Bus) {
					pal = TransportLinesManagerMod.savedAutoColorPaletteOnibus.value;
				} else if (t.Info.m_transportType == TransportInfo.TransportType.Metro) {
					pal = TransportLinesManagerMod.savedAutoColorPaletteMetro.value;
				} else if (t.Info.m_transportType == TransportInfo.TransportType.Train) {
					pal = TransportLinesManagerMod.savedAutoColorPaletteTrem.value;
				}
				Color c = TLMAutoColorPalettes.getColor (t.m_lineNumber, pal);
				TLMUtils.setLineColor (i, c);
				return c;
			} catch (Exception e) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Error, "ERRO!!!!! " + e.Message);
				TransportLinesManagerMod.savedAutoColor.value = false;
				return Color.clear;
			}
		}

		public void AutoName (ushort lineIdx)
		{
			TransportLine t = tm.m_lines.m_buffer [(int)lineIdx];
			try {
				int mn = (int)ModoNomenclatura.Numero;
				if (t.Info.m_transportType == TransportInfo.TransportType.Bus) {
					mn = TransportLinesManagerMod.savedNomenclaturaOnibus.value;
				} else if (t.Info.m_transportType == TransportInfo.TransportType.Metro) {
					mn = TransportLinesManagerMod.savedNomenclaturaMetro.value;
				} else if (t.Info.m_transportType == TransportInfo.TransportType.Train) {
					mn = TransportLinesManagerMod.savedNomenclaturaTrem.value;
				}
				TLMUtils.setLineName ((ushort)lineIdx, "[" + TLMUtils.getString ((ModoNomenclatura)mn, t.m_lineNumber) + "] " + TLMUtils.calculateAutoName (lineIdx));
			} catch (Exception e) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Error, "ERRO!!!!! " + e.Message);
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Error,e.StackTrace);
				TransportLinesManagerMod.savedAutoNaming.value = false;
			}
		}

	
		
		
		//NAVEGACAO

		private void swapWindow (UIComponent component, UIMouseEventParameter eventParam)
		{
			if (m_lineInfoPanel.isVisible || m_mainPanel.isVisible) {
				fecharTelaTransportes (component, eventParam);
			} else {
				abrirTelaTransportes (component, eventParam);
			}

		}

		private   void abrirTelaTransportes (UIComponent component, UIMouseEventParameter eventParam)
		{
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE1!");
			abrePainelButton.normalFgSprite = abrePainelButton.focusedFgSprite;
			m_lineInfoPanel.Hide ();
			m_mainPanel.Show ();	
			tm.LinesVisible = true;
			im.SetCurrentMode (InfoManager.InfoMode.Transport, InfoManager.SubInfoMode.NormalTransport);
			//			MainMenu ();
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE2!");
		}
		
		private  void fecharTelaTransportes (UIComponent component, UIFocusEventParameter eventParam)
		{
			fecharTelaTransportes (component, (UIMouseEventParameter)null);
		}
		
		private  void fecharTelaTransportes (UIComponent component, UIMouseEventParameter eventParam)
		{			
			abrePainelButton.normalFgSprite = abrePainelButton.disabledFgSprite;
			m_mainPanel.Hide ();	
			m_lineInfoPanel.Hide ();
			tm.LinesVisible = false;
			InfoManager im = Singleton<InfoManager>.instance;
			im.SetCurrentMode (InfoManager.InfoMode.None, InfoManager.SubInfoMode.NormalPower);
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "FECHA!");
		}

		private void createViews ()
		{
			/////////////////////////////////////////////////////			
			m_mainPanel = new TLMMainPanel (this);
			m_lineInfoPanel = new TLMLineInfoPanel (this);
		}
		


		

	}

	public class ResourceLoader
	{
		
		public static Assembly ResourceAssembly {
			get {
				//return null;
				return Assembly.GetAssembly (typeof(ResourceLoader));
			}
		}
		
		public static byte[] loadResourceData (string name)
		{
			name = "TransportLinesManager." + name;
			
			UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream (name);
			if (stream == null) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Error, "Could not find resource: " + name);
				return null;
			}
			
			BinaryReader read = new BinaryReader (stream);
			return read.ReadBytes ((int)stream.Length);
		}
		
		public static string loadResourceString (string name)
		{
			name = "TransportLinesManager." + name;
			
			UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream (name);
			if (stream == null) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Error, "Could not find resource: " + name);
				return null;
			}
			
			StreamReader read = new StreamReader (stream);
			return read.ReadToEnd ();
		}
		
		public static Texture2D loadTexture (int x, int y, string filename)
		{
			try {
				Texture2D texture = new Texture2D (x, y);
				texture.LoadImage (loadResourceData (filename));
				return texture;
			} catch (Exception e) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Error, "The file could not be read:" + e.Message);                
			}
			
			return null;
		}
		
	}

	public enum ModoNomenclatura
	{
		Numero=0,
		LatinoMinusculo=1,
		LatinoMaiusculo=2,
		GregoMinusculo=3,
		GregoMaiusculo=4,
		CirilicoMinusculo=5,
		CirilicoMaiusculo=6
	}
}