
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Klyte.TransportLinesManager.LineList
{
    using ColossalFramework;
    using ColossalFramework.UI;
    using Extensions;
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

        private UIDropDown m_LineTime;

        private UILabel m_LineStops;

        private UILabel m_LineVehicles;

        private UILabel m_LinePassengers;

        private UIComponent m_Background;

        private Color32 m_BackgroundColor;

        private int m_PassengerCount;

        private int m_lineNumber;

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
                return this.m_lineNumber;
            }
        }

        public string formattedLineNumber
        {
            get
            {
                return this.m_lineNumber.ToString();
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
                this.m_LineName.text = Singleton<TransportManager>.instance.GetLineName(this.m_LineID);
                if (this.m_LineOperation == null || this.m_LineOperation.completedOrFailed)
                {
                    bool dayActive;
                    bool nightActive;
                    Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].GetActive(out dayActive, out nightActive);
                    this.m_LineTime.selectedIndex = ((dayActive ? 0 : 2) + (nightActive ? 0 : 1));
                }
                m_lineNumber = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_lineNumber;
                this.m_LineStops.text = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].CountStops(this.m_LineID).ToString("N0");
                this.m_LineVehicles.text = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].CountVehicles(this.m_LineID).ToString("N0");
                int averageCount = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_passengers.m_residentPassengers.m_averageCount;
                int averageCount2 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_passengers.m_touristPassengers.m_averageCount;
                this.m_LinePassengers.text = (averageCount + averageCount2).ToString("N0");
                this.m_LinePassengers.tooltip = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_PASSENGERS", new object[]
                {
                averageCount,
                averageCount2
                });
                this.m_PassengerCount = averageCount + averageCount2;
                if (colors)
                {
                    this.m_LineColor.selectedColor = Singleton<TransportManager>.instance.GetLineColor(this.m_LineID);
                }
                if (visible)
                {
                    this.m_LineIsVisible.isChecked = ((Singleton<TransportManager>.instance.m_lines.m_buffer[(int)this.m_LineID].m_flags & TransportLine.Flags.Hidden) == TransportLine.Flags.None);
                }

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
            if (base.component.isVisible)
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
            this.m_LineColor.eventSelectedColorReleased += new PropertyChangedEventHandler<Color>(this.OnColorChanged);
            this.m_LineName = base.Find<UILabel>("LineName");
            this.m_LineNameField = this.m_LineName.Find<UITextField>("LineNameField");
            this.m_LineNameField.eventTextSubmitted += new PropertyChangedEventHandler<string>(this.OnRename);
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

            GameObject.Destroy(base.Find<UICheckBox>("DayLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("NightLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("DayNightLine").gameObject);


            this.m_LineTime = UIHelperExtension.CloneBasicDropDownNoLabel(new string[] {
                "Day & Night",
                "Day Only",
                "Night Only",
                "Disable (without delete)"
            }, changeLineTime, gameObject.GetComponent<UIPanel>());

            m_LineTime.area = new Vector4(630, 3, 140, 33);

            var m_DayLine = base.Find<UICheckBox>("DayLine");

            GameObject.Destroy(base.Find<UICheckBox>("NightLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("DayNightLine").gameObject);
            GameObject.Destroy(m_DayLine.gameObject);

            this.m_LineStops = base.Find<UILabel>("LineStops");
            this.m_LineVehicles = base.Find<UILabel>("LineVehicles");
            this.m_LinePassengers = base.Find<UILabel>("LinePassengers");
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

        private void changeLineTime(int selection)
        {
            m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
             {
                 switch (selection)
                 {
                     case 0:
                         Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_LineID].SetActive(true, true);
                         break;
                     case 1:
                         Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_LineID].SetActive(true, false);
                         break;
                     case 2:
                         Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_LineID].SetActive(false, true);
                         break;
                     case 3:
                         Singleton<TransportManager>.instance.m_lines.m_buffer[(int)m_LineID].SetActive(false, false);
                         break;
                 }
             });
        }
    }
}
