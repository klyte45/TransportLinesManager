using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.UI;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.BuildingAIExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.LineList.ExtraUI
{
    internal class TLMPrefixEditorUI
    {

        public UIComponent m_PrefixEditor;
        private UITabstrip m_StripAsteriskTab;

        UIHelperExtension m_prefixSelectorPanel;

        public UIDropDown m_systemTypeDropDown = null;

        public readonly TLMCW.ConfigIndex[] m_transportTypeOrder = {
            TLMConfigWarehouse.ConfigIndex.PLANE_CONFIG,
            TLMConfigWarehouse.ConfigIndex.BLIMP_CONFIG ,
            TLMConfigWarehouse.ConfigIndex.SHIP_CONFIG,
            TLMConfigWarehouse.ConfigIndex.FERRY_CONFIG,
            TLMConfigWarehouse.ConfigIndex.TRAIN_CONFIG,
            TLMConfigWarehouse.ConfigIndex.METRO_CONFIG,
            TLMConfigWarehouse.ConfigIndex.MONORAIL_CONFIG,
            TLMConfigWarehouse.ConfigIndex.TRAM_CONFIG,
            TLMConfigWarehouse.ConfigIndex.BUS_CONFIG,
            TLMConfigWarehouse.ConfigIndex.EVAC_BUS_CONFIG };

        //asset editor

        TextList<string> m_defaultAssets;
        TextList<string> m_prefixAssets;
        UIDropDown m_prefixSelection;
        private UITextureSprite m_preview;
        AVOPreviewRenderer m_previewRenderer;
        VehicleInfo m_lastInfo;

        //per hour budget
        UITextField m_prefixName = null;
        UITextField m_ticketPrice = null;
        uint[] m_hourBudgets = new uint[8];
        UICheckBox m_chkSingleBudget = null;
        UICheckBox m_chkPerHourBudget = null;
        UISlider[] m_budgetSliders;
        bool m_isLoadingPrefixData;

        public void Init()
        {
            if (TLMSingleton.isIPTLoaded)
            {
                return;
            }

            InitPrefixesSelector();

            InitSubTabstrip();

            InitPrefixDetailTab();

            InitAssetSelectionTab();

            InitBudgetConfigTab();

            //------
            m_prefixSelection.isVisible = false;
            m_StripAsteriskTab.tabPages.enabled = false;
            m_StripAsteriskTab.enabled = false;
        }

        private void InitBudgetConfigTab()
        {
            UIHelperExtension perPeriodBudgetContainer = createNewAsteriskTab(Locale.Get("TLM_PREFIX_BUDGET"));
            m_budgetSliders = new UISlider[8];
            m_chkPerHourBudget = (UICheckBox)perPeriodBudgetContainer.AddCheckbox(Locale.Get("TLM_USE_PER_PERIOD_BUDGET"), false, delegate (bool val)
            {
                for (int i = 0; i < 8; i++)
                {
                    m_hourBudgets[i] = m_hourBudgets[0];
                }
                updateBudgetSliders();
            });
            m_chkSingleBudget = (UICheckBox)perPeriodBudgetContainer.AddCheckbox(Locale.Get("TLM_USE_SINGLE_BUDGET"), true, delegate (bool val) { updateBudgetSliders(); });
            m_chkPerHourBudget.group = m_chkPerHourBudget.parent;
            m_chkSingleBudget.group = m_chkPerHourBudget.parent;
            for (int i = 0; i < 8; i++)
            {
                var j = i;
                m_budgetSliders[i] = GenerateBudgetMultiplierField(perPeriodBudgetContainer, Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL", i) + ":", delegate (float f)
                {
                    m_budgetSliders[j].transform.parent.GetComponentInChildren<UILabel>().text = string.Format(" x{0:0.00}", f);
                    if (!m_isLoadingPrefixData)
                    {
                        m_hourBudgets[j] = (uint)(f * 100);
                        setBudgetMultiplierDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1));
                    }
                });
            }
        }

        private void InitAssetSelectionTab()
        {
            UIHelperExtension assetSelectionTabContainer = createNewAsteriskTab(Locale.Get("TLM_CITY_ASSETS_SELECTION"));
            m_defaultAssets = assetSelectionTabContainer.AddTextList(Locale.Get("TLM_DEFAULT_ASSETS"), new Dictionary<string, string>(), delegate (string idx) { }, 340, 250);
            m_prefixAssets = assetSelectionTabContainer.AddTextList(Locale.Get("TLM_ASSETS_FOR_PREFIX"), new Dictionary<string, string>(), delegate (string idx) { }, 340, 250);
            foreach (Transform t in ((UIPanel)assetSelectionTabContainer.self).transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }

            m_prefixAssets.root.backgroundSprite = "EmptySprite";
            m_prefixAssets.root.color = Color.white;
            m_prefixAssets.root.width = 340;
            m_defaultAssets.root.backgroundSprite = "EmptySprite";
            m_defaultAssets.root.width = 340;
            assetSelectionTabContainer.AddSpace(10);

            m_prefixAssets.EventOnSelect += M_defaultAssets_eventOnSelect;
            m_defaultAssets.EventOnSelect += M_defaultAssets_eventOnSelect;


            OnButtonClicked reload = delegate
            {
                reloadAssetsList(m_prefixSelection.selectedIndex);
            };
            assetSelectionTabContainer.AddButton(Locale.Get("TLM_ADD"), delegate
            {
                if (m_defaultAssets.unselected)
                    return;
                var selected = m_defaultAssets.selectedItem;
                if (selected == null || selected.Equals(default(string)))
                    return;
                addAssetToPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), selected);
                reload();
            });
            assetSelectionTabContainer.AddButton(Locale.Get("TLM_REMOVE"), delegate
            {
                if (m_prefixAssets.unselected)
                    return;
                var selected = m_prefixAssets.selectedItem;
                if (selected == null || selected.Equals(default(string)))
                    return;
                removeAssetFromPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), selected);
                reload();
            });

            assetSelectionTabContainer.AddButton(Locale.Get("TLM_REMOVE_ALL"), delegate
            {
                removeAllAssetsFromPrefixDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1));
                reload();
            });
            assetSelectionTabContainer.AddButton(Locale.Get("TLM_RELOAD"), delegate
            {
                reload();
            });
            assetSelectionTabContainer.AddSpace(5);

            setPreviewWindow(assetSelectionTabContainer);
        }

        private void setPreviewWindow(UIHelperExtension assetSelectionTabContainer)
        {
            UIPanel uIPanel = assetSelectionTabContainer.self.AddUIComponent<UIPanel>();
            uIPanel.backgroundSprite = "GenericPanel";
            uIPanel.width = 900f;
            uIPanel.height = 250f;
            uIPanel.relativePosition = new Vector3(0f, 350);
            this.m_preview = uIPanel.AddUIComponent<UITextureSprite>();
            this.m_preview.size = uIPanel.size;
            this.m_preview.relativePosition = Vector3.zero;
            this.m_previewRenderer = assetSelectionTabContainer.self.gameObject.AddComponent<AVOPreviewRenderer>();
            this.m_previewRenderer.size = this.m_preview.size * 2f;
            this.m_preview.texture = this.m_previewRenderer.texture;
            uIPanel.eventMouseDown += delegate (UIComponent c, UIMouseEventParameter p)
            {
                assetSelectionTabContainer.self.eventMouseMove += new MouseEventHandler(RotateCamera);
                redrawModel();
            };
            uIPanel.eventMouseUp += delegate (UIComponent c, UIMouseEventParameter p)
            {
                assetSelectionTabContainer.self.eventMouseMove -= new MouseEventHandler(RotateCamera);
                redrawModel();
            };
            uIPanel.eventMouseWheel += delegate (UIComponent c, UIMouseEventParameter p)
            {
                this.m_previewRenderer.zoom -= Mathf.Sign(p.wheelDelta) * 0.25f;
                redrawModel();
            };
        }

        private void M_defaultAssets_eventOnSelect(string idx)
        {
            m_lastInfo = PrefabCollection<VehicleInfo>.FindLoaded(idx);
            redrawModel();
        }

        private void RotateCamera(UIComponent c, UIMouseEventParameter p)
        {
            this.m_previewRenderer.cameraRotation -= p.moveDelta.x / this.m_preview.width * 360f;
            redrawModel();
        }


        private void redrawModel()
        {
            if (m_lastInfo == default(VehicleInfo))
            {
                return;
            }
            m_previewRenderer.RenderVehicle(m_lastInfo, m_defaultAssets.root.color, true);
        }

        private void InitPrefixDetailTab()
        {
            UIHelperExtension detailsTabContainer = createNewAsteriskTab(Locale.Get("TLM_DETAILS"));
            m_prefixName = detailsTabContainer.AddTextField(Locale.Get("TLM_PREFIX_NAME"), delegate (string s) { setPrefixNameDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), s); });
            m_ticketPrice = detailsTabContainer.AddTextField(Locale.Get("TLM_TICKET_PRICE_LABEL"), delegate (string s)
            {
                uint f = uint.Parse("0" + s);
                setTicketPriceDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(m_prefixSelection.selectedIndex - 1), f);
            });
            m_prefixName.GetComponentInParent<UIPanel>().width = 300;
            m_prefixName.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
            m_prefixName.GetComponentInParent<UIPanel>().autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            m_prefixName.GetComponentInParent<UIPanel>().wrapLayout = true;

            m_ticketPrice.numericalOnly = true;
            m_ticketPrice.maxLength = 7;

            foreach (Transform t in ((UIPanel)detailsTabContainer.self).transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }
        }

        private void InitSubTabstrip()
        {
            TLMUtils.doLog("INIT TLM_TABS");
            m_StripAsteriskTab = m_prefixSelectorPanel.self.AddUIComponent<UITabstrip>();
            m_StripAsteriskTab.width = 840;
            m_StripAsteriskTab.height = 50;

            m_StripAsteriskTab.tabPages = m_prefixSelectorPanel.self.AddUIComponent<UITabContainer>();

            m_StripAsteriskTab.tabPages.width = 840;
            m_StripAsteriskTab.tabPages.height = 630;
        }

        private void InitPrefixesSelector()
        {
            m_prefixSelectorPanel = new UIHelperExtension(m_PrefixEditor);
            ((UIScrollablePanel)m_prefixSelectorPanel.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)m_prefixSelectorPanel.self).autoLayoutPadding = new RectOffset(5, 5, 0, 0);
            ((UIScrollablePanel)m_prefixSelectorPanel.self).wrapLayout = true;
            ((UIScrollablePanel)m_prefixSelectorPanel.self).autoLayout = true;

            string[] optionList = new string[m_transportTypeOrder.Length + 1];
            optionList[0] = "--" + Locale.Get("SELECT") + "--";
            for (int i = 1; i < optionList.Length; i++)
            {
                optionList[i] = TLMCW.getNameForTransportType(m_transportTypeOrder[i - 1]);
            }

            TLMUtils.doLog("INIT m_systemTypeDropDown");
            m_systemTypeDropDown = (UIDropDown)m_prefixSelectorPanel.AddDropdown(Locale.Get("TLM_TRANSPORT_SYSTEM"), optionList, 0, loadPrefixes);
            m_prefixSelection = (UIDropDown)m_prefixSelectorPanel.AddDropdown(Locale.Get("TLM_PREFIX"), new string[] { "" }, 0, selectPrefixAction);

            foreach (Transform t in m_prefixSelectorPanel.self.transform)
            {
                var panel = t.gameObject.GetComponent<UIPanel>();
                if (panel)
                {
                    panel.width = 340;
                }
            }
        }

        private void selectPrefixAction(int sel)
        {
            m_isLoadingPrefixData = true;
            if (sel == 0 || m_systemTypeDropDown.selectedIndex == 0)
            {
                ((UIScrollablePanel)m_prefixSelectorPanel.self).autoLayout = false;
                m_StripAsteriskTab.tabPages.enabled = false;

                m_StripAsteriskTab.enabled = false;
                return;
            }
            m_prefixName.text = getPrefixNameFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1));
            var hourBudgetsSaved = getPrefixBudgetMultiplierFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1));
            m_chkPerHourBudget.isChecked = hourBudgetsSaved.Length == 8;
            m_chkSingleBudget.isChecked = hourBudgetsSaved.Length == 1;
            for (int i = 0; i < 8; i++)
            {
                m_hourBudgets[i] = hourBudgetsSaved[i % hourBudgetsSaved.Length];
            }
            updateBudgetSliders();
            m_ticketPrice.text = "" + (getTicketPriceFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(sel - 1)));
            reloadAssetsList(sel);
            m_StripAsteriskTab.tabPages.enabled = true;
            m_StripAsteriskTab.enabled = true;
            m_StripAsteriskTab.selectedIndex = 0;
            m_isLoadingPrefixData = false;
        }

        private void loadPrefixes(int sel)
        {
            if (sel == 0)
            {
                m_prefixSelection.isVisible = false;
                m_prefixSelection.selectedIndex = 0;
                m_StripAsteriskTab.tabPages.enabled = false;
                return;
            }
            m_prefixSelection.isVisible = true;
            m_StripAsteriskTab.tabPages.enabled = false;
            TLMConfigWarehouse.ConfigIndex transportIndex = getConfigIndexFromDropDownSelection(sel);
            m_defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex);
            m_defaultAssets.root.color = TLMConfigWarehouse.getColorForTransportType(transportIndex);
            var m = (ModoNomenclatura)TLMConfigWarehouse.getCurrentConfigInt(transportIndex | TLMConfigWarehouse.ConfigIndex.PREFIX);
            m_prefixSelection.items = TLMUtils.getStringOptionsForPrefix(m, true, transportIndex);
            m_prefixSelection.selectedIndex = 0;
        }

        private void reloadAssetsList(int idx)
        {
            m_prefixAssets.itemsList = getPrefixAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex, (uint)(idx - 1));
            var t = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex);
            m_defaultAssets.itemsList = getBasicAssetListFromDropDownSelection(m_systemTypeDropDown.selectedIndex).Where(k => !m_prefixAssets.itemsList.ContainsKey(k.Key)).ToDictionary(k => k.Key, k => k.Value);
            m_StripAsteriskTab.EnableTab(1);
        }


        private void updateBudgetSliders()
        {
            if (m_chkSingleBudget.isChecked)
            {
                m_budgetSliders[0].parent.GetComponentInChildren<UILabel>().prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL") + ":";
            }
            else
            {
                m_budgetSliders[0].parent.GetComponentInChildren<UILabel>().prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL", 0) + ":";
            }
            for (int i = 0; i < 8; i++)
            {
                m_budgetSliders[i].parent.isVisible = i == 0 || !m_chkSingleBudget.isChecked;
                m_budgetSliders[i].value = m_hourBudgets[i] / 100f;
                m_budgetSliders[i].transform.parent.GetComponentInChildren<UILabel>().text = string.Format(" x{0:0.00}", m_budgetSliders[i].value);
            }
        }

        #region Asset Selection & details functions

        private Dictionary<string, string> getBasicAssetListFromDropDownSelection(int index, bool global = false)
        {
            return TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).GetAllBasicAssets(0);

        }
        private Dictionary<string, string> getPrefixAssetListFromDropDownSelection(int index, uint prefix, bool global = false)
        {
            return TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).GetSelectedBasicAssets(prefix);
        }

        private void addAssetToPrefixDropDownSelection(int index, uint prefix, string assetId, bool global = false)
        {
            TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).AddAsset(prefix, assetId);
        }

        private void removeAssetFromPrefixDropDownSelection(int index, uint prefix, string assetId, bool global = false)
        {
            TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).RemoveAsset(prefix, assetId);
        }

        private void removeAllAssetsFromPrefixDropDownSelection(int index, uint prefix, bool global = false)
        {
            TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).UseDefaultAssets(prefix);
        }

        private void setPrefixNameDropDownSelection(int index, uint prefix, string name, bool global = false)
        {
            TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).SetName(prefix, name);
        }

        private void setBudgetMultiplierDropDownSelection(int index, uint prefix, bool global = false)
        {
            uint[] saveData;
            if (m_chkSingleBudget.isChecked)
            {
                saveData = new uint[] { m_hourBudgets[0] };
            }
            else
            {
                saveData = m_hourBudgets;
            }

            TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).SetBudgetMultiplier(prefix, saveData);
        }

        private void setTicketPriceDropDownSelection(int index, uint prefix, uint value)
        {
            TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).SetTicketPrice(prefix, value);
        }
        private string getPrefixNameFromDropDownSelection(int index, uint prefix)
        {
            return TLMLineUtils.getTransportSystemPrefixName(getConfigIndexFromDropDownSelection(index), prefix) ?? string.Empty;
        }
        private uint[] getPrefixBudgetMultiplierFromDropDownSelection(int index, uint prefix)
        {
            return TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).GetBudgetsMultiplier(prefix);
        }
        private uint getTicketPriceFromDropDownSelection(int index, uint prefix)
        {
            return TLMLineUtils.getExtensionFromConfigIndex(getConfigIndexFromDropDownSelection(index)).GetTicketPrice(prefix);
        }

        private TLMConfigWarehouse.ConfigIndex getConfigIndexFromDropDownSelection(int index)
        {
            if (index < 0 || index > m_transportTypeOrder.Length)
            {
                return TLMConfigWarehouse.ConfigIndex.NIL;
            }
            else
            {
                return m_transportTypeOrder[index - 1];
            }
        }
        #endregion


        private UIHelperExtension createNewAsteriskTab(string title)
        {
            formatTabButton(m_StripAsteriskTab.AddTab(title));
            UIHelperExtension newTab = new UIHelperExtension(m_StripAsteriskTab.tabContainer.components[m_StripAsteriskTab.tabContainer.components.Count - 1]);
            ((UIPanel)newTab.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)newTab.self).autoLayoutPadding = new RectOffset(2, 2, 0, 0);
            ((UIPanel)newTab.self).wrapLayout = true;
            ((UIPanel)newTab.self).autoSize = true;
            ((UIPanel)newTab.self).autoLayout = true;
            ((UIPanel)newTab.self).width = 680;
            ((UIPanel)newTab.self).isVisible = false;
            ((UIPanel)newTab.self).padding = new RectOffset(0, 0, 0, 0);
            return newTab;
        }

        private UISlider GenerateBudgetMultiplierField(UIHelperExtension uiHelper, string title, OnValueChanged action)
        {
            return GenerateBudgetMultiplierField(uiHelper, title, action, out UILabel label, out UIPanel panel);
        }

        private static UISlider GenerateBudgetMultiplierField(UIHelperExtension uiHelper, string title, OnValueChanged action, out UILabel label, out UIPanel panel)
        {
            UISlider budgetMultiplier = (UISlider)uiHelper.AddSlider(Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL"), 0f, 5, 0.05f, 1, action);
            label = budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>();
            label.prefix = title;
            label.autoSize = true;
            label.wordWrap = false;
            label.text = string.Format(" x{0:0.00}", 0);
            panel = budgetMultiplier.GetComponentInParent<UIPanel>();
            panel.width = 300;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;
            panel.autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            panel.wrapLayout = true;
            return budgetMultiplier;
        }
        private void formatTabButton(UIButton tabButton)
        {
            tabButton.textPadding = new RectOffset(10, 10, 5, 0);
            tabButton.autoSize = true;
            tabButton.normalBgSprite = "GenericTab";
            tabButton.focusedBgSprite = "GenericTabFocused";
            tabButton.hoveredBgSprite = "GenericTabHovered";
            tabButton.pressedBgSprite = "GenericTabPressed";
            tabButton.disabledBgSprite = "GenericTabDisabled";
        }

    }
}
