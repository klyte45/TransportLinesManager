using ColossalFramework;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.LineList;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;
using Klyte.TransportLinesManager.Utils;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Extensors;
using System.Reflection;
using Klyte.TransportLinesManager.Overrides;

namespace Klyte.TransportLinesManager.UI
{
    public class TLMLinearMap
    {

        private LinearMapParentInterface parent;
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
        private UIButton distanceToggle;
        private Dictionary<ushort, UILabel> residentCounters = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> touristCounters = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> ttbTimers = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, UILabel> lineVehicles = new Dictionary<ushort, UILabel>();
        private Dictionary<ushort, float> stationOffsetX = new Dictionary<ushort, float>();
        private Dictionary<ushort, int> vehiclesOnStation = new Dictionary<ushort, int>();
        private const float vehicleYoffsetIncrement = -20f;
        private const float vehicleYbaseOffset = -75f;
        private TransportInfo.TransportType lastType = (TransportInfo.TransportType)(-1);

        private bool showIntersections = true;
        private bool showExtraStopInfo = false;

        public bool isVisible
        {
            get {
                return mainContainer.isVisible;
            }
            set {
                mainContainer.isVisible = value;
            }
        }

        public GameObject gameObject
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


        public string autoName
        {
            get {
                ushort lineID = parent.CurrentSelectedId;
                TransportLine t = TLMController.instance.tm.m_lines.m_buffer[(int)lineID];
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

        public TLMLinearMap(LinearMapParentInterface lip)
        {
            parent = lip;
            createLineStationsLinearView();
        }

        public void setLinearMapColor(Color c)
        {
            linearMapLineNumberFormat.color = c;
            linearMapLineNumber.textColor = TLMUtils.contrastColor(c);
            lineStationsPanel.color = c;
        }

        public void setLineNumberCircle(ushort lineID)
        {
            TLMLineUtils.setLineNumberCircleOnRef(lineID, linearMapLineNumber);
        }



        public void redrawLine()
        {
            ushort lineID = parent.CurrentSelectedId;
            TransportLine t = TLMController.instance.tm.m_lines.m_buffer[(int)lineID];
            int stopsCount = t.CountStops(lineID);
            int vehicleCount = t.CountVehicles(lineID);
            Color lineColor = TLMController.instance.tm.GetLineColor(lineID);
            setLinearMapColor(lineColor);
            clearStations();
            TLMLineUtils.getLineActive(ref t, out bool day, out bool night);
            if (!day || !night)
            {
                linearMapLineTime.backgroundSprite = day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
            }
            else
            {
                linearMapLineTime.backgroundSprite = "";
            }
            setLineNumberCircle(lineID);
            if (lineID == 0)
            {
                var tsd = TransportSystemDefinition.from(parent.CurrentTransportInfo);
                if (tsd != default(TransportSystemDefinition))
                {
                    linearMapLineNumberFormat.backgroundSprite = TLMLineUtils.GetIconForIndex(tsd.toConfigIndex());
                }
                lineStationsPanel.width = 0;
                return;
            }

            ItemClass.SubService ss = TLMLineUtils.getLineNamingParameters(lineID, out prefix, out sep, out suffix, out nonPrefix, out zerosEsquerda, out invertPrefixSuffix, out string bgSprite).subService;
            linearMapLineNumberFormat.backgroundSprite = bgSprite;
            m_autoName = TLMUtils.calculateAutoName(lineID);
            linearMapLineNumber.tooltip = m_autoName;
            string stationName = null;
            Vector3 local;
            string airport, taxi, harbor, regionalStation, cableCarStation;
            string namePrefix;
            bool isComplete = (Singleton<TransportManager>.instance.m_lines.m_buffer[TLMController.instance.CurrentSelectedId].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None;
            bool simmetric = TLMUtils.CalculateSimmetry(ss, stopsCount, t, out int middle);
            float addedWidth = 0;
            lineStationsPanel.width = 0;
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
                else if (TransportLinesManagerMod.showDistanceInLinearMap || parent.ForceShowStopsDistances)
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

        }

        private void AddVehicleToLinearMap(Color lineColor, ushort vehicleId)
        {

            UILabel vehicleLabel = null;
            TLMLineUtils.GetVehicleCapacityAndFill(vehicleId, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out int fill, out int cap);

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
            UIDragHandle dh = TLMUtils.createDragHandle(vehicleLabel, vehicleLabel);
            DraggableVehicleInfo dvi = null;
            TLMUtils.createUIElement<DraggableVehicleInfo>(ref dvi, vehicleLabel.transform);
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
                UIHitInfo[] hits = view.RaycastAll(eventParam.ray);
                DroppableStationInfo dsi = null;
                UIComponent res = null;
                int idxRes = -1;
                for (int i = hits.Length - 1; i >= 0; i--)
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
                    VehicleAI ai = (VehicleAI)Singleton<VehicleManager>.instance.m_vehicles.m_buffer[dvi.vehicleId].Info.GetAI();
                    ai.SetTarget(dvi.vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[dvi.vehicleId], dsi.nodeId);
                }
            }
        }

        private void draggingVehicle(UIComponent component, UIDragEventParameter eventParam)
        {
            component.GetComponentInChildren<DraggableVehicleInfo>().isDragging = true;
        }


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
                var labelStation = residentCounters[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding];
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
            UnityEngine.Object.Destroy(lineStationsPanel.gameObject);
            residentCounters.Clear();
            touristCounters.Clear();
            lineVehicles.Clear();
            stationOffsetX.Clear();
            createLineStationsPanel();
        }

        private void createLineStationsLinearView()
        {
            TLMUtils.createUIElement<UIPanel>(ref mainContainer, parent.TransformLinearMap);
            mainContainer.absolutePosition = new Vector3(2f, TLMController.instance.uiView.fixedHeight - 300f);
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
            linearMapLineNumber.width = linearMapLineNumberFormat.width;
            linearMapLineNumber.pivot = UIPivotPoint.MiddleCenter;
            linearMapLineNumber.name = "LineNumber";
            linearMapLineNumber.width = 50;
            linearMapLineNumber.height = 50;
            linearMapLineNumber.relativePosition = new Vector3(-0.5f, 0.5f);
            linearMapLineNumber.autoHeight = false;
            linearMapLineNumber.textAlignment = UIHorizontalAlignment.Center;
            linearMapLineNumber.verticalAlignment = UIVerticalAlignment.Middle;


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
            //    var dragH = TLMUtils.createDragHandle(prefixSelector, mainContainer);

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
                        distanceToggle.isVisible = true;
                    }
                    else
                    {
                        infoToggle.localeID = "TLM_SHOW_LINE_INTEGRATION_SHORT";
                        distanceToggle.isVisible = false;
                    }
                    redrawLine();
                };


                TLMUtils.createUIElement<UIButton>(ref distanceToggle, mainContainer.transform);
                TLMUtils.initButton(distanceToggle, true, "ButtonMenu");
                distanceToggle.relativePosition = new Vector3(0f, 135f);
                distanceToggle.width = 50;
                distanceToggle.height = 20;
                distanceToggle.wordWrap = true;
                distanceToggle.tooltipLocaleID = "TLM_TOGGLE_DISTANCE_LINEAR_MAP";
                distanceToggle.isTooltipLocalized = true;
                distanceToggle.textScale = 0.8f;
                distanceToggle.text = "Δd";
                distanceToggle.eventClick += (x, y) =>
                {
                    TransportLinesManagerMod.showDistanceInLinearMap = !TransportLinesManagerMod.showDistanceInLinearMap;
                    redrawLine();
                };
            }

            createLineStationsPanel();
        }

        private void SetSelectedPrefix(int y)
        {
            setLineNumberCircle(parent.CurrentSelectedId);
            FieldInfo lineNumberFieldArray = typeof(TransportManager).GetField("m_lineNumber", RedirectorUtils.allFlags);
            TransportManager tmInstance = Singleton<TransportManager>.instance;
            ((ushort[])lineNumberFieldArray.GetValue(tmInstance))[(int)parent.CurrentTransportInfo.m_transportType] = TLMUtils.GetFirstEmptyValueForPrefix(parent.CurrentTransportInfo, y);
        }

        public void updateBidings()
        {
            if (showExtraStopInfo)
            {
                foreach (var resLabel in residentCounters)
                {
                    TLMLineUtils.GetQuantityPassengerWaiting(resLabel.Key, out int residents, out int tourists, out int ttb);
                    resLabel.Value.text = residents.ToString();
                    touristCounters[resLabel.Key].text = tourists.ToString();
                    ttbTimers[resLabel.Key].text = ttb.ToString();
                    ttbTimers[resLabel.Key].color = getColorForTTB(ttb);
                }
                ushort lineID = parent.CurrentSelectedId;
                TransportLine t = TLMController.instance.tm.m_lines.m_buffer[(int)lineID];
                Color lineColor = TLMController.instance.tm.GetLineColor(lineID);
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
                        TLMLineUtils.GetVehicleCapacityAndFill(vehicleId, Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out int fill, out int cap);
                        vehicleLabel.text = string.Format("{0}/{1}", fill, cap);
                        var labelStation = residentCounters[Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding];
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

            TLMUtils.createUIElement<UIPanel>(ref lineStationsPanel, mainContainer.transform);
            lineStationsPanel.width = 140;
            lineStationsPanel.height = 30;
            lineStationsPanel.name = "LineStationsPanel";
            lineStationsPanel.autoLayout = false;
            lineStationsPanel.useCenter = true;
            lineStationsPanel.wrapLayout = false;
            lineStationsPanel.atlas = TLMController.taLineNumber;
            lineStationsPanel.backgroundSprite = "LinearBg";
            lineStationsPanel.pivot = UIPivotPoint.MiddleLeft;
            lineStationsPanel.relativePosition = new Vector3(75f, 10f);
            lineStationsPanel.color = TLMController.instance.tm.GetLineColor(parent.CurrentSelectedId);
        }

        private float addStationToLinearMap(string stationPrefix, string stationName, Vector3 location, float offsetX, List<ushort> intersections,
            string airport, string harbor, string taxi, string regionalTrainStation, string cableCarStation,
            ushort stationNodeId, ItemClass.SubService ss, Color lineColor, bool simple)//, out float intersectionPanelHeight)
        {
            ushort lineID = parent.CurrentSelectedId;
            TransportLine t = TLMController.instance.tm.m_lines.m_buffer[(int)lineID];
            TransportManager tm = Singleton<TransportManager>.instance;


            UIButton stationButton = null;
            TLMUtils.createUIElement<UIButton>(ref stationButton, lineStationsPanel.transform);
            stationButton.relativePosition = new Vector3(offsetX - 13, 15f);
            stationButton.width = 20;
            stationButton.height = 20;
            stationButton.color = lineColor;
            stationButton.name = "Station [" + stationName + "]";
            stationButton.atlas = TLMController.taLineNumber;
            stationButton.tooltip = stationName + "(id:" + stationNodeId + ")";
            TLMUtils.initButton(stationButton, true, "LinearStation");

            DroppableStationInfo dsi = null;
            TLMUtils.createUIElement<DroppableStationInfo>(ref dsi, stationButton.transform);
            dsi.nodeId = stationNodeId;
            dsi.name = "DSI Station [" + stationName + "] - " + stationNodeId;

            UITextField stationLabel = null;
            TLMUtils.createUIElement<UITextField>(ref stationLabel, stationButton.transform);
            stationLabel.autoSize = true;
            stationLabel.width = 220;
            stationLabel.height = 20;
            stationLabel.useOutline = true;
            stationLabel.pivot = UIPivotPoint.MiddleLeft;
            stationLabel.horizontalAlignment = UIHorizontalAlignment.Left;
            stationLabel.verticalAlignment = UIVerticalAlignment.Middle;
            stationLabel.name = "Station [" + stationName + "] Name";
            stationLabel.relativePosition = new Vector3(23f, -13f);
            stationLabel.text = (!string.IsNullOrEmpty(stationPrefix) ? stationPrefix.Trim() + " " : "") + stationName.Trim();
            stationLabel.textScale = Math.Max(0.5f, Math.Min(1, 24f / stationLabel.text.Length));

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
                    parent.OnRenameStationAction(autoName);
                });
            };

            stationButton.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            stationButton.gameObject.transform.localEulerAngles = new Vector3(0, 0, 45);
            stationButton.eventClick += (component, eventParam) =>
            {
                GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
                if (gameObject != null)
                {
                    var cameraController = gameObject.GetComponent<CameraController>();
                    InstanceID x = default(InstanceID);
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
                    var otherLinesIntersections = TLMLineUtils.SortLines(intersections, t);
                    UILabel distance = null;
                    int intersectionCount = otherLinesIntersections.Count + (airport != string.Empty ? 1 : 0) + (taxi != string.Empty ? 1 : 0) + (harbor != string.Empty ? 1 : 0) + (regionalTrainStation != string.Empty ? 1 : 0) + (cableCarStation != string.Empty ? 1 : 0);

                    if ((TransportLinesManagerMod.showDistanceInLinearMap || parent.ForceShowStopsDistances) && offsetX > 0)
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
                            seg = default(NetSegment);
                        }
                        UIPanel distContainer = null;
                        TLMUtils.createUIElement<UIPanel>(ref distContainer, stationButton.transform);
                        distContainer.size = new Vector2(0, 0);
                        distContainer.relativePosition = new Vector3(0, 0, 0);
                        TLMUtils.createUIElement<UILabel>(ref distance, distContainer.transform);
                        distance.autoSize = false;
                        distance.useOutline = true;
                        if (seg.Equals(default(NetSegment)))
                        {
                            distance.text = "???";
                            distance.color = Color.red;
                        }
                        else
                        {
                            distance.text = (int)seg.m_averageLength + "m";
                        }
                        distance.textScale = 0.7f;
                        distance.textAlignment = UIHorizontalAlignment.Center;
                        distance.verticalAlignment = UIVerticalAlignment.Middle;
                        distance.name = "dist.";
                        distance.font = UIHelperExtension.defaultFontCheckbox;
                        distance.width = 50f;
                        distance.height = 50;
                        distance.relativePosition = new Vector3(-42, 0);
                        distance.transform.localEulerAngles = new Vector3(0, 0, 45);
                        distance.isInteractive = false;
                    }

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

                        TLMLineUtils.PrintIntersections(airport, harbor, taxi, regionalTrainStation, cableCarStation, intersectionsPanel, otherLinesIntersections);
                        intersectionsPanel.autoLayout = true;
                        intersectionsPanel.wrapLayout = true;
                        intersectionsPanel.width = 55;
                        //		
                        return 42f;
                    }
                    else
                    {
                        TLMUtils.initButton(stationButton, true, "LinearHalfStation");
                        if (offsetX == 0)
                        {
                            stationButton.relativePosition = new Vector3(offsetX - 13, 15f);
                            return 31f;
                        }
                        else if (distance == null)
                        {
                            stationButton.relativePosition = new Vector3(offsetX - 23, 15f);
                            return 21f;
                        }
                        else
                        {
                            return 42f;
                        }
                    }
                }
                else if (showExtraStopInfo)
                {
                    float normalWidth = 42.5f;

                    NetNode stopNode = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)stationNodeId];

                    TLMLineUtils.GetQuantityPassengerWaiting(stationNodeId, out int residents, out int tourists, out int ttb);

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

                    UILabel timeTilBored = null;
                    TLMUtils.createUIElement<UILabel>(ref timeTilBored, stationInfoStatsPanel.transform);
                    timeTilBored.autoSize = false;
                    timeTilBored.text = tourists.ToString();
                    timeTilBored.tooltipLocaleID = "TLM_TIME_TIL_BORED";
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

        Vector3 getStation(ushort lineId, ushort stopId, ItemClass.SubService ss, out string stationName, out List<ushort> linhas, out string airport, out string harbor, out string taxiStand, out string regionalTrainStation, out string cableCarStation, out string prefix)
        {
            NetManager nm = Singleton<NetManager>.instance;
            BuildingManager bm = Singleton<BuildingManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[(int)stopId];
            stationName = TLMUtils.getStationName(stopId, lineId, ss, out ItemClass.Service servFound, out ItemClass.SubService subServFound, out prefix, out ushort buildingId);

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
            TLMLineUtils.GetNearLines(location, 120f, ref linhas);

            airport = String.Empty;
            taxiStand = String.Empty;
            harbor = String.Empty;
            regionalTrainStation = String.Empty;
            cableCarStation = string.Empty;

            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.TRAIN_SHOW_IN_LINEAR_MAP))
            {
                ushort trainStation = TLMUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, null, Building.Flags.None, Building.Flags.Untouchable | Building.Flags.Downgrading);

                if (trainStation > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = trainStation;
                    regionalTrainStation = bm.GetBuildingName(trainStation, iid);
                }
            }

            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.PLANE_SHOW_IN_LINEAR_MAP))
            {
                ushort airportId = TLMUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerPlane }, Building.Flags.None, Building.Flags.Untouchable);

                if (airportId > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = airportId;
                    airport = bm.GetBuildingName(airportId, iid);
                }
            }

            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.SHIP_SHOW_IN_LINEAR_MAP))
            {
                ushort harborId = TLMUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerShip }, Building.Flags.None, Building.Flags.Untouchable);

                if (harborId > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = harborId;
                    harbor = bm.GetBuildingName(harborId, iid);
                }
            }
            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.TAXI_SHOW_IN_LINEAR_MAP))
            {
                ushort taxiId = TLMUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 50f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTaxi, null, Building.Flags.None, Building.Flags.Untouchable);

                if (taxiId > 0)
                {
                    InstanceID iid = default(InstanceID);
                    iid.Building = taxiId;
                    taxiStand = bm.GetBuildingName(taxiId, iid);
                }
            }
            if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.CABLE_CAR_SHOW_IN_LINEAR_MAP))
            {
                ushort cableCarId = TLMUtils.FindBuilding(location != Vector3.zero ? location : nn.m_position, 120f, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportCableCar, null, Building.Flags.None, Building.Flags.Untouchable);

                if (cableCarId > 0)
                {
                    InstanceID iid = default(InstanceID);
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