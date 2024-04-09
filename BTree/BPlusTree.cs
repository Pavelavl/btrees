namespace BTrees;

public class BPlusTree<T> : BTree<T> where T : IComparable<T>
{
    public BPlusTreeNode<T> Root { get; set; }

    public BPlusTree(int degree) : base(degree)
    {
        Root = new BPlusTreeNode<T>(degree, true);
    }

    // Insertion (Ensures keys only in leaf nodes)
    public override void Insert(T key)
    {
        BPlusTreeNode<T> r = Root;
        if (r.IsFull)
        {
            BPlusTreeNode<T> s = new BPlusTreeNode<T>(Degree, false);
            if (s.Children == null) s.Children = new List<BPlusTreeNode<T>>();
            Root = s;
            s.Children.Add(r);
            SplitChild(s, 0);
            InsertNonFull(s, key);
        }
        else
        {
            InsertNonFull(r, key);
        }
    }

    // Search (Finds the leaf node containing the key)
    public BPlusTreeNode<T> Search(T key)
    {
        return Search(Root, key);
    }

    private BPlusTreeNode<T> Search(BPlusTreeNode<T> node, T key)
    {
        int i = 0;
        while (i < node.Keys.Count && key.CompareTo(node.Keys[i]) > 0)
        {
            i++;
        }

        if (i < node.Keys.Count && key.CompareTo(node.Keys[i]) == 0)
        {
            return node; // Key found in this node
        }

        if (node.IsLeaf)
        {
            return null; // Key not found
        }

        return Search(node.Children[i], key);
    }

    // Deletion (Simplified - removes from leaf node only)
    public void Delete(T key)
    {
        BPlusTreeNode<T> node = Search(key);
        if (node != null)
        {
            node.Keys.Remove(key);
        }
    }

    // Helper methods
    private void SplitChild(BPlusTreeNode<T> parentNode, int index)
    {
        var childNode = parentNode.Children[index];
        var newNode = new BPlusTreeNode<T>(Degree, childNode.IsLeaf);

        // Move the second half of keys and children from childNode to newNode
        for (int j = 0; j < Degree - 1; j++)
        {
            newNode.Keys.Add(childNode.Keys[j + Degree]);
        }

        if (!childNode.IsLeaf)
        {
            for (int j = 0; j < Degree; j++)
            {
                newNode.Children.Add(childNode.Children[j + Degree]);
            }
        }

        // Insert newNode as a child of parentNode
        parentNode.Children.Insert(index + 1, newNode);

        // A key from childNode will move up to parentNode
        parentNode.Keys.Insert(index, childNode.Keys[Degree - 1]);

        // Adjust linked list pointers for leaf nodes
        if (childNode.IsLeaf)
        {
            newNode.Next = childNode.Next;
            childNode.Next = newNode;
        }
    }

    private void InsertNonFull(BPlusTreeNode<T> node, T key)
    {
        int i = node.Keys.Count - 1;
        if (node.IsLeaf)
        {
            // Find the correct position for the new key
            while (i >= 0 && key.CompareTo(node.Keys[i]) < 0)
            {
                node.Keys.Insert(i + 1, key);
                i--;
            }

            // Insert the new key (handle empty leaf node case)
            if (i < 0)
            {
                node.Keys.Insert(0, key);
            }
            else
            {
                node.Keys.Insert(i + 1, key);
            }

            // Update Next pointer for new insertions
            if (node.Next != null && i == node.Keys.Count - 2)
            {
                node.Next.Next = node;
            }
        }
        else
        {
            // Find the child where the key should be inserted
            while (i >= 0 && key.CompareTo(node.Keys[i]) < 0)
            {
                i--;
            }

            // Check if the child is full and needs splitting
            if (node.Children[i + 1].IsFull)
            {
                SplitChild(node, i + 1);
                if (key.CompareTo(node.Keys[i + 1]) > 0)
                {
                    i++;
                }
            }

            // Insert the key in the appropriate child
            InsertNonFull(node.Children[i + 1], key);
        }
    }
}