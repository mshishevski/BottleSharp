namespace BottleSharp.Compiler.Binding;

public abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
}
