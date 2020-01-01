using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    public class UVMBudgetWIPTab : UICustomControl, IUVMPTWIPChild
    {

        #region Overridable

        public void Awake()
        {
            m_bg = component as UIPanel;
            m_bg.autoLayout = false;

            PublicTransportWorldInfoPanel ptwip = UVMPublicTransportWorldInfoPanel.m_obj.origInstance;

            BindComponents(ptwip);
            SetVehicleCounterEvents();
        }

        private void BindComponents(PublicTransportWorldInfoPanel __instance)
        {
            m_vehicleCountModifier = __instance.Find<UISlider>("SliderModifyVehicleCount");
            m_vehicleCountModifierLabel = __instance.Find<UILabel>("VehicleCountPercent");
            m_vehicleAmount = __instance.Find<UILabel>("VehicleAmount");
            m_vehicleCountPanel = RebindUI(__instance.Find<UIPanel>("PanelVehicleCount"));


            //TAB3
            m_ticketPriceSlider = __instance.Find<UISlider>("SliderTicketPrice");
            m_ticketPriceSlider.minValue = 0;
            m_ticketPriceSlider.eventValueChanged += OnTicketPriceChanged;
            m_ticketPriceLabel = __instance.Find<UILabel>("LabelTicketPrice");
            m_ticketPriceSection = RebindUI(__instance.Find<UIPanel>("TicketPriceSection"));
        }
        private T RebindUI<T>(T component) where T : UIComponent
        {
            Vector3 relPos = component.relativePosition;
            component.transform.SetParent(this.component.transform);
            component.relativePosition = relPos;
            return component;
        }
        private void OnTicketPriceChanged(UIComponent component, float value)
        {
            Singleton<TransportManager>.instance.m_lines.m_buffer[GetLineID()].m_ticketPrice = (ushort) value;
            float num = Mathf.RoundToInt(value) / 100f;
            m_ticketPriceLabel.text = num.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
        }

        private void SetVehicleCounterEvents() => m_vehicleCountModifier.eventValueChanged += OnVehicleCountModifierChanged;

        public void OnEnable()
        {
        }

        public void OnDisable()
        { }

        public void UpdateBindings()
        {
            if (m_bg.isVisible)
            {
                if (!m_alreadyShown)
                {
                    m_vehicleCountPanel.relativePosition = new Vector3(0, 10);
                    m_ticketPriceSection.relativePosition = new Vector3(0, 90);
                    m_alreadyShown = true;
                }
                ushort lineID = GetLineID();
                if (lineID != 0)
                {
                    int num = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountVehicles(lineID);
                    int num2 = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CalculateTargetVehicleCount();
                    string text = (num2 <= 0) ? num.ToString() : (num.ToString() + " / " + num2.ToString());
                    m_vehicleAmount.text = LocaleFormatter.FormatGeneric("TRANSPORT_LINE_VEHICLECOUNT", new object[]
                        {
                text
                        });
                }
            }
        }

        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }

            ushort lineID = GetLineID();
            if (lineID != 0)
            {
                m_vehicleCountModifier.value = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget;
                m_ticketPriceSlider.value = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_ticketPrice;
            }

        }

        #endregion


        public void OnGotFocus()
        {
        }


        private void OnVehicleCountModifierChanged(UIComponent component, float value)
        {
            ushort lineID = GetLineID();
            Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget = (ushort) value;
            m_vehicleCountModifierLabel.text = LocaleFormatter.FormatPercentage(Mathf.RoundToInt(value));
        }

        internal static ushort GetLineID() => UVMPublicTransportWorldInfoPanel.GetLineID();

        public static string GetVehicleTypeIcon(ushort lineId) => TransportSystemDefinition.From(lineId).GetCircleSpriteName().ToString();


        private static UIPanel m_bg;

        private UISlider m_vehicleCountModifier;
        private UILabel m_vehicleCountModifierLabel;
        private UILabel m_vehicleAmount;
        private UIPanel m_vehicleCountPanel;

        private UIPanel m_ticketPriceSection;
        private UISlider m_ticketPriceSlider;
        private UILabel m_ticketPriceLabel;

        private bool m_alreadyShown = false;

    }
}