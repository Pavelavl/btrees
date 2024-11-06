using System.Threading.Channels;
using BTrees;

namespace BTree;

public class BStarTree<T> : BPlusTree<T> where T : IComparable<T>
{
    // Минимальное количество ключей в узле для B*-дерева составляет 2/3 от степени
    public override int MinKeys => (int)Math.Ceiling(Degree * 2.0 / 3.0);
    
    public BStarTree(int degree) : base(degree)
    {
    }

    public override void Insert(T key)
    {
        if (Root == null)
        {
            Root = new BStarTreeNode<T>(Degree);
            Root.Keys.Add(key);
        }
        else
        {
            // Поиск листового узла, в который будет вставлен ключ
            BStarTreeNode<T> leafNode = (BStarTreeNode<T>)FindLeafNode(key);
            leafNode.Keys.Add(key);

            // Если узел переполнен, обрабатываем переполнение через перераспределение или разделение
            if (leafNode.IsFull)
                HandleNodeOverflow(leafNode);
        }
    }

    protected void HandleNodeOverflow(BStarTreeNode<T> node)
    {
        // Пытаемся перераспределить ключи с соседними узлами
        BStarTreeNode<T>? leftSibling = node.GetLeftSibling();
        BStarTreeNode<T>? rightSibling = node.GetRightSibling();

        if (leftSibling != null && leftSibling.CanDonate)
        {
            RedistributeKeys(leftSibling, node);
            return;
        }

        if (rightSibling != null && rightSibling.CanDonate)
        {
            RedistributeKeys(node, rightSibling);
            return;
        }

        // Если перераспределение невозможно, выполняем 3-стороннее разделение
        PerformThreeWaySplit(node);
    }

    private void RedistributeKeys(BStarTreeNode<T> fromNode, BStarTreeNode<T> toNode)
    {
        int transferCount = Math.Min(Math.Min(toNode.Keys.Count - MinKeys, fromNode.Keys.Count),
            (int)Math.Ceiling((toNode.Keys.Count + fromNode.Keys.Count) / 2.0) - toNode.Keys.Count);

        if (transferCount > 0)
        {
            toNode.Keys.InsertRange(0, fromNode.Keys.GetRange(fromNode.Keys.Count - transferCount, transferCount));
            fromNode.Keys.RemoveRange(fromNode.Keys.Count - transferCount, transferCount);
        }

        // Обновление указателей, если перераспределяем листовые узлы
        if (fromNode.IsLeaf && toNode.IsLeaf && fromNode.Children.Count > 0 && toNode.Children.Count > 0)
        {
            BStarTreeNode<T> lastChildOfFrom = (BStarTreeNode<T>)fromNode.Children.Last();
            lastChildOfFrom.Next = (BStarTreeNode<T>)toNode.Children.First();
        }
    }

    private void PerformThreeWaySplit(BStarTreeNode<T> fullNode)
    {
        int numKeys = fullNode.Keys.Count;
        int midLeftKeys = numKeys / 2;

        // Создаем левый, средний и правый узлы после разделения
        BStarTreeNode<T> leftNode = new BStarTreeNode<T>(Degree);
        leftNode.Keys.AddRange(fullNode.Keys.GetRange(0, midLeftKeys));
        BStarTreeNode<T> middleNode = new BStarTreeNode<T>(Degree);
        middleNode.Keys.Add(fullNode.Keys[midLeftKeys]);
        BStarTreeNode<T> rightNode = new BStarTreeNode<T>(Degree);
        int numKeysAfterMid = fullNode.Keys.Count - midLeftKeys - 1;
        rightNode.Keys.AddRange(fullNode.Keys.GetRange(midLeftKeys + 1, numKeysAfterMid));

        // Перераспределяем дочерние узлы, если узел не является листом
        if (!fullNode.IsLeaf)
        {
            leftNode.Children.AddRange(fullNode.Children.GetRange(0, midLeftKeys + 1));
            middleNode.Children.Add(fullNode.Children[midLeftKeys + 1]);
            rightNode.Children.AddRange(fullNode.Children.GetRange(midLeftKeys + 2, numKeysAfterMid + 1));

            // Обновляем ссылки на родителей у дочерних узлов
            foreach (BStarTreeNode<T> child in leftNode.Children)
            {
                child.Parent = leftNode;
            }

            foreach (BStarTreeNode<T> child in middleNode.Children)
            {
                child.Parent = middleNode;
            }

            foreach (BStarTreeNode<T> child in rightNode.Children)
            {
                child.Parent = rightNode;
            }
        }

        // Обработка листовых указателей для связного списка узлов (если требуется)
        if (fullNode.IsLeaf && leftNode.Children.Count > 0 && rightNode.Children.Count > 0)
        {
            BStarTreeNode<T> lastChildOfLeft = (BStarTreeNode<T>)leftNode.Children.Last();
            lastChildOfLeft.Next = (BStarTreeNode<T>)rightNode.Children.First();
        }

        // Если узел имеет родителя, добавляем узлы после разделения к родителю
        if (fullNode.Parent != null)
        {
            int parentIndex = fullNode.Parent.Children.IndexOf(fullNode);
            if (parentIndex == -1) return;
            int parentKeyIndex = CalculateKeyIndex(fullNode.Parent.Keys, fullNode.Keys[midLeftKeys]);
            fullNode.Parent.Keys.Insert(parentKeyIndex, fullNode.Keys[midLeftKeys]);
            fullNode.Parent.Children.RemoveAt(parentIndex);
            fullNode.Parent.Children.InsertRange(parentIndex, new List<BStarTreeNode<T>> { leftNode, middleNode, rightNode });
            leftNode.Parent = fullNode.Parent;
            middleNode.Parent = fullNode.Parent;
            rightNode.Parent = fullNode.Parent;

            // Если родительский узел переполнился, обрабатываем его переполнение
            if (fullNode.Parent.IsFull)
            {
                PerformThreeWaySplit((BStarTreeNode<T>)fullNode.Parent);
            }
        }
        else
        {
            // Создаем новый корень, если разделяем корень
            BStarTreeNode<T> newRoot = new BStarTreeNode<T>(Degree, false);
            newRoot.Keys.Add(fullNode.Keys[midLeftKeys]);
            newRoot.Children.Add(leftNode);
            newRoot.Children.Add(middleNode);
            newRoot.Children.Add(rightNode);
            leftNode.Parent = newRoot;
            middleNode.Parent = newRoot;
            rightNode.Parent = newRoot;
            Root = newRoot;
        }
    }

    int CalculateKeyIndex(List<T> parentKeys, T middleKey)
    {
        int low = 0;
        int high = parentKeys.Count - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            int comparisonResult = parentKeys[mid].CompareTo(middleKey);

            if (comparisonResult == 0)
            {
                return mid; // Ключ уже существует
            }
            else if (comparisonResult < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return low; // Возвращаем индекс для вставки
    }

    public override BStarTreeNode<T>? Search(T key)
    {
        if (Root == null)
            return null;

        BStarTreeNode<T> current = (BStarTreeNode<T>)Root;
        while (!current.IsLeaf)
        {
            int index = current.Keys.BinarySearch(key);
            if (index >= 0)
                return current; // Ключ найден в текущем узле

            index = -index - 1;

            // Определяем нужный дочерний узел
            if (index == current.Keys.Count && key.CompareTo(current.Keys[index - 1]) > 0)
                index = current.Children.Count - 1;

            current = (BStarTreeNode<T>)current.Children[index];
        }

        // Проверяем наличие ключа в листовом узле
        if (current.Keys.Contains(key))
            return current;

        return null; // Ключ не найден
    }
}