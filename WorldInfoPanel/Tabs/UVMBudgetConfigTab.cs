using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    internal class UVMBudgetConfigTab : UICustomControl, IUVMPTWIPChild
    {
        public UIComponent MainContainer { get; private set; }
        internal static UVMBudgetConfigTab Instance { get; private set; }

        private UIHelperExtension m_uiHelperNeighbors;

        private UVMBudgetTimeChart m_clockChart;
        private UIPanel m_titleContainer;
        private List<UVMBudgetEditorLine> m_timeRows = new List<UVMBudgetEditorLine>();

        private UIScrollablePanel m_entryListContainer;

        private UICheckBox m_showAbsoluteCheckbox;

        #region Awake
        public void Awake()
        {
            Instance = this;
            MainContainer = GetComponent<UIComponent>();
            m_uiHelperNeighbors = new UIHelperExtension(MainContainer);

            ((UIPanel) m_uiHelperNeighbors.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel) m_uiHelperNeighbors.Self).wrapLayout = true;
            ((UIPanel) m_uiHelperNeighbors.Self).autoLayout = true;

            UILabel titleLabel = m_uiHelperNeighbors.AddLabel("");
            titleLabel.autoSize = true;
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.minimumSize = new Vector2(MainContainer.width, 0);
            KlyteMonoUtils.LimitWidth(titleLabel, MainContainer.width);
            titleLabel.localeID = "K45_TLM_PER_HOUR_BUDGET_TITLE";

            m_uiHelperNeighbors.AddSpace(5);
            KlyteMonoUtils.CreateElement(out m_clockChart, m_uiHelperNeighbors.Self.transform, "DailyClock");
            m_showAbsoluteCheckbox = m_uiHelperNeighbors.AddCheckboxLocale("K45_TLM_SHOW_ABSOLUTE_VALUE", false, (x) =>
            {
                RebuildList(UVMPublicTransportWorldInfoPanel.GetLineID());
            });
            KlyteMonoUtils.LimitWidthAndBox(m_showAbsoluteCheckbox.label, m_uiHelperNeighbors.Self.width - 40f);
            KlyteMonoUtils.CreateElement(out m_titleContainer, m_uiHelperNeighbors.Self.transform, "Title");
            PopulateTitlePanel(m_titleContainer);
            KlyteMonoUtils.CreateScrollPanel(m_uiHelperNeighbors.Self, out m_entryListContainer, out _, m_uiHelperNeighbors.Self.width - 20f, m_uiHelperNeighbors.Self.height - 150, Vector3.zero);
        }
        #endregion

        private bool m_isDirty = false;

        public void MarkDirty() => m_isDirty = true;

        private void PopulateTitlePanel(UIPanel container)
        {

            container.width = transform.parent.gameObject.GetComponent<UIComponent>().width;
            container.height = 30;
            container.autoLayout = true;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            container.wrapLayout = false;
            container.name = "AzimuthEditorTitle";

            CreateTitleLabel(container, out UILabel cityId, "StartHour", Locale.Get("K45_TLM_START_HOUR"), 50);
            CreateTitleLabel(container, out UILabel generatedName, "GenName", Locale.Get("K45_TLM_BUDGET"), 270);

            KlyteMonoUtils.CreateUIElement(out UIButton add, container.transform, "RegenName");
            add.textScale = 1f;
            add.width = 30;
            add.height = 30;
            add.tooltip = Locale.Get("K45_TLM_ADD_BUDGET_DIVISION");
            KlyteMonoUtils.InitButton(add, true, "ButtonMenu");
            add.isVisible = true;
            add.text = "+";
            add.eventClick += (component, eventParam) => AddEntry();
        }

        private void CreateTitleLabel(UIPanel container, out UILabel label, string name, string text, uint width)
        {

            KlyteMonoUtils.CreateUIElement(out UIPanel nameContainer, container.transform, "GenNameContainer");
            nameContainer.autoSize = false;
            nameContainer.width = width;
            nameContainer.height = 30;
            nameContainer.autoLayout = true;
            nameContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            KlyteMonoUtils.CreateUIElement(out label, nameContainer.transform, name);
            KlyteMonoUtils.LimitWidth(label, width);
            label.autoSize = true;
            label.height = 30;
            label.padding = new RectOffset(3, 3, 4, 3);
            label.textAlignment = UIHorizontalAlignment.Center;
            label.text = text;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.minimumSize = new Vector2(width, 0);
        }


        private void RebuildList(ushort lineID)
        {
            var effectiveConfig = TLMLineUtils.GetEffectiveConfigForLine(lineID);
            TimeableList<BudgetEntryXml> budgetEntries = effectiveConfig.BudgetEntries;
            int stopsCount = budgetEntries.Count;
            if (stopsCount == 0)
            {
                budgetEntries.Add(new BudgetEntryXml()
                {
                    HourOfDay = 0,
                    Value = effectiveConfig is TLMTransportLineConfiguration ? 100u : TransportManager.instance.m_lines.m_buffer[lineID].m_budget
                });
            }

            RecountRows(budgetEntries, stopsCount);
            RedrawList();
        }

        private void RecountRows(TimeableList<BudgetEntryXml> config, int? stopsCount)
        {
            UVMBudgetEditorLine[] currentLines = m_entryListContainer.GetComponentsInChildren<UVMBudgetEditorLine>();
            m_timeRows = new List<UVMBudgetEditorLine>();
            int count = 0;
            for (int i = 0; i < stopsCount; i++, count++)
            {
                if (i < currentLines.Length)
                {
                    currentLines[i].SetLegendInfo(GetColorForNumber(i));
                    m_timeRows.Add(currentLines[i]);
                    currentLines[i].Entry = config[i];
                }
                else
                {
                    UVMBudgetEditorLine line = KlyteMonoUtils.CreateElement<UVMBudgetEditorLine>(m_entryListContainer.transform);
                    line.Entry = config[i];
                    line.SetLegendInfo(GetColorForNumber(i));
                    line.OnTimeChanged += ValidateTime;
                    line.OnDie += RemoveTime;
                    line.OnBudgetChanged += SetBudget;
                    m_timeRows.Add(line);
                }
            }
            for (int i = count; i < currentLines.Length; i++)
            {
                GameObject.Destroy(currentLines[i].gameObject);
            }
        }

        private void RemoveTime(BudgetEntryXml entry)
        {
            TimeableList<BudgetEntryXml> config = TLMLineUtils.GetEffectiveConfigForLine(UVMPublicTransportWorldInfoPanel.GetLineID()).BudgetEntries;
            if (config != default)
            {
                config.RemoveAtHour(entry.HourOfDay ?? -1);
                m_isDirty = true;
            }
        }
        private void ValidateTime(BudgetEntryXml idx, int val)
        {
            idx.HourOfDay = val;
            RedrawList();
        }

        private void SetBudget(BudgetEntryXml idx, float val)
        {
            idx.Value = (uint) val;
            RedrawList();
        }
        private void AddEntry()
        {
            TimeableList<BudgetEntryXml> config = TLMLineUtils.GetEffectiveConfigForLine(UVMPublicTransportWorldInfoPanel.GetLineID()).BudgetEntries;
            config.Add(new BudgetEntryXml()
            {
                HourOfDay = 0,
                Value = 100
            });
            RebuildList(UVMPublicTransportWorldInfoPanel.GetLineID());
        }

        private void RedrawList()
        {
            uint[] values = new uint[m_timeRows.Count];
            bool invalid = false;
            for (int i = 0; i < m_timeRows.Count; i++)
            {
                UVMBudgetEditorLine textField = m_timeRows[i];
                if (!uint.TryParse(textField.GetCurrentVal(), out uint res) || res < 0 || res > 23)
                {
                    textField.SetTextColor(Color.red);
                    values[i] = 1000;
                    invalid = true;
                }
                else
                {
                    textField.SetTextColor(Color.white);
                    values[i] = res;
                }
            }
            if (!invalid)
            {
                var sortedValues = new List<uint>(values);
                sortedValues.Sort();
                var updateInfo = new List<Tuple<int, int, Color, UVMBudgetEditorLine>>();
                for (int i = 0; i < m_timeRows.Count; i++)
                {
                    int zOrder = sortedValues.IndexOf(values[i]);
                    updateInfo.Add(Tuple.New(zOrder, (int) values[i], GetColorForNumber(i), m_timeRows[i]));
                }
                updateInfo.Sort((x, y) => x.First - y.First);
                for (int i = 0; i < updateInfo.Count; i++)
                {
                    float start = updateInfo[i].Second;
                    float end = updateInfo[(i + 1) % updateInfo.Count].Second;
                    if (end < start)
                    {
                        end += 24;
                    }

                    float angle = (start + end) / 2f;
                    updateInfo[i].Fourth.ZOrder = updateInfo[i].First;
                }
                m_clockChart.SetValues(updateInfo.Select(x => Tuple.New(x.Second, x.Third, x.Fourth.Entry.Value)).ToList());
            }
        }

        private Color GetColorForNumber(int num)
        {
            if (num < 0)
            {
                return Color.gray;
            }

            if (num < 10)
            {
                return m_colorOrder[num % 10];
            }

            return Color.Lerp(m_colorOrder[num % 10], Color.black, num / 10 / 5f);
        }

        internal static readonly List<Color> m_colorOrder = new List<Color>()
        {
            Color.red,
            Color.Lerp(Color.red,Color.yellow,0.5f),
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.Lerp(Color.blue,Color.cyan,0.5f),
            Color.blue,
            Color.Lerp(Color.blue,Color.magenta,0.5f),
            Color.magenta,
            Color.Lerp(Color.red,Color.magenta,0.5f),
        };

        public static bool IsAbsoluteValue() => Instance.m_showAbsoluteCheckbox.isVisible && Instance.m_showAbsoluteCheckbox.isChecked;

        public void OnEnable() { }
        public void OnDisable() { }
        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }

            ushort lineID = UVMPublicTransportWorldInfoPanel.GetLineID();
            if (lineID > 0)
            {
                m_showAbsoluteCheckbox.isVisible = TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineID);
                m_showAbsoluteCheckbox.isChecked = TLMTransportLineExtension.Instance.IsDisplayAbsoluteValues(lineID);
                RebuildList(lineID);
            }
        }
        public void UpdateBindings()
        {
            if (m_isDirty)
            {
                RebuildList(UVMPublicTransportWorldInfoPanel.GetLineID());
                m_isDirty = false;
            }
        }
        public void OnGotFocus() { }
    }


}
