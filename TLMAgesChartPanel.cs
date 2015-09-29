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
	public class TLMAgesChartPanel	{			
		private  UIPanel agesChartPanel;
		private  UIRadialChartAge agesChart;	
		private TLMLineInfoPanel lineInfoPanel;

		public TLMAgesChartPanel(TLMLineInfoPanel lineInfoPanel){
			this.lineInfoPanel = lineInfoPanel;
			createLineCharts ();
		}

		public void SetValues(int[] values){
			agesChart.SetValues (values);
		}

		private void createLineCharts(){
			TLMUtils.createUIElement<UIPanel> (ref agesChartPanel, lineInfoPanel.transform);
			agesChartPanel.relativePosition = new Vector3 (450f, 60f);
			agesChartPanel.width = 140;
			agesChartPanel.height = 70;
			agesChartPanel.name = "AgesChartPanel";
			agesChartPanel.autoLayout = false;
			agesChartPanel.useCenter = true;
			agesChartPanel.wrapLayout = false;
			
			UIPanel pieLegendPanel = null;
			TLMUtils.createUIElement<UIPanel> (ref pieLegendPanel, agesChartPanel.transform);
			pieLegendPanel.relativePosition = new Vector3 (70f, 0f);
			pieLegendPanel.width = 70;
			pieLegendPanel.height = 70;
			pieLegendPanel.name = "AgesChartLegendPanel";
			pieLegendPanel.wrapLayout = false;
			pieLegendPanel.autoLayout = false;
			pieLegendPanel.useCenter = true;
			
			TLMUtils.createUIElement<UIRadialChartAge> (ref agesChart, agesChartPanel.transform);		
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
		private  void criaFatiaELegenda(Color c, UIRadialChartAge chart, UIPanel legendPanel, string localeID, float offsetY){
			chart.AddSlice (c, c);
			UIPanel legendItemContainer = null;
			TLMUtils.createUIElement<UIPanel> (ref legendItemContainer, legendPanel.transform);
			legendItemContainer.width = legendPanel.width;
			legendItemContainer.relativePosition = new Vector3 (0f, offsetY);
			legendItemContainer.name = "LegendItem";
			legendItemContainer.autoLayout = false ;
			legendItemContainer.useCenter  = true  ;
			legendItemContainer.wrapLayout = false ;
			legendItemContainer.height = 20;
			UILabel legendColor = null;
			TLMUtils.createUIElement<UILabel> (ref legendColor, legendItemContainer.transform);
			legendColor.backgroundSprite = "EmptySprite";
			legendColor.width = 10;
			legendColor.height = 10;
			legendColor.relativePosition = new Vector3 (0,0);
			legendColor.color = c;
			UILabel legendName = null;
			TLMUtils.createUIElement<UILabel> (ref legendName, legendItemContainer.transform);
			legendName.textAlignment = UIHorizontalAlignment.Right;
			legendName.width = legendItemContainer.width - 10;
			legendName.localeID = localeID;
			legendName.textScale = 0.6f;
			legendName.relativePosition = new Vector3 (15f, 2f);
			legendName.verticalAlignment = UIVerticalAlignment.Middle;
		}
	}
}

