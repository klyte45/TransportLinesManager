using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager.WorldInfoPanelExt.Components
{
    internal class TLMDepotPrefixSelection<TSD> : MonoBehaviour where TSD : TLMSysDef<TSD>
    {
        private UIPanel m_parent => m_parentObj.mainPanel;
        private LateralListSelectParentInterface m_parentObj;
        private UIPanel m_mainPanel;
        private UIHelperExtension m_uiHelper;
        private UILabel m_title;
        public LateralListSelectParentInterface parent => m_parentObj;

        private UIScrollablePanel m_scrollablePanel;
        private UIScrollbar m_scrollbar;
        private UICheckBox[] m_prefixesCheckboxes = new UICheckBox[66];
        private TransportSystemDefinition m_tsd => Singleton<TSD>.instance.GetTSD();
        private bool m_isLoading;


        private UIButton m_addAllPrefixesButton;
        private UIButton m_removeAllPrefixesButton;

        public void Awake()
        {
            m_parentObj = GetComponentInParent<LateralListSelectParentInterface>();
            if (m_parentObj != null)
            {
                CreateWindow();
            }
        }

        private void CreateWindow()
        {
            CreateMainPanel();

            CreateScrollPanel();

            BindParentChanges();

            CreateAdditionalButtons();
        }

        private void CreateAdditionalButtons()
        {
            TLMUtils.createUIElement(out m_addAllPrefixesButton, m_mainPanel.transform);
            m_addAllPrefixesButton.relativePosition = new Vector3(195, 25);
            m_addAllPrefixesButton.text = Locale.Get("TLM_ADD_ALL_SHORT");
            ;
            m_addAllPrefixesButton.textScale = 0.6f;
            m_addAllPrefixesButton.width = 50;
            m_addAllPrefixesButton.height = 15;
            m_addAllPrefixesButton.tooltip = Locale.Get("TLM_ADD_ALL_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_addAllPrefixesButton, true, "ButtonMenu");
            m_addAllPrefixesButton.name = "AddAll";
            m_addAllPrefixesButton.isVisible = true;
            m_addAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                var buildingId = WorldInfoPanel.GetCurrentInstanceID();
                DepotAI depotAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId.Building].Info.GetAI() as DepotAI;
                if (m_tsd.isFromSystem(depotAI))
                {
                    TLMDepotAI.addAllPrefixesToDepot(buildingId.Building, m_tsd.isFromSystem(depotAI.m_secondaryTransportInfo));
                    updateCheckboxes(ref buildingId);
                }
            };


            TLMUtils.createUIElement(out m_removeAllPrefixesButton, m_mainPanel.transform);
            m_removeAllPrefixesButton.relativePosition = new Vector3(195, 5);
            m_removeAllPrefixesButton.text = Locale.Get("TLM_REMOVE_ALL_SHORT");
            m_removeAllPrefixesButton.textScale = 0.6f;
            m_removeAllPrefixesButton.width = 50;
            m_removeAllPrefixesButton.height = 15;
            m_removeAllPrefixesButton.tooltip = Locale.Get("TLM_REMOVE_ALL_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_removeAllPrefixesButton, true, "ButtonMenu");
            m_removeAllPrefixesButton.name = "RemoveAll";
            m_removeAllPrefixesButton.isVisible = true;
            m_removeAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                var buildingId = WorldInfoPanel.GetCurrentInstanceID();
                DepotAI depotAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId.Building].Info.GetAI() as DepotAI;
                if (m_tsd.isFromSystem(depotAI))
                {
                    TLMDepotAI.removeAllPrefixesFromDepot(buildingId.Building, m_tsd.isFromSystem(depotAI.m_secondaryTransportInfo));
                    updateCheckboxes(ref buildingId);
                }
            };
        }

        private void CreateMainPanel()
        {
            TLMUtils.createUIElement(out m_mainPanel, m_parent.transform);
            m_mainPanel.Hide();
            m_mainPanel.relativePosition = new Vector3(m_parent.width, 0.0f);
            m_mainPanel.width = 250;
            m_mainPanel.height = m_parent.height;
            m_mainPanel.zOrder = 50;
            m_mainPanel.color = new Color32(255, 255, 255, 255);
            m_mainPanel.backgroundSprite = "MenuPanel2";
            m_mainPanel.name = "PrefixSelectorWindow - " + m_tsd;
            m_mainPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            m_mainPanel.autoLayout = false;
            m_mainPanel.useCenter = true;
            m_mainPanel.wrapLayout = false;
            m_mainPanel.canFocus = true;
            TLMUtils.createDragHandle(m_mainPanel, m_mainPanel, 35f);

            TLMUtils.createUIElement(out m_title, m_mainPanel.transform);
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.autoSize = false;
            m_title.autoHeight = true;
            m_title.width = m_mainPanel.width - 70f;
            m_title.relativePosition = new Vector3(5, 5);
            m_title.textScale = 0.9f;
            m_title.prefix = Locale.Get("TLM_PREFIXES_SERVED") + "\n";
            m_title.text = TLMConfigWarehouse.getNameForTransportType(m_tsd.toConfigIndex());
        }

        private void CreateScrollPanel()
        {
            TLMUtils.createUIElement(out m_scrollablePanel, m_mainPanel.transform);
            m_scrollablePanel.width = m_mainPanel.width - 20f;
            m_scrollablePanel.height = m_mainPanel.height - 50f;
            m_scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            m_scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_scrollablePanel.autoLayout = true;
            m_scrollablePanel.clipChildren = true;
            m_scrollablePanel.relativePosition = new Vector3(5, 45);

            TLMUtils.createUIElement(out UIPanel trackballPanel, m_mainPanel.transform);
            trackballPanel.width = 10f;
            trackballPanel.height = m_scrollablePanel.height;
            trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
            trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            trackballPanel.autoLayout = true;
            trackballPanel.relativePosition = new Vector3(m_mainPanel.width - 15, 45);


            TLMUtils.createUIElement(out m_scrollbar, trackballPanel.transform);
            m_scrollbar.width = 10f;
            m_scrollbar.height = m_scrollbar.parent.height;
            m_scrollbar.orientation = UIOrientation.Vertical;
            m_scrollbar.pivot = UIPivotPoint.BottomLeft;
            m_scrollbar.AlignTo(trackballPanel, UIAlignAnchor.TopRight);
            m_scrollbar.minValue = 0f;
            m_scrollbar.value = 0f;
            m_scrollbar.incrementAmount = 25f;

            TLMUtils.createUIElement(out UISlicedSprite scrollBg, m_scrollbar.transform);
            scrollBg.relativePosition = Vector2.zero;
            scrollBg.autoSize = true;
            scrollBg.size = scrollBg.parent.size;
            scrollBg.fillDirection = UIFillDirection.Vertical;
            scrollBg.spriteName = "ScrollbarTrack";
            m_scrollbar.trackObject = scrollBg;

            TLMUtils.createUIElement(out UISlicedSprite scrollFg, scrollBg.transform);
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


            m_uiHelper = new UIHelperExtension(m_scrollablePanel);

            for (uint i = 0; i <= 65; i++)
            {
                uint j = i;
                m_prefixesCheckboxes[i] = (UICheckBox)m_uiHelper.AddCheckbox(i == 0 ? Locale.Get("TLM_UNPREFIXED") : i == 65 ? Locale.Get("TLM_REGIONAL") : i.ToString(), false, (x) =>
                {
                    if (!m_isLoading)
                    {
                        var buildingId = WorldInfoPanel.GetCurrentInstanceID();
                        DepotAI depotAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId.Building].Info.GetAI() as DepotAI;
                        togglePrefix(j, x, m_tsd.isFromSystem(depotAI.m_transportInfo), ref buildingId);
                    }
                });
            }
        }

        private void togglePrefix(uint prefix, bool value, bool secondary, ref InstanceID instanceID)
        {
            if (value)
            {
                TLMDepotAI.addPrefixToDepot(instanceID.Building, prefix, secondary);
            }
            else
            {
                TLMDepotAI.removePrefixFromDepot(instanceID.Building, prefix, secondary);
            }
        }


        private void BindParentChanges()
        {
            m_parentObj.eventWipOpen += (ref InstanceID instanceID) =>
            {
                TLMUtils.doLog("EventOnLineChanged");
                updateCheckboxes(ref instanceID);
            };
        }


        private void updateCheckboxes(ref InstanceID instanceID)
        {
            DepotAI depotAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[instanceID.Building].Info.GetAI() as DepotAI;
            bool secondary;
            if (m_tsd.isFromSystem(depotAI.m_transportInfo) && depotAI.m_maxVehicleCount > 0) { secondary = false; }
            else if (m_tsd.isFromSystem(depotAI.m_secondaryTransportInfo) && depotAI.m_maxVehicleCount2 > 0) { secondary = true; }
            else
            {
                m_mainPanel.isVisible = false;
                return;
            }
            m_mainPanel.isVisible = true;
            bool oldIsLoading = m_isLoading;
            m_isLoading = true;
            string[] prefixOptions = TLMUtils.getStringOptionsForPrefix(TransportSystemDefinition.from(secondary ? depotAI.m_secondaryTransportInfo : depotAI.m_transportInfo).toConfigIndex(), true, true, false);
            var prefixesServedList = TLMDepotAI.getPrefixesServedByDepot(instanceID.Building, secondary);
            for (uint i = 0; i <= 64; i++)
            {
                if (i < prefixOptions.Length)
                {
                    m_prefixesCheckboxes[i].isVisible = true;
                    m_prefixesCheckboxes[i].isChecked = prefixesServedList.Contains(i);
                    m_prefixesCheckboxes[i].text = prefixOptions[(int)i];
                }
                else
                {
                    m_prefixesCheckboxes[i].isVisible = false;
                }
            }
            m_prefixesCheckboxes[65].isChecked = prefixesServedList.Contains(65);
            m_prefixesCheckboxes[65].zOrder = 1;
            m_isLoading = oldIsLoading;
        }
    }
    internal sealed class TLMDepotPrefixSelectionNorBus : TLMDepotPrefixSelection<TLMSysDefNorBus> { }
    internal sealed class TLMDepotPrefixSelectionNorTrm : TLMDepotPrefixSelection<TLMSysDefNorTrm> { }
    internal sealed class TLMDepotPrefixSelectionNorMnr : TLMDepotPrefixSelection<TLMSysDefNorMnr> { }
    internal sealed class TLMDepotPrefixSelectionNorMet : TLMDepotPrefixSelection<TLMSysDefNorMet> { }
    internal sealed class TLMDepotPrefixSelectionNorTrn : TLMDepotPrefixSelection<TLMSysDefNorTrn> { }
    internal sealed class TLMDepotPrefixSelectionNorFer : TLMDepotPrefixSelection<TLMSysDefNorFer> { }
    internal sealed class TLMDepotPrefixSelectionNorBlp : TLMDepotPrefixSelection<TLMSysDefNorBlp> { }
    internal sealed class TLMDepotPrefixSelectionNorShp : TLMDepotPrefixSelection<TLMSysDefNorShp> { }
    internal sealed class TLMDepotPrefixSelectionNorPln : TLMDepotPrefixSelection<TLMSysDefNorPln> { }
    internal sealed class TLMDepotPrefixSelectionTouBus : TLMDepotPrefixSelection<TLMSysDefTouBus> { }
}
