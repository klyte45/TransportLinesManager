using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.LineList;
using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.Utils;

namespace Klyte.TransportLinesManager
{
    public class TLMController
    {
        private static TLMController _instance;

        public static TLMController instance
        {
            get {
                if (_instance == null) {
                    _instance = new TLMController();
                }
                return _instance;
            }
        }

        public static UITextureAtlas taTLM = null;
        public static UITextureAtlas taLineNumber = null;
        public UIView uiView;
        public UIComponent mainRef;
        public TransportManager tm
        {
            get {
                return Singleton<TransportManager>.instance;
            }
        }
        public InfoManager im
        {
            get {
                return Singleton<InfoManager>.instance;
            }
        }
        public UIButton abrePainelButton;
        public bool initialized = false;
        public bool initializedWIP = false;
        private TLMLineInfoPanel m_lineInfoPanel;
        private int lastLineCount = 0;

        private UIPanel _cachedDefaultListingLinesPanel;

        public UIPanel defaultListingLinesPanel
        {
            get {
                if (_cachedDefaultListingLinesPanel == null) {
                    _cachedDefaultListingLinesPanel = GameObject.Find("UIView").GetComponentInChildren<TLMPublicTransportDetailPanel>().GetComponent<UIPanel>();
                }
                return _cachedDefaultListingLinesPanel;
            }
        }


        public TLMLineInfoPanel lineInfoPanel
        {
            get {
                return m_lineInfoPanel;
            }
        }

        public Transform transform
        {
            get {
                return mainRef.transform;
            }
        }

        private TLMController() { }

        public void destroy()
        {
            if (abrePainelButton != null && abrePainelButton.gameObject != null) {
                UnityEngine.Object.Destroy(abrePainelButton.gameObject);
            }
            if (m_lineInfoPanel != null && m_lineInfoPanel.linearMap != null && m_lineInfoPanel.linearMap.gameObject != null) {
                UnityEngine.Object.Destroy(m_lineInfoPanel.linearMap.gameObject);
            }

            if (m_lineInfoPanel != null && m_lineInfoPanel.gameObject != null) {
                UnityEngine.Object.Destroy(m_lineInfoPanel.gameObject);
            }

            initialized = false;
            initializedWIP = false;

            _instance = null;
        }

        public void update()
        {
            if (!GameObject.FindGameObjectWithTag("GameController") || ((GameObject.FindGameObjectWithTag("GameController").GetComponent<ToolController>()).m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
                TLMUtils.doErrorLog("GameController NOT FOUND!");
                return;
            }
            if (!initialized) {
                TransportLinesManagerMod.instance.loadTLMLocale(false);

                uiView = GameObject.FindObjectOfType<UIView>();
                if (!uiView)
                    return;
                mainRef = uiView.FindUIComponent<UIPanel>("InfoPanel").Find<UITabContainer>("InfoViewsContainer").Find<UIPanel>("InfoViewsPanel");
                if (!mainRef)
                    return;
                mainRef.eventVisibilityChanged += delegate (UIComponent component, bool b) {
                    if (b) {
                        TransportLinesManagerMod.instance.showVersionInfoPopup();
                    }
                };


                createViews();
                mainRef.clipChildren = false;
                initialized = true;
            }

            initNearLinesOnWorldInfoPanel();

            if (m_lineInfoPanel.isVisible) {
                m_lineInfoPanel.updateBidings();
            }

            lastLineCount = tm.m_lineCount;
            TLMPublicTransportDetailPanelHooks.instance.update();

            return;
        }
        const int maxTryLoads = 100;

        public Color AutoColor(ushort i)
        {
            TransportLine t = tm.m_lines.m_buffer[(int) i];
            try {
                TLMCW.ConfigIndex transportType = TLMCW.getDefinitionForLine(i).toConfigIndex();
                bool prefixBased = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_PREFIX_BASED);

                bool randomOnOverflow = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);

                string pal = TLMCW.getCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_SUBLINE);
                ushort num = t.m_lineNumber;
                if (num >= 1000 && TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX) != (int) ModoNomenclatura.Nenhum) {
                    pal = TLMCW.getCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_MAIN);
                    if (prefixBased) {
                        num /= 1000;
                    } else {
                        num %= 1000;
                    }
                }
                Color c = TLMAutoColorPalettes.getColor(num, pal, randomOnOverflow);
                TLMUtils.setLineColor(i, c);
                //TLMUtils.doLog("Colocada a cor {0} na linha {1} ({3} {2})", c, i, t.m_lineNumber, t.Info.m_transportType);
                return c;
            } catch (Exception e) {
                TLMUtils.doErrorLog("ERRO!!!!! " + e.Message);
                TLMCW.setCurrentConfigBool(TLMCW.ConfigIndex.AUTO_COLOR_ENABLED, false);
                return Color.clear;
            }
        }


        //NAVEGACAO

        private void swapWindow(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lineInfoPanel.isVisible || defaultListingLinesPanel.isVisible) {
                fecharTelaTransportes(component, eventParam);
            } else {
                abrirTelaTransportes(component, eventParam);
            }

        }

        private void abrirTelaTransportes(UIComponent component, UIMouseEventParameter eventParam)
        {
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE1!");
            abrePainelButton.normalFgSprite = abrePainelButton.focusedFgSprite;
            m_lineInfoPanel.Hide();
            defaultListingLinesPanel.Show();
            tm.LinesVisible = 0x7FFFFFFF;
            //			MainMenu ();
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE2!");
        }

        private void fecharTelaTransportes(UIComponent component, UIFocusEventParameter eventParam)
        {
            fecharTelaTransportes(component, (UIMouseEventParameter) null);
        }

        private void fecharTelaTransportes(UIComponent component, UIMouseEventParameter eventParam)
        {
            abrePainelButton.normalFgSprite = abrePainelButton.disabledFgSprite;
            defaultListingLinesPanel.Hide();
            m_lineInfoPanel.Hide();
            tm.LinesVisible = 0;
            InfoManager im = Singleton<InfoManager>.instance;
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "FECHA!");
        }

        private void createViews()
        {
            /////////////////////////////////////////////////////	
            m_lineInfoPanel = new TLMLineInfoPanel(this);
        }

        private void initNearLinesOnWorldInfoPanel()
        {
            if (!initializedWIP) {
                UIPanel parent = GameObject.Find("UIView").transform.GetComponentInChildren<CityServiceWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent == null)
                    return;
                parent.eventVisibilityChanged += (component, value) => {
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInCityServicesWorldInfoPanel.value ? parent : null, true);
                };
                parent.eventPositionChanged += (component, value) => {
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInCityServicesWorldInfoPanel.value ? parent : null, true);
                };

                UIPanel parent2 = GameObject.Find("UIView").transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent2 == null)
                    return;

                parent2.eventVisibilityChanged += (component, value) => {
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInZonedBuildingWorldInfoPanel.value ? parent2 : null, true);
                };
                parent2.eventPositionChanged += (component, value) => {
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInZonedBuildingWorldInfoPanel.value ? parent2 : null, true);
                };
                UIPanel parent3 = GameObject.Find("UIView").transform.GetComponentInChildren<PublicTransportWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent3 == null)
                    return;

                parent3.eventVisibilityChanged += (component, value) => {
                    if (TransportLinesManagerMod.overrideWorldInfoPanelLine && value) {

                        PublicTransportWorldInfoPanel ptwip = parent3.gameObject.GetComponent<PublicTransportWorldInfoPanel>();
                        ptwip.StartCoroutine(OpenLineInfo(ptwip));
                        ptwip.Hide();
                    }
                };

                initializedWIP = true;
            }
        }

        private IEnumerator OpenLineInfo(PublicTransportWorldInfoPanel ptwip)
        {
            yield return 0;
            ushort lineId = 0;
            while (lineId == 0) {
                lineId = (ushort) (typeof(PublicTransportWorldInfoPanel).GetMethod("GetLineID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance).Invoke(ptwip, new object[0]));
            }
            TLMController.instance.lineInfoPanel.openLineInfo(lineId);

        }

        private ushort lastBuildingSelected = 0;

        private void updateNearLines(UIPanel parent, bool force = false)
        {
            if (parent != null) {
                Transform linesPanelObj = parent.transform.Find("TLMLinesNear");
                if (!linesPanelObj) {
                    linesPanelObj = initPanelNearLinesOnWorldInfoPanel(parent);
                }
                var prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                ushort buildingId = ((InstanceID) (prop.GetValue(parent.gameObject.GetComponent<WorldInfoPanel>()))).Building;
                if (lastBuildingSelected == buildingId && !force) {
                    return;
                } else {
                    lastBuildingSelected = buildingId;
                }
                Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];

                List<ushort> nearLines = new List<ushort>();

                TLMLineUtils.GetNearLines(b.CalculateSidewalkPosition(), 120f, ref nearLines);
                bool showPanel = nearLines.Count > 0;
                //				DebugOutputPanel.AddMessage (PluginManager.MessageType.Warning, "nearLines.Count = " + nearLines.Count);
                if (showPanel) {
                    foreach (Transform t in linesPanelObj) {
                        if (t.GetComponent<UILabel>() == null) {
                            GameObject.Destroy(t.gameObject);
                        }
                    }
                    Dictionary<string, ushort> lines = TLMLineUtils.SortLines(nearLines);
                    TLMLineUtils.PrintIntersections("", "", "", "", linesPanelObj.GetComponent<UIPanel>(), lines, scale, perLine);
                }
                linesPanelObj.GetComponent<UIPanel>().isVisible = showPanel;
            } else {
                var go = GameObject.Find("TLMLinesNear");
                if (!go) {
                    return;
                }
                Transform linesPanelObj = go.transform;
                linesPanelObj.GetComponent<UIPanel>().isVisible = false;
            }
        }



        private float scale = 1f;
        private int perLine = 9;

        private Transform initPanelNearLinesOnWorldInfoPanel(UIPanel parent)
        {
            UIPanel saida = parent.AddUIComponent<UIPanel>();
            saida.relativePosition = new Vector3(0, parent.height);
            saida.width = parent.width;
            saida.autoFitChildrenVertically = true;
            saida.autoLayout = true;
            saida.autoLayoutDirection = LayoutDirection.Horizontal;
            saida.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            saida.padding = new RectOffset(2, 2, 2, 2);
            saida.autoLayoutStart = LayoutStart.TopLeft;
            saida.wrapLayout = true;
            saida.name = "TLMLinesNear";
            saida.backgroundSprite = "GenericPanel";
            UILabel title = saida.AddUIComponent<UILabel>();
            title.autoSize = false;
            title.width = saida.width;
            title.textAlignment = UIHorizontalAlignment.Left;
            title.localeID = "TLM_NEAR_LINES";
            title.useOutline = true;
            title.height = 18;
            return saida.transform;
        }



    }


}