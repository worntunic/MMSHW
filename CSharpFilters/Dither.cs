using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace HWFilters
{
    public struct DitherFactor
    {
        public int xOffset;
        public int yOffset;
        public float factor;

        public DitherFactor(int x, int y, float factor)
        {
            this.xOffset = x;
            this.yOffset = y;
            this.factor = factor;
        }
    }
    public class Palette
    {
        private List<Color> colors = new List<Color>();

        public void AddColor(Color color)
        {
            if (!colors.Contains(color))
            {
                colors.Add(color);
            }
        }
        public void RemoveColor(Color color)
        {
            if (colors.Contains(color))
            {
                colors.Remove(color);
            }
        }
        public Color GetClosestPaletteColor(Color original)
        {
            int minOffset = int.MaxValue;
            int curColorIndex = -1;
            for (int i = 0; i < colors.Count; ++i)
            {
                int rOffset = Math.Abs(original.R - colors[i].R);
                int gOffset = Math.Abs(original.G - colors[i].G);
                int bOffset = Math.Abs(original.B - colors[i].B);
                int totalOffset = rOffset + gOffset + bOffset;
                if (totalOffset <= minOffset)
                {
                    minOffset = totalOffset;
                    curColorIndex = i;
                }
            }
            return colors[curColorIndex];
        }
    }
    public class Dither
    {
        private DitherFactor[] factors;
        private Palette palette;
        private bool grayscaleFirst;
        private const double GSFactorR = 0.299;
        private const double GSFactorG = 0.587;
        private const double GSFactorB = 0.114;

        public Dither(DitherFactor[] factors, Palette palette, bool grayscaleFirst = false)
        {
            this.factors = factors;
            this.palette = palette;
            this.grayscaleFirst = grayscaleFirst;

        }

        public Bitmap ApplyDither(Bitmap b)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);// PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;
                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        Color oldColor;
                        if (grayscaleFirst)
                        {
                            byte color = (byte)(p[2] * GSFactorB + p[1] * GSFactorG + p[0] * GSFactorR);
                            oldColor = Color.FromArgb(color, color, color);
                        } else
                        {
                            oldColor = Color.FromArgb(p[2], p[1], p[0]);
                        }
                        
                        Color newColor = palette.GetClosestPaletteColor(oldColor);
                        int rError = oldColor.R - newColor.R;
                        int gError = oldColor.G - newColor.G;
                        int bError = oldColor.B - newColor.B;

                            p[0] = (byte)newColor.B;
                            p[1] = (byte)newColor.G;
                            p[2] = (byte)newColor.R;

                        //Apply errors
                        for (int i = 0; i < factors.Length; ++i)
                        {
                            if (MathUtils.IsBetweenOrEqual(0, b.Width - 1, x + factors[i].xOffset)
                                && MathUtils.IsBetweenOrEqual(0, b.Height - 1, y + factors[i].yOffset))
                            {
                                int offset = factors[i].xOffset * 3 + factors[i].yOffset * stride;
                                p[offset] = (byte)(p[offset] + bError * factors[i].factor);
                                p[offset + 1] = (byte)(p[offset + 1] + gError * factors[i].factor);
                                p[offset + 2] = (byte)(p[offset + 2] + bError * factors[i].factor);
                            }
                        }
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            b.UnlockBits(bmData);
            return b;
        }
    }

    public static class StockDithers
    {
        public static Dither BillAtkinsonDither(Palette palette)
        {
            DitherFactor[] factors = new DitherFactor[6]
            {
                new DitherFactor(1, 0, 0.125f),
                new DitherFactor(2, 0, 0.125f),
                new DitherFactor(-1, 1, 0.125f),
                new DitherFactor(0, 1, 0.125f),
                new DitherFactor(1, 1, 0.125f),
                new DitherFactor(0, 2, 0.125f)
            };
            Dither dither = new Dither(factors, palette, true);
            return dither;
        }

        public static Bitmap ApplyBNWBillAtkinson(Bitmap bitmap)
        {

            Dither dither = BillAtkinsonDither(StockPalettes.BNW);
            return dither.ApplyDither(bitmap);
        }
        public static Bitmap ApplyGameboyBillAtkinson(Bitmap bitmap)
        {

            Dither dither = BillAtkinsonDither(StockPalettes.Gameboy);
            return dither.ApplyDither(bitmap);
        }
        public static Bitmap ApplyC64BillAtkinson(Bitmap bitmap)
        {

            Dither dither = BillAtkinsonDither(StockPalettes.C64);
            return dither.ApplyDither(bitmap);
        }
    }

    static class StockPalettes
    {
        private static Palette bnw;
        public static Palette BNW
        {
            get
            {
                if (bnw == null)
                {
                    bnw = new Palette();
                    bnw.AddColor(Color.White);
                    bnw.AddColor(Color.Black);
                }
                return bnw;
            }
        }
        private static Palette gameboy;
        public static Palette Gameboy 
        {
            get
            {
                if (gameboy == null)
                {
                    gameboy = new Palette();
                    gameboy.AddColor(Color.FromArgb(202, 220, 159));
                    gameboy.AddColor(Color.FromArgb(15, 56, 15));
                    gameboy.AddColor(Color.FromArgb(48, 98, 48));
                    gameboy.AddColor(Color.FromArgb(139, 172, 15));
                    gameboy.AddColor(Color.FromArgb(155, 188, 15));
                }
                return gameboy;
            }

        }
        private static Palette c64;
        public static Palette C64
        {
            get
            {
                if (c64 == null)
                {
                    c64 = new Palette();
                    c64.AddColor(Color.FromArgb(0, 0, 0));
                    c64.AddColor(Color.FromArgb(255, 255, 255));
                    c64.AddColor(Color.FromArgb(136, 0, 0));
                    c64.AddColor(Color.FromArgb(170, 255, 238));

                    c64.AddColor(Color.FromArgb(204, 68, 204));
                    c64.AddColor(Color.FromArgb(0, 204, 85));
                    c64.AddColor(Color.FromArgb(0, 0, 170));
                    c64.AddColor(Color.FromArgb(238, 238, 119));

                    c64.AddColor(Color.FromArgb(221, 136, 85));
                    c64.AddColor(Color.FromArgb(102, 68, 0));
                    c64.AddColor(Color.FromArgb(255, 119, 119));
                    c64.AddColor(Color.FromArgb(51, 51, 51));

                    c64.AddColor(Color.FromArgb(119, 119, 119));
                    c64.AddColor(Color.FromArgb(170, 255, 102));
                    c64.AddColor(Color.FromArgb(0, 136, 255));
                    c64.AddColor(Color.FromArgb(187, 187, 187));
                }
                return c64;
            }
        }
    }
}
