using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HWFilters
{
    /// <summary>
    /// Najlakši način da se izvrši funkcija za prvi zadatak (Grayscale pa Gamma):
    /// HWFilters.FilterGSGamma(bitmap, gamma);
    /// Gde je bitmap slika koja treba da se obradi
    /// A gamma niz od tri double vrednosti, koje predstavljaju uticaj na svaki od kanala tj.:
    /// gamma[0] - red
    /// gamma[1] - green
    /// gamma[2] - blue
    /// ILI
    /// HWFilters.FilterGSGamma(bitmap, gammaConfig)
    /// Gde je gammaConfig objekat klase GammaConfig
    /// koja može da se incijalizuje 
    /// ili sa new GammaConfig(double[3] gamma) 
    /// ili sa new GammaConfig(double red, double green, double blue)
    /// </summary>
    public struct GammaConfig {
        private double[] gamma;
        public double R
        {
            get
            {
                return gamma[0];
            }
            set
            {
                gamma[0] = value;
            }
        }
        public double G
        {
            get
            {
                return gamma[1];
            }
            set
            {
                gamma[1] = value;
            }
        }
        public double B
        {
            get
            {
                return gamma[2];
            }
            set
            {
                gamma[2] = value;
            }
        }

        public GammaConfig(double[] gamma)
        {
            if (gamma == null || gamma.Length != 3)
            {
                throw new Exception("Gamma configuration requires 3 double parameters");
            }
            this.gamma = new double[3];
            SetGamma(gamma[0], gamma[1], gamma[2]);
        }
        public GammaConfig(double red, double green, double blue)
        {
            this.gamma = new double[3];
            SetGamma(red, green, blue);
        }
        private void SetGamma(double red, double green, double blue)
        {
            R = red;
            G = green;
            B = blue;
        }
        public double[] GetGammaArray()
        {
            return gamma;
        }
    }
    public interface IPixelFilter
    {
        void PrepareData();
        Color ApplyFilter(Color pixelColor);
    }
    public class GrayscaleFilter : IPixelFilter
    {
        private const double GrayscaleRedFactor = 0.299;
        private const double GrayscaleGreenFactor = 0.587;
        private const double GrayscaleBlueFactor = 0.114;

        public GrayscaleFilter()
        {

        }

        public void PrepareData()
        {

        }

        public Color ApplyFilter(Color color)
        {
            int gsColor = 0;
            gsColor += (int)Math.Round(color.R * GrayscaleRedFactor);
            gsColor += (int)Math.Round(color.G * GrayscaleGreenFactor);
            gsColor += (int)Math.Round(color.B * GrayscaleBlueFactor);
            return Color.FromArgb(gsColor, gsColor, gsColor);
        }
    }
    public class GammaFilter : IPixelFilter
    {
        private GammaConfig gamma;
        private int[][] channelColors;

        public GammaFilter(double[] gamma)
        {
            this.gamma = new GammaConfig(gamma);
        }
        public GammaFilter(GammaConfig gamma)
        {
            this.gamma = gamma;
        }
        public GammaFilter(double red, double green, double blue)
        {
            this.gamma = new GammaConfig(red, green, blue);
        }

        public void PrepareData()
        {
            channelColors = new int[3][];
            double[] gammaExponents = new double[3];
            for (int i = 0; i < 3; ++i)
            {
                channelColors[i] = new int[256];
                gammaExponents[i] = 1.0 / gamma.GetGammaArray()[i];
            }
            for (int i = 0; i < 256; ++i)
            {
                channelColors[0][i] = MathUtils.Lerp(0, 255, Math.Pow(i / 255.0, gammaExponents[0]));
                channelColors[1][i] = MathUtils.Lerp(0, 255, Math.Pow(i / 255.0, gammaExponents[1]));
                channelColors[2][i] = MathUtils.Lerp(0, 255, Math.Pow(i / 255.0, gammaExponents[2]));
            }
        }

        public Color ApplyFilter(Color color)
        {
            return Color.FromArgb(channelColors[0][color.R], channelColors[1][color.G], channelColors[2][color.B]);
        }
    }
    public class HWFilter
    {
        private Bitmap bitmap;
        private List<IPixelFilter> filters;

        public HWFilter(Bitmap bitmap)
        {
            SetBitmap(bitmap);
            filters = new List<IPixelFilter>();
        }

        public void SetBitmap(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public void AddFilter(IPixelFilter filter)
        {
            filters.Add(filter);
        }

        public bool ApplyFilters()
        {
            for (int i = 0; i < filters.Count; i++)
            {
                filters[i].PrepareData();
            }
            for (int i = 0; i < bitmap.Width; ++i)
            {
                for (int j = 0; j < bitmap.Height; ++j)
                {
                    Color pixelColor = bitmap.GetPixel(i, j);
                    for (int f = 0; f < filters.Count; ++f)
                    {
                        pixelColor = filters[f].ApplyFilter(pixelColor);
                    }
                    bitmap.SetPixel(i, j, pixelColor);
                }
            }
            return true;
        }

        public static bool FilterGrayscale(Bitmap bitmap)
        {
            HWFilter hwFilter = new HWFilter(bitmap);

            GrayscaleFilter gsFilter = new GrayscaleFilter();
            hwFilter.AddFilter(gsFilter);
            return hwFilter.ApplyFilters();
        }

        public static bool FilterGamma(Bitmap bitmap, double[] gamma)
        {
            HWFilter hwFilter = new HWFilter(bitmap);

            GammaFilter gammaFilter = new GammaFilter(gamma);
            hwFilter.AddFilter(gammaFilter);

            return hwFilter.ApplyFilters();
        }

        public static Bitmap FilterGSGamma(Bitmap bitmap, double[] gamma)
        {
            GammaConfig gammaConfig = new GammaConfig(gamma);
            return FilterGSGamma(bitmap, gammaConfig);
        }

        public static Bitmap FilterGSGamma(Bitmap bitmap, GammaConfig gamma)
        {
            HWFilter hwFilter = new HWFilter(bitmap);

            GrayscaleFilter gsFilter = new GrayscaleFilter();
            GammaFilter gammaFilter = new GammaFilter(gamma);
            hwFilter.AddFilter(gsFilter);
            hwFilter.AddFilter(gammaFilter);
            hwFilter.ApplyFilters();
            return bitmap;
        }
    }
}
