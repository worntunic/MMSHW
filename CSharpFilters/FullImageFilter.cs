using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HWFilters
{
    interface IFullImageFilter
    {
        void PrepareData();
        bool ApplyFilter(Bitmap bitmap);
    }

    class NeighbourMergeFilter : IFullImageFilter
    {
        private int neighbourHoodWidth;

        public NeighbourMergeFilter(int neighbourHoodWidth)
        {
            this.neighbourHoodWidth = neighbourHoodWidth;
        }
        public bool ApplyFilter(Bitmap bitmap)
        {
            Bitmap output = new Bitmap(bitmap);
            for (int i = 0; i < bitmap.Width; ++i)
            {
                Console.WriteLine($"Row:{i}");
                for (int j = 0; j < bitmap.Height; ++j)
                {
                    bitmap.SetPixel(i, j, GetPixelFromNeighbourHood(output, i, j));
                }
            }
            return true;
        }

        private Color GetPixelFromNeighbourHood(Bitmap bitmap, int x, int y)
        {
            int left = MathUtils.Clamp(x - neighbourHoodWidth, 0, bitmap.Width - 1);
            int right = MathUtils.Clamp(x + neighbourHoodWidth, 0, bitmap.Width - 1);
            int top = MathUtils.Clamp(y - neighbourHoodWidth, 0, bitmap.Height - 1);
            int bottom = MathUtils.Clamp(y + neighbourHoodWidth, 0, bitmap.Height - 1);
            int r = 0, g = 0, b = 0;
            int neighbourCount = 0;
            for (int i = left; i <= right; ++i)
            {
                for(int j = top; j <= bottom; ++j)
                {
                    Color color = bitmap.GetPixel(i, j);
                    int factor = (neighbourHoodWidth - Math.Abs(x - i)) * (neighbourHoodWidth - Math.Abs(y - j));
                    r += color.R * factor;
                    g += color.G * factor;
                    b += color.B * factor;
                    neighbourCount++;
                }
            }
            r = MathUtils.Clamp(r / neighbourCount, 0, 255);
            g = MathUtils.Clamp(g / neighbourCount, 0, 255);
            b = MathUtils.Clamp(b / neighbourCount, 0, 255);
            return Color.FromArgb(r, g, b);
        }

        public void PrepareData()
        {
            
        }

        public static bool ApplyNeighbourhoodFilter(Bitmap bitmap, int neighbourhoodWidth)
        {
            NeighbourMergeFilter nmFilter = new NeighbourMergeFilter(neighbourhoodWidth);
            nmFilter.PrepareData();
            return nmFilter.ApplyFilter(bitmap);
        }
    }

    class PixelateChunkFilter : IFullImageFilter
    {
        public enum GradientType
        {
            No, Diagonal, RadialChunk, RadialImage
        }
        private int chunkWidth, chunkHeight;
        private GradientType gradientType;
        public PixelateChunkFilter(int chunkWidth, int chunkHeight, GradientType gradientType)
        {
            this.chunkWidth = chunkWidth;
            this.chunkHeight = chunkHeight;
            this.gradientType = gradientType;
        }

        public bool ApplyFilter(Bitmap bitmap)
        {
            int chunkCountW = (int)Math.Floor((double)bitmap.Width / chunkWidth);
            int chunkCountH = (int)Math.Floor((double)bitmap.Height / chunkHeight);

            for (int chunkI = 0; chunkI <= chunkCountW; ++chunkI)
            {
                for (int chunkJ = 0; chunkJ < chunkCountH; ++chunkJ)
                {
                    Console.WriteLine($"Chunk{chunkI * chunkCountH + chunkJ} out of {(chunkCountW + 1) * chunkCountH}");
                    int chunkStartX = chunkI * chunkWidth;
                    int chunkStartY = chunkJ * chunkHeight;
                    SetColorForChunk(bitmap, chunkStartX, chunkStartY);
                }
            }
            return true;
        }

        private void SetColorForChunk(Bitmap bitmap, int startX, int startY)
        {
            int endX = MathUtils.Clamp(startX + chunkWidth, 0, bitmap.Width - 1);
            int endY = MathUtils.Clamp(startY + chunkHeight, 0, bitmap.Height - 1);
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            int r, g, b;
            int pixelCount = 0;
            for (int x = startX; x <= endX; ++x)
            {
                for (int y = startY; y <= endY; ++y)
                {
                    pixelCount++;
                    Color color = bitmap.GetPixel(x, y);
                    totalR += color.R;
                    totalG += color.G;
                    totalB += color.B;
                    /*if (filterGradually)
                    {
                        r = MathUtils.Clamp(totalR / pixelCount, 0, 255);
                        g = MathUtils.Clamp(totalG / pixelCount, 0, 255);
                        b = MathUtils.Clamp(totalB / pixelCount, 0, 255);
                        bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }*/
                }
            }
            if (pixelCount == 0)
            {
                pixelCount = 1;
            }
            r = MathUtils.Clamp(totalR / pixelCount, 0, 255);
            g = MathUtils.Clamp(totalG / pixelCount, 0, 255);
            b = MathUtils.Clamp(totalB / pixelCount, 0, 255);
            Color fullColor = Color.FromArgb(r, g, b);
            for (int x = startX; x <= endX; ++x)
            {
                for (int y = startY; y <= endY; ++y)
                {
                    if (gradientType != GradientType.No)
                    {
                        Color prevColor = bitmap.GetPixel(x, y);
                        float t = GetTForGradient(startX, endX, x, startY, endY, y, bitmap.Size);
                        int newR = MathUtils.Clamp(MathUtils.Lerp(prevColor.R, r, t), 0, 255);
                        int newG = MathUtils.Clamp(MathUtils.Lerp(prevColor.G, g, t), 0, 255);
                        int newB = MathUtils.Clamp(MathUtils.Lerp(prevColor.B, b, t), 0, 255);
                        bitmap.SetPixel(x, y, Color.FromArgb(newR, newG, newB));
                    } else
                    {
                        bitmap.SetPixel(x, y, fullColor);
                    }

                }
            }
        }

        private float GetTForGradient(int startX, int endX, int x, int startY, int endY, int y, Size imageSize)
        {
            float t = 0f;
            if (gradientType == GradientType.Diagonal)
            {
                float tX = (((float)x - startX) / ((float)endX - startX));
                float tY = (((float)y - startY) / ((float)endY - startY));
                t = (tX + tY) / 2;
            } else if (gradientType == GradientType.RadialChunk)
            {
                float tXMid = ((float)(startX + endX) / 2) - startX;
                float tYMid = ((float)(startY + endY) / 2) - startY;

                float tX = 1 - Math.Abs(x - startX - tXMid) / tXMid;
                float tY = 1 - Math.Abs(y - startY - tYMid) / tYMid;
                
                t = (tX + tY) / 2;
                t = 1 - t;
            } else if (gradientType == GradientType.RadialImage)
            {
                float tXMid = ((float)(startX + endX) / 2);
                float tYMid = ((float)(startY + endY) / 2);
                float imageXMid = (imageSize.Width) / 2;
                float imageYMid = (imageSize.Height) / 2;
                float tX = Math.Abs(tXMid - imageXMid) / imageXMid;
                float tY = Math.Abs(tYMid - imageYMid) / imageYMid;
                t = (tX + tY) / 2;
                if (x == startX && y == startY)
                {
                    Console.WriteLine($"t:{t} x:{tXMid} y:{tYMid}");
                }
            }
            return t;
        }
        public void PrepareData()
        {
        }

        public static bool ApplyPixelChunkFilter(Bitmap bitmap, int chunkWidth, int chunkHeight, GradientType gradient)
        {
            PixelateChunkFilter pcFilter = new PixelateChunkFilter(chunkWidth, chunkHeight, gradient);
            pcFilter.PrepareData();
            return pcFilter.ApplyFilter(bitmap);
        }
    }
}
