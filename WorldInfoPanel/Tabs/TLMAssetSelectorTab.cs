using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMAssetSelectorTab : UICustomControl, IUVMPTWIPChild
    {
        private UILabel m_title;
        private Color m_lastColor = Color.clear;
        public void Awake() => CreateWindow();

        public UIPanel MainPanel { get; private set; }

        private UIScrollablePanel m_scrollablePanel;
        private UIScrollbar m_scrollbar;
        private AVOPreviewRenderer m_previewRenderer;
        private UITextureSprite m_preview;
        private UIPanel m_previewPanel;
        private VehicleInfo m_lastInfo;
        private Dictionary<string, string> m_defaultAssets = new Dictionary<string, string>();
        private UITemplateList<UIPanel> m_checkboxTemplateList;
        private bool m_isLoading;
        private TransportSystemDefinition m_lastSystem = default;
        private TransportSystemDefinition TransportSystem => TransportSystemDefinition.From(GetLineID());
        internal static ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        private void CreateWindow()
        {
            CreateMainPanel();

            CreateScrollPanel();

            SetPreviewWindow();

            CreateRemoveUndesiredModelsButton();

            CreateAssetLineTemplate();
            CreateTemplateList();
        }
        private void CreateTemplateList() => m_checkboxTemplateList = new UITemplateList<UIPanel>(m_scrollablePanel, "K45_TLM_AssetSelectionTabLineTemplate");
        private void CreateAssetLineTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(m_scrollablePanel.width - 40f, 36);
            panel.autoLayout = true;
            panel.wrapLayout = false;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            UICheckBox uiCheckbox = UIHelperExtension.AddCheckbox(panel, "AAAAAA", false);
            uiCheckbox.name = "AssetCheckbox";
            uiCheckbox.height = 29f;
            uiCheckbox.width = 290f;
            uiCheckbox.label.processMarkup = true;
            uiCheckbox.label.textScale = 0.8f;

            KlyteMonoUtils.CreateUIElement(out UITextField capEditField, panel.transform, "Cap", new Vector4(0, 0, 50, 30));
            KlyteMonoUtils.UiTextFieldDefaults(capEditField);
            KlyteMonoUtils.InitButtonFull(capEditField, false, "OptionsDropboxListbox");
            capEditField.isTooltipLocalized = true;
            capEditField.tooltipLocaleID = "K45_TLM_ASSET_CAPACITY_FIELD_DESCRIPTION";
            capEditField.numericalOnly = true;
            capEditField.maxLength = 6;
            capEditField.padding = new RectOffset(2, 2, 4, 2);

            TLMUiTemplateUtils.GetTemplateDict()["K45_TLM_AssetSelectionTabLineTemplate"] = panel;
        }
        private void CreateRemoveUndesiredModelsButton()
        {
            KlyteMonoUtils.CreateUIElement<UIButton>(out UIButton removeUndesired, MainPanel.transform);
            removeUndesired.relativePosition = new Vector3(MainPanel.width - 25f, 0f);
            removeUndesired.textScale = 0.6f;
            removeUndesired.width = 20;
            removeUndesired.height = 20;
            removeUndesired.tooltip = Locale.Get("K45_TLM_REMOVE_UNWANTED_TOOLTIP");
            KlyteMonoUtils.InitButton(removeUndesired, true, "ButtonMenu");
            removeUndesired.name = "DeleteLineButton";
            removeUndesired.isVisible = true;
            removeUndesired.eventClick += (component, eventParam) =>
            {
                TLMVehicleUtils.RemoveAllUnwantedVehicles();
            };

            UISprite icon = removeUndesired.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.width = 18;
            icon.height = 18;
            icon.spriteName = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_RemoveUnwantedIcon);
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
            m_scrollablePanel.height = MainPanel.height - 180f;
            m_scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            m_scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_scrollablePanel.scrollPadding = new RectOffset(10, 10, 10, 10);
            m_scrollablePanel.autoLayout = true;
            m_scrollablePanel.clipChildren = true;
            m_scrollablePanel.relativePosition = new Vector3(5, 35);
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

        private void CreateModelCheckBox(UICheckBox checkbox)
        {
            checkbox.eventMouseEnter += (x, y) =>
            {
                m_lastInfo = PrefabCollection<VehicleInfo>.FindLoaded(x.objectUserData.ToString());
                RedrawModel();
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
            TLMUtils.DoLog("tsd = {0}", tsd);
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID());

            if (TransportSystem != m_lastSystem)
            {
                m_defaultAssets = tsd.GetTransportExtension().GetAllBasicAssetsForLine(0);
                UIPanel[] depotChecks = m_checkboxTemplateList.SetItemCount(m_defaultAssets.Count);

                TLMUtils.DoLog("m_defaultAssets Size = {0} ({1})", m_defaultAssets?.Count, string.Join(",", m_defaultAssets.Keys?.ToArray() ?? new string[0]));
                for (int i = 0; i < m_defaultAssets.Count; i++)
                {
                    string assetName = m_defaultAssets.Keys.ElementAt(i);
                    UICheckBox checkbox = depotChecks[i].GetComponentInChildren<UICheckBox>();
                    checkbox.objectUserData = assetName;
                    UITextField capacityEditor = depotChecks[i].GetComponentInChildren<UITextField>();
                    capacityEditor.text = VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(assetName)).ToString("0");
                    if (checkbox.label.objectUserData == null)
                    {
                        checkbox.eventCheckChanged += (x, y) =>
                          {
                              if (m_isLoading)
                              {
                                  return;
                              }

                              ushort lineId = GetLineID();
                              IBasicExtension extension = TLMLineUtils.GetEffectiveExtensionForLine(lineId);

                              TLMUtils.DoErrorLog($"checkbox event: {x.objectUserData} => {y} at {extension}[{lineId}]");
                              if (y)
                              {
                                  extension.AddAssetToLine(lineId, x.objectUserData.ToString());
                              }
                              else
                              {
                                  extension.RemoveAssetFromLine(lineId, x.objectUserData.ToString());
                              }
                          };
                        CreateModelCheckBox(checkbox);
                        KlyteMonoUtils.LimitWidthAndBox(checkbox.label, 280);
                        capacityEditor.eventTextSubmitted += CapacityEditor_eventTextSubmitted; ;

                        capacityEditor.eventMouseEnter += (x, y) =>
                        {
                            m_lastInfo = PrefabCollection<VehicleInfo>.FindLoaded(checkbox.objectUserData.ToString());
                            RedrawModel();
                        };
                        checkbox.label.objectUserData = true;
                    }
                    checkbox.text = m_defaultAssets[assetName];
                }
                m_lastSystem = TransportSystem;
            }
            else
            {
                List<string> allowedAssets = config.GetAssetListForLine(GetLineID());
                for (int i = 0; i < m_checkboxTemplateList.items.Count; i++)
                {
                    UICheckBox checkbox = m_checkboxTemplateList.items[i].GetComponentInChildren<UICheckBox>();
                    checkbox.isChecked = allowedAssets.Contains(checkbox.objectUserData.ToString());
                }
            }

            if (TransportLinesManagerMod.DebugMode)
            {
                List<string> allowedAssets = config.GetAssetListForLine(GetLineID());
                TLMUtils.DoLog($"selectedAssets Size = {allowedAssets?.Count} ({ string.Join(",", allowedAssets?.ToArray() ?? new string[0])}) {config?.GetType()}");
            }

            if (config is TLMTransportLineConfiguration)
            {
                m_title.text = string.Format(Locale.Get("K45_TLM_ASSET_SELECT_WINDOW_TITLE"), TLMLineUtils.GetLineStringId(GetLineID()));
            }
            else
            {
                int prefix = (int)TLMLineUtils.GetPrefix(GetLineID());
                m_title.text = string.Format(Locale.Get("K45_TLM_ASSET_SELECT_WINDOW_TITLE_PREFIX"), prefix > 0 ? NumberingUtils.GetStringFromNumber(TLMPrefixesUtils.GetStringOptionsForPrefix(tsd), prefix + 1) : Locale.Get("K45_TLM_UNPREFIXED"), TLMConfigWarehouse.getNameForTransportType(tsd.ToConfigIndex()));
            }

            m_isLoading = false;


        }

        private void CapacityEditor_eventTextSubmitted(UIComponent x, string y)
        {
            if (m_isLoading || !int.TryParse(y.IsNullOrWhiteSpace() ? "0" : y, out int value))
            {
                return;
            }
            var capacityEditor = x as UITextField;
            string assetName = x.parent.GetComponentInChildren<UICheckBox>().objectUserData.ToString();
            VehicleInfo info = PrefabCollection<VehicleInfo>.FindLoaded(assetName);
            TransportSystemDefinition.From(info).GetTransportExtension().SetVehicleCapacity(assetName, value);
            m_isLoading = true;
            capacityEditor.text = VehicleUtils.GetCapacity(info).ToString("0");
            m_isLoading = false;
        }

        private void SetPreviewWindow()
        {
            KlyteMonoUtils.CreateUIElement(out m_previewPanel, MainPanel.transform);
            m_previewPanel.backgroundSprite = "GenericPanel";
            m_previewPanel.width = MainPanel.width - 15;
            m_previewPanel.height = 140;
            m_previewPanel.relativePosition = new Vector3(7.5f, MainPanel.height - 142);
            KlyteMonoUtils.CreateUIElement(out m_preview, m_previewPanel.transform);
            m_preview.size = m_previewPanel.size;
            m_preview.relativePosition = Vector3.zero;
            KlyteMonoUtils.CreateElement(out m_previewRenderer, MainPanel.transform);
            m_previewRenderer.Size = m_preview.size * 2f;
            m_preview.texture = m_previewRenderer.Texture;
            m_previewRenderer.Zoom = 3;
            m_previewRenderer.CameraRotation = 40;
        }

        public void UpdateBindings()
        {
            if (m_lastInfo != default(VehicleInfo) && GetComponentInParent<UIComponent>().isVisible)
            {
                m_previewRenderer.CameraRotation -= 2;
                RedrawModel();
            }
        }

        private void RedrawModel()
        {
            if (m_lastInfo == default(VehicleInfo))
            {
                m_preview.isVisible = false;
                return;
            }
            m_preview.isVisible = true;
            m_previewRenderer.RenderVehicle(m_lastInfo, m_lastColor == Color.clear ? Color.HSVToRGB(Math.Abs(m_previewRenderer.CameraRotation) / 360f, .5f, .5f) : m_lastColor, true);
        }

        public void OnEnable()
        { }
        public void OnDisable()
        { }
        public void OnGotFocus()
        { }

        public bool MayBeVisible() => TransportSystem.HasVehicles();

        public void Hide() => MainPanel.isVisible = false;
    }
}
