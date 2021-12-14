using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMTableRowDataOutsideConnection : UICustomControl
    {

        public const string ITEM_TEMPLATE = "K45_TLM_TLMTableRowDataOutsideConnection";

        public static void EnsureTemplate()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(ITEM_TEMPLATE))
            {
                return;
            }
            var go = new GameObject();
            var bg = go.AddComponent<UIPanel>();
            bg.autoSize = false;
            bg.width = 88;
            bg.height = 18;
            bg.pivot = UIPivotPoint.MiddleLeft;
            bg.name = "Content";
            bg.relativePosition = new Vector3(0f, 0f);

            var check = UIHelperExtension.AddCheckbox(bg, "X", false);
            check.name = "Checkbox";
            Destroy(check.label);
            check.width = 18;
            check.height = 18;
            check.relativePosition = new Vector3(35, 0);

            go.AddComponent<TLMTableRowDataOutsideConnection>();

            UITemplateUtils.GetTemplateDict()[ITEM_TEMPLATE] = bg;
        }

        private UIPanel m_content;
        private UICheckBox m_check;
        private Action<bool> m_onChangeCheck;

        public void Awake()
        {
            m_content = GetComponent<UIPanel>();
            m_check = Find<UICheckBox>("Checkbox");
            m_check.eventCheckChanged += (x, y) => m_onChangeCheck?.Invoke(y);
        }

        public void ResetData(bool currentValue, Action<bool> onChangeCheck)
        {
            m_onChangeCheck = null;
            m_check.isChecked = currentValue;
            m_onChangeCheck = onChangeCheck;
        }
    }
}