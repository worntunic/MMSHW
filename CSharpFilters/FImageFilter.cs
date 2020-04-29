using HWFilters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HWFilters
{
    /// <summary>
    /// Najlakši način da se izvrši funkcija za drugi zadatak (Mean Removal pa Sphere Offset):
    /// HWFilters.StockFilters.MeanRemovalThenSphere(bitmap, nWeight);
    /// Gde je bitmap slika koja treba da se obradi
    /// A nWeight integer koji predstavlja vrednost sredine konvolucione matrice za MeanRemoval
    /// 
    /// 
    /// Ako Sphere ne treba da bude u centru slike onda koristite, ili želite drugi tip popunjavanja okoline:
    /// HWFilters.StockFilters.MeanRemovalThenSphere(bitmap, msConfig);
    /// Gde je msConfig tipa MeanSphereConfig i sadrži
    /// nWeight - vrednost sredine konvolucione matrice
    /// paddingType - tip popunjavanja okoline (0, 255, Kopiranje okolnih i Simetrično u odnosu na traženi piksel)
    /// sphereMidPoint - objekat tipa PixelPoint koji sadrži x i y piksela koji želimo da bude centar sfere
    /// </summary>
    public interface IFilter
    {
        Bitmap ApplyFilter(Bitmap bitmap);
    }
    #region ConvolutionFilters
    public enum PaddingType { Zero, Full, CopyClosest, CopySymetrical }
    
    public struct ConvolutionConfig
    {
        public PaddingType paddingType;
        public int[][] matrix;
        public int offset;
        public int factor;
        public int width { get; private set; }
        public int height { get; private set; }
        public int xRadius { get; private set; }
        public int yRadius { get; private set; }

        public ConvolutionConfig(int[][] matrix, int offset, int factor, PaddingType type)
        {
            this.matrix = matrix;
            if (matrix != null)
            {
                width = matrix[0].Length;
                height = matrix.Length;
            } else
            {
                width = height = 0;
            }
            this.offset = offset;
            this.factor = factor;
            this.paddingType = type;
            yRadius = (int)Math.Floor(height / 2.0);
            xRadius = (int)Math.Floor(width / 2.0);
        }
        public void SetMatrixValue(int x, int y, int value)
        {
            matrix[y][x] = value;
        }
        public Color Calculate(Color[][] subImage)
        {
            if (subImage.Length != height || subImage[0].Length != width)
            {
                throw new ArgumentException("SubImage Must Be of equal width and height to Convolution Matrix");
            }
            int rValue = 0;
            int gValue = 0;
            int bValue = 0;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    rValue += matrix[y][x] * subImage[y][x].R;
                    gValue += matrix[y][x] * subImage[y][x].G;
                    bValue += matrix[y][x] * subImage[y][x].B;
                }
            }
            
            rValue = MathUtils.Clamp(rValue / factor + offset, 0, 255);
            gValue = MathUtils.Clamp(gValue / factor + offset, 0, 255);
            bValue = MathUtils.Clamp(bValue / factor + offset, 0, 255);
            return Color.FromArgb(rValue, gValue, bValue);
        }

        public static ConvolutionConfig GetIdentityConfig(int width, int height, int offset, int factor, PaddingType type = PaddingType.Zero)
        {
            return new ConvolutionConfig(FillMatrixWithValue(width, height, 1), offset, factor, type);
        }
        public static ConvolutionConfig GetZeroConfig(int width, int height, int offset, int factor, PaddingType type = PaddingType.Zero)
        {
            return new ConvolutionConfig(FillMatrixWithValue(width, height, 0), offset, factor, type);
        }
        public static ConvolutionConfig GetFilledConfig(int width, int height, int offset, int factor, int value, PaddingType type = PaddingType.Zero)
        {
            return new ConvolutionConfig(FillMatrixWithValue(width, height, value), offset, factor, type);
        }
        private static int[][] FillMatrixWithValue(int width, int height, int value)
        {
            int[][] matrix = new int[height][];
            for (int y = 0; y < height; ++y)
            {
                matrix[y] = new int[width];
                for (int x = 0; x < width; ++x)
                {
                    matrix[y][x] = value;
                }
            }
            return matrix;
        }

    }

    public class ConvolutionFilter : IFilter
    {
        public ConvolutionConfig convConfig;

        public ConvolutionFilter(ConvolutionConfig config)
        {
            this.convConfig = config;
        }
        public Bitmap ApplyFilter(Bitmap bitmap)
        {
            Bitmap bNew = (Bitmap)bitmap.Clone();
            Color[][] subImage = new Color[convConfig.height][];
            for (int y = 0; y < convConfig.height; ++y)
            {
                subImage[y] = new Color[convConfig.width];
            }
            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    GetSubImage(x, y, bitmap, ref subImage);
                    Color value = convConfig.Calculate(subImage);
                    bNew.SetPixel(x, y, value);
                }
            }
            return bNew;
        }

        private void GetSubImage(int centerX, int centerY, Bitmap bitmap, ref Color[][] subImage)
        {
            int startY = centerY - convConfig.yRadius;
            int startX = centerX - convConfig.xRadius;
            int endY = centerY + convConfig.yRadius;
            int endX = centerX + convConfig.xRadius;
            for (int y = startY; y <= endY; ++y)
            {
                for (int x = startX; x <= endX; ++x)
                {
                    int sY = y - startY;
                    int sX = x - startX;
                    subImage[sY][sX] = GetPixelOrPad(x, y, centerX, centerY, bitmap);
                }
            }
        }
        private int padCount = 0;
        private int pixelCount = 0;
        private Color GetPixelOrPad(int x, int y, int centerX, int centerY, Bitmap bitmap)
        {
            if (x < 0 || y < 0 || x >= bitmap.Width || y >= bitmap.Height)
            {
                padCount++;
                switch (convConfig.paddingType)
                {
                    case (PaddingType.Zero):
                        return Color.Black;
                    case (PaddingType.Full):
                        return Color.White;
                    case (PaddingType.CopyClosest):
                        return GetCopyClosestColor(x, y, bitmap);
                    case (PaddingType.CopySymetrical):
                        return GetCopySymetricalColor(x, y, centerX, centerY, bitmap);
                }
                throw new Exception($"PaddingType: {convConfig.paddingType} not found");
            } else
            {
                pixelCount++;
                return bitmap.GetPixel(x, y);
            }
        }

        private Color GetCopyClosestColor(int x, int y, Bitmap bitmap)
        {
            if (x < 0)
            {
                x = 0;
            } else if (x >= bitmap.Width)
            {
                x = bitmap.Width - 1;
            }
            if (y < 0)
            {
                y = 0;
            }
            else if (y >= bitmap.Height)
            {
                y = bitmap.Height - 1;
            }
            return bitmap.GetPixel(x, y);
        }
        private Color GetCopySymetricalColor(int x, int y, int centerX, int centerY, Bitmap bitmap)
        {
            int centerXDistance = centerX - x;
            int centerYDistance = centerY - y;

            if (!MathUtils.IsBetweenOrEqual(0, bitmap.Width - 1, x))
            {
                x = centerX + centerXDistance;
            }
            if (!MathUtils.IsBetweenOrEqual(0, bitmap.Height - 1, y))
            {
                y = centerY + centerYDistance;
            }
            return bitmap.GetPixel(x, y);
        }
    }
    #endregion
    #region OffsetFilters
    public struct PixelPoint
    {
        public int x;
        public int y;

        public void Set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public PixelPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class OffsetFilter : IFilter
    {
        public Bitmap ApplyFilter(Bitmap bitmap)
        {
            Bitmap newB = (Bitmap)bitmap.Clone();
            PixelPoint curPoint = new PixelPoint();
            PixelPoint newPoint = new PixelPoint();
            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    curPoint.x = x;
                    curPoint.y = y;
                    newPoint = GetOffset(curPoint);
                    newPoint.x = MathUtils.Clamp(newPoint.x, 0, bitmap.Width - 1);
                    newPoint.y = MathUtils.Clamp(newPoint.y, 0, bitmap.Height - 1);
                    newB.SetPixel(x, y, bitmap.GetPixel(newPoint.x, newPoint.y));
                }
            }
            return newB;
        }
        protected virtual PixelPoint GetOffset(PixelPoint startPoint)
        {
            return startPoint;
        }
    }
    public class SphereFilter : OffsetFilter
    {
        private PixelPoint midPoint;
        public SphereFilter(PixelPoint midPoint)
        {
            this.midPoint = midPoint;
        }
        public SphereFilter(int xMid, int yMid)
        {
            this.midPoint = new PixelPoint(xMid, yMid);
        }

        protected override PixelPoint GetOffset(PixelPoint startPoint) { 
            int relX = startPoint.x - midPoint.x;
            int relY = startPoint.y - midPoint.y;
            double theta = Math.Atan2(relY, relX);
            double radius = Math.Sqrt(relX * relX + relY * relY);
            double newRadius = radius * radius / (Math.Max(midPoint.x, midPoint.y));

            int newX = (int)(midPoint.x + newRadius * Math.Cos(theta));
            int newY = (int)(midPoint.y + newRadius * Math.Sin(theta));
            return new PixelPoint(newX, newY);
        }
    }
    #endregion
    public struct MeanSphereConfig
    {
        public int nWeight;
        public PaddingType paddingType;
        public PixelPoint sphereMidPoint;

        public MeanSphereConfig(int nWeight, PaddingType paddingType, PixelPoint sphereMidPoint)
        {
            this.nWeight = nWeight;
            this.paddingType = paddingType;
            this.sphereMidPoint = sphereMidPoint;
        }
    }
    public static class StockFilters
    {

        public static Bitmap MeanRemoval(Bitmap bitmap, int nWeight)
        {
            ConvolutionConfig config = ConvolutionConfig.GetFilledConfig(3, 3, 0, nWeight - 8, -1, PaddingType.Zero);
            config.SetMatrixValue(1, 1, nWeight);
            ConvolutionFilter filter = new ConvolutionFilter(config);
            return filter.ApplyFilter(bitmap);
        }
        public static Bitmap Sphere(Bitmap bitmap)
        {
            SphereFilter sFilter = new SphereFilter(new PixelPoint(bitmap.Width / 2, bitmap.Height / 2));
            return sFilter.ApplyFilter(bitmap);
        }
        public static Bitmap Sphere(Bitmap bitmap, PixelPoint pixelPoint)
        {
            SphereFilter sFilter = new SphereFilter(pixelPoint);
            return sFilter.ApplyFilter(bitmap);
        }
        public static Bitmap MeanRemovalThenSphere(Bitmap bitmap, int nWeight)
        {
            Bitmap newB = MeanRemoval(bitmap, nWeight);
            newB = Sphere(newB);
            return newB;
        }
        public static Bitmap MeanRemovalThenSphere(Bitmap bitmap, MeanSphereConfig msConfig)
        {
            ConvolutionConfig config = ConvolutionConfig.GetFilledConfig(3, 3, 0, msConfig.nWeight - 8, -1, msConfig.paddingType);
            config.SetMatrixValue(1, 1, msConfig.nWeight);
            ConvolutionFilter filter = new ConvolutionFilter(config);
            Bitmap newB = filter.ApplyFilter(bitmap);
            newB = Sphere(newB, msConfig.sphereMidPoint);
            return newB;
        }
    }
}
