namespace BTrees;

public class BTree<T> where T : IComparable<T>
{
    public BTreeNode<T> Root { get; set; }
    public int Degree { get; set; }

    // Минимальное количество ключей в узле, определяемое как половина порядка дерева
    public virtual int MinKeys => (int)Math.Ceiling(Degree * 1.0 / 2.0);

    public BTree(int degree)
    {
        Root = null;
        Degree = degree;
    }

    /// <summary>
    /// Вставка нового ключа в B-дерево
    /// Временная сложность: O(log_t N), где t - порядок дерева, N - количество элементов
    /// </summary>
    public virtual void Insert(T key)
    {
        if (Root == null)
        {
            // Создание нового корневого узла, если дерево пустое
            Root = new BTreeNode<T>(Degree, true);
            Root.Keys.Add(key);
        }
        else
        {
            // Если корневой узел полон, выполняется разделение
            if (Root.IsFull)
            {
                var s = new BTreeNode<T>(Degree, false);
                s.Children.Add(Root);
                s.SplitChild(0, Root); // Разделение корня

                int i = 0;
                if (s.Keys[0].CompareTo(key) < 0)
                    i++;
                s.Children[i].InsertNonFull(key);

                Root = s; // Обновляем корень после разделения
            }
            else
            {
                // Вставка в неполный узел
                Root.InsertNonFull(key);
            }
        }
    }

    /// <summary>
    /// Поиск ключа в B-дереве
    /// Временная сложность: O(t * log_t N), где t - порядок дерева, N - количество элементов
    /// </summary>
    public virtual BTreeNode<T> Search(T key)
    {
        if (Root == null)
            return null;
        return Root.Search(key);
    }

    /// <summary>
    /// Удаление ключа из B-дерева (упрощенная версия)
    /// Временная сложность: O(t * log_t N), так как происходит поиск ключа и удаление его из узла
    /// </summary>
    public virtual void Delete(T key)
    {
        if (Root == null)
            return;

        // Поиск ключа и его удаление, если он находится в листовом узле
        BTreeNode<T> node = Root.Search(key);
        if (node != null && node.IsLeaf && node.Keys.Contains(key))
        {
            node.Keys.Remove(key); // Удаление ключа из листового узла
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