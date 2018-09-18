using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMAssetSelectorWindowPrefixTab<T> : MonoBehaviour where T : TLMSysDef<T>
    {
        private TLMTabControllerPrefixList<T> m_parent => TLMTabControllerPrefixList<T>.instance;
        private UIPanel m_mainPanel;
        private UIHelperExtension m_uiHelper;
        private UILabel m_title;
        private Color m_lastColor = Color.clear;
        public void Awake()
        {
            CreateWindow();
        }

        public UIPanel mainPanel => m_mainPanel;

        private UIScrollablePanel m_scrollablePanel;
        private UIScrollbar m_scrollbar;
        private AVOPreviewRenderer m_previewRenderer;
        private UITextureSprite m_preview;
        private UIPanel m_previewPanel;
        private VehicleInfo m_lastInfo;
        private Dictionary<string, string> m_defaultAssets = new Dictionary<string, string>();
        private Dictionary<string, UICheckBox> m_checkboxes = new Dictionary<string, UICheckBox>();
        private bool m_isLoading;
        private ITLMTransportTypeExtension extension => Singleton<T>.instance.GetTSD().GetTransportExtension();
        private int m_prefixIdx => m_parent.SelectedPrefix;
        private TransportSystemDefinition m_system => Singleton<T>.instance.GetTSD();
        private bool loaded = false;

        private void CreateWindow()
        {
            CreateMainPanel();

            CreateScrollPanel();

            SetPreviewWindow();

            CreateRemoveUndesiredModelsButton();

            CreateCheckboxes();
            BindParentChanges();
        }

        private void CreateCheckboxes()
        {
            foreach (var i in m_checkboxes?.Keys)
            {
                UnityEngine.Object.Destroy(m_checkboxes[i].gameObject);
            }
            m_defaultAssets = extension.GetAllBasicAssets(0);
            m_checkboxes = new Dictionary<string, UICheckBox>();

            TLMUtils.doLog("m_defaultAssets Size = {0} ({1})", m_defaultAssets?.Count, string.Join(",", m_defaultAssets.Keys?.ToArray() ?? new string[0]));
            foreach (var i in m_defaultAssets.Keys)
            {
                var checkbox = (UICheckBox)m_uiHelper.AddCheckbox(m_defaultAssets[i], false, (x) =>
                {
                    if (m_isLoading || m_prefixIdx < 0) return;
                    if (x)
                    {
                        extension.AddAsset((uint)m_prefixIdx, i);
                    }
                    else
                    {
                        extension.RemoveAsset((uint)m_prefixIdx, i);
                    }
                });
                CreateModelCheckBox(i, checkbox);
                checkbox.label.tooltip = checkbox.label.text;
                checkbox.label.textScale = 0.9f;
                checkbox.label.transform.localScale = new Vector3(Math.Min((m_mainPanel.width - 70) / checkbox.label.width, 1), 1);
                m_checkboxes[i] = checkbox;
            }
        }

        private void CreateRemoveUndesiredModelsButton()
        {
            TLMUtils.createUIElement<UIButton>(out UIButton removeUndesired, m_mainPanel.transform);
            removeUndesired.relativePosition = new Vector3(m_mainPanel.width - 25f, 0f);
            removeUndesired.textScale = 0.6f;
            removeUndesired.width = 20;
            removeUndesired.height = 20;
            removeUndesired.tooltip = Locale.Get("TLM_REMOVE_UNWANTED_TOOLTIP");
            TLMUtils.initButton(removeUndesired, true, "ButtonMenu");
            removeUndesired.name = "DeleteLineButton";
            removeUndesired.isVisible = true;
            removeUndesired.eventClick += (component, eventParam) =>
            {
                TLMTransportExtensionUtils.RemoveAllUnwantedVehicles();
            };

            var icon = removeUndesired.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 18;
            icon.height = 18;
            icon.spriteName = "RemoveUnwantedIcon";
        }

        private void CreateMainPanel()
        {
            TLMUtils.createUIElement(out m_mainPanel, m_parent.transform);
            m_mainPanel.Hide();
            m_mainPanel.relativePosition = new Vector3(510f, 0.0f);
            m_mainPanel.width = 350;
            m_mainPanel.height = m_parent.GetComponent<UIComponent>().height;
            m_mainPanel.zOrder = 50;
            m_mainPanel.color = new Color32(255, 255, 255, 255);
            m_mainPanel.name = "AssetSelectorWindow";
            m_mainPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            m_mainPanel.autoLayout = false;
            m_mainPanel.useCenter = true;
            m_mainPanel.wrapLayout = false;
            m_mainPanel.canFocus = true;

            TLMUtils.createUIElement(out m_title, m_mainPanel.transform);
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.autoSize = false;
            m_title.autoHeight = true;
            m_title.width = m_mainPanel.width - 30f;
            m_title.relativePosition = new Vector3(5, 5);
            m_title.textScale = 0.9f;
            m_title.localeID = "TLM_ASSETS_FOR_PREFIX";
        }

        private void CreateScrollPanel()
        {
            TLMUtils.createUIElement(out m_scrollablePanel, m_mainPanel.transform);
            m_scrollablePanel.width = m_mainPanel.width - 20f;
            m_scrollablePanel.height = m_mainPanel.height - 180f;
            m_scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            m_scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_scrollablePanel.scrollPadding = new RectOffset(10, 10, 10, 10);
            m_scrollablePanel.autoLayout = true;
            m_scrollablePanel.clipChildren = true;
            m_scrollablePanel.relativePosition = new Vector3(5, 20);
            m_scrollablePanel.backgroundSprite = "ScrollbarTrack";

            TLMUtils.createUIElement(out UIPanel trackballPanel, m_mainPanel.transform);
            trackballPanel.width = 10f;
            trackballPanel.height = m_scrollablePanel.height;
            trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
            trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            trackballPanel.autoLayout = true;
            trackballPanel.relativePosition = new Vector3(m_mainPanel.width - 15, m_scrollablePanel.relativePosition.y);


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
        }

        private void CreateModelCheckBox(string i, UICheckBox checkbox)
        {
            checkbox.eventMouseEnter += (x, y) =>
            {
                m_lastInfo = PrefabCollection<VehicleInfo>.FindLoaded(i);
                redrawModel();
            };

        }
        private void BindParentChanges()
        {
            TLMTabControllerPrefixList<T>.eventOnPrefixChange += (prefix) =>
            {
                TLMUtils.doLog("EventOnLineChanged");
                TransportSystemDefinition tsd = Singleton<T>.instance.GetTSD();
                if (!tsd.hasVehicles())
                {
                    m_mainPanel.isVisible = false;
                    return;
                }
                m_isLoading = true;
                TLMUtils.doLog("tsd = {0}", tsd);
                if (!loaded)
                {
                    foreach (var i in m_checkboxes.Keys)
                    {
                        UnityEngine.Object.Destroy(m_checkboxes[i].gameObject);
                    }
                    m_defaultAssets = tsd.GetTransportExtension().GetAllBasicAssets(0);
                    m_checkboxes = new Dictionary<string, UICheckBox>();

                    TLMUtils.doLog("m_defaultAssets Size = {0} ({1})", m_defaultAssets?.Count, string.Join(",", m_defaultAssets.Keys?.ToArray() ?? new string[0]));
                    foreach (var i in m_defaultAssets.Keys)
                    {
                        var checkbox = (UICheckBox)m_uiHelper.AddCheckbox(m_defaultAssets[i], false, (x) =>
                        {
                            if (!m_isLoading)
                            {
                                if (x)
                                {
                                    tsd.GetTransportExtension().AddAsset((ushort)m_prefixIdx, i);

                                }
                                else
                                {
                                    tsd.GetTransportExtension().RemoveAsset((ushort)m_prefixIdx, i);

                                }
                            }
                        });
                        CreateModelCheckBox(i, checkbox);
                        checkbox.label.tooltip = checkbox.label.text;
                        checkbox.label.textScale = 0.9f;
                        checkbox.label.transform.localScale = new Vector3(Math.Min((m_mainPanel.width - 50) / checkbox.label.width, 1), 1);
                        m_checkboxes[i] = checkbox;
                    }
                }
                loaded = true;
                List<string> selectedAssets;
                selectedAssets = tsd.GetTransportExtension().GetAssetList((uint)prefix);

                TLMUtils.doLog("selectedAssets Size = {0} ({1})", selectedAssets?.Count, string.Join(",", selectedAssets?.ToArray() ?? new string[0]));
                foreach (var i in m_checkboxes.Keys)
                {
                    m_checkboxes[i].isChecked = selectedAssets.Contains(i);
                }

                TLMConfigWarehouse.ConfigIndex transportType = tsd.toConfigIndex();
                m_title.text = string.Format(Locale.Get("TLM_ASSET_SELECT_WINDOW_TITLE_PREFIX"), prefix > 0 ? TLMUtils.getStringFromNumber(TLMUtils.getStringOptionsForPrefix(transportType), (int)prefix + 1) : Locale.Get("TLM_UNPREFIXED"), TLMConfigWarehouse.getNameForTransportType(tsd.toConfigIndex()));

                m_isLoading = false;
            };
            TLMTabControllerPrefixList<T>.eventOnColorChange += (Color x) => m_lastColor = x;
        }

        private void SetPreviewWindow()
        {
            TLMUtils.createUIElement(out m_previewPanel, m_mainPanel.transform);
            m_previewPanel.backgroundSprite = "GenericPanel";
            m_previewPanel.width = m_mainPanel.width - 15;
            m_previewPanel.height = 140;
            m_previewPanel.relativePosition = new Vector3(7.5f, m_mainPanel.height - 150);
            TLMUtils.createUIElement(out m_preview, m_previewPanel.transform);
            this.m_preview.size = m_previewPanel.size;
            this.m_preview.relativePosition = Vector3.zero;
            TLMUtils.createElement(out m_previewRenderer, m_mainPanel.transform);
            this.m_previewRenderer.size = this.m_preview.size * 2f;
            this.m_preview.texture = this.m_previewRenderer.texture;
            m_previewRenderer.zoom = 3;
            m_previewRenderer.cameraRotation = 40;
        }

        private void Update()
        {
            if (m_lastInfo != default(VehicleInfo) && m_parent.GetComponent<UIComponent>().isVisible)
            {
                this.m_previewRenderer.cameraRotation -= 2;
                redrawModel();
            }
        }

        private void redrawModel()
        {
            if (m_lastInfo == default(VehicleInfo))
            {
                return;
            }
            m_previewRenderer.RenderVehicle(m_lastInfo, m_lastColor == Color.clear ? Color.HSVToRGB(Math.Abs(m_previewRenderer.cameraRotation) / 360f, .5f, .5f) : m_lastColor, true);
        }
    }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorBus : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorBus> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorTrm : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorTrm> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorMnr : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorMnr> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorMet : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorMet> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorTrn : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorTrn> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorFer : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorFer> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorBlp : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorBlp> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorShp : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorShp> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabNorPln : TLMAssetSelectorWindowPrefixTab<TLMSysDefNorPln> { }
    internal sealed class TLMAssetSelectorWindowPrefixTabTouBus : TLMAssetSelectorWindowPrefixTab<TLMSysDefTouBus> { }
}
