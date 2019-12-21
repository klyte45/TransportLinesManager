using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ICities;
using ColossalFramework.UI;
using ColossalFramework;
using ColossalFramework.Plugins;
using System.Threading;
using System;
using System.Linq;

namespace Klyte.Commons.Extensors
{
    public class TextList<T>
    {
        private UIScrollablePanel linesListPanel;
        private UIComponent m_parent;
        private Dictionary<T, string> m_itemsList;
        private T _selectedItem;
        public string name;
        public T selectedItem
        {
            get {
                return _selectedItem;
            }
            internal set {
                _selectedItem = value;
                refreshSelection();
            }
        }


        public Dictionary<T, string> itemsList
        {
            get {
                return m_itemsList;
            }
            set {
                if (value == null)
                {
                    value = new Dictionary<T, string>();
                }
                m_itemsList = value;
                selectedItem = default(T);
                redrawButtons();
            }
        }

        public UIPanel root
        {
            get {
                return linesListPanel.transform.GetComponentInParent<UIPanel>();
            }
        }

        public void Enable()
        {
            linesListPanel.enabled = true;
            redrawButtons();
        }
        public void Disable()
        {
            foreach (Transform t in linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }
            linesListPanel.enabled = false;
        }

        public KeyValuePair<T, string> popSelected()
        {
            KeyValuePair<T, string> saida = default(KeyValuePair<T, string>);
            if (m_itemsList.ContainsKey(selectedItem))
            {
                saida = m_itemsList.First(x => x.Key.Equals(selectedItem));
                m_itemsList.Remove(selectedItem);
            }
            selectedItem = default(T);
            redrawButtons();
            return saida;
        }

        public void addItemToList(T id, string name)
        {
            m_itemsList[id] = name;
            redrawButtons();
        }

        public TextList(UIComponent parent, Dictionary<T, string> initiaItemList, int width, int height, string name)
        {
            this.name = name;
            m_parent = parent;
            ((UIPanel)parent).autoFitChildrenVertically = true;
            ((UIPanel)parent).padding = new RectOffset(20, 20, 20, 20);
            UIPanel panelListing = m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
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

            GameObject scrollObj = new GameObject("Lines Listing Scroll", new Type[] { typeof(UIScrollablePanel) });
            //			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message, "SCROLL LOADED");
            linesListPanel = scrollObj.GetComponent<UIScrollablePanel>();
            linesListPanel.autoLayout = false;
            linesListPanel.width = width;
            linesListPanel.height = height;
            linesListPanel.useTouchMouseScroll = true;
            linesListPanel.scrollWheelAmount = 20;
            linesListPanel.eventMouseWheel += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                linesListPanel.scrollPosition -= new Vector2(0, eventParam.wheelDelta * linesListPanel.scrollWheelAmount);
            };
            panelListing.AttachUIComponent(linesListPanel.gameObject);
            linesListPanel.autoLayout = true;
            linesListPanel.autoLayoutDirection = LayoutDirection.Vertical;

            linesListPanel.useTouchMouseScroll = true;
            linesListPanel.scrollWheelAmount = 20;
            linesListPanel.eventMouseWheel += (UIComponent component, UIMouseEventParameter eventParam) =>
            {
                linesListPanel.scrollPosition -= new Vector2(0, eventParam.wheelDelta * linesListPanel.scrollWheelAmount);
                eventParam.Use();
            };

            foreach (Transform t in linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            itemsList = initiaItemList;
        }

        private static void initButton(UIButton button, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite;
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite;
            button.hoveredBgSprite = spriteHov;
            button.focusedBgSprite = spriteHov;
            button.pressedBgSprite = spriteHov;
            button.textColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = Color.red;
            button.hoveredTextColor = Color.gray;
        }

        public void Redraw()
        {
            redrawButtons();
        }

        private void redrawButtons()
        {
            foreach (Transform t in linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }
            foreach (var entry in m_itemsList)
            {
                GameObject itemContainer = new GameObject();

                itemContainer.transform.parent = linesListPanel.transform;
                UIButtonWithId itemButton = itemContainer.AddComponent<UIButtonWithId>();

                itemButton.width = linesListPanel.width;
                itemButton.height = 35;

                initButton(itemButton, "EmptySprite");
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
                    selectedItem = (T)itemButton.id;
                    EventOnSelect(selectedItem);
                    eventParam.Use();
                };
                itemButton.text = entry.Value;
                itemButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
                itemButton.name = string.Format("[{1}] {0}", entry.Value, entry.Key);
            }
        }

        private void refreshSelection()
        {
            foreach (Transform t in linesListPanel.transform)
            {
                var b = t.GetComponent<UIButtonWithId>();
                if (b.id.Equals(selectedItem))
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


        public void unselect()
        {
            selectedItem = default(T);
        }

        public bool unselected
        {
            get {
                return selectedItem == null || selectedItem.Equals(default(T));
            }
        }

        public event OnButtonSelect<T> EventOnSelect;

        public delegate void addItemCallback(int idx, string text);
    }

    class UIButtonWithId : UIButton
    {
        public object id;
    }
}

