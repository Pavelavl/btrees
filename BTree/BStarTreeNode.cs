using BTrees;

namespace BTree;

// BStarTreeNode (Extends BPlusTreeNode with redistribution logic)
public class BStarTreeNode<T> : BPlusTreeNode<T> where T : IComparable<T>
{
    public BStarTreeNode(int degree, bool isLeaf = true) : base(degree, isLeaf)
    {
        Degree = degree;
        Keys = new List<T>(degree);
        Children = new List<BTreeNode<T>>(degree + 1);
        Parent = null;
    }

    public bool CanRedistribute
    {
        get
        {
            if (IsLeaf)
            {
                return false; // Leaf nodes cannot redistribute
            }

            BStarTreeNode<T> leftSibling = GetLeftSibling();
            BStarTreeNode<T> rightSibling = GetRightSibling();

            return (leftSibling != null && leftSibling.CanDonate) ||
                   (rightSibling != null && rightSibling.CanDonate);
        }
    }

    public bool CanDonate => Keys.Count > MinKeys && Keys.Count + Keys.Count - 1 <= Degree * 2;
    
    public override int MinKeys => (int)Math.Ceiling(Degree * 2.0 / 3.0);

    public BStarTreeNode<T>? GetLeftSibling()
    {
        if (Parent == null)
        {
            return null;
        }

        int index = Parent.Children.IndexOf(this);
        if (index > 0)
        {
            return (BStarTreeNode<T>)Parent.Children[index - 1];
        }

        return null;
    }

    public BStarTreeNode<T>? GetRightSibling()
    {
        if (Parent == null)
        {
            return null;
        }

        int index = Parent.Children.IndexOf(this);
        if (index < Parent.Children.Count - 1)
        {
            return (BStarTreeNode<T>)Parent.Children[index + 1];
        }

        return null;
    }
    
    public BStarTreeNode<T>? GetRedistributionSibling()
    {
        BStarTreeNode<T> parent = (BStarTreeNode<T>)Parent;
        if (parent == null)
        {
            return null;
        }

        int index = parent.Children.IndexOf(this);

        // Look left for a donor sibling
        for (int i = index - 1; i >= 0; i--)
        {
            BStarTreeNode<T> potentialSibling = (BStarTreeNode<T>)parent.Children[i];
            if (potentialSibling.CanDonate)
            {
                return potentialSibling;
            }
        }

        // Look right for a donor sibling (if no left sibling found)
        for (int i = index + 1; i < parent.Children.Count; i++)
        {
            BStarTreeNode<T> potentialSibling = (BStarTreeNode<T>)parent.Children[i];
            if (potentialSibling.CanDonate)
            {
                return potentialSibling;
            }
        }

        return null; // No sibling found for redistribution
    }
}