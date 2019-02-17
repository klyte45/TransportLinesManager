
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.LineList
{
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using Extensions;
    using Extensors;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using UnityEngine;

    public class TLMPublicTransportLineInfo : ToolsModifierControl
    {
        private ushort m_LineID;

        private UICheckBox m_LineIsVisible;

        private UIColorField m_LineColor;

        private UILabel m_LineName;

        private UITextField m_LineNameField;

        private UICheckBox m_DayLine;

        private UICheckBox m_NightLine;

        private UICheckBox m_DayNightLine;

        private UICheckBox m_DisabledLine;

        //private UILabel m_noBudgetWarn;

        private UILabel m_LineStops;

        private UILabel m_LineVehicles;

        private UILabel m_LinePassengers;

        private UILabel m_budgetEffective;

        //    private UILabel m_LineEarnings;

        private UIButton m_LineNumberFormatted;

        private UIComponent m_Background;

        private Color32 m_BackgroundColor;

        private int m_PassengerCount;

        private int m_LineNumber;

        private bool m_mouseIsOver;

        private AsyncAction m_LineOperation;

        public ushort lineID
        {
            get
            {
                return this.m_LineID;
            }
            set
            {
                this.SetLineID(value);
            }
        }

        public string lineName
        {
            get
            {
                return this.m_LineName.text;
            }
        }

        public string stopCounts
        {
            get
            {
                return this.m_LineStops.text;
            }
        }

        public string vehicleCounts
        {
            get
            {
                return this.m_LineVehicles.text;
            }
        }

        public int lineNumber
        {
            get
            {
                return this.m_LineNumber;
            }
        }

        public string lineNumberFormatted
        {
            get
            {
                return this.m_LineNumberFormatted.text;
            }
        }

        public int passengerCountsInt
        {
            get
            {
                return this.m_PassengerCount;
            }
        }

        public string passengerCounts
        {
            get
            {
                return this.m_LinePassengers.text;
            }
        }

        private void SetLineID(ushort id)
        {
            this.m_LineID = id;
        }

        public void RefreshData(bool colors, bool visible)
        {
            if (Singleton<TransportManager>.exists)
            {

                bool isRowVisible;

                if (this.m_LineOperation == null || this.m_LineOperation.completedOrFailed)
                {

                    //this.m_DayLine.isVisible = (!isZeroed);
                    //this.m_NightLine.isVisible = (!isZeroed);
                    //this.m_DayNightLine.isVisible = (!isZeroed);
                    //this.m_DisabledLine.isVisible = (!isZeroed);
                    //this.m_noBudgetWarn.isVisible = (isZeroed);

                    bool isZeroed;
                    bool dayActive;
                    bool nightActive;

                    TLMLineUtils.getLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID], out dayActive, out nightActive, out isZeroed);
                    if (!isZeroed)
                    {
                        if (!dayActive || !nightActive)
                        {
                            m_LineColor.normalBgSprite = dayActive ? "DayIcon" : nightActive ? "NightIcon" : "DisabledIcon";
                        }
                        else {
                            m_LineColor.normalBgSprite = "";
                        }
                    }
                    else
                    {
                        m_LineColor.normalBgSprite = "NoBudgetIcon";
                        //m_noBudgetWarn.relativePosition = new Vector3(615, 2);
                    }
                    this.m_DayLine.isChecked = (dayActive && !nightActive);
                    this.m_NightLine.isChecked = (nightActive && !dayActive);
                    this.m_DayNightLine.isChecked = (dayActive && nightActive);
                    this.m_DisabledLine.isChecked = (!dayActive && !nightActive);
                    m_DisabledLine.relativePosition = new Vector3(730, 8);
                    isRowVisible = TLMPublicTransportDetailPanel.instance.isActivityVisible(dayActive, nightActive) && TLMPublicTransportDetailPanel.instance.isOnCurrentPrefixFilter(m_LineNumber);

                }
                else
                {
                    m_LineColor.normalBgSprite = "DisabledIcon";
                    isRowVisible = TLMPublicTransportDetailPanel.instance.isActivityVisible(false, false) && TLMPublicTransportDetailPanel.instance.isOnCurrentPrefixFilter(m_LineNumber);
                }
                if (!isRowVisible)
                {
                    GetComponent<UIComponent>().isVisible = false;
                    return;
                }
                GetComponent<UIComponent>().isVisible = true;
                this.m_LineName.text = Singleton<TransportManager>.instance.GetLineName(this.m_LineID);
                m_LineNumber = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_lineNumber;
                this.m_LineStops.text = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].CountStops(this.m_LineID).ToString("N0");
                this.m_LineVehicles.text = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].CountVehicles(this.m_LineID).ToString("N0");
                uint prefix = 0;
                TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].Info;
                if (TLMConfigWarehouse.getCurrentConfigInt(TLMConfigWarehouse.getConfigIndexForTransportInfo(info) | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
                {
                    prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
                }

                float overallBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;
                float prefixMultiplier = TLMUtils.getExtensionFromConfigIndex(TLMCW.getConfigIndexForTransportInfo(info)).getBudgetMultiplierForHour(prefix, (int)Singleton<SimulationManager>.instance.m_currentDayTimeHour) / 100f;

                this.m_budgetEffective.text = string.Format("{0:0%}", overallBudget * prefixMultiplier);//585+1/7 = frames/week                

                string vehTooltip = string.Format("{0} {1}", this.m_LineVehicles.text, Locale.Get("PUBLICTRANSPORT_VEHICLES"));


                this.m_LineVehicles.tooltip = vehTooltip;


                int averageCount = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_passengers.m_residentPassengers.m_averageCount;
                int averageCount2 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_passengers.m_touristPassengers.m_averageCount;
                this.m_LinePassengers.text = (averageCount + averageCount2).ToString("N0");


                //   this.m_LineEarnings.text = string.Format("~₡ {0:0.00}", (averageCount + averageCount2) / 50f);
                //    m_LineEarnings.relativePosition = m_LinePassengers.relativePosition + new Vector3(0, 20, 0);


                this.m_LinePassengers.tooltip = string.Format("{0}", LocaleFormatter.FormatGeneric("TRANSPORT_LINE_PASSENGERS", new object[]
                {
                averageCount,
                averageCount2
                }));
                ModoNomenclatura prefixMode;
                Separador sep;
                ModoNomenclatura suffix;
                ModoNomenclatura nonPrefix;
                bool zerosEsquerda;
                bool invertPrefixSuffix;
                string bgSprite;
                TLMLineUtils.getLineNamingParameters(lineID, out prefixMode, out sep, out suffix, out nonPrefix, out zerosEsquerda, out invertPrefixSuffix, out bgSprite);
                TLMLineUtils.setLineNumberCircleOnRef(lineNumber, prefixMode, sep, suffix, nonPrefix, zerosEsquerda, m_LineNumberFormatted, invertPrefixSuffix, 0.8f);
                m_LineColor.normalFgSprite = bgSprite;

                m_budgetEffective.tooltip = string.Format(Locale.Get("TLM_LINE_BUDGET_EXPLAIN"), TLMCW.getNameForTransportType(TLMCW.getConfigIndexForTransportInfo(info)), TLMUtils.getStringOptionsForPrefix(prefixMode, true)[prefix + 1], overallBudget, prefixMultiplier, overallBudget * prefixMultiplier);

                this.m_PassengerCount = averageCount + averageCount2;
                if (colors)
                {
                    this.m_LineColor.selectedColor = Singleton<TransportManager>.instance.GetLineColor(this.m_LineID);
                }
                if (visible)
                {
                    this.m_LineIsVisible.isChecked = ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_flags & TransportLine.Flags.Hidden) == TransportLine.Flags.None);
                }
                

                m_budgetEffective.relativePosition = new Vector3(m_LineVehicles.relativePosition.x, 19, 0);
            }
        }

        public void SetBackgroundColor()
        {
            Color32 backgroundColor = this.m_BackgroundColor;
            backgroundColor.a = (byte)((base.component.zOrder % 2 != 0) ? 127 : 255);
            if (this.m_mouseIsOver)
            {
                backgroundColor.r = (byte)Mathf.Min(255, backgroundColor.r * 3 >> 1);
                backgroundColor.g = (byte)Mathf.Min(255, backgroundColor.g * 3 >> 1);
                backgroundColor.b = (byte)Mathf.Min(255, backgroundColor.b * 3 >> 1);
            }
            this.m_Background.color = backgroundColor;
        }

        private void LateUpdate()
        {
            if (base.component.parent.isVisible)
            {
                this.RefreshData(false, false);
            }
        }

        private void Awake()
        {
            TLMUtils.clearAllVisibilityEvents(this.GetComponent<UIPanel>());
            base.component.eventZOrderChanged += delegate (UIComponent c, int r)
            {
                this.SetBackgroundColor();
            };
            this.m_LineIsVisible = base.Find<UICheckBox>("LineVisible");
            this.m_LineIsVisible.eventCheckChanged += delegate (UIComponent c, bool r)
            {
                if (this.m_LineID != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        if (r)
                        {
                            TransportLine[] expr_2A_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                            ushort expr_2A_cp_1 = this.m_LineID;
                            expr_2A_cp_0[(int)expr_2A_cp_1].m_flags = (expr_2A_cp_0[(int)expr_2A_cp_1].m_flags & ~TransportLine.Flags.Hidden);
                        }
                        else
                        {
                            TransportLine[] expr_5C_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                            ushort expr_5C_cp_1 = this.m_LineID;
                            expr_5C_cp_0[(int)expr_5C_cp_1].m_flags = (expr_5C_cp_0[(int)expr_5C_cp_1].m_flags | TransportLine.Flags.Hidden);
                        }
                    });
                }
            };
            this.m_LineColor = base.Find<UIColorField>("LineColor");
            this.m_LineColor.normalBgSprite = "";
            this.m_LineColor.focusedBgSprite = "";
            this.m_LineColor.hoveredBgSprite = "";
            this.m_LineColor.width = 40;
            this.m_LineColor.height = 40;
            this.m_LineColor.atlas = TLMController.taLineNumber;
            this.m_LineNumberFormatted = this.m_LineColor.GetComponentInChildren<UIButton>();
            m_LineNumberFormatted.textScale = 1.5f;
            m_LineNumberFormatted.useOutline = true;
            this.m_LineColor.eventSelectedColorChanged += new PropertyChangedEventHandler<Color>(this.OnColorChanged);
            this.m_LineName = base.Find<UILabel>("LineName");
            this.m_LineNameField = this.m_LineName.Find<UITextField>("LineNameField");
            this.m_LineNameField.maxLength = 256;
            this.m_LineNameField.eventTextChanged += new PropertyChangedEventHandler<string>(this.OnRename);
            this.m_LineName.eventMouseEnter += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.m_LineName.backgroundSprite = "TextFieldPanelHovered";
            };
            this.m_LineName.eventMouseLeave += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.m_LineName.backgroundSprite = string.Empty;
            };
            this.m_LineName.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.m_LineNameField.Show();
                this.m_LineNameField.text = this.m_LineName.text;
                this.m_LineNameField.Focus();
            };
            this.m_LineNameField.eventLeaveFocus += delegate (UIComponent c, UIFocusEventParameter r)
            {
                this.m_LineNameField.Hide();
                this.m_LineName.text = this.m_LineNameField.text;
            };


            this.m_DayLine = base.Find<UICheckBox>("DayLine");
            this.m_NightLine = base.Find<UICheckBox>("NightLine");
            this.m_DayNightLine = base.Find<UICheckBox>("DayNightLine");
            m_DisabledLine = GameObject.Instantiate(base.Find<UICheckBox>("DayLine"));
            m_DisabledLine.transform.SetParent(m_DayLine.transform.parent);
            this.m_DayLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                ushort lineID = this.m_LineID;
                if (Singleton<SimulationManager>.exists && lineID != 0)
                {
                    m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        changeLineTime(true, false);
                    });
                }
            };
            this.m_NightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                ushort lineID = this.m_LineID;
                if (Singleton<SimulationManager>.exists && lineID != 0)
                {
                    m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        changeLineTime(false, true);
                    });
                }
            };
            this.m_DayNightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                ushort lineID = this.m_LineID;
                if (Singleton<SimulationManager>.exists && lineID != 0)
                {
                    m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        changeLineTime(true, true);
                    });
                }
            };


            m_DisabledLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                ushort lineID = this.m_LineID;
                if (Singleton<SimulationManager>.exists && lineID != 0)
                {
                    m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        changeLineTime(false, false);
                    });
                }
            };


            m_NightLine.relativePosition = new Vector3(670, 8);
            m_DayNightLine.relativePosition = new Vector3(702, 8);


            //this.m_noBudgetWarn = GameObject.Instantiate(base.Find<UILabel>("LineName"));
            //m_noBudgetWarn.transform.SetParent(m_DayLine.transform.parent);
            //m_noBudgetWarn.isInteractive = false;
            //m_noBudgetWarn.relativePosition = new Vector3(615, 2);
            //m_noBudgetWarn.width = 145;
            //m_noBudgetWarn.isVisible = false;
            //m_noBudgetWarn.text = "";
            //m_noBudgetWarn.textScale = 0.9f;
            //m_noBudgetWarn.localeID = "TLM_LINE_DISABLED_NO_BUDGET";


            this.m_LineStops = base.Find<UILabel>("LineStops");
            this.m_LinePassengers = base.Find<UILabel>("LinePassengers");
            this.m_LineVehicles = base.Find<UILabel>("LineVehicles");
            //m_LinePassengers.relativePosition -= new Vector3(0, 6, 0);
            m_LineVehicles.relativePosition = new Vector3(m_LineVehicles.relativePosition.x, 5, 0);
            m_budgetEffective = GameObject.Instantiate(this.m_LineStops);
            m_budgetEffective.transform.SetParent(m_LineStops.transform.parent);
            //m_LineEarnings = GameObject.Instantiate(this.m_LinePassengers);
            //m_LineEarnings.transform.SetParent(m_LineStops.transform.parent);
            //m_LineEarnings.textColor = Color.green;
            this.m_Background = base.Find("Background");
            this.m_BackgroundColor = this.m_Background.color;
            this.m_mouseIsOver = false;
            base.component.eventMouseEnter += new MouseEventHandler(this.OnMouseEnter);
            base.component.eventMouseLeave += new MouseEventHandler(this.OnMouseLeave);
            base.Find<UIButton>("DeleteLine").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (this.m_LineID != 0)
                {
                    ConfirmPanel.ShowModal("CONFIRM_LINEDELETE", delegate (UIComponent comp, int ret)
                    {
                        if (ret == 1)
                        {
                            Singleton<SimulationManager>.instance.AddAction(delegate
                            {
                                Singleton<TransportManager>.instance.ReleaseLine(this.m_LineID);
                            });
                        }
                    });
                }
            };
            base.Find<UIButton>("ViewLine").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (this.m_LineID != 0)
                {
                    Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_stops].m_position;
                    InstanceID instanceID = default(InstanceID);
                    instanceID.TransportLine = this.m_LineID;
                    TLMController.instance.lineInfoPanel.openLineInfo(lineID);
                    TLMController.instance.defaultListingLinesPanel.Hide();
                }
            };
            base.component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    this.RefreshData(true, true);
                }
            };

            //Auto color & Auto Name
            UIButton buttonAutoName = null;
            TLMUtils.createUIElement<UIButton>(ref buttonAutoName, transform);
            buttonAutoName.pivot = UIPivotPoint.TopRight;
            buttonAutoName.relativePosition = new Vector3(164, 2);
            buttonAutoName.text = "A";
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.width = 15;
            buttonAutoName.height = 15;
            buttonAutoName.tooltip = Locale.Get("TLM_AUTO_NAME_SIMPLE_BUTTON_TOOLTIP");
            TLMUtils.initButton(buttonAutoName, true, "ButtonMenu");
            buttonAutoName.name = "AutoName";
            buttonAutoName.isVisible = true;
            buttonAutoName.eventClick += (component, eventParam) =>
            {
                DoAutoName();
            };

            UIButton buttonAutoColor = null;
            TLMUtils.createUIElement<UIButton>(ref buttonAutoColor, transform);
            buttonAutoColor.pivot = UIPivotPoint.TopRight;
            buttonAutoColor.relativePosition = new Vector3(80, 2);
            buttonAutoColor.text = "A";
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 15;
            buttonAutoColor.height = 15;
            buttonAutoColor.tooltip = Locale.Get("TLM_AUTO_COLOR_SIMPLE_BUTTON_TOOLTIP");
            TLMUtils.initButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.name = "AutoColor";
            buttonAutoColor.isVisible = true;
            buttonAutoColor.eventClick += (component, eventParam) =>
            {
                DoAutoColor();
            };


        }

        public void DoAutoColor()
        {
            m_LineColor.selectedColor = TLMController.instance.AutoColor(m_LineID);
        }

        public void DoAutoName()
        {
            string format = (TLMCW.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.ADD_LINE_NUMBER_IN_AUTONAME)) ? "[{0}] {1}" : "{1}";
            TLMUtils.setLineName(m_LineID, string.Format(format, lineNumberFormatted, TLMUtils.calculateAutoName(m_LineID)));
        }

        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!this.m_mouseIsOver)
            {
                this.m_mouseIsOver = true;
                this.SetBackgroundColor();
                if (this.m_LineID != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                        {
                            TransportLine[] expr_40_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                            ushort expr_40_cp_1 = this.m_LineID;
                            expr_40_cp_0[(int)expr_40_cp_1].m_flags = (expr_40_cp_0[(int)expr_40_cp_1].m_flags | TransportLine.Flags.Highlighted);
                        }
                    });
                }
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (this.m_mouseIsOver)
            {
                this.m_mouseIsOver = false;
                this.SetBackgroundColor();
                if (this.m_LineID != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                        {
                            TransportLine[] expr_40_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                            ushort expr_40_cp_1 = this.m_LineID;
                            expr_40_cp_0[(int)expr_40_cp_1].m_flags = (expr_40_cp_0[(int)expr_40_cp_1].m_flags & ~TransportLine.Flags.Highlighted);
                        }
                    });
                }
            }
        }

        private void OnEnable()
        {
            Singleton<TransportManager>.instance.eventLineColorChanged += new TransportManager.LineColorChangedHandler(this.OnLineChanged);
            Singleton<TransportManager>.instance.eventLineNameChanged += new TransportManager.LineNameChangedHandler(this.OnLineChanged);
        }

        private void OnDisable()
        {
            Singleton<TransportManager>.instance.eventLineColorChanged -= new TransportManager.LineColorChangedHandler(this.OnLineChanged);
            Singleton<TransportManager>.instance.eventLineNameChanged -= new TransportManager.LineNameChangedHandler(this.OnLineChanged);
        }

        private void OnRename(UIComponent comp, string text)
        {
            TLMUtils.setLineName(this.m_LineID, text);
        }

        private void OnLineChanged(ushort id)
        {
            if (id == this.m_LineID)
            {
                this.RefreshData(true, true);
            }
        }

        private void OnColorChanged(UIComponent comp, Color color)
        {
            TLMUtils.setLineColor(this.m_LineID, color);
        }
        private void changeLineTime(bool day, bool night)
        {
            m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
            {
                TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_LineID], day, night);
            });
        }
    }
}
