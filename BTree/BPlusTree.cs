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
            // Поиск листового узла, в который нужно вставить ключ
            BPlusTreeNode<T> leafNode = FindLeafNode(key);
            leafNode.Keys.Add(key);
            leafNode.Keys.Sort(); // Поддерживаем порядок ключей в узле

            // Если узел переполнен, обрабатываем переполнение
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
            
            // Проверяем, что индекс i находится в пределах списка дочерних узлов
            if (i < node.Children.Count)
            {
                return FindLeafNode((BPlusTreeNode<T>)node.Children[i], key);
            }
            else
            {
                // Если ключ больше всех ключей узла, идем к последнему дочернему узлу
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
        // Разделяем переполненный узел
        BPlusTreeNode<T> newNode = new BPlusTreeNode<T>(Degree);
        int mid = node.Keys.Count / 2;
        newNode.Keys.AddRange(node.Keys.GetRange(mid, node.Keys.Count - mid));
        node.Keys.RemoveRange(mid, node.Keys.Count - mid);

        // Обновляем указатели для связного списка листов
        newNode.Next = node.Next;
        node.Next = newNode;

        // Если у узла есть родитель, передаем часть ключей родителю
        if (node.Parent != null)
        {
            int index = node.Parent.Children.IndexOf(node);

            // Вставка разделяющего ключа и указателя на новый узел в родительский узел
            node.Parent.Keys.Insert(index, newNode.Keys[0]);
            node.Parent.Children.Insert(index + 1, newNode);
            newNode.Parent = node.Parent;

            // Если родительский узел переполнился, обрабатываем его переполнение
            if (((BPlusTreeNode<T>)node.Parent).IsFull)
            {
                HandleInternalNodeOverflow((BPlusTreeNode<T>)node.Parent);
            }
        }
        else
        {
            // Если текущий узел был корнем, создаем новый корень
            BPlusTreeNode<T> newRoot = new BPlusTreeNode<T>(Degree, false);
            newRoot.Keys.Add(newNode.Keys[0]);
            newRoot.Children.Add(node);
            newRoot.Children.Add(newNode);

            // Обновляем ссылки для нового корня
            node.Parent = newRoot;
            newNode.Parent = newRoot;
            Root = newRoot;
        }
    }

    private void HandleInternalNodeOverflow(BPlusTreeNode<T> node)
    {
        // Разделяем переполненный внутренний узел
        BPlusTreeNode<T> newNode = new BPlusTreeNode<T>(Degree, false);
        int mid = node.Keys.Count / 2;
        newNode.Keys.AddRange(node.Keys.GetRange(mid + 1, node.Keys.Count - (mid + 1)));
        node.Keys.RemoveRange(mid, node.Keys.Count - mid);

        // Перенос дочерних узлов в новый узел
        newNode.Children.AddRange(node.Children.GetRange(mid + 1, node.Children.Count - (mid + 1)));
        node.Children.RemoveRange(mid + 1, node.Children.Count - (mid + 1));
        
        // Обновляем ссылки для дочерних узлов
        foreach (BPlusTreeNode<T> child in newNode.Children)
        {
            child.Parent = newNode;
        }

        // Если у узла есть родитель, переносим разделяющий ключ к родителю
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
            // Создание нового корня, если узел был корнем
            BPlusTreeNode<T> newRoot = new BPlusTreeNode<T>(Degree, false);
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

        // Поиск ключа и его удаление, если он находится в листовом узле
        BTreeNode<T> node = Root.Search(key);
        if (node != null && node.Keys.Contains(key))
        {
            node.Keys.Remove(key);
        }
    }
}