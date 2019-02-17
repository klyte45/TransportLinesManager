using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMPaletteOptionsTab : UICustomControl
    {

        UIComponent parent;
        private UIDropDown editorSelector;

        public static event OnPalettesChanged onPaletteReloaded;

        private void Awake()
        {
            parent = GetComponentInParent<UIComponent>();
            UIHelperExtension group6 = new UIHelperExtension(parent);
            ((UIPanel)group6.self).autoLayoutDirection = LayoutDirection.Horizontal;
            ((UIPanel)group6.self).wrapLayout = true;
            ((UIPanel)group6.self).width = 730;

            group6.AddLabel(Locale.Get("TLM_CUSTOM_PALETTE_CONFIG"));
            group6.AddSpace(15);

            var fiPalette = TLMUtils.EnsureFolderCreation(TLMSingleton.palettesFolder);

            group6.AddLabel(Locale.Get("TLM_PALETTE_FOLDER_LABEL") + ":");
            var namesFilesButton = ((UIButton)group6.AddButton("/", () => { ColossalFramework.Utils.OpenInFileBrowser(fiPalette.FullName); }));
            namesFilesButton.textColor = Color.yellow;
            TLMUtils.LimitWidth(namesFilesButton, 710);
            namesFilesButton.text = fiPalette.FullName + Path.DirectorySeparatorChar;
            ((UIButton)group6.AddButton(Locale.Get("TLM_RELOAD_PALETTES"), delegate ()
            {
                TLMAutoColorPalettes.Reload();
                string idxSel = editorSelector.selectedValue;
                editorSelector.items = TLMAutoColorPalettes.paletteListForEditing;
                editorSelector.selectedIndex = TLMAutoColorPalettes.paletteListForEditing.ToList().IndexOf(idxSel);
                TLMConfigOptions.instance.updateDropDowns();
                onPaletteReloaded?.Invoke();
            })).width = 710;

            NumberedColorList colorList = null;
            editorSelector = group6.AddDropdown(Locale.Get("TLM_PALETTE_VIEW"), TLMAutoColorPalettes.paletteListForEditing, 0, delegate (int sel)
            {
                if (sel <= 0 || sel >= TLMAutoColorPalettes.paletteListForEditing.Length)
                {
                    colorList.Disable();
                }
                else
                {
                    colorList.colorList = TLMAutoColorPalettes.getColors(TLMAutoColorPalettes.paletteListForEditing[sel]);
                    colorList.Enable();
                }
            }) as UIDropDown;
            editorSelector.GetComponentInParent<UIPanel>().width = 710;
            editorSelector.width = 710;

            colorList = group6.AddNumberedColorList(null, new List<Color32>(), (c) => { }, null, null);
            colorList.m_atlasToUse = TLMController.taLineNumber;
            colorList.m_spriteName = TLMLineIcon.Square.getImageName();
        }

        public delegate void OnPalettesChanged();
    }
}
