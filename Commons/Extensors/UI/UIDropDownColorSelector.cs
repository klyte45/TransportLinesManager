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
    public class DropDownColorSelector
    {
        private UIDropDown m_r;
        private UIDropDown m_g;
        private UIDropDown m_b;
        private UILabel m_displayColor;
        private UILabel m_title;
        private UIComponent m_parent;
        private UIPanel uIPanel;
        private UIButton m_remove;
        private static string[] options;
        public int id;

        public Color32 selectedColor
        {
            get {
                return new Color32((byte)(m_r.selectedIndex == 0 ? 0 : m_r.selectedIndex * 4 - 1), (byte)(m_g.selectedIndex == 0 ? 0 : m_g.selectedIndex * 4 - 1), (byte)(m_b.selectedIndex == 0 ? 0 : m_b.selectedIndex * 4 - 1), 255);
            }
            set {
                setSelectedColor(value);
            }
        }

        public String title
        {
            get {
                if (m_title != null)
                {
                    return m_title.text;
                }
                return null;
            }
            set {
                if (m_title != null)
                {
                    m_title.text = value;
                }
            }
        }

        public UIComponent parent
        {
            get {
                return m_parent;
            }
        }

        public void Disable()
        {
            uIPanel.enabled = false;
        }

        public void Enable()
        {
            uIPanel.enabled = true;
        }


        private void setSelectedColor(Color32 val)
        {

            m_r.selectedIndex = (val.r + 1) / 4;
            m_g.selectedIndex = (val.g + 1) / 4;
            m_b.selectedIndex = (val.b + 1) / 4;
        }

        public DropDownColorSelector(UIComponent parent, Color initialColor, int id = 0)
        {
            this.id = 0;
            m_parent = parent;
            uIPanel = m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
            uIPanel.name = "DropDownColorSelector";
            uIPanel.height = 40;
            uIPanel.width = 280;
            uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            uIPanel.autoLayoutStart = LayoutStart.TopLeft;
            uIPanel.autoFitChildrenVertically = true;

            m_title = uIPanel.Find<UILabel>("Label");
            m_title.autoSize = false;
            m_title.height = 28;
            m_title.width = 60;
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.padding = new RectOffset(5, 5, 5, 5);

            m_r = uIPanel.Find<UIDropDown>("Dropdown");
            this.m_g = uIPanel.AttachUIComponent(GameObject.Instantiate(m_r.gameObject)) as UIDropDown;
            m_b = uIPanel.AttachUIComponent(GameObject.Instantiate(m_r.gameObject)) as UIDropDown;
            initializeDropDown(ref m_b);
            initializeDropDown(ref m_r);
            initializeDropDown(ref m_g);

            m_r.color = new Color32(255, 0, 0, 255);
            m_g.color = new Color32(0, 255, 0, 255);
            m_b.color = new Color32(0, 0, 255, 255);

            m_displayColor = GameObject.Instantiate(uIPanel.Find<UILabel>("Label").gameObject, uIPanel.transform).GetComponent<UILabel>();
            m_displayColor.autoSize = false;
            m_displayColor.name = "Color result";
            m_displayColor.relativePosition += new Vector3(0, 160, 0);
            m_displayColor.text = "";
            m_displayColor.height = 28;
            m_displayColor.width = 100;
            m_displayColor.textAlignment = UIHorizontalAlignment.Center;
            m_displayColor.backgroundSprite = "EmptySprite";
            m_displayColor.useOutline = true;
            m_displayColor.outlineColor = Color.black;
            m_displayColor.textColor = Color.white;
            m_displayColor.padding = new RectOffset(5, 5, 5, 5);

            m_remove = uIPanel.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kButtonTemplate)) as UIButton;
            m_remove.text = "x";
            m_remove.autoSize = false;
            m_remove.height = 27;
            m_remove.width = 27;
            m_remove.textPadding = new RectOffset(0, 0, 0, 0);
            m_remove.textHorizontalAlignment = UIHorizontalAlignment.Center;
            m_remove.eventClick += delegate (UIComponent c, UIMouseEventParameter sel)
            {
                Disable();
                eventOnRemove?.Invoke();
            };

            setSelectedColor(initialColor);

        }

        private void initializeDropDown(ref UIDropDown dropDown)
        {
            if (options == null)
            {
                List<string> optionsList = new List<string>();
                for (int i = 0; i <= 64; i++)
                {
                    optionsList.Add(String.Format("{0:X2}", i == 0 ? 0 : (i * 4) - 1));
                }
                options = optionsList.ToArray();
            }
            dropDown.items = options;
            dropDown.eventSelectedIndexChanged += (component, value) =>
            {
                m_displayColor.color = this.selectedColor;
                m_displayColor.text = String.Format("#{0:X2}{1:X2}{2:X2}", this.selectedColor.r, this.selectedColor.g, this.selectedColor.b);
                if (this.eventColorChanged != null)
                {
                    eventColorChanged(this.selectedColor);
                }
            };
            dropDown.useOutline = true;
            dropDown.outlineColor = Color.black;
            dropDown.textColor = Color.white;
            dropDown.width = 60;
            dropDown.height = 28;
            dropDown.itemPadding = new RectOffset(4, 4, 0, 0);
            dropDown.textFieldPadding = new RectOffset(5, 10, 5, 5);

            dropDown.focusedBgSprite = dropDown.normalBgSprite;
            dropDown.hoveredBgSprite = dropDown.normalBgSprite;
        }

        public event ColorChangeHandler eventColorChanged;
        public event ButtonClickHandler eventOnRemove;

        public void Destroy()
        {
            GameObject.Destroy(m_parent.gameObject);
        }

        public delegate void ButtonClickHandler();

        public delegate void ColorChangeHandler(Color32 value);

        public delegate void ListColorChangeHandler(List<Color32> values);
    }


}


