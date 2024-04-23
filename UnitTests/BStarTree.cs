using BTree;

namespace UnitTests;

[TestFixture]
public class BStarTree
{
    [Test]
    public void BStarTreeNode_IsFull_LowKeys()
    {
        var node = new BStarTreeNode<int>(3);
        node.Keys.Add(10);
        Assert.IsFalse(node.IsFull); // Not full with less than 2/3 minimum occupancy
    }

    [Test]
    public void BStarTreeNode_IsFull_MidKeys()
    {
        var node = new BStarTreeNode<int>(3);
        node.Keys.Add(10);
        node.Keys.Add(20);
        node.Keys.Add(30);
        node.Keys.Add(40);
        node.Keys.Add(50); // Overflow
        Assert.IsTrue(node.IsFull); // Full with exactly 2/3 occupancy
    }

    [Test]
    public void BStarTreeNode_IsFull_HighKeys()
    {
        var node = new BStarTreeNode<int>(3);
        node.Keys.Add(10);
        node.Keys.Add(20);
        node.Keys.Add(30);
        Assert.IsFalse(node.IsFull); // Full with keys within allowed range
    }

    [Test]
    public void BStarTree_Insert_RedistributeRight()
    {
        var bStarTree = new BStarTree<int>(3);
        bStarTree.Insert(10);
        bStarTree.Insert(20);
        bStarTree.Insert(30);
        bStarTree.Insert(15);
        bStarTree.Insert(40); // Overflow, redistribute to the right

        // Verify key distribution after redistribution
        Console.WriteLine(bStarTree.ToString());
        Assert.IsTrue(((BStarTreeNode<int>)bStarTree.Root.Children[0]).Keys.SequenceEqual(new[] { 10, 20 }));
        Assert.IsTrue(((BStarTreeNode<int>)bStarTree.Root.Children[1]).Keys.SequenceEqual(new[] { 30 }));
    }

    [Test]
    public void BStarTree_Insert_RedistributeLeft()
    {
        var bStarTree = new BStarTree<int>(3);
        bStarTree.Insert(40);
        bStarTree.Insert(30);
        bStarTree.Insert(20);
        bStarTree.Insert(15);
        bStarTree.Insert(10); // Overflow, redistribute to the left

        // Verify key distribution after redistribution
        Console.WriteLine(bStarTree.ToString());
        Assert.IsTrue(((BStarTreeNode<int>)bStarTree.Root.Children[1]).Keys.Count == 1 &&
                      ((BStarTreeNode<int>)bStarTree.Root.Children[1]).Keys[0] == 20);
    }

    [Test]
    public void BStarTree_Insert_CauseThreeWaySplit()
    {
        var bStarTree = new BStarTree<int>(3);
        bStarTree.Insert(10);
        bStarTree.Insert(20);
        bStarTree.Insert(30);
        bStarTree.Insert(40);
        bStarTree.Insert(50); // Overflow, causing a 3-way split

        // Verify the new root structure and child key distribution
        Assert.That(bStarTree.Root.Keys.Count, Is.EqualTo(1));
        Assert.That(bStarTree.Root.Children.Count, Is.EqualTo(3));
    }
}