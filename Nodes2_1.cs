using System.Numerics;

namespace Nodes.Task2;

public class TreeNode<T>(T value, TreeNode<T>? left = null, TreeNode<T>? right = null)
{
    public T Value { get; } = value;
    public TreeNode<T>? Left { get; init; } = left;
    public TreeNode<T>? Right { get; init; } = right;
    
    public int GetDepth()
    {
        var depth = 0;
        var queue = new Queue<TreeNode<T>>();
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            var size = queue.Count;
            for (var i = 0; i < size; i++)
            {
                var node = queue.Dequeue();
                if (node.Left != null) queue.Enqueue(node.Left);
                if (node.Right != null) queue.Enqueue(node.Right);
            }

            depth++;
        }

        return depth;
    }
}


public static class App
{
    private static void Main()
    {
        // [3,9,20,null,null,15,7]
        var root = new TreeNode<int>(3)
        {
            Left = new TreeNode<int>(9),
            Right = new TreeNode<int>(20)
            {
                Left = new TreeNode<int>(15),
                Right = new TreeNode<int>(7)
            }
        };
        
        Console.WriteLine(root.GetDepth()); // 3
        
        
        // [1,null,2]
        root = new TreeNode<int>(1)
        {
            Right = new TreeNode<int>(2)
        };
        
        Console.WriteLine(root.GetDepth()); // 2
    }
}