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
	public class TransportListInterface {
		//		private Material lineMaterial;
		public SavedInt savedBusCapacity;

		public   UIView uiView;
		public   UIButton abrePainelButton;		
		public  UILabel titleLabel ;
		public   bool b_show = false;
		public    UIPanel mainPanel ;
		public    List<GameObject> linesButtons = new List<GameObject> ();
		public    float offset;
		public  bool initialized = false;
		public static TransportListInterface instance;
		public CameraController m_CameraController;
		
		private  InstanceID lineIdSelecionado;
		private  TransportManager tm; 
		
		private  Dictionary<Int32,UInt16> trens;
		private  Dictionary<Int32,UInt16> metro ;
		private  Dictionary<Int32,UInt16> onibus ;
		
		//line info	
		public  UIPanel lineInfoPanel;
		public  UILabel lineLenghtLabel;			
		public  UILabel lineStopsLabel;			
		public  UITextField lineNumberLabel;		
		public  UILabel lineTransportIconTypeLabel;	
		public  UILabel viagensEvitadasLabel;
		public  UILabel passageirosEturistasLabel;
		public  UILabel veiculosLinhaLabel;
		public  UITextField lineNameField;
		public UIColorField lineColorPicker;
		//		public UIPanel scrollPanel;
		
		public  UIPanel agesChartPanel;
		public  UIRadialChartAge agesChart;	

		//ExtraData Panel		
		public    UIPanel extraDataPanel ;
		public  UILabel busCapacity;			
		public  UILabel trainCapacity;	


		public TransportListInterface(){			
			savedBusCapacity = new SavedInt("BusCapacity", Settings.gameSettingsFile, 30,true);
		}

		public    void destroy(){
			if (lineInfoPanel) {
				UnityEngine.Object.Destroy (lineInfoPanel.gameObject);
			}
			
			if (mainPanel) {
				UnityEngine.Object.Destroy (mainPanel.gameObject);
			}
			if (abrePainelButton) {
				UnityEngine.Object.Destroy (abrePainelButton.gameObject);
			}
			initialized = false;
		}
		
		public    void init(){
			if (((GameObject.FindGameObjectWithTag ("GameController").GetComponent<ToolController>()).m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
				return;
			}
			if (!initialized) {	
				setBusesCapacity(savedBusCapacity.value);
				uiView = GameObject.FindObjectOfType<UIView> ();			
				if (uiView == null)
					return;
				
				GameObject gameObject = GameObject.FindGameObjectWithTag ("MainCamera");
				if (gameObject != null)
				{
					m_CameraController = gameObject.GetComponent<CameraController> ();
				}
				
				tm = Singleton<TransportManager>.instance;

				createViews();

				initialized = true;
			}
			if (mainPanel.isVisible) {
				updateExtraInfoBidings();
			}
			if (lineInfoPanel.isVisible) {
				updateBidings ();
			}
			
		}
		
		private  void createUIElement<T>(ref T uiItem, Transform parent) where T : Component{			
			GameObject container = new GameObject ();
			container.transform.parent = parent;
			uiItem = container.AddComponent<T>();	
		}
		private  void uiTextFieldDefaults(UITextField uiItem){
			uiItem.selectionSprite = "EmptySprite";
			uiItem.useOutline = true;
			uiItem.hoveredBgSprite = "TextFieldPanelHovered";
			uiItem.focusedBgSprite ="TextFieldPanel";
			uiItem.builtinKeyNavigation = true;
			uiItem.submitOnFocusLost = true;
		}
		
		
		
		//NAVEGACAO
		private   void initButton (UIButton button, bool isCheck, string baseSprite)
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
		
		private   void abrirTelaTransportes (UIComponent component, UIMouseEventParameter eventParam)
		{
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE1!");
			abrePainelButton.Hide ();
			lineInfoPanel.Hide ();
			mainPanel.Show ();	
			listaLinhas ();	
			tm.LinesVisible = true;
			//			MainMenu ();
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE2!");
		}
		
		private  void fecharTelaTransportes (UIComponent component, UIFocusEventParameter eventParam){
			fecharTelaTransportes (component, (UIMouseEventParameter) null);
		}
		
		private  void fecharTelaTransportes (UIComponent component, UIMouseEventParameter eventParam)
		{
			mainPanel.Hide ();	
			lineInfoPanel.Hide ();
			abrePainelButton.Show ();
			clearLinhas ();			
			tm.LinesVisible = false;
			//			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "FECHA!");
		}
		
		private  void closeLineInfo(UIComponent component, UIMouseEventParameter eventParam)
		{			
			TransportLine t = tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine];	
			t.m_flags &= ~TransportLine.Flags.Selected;
			lineInfoPanel. Hide();			
			clearLinhas ();
			listaLinhas ();
			mainPanel.Show ();
		}
		
		
		
		public   void openLineInfo(UIComponent component, UIMouseEventParameter eventParam){
			ushort lineID = (component as UIButtonLineInfo).lineID;			
			
			lineIdSelecionado = default(InstanceID);
			lineIdSelecionado.TransportLine = lineID;	
			//lines info
			float totalSize = 0f;
			for (int i = 0; i< tm.m_lineCurves [(int)lineID].Length; i++) {
				Bezier3 bez = tm.m_lineCurves [(int)lineID][i];
				totalSize +=  calcBezierLenght (bez.a,bez.b,bez.c,bez.d,0.1f);
			}
			
			TransportLine t = tm.m_lines.m_buffer [(int)lineID];
			lineLenghtLabel.text = string.Format ("{0:N2}",totalSize);
			lineNumberLabel.text = ""+t.m_lineNumber;
			lineNumberLabel.color = tm.GetLineColor (lineID);
			lineStopsLabel.text = ""+t.CountStops(lineID);
			lineNameField.text = tm.GetLineName (lineID);
			lineTransportIconTypeLabel.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(t.Info.m_transportType);
			t.m_flags |= TransportLine.Flags.Selected;
			m_CameraController.SetTarget (lineIdSelecionado, tm.m_lineCurves [(int)lineID] [0].Min (), true);
			lineColorPicker.selectedColor = tm.GetLineColor (lineID);
			
			lineInfoPanel.Show ();
			mainPanel.Hide ();
		}
		
		private  void clearLinhas(){
			
			foreach (GameObject o in linesButtons) {
				UnityEngine.Object.Destroy (o);
			}
			linesButtons.Clear ();
			
		}
		
		private   void listaLinhas ()
		{
			
			trens = new Dictionary<int, ushort>();
			metro = new Dictionary<int, ushort>();
			onibus= new Dictionary<int, ushort>();
			
			for (ushort i =0; i< tm.m_lines.m_size; i++) {
				TransportLine t = tm.m_lines.m_buffer [(int)i];
				if (t.m_lineNumber == 0 || t.CountStops(i) ==0)
					continue; 
				switch (t.Info.m_transportType) {
				case TransportInfo.TransportType.Bus:
					while(onibus.ContainsKey(t.m_lineNumber)){
						t.m_lineNumber++;
					}
					onibus.Add (t.m_lineNumber, i);
					break;
					
				case TransportInfo.TransportType.Metro:
					while(metro.ContainsKey(t.m_lineNumber)){
						t.m_lineNumber++;
					}
					metro.Add (t.m_lineNumber, i);
					break;
					
				case TransportInfo.TransportType.Train:					
					while(trens.ContainsKey(t.m_lineNumber)){
						t.m_lineNumber++;
					}
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
			mainPanel.height = 15.0f + offset;
		}
		
		private    float drawButtonsFromDictionary (Dictionary<Int32,UInt16> map, float offset)
		{		
			int j = 0;
			List<Int32> keys = map.Keys.ToList ();
			keys.Sort ();
			foreach (Int32 k in keys) {
				
				TransportLine t = tm.m_lines.m_buffer [map [k]];
				string item = "[" + t.Info.m_transportType + " | " + t.m_lineNumber + "] " + t.GetColor () + " " + tm.GetLineName ( map [k]);
				GameObject itemContainer = new GameObject ();
				linesButtons.Add (itemContainer);				
				
				itemContainer.transform.parent = mainPanel.transform;
				UIButtonLineInfo itemButton = itemContainer.AddComponent<UIButtonLineInfo> ();
				
				itemButton.relativePosition = new Vector3 (10.0f + (j%10) * 65f, 10.0f + offset + 35 * (int)(j/10));
				itemButton.text = ("" + t.Info.m_transportType).Substring (0, 1) + t.m_lineNumber;
				itemButton.width = 60;
				itemButton.height = 30;
				initButton (itemButton, true, "ButtonMenu");
				itemButton.normalBgSprite = "EmptySprite";
				itemButton.color =tm.GetLineColor (map [k]);
				itemButton.textColor = ContrastColor(t.GetColor ());
				itemButton.lineID = map [k];
				itemButton.eventClick += openLineInfo;
				itemButton.textScale=1.2f;
				
				itemButton.name = "TransportLinesManagerLineButton"+itemButton.text;
				j++;
				
			}
			if (j > 0) {
				return 35 * (int)(j/10+1);
			} else {
				return 0;
			}
		}
		
		//ACOES
		private  void saveLineName(UIComponent u, string value){
			if (u.hasFocus) {
				if (value.Length > 0) {
					Singleton<InstanceManager>.instance.SetName (lineIdSelecionado, value);
					tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_flags |= TransportLine.Flags.CustomName;
				} else {				
					tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_flags &= ~TransportLine.Flags.CustomName;
				}
			} else {
			}
		}
		
		private  void saveLineNumber(UIComponent u, string value){
			if (u.hasFocus) {
				if (value.Length > 0) {
					bool numeroUsado = true;
					ushort num = UInt16.Parse(value);
					switch	(tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].Info.m_transportType){
					case TransportInfo.TransportType.Bus:
						numeroUsado = onibus.Keys.Contains(num) &&  onibus[num]!= lineIdSelecionado.TransportLine;
						break;
						
					case TransportInfo.TransportType.Metro:
						numeroUsado = metro.Keys.Contains(num) &&  metro[num]!= lineIdSelecionado.TransportLine;
						break;
						
					case TransportInfo.TransportType.Train:
						numeroUsado = trens.Keys.Contains(num) &&  trens[num]!= lineIdSelecionado.TransportLine;
						break;
					}
					if(numeroUsado || num>=10000 || num <1){
						lineNumberLabel.textColor = new Color(1,0,0,1);
					}else{
						lineNumberLabel.textColor = new Color(1,1,1,1);
						tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_lineNumber = num;
					}
				} else {				
				}
			} else {
			}
		}
		
		//UTILS
		private  void criaFatiaELegenda(Color c, UIRadialChartAge chart, UIPanel legendPanel, string localeID, float offsetY){
			chart.AddSlice (c, c);
			UIPanel legendItemContainer = null;
			createUIElement<UIPanel> (ref legendItemContainer, legendPanel.transform);
			legendItemContainer.width = legendPanel.width;
			legendItemContainer.relativePosition = new Vector3 (0f, offsetY);
			legendItemContainer.name = "LegendItem";
			legendItemContainer.autoLayout = false ;
			legendItemContainer.useCenter  = true  ;
			legendItemContainer.wrapLayout = false ;
			legendItemContainer.height = 20;
			UILabel legendColor = null;
			createUIElement<UILabel> (ref legendColor, legendItemContainer.transform);
			legendColor.backgroundSprite = "EmptySprite";
			legendColor.width = 10;
			legendColor.height = 10;
			legendColor.relativePosition = new Vector3 (0,0);
			legendColor.color = c;
			UILabel legendName = null;
			createUIElement<UILabel> (ref legendName, legendItemContainer.transform);
			legendName.textAlignment = UIHorizontalAlignment.Right;
			legendName.width = legendItemContainer.width - 10;
			legendName.localeID = localeID;
			legendName.textScale = 0.6f;
			legendName.relativePosition = new Vector3 (15f, 2f);
			legendName.verticalAlignment = UIVerticalAlignment.Middle;
		}
		
		private   Color ContrastColor(Color color)
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
		private  float calcBezierLenght(Vector3 a, Vector3 b, Vector3 c, Vector3 d , float precision){
			
			Vector3 aa = (-a + 3*(b-c) + d);
			Vector3 bb = 3*(a+c) - 6*b;
			Vector3 cc = 3*(b-a);
			
			int len = (int)(1.0f / precision);
			float[]	arcLengths = new float[len + 1];
			arcLengths[0] = 0;
			
			Vector3 ov = a;
			Vector3 v;
			float clen = 0.0f;
			for(int i = 1; i <= len; i++) {
				float t =(i * precision);
				v = ((aa* t + (bb))* t + cc)* t + a;
				clen += (ov - v).magnitude;
				arcLengths[i] = clen;
				ov = v;
			}
			return clen;
			
		}
		private void createViews(){
			/////////////////////////////////////////////////////			
			createMainView ();
			createInfoView ();
			
			createExtraOptionsView ();
		}

		private void createInfoView(){
			//line info painel
			
			createUIElement<UIPanel> (ref lineInfoPanel, uiView.transform);		
			lineInfoPanel.Hide ();
			lineInfoPanel.absolutePosition = new Vector3 (125.0f, 20.0f);
			lineInfoPanel.width = 650;
			lineInfoPanel.height = 290;
			lineInfoPanel.color = new Color32 (16, 32, 48, 255);
			lineInfoPanel.backgroundSprite = "InfoviewPanel";
			lineInfoPanel.name = "LineInfoPanel";
			lineInfoPanel.autoLayoutPadding = new RectOffset (5, 5, 10, 10);
			lineInfoPanel.autoLayout = false;
			lineInfoPanel.useCenter = true;
			lineInfoPanel.wrapLayout = false;
			lineInfoPanel.canFocus= true;
			
			
			
			createUIElement<UILabel> (ref lineTransportIconTypeLabel, lineInfoPanel.transform);
			
			lineTransportIconTypeLabel.autoSize = false;
			lineTransportIconTypeLabel.relativePosition = new Vector3 (10f, 15f);
			lineTransportIconTypeLabel.width = 30;
			lineTransportIconTypeLabel.height = 20;
			lineTransportIconTypeLabel.name = "LineTransportIcon";		
			
			createUIElement<UITextField> (ref lineNumberLabel, lineInfoPanel.transform);	
			lineNumberLabel.autoSize = false;
			lineNumberLabel.relativePosition = new Vector3 (50f, 5f);			 
			lineNumberLabel.horizontalAlignment = UIHorizontalAlignment.Center;
			lineNumberLabel.text = "";
			lineNumberLabel.width = 80;
			lineNumberLabel.height = 40;			
			lineNumberLabel.name = "LineNumberLabel";			
			lineNumberLabel.normalBgSprite = "EmptySprite";
			lineNumberLabel.textScale = 1.7f;
			lineNumberLabel.padding = new RectOffset (5, 5, 5, 5);
			lineNumberLabel.color = new Color (0, 0, 0, 1);
			uiTextFieldDefaults (lineNumberLabel);
			lineNumberLabel.numericalOnly = true;
			lineNumberLabel.maxLength = 4;
			lineNumberLabel.eventTextChanged += saveLineNumber;
			lineNumberLabel.eventLostFocus += ( component,  eventParam) => {
				TransportLine t = tm.m_lines.m_buffer [lineIdSelecionado.TransportLine];
				lineNumberLabel.text = "" + t.m_lineNumber;
			};
			lineNumberLabel.zOrder = 10;
			
			
			createUIElement<UITextField> (ref lineNameField, lineInfoPanel.transform);
			lineNameField.autoSize = false;
			lineNameField.relativePosition = new Vector3 (190f, 10f);			 
			lineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
			lineNameField.text = "NOME";
			lineNameField.width = 450;
			lineNameField.height = 25;			
			lineNameField.name = "LineNameLabel";		
			lineNameField.maxLength = 32;
			lineNameField.textScale=1.5f;
			uiTextFieldDefaults (lineNameField);
			lineNameField.eventTextChanged += saveLineName;
			lineNameField.eventLostFocus += ( component,  eventParam) => {
				lineNameField.text = tm.GetLineName (lineIdSelecionado.TransportLine);
			};
			
			createUIElement<UILabel> (ref lineLenghtLabel, lineInfoPanel.transform);
			lineLenghtLabel.autoSize = false;
			lineLenghtLabel.relativePosition = new Vector3 (10f, 60f);			 
			lineLenghtLabel.textAlignment = UIHorizontalAlignment.Left;
			lineLenghtLabel.text = "";
			lineLenghtLabel.width = 250;
			lineLenghtLabel.height = 25;
			lineLenghtLabel.prefix = "Line lenght: ";
			lineLenghtLabel.suffix = "m";
			lineLenghtLabel.name = "LineLenghtLabel";
			lineLenghtLabel.textScale = 0.8f;
			
			createUIElement<UILabel> (ref lineStopsLabel, lineInfoPanel.transform);			
			lineStopsLabel.autoSize = false;
			lineStopsLabel.relativePosition = new Vector3 (10f, 75f);			 
			lineStopsLabel.textAlignment = UIHorizontalAlignment.Left;
			lineStopsLabel.suffix = " Stops";
			lineStopsLabel.width = 250;
			lineStopsLabel.height = 25;			
			lineStopsLabel.name = "LineStopsLabel";
			lineStopsLabel.textScale = 0.8f;
			
			createUIElement<UILabel> (ref viagensEvitadasLabel, lineInfoPanel.transform);
			viagensEvitadasLabel.autoSize = false;
			viagensEvitadasLabel.relativePosition = new Vector3 (10f, 90f);			 
			viagensEvitadasLabel.textAlignment = UIHorizontalAlignment.Left;
			viagensEvitadasLabel.text = "";
			viagensEvitadasLabel.width = 250;
			viagensEvitadasLabel.height = 25;
			viagensEvitadasLabel.name = "AvoidedTravelsLabel";
			viagensEvitadasLabel.textScale = 0.8f;
			
			createUIElement<UILabel> (ref veiculosLinhaLabel, lineInfoPanel.transform);
			veiculosLinhaLabel.autoSize = false;
			veiculosLinhaLabel.relativePosition = new Vector3 (10f, 105f);			 
			veiculosLinhaLabel.textAlignment = UIHorizontalAlignment.Left;
			veiculosLinhaLabel.text = "";
			veiculosLinhaLabel.width = 250;
			veiculosLinhaLabel.height = 25;
			veiculosLinhaLabel.name = "VehiclesLineLabel";
			veiculosLinhaLabel.textScale = 0.8f;
			
			createUIElement<UILabel> (ref passageirosEturistasLabel, lineInfoPanel.transform);
			passageirosEturistasLabel.autoSize = false;
			passageirosEturistasLabel.relativePosition = new Vector3 (10f, 120f);			 
			passageirosEturistasLabel.textAlignment = UIHorizontalAlignment.Left;
			passageirosEturistasLabel.text = "";
			passageirosEturistasLabel.width = 350;
			passageirosEturistasLabel.height = 25;
			passageirosEturistasLabel.name = "TouristAndPassagersLabel";
			passageirosEturistasLabel.textScale = 0.8f;
			
			lineColorPicker = GameObject.Instantiate(PublicTransportWorldInfoPanel.FindObjectOfType<UIColorField> ().gameObject).GetComponent<UIColorField>();
			//				
			lineInfoPanel.AttachUIComponent(lineColorPicker.gameObject);
			lineColorPicker.name = "LineColorPicker";
			lineColorPicker.relativePosition = new Vector3 (15f, 35f);	
			lineColorPicker.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
			lineColorPicker.eventSelectedColorChanged += (UIComponent component, Color value) =>  {
				//					Log.debug("MUDOU");
				//					Log.debug(""+lineColorPicker.selectedColor);
				lineNumberLabel.color = lineColorPicker.selectedColor;					
				tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_color =lineColorPicker.selectedColor;
				tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_flags |= TransportLine.Flags.CustomColor;
			};
			
			//				UIButton lineColorButton = null;
			//				createUIElement<UIButton> (ref lineColorButton, lineInfoPanel.transform);	
			//				lineColorButton.relativePosition = new Vector3 (45f, 50f);
			//				lineColorButton.text = "ChangeColor";
			//				lineColorButton.width = 30;
			//				lineColorButton.height = 10;
			//				lineColorButton.textScale = 0.4f;
			//				initButton (lineColorButton, true, "ButtonMenu");	
			//				lineColorButton.name = "TransportLinesManagerCloseButton";
			//				lineColorButton.eventClick += (component, eventParam) => {
			//					lineColorPicker.component.Focus();
			//				};
			//				scrollPanel = UITemplateManager.Peek("ScrollablePanelTemplate") as UIPanel;
			//				lineInfoPanel.AttachUIComponent(scrollPanel.gameObject);
			//				scrollPanel.relativePosition = new Vector3(300,120);
			//				scrollPanel.width =340;
			//				scrollPanel.height = 200;
			//				scrollPanel.isVisible=true;
			
			createLineCharts ();
			
			
			UIButton deleteLine = null;
			createUIElement<UIButton> (ref deleteLine, lineInfoPanel.transform);	
			deleteLine.relativePosition = new Vector3 (10f, lineInfoPanel.height-40f);
			deleteLine.text = "Delete";
			deleteLine.width = 70;
			deleteLine.height = 30;
			initButton (deleteLine, true, "ButtonMenu");	
			deleteLine.name = "DeleteLineButton";
			deleteLine.color = new Color(1,0,0,1);
			deleteLine.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => {					
				TransportLine t = tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine];
				tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_lineNumber = 0;
				tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_flags |= TransportLine.Flags.Deleted | TransportLine.Flags.Hidden;
				tm.m_lines.m_buffer [(int)lineIdSelecionado.TransportLine].m_flags &= ~TransportLine.Flags.Created & ~TransportLine.Flags.Created;
				tm.m_lineCount--;
				t.ClearStops(lineIdSelecionado.TransportLine);
				
				closeLineInfo(component,eventParam);
			};
			
			UIButton voltarButton2 = null;
			createUIElement<UIButton> (ref voltarButton2, lineInfoPanel.transform);	
			voltarButton2.relativePosition = new Vector3 (lineInfoPanel.width - 250f, lineInfoPanel.height-40f);
			voltarButton2.text = "Close";
			voltarButton2.width = 240;
			voltarButton2.height = 30;
			initButton (voltarButton2, true, "ButtonMenu");	
			voltarButton2.name = "LineInfoCloseButton";
			voltarButton2.eventClick += closeLineInfo;
		}

		private void createLineCharts(){
			createUIElement<UIPanel> (ref agesChartPanel, lineInfoPanel.transform);
			agesChartPanel.relativePosition = new Vector3 (450f, 60f);
			agesChartPanel.width = 140;
			agesChartPanel.height = 70;
			agesChartPanel.name = "AgesChartPanel";
			agesChartPanel.autoLayout = false;
			agesChartPanel.useCenter = true;
			agesChartPanel.wrapLayout = false;
			
			UIPanel pieLegendPanel = null;
			createUIElement<UIPanel> (ref pieLegendPanel, agesChartPanel.transform);
			pieLegendPanel.relativePosition = new Vector3 (70f, 0f);
			pieLegendPanel.width = 70;
			pieLegendPanel.height = 70;
			pieLegendPanel.name = "AgesChartLegendPanel";
			pieLegendPanel.wrapLayout = false;
			pieLegendPanel.autoLayout = false;
			pieLegendPanel.useCenter = true;
			
			createUIElement<UIRadialChartAge> (ref agesChart, agesChartPanel.transform);		
			agesChart.spriteName = "PieChartWhiteBg";
			agesChart.tooltipLocaleID = "ZONEDBUILDING_AGECHART";			
			agesChart.relativePosition = new Vector3 (0, 0);		
			agesChart.width = 70;
			agesChart.height = 70;	
			agesChart.name = "AgesChart";
			Color32 criancaColor = new Color32(254,218,155,255);
			Color32 adolescenteColor = new Color32(205,239,145,255);
			Color32 jovemColor = new Color32(189,206,235,255);
			Color32 adultoColor = new Color32(255,162,162,255);
			Color32 idosoColor = new Color32(100,224,206,255);
			int y=0;
			criaFatiaELegenda(criancaColor,agesChart,pieLegendPanel,"ZONEDBUILDING_CHILDREN", 14*y++);
			criaFatiaELegenda(adolescenteColor,agesChart,pieLegendPanel,"ZONEDBUILDING_TEENS", 14*y++);
			criaFatiaELegenda(jovemColor,agesChart,pieLegendPanel,"ZONEDBUILDING_YOUNGS", 14*y++);
			criaFatiaELegenda(adultoColor,agesChart,pieLegendPanel,"ZONEDBUILDING_ADULTS", 14*y++);
			criaFatiaELegenda(idosoColor,agesChart,pieLegendPanel, "ZONEDBUILDING_SENIORS", 14*y++);
		}

		private void createMainView(){
			
			createUIElement<UIButton> (ref abrePainelButton, uiView.transform);
			abrePainelButton.relativePosition = new Vector3 (125.0f, 10.0f);
			abrePainelButton.tooltip = "Transport Lines (v"+TransportLinesManagerMod.version+")";
			abrePainelButton.width = 40;
			abrePainelButton.height = 40;
			abrePainelButton.name = "TransportLinesManagerButton";
			initButton (abrePainelButton, true, "ToolbarIconPublicTransport");	
			abrePainelButton.eventClick += abrirTelaTransportes;	
			//			abrePainelButton.normalBgSprite="IconTransportLineManager";

			createUIElement<UIPanel> (ref mainPanel, uiView.transform);
			mainPanel.Hide ();
			mainPanel.absolutePosition = new Vector3 (125.0f, 20.0f);
			mainPanel.width = 680;
			mainPanel.height = 530;
			mainPanel.color = new Color32 (16, 32, 48, 255);
			mainPanel.backgroundSprite = "InfoviewPanel";
			mainPanel.name = "TransportLinesManagerPanel";
			
			//
			UIButton voltarButton = null;
			createUIElement<UIButton> (ref voltarButton, mainPanel.transform);	
			voltarButton.relativePosition = new Vector3 (430f, 10f);
			voltarButton.text = "Close";
			voltarButton.width = 240;
			voltarButton.height = 30;
			initButton (voltarButton, true, "ButtonMenu");	
			voltarButton.name = "TransportLinesManagerCloseButton";
			voltarButton.eventClick += fecharTelaTransportes;
			
			
			createUIElement<UILabel> (ref titleLabel, mainPanel.transform);
			titleLabel.relativePosition = new Vector3 (15f, 15f);
			titleLabel.textAlignment = UIHorizontalAlignment.Center;
			titleLabel.text = "Transport Lines Manager v"+TransportLinesManagerMod.version+"";
			titleLabel.width = 630;
			titleLabel.height = 30;			
			titleLabel.name = "TransportLinesManagerLabelTitle";
		}

		private void createExtraOptionsView(){
						 
			createUIElement<UIPanel> (ref extraDataPanel, mainPanel.transform);
			extraDataPanel.relativePosition = new Vector3 (mainPanel.width, 0f);
			extraDataPanel.width = 580;
			extraDataPanel.height = 100;
			extraDataPanel.color = new Color32 (16, 32, 48, 255);
			extraDataPanel.backgroundSprite = "InfoviewPanel";
			extraDataPanel.name = "TLMExtraOptionsPanel";
			
			//
			UILabel title = null;
			createUIElement<UILabel> (ref title, extraDataPanel.transform);	
			title.relativePosition = new Vector3 (15f, 15f);
			title.text = "Extra Options";
			title.width = 240;
			title.height = 30;	
			title.name = "TLMExtraOptionsTitleLabel";
			title.eventClick += fecharTelaTransportes;

			createUIElement<UILabel> (ref busCapacity, extraDataPanel.transform);	
			busCapacity.relativePosition = new Vector3 (15f, 50f);
			busCapacity.prefix = "Bus Capacity: ";
			busCapacity.width = 240;
			busCapacity.height = 30;	
			busCapacity.name = "BusCapacityLabel";
			busCapacity.eventClick += fecharTelaTransportes;

			UIButton BusTo30Pas = null;
			createUIElement<UIButton> (ref BusTo30Pas, extraDataPanel.transform);	
			BusTo30Pas.relativePosition = new Vector3 (150f, 40f);
			BusTo30Pas.text = "To 30";
			BusTo30Pas.width = 40;
			BusTo30Pas.height = 30;
			initButton (BusTo30Pas, true, "ButtonMenu");	
			BusTo30Pas.name = "BusCapacityTo30Button";
			BusTo30Pas.eventClick += (component, eventParam) => {
				Singleton<BusAI>.instance.m_passengerCapacity = 30;
				setBusesCapacity(30);
			};
			
			UIButton BusTo60Pas = null;
			createUIElement<UIButton> (ref BusTo60Pas, extraDataPanel.transform);	
			BusTo60Pas.relativePosition = new Vector3 (195f, 40f);
			BusTo60Pas.text = "To 60"; 
			BusTo60Pas.width = 40;
			BusTo60Pas.height = 30;
			initButton (BusTo60Pas, true, "ButtonMenu");	
			BusTo60Pas.name = "BusCapacityTo60Button";
			BusTo60Pas.eventClick += (component, eventParam) => {
				Singleton<BusAI>.instance.m_passengerCapacity = 60;
				setBusesCapacity(60);
			};
			
			UIButton BusTo90Pas = null;
			createUIElement<UIButton> (ref BusTo90Pas, extraDataPanel.transform);	
			BusTo90Pas.relativePosition = new Vector3 (240f, 40f);
			BusTo90Pas.text = "To 90";
			BusTo90Pas.width = 40;
			BusTo90Pas.height = 30;
			initButton (BusTo90Pas, true, "ButtonMenu");	
			BusTo90Pas.name = "BusCapacityTo90Button";
			BusTo90Pas.eventClick += (component, eventParam) => {
				Singleton<BusAI>.instance.m_passengerCapacity = 90;
				setBusesCapacity(90);
			};

			UILabel warning = null;
			createUIElement<UILabel> (ref warning, extraDataPanel.transform);	
			warning.relativePosition = new Vector3 (295f, 50f);
			warning.text = "(Will need to reboot the depots manually!)";
			warning.width = 70;
			warning.height = 30;	
			warning.textScale = 0.8f;
			warning.textColor = new Color (1, 1, 0, 1);
			warning.name = "TLMExtraOptionsWarningLabel";


		}

		private void setBusesCapacity(int capacity){
			this.savedBusCapacity.value = capacity;
			Singleton<BusAI>.instance.m_passengerCapacity = capacity;
			for (int i=0; i<Singleton<VehicleManager>.instance.m_vehicles.m_buffer.Length;i++) {
				Vehicle v = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[i];
				if(v.Info.GetAI() is BusAI){
					BusAI b = (v.Info.GetAI() as BusAI);
					b.m_passengerCapacity = capacity;
					v.m_transferSize=(ushort)capacity;
				}
			}

		}

		private void updateExtraInfoBidings(){
			busCapacity.text = "" + Singleton<BusAI>.instance.m_passengerCapacity;
		}

		private void updateBidings(){
			ushort lineID = lineIdSelecionado.TransportLine;
			TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].Info;
			int turistas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_touristPassengers.m_averageCount;
			int residentes = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_residentPassengers.m_averageCount;
			if(residentes==0)residentes=1;
			int criancas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_childPassengers.m_averageCount;
			int adolescentes = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_teenPassengers.m_averageCount;
			int jovens = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_youngPassengers.m_averageCount;
			int adultos = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_adultPassengers.m_averageCount;
			int idosos = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_seniorPassengers.m_averageCount;
			int motoristas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_carOwningPassengers.m_averageCount;
			int veiculosLinha = Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].CountVehicles (lineID);
			int porcCriancas = (criancas*100/ residentes);
			int porcAdolescentes = (adolescentes*100/ residentes);
			int porcJovens =  (jovens*100/ residentes);
			int porcAdultos =(adultos*100/ residentes);
			int porcIdosos = (idosos*100/ residentes);
			int soma = porcCriancas + porcAdolescentes + porcJovens + porcAdultos + porcIdosos;
			if (soma != 0 && soma != 100)
			{
				porcAdultos = 100 - (porcCriancas + porcAdolescentes + porcJovens + porcIdosos);
			}
			agesChart.SetValues (new int[]
			                     {
				porcCriancas,
				porcAdolescentes,
				porcJovens,
				porcAdultos,
				porcIdosos
			});
			passageirosEturistasLabel.text= LocaleFormatter.FormatGeneric ("TRANSPORT_LINE_PASSENGERS", new object[]
			                                                               {
				residentes,
				turistas
			});
			veiculosLinhaLabel.text = LocaleFormatter.FormatGeneric ("TRANSPORT_LINE_VEHICLECOUNT", new object[]
			                                                         {
				veiculosLinha
			});
			int viagensSalvas = 0;
			int coeficienteViagens = 0;
			if (residentes + turistas != 0)
			{
				coeficienteViagens += criancas * 0;
				coeficienteViagens += adolescentes * 5;
				coeficienteViagens += jovens * ((15 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
				coeficienteViagens += adultos * ((20 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
				coeficienteViagens += idosos * ((10 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
			}
			if (coeficienteViagens != 0)
			{
				viagensSalvas = (int)(((long)motoristas * 10000L + (long)(coeficienteViagens >> 1)) / (long)coeficienteViagens);
				viagensSalvas = Mathf.Clamp (viagensSalvas, 0, 100);
			}
			viagensEvitadasLabel.text = LocaleFormatter.FormatGeneric ("TRANSPORT_LINE_TRIPSAVED", new object[]{
				viagensSalvas
			});
		}
	}
}