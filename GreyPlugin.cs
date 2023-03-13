using BepInEx;
using GreyScreen;
using RWCustom;
using System.Reflection;
using UnityEngine;

#region Assembly attributes

[assembly: AssemblyVersion(GreyPlugin.PLUGIN_VERSION)]
[assembly: AssemblyFileVersion(GreyPlugin.PLUGIN_VERSION)]
[assembly: AssemblyTitle(GreyPlugin.PLUGIN_NAME + " (" + GreyPlugin.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(GreyPlugin.PLUGIN_NAME)]

#endregion Assembly attributes

namespace GreyScreen
{
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("RainWorld.exe")]
    public class GreyPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "com.rainworldgame.greyscreen.plugin";
        public const string PLUGIN_NAME = "GreyScreen";
        public const string PLUGIN_VERSION = "0.5.0.0";

        public void Awake()
        {
            instance = this;

            On.RainWorld.OnModsInit += OnEnabled;
        }

        private void OnEnabled(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (init) return;
            init = true;
            oi = new GreyOption();
            maskColor = oi.config.Bind("maskColor", Custom.hexToColor("36393F"));
            activateKey = oi.config.Bind("activateKey", KeyCode.G);
            fgRemoval = oi.config.Bind("fgRemoval", true);
            MachineConnector.SetRegisteredOI("greyscreen", oi);

            GreyPatch.Patch();
        }

        private bool init = false;

        public void Update()
        {
            if (rw == null)
            {
                rw = UnityEngine.Object.FindObjectOfType<RainWorld>();
                return;
            }

            if (pm.currentMainLoop?.ID != ProcessManager.ProcessID.Game)
            { mask = false; infos = null; freeze = false; return; }
            RainWorldGame game = pm.currentMainLoop as RainWorldGame;
            if (game.pauseMenu != null) { return; }

            if (mask)
            {
                if (!curRoom.BeingViewed) { mask = false; infos = null; }
            }

            if (Input.GetKey(activateKey.Value))
            {
                if (hold > 20)
                {
                    if (keyDown)
                    {
                        keyDown = false;
                        // toggle freeze
                        freeze = !freeze;
                        Logger.LogMessage($"Freeze {(freeze ? "activate" : "deactivate")}");
                    }
                }
                else
                { hold++; keyDown = true; }
            }
            else
            {
                if (keyDown)
                {
                    mask = !mask;
                    if (mask)
                    {
                        Logger.LogMessage($"Apply Mask (removeFG: {(fgRemoval.Value ? "O" : "X")})");
                        infos = new CameraInfo[game.cameras.Length];
                        for (int i = 0; i < game.cameras.Length; i++)
                        {
                            infos[i] = new CameraInfo(game.cameras[i], (Texture2D)typeof(RoomCamera).GetField("paletteTexture", flags).GetValue(game.cameras[i]));
                            game.cameras[i].currentPalette = CameraInfo.ClonePalette(infos[i].pGrey);
                            typeof(RoomCamera).GetField("paletteTexture", flags).SetValue(game.cameras[i], infos[i].pGrey.texture);
                            game.cameras[i].ReturnFContainer("BackgroundShortcuts").isVisible = false;

                            if (fgRemoval.Value)
                            {
                                infos[i].mask = new FSprite("Futile_White", true)
                                {
                                    scaleX = game.rainWorld.options.ScreenSize.x / 16f,
                                    scaleY = 48f,
                                    anchorX = 0f,
                                    anchorY = 0f,
                                    color = maskColor.Value,
                                    shader = game.rainWorld.Shaders["Basic"]
                                };
                                game.cameras[i].ReturnFContainer("Background").AddChild(infos[i].mask);
                                game.cameras[i].ReturnFContainer("Foreground").isVisible = false;
                            }
                        }
                        curRoom = game.cameras[0].room;
                    }
                    else
                    {
                        Logger.LogMessage($"GreyScreen) Revert Mask (fgRemoval: {(fgRemoval.Value ? "O" : "X")})");
                        for (int i = 0; i < game.cameras.Length; i++)
                        {
                            if (infos[i].pOrig.texture != infos[i].tOrig)
                            {
                                game.cameras[i].currentPalette = new RoomPalette(infos[i].tOrig, 1f - infos[i].tOrig.GetPixel(9, 7).r, 1f - infos[i].tOrig.GetPixel(30, 7).r,
                                    infos[i].tOrig.GetPixel(2, 7), infos[i].tOrig.GetPixel(4, 7), infos[i].tOrig.GetPixel(5, 7), infos[i].tOrig.GetPixel(6, 7),
                                    infos[i].tOrig.GetPixel(7, 7), infos[i].tOrig.GetPixel(8, 7), infos[i].tOrig.GetPixel(1, 7), infos[i].tOrig.GetPixel(0, 7),
                                    infos[i].tOrig.GetPixel(10, 7), infos[i].tOrig.GetPixel(11, 7), infos[i].tOrig.GetPixel(12, 7), infos[i].tOrig.GetPixel(13, 7));
                            }
                            else
                            { game.cameras[i].currentPalette = CameraInfo.ClonePalette(infos[i].pOrig); }
                            typeof(RoomCamera).GetField("paletteTexture", flags).SetValue(game.cameras[i], infos[i].tOrig);
                            game.cameras[i].ReturnFContainer("BackgroundShortcuts").isVisible = true;

                            if (fgRemoval.Value)
                            {
                                infos[i].mask.RemoveFromContainer();
                                game.cameras[i].ReturnFContainer("Foreground").isVisible = true;
                            }
                        }
                        infos = null;
                    }
                }
                keyDown = false;
                hold = 0;
            }
        }

        public class CameraInfo
        {
            public CameraInfo(RoomCamera camera, Texture2D tOrig)
            {
                this.camera = camera;
                this.tOrig = ColorMagic.ClonePaletteTexture(tOrig);
                this.pOrig = ClonePalette(camera.currentPalette);
                this.GenerateGrey();
            }

            public void GenerateGrey()
            {
                Color c = maskColor.Value;

                this.pGrey = new RoomPalette(ColorMagic.GreyMagic(this.pOrig.texture),
                    0f, this.pOrig.darkness, this.pOrig.blackColor, c, c, c, c, c, c, c,
                    this.pOrig.shortcutColors[0], this.pOrig.shortcutColors[1], this.pOrig.shortcutColors[2], this.pOrig.shortCutSymbol);
            }

            public static RoomPalette ClonePalette(RoomPalette orig)
            {
                return new RoomPalette(ColorMagic.ClonePaletteTexture(orig.texture), orig.fogAmount, orig.darkness,
                    orig.blackColor, orig.waterColor1, orig.waterColor2,
                    orig.waterSurfaceColor1, orig.waterSurfaceColor2, orig.waterShineColor,
                    orig.fogColor, orig.skyColor,
                    orig.shortcutColors[0], orig.shortcutColors[1],
                    orig.shortcutColors[2], orig.shortCutSymbol);
            }

            public readonly RoomCamera camera;
            public RoomPalette pOrig;
            public RoomPalette pGrey;
            public Texture2D tOrig;
            public FSprite mask;
        }

        internal static GreyPlugin instance;
        internal static BepInEx.Logging.ManualLogSource LogSource => instance.Logger;

        internal static GreyOption oi;
        internal static Configurable<Color> maskColor;
        internal static Configurable<KeyCode> activateKey;
        internal static Configurable<bool> fgRemoval;

        internal static short hold = 0;
        internal static bool freeze = false;
        internal static bool mask = false;
        internal static bool keyDown = false;
        internal static RainWorld rw;
        internal static ProcessManager pm => rw.processManager;

        internal static CameraInfo[] infos;
        internal static Room curRoom;
        public const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    }
}