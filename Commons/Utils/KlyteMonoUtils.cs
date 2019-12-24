using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using System;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class KlyteMonoUtils
    {
        #region UI utils
        public static T CreateElement<T>(Transform parent, string name = null) where T : MonoBehaviour
        {
            CreateElement<T>(out T uiItem, parent, name);
            return uiItem;
        }
        public static void CreateElement<T>(out T uiItem, Transform parent, string name = null) where T : MonoBehaviour
        {
            var container = new GameObject();
            container.transform.parent = parent;
            uiItem = (T) container.AddComponent(typeof(T));
            if (name != null)
            {
                container.name = name;
            }
        }
        public static GameObject CreateElement(Type type, Transform parent)
        {
            var container = new GameObject();
            container.transform.parent = parent;
            container.AddComponent(type);
            return container;
        }
        public static void CreateUIElement<T>(out T uiItem, Transform parent, string name = null, Vector4 area = default) where T : UIComponent
        {
            var container = new GameObject();
            container.transform.parent = parent;
            uiItem = container.AddComponent<T>();
            if (name != null)
            {
                uiItem.name = name;
            }
            if (area != default)
            {
                uiItem.autoSize = false;
                uiItem.area = area;
            }
        }
        public static void UiTextFieldDefaults(UITextField uiItem)
        {
            uiItem.selectionSprite = "EmptySprite";
            uiItem.useOutline = true;
            uiItem.hoveredBgSprite = "TextFieldPanelHovered";
            uiItem.focusedBgSprite = "TextFieldPanel";
            uiItem.builtinKeyNavigation = true;
            uiItem.submitOnFocusLost = true;
        }
        public static Color ContrastColor(Color color)
        {
            if (color == default)
            {
                return Color.black;
            }
            // Counting the perceptive luminance - human eye favors green color... 
            float a = (0.299f * color.r) + (0.587f * color.g) + (0.114f * color.b);

            float d;
            if (a > 0.5)
            {
                d = 0; // bright colors - black font
            }
            else
            {
                d = 1; // dark colors - white font
            }

            return new Color(d, d, d, 1);
        }
        public static UIDragHandle CreateDragHandle(UIComponent parent, UIComponent target) => CreateDragHandle(parent, target, -1);
        public static UIDragHandle CreateDragHandle(UIComponent parent, UIComponent target, float height)
        {
            CreateUIElement(out UIDragHandle dh, parent.transform);
            dh.target = target;
            dh.relativePosition = new Vector3(0, 0);
            dh.width = parent.width;
            dh.height = height < 0 ? parent.height : height;
            dh.name = "DragHandle";
            dh.Start();
            return dh;
        }
        public static void InitButton(UIButton button, bool isCheck, string baseSprite, bool allLower = false)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite + "Disabled";
            button.hoveredBgSprite = spriteHov;
            button.focusedBgSprite = spriteHov;
            button.pressedBgSprite = isCheck ? sprite + "Pressed" : spriteHov;

            if (allLower)
            {
                button.normalBgSprite = button.normalBgSprite.ToLower();
                button.disabledBgSprite = button.disabledBgSprite.ToLower();
                button.hoveredBgSprite = button.hoveredBgSprite.ToLower();
                button.focusedBgSprite = button.focusedBgSprite.ToLower();
                button.pressedBgSprite = button.pressedBgSprite.ToLower();
            }

            button.textColor = new Color32(255, 255, 255, 255);
        }
        public static void InitButtonFull(UIButton button, bool isCheck, string baseSprite)
        {
            string sprite = baseSprite;
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite + "Disabled";
            button.hoveredBgSprite = baseSprite + "Focused";
            button.focusedBgSprite = baseSprite + "Hovered";
            button.pressedBgSprite = isCheck ? sprite + "Pressed" : button.focusedBgSprite;
            button.textColor = new Color32(255, 255, 255, 255);
        }
        public static void InitButtonSameSprite(UIButton button, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite;
            button.hoveredBgSprite = sprite;
            button.focusedBgSprite = sprite;
            button.pressedBgSprite = sprite;
            button.textColor = new Color32(255, 255, 255, 255);
        }
        public static void InitButtonFg(UIButton button, bool isCheck, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalFgSprite = sprite;
            button.disabledFgSprite = sprite;
            button.hoveredFgSprite = spriteHov;
            button.focusedFgSprite = spriteHov;
            button.pressedFgSprite = isCheck ? sprite + "Pressed" : spriteHov;
            button.textColor = new Color32(255, 255, 255, 255);
        }
        public static void CopySpritesEvents(UIButton source, UIButton target)
        {
            target.disabledBgSprite = source.disabledBgSprite;
            target.focusedBgSprite = source.focusedBgSprite;
            target.hoveredBgSprite = source.hoveredBgSprite;
            target.normalBgSprite = source.normalBgSprite;
            target.pressedBgSprite = source.pressedBgSprite;

            target.disabledFgSprite = source.disabledFgSprite;
            target.focusedFgSprite = source.focusedFgSprite;
            target.hoveredFgSprite = source.hoveredFgSprite;
            target.normalFgSprite = source.normalFgSprite;
            target.pressedFgSprite = source.pressedFgSprite;

        }


        public UISlider GenerateSliderField(UIHelperExtension uiHelper, OnValueChanged action, out UILabel label, out UIPanel container)
        {
            var budgetMultiplier = (UISlider) uiHelper.AddSlider("", 0f, 5, 0.05f, 1, action);
            label = budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>();
            label.autoSize = true;
            label.wordWrap = false;
            label.text = string.Format(" x{0:0.00}", 0);
            container = budgetMultiplier.GetComponentInParent<UIPanel>();
            container.width = 300;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            container.wrapLayout = true;
            return budgetMultiplier;
        }
        public static void ClearAllVisibilityEvents(UIComponent u)
        {
            System.Reflection.FieldInfo field = ReflectionUtils.GetEventField(u.GetType(), "eventVisibilityChanged");
            field.SetValue(u, null);
            for (int i = 0; i < u.components.Count; i++)
            {
                ClearAllVisibilityEvents(u.components[i]);
            }
        }

        public static PropertyChangedEventHandler<Vector2> LimitWidthAndBox<T>(T x) where T : UIComponent => LimitWidthAndBox(x, x.minimumSize.x, out _);
        public static PropertyChangedEventHandler<Vector2> LimitWidthAndBox<T>(T x, out UIPanel boxContainer) where T : UIComponent => LimitWidthAndBox(x, x.minimumSize.x, out boxContainer);
        public static PropertyChangedEventHandler<Vector2> LimitWidthAndBox<T>(T label, float maxWidth, bool alsoMinSize = false) where T : UIComponent => LimitWidthAndBox(label, maxWidth, out _, alsoMinSize);
        public static PropertyChangedEventHandler<Vector2> LimitWidthAndBox<T>(T label, float maxWidth, out UIPanel boxContainer, bool alsoMinSize = false) where T : UIComponent
        {
            CreateUIElement(out boxContainer, label.parent.transform, "CompoentContainer", new Vector4(0, 0, maxWidth, label.height));
            boxContainer.autoLayout = true;
            boxContainer.autoSize = true;
            boxContainer.zOrder = label.zOrder;
            boxContainer.maximumSize = new Vector2(maxWidth, 0);
            if (alsoMinSize)
            {
                boxContainer.minimumSize = new Vector2(maxWidth, 0);
            }
            label.transform.SetParent(boxContainer.transform);
            return LimitWidthPrivate(label, maxWidth, alsoMinSize);
        }

        [Obsolete("Use box version")]
        public static PropertyChangedEventHandler<Vector2> LimitWidth(UIComponent x) => LimitWidth(x, x.minimumSize.x);
        [Obsolete("Use box version")]
        public static PropertyChangedEventHandler<Vector2> LimitWidth(UIComponent x, float maxWidth, bool alsoMinSize = false) => LimitWidthPrivate(x, maxWidth, alsoMinSize);
        private static PropertyChangedEventHandler<Vector2> LimitWidthPrivate(UIComponent x, float maxWidth, bool alsoMinSize)
        {
            x.autoSize = true;
            void callback(UIComponent y, Vector2 z) => x.transform.localScale = new Vector3(Math.Min(1, maxWidth / x.width), x.transform.localScale.y, x.transform.localScale.z);
            x.eventSizeChanged += callback;
            if (alsoMinSize)
            {
                x.minimumSize = new Vector2(maxWidth, x.minimumSize.y);
            }
            callback(null, default);
            return callback;
        }
        public static UIHelperExtension CreateScrollPanel(UIComponent parent, out UIScrollablePanel scrollablePanel, out UIScrollbar scrollbar, float width, float height, Vector3 relativePosition)
        {
            CreateUIElement(out scrollablePanel, parent?.transform);
            scrollablePanel.width = width;
            scrollablePanel.height = height;
            scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            scrollablePanel.autoLayout = true;
            scrollablePanel.clipChildren = true;
            scrollablePanel.relativePosition = relativePosition;

            CreateUIElement(out UIPanel trackballPanel, parent?.transform);
            trackballPanel.width = 10f;
            trackballPanel.height = scrollablePanel.height;
            trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
            trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            trackballPanel.autoLayout = true;
            trackballPanel.relativePosition = new Vector3(relativePosition.x + width + 5, relativePosition.y);


            CreateUIElement(out scrollbar, trackballPanel.transform);
            scrollbar.width = 10f;
            scrollbar.height = scrollbar.parent.height;
            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.pivot = UIPivotPoint.BottomLeft;
            scrollbar.AlignTo(trackballPanel, UIAlignAnchor.TopRight);
            scrollbar.minValue = 0f;
            scrollbar.value = 0f;
            scrollbar.incrementAmount = 25f;

            CreateUIElement(out UISlicedSprite scrollBg, scrollbar.transform);
            scrollBg.relativePosition = Vector2.zero;
            scrollBg.autoSize = true;
            scrollBg.size = scrollBg.parent.size;
            scrollBg.fillDirection = UIFillDirection.Vertical;
            scrollBg.spriteName = "ScrollbarTrack";
            scrollbar.trackObject = scrollBg;

            CreateUIElement(out UISlicedSprite scrollFg, scrollBg.transform);
            scrollFg.relativePosition = Vector2.zero;
            scrollFg.fillDirection = UIFillDirection.Vertical;
            scrollFg.autoSize = true;
            scrollFg.width = scrollFg.parent.width - 4f;
            scrollFg.spriteName = "ScrollbarThumb";
            scrollbar.thumbObject = scrollFg;
            scrollablePanel.verticalScrollbar = scrollbar;
            scrollablePanel.eventMouseWheel += delegate (UIComponent component, UIMouseEventParameter param)
            {
                ((UIScrollablePanel) component).scrollPosition += new Vector2(0f, Mathf.Sign(param.wheelDelta) * -1f * ((UIScrollablePanel) component).verticalScrollbar.incrementAmount);
            };

            return new UIHelperExtension(scrollablePanel);
        }

        public static UIHelperExtension CreateHorizontalScrollPanel(UIComponent parent, out UIScrollablePanel scrollablePanel, out UIScrollbar scrollbar, float width, float height, Vector3 relativePosition)
        {
            CreateUIElement(out scrollablePanel, parent?.transform);
            scrollablePanel.width = width;
            scrollablePanel.height = height;
            scrollablePanel.autoLayoutDirection = LayoutDirection.Horizontal;
            scrollablePanel.wrapLayout = false;
            scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            scrollablePanel.autoLayout = true;
            scrollablePanel.clipChildren = true;
            scrollablePanel.relativePosition = relativePosition;

            CreateUIElement(out UIPanel trackballPanel, parent?.transform);
            trackballPanel.height = 10f;
            trackballPanel.width = scrollablePanel.width;
            trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
            trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            trackballPanel.autoLayout = true;
            trackballPanel.relativePosition = new Vector3(relativePosition.x, relativePosition.y + height + 5);


            CreateUIElement(out scrollbar, trackballPanel.transform);
            scrollbar.height = 10f;
            scrollbar.width = scrollbar.parent.width;
            scrollbar.orientation = UIOrientation.Horizontal;
            scrollbar.pivot = UIPivotPoint.BottomLeft;
            scrollbar.AlignTo(trackballPanel, UIAlignAnchor.TopLeft);
            scrollbar.minValue = 0f;
            scrollbar.value = 0f;
            scrollbar.incrementAmount = 25f;

            CreateUIElement(out UISlicedSprite scrollBg, scrollbar.transform);
            scrollBg.relativePosition = Vector2.zero;
            scrollBg.autoSize = true;
            scrollBg.size = scrollBg.parent.size;
            scrollBg.fillDirection = UIFillDirection.Horizontal;
            scrollBg.spriteName = "ScrollbarTrack";
            scrollbar.trackObject = scrollBg;

            CreateUIElement(out UISlicedSprite scrollFg, scrollBg.transform);
            scrollFg.relativePosition = Vector2.zero;
            scrollFg.fillDirection = UIFillDirection.Horizontal;
            scrollFg.autoSize = true;
            scrollFg.width = scrollFg.parent.width - 4f;
            scrollFg.spriteName = "ScrollbarThumb";
            scrollbar.thumbObject = scrollFg;
            scrollablePanel.horizontalScrollbar = scrollbar;
            scrollablePanel.eventMouseWheel += delegate (UIComponent component, UIMouseEventParameter param)
            {
                ((UIScrollablePanel) component).scrollPosition += new Vector2(Mathf.Sign(param.wheelDelta) * -1f * ((UIScrollablePanel) component).horizontalScrollbar.incrementAmount, 0);
            };

            return new UIHelperExtension(scrollablePanel);
        }

        private static UIColorField m_colorFieldTemplate;
        public static UIColorField CreateColorField(UIComponent parent)
        {
            if (m_colorFieldTemplate == null)
            {
                UIComponent uIComponent = UITemplateManager.Get("LineTemplate");
                if (uIComponent == null)
                {
                    return null;
                }
                m_colorFieldTemplate = uIComponent.Find<UIColorField>("LineColor");
                if (m_colorFieldTemplate == null)
                {
                    return null;
                }
            }
            var go = GameObject.Instantiate(m_colorFieldTemplate.gameObject, parent.transform);
            UIColorField component = go.GetComponent<UIColorField>();
            component.pickerPosition = UIColorField.ColorPickerPosition.RightAbove;
            component.transform.SetParent(parent.transform);
            component.eventColorPickerOpen += DefaultColorPickerHandler;
            component.size = new Vector2(28, 28);
            return component;
        }
        private static void DefaultColorPickerHandler(UIColorField dropdown, UIColorPicker popup, ref bool overridden)
        {
            UIPanel panel = popup.GetComponent<UIPanel>();
            overridden = true;
            panel.height = 250;
            CreateUIElement<UITextField>(out UITextField textField, panel.transform, "ColorText", new Vector4(15, 225, 200, 20));
            UiTextFieldDefaults(textField);
            textField.normalBgSprite = "TextFieldPanel";
            textField.maxLength = 6;
            textField.eventKeyUp += (x, y) =>
            {
                if (popup && textField.text.Length == 6)
                {
                    try
                    {
                        Color32 targetColor = ColorExtensions.FromRGB(((UITextField) x).text);
                        dropdown.selectedColor = targetColor;
                        popup.color = targetColor;
                        ((UITextField) x).textColor = Color.white;
                        ((UITextField) x).text = targetColor.ToRGB();
                    }
                    catch
                    {
                        ((UITextField) x).textColor = Color.red;
                    }
                }
            };
            popup.eventColorUpdated += (x) => textField.text = ((Color32) x).ToRGB();
            textField.text = ((Color32) popup.color).ToRGB();
        }

        #endregion
        public static void CreateTabsComponent(out UITabstrip tabstrip, out UITabContainer tabContainer, Transform parent, string namePrefix, Vector4 areaTabstrip, Vector4 areaContainer)
        {
            KlyteMonoUtils.CreateUIElement(out tabstrip, parent, $"{namePrefix}_Tabstrip", areaTabstrip);

            KlyteMonoUtils.CreateUIElement(out tabContainer, parent, $"{namePrefix}_TabContainer", areaContainer);
            tabstrip.tabPages = tabContainer;
            tabstrip.selectedIndex = 0;
            tabstrip.selectedIndex = -1;
        }
    }

}
