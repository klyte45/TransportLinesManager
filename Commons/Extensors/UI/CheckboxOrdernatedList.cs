using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Commons.Extensors
{
    public class CheckboxOrdernatedList<T> : UICustomControl where T : class, ICheckable
    {
        private UIPanel m_parent;

        public void Awake()
        {
            m_parent = gameObject.AddComponent<UIPanel>();

            m_parent.autoFitChildrenVertically = true;
            m_parent.autoLayoutPadding = new RectOffset(5, 5, 0, 0);
            m_parent.autoFitChildrenVertically = true;
            m_parent.clipChildren = true;
            m_parent.autoFitChildrenHorizontally = true;
            m_parent.autoFitChildrenHorizontally = true;
            m_parent.backgroundSprite = "OptionsDropboxListboxHovered";

            m_parent.autoLayout = true;
            m_parent.autoLayoutDirection = LayoutDirection.Vertical;


        }
        private bool m_isLoading = false;
        public void SetData(List<T> items)
        {
            m_isLoading = true;
            for (var i = 0; i < m_parent.components.Count && i < items.Count; i++)
            {
                ((UICheckBox) m_parent.components[i]).isChecked = items[i].IsChecked;
                ((UICheckBox) m_parent.components[i]).label.text = items[i].ToString();
                ((UICheckBox) m_parent.components[i]).objectUserData = new IdAndWeight
                {
                    id = items[i],
                    weight = i
                };
            }
            while (m_parent.components.Count > items.Count)
            {
                Destroy(m_parent.components[m_parent.components.Count - 1]);
                m_parent.components.RemoveAt(m_parent.components.Count - 1);
            }
            while (m_parent.components.Count < items.Count)
            {
                var i = m_parent.components.Count;
                UICheckBox check = AddCheckboxLocale(items[i].ToString(), items[i].IsChecked, new IdAndWeight
                {
                    id = items[i],
                    weight = i
                }, (x, y) =>
                {
                    (x.objectUserData as IdAndWeight).weight = m_parent.components.Where(x => (x as UICheckBox).isChecked).Count() - 1;
                    Reordenate();
                    EventOnValueChanged?.Invoke(GetSelectionOrder());
                });
                check.label.colorizeSprites = true;
                check.label.processMarkup = true;
            }
            m_isLoading = false;
            Reordenate();
        }


        public event Action<List<T>> EventOnValueChanged;

        private class IdAndWeight
        {
            public T id;
            public int weight;
        }

        private UICheckBox AddCheckboxLocale(string text, bool defaultValue, IdAndWeight idAndWeight, Action<UICheckBox, bool> eventCallback = null)
        {
            var uICheckBox = m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kCheckBoxTemplate)) as UICheckBox;
            uICheckBox.isChecked = defaultValue;
            uICheckBox.label.text = text;
            if (eventCallback != null)
            {
                uICheckBox.eventCheckChanged += delegate (UIComponent c, bool isChecked)
                {
                    eventCallback(c as UICheckBox, isChecked);
                };
            }
            uICheckBox.objectUserData = idAndWeight;
            return uICheckBox;

        }

        private void Reordenate()
        {
            if (m_isLoading)
            {
                return;
            }

            foreach (UICheckBox item in m_parent.components)
            {
                item.zOrder = (!item.isChecked ? 0 : 9999) + ((IdAndWeight) item.objectUserData).weight;
            }

            m_parent.components.OrderBy(x => (((UICheckBox) x).isChecked ? 0 : 9999) + ((IdAndWeight) ((UICheckBox) x).objectUserData).weight).ForEach(x => x.zOrder = 999999);
        }

        public List<T> GetSelectionOrder() => m_parent.components
            .Where(x => x is UICheckBox check && check.isChecked)
            .OrderBy(x => ((x as UICheckBox).objectUserData as IdAndWeight).weight)
            .Select(x => ((x as UICheckBox).objectUserData as IdAndWeight).id)
            .ToList();

    }

    public interface ICheckable
    {
        public bool IsChecked { get; set; }
    }
}

