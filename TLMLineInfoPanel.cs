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
using Klyte.Extensions;

namespace Klyte.TransportLinesManager
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
		private  UIDropDown linePrefixDropDown;
		private  UILabel lineTransportIconTypeLabel;
		private  UILabel viagensEvitadasLabel;
		private  UILabel passageirosEturistasLabel;
		private  UILabel veiculosLinhaLabel;
		private  UILabel autoNameLabel;
		private 	UIDropDown lineTime;
		private  UITextField lineNameField;
		private UIColorField lineColorPicker;
		private AsyncAction daytimeChange;

		public Transform transform {
			get {
				return lineInfoPanel.transform;
			}
		}
		
		public GameObject gameObject {
			get {
				try {						
					return lineInfoPanel.gameObject;
				} catch (Exception e) {
					return null;
				}
			}
		}

		public bool isVisible {
			get {
				return lineInfoPanel.isVisible;
			}
		}

		public TLMController controller {
			get {
				return m_controller;
			}
		}

		public TLMLinearMap linearMap {
			get {
				return m_linearMap;
			}
		}

		public InstanceID lineIdSelecionado {
			get {
				return m_lineIdSelecionado;
			}
		}
		
		public CameraController cameraController {
			get {
				return m_CameraController;
			}
		}

		public TLMLineInfoPanel (TLMController controller)
		{
			this.m_controller = controller;
			GameObject gameObject = GameObject.FindGameObjectWithTag ("MainCamera");
			if (gameObject != null) {
				m_CameraController = gameObject.GetComponent<CameraController> ();
			}
			createInfoView ();
		}

		public void Show ()
		{
			if (!GameObject.Find ("InfoViewsPanel").GetComponent<UIPanel> ().isVisible) {
				GameObject.Find ("InfoViewsPanel").GetComponent<UIPanel> ().isVisible = true;
			}
			lineInfoPanel.Show ();
		}
		
		public void Hide ()
		{
			lineInfoPanel.Hide ();
		}


		
		
		//ACOES
		private  void saveLineName (UITextField u)
		{
			string value = u.text;
			
			TLMUtils.setLineName (m_lineIdSelecionado.TransportLine, value);
		}

		private bool isNumeroUsado (int numLinha, TransportInfo.TransportType tipo)
		{
			bool numeroUsado = true;
			switch (tipo) {
			case TransportInfo.TransportType.Bus:
				numeroUsado = m_controller.mainPanel.onibus.Keys.Contains (numLinha) && m_controller.mainPanel.onibus [numLinha] != m_lineIdSelecionado.TransportLine;
				break;
				
			case TransportInfo.TransportType.Metro:
				numeroUsado = m_controller.mainPanel.metro.Keys.Contains (numLinha) && m_controller.mainPanel.metro [numLinha] != m_lineIdSelecionado.TransportLine;
				break;
				
			case TransportInfo.TransportType.Train:
				numeroUsado = m_controller.mainPanel.trens.Keys.Contains (numLinha) && m_controller.mainPanel.trens [numLinha] != m_lineIdSelecionado.TransportLine;		
				break;
			}
			return numeroUsado;
		}
		
		private  void saveLineNumber (UIComponent c, object v)
		{
			saveLineNumber ();
		}

		private  void saveLineNumber (UIComponent c, int v)
		{
			saveLineNumber ();
		}

		private  void saveLineNumber ()
		{
			String value = "0" + lineNumberLabel.text;
			int valPrefixo = linePrefixDropDown.selectedIndex;
			ModoNomenclatura mn;
			ModoNomenclatura mnPrefixo;
			Separador sep;
			bool zeros;
			var tipoLinha = m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine].Info.m_transportType;
			TLMLineUtils.GetLineNumberRules (out mn, out mnPrefixo, out sep,out zeros, tipoLinha);
			ushort num = ushort.Parse (value);
			if (mnPrefixo != ModoNomenclatura.Nenhum) {
				num = (ushort)(valPrefixo * 1000 + (num % 1000));
			}
			bool numeroUsado = isNumeroUsado (num, tipoLinha);
			if (num < 1) {
				lineNumberLabel.textColor = new Color (1, 0, 0, 1);
				return;
			}

			if (numeroUsado) {
				lineNumberLabel.textColor = new Color (1, 0, 0, 1);
			} else {
				lineNumberLabel.textColor = new Color (1, 1, 1, 1);
				m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine].m_lineNumber = num;
				m_linearMap.setLineNumberCircle (num, mnPrefixo, sep, mn,zeros);
				autoNameLabel.text = m_linearMap.autoName;
					
				if (mnPrefixo != ModoNomenclatura.Nenhum) {
					lineNumberLabel.text = (num % 1000).ToString ();
					linePrefixDropDown.selectedIndex = (num / 1000);
				} else {
					lineNumberLabel.text = (num % 10000).ToString ();
				}
			}
		}



		private void createInfoView ()
		{
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
			lineInfoPanel.canFocus = true;
			TLMUtils.createDragHandle (lineInfoPanel, lineInfoPanel, 35f);
			lineInfoPanel.eventVisibilityChanged += (component, value) => {
				if (m_linearMap != null) {
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

			GameObject lpddgo = GameObject.Instantiate (UITemplateManager.GetAsGameObject (UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel> ().Find<UIDropDown> ("Dropdown").gameObject);
			linePrefixDropDown = lpddgo.GetComponent<UIDropDown> ();
			lineInfoPanel.AttachUIComponent (linePrefixDropDown.gameObject);
			linePrefixDropDown.isLocalized = false;
			linePrefixDropDown.autoSize = false;
			linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;
			linePrefixDropDown.text = "";
			linePrefixDropDown.width = 40;
			linePrefixDropDown.height = 35;		
			linePrefixDropDown.name = "LinePrefixDropDown";		
			linePrefixDropDown.textScale = 1.6f;
			linePrefixDropDown.itemHeight = 35;
			linePrefixDropDown.itemPadding = new RectOffset (2, 2, 2, 2);
			linePrefixDropDown.textFieldPadding = new RectOffset (2, 2, 2, 2);
			linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
			linePrefixDropDown.relativePosition = new Vector3 (70f, 3f);

			
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
			lineNumberLabel.eventLostFocus += saveLineNumber;
			lineNumberLabel.zOrder = 10;
			
			
			TLMUtils.createUIElement<UITextField> (ref lineNameField, lineInfoPanel.transform);
			lineNameField.autoSize = false;
			lineNameField.relativePosition = new Vector3 (190f, 10f);			 
			lineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
			lineNameField.text = "NOME";
			lineNameField.width = 450;
			lineNameField.height = 25;			
			lineNameField.name = "LineNameLabel";		
			lineNameField.maxLength = 256;
			lineNameField.textScale = 1.5f;
			TLMUtils.uiTextFieldDefaults (lineNameField);
			lineNameField.eventGotFocus += ( component,  eventParam) => {
				lastLineName = lineNameField.text;
			};
			lineNameField.eventLostFocus += ( component,  eventParam) => {
				if (lastLineName != lineNameField.text) {
					saveLineName (lineNameField);
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
			
			lineColorPicker = GameObject.Instantiate (PublicTransportWorldInfoPanel.FindObjectOfType<UIColorField> ().gameObject).GetComponent<UIColorField> ();
			//				
			lineInfoPanel.AttachUIComponent (lineColorPicker.gameObject);
			lineColorPicker.name = "LineColorPicker";
			lineColorPicker.relativePosition = new Vector3 (50f, 10f);	

			lineColorPicker.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
			lineColorPicker.eventSelectedColorChanged += (UIComponent component, Color value) => {				
				TLMUtils.setLineColor (m_lineIdSelecionado.TransportLine, value);
				updateLineUI (value);				
			};

			lineTime = UIHelperExtension.CloneBasicDropDown ("Line Operation", new string[] {
				"Day & Night",
				"Day Only",
				"Night Only",
				"Disable (without delete)"
			}, changeLineTime, lineInfoPanel);
			lineTime.parent.relativePosition = new Vector3 (50f, 170f);
			
			UIButton deleteLine = null;
			TLMUtils.createUIElement<UIButton> (ref deleteLine, lineInfoPanel.transform);	
			deleteLine.relativePosition = new Vector3 (10f, lineInfoPanel.height - 40f);
			deleteLine.text = "Delete";
			deleteLine.width = 70;
			deleteLine.height = 30;
			TLMUtils.initButton (deleteLine, true, "ButtonMenu");	
			deleteLine.name = "DeleteLineButton";
			deleteLine.color = new Color (1, 0, 0, 1);
			deleteLine.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => {
				Singleton<SimulationManager>.instance.AddAction (delegate {
					Singleton<TransportManager>.instance.ReleaseLine (m_lineIdSelecionado.TransportLine);
				});
				closeLineInfo (component, eventParam);
			};
			UIButton voltarButton2 = null;
			TLMUtils.createUIElement<UIButton> (ref voltarButton2, lineInfoPanel.transform);	
			voltarButton2.relativePosition = new Vector3 (lineInfoPanel.width - 250f, lineInfoPanel.height - 40f);
			voltarButton2.text = "Close";
			voltarButton2.width = 240;
			voltarButton2.height = 30;
			TLMUtils.initButton (voltarButton2, true, "ButtonMenu");	
			voltarButton2.name = "LineInfoCloseButton";
			voltarButton2.eventClick += closeLineInfo;

			UIButton autoName = null;
			TLMUtils.createUIElement<UIButton> (ref autoName, lineInfoPanel.transform);	
			autoName.relativePosition = new Vector3 (lineInfoPanel.width - 250f, lineInfoPanel.height - 80f);
			autoName.text = "Use Auto Name";
			autoName.width = 240;
			autoName.height = 30;
			TLMUtils.initButton (autoName, true, "ButtonMenu");	
			autoName.name = "AutoNameButton";
			autoName.eventClick += (component, eventParam) => {
				lineNameField.text = m_linearMap.autoName;
				saveLineName (lineNameField);
			};

			UIButton autoColor = null;
			TLMUtils.createUIElement<UIButton> (ref autoColor, lineInfoPanel.transform);	
			autoColor.relativePosition = new Vector3 (lineInfoPanel.width - 250f, lineInfoPanel.height - 120f);
			autoColor.text = "Pick color from palette";
			autoColor.tooltip = "Redefine the line color using palette settings; Line number based";
			autoColor.width = 240;
			autoColor.height = 30;
			TLMUtils.initButton (autoColor, true, "ButtonMenu");	
			autoColor.name = "AutoNameButton";
			autoColor.eventMouseUp += (component, eventParam) => {
				lineColorPicker.selectedColor = m_controller.AutoColor (m_lineIdSelecionado.TransportLine);	
				updateLineUI (lineColorPicker.selectedColor);
			};


			agesPanel = new TLMAgesChartPanel (this);			
			m_linearMap = new TLMLinearMap (this);
		}

		private void updateLineUI (Color color)
		{
			lineNumberLabel.color = color;	
			m_linearMap.setLinearMapColor (color);	
		}
		
		public void updateBidings ()
		{
			ushort lineID = m_lineIdSelecionado.TransportLine;
			TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID];
			TransportInfo info = tl.Info;
			int turistas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_touristPassengers.m_averageCount;
			int residentes = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_residentPassengers.m_averageCount;
			if (residentes == 0)
				residentes = 1;
			int criancas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_childPassengers.m_averageCount;
			int adolescentes = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_teenPassengers.m_averageCount;
			int jovens = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_youngPassengers.m_averageCount;
			int adultos = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_adultPassengers.m_averageCount;
			int idosos = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_seniorPassengers.m_averageCount;
			int motoristas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].m_passengers.m_carOwningPassengers.m_averageCount;
			int veiculosLinha = Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].CountVehicles (lineID);
			int porcCriancas = (criancas * 100 / residentes);
			int porcAdolescentes = (adolescentes * 100 / residentes);
			int porcJovens = (jovens * 100 / residentes);
			int porcAdultos = (adultos * 100 / residentes);
			int porcIdosos = (idosos * 100 / residentes);
			int soma = porcCriancas + porcAdolescentes + porcJovens + porcAdultos + porcIdosos;
			if (soma != 0 && soma != 100) {
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
			passageirosEturistasLabel.text = LocaleFormatter.FormatGeneric ("TRANSPORT_LINE_PASSENGERS", new object[]
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
			if (residentes + turistas != 0) {
				coeficienteViagens += criancas * 0;
				coeficienteViagens += adolescentes * 5;
				coeficienteViagens += jovens * ((15 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
				coeficienteViagens += adultos * ((20 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
				coeficienteViagens += idosos * ((10 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
			}
			if (coeficienteViagens != 0) {
				viagensSalvas = (int)(((long)motoristas * 10000L + (long)(coeficienteViagens >> 1)) / (long)coeficienteViagens);
				viagensSalvas = Mathf.Clamp (viagensSalvas, 0, 100);
			}
			viagensEvitadasLabel.text = LocaleFormatter.FormatGeneric ("TRANSPORT_LINE_TRIPSAVED", new object[]{
				viagensSalvas
			});

			if (daytimeChange != null && daytimeChange.completedOrFailed) {
				linearMap.updateLine ();
				daytimeChange = null;
			}
		}

		public  void closeLineInfo (UIComponent component, UIMouseEventParameter eventParam)
		{			
			TransportLine t = m_controller.tm.m_lines.m_buffer [(int)m_lineIdSelecionado.TransportLine];
			Hide ();	
			m_controller.mainPanel.Show ();
		}
		
		public   void openLineInfo (UIComponent component, UIMouseEventParameter eventParam)
		{
			ushort lineID = (component as UIButtonLineInfo).lineID;	
			openLineInfo (lineID);

		}

		public   void openLineInfo (ushort lineID)
		{
			WorldInfoPanel.HideAllWorldInfoPanels ();
			linePrefixDropDown.eventSelectedIndexChanged -= saveLineNumber;
			lineNumberLabel.eventLostFocus -= saveLineNumber;

			m_lineIdSelecionado = default(InstanceID);
			m_lineIdSelecionado.TransportLine = lineID;	
			//lines info
			float totalSize = 0f;
			for (int i = 0; i< m_controller.tm.m_lineCurves [(int)lineID].Length; i++) {
				Bezier3 bez = m_controller.tm.m_lineCurves [(int)lineID] [i];
				totalSize += TLMUtils.calcBezierLenght (bez.a, bez.b, bez.c, bez.d, 0.1f);
			}
			
			TransportLine t = m_controller.tm.m_lines.m_buffer [(int)lineID];	
			ushort lineNumber = t.m_lineNumber;

			ModoNomenclatura mnPrefixo = ModoNomenclatura.Nenhum;
			var tipoLinha = t.Info.m_transportType;
			switch (tipoLinha) {
			case TransportInfo.TransportType.Bus:
				mnPrefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaOnibusPrefixo.value;
				break;
				
			case TransportInfo.TransportType.Metro:
				mnPrefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaMetroPrefixo.value;
				break;
				
			case TransportInfo.TransportType.Train:				
				mnPrefixo = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaTremPrefixo.value;
				break;
			}

			if (mnPrefixo != ModoNomenclatura.Nenhum) {
				lineNumberLabel.text = (lineNumber % 1000).ToString ();
				lineNumberLabel.relativePosition = new Vector3 (110f, 3f);
				lineNumberLabel.width = 55;
				linePrefixDropDown.enabled = false;
				linePrefixDropDown.items = TLMUtils.getStringOptionsForPrefix (mnPrefixo);
				linePrefixDropDown.selectedIndex = lineNumber / 1000;
				linePrefixDropDown.enabled = true;
				lineNumberLabel.maxLength = 3;
			} else {
				lineNumberLabel.text = (lineNumber).ToString ();
				lineNumberLabel.relativePosition = new Vector3 (80f, 3f);
				lineNumberLabel.width = 75;
				lineNumberLabel.maxLength = 4;
				linePrefixDropDown.enabled = false;
			}


			int stopsCount = t.CountStops (lineID);
			lineLenghtLabel.text = string.Format ("{0:N2}", totalSize);
			lineNumberLabel.color = m_controller.tm.GetLineColor (lineID);
			lineStopsLabel.text = "" + stopsCount;
			lineNameField.text = m_controller.tm.GetLineName (lineID);
			lineTransportIconTypeLabel.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon (t.Info.m_transportType);
			lineColorPicker.selectedColor = m_controller.tm.GetLineColor (lineID);

			bool day, night;
			t.GetActive (out day, out night);
			if (day && night) {
				lineTime.selectedIndex = 0;
			} else if (day) {
				lineTime.selectedIndex = 1;
			} else if (night) {
				lineTime.selectedIndex = 2;
			} else {
				lineTime.selectedIndex = 3;
			}

			m_linearMap.updateLine ();
			Show ();
			m_controller.mainPanel.Hide ();

			autoNameLabel.text = m_linearMap.autoName;

			
			linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
			lineNumberLabel.eventLostFocus += saveLineNumber;
		}

		private void changeLineTime (int selection)
		{
			daytimeChange = Singleton<SimulationManager>.instance.AddAction (delegate {				
				ushort lineID = m_lineIdSelecionado.TransportLine;
				switch (selection) {
				case 0:
					Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].SetActive (true, true);
					break;
				case 1:
					Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].SetActive (true, false);
					break;				
				case 2:				
					Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].SetActive (false, true);
					break;			
				case 3:
					Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineID].SetActive (false, false);
					break;
				}
			});

		}
	}
}

