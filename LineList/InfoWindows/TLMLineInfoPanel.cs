using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.LineList.ExtraUI;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.LineList
{
    internal class TLMLineInfoPanel : LinearMapParentInterface<TLMLineInfoPanel>
    {
        private TLMAgesChartPanel m_agesPanel;
        private TLMController m_controller => TLMController.instance;
        private TLMLinearMap m_linearMap;
        private int m_lastStopsCount = 0;

        //line info	
        private UIPanel m_lineInfoPanel;
        private InstanceID m_lineIdSelecionado;
        private CameraController m_CameraController;
        private string m_lastLineName;
        private UILabel m_lineLenghtLabel;
        private UILabel m_budgetLabel;
        private UITextField m_lineNumberLabel;
        private UIDropDown m_linePrefixDropDown;
        private UILabel m_lineTransportIconTypeLabel;
        private UILabel m_viagensEvitadasLabel;
        private UILabel m_passageirosEturistasLabel;
        private UILabel m_veiculosLinhaLabel;
        private UILabel m_autoNameLabel;
        private UITextField m_lineNameField;
        private UIColorField m_lineColorPicker;

        private UIHelperExtension m_uiHelper;
        private UICheckBox m_DayLine;
        private UICheckBox m_DayNightLine;
        private UICheckBox m_NightLine;
        private UICheckBox m_DisabledLine;
        private UICheckBox m_IgnorePrefix;

        private UISlider[] m_budgetSliders = new UISlider[8];
        private UIButton m_enableBudgetPerHour;
        private UIButton m_disableBudgetPerHour;
        private UIButton m_goToWorldInfoPanel;
        private UIButton m_absoluteCountMode;
        private UIButton m_multiplierMode;
        private UILabel m_lineBudgetSlidersTitle;

        private UIDropDown m_firstStopSelect;
        private UITextField m_ticketPriceEditor;

        private TLMAssetSelectorWindow m_assetSelectorWindow;
        public TLMAssetSelectorWindow assetSelectorWindow => m_assetSelectorWindow;

        #region Getters
        public UILabel autoNameLabel
        {
            get {
                return m_autoNameLabel;
            }
        }

        public override Transform TransformLinearMap
        {
            get {
                return m_lineInfoPanel.transform;
            }
        }

        public bool isVisible
        {
            get {
                return m_lineInfoPanel.isVisible;
            }
        }
        public UIPanel mainPanel
        {
            get {
                return m_lineInfoPanel;
            }
        }

        internal TLMController controller
        {
            get {
                return m_controller;
            }
        }

        public TLMLinearMap linearMap
        {
            get {
                return m_linearMap;
            }
        }

        public InstanceID lineIdSelecionado
        {
            get {
                return m_lineIdSelecionado;
            }
        }

        public CameraController cameraController
        {
            get {
                return m_CameraController;
            }
        }

        public override ushort CurrentSelectedId
        {
            get {
                return m_lineIdSelecionado.TransportLine;
            }
        }

        public override bool CanSwitchView
        {
            get {
                return true;
            }
        }

        public override bool ForceShowStopsDistances
        {
            get {
                return false;
            }
        }


        public override TransportInfo CurrentTransportInfo
        {
            get {
                return Singleton<TransportManager>.instance.m_lines.m_buffer[CurrentSelectedId].Info;
            }
        }
        #endregion

        #region Instantiation
        public void Awake()
        {
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
                m_CameraController = gameObject.GetComponent<CameraController>();
            }
            createInfoView();
        }

        private void createInfoView()
        {

            //line info painel
            createMainPanel();

            createTitleBarItems();

            createLineInfoLabels();

            createRightLineActionButtons();

            createDayNightBudgetControls();

            CreateBudgetSliders();

            CreateFirstStopSelector();

            CreateIgnorePrefixBudgetOption();

            CreateTicketPriceEditor();

            TLMUtils.createElement(out m_agesPanel, m_lineInfoPanel.transform);
            TLMUtils.createElement(out m_linearMap, transform);
            m_linearMap.parent = this;
            TLMUtils.createElement(out m_assetSelectorWindow, transform);
            m_assetSelectorWindow.lineInfo = this;
        }

        private void CreateIgnorePrefixBudgetOption()
        {
            m_IgnorePrefix = m_uiHelper.AddCheckboxLocale("TLM_IGNORE_PREFIX_BUDGETING", false);
            m_IgnorePrefix.relativePosition = new Vector3(5f, 300f);
            m_IgnorePrefix.eventCheckChanged += delegate (UIComponent comp, bool value)
            {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0)
                {
                    TLMTransportLineExtension.instance.SetUseCustomConfig(m_lineIdSelecionado.TransportLine, value);
                    m_linearMap.setLineNumberCircle(m_lineIdSelecionado.TransportLine);
                    updateSliders();
                    UpdateTicketPrice();
                    EventOnLineChanged(m_lineIdSelecionado.TransportLine);
                }
            };
            m_IgnorePrefix.label.textScale = 0.9f;
        }

        private void CreateFirstStopSelector()
        {
            m_firstStopSelect = m_uiHelper.AddDropdownLocalized("TLM_FIRST_STOP_DD_LABEL", new String[1], 0, ChangeFirstStop);
            m_firstStopSelect.eventMouseWheel -= null;

            UIPanel parent = m_firstStopSelect.GetComponentInParent<UIPanel>();
            parent.autoFitChildrenHorizontally = false;
            parent.relativePosition = new Vector3(5, 330);
            m_firstStopSelect.width = 435;
            UILabel label = parent.GetComponentInChildren<UILabel>();
            label.autoSize = false;
            label.width = 200;
            label.height = 40;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Center;
            parent.autoLayoutDirection = LayoutDirection.Horizontal;
            parent.autoLayout = true;
            parent.ResetLayout(false, true);
        }

        private void CreateTicketPriceEditor()
        {
            m_ticketPriceEditor = m_uiHelper.AddTextField("-", SetTicketPrice);

            UIPanel parent = m_ticketPriceEditor.GetComponentInParent<UIPanel>();
            parent.autoFitChildrenHorizontally = false;
            parent.autoLayout = false;
            parent.relativePosition = new Vector3(5, 265);
            parent.height = 40;
            m_ticketPriceEditor.width = 200;
            UILabel label = parent.GetComponentInChildren<UILabel>();
            label.autoSize = true;
            label.anchor = UIAnchorStyle.None;
            label.autoHeight = false;
            label.transform.localScale = new Vector3(Math.Min(300f / label.width, 1), 1);
            label.height = 40;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Center;
            label.localeID = "TLM_TICKET_PRICE_LABEL";
            label.eventSizeChanged += (UIComponent component, Vector2 value) => { label.transform.localScale = new Vector3(Math.Min(300f / label.width, 1), 1); };
            label.relativePosition = new Vector3(0, 5);
            m_ticketPriceEditor.relativePosition = new Vector3(330, 0);
            m_ticketPriceEditor.numericalOnly = true;
            m_ticketPriceEditor.maxLength = 6;
            m_ticketPriceEditor.horizontalAlignment = UIHorizontalAlignment.Right;
            m_ticketPriceEditor.width = 80;
        }


        private void CreateBudgetSliders()
        {
            TLMUtils.createUIElement(out m_lineBudgetSlidersTitle, m_lineInfoPanel.transform);
            m_lineBudgetSlidersTitle.autoSize = false;
            m_lineBudgetSlidersTitle.relativePosition = new Vector3(15f, 100f);
            m_lineBudgetSlidersTitle.width = 400f;
            m_lineBudgetSlidersTitle.height = 36f;
            m_lineBudgetSlidersTitle.textScale = 0.9f;
            m_lineBudgetSlidersTitle.textAlignment = UIHorizontalAlignment.Center;
            m_lineBudgetSlidersTitle.name = "LineBudgetSlidersTitle";
            m_lineBudgetSlidersTitle.font = UIHelperExtension.defaultFontCheckbox;
            m_lineBudgetSlidersTitle.wordWrap = true;

            for (int i = 0; i < m_budgetSliders.Length; i++)
            {
                m_budgetSliders[i] = GenerateVerticalBudgetMultiplierField(m_uiHelper, i);
            }
        }

        #region Day & Night Controls creation
        private void createDayNightBudgetControls()
        {
            DayNightInstantiateCheckBoxes();

            DayNightCreateActions();

            DayNightSetGroup();

            DayNightSetPosition();

        }

        private void DayNightSetPosition()
        {
            m_DisabledLine.relativePosition = new Vector3(450f, 200f);
            m_DayLine.relativePosition = new Vector3(450f, 220f);
            m_NightLine.relativePosition = new Vector3(450f, 240f);
            m_DayNightLine.relativePosition = new Vector3(450f, 260f);
        }

        private void DayNightSetGroup()
        {
            m_DisabledLine.group = m_DisabledLine.parent;
            m_DayLine.group = m_DisabledLine.parent;
            m_NightLine.group = m_DisabledLine.parent;
            m_DayNightLine.group = m_DisabledLine.parent;
        }

        private void DayNightCreateActions()
        {
            m_DayLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine], true, false);
                        m_linearMap.redrawLine();
                    });
                }
            };
            m_NightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine], false, true);
                        m_linearMap.redrawLine();
                    });
                }
            };
            m_DayNightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine], true, true);
                        m_linearMap.redrawLine();
                    });
                }
            };
            m_DisabledLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine], false, false);
                        m_linearMap.redrawLine();
                    });
                }
            };
        }

        private void DayNightInstantiateCheckBoxes()
        {
            m_DayLine = m_uiHelper.AddCheckboxLocale("TRANSPORT_LINE_DAY", false);
            m_NightLine = m_uiHelper.AddCheckboxLocale("TRANSPORT_LINE_NIGHT", false);
            m_DayNightLine = m_uiHelper.AddCheckboxLocale("TRANSPORT_LINE_DAYNNIGHT", false);
            m_DisabledLine = m_uiHelper.AddCheckboxLocale("TLM_TRANSPORT_LINE_DISABLED", false);
        }

        #endregion

        private void createRightLineActionButtons()
        {
            TLMUtils.createUIElement(out m_autoNameLabel, m_lineInfoPanel.transform);
            m_autoNameLabel.autoSize = false;
            m_autoNameLabel.relativePosition = new Vector3(400f, 120f);
            m_autoNameLabel.textAlignment = UIHorizontalAlignment.Left;
            m_autoNameLabel.prefix = Locale.Get("TLM_GENERATED_AUTO_NAME") + ": ";
            m_autoNameLabel.width = 240;
            m_autoNameLabel.height = 100;
            m_autoNameLabel.name = "AutoNameLabel";
            m_autoNameLabel.textScale = 0.5f;
            m_autoNameLabel.wordWrap = true;
            m_autoNameLabel.clipChildren = false;
            m_autoNameLabel.textAlignment = UIHorizontalAlignment.Right;
            m_autoNameLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out UIButton deleteLine, m_lineInfoPanel.transform);
            deleteLine.relativePosition = new Vector3(m_lineInfoPanel.width - 150f, m_lineInfoPanel.height - 230f);
            deleteLine.textScale = 0.6f;
            deleteLine.width = 40;
            deleteLine.height = 40;
            deleteLine.tooltip = Locale.Get("LINE_DELETE");
            TLMUtils.initButton(deleteLine, true, "ButtonMenu");
            deleteLine.name = "DeleteLineButton";
            deleteLine.isVisible = true;
            deleteLine.eventClick += (component, eventParam) =>
            {
                if (m_lineIdSelecionado.TransportLine != 0)
                {
                    ConfirmPanel.ShowModal("CONFIRM_LINEDELETE", delegate (UIComponent comp, int ret)
                    {
                        if (ret == 1)
                        {
                            Singleton<SimulationManager>.instance.AddAction(delegate
                            {
                                Singleton<TransportManager>.instance.ReleaseLine(m_lineIdSelecionado.TransportLine);
                                closeLineInfo(component, eventParam);
                            });
                        }
                    });
                }
            };

            var icon = deleteLine.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "RemoveUnwantedIcon";
            icon.color = Color.red;

            //Auto color & Auto Name
            TLMUtils.createUIElement(out UIButton buttonAutoName, m_lineInfoPanel.transform);
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.relativePosition = new Vector3(m_lineInfoPanel.width - 50f, m_lineInfoPanel.height - 230f);
            buttonAutoName.width = 40;
            buttonAutoName.height = 40;
            buttonAutoName.tooltip = Locale.Get("TLM_USE_AUTO_NAME");
            TLMUtils.initButton(buttonAutoName, true, "ButtonMenu");
            buttonAutoName.name = "AutoName";
            buttonAutoName.isVisible = true;
            buttonAutoName.eventClick += (component, eventParam) =>
            {
                m_lineNameField.text = m_linearMap.autoName;
                saveLineName(m_lineNameField);
            };

            icon = buttonAutoName.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.spriteName = "AutoNameIcon";
            icon.width = 36;
            icon.height = 36;

            TLMUtils.createUIElement(out UIButton buttonAutoColor, m_lineInfoPanel.transform);
            buttonAutoColor.relativePosition = new Vector3(m_lineInfoPanel.width - 100f, m_lineInfoPanel.height - 230f);
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 40;
            buttonAutoColor.height = 40;
            buttonAutoColor.tooltip = Locale.Get("TLM_PICK_COLOR_FROM_PALETTE_TOOLTIP");
            TLMUtils.initButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.name = "AutoColor";
            buttonAutoColor.isVisible = true;
            buttonAutoColor.eventClick += (component, eventParam) =>
            {
                m_lineColorPicker.selectedColor = m_controller.AutoColor(m_lineIdSelecionado.TransportLine);
                updateLineUI(m_lineColorPicker.selectedColor);
            };

            icon = buttonAutoColor.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "AutoColorIcon";

            TLMUtils.createUIElement(out m_enableBudgetPerHour, m_lineInfoPanel.transform);
            m_enableBudgetPerHour.relativePosition = new Vector3(m_lineInfoPanel.width - 200f, m_lineInfoPanel.height - 230f);
            m_enableBudgetPerHour.textScale = 0.6f;
            m_enableBudgetPerHour.width = 40;
            m_enableBudgetPerHour.height = 40;
            m_enableBudgetPerHour.tooltip = Locale.Get("TLM_USE_PER_PERIOD_BUDGET");
            TLMUtils.initButton(m_enableBudgetPerHour, true, "ButtonMenu");
            m_enableBudgetPerHour.name = "EnableBudgetPerHour";
            m_enableBudgetPerHour.isVisible = true;
            m_enableBudgetPerHour.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
                IBudgetableExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(m_lineIdSelecionado.TransportLine))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = m_lineIdSelecionado.TransportLine;
                }
                else
                {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                    idx = TLMLineUtils.getPrefix(m_lineIdSelecionado.TransportLine);
                }

                uint[] saveData = bte.GetBudgetsMultiplier(idx);
                uint[] newSaveData = new uint[8];
                for (int i = 0; i < 8; i++)
                {
                    newSaveData[i] = saveData[0];
                }
                bte.SetBudgetMultiplier(idx, newSaveData);

                updateSliders();
            };

            icon = m_enableBudgetPerHour.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "PerHourIcon";


            TLMUtils.createUIElement(out m_disableBudgetPerHour, m_lineInfoPanel.transform);
            m_disableBudgetPerHour.relativePosition = new Vector3(m_lineInfoPanel.width - 200f, m_lineInfoPanel.height - 230f);
            m_disableBudgetPerHour.textScale = 0.6f;
            m_disableBudgetPerHour.width = 40;
            m_disableBudgetPerHour.height = 40;
            m_disableBudgetPerHour.tooltip = Locale.Get("TLM_USE_SINGLE_BUDGET");
            TLMUtils.initButton(m_disableBudgetPerHour, true, "ButtonMenu");
            m_disableBudgetPerHour.name = "DisableBudgetPerHour";
            m_disableBudgetPerHour.isVisible = true;
            m_disableBudgetPerHour.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];

                IBudgetableExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(m_lineIdSelecionado.TransportLine))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = m_lineIdSelecionado.TransportLine;
                }
                else
                {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                    idx = TLMLineUtils.getPrefix(m_lineIdSelecionado.TransportLine);
                }
                uint[] saveData = bte.GetBudgetsMultiplier(idx);
                uint[] newSaveData = new uint[] { saveData[0] };
                bte.SetBudgetMultiplier(idx, newSaveData);

                updateSliders();
            };

            icon = m_disableBudgetPerHour.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "24hLineIcon";

            TLMUtils.createUIElement(out m_goToWorldInfoPanel, m_lineInfoPanel.transform);
            m_goToWorldInfoPanel.relativePosition = new Vector3(m_lineInfoPanel.width - 200f, m_lineInfoPanel.height - 230f);
            m_goToWorldInfoPanel.text = "IPT2";
            m_goToWorldInfoPanel.textScale = 0.6f;
            m_goToWorldInfoPanel.width = 40;
            m_goToWorldInfoPanel.height = 40;
            m_goToWorldInfoPanel.tooltip = Locale.Get("TLM_GO_TO_WORLD_INFO_PANEL_LINE");
            TLMUtils.initButton(m_goToWorldInfoPanel, true, "ButtonMenu");
            m_goToWorldInfoPanel.name = "IPT2WorldInfoButton";
            m_goToWorldInfoPanel.isVisible = true;
            m_goToWorldInfoPanel.eventClick += (component, eventParam) =>
            {
                WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(Vector3.zero, m_lineIdSelecionado);
            };

            //Absolute toggle
            TLMUtils.createUIElement(out m_absoluteCountMode, m_lineInfoPanel.transform);
            m_absoluteCountMode.relativePosition = new Vector3(m_lineInfoPanel.width - 250f, m_lineInfoPanel.height - 230f);
            m_absoluteCountMode.textScale = 0.6f;
            m_absoluteCountMode.width = 40;
            m_absoluteCountMode.height = 40;
            m_absoluteCountMode.tooltip = Locale.Get("TLM_USE_ABSOLUTE_BUDGET");
            TLMUtils.initButton(m_absoluteCountMode, true, "ButtonMenu");
            m_absoluteCountMode.name = "AbsoluteBudget";
            m_absoluteCountMode.isVisible = true;
            m_absoluteCountMode.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];

                IUseAbsoluteVehicleCountExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(m_lineIdSelecionado.TransportLine))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = m_lineIdSelecionado.TransportLine;
                }
                else
                {
                    return;
                }
                bte.SetUsingAbsoluteVehicleCount(idx, true);

                updateSliders();
            };

            icon = m_absoluteCountMode.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "AbsoluteMode";

            TLMUtils.createUIElement(out m_multiplierMode, m_lineInfoPanel.transform);
            m_multiplierMode.relativePosition = new Vector3(m_lineInfoPanel.width - 250f, m_lineInfoPanel.height - 230f);
            m_multiplierMode.textScale = 0.6f;
            m_multiplierMode.width = 40;
            m_multiplierMode.height = 40;
            m_multiplierMode.tooltip = Locale.Get("TLM_USE_RELATIVE_BUDGET");
            TLMUtils.initButton(m_multiplierMode, true, "ButtonMenu");
            m_multiplierMode.name = "RelativeBudget";
            m_multiplierMode.isVisible = true;
            m_multiplierMode.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];

                IUseAbsoluteVehicleCountExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(m_lineIdSelecionado.TransportLine))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = m_lineIdSelecionado.TransportLine;
                }
                else
                {
                    return;
                }
                bte.SetUsingAbsoluteVehicleCount(idx, false);

                updateSliders();
            };

            icon = m_multiplierMode.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "RelativeMode";

        }

        private void createLineInfoLabels()
        {
            TLMUtils.createUIElement(out m_lineLenghtLabel, m_lineInfoPanel.transform);
            m_lineLenghtLabel.autoSize = false;
            m_lineLenghtLabel.relativePosition = new Vector3(10f, 45f);
            m_lineLenghtLabel.textAlignment = UIHorizontalAlignment.Left;
            m_lineLenghtLabel.text = "";
            m_lineLenghtLabel.width = 550;
            m_lineLenghtLabel.height = 25;
            m_lineLenghtLabel.prefix = "";
            m_lineLenghtLabel.suffix = "";
            m_lineLenghtLabel.name = "LineLenghtLabel";
            m_lineLenghtLabel.textScale = 0.6f;
            m_lineLenghtLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out m_veiculosLinhaLabel, m_lineInfoPanel.transform);
            m_veiculosLinhaLabel.autoSize = false;
            m_veiculosLinhaLabel.relativePosition = new Vector3(10f, 55);
            m_veiculosLinhaLabel.textAlignment = UIHorizontalAlignment.Left;
            m_veiculosLinhaLabel.text = "";
            m_veiculosLinhaLabel.width = 550;
            m_veiculosLinhaLabel.height = 25;
            m_veiculosLinhaLabel.name = "VehiclesLineLabel";
            m_veiculosLinhaLabel.textScale = 0.6f;
            m_veiculosLinhaLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out m_viagensEvitadasLabel, m_lineInfoPanel.transform);
            m_viagensEvitadasLabel.autoSize = false;
            m_viagensEvitadasLabel.relativePosition = new Vector3(10f, 65);
            m_viagensEvitadasLabel.textAlignment = UIHorizontalAlignment.Left;
            m_viagensEvitadasLabel.text = "";
            m_viagensEvitadasLabel.width = 250;
            m_viagensEvitadasLabel.height = 25;
            m_viagensEvitadasLabel.name = "AvoidedTravelsLabel";
            m_viagensEvitadasLabel.textScale = 0.6f;
            m_viagensEvitadasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out m_passageirosEturistasLabel, m_lineInfoPanel.transform);
            m_passageirosEturistasLabel.autoSize = false;
            m_passageirosEturistasLabel.relativePosition = new Vector3(10f, 75f);
            m_passageirosEturistasLabel.textAlignment = UIHorizontalAlignment.Left;
            m_passageirosEturistasLabel.text = "";
            m_passageirosEturistasLabel.width = 350;
            m_passageirosEturistasLabel.height = 25;
            m_passageirosEturistasLabel.name = "TouristAndPassagersLabel";
            m_passageirosEturistasLabel.textScale = 0.6f;
            m_passageirosEturistasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out m_budgetLabel, m_lineInfoPanel.transform);
            m_budgetLabel.autoSize = false;
            m_budgetLabel.relativePosition = new Vector3(10f, 85f);
            m_budgetLabel.textAlignment = UIHorizontalAlignment.Left;
            m_budgetLabel.width = 550;
            m_budgetLabel.height = 25;
            m_budgetLabel.name = "ExtraInfoLabel";
            m_budgetLabel.textScale = 0.6f;
            m_budgetLabel.prefix = Locale.Get("TLM_LINE_EFFECTIVE_BUDGET") + ": ";
            m_budgetLabel.font = UIHelperExtension.defaultFontCheckbox;
        }

        private void createTitleBarItems()
        {


            TLMUtils.createUIElement(out m_lineTransportIconTypeLabel, m_lineInfoPanel.transform);
            m_lineTransportIconTypeLabel.autoSize = false;
            m_lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            m_lineTransportIconTypeLabel.width = 30;
            m_lineTransportIconTypeLabel.height = 20;
            m_lineTransportIconTypeLabel.name = "LineTransportIcon";
            m_lineTransportIconTypeLabel.clipChildren = true;
            TLMUtils.createDragHandle(m_lineTransportIconTypeLabel, m_lineInfoPanel);

            GameObject lpddgo = GameObject.Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject, m_lineInfoPanel.transform);
            m_linePrefixDropDown = lpddgo.GetComponent<UIDropDown>();
            m_linePrefixDropDown.isLocalized = false;
            m_linePrefixDropDown.autoSize = false;
            m_linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;
            m_linePrefixDropDown.text = "";
            m_linePrefixDropDown.width = 40;
            m_linePrefixDropDown.height = 30;
            m_linePrefixDropDown.name = "LinePrefixDropDown";
            m_linePrefixDropDown.textScale = 1.6f;
            m_linePrefixDropDown.itemHeight = 35;
            m_linePrefixDropDown.itemPadding = new RectOffset(2, 2, 2, 2);
            m_linePrefixDropDown.textFieldPadding = new RectOffset(2, 2, 2, 2);
            m_linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            m_linePrefixDropDown.relativePosition = new Vector3(70f, 5f);
            m_linePrefixDropDown.normalBgSprite = "OptionsDropboxListbox";
            m_linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;


            TLMUtils.createUIElement(out m_lineNumberLabel, m_lineInfoPanel.transform);
            m_lineNumberLabel.autoSize = false;
            m_lineNumberLabel.relativePosition = new Vector3(80f, 5f);
            m_lineNumberLabel.horizontalAlignment = UIHorizontalAlignment.Center;
            m_lineNumberLabel.text = "";
            m_lineNumberLabel.width = 75;
            m_lineNumberLabel.height = 30;
            m_lineNumberLabel.name = "LineNumberLabel";
            m_lineNumberLabel.normalBgSprite = "EmptySprite";
            m_lineNumberLabel.textScale = 1.6f;
            m_lineNumberLabel.padding = new RectOffset(0, 0, 0, 0);
            m_lineNumberLabel.color = new Color(0, 0, 0, 1);
            TLMUtils.uiTextFieldDefaults(m_lineNumberLabel);
            m_lineNumberLabel.numericalOnly = true;
            m_lineNumberLabel.maxLength = 4;
            m_lineNumberLabel.eventLostFocus += saveLineNumber;
            m_lineNumberLabel.zOrder = 10;


            TLMUtils.createUIElement(out m_lineNameField, m_lineInfoPanel.transform);
            m_lineNameField.autoSize = false;
            m_lineNameField.relativePosition = new Vector3(190f, 11f);
            m_lineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
            m_lineNameField.text = "NOME";
            m_lineNameField.width = 410;
            m_lineNameField.height = 18;
            m_lineNameField.name = "LineNameLabel";
            m_lineNameField.maxLength = 256;
            m_lineNameField.textScale = 1f;
            TLMUtils.uiTextFieldDefaults(m_lineNameField);
            m_lineNameField.eventGotFocus += (component, eventParam) =>
            {
                m_lastLineName = m_lineNameField.text;
            };
            m_lineNameField.eventLostFocus += (component, eventParam) =>
            {
                if (m_lastLineName != m_lineNameField.text)
                {
                    saveLineName(m_lineNameField);
                }
                m_lineNameField.text = m_controller.tm.GetLineName(m_lineIdSelecionado.TransportLine);
            };


            m_lineColorPicker = TLMUtils.CreateColorField(m_lineInfoPanel);
            m_lineColorPicker.name = "LineColorPicker";
            m_lineColorPicker.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
            m_lineColorPicker.relativePosition = new Vector3(42f, 10f);
            m_lineColorPicker.height = 26f;
            m_lineColorPicker.width = 26f;
            m_lineColorPicker.enabled = true;
            m_lineColorPicker.eventSelectedColorChanged += (UIComponent component, Color value) =>
            {
                TLMLineUtils.setLineColor(m_lineIdSelecionado.TransportLine, value);
                updateLineUI(value);
            };


            TLMUtils.createUIElement(out UIButton voltarButton2, m_lineInfoPanel.transform);
            voltarButton2.relativePosition = new Vector3(m_lineInfoPanel.width - 40f, 5f);
            voltarButton2.width = 30;
            voltarButton2.height = 30;
            TLMUtils.initButton(voltarButton2, true, "buttonclose", true);
            voltarButton2.name = "LineInfoCloseButton";
            voltarButton2.eventClick += closeLineInfo;
        }

        private void createMainPanel()
        {
            TLMUtils.createUIElement(out m_lineInfoPanel, m_controller.mainRef.transform);
            m_lineInfoPanel.Hide();
            m_lineInfoPanel.relativePosition = new Vector3(394.0f, 0.0f);
            m_lineInfoPanel.width = 650;
            m_lineInfoPanel.height = 380;
            m_lineInfoPanel.zOrder = 50;
            m_lineInfoPanel.color = new Color32(255, 255, 255, 255);
            m_lineInfoPanel.backgroundSprite = "MenuPanel2";
            m_lineInfoPanel.name = "LineInfoPanel";
            m_lineInfoPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            m_lineInfoPanel.autoLayout = false;
            m_lineInfoPanel.useCenter = true;
            m_lineInfoPanel.wrapLayout = false;
            m_lineInfoPanel.canFocus = true;
            TLMUtils.createDragHandle(m_lineInfoPanel, m_lineInfoPanel, 35f);
            m_lineInfoPanel.eventVisibilityChanged += (component, value) =>
            {
                m_linearMap?.setVisible(value);
            };

            m_uiHelper = new UIHelperExtension(m_lineInfoPanel);
        }
        #endregion

        #region Actions
        private void ChangeFirstStop(int idxSel)
        {
            if (idxSel <= 0 || idxSel >= m_firstStopSelect.items.Length) return;
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
            if ((t.m_flags & TransportLine.Flags.Invalid) != TransportLine.Flags.None)
            {
                return;
            }
            Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_stops = t.GetStop(idxSel);
            openLineInfo(m_lineIdSelecionado.TransportLine);
            if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED))
            {
                TLMController.instance.AutoName(m_lineIdSelecionado.TransportLine);
            }
        }

        private void saveLineName(UITextField u)
        {
            string value = u.text;

            TLMLineUtils.setLineName(m_lineIdSelecionado.TransportLine, value);
        }

        private void changeLineTime(int selection)
        {
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                ushort lineID = m_lineIdSelecionado.TransportLine;
                TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID], ((selection & 0x2) == 0), ((selection & 0x1) == 0));
            });
        }

        private void saveLineNumber(UIComponent c, object v)
        {
            saveLineNumber();
        }

        private void saveLineNumber(UIComponent c, int v)
        {
            saveLineNumber();
        }

        private void saveLineNumber()
        {
            String value = "0" + m_lineNumberLabel.text;
            int valPrefixo = m_linePrefixDropDown.selectedIndex;
            TLMLineUtils.getLineNamingParameters(m_lineIdSelecionado.TransportLine, out ModoNomenclatura prefixo, out Separador sep, out ModoNomenclatura sufixo, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix);
            ushort num = ushort.Parse(value);
            if (prefixo != ModoNomenclatura.Nenhum)
            {
                num = (ushort)(valPrefixo * 1000 + (num % 1000));
            }
            if (num < 1)
            {
                m_lineNumberLabel.textColor = new Color(1, 0, 0, 1);
                return;
            }
            bool numeroUsado = isNumeroUsado(num, m_lineIdSelecionado.TransportLine);

            if (numeroUsado)
            {
                m_lineNumberLabel.textColor = new Color(1, 0, 0, 1);
            }
            else
            {
                m_lineNumberLabel.textColor = new Color(1, 1, 1, 1);
                Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_lineNumber = num;
                m_linearMap.setLineNumberCircle(m_lineIdSelecionado.TransportLine);
                m_autoNameLabel.text = m_linearMap.autoName;
                if (prefixo != ModoNomenclatura.Nenhum)
                {
                    m_lineNumberLabel.text = (num % 1000).ToString();
                    m_linePrefixDropDown.selectedIndex = (num / 1000);
                }
                else
                {
                    m_lineNumberLabel.text = (num % 10000).ToString();
                }
                updateSliders();
            }
            EventOnLineChanged(m_lineIdSelecionado.TransportLine);
        }

        private void SetTicketPrice(string value)
        {
            bool res = UInt32.TryParse(value, out uint valInt);
            if (!res) return;
            ITicketPriceExtension tpe;
            uint idx;
            if (TLMTransportLineExtension.instance.IsUsingCustomConfig(m_lineIdSelecionado.TransportLine))
            {
                tpe = TLMTransportLineExtension.instance;
                idx = m_lineIdSelecionado.TransportLine;
            }
            else
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
                idx = TLMLineUtils.getPrefix(m_lineIdSelecionado.TransportLine);
                var tsd = TransportSystemDefinition.from(tl.Info);
                tpe = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
            }
            tpe.SetTicketPrice(idx, valInt);
        }
        #endregion

        #region Checking Methods

        private float getEffectiveBudget()
        {
            return TLMLineUtils.getEffectiveBugdet(m_lineIdSelecionado.TransportLine);
        }

        private bool isNumeroUsado(int numLinha, ushort lineIdx)
        {
            var tsdOr = TransportSystemDefinition.getDefinitionForLine(lineIdx);
            if (tsdOr == default(TransportSystemDefinition))
            {
                return true;
            }

            return TLMLineUtils.isNumberUsed(numLinha, ref tsdOr, lineIdx);
        }
        #endregion

        #region Budget Methods
        private void setBudgetHour(float x, int selectedHourIndex)
        {
            TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
            ushort val = (ushort)(x * 100 + 0.5f);
            IBudgetableExtension bte;
            uint[] saveData;
            uint idx;
            if (TLMTransportLineExtension.instance.IsUsingCustomConfig(m_lineIdSelecionado.TransportLine))
            {
                saveData = TLMTransportLineExtension.instance.GetBudgetsMultiplier(m_lineIdSelecionado.TransportLine);
                bte = TLMTransportLineExtension.instance;
                idx = m_lineIdSelecionado.TransportLine;
            }
            else
            {
                idx = TLMLineUtils.getPrefix(m_lineIdSelecionado.TransportLine);
                var tsd = TransportSystemDefinition.from(tl.Info);
                bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                saveData = bte.GetBudgetsMultiplier(TLMLineUtils.getPrefix(m_lineIdSelecionado.TransportLine));
            }
            if (selectedHourIndex >= saveData.Length || saveData[selectedHourIndex] == val)
            {
                return;
            }
            saveData[selectedHourIndex] = val;
            bte.SetBudgetMultiplier(idx, saveData);
        }

        private UISlider GenerateVerticalBudgetMultiplierField(UIHelperExtension uiHelper, int idx)
        {
            UISlider bugdetSlider = (UISlider)uiHelper.AddSlider(Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL"), 0f, 5, 0.05f, -1,
                (x) =>
                {

                });
            UILabel budgetSliderLabel = bugdetSlider.transform.parent.GetComponentInChildren<UILabel>();
            UIPanel budgetSliderPanel = bugdetSlider.GetComponentInParent<UIPanel>();

            budgetSliderPanel.relativePosition = new Vector2(45 * idx + 15, 130);
            budgetSliderPanel.width = 40;
            budgetSliderPanel.height = 160;
            bugdetSlider.zOrder = 0;
            budgetSliderPanel.autoLayout = true;

            bugdetSlider.size = new Vector2(40, 100);
            bugdetSlider.scrollWheelAmount = 0;
            bugdetSlider.orientation = UIOrientation.Vertical;
            bugdetSlider.clipChildren = true;
            bugdetSlider.thumbOffset = new Vector2(0, -100);
            bugdetSlider.color = Color.black;

            bugdetSlider.thumbObject.width = 40;
            bugdetSlider.thumbObject.height = 200;
            ((UISprite)bugdetSlider.thumbObject).spriteName = "ScrollbarThumb";
            ((UISprite)bugdetSlider.thumbObject).color = new Color32(1, 140, 46, 255);

            budgetSliderLabel.textScale = 0.5f;
            budgetSliderLabel.autoSize = false;
            budgetSliderLabel.wordWrap = true;
            budgetSliderLabel.pivot = UIPivotPoint.TopCenter;
            budgetSliderLabel.textAlignment = UIHorizontalAlignment.Center;
            budgetSliderLabel.text = string.Format(" x{0:0.00}", 0);
            budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL", idx);
            budgetSliderLabel.width = 40;
            budgetSliderLabel.font = UIHelperExtension.defaultFontCheckbox;

            var idx_loc = idx;
            bugdetSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                var lineid = m_lineIdSelecionado.TransportLine;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(lineid) && TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(lineid))
                {
                    budgetSliderLabel.text = string.Format(" {0:0}", val * 20);
                    if (budgetSliderLabel.suffix == string.Empty)
                    {
                        budgetSliderLabel.suffix = Locale.Get("TLM_BUDGET_MULTIPLIER_SUFFIX_ABSOLUTE_SHORT");
                    }
                }
                else
                {
                    budgetSliderLabel.text = string.Format(" x{0:0.00}", val);
                    if (budgetSliderLabel.suffix != string.Empty)
                    {
                        budgetSliderLabel.suffix = string.Empty;
                    }
                }
                setBudgetHour(val, idx_loc);
            };

            return bugdetSlider;
        }

        private void updateSliders()
        {
            if (TLMSingleton.isIPTLoaded)
            {
                m_goToWorldInfoPanel.isVisible = true;
                m_disableBudgetPerHour.isVisible = false;
                m_enableBudgetPerHour.isVisible = false;
                m_absoluteCountMode.isVisible = false;
                m_multiplierMode.isVisible = false;
                m_IgnorePrefix.isVisible = false;
                for (int i = 0; i < m_budgetSliders.Length; i++)
                {
                    m_budgetSliders[i].isEnabled = false;
                    m_budgetSliders[i].parent.isVisible = false;
                }
                m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_IPT2_NO_BUDGET_CONTROL"));
                return;
            }
            else
            {

                m_IgnorePrefix.isVisible = true;
                m_goToWorldInfoPanel.isVisible = false;
            }

            TransportLine t = m_controller.tm.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine];
            var tsd = TransportSystemDefinition.getDefinitionForLine(m_lineIdSelecionado.TransportLine);
            if (m_lineIdSelecionado.TransportLine <= 0 || tsd == default(TransportSystemDefinition))
            {
                return;
            }
            ushort lineNumber = t.m_lineNumber;

            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            ModoNomenclatura mnPrefixo = (ModoNomenclatura)TLMCW.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.PREFIX | transportType);

            uint[] multipliers;
            IBudgetableExtension bte;
            uint idx;

            m_IgnorePrefix.isChecked = TLMTransportLineExtension.instance.IsUsingCustomConfig(m_lineIdSelecionado.TransportLine);
            if (m_IgnorePrefix.isChecked)
            {
                idx = m_lineIdSelecionado.TransportLine;
                multipliers = TLMTransportLineExtension.instance.GetBudgetsMultiplier(m_lineIdSelecionado.TransportLine);
                bte = TLMTransportLineExtension.instance;
                m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_LINE"), TLMLineUtils.getLineStringId(m_lineIdSelecionado.TransportLine), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
                m_absoluteCountMode.isVisible = !TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(idx);
                m_multiplierMode.isVisible = TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(idx);
                m_budgetLabel.isVisible = false;
            }
            else
            {
                idx = TLMLineUtils.getPrefix(m_lineIdSelecionado.TransportLine);
                bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                multipliers = bte.GetBudgetsMultiplier(idx);
                m_absoluteCountMode.isVisible = false;
                m_multiplierMode.isVisible = false;
                m_budgetLabel.isVisible = true;

                if (mnPrefixo != ModoNomenclatura.Nenhum)
                {
                    m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_PREFIX"), idx > 0 ? TLMUtils.getStringFromNumber(TLMUtils.getStringOptionsForPrefix(mnPrefixo), (int)idx + 1) : Locale.Get("TLM_UNPREFIXED"), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
                }
                else
                {
                    m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_LINE"), TLMLineUtils.getLineStringId(m_lineIdSelecionado.TransportLine), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
                }
            }

            bool budgetPerHourEnabled = multipliers.Length == 8;
            m_disableBudgetPerHour.isVisible = budgetPerHourEnabled;
            m_enableBudgetPerHour.isVisible = !budgetPerHourEnabled && tsd.hasVehicles();
            for (int i = 0; i < m_budgetSliders.Length; i++)
            {
                UILabel budgetSliderLabel = m_budgetSliders[i].transform.parent.GetComponentInChildren<UILabel>();
                if (i == 0)
                {
                    if (multipliers.Length == 1)
                    {
                        budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL_ALL");
                    }
                    else
                    {
                        budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL", 0);
                    }
                }
                else
                {
                    m_budgetSliders[i].isEnabled = budgetPerHourEnabled;
                    m_budgetSliders[i].parent.isVisible = budgetPerHourEnabled;
                }

                if (i < multipliers.Length)
                {
                    m_budgetSliders[i].value = 0;
                    m_budgetSliders[i].value = multipliers[i] / 100f;
                }
            }

            m_DayLine.isVisible = !budgetPerHourEnabled;
            m_DayNightLine.isVisible = !budgetPerHourEnabled;
            m_NightLine.isVisible = !budgetPerHourEnabled;
            m_DisabledLine.isVisible = !budgetPerHourEnabled;

        }

        #endregion

        #region Update Methods
        private void updateLineUI(Color color)
        {
            m_lineNumberLabel.color = color;
            m_linearMap.setLinearMapColor(color);
        }

        public void updateBidings()
        {
            ushort lineID = m_lineIdSelecionado.TransportLine;
            TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID];
            TransportInfo info = tl.Info;
            TransportSystemDefinition tsd = TransportSystemDefinition.from(info);
            int turistas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_touristPassengers.m_averageCount;
            int residentes = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_residentPassengers.m_averageCount;
            int residentesPorc = residentes;
            if (residentesPorc == 0)
                residentesPorc = 1;
            int criancas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_childPassengers.m_averageCount;
            int adolescentes = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_teenPassengers.m_averageCount;
            int jovens = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_youngPassengers.m_averageCount;
            int adultos = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_adultPassengers.m_averageCount;
            int idosos = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_seniorPassengers.m_averageCount;
            int motoristas = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID].m_passengers.m_carOwningPassengers.m_averageCount;
            int veiculosLinha = TLMLineUtils.GetVehiclesCount(lineID);
            int porcCriancas = (criancas * 100 / residentesPorc);
            int porcAdolescentes = (adolescentes * 100 / residentesPorc);
            int porcJovens = (jovens * 100 / residentesPorc);
            int porcAdultos = (adultos * 100 / residentesPorc);
            int porcIdosos = (idosos * 100 / residentesPorc);
            int soma = porcCriancas + porcAdolescentes + porcJovens + porcAdultos + porcIdosos;
            if (soma != 0 && soma != 100)
            {
                porcAdultos = 100 - (porcCriancas + porcAdolescentes + porcJovens + porcIdosos);
            }
            m_agesPanel.SetValues(new int[]
                                 {
                porcCriancas,
                porcAdolescentes,
                porcJovens,
                porcAdultos,
                porcIdosos
            });
            m_passageirosEturistasLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_PASSENGERS", new object[]
                                                                           {
                residentes,
                turistas
            });

            int viagensSalvas = 0;
            int coeficienteViagens = 0;
            if (residentes + turistas != 0)
            {
                coeficienteViagens += criancas * 0;
                coeficienteViagens += adolescentes * 5;
                coeficienteViagens += jovens * ((15 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
                coeficienteViagens += adultos * ((20 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
                coeficienteViagens += idosos * ((10 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
            }
            if (coeficienteViagens != 0)
            {
                viagensSalvas = (int)(((long)motoristas * 10000L + (long)(coeficienteViagens >> 1)) / (long)coeficienteViagens);
                viagensSalvas = Mathf.Clamp(viagensSalvas, 0, 100);
            }
            m_viagensEvitadasLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_TRIPSAVED", new object[]{
                viagensSalvas
            });

            linearMap.updateSubIconLayer();

            //lines info
            int stopsCount = TLMLineUtils.GetStopsCount(lineID);
            if (m_lastStopsCount != stopsCount)
            {
                float totalSize = TLMLineUtils.GetLineLength(lineID);
                m_lineLenghtLabel.text = string.Format(Locale.Get("TLM_LENGHT_AND_STOPS"), totalSize, stopsCount);
                m_lastStopsCount = stopsCount;
            }

            //estatisticas novas
            m_veiculosLinhaLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_VEHICLECOUNT", new object[] { veiculosLinha }) + "/" + tl.CalculateTargetVehicleCount();

            uint prefix = 0;
            if (TLMConfigWarehouse.getCurrentConfigInt(tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
            {
                prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
            }

            float baseBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;

            m_budgetLabel.text = string.Format("{0:0%} ({1:0%})", getEffectiveBudget(), Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f + 0.004f);//585+1/7 = frames/week                ;
            m_budgetLabel.tooltip = string.Format(Locale.Get("TLM_LINE_BUDGET_EXPLAIN_2"),
                TLMCW.getNameForTransportType(tsd.toConfigIndex()),
                baseBudget, Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f + 0.004f, getEffectiveBudget());
            linearMap.updateBidings();
        }
        #endregion

        #region Open & Close
        public void Show()
        {
            if (!GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible)
            {
                GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible = true;
            }
            if (Singleton<ToolController>.instance.CurrentTool.GetType() == typeof(TransportTool))
            {
                Singleton<ToolController>.instance.CurrentTool = Singleton<DefaultTool>.instance;
            }
            m_lineInfoPanel.Show();
        }

        public void Hide()
        {
            m_lineInfoPanel.Hide();
        }

        public void closeLineInfo(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
            m_controller.OpenTLMPanel();
            TLMPublicTransportManagementPanel.instance?.OpenAt(UiCategoryTab.LineListing, TransportSystemDefinition.from(m_lineIdSelecionado.TransportLine));
        }

        public void openLineInfo(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort lineID = (component as UIButtonLineInfo).lineID;
            if (lineID > 0)
            {
                openLineInfo(lineID);
            }

        }

        public void openLineInfo(ushort lineID)
        {
            var tsd = TransportSystemDefinition.getDefinitionForLine(lineID);
            if (lineID <= 0 || tsd == default(TransportSystemDefinition))
            {
                return;
            }
            WorldInfoPanel.HideAllWorldInfoPanels();
            m_linePrefixDropDown.eventSelectedIndexChanged -= saveLineNumber;
            m_lineNumberLabel.eventLostFocus -= saveLineNumber;

            m_lineIdSelecionado = default(InstanceID);
            m_lineIdSelecionado.TransportLine = lineID;

            TransportLine t = m_controller.tm.m_lines.m_buffer[(int)lineID];
            ushort lineNumber = t.m_lineNumber;

            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            ModoNomenclatura mnPrefixo = (ModoNomenclatura)TLMCW.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.PREFIX | transportType);

            if (TLMLineUtils.hasPrefix(lineID))
            {
                m_lineNumberLabel.text = (lineNumber % 1000).ToString();
                m_lineNumberLabel.relativePosition = new Vector3(110f, 5f);
                m_lineNumberLabel.width = 55;
                m_linePrefixDropDown.enabled = false;

                var temp = TLMUtils.getStringOptionsForPrefix(mnPrefixo);
                m_linePrefixDropDown.items = temp;
                m_linePrefixDropDown.selectedIndex = lineNumber / 1000;
                m_linePrefixDropDown.enabled = true;
                m_lineNumberLabel.maxLength = 3;


            }
            else
            {
                m_lineNumberLabel.text = (lineNumber).ToString();
                m_lineNumberLabel.relativePosition = new Vector3(80f, 5f);
                m_lineNumberLabel.width = 75;
                m_lineNumberLabel.maxLength = 4;
                m_linePrefixDropDown.enabled = false;
            }



            m_lineNumberLabel.color = m_controller.tm.GetLineColor(lineID);
            m_lineNameField.text = m_controller.tm.GetLineName(lineID);

            m_lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            m_lineTransportIconTypeLabel.height = 20;
            m_lineTransportIconTypeLabel.atlas = m_linePrefixDropDown.atlas;
            m_lineTransportIconTypeLabel.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(t.Info.m_transportType);

            m_lineColorPicker.selectedColor = m_controller.tm.GetLineColor(lineID);

            TLMLineUtils.getLineActive(ref t, out bool day, out bool night);
            m_DayNightLine.isChecked = false;
            m_NightLine.isChecked = false;
            m_DayLine.isChecked = false;
            m_DisabledLine.isChecked = false;
            if (day && night)
            {
                m_DayNightLine.isChecked = true;
            }
            else if (day)
            {
                m_DayLine.isChecked = true;
            }
            else if (night)
            {
                m_NightLine.isChecked = true;
            }
            else
            {
                m_DisabledLine.isChecked = true;
            }
            updateSliders();
            m_linearMap.redrawLine();
            m_autoNameLabel.text = m_linearMap.autoName;
            m_lineInfoPanel.color = Color.Lerp(TLMCW.getColorForTransportType(transportType), Color.white, 0.4f);

            Show();
            m_controller.CloseTLMPanel();
            m_controller.depotInfoPanel.Hide();

            m_linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            m_lineNumberLabel.eventLostFocus += saveLineNumber;
            m_firstStopSelect.items = TLMLineUtils.getAllStopsFromLine(lineID);
            m_firstStopSelect.selectedIndex = 0;

            UpdateTicketPrice();

            EventOnLineChanged(lineID);
            SetViewMode();
        }

        private void UpdateTicketPrice()
        {
            ushort lineID = m_lineIdSelecionado.TransportLine;
            ITicketPriceExtension tpe;
            uint idx;
            if (TLMTransportLineExtension.instance.IsUsingCustomConfig(lineID))
            {
                tpe = TLMTransportLineExtension.instance;
                idx = lineID;
            }
            else
            {
                var tsd = TransportSystemDefinition.getDefinitionForLine(lineID);
                idx = TLMLineUtils.getPrefix(lineID);
                tpe = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
            }

            m_ticketPriceEditor.text = tpe.GetTicketPrice(idx).ToString();
        }
        #endregion

        #region Events
        public override void OnRenameStationAction(string autoName)
        {
            autoNameLabel.text = autoName;
        }
        public event OnLineLoad EventOnLineChanged;
        #endregion

        private void SetViewMode()
        {
            var tsd = TransportSystemDefinition.from(lineIdSelecionado.TransportLine);
            if (tsd == default(TransportSystemDefinition)) return;
            if (tsd.isTour())
            {
                InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Tours, InfoManager.SubInfoMode.Default);
            }
            else
            {
                InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Transport, InfoManager.SubInfoMode.Default);
            }
        }
    }

    public delegate void OnLineLoad(ushort lineId);

}

