using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Klyte.TransportLinesManager.UI.UVMPublicTransportWorldInfoPanel.UVMPublicTransportWorldInfoPanelObject;

namespace Klyte.TransportLinesManager.UI
{

    public class UVMTransportLineLinearMap : UICustomControl, IUVMPTWIPChild
    {
        private UIScrollablePanel m_bg;

        private MapMode m_currentMode = MapMode.NONE;
        private bool m_unscaledMode = false;
        private bool m_cachedUnscaledMode = false;

        #region Overridable

        private static Dictionary<string, UIComponent> GetTemplateDict() => (Dictionary<string, UIComponent>) typeof(UITemplateManager).GetField("m_Templates", RedirectorUtils.allFlags).GetValue(UITemplateManager.instance);

        public void Awake()
        {
            m_bg = component as UIScrollablePanel;

            PublicTransportWorldInfoPanel ptwip = UVMPublicTransportWorldInfoPanel.m_obj.origInstance;

            AddNewStopTemplate();

            ptwip.component.width = 800;

            BindComponents(ptwip);
            AdjustLineStopsPanel(ptwip);

            KlyteMonoUtils.CreateUIElement<UIPanel>(out m_panelModeSelector, m_bg.parent.transform);
            m_panelModeSelector.relativePosition = m_bg.relativePosition - new Vector3(0, 30);
            m_panelModeSelector.autoFitChildrenHorizontally = true;
            m_panelModeSelector.autoFitChildrenVertically = true;
            m_panelModeSelector.autoLayout = true;
            m_panelModeSelector.autoLayoutDirection = LayoutDirection.Horizontal;
            m_mapModeDropDown = UIHelperExtension.CloneBasicDropDownNoLabel(Enum.GetNames(typeof(MapMode)), (int idx) =>
            {
                m_currentMode = (MapMode) idx;
                RefreshVehicleButtons(GetLineID());
            }, m_panelModeSelector);
            m_mapModeDropDown.textScale = 0.75f;
            m_mapModeDropDown.size = new Vector2(200, 25);
            m_mapModeDropDown.itemHeight = 16;
            var uICheckBox = m_panelModeSelector.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kCheckBoxTemplate)) as UICheckBox;
            uICheckBox.text = "Unscaled";
            uICheckBox.eventCheckChanged += (x, val) => m_unscaledMode = val;
        }

        private static void AddNewStopTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(36, 36);
            UIButton t = UITemplateManager.Get<UIButton>("StopButton");
            panel.AttachUIComponent(t.gameObject);
            t.relativePosition = Vector2.zero;


            UIPanel connectionPanel = panel.AddUIComponent<UIPanel>();
            connectionPanel.name = "ConnectionPanel";
            connectionPanel.relativePosition = new Vector3(-50, 5);
            connectionPanel.size = new Vector3(50, 40);
            connectionPanel.autoLayout = true;
            connectionPanel.wrapLayout = true;
            connectionPanel.autoLayoutDirection = LayoutDirection.Vertical;
            connectionPanel.autoLayoutStart = LayoutStart.TopRight;

            GetTemplateDict()["StopButtonPanel"] = panel;
        }

        private void BindComponents(PublicTransportWorldInfoPanel __instance)
        {
            //STOPS
            m_stopsContainer = __instance.Find<UIPanel>("StopsPanel");
            m_stopButtons = new UITemplateList<UIPanel>(m_stopsContainer, "StopButtonPanel");
            m_vehicleButtons = new UITemplateList<UIButton>(m_stopsContainer, "VehicleButton");
            m_stopsLineSprite = __instance.Find<UISprite>("StopsLineSprite");
            m_lineEnd = __instance.Find<UISprite>("LineEnd");
            m_stopsLabel = __instance.Find<UILabel>("StopsLabel");
            m_vehiclesLabel = __instance.Find<UILabel>("VehiclesLabel");
            m_labelLineIncomplete = __instance.Find<UILabel>("LabelLineIncomplete");


            UISprite lineStart = __instance.Find<UISprite>("LineStart");
            lineStart.relativePosition = new Vector3(4, -8);

            m_vehiclesLabel.relativePosition = new Vector3(100, 12);

            m_stopsLineSprite.spriteName = "PlainWhite";
            m_stopsLineSprite.width = 25;

            m_connectionLabel = Instantiate(m_vehiclesLabel);
            m_connectionLabel.transform.SetParent(m_vehiclesLabel.transform.parent);
            m_connectionLabel.absolutePosition = m_vehiclesLabel.absolutePosition;
            m_connectionLabel.localeID = "K45_TLM_CONNECTIONS";

            m_lineStringLabel = Instantiate(m_vehiclesLabel);
            m_lineStringLabel.transform.SetParent(m_vehiclesLabel.transform.parent);
            m_lineStringLabel.absolutePosition = m_vehiclesLabel.absolutePosition + new Vector3(63, 0);
            m_lineStringLabel.autoSize = false;
            m_lineStringLabel.size = new Vector2(32, 32);
            m_lineStringLabel.isLocalized = false;
            m_lineStringLabel.text = "<k45Symbol TriangleIcon,FF8822,36>";
            m_lineStringLabel.textScale = 1;
            m_lineStringLabel.processMarkup = true;
        }

        private void AdjustLineStopsPanel(PublicTransportWorldInfoPanel __instance)
        {
            m_scrollPanel = __instance.Find<UIScrollablePanel>("ScrollablePanel");
            m_scrollPanel.eventGotFocus += OnGotFocusBind;
        }





        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void UpdateBindings()
        {
            ushort lineID = GetLineID();
            if (lineID != 0)
            {
                if (m_cachedUnscaledMode != m_unscaledMode)
                {
                    OnSetTarget();
                    m_cachedUnscaledMode = m_unscaledMode;
                }
                UpdateVehicleButtons(lineID);
                UpdateStopButtons(lineID);
            }
        }

        public static bool OnLinesOverviewClicked()
        {
            TransportLinesManagerMod.Instance.OpenPanelAtModTab();
            TLMPanel.Instance.OpenAt(UiCategoryTab.LineListing, TransportSystemDefinition.From(UVMPublicTransportWorldInfoPanel.GetLineID()));
            return false;
        }

        public static bool ResetScrollPosition()
        {
            m_scrollPanel.scrollPosition = m_cachedScrollPosition;
            return false;
        }


        public void OnSetTarget()
        {
            ushort lineID = GetLineID();
            if (lineID != 0)
            {
                LineType lineType = GetLineType(lineID);
                bool isTour = (lineType == LineType.WalkingTour);
                m_mapModeDropDown.isVisible = !isTour;
                m_vehiclesLabel.isVisible = !isTour && m_currentMode != MapMode.CONNECTIONS;
                m_connectionLabel.isVisible = !isTour || m_currentMode == MapMode.CONNECTIONS;


                if (isTour)
                {
                    m_currentMode = MapMode.CONNECTIONS;
                    m_stopsLabel.relativePosition = new Vector3(215f, 12f, 0f);
                    m_stopsLineSprite.relativePosition = m_kLineSSpritePositionForWalkingTours;
                    m_actualStopsX = m_kstopsXForWalkingTours;
                }
                else
                {
                    m_stopsLabel.relativePosition = new Vector3(215f, 12f, 0f);
                    m_stopsLineSprite.relativePosition = m_kLineSSpritePosition;
                    m_actualStopsX = m_kstopsX;

                }
                m_lineStringLabel.text = TLMLineUtils.GetIconString(lineID);
                m_stopsLineSprite.color = Singleton<TransportManager>.instance.GetLineColor(lineID);
                NetManager instance = Singleton<NetManager>.instance;
                int stopsCount = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountStops(lineID);
                float[] stopPositions = new float[stopsCount];
                m_cachedStopOrder = new ushort[stopsCount];
                float minDistance = float.MaxValue;
                float lineLength = 0f;
                UIPanel[] stopsButtons = m_stopButtons.SetItemCount(stopsCount);
                ushort firstStop = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops;
                ushort currentStop = firstStop;
                int idx = 0;
                while (currentStop != 0 && idx < stopsButtons.Length)
                {
                    stopsButtons[idx].GetComponentInChildren<UIButton>().objectUserData = currentStop;

                    m_cachedStopOrder[idx] = currentStop;
                    UILabel uilabel = stopsButtons[idx].Find<UILabel>("PassengerCount");
                    if (uilabel != null)
                    {
                        uilabel.processMarkup = true;
                        uilabel.prefix = $"<color white>{TLMLineUtils.getFullStationName(currentStop, lineID, ItemClass.SubService.None)}</color>";
                        uilabel.isVisible = true;
                        uilabel.text = "";

                        KlyteMonoUtils.LimitWidth(uilabel, 180, true);
                    }
                    CreateConnectionPanel(instance, stopsButtons, currentStop, idx);

                    for (int i = 0; i < 8; i++)
                    {
                        ushort segmentId = instance.m_nodes.m_buffer[currentStop].GetSegment(i);
                        if (segmentId != 0 && instance.m_segments.m_buffer[segmentId].m_startNode == currentStop)
                        {
                            currentStop = instance.m_segments.m_buffer[segmentId].m_endNode;
                            float segmentSize = m_unscaledMode ? m_kminStopDistance : instance.m_segments.m_buffer[segmentId].m_averageLength;
                            if (segmentSize == 0f)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Two transport line stops have zero distance");
                                segmentSize = 100f;
                            }
                            stopPositions[idx] = segmentSize;
                            if (segmentSize < minDistance)
                            {
                                minDistance = segmentSize;
                            }
                            lineLength += stopPositions[idx];
                            break;
                        }
                    }
                    if (stopsCount > 2 && currentStop == firstStop)
                    {
                        break;
                    }
                    if (stopsCount == 2 && idx > 0)
                    {
                        break;
                    }
                    if (stopsCount == 1)
                    {
                        break;
                    }
                    if (++idx >= 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
                float stopDistanceFactor = m_kminStopDistance / minDistance;
                m_uILineLength = stopDistanceFactor * lineLength;
                if (m_uILineLength < m_kminUILineLength)
                {
                    m_uILineLength = m_kminUILineLength;
                    stopDistanceFactor = m_uILineLength / lineLength;
                }
                else if (m_uILineLength > m_kmaxUILineLength)
                {
                    m_uILineLength = m_kmaxUILineLength;
                    stopDistanceFactor = m_uILineLength / lineLength;
                }
                if (stopsCount <= 2)
                {
                    m_uILineOffset = (stopDistanceFactor * stopPositions[stopPositions.Length - 1]) - 30f;
                }
                if (m_uILineOffset < 20f || stopsCount > 2)
                {
                    m_uILineOffset = stopDistanceFactor * stopPositions[stopPositions.Length - 1] / 2f;
                }
                m_stopsLineSprite.height = m_uILineLength;
                m_stopsContainer.height = m_uILineLength + m_kvehicleButtonHeight;
                Vector3 relativePosition = m_lineEnd.relativePosition;
                relativePosition.y = m_uILineLength + 13f;
                relativePosition.x = m_stopsLineSprite.relativePosition.x + 4f;
                m_lineEnd.relativePosition = relativePosition;
                float num8 = 0f;
                for (int j = 0; j < stopsCount; j++)
                {
                    Vector3 relativePosition2 = m_stopButtons.items[j].relativePosition;
                    relativePosition2.x = m_actualStopsX;
                    relativePosition2.y = ShiftVerticalPosition(num8);
                    m_stopButtons.items[j].relativePosition = relativePosition2;
                    num8 += stopPositions[j] * stopDistanceFactor;
                }
                RefreshVehicleButtons(lineID);
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None)
                {
                    m_labelLineIncomplete.isVisible = false;
                    m_stopsContainer.isVisible = true;
                }
                else
                {
                    m_labelLineIncomplete.isVisible = true;
                    m_stopsContainer.isVisible = false;
                }
            }
        }

        private void CreateConnectionPanel(NetManager instance, UIPanel[] stopsButtons, ushort currentStop, int idx)
        {
            ushort lineID = GetLineID();
            var linesFound = new List<ushort>();
            TLMLineUtils.GetNearLines(instance.m_nodes.m_buffer[currentStop].m_position, 150f, ref linesFound);
            linesFound.Remove(lineID);
            UIPanel connectionPanel = stopsButtons[idx].Find<UIPanel>("ConnectionPanel");

            while (connectionPanel.childCount < linesFound.Count)
            {
                KlyteMonoUtils.CreateUIElement(out UILabel lineLabel, connectionPanel.transform, "", new Vector4(0, 0, 15, 15));
                lineLabel.processMarkup = true;
                lineLabel.textScale = 0.5f;
                lineLabel.eventClicked += OpenLineLabel;
                lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            }
            while (connectionPanel.childCount > linesFound.Count)
            {
                UIComponent comp = connectionPanel.components[linesFound.Count];
                connectionPanel.components.RemoveAt(linesFound.Count);
                GameObject.Destroy(comp);
                connectionPanel.Invalidate();
            }
            int multiplier = linesFound.Count > m_kMaxConnectionsLine ? 1 : 2;
            for (int i = 0; i < linesFound.Count; i++)
            {
                ushort line = linesFound[i];
                UILabel lineLabel = connectionPanel.components[i].GetComponent<UILabel>();
                lineLabel.name = $"L{line}";
                lineLabel.tooltip = Singleton<TransportManager>.instance.GetLineName(line);
                lineLabel.text = TLMLineUtils.GetIconString(line);
                lineLabel.stringUserData = line.ToString();
                lineLabel.size = m_kBasicConnectionLogoSize * multiplier;
                lineLabel.textScale = m_kBasicConnectionLogoFontSize * multiplier;
            }

            connectionPanel.isVisible = m_currentMode == MapMode.CONNECTIONS;
        }

        private void OpenLineLabel(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (ushort.TryParse(component.stringUserData, out ushort lineId) && lineId != 0)
            {
                Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_stops].m_position;
                InstanceID iid = InstanceID.Empty;
                iid.TransportLine = lineId;
                WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
                eventParam.Use();
            }
        }

        #endregion

        private void UpdateVehicleButtons(ushort lineID)
        {
            if (m_vehicleCountMismatch)
            {
                RefreshVehicleButtons(lineID);
                m_vehicleCountMismatch = false;
            }
            if (m_currentMode == MapMode.CONNECTIONS)
            {
                return;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleId = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
            int idx = 0;
            while (vehicleId != 0)
            {
                if (idx > m_vehicleButtons.items.Count - 1)
                {
                    m_vehicleCountMismatch = true;
                    break;
                }
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].Info;


                if (m_unscaledMode)
                {
                    ushort nextStop = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding;
                    float prevStationIdx = Array.IndexOf(m_cachedStopOrder, TransportLine.GetPrevStop(nextStop));
                    float nextStationIdx = Array.IndexOf(m_cachedStopOrder, nextStop);
                    if (nextStationIdx < prevStationIdx)
                    {
                        nextStationIdx += m_cachedStopOrder.Length;
                    }
                    Vector3 relativePosition = m_vehicleButtons.items[idx].relativePosition;
                    if ((Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & (Vehicle.Flags.Leaving)) != 0)
                    {
                        relativePosition.y = (prevStationIdx * 0.75f) + (nextStationIdx * 0.25f);
                    }
                    else if ((Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & (Vehicle.Flags.Arriving)) != 0)
                    {
                        relativePosition.y = (prevStationIdx * 0.25f) + (nextStationIdx * 0.75f);
                    }
                    else if ((Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId].m_flags & (Vehicle.Flags.Stopped)) != 0)
                    {
                        relativePosition.y = (prevStationIdx);
                    }
                    else
                    {
                        relativePosition.y = (prevStationIdx * 0.5f) + (nextStationIdx * 0.5f);
                    }
                    relativePosition.y = ShiftVerticalPosition(relativePosition.y * m_kminStopDistance);
                    m_vehicleButtons.items[idx].relativePosition = relativePosition;
                }
                else
                {
                    if (info.m_vehicleAI.GetProgressStatus(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out float current, out float max))
                    {
                        float percent = current / max;
                        float y = m_uILineLength * percent;
                        Vector3 relativePosition = m_vehicleButtons.items[idx].relativePosition;
                        relativePosition.y = ShiftVerticalPosition(y);
                        m_vehicleButtons.items[idx].relativePosition = relativePosition;
                    }
                }
                info.m_vehicleAI.GetBufferStatus(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], out string text, out int passengerQuantity, out int passengerCapacity);
                UILabel labelVehicle = m_vehicleButtons.items[idx].Find<UILabel>("PassengerCount");
                labelVehicle.prefix = passengerQuantity.ToString() + "/" + passengerCapacity.ToString();
                labelVehicle.processMarkup = true;
                labelVehicle.textAlignment = UIHorizontalAlignment.Right;
                switch (m_currentMode)
                {
                    case MapMode.WAITING:
                    case MapMode.NONE:
                    case MapMode.CONNECTIONS:
                        labelVehicle.text = "";
                        labelVehicle.suffix = "";
                        break;
                    case MapMode.EARNINGS_ALL_TIME:
                        TLMTransportLineStatusesManager.instance.GetIncomeAndExpensesForVehicle(vehicleId, out long income, out long expense);
                        PrintIncomeExpenseVehicle(lineID, idx, labelVehicle, income, expense);
                        break;
                    case MapMode.EARNINGS_LAST_WEEK:
                        TLMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForVehicles(vehicleId, out long income2, out long expense2);
                        PrintIncomeExpenseVehicle(lineID, idx, labelVehicle, income2, expense2);
                        break;
                    case MapMode.EARNINGS_CURRENT_WEEK:
                        TLMTransportLineStatusesManager.instance.GetCurrentIncomeAndExpensesForVehicles(vehicleId, out long income3, out long expense3);
                        PrintIncomeExpenseVehicle(lineID, idx, labelVehicle, income3, expense3);
                        break;
                }

                vehicleId = instance.m_vehicles.m_buffer[vehicleId].m_nextLineVehicle;
                if (++idx >= 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            if (idx < m_vehicleButtons.items.Count - 1)
            {
                m_vehicleCountMismatch = true;
            }
        }

        private void PrintIncomeExpenseVehicle(ushort lineID, int idx, UILabel labelVehicle, long income, long expense)
        {
            m_vehicleButtons.items[idx].color = Color.Lerp(Color.white, income > expense ? Color.green : Color.red, Mathf.Max(income, expense) / 100f * Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].Info.m_ticketPrice);
            labelVehicle.text = $"\n<color #00cc00>{(income / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
            labelVehicle.suffix = $"\n<color #ff0000>{(expense / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
        }



        public void OnGotFocus() => m_cachedScrollPosition = m_scrollPanel.scrollPosition;
        private void OnGotFocusBind(UIComponent component, UIFocusEventParameter eventParam) => m_cachedScrollPosition = m_scrollPanel.scrollPosition;

        internal LineType GetLineType(ushort lineID) => UVMPublicTransportWorldInfoPanel.GetLineType(lineID);

        private float ShiftVerticalPosition(float y)
        {
            y += m_uILineOffset;
            if (y > m_uILineLength + 5f)
            {
                y -= m_uILineLength;
            }
            return y;
        }

        private void RefreshVehicleButtons(ushort lineID)
        {
            if (m_currentMode == MapMode.CONNECTIONS)
            {
                m_vehiclesLabel.isVisible = false;
                m_vehicleButtons.SetItemCount(0);
                m_connectionLabel.isVisible = true;
            }
            else
            {
                m_vehiclesLabel.isVisible = true;
                m_connectionLabel.isVisible = false;
                m_vehicleButtons.SetItemCount(Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountVehicles(lineID));
                VehicleManager instance = Singleton<VehicleManager>.instance;
                ushort num = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
                int num2 = 0;
                while (num != 0)
                {
                    Vector3 relativePosition = m_vehicleButtons.items[num2].relativePosition;
                    relativePosition.x = m_kvehiclesX;
                    m_vehicleButtons.items[num2].relativePosition = relativePosition;
                    m_vehicleButtons.items[num2].objectUserData = num;
                    num = instance.m_vehicles.m_buffer[num].m_nextLineVehicle;
                    if (++num2 >= 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }

        internal ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        private void UpdateStopButtons(ushort lineID)
        {
            if (GetLineType(lineID) != LineType.WalkingTour)
            {
                ushort stop = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_stops;
                foreach (UIPanel uibutton in m_stopButtons.items)
                {
                    UIPanel connectionPanel = uibutton.Find<UIPanel>("ConnectionPanel");
                    if (connectionPanel != null)
                    {
                        connectionPanel.isVisible = m_currentMode == MapMode.CONNECTIONS;
                    }

                    UILabel uilabel = uibutton.Find<UILabel>("PassengerCount");
                    switch (m_currentMode)
                    {
                        case MapMode.WAITING:
                            TLMLineUtils.GetQuantityPassengerWaiting(stop, out int residents, out int tourists, out int timeTillBored);
                            uilabel.text = $"\n{residents + tourists} (<color #00aa00>{residents}</color> + <color #aa88ff>{tourists}</color>)";
                            uibutton.color = Color.Lerp(Color.red, Color.white, timeTillBored / 255f);
                            uibutton.tooltip = $"Time Until Bored: {timeTillBored}";
                            break;
                        case MapMode.NONE:
                            uibutton.color = Color.white;
                            uilabel.text = "";
                            uibutton.tooltip = "";
                            break;
                        case MapMode.CONNECTIONS:
                            uibutton.color = Color.white;
                            uilabel.text = "";
                            uibutton.tooltip = "";
                            break;
                        case MapMode.EARNINGS_ALL_TIME:
                            TLMTransportLineStatusesManager.instance.GetStopIncome(stop, out long income);
                            PrintIncomeStop(lineID, uibutton, uilabel, income);
                            break;
                        case MapMode.EARNINGS_CURRENT_WEEK:
                            TLMTransportLineStatusesManager.instance.GetCurrentStopIncome(stop, out long income2);
                            PrintIncomeStop(lineID, uibutton, uilabel, income2);
                            break;
                        case MapMode.EARNINGS_LAST_WEEK:
                            TLMTransportLineStatusesManager.instance.GetLastWeekStopIncome(stop, out long income3);
                            PrintIncomeStop(lineID, uibutton, uilabel, income3);
                            break;
                    }
                    stop = TransportLine.GetNextStop(stop);
                }
            }
        }

        private static void PrintIncomeStop(ushort lineID, UIPanel uibutton, UILabel uilabel, long income)
        {
            uibutton.color = Color.Lerp(Color.white, Color.green, income / (1000f * Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].Info.m_ticketPrice));
            uilabel.text = $"\n<color #00cc00>{(income / 100.0f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}</color>";
            uibutton.tooltip = "";
        }




        private UILabel m_labelLineIncomplete;
        internal UISprite m_stopsLineSprite;

        internal UISprite m_lineEnd;


        internal float m_uILineLength;

        internal float m_uILineOffset;

        internal bool m_vehicleCountMismatch;
        private UIPanel m_stopsContainer;
        internal UITemplateList<UIPanel> m_stopButtons;

        internal UITemplateList<UIButton> m_vehicleButtons;

        internal float m_kstopsX = 170;
        internal float m_kstopsXForWalkingTours = 170;
        internal float m_kvehiclesX = 130;
        internal float m_kminStopDistance = 40f;
        internal float m_kvehicleButtonHeight = 36f;
        internal float m_kminUILineLength = 370f;
        internal float m_kmaxUILineLength = 10000f;

        internal float m_actualStopsX;
        internal Vector2 m_kLineSSpritePosition = new Vector2(175f, 20f);
        internal Vector2 m_kLineSSpritePositionForWalkingTours = new Vector2(175f, 20f);
        internal Vector2 m_kBasicConnectionLogoSize = new Vector2(16, 15);
        internal float m_kBasicConnectionLogoFontSize = 0.5f;
        internal int m_kMaxConnectionsLine = 5;

        internal UILabel m_stopsLabel;

        internal UILabel m_vehiclesLabel;

        internal UILabel m_connectionLabel;
        internal UILabel m_lineStringLabel;

        public static UIScrollablePanel m_scrollPanel;

        internal static Vector2 m_cachedScrollPosition;
        private UIDropDown m_mapModeDropDown;
        private UIPanel m_panelModeSelector;
        private ushort[] m_cachedStopOrder;

        private enum MapMode
        {
            NONE,
            WAITING,
            CONNECTIONS,
            EARNINGS_CURRENT_WEEK,
            EARNINGS_LAST_WEEK,
            EARNINGS_ALL_TIME,
        }
    }
}