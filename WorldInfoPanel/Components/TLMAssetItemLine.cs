using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{
    public class TLMAssetItemLine : UICustomControl
    {
        public const string TEMPLATE_NAME = "K45_TLM_AssetSelectionTabLineTemplate";
        private bool m_isLoading;
        private UICheckBox m_checkbox;
        private UITextField m_capacityEditor;
        private string m_currentAsset;
        public Action OnMouseEnter;

        public void Awake()
        {
            m_checkbox = Find<UICheckBox>("AssetCheckbox");
            m_capacityEditor = Find<UITextField>("Cap");
            m_checkbox.eventCheckChanged += (x, y) =>
            {
                if (m_isLoading)
                {
                    return;
                }

                ushort lineId = UVMPublicTransportWorldInfoPanel.GetLineID();
                IBasicExtension extension = TLMLineUtils.GetEffectiveExtensionForLine(lineId);

                LogUtils.DoLog($"checkbox event: {x.objectUserData} => {y} at {extension}[{lineId}]");
                if (y)
                {
                    extension.AddAssetToLine(lineId, m_currentAsset);
                }
                else
                {
                    extension.RemoveAssetFromLine(lineId, m_currentAsset);
                }
            };
            KlyteMonoUtils.LimitWidthAndBox(m_checkbox.label, 280);
            m_capacityEditor.eventTextSubmitted += CapacityEditor_eventTextSubmitted;

            m_checkbox.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
            m_capacityEditor.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();


        }

        public void SetAsset(string assetName, bool isAllowed)
        {
            m_isLoading = true;
            m_currentAsset = assetName;
            m_checkbox.text = Locale.GetUnchecked("VEHICLE_TITLE", assetName);
            m_checkbox.isChecked = isAllowed;
            m_capacityEditor.text = VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(assetName)).ToString("0");
            m_isLoading = false;
        }

        private void CapacityEditor_eventTextSubmitted(UIComponent x, string y)
        {
            if (m_isLoading || !int.TryParse(y.IsNullOrWhiteSpace() ? "0" : y, out int value))
            {
                return;
            }
            var capacityEditor = x as UITextField;
            string assetName = x.parent.GetComponentInChildren<UICheckBox>().objectUserData.ToString();
            VehicleInfo info = PrefabCollection<VehicleInfo>.FindLoaded(assetName);
            TransportSystemDefinition.From(info).GetTransportExtension().SetVehicleCapacity(assetName, value);
            m_isLoading = true;
            capacityEditor.text = VehicleUtils.GetCapacity(info).ToString("0");
            m_isLoading = false;
        }


        public static void EnsureTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(290, 26);
            panel.autoLayout = true;
            panel.wrapLayout = false;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            UICheckBox uiCheckbox = UIHelperExtension.AddCheckbox(panel, "AAAAAA", false);
            uiCheckbox.name = "AssetCheckbox";
            uiCheckbox.height = 29f;
            uiCheckbox.width = 290f;
            uiCheckbox.label.processMarkup = true;
            uiCheckbox.label.textScale = 0.8f;

            KlyteMonoUtils.CreateUIElement(out UITextField capEditField, panel.transform, "Cap", new Vector4(0, 0, 50, 23));
            KlyteMonoUtils.UiTextFieldDefaults(capEditField);
            KlyteMonoUtils.InitButtonFull(capEditField, false, "OptionsDropboxListbox");
            capEditField.isTooltipLocalized = true;
            capEditField.tooltipLocaleID = "K45_TLM_ASSET_CAPACITY_FIELD_DESCRIPTION";
            capEditField.numericalOnly = true;
            capEditField.maxLength = 6;
            capEditField.padding = new RectOffset(2, 2, 4, 2);

            go.AddComponent<TLMAssetItemLine>();
            TLMUiTemplateUtils.GetTemplateDict()[TEMPLATE_NAME] = panel;
        }
    }

}

