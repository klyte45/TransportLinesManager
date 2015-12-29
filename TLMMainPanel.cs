using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager
{
    public class TLMMainPanel
    {
        private TLMController m_controller;
        private UIPanel mainPanel;
        private List<GameObject> linesButtons = new List<GameObject>();
        private float offset;
        private Dictionary<Int32, UInt16> trensList;
        private Dictionary<Int32, UInt16> metroList;
        private Dictionary<Int32, UInt16> tramList;
        private Dictionary<Int32, UInt16> bulletTrainList;
        private Dictionary<Int32, UInt16> onibusList;
        private UIButton trensLeg;
        private UIButton metroLeg;
        private UIButton onibusLeg;
        private UIButton tramLeg;
        private UIButton bulletTrainLeg;
        private UIScrollablePanel linesListPanel;

        //botoes da parte das antigas configuraçoes		
        private UIButton resetLineNames;
        private UIButton resetLineColor;

        public void Show()
        {
            mainPanel.Show();
            clearLinhas();
            listaLinhas();
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
                catch (Exception e)
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
                    listaLinhas();
                }
                return bulletTrainList;
            }
        }

        public Dictionary<Int32, UInt16> trens
        {
            get
            {
                if (trensList == null)
                {
                    listaLinhas();
                }
                return trensList;
            }
        }
        public Dictionary<Int32, UInt16> trams
        {
            get
            {
                if (tramList == null)
                {
                    listaLinhas();
                }
                return tramList;
            }
        }

        public Dictionary<Int32, UInt16> metro
        {
            get
            {
                if (metroList == null)
                {
                    listaLinhas();
                }
                return metroList;
            }
        }

        public Dictionary<Int32, UInt16> onibus
        {
            get
            {
                if (onibusList == null)
                {
                    listaLinhas();
                }
                return onibusList;
            }
        }

        private void clearLinhas()
        {

            foreach (GameObject o in linesButtons)
            {
                UnityEngine.Object.Destroy(o);
            }
            linesButtons.Clear();
            //			linesListPanel.ScrollToTop ();
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
            linesListPanel = scrollObj.GetComponent<UIScrollablePanel>();
            linesListPanel.autoLayout = false;
            linesListPanel.width = mainPanel.width;
            linesListPanel.height = 290;
            linesListPanel.useTouchMouseScroll = true;
            linesListPanel.scrollWheelAmount = 20;
            linesListPanel.eventMouseWheel += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                linesListPanel.scrollPosition -= new Vector2(0, eventParam.wheelDelta * linesListPanel.scrollWheelAmount);
            };
            panelListing.AttachUIComponent(linesListPanel.gameObject);
            linesListPanel.relativePosition = new Vector3(0, 0);

            //botoes da antiga parte extra 

            createResetAllLinesNamingButton();
            createResetAllLinesColorButton();
            //  createLinesDrawButton();

        }

        private void listaLinhas()
        {

            trensList = new Dictionary<int, ushort>();
            metroList = new Dictionary<int, ushort>();
            onibusList = new Dictionary<int, ushort>();
            tramList = new Dictionary<int, ushort>();
            bulletTrainList = new Dictionary<int, ushort>();

            for (ushort i = 0; i < m_controller.tm.m_lines.m_size; i++)
            {
                TransportLine t = m_controller.tm.m_lines.m_buffer[(int)i];
                if (t.m_lineNumber == 0 || t.CountStops(i) == 0)
                    continue;
                switch (t.Info.m_transportType)
                {
                    case TransportInfo.TransportType.Bus:
                        while (onibusList.ContainsKey(t.m_lineNumber))
                        {
                            t.m_lineNumber++;
                        }
                        onibusList.Add(t.m_lineNumber, i);
                        break;

                    case TransportInfo.TransportType.Metro:
                        while (metroList.ContainsKey(t.m_lineNumber))
                        {
                            t.m_lineNumber++;
                        }
                        metroList.Add(t.m_lineNumber, i);
                        break;

                    case TransportInfo.TransportType.Train:
                        if (TLMCW.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.TRAM_LINES_IDS).Contains(i))
                        {
                            while (tramList.ContainsKey(t.m_lineNumber))
                            {
                                t.m_lineNumber++;
                            }
                            tramList.Add(t.m_lineNumber, i);
                        }
                        else if (TLMCW.getCurrentConfigListInt(TLMConfigWarehouse.ConfigIndex.BULLET_TRAIN_LINES_IDS).Contains(i))
                        {
                            while (bulletTrainList.ContainsKey(t.m_lineNumber))
                            {
                                t.m_lineNumber++;
                            }
                            bulletTrainList.Add(t.m_lineNumber, i);
                        }
                        else {
                            while (trensList.ContainsKey(t.m_lineNumber))
                            {
                                t.m_lineNumber++;
                            }
                            trensList.Add(t.m_lineNumber, i);
                        }
                        break;
                    default:
                        continue;
                }
            }
            offset = 0;
            offset += drawButtonsFromDictionary(bulletTrainList, offset);
            offset += drawButtonsFromDictionary(trensList, offset);
            offset += drawButtonsFromDictionary(tramList, offset);
            offset += drawButtonsFromDictionary(metroList, offset);
            offset += drawButtonsFromDictionary(onibusList, offset);
        }

        private float drawButtonsFromDictionary(Dictionary<Int32, UInt16> map, float offset)
        {
            int j = 0;
            List<Int32> keys = map.Keys.ToList();
            keys.Sort();
            foreach (Int32 k in keys)
            {

                TransportLine t = m_controller.tm.m_lines.m_buffer[map[k]];
                //				string item = "[" + t.Info.m_transportType + " | " + t.m_lineNumber + "] " + t.GetColor () + " " + tli.tm.GetLineName ( map [k]);
                GameObject itemContainer = new GameObject();
                linesButtons.Add(itemContainer);

                itemContainer.transform.parent = linesListPanel.transform;
                UIButtonLineInfo itemButton = itemContainer.AddComponent<UIButtonLineInfo>();

                itemButton.relativePosition = new Vector3(10.0f + (j % 10) * 40f, offset + 40 * (int)(j / 10));
                itemButton.width = 35;
                itemButton.height = 35;
                TLMUtils.initButton(itemButton, true, "ButtonMenu");
                itemButton.atlas = TLMController.taLineNumber;
                ModoNomenclatura mn, pre;
                Separador s;
                bool z;
                string icon;
                TLMLineUtils.getLineNamingParameters(map[k], out pre, out s, out mn, out z, out icon);
                TLMUtils.initButtonSameSprite(itemButton, icon);

                itemButton.color = m_controller.tm.GetLineColor(map[k]);
                itemButton.hoveredTextColor = itemButton.color;
                itemButton.textColor = TLMUtils.contrastColor(t.GetColor());
                itemButton.hoveredColor = itemButton.textColor;
                itemButton.tooltip = m_controller.tm.GetLineName((ushort)map[k]);
                itemButton.lineID = map[k];
                itemButton.eventClick += m_controller.lineInfoPanel.openLineInfo;
                setLineNumberMainListing(t.m_lineNumber, itemButton, pre, s, mn, z);

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

        private void createMainView()
        {

            UIPanel container = m_controller.mainRef.Find<UIPanel>("Container");
            TLMUtils.createUIElement<UIPanel>(ref mainPanel, m_controller.mainRef.transform);
            mainPanel.Hide();
            mainPanel.relativePosition = new Vector3(394.0f, 0.0f);
            mainPanel.width = 420;
            mainPanel.height = 430;
            mainPanel.color = new Color32(255, 255, 255, 255);
            mainPanel.backgroundSprite = "MenuPanel2";
            mainPanel.name = "TransportLinesManagerPanel";

            TLMUtils.createDragHandle(mainPanel, mainPanel, 35f);

            TLMUtils.createUIElement<UIButton>(ref bulletTrainLeg, mainPanel.transform);
            bulletTrainLeg.atlas = TLMController.taLineNumber;
            bulletTrainLeg.width = 40;
            bulletTrainLeg.height = 40;
            bulletTrainLeg.name = "BulletTrain";
            bulletTrainLeg.relativePosition = new Vector3(80, 45);
            TLMUtils.initButtonSameSprite(bulletTrainLeg, "BulletTrainIcon");
            UILabel bulletIcon = bulletTrainLeg.AddUIComponent<UILabel>();
            bulletIcon.atlas = TLMController.taLineNumber;
            bulletIcon.backgroundSprite = "BulletTrainImage";
            bulletIcon.width = 27;
            bulletIcon.height = 27;
            bulletIcon.relativePosition = new Vector3(6f, 6);

            TLMUtils.createUIElement<UIButton>(ref trensLeg, mainPanel.transform);
            trensLeg.atlas = TLMController.taLineNumber;
            trensLeg.width = 40;
            trensLeg.height = 40;
            trensLeg.name = "TrainLegend";
            trensLeg.relativePosition = new Vector3(140, 45);
            TLMUtils.initButtonSameSprite(trensLeg, "TrainIcon");
            UILabel tremIcon = trensLeg.AddUIComponent<UILabel>();
            tremIcon.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Train);
            tremIcon.width = 30;
            tremIcon.height = 20;
            tremIcon.relativePosition = new Vector3(5f, 10f);


            TLMUtils.createUIElement<UIButton>(ref tramLeg, mainPanel.transform);
            tramLeg.atlas = TLMController.taLineNumber;
            tramLeg.width = 40;
            tramLeg.height = 40;
            tramLeg.relativePosition = new Vector3(200, 45);
            tramLeg.name = "TramLegend";
            TLMUtils.initButtonSameSprite(tramLeg, "TramIcon");
            UILabel tramIcon = tramLeg.AddUIComponent<UILabel>();
            tramIcon.atlas = TLMController.taLineNumber;
            tramIcon.backgroundSprite = "TramImage";
            tramIcon.width = 27;
            tramIcon.height = 27;
            tramIcon.relativePosition = new Vector3(6f, 6);

            TLMUtils.createUIElement<UIButton>(ref metroLeg, mainPanel.transform);
            metroLeg.atlas = TLMController.taLineNumber;
            metroLeg.width = 40;
            metroLeg.height = 40;
            metroLeg.relativePosition = new Vector3(260, 45);
            metroLeg.name = "SubwayLegend";
            TLMUtils.initButtonSameSprite(metroLeg, "SubwayIcon");
            UILabel metroIcon = metroLeg.AddUIComponent<UILabel>();
            metroIcon.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Metro);
            metroIcon.width = 30;
            metroIcon.height = 20;
            metroIcon.relativePosition = new Vector3(5f, 10f);


            TLMUtils.createUIElement<UIButton>(ref onibusLeg, mainPanel.transform);
            onibusLeg.atlas = TLMController.taLineNumber;
            onibusLeg.width = 40;
            onibusLeg.height = 40;
            onibusLeg.relativePosition = new Vector3(320, 45);
            onibusLeg.name = "BusLegend";
            TLMUtils.initButtonSameSprite(onibusLeg, "BusIcon");
            UILabel onibusIcon = onibusLeg.AddUIComponent<UILabel>();
            onibusIcon.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(TransportInfo.TransportType.Bus);
            onibusIcon.width = 30;
            onibusIcon.height = 20;
            onibusIcon.relativePosition = new Vector3(5f, 10f);




            UILabel titleLabel = null;
            TLMUtils.createUIElement<UILabel>(ref titleLabel, mainPanel.transform);
            titleLabel.relativePosition = new Vector3(0, 15f);
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.text = "Transport Lines Manager v" + TransportLinesManagerMod.version + "";
            titleLabel.autoSize = false;
            titleLabel.width = mainPanel.width;
            titleLabel.height = 30;
            titleLabel.name = "TransportLinesManagerLabelTitle";
            TLMUtils.createDragHandle(titleLabel, mainPanel);
        }

        private void setLineNumberMainListing(int num, UIButton button, ModoNomenclatura prefix, Separador s, ModoNomenclatura sufix, bool zeros)
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
            l.text = TLMUtils.getString(prefix, s, sufix, num, zeros);
            float ratio = l.width / 50;
            TLMLineUtils.setLineNumberCircleOnRef(num, prefix, s, sufix, zeros, l, ratio);
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

    }





}
