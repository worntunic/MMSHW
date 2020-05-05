using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HWFilters;

namespace Testing
{
    class BitStreamTest
    {
        private const string filePath = "D:/test.test";

        public void ReadWriteSameBuffer()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileStream writeStream = new FileStream(filePath, FileMode.OpenOrCreate);
            BitStream bitWriteStream = new BitStream(writeStream);

            BitArray oneArray = new BitArray(new bool[] { false, false, true, false, true, false, true });
            BitArray twoArray = new BitArray(new bool[] { true, true, false });
            BitArray threeArray = new BitArray(new bool[] { false });

            bitWriteStream.WriteBits(oneArray);
            bitWriteStream.WriteBits(twoArray);
            bitWriteStream.WriteBits(threeArray);

            bitWriteStream.Flush();
            writeStream.Close();

            FileStream readStream = new FileStream(filePath, FileMode.Open);
            BitStream bitReadStream = new BitStream(readStream);

            BitArray oneTestArr = bitReadStream.ReadBits(oneArray.Length);
            BitArray twoTestArr = bitReadStream.ReadBits(twoArray.Length);
            BitArray threeTestArr = bitReadStream.ReadBits(threeArray.Length);

            bool differ = false;
            
            for (int i = 0; i < oneArray.Length; ++i)
            {
                if (oneArray[i] != oneTestArr[i])
                {
                    differ = true;
                }
            }
            for (int i = 0; i < twoArray.Length; ++i)
            {
                if (twoArray[i] != twoTestArr[i])
                {
                    differ = true;
                }
            }
            for (int i = 0; i < threeArray.Length; ++i)
            {
                if (threeArray[i] != threeTestArr[i])
                {
                    differ = true;
                }
            }
            readStream.Close();

            if (differ)
            {
                Console.WriteLine("Error: Same Write and Read Buffer sizes: Original and written arrays differ");
            } else
            {
                Console.WriteLine("Success: Same Write and Read Buffer sizes: Original and written arrays are the same");
            }
        }

        public void WriteDifBufferReadSameBuffer()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileStream writeStream = new FileStream(filePath, FileMode.OpenOrCreate);
            BitStream bitWriteStream = new BitStream(writeStream);

            Random bufferSizeRandom = new Random();
            int total = 2048;
            List<BitArray> buffers = new List<BitArray>();

            int curBufferSize = bufferSizeRandom.Next(1, total);
            int curBufferIndex = 0;
            buffers.Add(new BitArray(curBufferSize));

            for (int i = 0; i < total; ++i)
            {
                if (curBufferIndex >= curBufferSize)
                {
                    bitWriteStream.WriteBits(buffers[buffers.Count - 1]);
                    curBufferSize = bufferSizeRandom.Next(1, total - i + 1);
                    curBufferIndex = 0;

                    buffers.Add(new BitArray(curBufferSize));

                }
                buffers[buffers.Count - 1][curBufferIndex] = bufferSizeRandom.Next() % 2 == 0;
                curBufferIndex++;
            }
            bitWriteStream.WriteBits(buffers[buffers.Count - 1]);
            bitWriteStream.Flush();
            writeStream.Close();

            FileStream readStream = new FileStream(filePath, FileMode.OpenOrCreate);
            BitStream bitReadStream = new BitStream(readStream);

            int readBufferSize = 128;
            BitArray buffer = bitReadStream.ReadBits(readBufferSize);
            int curReadIndex = 0;
            int curWriteBufferIndex = 0;
            int curWriteBuffer = 0;
            bool differ = false;

            for (int i = 0; i < total; ++i)
            {
                if (curReadIndex == readBufferSize)
                {
                    curReadIndex = 0;
                    buffer = bitReadStream.ReadBits(readBufferSize);
                }
                if (curWriteBufferIndex == buffers[curWriteBuffer].Count)
                {
                    curWriteBuffer++;
                    curWriteBufferIndex = 0;
                }
                if (buffer[curReadIndex] != buffers[curWriteBuffer][curWriteBufferIndex])
                {
                    differ = true;
                }
                curReadIndex++;
                curWriteBufferIndex++;
            }
            readStream.Close();
            if (differ)
            {
                Console.WriteLine("Error: Different Write Buffers, Same Read Buffer: Original and written arrays differ");
            }
            else
            {
                Console.WriteLine("Success: Different Write Buffers, Same Read Buffer: Original and written arrays are the same");
            }
        }

        public void WriteSameBufferReadDifBuffer()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileStream writeStream = new FileStream(filePath, FileMode.OpenOrCreate);
            BitStream bitWriteStream = new BitStream(writeStream);
            int total = 1000000;
            int writeBufferSize = 2048;
            List<BitArray> buffers = new List<BitArray>();
            Random random = new Random();

            int curBufferIndex = 0;
            buffers.Add(new BitArray(writeBufferSize));

            for (int i = 0; i < total; ++i)
            {
                if (curBufferIndex >= writeBufferSize)
                {
                    bitWriteStream.WriteBits(buffers[buffers.Count - 1]);
                    curBufferIndex = 0;
                    buffers.Add(new BitArray(writeBufferSize));

                }
                buffers[buffers.Count - 1][curBufferIndex] = random.Next() % 2 == 0;
                curBufferIndex++;
            }
            bitWriteStream.WriteBits(buffers[buffers.Count - 1]);
            bitWriteStream.Flush();
            writeStream.Close();

            FileStream readStream = new FileStream(filePath, FileMode.OpenOrCreate);
            BitStream bitReadStream = new BitStream(readStream);

            int readBufferSize = random.Next(1, total - 1);
            BitArray buffer = bitReadStream.ReadBits(readBufferSize);
            int curReadIndex = 0;
            int curWriteBuffer = 0;
            int curWriteBufferIndex = 0;
            bool differ = false;

            for (int i = 0; i < total; ++i)
            {
                if (curReadIndex == readBufferSize)
                {
                    curReadIndex = 0;
                    readBufferSize = random.Next(1, total - i + 1);
                    buffer = bitReadStream.ReadBits(readBufferSize);
                }
                if (curWriteBufferIndex == buffers[curWriteBuffer].Count)
                {
                    curWriteBuffer++;
                    curWriteBufferIndex = 0;
                }
                if (buffer[curReadIndex] != buffers[curWriteBuffer][curWriteBufferIndex])
                {
                    Console.WriteLine($"Differing on index {i} : original({buffers[curWriteBuffer][curWriteBufferIndex]} : written({buffer[curReadIndex]}))");
                    differ = true;
                }
                curReadIndex++;
                curWriteBufferIndex++;
            }
            readStream.Close();
            if (differ)
            {
                Console.WriteLine("Error: Same Write Buffers, Dif Read Buffer: Original and written arrays differ");
            }
            else
            {
                Console.WriteLine("Success: Same Write Buffers, Dif Read Buffer: Original and written arrays are the same");
            }
        }
    }
}
