using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    public abstract class TLMBaseTimeChart<T, C, L, V> : MonoBehaviour
        where T : TLMBaseTimedConfigTab<T, C, L, V>
        where V : UintValueHourEntryXml<V>
        where L : TLMBaseSliderEditorLine<L, V>
        where C : TLMBaseTimeChart<T, C, L, V>
    {


        public abstract string ClockTooltip { get; }

        public abstract void OnUpdate();


        public abstract void CreateLabels();



        public abstract void OnDeleteTarget();

        public abstract void SetPasteTarget(TimeableList<V> newVal);

        public abstract TimeableList<V> GetCopyTarget();






        protected UIPanel m_container;
        private UIRadialChartExtended m_clock;
        private UISprite m_hourPointer;
        private UISprite m_minutePointer;

        private UIButton m_copyButton;
        private UIButton m_pasteButton;
        private UIButton m_eraseButton;

        private string m_clipboard;


        private void AwakeActionButtons()
        {
            m_copyButton = ConfigureActionButton(m_container, CommonsSpriteNames.K45_Copy);
            m_copyButton.eventClick += (x, y) => ActionCopy();
            m_pasteButton = ConfigureActionButton(m_container, CommonsSpriteNames.K45_Paste);
            m_pasteButton.eventClick += (x, y) => ActionPaste();
            m_pasteButton.isVisible = false;
            m_eraseButton = ConfigureActionButton(m_container, CommonsSpriteNames.K45_Delete);
            m_eraseButton.eventClick += (x, y) => ActionDelete();
            m_eraseButton.color = Color.red;

            m_copyButton.tooltip = Locale.Get("K45_TLM_COPY_CURRENT_LIST_CLIPBOARD");
            m_pasteButton.tooltip = Locale.Get("K45_TLM_PASTE_CLIPBOARD_TO_CURRENT_LIST");
            m_eraseButton.tooltip = Locale.Get("K45_TLM_DELETE_CURRENT_LIST");

            m_copyButton.relativePosition = new Vector3(-50, 0);
            m_pasteButton.relativePosition = new Vector3(-50, 25);
            m_eraseButton.relativePosition = new Vector3(m_container.width + 30, 0);
        }

        private void ActionCopy()
        {
            m_clipboard = XmlUtils.DefaultXmlSerialize(GetCopyTarget());
            m_pasteButton.isVisible = true;
            TLMBaseTimedConfigTab<T, C, L, V>.Instance.RebuildList();
        }
        private void ActionPaste()
        {
            if (m_clipboard == null)
            {
                return;
            }
            SetPasteTarget(XmlUtils.DefaultXmlDeserialize<TimeableList<V>>(m_clipboard));
            TLMBaseTimedConfigTab<T, C, L, V>.Instance.RebuildList();
        }
        private void ActionDelete()
        {
            OnDeleteTarget();
            TLMBaseTimedConfigTab<T, C, L, V>.Instance.RebuildList();
        }
        protected static UIButton ConfigureActionButton(UIComponent parent, CommonsSpriteNames spriteName)
        {
            KlyteMonoUtils.CreateUIElement(out UIButton actionButton, parent.transform, "Btn");
            KlyteMonoUtils.InitButton(actionButton, false, "ButtonMenu");
            actionButton.focusedBgSprite = "";
            actionButton.autoSize = false;
            actionButton.width = 20;
            actionButton.height = 20;
            actionButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            actionButton.normalFgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(spriteName);
            return actionButton;
        }
        public void SetValues(List<Tuple<int, Color, uint>> steps)
        {
            if (steps.Count == 0)
            {
                steps = new List<Tuple<int, Color, uint>>
                {
                    Tuple.New(0,Color.gray,1u)
                };
            }

            steps.Sort((x, y) => x.First - y.First);
            if (steps[0].First != 0)
            {
                steps.Insert(0, Tuple.New(0, steps.Last().Second, steps.Last().Third));
            }
            if (steps.Count != m_clock.sliceCount)
            {
                while (m_clock.sliceCount > 0)
                {
                    m_clock.RemoveSlice(0);
                }
                foreach (Tuple<int, Color, uint> loc in steps)
                {
                    var color = loc.Third == 0 ? Color.gray : loc.Second;
                    m_clock.AddSlice(color, color, 1);// loc.Third / dividerMultiplier);
                }
            }
            else
            {
                for (int i = 0; i < m_clock.sliceCount; i++)
                {
                    var color = steps[i].Third == 0 ? Color.gray : steps[i].Second;
                    m_clock.GetSlice(i).innerColor = color;
                    m_clock.GetSlice(i).outterColor = color;
                }
            }

            var targetValues = steps.Select(x => Mathf.Round(x.First * 100f / 24f)).ToList();
            m_clock.SetValuesStarts(targetValues.ToArray());
        }

        public void Awake()
        {
            LogUtils.DoLog("AWAKE UVMBudgetTimeChart!");
            UIPanel panel = transform.gameObject.AddComponent<UIPanel>();
            panel.width = 370;
            panel.height = 70;
            panel.autoLayout = false;
            panel.useCenter = true;
            panel.wrapLayout = false;
            panel.tooltipLocaleID = ClockTooltip;

            KlyteMonoUtils.CreateUIElement(out m_container, transform, "ClockContainer");
            m_container.relativePosition = new Vector3((panel.width / 2f) - 70, 0);
            m_container.width = 140;
            m_container.height = 70;
            m_container.autoLayout = false;
            m_container.useCenter = true;
            m_container.wrapLayout = false;
            m_container.tooltipLocaleID = ClockTooltip;

            KlyteMonoUtils.CreateUIElement(out m_clock, m_container.transform, "Clock");
            m_clock.spriteName = "K45_24hClock";
            m_clock.relativePosition = new Vector3(0, 0);
            m_clock.width = 70;
            m_clock.height = 70;

            KlyteMonoUtils.CreateUIElement(out m_minutePointer, m_container.transform, "Minute");
            m_minutePointer.width = 2;
            m_minutePointer.height = 27;
            m_minutePointer.pivot = UIPivotPoint.TopCenter;
            m_minutePointer.relativePosition = new Vector3(35, 35);
            m_minutePointer.spriteName = "EmptySprite";
            m_minutePointer.color = new Color32(35, 35, 35, 255);

            KlyteMonoUtils.CreateUIElement(out m_hourPointer, m_container.transform, "Hour");
            m_hourPointer.width = 3;
            m_hourPointer.height = 14;
            m_hourPointer.pivot = UIPivotPoint.TopCenter;
            m_hourPointer.relativePosition = new Vector3(35, 35);
            m_hourPointer.spriteName = "EmptySprite";
            m_hourPointer.color = new Color32(5, 5, 5, 255);

            CreateLabels();

            AwakeActionButtons();
        }

        public void Update()
        {
            if (m_container.isVisible)
            {
                m_minutePointer.transform.localEulerAngles = new Vector3(0, 0, (TLMLineUtils.ReferenceTimer % 1 * -360) + 180);
                m_hourPointer.transform.localEulerAngles = new Vector3(0, 0, (TLMLineUtils.ReferenceTimer / 24 * -360) + 180);
                OnUpdate();
            }
        }
    }

}

