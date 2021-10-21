using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    public abstract class TLMBaseTimedConfigTab<T, C, L, V> : UICustomControl, IUVMPTWIPChild where T : TLMBaseTimedConfigTab<T, C, L, V> where V : UintValueHourEntryXml<V> where L : TLMBaseSliderEditorLine<L, V> where C : TLMBaseTimeChart<T, C, L, V>
    {
        public abstract string GetTitleLocale();
        public abstract string GetValueColumnLocale();
        public abstract string GetTemplateName();
        public abstract void EnsureTemplate();
        public abstract float GetMaxSliderValue();

        public virtual void ExtraAwake() { }
        public virtual void ExtraOnSetTarget(ushort lineID) { }

        internal abstract List<Color> ColorOrder { get; }

        protected abstract TimeableList<V> Config { get; }
        protected abstract V DefaultEntry();

        public UIComponent MainContainer { get; private set; }
        internal static T Instance { get; private set; }

        protected UIHelperExtension m_uiHelper;

        private C m_clockChart;
        private UIPanel m_titleContainer;
        protected UITemplateList<UIPanel> m_timeRows;

        private UIScrollablePanel m_entryListContainer;


        #region Awake
        public void Awake()
        {
            Instance = (T)this;
            MainContainer = GetComponent<UIComponent>();
            m_uiHelper = new UIHelperExtension(MainContainer);

            ((UIPanel)m_uiHelper.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)m_uiHelper.Self).wrapLayout = true;
            ((UIPanel)m_uiHelper.Self).autoLayout = true;

            UILabel titleLabel = m_uiHelper.AddLabel("");
            titleLabel.autoSize = true;
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.wordWrap = false;
            titleLabel.minimumSize = new Vector2(MainContainer.width - 10, 0); ;
            titleLabel.localeID = GetTitleLocale();

            m_uiHelper.AddSpace(5);
            KlyteMonoUtils.CreateElement(out m_clockChart, m_uiHelper.Self.transform, "DailyClock");
            ExtraAwake();
            KlyteMonoUtils.CreateElement(out m_titleContainer, m_uiHelper.Self.transform, "Title");
            PopulateTitlePanel(m_titleContainer);
            KlyteMonoUtils.CreateScrollPanel(m_uiHelper.Self, out m_entryListContainer, out _, m_uiHelper.Self.width - 20f, m_uiHelper.Self.height - 150, Vector3.zero);
            EnsureTemplate();
            m_timeRows = new UITemplateList<UIPanel>(m_entryListContainer, GetTemplateName());
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
            CreateTitleLabel(container, out UILabel generatedName, "GenName", Locale.Get(GetValueColumnLocale()), 269);

            KlyteMonoUtils.CreateUIElement(out UIButton add, container.transform, "RegenName");
            add.textScale = 1f;
            add.width = 25;
            add.height = 25;
            add.tooltip = Locale.Get("K45_TLM_ADD_ENTRY");
            KlyteMonoUtils.InitButton(add, true, "OptionBase");
            add.isVisible = true;
            add.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            add.normalFgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_Plus);
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
            KlyteMonoUtils.LimitWidthAndBox(label, width);
            label.autoSize = true;
            label.height = 30;
            label.padding = new RectOffset(3, 3, 4, 3);
            label.textAlignment = UIHorizontalAlignment.Center;
            label.text = text;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.minimumSize = new Vector2(width, 0);
        }


        public void RebuildList() => RecountRows();

        private void RecountRows()
        {

            var rulesCount = Config.Count;
            var newRows = m_timeRows.SetItemCount(rulesCount);
            for (int i = 0; i < rulesCount; i++)
            {
                var controller = newRows[i].GetComponent<L>();
                controller.SetSliderParams(GetColorForNumber(i), GetMaxSliderValue());
                controller.Entry = Config[i];
                controller.OnTimeChanged = SetTime;
                controller.OnDie = RemoveTime;
                controller.OnBudgetChanged = SetValue;
            }
            ReorderLines();
        }

        private void ReorderLines()
        {
            var values = Config.Select(x => x.HourOfDay ?? 999).ToArray();
            var sortedValues = new List<int>(values);
            sortedValues.Sort();
            var updateInfo = new List<Tuple<int, int, Color, L>>();
            for (int i = 0; i < m_timeRows.items.Count; i++)
            {
                int zOrder = sortedValues.IndexOf(values[i]);
                updateInfo.Add(Tuple.New(zOrder, values[i], GetColorForNumber(i), m_timeRows.items[i].GetComponent<L>()));
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

        private void RemoveTime(V entry)
        {
            ;
            if (Config != default)
            {
                Config.RemoveAtHour(entry.HourOfDay ?? -1);
                m_isDirty = true;
            }
        }
        private void SetTime(V idx, int val)
        {
            idx.HourOfDay = val;
            ReorderLines();
        }

        private void SetValue(V idx, float val)
        {
            if (idx.Value != (uint)val)
            {
                idx.Value = (uint)val;
                ReorderLines();
            }
        }

        private void AddEntry()
        {
            Config.Add(DefaultEntry());
            RebuildList();
        }


        protected Color GetColorForNumber(int num)
        {
            if (num < 0)
            {
                return Color.gray;
            }

            if (num < 10)
            {
                return ColorOrder[num % 10];
            }

            return Color.Lerp(ColorOrder[num % 10], Color.black, num / 10 / 5f);
        }


        public void OnEnable() { }
        public void OnDisable() { }
        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineID, out ushort buildingId);
            if (lineID > 0 && buildingId == 0)
            {
                ExtraOnSetTarget(lineID);
                RebuildList();
            }
        }
        public void UpdateBindings()
        {
            if (m_isDirty)
            {
                RebuildList();
                m_isDirty = false;
            }
        }
        public void OnGotFocus() { }
        public bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out ushort buildingId) && buildingId == 0 && lineId > 0 && TransportSystemDefinition.FromLineId(lineId, buildingId).HasVehicles();
        public void Hide() => MainContainer.isVisible = false;
    }


}
