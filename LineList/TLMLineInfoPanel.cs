using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Extensions;
using System;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;
using Klyte.TransportLinesManager.Extensors;
using System.Collections.Generic;
using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Extensors.VehicleAIExt;
using ICities;
using System.Reflection;
using Klyte.TransportLinesManager.Interfaces;

namespace Klyte.TransportLinesManager.LineList
{
    public class TLMLineInfoPanel : LinearMapParentInterface
    {
        private TLMAgesChartPanel agesPanel;
        private TLMController m_controller;
        private TLMLinearMap m_linearMap;
        //		private readonly string costsFormat = "₡ {0:#,0.00} + ₡ {1:#,0.00} = ₡ {2:#,0.00}";
        private int lastStopsCount = 0;

        //line info	
        private UIPanel lineInfoPanel;
        private InstanceID m_lineIdSelecionado;
        private CameraController m_CameraController;
        private string lastLineName;
        private UILabel lineLenghtLabel;
        private UILabel budgetLabel;
        private UITextField lineNumberLabel;
        private UIDropDown linePrefixDropDown;
        private UILabel lineTransportIconTypeLabel;
        private UILabel viagensEvitadasLabel;
        private UILabel passageirosEturistasLabel;
        private UILabel veiculosLinhaLabel;
        private UILabel m_autoNameLabel;
        //private UILabel generalDebugLabel;
        private UITextField lineNameField;
        private UIColorField lineColorPicker;
        private AsyncAction daytimeChange;

        private UIHelperExtension uiHelper;
        private UICheckBox m_DayLine;
        private UICheckBox m_DayNightLine;
        private UICheckBox m_NightLine;
        private UICheckBox m_DisabledLine;

        private UISlider[] budgetSliders = new UISlider[8];
        private UIButton enableBudgetPerHour;
        private UIButton disableBudgetPerHour;
        private UIButton goToWorldInfoPanel;
        private UILabel lineBudgetSlidersTitle;


        public UILabel autoNameLabel
        {
            get {
                return m_autoNameLabel;
            }
        }

        public override Transform TransformLinearMap
        {
            get {
                return lineInfoPanel.transform;
            }
        }


        public Transform transform
        {
            get {
                return lineInfoPanel.transform;
            }
        }

        public GameObject gameObject
        {
            get {
                try {
                    return lineInfoPanel.gameObject;
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
                return lineInfoPanel.isVisible;
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

        public override bool PrefixSelector => false;

        public override TransportInfo CurrentTransportInfo
        {
            get {
                return Singleton<TransportManager>.instance.m_lines.m_buffer[CurrentSelectedId].Info;
            }
        }

        public TLMLineInfoPanel(TLMController controller)
        {
            this.m_controller = controller;
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null) {
                m_CameraController = gameObject.GetComponent<CameraController>();
            }
            createInfoView();
        }

        public void Show()
        {
            if (!GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible) {
                GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible = true;
            }
            if (Singleton<ToolController>.instance.CurrentTool.GetType() == typeof(TransportTool)) {
                Singleton<ToolController>.instance.CurrentTool = Singleton<DefaultTool>.instance;
            }
            lineInfoPanel.Show();
        }

        public void Hide()
        {
            lineInfoPanel.Hide();
        }




        //ACOES
        private void saveLineName(UITextField u)
        {
            string value = u.text;

            TLMUtils.setLineName(m_lineIdSelecionado.TransportLine, value);
        }

        private void setLineBudget(ushort value)
        {
            Singleton<TransportManager>.instance.m_lines.m_buffer[(int) m_lineIdSelecionado.TransportLine].m_budget = (ushort) value;
        }

        private void setLineBudgetForAllPrefix(ushort value)
        {
            int prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_lineNumber % 1000;
            for (ushort i = 0; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++) {
                if (Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber % 1000 == prefix) {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_budget = (ushort) value;
                }

            }
        }

        private float getEffectiveBudget()
        {
            return TLMLineUtils.getEffectiveBugdet(m_lineIdSelecionado.TransportLine);
        }

        private bool isNumeroUsado(int numLinha, ushort lineIdx)
        {
            var tsdOr = TLMCW.getDefinitionForLine(lineIdx);
            if (tsdOr == default(TransportSystemDefinition)) {
                return true;
            }
            TLMCW.ConfigIndex tipo = tsdOr.toConfigIndex();
            for (ushort i = 0; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++) {
                var tsd = TLMCW.getDefinitionForLine(i);
                if (tsd != default(TransportSystemDefinition) && i != lineIdx && tsd.toConfigIndex() == tipo && Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber == numLinha && (Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None) {
                    return true;
                }
            }
            return false;
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
            String value = "0" + lineNumberLabel.text;
            int valPrefixo = linePrefixDropDown.selectedIndex;
            TLMLineUtils.getLineNamingParameters(m_lineIdSelecionado.TransportLine, out ModoNomenclatura prefixo, out Separador sep, out ModoNomenclatura sufixo, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix);
            ushort num = ushort.Parse(value);
            if (prefixo != ModoNomenclatura.Nenhum) {
                num = (ushort) (valPrefixo * 1000 + (num % 1000));
            }
            if (num < 1) {
                lineNumberLabel.textColor = new Color(1, 0, 0, 1);
                return;
            }
            bool numeroUsado = isNumeroUsado(num, m_lineIdSelecionado.TransportLine);

            if (numeroUsado) {
                lineNumberLabel.textColor = new Color(1, 0, 0, 1);
            } else {
                lineNumberLabel.textColor = new Color(1, 1, 1, 1);
                Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_lineNumber = num;
                m_linearMap.setLineNumberCircle(m_lineIdSelecionado.TransportLine);
                m_autoNameLabel.text = m_linearMap.autoName;
                if (prefixo != ModoNomenclatura.Nenhum) {
                    lineNumberLabel.text = (num % 1000).ToString();
                    linePrefixDropDown.selectedIndex = (num / 1000);
                } else {
                    lineNumberLabel.text = (num % 10000).ToString();
                }
                updateSliders();
            }
        }

        private void createInfoView()
        {

            //line info painel
            createMainPanel();

            createTitleBarItems();

            createLineInfoLabels();

            createRightLineActionButtons();

            createDayNightBudgetControls();

            agesPanel = new TLMAgesChartPanel(this);
            m_linearMap = new TLMLinearMap(this);
        }

        private void createDayNightBudgetControls()
        {
            DayNightInstantiateCheckBoxes();

            DayNightCreateActions();

            DayNightSetGroup();

            SetPosition();

            TLMUtils.createUIElement<UILabel>(ref lineBudgetSlidersTitle, lineInfoPanel.transform);
            lineBudgetSlidersTitle.autoSize = false;
            lineBudgetSlidersTitle.relativePosition = new Vector3(15f, 130f);
            lineBudgetSlidersTitle.width = 400f;
            lineBudgetSlidersTitle.height = 36f;
            lineBudgetSlidersTitle.textScale = 0.9f;
            lineBudgetSlidersTitle.textAlignment = UIHorizontalAlignment.Center;
            lineBudgetSlidersTitle.name = "LineBudgetSlidersTitle";
            lineBudgetSlidersTitle.font = UIHelperExtension.defaultFontCheckbox;
            lineBudgetSlidersTitle.wordWrap = true;

            for (int i = 0; i < budgetSliders.Length; i++) {
                budgetSliders[i] = GenerateVerticalBudgetMultiplierField(uiHelper, i);
            }
        }

        private void SetPosition()
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
            m_DayLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c) {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0) {
                    Singleton<SimulationManager>.instance.AddAction(delegate {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int) m_lineIdSelecionado.TransportLine], true, false);
                        m_linearMap.redrawLine();
                    });
                }
            };
            m_NightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c) {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0) {
                    Singleton<SimulationManager>.instance.AddAction(delegate {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int) m_lineIdSelecionado.TransportLine], false, true);
                        m_linearMap.redrawLine();
                    });
                }
            };
            m_DayNightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c) {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0) {
                    Singleton<SimulationManager>.instance.AddAction(delegate {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int) m_lineIdSelecionado.TransportLine], true, true);
                        m_linearMap.redrawLine();
                    });
                }
            };
            m_DisabledLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c) {
                if (Singleton<SimulationManager>.exists && m_lineIdSelecionado.TransportLine != 0) {
                    Singleton<SimulationManager>.instance.AddAction(delegate {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int) m_lineIdSelecionado.TransportLine], false, false);
                        m_linearMap.redrawLine();
                    });
                }
            };
        }

        private void DayNightInstantiateCheckBoxes()
        {
            m_DayLine = uiHelper.AddCheckboxLocale("TRANSPORT_LINE_DAY", false);
            m_NightLine = uiHelper.AddCheckboxLocale("TRANSPORT_LINE_NIGHT", false);
            m_DayNightLine = uiHelper.AddCheckboxLocale("TRANSPORT_LINE_DAYNNIGHT", false);
            m_DisabledLine = uiHelper.AddCheckboxLocale("TLM_TRANSPORT_LINE_DISABLED", false);
        }

        private void createRightLineActionButtons()
        {
            TLMUtils.createUIElement<UILabel>(ref m_autoNameLabel, lineInfoPanel.transform);
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
            deleteLine.relativePosition = new Vector3(lineInfoPanel.width - 150f, lineInfoPanel.height - 140f);
            deleteLine.textScale = 0.6f;
            deleteLine.width = 40;
            deleteLine.height = 40;
            deleteLine.tooltip = Locale.Get("LINE_DELETE");
            TLMUtils.initButton(deleteLine, true, "ButtonMenu");
            deleteLine.name = "DeleteLineButton";
            deleteLine.isVisible = true;
            deleteLine.eventClick += (component, eventParam) => {
                if (m_lineIdSelecionado.TransportLine != 0) {
                    ConfirmPanel.ShowModal("CONFIRM_LINEDELETE", delegate (UIComponent comp, int ret) {
                        if (ret == 1) {
                            Singleton<SimulationManager>.instance.AddAction(delegate {
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
            TLMUtils.createUIElement<UIButton>(ref buttonAutoName, lineInfoPanel.transform);
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.relativePosition = new Vector3(lineInfoPanel.width - 50f, lineInfoPanel.height - 140f);
            buttonAutoName.width = 40;
            buttonAutoName.height = 40;
            buttonAutoName.tooltip = Locale.Get("TLM_USE_AUTO_NAME");
            TLMUtils.initButton(buttonAutoName, true, "ButtonMenu");
            buttonAutoName.name = "AutoName";
            buttonAutoName.isVisible = true;
            buttonAutoName.eventClick += (component, eventParam) => {
                lineNameField.text = m_linearMap.autoName;
                saveLineName(lineNameField);
            };

            icon = buttonAutoName.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.spriteName = "AutoNameIcon";
            icon.width = 36;
            icon.height = 36;

            UIButton buttonAutoColor = null;
            TLMUtils.createUIElement<UIButton>(ref buttonAutoColor, lineInfoPanel.transform);
            buttonAutoColor.relativePosition = new Vector3(lineInfoPanel.width - 100f, lineInfoPanel.height - 140f);
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 40;
            buttonAutoColor.height = 40;
            buttonAutoColor.tooltip = Locale.Get("TLM_PICK_COLOR_FROM_PALETTE_TOOLTIP");
            TLMUtils.initButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.name = "AutoColor";
            buttonAutoColor.isVisible = true;
            buttonAutoColor.eventClick += (component, eventParam) => {
                lineColorPicker.selectedColor = m_controller.AutoColor(m_lineIdSelecionado.TransportLine);
                updateLineUI(lineColorPicker.selectedColor);
            };

            icon = buttonAutoColor.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "AutoColorIcon";

            TLMUtils.createUIElement<UIButton>(ref enableBudgetPerHour, lineInfoPanel.transform);
            enableBudgetPerHour.relativePosition = new Vector3(lineInfoPanel.width - 200f, lineInfoPanel.height - 140f);
            enableBudgetPerHour.textScale = 0.6f;
            enableBudgetPerHour.width = 40;
            enableBudgetPerHour.height = 40;
            enableBudgetPerHour.tooltip = Locale.Get("TLM_USE_PER_PERIOD_BUDGET");
            TLMUtils.initButton(enableBudgetPerHour, true, "ButtonMenu");
            enableBudgetPerHour.name = "EnableBudgetPerHour";
            enableBudgetPerHour.isVisible = true;
            enableBudgetPerHour.eventClick += (component, eventParam) => {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
                if (TLMLineUtils.hasPrefix(ref tl)) {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    uint prefix = tl.m_lineNumber / 1000u;
                    BasicTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                    uint[] saveData = bte.GetBudgetsMultiplier(prefix);
                    uint[] newSaveData = new uint[8];
                    for (int i = 0; i < 8; i++) {
                        newSaveData[i] = saveData[0];
                    }
                    bte.SetBudgetMultiplier(prefix, newSaveData);
                }
                updateSliders();
            };

            icon = enableBudgetPerHour.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "PerHourIcon";


            TLMUtils.createUIElement<UIButton>(ref disableBudgetPerHour, lineInfoPanel.transform);
            disableBudgetPerHour.relativePosition = new Vector3(lineInfoPanel.width - 200f, lineInfoPanel.height - 140f);
            disableBudgetPerHour.textScale = 0.6f;
            disableBudgetPerHour.width = 40;
            disableBudgetPerHour.height = 40;
            disableBudgetPerHour.tooltip = Locale.Get("TLM_USE_SINGLE_BUDGET");
            TLMUtils.initButton(disableBudgetPerHour, true, "ButtonMenu");
            disableBudgetPerHour.name = "DisableBudgetPerHour";
            disableBudgetPerHour.isVisible = true;
            disableBudgetPerHour.eventClick += (component, eventParam) => {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
                if (TLMLineUtils.hasPrefix(ref tl)) {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    uint prefix = tl.m_lineNumber / 1000u;
                    BasicTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                    uint[] saveData = bte.GetBudgetsMultiplier(prefix);
                    uint[] newSaveData = new uint[] { saveData[0] };
                    bte.SetBudgetMultiplier(prefix, newSaveData);
                }
                updateSliders();
            };

            icon = disableBudgetPerHour.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "24hLineIcon";

            TLMUtils.createUIElement<UIButton>(ref goToWorldInfoPanel, lineInfoPanel.transform);
            goToWorldInfoPanel.relativePosition = new Vector3(lineInfoPanel.width - 200f, lineInfoPanel.height - 140f);
            goToWorldInfoPanel.text = "IPT2";
            goToWorldInfoPanel.textScale = 0.6f;
            goToWorldInfoPanel.width = 40;
            goToWorldInfoPanel.height = 40;
            goToWorldInfoPanel.tooltip = Locale.Get("TLM_GO_TO_WORLD_INFO_PANEL_LINE");
            TLMUtils.initButton(goToWorldInfoPanel, true, "ButtonMenu");
            goToWorldInfoPanel.name = "IPT2WorldInfoButton";
            goToWorldInfoPanel.isVisible = true;
            goToWorldInfoPanel.eventClick += (component, eventParam) => {
                WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(Vector3.zero, m_lineIdSelecionado);
            };
        }

        private void createLineInfoLabels()
        {
            TLMUtils.createUIElement<UILabel>(ref lineLenghtLabel, lineInfoPanel.transform);
            lineLenghtLabel.autoSize = false;
            lineLenghtLabel.relativePosition = new Vector3(10f, 45f);
            lineLenghtLabel.textAlignment = UIHorizontalAlignment.Left;
            lineLenghtLabel.text = "";
            lineLenghtLabel.width = 550;
            lineLenghtLabel.height = 25;
            lineLenghtLabel.prefix = "";
            lineLenghtLabel.suffix = "";
            lineLenghtLabel.name = "LineLenghtLabel";
            lineLenghtLabel.textScale = 0.6f;
            lineLenghtLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement<UILabel>(ref veiculosLinhaLabel, lineInfoPanel.transform);
            veiculosLinhaLabel.autoSize = false;
            veiculosLinhaLabel.relativePosition = new Vector3(10f, 55);
            veiculosLinhaLabel.textAlignment = UIHorizontalAlignment.Left;
            veiculosLinhaLabel.text = "";
            veiculosLinhaLabel.width = 550;
            veiculosLinhaLabel.height = 25;
            veiculosLinhaLabel.name = "VehiclesLineLabel";
            veiculosLinhaLabel.textScale = 0.6f;
            veiculosLinhaLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement<UILabel>(ref viagensEvitadasLabel, lineInfoPanel.transform);
            viagensEvitadasLabel.autoSize = false;
            viagensEvitadasLabel.relativePosition = new Vector3(10f, 65);
            viagensEvitadasLabel.textAlignment = UIHorizontalAlignment.Left;
            viagensEvitadasLabel.text = "";
            viagensEvitadasLabel.width = 250;
            viagensEvitadasLabel.height = 25;
            viagensEvitadasLabel.name = "AvoidedTravelsLabel";
            viagensEvitadasLabel.textScale = 0.6f;
            viagensEvitadasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement<UILabel>(ref passageirosEturistasLabel, lineInfoPanel.transform);
            passageirosEturistasLabel.autoSize = false;
            passageirosEturistasLabel.relativePosition = new Vector3(10f, 75f);
            passageirosEturistasLabel.textAlignment = UIHorizontalAlignment.Left;
            passageirosEturistasLabel.text = "";
            passageirosEturistasLabel.width = 350;
            passageirosEturistasLabel.height = 25;
            passageirosEturistasLabel.name = "TouristAndPassagersLabel";
            passageirosEturistasLabel.textScale = 0.6f;
            passageirosEturistasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement<UILabel>(ref budgetLabel, lineInfoPanel.transform);
            budgetLabel.autoSize = false;
            budgetLabel.relativePosition = new Vector3(10f, 85f);
            budgetLabel.textAlignment = UIHorizontalAlignment.Left;
            budgetLabel.width = 550;
            budgetLabel.height = 25;
            budgetLabel.name = "ExtraInfoLabel";
            budgetLabel.textScale = 0.6f;
            budgetLabel.prefix = Locale.Get("TLM_LINE_EFFECTIVE_BUDGET") + ": ";
            budgetLabel.font = UIHelperExtension.defaultFontCheckbox;

            //TLMUtils.createUIElement<UILabel>(ref generalDebugLabel, lineInfoPanel.transform);
            //generalDebugLabel.autoSize = false;
            //generalDebugLabel.relativePosition = new Vector3(10f, 185f);
            //generalDebugLabel.textAlignment = UIHorizontalAlignment.Left;
            //generalDebugLabel.prefix = "DEBUG: DATA AVAIL = ";
            //generalDebugLabel.width = 350;
            //generalDebugLabel.height = 100;
            //generalDebugLabel.name = "CustosLabel";
            //generalDebugLabel.textScale = 0.8f;
            //generalDebugLabel.wordWrap = true;
            //generalDebugLabel.clipChildren = false;
            //generalDebugLabel.enabled = false && TransportLinesManagerMod.debugMode.value;
        }

        private void createTitleBarItems()
        {


            TLMUtils.createUIElement<UILabel>(ref lineTransportIconTypeLabel, lineInfoPanel.transform);
            lineTransportIconTypeLabel.autoSize = false;
            lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            lineTransportIconTypeLabel.width = 30;
            lineTransportIconTypeLabel.height = 20;
            lineTransportIconTypeLabel.name = "LineTransportIcon";
            lineTransportIconTypeLabel.clipChildren = true;
            TLMUtils.createDragHandle(lineTransportIconTypeLabel, lineInfoPanel);

            GameObject lpddgo = GameObject.Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject);
            linePrefixDropDown = lpddgo.GetComponent<UIDropDown>();
            lineInfoPanel.AttachUIComponent(linePrefixDropDown.gameObject);
            linePrefixDropDown.isLocalized = false;
            linePrefixDropDown.autoSize = false;
            linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;
            linePrefixDropDown.text = "";
            linePrefixDropDown.width = 40;
            linePrefixDropDown.height = 30;
            linePrefixDropDown.name = "LinePrefixDropDown";
            linePrefixDropDown.textScale = 1.6f;
            linePrefixDropDown.itemHeight = 35;
            linePrefixDropDown.itemPadding = new RectOffset(2, 2, 2, 2);
            linePrefixDropDown.textFieldPadding = new RectOffset(2, 2, 2, 2);
            linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            linePrefixDropDown.relativePosition = new Vector3(70f, 5f);
            linePrefixDropDown.normalBgSprite = "OptionsDropboxListbox";
            linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;


            TLMUtils.createUIElement<UITextField>(ref lineNumberLabel, lineInfoPanel.transform);
            lineNumberLabel.autoSize = false;
            lineNumberLabel.relativePosition = new Vector3(80f, 5f);
            lineNumberLabel.horizontalAlignment = UIHorizontalAlignment.Center;
            lineNumberLabel.text = "";
            lineNumberLabel.width = 75;
            lineNumberLabel.height = 30;
            lineNumberLabel.name = "LineNumberLabel";
            lineNumberLabel.normalBgSprite = "EmptySprite";
            lineNumberLabel.textScale = 1.6f;
            lineNumberLabel.padding = new RectOffset(0, 0, 0, 0);
            lineNumberLabel.color = new Color(0, 0, 0, 1);
            TLMUtils.uiTextFieldDefaults(lineNumberLabel);
            lineNumberLabel.numericalOnly = true;
            lineNumberLabel.maxLength = 4;
            lineNumberLabel.eventLostFocus += saveLineNumber;
            lineNumberLabel.zOrder = 10;


            TLMUtils.createUIElement<UITextField>(ref lineNameField, lineInfoPanel.transform);
            lineNameField.autoSize = false;
            lineNameField.relativePosition = new Vector3(190f, 11f);
            lineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
            lineNameField.text = "NOME";
            lineNameField.width = 410;
            lineNameField.height = 18;
            lineNameField.name = "LineNameLabel";
            lineNameField.maxLength = 256;
            lineNameField.textScale = 1f;
            TLMUtils.uiTextFieldDefaults(lineNameField);
            lineNameField.eventGotFocus += (component, eventParam) => {
                lastLineName = lineNameField.text;
            };
            lineNameField.eventLostFocus += (component, eventParam) => {
                if (lastLineName != lineNameField.text) {
                    saveLineName(lineNameField);
                }
                lineNameField.text = m_controller.tm.GetLineName(m_lineIdSelecionado.TransportLine);
            };


            lineColorPicker = GameObject.Instantiate(PublicTransportWorldInfoPanel.FindObjectOfType<UIColorField>().gameObject).GetComponent<UIColorField>();
            lineInfoPanel.AttachUIComponent(lineColorPicker.gameObject);
            lineColorPicker.name = "LineColorPicker";
            lineColorPicker.relativePosition = new Vector3(42f, 10f);
            lineColorPicker.enabled = true;
            lineColorPicker.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
            lineColorPicker.eventSelectedColorChanged += (UIComponent component, Color value) => {
                TLMUtils.setLineColor(m_lineIdSelecionado.TransportLine, value);
                updateLineUI(value);
            };


            UIButton voltarButton2 = null;
            TLMUtils.createUIElement<UIButton>(ref voltarButton2, lineInfoPanel.transform);
            voltarButton2.relativePosition = new Vector3(lineInfoPanel.width - 40f, 5f);
            voltarButton2.width = 30;
            voltarButton2.height = 30;
            TLMUtils.initButton(voltarButton2, true, "buttonclose", true);
            voltarButton2.name = "LineInfoCloseButton";
            voltarButton2.eventClick += closeLineInfo;
        }

        private void createMainPanel()
        {
            TLMUtils.createUIElement<UIPanel>(ref lineInfoPanel, m_controller.mainRef.transform);
            lineInfoPanel.Hide();
            lineInfoPanel.relativePosition = new Vector3(394.0f, 0.0f);
            lineInfoPanel.width = 650;
            lineInfoPanel.height = 290;
            lineInfoPanel.zOrder = 50;
            lineInfoPanel.color = new Color32(255, 255, 255, 255);
            lineInfoPanel.backgroundSprite = "MenuPanel2";
            lineInfoPanel.name = "LineInfoPanel";
            lineInfoPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            lineInfoPanel.autoLayout = false;
            lineInfoPanel.useCenter = true;
            lineInfoPanel.wrapLayout = false;
            lineInfoPanel.canFocus = true;
            TLMUtils.createDragHandle(lineInfoPanel, lineInfoPanel, 35f);
            lineInfoPanel.eventVisibilityChanged += (component, value) => {
                if (m_linearMap != null) {
                    m_linearMap.isVisible = value;
                }
            };

            uiHelper = new UIHelperExtension(lineInfoPanel);
        }

        private void setBudgetHour(float x, int selectedHourIndex)
        {
            TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine];
            ushort val = (ushort) (x * 100 + 0.5f);
            if (TLMLineUtils.hasPrefix(ref tl)) {
                var tsd = TransportSystemDefinition.from(tl.Info);
                uint prefix = tl.m_lineNumber / 1000u;
                BasicTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                uint[] saveData = bte.GetBudgetsMultiplier(prefix);
                if (selectedHourIndex >= saveData.Length || saveData[selectedHourIndex] == val) {
                    return;
                }
                saveData[selectedHourIndex] = val;
                bte.SetBudgetMultiplier(prefix, saveData);
            } else {
                Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].m_budget = val;
            }
        }

        private UISlider GenerateVerticalBudgetMultiplierField(UIHelperExtension uiHelper, int idx)
        {
            UISlider bugdetSlider = (UISlider) uiHelper.AddSlider(Locale.Get("TLM_BUDGET_MULTIPLIER_LABEL"), 0f, 5, 0.05f, -1,
                (x) => {

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
            ((UISprite) bugdetSlider.thumbObject).spriteName = "ScrollbarThumb";
            ((UISprite) bugdetSlider.thumbObject).color = new Color32(1, 140, 46, 255);

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
            bugdetSlider.eventValueChanged += delegate (UIComponent c, float val) {
                budgetSliderLabel.text = string.Format(" x{0:0.00}", val);
                setBudgetHour(val, idx_loc);
            };

            return bugdetSlider;
        }


        private void updateLineUI(Color color)
        {
            lineNumberLabel.color = color;
            m_linearMap.setLinearMapColor(color);
        }

        public void updateBidings()
        {
            ushort lineID = m_lineIdSelecionado.TransportLine;
            TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID];
            TransportInfo info = tl.Info;
            int turistas = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_touristPassengers.m_averageCount;
            int residentes = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_residentPassengers.m_averageCount;
            int residentesPorc = residentes;
            if (residentesPorc == 0)
                residentesPorc = 1;
            int criancas = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_childPassengers.m_averageCount;
            int adolescentes = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_teenPassengers.m_averageCount;
            int jovens = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_youngPassengers.m_averageCount;
            int adultos = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_adultPassengers.m_averageCount;
            int idosos = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_seniorPassengers.m_averageCount;
            int motoristas = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID].m_passengers.m_carOwningPassengers.m_averageCount;
            int veiculosLinha = TLMLineUtils.GetVehiclesCount(lineID);
            int porcCriancas = (criancas * 100 / residentesPorc);
            int porcAdolescentes = (adolescentes * 100 / residentesPorc);
            int porcJovens = (jovens * 100 / residentesPorc);
            int porcAdultos = (adultos * 100 / residentesPorc);
            int porcIdosos = (idosos * 100 / residentesPorc);
            int soma = porcCriancas + porcAdolescentes + porcJovens + porcAdultos + porcIdosos;
            if (soma != 0 && soma != 100) {
                porcAdultos = 100 - (porcCriancas + porcAdolescentes + porcJovens + porcIdosos);
            }
            agesPanel.SetValues(new int[]
                                 {
                porcCriancas,
                porcAdolescentes,
                porcJovens,
                porcAdultos,
                porcIdosos
            });
            passageirosEturistasLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_PASSENGERS", new object[]
                                                                           {
                residentes,
                turistas
            });

            int viagensSalvas = 0;
            int coeficienteViagens = 0;
            if (residentes + turistas != 0) {
                coeficienteViagens += criancas * 0;
                coeficienteViagens += adolescentes * 5;
                coeficienteViagens += jovens * ((15 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
                coeficienteViagens += adultos * ((20 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
                coeficienteViagens += idosos * ((10 * residentes + 20 * turistas + (residentes + turistas >> 1)) / (residentes + turistas));
            }
            if (coeficienteViagens != 0) {
                viagensSalvas = (int) (((long) motoristas * 10000L + (long) (coeficienteViagens >> 1)) / (long) coeficienteViagens);
                viagensSalvas = Mathf.Clamp(viagensSalvas, 0, 100);
            }
            viagensEvitadasLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_TRIPSAVED", new object[]{
                viagensSalvas
            });

            if (daytimeChange != null && daytimeChange.completedOrFailed) {
                linearMap.redrawLine();
                daytimeChange = null;
            } else {
                linearMap.updateBidings();
            }


            //lines info
            int stopsCount = TLMLineUtils.GetStopsCount(lineID);
            if (lastStopsCount != stopsCount) {
                float totalSize = TLMLineUtils.GetLineLength(lineID);
                lineLenghtLabel.text = string.Format(Locale.Get("TLM_LENGHT_AND_STOPS"), totalSize, stopsCount);
                lastStopsCount = stopsCount;
            }

            //estatisticas novas
            veiculosLinhaLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_VEHICLECOUNT", new object[] { veiculosLinha });

            uint prefix = 0;
            if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportInfo(info) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int) ModoNomenclatura.Nenhum) {
                prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
            }

            float baseBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;

            budgetLabel.text = string.Format("{0:0%} ({1:0%})", getEffectiveBudget(), Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f + 0.004f);//585+1/7 = frames/week                ;
            budgetLabel.tooltip = string.Format(Locale.Get("TLM_LINE_BUDGET_EXPLAIN_2"),
                TLMCW.getNameForTransportType(TLMCW.getConfigIndexForTransportInfo(info)),
                baseBudget, Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f + 0.004f, getEffectiveBudget());

            //bool isZeroed = ((int)tl.m_flags & (int)TLMTransportLineFlags.ZERO_BUDGET_SETTED) > 0;
            //lineTime.isVisible = !isZeroed;
            //if (isZeroed)
            //{
            //    lineTimeTitle.localeID = ("TLM_LINE_DISABLED_NO_BUDGET");
            //    lineTimeTitle.tooltipLocaleID = ("TLM_LINE_DISABLED_NO_BUDGET_DESC");
            //}
            //else
            //{
            //    lineTimeTitle.localeID = ("TRANSPORT_LINE_ACTIVITY");
            //    lineTimeTitle.tooltipLocaleID = ("");
            //}

            //generalDebugLabel.enabled = TransportLinesManagerMod.debugMode.value;
            //if (TransportLinesManagerMod.debugMode.value)
            //{
            //    string debugTxt = "!";
            //    var extraDatas = ExtraVehiclesStats.instance.getLineVehiclesData(lineID);
            //    if (extraDatas.Count == 0)
            //    {
            //        debugTxt = "none";
            //    }
            //    else
            //    {
            //        foreach (var item in extraDatas)
            //        {
            //            debugTxt += string.Format("BUS ID {0} - {1} Fill, {2} per lap ||", item.Key, item.Value.avgFill.ToString("0.00%"), string.Format("{0}d {1}h{2}m", item.Value.timeTakenLap.TotalDays, item.Value.timeTakenLap.Hours, item.Value.timeTakenLap.Minutes));
            //        }
            //    }

            //    generalDebugLabel.text = debugTxt;
            //}

            //			//custos da linha
            //			float costVehicles = 0;
            //			ushort nextVehId = tl.m_vehicles;
            //			while (nextVehId >0) {
            //				costVehicles += Singleton<VehicleManager>.instance.m_vehicles.m_buffer [(int)nextVehId].Info.GetMaintenanceCost () ;
            //				nextVehId = Singleton<VehicleManager>.instance.m_vehicles.m_buffer [(int)nextVehId].m_nextLineVehicle;
            //			}
            //			float costStops = 0;
            //			int a = tl.m_stops;
            //			for (int i = 0; i< stopsCount; i++) {
            //				costStops += Singleton<NetManager>.instance.m_nodes.m_buffer [(int)tl.GetStop (i)].Info.GetMaintenanceCost () ;
            //			}
            //			custosLabel.text = String.Format (costsFormat, costVehicles, costStops, costVehicles + costStops);

        }



        public void closeLineInfo(UIComponent component, UIMouseEventParameter eventParam)
        {
            TransportLine t = m_controller.tm.m_lines.m_buffer[(int) m_lineIdSelecionado.TransportLine];
            Hide();
            m_controller.defaultListingLinesPanel.Show();
            TLMPublicTransportDetailPanel.instance.SetActiveTab(Array.IndexOf(TLMPublicTransportDetailPanel.tabSystemOrder, TLMCW.getConfigIndexForTransportInfo(t.Info)));
        }

        public void openLineInfo(UIComponent component, UIMouseEventParameter eventParam)
        {
            ushort lineID = (component as UIButtonLineInfo).lineID;
            if (lineID > 0) {
                openLineInfo(lineID);
            }

        }

        public void openLineInfo(ushort lineID)
        {
            var tsd = TLMCW.getDefinitionForLine(lineID);
            if (lineID <= 0 || tsd == default(TransportSystemDefinition)) {
                return;
            }
            WorldInfoPanel.HideAllWorldInfoPanels();
            linePrefixDropDown.eventSelectedIndexChanged -= saveLineNumber;
            lineNumberLabel.eventLostFocus -= saveLineNumber;

            m_lineIdSelecionado = default(InstanceID);
            m_lineIdSelecionado.TransportLine = lineID;

            TransportLine t = m_controller.tm.m_lines.m_buffer[(int) lineID];
            ushort lineNumber = t.m_lineNumber;

            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            ModoNomenclatura mnPrefixo = (ModoNomenclatura) TLMCW.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.PREFIX | transportType);

            if (mnPrefixo != ModoNomenclatura.Nenhum) {
                lineNumberLabel.text = (lineNumber % 1000).ToString();
                lineNumberLabel.relativePosition = new Vector3(110f, 5f);
                lineNumberLabel.width = 55;
                linePrefixDropDown.enabled = false;

                var temp = TLMUtils.getStringOptionsForPrefix(mnPrefixo);
                linePrefixDropDown.items = temp;
                linePrefixDropDown.selectedIndex = lineNumber / 1000;
                linePrefixDropDown.enabled = true;
                lineNumberLabel.maxLength = 3;


            } else {
                lineNumberLabel.text = (lineNumber).ToString();
                lineNumberLabel.relativePosition = new Vector3(80f, 5f);
                lineNumberLabel.width = 75;
                lineNumberLabel.maxLength = 4;
                linePrefixDropDown.enabled = false;
            }



            lineNumberLabel.color = m_controller.tm.GetLineColor(lineID);
            lineNameField.text = m_controller.tm.GetLineName(lineID);

            lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            lineTransportIconTypeLabel.height = 20;
            lineTransportIconTypeLabel.atlas = linePrefixDropDown.atlas;
            lineTransportIconTypeLabel.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(t.Info.m_transportType);

            lineColorPicker.selectedColor = m_controller.tm.GetLineColor(lineID);

            TLMLineUtils.getLineActive(ref t, out bool day, out bool night);
            m_DayNightLine.isChecked = false;
            m_NightLine.isChecked = false;
            m_DayLine.isChecked = false;
            m_DisabledLine.isChecked = false;
            if (day && night) {
                m_DayNightLine.isChecked = true;
            } else if (day) {
                m_DayLine.isChecked = true;
            } else if (night) {
                m_NightLine.isChecked = true;
            } else {
                m_DisabledLine.isChecked = true;
            }
            updateSliders();
            m_linearMap.redrawLine();
            m_autoNameLabel.text = m_linearMap.autoName;
            lineInfoPanel.color = Color.Lerp(TLMCW.getColorForTransportType(transportType), Color.white, 0.4f);

            Show();
            m_controller.defaultListingLinesPanel.Hide();
            m_controller.depotInfoPanel.Hide();

            linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            lineNumberLabel.eventLostFocus += saveLineNumber;


        }

        private void changeLineTime(int selection)
        {
            Singleton<SimulationManager>.instance.AddAction(delegate {
                ushort lineID = m_lineIdSelecionado.TransportLine;
                TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int) lineID], ((selection & 0x2) == 0), ((selection & 0x1) == 0));
            });
        }

        private void updateSliders()
        {
            if (TransportLinesManagerMod.isIPTLoaded) {
                goToWorldInfoPanel.isVisible = true;
                disableBudgetPerHour.isVisible = false;
                enableBudgetPerHour.isVisible = false;
                for (int i = 0; i < budgetSliders.Length; i++) {
                    budgetSliders[i].isEnabled = false;
                    budgetSliders[i].parent.isVisible = false;
                }
                lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_IPT2_NO_BUDGET_CONTROL"));

                return;
            } else {
                goToWorldInfoPanel.isVisible = false;
            }
            var tsd = TLMCW.getDefinitionForLine(m_lineIdSelecionado.TransportLine);
            if (m_lineIdSelecionado.TransportLine <= 0 || tsd == default(TransportSystemDefinition)) {
                return;
            }
            TransportLine t = m_controller.tm.m_lines.m_buffer[(int) m_lineIdSelecionado.TransportLine];
            ushort lineNumber = t.m_lineNumber;

            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            ModoNomenclatura mnPrefixo = (ModoNomenclatura) TLMCW.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.PREFIX | transportType);

            if (mnPrefixo != ModoNomenclatura.Nenhum) {
                uint prefix = t.m_lineNumber / 1000u;
                BasicTransportExtension bte = TLMUtils.getExtensionFromTransportSystemDefinition(tsd);
                uint[] multipliers = bte.GetBudgetsMultiplier(prefix);
                disableBudgetPerHour.isVisible = multipliers.Length == 8;
                enableBudgetPerHour.isVisible = multipliers.Length == 1;
                for (int i = 0; i < budgetSliders.Length; i++) {
                    UILabel budgetSliderLabel = budgetSliders[i].transform.parent.GetComponentInChildren<UILabel>();
                    if (i == 0) {
                        if (multipliers.Length == 1) {
                            budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL_ALL");
                        } else {
                            budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL", 0);
                        }
                    } else {
                        budgetSliders[i].isEnabled = multipliers.Length == 8;
                        budgetSliders[i].parent.isVisible = multipliers.Length == 8;
                    }

                    if (i < multipliers.Length) {
                        budgetSliders[i].value = multipliers[i] / 100f;
                    }
                }
                lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_PREFIX"), prefix > 0 ? TLMUtils.getStringFromNumber(TLMUtils.getStringOptionsForPrefix(mnPrefixo), (int) prefix + 1) : Locale.Get("TLM_UNPREFIXED"), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
            } else {
                disableBudgetPerHour.isVisible = false;
                enableBudgetPerHour.isVisible = false;
                for (int i = 0; i < budgetSliders.Length; i++) {
                    if (i == 0) {
                        UILabel budgetSliderLabel = budgetSliders[i].transform.parent.GetComponentInChildren<UILabel>();
                        budgetSliderLabel.prefix = Locale.Get("TLM_BUDGET_MULTIPLIER_PERIOD_LABEL_ALL");
                        budgetSliders[i].value = t.m_budget / 100f;
                    } else {
                        budgetSliders[i].isEnabled = false;
                        budgetSliders[i].parent.isVisible = false;
                    }
                }
                lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_LINE"), TLMLineUtils.getLineStringId(m_lineIdSelecionado.TransportLine), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
            }
        }

        public override void OnRenameStationAction(string autoName)
        {
            autoNameLabel.text = autoName;
        }
    }
}

