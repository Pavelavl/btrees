namespace BTrees;

public class BPlusTree<T> : BTree<T> where T : IComparable<T>
{
    public BPlusTree(int degree) : base(degree)
    {
    }

    public override void Insert(T key)
    {
        if (Root == null)
        {
            Root = new BPlusTreeNode<T>(Degree);
            Root.Keys.Add(key);
        }
        else
        {
            // Find the leaf node where the key should be inserted
            BPlusTreeNode<T> leafNode = FindLeafNode(key);
            leafNode.Keys.Add(key);
            leafNode.Keys.Sort(); // Maintain sorted order in leaf nodes

            // Handle splits and propagations if necessary
            if (leafNode.IsFull)
            {
                HandleNodeOverflow(leafNode);
            }
        }
    }

    protected virtual BPlusTreeNode<T> FindLeafNode(BPlusTreeNode<T> node, T key)
    {
        if (node.IsLeaf)
        {
            return node;
        }
        else
        {
            int i = 0;
            while (i < node.Keys.Count && key.CompareTo(node.Keys[i]) > 0)
            {
                i++;
            }
        
            if (i < node.Children.Count) // Проверяем, что индекс i не выходит за пределы списка дочерних узлов
            {
                return FindLeafNode((BPlusTreeNode<T>)node.Children[i], key);
            }
            else // Если индекс i вышел за пределы списка, значит ключ больше всех имеющихся ключей, поэтому переходим к последнему дочернему узлу
            {
                return FindLeafNode((BPlusTreeNode<T>)node.Children[node.Children.Count - 1], key);
            }
        }
    }

    protected virtual BPlusTreeNode<T> FindLeafNode(T key)
    {
        return FindLeafNode((BPlusTreeNode<T>)Root, key);
    }

    protected void HandleNodeOverflow(BPlusTreeNode<T> node)
    {
        // Split the leaf node
        BPlusTreeNode<T> newNode = new BPlusTreeNode<T>(Degree);
        int mid = node.Keys.Count / 2;
        newNode.Keys.AddRange(node.Keys.GetRange(mid, node.Keys.Count - mid));
        node.Keys.RemoveRange(mid, node.Keys.Count - mid);

        // Update linked list pointers
        newNode.Next = node.Next;
        node.Next = newNode;

        // Propagate the split upwards if necessary
        if (node.Parent != null)
        {
            // Find the index of the current node in its parent
            int index = node.Parent.Children.IndexOf(node);

            // Ensure parent has enough space for the new key
            if (index == node.Parent.Keys.Count)
            {
                // Decrement index to insert before the last element
                index--;
            }

            // Inserting within the list, insert key and child at correct indices
            node.Parent.Keys.Insert(index, newNode.Keys[0]);
            node.Parent.Children.Insert(index + 1, newNode);
            newNode.Parent = node.Parent;

            if (((BPlusTreeNode<T>)node.Parent).IsFull)
            {
                HandleInternalNodeOverflow((BPlusTreeNode<T>)node.Parent);
            }
        }
        else
        {
            BPlusTreeNode<T> newRoot = new BPlusTreeNode<T>(Degree, false);
            newRoot.Keys.Add(newNode.Keys[0]); // Add the first key of the new node as the separator key
            newRoot.Children.Add(node);
            newRoot.Children.Add(newNode);

            // Update parent pointers for the split nodes
            node.Parent = newRoot;
            newNode.Parent = newRoot;

            // Set the new root of the B+ Tree
            Root = newRoot;
        }
    }

    private void HandleInternalNodeOverflow(BPlusTreeNode<T> node)
    {
        // Split the internal node similar to BTree
        BPlusTreeNode<T> newNode = new BPlusTreeNode<T>(Degree, false);
        int mid = node.Keys.Count / 2;
        newNode.Keys.AddRange(node.Keys.GetRange(mid + 1, node.Keys.Count - (mid + 1)));
        node.Keys.RemoveRange(mid, node.Keys.Count - mid);

        // Move child nodes to the new node
        newNode.Children.AddRange(node.Children.GetRange(mid + 1, node.Children.Count - (mid + 1)));
        node.Children.RemoveRange(mid + 1, node.Children.Count - (mid + 1));
        
        // Upd mid after deleting
        mid = node.Keys.Count / 2;

        // Update parent pointers for moved child nodes
        foreach (BPlusTreeNode<T> child in newNode.Children)
        {
            child.Parent = newNode;
        }

        // Update linked list pointers for leaf nodes if necessary
        if (node.IsLeaf) 
        {
            BPlusTreeNode<T> lastChildOfNode = (BPlusTreeNode<T>)node.Children.Last();
            BPlusTreeNode<T> firstChildOfNewNode = (BPlusTreeNode<T>)newNode.Children.First();
            lastChildOfNode.Next = firstChildOfNewNode; 
        }

        // Propagate the split upwards 
        if (node.Parent != null)
        {
            int index = node.Parent.Children.IndexOf(node);
            node.Parent.Keys.Insert(index, node.Keys[mid]);
            node.Parent.Children.Insert(index + 1, newNode);
            newNode.Parent = node.Parent;

            if (node.Parent.IsFull)
            {
                HandleInternalNodeOverflow((BPlusTreeNode<T>)node.Parent);
            }
        }
        else
        {
            // Create a new root node
            BPlusTreeNode<T> newRoot = new BPlusTreeNode<T>(Degree, false);
            if (mid >= 0 && mid < node.Keys.Count) 
                newRoot.Keys.Add(node.Keys[mid]);
            newRoot.Children.Add(node);
            newRoot.Children.Add(newNode);
            node.Parent = newRoot;
            newNode.Parent = newRoot;
            Root = newRoot;
        }
    }
    
    public override void Delete(T key)
    {
        if (Root == null)
            return;

        // Find the key and remove it if found in a leaf
        BTreeNode<T> node = Root.Search(key);
        if (node != null && node.Keys.Contains(key))
        {
            node.Keys.Remove(key);
        }
    }
}