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
using Klyte.TransportLinesManager.LineList;

namespace Klyte.TransportLinesManager.UI
{
    public class TLMLineInfoPanel
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
        private UILabel detailedStatsLabel;
        private UITextField lineNumberLabel;
        private UITextField vehicleQuantitySet;
        private UIDropDown linePrefixDropDown;
        private UILabel lineTransportIconTypeLabel;
        private UILabel viagensEvitadasLabel;
        private UILabel passageirosEturistasLabel;
        private UILabel veiculosLinhaLabel;
        private UILabel m_autoNameLabel;
        private UILabel vehicleQuantitySetLabel;
        //private UILabel generalDebugLabel;
        private UIDropDown lineTime;
        private UILabel lineTimeTitle;
        private UITextField lineNameField;
        private UIColorField lineColorPicker;
        private AsyncAction daytimeChange;

        public UILabel autoNameLabel
        {
            get
            {
                return m_autoNameLabel;
            }
        }

        public Transform transform
        {
            get
            {
                return lineInfoPanel.transform;
            }
        }

        public GameObject gameObject
        {
            get
            {
                try
                {
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
            get
            {
                return lineInfoPanel.isVisible;
            }
        }

        public TLMController controller
        {
            get
            {
                return m_controller;
            }
        }

        public TLMLinearMap linearMap
        {
            get
            {
                return m_linearMap;
            }
        }

        public InstanceID lineIdSelecionado
        {
            get
            {
                return m_lineIdSelecionado;
            }
        }

        public CameraController cameraController
        {
            get
            {
                return m_CameraController;
            }
        }

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

        public void Show()
        {
            if (!GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible)
            {
                GameObject.Find("InfoViewsPanel").GetComponent<UIPanel>().isVisible = true;
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

        private bool isNumeroUsado(int numLinha, ushort lineIdx)
        {
            TLMCW.ConfigIndex tipo = TLMCW.getDefinitionForLine(lineIdx).toConfigIndex();
            for (ushort i = 0; i < Singleton<TransportManager>.instance.m_lines.m_buffer.Length; i++)
            {
                if (i != lineIdx && TLMCW.getDefinitionForLine(i).toConfigIndex() == tipo && Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_lineNumber == numLinha && (Singleton<TransportManager>.instance.m_lines.m_buffer[i].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                {
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
            ModoNomenclatura sufixo;
            ModoNomenclatura prefixo;
            ModoNomenclatura nonPrefix;
            Separador sep;
            bool zeros;
            bool invertPrefixSuffix;
            TLMLineUtils.getLineNamingParameters(m_lineIdSelecionado.TransportLine, out prefixo, out sep, out sufixo, out nonPrefix, out zeros, out invertPrefixSuffix);
            ushort num = ushort.Parse(value);
            if (prefixo != ModoNomenclatura.Nenhum)
            {
                num = (ushort)(valPrefixo * 1000 + (num % 1000));
            }
            bool numeroUsado = isNumeroUsado(num, m_lineIdSelecionado.TransportLine);
            if (num < 1)
            {
                lineNumberLabel.textColor = new Color(1, 0, 0, 1);
                return;
            }

            if (numeroUsado)
            {
                lineNumberLabel.textColor = new Color(1, 0, 0, 1);
            }
            else {
                lineNumberLabel.textColor = new Color(1, 1, 1, 1);
                m_controller.tm.m_lines.m_buffer[(int)m_lineIdSelecionado.TransportLine].m_lineNumber = num;
                m_linearMap.setLineNumberCircle(num, prefixo, sep, sufixo, nonPrefix, zeros, invertPrefixSuffix);
                m_autoNameLabel.text = m_linearMap.autoName;

                if (prefixo != ModoNomenclatura.Nenhum)
                {
                    lineNumberLabel.text = (num % 1000).ToString();
                    linePrefixDropDown.selectedIndex = (num / 1000);
                }
                else {
                    lineNumberLabel.text = (num % 10000).ToString();
                }
            }
        }

        private void createInfoView()
        {
            //line info painel

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
            lineInfoPanel.eventVisibilityChanged += (component, value) =>
            {
                if (m_linearMap != null)
                {
                    m_linearMap.isVisible = value;
                }
            };



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
            linePrefixDropDown.height = 35;
            linePrefixDropDown.name = "LinePrefixDropDown";
            linePrefixDropDown.textScale = 1.6f;
            linePrefixDropDown.itemHeight = 35;
            linePrefixDropDown.itemPadding = new RectOffset(2, 2, 2, 2);
            linePrefixDropDown.textFieldPadding = new RectOffset(2, 2, 2, 2);
            linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            linePrefixDropDown.relativePosition = new Vector3(70f, 3f);


            TLMUtils.createUIElement<UITextField>(ref lineNumberLabel, lineInfoPanel.transform);
            lineNumberLabel.autoSize = false;
            lineNumberLabel.relativePosition = new Vector3(80f, 3f);
            lineNumberLabel.horizontalAlignment = UIHorizontalAlignment.Center;
            lineNumberLabel.text = "";
            lineNumberLabel.width = 75;
            lineNumberLabel.height = 35;
            lineNumberLabel.name = "LineNumberLabel";
            lineNumberLabel.normalBgSprite = "EmptySprite";
            lineNumberLabel.textScale = 1.6f;
            lineNumberLabel.padding = new RectOffset(5, 5, 5, 5);
            lineNumberLabel.color = new Color(0, 0, 0, 1);
            TLMUtils.uiTextFieldDefaults(lineNumberLabel);
            lineNumberLabel.numericalOnly = true;
            lineNumberLabel.maxLength = 4;
            lineNumberLabel.eventLostFocus += saveLineNumber;
            lineNumberLabel.zOrder = 10;


            TLMUtils.createUIElement<UITextField>(ref lineNameField, lineInfoPanel.transform);
            lineNameField.autoSize = false;
            lineNameField.relativePosition = new Vector3(190f, 10f);
            lineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
            lineNameField.text = "NOME";
            lineNameField.width = 450;
            lineNameField.height = 25;
            lineNameField.name = "LineNameLabel";
            lineNameField.maxLength = 256;
            lineNameField.textScale = 1.5f;
            TLMUtils.uiTextFieldDefaults(lineNameField);
            lineNameField.eventGotFocus += (component, eventParam) =>
            {
                lastLineName = lineNameField.text;
            };
            lineNameField.eventLostFocus += (component, eventParam) =>
            {
                if (lastLineName != lineNameField.text)
                {
                    saveLineName(lineNameField);
                }
                lineNameField.text = m_controller.tm.GetLineName(m_lineIdSelecionado.TransportLine);
            };

            TLMUtils.createUIElement<UILabel>(ref lineLenghtLabel, lineInfoPanel.transform);
            lineLenghtLabel.autoSize = false;
            lineLenghtLabel.relativePosition = new Vector3(10f, 60f);
            lineLenghtLabel.textAlignment = UIHorizontalAlignment.Left;
            lineLenghtLabel.text = "";
            lineLenghtLabel.width = 550;
            lineLenghtLabel.height = 25;
            lineLenghtLabel.prefix = "";
            lineLenghtLabel.suffix = "";
            lineLenghtLabel.name = "LineLenghtLabel";
            lineLenghtLabel.textScale = 0.8f;

            TLMUtils.createUIElement<UILabel>(ref detailedStatsLabel, lineInfoPanel.transform);
            detailedStatsLabel.autoSize = false;
            detailedStatsLabel.relativePosition = new Vector3(10f, 75f);
            detailedStatsLabel.textAlignment = UIHorizontalAlignment.Left;
            detailedStatsLabel.width = 550;
            detailedStatsLabel.height = 25;
            detailedStatsLabel.name = "ExtraInfoLabel";
            detailedStatsLabel.textScale = 0.8f;
            detailedStatsLabel.prefix = Locale.Get("TLM_LINE_EFFECTIVE_BUDGET") + ": ";

            TLMUtils.createUIElement<UILabel>(ref veiculosLinhaLabel, lineInfoPanel.transform);
            veiculosLinhaLabel.autoSize = false;
            veiculosLinhaLabel.relativePosition = new Vector3(10f, 90);
            veiculosLinhaLabel.textAlignment = UIHorizontalAlignment.Left;
            veiculosLinhaLabel.text = "";
            veiculosLinhaLabel.width = 550;
            veiculosLinhaLabel.height = 25;
            veiculosLinhaLabel.name = "VehiclesLineLabel";
            veiculosLinhaLabel.textScale = 0.8f;

            TLMUtils.createUIElement<UILabel>(ref viagensEvitadasLabel, lineInfoPanel.transform);
            viagensEvitadasLabel.autoSize = false;
            viagensEvitadasLabel.relativePosition = new Vector3(10f, 105);
            viagensEvitadasLabel.textAlignment = UIHorizontalAlignment.Left;
            viagensEvitadasLabel.text = "";
            viagensEvitadasLabel.width = 250;
            viagensEvitadasLabel.height = 25;
            viagensEvitadasLabel.name = "AvoidedTravelsLabel";
            viagensEvitadasLabel.textScale = 0.8f;

            TLMUtils.createUIElement<UILabel>(ref passageirosEturistasLabel, lineInfoPanel.transform);
            passageirosEturistasLabel.autoSize = false;
            passageirosEturistasLabel.relativePosition = new Vector3(10f, 120f);
            passageirosEturistasLabel.textAlignment = UIHorizontalAlignment.Left;
            passageirosEturistasLabel.text = "";
            passageirosEturistasLabel.width = 350;
            passageirosEturistasLabel.height = 25;
            passageirosEturistasLabel.name = "TouristAndPassagersLabel";
            passageirosEturistasLabel.textScale = 0.8f;

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
            TLMUtils.createUIElement<UILabel>(ref vehicleQuantitySetLabel, lineInfoPanel.transform);
            vehicleQuantitySetLabel.autoSize = false;
            vehicleQuantitySetLabel.relativePosition = new Vector3(10f, 135f);
            vehicleQuantitySetLabel.textAlignment = UIHorizontalAlignment.Left;
            vehicleQuantitySetLabel.localeID = "TLM_SET_FIXED_VEHICLE_NUMBER_LINE";
            vehicleQuantitySetLabel.isLocalized = true;
            vehicleQuantitySetLabel.width = 300;
            vehicleQuantitySetLabel.height = 40;
            vehicleQuantitySetLabel.name = "FixedVehiclesLabel";
            vehicleQuantitySetLabel.textScale = 0.8f;
            vehicleQuantitySetLabel.clipChildren = false;
            vehicleQuantitySetLabel.zOrder = 0;

            vehicleQuantitySet = UITextField.Instantiate(lineNumberLabel);
            vehicleQuantitySet.transform.SetParent(lineInfoPanel.transform);
            vehicleQuantitySet.relativePosition = new Vector3(120f, 165f);
            vehicleQuantitySet.size = new Vector2(50, 30);
            vehicleQuantitySet.padding = new RectOffset(0, 0, 0, 0);
            vehicleQuantitySet.textScale = 0.8f;
            vehicleQuantitySet.numericalOnly = true;
            vehicleQuantitySet.eventLostFocus += (x, y) =>
            {
                TLMVehiclesLineManager.instance[this.lineIdSelecionado.TransportLine] = int.Parse(vehicleQuantitySet.text);
            };

            TLMUtils.createUIElement<UILabel>(ref m_autoNameLabel, lineInfoPanel.transform);
            m_autoNameLabel.autoSize = false;
            m_autoNameLabel.relativePosition = new Vector3(10f, 180f);
            m_autoNameLabel.textAlignment = UIHorizontalAlignment.Left;
            m_autoNameLabel.prefix = Locale.Get("TLM_GENERATED_AUTO_NAME") + ": ";
            m_autoNameLabel.width = 350;
            m_autoNameLabel.height = 100;
            m_autoNameLabel.name = "AutoNameLabel";
            m_autoNameLabel.textScale = 0.8f;
            m_autoNameLabel.wordWrap = true;
            m_autoNameLabel.clipChildren = false;

            lineColorPicker = GameObject.Instantiate(PublicTransportWorldInfoPanel.FindObjectOfType<UIColorField>().gameObject).GetComponent<UIColorField>();
            //				
            lineInfoPanel.AttachUIComponent(lineColorPicker.gameObject);
            lineColorPicker.name = "LineColorPicker";
            lineColorPicker.relativePosition = new Vector3(50f, 10f);
            lineColorPicker.enabled = true;
            lineColorPicker.anchor = UIAnchorStyle.Top & UIAnchorStyle.Left;
            lineColorPicker.eventSelectedColorChanged += (UIComponent component, Color value) =>
            {
                TLMUtils.setLineColor(m_lineIdSelecionado.TransportLine, value);
                updateLineUI(value);
            };

            lineTime = UIHelperExtension.CloneBasicDropDown(Locale.Get("TRANSPORT_LINE_ACTIVITY"), new string[] {
                Locale.Get("TRANSPORT_LINE_DAYNNIGHT"),
                 Locale.Get("TRANSPORT_LINE_DAY"),
                 Locale.Get("TRANSPORT_LINE_NIGHT"),
                 Locale.Get("TLM_TRANSPORT_LINE_DISABLED")
            }, changeLineTime, lineInfoPanel, out lineTimeTitle);
            lineTime.parent.relativePosition = new Vector3(120f, 220f);

            UIButton deleteLine = null;
            TLMUtils.createUIElement<UIButton>(ref deleteLine, lineInfoPanel.transform);
            deleteLine.relativePosition = new Vector3(10f, lineInfoPanel.height - 40f);
            deleteLine.localeID = "LINE_DELETE";
            deleteLine.isLocalized = true;
            deleteLine.autoSize = true;
            deleteLine.textPadding = new RectOffset(3, 3, 3, 3);
            TLMUtils.initButton(deleteLine, true, "ButtonMenu");
            deleteLine.name = "DeleteLineButton";
            deleteLine.color = new Color(1, 0, 0, 1);
            deleteLine.eventClick += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    Singleton<TransportManager>.instance.ReleaseLine(m_lineIdSelecionado.TransportLine);
                });
                closeLineInfo(component, eventParam);
            };
            UIButton voltarButton2 = null;
            TLMUtils.createUIElement<UIButton>(ref voltarButton2, lineInfoPanel.transform);
            voltarButton2.relativePosition = new Vector3(lineInfoPanel.width - 250f, lineInfoPanel.height - 40f);
            voltarButton2.localeID = "CLOSE";
            voltarButton2.isLocalized = true;
            voltarButton2.width = 240;
            voltarButton2.height = 30;
            TLMUtils.initButton(voltarButton2, true, "ButtonMenu");
            voltarButton2.name = "LineInfoCloseButton";
            voltarButton2.eventClick += closeLineInfo;

            UIButton autoName = null;
            TLMUtils.createUIElement<UIButton>(ref autoName, lineInfoPanel.transform);
            autoName.relativePosition = new Vector3(lineInfoPanel.width - 250f, lineInfoPanel.height - 80f);
            autoName.localeID = "TLM_USE_AUTO_NAME";
            autoName.isLocalized = true;
            autoName.width = 240;
            autoName.height = 30;
            TLMUtils.initButton(autoName, true, "ButtonMenu");
            autoName.name = "AutoNameButton";
            autoName.eventClick += (component, eventParam) =>
            {
                lineNameField.text = m_linearMap.autoName;
                saveLineName(lineNameField);
            };

            UIButton autoColor = null;
            TLMUtils.createUIElement<UIButton>(ref autoColor, lineInfoPanel.transform);
            autoColor.relativePosition = new Vector3(lineInfoPanel.width - 250f, lineInfoPanel.height - 120f);
            autoColor.localeID = "TLM_PICK_COLOR_FROM_PALETTE";
            autoColor.isLocalized = true;
            autoColor.tooltipLocaleID = "TLM_PICK_COLOR_FROM_PALETTE_TOOLTIP";
            autoColor.width = 240;
            autoColor.height = 30;
            TLMUtils.initButton(autoColor, true, "ButtonMenu");
            autoColor.name = "AutoNameButton";
            autoColor.eventMouseUp += (component, eventParam) =>
            {
                lineColorPicker.selectedColor = m_controller.AutoColor(m_lineIdSelecionado.TransportLine);
                updateLineUI(lineColorPicker.selectedColor);
            };


            agesPanel = new TLMAgesChartPanel(this);
            m_linearMap = new TLMLinearMap(this);
        }

        private void updateLineUI(Color color)
        {
            lineNumberLabel.color = color;
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
            viagensEvitadasLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_TRIPSAVED", new object[]{
                viagensSalvas
            });

            if (daytimeChange != null && daytimeChange.completedOrFailed)
            {
                linearMap.redrawLine();
                daytimeChange = null;
            }
            else
            {
                linearMap.updateBidings();
            }


            //lines info
            int stopsCount = TLMLineUtils.GetStopsCount(lineID);
            if (lastStopsCount != stopsCount)
            {
                float totalSize = TLMLineUtils.GetLineLength(lineID);
                lineLenghtLabel.text = string.Format(Locale.Get("TLM_LENGHT_AND_STOPS"), totalSize, stopsCount);
                lastStopsCount = stopsCount;
            }

            //estatisticas novas
            veiculosLinhaLabel.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_VEHICLECOUNT", new object[] { veiculosLinha });

            uint prefix = 0;
            if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportInfo(info) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
            {
                prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
            }

            float overallBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;
            float prefixMultiplier = TLMUtils.getExtensionFromConfigIndex(TLMCW.getConfigIndexForTransportInfo(info)).getBudgetMultiplierForHour(prefix, (int)Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 100f;

            detailedStatsLabel.text = string.Format("{0:0%}", overallBudget * prefixMultiplier);
            detailedStatsLabel.tooltip = string.Format(Locale.Get("TLM_LINE_BUDGET_EXPLAIN"),
                TLMCW.getNameForTransportType(TLMCW.getConfigIndexForTransportInfo(info)),
                TLMUtils.getStringOptionsForPrefix((ModoNomenclatura)TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportInfo(info) | TLMConfigWarehouse.ConfigIndex.PREFIX), true)[prefix + 1],
                overallBudget, prefixMultiplier,
                overallBudget * prefixMultiplier);

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
            if (lineID <= 0)
            {
                return;
            }
            WorldInfoPanel.HideAllWorldInfoPanels();
            linePrefixDropDown.eventSelectedIndexChanged -= saveLineNumber;
            lineNumberLabel.eventLostFocus -= saveLineNumber;

            m_lineIdSelecionado = default(InstanceID);
            m_lineIdSelecionado.TransportLine = lineID;

            TransportLine t = m_controller.tm.m_lines.m_buffer[(int)lineID];
            ushort lineNumber = t.m_lineNumber;

            TLMCW.ConfigIndex transportType = TLMCW.getDefinitionForLine(lineID).toConfigIndex();
            ModoNomenclatura mnPrefixo = (ModoNomenclatura)TLMCW.getCurrentConfigInt(TLMConfigWarehouse.ConfigIndex.PREFIX | transportType);

            if (mnPrefixo != ModoNomenclatura.Nenhum)
            {
                lineNumberLabel.text = (lineNumber % 1000).ToString();
                lineNumberLabel.relativePosition = new Vector3(110f, 3f);
                lineNumberLabel.width = 55;
                linePrefixDropDown.enabled = false;

                var temp = TLMUtils.getStringOptionsForPrefix(mnPrefixo);
                for (uint i = 0; i < temp.Length; i++)
                {
                    string prefixName = TLMUtils.getTransportSystemPrefixName(transportType, i);
                    if (prefixName != null && prefixName != string.Empty)
                    {
                        temp[(int)i] += " (" + prefixName + ")";
                    }
                }
                linePrefixDropDown.items = temp;
                linePrefixDropDown.selectedIndex = lineNumber / 1000;
                linePrefixDropDown.enabled = true;
                lineNumberLabel.maxLength = 3;
            }
            else {
                lineNumberLabel.text = (lineNumber).ToString();
                lineNumberLabel.relativePosition = new Vector3(80f, 3f);
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

            bool day, night, zeroed;
            TLMLineUtils.getLineActive(ref t, out day, out night, out zeroed);
            if (day && night)
            {
                lineTime.selectedIndex = 0;
            }
            else if (day)
            {
                lineTime.selectedIndex = 1;
            }
            else if (night)
            {
                lineTime.selectedIndex = 2;
            }
            else {
                lineTime.selectedIndex = 3;
            }

            m_linearMap.redrawLine();
            Show();
            m_controller.defaultListingLinesPanel.Hide();

            m_autoNameLabel.text = m_linearMap.autoName;
            vehicleQuantitySet.text = TLMVehiclesLineManager.instance[this.lineIdSelecionado.TransportLine].ToString();
            vehicleQuantitySet.area = new Vector4(150, 150, 50, 20);
            vehicleQuantitySet.color = Color.gray;
            linePrefixDropDown.eventSelectedIndexChanged += saveLineNumber;
            lineNumberLabel.eventLostFocus += saveLineNumber;
        }

        private void changeLineTime(int selection)
        {
            daytimeChange = Singleton<SimulationManager>.instance.AddAction(delegate
            {
                ushort lineID = m_lineIdSelecionado.TransportLine;
                TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)lineID], ((selection & 0x2) == 0), ((selection & 0x1) == 0));
            });

        }
    }
}

