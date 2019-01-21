using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Linq;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.LineList.ExtraUI
{
    internal class TLMBudgetControlSliders : UICustomControl
    {


        private IBudgetControlParentInterface parent;
        private UIHelperExtension m_uiHelper;
        private UICheckBox m_IgnorePrefix;

        private UISlider[] m_budgetSliders = new UISlider[8];
        private UIButton m_enableBudgetPerHour;
        private UIButton m_disableBudgetPerHour;
        private UIButton m_absoluteCountMode;
        private UIButton m_multiplierMode;
        private UILabel m_lineBudgetSlidersTitle;

        private UICheckBox m_DayLine;
        private UICheckBox m_DayNightLine;
        private UICheckBox m_NightLine;
        private UICheckBox m_DisabledLine;

        public event OnLineLoad onIgnorePrefixChanged;
        public event OnItemSelectedChanged onDayNightChanged;


        public bool IgnorePrefix => m_IgnorePrefix.isChecked;

        private void Awake()
        {

            parent = transform.parent.GetComponent<IBudgetControlParentInterface>();
            if (parent == null)
            {
                TLMUtils.doErrorLog(string.Join(",", transform.parent.GetComponents<MonoBehaviour>().Select(x => x.GetType().ToString() + $" ({x.GetType().IsSubclassOf(typeof(IBudgetControlParentInterface)).ToString()})").ToArray()));
                throw new Exception("TLMBudgetControlSliders: PARENT PANEL ISN'T A BUDGET CONTROL PARENT!");
            }
            TLMUtils.createUIElement(out UIPanel m_budgetPanel, transform.parent, "BudgetPanel", new Vector4(0, 0, 600, 180));
            m_budgetPanel.isInteractive = false;
            m_uiHelper = new UIHelperExtension(m_budgetPanel);
            CreateIgnorePrefixBudgetOption();
            if (!parent.PrefixSelectionMode)
            {
                createDayNightBudgetControls();
            }
            CreateBudgetSliders();
            CreateActionButtons();

            TLMUtils.doLog("SLIDERS EVENTS II");
            parent.onSelectionChanged += OnLoadLine;
        }

        private void CreateActionButtons()
        {
            TLMUtils.createUIElement(out m_enableBudgetPerHour, m_uiHelper.self.transform);
            m_enableBudgetPerHour.relativePosition = new Vector3(m_uiHelper.self.width - 150f, m_uiHelper.self.height - 30f);
            m_enableBudgetPerHour.textScale = 0.6f;
            m_enableBudgetPerHour.width = 40;
            m_enableBudgetPerHour.height = 40;
            m_enableBudgetPerHour.tooltip = Locale.Get("TLM_USE_PER_PERIOD_BUDGET");
            TLMUtils.initButton(m_enableBudgetPerHour, true, "ButtonMenu");
            m_enableBudgetPerHour.name = "EnableBudgetPerHour";
            m_enableBudgetPerHour.isVisible = true;
            m_enableBudgetPerHour.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[parent.CurrentSelectedId];
                IBudgetableExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(parent.CurrentSelectedId))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = parent.CurrentSelectedId;
                }
                else
                {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                    idx = TLMLineUtils.getPrefix(parent.CurrentSelectedId);
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

            var icon = m_enableBudgetPerHour.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = TLMController.taTLM;
            icon.width = 36;
            icon.height = 36;
            icon.spriteName = "PerHourIcon";


            TLMUtils.createUIElement(out m_disableBudgetPerHour, m_uiHelper.self.transform);
            m_disableBudgetPerHour.relativePosition = new Vector3(m_uiHelper.self.width - 150f, m_uiHelper.self.height - 30f);
            m_disableBudgetPerHour.textScale = 0.6f;
            m_disableBudgetPerHour.width = 40;
            m_disableBudgetPerHour.height = 40;
            m_disableBudgetPerHour.tooltip = Locale.Get("TLM_USE_SINGLE_BUDGET");
            TLMUtils.initButton(m_disableBudgetPerHour, true, "ButtonMenu");
            m_disableBudgetPerHour.name = "DisableBudgetPerHour";
            m_disableBudgetPerHour.isVisible = true;
            m_disableBudgetPerHour.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[parent.CurrentSelectedId];

                IBudgetableExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(parent.CurrentSelectedId))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = parent.CurrentSelectedId;
                }
                else
                {
                    var tsd = TransportSystemDefinition.from(tl.Info);
                    bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                    idx = TLMLineUtils.getPrefix(parent.CurrentSelectedId);
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



            //Absolute toggle
            TLMUtils.createUIElement(out m_absoluteCountMode, m_uiHelper.self.transform);
            m_absoluteCountMode.relativePosition = new Vector3(m_uiHelper.self.width - 200f, m_uiHelper.self.height - 30f);
            m_absoluteCountMode.textScale = 0.6f;
            m_absoluteCountMode.width = 40;
            m_absoluteCountMode.height = 40;
            m_absoluteCountMode.tooltip = Locale.Get("TLM_USE_ABSOLUTE_BUDGET");
            TLMUtils.initButton(m_absoluteCountMode, true, "ButtonMenu");
            m_absoluteCountMode.name = "AbsoluteBudget";
            m_absoluteCountMode.isVisible = true;
            m_absoluteCountMode.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[parent.CurrentSelectedId];

                IUseAbsoluteVehicleCountExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(parent.CurrentSelectedId))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = parent.CurrentSelectedId;
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

            TLMUtils.createUIElement(out m_multiplierMode, m_uiHelper.self.transform);
            m_multiplierMode.relativePosition = new Vector3(m_uiHelper.self.width - 200f, m_uiHelper.self.height - 30f);
            m_multiplierMode.textScale = 0.6f;
            m_multiplierMode.width = 40;
            m_multiplierMode.height = 40;
            m_multiplierMode.tooltip = Locale.Get("TLM_USE_RELATIVE_BUDGET");
            TLMUtils.initButton(m_multiplierMode, true, "ButtonMenu");
            m_multiplierMode.name = "RelativeBudget";
            m_multiplierMode.isVisible = true;
            m_multiplierMode.eventClick += (component, eventParam) =>
            {
                TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[parent.CurrentSelectedId];

                IUseAbsoluteVehicleCountExtension bte;
                uint idx;
                if (TLMTransportLineExtension.instance.IsUsingCustomConfig(parent.CurrentSelectedId))
                {

                    bte = TLMTransportLineExtension.instance;
                    idx = parent.CurrentSelectedId;
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

        private void CreateIgnorePrefixBudgetOption()
        {
            if (!parent.PrefixSelectionMode)
            {
                m_IgnorePrefix = m_uiHelper.AddCheckboxLocale("TLM_IGNORE_PREFIX_BUDGETING", false);
                m_IgnorePrefix.relativePosition = new Vector3(5f, 300f);
                m_IgnorePrefix.eventCheckChanged += delegate (UIComponent comp, bool value)
                {
                    if (Singleton<SimulationManager>.exists && parent.CurrentSelectedId != 0)
                    {
                        TLMTransportLineExtension.instance.SetUseCustomConfig(parent.CurrentSelectedId, value);
                        onIgnorePrefixChanged?.Invoke(parent.CurrentSelectedId);
                        updateSliders();
                    }
                };
                m_IgnorePrefix.label.textScale = 0.9f;
            }
        }
        private void CreateBudgetSliders()
        {
            TLMUtils.createUIElement(out m_lineBudgetSlidersTitle, m_uiHelper.self.transform);
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

        #region Budget Methods
        private void setBudgetHour(float x, int selectedHourIndex)
        {
            TransportLine tl = Singleton<TransportManager>.instance.m_lines.m_buffer[parent.CurrentSelectedId];
            ushort val = (ushort)(x * 100 + 0.5f);
            IBudgetableExtension bte;
            uint[] saveData;
            uint idx;
            if (TLMTransportLineExtension.instance.IsUsingCustomConfig(parent.CurrentSelectedId))
            {
                saveData = TLMTransportLineExtension.instance.GetBudgetsMultiplier(parent.CurrentSelectedId);
                bte = TLMTransportLineExtension.instance;
                idx = parent.CurrentSelectedId;
            }
            else
            {
                idx = TLMLineUtils.getPrefix(parent.CurrentSelectedId);
                var tsd = TransportSystemDefinition.from(tl.Info);
                bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                saveData = bte.GetBudgetsMultiplier(TLMLineUtils.getPrefix(parent.CurrentSelectedId));
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
                var lineid = parent.CurrentSelectedId;
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
            TransportSystemDefinition tsd;
            if (parent.PrefixSelectionMode)
            {
                if (TLMSingleton.isIPTLoaded)
                {
                    m_lineBudgetSlidersTitle.parent.isVisible = false;
                    return;
                }

                tsd = parent.TransportSystem;
            }
            else
            {
                if (TLMSingleton.isIPTLoaded)
                {
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
                }

                TransportLine t = TransportManager.instance.m_lines.m_buffer[(int)parent.CurrentSelectedId];
                tsd = TransportSystemDefinition.getDefinitionForLine(parent.CurrentSelectedId);
                if (parent.CurrentSelectedId <= 0 || tsd == default(TransportSystemDefinition))
                {
                    return;
                }
                ushort lineNumber = t.m_lineNumber;
            }

            TLMConfigWarehouse.ConfigIndex transportType = tsd.toConfigIndex();

            uint[] multipliers;
            IBudgetableExtension bte;
            uint idx;

            m_IgnorePrefix.isChecked = TLMTransportLineExtension.instance.IsUsingCustomConfig(parent.CurrentSelectedId);
            if (parent.PrefixSelectionMode)
            {
                idx = parent.CurrentSelectedId;
                bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                multipliers = bte.GetBudgetsMultiplier(idx);

                m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_PREFIX"), idx > 0 ? TLMUtils.getStringFromNumber(TLMUtils.getStringOptionsForPrefix(transportType), (int)idx + 1) : Locale.Get("TLM_UNPREFIXED"), TLMConfigWarehouse.getNameForTransportType(tsd.toConfigIndex()));

            }
            else
            {
                if (m_IgnorePrefix.isChecked)
                {
                    idx = parent.CurrentSelectedId;
                    multipliers = TLMTransportLineExtension.instance.GetBudgetsMultiplier(parent.CurrentSelectedId);
                    bte = TLMTransportLineExtension.instance;
                    m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_LINE"), TLMLineUtils.getLineStringId(parent.CurrentSelectedId), TLMConfigWarehouse.getNameForTransportType(tsd.toConfigIndex()));
                    m_absoluteCountMode.isVisible = !TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(idx);
                    m_multiplierMode.isVisible = TLMTransportLineExtension.instance.IsUsingAbsoluteVehicleCount(idx);
                }
                else
                {
                    idx = TLMLineUtils.getPrefix(parent.CurrentSelectedId);
                    bte = TLMLineUtils.getExtensionFromTransportSystemDefinition(ref tsd);
                    multipliers = bte.GetBudgetsMultiplier(idx);
                    m_absoluteCountMode.isVisible = false;
                    m_multiplierMode.isVisible = false;

                    if (TLMUtils.GetPrefixModoNomenclatura(transportType) != ModoNomenclatura.Nenhum)
                    {
                        m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_PREFIX"), idx > 0 ? TLMUtils.getStringFromNumber(TLMUtils.getStringOptionsForPrefix(transportType), (int)idx + 1) : Locale.Get("TLM_UNPREFIXED"), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
                    }
                    else
                    {
                        m_lineBudgetSlidersTitle.text = string.Format(Locale.Get("TLM_BUDGET_MULTIPLIER_TITLE_LINE"), TLMLineUtils.getLineStringId(parent.CurrentSelectedId), TLMCW.getNameForTransportType(tsd.toConfigIndex()));
                    }
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
            if (!parent.PrefixSelectionMode)
            {
                m_DayLine.isVisible = !budgetPerHourEnabled;
                m_DayNightLine.isVisible = !budgetPerHourEnabled;
                m_NightLine.isVisible = !budgetPerHourEnabled;
                m_DisabledLine.isVisible = !budgetPerHourEnabled;
            }

        }

        #endregion

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
                if (Singleton<SimulationManager>.exists && parent.CurrentSelectedId != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)parent.CurrentSelectedId], true, false);
                        onDayNightChanged?.Invoke();
                    });
                }
            };
            m_NightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                if (Singleton<SimulationManager>.exists && parent.CurrentSelectedId != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)parent.CurrentSelectedId], false, true);
                        onDayNightChanged?.Invoke();
                    });
                }
            };
            m_DayNightLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                if (Singleton<SimulationManager>.exists && parent.CurrentSelectedId != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)parent.CurrentSelectedId], true, true);
                        onDayNightChanged?.Invoke();
                    });
                }
            };
            m_DisabledLine.eventClicked += delegate (UIComponent comp, UIMouseEventParameter c)
            {
                if (Singleton<SimulationManager>.exists && parent.CurrentSelectedId != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[(int)parent.CurrentSelectedId], false, false);
                        onDayNightChanged?.Invoke();
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

        private void OnLoadLine()
        {
            TLMUtils.doLog("SLIDERS OnLoadLine");
            if (parent.PrefixSelectionMode)
            {

            }
            else
            {
                TransportLine t = TransportManager.instance.m_lines.m_buffer[parent.CurrentSelectedId];
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
            }
        }
    }
}
