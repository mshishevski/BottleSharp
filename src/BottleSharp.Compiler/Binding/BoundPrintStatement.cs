namespace BottleSharp.Compiler.Binding;

public sealed class BoundPrintStatement : BoundStatement
{
    public BoundPrintStatement(BoundExpression expression)
    {
        Expression = expression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.PrintStatement;

    public BoundExpression Expression { get; }
}
