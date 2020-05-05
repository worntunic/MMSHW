using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HWFilters.Compression
{
    struct HFCode
    {
        public int length;
        BitArray bitCode;
        
        public static HFCode FromBits(BitArray bitArray)
        {
            HFCode hfCode = new HFCode();
            hfCode.bitCode = bitArray;
            hfCode.length = bitArray.Length;
            return hfCode;
        }
        public static HFCode GetStartingCode(bool startingBit)
        {
            HFCode hfCode = new HFCode();
            hfCode.length = 1;
            hfCode.bitCode = new BitArray(hfCode.length);
            hfCode.bitCode[0] = startingBit;
            //hfCode.GenerateBytes();
            return hfCode;
        }
        private static HFCode GetNextCode(BitArray prevCode, bool newBit)
        {
            HFCode hfCode = new HFCode();
            hfCode.length = prevCode.Length + 1;
            hfCode.bitCode = new BitArray(prevCode);
            hfCode.bitCode.Length = hfCode.length;
            hfCode.bitCode[hfCode.length - 1] = newBit;
            //hfCode.GenerateBytes();
            return hfCode;
        }

        public HFCode GetLeft()
        {
            return GetNextCode(bitCode, false);
        }
        public HFCode GetRight()
        {
            return GetNextCode(bitCode, true);
        }
        public BitArray GetBitCode()
        {
            return bitCode;
        }
        public string GetStringCode()
        {
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < bitCode.Length; ++i)
            {
                strBuilder.Append(bitCode[i] ? "1" : "0");
            }
            return strBuilder.ToString();
        }
    }
    interface IHuffmanNode
    {
        double GetFrequency();
    }
    class HuffmanListNode<T> : IHuffmanNode
    {
        public T info;
        public double frequency;
        public HuffmanListNode<T> next;
        public HuffmanListNode<T> prev;

        public HuffmanListNode(T info, double frequency) {
            this.info = info;
            this.frequency = frequency;
            this.next = this.prev = null;
        }

        public bool EqualsInfo(T info)
        {
            return EqualityComparer<T>.Default.Equals(this.info, info);
        }

        public double GetFrequency()
        {
            return frequency;
        }
    }
    class HuffmanList<T>
    {
        private HuffmanListNode<T> start;
        HuffmanListNode<T> end;
        public int Count { get; private set; } = 0;
        public HuffmanListNode<T> this[int index]
        {
            get => GetNodeAt(index);
        }
        public HuffmanListNode<T> this[T info] {
            get => FindNode(info);
        }
        private HuffmanListNode<T> GetNodeAt(int index)
        {
            int i = 0;
            HuffmanListNode<T> node = start;
            while (i != index && node != null)
            {
                node = node.next;
                i++;
            }
            if (node == null)
            {
                throw new IndexOutOfRangeException();
            }
            return node;
        }
        private void ReplaceNodeAt(int index, HuffmanListNode<T> node)
        {
            HuffmanListNode<T> replacing = GetNodeAt(index);
            if (index != 0)
            {
                replacing.prev.next = node;
            } else {
                start = node;
            }
            if (index != Count - 1)
            {
                replacing.next.prev = node;
            } else
            {
                end = node;
            }
            node.prev = replacing.prev;
            node.next = replacing.next;
        }
        private void ChainAfter(HuffmanListNode<T> newNode, HuffmanListNode<T> afterThis)
        {
            newNode.next = afterThis.next;
            newNode.prev = afterThis;
            afterThis.next = newNode;
            if (newNode.next == null)
            {
                end = newNode;
            } else
            {
                newNode.next.prev = newNode;
            }
        }
        private void ChainBefore(HuffmanListNode<T> newNode, HuffmanListNode<T> beforeThis)
        {
            newNode.next = beforeThis;
            newNode.prev = beforeThis.prev;
            beforeThis.prev = newNode;
            if (newNode.prev == null)
            {
                start = newNode;
            } else
            {
                newNode.prev.next = newNode;
            }
        }
        public void Add(T value, double frequency)
        {
            HuffmanListNode<T> newNode = new HuffmanListNode<T>(value, frequency);
            if (start == null)
            {
                start = newNode;
                end = newNode;
            } else
            {
                HuffmanListNode<T> node = end;
                while(node != null && newNode.frequency > node.frequency)
                {
                    node = node.prev;
                }
                if (node == null)
                {
                    /*newNode.next = start;
                    start = newNode;*/
                    ChainBefore(newNode, start);
                } else
                {
                    ChainAfter(newNode, node);
                }
            }
            Count++;
        }
        public void SetFrequency(T info, double freq = 1)
        {
            SetFrequency(FindNode(info), freq);   
        }
        public void SetFrequency(HuffmanListNode<T> node, double freq = 1)
        {
            node.frequency = freq;
            RemoveNode(node);
            Add(node.info, node.frequency);
        }
        private HuffmanListNode<T> FindNode(T info)
        {
            HuffmanListNode<T> node = start;
            while (node != null || !node.EqualsInfo(info))
            {
                node = node.next;
            }
            if (node == null)
            {
                throw new KeyNotFoundException();
            } else
            {
                return node;
            }
        }
        public void RemoveNode(HuffmanListNode<T> node)
        {
            if (node.prev == null && node.next == null)
            {
                Console.Write("");
            }
            if (node.prev == null)
            {
                start = node.next;
            } else
            {
                node.prev.next = node.next;
            }
            if (node.next == null)
            {
                end = node.prev;
            } else
            {
                node.next.prev = node.prev;
            }
            Count--;
        }
        public void Clear()
        {
            start = null;
            end = null;
        }
        public void GetTwoLowestFreqNodes(out HuffmanListNode<T> first, out HuffmanListNode<T> second)
        {

            if (Count >= 1)
            {
                first = end;
                if (Count >= 2)
                {
                    second = end.prev;
                } else
                {
                    second = null;
                }
            } else
            {
                first = null;
                second = null;
            }

        }
    }
    class HuffmanTreeNode : IHuffmanNode
    {
        public double frequency;
        public HuffmanTreeNode left;
        public HuffmanTreeNode right;
        //public HuffmanTreeNode parent;

        public HuffmanTreeNode()
        {

        }
        public HuffmanTreeNode(double frequency)
        {
            this.frequency = frequency;
        }

        public double GetFrequency()
        {
            return frequency;
        }
        public virtual void PrintNode(HFCode code)
        {
            left.PrintNode(code.GetLeft());
            right.PrintNode(code.GetRight());
        }
        public virtual void AddToReadingTable<T>(HFCode inCode, Dictionary<HFCode, T> table)
        {
            left.AddToReadingTable(inCode.GetLeft(), table);
            right.AddToReadingTable(inCode.GetRight(), table);
        }
        public virtual void AddToWritingTable<T>(HFCode inCode, Dictionary<T, HFCode> table)
        {
            left.AddToWritingTable(inCode.GetLeft(), table);
            right.AddToWritingTable(inCode.GetRight(), table);
        }
    }
    class HuffmanLeafNode<T> : HuffmanTreeNode
    {
        public T info;

        public HuffmanLeafNode(T info, double frequency)
        {
            this.info = info;
            this.frequency = frequency;
            left = null;
            right = null;
        }
        public override void PrintNode(HFCode code)
        {
            Console.WriteLine($"{info}:{code.length}:{code.GetStringCode()}:{frequency.ToString()}");
            //base.PrintNode(inCode);
        }
        public override void AddToReadingTable<T>(HFCode inCode, Dictionary<HFCode, T> table)
        {
            table.Add(inCode, (T)(object)info);
        }
        public override void AddToWritingTable<T>(HFCode inCode, Dictionary<T, HFCode> table)
        {
            T val = (T)(object)info;
            table.Add(val, inCode);
        }
    }
    class HuffmanTree<T>
    {
        public HuffmanTreeNode root;
        private Dictionary<HFCode, T> readingTable;
        private Dictionary<T, HFCode> writingTable;

        private HuffmanTree()
        {

        }
        public HuffmanTree(HuffmanLeafNode<T> firstInit, HuffmanLeafNode<T> secondInit)
        {
            root = new HuffmanTreeNode();
            if (firstInit.frequency > secondInit.frequency)
            {
                root.left = secondInit;
                root.right = firstInit;
            } else
            {
                root.left = firstInit;
                root.right = secondInit;
            }
            CalcRootFrequency();
        }
        public double GetRootFrequency()
        {
            return root.frequency;
        }
        private void CalcRootFrequency()
        {
            root.frequency = root.left.frequency + root.right.frequency;
        }

        public void Combine(HuffmanTreeNode node)
        {
            HuffmanTreeNode newRoot = new HuffmanTreeNode();
            if (node.frequency > root.frequency)
            {
                newRoot.left = root;
                newRoot.right = node;
            } else
            {
                newRoot.left = node;
                newRoot.right = root;
            }
            root = newRoot;
            CalcRootFrequency();
        }
        public void PrintTree()
        {
            root.left.PrintNode(HFCode.GetStartingCode(false));
            root.right.PrintNode(HFCode.GetStartingCode(true));
        }
        public static HuffmanTree<T> ReconstructFromTable(Dictionary<HFCode, T> table)
        {
            HuffmanTree<T> hfTree = new HuffmanTree<T>();
            hfTree.root = new HuffmanTreeNode(-1);
            foreach (KeyValuePair<HFCode, T> pair in table)
            {
                BitArray bitCode = pair.Key.GetBitCode();
                HuffmanTreeNode curNode = hfTree.root;
                HuffmanTreeNode prevNode = curNode;
                for (int i = 0; i < bitCode.Length; ++i)
                {
                    //Go left or right depending on bitCode
                    prevNode = curNode;
                    curNode = (bitCode[i]) ? curNode.right : curNode.left;
                    if (curNode == null)
                    {
                        if (i == bitCode.Length - 1) //Create leaf
                        {
                            HuffmanLeafNode<T> hfLeafNode = new HuffmanLeafNode<T>(pair.Value, 0);
                            curNode = hfLeafNode;
                        }
                        else
                        {
                            curNode = new HuffmanTreeNode(i);
                        }

                        if (bitCode[i])
                        {
                            prevNode.right = curNode;
                        }
                        else
                        {
                            prevNode.left = curNode;
                        }
                    }
                }
            }
            return hfTree;
        }
        public Dictionary<HFCode, T> GetReadingTable()
        {
            if (readingTable == null)
            {
                readingTable = new Dictionary<HFCode, T>();
                root.left.AddToReadingTable(HFCode.GetStartingCode(false), readingTable);
                root.right.AddToReadingTable(HFCode.GetStartingCode(true), readingTable);
            }
            return readingTable;

        }
        public Dictionary<T, HFCode> GetWritingTable()
        {
            if (writingTable == null)
            {
                writingTable = new Dictionary<T, HFCode>();
                root.left.AddToWritingTable(HFCode.GetStartingCode(false), writingTable);
                root.right.AddToWritingTable(HFCode.GetStartingCode(true), writingTable);
            }
            return writingTable;
        }
    }
    class HuffmanCode<T>
    {
        HuffmanTree<T> tree;

        public HuffmanCode(T[] startingData)
        {
            HuffmanList<T> list = GenerateStartingList(startingData);
            tree = GenerateTree(list);
            tree.PrintTree();
        }

        public Dictionary<HFCode, T> GetReadingTable()
        {
            return tree.GetReadingTable();
        }
        public Dictionary<T, HFCode> GetWritingTable()
        {
            return tree.GetWritingTable();
        }

        private Dictionary<T, int> CalculateDataCount(T[] data)
        {
            Dictionary<T, int> dataCount = new Dictionary<T, int>();
            for (int i = 0; i < data.Length; i++)
            {
                if (dataCount.ContainsKey(data[i]))
                {
                    dataCount[data[i]]++;
                } else
                {
                    dataCount.Add(data[i], 1);
                }
            }
            return dataCount;
        }

        private HuffmanList<T> GenerateStartingList(T[] data)
        {
            Dictionary<T, int> dataCount = CalculateDataCount(data);

            HuffmanList<T> list = new HuffmanList<T>();
            foreach (KeyValuePair<T, int> pair in dataCount)
            {
                list.Add(pair.Key, (double)pair.Value / (double)data.Length);
            }
            return list;
        }

        private HuffmanTree<T> GenerateTree(HuffmanList<T> list)
        {
            IHuffmanNode[] curNodes = new IHuffmanNode[4];
            HuffmanListNode<T> listFirst, listSecond;
            HuffmanListNode<HuffmanTree<T>> treeFirst, treeSecond;
            HuffmanList<HuffmanTree<T>> subTrees = new HuffmanList<HuffmanTree<T>>();
            while (list.Count > 0 || subTrees.Count > 1)
            {
                list.GetTwoLowestFreqNodes(out listFirst, out listSecond);
                subTrees.GetTwoLowestFreqNodes(out treeFirst, out treeSecond);
                curNodes[0] = listFirst;
                curNodes[1] = listSecond;
                if (treeFirst != null)
                {
                    curNodes[2] = treeFirst.info.root;
                    if (treeSecond != null)
                    {
                        curNodes[3] = treeSecond.info.root;
                    } else
                    {
                        curNodes[3] = null;
                    }
                } else
                {
                    curNodes[2] = curNodes[3] = null;
                }


                double lowestFreq = double.MaxValue, secondLowestFreq = double.MaxValue;
                int lowestIndex = 0, secondLowestIndex = 1;

                for (int i = 0; i < 4; ++i)
                {
                    if (curNodes[i] != null)
                    {
                        if (curNodes[i].GetFrequency() <= lowestFreq)
                        {
                            secondLowestFreq = lowestFreq;
                            secondLowestIndex = lowestIndex;
                            lowestFreq = curNodes[i].GetFrequency();
                            lowestIndex = i;
                        } else if (curNodes[i].GetFrequency() <= secondLowestFreq)
                        {
                            secondLowestFreq = curNodes[i].GetFrequency();
                            secondLowestIndex = i;
                        }
                    }
                }
                //If both nodes are from list, create a new subtree
                if (lowestIndex < 2 && secondLowestIndex < 2)
                {
                    HuffmanLeafNode<T> first = new HuffmanLeafNode<T>(listFirst.info, listFirst.frequency);
                    HuffmanLeafNode<T> second = new HuffmanLeafNode<T>(listSecond.info, listSecond.frequency);
                    HuffmanTree<T> newSubTree = new HuffmanTree<T>(first, second);
                    subTrees.Add(newSubTree, newSubTree.GetRootFrequency());
                    list.RemoveNode(listFirst);
                    list.RemoveNode(listSecond);
                } else if (lowestIndex >= 2 && secondLowestIndex >= 2) //Merge trees
                {
                    treeFirst.info.Combine(treeSecond.info.root);
                    subTrees.SetFrequency(treeFirst, treeFirst.info.GetRootFrequency());
                    subTrees.RemoveNode(treeSecond);
                } else
                {
                    HuffmanLeafNode<T> first = new HuffmanLeafNode<T>(listFirst.info, listFirst.frequency);
                    treeFirst.info.Combine(first);
                    subTrees.SetFrequency(treeFirst, treeFirst.info.GetRootFrequency());
                    list.RemoveNode(listFirst);
                }
            }
            return subTrees[0].info;
        }
    }
}