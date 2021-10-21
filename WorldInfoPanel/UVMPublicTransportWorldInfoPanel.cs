using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Harmony;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    public class UVMPublicTransportWorldInfoPanel : Redirector, IRedirectable
    {
        #region Awake 
        public void Awake()
        {
            m_obj = new UVMPublicTransportWorldInfoPanelObject();

            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("Start", RedirectorUtils.allFlags), null, null, typeof(UVMPublicTransportWorldInfoPanel).GetMethod("TranspileStart", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("UpdateBindings", RedirectorUtils.allFlags), null, null, typeof(UVMPublicTransportWorldInfoPanel).GetMethod("TranspileUpdateBindings", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnEnable", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnEnableOverride", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnDisable", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnDisableOverride", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnLinesOverviewClicked", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnLinesOverviewClicked", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("ResetScrollPosition", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("ResetScrollPosition", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnSetTarget", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnSetTarget", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnGotFocus", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnGotFocus", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnLineColorChanged", RedirectorUtils.allFlags), typeof(Redirector).GetMethod("PreventDefault", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnLineNameChanged", RedirectorUtils.allFlags), typeof(Redirector).GetMethod("PreventDefault", RedirectorUtils.allFlags));
            AddRedirect(typeof(WorldInfoPanel).GetMethod("IsValidTarget", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("PreIsValidTarget", RedirectorUtils.allFlags));
            TransportManager.instance.eventLineColorChanged += (x) =>
            {
                if (GetLineID(out ushort lineId, out ushort buildingId) && x == lineId && buildingId == 0)
                {
                    MarkDirty(null);
                }
            };
            TransportManager.instance.eventLineNameChanged += (x) =>
            {
                if (GetLineID(out ushort lineId, out ushort buildingId) && x == lineId && buildingId == 0)
                {
                    m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(x);
                }
            };
        }


        public static bool PreIsValidTarget(ref WorldInfoPanel __instance, ref bool __result)
        {
            if (__instance is PublicTransportWorldInfoPanel && GetLineID(out ushort lineId, out ushort buildingId) && (lineId == 0 || buildingId > 0))
            {
                __result = true;
                return false;
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> TranspileStart(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var inst = new List<CodeInstruction>(instructions);
            Label label = il.DefineLabel();
            inst[2].labels.Add(label);
            inst.InsertRange(2, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call,typeof(UVMPublicTransportWorldInfoPanel).GetMethod("CheckEnabled", RedirectorUtils.allFlags) ),
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OverrideStart", RedirectorUtils.allFlags) ),
                new CodeInstruction(OpCodes.Ret ),
            });
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static IEnumerable<CodeInstruction> TranspileUpdateBindings(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            inst.InsertRange(2, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call,typeof(UVMPublicTransportWorldInfoPanel).GetMethod("UpdateBindings", RedirectorUtils.allFlags) ),
                new CodeInstruction(OpCodes.Ret ),
            });
            return inst;
        }

        public static bool CheckEnabled() => PluginManager.instance.FindPluginInfo(typeof(TransportLinesManagerMod).Assembly)?.isEnabled ?? false;
        public static bool ResetScrollPosition() => false;
        #endregion

        #region Overridable

        public static void OverrideStart(PublicTransportWorldInfoPanel __instance)
        {
            m_obj.origInstance = __instance;
            __instance.component.width = 800;

            BindComponents(__instance);

            SetNameFieldProperties();

            KlyteMonoUtils.CreateTabsComponent(out m_obj.m_lineConfigTabs, out _, __instance.transform, "LineConfig", new Vector4(15, 45, 365, 30), new Vector4(15, 80, 380, 445));

            m_obj.m_childControls.Add("Default", TabCommons.CreateTabLocalized<UVMMainWIPTab>(m_obj.m_lineConfigTabs, "ThumbStatistics", "K45_TLM_WIP_STATS_TAB", "Default", false));
            m_obj.m_childControls.Add("Reports", TabCommons.CreateTabLocalized<TLMReportsTab>(m_obj.m_lineConfigTabs, "IconMessage", "K45_TLM_WIP_REPORT_TAB", "Reports", false));
            m_obj.m_childControls.Add("Budget", TabCommons.CreateTabLocalized<UVMBudgetConfigTab>(m_obj.m_lineConfigTabs, "InfoPanelIconCurrency", "K45_TLM_WIP_BUDGET_CONFIGURATION_TAB", "Budget", false));
            m_obj.m_childControls.Add("Ticket", TabCommons.CreateTabLocalized<TLMTicketConfigTab>(m_obj.m_lineConfigTabs, "FootballTicketIcon", "K45_TLM_WIP_TICKET_CONFIGURATION_TAB", "Ticket", false));
            m_obj.m_childControls.Add("AssetSelection", TabCommons.CreateTabLocalized<TLMAssetSelectorTab>(m_obj.m_lineConfigTabs, "IconPolicyFreePublicTransport", "K45_TLM_WIP_ASSET_SELECTION_TAB", "AssetSelection", false));
            m_obj.m_childControls.Add("DepotSelection", TabCommons.CreateTabLocalized<TLMDepotSelectorTab>(m_obj.m_lineConfigTabs, "UIFilterBigBuildings", "K45_TLM_WIP_DEPOT_SELECTION_TAB", "DepotSelection", false));
            m_obj.m_childControls.Add("PrefixConfig", TabCommons.CreateTabLocalized<TLMPrefixOptionsTab>(m_obj.m_lineConfigTabs, "InfoIconLevel", "K45_TLM_WIP_PREFIX_CONFIG_TAB", "PrefixConfig", false));

            m_obj.m_childControls.Add("StopsPanel", __instance.Find<UIPanel>("StopsPanel").parent.gameObject.AddComponent<UVMTransportLineLinearMap>());
            DestroyNotUsed(__instance);

            m_obj.m_specificConfig = UIHelperExtension.AddCheckboxLocale(__instance.component, "K45_TLM_USE_SPECIFIC_CONFIG", false, (x) =>
            {
                if (GetLineID(out ushort lineId, out ushort buildingId))
                {
                    if (buildingId == 0)
                    {
                        TLMTransportLineExtension.Instance.SetUseCustomConfig(lineId, x);
                        MarkDirty(typeof(UVMPublicTransportWorldInfoPanel));
                    }
                }
            });
            m_obj.m_specificConfig.relativePosition = new Vector3(10, 530);
            m_obj.m_specificConfig.isInteractive = false;
            KlyteMonoUtils.LimitWidthAndBox(m_obj.m_specificConfig.label, 400);
        }

        private static void BindComponents(PublicTransportWorldInfoPanel __instance)
        {
            //PARENT
            m_obj.m_nameField = __instance.Find<UITextField>("LineName");
            m_obj.m_vehicleType = __instance.Find<UISprite>("VehicleType");
            m_obj.m_vehicleType.size = new Vector2(32, 22);
            m_obj.m_deleteButton = __instance.Find<UIButton>("DeleteLine");
        }

        private static void DestroyNotUsed(PublicTransportWorldInfoPanel __instance)
        {
            FakeDestroy(__instance.Find("ActivityPanel"));
            FakeDestroy(__instance.Find<UIPanel>("LineModelSelectorContainer"));
            FakeDestroy(__instance.Find<UILabel>("ModelLabel"));
            FakeDestroy(__instance.Find<UILabel>("LabelPassengers"));

            FakeDestroy(__instance.Find<UISlider>("SliderModifyVehicleCount"));
            FakeDestroy(__instance.Find<UILabel>("VehicleCountPercent"));
            FakeDestroy(__instance.Find<UILabel>("VehicleAmount"));
            FakeDestroy(__instance.Find<UIPanel>("PanelVehicleCount"));

            FakeDestroy(__instance.Find<UISlider>("SliderTicketPrice"));
            FakeDestroy(__instance.Find<UILabel>("LabelTicketPrice"));
            FakeDestroy(__instance.Find<UIPanel>("TicketPriceSection"));
        }

        public static void FakeDestroy(UIComponent comp)
        {
            comp.isVisible = false;
            comp.isEnabled = false;
            comp.isInteractive = false;
        }

        private static void SetNameFieldProperties()
        {
            if (m_obj.m_nameField != null)
            {
                m_obj.m_nameField.maxLength = 100;
                m_obj.m_nameField.eventTextSubmitted += OnRename;
            }
        }

        public static bool OnEnableOverride()
        {
            Singleton<TransportManager>.instance.eventLineNameChanged += OnLineNameChanged;

            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_obj.m_childControls)
            {
                tab.Value.OnEnable();
            }
            return false;
        }

        public static bool OnDisableOverride()
        {
            Singleton<TransportManager>.instance.eventLineNameChanged -= OnLineNameChanged;
            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_obj.m_childControls)
            {
                tab.Value.OnDisable();
            }
            return false;
        }

        protected static void UpdateBindings()
        {
            if (GetLineID(out ushort lineID, out ushort buildingId))
            {
                if (buildingId == 0)
                {
                    if (lineID < TransportManager.MAX_LINE_COUNT)
                    {
                        if (m_obj.m_cachedLength != Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_totalLength || m_dirty)
                        {
                            OnSetTarget();
                        }
                        m_obj.m_vehicleType.spriteName = GetVehicleTypeIcon();

                        foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_obj.m_childControls)
                        {
                            if (tab.Value.MayBeVisible())
                            {
                                tab.Value.UpdateBindings();
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("INVALID LINE TO UPDATE: " + lineID);
                    }
                }
                else
                {
                    if (m_dirty)
                    {
                        OnSetTarget();
                    }
                    m_obj.m_vehicleType.spriteName = GetVehicleTypeIcon();
                    foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_obj.m_childControls)
                    {
                        if (tab.Value.MayBeVisible())
                        {
                            tab.Value.UpdateBindings();
                        }
                    }
                }
            }
        }

        public static bool OnLinesOverviewClicked()
        {
            TransportLinesManagerMod.Instance.OpenPanelAtModTab();
            TLMPanel.Instance.OpenAt(GetCurrentTSD());
            return false;
        }



        protected static bool OnSetTarget()
        {
            GetLineID(out ushort lineID, out ushort buildingId);
            if (buildingId == 0)
            {
                if (lineID >= TransportManager.MAX_LINE_COUNT)
                {
                    throw new Exception($"INVALID LINE SET AS TARGET: {lineID}");
                }
                if (lineID != 0)
                {
                    m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(lineID);
                    m_obj.m_nameField.Enable();
                    m_obj.m_specificConfig.isVisible = TransportSystemDefinition.FromLineId(lineID, buildingId).HasVehicles();
                    m_obj.m_specificConfig.isChecked = TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineID);
                    m_obj.m_cachedLength = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_totalLength;
                    m_obj.m_deleteButton.isVisible = true;
                }
                else
                {
                    m_obj.m_nameField.text = string.Format(Locale.Get("K45_TLM_OUTSIDECONNECTION_LISTNAMETEMPLATE"), GetCurrentTSD().GetTransportName());
                    m_obj.m_nameField.Disable();
                    m_obj.m_specificConfig.isVisible = false;
                    m_obj.m_deleteButton.isVisible = false;
                }
            }
            else
            {
                var lines = TransportLinesManagerMod.Controller.BuildingLines.SafeGet(buildingId);
                if (lines == null)
                {
                    return false;
                }
                var lineObj = lines[lineID];
                m_obj.m_nameField.text = string.Format(Locale.Get("K45_TLM_OUTSIDECONNECTION_TARGETCITYTEMPLATE"), TLMStationUtils.GetStationName(lineObj.DstStop, lineID, lineObj.Info.m_class.m_subService, buildingId));
                m_obj.m_nameField.Disable();
                m_obj.m_specificConfig.isVisible = false;
                m_obj.m_deleteButton.isVisible = false;
            }

            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_obj.m_childControls)
            {
                if (tab.Value.MayBeVisible())
                {
                    m_obj.m_lineConfigTabs.ShowTab(tab.Key);
                    tab.Value.OnSetTarget(m_dirtySource);
                }
                else
                {
                    m_obj.m_lineConfigTabs.HideTab(tab.Key);
                    tab.Value.Hide();
                }
            }
            m_dirty = false;
            m_dirtySource = null;

            if (m_obj.m_lineConfigTabs.selectedIndex == -1 || !(m_obj.m_lineConfigTabs.tabPages.components[m_obj.m_lineConfigTabs.selectedIndex].GetComponent<IUVMPTWIPChild>()?.MayBeVisible() ?? false))
            {
                for (int i = 0; i < m_obj.m_lineConfigTabs.tabCount; i++)
                {
                    if (m_obj.m_lineConfigTabs.tabPages.components[i].GetComponent<IUVMPTWIPChild>()?.MayBeVisible() ?? false)
                    {
                        m_obj.m_lineConfigTabs.selectedIndex = i;
                        break;
                    }
                }
            }

            return false;
        }

        public static void MarkDirty(Type source) => SimulationManager.instance.StartCoroutine(MarkDirtyAsync(source));

        private static IEnumerator MarkDirtyAsync(Type source)
        {
            yield return 0;
            m_dirty = true;
            m_dirtySource = source;
            yield break;
        }

        #endregion


        public static bool OnGotFocus()
        {
            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_obj.m_childControls)
            {
                tab.Value.OnGotFocus();
            }
            return false;
        }


        private static void OnLineNameChanged(ushort id)
        {
            GetLineID(out ushort lineId, out ushort buildingId);
            if (buildingId > 0)
            {
                return;
            }
            if (lineId > 0 && id == lineId)
            {
                m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(id);
            }
        }
        private static void OnRename(UIComponent comp, string text)
        {
            GetLineID(out ushort lineId, out ushort buildingId);
            if (buildingId > 0)
            {
                return;
            }
            if (lineId > 0)
            {
                m_obj.origInstance.StartCoroutine(TLMController.Instance.RenameCoroutine(lineId, text));
            }
        }

        internal static UVMPublicTransportWorldInfoPanelObject.LineType GetLineType(ushort lineID, ushort buildingId)
        {
            if (buildingId == 0)
            {
                string name = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].Info.name;
                if (name != null)
                {
                    if (name == "Sightseeing Bus")
                    {
                        return UVMPublicTransportWorldInfoPanelObject.LineType.TouristBus;
                    }
                    if (name == "Pedestrian")
                    {
                        return UVMPublicTransportWorldInfoPanelObject.LineType.WalkingTour;
                    }
                }
            }
            return UVMPublicTransportWorldInfoPanelObject.LineType.Default;
        }



        public static void OnBudgetClicked()
        {
            if (ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy))
            {
                ToolsModifierControl.mainToolbar.ShowEconomyPanel(1);
                WorldInfoPanel.Hide<PublicTransportWorldInfoPanel>();
            }
        }


        internal static bool GetLineID(out ushort lineId, out ushort buildingId)
        {
            if (m_obj.CurrentInstanceID.Type == (InstanceType)TLMInstanceType.TransportSystemDefinition)
            {
                buildingId = 0;
                lineId = 0;
                return true;
            }
            if (m_obj.CurrentInstanceID.Type == (InstanceType)TLMInstanceType.BuildingLines)
            {
                buildingId = (ushort)(m_obj.CurrentInstanceID.Index >> 8);
                lineId = (ushort)(m_obj.CurrentInstanceID.Index & 0xFF);
                return true;
            }
            buildingId = 0;
            if (m_obj.CurrentInstanceID.Type == InstanceType.TransportLine)
            {
                lineId = m_obj.CurrentInstanceID.TransportLine;
                return true;
            }
            if (m_obj.CurrentInstanceID.Type == InstanceType.Vehicle)
            {
                ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[m_obj.CurrentInstanceID.Vehicle].GetFirstVehicle(m_obj.CurrentInstanceID.Vehicle);
                if (firstVehicle != 0)
                {
                    lineId = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle].m_transportLine;
                    return true;
                }
            }
            buildingId = 0xFFFF;
            lineId = 0xFFFF;
            return false;
        }

        internal static TransportSystemDefinition GetCurrentTSD() => GetLineID(out ushort lineId, out ushort buildingId) && (lineId > 0 || buildingId != 0) ? TransportSystemDefinition.FromLineId(lineId, buildingId) : TransportSystemDefinition.FromIndex(m_obj.CurrentInstanceID.Index);

        internal static void ForceReload() => OnSetTarget();

        public static string GetVehicleTypeIcon() => GetCurrentTSD()?.GetTransportTypeIcon();


        internal static UVMPublicTransportWorldInfoPanelObject m_obj;
        private static bool m_dirty;
        private static Type m_dirtySource;

        public Redirector RedirectorInstance => this;

        public class UVMPublicTransportWorldInfoPanelObject
        {

            public readonly Dictionary<string, IUVMPTWIPChild> m_childControls = new Dictionary<string, IUVMPTWIPChild>();


            internal PublicTransportWorldInfoPanel origInstance = null;

            private Func<PublicTransportWorldInfoPanel, InstanceID> m_getterInstanceId = ReflectionUtils.GetGetFieldDelegate<PublicTransportWorldInfoPanel, InstanceID>("m_InstanceID", typeof(PublicTransportWorldInfoPanel));
            internal InstanceID CurrentInstanceID => origInstance is null ? (default) : m_getterInstanceId(origInstance);

            internal UITextField m_nameField;

            internal UISprite m_vehicleType;

            internal UICheckBox m_specificConfig;

            internal UIButton m_deleteButton;

            internal float m_cachedLength;

            internal enum LineType
            {
                Default,
                TouristBus,
                WalkingTour
            }

            internal UITabstrip m_lineConfigTabs;
        }
    }
}