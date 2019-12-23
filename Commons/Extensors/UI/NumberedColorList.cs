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
    public class NumberedColorList
    {
        private readonly UIPanel m_linesListPanel;
        private readonly UIComponent m_parent;
        private List<Color32> m_colorList;
        private readonly UIButton m_add;

        public string m_spriteName = "EmptySprite";
        public UITextureAtlas m_atlasToUse = null;

        public List<Color32> ColorList
        {
            get {
                return m_colorList;
            }
            set {
                m_colorList = value;
                RedrawButtons();
            }
        }

        public void Enable()
        {
            m_linesListPanel.enabled = true;
            if (m_add)
            {
                m_add.enabled = true;
            }
            RedrawButtons();
        }
        public void Disable()
        {
            foreach (Transform t in m_linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }
            if (m_add)
            {
                m_add.enabled = false;
            }
            m_linesListPanel.enabled = false;
        }

        public NumberedColorList(UIComponent parent, List<Color32> initialColorList, UIComponent addButtonContainer)
        {
            m_parent = parent;
            parent.width = 500;
            ((UIPanel)parent).autoFitChildrenVertically = true;
            m_linesListPanel = m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
            m_linesListPanel.name = "NumberedColorList";
            m_linesListPanel.height = 40;
            m_linesListPanel.width = 500;
            m_linesListPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            m_linesListPanel.autoLayoutStart = LayoutStart.TopLeft;
            m_linesListPanel.autoFitChildrenVertically = true;
            m_linesListPanel.wrapLayout = true;
            m_linesListPanel.autoLayoutPadding = new RectOffset(5, 5, 5, 5);

            foreach (Transform t in m_linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            if (addButtonContainer != null)
            {
                m_add = addButtonContainer.GetComponentInChildren<UILabel>().AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kButtonTemplate)) as UIButton;
                m_add.text = "+";
                m_add.autoSize = false;
                m_add.height = 27;
                m_add.width = 27;
                m_add.relativePosition = new Vector3(70f, 0f, 0);
                m_add.textPadding = new RectOffset(0, 0, 0, 0);
                m_add.textHorizontalAlignment = UIHorizontalAlignment.Center;
                m_add.eventClick += delegate (UIComponent c, UIMouseEventParameter sel)
                {
                    m_colorList.Add(Color.white);
                    RedrawButtons();
                    EventOnAdd?.Invoke();
                };
            }
            ColorList = initialColorList;
        }

        private static void InitButton(UIButton button, string baseSprite, UITextureAtlas atlasToUse = null)
        {
            string sprite = baseSprite;
            string spriteHov = baseSprite;
            if(atlasToUse != null)
            {
                button.atlas = atlasToUse;
            }
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
            RedrawButtons();
        }

        private void RedrawButtons()
        {
            foreach (Transform t in m_linesListPanel.transform)
            {
                GameObject.Destroy(t.gameObject);
            }
            for (int j = 0; j < ColorList.Count; j++)
            {

                GameObject itemContainer = new GameObject();

                itemContainer.transform.parent = m_linesListPanel.transform;
                UIButtonWithId itemButton = itemContainer.AddComponent<UIButtonWithId>();

                itemButton.width = 35;
                itemButton.height = 35;

                InitButton(itemButton, m_spriteName, m_atlasToUse);
                itemButton.color = ColorList[j];
                itemButton.hoveredColor = itemButton.color;
                itemButton.pressedColor = itemButton.color;
                itemButton.focusedColor = itemButton.color;
                itemButton.textColor = Color.white;
                itemButton.hoveredColor = itemButton.textColor;
                itemButton.id = j + 1;
                itemButton.eventClick += (component, eventParam) =>
                {
                    EventOnClick?.Invoke(itemButton.id);
                };
                SetLineNumberMainListing(j + 1, itemButton);
                itemButton.name = "Color #" + (j + 1);
            }

        }

        private void SetLineNumberMainListing(int num, UIButton button)
        {
            UILabel l = button.AddUIComponent<UILabel>();
            l.autoSize = false;
            l.autoHeight = false;
            l.pivot = UIPivotPoint.TopLeft;
            l.verticalAlignment = UIVerticalAlignment.Middle;
            l.textAlignment = UIHorizontalAlignment.Center;
            l.relativePosition = new Vector3(0, 0);
            l.width = button.width;
            l.height = button.height;
            l.useOutline = true;
            l.text = num.ToString();
            float ratio = l.width / 50;
            if (l.text.Length == 4)
            {
                l.textScale = ratio;
                l.relativePosition = new Vector3(0f, 1f);
            }
            else if (l.text.Length == 3)
            {
                l.textScale = ratio * 1.25f;
                l.relativePosition = new Vector3(0f, 1.5f);
            }
            else if (l.text.Length == 2)
            {
                l.textScale = ratio * 1.75f;
                l.relativePosition = new Vector3(-0.5f, 0.5f);
            }
            else
            {
                l.textScale = ratio * 2.3f;
            }
        }

        private class UIButtonWithId : UIButton
        {
            public int id;
        }

        public event OnButtonSelect<int> EventOnClick;
        public event OnButtonClicked EventOnAdd;
    }
    public delegate void OnButtonSelect<T>(T idx);
}

