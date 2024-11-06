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

    public BTreeNode<T>? Search(T key)
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
                i--;

            Keys.Insert(i + 1, key);
        }
        else
        {
            while (i >= 0 && Keys[i].CompareTo(key) > 0)
                i--;

            if (Children[i + 1].IsFull)
            {
                SplitChild(i + 1, Children[i + 1]);

                if (Keys[i + 1].CompareTo(key) < 0)
                    i++;
            }

            Children[i + 1].InsertNonFull(key);
        }
    }

    public void SplitChild(int i, BTreeNode<T> y)
    {
        var z = new BTreeNode<T>(y.Degree, y.IsLeaf);

        // Переносим правую часть ключей и дочерних узлов
        z.Keys.AddRange(y.Keys.GetRange(Degree, y.Keys.Count - Degree));
        if (!y.IsLeaf)
            z.Children.AddRange(y.Children.GetRange(Degree, y.Children.Count - Degree));

        y.Keys.RemoveRange(Degree, y.Keys.Count - Degree);
        if (!y.IsLeaf)
            y.Children.RemoveRange(Degree, y.Children.Count - Degree);

        Children.Insert(i + 1, z);
        Keys.Insert(i, y.Keys[Degree - 1]);
        y.Keys.RemoveAt(Degree - 1);

        z.Parent = this; // Устанавливаем ссылку на родителя для нового узла
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