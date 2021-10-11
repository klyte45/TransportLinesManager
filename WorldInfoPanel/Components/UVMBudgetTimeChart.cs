using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Overrides;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    public class UVMBudgetTimeChart : TLMBaseTimeChart<UVMBudgetConfigTab, UVMBudgetTimeChart, UVMBudgetEditorLine, BudgetEntryXml>
    {

        private UILabel m_effectiveLabel;
        private UIProgressBar m_effectiveSprite;

        public override TimeableList<BudgetEntryXml> GetCopyTarget()
        {
            ushort lineID = UVMPublicTransportWorldInfoPanel.GetLineID();
            return TLMLineUtils.GetEffectiveExtensionForLine(lineID).GetBudgetsMultiplierForLine(lineID);
        }
        public override void SetPasteTarget(TimeableList<BudgetEntryXml> newVal)
        {
            ushort lineID = UVMPublicTransportWorldInfoPanel.GetLineID();
            TLMLineUtils.GetEffectiveExtensionForLine(lineID).SetAllBudgetMultipliersForLine(lineID, newVal);
        }
        public override void OnDeleteTarget()
        {
            ushort lineID = UVMPublicTransportWorldInfoPanel.GetLineID();
            TLMLineUtils.GetEffectiveExtensionForLine(lineID).RemoveAllBudgetMultipliersOfLine(lineID);
        }

        public override void CreateLabels()
        {
            KlyteMonoUtils.CreateUIElement(out UILabel titleEffective, m_container.transform, "TitleEffective");
            titleEffective.width = 70;
            titleEffective.height = 30;
            KlyteMonoUtils.LimitWidthAndBox(titleEffective, 70, out UIPanel container, true);
            container.relativePosition = new Vector3(70, 0);
            titleEffective.textScale = 0.8f;
            titleEffective.color = Color.white;
            titleEffective.isLocalized = true;
            titleEffective.localeID = "K45_TLM_EFFECTIVE_BUDGET_NOW";
            titleEffective.textAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out UIPanel effectiveContainer, m_container.transform, "ValueEffectiveContainer");
            effectiveContainer.width = 70;
            effectiveContainer.height = 40;
            effectiveContainer.relativePosition = new Vector3(70, 30);
            effectiveContainer.color = Color.white;
            effectiveContainer.autoLayout = false;

            KlyteMonoUtils.CreateUIElement(out m_effectiveSprite, effectiveContainer.transform, "BarBg");
            m_effectiveSprite.width = 70;
            m_effectiveSprite.height = 40;
            m_effectiveSprite.relativePosition = new Vector3(0, 0);
            m_effectiveSprite.backgroundSprite = "PlainWhite";
            m_effectiveSprite.progressSprite = "PlainWhite";
            m_effectiveSprite.color = Color.cyan;
            m_effectiveSprite.progressColor = Color.red;
            m_effectiveSprite.value = 0.5f;

            KlyteMonoUtils.CreateUIElement(out m_effectiveLabel, effectiveContainer.transform, "BarLabel");
            m_effectiveLabel.width = 70;
            m_effectiveLabel.height = 40;
            m_effectiveLabel.relativePosition = new Vector3(0, 0);
            m_effectiveLabel.color = Color.white;
            m_effectiveLabel.isLocalized = false;
            m_effectiveLabel.text = "%\n";
            m_effectiveLabel.textAlignment = UIHorizontalAlignment.Center;
            m_effectiveLabel.verticalAlignment = UIVerticalAlignment.Middle;
            m_effectiveLabel.useOutline = true;
            m_effectiveLabel.padding.top = 3;
            KlyteMonoUtils.LimitWidthAndBox(m_effectiveLabel, 70, true);
        }

        public override void OnUpdate()
        {
            ushort lineID = UVMPublicTransportWorldInfoPanel.GetLineID();
            Tuple<float, int, int, float> value = TLMLineUtils.GetBudgetMultiplierLineWithIndexes(lineID);
            m_effectiveSprite.color = UVMBudgetConfigTab.Instance.ColorOrder[value.Second % UVMBudgetConfigTab.Instance.ColorOrder.Count];
            m_effectiveSprite.progressColor = UVMBudgetConfigTab.Instance.ColorOrder[value.Third % UVMBudgetConfigTab.Instance.ColorOrder.Count];
            m_effectiveSprite.value = value.Fourth;
            int currentVehicleCount = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].CountVehicles(lineID);
            int targetVehicleCount = TransportLineOverrides.NewCalculateTargetVehicleCount(lineID);
            m_effectiveLabel.prefix = (value.First * 100).ToString("0");
            m_effectiveLabel.suffix = $"{currentVehicleCount.ToString("0")}/{targetVehicleCount.ToString("0")}";
        }


        public override string ClockTooltip { get; } = "K45_TLM_BUDGET_CLOCK";




    }

}

