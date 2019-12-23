
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.CommonsWindow.Components
{
    internal abstract class TLMLineListItem<T> : ToolsModifierControl where T : TLMSysDef<T>
    {
        private static readonly Color32 BackgroundColor = new Color32(49, 52, 58, 255);
        private static readonly Color32 BrokenBackgroundColor = new Color32(80, 26, 24, 255);
        private static readonly Color32 ForegroundColor = new Color32(185, 221, 254, 255);
        private static readonly Color32 SelectionBgColor = new Color32(233, 201, 148, 255);

        private ushort m_LineID;

        private UICheckBox m_LineIsVisible;

        private UIColorField m_LineColor;

        private UILabel m_LineName;

        private UITextField m_LineNameField;

        private UICheckBox m_DayLine;

        private UICheckBox m_NightLine;

        private UICheckBox m_DayNightLine;

        private UICheckBox m_DisabledLine;

        private UILabel m_LineStops;

        private UILabel m_LineVehicles;

        private UILabel m_LinePassengers;

        private UILabel m_lineBudgetLabel;

        private UILabel m_perHourBudgetInfo;

        private UIButton m_LineNumberFormatted;

        private UIPanel m_Background;

        private int m_PassengerCount;

        private int m_LineNumber;

        private bool m_mouseIsOver;

        private AsyncAction m_LineOperation;

        private UIHelperExtension m_uIHelper;

        private bool m_isUpdatingVisibility = false;

        public ushort lineID
        {
            get => m_LineID;
            set => SetLineID(value);
        }

        public string lineName => m_LineName.text;

        public int stopCounts => int.Parse(m_LineStops.text);

        public int vehicleCounts => int.Parse(m_LineVehicles.text);

        public int lineNumber => m_LineNumber;
        public int passengerCountsInt => m_PassengerCount;

        private void SetLineID(ushort id) => m_LineID = id;

        public void RefreshData(bool updateColors, bool updateVisibility)
        {
            if (m_LineOperation == null || m_LineOperation.completedOrFailed)
            {
                TLMLineUtils.getLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID], out bool dayActive, out bool nightActive);
                bool zeroed;
                unchecked
                {
                    zeroed = (Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_flags & (TransportLine.Flags) TLMTransportLineFlags.ZERO_BUDGET_CURRENT) != 0;
                }
                if (!dayActive || !nightActive || zeroed)
                {
                    m_LineColor.normalBgSprite = zeroed ? "NoBudgetIcon" : dayActive ? "DayIcon" : nightActive ? "NightIcon" : "DisabledIcon";
                }
                else
                {
                    m_LineColor.normalBgSprite = "";
                }

                m_DayLine.isChecked = (dayActive && !nightActive);
                m_NightLine.isChecked = (nightActive && !dayActive);
                m_DayNightLine.isChecked = (dayActive && nightActive);
                m_DisabledLine.isChecked = (!dayActive && !nightActive);
            }
            else
            {
                m_LineColor.normalBgSprite = "DisabledIcon";
            }
            m_LineName.text = Singleton<TransportManager>.instance.GetLineName(m_LineID);
            m_LineNumber = Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_lineNumber;
            m_LineStops.text = Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].CountStops(m_LineID).ToString("N0");
            m_LineVehicles.text = Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].CountVehicles(m_LineID).ToString("N0");
            uint prefix = 0;

            TransportSystemDefinition tsd = Singleton<T>.instance.GetTSD();
            if (TLMConfigWarehouse.GetCurrentConfigInt(tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX) != (int) ModoNomenclatura.Nenhum)
            {
                prefix = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_lineNumber / 1000u;
            }


            int averageCount = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_passengers.m_residentPassengers.m_averageCount;
            int averageCount2 = (int) Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_passengers.m_touristPassengers.m_averageCount;
            m_LinePassengers.text = (averageCount + averageCount2).ToString("N0");

            m_LinePassengers.tooltip = string.Format("{0}", LocaleFormatter.FormatGeneric("TRANSPORT_LINE_PASSENGERS", new object[]
            {
                averageCount,
                averageCount2
            }));
            TLMLineUtils.setLineNumberCircleOnRef(lineID, m_LineNumberFormatted, 0.8f);
            m_LineColor.normalFgSprite = TLMLineUtils.getIconForLine(lineID);

            m_PassengerCount = averageCount + averageCount2;

            SetBackgroundColor(((Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None));

            if (updateColors)
            {
                m_LineColor.selectedColor = Singleton<TransportManager>.instance.GetLineColor(m_LineID);
            }
            if (updateVisibility)
            {
                m_isUpdatingVisibility = true;
                m_LineIsVisible.isChecked = ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_flags & TransportLine.Flags.Hidden) == TransportLine.Flags.None);
                m_isUpdatingVisibility = false;
            }


            if (tsd.hasVehicles())
            {
                TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].Info;
                float overallBudget = Singleton<EconomyManager>.instance.GetBudget(info.m_class) / 100f;

                string vehTooltip = string.Format("{0} {1}", m_LineVehicles.text, Locale.Get("PUBLICTRANSPORT_VEHICLES"));
                m_LineVehicles.tooltip = vehTooltip;
                if (!TLMTransportLineExtension.instance.IsUsingCustomConfig(lineID) || !TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(lineID))
                {
                    m_lineBudgetLabel.text = string.Format("{0:0%}", TLMLineUtils.getEffectiveBugdet(lineID));//585+1/7 = frames/week  
                    m_lineBudgetLabel.tooltip = string.Format(Locale.Get("K45_TLM_LINE_BUDGET_EXPLAIN_2"),
                        TLMConfigWarehouse.getNameForTransportType(tsd.toConfigIndex()),
                        overallBudget, Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget / 100f, TLMLineUtils.getEffectiveBugdet(lineID));
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

        public void SetBackgroundColor(bool broken = true)
        {
            Color32 backgroundColor = !broken ? BackgroundColor : BrokenBackgroundColor;
            backgroundColor.a = !broken ? (byte) ((base.component.zOrder % 2 != 0) ? 127 : 255) : (byte) Mathf.Lerp(127, 255, Mathf.Abs((SimulationManager.instance.m_currentTickIndex % 60 / 30f) - 1));
            if (m_mouseIsOver)
            {
                backgroundColor.r = (byte) Mathf.Min(255, (backgroundColor.r * 3) >> 1);
                backgroundColor.g = (byte) Mathf.Min(255, (backgroundColor.g * 3) >> 1);
                backgroundColor.b = (byte) Mathf.Min(255, (backgroundColor.b * 3) >> 1);
            }
            m_Background.color = backgroundColor;
        }

        private void LateUpdate()
        {
            if (base.component.parent.isVisible)
            {
                RefreshData(false, false);
            }
        }

        private void Awake()
        {
            TransportSystemDefinition tsd = Singleton<T>.instance.GetTSD();
            AwakeBG();

            AwakeLineName();

            AwakeLabels();

            AwakeLineDetail();

            AwakeDeleteLine();

            AwakeVehicleLabels(tsd);

            AwakeShowLineButton();

            AwakeLineFormat();

            AwakeAutoButtons();

            AwakeBudgetTimeLabels(tsd);

            base.component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    RefreshData(true, true);
                }
            };
        }

        private void AwakeLineFormat()
        {
            m_LineColor = m_uIHelper.AddColorPickerNoLabel("LineColor", Color.clear, new Commons.Extensors.OnColorChanged(OnColorChanged));
            m_LineColor.normalBgSprite = "";
            m_LineColor.focusedBgSprite = "";
            m_LineColor.hoveredBgSprite = "";
            m_LineColor.width = 40;
            m_LineColor.height = 40;
            //m_LineColor.atlas = LineUtilsTextureAtlas.instance.atlas;
            m_LineNumberFormatted = m_LineColor.GetComponentInChildren<UIButton>();
            m_LineNumberFormatted.textScale = 1.5f;
            m_LineNumberFormatted.useOutline = true;
        }

        private void AwakeShowLineButton()
        {
            m_LineIsVisible = m_uIHelper.AddCheckboxNoLabel("LineVisibility");
            m_LineIsVisible.eventCheckChanged += (x, y) => ChangeLineVisibility(y);
            ((UISprite) m_LineIsVisible.checkedBoxObject).spriteName = "LineVisibilityToggleOn";
            ((UISprite) m_LineIsVisible.checkedBoxObject).tooltipLocaleID = "PUBLICTRANSPORT_HIDELINE";
            ((UISprite) m_LineIsVisible.checkedBoxObject).isTooltipLocalized = true;
            ;
            ((UISprite) m_LineIsVisible.checkedBoxObject).size = new Vector2(24, 24);
            ((UISprite) m_LineIsVisible.components[0]).spriteName = "LineVisibilityToggleOff";
            ((UISprite) m_LineIsVisible.components[0]).tooltipLocaleID = "PUBLICTRANSPORT_SHOWLINE";
            ((UISprite) m_LineIsVisible.components[0]).isTooltipLocalized = true;
            ((UISprite) m_LineIsVisible.components[0]).size = new Vector2(24, 24);
            m_LineIsVisible.relativePosition = new Vector3(20, 10);
        }

        private void AwakeBudgetTimeLabels(TransportSystemDefinition tsd)
        {
            if (tsd.hasVehicles())
            {
                AwakeDayNightChecks();
                KlyteMonoUtils.CreateUIElement(out m_perHourBudgetInfo, transform);
                m_perHourBudgetInfo.name = "PerHourIndicator";
                m_perHourBudgetInfo.autoSize = false;
                m_perHourBudgetInfo.autoHeight = true;
                m_perHourBudgetInfo.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
                m_perHourBudgetInfo.width = 180;
                m_perHourBudgetInfo.height = m_perHourBudgetInfo.parent.height;
                m_perHourBudgetInfo.verticalAlignment = UIVerticalAlignment.Middle;
                m_perHourBudgetInfo.textAlignment = UIHorizontalAlignment.Center;
                m_perHourBudgetInfo.textScale = 1f;
                m_perHourBudgetInfo.localeID = "K45_TLM_PER_HOUR_BUDGET_ACTIVE_LABEL";
                m_perHourBudgetInfo.wordWrap = true;
                m_perHourBudgetInfo.eventTextChanged += constraintedScale;
                constraintedScale(m_perHourBudgetInfo, "");
            }
        }

        private void AwakeAutoButtons()
        {
            //Auto color & Auto Name
            KlyteMonoUtils.CreateUIElement(out UIButton buttonAutoName, transform);
            buttonAutoName.pivot = UIPivotPoint.TopRight;
            buttonAutoName.relativePosition = new Vector3(164, 0);
            buttonAutoName.text = "A";
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.width = 15;
            buttonAutoName.height = 15;
            buttonAutoName.tooltip = Locale.Get("K45_TLM_AUTO_NAME_SIMPLE_BUTTON_TOOLTIP");
            KlyteMonoUtils.InitButton(buttonAutoName, true, "ButtonMenu");
            buttonAutoName.name = "AutoName";
            buttonAutoName.isVisible = true;
            buttonAutoName.eventClick += (component, eventParam) =>
            {
                DoAutoName();
            };

            KlyteMonoUtils.CreateUIElement(out UIButton buttonAutoColor, transform);
            buttonAutoColor.pivot = UIPivotPoint.TopRight;
            buttonAutoColor.relativePosition = new Vector3(83, 0);
            buttonAutoColor.text = "A";
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 15;
            buttonAutoColor.height = 15;
            buttonAutoColor.tooltip = Locale.Get("K45_TLM_AUTO_COLOR_SIMPLE_BUTTON_TOOLTIP");
            KlyteMonoUtils.InitButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.name = "AutoColor";
            buttonAutoColor.isVisible = true;
            buttonAutoColor.eventClick += (component, eventParam) =>
            {
                DoAutoColor();
            };
        }

        private void AwakeLineDetail()
        {
            KlyteMonoUtils.CreateUIElement(out UIButton view, transform, "ViewLine", new Vector4(784, 5, 28, 28));
            KlyteMonoUtils.InitButton(view, true, "LineDetailButton");
            view.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (m_LineID != 0)
                {
                    Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_stops].m_position;
                    InstanceID instanceID = default;
                    instanceID.TransportLine = m_LineID;
                    TLMController.instance.lineInfoPanel.openLineInfo(lineID);
                    TLMController.instance.CloseTLMPanel();
                }
            };
            component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    RefreshData(true, true);
                }
            };
        }

        private void AwakeDeleteLine()
        {
            KlyteMonoUtils.CreateUIElement(out UIButton view, transform, "DeleteLine", new Vector4(816, 5, 28, 28));
            KlyteMonoUtils.InitButton(view, true, "DeleteLineButton");
            view.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (m_LineID != 0)
                {
                    ConfirmPanel.ShowModal("CONFIRM_LINEDELETE", delegate (UIComponent comp, int ret)
                    {
                        if (ret == 1)
                        {
                            Singleton<SimulationManager>.instance.AddAction(delegate
                            {
                                Singleton<TransportManager>.instance.ReleaseLine(m_LineID);
                                GameObject.Destroy(gameObject);
                            });
                        }
                    });
                }
            };
        }

        private void AwakeVehicleLabels(TransportSystemDefinition tsd)
        {
            if (tsd.hasVehicles())
            {
                KlyteMonoUtils.CreateUIElement(out m_LineVehicles, transform, "LineVehicles");
                m_LineVehicles.autoSize = true;
                m_LineVehicles.pivot = UIPivotPoint.TopLeft;
                m_LineVehicles.verticalAlignment = UIVerticalAlignment.Middle;
                m_LineVehicles.minimumSize = new Vector2(80, 18);
                m_LineVehicles.relativePosition = new Vector2(445, 0);
                m_LineVehicles.textAlignment = UIHorizontalAlignment.Center;
                m_LineVehicles.textColor = ForegroundColor;
                KlyteMonoUtils.LimitWidth(m_LineVehicles);

                m_lineBudgetLabel = GameObject.Instantiate(m_LineStops, m_LineStops.transform.parent);
                m_lineBudgetLabel.relativePosition = new Vector3(m_LineVehicles.relativePosition.x, 19, 0);
                KlyteMonoUtils.LimitWidth(m_lineBudgetLabel);
            }

        }

        private void AwakeLabels()
        {
            KlyteMonoUtils.CreateUIElement(out m_LineStops, transform, "LineStops");
            m_LineStops.textAlignment = UIHorizontalAlignment.Center;
            m_LineStops.textColor = ForegroundColor;
            m_LineStops.minimumSize = new Vector2(80, 18);
            m_LineStops.relativePosition = new Vector3(540, 10);
            m_LineStops.pivot = UIPivotPoint.TopLeft;
            m_LineStops.wordWrap = false;
            m_LineStops.autoSize = true;
            KlyteMonoUtils.LimitWidth(m_LineStops);

            m_LinePassengers = Instantiate(m_LineStops);
            m_LinePassengers.transform.SetParent(m_LineStops.transform.parent);
            m_LinePassengers.name = "LinePassengers";
            m_LineStops.relativePosition = new Vector3(340, 10);

        }

        private void AwakeDayNightChecks()
        {
            GameObject temp = UITemplateManager.Get<UICheckBox>("OptionsCheckBoxTemplate").gameObject;
            m_DayLine = m_uIHelper.AddCheckboxNoLabel("DayLine");
            m_NightLine = m_uIHelper.AddCheckboxNoLabel("NightLine");
            m_DayNightLine = m_uIHelper.AddCheckboxNoLabel("DayNightLine");
            m_DisabledLine = m_uIHelper.AddCheckboxNoLabel("DisabledLine");

            m_DayLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                ushort lineID = m_LineID;
                if (Singleton<SimulationManager>.exists && lineID != 0)
                {
                    m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        changeLineTime(true, false);
                    });
                }
            };
            m_NightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                ushort lineID = m_LineID;
                if (Singleton<SimulationManager>.exists && lineID != 0)
                {
                    m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        changeLineTime(false, true);
                    });
                }
            };
            m_DayNightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                ushort lineID = m_LineID;
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
                ushort lineID = m_LineID;
                if (Singleton<SimulationManager>.exists && lineID != 0)
                {
                    m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        changeLineTime(false, false);
                    });
                }
            };


            m_DayLine.relativePosition = new Vector3(657, 8);
            m_NightLine.relativePosition = new Vector3(683, 8);
            m_DayNightLine.relativePosition = new Vector3(709, 8);
            m_DisabledLine.relativePosition = new Vector3(735, 8);
        }

        private void AwakeLineName()
        {
            KlyteMonoUtils.CreateUIElement(out m_LineName, transform, "LineName", new Vector4(146, 2, 198, 35));
            m_LineName.textColor = ForegroundColor;
            m_LineName.textAlignment = UIHorizontalAlignment.Center;
            m_LineName.verticalAlignment = UIVerticalAlignment.Middle;
            m_LineName.wordWrap = true;
            KlyteMonoUtils.CreateUIElement(out m_LineNameField, transform, "LineNameField", new Vector4(146, 10, 198, 20));
            m_LineNameField.maxLength = 256;
            m_LineNameField.isVisible = false;
            m_LineNameField.verticalAlignment = UIVerticalAlignment.Middle;
            m_LineNameField.horizontalAlignment = UIHorizontalAlignment.Center;
            m_LineNameField.selectionSprite = "EmptySprite";
            m_LineNameField.builtinKeyNavigation = true;
            m_LineName.eventMouseEnter += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_LineName.backgroundSprite = "TextFieldPanelHovered";
            };
            m_LineName.eventMouseLeave += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_LineName.backgroundSprite = string.Empty;
            };
            m_LineName.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                m_LineName.Hide();
                m_LineNameField.Show();
                m_LineNameField.text = m_LineName.text;
                m_LineNameField.Focus();
            };
            m_LineNameField.eventLeaveFocus += delegate (UIComponent c, UIFocusEventParameter r)
            {
                m_LineNameField.Hide();
                TLMLineUtils.setLineName(lineID, m_LineNameField.text);
                m_LineName.Show();
                m_LineName.text = m_LineNameField.text;
            };
        }


        private void AwakeBG()
        {
            m_uIHelper = new UIHelperExtension(GetComponent<UIPanel>());
            KlyteMonoUtils.CreateUIElement<UIPanel>(out m_Background, transform, "BG");
            m_mouseIsOver = false;
            component.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            component.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            m_Background.width = 844;
            m_Background.height = 38;

            m_Background.backgroundSprite = "InfoviewPanel";

        }

        public void ChangeLineVisibility(bool r)
        {
            if (m_LineID != 0 & !m_isUpdatingVisibility)
            {
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    if (r)
                    {
                        TransportLine[] expr_2A_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                        ushort expr_2A_cp_1 = m_LineID;
                        expr_2A_cp_0[expr_2A_cp_1].m_flags = (expr_2A_cp_0[expr_2A_cp_1].m_flags & ~TransportLine.Flags.Hidden);
                    }
                    else
                    {
                        TransportLine[] expr_5C_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                        ushort expr_5C_cp_1 = m_LineID;
                        expr_5C_cp_0[expr_5C_cp_1].m_flags = (expr_5C_cp_0[expr_5C_cp_1].m_flags | TransportLine.Flags.Hidden);
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

        public void DoAutoName() => TLMController.instance.AutoName(m_LineID);

        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!m_mouseIsOver)
            {
                m_mouseIsOver = true;
                if (m_LineID != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                        {
                            TransportLine[] expr_40_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                            ushort expr_40_cp_1 = m_LineID;
                            expr_40_cp_0[expr_40_cp_1].m_flags = (expr_40_cp_0[expr_40_cp_1].m_flags | TransportLine.Flags.Highlighted);
                        }
                    });
                }
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (m_mouseIsOver)
            {
                m_mouseIsOver = false;
                if (m_LineID != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                        {
                            TransportLine[] expr_40_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
                            ushort expr_40_cp_1 = m_LineID;
                            expr_40_cp_0[expr_40_cp_1].m_flags = (expr_40_cp_0[expr_40_cp_1].m_flags & ~TransportLine.Flags.Highlighted);
                        }
                    });
                }
            }
        }

        private void OnEnable()
        {
            Singleton<TransportManager>.instance.eventLineColorChanged += new TransportManager.LineColorChangedHandler(OnLineChanged);
            Singleton<TransportManager>.instance.eventLineNameChanged += new TransportManager.LineNameChangedHandler(OnLineChanged);
        }

        private void OnDisable()
        {
            Singleton<TransportManager>.instance.eventLineColorChanged -= new TransportManager.LineColorChangedHandler(OnLineChanged);
            Singleton<TransportManager>.instance.eventLineNameChanged -= new TransportManager.LineNameChangedHandler(OnLineChanged);
        }

        private void OnRename(UIComponent comp, string text) => TLMLineUtils.setLineName(m_LineID, text);

        private void OnLineChanged(ushort id)
        {
            if (id == m_LineID)
            {
                RefreshData(true, true);
            }
        }

        private void OnColorChanged(Color color)
        {
            TLMUtils.doLog($"COLOR CHANGED!! {color}\n{Environment.StackTrace}");

            TLMLineUtils.setLineColor(m_LineID, color);
        }
        private void changeLineTime(bool day, bool night)
        {
            m_LineOperation = Singleton<SimulationManager>.instance.AddAction(delegate
            {
                TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[m_LineID], day, night);
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
