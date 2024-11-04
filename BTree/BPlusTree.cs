namespace BTrees;

public class BPlusTree<T> : BTree<T> where T : IComparable<T>
{
    public BPlusTree(int degree) : base(degree)
    {
    }

    /// <summary>
    /// Вставка нового ключа в B+-дерево
    /// Временная сложность: O(log_t N), где t - порядок дерева, N - количество элементов
    /// </summary>
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

    /// <summary>
    /// Поиск листового узла для вставки ключа
    /// Временная сложность: O(log_t N), где t - порядок дерева, так как происходит спуск по дереву
    /// </summary>
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

    /// <summary>
    /// Вспомогательный метод для поиска листового узла по ключу
    /// Временная сложность: O(log_t N), поскольку используется рекурсивный спуск
    /// </summary>
    protected virtual BPlusTreeNode<T> FindLeafNode(T key)
    {
        return FindLeafNode((BPlusTreeNode<T>)Root, key);
    }

    /// <summary>
    /// Обработка переполнения узла в B+-дереве (разделение узлов)
    /// Временная сложность: O(log_t N) в худшем случае, если переполнение распространяется вверх по дереву
    /// </summary>
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

    /// <summary>
    /// Обработка переполнения внутреннего узла
    /// Временная сложность: O(log_t N), так как возможно повторное разделение на каждом уровне дерева
    /// </summary>
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
    
    /// <summary>
    /// Удаление ключа из B+-дерева (упрощенная версия)
    /// Временная сложность: O(log_t N), так как требуется найти ключ и удалить его
    /// </summary>
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