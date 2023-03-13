using System;
using BepInEx;
using Menu.Remix.MixedUI;
using UnityEngine;
using static Menu.Remix.MixedUI.UIfocusable;

namespace GreyScreen
{
    public class GreyOption : OptionInterface
    {
        public GreyOption() : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            this.Tabs = new OpTab[1] { new OpTab(this) };

            kbUse = new OpKeyBinder(GreyPlugin.activateKey, new Vector2(100f, 390f), new Vector2(150f, 30f), true);
            ckFg = new OpCheckBox(GreyPlugin.fgRemoval, new Vector2(105f, 320f)) { description = Translate("Whether hides foreground or recolours it. Read the tutorial.") };
            cpMask = new OpColorPicker(GreyPlugin.maskColor, new Vector2(350f, 300f)) { PaletteHex = paletteHex, PaletteName = paletteName };
            this.Tabs[0].AddItems(kbUse, ckFg, cpMask);
            ckFg.SetNextFocusable(ckFg, ckFg, ckFg, ckFg);
            cpMask.SetNextFocusable(cpMask, cpMask, cpMask, cpMask);
            kbUse.SetNextFocusable(kbUse, kbUse, kbUse, kbUse);
            MutualHorizontalFocusableBind(ckFg, cpMask);
            MutualHorizontalFocusableBind(kbUse, cpMask);
            MutualVerticalFocusableBind(ckFg, kbUse);
            ckFg.SetNextFocusable(NextDirection.Down, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
            cpMask.SetNextFocusable(NextDirection.Down, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.SaveButton));

            OpLabel[] lblDescs = new OpLabel[4];
            lblDescs[0] = new OpLabel(new Vector2(100f, 430f), new Vector2(200f, 20f), Translate("Action Key"), FLabelAlignment.Left)
            { description = Translate("Choose a Key to activate GreyScreen ingame"), bumpBehav = kbUse.bumpBehav };
            lblDescs[1] = new OpLabel(new Vector2(100f, 350f), new Vector2(200f, 20f), Translate("Hide Foreground"), FLabelAlignment.Left)
            { description = Translate("Whether hides foreground or recolours it. Read the tutorial."), bumpBehav = ckFg.bumpBehav };
            lblDescs[2] = new OpLabel(new Vector2(300f, 460f), new Vector2(200f, 20f), Translate("Mask Colour"), FLabelAlignment.Right)
            { description = Translate("Choose a Colour for GreyScreen"), bumpBehav = cpMask.bumpBehav };

            this.Tabs[0].AddItems(new OpRect(new Vector2(80f, 40f), new Vector2(440f, 200f)));
            string tt = "- " + Translate("Press Action Key ingame to toggle masking.");
            tt += Environment.NewLine + "- " + Translate("If Hide Foreground is checked, GreyScreen will hide terrain completely. Usually gives cleaner results.");
            tt += Environment.NewLine + "- " + Translate("If Hide Foreground is unchecked, GreyScreen will recolour terrain to the chosen colour. This allows capturing LightSources.");
            tt += Environment.NewLine + "- " + Translate("Hold Action Key to freeze screen effects. Useful for capturing multi-camera rooms, or optimised GIFs without removing scenery.");
            tt += Environment.NewLine + "- " + Translate("If the mod is in action, Pause Menu will not darken the screen.");

            lblDescs[3] = new OpLabelLong(new Vector2(100f, 60f), new Vector2(400f, 160f), tt);
            this.Tabs[0].AddItems(lblDescs);
        }

        private OpColorPicker cpMask;
        private OpKeyBinder kbUse;
        private OpCheckBox ckFg;

        public static readonly string[] paletteHex = {
            "FFFFFF",
            "000000",
            "FF0000",
            "00FF00",
            "0000FF",
            "FFFF00",
            "00FFFF",
            "FF00FF",
            "808080",
            "36393F",
            "1A1A1B",
            "15202B"
        };

        public static readonly string[] paletteName = {
            "White",
            "Black",
            "Red",
            "Lime",
            "Blue",
            "Yellow",
            "Cyan",
            "Magenta",
            "Grey",
            "DiscordDark",
            "RedditNight",
            "TwitterDark"
        };
    }
}