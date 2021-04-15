using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Xml;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMTicketPriceEditorLine : MonoBehaviour
    {
        private UIPanel m_container;
        private UITextField m_timeInput;
        private UILabel m_value;
        private UISlider m_slider;
        private UIButton m_die;
        private UISlider m_bugdetSlider;

        private bool m_loading = false;

        public int ZOrder
        {
            get => m_container.zOrder;
            set => m_container.zOrder = value;
        }

        public TicketPriceEntryXml Entry
        {
            get => m_entry; set
            {
                m_entry = value;
                FillData();
            }
        }
        public event Action<TicketPriceEntryXml> OnDie;
        public event Action<TicketPriceEntryXml, int> OnTimeChanged;
        public event Action<TicketPriceEntryXml, float> OnBudgetChanged;
        private TicketPriceEntryXml m_entry;

        public void SetLegendInfo(Color c, uint maxValue)
        {
            m_timeInput.eventTextChanged -= SendText;
            ((UISprite)m_slider.thumbObject).color = c;
            m_bugdetSlider.maxValue = maxValue;
            m_timeInput.eventTextChanged += SendText;
        }

        private void FillData()
        {
            m_loading = true;
            try
            {
                ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[UVMPublicTransportWorldInfoPanel.GetLineID()];
                m_timeInput.text = Entry.HourOfDay.ToString();
                m_slider.value = Entry.Value;
                m_value.text = Entry.Value == 0 ? Locale.Get("K45_TLM_DEFAULT_TICKET_VALUE") : (Entry.Value / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            }
            finally
            {
                m_loading = false;
            }
        }

        public string GetCurrentVal() => m_timeInput.text;

        public void SetTextColor(Color c) => m_timeInput.color = c;

        public void Awake()
        {
            m_container = transform.gameObject.AddComponent<UIPanel>();
            m_container.width = transform.parent.gameObject.GetComponent<UIComponent>().width;
            m_container.height = 30;
            m_container.autoLayout = true;
            m_container.autoLayoutDirection = LayoutDirection.Horizontal;
            m_container.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            m_container.wrapLayout = false;
            m_container.name = "TicketPriceEntryLine";

            KlyteMonoUtils.CreateUIElement(out m_timeInput, m_container.transform, "HourInput");
            KlyteMonoUtils.UiTextFieldDefaults(m_timeInput);
            m_timeInput.normalBgSprite = "OptionsDropboxListbox";
            m_timeInput.width = 50;
            m_timeInput.height = 28;
            m_timeInput.textScale = 1.6f;
            m_timeInput.maxLength = 2;
            m_timeInput.numericalOnly = true;
            m_timeInput.text = "0";
            m_timeInput.eventTextChanged += SendText;

            KlyteMonoUtils.CreateUIElement(out m_value, m_container.transform, "TicketPriceLabel");
            m_value.autoSize = false;
            m_value.width = 60;
            m_value.height = 30;
            m_value.textScale = 1.125f;
            m_value.textAlignment = UIHorizontalAlignment.Center;
            m_value.padding = new RectOffset(3, 3, 5, 3);
            KlyteMonoUtils.LimitWidthAndBox(m_value, 60);

            m_slider = GenerateTicketPriceField(m_container);

            KlyteMonoUtils.CreateUIElement(out m_die, m_container.transform, "Delete");
            m_die.textScale = 1f;
            m_die.width = 30;
            m_die.height = 30;
            m_die.tooltip = Locale.Get("K45_TLM_DELETE_STOP_TICKET_PRICE_LIST");
            KlyteMonoUtils.InitButton(m_die, true, "ButtonMenu");
            m_die.isVisible = true;
            m_die.text = "X";
            m_die.eventClick += (component, eventParam) => OnDie?.Invoke(Entry);

        }

        private UISlider GenerateTicketPriceField(UIComponent container)
        {
            m_bugdetSlider = UIHelperExtension.AddSlider(container, null, 0, 500, 5, -1,
               (x) =>
               {

               });
            Destroy(m_bugdetSlider.transform.parent.GetComponentInChildren<UILabel>());
            UIPanel budgetSliderPanel = m_bugdetSlider.GetComponentInParent<UIPanel>();

            budgetSliderPanel.width = 205;
            budgetSliderPanel.height = 20;
            budgetSliderPanel.autoLayout = true;

            m_bugdetSlider.size = new Vector2(200, 20);
            m_bugdetSlider.scrollWheelAmount = 0;
            m_bugdetSlider.clipChildren = true;
            m_bugdetSlider.thumbOffset = new Vector2(-200, 0);
            m_bugdetSlider.color = new Color32(128, 128, 128, 128);

            m_bugdetSlider.thumbObject.width = 400;
            m_bugdetSlider.thumbObject.height = 20;
            ((UISprite)m_bugdetSlider.thumbObject).spriteName = "PlainWhite";
            ((UISprite)m_bugdetSlider.thumbObject).color = new Color32(1, 140, 46, 255);

            m_bugdetSlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                SetBudgetHour(val);
            };

            return m_bugdetSlider;
        }

        private void SetBudgetHour(float val)
        {
            if (m_loading)
            {
                return;
            }

            OnBudgetChanged?.Invoke(Entry, val);
            FillData();
        }

        private bool alreadyCalling = false;
        private void SendText(UIComponent x, string y)
        {
            if (m_loading)
            {
                return;
            }
            if (alreadyCalling)
            {
                LogUtils.DoErrorLog($"TLMTicketPriceEditorLine: Recursive eventTextChanged call! {Environment.StackTrace}");
            }
            alreadyCalling = true;
            try
            {
                if (int.TryParse(y, out int time))
                {
                    OnTimeChanged?.Invoke(Entry, time);
                }
            }
            finally
            {
                alreadyCalling = false;
            }

        }
    }

}

