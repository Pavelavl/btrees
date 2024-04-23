using System.Text;

namespace BTrees;

public class BTreeNode<T> where T : IComparable<T>
{
    public int Degree { get; set; }
    public virtual bool IsLeaf => Children.Count == 0;
    public List<T> Keys { get; set; }
    public List<BTreeNode<T>> Children { get; set; }

    public BTreeNode<T>? Parent { get; set; }

    public virtual bool IsFull => Keys.Count == 2 * Degree - 1 && Keys.Count > MinKeys;

    public virtual int MinKeys => (int)Math.Ceiling(Degree * 1.0 / 2.0);

    public int Depth
    {
        get
        {
            if (IsLeaf)
            {
                return 0;
            }
            else
            {
                return Children[0].Depth + 1;
            }
        }
    }

    public BTreeNode(int degree, bool isLeaf)
    {
        Degree = degree;
        Keys = new List<T>(degree);
        Children = new List<BTreeNode<T>>(degree + 1);
        Parent = null;
    }

    public BTreeNode<T> Search(T key)
    {
        int i = 0;
        while (i < Keys.Count && key.CompareTo(Keys[i]) > 0)
            i++;

        if (i < Keys.Count && key.CompareTo(Keys[i]) == 0)
            return this;

        if (IsLeaf)
            return null;

        return Children[i].Search(key);
    }

    public void InsertNonFull(T key)
    {
        int i = Keys.Count - 1;
        if (IsLeaf)
        {
            while (i >= 0 && Keys[i].CompareTo(key) > 0)
            {
                if (i + 1 < Keys.Count)
                {
                    Keys[i + 1] = Keys[i];
                }

                i--;
            }

            Keys.Insert(i + 1, key);
        }
        else
        {
            while (i >= 0 && Keys[i].CompareTo(key) > 0)
                i--;
            // Check for full node and no right sibling
            if (i == -1 && IsFull && Children.Count == i + 2)
            {
                // 1. Create a new right sibling node
                var rightSibling = new BTreeNode<T>(Degree, false);

                // 2. Move half of the keys and children to the right sibling
                int mid = Keys.Count / 2;
                rightSibling.Keys.AddRange(Keys.GetRange(mid, Keys.Count - mid));
                Keys.RemoveRange(mid, Keys.Count - mid);
                rightSibling.Children.AddRange(Children.GetRange(mid + 1, Children.Count - (mid + 1)));
                Children.RemoveRange(mid + 1, Children.Count - (mid + 1));

                // 3. Promote the middle key (now the maximum) to become a separator key
                // This promotion happens within the current node

                // Shift remaining keys in the current node to make space
                Keys.Insert(mid, Keys[mid - 1]);
                Keys.RemoveAt(mid - 1);

                // Move the last child of the current node to become the first child of the right sibling
                rightSibling.Children.Insert(0, Children[mid]);
                Children.RemoveAt(mid);

                // 4. Add the right sibling to the parent node and adjust keys
                // This step depends on your BTree structure and insertion logic
                if (Parent != null) // Check if Parent exists
                {
                    int parentIndex = Parent.Children.IndexOf(this);
                    Parent.Children.Insert(parentIndex + 1, rightSibling);
                    Parent.Keys.Insert(parentIndex, Keys[mid - 1]);
                    rightSibling.Parent = Parent; // Set Parent for rightSibling
                }
            }
            else
            {
                if (i + 1 < Children.Count && Children[i + 1].IsFull)
                {
                    SplitChild(i + 1, Children[i + 1]);
                    if (Keys[i + 1].CompareTo(key) < 0)
                        i++;
                }

                if (i + 1 < Children.Count)
                {
                    Children[i + 1].InsertNonFull(key);
                }
                else
                {
                    // Handle the case when the child node is out of bounds (last node)
                    // We can create a new right sibling and perform a split here 
                    // since there's no existing sibling to the right.

                    var newRightSibling = new BTreeNode<T>(Degree, false);
                    int mid = Keys.Count / 2;

                    // Move half of the keys and the last child to the new sibling
                    newRightSibling.Keys.AddRange(Keys.GetRange(mid, Keys.Count - mid));
                    Keys.RemoveRange(mid, Keys.Count - mid);
                    newRightSibling.Children.Add(Children[mid]);
                    Children.RemoveAt(mid);

                    // Promote the middle key to become a separator key
                    if (mid < Keys.Count)
                    {
                        Keys.Insert(mid, Keys[mid - 1]);
                    }

                    Keys.RemoveAt(mid - 1);

                    // Insert the key into the appropriate child (either current or new sibling)
                    if (mid < Keys.Count && key.CompareTo(Keys[mid - 1]) > 0)
                    {
                        newRightSibling.InsertNonFull(key);
                    }
                    else
                    {
                        Children[mid - 1].InsertNonFull(key);
                    }
                }
            }
        }
    }

    public void SplitChild(int i, BTreeNode<T> y)
    {
        var z = new BTreeNode<T>(y.Degree, y.IsLeaf);

        // Use correct index for splitting keys
        z.Keys.AddRange(y.Keys.GetRange(Degree, y.Keys.Count - Degree));

        if (!y.IsLeaf)
        {
            // Use correct count for splitting children
            z.Children.AddRange(y.Children.GetRange(Degree + 1, y.Children.Count - Degree - 1));
        }

        // Use correct index and count for removal
        y.Keys.RemoveRange(Degree, y.Keys.Count - Degree);

        if (!y.IsLeaf)
        {
            // Only remove children for non-leaf nodes
            y.Children.RemoveRange(Degree + 1, y.Children.Count - Degree - 1);
        }

        // Remainder of the code remains unchanged
        Children.Insert(i + 1, z);
        Keys.Insert(i, y.Keys[Degree - 1]);
        y.Keys.RemoveAt(Degree - 1);

        z.Parent = this; // Set Parent for the new child node
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        ToStringHelper(this, result, "", "");
        return result.ToString();
    }

    private void ToStringHelper(BTreeNode<T>? node, StringBuilder? result, string prefix, string childrenPrefix)
    {
        if (node == null)
            return;

        result?.Append($"{prefix}[");
        for (int i = 0; i < node.Keys.Count; i++)
        {
            result?.Append(node.Keys[i]);
            if (i < node.Keys.Count - 1)
                result?.Append(", ");
        }

        result?.AppendLine("]");

        if (!node.IsLeaf)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                ToStringHelper(node.Children[i], result,
                    $"{childrenPrefix}{(i == node.Children.Count - 1 ? "└─ " : "├─ ")}",
                    $"{childrenPrefix}{(i == node.Children.Count - 1 ? "   " : "│  ")}");
            }
        }
    }

    public int GetIndexInParent()
    {
        // Assuming the parent node maintains a reference to its children and their count
        if (Parent != null && Parent.Children.Count > 0)
        {
            for (int i = 0; i < Parent.Children.Count; i++)
            {
                if (Parent.Children[i] == this)
                {
                    return i;
                }
            }
        }

        // Return -1 if the node is not part of a parent's children array
        return -1;
    }
}