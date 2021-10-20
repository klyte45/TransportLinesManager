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

        internal TLMLineCreationToolbox LineCreationToolbox => PublicTransportInfoViewPanelOverrides.Toolbox;

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
            ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[i];
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
        public void OnRenameStationAction(string autoName)
        {

        }

        //------------------------------------

        protected override void StartActions()
        {
            TLMTransportTypeDataContainer.Instance.RefreshCapacities();

            using (var x = new EnumerableActionThread(new Func<ThreadBase, IEnumerator>(VehicleUtils.UpdateCapacityUnits)))
            {
                TLMNearLinesController.InitNearLinesOnWorldInfoPanel();
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
                if (id > 0 && UVMPublicTransportWorldInfoPanel.GetLineID() == id)
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