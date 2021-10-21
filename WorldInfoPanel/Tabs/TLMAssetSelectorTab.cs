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
        private Dictionary<TransportSystemDefinition, string> m_clipboard = new Dictionary<TransportSystemDefinition, string>();

        private UIButton m_copyButton;
        private UIButton m_pasteButton;
        private UIButton m_eraseButton;


        private TransportSystemDefinition TransportSystem => UVMPublicTransportWorldInfoPanel.GetCurrentTSD();
        internal static ushort GetLineID()
        {
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out ushort buildingId);
            return buildingId == 0 ? lineId : (ushort)0;
        }

        private void CreateWindow()
        {
            CreateMainPanel();

            KlyteMonoUtils.CreateUIElement(out m_nameFilter, MainPanel.transform);
            KlyteMonoUtils.UiTextFieldDefaults(m_nameFilter);
            KlyteMonoUtils.InitButtonFull(m_nameFilter, false, "OptionsDropboxListbox");
            m_nameFilter.tooltipLocaleID = "K45_TLM_ASSET_FILTERBY";
            m_nameFilter.relativePosition = new Vector3(5, 50);
            m_nameFilter.height = 23;
            m_nameFilter.width = MainPanel.width - 10f;
            m_nameFilter.eventKeyUp += (x, y) => UpdateAssetList(TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem));
            m_nameFilter.eventTextSubmitted += (x, y) => UpdateAssetList(TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem));
            m_nameFilter.eventTextCancelled += (x, y) => UpdateAssetList(TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem));
            m_nameFilter.horizontalAlignment = UIHorizontalAlignment.Left;
            m_nameFilter.padding = new RectOffset(2, 2, 4, 2);

            CreateScrollPanel();

            SetPreviewWindow();

            CreateButtons();

            CreateTemplateList();
        }

        private void CreateTemplateList()
        {
            TLMAssetItemLine.EnsureTemplate();
            m_checkboxTemplateList = new UITemplateList<UIPanel>(m_scrollablePanel, TLMAssetItemLine.TEMPLATE_NAME);
        }

        private void CreateButtons()
        {
            var removeUndesired = ConfigureActionButton(MainPanel, CommonsSpriteNames.K45_RemoveUnwantedIcon);
            removeUndesired.eventClick += (component, eventParam) => TLMVehicleUtils.RemoveAllUnwantedVehicles();

            m_copyButton = ConfigureActionButton(MainPanel, CommonsSpriteNames.K45_Copy);
            m_copyButton.eventClick += (x, y) => ActionCopy();
            m_pasteButton = ConfigureActionButton(MainPanel, CommonsSpriteNames.K45_Paste);
            m_pasteButton.eventClick += (x, y) => ActionPaste();
            m_eraseButton = ConfigureActionButton(MainPanel, CommonsSpriteNames.K45_Delete);
            m_eraseButton.eventClick += (x, y) => ActionDelete();
            m_eraseButton.color = Color.red;

            removeUndesired.tooltip = Locale.Get("K45_TLM_REMOVE_UNWANTED_TOOLTIP");
            m_copyButton.tooltip = Locale.Get("K45_TLM_COPY_CURRENT_LIST_CLIPBOARD");
            m_pasteButton.tooltip = Locale.Get("K45_TLM_PASTE_CLIPBOARD_TO_CURRENT_LIST");
            m_eraseButton.tooltip = Locale.Get("K45_TLM_DELETE_CURRENT_LIST");

            removeUndesired.relativePosition = new Vector3(MainPanel.width - 50, 0f);
            m_copyButton.relativePosition = new Vector3(MainPanel.width - 50f, 25);
            m_pasteButton.relativePosition = new Vector3(MainPanel.width - 25f, 25);
            m_eraseButton.relativePosition = new Vector3(MainPanel.width - 25f, 0);
        }

        private void ActionCopy()
        {
            var lineId = GetLineID();
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem);
            var dataClipboard = XmlUtils.DefaultXmlSerialize(config.GetAssetListForLine(lineId).ToList());
            m_clipboard[TransportSystem] = dataClipboard;
            m_pasteButton.isVisible = true;
            UpdateAssetList(config);
        }
        private void ActionPaste()
        {
            if (!m_clipboard.ContainsKey(TransportSystem))
            {
                return;
            }
            var lineId = GetLineID();
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem);
            config.SetAssetListForLine(lineId, XmlUtils.DefaultXmlDeserialize<List<string>>(m_clipboard[TransportSystem]));
            UpdateAssetList(config);
        }
        private void ActionDelete()
        {
            var lineId = GetLineID();
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem);
            config.SetAssetListForLine(lineId, new List<string>());
            UpdateAssetList(config);
        }

        protected static UIButton ConfigureActionButton(UIComponent parent, CommonsSpriteNames spriteName)
        {
            KlyteMonoUtils.CreateUIElement(out UIButton actionButton, parent.transform, "Btn");
            KlyteMonoUtils.InitButton(actionButton, false, "OptionBase");
            actionButton.focusedBgSprite = "";
            actionButton.autoSize = false;
            actionButton.width = 20;
            actionButton.height = 20;
            actionButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            actionButton.normalFgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(spriteName);
            actionButton.canFocus = false;
            return actionButton;
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
            m_title.autoHeight = false;
            m_title.width = MainPanel.width - 55f;
            m_title.height = 45f;
            m_title.relativePosition = new Vector3(5, 10);
            m_title.textScale = 0.9f;
            m_title.localeID = "K45_TLM_ASSETS_FOR_PREFIX";
        }

        private void CreateScrollPanel()
        {
            KlyteMonoUtils.CreateScrollPanel(MainPanel, out m_scrollablePanel, out _, MainPanel.width - 25f, MainPanel.height - 220f, new Vector3(5, 75));
            m_scrollablePanel.backgroundSprite = "ScrollbarTrack";
            m_scrollablePanel.scrollPadding.top = 10;
            m_scrollablePanel.scrollPadding.bottom = 10;
            m_scrollablePanel.scrollPadding.left = 6;
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
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem);

            UpdateAssetList(config);

            if (GetLineID() == 0)
            {
                m_title.text = Locale.Get("K45_TLM_ASSET_SELECT_WINDOW_TITLE_OUTSIDECONNECTION");
            }
            else if (config is TLMTransportLineConfiguration)
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
            var lineId = GetLineID();
            m_lastInfo = default;
            m_pasteButton.isVisible = m_clipboard.ContainsKey(TransportSystem);
            var targetAssets = TransportSystem.GetTransportExtension().GetAllBasicAssetsForLine(lineId).Where(x => x.Value.Contains(m_nameFilter.text)).ToList();
            UIPanel[] assetsCheck = m_checkboxTemplateList.SetItemCount(targetAssets.Count);
            List<string> allowedAssets = config.GetAssetListForLine(lineId);

            if (TransportLinesManagerMod.DebugMode)
            {
                LogUtils.DoLog($"selectedAssets Size = {allowedAssets?.Count} ({ string.Join(",", allowedAssets?.ToArray() ?? new string[0])}) {config?.GetType()}");
            }

            for (int i = 0; i < assetsCheck.Length; i++)
            {
                string assetName = targetAssets[i].Key;
                var controller = assetsCheck[i].GetComponent<TLMAssetItemLine>();
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

        public bool MayBeVisible() => TransportSystem?.HasVehicles() ?? false;

        public void Hide() => MainPanel.isVisible = false;
    }
}
