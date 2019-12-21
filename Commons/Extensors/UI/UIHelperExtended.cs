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
using System.Reflection;
using Klyte.Commons.Utils;

namespace Klyte.Commons.Extensors
{
    public class UIHelperExtension : UIHelperBase
    {

        //
        // Static Fields
        //
        public static readonly string kButtonTemplate = "OptionsButtonTemplate";
        public static readonly string kGroupTemplate = "OptionsGroupTemplate";
        public static readonly string kDropdownTemplate = "OptionsDropdownTemplate";
        public static readonly string kCheckBoxTemplate = "OptionsCheckBoxTemplate";
        public static readonly string kSliderTemplate = "OptionsSliderTemplate";
        public static readonly string kTextfieldTemplate = "OptionsTextfieldTemplate";
        public static readonly string kGroupPropertyTemplate = "GroupPropertySet";

        public static readonly UIFont defaultFontCheckbox = ((UITemplateManager.GetAsGameObject(kCheckBoxTemplate)).GetComponent<UICheckBox>()).label.font;

        public static string version
        {
            get {
                return typeof(UIHelperExtension).Assembly.GetName().Version.Major + "." + typeof(UIHelperExtension).Assembly.GetName().Version.Minor + "." + typeof(UIHelperExtension).Assembly.GetName().Version.Build;
            }
        }

        //
        // Fields
        //
        private UIComponent m_Root;

        //
        // Properties
        //
        public UIComponent self
        {
            get {
                return this.m_Root;
            }
        }

        //
        // Methods
        //
        public object AddButton(string text, OnButtonClicked eventCallback)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(text))
            {
                UIButton uIButton = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kButtonTemplate)) as UIButton;
                uIButton.text = text;
                uIButton.eventClick += delegate (UIComponent c, UIMouseEventParameter sel)
                {
                    eventCallback();
                };
                return uIButton;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create button with no name or no event");
            return null;
        }

        public object AddCheckbox(string text, bool defaultValue, OnCheckChanged eventCallback)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(text))
            {
                UICheckBox uICheckBox = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kCheckBoxTemplate)) as UICheckBox;
                uICheckBox.text = text;
                uICheckBox.isChecked = defaultValue;
                uICheckBox.eventCheckChanged += delegate (UIComponent c, bool isChecked)
                {
                    eventCallback(isChecked);
                };
                return uICheckBox;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create checkbox with no name or no event");
            return null;
        }

        internal void AddDropdownLocalized(string v, object p1, object value, Action<int> p2)
        {
            throw new NotImplementedException();
        }

        public UICheckBox AddCheckboxLocale(string text, bool defaultValue, OnCheckChanged eventCallback = null)
        {
            if (!string.IsNullOrEmpty(text))
            {
                UICheckBox uICheckBox = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kCheckBoxTemplate)) as UICheckBox;
                uICheckBox.label.isLocalized = true;
                uICheckBox.label.localeID = text;
                uICheckBox.isChecked = defaultValue;
                if (eventCallback != null)
                {
                    uICheckBox.eventCheckChanged += delegate (UIComponent c, bool isChecked)
                    {
                        eventCallback(isChecked);
                    };
                }
                return uICheckBox;
            }
            throw new NotSupportedException("Cannot create checkbox with no name");
        }
        public UICheckBox AddCheckboxNoLabel(string name, OnCheckChanged eventCallback = null)
        {
            UICheckBox uICheckBox = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kCheckBoxTemplate)) as UICheckBox;
            uICheckBox.width = uICheckBox.height;
            GameObject.Destroy(uICheckBox.label.gameObject);
            uICheckBox.name = name;
            if (eventCallback != null)
            {
                uICheckBox.eventCheckChanged += delegate (UIComponent c, bool isChecked)
                {
                    eventCallback(isChecked);
                };
            }
            return uICheckBox;
        }

        public object AddDropdown(string text, string[] options, int defaultSelection, OnDropdownSelectionChanged eventCallback)
        {
            return AddDropdown(text, options, defaultSelection, eventCallback, false);
        }

        public UIDropDown AddDropdown(string text, string[] options, int defaultSelection, OnDropdownSelectionChanged eventCallback, bool limitLabelByPanelWidth = false)
        {
            UIDropDown uIDropDown = AddDropdownBase(text, options, eventCallback, limitLabelByPanelWidth);
            if (uIDropDown != null)
            {
                uIDropDown.selectedIndex = defaultSelection;
                return uIDropDown;
            }
            else
            {
                return null;
            }
        }

        public void AddLabel(object p)
        {
            throw new NotImplementedException();
        }

        public UIDropDown AddDropdownLocalized(string text, string[] options, int defaultSelection, OnDropdownSelectionChanged eventCallback, bool limitLabelByPanelWidth = false)
        {
            UIDropDown uIDropDown = AddDropdownBaseLocalized(text, options, eventCallback, defaultSelection, limitLabelByPanelWidth);
            if (uIDropDown != null)
            {
                return uIDropDown;
            }
            else
            {
                return null;
            }
        }

        public UIDropDown AddDropdown(string text, string[] options, string defaultSelection, OnDropdownSelectionChanged eventCallback, bool limitLabelByPanelWidth = false)
        {
            UIDropDown uIDropDown = AddDropdownBase(text, options, eventCallback, limitLabelByPanelWidth);
            if (uIDropDown != null)
            {
                bool hasIdx = options.Contains(defaultSelection);
                if (hasIdx)
                {
                    uIDropDown.selectedIndex = options.ToList().IndexOf(defaultSelection);
                }
                return uIDropDown;
            }
            else
            {
                return null;
            }
        }

        private UIDropDown AddDropdownBase(string text, string[] options, OnDropdownSelectionChanged eventCallback, bool limitLabelByPanelWidth = false)
        {
            return CloneBasicDropDown(text, options, eventCallback, this.m_Root, limitLabelByPanelWidth);
        }

        public static UIDropDown CloneBasicDropDown(string text, string[] options, OnDropdownSelectionChanged eventCallback, UIComponent parent, out UILabel label, bool limitLabelByPanelWidth = false)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(text))
            {
                UIPanel uIPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject(kDropdownTemplate)) as UIPanel;
                label = uIPanel.Find<UILabel>("Label");
                if (limitLabelByPanelWidth) { KlyteUtils.LimitWidth(label, (uint)uIPanel.width); }
                label.text = text;
                UIDropDown uIDropDown = uIPanel.Find<UIDropDown>("Dropdown");
                uIDropDown.items = options;
                uIDropDown.eventSelectedIndexChanged += delegate (UIComponent c, int sel)
                {
                    eventCallback(sel);
                };
                return uIDropDown;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
            label = null;
            return null;
        }
        public static UIDropDown CloneBasicDropDown(string text, string[] options, OnDropdownSelectionChanged eventCallback, UIComponent parent, bool limitLabelByPanelWidth = false)
        {
            return CloneBasicDropDown(text, options, eventCallback, parent, out UILabel l, limitLabelByPanelWidth);
        }


        private UIDropDown AddDropdownBaseLocalized(string text, string[] options, OnDropdownSelectionChanged eventCallback, int defaultSelection, bool limitLabelByPanelWidth = false)
        {
            return CloneBasicDropDownLocalized(text, options, eventCallback, defaultSelection, this.m_Root, limitLabelByPanelWidth);
        }

        public static UIDropDown CloneBasicDropDownLocalized(string text, string[] options, OnDropdownSelectionChanged eventCallback, int defaultSelection, UIComponent parent, bool limitLabelByPanelWidth = false)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(text))
            {
                UIPanel uIPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject(kDropdownTemplate)) as UIPanel;
                uIPanel.Find<UILabel>("Label").localeID = text;
                uIPanel.Find<UILabel>("Label").isLocalized = true;
                if (limitLabelByPanelWidth) { KlyteUtils.LimitWidth(uIPanel.Find<UILabel>("Label"), (uint)uIPanel.width); }
                UIDropDown uIDropDown = uIPanel.Find<UIDropDown>("Dropdown");
                uIDropDown.items = options;
                uIDropDown.selectedIndex = defaultSelection;
                uIDropDown.eventSelectedIndexChanged += delegate (UIComponent c, int sel)
                {
                    eventCallback(sel);
                };
                return uIDropDown;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
            return null;
        }

        public static UIDropDown CloneBasicDropDownNoLabel(string[] options, OnDropdownSelectionChanged eventCallback, UIComponent parent)
        {
            if (eventCallback != null)
            {
                UIDropDown uIDropDown = GameObject.Instantiate(UITemplateManager.GetAsGameObject(kDropdownTemplate).GetComponentInChildren<UIDropDown>().gameObject, parent.transform).GetComponent<UIDropDown>();
                uIDropDown.items = options;
                uIDropDown.eventSelectedIndexChanged += delegate (UIComponent c, int sel)
                {
                    eventCallback(sel);
                };
                return uIDropDown;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
            return null;
        }

        public object AddSlider(string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(text))
            {
                UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kSliderTemplate)) as UIPanel;
                uIPanel.Find<UILabel>("Label").text = text;
                UISlider uISlider = uIPanel.Find<UISlider>("Slider");
                uISlider.minValue = min;
                uISlider.maxValue = max;
                uISlider.stepSize = step;
                uISlider.value = defaultValue;
                uISlider.eventValueChanged += delegate (UIComponent c, float val)
                {
                    eventCallback(val);
                };
                return uISlider;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create slider with no name or no event");
            return null;
        }

        public object AddSpace(int height)
        {
            if (height > 0)
            {
                UIPanel uIPanel = this.m_Root.AddUIComponent<UIPanel>();
                uIPanel.name = "Space";
                uIPanel.isInteractive = false;
                uIPanel.height = (float)height;
                return uIPanel;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create space of " + height + " height");
            return null;
        }

        public UIHelperExtension(UIComponent panel)
        {
            this.m_Root = panel;
        }

        public UIHelperExtension(UIHelper panel)
        {
            this.m_Root = (UIComponent)panel.self;
        }

        public UIHelperExtension AddGroupExtended(string text)
        {
            return AddGroupExtended(text, out UILabel label, out UIPanel parentPanel);
        }

        public UIHelperBase AddGroup(string text)
        {
            return AddGroupExtended(text, out UILabel label, out UIPanel parentPanel);
        }

        public UIHelperExtension AddGroupExtended(string text, out UILabel label, out UIPanel parentPanel)
        {
            if (!string.IsNullOrEmpty(text))
            {
                parentPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kGroupTemplate)) as UIPanel;
                label = parentPanel.Find<UILabel>("Label");
                label.text = text;
                return new UIHelperExtension(parentPanel.Find("Content"));
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create group with no name");
            label = null;
            parentPanel = null;
            return null;
        }

        public object AddTextfield(string text, string defaultContent, OnTextChanged eventChangedCallback, OnTextSubmitted eventSubmittedCallback)
        {
            if ((eventChangedCallback != null || eventSubmittedCallback != null) && !string.IsNullOrEmpty(text))
            {
                UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kTextfieldTemplate)) as UIPanel;
                uIPanel.Find<UILabel>("Label").text = text;
                UITextField uITextField = uIPanel.Find<UITextField>("Text Field");
                uITextField.text = defaultContent;
                uITextField.eventTextChanged += delegate (UIComponent c, string sel)
                {
                    eventChangedCallback?.Invoke(sel);
                };
                uITextField.eventTextSubmitted += delegate (UIComponent c, string sel)
                {
                    eventSubmittedCallback?.Invoke(sel);
                };
                return uITextField;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
            return null;
        }

        public UITextField[] AddVector3Field(string name, Vector3 defaultValue, Action<Vector3> eventSubmittedCallback)
        {
            if ((eventSubmittedCallback != null) && !string.IsNullOrEmpty(name))
            {
                UITextField[] result = new UITextField[3];
                UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kTextfieldTemplate)) as UIPanel;
                uIPanel.Find<UILabel>("Label").text = name;
                uIPanel.autoLayout = true;
                uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                uIPanel.wrapLayout = false;
                uIPanel.autoFitChildrenVertically = true;
                result[0] = uIPanel.Find<UITextField>("Text Field");
                result[0].numericalOnly = true;
                result[0].width = 60;
                result[0].allowNegative = true;
                result[0].allowFloats = true;
                result[1] = GameObject.Instantiate(result[0]);
                result[2] = GameObject.Instantiate(result[0]);
                result[1].transform.SetParent(result[0].transform.parent);
                result[2].transform.SetParent(result[0].transform.parent);

                void textSubmitAction(UIComponent c, string sel)
                {
                    Vector3 resultV3 = new Vector3();
                    float.TryParse(result[0].text, out resultV3.x);
                    float.TryParse(result[1].text, out resultV3.y);
                    float.TryParse(result[2].text, out resultV3.z);
                    eventSubmittedCallback?.Invoke(resultV3);
                }
                result[0].eventTextSubmitted += textSubmitAction;
                result[1].eventTextSubmitted += textSubmitAction;
                result[2].eventTextSubmitted += textSubmitAction;
                result[0].text = defaultValue.x.ToString();
                result[1].text = defaultValue.y.ToString();
                result[2].text = defaultValue.z.ToString();
                result[1].text = defaultValue.y.ToString();
                result[0].zOrder = 1;
                result[1].zOrder = 2;
                result[2].zOrder = 3;
                return result;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
            return null;
        }

        public UITextField AddFloatField(string name, float defaultValue, Action<float> eventSubmittedCallback, bool acceptNegative = true)
        {
            if ((eventSubmittedCallback != null) && !string.IsNullOrEmpty(name))
            {
                UITextField result;
                UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(kTextfieldTemplate)) as UIPanel;
                uIPanel.Find<UILabel>("Label").text = name;
                uIPanel.autoLayout = true;
                uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                uIPanel.wrapLayout = false;
                uIPanel.autoFitChildrenVertically = true;
                result = uIPanel.Find<UITextField>("Text Field");
                result.numericalOnly = true;
                result.width = 60;
                result.allowNegative = acceptNegative;
                result.allowFloats = true;

                void textSubmitAction(UIComponent c, string sel)
                {
                    float.TryParse(result.text, out float val);
                    eventSubmittedCallback?.Invoke(val);
                }
                result.eventTextSubmitted += textSubmitAction;
                result.text = defaultValue.ToString();
                return result;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
            return null;
        }

        public UITextField AddTextField(string name, OnTextChanged eventCallback, string defaultValue = "", OnTextSubmitted eventSubmit = null)
        {
            return (UITextField)AddTextfield(name, defaultValue, eventCallback, eventSubmit);
        }

        public UITextField AddPasswordField(string name, OnTextChanged eventCallback)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(name))
            {
                UITextField uITextField = (UITextField)AddTextfield(name, "", eventCallback, null);
                uITextField.isPasswordField = true;
                return uITextField;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create textField with no name or no event");
            return null;
        }

        public UILabel AddLabel(string name)
        {

            UIPanel uIPanel = m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
            uIPanel.autoFitChildrenVertically = true;
            UILabel label = uIPanel.Find<UILabel>("Label");
            label.text = name;
            label.maximumSize = new Vector2(700, 9999);
            label.minimumSize = new Vector2(700, 0);
            label.wordWrap = true;
            GameObject.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);

            return label;

        }

        public UITextureSprite AddNamedTexture(string name)
        {
            UIPanel uIPanel = m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
            uIPanel.Find<UILabel>("Label").text = name;
            GameObject.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);


            UITextureSprite uITextureSprite = uIPanel.AddUIComponent<UITextureSprite>();
            uITextureSprite.isVisible = true;
            uITextureSprite.name = "TextureSprite";
            return uITextureSprite;

        }

        public DropDownColorSelector AddColorField(string name, Color defaultValue, OnColorChanged eventCallback, OnButtonClicked eventRemove)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(name))
            {
                UIPanel uIPanel = m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
                uIPanel.name = "DropDownColorSelector";
                uIPanel.Find<UILabel>("Label").text = name;
                GameObject.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
                DropDownColorSelector ddcs = new DropDownColorSelector(uIPanel, defaultValue);

                ddcs.eventColorChanged += (Color32 value) =>
                {
                    eventCallback(value);
                };

                ddcs.eventOnRemove += () =>
                {
                    eventRemove?.Invoke();
                };
                return ddcs;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
            return null;
        }

        public UIColorField AddColorPicker(string name, Color defaultValue, OnColorChanged eventCallback)
        {
            return AddColorPicker(name, defaultValue, eventCallback, out UILabel title);
        }

        public UIColorField AddColorPicker(string name, Color defaultValue, OnColorChanged eventCallback, out UILabel title)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(name))
            {
                UIPanel panel = m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
                panel.name = "DropDownColorSelector";
                title = panel.Find<UILabel>("Label");
                title.text = name;
                panel.autoLayoutDirection = LayoutDirection.Horizontal;
                panel.wrapLayout = false;
                panel.autoFitChildrenVertically = true;
                GameObject.Destroy(panel.Find<UIDropDown>("Dropdown").gameObject);
                var colorField = KlyteUtils.CreateColorField(panel);

                colorField.eventSelectedColorReleased += (cp, value) =>
                {
                    eventCallback(value);
                };

                return colorField;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
            title = null;
            return null;
        }

        public UIColorField AddColorPickerNoLabel(string name, Color defaultValue, OnColorChanged eventCallback)
        {
            if (eventCallback != null && !string.IsNullOrEmpty(name))
            {
                var colorField = KlyteUtils.CreateColorField(m_Root);

                colorField.eventSelectedColorReleased += (cp, value) =>
                {
                    eventCallback(value);
                };

                return colorField;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
            return null;
        }

        public NumberedColorList AddNumberedColorList(string name, List<Color32> defaultValues, OnButtonSelect<int> eventCallback, UIComponent addButtonContainer, OnButtonClicked eventAdd)
        {
            if (eventCallback != null)
            {
                UIPanel uIPanel = m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
                uIPanel.name = "NumberedColorList";
                if (string.IsNullOrEmpty(name))
                {
                    uIPanel.Find<UILabel>("Label").text = "";
                }
                else
                {
                    uIPanel.Find<UILabel>("Label").text = name;
                }
                GameObject.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
                NumberedColorList ddcs = new NumberedColorList(uIPanel, defaultValues, addButtonContainer);

                ddcs.eventOnClick += (int value) =>
                {
                    eventCallback(value);
                };

                ddcs.eventOnAdd += () =>
                {
                    eventAdd?.Invoke();
                };
                return ddcs;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
            return null;
        }


        public TextList<T> AddTextList<T>(string name, Dictionary<T, string> defaultValues, OnButtonSelect<T> eventCallback, int width, int height)
        {
            if (eventCallback != null)
            {
                UIPanel uIPanel = m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
                uIPanel.name = "NumberedColorList";
                if (string.IsNullOrEmpty(name))
                {
                    uIPanel.Find<UILabel>("Label").text = "";
                }
                else
                {
                    uIPanel.Find<UILabel>("Label").text = name;
                }
                GameObject.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
                TextList<T> ddcs = new TextList<T>(uIPanel, defaultValues, width, height, name);

                ddcs.EventOnSelect += (T value) =>
                {
                    eventCallback(value);
                };

                return ddcs;
            }
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
            return null;
        }

        #region Property Group
        private void OnGroupClicked(UIComponent comp, UIMouseEventParameter p)
        {
            UILabel uibutton = p.source as UILabel;
            if (uibutton != null && !string.IsNullOrEmpty(uibutton.stringUserData))
            {
                if (this.m_GroupStates.TryGetValue(uibutton.stringUserData, out DecorationPropertiesPanel.GroupInfo groupInfo))
                {
                    uibutton.backgroundSprite = ((!groupInfo.m_Folded) ? "OptionsDropbox" : "OptionsDropboxFocused");
                    groupInfo.m_Folded = !groupInfo.m_Folded;
                    RecalculateHeight(groupInfo);
                    this.m_GroupStates[uibutton.stringUserData] = groupInfo;
                }
            }
        }

        public void RecalculateHeight(UIComponent toggleComponent)
        {
            if (this.m_GroupStates.TryGetValue(toggleComponent.stringUserData, out DecorationPropertiesPanel.GroupInfo groupInfo))
            {
                RecalculateHeight(groupInfo);
            }
        }

        private void RecalculateHeight(DecorationPropertiesPanel.GroupInfo groupInfo)
        {
            if (!groupInfo.m_Folded)
            {
                UIPanel propertyContainer = groupInfo.m_PropertyContainer;
                propertyContainer.Show();
                float endValue = this.CalculateHeight(propertyContainer);
                ValueAnimator.Animate("PropGroupProp general", delegate (float val)
                {
                    Vector2 size = groupInfo.m_Container.size;
                    size.y = val;
                    groupInfo.m_Container.size = size;
                }, new AnimatedFloat(m_DefaultGroupHeight, endValue, 0.2f));
            }
            else
            {
                UIPanel container = groupInfo.m_PropertyContainer;
                float startValue = this.CalculateHeight(container);
                ValueAnimator.Animate("PropGroupProp general", delegate (float val)
                {
                    Vector2 size = groupInfo.m_Container.size;
                    size.y = val;
                    groupInfo.m_Container.size = size;
                }, new AnimatedFloat(startValue, m_DefaultGroupHeight, 0.2f), delegate ()
                {
                    container.Hide();
                });
            }
        }


        // Token: 0x0600125B RID: 4699 RVA: 0x000FA85C File Offset: 0x000F8C5C
        private float CalculatePropertiesHeight(UIPanel comp)
        {
            float num = 0f;
            for (int i = 0; i < comp.childCount; i++)
            {
                num += comp.components[i].size.y + (float)comp.autoLayoutPadding.vertical;
            }
            return num;
        }

        // Token: 0x0600125C RID: 4700 RVA: 0x000FA8B0 File Offset: 0x000F8CB0
        private float CalculateHeight(UIPanel container)
        {
            float num = 0f;
            num += this.m_DefaultGroupHeight;
            num += container.padding.top + container.padding.bottom;
            return num + this.CalculatePropertiesHeight(container);
        }

        public UIHelperExtension AddTogglableGroup(string title)
        {
            return AddTogglableGroup(title, out UILabel toggleLabel);
        }

        public UIHelperExtension AddTogglableGroup(string title, out UILabel toggleLabel)
        {
            if (m_GroupStates == null) m_GroupStates = new Dictionary<string, DecorationPropertiesPanel.GroupInfo>();
            var newGroup = AddGroupExtended(title, out toggleLabel, out UIPanel parentPanel);
            toggleLabel.text = title;
            toggleLabel.stringUserData = title;
            toggleLabel.eventClick += new MouseEventHandler(OnGroupClicked);
            toggleLabel.backgroundSprite = "OptionsDropbox";
            toggleLabel.padding = new RectOffset(10, 10, 10, 10);
            var uipanel = (UIPanel)newGroup.self;
            uipanel.Hide();
            uipanel.autoFitChildrenVertically = false;
            uipanel.clipChildren = true;
            uipanel.autoFitChildrenHorizontally = true;
            uipanel.backgroundSprite = "OptionsDropboxListboxHovered";
            uipanel.minimumSize = new Vector2(self.width - 20, 0);
            uipanel.maximumSize = uipanel.minimumSize;
            uipanel.padding = new RectOffset(10, 10, 10, 10);
            uipanel.size = new Vector2(newGroup.self.size.x, 0f);

            KlyteUtils.LimitWidth(toggleLabel, uipanel.width, true);

            parentPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);

            m_GroupStates.Add(title, new DecorationPropertiesPanel.GroupInfo
            {
                m_Folded = true,
                m_Container = newGroup.self,
                m_PropertyContainer = uipanel
            });
            return newGroup;
        }
        private Dictionary<string, DecorationPropertiesPanel.GroupInfo> m_GroupStates = null;
        private float m_DefaultGroupHeight = 0;
        #endregion
    }


    public delegate void OnColorChanged(Color val);

    public delegate void OnMultipleColorChanged(List<Color32> val);
}