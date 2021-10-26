using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Cache;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.OptionsMenu.Tabs;
using Klyte.TransportLinesManager.Utils;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    public class TLMRegionalMainTab : UICustomControl, IUVMPTWIPChild
    {
        public UIPanel MainPanel { get; private set; }
        private UIHelperExtension m_helper;

        private UITextField m_lineCode;
        private UIDropDown m_formatDD;
        private UIColorField m_prefixColor;

        private bool m_isLoading;
        private InnerBuildingLine GetLineData() => GetLineID(out ushort lineId) ? TransportLinesManagerMod.Controller.BuildingLines[lineId] : null;
        internal static bool GetLineID(out ushort lineId) => UVMPublicTransportWorldInfoPanel.GetLineID(out lineId, out bool fromBuilding) && fromBuilding;

        public void Awake()
        {
            MainPanel = GetComponent<UIPanel>();
            MainPanel.relativePosition = new Vector3(510f, 0.0f);
            MainPanel.width = 350;
            MainPanel.height = GetComponentInParent<UIComponent>().height;
            MainPanel.zOrder = 50;
            MainPanel.color = new Color32(255, 255, 255, 255);
            MainPanel.name = "RegionalLineDetailsWindow";
            MainPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            MainPanel.autoLayout = true;
            MainPanel.autoLayoutDirection = LayoutDirection.Vertical;

            m_helper = new UIHelperExtension(MainPanel);

            LogUtils.DoLog("Name");
            m_lineCode = CreateMiniTextField("K45_TLM_REGIONALLINE_CODE", OnRegionalLineCodeChanged);

            LogUtils.DoLog("ColorSel");
            CreateColorSelector();

            LogUtils.DoLog("Format");
            m_formatDD = CreateMiniDropdown("K45_TLM_ICON", SetFormatPrefix, TLMLineIconExtension.GetDropDownOptions(Locale.Get("K45_TLM_LINE_ICON_ENUM_TT_DEFAULT")));

        }

        private UIDropDown CreateMiniDropdown(string localeId, OnDropdownSelectionChanged onValueChanged, string[] values)
        {
            UIDropDown ddObj = UIHelperExtension.CloneBasicDropDownLocalized(localeId, values, onValueChanged, 0, MainPanel, out UILabel label, out UIPanel container);
            container.autoFitChildrenHorizontally = false;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayout = true;
            container.autoFitChildrenHorizontally = true;
            container.autoFitChildrenVertically = true;
            ReflectionUtils.GetEventField(typeof(UIDropDown), "eventMouseWheel")?.SetValue(ddObj, null);
            ddObj.isLocalized = false;
            ddObj.autoSize = false;
            ddObj.horizontalAlignment = UIHorizontalAlignment.Center;
            ddObj.itemPadding = new RectOffset(2, 2, 6, 6);
            ddObj.textFieldPadding = new RectOffset(4, 40, 4, 4);
            ddObj.name = localeId;
            ddObj.size = new Vector3(240, 22);
            ddObj.textScale = 0.8f;
            ddObj.listPosition = UIDropDown.PopupListPosition.Automatic;
            ddObj.horizontalAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.LimitWidthAndBox(label, 130);
            label.textScale = 1;
            label.padding.top = 4;
            label.position = Vector3.zero;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Left;

            return ddObj;
        }

        private UITextField CreateMiniTextField(string localeId, OnTextSubmitted onValueChanged)
        {
            UITextField ddObj = UIHelperExtension.AddTextfield(MainPanel, localeId, "", out UILabel label, out UIPanel container);
            container.autoFitChildrenHorizontally = false;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayout = true;
            container.autoFitChildrenHorizontally = true;
            container.autoFitChildrenVertically = true;

            ddObj.isLocalized = false;
            ddObj.autoSize = false;
            ddObj.eventTextSubmitted += (x, y) => onValueChanged(y);
            ddObj.name = localeId;
            ddObj.size = new Vector3(240, 22);
            ddObj.textScale = 1;


            KlyteMonoUtils.LimitWidthAndBox(label, 130);
            label.textScale = 1;
            label.padding.top = 4;
            label.position = Vector3.zero;
            label.isLocalized = true;
            label.localeID = localeId;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Left;

            return ddObj;
        }

        private void OnRegionalLineCodeChanged(string val)
        {
            if (!m_isLoading)
            {
                GetLineData().LineDataObject.Identifier = val.Replace("\\n", "\n");
                UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
            }
        }

        private void SetFormatPrefix(int value)
        {
            if (!m_isLoading)
            {
                GetLineData().LineDataObject.LineBgSprite = (LineIconSpriteNames)Enum.Parse(typeof(LineIconSpriteNames), value.ToString());
                UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
            }
        }

        private void CreateColorSelector()
        {
            m_prefixColor = m_helper.AddColorPicker("A", Color.clear, OnChangePrefixColor, out UILabel lbl, out UIPanel container);

            KlyteMonoUtils.LimitWidthAndBox(lbl, 260, true);
            lbl.isLocalized = true;
            lbl.localeID = "K45_TLM_REGIONALLINE_COLOR_LABEL";
            lbl.verticalAlignment = UIVerticalAlignment.Middle;
            lbl.font = UIHelperExtension.defaultFontCheckbox;
            lbl.textScale = 1;
        }

        private void OnChangePrefixColor(Color selectedColor)
        {
            if (!m_isLoading)
            {
                GetLineData().LineDataObject.LineColor = selectedColor;

                UVMPublicTransportWorldInfoPanel.MarkDirty(GetType());
            }
        }

        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }
            m_isLoading = true;
            m_prefixColor.selectedColor = GetLineData().LineDataObject.LineColor;
            m_lineCode.text = GetLineData().LineDataObject.Identifier.Replace("\n","\\n");
            m_formatDD.selectedIndex = Math.Max(0, (int)GetLineData().LineDataObject.LineBgSprite);

            m_isLoading = false;

        }
        public void UpdateBindings() { }
        public void OnEnable() { }
        public void OnDisable() { }
        public void OnGotFocus() { }
        public bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetLineID(out _, out bool fromBuilding) && fromBuilding && GetLineData()?.LineDataObject != null;
        public void Hide() => MainPanel.isVisible = false;
    }
}