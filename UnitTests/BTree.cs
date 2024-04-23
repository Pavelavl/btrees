namespace UnitTests;

[TestFixture]
public class BTreeTests
{
    private BTree<int> bTree;

    [SetUp]
    public void Setup()
    {
        bTree = new BTree<int>(2); // Degree = 2
    }

    [Test]
    public void Insert_EmptyTree()
    {
        bTree.Insert(10);
        Assert.That(bTree.ToString(), Is.EqualTo("[10]\r\n"));
    }

    [Test]
    public void Insert_MultipleKeys()
    {
        bTree.Insert(10);
        bTree.Insert(20);
        bTree.Insert(5);
        Assert.That(bTree.ToString(), Is.EqualTo("[5, 10, 20]\r\n"));
    }

    [Test]
    public void Insert_CauseSplit()
    {
        bTree = new(3);
        bTree.Insert(10);
        bTree.Insert(20);
        bTree.Insert(30);
        Assert.That(bTree.ToString(), Is.EqualTo("[10, 20, 30]\r\n"));
    }

    [Test]
    public void Search_ExistingKey()
    {
        bTree.Insert(10);
        bTree.Insert(20);
        bTree.Insert(5);

        var foundNode = bTree.Search(20);
        Console.WriteLine(bTree);
        Assert.NotNull(foundNode);
        Assert.That(foundNode.Keys[0], Is.EqualTo(20));
    }

    [Test]
    public void Search_NonExistentKey()
    {
        bTree.Insert(10);
        bTree.Insert(20);
        bTree.Insert(5);

        var foundNode = bTree.Search(30);
        Assert.Null(foundNode);
    }

    [Test]
    public void Delete_LeafNode()
    {
        bTree.Insert(10);
        bTree.Insert(20);
        bTree.Insert(5);

        bTree.Delete(10);
        Assert.That(bTree.ToString(), Is.EqualTo("[5, 20]\r\n"));
    }

    [Test]
    public void Delete_NonExistentKey()
    {
        bTree.Insert(10);
        bTree.Insert(20);
        bTree.Insert(5);

        bTree.Delete(30);
        Assert.That(bTree.ToString(), Is.EqualTo("[5, 10, 20]\r\n"));
    }
}