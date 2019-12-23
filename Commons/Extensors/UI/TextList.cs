using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.Commons.Extensors
{
    public class TextList<T>
    {
        private readonly UIScrollablePanel m_linesListPanel;
        private readonly UIComponent m_parent;
        private Dictionary<T, string> m_itemsList;
        private T m_selectedItem;
        public string name;
        public T SelectedItem
        {
            get => m_selectedItem;
            internal set {
                m_selectedItem = value;
                RefreshSelection();
            }
        }


        public Dictionary<T, string> ItemsList
        {
            get => m_itemsList;
            set {
                if (value == null)
                {
                    value = new Dictionary<T, string>();
                }
                m_itemsList = value;
                SelectedItem = default;
                RedrawButtons();
            }
        }

        public UIPanel Root => m_linesListPanel.transform.GetComponentInParent<UIPanel>();

        public void Enable()
        {
            m_linesListPanel.enabled = true;
            RedrawButtons();
        }
        public void Disable()
        {
            foreach (Transform t in m_linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }
            m_linesListPanel.enabled = false;
        }

        public KeyValuePair<T, string> PopSelected()
        {
            KeyValuePair<T, string> saida = default;
            if (m_itemsList.ContainsKey(SelectedItem))
            {
                saida = m_itemsList.First(x => x.Key.Equals(SelectedItem));
                m_itemsList.Remove(SelectedItem);
            }
            SelectedItem = default;
            RedrawButtons();
            return saida;
        }

        public void AddItemToList(T id, string name)
        {
            m_itemsList[id] = name;
            RedrawButtons();
        }

        public TextList(UIComponent parent, Dictionary<T, string> initiaItemList, int width, int height, string name)
        {
            this.name = name;
            m_parent = parent;
            ((UIPanel) parent).autoFitChildrenVertically = true;
            ((UIPanel) parent).padding = new RectOffset(20, 20, 20, 20);
            var panelListing = m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
            panelListing.name = "TextList";
            panelListing.height = height;
            panelListing.width = width;
            panelListing.autoLayoutDirection = LayoutDirection.Vertical;
            panelListing.autoLayoutStart = LayoutStart.TopLeft;
            panelListing.autoFitChildrenVertically = true;
            panelListing.wrapLayout = true;
            panelListing.padding = new RectOffset(0, 0, 0, 0);
            panelListing.clipChildren = true;
            panelListing.pivot = UIPivotPoint.MiddleCenter;
            panelListing.relativePosition = Vector2.zero;
            foreach (Transform t in panelListing.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            var scrollObj = new GameObject("Lines Listing Scroll", new Type[] { typeof(UIScrollablePanel) });
            //			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, "SCROLL LOADED");
            m_linesListPanel = scrollObj.GetComponent<UIScrollablePanel>();
            m_linesListPanel.autoLayout = false;
            m_linesListPanel.width = width;
            m_linesListPanel.height = height;
            m_linesListPanel.useTouchMouseScroll = true;
            m_linesListPanel.scrollWheelAmount = 20;
            m_linesListPanel.eventMouseWheel += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                m_linesListPanel.scrollPosition -= new Vector2(0, eventParam.wheelDelta * m_linesListPanel.scrollWheelAmount);
            };
            panelListing.AttachUIComponent(m_linesListPanel.gameObject);
            m_linesListPanel.autoLayout = true;
            m_linesListPanel.autoLayoutDirection = LayoutDirection.Vertical;

            m_linesListPanel.useTouchMouseScroll = true;
            m_linesListPanel.scrollWheelAmount = 20;
            m_linesListPanel.eventMouseWheel += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                m_linesListPanel.scrollPosition -= new Vector2(0, eventParam.wheelDelta * m_linesListPanel.scrollWheelAmount);
                eventParam.Use();
            };

            foreach (Transform t in m_linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            ItemsList = initiaItemList;
        }

        private static void InitButton(UIButton button, string baseSprite)
        {
            var sprite = baseSprite;//"ButtonMenu";
            var spriteHov = baseSprite;
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite;
            button.hoveredBgSprite = spriteHov;
            button.focusedBgSprite = spriteHov;
            button.pressedBgSprite = spriteHov;
            button.textColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = Color.red;
            button.hoveredTextColor = Color.gray;
        }

        public void Redraw() => RedrawButtons();

        private void RedrawButtons()
        {
            foreach (Transform t in m_linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }
            foreach (KeyValuePair<T, string> entry in m_itemsList)
            {
                var itemContainer = new GameObject();

                itemContainer.transform.parent = m_linesListPanel.transform;
                UIButtonWithId itemButton = itemContainer.AddComponent<UIButtonWithId>();

                itemButton.width = m_linesListPanel.width;
                itemButton.height = 35;

                InitButton(itemButton, "EmptySprite");
                itemButton.hoveredColor = Color.gray;
                itemButton.pressedColor = Color.black;
                itemButton.focusedColor = Color.black;
                itemButton.color = new Color(0, 0, 0, 0.7f);
                itemButton.textColor = Color.white;
                itemButton.focusedTextColor = Color.white;
                itemButton.hoveredTextColor = Color.white;
                itemButton.pressedTextColor = Color.white;
                itemButton.outlineColor = Color.black;
                itemButton.useOutline = true;
                itemButton.id = entry.Key;
                itemButton.eventClick += (component, eventParam) =>
                {
                    SelectedItem = (T) itemButton.id;
                    EventOnSelect(SelectedItem);
                    eventParam.Use();
                };
                itemButton.text = entry.Value;
                itemButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
                itemButton.name = string.Format("[{1}] {0}", entry.Value, entry.Key);
            }
        }

        private void RefreshSelection()
        {
            foreach (Transform t in m_linesListPanel.transform)
            {
                UIButtonWithId b = t.GetComponent<UIButtonWithId>();
                if (b.id.Equals(SelectedItem))
                {
                    b.color = new Color(255, 255, 255, 1f);
                    b.textColor = Color.black;
                    b.focusedTextColor = Color.black;
                    b.hoveredTextColor = Color.black;
                    b.pressedTextColor = Color.black;
                    b.hoveredColor = Color.white;
                    b.pressedColor = Color.white;
                    b.focusedColor = Color.white;
                }
                else
                {
                    b.color = new Color(0, 0, 0, 0.7f);
                    b.textColor = Color.white;
                    b.focusedTextColor = Color.white;
                    b.hoveredTextColor = Color.white;
                    b.pressedTextColor = Color.white;
                }
            }
        }


        public void Unselect() => SelectedItem = default;

        public bool Unselected => SelectedItem == null || SelectedItem.Equals(default(T));

        public event OnButtonSelect<T> EventOnSelect;

        public delegate void addItemCallback(int idx, string text);
    }

    internal class UIButtonWithId : UIButton
    {
        public object id;
    }
}

