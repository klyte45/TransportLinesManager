using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager
{
    public class TLMLinearMap
    {

        private TLMLineInfoPanel lineInfoPanel;
        private UILabel linearMapLineNumberFormat;
        private UILabel linearMapLineNumber;
        private UILabel linearMapLineTime;
        private UIPanel lineStationsPanel;
        private UIPanel mainContainer;
        private string m_autoName;
        private ModoNomenclatura prefix;
        private ModoNomenclatura suffix;
        private Separador sep;
        private bool zerosEsquerda;
        private bool invertPrefixSuffix;

        public bool isVisible
        {
            get
            {
                return mainContainer.isVisible;
            }
            set
            {
                mainContainer.isVisible = value;
            }
        }

        public GameObject gameObject
        {
            get
            {
                try
                {
                    return mainContainer.gameObject;
                }
                catch (Exception e)
                {
                    return null;
                }

            }
        }

        public string autoName
        {
            get
            {
                ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;
                TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer[(int)lineID];
                return "[" + TLMUtils.getString(prefix, sep, suffix, t.m_lineNumber, zerosEsquerda, invertPrefixSuffix).Replace('\n', ' ') + "] " + m_autoName;
            }
        }

        public TLMLinearMap(TLMLineInfoPanel lip)
        {
            lineInfoPanel = lip;
            createLineStationsLinearView();
        }

        public void setLinearMapColor(Color c)
        {
            linearMapLineNumberFormat.color = c;
            linearMapLineNumber.textColor = TLMUtils.contrastColor(c);
            lineStationsPanel.color = c;
        }

        public void setLineNumberCircle(int num, ModoNomenclatura pre, Separador s, ModoNomenclatura mn, bool zeros, bool invertPrefixSuffix)
        {
            TLMLineUtils.setLineNumberCircleOnRef(num, pre, s, mn, zeros, linearMapLineNumber, invertPrefixSuffix);
        }



        public void updateLine()
        {
            ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;
            TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer[(int)lineID];
            int stopsCount = t.CountStops(lineID);
            setLinearMapColor(lineInfoPanel.controller.tm.GetLineColor(lineID));
            clearStations();
            String bgSprite;
            ItemClass.SubService ss = TLMLineUtils.getLineNamingParameters(lineID, out prefix, out sep, out suffix, out zerosEsquerda, out invertPrefixSuffix, out bgSprite);
            linearMapLineNumberFormat.backgroundSprite = bgSprite;
            bool day, night;
            t.GetActive(out day, out night);
            if (!day || !night)
            {
                linearMapLineTime.backgroundSprite = day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
            }
            else {
                linearMapLineTime.backgroundSprite = "";
            }
            setLineNumberCircle(t.m_lineNumber, prefix, sep, suffix, zerosEsquerda, invertPrefixSuffix);

            m_autoName = "";
            ushort[] stopBuildings = new ushort[stopsCount];
            MultiMap<ushort, Vector3> bufferToDraw = new MultiMap<ushort, Vector3>();
            int perfectSimetricLineStationsCount = (stopsCount + 2) / 2;
            bool simetric = t.Info.m_transportType != TransportInfo.TransportType.Bus;
            int j;
            if (simetric)
            {

                NetManager nm = Singleton<NetManager>.instance;
                BuildingManager bm = Singleton<BuildingManager>.instance;
                //try to find the loop
                int middle = -1;
                for (j = -1; j < stopsCount / 2; j++)
                {
                    int offsetL = (j + stopsCount) % stopsCount;
                    int offsetH = (j + 2) % stopsCount;
                    NetNode nn1 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetL)];
                    NetNode nn2 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetH)];
                    ushort buildingId1 = bm.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
                    ushort buildingId2 = bm.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
                    //					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                    //					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);

                    if (buildingId1 == buildingId2)
                    {
                        middle = j + 1;
                        break;
                    }
                }
                //				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"middle="+middle);
                if (middle >= 0)
                {
                    for (j = 1; j <= stopsCount / 2; j++)
                    {
                        int offsetL = (-j + middle + stopsCount) % stopsCount;
                        int offsetH = (j + middle) % stopsCount;
                        //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
                        //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"t.GetStop (offsetL)="+t.GetStop (offsetH)+"|t.GetStop (offsetH)="+t.GetStop (offsetH));

                        NetNode nn1 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetL)];
                        NetNode nn2 = nm.m_nodes.m_buffer[(int)t.GetStop(offsetH)];

                        ushort buildingId1 = bm.FindBuilding(nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
                        ushort buildingId2 = bm.FindBuilding(nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
                        //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);

                        //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
                        //						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);

                        if (buildingId1 != buildingId2)
                        {
                            simetric = false;
                            break;
                        }
                    }
                }
                else {
                    simetric = false;
                }
                if (simetric)
                {
                    lineStationsPanel.width = 5;
                    for (j = 0; j <= stopsCount / 2; j++)
                    {
                        string stationName = null;
                        List<ushort> intersections;
                        string airport, port, taxi;
                        Vector3 local = getStation(t.GetStop(middle + j), ss, out stationName, out intersections, out airport, out port, out taxi);
                        lineStationsPanel.width += addStationToLinearMap(stationName, local, lineStationsPanel.width, intersections, airport, port, taxi) + 5;
                        if (j == 0)
                        {
                            m_autoName += stationName + " - ";
                        }
                        if (j == stopsCount / 2)
                        {
                            m_autoName += stationName;
                        }
                    }
                }
            }
            if (!simetric)
            {
                DistrictManager dm = Singleton<DistrictManager>.instance;
                byte lastDistrict = 0;
                string stationName = null;
                Vector3 local;
                byte district;
                List<int> districtList = new List<int>();
                lineStationsPanel.width = 5;
                string airport, port, taxi;
                for (j = 0; j < stopsCount; j++)
                {
                    List<ushort> intersections;
                    local = getStation(t.GetStop(j), ss, out stationName, out intersections, out airport, out port, out taxi);
                    lineStationsPanel.width += addStationToLinearMap(stationName, local, lineStationsPanel.width, intersections, airport, port, taxi);
                    district = dm.GetDistrict(local);
                    if ((district != lastDistrict) && district != 0)
                    {
                        districtList.Add(district);
                    }
                    if (district != 0)
                    {
                        lastDistrict = district;
                    }
                }

                List<ushort> intersections2;
                local = getStation(t.GetStop(0), ss, out stationName, out intersections2, out airport, out port, out taxi);
                lineStationsPanel.width += addStationToLinearMap(stationName, local, lineStationsPanel.width, intersections2, airport, port, taxi) + 5;
                district = dm.GetDistrict(local);
                if ((district != lastDistrict) && district != 0)
                {
                    districtList.Add(district);
                }
                int middle;
                int[] districtArray = districtList.ToArray();
                if (districtArray.Length == 1)
                {
                    m_autoName = (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.CIRCULAR_IN_SINGLE_DISTRICT_LINE) ? "Circular " : "") + dm.GetDistrictName(districtArray[0]);
                }
                else if (TLMUtils.findSimetry(districtArray, out middle))
                {
                    int firstIdx = middle;
                    int lastIdx = middle + districtArray.Length / 2;

                    m_autoName = dm.GetDistrictName(districtArray[firstIdx % districtArray.Length]) + " - " + dm.GetDistrictName(districtArray[lastIdx % districtArray.Length]);
                    if (lastIdx - firstIdx > 1)
                    {
                        m_autoName += ", via ";
                        for (int k = firstIdx + 1; k < lastIdx; k++)
                        {
                            m_autoName += dm.GetDistrictName(districtArray[k % districtArray.Length]);
                            if (k + 1 != lastIdx)
                            {
                                m_autoName += ", ";
                            }
                        }
                    }
                }
                else {
                    bool inicio = true;
                    foreach (int i in districtArray)
                    {
                        m_autoName += (inicio ? "" : " - ") + dm.GetDistrictName(i);
                        inicio = false;
                    }
                }
            }
        }

        private void clearStations()
        {
            UnityEngine.Object.Destroy(lineStationsPanel.gameObject);
            createLineStationsPanel();
        }

        private void createLineStationsLinearView()
        {
            TLMUtils.createUIElement<UIPanel>(ref mainContainer, lineInfoPanel.transform);
            mainContainer.absolutePosition = new Vector3(2f, lineInfoPanel.controller.uiView.fixedHeight - 300f);
            mainContainer.name = "LineStationsLinearView";
            mainContainer.height = 50;
            mainContainer.autoSize = true;

            TLMUtils.createUIElement<UILabel>(ref linearMapLineNumberFormat, mainContainer.transform);
            linearMapLineNumberFormat.autoSize = false;
            linearMapLineNumberFormat.width = 50;
            linearMapLineNumberFormat.height = 50;
            linearMapLineNumberFormat.color = new Color(1, 0, 0, 1);
            linearMapLineNumberFormat.pivot = UIPivotPoint.MiddleLeft;
            linearMapLineNumberFormat.textAlignment = UIHorizontalAlignment.Center;
            linearMapLineNumberFormat.verticalAlignment = UIVerticalAlignment.Middle;
            linearMapLineNumberFormat.name = "LineFormat";
            linearMapLineNumberFormat.relativePosition = new Vector3(0f, 0f);
            linearMapLineNumberFormat.atlas = TLMController.taLineNumber;
            TLMUtils.createDragHandle(linearMapLineNumberFormat, mainContainer);

            TLMUtils.createUIElement<UILabel>(ref linearMapLineNumber, linearMapLineNumberFormat.transform);
            linearMapLineNumber.autoSize = false;
            linearMapLineNumber.autoHeight = false;
            linearMapLineNumber.width = linearMapLineNumberFormat.width;
            linearMapLineNumber.pivot = UIPivotPoint.MiddleCenter;
            linearMapLineNumber.textAlignment = UIHorizontalAlignment.Center;
            linearMapLineNumber.verticalAlignment = UIVerticalAlignment.Middle;
            linearMapLineNumber.name = "LineNumber";


            linearMapLineNumber.width = 50;
            linearMapLineNumber.height = 50;
            linearMapLineNumber.relativePosition = new Vector3(-0.5f, 0.5f);
            TLMUtils.createDragHandle(linearMapLineNumber, mainContainer);

            TLMUtils.createUIElement<UILabel>(ref linearMapLineTime, linearMapLineNumberFormat.transform);
            linearMapLineTime.autoSize = false;
            linearMapLineTime.width = 50;
            linearMapLineTime.height = 50;
            linearMapLineTime.color = new Color(1, 1, 1, 1);
            linearMapLineTime.pivot = UIPivotPoint.MiddleLeft;
            linearMapLineTime.textAlignment = UIHorizontalAlignment.Center;
            linearMapLineTime.verticalAlignment = UIVerticalAlignment.Middle;
            linearMapLineTime.name = "LineTime";
            linearMapLineTime.relativePosition = new Vector3(0f, 0f);
            linearMapLineTime.atlas = TLMController.taLineNumber;
            TLMUtils.createDragHandle(linearMapLineTime, mainContainer);

            createLineStationsPanel();
        }

        private void createLineStationsPanel()
        {

            TLMUtils.createUIElement<UIPanel>(ref lineStationsPanel, mainContainer.transform);
            lineStationsPanel.width = 140;
            lineStationsPanel.height = 30;
            lineStationsPanel.name = "LineStationsPanel";
            lineStationsPanel.autoLayout = false;
            lineStationsPanel.useCenter = true;
            lineStationsPanel.wrapLayout = false;
            lineStationsPanel.backgroundSprite = "GenericPanelWhite";
            lineStationsPanel.pivot = UIPivotPoint.MiddleLeft;
            lineStationsPanel.relativePosition = new Vector3(60f, 10f);
            lineStationsPanel.color = lineInfoPanel.controller.tm.GetLineColor(lineInfoPanel.lineIdSelecionado.TransportLine);
        }

        private float addStationToLinearMap(string stationName, Vector3 location, float offsetX, List<ushort> intersections, string airport, string port, string taxi)//, out float intersectionPanelHeight)
        {
            ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;
            TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer[(int)lineID];
            TransportManager tm = Singleton<TransportManager>.instance;




            UIButton stationButton = null;
            TLMUtils.createUIElement<UIButton>(ref stationButton, lineStationsPanel.transform);
            stationButton.relativePosition = new Vector3(offsetX, 15f);
            stationButton.width = 20;
            stationButton.height = 20;
            stationButton.name = "Station [" + stationName + "]";
            TLMUtils.initButton(stationButton, true, "IconPolicyBaseCircle");

            UILabel stationLabel = null;
            TLMUtils.createUIElement<UILabel>(ref stationLabel, stationButton.transform);
            stationLabel.autoSize = true;
            stationLabel.width = 20;
            stationLabel.height = 20;
            stationLabel.useOutline = true;
            stationLabel.pivot = UIPivotPoint.MiddleLeft;
            stationLabel.textAlignment = UIHorizontalAlignment.Center;
            stationLabel.verticalAlignment = UIVerticalAlignment.Middle;
            stationLabel.name = "Station [" + stationName + "] Name";
            stationLabel.relativePosition = new Vector3(23f, -13f);
            stationLabel.text = stationName;

            stationButton.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            stationButton.gameObject.transform.localEulerAngles = new Vector3(0, 0, 45);
            stationButton.eventClick += (component, eventParam) =>
            {
                lineInfoPanel.cameraController.SetTarget(lineInfoPanel.lineIdSelecionado, location, false);
                lineInfoPanel.cameraController.ClearTarget();

            };

            var otherLinesIntersections = TLMLineUtils.IndexLines(intersections, t);

            int intersectionCount = otherLinesIntersections.Count + (airport != string.Empty ? 1 : 0) + (taxi != string.Empty ? 1 : 0) + (port != string.Empty ? 1 : 0);
            if (intersectionCount > 0)
            {
                UIPanel intersectionsPanel = null;
                TLMUtils.createUIElement<UIPanel>(ref intersectionsPanel, stationButton.transform);
                intersectionsPanel.autoSize = false;
                intersectionsPanel.autoLayout = false;
                intersectionsPanel.autoLayoutStart = LayoutStart.TopLeft;
                intersectionsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                intersectionsPanel.relativePosition = new Vector3(-20, 10);
                intersectionsPanel.wrapLayout = false;
                intersectionsPanel.autoFitChildrenVertically = true;

                TLMLineUtils.PrintIntersections(airport, port, taxi, intersectionsPanel, otherLinesIntersections);

                intersectionsPanel.autoLayout = true;
                intersectionsPanel.wrapLayout = true;
                intersectionsPanel.width = 55;
                //				
                return 42f;
            }
            else {
                return 25f;
            }

        }



       

        Vector3 getStation(uint stopId, ItemClass.SubService ss, out string stationName, out List<ushort> linhas, out string airport, out string passengerPort, out string taxiStand)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            ushort buildingId = 0;
            bool transportBuilding = false;
            if (ss != ItemClass.SubService.None)
            {
                buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.CustomName, Building.Flags.Untouchable);
                transportBuilding = true;
            }

            if (buildingId == 0)
            {
                buildingId = bm.FindBuilding(nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, Building.Flags.Active | Building.Flags.CustomName, Building.Flags.Untouchable);
                if (buildingId == 0)
                {
                    int iterator = 0;
                    while (buildingId == 0 && iterator < TLMUtils.seachOrder.Count())
                    {
                        buildingId = bm.FindBuilding(nn.m_position, 100f, TLMUtils.seachOrder[iterator], ItemClass.SubService.None, Building.Flags.None, Building.Flags.Untouchable);
                        iterator++;
                    }
                }
                else {
                    transportBuilding = true;
                }
            }
            Vector3 location = nn.m_position;
            Building b = bm.m_buildings.m_buffer[buildingId];
            if (buildingId > 0)
            {
                InstanceID iid = default(InstanceID);
                iid.Building = buildingId;
                iid.TransportLine = lineInfoPanel.lineIdSelecionado.TransportLine;
                stationName = bm.GetBuildingName(buildingId, iid);
            }
            else {
                DistrictManager dm = Singleton<DistrictManager>.instance;
                int dId = dm.GetDistrict(location);
                if (dId > 0)
                {
                    District d = dm.m_districts.m_buffer[dId];
                    stationName = "[D] " + dm.GetDistrictName(dId);
                }
                else {
                    stationName = "[X=" + location.x + "|Y=" + location.y + "|Z=" + location.z + "]";
                }
            }

            //paradas proximas (metro e trem)
            TransportManager tm = Singleton<TransportManager>.instance;
            TransportInfo thisLineInfo = tm.m_lines.m_buffer[(int)nn.m_transportLine].Info;
            TransportLine thisLine = tm.m_lines.m_buffer[(int)nn.m_transportLine];
            linhas = new List<ushort>();
            TLMLineUtils.GetNearLines(nn.m_position, 30f, ref linhas);
            Vector3 sidewalkPosition = Vector3.zero;
            if (buildingId > 0 && transportBuilding)
            {
                sidewalkPosition = b.CalculateSidewalkPosition();
                TLMLineUtils.GetNearLines(sidewalkPosition, 100f, ref linhas);
            }

            airport = String.Empty;
            passengerPort = String.Empty;
            taxiStand = String.Empty;

            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP))
            {
                ushort airportId = bm.FindBuilding(sidewalkPosition != Vector3.zero ? sidewalkPosition : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, Building.Flags.None, Building.Flags.Untouchable);

                if (airportId > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = airportId;
                    airport = bm.GetBuildingName(airportId, iid);
                }
            }
            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP))
            {
                ushort portId = bm.FindBuilding(sidewalkPosition != Vector3.zero ? sidewalkPosition : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, Building.Flags.None, Building.Flags.Untouchable);

                if (portId > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = portId;
                    passengerPort = bm.GetBuildingName(portId, iid);
                }
            }
            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP))
            {
                ushort taxiId = bm.FindBuilding(sidewalkPosition != Vector3.zero ? sidewalkPosition : nn.m_position, 50f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTaxi, Building.Flags.None, Building.Flags.Untouchable);

                if (taxiId > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = taxiId;
                    taxiStand = bm.GetBuildingName(taxiId, iid);
                }
            }


            return location;
        }



    }
}