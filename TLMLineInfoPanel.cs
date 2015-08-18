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
	public class  TLMLineInfoPanel 
	{
		private TLMAgesChartPanel agesPanel;
		private TLMController m_controller;
		private TLMLinearMap m_linearMap;

		//line info	
		private  UIPanel lineInfoPanel;

		private  InstanceID m_lineIdSelecionado;
		private CameraController m_CameraController;
		private string lastLineName;
		private  UILabel lineLenghtLabel;			
		private  UILabel lineStopsLabel;			
		private  UITextField lineNumberLabel;		
		private  UILabel lineTransportIconTypeLabel;	
		private  UILabel viagensEvitadasLabel;
		private  UILabel passageirosEturistasLabel;
		private  UILabel veiculosLinhaLabel;
		private  UILabel autoNameLabel;
		private  UITextField lineNameField;
		private UIColorField lineColorPicker;	

		public Transform transform {
			get{
				return lineInfoPanel.transform;
			}
		}
		
		public GameObject gameObject{
			get{
				return lineInfoPanel.gameObject;
			}
		}
		public bool isVisible{
			get{
				return lineInfoPanel.isVisible;
			}
		}
		public TLMController controller{
			get{
				return m_controller;
			}
		}
		public TLMLinearMap linearMap{
			get{
				return m_linearMap;
			}
		}
		public InstanceID lineIdSelecionado{
			get{
				return m_lineIdSelecionado;
			}
		}
		
		public CameraController cameraController{
			get{
				return m_CameraController;
			}
		}

		public TLMLineInfoPanel(TLMController controller){
			this.m_controller = controller;
			GameObject gameObject = GameObject.FindGameObjectWithTag ("MainCamera");
			if (gameObject != null)
			{
				m_CameraController = gameObject.GetComponent<CameraController> ();
			}
			createInfoView ();
		}

		public void Show(){
			lineInfoPanel.Show();
		}
		
		public void Hide(){
			lineInfoPanel.Hide();
		}


		
		
		//ACOES
		private  void saveLineName(UITextField u){
			string value = u.text;
			
			TLMUtils.setLineName (m_lineIdSelecionado.TransportLine, value);
		}
		
		private  void saveLineNumber(UITextField u){
			String value = u.text;
			if (value.Length > 0) {
				bool numeroUsado = true;
				ushort num = UInt16.Parse(value);
				if(num>=10000 || num <1){
					lineNumberLabel.textColor = new Color(1,0,0,1);
					return;
				}
				ModoNomenclatura mn = ModoNomenclatura.Numero;
				switch	(m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine].Info.m_transportType){
				case TransportInfo.TransportType.Bus:
					numeroUsado = m_controller.mainPanel.onibus.Keys.Contains(num) &&  m_controller.mainPanel.onibus[num]!= m_lineIdSelecionado.TransportLine;
					mn = (ModoNomenclatura) m_controller.savedNomenclaturaOnibus.value;
					break;
					
				case TransportInfo.TransportType.Metro:
					numeroUsado = m_controller.mainPanel.metro.Keys.Contains(num) &&  m_controller.mainPanel.metro[num]!= m_lineIdSelecionado.TransportLine;
					mn = (ModoNomenclatura)  m_controller.savedNomenclaturaMetro.value;
					break;
					
				case TransportInfo.TransportType.Train:
					numeroUsado = m_controller.mainPanel.trens.Keys.Contains(num) &&  m_controller.mainPanel.trens[num]!= m_lineIdSelecionado.TransportLine;					
					mn = (ModoNomenclatura)  m_controller.savedNomenclaturaTrem.value;
					break;
				}
				if(numeroUsado){
					lineNumberLabel.textColor = new Color(1,0,0,1);
				}else{
					lineNumberLabel.textColor = new Color(1,1,1,1);
					m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine].m_lineNumber = num;
					m_linearMap.setLineNumberCircle(num,mn);
					autoNameLabel.text = m_linearMap.autoName;
				}
			} else {				
			}
		}
		


		private void createInfoView(){
			//line info painel
			
			TLMUtils.createUIElement<UIPanel> (ref lineInfoPanel, m_controller.mainRef.transform);		
			lineInfoPanel.Hide ();
			lineInfoPanel.relativePosition = new Vector3 (394.0f, 0.0f);
			lineInfoPanel.width = 650;
			lineInfoPanel.height = 290;
			lineInfoPanel.color = new Color32 (255, 255, 255, 255);
			lineInfoPanel.backgroundSprite = "MenuPanel2";
			lineInfoPanel.name = "LineInfoPanel";
			lineInfoPanel.autoLayoutPadding = new RectOffset (5, 5, 10, 10);
			lineInfoPanel.autoLayout = false;
			lineInfoPanel.useCenter = true;
			lineInfoPanel.wrapLayout = false;
			lineInfoPanel.canFocus= true;
			TLMUtils.createDragHandle (lineInfoPanel, lineInfoPanel,35f);
			lineInfoPanel.eventVisibilityChanged += (component, value) => {
				if(m_linearMap != null){
					m_linearMap.isVisible = value;
				}
			};
			
			
			
			TLMUtils.createUIElement<UILabel> (ref lineTransportIconTypeLabel, lineInfoPanel.transform);			
			lineTransportIconTypeLabel.autoSize = false;
			lineTransportIconTypeLabel.relativePosition = new Vector3 (10f, 12f);
			lineTransportIconTypeLabel.width = 30;
			lineTransportIconTypeLabel.height = 20;
			lineTransportIconTypeLabel.name = "LineTransportIcon";	
			lineTransportIconTypeLabel.clipChildren = true;
			TLMUtils.createDragHandle (lineTransportIconTypeLabel, lineInfoPanel);
			
			TLMUtils.createUIElement<UITextField> (ref lineNumberLabel, lineInfoPanel.transform);	
			lineNumberLabel.autoSize = false;
			lineNumberLabel.relativePosition = new Vector3 (80f, 3f);			 
			lineNumberLabel.horizontalAlignment = UIHorizontalAlignment.Center;
			lineNumberLabel.text = "";
			lineNumberLabel.width = 75;
			lineNumberLabel.height = 35;			
			lineNumberLabel.name = "LineNumberLabel";			
			lineNumberLabel.normalBgSprite = "EmptySprite";
			lineNumberLabel.textScale = 1.6f;
			lineNumberLabel.padding = new RectOffset (5, 5, 5, 5);
			lineNumberLabel.color = new Color (0, 0, 0, 1);
			TLMUtils.uiTextFieldDefaults (lineNumberLabel);
			lineNumberLabel.numericalOnly = true;
			lineNumberLabel.maxLength = 4;
			lineNumberLabel.eventLostFocus += ( component,  eventParam) => {
				saveLineNumber(lineNumberLabel);
				TransportLine t = m_controller.tm.m_lines.m_buffer [m_lineIdSelecionado.TransportLine];
				lineNumberLabel.text = "" + t.m_lineNumber;
			};
			lineNumberLabel.zOrder = 10;
			TLMUtils.createDragHandle (lineNumberLabel, lineInfoPanel);
			
			
			TLMUtils.createUIElement<UITextField> (ref lineNameField, lineInfoPanel.transform);
			lineNameField.autoSize = false;
			lineNameField.relativePosition = new Vector3 (190f, 10f);			 
			lineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
			lineNameField.text = "NOME";
			lineNameField.width = 450;
			lineNameField.height = 25;			
			lineNameField.name = "LineNameLabel";		
			lineNameField.maxLength = 256;
			lineNameField.textScale=1.5f;
			TLMUtils.uiTextFieldDefaults (lineNameField);
			lineNameField.eventGotFocus += ( component,  eventParam) => {
				lastLineName = lineNameField.text;
			};
			lineNameField.eventLostFocus += ( component,  eventParam) => {
				if(lastLineName != lineNameField.text){
					saveLineName(lineNameField);
				}
				lineNameField.text = m_controller.tm.GetLineName (m_lineIdSelecionado.TransportLine);
			};
			TLMUtils.createDragHandle (lineNameField, lineInfoPanel);
			
			TLMUtils.createUIElement<UILabel> (ref lineLenghtLabel, lineInfoPanel.transform);
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
			
			TLMUtils.createUIElement<UILabel> (ref lineStopsLabel, lineInfoPanel.transform);			
			lineStopsLabel.autoSize = false;
			lineStopsLabel.relativePosition = new Vector3 (10f, 75f);			 
			lineStopsLabel.textAlignment = UIHorizontalAlignment.Left;
			lineStopsLabel.suffix = " Stops";
			lineStopsLabel.width = 250;
			lineStopsLabel.height = 25;			
			lineStopsLabel.name = "LineStopsLabel";
			lineStopsLabel.textScale = 0.8f;
			
			TLMUtils.createUIElement<UILabel> (ref viagensEvitadasLabel, lineInfoPanel.transform);
			viagensEvitadasLabel.autoSize = false;
			viagensEvitadasLabel.relativePosition = new Vector3 (10f, 90f);			 
			viagensEvitadasLabel.textAlignment = UIHorizontalAlignment.Left;
			viagensEvitadasLabel.text = "";
			viagensEvitadasLabel.width = 250;
			viagensEvitadasLabel.height = 25;
			viagensEvitadasLabel.name = "AvoidedTravelsLabel";
			viagensEvitadasLabel.textScale = 0.8f;
			
			TLMUtils.createUIElement<UILabel> (ref veiculosLinhaLabel, lineInfoPanel.transform);
			veiculosLinhaLabel.autoSize = false;
			veiculosLinhaLabel.relativePosition = new Vector3 (10f, 105f);			 
			veiculosLinhaLabel.textAlignment = UIHorizontalAlignment.Left;
			veiculosLinhaLabel.text = "";
			veiculosLinhaLabel.width = 250;
			veiculosLinhaLabel.height = 25;
			veiculosLinhaLabel.name = "VehiclesLineLabel";
			veiculosLinhaLabel.textScale = 0.8f;

			TLMUtils.createUIElement<UILabel> (ref passageirosEturistasLabel, lineInfoPanel.transform);
			passageirosEturistasLabel.autoSize = false;
			passageirosEturistasLabel.relativePosition = new Vector3 (10f, 120f);			 
			passageirosEturistasLabel.textAlignment = UIHorizontalAlignment.Left;
			passageirosEturistasLabel.text = "";
			passageirosEturistasLabel.width = 350;
			passageirosEturistasLabel.height = 25;
			passageirosEturistasLabel.name = "TouristAndPassagersLabel";
			passageirosEturistasLabel.textScale = 0.8f;

			TLMUtils.createUIElement<UILabel> (ref autoNameLabel, lineInfoPanel.transform);
			autoNameLabel.autoSize = false; 
			autoNameLabel.relativePosition = new Vector3 (10f, 135f);			 
			autoNameLabel.textAlignment = UIHorizontalAlignment.Left;
			autoNameLabel.prefix = "Generated Auto Name: ";
			autoNameLabel.width = 350;
			autoNameLabel.height = 100;
			autoNameLabel.name = "AutoNameLabel";
			autoNameLabel.textScale = 0.8f;
			autoNameLabel.wordWrap = true;
			autoNameLabel.clipChildren = false;
			
			lineColorPicker = GameObject.Instantiate(PublicTransportWorldInfoPanel.FindObjectOfType<UIColorField> ().gameObject).GetComponent<UIColorField>();
			//				
			lineInfoPanel.AttachUIComponent(lineColorPicker.gameObject);
			lineColorPicker.name = "LineColorPicker";
			lineColorPicker.relativePosition = new Vector3 (50f, 10f);	
			lineColorPicker.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
			lineColorPicker.eventSelectedColorChanged += (UIComponent component, Color value) =>  {
				lineNumberLabel.color = value;	
				TLMUtils.setLineColor(m_lineIdSelecionado.TransportLine,value);
				m_linearMap.setLinearMapColor(value);				
			};			
			
			UIButton deleteLine = null;
			TLMUtils.createUIElement<UIButton> (ref deleteLine, lineInfoPanel.transform);	
			deleteLine.relativePosition = new Vector3 (10f, lineInfoPanel.height-40f);
			deleteLine.text = "Delete";
			deleteLine.width = 70;
			deleteLine.height = 30;
			TLMUtils.initButton (deleteLine, true, "ButtonMenu");	
			deleteLine.name = "DeleteLineButton";
			deleteLine.color = new Color(1,0,0,1);
			deleteLine.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => {					
				TransportLine t = m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine];
				m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine].m_lineNumber = 0;
				m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine].m_flags |= TransportLine.Flags.Deleted | TransportLine.Flags.Hidden;
				m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine].m_flags &= ~TransportLine.Flags.Created & ~TransportLine.Flags.Created;
				m_controller.tm.m_lineCount--;
				t.ClearStops(m_lineIdSelecionado.TransportLine);
				
				m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine] = default(TransportLine);
				closeLineInfo(component,eventParam);
			};
			UIButton voltarButton2 = null;
			TLMUtils.createUIElement<UIButton> (ref voltarButton2, lineInfoPanel.transform);	
			voltarButton2.relativePosition = new Vector3 (lineInfoPanel.width - 250f, lineInfoPanel.height-40f);
			voltarButton2.text = "Close";
			voltarButton2.width = 240;
			voltarButton2.height = 30;
			TLMUtils.initButton (voltarButton2, true, "ButtonMenu");	
			voltarButton2.name = "LineInfoCloseButton";
			voltarButton2.eventClick += closeLineInfo;

			UIButton autoName = null;
			TLMUtils.createUIElement<UIButton> (ref autoName, lineInfoPanel.transform);	
			autoName.relativePosition = new Vector3 (lineInfoPanel.width - 250f, lineInfoPanel.height-80f);
			autoName.text = "Use Auto Name";
			autoName.width = 240;
			autoName.height = 30;
			TLMUtils.initButton (autoName, true, "ButtonMenu");	
			autoName.name = "AutoNameButton";
			autoName.eventClick += (component, eventParam) => {
				lineNameField.text = m_linearMap.autoName;
				saveLineName(lineNameField);
			};


			agesPanel = new TLMAgesChartPanel (this);			
			m_linearMap = new TLMLinearMap(this);
		}


		

		
		
		public void updateBidings(){
			ushort lineID = m_lineIdSelecionado.TransportLine;
			TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID];
			TransportInfo info = tl.Info;
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
			agesPanel.SetValues (new int[]
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

		public  void closeLineInfo(UIComponent component, UIMouseEventParameter eventParam)
		{			
			TransportLine t = m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine];	
			
			t.m_flags |= TransportLine.Flags.Created;
			t.m_flags &= ~TransportLine.Flags.Selected &~TransportLine.Flags.Hidden;
			lineInfoPanel. Hide();	
			m_controller.mainPanel.Show ();
		}
		
		
		
		public   void openLineInfo(UIComponent component, UIMouseEventParameter eventParam){
			ushort lineID = (component as UIButtonLineInfo).lineID;			
			
			m_lineIdSelecionado = default(InstanceID);
			m_lineIdSelecionado.TransportLine = lineID;	
			//lines info
			float totalSize = 0f;
			for (int i = 0; i< m_controller.tm.m_lineCurves [(int)lineID].Length; i++) {
				Bezier3 bez = m_controller.tm.m_lineCurves [(int)lineID][i];
				totalSize +=  TLMUtils.calcBezierLenght (bez.a,bez.b,bez.c,bez.d,0.1f);
			}
			
			TransportLine t = m_controller.tm.m_lines.m_buffer [(int)lineID];
			int stopsCount = t.CountStops (lineID);
			lineLenghtLabel.text = string.Format ("{0:N2}",totalSize);
			lineNumberLabel.text = ""+t.m_lineNumber;
			lineNumberLabel.color = m_controller.tm.GetLineColor (lineID);
			lineStopsLabel.text = ""+stopsCount;
			lineNameField.text = m_controller.tm.GetLineName (lineID);
			lineTransportIconTypeLabel.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(t.Info.m_transportType);
			t.m_flags &= ~TransportLine.Flags.Created;
			t.m_flags |= TransportLine.Flags.Selected |TransportLine.Flags.Hidden;
			lineColorPicker.selectedColor = m_controller.tm.GetLineColor (lineID);
			

			m_linearMap.updateLine ();
			lineInfoPanel.Show ();
			m_controller.mainPanel.Hide ();

			autoNameLabel.text = m_linearMap.autoName;
		}

	}
}

