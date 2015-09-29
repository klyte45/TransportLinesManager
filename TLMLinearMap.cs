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
	public class  TLMLinearMap
	{

		private TLMLineInfoPanel lineInfoPanel;
		private UILabel linearMapLineNumberFormat;
		private UILabel linearMapLineNumber;
		private UILabel linearMapLineTime;
		private UIPanel lineStationsPanel;
		private UIPanel mainContainer;
		private string m_autoName;
		private ModoNomenclatura mn;
	
		public bool isVisible {
			get {
				return mainContainer.isVisible;
			}
			set {
				mainContainer.isVisible = value;
			}
		}
		
		public GameObject gameObject {
			get {
				try {
					return mainContainer.gameObject;
				} catch (Exception e) {
					return null;
				}

			}
		}

		public string autoName {
			get {				
				ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;		
				TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer [(int)lineID];
				return "[" + TLMUtils.getString (mn, t.m_lineNumber) + "] " + m_autoName;
			}
		}

		public TLMLinearMap (TLMLineInfoPanel lip)
		{
			lineInfoPanel = lip;
			createLineStationsLinearView ();
		}

		public void setLinearMapColor (Color c)
		{
			linearMapLineNumberFormat.color = c;			
			linearMapLineNumber.textColor = TLMUtils.contrastColor (c);
			lineStationsPanel.color = c;
		}
		
		public void setLineNumberCircle (int num, ModoNomenclatura mn)
		{
			setLineNumberCircleOnRef (num, mn, linearMapLineNumber);
		}

		public void setLineNumberCircleOnRef (int num, ModoNomenclatura mn, UILabel reference)
		{
			reference.text = TLMUtils.getString (mn, num);
			int lenght = reference.text.Length;
			if (lenght == 4) {
				reference.textScale = 1f;				
				reference.relativePosition = new Vector3 (0f, 1f);
			} else if (lenght == 3) {
				reference.textScale = 1.25f;
				reference.relativePosition = new Vector3 (0f, 1.5f);
			} else if (lenght == 2) {
				reference.textScale = 1.75f;
				reference.relativePosition = new Vector3 (-0.5f, 0.5f);
			} else {
				reference.textScale = 2.3f;
				reference.relativePosition = new Vector3 (-0.5f, 0f);
			}
		}

		private ItemClass.SubService setFormatBgByType (TransportLine line, out String bgSprite, out ModoNomenclatura nomenclatura)
		{
			if (line.Info.m_transportType == TransportInfo.TransportType.Train) {
				bgSprite = "TrainIcon";
				nomenclatura = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaTrem.value;
				return ItemClass.SubService.PublicTransportTrain;
			} else if (line.Info.m_transportType == TransportInfo.TransportType.Metro) {
				bgSprite = "SubwayIcon";				
				nomenclatura = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaMetro.value;
				return ItemClass.SubService.PublicTransportMetro;
			} else {
				bgSprite = "BusIcon";				
				nomenclatura = (ModoNomenclatura)TransportLinesManagerMod.savedNomenclaturaOnibus.value;
				return ItemClass.SubService.None;
			}
		}

		public void updateLine ()
		{	
			ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;		
			TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer [(int)lineID];
			int stopsCount = t.CountStops (lineID);
			setLinearMapColor (lineInfoPanel.controller.tm.GetLineColor (lineID));
			clearStations ();
			String bgSprite;
			ItemClass.SubService ss = setFormatBgByType (t, out bgSprite, out mn);	
			linearMapLineNumberFormat.backgroundSprite = bgSprite;
			bool day, night;
			t.GetActive (out day, out  night);
			if (!day || !night) {
				linearMapLineTime.backgroundSprite = day ? "DayIcon" : night?"NightIcon": "DisabledIcon";
			} else {
				linearMapLineTime.backgroundSprite = "";
			}
			setLineNumberCircle (t.m_lineNumber, mn);
			
			m_autoName = "";
			ushort[] stopBuildings = new ushort[stopsCount];
			MultiMap<ushort,Vector3> bufferToDraw = new MultiMap<ushort,Vector3> ();
			int perfectSimetricLineStationsCount = (stopsCount + 2) / 2;
			bool simetric = t.Info.m_transportType != TransportInfo.TransportType.Bus;
			int j;
			if (simetric) {
				
				NetManager nm = Singleton<NetManager>.instance;
				BuildingManager bm = Singleton<BuildingManager>.instance;
				//try to find the loop
				int middle = -1;
				for (j = -1; j<stopsCount/2; j++) {					
					int offsetL = (j + stopsCount) % stopsCount;
					int offsetH = (j + 2) % stopsCount;					
					NetNode nn1 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetL)];
					NetNode nn2 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetH)];
					ushort buildingId1 = bm.FindBuilding (nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
					ushort buildingId2 = bm.FindBuilding (nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
					//					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
					//					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
					
					if (buildingId1 == buildingId2) {
						middle = j + 1;
						break;
					}
				}
				//				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"middle="+middle);
				if (middle >= 0) {	
					for (j = 1; j<=stopsCount/2; j++) {
						int offsetL = (-j + middle + stopsCount) % stopsCount;
						int offsetH = (j + middle) % stopsCount;
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"t.GetStop (offsetL)="+t.GetStop (offsetH)+"|t.GetStop (offsetH)="+t.GetStop (offsetH));
						
						NetNode nn1 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetL)];
						NetNode nn2 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetH)];
						
						ushort buildingId1 = bm.FindBuilding (nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
						ushort buildingId2 = bm.FindBuilding (nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
						
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
						//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
						
						if (buildingId1 != buildingId2) {
							simetric = false;
							break;
						}
					}				
				} else {
					simetric = false;
				}
				if (simetric) {
					lineStationsPanel.width = 5;
					for (j = 0; j<=stopsCount/2; j++) {	
						string stationName = null;
						List<ushort> intersections;
						string airport, port, taxi;
						Vector3 local = getStation (t.GetStop (middle + j), ss, out stationName, out intersections, out airport, out  port, out  taxi);
						lineStationsPanel.width += addStationToLinearMap (stationName, local, lineStationsPanel.width, intersections, airport, port, taxi) + 5;
						if (j == 0) {
							m_autoName += stationName + " - ";
						}
						if (j == stopsCount / 2) {
							m_autoName += stationName;
						}
					}					
				}
			}
			if (!simetric) {
				DistrictManager dm = Singleton<DistrictManager>.instance;
				byte lastDistrict = 0;
				string stationName = null;
				Vector3 local;
				byte district;
				List<int> districtList = new List<int> ();
				lineStationsPanel.width = 5;
				string airport, port, taxi;
				for (j = 0; j<stopsCount; j++) {	
					List<ushort> intersections;
					local = getStation (t.GetStop (j), ss, out stationName, out intersections, out airport, out  port, out  taxi);
					lineStationsPanel.width += addStationToLinearMap (stationName, local, lineStationsPanel.width, intersections, airport, port, taxi);
					district = dm.GetDistrict (local);
					if ((district != lastDistrict) && district != 0) {
						districtList.Add (district);
					}
					if (district != 0) {
						lastDistrict = district;
					}
				}			
				
				List<ushort> intersections2;
				local = getStation (t.GetStop (0), ss, out stationName, out intersections2, out airport, out  port, out  taxi);
				lineStationsPanel.width += addStationToLinearMap (stationName, local, lineStationsPanel.width, intersections2, airport, port, taxi) + 5;
				district = dm.GetDistrict (local);
				if ((district != lastDistrict) && district != 0) {
					districtList.Add (district);
				}
				int middle;
				int[] districtArray = districtList.ToArray ();
				if (districtArray.Length == 1) {
					m_autoName = (TransportLinesManagerMod.savedCircularOnSingleDistrict.value ? "Circular " : "") + dm.GetDistrictName (districtArray [0]);
				} else if (TLMUtils.findSimetry (districtArray, out middle)) {
					int firstIdx = middle;
					int lastIdx = middle + districtArray.Length / 2;

					m_autoName = dm.GetDistrictName (districtArray [firstIdx % districtArray.Length]) + " - " + dm.GetDistrictName (districtArray [lastIdx % districtArray.Length]);
					if (lastIdx - firstIdx > 1) {
						m_autoName += ", via ";
						for (int k = firstIdx+1; k<lastIdx; k++) {
							m_autoName += dm.GetDistrictName (districtArray [k % districtArray.Length]);
							if (k + 1 != lastIdx) {
								m_autoName += ", ";
							}
						}
					}
				} else {
					bool inicio = true;
					foreach (int i in districtArray) {
						m_autoName += (inicio ? "" : " - ") + dm.GetDistrictName (i);
						inicio = false;
					}
				}
			}
		}
		
		private void clearStations ()
		{			
			UnityEngine.Object.Destroy (lineStationsPanel.gameObject);
			createLineStationsPanel ();
		}
		
		private void createLineStationsLinearView ()
		{
			TLMUtils.createUIElement<UIPanel> (ref mainContainer, lineInfoPanel.transform);			
			mainContainer.absolutePosition = new Vector3 (2f, lineInfoPanel.controller.uiView.fixedHeight - 300f);
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
			linearMapLineNumberFormat.relativePosition = new Vector3 (0f, 0f);
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
			linearMapLineNumber.relativePosition = new Vector3 (-0.5f, 0.5f);
			TLMUtils.createDragHandle (linearMapLineNumber, mainContainer);

			TLMUtils.createUIElement<UILabel> (ref linearMapLineTime, linearMapLineNumberFormat.transform);
			linearMapLineTime.autoSize = false;
			linearMapLineTime.width = 50;
			linearMapLineTime.height = 50;
			linearMapLineTime.color = new Color (1, 1, 1, 1);
			linearMapLineTime.pivot = UIPivotPoint.MiddleLeft;
			linearMapLineTime.textAlignment = UIHorizontalAlignment.Center;
			linearMapLineTime.verticalAlignment = UIVerticalAlignment.Middle;
			linearMapLineTime.name = "LineTime";
			linearMapLineTime.relativePosition = new Vector3 (0f, 0f);
			linearMapLineTime.atlas = TLMController.taLineNumber;
			TLMUtils.createDragHandle (linearMapLineTime, mainContainer);
			
			createLineStationsPanel ();
		}
		
		private void createLineStationsPanel ()
		{
			
			TLMUtils.createUIElement<UIPanel> (ref lineStationsPanel, mainContainer.transform);
			lineStationsPanel.width = 140;
			lineStationsPanel.height = 30;
			lineStationsPanel.name = "LineStationsPanel";
			lineStationsPanel.autoLayout = false;
			lineStationsPanel.useCenter = true;
			lineStationsPanel.wrapLayout = false;
			lineStationsPanel.backgroundSprite = "GenericPanelWhite";
			lineStationsPanel.pivot = UIPivotPoint.MiddleLeft;
			lineStationsPanel.relativePosition = new Vector3 (60f, 10f);
			lineStationsPanel.color = lineInfoPanel.controller.tm.GetLineColor (lineInfoPanel.lineIdSelecionado.TransportLine);
		}
		
		private float addStationToLinearMap (string stationName, Vector3 location, float offsetX, List<ushort> intersections, string airport, string  port, string  taxi)//, out float intersectionPanelHeight)
		{	
			ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;	
			TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer [(int)lineID];
			TransportManager tm = Singleton<TransportManager> .instance;


			Dictionary<String,ushort> otherLinesIntersections = new Dictionary<String,ushort> ();
			
			foreach (ushort s in intersections) {
				TransportLine tl = tm.m_lines.m_buffer [(int)s];
				if (tl.Info.GetSubService () != t.Info.GetSubService () || tl.m_lineNumber != t.m_lineNumber) {
					string transportTypeLetter = "";
					switch (tl.Info.m_transportType) {
					case TransportInfo.TransportType.Bus:
						transportTypeLetter = "E";
						break;
					case TransportInfo.TransportType.Metro:
						transportTypeLetter = "B";
						break;
					case TransportInfo.TransportType.Train:
						transportTypeLetter = "C";
						break;
					}
					otherLinesIntersections.Add (transportTypeLetter + tl.m_lineNumber.ToString ().PadLeft (5, '0'), s);
				}
			}
			
			UIButton stationButton = null;			
			TLMUtils.createUIElement<UIButton> (ref stationButton, lineStationsPanel.transform);
			stationButton.relativePosition = new Vector3 (offsetX, 15f);
			stationButton.width = 20;
			stationButton.height = 20;
			stationButton.name = "Station [" + stationName + "]";
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
			stationLabel.name = "Station [" + stationName + "] Name";
			stationLabel.relativePosition = new Vector3 (23f, -13f);
			stationLabel.text = stationName;
			
			stationButton.gameObject.transform.localPosition = new Vector3 (0, 0, 0);
			stationButton.gameObject.transform.localEulerAngles = new Vector3 (0, 0, 45);
			stationButton.eventClick += (component, eventParam) => {				
				lineInfoPanel.cameraController.SetTarget (lineInfoPanel.lineIdSelecionado, location, false);
				lineInfoPanel.cameraController.ClearTarget ();
				
			};
			
			
			int intersectionCount = otherLinesIntersections.Count + (airport != string.Empty ? 1 : 0) + (taxi != string.Empty ? 1 : 0) + (port != string.Empty ? 1 : 0);
			if (intersectionCount > 0) {
				UIPanel intersectionsPanel = null;
				TLMUtils.createUIElement<UIPanel> (ref intersectionsPanel, stationButton.transform);
				intersectionsPanel.autoSize = false;
				intersectionsPanel.autoLayout = false;
				intersectionsPanel.autoLayoutStart = LayoutStart.TopLeft;
				intersectionsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
				intersectionsPanel.relativePosition = new Vector3 (-20, 10);
				intersectionsPanel.wrapLayout = false;

				float size = otherLinesIntersections.Count > 3 ? 20 : 40;
				float multiplier = otherLinesIntersections.Count > 3 ? 0.4f :0.8f;
				foreach (var s in otherLinesIntersections.OrderBy (x => x.Key)) {
					TransportLine intersectLine = tm.m_lines.m_buffer [(int)s.Value];
					String bgSprite;
					ModoNomenclatura nomenclatura;
					ItemClass.SubService ss = setFormatBgByType (intersectLine, out bgSprite, out nomenclatura);	
					UIButtonLineInfo lineCircleIntersect = null;
					TLMUtils.createUIElement<UIButtonLineInfo> (ref lineCircleIntersect, intersectionsPanel.transform);
					lineCircleIntersect.autoSize = false;
					lineCircleIntersect.width = size;
					lineCircleIntersect.height = size;
					lineCircleIntersect.color = intersectLine.m_color;
					lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
					lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
					lineCircleIntersect.name = "LineFormat";
					lineCircleIntersect.relativePosition = new Vector3 (0f, 0f);
					lineCircleIntersect.atlas = TLMController.taLineNumber;	
					lineCircleIntersect.normalBgSprite = bgSprite;
					lineCircleIntersect.hoveredColor = Color.white;
					lineCircleIntersect.hoveredTextColor = Color.red;
					lineCircleIntersect.lineID = s.Value;
					lineCircleIntersect.tooltip = tm.GetLineName (s.Value);
					lineCircleIntersect.eventClick += lineInfoPanel.openLineInfo;
					TLMUtils.createDragHandle (lineCircleIntersect, mainContainer);


					UILabel lineNumberIntersect = null;
				
					TLMUtils.createUIElement<UILabel> (ref lineNumberIntersect, lineCircleIntersect.transform);
					lineNumberIntersect.autoSize = false;
					lineNumberIntersect.autoHeight = false;
					lineNumberIntersect.width = lineCircleIntersect.width;
					lineNumberIntersect.pivot = UIPivotPoint.MiddleCenter;
					lineNumberIntersect.textAlignment = UIHorizontalAlignment.Center;
					lineNumberIntersect.verticalAlignment = UIVerticalAlignment.Middle;
					lineNumberIntersect.name = "LineNumber";
					lineNumberIntersect.height = size;
					lineNumberIntersect.relativePosition = new Vector3 (-0.5f, 0.5f);
					lineNumberIntersect.textColor = Color.white;
					lineNumberIntersect.outlineColor = Color.black;
					lineNumberIntersect.useOutline = true;
					bool day, night;
					intersectLine.GetActive (out day, out  night);
					if (!day || !night) {
						UILabel daytimeIndicator = null;
						TLMUtils.createUIElement<UILabel> (ref daytimeIndicator, lineCircleIntersect.transform);
						daytimeIndicator.autoSize = false;
						daytimeIndicator.width = size;
						daytimeIndicator.height = size;
						daytimeIndicator.color = Color.white;
						daytimeIndicator.pivot = UIPivotPoint.MiddleLeft;
						daytimeIndicator.verticalAlignment = UIVerticalAlignment.Middle;
						daytimeIndicator.name = "LineTime";
						daytimeIndicator.relativePosition = new Vector3 (0f, 0f);
						daytimeIndicator.atlas = TLMController.taLineNumber;	
						daytimeIndicator.backgroundSprite = day ? "DayIcon" : night?"NightIcon": "DisabledIcon";
					}
					TLMUtils.createDragHandle (lineNumberIntersect, mainContainer);	
					setLineNumberCircleOnRef (intersectLine.m_lineNumber, nomenclatura, lineNumberIntersect);
					lineNumberIntersect.textScale *= multiplier;
					lineNumberIntersect.relativePosition *= multiplier;
				}
				if (airport != string.Empty) {
					addExtraStationBuildingIntersection (intersectionsPanel, size, "AirplaneIcon", airport);
				}
				if (port != string.Empty) {
					addExtraStationBuildingIntersection (intersectionsPanel, size, "ShipIcon", port);
				}
				if (taxi != string.Empty) {
					addExtraStationBuildingIntersection (intersectionsPanel, size, "TaxiIcon", taxi);
				}
				intersectionsPanel.autoLayout = true;
				intersectionsPanel.wrapLayout = true;
				intersectionsPanel.width = 55;
//				
				return 42f;
			} else {
				return 25f;
			}
			
		}

		private void addExtraStationBuildingIntersection (UIComponent parent, float size, string bgSprite, string description)
		{
			UILabel lineCircleIntersect = null;
			TLMUtils.createUIElement<UILabel> (ref lineCircleIntersect, parent.transform);
			lineCircleIntersect.autoSize = false;
			lineCircleIntersect.width = size;
			lineCircleIntersect.height = size;
			lineCircleIntersect.pivot = UIPivotPoint.MiddleLeft;
			lineCircleIntersect.verticalAlignment = UIVerticalAlignment.Middle;
			lineCircleIntersect.name = "LineFormat";
			lineCircleIntersect.relativePosition = new Vector3 (0f, 0f);
			lineCircleIntersect.atlas = TLMController.taLineNumber;	
			lineCircleIntersect.backgroundSprite = bgSprite;
			lineCircleIntersect.tooltip = description;
		}

		private static ItemClass.Service[] seachOrder = new ItemClass.Service[]{
			ItemClass.Service.Monument,
			ItemClass.Service.Beautification,
			ItemClass.Service.Government,
			ItemClass.Service.HealthCare,
			ItemClass.Service.FireDepartment,
			ItemClass.Service.PoliceDepartment,
			ItemClass.Service.Tourism,
			ItemClass.Service.Education,
			ItemClass.Service.Garbage,
			ItemClass.Service.Office,
			ItemClass.Service.Commercial,
			ItemClass.Service.Industrial,
			ItemClass.Service.Residential,	
			ItemClass.Service.Electricity,		
			ItemClass.Service.Water
		};

		Vector3 getStation (uint stopId, ItemClass.SubService ss, out string stationName, out List<ushort> linhas, out string airport, out string passengerPort, out string taxiStand)
		{
			NetManager nm = Singleton<NetManager>.instance;
			BuildingManager bm = Singleton<BuildingManager>.instance;
			NetNode nn = nm.m_nodes.m_buffer [(int)stopId];
			ushort buildingId=0;
			bool transportBuilding = false;
			if (ss != ItemClass.SubService.None) {
				buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.Untouchable);
				transportBuilding = true;
			} 

			if(buildingId==0){
				buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.Untouchable);
				if (buildingId == 0) {
					int iterator = 0;
					while (buildingId == 0 && iterator<seachOrder.Count()) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, seachOrder [iterator], ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
						iterator++;
					}
				} else {
					transportBuilding = true;
				}
			}
			Vector3 location = nn.m_position;
			Building b = bm.m_buildings.m_buffer [buildingId];
			if (buildingId > 0) {
				InstanceID iid = default(InstanceID);
				iid.Building = buildingId;
				iid.TransportLine = lineInfoPanel.lineIdSelecionado.TransportLine;
				stationName = bm.GetBuildingName (buildingId, iid);
			} else {
				DistrictManager dm = Singleton<DistrictManager>.instance;
				int dId = dm.GetDistrict (location);
				if (dId > 0) {
					District d = dm.m_districts.m_buffer [dId];
					stationName = "[D] " + dm.GetDistrictName (dId);
				} else {
					stationName = "[X=" + location.x + "|Y=" + location.y + "|Z=" + location.z + "]";
				}
			}

			//paradas proximas (metro e trem)
			TransportManager tm = Singleton<TransportManager>.instance;
			TransportInfo thisLineInfo = tm.m_lines.m_buffer [(int)nn.m_transportLine].Info;			
			TransportLine thisLine = tm.m_lines.m_buffer [(int)nn.m_transportLine];
			linhas = new List<ushort> ();
			GetNearStops (nn.m_position, 30f, ref linhas);
			Vector3 sidewalkPosition = Vector3.zero;
			if (buildingId > 0 && transportBuilding) {
				sidewalkPosition = b.CalculateSidewalkPosition ();
				GetNearStops (sidewalkPosition, 100f, ref linhas);
			}

			airport = String.Empty;
			passengerPort = String.Empty;
			taxiStand = String.Empty;

			if (TransportLinesManagerMod.savedShowAirportsOnLinearMap.value) {
				ushort airportId = bm.FindBuilding (sidewalkPosition != Vector3.zero ? sidewalkPosition : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, Building.Flags.None, Building.Flags.Untouchable) ;

				if (airportId > 0) {
					InstanceID iid = default(InstanceID);
					iid.Building = airportId;
					airport = bm.GetBuildingName (airportId, iid);
				}
			}
			if (TransportLinesManagerMod.savedShowPassengerPortsOnLinearMap.value) {
				ushort portId = bm.FindBuilding (sidewalkPosition != Vector3.zero ? sidewalkPosition : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, Building.Flags.None, Building.Flags.Untouchable) ;

				if (portId > 0) {
					InstanceID iid = default(InstanceID);
					iid.Building = portId;
					passengerPort = bm.GetBuildingName (portId, iid);
				}
			}
			if (TransportLinesManagerMod.savedShowPassengerPortsOnLinearMap.value) {
				ushort taxiId = bm.FindBuilding (sidewalkPosition != Vector3.zero ? sidewalkPosition : nn.m_position, 50f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTaxi, Building.Flags.None, Building.Flags.Untouchable);

				if (taxiId > 0) {
					InstanceID iid = default(InstanceID);
					iid.Building = taxiId;
					taxiStand = bm.GetBuildingName (taxiId, iid);
				}
			}


			return location;
		}

		public bool GetNearStops (Vector3 pos, float maxDistance, ref List<ushort> linesFound)
		{
			int num = Mathf.Max ((int)((pos.x - maxDistance) / 64f + 135f), 0);
			int num2 = Mathf.Max ((int)((pos.z - maxDistance) / 64f + 135f), 0);
			int num3 = Mathf.Min ((int)((pos.x + maxDistance) / 64f + 135f), 269);
			int num4 = Mathf.Min ((int)((pos.z + maxDistance) / 64f + 135f), 269);
			bool noneFound = true;
			NetManager nm = Singleton<NetManager>.instance;
			TransportManager tm = Singleton<TransportManager>.instance;
			for (int i = num2; i <= num4; i++) {
				for (int j = num; j <= num3; j++) {
					ushort num6 = nm.m_nodeGrid [i * 270 + j];
					int num7 = 0;
					while (num6 != 0) {
						NetInfo info = nm.m_nodes.m_buffer [(int)num6].Info;

						if ((info.m_class.m_service == ItemClass.Service.PublicTransport) && 
							((info.m_class.m_subService == ItemClass.SubService.PublicTransportTrain && TransportLinesManagerMod.savedShowTrainLinesOnLinearMap.value)
							|| (info.m_class.m_subService == ItemClass.SubService.PublicTransportMetro && TransportLinesManagerMod.savedShowMetroLinesOnLinearMap.value)
							|| (info.m_class.m_subService == ItemClass.SubService.PublicTransportBus && TransportLinesManagerMod.savedShowBusLinesOnLinearMap.value))) {
							ushort transportLine = nm.m_nodes.m_buffer [(int)num6].m_transportLine;
							if (transportLine != 0) {
								TransportInfo info2 = tm.m_lines.m_buffer [(int)transportLine].Info;
								if (!linesFound.Contains (transportLine) && (tm.m_lines.m_buffer [(int)transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None) {
									float num8 = Vector3.SqrMagnitude (pos - nm.m_nodes.m_buffer [(int)num6].m_position);
									if (num8 < maxDistance * maxDistance) {
										linesFound.Add (transportLine);
										GetNearStops (nm.m_nodes.m_buffer [(int)num6].m_position, maxDistance, ref linesFound);
										noneFound = false;
									}
								}
							}
						}

						num6 = nm.m_nodes.m_buffer [(int)num6].m_nextGridNode;
						if (++num7 >= 32768) {
							CODebugBase<LogChannel>.Error (LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
							break;
						}
					}
				}
			}
			return noneFound;
		}

	}
}