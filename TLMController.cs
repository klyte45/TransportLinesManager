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
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Interfaces;
using System.Reflection;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;

namespace Klyte.TransportLinesManager
{
    public class TLMController : LinearMapParentInterface<TLMController>
    {

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
        private TLMDepotInfoPanel m_depotInfoPanel;
        private TLMLinearMap m_linearMapCreatingLine;
        private TLMLineCreationToolbox m_lineCreationToolbox;
        private int lastLineCount = 0;

        private UIPanel _cachedDefaultListingLinesPanel;

        public UIPanel defaultListingLinesPanel
        {
            get {
                if (_cachedDefaultListingLinesPanel == null)
                {
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

        public TLMDepotInfoPanel depotInfoPanel
        {
            get {
                return m_depotInfoPanel;
            }
        }

        public Transform TargetTransform
        {
            get {
                return mainRef.transform;
            }
        }


        public override Transform TransformLinearMap
        {
            get {
                return uiView.transform;
            }
        }
        private ushort m_currentSelectedId;

        public override ushort CurrentSelectedId
        {
            get {
                return m_currentSelectedId;
            }
        }
        public void setCurrentSelectedId(ushort line)
        {
            m_currentSelectedId = line;
        }

        public override bool CanSwitchView
        {
            get {
                return false;
            }
        }

        public TLMLinearMap LinearMapCreatingLine
        {
            get {
                return m_linearMapCreatingLine;
            }
        }

        public TLMLineCreationToolbox LineCreationToolbox
        {
            get {
                return m_lineCreationToolbox;
            }
        }

        public override bool ForceShowStopsDistances
        {
            get {
                return true;
            }
        }


        public override TransportInfo CurrentTransportInfo
        {
            get {
                return Singleton<TransportTool>.instance.m_prefab;
            }
        }


        private TLMController()
        {
        }

        public void destroy()
        {
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
            if (m_linearMapCreatingLine != null && m_linearMapCreatingLine.gameObject != null)
            {
                UnityEngine.Object.Destroy(m_linearMapCreatingLine.gameObject);
            }

            if (m_lineCreationToolbox != null && m_lineCreationToolbox.gameObject != null)
            {
                UnityEngine.Object.Destroy(m_lineCreationToolbox.gameObject);
            }
            if (m_depotInfoPanel != null && m_depotInfoPanel.gameObject != null)
            {
                UnityEngine.Object.Destroy(m_depotInfoPanel.gameObject);
            }

            initialized = false;
            initializedWIP = false;
        }

        public void update()
        {
            if (!GameObject.FindGameObjectWithTag("GameController") || ((GameObject.FindGameObjectWithTag("GameController").GetComponent<ToolController>()).m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None)
            {
                TLMUtils.doErrorLog("GameController NOT FOUND!");
                return;
            }
            if (!initialized)
            {
                TransportLinesManagerMod.instance.loadTLMLocale(false);

                uiView = GameObject.FindObjectOfType<UIView>();
                if (!uiView)
                    return;
                mainRef = uiView.FindUIComponent<UIPanel>("InfoPanel").Find<UITabContainer>("InfoViewsContainer").Find<UIPanel>("InfoViewsPanel");
                if (!mainRef)
                    return;
                mainRef.eventVisibilityChanged += delegate (UIComponent component, bool b)
                {
                    if (b)
                    {
                        TransportLinesManagerMod.instance.showVersionInfoPopup();
                    }
                };


                createViews();
                mainRef.clipChildren = false;
                initialized = true;
            }

            initNearLinesOnWorldInfoPanel();

            if (m_lineInfoPanel.isVisible)
            {
                m_lineInfoPanel.updateBidings();
            }

            if (m_depotInfoPanel.isVisible)
            {
                m_depotInfoPanel.updateBidings();
            }

            lastLineCount = tm.m_lineCount;
            TLMPublicTransportDetailPanelHooks.instance.update();

            if (m_lineInfoPanel?.assetSelectorWindow!=null)
            {
                m_lineInfoPanel?.assetSelectorWindow?.RotateCamera();
            }

            return;
        }
        const int maxTryLoads = 100;

        public Color AutoColor(ushort i)
        {
            TransportLine t = tm.m_lines.m_buffer[(int)i];
            try
            {
                var tsd = TLMCW.getDefinitionForLine(i);
                if (tsd == default(TransportSystemDefinition))
                {
                    return Color.clear;
                }
                TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
                Color c = TLMUtils.CalculateAutoColor(t.m_lineNumber, transportType);
                TLMUtils.setLineColor(i, c);
                //TLMUtils.doLog("Colocada a cor {0} na linha {1} ({3} {2})", c, i, t.m_lineNumber, t.Info.m_transportType);
                return c;
            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog("ERRO!!!!! " + e.Message);
                TLMCW.setCurrentConfigBool(TLMCW.ConfigIndex.AUTO_COLOR_ENABLED, false);
                return Color.clear;
            }
        }

        public void AutoName(ushort m_LineID)
        {
            TLMUtils.setLineName(m_LineID, TLMUtils.calculateAutoName(m_LineID, true));
        }


        //NAVEGACAO

        private void swapWindow(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_lineInfoPanel.isVisible || defaultListingLinesPanel.isVisible || m_depotInfoPanel.isVisible)
            {
                fecharTelaTransportes(component, eventParam);
            }
            else
            {
                abrirTelaTransportes(component, eventParam);
            }

        }

        private void abrirTelaTransportes(UIComponent component, UIMouseEventParameter eventParam)
        {
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "ABRE1!");
            abrePainelButton.normalFgSprite = abrePainelButton.focusedFgSprite;
            m_lineInfoPanel.Hide();
            m_depotInfoPanel.Hide();
            defaultListingLinesPanel.Show();
            tm.LinesVisible = 0x7FFFFFFF;
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
            defaultListingLinesPanel.Hide();
            m_lineInfoPanel.Hide();
            m_depotInfoPanel.Hide();
            tm.LinesVisible = 0;
            InfoManager im = Singleton<InfoManager>.instance;
            //			DebugOutputPanel.AddMessage (ColossalFramework.Plugins.PluginManager.MessageType.Warning, "FECHA!");
        }

        private void createViews()
        {
            m_lineInfoPanel = new TLMLineInfoPanel(this);
            m_depotInfoPanel = new TLMDepotInfoPanel(this);
            m_linearMapCreatingLine = new TLMLinearMap(this)
            {
                isVisible = false
            };
            m_lineCreationToolbox = new TLMLineCreationToolbox(this);
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
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInCityServicesWorldInfoPanel.value ? parent : null, true);
                    updateDepotEditShortcutButton(parent);
                };
                parent.eventPositionChanged += (component, value) =>
                {
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInCityServicesWorldInfoPanel.value ? parent : null, true);
                    updateDepotEditShortcutButton(parent);
                };

                UIPanel parent2 = GameObject.Find("UIView").transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent2 == null)
                    return;

                parent2.eventVisibilityChanged += (component, value) =>
                {
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInZonedBuildingWorldInfoPanel.value ? parent2 : null, true);
                    updateDepotEditShortcutButton(parent2);
                };
                parent2.eventPositionChanged += (component, value) =>
                {
                    updateNearLines(TransportLinesManagerMod.savedShowNearLinesInZonedBuildingWorldInfoPanel.value ? parent2 : null, true);
                    updateDepotEditShortcutButton(parent2);
                };
                UIPanel parent3 = GameObject.Find("UIView").transform.GetComponentInChildren<PublicTransportWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                if (parent3 == null)
                    return;

                parent3.eventVisibilityChanged += (component, value) =>
                {
                    if (TransportLinesManagerMod.overrideWorldInfoPanelLine && value)
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

        private void updateNearLines(UIPanel parent, bool force = false)
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
                else
                {
                    lastBuildingSelected = buildingId;
                }
                Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];

                List<ushort> nearLines = new List<ushort>();

                TLMLineUtils.GetNearLines(b.CalculateSidewalkPosition(), 120f, ref nearLines);
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
                    Dictionary<string, ushort> lines = TLMLineUtils.SortLines(nearLines);
                    TLMLineUtils.PrintIntersections("", "", "", "", "", linesPanelObj.GetComponent<UIPanel>(), lines, scale, perLine);
                }
                linesPanelObj.GetComponent<UIPanel>().isVisible = showPanel;
            }
            else
            {
                var go = GameObject.Find("TLMLinesNear");
                if (!go)
                {
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

        private UIButton initDepotShortcutOnWorldInfoPanel(UIPanel parent)
        {
            UIButton saida = parent.AddUIComponent<UIButton>();
            saida.relativePosition = new Vector3(10, parent.height - 50);
            saida.atlas = taTLM;
            saida.width = 30;
            saida.height = 30;
            saida.name = "TLMDepotShortcut";
            saida.tooltipLocaleID = "TLM_GOTO_DEPOT_PREFIX_EDIT";
            TLMUtils.initButton(saida, false, "TransportLinesManagerIcon");
            saida.eventClick += (x, y) =>
            {
                var prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                       | System.Reflection.BindingFlags.Instance);
                ushort buildingId = ((InstanceID)(prop.GetValue(parent.gameObject.GetComponent<WorldInfoPanel>()))).Building;
                depotInfoPanel.openDepotInfo(buildingId, false);
            };

            UILabel prefixes = saida.AddUIComponent<UILabel>();
            prefixes.autoSize = false;
            prefixes.width = 200;
            prefixes.wordWrap = true;
            prefixes.textAlignment = UIHorizontalAlignment.Left;
            prefixes.prefix = Locale.Get("TLM_PREFIXES_SERVED") + ":\n";
            prefixes.useOutline = true;
            prefixes.height = 60;
            prefixes.textScale = 0.6f;
            prefixes.relativePosition = new Vector3(35, 1);
            prefixes.name = "Prefixes";
            return saida;
        }

        private void updateDepotEditShortcutButton(UIPanel parent)
        {
            if (parent != null)
            {
                UIButton depotShortcut = parent.Find<UIButton>("TLMDepotShortcut");
                if (!depotShortcut)
                {
                    depotShortcut = initDepotShortcutOnWorldInfoPanel(parent);
                }
                var prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                ushort buildingId = ((InstanceID)(prop.GetValue(parent.gameObject.GetComponent<WorldInfoPanel>()))).Building;
                if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].Info.GetAI() is DepotAI ai)
                {
                    byte count = 0;
                    List<string> lines = new List<string>();
                    if (ai.m_transportInfo != null && ai.m_maxVehicleCount > 0)
                    {
                        lines.Add(string.Format("{0}: {1}", TLMConfigWarehouse.getNameForTransportType(TransportSystemDefinition.from(ai.m_transportInfo).toConfigIndex()), TLMUtils.getPrefixesServedAbstract(buildingId, false)));
                        count++;
                    }
                    if (ai.m_secondaryTransportInfo != null && ai.m_maxVehicleCount2 > 0)
                    {
                        lines.Add(string.Format("{0}: {1}", TLMConfigWarehouse.getNameForTransportType(TransportSystemDefinition.from(ai.m_secondaryTransportInfo).toConfigIndex()), TLMUtils.getPrefixesServedAbstract(buildingId, true)));
                        count++;
                    }
                    UILabel label = depotShortcut.GetComponentInChildren<UILabel>();
                    label.text = string.Join("\n", lines.ToArray());
                    depotShortcut.isVisible = count > 0;
                }
                else
                {
                    depotShortcut.isVisible = false;
                }

            }
        }

        public override void OnRenameStationAction(string autoName)
        {

        }
    }


}