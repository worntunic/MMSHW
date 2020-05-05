using HWFilters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class FitmapTest
    {
        Bitmap bitmap;
        
        private const string filePath = "D:/fTest.fmp1";
        public FitmapTest(Bitmap bmp)
        {
            this.bitmap = bmp;
        }

        public void SaveThenLoadFitmap()
        {
            Fitmap f = new Fitmap(bitmap);
            f.SetCompressionAlg(FitmapCompression.CompressionType.HuffmanFull);
            f.SaveToFile(filePath);

            Fitmap loadedF = Fitmap.LoadFromFile(filePath);
            //Check C Bytes
            byte[] curBytes, curLoaded;
            curBytes = f.CValues;
            curLoaded = loadedF.CValues;
            for (int i = 0; i < curBytes.Length; ++i)
            {
                if (curBytes[i] != curLoaded[i])
                {
                    Console.WriteLine($"C differs ({i % f.Width()},{i / f.Height()}): orig:({curBytes[i]}):loaded:({curLoaded[i]})");
                }
            }
            curBytes = f.MValues;
            curLoaded = loadedF.MValues;
            for (int i = 0; i < curBytes.Length; ++i)
            {
                if (curBytes[i] != curLoaded[i])
                {
                    Console.WriteLine($"M differs ({i % f.Width()},{i / f.Height()}): orig:({curBytes[i]}):loaded:({curLoaded[i]})");
                }
            }
            curBytes = f.YValues;
            curLoaded = loadedF.YValues;
            for (int i = 0; i < curBytes.Length; ++i)
            {
                if (curBytes[i] != curLoaded[i])
                {
                    Console.WriteLine($"Y differs ({i % f.Width()},{i / f.Height()}): orig:({curBytes[i]}):loaded:({curLoaded[i]})");
                }
            }
        }
    }
}
