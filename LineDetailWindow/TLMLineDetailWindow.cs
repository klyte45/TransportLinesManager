using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using Klyte.TransportLinesManager.CommonsWindow;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.LineDetailWindow.Components;
using Klyte.TransportLinesManager.TextureAtlas;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.LineDetailWindow
{
    internal class TLMLineDetailWindow : UIPanel, ILinearMapParentInterface, IBudgetControlParentInterface
    {
        private TLMAgesChartPanel m_agesPanel;
        private TLMLinearMap m_linearMap;
        private int m_lastStopsCount = 0;

        //line info	
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
        private UIButton m_goToWorldInfoPanel;

        private UIHelperExtension m_uiHelper;

        private TLMBudgetControlSliders m_budgetSliders;

        private UIDropDown m_firstStopSelect;
        private UITextField m_ticketPriceEditor;

        private TLMAssetSelectorWindow m_assetSelectorWindow;
        public TLMAssetSelectorWindow assetSelectorWindow => m_assetSelectorWindow;

        #region Getters
        public UILabel autoNameLabel => m_autoNameLabel;

        public Transform TransformLinearMap => this.transform;

        public UIPanel mainPanel => this;

        public TLMLinearMap linearMap => m_linearMap;

        public InstanceID lineIdSelecionado => m_lineIdSelecionado;

        public CameraController cameraController => m_CameraController;

        public ushort CurrentSelectedId => m_lineIdSelecionado.TransportLine;

        public bool CanSwitchView => true;

        public bool ForceShowStopsDistances => false;

        public TransportInfo CurrentTransportInfo => Singleton<TransportManager>.instance.m_lines.m_buffer[CurrentSelectedId].Info;

        public bool PrefixSelectionMode => false;

        public TransportSystemDefinition TransportSystem => TransportSystemDefinition.from(Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineIdSelecionado.TransportLine].Info);
        #endregion

        #region Instantiation
        public override void Awake()
        {
            base.Awake();
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

            CreateFirstStopSelector();

            CreateTicketPriceEditor();

            CreateBudgetSliders();

            TLMUtils.doLog("AGES");
            TLMUtils.createElement(out m_agesPanel, this.transform);

            TLMUtils.doLog("LINEAR MAP");
            TLMUtils.createElement(out m_linearMap, transform);
            m_linearMap.parent = this;


            TLMUtils.doLog("ASSET SEL");
            TLMUtils.createElement(out m_assetSelectorWindow, transform);
            m_assetSelectorWindow.lineInfo = this;
        }

        private void CreateBudgetSliders()
        {
            TLMUtils.doLog("SLIDERS");
            TLMUtils.createElement(out m_budgetSliders, this.transform, "Budget Sliders");
            m_budgetSliders.mainPanel.relativePosition = new Vector2(0, 100);
        }

        public override void Start()
        {
            base.Start();
            TLMUtils.doLog("SLIDERS EVENTS");
            m_budgetSliders.onIgnorePrefixChanged += onIgnorePrefixToggle;
            m_budgetSliders.onDayNightChanged += m_linearMap.redrawLine;
        }

        private void onIgnorePrefixToggle(ushort lineId)
        {
            m_linearMap.setLineNumberCircle(lineId);
            UpdateTicketPrice();
            onLineChanged?.Invoke(lineId);
            m_budgetLabel.isVisible = m_budgetSliders.IgnorePrefix;
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





        private void createRightLineActionButtons()
        {
            TLMUtils.createUIElement(out m_autoNameLabel, this.transform);
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

            TLMUtils.createUIElement(out UIButton deleteLine, this.transform);
            deleteLine.relativePosition = new Vector3(this.width - 150f, this.height - 230f);
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
            icon.atlas = TLMCommonTextureAtlas.instance.atlas;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "RemoveUnwantedIcon";
            icon.color = Color.red;

            //Auto color & Auto Name
            TLMUtils.createUIElement(out UIButton buttonAutoName, this.transform);
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.relativePosition = new Vector3(this.width - 50f, this.height - 230f);
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
            icon.atlas = TLMCommonTextureAtlas.instance.atlas;
            icon.spriteName = "AutoNameIcon";
            icon.width = 36;
            icon.height = 36;

            TLMUtils.createUIElement(out UIButton buttonAutoColor, this.transform);
            buttonAutoColor.relativePosition = new Vector3(this.width - 100f, this.height - 230f);
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 40;
            buttonAutoColor.height = 40;
            buttonAutoColor.tooltip = Locale.Get("TLM_PICK_COLOR_FROM_PALETTE_TOOLTIP");
            TLMUtils.initButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.name = "AutoColor";
            buttonAutoColor.isVisible = true;
            buttonAutoColor.eventClick += (component, eventParam) =>
            {
                m_lineColorPicker.selectedColor = TLMController.instance.AutoColor(m_lineIdSelecionado.TransportLine);
                updateLineUI(m_lineColorPicker.selectedColor);
            };

            icon = buttonAutoColor.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMCommonTextureAtlas.instance.atlas;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "AutoColorIcon";


        }

        private void createLineInfoLabels()
        {
            TLMUtils.createUIElement(out m_lineLenghtLabel, this.transform);
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

            TLMUtils.createUIElement(out m_veiculosLinhaLabel, this.transform);
            m_veiculosLinhaLabel.autoSize = false;
            m_veiculosLinhaLabel.relativePosition = new Vector3(10f, 55);
            m_veiculosLinhaLabel.textAlignment = UIHorizontalAlignment.Left;
            m_veiculosLinhaLabel.text = "";
            m_veiculosLinhaLabel.width = 550;
            m_veiculosLinhaLabel.height = 25;
            m_veiculosLinhaLabel.name = "VehiclesLineLabel";
            m_veiculosLinhaLabel.textScale = 0.6f;
            m_veiculosLinhaLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out m_viagensEvitadasLabel, this.transform);
            m_viagensEvitadasLabel.autoSize = false;
            m_viagensEvitadasLabel.relativePosition = new Vector3(10f, 65);
            m_viagensEvitadasLabel.textAlignment = UIHorizontalAlignment.Left;
            m_viagensEvitadasLabel.text = "";
            m_viagensEvitadasLabel.width = 250;
            m_viagensEvitadasLabel.height = 25;
            m_viagensEvitadasLabel.name = "AvoidedTravelsLabel";
            m_viagensEvitadasLabel.textScale = 0.6f;
            m_viagensEvitadasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out m_passageirosEturistasLabel, this.transform);
            m_passageirosEturistasLabel.autoSize = false;
            m_passageirosEturistasLabel.relativePosition = new Vector3(10f, 75f);
            m_passageirosEturistasLabel.textAlignment = UIHorizontalAlignment.Left;
            m_passageirosEturistasLabel.text = "";
            m_passageirosEturistasLabel.width = 350;
            m_passageirosEturistasLabel.height = 25;
            m_passageirosEturistasLabel.name = "TouristAndPassagersLabel";
            m_passageirosEturistasLabel.textScale = 0.6f;
            m_passageirosEturistasLabel.font = UIHelperExtension.defaultFontCheckbox;

            TLMUtils.createUIElement(out m_budgetLabel, this.transform);
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


            TLMUtils.createUIElement(out m_lineTransportIconTypeLabel, this.transform);
            m_lineTransportIconTypeLabel.autoSize = false;
            m_lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            m_lineTransportIconTypeLabel.width = 30;
            m_lineTransportIconTypeLabel.height = 20;
            m_lineTransportIconTypeLabel.name = "LineTransportIcon";
            m_lineTransportIconTypeLabel.clipChildren = true;
            TLMUtils.createDragHandle(m_lineTransportIconTypeLabel, this);

            GameObject lpddgo = GameObject.Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject, this.transform);
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


            TLMUtils.createUIElement(out m_lineNumberLabel, this.transform);
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


            TLMUtils.createUIElement(out m_lineNameField, this.transform);
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
                m_lineNameField.text = TransportManager.instance.GetLineName(m_lineIdSelecionado.TransportLine);
            };


            m_lineColorPicker = TLMUtils.CreateColorField(this);
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


            TLMUtils.createUIElement(out UIButton voltarButton2, this.transform);
            voltarButton2.relativePosition = new Vector3(this.width - 40f, 5f);
            voltarButton2.width = 30;
            voltarButton2.height = 30;
            TLMUtils.initButton(voltarButton2, true, "buttonclose", true);
            voltarButton2.name = "LineInfoCloseButton";
            voltarButton2.eventClick += closeLineInfo;
        }

        private void createMainPanel()
        {
            this.Hide();
            this.relativePosition = new Vector3(394.0f, 0.0f);
            this.width = 650;
            this.height = 380;
            this.zOrder = 50;
            this.color = new Color32(255, 255, 255, 255);
            this.backgroundSprite = "MenuPanel2";
            this.name = "LineInfoPanel";
            this.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            this.autoLayout = false;
            this.useCenter = true;
            this.wrapLayout = false;
            this.canFocus = true;
            TLMUtils.createDragHandle(this, this, 35f);
            this.eventVisibilityChanged += (component, value) =>
            {
                m_linearMap?.setVisible(value);
            };

            m_uiHelper = new UIHelperExtension(this);
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
                CallChildrenUpdateEvents(m_lineIdSelecionado.TransportLine);
            }
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
        public new void Show()
        {
            if (!GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible)
            {
                GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible = true;
            }
            if (Singleton<ToolController>.instance.CurrentTool.GetType() == typeof(TransportTool))
            {
                Singleton<ToolController>.instance.CurrentTool = Singleton<DefaultTool>.instance;
            }
            base.Show();
        }

        public void closeLineInfo(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
            TLMController.instance.OpenTLMPanel();
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
            global::WorldInfoPanel.HideAllWorldInfoPanels();
            m_linePrefixDropDown.eventSelectedIndexChanged -= saveLineNumber;
            m_lineNumberLabel.eventLostFocus -= saveLineNumber;

            m_lineIdSelecionado = default(InstanceID);
            m_lineIdSelecionado.TransportLine = lineID;

            TransportLine t = TransportManager.instance.m_lines.m_buffer[(int)lineID];
            ushort lineNumber = t.m_lineNumber;

            TLMCW.ConfigIndex transportType = tsd.toConfigIndex();
            ModoNomenclatura mnPrefixo = (ModoNomenclatura)TLMCW.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.PREFIX | transportType);

            if (TLMLineUtils.hasPrefix(lineID))
            {
                m_lineNumberLabel.text = (lineNumber % 1000).ToString();
                m_lineNumberLabel.relativePosition = new Vector3(110f, 5f);
                m_lineNumberLabel.width = 55;
                m_linePrefixDropDown.enabled = false;

                var temp = TLMUtils.getStringOptionsForPrefix(transportType, true, true, false);
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



            m_lineNumberLabel.color = TransportManager.instance.GetLineColor(lineID);
            m_lineNameField.text = TransportManager.instance.GetLineName(lineID);

            m_lineTransportIconTypeLabel.relativePosition = new Vector3(10f, 12f);
            m_lineTransportIconTypeLabel.height = 20;
            m_lineTransportIconTypeLabel.atlas = m_linePrefixDropDown.atlas;
            m_lineTransportIconTypeLabel.backgroundSprite = PublicTransportWorldInfoPanel.GetVehicleTypeIcon(t.Info.m_transportType);

            m_lineColorPicker.selectedColor = TransportManager.instance.GetLineColor(lineID);


            m_linearMap.redrawLine();
            m_autoNameLabel.text = m_linearMap.autoName;
            this.color = Color.Lerp(TLMCW.getColorForTransportType(transportType), Color.white, 0.4f);

            Show();
            TLMController.instance.CloseTLMPanel();

            m_linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            m_lineNumberLabel.eventLostFocus += saveLineNumber;
            m_firstStopSelect.items = TLMLineUtils.getAllStopsFromLine(lineID);
            m_firstStopSelect.selectedIndex = 0;

            UpdateTicketPrice();

            CallChildrenUpdateEvents(lineID);
            SetViewMode();
        }

        private void CallChildrenUpdateEvents(ushort lineID)
        {
            onLineChanged?.Invoke(lineID);
            onSelectionChanged?.Invoke();
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
        public void OnRenameStationAction(string autoName)
        {
            autoNameLabel.text = autoName;
        }
        public event OnLineLoad onLineChanged;
        public event OnItemSelectedChanged onSelectionChanged;
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

