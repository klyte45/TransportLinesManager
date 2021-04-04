using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    internal class TLMTicketConfigTab : UICustomControl, IUVMPTWIPChild
    {
        public UIComponent MainContainer { get; private set; }
        internal static TLMTicketConfigTab Instance { get; private set; }

        private UIHelperExtension m_uiHelper;

        private TLMTicketPriceTimeChart m_clockChart;
        private UIPanel m_titleContainer;
        private List<TLMTicketPriceEditorLine> m_timeRows = new List<TLMTicketPriceEditorLine>();

        private UIScrollablePanel m_entryListContainer;


        #region Awake
        public void Awake()
        {
            Instance = this;
            MainContainer = GetComponent<UIComponent>();
            m_uiHelper = new UIHelperExtension(MainContainer);

            ((UIPanel) m_uiHelper.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel) m_uiHelper.Self).wrapLayout = true;
            ((UIPanel) m_uiHelper.Self).autoLayout = true;

            UILabel titleLabel = m_uiHelper.AddLabel("");
            titleLabel.autoSize = true;
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.wordWrap = false;
            titleLabel.minimumSize = new Vector2(MainContainer.width - 10, 0);
            KlyteMonoUtils.LimitWidth(titleLabel, MainContainer.width);
            titleLabel.localeID = "K45_TLM_PER_HOUR_TICKET_PRICE_TITLE";

            m_uiHelper.AddSpace(5);
            KlyteMonoUtils.CreateElement(out m_clockChart, m_uiHelper.Self.transform, "DailyClock");
            m_uiHelper.AddSpace(20);
            KlyteMonoUtils.CreateElement(out m_titleContainer, m_uiHelper.Self.transform, "Title");
            PopulateTitlePanel(m_titleContainer);
            KlyteMonoUtils.CreateScrollPanel(m_uiHelper.Self, out m_entryListContainer, out _, m_uiHelper.Self.width - 20f, m_uiHelper.Self.height - 150, Vector3.zero);
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
            CreateTitleLabel(container, out UILabel generatedName, "GenName", Locale.Get("K45_TLM_TICKET_PRICE"), 270);

            KlyteMonoUtils.CreateUIElement(out UIButton add, container.transform, "RegenName");
            add.textScale = 1f;
            add.width = 30;
            add.height = 30;
            add.tooltip = Locale.Get("K45_TLM_ADD_TICKET_ENTRY");
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


        public void RebuildList(ushort lineID)
        {

            TimeableList<TicketPriceEntryXml> config = TLMLineUtils.GetEffectiveConfigForLine(lineID).TicketPriceEntries;
            int stopsCount = config.Count;
            if (stopsCount == 0)
            {
                config.Add(new TicketPriceEntryXml()
                {
                    HourOfDay = 0,
                    Value = 0
                });
            }

            RecountRows(config, stopsCount, ref TransportManager.instance.m_lines.m_buffer[lineID]);
            RedrawList();
        }

        private void RecountRows(TimeableList<TicketPriceEntryXml> config, int? stopsCount, ref TransportLine tl)
        {
            TLMTicketPriceEditorLine[] currentLines = m_entryListContainer.GetComponentsInChildren<TLMTicketPriceEditorLine>();
            m_timeRows = new List<TLMTicketPriceEditorLine>();
            var tsd = TransportSystemDefinition.GetDefinitionForLine(ref tl);
            uint maxTicketPrice = TLMLineUtils.GetTicketPriceForLine(ref tsd, 0).First.Value * 5;
            int count = 0;
            for (int i = 0; i < stopsCount; i++, count++)
            {
                if (i < currentLines.Length)
                {
                    currentLines[i].SetLegendInfo(GetColorForNumber(i), maxTicketPrice);
                    m_timeRows.Add(currentLines[i]);
                    currentLines[i].Entry = config[i];
                }
                else
                {
                    TLMTicketPriceEditorLine line = KlyteMonoUtils.CreateElement<TLMTicketPriceEditorLine>(m_entryListContainer.transform);
                    line.Entry = config[i];
                    line.SetLegendInfo(GetColorForNumber(i), maxTicketPrice);
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

        private void RemoveTime(TicketPriceEntryXml entry)
        {
            TimeableList<TicketPriceEntryXml> config = TLMLineUtils.GetEffectiveConfigForLine(UVMPublicTransportWorldInfoPanel.GetLineID()).TicketPriceEntries;
            if (config != default)
            {
                config.RemoveAtHour(entry.HourOfDay ?? -1);
                m_isDirty = true;
            }
        }
        private void ValidateTime(TicketPriceEntryXml idx, int val)
        {
            idx.HourOfDay = val;
            RedrawList();
        }

        private void SetBudget(TicketPriceEntryXml idx, float val)
        {
            idx.Value = (uint) val;
            RedrawList();
        }
        private void AddEntry()
        {
            ushort lineId = UVMPublicTransportWorldInfoPanel.GetLineID();
            Interfaces.IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
            config.SetTicketPriceToLine(lineId, 0, 0);
            RebuildList(UVMPublicTransportWorldInfoPanel.GetLineID());
        }

        private void RedrawList()
        {
            uint[] values = new uint[m_timeRows.Count];
            bool invalid = false;
            for (int i = 0; i < m_timeRows.Count; i++)
            {
                TLMTicketPriceEditorLine textField = m_timeRows[i];
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
                var updateInfo = new List<Tuple<int, int, Color, TLMTicketPriceEditorLine>>();
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
            Color.Lerp(Color.red,Color.magenta,0.5f),
            Color.magenta,
            Color.Lerp(Color.blue,Color.magenta,0.5f),
            Color.blue,
            Color.Lerp(Color.blue,Color.cyan,0.5f),
            Color.cyan,
            Color.green,
            Color.yellow,
            Color.Lerp(Color.red,Color.yellow,0.5f),
            Color.red,
        };


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
        public bool MayBeVisible() => TransportSystemDefinition.From(UVMPublicTransportWorldInfoPanel.GetLineID()).HasVehicles();
        public void Hide() => MainContainer.isVisible = false;
    }


}
