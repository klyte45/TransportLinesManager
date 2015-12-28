using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager
{
    public class TLMController
    {
        public static TLMController instance;
        public static UITextureAtlas taTLM = null;
        public static UITextureAtlas taLineNumber = null;
        public UIView uiView;
        public UIComponent mainRef;
        public TransportManager tm;
        public InfoManager im;
        public UIButton abrePainelButton;
        public bool initialized = false;
        public bool initializedWIP = false;
        private TLMMainPanel m_mainPanel;
        private TLMLineInfoPanel m_lineInfoPanel;
        private int lastLineCount = 0;



        public TLMMainPanel mainPanel
        {
            get
            {
                return m_mainPanel;
            }
        }

        public TLMLineInfoPanel lineInfoPanel
        {
            get
            {
                return m_lineInfoPanel;
            }
        }

        public Transform transform
        {
            get
            {
                return mainRef.transform;
            }
        }

        public TLMController()
        {
        }

        public void destroy()
        {



            if (m_mainPanel != null && m_mainPanel.gameObject != null)
            {
                UnityEngine.Object.Destroy(m_mainPanel.gameObject);
            }

            if (abrePainelButton != null && abrePainelButton.gameObject != null)
            {
                UnityEngine.Object.Destroy(abrePainelButton.gameObject);
            }
            if (m_lineInfoPanel != null && m_lineInfoPanel.linearMap != null && m_lineInfoPanel.linearMap.gameObject != null)
            {
                UnityEngine.Object.Destroy(m_lineInfoPanel.linearMap.gameObject);
            }

            if (m_lineInfoPanel != null && m_lineInfoPanel.gameObject != null)
            {
                UnityEngine.Object.Destroy(m_lineInfoPanel.gameObject);
            }

            initialized = false;
            initializedWIP = false;
        }

        public void init()
        {
            if (!GameObject.FindGameObjectWithTag("GameController") || ((GameObject.FindGameObjectWithTag("GameController").GetComponent<ToolController>()).m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None)
            {
                return;
            }
            if (!initialized)
            {


                uiView = GameObject.FindObjectOfType<UIView>();
                if (!uiView)
                    return;
                mainRef = uiView.FindUIComponent<UIPanel>("InfoPanel").Find<UITabContainer>("InfoViewsContainer").Find<UIPanel>("InfoViewsPanel");
                if (!mainRef)
                    return;


                tm = Singleton<TransportManager>.instance;
                im = Singleton<InfoManager>.instance;
                createViews();
                mainRef.clipChildren = false;
                UIPanel container = mainRef.Find<UIPanel>("Container");
                abrePainelButton = container.Find<UIButton>("PublicTransport");
                //				container.AttachUIComponent (abrePainelButton.gameObject);


                abrePainelButton.atlas = taTLM;
                abrePainelButton.tooltip = "Transport Lines Manager (v" + TransportLinesManagerMod.version + ")";
                abrePainelButton.name = "TransportLinesManagerButton";
                TLMUtils.initButtonFg(abrePainelButton, false, "TransportLinesManagerIcon");
                abrePainelButton.eventClick += swapWindow;
                abrePainelButton.eventVisibilityChanged += (UIComponent component, bool value) =>
                {
                    if (!value)
                    {
                        fecharTelaTransportes(component, (UIMouseEventParameter)null);
                    }
                };


                container.height = 37 * ((int)((container.childCount + 1) / 2)) + 6;
                initialized = true;
            }

            initNearLinesOnWorldInfoPanel();

            if (m_mainPanel.isVisible || m_lineInfoPanel.isVisible)
            {
                if (!tm.LinesVisible)
                {
                    tm.LinesVisible = true;
                }
                if (im.CurrentMode != InfoManager.InfoMode.Transport)
                {
                    im.SetCurrentMode(InfoManager.InfoMode.Transport, InfoManager.SubInfoMode.NormalTransport);
                }
            }
            if (m_lineInfoPanel.isVisible)
            {
                m_lineInfoPanel.updateBidings();
            }

            if (lastLineCount != tm.m_lineCount && (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.AUTO_COLOR_ENABLED) || TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.AUTO_NAME_ENABLED)))
            {
                CheckForAutoChanges();
                if (mainPanel.isVisible)
                {
                    mainPanel.Show();
                }
            }
            lastLineCount = tm.m_lineCount;
        }

        void CheckForAutoChanges()
        {
            for (ushort i = 0; i < tm.m_lines.m_size; i++)
            {
                TransportLine t = tm.m_lines.m_buffer[(int)i];
                if ((t.m_flags & (TransportLine.Flags.Created)) != TransportLine.Flags.None)
                {
                    if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.AUTO_NAME_ENABLED) && ((t.m_flags & (TransportLine.Flags.CustomName)) == TransportLine.Flags.None))
                    {
                        AutoName(i);
                    }
                    if (TLMCW.getCurrentConfigBool(TLMCW.ConfigIndex.AUTO_COLOR_ENABLED) && ((t.m_flags & (TransportLine.Flags.CustomColor)) == TransportLine.Flags.None))
                    {
                        AutoColor(i);
                    }
                }
            }
        }

        public Color AutoColor(ushort i)
        {
            TransportLine t = tm.m_lines.m_buffer[(int)i];
            try
            {
                TLMCW.ConfigIndex transportType = TLMCW.getConfigIndexForLine(i);
                bool prefixBased = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_PREFIX_BASED);

                bool randomOnOverflow = TLMCW.getCurrentConfigBool(transportType | TLMCW.ConfigIndex.PALETTE_RANDOM_ON_OVERFLOW);

                string pal = TLMCW.getCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_SUBLINE);
                ushort num = t.m_lineNumber;
                if (num >= 1000 && TLMCW.getCurrentConfigInt(transportType | TLMCW.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
                {
                    pal = TLMCW.getCurrentConfigString(transportType | TLMCW.ConfigIndex.PALETTE_MAIN);
                    if (prefixBased)
                    {
                        num /= 1000;
                    }
                }
                Color c = TLMAutoColorPalettes.getColor(num, pal, randomOnOverflow);
                TLMUtils.setLineColor(i, c);
                return c;
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "ERRO!!!!! " + e.Message);
                TLMCW.setCurrentConfigBool(TLMCW.ConfigIndex.AUTO_COLOR_ENABLED, false);
                return Color.clear;
            }
        }

        public void AutoName(ushort lineIdx)
        {
            TransportLine t = tm.m_lines.m_buffer[(int)lineIdx];
            try
            {
                ModoNomenclatura sufixo, prefixo;
                Separador s;
                bool z;
                TLMLineUtils.getLineNamingParameters(lineIdx, out prefixo, out s, out sufixo, out z);

                TLMUtils.setLineName((ushort)lineIdx, "[" + TLMUtils.getString(prefixo, s, sufixo, t.m_lineNumber, z).Replace('\n', ' ') + "] " + TLMUtils.calculateAutoName(lineIdx));
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "ERRO!!!!! " + e.Message);
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, e.StackTrace);
                TLMCW.setCurrentConfigBool(TLMCW.ConfigIndex.AUTO_COLOR_ENABLED, false);
            }
        }






        //NAVEGACAO

        private void swapWindow(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lineInfoPanel.isVisible || m_mainPanel.isVisible)
            {
                fecharTelaTransportes(component, eventParam);
            }
            else {
                abrirTelaTransportes(component, eventParam);
            }

        }

        private void abrirTelaTransportes(UIComponent component, UIMouseEventParameter eventParam)
        {
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE1!");
            abrePainelButton.normalFgSprite = abrePainelButton.focusedFgSprite;
            m_lineInfoPanel.Hide();
            m_mainPanel.Show();
            tm.LinesVisible = true;
            im.SetCurrentMode(InfoManager.InfoMode.Transport, InfoManager.SubInfoMode.NormalTransport);
            //			MainMenu ();
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE2!");
        }

        private void fecharTelaTransportes(UIComponent component, UIFocusEventParameter eventParam)
        {
            fecharTelaTransportes(component, (UIMouseEventParameter)null);
        }

        private void fecharTelaTransportes(UIComponent component, UIMouseEventParameter eventParam)
        {
            abrePainelButton.normalFgSprite = abrePainelButton.disabledFgSprite;
            m_mainPanel.Hide();
            m_lineInfoPanel.Hide();
            tm.LinesVisible = false;
            InfoManager im = Singleton<InfoManager>.instance;
            im.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.NormalPower);
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "FECHA!");
        }

        private void createViews()
        {
            /////////////////////////////////////////////////////			
            m_mainPanel = new TLMMainPanel(this);
            m_lineInfoPanel = new TLMLineInfoPanel(this);
        }

        private void initNearLinesOnWorldInfoPanel()
        {
            if (!initializedWIP)
            {
                UIPanel parent = GameObject.Find("UIView").transform.GetComponentInChildren<CityServiceWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent == null)
                    return;
                parent.eventVisibilityChanged += (component, value) =>
                {
                    if (TransportLinesManagerMod.savedShowNearLinesInCityServicesWorldInfoPanel.value)
                    {
                        loadNearLines(parent, true);
                    }
                    else {
                        Transform linesPanelObj = parent.transform.Find("TLMLinesNear");
                        if (!linesPanelObj)
                        {
                            return;
                        }
                        linesPanelObj.GetComponent<UIPanel>().isVisible = false;
                    }
                };
                parent.eventPositionChanged += (component, value) =>
                {
                    if (TransportLinesManagerMod.savedShowNearLinesInCityServicesWorldInfoPanel.value)
                    {
                        loadNearLines(parent);
                    }
                    else {
                        Transform linesPanelObj = parent.transform.Find("TLMLinesNear");
                        if (!linesPanelObj)
                        {
                            return;
                        }
                        linesPanelObj.GetComponent<UIPanel>().isVisible = false;
                    }
                };

                UIPanel parent2 = GameObject.Find("UIView").transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent2 == null)
                    return;
                parent2.eventVisibilityChanged += (component, value) =>
                {
                    if (TransportLinesManagerMod.savedShowNearLinesInZonedBuildingWorldInfoPanel.value)
                    {
                        loadNearLines(parent2, true);
                    }
                    else {
                        Transform linesPanelObj = parent2.transform.Find("TLMLinesNear");
                        if (!linesPanelObj)
                        {
                            return;
                        }
                        linesPanelObj.GetComponent<UIPanel>().isVisible = false;
                    }
                };
                parent2.eventPositionChanged += (component, value) =>
                {
                    if (TransportLinesManagerMod.savedShowNearLinesInZonedBuildingWorldInfoPanel.value)
                    {
                        loadNearLines(parent2);
                    }
                    else {
                        Transform linesPanelObj = parent2.transform.Find("TLMLinesNear");
                        if (!linesPanelObj)
                        {
                            return;
                        }
                        linesPanelObj.GetComponent<UIPanel>().isVisible = false;
                    }
                };

                UIPanel parent3 = GameObject.Find("UIView").transform.GetComponentInChildren<PublicTransportWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent3 == null)
                    return;

                parent3.eventVisibilityChanged += (component, value) =>
                {
                    if (TransportLinesManagerMod.savedOverrideDefaultLineInfoPanel.value && value)
                    {

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
            while (lineId == 0)
            {
                lineId = (ushort)(typeof(PublicTransportWorldInfoPanel).GetMethod("GetLineID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance).Invoke(ptwip, new object[0]));
            }
            TLMController.instance.lineInfoPanel.openLineInfo(lineId);

        }

        private ushort lastBuildingSelected = 0;

        private void loadNearLines(UIPanel parent, bool force = false)
        {
            if (parent != null)
            {
                Transform linesPanelObj = parent.transform.Find("TLMLinesNear");
                if (!linesPanelObj)
                {
                    linesPanelObj = initPanelNearLinesOnWorldInfoPanel(parent);
                }
                var prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                ushort buildingId = ((InstanceID)(prop.GetValue(parent.gameObject.GetComponent<WorldInfoPanel>()))).Building;
                if (lastBuildingSelected == buildingId && !force)
                {
                    return;
                }
                else {
                    lastBuildingSelected = buildingId;
                }
                Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];

                List<ushort> nearLines = new List<ushort>();

                TLMLineUtils.GetNearLines(b.CalculateSidewalkPosition(), 100f, ref nearLines);
                bool showPanel = nearLines.Count > 0;
                //				DebugOutputPanel.AddMessage (PluginManager.MessageType.Warning, "nearLines.Count = " + nearLines.Count);
                if (showPanel)
                {
                    foreach (Transform t in linesPanelObj)
                    {
                        if (t.GetComponent<UILabel>() == null)
                        {
                            GameObject.Destroy(t.gameObject);
                        }
                    }
                    Dictionary<string, ushort> lines = TLMLineUtils.IndexLines(nearLines);
                    TLMLineUtils.PrintIntersections("", "", "", linesPanelObj.GetComponent<UIPanel>(), lines, scale, perLine);
                }
                linesPanelObj.GetComponent<UIPanel>().isVisible = showPanel;
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
            title.text = "Near Lines";
            title.useOutline = true;
            title.height = 18;
            return saida.transform;
        }

    }


}