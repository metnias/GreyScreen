using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GreyScreen
{
    public static class ColorMagic
    {
        public static Texture2D ClonePaletteTexture(Texture2D orig)
        {
            Texture2D t = new Texture2D(orig.width, orig.height, TextureFormat.ARGB32, false);
            for (int v = 0; v < orig.height; v++)
            {
                for (int u = 0; u < orig.width; u++)
                {
                    t.SetPixel(u, v, orig.GetPixel(u, v));
                }
            }
            t.Apply();
            return t;
        }

        public static Texture2D GreyMagic(Texture2D orig)
        {
            Texture2D t = ClonePaletteTexture(orig);
            Color c = GreyPlugin.maskColor.Value;
            //Shade
            List<ColorID> ids = new List<ColorID>
                {
                    ColorID.Sky,
                    ColorID.Fog,
                    //ColorID.Black,
                    //ColorID.Item,
                    ColorID.DeepWaterClose,
                    ColorID.DeepWaterFar,
                    ColorID.WaterSurfaceClose,
                    ColorID.WaterSurfaceFar,
                    ColorID.Effect1CloseShaded,
                    ColorID.Effect1FarShaded,
                    ColorID.Effect2CloseShaded,
                    ColorID.Effect2FarShaded,
                    ColorID.Effect1CloseLit,
                    ColorID.Effect1FarLit,
                    ColorID.Effect2CloseLit,
                    ColorID.Effect2FarLit,
                    ColorID.WaterSurfaceHighlight,
                    ColorID.SkyBloomObsolete
                };

            for (int p = 0; p < 2; p++)
            {
                for (int r = 0; r < 32; r++)
                {
                    SetColor(p, ColorID.Rainbow, c, ref t, r);
                }
                for (int k = 0; k < 30; k++)
                {
                    SetColor(p, ColorID.Terrain0, c, ref t, k);
                    SetColor(p, ColorID.Terrain1, c, ref t, k);
                    SetColor(p, ColorID.Terrain2, c, ref t, k); //close
                    SetColor(p, ColorID.Terrain3, c, ref t, k);
                    SetColor(p, ColorID.Terrain4, c, ref t, k);
                    SetColor(p, ColorID.Terrain5, c, ref t, k); //far
                }

                for (int d = ids.Count - 1; d >= 0; d--)
                {
                    SetColor(p, ids[d], c, ref t);
                }

                SetColor(p, ColorID.FogAmount, Color.black, ref t);
            }

            t.Apply();
            return t;
        }

        public static Color GetColor(int rain, ColorID id, Texture2D palette, int offset = 0)
        {
            IntVector2 uv = ColorUV[id];
            return palette.GetPixel(uv.x + offset, uv.y + rain * 8);
        }

        public static void SetColor(int rain, ColorID id, Color color, ref Texture2D palette, int offset = 0)
        {
            IntVector2 uv = ColorUV[id];
            palette.SetPixel(uv.x + offset, uv.y + rain * 8, color);
        }

        public static HSLColor RGBtoHSL(Color C)
        {
            RXColorHSL rx = RXColor.HSLFromColor(C);
            HSLColor result = new HSLColor
            {
                hue = rx.h,
                saturation = rx.s,
                lightness = rx.l
            };
            return result;
        }

        public enum ColorID
        {
            Sky,
            Fog,
            Black,
            Item,
            DeepWaterClose,
            DeepWaterFar,
            WaterSurfaceClose,
            WaterSurfaceFar,
            WaterSurfaceHighlight,
            FogAmount,
            Shortcut1,
            Shortcut2,
            Shortcut3,
            ShortcutSymbol,
            SkyBloomObsolete,
            RoomDarkness,
            RainPalette,
            Rainbow,
            Effect1CloseLit,
            Effect1CloseShaded,
            Effect1FarLit,
            Effect1FarShaded,
            Effect2CloseLit,
            Effect2CloseShaded,
            Effect2FarLit,
            Effect2FarShaded,
            Terrain0,
            Terrain1,
            Terrain2,
            Terrain3,
            Terrain4,
            Terrain5
        }

        public static Dictionary<ColorID, IntVector2> ColorUV;

        public static void GenerateColorUV()
        {
            Dictionary<ColorID, IntVector2> dict = new Dictionary<ColorID, IntVector2>
            {
                { ColorID.Sky, new IntVector2(0, 7) },
                { ColorID.Fog, new IntVector2(1, 7) },
                { ColorID.Black, new IntVector2(2, 7) },
                { ColorID.Item, new IntVector2(3, 7) },
                { ColorID.DeepWaterClose, new IntVector2(4, 7) },
                { ColorID.DeepWaterFar, new IntVector2(5, 7) },
                { ColorID.WaterSurfaceClose, new IntVector2(6, 7) },
                { ColorID.WaterSurfaceFar, new IntVector2(7, 7) },
                { ColorID.WaterSurfaceHighlight, new IntVector2(8, 7) },
                { ColorID.FogAmount, new IntVector2(9, 7) },
                { ColorID.Shortcut1, new IntVector2(10, 7) },
                { ColorID.Shortcut2, new IntVector2(11, 7) },
                { ColorID.Shortcut3, new IntVector2(12, 7) },
                { ColorID.ShortcutSymbol, new IntVector2(13, 7) },
                { ColorID.SkyBloomObsolete, new IntVector2(29, 7) },
                { ColorID.RoomDarkness, new IntVector2(30, 7) },
                { ColorID.RainPalette, new IntVector2(31, 7) },
                { ColorID.Rainbow, new IntVector2(0, 6) },
                { ColorID.Terrain0, new IntVector2(0, 5) },
                { ColorID.Terrain1, new IntVector2(0, 4) },
                { ColorID.Terrain2, new IntVector2(0, 3) },
                { ColorID.Terrain3, new IntVector2(0, 2) },
                { ColorID.Terrain4, new IntVector2(0, 1) },
                { ColorID.Terrain5, new IntVector2(0, 0) },
                { ColorID.Effect1CloseLit, new IntVector2(30, 6) },
                { ColorID.Effect1CloseShaded, new IntVector2(31, 6) },
                { ColorID.Effect1FarLit, new IntVector2(30, 5) },
                { ColorID.Effect1FarShaded, new IntVector2(31, 5) },
                { ColorID.Effect2CloseLit, new IntVector2(30, 4) },
                { ColorID.Effect2CloseShaded, new IntVector2(31, 4) },
                { ColorID.Effect2FarLit, new IntVector2(30, 3) },
                { ColorID.Effect2FarShaded, new IntVector2(31, 3) }
            };

            ColorUV = dict;
        }
    }
}