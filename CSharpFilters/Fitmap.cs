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
        private const ushort commonHeaderSize = 10;
        private const byte latestVersion = 1;
        private const ushort versionZeroHeaderSize = 8;
        private const byte headerInfoData = 5;
        //Header data
        private ushort headerSize;
        private ushort fullSize;
        private byte fitmapVersion;
        private byte compressionType;
        private ushort width;
        private ushort height;
        //Constructed Data
        private byte[] cBytes;
        private byte[] mBytes;
        private byte[] yBytes;

        public int Width() => width;
        public int Height() => height;
        public byte[] CValues => cBytes;
        public byte[] MValues => mBytes;
        public byte[] YValues => yBytes;

        public Fitmap(Bitmap bitmap)
        {
            this.width = (ushort)bitmap.Width;
            this.height = (ushort)bitmap.Height;
            this.headerSize = commonHeaderSize;
            this.fitmapVersion = latestVersion;
            SetCompressionAlg(FitmapCompression.CompressionType.NoCompression);
            DownsampleImage(bitmap);
            fullSize = (ushort)(headerSize + cBytes.Length + mBytes.Length + yBytes.Length);
        }
        private Fitmap()
        {

        }
        public void SetCompressionAlg(FitmapCompression.CompressionType compressionType)
        {
            this.compressionType = (byte)compressionType;
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
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            FileStream fStream = new FileStream(fileName, FileMode.OpenOrCreate);
            /*byte[] header = new byte[headerSize];
            byte[] tmp = BitConverter.GetBytes(headerSize);*/
            fStream.Write(BitConverter.GetBytes(headerSize), 0, 2);
            fStream.Write(BitConverter.GetBytes(fullSize), 0, 2);
            fStream.Write(BitConverter.GetBytes(fitmapVersion), 0, 1);
            fStream.Write(BitConverter.GetBytes(compressionType), 0, 1);
            fStream.Write(BitConverter.GetBytes(width), 0, 2);
            fStream.Write(BitConverter.GetBytes(height), 0, 2);
            FitmapCompression.Write(compressionType, cBytes, mBytes, yBytes, fStream);
            fStream.Close();
        }
        private void ReadHeaderV0(FileStream fStream)
        {
            byte[] header = new byte[headerSize - 2];
            fStream.Read(header, 0, headerSize - 2);
            fullSize = BitConverter.ToUInt16(header, 0);
            width = BitConverter.ToUInt16(header, 2);
            height = BitConverter.ToUInt16(header, 4);
        }
        private void ReadHeaderInfo(FileStream fStream)
        {
            byte[] header = new byte[headerInfoData - 2];
            fStream.Read(header, 0, headerInfoData - 2);
            fullSize = BitConverter.ToUInt16(header, 0);
            fitmapVersion = header[2];
        }
        private void ReadHeaderBasedOnVersion(FileStream fStream)
        {
            byte[] header = new byte[headerSize - headerInfoData];
            if (fitmapVersion == 1)
            {
                fStream.Read(header, 0, header.Length);
                compressionType = header[0];
                width = BitConverter.ToUInt16(header, 1);
                height = BitConverter.ToUInt16(header, 3);
            }
        }
        public static Fitmap LoadFromFile(string fileName)
        {
            Fitmap f = new Fitmap();
            FileStream fStream = new FileStream(fileName, FileMode.Open);
            byte[] headerSize = new byte[2];
            fStream.Read(headerSize, 0, 2);
            f.headerSize = BitConverter.ToUInt16(headerSize, 0);
            if (f.headerSize == versionZeroHeaderSize)
            {
                f.ReadHeaderV0(fStream);
                f.fitmapVersion = 0;
                f.compressionType = 0;
            } else
            {
                f.ReadHeaderInfo(fStream);
                f.ReadHeaderBasedOnVersion(fStream);
            }
            int cLength = f.width * f.height;
            int sampledLength = f.GetSampledRowLength() * f.height;
            f.cBytes = new byte[cLength];
            f.mBytes = new byte[sampledLength];
            f.yBytes = new byte[sampledLength];
            FitmapCompression.Read(f.compressionType, f.cBytes, f.mBytes, f.yBytes, fStream);
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