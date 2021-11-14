using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;

namespace Klyte.TransportLinesManager.UI
{
    public class UVMBudgetEditorLine : TLMBaseSliderEditorLine<UVMBudgetEditorLine, BudgetEntryXml>
    {
        public const string BUDGET_LINE_TEMPLATE = "TLM_BudgetLineTemplate";
        protected override void ExtraOnFillData(ref TransportLine t)
        {

            float stepsize = UVMBudgetConfigTab.IsAbsoluteValue() ? TLMLineUtils.CalculateBudgetForEachVehicle(t.Info, t.m_totalLength) * 100f : 5f;
            m_valueSlider.maxValue = UVMBudgetConfigTab.IsAbsoluteValue() ? stepsize * 50 : 500f;
            m_valueSlider.value = Entry.Value;
            m_valueSlider.stepSize = stepsize;
        }

        public static void EnsureTemplate() => EnsureTemplate(BUDGET_LINE_TEMPLATE);
        public override string GetValueFormat(ref TransportLine t)
        {
            string text = $"{(UVMBudgetConfigTab.IsAbsoluteValue() ? TLMLineUtils.ProjectTargetVehicleCount(t.Info, t.m_totalLength, Entry.Value / 100f) : (int)Entry.Value)}";
            return UVMBudgetConfigTab.IsAbsoluteValue() ? $"<sprite IconPolicyFreePublicTransport>x{text}" : text + "%";

        }

        public override uint GetValueAsInt(ref TransportLine t) => UVMBudgetConfigTab.IsAbsoluteValue() ? (uint)TLMLineUtils.ProjectTargetVehicleCount(t.Info, t.m_totalLength, Entry.Value / 100f) : Entry.Value;
        public override void SetValueFromTyping(ref TransportLine t, uint value)
        {
            if (UVMBudgetConfigTab.IsAbsoluteValue())
            {
                SetValue(value * TLMLineUtils.CalculateBudgetForEachVehicle(t.Info, t.m_totalLength) * 100);
            }
            else
            {
                SetValue(value);
            }
        }
    }

}

