using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace HWFilters
{
    /// <summary>
    /// Najlakši način da se izvrše metode za domaći je pozivanjem dve statičke funkcije:
    /// 
    /// HWFilters.Fitmap.SaveBitmap(bitmap, fileName);
    /// Gde je bitmap (tipa Bitmap) slika koju pretvaramo u fitmap i snimamo u putanju fileName (tipa string)
    /// 
    /// HWFilters.Fitmap.LoadBitmapFromFile(fileName);
    /// Učitava Fitmap iz putanje fileName (tipa string) i pretvara ga u objekat tipa Bitmap (koji vraća)
    /// </summary>
    public class Fitmap
    {      
        //Constants
        private const ushort commonHeaderSize = 8;
        //Header data
        private ushort fullSize;
        private ushort headerSize;
        //Image data
        private ushort width;
        private ushort height;
        //Constructed Data
        private byte[] cBytes;
        private byte[] mBytes;
        private byte[] yBytes;

        public Fitmap(Bitmap bitmap)
        {
            this.width = (ushort)bitmap.Width;
            this.height = (ushort)bitmap.Height;
            this.headerSize = commonHeaderSize;
            DownsampleImage(bitmap);
            fullSize = (ushort)(headerSize + cBytes.Length + mBytes.Length + yBytes.Length);
        }
        private Fitmap()
        {

        }
        public Bitmap GetBitmap()
        {
            return UpsampleImage();
        }

        #region Sampling
        private int GetSampledRowLength()
        {
            return (((width - 1) / 8) + 1) * 4;
        }

        private void DownsampleImage(Bitmap bitmap)
        {
            int byteCount = width * height;
            int sampledLength = GetSampledRowLength() * height;
            cBytes = new byte[byteCount];
            mBytes = new byte[sampledLength];
            yBytes = new byte[sampledLength];

            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int sampledIndex = 0;
                int unsampledIndex = 0;
                fixed (byte* cB = cBytes, mB = mBytes, yB = yBytes) {
                    int nOffset = stride - bitmap.Width * 3;
                    int sampleBatchOffset = bitmap.Width % 4;
                    bool takeFirst = true;
                    for (int y = 0; y < height; ++y)
                    {
                        takeFirst = (y % 2 == 0);
                        for (int x = 0; x < width; ++x)
                        {
                            int modVal = x % 8;
                            if (takeFirst && modVal < 4 || !takeFirst && modVal >= 4)
                            {
                                mB[sampledIndex] = (byte)(255 - p[1]);
                                yB[sampledIndex] = (byte)(255 - p[0]);
                                sampledIndex++;
                            }
                            cB[unsampledIndex] = (byte) (255 - p[2]);
                            unsampledIndex++;
                            p += 3;
                        }

                        //Pad samples to 4
                        while (sampledIndex % 4 != 0)
                        {
                            mB[sampledIndex] = mB[sampledIndex - 1];
                            yB[sampledIndex] = yB[sampledIndex - 1];
                            sampledIndex++;
                        }
                        p += nOffset;
                    }
                }
            }
            bitmap.UnlockBits(bmData);
        }

        private Bitmap UpsampleImage()
        {
            Bitmap bitmap = new Bitmap(width, height);
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int sampledRowLength = GetSampledRowLength();
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int sampledIndex = 0;
                int unsampledIndex = 0;
                fixed (byte* cB = cBytes, mB = mBytes, yB = yBytes)
                {
                    int nOffset = stride - bitmap.Width * 3;
                    for (int y = 0; y < bitmap.Height; ++y)
                    {
                        for (int x = 0; x < bitmap.Width; ++x)
                        {
                            sampledIndex = ((x / 8) * 4) + (x % 4) + y * sampledRowLength;
                            p[1] = (byte)(255 - mB[sampledIndex]);
                            p[0] = (byte)(255 - yB[sampledIndex]);

                            p[2] = (byte)(255 - cB[unsampledIndex]);
                            unsampledIndex++;
                            p += 3;
                        }
                        p += nOffset;
                    }
                }
            }
            bitmap.UnlockBits(bmData);
            return bitmap;
        }

        #endregion
        #region File Management
        public void SaveToFile(string fileName)
        {
            FileStream fStream = new FileStream(fileName, FileMode.OpenOrCreate);
            /*byte[] header = new byte[headerSize];
            byte[] tmp = BitConverter.GetBytes(headerSize);*/
            fStream.Write(BitConverter.GetBytes(headerSize), 0, 2);
            fStream.Write(BitConverter.GetBytes(fullSize), 0, 2);
            fStream.Write(BitConverter.GetBytes(width), 0, 2);
            fStream.Write(BitConverter.GetBytes(height), 0, 2);
            int fullStep = cBytes.Length / 100;
            int halfStep = mBytes.Length / 100;
            int curOffset = 0;
            for (int i = 0; i < 100; ++i)
            {
                fStream.Write(cBytes, curOffset, fullStep);
                curOffset += fullStep;
            }
            curOffset = 0;
            for (int i = 0; i < 100; ++i)
            {
                fStream.Write(mBytes, curOffset, halfStep);
                curOffset += halfStep;
            }
            curOffset = 0;
            for (int i = 0; i < 100; ++i)
            {
                fStream.Write(yBytes, curOffset, halfStep);
                curOffset += halfStep;
            }
            fStream.Close();
        }
        public static Fitmap LoadFromFile(string fileName)
        {
            Fitmap f = new Fitmap();
            FileStream fStream = new FileStream(fileName, FileMode.Open);
            byte[] headerSize = new byte[2];
            fStream.Read(headerSize, 0, 2);
            f.headerSize = BitConverter.ToUInt16(headerSize, 0);
            byte[] header = new byte[f.headerSize - 2];
            fStream.Read(header, 0, f.headerSize - 2);
            f.fullSize = BitConverter.ToUInt16(header, 0);
            f.width = BitConverter.ToUInt16(header, 2);
            f.height = BitConverter.ToUInt16(header, 4);

            int cLength = f.width * f.height;
            int sampledLength = f.GetSampledRowLength() * f.height;
            f.cBytes = new byte[cLength];
            f.mBytes = new byte[sampledLength];
            f.yBytes = new byte[sampledLength];

            int fullStep = cLength / 100;
            int halfStep = sampledLength / 100;
            int curOffset = 0;

            for (int i = 0; i < 100; ++i)
            {
                fStream.Read(f.cBytes, curOffset, fullStep);
                curOffset += fullStep;
            }
            curOffset = 0;
            for (int i = 0; i < 100; ++i)
            {
                fStream.Read(f.mBytes, curOffset, halfStep);
                curOffset += halfStep;
            }
            curOffset = 0;
            for (int i = 0; i < 100; ++i)
            {
                fStream.Read(f.yBytes, curOffset, halfStep);
                curOffset += halfStep;
            }
            fStream.Close();

            return f;
        }
        #endregion
        #region Homework Assignments
        public static Fitmap SaveBitmap(Bitmap bmp, string fileName)
        {
            Fitmap f = new Fitmap(bmp);
            f.SaveToFile(fileName);
            return f;
        }
        public static Bitmap LoadBitmapFromFile(string fileName)
        {
            Fitmap f = LoadFromFile(fileName);
            return f.GetBitmap();
        }
        #endregion
    }
}