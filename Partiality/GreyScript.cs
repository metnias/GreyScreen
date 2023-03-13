using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace GreyScreen
{
    public class GreyScript : MonoBehaviour
    {
        public static bool mask = false;
        public static bool keyDown = false;
        public static RainWorld rw;
        public static ProcessManager pm => rw.processManager;

        public static CameraInfo[] infos;
        public static Room curRoom;
        public const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public void Update()
        {
            if (rw == null)
            {
                rw = UnityEngine.Object.FindObjectOfType<RainWorld>();
                return;
            }

            if (pm.currentMainLoop?.ID != ProcessManager.ProcessID.Game) { mask = false; infos = null; return; }
            RainWorldGame game = pm.currentMainLoop as RainWorldGame;
            if (game.pauseMenu != null) { return; }

            if (mask)
            {
                if (!curRoom.BeingViewed) { mask = false; infos = null; }
            }

            if (!keyDown)
            {
                if (Input.GetKey(GreyMod.configs.key))
                {
                    mask = !mask;
                    keyDown = true;
                    if (mask)
                    {
                        Debug.Log("GreyScreen: Apply Mask");
                        infos = new CameraInfo[game.cameras.Length];
                        for (int i = 0; i < game.cameras.Length; i++)
                        {
                            infos[i] = new CameraInfo(game.cameras[i], (Texture2D)typeof(RoomCamera).GetField("paletteTexture", flags).GetValue(game.cameras[i]));
                            game.cameras[i].currentPalette = CameraInfo.ClonePalette(infos[i].pGrey);
                            typeof(RoomCamera).GetField("paletteTexture", flags).SetValue(game.cameras[i], infos[i].pGrey.texture);
                            game.cameras[i].ReturnFContainer("BackgroundShortcuts").isVisible = false;

                            if (GreyMod.configs.removefg)
                            {
                                infos[i].mask = new FSprite("Futile_White", true)
                                {
                                    scaleX = game.rainWorld.options.ScreenSize.x / 16f,
                                    scaleY = 48f,
                                    anchorX = 0f,
                                    anchorY = 0f,
                                    color = GreyMod.configs.mask,
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
                        Debug.Log("GreyScreen: Revert Mask");
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

                            if (GreyMod.configs.removefg)
                            {
                                infos[i].mask.RemoveFromContainer();
                                game.cameras[i].ReturnFContainer("Foreground").isVisible = true;
                            }
                        }
                        infos = null;
                    }
                }
            }
            else if (!Input.GetKey(GreyMod.configs.key))
            { keyDown = false; }
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
                Color c = GreyMod.configs.mask;

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
    }
}
