using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;

namespace GreyScreen
{
    public class GreyMod : PartialityMod
    {
        public GreyMod()
        {
            instance = this;
            this.ModID = "GreyScreen";
            this.Version = "0200";
            this.author = "topicular";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            configs = new Configs()
            {
                trans = false
            };
            ColorMagic.GenerateColorUV();
            go = new GameObject("GreyScreen");
            script = go.AddComponent<GreyScript>();
            GreyPatch.Patch();
        }

        public static GreyOption LoadOI()
        {
            oi = new GreyOption(instance);
            return oi;
        }

        public static string Translate(string orig)
        {
            if (configs.trans) { oi.Translate(orig); }
            return orig;
        }


        public static GreyMod instance;
        public static GameObject go;
        public static GreyScript script;
        public static GreyOption oi;
        public static Configs configs;


    }

    public struct Configs
    {
        public bool trans;
        public Color mask;
        public KeyCode key;
        public bool removefg;
    }

}
