using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.LineList;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager
{
    internal class TLMController : LinearMapParentInterface<TLMController>
    {

        public static UITextureAtlas taTLM
        {
            get {
                if (_taTLM == null)
                {
                    TLMResourceLoader.Ensure();
                    _taTLM = TLMResourceLoader.instance.CreateTextureAtlas("UI.Images.sprites.png", "TransportLinesManagerSprites", GameObject.FindObjectOfType<UIPanel>().atlas.material, 64, 64, new string[] {
                    "TransportLinesManagerIcon","TransportLinesManagerIconHovered","AutoNameIcon","AutoColorIcon","RemoveUnwantedIcon","ConfigIcon","24hLineIcon", "PerHourIcon","AbsoluteMode","RelativeMode"
                });
                }
                return _taTLM;
            }
        }
        public static UITextureAtlas taLineNumber
        {
            get {
                if (_taLineNumber == null)
                {
                    TLMResourceLoader.Ensure();
                    _taLineNumber = TLMResourceLoader.instance.CreateTextureAtlas("UI.Images.lineFormat.png", "TransportLinesManagerLinearLineSprites", GameObject.FindObjectOfType<UIPanel>().atlas.material, 64, 64, new string[] {
                "TourBusIcon","TourPedIcon",  "CableCarTabIcon","TaxiTabIcon",  "EvacBusIcon","DepotIcon", "LinearHalfStation","LinearStation","LinearBg","PlaneLineIcon","TramIcon","ShipLineIcon","FerryIcon","CableCarIcon", "BlimpIcon","BusIcon","SubwayIcon","TrainIcon","MonorailIcon","ShipIcon","AirplaneIcon","TaxiIcon","DayIcon",
                    "NightIcon","DisabledIcon","NoBudgetIcon","BulletTrainImage","LowBusImage","HighBusImage","VehicleLinearMap","RegionalTrainIcon"
                });
                }
                return _taLineNumber;
            }
        }

        private static UITextureAtlas _taTLM = null;
        private static UITextureAtlas _taLineNumber = null;

        public UIView uiView;
        public UIComponent mainRef;
        public TransportManager tm => Singleton<TransportManager>.instance;
        public InfoManager im => Singleton<InfoManager>.instance;
        public bool initialized = false;
        public bool initializedWIP = false;
        private TLMLineInfoPanel m_lineInfoPanel;
        private TLMDepotInfoPanel m_depotInfoPanel;
        private TLMLinearMap m_linearMapCreatingLine;
        private TLMLineCreationToolbox m_lineCreationToolbox;
        private int lastLineCount = 0;

        //private UIPanel _cachedDefaultListingLinesPanel;



        public TLMLineInfoPanel lineInfoPanel => m_lineInfoPanel;
        public TLMDepotInfoPanel depotInfoPanel => m_depotInfoPanel;
        public Transform TargetTransform => mainRef?.transform;
        public override Transform TransformLinearMap => uiView?.transform;

        private ushort m_currentSelectedId;

        public override ushort CurrentSelectedId => m_currentSelectedId;
        public void setCurrentSelectedId(ushort line) => m_currentSelectedId = line;

        public override bool CanSwitchView => false;

        public TLMLinearMap LinearMapCreatingLine
        {
            get {
                if (m_linearMapCreatingLine != null)
                {
                    return m_linearMapCreatingLine;
                }
                else
                {
                    TLMUtils.doErrorLog("LinearMapCreatingLine is NULL!!!!");
                    return null;
                }
            }
        }

        public TLMLineCreationToolbox LineCreationToolbox
        {
            get {
                if (m_lineCreationToolbox != null)
                {
                    return m_lineCreationToolbox;
                }
                else
                {
                    TLMUtils.doErrorLog("LineCreationToolbox is NULL!!!!");
                    return null;
                }
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
        public void Update()
        {
            if (!GameObject.FindGameObjectWithTag("GameController") || ((GameObject.FindGameObjectWithTag("GameController")?.GetComponent<ToolController>())?.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None)
            {
                TLMUtils.doErrorLog("GameController NOT FOUND!");
                return;
            }
            if (!initialized)
            {
                Awake();
            }

            if (m_lineInfoPanel?.isVisible ?? false)
            {
                m_lineInfoPanel?.updateBidings();
            }

            if (m_depotInfoPanel?.isVisible ?? false)
            {
                m_depotInfoPanel?.updateBidings();
            }

            lastLineCount = tm.m_lineCount;

            m_lineInfoPanel?.assetSelectorWindow?.RotateCamera();
        }

        public void Awake()
        {
            if (!initialized && gameObject != null)
            {
                TLMSingleton.instance.loadTLMLocale(false);

                uiView = GameObject.FindObjectOfType<UIView>();
                if (!uiView)
                    return;
                mainRef = uiView.FindUIComponent<UIPanel>("InfoPanel").Find<UITabContainer>("InfoViewsContainer").Find<UIPanel>("InfoViewsPanel");
                if (!mainRef)
                    return;
                createViews();
                mainRef.clipChildren = false;

                var typeTarg = typeof(Redirector<>);
                var instances = from t in Assembly.GetAssembly(typeof(TLMController)).GetTypes()
                                let y = t.BaseType
                                where t.IsClass && !t.IsAbstract && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                                select t;

                foreach (Type t in instances)
                {
                    TLMUtils.doLog($"Adding hooks: {t}");
                    gameObject.AddComponent(t);
                }

                initialized = true;
            }
        }


        public Color AutoColor(ushort i, bool ignoreRandomIfSet = true, bool ignoreAnyIfSet = false)
        {
            TransportLine t = tm.m_lines.m_buffer[(int)i];
            try
            {
                var tsd = TransportSystemDefinition.getDefinitionForLine(i);
                if (tsd == default(TransportSystemDefinition) || (((t.m_flags & TransportLine.Flags.CustomColor) > 0) && ignoreAnyIfSet))
                {
                    return Color.clear;
                }
                TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
                Color c = TLMUtils.CalculateAutoColor(t.m_lineNumber, transportType, ref tsd, ((t.m_flags & TransportLine.Flags.CustomColor) > 0) && ignoreRandomIfSet, true);
                if (c.a == 1)
                {
                    TLMLineUtils.setLineColor(i, c);
                }
                else
                {
                    c = Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_color;
                }
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
            TLMLineUtils.setLineName(m_LineID, TLMLineUtils.calculateAutoName(m_LineID));
        }


        //NAVEGACAO

        private void createViews()
        {
            TLMUtils.createElement(out m_lineInfoPanel, transform);
            TLMUtils.createElement(out m_depotInfoPanel, transform);
            TLMUtils.createElement(out m_linearMapCreatingLine, transform);
            TLMUtils.createElement(out m_lineCreationToolbox, transform);
            m_linearMapCreatingLine.parent = this;
            m_linearMapCreatingLine.setVisible(false);
        }

        private void initNearLinesOnWorldInfoPanel()
        {
            if (!initializedWIP)
            {
                BuildingWorldInfoPanel[] panelList = GameObject.Find("UIView").GetComponentsInChildren<BuildingWorldInfoPanel>();
                TLMUtils.doLog("WIP LIST: [{0}]", string.Join(", ", panelList.Select(x => x.name).ToArray()));

                foreach (BuildingWorldInfoPanel wip in panelList)
                {
                    TLMUtils.doLog("LOADING WIP HOOK FOR: {0}", wip.name);
                    UIComponent parent2 = wip.GetComponent<UIComponent>();

                    if (parent2 == null)
                        continue;

                    parent2.eventVisibilityChanged += (component, value) =>
                    {
                        updateNearLines(TLMSingleton.savedShowNearLinesInZonedBuildingWorldInfoPanel.value ? parent2 : null, true);
                        updateDepotEditShortcutButton(parent2);
                    };
                    parent2.eventPositionChanged += (component, value) =>
                    {
                        updateNearLines(TLMSingleton.savedShowNearLinesInZonedBuildingWorldInfoPanel.value ? parent2 : null, true);
                        updateDepotEditShortcutButton(parent2);
                    };


                    UIPanel parent3 = GameObject.Find("UIView").transform.GetComponentInChildren<PublicTransportWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                    if (parent3 == null)
                        return;

                    parent3.eventVisibilityChanged += (component, value) =>
                    {
                        if (TLMSingleton.overrideWorldInfoPanelLine && value)
                        {
                            PublicTransportWorldInfoPanel ptwip = parent3.gameObject.GetComponent<PublicTransportWorldInfoPanel>();
                            ptwip.StartCoroutine(OpenLineInfo(ptwip));
                            ptwip.Hide();
                        }
                    };

                }
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

        private void updateNearLines(UIComponent parent, bool force = false)
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

        private Transform initPanelNearLinesOnWorldInfoPanel(UIComponent parent)
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

        private UIButton initDepotShortcutOnWorldInfoPanel(UIComponent parent)
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

        private void updateDepotEditShortcutButton(UIComponent parent)
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
                    if (ai.m_transportInfo != null && ai.m_maxVehicleCount > 0 && TransportSystemDefinition.from(ai.m_transportInfo).isPrefixable())
                    {
                        lines.Add(string.Format("{0}: {1}", TLMConfigWarehouse.getNameForTransportType(TransportSystemDefinition.from(ai.m_transportInfo).toConfigIndex()), TLMLineUtils.getPrefixesServedString(buildingId, false)));
                        count++;
                    }
                    if (ai.m_secondaryTransportInfo != null && ai.m_maxVehicleCount2 > 0 && TransportSystemDefinition.from(ai.m_secondaryTransportInfo).isPrefixable())
                    {
                        lines.Add(string.Format("{0}: {1}", TLMConfigWarehouse.getNameForTransportType(TransportSystemDefinition.from(ai.m_secondaryTransportInfo).toConfigIndex()), TLMLineUtils.getPrefixesServedString(buildingId, true)));
                        count++;
                    }
                    depotShortcut.isVisible = count > 0;
                    if (depotShortcut.isVisible)
                    {
                        UILabel label = depotShortcut.GetComponentInChildren<UILabel>();
                        label.text = string.Join("\n", lines.ToArray());
                    }
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

        //------------------------------------

        public void Start()
        {
            KlyteModsPanel.instance.AddTab(ModTab.TransportLinesManager, typeof(TLMPublicTransportManagementPanel), taTLM, "TransportLinesManagerIconHovered", "Transport Lines Manager (v" + TLMSingleton.version + ")");
            initNearLinesOnWorldInfoPanel();
        }

        public void OpenTLMPanel()
        {
            KlyteModsPanel.instance.OpenAt(ModTab.TransportLinesManager);
        }
        public void CloseTLMPanel()
        {
            KCController.instance.CloseKCPanel();
        }

    }


}