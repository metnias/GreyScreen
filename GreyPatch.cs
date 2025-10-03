using UnityEngine;

namespace GreyScreen
{
    public static class GreyPatch
    {
        public static void Patch()
        {
            ColorMagic.GenerateColorUV();
            //On.CustomDecal.DrawSprites += DecalPatch;
            On.Water.DrawSprites += WaterPatch;
            On.RoomCamera.ApplyFade += CameraFadePatch;
            On.RainWorldGame.ExitGame += ExitGamePatch;
            On.Menu.PauseMenu.ctor += PauseNoFade;
            On.RoomCamera.DrawUpdate += RCDrawFreeze;
        }

        private static void DecalPatch(On.CustomDecal.orig_DrawSprites orig, CustomDecal self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (GreyPlugin.mask) { sLeaser.sprites[0].alpha = 0f; }
            else { sLeaser.sprites[0].alpha = 1f; }
        }

        private static void WaterPatch(On.Water.orig_DrawSprites orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (GreyPlugin.mask) { sLeaser.sprites[1].isVisible = false; }
            else { sLeaser.sprites[1].isVisible = !rCam.voidSeaMode; }
        }

        private static void CameraFadePatch(On.RoomCamera.orig_ApplyFade orig, RoomCamera self)
        {
            if (!GreyPlugin.mask) { orig.Invoke(self); return; }
            foreach (GreyPlugin.CameraInfo info in GreyPlugin.infos)
            {
                if (info.camera == self)
                {
                    typeof(RoomCamera).GetField("paletteTexture", GreyPlugin.flags).SetValue(self, info.tOrig);
                    orig.Invoke(self);
                    info.tOrig = (Texture2D)typeof(RoomCamera).GetField("paletteTexture", GreyPlugin.flags).GetValue(self);
                    typeof(RoomCamera).GetField("paletteTexture", GreyPlugin.flags).SetValue(self, info.pGrey.texture);
                    typeof(RoomCamera).GetMethod("ApplyPalette", GreyPlugin.flags).Invoke(self, new object[] { });

                    return;
                }
            }
            orig.Invoke(self);
        }

        private static void ExitGamePatch(On.RainWorldGame.orig_ExitGame orig, RainWorldGame self, bool asDeath, bool asQuit)
        {
            orig.Invoke(self, asDeath, asQuit);
            GreyPlugin.mask = false;
        }

        private static void PauseNoFade(On.Menu.PauseMenu.orig_ctor orig, Menu.PauseMenu self, ProcessManager manager, RainWorldGame game)
        {
            orig(self, manager, game);
            if (GreyPlugin.mask || GreyPlugin.freeze) { self.blackSprite.isVisible = false; }
        }

        private static void RCDrawFreeze(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            if (GreyPlugin.freeze)
            {
                orig(self, timeStacker, 0f);
                Shader.SetGlobalFloat("_cloudsSpeed", 0f);
            }
            else
            {
                orig(self, timeStacker, timeSpeed);
            }
            if (GreyPlugin.mask)
            {
                Shader.DisableKeyword("RoomHasDeathFall");
                if (self.fullScreenEffect != null)
                    self.fullScreenEffect.alpha = 0f;
            }
        }
    }
}