using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using Partiality.Modloader;
using UnityEngine;

namespace GreyScreen
{
    public class GreyOption : OptionInterface
    {
        public GreyOption(PartialityMod mod) : base(mod: mod)
        {
            try
            {
                this.transFile = "GreyOption.Translations.txt"; GreyMod.configs.trans = true;
            }
            catch { GreyMod.configs.trans = false; }
        }

        public override void Initialize()
        {
            base.Initialize();
            this.Tabs = new OpTab[1] { new OpTab() };

            binder = new OpKeyBinder(new Vector2(400f, 100f), new Vector2(50f, 24f), this.mod, "maskKey", KeyCode.G.ToString(), true);
            binder.SetDescription("Key for toggling Mask");
            cpMask = new OpColorPicker(new Vector2(100f, 100f), "mask", "36393F") { PaletteHex = paletteHex, PaletteName = paletteName, description = "Choose Mask Colour" };
            ckBkg = new OpCheckBox(new Vector2(400f, 180f), "fg", true) { description = "Check this to remove Foreground (Cleaner result but no lighting effect)" };
            this.Tabs[0].AddItems(binder, cpMask, ckBkg);
        }

        public OpLabel lID;
        public OpColorPicker cpMask;
        public OpKeyBinder binder;
        public OpCheckBox ckBkg;

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
            GreyMod.configs.key = (KeyCode)Enum.Parse(typeof(KeyCode), config["maskKey"]);
            GreyMod.configs.mask = OpColorPicker.HexToColor(config["mask"]);
            GreyMod.configs.removefg = config["fg"] == "true";
            Debug.Log(string.Concat("GreyScreen) key: ", config["maskKey"], " mask: ", config["mask"], " removeFg: ", config["fg"]));
        }

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
