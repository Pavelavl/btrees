using BTree;
using BTrees;

namespace TestApp;

class Program
{
    static BTree<int> currentTree = new BTree<int>(3); // Default to BTree
    static int currentStructure = 1; // 1 - BTree, 2 - BPlusTree, 3 - BStarTree

    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\n\nSelected structure: " + GetStructureName(currentStructure));
            Console.WriteLine($"Current tree: \n------------------\n {currentTree} \n------------------");
            Console.WriteLine("1. Select structure");
            Console.WriteLine("2. Generate tree");
            Console.WriteLine("3. Insert element");
            Console.WriteLine("4. Delete element");
            Console.WriteLine("5. Search element");
            Console.WriteLine("6. Exit");

            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SelectStructure();
                    break;
                case "2":
                    GenerateTree();
                    break;
                case "3":
                    InsertElement();
                    break;
                case "4":
                    DeleteElement();
                    break;
                case "5":
                    SearchElement();
                    break;
                case "6":
                    return; // Exit the application
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static string GetStructureName(int structure)
    {
        switch (structure)
        {
            case 1:
                return "BTree";
            case 2:
                return "BPlusTree";
            case 3:
                return "BStarTree";
            default:
                return "Unknown";
        }
    }
    
    // ... (Previous code - BTree and Program classes) ...

    static void SelectStructure()
    {
        Console.WriteLine("Select structure:");
        Console.WriteLine("1. BTree");
        Console.WriteLine("2. BPlusTree");
        Console.WriteLine("3. BStarTree");
        Console.Write("Enter your choice: ");
        string choice = Console.ReadLine() ?? "";
        currentStructure = int.Parse(choice);

        SelectStructureHelper();
    }
    
    static void SelectStructureHelper()
    {
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
    }

    static void PrintTree()
    {
        Console.WriteLine(currentTree.ToString());
    }

    static void GenerateTree()
    {
        SelectStructureHelper();
        Console.Write("Enter number of elements to generate: ");
        int numElements = int.Parse(Console.ReadLine());
        Random random = new Random();
        for (int i = 0; i < numElements; i++)
        {
            int value = random.Next(0, 100000000);
            currentTree.Insert(value);
        }
        Console.WriteLine("Tree was generated with " + numElements + " elements.");
    }

    static void InsertElement()
    {
        Console.Write("Enter element to insert: ");
        int value = int.Parse(Console.ReadLine());
        currentTree.Insert(value);
        Console.WriteLine("Element inserted.");
    }

    static void DeleteElement()
    {
        Console.Write("Enter element to delete: ");
        int value = int.Parse(Console.ReadLine());
        currentTree.Delete(value);
        Console.WriteLine("Element deleted.");
    }

    static void SearchElement()
    {
        Console.Write("Enter element to search: ");
        int value = int.Parse(Console.ReadLine());
        BTreeNode<int> node = currentTree.Search(value);
        if (node != null)
        {
            Console.WriteLine($"\nElement found: \n------------------\n {node} \n------------------");
        }
        else
        {
            Console.WriteLine("Element not found.");
        }
    }
}