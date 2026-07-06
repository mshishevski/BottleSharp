namespace BottleSharp.Compiler.Binding;

public sealed class BoundAssignmentStatement : BoundStatement
{
    public BoundAssignmentStatement(VariableSymbol variable, BoundExpression expression)
    {
        Variable = variable;
        Expression = expression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.AssignmentStatement;

    public VariableSymbol Variable { get; }

    public BoundExpression Expression { get; }
}
