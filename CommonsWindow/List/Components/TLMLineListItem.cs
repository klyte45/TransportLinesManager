
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.TextureAtlas;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.TextureAtlas;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow.Components
{
    internal abstract class TLMLineListItem<T> : ToolsModifierControl where T : TLMSysDef<T>
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

        private UILabel m_lineBudgetLabel;

        private UILabel m_perHourBudgetInfo;

        //    private UILabel m_LineEarnings;

        private UIButton m_LineNumberFormatted;

        private UIComponent m_Background;

        private Color32 m_BackgroundColor;

        private int m_PassengerCount;

        private int m_LineNumber;

        private bool m_mouseIsOver;

        private AsyncAction m_LineOperation;

        private UIPanel m_lineIncompleteWarning;

        public ushort lineID
        {
            get {
                return this.m_LineID;
            }
            set {
                this.SetLineID(value);
            }
        }

        public string lineName
        {
            get {
                return this.m_LineName.text;
            }
        }

        public int stopCounts
        {
            get {
                return int.Parse(m_LineStops.text);
            }
        }

        public int vehicleCounts
        {
            get {
                return int.Parse(m_LineVehicles.text);
            }
        }

        public int lineNumber
        {
            get {
                return this.m_LineNumber;
            }
        }

        public string lineNumberFormatted
        {
            get {
                return this.m_LineNumberFormatted.text;
            }
        }

        public int passengerCountsInt
        {
            get {
                return this.m_PassengerCount;
            }
        }

        public string passengerCounts
        {
            get {
                return this.m_LinePassengers.text;
            }
        }

        private void SetLineID(ushort id)
        {
            this.m_LineID = id;
        }

        public void RefreshData(bool updateColors, bool updateVisibility)
        {
            if (this.m_LineOperation == null || this.m_LineOperation.completedOrFailed)
            {
                TLMLineUtils.getLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID], out bool dayActive, out bool nightActive);
                bool zeroed;
                unchecked
                {
                    zeroed = (Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_flags & (TransportLine.Flags)TLMTransportLineFlags.ZERO_BUDGET_CURRENT) != 0;
                }
                if (!dayActive || !nightActive || zeroed)
                {
                    m_LineColor.normalBgSprite = zeroed ? "NoBudgetIcon" : dayActive ? "DayIcon" : nightActive ? "NightIcon" : "DisabledIcon";
                }
                else
                {
                    m_LineColor.normalBgSprite = "";
                }

                this.m_DayLine.isChecked = (dayActive && !nightActive);
                this.m_NightLine.isChecked = (nightActive && !dayActive);
                this.m_DayNightLine.isChecked = (dayActive && nightActive);
                this.m_DisabledLine.isChecked = (!dayActive && !nightActive);
                m_DisabledLine.relativePosition = new Vector3(730, 8);
            }
            else
            {
                m_LineColor.normalBgSprite = "DisabledIcon";
            }
            this.m_LineName.text = Singleton<TransportManager>.instance.GetLineName(this.m_LineID);
            m_LineNumber = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_lineNumber;
            this.m_LineStops.text = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].CountStops(this.m_LineID).ToString("N0");
            this.m_LineVehicles.text = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].CountVehicles(this.m_LineID).ToString("N0");
            uint prefix = 0;

            var tsd = Singleton<T>.instance.GetTSD();
            if (TLMConfigWarehouse.getCurrentConfigInt(tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int)ModoNomenclatura.Nenhum)
            {
                prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
            }


            int averageCount = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_passengers.m_residentPassengers.m_averageCount;
            int averageCount2 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_passengers.m_touristPassengers.m_averageCount;
            this.m_LinePassengers.text = (averageCount + averageCount2).ToString("N0");

            this.m_LinePassengers.tooltip = string.Format("{0}", LocaleFormatter.FormatGeneric("TRANSPORT_LINE_PASSENGERS", new object[]
            {
                averageCount,
                averageCount2
            }));
            TLMLineUtils.setLineNumberCircleOnRef(lineID, m_LineNumberFormatted, 0.8f);
            m_LineColor.normalFgSprite = TLMLineUtils.getIconForLine(lineID);

            this.m_PassengerCount = averageCount + averageCount2;

            this.m_lineIncompleteWarning.isVisible = ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None);

            if (updateColors)
            {
                this.m_LineColor.selectedColor = Singleton<TransportManager>.instance.GetLineColor(this.m_LineID);
            }
            if (updateVisibility)
            {
                this.m_LineIsVisible.isChecked = ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_flags & TransportLine.Flags.Hidden) == TransportLine.Flags.None);
            }


            if (tsd.hasVehicles())
            {
                TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].Info;
                float overallBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;

                string vehTooltip = string.Format("{0} {1}", this.m_LineVehicles.text, Locale.Get("PUBLICTRANSPORT_VEHICLES"));
                this.m_LineVehicles.tooltip = vehTooltip;
                if (!TLMTransportLineExtension.instance.IsUsingCustomConfig(this.lineID) || !TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(this.lineID))
                {
                    this.m_lineBudgetLabel.text = string.Format("{0:0%}", TLMLineUtils.getEffectiveBugdet(lineID));//585+1/7 = frames/week  
                    m_lineBudgetLabel.tooltip = string.Format(Locale.Get("TLM_LINE_BUDGET_EXPLAIN_2"),
                        TLMConfigWarehouse.getNameForTransportType(tsd.toConfigIndex()),
                        overallBudget, Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f, TLMLineUtils.getEffectiveBugdet(lineID));
                    m_lineBudgetLabel.relativePosition = new Vector3(m_LineVehicles.relativePosition.x, 19, 0);
                    m_lineBudgetLabel.isVisible = true;
                }
                else
                {
                    m_lineBudgetLabel.isVisible = false;
                }


                bool tlmPerHour = TLMLineUtils.isPerHourBudget(m_LineID);
                m_DayLine.isVisible = !tlmPerHour;
                m_DayNightLine.isVisible = !tlmPerHour;
                m_NightLine.isVisible = !tlmPerHour;
                m_DisabledLine.isVisible = !tlmPerHour;
                m_perHourBudgetInfo.isVisible = tlmPerHour;

                m_perHourBudgetInfo.relativePosition = new Vector3(615, 2);
            }
            else
            {
                m_DayLine.isVisible = true;
                m_DayNightLine.isVisible = true;
                m_NightLine.isVisible = true;
                m_DisabledLine.isVisible = true;
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
            base.component.eventZOrderChanged += delegate (UIComponent c, int r)
            {
                this.SetBackgroundColor();
            };
            this.m_LineIsVisible = base.Find<UICheckBox>("LineVisible");
            this.m_LineIsVisible.eventCheckChanged += (x, y) => ChangeLineVisibility(y);
            this.m_LineColor = base.Find<UIColorField>("LineColor");
            this.m_LineColor.normalBgSprite = "";
            this.m_LineColor.focusedBgSprite = "";
            this.m_LineColor.hoveredBgSprite = "";
            this.m_LineColor.width = 40;
            this.m_LineColor.height = 40;
            this.m_LineColor.atlas = LineUtilsTextureAtlas.instance.atlas;
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
            m_DisabledLine = GameObject.Instantiate(base.Find<UICheckBox>("DayLine"), m_DayLine.transform.parent);
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



            m_NightLine.relativePosition = new Vector3(678, 8);
            m_DayNightLine.relativePosition = new Vector3(704, 8);


            this.m_LineStops = base.Find<UILabel>("LineStops");
            this.m_LinePassengers = base.Find<UILabel>("LinePassengers");

            var tsd = Singleton<T>.instance.GetTSD();
            this.m_LineVehicles = base.Find<UILabel>("LineVehicles");
            if (tsd.hasVehicles())
            {
                m_LineVehicles.relativePosition = new Vector3(m_LineVehicles.relativePosition.x, 5, 0);
                m_lineBudgetLabel = GameObject.Instantiate(this.m_LineStops, m_LineStops.transform.parent);
            }
            else
            {
                Destroy(m_LineVehicles.gameObject);
            }

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
                                GameObject.Destroy(gameObject);
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
                    TLMController.instance.CloseTLMPanel();
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
            TLMUtils.createUIElement(out UIButton buttonAutoName, transform);
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

            TLMUtils.createUIElement(out UIButton buttonAutoColor, transform);
            buttonAutoColor.pivot = UIPivotPoint.TopRight;
            buttonAutoColor.relativePosition = new Vector3(90, 2);
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

            m_lineIncompleteWarning = base.Find<UIPanel>("WarningIncomplete");

            if (tsd.hasVehicles())
            {
                TLMUtils.createUIElement(out m_perHourBudgetInfo, transform);
                m_perHourBudgetInfo.name = "PerHourIndicator";
                m_perHourBudgetInfo.autoSize = false;
                m_perHourBudgetInfo.autoHeight = true;
                m_perHourBudgetInfo.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
                m_perHourBudgetInfo.width = 180;
                m_perHourBudgetInfo.height = m_perHourBudgetInfo.parent.height;
                m_perHourBudgetInfo.verticalAlignment = UIVerticalAlignment.Middle;
                m_perHourBudgetInfo.textAlignment = UIHorizontalAlignment.Center;
                m_perHourBudgetInfo.textScale = 1f;
                m_perHourBudgetInfo.localeID = "TLM_PER_HOUR_BUDGET_ACTIVE_LABEL";
                m_perHourBudgetInfo.wordWrap = true;
                m_perHourBudgetInfo.eventTextChanged += constraintedScale;
                constraintedScale(m_perHourBudgetInfo, "");
            }
        }

        public void ChangeLineVisibility(bool r)
        {
            if (m_LineID != 0)
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
        }

        private void constraintedScale(UIComponent component, string value)
        {
            component.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
            float ratio = Math.Min(1, component.height / component.parent.height);
            component.transform.localScale = new Vector3(ratio, ratio);
        }

        public void DoAutoColor()
        {
            TLMUtils.doLog("AutoColor");
            m_LineColor.selectedColor = TLMController.instance.AutoColor(m_LineID);
        }

        public void DoAutoName()
        {
            TLMController.instance.AutoName(m_LineID);
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
            TLMLineUtils.setLineName(this.m_LineID, text);
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
            TLMUtils.doLog($"COLOR CHANGED!! {color}\n{Environment.StackTrace}");

            TLMLineUtils.setLineColor(this.m_LineID, color);
        }
        private void changeLineTime(bool day, bool night)
        {
            m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
            {
                TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_LineID], day, night);
            });
        }

    }

    internal sealed class TLMLineListItemNorBus : TLMLineListItem<TLMSysDefNorBus> { }
    internal sealed class TLMLineListItemEvcBus : TLMLineListItem<TLMSysDefEvcBus> { }
    internal sealed class TLMLineListItemNorTrm : TLMLineListItem<TLMSysDefNorTrm> { }
    internal sealed class TLMLineListItemNorMnr : TLMLineListItem<TLMSysDefNorMnr> { }
    internal sealed class TLMLineListItemNorMet : TLMLineListItem<TLMSysDefNorMet> { }
    internal sealed class TLMLineListItemNorTrn : TLMLineListItem<TLMSysDefNorTrn> { }
    internal sealed class TLMLineListItemNorFer : TLMLineListItem<TLMSysDefNorFer> { }
    internal sealed class TLMLineListItemNorBlp : TLMLineListItem<TLMSysDefNorBlp> { }
    internal sealed class TLMLineListItemNorShp : TLMLineListItem<TLMSysDefNorShp> { }
    internal sealed class TLMLineListItemNorPln : TLMLineListItem<TLMSysDefNorPln> { }
    internal sealed class TLMLineListItemTouBus : TLMLineListItem<TLMSysDefTouBus> { }
    internal sealed class TLMLineListItemTouPed : TLMLineListItem<TLMSysDefTouPed> { }
}
