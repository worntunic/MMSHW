using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            BitStreamTest bsTest = new BitStreamTest();
            bsTest.ReadWriteSameBuffer();
            bsTest.WriteDifBufferReadSameBuffer();
            bsTest.WriteSameBufferReadDifBuffer();

            Bitmap bmp = new Bitmap("C:/Users/lazar/Pictures/fitmap2.bmp");
            FitmapTest fmpTest = new FitmapTest(bmp);
            fmpTest.SaveThenLoadFitmap();
            Console.Read();
        }
    }
}
