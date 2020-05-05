using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HWFilters
{
    public class BitStream
    {
        FileStream fStream;
        private const int bufferLength = 256;
        byte[] writeBuffer = new byte[bufferLength];
        int writeBufferCount;
        UInt32 currentBuffer;
        int currentBufferCount = 0;
        BitArray alreadyReadBits = new BitArray(0);

        public BitStream(FileStream stream)
        {
            this.fStream = stream;
        }
        public BitArray ReadBits(int numberOfBits)
        {
            int bitsToRead = numberOfBits - alreadyReadBits.Length;
            BitArray bitArray = new BitArray(numberOfBits);
            int offset = alreadyReadBits.Length;
            //If already read bits has more values
            for (int i = 0; i < numberOfBits && alreadyReadBits.Length > 0; ++i)
            {
                bitArray[i] = alreadyReadBits[0];
                if (alreadyReadBits.Count != 1)
                {
                    for (int j = 0; j < alreadyReadBits.Length - 1; ++j)
                    {
                        alreadyReadBits[j] = alreadyReadBits[j + 1];
                    }
                }
                alreadyReadBits.Length--;
            }
            //alreadyReadBits.Length = alreadyReadBits.Length - numberOfBits;


            if (bitsToRead > 0)
            {
                int bytesToRead = ((bitsToRead - 1) / 8) + 1;
                byte[] readBuffer = new byte[bytesToRead];
                fStream.Read(readBuffer, 0, bytesToRead);
                BitArray readBits = new BitArray(readBuffer);
                alreadyReadBits.Length = readBits.Length - bitsToRead;
                for (int i = 0; i < readBits.Length; ++i)
                {
                    bool currentBit = readBits[7 - (i % 8) + ((i / 8) * 8)];
                    if (i < bitsToRead)
                    {
                        bitArray[offset + i] = currentBit;
                    }
                    else
                    {
                        alreadyReadBits[i - bitsToRead] = currentBit;
                    }
                }
            }

            return bitArray;
        }
        public void WriteBits(BitArray inputBits)
        {
            for (int i = 0; i < inputBits.Length; ++i)
            {
                currentBuffer = (currentBuffer << 1) | Convert.ToUInt32(inputBits[i]);
                currentBufferCount++;
                if (currentBufferCount == 32)
                {
                    CopyToWriteBuffer();
                }
            }
        }

        private void CopyToWriteBuffer(bool forceFlush = false)
        {
            writeBuffer[writeBufferCount] = (byte)(currentBuffer >> 0x18);
            writeBuffer[writeBufferCount + 1] = (byte)(currentBuffer >> 0x10);
            writeBuffer[writeBufferCount + 2] = (byte)(currentBuffer >> 8);
            writeBuffer[writeBufferCount + 3] = (byte)(currentBuffer);
            currentBufferCount = 0;
            writeBufferCount += 4;
            if (writeBufferCount == bufferLength || forceFlush)
            {
                fStream.Write(writeBuffer, 0, writeBufferCount);
                writeBufferCount = 0;
            }
        }
        public void Flush()
        {
            int leftover = 32 - currentBufferCount;
            currentBuffer = currentBuffer << leftover;
            CopyToWriteBuffer(true);
        }
    }
}
