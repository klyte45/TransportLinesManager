using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Extensions;
using System;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;
using Klyte.TransportLinesManager.Extensors;
using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Extensors.VehicleAIExt;
using Klyte.TransportLinesManager.Interfaces;

namespace Klyte.TransportLinesManager.LineList
{
    public class TLMLineInfoPanel : LinearMapParentInterface<TLMLineInfoPanel>
    {
        private TLMAgesChartPanel m_agesPanel;
        private TLMController m_controller;
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
        private AsyncAction m_daytimeChange;

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
        private UILabel m_lineBudgetSlidersTitle;

        private UIDropDown m_firstStopSelect;

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


        public new Transform transform
        {
            get {
                return m_lineInfoPanel.transform;
            }
        }

        public new GameObject gameObject
        {
            get {
                try
                {
                    return m_lineInfoPanel.gameObject;
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    return null;
                }
            }
        }

        public bool isVisible
        {
            get {
                return m_lineInfoPanel.isVisible;
            }
        }

        public TLMController controller
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
        public TLMLineInfoPanel(TLMController controller)
        {
            this.m_controller = controller;
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

            m_agesPanel = new TLMAgesChartPanel(this);
            m_linearMap = new TLMLinearMap(this);
        }

        private void CreateIgnorePrefixBudgetOption()
        {
            m_IgnorePrefix = m_uiHelper.AddCheckboxLocale("TLM_IGNORE_PREFIX_BUDGETING", false);
            m_IgnorePrefix.relativePosition = new Vector3(5f, 300f);
            m_IgnorePrefix.eventCheckChanged += delegate (UIComponent comp, bool value)
            {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0)
                {
                    TLMTransportLineExtensions.instance.SetIgnorePrefixBudget(m_lineIdSelecionado.TransportLine, value);
                    updateSliders();
                }
            };
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

        private void CreateBudgetSliders()
        {
            TLMUtils.createUIElement<UILabel>(ref m_lineBudgetSlidersTitle, m_lineInfoPanel.transform);
            m_lineBudgetSlidersTitle.autoSize = false;
            m_lineBudgetSlidersTitle.relativePosition = new Vector3(15f, 130f);
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
            TLMUtils.createUIElement<UILabel>(ref m_autoNameLabel, m_lineInfoPanel.transform);
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

            UIButton deleteLine = null;
            TLMUtils.createUIElement<UIButton>(ref deleteLine, transform);
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
            UIButton buttonAutoName = null;
            TLMUtils.createUIElement<UIButton>(ref buttonAutoName, m_lineInfoPanel.transform);
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

            UIButton buttonAutoColor = null;
            TLMUtils.createUIElement<UIButton>(ref buttonAutoColor, m_lineInfoPanel.transform);
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

            TLMUtils.createUIElement<UIButton>(ref m_enableBudgetPerHour, m_lineInfoPanel.transform);
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
                if (TLMLineUtils.hasPrefix(ref tl))
                {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    uint prefix = tl.m_lineNumber / 1000u;
                    ITLMTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                    uint[] saveData = bte.GetBudgetsMultiplier(prefix);
                    uint[] newSaveData = new uint[8];
                    for (int i = 0; i < 8; i++)
                    {
                        newSaveData[i] = saveData[0];
                    }
                    bte.SetBudgetMultiplier(prefix, newSaveData);
                }
                updateSliders();
            };

            icon = m_enableBudgetPerHour.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "PerHourIcon";


            TLMUtils.createUIElement<UIButton>(ref m_disableBudgetPerHour, m_lineInfoPanel.transform);
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
                if (TLMLineUtils.hasPrefix(ref tl))
                {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    uint prefix = tl.m_lineNumber / 1000u;
                    ITLMTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                    uint[] saveData = bte.GetBudgetsMultiplier(prefix);
                    uint[] newSaveData = new uint[] { saveData[0] };
                    bte.SetBudgetMultiplier(prefix, newSaveData);
                }
                updateSliders();
            };

            icon = m_disableBudgetPerHour.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "24hLineIcon";

            TLMUtils.createUIElement<UIButton>(ref m_goToWorldInfoPanel, m_lineInfoPanel.transform);
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
        }

        private void createLineInfoLabels()
        {
            TLMUtils.createUIElement<UILabel>(ref m_lineLenghtLabel, m_lineInfoPanel.transform);
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

            TLMUtils.createUIElement<UILabel>(ref m_veiculosLinhaLabel, m_lineInfoPanel.transform);
            m_veiculosLinhaLabel.autoSize = false;
            m_veiculosLinhaLabel.relativePosition = new Vector3(10f, 55);
            m_veiculosLinhaLabel.textAlignment = UIHorizontalAlignment.Left;
            m_veiculosLinhaLabel.text = "";
            m_veiculosLinhaLabel.width = 550;
            m_veiculosLinhaLabel.height = 25;
            m_veiculosLinhaLabel.name = "VehiclesLineLabel";
            m_veiculosLinhaLabel.textScale = 0.6f;
            m_veiculosLinhaLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement<UILabel>(ref m_viagensEvitadasLabel, m_lineInfoPanel.transform);
            m_viagensEvitadasLabel.autoSize = false;
            m_viagensEvitadasLabel.relativePosition = new Vector3(10f, 65);
            m_viagensEvitadasLabel.textAlignment = UIHorizontalAlignment.Left;
            m_viagensEvitadasLabel.text = "";
            m_viagensEvitadasLabel.width = 250;
            m_viagensEvitadasLabel.height = 25;
            m_viagensEvitadasLabel.name = "AvoidedTravelsLabel";
            m_viagensEvitadasLabel.textScale = 0.6f;
            m_viagensEvitadasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement<UILabel>(ref m_passageirosEturistasLabel, m_lineInfoPanel.transform);
            m_passageirosEturistasLabel.autoSize = false;
            m_passageirosEturistasLabel.relativePosition = new Vector3(10f, 75f);
            m_passageirosEturistasLabel.textAlignment = UIHorizontalAlignment.Left;
            m_passageirosEturistasLabel.text = "";
            m_passageirosEturistasLabel.width = 350;
            m_passageirosEturistasLabel.height = 25;
            m_passageirosEturistasLabel.name = "TouristAndPassagersLabel";
            m_passageirosEturistasLabel.textScale = 0.6f;
            m_passageirosEturistasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement<UILabel>(ref m_budgetLabel, m_lineInfoPanel.transform);
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


            TLMUtils.createUIElement<UILabel>(ref m_lineTransportIconTypeLabel, m_lineInfoPanel.transform);
            m_lineTransportIconTypeLabel.autoSize = false;
            m_lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            m_lineTransportIconTypeLabel.width = 30;
            m_lineTransportIconTypeLabel.height = 20;
            m_lineTransportIconTypeLabel.name = "LineTransportIcon";
            m_lineTransportIconTypeLabel.clipChildren = true;
            TLMUtils.createDragHandle(m_lineTransportIconTypeLabel, m_lineInfoPanel);

            GameObject lpddgo = GameObject.Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject);
            m_linePrefixDropDown = lpddgo.GetComponent<UIDropDown>();
            m_lineInfoPanel.AttachUIComponent(m_linePrefixDropDown.gameObject);
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


            TLMUtils.createUIElement<UITextField>(ref m_lineNumberLabel, m_lineInfoPanel.transform);
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


            TLMUtils.createUIElement<UITextField>(ref m_lineNameField, m_lineInfoPanel.transform);
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


            m_lineColorPicker = GameObject.Instantiate(PublicTransportWorldInfoPanel.FindObjectOfType<UIColorField>().gameObject).GetComponent<UIColorField>();
            m_lineInfoPanel.AttachUIComponent(m_lineColorPicker.gameObject);
            m_lineColorPicker.name = "LineColorPicker";
            m_lineColorPicker.relativePosition = new Vector3(42f, 10f);
            m_lineColorPicker.enabled = true;
            m_lineColorPicker.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
            m_lineColorPicker.eventSelectedColorChanged += (UIComponent component, Color value) =>
            {
                TLMUtils.setLineColor(m_lineIdSelecionado.TransportLine, value);
                updateLineUI(value);
            };


            UIButton voltarButton2 = null;
            TLMUtils.createUIElement<UIButton>(ref voltarButton2, m_lineInfoPanel.transform);
            voltarButton2.relativePosition = new Vector3(m_lineInfoPanel.width - 40f, 5f);
            voltarButton2.width = 30;
            voltarButton2.height = 30;
            TLMUtils.initButton(voltarButton2, true, "buttonclose", true);
            voltarButton2.name = "LineInfoCloseButton";
            voltarButton2.eventClick += closeLineInfo;
        }

        private void createMainPanel()
        {
            TLMUtils.createUIElement<UIPanel>(ref m_lineInfoPanel, m_controller.mainRef.transform);
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
                if (m_linearMap != null)
                {
                    m_linearMap.isVisible = value;
                }
            };

            m_uiHelper = new UIHelperExtension(m_lineInfoPanel);
        }
        #endregion

        #region Actions
        private void saveLineName(UITextField u)
        {
            string value = u.text;

            TLMUtils.setLineName(m_lineIdSelecionado.TransportLine, value);
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
        }
        #endregion.

        #region Checking Methods

        private float getEffectiveBudget()
        {
            return TLMLineUtils.getEffectiveBugdet(m_lineIdSelecionado.TransportLine);
        }

        private bool isNumeroUsado(int numLinha, ushort lineIdx)
        {
            var tsdOr = TLMCW.getDefinitionForLine(lineIdx);
            if (tsdOr == default(TransportSystemDefinition))
            {
                return true;
            }

            return TLMLineUtils.isNumberUsed(numLinha, tsdOr, lineIdx);
        }
        #endregion

        #region Budget Methods
        private void setBudgetHour(float x, int selectedHourIndex)
        {
            if (TLMTransportLineExtensions.instance.GetIgnorePrefixBudget(m_lineIdSelecionado.TransportLine))
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_budget = (ushort)(x * 100 + 0.5f);
            }
            else
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
                ushort val = (ushort)(x * 100 + 0.5f);
                if (TLMLineUtils.hasPrefix(ref tl))
                {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    uint prefix = tl.m_lineNumber / 1000u;
                    ITLMTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                    uint[] saveData = bte.GetBudgetsMultiplier(prefix);
                    if (selectedHourIndex >= saveData.Length || saveData[selectedHourIndex] == val)
                    {
                        return;
                    }
                    saveData[selectedHourIndex] = val;
                    bte.SetBudgetMultiplier(prefix, saveData);
                }
                else
                {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_budget = val;
                }
            }
        }

        private UISlider GenerateVerticalBudgetMultiplierField(UIHelperExtension uiHelper, int idx)
        {
            UISlider bugdetSlider = (UISlider)uiHelper.AddSlider(Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL"), 0f, 5, 0.05f, -1,
                (x) =>
                {

                });
            UILabel budgetSliderLabel = bugdetSlider.transform.parent.GetComponentInChildren<UILabel>();
            UIPanel budgetSliderPanel = bugdetSlider.GetComponentInParent<UIPanel>();

            budgetSliderPanel.relativePosition = new Vector2(50 * idx + 15, 160);
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
                budgetSliderLabel.text = string.Format(" x{0:0.00}", val);
                setBudgetHour(val, idx_loc);
            };

            return bugdetSlider;
        }

        private void updateSliders()
        {
            if (TransportLinesManagerMod.isIPTLoaded)
            {
                m_goToWorldInfoPanel.isVisible = true;
                m_disableBudgetPerHour.isVisible = false;
                m_enableBudgetPerHour.isVisible = false;
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
                m_goToWorldInfoPanel.isVisible = false;
            }

            TransportLine t = m_controller.tm.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine];
            var tsd = TLMCW.getDefinitionForLine(m_lineIdSelecionado.TransportLine);
            if (m_lineIdSelecionado.TransportLine <= 0 || tsd == default(TransportSystemDefinition))
            {
                return;
            }
            ushort lineNumber = t.m_lineNumber;

            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            ModoNomenclatura mnPrefixo = (ModoNomenclatura)TLMCW.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.PREFIX | transportType);

            if (mnPrefixo != ModoNomenclatura.Nenhum)
            {
                m_IgnorePrefix.isVisible = true;
                if (TLMTransportLineExtensions.instance.GetIgnorePrefixBudget(m_lineIdSelecionado.TransportLine))
                {
                    m_disableBudgetPerHour.isVisible = false;
                    m_enableBudgetPerHour.isVisible = false;
                    m_IgnorePrefix.isChecked = true;
                    for (int i = 0; i < m_budgetSliders.Length; i++)
                    {
                        if (i == 0)
                        {
                            UILabel budgetSliderLabel = m_budgetSliders[i].transform.parent.GetComponentInChildren<UILabel>();
                            budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL_ALL");
                            m_budgetSliders[i].value = t.m_budget / 100f;
                        }
                        else
                        {
                            m_budgetSliders[i].isEnabled = false;
                            m_budgetSliders[i].parent.isVisible = false;
                        }
                    }
                    m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_LINE"), TLMLineUtils.getLineStringId(m_lineIdSelecionado.TransportLine), TLMCW.getNameForTransportType(tsd.toConfigIndex()));

                }
                else
                {
                    uint prefix = t.m_lineNumber / 1000u;
                    ITLMTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                    uint[] multipliers = bte.GetBudgetsMultiplier(prefix);
                    m_disableBudgetPerHour.isVisible = multipliers.Length == 8;
                    m_enableBudgetPerHour.isVisible = multipliers.Length == 1;
                    m_IgnorePrefix.isChecked = false;
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
                            m_budgetSliders[i].isEnabled = multipliers.Length == 8;
                            m_budgetSliders[i].parent.isVisible = multipliers.Length == 8;
                        }

                        if (i < multipliers.Length)
                        {
                            m_budgetSliders[i].value = multipliers[i] / 100f;
                        }
                    }
                    m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_PREFIX"), prefix > 0 ? TLMUtils.getStringFromNumber(TLMUtils.getStringOptionsForPrefix(mnPrefixo), (int)prefix + 1) : Locale.Get("TLM_UNPREFIXED"), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
                }
            }
            else
            {
                m_disableBudgetPerHour.isVisible = false;
                m_enableBudgetPerHour.isVisible = false;
                m_IgnorePrefix.isVisible = false;
                for (int i = 0; i < m_budgetSliders.Length; i++)
                {
                    if (i == 0)
                    {
                        UILabel budgetSliderLabel = m_budgetSliders[i].transform.parent.GetComponentInChildren<UILabel>();
                        budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL_ALL");
                        m_budgetSliders[i].value = t.m_budget / 100f;
                    }
                    else
                    {
                        m_budgetSliders[i].isEnabled = false;
                        m_budgetSliders[i].parent.isVisible = false;
                    }
                }
                m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_LINE"), TLMLineUtils.getLineStringId(m_lineIdSelecionado.TransportLine), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
            }
        }

        private void setLineBudget(ushort value)
        {
            Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine].m_budget = (ushort)value;
        }

        private void setLineBudgetForAllPrefix(ushort value)
        {
            int prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_lineNumber % 1000;
            for (ushort i = 0; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++)
            {
                if (Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber % 1000 == prefix)
                {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_budget = (ushort)value;
                }

            }
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

            if (m_daytimeChange != null && m_daytimeChange.completedOrFailed)
            {
                linearMap.redrawLine();
                m_daytimeChange = null;
            }
            else
            {
                linearMap.updateBidings();
            }


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
            if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportInfo(info) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
            {
                prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
            }

            float baseBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;

            m_budgetLabel.text = string.Format("{0:0%} ({1:0%})", getEffectiveBudget(), Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f + 0.004f);//585+1/7 = frames/week                ;
            m_budgetLabel.tooltip = string.Format(Locale.Get("TLM_LINE_BUDGET_EXPLAIN_2"),
                TLMCW.getNameForTransportType(TLMCW.getConfigIndexForTransportInfo(info)),
                baseBudget, Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f + 0.004f, getEffectiveBudget());
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
            TransportLine t = m_controller.tm.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine];
            Hide();
            m_controller.defaultListingLinesPanel.Show();
            TLMPublicTransportDetailPanel.instance.SetActiveTab(Array.IndexOf(TLMPublicTransportDetailPanel.tabSystemOrder, TLMCW.getConfigIndexForTransportInfo(t.Info)));
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
            var tsd = TLMCW.getDefinitionForLine(lineID);
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

            if (mnPrefixo != ModoNomenclatura.Nenhum)
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
            m_controller.defaultListingLinesPanel.Hide();
            m_controller.depotInfoPanel.Hide();

            m_linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            m_lineNumberLabel.eventLostFocus += saveLineNumber;
            m_firstStopSelect.items = TLMLineUtils.getAllStopsFromLine(lineID);
            m_firstStopSelect.selectedIndex = 0;
        }
        #endregion

        #region Events
        public override void OnRenameStationAction(string autoName)
        {
            autoNameLabel.text = autoName;
        }
        #endregion

    }
}

