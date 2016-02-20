using ColossalFramework;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.MapDrawer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.UI
{
    public class TLMMainPanel
    {
        private TLMController m_controller;
        private UIPanel mainPanel;
        private List<GameObject> linesGameObjects = new List<GameObject>();
        private float offset;
        private Dictionary<Int32, UInt16> trainList;
        private Dictionary<Int32, UInt16> metroList;
        private Dictionary<Int32, UInt16> surfaceMetroList;
        private Dictionary<Int32, UInt16> bulletTrainList;
        private Dictionary<Int32, UInt16> busList;
        private Dictionary<Int32, UInt16> tramList;
        private Dictionary<Int32, UInt16> lowBusList;
        private Dictionary<Int32, UInt16> highBusList;
        private Dictionary<Int32, UInt16> shipList;
        private UIButton trainLeg;
        private UIButton tramLeg;
        private UIButton metroLeg;
        private UIButton busLeg;
        private UIButton lowBusLeg;
        private UIButton highBusLeg;
        private UIButton surfaceMetroLeg;
        private UIButton bulletTrainLeg;
        private UIButton shipLeg;
        private UIScrollablePanel allLinesListPanel;
        private UIScrollablePanel filteredLinesListPanel;
        private CurrentFilterSelected _currentSelection = CurrentFilterSelected.NONE;
        private CurrentFilterSelected currentSelection
        {
            get
            {
                return _currentSelection;
            }

            set
            {
                if (value == _currentSelection)
                {
                    _currentSelection = CurrentFilterSelected.NONE;
                }
                else
                {
                    _currentSelection = value;
                }
                listLines();
            }
        }

        //botoes da parte das antigas configuraçoes		
        private UIButton resetLineNames;
        private UIButton resetLineColor;

        public void Show()
        {
            TransportLinesManagerMod.instance.showVersionInfoPopup();
            mainPanel.Show();
            clearLines();
            listLines();
        }

        public void Hide()
        {
            mainPanel.Hide();
        }

        public GameObject gameObject
        {
            get
            {
                try
                {
                    return mainPanel.gameObject;
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    return null;
                }
            }
        }

        public Transform transform
        {
            get
            {
                return mainPanel.transform;
            }
        }

        public bool isVisible
        {
            get
            {
                return mainPanel.isVisible;
            }
        }

        public float width
        {
            get
            {
                return mainPanel.width;
            }
        }

        public TLMController controller
        {
            get
            {
                return m_controller;
            }
        }

        public Dictionary<Int32, UInt16> bulletTrains
        {
            get
            {
                if (bulletTrainList == null)
                {
                    listLines();
                }
                return bulletTrainList;
            }
        }

        public Dictionary<Int32, UInt16> train
        {
            get
            {
                if (trainList == null)
                {
                    listLines();
                }
                return trainList;
            }
        }
        public Dictionary<Int32, UInt16> surfaceMetros
        {
            get
            {
                if (surfaceMetroList == null)
                {
                    listLines();
                }
                return surfaceMetroList;
            }
        }

        public Dictionary<Int32, UInt16> metro
        {
            get
            {
                if (metroList == null)
                {
                    listLines();
                }
                return metroList;
            }
        }

        public Dictionary<Int32, UInt16> bus
        {
            get
            {
                if (busList == null)
                {
                    listLines();
                }
                return busList;
            }
        }

        public Dictionary<Int32, UInt16> trams
        {
            get
            {
                if (tramList == null)
                {
                    listLines();
                }
                return tramList;
            }
        }

        public Dictionary<Int32, UInt16> lowBus
        {
            get
            {
                if (lowBusList == null)
                {
                    listLines();
                }
                return lowBusList;
            }
        }
        public Dictionary<Int32, UInt16> highBus
        {
            get
            {
                if (highBusList == null)
                {
                    listLines();
                }
                return highBusList;
            }
        }
        public Dictionary<Int32, UInt16> ships
        {
            get
            {
                if (shipList == null)
                {
                    listLines();
                }
                return shipList;
            }
        }

        private void clearLines()
        {

            foreach (GameObject o in linesGameObjects)
            {
                UnityEngine.Object.Destroy(o);
            }
            linesGameObjects.Clear();
            allLinesListPanel.ScrollToTop();
            filteredLinesListPanel.ScrollToTop();

            foreach (var x in new UIButton[] { shipLeg, bulletTrainLeg, highBusLeg, lowBusLeg, metroLeg, trainLeg, surfaceMetroLeg, busLeg, tramLeg })
            {
                if (x != null)
                {
                    x.focusedColor = Color.white;
                    x.color = Color.white;
                }
            }
        }

        public TLMMainPanel(TLMController tli)
        {
            this.m_controller = tli;
            createMainView();
            UIPanel panelListing = mainPanel.AddUIComponent<UIPanel>();
            //			DebugOutputPanel.AddMessage (PluginManager.MessageType.Error, "!!!!");
            panelListing.width = mainPanel.width;
            panelListing.height = 290;
            panelListing.relativePosition = new Vector3(0, 90);
            panelListing.name = "Lines Listing";
            panelListing.clipChildren = true;
            //			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, "LOADING SCROLL");
            GameObject scrollObj = new GameObject("Lines Listing Scroll", new Type[] { typeof(UIScrollablePanel) });
            //			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, "SCROLL LOADED");
            allLinesListPanel = scrollObj.GetComponent<UIScrollablePanel>();
            allLinesListPanel.autoLayout = false;
            allLinesListPanel.width = mainPanel.width;
            allLinesListPanel.height = 290;
            allLinesListPanel.useTouchMouseScroll = true;
            allLinesListPanel.scrollWheelAmount = 20;
            panelListing.AttachUIComponent(allLinesListPanel.gameObject);
            allLinesListPanel.relativePosition = new Vector3(0, 0);

            filteredLinesListPanel = GameObject.Instantiate(allLinesListPanel.gameObject).GetComponent<UIScrollablePanel>();
            filteredLinesListPanel.transform.SetParent(allLinesListPanel.transform.parent);
            filteredLinesListPanel.autoLayout = true;
            filteredLinesListPanel.autoLayoutDirection = LayoutDirection.Vertical;
            filteredLinesListPanel.transform.localPosition = Vector3.zero;


            allLinesListPanel.eventMouseWheel += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                allLinesListPanel.scrollPosition -= new Vector2(0, eventParam.wheelDelta * allLinesListPanel.scrollWheelAmount);
            };

            filteredLinesListPanel.eventMouseWheel += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                filteredLinesListPanel.scrollPosition -= new Vector2(0, eventParam.wheelDelta * allLinesListPanel.scrollWheelAmount);
            };

            //botoes da antiga parte extra 

            createResetAllLinesNamingButton();
            createResetAllLinesColorButton();
            if (TransportLinesManagerMod.betaMapGen.value)
            {
                createLinesDrawButton();
            }

        }

        private void listLines()
        {
            clearLines();
            trainList = new Dictionary<int, ushort>();
            metroList = new Dictionary<int, ushort>();
            busList = new Dictionary<int, ushort>();
            lowBusList = new Dictionary<int, ushort>();
            highBusList = new Dictionary<int, ushort>();
            surfaceMetroList = new Dictionary<int, ushort>();
            bulletTrainList = new Dictionary<int, ushort>();
            shipList = new Dictionary<int, ushort>();
            tramList = new Dictionary<int, ushort>();

            for (ushort i = 0; i < m_controller.tm.m_lines.m_size; i++)
            {
                TransportLine t = m_controller.tm.m_lines.m_buffer[(int)i];
                if (t.m_lineNumber == 0 || t.CountStops(i) == 0)
                    continue;
                switch (t.Info.m_transportType)
                {
                    case TransportInfo.TransportType.Bus:
                        if (TransportLinesManagerMod.isIPTCompatibiltyMode)
                        {
                            while (busList.ContainsKey(t.m_lineNumber))
                            {
                                t.m_lineNumber++;
                            }
                            busList.Add(t.m_lineNumber, i);
                        }
                        else
                        {
                            if (TLMCW.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.LOW_BUS_LINES_IDS).Contains(i))
                            {
                                while (lowBusList.ContainsKey(t.m_lineNumber))
                                {
                                    t.m_lineNumber++;
                                }
                                lowBusList.Add(t.m_lineNumber, i);
                            }
                            else if (TLMCW.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.HIGH_BUS_LINES_IDS).Contains(i))
                            {
                                while (highBusList.ContainsKey(t.m_lineNumber))
                                {
                                    t.m_lineNumber++;
                                }
                                highBusList.Add(t.m_lineNumber, i);
                            }
                            else {
                                while (busList.ContainsKey(t.m_lineNumber))
                                {
                                    t.m_lineNumber++;
                                }
                                busList.Add(t.m_lineNumber, i);
                            }
                        }

                        break;

                    case TransportInfo.TransportType.Metro:
                        while (metroList.ContainsKey(t.m_lineNumber))
                        {
                            t.m_lineNumber++;
                        }
                        metroList.Add(t.m_lineNumber, i);
                        break;
                    case TransportInfo.TransportType.Tram:
                        while (tramList.ContainsKey(t.m_lineNumber))
                        {
                            t.m_lineNumber++;
                        }
                        tramList.Add(t.m_lineNumber, i);
                        break;

                    case TransportInfo.TransportType.Ship:
                        while (shipList.ContainsKey(t.m_lineNumber))
                        {
                            t.m_lineNumber++;
                        }
                        shipList.Add(t.m_lineNumber, i);
                        break;

                    case TransportInfo.TransportType.Train:
                        if (TransportLinesManagerMod.isIPTCompatibiltyMode)
                        {
                            while (trainList.ContainsKey(t.m_lineNumber))
                            {
                                t.m_lineNumber++;
                            }
                            trainList.Add(t.m_lineNumber, i);

                        }
                        else
                        {
                            if (TLMCW.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.SURFACE_METRO_LINES_IDS).Contains(i))
                            {
                                while (surfaceMetroList.ContainsKey(t.m_lineNumber))
                                {
                                    t.m_lineNumber++;
                                }
                                surfaceMetroList.Add(t.m_lineNumber, i);
                            }
                            else if (TLMCW.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_LINES_IDS).Contains(i))
                            {
                                while (bulletTrainList.ContainsKey(t.m_lineNumber))
                                {
                                    t.m_lineNumber++;
                                }
                                bulletTrainList.Add(t.m_lineNumber, i);
                            }
                            else
                            {
                                while (trainList.ContainsKey(t.m_lineNumber))
                                {
                                    t.m_lineNumber++;
                                }
                                trainList.Add(t.m_lineNumber, i);
                            }
                        }
                        break;
                    default:
                        continue;
                }
            }
            if (currentSelection == CurrentFilterSelected.NONE)
            {
                allLinesListPanel.enabled = true;
                filteredLinesListPanel.enabled = false;
                offset = 0;
                offset += drawButtonsFromDictionary(shipList, offset);
                offset += drawButtonsFromDictionary(bulletTrainList, offset);
                offset += drawButtonsFromDictionary(trainList, offset);
                offset += drawButtonsFromDictionary(surfaceMetroList, offset);
                offset += drawButtonsFromDictionary(metroList, offset);
                offset += drawButtonsFromDictionary(tramList, offset);
                offset += drawButtonsFromDictionary(highBusList, offset);
                offset += drawButtonsFromDictionary(busList, offset);
                offset += drawButtonsFromDictionary(lowBusList, offset);
            }
            else
            {
                allLinesListPanel.enabled = false;
                filteredLinesListPanel.enabled = true;
                switch (currentSelection)
                {
                    case CurrentFilterSelected.BULLET:
                        bulletTrainLeg.color = new Color32(0, 128, 0, 255);
                        bulletTrainLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(bulletTrainList);
                        break;
                    case CurrentFilterSelected.HIGH_BUS:
                        highBusLeg.color = new Color32(0, 128, 0, 255);
                        highBusLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(highBusList);
                        break;
                    case CurrentFilterSelected.LOW_BUS:
                        lowBusLeg.color = new Color32(0, 128, 0, 255);
                        lowBusLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(lowBusList);
                        break;
                    case CurrentFilterSelected.METRO:
                        metroLeg.color = new Color32(0, 128, 0, 255);
                        metroLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(metroList);
                        break;
                    case CurrentFilterSelected.TRAM:
                        tramLeg.color = new Color32(0, 128, 0, 255);
                        tramLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(tramList);
                        break;
                    case CurrentFilterSelected.REGIONAL_TRAIN:
                        trainLeg.color = new Color32(0, 128, 0, 255);
                        trainLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(trainList);
                        break;
                    case CurrentFilterSelected.REGULAR_BUS:
                        busLeg.color = new Color32(0, 128, 0, 255);
                        busLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(busList);
                        break;
                    case CurrentFilterSelected.SURFACE_METRO:
                        surfaceMetroLeg.color = new Color32(0, 128, 0, 255);
                        surfaceMetroLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(surfaceMetroList);
                        break;
                    case CurrentFilterSelected.SHIP:
                        shipLeg.color = new Color32(0, 128, 0, 255);
                        shipLeg.focusedColor = new Color32(0, 128, 0, 255);
                        drawDetailedButtonFromDictionary(shipList);
                        break;
                }
            }
        }

        private float drawButtonsFromDictionary(Dictionary<Int32, UInt16> map, float offsetDraw)
        {
            int j = 0;
            List<Int32> keys = map.Keys.ToList();
            keys.Sort();
            foreach (Int32 k in keys)
            {

                TransportLine t = m_controller.tm.m_lines.m_buffer[map[k]];
                //				string item = "[" + t.Info.m_transportType + " | " + t.m_lineNumber + "] " + t.GetColor () + " " + tli.tm.GetLineName ( map [k]);
                GameObject itemContainer = new GameObject();
                linesGameObjects.Add(itemContainer);

                itemContainer.transform.parent = allLinesListPanel.transform;
                UIButtonLineInfo itemButton = itemContainer.AddComponent<UIButtonLineInfo>();

                itemButton.relativePosition = new Vector3(10.0f + (j % 10) * 40f, offsetDraw + 40 * (int)(j / 10));
                itemButton.width = 35;
                itemButton.height = 35;
                TLMUtils.initButton(itemButton, true, "ButtonMenu");
                itemButton.atlas = TLMController.taLineNumber;
                ModoNomenclatura mn, pre;
                Separador s;
                bool z;
                bool invertPrefixSuffix;
                string icon;
                TLMLineUtils.getLineNamingParameters(map[k], out pre, out s, out mn, out z, out invertPrefixSuffix, out icon);
                TLMUtils.initButtonSameSprite(itemButton, icon);

                itemButton.color = m_controller.tm.GetLineColor(map[k]);
                itemButton.hoveredTextColor = itemButton.color;
                itemButton.textColor = TLMUtils.contrastColor(t.GetColor());
                itemButton.hoveredColor = itemButton.textColor;
                itemButton.tooltip = m_controller.tm.GetLineName((ushort)map[k]);
                itemButton.lineID = map[k];
                itemButton.eventClick += m_controller.lineInfoPanel.openLineInfo;
                setLineNumberMainListing(t.m_lineNumber, itemButton, pre, s, mn, z, invertPrefixSuffix);

                bool day, night;
                t.GetActive(out day, out night);
                if (!day || !night)
                {
                    UILabel lineTime = null;
                    TLMUtils.createUIElement<UILabel>(ref lineTime, itemButton.transform);
                    lineTime.relativePosition = new Vector3(0, 0);
                    lineTime.width = 35;
                    lineTime.height = 35;
                    lineTime.atlas = TLMController.taLineNumber;
                    lineTime.backgroundSprite = day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
                }
                itemButton.name = "TransportLinesManagerLineButton" + itemButton.text;
                j++;

            }
            if (j > 0)
            {
                return 40 * (int)((j - 1) / 10 + 1);
            }
            else {
                return 0;
            }
        }

        private void drawDetailedButtonFromDictionary(Dictionary<Int32, UInt16> map)
        {
            List<Int32> keys = map.Keys.ToList();
            keys.Sort();
            foreach (Int32 k in keys)
            {
                ushort lineId = map[k];

                TransportLine t = m_controller.tm.m_lines.m_buffer[lineId];
                GameObject itemContainer = new GameObject();
                linesGameObjects.Add(itemContainer);

                itemContainer.transform.parent = filteredLinesListPanel.transform;
                UIPanel container = itemContainer.AddComponent<UIPanel>();
                container.width = container.transform.GetComponentInParent<UIScrollablePanel>().width;
                container.height = 55;
                container.autoFitChildrenHorizontally = false;
                container.autoFitChildrenVertically = false;
                container.autoLayout = false;

                UIButtonLineInfo itemButton = null;
                TLMUtils.createUIElement<UIButtonLineInfo>(ref itemButton, container.transform);


                itemButton.relativePosition = new Vector3(10.0f, 10f);
                itemButton.width = 35;
                itemButton.height = 35;
                TLMUtils.initButton(itemButton, true, "ButtonMenu");
                itemButton.atlas = TLMController.taLineNumber;
                ModoNomenclatura mn, pre;
                Separador s;
                bool z;
                bool invertPrefixSuffix;
                string icon;
                TLMLineUtils.getLineNamingParameters(lineId, out pre, out s, out mn, out z, out invertPrefixSuffix, out icon);
                TLMUtils.initButtonSameSprite(itemButton, icon);

                itemButton.color = m_controller.tm.GetLineColor(lineId);
                itemButton.hoveredTextColor = itemButton.color;
                itemButton.textColor = TLMUtils.contrastColor(t.GetColor());
                itemButton.hoveredColor = itemButton.textColor;
                itemButton.lineID = lineId;
                itemButton.eventClick += m_controller.lineInfoPanel.openLineInfo;
                string lineNum = setLineNumberMainListing(t.m_lineNumber, itemButton, pre, s, mn, z, invertPrefixSuffix);

                bool day, night;
                t.GetActive(out day, out night);
                UILabel lineTime = null;
                TLMUtils.createUIElement<UILabel>(ref lineTime, itemButton.transform);
                lineTime.relativePosition = new Vector3(0, 0);
                lineTime.width = 35;
                lineTime.height = 35;
                lineTime.atlas = TLMController.taLineNumber;
                if (!day || !night)
                {
                    lineTime.backgroundSprite = day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
                }

                itemButton.name = "TransportLinesManagerLineButton" + itemButton.text;
                UILabel lineName = null;
                TLMUtils.createUIElement<UILabel>(ref lineName, container.transform);
                lineName.relativePosition = new Vector3(55f, 10f);
                lineName.text = m_controller.tm.GetLineName(lineId); ;
                lineName.tooltip = lineName.text;
                lineName.textScale = 0.7f;
                lineName.name = "LineName";

                UILabel lineExtraInfo = null;
                TLMUtils.createUIElement<UILabel>(ref lineExtraInfo, container.transform);
                lineExtraInfo.relativePosition = new Vector3(55f, 25f);
                uint totalPassengers = t.m_passengers.m_residentPassengers.m_averageCount + t.m_passengers.m_touristPassengers.m_averageCount;
                lineExtraInfo.text = "";
                lineExtraInfo.tooltip = lineExtraInfo.text;
                lineExtraInfo.textScale = 0.7f;
                lineExtraInfo.name = "LineExtraInfo1";

                UILabel lineExtraInfo2 = null;
                TLMUtils.createUIElement<UILabel>(ref lineExtraInfo2, container.transform);
                lineExtraInfo2.relativePosition = new Vector3(55f, 40f);
                float vehicles = t.CountVehicles(lineId);
                lineExtraInfo2.text = "";
                lineExtraInfo2.tooltip = lineExtraInfo2.text;
                lineExtraInfo2.textScale = 0.7f;
                lineExtraInfo2.name = "LineExtraInfo2";


                UIButton buttonCycleTime = null;
                TLMUtils.createUIElement<UIButton>(ref buttonCycleTime, container.transform);
                buttonCycleTime.pivot = UIPivotPoint.TopRight;
                buttonCycleTime.relativePosition = new Vector3(container.width - 10f, 5f);
                buttonCycleTime.text = "Cycle Day/Night";
                buttonCycleTime.textScale = 0.6f;
                buttonCycleTime.width = 100;
                buttonCycleTime.height = 15;
                buttonCycleTime.tooltip = "Cycle the line work by day & night => day => night => disabled";
                TLMUtils.initButton(buttonCycleTime, true, "ButtonMenu");
                buttonCycleTime.name = "CycleTime";
                buttonCycleTime.isVisible = true;
                buttonCycleTime.eventClick += (component, eventParam) =>
                {
                    bool dayActive, nightActive;
                    Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].GetActive(out dayActive, out nightActive);
                    int selection = (dayActive ? 0 : 2) + (nightActive ? 0 : 1);
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        switch (selection)
                        {
                            case 3:
                                Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].SetActive(true, true);
                                lineTime.backgroundSprite = "";
                                break;
                            case 0:
                                Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].SetActive(true, false);
                                lineTime.backgroundSprite = "DayIcon";
                                break;
                            case 1:
                                Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].SetActive(false, true);
                                lineTime.backgroundSprite = "NightIcon";
                                break;
                            case 2:
                                Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].SetActive(false, false);
                                lineTime.backgroundSprite = "DisabledIcon";
                                break;
                        }
                    });
                };

                UIButton buttonAutoName = null;
                TLMUtils.createUIElement<UIButton>(ref buttonAutoName, container.transform);
                buttonAutoName.pivot = UIPivotPoint.TopRight;
                buttonAutoName.relativePosition = new Vector3(container.width - 10f, 20f);
                buttonAutoName.text = "Use Auto Name";
                buttonAutoName.textScale = 0.6f;
                buttonAutoName.width = 100;
                buttonAutoName.height = 15;
                buttonAutoName.tooltip = "Use auto name in this line";
                TLMUtils.initButton(buttonAutoName, true, "ButtonMenu");
                buttonAutoName.name = "AutoName";
                buttonAutoName.isVisible = true;
                buttonAutoName.eventClick += (component, eventParam) =>
                {
                    lineName.text = string.Format("[{0}] {1}", lineNum, TLMUtils.calculateAutoName(lineId));
                    TLMUtils.setLineName(lineId, lineName.text);
                };

                UIButton buttonAutoColor = null;
                TLMUtils.createUIElement<UIButton>(ref buttonAutoColor, container.transform);
                buttonAutoColor.pivot = UIPivotPoint.TopRight;
                buttonAutoColor.relativePosition = new Vector3(container.width - 10f, 35f);
                buttonAutoColor.text = "Use Auto Color";
                buttonAutoColor.textScale = 0.6f;
                buttonAutoColor.width = 100;
                buttonAutoColor.height = 15;
                buttonAutoColor.tooltip = "Pick a color from the palette for this line";
                TLMUtils.initButton(buttonAutoColor, true, "ButtonMenu");
                buttonAutoColor.name = "AutoColor";
                buttonAutoColor.isVisible = true;
                buttonAutoColor.eventClick += (component, eventParam) =>
                {
                    TLMUtils.setLineColor(lineId, TLMController.instance.AutoColor(lineId));
                    itemButton.color = m_controller.tm.GetLineColor(lineId);
                };

            }
        }

        private void createMainView()
        {

            UIPanel container = m_controller.mainRef.Find<UIPanel>("Container");
            TLMUtils.createUIElement<UIPanel>(ref mainPanel, m_controller.mainRef.transform);
            mainPanel.Hide();
            mainPanel.relativePosition = new Vector3(394.0f, 0.0f);
            mainPanel.width = 480;
            mainPanel.height = 430;
            mainPanel.color = new Color32(255, 255, 255, 255);
            mainPanel.backgroundSprite = "MenuPanel2";
            mainPanel.name = "TransportLinesManagerPanel";

            TLMUtils.createDragHandle(mainPanel, mainPanel, 35f);
            if (!TransportLinesManagerMod.isIPTCompatibiltyMode)
            {
                addIcon(60, "BulletTrain", "BulletTrainImage", ref bulletTrainLeg, CurrentFilterSelected.BULLET, true);
                addIcon(160, "SurfaceMetro", "SurfaceMetroImage", ref surfaceMetroLeg, CurrentFilterSelected.SURFACE_METRO, true);
                addIcon(410, "LowBus", "LowBusImage", ref lowBusLeg, CurrentFilterSelected.LOW_BUS, true);
                addIcon(310, "HighBus", "HighBusImage", ref highBusLeg, CurrentFilterSelected.HIGH_BUS, true);
            }

            addIcon(10, "ShipLine", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Ship), ref shipLeg, CurrentFilterSelected.SHIP, false);
            addIcon(110, "Train", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train), ref trainLeg, CurrentFilterSelected.REGIONAL_TRAIN, false);
            addIcon(210, "Subway", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro), ref metroLeg, CurrentFilterSelected.METRO, false);
            addIcon(260, "Tram", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Tram), ref tramLeg, CurrentFilterSelected.TRAM, false);
            addIcon(360, "Bus", PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus), ref busLeg, CurrentFilterSelected.REGULAR_BUS, false);

            UILabel titleLabel = null;
            TLMUtils.createUIElement<UILabel>(ref titleLabel, mainPanel.transform);
            titleLabel.relativePosition = new Vector3(0, 15f);
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.text = "Transport Lines Manager v" + TransportLinesManagerMod.version;
            titleLabel.autoSize = false;
            titleLabel.width = mainPanel.width;
            titleLabel.height = 30;
            titleLabel.name = "TransportLinesManagerLabelTitle";
            TLMUtils.createDragHandle(titleLabel, mainPanel);
        }

        private void addIcon(int xpos, string namePrefix, string iconName, ref UIButton targetButton, CurrentFilterSelected onClick, bool alternativeIconAtlas)
        {
            TLMUtils.doLog("addIcon: init " + namePrefix);
            TLMUtils.createUIElement<UIButton>(ref targetButton, mainPanel.transform);
            TLMUtils.doLog("addIcon: targetButton created");

            targetButton.atlas = TLMController.taLineNumber;
            targetButton.width = 40;
            targetButton.height = 40;
            targetButton.relativePosition = new Vector3(xpos, 45);
            targetButton.name = namePrefix + "Legend";
            TLMUtils.initButtonSameSprite(targetButton, namePrefix + "Icon");
            targetButton.hoveredColor = Color.gray;
            targetButton.focusedColor = Color.white;
            TLMUtils.doLog("addIcon: pre eventClick");
            targetButton.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                currentSelection = onClick;
            };
            TLMUtils.doLog("addIcon: init label icon");
            UILabel icon = targetButton.AddUIComponent<UILabel>();
            if (alternativeIconAtlas)
            {
                icon.atlas = TLMController.taLineNumber;
                icon.width = 27;
                icon.height = 27;
                icon.relativePosition = new Vector3(6f, 6);
            }
            else
            {
                icon.width = 30;
                icon.height = 20;
                icon.relativePosition = new Vector3(5f, 10f);
            }
            icon.backgroundSprite = iconName;
            TLMUtils.doLog("addIcon: end");
        }

        private string setLineNumberMainListing(int num, UIButton button, ModoNomenclatura prefix, Separador s, ModoNomenclatura sufix, bool zeros, bool invertPrefixSuffix)
        {

            UILabel l = button.AddUIComponent<UILabel>();
            l.autoSize = false;
            l.autoHeight = false;
            l.pivot = UIPivotPoint.TopLeft;
            l.verticalAlignment = UIVerticalAlignment.Middle;
            l.textAlignment = UIHorizontalAlignment.Center;
            l.relativePosition = new Vector3(0, 0);
            l.width = button.width;
            l.height = button.height;
            l.useOutline = true;
            l.text = TLMUtils.getString(prefix, s, sufix, num, zeros, invertPrefixSuffix);
            float ratio = l.width / 50;
            TLMLineUtils.setLineNumberCircleOnRef(num, prefix, s, sufix, zeros, l, invertPrefixSuffix, ratio);
            return l.text;
        }

        //botoes da antiga parte extra
        private void createResetAllLinesNamingButton()
        {
            TLMUtils.createUIElement<UIButton>(ref resetLineNames, mainPanel.transform);
            resetLineNames.width = mainPanel.width / 2 - 15;
            resetLineNames.height = 30;
            resetLineNames.relativePosition = new Vector3(mainPanel.width - resetLineNames.width - 10f, mainPanel.height - 40f);
            resetLineNames.text = "Reset all names";
            resetLineNames.tooltip = "Will reset all names to default name, under the current naming strategy";
            TLMUtils.initButton(resetLineNames, true, "ButtonMenu");
            resetLineNames.name = "RenameAllButton";
            resetLineNames.isVisible = true;
            resetLineNames.eventClick += (component, eventParam) =>
            {
                for (ushort i = 0; i < controller.tm.m_lines.m_size; i++)
                {
                    TransportLine t = controller.tm.m_lines.m_buffer[(int)i];
                    if ((t.m_flags & (TransportLine.Flags.Created)) != TransportLine.Flags.None && t.m_lineNumber > 0)
                    {
                        controller.AutoName(i);
                    }
                }
                Show();
            };
        }

        private void createLinesDrawButton()
        {
            UIButton createLineDraw = null;
            TLMUtils.createUIElement<UIButton>(ref createLineDraw, mainPanel.transform);
            createLineDraw.width = mainPanel.width / 2 - 15;
            createLineDraw.height = 30;
            createLineDraw.relativePosition = new Vector3(mainPanel.width - createLineDraw.width - 10f, mainPanel.height + 10f);
            createLineDraw.text = "DRAW MAP!";
            createLineDraw.tooltip = "DRAW MAP!";
            TLMUtils.initButton(createLineDraw, true, "ButtonMenu");
            createLineDraw.name = "DrawMapButton";
            createLineDraw.isVisible = true;
            createLineDraw.eventClick += (component, eventParam) =>
            {
                TLMMapDrawer.drawCityMap();
            };
        }

        private void createResetAllLinesColorButton()
        {
            TLMUtils.createUIElement<UIButton>(ref resetLineColor, mainPanel.transform);
            resetLineColor.relativePosition = new Vector3(10f, mainPanel.height - 40f);
            resetLineColor.text = "Reset all colors";
            resetLineColor.tooltip = "Will reset all colors to default, under the current color pallet strategy if it's actived (based in lines' numbers)";
            resetLineColor.width = mainPanel.width / 2 - 15;
            resetLineColor.height = 30;
            TLMUtils.initButton(resetLineColor, true, "ButtonMenu");
            resetLineColor.name = "RecolorAllButton";
            resetLineColor.isVisible = TransportLinesManagerMod.instance.currentLoadedCityConfig.getBool(TLMCW.ConfigIndex.AUTO_COLOR_ENABLED);
            resetLineColor.eventClick += (component, eventParam) =>
            {
                for (ushort i = 0; i < controller.tm.m_lines.m_size; i++)
                {
                    TransportLine t = controller.tm.m_lines.m_buffer[(int)i];
                    if ((t.m_flags & (TransportLine.Flags.Created)) != TransportLine.Flags.None)
                    {
                        controller.AutoColor(i);
                    }
                }
                Show();
            };
        }



        internal void updateBidings()
        {
            if (filteredLinesListPanel.isVisible)
            {
                foreach (var lineGO in linesGameObjects)
                {
                    ushort lineId = lineGO.GetComponentInChildren<UIButtonLineInfo>().lineID;
                    float lineLength = TLMLineUtils.GetLineLength(lineId);
                    int stopsCount = TLMLineUtils.GetStopsCount(lineId);
                    var vehicles = TLMLineUtils.GetVehiclesCount(lineId);
                    TransportLine t = m_controller.tm.m_lines.m_buffer[lineId];

                    string line1 = string.Format("{0} Stp | {1}m Len | {2} Res | {3} Tou", stopsCount, lineLength.ToString("#,##0.0"), t.m_passengers.m_residentPassengers.m_averageCount.ToString("#,##0"), t.m_passengers.m_touristPassengers.m_averageCount.ToString("#,##0"));
                    string line2 = string.Format("{0} Vehicles | Waiting lap end for more stats...", vehicles);
                    var stats = ExtraVehiclesStats.instance.getLineVehiclesData(lineId);
                    if (stats.Count > 0)
                    {
                        List<float> fill = new List<float>();
                        List<float> stdDevs = new List<float>();
                        List<long> lapTimes = new List<long>();
                        foreach (var kv in stats)
                        {
                            fill.Add(kv.Value.avgFill);
                            stdDevs.Add(kv.Value.stdDevFill);
                            lapTimes.Add(kv.Value.framesTakenLap);
                        }
                        line2 = string.Format("Avg Fill: {0} ± {1} | Avg Lap: {2} | {3}/{4} Veh", fill.Average().ToString("0.0%"), stdDevs.Average().ToString("0.0%"), ExtraVehiclesStats.ExtraData.framesToDaysTakenLapFormated((long)lapTimes.Average()), stats.Count, vehicles);
                    }
                    var lineExtraInfo1 = lineGO.transform.Find("LineExtraInfo1").GetComponent<UILabel>();
                    var lineExtraInfo2 = lineGO.transform.Find("LineExtraInfo2").GetComponent<UILabel>();

                    lineExtraInfo1.text = line1;
                    lineExtraInfo2.text = line2;
                    lineExtraInfo1.tooltip = line1;
                    lineExtraInfo2.tooltip = line2;
                }
            }
        }

        private enum CurrentFilterSelected
        {
            NONE,
            BULLET,
            REGIONAL_TRAIN,
            SURFACE_METRO,
            METRO,
            TRAM,
            HIGH_BUS,
            REGULAR_BUS,
            LOW_BUS,
            SHIP
        }

    }





}
