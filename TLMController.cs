using ColossalFramework;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.LineDetailWindow;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.WorldInfoPanelExt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager
{
    public class TLMController : MonoBehaviour, ILinearMapParentInterface
    {

        internal static TLMController instance => TransportLinesManagerMod.instance.controller;

        public bool initialized = false;
        public bool initializedWIP = false;
        private TLMLineDetailWindow m_lineInfoPanel;
        private TLMLinearMap m_linearMapCreatingLine;
        private TLMLineCreationToolbox m_lineCreationToolbox;
        private int lastLineCount = 0;

        //private UIPanel _cachedDefaultListingLinesPanel;

        public static readonly string FOLDER_NAME = "TransportLinesManager";
        public static readonly string FOLDER_PATH = TLMUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string PALETTE_SUBFOLDER_NAME = "ColorPalettes";
        public const string EXPORTED_MAPS_SUBFOLDER_NAME = "ExportedMaps";

        public static string palettesFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + PALETTE_SUBFOLDER_NAME;
        public static string configsFolder { get; } = TLMConfigWarehouse.CONFIG_PATH;
        public static string exportedMapsFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + EXPORTED_MAPS_SUBFOLDER_NAME;

        internal TLMLineDetailWindow lineInfoPanel => m_lineInfoPanel;
        public Transform TransformLinearMap => FindObjectOfType<UIView>()?.transform;

        private ushort m_currentSelectedId;

        public ushort CurrentSelectedId => m_currentSelectedId;
        public void setCurrentSelectedId(ushort line) => m_currentSelectedId = line;

        public bool CanSwitchView => false;

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

        internal TLMLineCreationToolbox LineCreationToolbox
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

        public bool ForceShowStopsDistances => true;

        public TransportInfo CurrentTransportInfo => Singleton<TransportTool>.instance.m_prefab;
        public void Update()
        {
            if (!GameObject.FindGameObjectWithTag("GameController") || ((GameObject.FindGameObjectWithTag("GameController")?.GetComponent<ToolController>())?.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None)
            {
                TLMUtils.doErrorLog("GameController NOT FOUND!");
                return;
            }
            if (m_lineInfoPanel?.isVisible ?? false)
            {
                m_lineInfoPanel?.updateBidings();

                lastLineCount = TransportManager.instance.m_lineCount;

                m_lineInfoPanel?.assetSelectorWindow?.RotateCamera();
            }
        }

        public Color AutoColor(ushort i, bool ignoreRandomIfSet = true, bool ignoreAnyIfSet = false)
        {
            TransportLine t = TransportManager.instance.m_lines.m_buffer[i];
            try
            {
                var tsd = TransportSystemDefinition.getDefinitionForLine(i);
                if (tsd == default || (((t.m_flags & TransportLine.Flags.CustomColor) > 0) && ignoreAnyIfSet))
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

        public void AutoName(ushort m_LineID) => TLMLineUtils.setLineName(m_LineID, TLMLineUtils.calculateAutoName(m_LineID));


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
                    {
                        continue;
                    }

                    parent2.eventVisibilityChanged += EventWIPChanged;
                    parent2.eventPositionChanged += EventWIPChanged;


                    UIPanel parent3 = GameObject.Find("UIView").transform.GetComponentInChildren<PublicTransportWorldInfoPanel>().gameObject.GetComponent<UIPanel>();

                    if (parent3 == null)
                    {
                        return;
                    }

                    parent3.eventVisibilityChanged += (component, value) =>
                    {
                        if (TransportLinesManagerMod.overrideWorldInfoPanelLine && value)
                        {
                            PublicTransportWorldInfoPanel ptwip = parent3.gameObject.GetComponent<PublicTransportWorldInfoPanel>();
                            ptwip.StartCoroutine(OpenLineInfo(ptwip));
                            ptwip.Hide();
                        }
                    };

                }
                initDepotAIPrefixListContainer(GameObject.Find("UIView").GetComponentInChildren<CityServiceWorldInfoPanel>().GetComponent<UIComponent>());
                initializedWIP = true;
            }
        }

        private void EventWIPChanged<T>(UIComponent component, T value)
        {
            updateNearLines(TransportLinesManagerMod.showNearLinesGrow ? component : null, true);
            updateDepotPrefixLists(component);
        }

        private IEnumerator OpenLineInfo(PublicTransportWorldInfoPanel ptwip)
        {
            yield return 0;
            ushort lineId = 0;
            while (lineId == 0)
            {
                lineId = (ushort) (typeof(PublicTransportWorldInfoPanel).GetMethod("GetLineID", System.Reflection.BindingFlags.NonPublic
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
                System.Reflection.FieldInfo prop = typeof(global::WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                WorldInfoPanel wip = parent.gameObject.GetComponent<global::WorldInfoPanel>();
                ushort buildingId = ((InstanceID) (prop.GetValue(wip))).Building;
                if (lastBuildingSelected == buildingId && !force)
                {
                    return;
                }
                else
                {
                    lastBuildingSelected = buildingId;
                }
                Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];

                var nearLines = new List<ushort>();

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
                linesPanelObj.GetComponent<UIPanel>().relativePosition = new Vector3(0, parent.height);
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
            title.localeID = "K45_TLM_NEAR_LINES";
            title.useOutline = true;
            title.height = 18;
            return saida.transform;
        }

        public void OnRenameStationAction(string autoName)
        {

        }

        //------------------------------------

        public void Start()
        {
            TLMUtils.createElement(out m_lineInfoPanel, FindObjectOfType<UIView>().transform);
            TLMUtils.createElement(out m_linearMapCreatingLine, transform);
            TLMUtils.createElement(out m_lineCreationToolbox, transform);
            m_linearMapCreatingLine.parent = this;
            m_linearMapCreatingLine.setVisible(false);
            initNearLinesOnWorldInfoPanel();
        }

        public void OpenTLMPanel() => TransportLinesManagerMod.instance.OpenPanelAtModTab();
        public void CloseTLMPanel() => TransportLinesManagerMod.instance.ClosePanel();

        private TLMDepotWorldInfoPanelPrefixListsParent initDepotAIPrefixListContainer(UIComponent parent)
        {

            TLMDepotWorldInfoPanelPrefixListsParent saida = parent.AddUIComponent<TLMDepotWorldInfoPanelPrefixListsParent>();

            return saida;
        }

        private ushort m_lastWipId;

        private void updateDepotPrefixLists(UIComponent parent)
        {
            if (parent != null)
            {

                InstanceID buildingId = WorldInfoPanel.GetCurrentInstanceID();
                if (buildingId.Building > 0 && m_lastWipId != buildingId.Building)
                {


                    TLMDepotWorldInfoPanelPrefixListsParent depotLists = parent.GetComponentInChildren<TLMDepotWorldInfoPanelPrefixListsParent>();
                    if (depotLists == null)
                    {
                        depotLists = initDepotAIPrefixListContainer(parent);
                    }

                    if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId.Building].Info?.GetAI() is DepotAI)
                    {
                        depotLists.isVisible = true;
                        depotLists.WipOpen(ref buildingId);
                    }
                    else
                    {
                        depotLists.isVisible = false;
                    }
                    m_lastWipId = buildingId.Building;
                }

            }
        }
    }


}