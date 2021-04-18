using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.Sprites;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.OptionsMenu.Tabs
{
    internal class TLMPaletteOptionsTab : UICustomControl, ITLMConfigOptionsTab
    {
        private UIComponent parent;
        private UIDropDown m_paletteSelect;

        public static event OnPalettesChanged onPaletteReloaded;

        public void ReloadData()
        {
            string idxSel = m_paletteSelect.selectedValue;
            m_paletteSelect.items = TLMAutoColorPalettes.paletteListForEditing;
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

            FileInfo fiPalette = FileUtils.EnsureFolderCreation(TLMController.palettesFolder);

            group6.AddLabel(Locale.Get("K45_TLM_PALETTE_FOLDER_LABEL") + ":");
            var namesFilesButton = ((UIButton)group6.AddButton("/", () => ColossalFramework.Utils.OpenInFileBrowser(fiPalette.FullName)));
            namesFilesButton.textColor = Color.yellow;
            KlyteMonoUtils.LimitWidth(namesFilesButton, 710);
            namesFilesButton.text = fiPalette.FullName + Path.DirectorySeparatorChar;
            ((UIButton)group6.AddButton(Locale.Get("K45_TLM_RELOAD_PALETTES"), delegate ()
            {
                TLMAutoColorPalettes.Reload();
                ReloadData();
                onPaletteReloaded?.Invoke();
            })).width = 710;

            NumberedColorList colorList = null;
            m_paletteSelect = group6.AddDropdown(Locale.Get("K45_TLM_PALETTE_VIEW"), TLMAutoColorPalettes.paletteListForEditing, 0, delegate (int sel)
            {
                if (sel <= 0 || sel >= TLMAutoColorPalettes.paletteListForEditing.Length)
                {
                    colorList.Disable();
                }
                else
                {
                    colorList.ColorList = TLMAutoColorPalettes.getColors(TLMAutoColorPalettes.paletteListForEditing[sel]);
                    colorList.Enable();
                }
            }) as UIDropDown;
            m_paletteSelect.GetComponentInParent<UIPanel>().width = 710;
            m_paletteSelect.width = 710;

            colorList = group6.AddNumberedColorList(null, new List<Color32>(), (c) => { }, null, null);
            colorList.m_spriteName = KlyteResourceLoader.GetDefaultSpriteNameFor(LineIconSpriteNames.K45_SquareIcon, true);
            colorList.Size = new Vector2(750, colorList.Size.y);
        }

        public delegate void OnPalettesChanged();
    }
}
