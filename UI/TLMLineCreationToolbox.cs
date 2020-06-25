using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    internal class TLMLineCreationToolbox
    {

        #region Line Draw Button Click Impl

        public static void OnButtonClickedPre(ref TransportInfo __state) => __state = ToolManager.instance.m_properties?.CurrentTool is TransportTool tt ? tt.m_prefab : null;

        public static void OnButtonClickedPos(ref TransportInfo __state)
        {
            TransportInfo newPrefab = ToolManager.instance.m_properties?.CurrentTool is TransportTool tt ? tt.m_prefab : null;
            if (newPrefab != null && __state != newPrefab)
            {
                TLMController.instance.LineCreationToolbox.syncForm();
            }
        }
        #endregion

        public TLMLineCreationToolbox()
        {
        }

        private static FieldInfo tt_nextLineNum = typeof(TransportManager).GetField("m_lineNumber", RedirectorUtils.allFlags);
        private static readonly SavedBool m_showLineCreationToolBox = new SavedBool("K45_TLM_showLineToolbox", Settings.gameSettingsFile, true);

        public TransportInfo.TransportType currentType
        {
            get {

                if (transportTool.m_prefab != null)
                {
                    return transportTool.m_prefab.m_transportType;
                }
                else
                {
                    return TransportInfo.TransportType.Bus;
                }
            }
        }
        private TLMController m_controller => TLMController.instance;

        private UIHelperExtension uiHelper;

        private UIPanel m_bg;
        private UIButton m_toolboxToggleButton;
        private UIDropDown linePrefixDropDown;
        private UITextField lineNumberTxtBox;
        private UILabel lineNumber;
        private UIPanel mainContainer;
        private UIPanel contentContainer;
        private UICheckBox prefixIncrementChk;

        private TransportInfo lastPrefab;
        private TransportTool _transportTool;

        private TransportTool transportTool => ToolManager.instance.m_properties?.CurrentTool as TransportTool;


        private ushort nextLineNumber
        {
            get {
                if (tt_nextLineNum != null)
                {
                    return ((ushort[])tt_nextLineNum.GetValue(Singleton<TransportManager>.instance))[(int)currentType];
                }
                return 0;
            }
            set {

                if (tt_nextLineNum != null)
                {
                    ushort[] arr = ((ushort[])tt_nextLineNum.GetValue(Singleton<TransportManager>.instance));
                    arr[(int)currentType] = value;
                    tt_nextLineNum.SetValue(Singleton<TransportManager>.instance, arr);
                }
            }
        }

        public void setVisible(bool value) => m_bg.isVisible = value;

        internal TLMLineCreationToolbox(PublicTransportInfoViewPanel parent)
        {
            KlyteMonoUtils.CreateUIElement(out m_bg, parent.GetComponentInChildren<UISlicedSprite>().transform);
            m_bg.name = "TLMLineCreationToolboxBG";
            m_bg.height = 0;
            m_bg.width = 0;
            m_bg.relativePosition = new Vector3(0, 0);
            m_bg.isInteractive = false;

            KlyteMonoUtils.CreateUIElement(out m_toolboxToggleButton, m_bg.transform);
            m_toolboxToggleButton.disabledTextColor = new Color32(128, 128, 128, byte.MaxValue);
            KlyteMonoUtils.InitButtonFull(m_toolboxToggleButton, false, "OptionBase");
            m_toolboxToggleButton.size = new Vector2(36, 36);
            m_toolboxToggleButton.name = "TLMLineCreationToolboxToggle";
            m_toolboxToggleButton.zOrder = 11;
            m_toolboxToggleButton.textScale = 1;
            m_toolboxToggleButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            m_toolboxToggleButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
            m_toolboxToggleButton.relativePosition = new Vector3(parent.component.width - 40, 2);

            m_toolboxToggleButton.processMarkup = true;
            m_toolboxToggleButton.eventClicked += (x, y) =>
            {
                m_showLineCreationToolBox.value = !m_showLineCreationToolBox;
                UpdateToolBoxVisibility();
            };

            KlyteMonoUtils.CreateUIElement(out mainContainer, m_bg.transform);
            mainContainer.name = "TLMLineCreationToolbox";
            mainContainer.height = 210;
            mainContainer.width = 180;
            mainContainer.backgroundSprite = "MenuPanel2";
            mainContainer.relativePosition = new Vector3(parent.component.width, 0);


            KlyteMonoUtils.CreateUIElement(out UILabel title, mainContainer.transform);
            title.autoSize = false;
            title.width = mainContainer.width;
            title.height = 30;
            title.color = new Color(1, 0, 0, 1);
            title.pivot = UIPivotPoint.MiddleLeft;
            title.textAlignment = UIHorizontalAlignment.Center;
            title.verticalAlignment = UIVerticalAlignment.Middle;
            title.name = "Title";
            title.relativePosition = new Vector3(0, 5);
            title.localeID = "K45_TLM_PREFIX_SELECTOR_WIN_TITLE";

            KlyteMonoUtils.CreateUIElement(out contentContainer, mainContainer.transform);
            contentContainer.relativePosition = new Vector3(2f, 32f);
            contentContainer.name = "TLMLineCreationToolboxContent";
            contentContainer.size = new Vector3(mainContainer.width - 4, mainContainer.height - 34);

            uiHelper = new UIHelperExtension(contentContainer);


            var lpddgo = GameObject.Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject, contentContainer.transform);
            linePrefixDropDown = lpddgo.GetComponent<UIDropDown>();
            linePrefixDropDown.isLocalized = false;
            linePrefixDropDown.autoSize = false;
            linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;
            linePrefixDropDown.text = "";
            linePrefixDropDown.width = 75;
            linePrefixDropDown.height = 30;
            linePrefixDropDown.name = "LinePrefixDropDown";
            linePrefixDropDown.textScale = 1.6f;
            linePrefixDropDown.itemHeight = 35;
            linePrefixDropDown.itemPadding = new RectOffset(2, 2, 2, 2);
            linePrefixDropDown.textFieldPadding = new RectOffset(2, 2, 2, 2);
            linePrefixDropDown.eventSelectedIndexChanged += setNextLinePrefix;
            linePrefixDropDown.relativePosition = new Vector3(5f, 13f);
            linePrefixDropDown.normalBgSprite = "OptionsDropboxListbox";
            linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out lineNumberTxtBox, contentContainer.transform);
            lineNumberTxtBox.autoSize = false;
            lineNumberTxtBox.horizontalAlignment = UIHorizontalAlignment.Center;
            lineNumberTxtBox.text = "";
            lineNumberTxtBox.width = 90;
            lineNumberTxtBox.height = 30;
            lineNumberTxtBox.name = "LineNumberLabel";
            lineNumberTxtBox.normalBgSprite = "EmptySprite";
            lineNumberTxtBox.textScale = 1.6f;
            lineNumberTxtBox.padding = new RectOffset(0, 0, 0, 0);
            lineNumberTxtBox.color = new Color(0, 0, 0, 1);
            KlyteMonoUtils.UiTextFieldDefaults(lineNumberTxtBox);
            lineNumberTxtBox.numericalOnly = true;
            lineNumberTxtBox.maxLength = 4;
            lineNumberTxtBox.eventLostFocus += setNextLineNumber;
            lineNumberTxtBox.zOrder = 10;
            lineNumberTxtBox.text = "0";
            lineNumberTxtBox.relativePosition = new Vector3(85f, 13f);

            KlyteMonoUtils.CreateUIElement(out lineNumber, contentContainer.transform);
            lineNumber.autoSize = false;
            lineNumber.pivot = UIPivotPoint.MiddleCenter;
            lineNumber.name = "LineNumber";
            lineNumber.width = 150;
            lineNumber.processMarkup = true;
            lineNumber.textScale = 2.5f;
            lineNumber.isInteractive = false;
            lineNumber.height = 80;
            lineNumber.relativePosition = new Vector3(20f, 50);
            lineNumber.autoHeight = false;
            lineNumber.textAlignment = UIHorizontalAlignment.Center;
            lineNumber.verticalAlignment = UIVerticalAlignment.Middle;

            prefixIncrementChk = uiHelper.AddCheckboxLocale("K45_TLM_AUTOINCREMENT_PREFIX", false, delegate (bool value)
             {
                 var tsd = TransportSystemDefinition.From(transportTool.m_prefab);
                 if (TransportLinesManagerMod.DebugMode)
                 {
                     LogUtils.DoLog("Type = " + tsd.ToConfigIndex() + "|prop=" + (tsd.ToConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT) + "|valToSet = " + value);
                 }
                 TLMConfigWarehouse.SetCurrentConfigBool(tsd.ToConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT, value);
             });
            prefixIncrementChk.relativePosition = new Vector3(5f, 130f);

            uiHelper.AddCheckboxLocale("K45_TLM_SHOW_LINEAR_MAP", TLMController.LinearMapWhileCreatingLineVisibility, delegate (bool value)
            {
                TLMController.LinearMapWhileCreatingLineVisibility = value;
            }).relativePosition = new Vector3(5f, 153f);

            UpdateToolBoxVisibility();
            setVisible(false);
        }

        private void UpdateToolBoxVisibility()
        {
            mainContainer.isVisible = m_showLineCreationToolBox;
            if (m_showLineCreationToolBox)
            {
                m_toolboxToggleButton?.Focus();
            }
            else
            {
                m_toolboxToggleButton?.Unfocus();
            }
        }

        private void setNextLinePrefix(UIComponent component, int value) => saveLineNumber();

        private void setNextLineNumber(UIComponent component, UIFocusEventParameter eventParam) => saveLineNumber();

        private void saveLineNumber()
        {
            string value = "0" + lineNumberTxtBox.text;
            int valPrefixo = linePrefixDropDown.selectedIndex;

            var tsd = TransportSystemDefinition.From(transportTool.m_prefab);
            TLMLineUtils.GetNamingRulesFromTSD(out ModoNomenclatura prefixo, out Separador sep, out ModoNomenclatura sufixo, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, ref tsd);
            ushort num = ushort.Parse(value);
            if (prefixo != ModoNomenclatura.Nenhum)
            {
                num = (ushort)((valPrefixo * 1000) + (num % 1000));
            }
            nextLineNumber = (ushort)(num - 1);
            incrementNumber();
        }

        public int getCurrentPrefix() => ((nextLineNumber + 1) & 0xFFFF) / 1000;

        public void incrementNumber()
        {
            //TLMUtils.doLog("Increment Toolbox num");
            var tsd = TransportSystemDefinition.From(transportTool.m_prefab);
            int num = nextLineNumber;
            bool prefixIncrementVal = TLMConfigWarehouse.GetCurrentConfigBool(tsd.ToConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT);
            //TLMUtils.doLog("prefixIncrement = " + prefixIncrementVal + "| num = " + num);
            while (((num + 1) & 0xFFFF) == 0 || TLMLineUtils.IsLineNumberAlredyInUse((num + 1) & 0xFFFF, ref tsd, 0))
            {
                if (!TLMPrefixesUtils.HasPrefix(transportTool.m_prefab) || !prefixIncrementVal)
                {
                    num++;
                }
                else
                {
                    num++;
                    num /= 1000;
                    num++;
                    num &= 0x4F;
                    num *= 1000;
                    num--;

                }
            }
            nextLineNumber = (ushort)num;
            syncForm();
        }

        public void syncForm()
        {

            var tsd = TransportSystemDefinition.From(transportTool.m_prefab);
            var configIdx = tsd.ToConfigIndex();
            if (TLMPrefixesUtils.HasPrefix(transportTool.m_prefab))
            {
                linePrefixDropDown.isVisible = true;
                linePrefixDropDown.items = TLMPrefixesUtils.GetPrefixesOptions(configIdx, false).ToArray();
                linePrefixDropDown.selectedIndex = getCurrentPrefix();
                lineNumberTxtBox.text = getCurrentNumber().ToString();
                lineNumberTxtBox.width = 90;
                lineNumberTxtBox.relativePosition = new Vector3(85f, 13);
                lineNumberTxtBox.maxLength = 3;
                prefixIncrementChk.isChecked = TLMConfigWarehouse.GetCurrentConfigBool(configIdx | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT);
                prefixIncrementChk.isVisible = true;
            }
            else
            {
                linePrefixDropDown.isVisible = false;
                lineNumberTxtBox.text = getCurrentNumber().ToString();
                lineNumberTxtBox.width = 170;
                lineNumberTxtBox.relativePosition = new Vector3(5f, 13f);
                lineNumberTxtBox.maxLength = 4;
                prefixIncrementChk.isVisible = false;
            }
            updateUI();
        }

        public void Update()
        {
            if (mainContainer.isVisible)
            {
                if (lastPrefab != transportTool.m_prefab)
                {
                    lastPrefab = transportTool.m_prefab;
                    syncForm();
                }
                else
                {
                    updateUI(true);
                }
            }
        }

        private void updateUI(bool syncFromInput = false)
        {

            var tsd = TransportSystemDefinition.From(transportTool.m_prefab);
            TLMLineUtils.GetNamingRulesFromTSD(out ModoNomenclatura prefixo, out Separador sep, out ModoNomenclatura sufixo, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, ref tsd);

            if (syncFromInput)
            {
                string value = "0" + lineNumberTxtBox.text;
                int valPrefixo = linePrefixDropDown.selectedIndex;

                ushort num = ushort.Parse(value);
                if (prefixo != ModoNomenclatura.Nenhum)
                {
                    num = (ushort)((valPrefixo * 1000) + (num % 1000));
                }
                if (nextLineNumber + 1 != num)
                {
                    nextLineNumber = (ushort)(num - 1);
                }
            }


            var configIdx = tsd.ToConfigIndex();
            Color color;

            if (TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
            {
                color = TLMPrefixesUtils.CalculateAutoColor((ushort)(nextLineNumber + 1), configIdx, ref tsd, true);
            }
            else
            {
                color = TLMConfigWarehouse.getColorForTransportType(configIdx);
            }

            lineNumberTxtBox.color = color;
            string lineStr = TLMLineUtils.GetIconString(KlyteResourceLoader.GetDefaultSpriteNameFor(TLMPrefixesUtils.GetLineIcon((ushort)(nextLineNumber + 1), configIdx, ref tsd), true), color, TLMPrefixesUtils.GetString(prefixo, sep, sufixo, nonPrefix, (nextLineNumber + 1) & 0xFFFF, zeros, invertPrefixSuffix));
            m_toolboxToggleButton.text = lineStr;
            lineNumber.text = lineStr;
        }
         
        public int getCurrentNumber()
        {

            if (TLMPrefixesUtils.HasPrefix(transportTool.m_prefab))
            {
                return ((nextLineNumber + 1) & 0xFFFF) % 1000;
            }
            else
            {
                return (nextLineNumber + 1) & 0xFFFF;
            }
        }

    }
}
