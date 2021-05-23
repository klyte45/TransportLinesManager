using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.ModShared;
using Klyte.TransportLinesManager.Overrides;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMController : BaseController<TransportLinesManagerMod, TLMController>, ILinearMapParentInterface
    {
        public static DataContainer Container => DataContainer.instance;
        public static TLMTransportLineStatusesManager Statuses => TLMTransportLineStatusesManager.instance;

        internal static TLMController Instance => TransportLinesManagerMod.Controller;

        public bool initializedWIP = false;
        private TLMLinearMap m_linearMapCreatingLine;

        //private UIPanel _cachedDefaultListingLinesPanel;

        public static readonly string FOLDER_NAME = "TransportLinesManager";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string PALETTE_SUBFOLDER_NAME = "ColorPalettes";
        public const string EXPORTED_MAPS_SUBFOLDER_NAME = "ExportedMaps";
        public const ulong REALTIME_MOD_ID = 1420955187;
        public const ulong IPT2_MOD_ID = 928128676;
        private bool? m_isRealTimeEnabled = null;
        protected static string GlobalBaseConfigFileName { get; } = "TLM_GlobalData.xml";
        public static string GlobalBaseConfigPath { get; } = Path.Combine(FOLDER_PATH, GlobalBaseConfigFileName);

        public static bool IsRealTimeEnabled
        {
            get
            {
                if (Instance?.m_isRealTimeEnabled == null)
                {
                    VerifyIfIsRealTimeEnabled();
                }
                return Instance?.m_isRealTimeEnabled == true;
            }
        }
        public static void VerifyIfIsRealTimeEnabled()
        {
            if (Instance != null)
            {
                Instance.m_isRealTimeEnabled = VerifyModEnabled(REALTIME_MOD_ID);
            }
        }

        public static bool IsIPT2Enabled() => VerifyModEnabled(IPT2_MOD_ID);

        private static bool VerifyModEnabled(ulong modId)
        {
            PluginManager.PluginInfo pluginInfo = Singleton<PluginManager>.instance.GetPluginsInfo().FirstOrDefault((PluginManager.PluginInfo pi) => pi.publishedFileID.AsUInt64 == modId);
            return !(pluginInfo == null || !pluginInfo.isEnabled);
        }

        public static string PalettesFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + PALETTE_SUBFOLDER_NAME;
        public static string ExportedMapsFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + EXPORTED_MAPS_SUBFOLDER_NAME;

        public Transform TransformLinearMap => UIView.GetAView()?.transform;

        public ushort CurrentSelectedId { get; private set; }
        public void SetCurrentSelectedId(ushort line) => CurrentSelectedId = line;

        public bool CanSwitchView => false;

        private static readonly SavedBool m_showLinearMapWhileCreatingLine = new SavedBool("K45_TLM_showLinearMapWhileCreatingLine", Settings.gameSettingsFile, true);

        public TLMLinearMap LinearMapCreatingLine
        {
            get
            {
                if (m_linearMapCreatingLine != null)
                {
                    return m_linearMapCreatingLine;
                }
                else
                {
                    LogUtils.DoErrorLog("LinearMapCreatingLine is NULL!!!!");
                    return null;
                }
            }
        }
        public static bool LinearMapWhileCreatingLineVisibility
        {
            get => m_showLinearMapWhileCreatingLine;
            set
            {
                m_showLinearMapWhileCreatingLine.value = value;
                Instance.LinearMapCreatingLine.SetVisible(value);
            }
        }

        internal TLMLineCreationToolbox LineCreationToolbox => PublicTransportInfoViewPanelOverrides.Toolbox;

        public static void RedrawMap(ushort lineCurrent)
        {
            if (Singleton<TransportManager>.instance.m_lines.m_buffer[lineCurrent].m_stops == 0)
            {
                lineCurrent = 0;
            }

            Instance.SetCurrentSelectedId(lineCurrent);
            Instance.LinearMapCreatingLine.RedrawLine();

        }

        public bool ForceShowStopsDistances => true;

        public TransportInfo CurrentTransportInfo => Singleton<TransportTool>.instance.m_prefab;

        public TLMFacade SharedInstance { get; internal set; }
        internal IBridgeADR ConnectorADR { get; private set; }
        internal IBridgeWTS ConnectorWTS { get; private set; }

        public void Update()
        {
            if (!GameObject.FindGameObjectWithTag("GameController") || ((GameObject.FindGameObjectWithTag("GameController")?.GetComponent<ToolController>())?.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None)
            {
                LogUtils.DoErrorLog("GameController NOT FOUND!");
                return;
            }
        }

        public static Color AutoColor(ushort i, bool ignoreRandomIfSet = true, bool ignoreAnyIfSet = false)
        {
            TransportLine t = TransportManager.instance.m_lines.m_buffer[i];
            try
            {
                var tsd = TransportSystemDefinition.GetDefinitionForLine(i);
                if (tsd == default || (((t.m_flags & TransportLine.Flags.CustomColor) > 0) && ignoreAnyIfSet))
                {
                    return Color.clear;
                }
                Color c = TLMPrefixesUtils.CalculateAutoColor(t.m_lineNumber, tsd, ((t.m_flags & TransportLine.Flags.CustomColor) > 0) && ignoreRandomIfSet, true);
                if (c.a == 1)
                {
                    Instance.StartCoroutine(TLMLineUtils.RunColorChange(Instance, i, c));
                }
                else
                {
                    c = Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_color;
                }
                LogUtils.DoLog("Colocada a cor #{0} na linha {1} ({3} {2})", c.ToRGB(), i, t.m_lineNumber, t.Info.m_transportType);
                return c;
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("ERRO!!!!! " + e.Message);
                TLMBaseConfigXML.Instance.UseAutoColor = false;
                return Color.clear;
            }
        }

        public static void AutoName(ushort m_LineID) => TLMLineUtils.SetLineName(m_LineID, TLMLineUtils.CalculateAutoName(m_LineID, out _));


        private void InitNearLinesOnWorldInfoPanel()
        {
            if (!initializedWIP)
            {
                BuildingWorldInfoPanel[] panelList = UIView.GetAView().GetComponentsInChildren<BuildingWorldInfoPanel>();
                LogUtils.DoLog("WIP LIST: [{0}]", string.Join(", ", panelList.Select(x => x.name).ToArray()));

                foreach (BuildingWorldInfoPanel wip in panelList)
                {
                    LogUtils.DoLog("LOADING WIP HOOK FOR: {0}", wip.name);
                    UIComponent parent2 = wip.GetComponent<UIComponent>();

                    if (parent2 == null)
                    {
                        continue;
                    }
                    var isGrow = wip is ZonedBuildingWorldInfoPanel;
                    parent2.eventVisibilityChanged += (x, y) => EventWIPChanged(x, isGrow);
                    parent2.eventPositionChanged += (x, y) => EventWIPChanged(x, isGrow);
                    parent2.eventSizeChanged += (x, y) => EventWIPChanged(x, isGrow);

                }
                initializedWIP = true;
            }
        }

        private void EventWIPChanged(UIComponent component, bool isGrow) => UpdateNearLines((isGrow ? TransportLinesManagerMod.ShowNearLinesGrow : TransportLinesManagerMod.ShowNearLinesPlop) ? component : null, true);


        private ushort lastBuildingSelected = 0;

        private void UpdateNearLines(UIComponent parent, bool force = false)
        {
            if (parent != null)
            {
                Transform linesPanelObj = parent.transform.Find("TLMLinesNear");
                if (!linesPanelObj)
                {
                    linesPanelObj = InitPanelNearLinesOnWorldInfoPanel(parent);
                }
                System.Reflection.FieldInfo prop = typeof(WorldInfoPanel).GetField("m_InstanceID", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance);
                WorldInfoPanel wip = parent.gameObject.GetComponent<WorldInfoPanel>();
                ushort buildingId = ((InstanceID)(prop.GetValue(wip))).Building;
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
                Vector3 sidewalk = b.CalculateSidewalkPosition();
                TLMLineUtils.GetNearLines(sidewalk, 120f, ref nearLines);
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
                    TLMLineUtils.PrintIntersections("", "", "", "", "", linesPanelObj.GetComponent<UIPanel>(), lines, sidewalk, scale, perLine);
                }
                linesPanelObj.GetComponent<UIPanel>().isVisible = showPanel;
                linesPanelObj.GetComponent<UIPanel>().relativePosition = new Vector3(0, parent.height + 10);
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

        private Transform InitPanelNearLinesOnWorldInfoPanel(UIComponent parent)
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

        protected override void StartActions()
        {
            TLMTransportTypeDataContainer.Instance.RefreshCapacities();

            using (var x = new EnumerableActionThread(new Func<ThreadBase, IEnumerator>(VehicleUtils.UpdateCapacityUnits)))
            {
                KlyteMonoUtils.CreateElement(out m_linearMapCreatingLine, transform);
                m_linearMapCreatingLine.Parent = this;
                m_linearMapCreatingLine.SetVisible(false);
                InitNearLinesOnWorldInfoPanel();
            }
        }

        public void OpenTLMPanel() => TransportLinesManagerMod.Instance.OpenPanelAtModTab();
        public void CloseTLMPanel() => TransportLinesManagerMod.Instance.ClosePanel();


        public IEnumerator RenameCoroutine(ushort id, string newName)
        {
            if (Singleton<SimulationManager>.exists)
            {
                AsyncTask<bool> task = Singleton<SimulationManager>.instance.AddAction(Singleton<TransportManager>.instance.SetLineName(id, newName));
                yield return task.WaitTaskCompleted(this);
                if (UVMPublicTransportWorldInfoPanel.GetLineID() == id)
                {
                    UVMPublicTransportWorldInfoPanel.m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(id);
                }
            }
            yield break;
        }

        internal void Awake()
        {
            SharedInstance = gameObject.AddComponent<TLMFacade>();
            ConnectorADR = PluginUtils.GetImplementationTypeForMod<BridgeADR, BridgeADRFallback, IBridgeADR>(gameObject, "KlyteAddresses", "2.99.99.0");
            ConnectorWTS = PluginUtils.GetImplementationTypeForMod<BridgeWTS, BridgeWTSFallback, IBridgeWTS>(gameObject, "KlyteWriteTheSigns", "0.3.0.0");
        }

    }


}