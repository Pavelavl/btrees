namespace BTrees;

public class BStarTree<T> : BPlusTree<T> where T : IComparable<T>
{
    public BStarTree(int degree) : base(degree)
    {
    }

    // Overriding insertion methods to enforce 2/3 fullness
    public override void Insert(T key)
    {
        if (Root == null)
        {
            Root = new BPlusTreeNode<T>(Degree, true);
            Root.Keys.Add(key);
        }
        else
        {
            if (Root.IsFull)
            {
                var s = new BPlusTreeNode<T>(Degree, false);
                s.Children.Add(Root);
                SplitChild(s, 0); // Split root
                InsertNonFull(s, key);
                Root = s;
            }
            else
            {
                InsertNonFull(Root, key);
            }
        }
    }

    // Modified SplitChild to handle overflow and redistribution
    private void SplitChild(BPlusTreeNode<T> parentNode, int index)
    {
        var childNode = parentNode.Children[index];
        var newNode = new BPlusTreeNode<T>(Degree, childNode.IsLeaf);

        // Move half of the keys to the new node
        newNode.Keys.AddRange(childNode.Keys.GetRange(Degree - 1, Degree - 1));
        childNode.Keys.RemoveRange(Degree - 1, Degree - 1);

        // If not a leaf node, move child pointers as well
        if (!childNode.IsLeaf)
        {
            newNode.Children.AddRange(childNode.Children.GetRange(Degree, Degree));
            childNode.Children.RemoveRange(Degree, Degree);
        }

        // Insert new node into parent
        parentNode.Children.Insert(index + 1, newNode);
        parentNode.Keys.Insert(index, childNode.Keys[Degree - 2]);

        // Check for overflow in parent and redistribute if possible
        if (parentNode.Keys.Count == 2 * Degree)
        {
            var childIndex = parentNode.Children.IndexOf(childNode);
            RedistributeKeys(parentNode, childIndex, childIndex + 1);
        }
    }

    private void RedistributeKeys(BPlusTreeNode<T> parent, int leftChildIndex, int rightChildIndex)
    {
        BPlusTreeNode<T> leftChild = parent.Children[leftChildIndex];
        BPlusTreeNode<T> rightChild = parent.Children[rightChildIndex];

        // Calculate the total number of keys in both children
        int totalKeys = leftChild.Keys.Count + rightChild.Keys.Count;
        int midIndex = totalKeys / 2; // Calculate the middle index for redistribution

        if (leftChildIndex == rightChildIndex - 1)
        {
            // Case 1: Redistribute between adjacent siblings

            if (leftChild.Keys.Count > rightChild.Keys.Count)
            {
                // Left child has more keys, move one to the right
                rightChild.Keys.Insert(0, parent.Keys[leftChildIndex]);
                parent.Keys[leftChildIndex] = leftChild.Keys[leftChild.Keys.Count - 1];
                leftChild.Keys.RemoveAt(leftChild.Keys.Count - 1);

                // Adjust child pointers if not leaf nodes
                if (!leftChild.IsLeaf)
                {
                    rightChild.Children.Insert(0, leftChild.Children[leftChild.Children.Count - 1]);
                    leftChild.Children.RemoveAt(leftChild.Children.Count - 1);
                }
            }
            else
            {
                // Right child has more keys (or equal), move one to the left
                leftChild.Keys.Add(parent.Keys[leftChildIndex]);
                parent.Keys[leftChildIndex] = rightChild.Keys[0];
                rightChild.Keys.RemoveAt(0);

                // Adjust child pointers if not leaf nodes 
                if (!rightChild.IsLeaf)
                {
                    leftChild.Children.Add(rightChild.Children[0]);
                    rightChild.Children.RemoveAt(0);
                }
            }
        }
        else
        {
            // Case 2: Redistribute between non-adjacent siblings (merge scenario)

            // Combine keys from both children and the parent key into a temporary list
            List<T> combinedKeys = new List<T>();
            combinedKeys.AddRange(leftChild.Keys);
            combinedKeys.Add(parent.Keys[leftChildIndex]);
            combinedKeys.AddRange(rightChild.Keys);

            // Combine child pointers if not leaf nodes
            List<BPlusTreeNode<T>> combinedChildren = new List<BPlusTreeNode<T>>();
            if (!leftChild.IsLeaf)
            {
                combinedChildren.AddRange(leftChild.Children);
                combinedChildren.AddRange(rightChild.Children);
            }

            // Clear the original children
            leftChild.Keys.Clear();
            rightChild.Keys.Clear();
            if (!leftChild.IsLeaf)
            {
                leftChild.Children.Clear();
                rightChild.Children.Clear();
            }

            // Redistribute keys and child pointers back to the children
            for (int i = 0; i < midIndex; i++)
            {
                leftChild.Keys.Add(combinedKeys[i]);
                if (!leftChild.IsLeaf)
                {
                    leftChild.Children.Add(combinedChildren[i]);
                }
            }

            for (int i = midIndex + 1; i < totalKeys; i++)
            {
                rightChild.Keys.Add(combinedKeys[i]);
                if (!leftChild.IsLeaf)
                {
                    rightChild.Children.Add(combinedChildren[i]);
                }
            }

            // The middle key becomes the separator in the parent 
            parent.Keys[leftChildIndex] = combinedKeys[midIndex];

            // If the parent becomes empty (all keys moved to children), remove it 
            if (parent.Keys.Count == 0)
            {
                // Adjust root if necessary 
                if (parent == Root)
                {
                    Root = leftChild;
                    leftChild.Next = rightChild; // Maintain leaf node connection 
                }
                else
                {
                    // Find the parent's index in its parent and remove
                    BPlusTreeNode<T> grandParent = FindParent(Root, parent);
                    int parentIndex = grandParent.Children.IndexOf(parent);
                    grandParent.Children.RemoveAt(parentIndex);
                    grandParent.Keys.RemoveAt(parentIndex - 1);

                    // Include parentIndex in the RedistributeKeys call
                    RedistributeKeys(grandParent, parentIndex - 1, parentIndex); // Pass grandparent and both child indices
                }
            }
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
                i--;
            }

            // Insert the key
            node.Keys.Insert(i + 1, key);

            // Check if redistribution is needed
            if (node.Keys.Count > 2 * Degree - 1)
            {
                // Find the parent node and child index
                BPlusTreeNode<T> parent = FindParent(Root, node);
                int childIndex = parent.Children.IndexOf(node);

                // Check if redistribution with siblings is possible
                if (childIndex > 0 && parent.Children[childIndex - 1].Keys.Count < 2 * Degree - 1)
                {
                    RedistributeKeys(parent, childIndex - 1, childIndex);
                }
                else if (childIndex < parent.Children.Count - 1 &&
                         parent.Children[childIndex + 1].Keys.Count < 2 * Degree - 1)
                {
                    RedistributeKeys(parent, childIndex, childIndex + 1);
                }
                else
                {
                    // Split the node if redistribution is not possible
                    SplitChild(parent, childIndex);
                }
            }
        }
        else // Internal node
        {
            // Find the child to descend into
            while (i >= 0 && key.CompareTo(node.Keys[i]) < 0)
            {
                i--;
            }

            // Descend into the child
            i++;

            // Check if the child is full and needs splitting
            if (node.Children[i].IsFull)
            {
                SplitChild(node, i);

                // Determine which child to descend into after splitting
                if (key.CompareTo(node.Keys[i]) > 0)
                {
                    i++;
                }
            }

            // Recursively insert into the child
            InsertNonFull(node.Children[i], key);
        }
    }
    
    private BPlusTreeNode<T> FindParent(BPlusTreeNode<T> root, BPlusTreeNode<T> node)
    {
        // If the node is the root, it has no parent
        if (root == node) 
        {
            return null;
        }

        // Iterate through the children of the root
        foreach (BPlusTreeNode<T> child in root.Children)
        {
            // If the child is the target node, return the current root
            if (child == node) 
            {
                return root;
            }

            // Recursively search in the child subtree
            BPlusTreeNode<T> parent = FindParent(child, node);
            if (parent != null) 
            {
                return parent; 
            }
        }

        // Node not found in the subtree
        return null; 
    }
}