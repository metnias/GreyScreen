using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;

namespace GreyScreen
{
    public static class GreyPatch
    {
        public static void Patch()
        {
            On.CustomDecal.DrawSprites += new On.CustomDecal.hook_DrawSprites(DecalPatch);
            On.Water.DrawSprites += new On.Water.hook_DrawSprites(WaterPatch);
            On.RoomCamera.ApplyFade += new On.RoomCamera.hook_ApplyFade(CameraFadePatch);
            On.RainWorldGame.ExitGame += new On.RainWorldGame.hook_ExitGame(ExitGamePatch);
        }

        public static void DecalPatch(On.CustomDecal.orig_DrawSprites orig, CustomDecal self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (GreyScript.mask) { sLeaser.sprites[0].alpha = 0f; }
            else { sLeaser.sprites[0].alpha = 1f; }
        }

        public static void WaterPatch(On.Water.orig_DrawSprites orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (GreyScript.mask) { sLeaser.sprites[1].isVisible = false; }
            else { sLeaser.sprites[1].isVisible = !rCam.voidSeaMode; }
        }

        public static void CameraFadePatch(On.RoomCamera.orig_ApplyFade orig, RoomCamera self)
        {
            if (!GreyScript.mask) { orig.Invoke(self); return; }
            foreach (GreyScript.CameraInfo info in GreyScript.infos)
            {
                if (info.camera == self)
                {
                    typeof(RoomCamera).GetField("paletteTexture", GreyScript.flags).SetValue(self, info.tOrig);
                    orig.Invoke(self);
                    info.tOrig = (Texture2D)typeof(RoomCamera).GetField("paletteTexture", GreyScript.flags).GetValue(self);
                    typeof(RoomCamera).GetField("paletteTexture", GreyScript.flags).SetValue(self, info.pGrey.texture);
                    typeof(RoomCamera).GetMethod("ApplyPalette", GreyScript.flags).Invoke(self, new object[] { });

                    return;
                }
            }
            orig.Invoke(self);
        }

        public static void ExitGamePatch(On.RainWorldGame.orig_ExitGame orig, RainWorldGame self, bool asDeath, bool asQuit)
        {
            orig.Invoke(self, asDeath, asQuit);
            GreyScript.mask = false;
        }
    }
}
