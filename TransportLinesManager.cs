using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.MapDrawer;
using Klyte.TransportLinesManager.OptionsMenu;
using Klyte.TransportLinesManager.TextureAtlas;
using Klyte.TransportLinesManager.Utils;
using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("12.25.0.3")]
namespace Klyte.TransportLinesManager
{
    public class TransportLinesManagerMod : BasicIUserMod<TransportLinesManagerMod, TLMResourceLoader, TLMController, TLMCommonTextureAtlas, TLMPublicTransportManagementPanel>
    {

        public TransportLinesManagerMod() => Construct();


        public override string SimpleName => "Transport Lines Manager";
        public override string Description => "Allows to customize and manage your public transport systems.";
        public override bool UseGroup9 => false;

        public override void doErrorLog(string fmt, params object[] args) => TLMUtils.doErrorLog(fmt, args);

        public override void doLog(string fmt, params object[] args) => TLMUtils.doLog(fmt, args);

        public override void LoadSettings()
        {
        }

        public override void TopSettingsUI(UIHelperExtension helper) => TLMConfigOptions.instance.GenerateOptionsMenu(helper);

        internal void PopulateGroup9(UIHelperExtension helper) => CreateGroup9(helper);

        public override void Group9SettingsUI(UIHelperExtension group9)
        {
            group9.AddButton(Locale.Get("K45_TLM_DRAW_CITY_MAP"), TLMMapDrawer.drawCityMap);
            group9.AddButton("Open generated map folder", () => ColossalFramework.Utils.OpenInFileBrowser(TLMController.exportedMapsFolder));
        }

        private readonly SavedBool m_savedShowNearLinesInCityServicesWorldInfoPanel = new SavedBool("showNearLinesInCityServicesWorldInfoPanel", Settings.gameSettingsFile, true, true);
        private readonly SavedBool m_savedShowNearLinesInZonedBuildingWorldInfoPanel = new SavedBool("showNearLinesInZonedBuildingWorldInfoPanel", Settings.gameSettingsFile, false, true);
        private readonly SavedBool m_savedOverrideDefaultLineInfoPanel = new SavedBool("TLMOverrideDefaultLineInfoPanel", Settings.gameSettingsFile, true, true);
        private readonly SavedBool m_showDistanceInLinearMap = new SavedBool("TLMshowDistanceInLinearMap", Settings.gameSettingsFile, true, true);

        public static bool showNearLinesPlop
        {
            get => instance.m_savedShowNearLinesInCityServicesWorldInfoPanel.value;
            set => instance.m_savedShowNearLinesInCityServicesWorldInfoPanel.value = value;
        }
        public static bool showNearLinesGrow
        {
            get => instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value;
            set => instance.m_savedShowNearLinesInZonedBuildingWorldInfoPanel.value = value;
        }
        public static bool overrideWorldInfoPanelLine
        {
            get => instance.m_savedOverrideDefaultLineInfoPanel.value;
            set => instance.m_savedOverrideDefaultLineInfoPanel.value = value;
        }
        public static bool showDistanceLinearMap
        {
            get => instance.m_showDistanceInLinearMap.value;
            set => instance.m_showDistanceInLinearMap.value = value;
        }



        public static SavedFloat ButtonPosX { get; } = new SavedFloat("K45_ButtonPosX", Settings.gameSettingsFile, 300, true);
        public static SavedFloat ButtonPosY { get; } = new SavedFloat("K45_ButtonPosY", Settings.gameSettingsFile, 20, true);


        private UIButton m_modPanelButton;
        private UITabstrip m_modsTabstrip;
        private UIPanel m_modsPanel;


        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            m_modsPanel = UIView.Find<UIPanel>("K45_ModsPanel");
            if (m_modsPanel == null)
            {
                UIComponent uicomponent = UIView.Find("TSBar");
                UIPanel bg = uicomponent.AddUIComponent<UIPanel>();
                bg.name = "K45_MB";
                bg.absolutePosition = new Vector2(ButtonPosX.value, ButtonPosY.value);
                bg.width = 60f;
                bg.height = 60f;
                bg.zOrder = 1;
                UIButton doneButton = bg.AddUIComponent<UIButton>();
                doneButton.normalBgSprite = "GenericPanel";
                doneButton.width = 100f;
                doneButton.height = 50f;
                doneButton.relativePosition = new Vector2(-40f, 70f);
                doneButton.text = "Done";
                doneButton.hoveredTextColor = new Color32(0, byte.MaxValue, byte.MaxValue, 1);
                doneButton.Hide();
                doneButton.zOrder = 99;
                UIDragHandle handle = bg.AddUIComponent<UIDragHandle>();
                handle.name = "K45_DragHandle";
                handle.relativePosition = Vector2.zero;
                handle.width = bg.width - 5f;
                handle.height = bg.height - 5f;
                handle.zOrder = 0;
                handle.target = bg;
                handle.Start();
                handle.enabled = false;
                bg.zOrder = 9;

                bg.isInteractive = false;
                handle.zOrder = 10;
                doneButton.eventClick += (component, ms) =>
                {
                    doneButton.Hide();
                    handle.zOrder = 10;
                    handle.enabled = false;
                    ButtonPosX.value = (int) bg.absolutePosition.x;
                    ButtonPosY.value = (int) bg.absolutePosition.y;
                };
                bg.color = new Color32(96, 96, 96, byte.MaxValue);
                m_modPanelButton = bg.AddUIComponent<UIButton>();
                m_modPanelButton.disabledTextColor = new Color32(128, 128, 128, byte.MaxValue);
                TLMUtils.initButton(m_modPanelButton, false, CommonTextureAtlas.instance.SpriteNames[0], false);
                m_modPanelButton.atlas = CommonTextureAtlas.instance.atlas;
                m_modPanelButton.relativePosition = new Vector3(5f, 0f);
                m_modPanelButton.size = new Vector2(64, 64);
                m_modPanelButton.name = "K45_ModsButton";
                m_modPanelButton.zOrder = 11;
                m_modPanelButton.textScale = 1.3f;
                m_modPanelButton.textVerticalAlignment = UIVerticalAlignment.Middle;
                m_modPanelButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
                m_modPanelButton.eventDoubleClick += (component, ms) =>
                {
                    handle.zOrder = 13;
                    doneButton.Show();
                    handle.enabled = true;
                };

                m_modsPanel = bg.AddUIComponent<UIPanel>();
                m_modsPanel.name = "K45_ModsPanel";
                m_modsPanel.size = new Vector2(875, 550);
                m_modsPanel.relativePosition = new Vector3(0f, 7f);
                m_modsPanel.isInteractive = false;
                m_modsPanel.Hide();

                m_modPanelButton.eventClicked += TogglePanel;

                CreateTabsComponent(out m_modsTabstrip, out _, m_modsPanel.transform, "K45", new Vector4(74, 0, m_modsPanel.width - 84, 40), new Vector4(64, 40, m_modsPanel.width - 64, m_modsPanel.height));
            }
            else
            {
                m_modPanelButton = UIView.Find<UIButton>("K45_ModsButton");
                m_modsTabstrip = UIView.Find<UITabstrip>("K45_Tabstrip");
            }

            AddTab();
        }
        public static void CreateTabsComponent(out UITabstrip tabstrip, out UITabContainer tabContainer, Transform parent, string namePrefix, Vector4 areaTabstrip, Vector4 areaContainer)
        {
            TLMUtils.createUIElement(out tabstrip, parent, $"{namePrefix}_Tabstrip", areaTabstrip);

            TLMUtils.createUIElement(out tabContainer, parent, $"{namePrefix}_TabContainer", areaContainer);
            tabstrip.tabPages = tabContainer;
            tabstrip.selectedIndex = 0;
            tabstrip.selectedIndex = -1;
        }
        private void TogglePanel(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_modsPanel == null)
            {
                return;
            }

            m_modsPanel.isVisible = !m_modsPanel.isVisible;
            if (m_modsPanel.isVisible)
            {
                m_modPanelButton?.Focus();
            }
            else
            {
                m_modPanelButton?.Unfocus();
            }
        }


        internal void AddTab()
        {
            if (m_modsTabstrip.Find<UIComponent>("TLM") != null)
            {
                return;
            }

            UIButton superTab = CreateTabTemplate();
            superTab.normalFgSprite = TLMCommonTextureAtlas.instance.SpriteNames[1];
            superTab.atlas = TLMCommonTextureAtlas.instance.atlas;
            superTab.color = Color.gray;
            superTab.focusedColor = Color.white;
            superTab.hoveredColor = Color.white;
            superTab.disabledColor = Color.black;
            superTab.playAudioEvents = true;
            superTab.tooltip = TransportLinesManagerMod.instance.Name;
            superTab.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;

            TLMUtils.createUIElement(out UIPanel content, null);
            content.name = "Container";
            content.area = new Vector4(0, 0, m_modsPanel.width + 70, m_modsPanel.height);

            UIComponent component = m_modsTabstrip.AddTab("TLM", superTab.gameObject, content.gameObject, typeof(TLMPublicTransportManagementPanel));

            content.eventVisibilityChanged += (x, y) => { if (y) { TransportLinesManagerMod.instance.showVersionInfoPopup(); } };


        }
        private static UIButton CreateTabTemplate()
        {
            TLMUtils.createUIElement(out UIButton tabTemplate, null, "KCTabTemplate");
            TLMUtils.initButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }
        public void ClosePanel()
        {
            if (m_modsPanel == null)
            {
                return;
            }

            m_modsPanel.isVisible = false;
            m_modPanelButton?.Unfocus();

        }

        public void OpenPanel()
        {
            if (m_modsPanel == null)
            {
                return;
            }

            m_modsPanel.isVisible = true;
            m_modPanelButton?.Focus();
        }

        public void OpenPanelAtModTab()
        {
            OpenPanel();
            m_modsTabstrip.ShowTab("TLM");
        }


    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
