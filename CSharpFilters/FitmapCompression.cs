using HWFilters.Compression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HWFilters
{
    interface IFitmapCompression
    {
        void Write(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream);
        void Read(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream);
    }
    public class NoCompression : IFitmapCompression
    {
        public void Read(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
            int fullStep = cBytes.Length / 100;
            int halfStep = mBytes.Length / 100;
            int curOffset = 0;

            for (int i = 0; i < 100; ++i)
            {
                fStream.Read(cBytes, curOffset, fullStep);
                curOffset += fullStep;
            }
            curOffset = 0;
            for (int i = 0; i < 100; ++i)
            {
                fStream.Read(mBytes, curOffset, halfStep);
                curOffset += halfStep;
            }
            curOffset = 0;
            for (int i = 0; i < 100; ++i)
            {
                fStream.Read(yBytes, curOffset, halfStep);
                curOffset += halfStep;
            }
        }

        public void Write(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
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
        }
    }
    public class FullHuffmanCompression : IFitmapCompression
    {
        public void Read(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
            //Reading Dictionary Length
            byte[] dictionaryHeader = new byte[2];
            fStream.Read(dictionaryHeader, 0, 2);
            short dictionaryLength = BitConverter.ToInt16(dictionaryHeader, 0);
            Dictionary<HFCode, byte> readTable = new Dictionary<HFCode, byte>();
            //Reading Values and Code Lengths
            byte[] valueAndCodeLength = new byte[3 * dictionaryLength];
            fStream.Read(valueAndCodeLength, 0, 3 * dictionaryLength);

            //Reading Values and Codes
            BitStream bitStream = new BitStream(fStream);
            for (int i = 0; i < dictionaryLength; ++i)
            {
                byte value = valueAndCodeLength[3 * i];
                ushort codeLength = BitConverter.ToUInt16(valueAndCodeLength, 3 * i + 1);
                BitArray bits = bitStream.ReadBits(codeLength);
                HFCode code = HFCode.FromBits(bits);
                readTable.Add(code, value);
            }
            //Reconstruct tree
            HuffmanTree<byte> hfTree = HuffmanTree<byte>.ReconstructFromTable(readTable);
            hfTree.PrintTree();
            //Read values to bytes
            int bufferSize = 128;
            BitArray buffer = bitStream.ReadBits(bufferSize);

            int bufferIndex = 0;
            for (int i = 0; i < cBytes.Length; ++i)
            {
                HuffmanTreeNode curNode = hfTree.root;
                while (!(curNode is HuffmanLeafNode<byte>))
                {
                    if (bufferIndex == buffer.Length)
                    {
                        buffer = bitStream.ReadBits(buffer.Length);
                        bufferIndex = 0;
                    }
                    curNode = (buffer[bufferIndex]) ? curNode.right : curNode.left;

                    bufferIndex++;
                }
                cBytes[i] = ((HuffmanLeafNode<byte>)curNode).info;
            }
            for (int i = 0; i < mBytes.Length; ++i)
            {
                HuffmanTreeNode curNode = hfTree.root;
                while (!(curNode is HuffmanLeafNode<byte>))
                {
                    if (bufferIndex == buffer.Length)
                    {
                        buffer = bitStream.ReadBits(buffer.Length);
                        bufferIndex = 0;
                    }
                    curNode = (buffer[bufferIndex]) ? curNode.right : curNode.left;
                    bufferIndex++;
                }
                mBytes[i] = ((HuffmanLeafNode<byte>)curNode).info;
            }
            for (int i = 0; i < yBytes.Length; ++i)
            {
                HuffmanTreeNode curNode = hfTree.root;
                while (!(curNode is HuffmanLeafNode<byte>))
                {
                    if (bufferIndex == buffer.Length)
                    {
                        buffer = bitStream.ReadBits(buffer.Length);
                        bufferIndex = 0;
                    }
                    curNode = (buffer[bufferIndex]) ? curNode.right : curNode.left;
                    bufferIndex++;
                }
                yBytes[i] = ((HuffmanLeafNode<byte>)curNode).info;
            }
            fStream.Close();
        }

        public void Write(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
            byte[] fullBytes = new byte[cBytes.Length + mBytes.Length + yBytes.Length];
            System.Buffer.BlockCopy(cBytes, 0, fullBytes, 0, cBytes.Length);
            System.Buffer.BlockCopy(mBytes, 0, fullBytes, cBytes.Length, mBytes.Length);
            System.Buffer.BlockCopy(yBytes, 0, fullBytes, cBytes.Length + mBytes.Length, yBytes.Length);
            HuffmanCode<byte> fHComp = new HuffmanCode<byte>(fullBytes);
            Dictionary<byte, HFCode> table = fHComp.GetWritingTable();

            //Writing Dictionary Length
            byte[] dictionaryHeader = new byte[2];
            dictionaryHeader = BitConverter.GetBytes((short)table.Count);
            fStream.Write(dictionaryHeader, 0, 2);
            //Writing Value and Length foreach Dictionary entry
            byte[] dictionaryBitLength = new byte[2];
            foreach (KeyValuePair<byte, HFCode> pair in table)
            {
                fStream.WriteByte(pair.Key);
                dictionaryBitLength = BitConverter.GetBytes((ushort)pair.Value.length);
                fStream.Write(dictionaryBitLength, 0, 2);
            }
            //Writing Code for each Dictionary entry
            BitStream bitStream = new BitStream(fStream);
            foreach (KeyValuePair<byte, HFCode> pair in table)
            {
                bitStream.WriteBits(pair.Value.GetBitCode());
            }
            //Write Data
            long dataLength = 0;
            BitArray buffer;
            for (int i = 0; i < cBytes.Length; ++i)
            {
                buffer = table[cBytes[i]].GetBitCode();
                dataLength += buffer.Length;
                bitStream.WriteBits(buffer);
            }
            for (int i = 0; i < mBytes.Length; ++i)
            {
                buffer = table[mBytes[i]].GetBitCode();
                dataLength += buffer.Length;
                bitStream.WriteBits(buffer);
            }
            for (int i = 0; i < yBytes.Length; ++i)
            {
                buffer = table[yBytes[i]].GetBitCode();
                dataLength += buffer.Length;
                bitStream.WriteBits(buffer);
            }
            Console.WriteLine($"Written bits: {dataLength}: bytes: {(dataLength / 8) + 1} : avgBytes: {((double)dataLength / (double)fullBytes.Length)}");
            bitStream.Flush();
            fStream.Close();
        }
    }

    public class ChannelHuffmanCompression : IFitmapCompression
    {
        public void Read(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
            //Reading Dictionary Length
            //C
            byte[] dictionaryHeader = new byte[2];
            fStream.Read(dictionaryHeader, 0, 2);
            short cDictLength = BitConverter.ToInt16(dictionaryHeader, 0);
            Dictionary<HFCode, byte> cReadTable = new Dictionary<HFCode, byte>();
            //Reading Values and Code Lengths
            byte[] cValueAndCodeLength = new byte[3 * cDictLength];
            fStream.Read(cValueAndCodeLength, 0, 3 * cDictLength);
            //M
            dictionaryHeader = new byte[2];
            fStream.Read(dictionaryHeader, 0, 2);
            short mDictLength = BitConverter.ToInt16(dictionaryHeader, 0);
            Dictionary<HFCode, byte> mReadTable = new Dictionary<HFCode, byte>();
            //Reading Values and Code Lengths
            byte[] mValueAndCodeLength = new byte[3 * mDictLength];
            fStream.Read(mValueAndCodeLength, 0, 3 * mDictLength);
            //M
            dictionaryHeader = new byte[2];
            fStream.Read(dictionaryHeader, 0, 2);
            short yDictLength = BitConverter.ToInt16(dictionaryHeader, 0);
            Dictionary<HFCode, byte> yReadTable = new Dictionary<HFCode, byte>();
            //Reading Values and Code Lengths
            byte[] yValueAndCodeLength = new byte[3 * yDictLength];
            fStream.Read(yValueAndCodeLength, 0, 3 * yDictLength);
            //Reading Values and Codes
            BitStream bitStream = new BitStream(fStream);
            //C
            for (int i = 0; i < cDictLength; ++i)
            {
                byte value = cValueAndCodeLength[3 * i];
                ushort codeLength = BitConverter.ToUInt16(cValueAndCodeLength, 3 * i + 1);
                BitArray bits = bitStream.ReadBits(codeLength);
                HFCode code = HFCode.FromBits(bits);
                cReadTable.Add(code, value);
            }
            //M
            for (int i = 0; i < mDictLength; ++i)
            {
                byte value = mValueAndCodeLength[3 * i];
                ushort codeLength = BitConverter.ToUInt16(mValueAndCodeLength, 3 * i + 1);
                BitArray bits = bitStream.ReadBits(codeLength);
                HFCode code = HFCode.FromBits(bits);
                mReadTable.Add(code, value);
            }
            //Y
            for (int i = 0; i < yDictLength; ++i)
            {
                byte value = yValueAndCodeLength[3 * i];
                ushort codeLength = BitConverter.ToUInt16(yValueAndCodeLength, 3 * i + 1);
                BitArray bits = bitStream.ReadBits(codeLength);
                HFCode code = HFCode.FromBits(bits);
                yReadTable.Add(code, value);
                //Console.WriteLine($"{value}:{code.length}:{code.GetStringCode()}");
            }
            //Reconstruct tree
            //C
            HuffmanTree<byte> cTree = HuffmanTree<byte>.ReconstructFromTable(cReadTable);
            HuffmanTree<byte> mTree = HuffmanTree<byte>.ReconstructFromTable(mReadTable);
            HuffmanTree<byte> yTree = HuffmanTree<byte>.ReconstructFromTable(yReadTable);
            //Read values to bytes
            int bufferSize = 128;
            BitArray buffer = bitStream.ReadBits(bufferSize);

            int bufferIndex = 0;
            for (int i = 0; i < cBytes.Length; ++i)
            {
                HuffmanTreeNode curNode = cTree.root;
                while (!(curNode is HuffmanLeafNode<byte>))
                {
                    if (bufferIndex == buffer.Length)
                    {
                        buffer = bitStream.ReadBits(buffer.Length);
                        bufferIndex = 0;
                    }
                    curNode = (buffer[bufferIndex]) ? curNode.right : curNode.left;

                    bufferIndex++;
                }
                cBytes[i] = ((HuffmanLeafNode<byte>)curNode).info;
            }
            for (int i = 0; i < mBytes.Length; ++i)
            {
                HuffmanTreeNode curNode = mTree.root;
                while (!(curNode is HuffmanLeafNode<byte>))
                {
                    if (bufferIndex == buffer.Length)
                    {
                        buffer = bitStream.ReadBits(buffer.Length);
                        bufferIndex = 0;
                    }
                    curNode = (buffer[bufferIndex]) ? curNode.right : curNode.left;
                    bufferIndex++;
                }
                mBytes[i] = ((HuffmanLeafNode<byte>)curNode).info;
            }
            for (int i = 0; i < yBytes.Length; ++i)
            {
                HuffmanTreeNode curNode = yTree.root;
                while (!(curNode is HuffmanLeafNode<byte>))
                {
                    if (bufferIndex == buffer.Length)
                    {
                        buffer = bitStream.ReadBits(buffer.Length);
                        bufferIndex = 0;
                    }
                    curNode = (buffer[bufferIndex]) ? curNode.right : curNode.left;
                    bufferIndex++;
                }
                yBytes[i] = ((HuffmanLeafNode<byte>)curNode).info;
            }
            fStream.Close();
        }

        public void Write(byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
            HuffmanCode<byte> cFHComp = new HuffmanCode<byte>(cBytes);
            Dictionary<byte, HFCode> cTable = cFHComp.GetWritingTable();
            HuffmanCode<byte> mFHComp = new HuffmanCode<byte>(mBytes);
            Dictionary<byte, HFCode> mTable = mFHComp.GetWritingTable();
            HuffmanCode<byte> yFHComp = new HuffmanCode<byte>(yBytes);
            Dictionary<byte, HFCode> yTable = yFHComp.GetWritingTable();

            //Writing C Dictionary Length
            byte[] dictionaryHeader = new byte[2];
            dictionaryHeader = BitConverter.GetBytes((short)cTable.Count);
            fStream.Write(dictionaryHeader, 0, 2);
            //Writing Value and Length foreach Dictionary entry
            byte[] dictionaryBitLength = new byte[2];
            foreach (KeyValuePair<byte, HFCode> pair in cTable)
            {
                fStream.WriteByte(pair.Key);
                dictionaryBitLength = BitConverter.GetBytes((ushort)pair.Value.length);
                fStream.Write(dictionaryBitLength, 0, 2);
            }
            //Writing M Dictionary Length
            dictionaryHeader = BitConverter.GetBytes((short)mTable.Count);
            fStream.Write(dictionaryHeader, 0, 2);
            //Writing Value and Length foreach Dictionary entry
            foreach (KeyValuePair<byte, HFCode> pair in mTable)
            {
                fStream.WriteByte(pair.Key);
                dictionaryBitLength = BitConverter.GetBytes((ushort)pair.Value.length);
                fStream.Write(dictionaryBitLength, 0, 2);
            }
            //Writing Y Dictionary Length
            dictionaryHeader = BitConverter.GetBytes((short)yTable.Count);
            fStream.Write(dictionaryHeader, 0, 2);
            //Writing Value and Length foreach Dictionary entry
            foreach (KeyValuePair<byte, HFCode> pair in yTable)
            {
                fStream.WriteByte(pair.Key);
                dictionaryBitLength = BitConverter.GetBytes((ushort)pair.Value.length);
                fStream.Write(dictionaryBitLength, 0, 2);
            }

            //Writing Code for each Dictionary entry
            BitStream bitStream = new BitStream(fStream);
            foreach (KeyValuePair<byte, HFCode> pair in cTable)
            {
                bitStream.WriteBits(pair.Value.GetBitCode());
            }
            foreach (KeyValuePair<byte, HFCode> pair in mTable)
            {
                bitStream.WriteBits(pair.Value.GetBitCode());
            }
            foreach (KeyValuePair<byte, HFCode> pair in yTable)
            {
                bitStream.WriteBits(pair.Value.GetBitCode());
            }
            //Write Data
            long dataLength = 0;
            BitArray buffer;
            for (int i = 0; i < cBytes.Length; ++i)
            {
                buffer = cTable[cBytes[i]].GetBitCode();
                dataLength += buffer.Length;
                bitStream.WriteBits(buffer);
            }
            for (int i = 0; i < mBytes.Length; ++i)
            {
                buffer = mTable[mBytes[i]].GetBitCode();
                dataLength += buffer.Length;
                bitStream.WriteBits(buffer);
            }
            for (int i = 0; i < yBytes.Length; ++i)
            {
                buffer = yTable[yBytes[i]].GetBitCode();
                dataLength += buffer.Length;
                bitStream.WriteBits(buffer);
            }
            Console.WriteLine($"Written bits: {dataLength}: bytes: {(dataLength / 8) + 1} : avgBits: {((double)dataLength / (cBytes.Length + mBytes.Length + yBytes.Length))}");
            bitStream.Flush();
            fStream.Close();
        }
    }

    public static class FitmapCompression {
        public enum CompressionType
        {
            NoCompression, HuffmanFull, HuffmanPerChannel
        }
        private static IFitmapCompression GetAlgBasedOnType(int compressionType)
        {
            IFitmapCompression alg = null;
            switch (compressionType)
            {
                case (0):
                {
                    alg = new NoCompression();
                    break;
                }
                case (1):
                {
                    alg = new FullHuffmanCompression();
                    break;
                }
                case (2):
                {
                    alg = new ChannelHuffmanCompression();
                    break;
                }
                default:
                {
                    alg = new NoCompression();
                    break;
                }
            }
            return alg;
        }

        public static void Write(int compressionType, byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
            IFitmapCompression alg = GetAlgBasedOnType(compressionType);
            alg.Write(cBytes, mBytes, yBytes, fStream);
        }
        public static void Read(int compressionType, byte[] cBytes, byte[] mBytes, byte[] yBytes, FileStream fStream)
        {
            IFitmapCompression alg = GetAlgBasedOnType(compressionType);
            alg.Read(cBytes, mBytes, yBytes, fStream);
        }
    }
}
