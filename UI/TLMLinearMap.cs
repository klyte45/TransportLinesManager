using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.UI
{
    public class TLMLinearMap : MonoBehaviour
    {

        internal ILinearMapParentInterface parent
        {
            private get => m_parent;
            set {
                if (m_parent == null)
                {
                    m_parent = value;
                    createLineStationsLinearView();
                }
            }
        }
        private ILinearMapParentInterface m_parent;
        private UILabel linearMapLineNumberFormat;
        private UILabel linearMapLineNumber;
        private UILabel linearMapLineTime;
        private UIPanel lineStationsPanel;
        private UIPanel mainContainer;
        private string m_autoName;
        private UIButton infoToggle;
        private UIButton distanceToggle;
        private Dictionary<ushort, UILabel> residentCounters = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> touristCounters = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> ttbTimers = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> lineVehicles = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, float> stationOffsetX = new Dictionary<ushort, float>();
        private Dictionary<ushort, int> vehiclesOnStation = new Dictionary<ushort, int>();
        private const float vehicleYoffsetIncrement = -20f;
        private const float vehicleYbaseOffset = -75f;

        private bool showIntersections = true;
        private bool showExtraStopInfo = false;

        public bool getVisible() => mainContainer.isVisible;
        public void setVisible(bool value) => mainContainer.isVisible = value;


        public GameObject containerGameObject
        {
            get {
                try
                {
                    return mainContainer.gameObject;
                }
                catch (Exception e)
                {
                    TLMUtils.doErrorLog(e.ToString());
                    return null;
                }

            }
        }


        public string autoName => m_autoName;

        public void setLinearMapColor(Color c)
        {
            linearMapLineNumberFormat.color = c;
            lineStationsPanel.color = c;
        }

        public void setLineNumberCircle(ushort lineID)
        {
            TLMLineUtils.setLineNumberCircleOnRef(lineID, linearMapLineNumber);
            try
            {
                m_autoName = TLMLineUtils.calculateAutoName(lineID);
                linearMapLineNumber.tooltip = m_autoName;
            }
            catch { }
        }



        public void redrawLine()
        {
            TLMUtils.doLog("init RedrawLine");
            ushort lineID = parent.CurrentSelectedId;
            TransportLine t = TransportManager.instance.m_lines.m_buffer[lineID];
            int stopsCount = t.CountStops(lineID);
            int vehicleCount = t.CountVehicles(lineID);
            Color lineColor = TransportManager.instance.GetLineColor(lineID);
            TLMUtils.doLog("p1");
            setLinearMapColor(lineColor);
            clearStations();
            updateSubIconLayer();
            setLineNumberCircle(lineID);
            TLMUtils.doLog("p2");
            if (lineID == 0)
            {
                var tsd = TransportSystemDefinition.From(parent.CurrentTransportInfo);
                if (tsd != default)
                {

                    linearMapLineNumberFormat.backgroundSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(TLMUtils.GetLineIcon(0, tsd.ToConfigIndex(), ref tsd), true);
                }
                lineStationsPanel.width = 0;
                return;
            }

            TLMUtils.doLog("p3");
            ItemClass.SubService ss = TransportSystemDefinition.GetDefinitionForLine(lineID).SubService;
            linearMapLineNumberFormat.backgroundSprite = TLMLineUtils.getIconForLine(lineID);
            TLMUtils.doLog("p4");
            m_autoName = TLMLineUtils.calculateAutoName(lineID);
            linearMapLineNumber.tooltip = m_autoName;
            string stationName;
            Vector3 local;
            string airport, taxi, harbor, regionalStation, cableCarStation;
            string namePrefix;
            TLMUtils.doLog("p5");
            bool isComplete = (Singleton<TransportManager>.instance.m_lines.m_buffer[TLMController.instance.CurrentSelectedId].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None;
            bool simmetric = TLMLineUtils.CalculateSimmetry(ss, stopsCount, t, out int middle);
            float addedWidth = 0;
            lineStationsPanel.width = 20;
            if (t.Info.m_transportType != TransportInfo.TransportType.Bus && t.Info.m_transportType != TransportInfo.TransportType.Tram && simmetric && !showExtraStopInfo)
            {
                int maxIt = middle + stopsCount / 2;
                for (int j = middle; j <= maxIt; j++)
                {
                    ushort stationId = t.GetStop(j);
                    local = getStation(lineID, stationId, ss, out stationName, out List<ushort> intersections, out airport, out harbor, out taxi, out regionalStation, out cableCarStation, out namePrefix);
                    addedWidth = addStationToLinearMap(namePrefix, stationName, local, lineStationsPanel.width, intersections, airport, harbor, taxi, regionalStation, cableCarStation, stationId, ss, lineColor, false) + (j == middle + stopsCount / 2 ? 5 : 0);
                    lineStationsPanel.width += addedWidth;
                }
            }
            else
            {
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
                    local = getStation(lineID, stationId, ss, out stationName, out List<ushort> intersections, out airport, out harbor, out taxi, out regionalStation, out cableCarStation, out namePrefix);
                    lineStationsPanel.width += addStationToLinearMap(namePrefix, stationName, local, lineStationsPanel.width, intersections, airport, harbor, taxi, regionalStation, cableCarStation, stationId, ss, lineColor, true);
                }
                else if (TransportLinesManagerMod.showDistanceLinearMap || parent.ForceShowStopsDistances)
                {
                    minI--;
                }
                for (int i = minI; i < maxI; i++)
                {
                    int j = (i + stopsCount) % stopsCount;
                    ushort stationId = t.GetStop(j);
                    local = getStation(lineID, stationId, ss, out stationName, out List<ushort> intersections, out airport, out harbor, out taxi, out regionalStation, out cableCarStation, out namePrefix);
                    addedWidth = addStationToLinearMap(namePrefix, stationName, local, lineStationsPanel.width, intersections, airport, harbor, taxi, regionalStation, cableCarStation, stationId, ss, lineColor, false);
                    lineStationsPanel.width += addedWidth;
                }
            }
            lineStationsPanel.width += 20;
            TLMUtils.doLog("p6");
            lineStationsPanel.width -= addedWidth;
            if (showExtraStopInfo)
            {
                vehiclesOnStation.Clear();
                for (int v = 0; v < vehicleCount; v++)
                {
                    ushort vehicleId = t.GetVehicle(v);

                    AddVehicleToLinearMap(lineColor, vehicleId);
                }
            }
            TLMUtils.doLog("end RedrawLine");
        }

        public TransportLine updateSubIconLayer()
        {
            TransportLine t = TransportManager.instance.m_lines.m_buffer[parent.CurrentSelectedId];
            TLMLineUtils.getLineActive(ref t, out bool day, out bool night);
            bool zeroed;
            unchecked
            {
                zeroed = (t.m_flags & (TransportLine.Flags) TLMTransportLineFlags.ZERO_BUDGET_CURRENT) != 0;
            }
            if (!day || !night || zeroed)
            {
                linearMapLineTime.backgroundSprite = zeroed ? "NoBudgetIcon" : day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
            }
            else
            {
                linearMapLineTime.backgroundSprite = "";
            }

            return t;
        }

        private void AddVehicleToLinearMap(Color lineColor, ushort vehicleId)
        {

            TLMLineUtils.GetVehicleCapacityAndFill(vehicleId, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out int fill, out int cap);

            KlyteMonoUtils.CreateUIElement(out UILabel vehicleLabel, lineStationsPanel.transform);
            vehicleLabel.autoSize = false;
            vehicleLabel.text = string.Format("{0}/{1}", fill, cap);
            vehicleLabel.useOutline = true;
            vehicleLabel.width = 50;
            vehicleLabel.height = 33;
            vehicleLabel.pivot = UIPivotPoint.TopCenter;
            vehicleLabel.verticalAlignment = UIVerticalAlignment.Middle;

            vehicleLabel.padding = new RectOffset(0, 0, 2, 0);
            vehicleLabel.textScale = 0.6f;
            vehicleLabel.backgroundSprite = "VehicleLinearMap";
            vehicleLabel.color = lineColor;
            vehicleLabel.textAlignment = UIHorizontalAlignment.Center;
            vehicleLabel.tooltip = Singleton<VehicleManager>.instance.GetVehicleName(vehicleId);

            vehicleLabel.eventClick += (x, y) =>
            {
                InstanceID id = default;
                id.Vehicle = vehicleId;
                Camera.main.GetComponent<CameraController>().SetTarget(id, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].GetLastFramePosition(), true);
            };
            UIDragHandle dh = KlyteMonoUtils.CreateDragHandle(vehicleLabel, vehicleLabel);
            KlyteMonoUtils.CreateUIElement(out DraggableVehicleInfo dvi, vehicleLabel.transform);
            dvi.vehicleId = vehicleId;
            dvi.name = "Vehicle" + vehicleId;

            vehicleLabel.eventMouseLeave += vehicleHover;
            vehicleLabel.eventMouseUp += vehicleHover;
            vehicleLabel.eventMouseDown += vehicleHover;
            vehicleLabel.eventDragStart += draggingVehicle;
            vehicleLabel.eventMouseHover += vehicleHover;

            updateVehiclePosition(vehicleLabel);

            lineVehicles.Add(vehicleId, vehicleLabel);
        }

        private void vehicleHover(UIComponent component, UIMouseEventParameter eventParam)
        {
            bool oldVal = component.GetComponentInChildren<DraggableVehicleInfo>().isDragging;
            bool newVal = (eventParam.buttons & UIMouseButton.Left) != UIMouseButton.None;
            component.GetComponentInChildren<DraggableVehicleInfo>().isDragging = newVal;
            if (oldVal != newVal && newVal == false)
            {
                TLMUtils.doLog("onVehicleDrop! {0}", component.name);
                DraggableVehicleInfo dvi = eventParam.source.parent.GetComponentInChildren<DraggableVehicleInfo>();
                UIView view = GameObject.FindObjectOfType<UIView>();
                PoolList<UIHitInfo> hits = view.RaycastAll(eventParam.ray);
                DroppableStationInfo dsi = null;
                UIComponent res = null;
                int idxRes = -1;
                for (int i = hits.Count - 1; i >= 0; i--)
                {
                    UIHitInfo hit = hits[i];
                    DroppableStationInfo[] dsiList = hit.component.GetComponentsInChildren<DroppableStationInfo>();
                    if (dsiList.Length == 0)
                    {
                        dsiList = hit.component.parent.GetComponentsInChildren<DroppableStationInfo>();
                    }

                    if (dsiList.Length == 1)
                    {
                        dsi = dsiList[0];
                        res = hit.component;
                        idxRes = i;
                        break;
                    }
                }
                if (dvi == null || dsi == null)
                {
                    TLMUtils.doLog("Drag Drop falhou! {0}", eventParam.source.name);
                    return;
                }
                else
                {
                    TLMUtils.doLog("Drag Funcionou! {0}/{1} ({2}-{3})", eventParam.source.name, dsi.gameObject.name, res.gameObject.name, idxRes);
                    var ai = (VehicleAI) Singleton<VehicleManager>.instance.m_vehicles.m_buffer[dvi.vehicleId].Info.GetAI();
                    ai.SetTarget(dvi.vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[dvi.vehicleId], dsi.nodeId);
                }
            }
        }

        private void draggingVehicle(UIComponent component, UIDragEventParameter eventParam) => component.GetComponentInChildren<DraggableVehicleInfo>().isDragging = true;


        private void updateVehiclePosition(UILabel vehicleLabel)
        {
            try
            {
                DraggableVehicleInfo dvi = vehicleLabel.GetComponentInChildren<DraggableVehicleInfo>();
                if (dvi.isDragging)
                {
                    return;
                }
                ushort vehicleId = dvi.vehicleId;
                ushort stopId = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding;
                UILabel labelStation = residentCounters[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding];
                float destX = stationOffsetX[stopId] - labelStation.width;
                if (Singleton<TransportManager>.instance.m_lines.m_buffer[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_transportLine].GetStop(0) == stopId && (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & Vehicle.Flags.Stopped) != 0)
                {
                    destX = stationOffsetX[TransportLine.GetPrevStop(stopId)];
                }
                float yOffset = vehicleYbaseOffset;
                int busesOnStation = vehiclesOnStation.ContainsKey(stopId) ? vehiclesOnStation[stopId] : 0;
                if ((Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & Vehicle.Flags.Stopped) != 0)
                {
                    ushort prevStop = TransportLine.GetPrevStop(stopId);
                    destX -= labelStation.width / 2;
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
            catch (Exception e)
            {
                TLMUtils.doLog("ERROR UPDATING VEHICLE!!!");
                TLMUtils.doErrorLog(e.ToString());
                //redrawLine();
            }
        }

        private void clearStations()
        {
            GameObject.Destroy(lineStationsPanel.gameObject);
            residentCounters.Clear();
            touristCounters.Clear();
            lineVehicles.Clear();
            stationOffsetX.Clear();
            createLineStationsPanel();
        }

        private void createLineStationsLinearView()
        {
            KlyteMonoUtils.CreateUIElement(out mainContainer, parent.TransformLinearMap);
            mainContainer.absolutePosition = new Vector3(2f, FindObjectOfType<UIView>().fixedHeight - 300f);
            mainContainer.name = "LineStationsLinearView";
            mainContainer.height = 50;
            mainContainer.autoSize = true;

            KlyteMonoUtils.CreateUIElement(out linearMapLineNumberFormat, mainContainer.transform);
            linearMapLineNumberFormat.autoSize = false;
            linearMapLineNumberFormat.width = 50;
            linearMapLineNumberFormat.height = 50;
            linearMapLineNumberFormat.color = new Color(1, 0, 0, 1);
            linearMapLineNumberFormat.pivot = UIPivotPoint.MiddleLeft;
            linearMapLineNumberFormat.textAlignment = UIHorizontalAlignment.Center;
            linearMapLineNumberFormat.verticalAlignment = UIVerticalAlignment.Middle;
            linearMapLineNumberFormat.name = "LineFormat";
            linearMapLineNumberFormat.relativePosition = new Vector3(0f, 0f);
            KlyteMonoUtils.CreateDragHandle(linearMapLineNumberFormat, mainContainer);




            KlyteMonoUtils.CreateUIElement(out linearMapLineNumber, linearMapLineNumberFormat.transform);

            linearMapLineNumber.autoSize = false;
            linearMapLineNumber.width = linearMapLineNumberFormat.width;
            linearMapLineNumber.pivot = UIPivotPoint.MiddleCenter;
            linearMapLineNumber.name = "LineNumber";
            linearMapLineNumber.width = 50;
            linearMapLineNumber.height = 50;
            linearMapLineNumber.relativePosition = new Vector3(-0.5f, 0.5f);
            linearMapLineNumber.autoHeight = false;
            linearMapLineNumber.textAlignment = UIHorizontalAlignment.Center;
            linearMapLineNumber.verticalAlignment = UIVerticalAlignment.Middle;


            KlyteMonoUtils.CreateUIElement(out linearMapLineTime, linearMapLineNumberFormat.transform);
            linearMapLineTime.autoSize = false;
            linearMapLineTime.width = 50;
            linearMapLineTime.height = 50;
            linearMapLineTime.color = new Color(1, 1, 1, 1);
            linearMapLineTime.pivot = UIPivotPoint.MiddleLeft;
            linearMapLineTime.textAlignment = UIHorizontalAlignment.Center;
            linearMapLineTime.verticalAlignment = UIVerticalAlignment.Middle;
            linearMapLineTime.name = "LineTime";
            linearMapLineTime.relativePosition = new Vector3(0f, 0f);

            //if (parent.PrefixSelector)
            //{
            //    prefixSelector = UIHelperExtension.CloneBasicDropDownNoLabel(new string[] { "/", "B" }, (y) =>
            //    {
            //        SetSelectedPrefix(y);
            //    }, linearMapLineNumberFormat
            //    );
            //    prefixSelector.autoSize = false;
            //    prefixSelector.width = linearMapLineNumberFormat.width;
            //    prefixSelector.pivot = UIPivotPoint.MiddleCenter;
            //    prefixSelector.name = "LinePrefixSelector";
            //    prefixSelector.width = 50;
            //    prefixSelector.height = 50;
            //    prefixSelector.relativePosition = new Vector3(-0.5f, 0.5f);
            //    prefixSelector.textScale = 1;
            //    prefixSelector.textFieldPadding.top = 999999;
            //    prefixSelector.textFieldPadding.bottom = 999999;
            //    prefixSelector.textFieldPadding.left = 999999;
            //    prefixSelector.textFieldPadding.right = 999999;
            //    prefixSelector.normalBgSprite = null;
            //    prefixSelector.hoveredBgSprite = null;
            //    prefixSelector.focusedBgSprite = null;
            //    prefixSelector.zOrder = 999;
            //    var dragH = KlyteMonoUtils.CreateDragHandle(prefixSelector, mainContainer);

            //    dragH.eventClicked += (x, y) =>
            //    {
            //        prefixSelector.SimulateClick();
            //    };
            //    TransportManagerOverrides.OnLineRelease += () =>
            //        {
            //            TLMUtils.doLog("OnLineRelease");
            //            if (isVisible)
            //            {
            //                SetSelectedPrefix(prefixSelector.selectedIndex);
            //                UpdatePrefixSelector();
            //            }
            //        };

            //}

            if (parent.CanSwitchView)
            {
                KlyteMonoUtils.CreateUIElement(out infoToggle, mainContainer.transform);
                KlyteMonoUtils.InitButton(infoToggle, true, "ButtonMenu");
                infoToggle.relativePosition = new Vector3(0f, 60f);
                infoToggle.width = 50;
                infoToggle.height = 70;
                infoToggle.wordWrap = true;
                infoToggle.localeID = "K45_TLM_SHOW_EXTRA_INFO";
                infoToggle.isLocalized = true;
                infoToggle.textScale = 0.8f;
                infoToggle.eventClick += (x, y) =>
                {
                    showIntersections = !showIntersections;
                    showExtraStopInfo = !showIntersections;
                    if (showIntersections)
                    {
                        infoToggle.localeID = "K45_TLM_SHOW_EXTRA_INFO";
                        distanceToggle.isVisible = true;
                    }
                    else
                    {
                        infoToggle.localeID = "K45_TLM_SHOW_LINE_INTEGRATION_SHORT";
                        distanceToggle.isVisible = false;
                    }
                    redrawLine();
                };


                KlyteMonoUtils.CreateUIElement(out distanceToggle, mainContainer.transform);
                KlyteMonoUtils.InitButton(distanceToggle, true, "ButtonMenu");
                distanceToggle.relativePosition = new Vector3(0f, 135f);
                distanceToggle.width = 50;
                distanceToggle.height = 20;
                distanceToggle.wordWrap = true;
                distanceToggle.tooltipLocaleID = "K45_TLM_TOGGLE_DISTANCE_LINEAR_MAP";
                distanceToggle.isTooltipLocalized = true;
                distanceToggle.textScale = 0.8f;
                distanceToggle.text = "Δd";
                distanceToggle.eventClick += (x, y) =>
                {
                    TransportLinesManagerMod.showDistanceLinearMap = !TransportLinesManagerMod.showDistanceLinearMap;
                    redrawLine();
                };
            }

            createLineStationsPanel();
        }

        public void updateBidings()
        {
            if (showExtraStopInfo)
            {
                foreach (KeyValuePair<ushort, UILabel> resLabel in residentCounters)
                {
                    TLMLineUtils.GetQuantityPassengerWaiting(resLabel.Key, out int residents, out int tourists, out int ttb);
                    resLabel.Value.text = residents.ToString();
                    touristCounters[resLabel.Key].text = tourists.ToString();
                    ttbTimers[resLabel.Key].text = ttb.ToString();
                    ttbTimers[resLabel.Key].color = getColorForTTB(ttb);
                }
                ushort lineID = parent.CurrentSelectedId;
                TransportLine t = TransportManager.instance.m_lines.m_buffer[lineID];
                Color lineColor = TransportManager.instance.GetLineColor(lineID);
                int vehicleCount = t.CountVehicles(lineID);
                var oldItems = lineVehicles.Keys.ToList();
                vehiclesOnStation.Clear();
                for (int v = 0; v < vehicleCount; v++)
                {
                    ushort vehicleId = t.GetVehicle(v);
                    UILabel vehicleLabel = null;

                    if (oldItems.Contains(vehicleId))
                    {
                        vehicleLabel = lineVehicles[vehicleId];
                        TLMLineUtils.GetVehicleCapacityAndFill(vehicleId, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out int fill, out int cap);
                        vehicleLabel.text = string.Format("{0}/{1}", fill, cap);
                        UILabel labelStation = residentCounters[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding];
                        updateVehiclePosition(vehicleLabel);
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


        private Color32 getColorForTTB(int ttb)
        {
            if (ttb > 200)
            {
                return Color.green;
            }
            else if (ttb > 150)
            {
                return Color.Lerp(Color.yellow, Color.green, (ttb - 150) / 50f);
            }
            else if (ttb > 50)
            {
                return Color.Lerp(Color.red, Color.yellow, (ttb - 50) / 100f);
            }
            else
            {
                return Color.red;
            }
        }

        private void createLineStationsPanel()
        {

            KlyteMonoUtils.CreateUIElement(out lineStationsPanel, mainContainer.transform);
            lineStationsPanel.width = 140;
            lineStationsPanel.height = 25;
            lineStationsPanel.name = "LineStationsPanel";
            lineStationsPanel.autoLayout = false;
            lineStationsPanel.useCenter = true;
            lineStationsPanel.wrapLayout = false;
            lineStationsPanel.backgroundSprite = "PlainWhite";
            lineStationsPanel.pivot = UIPivotPoint.MiddleLeft;
            lineStationsPanel.relativePosition = new Vector3(75f, 10f);
            lineStationsPanel.color = TransportManager.instance.GetLineColor(parent.CurrentSelectedId);
        }

        private float addStationToLinearMap(string stationPrefix, string stationName, Vector3 location, float offsetX, List<ushort> intersections,
            string airport, string harbor, string taxi, string regionalTrainStation, string cableCarStation,
            ushort stationNodeId, ItemClass.SubService ss, Color lineColor, bool simple)//, out float intersectionPanelHeight)
        {
            ushort lineID = parent.CurrentSelectedId;
            TransportLine t = TransportManager.instance.m_lines.m_buffer[lineID];
            TransportManager tm = Singleton<TransportManager>.instance;

            if (stationName == null)
            {
                stationName = "???";
            }

            KlyteMonoUtils.CreateUIElement(out UIButton stationButton, lineStationsPanel.transform);
            stationButton.relativePosition = new Vector3(offsetX - 10, 13f);
            stationButton.width = 25;
            stationButton.height = 25;
            stationButton.color = Color.white;
            stationButton.name = "Station [" + stationName + "]";
            stationButton.tooltip = stationName + (TransportLinesManagerMod.DebugMode ? "(id:" + stationNodeId + ")" : "");
            KlyteMonoUtils.InitButton(stationButton, true, "DistrictOptionBrushMedium");

            KlyteMonoUtils.CreateUIElement(out DroppableStationInfo dsi, stationButton.transform);
            dsi.nodeId = stationNodeId;
            dsi.name = "DSI Station [" + stationName + "] - " + stationNodeId;

            KlyteMonoUtils.CreateUIElement(out UITextField stationLabel, stationButton.transform);
            stationLabel.autoSize = true;
            stationLabel.width = 220;
            stationLabel.height = 20;
            stationLabel.useOutline = true;
            stationLabel.pivot = UIPivotPoint.MiddleLeft;
            stationLabel.horizontalAlignment = UIHorizontalAlignment.Left;
            stationLabel.verticalAlignment = UIVerticalAlignment.Middle;
            stationLabel.name = "Station [" + stationName + "] Name";
            stationLabel.relativePosition = new Vector3(28f, -13f);
            stationLabel.text = (!string.IsNullOrEmpty(stationPrefix) ? stationPrefix.Trim() + " " : "") + stationName.Trim();
            stationLabel.textScale = Math.Max(0.5f, Math.Min(1, 24f / stationLabel.text.Length));

            KlyteMonoUtils.UiTextFieldDefaults(stationLabel);
            stationLabel.color = new Color(0.3f, 0.3f, 0.3f, 1);
            stationLabel.textColor = Color.white;
            stationLabel.cursorWidth = 2;
            stationLabel.cursorBlinkTime = 100;
            stationLabel.eventGotFocus += (x, y) =>
            {
                stationLabel.text = TLMLineUtils.getStationName(stationNodeId, lineID, ss);
            };
            stationLabel.eventTextSubmitted += (x, y) =>
            {
                TLMLineUtils.setStopName(y, stationNodeId, lineID, () =>
                {
                    stationLabel.text = TLMLineUtils.getFullStationName(stationNodeId, lineID, ss);
                    m_autoName = TLMLineUtils.calculateAutoName(lineID);
                    parent.OnRenameStationAction(autoName);
                });
            };

            stationButton.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            stationButton.gameObject.transform.localEulerAngles = new Vector3(0, 0, 45);
            stationButton.eventClick += (component, eventParam) =>
            {
                var gameObject = GameObject.FindGameObjectWithTag("MainCamera");
                if (gameObject != null)
                {
                    CameraController cameraController = gameObject.GetComponent<CameraController>();
                    InstanceID x = default;
                    x.TransportLine = parent.CurrentSelectedId;
                    cameraController.SetTarget(x, location, false);
                    cameraController.ClearTarget();
                }

            };
            if (!simple)
            {
                if (!stationOffsetX.ContainsKey(stationNodeId))
                {
                    stationOffsetX.Add(stationNodeId, offsetX);
                }
                if (showIntersections)
                {
                    Dictionary<string, ushort> otherLinesIntersections = TLMLineUtils.SortLines(intersections, t);
                    UILabel distance = null;
                    int intersectionCount = otherLinesIntersections.Count + (airport != string.Empty ? 1 : 0) + (taxi != string.Empty ? 1 : 0) + (harbor != string.Empty ? 1 : 0) + (regionalTrainStation != string.Empty ? 1 : 0) + (cableCarStation != string.Empty ? 1 : 0);

                    if ((TransportLinesManagerMod.showDistanceLinearMap || parent.ForceShowStopsDistances) && offsetX > 20)
                    {
                        NetSegment seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment0];
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment1];
                        }
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment2];
                        }
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment3];
                        }
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment4];
                        }
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment5];
                        }
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment6];
                        }
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = Singleton<NetManager>.instance.m_segments.m_buffer[Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId].m_segment7];
                        }
                        if (seg.m_endNode != stationNodeId)
                        {
                            seg = default;
                        }
                        KlyteMonoUtils.CreateUIElement(out UIPanel distContainer, stationButton.transform);
                        distContainer.size = new Vector2(0, 0);
                        distContainer.relativePosition = new Vector3(0, 0, 0);
                        KlyteMonoUtils.CreateUIElement(out distance, distContainer.transform);
                        distance.autoSize = false;
                        distance.useOutline = true;
                        if (seg.Equals(default(NetSegment)))
                        {
                            distance.text = "???";
                            distance.color = Color.red;
                        }
                        else
                        {
                            distance.text = (int) seg.m_averageLength + "m";
                        }
                        distance.textScale = 0.75f;
                        distance.textAlignment = UIHorizontalAlignment.Center;
                        distance.verticalAlignment = UIVerticalAlignment.Middle;
                        distance.name = "dist.";
                        distance.font = UIHelperExtension.defaultFontCheckbox;
                        distance.width = 50f;
                        distance.height = 50;
                        distance.relativePosition = new Vector3(-25, 25);
                        distance.transform.localEulerAngles = new Vector3(0, 0, 90);
                        distance.isInteractive = false;
                    }

                    if (intersectionCount > 0)
                    {
                        KlyteMonoUtils.CreateUIElement(out UIPanel intersectionsPanel, stationButton.transform);
                        intersectionsPanel.autoSize = false;
                        intersectionsPanel.autoLayout = false;
                        intersectionsPanel.autoLayoutStart = LayoutStart.TopLeft;
                        intersectionsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                        intersectionsPanel.relativePosition = new Vector3(-15f, 10);
                        intersectionsPanel.wrapLayout = false;
                        intersectionsPanel.autoFitChildrenVertically = true;

                        TLMLineUtils.PrintIntersections(airport, harbor, taxi, regionalTrainStation, cableCarStation, intersectionsPanel, otherLinesIntersections, Vector3.zero);
                        intersectionsPanel.autoLayout = true;
                        intersectionsPanel.wrapLayout = true;
                        intersectionsPanel.width = 55;
                        //		
                        return 42f;
                    }
                    else
                    {
                        if (offsetX == 0)
                        {
                            stationButton.relativePosition = new Vector3(offsetX - 18, 15f);
                            return 36f;
                        }
                        else if (distance == null)
                        {
                            stationButton.relativePosition = new Vector3(offsetX - 28, 15f);
                            return 26f;
                        }
                        else
                        {
                            return 47f;
                        }
                    }
                }
                else if (showExtraStopInfo)
                {
                    float normalWidth = 42.5f;

                    NetNode stopNode = Singleton<NetManager>.instance.m_nodes.m_buffer[stationNodeId];

                    TLMLineUtils.GetQuantityPassengerWaiting(stationNodeId, out int residents, out int tourists, out int ttb);

                    KlyteMonoUtils.CreateUIElement(out UIPanel stationInfoStatsPanel, stationButton.transform);
                    stationInfoStatsPanel.autoSize = false;
                    stationInfoStatsPanel.autoLayout = false;
                    stationInfoStatsPanel.autoFitChildrenVertically = true;
                    stationInfoStatsPanel.autoLayoutStart = LayoutStart.TopLeft;
                    stationInfoStatsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                    stationInfoStatsPanel.relativePosition = new Vector3(-20, 10);
                    stationInfoStatsPanel.autoLayout = true;
                    stationInfoStatsPanel.wrapLayout = true;
                    stationInfoStatsPanel.width = normalWidth;

                    KlyteMonoUtils.CreateUIElement(out UILabel residentsWaiting, stationInfoStatsPanel.transform);
                    residentsWaiting.autoSize = false;
                    residentsWaiting.useOutline = true;
                    residentsWaiting.text = residents.ToString();
                    residentsWaiting.tooltipLocaleID = "K45_TLM_RESIDENTS_WAITING";
                    residentsWaiting.backgroundSprite = "EmptySprite";
                    residentsWaiting.color = new Color32(0x12, 0x68, 0x34, 255);
                    residentsWaiting.width = normalWidth;
                    residentsWaiting.padding = new RectOffset(0, 0, 4, 2);
                    residentsWaiting.height = 20;
                    residentsWaiting.textScale = 0.7f;
                    residentsWaiting.textAlignment = UIHorizontalAlignment.Center;
                    residentCounters[stationNodeId] = residentsWaiting;

                    KlyteMonoUtils.CreateUIElement(out UILabel touristsWaiting, stationInfoStatsPanel.transform);
                    touristsWaiting.autoSize = false;
                    touristsWaiting.text = tourists.ToString();
                    touristsWaiting.tooltipLocaleID = "K45_TLM_TOURISTS_WAITING";
                    touristsWaiting.useOutline = true;
                    touristsWaiting.width = normalWidth;
                    touristsWaiting.height = 20;
                    touristsWaiting.padding = new RectOffset(0, 0, 4, 2);
                    touristsWaiting.textScale = 0.7f;
                    touristsWaiting.backgroundSprite = "EmptySprite";
                    touristsWaiting.color = new Color32(0x1f, 0x25, 0x68, 255);
                    touristsWaiting.textAlignment = UIHorizontalAlignment.Center;
                    touristCounters[stationNodeId] = touristsWaiting;

                    KlyteMonoUtils.CreateUIElement(out UILabel timeTilBored, stationInfoStatsPanel.transform);
                    timeTilBored.autoSize = false;
                    timeTilBored.text = tourists.ToString();
                    timeTilBored.tooltipLocaleID = "K45_TLM_TIME_TIL_BORED";
                    timeTilBored.useOutline = true;
                    timeTilBored.width = normalWidth;
                    timeTilBored.height = 20;
                    timeTilBored.padding = new RectOffset(0, 0, 4, 2);
                    timeTilBored.textScale = 0.7f;
                    timeTilBored.backgroundSprite = "EmptySprite";
                    timeTilBored.color = new Color32(0x1f, 0x25, 0x68, 255);
                    timeTilBored.textAlignment = UIHorizontalAlignment.Center;
                    ttbTimers[stationNodeId] = timeTilBored;
                    //				
                    return normalWidth;
                }
                else
                {
                    return 30f;
                }
            }
            else
            {
                return 30f;
            }

        }

        private Vector3 getStation(ushort lineId, ushort stopId, ItemClass.SubService ss, out string stationName, out List<ushort> linhas, out string airport, out string harbor, out string taxiStand, out string regionalTrainStation, out string cableCarStation, out string prefix)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[stopId];
            stationName = TLMLineUtils.getStationName(stopId, lineId, ss, out ItemClass.Service servFound, out ItemClass.SubService subServFound, out prefix, out ushort buildingId, out NamingType namingType);

            //paradas proximas (metro e trem)
            TransportManager tm = Singleton<TransportManager>.instance;
            TransportInfo thisLineInfo = tm.m_lines.m_buffer[nn.m_transportLine].Info;
            TransportLine thisLine = tm.m_lines.m_buffer[nn.m_transportLine];
            linhas = new List<ushort>();
            Vector3 location = nn.m_position;
            if (buildingId > 0 && ss == subServFound)
            {
                location = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].CalculateSidewalkPosition();
            }
            TLMLineUtils.GetNearLines(location, 120f, ref linhas);

            airport = string.Empty;
            taxiStand = string.Empty;
            harbor = string.Empty;
            regionalTrainStation = string.Empty;
            cableCarStation = string.Empty;

            if (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP))
            {
                ushort trainStation = BuildingUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, null, Building.Flags.None, Building.Flags.Untouchable | Building.Flags.Downgrading);

                if (trainStation > 0)
                {
                    InstanceID iid = default;
                    iid.Building = trainStation;
                    regionalTrainStation = bm.GetBuildingName(trainStation, iid);
                }
            }

            if (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP))
            {
                ushort airportId = BuildingUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerPlane }, Building.Flags.None, Building.Flags.Untouchable);

                if (airportId > 0)
                {
                    InstanceID iid = default;
                    iid.Building = airportId;
                    airport = bm.GetBuildingName(airportId, iid);
                }
            }

            if (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP))
            {
                ushort harborId = BuildingUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerShip }, Building.Flags.None, Building.Flags.Untouchable);

                if (harborId > 0)
                {
                    InstanceID iid = default;
                    iid.Building = harborId;
                    harbor = bm.GetBuildingName(harborId, iid);
                }
            }
            if (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP))
            {
                ushort taxiId = BuildingUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 50f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTaxi, null, Building.Flags.None, Building.Flags.Untouchable);

                if (taxiId > 0)
                {
                    InstanceID iid = default;
                    iid.Building = taxiId;
                    taxiStand = bm.GetBuildingName(taxiId, iid);
                }
            }
            if (TLMCW.GetCurrentConfigBool(TLMCW.ConfigIndex.CABLE_CAR_SHOW_IN_LINEAR_MAP))
            {
                ushort cableCarId = BuildingUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportCableCar, null, Building.Flags.None, Building.Flags.Untouchable);

                if (cableCarId > 0)
                {
                    InstanceID iid = default;
                    iid.Building = cableCarId;
                    cableCarStation = bm.GetBuildingName(cableCarId, iid);
                }
            }


            return location;
        }

        private class DraggableVehicleInfo : UIComponent
        {
            public ushort vehicleId;
            public bool isDragging = false;
        }
        private class DroppableStationInfo : UIComponent
        {
            public ushort nodeId;
        }

    }
}