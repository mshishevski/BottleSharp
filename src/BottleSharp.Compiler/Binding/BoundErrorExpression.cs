namespace BottleSharp.Compiler.Binding;

public sealed class BoundErrorExpression : BoundExpression
{
    public static BoundErrorExpression Instance { get; } = new();

    private BoundErrorExpression()
    {
    }

    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;

    public override TypeSymbol Type => TypeSymbol.Error;
}
