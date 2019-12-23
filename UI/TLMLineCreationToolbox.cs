using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    internal class TLMLineCreationToolbox : MonoBehaviour
    {

        #region Line Draw Button Click Impl

        public static void OnButtonClickedPre(ref TransportInfo __state) => __state = Singleton<ToolController>.instance.gameObject.GetComponentInChildren<TransportTool>().m_prefab;

        public static void OnButtonClickedPos(ref TransportInfo __state)
        {
            if (__state != Singleton<ToolController>.instance.gameObject.GetComponentInChildren<TransportTool>().m_prefab)
            {
                TLMController.instance.LineCreationToolbox.syncForm();
            }
        }
        #endregion

        public TLMLineCreationToolbox()
        {
        }

        private static FieldInfo tt_nextLineNum = typeof(TransportManager).GetField("m_lineNumber", RedirectorUtils.allFlags);

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

        private UIDropDown linePrefixDropDown;
        private UITextField lineNumberTxtBox;
        private UILabel lineFormat;
        private UILabel lineNumber;
        private UIPanel mainContainer;
        private UICheckBox prefixIncrementChk;

        private TransportInfo lastPrefab;
        private TransportTool _transportTool;

        private TransportTool transportTool
        {
            get {
                if (_transportTool == null)
                {
                    _transportTool = Singleton<ToolController>.instance.gameObject.GetComponentInChildren<TransportTool>();
                }
                return _transportTool;
            }
        }


        private ushort nextLineNumber
        {
            get {
                if (tt_nextLineNum != null)
                {
                    return ((ushort[]) tt_nextLineNum.GetValue(Singleton<TransportManager>.instance))[(int) currentType];
                }
                return 0;
            }
            set {

                if (tt_nextLineNum != null)
                {
                    ushort[] arr = ((ushort[]) tt_nextLineNum.GetValue(Singleton<TransportManager>.instance));
                    arr[(int) currentType] = value;
                    tt_nextLineNum.SetValue(Singleton<TransportManager>.instance, arr);
                }
            }
        }

        public bool isVisible() => mainContainer.isVisible;
        public void setVisible(bool value) => mainContainer.isVisible = value;

        public void Awake()
        {
            createToolbox();
            setVisible(false);
        }

        private void createToolbox()
        {

            KlyteMonoUtils.CreateUIElement(out mainContainer, m_controller.TransformLinearMap);
            mainContainer.absolutePosition = new Vector3(2f, FindObjectOfType<UIView>().fixedHeight - 300f);
            mainContainer.name = "TLMLineCreationToolbox";
            mainContainer.height = 190;
            mainContainer.width = 180;
            mainContainer.backgroundSprite = "MenuPanel2";
            mainContainer.relativePosition = new Vector3(320f, 57f);

            uiHelper = new UIHelperExtension(mainContainer);

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
            KlyteMonoUtils.CreateDragHandle(title, mainContainer);


            var lpddgo = GameObject.Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject, mainContainer.transform);
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
            linePrefixDropDown.relativePosition = new Vector3(5f, 45f);
            linePrefixDropDown.normalBgSprite = "OptionsDropboxListbox";
            linePrefixDropDown.horizontalAlignment = UIHorizontalAlignment.Center;

            KlyteMonoUtils.CreateUIElement(out lineNumberTxtBox, mainContainer.transform);
            lineNumberTxtBox.autoSize = false;
            lineNumberTxtBox.relativePosition = new Vector3(85f, 45f);
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
            ;
            lineNumberTxtBox.zOrder = 10;
            lineNumberTxtBox.text = "0";

            KlyteMonoUtils.CreateUIElement(out lineFormat, mainContainer.transform);
            lineFormat.autoSize = false;
            lineFormat.width = 80;
            lineFormat.height = 80;
            lineFormat.color = new Color(1, 0, 0, 1);
            lineFormat.pivot = UIPivotPoint.MiddleLeft;
            lineFormat.textAlignment = UIHorizontalAlignment.Center;
            lineFormat.verticalAlignment = UIVerticalAlignment.Middle;
            lineFormat.name = "LineFormat";
            lineFormat.relativePosition = new Vector3(55f, 80f);
            KlyteMonoUtils.CreateDragHandle(lineFormat, mainContainer);

            KlyteMonoUtils.CreateUIElement(out lineNumber, lineFormat.transform);
            lineNumber.autoSize = false;
            lineNumber.width = lineFormat.width;
            lineNumber.pivot = UIPivotPoint.MiddleCenter;
            lineNumber.name = "LineNumber";
            lineNumber.width = 80;
            lineNumber.height = 80;
            lineNumber.relativePosition = new Vector3(-0.5f, 0.5f);
            lineNumber.autoHeight = false;
            lineNumber.textAlignment = UIHorizontalAlignment.Center;
            lineNumber.verticalAlignment = UIVerticalAlignment.Middle;

            prefixIncrementChk = uiHelper.AddCheckboxLocale("K45_TLM_AUTOINCREMENT_PREFIX", false, delegate (bool value)
             {
                 var tsd = TransportSystemDefinition.from(transportTool.m_prefab);
                 if (TransportLinesManagerMod.DebugMode)
                 {
                     TLMUtils.doLog("Type = " + tsd.toConfigIndex() + "|prop=" + (tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT) + "|valToSet = " + value);
                 }
                 TLMConfigWarehouse.SetCurrentConfigBool(tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT, value);
             });
            prefixIncrementChk.relativePosition = new Vector3(5f, 162.5f);

        }

        private void setNextLinePrefix(UIComponent component, int value) => saveLineNumber();

        private void setNextLineNumber(UIComponent component, UIFocusEventParameter eventParam) => saveLineNumber();

        private void saveLineNumber()
        {
            string value = "0" + lineNumberTxtBox.text;
            int valPrefixo = linePrefixDropDown.selectedIndex;

            var tsd = TransportSystemDefinition.from(transportTool.m_prefab);
            TLMLineUtils.GetNamingRulesFromTSD(out ModoNomenclatura prefixo, out Separador sep, out ModoNomenclatura sufixo, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, tsd);
            ushort num = ushort.Parse(value);
            if (prefixo != ModoNomenclatura.Nenhum)
            {
                num = (ushort) ((valPrefixo * 1000) + (num % 1000));
            }
            nextLineNumber = (ushort) (num - 1);
            incrementNumber();
        }

        public int getCurrentPrefix() => ((nextLineNumber + 1) & 0xFFFF) / 1000;

        public void incrementNumber()
        {
            //TLMUtils.doLog("Increment Toolbox num");
            var tsd = TransportSystemDefinition.from(transportTool.m_prefab);
            int num = nextLineNumber;
            bool prefixIncrementVal = TLMConfigWarehouse.GetCurrentConfigBool(tsd.toConfigIndex() | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT);
            //TLMUtils.doLog("prefixIncrement = " + prefixIncrementVal + "| num = " + num);
            while (((num + 1) & 0xFFFF) == 0 || TLMLineUtils.isNumberUsed((num + 1) & 0xFFFF, ref tsd, 0))
            {
                if (!TLMLineUtils.hasPrefix(transportTool.m_prefab) || !prefixIncrementVal)
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
            nextLineNumber = (ushort) num;
            syncForm();
        }

        private void syncForm()
        {

            var tsd = TransportSystemDefinition.from(transportTool.m_prefab);
            TLMConfigWarehouse.ConfigIndex configIdx = tsd.toConfigIndex();
            if (TLMLineUtils.hasPrefix(transportTool.m_prefab))
            {
                linePrefixDropDown.isVisible = true;
                linePrefixDropDown.items = TLMUtils.getPrefixesOptions(configIdx, false).ToArray();
                linePrefixDropDown.selectedIndex = getCurrentPrefix();
                lineNumberTxtBox.text = getCurrentNumber().ToString();
                lineNumberTxtBox.width = 90;
                lineNumberTxtBox.relativePosition = new Vector3(85f, 45f);
                lineNumberTxtBox.maxLength = 3;
                prefixIncrementChk.isChecked = TLMConfigWarehouse.GetCurrentConfigBool(configIdx | TLMConfigWarehouse.ConfigIndex.PREFIX_INCREMENT);
                prefixIncrementChk.isVisible = true;
            }
            else
            {
                linePrefixDropDown.isVisible = false;
                lineNumberTxtBox.text = getCurrentNumber().ToString();
                lineNumberTxtBox.width = 170;
                lineNumberTxtBox.relativePosition = new Vector3(5f, 45f);
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

            var tsd = TransportSystemDefinition.from(transportTool.m_prefab);
            TLMLineUtils.GetNamingRulesFromTSD(out ModoNomenclatura prefixo, out Separador sep, out ModoNomenclatura sufixo, out ModoNomenclatura nonPrefix, out bool zeros, out bool invertPrefixSuffix, tsd);

            if (syncFromInput)
            {
                string value = "0" + lineNumberTxtBox.text;
                int valPrefixo = linePrefixDropDown.selectedIndex;

                ushort num = ushort.Parse(value);
                if (prefixo != ModoNomenclatura.Nenhum)
                {
                    num = (ushort) ((valPrefixo * 1000) + (num % 1000));
                }
                if (nextLineNumber + 1 != num)
                {
                    nextLineNumber = (ushort) (num - 1);
                }
            }


            TLMConfigWarehouse.ConfigIndex configIdx = tsd.toConfigIndex();
            Color color;

            if (TLMConfigWarehouse.GetCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
            {
                color = TLMUtils.CalculateAutoColor((ushort) (nextLineNumber + 1), configIdx, ref tsd, true);
            }
            else
            {
                color = TLMConfigWarehouse.getColorForTransportType(configIdx);
            }

            lineNumberTxtBox.color = color;
            lineFormat.color = color;
            lineFormat.backgroundSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(TLMUtils.GetLineIcon((ushort) (nextLineNumber + 1), configIdx, ref tsd));
            lineNumber.text = TLMUtils.getString(prefixo, sep, sufixo, nonPrefix, (nextLineNumber + 1) & 0xFFFF, zeros, invertPrefixSuffix);
            lineNumber.textColor = KlyteMonoUtils.ContrastColor(color);
            int txtLen = lineNumber.text.Length;
            switch (txtLen)
            {
                case 1:
                    lineNumber.textScale = 4;
                    break;
                case 2:
                    lineNumber.textScale = 3;
                    break;
                case 3:
                    lineNumber.textScale = 2.25f;
                    break;
                case 4:
                    lineNumber.textScale = 1.75f;
                    break;
                case 5:
                    lineNumber.textScale = 1.5f;
                    break;
                case 6:
                    lineNumber.textScale = 1.35f;
                    break;
                case 7:
                    lineNumber.textScale = 1.2f;
                    break;
                case 8:
                    lineNumber.textScale = 1.1f;
                    break;
                default:
                    lineNumber.textScale = 1f;
                    break;
            }

        }
        public int getCurrentNumber()
        {

            if (TLMLineUtils.hasPrefix(transportTool.m_prefab))
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
