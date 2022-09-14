using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.ModShared;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections;
using UnityEngine;
using static Klyte.TransportLinesManager.UI.UVMPublicTransportWorldInfoPanel.UVMPublicTransportWorldInfoPanelObject;

namespace Klyte.TransportLinesManager.UI
{

    public class UVMMainWIPTab : UICustomControl, IUVMPTWIPChild
    {

        private UIPanel m_bg;

        #region Overridable

        public void Awake()
        {
            m_bg = component as UIPanel;
            m_bg.autoLayout = false;


            PublicTransportWorldInfoPanel ptwip = UVMPublicTransportWorldInfoPanel.m_obj.origInstance;

            BindFields(ptwip);
            m_lineColorContainer.relativePosition = new Vector3(170, 10);
            m_lineLengthLabel.relativePosition = new Vector3(10, 45);
            m_passengersContainer.relativePosition = new Vector3(10, 70);
            m_ageChartContainer.relativePosition = new Vector3(10, 90);
            m_tripSavedContainer.relativePosition = new Vector3(10, 220);
            m_pullValuePanel.relativePosition = new Vector3(10, 220);

            SetColorPickerEvents();
            SetColorButtonEvents();
            SetLegendColors(ptwip);
            SetDemographicGraphicColors(ptwip);

            CreatePrefixAndLineNumberEditor();
            CreateFirstStopSelector();
            CreateActionButtonsRow();
            CreateCustomLineCodeEditor();
        }

        private void BindFields(PublicTransportWorldInfoPanel ptwip)
        {
            LogUtils.DoLog("COLOR");
            UIComponent parentToDestroy = ptwip.Find<UILabel>("Color").parent.parent;
            m_lineColorContainer = RebindUI(ptwip.Find<UILabel>("Color").parent);
            m_colorField = component.Find<UIColorField>("ColorField");
            m_colorField.gameObject.AddComponent<UIColorFieldExtension>();
            m_colorFieldButton = m_colorField.Find<UIButton>("Button");

            LogUtils.DoLog("CHART");
            m_ageChart = (ptwip.Find<UIRadialChart>("AgeChart"));
            m_childLegend = (ptwip.Find<UILabel>("ChildAmount"));
            m_teenLegend = (ptwip.Find<UILabel>("TeenAmount"));
            m_youngLegend = (ptwip.Find<UILabel>("YoungAmount"));
            m_adultLegend = (ptwip.Find<UILabel>("AdultAmount"));
            m_seniorLegend = (ptwip.Find<UILabel>("SeniorAmount"));
            UIComponent parentToDestroy2 = m_ageChart.parent.parent;
            m_ageChartContainer = RebindUI(m_ageChart.parent);

            LogUtils.DoLog("TRIP");
            m_tripSaved = (ptwip.Find<UILabel>("TripSaved"));
            m_tripSavedContainer = RebindUI(m_tripSaved.parent);


            LogUtils.DoLog("TYPE");
            m_type = (ptwip.Find<UILabel>("Type"));
            m_typeContainer = RebindUI(m_type.parent);

            LogUtils.DoLog("PASSENGERS");
            m_passengers = (ptwip.Find<UILabel>("Passengers"));
            m_passengersContainer = RebindUI(m_passengers.parent);

            LogUtils.DoLog("LENGTH");
            m_lineLengthLabel = RebindUI(ptwip.Find<UILabel>("LineLengthLabel"));

            LogUtils.DoLog("WALKING");
            m_pullValuePanel = RebindUI(ptwip.Find<UIPanel>("WalkingTourPullValuePanel"));
            m_pullValue = (component.Find<UILabel>("PullValue"));
            m_warningTooLongText = (component.Find<UILabel>("WarningTooLongText"));
            m_warningTooLongIcon = (component.Find<UISprite>("WarningTooLongIcon"));

            LogUtils.DoLog("DESTROY");
            UVMPublicTransportWorldInfoPanel.FakeDestroy(parentToDestroy);
            UVMPublicTransportWorldInfoPanel.FakeDestroy(parentToDestroy2);
        }

        private T RebindUI<T>(T component) where T : UIComponent
        {
            Vector3 relPos = component.relativePosition;
            component.transform.SetParent(this.component.transform);
            component.relativePosition = relPos;
            return component;
        }

        private void SetColorButtonEvents()
        {
            m_colorField.eventColorPickerOpen += delegate (UIColorField field, UIColorPicker picker, ref bool overridden)
            {
                m_colorFieldButton.isInteractive = false;
            };
            m_colorField.eventColorPickerClose += delegate (UIColorField field, UIColorPicker picker, ref bool overridden)
            {
                m_colorFieldButton.isInteractive = true;
            };
        }
        private void SetColorPickerEvents() => m_colorField.eventSelectedColorChanged += OnColorChanged;
        private void SetLegendColors(PublicTransportWorldInfoPanel __instance)
        {
            m_childLegend.color = __instance.m_ChildColor;
            m_teenLegend.color = __instance.m_TeenColor;
            m_youngLegend.color = __instance.m_YoungColor;
            m_adultLegend.color = __instance.m_AdultColor;
            m_seniorLegend.color = __instance.m_SeniorColor;
        }

        private void SetDemographicGraphicColors(PublicTransportWorldInfoPanel __instance)
        {
            UIRadialChart.SliceSettings slice = m_ageChart.GetSlice(0);
            Color32 color = __instance.m_ChildColor;
            m_ageChart.GetSlice(0).outterColor = color;
            slice.innerColor = color;
            UIRadialChart.SliceSettings slice2 = m_ageChart.GetSlice(1);
            color = __instance.m_TeenColor;
            m_ageChart.GetSlice(1).outterColor = color;
            slice2.innerColor = color;
            UIRadialChart.SliceSettings slice3 = m_ageChart.GetSlice(2);
            color = __instance.m_YoungColor;
            m_ageChart.GetSlice(2).outterColor = color;
            slice3.innerColor = color;
            UIRadialChart.SliceSettings slice4 = m_ageChart.GetSlice(3);
            color = __instance.m_AdultColor;
            m_ageChart.GetSlice(3).outterColor = color;
            slice4.innerColor = color;
            UIRadialChart.SliceSettings slice5 = m_ageChart.GetSlice(4);
            color = __instance.m_SeniorColor;
            m_ageChart.GetSlice(4).outterColor = color;
            slice5.innerColor = color;
        }

        public void OnEnable() => Singleton<TransportManager>.instance.eventLineColorChanged += OnLineColorChanged;

        public void OnDisable() => Singleton<TransportManager>.instance.eventLineColorChanged -= OnLineColorChanged;

        private int colorChangeCooldown = 0;
        internal void OnColorChanged(UIComponent comp, Color color)
        {
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && lineId > 0 && !fromBuilding)
            {
                UVMPublicTransportWorldInfoPanel.m_obj.origInstance.StartCoroutine(ChangeColorCoroutine(lineId, color));
            }
        }

        private IEnumerator ChangeColorCoroutine(ushort id, Color color)
        {
            if (colorChangeCooldown > 0)
            {
                yield break;
            }
            colorChangeCooldown = 3;
            do
            {
                colorChangeCooldown--;
                yield return 0;
            } while (colorChangeCooldown > 0);

            if (Singleton<SimulationManager>.exists)
            {
                AsyncTask<bool> task = Singleton<SimulationManager>.instance.AddAction(Singleton<TransportManager>.instance.SetLineColor(id, color));
                yield return task.WaitTaskCompleted(this);
                if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && lineId == id && !fromBuilding)
                {
                    m_colorField.selectedColor = Singleton<TransportManager>.instance.GetLineColor(id);
                }
            }
            yield break;
        }
        public void OnSetTarget(Type source)
        {
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineID, out bool fromBuilding))
            {
                m_firstStopSelect.items = TLMLineUtils.GetAllStopsFromLine(lineID, fromBuilding);
                m_firstStopSelect.selectedIndex = 0;
                if (source == GetType())
                {
                    return;
                }

                if (lineID != 0)
                {
                    m_colorField.selectedColor = Singleton<TransportManager>.instance.GetLineColor(lineID);
                    LineType lineType = UVMPublicTransportWorldInfoPanel.GetLineType(lineID, fromBuilding);
                    m_weeklyPassengersString = ((lineType != LineType.WalkingTour) ? "TRANSPORT_LINE_PASSENGERS" : "TRANSPORT_LINE_PASSENGERS_WALKINGTOUR");
                    m_ageChart.tooltipLocaleID = ((lineType != LineType.WalkingTour) ? "PUBLICTRANSPORT_PASSENGERAGEGROUPS_TOOLTIP" : "PUBLICTRANSPORT_PASSENGERAGEGROUPS_TOOLTIP_WALKINGTOUR");
                    m_tripSaved.isVisible = (lineType == LineType.Default);
                    m_pullValuePanel.isVisible = (lineType == LineType.WalkingTour);
                    m_lineLengthLabel.text = StringUtils.SafeFormat(Locale.Get("LINEINFOPANEL_LINELENGTH"), (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_totalLength / 1000f).ToString("F2", LocaleManager.cultureInfo));

                    m_linePrefixDropDown.eventSelectedIndexChanged -= SaveLineNumber;
                    m_lineNumberLabel.eventLostFocus -= SaveLineNumber;
                    m_customLineCodeInput.eventTextSubmitted -= SaveLineCode;

                    ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[lineID];
                    ushort lineNumber = t.m_lineNumber;

                    var tsd = TransportSystemDefinition.GetDefinitionForLine(lineID, fromBuilding);
                    var config = tsd.GetConfig();
                    var mnPrefixo = config.Prefix;

                    if (TLMPrefixesUtils.HasPrefix(lineID))
                    {
                        m_lineNumberLabel.maxLength = 3;
                        m_lineNumberLabel.width = 40;
                        m_lineNumberLabel.text = (lineNumber % 1000).ToString();
                        string[] temp = TLMPrefixesUtils.GetStringOptionsForPrefix(tsd, true, true, false);
                        m_linePrefixDropDown.items = temp;
                        m_linePrefixDropDown.selectedIndex = lineNumber / 1000;
                        bool invertPrefixSuffix = config.InvertPrefixSuffix;
                        if (invertPrefixSuffix)
                        {
                            m_linePrefixDropDown.zOrder = 9999;
                        }
                        else
                        {
                            m_lineNumberLabel.zOrder = 9999;
                        }
                        m_linePrefixDropDown.enabled = true;
                    }
                    else
                    {
                        m_lineNumberLabel.maxLength = 4;
                        m_lineNumberLabel.width = 180;
                        m_lineNumberLabel.text = (lineNumber).ToString();
                        m_linePrefixDropDown.enabled = false;
                    }



                    m_lineNumberLabel.color = TransportManager.instance.GetLineColor(lineID);
                    m_customLineCodeInput.text = TLMTransportLineExtension.Instance.SafeGet(lineID).CustomCode ?? "";


                    m_linePrefixDropDown.eventSelectedIndexChanged += SaveLineNumber;
                    m_lineNumberLabel.eventLostFocus += SaveLineNumber;
                    m_customLineCodeInput.eventTextSubmitted += SaveLineCode;
                }
            }
        }

        private uint m_lastDrawTick;
        public void UpdateBindings()
        {
            if (component.isVisible && m_lastDrawTick + 31 < SimulationManager.instance.m_currentTickIndex)
            {
                UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineID, out bool fromBuilding);
                if (lineID != 0 && !fromBuilding)
                {
                    TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].Info;
                    m_type.text = Locale.Get("TRANSPORT_LINE", info.name);
                    int averageCount = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_touristPassengers.m_averageCount;
                    int averageCount2 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_residentPassengers.m_averageCount;
                    int total = averageCount + averageCount2;
                    int averageCount3 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_childPassengers.m_averageCount;
                    int averageCount4 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_teenPassengers.m_averageCount;
                    int averageCount5 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_youngPassengers.m_averageCount;
                    int averageCount6 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_adultPassengers.m_averageCount;
                    int averageCount7 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_seniorPassengers.m_averageCount;
                    int averageCount8 = (int)Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_passengers.m_carOwningPassengers.m_averageCount;
                    int percentageValue = GetPercentageValue(averageCount3, total);
                    int percentageValue2 = GetPercentageValue(averageCount4, total);
                    int percentageValue3 = GetPercentageValue(averageCount5, total);
                    int num3 = GetPercentageValue(averageCount6, total);
                    int percentageValue4 = GetPercentageValue(averageCount7, total);
                    int num4 = percentageValue + percentageValue2 + percentageValue3 + num3 + percentageValue4;
                    if (num4 != 0 && num4 != 100)
                    {
                        num3 = 100 - (percentageValue + percentageValue2 + percentageValue3 + percentageValue4);
                    }
                    m_ageChart.SetValues(new int[]
                    {
                percentageValue,
                percentageValue2,
                percentageValue3,
                num3,
                percentageValue4
                    });
                    m_passengers.text = LocaleFormatter.FormatGeneric(m_weeklyPassengersString, new object[]
                    {
                averageCount2,
                averageCount
                    });
                    int num5 = 0;
                    int num6 = 0;
                    if (averageCount2 + averageCount != 0)
                    {
                        num6 += averageCount3 * 0;
                        num6 += averageCount4 * 5;
                        num6 += averageCount5 * (((15 * averageCount2) + (20 * averageCount) + ((averageCount2 + averageCount) >> 1)) / (averageCount2 + averageCount));
                        num6 += averageCount6 * (((20 * averageCount2) + (20 * averageCount) + ((averageCount2 + averageCount) >> 1)) / (averageCount2 + averageCount));
                        num6 += averageCount7 * (((10 * averageCount2) + (20 * averageCount) + ((averageCount2 + averageCount) >> 1)) / (averageCount2 + averageCount));
                    }
                    if (num6 != 0)
                    {
                        num5 = (int)(((averageCount8 * 10000L) + (num6 >> 1)) / num6);
                        num5 = Mathf.Clamp(num5, 0, 100);
                    }
                    m_tripSaved.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_TRIPSAVED", new object[]
                    {
                num5
                    });
                    if (m_pullValuePanel.isVisible)
                    {
                        m_pullValue.text = StringUtils.SafeFormat(Locale.Get("PUBTRANSWORLDINFOPANEL_PULLVALUE"), Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].GetPullValue(lineID));
                        ushort stop = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].GetStop(0);
                        if ((Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_problems & Notification.Problem1.TooLong) != Notification.Problem1.None)
                        {
                            m_warningTooLongIcon.isVisible = true;
                            m_warningTooLongText.isVisible = true;
                            if ((Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_problems & Notification.Problem1.MajorProblem) != Notification.Problem1.None)
                            {
                                m_warningTooLongText.text = Locale.Get("PUBTRANSWORLDINFOPANEL_LINEWAYTOOLONG");
                                m_warningTooLongIcon.spriteName = "BuildingNotificationTooLongTour";
                            }
                            else
                            {
                                m_warningTooLongText.text = Locale.Get("PUBTRANSWORLDINFOPANEL_LINETOOLONG");
                                m_warningTooLongIcon.spriteName = "BuildingNotificationVeryLongTour";
                            }
                        }
                        else
                        {
                            m_warningTooLongIcon.isVisible = false;
                            m_warningTooLongText.isVisible = false;
                        }
                    }
                }
                m_lastDrawTick = SimulationManager.instance.m_currentTickIndex;
            }
        }
        #endregion

        private void OnLineColorChanged(ushort id)
        {
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding && id == lineId)
            {
                m_colorField.selectedColor = Singleton<TransportManager>.instance.GetLineColor(id);
            }
        }

        private int GetPercentageValue(int value, int total)
        {
            float num = value / (float)total;
            return Mathf.Clamp(Mathf.FloorToInt(num * 100f), 0, 100);
        }

        public void Hide() => m_bg.isVisible = false;
        public void OnGotFocus() { }
        public bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && lineId > 0 && !fromBuilding;

        #region Number & Prefix edit

        private void CreatePrefixAndLineNumberEditor()
        {
            m_linePrefixDropDown = UIHelperExtension.CloneBasicDropDownLocalized("K45_TLM_PREFIX_LINE_DETAILS_DD_LABEL", new string[1], (x) => StartCoroutine(SaveLineNumber()), 0, m_bg, out UILabel label, out UIPanel container);
            container.autoFitChildrenHorizontally = false;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayout = false;
            ReflectionUtils.GetEventField(typeof(UIDropDown), "eventMouseWheel")?.SetValue(m_linePrefixDropDown, null);
            m_linePrefixDropDown.isLocalized = false;
            m_linePrefixDropDown.autoSize = false;
            m_linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;
            m_linePrefixDropDown.itemPadding = new RectOffset(2, 2, 2, 2);
            m_linePrefixDropDown.textFieldPadding = new RectOffset(4, 4, 4, 4);
            m_linePrefixDropDown.name = "LinePrefixDropDown";
            m_linePrefixDropDown.size = new Vector3(140, 22);
            m_linePrefixDropDown.textScale = 1;
            m_linePrefixDropDown.listPosition = UIDropDown.PopupListPosition.Automatic;
            KlyteMonoUtils.InitButtonFull(m_linePrefixDropDown, false, "OptionsDropboxListbox");
            m_linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.LimitWidthAndBox(label, 200);
            label.textScale = 1;
            label.padding.top = 4;
            label.position = Vector3.zero;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Left;

            KlyteMonoUtils.CreateUIElement(out m_lineNumberLabel, container.transform);
            m_lineNumberLabel.autoSize = false;
            m_lineNumberLabel.horizontalAlignment = UIHorizontalAlignment.Right;
            m_lineNumberLabel.text = "";
            m_lineNumberLabel.name = "LineNumberLabel";
            m_lineNumberLabel.normalBgSprite = "EmptySprite";
            m_lineNumberLabel.textScale = 1f;
            m_lineNumberLabel.padding = new RectOffset(4, 4, 4, 4);
            m_lineNumberLabel.color = new Color(0, 0, 0, 1);
            KlyteMonoUtils.UiTextFieldDefaults(m_lineNumberLabel);
            m_lineNumberLabel.numericalOnly = true;
            m_lineNumberLabel.maxLength = 4;
            m_lineNumberLabel.eventLostFocus += SaveLineNumber;
            m_lineNumberLabel.size = new Vector4(40, 22);
            container.autoLayout = true;
            container.relativePosition = new Vector3(0, 375);
        }


        private void SaveLineNumber<T>(UIComponent c, T v) => StartCoroutine(SaveLineNumber());

        private IEnumerator SaveLineNumber()
        {
            yield return 0;
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
            if (!fromBuilding && lineId > 0)
            {
                string value = m_lineNumberLabel.text;
                int valPrefixo = m_linePrefixDropDown.selectedIndex;
                var tsd = TransportSystemDefinition.FromLineId(lineId, fromBuilding);
                var hasPrefix = TLMPrefixesUtils.HasPrefix(tsd);
                ushort.TryParse(value, out ushort num);
                if (hasPrefix)
                {
                    num = (ushort)(valPrefixo * 1000 + (num % 1000));
                }
                if (num < 1)
                {
                    m_lineNumberLabel.textColor = new Color(1, 0, 0, 1);
                    yield break;
                }
                bool numeroUsado = IsLineNumberAlredyInUse(num, lineId);

                if (numeroUsado)
                {
                    m_lineNumberLabel.textColor = new Color(1, 0, 0, 1);
                }
                else
                {
                    m_lineNumberLabel.textColor = new Color(1, 1, 1, 1);
                    Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_lineNumber = num;
                    TLMFacade.Instance.OnLineSymbolParameterChanged();
                    if (hasPrefix)
                    {
                        m_lineNumberLabel.text = (num % 1000).ToString();
                        m_linePrefixDropDown.selectedIndex = (num / 1000);
                    }
                    else
                    {
                        m_lineNumberLabel.text = (num % 10000).ToString();
                    }
                    UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
                }
                yield break;
            }
        }

        private bool IsLineNumberAlredyInUse(int numLinha, ushort lineIdx) => TLMLineUtils.IsLineNumberAlredyInUse(numLinha, TransportSystemDefinition.GetDefinitionForLine(lineIdx, false), lineIdx);
        #endregion

        #region Custom line code

        private void CreateCustomLineCodeEditor()
        {
            m_customLineCodeInput = UIHelperExtension.AddTextfield(m_bg, Locale.Get("K45_TLM_CUSTOMLINECODE"), "", out UILabel lbl, out UIPanel container);
            container.width = m_bg.width / 2;
            container.autoLayout = true;
            container.autoLayoutDirection = LayoutDirection.Vertical;
            KlyteMonoUtils.LimitWidthAndBox(lbl, m_bg.width / 2, true);
            lbl.textAlignment = UIHorizontalAlignment.Center;

            m_customLineCodeInput.autoSize = false;
            m_customLineCodeInput.horizontalAlignment = UIHorizontalAlignment.Center;
            m_customLineCodeInput.text = "";
            m_customLineCodeInput.maxLength = 20;
            m_customLineCodeInput.submitOnFocusLost = true;
            m_customLineCodeInput.eventTextSubmitted += SaveLineCode;
            m_customLineCodeInput.size = new Vector4(m_bg.width / 2, 25);
            container.relativePosition = new Vector3(m_bg.width / 2, 315);
        }

        private void SaveLineCode(UIComponent component, string text)
        {
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && lineId > 0 && !fromBuilding)
            {
                TLMTransportLineExtension.Instance.SafeGet(lineId).CustomCode = text;
                UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
            }
        }
        #endregion

        #region First stop
        private void CreateFirstStopSelector()
        {
            m_firstStopSelect = UIHelperExtension.CloneBasicDropDownLocalized("K45_TLM_FIRST_STOP_DD_LABEL", new string[1], ChangeFirstStop, 0, m_bg, out UILabel label, out UIPanel container);
            ReflectionUtils.GetEventField(typeof(UIDropDown), "eventMouseWheel")?.SetValue(m_firstStopSelect, null);

            container.autoFitChildrenHorizontally = false;
            container.relativePosition = new Vector3(0, 410);
            m_firstStopSelect.width = 340;
            m_firstStopSelect.transform.localScale = Vector3.one * 0.75f;
            m_firstStopSelect.listPosition = UIDropDown.PopupListPosition.Automatic;

            KlyteMonoUtils.LimitWidthAndBox(label, 120);
            label.textScale = 1;
            label.padding.top = 7;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Left;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayout = true;
            container.ResetLayout(false, true);
        }
        private void ChangeFirstStop(int idxSel)
        {
            if (idxSel <= 0 || idxSel >= m_firstStopSelect.items.Length)
            {
                return;
            }
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
            if (!fromBuilding)
            {
                TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineId];
                if ((t.m_flags & TransportLine.Flags.Invalid) != TransportLine.Flags.None)
                {
                    return;
                }
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].m_stops = t.GetStop(idxSel);
                UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
                if (TLMBaseConfigXML.Instance.UseAutoName)
                {
                    TLMController.AutoName(lineId);
                }
                TransportLinesManagerMod.Controller.SharedInstance.OnAutoNameParameterChanged();
            }
        }
        #endregion

        #region Action buttons row
        private void CreateActionButtonsRow()
        {
            KlyteMonoUtils.CreateUIElement(out UIButton buttonAutoName, transform);
            buttonAutoName.textScale = 0.6f;
            buttonAutoName.relativePosition = new Vector3(45, 325);
            buttonAutoName.width = 40;
            buttonAutoName.height = 40;
            buttonAutoName.tooltip = Locale.Get("K45_TLM_USE_AUTO_NAME");
            KlyteMonoUtils.InitButton(buttonAutoName, true, "ButtonMenu");
            buttonAutoName.name = "AutoName";
            buttonAutoName.isVisible = true;
            buttonAutoName.eventClicked += (component, eventParam) =>
            {
                UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
                if (!fromBuilding && lineId > 0)
                {
                    TLMController.AutoName(lineId);
                    UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
                }
            };
            buttonAutoName.normalFgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoNameIcon);

            KlyteMonoUtils.CreateUIElement(out UIButton buttonAutoColor, transform);
            buttonAutoColor.relativePosition = new Vector3(0f, 325f);
            buttonAutoColor.textScale = 0.6f;
            buttonAutoColor.width = 40;
            buttonAutoColor.height = 40;
            buttonAutoColor.tooltip = Locale.Get("K45_TLM_PICK_COLOR_FROM_PALETTE_TOOLTIP");
            KlyteMonoUtils.InitButton(buttonAutoColor, true, "ButtonMenu");
            buttonAutoColor.name = "AutoColor";
            buttonAutoColor.isVisible = true;
            buttonAutoColor.eventClicked += (component, eventParam) =>
            {
                if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && lineId > 0 && !fromBuilding)
                {
                    TLMController.AutoColor(lineId);
                }
            };
            buttonAutoColor.normalFgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoColorIcon);
        }
        #endregion

        internal UIRadialChart m_ageChart;

        internal UILabel m_childLegend;

        internal UILabel m_teenLegend;

        internal UILabel m_youngLegend;

        internal UILabel m_adultLegend;

        internal UILabel m_seniorLegend;
        private UIComponent m_ageChartContainer;
        private UIComponent m_lineColorContainer;
        internal UIColorField m_colorField;

        internal UIButton m_colorFieldButton;

        internal UILabel m_passengers;
        private UIComponent m_passengersContainer;
        internal UILabel m_type;
        private UIComponent m_typeContainer;
        internal UILabel m_tripSaved;
        private UIComponent m_tripSavedContainer;
        internal UILabel m_lineLengthLabel;
        internal UIPanel m_pullValuePanel;

        internal UILabel m_pullValue;

        internal UILabel m_warningTooLongText;

        internal UISprite m_warningTooLongIcon;

        internal string m_weeklyPassengersString;

        private UIDropDown m_linePrefixDropDown;
        private UITextField m_lineNumberLabel;
        private UITextField m_customLineCodeInput;
        private UIDropDown m_firstStopSelect;
    }
}