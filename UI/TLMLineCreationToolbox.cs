using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.UI
{

    internal class TLMLineCreationToolbox : UICustomControl
    {

        #region Line Draw Button Click Impl

        public static void OnButtonClickedPre(ref TransportInfo __state) => __state = ToolManager.instance.m_properties?.CurrentTool is TransportTool tt ? tt.m_prefab : null;

        public static void OnButtonClickedPos(ref TransportInfo __state)
        {
            TransportInfo newPrefab = ToolManager.instance.m_properties?.CurrentTool is TransportTool tt ? tt.m_prefab : null;
            if (newPrefab != null && __state != newPrefab)
            {
                TLMController.Instance.LineCreationToolbox.SyncForm();
            }
        }
        #endregion

        private static FieldInfo tt_nextLineNum = typeof(TransportManager).GetField("m_lineNumber", RedirectorUtils.allFlags);
        private static readonly SavedBool m_showLineCreationToolBox = new SavedBool("K45_TLM_showLineToolbox", Settings.gameSettingsFile, true);

        public TransportInfo.TransportType CurrentType => TransportTool.m_prefab?.m_transportType ?? TransportInfo.TransportType.Bus;

        private UIHelperExtension uiHelper;

        private UIPanel m_bg;
        private TLMLineItemButtonControl m_toolboxToggleButton;
        private UIDropDown linePrefixDropDown;
        private UITextField lineNumberTxtBox;
        private UIPanel mainContainer;
        private UIPanel contentContainer;
        private UICheckBox prefixIncrementChk;

        private TransportInfo lastPrefab;

        private TransportTool TransportTool => ToolManager.instance.m_properties?.CurrentTool as TransportTool;


        private ushort NextLineNumber
        {
            get => tt_nextLineNum != null
                    ? ((ushort[])tt_nextLineNum.GetValue(Singleton<TransportManager>.instance))[(int)CurrentType]
                    : (ushort)0;
            set
            {
                if (tt_nextLineNum != null)
                {
                    ushort[] arr = ((ushort[])tt_nextLineNum.GetValue(Singleton<TransportManager>.instance));
                    arr[(int)CurrentType] = value;
                    tt_nextLineNum.SetValue(Singleton<TransportManager>.instance, arr);
                }
            }
        }

        public void SetVisible(bool value) => m_bg.isVisible = value;

        internal void Awake()
        {
            PublicTransportInfoViewPanel parent = GetComponent<PublicTransportInfoViewPanel>();
            KlyteMonoUtils.CreateUIElement(out m_bg, parent.GetComponentInChildren<UISlicedSprite>().transform);
            m_bg.name = "TLMLineCreationToolboxBG";
            m_bg.height = 0;
            m_bg.width = 0;
            m_bg.relativePosition = new Vector3(0, 0);
            m_bg.isInteractive = false;

            TLMLineItemButtonControl.EnsureTemplate();
            var toggleButton = m_bg.AttachUIComponent(UITemplateManager.GetAsGameObject(TLMLineItemButtonControl.LINE_ITEM_TEMPLATE)) as UIButton;
            toggleButton.relativePosition = new Vector3(parent.component.width - 40, 2);
            toggleButton.zOrder = 11;

            m_toolboxToggleButton = toggleButton.GetComponent<TLMLineItemButtonControl>();
            m_toolboxToggleButton.name = "TLMLineCreationToolboxToggle";
            m_toolboxToggleButton.Resize(36);
            m_toolboxToggleButton.OverrideClickEvent((x, y) =>
           {
               m_showLineCreationToolBox.value = !m_showLineCreationToolBox;
               UpdateToolBoxVisibility();
           });

            KlyteMonoUtils.CreateUIElement(out mainContainer, m_bg.transform);
            mainContainer.name = "TLMLineCreationToolbox";
            mainContainer.height = 130;
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
            linePrefixDropDown.eventSelectedIndexChanged += SetNextLinePrefix;
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
            lineNumberTxtBox.eventLostFocus += SetNextLineNumber;
            lineNumberTxtBox.zOrder = 10;
            lineNumberTxtBox.text = "0";
            lineNumberTxtBox.relativePosition = new Vector3(85f, 13f);


            prefixIncrementChk = uiHelper.AddCheckboxLocale("K45_TLM_AUTOINCREMENT_PREFIX", false, delegate (bool value)
             {
                 if (!alreadySyncing)
                 {
                     TransportSystemDefinition.From(TransportTool.m_prefab).GetConfig().IncrementPrefixOnNewLine = value;
                 }
             });
            prefixIncrementChk.relativePosition = new Vector3(5f, 50f);

            UpdateToolBoxVisibility();
            SetVisible(false);
        }

        private void UpdateToolBoxVisibility()
        {
            mainContainer.isVisible = m_showLineCreationToolBox;
            if (m_showLineCreationToolBox)
            {
                m_toolboxToggleButton?.GetComponent<UIButton>().Focus();
            }
            else
            {
                m_toolboxToggleButton?.GetComponent<UIButton>().Unfocus();
            }
        }

        private void SetNextLinePrefix(UIComponent component, int value) => SaveLineNumber();

        private void SetNextLineNumber(UIComponent component, UIFocusEventParameter eventParam) => SaveLineNumber();

        private void SaveLineNumber()
        {
            if (!alreadySyncing)
            {
                string value = "0" + lineNumberTxtBox.text;
                int valPrefixo = linePrefixDropDown.selectedIndex;

                var tsd = TransportSystemDefinition.From(TransportTool.m_prefab);
                TLMLineUtils.GetNamingRulesFromTSD(out NamingMode prefixo, out _, out _, out _, out _, out _, tsd);
                ushort num = ushort.Parse(value);
                if (prefixo != NamingMode.None)
                {
                    num = (ushort)((valPrefixo * 1000) + (num % 1000));
                }
                NextLineNumber = (ushort)(num - 1);
                IncrementNumber();
            }
        }

        public int GetCurrentPrefix() => ((NextLineNumber + 1) & 0xFFFF) / 1000;

        public void IncrementNumber()
        {
            //TLMUtils.doLog("Increment Toolbox num");
            var tsd = TransportSystemDefinition.From(TransportTool.m_prefab);
            int num = NextLineNumber;
            bool prefixIncrementVal = tsd.GetConfig().IncrementPrefixOnNewLine;
            //TLMUtils.doLog("prefixIncrement = " + prefixIncrementVal + "| num = " + num);
            while (((num + 1) & 0xFFFF) == 0 || TLMLineUtils.IsLineNumberAlredyInUse((num + 1) & 0xFFFF, tsd, 0))
            {
                if (!TLMPrefixesUtils.HasPrefix(TransportTool.m_prefab) || !prefixIncrementVal)
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
            NextLineNumber = (ushort)num;
            StartCoroutine(SyncFormAsync());
        }
        public void SyncForm() => StartCoroutine(SyncFormAsync());
        private bool alreadySyncing = false;
        private IEnumerator SyncFormAsync()
        {
            if (alreadySyncing)
            {
                yield break;
            }
            alreadySyncing = true;
            yield return 0;
            var tsd = TransportSystemDefinition.From(TransportTool.m_prefab);
            var config = tsd.GetConfig();
            if (TLMPrefixesUtils.HasPrefix(TransportTool.m_prefab))
            {
                linePrefixDropDown.isVisible = true;
                linePrefixDropDown.items = TLMPrefixesUtils.GetPrefixesOptions(tsd, false).ToArray();
                linePrefixDropDown.selectedIndex = GetCurrentPrefix();
                lineNumberTxtBox.text = GetCurrentNumber().ToString();
                lineNumberTxtBox.width = 90;
                lineNumberTxtBox.relativePosition = new Vector3(85f, 13);
                lineNumberTxtBox.maxLength = 3;
                prefixIncrementChk.isChecked = config.IncrementPrefixOnNewLine;
                prefixIncrementChk.isVisible = true;
            }
            else
            {
                linePrefixDropDown.isVisible = false;
                lineNumberTxtBox.text = GetCurrentNumber().ToString();
                lineNumberTxtBox.width = 170;
                lineNumberTxtBox.relativePosition = new Vector3(5f, 13f);
                lineNumberTxtBox.maxLength = 4;
                prefixIncrementChk.isVisible = false;
            }
            alreadySyncing = false;
            m_isDirty = true;
        }

        public void Update()
        {
            if (m_toolboxToggleButton.GetComponent<UIButton>().isVisible)
            {
                if (lastPrefab != TransportTool.m_prefab)
                {
                    lastPrefab = TransportTool.m_prefab;
                    StartCoroutine(SyncFormAsync());
                }
                else if (m_isDirty)
                {
                    StartCoroutine(UpdateUI(true));
                }
            }
        }

        private bool m_isDirty;

        public void MarkDirty() => m_isDirty = true;

        private IEnumerator UpdateUI(bool syncFromInput = false)
        {
            yield return 0;
            if (!m_isDirty)
            {
                yield break;
            }

            var tsd = TransportSystemDefinition.From(TransportTool.m_prefab);
            TLMLineUtils.GetNamingRulesFromTSD(out NamingMode prefixo, out Separator sep, out NamingMode sufixo, out NamingMode nonPrefix, out bool zeros, out bool invertPrefixSuffix, tsd);

            if (syncFromInput)
            {
                string value = "0" + lineNumberTxtBox.text;
                int valPrefixo = linePrefixDropDown.selectedIndex;

                ushort num = ushort.Parse(value);
                if (prefixo != NamingMode.None)
                {
                    num = (ushort)((valPrefixo * 1000) + (num % 1000));
                }
                if (NextLineNumber + 1 != num)
                {
                    NextLineNumber = (ushort)(num - 1);
                }
            }


            Color color = TLMBaseConfigXML.Instance.UseAutoColor
                ? TLMPrefixesUtils.CalculateAutoColor((ushort)(NextLineNumber + 1), tsd, true)
                : tsd.Color;
            lineNumberTxtBox.color = color;
            m_toolboxToggleButton.SetFixed(KlyteResourceLoader.GetDefaultSpriteNameFor(TLMPrefixesUtils.GetLineIcon((ushort)(NextLineNumber + 1), tsd), true), TLMPrefixesUtils.GetString(prefixo, sep, sufixo, nonPrefix, (NextLineNumber + 1) & 0xFFFF, zeros, invertPrefixSuffix), color);
            m_isDirty = false;
        }

        public int GetCurrentNumber() => TLMPrefixesUtils.HasPrefix(TransportTool.m_prefab)
            ? ((NextLineNumber + 1) & 0xFFFF) % 1000
            : (NextLineNumber + 1) & 0xFFFF;

    }
}
