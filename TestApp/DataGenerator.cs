using BTrees;
using BTree;

public class DataGenerator {
    public static BTree<int> Generate(int currentStructure)
    {
        BTree<int>? currentTree = null;
        switch (currentStructure)
        {
            case 1:
                currentTree = new BTree<int>(3);
                break;
            case 2:
                currentTree = new BPlusTree<int>(3);
                break;
            case 3:
                currentTree = new BStarTree<int>(3);
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
        return currentTree ?? new BTree<int>(1);
    }
}