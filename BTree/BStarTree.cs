using System.Threading.Channels;
using BTrees;

namespace BTree;

public class BStarTree<T> : BPlusTree<T> where T : IComparable<T>
{
    public override int MinKeys => (int)Math.Ceiling(Degree * 2.0 / 3.0);
    
    public BStarTree(int degree) : base(degree)
    {
    }

    public override void Insert(T key)
    {
        if (Root == null)
        {
            Root = new BStarTreeNode<T>(Degree);
            Root.Keys.Add(key);
        }
        else
        {
            // Find the leaf node where the key should be inserted
            BStarTreeNode<T> leafNode = (BStarTreeNode<T>)FindLeafNode(key);

            leafNode.Keys.Add(key);

            // Handle splits if necessary after redistribution
            if (leafNode.IsFull)
                HandleNodeOverflow(leafNode);
        }
    }

    protected void HandleNodeOverflow(BStarTreeNode<T> node)
    {
        // Try to redistribute with left or right sibling before splitting
        BStarTreeNode<T>? leftSibling = node.GetLeftSibling();
        BStarTreeNode<T>? rightSibling = node.GetRightSibling();

        if (leftSibling != null && leftSibling.CanDonate)
        {
            RedistributeKeys(leftSibling, node);
            return;
        }

        if (rightSibling != null && rightSibling.CanDonate)
        {
            RedistributeKeys(node, rightSibling);
            return;
        }

        // Perform 3-way split if redistribution fails
        PerformThreeWaySplit(node);
    }

    private void RedistributeKeys(BStarTreeNode<T> fromNode, BStarTreeNode<T> toNode)
    {
        int transferCount = Math.Min(Math.Min(toNode.Keys.Count - MinKeys, fromNode.Keys.Count),
            (int)Math.Ceiling((toNode.Keys.Count + fromNode.Keys.Count) / 2.0) - toNode.Keys.Count);

        if (transferCount > 0)
        {
            toNode.Keys.InsertRange(0, fromNode.Keys.GetRange(fromNode.Keys.Count - transferCount, transferCount));
            fromNode.Keys.RemoveRange(fromNode.Keys.Count - transferCount, transferCount);
        }

        // Update linked list pointers if leaf nodes
        if (fromNode.IsLeaf && toNode.IsLeaf && fromNode.Children.Count > 0 && toNode.Children.Count > 0)
        {
            BStarTreeNode<T> lastChildOfFrom = (BStarTreeNode<T>)fromNode.Children.Last();
            lastChildOfFrom.Next = (BStarTreeNode<T>)toNode.Children.First();
        }
    }

    private void PerformThreeWaySplit(BStarTreeNode<T> fullNode)
    {
        int numKeys = fullNode.Keys.Count;
        int midLeftKeys = numKeys / 2;

        BStarTreeNode<T> leftNode = new BStarTreeNode<T>(Degree);
        leftNode.Keys.AddRange(fullNode.Keys.GetRange(0, midLeftKeys));
        BStarTreeNode<T> middleNode = new BStarTreeNode<T>(Degree);
        middleNode.Keys.Add(fullNode.Keys[midLeftKeys]);
        BStarTreeNode<T> rightNode = new BStarTreeNode<T>(Degree);
        int numKeysAfterMid = fullNode.Keys.Count - midLeftKeys - 1;
        rightNode.Keys.AddRange(fullNode.Keys.GetRange(midLeftKeys + 1, numKeysAfterMid));

        // Redistribute children if not leaf nodes
        if (!fullNode.IsLeaf)
        {
            leftNode.Children.AddRange(fullNode.Children.GetRange(0, midLeftKeys + 1));
            middleNode.Children.Add(fullNode.Children[midLeftKeys + 1]);
            rightNode.Children.AddRange(fullNode.Children.GetRange(midLeftKeys + 2, numKeysAfterMid + 1));

            // Update child parent pointers
            foreach (BStarTreeNode<T> child in leftNode.Children)
            {
                child.Parent = leftNode;
            }

            foreach (BStarTreeNode<T> child in middleNode.Children)
            {
                child.Parent = middleNode;
            }

            foreach (BStarTreeNode<T> child in rightNode.Children)
            {
                child.Parent = rightNode;
            }
        }

        // Handle linked list pointers for leaf nodes (if necessary)
        if (fullNode.IsLeaf && leftNode.Children.Count > 0 && rightNode.Children.Count > 0)
        {
            BStarTreeNode<T> lastChildOfLeft = (BStarTreeNode<T>)leftNode.Children.Last();
            lastChildOfLeft.Next = (BStarTreeNode<T>)rightNode.Children.First();
        }

        if (fullNode.Parent != null)
        {
            int parentIndex = fullNode.Parent.Children.IndexOf(fullNode);
            if (parentIndex == -1) return;
            int parentKeyIndex = CalculateKeyIndex(fullNode.Parent.Keys, fullNode.Keys[midLeftKeys]); // Calculate key index
            fullNode.Parent.Keys.Insert(parentKeyIndex, fullNode.Keys[midLeftKeys]); // Insert at calculated index
            fullNode.Parent.Children.RemoveAt(parentIndex);
            List<BStarTreeNode<T>> splitChildren = new List<BStarTreeNode<T>>();
            splitChildren.Add(leftNode);
            splitChildren.Add(middleNode);
            splitChildren.Add(rightNode);
            fullNode.Parent.Children.InsertRange(parentIndex, splitChildren);
            leftNode.Parent = fullNode.Parent;
            middleNode.Parent = fullNode.Parent;
            rightNode.Parent = fullNode.Parent;
            // Check for overflow in the parent node and propagate split if needed
            if (fullNode.Parent.IsFull)
            {
                PerformThreeWaySplit((BStarTreeNode<T>)fullNode.Parent);
            }
        }
        else
        {
            Console.WriteLine("new root");
            // If fullNode is the root, create a new root and set it as the parent of the split nodes
            BStarTreeNode<T> newRoot = new BStarTreeNode<T>(Degree, false);
            newRoot.Keys.Add(fullNode.Keys[midLeftKeys]); // Middle key becomes the only key in the new root
            newRoot.Children.Add(leftNode);
            newRoot.Children.Add(middleNode);
            newRoot.Children.Add(rightNode);
            leftNode.Parent = newRoot;
            middleNode.Parent = newRoot;
            rightNode.Parent = newRoot;
            Root = newRoot; // Update the root of the tree
        }
    }

    int CalculateKeyIndex(List<T> parentKeys, T middleKey)
    {
        int low = 0;
        int high = parentKeys.Count - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            int comparisonResult = parentKeys[mid].CompareTo(middleKey);

            if (comparisonResult == 0)
            {
                return mid; // Key already exists at this index
            }
            else if (comparisonResult < 0)
            {
                low = mid + 1; // Middle key is greater than current key
            }
            else
            {
                high = mid - 1; // Middle key is less than current key
            }
        }

        return low; // Insert at the insertion point for the middle key
    }
    
    public override BStarTreeNode<T>? Search(T key)
    {
        if (Root == null)
            return null;

        BStarTreeNode<T> current = (BStarTreeNode<T>)Root;
        while (!current.IsLeaf)
        {
            int index = current.Keys.BinarySearch(key);
            if (index >= 0)
                return current; // Key found in current node

            // Key not found in current node, determine child to descend into
            index = -index - 1;

            // Adjust index if key falls after last key in current node
            if (index == current.Keys.Count && key.CompareTo(current.Keys[index - 1]) > 0)
                index = current.Children.Count - 1;

            current = (BStarTreeNode<T>)current.Children[index];
        }

        // Reached a leaf node, check if the key exists
        if (current.Keys.Contains(key))
            return current;

        return null; // Key not found in the tree
    }
}