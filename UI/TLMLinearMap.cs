using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.UI
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
        private ModoNomenclatura nonPrefix;
        private Separador sep;
        private bool zerosEsquerda;
        private bool invertPrefixSuffix;
        private UIButton infoToggle;
        private Dictionary<ushort, UILabel> residentCounters = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> touristCounters = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> lineVehicles = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, float> stationOffsetX = new Dictionary<ushort, float>();
        private Dictionary<ushort, int> vehiclesOnStation = new Dictionary<ushort, int>();
        private const float vehicleYoffsetIncrement = -20f;
        private const float vehicleYbaseOffset = -55f;

        private bool showIntersections = true;
        private bool showExtraStopInfo = false;

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
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
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
                if (TLMCW.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME))
                {
                    return "[" + TLMUtils.getString(prefix, sep, suffix, nonPrefix, t.m_lineNumber, zerosEsquerda, invertPrefixSuffix).Replace('\n', ' ') + "] " + m_autoName;
                }
                else
                {
                    return m_autoName;
                }
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

        public void setLineNumberCircle(int num, ModoNomenclatura pre, Separador s, ModoNomenclatura mn, ModoNomenclatura np, bool zeros, bool invertPrefixSuffix)
        {
            TLMLineUtils.setLineNumberCircleOnRef(num, pre, s, mn, np, zeros, linearMapLineNumber, invertPrefixSuffix);
        }



        public void redrawLine()
        {
            ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;
            TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer[(int)lineID];
            int stopsCount = t.CountStops(lineID);
            int vehicleCount = t.CountVehicles(lineID);
            Color lineColor = lineInfoPanel.controller.tm.GetLineColor(lineID);
            setLinearMapColor(lineColor);
            clearStations();
            String bgSprite;
            ItemClass.SubService ss = TLMLineUtils.getLineNamingParameters(lineID, out prefix, out sep, out suffix, out nonPrefix, out zerosEsquerda, out invertPrefixSuffix, out bgSprite);
            linearMapLineNumberFormat.backgroundSprite = bgSprite;


            bool day, night, zeroed;
            TLMLineUtils.getLineActive(ref t, out day, out night, out zeroed);
            if (zeroed)
            {
                linearMapLineTime.backgroundSprite = "NoBudgetIcon";
            }
            else {
                if (!day || !night)
                {
                    linearMapLineTime.backgroundSprite = day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
                }
                else {
                    linearMapLineTime.backgroundSprite = "";
                }
            }

            setLineNumberCircle(t.m_lineNumber, prefix, sep, suffix, nonPrefix, zerosEsquerda, invertPrefixSuffix);

            m_autoName = TLMUtils.calculateAutoName(lineID);
            string stationName = null;
            Vector3 local;
            string airport, taxi;
            int middle;
            string namePrefix;
            bool simmetric = TLMUtils.CalculateSimmetry(ss, stopsCount, t, out middle);
            if (t.Info.m_transportType != TransportInfo.TransportType.Bus && t.Info.m_transportType != TransportInfo.TransportType.Tram && simmetric && !showExtraStopInfo)
            {
                lineStationsPanel.width = 5;
                for (int j = middle; j <= middle + stopsCount / 2; j++)
                {
                    List<ushort> intersections;
                    ushort stationId = t.GetStop(j);
                    local = getStation(lineID, stationId, ss, out stationName, out intersections, out airport, out taxi, out namePrefix);
                    lineStationsPanel.width += addStationToLinearMap(namePrefix, stationName, local, lineStationsPanel.width, intersections, airport, taxi, stationId, ss) + (j == middle + stopsCount / 2 ? 5 : 0);
                }
            }
            else {
                lineStationsPanel.width = 5;
                int minI = 0, maxI = stopsCount;
                if (simmetric)
                {
                    minI = middle + 1;
                    maxI = stopsCount + middle + 1;
                }
                if (showExtraStopInfo)
                {
                    int j = (minI - 1 + stopsCount) % stopsCount;
                    ushort stationId = t.GetStop(j);
                    List<ushort> intersections;
                    local = getStation(lineID, stationId, ss, out stationName, out intersections, out airport, out taxi, out namePrefix);
                    lineStationsPanel.width += addStationToLinearMap(namePrefix, stationName, local, lineStationsPanel.width, intersections, airport, taxi, stationId, ss, true);
                }
                for (int i = minI; i < maxI; i++)
                {
                    int j = i % stopsCount;
                    List<ushort> intersections;
                    ushort stationId = t.GetStop(j);
                    local = getStation(lineID, stationId, ss, out stationName, out intersections, out airport, out taxi, out namePrefix);
                    lineStationsPanel.width += addStationToLinearMap(namePrefix, stationName, local, lineStationsPanel.width, intersections, airport, taxi, stationId, ss) + (j == stopsCount - (showExtraStopInfo ? 0 : 1) ? 5 : 0);
                }
            }
            if (showExtraStopInfo)
            {
                vehiclesOnStation.Clear();
                for (int v = 0; v < vehicleCount; v++)
                {
                    ushort vehicleId = t.GetVehicle(v);

                    AddVehicleToLinearMap(lineColor, vehicleId);
                }
            }

        }

        private void AddVehicleToLinearMap(Color lineColor, ushort vehicleId)
        {

            UILabel vehicleLabel = null;
            int fill, cap;
            TLMLineUtils.GetVehicleCapacityAndFill(vehicleId, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out fill, out cap);

            TLMUtils.createUIElement<UILabel>(ref vehicleLabel, lineStationsPanel.transform);
            vehicleLabel.autoSize = false;
            vehicleLabel.text = string.Format("{0}/{1}", fill, cap);
            vehicleLabel.useOutline = true;
            vehicleLabel.width = 50;
            vehicleLabel.height = 33;
            vehicleLabel.pivot = UIPivotPoint.TopCenter;
            vehicleLabel.verticalAlignment = UIVerticalAlignment.Middle;
            vehicleLabel.atlas = TLMController.taLineNumber;

            vehicleLabel.padding = new RectOffset(0, 0, 2, 0);
            vehicleLabel.textScale = 0.6f;
            vehicleLabel.backgroundSprite = "VehicleLinearMap";
            vehicleLabel.color = lineColor;
            vehicleLabel.textAlignment = UIHorizontalAlignment.Center;
            vehicleLabel.tooltip = Singleton<VehicleManager>.instance.GetVehicleName(vehicleId);

            vehicleLabel.eventClick += (x, y) =>
            {
                InstanceID id = default(InstanceID);
                id.Vehicle = vehicleId;
                Camera.main.GetComponent<CameraController>().SetTarget(id, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].GetLastFramePosition(), true);
            };
            updateVehiclePosition(vehicleId, vehicleLabel);

            lineVehicles.Add(vehicleId, vehicleLabel);
        }

        private void updateVehiclePosition(ushort vehicleId, UILabel vehicleLabel)
        {
            try
            {
                ushort stopId = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding;
                var labelStation = residentCounters[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding];
                float destX = stationOffsetX[stopId] - labelStation.width / 4 * 3;
                if (Singleton<TransportManager>.instance.m_lines.m_buffer[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_transportLine].GetStop(0) == stopId && (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & Vehicle.Flags.Stopped) != 0)
                {
                    destX = stationOffsetX[TransportLine.GetPrevStop(stopId)] + labelStation.width / 4;
                }
                float yOffset = vehicleYbaseOffset;
                int busesOnStation = vehiclesOnStation.ContainsKey(stopId) ? vehiclesOnStation[stopId] : 0;
                if ((Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & Vehicle.Flags.Stopped) != 0)
                {
                    ushort prevStop = TransportLine.GetPrevStop(stopId);
                    destX = stationOffsetX[prevStop] - labelStation.width / 4;
                    busesOnStation = Math.Max(busesOnStation, vehiclesOnStation.ContainsKey(prevStop) ? vehiclesOnStation[prevStop] : 0);
                    vehiclesOnStation[prevStop] = busesOnStation + 1;
                }
                else if ((Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & Vehicle.Flags.Arriving) != 0)
                {
                    destX += labelStation.width / 4;
                    ushort nextStop = TransportLine.GetNextStop(stopId);
                    busesOnStation = Math.Max(busesOnStation, vehiclesOnStation.ContainsKey(nextStop) ? vehiclesOnStation[nextStop] : 0);
                    vehiclesOnStation[nextStop] = busesOnStation + 1;
                }
                else if ((Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & Vehicle.Flags.Leaving) != 0)
                {
                    destX -= labelStation.width / 4;
                    ushort prevStop = TransportLine.GetPrevStop(stopId);
                    busesOnStation = Math.Max(busesOnStation, vehiclesOnStation.ContainsKey(prevStop) ? vehiclesOnStation[prevStop] : 0);
                    vehiclesOnStation[prevStop] = busesOnStation + 1;
                }
                else
                {
                    ushort prevStop = TransportLine.GetPrevStop(stopId);
                    busesOnStation = Math.Max(busesOnStation, vehiclesOnStation.ContainsKey(prevStop) ? vehiclesOnStation[prevStop] : 0);
                }
                yOffset = vehicleYbaseOffset + busesOnStation * vehicleYoffsetIncrement;
                vehiclesOnStation[stopId] = busesOnStation + 1;
                vehicleLabel.position = new Vector3(destX, yOffset);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                TLMUtils.doLog("ERROR UPDATING VEHICLE!!!");
                redrawLine();
            }
        }

        private void clearStations()
        {
            UnityEngine.Object.Destroy(lineStationsPanel.gameObject);
            residentCounters.Clear();
            touristCounters.Clear();
            lineVehicles.Clear();
            stationOffsetX.Clear();
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

            TLMUtils.createUIElement<UIButton>(ref infoToggle, mainContainer.transform);
            TLMUtils.initButton(infoToggle, true, "ButtonMenu");
            infoToggle.relativePosition = new Vector3(0f, 60f);
            infoToggle.width = 50;
            infoToggle.height = 70;
            infoToggle.wordWrap = true;
            infoToggle.localeID = "TLM_SHOW_EXTRA_INFO";
            infoToggle.isLocalized = true;
            infoToggle.textScale = 0.8f;
            infoToggle.eventClick += (x, y) =>
            {
                showIntersections = !showIntersections;
                showExtraStopInfo = !showIntersections;
                if (showIntersections)
                {
                    infoToggle.localeID = "TLM_SHOW_EXTRA_INFO";
                }
                else
                {
                    infoToggle.localeID = "TLM_SHOW_LINE_INTEGRATION_SHORT";
                }
                redrawLine();
            };

            createLineStationsPanel();
        }

        public void updateBidings()
        {
            if (showExtraStopInfo)
            {
                foreach (var resLabel in residentCounters)
                {
                    int residents, tourists;
                    TLMLineUtils.GetQuantityPassengerWaiting(resLabel.Key, out residents, out tourists);
                    resLabel.Value.text = residents.ToString();
                    touristCounters[resLabel.Key].text = tourists.ToString();
                }
                ushort lineID = lineInfoPanel.lineIdSelecionado.TransportLine;
                TransportLine t = lineInfoPanel.controller.tm.m_lines.m_buffer[(int)lineID];
                Color lineColor = lineInfoPanel.controller.tm.GetLineColor(lineID);
                int vehicleCount = t.CountVehicles(lineID);
                List<ushort> oldItems = lineVehicles.Keys.ToList();
                vehiclesOnStation.Clear();
                for (int v = 0; v < vehicleCount; v++)
                {
                    ushort vehicleId = t.GetVehicle(v);
                    UILabel vehicleLabel = null;

                    if (oldItems.Contains(vehicleId))
                    {
                        vehicleLabel = lineVehicles[vehicleId];
                        int fill, cap;
                        TLMLineUtils.GetVehicleCapacityAndFill(vehicleId, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out fill, out cap);
                        vehicleLabel.text = string.Format("{0}/{1}", fill, cap);
                        var labelStation = residentCounters[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding];
                        updateVehiclePosition(vehicleId, vehicleLabel);
                        oldItems.Remove(vehicleId);
                    }
                    else
                    {
                        AddVehicleToLinearMap(lineColor, vehicleId);
                    }

                }
                foreach (ushort dead in oldItems)
                {
                    GameObject.Destroy(lineVehicles[dead].gameObject);
                    lineVehicles.Remove(dead);
                }
            }
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

        private float addStationToLinearMap(string stationPrefix, string stationName, Vector3 location, float offsetX, List<ushort> intersections, string airport, string taxi, ushort stationNodeId, ItemClass.SubService ss, bool simple = false)//, out float intersectionPanelHeight)
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

            UITextField stationLabel = null;
            TLMUtils.createUIElement<UITextField>(ref stationLabel, stationButton.transform);
            stationLabel.autoSize = true;
            stationLabel.width = 200;
            stationLabel.height = 20;
            stationLabel.useOutline = true;
            stationLabel.pivot = UIPivotPoint.MiddleLeft;
            stationLabel.horizontalAlignment = UIHorizontalAlignment.Left;
            stationLabel.verticalAlignment = UIVerticalAlignment.Middle;
            stationLabel.name = "Station [" + stationName + "] Name";
            stationLabel.relativePosition = new Vector3(23f, -13f);
            stationLabel.text = (!string.IsNullOrEmpty(stationPrefix) ? stationPrefix + " " : "") + stationName;
            TLMUtils.uiTextFieldDefaults(stationLabel);
            stationLabel.color = new Color(0.3f, 0.3f, 0.3f, 1);
            stationLabel.textColor = Color.white;
            stationLabel.cursorWidth = 2;
            stationLabel.cursorBlinkTime = 100;
            stationLabel.eventGotFocus += (x, y) =>
            {
                stationLabel.text = TLMUtils.getStationName(stationNodeId, lineID, ss);
            };
            stationLabel.eventTextSubmitted += (x, y) =>
            {
                TLMUtils.setStopName(y, stationNodeId, lineID, () =>
                {
                    stationLabel.text = TLMUtils.getFullStationName(stationNodeId, lineID, ss);
                    m_autoName = TLMUtils.calculateAutoName(lineID);
                    lineInfoPanel.autoNameLabel.text = autoName;
                });
            };

            stationButton.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            stationButton.gameObject.transform.localEulerAngles = new Vector3(0, 0, 45);
            stationButton.eventClick += (component, eventParam) =>
            {
                lineInfoPanel.cameraController.SetTarget(lineInfoPanel.lineIdSelecionado, location, false);
                lineInfoPanel.cameraController.ClearTarget();

            };
            if (!simple)
            {
                stationOffsetX.Add(stationNodeId, offsetX);
                if (showIntersections)
                {
                    var otherLinesIntersections = TLMLineUtils.SortLines(intersections, t);

                    int intersectionCount = otherLinesIntersections.Count + (airport != string.Empty ? 1 : 0) + (taxi != string.Empty ? 1 : 0);
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

                        TLMLineUtils.PrintIntersections(airport, taxi, intersectionsPanel, otherLinesIntersections);

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
                else if (showExtraStopInfo)
                {
                    float normalWidth = 42.5f;

                    NetNode stopNode = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)stationNodeId];

                    int residents, tourists;
                    TLMLineUtils.GetQuantityPassengerWaiting(stationNodeId, out residents, out tourists);

                    UIPanel stationInfoStatsPanel = null;
                    TLMUtils.createUIElement<UIPanel>(ref stationInfoStatsPanel, stationButton.transform);
                    stationInfoStatsPanel.autoSize = false;
                    stationInfoStatsPanel.autoLayout = false;
                    stationInfoStatsPanel.autoFitChildrenVertically = true;
                    stationInfoStatsPanel.autoLayoutStart = LayoutStart.TopLeft;
                    stationInfoStatsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                    stationInfoStatsPanel.relativePosition = new Vector3(-20, 10);
                    stationInfoStatsPanel.autoLayout = true;
                    stationInfoStatsPanel.wrapLayout = true;
                    stationInfoStatsPanel.width = normalWidth;

                    UILabel residentsWaiting = null;
                    TLMUtils.createUIElement<UILabel>(ref residentsWaiting, stationInfoStatsPanel.transform);
                    residentsWaiting.autoSize = false;
                    residentsWaiting.useOutline = true;
                    residentsWaiting.text = residents.ToString();
                    residentsWaiting.tooltipLocaleID = "TLM_RESIDENTS_WAITING";
                    residentsWaiting.backgroundSprite = "EmptySprite";
                    residentsWaiting.color = new Color32(0x12, 0x68, 0x34, 255);
                    residentsWaiting.width = normalWidth;
                    residentsWaiting.padding = new RectOffset(0, 0, 4, 2);
                    residentsWaiting.height = 20;
                    residentsWaiting.textScale = 0.7f;
                    residentsWaiting.textAlignment = UIHorizontalAlignment.Center;
                    residentCounters[stationNodeId] = residentsWaiting;

                    UILabel touristsWaiting = null;
                    TLMUtils.createUIElement<UILabel>(ref touristsWaiting, stationInfoStatsPanel.transform);
                    touristsWaiting.autoSize = false;
                    touristsWaiting.text = tourists.ToString();
                    touristsWaiting.tooltipLocaleID = "TLM_TOURISTS_WAITING";
                    touristsWaiting.useOutline = true;
                    touristsWaiting.width = normalWidth;
                    touristsWaiting.height = 20;
                    touristsWaiting.padding = new RectOffset(0, 0, 4, 2);
                    touristsWaiting.textScale = 0.7f;
                    touristsWaiting.backgroundSprite = "EmptySprite";
                    touristsWaiting.color = new Color32(0x1f, 0x25, 0x68, 255);
                    touristsWaiting.textAlignment = UIHorizontalAlignment.Center;
                    touristCounters[stationNodeId] = touristsWaiting;
                    //				
                    return normalWidth;
                }
                else
                {
                    return 25f;
                }
            }
            else
            {
                return 30f;
            }

        }





        Vector3 getStation(ushort lineId, ushort stopId, ItemClass.SubService ss, out string stationName, out List<ushort> linhas, out string airport, out string taxiStand, out string prefix)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            ItemClass.Service servFound;
            ItemClass.SubService subServFound;
            ushort buildingId;
            stationName = TLMUtils.getStationName(stopId, lineId, ss, out servFound, out subServFound, out prefix, out buildingId);

            //paradas proximas (metro e trem)
            TransportManager tm = Singleton<TransportManager>.instance;
            TransportInfo thisLineInfo = tm.m_lines.m_buffer[(int)nn.m_transportLine].Info;
            TransportLine thisLine = tm.m_lines.m_buffer[(int)nn.m_transportLine];
            linhas = new List<ushort>();
            Vector3 location = nn.m_position;
            if (buildingId > 0 && ss == subServFound)
            {
                location = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].CalculateSidewalkPosition();
            }
            TLMLineUtils.GetNearLines(location, 100f, ref linhas);

            airport = String.Empty;
            taxiStand = String.Empty;

            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP))
            {
                ushort airportId = bm.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, Building.Flags.None, Building.Flags.Untouchable);

                if (airportId > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = airportId;
                    airport = bm.GetBuildingName(airportId, iid);
                }
            }
            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP))
            {
                ushort taxiId = bm.FindBuilding(location != Vector3.zero ? location : nn.m_position, 50f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTaxi, Building.Flags.None, Building.Flags.Untouchable);

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