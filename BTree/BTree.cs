namespace BTrees;

public class BTree<T> where T : IComparable<T>
{
    public BTreeNode<T> Root { get; set; }
    public int Degree { get; set; }

    public virtual int MinKeys => (int)Math.Ceiling(Degree * 1.0 / 2.0);

    public BTree(int degree)
    {
        Root = null;
        Degree = degree;
    }

    public virtual void Insert(T key)
    {
        if (Root == null)
        {
            Root = new BTreeNode<T>(Degree, true);
            Root.Keys.Add(key);
        }
        else
        {
            if (Root.IsFull)
            {
                var s = new BTreeNode<T>(Degree, false);
                s.Children.Add(Root);
                s.SplitChild(0, Root);

                int i = 0;
                if (s.Keys[0].CompareTo(key) < 0)
                    i++;
                s.Children[i].InsertNonFull(key);

                Root = s;
            }
            else
            {
                Root.InsertNonFull(key);
            }
        }
    }

    public virtual BTreeNode<T> Search(T key)
    {
        if (Root == null)
            return null;
        return Root.Search(key);
    }

    // Simplified deletion (only from leaves and without merging/redistribution)
    public virtual void Delete(T key)
    {
        if (Root == null)
            return;

        // Find the key and remove it if found in a leaf
        BTreeNode<T> node = Root.Search(key);
        if (node != null && node.IsLeaf && node.Keys.Contains(key))
        {
            node.Keys.Remove(key);
        }
    }
    
    public override string ToString()
    {
        if (Root == null)
        {
            return "Empty Tree";
        }
        return Root.ToString();
    }
}