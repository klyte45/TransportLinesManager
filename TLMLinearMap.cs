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
	public class  TLMLinearMap
	{

		private TLMLineInfoPanel lineInfoPanel;

		private UILabel linearMapLineNumberFormat;
		private UILabel linearMapLineNumber;
		private UIPanel lineStationsPanel;		
		private UIPanel mainContainer;
		private string m_autoName;
		private ModoNomenclatura mn;
	
		public bool isVisible{
			get{
				return mainContainer.isVisible;
			}
			set{
				mainContainer.isVisible = value;
			}
		}

		
		public GameObject gameObject{
			get{
				return mainContainer.gameObject;
			}
		}

		public string autoName{
			get{				
				ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;		
				TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer [(int)lineID];
				return "["+TLMUtils.getString(mn,t.m_lineNumber)+"] " +m_autoName;
			}
		}

		public TLMLinearMap(TLMLineInfoPanel lip){
			lineInfoPanel = lip;
			createLineStationsLinearView ();
		}
		public void setLinearMapColor(Color c){
			linearMapLineNumberFormat.color = c;			
			linearMapLineNumber.textColor = TLMUtils.contrastColor (c);
			lineStationsPanel.color = c;
		}
		
		public void setLineNumberCircle(int num, ModoNomenclatura mn){
			linearMapLineNumber.text = TLMUtils.getString(mn,num);
			int lenght = linearMapLineNumber.text.Length;
			if (lenght==4) {
				linearMapLineNumber.textScale = 1f;				
				linearMapLineNumber.relativePosition = new Vector3 (0f,1f);
			} else if (lenght==3) {
				linearMapLineNumber.textScale = 1.25f;
				linearMapLineNumber.relativePosition = new Vector3 (0f,1.5f);
			} else if (lenght==2) {
				linearMapLineNumber.textScale = 1.75f;
				linearMapLineNumber.relativePosition = new Vector3 (-0.5f,0.5f);
			} else {
				linearMapLineNumber.textScale = 2.3f;
				linearMapLineNumber.relativePosition = new Vector3 (-0.5f,0f);
			}
		}

		public void updateLine(){	
			ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;		
			TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer [(int)lineID];
			int stopsCount = t.CountStops (lineID);
			setLinearMapColor(lineInfoPanel.controller.tm.GetLineColor (lineID));
			clearStations ();
			ItemClass.SubService ss = ItemClass.SubService.None;	
			if (t.Info.m_transportType == TransportInfo.TransportType.Train) {
				ss = ItemClass.SubService.PublicTransportTrain;
				linearMapLineNumberFormat.backgroundSprite = "TrainIcon";
				mn = (ModoNomenclatura) lineInfoPanel.controller.savedNomenclaturaTrem.value;
			} else if (t.Info.m_transportType == TransportInfo.TransportType.Metro) {
				ss = ItemClass.SubService.PublicTransportMetro;
				linearMapLineNumberFormat.backgroundSprite = "SubwayIcon";				
				mn = (ModoNomenclatura) lineInfoPanel.controller.savedNomenclaturaMetro.value;
			} else {
				linearMapLineNumberFormat.backgroundSprite = "BusIcon";				
				mn = (ModoNomenclatura) lineInfoPanel.controller.savedNomenclaturaOnibus.value;
			}
			setLineNumberCircle(t.m_lineNumber, mn);
			
			m_autoName = "";
			ushort[] stopBuildings = new ushort[stopsCount];
			MultiMap<ushort,Vector3> bufferToDraw = new MultiMap<ushort,Vector3>();
			int perfectSimetricLineStationsCount = (stopsCount+2)/2;
			bool simetric = t.Info.m_transportType != TransportInfo.TransportType.Bus ;
			int j;
			if (simetric) {
				
				NetManager nm = Singleton<NetManager>.instance;
				BuildingManager bm = Singleton<BuildingManager>.instance;
				//try to find the loop
				int middle = -1;
				for (j = -1; j<stopsCount/2; j++) {					
					int offsetL = (j+stopsCount)%stopsCount;
					int offsetH = (j+2)%stopsCount;					
					NetNode nn1 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetL)];
					NetNode nn2 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetH)];
					ushort buildingId1 = bm.FindBuilding (nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
					ushort buildingId2 = bm.FindBuilding (nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
					//					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
					//					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
					
					if(buildingId1==buildingId2){
						middle = j+1;
						break;
					}
				}
				//				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"middle="+middle);
				if(middle>=0){	
					for (j = 1; j<=stopsCount/2; j++) {
						int offsetL = (-j+middle+stopsCount)%stopsCount;
						int offsetH = (j+middle)%stopsCount;
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"t.GetStop (offsetL)="+t.GetStop (offsetH)+"|t.GetStop (offsetH)="+t.GetStop (offsetH));
						
						NetNode nn1 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetL)];
						NetNode nn2 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetH)];
						
						ushort buildingId1 = bm.FindBuilding (nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
						ushort buildingId2 = bm.FindBuilding (nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
						
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
						
						if(buildingId1!=buildingId2){
							simetric =false;
							break;
						}
					}				
				}else{
					simetric =false;
				}
				if(simetric){
					for (j = 0; j<=stopsCount/2; j++) {	
						string stationName = null;
						Vector3 local = getStation (t.GetStop (middle+j), ss, out stationName);
						addStationToLinearMap(stationName,local,j*25f+5f);
						if(j==0){
							m_autoName += stationName + " - ";
						}
						if(j==stopsCount/2){
							m_autoName += stationName;
						}
					}					
					lineStationsPanel.width = perfectSimetricLineStationsCount* 25f + 10f;
				}
			}
			if (!simetric) {
				DistrictManager dm = Singleton<DistrictManager>.instance;
				byte lastDistrict = 0;
				string stationName = null;
				Vector3 local;
				byte district;
				List<int> districtList = new List<int>();
				for (j = 0; j<stopsCount; j++) {	
					local  = getStation(t.GetStop(j), ss, out stationName);
					addStationToLinearMap (stationName,local,j * 25f + 5f);
					district = dm.GetDistrict(local);
					if((district != lastDistrict) && district != 0){
						districtList.Add(district);
					}
					if(district != 0){
						lastDistrict = district;
					}
				}			
				
				local = getStation(t.GetStop(0), ss, out stationName);
				addStationToLinearMap(stationName,local,j*25f+5f);
				lineStationsPanel.width = (j+1) * 25f + 10f;
				district = dm.GetDistrict(local);
				if((district != lastDistrict) && district != 0){
					districtList.Add(district);
				}
				int middle;
				int[] districtArray = districtList.ToArray();
				if(districtArray.Length ==1){
					m_autoName = (lineInfoPanel.controller.savedCircularOnSingleDistrict.value?"Circular ":"")+dm.GetDistrictName(districtArray[0]);
				}else if(TLMUtils.findSimetry(districtArray,out middle)){
					int firstIdx = middle;
					int lastIdx = middle + districtArray.Length/2;

					m_autoName = dm.GetDistrictName(districtArray[firstIdx%districtArray.Length])+" - "+dm.GetDistrictName(districtArray[lastIdx%districtArray.Length]);
					if(lastIdx-firstIdx>1){
						m_autoName += ", via ";
						for(int k = firstIdx+1; k<lastIdx;k++){
							m_autoName += dm.GetDistrictName(districtArray[k%districtArray.Length]);
							if(k+1!=lastIdx){
								m_autoName += ", ";
							}
						}
					}
				} else {
					bool inicio = true;
					foreach(int i in districtArray){
						m_autoName += (inicio ? "" :" - ") + dm.GetDistrictName(i);
						inicio = false;
					}
				}
			}
		}


		
		private void clearStations(){			
			UnityEngine.Object.Destroy (lineStationsPanel.gameObject);
			createLineStationsPanel() ;
		}	
		
		private void createLineStationsLinearView(){
			TLMUtils.createUIElement<UIPanel> (ref mainContainer, lineInfoPanel.transform);			
			mainContainer.absolutePosition = new Vector3 (2f, lineInfoPanel.controller.uiView.fixedHeight-180f);
			mainContainer.name = "LineStationsLinearView";
			mainContainer.height = 50;
			mainContainer.autoSize = true;
			
			TLMUtils.createUIElement<UILabel> (ref linearMapLineNumberFormat, mainContainer.transform);
			linearMapLineNumberFormat.autoSize = false;
			linearMapLineNumberFormat.width = 50;
			linearMapLineNumberFormat.height = 50;
			linearMapLineNumberFormat.color = new Color (1, 0, 0, 1);
			linearMapLineNumberFormat.pivot = UIPivotPoint.MiddleLeft;
			linearMapLineNumberFormat.textAlignment = UIHorizontalAlignment.Center;
			linearMapLineNumberFormat.verticalAlignment = UIVerticalAlignment.Middle;
			linearMapLineNumberFormat.name = "LineFormat";
			linearMapLineNumberFormat.relativePosition = new Vector3 (0f,0f);
			linearMapLineNumberFormat.atlas = TLMController.taLineNumber;
			TLMUtils.createDragHandle (linearMapLineNumberFormat, mainContainer);
			
			TLMUtils.createUIElement<UILabel> (ref linearMapLineNumber, linearMapLineNumberFormat.transform);
			linearMapLineNumber.autoSize = false;
			linearMapLineNumber.autoHeight = false;
			linearMapLineNumber.width = linearMapLineNumberFormat.width;
			linearMapLineNumber.pivot = UIPivotPoint.MiddleCenter;
			linearMapLineNumber.textAlignment = UIHorizontalAlignment.Center;
			linearMapLineNumber.verticalAlignment = UIVerticalAlignment.Middle;
			linearMapLineNumber.name = "LineNumber";
			
			linearMapLineNumber.width = 50;
			linearMapLineNumber.height = 50;
			linearMapLineNumber.relativePosition = new Vector3 (-0.5f,0.5f);
			TLMUtils.createDragHandle (linearMapLineNumber, mainContainer);
			
			createLineStationsPanel ();
		}
		
		private void createLineStationsPanel(){
			
			TLMUtils.createUIElement<UIPanel> (ref lineStationsPanel, mainContainer.transform);
			lineStationsPanel.width = 140;
			lineStationsPanel.height = 30;
			lineStationsPanel.name = "LineStationsPanel";
			lineStationsPanel.autoLayout = false;
			lineStationsPanel.useCenter = true;
			lineStationsPanel.wrapLayout = false;
			lineStationsPanel.backgroundSprite = "GenericPanelWhite";
			lineStationsPanel.pivot = UIPivotPoint.MiddleLeft;
			lineStationsPanel.relativePosition = new Vector3 (60f,10f);
			lineStationsPanel.color = lineInfoPanel.controller.tm.GetLineColor (lineInfoPanel.lineIdSelecionado.TransportLine);
		}
		
		private void addStationToLinearMap(string stationName, Vector3 location,float offsetX){	
						
			
			UIButton stationButton = null;			
			TLMUtils.createUIElement<UIButton> (ref stationButton, lineStationsPanel.transform);
			stationButton.relativePosition = new Vector3 (offsetX, 15f);
			stationButton.width = 20;
			stationButton.height = 20;
			stationButton.name = "Station ["+stationName+"]";
			TLMUtils.initButton (stationButton, true, "IconPolicyBaseCircle");
			
			UILabel stationLabel = null;
			TLMUtils.createUIElement<UILabel> (ref stationLabel, stationButton.transform);
			stationLabel.autoSize = true;
			stationLabel.width = 20;
			stationLabel.height = 20;
			stationLabel.useOutline = true;
			stationLabel.pivot = UIPivotPoint.MiddleLeft;
			stationLabel.textAlignment = UIHorizontalAlignment.Center;
			stationLabel.verticalAlignment = UIVerticalAlignment.Middle;
			stationLabel.name = "Station ["+stationName+"] Name";
			stationLabel.relativePosition = new Vector3 (23f, -13f);
			stationLabel.text = stationName;
			
			stationButton.gameObject.transform.localPosition = new Vector3 (0, 0, 0);
			stationButton.gameObject.transform.localEulerAngles = new Vector3 (0, 0, 45);
			stationButton.eventClick += (component, eventParam) => {				
				lineInfoPanel.cameraController.SetTarget (lineInfoPanel.lineIdSelecionado, location, false);
				lineInfoPanel.cameraController.ClearTarget();
				
			};
			
		}

		Vector3 getStation (uint stopId, ItemClass.SubService ss, out string stationName)
		{
			NetManager nm = Singleton<NetManager>.instance;
			BuildingManager bm = Singleton<BuildingManager>.instance;
			NetNode nn = nm.m_nodes.m_buffer [(int)stopId];
			ushort buildingId;
			if (ss != ItemClass.SubService.None) {
				buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
			}
			else {
				buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Monument, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Beautification, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Government, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.HealthCare, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.FireDepartment, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PoliceDepartment, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Tourism, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Education, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Garbage, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Office, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Commercial, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Industrial, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Water, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Electricity, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
				}
				if (buildingId == 0) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Residential, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
				}
			}
			Vector3 location = nn.m_position;
			if (buildingId > 0) {
				Building b = bm.m_buildings.m_buffer [buildingId];
				InstanceID iid = default(InstanceID);
				iid.Building = buildingId;
				iid.TransportLine = lineInfoPanel.lineIdSelecionado.TransportLine;
				stationName = bm.GetBuildingName (buildingId, iid);
			}
			else {
				DistrictManager dm = Singleton<DistrictManager>.instance;
				int dId = dm.GetDistrict (location);
				if (dId > 0) {
					District d = dm.m_districts.m_buffer [dId];
					stationName = "[D] " + dm.GetDistrictName (dId);
				}
				else {
					stationName = "[X=" + location.x + "|Y=" + location.y + "|Z=" + location.z + "]";
				}
			}
			return location;
		}
	}
}

