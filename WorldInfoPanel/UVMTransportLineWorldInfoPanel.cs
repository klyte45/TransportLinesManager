using ColossalFramework;
using ColossalFramework.UI;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    public class UVMPublicTransportWorldInfoPanel : Redirector, IRedirectable
    {
        public static UVMPublicTransportWorldInfoPanel Instance { get; private set; }

        private static readonly Dictionary<string, IUVMPTWIPChild> m_childControls = new Dictionary<string, IUVMPTWIPChild>();

        #region Awake 
        public void Awake()
        {
            Instance = this;

            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("UpdateBindings", RedirectorUtils.allFlags), null, null, typeof(UVMPublicTransportWorldInfoPanel).GetMethod("TranspileUpdateBindings", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnEnable", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnEnableOverride", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnDisable", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnDisableOverride", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnLinesOverviewClicked", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnLinesOverviewClicked", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("ResetScrollPosition", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("ResetScrollPosition", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnSetTarget", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnSetTarget", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnGotFocus", RedirectorUtils.allFlags), typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OnGotFocus", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnLineColorChanged", RedirectorUtils.allFlags), typeof(Redirector).GetMethod("PreventDefault", RedirectorUtils.allFlags));
            AddRedirect(typeof(PublicTransportWorldInfoPanel).GetMethod("OnLineNameChanged", RedirectorUtils.allFlags), typeof(Redirector).GetMethod("PreventDefault", RedirectorUtils.allFlags));
            TransportManager.instance.eventLineColorChanged += (x) =>
            {
                if (x == GetLineID())
                {
                    MarkDirty(null);
                }
            };
            TransportManager.instance.eventLineNameChanged += (x) =>
            {
                if (x == GetLineID())
                {
                    m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(x);
                }
            };
        }

        public static IEnumerable<CodeInstruction> TranspileStart(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>
            {
                instructions.ElementAt(0),
                instructions.ElementAt(1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OverrideStart", RedirectorUtils.allFlags) ),
                new CodeInstruction(OpCodes.Ret ),
            };
            return inst;
        }
        public static IEnumerable<CodeInstruction> TranspileUpdateBindings(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>
            {
                instructions.ElementAt(0),
                instructions.ElementAt(1),
                new CodeInstruction(OpCodes.Call,typeof(UVMPublicTransportWorldInfoPanel).GetMethod("UpdateBindings", RedirectorUtils.allFlags) ),
                new CodeInstruction(OpCodes.Ret ),
            };
            return inst;
        }

        #endregion

        #region Overridable

        public static void OverrideStart(PublicTransportWorldInfoPanel __instance)
        {
            m_obj.origInstance = __instance;
            __instance.component.width = 800;

            BindComponents(__instance);

            SetNameFieldProperties();



            KlyteMonoUtils.CreateTabsComponent(out m_obj.m_lineConfigTabs, out _, __instance.transform, "LineConfig", new Vector4(15, 45, 365, 30), new Vector4(15, 80, 380, 440));

            m_childControls.Add("Default", TabCommons.CreateTab<UVMMainWIPTab>(m_obj.m_lineConfigTabs, "ThumbStatistics", "K45_TLM_WIP_STATS_TAB", "Default", false));
            m_childControls.Add("FinanceReport", TabCommons.CreateTab<TLMLineFinanceReportTab>(m_obj.m_lineConfigTabs, "IconMessage", "K45_TLM_WIP_FINANCE_REPORT_TAB", "FinanceReport", false));
            m_childControls.Add("Budget", TabCommons.CreateTab<UVMBudgetConfigTab>(m_obj.m_lineConfigTabs, "InfoPanelIconCurrency", "K45_TLM_WIP_BUDGET_CONFIGURATION_TAB", "Budget", false));
            m_childControls.Add("Ticket", TabCommons.CreateTab<TLMTicketConfigTab>(m_obj.m_lineConfigTabs, "FootballTicketIcon", "K45_TLM_WIP_TICKET_CONFIGURATION_TAB", "Ticket", false));
            m_childControls.Add("AssetSelection", TabCommons.CreateTab<TLMAssetSelectorTab>(m_obj.m_lineConfigTabs, "IconPolicyFreePublicTransport", "K45_TLM_WIP_ASSET_SELECTION_TAB", "AssetSelection", false));
            m_childControls.Add("DepotSelection", TabCommons.CreateTab<TLMDepotSelectorTab>(m_obj.m_lineConfigTabs, "UIFilterBigBuildings", "K45_TLM_WIP_DEPOT_SELECTION_TAB", "DepotSelection", false));
            m_childControls.Add("PrefixConfig", TabCommons.CreateTab<TLMPrefixOptionsTab>(m_obj.m_lineConfigTabs, "InfoIconLevel", "K45_TLM_WIP_PREFIX_CONFIG_TAB", "PrefixConfig", false));
            m_childControls.Add("StopsPanel", __instance.Find<UIPanel>("StopsPanel").parent.gameObject.AddComponent<UVMTransportLineLinearMap>());
            DestroyNotUsed(__instance);

            m_obj.m_specificConfig = UIHelperExtension.AddCheckboxLocale(__instance.component, "K45_TLM_USE_SPECIFIC_CONFIG", false, (x) =>
            {
                TLMTransportLineExtension.Instance.SetUseCustomConfig(GetLineID(), x);
                MarkDirty(typeof(UVMPublicTransportWorldInfoPanel));
            });
            m_obj.m_specificConfig.relativePosition = new Vector3(10, 530);
            KlyteMonoUtils.LimitWidthAndBox(m_obj.m_specificConfig.label, 500);
        }

        private static void BindComponents(PublicTransportWorldInfoPanel __instance)
        {
            //PARENT
            m_obj.m_nameField = __instance.Find<UITextField>("LineName");
            m_obj.m_vehicleType = __instance.Find<UISprite>("VehicleType");
            m_obj.m_vehicleType.size = new Vector2(32, 22);
        }

        private static void DestroyNotUsed(PublicTransportWorldInfoPanel __instance)
        {
            Destroy(__instance.Find("ActivityPanel"));
            Destroy(__instance.Find<UIPanel>("LineModelSelectorContainer"));
            Destroy(__instance.Find<UILabel>("ModelLabel"));
            Destroy(__instance.Find<UILabel>("LabelPassengers"));

            Destroy(__instance.Find<UISlider>("SliderModifyVehicleCount"));
            Destroy(__instance.Find<UILabel>("VehicleCountPercent"));
            Destroy(__instance.Find<UILabel>("VehicleAmount"));
            Destroy(__instance.Find<UIPanel>("PanelVehicleCount"));

            Destroy(__instance.Find<UISlider>("SliderTicketPrice"));
            Destroy(__instance.Find<UILabel>("LabelTicketPrice"));
            Destroy(__instance.Find<UIPanel>("TicketPriceSection"));
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

            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_childControls)
            {
                tab.Value.OnEnable();
            }
            return false;
        }

        public static bool OnDisableOverride()
        {
            Singleton<TransportManager>.instance.eventLineNameChanged -= OnLineNameChanged;
            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_childControls)
            {
                tab.Value.OnDisable();
            }
            return false;
        }

        protected static void UpdateBindings()
        {
            ushort lineID = GetLineID();
            if (lineID != 0)
            {
                if (m_obj.m_cachedLength != Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_totalLength || m_dirty)
                {
                    OnSetTarget();
                }
                m_obj.m_vehicleType.spriteName = GetVehicleTypeIcon(lineID);

                foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_childControls)
                {
                    if (tab.Value.MayBeVisible())
                    {
                        tab.Value.UpdateBindings();
                    }
                }
            }
        }

        public static bool OnLinesOverviewClicked()
        {
            TransportLinesManagerMod.Instance.OpenPanelAtModTab();
            TLMPanel.Instance.OpenAt(TransportSystemDefinition.From(GetLineID()));
            return false;
        }



        protected static bool OnSetTarget()
        {
            ushort lineID = GetLineID();
            if (lineID != 0)
            {
                m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(lineID);
                m_obj.m_specificConfig.isVisible = TransportSystemDefinition.From(lineID).HasVehicles();
                m_obj.m_specificConfig.isChecked = TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineID);
            }

            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_childControls)
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

            m_obj.m_cachedLength = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_totalLength;
            if (!m_dirty)
            {
                ReflectionUtils.RunPrivateMethod<object>(m_obj.origInstance, "UpdateBindings");
            }

            m_dirty = false;
            m_dirtySource = null;
            return false;
        }

        public static void MarkDirty(Type source) => Instance.StartCoroutine(MarkDirtyAsync(source));

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
            foreach (KeyValuePair<string, IUVMPTWIPChild> tab in m_childControls)
            {
                tab.Value.OnGotFocus();
            }
            return false;
        }


        private static void OnLineNameChanged(ushort id)
        {
            if (id == GetLineID())
            {
                m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(id);
            }
        }



        private static void OnRename(UIComponent comp, string text) => m_obj.origInstance.StartCoroutine(Instance.RenameCoroutine(GetLineID(), text));




        private IEnumerator RenameCoroutine(ushort id, string newName)
        {
            if (Singleton<SimulationManager>.exists)
            {
                AsyncTask<bool> task = Singleton<SimulationManager>.instance.AddAction(Singleton<TransportManager>.instance.SetLineName(id, newName));
                yield return task.WaitTaskCompleted(this);
                if (GetLineID() == id)
                {
                    m_obj.m_nameField.text = Singleton<TransportManager>.instance.GetLineName(id);
                }
            }
            yield break;
        }

        internal static UVMPublicTransportWorldInfoPanelObject.LineType GetLineType(ushort lineID)
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


        internal static ushort GetLineID()
        {
            if (m_obj.CurrentInstanceID.Type == InstanceType.TransportLine)
            {
                return m_obj.CurrentInstanceID.TransportLine;
            }
            if (m_obj.CurrentInstanceID.Type == InstanceType.Vehicle)
            {
                ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[m_obj.CurrentInstanceID.Vehicle].GetFirstVehicle(m_obj.CurrentInstanceID.Vehicle);
                if (firstVehicle != 0)
                {
                    return Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle].m_transportLine;
                }
            }
            return 0;
        }

        public static string GetVehicleTypeIcon(ushort lineId) => TransportSystemDefinition.From(lineId).GetTransportTypeIcon();



        internal static UVMPublicTransportWorldInfoPanelObject m_obj = new UVMPublicTransportWorldInfoPanelObject();
        private static bool m_dirty;
        private static Type m_dirtySource;

        public Redirector RedirectorInstance => Instance;

        public class UVMPublicTransportWorldInfoPanelObject
        {
            internal PublicTransportWorldInfoPanel origInstance = null;

            private Func<PublicTransportWorldInfoPanel, InstanceID> m_getterInstanceId = ReflectionUtils.GetGetFieldDelegate<PublicTransportWorldInfoPanel, InstanceID>("m_InstanceID", typeof(PublicTransportWorldInfoPanel));
            internal InstanceID CurrentInstanceID
            {
                get {
                    if (origInstance == null)
                    {
                        return default;
                    }

                    return m_getterInstanceId(origInstance);
                }
            }

            internal UITextField m_nameField;

            internal UISprite m_vehicleType;

            internal UICheckBox m_specificConfig;

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