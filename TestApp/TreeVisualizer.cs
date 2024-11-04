using System;
using System.Collections.Generic;
using BTrees;

public class TreeVisualizer<T> where T : IComparable<T>
{
    public void DisplayTree(BTree<T> tree)
    {
        if (tree.Root == null)
        {
            Console.WriteLine("Tree is empty.");
            return;
        }

        Console.WriteLine("Tree Structure:");
        DisplayNode(tree.Root, "", true);
    }

    private void DisplayNode(BTreeNode<T> node, string indent, bool isLast)
    {
        Console.Write(indent);
        if (isLast)
        {
            Console.Write("└─");
            indent += "  ";
        }
        else
        {
            Console.Write("├─");
            indent += "| ";
        }

        // Display the keys of the current node
        Console.WriteLine(string.Join(", ", node.Keys));

        // Recursively display each child node
        for (int i = 0; i < node.Children.Count; i++)
        {
            DisplayNode(node.Children[i], indent, i == node.Children.Count - 1);
        }
    }
}
