using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
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
        private AVOPreviewRenderer m_previewRenderer;
        private UITextureSprite m_preview;
        private UIPanel m_previewPanel;
        private VehicleInfo m_lastInfo;
        private UITemplateList<UIPanel> m_checkboxTemplateList;
        private UITextField m_nameFilter;
        private TransportSystemDefinition TransportSystem => TransportSystemDefinition.From(GetLineID());
        internal static ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        private void CreateWindow()
        {
            CreateMainPanel();

            KlyteMonoUtils.CreateUIElement(out m_nameFilter, MainPanel.transform);
            KlyteMonoUtils.UiTextFieldDefaults(m_nameFilter);
            KlyteMonoUtils.InitButtonFull(m_nameFilter, false, "OptionsDropboxListbox");
            m_nameFilter.tooltipLocaleID = "K45_TLM_ASSET_FILTERBY";
            m_nameFilter.relativePosition = new Vector3(5, 35);
            m_nameFilter.height = 23;
            m_nameFilter.width = MainPanel.width - 10f;
            m_nameFilter.eventKeyUp += (x, y) => UpdateAssetList(TLMLineUtils.GetEffectiveExtensionForLine(GetLineID()));
            m_nameFilter.horizontalAlignment = UIHorizontalAlignment.Left;
            m_nameFilter.padding = new RectOffset(2, 2, 4, 2);

            CreateScrollPanel();

            SetPreviewWindow();

            CreateRemoveUndesiredModelsButton();

            CreateTemplateList();
        }

        private void CreateTemplateList()
        {
            TLMAssetItemLine.EnsureTemplate();
            m_checkboxTemplateList = new UITemplateList<UIPanel>(m_scrollablePanel, TLMAssetItemLine.TEMPLATE_NAME);
        }

        private void CreateRemoveUndesiredModelsButton()
        {
            KlyteMonoUtils.CreateUIElement(out UIButton removeUndesired, MainPanel.transform);
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
            KlyteMonoUtils.CreateScrollPanel(MainPanel, out m_scrollablePanel, out _, MainPanel.width - 25f, MainPanel.height - 205f, new Vector3(5, 60));
            m_scrollablePanel.backgroundSprite = "ScrollbarTrack";
            m_scrollablePanel.scrollPadding.top = 10;
            m_scrollablePanel.scrollPadding.bottom = 10;
            m_scrollablePanel.scrollPadding.left = 8;
            m_scrollablePanel.scrollPadding.right = 8;
            m_scrollablePanel.eventMouseLeave += (x, u) => m_lastInfo = default;
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
            LogUtils.DoLog("tsd = {0}", tsd);
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID());

            UpdateAssetList(config);


            if (config is TLMTransportLineConfiguration)
            {
                m_title.text = string.Format(Locale.Get("K45_TLM_ASSET_SELECT_WINDOW_TITLE"), TLMLineUtils.GetLineStringId(GetLineID()));
            }
            else
            {
                int prefix = (int)TLMPrefixesUtils.GetPrefix(GetLineID());
                m_title.text = string.Format(Locale.Get("K45_TLM_ASSET_SELECT_WINDOW_TITLE_PREFIX"), prefix > 0 ? NumberingUtils.GetStringFromNumber(TLMPrefixesUtils.GetStringOptionsForPrefix(tsd), prefix + 1) : Locale.Get("K45_TLM_UNPREFIXED"), tsd.GetTransportName());
            }
        }

        private void UpdateAssetList(IBasicExtension config)
        {
            m_lastInfo = default;
            var targetAssets = TransportSystem.GetTransportExtension().GetAllBasicAssetsForLine(0).Where(x => x.Value.Contains(m_nameFilter.text)).ToList();
            UIPanel[] depotChecks = m_checkboxTemplateList.SetItemCount(targetAssets.Count);
            List<string> allowedAssets = config.GetAssetListForLine(GetLineID());

            if (TransportLinesManagerMod.DebugMode)
            {
                LogUtils.DoLog($"selectedAssets Size = {allowedAssets?.Count} ({ string.Join(",", allowedAssets?.ToArray() ?? new string[0])}) {config?.GetType()}");
            }

            for (int i = 0; i < depotChecks.Length; i++)
            {
                string assetName = targetAssets[i].Key;
                var controller = depotChecks[i].GetComponent<TLMAssetItemLine>();
                controller.SetAsset(assetName, allowedAssets.Contains(assetName));
                controller.OnMouseEnter = () =>
                {
                    m_lastInfo = PrefabCollection<VehicleInfo>.FindLoaded(assetName);
                    RedrawModel();
                };
            }
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
            if (GetComponentInParent<UIComponent>().isVisible)
            {
                if (m_lastInfo is null)
                {
                    m_preview.isVisible = false;
                    return;
                }
                else
                {
                    m_preview.isVisible = true;
                    m_previewRenderer.CameraRotation -= 1;
                    RedrawModel();
                    
                }
            }
        }        

        private void RedrawModel() => m_previewRenderer.RenderVehicle(m_lastInfo, m_lastColor == Color.clear ? Color.HSVToRGB(Math.Abs(m_previewRenderer.CameraRotation) / 360f, .5f, .5f) : m_lastColor, true);
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
