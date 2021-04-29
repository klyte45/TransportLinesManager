using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMDepotSelectorTab : UICustomControl, IUVMPTWIPChild
    {
        private UIHelperExtension m_uiHelper;
        private UILabel m_title;
        private Color m_lastColor = Color.clear;
        private UITemplateList<UIPanel> m_checkboxTemplateList;
        public void Awake() => CreateWindow();

        public UIPanel MainPanel { get; private set; }

        private UIScrollablePanel m_scrollablePanel;
        private UIScrollbar m_scrollbar;
        private Dictionary<string, UICheckBox> m_checkboxes = new Dictionary<string, UICheckBox>();
        private bool m_isLoading;
        private TransportSystemDefinition TransportSystem => TransportSystemDefinition.From(GetLineID());
        internal static ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        private void CreateWindow()
        {
            CreateMainPanel();

            CreateScrollPanel();

            CreateDepotLineTemplate();

            CreateTemplateList();
        }

        private void CreateTemplateList() => m_checkboxTemplateList = new UITemplateList<UIPanel>(m_scrollablePanel, "K45_TLM_DepotSelectionTabLineTemplate");

        private void CreateDepotLineTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(m_scrollablePanel.width - 40f, 36);
            panel.autoLayout = true;
            panel.wrapLayout = false;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            UICheckBox uiCheckbox = UIHelperExtension.AddCheckbox(panel, "AAAAAA", false);
            uiCheckbox.name = "DepotCheckbox";
            uiCheckbox.height = 29f;
            uiCheckbox.width = 310f;
            uiCheckbox.label.processMarkup = true;
            uiCheckbox.label.textScale = 0.8f;

            KlyteMonoUtils.CreateUIElement(out UIButton gotoButton, panel.transform, "GoTo", new Vector4(0, 0, 30, 30));
            KlyteMonoUtils.InitButton(gotoButton, true, "LineDetailButton");

            TLMUiTemplateUtils.GetTemplateDict()["K45_TLM_DepotSelectionTabLineTemplate"] = panel;
        }

        private void CreateMainPanel()
        {
            MainPanel = GetComponent<UIPanel>();
            MainPanel.relativePosition = new Vector3(510f, 0.0f);
            MainPanel.width = 350;
            MainPanel.height = GetComponentInParent<UIComponent>().height;
            MainPanel.zOrder = 50;
            MainPanel.color = new Color32(255, 255, 255, 255);
            MainPanel.name = "AssetSelectorWindow";
            MainPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            MainPanel.autoLayout = false;
            MainPanel.useCenter = true;
            MainPanel.wrapLayout = false;
            MainPanel.canFocus = true;

            KlyteMonoUtils.CreateUIElement(out m_title, MainPanel.transform);
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.autoSize = false;
            m_title.autoHeight = true;
            m_title.width = MainPanel.width - 30f;
            m_title.relativePosition = new Vector3(5, 5);
            m_title.textScale = 0.9f;
            m_title.localeID = "K45_TLM_ASSETS_FOR_PREFIX";
        }

        private void CreateScrollPanel()
        {
            KlyteMonoUtils.CreateUIElement(out m_scrollablePanel, MainPanel.transform);
            m_scrollablePanel.width = MainPanel.width - 20f;
            m_scrollablePanel.height = MainPanel.height - 40f;
            m_scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            m_scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_scrollablePanel.scrollPadding = new RectOffset(10, 10, 10, 10);
            m_scrollablePanel.autoLayout = true;
            m_scrollablePanel.clipChildren = true;
            m_scrollablePanel.relativePosition = new Vector3(5, 40);
            m_scrollablePanel.backgroundSprite = "ScrollbarTrack";

            KlyteMonoUtils.CreateUIElement(out UIPanel trackballPanel, MainPanel.transform);
            trackballPanel.width = 10f;
            trackballPanel.height = m_scrollablePanel.height;
            trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
            trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            trackballPanel.autoLayout = true;
            trackballPanel.relativePosition = new Vector3(MainPanel.width - 15, m_scrollablePanel.relativePosition.y);


            KlyteMonoUtils.CreateUIElement(out m_scrollbar, trackballPanel.transform);
            m_scrollbar.width = 10f;
            m_scrollbar.height = m_scrollbar.parent.height;
            m_scrollbar.orientation = UIOrientation.Vertical;
            m_scrollbar.pivot = UIPivotPoint.BottomLeft;
            m_scrollbar.AlignTo(trackballPanel, UIAlignAnchor.TopRight);
            m_scrollbar.minValue = 0f;
            m_scrollbar.value = 0f;
            m_scrollbar.incrementAmount = 25f;

            KlyteMonoUtils.CreateUIElement(out UISlicedSprite scrollBg, m_scrollbar.transform);
            scrollBg.relativePosition = Vector2.zero;
            scrollBg.autoSize = true;
            scrollBg.size = scrollBg.parent.size;
            scrollBg.fillDirection = UIFillDirection.Vertical;
            scrollBg.spriteName = "ScrollbarTrack";
            m_scrollbar.trackObject = scrollBg;

            KlyteMonoUtils.CreateUIElement(out UISlicedSprite scrollFg, scrollBg.transform);
            scrollFg.relativePosition = Vector2.zero;
            scrollFg.fillDirection = UIFillDirection.Vertical;
            scrollFg.autoSize = true;
            scrollFg.width = scrollFg.parent.width - 4f;
            scrollFg.spriteName = "ScrollbarThumb";
            m_scrollbar.thumbObject = scrollFg;
            m_scrollablePanel.verticalScrollbar = m_scrollbar;
            m_scrollablePanel.eventMouseWheel += delegate (UIComponent component, UIMouseEventParameter param)
            {
                m_scrollablePanel.scrollPosition += new Vector2(0f, Mathf.Sign(param.wheelDelta) * -1f * m_scrollbar.incrementAmount);
            };


        }


        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }

            TransportSystemDefinition tsd = TransportSystem;
            if (!tsd.HasVehicles())
            {
                MainPanel.isVisible = false;
                return;
            }
            m_isLoading = true;

            var lineId = GetLineID();
            List<ushort> cityDepotList = TLMDepotUtils.GetAllDepotsFromCity(tsd);
            Interfaces.IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
            List<ushort> targetDepotList = config.GetAllowedDepots(tsd, lineId);
            UIPanel[] depotChecks = m_checkboxTemplateList.SetItemCount(cityDepotList.Count);
            LogUtils.DoLog($"depotChecks = {depotChecks.Length}");
            for (int idx = 0; idx < cityDepotList.Count; idx++)
            {
                ushort buildingID = cityDepotList[idx];
                UICheckBox uiCheck = depotChecks[idx].GetComponentInChildren<UICheckBox>();
                uiCheck.objectUserData = buildingID;
                uiCheck.isChecked = targetDepotList.Contains(buildingID);

                UILabel uilabel = uiCheck.label;

                uilabel.prefix = BuildingUtils.GetBuildingName(buildingID, out _, out _);
                ref Building depotBuilding = ref BuildingManager.instance.m_buildings.m_buffer[buildingID];
                Vector3 sidewalk = depotBuilding.CalculateSidewalkPosition();
                TransportLinesManagerMod.Controller.ConnectorADR.GetAddressStreetAndNumber(sidewalk, depotBuilding.m_position, out int number, out string streetName);
                byte districtId = DistrictManager.instance.GetDistrict(sidewalk);
                uilabel.text = $"\n<color gray>{streetName}, {number} - {(districtId == 0 ? SimulationManager.instance.m_metaData.m_CityName : DistrictManager.instance.GetDistrictName(districtId))}</color>";



                if (uilabel.objectUserData == null)
                {
                    uiCheck.eventCheckChanged += (x, y) =>
                    {
                        if (!m_isLoading)
                        {
                            if (y)
                            {
                                TLMLineUtils.GetEffectiveExtensionForLine(UVMPublicTransportWorldInfoPanel.GetLineID()).AddDepotForLine(UVMPublicTransportWorldInfoPanel.GetLineID(), (ushort)x.objectUserData);
                            }
                            else
                            {
                                TLMLineUtils.GetEffectiveExtensionForLine(UVMPublicTransportWorldInfoPanel.GetLineID()).RemoveDepotForLine(UVMPublicTransportWorldInfoPanel.GetLineID(), (ushort)x.objectUserData);
                            }
                        }
                    };
                    KlyteMonoUtils.LimitWidthAndBox(uiCheck.label, 280, true);
                    UIButton gotoButton = depotChecks[idx].Find<UIButton>("GoTo");
                    gotoButton.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
                    {
                        ushort buildingId = (ushort)uiCheck.objectUserData;
                        if (buildingId != 0)
                        {
                            Vector3 position = BuildingManager.instance.m_buildings.m_buffer[buildingId].m_position;
                            ToolsModifierControl.cameraController.SetTarget(new InstanceID() { Building = buildingId }, position, true);
                        }
                    };
                    uilabel.objectUserData = true;
                }
            }

            if (config is TLMTransportLineConfiguration)
            {
                m_title.text = string.Format(Locale.Get("K45_TLM_DEPOT_SELECT_WINDOW_TITLE"), TLMLineUtils.GetLineStringId(GetLineID()));
            }
            else
            {
                int prefix = (int)TLMPrefixesUtils.GetPrefix(GetLineID());
                m_title.text = string.Format(Locale.Get("K45_TLM_DEPOT_SELECT_WINDOW_TITLE_PREFIX"), prefix > 0 ? NumberingUtils.GetStringFromNumber(TLMPrefixesUtils.GetStringOptionsForPrefix(tsd), prefix + 1) : Locale.Get("K45_TLM_UNPREFIXED"), tsd.GetTransportName());
            }


            m_isLoading = false;

        }



        public void UpdateBindings() { }
        public void OnEnable() { }
        public void OnDisable() { }
        public void OnGotFocus() { }
        public bool MayBeVisible() => TransportSystem.HasVehicles();

        public void Hide() => MainPanel.isVisible = false;
    }
}
