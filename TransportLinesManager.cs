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



namespace TransportLinesManager 
{
	public class TransportLinesManagerMod :  IUserMod{
			
		public string Name { 
			get { 

				return "Transport Lines Manager 0.3β"; 
			} 
		}

		public string Description { 
			get { return "A shortcut to manage all city's public transports lines. THIS DOESN'T WORK WHEN CHANGES THE CITY INGAME!"; } 
		}


	}
		
	public class TransportLinesManagerThreadMod : ThreadingExtensionBase
	{			
		public bool init ()
		{	
			return true;
		}
			
		public override void OnCreated (IThreading threading)
		{			
				
		}
			
		public override void OnUpdate (float realTimeDelta, float simulationTimeDelta)
		{
			TransportListInterface.init ();
		}	
	}




	public class TransportListInterface {
		
		private static List<GameObject> visualizations;
//		private Material lineMaterial;
		public static  UIView uiView;
		public static  UIButton abrePainelButton;
		public static  bool b_show = false;
		public static   UIPanel painel ;
		public  static  GameObject painelContainer;
		public  static  List<GameObject> linesButtons = new List<GameObject> ();
		public  static  float offset;
		public static bool initialized = false;

		public  static  void init(){
			
			if (initialized)
				return;	
			uiView = GameObject.FindObjectOfType<UIView> ();			
			if (uiView == null)
				return;
			/////////////////////////////////////////////////////

			GameObject painelTransportesContainer = new GameObject ();			
			painelTransportesContainer.transform.parent = uiView.transform;
			abrePainelButton = painelTransportesContainer.AddComponent<UIButton> ();
			abrePainelButton.relativePosition = new Vector3 (125.0f, 10.0f);
			abrePainelButton.tooltip = "Transport Lines/Linhas de transporte";
			abrePainelButton.width = 40;
			abrePainelButton.height = 40;
			initButton (abrePainelButton, true,"ToolbarIconPublicTransport");	
			abrePainelButton.eventClick += abrirTelaTransportes;	
//			abrePainelButton.normalBgSprite="IconTransportLineManager";
			
			
			painelContainer = new GameObject ();
			painelContainer.transform.parent = uiView.transform;
			painel = painelContainer.AddComponent<UIPanel> ();			
			painel.Hide ();
			painel.absolutePosition = new Vector3 (125.0f, 20.0f);
			painel.width = 650;
			painel.height = 530;
			painel.color = new Color32(16, 32, 48, 255);
			painel.backgroundSprite="InfoviewPanel";
			painel.eventLeaveFocus += fecharTelaTransportes;
			
			//
			GameObject voltarButtonContainer = new GameObject ();
			voltarButtonContainer.transform.parent = painelContainer.transform;
			UIButton voltarButton = voltarButtonContainer.AddComponent<UIButton> ();	
			voltarButton.relativePosition = new Vector3 (400f, 10f);
			voltarButton.text = "Close/Fechar";
			voltarButton.width = 240;
			voltarButton.height = 30;
			initButton (voltarButton, true,"ButtonMenu");	
			voltarButton.eventClick += fecharTelaTransportes;
			
			GameObject titleLabelContainer = new GameObject ();
			titleLabelContainer.transform.parent = painelContainer.transform;
			UILabel titleLabel = titleLabelContainer.AddComponent<UILabel> ();	
			titleLabel.relativePosition = new Vector3 (15f, 15f);
			titleLabel.textAlignment = UIHorizontalAlignment.Center;
			titleLabel.text = "Transport Lines/Linhas de transporte";
			titleLabel.width = 630;
			titleLabel.height = 30;
			//			UITextField close = painel;
			//			close.absolutePosition = new Vector3 (75.0f, 10.0f);
			//			close.eventClick += fecharTelaTransportes;
			
//			lineMaterial = new Material(Shader.Find ("Transparent/Diffuse"));
			
			initialized = true;
		}

		
		
		private static  void initButton (UIButton button, bool isCheck, string baseSprite)
		{
			
			string sprite = baseSprite;//"ButtonMenu";
			string spriteHov = baseSprite + "Hovered";
			button.normalBgSprite = sprite;
			button.disabledBgSprite = sprite + "Disabled";
			button.hoveredBgSprite = spriteHov;
			button.focusedBgSprite = spriteHov;
			button.pressedBgSprite = sprite + "Pressed";
			button.textColor = new Color32 (255, 255, 255, 255);
			
		}

		private  static void abrirTelaTransportes (UIComponent component, UIMouseEventParameter eventParam)
		{
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE1!");
			abrePainelButton.Hide ();
			painel.Show ();	
			listaLinhas ();
			painel.height = 15.0f + offset;
			//			MainMenu ();
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE2!");
		}
		
		private static void fecharTelaTransportes (UIComponent component, UIFocusEventParameter eventParam){
			fecharTelaTransportes (component, (UIMouseEventParameter) null);
		}
		
		private static void fecharTelaTransportes (UIComponent component, UIMouseEventParameter eventParam)
		{
			painel.Hide ();	
			abrePainelButton.Show ();
			foreach (GameObject o in linesButtons) {
				UnityEngine.Object.Destroy (o);
			}
			linesButtons.Clear ();
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "FECHA!");
		}
		
		private  static void listaLinhas ()
		{
			TransportManager tm = Singleton<TransportManager>.instance;
			Dictionary<Int32,UInt16> trens = new Dictionary<Int32,UInt16> ();
			Dictionary<Int32,UInt16> metro = new Dictionary<Int32,UInt16> ();
			Dictionary<Int32,UInt16> onibus = new Dictionary<Int32,UInt16> ();
			
			for (ushort i =0; i< tm.m_lines.m_size; i++) {
				TransportLine t = tm.m_lines.m_buffer [(int)i];
				if (t.m_lineNumber == 0)
					continue;
				
				switch (t.Info.m_transportType) {
				case TransportInfo.TransportType.Bus:
					onibus.Add (t.m_lineNumber, i);
					break;
					
				case TransportInfo.TransportType.Metro:
					metro.Add (t.m_lineNumber, i);
					break;
					
				case TransportInfo.TransportType.Train:
					trens.Add (t.m_lineNumber, i);
					break;
				default:
					continue;
				}
			}
			offset = 30;
			offset += drawButtonsFromDictionary (trens, offset);
			offset += drawButtonsFromDictionary (metro, offset);
			offset += drawButtonsFromDictionary (onibus, offset);
		}
		
		private  static  float drawButtonsFromDictionary (Dictionary<Int32,UInt16> map, float offset)
		{


			TransportManager tm = Singleton<TransportManager>.instance;
			int j = 0;
			List<Int32> keys = map.Keys.ToList ();
			keys.Sort ();
			foreach (Int32 k in keys) {
				
				TransportLine t = tm.m_lines.m_buffer [map [k]];
				string item = "[" + t.Info.m_transportType + " | " + t.m_lineNumber + "] " + t.GetColor () + " " + tm.GetLineName ( map [k]);
				
				GameObject itemContainer = new GameObject ();
				linesButtons.Add (itemContainer);
				itemContainer.transform.parent = painelContainer.transform;
				UIButtonLineInfo itemButton = itemContainer.AddComponent<UIButtonLineInfo> ();
				
				itemButton.relativePosition = new Vector3 (10.0f + (j%18) * 35f, 10.0f + offset + 35 * (int)(j/18));
				itemButton.text = ("" + t.Info.m_transportType).Substring (0, 1) + t.m_lineNumber;
				itemButton.width = 35;
				itemButton.height = 30;
				initButton (itemButton, true, "ButtonMenu");
				itemButton.normalBgSprite = "EmptySprite";
				itemButton.color = t.GetColor ();
				itemButton.textColor = ContrastColor(t.GetColor ());
				itemButton.lineID = map [k];
				itemButton.eventClick += openLineInfo;
				j++;
				
			}
			if (j > 0) {
				return 35 * (int)(j/18+1);
			} else {
				return 0;
			}
		}
		private  static Color ContrastColor(Color color)
		{
			int d = 0;
			
			// Counting the perceptive luminance - human eye favors green color... 
			double a = ( 0.299 * color.r + 0.587 * color.g + 0.114 * color.b);
			
			if (a > 0.5)
				d = 0; // bright colors - black font
			else
				d = 1; // dark colors - white font
			
			return  new Color(d, d, d,1);
		}
		public  static void openLineInfo(UIComponent component, UIMouseEventParameter eventParam){
			ushort lineID = (component as UIButtonLineInfo).lineID;			
			TransportManager tm = Singleton<TransportManager>.instance;
			InstanceID id = default(InstanceID);
			id.TransportLine = lineID;			
			Vector3 pointRef = tm.m_lineCurves [(int)lineID] [0].Min ();
			DefaultTool.OpenWorldInfoPanel (id, tm.m_lineCurves [(int)lineID][0].Min () - new Vector3 (3f, 3f, 3f));
			Camera.current.transform.position = pointRef;
			Camera.current.
		}
		
		private  static  void VisualizePath(Vector3[] positions) {
						
			Vector3 offset = new Vector3(0, 5, 0);
			
			for (int i = 0; i < positions.Length - 1 ; i++)
			{
				GameObject lineGameObject = new GameObject();
				LineRenderer theLine = lineGameObject.AddComponent<LineRenderer>();
//				theLine.material = lineMaterial;
				theLine.enabled = true;
				theLine.SetWidth(5, 5);
				theLine.SetVertexCount(2);
				theLine.SetPosition(0, positions[i + 0] + offset);
				theLine.SetPosition(1, positions[i + 1] + offset);
				
				visualizations.Add(lineGameObject);
			}          
			
		}
		
		
		static void HideAllVisualizations()
		{
			
			foreach (GameObject v in visualizations)
			{
				GameObject.Destroy(v);
			}
			
			visualizations = new List<GameObject>();
		}
	}

	public class UIButtonLineInfo : UIButton {
		public ushort lineID;
	}

	public class ResourceLoader
	{
		
		public static Assembly ResourceAssembly
		{
			get {
				//return null;
				return Assembly.GetAssembly(typeof(ResourceLoader));
			}
		}
		
		
		public static byte[] loadResourceData(string name)
		{
			name = "TransportLinesManager." + name;

//			string [] resources = ResourceAssembly.GetManifestResourceNames();
			
//			Log.debug("resources: ");
//			// Build the string of resources.
//			foreach (string resource in resources)
//				Log.debug("RES = " + resource);

			
			UnmanagedMemoryStream stream  = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
			if (stream == null)
			{
				Log.error("Could not find resource: " + name);
				return null;
			}
			
			BinaryReader read = new BinaryReader(stream);
			return read.ReadBytes((int)stream.Length);
		}
		
		public static string loadResourceString(string name)
		{
			name = "TransportLinesManager." + name;
			
			UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
			if (stream == null)
			{
				Log.error("Could not find resource: " + name);
				return null;
			}
			
			StreamReader read = new StreamReader(stream);
			return read.ReadToEnd();
		}
		
		
		
		
		public static Texture2D loadTexture(int x, int y, string filename)
		{
			try
			{
				Texture2D texture = new Texture2D(x,y);
				texture.LoadImage(loadResourceData(filename));
				return texture;
			}
			catch (Exception e)
			{
				Log.error("The file could not be read:" + e.Message);                
			}
			
			return null;
		}
		
	}

	public class Log
	{
		private static bool internalGameLog = true;
		private static StreamWriter _logFile;
		public static StreamWriter logFile {
			get {
				
				if(_logFile == null) {
					_logFile = new StreamWriter(new FileStream("TransportLinesManager.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
					_logFile.WriteLine("Loggin started at" + DateTime.Now.ToString());
					_logFile.WriteLine("Base Log from TrafficReportMod");
				}
				return _logFile;
			}
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void error(string message)
		{
			if (internalGameLog) {
				 DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, message);
			}
			logFile.WriteLine("ERROR: " + message);
			logFile.Flush();
		}
		
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void info(string message)
		{
			
			if (internalGameLog) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, message);
			}
			logFile.WriteLine("INFO: " + message);
			logFile.Flush();
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void debug(string message)
		{
			if (internalGameLog) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, message);
			}
			logFile.WriteLine("DEBUG: " + message);
			logFile.Flush();
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void warn(string message)
		{
			if (internalGameLog) {
				DebugOutputPanel.AddMessage (PluginManager.MessageType.Warning, message);
			}
			logFile.WriteLine("WARN: " + message);
			logFile.Flush();
		}
	}
}
