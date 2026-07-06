namespace BottleSharp.Compiler.Binding;

public sealed class BoundLiteralExpression : BoundExpression
{
    public BoundLiteralExpression(object? value, TypeSymbol type)
    {
        Value = value;
        Type = type;
    }

    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;

    public object? Value { get; }

    public override TypeSymbol Type { get; }
}
