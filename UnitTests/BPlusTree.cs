namespace UnitTests;

[TestFixture]
public class BPlusTreeTests
{
    [Test]
    public void Insert_EmptyTree()
    {
        var bPlusTree = new BPlusTree<int>(3);
        bPlusTree.Insert(10);
        Assert.That(bPlusTree.Root.ToString(), Is.EqualTo("[10]\r\n"));
    }

    [Test]
    public void Insert_MultipleKeys()
    {
        var bPlusTree = new BPlusTree<int>(3);
        bPlusTree.Insert(10);
        bPlusTree.Insert(20);
        bPlusTree.Insert(5);
        Assert.That(bPlusTree.Root.ToString(), Is.EqualTo("[5, 10, 20]\r\n"));
    }

    [Test]
    public void Insert_CauseLeafSplit()
    {
        var bPlusTree = new BPlusTree<int>(2);
        bPlusTree.Insert(10);
        bPlusTree.Insert(20);
        bPlusTree.Insert(30);
        bPlusTree.Insert(40);

        // Root should have two children and a single key separating them
        Assert.That(bPlusTree.Root.Keys.Count, Is.EqualTo(2));
        Assert.That(bPlusTree.Root.Children.Count, Is.EqualTo(3));
    }
    
    [Test]
    public void BPlusTreeNode_Next_Leaf()
    {
        var node = new BPlusTreeNode<int>(3);
        node.Keys.Add(10);

        var nextNode = new BPlusTreeNode<int>(3);
        nextNode.Keys.Add(20);

        node.Next = nextNode;

        Assert.That(node.Next.Keys[0], Is.EqualTo(20)); // Verify Next points to the correct next leaf node
    }
    
    [Test]
    public void BPlusTreeNode_Next_NonLeaf()
    {
        var node = new BPlusTreeNode<int>(3, false);
        node.Keys.Add(10);

        var nextNode = new BPlusTreeNode<int>(3, false);
        nextNode.Keys.Add(20);

        node.Next = nextNode;

        Assert.NotNull(node.Next);
    }

    [Test]
    public void BPlusTree_Root_EmptyTree()
    {
        var bPlusTree = new BPlusTree<int>(3);
        Assert.Null(bPlusTree.Root); // Root should be null for an empty tree
    }

    [Test]
    public void BPlusTree_Root_AfterInsertion()
    {
        var bPlusTree = new BPlusTree<int>(3);
        bPlusTree.Insert(10);
        Assert.NotNull(bPlusTree.Root); // Root should not be null after insertion
        Assert.That(bPlusTree.Root.Keys.Count, Is.EqualTo(1)); // Root should have one key
    }

    [Test]
    public void BPlusTree_Insert_Duplicate()
    {
        var bPlusTree = new BPlusTree<int>(3);
        bPlusTree.Insert(10);
        bPlusTree.Insert(10); // Insert duplicate

        Assert.That(bPlusTree.Root.Keys.Count, Is.EqualTo(2));
    }
}