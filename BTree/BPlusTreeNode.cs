namespace BTrees;

// BPlusTreeNode (Extends BTreeNode with linked list and key-only leaf nodes)
public class BPlusTreeNode<T> : BTreeNode<T> where T : IComparable<T>
{
    public new List<BPlusTreeNode<T>> Children { get; set; }
    public BPlusTreeNode<T> Next { get; set; } // Pointer to the next leaf node
    
    public bool IsLeaf
    {
        get => base.IsLeaf;
        set
        {
            base.IsLeaf = value;
            Next = null; // Set Next to null for non-leaf nodes
        }
    }

    public BPlusTreeNode(int degree, bool isLeaf) : base(degree, isLeaf)
    {
        Next = null;
    }
}