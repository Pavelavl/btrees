namespace BTrees;

// BPlusTreeNode (Extends BTreeNode with linked list and key-only leaf nodes)
public class BPlusTreeNode<T> : BTreeNode<T> where T : IComparable<T>
{
    public BPlusTreeNode<T>? Next { get; set; } // Pointer to the next leaf node

    public BPlusTreeNode(int degree, bool isLeaf = true) : base(degree, isLeaf)
    {
        Degree = degree;
        Keys = new List<T>(degree);
        Children = new List<BTreeNode<T>>(degree + 1);
        Parent = null;
    }
}