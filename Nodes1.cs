using System.Numerics;

namespace Nodes.Task1;

public abstract class Slot(string title, Type dataType)
{
    public string Title { get; } = title;
    public Type Type { get; } = dataType;
}

public class ExecutionSlot(string header, IExecutableBlock? owner) : Slot(header, typeof(ExecutionSlot))
{
    public IExecutableBlock? ExecutableBlockOwner { get; set; } = owner;
}

public class DataInputSlot(string identifier, Type dataType, DataOutputSlot? src = null)
    : Slot(identifier, dataType)
{
    public DataOutputSlot? Source { get; set; } = src;

    public bool IsSourceValid => Source != null 
                                 && Source.Evaluated 
                                 && (Source.Type == Type || Type == typeof(object));
}

public class NecessaryDataInputSlot(string name, Type t, DataOutputSlot? src = null) : DataInputSlot(name, t, src);

public class DataOutputSlot : Slot
{
    private object? _val;
    public bool Evaluated { get; private set; }
    public object? Value
    {
        get => _val;
        private set
        {
            _val = value;
            Evaluated = value != null;
        }
    }

    public DataOutputSlot(string id, Type t, object? val = null, bool isEval = false) 
        : base(id, t)
    {
        Value = val;
        Evaluated = isEval || val != null;
    }

    public void AssignValue(object? val)
    {
        Value = val;
        Evaluated = true;
    }
}

public interface IBlock
{
    string Title { get; }
}

public interface IExecutableBlock : IBlock
{
    void Execute();
}

public interface IInitialBlock : IBlock
{
    ExecutionSlot NextExecutionSlot { get; }

    void ConnectNext(IExecutableBlock target)
    {
        NextExecutionSlot.ExecutableBlockOwner = target;
    }
}

public interface ITerminalBlock : IBlock
{
    ExecutionSlot PreviousExecutionSlot { get; }

    void ConnectPrevious(IExecutableBlock src)
    {
        PreviousExecutionSlot.ExecutableBlockOwner = src;
    }
}

public interface IInputBlock : IBlock
{
    DataInputSlot[] GetInputSlots();
}

public interface IOutputBlock : IBlock
{
    DataOutputSlot[] GetOutputSlots();
}

public abstract class SimpleOrderedExecutableBlock 
    : IInputBlock, IOutputBlock, ITerminalBlock, IInitialBlock, IExecutableBlock
{
    private readonly ExecutionSlot _prev;
    private readonly ExecutionSlot _next;

    protected SimpleOrderedExecutableBlock(IInitialBlock? prevBlock, ITerminalBlock? nextBlock)
    {
        if (prevBlock == null)
        {
            _prev = new ExecutionSlot("Previous", null);
        }
        else
        {
            prevBlock.ConnectNext(this);
            _prev = new ExecutionSlot("Previous", prevBlock as IExecutableBlock);
        }

        _next = nextBlock == null 
            ? new ExecutionSlot("Next", null) 
            : PrepareNextSlot(nextBlock);
    }

    private ExecutionSlot PrepareNextSlot(ITerminalBlock next)
    {
        next.ConnectPrevious(this);
        return new ExecutionSlot("Next", next as IExecutableBlock);
    }

    public abstract string Title { get; }

    public abstract DataInputSlot[] GetInputSlots();
    public DataInputSlot[] Inputs => GetInputSlots();

    public abstract DataOutputSlot[] GetOutputSlots();
    public DataOutputSlot[] Outputs => GetOutputSlots();

    public ExecutionSlot PreviousExecutionSlot => _prev;
    public ExecutionSlot Previous => _prev;

    public ExecutionSlot NextExecutionSlot => _next;
    public ExecutionSlot Next => _next;

    private bool ValidateInputs() => !Inputs.Any(input => input is NecessaryDataInputSlot n && !n.IsSourceValid);

    protected static void LinkInput(DataInputSlot slot, DataOutputSlot source) => slot.Source = source;

    public void ConnectInput(int idx, IOutputBlock src, int outputIdx) => 
        LinkInput(Inputs[idx], src.GetOutputSlots()[outputIdx]);

    public void ConnectInput(int idx, DataOutputSlot src) => LinkInput(Inputs[idx], src);

    protected abstract void ExecuteCore();

    public virtual IExecutableBlock? ExecuteAndMoveNext()
    {
        if (!ValidateInputs())
            throw new InvalidOperationException("Missing required inputs");
        
        ExecuteCore();
        return Next.ExecutableBlockOwner;
    }

    public void Execute() => ExecuteAndMoveNext()?.Execute();
}

public class Constant<T> : IOutputBlock
{
    public string Title => $"Constant<{typeof(T).Name}>";

    private readonly DataOutputSlot[] _outputs;

    public Constant(T val)
    {
        _outputs = new DataOutputSlot[1];
        _outputs[0] = new DataOutputSlot("Output", typeof(T), val);
    }

    public DataOutputSlot[] GetOutputSlots() => _outputs;
}

public sealed class IntConstant(int number) : Constant<int>(number);

public class Entrypoint : IInitialBlock
{
    public string Title => nameof(Entrypoint);

    private readonly ExecutionSlot _next = new("Next", null);
    public ExecutionSlot NextExecutionSlot => _next;

    public void Execute() => _next.ExecutableBlockOwner?.Execute();
}

public class Adder<T1, T2, TResult>(IInitialBlock prev, ITerminalBlock? next = null)
    : SimpleOrderedExecutableBlock(prev, next)
    where T1 : IAdditionOperators<T1, T2, TResult>
{
    public override string Title => $"Adder<{typeof(T1).Name},{typeof(T2).Name},{typeof(TResult).Name}>";

    private readonly DataInputSlot[] _inputs = new DataInputSlot[]
    {
        new NecessaryDataInputSlot("Term1", typeof(T1)),
        new NecessaryDataInputSlot("Term2", typeof(T2))
    };
    private readonly DataOutputSlot[] _outputs = new DataOutputSlot[] { new DataOutputSlot("Sum", typeof(TResult)) };

    public override DataInputSlot[] GetInputSlots() => _inputs;
    public override DataOutputSlot[] GetOutputSlots() => _outputs;

    protected override void ExecuteCore()
    {
        dynamic term1 = _inputs[0].Source!.Value!;
        dynamic term2 = _inputs[1].Source!.Value!;
        _outputs[0].AssignValue(term1 + term2);
    }
}

public sealed class IntAdder(IInitialBlock prev, ITerminalBlock? next = null) : Adder<int, int, int>(prev, next);

public class Printer(IInitialBlock prev, ITerminalBlock? next = null) : SimpleOrderedExecutableBlock(prev, next)
{
    public override string Title => nameof(Printer);

    private readonly DataInputSlot[] _inputs = new DataInputSlot[] { new NecessaryDataInputSlot("Input", typeof(object)) };
    private readonly DataOutputSlot[] _outputs = new DataOutputSlot[] { new DataOutputSlot("Output", typeof(object)) };

    public override DataInputSlot[] GetInputSlots() => _inputs;
    public override DataOutputSlot[] GetOutputSlots() => _outputs;

    protected override void ExecuteCore()
    {
        var value = _inputs[0].Source!.Value;
        Console.WriteLine(value);
        _outputs[0].AssignValue(value);
    }
}

public sealed class BlockExecutor
{
    private SimpleOrderedExecutableBlock? _current;

    private BlockExecutor(IBlock entry)
    {
        _current = entry switch
        {
            SimpleOrderedExecutableBlock blk => blk,
            IInitialBlock init => init.NextExecutionSlot.ExecutableBlockOwner as SimpleOrderedExecutableBlock,
            _ => null
        };
    }

    private void Proceed()
    {
        if (_current == null) return;
        _current = _current.ExecuteAndMoveNext() as SimpleOrderedExecutableBlock;
    }

    public void ExecuteCompletely()
    {
        while (_current != null)
        {
            Proceed();
        }
    }

    public static BlockExecutor Create(IBlock entry) => new(entry);
}

public static class App
{
    private static void Main()
    {
        var rand = new Random();
        int a = rand.Next(10), b = rand.Next(10);
        Console.WriteLine($"Calculating {a} + {b} = {a + b}");

        var start = new Entrypoint();
        var addBlock = new IntAdder(start);
        var printBlock = new Printer(addBlock);

        var constA = new IntConstant(a);
        var constB = new IntConstant(b);

        addBlock.ConnectInput(0, constA, 0);
        addBlock.ConnectInput(1, constB, 0);
        printBlock.ConnectInput(0, addBlock, 0);

        Console.Write("Recursive: ");
        start.Execute(); // possible stack overflow
        
        Console.Write("Iterative: ");
        BlockExecutor.Create(start).ExecuteCompletely(); // no stack overflow
    }
}