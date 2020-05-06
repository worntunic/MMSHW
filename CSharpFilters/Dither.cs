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
        public static Dither BillAtkinsonDither(Palette palette, bool applyGS)
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
            Dither dither = new Dither(factors, palette, applyGS);
            return dither;
        }

        public static Bitmap ApplyBillAtkinson(Bitmap bitmap, Palette palette, bool applyGS)
        {
            Dither dither = BillAtkinsonDither(palette, applyGS);
            return dither.ApplyDither(bitmap);
        }

        public static Bitmap ApplyBNWBillAtkinson(Bitmap bitmap, bool applyGS)
        {
            return ApplyBillAtkinson(bitmap, StockPalettes.BNW, applyGS);
        }
        public static Bitmap ApplyGameboyBillAtkinson(Bitmap bitmap, bool applyGS)
        {

            return ApplyBillAtkinson(bitmap, StockPalettes.Gameboy, applyGS);
        }
        public static Bitmap ApplyC64BillAtkinson(Bitmap bitmap, bool applyGS)
        {
            return ApplyBillAtkinson(bitmap, StockPalettes.C64, applyGS);
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
        public static Palette FromColorHisto(Bitmap bmp, int paletteSize)
        {
            Dictionary<Color, int> colorHisto = new Dictionary<Color, int>();
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (colorHisto.ContainsKey(clr))
                    {
                        colorHisto[clr]++;
                    } else
                    {
                        colorHisto.Add(clr, 1);
                    }
                }
            }
            Palette palette = new Palette();
            for (int i = 0; i < paletteSize; ++i)
            {
                int maxVal = int.MinValue;
                Color maxColor = Color.White;
                foreach (KeyValuePair<Color, int> pair in colorHisto)
                {
                    if (pair.Value >= maxVal) {
                        maxVal = pair.Value;
                        maxColor = pair.Key;
                    }
                }
                colorHisto.Remove(maxColor);
                palette.AddColor(maxColor);
            }
            return palette;
        }
        public static Palette FromEvenBrightnessColorHisto(Bitmap bmp, int paletteSize)
        {
            Dictionary<Color, int> colorHisto = new Dictionary<Color, int>();
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (colorHisto.ContainsKey(clr))
                    {
                        colorHisto[clr]++;
                    }
                    else
                    {
                        colorHisto.Add(clr, 1);
                    }
                }
            }
            Palette palette = new Palette();
            float brightnessStep = 1.0f / (float)paletteSize;
            for (int i = 0; i < paletteSize; ++i)
            {
                int maxVal = int.MinValue;
                Color maxColor = Color.White;
                foreach (KeyValuePair<Color, int> pair in colorHisto)
                {
                    float brightness = pair.Key.GetBrightness();
                    if (pair.Value >= maxVal && brightness >= i * brightnessStep && brightness <= ((i + 1) * brightnessStep))
                    {
                        maxVal = pair.Value;
                        maxColor = pair.Key;
                    }
                }
                colorHisto.Remove(maxColor);
                palette.AddColor(maxColor);
            }
            return palette;
        }
        public static Palette FromEvenSaturationColorHisto(Bitmap bmp, int paletteSize)
        {
            Dictionary<Color, int> colorHisto = new Dictionary<Color, int>();
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (colorHisto.ContainsKey(clr))
                    {
                        colorHisto[clr]++;
                    }
                    else
                    {
                        colorHisto.Add(clr, 1);
                    }
                }
            }
            Palette palette = new Palette();
            float saturationStep = 1.0f / (float)paletteSize;
            for (int i = 0; i < paletteSize; ++i)
            {
                int maxVal = int.MinValue;
                Color maxColor = Color.White;
                foreach (KeyValuePair<Color, int> pair in colorHisto)
                {
                    float saturation = pair.Key.GetSaturation();
                    if (pair.Value >= maxVal && saturation >= i * saturationStep && saturation <= ((i + 1) * saturationStep))
                    {
                        maxVal = pair.Value;
                        maxColor = pair.Key;
                    }
                }
                colorHisto.Remove(maxColor);
                palette.AddColor(maxColor);
            }
            return palette;
        }
        public static Palette FromEvenHueColorHisto(Bitmap bmp, int paletteSize)
        {
            Dictionary<Color, int> colorHisto = new Dictionary<Color, int>();
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (colorHisto.ContainsKey(clr))
                    {
                        colorHisto[clr]++;
                    }
                    else
                    {
                        colorHisto.Add(clr, 1);
                    }
                }
            }
            Palette palette = new Palette();
            float hueStep = 1.0f / (float)paletteSize;
            for (int i = 0; i < paletteSize; ++i)
            {
                int maxVal = int.MinValue;
                Color maxColor = Color.White;
                foreach (KeyValuePair<Color, int> pair in colorHisto)
                {
                    float hue = pair.Key.GetHue();
                    if (pair.Value >= maxVal && hue >= i * hueStep && hue <= ((i + 1) * hueStep))
                    {
                        maxVal = pair.Value;
                        maxColor = pair.Key;
                    }
                }
                colorHisto.Remove(maxColor);
                palette.AddColor(maxColor);
            }
            return palette;
        }
        public static Palette FromChannellHisto(Bitmap bmp, int paletteSize)
        {
            Dictionary<byte, int> rHisto = new Dictionary<byte, int>();
            Dictionary<byte, int> gHisto = new Dictionary<byte, int>();
            Dictionary<byte, int> bHisto = new Dictionary<byte, int>();
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (rHisto.ContainsKey(clr.R))
                    {
                        rHisto[clr.R]++;
                    }
                    else
                    {
                        rHisto.Add(clr.R, 1);
                    }
                    if (gHisto.ContainsKey(clr.G))
                    {
                        gHisto[clr.G]++;
                    }
                    else
                    {
                        gHisto.Add(clr.G, 1);
                    }
                    if (bHisto.ContainsKey(clr.B))
                    {
                        bHisto[clr.B]++;
                    }
                    else
                    {
                        bHisto.Add(clr.B, 1);
                    }
                }
            }
            Palette palette = new Palette();
            for (int i = 0; i < paletteSize; ++i)
            {
                int maxVal = int.MinValue;
                byte r, g, b;
                r = g = b = 0;
                foreach (KeyValuePair<byte, int> pair in rHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        r = pair.Key;
                    }
                }
                maxVal = int.MinValue;
                foreach (KeyValuePair<byte, int> pair in gHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        g = pair.Key;
                    }
                }
                maxVal = int.MinValue;
                foreach (KeyValuePair<byte, int> pair in bHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        b = pair.Key;
                    }
                }
                rHisto.Remove(r);
                gHisto.Remove(g);
                bHisto.Remove(b);
                palette.AddColor(Color.FromArgb(r, g, b));
            }
            return palette;
        }
        public static Palette FromChannellDividedHisto(Bitmap bmp, int paletteSize)
        {
            Dictionary<Color, int> colorHisto = new Dictionary<Color, int>();
            Dictionary<byte, int> rHisto = new Dictionary<byte, int>();
            Dictionary<byte, int> gHisto = new Dictionary<byte, int>();
            Dictionary<byte, int> bHisto = new Dictionary<byte, int>();
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (colorHisto.ContainsKey(clr))
                    {
                        colorHisto[clr]++;
                    } else
                    {
                        colorHisto.Add(clr, 1);
                    }
                    if (rHisto.ContainsKey(clr.R))
                    {
                        rHisto[clr.R]++;
                    }
                    else
                    {
                        rHisto.Add(clr.R, 1);
                    }
                    if (gHisto.ContainsKey(clr.G))
                    {
                        gHisto[clr.G]++;
                    }
                    else
                    {
                        gHisto.Add(clr.G, 1);
                    }
                    if (bHisto.ContainsKey(clr.B))
                    {
                        bHisto[clr.B]++;
                    }
                    else
                    {
                        bHisto.Add(clr.B, 1);
                    }
                }
            }

            Palette palette = new Palette();
            for (int i = 0; i < paletteSize / 3; ++i)
            {
                int maxVal = int.MinValue;
                Color maxColor = Color.White;
                foreach (KeyValuePair<Color, int> pair in colorHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        maxColor = pair.Key;
                    }
                }
                byte r, g, b;
                r = g = b = 0;
                foreach (KeyValuePair<byte, int> pair in rHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        r = pair.Key;
                    }
                }
                maxVal = int.MinValue;
                foreach (KeyValuePair<byte, int> pair in gHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        g = pair.Key;
                    }
                }
                maxVal = int.MinValue;
                foreach (KeyValuePair<byte, int> pair in bHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        b = pair.Key;
                    }
                }
                colorHisto.Remove(maxColor);
                rHisto.Remove(r);
                gHisto.Remove(g);
                bHisto.Remove(b);
                palette.AddColor(Color.FromArgb(r, maxColor.G, maxColor.B));
                palette.AddColor(Color.FromArgb(maxColor.R, g, maxColor.B));
                palette.AddColor(Color.FromArgb(maxColor.R, maxColor.G, b));
            }
            return palette;
        }

        public static Palette FromHSBHisto(Bitmap bmp, int paletteSize)
        {
            Dictionary<decimal, int> hHisto = new Dictionary<decimal, int>();
            Dictionary<decimal, int> sHisto = new Dictionary<decimal, int>();
            Dictionary<decimal, int> bHisto = new Dictionary<decimal, int>();
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color clr = bmp.GetPixel(x, y);
                    decimal h = Decimal.Round((decimal)clr.GetHue(), 3);
                    decimal s = Decimal.Round((decimal)clr.GetSaturation(), 3);
                    decimal b = Decimal.Round((decimal)clr.GetBrightness(), 3);
                    if (hHisto.ContainsKey(h))
                    {
                        hHisto[h]++;
                    }
                    else
                    {
                        hHisto.Add(h, 1);
                    }
                    if (sHisto.ContainsKey(s))
                    {
                        sHisto[s]++;
                    }
                    else
                    {
                        sHisto.Add(s, 1);
                    }
                    if (bHisto.ContainsKey(b))
                    {
                        bHisto[b]++;
                    }
                    else
                    {
                        bHisto.Add(b, 1);
                    }
                }
            }
            Palette palette = new Palette();
            for (int i = 0; i < paletteSize; ++i)
            {
                int maxVal = int.MinValue;
                decimal h, s, b;
                h = s = b = 0;
                foreach (KeyValuePair<decimal, int> pair in hHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        h = pair.Key;
                    }
                }
                maxVal = int.MinValue;
                foreach (KeyValuePair<decimal, int> pair in sHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        s = pair.Key;
                    }
                }
                maxVal = int.MinValue;
                foreach (KeyValuePair<decimal, int> pair in bHisto)
                {
                    if (pair.Value >= maxVal)
                    {
                        maxVal = pair.Value;
                        b = pair.Key;
                    }
                }
                hHisto.Remove(h);
                sHisto.Remove(s);
                bHisto.Remove(b);
                
                palette.AddColor(HSVToRGB(Decimal.ToDouble(h), Decimal.ToDouble(s), Decimal.ToDouble(b)));
            }
            return palette;
        }

        private static Color HSVToRGB(double h, double S, double V)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            int r = Clamp((int)(R * 255.0));
            int g = Clamp((int)(G * 255.0));
            int b = Clamp((int)(B * 255.0));
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        private static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }
}
