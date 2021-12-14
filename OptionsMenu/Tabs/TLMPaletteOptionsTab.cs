using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMPaletteOptionsTab : UICustomControl, ITLMConfigOptionsTab
    {
        private const string COLOR_SELECTOR_TEMPLATE = "K45_TLM_PaletteColorListSelectorTemplate";

        private UIComponent parent;
        private UIDropDown m_paletteSelect;
        private UITemplateList<UIPanel> m_colorFieldTemplateListColors;
        private UIButton m_addColor;
        private UIScrollablePanel m_colorListScroll;
        private bool canEdit;

        public static event OnPalettesChanged OnPaletteReloaded;

        public void ReloadData()
        {
            string idxSel = m_paletteSelect.selectedValue;
            m_paletteSelect.items = TLMAutoColorPaletteContainer.PaletteListForEditing;
            m_paletteSelect.selectedValue = idxSel;

        }

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            var group6 = new UIHelperExtension(parent.GetComponentInChildren<UIScrollablePanel>());
            ((UIScrollablePanel)group6.Self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIScrollablePanel)group6.Self).wrapLayout = true;
            ((UIScrollablePanel)group6.Self).width = 730;

            group6.AddLabel(Locale.Get("K45_TLM_CUSTOM_PALETTE_CONFIG"));
            group6.AddSpace(15);

            FileInfo fiPalette = FileUtils.EnsureFolderCreation(TLMController.PalettesFolder);

            group6.AddLabel(Locale.Get("K45_TLM_PALETTE_FOLDER_LABEL") + ":");
            var namesFilesButton = ((UIButton)group6.AddButton("/", () => ColossalFramework.Utils.OpenInFileBrowser(fiPalette.FullName)));
            namesFilesButton.textColor = Color.yellow;
            KlyteMonoUtils.LimitWidthAndBox(namesFilesButton, 710);
            namesFilesButton.text = fiPalette.FullName + Path.DirectorySeparatorChar;
            ((UIButton)group6.AddButton(Locale.Get("K45_TLM_RELOAD_PALETTES"), delegate ()
            {
                TLMAutoColorPaletteContainer.Reload();
                ReloadData();
                OnPaletteReloaded?.Invoke();
            })).width = 710;

            UIPanel m_listColorContainer = null;
            m_paletteSelect = group6.AddDropdown(Locale.Get("K45_TLM_PALETTE_VIEW"), TLMAutoColorPaletteContainer.PaletteListForEditing, 0, delegate (int sel)
            {
                if (sel <= 0 || sel >= TLMAutoColorPaletteContainer.PaletteListForEditing.Length)
                {
                    m_listColorContainer?.Disable();
                    m_colorFieldTemplateListColors?.SetItemCount(0);
                }
                else
                {
                    m_listColorContainer?.Enable();
                    UpdateColorList(TLMAutoColorPaletteContainer.GetColors(TLMAutoColorPaletteContainer.PaletteListForEditing[sel]));
                }
            }) as UIDropDown;
            m_paletteSelect.GetComponentInParent<UIPanel>().width = 720;
            m_paletteSelect.GetComponentInParent<UIPanel>().autoLayoutDirection = LayoutDirection.Horizontal;
            m_paletteSelect.GetComponentInParent<UIPanel>().wrapLayout = true;
            m_paletteSelect.width = 710;

            KlyteMonoUtils.CreateUIElement(out m_listColorContainer, group6.Self.transform, "listColors", new UnityEngine.Vector4(0, 0, group6.Self.width, group6.Self.height - 250));
            KlyteMonoUtils.CreateScrollPanel(m_listColorContainer, out m_colorListScroll, out _, m_listColorContainer.width - 20, m_listColorContainer.height);
            m_colorListScroll.backgroundSprite = "OptionsScrollbarTrack";
            m_colorListScroll.autoLayout = true;
            m_colorListScroll.autoLayoutDirection = LayoutDirection.Horizontal;
            m_colorListScroll.wrapLayout = true;
        }
        public void Start()
        {
            CreateTemplateColorItem();
            m_colorFieldTemplateListColors = new UITemplateList<UIPanel>(m_colorListScroll, COLOR_SELECTOR_TEMPLATE);
            if (canEdit)
            {
                KlyteMonoUtils.InitCircledButton(m_colorListScroll, out m_addColor, CommonsSpriteNames.K45_Plus, (x, y) => AddColor(), "", 36);
                DefaultEditorUILib.AddButtonInEditorRow(m_paletteSelect, CommonsSpriteNames.K45_Plus, () => AddPalette(), "K45_TLM_ADDPALETTE", true, (int)m_paletteSelect.height);
            }
        }

        private void AddPalette(string errorMsg = null, string oldVal = null) => K45DialogControl.ShowModalPromptText(new K45DialogControl.BindProperties
        {
            title = Locale.Get("K45_TLM_ADDPALETTE"),
            message = (errorMsg + "\n").TrimToNull() + Locale.Get("K45_TLM_ADDPALETTE_PROMPTNAME"),
            defaultTextFieldContent = oldVal,
            showButton1 = true,
            textButton1 = Locale.Get("EXCEPTION_OK"),
            textButton2 = Locale.Get("CANCEL"),
            showButton2 = true
        }, (x, val) =>
         {
             if (x == 1)
             {
                 if (val == TLMAutoColorPaletteContainer.PALETTE_RANDOM || !(TLMAutoColorPaletteContainer.GetPalette(val) is null))
                 {
                     AddPalette(Locale.Get("K45_TLM_ADDPALETTE_ERROR_PALETTEALREADYEXISTS"), val);
                 }
                 else if (val.IsNullOrWhiteSpace())
                 {
                     AddPalette(Locale.Get("K45_TLM_ADDPALETTE_ERROR_INVALIDNAME"), val);
                 }
                 TLMAutoColorPaletteContainer.AddPalette(val);
                 TLMAutoColorPaletteContainer.Save(val);
                 ReloadData();
                 m_paletteSelect.selectedValue = val;
             }
             return true;
         });

        private void CreateTemplateColorItem()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(COLOR_SELECTOR_TEMPLATE))
            {
                UITemplateUtils.GetTemplateDict().Remove(COLOR_SELECTOR_TEMPLATE);
            }
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(36, 36);
            panel.autoLayout = true;
            panel.wrapLayout = false;
            panel.padding = new RectOffset(4, 4, 4, 4);
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            canEdit = KlyteMonoUtils.EnsureColorFieldTemplate();

            KlyteMonoUtils.CreateUIElement(out UIColorField colorField, panel.transform);
            KlyteMonoUtils.InitColorField(colorField, 36);
            var triggerButton = UIHelperExtension.AddLabel(colorField, "0", 36, out _);
            triggerButton.autoSize = false;
            triggerButton.size = colorField.size;
            triggerButton.backgroundSprite = "ColorPickerOutline";
            triggerButton.outlineColor = Color.black;
            triggerButton.outlineSize = 1;
            triggerButton.textScale = 1;
            triggerButton.textAlignment = UIHorizontalAlignment.Center;
            triggerButton.verticalAlignment = UIVerticalAlignment.Middle;
            colorField.triggerButton = triggerButton;
            triggerButton.size = colorField.size;
            triggerButton.relativePosition = default;
            colorField.normalFgSprite = "ColorPickerColor";
            colorField.pickerPosition = UIColorField.ColorPickerPosition.LeftBelow;

            if (!canEdit)
            {
                triggerButton.Disable();
            }

            UITemplateUtils.GetTemplateDict()[COLOR_SELECTOR_TEMPLATE] = panel;
        }
        private void AddColor()
        {
            if (canEdit && GetPaletteName(out string paletteName))
            {
                var palette = TLMAutoColorPaletteContainer.GetPalette(paletteName);
                palette.Add();
                StartCoroutine(SavePalette(paletteName));
            }
        }

        private void UpdateColorList(List<Color32> colors)
        {
            UIPanel[] colorPickers = m_colorFieldTemplateListColors.SetItemCount(colors.Count);

            for (int i = 0; i < colors.Count; i++)
            {
                UIColorField colorField = colorPickers[i].GetComponentInChildren<UIColorField>();
                if (canEdit && colorField.objectUserData == null)
                {
                    colorField.colorPicker = KlyteMonoUtils.GetDefaultPicker();
                    colorField.eventSelectedColorReleased += (x, y) =>
                    {
                        if (GetPaletteName(out string paletteName))
                        {
                            var palette = TLMAutoColorPaletteContainer.GetPalette(paletteName);
                            var selColor = ((UIColorField)x).selectedColor;
                            palette[x.parent.zOrder] = selColor;
                            if (selColor == default)
                            {
                                ((UIColorField)x).isVisible = false;
                                ((UIColorField)x).OnDisable();
                            }
                            StartCoroutine(SavePalette(paletteName));
                        }
                    };
                    colorField.gameObject.AddComponent<UIColorFieldExtension>();
                    colorField.objectUserData = true;
                }
                (colorField.triggerButton as UILabel).text = $"{i.ToString("0")}";
                (colorField.triggerButton as UILabel).textColor = KlyteMonoUtils.ContrastColor(colors[i]);
                (colorField.triggerButton as UILabel).disabledTextColor = KlyteMonoUtils.ContrastColor(colors[i]);
                colorField.selectedColor = colors[i];
                colorField.isVisible = true;
            }
            if (canEdit)
            {
                m_addColor.zOrder = 99999999;
            }
        }

        private bool GetPaletteName(out string paletteName)
        {
            var sel = m_paletteSelect.selectedIndex;
            if (sel <= 0 || sel > TLMAutoColorPaletteContainer.PaletteListForEditing.Length)
            {
                paletteName = null;
                return false;
            }
            paletteName = TLMAutoColorPaletteContainer.PaletteListForEditing[sel];
            return true;
        }

        public delegate void OnPalettesChanged();

        private int framesCooldownSave = 0;
        private IEnumerator SavePalette(string paletteName)
        {
            if (!canEdit)
            {
                yield break;
            }

            if (framesCooldownSave > 0)
            {
                framesCooldownSave = 3;
                yield break;
            }
            framesCooldownSave = 3;
            do
            {
                yield return null;
                framesCooldownSave--;
            } while (framesCooldownSave > 0);

            TLMAutoColorPaletteContainer.Save(paletteName);
            UpdateColorList(TLMAutoColorPaletteContainer.GetColors(paletteName));
        }
    }
}
