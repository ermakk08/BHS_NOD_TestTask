namespace Nodes.Task3;

public class TreeNode<T> where T : IComparable<T>
{
    private TreeNode<T>? _left;
    private TreeNode<T>? _right;

    public TreeNode(params T[] data)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Invalid input array");

        Value = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            Insert(data[i]);
        }
    }

    private TreeNode(T val, TreeNode<T>? lhs = null, TreeNode<T>? rhs = null)
    {
        Value = val;
        _left = lhs;
        _right = rhs;
    }

    public T Value { get; }
    public TreeNode<T>? Left => _left;
    public TreeNode<T>? Right => _right;

    public void Insert(T item)
    {
        TreeNode<T>? current = this;
        TreeNode<T>? prev = null;

        while (current != null)
        {
            prev = current;
            if (item.CompareTo(current.Value) < 0)
            {
                current = current._left;
            }
            else
            {
                current = current._right;
            }
        }

        if (prev == null) throw new NullReferenceException("Cannot insert into null node");

        if (item.CompareTo(prev.Value) < 0)
        {
            prev._left = new TreeNode<T>(item);
        }
        else
        {
            prev._right = new TreeNode<T>(item);
        }
    }
}

public class BSTIterator<T> where T : IComparable<T>
{
    private readonly Stack<TreeNode<T>> _nodes = new();
    private TreeNode<T>? _pointer;

    public BSTIterator(TreeNode<T>? root)
    {
        _pointer = root;
    }

    public T Next()
    {
        while (_pointer != null)
        {
            _nodes.Push(_pointer);
            _pointer = _pointer.Left;
        }
        
        TreeNode<T> result = _nodes.Pop();
        _pointer = result.Right;
        
        return result.Value;
    }

    public bool HasNext()
    {
        return _nodes.Count > 0 || _pointer != null;
    }
}

public static class App
{
    private static void _Main()
    {
        TreeNode<int> root = new TreeNode<int>(7, 3, 15, 9, 20);

        BSTIterator<int> iterator = new BSTIterator<int>(root);
        Console.WriteLine(iterator.Next()); // 3
        Console.WriteLine(iterator.Next()); // 7
        Console.WriteLine(iterator.HasNext()); // True
        Console.WriteLine(iterator.Next()); // 9
        Console.WriteLine(iterator.HasNext()); // True
        Console.WriteLine(iterator.Next()); // 15
        Console.WriteLine(iterator.HasNext()); // True
        Console.WriteLine(iterator.Next()); // 20
        Console.WriteLine(iterator.HasNext()); // False
    }
}